using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ServiceUnitTests.StackHashServices;
using System.ServiceModel;
using System.Data.SqlClient;
using System.Reflection;

using StackHashUtilities;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for DiagnosticUnitTest
    /// </summary>
    [TestClass]
    public class DiagnosticUnitTest
    {
        private Utils m_Utils;
        private TestContext testContextInstance;
        private String m_LogFolder;

        public DiagnosticUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

            m_Utils.DisableLogging();

            // Delete any log files that may be lying around.
            m_LogFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "StackHash\\Test\\logs");
            Utils.DeleteLogFiles(m_LogFolder);

            m_Utils.RemoveAllContexts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                try
                {
                    m_Utils.DeactivateContext(0);
                    m_Utils.DeleteIndex(0);
                }
                catch 
                {
                }
                m_Utils.Dispose();
                m_Utils = null;
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
        public void CheckDiagnosticsAreGenerated()
        {
            double size1 = 0;
            double size2 = 0;
            double size3 = 0;

            // Start with logging disabled.
            m_Utils.DisableLogging();

            String diagnosticsFileName = Path.Combine(m_LogFolder, "StackHashServiceDiagnosticsLog_00000001.txt");

            m_Utils.EnableLogging();

            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();
            Assert.AreEqual(true, settings.Settings.EnableLogging);

            FileInfo info = new FileInfo(diagnosticsFileName);
            size2 = info.Length;

            m_Utils.DisableLogging();

            settings = m_Utils.GetContextSettings();
            Assert.AreEqual(false, settings.Settings.EnableLogging);

            info = new FileInfo(diagnosticsFileName);
            size3 = info.Length;

            Assert.AreEqual(true, size1 < size2);
            Assert.AreEqual(true, size2 < size3);
        }

        [TestMethod]
        public void CheckDiagnosticsAreGeneratedWithRestart()
        {
            double size1 = 0;
            double size2 = 0;
            double size3 = 0;

            // Start with logging disabled.
            m_Utils.DisableLogging();

            String diagnosticsFileName = Path.Combine(m_LogFolder, "StackHashServiceDiagnosticsLog_00000001.txt");

            m_Utils.EnableLogging();

            m_Utils.RestartService();

            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();
            Assert.AreEqual(true, settings.Settings.EnableLogging);

            FileInfo info = new FileInfo(diagnosticsFileName);
            size2 = info.Length;

            m_Utils.DisableLogging();

            m_Utils.RestartService();

            settings = m_Utils.GetContextSettings();
            Assert.AreEqual(false, settings.Settings.EnableLogging);

            info = new FileInfo(diagnosticsFileName);
            size3 = info.Length;

            Assert.AreEqual(true, size1 < size2);
            Assert.AreEqual(true, size2 < size3);
        }
        
        [TestMethod]
        public void GetServiceStatusNoActiveContexts()
        {
            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(0, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);
        }

        /// <summary>
        /// Determines if the client is permitted to abort a task of the specified type.
        /// </summary>
        /// <param name="taskType">Task type to check.</param>
        /// <returns>True - client can abort the task. False - client cannot abort the task.</returns>
        public bool CanTaskBeAbortedByClient(StackHashTaskType taskType)
        {
            if ((taskType == StackHashTaskType.BugReportTask) ||
                (taskType == StackHashTaskType.DebugScriptTask) ||
                (taskType == StackHashTaskType.DownloadCabTask) ||
                (taskType == StackHashTaskType.ErrorIndexCopyTask) ||
                (taskType == StackHashTaskType.ErrorIndexMoveTask) ||
                (taskType == StackHashTaskType.PurgeTask) ||
                (taskType == StackHashTaskType.WinQualSynchronizeTask))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [TestMethod]
        public void GetServiceStatusOneInactiveContext()
        {
            m_Utils.CreateNewContext();
            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(false, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(true, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            Array allEnumValues = (typeof(StackHashTaskType)).GetEnumValues();

            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
                Assert.AreEqual(CanTaskBeAbortedByClient(taskType), taskStatus.CanBeAbortedByClient);
            }
        }

        [TestMethod]
        public void GetServiceStatusOneInactiveContextFailedToActivate()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];

            // Set some invalid stuff.
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\SLQEXPRESS;Integrated Security=True;";
            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);

            try
            {
                try
                {
                    // Activate the context.
                    m_Utils.ActivateContext(0);
                }
                catch (FaultException<ReceiverFaultDetail> ex)
                {
                    Assert.AreEqual(StackHashServiceErrorCode.FailedToCreateDatabase, ex.Detail.ServiceErrorCode);
                }

                GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

                Assert.AreEqual(true, status.Status.HostRunningInTestMode);
                Assert.AreEqual(false, status.Status.InitializationFailed);

                Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
                Assert.AreEqual(false, status.Status.ContextStatusCollection[0].IsActive);
                Assert.AreEqual(StackHashServiceErrorCode.FailedToCreateDatabase, status.Status.ContextStatusCollection[0].CurrentError);
                Assert.AreEqual(false, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


                // There should be an entry for all event types.
                Array allEnumValues = (typeof(StackHashTaskType)).GetEnumValues();

                foreach (StackHashTaskType taskType in allEnumValues)
                {
                    StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                    Assert.AreNotEqual(null, taskStatus);
                    Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
                }
            }
            finally
            {
                settings.SqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
                m_Utils.SetContextSettings(settings);
            }
        }

        [TestMethod]
        public void GetServiceStatusOneInactiveContextFailedToActivateThenFixAndActivate()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];

            // Set some invalid stuff.
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\SLQEXPRESS;Integrated Security=True;";
            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);

            try
            {
                // Activate the context.
                m_Utils.ActivateContext(0);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.FailedToCreateDatabase, ex.Detail.ServiceErrorCode);
            }

            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(false, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.FailedToCreateDatabase, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(false, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            Array allEnumValues = (typeof(StackHashTaskType)).GetEnumValues();

            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
            }

            // FIX 
            settings.SqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            m_Utils.SetContextSettings(settings);

            // Activate the context.
            m_Utils.ActivateContext(0);

            status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(true, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(true, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                if (taskType == StackHashTaskType.WinQualSynchronizeTimerTask)
                    Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);
                else if (taskType == StackHashTaskType.PurgeTimerTask)
                    Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);
                else if (taskType == StackHashTaskType.BugTrackerTask)
                    Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);
                else
                    Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
            }
        }


        /// <summary>
        /// Create a context with an XML database.
        /// Then activate it.
        /// Then corrupt the XML file so it fails to load when the service is restarted.
        /// Service should fail to activate but profile should still exist.
        /// </summary>
        [TestMethod]
        public void GetServiceStatusOneInactiveContextActivateOkThenCorruptThenRestartService()
        {
            m_Utils.CreateNewContext(ErrorIndexType.Xml);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];


            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);
            m_Utils.DeleteIndex(0);

            // Corrupt the index settings so the next service load should fail.
            String settingsPath = Path.Combine(settings.ErrorIndexSettings.Folder, settings.ErrorIndexSettings.Name);
            String settingsFileName = settingsPath + "\\settings.xml";

            m_Utils.ActivateContext(0);

            m_Utils.CorruptFile(settingsFileName);

            m_Utils.RestartService();

            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(false, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.ContextLoadError, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(false, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            Array allEnumValues = (typeof(StackHashTaskType)).GetEnumValues();

            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
            }

            resp = m_Utils.GetContextSettings();
            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
        }

        /// <summary>
        /// Create a context with an XML database.
        /// Then activate it.
        /// Then corrupt the XML file so it fails to load when the service is restarted.
        /// Then fix the error.
        /// Restart service.
        /// Should be status ok.
        /// The reactivate - should be ok.
        /// </summary>
        [TestMethod]
        public void GetServiceStatusOneInactiveContextActivateOkThenCorruptThenRestartServiceFixRestartActivate()
        {
            m_Utils.CreateNewContext(ErrorIndexType.Xml);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];


            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);
            m_Utils.DeleteIndex(0);

            // Corrupt the index settings so the next service load should fail.
            String settingsPath = Path.Combine(settings.ErrorIndexSettings.Folder, settings.ErrorIndexSettings.Name);
            String settingsFileName = settingsPath + "\\settings.xml";

            m_Utils.ActivateContext(0);

            m_Utils.CorruptFile(settingsFileName);

            m_Utils.RestartService();

            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(false, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.ContextLoadError, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(false, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            Array allEnumValues = (typeof(StackHashTaskType)).GetEnumValues();

            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
            }


            // Fix the problem by deleting the settings file - it should be recreated.
            File.Delete(settingsFileName);

            m_Utils.RestartService();

            // Should be inactive now.
            status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(false, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(true, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
            }

            
            // Finally activate.
            m_Utils.ActivateContext(0);

            // Should be inactive now.
            status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(true, status.Status.ContextStatusCollection[0].IsActive);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, status.Status.ContextStatusCollection[0].CurrentError);
            Assert.AreEqual(true, String.IsNullOrEmpty(status.Status.ContextStatusCollection[0].LastContextException));


            // There should be an entry for all event types.
            foreach (StackHashTaskType taskType in allEnumValues)
            {
                StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);
                if (taskType == StackHashTaskType.WinQualSynchronizeTimerTask)
                    Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);
                else if (taskType == StackHashTaskType.PurgeTimerTask)
                    Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);
                else if (taskType == StackHashTaskType.BugTrackerTask)
                    Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);
                else
                    Assert.AreEqual(StackHashTaskState.NotRunning, taskStatus.TaskState);
            }

        }

        
        [TestMethod]
        public void GetServiceStatusOneActiveContext()
        {
            m_Utils.CreateAndSetNewContext();
            m_Utils.ActivateContext(0);

            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, status.Status.HostRunningInTestMode);
            Assert.AreEqual(false, status.Status.InitializationFailed);

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(true, status.Status.ContextStatusCollection[0].IsActive);

            // There should be an entry for all event types.
            Array allEnumValues = (typeof(StackHashTaskType)).GetEnumValues();

            StackHashTaskStatus taskStatus;
            foreach (StackHashTaskType taskType in allEnumValues)
            {
                taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, taskType);
                Assert.AreNotEqual(null, taskStatus);

                Assert.AreEqual((taskType == StackHashTaskType.WinQualSynchronizeTimerTask) || 
                                (taskType == StackHashTaskType.PurgeTimerTask) ||
                                (taskType == StackHashTaskType.BugTrackerTask), taskStatus.TaskState == StackHashTaskState.Running);
            }
            
            m_Utils.DeactivateContext(0);
        }

        [TestMethod]
        public void EnableReporting()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);
        }

        [TestMethod]
        public void DisableReporting()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);

            // Start with logging disabled.
            m_Utils.DisableReporting();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(false, resp.Settings.ReportingEnabled);
        }


        [TestMethod]
        public void EnableDisableEnableReporting()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);

            // Start with logging disabled.
            m_Utils.DisableReporting();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(false, resp.Settings.ReportingEnabled);

            // Start with logging disabled.
            m_Utils.EnableReporting();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);
        
        }

        [TestMethod]
        public void EnableReportingWithReset()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);
        }

        [TestMethod]
        public void DisableReportingWithReset()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();
            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);

            // Start with logging disabled.
            m_Utils.DisableReporting();
            m_Utils.RestartService();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(false, resp.Settings.ReportingEnabled);
        }


        [TestMethod]
        public void EnableDisableEnableReportingWithReset()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();
            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);

            // Start with logging disabled.
            m_Utils.DisableReporting();
            m_Utils.RestartService();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(false, resp.Settings.ReportingEnabled);

            // Start with logging disabled.
            m_Utils.EnableReporting();

            resp = m_Utils.GetContextSettings();
            m_Utils.RestartService();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);

            Assert.AreEqual(true, resp.Settings.ReportingEnabled);
        }


        [TestMethod]
        public void TestDatabaseConnectionInvalidDatabaseName()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];

            // Set an invalid connection string.
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\SLQEXPRESS;Integrated Security=True;";
            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(0);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.FailedToConnectToMaster, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, testResp.LastException.Contains("The server was not found or was not accessible"));
        }

