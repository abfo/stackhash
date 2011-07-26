using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;


using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashTasks;
using StackHashUtilities;
using StackHashDebug;


namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for MoveIndexUnitTests
    /// </summary>
    [TestClass]
    public class MoveIndexUnitTests
    {
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_ServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";
        List<AdminReportEventArgs> m_AdminReports = new List<AdminReportEventArgs>();
        List<AdminReportEventArgs> m_MoveAdminReports = new List<AdminReportEventArgs>();
        AutoResetEvent m_MoveCompletedEvent = new AutoResetEvent(false);
        AutoResetEvent m_MoveProgressEvent = new AutoResetEvent(false);
        DbProviderFactory m_ProviderFactory;
        static String s_MasterConnectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
        static String s_ConnectionString = TestSettings.DefaultConnectionString;
        String s_TestCab = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
        List<StackHashCopyIndexProgressAdminReport> m_CopyIndexProgressReports = new List<StackHashCopyIndexProgressAdminReport>();

        public MoveIndexUnitTests()
        {
            m_ProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        private TestContext testContextInstance;

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

        [TestInitialize()]
        public void MyTestInitialize() 
        {
            m_AdminReports.Clear();
            m_MoveAdminReports.Clear();
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

        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminArgs = e as AdminReportEventArgs;

            if (adminArgs.Report.Operation == StackHashAdminOperation.ErrorIndexMoveCompleted)
            {
                m_AdminReports.Add(adminArgs);
                m_MoveCompletedEvent.Set();
            }
            //else if (adminArgs.Report.Operation == StackHashAdminOperation.ErrorIndexMoveProgress)
            //{
            //    m_CopyIndexProgressReports.Add(adminArgs.Report as StackHashCopyIndexProgressAdminReport);
            //    m_MoveProgressEvent.Set();
            //}
            else if (adminArgs.Report.Operation == StackHashAdminOperation.ErrorIndexMoveProgress)
            {
                m_MoveAdminReports.Add(adminArgs);
            }
            else if (adminArgs.Report.Operation != StackHashAdminOperation.ContextStateChanged)
            {
                m_AdminReports.Add(adminArgs);
            }
        }

        private void waitForMoveCompleted(int timeout)
        {
            m_MoveCompletedEvent.WaitOne(timeout);
        }

        private void waitForMoveProgress(int timeout)
        {
            m_MoveProgressEvent.WaitOne(timeout);
        }

        private IErrorIndex getIndex(ErrorIndexSettings settings, StackHashSqlConfiguration sqlSettings)
        {
            IErrorIndex index;
            if (settings.Type == ErrorIndexType.Xml)
            {
                index = new XmlErrorIndex(settings.Folder, settings.Name);
                index = new ErrorIndexCache(index);
            }
            else
            {
                sqlSettings.InitialCatalog = settings.Name;
                index = new SqlErrorIndex(sqlSettings, settings.Name, settings.Folder);
            }

            return index;

        }


        /// <summary>
        /// An index will be created in sourceFolder\SourceIndex called SourceIndex.
        /// It will then be moved to destFolder\DestIndex.
        /// If defaultDatabaseLocation is specified then only the cab files will be moved and not the SQL database.
        /// </summary>
        private void runMoveTask(String settingsFolder, String sourceErrorIndexFolder, String sourceErrorIndexName, String destErrorIndexFolder, 
            String destErrorIndexName, bool defaultDatabaseLocation, StackHashTestIndexData testIndexData)
        {
            String scriptFolder = settingsFolder + "\\Scripts";


            SqlCommands sqlCommands = new SqlCommands(m_ProviderFactory, s_MasterConnectionString, s_MasterConnectionString, 1);

            // Create the source database folders and settings.

            if (sqlCommands.DatabaseExists(destErrorIndexName))
            {
                try { sqlCommands.DeleteDatabase(destErrorIndexName); }
                catch { ; }
            }
            if (sqlCommands.DatabaseExists(sourceErrorIndexName))
            {
                try { sqlCommands.DeleteDatabase(sourceErrorIndexName); }
                catch { ; }
            }

            if (Directory.Exists(settingsFolder))
                PathUtils.DeleteDirectory(settingsFolder, true);
            if (Directory.Exists(destErrorIndexFolder))
                PathUtils.DeleteDirectory(destErrorIndexFolder, true);
            if (Directory.Exists(sourceErrorIndexFolder))
                PathUtils.DeleteDirectory(sourceErrorIndexFolder, true);
            
            if (!Directory.Exists(sourceErrorIndexFolder))
                Directory.CreateDirectory(sourceErrorIndexFolder);
            if (!Directory.Exists(settingsFolder))
                Directory.CreateDirectory(settingsFolder);
            if (!Directory.Exists(scriptFolder))
                Directory.CreateDirectory(scriptFolder);
            if (!Directory.Exists(destErrorIndexFolder))
                Directory.CreateDirectory(destErrorIndexFolder);


            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(settingsFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = sourceErrorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = sourceErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                contextSettings.SqlSettings.ConnectionString = s_ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = sourceErrorIndexName;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", settingsFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Create a dummy controller to record the callbacks.
                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Progress reports don't come through the controller context - they come straight through the contoller so create a dummy.
                Controller controller = new Controller();
                Reporter reporter = new Reporter(controller);
                controller.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);


                // ******************************************
                // CREATE THE SOURCE INDEX
                // ******************************************

                // Delete any old index first.
                SqlConnection.ClearAllPools();

                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                // Activate the context and the associated index - this will create the index if necessary.
                controllerContext.Activate(null, defaultDatabaseLocation);

                String[] databaseFiles = Directory.GetFiles(Path.Combine(sourceErrorIndexFolder, sourceErrorIndexName), "*.mdf");
                Assert.AreEqual(defaultDatabaseLocation, databaseFiles.Length == 0);                    

                controllerContext.CreateTestIndex(testIndexData);


                Guid guid = new Guid();
                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);


                // ******************************************
                // MOVE TO DESTINATION
                // ******************************************
                               
                // Deactivate before the move.
                controllerContext.Deactivate();

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration(s_ConnectionString, destErrorIndexName, 1, 100, 15, 100);
                
                // Move the index.
                controllerContext.RunMoveIndexTask(clientData, destErrorIndexFolder, destErrorIndexName, sqlConfig);

                // Wait for the move task to complete.
                waitForMoveCompleted(60000 * 20);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexMoveStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexMoveCompleted, m_AdminReports[1].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(StackHashServiceErrorCode.NoError, m_AdminReports[1].Report.ServiceErrorCode);

                if ((testIndexData.NumberOfCabs > 0) && (sourceErrorIndexFolder[0] != destErrorIndexFolder[0]))
                    Assert.AreEqual(true, m_MoveAdminReports.Count > 0);

                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                ErrorIndexSettings destIndexData = new ErrorIndexSettings() 
                { 
                    Folder = destErrorIndexFolder, 
                    Name = destErrorIndexName, 
                    Type = ErrorIndexType.SqlExpress 
                };

                IErrorIndex index1 = getIndex(destIndexData, sqlConfig);

                try
                {
                    index1.Activate();
                   
                    // Make a single call just to ensure the database is still in tact.
                    StackHashProductCollection products = index1.LoadProductList();

                    Assert.AreEqual(testIndexData.NumberOfProducts, products.Count);
                }
                finally
                {
                    index1.Deactivate();
                    index1.Dispose();
                    SqlConnection.ClearAllPools();
                }
            }
            finally
            {
                SqlConnection.ClearAllPools();
                if (sqlCommands.DatabaseExists(destErrorIndexName))
                {
                    try { sqlCommands.DeleteDatabase(destErrorIndexName); }
                    catch { ; }
                }
                if (sqlCommands.DatabaseExists(sourceErrorIndexName))
                {
                    try { sqlCommands.DeleteDatabase(sourceErrorIndexName); }
                    catch { ; }
                }

                SqlConnection.ClearAllPools();

                if (Directory.Exists(sourceErrorIndexFolder))
                {
                    PathUtils.SetFilesWritable(sourceErrorIndexFolder, true);
                    PathUtils.DeleteDirectory(sourceErrorIndexFolder, true);
                }
                if (Directory.Exists(destErrorIndexFolder))
                {
                    PathUtils.SetFilesWritable(destErrorIndexFolder, true);
                    PathUtils.DeleteDirectory(destErrorIndexFolder, true);
                }
                if (Directory.Exists(settingsFolder))
                {
                    PathUtils.SetFilesWritable(settingsFolder, true);
                    PathUtils.DeleteDirectory(settingsFolder, true);
                }
            }
        }


        /// <summary>
        /// Empty initial database (no cabs).
        /// Move to a new destination folder - database name remains unchanged.
        /// Database stored in cab folder.
        /// </summary>
        [TestMethod]
        public void Sql_EmptySource_DestinationFolderChanged()
        {
            bool databaseInDefaultLocation = false;

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 0,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "c:\\stackhashunittests\\MoveUnitTestsDest\\";
            String destDatabaseName = "SourceMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }

        /// <summary>
        /// Initial database contains some cabs.
        /// Move to a new destination folder - database name remains unchanged.
        /// Database stored in cab folder.
        /// </summary>
        [TestMethod]
        public void Sql_NonEmptySource_DestinationFolderChanged()
        {
            bool databaseInDefaultLocation = false;

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 2,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "c:\\stackhashunittests\\MoveUnitTestsDest\\";
            String destDatabaseName = "SourceMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }

        /// <summary>
        /// Initial database contains some cabs.
        /// Move to a new destination folder - database name remains unchanged.
        /// Database stored in default location on SqlServer.
        /// </summary>
        [TestMethod]
        public void Sql_NonEmptySource_DestinationFolderChanged_DatabaseInDefaultLocation()
        {
            bool databaseInDefaultLocation = true;

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 2,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "c:\\stackhashunittests\\MoveUnitTestsDest\\";
            String destDatabaseName = "SourceMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }

        /// <summary>
        /// Empty initial database (no cabs).
        /// Database stored in cab folder.
        /// Rename the database only.
        /// The subfolder where the database is stored will still change and SqlServer will need 
        /// to be told of the name change.
        /// </summary>
        [TestMethod]
        public void Sql_EmptySource_NameChanged()
        {
            bool databaseInDefaultLocation = false;

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 0,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "c:\\Stackhashunittests\\MoveUnitTESTS";  // Change the case and strip the backslash to try and confuse.
            String destDatabaseName = "DestinationMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }


        /// <summary>
        /// Non Empty initial database (contains cabs).
        /// Database stored in cab folder.
        /// Rename the database only.
        /// The subfolder where the database is stored will still change and SqlServer will need 
        /// to be told of the name change.
        /// </summary>
        [TestMethod]
        public void Sql_NonEmptySource_NameChanged()
        {
            bool databaseInDefaultLocation = false;

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 2,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "c:\\Stackhashunittests\\MoveUnitTESTS";  // Change the case and strip the backslash to try and confuse.
            String destDatabaseName = "DestinationMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }


        /// <summary>
        /// Non Empty initial database (contains cabs).
        /// Database stored in default location.
        /// Rename the database only.
        /// The subfolder where the database is stored will still change and SqlServer will need 
        /// to be told of the name change.
        /// </summary>
        [TestMethod]
        public void Sql_NonEmptySource_NameChanged_DefaultDatabaseLocation()
        {
            bool databaseInDefaultLocation = true;

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 2,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "c:\\Stackhashunittests\\MoveUnitTESTS";  // Change the case and strip the backslash to try and confuse.
            String destDatabaseName = "DestinationMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }

        /// <summary>
        /// Non Empty initial database (contains cabs).
        /// Database stored in cab folder.
        /// Rename the database and change the folder.
        /// </summary>
        [TestMethod]
        public void Sql_NonEmptySource_NameAndFolderChanged()
        {
            bool databaseInDefaultLocation = false;

            try
            {
                DriveInfo allDrives = new DriveInfo("z:");
                if (allDrives == null)
                    return;

                if (!allDrives.IsReady)
                    return;
            }
            catch
            {
                // Drive not defined.
                return;
            }

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 2,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "z:\\Stackhashunittests\\MoveUnitTESTS";  // Change the case and strip the backslash to try and confuse.
            String destDatabaseName = "DestinationMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }


        /// <summary>
        /// Non Empty initial database (contains cabs).
        /// Database stored in default location.
        /// Rename the database and folder.
        /// </summary>
        [TestMethod]
        public void Sql_NonEmptySource_NameAndFolderChanged_DefaultDatabaseLocation()
        {
            bool databaseInDefaultLocation = true;

            try
            {
                DriveInfo allDrives = new DriveInfo("z:");
                if (allDrives == null)
                    return;

                if (!allDrives.IsReady)
                    return;
            }
            catch
            {
                // Drive not defined.
                return;
            }

            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 2,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            String settingsFolder = "c:\\stackhashunittests\\Settings\\";
            String sourceFolder = "c:\\stackhashunittests\\MoveUnitTests\\";
            String sourceDatabaseName = "SourceMoveIndex";
            String destFolder = "z:\\Stackhashunittests\\MoveUnitTESTS";  // Change the case and strip the backslash to try and confuse.
            String destDatabaseName = "DestinationMoveIndex";

            runMoveTask(settingsFolder, sourceFolder, sourceDatabaseName, destFolder, destDatabaseName, databaseInDefaultLocation, testIndexData);
        }
    }
}
