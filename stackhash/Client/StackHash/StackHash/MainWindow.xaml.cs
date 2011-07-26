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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;
using System.ComponentModel;
using System.Diagnostics;
using StackHash.StackHashService;
using StackHashUtilities;
using System.Globalization;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Threading;

namespace StackHash
{
    /// <summary>
    /// Main (entry) window for StackHash
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class MainWindow : Window , IDisposable
    {
        private const string MainWindowKey = "MainWindow";
        private const string ToolBarMainKey = "MainWindow.toolBarMain";
        private const string ToolBarSearchKey = "MainWindow.toolBarSearch";
        private const string ToolBarToolsKey = "MainWindow.toolBarTools";

        private enum ActionOnCabExtract
        {
            None,
            Debug,
            ExtractNextCab,
            ShowLocalFolder
        }

        private enum NextUriStep
        {
            DetectAndSelectContext,
            SelectProduct,
            FindEvent,
            SelectEvent,
            SelectCab
        }

        private ClientLogic _clientLogic;
        private string _lastSearch;
        private string _lastExportPath;
        private DefaultDebugger _nextDebugger;
        private DumpArchitecture _nextArchitecture;
        private ActionOnCabExtract _actionOnCabExtract;
        private bool _scriptRunFromCommand;
        private DependencyObject _editMenuFocusScope;
        private DependencyObject _editToolbarFocusScope;
        private Storyboard _busyAnimation;
        private object _serviceErrorMessageLock;
        private Queue<StackHashCab> _cabsToExtract;
        private string _cabExtractFolder;
        private bool _contextInactiveWindowOpen;
        private bool _setupWizardWasProfileOnly;
        private NextUriStep _nextUriStep;

        /// <summary>
        /// Main (entry) window for StackHash
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _serviceErrorMessageLock = new object();
            _nextUriStep = NextUriStep.DetectAndSelectContext;

            if (UserSettings.Settings.HaveWindow(MainWindowKey))
            {
                UserSettings.Settings.RestoreWindow(MainWindowKey, this);
            }
            else
            {
                // default to taking up most of the screen
                this.Width = SystemParameters.PrimaryScreenWidth - Properties.Settings.Default.MainWindowDefaultMargin;
                this.Height = SystemParameters.PrimaryScreenHeight - Properties.Settings.Default.MainWindowDefaultMargin;
            }

            UserSettings.Settings.RestoreToolBar(ToolBarMainKey, toolBarMain);
            UserSettings.Settings.RestoreToolBar(ToolBarSearchKey, toolBarSearch);
            UserSettings.Settings.RestoreToolBar(ToolBarToolsKey, toolBarTools);

            // create client logic
            _clientLogic = UserSettings.Settings.CreateClientLogic(this.Dispatcher);
            this.DataContext = _clientLogic;
            comboBoxSearch.DataContext = UserSettings.Settings;

            // hook up commands
            InitializeCommandBindings();
        }

        private void DoNextUriStep()
        {
            App app = App.Current as App;
            if (app.CurrentStackHashUri != null)
            {
                bool navigationComplete = false;

                switch (_nextUriStep)
                {
                    case NextUriStep.DetectAndSelectContext:
                        if (UserSettings.Settings.CurrentContextId == app.CurrentStackHashUri.ContextId)
                        {
                            // we're already at this context Id, select the product if specified
                            if (app.CurrentStackHashUri.ProductId == null)
                            {
                                navigationComplete = true;
                            }
                            else
                            {
                                // select the product
                                navigationComplete = DoSelectProduct(app.CurrentStackHashUri);
                            }
                        }
                        else
                        {
                            // see if we can switch to the requested context id
                            bool canSwitch = false;
                            if (_clientLogic.ActiveContextCollection != null)
                            {
                                foreach (DisplayContext context in _clientLogic.ActiveContextCollection)
                                {
                                    if (context.Id == app.CurrentStackHashUri.ContextId)
                                    {
                                        canSwitch = true;
                                        break;
                                    }
                                }
                            }

                            if (canSwitch)
                            {
                                // switch to the requested context
                                if (app.CurrentStackHashUri.ProductId == null)
                                {
                                    navigationComplete = true;
                                }
                                else
                                {
                                    _nextUriStep = NextUriStep.SelectProduct;
                                }

                                UserSettings.Settings.CurrentContextId = app.CurrentStackHashUri.ContextId;
                                _clientLogic.CheckForContextIdChange();
                            }
                            else
                            {
                                // navigation failure...
                                string profileName = app.CurrentStackHashUri.ContextId.ToString();
                                bool profileKnown = false;
                                if (_clientLogic.ContextCollection != null)
                                {
                                    foreach (DisplayContext context in _clientLogic.ContextCollection)
                                    {
                                        if (context.Id == app.CurrentStackHashUri.ContextId)
                                        {
                                            profileName = context.ProfileName;
                                            profileKnown = true;
                                            break;
                                        }
                                    }
                                }

                                string messageTemplate = profileKnown ? Properties.Resources.NavigationFailed_InactiveProfile_MBMessage : 
                                    Properties.Resources.NavigationFailed_UnknownProfile_MBMessage;

                                StackHashMessageBox.Show(this,
                                    string.Format(CultureInfo.CurrentCulture,
                                    messageTemplate,
                                    app.CurrentStackHashUri.RawUri,
                                    profileName),
                                    Properties.Resources.NavigationFailed_MBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error);

                                navigationComplete = true;
                            }
                        }
                        break;

                    case NextUriStep.SelectProduct:
                        navigationComplete = DoSelectProduct(app.CurrentStackHashUri);
                        break;

                    case NextUriStep.FindEvent:
                        _nextUriStep = NextUriStep.SelectEvent;
                        comboBoxSearch.Text = string.Format(CultureInfo.InvariantCulture,
                            "eid={0} etype:\"{1}\"",
                            app.CurrentStackHashUri.EventId,
                            app.CurrentStackHashUri.EventType);
                        DoSearch();
                        break;

                    case NextUriStep.SelectEvent:
                        DisplayEventPackage eventToSelect = null;
                        if (_clientLogic.EventPackages != null)
                        {
                            foreach(DisplayEventPackage eventPackage in _clientLogic.EventPackages)
                            {
                                if ((eventPackage.Id == app.CurrentStackHashUri.EventId) &&
                                    (eventPackage.EventTypeName == app.CurrentStackHashUri.EventType))
                                {
                                    eventToSelect = eventPackage;
                                    break;
                                }
                            }
                        }

                        if (eventToSelect != null)
                        {
                            if (app.CurrentStackHashUri.CabId == null)
                            {
                                navigationComplete = true;
                            }
                            else
                            {
                                _nextUriStep = NextUriStep.SelectCab;
                            }

                            viewEventList.SelectAndShowEventDetails(eventToSelect,
                                _nextUriStep == NextUriStep.SelectCab);
                        }
                        else
                        {
                            StackHashMessageBox.Show(this,
                                string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.NavigationFailed_UnknownEvent_MBMessage,
                                app.CurrentStackHashUri.RawUri,
                                app.CurrentStackHashUri.EventId,
                                app.CurrentStackHashUri.EventType),
                                Properties.Resources.NavigationFailed_MBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error);

                            navigationComplete = true;
                        }
                        break;

                    case NextUriStep.SelectCab:
                        DisplayCab cabToSelect = null;
                        if ((_clientLogic.CurrentEventPackage != null) && (_clientLogic.CurrentEventPackage.Cabs != null))
                        {
                            foreach (DisplayCab cab in _clientLogic.CurrentEventPackage.Cabs)
                            {
                                if (cab.Id == app.CurrentStackHashUri.CabId)
                                {
                                    cabToSelect = cab;
                                    break;
                                }
                            }
                        }

                        if (cabToSelect != null)
                        {
                            viewEventDetails.SelectCab(cabToSelect);
                        }
                        else
                        {
                            StackHashMessageBox.Show(this,
                                string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.NavigationFailed_UnknownCab_MBMessage,
                                app.CurrentStackHashUri.RawUri,
                                app.CurrentStackHashUri.CabId),
                                Properties.Resources.NavigationFailed_MBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error);
                        }

                        navigationComplete = true;
                        break;
                }

