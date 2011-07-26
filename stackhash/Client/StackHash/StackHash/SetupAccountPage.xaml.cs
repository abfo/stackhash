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
using System.Diagnostics;
using StackHashUtilities;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using StackHash.StackHashService;
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// Account page for the setup wizard
    /// </summary>
    public partial class SetupAccountPage : SetupBasePage
    {
        private bool _firstInitComplete;
        private bool _is64;
        private string _profileName;
        private string _contextDatabaseName;
        private bool _isDatabaseInCabFolder;
        private string _username;
        private string _password;
        private string _path32;
        private string _path64;
        private string _pathLocalStore;
        private string _connectionString;
        private Process _dbConfigProcess;

        /// <summary>
        /// Account page for the setup wizard
        /// </summary>
        public SetupAccountPage()
        {
            InitializeComponent();

            this.BackEnabled = true;
            this.NextEnabled = true;
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's now active
        /// </summary>
        public override void PageActivated()
        {
            if (!_firstInitComplete)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    // testing
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
                        

                    // disable 64-bit path if not on a 64 bit system
                    _is64 = SystemInformation.Is64BitSystem();
                    label64.IsEnabled = _is64;
                    textBox64.IsEnabled = _is64;
                    buttonBrowse64.IsEnabled = _is64;

                    string cdb32 = ClientUtils.GetCdbPath(ImageFileMachine.I386);
                    if (cdb32 != null)
                    {
                        textBox32.Text = cdb32;
                    }

                    if (_is64)
                    {
                        string cdb64 = ClientUtils.GetCdbPath(ImageFileMachine.AMD64);
                        if (cdb64 != null)
                        {
                            textBox64.Text = cdb64;
                        }
                    }

                    // load any previous settings
                    DBConfigSettings.Settings.Load();

                    // DBConfigSettings Profile name is the context DATABASE NAME
                    if (!string.IsNullOrEmpty(DBConfigSettings.Settings.ProfileName))
                    {
                        _contextDatabaseName = DBConfigSettings.Settings.ProfileName;
                    }

                    _isDatabaseInCabFolder = DBConfigSettings.Settings.IsDatabaseInCabFolder;

                    if (!string.IsNullOrEmpty(DBConfigSettings.Settings.ConnectionString))
                    {
                        textBoxDatabase.Text = DBConfigSettings.Settings.ConnectionString;
                    }

                    _firstInitComplete = true;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }

            this.ClientLogic.ClientLogicSetupWizardPrompt += new EventHandler<ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt);

            UpdateState();
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's no longer active
        /// </summary>
        public override void PageDeactivated()
        {
            this.ClientLogic.ClientLogicSetupWizardPrompt -= ClientLogic_ClientLogicSetupWizardPrompt;
        }

        /// <summary>
        /// Request navigation to the previous page - ShowPreviousPage will fire if this is successfull
        /// </summary>
        public override void TryBack()
        {
            // can always go back
            DoRaiseShowPreviousPage();
        }

        /// <summary>
        /// Request navigation to the next page - ShowNextPage will fire if this is successfull
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void TryNext()
        {
            UpdateState();
            if (this.NextEnabled)
            {
                // try to save the client WinDbg paths from the cdb paths (if present)
                string path32WinDbg = string.Empty;
                string path64WinDbg = string.Empty;

                if (!string.IsNullOrEmpty(_path32))
                {
                    try
                    {
                        path32WinDbg = System.IO.Path.GetDirectoryName(_path32);
                        path32WinDbg = System.IO.Path.Combine(path32WinDbg, "WinDbg.exe");
                        if (File.Exists(path32WinDbg))
                        {
                            UserSettings.Settings.DebuggerPathX86 = path32WinDbg;
                        }
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(_path64))
                {
                    try
                    {
                        path64WinDbg = System.IO.Path.GetDirectoryName(_path64);
                        path64WinDbg = System.IO.Path.Combine(path64WinDbg, "WinDbg.exe");
                        if (File.Exists(path64WinDbg))
                        {
                            UserSettings.Settings.DebuggerPathAmd64 = path64WinDbg;
                        }
                    }
                    catch { }
                }

                if (Directory.Exists(_pathLocalStore))
                {
                    // add or update the context
                    this.ClientLogic.AdminCreateFirstContext(_contextDatabaseName,
                        _username,
                        _password,
                        _path32,
                        _path64,
                        _pathLocalStore,
                        _connectionString,
                        _profileName,
                        _isDatabaseInCabFolder);
                }
                else
                {
                    StackHashMessageBox.Show(Window.GetWindow(this),
                        Properties.Resources.SetupWizard_IndexFolderMisingMBMessage,
                        Properties.Resources.SetupWizard_IndexFolderMissingMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error);
                }
            }
        }

        private void ClientLogic_ClientLogicSetupWizardPrompt(object sender, ClientLogicSetupWizardPromptEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                switch (e.Prompt)
                {
                    case ClientLogicSetupWizardPromptOperation.FirstContextCreated:
                        if (e.Succeeded)
                        {
                            // test the provided WinQual Credentials
                            this.ClientLogic.AdminTestWinQualLogOn(UserSettings.Settings.CurrentContextId, _username, _password);
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;

                            StackHashMessageBox.Show(Window.GetWindow(this),
                                Properties.Resources.SetupWizard_AddContextFailedMBMessage,
                                Properties.Resources.SetupWizard_AddContextFailedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error,
                                e.LastException,
                                e.LastServiceError);
                        }
                        break;

                    case ClientLogicSetupWizardPromptOperation.LogOnTestComplete:
                        if (e.Succeeded)
                        {
                            // sync with WinQual to get the product list
                            this.DisableWizard = true; // force diable as the client won't stay busy during the sync
                            this.ClientLogic.AdminStartSync(false, true, true, null);
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;

                            StackHashMessageBox.Show(Window.GetWindow(this),
                                Properties.Resources.SetupWizard_LogOnTestFailedMBMessage,
                                Properties.Resources.SetupWizard_LogOnTestFailedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error,
                                e.LastException,
                                e.LastServiceError);
                        }
                        break;

                    case ClientLogicSetupWizardPromptOperation.ProductListUpdated:
                        // always enable the wizard again when the sync completes
                        this.DisableWizard = false;

                        if (e.Succeeded)
                        {
                            // we have the product list, can go to the next page
                            this.DoRaiseShowNextPage();
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;

                            StackHashMessageBox.Show(Window.GetWindow(this),
                                Properties.Resources.SetupWizard_GetProductListFailedMBMessage,
                                Properties.Resources.SetupWizard_GetProductListFailedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error,
                                e.LastException,
                                e.LastServiceError);
                        }
                        break;

                    case ClientLogicSetupWizardPromptOperation.SyncFailed:
                        // always enable the wizard again when the sync fails
                        this.DisableWizard = false;

                        // report the error
                        Mouse.OverrideCursor = null;
                        StackHashMessageBox.Show(Window.GetWindow(this),
                            Properties.Resources.Error_SyncFailedMBMessage,
                            Properties.Resources.Error_SyncFailedMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error,
                            e.LastException,
                            e.LastServiceError);
                        break;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt),
                    sender, e);
            }
        }

        private void UpdateState()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(_profileName)) { valid = false; }
            if (string.IsNullOrEmpty(_username)) { valid = false; }
            if (string.IsNullOrEmpty(_password)) { valid = false; }
            if (string.IsNullOrEmpty(_connectionString)) { valid = false; }

            if (string.IsNullOrEmpty(_pathLocalStore))
            {
                valid = false;
            }

            if (!string.IsNullOrEmpty(_path32))
            {
                if (!File.Exists(_path32))
                {
                    valid = false;
                }
            }

            if (!string.IsNullOrEmpty(_path64))
            {
                if (!File.Exists(_path64))
                {
                    valid = false;
                }
            }

            this.NextEnabled = valid;
        }

        private void buttonBrowse64_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textBox64.Text))
            {
                ofd.FileName = textBox64.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.ProfileAddEdit_BrowseWinQualTitle64;

            if (ofd.ShowDialog(Window.GetWindow(this)) == true)
            {
                textBox64.Text = ofd.FileName;
            }
        }

        private void buttonBrowse32_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textBox32.Text))
            {
                ofd.FileName = textBox32.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.ProfileAddEdit_BrowseWinQualTitle32;

            if (ofd.ShowDialog(Window.GetWindow(this)) == true)
            {
                textBox32.Text = ofd.FileName;
            }
        }

        private void textBoxProfileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _profileName = textBoxProfileName.Text;
            UpdateState();
        }

        private void textBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            _username = textBoxUsername.Text;
            UpdateState();
        }

        private void passwordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _password = passwordBoxPassword.Password;
            UpdateState();
        }

        private void textBox32_TextChanged(object sender, TextChangedEventArgs e)
        {
            _path32 = textBox32.Text;
            UpdateState();
        }

        private void textBox64_TextChanged(object sender, TextChangedEventArgs e)
        {
            _path64 = textBox64.Text;
            UpdateState();
        }

        private void linkWinQual_Click(object sender, RoutedEventArgs e)
        {
            DefaultBrowser.OpenUrlInInternetExplorer(linkWinQual.NavigateUri.ToString());
        }

        private void linkTools_Click(object sender, RoutedEventArgs e)
        {
            DefaultBrowser.OpenUrl(linkTools.NavigateUri.ToString());
        }

        private void buttonSelectDatabse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // save settings (to pass to the config tool)
                DBConfigSettings.Settings.ResetSettings();
                DBConfigSettings.Settings.IsNewProfile = true;
                DBConfigSettings.Settings.ServiceHost = ServiceProxy.Services.ServiceHost;
                DBConfigSettings.Settings.ServicePort = ServiceProxy.Services.ServicePort;
                DBConfigSettings.Settings.ServiceUsername = UserSettings.Settings.ServiceUsername;
                DBConfigSettings.Settings.ServicePassword = UserSettings.Settings.ServicePassword;
                DBConfigSettings.Settings.ServiceDomain = UserSettings.Settings.ServiceDomain;

                if (this.ClientLogic.ContextCollection != null)
                {
                    foreach (DisplayContext settings in this.ClientLogic.ContextCollection)
                    {
                        DBConfigSettings.Settings.ExistingProfileFolders.Add(settings.StackHashContextSettings.ErrorIndexSettings.Folder);
                        DBConfigSettings.Settings.ExistingProfileNames.Add(settings.StackHashContextSettings.ErrorIndexSettings.Name);
                    }
                }

                DBConfigSettings.Settings.IsDatabaseInCabFolder = _isDatabaseInCabFolder;
                DBConfigSettings.Settings.ProfileFolder = _pathLocalStore;
                DBConfigSettings.Settings.ConnectionString = _connectionString;
                DBConfigSettings.Settings.Save();

                Mouse.OverrideCursor = Cursors.Wait;
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

                    StackHashMessageBox.Show(Window.GetWindow(this),
                        Properties.Resources.DBConfigLaunchFailedMBMessage,
                        Properties.Resources.DBConfigLaunchFailedMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Error,
                        ex,
                        StackHashMessageBox.ParseServiceErrorFromException(ex));
                }
            }
        }

        private void _dbConfigProcess_Exited(object sender, EventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
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

                    _pathLocalStore = DBConfigSettings.Settings.ProfileFolder;
                    _contextDatabaseName = DBConfigSettings.Settings.ProfileName; // not the ProfileName for DBConfigSettings is the DATABASE NAME
                    _isDatabaseInCabFolder = DBConfigSettings.Settings.IsDatabaseInCabFolder;
                    textBoxDatabase.Text = DBConfigSettings.Settings.ConnectionString;
                }

                Mouse.OverrideCursor = null;
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, EventArgs>(_dbConfigProcess_Exited), sender, e);
            }
        }

        private void textBoxDatabase_TextChanged(object sender, TextChangedEventArgs e)
        {
            _connectionString = textBoxDatabase.Text;
            UpdateState();
        }
    }
}
