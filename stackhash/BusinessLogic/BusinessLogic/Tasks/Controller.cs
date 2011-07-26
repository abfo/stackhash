using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Data.Common;
using System.Reflection;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashCabs;
using StackHashDebug;
using StackHashBugTrackerInterfaceV1;


namespace StackHashTasks
{
    public class AdminReportEventArgs : EventArgs
    {
        private StackHashAdminReport m_Report;
        private bool m_SendToAll;

        public StackHashAdminReport Report
        {
            get { return m_Report; }
        }

        public bool SendToAll
        {
            get { return m_SendToAll; }
        }

        public AdminReportEventArgs(StackHashAdminReport report, bool sendToAll)
        {
            m_Report = report;
            m_SendToAll = sendToAll;
        }
    }
    
    

    /// <summary>
    /// Acts as a router between ControllerContext objects which do the real work for a context
    /// and clients performing tasks on a context.
    /// Contexts have an ID which is used to manager the routing.
    /// Contexts are created at startup from the persisted context settings.
    /// Global objects are also created here.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506")]
    public class Controller : IDisposable
    {
        #region Fields

        private Reporter m_Reporter;
        private Dictionary<int, ControllerContext> m_Contexts;
        private ScriptManager m_ScriptManager;
        private IDebugger m_Debugger;
        private SettingsManager m_SettingsManager;
        private LogManager m_LogManager;
        private bool m_IsTestMode;
        private StackHashTestData m_TestData;
        private LicenseManager m_LicenseManager;
        private static String s_LogOverrideFileName = "forceshlog.txt";
        private BugTrackerManager m_BugTrackerManager;
        private Timer m_ContextRetryTimer;

        static Controller s_TheController;

        /// <summary>
        /// Hook up to this to receive admin event reports.
        /// </summary>
        public event EventHandler<AdminReportEventArgs> AdminReports;

        #endregion // Fields


        #region Constructors

        /// <summary>
        /// Provided just for test purposes.
        /// </summary>
        public Controller()
        {
            m_Reporter = new Reporter(this);
            s_TheController = this;
        }

        /// <summary>
        /// Logs a message to the event log from a plugin.
        /// </summary>
        /// <param name="source">Caller - will be null.</param>
        /// <param name="e">The message to log.</param>
        public void LogPlugInEvent(Object source, BugTrackerTraceEventArgs e)
        {
            if (e == null)
                return;

            DiagSeverity severity = DiagSeverity.Information;

            switch (e.Severity)
            {
                case BugTrackerTraceSeverity.Information:
                    severity = DiagSeverity.Information;
                    break;
                case BugTrackerTraceSeverity.Warning:
                    severity = DiagSeverity.Warning;
                    break;
                case BugTrackerTraceSeverity.ComponentFatal:
                    severity = DiagSeverity.ComponentFatal;
                    break;
                case BugTrackerTraceSeverity.ApplicationFatal:
                    severity = DiagSeverity.ApplicationFatal;
                    break;
                default:
                    severity = DiagSeverity.Information;
                    break;
            }

            if (e.Message != null)
                DiagnosticsHelper.LogMessage(severity, e.Message);
        }


        private void createGuidFile(String folder, String guid)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");
            if (String.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid");
            if (!Directory.Exists(folder))
                throw new ArgumentException("Settings folder does not exist: " + folder, "folder");

            String guidFileName = folder + "\\ServiceInstanceData.txt";

            StreamWriter textWriter = null;

            try
            {
                textWriter = new StreamWriter(guidFileName);

                textWriter.WriteLine("ServiceGuid=" + guid);
                textWriter.WriteLine("ServiceMachineName=" + Environment.MachineName);
            }
            finally
            {
                if (textWriter != null)
                {
                    textWriter.Flush();
                    textWriter.Close();
                }
            }
        }

        /// <summary>
        /// Try to activate contexts again.
        /// </summary>
        /// <param name="state">Not currently used.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private void contextRetryTimerExpired(Object state)
        {
            // Check all contexts again.
            bool shouldStartRetryTimer = false;
            Monitor.Enter(m_Contexts);
            try
            {
                foreach (ControllerContext context in m_Contexts.Values)
                {
                    if (!context.IsActive && context.LastActivateFailed)
                    {
                        try
                        {
                            context.Activate(null, false);
                        }
                        catch (System.Exception ex)
                        {
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to start context on retry: " + context.Id.ToString(CultureInfo.InvariantCulture), ex);
                        }

                        if (!context.IsActive)
                            shouldStartRetryTimer = true;
                    }
                }
            }
            finally
            {
                Monitor.Exit(m_Contexts);
            }

            // Only retry for a set time.
            if (shouldStartRetryTimer && (Environment.TickCount / 1000) < AppSettings.ContextRetryPeriodInSeconds)
                startContextRetryTimer();
            else
                stopContextRetryTimer();
        }


        private void startContextRetryTimer()
        {
            m_ContextRetryTimer.Change(AppSettings.ContextRetryTimeoutInSeconds * 1000, Timeout.Infinite);
        }

