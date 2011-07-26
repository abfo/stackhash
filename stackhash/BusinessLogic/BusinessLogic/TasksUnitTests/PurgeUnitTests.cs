using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Data.SqlClient;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashDebug;
using StackHashUtilities;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for PurgeUnitTests
    /// </summary>
    [TestClass]
    public class PurgeUnitTests
    {
        List<AdminReportEventArgs> m_AdminReports = new List<AdminReportEventArgs>();
        AutoResetEvent m_PurgeCompletedEvent = new AutoResetEvent(false);
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_TestServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";

        public PurgeUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
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
        //[ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) 
        // {
        // }

        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        //public static void MyClassCleanup() 
        //{
 
        //}

        [TestInitialize()]
        public void MyTestInitialize() 
        {
            m_AdminReports.Clear();
            m_PurgeCompletedEvent.Reset();
        }
        
         [TestCleanup()]
         public void MyTestCleanup() { }
        
        #endregion


        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminArgs = e as AdminReportEventArgs;
            if (adminArgs.Report.Operation != StackHashAdminOperation.ContextStateChanged)
                m_AdminReports.Add(adminArgs);

            if (adminArgs.Report.Operation == StackHashAdminOperation.PurgeCompleted)
                m_PurgeCompletedEvent.Set();        
        }


        private void waitForPurgeCompleted(int timeout)
        {
            m_PurgeCompletedEvent.WaitOne(timeout);
        }


        public void runPurgeTaskNoFilesInIndex(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }
            
            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);
            ControllerContext controllerContext = null;

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                contextSettings.ErrorIndexSettings.Type = indexType;

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = 1;
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);

            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskNoFilesInIndex()
        {
            runPurgeTaskNoFilesInIndex(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskNoFilesInIndexSql()
        {
            runPurgeTaskNoFilesInIndex(ErrorIndexType.SqlExpress);
        }



        
        public void runPurgeTaskIndexNotEmptyNoFilesPurged(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = 1;
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();


                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 1;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);
            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexNotEmptyNoFilesPurged()
        {
            runPurgeTaskIndexNotEmptyNoFilesPurged(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexNotEmptyNoFilesPurgedSql()
        {
            runPurgeTaskIndexNotEmptyNoFilesPurged(ErrorIndexType.SqlExpress);
        }



        public void runPurgeTaskIndexGlobalPurgeAll(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;
                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = 0; // Should purge all.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);


                // Delete any old index that might be lying around.
                controllerContext.DeleteIndex();
                
                // Activate the context and the associated index.
                controllerContext.Activate();


                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 2;

                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                Assert.AreEqual(10, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(cabFileSize * 10, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);

            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurgeAll()
        {
            runPurgeTaskIndexGlobalPurgeAll(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurgeAllSql()
        {
            runPurgeTaskIndexGlobalPurgeAll(ErrorIndexType.SqlExpress);
        }



        public void runPurgeTaskIndexGlobalPurge1CabInEachEvent(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);
            ControllerContext controllerContext = null;

            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 2;

                
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;
                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = testIndexData.NumberOfCabs - 1; 
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();



                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                Assert.AreEqual(testIndexData.NumberOfEvents * 1, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(cabFileSize * testIndexData.NumberOfEvents * 1, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);


            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }

                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge1CabInEachEvent()
        {
            runPurgeTaskIndexGlobalPurge1CabInEachEvent(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge1CabInEachEventSql()
        {
            runPurgeTaskIndexGlobalPurge1CabInEachEvent(ErrorIndexType.SqlExpress);
        }


        public void runPurgeTaskIndexGlobalPurge3CabInEachEvent(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);
            ControllerContext controllerContext = null;

            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 5;


                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = testIndexData.NumberOfCabs - 3; // Should 3 out of 5.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);


                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();



                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                // Should purge 3 out of 5 cabs in each event.
                Assert.AreEqual(testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(cabFileSize * testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);

            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }


        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3CabInEachEvent()
        {
            runPurgeTaskIndexGlobalPurge3CabInEachEvent(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3CabInEachEventSql()
        {
            runPurgeTaskIndexGlobalPurge3CabInEachEvent(ErrorIndexType.SqlExpress);
        }


        public void runPurgeTaskIndexGlobalPurge3CabAndDumpsInEachEvent(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;

            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 5;
                testIndexData.UnwrapCabs = true;


                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = testIndexData.NumberOfCabs - 3; // Should 3 out of 5.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();



                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;
                int dumpFileSize = 107979;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                // Should purge 3 out of 5 cabs in each event.
                Assert.AreEqual(testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(cabFileSize * testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(dumpFileSize * testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);

            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3CabAndDumpsInEachEvent()
        {
            runPurgeTaskIndexGlobalPurge3CabAndDumpsInEachEvent(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3CabAndDumpsInEachEventSql()
        {
            runPurgeTaskIndexGlobalPurge3CabAndDumpsInEachEvent(ErrorIndexType.SqlExpress);
        }

        public void runPurgeTaskIndexGlobalPurge3CabOnlyInEachEvent(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;

            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 5;
                testIndexData.UnwrapCabs = true;


                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = testIndexData.NumberOfCabs - 3; // Should 3 out of 5.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = false;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();



                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;
//                int dumpFileSize = 107979;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                // Should purge 3 out of 5 cabs in each event.
                Assert.AreEqual(testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(cabFileSize * testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);
            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3CabOnlyInEachEvent()
        {
            runPurgeTaskIndexGlobalPurge3CabOnlyInEachEvent(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3CabOnlyInEachEventSql()
        {
            runPurgeTaskIndexGlobalPurge3CabOnlyInEachEvent(ErrorIndexType.SqlExpress);
        }


        public void runPurgeTaskIndexGlobalPurge3DumpsOnlyInEachEvent(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;

            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 5;
                testIndexData.UnwrapCabs = true;


                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = testIndexData.NumberOfCabs - 3; // Should 3 out of 5.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = false;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;


                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);


                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();



                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                //int cabFileSize = 38496;
                int dumpFileSize = 107979;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                // Should purge 3 out of 5 cabs in each event.
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(0, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(dumpFileSize * testIndexData.NumberOfEvents * 3, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);

            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3DumpsOnlyInEachEvent()
        {
            runPurgeTaskIndexGlobalPurge3DumpsOnlyInEachEvent(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalPurge3DumpsOnlyInEachEventSql()
        {
            runPurgeTaskIndexGlobalPurge3DumpsOnlyInEachEvent(ErrorIndexType.SqlExpress);
        }


        public void runPurgeTaskIndexGlobalProductOveride(ErrorIndexType indexType)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;

            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 2;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 5;
                testIndexData.UnwrapCabs = true;


                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = 0; // Purge ALL.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;

                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[1].AgeToPurge = testIndexData.NumberOfCabs - 3; // Should 3 out of 5.
                contextSettings.PurgeOptionsCollection[1].PurgeObject = StackHashPurgeObject.PurgeProduct;
                contextSettings.PurgeOptionsCollection[1].Id = 1;
                contextSettings.PurgeOptionsCollection[1].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[1].PurgeDumpFiles = true;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);


                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();



                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;
                int dumpFileSize = 107979;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                // Check that the correct purge statistics are returned.
                StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                // Should purge 3 out of 5 cabs in each event.
                int expectedCabsPurged = (testIndexData.NumberOfProducts - 1) * (testIndexData.NumberOfEvents * testIndexData.NumberOfCabs) +
                    (testIndexData.NumberOfEvents * 3);

                Assert.AreEqual(expectedCabsPurged, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                Assert.AreEqual(cabFileSize * expectedCabsPurged, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                Assert.AreEqual(expectedCabsPurged, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                Assert.AreEqual(dumpFileSize * expectedCabsPurged, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);


            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalProductOveride()
        {
            runPurgeTaskIndexGlobalProductOveride(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalProductOverideSql()
        {
            runPurgeTaskIndexGlobalProductOveride(ErrorIndexType.SqlExpress);
        }


        public void runPurgeTaskIndexGlobalProductAndEventOveride(ErrorIndexType indexType, bool includeEmailNotification)
        {
            String errorIndexFolder = "c:\\stackhasunittests\\StackHashPurgeTests";
            String errorIndexName = "TestIndex";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            ControllerContext controllerContext = null;


            try
            {
                // Create an index with some files.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 2;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 5;
                testIndexData.NumberOfCabs = 5;
                testIndexData.UnwrapCabs = true;


                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                // Set up the purge schedule to purge all files older that 1 day - globally.              
                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();

                Schedule purgeSchedule = new Schedule();
                purgeSchedule.Period = SchedulePeriod.Hourly;
                purgeSchedule.DaysOfWeek = DaysOfWeek.All;
                purgeSchedule.Time = new ScheduleTime(1, 0, 0);

                contextSettings.CabFilePurgeSchedule.Add(purgeSchedule);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;
                contextSettings.ErrorIndexSettings.Type = indexType;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = 0; // Purge ALL.
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].Id = 0;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;

                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[1].AgeToPurge = testIndexData.NumberOfCabs - 3; // Should 3 out of 5.
                contextSettings.PurgeOptionsCollection[1].PurgeObject = StackHashPurgeObject.PurgeProduct;
                contextSettings.PurgeOptionsCollection[1].Id = 1;
                contextSettings.PurgeOptionsCollection[1].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[1].PurgeDumpFiles = true;

                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[2].AgeToPurge = 0; // All
                contextSettings.PurgeOptionsCollection[2].PurgeObject = StackHashPurgeObject.PurgeEvent;
                contextSettings.PurgeOptionsCollection[2].Id = 4;
                contextSettings.PurgeOptionsCollection[2].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[2].PurgeDumpFiles = true;


                if (includeEmailNotification)
                {
                    contextSettings.EmailSettings = new StackHashEmailSettings();
                    contextSettings.EmailSettings.OperationsToReport = new StackHashAdminOperationCollection();
                    contextSettings.EmailSettings.OperationsToReport.Add(StackHashAdminOperation.PurgeCompleted);
                    contextSettings.EmailSettings.SmtpSettings.SmtpHost = TestSettings.SmtpHost;
                    contextSettings.EmailSettings.SmtpSettings.SmtpPort = TestSettings.SmtpPort;
                    contextSettings.EmailSettings.SmtpSettings.SmtpUsername = TestSettings.TestEmail1;

                    String password = TestSettings.TestEmail1Password;

                    contextSettings.EmailSettings.SmtpSettings.SmtpPassword = password;
                    contextSettings.EmailSettings.SmtpSettings.SmtpFrom = TestSettings.TestEmail1;
                    contextSettings.EmailSettings.SmtpSettings.SmtpRecipients = TestSettings.TestEmail1 + "," + TestSettings.TestEmail2;
                }

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                BugTrackerManager bugTrackerManager = new BugTrackerManager(new String[0]);

                // Create a dummy controller to record the callbacks.
                controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, null, licenseManager);

                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate();

                testIndexData.CabFileName = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";
                int cabFileSize = 38496;
                int dumpFileSize = 107979;

                controllerContext.CreateTestIndex(testIndexData);

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the purge task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);

                controllerContext.SetPurgeOptions(contextSettings.CabFilePurgeSchedule, contextSettings.PurgeOptionsCollection, true);
                controllerContext.RunPurgeTask(clientData);

                // Wait for the purge task to complete.
                waitForPurgeCompleted(300000);

                try
                {
                    Assert.AreEqual(2, m_AdminReports.Count);
                    Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                    Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                    Assert.AreEqual(StackHashAdminOperation.PurgeStarted, m_AdminReports[0].Report.Operation);

                    Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                    Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                    Assert.AreEqual(StackHashAdminOperation.PurgeCompleted, m_AdminReports[1].Report.Operation);

                    // Check that the correct purge statistics are returned.
                    StackHashPurgeCompleteAdminReport purgeAdminReport = m_AdminReports[1].Report as StackHashPurgeCompleteAdminReport;

                    // Should purge 3 out of 5 cabs in each event.
                    int expectedCabsPurged = (testIndexData.NumberOfProducts - 1) * (testIndexData.NumberOfEvents * testIndexData.NumberOfCabs) +
                        ((testIndexData.NumberOfEvents - 1) * 3) + (testIndexData.NumberOfCabs);

                    Assert.AreEqual(expectedCabsPurged, purgeAdminReport.PurgeStatistics.NumberOfCabs);
                    Assert.AreEqual(cabFileSize * expectedCabsPurged, purgeAdminReport.PurgeStatistics.CabsTotalSize);
                    Assert.AreEqual(expectedCabsPurged, purgeAdminReport.PurgeStatistics.NumberOfDumpFiles);
                    Assert.AreEqual(dumpFileSize * expectedCabsPurged, purgeAdminReport.PurgeStatistics.DumpFilesTotalSize);
                }
                finally
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                    controllerContext = null;
                }
            }
            finally
            {
                if (controllerContext != null)
                {
                    SqlConnection.ClearAllPools();
                    controllerContext.Deactivate();
                    controllerContext.DeleteIndex();
                    controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                    controllerContext.Dispose();
                }
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalProductAndEventOveride()
        {
            runPurgeTaskIndexGlobalProductAndEventOveride(ErrorIndexType.Xml, false);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalProductAndEventOverideSql()
        {
            runPurgeTaskIndexGlobalProductAndEventOveride(ErrorIndexType.SqlExpress, false);
        }

        [TestMethod]
        public void RunPurgeTaskIndexGlobalProductAndEventOverideSqlWithEmailNotification()
        {
            runPurgeTaskIndexGlobalProductAndEventOveride(ErrorIndexType.SqlExpress, true);
        }
    }
}
