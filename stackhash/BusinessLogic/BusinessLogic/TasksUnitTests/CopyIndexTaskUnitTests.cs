using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashTasks;
using StackHashUtilities;
using StackHashDebug;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for CopyIndexTaskUnitTests
    /// </summary>
    [TestClass]
    public class CopyIndexTaskUnitTests
    {
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_ServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";
        List<AdminReportEventArgs> m_AdminReports = new List<AdminReportEventArgs>();
        AutoResetEvent m_CopyCompletedEvent = new AutoResetEvent(false);
        AutoResetEvent m_CopyProgressEvent = new AutoResetEvent(false);
        DbProviderFactory m_ProviderFactory;
        static String s_MasterConnectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
        static String s_ConnectionString = TestSettings.DefaultConnectionString;
        String s_TestCab = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
        List<StackHashCopyIndexProgressAdminReport> m_CopyIndexProgressReports = new List<StackHashCopyIndexProgressAdminReport>();
        long m_LastProgressEventCount = 0;

        public CopyIndexTaskUnitTests()
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

        [TestCleanup()]
        public void MyTestCleanup() 
        { 
             SqlConnection.ClearAllPools();
             m_CopyIndexProgressReports.Clear();
             m_LastProgressEventCount = 0;
        }
        
        #endregion

        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminArgs = e as AdminReportEventArgs;

            if (adminArgs.Report.Operation == StackHashAdminOperation.ErrorIndexCopyCompleted)
            {
                m_AdminReports.Add(adminArgs);
                m_CopyCompletedEvent.Set();
            }
            else if (adminArgs.Report.Operation == StackHashAdminOperation.ErrorIndexCopyProgress)
            {
                m_CopyIndexProgressReports.Add(adminArgs.Report as StackHashCopyIndexProgressAdminReport);
                m_CopyProgressEvent.Set();
                m_LastProgressEventCount = m_CopyIndexProgressReports[m_CopyIndexProgressReports.Count - 1].CurrentEvent; 
            }
            else if (adminArgs.Report.Operation != StackHashAdminOperation.ContextStateChanged)
            {
                m_AdminReports.Add(adminArgs);
            }
        }

        private void waitForCopyCompleted(int timeout)
        {
            m_CopyCompletedEvent.WaitOne(timeout);
        }

        private void waitForCopyProgress(int timeout)
        {
            m_CopyProgressEvent.WaitOne(timeout);
        }

        private void waitForCopyProgress(int timeout, int eventCount)
        {
            int timeToWait = timeout;
            while ((m_LastProgressEventCount < eventCount) && (timeout > 0))
            {
                m_CopyProgressEvent.WaitOne(100);
                timeToWait -= 100;
            }
        }

        
        private void compareCabFiles(IErrorIndex sourceIndex, IErrorIndex destinationIndex,
            StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            // Copy the actual Cab file and any results files.
            String sourceCabFolder = sourceIndex.GetCabFolder(product, file, theEvent, cab);
            String destCabFolder = destinationIndex.GetCabFolder(product, file, theEvent, cab);

            if (Directory.Exists(sourceCabFolder))
            {
                String sourceCabFileName = sourceIndex.GetCabFileName(product, file, theEvent, cab);

                if (File.Exists(sourceCabFileName))
                {
                    String destinationCabFileName =
                        String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", destCabFolder, Path.GetFileName(sourceCabFileName));

                    Assert.AreEqual(true, File.Exists(destinationCabFileName));
                }


                // Copy the analysis data if it exists.
                String sourceAnalysisFolder = String.Format(CultureInfo.InvariantCulture, "{0}\\Analysis", sourceCabFolder);
                String destAnalysisFolder = String.Format(CultureInfo.InvariantCulture, "{0}\\Analysis", destCabFolder);

                if (Directory.Exists(sourceAnalysisFolder))
                {
                    String[] allFiles = Directory.GetFiles(sourceAnalysisFolder, "*.log");

                    if (allFiles.Length != 0)
                    {
                        foreach (String fileName in allFiles)
                        {
                            String destAnalysisFile =
                                String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", destAnalysisFolder, Path.GetFileName(fileName));
                            Assert.AreEqual(true, File.Exists(destAnalysisFile));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares all data in 2 databases.
        /// Throw an exception if not the same.
        /// </summary>
        /// <param name="index1">First index to compare.</param>
        /// <param name="index2">Second index to compare.</param>
        private void compareIndexes(IErrorIndex index1, IErrorIndex index2, bool ignoreNotes)
        {
            StackHashProductCollection products1 = index1.LoadProductList();
            StackHashProductCollection products2 = index2.LoadProductList();

            Assert.AreEqual(products1.Count, products2.Count);

            foreach (StackHashProduct product in products1)
            {
                StackHashProduct matchedProduct = products2.FindProduct(product.Id);

                matchedProduct.FilesLink = product.FilesLink; // Not used.

                Assert.AreNotEqual(null, matchedProduct);
                Assert.AreEqual(0, product.CompareTo(matchedProduct));
            }

            foreach (StackHashProduct product in products1)
            {
                Assert.AreEqual(index1.GetLastHitTimeLocal(product.Id), index2.GetLastHitTimeLocal(product.Id));
                Assert.AreEqual(index1.GetLastSyncCompletedTimeLocal(product.Id), index2.GetLastSyncCompletedTimeLocal(product.Id));
                Assert.AreEqual(index1.GetLastSyncStartedTimeLocal(product.Id), index2.GetLastSyncStartedTimeLocal(product.Id));
                Assert.AreEqual(index1.GetLastSyncTimeLocal(product.Id), index2.GetLastSyncTimeLocal(product.Id));

                StackHashFileCollection files1 = index1.LoadFileList(product);
                StackHashFileCollection files2 = index2.LoadFileList(product);

                Assert.AreEqual(files1.Count, files2.Count);

                foreach (StackHashFile file in files1)
                {
                    StackHashFile matchedFile = files2.FindFile(file.Id);

                    Assert.AreNotEqual(null, matchedFile);
                    Assert.AreEqual(0, file.CompareTo(matchedFile));
                }
                
                foreach (StackHashFile file in files1)
                {
                    StackHashSearchCriteriaCollection searchCriteria = new StackHashSearchCriteriaCollection()
                        {
                            new StackHashSearchCriteria(
                                new StackHashSearchOptionCollection()
                                {
                                    new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Id, 0),
                                    new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file.Id, 0),
                                })
                        };

                    StackHashEventPackageCollection events1 = index1.GetEvents(searchCriteria, null);
                    StackHashEventPackageCollection events2 = index2.GetEvents(searchCriteria, null);

                    Assert.AreNotEqual(null, events1);
                    Assert.AreNotEqual(null, events2);

                    Assert.AreEqual(events1.Count, events2.Count);

                    foreach (StackHashEventPackage eventPackage in events1)
                    {
                        StackHashEventPackage matchedEvent = events2.FindEventPackage(eventPackage.EventData.Id, eventPackage.EventData.EventTypeName);

                        Assert.AreNotEqual(null, matchedEvent);
                        Assert.AreEqual(0, eventPackage.EventData.CompareTo(matchedEvent.EventData, false));

                        // Only normalizes for XML to Sql.
                        if (index1.IndexType == ErrorIndexType.Xml)
                            eventPackage.EventInfoList = eventPackage.EventInfoList.Normalize();

                        Assert.AreEqual(0, eventPackage.EventInfoList.CompareTo(matchedEvent.EventInfoList));
                        Assert.AreEqual(0, eventPackage.Cabs.CompareTo(matchedEvent.Cabs));

                        foreach (StackHashCabPackage cab in eventPackage.Cabs)
                        {
                            if (!ignoreNotes)
                            {
                                // Get the cab notes.
                                StackHashNotes cabNotes1 = index1.GetCabNotes(product, file, eventPackage.EventData, cab.Cab);
                                StackHashNotes cabNotes2 = index2.GetCabNotes(product, file, eventPackage.EventData, cab.Cab);

                                Assert.AreEqual(cabNotes1.Count, cabNotes2.Count);

                                for (int i = 0; i < cabNotes1.Count; i++)
                                {
                                    // Make sure that there is a matching note in the source and destination index.
                                    Assert.AreEqual(0, cabNotes1[i].CompareTo(cabNotes2[i]));

                                    // Make sure there are no duplicate entries in either list.
                                    if (cabNotes1.MatchingCount(cabNotes1[i]) != 1)
                                        Console.WriteLine("Help");
                                    Assert.AreEqual(1, cabNotes1.MatchingCount(cabNotes1[i]));
                                    Assert.AreEqual(1, cabNotes2.MatchingCount(cabNotes2[i]));
                                }
                            }


                            compareCabFiles(index1, index2, product, file, eventPackage.EventData, cab.Cab);
                        }

                        // Check the event infos are as expected.
                        Assert.AreEqual(0, eventPackage.EventInfoList.CompareTo(matchedEvent.EventInfoList));

                        if (!ignoreNotes)
                        {
                            StackHashNotes eventNotes1 = index1.GetEventNotes(product, file, eventPackage.EventData);
                            StackHashNotes eventNotes2 = index2.GetEventNotes(product, file, eventPackage.EventData);

                            Assert.AreEqual(eventNotes1.Count, eventNotes2.Count);

                            for (int i = 0; i < eventNotes1.Count; i++)
                            {
                                Assert.AreEqual(0, eventNotes1[i].CompareTo(eventNotes2[i]));
                            }
                        }
                    }
                }
            }

            Type taskEnumType = typeof(StackHashTaskType);
            Array allValues = Enum.GetValues(taskEnumType);

            foreach (StackHashTaskType taskType in allValues)
            {
                StackHashTaskStatus taskStatus1 = index1.GetTaskStatistics(taskType);
                StackHashTaskStatus taskStatus2 = index2.GetTaskStatistics(taskType);


                // Ignore the task completed stats for the copy task as the task was still running when we got them.
                if (taskType == StackHashTaskType.ErrorIndexCopyTask)
                {
                    taskStatus2.LastSuccessfulRunTimeUtc = taskStatus1.LastSuccessfulRunTimeUtc;
                    taskStatus2.LastDurationInSeconds = taskStatus1.LastDurationInSeconds;
                    taskStatus2.SuccessCount++;
                    taskStatus2.TaskState = taskStatus1.TaskState;
                }

                if (taskStatus1.CompareTo(taskStatus2) != 0)
                {
                    Console.WriteLine("TaskType: " + taskType.ToString());

                    Console.WriteLine(taskStatus1.ToString());
                    Console.WriteLine(taskStatus2.ToString());

                    Assert.AreEqual(0, taskStatus1.CompareTo(taskStatus2));
                }
            }
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

        private void runCopyTask(ErrorIndexType srcErrorIndexType, ErrorIndexType destErrorIndexType,
            StackHashTestIndexData testIndexData, bool switchIndexInContext)
        {
            String rootTestFolder = "c:\\stackhashunittests\\CopyUnitTests\\";
            String sourceErrorIndexFolder = rootTestFolder;
            String sourceErrorIndexName = "SourceIndex";
            String destErrorIndexFolder = rootTestFolder;
            String destErrorIndexName = "DestIndex";

            String scriptFolder = rootTestFolder + "Scripts";


            SqlCommands sqlCommands = new SqlCommands(m_ProviderFactory, s_MasterConnectionString, s_MasterConnectionString, 1);

            // Delete any remnants that may exist from a previous test.
            try
            {
                sqlCommands.DeleteDatabase(sourceErrorIndexName);
            }
            catch { };

            try
            {
                sqlCommands.DeleteDatabase(destErrorIndexName);
            }
            catch { };


            if (Directory.Exists(rootTestFolder))
            {
                PathUtils.SetFilesWritable(rootTestFolder, true);
                PathUtils.DeleteDirectory(rootTestFolder, true);
            }

            Directory.CreateDirectory(rootTestFolder);
            Directory.CreateDirectory(scriptFolder);



            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(rootTestFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = rootTestFolder;
                contextSettings.ErrorIndexSettings.Name = sourceErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = srcErrorIndexType;

                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                contextSettings.SqlSettings.ConnectionString = s_ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = "TestIndex";

                ErrorIndexSettings originalSourceIndexSettings = contextSettings.ErrorIndexSettings;
                StackHashSqlConfiguration originalSqlSettings = contextSettings.SqlSettings;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", rootTestFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Create a dummy controller to record the callbacks.
                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                SqlConnection.ClearAllPools();
                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; } 

                // Activate the context and the associated index - this will create the index if necessary.
                controllerContext.Activate();


                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Progress reports don't come through the controller context - they come straight through the contoller so create a dummy.
                Controller controller = new Controller();
                Reporter reporter = new Reporter(controller);
                controller.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                Guid guid = new Guid();
                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                ErrorIndexSettings destIndexData = new ErrorIndexSettings() 
                    { Folder = destErrorIndexFolder, Name = destErrorIndexName, Type = destErrorIndexType };



                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                    {
                        try
                        {
                            sqlCommands.DeleteDatabase(destErrorIndexName);
                        }
                        catch {};
                    }
                    sqlCommands.CreateStackHashDatabase(destErrorIndexFolder + destErrorIndexName, destErrorIndexName, false);
                }

                // Deactivate before the copy.
                controllerContext.Deactivate();

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration(s_ConnectionString, destErrorIndexName, 1, 100, 15, 100);
                controllerContext.RunCopyIndexTask(clientData, destIndexData, sqlConfig, switchIndexInContext, false);

                // Wait for the copy task to complete.
                waitForCopyCompleted(60000 * 20);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyCompleted, m_AdminReports[1].Report.Operation);

                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                
                // Compare the indexes.
                IErrorIndex index1 = getIndex(originalSourceIndexSettings, originalSqlSettings);
                index1.Activate();
                IErrorIndex index2 = getIndex(destIndexData, sqlConfig);
                index2.Activate();

                compareIndexes(index1, index2, false);

                StackHashContextSettings newContextSettings = settingsManager.GetContextSettings(0);

                if (switchIndexInContext)
                {
                    Assert.AreEqual(destIndexData.Folder, newContextSettings.ErrorIndexSettings.Folder);
                    Assert.AreEqual(destIndexData.Name, newContextSettings.ErrorIndexSettings.Name);
                    Assert.AreEqual(destIndexData.Status, newContextSettings.ErrorIndexSettings.Status);
                    Assert.AreEqual(destIndexData.Type, newContextSettings.ErrorIndexSettings.Type);
                }
                else
                {
                    Assert.AreEqual(originalSourceIndexSettings.Folder, newContextSettings.ErrorIndexSettings.Folder);
                    Assert.AreEqual(originalSourceIndexSettings.Name, newContextSettings.ErrorIndexSettings.Name);
                    Assert.AreEqual(originalSourceIndexSettings.Status, newContextSettings.ErrorIndexSettings.Status);
                    Assert.AreEqual(originalSourceIndexSettings.Type, newContextSettings.ErrorIndexSettings.Type);
                }


                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                        sqlCommands.DeleteDatabase(destErrorIndexName);
                }

                SqlConnection.ClearAllPools();
                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                controllerContext.Dispose();
                SqlConnection.ClearAllPools();

//                if (switchIndexInContext)
                {
                    index1.Deactivate();
                    index2.Deactivate();

                    try
                    {
                        index1.DeleteIndex();
                    }
                    catch { ; }

                    try
                    {
                        index2.DeleteIndex();
                    }
                    catch { ; }
                    index1.Dispose();
                    index2.Dispose();
                    SqlConnection.ClearAllPools();
                }

                int totalExpectedEvents = testIndexData.NumberOfProducts * testIndexData.NumberOfFiles * testIndexData.NumberOfEvents;

                if (totalExpectedEvents > 0)
                {
                    Assert.AreEqual(true, m_CopyIndexProgressReports.Count > 0);
                    long lastEvent = 0;
                    foreach (StackHashCopyIndexProgressAdminReport report in m_CopyIndexProgressReports)
                    {
                        Assert.AreEqual(totalExpectedEvents, report.TotalEvents);
                        Assert.AreEqual(true, report.CurrentEvent > lastEvent);
                        Assert.AreEqual(true, report.CurrentEventId != 0);
                        Assert.AreNotEqual(null, report.SourceIndexFolder);
                        Assert.AreNotEqual(null, report.SourceIndexName);
                        Assert.AreNotEqual(null, report.DestinationIndexFolder);
                        Assert.AreNotEqual(null, report.DestinationIndexName);
                    }
                }

            }
            finally
            {
                try
                {
                    sqlCommands.DeleteDatabase(sourceErrorIndexName);
                }
                catch { };

                try
                {
                    sqlCommands.DeleteDatabase(destErrorIndexName);
                }
                catch { };

                if (Directory.Exists(rootTestFolder))
                {
                    PathUtils.SetFilesWritable(rootTestFolder, true);
                    PathUtils.DeleteDirectory(rootTestFolder, true);
                }
            }
        }

        private void runCopyTaskWithDelete(ErrorIndexType srcErrorIndexType, ErrorIndexType destErrorIndexType,
            StackHashTestIndexData testIndexData, bool switchIndexInContext)
        {
            String rootTestFolder = "c:\\stackhashunittests\\CopyUnitTests\\";
            String sourceErrorIndexFolder = rootTestFolder;
            String sourceErrorIndexName = "SourceIndex";
            String destErrorIndexFolder = rootTestFolder;
            String destErrorIndexName = "DestIndex";

            String scriptFolder = rootTestFolder + "Scripts";


            SqlCommands sqlCommands = new SqlCommands(m_ProviderFactory, s_MasterConnectionString, s_MasterConnectionString, 1);


            // Delete any remnants that may exist from a previous test.
            try
            {
                sqlCommands.DeleteDatabase(sourceErrorIndexName);
            }
            catch { };

            try
            {
                sqlCommands.DeleteDatabase(destErrorIndexName);
            }
            catch { };

            if (Directory.Exists(rootTestFolder))
            {
                PathUtils.SetFilesWritable(rootTestFolder, true);
                PathUtils.DeleteDirectory(rootTestFolder, true);
            }

            Directory.CreateDirectory(rootTestFolder);
            Directory.CreateDirectory(scriptFolder);


            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(rootTestFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = rootTestFolder;
                contextSettings.ErrorIndexSettings.Name = sourceErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = srcErrorIndexType;

                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                contextSettings.SqlSettings.ConnectionString = s_ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = "TestIndex";

                ErrorIndexSettings originalSourceIndexSettings = contextSettings.ErrorIndexSettings;
                StackHashSqlConfiguration originalSqlSettings = contextSettings.SqlSettings;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", rootTestFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Create a dummy controller to record the callbacks.
                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);
                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                SqlConnection.ClearAllPools();

                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                // Activate the context and the associated index - this will create the index if necessary.
                controllerContext.Activate();

                // Create the test index.
                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                Guid guid = new Guid();
                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                ErrorIndexSettings destIndexData = new ErrorIndexSettings() { Folder = destErrorIndexFolder, Name = destErrorIndexName, Type = destErrorIndexType };



                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                        sqlCommands.DeleteDatabase(destErrorIndexName);
                    sqlCommands.CreateStackHashDatabase(destErrorIndexFolder + destErrorIndexName, destErrorIndexName, false);
                }

                // Deactivate before the copy.
                controllerContext.Deactivate();

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration(s_ConnectionString, destErrorIndexName, 1, 100, 15, 100);
                controllerContext.RunCopyIndexTask(clientData, destIndexData, sqlConfig, switchIndexInContext, true);

                // Wait for the copy task to complete.
                waitForCopyCompleted(60000 * 20);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyCompleted, m_AdminReports[1].Report.Operation);

                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);


                // Check quickly that the original index is not longer around.
                IErrorIndex index1 = getIndex(originalSourceIndexSettings, originalSqlSettings);
                Assert.AreEqual(index1.Status, ErrorIndexStatus.NotCreated);

                IErrorIndex index2 = getIndex(destIndexData, sqlConfig);
                index2.Activate();

                StackHashContextSettings newContextSettings = settingsManager.GetContextSettings(0);

                Assert.AreEqual(destIndexData.Folder, newContextSettings.ErrorIndexSettings.Folder);
                Assert.AreEqual(destIndexData.Name, newContextSettings.ErrorIndexSettings.Name);
                Assert.AreEqual(destIndexData.Status, newContextSettings.ErrorIndexSettings.Status);
                Assert.AreEqual(destIndexData.Type, newContextSettings.ErrorIndexSettings.Type);

                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                        sqlCommands.DeleteDatabase(destErrorIndexName);
                }

                SqlConnection.ClearAllPools();

                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }
                controllerContext.Dispose();
                SqlConnection.ClearAllPools();

                index1.Deactivate();
                index2.Deactivate();

                try
                {
                    index1.DeleteIndex();
                }
                catch { ; }

                try
                {
                    index2.DeleteIndex();
                }
                catch { ; }
                index1.Dispose();
                index2.Dispose();
                SqlConnection.ClearAllPools();
            }
            finally
            {
                try
                {
                    sqlCommands.DeleteDatabase(sourceErrorIndexName);
                }
                catch { };

                try
                {
                    sqlCommands.DeleteDatabase(destErrorIndexName);
                }
                catch { };

                if (Directory.Exists(rootTestFolder))
                {
                    PathUtils.SetFilesWritable(rootTestFolder, true);
                    PathUtils.DeleteDirectory(rootTestFolder, true);
                }
            }
        }

        [TestMethod]
        public void XmlToSql_Empty()
        {
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

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_OneOfEachObject()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_OneProductOnly()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F1E()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F1E1EI()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F1E1EI1C()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F1E1EI1C1EN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F1E1EI1C1EN1CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToSql_2P2F2E2EI2C2EN2CN()
        {
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

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToSql_2P2F201E1EI1C1EN1CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 201,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToSql_VariousNumbersOfObjects1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 3,
                NumberOfEvents = 4,
                NumberOfEventInfos = 5,
                NumberOfCabs = 1,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 3,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToSql_SwitchIndexAfterCopy()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, true);
        }



        [TestMethod]
        public void XmlToXml_Empty()
        {
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

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_OneOfEachObject()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_OneProductOnly()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F1E()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F1E1EI()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F1E1EI1C()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F1E1EI1C1EN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F1E1EI1C1EN1CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }


        [TestMethod]
        public void XmlToXml_2P2F2E2EI2C2EN2CN()
        {
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

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_1P1F250E2EI0C2EN0CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 250,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1P1F250E2EI0C2EN0CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 250,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F250E2EI0C2EN0CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 250,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToXml_VariousNumbersOfObjects1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 3,
                NumberOfEvents = 4,
                NumberOfEventInfos = 5,
                NumberOfCabs = 1,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 3,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void XmlToXml_SwitchIndexAfterCopy()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.Xml, testIndexData, true);
        }

        [TestMethod]
        public void SqlToSql_Empty()
        {
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

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_WithDelete()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDelete(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, true);
        }

        
        [TestMethod]
        public void SqlToSql_OneOfEachObject()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_OneProductOnly()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F1E()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F1E1EI()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F1E1EI1C()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F1E1EI1C1EN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1P1F1E1EI1C1EN1CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void SqlToSql_2P2F2E2EI2C2EN2CN()
        {
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

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }



        [TestMethod]
        public void SqlToSql_VariousNumbersOfObjects1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 3,
                NumberOfEvents = 4,
                NumberOfEventInfos = 5,
                NumberOfCabs = 1,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 3,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_SwitchIndexAfterCopy()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, true);
        }


        [TestMethod]
        public void SqlToXml_Empty()
        {
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

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_OneOfEachObject()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_OneProductOnly()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_1P1F()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_1P1F1E()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_1P1F1E1EI()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_1P1F1E1EI1C()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_1P1F1E1EI1C1EN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_1P1F1E1EI1C1EN1CN()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }


        [TestMethod]
        public void SqlToXml_2P2F2E2EI2C2EN2CN()
        {
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

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }



        [TestMethod]
        public void SqlToXml_VariousNumbersOfObjects1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 3,
                NumberOfEvents = 4,
                NumberOfEventInfos = 5,
                NumberOfCabs = 1,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 3,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, false);
        }

        [TestMethod]
        public void SqlToXml_SwitchIndexAfterCopy()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.SqlExpress, ErrorIndexType.Xml, testIndexData, true);
        }

        
        [TestMethod]
        [Ignore]
        public void XmlToSql_VariousNumbersOfObjects2()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 5,
                NumberOfFiles = 5,
                NumberOfEvents = 50,
                NumberOfEventInfos = 5,
                NumberOfCabs = 3,
                NumberOfEventNotes = 10,
                NumberOfCabNotes = 10,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTask(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        /// <summary>
        /// Copy fails in middle - should NOT delete source index.
        /// </summary>
        private void runCopyTaskWithDeleteFail(ErrorIndexType srcErrorIndexType, ErrorIndexType destErrorIndexType,
            StackHashTestIndexData testIndexData, bool switchIndexInContext)
        {
            String rootTestFolder = "c:\\stackhashunittests\\CopyUnitTests\\";
            String sourceErrorIndexFolder = rootTestFolder;
            String sourceErrorIndexName = "SourceIndex";
            String destErrorIndexFolder = rootTestFolder;
            String destErrorIndexName = "DestIndex";

            String scriptFolder = rootTestFolder + "Scripts";


            SqlCommands sqlCommands = new SqlCommands(m_ProviderFactory, s_MasterConnectionString, s_MasterConnectionString, 1);

            try
            {
                sqlCommands.DeleteDatabase(sourceErrorIndexName);
            }
            catch { };

            try
            {
                sqlCommands.DeleteDatabase(destErrorIndexName);
            }
            catch { };

            // Delete any remnants that may exist from a previous test.
            if (Directory.Exists(rootTestFolder))
            {
                PathUtils.SetFilesWritable(rootTestFolder, true);
                PathUtils.DeleteDirectory(rootTestFolder, true);
            }

            Directory.CreateDirectory(rootTestFolder);
            Directory.CreateDirectory(scriptFolder);


            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(rootTestFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = rootTestFolder;
                contextSettings.ErrorIndexSettings.Name = sourceErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = srcErrorIndexType;

                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                contextSettings.SqlSettings.ConnectionString = s_ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = "TestIndex";

                ErrorIndexSettings originalSourceIndexSettings = contextSettings.ErrorIndexSettings;
                StackHashSqlConfiguration originalSqlSettings = contextSettings.SqlSettings;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", rootTestFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Create a dummy controller to record the callbacks.
                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);
                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                SqlConnection.ClearAllPools();

                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                // Activate the context and the associated index - this will create the index if necessary.
                controllerContext.Activate();

                // Create the test index.
                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Progress reports don't come through the controller context - they come straight through the contoller so create a dummy.
                Controller controller = new Controller();
                Reporter reporter = new Reporter(controller);
                controller.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);


                Guid guid = new Guid();
                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                ErrorIndexSettings destIndexData = new ErrorIndexSettings() { Folder = destErrorIndexFolder, Name = destErrorIndexName, Type = destErrorIndexType };



                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                        sqlCommands.DeleteDatabase(destErrorIndexName);
                    sqlCommands.CreateStackHashDatabase(destErrorIndexFolder + destErrorIndexName, destErrorIndexName, false);
                }

                // Deactivate before the copy.
                controllerContext.Deactivate();

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration(s_ConnectionString, destErrorIndexName, 1, 100, 15, 100);
                controllerContext.RunCopyIndexTask(clientData, destIndexData, sqlConfig, switchIndexInContext, true);

                // Wait for the first progress report - then abort.
                waitForCopyProgress(20000);

                controllerContext.AbortTaskOfType(StackHashTaskType.ErrorIndexCopyTask);

                // Wait for the copy task to complete.
                waitForCopyCompleted(60000 * 20);


                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyStarted, m_AdminReports[0].Report.Operation);

                Assert.AreNotEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyCompleted, m_AdminReports[1].Report.Operation);

                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);


                // Check quickly that the original index is still around.
                IErrorIndex index1 = getIndex(originalSourceIndexSettings, originalSqlSettings);

                try
                {
                    Assert.AreEqual(ErrorIndexStatus.Created, index1.Status);
                }
                finally
                {
                    IErrorIndex index2 = getIndex(destIndexData, sqlConfig);
                    if (destErrorIndexType != ErrorIndexType.Xml)
                    {
                        if (sqlCommands.DatabaseExists(destErrorIndexName))
                            sqlCommands.DeleteDatabase(destErrorIndexName);
                    }

                    SqlConnection.ClearAllPools();
                    try
                    {
                        controllerContext.DeleteIndex();
                    }
                    catch { ; }
                    controllerContext.Dispose();
                    SqlConnection.ClearAllPools();

                    index1.Deactivate();
                    index2.Deactivate();

                    try
                    {
                        index1.DeleteIndex();
                    }
                    catch { ; }
                    try
                    {
                        index2.DeleteIndex();
                    }
                    catch { ; }
                    index1.Dispose();
                    index2.Dispose();
                    SqlConnection.ClearAllPools();
                }
            }
            finally
            {
                try
                {
                    sqlCommands.DeleteDatabase(sourceErrorIndexName);
                }
                catch { };

                try
                {
                    sqlCommands.DeleteDatabase(destErrorIndexName);
                }
                catch { };
                if (Directory.Exists(rootTestFolder))
                {
                    PathUtils.SetFilesWritable(rootTestFolder, true);
                    PathUtils.DeleteDirectory(rootTestFolder, true);
                }
            }
        }

        [TestMethod]
        public void XmlToSql_CopyFailsWithDeleteSpecified()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 10,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDeleteFail(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, true);
        }

        [TestMethod]
        public void SqlToSql_CopyFailsWithDeleteSpecified()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 10,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 0,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDeleteFail(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, true);
        }

        /// <summary>
        /// An event may be assigned to more than one file/product. In the old XML index this will have been stored as 
        /// 2 separate folders. In the SQL index this is stored as a single item. Need to ignore the second event when doing
        /// the copy along with any duplicate notes etc...
        /// </summary>
        private void runCopyTaskWithDuplicateEvents(ErrorIndexType srcErrorIndexType, ErrorIndexType destErrorIndexType,
            StackHashTestIndexData testIndexData, bool switchIndexInContext)
        {
            String rootTestFolder = "c:\\stackhashunittests\\CopyUnitTests\\";
            String sourceErrorIndexFolder = rootTestFolder;
            String sourceErrorIndexName = "SourceIndex";
            String destErrorIndexFolder = rootTestFolder;
            String destErrorIndexName = "DestIndex";

            String scriptFolder = rootTestFolder + "Scripts";


            SqlCommands sqlCommands = new SqlCommands(m_ProviderFactory, s_MasterConnectionString, s_MasterConnectionString, 1);

            try
            {
                sqlCommands.DeleteDatabase(sourceErrorIndexName);
            }
            catch { };

            try
            {
                sqlCommands.DeleteDatabase(destErrorIndexName);
            }
            catch { };

            // Delete any remnants that may exist from a previous test.
            if (Directory.Exists(rootTestFolder))
            {
                PathUtils.SetFilesWritable(rootTestFolder, true);
                PathUtils.DeleteDirectory(rootTestFolder, true);
            }

            Directory.CreateDirectory(rootTestFolder);
            Directory.CreateDirectory(scriptFolder);



            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(rootTestFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = rootTestFolder;
                contextSettings.ErrorIndexSettings.Name = sourceErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = srcErrorIndexType;

                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                contextSettings.SqlSettings.ConnectionString = s_ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = "TestIndex";

                ErrorIndexSettings originalSourceIndexSettings = contextSettings.ErrorIndexSettings;
                StackHashSqlConfiguration originalSqlSettings = contextSettings.SqlSettings;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", rootTestFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Create a dummy controller to record the callbacks.
                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);
                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                SqlConnection.ClearAllPools();

                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                // Activate the context and the associated index - this will create the index if necessary.
                controllerContext.Activate();

                // Create the test index - include duplicates.
                controllerContext.CreateTestIndex(testIndexData, true);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Progress reports don't come through the controller context - they come straight through the contoller so create a dummy.
                Controller controller = new Controller();
                Reporter reporter = new Reporter(controller);
                controller.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);


                Guid guid = new Guid();
                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                ErrorIndexSettings destIndexData = new ErrorIndexSettings() { Folder = destErrorIndexFolder, Name = destErrorIndexName, Type = destErrorIndexType };



                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                        sqlCommands.DeleteDatabase(destErrorIndexName);
                    sqlCommands.CreateStackHashDatabase(destErrorIndexFolder + destErrorIndexName, destErrorIndexName, false);
                }

                // Deactivate before the copy.
                controllerContext.Deactivate();

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration(s_ConnectionString, destErrorIndexName, 1, 100, 15, 100);
                controllerContext.RunCopyIndexTask(clientData, destIndexData, sqlConfig, switchIndexInContext, false);

                // Wait for the first progress report - then abort.
                waitForCopyProgress(20000);

                // Wait for the copy task to complete.
                waitForCopyCompleted(60000 * 20);


                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyCompleted, m_AdminReports[1].Report.Operation);

                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);


                // Check quickly that the original index is still around.
                IErrorIndex index1 = getIndex(originalSourceIndexSettings, originalSqlSettings);
                IErrorIndex index2 = getIndex(destIndexData, sqlConfig);
                index1.Activate();
                index2.Activate();

                try
                {
                    Assert.AreEqual(ErrorIndexStatus.Created, index1.Status);
                    Assert.AreEqual(ErrorIndexStatus.Created, index2.Status);

                    compareIndexes(index1, index2, false);  // Don't ignore event and cab notes
                }
                finally
                {
                    if (srcErrorIndexType != ErrorIndexType.Xml)
                    {
                        if (sqlCommands.DatabaseExists(sourceErrorIndexName))
                            sqlCommands.DeleteDatabase(sourceErrorIndexName);
                    }

                    if (destErrorIndexType != ErrorIndexType.Xml)
                    {
                        if (sqlCommands.DatabaseExists(destErrorIndexName))
                            sqlCommands.DeleteDatabase(destErrorIndexName);
                    }

                    SqlConnection.ClearAllPools();

                    try
                    {
                        controllerContext.DeleteIndex();
                    }
                    catch { ; }
                    controllerContext.Dispose();
                    SqlConnection.ClearAllPools();

                    index1.Deactivate();
                    index2.Deactivate();

                    try
                    {
                        index1.DeleteIndex();
                    }
                    catch { ; }

                    try
                    {
                        index2.DeleteIndex();
                    }
                    catch { ; }
                    index1.Dispose();
                    index2.Dispose();
                    SqlConnection.ClearAllPools();
                }
            }
            finally
            {
                try
                {
                    sqlCommands.DeleteDatabase(sourceErrorIndexName);
                }
                catch { };

                try
                {
                    sqlCommands.DeleteDatabase(destErrorIndexName);
                }
                catch { };

                if (Directory.Exists(rootTestFolder))
                {
                    PathUtils.SetFilesWritable(rootTestFolder, true);
                    PathUtils.DeleteDirectory(rootTestFolder, true);
                }
            }
        }

        [TestMethod]
        public void XmlToSql_CopyTaskWithDuplicateEvents()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDuplicateEvents(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_CopyTaskWithDuplicateEvents()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDuplicateEvents(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToSql_CopyTaskWithDuplicateEvents2Notes()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDuplicateEvents(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_CopyTaskWithDuplicateEvents2Notes()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 1,
                NumberOfEventNotes = 2,
                NumberOfCabNotes = 2,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDuplicateEvents(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }


        [TestMethod]
        public void XmlToSql_CopyTaskWithDuplicateEventsVarious()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 2,
                NumberOfEvents = 102,
                NumberOfEventInfos = 2,
                NumberOfCabs = 2,
                NumberOfEventNotes = 20,
                NumberOfCabNotes = 30,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDuplicateEvents(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_CopyTaskWithDuplicateEventsVarious()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 2,
                NumberOfFiles = 3,
                NumberOfEvents = 4,
                NumberOfEventInfos = 5,
                NumberOfCabs = 6,
                NumberOfEventNotes = 7,
                NumberOfCabNotes = 8,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskWithDuplicateEvents(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        /// <summary>
        /// Run the copy task then halt in the middle and resume to completion.
        /// </summary>
        private void runCopyTaskInterrupted(ErrorIndexType srcErrorIndexType, ErrorIndexType destErrorIndexType,
            StackHashTestIndexData testIndexData, bool switchIndexInContext)
        {
            String rootTestFolder = "c:\\stackhashunittests\\CopyUnitTests\\";
            String sourceErrorIndexFolder = rootTestFolder;
            String sourceErrorIndexName = "SourceIndex";
            String destErrorIndexFolder = rootTestFolder;
            String destErrorIndexName = "DestIndex";

            String scriptFolder = rootTestFolder + "Scripts";

            SqlCommands sqlCommands = new SqlCommands(m_ProviderFactory, s_MasterConnectionString, s_MasterConnectionString, 1);

            try
            {
                sqlCommands.DeleteDatabase(sourceErrorIndexName);
            }
            catch { };

            try
            {
                sqlCommands.DeleteDatabase(destErrorIndexName);
            }
            catch { };


            // Delete any remnants that may exist from a previous test.
            if (Directory.Exists(rootTestFolder))
            {
                PathUtils.SetFilesWritable(rootTestFolder, true);
                PathUtils.DeleteDirectory(rootTestFolder, true);
            }

            Directory.CreateDirectory(rootTestFolder);
            Directory.CreateDirectory(scriptFolder);


            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(rootTestFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = rootTestFolder;
                contextSettings.ErrorIndexSettings.Name = sourceErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = srcErrorIndexType;

                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                contextSettings.SqlSettings.ConnectionString = s_ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = "TestIndex";

                ErrorIndexSettings originalSourceIndexSettings = contextSettings.ErrorIndexSettings;
                StackHashSqlConfiguration originalSqlSettings = contextSettings.SqlSettings;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", rootTestFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Create a dummy controller to record the callbacks.
                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                SqlConnection.ClearAllPools();
                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                // Activate the context and the associated index - this will create the index if necessary.
                controllerContext.Activate();


                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Progress reports don't come through the controller context - they come straight through the contoller so create a dummy.
                Controller controller = new Controller();
                Reporter reporter = new Reporter(controller);
                controller.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                Guid guid = new Guid();
                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                ErrorIndexSettings destIndexData = new ErrorIndexSettings() { Folder = destErrorIndexFolder, Name = destErrorIndexName, Type = destErrorIndexType };



                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                    {
                        try
                        {
                            sqlCommands.DeleteDatabase(destErrorIndexName);
                        }
                        catch { };
                    }
                    sqlCommands.CreateStackHashDatabase(destErrorIndexFolder + destErrorIndexName, destErrorIndexName, false);
                }

                // Deactivate before the copy.
                controllerContext.Deactivate();

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration(s_ConnectionString, destErrorIndexName, 1, 100, 15, 100);
                controllerContext.RunCopyIndexTask(clientData, destIndexData, sqlConfig, switchIndexInContext, false);

                // Wait until at 50 % complete.
                int totalEvents = testIndexData.NumberOfProducts * testIndexData.NumberOfFiles * testIndexData.NumberOfEvents;
                waitForCopyProgress(120000, (totalEvents + 1) / 2); // Round up so that you always wait for at least 1 event if NumberOfEvents >= 1.

                controllerContext.AbortTaskOfType(StackHashTaskType.ErrorIndexCopyTask);

                // Wait for the copy task to complete.
                waitForCopyCompleted(60000 * 20);

                // Run again.
                controllerContext.RunCopyIndexTask(clientData, destIndexData, sqlConfig, switchIndexInContext, false);
                waitForCopyCompleted(60000 * 20);

                Assert.AreEqual(true, m_AdminReports.Count > 2);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[m_AdminReports.Count - 1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[m_AdminReports.Count - 1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.ErrorIndexCopyCompleted, m_AdminReports[m_AdminReports.Count - 1].Report.Operation);

                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Compare the indexes.
                IErrorIndex index1 = getIndex(originalSourceIndexSettings, originalSqlSettings);
                index1.Activate();
                IErrorIndex index2 = getIndex(destIndexData, sqlConfig);
                index2.Activate();

                compareIndexes(index1, index2, false);

                StackHashContextSettings newContextSettings = settingsManager.GetContextSettings(0);

                if (switchIndexInContext)
                {
                    Assert.AreEqual(destIndexData.Folder, newContextSettings.ErrorIndexSettings.Folder);
                    Assert.AreEqual(destIndexData.Name, newContextSettings.ErrorIndexSettings.Name);
                    Assert.AreEqual(destIndexData.Status, newContextSettings.ErrorIndexSettings.Status);
                    Assert.AreEqual(destIndexData.Type, newContextSettings.ErrorIndexSettings.Type);
                }
                else
                {
                    Assert.AreEqual(originalSourceIndexSettings.Folder, newContextSettings.ErrorIndexSettings.Folder);
                    Assert.AreEqual(originalSourceIndexSettings.Name, newContextSettings.ErrorIndexSettings.Name);
                    Assert.AreEqual(originalSourceIndexSettings.Status, newContextSettings.ErrorIndexSettings.Status);
                    Assert.AreEqual(originalSourceIndexSettings.Type, newContextSettings.ErrorIndexSettings.Type);
                }


                if (destErrorIndexType != ErrorIndexType.Xml)
                {
                    if (sqlCommands.DatabaseExists(destErrorIndexName))
                        sqlCommands.DeleteDatabase(destErrorIndexName);
                }

                SqlConnection.ClearAllPools();
                try
                {
                    controllerContext.DeleteIndex();
                }
                catch { ; }

                controllerContext.Dispose();
                SqlConnection.ClearAllPools();

                //                if (switchIndexInContext)
                {
                    index1.Deactivate();
                    index2.Deactivate();

                    try
                    {
                        index1.DeleteIndex();
                    }
                    catch { ; }

                    try
                    {
                        index2.DeleteIndex();
                    }
                    catch { ; }
                    index1.Dispose();
                    index2.Dispose();
                    SqlConnection.ClearAllPools();
                }

                int totalExpectedEvents = testIndexData.NumberOfProducts * testIndexData.NumberOfFiles * testIndexData.NumberOfEvents;

                if (totalExpectedEvents > 0)
                {
                    Assert.AreEqual(true, m_CopyIndexProgressReports.Count > 0);
                    long lastEvent = 0;
                    foreach (StackHashCopyIndexProgressAdminReport report in m_CopyIndexProgressReports)
                    {
                        Assert.AreEqual(totalExpectedEvents, report.TotalEvents);
                        Assert.AreEqual(true, report.CurrentEvent > lastEvent);
                        Assert.AreEqual(true, report.CurrentEventId != 0);
                        Assert.AreNotEqual(null, report.SourceIndexFolder);
                        Assert.AreNotEqual(null, report.SourceIndexName);
                        Assert.AreNotEqual(null, report.DestinationIndexFolder);
                        Assert.AreNotEqual(null, report.DestinationIndexName);
                    }
                }

            }
            finally
            {
                try
                {
                    sqlCommands.DeleteDatabase(sourceErrorIndexName);
                }
                catch { };

                try
                {
                    sqlCommands.DeleteDatabase(destErrorIndexName);
                }
                catch { };

                if (Directory.Exists(rootTestFolder))
                {
                    PathUtils.SetFilesWritable(rootTestFolder, true);
                    PathUtils.DeleteDirectory(rootTestFolder, true);
                }
            }
        }

        [TestMethod]
        public void XmlToSql_EmptyWithInterrupt()
        {
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

            runCopyTaskInterrupted(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_1EventWithInterrupt()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskInterrupted(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void XmlToSql_200EventWithInterrupt()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 200,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskInterrupted(ErrorIndexType.Xml, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_EmptyWithInterrupt()
        {
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

            runCopyTaskInterrupted(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_1EventWithInterrupt()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 1,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskInterrupted(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }

        [TestMethod]
        public void SqlToSql_200EventWithInterrupt()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData()
            {
                NumberOfProducts = 1,
                NumberOfFiles = 1,
                NumberOfEvents = 200,
                NumberOfEventInfos = 1,
                NumberOfCabs = 0,
                NumberOfEventNotes = 1,
                NumberOfCabNotes = 1,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            runCopyTaskInterrupted(ErrorIndexType.SqlExpress, ErrorIndexType.SqlExpress, testIndexData, false);
        }
    }
}
