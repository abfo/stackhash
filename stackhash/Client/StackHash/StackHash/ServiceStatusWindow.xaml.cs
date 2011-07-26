using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using StackHash.StackHashService;
using StackHashUtilities;
using System.Globalization;
using System.Collections.Specialized;

namespace StackHash
{
    /// <summary>
    /// Displays the current status of the service 
    /// </summary>
    public partial class ServiceStatusWindow : Window, IDisposable
    {
        private class WorkerArg
        {
            public bool AbortTask { get; private set; }
            public StackHashTaskType TaskType { get; private set; }
            public int ContextId { get; private set; }

            // use this constructor to NOT abort a task
            public WorkerArg()
            {

            }

            // use this constructor to abort a task of the specified type
            public WorkerArg(StackHashTaskType taskType, int contextId)
            {
                this.AbortTask = true;
                this.TaskType = taskType;
                this.ContextId = contextId;
            }
        }

        private const string WindowKey = "ServiceStatusWindow";
        private const string ListViewContextsKey = "ServiceStatusWindow.listViewContexts";
        private const string ListViewTasksKey = "ServiceStatusWindow.listViewTasks";

        private BackgroundWorker _worker;
        private bool _windowClosed;
        private ListViewSorter _contextListSorter;
        private ListViewSorter _taskListSorter;
        private int _reselectContextId;
        private bool _reselectContextIdIsActive;
        private bool _sortingContexts;
        private bool _sortingTasks;
        private ClientLogic _clientLogic;

        /// <summary>
        /// Displays the current status of the service 
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        public ServiceStatusWindow(ClientLogic clientLogic)
        {
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }
            _clientLogic = clientLogic;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            UserSettings.Settings.RestoreGridView(ListViewContextsKey, listViewContexts.View as GridView);
            UserSettings.Settings.RestoreGridView(ListViewTasksKey, listViewTasks.View as GridView);

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);

            _reselectContextId = UserSettings.InvalidContextId;