//        [TestMethod]  // Only enable this if you disable the NETWORK SERVICE rights on the SQL server.
        public void TestDatabaseConnectionServiceDoesNotHavePermission()
        {
            String databaseFolder = "c:\\PermissionsTest";
            String databaseName = "PermissionsTest";

            // Create the database from here.
            StackHashSqlControl.InstallerInterface installerInterface =
                new StackHashSqlControl.InstallerInterface(TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;",
                    "PermissionsTest", databaseFolder);

            installerInterface.Connect();
            installerInterface.CreateDatabase(true); // Use default location for the index.
            installerInterface.Disconnect();
            SqlConnection.ClearAllPools();

            try
            {
                m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
                StackHashContextSettings settings = resp.Settings.ContextCollection[0];

                // Set an invalid connection string.
                settings.ErrorIndexSettings.Folder = databaseFolder;
                settings.ErrorIndexSettings.Name = databaseName;
                m_Utils.SetContextSettings(settings);

                TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(0);

                Assert.AreEqual(StackHashErrorIndexDatabaseStatus.DatabaseExistsButFailedToConnect, testResp.TestResult);

                if (testResp.LastException != null)
                    Console.WriteLine(testResp.LastException);

                Assert.AreEqual(false, String.IsNullOrEmpty(testResp.LastException));
            }
            finally
            {
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void TestDatabaseConnectionCabFolderAllOk()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];

            // Set a valid connection string.
            // Keep the connection string at the default.
            //settings.SqlSettings.ConnectionString = "Data Source=(local)\\SQLEXPRESS;Integrated Security=True;";
            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);

            m_Utils.ActivateContext(0); // Create the database.

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(0);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);
            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
            
            Assert.AreEqual(true, testResp.IsCabFolderAccessible);
            if (testResp.CabFolderAccessLastException != null)
                Console.WriteLine(testResp.CabFolderAccessLastException);
            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.CabFolderAccessLastException));
        }

        [TestMethod]
        public void TestDatabaseConnectionInvalidCabFolder()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];

            // Change folder to something invalid.
            settings.ErrorIndexSettings.Folder = "c:\\folderdoesntexist";

            // Set an invalid connection string.
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\SQLEXPRESS;Integrated Security=True;";
            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);


            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(0);

            Assert.AreEqual(false, testResp.IsCabFolderAccessible);
            if (testResp.CabFolderAccessLastException != null)
                Console.WriteLine(testResp.CabFolderAccessLastException);
            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.CabFolderAccessLastException));
        }

        [TestMethod]
