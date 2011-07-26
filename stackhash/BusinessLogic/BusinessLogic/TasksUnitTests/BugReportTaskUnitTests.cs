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

using StackHashTestBugTrackerPlugIn;


namespace TasksUnitTests
{
    public enum PlugInOptions
    {
        None,
        SetBugId,
        CabUpdatedSetBugId,
        CabAddedSetBugId,
        CabNoteAddedBugId,
        DebugScriptExecutedBugId,
        ManualCabAddedSetBugId,
    } 

    /// <summary>
    /// Summary description for BugReportTaskUnitTests
    /// </summary>
    [TestClass]
    public class BugReportTaskUnitTests
    {
        private const int s_TaskTimeout = 120000;
        private String m_TempPath;
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_ServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";
        private static int s_RetryLimit = 40;
        private String m_ExpectedLastEventNote;
        private String m_ExpectedLastCabNote;
        private AutoResetEvent m_BugReportTaskCompleteEvent = new AutoResetEvent(false);

        public BugReportTaskUnitTests()
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

        /// <summary>
        /// Not called.
        /// This is just present to enforce a dependency on the TestBugTrackerPlugIn dll ensuring that 
        /// it is copied when mstest creates the temp assembly folder for running the tests.
        /// </summary>
        /// <returns>Not used.</returns>
        static NameValueCollection createDependencyOnTestPlugIn()
        {
            TestBugTrackerPlugInContext context = new TestBugTrackerPlugInContext();
            return context.ContextDiagnostics;
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
            m_BugReportTaskCompleteEvent.Reset();
            SqlConnection.ClearAllPools();
            m_TempPath = Path.GetTempPath() + "StackHashBugTrackerTests\\";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);

        }

        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminArgs = e as AdminReportEventArgs;

