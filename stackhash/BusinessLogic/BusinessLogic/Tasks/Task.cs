using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using StackHashUtilities;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashErrorIndex;

namespace StackHashTasks
{
    public class StateChangedEventArgs : EventArgs
    {
        TaskState m_TaskState;
        StackHashClientData m_ClientData;
        Task m_Task;

        public TaskState TaskState
        {
            get { return m_TaskState; }
        }

        public Task ChangedTask
        {
            get { return m_Task; }
        }

        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
        }

        public StateChangedEventArgs(StackHashClientData clientData, TaskState taskState, Task task)
        {
            // Clone the state.
            m_TaskState = new TaskState(taskState);
            m_ClientData = clientData;
            m_Task = task;
        }
    }

    /// <summary>
    /// TaskState indicates a snapshot of the state of a task and the demands 
    /// made upon it. e.g. if a request to abort, suspend or resume has been made.
    /// Also indicates if the task is in an Aborted state and whether the test is started 
    /// or completed.
    /// </summary>
    public class TaskState
    {
        bool m_Suspended;
        bool m_SuspendRequested;
        bool m_InternalAbortRequested;
        bool m_ExternalAbortRequested;
        bool m_Aborted;
        bool m_TaskStarted;
        bool m_TaskCompleted;

        public bool Suspended
        {
            get { return m_Suspended; }
            set { m_Suspended = value; }
        }
        public bool SuspendRequested
        {
            get { return m_SuspendRequested; }
            set { m_SuspendRequested = value; }
        }
        public bool InternalAbortRequested
        {
            get { return m_InternalAbortRequested; }
            set { m_InternalAbortRequested = value; }
        }
        public bool ExternalAbortRequested
        {
            get { return m_ExternalAbortRequested; }
            set { m_ExternalAbortRequested = value; }
        }
        public bool AbortRequested
        {
            get { return m_ExternalAbortRequested || m_InternalAbortRequested; }
        }
        public bool Aborted
        {
            get { return m_Aborted; }
            set { m_Aborted = value; }
        }
        public bool TaskStarted
        {
            get { return m_TaskStarted; }
            set { m_TaskStarted = value; }
        }
        public bool TaskCompleted
        {
            get { return m_TaskCompleted; }
            set { m_TaskCompleted = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TaskState()
        {
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="state"></param>
        public TaskState(TaskState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            m_Suspended = state.Suspended;
            m_SuspendRequested = state.SuspendRequested;
            m_InternalAbortRequested = state.InternalAbortRequested;
            m_ExternalAbortRequested = state.ExternalAbortRequested;
            m_Aborted = state.Aborted;
            m_TaskStarted = state.TaskStarted;
            m_TaskCompleted = state.TaskCompleted;
         }
    }


    /// <summary>
    /// Task is the base class for all other StackHash tasks. It provides 
    /// standard features to run a task on a new thread. 
    /// It is an abstract class so one must derive from the class before using it.
    /// Tasks can be aborted, paused and resumed.
    /// </summary>
    public abstract class Task : IDisposable
    {
        private StackHashTaskType m_TaskType;
        private TaskParameters m_TaskParameters;    // Execution environment for the task.
        private System.Exception m_LastException;   // The last exception reported by the task.
        private Thread m_Thread;                    // The thread associated with the task (if there is one).

        private TaskState m_TaskState = new TaskState(); // Current state abort/pause/resume state of task.

        // Events used to trigger action within the task.
        private const int s_AbortEvent = 0;
        private const int s_PauseEvent = 1;
        private const int s_ResumeEvent = 2;
        private const int s_NumberOfEvents = 3;
        private ManualResetEvent[] m_Events = new ManualResetEvent[s_NumberOfEvents];

        private ManualResetEvent m_TaskStartedEvent = new ManualResetEvent(false);
        private ManualResetEvent m_TaskCompletedEvent = new ManualResetEvent(false);
        private ManualResetEvent m_TaskStartedNotificationEvent = new ManualResetEvent(false);

        /// <summary>
        /// Event handler trigger when the Abort, Pause, Resume and Start, End state 
        /// of the task changes.
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Name of the task.
        /// </summary>
        public string Name
        {
            get { return m_TaskParameters.Name; }
        }

        /// <summary>
        /// Defines if the task can be run in parallel with other tasks.
        /// </summary>
        public TaskParameters InitializationData
        {
            get { return m_TaskParameters; }
        }

        /// <summary>
        /// Current pause, resume, abort, start and stopped state of the task.
        /// </summary>
	    public TaskState CurrentTaskState
	    {
            get { return new TaskState(m_TaskState); }
	    }

        /// <summary>
        /// Current pause, resume, abort, start and stopped state of the task.
        /// </summary>
        public StackHashClientData ClientData
        {
            get { return m_TaskParameters.ClientData; }
        }

        
        /// <summary>
        /// The last exception reported by the task during execution.
        /// </summary>
        public System.Exception LastException
	    {
            get { return m_LastException; }
            set { m_LastException = value; }
	    }

        /// <summary>
        /// Event indicating that the task has started.
        /// </summary>
        public ManualResetEvent TaskStartedEvent
        {
            get { return m_TaskStartedEvent; }
        }

        /// <summary>
        /// Event indicating that a notification of the task starting has been sent.
        /// This is necessary to ensure that TaskStarted and TaskCompleted events don't arrive out of order.
        /// </summary>
        public ManualResetEvent TaskStartedNotificationEvent
        {
            get { return m_TaskStartedNotificationEvent; }
        }

        /// <summary>
        /// Event indicating that the task has completed.
        /// </summary>
        public ManualResetEvent TaskCompletedEvent
        {
            get { return m_TaskCompletedEvent; }
        }

        /// <summary>
        /// Task state can only be set by the derived class.
        /// </summary>
        protected TaskState WritableTaskState
        {
            get { return m_TaskState; }
        }

        /// <summary>
        /// Task type could be WinQualSync, Analyze etc...
        /// </summary>
        public StackHashTaskType TaskType
        {
            get { return m_TaskType; }
        }

        /// <summary>
        /// Task parameters.
        /// </summary>
        public TaskParameters TaskParameters
        {
            get { return m_TaskParameters; }
        }

        
        /// <summary>
        /// Constructs a Task object. This is a base class 
        /// Creates the thread associated with this task (if a separate on is to be used).
        /// Initialises the Signalling events.
        /// </summary>
        /// <param name="taskParameters">Basic task parameters indicating how task is to be run</param>
        /// <param name="type">Type of task that this is.</param>
         
        protected Task(TaskParameters taskParameters, StackHashTaskType type)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");

            m_TaskParameters = taskParameters;
            m_TaskType = type;

            // Default the name if not provided.
            if (m_TaskParameters.Name == null)
                m_TaskParameters.Name = "StackHashTask";

            if (m_TaskParameters.UseSeparateThread)
	        {
		        m_Thread = new Thread(new ThreadStart(this.EntryPoint));
                m_Thread.Name = m_TaskParameters.Name;
                m_Thread.IsBackground = m_TaskParameters.IsBackgroundTask;
	        }

            m_Events[s_AbortEvent] = new ManualResetEvent(false);
	        m_Events[s_PauseEvent] = new ManualResetEvent(false);
	        m_Events[s_ResumeEvent] = new ManualResetEvent(false);
        }


        /// <summary>
        /// Called by the task to indicate that the task state has changed.
        /// </summary>
        protected virtual void OnTaskStateChange()
        {
            StateChangedEventArgs args = new StateChangedEventArgs(m_TaskParameters.ClientData, m_TaskState, this);

            EventHandler<StateChangedEventArgs> handler = StateChanged;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Starts the task. If the task is set up to run on a separate thread then that thread 
        /// is started. This will cause execution of that thread to start at EntryPoint.
        /// If no separate thread is to be used then this function will call EntryPoint directly
        /// and will not return until the task is complete.
        /// </summary>

        public virtual void Run()
        {
            if (m_TaskParameters.UseSeparateThread)
	        {
		        m_Thread.Start();
	        }
	        else
	        {
		        // Make a note of the thread under which we are being invoked.
		        // This will be used by the Join function.
		        m_Thread = Thread.CurrentThread;
        		
		        m_TaskState.TaskStarted = true;

		        // Call the thread entry directly.
		        EntryPoint(); 

		        m_TaskState. TaskCompleted = true;

		        // We are "out" of this thread now. This will force a call to IsAlive to be false.
		        m_Thread = null;
	        }        
        }


        /// <summary>
        /// Called by the Task itself to abort itself. External aborts should call 
        /// StopExternal().
        /// Signals to the thread that it should stop what it is doing. 
        /// The thread will stop as soon as it is able to. 
        /// </summary>

        protected virtual void StopInternal()
        {
            stop(true);
        }


        /// <summary>
        /// Called by owner of the task to abort the task. 
        /// If a task wishes to abort itself, it should call StopInternal().
        /// Signals to the thread that it should stop what it is doing. 
        /// The thread will stop as soon as it is able to. 
        /// </summary>

        public virtual void StopExternal()
        {
            stop(false);
        }


        /// <summary>
        /// Signals to the thread that it should stop what it is doing. 
        /// Records the abort in the last exception and sets the abort event.
        /// </summary>
        /// <param name="isInternalAbort">true - internal, false external</param>

        private void stop(bool isInternalAbort)
        {
            if (isInternalAbort)
                m_TaskState.InternalAbortRequested = true;
            else
                m_TaskState.ExternalAbortRequested = true;

	        // Need to do this as a signalled event as well as bool in case 
	        // the thread is suspended.
            string exceptionString = string.Format(CultureInfo.InvariantCulture,
                "{0} Thread: {1} Abort Internal: {2}", DateTime.Now.ToUniversalTime(), m_TaskParameters.Name, isInternalAbort);
	        m_LastException = new OperationCanceledException(exceptionString, m_LastException);

	        OnTaskStateChange();

	        m_Events[s_AbortEvent].Set();
        }

        /// <summary>
        /// Signals to the thread that it should suspend what it is doing. 
        /// The task should periodically check this event with CheckSuspend().
        /// </summary>

        public void Suspend()
        {
	        m_TaskState.SuspendRequested = true;

	        OnTaskStateChange();
        }


        /// <summary>
        /// Signals to the thread that it should resume what it was doing.
        /// Triggers the event on which the task MAY be waiting.
        /// </summary>

        public void Resume()
        {
            m_TaskState.SuspendRequested = false;
            OnTaskStateChange();

            // Resume if neither local or remote suspend requested.
	        // It is possible for both ends to request a suspend at the same time
	        // so only trigger a resume event if neither end has a suspend outstanding.
	        if (m_TaskState.Suspended)
		        m_Events[s_ResumeEvent].Set();
        }


        /// <summary>
        /// Called by the task to determine if it should suspend or not.
        /// If so, then the method will not return until the parent has 
        /// called Resume().
        /// </summary>

        public void CheckSuspend()
        {
	        try
	        {
		        // Don't suspend if aborting.
                if (m_TaskState.SuspendRequested && !m_TaskState.AbortRequested)
		        {
                    m_TaskState.Suspended = true;
			        OnTaskStateChange();

			        WaitHandle.WaitAny(m_Events);

			        m_TaskState.Suspended = false;
			        OnTaskStateChange();
		        }
	        }
	        finally
	        {
		        m_TaskState.Suspended = false;
	        }
        }


        /// <summary>
        /// Waits for the task thread to complete or for the specified timeout.
        /// This call waits on the created thread or the thread which was
        /// used to start the task.
        /// </summary>
        /// <param name="timeout">Time to wait in milliseconds.</param>

        public bool Join(int timeout)
        {
            if (m_Thread != null)
            {
	            if (m_TaskParameters.UseSeparateThread)
	            {
		            // Cannot call Join on a thread that hasn't been started.
		            if ((m_Thread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
			            return m_Thread.Join(timeout);
		            else
			            return true;
	            }
	            else
	            {
		            int timeIntervalBetweenChecks = 2000;
		            long timeSoFar = 0;

		            if (!m_TaskState.TaskStarted)
			            return true;

		            // Not a separate thread - wait for the task completion event.
		            while ((timeSoFar < timeout) || (timeout == Timeout.Infinite))
		            {
			            if (m_TaskState.TaskCompleted)
				            return true;
			            else
				            Thread.Sleep(timeIntervalBetweenChecks);

			            timeSoFar += timeIntervalBetweenChecks;
		            }
		            return false;
	            }
            }
            else
            {
	            return true;
            }
        }

        /// <summary>
        /// Called to set the state of the task to started.
        /// Should only be called once.
        /// </summary>
        public void SetTaskStarted(IErrorIndex index)
        {
            m_TaskState.TaskStarted = true;
            OnTaskStateChange();
            m_TaskStartedEvent.Set();
            UpdateTaskStartedStatistics(index);

            // Wait for the task manager to send the TaskStarted notification message. 
            // The task manager sends TaskStarted after starting the Task. If the Task thread completes before the
            // task manager thread gets another lookin then it will send the TaskCompleted message and then the 
            // task manager thread will send the TaskStarted message (out of order).
            m_TaskStartedNotificationEvent.WaitOne(); 
        }

        /// <summary>
        /// Called to set the state of the task to started.
        /// Should only be called once.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void SetTaskCompleted(IErrorIndex index)
        {
            try
            {
                m_TaskState.TaskCompleted = true;
                OnTaskStateChange();
                m_TaskCompletedEvent.Set();
                UpdateTaskCompletedStatistics(index);
            }
            catch (System.Exception ex)
            {
                // Log and ignore.
                DiagnosticsHelper.LogException(StackHashUtilities.DiagSeverity.ComponentFatal, "Error updating task status in index", ex);
            }
        }

        /// <summary>
        /// Updates the statistics related to the task in the specified index.
        /// The stats relate to tasks that have been run on that index.
        /// </summary>
        /// <param name="index">Index to update.</param>
        public void UpdateTaskStartedStatistics(IErrorIndex index)
        {
            // Set the task status in the index - this will default if not found.
            // Only do this for an active context - otherwise a WinQualLogon task might trigger an update to 
            // the stats before the error index is created.
            if ((index != null) && (index.Status == ErrorIndexStatus.Created))
            {
                StackHashTaskStatus taskStatus = index.GetTaskStatistics(this.TaskType);
                taskStatus.RunCount++;
                taskStatus.TaskState = StackHashTaskState.Running;
                taskStatus.LastStartedTimeUtc = DateTime.Now.ToUniversalTime();
                taskStatus.LastException = null;
                taskStatus.ServiceErrorCode = StackHashServiceErrorCode.NoError;
                index.SetTaskStatistics(taskStatus);
            }
        }


        /// <summary>
        /// Updates the statistics related to the task in the specified index.
        /// The stats relate to tasks that have been run on that index.
        /// </summary>
        /// <param name="index">Index to update.</param>
        public void UpdateTaskCompletedStatistics(IErrorIndex index)
        {
            if ((index != null) && (index.Status == ErrorIndexStatus.Created))
            {
                StackHashTaskStatus taskStatus = index.GetTaskStatistics(this.TaskType);

                taskStatus.TaskState = StackHashTaskState.Completed;

                taskStatus.ServiceErrorCode = StackHashException.GetServiceErrorCode(this.LastException);
                if (this.LastException != null)
                {
                    taskStatus.FailedCount++;
                    taskStatus.LastFailedRunTimeUtc = taskStatus.LastStartedTimeUtc;
                    taskStatus.LastException = this.LastException.ToString();
                }
                else
                {
                    taskStatus.SuccessCount++;
                    taskStatus.LastSuccessfulRunTimeUtc = taskStatus.LastStartedTimeUtc;
                }

                taskStatus.LastDurationInSeconds = (int)(DateTime.Now.ToUniversalTime() - taskStatus.LastStartedTimeUtc).TotalSeconds;
                taskStatus.TaskState = StackHashTaskState.Completed;
                index.SetTaskStatistics(taskStatus);
            }
        }

        /// <summary>
        /// Main thread entry point - inherited task must provide its own entry point.
        /// </summary>

        public virtual void EntryPoint()
        {
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of the task managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">True - </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (ManualResetEvent resetEvent in m_Events)
                {
                    if (resetEvent != null)
                        resetEvent.Close();
                }

                m_TaskStartedEvent.Close();
                m_TaskStartedNotificationEvent.Close();
                m_TaskCompletedEvent.Close();
            }
        }

        /// <summary>
        /// Disposes of task managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