            _contextListSorter = new ListViewSorter(listViewContexts);
            _taskListSorter = new ListViewSorter(listViewTasks);
        }

        private void ListViewTasksItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_sortingTasks)
            {
                _sortingTasks = true;
                _taskListSorter.SortLastColumn();
                _sortingTasks = false;
            }
        }

        private void ListViewContextsItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_sortingContexts)
            {
                _sortingContexts = true;
                _contextListSorter.SortLastColumn();

                // try to reselect the previously selected context
                if (_reselectContextId != UserSettings.InvalidContextId)
                {
                    StackHashStatus status = this.DataContext as StackHashStatus;
                    if (status != null)
                    {
                        foreach (StackHashContextStatus contextStatus in status.ContextStatusCollection)
                        {
                            if (contextStatus.ContextId == _reselectContextId)
                            {
                                listViewContexts.SelectedItem = contextStatus;
                                break;
                            }
                        }
                    }
                }

                _sortingContexts = false;
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // do nothing if the window has closed
            if (_windowClosed)
            {
                return;
            }

            this.IsEnabled = true;

            if (e.Error == null)
            {
                _reselectContextId = UserSettings.Settings.CurrentContextId;

                // update the datacontext with the new StackHashStatus object
                this.DataContext = e.Result;

                // try to select the current profile
                //if (!_initialContextSelected)
                //{
                //    if (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId)
                //    {
                //        StackHashStatus status = e.Result as StackHashStatus;
                //        if ((status != null) && (status.ContextStatusCollection != null))
                //        {
                //            foreach (StackHashContextStatus contextStatus in status.ContextStatusCollection)
                //            {
                //                if (contextStatus.ContextId == UserSettings.Settings.CurrentContextId)
                //                {
                //                    listViewContexts.SelectedItem = contextStatus;
                //                    _reselectContextId = contextStatus.ContextId;
                //                    _reselectContextIdIsActive = contextStatus.IsActive;
                //                    break;
                //                }
                //            }
                //        }
                //    }

                //    _initialContextSelected = true;
                //}
            }
            else
            {
                StackHashServiceErrorCode errorCode = StackHashMessageBox.ParseServiceErrorFromException(e.Error);
                if (errorCode != StackHashServiceErrorCode.Aborted)
                {
                    // show error
                    StackHashMessageBox.Show(this,
                        Properties.Resources.Error_ServiceCallFailedMBMessage,
                        Properties.Resources.Error_ServiceCallFailedMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error,
                        e.Error,
                        errorCode);
                }
            }
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArg workerArg = e.Argument as WorkerArg;
            if ((workerArg != null) && (workerArg.AbortTask))
            {
                // abort the specified task
                AbortTaskRequest abortRequest = new AbortTaskRequest();
                abortRequest.ClientData = UserSettings.Settings.GenerateClientData();
                abortRequest.ContextId = workerArg.ContextId;
                abortRequest.TaskType = workerArg.TaskType;

                ServiceProxy.Services.Admin.AbortTask(abortRequest);
            }

            // get an updated service status
            GetStackHashServiceStatusRequest request = new GetStackHashServiceStatusRequest();
            request.ClientData = UserSettings.Settings.GenerateClientData();

            GetStackHashServiceStatusResponse response = ServiceProxy.Services.Admin.GetServiceStatus(request);

            DumpStatus(response.Status);

            e.Result = response.Status;
        }

        private void DumpStatus(StackHashStatus status)
        {
            if (status == null)
            {
                return;
            }

            try
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Dumping Service Status...");
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ". Initialization failed: {0}", status.InitializationFailed));
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ". Host running in test mode: {0}", status.HostRunningInTestMode));

                foreach (StackHashContextStatus contextStatus in status.ContextStatusCollection)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ".. Active: {0}", contextStatus.IsActive));
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ".. Last synchronization logon failed: {0}", contextStatus.LastSynchronizationLogOnFailed));
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ".. Last synchronization login service error: {0}", contextStatus.LastSynchronizationLogOnServiceError));
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ".. Last synchronization logon exception: {0}", contextStatus.LastSynchronizationLogOnException));
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ".. Current error: {0}", contextStatus.CurrentError));
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, string.Format(CultureInfo.InvariantCulture, ".. Last context exception: {0}", contextStatus.LastContextException));

                    foreach (StackHashTaskStatus taskStatus in contextStatus.TaskStatusCollection)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "... Type: {0}, State: {1}, Last Exception: {2}, Last Duration: {3}, Run Count: {4}, Success: {5}, Failure: {6}, Last Started: {7}, Last Succeeded: {8}, Last Failed: {9}",
                            taskStatus.TaskType,
                            taskStatus.TaskState,
                            taskStatus.LastException,
                            taskStatus.LastDurationInSeconds,
                            taskStatus.RunCount,
                            taskStatus.SuccessCount,
                            taskStatus.FailedCount,
                            taskStatus.LastStartedTimeUtc,
                            taskStatus.LastSuccessfulRunTimeUtc,
                            taskStatus.LastFailedRunTimeUtc));
                    }
                }

            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning,
                    "Failed to dump service status",
                    ex);
            }
        }

        private void RefreshStatus()
        {
            if (!_worker.IsBusy)
            {
                this.IsEnabled = false;
                _worker.RunWorkerAsync(new WorkerArg());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                ((INotifyCollectionChanged)listViewContexts.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(ListViewContextsItems_CollectionChanged);
                ((INotifyCollectionChanged)listViewTasks.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(ListViewTasksItems_CollectionChanged);

                RefreshStatus();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _windowClosed = true;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                ((INotifyCollectionChanged)listViewContexts.Items).CollectionChanged -= ListViewContextsItems_CollectionChanged;
                ((INotifyCollectionChanged)listViewTasks.Items).CollectionChanged -= ListViewTasksItems_CollectionChanged;

                UserSettings.Settings.SaveGridView(ListViewContextsKey, listViewContexts.View as GridView);
                UserSettings.Settings.SaveGridView(ListViewTasksKey, listViewTasks.View as GridView);
                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void listViewTasks_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _sortingTasks = true;
                _taskListSorter.SortColumn(header);
                _sortingTasks = false;
            }
        }

        private void listViewContexts_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _sortingContexts = true;
                _contextListSorter.SortColumn(header);
                _sortingContexts = false;
            }
        }

        private void listViewContexts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StackHashContextStatus contextStatus = listViewContexts.SelectedItem as StackHashContextStatus;
            if (contextStatus != null)
            {
                _reselectContextId = contextStatus.ContextId;
                _reselectContextIdIsActive = contextStatus.IsActive;
            }
        }

        #region IDisposable Members

        /// <summary />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary />
        ~ServiceStatusWindow()
        {
            Dispose(false);
        }

        private void Dispose(bool canDisposeManagedResources)
        {
            if (canDisposeManagedResources)
            {
                if (_worker != null)
                {
                    _worker.Dispose();
                    _worker = null;
                }
            }
        }

        #endregion

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("service-status.htm");
        }

        private void buttonPluginDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            PluginDiagnostics pluginDiagnostics = new PluginDiagnostics(_clientLogic, _reselectContextId, _reselectContextIdIsActive);
            pluginDiagnostics.Owner = this;
            pluginDiagnostics.ShowDialog();
        }

        private void menuItemAbortTask_Click(object sender, RoutedEventArgs e)
        {
            if (!_worker.IsBusy)
            {
                StackHashTaskStatus selectedTask = listViewTasks.SelectedItem as StackHashTaskStatus;
                if (selectedTask != null)
                {
                    this.IsEnabled = false;
                    _worker.RunWorkerAsync(new WorkerArg(selectedTask.TaskType, _reselectContextId));
                }
            }
        }

        private void listViewTasks_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool canAbort = false;

            if (!_worker.IsBusy)
            {
                StackHashTaskStatus selectedTask = listViewTasks.SelectedItem as StackHashTaskStatus;
                if ((selectedTask != null) && (selectedTask.CanBeAbortedByClient) && (selectedTask.TaskState == StackHashTaskState.Running))
                {
                    canAbort = true;
                }
            }

            menuItemAbortTask.IsEnabled = canAbort;
        }
    }
}
