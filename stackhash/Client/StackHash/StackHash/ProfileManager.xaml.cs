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
using System.Collections.ObjectModel;
using StackHashUtilities;
using System.ComponentModel;
using System.Collections.Specialized;

namespace StackHash
{
    /// <summary>
    /// Allows the user to add, edit and delete profiles as well as setting the active profile
    /// </summary>
    public partial class ProfileManager : Window
    {
        private const string ListViewProfilesKey = "ProfileManager.listViewProfiles";
        private const string WindowKey = "ProfileManager";

        private class ContextValidation : IDataErrorInfo
        {
            public int ClientTimeoutInMinutes { get; set; }
            public bool ValidationEnabled { get; set; }

            public ContextValidation(int clientTimeoutInMinutes)
            {
                this.ClientTimeoutInMinutes = clientTimeoutInMinutes;
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
                        case "ClientTimeoutInMinutes":
                            if (this.ClientTimeoutInMinutes <= 0)
                            {
                                result = Properties.Resources.ProfileManager_ValidationErrorClientTimeout;
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private enum ExpectedAction
        {
            None,
            ActivateProfile,
            DeactivateProfile,
            ActivateLog,
            DeactivateLog,
            ActivateReporting,
            DeactivateReporting
        }

        private ClientLogic _clientLogic;
        private ObservableCollection<StackHashContextSettings> _allContexts;
        private ListViewSorter _listViewSorter;
        private bool _updatingLogAndReportingStatus;
        private bool _updatingProfileStatus;
        private bool _activateRequired;
        private int _activateContextId;
        private bool _haveContexts;
        private StackHashContextSettings _currentAddSettings;
        private Process _dbConfigProcess;
        private ExpectedAction _currentExpectedAction;
        private ContextValidation _contextValidation;
        private int _activeContextIdOnLoad;

        /// <summary>
        /// Allows the user to add, edit and delete profiles as well as setting the active profile
        /// </summary>
        public ProfileManager(ClientLogic clientLogic)
        {
            Debug.Assert(clientLogic != null);
            _clientLogic = clientLogic;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
            UserSettings.Settings.RestoreGridView(ListViewProfilesKey, listViewProfiles.View as GridView);

            _listViewSorter = new ListViewSorter(listViewProfiles);
            _listViewSorter.AddDefaultSort("ProfileName", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("IsActive", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("CurrentErrorText", ListSortDirection.Ascending);

            this.DataContext = _clientLogic;
        }

        private void UpdateState()
        {
            bool showErrorDetails = false;

            // update the list of all contexts
            if ((_clientLogic.ContextCollection != null) && (_clientLogic.ContextCollection.Count > 0))
            {
                // cache the new context list
                _allContexts = new ObservableCollection<StackHashContextSettings>();
                foreach (DisplayContext contextSettings in _clientLogic.ContextCollection)
                {
                    _allContexts.Add(contextSettings.StackHashContextSettings);

                    // show the error details button if any context has an error
                    if (contextSettings.CurrentError != StackHashServiceErrorCode.NoError)
                    {
                        showErrorDetails = true;
                    }
                }

                _haveContexts = true;

                // if no current context Id try to select the first active one
                if (UserSettings.Settings.CurrentContextId == UserSettings.InvalidContextId)
                {
                    foreach (DisplayContext contextSettings in _clientLogic.ContextCollection)
                    {
                        if (contextSettings.IsActive)
                        {
                            UserSettings.Settings.CurrentContextId = contextSettings.Id;
                            break;
                        }
                    }
                }
            }
            else
            {
                // if no context collection use an empty list
                _allContexts = new ObservableCollection<StackHashContextSettings>();
                _haveContexts = false;
            }

            DisplayContext context = listViewProfiles.SelectedItem as DisplayContext;
            if (context == null)
            {
                buttonEdit.IsEnabled = false;
                buttonDelete.IsEnabled = false;
                buttonTestData.IsEnabled = false;
                buttonErrorDetails.IsEnabled = false;
            }
            else
            {
                buttonEdit.IsEnabled = true;
                buttonDelete.IsEnabled = true;
                buttonTestData.IsEnabled = true;
                buttonErrorDetails.IsEnabled = context.CurrentError != StackHashServiceErrorCode.NoError;
            }

            if (showErrorDetails)
            {
                buttonErrorDetails.Visibility = Visibility.Visible;
            }
            else
            {
                buttonErrorDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateProyControl()
        {
            ProxySettings proxySettings;

            if (_clientLogic.ServiceProxySettings != null)
            {
                proxySettings = new ProxySettings(_clientLogic.ServiceProxySettings.UseProxy,
                    _clientLogic.ServiceProxySettings.UseProxyAuthentication,
                    _clientLogic.ServiceProxySettings.ProxyHost,
                    _clientLogic.ServiceProxySettings.ProxyPort,
                    _clientLogic.ServiceProxySettings.ProxyUserName,
                    _clientLogic.ServiceProxySettings.ProxyPassword,
                    _clientLogic.ServiceProxySettings.ProxyDomain);
            }
            else
            {
                proxySettings = new ProxySettings(false, false, string.Empty, UserSettings.DefaultProxyServerPort, string.Empty, string.Empty, string.Empty);
            }

            proxySettingsControl.ProxySettings = proxySettings;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _activeContextIdOnLoad = UserSettings.Settings.CurrentContextId;

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

            _contextValidation = new ContextValidation(_clientLogic.ClientTimeoutInSeconds / 60);
            tabItemAdvanced.DataContext = _contextValidation;

            UpdateProyControl();

            buttonTestData.Visibility = Visibility.Collapsed;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
                _clientLogic.ClientLogicError += new EventHandler<ClientLogicErrorEventArgs>(_clientLogic_ClientLogicError);
                _clientLogic.RefreshContextSettings();
            }
        }

        void _clientLogic_ClientLogicError(object sender, ClientLogicErrorEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                _updatingLogAndReportingStatus = true;
                _updatingProfileStatus = true;

                switch (_currentExpectedAction)
                {
                    case ExpectedAction.ActivateProfile:
                    case ExpectedAction.DeactivateProfile:
                        listViewProfiles.Items.Refresh();
                        break;

                    case ExpectedAction.ActivateLog:
                        checkBoxServiceLog.IsChecked = false;
                        break;

                    case ExpectedAction.DeactivateLog:
                        checkBoxServiceLog.IsChecked = true;
                        break;

                    case ExpectedAction.ActivateReporting:
                        // reporting removed
                        break;

                    case ExpectedAction.DeactivateReporting:
                        // reporting removed
                        break;
                }

                _updatingLogAndReportingStatus = false;
                _updatingProfileStatus = false;

                _currentExpectedAction = ExpectedAction.None;

                UpdateState();
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicErrorEventArgs>(_clientLogic_ClientLogicError), sender, e);
            }
        }

        private void UpdateContextSettings()
        {
            _clientLogic.RefreshContextSettings();
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                // reset expected action
                _currentExpectedAction = ExpectedAction.None;

                switch (e.UIRequest)
                {
                    case ClientLogicUIRequest.ProxySettingsUpdated:
                        // before closing try to reselect the active context Id on load
                        if (UserSettings.Settings.CurrentContextId != _activeContextIdOnLoad)
                        {
                            if ((_clientLogic.ContextCollection != null) && (_clientLogic.ContextCollection.Count > 0))
                            {
                                foreach (DisplayContext contextSettings in _clientLogic.ContextCollection)
                                {
                                    if ((contextSettings.IsActive) && (contextSettings.Id == _activeContextIdOnLoad))
                                    {
                                        UserSettings.Settings.CurrentContextId = contextSettings.Id;
                                        break;
                                    }
                                }
                            }
                        }

                        // proxy settings updated on exit so close when complete
                        this.DialogResult = true;
                        break;

                    case ClientLogicUIRequest.ContextCollectionReady:
                        UpdateState();
                        UpdateProyControl();

                        _updatingLogAndReportingStatus = true;
                        _updatingProfileStatus = true;

                        checkBoxServiceLog.IsChecked = _clientLogic.ServiceLogEnabled;
                        listViewProfiles.Items.Refresh();

                        _updatingProfileStatus = false;
                        _updatingLogAndReportingStatus = false;

                        if (_activateRequired)
                        {
                            _activateRequired = false;
                            _clientLogic.AdminActivateContext(_activateContextId);
                        }
                        break;

                    case ClientLogicUIRequest.NewContextSettingsReady:
                        // get DB settings for the add operation - BeginInvoke to free up ClientLogic
                        this.Dispatcher.BeginInvoke(new Action(RunDBConfigForAdd));
                        break;

                    case ClientLogicUIRequest.MoveIndexComplete:
                        // update contexts - BeginInvoke to free up ClientLogic
                        this.Dispatcher.BeginInvoke(new Action(UpdateContextSettings));
                        break;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void RunDBConfigForAdd()
        {
            _currentAddSettings = _clientLogic.AdminCollectNewContext();
            Debug.Assert(_currentAddSettings != null);

            if (_currentAddSettings != null)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    DBConfigSettings.Settings.ResetSettings();
                    DBConfigSettings.Settings.IsNewProfile = true;
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

                    if (userCancel)
                    {
                        // user cancelled DB config
                        DeleteContextCore(_currentAddSettings.Id);
                        _currentAddSettings = null;
                    }
                    else
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

                    _currentAddSettings.ErrorIndexSettings.Folder = DBConfigSettings.Settings.ProfileFolder;
                    _currentAddSettings.ErrorIndexSettings.Name = DBConfigSettings.Settings.ProfileName;
                    _currentAddSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
                    _currentAddSettings.ErrorIndexSettings.Location = DBConfigSettings.Settings.IsDatabaseInCabFolder ? StackHashErrorIndexLocation.InCabFolder : StackHashErrorIndexLocation.OnSqlServer;

                    _currentAddSettings.SqlSettings.ConnectionString = DBConfigSettings.Settings.ConnectionString;
                    _currentAddSettings.SqlSettings.InitialCatalog = DBConfigSettings.Settings.ProfileName;

                    _currentAddSettings.WinQualSettings.CompanyName = DBConfigSettings.Settings.ProfileName;

                    CompleteAdd();
                }
                else
                {
                    // user cancelled DB config
                    DeleteContextCore(_currentAddSettings.Id);
                    _currentAddSettings = null;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, EventArgs>(_dbConfigProcess_Exited), sender, e);
            }
        }

        private void CompleteAdd()
        {
            Debug.Assert(_currentAddSettings != null);

            if (_currentAddSettings != null)
            {
                ProfileAddEdit profileAdd = new ProfileAddEdit(_currentAddSettings, _allContexts, true, _clientLogic);

                profileAdd.Owner = this;

                if (profileAdd.ShowDialog() == true)
                {
                    // ask the user if they want to activate this profile
                    if (StackHashMessageBox.Show(this,
                        string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.ProfileManager_ActivateProfileMBMessage,
                        _currentAddSettings.WinQualSettings.CompanyName),
                        Properties.Resources.ProfileManager_ActivateProfileMBTitle,
                        StackHashMessageBoxType.YesNo,
                        StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
                    {
                        _activateRequired = true;
                        _activateContextId = _currentAddSettings.Id;

                        // if no contexts or no current context then also make this Id the current client context
                        if ((!_haveContexts) || (UserSettings.Settings.CurrentContextId == UserSettings.InvalidContextId))
                        {
                            UserSettings.Settings.CurrentContextId = _currentAddSettings.Id;
                        }
                    }

                    _clientLogic.SaveContextSettings(_currentAddSettings.Id,
                        _currentAddSettings, 
                        profileAdd.CollectionPolicies, 
                        profileAdd.Plugins, 
                        profileAdd.WorkFlowMappings,
                        _currentAddSettings.IsActive);
                }
                else
                {
                    DeleteContextCore(_currentAddSettings.Id);
                }

                _currentAddSettings = null;
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            _clientLogic.AdminCreateNewContext();
        }

        private void DoEdit()
        {
            DisplayContext context = listViewProfiles.SelectedItem as DisplayContext;   
            
            if (context != null)
            {
                StackHashContextSettings contextSettings = context.StackHashContextSettings;

                ProfileAddEdit profileEdit = new ProfileAddEdit(contextSettings, _allContexts, false, _clientLogic);
                profileEdit.Owner = this;
                if (profileEdit.ShowDialog() == true)
                {
                    _clientLogic.SaveContextSettings(contextSettings.Id,
                        contextSettings, 
                        profileEdit.CollectionPolicies,
                        profileEdit.Plugins,
                        profileEdit.WorkFlowMappings,
                        contextSettings.IsActive);
                }
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            DoEdit();
        }

        private void DeleteContextCore(int contextId)
        {
            // if the current context is deleted then go back to the invalid state
            if (UserSettings.Settings.CurrentContextId == contextId)
            {
                UserSettings.Settings.CurrentContextId = UserSettings.InvalidContextId;
            }

            // remove the context
            _clientLogic.AdminRemoveContext(contextId);
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            DisplayContext context = listViewProfiles.SelectedItem as DisplayContext;
            Debug.Assert(context != null);

            if (StackHashMessageBox.Show(this,
                string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.ProfileManager_DeleteProfileMBMessage,
                context.StackHashContextSettings.WinQualSettings.CompanyName,
                context.StackHashContextSettings.ErrorIndexSettings.Folder),
                Properties.Resources.ProfileManager_DeleteProfileMBTitle,
                StackHashMessageBoxType.YesNo,
                StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
            {
                DeleteContextCore(context.Id);
            }
        }

        private void listViewProfiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientUtils.OriginalSourceIsListViewItem(e.OriginalSource))
            {
                DoEdit();
            }
        }

        private void listViewProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateState();
        }

        private void ProfileActive_Checked(object sender, RoutedEventArgs e)
        {
            if (!_updatingProfileStatus)
            {
                CheckBox checkBox = e.OriginalSource as CheckBox;
                DisplayContext context = checkBox.DataContext as DisplayContext;
                Debug.Assert(context != null);

                _currentExpectedAction = ExpectedAction.ActivateProfile;
                _clientLogic.AdminActivateContext(context.Id);
            }
        }

        private void ProfileActive_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_updatingProfileStatus)
            {
                CheckBox checkBox = e.OriginalSource as CheckBox;
                DisplayContext context = checkBox.DataContext as DisplayContext;
                Debug.Assert(context != null);

                // if this is the selected profile then clear the selection
                if (context.Id == UserSettings.Settings.CurrentContextId)
                {
                    UserSettings.Settings.CurrentContextId = UserSettings.InvalidContextId;
                }

                _currentExpectedAction = ExpectedAction.DeactivateProfile;
                _clientLogic.AdminDeactivateContext(context.Id);
            }
        }

        private void checkBoxServiceLog_Checked(object sender, RoutedEventArgs e)
        {
            // dont' respond during initial update
            if (!_updatingLogAndReportingStatus)
            {
                _currentExpectedAction = ExpectedAction.ActivateLog;
                _clientLogic.AdminEnableLogging();
            }
        }

        private void checkBoxServiceLog_Unchecked(object sender, RoutedEventArgs e)
        {
            // dont' respond during initial update
            if (!_updatingLogAndReportingStatus)
            {
                _currentExpectedAction = ExpectedAction.DeactivateLog;
                _clientLogic.AdminDisableLogging();
            }
        }

        private void listViewProfiles_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewSorter.SortColumn(header);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
                _clientLogic.ClientLogicError -= _clientLogic_ClientLogicError;

                UserSettings.Settings.SaveGridView(ListViewProfilesKey, listViewProfiles.View as GridView);
                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            _contextValidation.ValidationEnabled = true;

            if (BindingValidator.IsValid(tabItemAdvanced))
            {
                if (proxySettingsControl.IsValid)
                {
                    // update proxy settings - dialog will close when complete
                    StackHashProxySettings proxySettings = new StackHashProxySettings();
                    proxySettings.UseProxy = proxySettingsControl.ProxySettings.UseProxy;
                    proxySettings.UseProxyAuthentication = proxySettingsControl.ProxySettings.UseProxyAuthentication;
                    proxySettings.ProxyHost = proxySettingsControl.ProxySettings.ProxyHost;
                    proxySettings.ProxyPort = proxySettingsControl.ProxySettings.ProxyPort;
                    proxySettings.ProxyUserName = proxySettingsControl.ProxySettings.ProxyUsername;
                    proxySettings.ProxyPassword = proxySettingsControl.ProxySettings.ProxyPassword;
                    proxySettings.ProxyDomain = proxySettingsControl.ProxySettings.ProxyDomain;
                    _clientLogic.ServiceProxySettings = proxySettings;

                    _clientLogic.ClientTimeoutInSeconds = _contextValidation.ClientTimeoutInMinutes * 60;

                    _clientLogic.AdminUpdateServiceProxySettingsAndClientTimeout();
                }
                else
                {
                    // if necessary select the proxy tab and highlight the error
                    if (tabControl.SelectedItem != tabItemProxyServer)
                    {
                        tabControl.SelectedItem = tabItemProxyServer;
                        bool unused = proxySettingsControl.IsValid;
                    }
                }
            }
            else
            {
                // advanced tab not valid
                if (tabControl.SelectedItem != tabItemAdvanced)
                {
                    tabControl.SelectedItem = tabItemAdvanced;
                    bool unused = BindingValidator.IsValid(tabItemAdvanced);
                }
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabControl.SelectedItem == tabItemProfiles)
            {
                StackHashHelp.ShowTopic("service-options-profiles.htm");
            }
            else if (tabControl.SelectedItem == tabItemProxyServer)
            {
                StackHashHelp.ShowTopic("service-options-proxy-server.htm");
            }
            else if (tabControl.SelectedItem == tabItemTroubleshooting)
            {
                StackHashHelp.ShowTopic("service-options-troubleshooting.htm");
            }
            else if (tabControl.SelectedItem == tabItemAdvanced)
            {
                StackHashHelp.ShowTopic("service-options-advanced.htm");
            }
            else
            {
                StackHashHelp.ShowTopic("service-options.htm");
            }
        }

        private void checkBoxServiceReporting_Checked(object sender, RoutedEventArgs e)
        {
            if (!_updatingLogAndReportingStatus)
            {
                _currentExpectedAction = ExpectedAction.ActivateReporting;
                _clientLogic.AdminUpdateReportingState(true);
            }
        }

        private void checkBoxServiceReporting_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_updatingLogAndReportingStatus)
            {
                _currentExpectedAction = ExpectedAction.DeactivateReporting;
                _clientLogic.AdminUpdateReportingState(false);
            }
        }

        private void buttonTestData_Click(object sender, RoutedEventArgs e)
        {
            DisplayContext context = listViewProfiles.SelectedItem as DisplayContext;
            if (context != null)
            {
                StackHashContextSettings contextSettings = context.StackHashContextSettings;

                AddTestData addTestData = new AddTestData(_clientLogic, contextSettings);
                addTestData.Owner = this;
                addTestData.ShowDialog();
            }
        }

        private void buttonAdd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((Mouse.RightButton == MouseButtonState.Pressed) &&
                ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift))))
            {
                buttonTestData.Visibility = Visibility.Visible;
                e.Handled = true;
            }
        }

        private void buttonErrorDetails_Click(object sender, RoutedEventArgs e)
        {
            DisplayContext context = listViewProfiles.SelectedItem as DisplayContext;
            if (context != null)
            {
                StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ProfileManager_ErrorDetailsMBMessage,
                    context.ProfileName,
                    context.CurrentErrorText,
                    context.LastContextException),
                    Properties.Resources.ProfileManager_ErrorDetailsMBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Information,
                    new AdminReportException(context.LastContextException),
                    context.CurrentError);
            }
        }
    }
}
