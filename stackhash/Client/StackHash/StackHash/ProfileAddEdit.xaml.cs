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
using StackHash.StackHashService;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Win32;
using System.ComponentModel;
using System.Collections.ObjectModel;
using StackHashUtilities;

namespace StackHash
{
    /// <summary>
    /// Edits a StackHash profile
    /// </summary>
    public partial class ProfileAddEdit : Window
    {
        private const string WindowKey = "ProfileAddEdit";
        private const string ListViewPluginsKey = "ProfileAddEdit.listViewPlugins";
        private const int WorkFlowStatusCount = 16;

        private class ContextValidation : IDataErrorInfo
        {
            private const int ValidationMinPool = 2;
            private const int ValidationMaxPool = 100;
            private const int ValidationMinConnectionTimeout = 5;
            private const int ValidationMaxConnectionTimeout = 120;
            private const int ValidationMinEventsPerBlock = 10;
            private const int ValidationMaxEventsPerBlock = 10000;

            public bool ValidationEnabled { get; set; }
            public string Company { get; set; }
            public string Username { get; set; }
            public string IndexFolder { get; set; }
            public string ConnectionString { get; set; }
            public int PurgeDays { get; set; }
            public string WinDbgPath { get; set; }
            public string SymbolPath { get; set; }
            public string ImagePath { get; set; }
            public string WinDbgPath64 { get; set; }
            public string SymbolPath64 { get; set; }
            public string ImagePath64 { get; set; }
            public bool ForceRerun { get; set; }
            public int RequestRetryCount { get; set; }
            public int RequestTimeoutMinutes { get; set; }
            public bool RetrySync { get; set; }
            public int RetrySyncDelayMinutes { get; set; }
            public int CabDownloadFailureLimit { get; set; }
            public int SyncsBeforeResync { get; set; }
            public int MinPoolSize { get; set; }
            public int MaxPoolSize { get; set; }
            public int ConnectionTimeout { get; set; }
            public int EventsPerBlock { get; set; }
            public string SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public string SmtpUser { get; set; }
            public string SmtpFrom { get; set; }
            public string SmtpTo { get; set; }
            public bool SmtpNotifySync { get; set; }
            public bool SmtpNotifyAnalyze { get; set; }
            public bool SmtpNotifyPurge { get; set; }
            public bool SmtpNotifyPluginReport { get; set; }
            public bool SmtpNotifyPluginError { get; set; }
            public bool EnableNewProducts { get; set; }
            public string[] WorkFlowStatuses { get; set; }
            public bool WorkFlowStatusesEnabled { get; set; }

            public bool Is64 { get; private set; }
            private ObservableCollection<StackHashContextSettings> _allProfiles;
            private int _contextId;
           

            public ContextValidation(string company, 
                string username, 
                string indexFolder, 
                string connectionString, 
                int purgeDays,
                string winDbgPath,
                string symbolPath,
                string imagePath,
                string winDbgPath64,
                string symbolPath64,
                string imagePath64,
                ObservableCollection<StackHashContextSettings> allProfiles,
                int contextId,
                bool is64,
                bool forceRerun,
                int requestRetryCount,
                int requestTimeoutMinutes,
                bool retrySync,
                int retrySyncDelayMinutes,
                int cabDownloadFailureLimit,
                int syncsBeforeResync,
                int minPoolSize,
                int maxPoolSize,
                int connectionTimeout,
                int eventsPerBlock,
                string smtpServer,
                int smtpPort,
                string smtpUser,
                string smtpFrom,
                string smtpTo,
                bool smtpNotifySync,
                bool smtpNotifyAnalyze,
                bool smtpNotifyPurge,
                bool smtpNotifyPluginReport,
                bool smtpNotifyPluginError,
                bool enableNewProducts,
                string[] workFlowStatuses,
                bool workFlowStatusesEnabled)
            {
                Company = company;
                Username = username;
                IndexFolder = indexFolder;
                ConnectionString = connectionString;
                PurgeDays = purgeDays;
                WinDbgPath = winDbgPath;
                SymbolPath = symbolPath;
                ImagePath = imagePath;
                WinDbgPath64 = winDbgPath64;
                SymbolPath64 = symbolPath64;
                ImagePath64 = imagePath64;
                _allProfiles = allProfiles;
                _contextId = contextId;
                Is64 = is64;
                ForceRerun = forceRerun;
                RequestRetryCount = requestRetryCount;
                RequestTimeoutMinutes = requestTimeoutMinutes;
                RetrySync = retrySync;
                RetrySyncDelayMinutes = retrySyncDelayMinutes;
                CabDownloadFailureLimit = cabDownloadFailureLimit;
                SyncsBeforeResync = syncsBeforeResync;
                MinPoolSize = minPoolSize;
                MaxPoolSize = maxPoolSize;
                ConnectionTimeout = connectionTimeout;
                EventsPerBlock = eventsPerBlock;
                SmtpServer = smtpServer;
                SmtpPort = smtpPort;
                SmtpUser = smtpUser;
                SmtpFrom = smtpFrom;
                SmtpTo = smtpTo;
                SmtpNotifySync = smtpNotifySync;
                SmtpNotifyAnalyze = smtpNotifyAnalyze;
                SmtpNotifyPurge = smtpNotifyPurge;
                SmtpNotifyPluginReport = smtpNotifyPluginReport;
                SmtpNotifyPluginError = smtpNotifyPluginError;
                EnableNewProducts = enableNewProducts;
                WorkFlowStatuses = workFlowStatuses;
                WorkFlowStatusesEnabled = workFlowStatusesEnabled;
            }

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get 
                {
                    if (!ValidationEnabled)
                    {
                        return null;
                    }

                    string result = null;

                    switch (columnName)
                    {
                        case "Company":
                            if (string.IsNullOrEmpty(Company))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorCompany;
                            }
                            else
                            {
                                // prevent duplicate company / profile names
                                foreach (StackHashContextSettings context in _allProfiles)
                                {
                                    // don't need to check the current context
                                    if (context.Id == _contextId)
                                    {
                                        continue;
                                    }

                                    if (string.Compare(Company, context.WinQualSettings.CompanyName, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        result = Properties.Resources.ProfileAddEdit_ValidationErrorCompanyDuplicate;
                                        break;
                                    }
                                }
                            }
                            break;

                        case "Username":
                            if (string.IsNullOrEmpty(Username))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorUsername;
                            }
                            break;

                        case "IndexFolder":
                            if (string.IsNullOrEmpty(IndexFolder))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorIndexFolder;
                            }
                            else if (!System.IO.Directory.Exists(IndexFolder))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorIndexFolder;
                            }
                            else
                            {
                                // prevent duplicate folders
                                foreach (StackHashContextSettings context in _allProfiles)
                                {
                                    // don't need to check the current context
                                    if (context.Id == _contextId)
                                    {
                                        continue;
                                    }

                                    if (string.Compare(IndexFolder, context.ErrorIndexSettings.Folder, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        result = Properties.Resources.ProfileAddEdit_ValidationErrorIndexFolderDuplicate;
                                        break;
                                    }
                                }
                            }
                            break;

                        case "PurgeDays":
                            if (PurgeDays <= 0)
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorPurgeDays;
                            }
                            break;