        private void stopContextRetryTimer()
        {
            m_ContextRetryTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Constructs a controller for routing and the controller context objects to perform the real work.
        /// The controller object is the main entry point called by the WCF service implemenation.
        /// </summary>
        /// <param name="settingsFileName">Path and filename of the settings file.</param>
        /// <param name="isRunningInService">True - if running in service, false if running in windows app.</param>
        /// <param name="isTestMode">True - if running in test mode, false if running in production mode.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506")]
        public Controller(string settingsFileName, bool isRunningInService, bool isTestMode)
        {
            String tempPath = Path.GetTempPath();
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Temp Path: " + tempPath);

            m_ContextRetryTimer = new Timer(contextRetryTimerExpired);

            m_IsTestMode = isTestMode;
            TestManager.IsTestMode = m_IsTestMode; // Running unit tests.

            m_Reporter = new Reporter(this);

            String logOverrideFileName = Path.GetDirectoryName(settingsFileName) + "\\" + s_LogOverrideFileName;
            bool logOverrideFileExists = File.Exists(logOverrideFileName);

            m_LogManager = new LogManager(isRunningInService, m_IsTestMode);
            if (logOverrideFileExists)
                m_LogManager.StartLogging(false);

            if (settingsFileName == null)
                throw new ArgumentNullException("settingsFileName");

            // Scripts are stored in a subfolder of the settings folder.
            String scriptsFolder = String.Format(CultureInfo.InvariantCulture,
                "{0}\\DebuggerScripts\\", Path.GetDirectoryName(settingsFileName));
            String licenseFileName = String.Format(CultureInfo.InvariantCulture,
                "{0}\\license.bin", Path.GetDirectoryName(settingsFileName));

            // Global objects.
            m_ScriptManager = new ScriptManager(scriptsFolder);
            m_Debugger = new Windbg();
            m_SettingsManager = new SettingsManager(settingsFileName);

            if (isTestMode)
                m_SettingsManager.ServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";

            createGuidFile(Path.GetDirectoryName(settingsFileName), m_SettingsManager.ServiceGuid);

            m_LicenseManager = new LicenseManager(licenseFileName, m_SettingsManager.ServiceGuid);

            if (m_SettingsManager.CurrentSettings.EnableLogging)
                m_LogManager.StartLogging(false);


            // Bug tracking modules must be contained in a subfolder of this folder.
            String settingsPath = Path.GetDirectoryName(settingsFileName);
            String bugTrackerPlugInFolder = Path.Combine(settingsPath, "BugTrackerPlugIns");

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Loading bug tracker modules from: " + bugTrackerPlugInFolder);

            m_BugTrackerManager = new BugTrackerManager(new String[] { bugTrackerPlugInFolder });


            // Hook up the diagnostics tracer for plug-ins.
            BugTrackerTrace.LogMessageHook += new EventHandler<BugTrackerTraceEventArgs>(LogPlugInEvent);

            // One context is created per WinQual company logon.
            // For most companies this will probably be just the one login. 
            // However, for the likes of some umbrella companies where many companies are involved, then multiple
            // logins may be required.
            m_Contexts = new Dictionary<int, ControllerContext>();

            
            // Make sure the bugtracker plug-in settings are up to date. The plugin description and Url may 
            // change if the plugin changes so update if necessary.
            foreach (StackHashContextSettings contextSettings in m_SettingsManager.CurrentSettings.ContextCollection)
            {
                if (m_BugTrackerManager.UpdatePlugInSettings(contextSettings.BugTrackerSettings))
                    m_SettingsManager.SetContextSettings(contextSettings, false);
            }


            bool shouldStartRetryTimer = false;
            foreach (StackHashContextSettings contextSettings in m_SettingsManager.CurrentSettings.ContextCollection)
            {
                try
                {
                    if (createNewControllerContext(contextSettings))
                        shouldStartRetryTimer = true;
                }
                catch (System.Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to start context: " + contextSettings.Id.ToString(CultureInfo.InvariantCulture), ex);
                }
            }

            // Define the certificate policy for the application.
            ServicePointManager.ServerCertificateValidationCallback = 
                new RemoteCertificateValidationCallback(MyCertificateValidation.ValidateServerCertificate);

            if (shouldStartRetryTimer)
                startContextRetryTimer();

            s_TheController = this;
        }

        #endregion // Constructors

        /// <summary>
        /// Used to locate a context object using the context Id.
        /// </summary>
        /// <param name="contextId">Id of the context to find.</param>
        /// <returns>Context or null</returns>
        public ControllerContext FindContext(int contextId)
        {
            Monitor.Enter(m_Contexts);
            try
            {
                foreach (ControllerContext context in m_Contexts.Values)
                {
                    if (context.Id == contextId)
                        return context;
                }

                return null;
            }
            finally
            {
                Monitor.Exit(m_Contexts);
            }
        }

        #region PrivateMethods



        /// <summary>
        /// Determines if the specified context ID is valid.
        /// if contextMustBeActive and contextMustBeInactive are both false then the context
        /// can be inactive or active.
        /// </summary>
        /// <param name="contextId">The context ID to check.</param>
        /// <param name="contextMustBeActive">true - the context must be active.</param>
        /// <param name="contextMustBeInactive">true - the context must be inactive</param>
        private void validateContextId(int contextId, bool contextMustBeActive, bool contextMustBeInactive)
        {
            ControllerContext context = FindContext(contextId);
            if (context == null)
                throw new ArgumentException("Invalid ContextId Specified", "contextId");

            if (contextMustBeActive && !context.IsActive)
                throw new StackHashException("Context is not active", StackHashServiceErrorCode.ContextIsNotActive);

            if (contextMustBeInactive && context.IsActive)
                throw new StackHashException("Context must be inactive", StackHashServiceErrorCode.ContextIsActive);
        }


        /// <summary>
        /// Creates a new controller context using the specified settings.
        /// </summary>
        /// <param name="contextSettings">Settings used to create the new context</param>
        /// <returns>True - active required - but failed - should retry.</returns>
        private bool createNewControllerContext(StackHashContextSettings contextSettings)
        {
            Monitor.Enter(m_Contexts);
            ControllerContext newControllerContext = null;

            try
            {
                // Check if already exists.
                if (FindContext(contextSettings.Id) != null)
                    throw new ArgumentException("Context already exists", "contextSettings");

                newControllerContext =
                    new ControllerContext(contextSettings, m_ScriptManager, m_Debugger, m_SettingsManager, m_IsTestMode, m_TestData, m_LicenseManager);

                // Hook up to the admin reports.
                newControllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                m_Contexts.Add(newControllerContext.Id, newControllerContext);

                return newControllerContext.LastActivateFailed;
            }
            catch (System.Exception ex)
            {
                if (newControllerContext != null)
                    newControllerContext.Dispose();

                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Error creating context", ex);
                throw;
            }
            finally
            {
                Monitor.Exit(m_Contexts);
            }
        }

        #endregion // PrivateMethods


        #region PublicProperties

        public static Controller TheController
        {
            get { return s_TheController; }
        }

        public int ClientTimeoutInSeconds
        {
            get
            {
                if (m_SettingsManager == null)
                    return StackHashSettings.DefaultClientTimeoutInSeconds;
                else
                    return m_SettingsManager.ClientTimeoutInSeconds;
            }
        }

        /// <summary>
        /// Returns the current stackhash settings.
        /// This includes static persisted data as well as dynamic data.
        /// </summary>
        /// <returns>Current state of the settings.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public StackHashSettings StackHashSettings
        {
            get
            {
                // Get the persisted settings.
                StackHashSettings settings = m_SettingsManager.CurrentSettings;

                foreach (StackHashContextSettings context in settings.ContextCollection)
                {
                    ControllerContext controllerContext = FindContext(context.Id);

                    // The workflow mappings are retrieved from the database. Only try to get them if the 
                    // database has been created.
                    context.WorkFlowMappings = null;
                    if (context.IsIndexCreated)
                    {
                        try
                        {
                            context.WorkFlowMappings = controllerContext.GetMappings(StackHashMappingType.WorkFlow);
                        }
                        catch (System.Exception ex)
                        {
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to get workflow settings", ex);
                        }
                    }

                }                                    
                
                return m_SettingsManager.CurrentSettings;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.ContextCollection == null)
                    throw new ArgumentException("Null settings collection specified", "value");

                foreach (StackHashContextSettings settings in value.ContextCollection)
                {
                    ControllerContext context = FindContext(settings.Id);
                    if (context == null)
                        throw new ArgumentException("Settings id not found", "value");

                    context.UpdateSettings(settings);
                }
            }
        }

