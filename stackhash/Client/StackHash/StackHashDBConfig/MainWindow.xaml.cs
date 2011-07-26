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
using StackHash;
using System.Diagnostics;
using StackHashSqlControl;
using Microsoft.Win32;
using System.Globalization;
using System.Data.SqlClient;
using StackHash.StackHashService;
using System.ComponentModel;
using System.Reflection;

namespace StackHashDBConfig
{
    /// <summary>
    /// StackHash Database Configuration Tool Main Window
    /// </summary>
    public partial class MainWindow : Window
    {
        private class WorkerArg
        {
            public bool TestConnectionOnly { get; private set; }
            public bool CreateDatabase { get; private set; }
            public bool UseDefaultLocation { get; private set; }
            public string CabFolder { get; private set; }
            public string ConnectionString { get; private set; }
            public string MasterConnectionString { get; private set; }
            public string ProfileName { get; private set; }

            public WorkerArg(bool testConnectionOnly,
                bool createDatabase,
                bool useDefaultLocation,
                string cabFolder,
                string connectionString,
                string masterConnectionString,
                string profileName)
            {
                this.TestConnectionOnly = testConnectionOnly;
                this.CreateDatabase = createDatabase;
                this.UseDefaultLocation = useDefaultLocation;
                this.CabFolder = cabFolder;
                this.ConnectionString = connectionString;
                this.MasterConnectionString = masterConnectionString;
                this.ProfileName = profileName;
            }
        }

        private class WorkerResult
        {
            public bool TestConnectionOnly { get; set; }
            public StackHashErrorIndexDatabaseStatus TestStatus { get; set; }
            public string TestLastExceptionText { get; set; }
            public Exception WorkerException { get; set; }
            public bool CanAccessCabFolder { get; set; }
            public string CabFolderExceptionText { get; set; }
        }

        private const int DefaultSqlMinPoolSize = 6;
        private const int DefaultSqlMaxPoolSize = 100;
        private const int DefaultSqlConnectionTimeout = 15;
        private const int DefaultSqlEventsPerBlock = 100;

        private DatabaseList _databaseList;
        private DatabaseInstance _selectedInstance;
        private bool _configurationOK;
        private bool _configurationSucceeded;
        private string _initialProfileName;
        private string _initialCabFolder;
        private string _initialConnectionString;
        private bool _suppressConnectionStringChanges;
        private BackgroundWorker _worker;
        private int _clientDataRequestId;
        private Guid _clientDataSessionId;
        private bool _suppressInstanceListErrors;
        private bool _localServiceConfigComplete;
        private Guid _localServiceGuid;

        /// <summary>
        /// StackHash Database Configuration Tool Main Window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);

            // allocate a session Id;
            _clientDataSessionId = Guid.NewGuid();
        }

        private StackHashClientData GenerateClientData()
        {
            StackHashClientData clientData = new StackHashClientData();
            clientData.ApplicationGuid = _clientDataSessionId;
            clientData.ClientId = 0;
            clientData.ClientName = Environment.UserName + "_StackHashDBConfig";
            clientData.ClientRequestId = _clientDataRequestId;
            clientData.ServiceGuid = _localServiceGuid == Guid.Empty ? null : _localServiceGuid.ToString();

            _clientDataRequestId++;

            return clientData;
        }

        private void UpdateState(bool updateConnectionString)
        {
            if (updateConnectionString)
            {
                UpdateConnectionString();
            }

            _configurationOK = ((!string.IsNullOrEmpty(textBoxCabFolder.Text)) && 
                (!string.IsNullOrEmpty(textBoxDatabaseName.Text)) &&
                (!string.IsNullOrEmpty(textBoxConnectionString.Text)) &&       
                (System.IO.Directory.Exists(textBoxCabFolder.Text)));

            buttonOK.IsEnabled = _configurationOK;
            buttonRefresh.IsEnabled = comboBoxDatabase.SelectedItem is DatabaseSettings;

            buttonTestConnectionString.IsEnabled = ((comboBoxDatabase.SelectedItem is DatabaseSettings) &&
                (!string.IsNullOrEmpty(textBoxConnectionString.Text)));
        }

