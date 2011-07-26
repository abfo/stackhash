using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.IO;
using System.Collections.ObjectModel;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashDebug;
using StackHashUtilities;
using StackHashCabs;

namespace StackHashTasks
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506")]
    public class ControllerContext : IDisposable
    {
        private StackHashServiceErrorCode m_CurrentContextError; // Only accept commands if this is NoError.
        private System.Exception m_LastContextException;

        private TaskManager m_TaskController;
        private IErrorIndex m_ErrorIndex;
        private WinQualSyncTimerTask m_WinQualSyncTimerTask;
        private int m_Id;
        private StackHashDebuggerSettings m_DebuggerSettings;
        private ScriptResultsManager m_ScriptResultsManager;
        private ScriptManager m_ScriptManager;
        private IDebugger m_DebuggerManager;
        private SettingsManager m_SettingsManager;
        private bool m_IsActive;
        private bool m_LastActivateFailed;
        private StackHashContextSettings m_ContextSettings;
        private bool m_IsTestMode;
        private StackHashTestData m_TestData;
        private LicenseManager m_LicenseManager;
        private bool m_MoveInProgress;
        private bool m_CopyInProgress;
        private BugTrackerContext m_BugTrackerContext;
        private MailManager m_MailManager;
        private int m_MaxRowsToGetPerRequest = 5000;

        public TaskManager TaskController
        {
            get { return m_TaskController; }
        }

        public int MaxRowsToGetPerRequest
        {
            get { return m_MaxRowsToGetPerRequest; }
            set { m_MaxRowsToGetPerRequest = value; }
        }

        public IErrorIndex ErrorIndex
        {
            get { return m_ErrorIndex; }
        }

        public WinQualSettings WinQualSettings
        {
            get { return m_ContextSettings.WinQualSettings; }
        }

        public WinQualSyncTimerTask WinQualSyncTimerTask
        {
            get { return m_WinQualSyncTimerTask; }
            set { m_WinQualSyncTimerTask = value; }
        }

        public int Id
        {
            get { return m_Id; }
        }

        public bool IsActive
        {
            get { return m_IsActive; }
        }

        public bool LastActivateFailed
        {
            get { return m_LastActivateFailed; }
        }

        
        public StackHashDebuggerSettings DebuggerSettings
        {
            get { return m_DebuggerSettings; }
            set { m_DebuggerSettings = value; }
        }

        public ScriptResultsManager ScriptResultsManager
        {
            get { return m_ScriptResultsManager; }
            set { m_ScriptResultsManager = value; }
        }

        public event EventHandler<AdminReportEventArgs> AdminReports;


        /// <summary>
        /// Default constructor - used for testing only.
        /// </summary>
        public ControllerContext() // For testing purposes
        {
        }



        /// <summary>
        /// Called when a task has started. 
        /// Just calls through to the controller to report the event to clients.
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        private void taskStartedEventHandler(Object sender, EventArgs e)
        {
            bool sendEvent = false;
            bool sendToAll = true;

            TaskStartedEventArgs startEventArgs = e as TaskStartedEventArgs;

            StackHashAdminReport adminReport = new StackHashAdminReport();
            adminReport.ContextId = m_Id;
            adminReport.ClientData = startEventArgs.StartedTask.ClientData;
            adminReport.LastException = null;
            adminReport.ServiceErrorCode = StackHashException.GetServiceErrorCode(startEventArgs.StartedTask.LastException);
            adminReport.IsRetry = startEventArgs.StartedTask.TaskParameters.IsRetry;

            if (startEventArgs.StartedTask.LastException == null)
            {
                adminReport.ResultData = StackHashAsyncOperationResult.Success;
                adminReport.Message = null;
            }
            else
            {
                adminReport.ResultData = StackHashAsyncOperationResult.Failed;
                adminReport.LastException = startEventArgs.StartedTask.LastException.ToString();
                adminReport.Message = startEventArgs.StartedTask.LastException.Message;
            }

            switch (startEventArgs.StartedTask.TaskType)
            {
                case StackHashTaskType.WinQualSynchronizeTask:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncStarted;
                    sendEvent = true;
                    break;
                case StackHashTaskType.ErrorIndexMoveTask:
                    adminReport.Operation = StackHashAdminOperation.ErrorIndexMoveStarted;
                    sendEvent = true;
                    break;
                case StackHashTaskType.AnalyzeTask:
                    adminReport.Operation = StackHashAdminOperation.AnalyzeStarted;
                    sendEvent = true;
                    break;
                case StackHashTaskType.DebugScriptTask:
                    adminReport.Operation = StackHashAdminOperation.RunScriptStarted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.WinQualLogOnTask:
                    adminReport.Operation = StackHashAdminOperation.WinQualLogOnStarted;
                    break;
                case StackHashTaskType.PurgeTask:
                    adminReport.Operation = StackHashAdminOperation.PurgeStarted;
                    sendEvent = true;
                    break;
                case StackHashTaskType.WinQualSynchronizeTimerTask:
                    sendEvent = false;
                    break;
                case StackHashTaskType.PurgeTimerTask:
                    sendEvent = false;
                    break;
                case StackHashTaskType.DownloadCabTask:
                    adminReport.Operation = StackHashAdminOperation.DownloadCabStarted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.ErrorIndexCopyTask:
                    adminReport.Operation = StackHashAdminOperation.ErrorIndexCopyStarted;
                    sendEvent = true;
                    break;
                case StackHashTaskType.BugTrackerTask:
                    adminReport.Operation = StackHashAdminOperation.BugTrackerStarted;
                    sendEvent = false;
                    break;
                case StackHashTaskType.BugReportTask:
                    adminReport.Operation = StackHashAdminOperation.BugReportStarted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.UploadFileTask:
                    adminReport.Operation = StackHashAdminOperation.UploadFileStarted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                default:
                    throw new NotSupportedException("Task type not supported");
            }


            // Tell the clients if this is a task start they should know about.
            if (sendEvent)
                onAdminReport(adminReport, sendToAll);
        }


        /// <summary>
        /// Processing necessary when an index is copied from 1 location to another.
        /// </summary>
        /// <param name="completedEventArgs">Completed task arguments.</param>
        private void ProcessErrorIndexCopyComplete(TaskCompletedEventArgs completedEventArgs)
        {
            if (completedEventArgs.CompletedTask.LastException == null)
            {
                ErrorIndexCopyTaskParameters taskParams = (completedEventArgs.CompletedTask as ErrorIndexCopyTask).TaskParameters as ErrorIndexCopyTaskParameters;
                IErrorIndex oldIndex = m_ErrorIndex;

                if (taskParams.AssignCopyToContext)
                {
                    ChangeIndex(taskParams.DestinationErrorIndexSettings, taskParams.DestinationSqlSettings);
                }

                if (taskParams.DeleteSourceIndexWhenComplete)
                {
                    if (oldIndex != null)
                    {
                        oldIndex.DeleteIndex();
                        oldIndex.Dispose();
                    }
                }
            }

        }


        /// <summary>
        /// Called when a task has completed. 
        /// Just calls through to the controller to report the event to clients.
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        private void taskCompletedEventHandler(Object sender, EventArgs e)
        {
            bool sendEvent = false;
            bool sendToAll = true;
            StackHashAdminReport adminReport = new StackHashAdminReport();

            TaskCompletedEventArgs completedEventArgs = e as TaskCompletedEventArgs;

            switch (completedEventArgs.CompletedTask.TaskType)
            {
                case StackHashTaskType.WinQualSynchronizeTask:
                    WinQualSyncTask winQualSyncTask = completedEventArgs.CompletedTask as WinQualSyncTask;

                    StackHashWinQualSyncCompleteAdminReport syncAdminReport = new StackHashWinQualSyncCompleteAdminReport();
                    syncAdminReport.Operation = StackHashAdminOperation.WinQualSyncCompleted;
                    syncAdminReport.ErrorIndexStatistics = winQualSyncTask.Statistics;
                    adminReport = syncAdminReport;
                    sendEvent = true;
                    break;
                case StackHashTaskType.ErrorIndexMoveTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.ErrorIndexMoveCompleted;
                    sendEvent = true;
                    m_MoveInProgress = false;
                    break;
                case StackHashTaskType.AnalyzeTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.AnalyzeCompleted;
                    sendEvent = true;
                    break;
                case StackHashTaskType.DebugScriptTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.RunScriptCompleted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.WinQualLogOnTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.WinQualLogOnCompleted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.PurgeTask:
                    PurgeTask purgeTask = completedEventArgs.CompletedTask as PurgeTask;
                    StackHashPurgeCompleteAdminReport purgeAdminReport = new StackHashPurgeCompleteAdminReport();
                    purgeAdminReport.Operation = StackHashAdminOperation.PurgeCompleted;
                    purgeAdminReport.PurgeStatistics = purgeTask.Statistics;
                    adminReport = purgeAdminReport;
                    sendEvent = true;
                    break;
                case StackHashTaskType.WinQualSynchronizeTimerTask:
                    sendEvent = false;
                    break;
                case StackHashTaskType.PurgeTimerTask:
                    sendEvent = false;
                    break;
                case StackHashTaskType.DownloadCabTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.DownloadCabCompleted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.ErrorIndexCopyTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.ErrorIndexCopyCompleted;
                    sendEvent = true;
                    ProcessErrorIndexCopyComplete(completedEventArgs);
                    m_CopyInProgress = false;
                    break;
                case StackHashTaskType.BugTrackerTask:
                    sendEvent = false;
                    break;
                case StackHashTaskType.BugReportTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.BugReportCompleted;
                    adminReport.Description = (completedEventArgs.CompletedTask.TaskParameters as BugReportTaskParameters).ReportRequest.ToString();
                    sendEvent = true;
                    sendToAll = false;
                    break;
                case StackHashTaskType.UploadFileTask:
                    adminReport = new StackHashAdminReport();
                    adminReport.Operation = StackHashAdminOperation.UploadFileCompleted;
                    sendEvent = true;
                    sendToAll = false;
                    break;
                default:
                    throw new NotSupportedException("Task type not supported");
            }

            if (sendEvent)
            {
                adminReport.IsRetry = completedEventArgs.CompletedTask.TaskParameters.IsRetry;
                adminReport.ServiceErrorCode = StackHashException.GetServiceErrorCode(completedEventArgs.CompletedTask.LastException);
                if (completedEventArgs.CompletedTask.LastException == null)
                {
                    adminReport.ResultData = StackHashAsyncOperationResult.Success;
                    adminReport.Message = null;
                }
                else
                {
                    if (completedEventArgs.CompletedTask.CurrentTaskState.Aborted)
                        adminReport.ResultData = StackHashAsyncOperationResult.Aborted;
                    else
                        adminReport.ResultData = StackHashAsyncOperationResult.Failed;
                    adminReport.LastException = completedEventArgs.CompletedTask.LastException.ToString();
                    adminReport.Message = completedEventArgs.CompletedTask.LastException.Message;
                }

                adminReport.ContextId = m_Id;
                adminReport.ClientData = completedEventArgs.CompletedTask.ClientData;


                onAdminReport(adminReport, sendToAll);
            }

            // If this is this is a WinQualSync then start a cab unwrap - if the WinQualSync was successful and
            // not aborted.
            if (completedEventArgs.CompletedTask.TaskType == StackHashTaskType.WinQualSynchronizeTask)
            {
                if (completedEventArgs.CompletedTask.LastException == null)
                {
                    if (completedEventArgs.CompletedTask.CurrentTaskState.AbortRequested == false)
                    {
                        WinQualSyncTask syncTask = completedEventArgs.CompletedTask as WinQualSyncTask;

                        // Don't run the sync task of only syncing products.
                        if (!syncTask.JustSyncProductList)
                            RunAnalyzeTask(completedEventArgs.CompletedTask.ClientData, false, false);
                    }
                }
                else
                {
                    // Only retry if the scheduled task started the WinQualSync task.
                    if (completedEventArgs.CompletedTask.ClientData == null)
                    {
                        // Don't restart the synchronized task if the user aborted it or license has expired.
                        if ((completedEventArgs.CompletedTask.CurrentTaskState.AbortRequested == false) &&
                            (adminReport.ServiceErrorCode != StackHashServiceErrorCode.LicenseEventCountExceeded) &&
                            (adminReport.ServiceErrorCode != StackHashServiceErrorCode.LicenseExpired))
                        {
                            if (m_ContextSettings.WinQualSettings.RetryAfterError)
                            {
                                StackHashServiceErrorCode serviceErrorCode = StackHashException.GetServiceErrorCode(completedEventArgs.CompletedTask.LastException);

                                // Don't start the task again if a programmer error occurred.
                                if (serviceErrorCode != StackHashServiceErrorCode.UnexpectedError)
                                {
                                    if (m_WinQualSyncTimerTask != null)
                                        m_WinQualSyncTimerTask.Reschedule(m_ContextSettings.WinQualSettings.DelayBeforeRetryInSeconds);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Called when an admin report should be sent to 1 or more clients.
        /// </summary>
        /// <param name="adminReport">The admin report.</param>
        /// <param name="sendToAll">True - send to all clients, false - just send to specific client.</param>
        private void onAdminReport(StackHashAdminReport adminReport, bool sendToAll)
        {
            m_MailManager.SendAdminEmails(adminReport);

            if (AdminReports != null)
                AdminReports(this, new AdminReportEventArgs(adminReport, sendToAll));
        }


        /// <summary>
        /// Sets a new index for the current context.
        /// The caller must record the old index and dispose it if necessary.
        /// </summary>
        /// <param name="errorIndexSettings">Error index settings.</param>
        /// <param name="sqlSettings">Sql Server settings.</param>
        public void ChangeIndex(ErrorIndexSettings errorIndexSettings, StackHashSqlConfiguration sqlSettings)
        {
            if (errorIndexSettings == null)
                throw new ArgumentNullException("errorIndexSettings");

            IErrorIndex newIndex = null;

            if (errorIndexSettings.Type == ErrorIndexType.Xml)
            {
                newIndex = new XmlErrorIndex(errorIndexSettings.Folder, errorIndexSettings.Name);

                // Add a cache.
                newIndex = new ErrorIndexCache(newIndex);
            }
            else if (errorIndexSettings.Type == ErrorIndexType.SqlExpress)
            {
                newIndex = new SqlErrorIndex(sqlSettings, errorIndexSettings.Name, errorIndexSettings.Folder);
            }
            else
            {
                throw new NotImplementedException("SQL database not implemented yet");
            }

            // Make sure the status is recorded for the context.
            if (newIndex.Status != errorIndexSettings.Status)
            {
                errorIndexSettings.Status = newIndex.Status;
            }

            m_SettingsManager.SetContextErrorIndexSettings(m_Id, errorIndexSettings, sqlSettings);
            m_ContextSettings = m_SettingsManager.Find(m_Id);

            m_ErrorIndex = newIndex;
        }


        /// <summary>
        /// Creates a new controller context using the specified settings.
        /// </summary>
        /// <param name="contextSettings">Settings used to create the new context</param>
        /// <param name="scriptManager">Manages debugger scripts.</param>
        /// <param name="debugger">Debugger object.</param>
        /// <param name="settingsManager">Settings manager.</param>
        /// <param name="winQualServices">WinQual connextivity component.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        [SuppressMessage("Microsoft.Design", "CA1031", Justification="This constructor cannot throw an exception")]
        public ControllerContext(StackHashContextSettings contextSettings, ScriptManager scriptManager,
            IDebugger debugger, SettingsManager settingsManager, bool isTestMode, StackHashTestData testData, LicenseManager licenseManager)
        {
            if (contextSettings == null)
                throw new ArgumentNullException("contextSettings");
            if (scriptManager == null)
                throw new ArgumentNullException("scriptManager");
            if (debugger == null)
                throw new ArgumentNullException("debugger");
            if (settingsManager == null)
                throw new ArgumentNullException("settingsManager");
            if (licenseManager == null)
                throw new ArgumentNullException("licenseManager");

            m_CurrentContextError = StackHashServiceErrorCode.NoError;
            m_LastContextException = null;

            m_IsTestMode = isTestMode;
            m_TestData = testData;

            bool settingsChanged = false;

            m_ContextSettings = contextSettings;
            m_ScriptManager = scriptManager;
            m_DebuggerManager = debugger;
            m_SettingsManager = settingsManager;
            m_LicenseManager = licenseManager;
            m_Id = m_ContextSettings.Id;

            try
            {
                // Create a task manager for the context and hook up to events.
                m_TaskController = new TaskManager(contextSettings.Id.ToString(CultureInfo.InvariantCulture));
                m_TaskController.TaskStarted += new EventHandler<TaskStartedEventArgs>(this.taskStartedEventHandler);
                m_TaskController.TaskCompleted += new EventHandler<TaskCompletedEventArgs>(this.taskCompletedEventHandler);

                // Default the debugger settings if necessary.
                if (m_ContextSettings.DebuggerSettings == null)
                    m_ContextSettings.DebuggerSettings = new StackHashDebuggerSettings();

                if (String.IsNullOrEmpty(m_ContextSettings.DebuggerSettings.DebuggerPathAndFileName))
                {
                    m_ContextSettings.DebuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
                    settingsChanged = true;
                }

                if ((m_ContextSettings.DebuggerSettings.BinaryPath == null) ||
                    (m_ContextSettings.DebuggerSettings.BinaryPath.Count == 0))
                {
                    m_ContextSettings.DebuggerSettings.BinaryPath = new StackHashSearchPath();
                    m_ContextSettings.DebuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
                    settingsChanged = true;
                }

                if ((m_ContextSettings.DebuggerSettings.SymbolPath == null) ||
                    (m_ContextSettings.DebuggerSettings.SymbolPath.Count == 0))
                {
                    m_ContextSettings.DebuggerSettings.SymbolPath = new StackHashSearchPath();
                    m_ContextSettings.DebuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
                    settingsChanged = true;
                }

                if (String.IsNullOrEmpty(m_ContextSettings.DebuggerSettings.DebuggerPathAndFileName64Bit))
                {
                    m_ContextSettings.DebuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;
                    settingsChanged = true;
                }

                if ((m_ContextSettings.DebuggerSettings.BinaryPath64Bit == null) ||
                    (m_ContextSettings.DebuggerSettings.BinaryPath64Bit.Count == 0))
                {
                    m_ContextSettings.DebuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
                    m_ContextSettings.DebuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
                    settingsChanged = true;
                }

                if ((m_ContextSettings.DebuggerSettings.SymbolPath64Bit == null) ||
                    (m_ContextSettings.DebuggerSettings.SymbolPath64Bit.Count == 0))
                {
                    m_ContextSettings.DebuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
                    m_ContextSettings.DebuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
                    settingsChanged = true;
                }

                // Initialise the Analyze task fields if not present. This may happen pre-beta as the field was 
                // May 4th 2010. This block can be removed pre-beta release.
                if (m_ContextSettings.AnalysisSettings == null)
                {
                    m_ContextSettings.AnalysisSettings = new StackHashAnalysisSettings();
                    m_ContextSettings.AnalysisSettings.ForceRerun = false;
                    settingsChanged = true;
                }

                m_DebuggerSettings = m_ContextSettings.DebuggerSettings;

                // Connect the associated ErrorIndex. This could be XML, SQL or SQLExpress.
                m_ErrorIndex = null;

                if (AppSettings.ForceSqlDatabase)
                    m_ContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

                if (m_ContextSettings.ErrorIndexSettings.Type == ErrorIndexType.Xml)
                {
                    m_ErrorIndex = new XmlErrorIndex(m_ContextSettings.ErrorIndexSettings.Folder,
                                                   m_ContextSettings.ErrorIndexSettings.Name);

                    // Add a cache.
                    m_ErrorIndex = new ErrorIndexCache(m_ErrorIndex);
                }
                else if (m_ContextSettings.ErrorIndexSettings.Type == ErrorIndexType.SqlExpress)
                {
                    m_ErrorIndex = new SqlErrorIndex(contextSettings.SqlSettings,
                                                     m_ContextSettings.ErrorIndexSettings.Name,
                                                     m_ContextSettings.ErrorIndexSettings.Folder);
                }
                else
                {
                    throw new NotImplementedException("SQL database not implemented yet");
                }

                m_MailManager = new MailManager(m_ContextSettings.EmailSettings, m_ContextSettings.WinQualSettings.CompanyName);

                // Check the status of the index.
                if (m_ErrorIndex.Status != m_ContextSettings.ErrorIndexSettings.Status)
                {
                    // Make sure the status is recorded for the context.
                    m_ContextSettings.ErrorIndexSettings.Status = m_ErrorIndex.Status;
                    settingsChanged = true;
                }

                // Performs validation which may cause an exception.
                if (settingsChanged)
                    m_SettingsManager.SetContextSettings(m_ContextSettings, true);  

                if (m_ContextSettings.IsActive)
                {
                    Activate(null, false);
                }

            }

            catch (System.Exception ex)
            {
                m_CurrentContextError = StackHashServiceErrorCode.ContextLoadError;
                m_LastContextException = ex;

                StackHashException stackHashException = ex as StackHashException;
                if (stackHashException != null)
                {
                    if (stackHashException.ServiceErrorCode != StackHashServiceErrorCode.NoError)
                        m_CurrentContextError = stackHashException.ServiceErrorCode;
                }

                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Error constructing context", ex);

                try
                {
                    // Only set the retry on this context if it was intended to be activated.
                    if (m_ContextSettings.IsActive)
                        m_LastActivateFailed = true;
                    m_ContextSettings.IsActive = false;
                    m_SettingsManager.SetContextSettings(m_ContextSettings, true);
                }
                catch (System.Exception ex2)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Error writing settings file", ex2);
                }
                // Don't rethrow - just absorb.
            }
        }


        /// <summary>
        /// Sends a context activation report to all clients. This needs to be done in a separate thread to avoid deadlocking 
        /// the caller.
        /// </summary>
        /// <param name="clientData">Client who requested the activation/deactivation.</param>
        /// <param name="isActivate">True - activate, False - deactivate.</param>
        /// <param name="lastException">Exception that occurred during the call or null if no exception.</param>
        private void sendContextAdminReport(StackHashClientData clientData, bool isActivate, System.Exception lastException)
        {
            Thread newThread = new Thread(delegate()
            {
                // Send an admin report indicating the state has changed.
                StackHashContextStateAdminReport adminReport = new StackHashContextStateAdminReport(m_IsActive, isActivate);
                adminReport.ClientData = clientData;
                adminReport.ContextId = m_Id;
                adminReport.Description = "Context activation requested";
                adminReport.ServiceErrorCode = StackHashServiceErrorCode.NoError;

                if (lastException != null)
                    adminReport.LastException = lastException.ToString();

                if (isActivate)
                {
                    if (m_IsActive)
                    {
                        adminReport.Message = "Activate attempt succeeded";
                        adminReport.ResultData = StackHashAsyncOperationResult.Success;
                    }
                    else
                    {
                        adminReport.Message = "Activate attempt failed";
                        adminReport.ResultData = StackHashAsyncOperationResult.Failed;
                        adminReport.ServiceErrorCode = StackHashServiceErrorCode.ActivateFailed;
                    }
                }
                else
                {
                    if (m_IsActive)
                    {
                        adminReport.Message = "Deactivate attempt failed";
                        adminReport.ResultData = StackHashAsyncOperationResult.Failed;
                        adminReport.ServiceErrorCode = StackHashServiceErrorCode.DeactivateFailed;
                    }
                    else
                    {
                        adminReport.Message = "Deactivate attempt succeeded";
                        adminReport.ResultData = StackHashAsyncOperationResult.Success;
                    }
                }
                adminReport.Operation = StackHashAdminOperation.ContextStateChanged;
                onAdminReport(adminReport, true);
            });

            newThread.Start();
        }


        /// <summary>
        /// Activates the specified context. 
        /// </summary>
        public void Activate()
        {
            Activate(null, false);
        }

        /// <summary>
        /// Performs tests on the the database and cab folders.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public ErrorIndexConnectionTestResults TestDatabaseConnection()
        {
            ErrorIndexConnectionTestResults results = m_ErrorIndex.GetDatabaseStatus();

            // Try accessing the cab folder.
            results.IsCabFolderAccessible = false;
            if (!String.IsNullOrEmpty(m_ErrorIndex.ErrorIndexPath))
            {
                if (Directory.Exists(m_ErrorIndex.ErrorIndexPath))
                {
                    // Try to create a test file in the cabFolder.
                    String testFile = Path.Combine(m_ErrorIndex.ErrorIndexPath, "AccessTest.txt");

                    FileStream testFileStream = null;

                    try
                    {
                        if (File.Exists(testFile))
                            File.Delete(testFile);

                        testFileStream = File.Create(testFile);

                        results.IsCabFolderAccessible = true;
                        results.CabFolderAccessLastException = null;
                    }
                    catch (System.Exception ex)
                    {
                        results.CabFolderAccessLastException = ex;
                    }
                    finally
                    {
                        if (testFileStream != null)
                        {
                            testFileStream.Close();

                            if (File.Exists(testFile))
                                File.Delete(testFile);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Activates the specified context.
        /// </summary>
        /// <param name="contextData">Identifies the client activating the context.</param>
        public void Activate(StackHashClientData clientData, bool createIndexInDefaultLocation)
        {
            if (m_IsActive)
                return;

            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);


            // Check if the settings can be activated. If, say, the error is already in use then 
            // it cannot be activated again. This call throws an exception if there is an error.
            m_SettingsManager.PreActivationCheck(this.Id);
            
            // Don't allow activation of the default name.
            if (m_ErrorIndex.ErrorIndexName.ToUpperInvariant() == XmlErrorIndex.DefaultErrorIndexName.ToUpperInvariant())
                throw new StackHashException("Error Index Name must be changed from the default", StackHashServiceErrorCode.ErrorIndexNameIsDefault);

            try
            {
                // Activate the error index.
                m_ErrorIndex.Activate(m_IsTestMode == true, createIndexInDefaultLocation);  // Create index only in test mode.

                // Check the status of the index. The activate should have created the index.
                if (m_ErrorIndex.Status != m_ContextSettings.ErrorIndexSettings.Status)
                {
                    // Make sure the status is recorded for the context.
                    m_ContextSettings.ErrorIndexSettings.Status = m_ErrorIndex.Status;
                }

                // Create the object that manages script results.
                m_ScriptResultsManager =
                    new ScriptResultsManager(m_ErrorIndex, m_ScriptManager, m_DebuggerManager, m_ContextSettings.DebuggerSettings);

                // Activate the bug tracking modules.
                m_BugTrackerContext = new BugTrackerContext(BugTrackerManager.CurrentBugTrackerManager, m_ContextSettings.BugTrackerSettings);

                // Activate the Update table only if there are plugins to service it.
                if (m_BugTrackerContext.NumberOfLoadedPlugIns > 0)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Activating Update Table: " + m_BugTrackerContext.NumberOfLoadedPlugIns.ToString(CultureInfo.InvariantCulture));
                    m_ErrorIndex.UpdateTableActive = true;
                }

                // Set the new state to active - in memory and in the settings file.
                m_IsActive = true;
                m_ContextSettings = m_SettingsManager.Find(m_Id);
                m_ContextSettings.IsActive = true;
                m_ContextSettings.IsIndexCreated = true;
                m_SettingsManager.SetContextSettings(m_ContextSettings, true);

                // Start the WinQualSyncTimerTask - do this last otherwise the AdminReport won't see the context as active
                // and therefore won't set the stats for the WinQualSync task.
                m_WinQualSyncTimerTask = startWinQualTimerSyncTask(m_ContextSettings);

                // Set the Purge task running here.
                startPurgeTimerTask(m_ContextSettings);

                // Set the Bug tracking task running. This task will wait for updates in the Update Table of the Error Index
                // and report them to any bug tracking modules that might be present.
                startBugTrackerTask(m_ContextSettings);

                sendContextAdminReport(clientData, true, null);
                m_CurrentContextError = StackHashServiceErrorCode.NoError;
                m_LastContextException = null;
            }
            catch (System.Exception ex)
            {
                m_ErrorIndex.Deactivate(); // This may be active already so deactivate it.

                m_CurrentContextError = StackHashServiceErrorCode.ContextActivationError;
                m_LastContextException = ex;

                StackHashException stackHashException = ex as StackHashException;
                if (stackHashException != null)
                {
                    if (stackHashException.ServiceErrorCode != StackHashServiceErrorCode.NoError)
                        m_CurrentContextError = stackHashException.ServiceErrorCode;
                }

                sendContextAdminReport(clientData, true, ex);
                Deactivate(clientData, false); // Don't allow the deactivate to report.
                throw;
            }
        }


        /// <summary>
        /// Deactivates the context sending an admin report with no client data. Should only be called from tests.
        /// </summary>
        /// <param name="contextData">Identifies the client deactivating the context.</param>
        public void Deactivate()
        {
            Deactivate(null, true);
        }

        /// <summary>
        /// Deactivates the context.
        /// </summary>
        /// <param name="contextData">Identifies the client deactivating the context.</param>
        /// <param name="sendAdminReport">True - sends an admin report, False - doesn't send admin report.</param>
        public void Deactivate(StackHashClientData clientData, bool sendAdminReport)
        {
            if (!m_IsActive)
                return;
            try
            {
                m_ContextSettings = m_SettingsManager.Find(m_Id);
                m_ContextSettings.IsActive = false;
                m_SettingsManager.SetContextSettings(m_ContextSettings, false);

                stopWinQualTimerSyncTask(); // This is a permanently running task.
                stopPurgeTimerTask();
                stopBugTrackerTask();

                m_TaskController.AbortAllTasks(true); // Wait for the tasks to complete.

                // Null the dynamic objects so they can get garbage collected.
                m_WinQualSyncTimerTask = null;
                m_ScriptResultsManager = null;
                m_IsActive = false;
                m_ErrorIndex.Deactivate(); // Stop anyone using the error index.

                m_BugTrackerContext.Dispose();
                m_BugTrackerContext = null;

                if (sendAdminReport)
                    sendContextAdminReport(clientData, false, null);
            }
            catch (System.Exception ex)
            {
                if (sendAdminReport)
                    sendContextAdminReport(clientData, false, ex);
                throw;
            }
        }


        /// <summary>
        /// Stop the WinQualTimerSyncTask. This is called when the settings have changed.
        /// </summary>
        private void stopWinQualTimerSyncTask()
        {
            if (m_WinQualSyncTimerTask != null)
            {
                m_WinQualSyncTimerTask.StopExternal();
                m_WinQualSyncTimerTask.Join(Timeout.Infinite);
            }
            m_WinQualSyncTimerTask = null;
        }


        /// <summary>
        /// Stop the PurgeTimerTask. This is called when the settings have changed.
        /// </summary>
        private void stopPurgeTimerTask()
        {
            m_TaskController.AbortTasksOfType(StackHashTaskType.PurgeTimerTask, true, false);
        }

        /// <summary>
        /// Stop the BugTrackerTask. 
        /// </summary>
        private void stopBugTrackerTask()
        {
            m_TaskController.AbortTasksOfType(StackHashTaskType.BugTrackerTask, true, false);
        }

        
        /// <summary>
        /// Starts the WinQualTimerSyncTask within the specified context.
        /// </summary>
        /// <param name="contextSettings">Settings for the context.</param>
        private WinQualSyncTimerTask startWinQualTimerSyncTask(StackHashContextSettings contextSettings)
        {
            WinQualSyncTimerTaskParameters winQualSyncTimerTaskParameters = new WinQualSyncTimerTaskParameters();

            // Set standard task settings.
            winQualSyncTimerTaskParameters.SettingsManager = m_SettingsManager;
            winQualSyncTimerTaskParameters.IsBackgroundTask = true;
            winQualSyncTimerTaskParameters.UseSeparateThread = true;
            winQualSyncTimerTaskParameters.Name = "WinQualSyncTimerTask" + contextSettings.Id.ToString(CultureInfo.InvariantCulture);
            winQualSyncTimerTaskParameters.RunInParallel = true;
            winQualSyncTimerTaskParameters.ContextId = this.Id;
            winQualSyncTimerTaskParameters.ControllerContext = this;
            

            // Settings specific to the winqual timer sync task.
            winQualSyncTimerTaskParameters.UserName = contextSettings.WinQualSettings.UserName;
            winQualSyncTimerTaskParameters.Password = contextSettings.WinQualSettings.Password;
            winQualSyncTimerTaskParameters.ScheduleCollection = contextSettings.WinQualSyncSchedule;

            WinQualSyncTimerTask winQualSyncTimerTask = null;

            try
            {
                winQualSyncTimerTask = new WinQualSyncTimerTask(winQualSyncTimerTaskParameters);
                this.TaskController.RunConcurrentTask(winQualSyncTimerTask);

                m_WinQualSyncTimerTask = winQualSyncTimerTask;

                return winQualSyncTimerTask;
            }
            catch (System.Exception)
            {
                if (winQualSyncTimerTask != null)
                    winQualSyncTimerTask.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Starts the PurgeTimerTask within the specified context.
        /// </summary>
        /// <param name="contextSettings">Settings for the context.</param>
        private Task startPurgeTimerTask(StackHashContextSettings contextSettings)
        {
            PurgeTimerTaskParameters purgeTimerTaskParameters = new PurgeTimerTaskParameters();

            // Set standard task settings.
            purgeTimerTaskParameters.SettingsManager = m_SettingsManager;
            purgeTimerTaskParameters.IsBackgroundTask = true;
            purgeTimerTaskParameters.UseSeparateThread = true;
            purgeTimerTaskParameters.Name = "PurgeTimerTask" + contextSettings.Id.ToString(CultureInfo.InvariantCulture);
            purgeTimerTaskParameters.RunInParallel = true;
            purgeTimerTaskParameters.ContextId = this.Id;
            purgeTimerTaskParameters.ControllerContext = this;


            // Settings specific to the winqual timer sync task.
            purgeTimerTaskParameters.ScheduleCollection = contextSettings.CabFilePurgeSchedule;

            PurgeTimerTask purgeTimerTask = null;

            try
            {
                purgeTimerTask = new PurgeTimerTask(purgeTimerTaskParameters);
                this.TaskController.RunConcurrentTask(purgeTimerTask);

                return purgeTimerTask;
            }
            catch (System.Exception)
            {
                if (purgeTimerTask != null)
                    purgeTimerTask.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Starts the bug tracker task. This task sits waiting for updates to the error index Update Table 
        /// and passes those change details on to any available Bug Tracking system plug-ins.
        /// </summary>
        /// <param name="contextSettings">Settings for the context.</param>
        private Task startBugTrackerTask(StackHashContextSettings contextSettings)
        {
            BugTrackerTaskParameters bugTrackerTaskParameters = new BugTrackerTaskParameters();

            // Set standard task settings.
            bugTrackerTaskParameters.SettingsManager = m_SettingsManager;
            bugTrackerTaskParameters.IsBackgroundTask = true;
            bugTrackerTaskParameters.UseSeparateThread = true;
            bugTrackerTaskParameters.Name = "BugTrackerTask" + contextSettings.Id.ToString(CultureInfo.InvariantCulture);
            bugTrackerTaskParameters.RunInParallel = true;
            bugTrackerTaskParameters.ContextId = this.Id;
            bugTrackerTaskParameters.ControllerContext = this;

            bugTrackerTaskParameters.ErrorIndex = m_ErrorIndex;
            bugTrackerTaskParameters.PlugInContext = m_BugTrackerContext;
            bugTrackerTaskParameters.PlugInManager = BugTrackerManager.CurrentBugTrackerManager;
            bugTrackerTaskParameters.TheScriptResultsManager = m_ScriptResultsManager;


            BugTrackerTask bugTrackerTask = null;

            try
            {
                bugTrackerTask = new BugTrackerTask(bugTrackerTaskParameters);
                this.TaskController.RunConcurrentTask(bugTrackerTask);

                return bugTrackerTask;
            }
            catch (System.Exception)
            {
                if (bugTrackerTask != null)
                    bugTrackerTask.Dispose();
                throw;
            }
        }

        
        
        /// <summary>
        /// Removes the specified context settings.
        /// </summary>
        /// <param name="resetNextContextIdIfAppropriate">true - resets the next context ID to 0 if no more contexts.</param>
        public void RemoveContextSettings(bool resetNextContextIdIfAppropriate)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            m_SettingsManager.RemoveContextSettings(m_Id, resetNextContextIdIfAppropriate);
        }

        private bool foldersAreIdentical(String folder1, String folder2)
        {
            String processedFolder1 = PathUtils.ProcessPath(folder1);
            String processedFolder2 = PathUtils.ProcessPath(folder2);

            return (String.Compare(processedFolder1, processedFolder2, StringComparison.OrdinalIgnoreCase) == 0);
        }

    
        /// <summary>
        /// Called to change the current settings for the context.
        /// </summary>
        /// <param name="contextSettings">The new context settings.</param>
        public void UpdateSettings(StackHashContextSettings contextSettings)
        {
            if (contextSettings == null)
                throw new ArgumentNullException("contextSettings");

            // Force the sql initial database name to be the same as the index name.
            contextSettings.SqlSettings.InitialCatalog = contextSettings.ErrorIndexSettings.Name;

            bool indexChanged = false;

            // Strip any end backslash.
            String originalFolder = contextSettings.ErrorIndexSettings.Folder;

            if (!foldersAreIdentical(contextSettings.ErrorIndexSettings.Folder, m_ContextSettings.ErrorIndexSettings.Folder) ||
                (String.Compare(contextSettings.ErrorIndexSettings.Name, m_ContextSettings.ErrorIndexSettings.Name, StringComparison.OrdinalIgnoreCase) != 0))
            {
                indexChanged = true;
            }


            // Change the settings.
            // Can't change the error index settings once the index has been created.
            if (m_ContextSettings.ErrorIndexSettings.Status == ErrorIndexStatus.Created)
                m_SettingsManager.SetContextSettings(contextSettings, false);
            else
                m_SettingsManager.SetContextSettings(contextSettings, true);

            m_ContextSettings = m_SettingsManager.Find(m_Id);

            // This will just rename the index if it has not previously been created.
            if (indexChanged)
                m_ErrorIndex.MoveIndex(m_ContextSettings.ErrorIndexSettings.Folder,
                    m_ContextSettings.ErrorIndexSettings.Name, contextSettings.SqlSettings, false);

            if (m_ContextSettings.ErrorIndexSettings.Type == ErrorIndexType.SqlExpress)
                ((SqlErrorIndex)m_ErrorIndex).SqlSettings = m_ContextSettings.SqlSettings;

            // Inform the email manager of the change in profile name if there is one.
            m_MailManager.ProfileName = m_ContextSettings.WinQualSettings.CompanyName;
                
            // Only stop and start the calendar tasks if the profile is currently active.
            if (m_IsActive)
            {
                // Recreate the script manager in case the debug params have changed.
                m_ScriptResultsManager =
                    new ScriptResultsManager(m_ErrorIndex, m_ScriptManager, m_DebuggerManager, m_ContextSettings.DebuggerSettings);

                // Restart the Sync timer with the new parameters.
                stopWinQualTimerSyncTask();
                startWinQualTimerSyncTask(m_ContextSettings);

                // Restart the purge task with the new settings.
                stopPurgeTimerTask();
                startPurgeTimerTask(m_ContextSettings);
            }
        }


        /// <summary>
        /// Creates an object to communicate to the WinQual service.
        /// More than one of these can exist.
        /// </summary>
        /// <returns></returns>
        private IWinQualServices makeNewWinQualServices()
        {
            // Create the new context.
            IWinQualServices winQualServices;
            if (m_IsTestMode && (m_TestData != null) && (m_TestData.DummyWinQualSettings.UseDummyWinQual))
                winQualServices = new DummyWinQualServices(m_TestData.DummyWinQualSettings);
            else
                winQualServices = new WinQualAtomFeedServices(
                    m_SettingsManager.ProxySettings, m_ContextSettings.WinQualSettings.RequestRetryCount, m_ContextSettings.WinQualSettings.RequestTimeout,
                    AppSettings.PullDateMinimumDuration, m_ContextSettings.WinQualSettings.MaxCabDownloadFailuresBeforeAbort, AppSettings.UseWindowsLiveId,
                    AppSettings.IntervalBetweenWinQualLogonsInHours);

            return winQualServices;
        }


        /// <summary>
        /// Sets test data used by the controller.
        /// </summary>
        /// <param name="testData">Test data used to create objects.</param>
        public void SetTestData(StackHashTestData testData)
        {
            m_TestData = testData;
        }


        /// <summary>
        /// Get all stored events.
        /// </summary>
        /// <param name="activeOnly">True - only check active profiles.</param>
        /// <param name="activeProductsOnly">True - only include active products. False - include all products.</param>
        public long GetTotalStoredEvents(bool activeOnly, bool activeProductsOnly)
        {
            long totalEvents = 0;

            Collection<int> activeProductIds = new Collection<int>();


            if (!activeOnly || m_IsActive)
            {
                if (m_ErrorIndex != null)
                {
                    if (activeProductsOnly)
                    {
                        StackHashProductCollection products = m_ErrorIndex.LoadProductList();

                        foreach (StackHashProduct product in products)
                        {
                            // Check if enabled for sync.
                            if (this.WinQualSettings.ProductsToSynchronize.FindProduct(product.Id) != null)
                            {
                                activeProductIds.Add(product.Id);
                            }
                        }

                        // Now get all product events in one go. Duplicate events across products will be ignored.
                        totalEvents = m_ErrorIndex.GetProductEventCount(activeProductIds);
                    }
                }
            }
            return totalEvents;
        }

        
        /// <summary>
        /// Synchronizes the local database with the WinQual service online.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="forceFullSynchronize">True - full sync, false - syncs from last successful sync time.</param>
        /// <param name="waitForCompletion">true - thread waits, false - returns immediately.</param>
        /// <param name="justSyncProducts">true - just sync the product list, false - sync according to producttosync</param>
        /// <param name="productsToSynchronize">List of products to sync - can be null.</param>
        /// <param name="isAutomatic">True - started by timer, false - started by user.</param>
        virtual public void RunSynchronizeTask(StackHashClientData clientData, bool forceFullSynchronize, bool waitForCompletion,
            bool justSyncProducts, StackHashProductSyncDataCollection productsToSynchronize, bool isTimedSync, bool isRetrySync)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            // Don't sync if a synchronize task is already running.
            if (m_TaskController.IsTaskRunning(StackHashTaskType.WinQualSynchronizeTask))
                throw new StackHashException("Win Qual synchronize already in progress", StackHashServiceErrorCode.TaskAlreadyInProgress);
            
            // Stop any AnalyzeTask that may be running. This will be restarted after the sync.
            m_TaskController.AbortTasksOfType(StackHashTaskType.AnalyzeTask, false, false);

            // An asynchronous task is created to perform the synchronization.
            WinQualSyncTaskParameters syncTaskParams = new WinQualSyncTaskParameters();
            syncTaskParams.ContextId = this.Id;
            syncTaskParams.ControllerContext = this;
            syncTaskParams.SettingsManager = m_SettingsManager;
            syncTaskParams.Name = "WinQualSyncTask_" + this.Id.ToString(CultureInfo.InvariantCulture);
            syncTaskParams.IsBackgroundTask = true;
            syncTaskParams.RunInParallel = true;
            syncTaskParams.UseSeparateThread = true;
            syncTaskParams.ClientData = clientData;
            syncTaskParams.ThisWinQualContext = new WinQualContext(makeNewWinQualServices());
            syncTaskParams.ErrorIndex = this.ErrorIndex;
            syncTaskParams.ForceFullSynchronize = forceFullSynchronize;
            syncTaskParams.TotalStoredEvents = (Controller.TheController != null) ? Controller.TheController.GetStoredEventsAcrossAllProfiles(true, true) : GetTotalStoredEvents(true, true);
            syncTaskParams.IsTimedSync = isTimedSync;
            syncTaskParams.IsSpecificProductSync = (productsToSynchronize != null);
            syncTaskParams.EnableNewProductsAutomatically = m_ContextSettings.WinQualSettings.EnableNewProductsAutomatically;

            if (productsToSynchronize != null)
                syncTaskParams.ProductsToSynchronize = productsToSynchronize;
            else
               syncTaskParams.ProductsToSynchronize = this.WinQualSettings.ProductsToSynchronize;

            syncTaskParams.JustSyncProductList = justSyncProducts;
            syncTaskParams.PurgeOptionsCollection = m_ContextSettings.PurgeOptionsCollection;
            syncTaskParams.WinQualSettings = m_ContextSettings.WinQualSettings;
            syncTaskParams.TheLicenseManager = m_LicenseManager;
            syncTaskParams.CollectionPolicy = m_ContextSettings.CollectionPolicy;
            syncTaskParams.IsRetry = isRetrySync;

            WinQualSyncTask syncTask = null;

            try
            {
                syncTask = new WinQualSyncTask(syncTaskParams);

                this.TaskController.RunConcurrentTask(syncTask as Task);

                if (waitForCompletion)
                    this.TaskController.WaitForTaskCompletion(syncTask as Task, Timeout.Infinite);
            }
            catch (System.Exception)
            {
                if (syncTask != null)
                    syncTask.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Uploads the specified data as a file to the web services.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="fileData">File data to send.</param>
        virtual public void RunUploadFileTask(StackHashClientData clientData, String fileData)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot upload mappings while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot upload mappings while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Cannot upload settings for an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            // Don't sync if a synchronize task is already running.
            if (m_TaskController.IsTaskRunning(StackHashTaskType.UploadFileTask))
                throw new StackHashException("File upload already in progress", StackHashServiceErrorCode.TaskAlreadyInProgress);


            // Store the file data in a file.
            String tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, fileData, Encoding.Unicode);

            // An asynchronous task is created to perform the synchronization.
            UploadFileTaskParameters uploadFileTaskParams = new UploadFileTaskParameters();
            uploadFileTaskParams.ContextId = this.Id;
            uploadFileTaskParams.ControllerContext = this;
            uploadFileTaskParams.SettingsManager = m_SettingsManager;
            uploadFileTaskParams.Name = "UploadFileTask_" + this.Id.ToString(CultureInfo.InvariantCulture);
            uploadFileTaskParams.IsBackgroundTask = true;
            uploadFileTaskParams.RunInParallel = true;
            uploadFileTaskParams.UseSeparateThread = true;
            uploadFileTaskParams.ClientData = clientData;
            uploadFileTaskParams.ThisWinQualContext = new WinQualContext(makeNewWinQualServices());
            uploadFileTaskParams.ErrorIndex = this.ErrorIndex;
            uploadFileTaskParams.FileName = tempFileName;
            uploadFileTaskParams.WinQualSettings = m_ContextSettings.WinQualSettings;

            UploadFileTask uploadFileTask = null;

            try
            {
                uploadFileTask = new UploadFileTask(uploadFileTaskParams);

                this.TaskController.RunConcurrentTask(uploadFileTask as Task);
            }
            catch (System.Exception)
            {
                if (uploadFileTask != null)
                    uploadFileTask.Dispose();
                throw;
            }
        }

        
        public void AbortTaskOfType(StackHashTaskType taskType)
        {
            AbortTaskOfType(taskType, false);
        }

        public void AbortTaskOfType(StackHashTaskType taskType, bool wait)
        {
            if (m_TaskController != null)
                m_TaskController.AbortTasksOfType(taskType, wait, true);
        }

        /// <summary>
        /// Downloads the specified cab from the WinQual site.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="product">The product to which the can refers.</param>
        /// <param name="file">The file to which the can refers.</param>
        /// <param name="theEvent">The event to which the can refers.</param>
        /// <param name="cab">The cab to be downloaded.</param>
        /// <param name="waitForCompletion">true - thread waits, false - returns immediately.</param>
        virtual public void RunDownloadCabTask(StackHashClientData clientData, StackHashProduct product, StackHashFile file, 
            StackHashEvent theEvent, StackHashCab cab, bool waitForCompletion)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            if (!m_ErrorIndex.ProductExists(product))
                throw new ArgumentException("Product does not exist");
            if (!m_ErrorIndex.FileExists(product, file))
                throw new ArgumentException("File does not exist");
            if (!m_ErrorIndex.EventExists(product, file, theEvent))
                throw new ArgumentException("Event does not exist");
            if (!m_ErrorIndex.CabExists(product, file, theEvent, cab))
                throw new ArgumentException("Cab does not exist");


            // An asynchronous task is created to perform the synchronization.
            DownloadCabTaskParameters downloadCabTaskParams = new DownloadCabTaskParameters();
            downloadCabTaskParams.ContextId = this.Id;
            downloadCabTaskParams.ControllerContext = this;
            downloadCabTaskParams.SettingsManager = m_SettingsManager;
            downloadCabTaskParams.UserName = this.WinQualSettings.UserName;
            downloadCabTaskParams.Password = this.WinQualSettings.Password;
            downloadCabTaskParams.Name = "DownloadCabTask_" + this.Id.ToString(CultureInfo.InvariantCulture) + "_" + cab.Id.ToString(CultureInfo.InvariantCulture);
            downloadCabTaskParams.IsBackgroundTask = true;
            downloadCabTaskParams.RunInParallel = true;
            downloadCabTaskParams.UseSeparateThread = true;
            downloadCabTaskParams.ClientData = clientData;
            downloadCabTaskParams.ThisWinQualContext = new WinQualContext(makeNewWinQualServices());
            downloadCabTaskParams.ErrorIndex = this.ErrorIndex;
            downloadCabTaskParams.Product = product;
            downloadCabTaskParams.File = file;
            downloadCabTaskParams.Event = theEvent;
            downloadCabTaskParams.Cab = cab;


            DownloadCabTask downloadCabTask = null;

            try
            {
                downloadCabTask = new DownloadCabTask(downloadCabTaskParams);

                this.TaskController.RunConcurrentTask(downloadCabTask as Task);

                if (waitForCompletion)
                    this.TaskController.WaitForTaskCompletion(downloadCabTask as Task, Timeout.Infinite);
            }
            catch (System.Exception)
            {
                if (downloadCabTask != null)
                    downloadCabTask.Dispose();
                throw;
            }
        }

        
        /// <summary>
        /// Purges old cabs (currently > 180 days) from the index.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        virtual public void RunPurgeTask(StackHashClientData clientData)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);
            if (m_TaskController.IsTaskRunning(StackHashTaskType.PurgeTask))
                throw new StackHashException("Purge already in progress", StackHashServiceErrorCode.TaskAlreadyInProgress);

            // An asynchronous task is created to perform the purge.
            PurgeTaskParameters purgeTaskParams = new PurgeTaskParameters();
            purgeTaskParams.ContextId = this.Id;
            purgeTaskParams.ControllerContext = this;
            purgeTaskParams.SettingsManager = m_SettingsManager;
            purgeTaskParams.Name = "PurgeTask_" + this.Id.ToString(CultureInfo.InvariantCulture);
            purgeTaskParams.IsBackgroundTask = true;
            purgeTaskParams.RunInParallel = true;
            purgeTaskParams.UseSeparateThread = true;
            purgeTaskParams.ClientData = clientData;
            purgeTaskParams.ErrorIndex = this.ErrorIndex;

            purgeTaskParams.PurgeOptions = m_ContextSettings.PurgeOptionsCollection;

            PurgeTask purgeTask = null;

            try
            {
                purgeTask = new PurgeTask(purgeTaskParams);

                this.TaskController.RunConcurrentTask(purgeTask as Task);
            }
            catch (System.Exception)
            {
                if (purgeTask != null)
                    purgeTask.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Processes cab files.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="forceRedo">True - full cab processing, false - only do new ones.</param>
        /// <param name="waitForCompletion">true - thread waits, false - returns immediately.</param>

        virtual public void RunAnalyzeTask(StackHashClientData clientData, bool forceRedo, bool waitForCompletion)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            // Only one Analyze task should be running.
            if (!m_IsActive)
                return;

            if (m_TaskController.IsTaskRunning(StackHashTaskType.AnalyzeTask))
                throw new StackHashException("Analyze task already running", StackHashServiceErrorCode.TaskAlreadyInProgress);

            // An asynchronous task is created to perform the cab processing.
            AnalyzeTaskParameters analyzeTaskParams = new AnalyzeTaskParameters();
            analyzeTaskParams.ContextId = this.Id;
            analyzeTaskParams.ControllerContext = this;
            analyzeTaskParams.SettingsManager = m_SettingsManager;
            analyzeTaskParams.Name = "AnalyzeTask_" + this.Id.ToString(CultureInfo.InvariantCulture);
            analyzeTaskParams.IsBackgroundTask = true;
            analyzeTaskParams.RunInParallel = true;
            analyzeTaskParams.UseSeparateThread = true;
            analyzeTaskParams.ClientData = clientData;
            analyzeTaskParams.AnalysisSettings = m_ContextSettings.AnalysisSettings;
            analyzeTaskParams.Debugger = new Windbg();
            analyzeTaskParams.DebuggerSettings = m_ContextSettings.DebuggerSettings;
            analyzeTaskParams.ForceUnpack = false;
            analyzeTaskParams.ErrorIndex = m_ErrorIndex;
            analyzeTaskParams.TheScriptManager = m_ScriptManager;
            analyzeTaskParams.TheScriptResultsManager = m_ScriptResultsManager;
            analyzeTaskParams.ProductsToSynchronize = this.WinQualSettings.ProductsToSynchronize;


            AnalyzeTask analyzeTask = null;
            try
            {
                analyzeTask = new AnalyzeTask(analyzeTaskParams);

                this.TaskController.RunConcurrentTask(analyzeTask as Task);

                if (waitForCompletion)
                    this.TaskController.WaitForTaskCompletion(analyzeTask as Task, Timeout.Infinite);
            }
            catch (System.Exception)
            {
                if (analyzeTask != null)
                    analyzeTask.Dispose();
                throw;
            }
        }

                
        /// <summary>
        /// Runs the specified scripts on the specified cab.
        /// </summary>
        /// <param name="product">The product owning the cab.</param>
        /// <param name="file">The file owning the cab.</param>
        /// <param name="theEvent">The event owning the cab.</param>
        /// <param name="cab">The cab to process.</param>
        /// <param name="dumpFileName">The dump filename - can be null.</param>
        /// <param name="scriptNames">List of scripts to run on the cab.</param>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="forceRedo">True - full cab processing, false - only do new ones.</param>
        /// <param name="waitForCompletion">true - thread waits, false - returns immediately.</param>

        virtual public void RunDebugScriptTask(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, 
            StackHashCab cab, String dumpFileName, StackHashScriptNamesCollection scriptNames, StackHashClientData clientData, 
            bool forceRedo, bool waitForCompletion)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Profile must be active to run debug script", StackHashServiceErrorCode.ProfileInactive);

            // An asynchronous task is created to perform the cab processing.
            DebugScriptTaskParameters debugScriptTaskParams = new DebugScriptTaskParameters();
            debugScriptTaskParams.ContextId = this.Id;
            debugScriptTaskParams.ControllerContext = this;
            debugScriptTaskParams.SettingsManager = m_SettingsManager;
            debugScriptTaskParams.Name = "DebugScriptTask_" + this.Id.ToString(CultureInfo.InvariantCulture);
            debugScriptTaskParams.IsBackgroundTask = true;
            debugScriptTaskParams.RunInParallel = true;
            debugScriptTaskParams.UseSeparateThread = true;
            debugScriptTaskParams.ClientData = clientData;
            debugScriptTaskParams.Debugger = new Windbg();
            debugScriptTaskParams.ErrorIndex = m_ErrorIndex;
            debugScriptTaskParams.TheScriptResultsManager = m_ScriptResultsManager;
            debugScriptTaskParams.TheScriptManager = m_ScriptManager;
            debugScriptTaskParams.DumpFileName = dumpFileName;
            debugScriptTaskParams.Product = product;
            debugScriptTaskParams.File = file;
            debugScriptTaskParams.TheEvent = theEvent;
            debugScriptTaskParams.Cab = cab;
            debugScriptTaskParams.ScriptsToRun = scriptNames;

            DebugScriptTask debugScriptTask = null;

            try
            {
                debugScriptTask = new DebugScriptTask(debugScriptTaskParams);

                this.TaskController.RunConcurrentTask(debugScriptTask as Task);

                if (waitForCompletion)
                    this.TaskController.WaitForTaskCompletion(debugScriptTask as Task, Timeout.Infinite);
            }
            catch (System.Exception)
            {
                if (debugScriptTask != null)
                    debugScriptTask.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Starts the bug tracker task - or pings it to start work if it is already running.
        /// This might be necessary to "restart" the task processing after it has stopped due to an exception 
        /// calling the bug tracker plugins.
        /// </summary>
        virtual public void RunBugTrackerTask()
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot run bug tracker task while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot run bug tracker task while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Profile must be active to run bug tracker task", StackHashServiceErrorCode.ProfileInactive);

            BugTrackerTask bugTrackerTask = m_TaskController.GetConcurrentTaskOfType(StackHashTaskType.BugTrackerTask) as BugTrackerTask;


            if (bugTrackerTask == null)
            {
                // The bug tracker task is not running so start it.
                startBugTrackerTask(m_ContextSettings);
            }
            else
            {
                // The bug tracker task is running. Ping it so it starts to process the outstanding Update Table entries.
                bugTrackerTask.CheckForUpdateEntries();
            }
        }


        /// <summary>
        /// Starts the bug reporting task. This task reports details of the specified products, files, events, cabs and scripts
        /// to the enabled bug tracker plugins.
        /// </summary>
        /// <param name="clientData">Client data of requestor or null.</param>
        /// <param name="bugReportDataCollection">Bug report to run.</param>
        /// <param name="plugIns">List of plug-ins to report to or null if all.</param>
        virtual public void RunBugReportTask(StackHashClientData clientData, StackHashBugReportDataCollection bugReportDataCollection,
            StackHashBugTrackerPlugInSelectionCollection plugIns)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot run bug tracker task while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot run bug tracker task while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (!m_IsActive)
                throw new StackHashException("Profile must be active to run bug tracker task", StackHashServiceErrorCode.ProfileInactive);

            if (bugReportDataCollection == null)
                throw new ArgumentNullException("bugReportDataCollection");


            // Only allow 1 report task to run at a time.

            // Get a list of the currently running BugReportTasks.
            List<Task> runningReportTasks = m_TaskController.GetTasksOfType(StackHashTaskType.BugReportTask);

            foreach (Task thisTask in runningReportTasks)
            {
                if (thisTask.CurrentTaskState.TaskCompleted == false)
                {
                    BugReportTask runningBugReportTask = thisTask as BugReportTask;

                    if (runningBugReportTask != null)
                    {
                        if (bugReportDataCollection.IsConflicting(runningBugReportTask.BugReportDataCollection))
                        {
                            throw new StackHashException("Bug report task already in progress", StackHashServiceErrorCode.TaskAlreadyInProgress);
                        }
                    }
                }
            }

            if (m_BugTrackerContext.NumberOfLoadedPlugIns == 0)
                throw new StackHashException("No bug trackers loaded for the profile", StackHashServiceErrorCode.BugTrackerNoPlugInsFound);

            BugReportTask bugReportTask = m_TaskController.GetConcurrentTaskOfType(StackHashTaskType.BugReportTask) as BugReportTask;

            BugReportTaskParameters bugReportTaskParameters = new BugReportTaskParameters();

            // Set standard task settings.
            bugReportTaskParameters.SettingsManager = m_SettingsManager;
            bugReportTaskParameters.IsBackgroundTask = true;
            bugReportTaskParameters.UseSeparateThread = true;
            bugReportTaskParameters.Name = "BugReportTask" + m_ContextSettings.Id.ToString(CultureInfo.InvariantCulture);
            bugReportTaskParameters.RunInParallel = true;
            bugReportTaskParameters.ContextId = this.Id;
            bugReportTaskParameters.ControllerContext = this;
            bugReportTaskParameters.ClientData = clientData;

            bugReportTaskParameters.ErrorIndex = m_ErrorIndex;
            bugReportTaskParameters.PlugInContext = m_BugTrackerContext;
            bugReportTaskParameters.PlugInManager = BugTrackerManager.CurrentBugTrackerManager;
            bugReportTaskParameters.TheScriptResultsManager = m_ScriptResultsManager;
            bugReportTaskParameters.ReportRequest = bugReportDataCollection;
            bugReportTaskParameters.PlugIns = plugIns;

            bugReportTask = null;

            try
            {
                bugReportTask = new BugReportTask(bugReportTaskParameters);
                this.TaskController.RunConcurrentTask(bugReportTask);
            }
            catch (System.Exception)
            {
                if (bugReportTask != null)
                    bugReportTask.Dispose();
                throw;
            }
        }


        
        /// <summary>
        /// Logs on to WinQual site. This is used primarily for testing the username and password.
        /// This is an asynchronous call. Listen for an admin task completion report to find out the result.
        /// </summary>
        /// <param name="clientData">Identifies the client making the request.</param>
        /// <param name="userName">WinQual username.</param>
        /// <param name="password">WinQual password.</param>
        public void RunWinQualLogOnTask(StackHashClientData clientData, String userName, String password)
        {
            // This can be run on an inactive profile.
            
            // Set up parameters to the LogOn task.
            WinQualLogOnTaskParameters logOnTaskParams = new WinQualLogOnTaskParameters();
            logOnTaskParams.ContextId = this.Id;
            logOnTaskParams.ControllerContext = this;
            logOnTaskParams.SettingsManager = m_SettingsManager;
            logOnTaskParams.Name = "WinQualLogOnTask";
            logOnTaskParams.IsBackgroundTask = true;
            logOnTaskParams.RunInParallel = true;
            logOnTaskParams.UseSeparateThread = true;
            logOnTaskParams.ClientData = clientData;
            logOnTaskParams.UserName = userName;
            logOnTaskParams.Password = password;
            logOnTaskParams.WinQualServicesObject = makeNewWinQualServices();
            logOnTaskParams.ErrorIndex = m_ErrorIndex;

            // Create and queue the logon task.
            WinQualLogOnTask logOnTask = null;

            try
            {
                logOnTask = new WinQualLogOnTask(logOnTaskParams);

                this.TaskController.RunConcurrentTask(logOnTask as Task);
            }
            catch (System.Exception)
            {
                if (logOnTask != null)
                    logOnTask.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Testing only - creates a test index with the specified number of products, files, events and cabs.
        /// </summary>
        /// <param name="testData">Specifies the content of the test index.</param>
        public void CreateTestIndex(StackHashTestIndexData testData)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            if (testData == null)
                throw new ArgumentNullException("testData");

            // Call the test manager to perform the action.
            TestManager.CreateTestIndex(m_ErrorIndex, testData, testData.DuplicateFileIdsAcrossProducts);
        }

        /// <summary>
        /// Testing only - creates a test index with the specified number of products, files, events and cabs.
        /// Also includes duplicate files and events if requested.
        /// </summary>
        /// <param name="testData">Specifies the content of the test index.</param>
        public void CreateTestIndex(StackHashTestIndexData testData, bool includeDuplicates)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);


            // Call the test manager to perform the action.
            TestManager.CreateTestIndex(m_ErrorIndex, testData, includeDuplicates);
        }

        
        /// <summary>
        /// Deletes the specified error index.
        /// </summary>
        public void DeleteIndex()
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);

            m_ErrorIndex.DeleteIndex();

            // Check the status of the index. The activate should have created the index.
            if (m_ErrorIndex.Status != m_ContextSettings.ErrorIndexSettings.Status)
            {
                // Make sure the status is recorded for the context.
                m_ContextSettings.ErrorIndexSettings.Status = m_ErrorIndex.Status;
                m_SettingsManager.SetContextSettings(m_ContextSettings, true);
            }
        }


        /// <summary>
        /// Moves the index to the specified location. Note that if the folder exists an error will occur.
        /// If only the name has changed then this amounts to a folder rename.
        /// Must be inactive to perform this action.
        /// </summary>
        /// <param name="clientData">Client data - used in task completion callback.</param>
        /// <param name="newErrorIndexPath">Root path for the folder.</param>
        /// <param name="newErrorIndexName">New name of the index.</param>
        /// <param name="newSqlSettings">Can be null if not an Sql index.</param>
        public void RunMoveIndexTask(StackHashClientData clientData, String newErrorIndexPath, String newErrorIndexName, 
            StackHashSqlConfiguration newSqlSettings)
        {
            if (m_IsActive)
                throw new StackHashException("Cannot move an index while profile is active", StackHashServiceErrorCode.ProfileActive);
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);



            // Start the move index task.
            ErrorIndexMoveTaskParameters moveParams = new ErrorIndexMoveTaskParameters();
            moveParams.ContextId = this.Id;
            moveParams.ControllerContext = this;
            moveParams.SettingsManager = m_SettingsManager;
            moveParams.ClientData = clientData;
            moveParams.ErrorIndex = m_ErrorIndex;
            moveParams.IsBackgroundTask = true;
            moveParams.Name = "MoveIndex_" + m_Id.ToString(CultureInfo.InvariantCulture);
            moveParams.NewErrorIndexPath = newErrorIndexPath;
            moveParams.NewErrorIndexName = newErrorIndexName;
            moveParams.NewSqlSettings = newSqlSettings;
            moveParams.RunInParallel = true;
            moveParams.UseSeparateThread = true;
            moveParams.IntervalBetweenProgressReportsInSeconds = AppSettings.IntervalBetweenProgressReportsInSeconds;

            ErrorIndexMoveTask moveTask = null;
            try
            {
                moveTask = new ErrorIndexMoveTask(moveParams);
                m_MoveInProgress = true;
                m_TaskController.Enqueue(moveTask);
            }
            catch
            {
                m_MoveInProgress = false;
                if (moveTask != null)
                    moveTask.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Copies the current index to the specified location. 
        /// The destination index will be created and the whole source index will be copied to it.
        /// </summary>
        /// <param name="clientData">Client data - used in task completion callback.</param>
        /// <param name="destinationIndexSettings">Destination index settings.</param>
        /// <param name="sqlConfiguration">Sql settings - can be null.</param>
        /// <param name="assignCopyToContext">True - makes the copy the new index for the context.</param>
        /// <param name="destinationIndexSettings">True - delete the original index if successful.</param>
        public void RunCopyIndexTask(StackHashClientData clientData, ErrorIndexSettings destinationIndexSettings,
            StackHashSqlConfiguration sqlConfiguration, bool assignCopyToContext, bool deleteSourceIndexWhenComplete)
        {
            if (m_IsActive)
                throw new StackHashException("Cannot copy an index while profile is active", StackHashServiceErrorCode.ProfileInactive);
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (deleteSourceIndexWhenComplete && !assignCopyToContext)
                throw new StackHashException("Can only delete old index if a switch takes place too", StackHashServiceErrorCode.CopyIndexDeleteButNoSwitch);

            if (clientData == null)
                throw new ArgumentNullException("clientData");

            if (destinationIndexSettings == null)
                throw new ArgumentNullException("destinationIndexSettings");

            // Create the destination index.
            IErrorIndex destinationIndex;

            if (destinationIndexSettings.Type == ErrorIndexType.Xml)
            {
                destinationIndex = new XmlErrorIndex(destinationIndexSettings.Folder, destinationIndexSettings.Name);
            }
            else if (destinationIndexSettings.Type == ErrorIndexType.SqlExpress)
            {
                if (sqlConfiguration == null)
                    throw new ArgumentNullException("sqlConfiguration");

                destinationIndex = new SqlErrorIndex(sqlConfiguration, destinationIndexSettings.Name, destinationIndexSettings.Folder);
            }
            else
            {
                throw new NotSupportedException("Index type not supported");
            }
            
            // Activate and deactivate the destination to make sure it exists.
            destinationIndex.Activate();
            destinationIndex.Deactivate();

            // Start the copy index task.
            ErrorIndexCopyTaskParameters copyParams = new ErrorIndexCopyTaskParameters();
            copyParams.ContextId = this.Id;
            copyParams.ControllerContext = this;
            copyParams.SettingsManager = m_SettingsManager;
            copyParams.ClientData = clientData;
            copyParams.ErrorIndex = m_ErrorIndex;
            copyParams.IsBackgroundTask = true;
            copyParams.Name = "CopyIndex_" + m_Id.ToString(CultureInfo.InvariantCulture);
            copyParams.RunInParallel = true;
            copyParams.UseSeparateThread = true;

            copyParams.SourceIndex = m_ErrorIndex;
            copyParams.DestinationIndex = destinationIndex;
            copyParams.AssignCopyToContext = assignCopyToContext;
            copyParams.DeleteSourceIndexWhenComplete = deleteSourceIndexWhenComplete;
            copyParams.DestinationErrorIndexSettings = destinationIndexSettings;
            copyParams.DestinationSqlSettings = sqlConfiguration;
            copyParams.EventsPerBlock = AppSettings.CopyIndexEventsPerBlock;

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Copying index with block size " + copyParams.EventsPerBlock.ToString(CultureInfo.InvariantCulture));

            ErrorIndexCopyTask copyTask = null;
            try
            {
                copyTask = new ErrorIndexCopyTask(copyParams);
                m_CopyInProgress = true;
                m_TaskController.Enqueue(copyTask);
            }
            catch
            {
                m_CopyInProgress = false;
                if (copyTask != null)
                    copyTask.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Set the data collection policy. This will merge or replace existing policy records unless setAll is specified
        /// in which case the entire collection will be replaced.
        /// </summary>
        /// <param name="policyCollection">A collection of data collection policies.</param>
        /// <param name="setAll">true - Sets the entire structure, false - merges</param>
        public void SetDataCollectionPolicy(StackHashCollectionPolicyCollection policyCollection, bool setAll)
        {
            if (policyCollection == null)
                throw new ArgumentNullException("policyCollection");

            m_SettingsManager.SetDataCollectionPolicy(this.Id, policyCollection, setAll);
            m_ContextSettings = m_SettingsManager.GetContextSettings(m_ContextSettings.Id);
        }


        /// <summary>
        /// Gets the data collection policy for the specified object.
        /// </summary>
        /// <param name="policyObject">Global, Product, File, Event or Cab.</param>
        /// <param name="id">Id of the object to get.</param>
        /// <param name="conditionObject">The object to which the condition applies.</param>
        /// <param name="objectToCollect">The type of object being collected.</param>
        /// <param name="getAll">True - gets all policies, false - gets individual policy.</param>
        /// <returns>Individual policy or all policities as requested.</returns>
        public StackHashCollectionPolicyCollection GetDataCollectionPolicy(StackHashCollectionObject policyObject, int id, 
            StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect, bool getAll)
        {
            return m_SettingsManager.GetDataCollectionPolicy(this.Id, policyObject, id, conditionObject, objectToCollect, getAll);
        }

        /// <summary>
        /// Gets the active data collection policy for the specified object.
        /// </summary>
        /// <param name="productId">ID of product or 0.</param>
        /// <param name="fileId">ID of file or 0.</param>
        /// <param name="eventId">ID of event or 0.</param>
        /// <param name="cabId">ID of cab or 0.</param>
        /// <param name="objectToCollect">Object being collected.</param>
        /// <returns>Prioritized policy.</returns>
        public StackHashCollectionPolicyCollection GetActiveDataCollectionPolicy(int productId, int fileId, int eventId, int cabId, StackHashCollectionObject objectToCollect)
        {
            return m_SettingsManager.GetActiveDataCollectionPolicy(this.Id, productId, fileId, eventId, cabId, objectToCollect);
        }

        /// <summary>
        /// Removes the specified policy.
        /// </summary>
        /// <param name="rootObject">Global, Product, File, Event or Cab.</param>
        /// <param name="id">Id of the object to get.</param>
        /// <param name="conditionObject">Object to which the condition refers.</param>
        /// <param name="objectToCollect">Object being collected.</param>
        public void RemoveDataCollectionPolicy(StackHashCollectionObject rootObject, int id, 
            StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect)
        {
            m_SettingsManager.RemoveDataCollectionPolicy(this.Id, rootObject, id, conditionObject, objectToCollect);
        }

        /// <summary>
        /// Sets the purge options and schedule for the context.
        /// </summary>
        /// <param name="purgeSchedule">When the automatic purge is to take place.</param>
        /// <param name="purgeOptions">What is to be purged</param>
        /// <param name="setAll">True - replace, false - individual.</param>
        public void SetPurgeOptions(ScheduleCollection purgeSchedule, StackHashPurgeOptionsCollection purgeOptions, bool setAll)
        {
            m_SettingsManager.SetPurgeOptions(m_ContextSettings.Id, purgeSchedule, purgeOptions, setAll);
            m_ContextSettings = m_SettingsManager.GetContextSettings(m_ContextSettings.Id);

            // Now restart the purge timer task.
            stopPurgeTimerTask();
            startPurgeTimerTask(m_ContextSettings);
        }


        /// <summary>
        /// Gets the purge options for the specified context and object type.
        /// </summary>
        /// <param name="purgeObject">Object to get the settings for.</param>
        /// <param name="id">Id of the object.</param>
        /// <param name="getAll">True - gets all purge options, false - gets individual purge option.</param>
        /// <returns>List of matching purge options.</returns>
        public StackHashPurgeOptionsCollection GetPurgeOptions(StackHashPurgeObject purgeObject, int id, bool getAll)
        {
            return m_SettingsManager.GetPurgeOptions(m_ContextSettings.Id, purgeObject, id, getAll);
        }


        /// <summary>
        /// Gets the active purge options for the specified object.
        /// </summary>
        /// <param name="productId">ID of product or 0.</param>
        /// <param name="fileId">ID of file or 0.</param>
        /// <param name="eventId">ID of event or 0.</param>
        /// <param name="cabId">ID of cab or 0.</param>
        /// <returns>Prioritized options.</returns>
        public StackHashPurgeOptionsCollection GetActivePurgeOptions(int productId, int fileId, int eventId, int cabId)
        {
            return m_SettingsManager.GetActivePurgeOptions(m_ContextSettings.Id, productId, fileId, eventId, cabId);
        }

        
        /// <summary>
        /// Removes the purge options for the specified context and object type.
        /// </summary>
        /// <param name="purgeObject">Object to remove.</param>
        /// <param name="id">Id of the object.</param>
        /// <returns>List of matching purge options.</returns>
        public void RemovePurgeOptions(StackHashPurgeObject purgeObject, int id)
        {
            m_SettingsManager.RemovePurgeOptions(m_ContextSettings.Id, purgeObject, id);
        }

        
        /// <summary>
        /// Enables or disables the product for synchronization.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="enabled"></param>
        public void SetProductWinQualState(int productId, bool enabled)
        {
            m_SettingsManager.SetProductSynchronization(m_ContextSettings.Id, productId, enabled);
            m_ContextSettings = m_SettingsManager.GetContextSettings(m_ContextSettings.Id);
        }


        /// <summary>
        /// Sets the product sync state - e.g. whether it is enabled, disabled and how many
        /// cabs can be downloaded.
        /// </summary>
        /// <param name="productId">Id of product.</param>
        /// <param name="productSyncState">New state.</param>
        public void SetProductSyncData(StackHashProductSyncData productSyncState)
        {
            m_SettingsManager.SetProductSyncData(m_ContextSettings.Id, productSyncState);
            m_ContextSettings = m_SettingsManager.GetContextSettings(m_ContextSettings.Id);
        }


        /// <summary>
        /// Set the email notification settings for the specified context.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="emailSettings">New email settings</param>
        public void SetEmailSettings(StackHashEmailSettings emailSettings)
        {
            m_SettingsManager.SetEmailSettings(m_ContextSettings.Id, emailSettings);
            m_ContextSettings = m_SettingsManager.GetContextSettings(m_ContextSettings.Id);
            m_MailManager.SetEmailSettings(emailSettings);
        }


        /// <summary>
        /// Sets the bugId field in the specified event.
        /// </summary>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to set.</param>
        /// <param name="bugId">The bug ID to set.</param>
        public void SetEventBugId(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, String bugId)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Read the current state of the event.
            StackHashEvent newEvent = ErrorIndex.GetEvent(product, file, theEvent);

            newEvent.BugId = bugId;

            // Replace with new bug Id.
            m_ErrorIndex.AddEvent(product, file, newEvent, true);   // Update non winqual fields too.         
        }

        /// <summary>
        /// Sets the WorkFlowStatusId field in the specified event.
        /// </summary>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to set.</param>
        /// <param name="workFlowStatus">The WorkFlowStatus to set.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public void SetWorkFlowStatus(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int workFlowStatus)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot set work flow status while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot set work flow status while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot set work flow status an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Read the current state of the event.
            StackHashEvent newEvent = ErrorIndex.GetEvent(product, file, theEvent);

            newEvent.WorkFlowStatus = workFlowStatus;

            // Replace with new bug Id.
            m_ErrorIndex.AddEvent(product, file, newEvent, true);   // Update non winqual fields too.         
        }



        /// <summary>
        /// Use -2 to get the last time the winqual site was updated.
        /// This is set during a product sync.
        /// </summary>
        /// <returns>Last date the product was updated.</returns>
        public DateTime GetLastProductSyncTime(int productId)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot get last update time on an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            return m_ErrorIndex.GetLastSyncTimeLocal(productId); // Special entry -2 in the product sync data table for the last site update time.
        }

        /// <summary>
        /// Get a list of products associated with this context.
        /// </summary>
        /// <returns>Product data for the context.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashProductInfoCollection GetProducts()
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot get products on an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            StackHashProductCollection products = m_ErrorIndex.LoadProductList();

            // Now for each product get its sync enabled state.
            StackHashProductInfoCollection productInfos = new StackHashProductInfoCollection();

            foreach (StackHashProduct product in products)
            {
                bool isSyncEnabled = m_SettingsManager.GetProductSynchronization(this.Id, product.Id);

                StackHashProductSyncData productSyncData = null;
                if (isSyncEnabled)
                    productSyncData = m_SettingsManager.GetProductSyncData(this.Id, product.Id);

                DateTime lastSyncTime = new DateTime(0, DateTimeKind.Utc);
                DateTime lastSyncCompletedTime = new DateTime(0, DateTimeKind.Utc);
                DateTime lastSyncStartedTime = new DateTime(0, DateTimeKind.Utc);

                // Convert the sync time to UTC - this allows the client to display it in local time.
                if (m_ErrorIndex != null)
                {
                    lastSyncTime = m_ErrorIndex.GetLastSyncTimeLocal(product.Id).ToUniversalTime();
                    lastSyncCompletedTime = m_ErrorIndex.GetLastSyncCompletedTimeLocal(product.Id).ToUniversalTime();
                    lastSyncStartedTime = m_ErrorIndex.GetLastSyncStartedTimeLocal(product.Id).ToUniversalTime();
                }

                productInfos.Add(new StackHashProductInfo(product, isSyncEnabled, lastSyncTime, productSyncData, lastSyncCompletedTime, lastSyncStartedTime));
            }

            return productInfos;
        }


        /// <summary>
        /// Get a list of files associated with the specified product.
        /// </summary>
        /// <param name="product">The product for which the file list is required.</param>
        public StackHashFileCollection GetFiles(StackHashProduct product)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (product == null)
                throw new ArgumentNullException("product");

            return m_ErrorIndex.LoadFileList(product);
        }

        
        /// <summary>
        /// Get a list of events associated with the specified product file.
        /// </summary>
        /// <param name="product">The product for which the file refers.</param>
        /// <param name="file">The file whos event list is required.</param>
        public StackHashEventCollection GetEvents(StackHashProduct product, StackHashFile file)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (product == null)
                throw new ArgumentNullException("product");

            if (file == null)
                throw new ArgumentNullException("file");

            return m_ErrorIndex.LoadEventList(product, file);
        }


        /// <summary>
        /// Get a list of all events associated with a product.
        /// </summary>
        /// <param name="product">The product for which events are required.</param>
        public StackHashEventPackageCollection GetProductEvents(StackHashProduct product)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (product == null)
                throw new ArgumentNullException("product");

            return m_ErrorIndex.GetProductEvents(product);
        }

        
        /// <summary>
        /// Get a list of event data associated with the specified event.
        /// </summary>
        /// <param name="product">The product for which the file refers.</param>
        /// <param name="file">The file whos event list is required.</param>
        /// <param name="theEvent">Event for which the data is required.</param>
        public StackHashEventPackage GetEventPackage(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (product == null)
                throw new ArgumentNullException("product");

            if (file == null)
                throw new ArgumentNullException("file");

            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Refresh the event package.
            StackHashEvent refreshedEvent = m_ErrorIndex.GetEvent(product, file, theEvent);

            StackHashEventInfoCollection eventCollection = m_ErrorIndex.LoadEventInfoList(product, file, refreshedEvent);

            StackHashCabCollection cabCollection = m_ErrorIndex.LoadCabList(product, file, refreshedEvent);

            StackHashEventPackage eventPackage = new StackHashEventPackage(eventCollection, new StackHashCabPackageCollection(cabCollection), refreshedEvent, product.Id);

            populateCabContents(eventPackage);

            return eventPackage;
        }

        /// <summary>
        /// Gets the specified CAB file.
        /// Note that the caller is responsible for closing the stream.
        /// </summary>
        /// <param name="product">The product for which the file refers.</param>
        /// <param name="file">The file whos event list is required.</param>
        /// <param name="theEvent">Event for which the data is required.</param>
        /// <param name="cab">Cab for which the file is required.</param>
        /// <param name="fileName">Name of the file or null if cab itself is to be returned.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public Stream GetCabFile(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String fileName)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (product == null)
                throw new ArgumentNullException("product");

            if (file == null)
                throw new ArgumentNullException("file");

            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (cab == null)
                throw new ArgumentNullException("cab");


            String cabFileName = m_ErrorIndex.GetCabFileName(product, file, theEvent, cab);

            if (!File.Exists(cabFileName))
            {
                // Set the downloaded flag appropriately if different.
                StackHashCab loadedCab = m_ErrorIndex.GetCab(product, file, theEvent, cab.Id);

                if (loadedCab != null)
                {
                    if (loadedCab.CabDownloaded)
                    {
                        loadedCab.CabDownloaded = false;
                        m_ErrorIndex.AddCab(product, file, theEvent, loadedCab, false);
                    }
                }
                throw new StackHashException("Cab file not downloaded", StackHashServiceErrorCode.CabDoesNotExist);
            }

            String fileToStream = null;
            if (String.IsNullOrEmpty(fileName))
            {
                fileToStream = cabFileName;
            }
            else
            {
                // Stream the specific file within the cab to the client.
                String cabFolder = m_ErrorIndex.GetCabFolder(product, file, theEvent, cab);

                fileToStream = Path.Combine(cabFolder, fileName);
                fileToStream = Path.GetFullPath(fileToStream);

                if (!fileToStream.StartsWith(cabFolder, StringComparison.OrdinalIgnoreCase))
                {
                    // Perhaps an attempt to open files not in in the cab folder.
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Attempt to download file: " + fileToStream);
                    throw new ArgumentException("Filename must be in cab folder", "fileName");
                }

                if (!File.Exists(fileToStream))
                {
                    // Check if the file is inside the cab.
                    StackHashCabFileContents cabFileContents = getCabFileContents(cabFileName);

                    bool matchingFileFound = false;
                    foreach (StackHashCabFile cabFile in cabFileContents.Files)
                    {
                        if (String.Compare(fileName, cabFile.FileName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            matchingFileFound = true;
                            break;
                        }
                    }

                    if (!matchingFileFound)
                        throw new ArgumentException("File not found in cab: " + fileName, "fileName");

                    // Extract the file from the cab.
                    Cabs.ExtractCab(cabFileName, cabFolder);

                    if (!File.Exists(fileToStream))
                        throw new ArgumentException("Failed to extract file from cab: " + fileName, "fileName");
                }
            }
            // Stream the entire cab file to the client.
            FileStream fileStream = null;

            try
            {
                fileStream = File.Open(fileToStream, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch
            {
                if (fileStream != null)
                    fileStream.Close();
                fileStream = null;
            }
            return fileStream;

        }


        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="searchCriteria">Specifies what to search.</param>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteria)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            StackHashEventPackageCollection eventCollection = m_ErrorIndex.GetEvents(searchCriteria, this.WinQualSettings.ProductsToSynchronize);

            return eventCollection;
        }


        /// <summary>
        /// Checks if the specified cab contains script results matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">Cab whose scripts are to be examined.</param>
        /// <returns>True - cab contains matching script, False - no matching script.</returns>
        private bool doesCabMatchCriteria(StackHashSearchCriteria searchCriteria, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            // Should really check the objects match the criteria here as well as the script.
            if (!searchCriteria.ContainsObject(StackHashObjectType.Script))
                return true;

            return m_ScriptResultsManager.CabMatchesSearchCriteria(product, file, theEvent, cab, searchCriteria);
        }


        
        /// <summary>
        /// Removes an cabs that do not match the script search criteria.
        /// </summary>
        /// <param name="searchCriteria">Script search criteria.</param>
        /// <param name="eventPackage">Event package to check.</param>
        /// <returns>True - still contains matching cabs. False - no matching cabs.</returns>
        private bool removeUnmatchingCabs(StackHashSearchCriteria searchCriteria, StackHashEventPackage eventPackage)
        {
            // TODO: Should really check the objects match the criteria here as well as the script.
            if (!searchCriteria.ContainsObject(StackHashObjectType.Script))
                return true;

            // No cabs so scripts can't have been run - so is not a match.
            if ((eventPackage.Cabs == null) || (eventPackage.Cabs.Count == 0))
                return false;

            StackHashProduct product = m_ErrorIndex.GetProduct(eventPackage.ProductId);
            StackHashFile file = m_ErrorIndex.GetFile(product, eventPackage.EventData.FileId);

            StackHashCabPackageCollection cabsToRemove = new StackHashCabPackageCollection();

            foreach (StackHashCabPackage cab in eventPackage.Cabs)
            {
                if (!doesCabMatchCriteria(searchCriteria, product, file, eventPackage.EventData, cab.Cab))
                {
                    cabsToRemove.Add(cab);
                }
            }

            // Remove the cabs.
            foreach (StackHashCabPackage cab in cabsToRemove)
            {
                eventPackage.Cabs.Remove(cab);
            }

            // If no cabs left then remove the event.
            if (eventPackage.Cabs.Count == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Removes an cabs that do not match the script search criteria.
        /// </summary>
        /// <param name="allSearchCriteria">Script search criteria.</param>
        /// <param name="eventPackage">Event package to check.</param>
        /// <returns>True - still contains matching cabs. False - no matching cabs.</returns>
        private bool removeUnmatchingCabs(StackHashSearchCriteriaCollection allSearchCriteria, StackHashEventPackage eventPackage)
        {
            if (allSearchCriteria.ObjectCount(StackHashObjectType.Script) == 0)
                return true;

            //// No cabs so scripts can't have been run - so is not a match.
            //if ((eventPackage.Cabs == null) || (eventPackage.Cabs.Count == 0))
            //    return false;

            StackHashProduct product = null;
            StackHashFile file = null;

            if (m_ErrorIndex.IndexType == ErrorIndexType.Xml)
            {
                product = m_ErrorIndex.GetProduct(eventPackage.ProductId);
                file = m_ErrorIndex.GetFile(product, eventPackage.EventData.FileId);
            }
            else
            {
                // These are not actually needed to get the cab data from an SQL index so don't issue 
                // unnecessary calls to the database.
                product = new StackHashProduct();
                product.Id = eventPackage.ProductId;

                file = new StackHashFile();
                file.Id = eventPackage.EventData.FileId;
            }

            // Assume none of the cabs match.
            foreach (StackHashCabPackage cab in eventPackage.Cabs)
            {
                cab.IsSearchMatch = false;
            }


            int criteriaNumber = 0;
            int numberOfScriptMatches = 0;

            foreach (StackHashSearchCriteria searchCriteria in allSearchCriteria)
            {
                if (criteriaNumber == 0)
                    criteriaNumber = 1;
                else
                    criteriaNumber <<= 1;

                if ((eventPackage.CriteriaMatchMap & criteriaNumber) == 0)
                {
                    // Didn't match the rest of the criteria search options so must have matched a different criteria.
                    continue;
                }

                // If there is a criteria with no script search options then all the cabs are considered a match.
                if (!searchCriteria.ContainsObject(StackHashObjectType.Script))
                {
                    foreach (StackHashCabPackage cab in eventPackage.Cabs)
                    {
                        cab.IsSearchMatch = true;
                    }
                    numberOfScriptMatches++;
                    break;
                }


                // Matched the rest of the criteria so see if the script option matches too.
                foreach (StackHashCabPackage cab in eventPackage.Cabs)
                {
                    if (doesCabMatchCriteria(searchCriteria, product, file, eventPackage.EventData, cab.Cab))
                    {
                        cab.IsSearchMatch = true;
                        numberOfScriptMatches++;
                    }
                }
            }

            return (numberOfScriptMatches > 0);
        }


        /// <summary>
        /// Removes events that have no matching script files.
        /// </summary>
        /// <param name="allSearchCriteria">Script criteria.</param>
        /// <param name="allEvents">Event list to search.</param>
        /// <returns>List of events matching the script search criteria.</returns>
        private StackHashEventPackageCollection removeEventsAndCabsThatDoNotMatchScriptSearchCriteria(
            StackHashSearchCriteriaCollection allSearchCriteria, StackHashEventPackageCollection allEvents)
        {
            // If no script criteria then just return the original list.
            if (allSearchCriteria.ObjectCount(StackHashObjectType.Script) == 0)
                return allEvents;

            StackHashEventPackageCollection matchingEvents = new StackHashEventPackageCollection();

            foreach (StackHashEventPackage eventPackage in allEvents)
            {
                // This call removes unwanted cabs and returns true if any remain.
                if (removeUnmatchingCabs(allSearchCriteria, eventPackage))
                    matchingEvents.Add(eventPackage);
            }

            return matchingEvents;
        }


        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="searchCriteria">Specifies what to search.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        public StackHashEventPackageCollection GetEvents(
            StackHashSearchCriteriaCollection searchCriteria, long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder) 
        {
            return GetEvents(searchCriteria, startRow, numberOfRows, sortOrder, StackHashSearchDirection.Forwards, false);
        }


        /// <summary>
        /// Gets the list of files contained in a cab.
        /// </summary>
        /// <param name="cabFileName">Cab file to examine.</param>
        /// <returns>Contents of the cab.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private StackHashCabFileContents getCabFileContents(String cabFileName)
        {
            StackHashCabFileContents cabFileContents = new StackHashCabFileContents();

            try
            {
                if (File.Exists(cabFileName))
                {
                    Collection<CabinetFileInfo> cabFiles = Cabs.GetCabFiles(cabFileName);

                    foreach (CabinetFileInfo cabFile in cabFiles)
                    {
                        cabFileContents.Files.Add(new StackHashCabFile(cabFile.RelativePath, cabFile.Size));
                    }
                }
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to get cab file list from cab: " + cabFileName, ex);
            }

            return cabFileContents;
        }


        /// <summary>
        /// Populates the specified event package with the cab contents data.
        /// </summary>
        /// <param name="eventPackage">Event package to populate.</param>
        private void populateCabContents(StackHashEventPackage eventPackage)
        {
            if (eventPackage == null)
                return;

            if (eventPackage.Cabs == null)
                return;

            foreach (StackHashCabPackage cabPackage in eventPackage.Cabs)
            {
                StackHashProduct product = new StackHashProduct() { Id = eventPackage.ProductId };
                StackHashFile file = new StackHashFile() { Id = eventPackage.EventData.FileId };

                if (m_ErrorIndex.IndexType == ErrorIndexType.Xml)
                {
                    // For the xml index the full product and file names are required.
                    product = m_ErrorIndex.GetProduct(eventPackage.ProductId);
                    file = m_ErrorIndex.GetFile(product, eventPackage.EventData.FileId);
                }

                cabPackage.FullPath = m_ErrorIndex.GetCabFileName(product, file, eventPackage.EventData, cabPackage.Cab);
                cabPackage.CabFileContents = getCabFileContents(cabPackage.FullPath);
            }
        }


        /// <summary>
        /// Populates the collection of event packages with the contents of each cab therein.
        /// </summary>
        /// <param name="allEvents">Event packages to populate.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private void populateCabContents(StackHashEventPackageCollection allEvents)
        {
            foreach (StackHashEventPackage eventPackage in allEvents)
            {
                populateCabContents(eventPackage);
            }
        }

        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="searchCriteria">Specifies what to search.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <param name="searchDirection">Forwards or backwards.</param>
        /// <param name="countAllMatches">True - counts all matches as well as returning the first window.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public StackHashEventPackageCollection GetEvents(
            StackHashSearchCriteriaCollection searchCriteria, long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder,
            StackHashSearchDirection searchDirection, bool countAllMatches)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot get events while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot get events while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot get events on inactive profile", StackHashServiceErrorCode.ProfileInactive);
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");
            if (sortOrder == null)
                throw new ArgumentNullException("sortOrder");
            if (sortOrder.Count == 0)
                throw new ArgumentException("Must specify a sort order", "sortOrder");

            // Reset any out of range values.
            if (startRow > Int32.MaxValue)
                startRow = Int32.MaxValue;
            if (numberOfRows > Int32.MaxValue)
                numberOfRows = Int32.MaxValue;
            if (startRow < 1)
                startRow = 1;
            
            // Just get one load of events regardless of how many have been requested.
            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            bool noMoreEvents = false;
            long currentStartRow = startRow;
            long maximumSqlRows = 0;


            // Don't allow a get of more than say 5000 events at a time. If the user has asked for more then 
            // this is probably because he wants to count the events as opposed to return them.
            // We don't want to exhaust memory loading too many at once. This will force the get in blocks.
            long rowsToGetAtATime = numberOfRows;
            if (rowsToGetAtATime > m_MaxRowsToGetPerRequest)
                rowsToGetAtATime = m_MaxRowsToGetPerRequest;

            while (!noMoreEvents && (allEvents.TotalRows < numberOfRows))
            {
                StackHashEventPackageCollection nextEvents = m_ErrorIndex.GetEvents(searchCriteria, currentStartRow, rowsToGetAtATime, 
                    sortOrder, this.WinQualSettings.ProductsToSynchronize);

                // Just record the number of rows in the search. This is not the number of rows returned and will stay constant for all calls in this loop.
                if (nextEvents.MaximumSqlRows != 0)
                    maximumSqlRows = nextEvents.MaximumSqlRows;

                // If not all the requested events were returned from the index (searching forward) then it must have run out of matches.
                if ((searchDirection == StackHashSearchDirection.Forwards) && (nextEvents.Count < rowsToGetAtATime))
                    noMoreEvents = true;

                // If searching backwards and there we reached row 1 then we are at the start of the matching events so there are no more to read.
                if ((searchDirection == StackHashSearchDirection.Backwards) && (currentStartRow == 1))
                    noMoreEvents = true;

                // Filter out events that don't match the Script search criteria (if there is one).
                nextEvents = removeEventsAndCabsThatDoNotMatchScriptSearchCriteria(searchCriteria, nextEvents);

                // How many events are required to fill the requested quota.
                long eventsRemaining = numberOfRows - allEvents.Count;

                if (searchDirection == StackHashSearchDirection.Forwards)
                {
                    // Add the events to the end to maintain sort order.
                    allEvents = allEvents.MergeEvents(nextEvents, true, eventsRemaining);

                    currentStartRow += rowsToGetAtATime;
                }
                else
                {
                    // Add the events to the start to maintain sort order.
                    allEvents = allEvents.MergeEvents(nextEvents, false, eventsRemaining);

                    // The caller may have specified Int64.MaxValue for the last row if they didn't know how many rows there were.
                    if (currentStartRow == Int32.MaxValue)
                        currentStartRow = maximumSqlRows;

                    if (currentStartRow <= 1)
                        noMoreEvents = true;

                    currentStartRow -= rowsToGetAtATime;
                    if (currentStartRow < 1)
                        currentStartRow = 1;
                }
            }

            if (searchCriteria.ObjectCount(StackHashObjectType.Script) == 0)
            {
                allEvents.TotalRows = maximumSqlRows;
            }
            else
            {
                if (countAllMatches)
                {
                    // Count the remaining matches if there are any. If we haven't retrieved the full requested window then there can't be any more.
                    if (allEvents.Count == numberOfRows)
                    {
                        // There must be some more rows to look at.
                        if (searchDirection == StackHashSearchDirection.Forwards)
                            allEvents.TotalRows = allEvents.Count + CountEvents(searchCriteria, allEvents.MaximumRowNumber + 1, Int32.MaxValue, sortOrder, searchDirection);
                        else
                            allEvents.TotalRows = allEvents.Count + CountEvents(searchCriteria, allEvents.MinimumRowNumber - 1, Int32.MaxValue, sortOrder, searchDirection);
                    }
                }
            }

            populateCabContents(allEvents);

            return allEvents;
        }


        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="searchCriteria">Specifies what to search.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <param name="searchDirection">Forwards or backwards.</param>
        public long CountEvents(
            StackHashSearchCriteriaCollection searchCriteria, long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder,
            StackHashSearchDirection searchDirection)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot get events while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot get events while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot get events on inactive profile", StackHashServiceErrorCode.ProfileInactive);
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");
            if (sortOrder == null)
                throw new ArgumentNullException("sortOrder");
            if (sortOrder.Count == 0)
                throw new ArgumentException("Must specify a sort order", "sortOrder");

            // Reset any out of range values.
            if (startRow > Int32.MaxValue)
                startRow = Int32.MaxValue;
            if (numberOfRows > Int32.MaxValue)
                numberOfRows = Int32.MaxValue;
            if (startRow < 1)
                startRow = 1;


            bool noMoreEvents = false;
            long currentStartRow = startRow;
            long totalMatchedRows = 0;
            long maximumSqlRows = 0;

            // Don't allow a get of more than say 5000 events at a time.
            long rowsToGetAtATime = numberOfRows;
            if (rowsToGetAtATime > m_MaxRowsToGetPerRequest)
                rowsToGetAtATime = m_MaxRowsToGetPerRequest;

            while (!noMoreEvents && (totalMatchedRows < numberOfRows))
            {
                StackHashEventPackageCollection nextEvents = m_ErrorIndex.GetEvents(searchCriteria, currentStartRow, rowsToGetAtATime,
                    sortOrder, this.WinQualSettings.ProductsToSynchronize);

                // Just record the number of rows in the search. This is not the number of rows returned and will stay constant for all calls in this loop.
                if (nextEvents.MaximumSqlRows != 0)
                    maximumSqlRows = nextEvents.MaximumSqlRows;

                // If not all the requested events were returned from the index (searching forward) then it must have run out of matches.
                if ((searchDirection == StackHashSearchDirection.Forwards) && (nextEvents.Count < rowsToGetAtATime))
                    noMoreEvents = true;

                // If searching backwards and there we reached row 1 then we are at the start of the matching events so there are no more to read.
                if ((searchDirection == StackHashSearchDirection.Backwards) && (currentStartRow == 1))
                    noMoreEvents = true;

                // Filter out events that don't match the Script search criteria (if there is one).
                nextEvents = removeEventsAndCabsThatDoNotMatchScriptSearchCriteria(searchCriteria, nextEvents);

                if (searchDirection == StackHashSearchDirection.Forwards)
                {
                    totalMatchedRows += nextEvents.Count;
                    currentStartRow += rowsToGetAtATime;
                }
                else
                {
                    totalMatchedRows += nextEvents.Count;

                    // The caller may have specified Int64.MaxValue for the last row if they didn't know how many rows there were.
                    if (currentStartRow == Int32.MaxValue)
                        currentStartRow = maximumSqlRows;

                    if (currentStartRow <= 1)
                        noMoreEvents = true;

                    currentStartRow -= rowsToGetAtATime;
                    if (currentStartRow < 1)
                        currentStartRow = 1;
                }
            }

            return totalMatchedRows;
        }

        
        /// <summary>
        /// Runs a script on a particular cab file.
        /// </summary>
        /// <param name="product">Product data</param>
        /// <param name="file">File data</param>
        /// <param name="theEvent">Event data</param>
        /// <param name="cab">Cab data</param>
        /// <param name="dumpFileName">Name of the dump file or null</param>
        /// <param name="scriptName">Name of script to run on the dump file</param>
        /// <param name="clientData">Data describing the client</param>
        /// <returns>Full result of running the script</returns>
        public StackHashScriptResult RunScript(StackHashProduct product, StackHashFile file,
            StackHashEvent theEvent, StackHashCab cab, String dumpFileName, String scriptName, StackHashClientData clientData)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            // Call through to the appropriate script results manager to perform the action.
            return m_ScriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, true, clientData, true);
        }

        /// <summary>
        /// Gets the script result files for all scripts.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which results are required.</param>
        /// <returns>Script files.</returns>
        public StackHashScriptResultFiles GetResultFiles(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            return m_ScriptResultsManager.GetResultFiles(product, file, theEvent, cab);
        }


        /// <summary>
        /// Gets script results file data a particular script and cab.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which results are required.</param>
        /// <param name="scriptName"></param>
        /// <returns>Single script result.</returns>
        public StackHashScriptResult GetResultFileData(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String scriptName)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            return m_ScriptResultsManager.GetResultFileData(product, file, theEvent, cab, scriptName);

        }


        /// <summary>
        /// Removes the script result data for the specified cab and script file.
        /// </summary>
        /// <param name="clientData">The client that makes the request.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which results are to be deleted.</param>
        /// <param name="scriptName">Name of script file whos results are to be deleted.</param>
        public void RemoveResultFileData(StackHashClientData clientData, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String scriptName)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (clientData == null)
                throw new ArgumentNullException("clientData");
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (scriptName == null)
                throw new ArgumentNullException("scriptName");

            m_ScriptResultsManager.RemoveResultFileData(product, file, theEvent, cab, scriptName);

            String noteText = String.Format(CultureInfo.InvariantCulture, "Script results removed for script: {0}", scriptName);
            StackHashNoteEntry noteEntry = new StackHashNoteEntry(DateTime.Now.ToUniversalTime(), "Service", clientData.ClientName, noteText);
            m_ErrorIndex.AddCabNote(product, file, theEvent, cab, noteEntry);
        }


        /// <summary>
        /// Gets the notes for the specified cab.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which notes are required.</param>
        /// <returns>Notes for the cab.</returns>
        public StackHashNotes GetCabNotes(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            StackHashNotes notes = m_ErrorIndex.GetCabNotes(product, file, theEvent, cab);
            return notes;
        }


        /// <summary>
        /// Adds a note for the specified cab.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which note is to be added.</param>
        /// <param name="note">Note to add.</param>
        public void AddCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");

            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (note == null)
                throw new ArgumentNullException("note");

            // Set the time of update to the current time (GMT).
            note.TimeOfEntry = DateTime.Now.ToUniversalTime();

            m_ErrorIndex.AddCabNote(product, file, theEvent, cab, note);
        }


        /// <summary>
        /// Deletes a note from the specified cab.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which note is to be deleted.</param>
        /// <param name="noteId">Note to delete.</param>
        public void DeleteCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (noteId < 1)
                throw new ArgumentException("Note Id must be greater than zero", "noteId");

            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot delete cab note for an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            m_ErrorIndex.DeleteCabNote(product, file, theEvent, cab, noteId);
        }


        /// <summary>
        /// Gets notes for a particular event.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event for which notes required.</param>
        /// <returns>Notes for the event.</returns>
        public StackHashNotes GetEventNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            StackHashNotes notes = m_ErrorIndex.GetEventNotes(product, file, theEvent);
            return notes;
        }


        /// <summary>
        /// Adds a note to the specified event.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to add note to.</param>
        /// <param name="note">Note to add.</param>
        public void AddEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (note == null)
                throw new ArgumentNullException("note");
 
            // Set the time of update to the current time (GMT).
            note.TimeOfEntry = DateTime.Now.ToUniversalTime();

            m_ErrorIndex.AddEventNote(product, file, theEvent, note);
        }

        /// <summary>
        /// Deletes the specified event note.
        /// </summary>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to delete note from.</param>
        /// <param name="noteId">Note to delete.</param>
        public void DeleteEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot delete event notes on an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            m_ErrorIndex.DeleteEventNote(product, file, theEvent, noteId);
        }


        /// <summary>
        /// Gets a rollup of all of the product statistics.
        /// This includes the locale, OS and hit date rollups.
        /// </summary>
        /// <param name="productId">Product for which the stats is required.</param>
        /// <returns>Rollup stats.</returns>
        public StackHashProductSummary GetProductSummary(int productId)
        {
            if (m_MoveInProgress)
                throw new StackHashException("Cannot activate while index move is in progress", StackHashServiceErrorCode.MoveInProgress);
            if (m_CopyInProgress)
                throw new StackHashException("Cannot activate while index copy is in progress", StackHashServiceErrorCode.CopyInProgress);
            if (!m_IsActive)
                throw new StackHashException("Cannot synchronize an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            StackHashProductSummary productSummary = new StackHashProductSummary();

            productSummary.LocaleSummaryCollection = m_ErrorIndex.GetLocaleSummary(productId);
            productSummary.OperatingSystemSummary = m_ErrorIndex.GetOperatingSystemSummary(productId);
            productSummary.HitDateSummary = m_ErrorIndex.GetHitDateSummary(productId);

            return productSummary;
        }

        /// <summary>
        /// Aborts the specified task type.
        /// </summary>
        /// <param name="contextId"></param>
        public void AbortTask(StackHashClientData clientData, StackHashTaskType taskType)
        {
            if (clientData == null)
                throw new ArgumentNullException("clientData");

            if (clientData.ClientName != null)
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "User " + clientData.ClientName + " aborting " + taskType.ToString());
                
            if (m_TaskController.CanTaskBeAbortedByClient(taskType))
            {
                AbortTaskOfType(taskType, false); // Don't wait.
            }
            else
            {
                throw new StackHashException("Cannot abort task of type " + taskType.ToString(), StackHashServiceErrorCode.CannotStopSpecifiedTask);
            }
        }

        public StackHashContextStatus Status
        {
            get
            {
                StackHashContextStatus contextStatus = new StackHashContextStatus();
                contextStatus.CurrentError = m_CurrentContextError;
                contextStatus.LastContextException = m_LastContextException.BuildDescription();

                contextStatus.IsActive = m_IsActive;
                contextStatus.ContextId = m_Id;
                if (m_ContextSettings.WinQualSettings != null)
                    contextStatus.ContextName = m_ContextSettings.WinQualSettings.CompanyName;
                else
                    contextStatus.ContextName = null;



                // Create a status for all tasks types.
                contextStatus.TaskStatusCollection = new StackHashTaskStatusCollection();
                Type taskEnumType = typeof(StackHashTaskType);
                Array allValues = Enum.GetValues(taskEnumType);

                StackHashTaskStatus winQualSyncTaskStatus = null;

                foreach (StackHashTaskType taskType in allValues)
                {
                    StackHashTaskStatus taskStatus;

                    if (m_IsActive)
                        taskStatus = m_ErrorIndex.GetTaskStatistics(taskType);
                    else
                        taskStatus = new StackHashTaskStatus();

                    // Set the actual state of the task.
                    taskStatus.TaskState = m_TaskController.GetTaskState(taskType);
                    taskStatus.TaskType = taskType;
                    taskStatus.CanBeAbortedByClient = m_TaskController.CanTaskBeAbortedByClient(taskType);

                    contextStatus.TaskStatusCollection.Add(taskStatus);

                    if (taskType == StackHashTaskType.WinQualSynchronizeTask)
                        winQualSyncTaskStatus = taskStatus;
                }

                // Get the last WinQual sync status.
                if (winQualSyncTaskStatus != null)
                {
                    if (winQualSyncTaskStatus.ServiceErrorCode == StackHashServiceErrorCode.WinQualLogOnFailed)
                    {
                        contextStatus.LastSynchronizationLogOnFailed = true;
                        contextStatus.LastSynchronizationLogOnException = winQualSyncTaskStatus.LastException;
                        contextStatus.LastSynchronizationLogOnServiceError = winQualSyncTaskStatus.ServiceErrorCode;
                    }
                    else
                    {
                        contextStatus.LastSynchronizationLogOnFailed = false;
                        contextStatus.LastSynchronizationLogOnServiceError = StackHashServiceErrorCode.NoError;
                    }
                }

                if (m_BugTrackerContext != null)
                {
                    contextStatus.PlugInDiagnostics = m_BugTrackerContext.GetContextDiagnostics(null);
                }
                
                return contextStatus;
            }
        }

        /// <summary>
        /// Check to see if the database exists or not.
        /// Causes an exception if no connection could be made to the database or
        /// the database is unknown.
        /// </summary>
        /// <param name="contextId">Context whose connection is to be tested.</param>
        /// <returns>True - connection ok and database exists.</returns>
        /// <returns>False - connection ok and database does not exist.</returns>
        public bool CheckDatabaseConnection()
        {
            return (m_ErrorIndex.Status == ErrorIndexStatus.Created);
        }

        /// <summary>
        /// Gets default properties and diagnostics for all loaded BugTracker plugins.
        /// If plugInName is specified then only that plugin diagnostics are returned otherwise all plugin diagnostics 
        /// are returned.
        /// </summary>
        /// <param name="plugInName">Name of the plug-in or null for all plug-ins</param>
        /// <returns>Full plug-in properties and diagnostics.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashBugTrackerPlugInDiagnosticsCollection GetBugTrackerPlugInDiagnostics(String plugInName)
        {
            if (m_BugTrackerContext != null)
                return m_BugTrackerContext.GetContextDiagnostics(plugInName);
            else
                return new StackHashBugTrackerPlugInDiagnosticsCollection(); // Return the empty set.
        }


        /// <summary>
        /// Gets mappings of a particular type.
        /// </summary>
        /// <param name="mappingType">Type of mapping required.</param>
        /// <returns>Mappings.</returns>
        public StackHashMappingCollection GetMappings(StackHashMappingType mappingType)
        {
            StackHashMappingCollection mappings = m_ErrorIndex.GetMappings(mappingType);
            return mappings;
        }


        /// <summary>
        /// Updates the specified mapping entries.
        /// </summary>
        /// <param name="mappingType">Type of mapping to set.</param>
        /// <param name="mappings">The mappings to update.</param>
        public void UpdateMappings(StackHashMappingType mappingType, StackHashMappingCollection mappings)
        {
            //if (!m_IsActive)
            //    throw new StackHashException("Cannot get mappings from an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (mappings == null)
                throw new ArgumentNullException("mappings");

            // Check that all the mappings are the same type.
            foreach (StackHashMapping mapping in mappings)
            {
                if (mapping.MappingType != mappingType)
                    throw new ArgumentException("Mapping types don't match", "mappings");
                if (mapping.Name == null)
                    throw new ArgumentException("Mapping name of null found", "mappings");
            }

            // Check the workflow mapping params.
            if (mappingType == StackHashMappingType.WorkFlow)
            {
                int expectedWorkFlowMappingCount = StackHashMappingCollection.DefaultWorkFlowMappings.Count;
                if (mappings.Count != expectedWorkFlowMappingCount)
                    throw new ArgumentException("Unexpected number of workflow mappings: " + mappings.Count, "mappings");

                // Should be the correct ids too.
                Dictionary<int, int> workFlowIds = new Dictionary<int, int>();

                foreach (StackHashMapping mapping in mappings)
                {
                    if (workFlowIds.ContainsKey(mapping.Id))
                        throw new ArgumentException("Duplicate workflow mapping id", "mappings");
                    if (mapping.Id < 0)
                        throw new ArgumentException("Invalid negative workflow id", "mappings");
                    if (mapping.Id > expectedWorkFlowMappingCount)
                        throw new ArgumentException("Workflow id too large", "mappings");
                    workFlowIds[mapping.Id] = 1;
                }
            }
            else if (mappingType == StackHashMappingType.Group)
            {
                throw new NotImplementedException("Groups are not currently implemented");
            }
            m_ErrorIndex.AddMappings(mappings);
        }


        /// <summary>
        /// Get the cab package for a particular cab.
        /// This contains a little more information that would be too much for each individual cab.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">Cab for which more data is required.</param>
        /// <returns>Cab package</returns>
        public StackHashCabPackage GetCabPackage(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new StackHashException("Cannot get cab package from an inactive profile", StackHashServiceErrorCode.ProfileInactive);

            if (cab == null)
                throw new ArgumentNullException("cab");

            StackHashCab currentCab = m_ErrorIndex.GetCab(product, file, theEvent, cab.Id);
            String cabFileName = m_ErrorIndex.GetCabFileName(product, file, theEvent, currentCab);

            Collection<CabinetFileInfo> cabFiles = Cabs.GetCabFiles(cabFileName);

            StackHashCabFileContents contents = new StackHashCabFileContents();

            foreach (CabinetFileInfo cabFileInfo in cabFiles)
            {
                contents.Files.Add(new StackHashCabFile(cabFileInfo.FileName, cabFileInfo.Size));
            }

            StackHashCabPackage cabPackage = new StackHashCabPackage(currentCab, cabFileName, contents, true);

            return cabPackage;
        }

        
        #region IDisposable Members

        /// <summary>
        /// Disposes of all resources.
        /// </summary>
        /// <param name="disposing">True - disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stop as tasks.
                m_TaskController.AbortAllTasks(true);
                m_TaskController.Close();

                m_TaskController.TaskStarted -= new EventHandler<TaskStartedEventArgs>(this.taskStartedEventHandler);
                m_TaskController.TaskCompleted -= new EventHandler<TaskCompletedEventArgs>(this.taskCompletedEventHandler);

                if (m_ErrorIndex != null)
                    m_ErrorIndex.Dispose();

                m_WinQualSyncTimerTask = null;

                if (m_BugTrackerContext != null)
                    m_BugTrackerContext.Dispose();

                if (m_MailManager != null)
                    m_MailManager.Dispose();
            }
        }

        /// <summary>
        /// Dispose of managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