        /// <summary>
        /// Returns the current stackhash context statuses.
        /// </summary>
        /// <returns>Current state of the contexts.</returns>
        public StackHashStatus Status
        {
            get
            {
                // Get the persisted settings.
                StackHashStatus status = new StackHashStatus();
                status.ContextStatusCollection = new StackHashContextStatusCollection();

                status.HostRunningInTestMode = m_IsTestMode;
                status.InitializationFailed = false;

                Monitor.Enter(m_Contexts);

                try
                {
                    // Add the dynamic settings dynamic settings.
                    foreach (ControllerContext context in m_Contexts.Values)
                    {
                        StackHashContextStatus contextStatus = context.Status;

                        if (contextStatus != null)
                            status.ContextStatusCollection.Add(contextStatus);
                    }
                }
                finally
                {
                    Monitor.Exit(m_Contexts);
                }

                return status;
            }
        }

        #endregion // PublicProperties

        
        #region PublicMethods


        /// <summary>
        /// Check to see if the database exists or not.
        /// Causes an exception if no connection could be made to the database or
        /// the database is unknown.
        /// </summary>
        /// <param name="contextId">Context whose connection is to be tested.</param>
        /// <returns>True - connection ok and database exists.</returns>
        /// <returns>Database test result.</returns>
        public ErrorIndexConnectionTestResults TestDatabaseConnection(int contextId)
        {
            validateContextId(contextId, false, false);
            return m_Contexts[contextId].TestDatabaseConnection();
        }


        /// <summary>
        /// Check to see if the database exists or not.
        /// </summary>
        /// <param name="sqlSettings">The SQL connection settings to test.</param>
        /// <param name="testDatabaseExistence">True - test database existence as well as the cab folder.</param>
        /// <param name="cabFolder">The cab folder.</param>
        /// <param name="">The SQL connection settings to test.</param>
        /// <returns>True - connection ok and database exists.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public ErrorIndexConnectionTestResults TestDatabaseConnection(StackHashSqlConfiguration sqlSettings, 
            bool testDatabaseExistence, String cabFolder)
        {
            if (sqlSettings == null)
                throw new ArgumentNullException("sqlSettings");
            if (sqlSettings.ConnectionString == null)
                throw new ArgumentException("Connection string not defined", "sqlSettings");
            if (sqlSettings.InitialCatalog == null)
                throw new ArgumentException("InitialCatalog not defined", "sqlSettings");

            String initialCatalog = sqlSettings.InitialCatalog;

            sqlSettings.InitialCatalog = "MASTER";
            String masterConnectionString = sqlSettings.ToConnectionString();

            sqlSettings.InitialCatalog = initialCatalog;
            String connectionString = sqlSettings.ToConnectionString();

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");

            SqlCommands sqlCommands = new SqlCommands(providerFactory, connectionString, masterConnectionString, 1);
            ErrorIndexConnectionTestResults results = sqlCommands.GetDatabaseStatus(initialCatalog, testDatabaseExistence);

            // Try accessing the cab folder.
            results.IsCabFolderAccessible = false;
            if (!String.IsNullOrEmpty(cabFolder))
            {
                if (Directory.Exists(cabFolder))
                {
                    // Try to create a test file in the cabFolder.
                    String testFile = Path.Combine(cabFolder, "AccessTest.txt");

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
        /// The service creates a GUID when the settings.xml file is created for the first time in 
        /// c:\program data\stackhash on Vista
        /// c:\documents and settings\all users\application data\stackhash on XP
        /// </summary>
        public String ServiceGuid
        {
            get
            {
                if (m_SettingsManager != null)
                    return m_SettingsManager.ServiceGuid;
                else
                    return null;
            }
        }

        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("Microsoft.Security", "CA2109")]
        public void OnAdminReport(Object sender, EventArgs e)
        {
            EventHandler<AdminReportEventArgs> handler = AdminReports;
            if (handler != null)
                handler(this, e as AdminReportEventArgs);
        }


        /// <summary>
        /// Enables or disables synchronization for the specified product.
        /// </summary>
        /// <param name="contextId">Context to set.</param>
        /// <param name="productId">Product to enable/disable.</param>
        /// <param name="enabled">True - enable. False - disable.</param>
        public void SetProductWinQualState(int contextId, int productId, bool enabled)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].SetProductWinQualState(productId, enabled);
        }