                        case "WinDbgPath":
                            // must be empty or exit
                            if ((!string.IsNullOrEmpty(WinDbgPath)) && (!System.IO.File.Exists(WinDbgPath)))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorWinDbg;
                            }
                            break;

                        case "SymbolPath":
                            if (SymbolPath.Contains('\"') || SymbolPath.Contains('\''))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorSearchPathQuotes;
                            }
                            break;

                        case "ImagePath":
                            if (ImagePath.Contains('\"') || ImagePath.Contains('\''))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorSearchPathQuotes;
                            }
                            break;

                        case "WinDbgPath64":
                            // only validate if on a 64-bit system
                            if (Is64)
                            {
                                // must be empty or exist
                                if ((!string.IsNullOrEmpty(WinDbgPath64)) && (!System.IO.File.Exists(WinDbgPath64)))
                                {
                                    result = Properties.Resources.ProfileAddEdit_ValidationErrorWinDbg;
                                }
                            }
                            break;

                        case "SymbolPath64":
                            if (SymbolPath64.Contains('\"') || SymbolPath64.Contains('\''))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorSearchPathQuotes;
                            }
                            break;

                        case "ImagePath64":
                            if (ImagePath64.Contains('\"') || ImagePath64.Contains('\''))
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorSearchPathQuotes;
                            }
                            break;

                        case "RequestRetryCount":
                            if (RequestRetryCount <= 0)
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorRetry;
                            }
                            break;

                        case "RequestTimeoutMinutes":
                            if (RequestTimeoutMinutes <= 0)
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorTimeout;
                            }
                            break;

                        case "RetrySyncDelayMinutes":
                            if (RetrySyncDelayMinutes <= 0)
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorSyncRetryMins;
                            }
                            break;

                        case "CabDownloadFailureLimit":
                            if (CabDownloadFailureLimit < 0)
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorCabFailLimit;
                            }
                            break;

                        case "SyncsBeforeResync":
                            if (SyncsBeforeResync <= 0)
                            {
                                result = Properties.Resources.ProfileAddEdit_ValidationErrorSyncsBeforeResync;
                            }
                            break;

                        case "MinPoolSize":
                            if ((MinPoolSize < ValidationMinPool) || (MinPoolSize > ValidationMaxPool))
                            {
                                result = string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.ProfileAddEdit_ValidationErrorMinPoolSize,
                                    ValidationMinPool,
                                    ValidationMaxPool);
                            }
                            break;

                        case "MaxPoolSize":
                            if ((MaxPoolSize < ValidationMinPool) || (MaxPoolSize > ValidationMaxPool) || (MaxPoolSize < MinPoolSize))
                            {
                                result = string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.ProfileAddEdit_ValidationErrorMaxPoolSize,
                                    ValidationMinPool,
                                    ValidationMaxPool);
                            }
                            break;

                        case "ConnectionTimeout":
                            if ((ConnectionTimeout < ValidationMinConnectionTimeout) || (ConnectionTimeout > ValidationMaxConnectionTimeout))
                            {
                                result = string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.ProfileAddEdit_ValidationErrorConnectionTimeout,
                                    ValidationMinConnectionTimeout,
                                    ValidationMaxConnectionTimeout);
                            }
                            break;

                        case "EventsPerBlock":
                            if ((EventsPerBlock < ValidationMinEventsPerBlock) || (ConnectionTimeout > ValidationMaxEventsPerBlock))
                            {
                                result = string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.ProfileAddEdit_ValidationErrorEventsPerBlock,
                                    ValidationMinEventsPerBlock,
                                    ValidationMaxEventsPerBlock);
                            }
                            break;

                        case "SmtpServer":
                            if (SmtpNotifySync || SmtpNotifyPurge || SmtpNotifyAnalyze || SmtpNotifyPluginError || SmtpNotifyPluginReport)
                            {
                                if (string.IsNullOrEmpty(this.SmtpServer))
                                {
                                    result = Properties.Resources.ProfileAddEdit_ValidationErrorSmtpServer;
                                }
                            }
                            break;

                        case "SmtpPort":
                            if (SmtpNotifySync || SmtpNotifyPurge || SmtpNotifyAnalyze || SmtpNotifyPluginError || SmtpNotifyPluginReport)
                            {
                                if ((this.SmtpPort < 0) || (this.SmtpPort > 65535))
                                {
                                    result = Properties.Resources.ProfileAddEdit_ValidationErrorSmtpPort;
                                }
                            }
                            break;

                        case "SmtpFrom":
                            if (SmtpNotifySync || SmtpNotifyPurge || SmtpNotifyAnalyze || SmtpNotifyPluginError || SmtpNotifyPluginReport)
                            {
                                if (!ClientUtils.IsEmailValid(SmtpFrom, false))
                                {
                                    result = Properties.Resources.ProfileAddEdit_ValidationErrorSmtpFrom;
                                }
                            }
                            break;

                        case "SmtpTo":
                            if (SmtpNotifySync || SmtpNotifyPurge || SmtpNotifyAnalyze || SmtpNotifyPluginError || SmtpNotifyPluginReport)
                            {
                                if (!ClientUtils.IsEmailValid(SmtpTo, true))
                                {
                                    result = Properties.Resources.ProfileAddEdit_ValidationErrorSmtpTo;
                                }
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private ContextValidation _contextValidation;
        private StackHashContextSettings _profile;
        private ObservableCollection<StackHashContextSettings> _allProfiles;
        private bool _add;
        private ClientLogic _clientLogic;
        private Process _dbConfigProcess;
        private string _originalProfileName;
        private string _originalProfileFolder;
        private string _originalConnectionString;
        private string _originalInitialCatalog;
        private ListViewSorter _listViewPluginsSorter;
        private bool _syncPluginPromptShown;


        /// <summary>
        /// Gets the collection policies to set/update
        /// </summary>
        public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }

        /// <summary>
        /// Gets the collection of plugins to set/update
        /// </summary>
        public StackHashBugTrackerPlugInCollection Plugins { get; private set; }

        /// <summary>
        /// Gets the collection of WorkFlow mappings to set/update
        /// </summary>
        public StackHashMappingCollection WorkFlowMappings { get; private set; }

        /// <summary>
        /// Edits a StackHash profile
        /// </summary>
        /// <param name="profile">The profile to edit</param>
        /// <param name="allProfiles">All current profiles</param>
        /// <param name="add">True if this is a new profile</param>
        /// <param name="clientLogic">ClientLogic</param>
        public ProfileAddEdit(StackHashContextSettings profile, ObservableCollection<StackHashContextSettings> allProfiles, bool add, ClientLogic clientLogic)
        {
            Debug.Assert(profile != null);
            _profile = profile;

            Debug.Assert(allProfiles != null);
            _allProfiles = allProfiles;

            Debug.Assert(clientLogic != null);
            _clientLogic = clientLogic;

            _add = add;

            this.CollectionPolicies = new StackHashCollectionPolicyCollection();
            this.Plugins = new StackHashBugTrackerPlugInCollection();

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
            UserSettings.Settings.RestoreGridView(ListViewPluginsKey, listViewPlugins.View as GridView);

            _listViewPluginsSorter = new ListViewSorter(listViewPlugins);
            _listViewPluginsSorter.AddDefaultSort("Name", ListSortDirection.Ascending);
            _listViewPluginsSorter.AddDefaultSort("Enabled", ListSortDirection.Ascending);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = _add ? Properties.Resources.ProfileAddEdit_TitleAdd : Properties.Resources.ProfileAddEdit_TitleEdit;

            // set shield icon if not an admin
            if (SystemInformation.IsAdmin())
            {
                // no shield if we're an admin...
                imageShield.Visibility = Visibility.Collapsed;
            }
            else
            {
                // .. otherwise get the correct shield icon for the platform
                imageShield.Source = ClientUtils.GetShieldIconAsBitmapSource();
            }

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged += new PropertyChangedEventHandler(_clientLogic_PropertyChanged);
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
            }

            LoadFromProfile();
        }

        private void UpdateState()
        {
            StackHashBugTrackerPlugIn plugin = listViewPlugins.SelectedItem as StackHashBugTrackerPlugIn;

            if (plugin != null)
            {
                buttonPluginSettings.IsEnabled = true;
                buttonRemovePlugin.IsEnabled = true;
                buttonPluginHelp.IsEnabled = plugin.HelpUrl != null;
            }
            else
            {
                buttonPluginSettings.IsEnabled = false;
                buttonRemovePlugin.IsEnabled = false;
                buttonPluginHelp.IsEnabled = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;

                UserSettings.Settings.SaveGridView(ListViewPluginsKey, listViewPlugins.View as GridView);
                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }

        void _clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "NotBusy")
                {
                    gridMain.IsEnabled = _clientLogic.NotBusy;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                switch (e.UIRequest)
                {
                    case ClientLogicUIRequest.TestWinQualLogOnSuccess:
                        StackHashMessageBox.Show(this,
                            Properties.Resources.ProfileAddEdit_WinQualLogOnSuccessMBMessage,
                            Properties.Resources.ProfileAddEdit_WinQualLogOnSuccessMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Information);
                        break;

                    case ClientLogicUIRequest.MoveIndexComplete:
                    case ClientLogicUIRequest.CopyIndexComplete:
                    case ClientLogicUIRequest.ContextCollectionReady:
                        // update settings that may have changed as a result of the move
                        textIndexFolder.Text = DBConfigSettings.Settings.ProfileFolder;
                        textConnectionString.Text = DBConfigSettings.Settings.ConnectionString;

                        _profile.ErrorIndexSettings.Folder = DBConfigSettings.Settings.ProfileFolder;
                        _profile.ErrorIndexSettings.Name = DBConfigSettings.Settings.ProfileName;
                        _profile.SqlSettings.ConnectionString = DBConfigSettings.Settings.ConnectionString;
                        _profile.SqlSettings.InitialCatalog = DBConfigSettings.Settings.ProfileName;
                        break;

                    case ClientLogicUIRequest.DatabaseTestComplete:
                        if (_clientLogic.LastDatabaseTestStatus == StackHashErrorIndexDatabaseStatus.Success)
                        {
                            StackHashMessageBox.Show(this,
                                Properties.Resources.ProfileAddEdit_DatabaseTestSucceededMBMessage,
                                Properties.Resources.ProfileAddEdit_DatabaseTestSucceededMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Information);
                        }
                        else
                        {
                            AdminReportException exceptionWrapper = null;
                            if (!string.IsNullOrEmpty(_clientLogic.LastDatabaseTestExceptionText))
                            {
                                exceptionWrapper = new AdminReportException(_clientLogic.LastDatabaseTestExceptionText);
                            }

                            StackHashMessageBox.Show(this,
                                string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.ProfileAddEdit_DatabaseTestFailedMBMessage,
                                StackHashMessageBox.GetDatabaseStatusMessage(_clientLogic.LastDatabaseTestStatus)),
                                Properties.Resources.ProfileAddEdit_DatabaseTestFailedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error,
                                exceptionWrapper,
                                StackHashServiceErrorCode.NoError);
                        }
                        break;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void buttonBrowseWinDbg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textWinDbg.Text))
            {
                ofd.FileName = textWinDbg.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.ProfileAddEdit_BrowseWinQualTitle32;

            if (ofd.ShowDialog(this) == true)
            {
                textWinDbg.Text = ofd.FileName;
            }
        }

        private void buttonBrowseWinDbg64_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textWinDbg64.Text))
            {
                ofd.FileName = textWinDbg64.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.ProfileAddEdit_BrowseWinQualTitle64;

            if (ofd.ShowDialog(this) == true)
            {
                textWinDbg64.Text = ofd.FileName;
            }
        }

        private bool IsPasswordValid()
        {
            bool valid = true;

            // can't data bind to password so need to handle separately
            if (passPassword.Password.Length > 0)
            {
                rectPassError.Stroke = Brushes.Transparent;
                passPassword.ToolTip = null;
            }
            else
            {
                valid = false;
                rectPassError.Stroke = Brushes.Red;
                passPassword.ToolTip = Properties.Resources.ProfileAddEdit_ValidationErrorPassword;
            }

            return valid;
        }

        private bool IsSmtpPasswordValid()
        {
            return true;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // clear any error formatting to mimic the data bound controls
            rectPassError.Stroke = Brushes.Transparent;
            passPassword.ToolTip = null;

            this.InvalidateVisual();
        }

        private void passPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            IsPasswordValid();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            // enable validation on OK
            _contextValidation.ValidationEnabled = true;

            bool valid = true;

            // check password
            if (!IsPasswordValid())
            {
                valid = false;

                if (tabControl.SelectedItem != tabItemWinQual)
                {
                    tabControl.SelectedItem = tabItemWinQual;
                }

                // highlight any other validation errors on the same tab
                BindingValidator.IsValid(tabItemWinQual);
            }

            // check SMTP password
            if (valid)
            {
                if (!IsSmtpPasswordValid())
                {
                    valid = false;

                    if (tabControl.SelectedItem != tabItemNotifications)
                    {
                        tabControl.SelectedItem = tabItemNotifications;
                    }

                    // highlight any other validation errors on the same tab
                    BindingValidator.IsValid(tabItemNotifications);
                }
            }

            // check sync schedule
            if (valid)
            {
                if (!schSync.OneDaySelected)
                {
                    valid = false;

                    if (tabControl.SelectedItem != tabItemSyncSchedule)
                    {
                        tabControl.SelectedItem = tabItemSyncSchedule;
                    }

                    // highlight any other validation errors on the same tab
                    BindingValidator.IsValid(tabItemSyncSchedule);

                    StackHashMessageBox.Show(this,
                        Properties.Resources.ProfileAddEdit_InvalidScheduleMBMessage,
                        Properties.Resources.ProfileAddEdit_InvalidScheduleMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error);
                }
            }

            // check purge schedule
            if (valid)
            {
                if (!schPurge.OneDaySelected)
                {
                    valid = false;

                    if (tabControl.SelectedItem != tabItemPurgeSchedule)
                    {
                        tabControl.SelectedItem = tabItemPurgeSchedule;
                    }

                    // highlight any other validation errors on the same tab
                    BindingValidator.IsValid(tabItemPurgeSchedule);

                    StackHashMessageBox.Show(this,
                        Properties.Resources.ProfileAddEdit_InvalidScheduleMBMessage,
                        Properties.Resources.ProfileAddEdit_InvalidScheduleMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error);
                }
            }

            // check collection policy
            if (valid)
            {
                if (!collectionPolicyControl.IsValid)
                {
                    valid = false;

                    if (tabControl.SelectedItem != tabItemCollectionPolicy)
                    {
                        tabControl.SelectedItem = tabItemCollectionPolicy;

                        // validate again to higlight errors
                        valid = collectionPolicyControl.IsValid;
                    }
                }
            }

            // check other tabs
            if (valid)
            {
                foreach (TabItem tab in tabControl.Items)
                {
                    if (!BindingValidator.IsValid(tab))
                    {
                        valid = false;

                        // activate the bad tab if it's not the current one
                        if (tabControl.SelectedItem != tab)
                        {
                            tabControl.SelectedItem = tab;

                            // need to validate again to highlight errors
                            BindingValidator.IsValid(tab);
                        }

                        break;
                    }
                }
            }

            if (valid)
            {
                SaveToProfile();
                DialogResult = true;
            }
        }

        private void LoadFromProfile()
        {
            bool smtpNotifySync = false;
            bool smtpNotifyAnalyze = false;
            bool smtpNotifyPurge = false;
            bool smtpNotifyPluginReport = false;
            bool smtpNotifyPluginError = false;

            foreach (StackHashAdminOperation operation in _profile.EmailSettings.OperationsToReport)
            {
                switch (operation)
                {
                    case StackHashAdminOperation.WinQualSyncStarted:
                    case StackHashAdminOperation.WinQualSyncCompleted:
                        smtpNotifySync = true;
                        break;

                    case StackHashAdminOperation.AnalyzeStarted:
                    case StackHashAdminOperation.AnalyzeCompleted:
                        smtpNotifyAnalyze = true;
                        break;

                    case StackHashAdminOperation.PurgeStarted:
                    case StackHashAdminOperation.PurgeCompleted:
                        smtpNotifyPurge = true;
                        break;

                    case StackHashAdminOperation.BugReportStarted:
                    case StackHashAdminOperation.BugReportCompleted:
                        smtpNotifyPluginReport = true;
                        break;

                    case StackHashAdminOperation.BugTrackerPlugInStatus:
                        smtpNotifyPluginError = true;
                        break;
                }
            }

            string[] workFlowStatuses = new string[WorkFlowStatusCount];
            bool workFlowStatusesEnabled;
            if (_profile.WorkFlowMappings == null)
            {
                workFlowStatusesEnabled = false;
            }
            else
            {
                workFlowStatusesEnabled = true;
                this.WorkFlowMappings = _profile.WorkFlowMappings;
                for (int i = 0; i < WorkFlowStatusCount; i++)
                {
                    workFlowStatuses[i] = _profile.WorkFlowMappings[i].Name;
                }
            }

            _contextValidation = new ContextValidation(_profile.WinQualSettings.CompanyName,
                _profile.WinQualSettings.UserName,
                _profile.ErrorIndexSettings.Folder,
                _profile.SqlSettings.ConnectionString,
                _profile.WinQualSettings.AgeOldToPurgeInDays,
                _profile.DebuggerSettings.DebuggerPathAndFileName,
                ClientUtils.SearchPathToString(_profile.DebuggerSettings.SymbolPath),
                ClientUtils.SearchPathToString(_profile.DebuggerSettings.BinaryPath),
                _profile.DebuggerSettings.DebuggerPathAndFileName64Bit,
                ClientUtils.SearchPathToString(_profile.DebuggerSettings.SymbolPath64Bit),
                ClientUtils.SearchPathToString(_profile.DebuggerSettings.BinaryPath64Bit),
                _allProfiles,
                _profile.Id,
                SystemInformation.Is64BitSystem(),
                _profile.AnalysisSettings.ForceRerun,
                _profile.WinQualSettings.RequestRetryCount,
                _profile.WinQualSettings.RequestTimeout / 60000,                // convert ms to mins
                _profile.WinQualSettings.RetryAfterError,
                _profile.WinQualSettings.DelayBeforeRetryInSeconds / 60,        // convert s to mins
                _profile.WinQualSettings.MaxCabDownloadFailuresBeforeAbort,
                _profile.WinQualSettings.SyncsBeforeResync,
                _profile.SqlSettings.MinPoolSize,
                _profile.SqlSettings.MaxPoolSize,
                _profile.SqlSettings.ConnectionTimeout,
                _profile.SqlSettings.EventsPerBlock,
                _profile.EmailSettings.SmtpSettings.SmtpHost,
                _profile.EmailSettings.SmtpSettings.SmtpPort,
                _profile.EmailSettings.SmtpSettings.SmtpUsername,
                _profile.EmailSettings.SmtpSettings.SmtpFrom,
                _profile.EmailSettings.SmtpSettings.SmtpRecipients,
                smtpNotifySync,
                smtpNotifyAnalyze,
                smtpNotifyPurge,
                smtpNotifyPluginReport,
                smtpNotifyPluginError,
                _profile.WinQualSettings.EnableNewProductsAutomatically,
                workFlowStatuses,
                workFlowStatusesEnabled);   

            // can't databind the passwords
            passPassword.Password = _profile.WinQualSettings.Password;
            passSmtpPassword.Password = _profile.EmailSettings.SmtpSettings.SmtpPassword;
            
            this.DataContext = _contextValidation;

            // currently only using one schedule
            if (_profile.WinQualSyncSchedule.Count > 0)
            {
                schSync.UpdateFromSchedule(_profile.WinQualSyncSchedule[0]);
            }

            if (_profile.CabFilePurgeSchedule.Count > 0)
            {
                schPurge.UpdateFromSchedule(_profile.CabFilePurgeSchedule[0]);
            }

            // find and show global collection policies
            StackHashCollectionPolicy globalCabPolicy = null;
            StackHashCollectionPolicy globalEventPolicy = null;

            foreach (StackHashCollectionPolicy policy in _profile.CollectionPolicy)
            {
                if ((policy.RootObject == StackHashCollectionObject.Global) &&
                    (policy.ObjectToCollect == StackHashCollectionObject.Cab))
                {
                    if (policy.ConditionObject == StackHashCollectionObject.Cab)
                    {
                        globalCabPolicy = policy;
                    }
                    else if (policy.ConditionObject == StackHashCollectionObject.Event)
                    {
                        globalEventPolicy = policy;
                    }
                }
            }

            collectionPolicyControl.SetPolicies(globalCabPolicy, globalEventPolicy, StackHashCollectionObject.Global, 0);
            UpdateRetryDelayState();

            // load a local list of plugins
            if ((_profile.BugTrackerSettings != null) && (_profile.BugTrackerSettings.PlugInSettings != null))
            {
                this.Plugins.AddRange(_profile.BugTrackerSettings.PlugInSettings);
            }

            // plugin list not driven by ContextValidation
            listViewPlugins.ItemsSource = this.Plugins;

            UpdateState();
        }

        private void SaveToProfile()
        {
            _profile.WinQualSettings.CompanyName = _contextValidation.Company;
            _profile.WinQualSettings.UserName = _contextValidation.Username;
            _profile.WinQualSettings.AgeOldToPurgeInDays = _contextValidation.PurgeDays;
            _profile.DebuggerSettings.DebuggerPathAndFileName = _contextValidation.WinDbgPath;
            _profile.DebuggerSettings.SymbolPath = ClientUtils.StringToSearchPath(_contextValidation.SymbolPath);
            _profile.DebuggerSettings.BinaryPath = ClientUtils.StringToSearchPath(_contextValidation.ImagePath);
            _profile.DebuggerSettings.DebuggerPathAndFileName64Bit = _contextValidation.WinDbgPath64;
            _profile.DebuggerSettings.SymbolPath64Bit = ClientUtils.StringToSearchPath(_contextValidation.SymbolPath64);
            _profile.DebuggerSettings.BinaryPath64Bit = ClientUtils.StringToSearchPath(_contextValidation.ImagePath64);
            _profile.AnalysisSettings.ForceRerun = _contextValidation.ForceRerun;
            _profile.WinQualSettings.RequestRetryCount = _contextValidation.RequestRetryCount;
            _profile.WinQualSettings.RequestTimeout = _contextValidation.RequestTimeoutMinutes * 60000; // convert back to ms
            _profile.WinQualSettings.DelayBeforeRetryInSeconds = _contextValidation.RetrySyncDelayMinutes * 60; // convert back to s
            _profile.WinQualSettings.MaxCabDownloadFailuresBeforeAbort = _contextValidation.CabDownloadFailureLimit;
            _profile.WinQualSettings.RetryAfterError = _contextValidation.RetrySync;
            _profile.WinQualSettings.SyncsBeforeResync = _contextValidation.SyncsBeforeResync;
            _profile.WinQualSettings.EnableNewProductsAutomatically = _contextValidation.EnableNewProducts;
            _profile.SqlSettings.MinPoolSize = _contextValidation.MinPoolSize;
            _profile.SqlSettings.MaxPoolSize = _contextValidation.MaxPoolSize;
            _profile.SqlSettings.ConnectionTimeout = _contextValidation.ConnectionTimeout;
            _profile.SqlSettings.EventsPerBlock = _contextValidation.EventsPerBlock;
            _profile.EmailSettings.SmtpSettings.SmtpHost = _contextValidation.SmtpServer;
            _profile.EmailSettings.SmtpSettings.SmtpPort = _contextValidation.SmtpPort;
            _profile.EmailSettings.SmtpSettings.SmtpUsername = _contextValidation.SmtpUser;
            _profile.EmailSettings.SmtpSettings.SmtpFrom = _contextValidation.SmtpFrom;
            _profile.EmailSettings.SmtpSettings.SmtpRecipients = _contextValidation.SmtpTo;

            _profile.EmailSettings.OperationsToReport.Clear();
            if (_contextValidation.SmtpNotifySync)
            {
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncStarted);
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncCompleted);
            }
            if (_contextValidation.SmtpNotifyAnalyze)
            {
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.AnalyzeStarted);
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.AnalyzeCompleted);
            }
            if (_contextValidation.SmtpNotifyPurge)
            {
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.PurgeStarted);
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.PurgeCompleted);
            }
            if (_contextValidation.SmtpNotifyPluginReport)
            {
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.BugReportStarted);
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.BugReportCompleted);
            }
            if (_contextValidation.SmtpNotifyPluginError)
            {
                _profile.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.BugTrackerPlugInStatus);
            }

            // can't databind the passwords
            _profile.WinQualSettings.Password = passPassword.Password;
            _profile.EmailSettings.SmtpSettings.SmtpPassword = passSmtpPassword.Password;

            // only SQL supported at present
            _profile.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            _profile.WinQualSyncSchedule.Clear();
            _profile.WinQualSyncSchedule.Add(schSync.SaveToSchedule());

            _profile.CabFilePurgeSchedule.Clear();
            _profile.CabFilePurgeSchedule.Add(schPurge.SaveToSchedule());

            // store collection policies
            StackHashCollectionPolicyCollection policiesToUpdate;
            StackHashCollectionPolicyCollection policiesToRemove;
            collectionPolicyControl.UpdateAndReturnPolicies(out policiesToUpdate, out policiesToRemove);

            Debug.Assert(policiesToRemove.Count == 0);

            this.CollectionPolicies.AddRange(policiesToUpdate);

            // store updated workflow mappings
            if (_contextValidation.WorkFlowStatusesEnabled)
            {
                for (int i = 0; i < WorkFlowStatusCount; i++)
                {
                    this.WorkFlowMappings[i].Name = _contextValidation.WorkFlowStatuses[i];
                }
            }
        }

        private void buttonTestCredentials_Click(object sender, RoutedEventArgs e)
        {
            _clientLogic.AdminTestWinQualLogOn(_profile.Id, _contextValidation.Username, passPassword.Password);
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabControl.SelectedItem == tabItemWinQual)
            {
                StackHashHelp.ShowTopic("add-edit-profile-winqual.htm");
            }
            else if (tabControl.SelectedItem == tabItemSyncSchedule)
            {
                StackHashHelp.ShowTopic("add-edit-profile-sync.htm");
            }
            else if (tabControl.SelectedItem == tabItemPurgeSchedule)
            {
                StackHashHelp.ShowTopic("add-edit-profile-purge.htm");
            }
            else if ((tabControl.SelectedItem == tabItem32BitDebugger) || (tabControl.SelectedItem == tabItem64BitDebugger))
            {
                StackHashHelp.ShowTopic("add-edit-profile-debugger.htm");
            }
            else if (tabControl.SelectedItem == tabItemAdvanced)
            {
                StackHashHelp.ShowTopic("add-edit-profile-advanced.htm");
            }
            else if (tabControl.SelectedItem == tabItemCollectionPolicy)
            {
                StackHashHelp.ShowTopic("add-edit-profile-collection.htm");
            }
            else if (tabControl.SelectedItem == tabItemDatabase)
            {
                StackHashHelp.ShowTopic("add-edit-profile-database.htm");
            }
            else if (tabControl.SelectedItem == tabItemPlugins)
            {
                StackHashHelp.ShowTopic("add-edit-profile-plugins.htm");
            }
            else if (tabControl.SelectedItem == tabItemNotifications)
            {
                StackHashHelp.ShowTopic("add-edit-profile-notifications.htm");
            }
            else if (tabControl.SelectedItem == tabItemStatuses)
            {
                StackHashHelp.ShowTopic("add-edit-profile-statuses.htm");
            }
            else
            {
                StackHashHelp.ShowTopic("add-edit-profile.htm");
            }
        }

        private void UpdateRetryDelayState()
        {
            labelRetryDelay.IsEnabled = checkBoxRetry.IsChecked == true;
            textBoxRetryDelay.IsEnabled = checkBoxRetry.IsChecked == true;
        }

        private void checkBoxRetry_Checked(object sender, RoutedEventArgs e)
        {
            UpdateRetryDelayState();
        }

        private void checkBoxRetry_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateRetryDelayState();
        }

        private void buttonMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // store current settings to detect changes
                _originalConnectionString = _profile.SqlSettings.ConnectionString;
                _originalInitialCatalog = _profile.SqlSettings.InitialCatalog;
                _originalProfileFolder = _profile.ErrorIndexSettings.Folder;
                _originalProfileName = _profile.SqlSettings.InitialCatalog; // this is the database name

                DBConfigSettings.Settings.ResetSettings();
                DBConfigSettings.Settings.IsNewProfile = false;
                DBConfigSettings.Settings.IsUpgrade = false;
                DBConfigSettings.Settings.ServiceHost = ServiceProxy.Services.ServiceHost;
                DBConfigSettings.Settings.ServicePort = ServiceProxy.Services.ServicePort;
                DBConfigSettings.Settings.ServiceUsername = UserSettings.Settings.ServiceUsername;
                DBConfigSettings.Settings.ServicePassword = UserSettings.Settings.ServicePassword;
                DBConfigSettings.Settings.ServiceDomain = UserSettings.Settings.ServiceDomain;

                if (_clientLogic.ContextCollection != null)
                {
                    foreach (DisplayContext existingSettings in _clientLogic.ContextCollection)
                    {
                        DBConfigSettings.Settings.ExistingProfileFolders.Add(existingSettings.StackHashContextSettings.ErrorIndexSettings.Folder);
                        DBConfigSettings.Settings.ExistingProfileNames.Add(existingSettings.StackHashContextSettings.ErrorIndexSettings.Name);
                    }
                }

                DBConfigSettings.Settings.ConnectionString = _profile.SqlSettings.ConnectionString;
                DBConfigSettings.Settings.ProfileFolder = _profile.ErrorIndexSettings.Folder;
                DBConfigSettings.Settings.ProfileName = _profile.SqlSettings.InitialCatalog; // this is the database name
                DBConfigSettings.Settings.IsDatabaseInCabFolder = _profile.ErrorIndexSettings.Location == StackHashErrorIndexLocation.InCabFolder;

                DBConfigSettings.Settings.Save();

                _dbConfigProcess = Process.Start("StackHashDBConfig.exe");
                _dbConfigProcess.EnableRaisingEvents = true;
                _dbConfigProcess.Exited += new EventHandler(_dbConfigProcess_Exited);
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;

                bool userCancel = false;
                Win32Exception win32ex = ex as Win32Exception;
                if (win32ex != null)
                {
                    userCancel = (win32ex.NativeErrorCode == 1223);
                }

                if (!userCancel)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                        "Failed to launch StackHashDBConfig.exe",
                        ex);

                    StackHashMessageBox.Show(this,
                        Properties.Resources.DBConfigLaunchFailedMBMessage,
                        Properties.Resources.DBConfigLaunchFailedMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error,
                        ex,
                        StackHashMessageBox.ParseServiceErrorFromException(ex));
                }
            }
        }

        void _dbConfigProcess_Exited(object sender, EventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                Mouse.OverrideCursor = null;

                int exitCode = -1;

                if (_dbConfigProcess != null)
                {
                    exitCode = _dbConfigProcess.ExitCode;
                    _dbConfigProcess.Close();
                    _dbConfigProcess = null;
                }

                // will be 0 on success
                if (exitCode == 0)
                {
                    // reload DB settings
                    DBConfigSettings.Settings.Load();

                    bool moveIndex = false;

                    if (string.Compare(_originalProfileName, DBConfigSettings.Settings.ProfileName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        moveIndex = true;
                    }

                    if (string.Compare(_originalProfileFolder, DBConfigSettings.Settings.ProfileFolder, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        moveIndex = true;
                    }

                    if (string.Compare(_originalInitialCatalog, DBConfigSettings.Settings.ProfileName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        moveIndex = true;
                    }

                    if (string.Compare(_originalConnectionString, DBConfigSettings.Settings.ConnectionString, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        moveIndex = true;
                    }

                    if (moveIndex)
                    {
                        if (DBConfigSettings.Settings.DatabaseCopyRequired)
                        {
                            // connection string has change, we need to "copy" the index
                            _clientLogic.AdminCopyIndex(_profile.Id,
                                DBConfigSettings.Settings.ProfileFolder,
                                DBConfigSettings.Settings.ProfileName,
                                DBConfigSettings.Settings.ConnectionString,
                                DBConfigSettings.Settings.ProfileName,
                                _profile.IsActive,
                                DBConfigSettings.Settings.IsDatabaseInCabFolder);
                        }
                        else
                        {
                            // connection string is the same, we can "move" the index
                            _clientLogic.AdminMoveIndex(_profile.Id,
                                DBConfigSettings.Settings.ProfileFolder,
                                DBConfigSettings.Settings.ProfileName,
                                DBConfigSettings.Settings.ConnectionString,
                                DBConfigSettings.Settings.ProfileName,
                                _profile.IsActive,
                                _profile.SqlSettings.MinPoolSize,
                                _profile.SqlSettings.MaxPoolSize,
                                _profile.SqlSettings.ConnectionTimeout,
                                _profile.SqlSettings.EventsPerBlock);
                        }
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, EventArgs>(_dbConfigProcess_Exited), sender, e);
            }
        }

        private void buttonTestConnectionString_Click(object sender, RoutedEventArgs e)
        {
            _clientLogic.AdminTestDatabase(_profile.Id);
        }

        private void buttonAddPlugin_Click(object sender, RoutedEventArgs e)
        {
            // build a list of plugins that we could add
            ObservableCollection<StackHashBugTrackerPlugInDiagnostics> availableToAdd = new ObservableCollection<StackHashBugTrackerPlugInDiagnostics>();
            if (_clientLogic.AvailablePlugIns != null)
            {
                bool canAdd = false;
                foreach (StackHashBugTrackerPlugInDiagnostics candidatePlugin in _clientLogic.AvailablePlugIns)
                {
                    canAdd = true;

                    if (candidatePlugin.Loaded)
                    {
                        foreach (StackHashBugTrackerPlugIn configuredPlugin in this.Plugins)
                        {
                            if (string.Compare(candidatePlugin.Name, configuredPlugin.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                // plugin already configured
                                canAdd = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // plugin not loaded
                        canAdd = false;
                    }

                    if (canAdd)
                    {
                        availableToAdd.Add(candidatePlugin);
                    }
                }
            }

            if (availableToAdd.Count > 0)
            {
                // allow the user to select from available plugins
                AddPlugin addPlugin = new AddPlugin(availableToAdd);
                addPlugin.Owner = this;

                if ((addPlugin.ShowDialog() == true) && (addPlugin.SelectedPlugin != null))
                {
                    bool tryingToAddSecondReferenceChanger = false;

                    if (addPlugin.SelectedPlugin.PlugInSetsBugReference)
                    {
                        // make sure we don't already have a reference changer
                        foreach (StackHashBugTrackerPlugIn configuredPlugin in this.Plugins)
                        {
                            if ((configuredPlugin.ChangesBugReference) && (configuredPlugin.Enabled))
                            {
                                tryingToAddSecondReferenceChanger = true;
                                break;
                            }
                        }
                    }

                    if (tryingToAddSecondReferenceChanger)
                    {
                        // let the user know they can't add it
                        StackHashMessageBox.Show(this,
                            Properties.Resources.ProfileAddEdit_MoreThanOneRefChangerMBMessage,
                            Properties.Resources.ProfileAddEdit_MoreThanOneRefChangerMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error);
                    }
                    else
                    {
                        // ok to add the plugin
                        StackHashBugTrackerPlugIn plugin = new StackHashBugTrackerPlugIn();
                        plugin.Enabled = true;
                        plugin.Name = addPlugin.SelectedPlugin.Name;
                        plugin.PlugInDescription = addPlugin.SelectedPlugin.PlugInDescription;
                        plugin.HelpUrl = addPlugin.SelectedPlugin.HelpUrl;
                        plugin.ChangesBugReference = addPlugin.SelectedPlugin.PlugInSetsBugReference;
                        plugin.Properties = new StackHashNameValueCollection();

                        if (addPlugin.SelectedPlugin.DefaultProperties != null)
                        {
                            foreach (StackHashNameValuePair defaultPair in addPlugin.SelectedPlugin.DefaultProperties)
                            {
                                StackHashNameValuePair pair = new StackHashNameValuePair();
                                pair.Name = defaultPair.Name;
                                pair.Value = defaultPair.Value;

                                plugin.Properties.Add(pair);
                            }
                        }

                        this.Plugins.Add(plugin);
                        listViewPlugins.Items.Refresh();

                        if (!_syncPluginPromptShown)
                        {
                            StackHashMessageBox.Show(this,
                                Properties.Resources.ProfileAddEdit_PluginAddedMBMessage,
                                Properties.Resources.ProfileAddEdit_PluginAddedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Information);

                            _syncPluginPromptShown = true;
                        }
                    }
                }
            }
            else
            {
                // let the user know that no more plugins can be added
                StackHashMessageBox.Show(this,
                    Properties.Resources.ProfileAddEdit_NoMorePluginsMBMessage,
                    Properties.Resources.ProfileAddEdit_NoMorePluginsMBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Information);
            }
        }

        private void DoEditPlugin()
        {
            StackHashBugTrackerPlugIn plugin = listViewPlugins.SelectedItem as StackHashBugTrackerPlugIn;
            if (plugin != null)
            {
                // create a copy of existing settings so we can discard changes on cancel
                StackHashNameValueCollection settings = new StackHashNameValueCollection();
                foreach (StackHashNameValuePair existingSetting in plugin.Properties)
                {
                    StackHashNameValuePair newSetting = new StackHashNameValuePair();
                    newSetting.Name = existingSetting.Name;
                    newSetting.Value = existingSetting.Value;
                    settings.Add(newSetting);
                }

                PluginSettings pluginSettings = new PluginSettings(plugin.Name, 
                    settings, 
                    plugin.HelpUrl == null ? null : plugin.HelpUrl.ToString());

                pluginSettings.Owner = this;
                if (pluginSettings.ShowDialog() == true)
                {
                    // store the (possibly) updated settings on OK
                    plugin.Properties = settings;
                }
            }
        }

        private void buttonPluginSettings_Click(object sender, RoutedEventArgs e)
        {
            DoEditPlugin();
        }

        private void buttonRemovePlugin_Click(object sender, RoutedEventArgs e)
        {
            StackHashBugTrackerPlugIn plugin = listViewPlugins.SelectedItem as StackHashBugTrackerPlugIn;
            if (plugin != null)
            {
                if (StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ProfileAddEdit_RemovePluginMBMessage,
                    plugin.Name),
                    Properties.Resources.ProfileAddEdit_RemovePluginMBTitle,
                    StackHashMessageBoxType.YesNo,
                    StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
                {
                    this.Plugins.Remove(plugin);
                    listViewPlugins.Items.Refresh();
                }
            }
        }

        private void listViewPlugins_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewPluginsSorter.SortColumn(header);
            }
        }

        private void listViewPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateState();
        }

        private void listViewPlugins_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientUtils.OriginalSourceIsListViewItem(e.OriginalSource))
            {
                DoEditPlugin();
            }
        }

        private void buttonPluginDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            PluginDiagnostics pluginDiagnostics = new PluginDiagnostics(_clientLogic, _profile.Id, _profile.IsActive);
            pluginDiagnostics.Owner = this;
            pluginDiagnostics.ShowDialog();
        }

        private void buttonPluginHelp_Click(object sender, RoutedEventArgs e)
        {
            StackHashBugTrackerPlugIn plugin = listViewPlugins.SelectedItem as StackHashBugTrackerPlugIn;
            if ((plugin != null) && (plugin.HelpUrl != null))
            {
                DefaultBrowser.OpenUrl(plugin.HelpUrl.ToString());
            }
        }

        private void PluginEnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = e.OriginalSource as CheckBox;
            if (checkBox != null)
            {
                StackHashBugTrackerPlugIn plugin = checkBox.DataContext as StackHashBugTrackerPlugIn;
                if ((plugin != null) && (plugin.ChangesBugReference))
                {
                    bool tryingToEnableSecondBugTrackerPlugin = false;

                    foreach (StackHashBugTrackerPlugIn configuredPlugin in this.Plugins)
                    {
                        // don't care about the plugin we just checked
                        if (configuredPlugin == plugin)
                        {
                            continue;
                        }

                        if ((configuredPlugin.ChangesBugReference) && (configuredPlugin.Enabled))
                        {
                            tryingToEnableSecondBugTrackerPlugin = true;
                            break;
                        }
                    }

                    if (tryingToEnableSecondBugTrackerPlugin)
                    {
                        plugin.Enabled = false;

                        StackHashMessageBox.Show(this,
                            Properties.Resources.ProfileAddEdit_EnableMoreThanOneRefChangeMBMessage,
                            Properties.Resources.ProfileAddEdit_EnableMoreThanOneRefChangerMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error);
                    }
                }
            }
        }

        private void passSmtpPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            IsSmtpPasswordValid();
        }
    }
}
