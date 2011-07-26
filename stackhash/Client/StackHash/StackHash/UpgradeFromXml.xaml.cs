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
using StackHashUtilities;
using StackHash.StackHashService;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Window used to upgrade indexes from XML to SQL
    /// </summary>
    public partial class UpgradeFromXml : Window
    {
        private List<StackHashContextSettings> _xmlSettings;
        private ClientLogic _clientLogic;
        private Process _dbConfigProcess;
        private int _copyContextId;
        private bool _copyContextIsActive;

        /// <summary>
        /// Window used to upgrade indexes from XML to SQL
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        public UpgradeFromXml(ClientLogic clientLogic)
        {
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }
            _clientLogic = clientLogic;

            InitializeComponent();

            _xmlSettings = new List<StackHashContextSettings>();
        }

        private void UpdateState()
        {
            buttonUpgrade.IsEnabled = listBoxProfiles.SelectedItem is StackHashContextSettings;
        }

        private void UpdateXmlSettings()
        {
            _xmlSettings.Clear();

            if (_clientLogic.ContextCollection != null)
            {
                foreach (DisplayContext settings in _clientLogic.ContextCollection)
                {
                    if (settings.StackHashContextSettings.ErrorIndexSettings.Type == ErrorIndexType.Xml)
                    {
                        _xmlSettings.Add(settings.StackHashContextSettings);
                    }
                }
            }

            if (_xmlSettings.Count == 0)
            {
                // no more upgrades requied, close the window
                DialogResult = true;
                Close();
            }
            else
            {
                // update list
                listBoxProfiles.ItemsSource = _xmlSettings;
                listBoxProfiles.SelectedIndex = 0;
            }

            UpdateState();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            StackHashContextSettings settings = listBoxProfiles.SelectedItem as StackHashContextSettings;
            if (settings != null)
            {
                if (StackHashMessageBox.Show(this,
                   string.Format(CultureInfo.CurrentCulture,
                   Properties.Resources.ProfileManager_DeleteProfileMBMessage,
                   settings.WinQualSettings.CompanyName,
                   settings.ErrorIndexSettings.Folder),
                   Properties.Resources.ProfileManager_DeleteProfileMBTitle,
                   StackHashMessageBoxType.YesNo,
                   StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
                {
                    // if the current context is deleted then go back to the invalid state
                    if (UserSettings.Settings.CurrentContextId == settings.Id)
                    {
                        UserSettings.Settings.CurrentContextId = UserSettings.InvalidContextId;
                    }

                    // remove the context
                    _clientLogic.AdminRemoveContext(settings.Id);
                }
            }
        }

        private void buttonUpgrade_Click(object sender, RoutedEventArgs e)
        {
            StackHashContextSettings settings = listBoxProfiles.SelectedItem as StackHashContextSettings;
            if (settings != null)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    DBConfigSettings.Settings.ResetSettings();
                    DBConfigSettings.Settings.IsNewProfile = true;
                    DBConfigSettings.Settings.IsUpgrade = true;
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

                    DBConfigSettings.Settings.ProfileName = settings.ErrorIndexSettings.Name;
                    DBConfigSettings.Settings.ProfileFolder = null; // not populated as user is required to change this
                    DBConfigSettings.Settings.ConnectionString = null;
                    DBConfigSettings.Settings.Save();

                    _copyContextId = settings.Id;
                    _copyContextIsActive = settings.IsActive;

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

                    _clientLogic.AdminCopyIndex(_copyContextId,
                        DBConfigSettings.Settings.ProfileFolder,
                        DBConfigSettings.Settings.ProfileName,
                        DBConfigSettings.Settings.ConnectionString,
                        DBConfigSettings.Settings.ProfileName,
                        _copyContextIsActive,
                        DBConfigSettings.Settings.IsDatabaseInCabFolder);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, EventArgs>(_dbConfigProcess_Exited), sender, e);
            }
        }

        private void listBoxProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateState();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                _clientLogic.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_clientLogic_PropertyChanged);
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
            }

            UpdateXmlSettings();
        }

        private void UpdateContextSettings()
        {
            _clientLogic.RefreshContextSettings();
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                switch (e.UIRequest)
                {
                    case ClientLogicUIRequest.ContextCollectionReady:
                        UpdateXmlSettings();
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

        void _clientLogic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "NotBusy")
                {
                    if (_clientLogic.NotBusy)
                    {
                        gridMain.IsEnabled = true;
                        Mouse.OverrideCursor = null;
                    }
                    else
                    {
                        gridMain.IsEnabled = false;
                        Mouse.OverrideCursor = Cursors.Wait;
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            if (StackHashMessageBox.Show(this,
                Properties.Resources.UpgradeFromXml_CloseMBMessage,
                Properties.Resources.UpgradeFromXml_CloseMBTitle,
                StackHashMessageBoxType.YesNo,
                StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("upgrade-profile.htm");
        }
    }
}
