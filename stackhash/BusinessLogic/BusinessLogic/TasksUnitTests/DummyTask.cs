using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashTasks;
using System.Threading;

using StackHashBusinessObjects;

namespace TasksUnitTests
{

    public class DummyTaskParameters : TaskParameters
    {
        private int m_TimeToWait;
        private bool m_SetLastException;
        private Exception m_LastExceptionToSet;
        private bool m_WaitForAbort;
        private bool m_WaitForSuspend;

        public int TimeToWait
        {
            get { return m_TimeToWait; }
            set { m_TimeToWait = value; }
        }


        public bool SetLastException
        {
            get { return m_SetLastException; }
            set { m_SetLastException = value; }
        }

        public Exception LastExceptionToSet
        {
            get { return m_LastExceptionToSet; }
            set { m_LastExceptionToSet = value; }
        }
        public bool WaitForAbort
        {
            get { return m_WaitForAbort; }
            set { m_WaitForAbort = value; }
        }
        public bool WaitForSuspend
        {
            get { return m_WaitForSuspend; }
            set { m_WaitForSuspend = value; }
        }
    }

    public class DummyTask : Task
    {
        private bool m_EntryPointExecuted;
        private int m_ExecutionCount;

        private DummyTaskParameters m_Params;

        public bool EntryPointExecuted
        {
            get { return m_EntryPointExecuted; }
            set { m_EntryPointExecuted = value; }
        }
        public int ExecutionCount
        {
            get { return m_ExecutionCount; }
            set { m_ExecutionCount = value; }
        }

        public DummyTask(TaskParameters taskParams)
            : base(taskParams, StackHashTaskType.DummyTask)
        {
            m_Params = taskParams as DummyTaskParameters;
        }

        public override void EntryPoint()
        {
            SetTaskStarted(null);

            try
            {
                m_EntryPointExecuted = true;
                m_ExecutionCount++;

                if (m_Params.SetLastException)
                    this.LastException = m_Params.LastExceptionToSet;

                if (m_Params.WaitForAbort)
                {
                    while (!WritableTaskState.AbortRequested)
                    {
                        Thread.Sleep(500);
                    }
                    this.WritableTaskState.Aborted = true;
                }
                else if (m_Params.WaitForSuspend)
                {
                    this.CheckSuspend();
                    if (this.WritableTaskState.AbortRequested)
                        this.WritableTaskState.Aborted = true;
                }
                else
                {
                    Thread.Sleep(m_Params.TimeToWait);
                }
            }
            finally
            {
                SetTaskCompleted(null);
            }
        }

        /// <summary>
        /// Forces a call down to the protected StopInternal function.
        /// </summary>
        public void ForceCallToStopInternal()
        {
            StopInternal();
        }
    }
}