        /// <summary>
        /// Set the maximum rows to request in each windowed search. Used for testing.
        /// Default is 5000.
        /// </summary>
        /// <param name="contextId">Context to set.</param>
        /// <param name="maxRowsToGetPerRequest">Rows to get (max).</param>
        public void SetMaxRowsToGetPerRequest(int contextId, int maxRowsToGetPerRequest)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].MaxRowsToGetPerRequest = maxRowsToGetPerRequest;
        }


        /// <summary>
        /// Sets the product sync state - e.g. whether it is enabled, disabled and how many
        /// cabs can be downloaded.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="productSyncState">New state.</param>
        public void SetProductSyncData(int contextId, StackHashProductSyncData productSyncState)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].SetProductSyncData(productSyncState);
        }


        /// <summary>
        /// Set the email notification settings for the specified context.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="emailSettings">New email settings</param>
        public void SetEmailSettings(int contextId, StackHashEmailSettings emailSettings)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].SetEmailSettings(emailSettings);
        }


        /// <summary>
        /// Sets the client timeout.
        /// </summary>
        /// <param name="clientTimeoutInSeconds">New timeout.</param>
        public void SetClientTimeoutInSeconds(int clientTimeoutInSeconds)
        {
            if (m_SettingsManager != null)
                m_SettingsManager.ClientTimeoutInSeconds = clientTimeoutInSeconds;
        }
        

        /// <summary>
        /// Enables logging for the service.
        /// </summary>
        virtual public void EnableLogging()
        {
            if (m_LogManager != null)
            {
                m_SettingsManager.EnableLogging = true;
                m_LogManager.StartLogging(true);
            }
        }


        /// <summary>
        /// Disables logging for the service.
        /// </summary>
        virtual public void DisableLogging()
        {
            if (m_LogManager != null)
            {
                m_SettingsManager.EnableLogging = false;
                m_LogManager.StopLogging();
            }
        }

        /// <summary>
        /// Enables reporting stats to the StackHash service.
        /// </summary>
        virtual public void EnableReporting()
        {
            m_SettingsManager.ReportingEnabled = true;
        }


        /// <summary>
        /// Disables reporting of stats to the StackHash service.
        /// </summary>
        virtual public void DisableReporting()
        {
            m_SettingsManager.ReportingEnabled = false;
        }
        
        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        virtual public void SetProxySettings(StackHashProxySettings proxySettings)
        {
            m_SettingsManager.ProxySettings = proxySettings;
        }

        /// <summary>
        /// Gets the current license data.
        /// </summary>
        virtual public StackHashLicenseData LicenseData
        {
            get
            {
                if (m_LicenseManager != null)
                    return m_LicenseManager.LicenseData;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the current license usage data.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        virtual public StackHashLicenseUsage LicenseUsage
        {
            get
            {
                StackHashLicenseUsage licenseData = new StackHashLicenseUsage();
                licenseData.ContextLicenseUsageCollection = new StackHashContextLicenseUsageCollection();
                licenseData.ClientLicenseUsageCollection = new StackHashClientLicenseUsageCollection();

                // Get all the active context data.
                foreach (ControllerContext controllerContext in m_Contexts.Values)
                {
                    if (controllerContext.IsActive)
                    {
                        try
                        {
                            StackHashContextLicenseUsage contextLicenseUsage = new StackHashContextLicenseUsage();
                            contextLicenseUsage.ContextId = controllerContext.Id;

                            contextLicenseUsage.NumberOfStoredEvents = controllerContext.GetTotalStoredEvents(true, true);
                            licenseData.ContextLicenseUsageCollection.Add(contextLicenseUsage);
                        }
                        catch
                        {
                        }
                    }
                }

                return licenseData;
            }
        }

        
        /// <summary>
        /// Sets the current license data.
        /// </summary>
        /// <param name="licenseId">The ID of the new license.</param>
        virtual public void SetLicense(String licenseId)
        {
            m_LicenseManager.SetLicense(licenseId);
        }


        /// <summary>
        /// Creates a new context with default settings and starts the appropriate timers.
        /// </summary>
        /// <returns>A copy of the default contexts settings.</returns>
        virtual public StackHashContextSettings CreateNewContext(ErrorIndexType errorIndexType)
        {
            StackHashContextSettings newContextSettings = m_SettingsManager.CreateNewContextSettings(errorIndexType);

            createNewControllerContext(newContextSettings);

            return newContextSettings;
        }

        /// <summary>
        /// Creates a new context with default settings and starts the appropriate timers.
        /// </summary>
        /// <returns>A copy of the default contexts settings.</returns>
        virtual public StackHashContextSettings CreateNewContext()
        {
            return CreateNewContext(ErrorIndexType.Xml);
        }


        /// <summary>
        /// Changes the specified context settings.
        /// </summary>
        /// <param name="contextSettings">New settings. The ID is used to find the correct settings.</param>
        virtual public void ChangeContextSettings(StackHashContextSettings contextSettings)
        {
            if (contextSettings == null)
                throw new ArgumentNullException("contextSettings");
            m_Contexts[contextSettings.Id].UpdateSettings(contextSettings);
        }


        /// <summary>
        /// Removes the specified context settings.
        /// </summary>
        /// <param name="contextId">Id of the context to remove.</param>
        /// <param name="resetNextContextIdIfAppropriate">true - resets the next context ID to 0 if no more contexts.</param>
        virtual public void RemoveContextSettings(int contextId, bool resetNextContextIdIfAppropriate)
        {
            // Check if already deleted.
            if (FindContext(contextId) == null)
                return;

            validateContextId(contextId, false, false);

            m_Contexts[contextId].AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            m_Contexts[contextId].RemoveContextSettings(resetNextContextIdIfAppropriate);
            m_Contexts[contextId].Dispose();
            m_Contexts.Remove(contextId);
        }


        /// <summary>
        /// Set the data collection policy. This will merge or replace existing policy records unless setAll is specified
        /// in which case the entire collection will be replaced.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="policyCollection">A collection of data collection policies.</param>
        /// <param name="setAll">true - Sets the entire structure, false - merges</param>
        public void SetDataCollectionPolicy(int contextId, StackHashCollectionPolicyCollection policyCollection, bool setAll)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].SetDataCollectionPolicy(policyCollection, setAll);
        }


        /// <summary>
        /// Gets the data collection policy for the specified object.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="policyObject">Global, Product, File, Event or Cab.</param>
        /// <param name="id">Id of the object to get.</param>
        /// <param name="conditionObject">The object to which the condition applies.</param>
        /// <param name="objectToCollect">The type of object being collected.</param>
        /// <param name="getAll">True - gets all policies, false - gets individual policy.</param>
        /// <returns>Individual policy or all policities as requested.</returns>
        public StackHashCollectionPolicyCollection GetDataCollectionPolicy(int contextId, StackHashCollectionObject policyObject, 
            int id, StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect, bool getAll)
        {
            validateContextId(contextId, false, false);
            return m_Contexts[contextId].GetDataCollectionPolicy(policyObject, id, conditionObject, objectToCollect, getAll);
        }


        /// <summary>
        /// Gets the active data collection policy for the specified object.
        /// </summary>
        /// <param name="contextId">The context settings to get.</param>
        /// <param name="productId">ID of product or 0.</param>
        /// <param name="fileId">ID of file or 0.</param>
        /// <param name="eventId">ID of event or 0.</param>
        /// <param name="cabId">ID of cab or 0.</param>
        /// <param name="objectToCollect">Object being collected.</param>
        /// <returns>Prioritized policy.</returns>
        public StackHashCollectionPolicyCollection GetActiveDataCollectionPolicy(int contextId, int productId, int fileId, int eventId, int cabId, StackHashCollectionObject objectToCollect)
        {
            validateContextId(contextId, false, false);
            return m_Contexts[contextId].GetActiveDataCollectionPolicy(productId, fileId, eventId, cabId, objectToCollect);
        }


        /// <summary>
        /// Removes the specified policy.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="rootObject">Global, Product, File, Event or Cab.</param>
        /// <param name="id">Id of the object to get.</param>
        /// <param name="conditionObject">Object to which the condition refers.</param>
        /// <param name="objectToCollect">Object being collected.</param>
        public void RemoveDataCollectionPolicy(int contextId, StackHashCollectionObject rootObject, int id, 
            StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].RemoveDataCollectionPolicy(rootObject, id, conditionObject, objectToCollect);
        }
        

        /// <summary>
        /// Sets the bugId field in the specified event.
        /// </summary>
        /// <param name="contextId">Id of the context to set.</param>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to set.</param>
        /// <param name="bugId">The bug ID to set.</param>
        virtual public void SetEventBugId(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, String bugId)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].SetEventBugId(product, file, theEvent, bugId);
        }


        /// <summary>
        /// Sets the WorkFlowStatus field in the specified event.
        /// </summary>
        /// <param name="contextId">Id of the context to set.</param>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to set.</param>
        /// <param name="workFlowStatus">The workFlowStatus to set.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        virtual public void SetWorkFlowStatus(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int workFlowStatus)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].SetWorkFlowStatus(product, file, theEvent, workFlowStatus);
        }


        /// <summary>
        /// Sets the purge options and schedule for the context.
        /// </summary>
        /// <param name="contextId">The id of the context to set.</param>
        /// <param name="purgeSchedule">When the automatic purge is to take place.</param>
        /// <param name="purgeOptions">What is to be purged</param>
        /// <param name="setAll">True - replace, false - individual.</param>
        public void SetPurgeOptions(int contextId, ScheduleCollection purgeSchedule, StackHashPurgeOptionsCollection purgeOptions, bool setAll)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].SetPurgeOptions(purgeSchedule, purgeOptions, setAll);
        }
 
       
        /// <summary>
        /// Gets the purge options for the specified context and object type.
        /// </summary>
        /// <param name="contextId">The id of the context.</param>
        /// <param name="purgeObject">Object to get the settings for.</param>
        /// <param name="id">Id of the object.</param>
        /// <param name="getAll">True - gets all purge options, false - gets individual purge option.</param>
        /// <returns>Individual or all purge options.</returns>
        public StackHashPurgeOptionsCollection GetPurgeOptions(int contextId, StackHashPurgeObject purgeObject, int id, bool getAll)
        {
            validateContextId(contextId, false, false);
            return m_Contexts[contextId].GetPurgeOptions(purgeObject, id, getAll);
        }


        /// <summary>
        /// Gets the active purge options for the specified object. 
        /// </summary>
        /// <param name="contextId">The id of the context.</param>
        /// <param name="productId">ID of product or 0.</param>
        /// <param name="fileId">ID of file or 0.</param>
        /// <param name="eventId">ID of event or 0.</param>
        /// <param name="cabId">ID of cab or 0.</param>
        /// <returns>Prioritized options.</returns>
        public StackHashPurgeOptionsCollection GetActivePurgeOptions(int contextId, int productId, int fileId, int eventId, int cabId)
        {
            validateContextId(contextId, false, false);
            return m_Contexts[contextId].GetActivePurgeOptions(productId, fileId, eventId, cabId);
        }

        
        /// <summary>
        /// Removes the purge options for the specified context and object type.
        /// </summary>
        /// <param name="contextId">The id of the context.</param>
        /// <param name="purgeObject">Object to get the settings for.</param>
        /// <param name="id">Id of the object.</param>
        public void RemovePurgeOptions(int contextId, StackHashPurgeObject purgeObject, int id)
        {
            validateContextId(contextId, false, false);

            m_Contexts[contextId].RemovePurgeOptions(purgeObject, id);
        }


        /// <summary>
        /// Activates the specified context settings.
        /// </summary>
        /// <param name="contextData">Identifies the client activating the context.</param>
        /// <param name="contextId">Id of the context to activate.</param>
        virtual public void ActivateContextSettings(StackHashClientData clientData, int contextId)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].Activate(clientData, false);
        }


        /// <summary>
        /// Deactivates the specified context settings.
        /// </summary>
        /// <param name="contextData">Identifies the client deactivating the context.</param>
        /// <param name="contextId">Id of the context to deactivate.</param>
        virtual public void DeactivateContextSettings(StackHashClientData clientData, int contextId)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].Deactivate(clientData, true);
        }
        

        /// <summary>
        /// Starts the synchronization task associated with the specified context.
        /// This method does not wait for completion of the synchronization.
        /// </summary>
        /// <param name="clientData">Client data.</param>
        /// <param name="contextId">Winqual login context.</param>
        /// <param name="forceFullSynchronize">true - forces a full resync, false - syncs from last successful sync time.</param>
        /// <param name="justSyncProducts">true - just sync the products, false - sync according to enabled products.</param>
        /// <param name="productsToSynchronize">List of products to sync - can be null.</param>
        virtual public void RunSynchronization(StackHashClientData clientData, int contextId, bool forceFullSynchronize,
            bool justSyncProducts, StackHashProductSyncDataCollection productsToSynchronize)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RunSynchronizeTask(clientData, forceFullSynchronize, false, justSyncProducts, 
                productsToSynchronize, false, false); // Don't wait for completion. User call.
        }


        /// <summary>
        /// Downloads the specified cab from the WinQual site.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="contextId">Winqual login context.</param>
        /// <param name="product">The product to which the can refers.</param>
        /// <param name="file">The file to which the can refers.</param>
        /// <param name="theEvent">The event to which the can refers.</param>
        /// <param name="cab">The cab to be downloaded.</param>
        /// <param name="waitForCompletion">true - thread waits, false - returns immediately.</param>
        virtual public void RunDownloadCabTask(StackHashClientData clientData, int contextId, StackHashProduct product, StackHashFile file,
            StackHashEvent theEvent, StackHashCab cab, bool waitForCompletion)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RunDownloadCabTask(clientData, product, file, theEvent, cab, false);
        }

        /// <summary>
        /// Uploads the specified data as a file to the web services.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="fileData">File data to send.</param>
        virtual public void RunUploadFileTask(StackHashClientData clientData, int contextId, String fileData)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RunUploadFileTask(clientData, fileData);
        }
            
        /// <summary>
        /// Runs the WinQual login task in order to check that the parameters are in fact correct.
        /// This is an asynchronous call. 
        /// A context must exist but it need not be active.
        /// </summary>
        /// <param name="clientData">Client data.</param>
        /// <param name="contextId">Winqual login context.</param>
        /// <param name="userName">WinQual username.</param>
        /// <param name="password">WinQual password.</param>
        virtual public void RunWinQualLogOn(StackHashClientData clientData, int contextId, String userName, String password)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].RunWinQualLogOnTask(clientData, userName, password);
        }

        
        /// <summary>
        /// Runs the specified scripts on the specified cab.
        /// </summary>
        /// <param name="contextId">Winqual login context.</param>
        /// <param name="product">The product owning the cab.</param>
        /// <param name="file">The file owning the cab.</param>
        /// <param name="theEvent">The event owning the cab.</param>
        /// <param name="cab">The cab to process.</param>
        /// <param name="dumpFileName">The dump filename - can be null.</param>
        /// <param name="scriptNames">List of scripts to run.</param>
        /// <param name="clientData">Data passed to the client callback.</param>
        /// <param name="forceRedo">True - full cab processing, false - only do new ones.</param>
        virtual public void RunDebugScriptTask(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent,
            StackHashCab cab, String dumpFileName, StackHashScriptNamesCollection scriptNames, StackHashClientData clientData, bool forceRedo)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RunDebugScriptTask(product, file, theEvent, cab, dumpFileName, scriptNames, clientData, forceRedo, false);
        }


        /// <summary>
        /// Starts the bug tracker task - or pings it to start work if it is already running.
        /// This might be necessary to "restart" the task processing after it has stopped due to an exception 
        /// calling the bug tracker plugins.
        /// </summary>
        virtual public void RunBugTrackerTask(int contextId)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RunBugTrackerTask();
        }

        /// <summary>
        /// Starts the bug reporting task. This task reports details of the specified products, files, events, cabs and scripts
        /// to the enabled bug tracker plugins.
        /// </summary>
        virtual public void RunBugReportTask(int contextId, StackHashClientData clientData, StackHashBugReportDataCollection bugReportDataCollection,
            StackHashBugTrackerPlugInSelectionCollection plugIns)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RunBugReportTask(clientData, bugReportDataCollection, plugIns);
        }


        /// <summary>
        /// Aborts the synchronization task associated with the specified context.
        /// This method does not wait for completion of the synchronization.
        /// </summary>
        /// <param name="contextId"></param>
        virtual public void AbortSynchronization(int contextId)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].TaskController.AbortSyncTask();
        }

        /// <summary>
        /// Aborts the specified task type.
        /// </summary>
        /// <param name="contextId"></param>
        virtual public void AbortTask(int contextId, StackHashClientData clientData, StackHashTaskType taskType)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].AbortTask(clientData, taskType);
        }


        /// <summary>
        /// Get a list of products associated with the specified context.
        /// </summary>
        /// <param name="contextId"></param>
        virtual public StackHashProductInfoCollection GetProducts(int contextId)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetProducts();
        }

        /// <summary>
        /// Get the last time the winqual site was updated.
        /// This is returned during a product sync.
        /// </summary>
        /// <returns></returns>
        virtual public DateTime GetLastSiteUpdateTime(int contextId)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetLastProductSyncTime(-2);
        }


        /// <summary>
        /// Get a list of files associated with the specified product.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="product">The product for which the file list is required.</param>
        virtual public StackHashFileCollection GetFiles(int contextId, StackHashProduct product)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetFiles(product);
        }


        /// <summary>
        /// Get a list of events associated with the specified product file.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="product">The product for which the file refers.</param>
        /// <param name="file">The file whos event list is required.</param>
        virtual public StackHashEventCollection GetEvents(int contextId, StackHashProduct product, StackHashFile file)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetEvents(product, file);
        }


        /// <summary>
        /// Get a list of event data associated with the specified event.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="product">The product for which the file refers.</param>
        /// <param name="file">The file whos event list is required.</param>
        /// <param name="theEvent">Event for which the data is required.</param>
        virtual public StackHashEventPackage GetEventPackage(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetEventPackage(product, file, theEvent);
        }


        /// <summary>
        /// Get a list of all events associated with a product.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="product">The product for which events are required.</param>
        virtual public StackHashEventPackageCollection GetProductEvents(int contextId, StackHashProduct product)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetProductEvents(product);
        }


        /// <summary>
        /// Gets the specified CAB file.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="product">The product for which the file refers.</param>
        /// <param name="file">The file whos event list is required.</param>
        /// <param name="theEvent">Event for which the data is required.</param>
        /// <param name="cab">Cab for which the file is required.</param>
        /// <param name="fileName">Name of the file or null if cab itself is to be returned.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        virtual public Stream GetCabFile(int contextId, 
                                         StackHashProduct product, 
                                         StackHashFile file, 
                                         StackHashEvent theEvent,
                                         StackHashCab cab,
                                         String fileName)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetCabFile(product, file, theEvent, cab, fileName);
        }


        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="searchCriteria">Specifies what to search.</param>
        virtual public StackHashEventPackageCollection GetEvents(
            int contextId, StackHashSearchCriteriaCollection searchCriteria)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetEvents(searchCriteria);
        }

        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="searchCriteria">Specifies what to search.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        virtual public StackHashEventPackageCollection GetEvents(
            int contextId, StackHashSearchCriteriaCollection searchCriteria, long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder)
        {
            return GetEvents(contextId, searchCriteria, startRow, numberOfRows, sortOrder, StackHashSearchDirection.Forwards, false);
        }

        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="searchCriteria">Specifies what to search.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <param name="direction">Search forwards or backwards from specified row.</param>
        /// <param name="countAllMatches">True - counts all matches as well as returning the first window.</param>
        virtual public StackHashEventPackageCollection GetEvents(
            int contextId, StackHashSearchCriteriaCollection searchCriteria, long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder,
            StackHashSearchDirection direction, bool countAllMatches)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetEvents(searchCriteria, startRow, numberOfRows, sortOrder, direction, countAllMatches);
        }

        /// <summary>
        /// Get a list of all events across all products matching the specified criteria.
        /// </summary>
        /// <param name="contextId">The WinQual login context.</param>
        /// <param name="searchCriteria">Specifies what to search.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        virtual public long CountEvents(
            int contextId, StackHashSearchCriteriaCollection searchCriteria, long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder, 
            StackHashSearchDirection direction)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].CountEvents(searchCriteria, startRow, numberOfRows, sortOrder, direction);
        }

        
        /// <summary>
        /// Gets a list of all scripts currently saved.
        /// </summary>
        virtual public StackHashScriptFileDataCollection ScriptNames
        {
            get
            {
                return m_ScriptManager.ScriptNames;
            }
        }


        /// <summary>
        /// Adds a new debugger script.
        /// </summary>
        /// <param name="script">Script to add.</param>
        /// <param name="overwrite">True - overwrites existing script. False - no overwrite.</param>
        virtual public void AddScript(StackHashScriptSettings script, bool overwrite)
        {
            m_ScriptManager.AddScript(script, overwrite);
        }


        /// <summary>
        /// Removes the specified script file.
        /// </summary>
        /// <param name="scriptName">Name of script to remove.</param>
        virtual public void RemoveScript(String scriptName)
        {
            m_ScriptManager.RemoveScript(scriptName);
        }


        /// <summary>
        /// Renames the specified script file.
        /// </summary>
        /// <param name="oldScriptName">Current name of the script.</param>
        /// <param name="newScriptName">New name of the script.</param>
        virtual public void RenameScript(String oldScriptName, String newScriptName)
        {
            m_ScriptManager.RenameScript(oldScriptName, newScriptName);
        }


        /// <summary>
        /// Gets the specified named script.
        /// </summary>
        /// <param name="scriptName">Name of script to retrieve.</param>
        /// <returns></returns>
        virtual public StackHashScriptSettings GetScript(string scriptName)
        {
            return m_ScriptManager.LoadScript(scriptName);
        }


        /// <summary>
        /// Runs a script on a particular cab file.
        /// </summary>
        /// <param name="contextId">Winqual context</param>
        /// <param name="product">Product data</param>
        /// <param name="file">File data</param>
        /// <param name="theEvent">Event data</param>
        /// <param name="cab">Cab data</param>
        /// <param name="dumpFileName">Name of the dump file or null</param>
        /// <param name="scriptName">Name of script to run on the dump file</param>
        /// <param name="clientData">Data describing the client</param>
        /// <returns>Full result of running the script</returns>
        virtual public StackHashScriptResult RunScript(int contextId, StackHashProduct product, StackHashFile file, 
            StackHashEvent theEvent, StackHashCab cab, String dumpFileName, String scriptName, StackHashClientData clientData)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].RunScript(product, file, theEvent, cab, dumpFileName, scriptName, clientData);
        }


        /// <summary>
        /// Gets the script result files for all scripts.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which results are required.</param>
        /// <returns>One or more script results files.</returns>
        virtual public StackHashScriptResultFiles GetResultFiles(int contextId, StackHashProduct product, 
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetResultFiles(product, file, theEvent, cab);
        }


        /// <summary>
        /// Gets script results file data a particular script and cab.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which results are required.</param>
        /// <param name="scriptName">Name of script for which the results are required.</param>
        /// <returns>Single script result.</returns>
        virtual public StackHashScriptResult GetResultFileData(int contextId, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String scriptName)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetResultFileData(product, file, theEvent, cab, scriptName);
        }


        /// <summary>
        /// Removes the script result data for the specified cab and script file.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="clientData">Identifies the calling client.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which results are to be deleted.</param>
        /// <param name="scriptName">Name of script file whos results are to be deleted.</param>
        virtual public void RemoveResultFileData(int contextId, StackHashClientData clientData, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String scriptName)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].RemoveResultFileData(clientData, product, file, theEvent, cab, scriptName);
        }

        
        /// <summary>
        /// Gets the notes for the specified cab.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which notes are required.</param>
        /// <returns>Notes for the cab.</returns>
        virtual public StackHashNotes GetCabNotes(int contextId, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetCabNotes(product, file, theEvent, cab);
        }


        /// <summary>
        /// Adds a note for the specified cab.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which note is to be added.</param>
        /// <param name="note">Note to add.</param>
        virtual public void AddCabNote(int contextId, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].AddCabNote(product, file, theEvent, cab, note);
        }


        /// <summary>
        /// Delete a note for the specified cab.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to which cab belongs.</param>
        /// <param name="cab">The cab for which note is to be deleted.</param>
        /// <param name="noteId">Note to delete.</param>
        virtual public void DeleteCabNote(int contextId, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].DeleteCabNote(product, file, theEvent, cab, noteId);
        }

        /// <summary>
        /// Gets notes for a particular event.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event for which notes required.</param>
        /// <returns>Notes for the event.</returns>
        virtual public StackHashNotes GetEventNotes(int contextId, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetEventNotes(product, file, theEvent);
        }


        /// <summary>
        /// Adds a note to the specified event.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to add note to.</param>
        /// <param name="note">Note to add.</param>
        virtual public void AddEventNote(int contextId, StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].AddEventNote(product, file, theEvent, note);
        }


        /// <summary>
        /// Deletes the specified event note.
        /// </summary>
        /// <param name="contextId">Context ID.</param>
        /// <param name="product">Product to which cab belongs.</param>
        /// <param name="file">File to which cab belongs.</param>
        /// <param name="theEvent">Event to delete note from.</param>
        /// <param name="noteId">Note to delete.</param>
        public void DeleteEventNote(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].DeleteEventNote(product, file, theEvent, noteId);
        }


        /// <summary>
        /// Delete the test index folder for the specified context.
        /// </summary>
        /// <param name="contextId">Context to create the files in.</param>
        public void DeleteIndex(int contextId)
        {
            validateContextId(contextId, false, true);
            m_Contexts[contextId].DeleteIndex();
        }


        /// <summary>
        /// This is just used for testing end to end service calls. Called to create a test index at the service.
        /// </summary>
        /// <param name="contextId">Context to create the files in.</param>
        /// <param name="testData">Test data used to create an index.</param>
        public void CreateTestIndex(int contextId, StackHashTestIndexData testData)
        {
            validateContextId(contextId, true, false);
            m_Contexts[contextId].CreateTestIndex(testData);
        }


        /// <summary>
        /// Sets test data used by the controller.
        /// </summary>
        /// <param name="testData">Test data used to create objects.</param>
        public void SetTestData(StackHashTestData testData)
        {
            m_TestData = testData;

            // Tell all the contexts.
            foreach (ControllerContext controllerContext in m_Contexts.Values)
            {
                controllerContext.SetTestData(testData);
            }
        }


        /// <summary>
        /// Get all stored events across all profiles.
        /// </summary>
        /// <param name="activeOnly">True - only check active profiles.</param>
        /// <param name="activeProductsOnly">True - only include active products. False - include all products.</param>
        public long GetStoredEventsAcrossAllProfiles(bool activeOnly, bool activeProductsOnly)
        {
            long totalEvents = 0;

            if (m_Contexts != null)
            {
                foreach (ControllerContext controllerContext in m_Contexts.Values)
                {
                    if (!activeOnly || controllerContext.IsActive)
                    {
                        totalEvents += controllerContext.GetTotalStoredEvents(activeOnly, activeProductsOnly);
                    }
                }
            }
            return totalEvents;
        }


        /// <summary>
        /// Moves the index to the specified location. Note that if the folder exists an error will occur.
        /// If only the name has changed then this amounts to a folder rename.
        /// Must be inactive to perform this action.
        /// </summary>
        /// <param name="contextId">ID of context to whose index is to be moved.</param>
        /// <param name="clientData">Client data used in the admin callback.</param>
        /// <param name="newErrorIndexPath">Root path for the folder.</param>
        /// <param name="newErrorIndexName">New name of the index.</param>
        /// <param name="newSqlSettings">Can be null if not an Sql index.</param>
        public void MoveIndex(int contextId, StackHashClientData clientData, String newErrorIndexPath,
            String newErrorIndexName, StackHashSqlConfiguration newSqlSettings)
        {
            validateContextId(contextId, false, true);
            m_Contexts[contextId].RunMoveIndexTask(clientData, newErrorIndexPath, newErrorIndexName, newSqlSettings);
        }

        /// <summary>
        /// Copies the index for the specified context to the specified location. 
        /// The original index is left in-tact.
        /// The index must be active.
        /// </summary>
        /// <param name="contextId">ID of context to whose index is to be moved.</param>
        /// <param name="clientData">Client data used in the admin callback.</param>
        /// <param name="destinationIndexSettings">Destination index settings.</param>
        /// <param name="sqlConfiguration">Sql settings - can be null.</param>
        /// <param name="assignCopyToContext">True - makes the copy the new index for the context.</param>
        /// <param name="destinationIndexSettings">True - delete the original index if successful.</param>
        public void CopyIndex(int contextId, StackHashClientData clientData, ErrorIndexSettings destinationIndexSettings, 
            StackHashSqlConfiguration sqlConfiguration, bool assignCopyToContext, bool deleteSourceIndexWhenComplete)
        {
            validateContextId(contextId, false, true);
            m_Contexts[contextId].RunCopyIndexTask(clientData, destinationIndexSettings, sqlConfiguration, assignCopyToContext, deleteSourceIndexWhenComplete);
        }


        /// <summary>
        /// Gets a rollup of all of the product statistics.
        /// This includes the locale, OS and hit date rollups.
        /// </summary>
        /// <param name="contextId">ID of context for which stats is required.</param>
        /// <param name="productId">Product for which the stats is required.</param>
        /// <returns>Rollup stats.</returns>
        public StackHashProductSummary GetProductSummary(int contextId, int productId)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetProductSummary(productId);
        }

        /// <summary>
        /// Gets default properties and diagnostics for all loaded BugTracker plugins.
        /// If context id = -1 then the DLL diagnostics are returned otherwise the context diagnostics are returned.
        /// If plugInName is specified then only that plugin diagnostics are returned otherwise all plugin diagnostics 
        /// are returned.
        /// </summary>
        /// <param name="contextId">Id of context or -1 for no context.</param>
        /// <param name="plugInName">Name of the plug-in or null for all plug-ins</param>
        /// <returns>Full plug-in properties and diagnostics.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashBugTrackerPlugInDiagnosticsCollection GetBugTrackerPlugInDiagnostics(int contextId, String plugInName)
        {
            if (contextId == -1)
            {
                return m_BugTrackerManager.GetBugTrackerPlugInDiagnostics(plugInName);
            }
            else
            {
                validateContextId(contextId, true, false);
                return m_Contexts[contextId].GetBugTrackerPlugInDiagnostics(plugInName);
            }
        }


        /// <summary>
        /// Sets the plugin settings for a context.
        /// </summary>
        /// <param name="contextId">Context whose settings are to be set.</param>
        /// <param name="settings">New plugin settings.</param>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public void SetContextBugTrackerPlugInSettings(int contextId, StackHashBugTrackerPlugInSettings settings)
        {
            validateContextId(contextId, false, true);

            m_BugTrackerManager.UpdatePlugInSettings(settings);
            m_SettingsManager.SetContextBugTrackerPlugInSettings(contextId, settings);
        }

        /// <summary>
        /// Gets the plugin settings for a context.
        /// </summary>
        /// <param name="contextId">Context whose settings are to be retrieved.</param>
        /// <returns>Bug tracker settings for the specified context.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashBugTrackerPlugInSettings GetContextBugTrackerPlugInSettings(int contextId)
        {
            validateContextId(contextId, false, false);
            StackHashBugTrackerPlugInSettings plugInSettings = m_SettingsManager.GetContextBugTrackerPlugInSettings(contextId);
            m_BugTrackerManager.UpdatePlugInSettings(plugInSettings);
            return plugInSettings;
        }

        /// <summary>
        /// Gets mappings of a particular type.
        /// </summary>
        /// <param name="contextId">Context whose mappings are to be retrieved.</param>
        /// <param name="mappingType">Type of mapping required.</param>
        /// <returns>Mappings.</returns>
        public StackHashMappingCollection GetMappings(int contextId, StackHashMappingType mappingType)
        {
            validateContextId(contextId, false, false);
            return m_Contexts[contextId].GetMappings(mappingType);
        }

        /// <summary>
        /// Updates the specified mapping entries.
        /// </summary>
        /// <param name="contextId">Context whose mappings are to be retrieved.</param>
        /// <param name="mappingType">Type of mapping to set.</param>
        /// <param name="mappings">The mappings to update.</param>
        public void UpdateMappings(int contextId, StackHashMappingType mappingType, StackHashMappingCollection mappings)
        {
            validateContextId(contextId, false, false);
            m_Contexts[contextId].UpdateMappings(mappingType, mappings);
        }

        /// <summary>
        /// Get the cab package for a particular cab.
        /// This contains a little more information that would be too much for each individual cab.
        /// </summary>
        /// <param name="contextId">Context owning the index.</param>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">Cab for which more data is required.</param>
        /// <returns>Cab package</returns>
        public StackHashCabPackage GetCabPackage(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            validateContextId(contextId, true, false);
            return m_Contexts[contextId].GetCabPackage(product, file, theEvent, cab);
        }
        
        
        #endregion PublicMethods


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (ControllerContext controllerContext in m_Contexts.Values)
                {
                    // Unhook any event handlers.
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                    controllerContext.Dispose();
                }

                if (m_LogManager != null)
                {
                    m_LogManager.Dispose();
                    m_LogManager = null;
                }

                if (m_LicenseManager != null)
                    m_LicenseManager.Dispose();

                if (m_ContextRetryTimer != null)
                    m_ContextRetryTimer.Dispose();

                BugTrackerTrace.LogMessageHook -= new EventHandler<BugTrackerTraceEventArgs>(LogPlugInEvent);

                //if (m_BugTrackerManager != null)
                //    m_BugTrackerManager.Dispose();
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
