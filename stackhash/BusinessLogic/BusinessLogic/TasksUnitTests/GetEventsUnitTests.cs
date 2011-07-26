using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlClient;

using StackHashBusinessObjects;
using StackHashTasks;
using StackHashUtilities;
using StackHashErrorIndex;


namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for GetEventsUnitTests
    /// </summary>
    [TestClass]
    public class GetEventsUnitTests
    {
        private String m_IndexPath;
        private String m_SettingsFileName;
        private Controller m_Controller;


        public GetEventsUnitTests()
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();
            m_IndexPath = "c:\\stackhashunittests";
            m_SettingsFileName = "c:\\stackhashunittests\\TestSettings.xml";

            TidyTest();

            if (!Directory.Exists(m_IndexPath))
                Directory.CreateDirectory(m_IndexPath);

        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (m_Controller != null)
            {
                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }

            if (Directory.Exists(m_IndexPath))
            {
                // Mark scripts as not readonly.
                PathUtils.MarkDirectoryWritable(m_IndexPath, true);
                PathUtils.DeleteDirectory(m_IndexPath, true);

                if (File.Exists(m_SettingsFileName))
                    File.Delete(m_SettingsFileName);
            }
            SqlConnection.ClearAllPools();
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
        /// Search the index for events based on specified criteria.
        /// </summary>
        /// <param name="testIndexData">Test data for the index.</param>
        private StackHashEventPackageCollection getEvents(StackHashTestIndexData testIndexData, StackHashSearchCriteriaCollection searchCriteria, 
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder, StackHashSearchDirection direction, int maxRowsToGetPerRequest)
        {
            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);
            if (maxRowsToGetPerRequest > 0)
                m_Controller.SetMaxRowsToGetPerRequest(0, maxRowsToGetPerRequest);

            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                m_Controller.CreateTestIndex(0, testIndexData);

                // Enable the products otherwise they won't show up in the searches.
                StackHashProductInfoCollection products = m_Controller.GetProducts(0);
                foreach (StackHashProductInfo product in products)
                {
                    StackHashProductSyncData syncData = new StackHashProductSyncData();
                    syncData.ProductId = product.Product.Id;
                    m_Controller.SetProductSyncData(0, syncData);
                }

                return m_Controller.GetEvents(0, searchCriteria, startRow, numberOfRows, sortOrder, direction, startRow == 1);
            }
            finally
            {
                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }
        }

        
        /// <summary>
        /// Events in index: 1
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1
        /// </summary>
        [TestMethod]
        public void GetEvents_1Event_WindowOf1_SearchOnBugId_1Match()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            
            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(1, events.TotalRows);
            Assert.AreEqual(1, events.MaximumRowNumber);
            Assert.AreEqual(1, events.MinimumRowNumber);
        }

        /// <summary>
        /// Events in index: 1
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1
        /// </summary>
        [TestMethod]
        public void GetEvents_1Event_WindowOf1_SearchOnBugId_1Match_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(1, events.TotalRows);
            Assert.AreEqual(1, events.MaximumRowNumber);
            Assert.AreEqual(1, events.MinimumRowNumber);
        }

        
        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1 - for first window.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Events_WindowOf1_SearchOnBugId_1Match_FirstWindow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(1, events.MaximumRowNumber);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1 - for first window.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Events_WindowOf1_SearchOnBugId_1Match_FirstWindow_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(1, events.MaximumRowNumber);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }
        
        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1 - for second window.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Events_WindowOf1_SearchOnBugId_1Match_SecondWindow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 2;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
            Assert.AreEqual(2, events.TotalRows);
            Assert.AreEqual(2, events.MaximumRowNumber);
            Assert.AreEqual(2, events.MinimumRowNumber);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1 - for first window.
        /// Backward search.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Events_WindowOf1_SearchOnBugId_1Match_FirstWindow_BackwardSearch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = Int32.MaxValue;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
            Assert.AreEqual(2, events.MaximumRowNumber);
            Assert.AreEqual(2, events.MinimumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }


        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1 - for second window.
        /// Backward search.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Events_WindowOf1_SearchOnBugId_1Match_SecondWindow_BackwardSearch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 2 - 1; // first window returns minrow of 2 - see previous test.
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(1, events.MaximumRowNumber);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }


        /// <summary>
        /// Events in index: 1
        /// Window size: 1
        /// Search on: Script
        /// No script data.
        /// Matching Events Expected: 0
        /// </summary>
        [TestMethod]
        public void GetEvents_1Event_WindowOf1_SearchOnScript_0Match_NoScriptData()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Hello", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(0, events.Count);
        }


        /// <summary>
        /// Events in index: 1
        /// Window size: 1
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 0
        /// </summary>
        [TestMethod]
        public void GetEvents_1Event_WindowOf1_SearchOnScript_0Match_1Script()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Hello", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(0, events.Count);
        }

        /// <summary>
        /// Events in index: 1
        /// Window size: 1
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 1
        /// </summary>
        [TestMethod]
        public void GetEvents_1Event_WindowOf1_SearchOnScript_1Match_1Script()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB1", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
        }


        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 1 - second.
        /// 
        /// This should test getting an event where the code needs to try again to get more.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf1_SearchOnScript_SecondEventMatch_1Script()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 2 - both.
        /// MaxEventsPerBlock = default.
        /// 
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf2_SearchOnScript_BothEventsMatch_1Script()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(2, events[1].EventData.Id);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 2 - both.
        /// MaxEventsPerBlock = 1.
        /// Descending EventId order.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf2_SearchOnScript_BothEventsMatch_1Script_1EventPerBlock_Desc()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
            Assert.AreEqual(1, events[1].EventData.Id);
        }


        
        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 1 - second.
        /// 1 event per block.
        /// 
        /// This should test getting an event where the code needs to try again to get more.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf1_SearchOnScript_SecondEventMatch_1Script_1EventPerBlock()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
        }
        
        /// <summary>
        /// Events in index: 2
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 1 - first.
        /// 
        /// This should test getting an event where the code needs to try again to get more - but runs out of events before
        /// the required number of rows are retrieved.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf2_SearchOnScript_SecondEventMatch_1Script_NotEnoughEvents()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 1 - first.
        /// DESC on EventId
        /// 
        /// This should test getting an event where the code needs to try again to get more - but runs out of events before
        /// the required number of rows are retrieved.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf2_SearchOnScript_SecondEventMatch_1Script_NotEnoughEvents_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 1 - first.
        /// 
        /// This should test getting an event where the code needs to try again to get more - but runs out of events before
        /// the required number of rows are retrieved.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf2_SearchOnScript_SecondEventMatch_1Script_NotEnoughEvents_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
        }


        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// 
        /// Events separated - 3 Script Criteria.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(10, events[1].EventData.Id);

            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(10, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// Descending EventId
        /// 
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// 
        /// Events separated - 3 Script Criteria.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB17", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(17, events[0].EventData.Id);
            Assert.AreEqual(10, events[1].EventData.Id);

            Assert.AreEqual(4, events.MinimumRowNumber);
            Assert.AreEqual(11, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(10, events[1].EventData.Id);

            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(10, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 2 empty criteria.
        /// </summary>
        [TestMethod]
        public void GetEvents_EmptyCriteria()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(3, events[0].CriteriaMatchMap);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(3, events[1].CriteriaMatchMap);

            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(2, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 2 empty criteria.
        /// Desc on Event ID
        /// </summary>
        [TestMethod]
        public void GetEvents_EmptyCriteria_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(20, events[0].EventData.Id);
            Assert.AreEqual(3, events[0].CriteriaMatchMap);
            Assert.AreEqual(19, events[1].EventData.Id);
            Assert.AreEqual(3, events[1].CriteriaMatchMap);

            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(2, events.MaximumRowNumber);
        }

        
        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 2 empty criteria.
        /// </summary>
        [TestMethod]
        public void GetEvents_EmptyCriteria_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(3, events[0].CriteriaMatchMap);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(3, events[1].CriteriaMatchMap);

            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(2, events.MaximumRowNumber);
        }

        
        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 2 search criteria with scripts.
        /// </summary>
        [TestMethod]
        public void GetEvents_2ScriptCriteria()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(3, events[0].CriteriaMatchMap);
            Assert.AreEqual(1, events[0].Cabs.Count);
            Assert.AreEqual(10, events[1].EventData.Id);
            Assert.AreEqual(3, events[1].CriteriaMatchMap);
            Assert.AreEqual(1, events[1].Cabs.Count);

            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(10, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 1 search criteria with scripts and productId
        /// </summary>
        [TestMethod]
        public void GetEvents_1ScriptCriteria_ProductIdMatch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(1, events[0].CriteriaMatchMap);
            Assert.AreEqual(1, events[0].Cabs.Count);

            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 1 search criteria with scripts and productId
        /// Descending EventId
        /// </summary>
        [TestMethod]
        public void GetEvents_1ScriptCriteria_ProductIdMatch_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(1, events[0].CriteriaMatchMap);
            Assert.AreEqual(1, events[0].Cabs.Count);

            Assert.AreEqual(18, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
        }


        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 1 search criteria with scripts - 1 cab - 2 scripts - first matches.
        /// 1 event match.
        /// </summary>
        [TestMethod]
        public void GetEvents_1ScriptCriteria2Scripts_FirstScriptMatches()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 2;


            // Event   Cab   Script.
            // 1       1     0
            // 1       1     1
            // 1       2     0
            // 1       2     1
            // 2       3     0
            // 2       3     1
            // 2       4     0
            // 2       4     1
            // 3       5     0
            // 3       5     1
            // 3       6     0
            // 3       6     1
            // etc...
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script0_CAB3.log", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id); 
            Assert.AreEqual(1, events[0].CriteriaMatchMap); // Should match 
            Assert.AreEqual(1, events[0].Cabs.Count);
            Assert.AreEqual(true, events[0].Cabs[0].IsSearchMatch);
            
            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 1 search criteria with scripts - 1 cab - 2 scripts - second matches.
        /// 1 event match.
        /// </summary>
        [TestMethod]
        public void GetEvents_1ScriptCriteria2Scripts_SecondScriptMatches()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 2;


            // Event   Cab   Script.
            // 1       1     0
            // 1       1     1
            // 1       2     0
            // 1       2     1
            // 2       3     0
            // 2       3     1
            // 2       4     0
            // 2       4     1
            // 3       5     0
            // 3       5     1
            // 3       6     0
            // 3       6     1
            // etc...
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script1_CAB3.log", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(1, events[0].CriteriaMatchMap); // Should match 
            Assert.AreEqual(1, events[0].Cabs.Count);
            Assert.AreEqual(true, events[0].Cabs[0].IsSearchMatch);

            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 1 search criteria with scripts - 1 cab - 2 scripts - second matches.
        /// 1 event match.
        /// Descending on EventId.
        /// </summary>
        [TestMethod]
        public void GetEvents_1ScriptCriteria2Scripts_SecondScriptMatches_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 2;


            // Event   Cab   Script.
            // 1       1     0
            // 1       1     1
            // 1       2     0
            // 1       2     1
            // 2       3     0
            // 2       3     1
            // 2       4     0
            // 2       4     1
            // 3       5     0
            // 3       5     1
            // 3       6     0
            // 3       6     1
            // etc...
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script1_CAB3.log", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);
            Assert.AreEqual(1, events[0].CriteriaMatchMap); // Should match 
            Assert.AreEqual(1, events[0].Cabs.Count);
            Assert.AreEqual(true, events[0].Cabs[0].IsSearchMatch);

            Assert.AreEqual(18, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
        }

        
        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// 2 search criteria with scripts - 2 cabs - only 1 cab matches in each set.
        /// </summary>
        [TestMethod]
        public void GetEvents_2ScriptCriteria2Scripts()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 2;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 2;


            // Event   Cab   Script.
            // 1       1     0
            // 1       1     1
            // 1       2     0
            // 1       2     1
            // 2       3     0
            // 2       3     1
            // 2       4     0
            // 2       4     1
            // 3       5     0
            // 3       5     1
            // 3       6     0
            // 3       6     1
            // etc...
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.LessThan, 4, 0),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script0", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.GreaterThan, 3, 0),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script1", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id); // CABID3 belongs to Event ID 2
            Assert.AreEqual(3, events[0].CriteriaMatchMap);  
            Assert.AreEqual(2, events[0].Cabs.Count);
            Assert.AreEqual(true, events[0].Cabs[0].IsSearchMatch);
            Assert.AreEqual(false, events[0].Cabs[1].IsSearchMatch);

            Assert.AreEqual(5, events[1].EventData.Id); // CABID10 belongs to Event ID 5
            Assert.AreEqual(2, events[1].CriteriaMatchMap);
            Assert.AreEqual(2, events[1].Cabs.Count);
            Assert.AreEqual(false, events[1].Cabs[0].IsSearchMatch);
            Assert.AreEqual(true, events[1].Cabs[1].IsSearchMatch);

            Assert.AreEqual(2, events.MinimumRowNumber);
            Assert.AreEqual(5, events.MaximumRowNumber);
        }

        
        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// This should test getting an event where the code needs to try again to get more - first window was ok (previous test).
        /// Now use the values 3 (Min) 10 (Max) to get the next window.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SecondWindow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 10 + 1; // Previous window max - see previous test.
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(18, events[0].EventData.Id);

            Assert.AreEqual(18, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// This should test getting an event where the code needs to try again to get more - first window was ok (previous test).
        /// Now use the values 3 (Min) 10 (Max) to get the next window.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SecondWindow_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 10 + 1; // Previous window max - see previous test.
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(18, events[0].EventData.Id);

            Assert.AreEqual(18, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// REVERSE ORDER
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_ReverseOrder()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));  // Reverse ID order.

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(18, events[0].EventData.Id);
            Assert.AreEqual(10, events[1].EventData.Id);

            // The events are in reverse order so 20 will be row 1, 19 row 2 etc... so the expected rows for 18 and 10 will be 3 and 11.
            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(11, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// REVERSE ORDER
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_ReverseOrder_MaxRowsPerRequest()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));  // Reverse ID order.

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 1);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(18, events[0].EventData.Id);
            Assert.AreEqual(10, events[1].EventData.Id);

            // The events are in reverse order so 20 will be row 1, 19 row 2 etc... so the expected rows for 18 and 10 will be 3 and 11.
            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(11, events.MaximumRowNumber);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// SEARCH BACKWARD.
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SearchBackwards()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));  

            long startRow = Int32.MaxValue; // Something big.
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(10, events[0].EventData.Id);
            Assert.AreEqual(18, events[1].EventData.Id);

            // The events are in reverse order so 20 will be row 1, 19 row 2 etc... so the expected rows for 18 and 10 will be 3 and 11.
            Assert.AreEqual(10, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// SEARCH BACKWARD.
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SearchBackwards_MaxRowsPerRequest()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));  // Reverse ID order.

            long startRow = Int32.MaxValue; // Something big.
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 1);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(10, events[0].EventData.Id);
            Assert.AreEqual(18, events[1].EventData.Id);

            // The events are in reverse order so 20 will be row 1, 19 row 2 etc... so the expected rows for 18 and 10 will be 3 and 11.
            Assert.AreEqual(10, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// START ROW = TOO BIG - should reduce it to Int32.MaxValue.
        /// 
        /// SEARCH BACKWARD.
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SearchBackwards_StartRowTooBig()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));  // Reverse ID order.

            long startRow = Int64.MaxValue; // Something big.
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(10, events[0].EventData.Id);
            Assert.AreEqual(18, events[1].EventData.Id);

            // The events are in reverse order so 20 will be row 1, 19 row 2 etc... so the expected rows for 18 and 10 will be 3 and 11.
            Assert.AreEqual(10, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// START ROW = TOO BIG - should reduce it to Int32.MaxValue.
        /// 
        /// SEARCH BACKWARD.
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SearchBackwards_StartRowTooBig_MaxRowsPerRequest1()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));  // Reverse ID order.

            long startRow = Int64.MaxValue; // Something big.
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 1);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(10, events[0].EventData.Id);
            Assert.AreEqual(18, events[1].EventData.Id);

            // The events are in reverse order so 20 will be row 1, 19 row 2 etc... so the expected rows for 18 and 10 will be 3 and 11.
            Assert.AreEqual(10, events.MinimumRowNumber);
            Assert.AreEqual(18, events.MaximumRowNumber);
            Assert.AreEqual(2, events.TotalRows);
        }

        /// <summary>
        /// Events in index: 20 (Id 1,2,3,4,5,6,7,8,9,10,11...20). 
        /// Window size: 2
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 3.
        /// 
        /// SEARCH BACKWARD - second window
        /// This should test getting an event where the code needs to try again to get more - but there are still more events to be retrieved.
        /// The min and max row numbers should be set up appropriately.
        /// </summary>
        [TestMethod]
        public void GetEvents_20Event_WindowOf2_SearchOnScript_3EventMatch_1Script_2GetsRequired_SearchBackwards_SecondWindow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 20;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB10", null, true),
            };
            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB18", null, true),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));  

            long numberOfRows = 2;
            long startRow = 10 - numberOfRows; // This is the last window (see prev test) - window size.

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(3, events[0].EventData.Id);

            Assert.AreEqual(3, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            Assert.AreEqual(1, events.TotalRows); // Only expecting 1 more window.
        }



        /// <summary>
        /// Search the index for events based on specified criteria.
        /// </summary>
        /// <param name="testIndexData">Test data for the index.</param>
        private long countEvents(StackHashTestIndexData testIndexData, StackHashSearchCriteriaCollection searchCriteria,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOrder, StackHashSearchDirection direction)
        {
            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);


            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                m_Controller.CreateTestIndex(0, testIndexData);

                // Enable the products otherwise they won't show up in the searches.
                StackHashProductInfoCollection products = m_Controller.GetProducts(0);
                foreach (StackHashProductInfo product in products)
                {
                    StackHashProductSyncData syncData = new StackHashProductSyncData();
                    syncData.ProductId = product.Product.Id;
                    m_Controller.SetProductSyncData(0, syncData);
                }

                return m_Controller.CountEvents(0, searchCriteria, startRow, numberOfRows, sortOrder, direction);
            }
            finally
            {
                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }
        }


        /// <summary>
        /// Events in index: 1
        /// Window size: 1
        /// Search on: BugId
        /// Events expected: 1
        /// </summary>
        [TestMethod]
        public void CountEvents_1Event_WindowOf1_SearchOnBugId_1Match()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "Bug", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 1;

            long numEvents = countEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards);

            Assert.AreEqual(1, numEvents);
        }

        /// <summary>
        /// Events in index: 2
        /// Window size: 1
        /// Search on: Script
        /// One script for cab.
        /// Matching Events Expected: 2 in total - 1 per window.
        /// Total events should NOT be set because we are getting the second window.
        /// </summary>
        [TestMethod]
        public void GetEvents_2Event_WindowOf1_SearchOnScript_1Match_1Script_SecondWindow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 2;
            long numberOfRows = 1;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(2, events[0].EventData.Id);
            Assert.AreEqual(2, events.MinimumRowNumber);
            Assert.AreEqual(2, events.MaximumRowNumber);
            Assert.AreEqual(1, events.TotalRows);
        }

        /// <summary>
        /// Rows 1, 5 and 6 match of 6 events.
        /// Window 3 backwards starting at row 2.
        /// </summary>
        [TestMethod]
        public void GetEvents_4Event_WindowOf3_SearchOnScript_3Match_1Script_BackwardsWithGap()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 6;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB1", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB5", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB6", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 4;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(5, events[1].EventData.Id);
            Assert.AreEqual(6, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(6, events.MaximumRowNumber);
//            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Row 1,2,3 - Match
        /// Window 3 backwards starting at row 2.
        /// Should return 1,2,3.
        /// </summary>
        [TestMethod]
        public void GetEvents_3Event_WindowOf3_SearchOnScript_3Match_1Script_BackwardsAtStartOfRowOverlappingEndOfRow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 3;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB1", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 2;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(3, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Row 1,2,3 - Match
        /// Window 3 backwards starting at row 2.
        /// Should return 1,2,3.
        /// 10 events in total.
        /// </summary>
        [TestMethod]
        public void GetEvents_10Event_WindowOf3_SearchOnScript_3Match_1Script_BackwardsAtStartOfRow()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 10;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB1", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB3", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 2;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(3, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Row 1,2,8 - Match
        /// Window 13 backwards starting at row 8.
        /// Should return 1,2,3.
        /// Request more than maxRowsToGetPerRequest events.
        /// </summary>
        [TestMethod]
        public void GetEvents_10Event_WindowOf3_SearchOnScript_3Match_1Script_BackwardsAtStartOfRow_MaxRowsPerRequestExceeded()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 10;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = false ;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB1.log", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB2.log", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB8.log", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 8;
            int maxRowsToGetPerRequest = 3;
            long numberOfRows = maxRowsToGetPerRequest + 10;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, maxRowsToGetPerRequest);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(8, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(8, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        
        /// <summary>
        /// No cabs - script contains "Fred" OR EventId > 0.
        /// Should return all events.
        /// </summary>
        [TestMethod]
        public void GetEvents_6Event_WindowOf10_SearchOnScriptOrEventId_AllMatch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 6;
            testIndexData.NumberOfCabs = 0;
            testIndexData.UnwrapCabs = false;
            testIndexData.NumberOfScriptResults = 0;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB1", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.GreaterThan, 0, 0)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);


            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 0;
            long numberOfRows = 10;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(3, events[2].EventData.Id);
            Assert.AreEqual(4, events[3].EventData.Id);
            Assert.AreEqual(5, events[4].EventData.Id);
            Assert.AreEqual(6, events[5].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(6, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Row 4,5,6 - Match
        /// Window 3 backwards starting at row 2.
        /// Should return none.
        /// 10 events in total.
        /// Descending.
        /// </summary>
        [TestMethod]
        public void GetEvents_10Event_WindowOf3_SearchOnScript_3Match_1Script_Backwards_StartRow2_Descending_NoEvents()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 10;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB4", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB5", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB6", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 2;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(0, events.Count);
        }

        /// <summary>
        /// Row 4,5,6 - Match
        /// Window 3 backwards starting at row 3.
        /// Should return event 6.
        /// 10 events in total.
        /// Descending.
        /// </summary>
        [TestMethod]
        public void GetEvents_10Event_WindowOf3_SearchOnScript_3Match_1Script_Backwards_StartRow3_Descending_1Events()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 10;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB4", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB5", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB6", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 3;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(6, events[0].EventData.Id);
            Assert.AreEqual(5, events.MinimumRowNumber);
            Assert.AreEqual(5, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Row 4,5,6 - Match
        /// Window 3 backwards starting at row 8.
        /// Should return event 4,5,6.
        /// 10 events in total.
        /// Descending.
        /// </summary>
        [TestMethod]
        public void GetEvents_10Event_WindowOf3_SearchOnScript_3Match_1Script_Backwards_StartRow8_Descending_3Events()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 10;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB4", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB5", null, true)
            };
            StackHashSearchCriteria criteria2 = new StackHashSearchCriteria(options2);

            StackHashSearchOptionCollection options3 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB6", null, true)
            };
            StackHashSearchCriteria criteria3 = new StackHashSearchCriteria(options3);

            searchCriteriaCollection.Add(criteria1);
            searchCriteriaCollection.Add(criteria2);
            searchCriteriaCollection.Add(criteria3);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 8;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Backwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(6, events[0].EventData.Id);
            Assert.AreEqual(5, events[1].EventData.Id);
            Assert.AreEqual(4, events[2].EventData.Id);
            Assert.AreEqual(5, events.MinimumRowNumber);
            Assert.AreEqual(7, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// </summary>
        [TestMethod]
        public void Fog1352()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(1, events[0].EventData.Id);
            Assert.AreEqual(2, events[1].EventData.Id);
            Assert.AreEqual(3, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// Descending
        /// </summary>
        [TestMethod]
        public void Fog1352_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(5, events[0].EventData.Id);
            Assert.AreEqual(4, events[1].EventData.Id);
            Assert.AreEqual(3, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }


        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// More than 1 product.
        /// </summary>
        [TestMethod]
        public void Fog1352_10Products()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 10;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 2, 1)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", true));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(6, events[0].EventData.Id);
            Assert.AreEqual(7, events[1].EventData.Id);
            Assert.AreEqual(8, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// More than 1 product.
        /// Descending
        /// </summary>
        [TestMethod]
        public void Fog1352_10Products_Descending()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 10;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 2, 1)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "Id", false));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(10, events[0].EventData.Id);
            Assert.AreEqual(9, events[1].EventData.Id);
            Assert.AreEqual(8, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }

        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// More than 1 product.
        /// Show events with 1 or more cabs.
        /// </summary>
        [TestMethod]
        public void Fog1352_10Products_EventsWithMoreThan1Cab()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 10;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 2, 1),
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(6, events[0].EventData.Id);
            Assert.AreEqual(7, events[1].EventData.Id);
            Assert.AreEqual(8, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }


        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// More than 1 product.
        /// Show events with 1 or more cabs.
        /// </summary>
        [TestMethod]
        public void Fog1352_10Products_EventsWithMoreThan1Cab_2EventsPerBlock()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 10;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 2, 1),
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 2);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(6, events[0].EventData.Id);
            Assert.AreEqual(7, events[1].EventData.Id);
            Assert.AreEqual(8, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }


        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// More than 1 product.
        /// Show events with 1 or more cabs.
        /// Every second event has cabs.
        /// </summary>
        [TestMethod]
        public void Fog1352_10Products_Every2ndEventHasCabs_First2()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 10;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.EventsToAssignCabs = 2;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 3, 1),
