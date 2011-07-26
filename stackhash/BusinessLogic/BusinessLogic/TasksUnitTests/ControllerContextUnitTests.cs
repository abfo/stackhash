using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashDebug;
using StackHashUtilities;

using System.Reflection;


namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for ControllerContextUnitTests
    /// </summary>
    [TestClass]
    public class ControllerContextUnitTests
    {
        String m_TempPath;
        String m_ScriptPath;
        List<AdminReportEventArgs> m_AllReports = new List<AdminReportEventArgs>();
        private TestContext testContextInstance;
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_TestServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";
        private AutoResetEvent m_ActivationAdminEvent = new AutoResetEvent(false);
        private AutoResetEvent m_DeactivationAdminEvent = new AutoResetEvent(false);


        public ControllerContextUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashTaskTesting";
            m_ScriptPath = m_TempPath + "\\Scripts";
            m_ActivationAdminEvent.Reset();
            m_DeactivationAdminEvent.Reset();
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            String[] folders = { dllLocation };
            BugTrackerManager manager = new BugTrackerManager(folders);
            TidyTest();
            m_AllReports.Clear();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
            if (!Directory.Exists(m_ScriptPath))
                Directory.CreateDirectory(m_ScriptPath);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath))
            {
                PathUtils.MarkDirectoryWritable(m_TempPath, true); 
                PathUtils.DeleteDirectory(m_TempPath, true);
            }
        }


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs args = e as AdminReportEventArgs;

            m_AllReports.Add(args);

            if (args.Report.Operation == StackHashAdminOperation.ContextStateChanged)
            {
                StackHashContextStateAdminReport contextChangedAdminReport = args.Report as StackHashContextStateAdminReport;

                if (contextChangedAdminReport.IsActivationAttempt)
                    m_ActivationAdminEvent.Set();
                else
                    m_DeactivationAdminEvent.Set();
            }
        }

        
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ConstructorInactive()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);
            context.Dispose();
        }

        [TestMethod]
        public void ConstructorInactivePlaintextPassword()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();
            settings.WinQualSettings.Password = "ThisPassword";
            settingsManager.SetContextSettings(settings, false);

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);

            String password = settings.WinQualSettings.Password;

            Assert.AreEqual(password, settingsManager.CurrentSettings.ContextCollection[0].WinQualSettings.Password);

            // Reload to make sure the settings file was updated too.
            settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            Assert.AreEqual(password, settingsManager.CurrentSettings.ContextCollection[0].WinQualSettings.Password);

            String [] lines = File.ReadAllLines(settingsFileName);

            foreach (String line in lines)
            {
                Assert.AreEqual(false, line.Contains(settings.WinQualSettings.Password));
            }
            context.Dispose();
        }

        
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void ActivateInvalidErrorIndexPath()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            settings.ErrorIndexSettings.Name = "Test";
            settings.ErrorIndexSettings.Folder = "rubbish %%$£";
            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);

            // Hook up to receive admin events.
            context.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);

            StackHashClientData clientData = new StackHashClientData(Guid.NewGuid(), "Mark", 24);


            context.Activate(clientData, false);
            Assert.AreEqual(true, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);

            // Wait for the activation event.
            m_ActivationAdminEvent.WaitOne(3000);

            Assert.AreEqual(1, m_AllReports.Count);
            Assert.AreEqual(clientData.ApplicationGuid, m_AllReports[0].Report.ClientData.ApplicationGuid);
            Assert.AreEqual(clientData.ClientId, m_AllReports[0].Report.ClientData.ClientId);
            Assert.AreEqual(clientData.ClientName, m_AllReports[0].Report.ClientData.ClientName);
            Assert.AreEqual(clientData.ClientRequestId, m_AllReports[0].Report.ClientData.ClientRequestId);

            Assert.AreEqual(0, m_AllReports[0].Report.ContextId);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Description);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Message);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashServiceErrorCode.ActivateFailed, m_AllReports[0].Report.ServiceErrorCode);
            Assert.AreNotEqual(null, m_AllReports[0].Report.LastException);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);

            StackHashContextStateAdminReport contextChanged = m_AllReports[0].Report as StackHashContextStateAdminReport;

            Assert.AreEqual(true, contextChanged.IsActivationAttempt);
            Assert.AreEqual(false, contextChanged.IsActive);

            context.Dispose();
        }

        [TestMethod]
        public void ActivateOk()
        {
            string settingsFileName = string.Format("{0}\\ServiceSettings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.ErrorIndexSettings.Name = "TestIndex";
            settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;


            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);
            
            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);

            // Hook up to receive admin events.
            context.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            StackHashClientData clientData = new StackHashClientData(Guid.NewGuid(), "Mark", 24);
            context.Activate(clientData, false);

            // Wait for the activation event.
            m_ActivationAdminEvent.WaitOne(3000);

            Assert.AreEqual(1, m_AllReports.Count);
            Assert.AreEqual(clientData.ApplicationGuid, m_AllReports[0].Report.ClientData.ApplicationGuid);
            Assert.AreEqual(clientData.ClientId, m_AllReports[0].Report.ClientData.ClientId);
            Assert.AreEqual(clientData.ClientName, m_AllReports[0].Report.ClientData.ClientName);
            Assert.AreEqual(clientData.ClientRequestId, m_AllReports[0].Report.ClientData.ClientRequestId);

            Assert.AreEqual(0, m_AllReports[0].Report.ContextId);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Description);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Message);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, m_AllReports[0].Report.ServiceErrorCode);
            Assert.AreEqual(null, m_AllReports[0].Report.LastException);
            Assert.AreEqual(null, m_AllReports[0].Report.LastException);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);

            StackHashContextStateAdminReport contextChanged = m_AllReports[0].Report as StackHashContextStateAdminReport;

            Assert.AreEqual(true, contextChanged.IsActivationAttempt);
            Assert.AreEqual(true, contextChanged.IsActive);

            Assert.AreEqual(true, context.IsActive);
            Assert.AreNotEqual(null, context.WinQualSyncTimerTask);
            context.Dispose();
        }

        [TestMethod]
        public void ActivateDeactivate()
        {
            string settingsFileName = string.Format("{0}\\ServiceSettings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.ErrorIndexSettings.Name = "TestIndex";
            settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;


            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);


            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);

            // Hook up to receive admin events.
            context.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);

            StackHashClientData clientData = new StackHashClientData(Guid.NewGuid(), "Mark", 24);

            context.Activate(clientData, false);
            Assert.AreEqual(true, context.IsActive);
            Assert.AreNotEqual(null, context.WinQualSyncTimerTask);

            // Wait for the activation event.
            m_ActivationAdminEvent.WaitOne(3000);

            Assert.AreEqual(1, m_AllReports.Count);
            Assert.AreEqual(clientData.ApplicationGuid, m_AllReports[0].Report.ClientData.ApplicationGuid);
            Assert.AreEqual(clientData.ClientId, m_AllReports[0].Report.ClientData.ClientId);
            Assert.AreEqual(clientData.ClientName, m_AllReports[0].Report.ClientData.ClientName);
            Assert.AreEqual(clientData.ClientRequestId, m_AllReports[0].Report.ClientData.ClientRequestId);

            Assert.AreEqual(0, m_AllReports[0].Report.ContextId);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Description);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Message);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, m_AllReports[0].Report.ServiceErrorCode);
            Assert.AreEqual(null, m_AllReports[0].Report.LastException);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);

            StackHashContextStateAdminReport contextChanged = m_AllReports[0].Report as StackHashContextStateAdminReport;

            Assert.AreEqual(true, contextChanged.IsActivationAttempt);
            Assert.AreEqual(true, contextChanged.IsActive);

            m_AllReports.Clear();

            context.Deactivate(clientData, true);
            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(null, context.WinQualSyncTimerTask);

            // Wait for the activation event.
            m_DeactivationAdminEvent.WaitOne(3000);

            Assert.AreEqual(1, m_AllReports.Count);
            Assert.AreEqual(clientData.ApplicationGuid, m_AllReports[0].Report.ClientData.ApplicationGuid);
            Assert.AreEqual(clientData.ClientId, m_AllReports[0].Report.ClientData.ClientId);
            Assert.AreEqual(clientData.ClientName, m_AllReports[0].Report.ClientData.ClientName);
            Assert.AreEqual(clientData.ClientRequestId, m_AllReports[0].Report.ClientData.ClientRequestId);

            Assert.AreEqual(0, m_AllReports[0].Report.ContextId);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Description);
            Assert.AreNotEqual(null, m_AllReports[0].Report.Message);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, m_AllReports[0].Report.ServiceErrorCode);
            Assert.AreEqual(null, m_AllReports[0].Report.LastException);
            Assert.AreEqual(StackHashAdminOperation.ContextStateChanged, m_AllReports[0].Report.Operation);

            contextChanged = m_AllReports[0].Report as StackHashContextStateAdminReport;

            Assert.AreEqual(false, contextChanged.IsActivationAttempt);
            Assert.AreEqual(false, contextChanged.IsActive);


            context.Dispose();
        }

        /// <summary>
        /// Invalid SQL database name.
        /// </summary>
        [TestMethod]
        public void InvalidSqlDatabaseName()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();

            settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            settings.ErrorIndexSettings.Name = "****";
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.SqlSettings.ConnectionString = "";

            settings.IsActive = true; // Force an attempt to activate.

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            
            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.InvalidDatabaseName, context.Status.CurrentError);
            Assert.AreNotEqual(null, context.Status.LastContextException);
            context.Dispose();
        }

        /// <summary>
        /// Unknown SQLServer instance. SLQEXPRESS.
        /// This is running in test mode so will try and create the database.
        /// </summary>
        [TestMethod]
        public void InvalidSqlInstanceTestMode()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();

            settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            settings.ErrorIndexSettings.Name = "TestIndex";
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\INVALIDEXPRESS;Integrated Security=True";

            settings.IsActive = true; // Force an attempt to activate.

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();


            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.FailedToCreateDatabase, context.Status.CurrentError);
            Assert.AreNotEqual(null, context.Status.LastContextException);
            context.Dispose();
        }

        /// <summary>
        /// Unknown SQLServer instance. SLQEXPRESS.
        /// This is not running in test mode so will not try and create the database.
        /// </summary>
        [TestMethod]
        public void InvalidSqlInstanceProductMode()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;
            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();

            settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            settings.ErrorIndexSettings.Name = "TestIndex";
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\INVALIDEXPRESS;Integrated Security=True";

            settings.IsActive = true; // Force an attempt to activate.

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();


            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, false, StackHashTestData.Default, licenseManager);

            Assert.AreEqual(false, context.IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.SqlConnectionError, context.Status.CurrentError);
            Assert.AreNotEqual(null, context.Status.LastContextException);
            context.Dispose();
        }

    }
}