//        [Ignore] // Can't do this because the test runs with admin so can do anything.
        public void TestDatabaseConnectionCabFolderInaccessible()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            StackHashContextSettings settings = resp.Settings.ContextCollection[0];

            // Change folder to something invalid.
            settings.ErrorIndexSettings.Folder = "c:\\windows";

            // Set an invalid connection string.
            settings.SqlSettings.ConnectionString = "Data Source=(local)\\SQLEXPRESS;Integrated Security=True;";
            settings.ErrorIndexSettings.Name = "TestForContextErrors";
            m_Utils.SetContextSettings(settings);


            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(0);

            Assert.AreEqual(false, testResp.IsCabFolderAccessible);
            if (testResp.CabFolderAccessLastException != null)
                Console.WriteLine(testResp.CabFolderAccessLastException);
            Assert.AreEqual(false, String.IsNullOrEmpty(testResp.CabFolderAccessLastException));
            Assert.AreEqual(true, testResp.CabFolderAccessLastException.Contains("denied"));
        }


        /// <summary>
        /// Tests a connection string outside of a profile. Invalid database name.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_InvalidDatabaseName()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "Bla Bla Bla"; // Invalid.

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.InvalidDatabaseName, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
        }

        /// <summary>
        /// Tests a connection string outside of a profile. 
        /// Try to connect to an unknown instance.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_InstanceDoesNotExist()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = "Data Source=(local)\\SQLEXPRESS2;Integrated Security=True"; // Instance invalid.
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.FailedToConnectToMaster, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            // Should come back with "a network or instance related error...".
            Assert.AreEqual(false, String.IsNullOrEmpty(testResp.LastException));
        }

        /// <summary>
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_InstanceExists_NoDatabaseCheck()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
        }

        
        /// <summary>
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_InstanceOkDoNotCheckDatabase()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
        }

        /// <summary>
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance. Database doesn't exist.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_InstanceOkDatabaseDoesNotExist()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, true);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.ConnectedToMasterButDatabaseDoesNotExist, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
        }

        /// <summary>
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance. Database does exist.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_InstanceOkDatabaseExists()
        {
            String databaseFolder = "c:\\PermissionsTest";
            String databaseName = "PermissionsTest";

            // Create the database from here.
            StackHashSqlControl.InstallerInterface installerInterface =
                new StackHashSqlControl.InstallerInterface(TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;",
                    databaseName, databaseFolder);

            installerInterface.Connect();
            installerInterface.CreateDatabase(true); // Use default location for the index.
            installerInterface.Disconnect();
            SqlConnection.ClearAllPools();

            try
            {
                StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
                sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
                sqlSettings.ConnectionTimeout = 15;
                sqlSettings.EventsPerBlock = 100;
                sqlSettings.MinPoolSize = 1;
                sqlSettings.MaxPoolSize = 10;
                sqlSettings.InitialCatalog = databaseName;

                TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, true);

                Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

                if (testResp.LastException != null)
                    Console.WriteLine(testResp.LastException);

                Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
            }
            finally
            {
                installerInterface.Connect();
                installerInterface.DeleteDatabase(databaseName); 
                installerInterface.Disconnect();
                SqlConnection.ClearAllPools();
            }
        }

        /// <summary>
        /// Cab folder doesn't exist.
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_CabFolderDoesntExist()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            String cabFolder = "C:\\rubbishfolder";
            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false, cabFolder);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));

            Assert.AreEqual(false, testResp.IsCabFolderAccessible);
        }

        /// <summary>
        /// Cab folder inaccessible.
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_CabFolderNotAccessible()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            String cabFolder = "C:\\users\\guest";
            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false, cabFolder);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
            Assert.AreEqual(false, testResp.IsCabFolderAccessible);

            if (testResp.CabFolderAccessLastException != null)
                Console.WriteLine(testResp.CabFolderAccessLastException);

            // If the folder doesn't exist then this won't be set.

