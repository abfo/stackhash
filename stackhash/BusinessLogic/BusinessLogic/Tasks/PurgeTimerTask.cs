using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;


namespace StackHashTasks
{
    public class PurgeTimerTaskParameters : TaskParameters
    {
        StackHashPurgeOptionsCollection m_PurgeOptionsCollection;
        ScheduleCollection m_ScheduleCollection;

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashPurgeOptionsCollection PurgeOptionsCollection
        {
            get { return m_PurgeOptionsCollection; }
            set { m_PurgeOptionsCollection = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public ScheduleCollection ScheduleCollection
        {
            get { return m_ScheduleCollection; }
            set { m_ScheduleCollection = value; }
        }
    }


    public class PurgeTimerTask : Task
    {
        private PurgeTimerTaskParameters m_TaskParameters;
        private Timer m_Timer;
        private long m_TimeInMilliseconds;

        private static int s_AbortEventIndex = 0;
        private static int s_TimeExpiredEventIndex = 1;
        private EventWaitHandle[] m_Events;
        private int m_TimerExpiredCount;


        public long TimeInMilliseconds
        {
            get { return m_TimeInMilliseconds; }
        }

        public long TimerExpiredCount
        {
            get { return m_TimerExpiredCount; }
        }

        /// <summary>
        /// Timer expired callback. Signal the main thread to do some work.
        /// </summary>
        /// <param name="state">Not currently used.</param>
        private void timerExpired(Object state)
        {
            m_Events[s_TimeExpiredEventIndex].Set();
            m_TimerExpiredCount++;
        }

        /// <summary>
        /// This task determines the next scheduled time for the PurgeTask to run and then
        /// waits for that time to come. When it does, the PurgeTask is started and this task
        /// then waits for the next scheduled time.
        /// </summary>
        /// <param name="taskParameters">Schedule information.</param>
        public PurgeTimerTask(PurgeTimerTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.PurgeTimerTask)
        {
            m_TaskParameters = taskParameters;
            m_Events = new EventWaitHandle[] { new ManualResetEvent(false), new AutoResetEvent(false) };
        }


        /// <summary>
        /// The timer has expired so it is time to start the Purge task.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        void runPurgeTask()
        {
            try
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Purge Timer Task: Attempting to start scheduled Purge task");
                m_TaskParameters.ControllerContext.RunPurgeTask(m_TaskParameters.ClientData); // Don't wait for completion.
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Information, "Unable to start scheduled Purge task", ex);
            }
        }


        void startTimer()
        {
            // Work out how many milliseconds to the next scheduled event.
            // Record the current time so the subtraction later does not go negative. 
            DateTime now = DateTime.Now.ToUniversalTime();

            DateTime nextScheduledTime = m_TaskParameters.ScheduleCollection.NextScheduledTime.ToUniversalTime();

            m_TimeInMilliseconds = (nextScheduledTime.Ticks - now.Ticks) / 10000;

            if (m_TimeInMilliseconds < 0)
                m_TimeInMilliseconds = 0;

            if (m_TimeInMilliseconds == 0)
                m_TimeInMilliseconds = 1;

            // Start a single shot timer to wake up the next time the sync object is ready.
            if (m_Timer == null)
                m_Timer = new Timer(timerExpired, null, m_TimeInMilliseconds, Timeout.Infinite);
            else
                m_Timer.Change(m_TimeInMilliseconds, Timeout.Infinite);
        }


        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);

                startTimer();

                // Now wait for the an abort or timer event.
                int eventIndex;
                while ((eventIndex = WaitHandle.WaitAny(m_Events)) != s_AbortEventIndex)
                {
                    if (eventIndex == s_TimeExpiredEventIndex)
                    {
                        // The timer may go off fractionally before time. In this case if you start the timer again too soon
                        // it may set a time of only a couple of milliseconds into the future (as the real next time hasn't quite
                        // arrived because the system timer went off a bit early).
                        Thread.Sleep(1000);

                        runPurgeTask();

                        startTimer();
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected event ID " + eventIndex.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (!CurrentTaskState.AbortRequested)
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Purge timer task stopped", ex);
                LastException = ex;
            }
            finally
            {
                if (m_Timer != null)
                {
                    m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_Timer.Dispose();
                    m_Timer = null;
                }
                SetTaskCompleted(m_TaskParameters.ErrorIndex);
            }
        }

        /// <summary>
        /// Abort the current task.
        /// </summary>
        public override void StopExternal()
        {
            if (m_Timer != null)
            {
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                m_Timer.Dispose();
                m_Timer = null;
            }
            m_Events[s_AbortEventIndex].Set();
            base.StopExternal();
        }
    }
}