            if (adminArgs.Report.Operation == StackHashAdminOperation.BugReportCompleted)
                m_BugReportTaskCompleteEvent.Set();
        }

        private void waitForBugReportCompleted(int timeout)
        {
            if (!m_BugReportTaskCompleteEvent.WaitOne(timeout))
                throw new TimeoutException("Timed out waiting for cab BugReport to complete");
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

        private void checkBugIds(ControllerContext controllerContext, StackHashTestData testData, PlugInOptions options)
        {
            String expectedPrefix = getPrefix(options);

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
                        String expectedBugId = expectedPrefix + theEvent.Id.ToString();
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



        private void dumpDiagnostics(NameValueCollection diagnostics)
        {
            Console.WriteLine("ProductAddedCount=" + diagnostics["ProductAddedCount"]);
            Console.WriteLine("ProductUpdatedCount=" + diagnostics["ProductUpdatedCount"]);
            Console.WriteLine("FileAddedCount=" + diagnostics["FileAddedCount"]);
            Console.WriteLine("FileUpdatedCount=" + diagnostics["FileUpdatedCount"]);
            Console.WriteLine("EventAddedCount=" + diagnostics["EventAddedCount"]);
            Console.WriteLine("EventUpdatedCount=" + diagnostics["EventUpdatedCount"]);
            Console.WriteLine("EventCompleteCount=" + diagnostics["EventCompleteCount"]);
            Console.WriteLine("EventNoteAddedCount=" + diagnostics["EventNoteAddedCount"]);
            Console.WriteLine("CabAddedCount=" + diagnostics["CabAddedCount"]);
            Console.WriteLine("CabUpdatedCount=" + diagnostics["CabUpdatedCount"]);
            Console.WriteLine("CabNoteAddedCount=" + diagnostics["CabNoteAddedCount"]);
        }

        private void setOptions(PlugInOptions options, StackHashNameValueCollection properties)
        {
            if (options == PlugInOptions.SetBugId)
                properties.Add(new StackHashNameValuePair("SetBugId", "True"));
            if (options == PlugInOptions.CabUpdatedSetBugId)
                properties.Add(new StackHashNameValuePair("CabUpdatedSetBugId", "True"));
            if (options == PlugInOptions.CabAddedSetBugId)
                properties.Add(new StackHashNameValuePair("CabAddedSetBugId", "True"));
            if (options == PlugInOptions.CabNoteAddedBugId)
                properties.Add(new StackHashNameValuePair("CabNoteAddedBugId", "True"));
            if (options == PlugInOptions.DebugScriptExecutedBugId)
                properties.Add(new StackHashNameValuePair("DebugScriptExecutedBugId", "True"));
            if (options == PlugInOptions.ManualCabAddedSetBugId)
                properties.Add(new StackHashNameValuePair("ManualCabAddedSetBugId", "True"));
        }

        private String getPrefix(PlugInOptions options)
        {
            String prefix = "";
            if (options == PlugInOptions.SetBugId)
                prefix = "TestPlugInBugId";
            if (options == PlugInOptions.CabUpdatedSetBugId)
                prefix = "TestCabUpdatedPlugInBugId";
            if (options == PlugInOptions.CabAddedSetBugId)
                prefix = "TestCabAddedPlugInBugId";
            if (options == PlugInOptions.CabNoteAddedBugId)
                prefix = "TestCabNoteAddedPlugInBugId";
            if (options == PlugInOptions.DebugScriptExecutedBugId)
                prefix = "TestDebugScriptExecutedPlugInBugId";
            if (options == PlugInOptions.ManualCabAddedSetBugId)
                prefix = "ManualCabAddedSetBugId";

            return prefix;
        }


        /// <summary>
        /// Runs the report task.
        /// Note that when creating the test database, the BugTrackerTask will call the plugin so the 
        /// total counts will be double those expected.
        /// </summary>
        /// <param name="testData"></param>
        /// <param name="runBugReportTaskTwice"></param>
        private void runBugReportTask(StackHashTestData testData, PlugInOptions options, 
            StackHashBugReportDataCollection bugReportDataCollection, bool runBugReportTaskTwice)
        {
            int expectedProducts = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts;
            int expectedFiles = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles * expectedProducts;
            int expectedEvents = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents * expectedFiles;
            int expectedHits = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos * expectedEvents;
            int expectedCabs = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs * expectedEvents;
            int expectedCabNotes = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes * expectedCabs;
            int expectedEventNotes = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes * expectedEvents;

            int expectedProductUpdates = 0;
            if (testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents != 0)
                expectedProductUpdates = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts;
            int expectedCabUpdates = 0;


            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Console.WriteLine("DLL PATH: " + dllLocation);
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
            setOptions(options, contextSettings.BugTrackerSettings.PlugInSettings[0].Properties);

            ScriptManager scriptManager = new ScriptManager(errorIndexPath + "\\scripts");

            string licenseFileName = string.Format("{0}\\License.bin", errorIndexPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_ServiceGuid);
            licenseManager.SetLicense(s_LicenseId);

            // Create a dummy controller to record the callbacks.
            ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                settingsManager, true, null, licenseManager);

            // Hook up to receive admin reports.
            controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            try
            {
                // Get the number of automatic scripts that will be run on cabs by the analyze task.
                int expectedScriptReports = 0;


                // Delete any old index first.
                controllerContext.Activate(null, true);
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();

                // Activate the context and the associated index.
                controllerContext.Activate(null, true);


                // Wait till the bug tracker task is actually running.
                int retryCount = 0;
                while ((controllerContext.TaskController.GetTaskState(StackHashTaskType.BugTrackerTask) != StackHashTaskState.Running) &&
                       (retryCount < s_RetryLimit))
                {
                    Thread.Sleep(200);
                    retryCount++;
                }


                StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventCompleteCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);


                controllerContext.CreateTestIndex(testData.DummyWinQualSettings.ObjectsToCreate);

                // Now wait for the BugTrackerTask to complete its updates.
                bool completed = false;
                retryCount = 0;
                while (!completed && retryCount < s_RetryLimit * (expectedCabs + 1))
                {
                    // Check the plugin to make sure it was called.
                    fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                    Assert.AreEqual(1, fullDiagnostics.Count);
                    diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();
                    completed = (diagnostics["ProductAddedCount"] == expectedProducts.ToString()) &&
                        (diagnostics["ProductUpdatedCount"] == expectedProductUpdates.ToString()) &&
                        (diagnostics["FileAddedCount"] == expectedFiles.ToString()) &&
                        (diagnostics["EventAddedCount"] == expectedEvents.ToString()) &&
                        (diagnostics["EventCompleteCount"] == "0") &&
                        (diagnostics["CabAddedCount"] == expectedCabs.ToString()) &&
                        (diagnostics["CabUpdatedCount"] == expectedCabUpdates.ToString()) &&
                        (diagnostics["CabNoteAddedCount"] == expectedCabNotes.ToString()) &&
                        (diagnostics["EventNoteAddedCount"] == expectedEventNotes.ToString()) &&
                        (diagnostics["CabNoteAddedCount"] == expectedCabNotes.ToString() &&
                        (diagnostics["ScriptRunCount"] == expectedScriptReports.ToString()));

                    Thread.Sleep(1000);
                    retryCount++;
                }

                if (retryCount >= s_RetryLimit * (expectedCabs + 1))
                    dumpDiagnostics(diagnostics);

                Assert.AreEqual(true, retryCount < s_RetryLimit * (expectedCabs + 1));

                int numProducts = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts;
                int numFiles = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles;
                int numEvents = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents;
                int numCabs = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs;
                int numEventNotes = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes;
                int numCabNotes = testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes;

                if (bugReportDataCollection[0].Product != null)
                    numProducts = 1;

                if (bugReportDataCollection[0].File != null)
                    numFiles = numProducts * 1;
                else
                    numFiles = numProducts * numFiles;

                if (bugReportDataCollection[0].TheEvent != null)
                    numEvents = numProducts * numFiles * 1;
                else
                    numEvents = numFiles * numEvents;

                if (bugReportDataCollection[0].Cab != null)
                    numCabs = numEvents * 1;
                else
                    numCabs = numEvents * numCabs;

                numEventNotes = numEvents * numEventNotes;
                numCabNotes = numCabs * numCabNotes;

                int numEventCompletions = 0;

                // Reestimate the new counts.
                if (((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeAllObjects) != 0) ||
                    ((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeProducts) != 0))
                {
                    expectedProducts += numProducts;
                }

                if (((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeAllObjects) != 0) ||
                    ((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeFiles) != 0))
                {
                        expectedFiles += numFiles;
                }

                if (((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeAllObjects) != 0) ||
                    ((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeEvents) != 0))
                {
                    expectedEvents += numEvents;
                    numEventCompletions = numEvents;
                }

                if (((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeAllObjects) != 0) ||
                    ((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeCabs) != 0))
                {
                    expectedCabs += numCabs;
                }

                if (((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeAllObjects) != 0) ||
                    ((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeCabNotes) != 0))
                {
                    expectedCabNotes += numCabNotes;
                }

                if (((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeAllObjects) != 0) ||
                    ((bugReportDataCollection[0].Options & StackHashReportOptions.IncludeEventNotes) != 0))
                {
                    expectedEventNotes += numEventNotes;
                }

                StackHashClientData clientData = new StackHashClientData(Guid.NewGuid(), "TestClient", 1);
                controllerContext.RunBugReportTask(clientData, bugReportDataCollection, null);


                if (runBugReportTaskTwice)
                {
                    try
                    {
                        controllerContext.RunBugReportTask(clientData, bugReportDataCollection, null);
                        Assert.AreEqual("Shouldnt have reached here", "No");
                    }
                    catch (StackHashException ex)
                    {
                        Assert.AreEqual(StackHashServiceErrorCode.TaskAlreadyInProgress, ex.ServiceErrorCode);
                    }
                }

                waitForBugReportCompleted(60000);


                // Check the plugin to make sure it was called.
                fullDiagnostics = controllerContext.GetBugTrackerPlugInDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual(expectedProducts.ToString(), diagnostics["ProductAddedCount"]);
                Assert.AreEqual(expectedProductUpdates.ToString(), diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual(expectedFiles.ToString(), diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual(expectedEvents.ToString(), diagnostics["EventAddedCount"]);
                Assert.AreEqual(numEventCompletions.ToString(), diagnostics["EventCompleteCount"]); // Won't hit on when the index is created just during the report task.
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

                if (options != PlugInOptions.None)
                    checkBugIds(controllerContext, testData, options);
            }
            finally
            {
                // Hook up to receive admin reports.
                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                controllerContext.Deactivate();
                controllerContext.DeleteIndex();
                controllerContext.Dispose();
            }
        }

        [TestMethod]
        public void BugReportTask0Product()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 0;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2ProductDontReportProducts()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeFiles));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2ProductJustReportProducts()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeProducts));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1FileReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product2FileReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product1FileReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product2FileReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1FileDontReportFiles()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeProducts));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1FileJustReportFiles()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeFiles));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1EventReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        //[TestMethod]
        //public void BugReportTask1Product1File2EventReportAllRep()
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        if (i != 0)
        //            MyTestInitialize();
        //        BugReportTask1Product1File2EventReportAll();
        //        MyTestCleanup();
        //    }            
        //}

        [TestMethod]
        public void BugReportTask1Product1File2EventReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product2File2EventReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product2File2EventJustReportEvents()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeEvents));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product2File2EventDontReportEvents()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeProducts | StackHashReportOptions.IncludeFiles));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1EventNoteReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1EventNoteDontReportEventNotes()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeProducts | StackHashReportOptions.IncludeFiles | StackHashReportOptions.IncludeEvents));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1EventNoteJustReportEventNotes()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeEventNotes));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }
        
        [TestMethod]
        public void BugReportTask1Product1File1Event2EventNoteReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File2Event1EventNoteReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File2Event2EventNoteReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product3File4Event5EventNoteReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 3;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 4;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 5;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1CabReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event2CabReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event2CabDontReportCabs()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeProducts | StackHashReportOptions.IncludeFiles | StackHashReportOptions.IncludeEvents));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event2CabJustReportCabs()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeCabs));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        
        [TestMethod]
        public void BugReportTask1Product1File2Event1CabReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File2Event2CabReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        
        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab1CabNoteReportAll()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab2CabNoteReportAll()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }
        [TestMethod]
        public void BugReportTask1Product1File1Event2Cab1CabNoteReportAll()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event2Cab2CabNoteReportAll()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask2Product2File2Event2Cab2CabNoteReportAll()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab1CabNoteDontReportCabNotes()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeProducts));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab1CabNoteJustReportCabNotes()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeCabNotes));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        [TestMethod]
        public void BugReportRunBugReportTaskTwice()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            bugReportDataCollection.Add(new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeCabNotes));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, true);
        }

        
        
        // **********************************************
        // SPECIFIC OBJECT TESTING
        // **********************************************


        /// <summary>
        /// 1 product in database.
        /// Report that product by ID.
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product_By_Product()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 2 products in database.
        /// Report 2nd product by ID.
        /// </summary>
        [TestMethod]
        public void BugReportTask2Product_By_Product()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 2;

            bugReportDataCollection.Add(new StackHashBugReportData(product, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 2 products in database + file, event...
        /// Report 2nd product by ID.
        /// </summary>
        [TestMethod]
        public void BugReportTask2Product1Event1File1Cab_By_Product()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 2;

            bugReportDataCollection.Add(new StackHashBugReportData(product, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 2 products in database + multiple files, events etc...
        /// Report 2nd product by ID.
        /// </summary>
        [TestMethod]
        public void BugReportTask2Product2Event2File2Cab_By_Product()
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

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 2;

            bugReportDataCollection.Add(new StackHashBugReportData(product, null, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 1 products 1 file in database.
        /// Report by specific file
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1_By_File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }


        /// <summary>
        /// 1 product 2 files
        /// Report by specific file
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File2_By_File()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 1 product 2 files + 1 event and 1 cab
        /// Report by specific file
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab_By_File()
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
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 2 product 2 files + 2 event and 2 cab
        /// Report by specific file
        /// </summary>
        [TestMethod]
        public void BugReportTask2Product2File2Event2Cab_By_File()
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
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 2;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, null, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }
        /// <summary>
        /// 1 product 1 files + 1 event
        /// Report by specific event
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1Event_By_Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        
        /// <summary>
        /// 1 product 1 files + 2 event
        /// Report by specific event
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File2Event_By_Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }


        /// <summary>
        /// 1 product 1 files + 2 event + cabs
        /// Report by specific event
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File2Event1Cab_By_Event()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 1 product 1 files + 1 event + 1 cab
        /// Report by specific cab
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab_By_Cab()
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
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";
            StackHashCab cab = new StackHashCab();
            cab.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, cab, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 1 product 1 files + 1 event + 2 cab
        /// Report by specific cab
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1Event2Cab_By_Cab()
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
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";
            StackHashCab cab = new StackHashCab();
            cab.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, cab, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 1 product 1 files + 2 event + 1 cab
        /// Report by specific cab
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File2Event1Cab_By_Cab()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 1;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";
            StackHashCab cab = new StackHashCab();
            cab.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, cab, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }

        /// <summary>
        /// 2 product 2 files + 2 event + 2 cab
        /// Report by specific cab
        /// </summary>
        [TestMethod]
        public void BugReportTask2Product2File2Event2Cab_By_Cab()
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
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 2;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";
            StackHashCab cab = new StackHashCab();
            cab.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, cab, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.None, bugReportDataCollection, false);
        }


        /// <summary>
        /// Check that the bug ref is updated for CabAdded.
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1Event1Cab_By_Cab_BugRefUpdate()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 0;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";
            StackHashCab cab = new StackHashCab();
            cab.Id = 1;

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, cab, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.ManualCabAddedSetBugId, bugReportDataCollection, false);
        }

        /// <summary>
        /// Check that the bug ref is updated for CabAdded if 2 cabs.
        /// </summary>
        [TestMethod]
        public void BugReportTask1Product1File1Event2Cabs_By_Event_BugRefUpdateInCabFunction()
        {
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 1;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 2;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabNotes = 0;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventNotes = 0;

            StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
            StackHashProduct product = new StackHashProduct();
            product.Id = 1;
            StackHashFile file = new StackHashFile();
            file.Id = 1;
            StackHashEvent theEvent = new StackHashEvent();
            theEvent.Id = 1;
            theEvent.EventTypeName = "Crash 32bit";

            bugReportDataCollection.Add(new StackHashBugReportData(product, file, theEvent, null, null, StackHashReportOptions.IncludeAllObjects));

            runBugReportTask(testData, PlugInOptions.ManualCabAddedSetBugId, bugReportDataCollection, false);
        }

    }
}
