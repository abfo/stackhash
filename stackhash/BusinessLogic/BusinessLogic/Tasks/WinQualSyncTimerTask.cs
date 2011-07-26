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
    public class WinQualSyncTimerTaskParameters : TaskParameters
    {
        string m_UserName;
        string m_Password;
        WinQualContext m_WinQualContext;
        ScheduleCollection m_ScheduleCollection;
        IWinQualServices m_WinQualServicesObject;

        public string UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }

        public WinQualContext ThisWinQualContext
        {
            get { return m_WinQualContext; }
            set { m_WinQualContext = value; }
        }

        public IWinQualServices WinQualServicesObject
        {
            get { return m_WinQualServicesObject; }
            set { m_WinQualServicesObject = value; }
        }

        

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public ScheduleCollection ScheduleCollection
        {
            get { return m_ScheduleCollection; }
            set { m_ScheduleCollection = value; }
        }
    }


    public class WinQualSyncTimerTask : Task
    {
        private WinQualSyncTimerTaskParameters m_TaskParameters;
        private Timer m_Timer;
        private long m_TimeInMilliseconds;
        
        private static int s_AbortEventIndex = 0;
        private static int s_TimeExpiredEventIndex = 1;
        private EventWaitHandle[] m_Events;
        private int m_TimerExpiredCount;
        private bool m_TimerRunning = false;

        private bool m_IsRetryRequest;


        public long TimeInMilliseconds
        {
            get { return m_TimeInMilliseconds; }
        }

        public long TimerExpiredCount
        {
            get { return m_TimerExpiredCount; }
        }

        public bool IsRetryRequest
        {
            get { return m_IsRetryRequest; }
        }

        /// <summary>
        /// Timer expired callback. Signal the main thread to do some work.
        /// </summary>
        /// <param name="state">Not currently used.</param>
        private void timerExpired(Object state)
        {
            m_TimerRunning = false;
            m_Events[s_TimeExpiredEventIndex].Set();
            m_TimerExpiredCount++;
        }

        /// <summary>
        /// This task determines the next scheduled time for the WinQualSyncTask to run and then
        /// waits for that time to come. When it does, the WinQualSyncTask is started and this task
        /// then waits for the next scheduled time.
        /// </summary>
        /// <param name="taskParameters">Schedule information.</param>
        public WinQualSyncTimerTask(WinQualSyncTimerTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.WinQualSynchronizeTimerTask)
        {
            m_TaskParameters = taskParameters;
            m_Events = new EventWaitHandle[] { new ManualResetEvent(false), new AutoResetEvent(false) };
        }


        /// <summary>
        /// The timer has expired so it is time to start the WinQualSync task.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        void runWinQualSyncTask()
        {
            try
            {
                m_TaskParameters.ControllerContext.RunSynchronizeTask(null, false, false, false, null, true, m_IsRetryRequest); // Don't wait for completion.
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Information, "Unable to start scheduled Win Qual Sync service", ex);
            }
        }


        private void stopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                m_TimerRunning = false;
                m_Timer.Dispose();
                m_Timer = null;
            }
        }


        public void Reschedule(int timeToExpiryInSeconds)
        {
            Monitor.Enter(this);

            try
            {
                m_IsRetryRequest = true;

                // Stop the timer and restart.
                stopTimer();

                m_TimeInMilliseconds = (long)timeToExpiryInSeconds * 1000;

                // Start a single shot timer to wake up the next time the sync object is ready.
                if (m_Timer == null)
                    m_Timer = new Timer(timerExpired, null, m_TimeInMilliseconds, Timeout.Infinite);
                else
                    m_Timer.Change(m_TimeInMilliseconds, Timeout.Infinite);
                m_TimerRunning = true;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }



        void startTimer()
        {
            Monitor.Enter(this);

            try
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
                {
                    m_Timer = new Timer(timerExpired, null, m_TimeInMilliseconds, Timeout.Infinite);
                    m_TimerRunning = true;
                }
                else
                {
                    // If the timer is already running then it must have been due to a resync arriving due to 
                    // a failed sync task. This is a race condition because the task can complete before the 
                    // this function is called to set the next scheduled time. A sync failure sets a shorter
                    // schedule for the next sync. The consequence of overwriting the reschedule time is that 
                    // a reschedule on failure gets overwritten with a scheduled sync time and thus the 
                    // shorter resync doesn't occur. e.g. if a resync on fail is set to 15 mins and the normal 
                    // sync is daily then if the task fails before the normal scheduled sync is started then
                    // then resync will set 15 mins correctly but the scheduled sync will overwrite it with
                    // 24 hours later.
                    if (!m_TimerRunning)
                    {
                        m_Timer.Change(m_TimeInMilliseconds, Timeout.Infinite);
                        m_TimerRunning = true;
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
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

                // Now wait for an abort or timer event.
                int eventIndex;
                while ((eventIndex = WaitHandle.WaitAny(m_Events)) != s_AbortEventIndex)
                {
                    if (eventIndex == s_TimeExpiredEventIndex)
                    {
                        // The timer may go off fractionally before time. In this case if you start the timer again too soon
                        // it may set a time of only a couple of milliseconds into the future (as the real next time hasn't quite
                        // arrived because the system timer went off a bit early).
                        Thread.Sleep(1000);

                        runWinQualSyncTask();

                        m_IsRetryRequest = false;

                        // There is a race condition where the task that is started above completes with a failure and reschedules
                        // for say 15 minutes time before this call is reached. In this case the startTimer will overwrite the 
                        // desired 15 minutes with the next scheduled time. The startTimer function allows for this and only 
                        // sets the timer if it hasn't already been set up.
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
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Sync timer task stopped", ex);

                LastException = ex;
            }
            finally
            {
                stopTimer();
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