//                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            long startRow = 1;
            long numberOfRows = 2;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(11, events[0].EventData.Id);
            Assert.AreEqual(13, events[1].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(3, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }


        /// <summary>
        /// Fog1352.
        /// 5 events in a product - 1 cab and 1 script in each.
        /// Window of 3 - all events match - should return the first 3 only.
        /// More than 1 product.
        /// Show events with 1 or more cabs.
        /// Every second event has cabs.
        /// </summary>
        [TestMethod]
        public void Fog1352_10Products_Every2ndEventHasCabs_All3()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 10;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 5;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UnwrapCabs = true;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.EventsToAssignCabs = 2;

            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "_CAB", null, true),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 3, 1),
//                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            long startRow = 1;
            long numberOfRows = 3;

            StackHashEventPackageCollection events = getEvents(testIndexData, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, 0);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(11, events[0].EventData.Id);
            Assert.AreEqual(13, events[1].EventData.Id);
            Assert.AreEqual(15, events[2].EventData.Id);
            Assert.AreEqual(1, events.MinimumRowNumber);
            Assert.AreEqual(5, events.MaximumRowNumber);
            //            Assert.AreEqual(3, events.TotalRows);
        }


        /// <summary>
        /// Fog1352, 1476.
        /// Duplicate event in middle of page.
        /// Windows = 3.
        /// File1 - Event1.
        /// File1 - Event2.
        /// File2 - Event1.
        /// File2 - Event3.
        /// 
        /// Should return Ev1, Ev2, Ev3.
        /// The bug is that it was just returning 2 events. Ev1, Ev2 because the SQL was returned Ev1, Ev2, Ev2 and
        /// removing the duplicate in higher level code.
        /// </summary>
        [TestMethod]
        public void Fog1352_1476_DuplicateEvents_MiddleOfPage()
        {
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);

            int maxRowsToGetPerRequest = 0;
            if (maxRowsToGetPerRequest > 0)
                m_Controller.SetMaxRowsToGetPerRequest(0, maxRowsToGetPerRequest);

            StackHashEventPackageCollection events = null;

            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                // Add the events.
                IErrorIndex index = m_Controller.FindContext(0).ErrorIndex;

                StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, "www.link", 1, "Product1", 3, 0, "1.2.3.4");
                index.AddProduct(product1);
                StackHashProductSyncData syncData = new StackHashProductSyncData();
                syncData.ProductId = product1.Id;
                m_Controller.SetProductSyncData(0, syncData);

                StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 100, DateTime.Now, "File1", "1.2.3.4");
                StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 101, DateTime.Now, "File2", "1.2.3.4");
                index.AddFile(product1, file1);
                index.AddFile(product1, file2);

                StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 1, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 2, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event3 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 3, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);

                index.AddEvent(product1, file1, event1);
                index.AddEvent(product1, file1, event2);
                index.AddEvent(product1, file2, event1); 
                index.AddEvent(product1, file2, event3);

                long startRow = 1;
                long numberOfRows = 3;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(3, events.Count);
                Assert.AreEqual(1, events[0].EventData.Id);
                Assert.AreEqual(2, events[1].EventData.Id);
                Assert.AreEqual(3, events[2].EventData.Id);
                Assert.AreEqual(1, events.MinimumRowNumber);
                Assert.AreEqual(3, events.MaximumRowNumber);
                Assert.AreEqual(3, events.TotalRows);
            }
            finally
            {
                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }

        }


        /// <summary>
        /// Fog1352, 1476.
        /// Duplicate event at end of page. Next page should not start with the same event.
        /// Windows = 3.
        /// File1 - Event1.
        /// File1 - Event2.
        /// File1 - Event3.
        /// File2 - Event3.
        /// File1 - Event4.
        /// 
        /// Should return Ev1, Ev2, Ev3 for page 1 and Ev4 for page2.
        /// With the bug, Event3 is returned as 2 separate rows so appears on the next page too.
        /// </summary>
        [TestMethod]
        public void Fog1352_1476_DuplicateEvents_EndOfPage()
        {
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);

            int maxRowsToGetPerRequest = 0;
            if (maxRowsToGetPerRequest > 0)
                m_Controller.SetMaxRowsToGetPerRequest(0, maxRowsToGetPerRequest);

            StackHashEventPackageCollection events = null;

            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                // Add the events.
                IErrorIndex index = m_Controller.FindContext(0).ErrorIndex;

                StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, "www.link", 1, "Product1", 3, 0, "1.2.3.4");
                index.AddProduct(product1);
                StackHashProductSyncData syncData = new StackHashProductSyncData();
                syncData.ProductId = product1.Id;
                m_Controller.SetProductSyncData(0, syncData);

                StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 100, DateTime.Now, "File1", "1.2.3.4");
                StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 101, DateTime.Now, "File2", "1.2.3.4");
                index.AddFile(product1, file1);
                index.AddFile(product1, file2);

                StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 1, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 2, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event3 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 3, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event4 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 4, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);

                index.AddEvent(product1, file1, event1);
                index.AddEvent(product1, file1, event2);
                index.AddEvent(product1, file1, event3);
                index.AddEvent(product1, file2, event3);
                index.AddEvent(product1, file2, event4);

                long startRow = 1;
                long numberOfRows = 3;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(3, events.Count);
                Assert.AreEqual(1, events[0].EventData.Id);
                Assert.AreEqual(2, events[1].EventData.Id);
                Assert.AreEqual(3, events[2].EventData.Id);
                Assert.AreEqual(1, events.MinimumRowNumber);
                Assert.AreEqual(3, events.MaximumRowNumber);
                Assert.AreEqual(4, events.TotalRows);

                startRow = events.MaximumRowNumber + 1;
                numberOfRows = 3;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(1, events.Count);
                Assert.AreEqual(4, events[0].EventData.Id);
                Assert.AreEqual(4, events.MinimumRowNumber);
                Assert.AreEqual(4, events.MaximumRowNumber);
            }
            finally
            {
                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }

        }

        /// <summary>
        /// Fog1352, 1476.
        /// Duplicate event at start of 2nd page. Prev page should not end with the same event.
        /// Get first page Forwards and second page Backwards.
        /// Windows = 2.
        /// File1 - Event1. (Row1)
        /// File1 - Event2. (Row2)
        /// File1 - Event3. (Row3)
        /// File2 - Event3. 
        /// File1 - Event4. (Row4)
        /// 
        /// Request page starting row 3. Row 3 contains a duplicate event.
        /// Should return Ev3, Ev4 for page 1 and Ev1, Ev2 (window size = 2).
        /// </summary>
        [TestMethod]
        public void Fog1352_1476_DuplicateEvents_StartOfPage_FirstPageForwards()
        {
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);

            int maxRowsToGetPerRequest = 0;
            if (maxRowsToGetPerRequest > 0)
                m_Controller.SetMaxRowsToGetPerRequest(0, maxRowsToGetPerRequest);

            StackHashEventPackageCollection events = null;

            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                // Add the events.
                IErrorIndex index = m_Controller.FindContext(0).ErrorIndex;

                StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, "www.link", 1, "Product1", 3, 0, "1.2.3.4");
                index.AddProduct(product1);
                StackHashProductSyncData syncData = new StackHashProductSyncData();
                syncData.ProductId = product1.Id;
                m_Controller.SetProductSyncData(0, syncData);

                StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 100, DateTime.Now, "File1", "1.2.3.4");
                StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 101, DateTime.Now, "File2", "1.2.3.4");
                index.AddFile(product1, file1);
                index.AddFile(product1, file2);

                StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 1, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 2, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event3 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 3, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event4 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 4, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);

                index.AddEvent(product1, file1, event1);
                index.AddEvent(product1, file1, event2);
                index.AddEvent(product1, file1, event3);
                index.AddEvent(product1, file2, event3);
                index.AddEvent(product1, file2, event4);

                long startRow = 3;
                long numberOfRows = 2;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(2, events.Count);
                Assert.AreEqual(3, events[0].EventData.Id);
                Assert.AreEqual(4, events[1].EventData.Id);
                Assert.AreEqual(3, events.MinimumRowNumber);
                Assert.AreEqual(4, events.MaximumRowNumber);
                Assert.AreEqual(4, events.TotalRows);

                numberOfRows = 2;
                startRow = events.MinimumRowNumber - numberOfRows;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(2, events.Count);
                Assert.AreEqual(1, events[0].EventData.Id);
                Assert.AreEqual(2, events[1].EventData.Id);
                Assert.AreEqual(1, events.MinimumRowNumber);
                Assert.AreEqual(2, events.MaximumRowNumber);
            }
            finally
            {
                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }

        }


        /// <summary>
        /// Fog1352, 1476.
        /// Script search.
        /// Duplicate event in middle of page.
        /// Windows = 3.
        /// File1 - Event1.
        /// File1 - Event2 - script match.
        /// File2 - Event1.
        /// File2 - Event3.
        /// File2 - Event4 - script match.
        /// File2 - Event5 - script match.
        /// File2 - Event6 - script match.
        /// 
        /// Should return first 3 matches even though there is a duplicate event.
        /// 
        /// </summary>
        [TestMethod]
        public void Fog1352_1476_ScriptSearchCount_DuplicateEvents_MiddleOfPage1()
        {
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script", null, false),
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);

            int maxRowsToGetPerRequest = 0;
            if (maxRowsToGetPerRequest > 0)
                m_Controller.SetMaxRowsToGetPerRequest(0, maxRowsToGetPerRequest);

            StackHashEventPackageCollection events = null;

            String cabFile1 = null;
            String cabFile2 = null;
            String cabFile3 = null;
            String cabFile4 = null;

            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                // Add the events.
                IErrorIndex index = m_Controller.FindContext(0).ErrorIndex;

                StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, "www.link", 1, "Product1", 3, 0, "1.2.3.4");
                index.AddProduct(product1);
                StackHashProductSyncData syncData = new StackHashProductSyncData();
                syncData.ProductId = product1.Id;
                m_Controller.SetProductSyncData(0, syncData);

                StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 100, DateTime.Now, "File1", "1.2.3.4");
                StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 101, DateTime.Now, "File2", "1.2.3.4");
                index.AddFile(product1, file1);
                index.AddFile(product1, file2);

                StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 1, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 2, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event3 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 3, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);
                StackHashEvent event4 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 4, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);
                StackHashEvent event5 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 5, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);
                StackHashEvent event6 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 6, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);

                index.AddEvent(product1, file1, event1);
                index.AddEvent(product1, file1, event2);
                index.AddEvent(product1, file2, event1);
                index.AddEvent(product1, file2, event3);
                index.AddEvent(product1, file2, event4);
                index.AddEvent(product1, file2, event5);
                index.AddEvent(product1, file2, event6);


                cabFile1 = Path.GetTempFileName();
                cabFile2 = Path.GetTempFileName();
                cabFile3 = Path.GetTempFileName();
                cabFile4 = Path.GetTempFileName();
                StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, event2.Id, event2.EventTypeName, cabFile1, 100, 100);
                StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, event4.Id, event4.EventTypeName, cabFile2, 101, 100);
                StackHashCab cab3 = new StackHashCab(DateTime.Now, DateTime.Now, event5.Id, event5.EventTypeName, cabFile3, 102, 100);
                StackHashCab cab4 = new StackHashCab(DateTime.Now, DateTime.Now, event5.Id, event5.EventTypeName, cabFile4, 103, 100);

                index.AddCab(product1, file1, event2, cab1, false);
                index.AddCab(product1, file2, event4, cab2, false);
                index.AddCab(product1, file2, event5, cab3, false);
                index.AddCab(product1, file2, event6, cab4, false);

                TestManager.CreateTestScripts(index, product1, file1, event2, cab1, 1, 0);
                TestManager.CreateTestScripts(index, product1, file2, event4, cab2, 1, 0);
                TestManager.CreateTestScripts(index, product1, file2, event5, cab3, 1, 0);
                TestManager.CreateTestScripts(index, product1, file2, event6, cab4, 1, 0);

                long startRow = 1;
                long numberOfRows = 3;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(3, events.Count);
                Assert.AreEqual(2, events[0].EventData.Id);
                Assert.AreEqual(4, events[1].EventData.Id);
                Assert.AreEqual(5, events[2].EventData.Id);
                Assert.AreEqual(2, events.MinimumRowNumber);
                Assert.AreEqual(5, events.MaximumRowNumber);
                Assert.AreEqual(4, events.TotalRows);
            }
            finally
            {
                if ((cabFile1 != null) && File.Exists(cabFile1))
                    File.Delete(cabFile1);
                if ((cabFile2 != null) && File.Exists(cabFile2))
                    File.Delete(cabFile2);
                if ((cabFile3 != null) && File.Exists(cabFile3))
                    File.Delete(cabFile3);
                if ((cabFile4 != null) && File.Exists(cabFile4))
                    File.Delete(cabFile4);

                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }

        }

        /// <summary>
        /// Fog1352, 1476.
        /// Script search.
        /// Duplicate event in middle of page.
        /// Windows = 3.
        /// File1 - Event1 - script match
        /// File1 - Event2 
        /// File2 - Event1 - script match (same event as above).
        /// File2 - Event3.
        /// File2 - Event4 - script match.
        /// File2 - Event5 - script match.
        /// File2 - Event6 - script match.
        /// 
        /// Script match on duplicate event. Should return ev1, ev4 amd ev5.
        /// 
        /// </summary>
        [TestMethod]
        public void Fog1352_1476_ScriptMatchOnDuplicateEvent()
        {
            StackHashSearchCriteriaCollection searchCriteriaCollection = new StackHashSearchCriteriaCollection();
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 1),
                new StringSearchOption(StackHashObjectType.Script, "Content", StackHashSearchOptionType.StringContains, "Script", null, false),
            };
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(options1);

            searchCriteriaCollection.Add(criteria1);

            StackHashSortOrderCollection sortOrders = new StackHashSortOrderCollection();
            sortOrders.Add(new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true));

            m_Controller = new Controller(m_SettingsFileName, false, true);

            StackHashContextSettings contextSettings = m_Controller.CreateNewContext(ErrorIndexType.SqlExpress);

            // Clone them and make the necessary changes.
            StackHashContextSettings newContextSettings = new StackHashContextSettings();
            newContextSettings.AnalysisSettings = contextSettings.AnalysisSettings;
            newContextSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;
            newContextSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
            newContextSettings.CollectionPolicy = contextSettings.CollectionPolicy;
            newContextSettings.DebuggerSettings = contextSettings.DebuggerSettings;
            newContextSettings.EmailSettings = contextSettings.EmailSettings;

            newContextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            newContextSettings.ErrorIndexSettings.Folder = m_IndexPath;
            newContextSettings.ErrorIndexSettings.Name = "GetEventsTestIndex";
            newContextSettings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            newContextSettings.Id = contextSettings.Id;
            newContextSettings.IsActive = contextSettings.IsActive;
            newContextSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;
            newContextSettings.SqlSettings = new StackHashSqlConfiguration(contextSettings.SqlSettings.ConnectionString,
                newContextSettings.ErrorIndexSettings.Name, 5, 10, 100, 10);
            newContextSettings.WinQualSettings = contextSettings.WinQualSettings;
            newContextSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;


            m_Controller.ChangeContextSettings(newContextSettings);

            int maxRowsToGetPerRequest = 0;
            if (maxRowsToGetPerRequest > 0)
                m_Controller.SetMaxRowsToGetPerRequest(0, maxRowsToGetPerRequest);

            StackHashEventPackageCollection events = null;

            String cabFile1 = null;
            String cabFile2 = null;
            String cabFile3 = null;
            String cabFile4 = null;

            try
            {
                m_Controller.ActivateContextSettings(null, 0);

                // Add the events.
                IErrorIndex index = m_Controller.FindContext(0).ErrorIndex;

                StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, "www.link", 1, "Product1", 3, 0, "1.2.3.4");
                index.AddProduct(product1);
                StackHashProductSyncData syncData = new StackHashProductSyncData();
                syncData.ProductId = product1.Id;
                m_Controller.SetProductSyncData(0, syncData);

                StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 100, DateTime.Now, "File1", "1.2.3.4");
                StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 101, DateTime.Now, "File2", "1.2.3.4");
                index.AddFile(product1, file1);
                index.AddFile(product1, file2);

                StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 1, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 2, new StackHashEventSignature(new StackHashParameterCollection()), 10, file1.Id);
                StackHashEvent event3 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 3, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);
                StackHashEvent event4 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 4, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);
                StackHashEvent event5 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 5, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);
                StackHashEvent event6 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName1", 6, new StackHashEventSignature(new StackHashParameterCollection()), 10, file2.Id);

                index.AddEvent(product1, file1, event1);
                index.AddEvent(product1, file1, event2);
                index.AddEvent(product1, file2, event1);
                index.AddEvent(product1, file2, event3);
                index.AddEvent(product1, file2, event4);
                index.AddEvent(product1, file2, event5);
                index.AddEvent(product1, file2, event6);


                cabFile1 = Path.GetTempFileName();
                cabFile2 = Path.GetTempFileName();
                cabFile3 = Path.GetTempFileName();
                cabFile4 = Path.GetTempFileName();
                StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, event2.Id, event2.EventTypeName, cabFile1, 100, 100);
                StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, event4.Id, event4.EventTypeName, cabFile2, 101, 100);
                StackHashCab cab3 = new StackHashCab(DateTime.Now, DateTime.Now, event5.Id, event5.EventTypeName, cabFile3, 102, 100);
                StackHashCab cab4 = new StackHashCab(DateTime.Now, DateTime.Now, event5.Id, event5.EventTypeName, cabFile4, 103, 100);

                index.AddCab(product1, file1, event1, cab1, false);
                index.AddCab(product1, file2, event4, cab2, false);
                index.AddCab(product1, file2, event5, cab3, false);
                index.AddCab(product1, file2, event6, cab4, false);

                TestManager.CreateTestScripts(index, product1, file1, event1, cab1, 1, 0);
                TestManager.CreateTestScripts(index, product1, file2, event4, cab2, 1, 0);
                TestManager.CreateTestScripts(index, product1, file2, event5, cab3, 1, 0);
                TestManager.CreateTestScripts(index, product1, file2, event6, cab4, 1, 0);

                long startRow = 1;
                long numberOfRows = 3;

                events = m_Controller.GetEvents(0, searchCriteriaCollection, startRow, numberOfRows, sortOrders, StackHashSearchDirection.Forwards, startRow == 1);

                Assert.AreEqual(3, events.Count);
                Assert.AreEqual(1, events[0].EventData.Id);
                Assert.AreEqual(4, events[1].EventData.Id);
                Assert.AreEqual(5, events[2].EventData.Id);
                Assert.AreEqual(1, events.MinimumRowNumber);
                Assert.AreEqual(5, events.MaximumRowNumber);
                Assert.AreEqual(4, events.TotalRows);
            }
            finally
            {
                if ((cabFile1 != null) && File.Exists(cabFile1))
                    File.Delete(cabFile1);
                if ((cabFile2 != null) && File.Exists(cabFile2))
                    File.Delete(cabFile2);
                if ((cabFile3 != null) && File.Exists(cabFile3))
                    File.Delete(cabFile3);
                if ((cabFile4 != null) && File.Exists(cabFile4))
                    File.Delete(cabFile4);

                m_Controller.DeactivateContextSettings(null, 0);
                m_Controller.DeleteIndex(0);
                m_Controller.RemoveContextSettings(0, true);
                m_Controller.Dispose();
                m_Controller = null;
            }

        }

    }
}