        private void RefreshSelectedDatabaseType()
        {
            DatabaseSettings databaseSettings = comboBoxDatabase.SelectedItem as DatabaseSettings;
            if (databaseSettings != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    databaseSettings.RefreshInstances();
                }
                catch (Exception ex)
                {
                    if (!_suppressInstanceListErrors)
                    {
                        Mouse.OverrideCursor = null;

                        StackHashMessageBox.Show(this,
                            Properties.Resources.InstanceListFailed_MBMessage,
                            Properties.Resources.InstanceListFailed_MBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error,
                            ex,
                            StackHashServiceErrorCode.NoError);
                    }
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }

            UpdateState(false);
        }

        private void UpdateConnectionString()
        {
            if (!_suppressConnectionStringChanges)
            {
                // get the instance name
                string instanceName = string.Empty;
                if (_selectedInstance != null)
                {
                    instanceName = _selectedInstance.InstanceName;
                }

                // get the connection string for the selected instance of the selected database type
                DatabaseSettings databaseSettings = comboBoxDatabase.SelectedItem as DatabaseSettings;
                if (databaseSettings != null)
                {
                    textBoxConnectionString.Text = databaseSettings.GetDatabaseConnectionString(instanceName, textBoxDatabaseName.Text);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // load in any existing settings
            DBConfigSettings.Settings.Load();

            if (!DBConfigSettings.Settings.IsNewProfile)
            {
                checkBoxStoreDatabaseInCabFolder.Visibility = Visibility.Collapsed;
            }

            checkBoxStoreDatabaseInCabFolder.IsChecked = DBConfigSettings.Settings.IsDatabaseInCabFolder;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _suppressInstanceListErrors = true;

            // configure the service proxy
            ServiceProxy.Services.OpenAll();
            ServiceProxy.Services.UpdateServiceEndpointAndAccount(DBConfigSettings.Settings.ServiceHost,
                DBConfigSettings.Settings.ServicePort,
                DBConfigSettings.Settings.ServiceUsername,
                DBConfigSettings.Settings.ServicePassword,
                DBConfigSettings.Settings.ServiceDomain);

            // for an existing profile don't update the connection string while we init
            _suppressConnectionStringChanges = !DBConfigSettings.Settings.IsNewProfile;

            // create list of supported databases
            _databaseList = new DatabaseList();
            this.DataContext = _databaseList;
            
            _initialCabFolder = DBConfigSettings.Settings.ProfileFolder;
            _initialProfileName = DBConfigSettings.Settings.ProfileName;
            _initialConnectionString = DBConfigSettings.Settings.ConnectionString;

            if (!string.IsNullOrEmpty(DBConfigSettings.Settings.ProfileName))
            {
                textBoxDatabaseName.Text = DBConfigSettings.Settings.ProfileName;
            }

            if (!string.IsNullOrEmpty(DBConfigSettings.Settings.ProfileFolder))
            {
                textBoxCabFolder.Text = DBConfigSettings.Settings.ProfileFolder;
            }

            if (!string.IsNullOrEmpty(DBConfigSettings.Settings.ConnectionString))
            {
                // best effort only to reselect the instance pointed to be the connection string
                TrySelectInstance(DBConfigSettings.Settings.ConnectionString);

                textBoxConnectionString.Text = DBConfigSettings.Settings.ConnectionString;
            }
            else
            {
                // if no connection string best effort to select a STACKHASH instance
                TrySelectInstance(null);
            }

            Mouse.OverrideCursor = null;
            UpdateState(false);

            // do this last - we want to preserve any connection string passed in from the user
            _suppressInstanceListErrors = false;
            _suppressConnectionStringChanges = false;
        }

        private void TrySelectInstance(string connectionString)
        {
            // attempt to select a database instance based on the connection string
            // NOTE this is SQL specific and will need to be revisited once additional
            // database types are supported

            try
            {
                DatabaseSettings databaseSettings = comboBoxDatabase.SelectedItem as DatabaseSettings;
                if (databaseSettings != null)
                {
                    string instanceToSelect = null;

                    if (connectionString == null)
                    {
                        instanceToSelect = "STACKHASH";
                    }
                    else
                    {
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                        string dataSource = (string)builder["Data Source"];


                        if (string.Compare("(local)", dataSource, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            instanceToSelect = "MSSQLSERVER";
                        }
                        else
                        {
                            instanceToSelect = dataSource.Replace("(local)\\", "");
                        }
                    }

                    if (instanceToSelect != null)
                    {
                        foreach (DatabaseInstance instance in databaseSettings.Instances)
                        {
                            if (string.Compare(instance.InstanceName, instanceToSelect, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                listViewInstances.SelectedItem = instance;
                                break;
                            }
                        }
                    }

                    // final shot, if no selected item select the first (if any)
                    if (listViewInstances.SelectedItem == null)
                    {
                        if (databaseSettings.Instances.Count > 0)
                        {
                            listViewInstances.SelectedItem = databaseSettings.Instances[0];
                        }
                    }
                }
            }
            catch { }
        }

        private void comboBoxDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedDatabaseType();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (_worker.IsBusy)
            {
                return;
            }

            string errors = ValidateAndReturnErrors();
            if (errors != null)
            {
                StackHashMessageBox.Show(this,
                    errors,
                    Properties.Resources.Error_Title,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Error);

                return;
            }

            DatabaseSettings databaseSettings = comboBoxDatabase.SelectedItem as DatabaseSettings;
            if (databaseSettings != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                bool createDatabase = true;

                // if we're editing an existing profile then we only create a database if a copy is required
                // copy is required if the data source has changed (otherwise it's a move)
                if (!DBConfigSettings.Settings.IsNewProfile)
                {
                    createDatabase = databaseSettings.HasDataSourceChanged(_initialConnectionString, textBoxConnectionString.Text);
                    DBConfigSettings.Settings.DatabaseCopyRequired = createDatabase;
                }

                _worker.RunWorkerAsync(new WorkerArg(false,
                    createDatabase,
                    checkBoxStoreDatabaseInCabFolder.IsChecked == false,
                    textBoxCabFolder.Text,
                    textBoxConnectionString.Text,
                    databaseSettings.GetMasterConnectionString(textBoxConnectionString.Text),
                    textBoxDatabaseName.Text));
            }

            e.Handled = true;
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Mouse.OverrideCursor = null;

            WorkerResult result = e.Result as WorkerResult;

            // check that the test succeded
            if (result.TestStatus != StackHashErrorIndexDatabaseStatus.Success)
            {
                // test failed - always a fatal error
                StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.TestFailed_MBMessage,
                    StackHashMessageBox.GetDatabaseStatusMessage(result.TestStatus)),
                    Properties.Resources.TestFailed_MBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Error,
                    result.WorkerException,
                    StackHashServiceErrorCode.NoError);
            }
            else if (!result.CanAccessCabFolder)
            {
                StackHashMessageBox.Show(this,
                    Properties.Resources.TestCabFolderFailed_MBMessage,
                    Properties.Resources.TestCabFolderFailed_MBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Error,
                    result.WorkerException,
                    StackHashServiceErrorCode.NoError);
            }
            else
            {
                if (result.TestConnectionOnly)
                {
                    // test succeeded
                    StackHashMessageBox.Show(this,
                        Properties.Resources.TestSuccess_MBMessage,
                        Properties.Resources.TestSuccess_MBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Information);
                }
                else
                {
                    if (result.WorkerException == null)
                    {
                        // config complete, we can close
                        Close();
                    }
                    else
                    {
                        // failed to create database
                        StackHashMessageBox.Show(this,
                            Properties.Resources.CreateFailed_MBMessage,
                            Properties.Resources.CreateFailed_MBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error,
                            result.WorkerException,
                            StackHash.StackHashService.StackHashServiceErrorCode.NoError);
                    }
                }
            }
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerResult result = new WorkerResult();
            InstallerInterface installer = null;

            try
            {
                WorkerArg arg = e.Argument as WorkerArg;
                result.TestConnectionOnly = arg.TestConnectionOnly;
                result.TestStatus = StackHashErrorIndexDatabaseStatus.Unknown;

                // configure for local access to the service
                if (!_localServiceConfigComplete)
                {
                    _localServiceGuid = ClientLogic.GetLocalServiceGuid();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                    
                    CheckVersionRequest checkVersionRequest = new CheckVersionRequest();
                    checkVersionRequest.ServiceGuid = _localServiceGuid == Guid.Empty ? null : _localServiceGuid.ToString();
                    checkVersionRequest.ClientData = GenerateClientData();
                    checkVersionRequest.MajorVersion = fvi.ProductMajorPart;
                    checkVersionRequest.MinorVersion = fvi.ProductMinorPart;
                    CheckVersionResponse checkVersionResponse = ServiceProxy.Services.Admin.CheckVersion(checkVersionRequest);

                    _localServiceConfigComplete = true;
                }

                // always test that the service can access the database

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration();
                sqlConfig.ConnectionString = arg.MasterConnectionString;
                sqlConfig.ConnectionTimeout = DefaultSqlConnectionTimeout;
                sqlConfig.EventsPerBlock = DefaultSqlEventsPerBlock;
                sqlConfig.InitialCatalog = arg.ProfileName;
                sqlConfig.MaxPoolSize = DefaultSqlMaxPoolSize;
                sqlConfig.MinPoolSize = DefaultSqlMinPoolSize;

                TestDatabaseConnectionRequest request = new TestDatabaseConnectionRequest();
                request.ClientData = GenerateClientData();
                request.ContextId = -1;
                request.SqlSettings = sqlConfig;
                request.TestDatabaseExistence = false;
                request.CabFolder = arg.CabFolder;

                TestDatabaseConnectionResponse response = ServiceProxy.Services.Admin.TestDatabaseConnection(request);
                result.TestStatus = response.TestResult;
                result.TestLastExceptionText = response.LastException;
                result.CanAccessCabFolder = response.IsCabFolderAccessible;
                result.CabFolderExceptionText = response.CabFolderAccessLastException;

                // contine if the test succeeded and we're not just testing
                if ((result.TestStatus == StackHashErrorIndexDatabaseStatus.Success) &&
                    (result.CanAccessCabFolder) &&
                    (!arg.TestConnectionOnly))
                {
                    // make sure NETWORK SERVICE can access the cab folder
                    FolderPermissionHelper.NSAddAccess(arg.CabFolder);

                    if (arg.CreateDatabase)
                    {
                        installer = new InstallerInterface(arg.MasterConnectionString,
                            arg.ProfileName,
                            arg.CabFolder);

                        installer.Connect();

                        if (!installer.DatabaseExists())
                        {
                            installer.CreateDatabase(arg.UseDefaultLocation);
                        }
                    }

                    DBConfigSettings.Settings.ProfileName = arg.ProfileName;
                    DBConfigSettings.Settings.ConnectionString = arg.ConnectionString;
                    DBConfigSettings.Settings.ProfileFolder = arg.CabFolder;
                    DBConfigSettings.Settings.Save();

                    _configurationSucceeded = true;
                }
                
            }
            catch (Exception ex)
            {
                result.WorkerException = ex;
            }
            finally
            {
                if (installer != null)
                {
                    installer.Disconnect();
                }
            }

            // if a test exception was reported include it in the  WorkerException
            if (!string.IsNullOrEmpty(result.TestLastExceptionText))
            {
                if (result.WorkerException == null)
                {
                    result.WorkerException = new AdminReportException(result.TestLastExceptionText);
                }
                else
                {
                    result.WorkerException = new AdminReportException(result.TestLastExceptionText, result.WorkerException);
                }
            }
            else if (!string.IsNullOrEmpty(result.CabFolderExceptionText))
            {
                if (result.WorkerException == null)
                {
                    result.WorkerException = new AdminReportException(result.CabFolderExceptionText);
                }
                else
                {
                    result.WorkerException = new AdminReportException(result.CabFolderExceptionText, result.WorkerException);
                }
            }

            e.Result = result;
        }

        private string ValidateAndReturnErrors()
        {
            string errors = null;

            bool anyErrors = false;
            StringBuilder errorsBuilder = new StringBuilder();

            // profile name must be valid
            if (!StackHashSqlControl.SqlUtils.IsValidSqlDatabaseName(textBoxDatabaseName.Text))
            {
                anyErrors = true;
                errorsBuilder.AppendLine();
                errorsBuilder.AppendFormat(Properties.Resources.Error_InvalidProfileName, ClientUtils.ProfileNameRulesText);
            }

            // profile name must not already be in use
            foreach (string existingName in DBConfigSettings.Settings.ExistingProfileNames)
            {
                if (string.Compare(_initialProfileName, textBoxDatabaseName.Text, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // OK for the profile name to be the same as itself
                    continue;
                }

                if (string.Compare(existingName, textBoxDatabaseName.Text, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // name is a duplicate
                    anyErrors = true;
                    errorsBuilder.AppendLine();
                    errorsBuilder.AppendLine(Properties.Resources.Error_DuplicateProfileName);
                    break;
                }
            }

            // cab folder must not be UNC if the database is stored in it
            if (checkBoxStoreDatabaseInCabFolder.IsChecked == true)
            {
                if (textBoxCabFolder.Text.IndexOf("\\\\", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    anyErrors = true;
                    errorsBuilder.AppendLine();
                    errorsBuilder.AppendLine(Properties.Resources.Error_CabFolderIsUNC);
                }
            }

            // cab folder must exist
            if (!System.IO.Directory.Exists(textBoxCabFolder.Text))
            {
                anyErrors = true;
                errorsBuilder.AppendLine();
                errorsBuilder.AppendLine(Properties.Resources.Error_CabFolderMissing);
            }

            // cab folder must not already be in use
            foreach (string existingFolder in DBConfigSettings.Settings.ExistingProfileFolders)
            {
                if (string.Compare(_initialCabFolder, textBoxCabFolder.Text, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (DBConfigSettings.Settings.IsUpgrade)
                    {
                        // must pick a new folder for upgrade
                        anyErrors = true;
                        errorsBuilder.AppendLine();
                        errorsBuilder.AppendLine(Properties.Resources.Error_UpgradeFolder);
                        break;
                    }
                    else
                    {
                        // OK for the profile name to be the same as itself otherwise
                        continue;
                    }
                }

                if (string.Compare(existingFolder, textBoxCabFolder.Text, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // name is a duplicate
                    anyErrors = true;
                    errorsBuilder.AppendLine();
                    errorsBuilder.AppendLine(Properties.Resources.Error_DuplicateCabFolder);
                    break;
                }
            }

            if (anyErrors)
            {
                errors = string.Format(CultureInfo.InvariantCulture,
                    Properties.Resources.Error_All,
                    errorsBuilder.ToString());
            }

            return errors;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_worker.IsBusy)
            {
                return;
            }

            if (StackHashMessageBox.Show(this,
                Properties.Resources.ConfirmCancel_MBMessage,
                Properties.Resources.ConfirmCancel_MBTitle,
                StackHashMessageBoxType.YesNo,
                StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
            {
                Close();
            }

            e.Handled = true;
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (_worker.IsBusy)
            {
                return;
            }

            RefreshSelectedDatabaseType();
        }

        private void hyperlinkProxySettings_Click(object sender, RoutedEventArgs e)
        {
            if (_worker.IsBusy)
            {
                return;
            }

            ProxySettingsWindow proxySettingsWindow = new ProxySettingsWindow();
            proxySettingsWindow.Owner = this;
            proxySettingsWindow.ShowDialog();
        }

        private void hyperlinkInstallExpress_Click(object sender, RoutedEventArgs e)
        {
            DefaultBrowser.OpenUrl("http://www.microsoft.com/sqlserver/en/us/editions/express.aspx");
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (_worker.IsBusy)
            {
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = false;
            ofd.CheckFileExists = false;
            ofd.CheckPathExists = true;
            if (System.IO.Directory.Exists(textBoxCabFolder.Text))
            {
                ofd.InitialDirectory = textBoxCabFolder.Text;
            }
            else
            {
                ofd.InitialDirectory = System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.FileName = Properties.Resources.SelectFolder;
            ofd.Filter = Properties.Resources.SelectFolderFilter;
            ofd.Title = Properties.Resources.SelectFolderTitle;

            if (ofd.ShowDialog(this) == true)
            {
                textBoxCabFolder.Text = System.IO.Path.GetDirectoryName(ofd.FileName);
            }

            UpdateState(false);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedInstance = listViewInstances.SelectedItem as DatabaseInstance;
            UpdateState(true);
        }

        private void textBoxCabFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateState(false);
        }

        private void textBoxDatabaseName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateState(true);
        }

        private void textBoxConnectionString_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateState(false);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // don't allow close while the worker is running
            if (_worker.IsBusy)
            {
                e.Cancel = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }

            Environment.ExitCode = _configurationSucceeded ? 0 : -1;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("database-configuration.htm");
        }

        private void buttonTestConnectionString_Click(object sender, RoutedEventArgs e)
        {
            if (_worker.IsBusy)
            {
                return;
            }

            DatabaseSettings databaseSettings = comboBoxDatabase.SelectedItem as DatabaseSettings;
            if (databaseSettings != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                _worker.RunWorkerAsync(new WorkerArg(true,
                    false,
                    checkBoxStoreDatabaseInCabFolder.IsChecked == false,
                    textBoxCabFolder.Text,
                    textBoxConnectionString.Text,
                    databaseSettings.GetMasterConnectionString(textBoxConnectionString.Text),
                    textBoxDatabaseName.Text));
            }
        }
    }
}