//            Assert.AreEqual(null, testResp.CabFolderAccessLastException);
        }

        /// <summary>
        /// Cab folder accessible.
        /// Tests a connection string outside of a profile. 
        /// Try to connect to a valid instance.
        /// Don't test the database existence.
        /// </summary>
        [TestMethod]
        public void TestDatabaseConnectionNonProfile_CabFolderAccessible()
        {
            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration();
            sqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            sqlSettings.ConnectionTimeout = 15;
            sqlSettings.EventsPerBlock = 100;
            sqlSettings.MinPoolSize = 1;
            sqlSettings.MaxPoolSize = 10;
            sqlSettings.InitialCatalog = "ValidDatabaseName";

            String cabFolder = "C:\\StackHashUnitTests";
            TestDatabaseConnectionResponse testResp = m_Utils.TestDatabaseConnection(sqlSettings, false, cabFolder);

            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, testResp.TestResult);

            if (testResp.LastException != null)
                Console.WriteLine(testResp.LastException);

            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.LastException));
            Assert.AreEqual(true, testResp.IsCabFolderAccessible);

            if (testResp.CabFolderAccessLastException != null)
                Console.WriteLine(testResp.CabFolderAccessLastException);
            Assert.AreEqual(true, String.IsNullOrEmpty(testResp.CabFolderAccessLastException));
        }

        [TestMethod]
        public void GetBugTrackerPlugInDiagnostics()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();

            m_Utils.RestartService();

            GetBugTrackerPlugInDiagnosticsResponse resp = m_Utils.GetBugTrackerPlugInDiagnostics(-1, "TestPlugIn");

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            bool testPlugInFound = false;

            foreach (StackHashBugTrackerPlugInDiagnostics diagnostics in resp.BugTrackerPlugInDiagnostics)
            {
                if (diagnostics.Name == "TestPlugIn")
                {
                    testPlugInFound = true;
                    Assert.AreEqual(13, resp.BugTrackerPlugInDiagnostics[0].DefaultProperties.Count);
                    Assert.AreEqual(0, String.Compare(resp.BugTrackerPlugInDiagnostics[0].FileName, "C:\\ProgramData\\StackHash\\Test\\BugTrackerPlugIns\\StackHashTestBugTrackerPlugin.dll", StringComparison.OrdinalIgnoreCase));
                    Assert.AreEqual(0, String.Compare(resp.BugTrackerPlugInDiagnostics[0].Name, "TestPlugIn", StringComparison.OrdinalIgnoreCase));
                    Assert.AreEqual(true, resp.BugTrackerPlugInDiagnostics[0].Loaded);
                    Assert.AreEqual("No Error", resp.BugTrackerPlugInDiagnostics[0].LastException);
                    Assert.AreEqual("Plug-in used to control StackHash unit testing.", resp.BugTrackerPlugInDiagnostics[0].PlugInDescription);
                    Assert.AreEqual("http://www.stackhash.com/", resp.BugTrackerPlugInDiagnostics[0].HelpUrl.ToString());
                }
            }

            Assert.AreEqual(true, testPlugInFound);
        }

        [TestMethod]
        [Ignore]
        /// The problem with this test is that the loaded DLL can't be deleted until the service drops the 
        /// reference to the assembly. This won't happen until the service AppDomain is unloaded.
        public void GetBugTrackerPlugInDiagnosticsInvalidPlugIn()
        {
            // Start with logging disabled.
            m_Utils.EnableReporting();

            // Copy a random DLL to the plugin folder.
            String dllName = Assembly.GetExecutingAssembly().Location;
            String destFileName = Path.Combine("C:\\ProgramData\\StackHash\\Test\\BugTrackerPlugIns", Path.GetFileName(dllName));



            File.Copy(dllName, destFileName, true);

            try
            {
                m_Utils.RestartService();

                GetBugTrackerPlugInDiagnosticsResponse resp = m_Utils.GetBugTrackerPlugInDiagnostics(-1, null);

                Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
                Assert.AreEqual(null, resp.ResultData.LastException);

                Assert.AreEqual(true, resp.BugTrackerPlugInDiagnostics.Count == 2);

                int errorPlugInIndex = 0;
                int realPlugInIndex = 1;
                if (resp.BugTrackerPlugInDiagnostics[0].Name == "TestPlugIn")
                {
                    realPlugInIndex = 0;
                    errorPlugInIndex = 1;
                }

                Assert.AreEqual(5, resp.BugTrackerPlugInDiagnostics[realPlugInIndex].DefaultProperties.Count);
                Assert.AreEqual(0, String.Compare(resp.BugTrackerPlugInDiagnostics[realPlugInIndex].FileName, "C:\\ProgramData\\StackHash\\Test\\BugTrackerPlugIns\\StackHashTestBugTrackerPlugin.dll", StringComparison.OrdinalIgnoreCase));
                Assert.AreEqual(0, String.Compare(resp.BugTrackerPlugInDiagnostics[realPlugInIndex].Name, "TestPlugIn", StringComparison.OrdinalIgnoreCase));
                Assert.AreEqual(true, resp.BugTrackerPlugInDiagnostics[realPlugInIndex].Loaded);

                Assert.AreEqual(null, resp.BugTrackerPlugInDiagnostics[errorPlugInIndex].DefaultProperties);
                Assert.AreEqual(0, String.Compare(resp.BugTrackerPlugInDiagnostics[errorPlugInIndex].FileName, destFileName, StringComparison.OrdinalIgnoreCase));
                Assert.AreEqual(null, resp.BugTrackerPlugInDiagnostics[errorPlugInIndex].Name);
                Assert.AreEqual(false, resp.BugTrackerPlugInDiagnostics[errorPlugInIndex].Loaded);
            }
            finally
            {
                if (File.Exists(destFileName))
                    File.Delete(destFileName);
            }
        }


        [TestMethod]
        public void SetClientBumpingTimeout()
        {
            StackHashSettings settings = m_Utils.GetContextSettings().Settings;

            int oldClientTimeout = settings.ClientTimeoutInSeconds;

            m_Utils.SetClientTimeout(oldClientTimeout + 1);

            settings = m_Utils.GetContextSettings().Settings;

            Assert.AreEqual(oldClientTimeout + 1, settings.ClientTimeoutInSeconds);
        }

        [TestMethod]
        public void SetClientBumpingTimeoutWithReset()
        {
            StackHashSettings settings = m_Utils.GetContextSettings().Settings;

            int oldClientTimeout = settings.ClientTimeoutInSeconds;

            m_Utils.SetClientTimeout(oldClientTimeout + 1);

            m_Utils.RestartService();

            settings = m_Utils.GetContextSettings().Settings;

            Assert.AreEqual(oldClientTimeout + 1, settings.ClientTimeoutInSeconds);
        }
    }
}