                // got back to waiting for a Url
                if (navigationComplete)
                {
                    app.CurrentStackHashUri = null;
                    _nextUriStep = NextUriStep.DetectAndSelectContext;
                }
            }
        }

        private bool DoSelectProduct(StackHashUri currentUri)
        {
            bool navigationComplete = false;

            bool foundProduct = false;
            bool productEnabled = false;
            string productName = string.Format(CultureInfo.CurrentCulture, "{0}", currentUri.ProductId);
            DisplayProduct productToSelect = null;

            if (_clientLogic.Products != null)
            {
                foreach (DisplayProduct product in _clientLogic.Products)
                {
                    if (product.Id == currentUri.ProductId)
                    {
                        foundProduct = true;
                        productToSelect = product;
                        productEnabled = product.SynchronizeEnabled;
                        productName = product.NameAndVersion;
                        break;
                    }
                }
            }

            if (foundProduct && productEnabled)
            {
                if (currentUri.EventId == null)
                {
                    navigationComplete = true;
                }
                else
                {
                    _nextUriStep = NextUriStep.FindEvent;
                }

                _clientLogic.CurrentView = ClientLogicView.ProductList;
                viewMainProductList.SelectProduct(productToSelect);
            }
            else
            {
                string messageTemplate = foundProduct ? Properties.Resources.NavigationFailed_InactiveProduct_MBMessage :
                    Properties.Resources.NavigationFailed_UnknownProduct_MBMessage;

                StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    messageTemplate,
                    currentUri.RawUri,
                    productName),
                    Properties.Resources.NavigationFailed_MBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Error);

                navigationComplete = true;
            }

            return navigationComplete;
        }
        
        private void DoProfileSettings()
        {
            if (_clientLogic.ServiceIsLocal)
            {
                ProfileManager profileManager = new ProfileManager(_clientLogic);
                profileManager.Owner = this;
                profileManager.ShowDialog();

                DoSaveUserSettings();
                comboBoxProfile.Items.Refresh();

                // check to see if the current context ID was updated
                _clientLogic.CheckForContextIdChange();
            }
            else
            {
                StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ServiceOptionsUnavailable_MBMessage,
                    _clientLogic.ServiceHost),
                    Properties.Resources.ServiceOptionsUnavailable_MBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Information);
            }
        }

        private void DoLocalSettings(bool fromMenu)
        {
            int currentDefaultDisplayFilter = UserSettings.Settings.GetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId);

            int currentEventPageSize = UserSettings.Settings.EventPageSize;

            OptionsWindow optionsWindow = new OptionsWindow(_clientLogic);
            optionsWindow.Owner = this;
            optionsWindow.ShowDialog();

            DoSaveUserSettings();
            comboBoxProfile.Items.Refresh();

            if (currentDefaultDisplayFilter != UserSettings.Settings.GetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId))
            {
                _clientLogic.CurrentView = ClientLogicView.ProductList;
                viewMainProductList.ClearSelection();

                // update display filters for all products
                if (_clientLogic.Products != null)
                {
                    foreach (DisplayProduct product in _clientLogic.Products)
                    {
                        product.UpdateDisplayFilter();
                    }
                }
            }
            else if (currentEventPageSize != UserSettings.Settings.EventPageSize)
            {
                // event page size has change, return to the product list
                _clientLogic.CurrentView = ClientLogicView.ProductList;
            }

            // check to see if the current context ID was updated
            _clientLogic.CheckForContextIdChange();
        }

        private bool DoSetupWizard()
        {
            bool success = false;

            SetupWizard setupWizard = new SetupWizard(_clientLogic);
            setupWizard.Owner = this;
            if (setupWizard.ShowDialog() == true)
            {
                _setupWizardWasProfileOnly = setupWizard.SetupWasProfileOnly;

                UserSettings.Settings.FirstConfigComplete = true;
                DoSaveUserSettings();

                success = true;
            }
            else
            {
                // exit if the wizard didn't complete successfully
                Close();

                success = false;
            }

            return success;
        }

        private void DoSaveUserSettings()
        {
            // save settings with retry
            while (true)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    UserSettings.Settings.Save();
                    break;
                }
                catch (Exception ex)
                {
                    Mouse.OverrideCursor = null;

                    if (StackHashMessageBox.Show(this,
                        Properties.Resources.Error_UserSettingsSaveMBMessage,
                        Properties.Resources.Error_UserSettingsSaveMBTitle,
                        StackHashMessageBoxType.RetryCancel,
                        StackHashMessageBoxIcon.Error,
                        ex, StackHashServiceErrorCode.NoError) != StackHashDialogResult.Retry)
                    {
                        break;
                    }
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void DoUploadMappingFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.XmlBrowseFilter;
            ofd.Title = Properties.Resources.MainWindow_BrowseForMappingFile;

            if (ofd.ShowDialog(this) == true)
            {
                try
                {
                    string mappingFileContents = System.IO.File.ReadAllText(ofd.FileName);
                    if (string.IsNullOrEmpty(mappingFileContents))
                    {
                        // nothing in the file to upload
                        StackHashMessageBox.Show(this,
                            Properties.Resources.MainWindow_MappingFileEmptyMBMessage,
                            Properties.Resources.MainWindow_MappingFileEmptyMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error);
                    }
                    else
                    {
                        // start the upload
                        _clientLogic.AdminUploadMappingFile(mappingFileContents);
                    }
                }
                catch (Exception ex)
                {
                    // failed to read file
                    StackHashMessageBox.Show(this,
                        Properties.Resources.MainWindow_MappingFileLoadFailedMBMessage,
                        Properties.Resources.MainWindow_MappingFileLoadFailedMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error,
                        ex,
                        StackHashServiceErrorCode.NoError);
                }
            }
        }

        void _clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                bool invalidate = false;

                switch (e.PropertyName)
                {
                    case "NotBusy":
                        invalidate = true;

                        // Animate while busy
                        if (_busyAnimation != null)
                        {
                            if (_clientLogic.NotBusy)
                            {
                                _busyAnimation.Stop();
                            }
                            else
                            {
                                _busyAnimation.Begin();
                            }
                        }

                        // wait cursor while busy
                        if (_clientLogic.NotBusy)
                        {
                            Mouse.OverrideCursor = null;

                            // continue StackHashUri navigation if necessary
                            DoNextUriStep();
                        }
                        else
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                        }
                        break;

                    case "ServiceStatusText":
                        invalidate = true;
                        break;

                    case "SyncRunning":
                        if (_clientLogic.SyncRunning)
                        {
                            // clear sync status
                            statusBarItemSyncSucceeded.Visibility = Visibility.Collapsed;
                            statusBarItemSyncFailed.Visibility = Visibility.Collapsed;
                        }
                        break;
                }

                if (invalidate)
                {
                    // update command states when NotBusy or ServiceStatusText changes
                    this.Dispatcher.BeginInvoke(new Action(CommandManager.InvalidateRequerySuggested));
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        private void DoShowScriptResult()
        {
            if (_clientLogic.CurrentScriptResult != null)
            {
                ScriptResultViewer scriptResultViewer = new ScriptResultViewer();
                scriptResultViewer.DataContext = _clientLogic.CurrentScriptResult;
                scriptResultViewer.Owner = this;

                scriptResultViewer.ShowDialog();
            }
        }

        private void DoUpgradeFromXml()
        {
            UpgradeFromXml upgradeFromXml = new UpgradeFromXml(_clientLogic);
            upgradeFromXml.Owner = this;

            if (upgradeFromXml.ShowDialog() == true)
            {
                _clientLogic.CheckForContextIdChange();
            }
            else
            {
                Close();
            }
        }

        private void DoSendAllToPlugins()
        {
            SelectPlugins selectPlugins = new SelectPlugins(_clientLogic.ActivePlugins);
            selectPlugins.Owner = this;

            if (selectPlugins.ShowDialog() == true)
            {
                if ((selectPlugins.SelectedPlugins != null) && (selectPlugins.SelectedPlugins.Length > 0))
                {
                    _clientLogic.SendAllToPlugins(selectPlugins.SelectedPlugins);
                }
            }
        }

        private void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                // ignore requests while the context inactive window is open
                if (_contextInactiveWindowOpen)
                {
                    return;
                }

                Mouse.OverrideCursor = null;

                switch (e.UIRequest)
                {
                    case ClientLogicUIRequest.OtherClientDeactivatedThisContext:
                        _contextInactiveWindowOpen = true;
                        
                        ContextInactive contextInactive = new ContextInactive(_clientLogic);
                        contextInactive.Owner = this;
                        if (contextInactive.ShowDialog() == false)
                        {
                            // closed before the context was activated again - quit StackHash
                            Close();
                        }
                        
                        _contextInactiveWindowOpen = false;
                        break;

                    case ClientLogicUIRequest.ProfileSettings:
                        // no profile Id - run local settings to get one
                        DoLocalSettings(false);
                        break;

                    case ClientLogicUIRequest.UpgradeIndexFromXml:
                        DoUpgradeFromXml();
                        break;

                    case ClientLogicUIRequest.ExportComplete:
                        if (StackHashMessageBox.Show(this,
                            string.Format(CultureInfo.CurrentCulture,
                            Properties.Resources.ExportComplete_MBMessage,
                            _lastExportPath),
                            Properties.Resources.ExportComplete_MBTitle,
                            StackHashMessageBoxType.OkOpenFolder,
                            StackHashMessageBoxIcon.Information) == StackHashDialogResult.OpenFolder)
                        {
                            DefaultBrowser.OpenFolder(System.IO.Path.GetDirectoryName(_lastExportPath));
                        }
                        break;

                    case ClientLogicUIRequest.UploadMappingFileCompleted:
                        StackHashMessageBox.Show(this,
                            Properties.Resources.MainWindow_MappingFileUploadSuccessMBMessage,
                            Properties.Resources.MainWindow_MappingFileUploadSuccessMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Information);
                        break;

                    case ClientLogicUIRequest.ClientBumped:
                        StackHashMessageBox.Show(this,
                            string.Format(CultureInfo.CurrentCulture,
                            Properties.Resources.ClientBumped_MBMessage,
                            DateTime.Now.ToShortDateString(),
                            DateTime.Now.ToLongTimeString()),
                            Properties.Resources.ClientBumped_MBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Information);
                        RefreshViewIfNotBusy();
                        break;

                    case ClientLogicUIRequest.CabFolderReady:
                        switch (_actionOnCabExtract)
                        {
                            case ActionOnCabExtract.Debug:
                                DoDebug(_nextDebugger, _nextArchitecture);
                                break;

                            case ActionOnCabExtract.ShowLocalFolder:
                                if (StackHashMessageBox.Show(this,
                                    string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.EventDetails_ExtractCompleteMBMessage,
                                    _clientLogic.CurrentCab.Id,
                                    _clientLogic.CurrentCabFolder),
                                    Properties.Resources.EventDetails_ExtractCompleteMBTitle,
                                    StackHashMessageBoxType.OkOpenFolder,
                                    StackHashMessageBoxIcon.Information) == StackHashDialogResult.OpenFolder)
                                {
                                    DefaultBrowser.OpenFolder(_clientLogic.CurrentCabFolder);
                                }
                                break;

                            case ActionOnCabExtract.ExtractNextCab:
                                // start on the next one
                                this.Dispatcher.BeginInvoke(new Action(DoExtractNextCab));
                                break;
                        }

                        // reset action unless we're extracting more cabs
                        if (_actionOnCabExtract != ActionOnCabExtract.ExtractNextCab)
                        {
                            _actionOnCabExtract = ActionOnCabExtract.None;
                        }
                        break;

                    case ClientLogicUIRequest.ScriptResultsReady:
                        if (_scriptRunFromCommand)
                        {
                            _scriptRunFromCommand = false;
                            this.Dispatcher.BeginInvoke(new Action(DoShowScriptResult));
                        }
                        break;

                    case ClientLogicUIRequest.ContextStatusReady:
                        SyncReport syncReport = new SyncReport(_clientLogic.ContextStatus);
                        syncReport.Owner = this;
                        syncReport.ShowDialog();
                        break;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void CheckForContextIdChange()
        {
            _clientLogic.CheckForContextIdChange();
        }

        private void RefreshView()
        {
            _clientLogic.RefreshCurrentView();
        }

        private void hyperlinkPluginFailure_Click(object sender, RoutedEventArgs e)
        {
            ShowPluginFailureMessage(null, StackHashServiceErrorCode.NoError);
        }

        private void ShowPluginFailureMessage(Exception relatedException, StackHashServiceErrorCode relatedServiceErrorCode)
        {
            StackHashMessageBox.Show(this,
                Properties.Resources.Error_PluginFailureMBMessage,
                Properties.Resources.Error_PluginFailureMBTitle,
                StackHashMessageBoxType.Ok,
                StackHashMessageBoxIcon.Error,
                relatedException,
                relatedServiceErrorCode);
        }

        private void _clientLogic_ClientLogicError(object sender, ClientLogicErrorEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                Mouse.OverrideCursor = null;

                // ignore requests while the context inactive window is open
                if (_contextInactiveWindowOpen)
                {
                    return;
                }

                if (Monitor.TryEnter(_serviceErrorMessageLock))
                {
                    try
                    {
                        if ((e.ServiceError == StackHashServiceErrorCode.CabDoesNotExist) ||
                            (e.ServiceError == StackHashServiceErrorCode.CabDownloadFailed) ||
                            (e.ServiceError == StackHashServiceErrorCode.CabIsCorrupt))
                        {
                            // on cab failure refresh the view so that the state can toggle to not downloaded
                            // begininvoke to make sure we don't block ClientLogic
                            this.Dispatcher.BeginInvoke(new Action(RefreshView));
                        }

                        if ((e.Error == ClientLogicErrorCode.PluginFailure) || (e.ServiceError == StackHashServiceErrorCode.BugTrackerPlugInError))
                        {
                            ShowPluginFailureMessage(e.Exception, e.ServiceError);
                            return;
                        }

                        switch (e.Error)
                        {
                            case ClientLogicErrorCode.BugReportFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_BugReportFailedMBMessage,
                                    Properties.Resources.Error_BugReportFailedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.LocalCabExtractFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_CabCorruptMBMessage,
                                    Properties.Resources.Error_CabCorruptMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.ServiceConnectFailed:
                                if (StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_ServiceConnectFailedMBMessage,
                                    Properties.Resources.Error_ServiceConnectFailedMBTitle,
                                    StackHashMessageBoxType.RetryCancel,
                                    StackHashMessageBoxIcon.Warning,
                                    e.Exception,
                                    e.ServiceError) == StackHashDialogResult.Retry)
                                {
                                    _clientLogic.AdminServiceConnect();
                                }
                                break;

                            case ClientLogicErrorCode.AdminReportError:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_AdminReportErrorMBMessage,
                                    Properties.Resources.Error_AdminReportErrorMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.ServiceCallFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_ServiceCallFailedMBMessage,
                                    Properties.Resources.Error_ServiceCallFailedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.ServiceCallTimeout:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_ServiceCallTimeoutMBMessage,
                                    Properties.Resources.Error_ServiceCallTimeoutMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.WinQualLogOnFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_WinQualLogOnMBMessage,
                                    Properties.Resources.Error_WinQualLogOnMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.SynchronizationFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_SyncFailedMBMessage,
                                    Properties.Resources.Error_SyncFailedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.ExportFailed:
                                StackHashMessageBox.Show(this,
                                    string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.Error_ExportFailedMBMessage,
                                    _lastExportPath),
                                    Properties.Resources.Error_ExportFailedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.MoveIndexFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_MoveFailedMBMessage,
                                    Properties.Resources.Error_MoveFailedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.CopyIndexFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_CopyFailedMBMessage,
                                    Properties.Resources.Error_CopyFailedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.UploadMappingFileFailed:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_UploadMappingFileMBMessage,
                                    Properties.Resources.Error_UploadMappingFileMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;

                            case ClientLogicErrorCode.Unexpected:
                            default:
                                StackHashMessageBox.Show(this,
                                    Properties.Resources.Error_UnexpectedMBMessage,
                                    Properties.Resources.Error_UnexpectedMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    e.Exception,
                                    e.ServiceError);
                                break;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_serviceErrorMessageLock);
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicErrorEventArgs>(_clientLogic_ClientLogicError), sender, e);
            }
        }

        void _clientLogic_SyncComplete(object sender, SyncCompleteEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.Succeeded)
                {
                    statusBarItemSyncSucceeded.Visibility = Visibility.Visible;
                    statusBarItemSyncFailed.Visibility = Visibility.Collapsed;
                }
                else
                {
                    statusBarItemSyncSucceeded.Visibility = Visibility.Collapsed;
                    statusBarItemSyncFailed.Visibility = Visibility.Visible;
                }

                this.Dispatcher.BeginInvoke(new Action(RefreshViewIfNotBusy));
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, SyncCompleteEventArgs>(_clientLogic_SyncComplete), sender, e);
            }
        }

        private void RefreshViewIfNotBusy()
        {
            if (_clientLogic.NotBusy)
            {
                _clientLogic.RefreshCurrentView();
            }
        }

        private void DoRunScript(string scriptName)
        {
            if (scriptName != null)
            {
                _scriptRunFromCommand = true;
                _clientLogic.AdminTestScript(scriptName);
            }
        }

        private string GetCabExtractFolder()
        {
            string extractFolder = null;

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = false;
            ofd.CheckFileExists = false;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.FileName = Properties.Resources.SelectFolder;
            ofd.Filter = Properties.Resources.SelectFolderFilter;
            ofd.Title = Properties.Resources.EventDetails_SelectFolderTitle;

            if (ofd.ShowDialog() == true)
            {
                extractFolder = System.IO.Path.GetDirectoryName(ofd.FileName);
            }

            return extractFolder;
        }

        // extract one cab
        private void DoExtractCabLocal()
        {
            if (_clientLogic.CurrentCab != null)
            {
                string extractFolder = GetCabExtractFolder();
                if (extractFolder != null)
                {
                    _actionOnCabExtract = ActionOnCabExtract.ShowLocalFolder;
                    _clientLogic.ExtractCurrentCab(extractFolder);
                }
            }
        }

        // start extracting a list of cabs (in _cabsToExtract)
        private void DoExtractFirstCab()
        {
            _cabExtractFolder = GetCabExtractFolder();
            if (_cabExtractFolder != null)
            {
                DoExtractNextCab();
            }
        }

        // extract the next cab in a list of cabs (in _cabsToExtract)
        private void DoExtractNextCab()
        {
            if ((_cabsToExtract != null) && (_cabsToExtract.Count > 0))
            {
                _actionOnCabExtract = ActionOnCabExtract.ExtractNextCab;
                _clientLogic.ExtractCab(_cabExtractFolder, _cabsToExtract.Dequeue());
            }
            else
            {
                StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.MainWindow_CabsExtractedMBMessage,
                    _clientLogic.CurrentEventPackage.Id,
                    _cabExtractFolder),
                    Properties.Resources.MainWindow_CabsExtractedMBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Information);

                _cabsToExtract = null;
                _cabExtractFolder = null;
                _actionOnCabExtract = ActionOnCabExtract.None;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void DoDebug(DefaultDebugger debugger, DumpArchitecture arch)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                System.IO.FileInfo largestDumpFileInfo = null;
                System.IO.FileInfo versionInfoFileInfo = null;

                // list all files and try to find the info file and the largest dump
                System.IO.DirectoryInfo cabDirectoryInfo = new System.IO.DirectoryInfo(_clientLogic.CurrentCabFolder);
                System.IO.FileInfo[] files = cabDirectoryInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                long largest = 0;
                foreach (System.IO.FileInfo file in files)
                {
                    if (string.Compare(".TXT", System.IO.Path.GetExtension(file.FullName), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        versionInfoFileInfo = file;
                    }
                    else
                    {
                        if (file.Length > largest)
                        {
                            largestDumpFileInfo = file;
                            largest = file.Length;
                        }
                    }
                }

                // try to get the architecture if not known
                if (arch == DumpArchitecture.Unknown)
                {
                    if ((_clientLogic.CurrentCab != null) &&
                        (_clientLogic.CurrentCab.MachineArchitecture != null))
                    {
                        if (string.Compare("x64", _clientLogic.CurrentCab.MachineArchitecture, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            arch = DumpArchitecture.X64;
                        }
                        else if (string.Compare("x86", _clientLogic.CurrentCab.MachineArchitecture, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            arch = DumpArchitecture.X86;
                        }
                    }
                }

                // if we couldn't get if from the dump analysis try to get it from the version info file
                if ((arch == DumpArchitecture.Unknown) && (versionInfoFileInfo != null))
                {
                    arch = ClientUtils.GetArchitectureFromVersionFile(versionInfoFileInfo.FullName);
                }

                // check that we can debug it - note, if unknown we assume X86
                bool canDebug = true;

                if (arch == DumpArchitecture.IA64)
                {
                    // itanium
                    canDebug = false;

                    Mouse.OverrideCursor = null;

                    StackHashMessageBox.Show(this,
                        Properties.Resources.Error_CantDebugItaniumMBMessage,
                        Properties.Resources.Error_CantDebugMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error);
                }
                else if (largestDumpFileInfo == null)
                {
                    // no dump
                    canDebug = false;

                    Mouse.OverrideCursor = null;

                    StackHashMessageBox.Show(this,
                        Properties.Resources.Error_CantDebugNoDump,
                        Properties.Resources.Error_CantDebugMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error);
                }

                if (canDebug)
                {
                    Mouse.OverrideCursor = null;

                    string commandLine = null;
                    string debuggerPath = GetDebuggerPath(arch, debugger, largestDumpFileInfo.FullName, out commandLine);

                    Mouse.OverrideCursor = Cursors.Wait;

                    // check that we have a debugger path
                    if ((!string.IsNullOrEmpty(debuggerPath)) && (!string.IsNullOrEmpty(commandLine)))
                    {
                        // run the debugger
                        Process.Start(debuggerPath, commandLine);
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                    "DoDebug Failed",
                    ex);

                Mouse.OverrideCursor = null;

                StackHashMessageBox.Show(this,
                    Properties.Resources.Error_DoDebugMBMessage,
                    Properties.Resources.Error_DoDebugMBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Error,
                    ex,
                    StackHashServiceErrorCode.NoError);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private string GetDebuggerPath(DumpArchitecture architecture, DefaultDebugger defaultDebugger, string dumpFilePath, out string commandLine)
        {
            commandLine = null;
            string debuggerPath = null;

            switch (defaultDebugger)
            {
                case DefaultDebugger.DebuggingToolsForWindows:
                    commandLine = string.Format(CultureInfo.InvariantCulture, "-z \"{0}\"", dumpFilePath);
                    switch (architecture)
                    {
                        case DumpArchitecture.X64:

                            if (string.IsNullOrEmpty(UserSettings.Settings.DebuggerPathAmd64))
                            {
                                OpenFileDialog ofd = new OpenFileDialog();

                                ofd.AddExtension = true;
                                ofd.CheckFileExists = true;
                                ofd.CheckPathExists = true;
                                ofd.Multiselect = false;
                                ofd.RestoreDirectory = true;
                                ofd.ShowReadOnly = false;
                                ofd.ValidateNames = true;
                                ofd.Filter = Properties.Resources.ExeBrowseFilter;
                                ofd.Title = Properties.Resources.OptionsWindow_BrowseAmd64DebuggerTitle;

                                if (ofd.ShowDialog(this) == true)
                                {
                                    UserSettings.Settings.DebuggerPathAmd64 = ofd.FileName;
                                }
                            }

                            debuggerPath = UserSettings.Settings.DebuggerPathAmd64;
                            break;

                        case DumpArchitecture.X86:
                        default:

                            if (string.IsNullOrEmpty(UserSettings.Settings.DebuggerPathX86))
                            {
                                OpenFileDialog ofd = new OpenFileDialog();

                                ofd.AddExtension = true;
                                ofd.CheckFileExists = true;
                                ofd.CheckPathExists = true;
                                ofd.Multiselect = false;
                                ofd.RestoreDirectory = true;
                                ofd.ShowReadOnly = false;
                                ofd.ValidateNames = true;
                                ofd.Filter = Properties.Resources.ExeBrowseFilter;
                                ofd.Title = Properties.Resources.OptionsWindow_BrowseX86DebuggerTitle;

                                if (ofd.ShowDialog(this) == true)
                                {
                                    UserSettings.Settings.DebuggerPathX86 = ofd.FileName;
                                }
                            }

                            // for unknown architecture also attempt to debug using 32-bit
                            debuggerPath = UserSettings.Settings.DebuggerPathX86;
                            break;
                    }
                    break;

                case DefaultDebugger.VisualStudio:
                    commandLine = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", dumpFilePath);

                    if (string.IsNullOrEmpty(UserSettings.Settings.DebuggerPathVisualStudio))
                    {
                        OpenFileDialog ofd = new OpenFileDialog();

                        ofd.AddExtension = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.Multiselect = false;
                        ofd.RestoreDirectory = true;
                        ofd.ShowReadOnly = false;
                        ofd.ValidateNames = true;
                        ofd.Filter = Properties.Resources.ExeBrowseFilter;
                        ofd.Title = Properties.Resources.OptionsWindow_BrowseVisualStudio;

                        if (ofd.ShowDialog(this) == true)
                        {
                            UserSettings.Settings.DebuggerPathVisualStudio = ofd.FileName;
                        }
                    }

                    debuggerPath = UserSettings.Settings.DebuggerPathVisualStudio;
                    break;
            }

            return debuggerPath;
        }

        private void DoSearch()
        {
            if (_clientLogic.NotBusy)
            {
                if (comboBoxSearch.Text.Length > 0)
                {
                    StackHashUri uri;
                    if (StackHashUri.TryParse(comboBoxSearch.Text, out uri))
                    {
                        // StackHashUri is in the search box - start loading it
                        App app = Application.Current as App;
                        if (app != null)
                        {
                            app.CurrentStackHashUri = uri;
                            _nextUriStep = NextUriStep.DetectAndSelectContext;
                            UserSettings.Settings.AddSearch(comboBoxSearch.Text);
                            DoNextUriStep();
                        }
                    }
                    else
                    {
                        // look for a regular search
                        bool runSearch = true;
                        if (_clientLogic.CurrentProduct == null)
                        {
                            if (!UserSettings.Settings.IsMessageSuppressed(UserSettings.SuppressMainWindowSearchAllProducts))
                            {
                                bool suppress;
                                if (StackHashMessageBox.Show(this,
                                    Properties.Resources.MainWindow_SearchAllProductsMBMessage,
                                    Properties.Resources.MainWindow_SearchAllProdcutsMBTitle,
                                    StackHashMessageBoxType.YesNo,
                                    StackHashMessageBoxIcon.Question,
                                    true,
                                    out suppress) != StackHashDialogResult.Yes)
                                {
                                    runSearch = false;
                                }
                                if (suppress)
                                {
                                    UserSettings.Settings.SuppressMessage(UserSettings.SuppressMainWindowSearchAllProducts);
                                }
                            }
                        }

                        if (runSearch)
                        {
                            string unused;

                            try
                            {
                                StackHashProduct product = null;
                                int hitThreshold = UserSettings.Settings.GetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId);
                                if (_clientLogic.CurrentProduct != null)
                                {
                                    product = _clientLogic.CurrentProduct.StackHashProductInfo.Product;
                                    hitThreshold = UserSettings.Settings.GetDisplayHitThreshold(product.Id);
                                }
                                StackHashSearchCriteriaCollection search = SearchBuilder.CompileSearch(comboBoxSearch.Text, product, out unused);

                                if (ScriptManager.SearchContainsScriptSearch(search))
                                {
                                    if (!UserSettings.Settings.IsMessageSuppressed(UserSettings.SuppressMainWindowSearchScripts))
                                    {
                                        bool suppress;
                                        if (StackHashMessageBox.Show(this,
                                            Properties.Resources.MainWindow_SearchScriptsMBMessage,
                                            Properties.Resources.MainWindow_SearchScriptsMBTitle,
                                            StackHashMessageBoxType.YesNo,
                                            StackHashMessageBoxIcon.Question,
                                            true,
                                            out suppress) != StackHashDialogResult.Yes)
                                        {
                                            runSearch = false;
                                        }
                                        if (suppress)
                                        {
                                            UserSettings.Settings.SuppressMessage(UserSettings.SuppressMainWindowSearchScripts);
                                        }
                                    }
                                }

                                if (runSearch)
                                {
                                    UserSettings.Settings.AddSearch(comboBoxSearch.Text);
                                    _lastSearch = comboBoxSearch.Text;

                                    _clientLogic.PopulateEventPackages(_clientLogic.CurrentProduct,
                                        search,
                                        _clientLogic.LastEventsSort,
                                        1,
                                        UserSettings.Settings.EventPageSize,
                                        hitThreshold,
                                        ClientLogicView.EventList,
                                        PageIntention.First,
                                        UserSettings.Settings.ShowEventsWithoutCabs);
                                }
                            }
                            catch (SearchParseException ex)
                            {
                                StackHashMessageBox.Show(this,
                                    string.Format(CultureInfo.CurrentCulture,
                                    Properties.Resources.MainWindow_SearchParseFailedMBMessage,
                                    ex.Message),
                                    Properties.Resources.MainWindow_SearchParseFauiledMBTitle,
                                    StackHashMessageBoxType.Ok,
                                    StackHashMessageBoxIcon.Error,
                                    ex,
                                    StackHashServiceErrorCode.NoError);
                            }
                        }
                    }
                }
                else
                {
                    // search box entry - clear any current search
                    ClearSearch();
                }
            }
        }

        private void ClearSearch()
        {
            if (_clientLogic.NotBusy)
            {
                comboBoxSearch.Text = string.Empty;

                if (_clientLogic.CurrentProduct == null)
                {
                    // return to the product list when clearing the search if no current product
                    _clientLogic.CurrentView = ClientLogicView.ProductList;
                }
                else
                {
                    _clientLogic.PopulateEventPackages(_clientLogic.CurrentProduct,
                        null,
                        _clientLogic.LastEventsSort,
                        1,
                        UserSettings.Settings.EventPageSize,
                        UserSettings.Settings.GetDisplayHitThreshold(_clientLogic.CurrentProduct.Id),
                        _clientLogic.CurrentView == ClientLogicView.ProductList ? ClientLogicView.ProductList : ClientLogicView.EventList,
                        PageIntention.First,
                        UserSettings.Settings.ShowEventsWithoutCabs);
                }
            }
        }

        private string GetExportPath(string initialFilename)
        {
            string exportPath = null;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.CreatePrompt = false;
            sfd.DefaultExt = Properties.Resources.ExportSaveDefaultExtension;
            sfd.FileName = initialFilename;
            sfd.Filter = Properties.Resources.ExportSaveFilter;
            sfd.OverwritePrompt = true;
            sfd.Title = Properties.Resources.ExportSaveTitle;

            if (sfd.ShowDialog(this) == true)
            {
                exportPath = sfd.FileName;
            }

            return exportPath;
        }

        private void DoExportProductList()
        {
            string exportPath = GetExportPath(Properties.Resources.ExportSaveProductListName);
            if (exportPath != null)
            {
                _lastExportPath = exportPath;
                _clientLogic.ExportProductList(exportPath);
            }
        }

        private void DoExportEventList(bool includeCabsAndEventInfos)
        {
            string exportName;

            if (_clientLogic.CurrentProduct == null)
            {
                exportName = Properties.Resources.ExportSaveDefaultName;
            }
            else
            {
                exportName = ClientUtils.MakeSafeDirectoryName(string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ExportSaveEventListName,
                    _clientLogic.CurrentProduct.Name,
                    _clientLogic.CurrentProduct.Version));
            }

            string exportPath = GetExportPath(exportName);

            if (exportPath != null)
            {
                _lastExportPath = exportPath;
                _clientLogic.ExportEventList(exportPath, includeCabsAndEventInfos);
            }
        }

        private void DoShowAbout()
        {
            About about = new About();
            about.Owner = this;
            about.DataContext = UserSettings.Settings;
            about.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // save menu / toolbar focus scopes
            _editMenuFocusScope = FocusManager.GetFocusScope(menuItemCut);
            _editToolbarFocusScope = FocusManager.GetFocusScope(toolbarButtonCut);

            // hide sync report to start with
            statusBarItemSyncSucceeded.Visibility = Visibility.Collapsed;
            statusBarItemSyncFailed.Visibility = Visibility.Collapsed;

            // run startup tasks (if not in designer)
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _busyAnimation = (Storyboard)ellipseBusy.FindResource("StoryboardEllipse");

                menuItemShowDisabledProducts.IsChecked = UserSettings.Settings.ShowDisabledProducts;
                menuItemShowEventsWithoutCabs.IsChecked = UserSettings.Settings.ShowEventsWithoutCabs;

                bool setupWizardRun = false;

                // run setup wizard if necessary
                if (!UserSettings.Settings.FirstConfigComplete)
                {
                    // first run - show setup wizard
                    if (!DoSetupWizard())
                    {
                        // don't continue if the setup wizard failed
                        return;
                    }
                    setupWizardRun = true;
                }

                // show the product list
                _clientLogic.RefreshCurrentPolicies();
                viewMainProductList.HookUpProductList();

                // hook up to client logic after the setup wizard
                _clientLogic.ClientLogicError += new EventHandler<ClientLogicErrorEventArgs>(_clientLogic_ClientLogicError);
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
                _clientLogic.PropertyChanged += new PropertyChangedEventHandler(_clientLogic_PropertyChanged);
                _clientLogic.SyncComplete += new EventHandler<SyncCompleteEventArgs>(_clientLogic_SyncComplete);
                _clientLogic.ClientLogicContextIdChanged += new EventHandler(_clientLogic_ClientLogicContextIdChanged);

                // start listening to other instances
                App app = App.Current as App;
                if (app != null)
                {
                    app.StackHashUriNavigationRequest += new EventHandler(app_StackHashUriNavigationRequest);
                }

                if (setupWizardRun)
                {
                    // setup completed, start a sync (check for busy in case we bailed early)
                    if ((_clientLogic.NotBusy) && (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId))
                    {
                        if (_setupWizardWasProfileOnly)
                        {
                            // profile selected only
                            _clientLogic.CheckForContextIdChange();
                        }
                        else
                        {
                            // full setup - start a sync
                            _clientLogic.AdminStartSync(false, null);
                        }
                    }
                }
                else
                {
                    // connect
                    _clientLogic.AdminServiceConnect();
                }
            }
        }

        private void app_StackHashUriNavigationRequest(object sender, EventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (_clientLogic.NotBusy)
                {
                    // start navigation if we're idle (otherwise navigation will commence when we are idle)
                    DoNextUriStep();
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, EventArgs>(app_StackHashUriNavigationRequest), sender, e);
            }
        }

        private void HideSyncReport()
        {
            statusBarItemSyncSucceeded.Visibility = Visibility.Collapsed;
            statusBarItemSyncFailed.Visibility = Visibility.Collapsed;
        }

        void _clientLogic_ClientLogicContextIdChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(HideSyncReport));
        }    

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // notify the main view controls that we're closed
                viewCabDetails.StackHashMainWindowClosed();
                viewEventDetails.StackHashMainWindowClosed();
                viewEventList.StackHashMainWindowClosed();
                viewMainProductList.StackHashMainWindowClosed();

                // unhook from client logic
                _clientLogic.ClientLogicError -= _clientLogic_ClientLogicError;
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
                _clientLogic.SyncComplete -= _clientLogic_SyncComplete;
                _clientLogic.ClientLogicContextIdChanged -= _clientLogic_ClientLogicContextIdChanged;

                // stop listening to other instances
                App app = App.Current as App;
                if (app != null)
                {
                    app.StackHashUriNavigationRequest -= app_StackHashUriNavigationRequest;
                }

                UserSettings.Settings.SaveToolBar(ToolBarMainKey, toolBarMain);
                UserSettings.Settings.SaveToolBar(ToolBarSearchKey, toolBarSearch);
                UserSettings.Settings.SaveToolBar(ToolBarToolsKey, toolBarTools);
                UserSettings.Settings.SaveWindow(MainWindowKey, this);
            }

            // dipose client logic
            if (_clientLogic != null)
            {
                _clientLogic.Dispose();
                _clientLogic = null;
            }
        }

        private void comboBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return) || (e.Key == Key.Enter))
            {
                DoSearch();
            }
            else
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void comboBoxSearch_DropDownClosed(object sender, EventArgs e)
        {
            // search if the selection actually changed
            if (comboBoxSearch.Text != _lastSearch)
            {
                DoSearch();
            }
        }

        private void searchListRemove_Click(object sender, RoutedEventArgs e)
        {
            MenuItem sourceItem = e.OriginalSource as MenuItem;
            if (sourceItem != null)
            {
                string search = sourceItem.DataContext as string;
                if (search != null)
                {
                    UserSettings.Settings.RemoveSearch(search);
                }
            }
        }

        /// <summary>
        /// If there are products but none are enabled for sync prompt the user
        /// to make sure they want to continue
        /// </summary>
        /// <returns>True if sync should continue</returns>
        private bool CheckForAnySyncEnabledProducts()
        {
            bool continueSync = true;

            if ((_clientLogic.Products != null) && (_clientLogic.Products.Count > 0))
            {
                bool anySyncEnabled = false;

                foreach (DisplayProduct product in _clientLogic.Products)
                {
                    if (product.SynchronizeEnabled)
                    {
                        anySyncEnabled = true;
                        break;
                    }
                }

                if (!anySyncEnabled)
                {
                    if (StackHashMessageBox.Show(this,
                        Properties.Resources.MainWindow_NoEnabledProductsMBMessage,
                        Properties.Resources.MainWindow_NoEnabledProductsMBTitle,
                        StackHashMessageBoxType.YesNo,
                        StackHashMessageBoxIcon.Question) != StackHashDialogResult.Yes)
                    {
                        continueSync = false;
                    }
                }
            }

            return continueSync;
        }

        private bool CheckForSyncWarnings()
        {
            bool continueSync = true;
            bool suppress = false;

            // check that a profile debugger has been set
            if (continueSync)
            {
                bool haveDebugger = true;
                if (_clientLogic.CurrentContext != null)
                {
                    if (string.IsNullOrEmpty(_clientLogic.CurrentContext.StackHashContextSettings.DebuggerSettings.DebuggerPathAndFileName) &&
                        string.IsNullOrEmpty(_clientLogic.CurrentContext.StackHashContextSettings.DebuggerSettings.DebuggerPathAndFileName64Bit))
                    {
                        haveDebugger = false;
                    }
                }
                if (!haveDebugger)
                {
                    if (!UserSettings.Settings.IsMessageSuppressed(UserSettings.SuppressMainWindowDebuggerCheck))
                    {
                        if (StackHashMessageBox.Show(this,
                         Properties.Resources.MainWindow_StartSyncNoDebuggerMBMessage,
                         Properties.Resources.MainWindow_StartSyncNoDebuggerMBTitle,
                         StackHashMessageBoxType.YesNo,
                         StackHashMessageBoxIcon.Question,
                         true,
                         out suppress) != StackHashDialogResult.Yes)
                        {
                            continueSync = false;
                        }
                        if (suppress)
                        {
                            UserSettings.Settings.SuppressMessage(UserSettings.SuppressMainWindowDebuggerCheck);
                        }
                    }
                }
            }

            // warning that sync can take a long time
            if (continueSync)
            {
                if (!UserSettings.Settings.IsMessageSuppressed(UserSettings.SuppressMainWindowSync))
                {
                    if (StackHashMessageBox.Show(this,
                         Properties.Resources.MainWindow_StartSyncMBMessage,
                         Properties.Resources.MainWindow_StartSyncMBTitle,
                         StackHashMessageBoxType.YesNo,
                         StackHashMessageBoxIcon.Question,
                         true,
                         out suppress) != StackHashDialogResult.Yes)
                    {
                        continueSync = false;
                    }
                    if (suppress)
                    {
                        UserSettings.Settings.SuppressMessage(UserSettings.SuppressMainWindowSync);
                    }
                }
            }

            return continueSync;
        }

        #region Command Handlers

        private void InitializeCommandBindings()
        {
            CommandBinding commandBindingExit = new CommandBinding(StackHashCommands.ExitCommand);
            commandBindingExit.Executed += new ExecutedRoutedEventHandler(commandBindingExit_Executed);
            CommandBindings.Add(commandBindingExit);

            CommandBinding commandBindingSync = new CommandBinding(StackHashCommands.SyncCommand);
            commandBindingSync.CanExecute += new CanExecuteRoutedEventHandler(commandBindingSync_CanExecute);
            commandBindingSync.Executed += new ExecutedRoutedEventHandler(commandBindingSync_Executed);
            CommandBindings.Add(commandBindingSync);

            CommandBinding commandBindingCancelSync = new CommandBinding(StackHashCommands.CancelSyncCommand);
            commandBindingCancelSync.CanExecute += new CanExecuteRoutedEventHandler(commandBindingCancelSync_CanExecute);
            commandBindingCancelSync.Executed += new ExecutedRoutedEventHandler(commandBindingCancelSync_Executed);
            CommandBindings.Add(commandBindingCancelSync);

            CommandBinding commandBindingProfileManager = new CommandBinding(StackHashCommands.ProfileManagerCommand);
            commandBindingProfileManager.CanExecute += new CanExecuteRoutedEventHandler(commandBindingProfileManager_CanExecute);
            commandBindingProfileManager.Executed += new ExecutedRoutedEventHandler(commandBindingProfileManager_Executed);
            CommandBindings.Add(commandBindingProfileManager);

            CommandBinding commandBindingSearch = new CommandBinding(StackHashCommands.SearchCommand);
            commandBindingSearch.CanExecute += new CanExecuteRoutedEventHandler(commandBindingSearch_CanExecute);
            commandBindingSearch.Executed += new ExecutedRoutedEventHandler(commandBindingSearch_Executed);
            CommandBindings.Add(commandBindingSearch);

            CommandBinding commandBindingClearSearch = new CommandBinding(StackHashCommands.ClearSearchCommand);
            commandBindingClearSearch.CanExecute += new CanExecuteRoutedEventHandler(commandBindingClearSearch_CanExecute);
            commandBindingClearSearch.Executed += new ExecutedRoutedEventHandler(commandBindingClearSearch_Executed);
            CommandBindings.Add(commandBindingClearSearch);

            CommandBinding commandBindingBuildSearch = new CommandBinding(StackHashCommands.BuildSearchCommand);
            commandBindingBuildSearch.CanExecute += new CanExecuteRoutedEventHandler(commandBindingBuildSearch_CanExecute);
            commandBindingBuildSearch.Executed += new ExecutedRoutedEventHandler(commandBindingBuildSearch_Executed);
            CommandBindings.Add(commandBindingBuildSearch);

            CommandBinding commandBindingResync = new CommandBinding(StackHashCommands.ResyncCommand);
            commandBindingResync.CanExecute += new CanExecuteRoutedEventHandler(commandBindingResync_CanExecute);
            commandBindingResync.Executed += new ExecutedRoutedEventHandler(commandBindingResync_Executed);
            CommandBindings.Add(commandBindingResync);

            CommandBinding commandBindingScriptManager = new CommandBinding(StackHashCommands.ScriptManagerCommand);
            commandBindingScriptManager.CanExecute += new CanExecuteRoutedEventHandler(commandBindingScriptManager_CanExecute);
            commandBindingScriptManager.Executed += new ExecutedRoutedEventHandler(commandBindingScriptManager_Executed);
            CommandBindings.Add(commandBindingScriptManager);

            CommandBinding commandBindingAbout = new CommandBinding(StackHashCommands.AboutCommand);
            commandBindingAbout.Executed += new ExecutedRoutedEventHandler(commandBindingAbout_Executed);
            commandBindingAbout.CanExecute += new CanExecuteRoutedEventHandler(commandBindingAbout_CanExecute);
            CommandBindings.Add(commandBindingAbout);

            CommandBinding commandBindingBack = new CommandBinding(NavigationCommands.BrowseBack);
            commandBindingBack.CanExecute += new CanExecuteRoutedEventHandler(commandBindingBack_CanExecute);
            commandBindingBack.Executed += new ExecutedRoutedEventHandler(commandBindingBack_Executed);
            CommandBindings.Add(commandBindingBack);

            InputBindings.Add(new InputBinding(NavigationCommands.BrowseBack, new KeyGesture(Key.Left, ModifierKeys.Alt)));

            CommandBinding commandBindingHome = new CommandBinding(NavigationCommands.BrowseHome);
            commandBindingHome.CanExecute += new CanExecuteRoutedEventHandler(commandBindingHome_CanExecute);
            commandBindingHome.Executed += new ExecutedRoutedEventHandler(commandBindingHome_Executed);
            CommandBindings.Add(commandBindingHome);

            InputBindings.Add(new InputBinding(NavigationCommands.BrowseHome, new KeyGesture(Key.Home, ModifierKeys.Alt)));

            CommandBinding commandDebug = new CommandBinding(StackHashCommands.DebugCommand);
            commandDebug.CanExecute += new CanExecuteRoutedEventHandler(commandDebug_CanExecute);
            commandDebug.Executed += new ExecutedRoutedEventHandler(commandDebug_Executed);
            CommandBindings.Add(commandDebug);

            CommandBinding optionsCommand = new CommandBinding(StackHashCommands.OptionsCommand);
            optionsCommand.CanExecute += new CanExecuteRoutedEventHandler(optionsCommand_CanExecute);
            optionsCommand.Executed += new ExecutedRoutedEventHandler(optionsCommand_Executed);
            CommandBindings.Add(optionsCommand);

            CommandBinding serviceStatusCommand = new CommandBinding(StackHashCommands.ServiceStatusCommand);
            serviceStatusCommand.Executed += new ExecutedRoutedEventHandler(serviceStatusCommand_Executed);
            CommandBindings.Add(serviceStatusCommand);

            CommandBinding showDisabledProductCommand = new CommandBinding(StackHashCommands.ShowDisabledProductsCommand);
            showDisabledProductCommand.CanExecute += new CanExecuteRoutedEventHandler(showDisabledProductCommand_CanExecute);
            showDisabledProductCommand.Executed += new ExecutedRoutedEventHandler(showDisabledProductCommand_Executed);
            CommandBindings.Add(showDisabledProductCommand);

            CommandBinding debugVisualStudioCommand = new CommandBinding(StackHashCommands.DebugVisualStudioCommand);
            debugVisualStudioCommand.CanExecute += new CanExecuteRoutedEventHandler(debugVisualStudioCommand_CanExecute);
            debugVisualStudioCommand.Executed += new ExecutedRoutedEventHandler(debugVisualStudioCommand_Executed);
            CommandBindings.Add(debugVisualStudioCommand);

            CommandBinding debugX86Command = new CommandBinding(StackHashCommands.DebugX86Command);
            debugX86Command.CanExecute += new CanExecuteRoutedEventHandler(debugX86Command_CanExecute);
            debugX86Command.Executed += new ExecutedRoutedEventHandler(debugX86Command_Executed);
            CommandBindings.Add(debugX86Command);

            CommandBinding debugX64Command = new CommandBinding(StackHashCommands.DebugX64Command);
            debugX64Command.CanExecute += new CanExecuteRoutedEventHandler(debugX64Command_CanExecute);
            debugX64Command.Executed += new ExecutedRoutedEventHandler(debugX64Command_Executed);
            CommandBindings.Add(debugX64Command);

            CommandBinding refreshCommand = new CommandBinding(StackHashCommands.RefreshCommand);
            refreshCommand.CanExecute += new CanExecuteRoutedEventHandler(refreshCommand_CanExecute);
            refreshCommand.Executed += new ExecutedRoutedEventHandler(refreshCommand_Executed);
            CommandBindings.Add(refreshCommand);

            CommandBinding extractCabCommand = new CommandBinding(StackHashCommands.ExtractCabCommand);
            extractCabCommand.CanExecute += new CanExecuteRoutedEventHandler(extractCabCommand_CanExecute);
            extractCabCommand.Executed += new ExecutedRoutedEventHandler(extractCabCommand_Executed);
            CommandBindings.Add(extractCabCommand);

            CommandBinding runScriptCommand = new CommandBinding(StackHashCommands.RunScriptCommand);
            runScriptCommand.CanExecute += new CanExecuteRoutedEventHandler(runScriptCommand_CanExecute);
            CommandBindings.Add(runScriptCommand);

            CommandBinding runScriptByNameCommand = new CommandBinding(StackHashCommands.RunScriptByNameCommand);
            runScriptByNameCommand.CanExecute += new CanExecuteRoutedEventHandler(runScriptByNameCommand_CanExecute);
            runScriptByNameCommand.Executed += new ExecutedRoutedEventHandler(runScriptByNameCommand_Executed);
            CommandBindings.Add(runScriptByNameCommand);

            CommandBinding debugUsingCommand = new CommandBinding(StackHashCommands.DebugUsingCommand);
            debugUsingCommand.CanExecute += new CanExecuteRoutedEventHandler(debugUsingCommand_CanExecute);
            CommandBindings.Add(debugUsingCommand);

            CommandBinding cutCommand = new CommandBinding(ApplicationCommands.Cut);
            cutCommand.PreviewCanExecute += new CanExecuteRoutedEventHandler(cutCommand_PreviewCanExecute);
            CommandBindings.Add(cutCommand);

            CommandBinding copyCommnad = new CommandBinding(ApplicationCommands.Copy);
            copyCommnad.PreviewCanExecute += new CanExecuteRoutedEventHandler(copyCommnad_PreviewCanExecute);
            CommandBindings.Add(copyCommnad);

            CommandBinding pasteCommand = new CommandBinding(ApplicationCommands.Paste);
            pasteCommand.PreviewCanExecute += new CanExecuteRoutedEventHandler(pasteCommand_PreviewCanExecute);
            CommandBindings.Add(pasteCommand);

            CommandBinding selectAllCommand = new CommandBinding(ApplicationCommands.SelectAll);
            selectAllCommand.PreviewCanExecute += new CanExecuteRoutedEventHandler(selectAllCommand_PreviewCanExecute);
            CommandBindings.Add(selectAllCommand);

            CommandBinding exportCommand = new CommandBinding(StackHashCommands.ExportCommand);
            exportCommand.CanExecute += new CanExecuteRoutedEventHandler(exportCommand_CanExecute);
            CommandBindings.Add(exportCommand);

            CommandBinding exportProductListCommand = new CommandBinding(StackHashCommands.ExportProductListCommand);
            exportProductListCommand.CanExecute += new CanExecuteRoutedEventHandler(exportProductListCommand_CanExecute);
            exportProductListCommand.Executed += new ExecutedRoutedEventHandler(exportProductListCommand_Executed);
            CommandBindings.Add(exportProductListCommand);

            CommandBinding exportEventListCommand = new CommandBinding(StackHashCommands.ExportEventListCommand);
            exportEventListCommand.CanExecute += new CanExecuteRoutedEventHandler(exportEventListCommand_CanExecute);
            exportEventListCommand.Executed += new ExecutedRoutedEventHandler(exportEventListCommand_Executed);
            CommandBindings.Add(exportEventListCommand);

            CommandBinding exportEventListFullCommand = new CommandBinding(StackHashCommands.ExportEventListFullCommand);
            exportEventListFullCommand.CanExecute += new CanExecuteRoutedEventHandler(exportEventListFullCommand_CanExecute);
            exportEventListFullCommand.Executed += new ExecutedRoutedEventHandler(exportEventListFullCommand_Executed);
            CommandBindings.Add(exportEventListFullCommand);

            CommandBinding helpCommand = new CommandBinding(ApplicationCommands.Help);
            helpCommand.Executed += new ExecutedRoutedEventHandler(helpCommand_Executed);
            CommandBindings.Add(helpCommand);

            CommandBinding downloadCabCommand = new CommandBinding(StackHashCommands.DownloadCabCommand);
            downloadCabCommand.CanExecute += new CanExecuteRoutedEventHandler(downloadCabCommand_CanExecute);
            downloadCabCommand.Executed += new ExecutedRoutedEventHandler(downloadCabCommand_Executed);
            CommandBindings.Add(downloadCabCommand);

            CommandBinding syncReportCommand = new CommandBinding(StackHashCommands.SyncReportCommand);
            syncReportCommand.CanExecute += new CanExecuteRoutedEventHandler(syncReportCommand_CanExecute);
            syncReportCommand.Executed += new ExecutedRoutedEventHandler(syncReportCommand_Executed);
            CommandBindings.Add(syncReportCommand);

            CommandBinding extractAllCabsCommand = new CommandBinding(StackHashCommands.ExtractAllCabsCommand);
            extractAllCabsCommand.CanExecute += new CanExecuteRoutedEventHandler(extractAllCabsCommand_CanExecute);
            extractAllCabsCommand.Executed += new ExecutedRoutedEventHandler(extractAllCabsCommand_Executed);
            CommandBindings.Add(extractAllCabsCommand);

            CommandBinding sendAllToPluginsCommand = new CommandBinding(StackHashCommands.SendAllToPluginsCommand);
            sendAllToPluginsCommand.CanExecute += new CanExecuteRoutedEventHandler(sendAllToPluginsCommand_CanExecute);
            sendAllToPluginsCommand.Executed += new ExecutedRoutedEventHandler(sendAllToPluginsCommand_Executed);
            CommandBindings.Add(sendAllToPluginsCommand);

            CommandBinding syncProductCommand = new CommandBinding(StackHashCommands.SyncProductCommand);
            syncProductCommand.CanExecute += new CanExecuteRoutedEventHandler(syncProductCommand_CanExecute);
            syncProductCommand.Executed += new ExecutedRoutedEventHandler(syncProductCommand_Executed);
            CommandBindings.Add(syncProductCommand);

            CommandBinding resyncProductCommand = new CommandBinding(StackHashCommands.ResyncProductCommand);
            resyncProductCommand.CanExecute += new CanExecuteRoutedEventHandler(resyncProductCommand_CanExecute);
            resyncProductCommand.Executed += new ExecutedRoutedEventHandler(resyncProductCommand_Executed);
            CommandBindings.Add(resyncProductCommand);

            CommandBinding uploadMappingCommand = new CommandBinding(StackHashCommands.UploadMappingCommand);
            uploadMappingCommand.CanExecute += new CanExecuteRoutedEventHandler(uploadMappingCommand_CanExecute);
            uploadMappingCommand.Executed += new ExecutedRoutedEventHandler(uploadMappingCommand_Executed);
            CommandBindings.Add(uploadMappingCommand);

            CommandBinding openCabFolderCommand = new CommandBinding(StackHashCommands.OpenCabFolderCommand);
            openCabFolderCommand.CanExecute += new CanExecuteRoutedEventHandler(openCabFolderCommand_CanExecute);
            openCabFolderCommand.Executed += new ExecutedRoutedEventHandler(openCabFolderCommand_Executed);
            CommandBindings.Add(openCabFolderCommand);

            CommandBinding showEventsWithoutCabsCommand = new CommandBinding(StackHashCommands.ShowEventsWithoutCabsCommand);
            showEventsWithoutCabsCommand.CanExecute += new CanExecuteRoutedEventHandler(showEventsWithoutCabsCommand_CanExecute);
            showEventsWithoutCabsCommand.Executed += new ExecutedRoutedEventHandler(showEventsWithoutCabsCommand_Executed);
            CommandBindings.Add(showEventsWithoutCabsCommand);
        }

        void uploadMappingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((_clientLogic.NotBusy) &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId))
            {
                DoUploadMappingFile();
            }
        }

        void uploadMappingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.NotBusy) &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId));
        }

        void sendAllToPluginsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((_clientLogic.NotBusy) &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (_clientLogic.ActivePlugins != null) &&
                (_clientLogic.ActivePlugins.Count > 0) &&
                (!_clientLogic.PluginHasError))
            {
                DoSendAllToPlugins();
            }
        }

        void sendAllToPluginsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.NotBusy) && 
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (_clientLogic.ActivePlugins != null) &&
                (_clientLogic.ActivePlugins.Count > 0) &&
                (!_clientLogic.PluginHasError));
        }

        void extractAllCabsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((_clientLogic.CurrentEventPackage != null) && (_clientLogic.CurrentEventPackage.Cabs.Count > 0))
            {
                // build list of cabs to extract
                _cabsToExtract = new Queue<StackHashCab>();
                foreach (DisplayCab cab in _clientLogic.CurrentEventPackage.Cabs)
                {
                    _cabsToExtract.Enqueue(cab.StackHashCabPackage.Cab);
                }

                // prompt for extract folder and start extracting
                DoExtractFirstCab();
            }
        }

        void extractAllCabsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentEventPackage != null) &&
                (_clientLogic.CurrentEventPackage.Cabs.Count > 0) &&
                (_clientLogic.NotBusy));
        }

        void downloadCabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _clientLogic.DownloadCab();
        }

        void downloadCabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (!_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void openCabFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DefaultBrowser.OpenFolder(System.IO.Path.GetDirectoryName(_clientLogic.CurrentCab.FullPath));
        }

        void openCabFolderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.ServiceIsLocal) && (_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void helpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (_clientLogic.CurrentView)
            {
                case ClientLogicView.ProductList:
                    StackHashHelp.ShowTopic("main-window-product-list.htm");
                    break;

                case ClientLogicView.EventList:
                    StackHashHelp.ShowTopic("main-window-event-list.htm");
                    break;

                case ClientLogicView.EventDetail:
                    StackHashHelp.ShowTopic("main-window-event-details.htm");
                    break;

                case ClientLogicView.CabDetail:
                    StackHashHelp.ShowTopic("main-window-cab-details.htm");
                    break;

                default:
                    StackHashHelp.ShowTopic("main-window.htm");
                    break;
            }
        }

        void exportEventListFullCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoExportEventList(true);
        }

        void exportEventListFullCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.EventPackages != null) &&
                (_clientLogic.EventPackages.Count > 0) &&
                (_clientLogic.NotBusy));
        }

        void exportEventListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoExportEventList(false);
        }

        void exportEventListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.EventPackages != null) && 
                (_clientLogic.EventPackages.Count > 0) &&
                (_clientLogic.NotBusy));
        }

        void exportProductListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoExportProductList();
        }

        void exportProductListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.Products != null) && 
                (_clientLogic.Products.Count > 0) &&
                (_clientLogic.NotBusy));
        }

        void exportCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // if either of the export commands are enabled...
            if (_clientLogic.NotBusy)
            {
                e.CanExecute = (((_clientLogic.EventPackages != null) && (_clientLogic.EventPackages.Count > 0)) ||
                    ((_clientLogic.Products != null) && (_clientLogic.Products.Count > 0)));
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void SetCommandTargetIfNotInEditScope(MenuItem menuItem, Button button)
        {
            // set the command target to the keyboard focused element unless this is in the edit menu
            // or edit toolbar or it's this window - this prevents a WPF problem where commands cannot
            // target an element in a different nested focus scope (see Case 418)
            IInputElement keyboardFocusedElement = Keyboard.FocusedElement;
            if ((keyboardFocusedElement != null) && (keyboardFocusedElement != this))
            {
                DependencyObject keyboardFocusedDependencyObject = keyboardFocusedElement as DependencyObject;
                if (keyboardFocusedDependencyObject != null)
                {
                    DependencyObject targetFocusScope = FocusManager.GetFocusScope(keyboardFocusedDependencyObject);
                    if ((targetFocusScope != _editMenuFocusScope) && (targetFocusScope != _editToolbarFocusScope))
                    {
                        if (menuItem != null)
                        {
                            menuItem.CommandTarget = keyboardFocusedElement;
                        }

                        if (button != null)
                        {
                            button.CommandTarget = keyboardFocusedElement;
                        }
                    }
                }
            }
        }

        void selectAllCommand_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SetCommandTargetIfNotInEditScope(menuItemSelectAll, null);
        }

        void pasteCommand_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SetCommandTargetIfNotInEditScope(menuItemPaste, toolbarButtonPaste);
        }

        void copyCommnad_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SetCommandTargetIfNotInEditScope(menuItemCopy, toolbarButtonCopy);
        }

        void cutCommand_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SetCommandTargetIfNotInEditScope(menuItemCut, toolbarButtonCut);
        }

        void debugUsingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void runScriptByNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoRunScript(e.Parameter as string);   
        }

        void runScriptByNameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void runScriptCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && 
                (_clientLogic.CurrentCab.CabDownloaded) &&
                (_clientLogic.ScriptData != null) && 
                (_clientLogic.ScriptData.Count > 0) &&
                (_clientLogic.NotBusy));
        }

        void extractCabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoExtractCabLocal();
        }

        void extractCabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void refreshCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _clientLogic.RefreshCurrentView();
        }

        void refreshCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.NotBusy && (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId));
        }

        private void UpdateShowDisabledProductsCore()
        {
            menuItemShowDisabledProducts.IsChecked = UserSettings.Settings.ShowDisabledProducts;
            _clientLogic.CurrentView = ClientLogicView.ProductList;
            _clientLogic.PopulateProducts();
        }

        private void viewMainProductList_ShowDisabledProducts(object sender, EventArgs e)
        {
            if (!UserSettings.Settings.ShowDisabledProducts)
            {
                UserSettings.Settings.ShowDisabledProducts = true;
                UpdateShowDisabledProductsCore();
            }
        }

        void showDisabledProductCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettings.Settings.ShowDisabledProducts = !UserSettings.Settings.ShowDisabledProducts;
            UpdateShowDisabledProductsCore();
        }

        void showEventsWithoutCabsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UserSettings.Settings.ShowEventsWithoutCabs = !UserSettings.Settings.ShowEventsWithoutCabs;
            menuItemShowEventsWithoutCabs.IsChecked = UserSettings.Settings.ShowEventsWithoutCabs;
            _clientLogic.CurrentView = ClientLogicView.ProductList;
            _clientLogic.PopulateProducts();
        }

        void showEventsWithoutCabsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _clientLogic.NotBusy;
        }

        void showDisabledProductCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _clientLogic.NotBusy && _clientLogic.AnyEnabledProducts;
        }

        void serviceStatusCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (ServiceStatusWindow serviceStatusWindow = new ServiceStatusWindow(_clientLogic))
            {
                serviceStatusWindow.Owner = this;
                serviceStatusWindow.ShowDialog();
            }
        }

        void optionsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoLocalSettings(true);
        }

        void optionsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _clientLogic.NotBusy;
        }

        void debugX64Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_clientLogic.CurrentCab != null)
            {
                _nextArchitecture = DumpArchitecture.X64;
                _nextDebugger = DefaultDebugger.DebuggingToolsForWindows;
                _actionOnCabExtract = ActionOnCabExtract.Debug;
                _clientLogic.ExtractCurrentCab();
            }
        }

        void debugX64Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void debugX86Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_clientLogic.CurrentCab != null)
            {
                _nextArchitecture = DumpArchitecture.X86;
                _nextDebugger = DefaultDebugger.DebuggingToolsForWindows;
                _actionOnCabExtract = ActionOnCabExtract.Debug;
                _clientLogic.ExtractCurrentCab();
            }
        }

        void debugX86Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void debugVisualStudioCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_clientLogic.CurrentCab != null)
            {
                _nextArchitecture = DumpArchitecture.Unknown;
                _nextDebugger = DefaultDebugger.VisualStudio;
                _actionOnCabExtract = ActionOnCabExtract.Debug;
                _clientLogic.ExtractCurrentCab();
            }
        }

        void debugVisualStudioCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void commandDebug_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_clientLogic.CurrentCab != null)
            {
                _nextArchitecture = DumpArchitecture.Unknown;
                _nextDebugger = UserSettings.Settings.DefaultDebugTool;
                _actionOnCabExtract = ActionOnCabExtract.Debug;
                _clientLogic.ExtractCurrentCab();
            }
        }

        void commandDebug_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentCab.CabDownloaded) && (_clientLogic.NotBusy));
        }

        void commandBindingHome_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _clientLogic.CurrentView = ClientLogicView.ProductList;
        }

        void commandBindingHome_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // can go home unless we're on the product page or ClientLogic is busy
            e.CanExecute = ((_clientLogic.CurrentView != ClientLogicView.ProductList) && _clientLogic.NotBusy);
        }

        void commandBindingBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (_clientLogic.CurrentView)
            {
                case ClientLogicView.ProductList:
                default:
                    Debug.Assert(false, "Back should be disabled on the product list");
                    break;

                case ClientLogicView.EventList:
                    _clientLogic.CurrentView = ClientLogicView.ProductList;
                    break;

                case ClientLogicView.EventDetail:
                    _clientLogic.CurrentView = ClientLogicView.EventList;
                    break;

                case ClientLogicView.CabDetail:
                    _clientLogic.CurrentView = ClientLogicView.EventDetail;
                    break;
            }
        }

        void commandBindingBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // can go back unless we're on the product page or ClientLogic is busy
            e.CanExecute = ((_clientLogic.CurrentView != ClientLogicView.ProductList) && _clientLogic.NotBusy);
        }

        void commandBindingBuildSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SearchBuilder searchBuilder = new SearchBuilder(comboBoxSearch.Text);
            searchBuilder.Owner = this;

            if (searchBuilder.ShowDialog() == true)
            {
                comboBoxSearch.Text = searchBuilder.SearchString;
                DoSearch();
            }
        }

        void commandBindingBuildSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _clientLogic.NotBusy;
        }

        void commandBindingClearSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClearSearch();
        }

        void commandBindingClearSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((comboBoxSearch.Text.Length > 0) && (_clientLogic.NotBusy));
        }

        void commandBindingSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoSearch();
        }

        void commandBindingSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((comboBoxSearch.Text.Length > 0) && (_clientLogic.NotBusy));
        }

        void commandBindingProfileManager_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoProfileSettings();
        }

        void commandBindingProfileManager_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _clientLogic.NotBusy;
        }

        void commandBindingScriptManager_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ScriptManager scriptManager = new ScriptManager(_clientLogic);
            scriptManager.Owner = this;
            
            scriptManager.ShowDialog();
        }

        void commandBindingScriptManager_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _clientLogic.NotBusy;
        }

        void commandBindingCancelSync_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _clientLogic.AdminCancelSync();
        }

        void commandBindingCancelSync_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.SyncRunning && _clientLogic.NotBusy);
        }

        void syncReportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            statusBarItemSyncSucceeded.Visibility = Visibility.Collapsed;
            statusBarItemSyncFailed.Visibility = Visibility.Collapsed;
            _clientLogic.AdminGetContextStatus();
        }

        void syncReportCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.NotBusy &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (!_clientLogic.SyncRunning));
        }

        void resyncProductCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckForSyncWarnings())
            {
                List<int> productsToSync = new List<int>();
                productsToSync.Add(_clientLogic.CurrentProduct.Id);
                _clientLogic.AdminStartSync(true, productsToSync);
            }
        }

        void resyncProductCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.NotBusy &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (!_clientLogic.SyncRunning) &&
                (_clientLogic.CurrentProduct != null) &&
                (_clientLogic.CurrentProduct.SynchronizeEnabled));
        }

        void syncProductCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckForSyncWarnings())
            {
                List<int> productsToSync = new List<int>();
                productsToSync.Add(_clientLogic.CurrentProduct.Id);
                _clientLogic.AdminStartSync(false, productsToSync);
            }
        }

        void syncProductCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.NotBusy &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (!_clientLogic.SyncRunning) &&
                (_clientLogic.CurrentProduct != null) &&
                (_clientLogic.CurrentProduct.SynchronizeEnabled));
        }

        void commandBindingSync_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckForAnySyncEnabledProducts())
            {
                if (CheckForSyncWarnings())
                {
                    _clientLogic.AdminStartSync(false, null);
                }
            }
        }

        void commandBindingSync_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.NotBusy && 
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (!_clientLogic.SyncRunning));
        }

        void commandBindingResync_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckForAnySyncEnabledProducts())
            {
                if (CheckForSyncWarnings())
                {
                    _clientLogic.AdminStartSync(true, null);
                }
            }
        }

        void commandBindingResync_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_clientLogic.NotBusy &&
                (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId) &&
                (!_clientLogic.SyncRunning));
        }

        void commandBindingExit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        void commandBindingAbout_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void commandBindingAbout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoShowAbout();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose the MainWindow
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool canDisposeManagedResources)
        {
            if (canDisposeManagedResources)
            {
                if (_clientLogic != null)
                {
                    if (!_clientLogic.IsDisposed)
                    {
                        _clientLogic.Dispose();
                    }
                    _clientLogic = null;
                }
            }
        }

        /// <summary />
        ~MainWindow()
        {
            Dispose(false);
        }

        #endregion

        private void MenuItem_IsEnabledChangedDimIcon(object sender, DependencyPropertyChangedEventArgs e)
        {
            // dim icon (if present) if disabled... can't seem to do this through a style in App.xaml like
            // with the buttons
            MenuItem menu = sender as MenuItem;
            if (menu != null)
            {
                Image menuImage = menu.Icon as Image;
                if (menuImage != null)
                {
                    if (menu.IsEnabled)
                    {
                        menuImage.Opacity = 1.0;
                    }
                    else
                    {
                        menuImage.Opacity = 0.5;
                    }
                }
            }
        }

        private void HandleSearchRequest(object sender, SearchRequestEventArgs e)
        {
            comboBoxSearch.Text = e.Search;
            DoSearch();
        }

        private void viewMainProductList_SearchOrPopulateEvents(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void View_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // ignore mouse if busy
            e.Handled = !_clientLogic.NotBusy;
        }

        private void comboBoxProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // changing the selection will set _clientLogic.CurrentContext but will not update UserSettings
            if ((_clientLogic.NotBusy) && (_clientLogic.CurrentContext != null))
            {
                if (_clientLogic.CurrentContext.Id != UserSettings.Settings.CurrentContextId)
                {
                    UserSettings.Settings.CurrentContextId = _clientLogic.CurrentContext.Id;
                    _clientLogic.CheckForContextIdChange();
                }
            }
        }
    }
}
