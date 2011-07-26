using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    /// <summary>
    /// Parameters passed to the TaskStarted delegate.
    /// </summary>
    public class TaskStartedEventArgs : EventArgs
    {
        private Task m_StartedTask;

        /// <summary>
        /// Identifies the task that has started.
        /// </summary>
        public Task StartedTask
        {
            get { return m_StartedTask; }
            set { m_StartedTask = value; }
        }
    }


    /// <summary>
    /// Parameters passed to the TaskCompleted event delegate.
    /// </summary>
    public class TaskCompletedEventArgs : EventArgs
    {
        private Task m_CompletedTask;

        /// <summary>
        /// Identifies the task that has completed.
        /// </summary>
        public Task CompletedTask
        {
            get { return m_CompletedTask; }
            set { m_CompletedTask = value; }
        }
    }


    /// <summary>
    /// Data associated with each task on the queue and concurrent task list. 
    /// </summary>
    public class TaskData
    {
        Task m_Task;

        /// <summary>
        /// The task to which this data refers.
        /// </summary>
        public Task ThisTask
        {
            get { return m_Task; }
            set { m_Task = value; }
        }

        public TaskData(Task task)
        {
            m_Task = task;
        }

        public StackHashTaskStatus TaskStatus
        {
            get
            {
                StackHashTaskStatus taskStatus = new StackHashTaskStatus();

                taskStatus.ServiceErrorCode = StackHashException.GetServiceErrorCode(m_Task.LastException);

                if (m_Task.LastException != null)
                {
                    taskStatus.LastException = m_Task.LastException.ToString();
                    taskStatus.TaskState = StackHashTaskState.Completed;
                }
                else if (m_Task.CurrentTaskState.TaskCompleted)
                {
                    taskStatus.TaskState = StackHashTaskState.Completed;
                }
                else if (m_Task.CurrentTaskState.TaskStarted)
                {
                    // Not completed so must be running.
                    taskStatus.TaskState = StackHashTaskState.Running;
                }

                // Get the type of the task.
                taskStatus.TaskType = m_Task.TaskType;

                return taskStatus;
            }
        }
    }


    /// <summary>
    /// A TaskManager executes operates a queue of tasks and also a concurrent task list.
    /// </summary>
    public class TaskManager : IDisposable
    {
        private Queue<TaskData> m_TaskQueue;        // The main task queue.
        private List<TaskData> m_ConcurrentTasks;   // Tasks that execute in parallel.
        Thread m_TaskRunnerThread;                  // The thread on which the task runner executes tasks in the queue.
        private TaskData m_CurrentTask;                         // The currently running task from the queue.
        
        private const int s_AbortEventIndex = 0;                // Signal to abort all tasks.
        private const int s_TaskQueuedEventIndex = 1;           // Signal when a task is queued.
        private  const int s_TaskCompletedEventIndex = 2;       // Signal when a task is complete.
        private AutoResetEvent[] m_Events;                      // All signals.
        

        /// <summary>
        /// Event handler triggered when a task starts.
        /// </summary>
        public event EventHandler<TaskStartedEventArgs> TaskStarted;


        /// <summary>
        /// Event handler triggered when a task completes.
        /// </summary>
        public event EventHandler<TaskCompletedEventArgs> TaskCompleted;


        /// <summary>
        /// Initialises the task queue, concurrent task list and events.
        /// </summary>
        /// <param name="name">Name of the task manager used as thread name.</param>
        public TaskManager(String name)
        {
            // Initialise the signals.
            m_Events = new AutoResetEvent[] { new AutoResetEvent(false), new AutoResetEvent(false), new AutoResetEvent(false) };
            
            // Create the task queue.
            m_TaskQueue = new Queue<TaskData>();

            // Create the concurrent task list - tasks that will be run in parallel.
            m_ConcurrentTasks = new List<TaskData>();

            // Create and start the task runner thread.
            // This thread manages executing tasks on the task queue.
            m_TaskRunnerThread = new Thread(new ThreadStart(TaskRunnerStart));
            m_TaskRunnerThread.Name = "StackHashTaskRunner_" + name;
            m_TaskRunnerThread.Start();
        }


        /// <summary>
        /// Determines if the client is permitted to abort a task of the specified type.
        /// </summary>
        /// <param name="taskType">Task type to check.</param>
        /// <returns>True - client can abort the task. False - client cannot abort the task.</returns>
        public bool CanTaskBeAbortedByClient(StackHashTaskType taskType)
        {
            if ((taskType == StackHashTaskType.BugReportTask) ||
                (taskType == StackHashTaskType.DebugScriptTask) ||
                (taskType == StackHashTaskType.DownloadCabTask) ||
                (taskType == StackHashTaskType.ErrorIndexCopyTask) ||
                (taskType == StackHashTaskType.ErrorIndexMoveTask) ||
                (taskType == StackHashTaskType.PurgeTask) ||
                (taskType == StackHashTaskType.WinQualSynchronizeTask))
            {
                return true;
            }
            else
            {
                // The other tasks must remain alive for the profile to work properly.
                return false;
            }
        }


        /// <summary>
        /// Invoked when the state of the task changes. 
        /// </summary>
        /// <param name="sender">Object invoking this delegate.</param>
        /// <param name="e">Callback events.</param>
        private void StateChangedCallback(object sender, StateChangedEventArgs e)
        {
            // Check for task completed events and signal the main thread that a task may await removal from the task queue.
            if (e.TaskState.TaskCompleted)
            {
                m_Events[s_TaskCompletedEventIndex].Set();
            }
        }


        /// <summary>
        /// If a queued task is currently still running then this does nothing.
        /// If a queued task is not running and the queue is not empty then the next task is started.
        /// </summary>
        private void runQueuedTask()
        {
            if (m_CurrentTask == null)
            {
                // Execute the task at the head of the queue.
                Monitor.Enter(m_TaskQueue);
                try
                {
                    if (m_TaskQueue.Count != 0)
                        m_CurrentTask = m_TaskQueue.Dequeue();
                }
                finally
                {
                    Monitor.Exit(m_TaskQueue);
                }

                if (m_CurrentTask != null)
                {
                    // A task was found so run it.
                    // Hook the task up to the state changed event and run it.
                    m_CurrentTask.ThisTask.StateChanged += StateChangedCallback;
                    m_CurrentTask.ThisTask.Run();

                    // Signal that the task has been started.
                    OnTaskStarted(m_CurrentTask.ThisTask);
                }
            }
        }


        /// <summary>
        /// Removes completed tasks from the queue. Called when a task completed event is triggered.
        /// </summary>
        private void removeCompletedQueuedTasks()
        {
            // Note that only this method can set m_CurrentTask = null so no race conditions as
            // this method is only called on the task runner thread.

            TaskData currentTask = m_CurrentTask;
            if (currentTask != null)
            {
                // A task has signalled completion.
                // If it is a queued task then it should be the current task.
                if (m_CurrentTask.ThisTask.CurrentTaskState.TaskCompleted)
                {
                    // Unregister for events.
                    m_CurrentTask.ThisTask.StateChanged -= StateChangedCallback;

                    // Wait for the thread to complete - An abort will filter down to all running tasks
                    // so this will eventually return.
                    // If the thread has already completed then this should terminate immediately.
                    // Once the task has signalled completion it shouldn't be doing any work so the 
                    // thread should execute quickly after signalling completion.
                    m_CurrentTask.ThisTask.Join(Timeout.Infinite);

                    // Signal to the client that the task has completed.
                    OnTaskCompleted(m_CurrentTask.ThisTask);

                    // No current task is now running.
                    m_CurrentTask = null;

                    // Trigger an event so the queue is checked again to start any pending tasks.
                    m_Events[s_TaskQueuedEventIndex].Set();
                }
            }
        }


        /// <summary>
        /// Removes completed tasks from the concurrent task list.
        /// Note that more than one concurrent task may have completed.
        /// </summary>
        private void removeCompletedConcurrentTasks()
        {
            // List of completed tasks taken from the concurrent task queue.
            List<TaskData> completedTasks = new List<TaskData>();
            Monitor.Enter(m_ConcurrentTasks);

            try
            {
                // Have to loop like this - enumerators would become out of date if entries were
                // removed while enumerating.
                int taskIndex = 0;
                while (taskIndex < m_ConcurrentTasks.Count)
                {
                    if (m_ConcurrentTasks[taskIndex].ThisTask.CurrentTaskState.TaskCompleted)
                    {
                        // Unregister for events.
                        m_ConcurrentTasks[taskIndex].ThisTask.StateChanged -= StateChangedCallback;

                        // Wait for the thread to complete - An abort will filter down to all running tasks
                        // so this will eventually return.
                        // If the thread has already completed then this should terminate immediately.
                        // Once the task has signalled completion it shouldn't be doing any work so the 
                        // thread should execute quickly.
                        m_ConcurrentTasks[taskIndex].ThisTask.Join(Timeout.Infinite);

                        // Add the item to the list of tasks to signal completion for.
                        // This is done later so as not to cause reentrancy issues.
                        completedTasks.Add(m_ConcurrentTasks[taskIndex]);

                        // Remove this entry from concurrent list.
                        m_ConcurrentTasks.Remove(m_ConcurrentTasks[taskIndex]);
                    }

                    taskIndex++;
                }
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }

            // Notify anyone that wants to know that the task(s) have completed.
            foreach (TaskData task in completedTasks)
            {
                OnTaskCompleted(task.ThisTask);
            }
        }


        /// <summary>
        /// Main thread that runs the tasks on the queue. 
        /// This thread will sit waiting for tasks to process or for an Abort notification.
        /// TaskQueued events signal that an event has been placed on the queue - or the queue 
        /// should at least be checked for events to run.
        /// </summary>
        private void TaskRunnerStart()
        {
            try
            {
                // Keep looping until the thread is aborted.
                int eventIndex;
                while ((eventIndex = WaitHandle.WaitAny(m_Events)) != s_AbortEventIndex)
                {
                    if (eventIndex == s_TaskQueuedEventIndex)
                    {
                        // A task has been queued - start it if a queued task is not already running.
                        runQueuedTask();
                    }
                    else if (eventIndex == s_TaskCompletedEventIndex)
                    {
                        // A concurrent task or queued task may have completed. Find and remove completed tasks.
                        removeCompletedQueuedTasks();
                        removeCompletedConcurrentTasks();
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected event ID " + eventIndex.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (System.Exception ex)
            {
                // This shouldn't happen.
                DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal, "Task manager exiting", ex);
                throw;
            }
        }


        /// <summary>
        /// Called to indicate that a task has started.
        /// </summary>
        /// <param name="startedTask">The task that started.</param>
        protected virtual void OnTaskStarted(Task startedTask)
        {
            if (startedTask == null)
                throw new ArgumentNullException("startedTask");

            if (startedTask.LastException == null)
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Task started: " + startedTask.Name);
            else
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Task started: " + startedTask.Name + " " + startedTask.LastException);

            TaskStartedEventArgs args = new TaskStartedEventArgs();
            args.StartedTask = startedTask;

            EventHandler<TaskStartedEventArgs> handler = TaskStarted;

            if (handler != null)
                handler(this, args);

            // Inform the task that the TaskStarted notification has been sent. It can then carry on.
            startedTask.TaskStartedNotificationEvent.Set();
        }


        /// <summary>
        /// Called to indicate that a task has completed.
        /// Signals interested parties.
        /// </summary>
        /// <param name="completedTask">The task that completed.</param>
        protected virtual void OnTaskCompleted(Task completedTask)
        {
            if (completedTask == null)
                throw new ArgumentNullException("completedTask");

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Task completed: " + completedTask.Name);
            if (completedTask.LastException != null)
            {
                String errorString = String.Format(CultureInfo.InvariantCulture, "Task Failed: {0} {1}", 
                    completedTask.Name, completedTask.LastException);
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, errorString);
            }

            TaskCompletedEventArgs args = new TaskCompletedEventArgs();
            args.CompletedTask = completedTask;

            EventHandler<TaskCompletedEventArgs> handler = TaskCompleted;

            if (handler != null)
                handler(this, args);
        }


        /// <summary>
        /// Aborts the currently running task.
        /// </summary>
        /// <param name="wait">True - wait for task to complete, false - don't wait.</param>
        public virtual void AbortCurrentTask(bool wait)
        {
            TaskData currentTaskData = m_CurrentTask;
            if (currentTaskData != null)
            {
                Task currentTask = currentTaskData.ThisTask;

                if (currentTask != null)
                    currentTask.StopExternal();
                if (wait)
                    currentTask.Join(120000); // Wait up to 2 mins.
            }
        }


        /// <summary>
        /// Aborts all currently queued tasks.
        /// </summary>
        /// <param name="wait">True - wait for tasks to complete, false - don't wait.</param>
        public virtual void AbortAllTasks(bool wait)
        {
            // First, abort all tasks that are queued but not running.
            Monitor.Enter(m_TaskQueue);

            List<Task> completedTasks = new List<Task>();

            try
            {
                while (m_TaskQueue.Count != 0)
                {
                    TaskData thisTask = m_TaskQueue.Dequeue();

                    // Stop the task. Only the current task will be running from the queue - this gets stopped below.
                    thisTask.ThisTask.StopExternal();
                    completedTasks.Add(thisTask.ThisTask);
                }

                // Now report those tasks as complete.
                foreach (Task task in completedTasks)
                {
                    OnTaskCompleted(task);
                }

                // Now abort the currently running task and concurrent tasks.
                AbortCurrentTask(wait);
                AbortAllConcurrentTasks(wait);
            }
            finally
            {
                Monitor.Exit(m_TaskQueue);
            }
        }


        
        /// <summary>
        /// Aborts all concurrent tasks of the specified type.
        /// </summary>
        /// <param name="taskType">Type of task to abort.</param>
        /// <param name="wait">True - Wait for tasks to complete.</param>
        /// <param name="includeCurrentTask">True - abort current queued task if matching type.</param>
        public virtual void AbortTasksOfType(StackHashTaskType taskType, bool wait, bool includeCurrentTask)
        {
            // First, abort all tasks that are queued but not running.
            Monitor.Enter(m_ConcurrentTasks);

            TaskData currentTask = m_CurrentTask;

            if ((currentTask != null)
                && includeCurrentTask &&
                (currentTask.ThisTask != null) &&
                (currentTask.ThisTask.TaskType == taskType))
            {
                currentTask.ThisTask.StopExternal();
            }

            List<TaskData> tasksToStop = new List<TaskData>();

            try
            {
                foreach (TaskData taskData in m_ConcurrentTasks)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                    {
                        tasksToStop.Add(taskData);
                    }
                }

                foreach (TaskData task in tasksToStop)
                {
                    task.ThisTask.StopExternal();

                    if (wait)
                    {
                        task.ThisTask.Join(60000);
                    }
                }

                // The tasks will be removed from the concurrent task loop by the main thread when it receives the 
                // TaskCompleted notification.
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }
        }


        /// <summary>
        /// Gets all concurrent tasks of the specified type.
        /// </summary>
        /// <param name="taskType">Task type to retrieve.</param>
        [SuppressMessage("Microsoft.Design", "CA1002")]
        public List<Task> GetTasksOfType(StackHashTaskType taskType)
        {
            List<Task> allTasks = new List<Task>();

            // First, abort all tasks that are queued but not running.
            Monitor.Enter(m_ConcurrentTasks);

            try
            {
                foreach (TaskData taskData in m_ConcurrentTasks)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                    {
                        allTasks.Add(taskData.ThisTask);
                    }
                }

                return allTasks;
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }
        }

        
        /// <summary>
        /// Returns the first concurrent task of the specified type.
        /// </summary>
        /// <param name="taskType">Type of concurrent task to get.</param>
        public virtual Task GetConcurrentTaskOfType(StackHashTaskType taskType)
        {
            Monitor.Enter(m_ConcurrentTasks);

            try
            {
                foreach (TaskData taskData in m_ConcurrentTasks)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                    {
                        return taskData.ThisTask;
                    }
                }

                return null;
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }
        }


        /// <summary>
        /// Aborts all concurrent tasks without waiting for the tasks to complete.
        /// </summary>
        public virtual void AbortAllConcurrentTasks()
        {
            AbortAllConcurrentTasks(false);
        }


        /// <summary>
        /// Aborts all current concurrent tasks.
        /// </summary>
        /// <param name="wait">True - wait for tasks to complete. False - don't wait.</param>
        public virtual void AbortAllConcurrentTasks(bool wait)
        {
            Monitor.Enter(m_ConcurrentTasks);

            List<Task> completedTasks = new List<Task>();

            try
            {
                foreach (TaskData taskData in m_ConcurrentTasks)
                {
                    if (!taskData.ThisTask.CurrentTaskState.TaskCompleted)
                    {
                        taskData.ThisTask.StopExternal();
                        if (wait)
                            taskData.ThisTask.Join(120000); // May take a while for the debugger to stop - but don't wait forever.
                        completedTasks.Add(taskData.ThisTask);
                    }
                }
            }
            finally
            {
                m_ConcurrentTasks.Clear();
                Monitor.Exit(m_ConcurrentTasks);
            }

            // Now report those tasks as complete.
            foreach (Task task in completedTasks)
            {
                OnTaskCompleted(task);
            }
        }


        /// <summary>
        /// Checks if a task of the specified type is running in any queue.
        /// </summary>
        /// <param name="taskType">The task to check for.</param>
        public virtual bool IsTaskRunning(StackHashTaskType taskType)
        {
            TaskData currentTask = m_CurrentTask;

            // Check the current task running (from the queue).
            if ((currentTask != null) && (currentTask.ThisTask.TaskType == taskType))
                return true;

            if (IsQueuedTaskRunning(taskType) || (IsConcurrentTaskRunning(taskType)))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Checks the task queue to see if a task of the specified type is running.
        /// </summary>
        /// <param name="taskType">The task to check for.</param>
        public virtual bool IsQueuedTaskRunning(StackHashTaskType taskType)
        {
            Monitor.Enter(m_TaskQueue);

            // The mere presence in the queue is sufficient as the task will be removed once complete.
            try
            {
                foreach (TaskData taskData in m_TaskQueue)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                        return true;
                }
            }
            finally
            {
                Monitor.Exit(m_TaskQueue);
            }

            return false;
        }


        /// <summary>
        /// Checks the concurrent task list to see if a task of the specified type 
        /// is running.
        /// </summary>
        /// <param name="taskType">The task to check for.</param>
        public virtual bool IsConcurrentTaskRunning(StackHashTaskType taskType)
        {
            Monitor.Enter(m_ConcurrentTasks);

            // The mere presence in the list is sufficient as the task will be removed once complete.
            try
            {
                foreach (TaskData taskData in m_ConcurrentTasks)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                        return true;
                }
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }

            return false;
        }

        
        /// <summary>
        /// Gets the status of the task with the specified state.
        /// Only one task of each type will be running.
        /// </summary>
        /// <param name="taskType">Type of task to get status for.</param>
        public virtual StackHashTaskState GetTaskState(StackHashTaskType taskType)
        {
            TaskData currentTask = m_CurrentTask;

            if ((currentTask != null) &&
                (currentTask.ThisTask.TaskType == taskType))
            {
                return (currentTask.TaskStatus.TaskState);
            }


            // Check the queue.
            Monitor.Enter(m_TaskQueue);

            try
            {
                foreach (TaskData taskData in m_TaskQueue)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                        return StackHashTaskState.Queued;
                }
            }
            finally
            {
                Monitor.Exit(m_TaskQueue);
            }

            // Check the concurrent tasks.
            Monitor.Enter(m_ConcurrentTasks);

            try
            {
                foreach (TaskData taskData in m_ConcurrentTasks)
                {
                    if (taskData.ThisTask.TaskType == taskType)
                        return (taskData.TaskStatus.TaskState);
                }
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }

            
            return StackHashTaskState.NotRunning;
        }


        /// <summary>
        /// Aborts any sync and analyze task that is running and removes any sync tasks from the queue.
        /// </summary>
        public virtual void AbortSyncTask()
        {
            AbortTasksOfType(StackHashTaskType.WinQualSynchronizeTask, false, false);
            AbortTasksOfType(StackHashTaskType.AnalyzeTask, false, false);
        }


        /// <summary>
        /// Queues a task for processing. The task will be run when this TaskManager has completed
        /// all tasks ahead of this one on the queue.
        /// </summary>
        /// <param name="task">Task to add to the queue.</param>
        public void Enqueue(Task task)
        {
            TaskData TaskData = new TaskData(task);

            Monitor.Enter(m_TaskQueue);
            
            try
            {
                m_TaskQueue.Enqueue(TaskData);
                m_Events[s_TaskQueuedEventIndex].Set();
            }
            finally
            {
                Monitor.Exit(m_TaskQueue);
            }
        }

        /// <summary>
        /// Waits for a task start for the specified task.
        /// </summary>
        /// <param name="task">Task to wait for.</param>
        /// <param name="timeoutInMilliseconds">Time to wait in milliseconds.</param>
        public void WaitForTaskStart(Task task, int timeoutInMilliseconds)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            EventWaitHandle [] waitEvents = new EventWaitHandle [] {task.TaskStartedEvent};
            WaitHandle.WaitAny(waitEvents, timeoutInMilliseconds);
        }


        /// <summary>
        /// Waits for a task completion for the specified task.
        /// </summary>
        /// <param name="task">Task to wait for.</param>
        /// <param name="timeoutInMilliseconds">Time to wait in milliseconds.</param>
        public void WaitForTaskCompletion(Task task, int timeoutInMilliseconds)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            // First wait for the task to start.
            WaitForTaskStart(task, Timeout.Infinite);

            // Now wait for it to complete.
            task.Join(timeoutInMilliseconds);
        }


        /// <summary>
        /// Aborts the task manager thread and waits for it to complete.
        /// </summary>
        public void Close()
        {
            m_Events[s_AbortEventIndex].Set();
            m_TaskRunnerThread.Join();
        }


        /// <summary>
        /// Runs a concurrent task. The task is noted (so that it can be aborted if necessary)
        /// and then started.
        /// </summary>
        /// <param name="task">The task to run.</param>
        public void RunConcurrentTask(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            TaskData TaskData = new TaskData(task);

            Monitor.Enter(m_ConcurrentTasks);

            try
            {
                m_ConcurrentTasks.Add(TaskData);

                TaskData.ThisTask.StateChanged += StateChangedCallback;
                TaskData.ThisTask.Run();
            }
            finally
            {
                Monitor.Exit(m_ConcurrentTasks);
            }

            // Signal that the task has been started.
            OnTaskStarted(task);

            // When the task is complete it will post a TaskComplete event.
        }


        /// <summary>
        /// Returns the status of all known tasks. Note only one task of each type will be running.
        /// </summary>
        public StackHashTaskStatusCollection TaskStatuses
        {
            get
            {
                StackHashTaskStatusCollection taskStatuses = new StackHashTaskStatusCollection();

                Monitor.Enter(m_TaskQueue);
                Monitor.Enter(m_ConcurrentTasks);
                
                try
                {
                    StackHashTaskStatus status;

                    TaskData currentTask = m_CurrentTask;

                    // Get the current task status first.
                    if (currentTask != null)
                    {
                        status = currentTask.TaskStatus;
                        taskStatuses.Add(status);
                    }

                    if (m_TaskQueue != null)
                    {
                        // Get the status of all queued tasks.
                        foreach (TaskData task in m_TaskQueue)
                        {
                            status = task.TaskStatus;

                            taskStatuses.Add(status);
                        }
                    }

                    if (m_ConcurrentTasks != null)
                    {
                        // Get the status of all concurrent tasks.
                        foreach (TaskData task in m_ConcurrentTasks)
                        {
                            status = task.TaskStatus;

                            taskStatuses.Add(status);
                        }
                    }

                    return taskStatuses;
                }
                finally
                {
                    Monitor.Exit(m_TaskQueue);
                    Monitor.Exit(m_ConcurrentTasks);
                }
            }
        }


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Signal the abort so the queue thread completes.
                m_Events[s_AbortEventIndex].Set();
                m_TaskRunnerThread.Join();

                // Scrap all concurrent threads.
                AbortAllConcurrentTasks(true);

                foreach (AutoResetEvent signal in m_Events)
                {
                    signal.Close();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


    }
}
