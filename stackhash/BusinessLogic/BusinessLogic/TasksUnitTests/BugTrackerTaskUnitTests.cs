using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;
using System.Threading;
using System.Data.SqlClient;

using StackHashBusinessObjects;
using StackHashTasks;
using StackHashUtilities;
using StackHashErrorIndex;
using StackHashDebug;
using StackHashBugTrackerInterfaceV1;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for BugTrackerTaskUnitTests
    /// </summary>
    [TestClass]
    public class BugTrackerTaskUnitTests
    {
        private const int s_TaskTimeout = 120000;
        private String m_TempPath;
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_ServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";
        private static int s_RetryLimit = 20;
        private String m_ExpectedLastEventNote;
        private String m_ExpectedLastCabNote;
        private List<AdminReportEventArgs> m_AdminReports = new List<AdminReportEventArgs>();
        private AutoResetEvent m_AdminEvent = new AutoResetEvent(false);
        private AutoResetEvent m_AnalyzeCompletedEvent = new AutoResetEvent(false);


        public BugTrackerTaskUnitTests()
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_AdminReports.Clear();
            m_AdminEvent.Reset();

            SqlConnection.ClearAllPools();
            m_TempPath = Path.GetTempPath() + "StackHashBugTrackerTests\\";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);        
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
                // Mark scripts as not readonly.
                PathUtils.MarkDirectoryWritable(m_TempPath, true);
                PathUtils.DeleteDirectory(m_TempPath, true);
            }
            SqlConnection.ClearAllPools();
        }

        [TestMethod]
        public void NoUpdateTableEntries()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            
            String errorIndexName = "BugTrackerErrorIndex";
            String errorIndexPath = m_TempPath;

            // Create a settings manager and a new context.
            SettingsManager settingsManager = new SettingsManager(errorIndexPath + "\\ServiceSettings.XML");
            StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

            contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            contextSettings.ErrorIndexSettings.Folder = errorIndexPath;
            contextSettings.ErrorIndexSettings.Name = errorIndexName;
            contextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            contextSettings.SqlSettings.InitialCatalog = errorIndexName;

            contextSettings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
            contextSettings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            contextSettings.BugTrackerSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            contextSettings.BugTrackerSettings.PlugInSettings[0].Name = "TestPlugIn";
            contextSettings.BugTrackerSettings.PlugInSettings[0].Enabled = true;


            ScriptManager scriptManager = new ScriptManager(errorIndexPath + "\\scripts");

            string licenseFileName = string.Format("{0}\\License.bin", errorIndexPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            // Create a dummy controller to record the callbacks.
            ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                settingsManager, true, null, licenseManager);

            try
            {
                // Delete any old index first.
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate(null, true);

                StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);


                // Wait till the task is actually running.
                int retryCount = 0;
                while ((controllerContext.TaskController.GetTaskState(StackHashTaskType.BugTrackerTask) != StackHashTaskState.Running) &&
                       (retryCount < 20))
                {
                    Thread.Sleep(200);
                    retryCount++;
                }

                Assert.AreEqual(true, retryCount < 20);

                // Ping it to process the Update Table.
                controllerContext.RunBugTrackerTask();

                // Now abort it.
                controllerContext.AbortTaskOfType(StackHashTaskType.BugTrackerTask, true); // And wait till it completes.
            }
            finally
            {
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();
                controllerContext.Dispose();
            }
        }

        private void addEventNotes(ControllerContext controllerContext, StackHashTestData testData, BugTrackerManager plugInManager)
        {
            StackHashTestIndexData indexData = testData.DummyWinQualSettings.ObjectsToCreate;

            StackHashProductInfoCollection products = controllerContext.GetProducts();

            foreach (StackHashProductInfo product in products)
            {
                StackHashFileCollection files = controllerContext.GetFiles(product.Product);

                foreach (StackHashFile file in files)
                {
                    StackHashEventCollection allEvents = controllerContext.GetEvents(product.Product, file);

                    foreach (StackHashEvent theEvent in allEvents)
                    {
                        for (int noteCount = 0; noteCount < indexData.NumberOfEventNotes; noteCount++)
                        {
                            StackHashNoteEntry noteEntry = new StackHashNoteEntry();
                            noteEntry.Source = "StackHashClient";
                            noteEntry.User = "MarkJ";
                            noteEntry.Note = "Event Note " + theEvent.Id.ToString() + " " + noteCount.ToString();
                            controllerContext.AddEventNote(product.Product, file, theEvent, noteEntry);

                            m_ExpectedLastEventNote = noteEntry.Note;
                        }
                    }
                }
            }
        }

        private void checkBugIds(ControllerContext controllerContext, StackHashTestData testData)
        {
            StackHashTestIndexData indexData = testData.DummyWinQualSettings.ObjectsToCreate;

            StackHashProductInfoCollection products = controllerContext.GetProducts();

            foreach (StackHashProductInfo product in products)
            {
                StackHashFileCollection files = controllerContext.GetFiles(product.Product);

                foreach (StackHashFile file in files)
                {
                    StackHashEventCollection allEvents = controllerContext.GetEvents(product.Product, file);

                    foreach (StackHashEvent theEvent in allEvents)
                    {
                        String expectedBugId = "TestPlugInBugId" + theEvent.Id.ToString();
                        Assert.AreEqual(expectedBugId, theEvent.PlugInBugId);
                    }
                }
            }
        }

        
        private void addCabNotes(ControllerContext controllerContext, StackHashTestData testData, BugTrackerManager plugInManager)
        {
            StackHashTestIndexData indexData = testData.DummyWinQualSettings.ObjectsToCreate;

            StackHashProductInfoCollection products = controllerContext.GetProducts();

            foreach (StackHashProductInfo product in products)
            {
                StackHashFileCollection files = controllerContext.GetFiles(product.Product);

                foreach (StackHashFile file in files)
                {
                    StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
                    {
                        new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Product.Id, 0),
                        new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file.Id, 0),
                    };

                    StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
                    {
                        new StackHashSearchCriteria(options)                    
                    };

                    StackHashEventPackageCollection allEvents = controllerContext.GetEvents(allCriteria);

                    foreach (StackHashEventPackage theEvent in allEvents)
                    {
                        foreach (StackHashCabPackage cab in theEvent.Cabs)
                        {
                            for (int noteCount = 0; noteCount < indexData.NumberOfCabNotes; noteCount++)
                            {
                                StackHashNoteEntry noteEntry = new StackHashNoteEntry();
                                noteEntry.Source = "StackHashClient";
                                noteEntry.User = "MarkJ";
                                noteEntry.Note = "Cab Note " + theEvent.EventData.Id.ToString() + " " + noteCount.ToString();
                                controllerContext.AddCabNote(product.Product, file, theEvent.EventData, cab.Cab, noteEntry);

                                m_ExpectedLastCabNote = noteEntry.Note;
                            }
                        }
                    }
                }
            }
        }

        private void waitForAnalyzeCompleted(int timeout)
        {
            if (timeout > 60000 * 5)
                timeout = 60000 * 5;

            m_AnalyzeCompletedEvent.WaitOne(timeout);
        }


        private void runBugTrackerTask(StackHashTestData testData, bool changeBugId)
        {
            int expectedProducts = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts;


            int expectedFiles = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles * expectedProducts;
            int expectedEvents = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents * expectedFiles;
            int expectedHits = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos * expectedEvents;
            int expectedCabs = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs * expectedEvents;
            int expectedCabNotes = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes * expectedCabs;
            int expectedEventNotes = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes * expectedEvents;

            int expectedProductUpdates = 0;
            if (expectedEvents > 0)
                expectedProductUpdates = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts; // Should get updated at the end of the sync.

            int expectedCabUpdates = 0;
            if (expectedCabs > 0)
                expectedCabUpdates = expectedCabs; // Should get updated as each cab is processed.

            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);


            String errorIndexName = "BugTrackerErrorIndex";
            String errorIndexPath = m_TempPath;

            // Create a settings manager and a new context.
            SettingsManager settingsManager = new SettingsManager(errorIndexPath + "\\ServiceSettings.XML");
            StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

            contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            contextSettings.ErrorIndexSettings.Folder = errorIndexPath;
            contextSettings.ErrorIndexSettings.Name = errorIndexName;
            contextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            contextSettings.SqlSettings.InitialCatalog = errorIndexName;

            // Enable product sync for all the expected products - otherwise the winqual sync task won't add them.
            // Product ids start at 1 in the Dummy Winqual Sync task.
            contextSettings.WinQualSettings.ProductsToSynchronize = new StackHashProductSyncDataCollection();
            for (int i = 0; i < expectedProducts; i++)
            {
                contextSettings.WinQualSettings.ProductsToSynchronize.Add(new StackHashProductSyncData(i + 1));
            }
                
            contextSettings.WinQualSettings.ProductsToSynchronize.Add(new StackHashProductSyncData(1));

            contextSettings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
            contextSettings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            contextSettings.BugTrackerSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            contextSettings.BugTrackerSettings.PlugInSettings[0].Enabled = true;
            contextSettings.BugTrackerSettings.PlugInSettings[0].Name = "TestPlugIn";
            contextSettings.BugTrackerSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            contextSettings.BugTrackerSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair("SetBugId", changeBugId.ToString()));

            ScriptManager scriptManager = new ScriptManager(errorIndexPath + "\\scripts");

            string licenseFileName = string.Format("{0}\\License.bin", errorIndexPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            // Create a dummy controller to record the callbacks.
            ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                settingsManager, true, null, licenseManager);

            controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            try
            {
                // Get the number of automatic scripts that will be run on cabs by the analyze task.
                int numAutoScripts = scriptManager.AutoScripts.Count;
                int expectedScriptReports = numAutoScripts * expectedCabs;


                // Delete any old index first.
                controllerContext.Activate(null, true);
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate(null, true);


                StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);



                // Wait till the task is actually running.
                int retryCount = 0;
                while ((controllerContext.TaskController.GetTaskState(StackHashTaskType.BugTrackerTask) != StackHashTaskState.Running) &&
                       (retryCount < s_RetryLimit))
                {
                    Thread.Sleep(200);
                    retryCount++;
                }

                Assert.AreEqual(true, retryCount < s_RetryLimit);

                controllerContext.SetTestData(testData);

                // Note that this runs the Sync + Analyse task but only waits for the sync task to complete.
                controllerContext.RunSynchronizeTask(null, false, true, false, null, true, false);

                // Wait for the analyze task to complete.
                // Debugger should only take a second but sometimes if another program is running it might take as long as 90 seconds.
                // See 1026.
                waitForAnalyzeCompleted(60000 * 2 * expectedCabs); 

                addEventNotes(controllerContext, testData, manager);
                addCabNotes(controllerContext, testData, manager);

                bool completed = false;
                retryCount = 0;
                int retryLimit = s_RetryLimit * (expectedCabs + 1);
                while (!completed && retryCount < retryLimit)
                {
                    // Check the plugin to make sure it was called.
                    fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                    Assert.AreEqual(1, fullDiagnostics.Count);
                    diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();
                    completed = (diagnostics["ProductAddedCount"] == expectedProducts.ToString()) &&
                        (diagnostics["ProductUpdatedCount"] == expectedProductUpdates.ToString()) &&
                        (diagnostics["FileAddedCount"] == expectedFiles.ToString()) &&
                        (diagnostics["EventAddedCount"] == expectedEvents.ToString()) &&
                        (diagnostics["CabAddedCount"] == expectedCabs.ToString()) &&
                        (diagnostics["CabUpdatedCount"] == expectedCabUpdates.ToString()) &&
                        (diagnostics["CabNoteAddedCount"] == expectedCabNotes.ToString()) &&
                        (diagnostics["EventNoteAddedCount"] == expectedEventNotes.ToString()) &&
                        (diagnostics["CabNoteAddedCount"] == expectedCabNotes.ToString() &&
                        (diagnostics["ScriptRunCount"] == expectedScriptReports.ToString()));

                    Thread.Sleep(1000);
                    retryCount++;
                }
                Console.WriteLine("Retry Limit = " + retryLimit.ToString());
                Console.WriteLine("Retry Count = " + retryCount.ToString());

                foreach (String key in diagnostics.Keys)
                {
                    Console.WriteLine(key + "=" + diagnostics[key]);
                }

                Assert.AreEqual(true, retryCount < s_RetryLimit * (expectedCabs + 1));

                // Check the plugin to make sure it was called.
                fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual(expectedProducts.ToString(), diagnostics["ProductAddedCount"]);
                Assert.AreEqual(expectedProductUpdates.ToString(), diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual(expectedFiles.ToString(), diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual(expectedEvents.ToString(), diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual(expectedEventNotes.ToString(), diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual(expectedCabs.ToString(), diagnostics["CabAddedCount"]);
                Assert.AreEqual(expectedCabUpdates.ToString(), diagnostics["CabUpdatedCount"]);
                Assert.AreEqual(expectedCabNotes.ToString(), diagnostics["CabNoteAddedCount"]);
                Assert.AreEqual(expectedScriptReports.ToString(), diagnostics["ScriptRunCount"]);

                if (m_ExpectedLastEventNote != null)
                    Assert.AreEqual(m_ExpectedLastEventNote, diagnostics["LastEventNote"]);
                if (m_ExpectedLastCabNote != null)
                    Assert.AreEqual(m_ExpectedLastCabNote, diagnostics["LastCabNote"]);

                if (changeBugId)
                    checkBugIds(controllerContext, testData);
            }
            finally
            {
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();
                controllerContext.Dispose();
            }
        }

        [TestMethod]
        public void BugTrackerTask1NewProduct()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask2NewProducts()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask10NewProducts()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 10;

            runBugTrackerTask(testData, false);
        }


        [TestMethod]
        public void BugTrackerTask1Product1File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask2Product1File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask10Product1File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 10;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product2File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product10File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 10;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask10Product10File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 10;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 10;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;

            runBugTrackerTask(testData, false);
        }


        /// <summary>
        /// Here, we inform the test plug-in to return a new BugId and 
        /// we check that the Event has been updated in the database with this 
        /// new bug id.
        /// In this case no bug Id is present in the Event to start with - i.e. it is null.
        /// </summary>
        [TestMethod]
        public void BugTrackerTask1Product1File1EventSpecifyBugId()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;

            runBugTrackerTask(testData, true);
        }


        /// <summary>
        /// Here, we inform the test plug-in to return a new BugId and 
        /// we check that the Event has been updated in the database with this 
        /// new bug id.
        /// In this case a bug id IS present in the Event to start with so it should be replaced.
        /// </summary>
        [TestMethod]
        public void BugTrackerTask1Product1File1EventSpecifyBugIdReplace()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.SetBugId = true;  // Creates a database with events that have a bug id set.

            runBugTrackerTask(testData, true);
        }


        /// <summary>
        /// Here, we inform the test plug-in to return a new BugId and 
        /// we check that the Event has been updated in the database with this 
        /// new bug id.
        /// In this case a bug id IS present in the Event to start with so it should be replaced.
        /// The bug ID should be passed in to further calls.
        /// </summary>
        [TestMethod]
        public void BugTrackerTask1Product1File1Event1CabSpecifyBugIdReplace()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.SetBugId = true;  // Creates a database with events that have a bug id set.

            runBugTrackerTask(testData, true);
        }
        
        [TestMethod]
        public void BugTrackerTask1Product1File2Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File10Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 10;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask2Product2File2Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event1EventNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event2EventNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask2Product2File2Event2EventNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 2;

            runBugTrackerTask(testData, false);
        }


        [TestMethod]
        public void BugTrackerTask1Product1File1Event1Cab()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.UnwrapCabs = true;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event1CabSetBugRef()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.UnwrapCabs = true;

            runBugTrackerTask(testData, true);
        }


        [TestMethod]
        public void BugTrackerTask1Product1File1Event2Cab()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask2Product2File2Event2Cab()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;

            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event1Cab1CabNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 1;
            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event1Cab2CabNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 2;
            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask1Product1File1Event2Cab1CabNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 1;
            runBugTrackerTask(testData, false);
        }

        [TestMethod]
        public void BugTrackerTask2Product2File2Event2Cab2CabNote()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 2;
            runBugTrackerTask(testData, false);
        }


        /// <summary>
        /// Grand mix of all objects.
        /// </summary>
        [TestMethod]
        public void BugTrackerTaskMixOfObjects()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 3;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 4;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 5;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 6;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 7;
            runBugTrackerTask(testData, false);
        }

        
        /// <summary>
        /// Sync adds a new product.
        /// A resync should not call the plugin because the product data has not changed.
        /// </summary>
        [TestMethod]
        public void BugTrackerTask1NewProductResyncSameProduct()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            String errorIndexName = "BugTrackerErrorIndex";
            String errorIndexPath = m_TempPath;

            // Create a settings manager and a new context.
            SettingsManager settingsManager = new SettingsManager(errorIndexPath + "\\ServiceSettings.XML");
            StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

            contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            contextSettings.ErrorIndexSettings.Folder = errorIndexPath;
            contextSettings.ErrorIndexSettings.Name = errorIndexName;
            contextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            contextSettings.SqlSettings.InitialCatalog = errorIndexName;

            contextSettings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
            contextSettings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            contextSettings.BugTrackerSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            contextSettings.BugTrackerSettings.PlugInSettings[0].Name = "TestPlugIn";
            contextSettings.BugTrackerSettings.PlugInSettings[0].Enabled = true;

            ScriptManager scriptManager = new ScriptManager(errorIndexPath + "\\scripts");

            string licenseFileName = string.Format("{0}\\License.bin", errorIndexPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            // Create a dummy controller to record the callbacks.
            ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                settingsManager, true, null, licenseManager);

            try
            {
                // Delete any old index first.
                controllerContext.Activate(null, true);
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate(null, true);

                StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);



                // Wait till the task is actually running.
                int retryCount = 0;
                while ((controllerContext.TaskController.GetTaskState(StackHashTaskType.BugTrackerTask) != StackHashTaskState.Running) &&
                       (retryCount < s_RetryLimit))
                {
                    Thread.Sleep(200);
                    retryCount++;
                }

                Assert.AreEqual(true, retryCount < s_RetryLimit);

                // Add a product to the database using the WinQualSync task.
                StackHashTestData testData = new StackHashTestData();
                testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
                testData.DummyWinQualSettings.UseDummyWinQual = true;
                testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
                testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
                controllerContext.SetTestData(testData);

                // Sync.
                controllerContext.RunSynchronizeTask(null, false, true, false, null, true, false);


                bool completed = false;
                retryCount = 0;
                while (!completed && retryCount < s_RetryLimit)
                {
                    // Check the plugin to make sure it was called.
                    fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                    Assert.AreEqual(1, fullDiagnostics.Count);
                    diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();
                    completed = diagnostics["ProductAddedCount"] == "1";

                    Thread.Sleep(200);
                    retryCount++;
                }

                Assert.AreEqual(true, retryCount < s_RetryLimit);

                // Check the plugin to make sure it was called.
                fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("1", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

                // Sync again.
                controllerContext.RunSynchronizeTask(null, false, true, false, null, true, false);

                // Give it a chance to run.
                Thread.Sleep(1000);

                // Abort the bug tracker task. 
                controllerContext.AbortTaskOfType(StackHashTaskType.BugTrackerTask, true);

                // Check the plugin to make sure it was called.
                fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("1", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);
            }
            finally
            {
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();
                controllerContext.Dispose();
            }
        }

        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminArgs = e as AdminReportEventArgs;

            if (adminArgs.Report.Operation == StackHashAdminOperation.BugTrackerPlugInStatus)
            {
                m_AdminReports.Add(adminArgs);
                m_AdminEvent.Set();
            }
            else if (adminArgs.Report.Operation == StackHashAdminOperation.AnalyzeCompleted)
            {
                m_AnalyzeCompletedEvent.Set();
            }
        }

        private void waitForBugStatusAdminEvent(int timeout)
        {
            m_AdminEvent.WaitOne(timeout);
        }


        /// <summary>
        /// A plugin error should cause the BugTrackerTask to stop reporting but not remove the update table entry.
        /// An admin report should also be sent to the clients.
        /// </summary>
        [TestMethod]
        public void BugTrackerTaskPlugInError()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            String errorIndexName = "BugTrackerErrorIndex";
            String errorIndexPath = m_TempPath;

            // Create a settings manager and a new context.
            SettingsManager settingsManager = new SettingsManager(errorIndexPath + "\\ServiceSettings.XML");
            StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

            contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            contextSettings.ErrorIndexSettings.Folder = errorIndexPath;
            contextSettings.ErrorIndexSettings.Name = errorIndexName;
            contextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            contextSettings.SqlSettings.InitialCatalog = errorIndexName;

            contextSettings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
            contextSettings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            contextSettings.BugTrackerSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            contextSettings.BugTrackerSettings.PlugInSettings[0].Name = "TestPlugIn";
            contextSettings.BugTrackerSettings.PlugInSettings[0].Enabled = true;
            contextSettings.BugTrackerSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            contextSettings.BugTrackerSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair("ProductAddedException", "1"));

            ScriptManager scriptManager = new ScriptManager(errorIndexPath + "\\scripts");

            string licenseFileName = string.Format("{0}\\License.bin", errorIndexPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            DummyController controller = new DummyController();
            Reporter newReporter = new Reporter(controller);

            controller.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            // Create a dummy controller to record the callbacks.
            ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                settingsManager, true, null, licenseManager);

            try
            {
                // Delete any old index first.
                controllerContext.Activate(null, true);
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();


                // Activate the context and the associated index.
                controllerContext.Activate(null, true);


                StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

                // Wait till the task is actually running.
                int retryCount = 0;
                while ((controllerContext.TaskController.GetTaskState(StackHashTaskType.BugTrackerTask) != StackHashTaskState.Running) &&
                       (retryCount < s_RetryLimit))
                {
                    Thread.Sleep(200);
                    retryCount++;
                }

                Assert.AreEqual(true, retryCount < s_RetryLimit);

                // Add a product to the database using the WinQualSync task.
                StackHashTestData testData = new StackHashTestData();
                testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
                testData.DummyWinQualSettings.UseDummyWinQual = true;
                testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
                testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 5;
                controllerContext.SetTestData(testData);

                // Sync.
                controllerContext.RunSynchronizeTask(null, false, true, false, null, true, false);


                bool completed = false;
                retryCount = 0;
                while (!completed && retryCount < s_RetryLimit)
                {
                    // Check the plugin to make sure it was called.
                    fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                    Assert.AreEqual(1, fullDiagnostics.Count);
                    diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();
                    completed = diagnostics["ProductAddedCount"] == "1";

                    Thread.Sleep(200);
                    retryCount++;
                }


                waitForBugStatusAdminEvent(10000);

                // Give the BugTrackerTask a chance to process all messages.
                Thread.Sleep(1000);

                Assert.AreEqual(1, m_AdminReports.Count);
                StackHashBugTrackerStatusAdminReport statusReport = m_AdminReports[0].Report as StackHashBugTrackerStatusAdminReport;
                Assert.AreEqual(1, statusReport.PlugInDiagnostics.Count);


                Assert.AreEqual(true, retryCount < s_RetryLimit);

                // Check the plugin to make sure it was called just for the processed product.
                fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("1", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

            }
            finally
            {
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();
                controllerContext.Dispose();
            }
        }
    }
}
