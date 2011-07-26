using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for GetEventsUnitTests
    /// </summary>
    [TestClass]
    public class GetEventsUnitTests
    {
        Utils m_Utils;

        public GetEventsUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

            m_Utils.RemoveAllContexts();
            m_Utils.RemoveAllScripts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.Dispose();
                m_Utils = null;
            }
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

        public void getWindowedEventsByOrder(ErrorIndexType indexType, int numProducts, int numFiles, int numEvents, 
            int numEventInfos, int numCabs, int windowSize, bool restrictSearchToParticularProduct, bool addLotsOfSearchOptions, 
            List<int> enabledProducts, StackHashSearchDirection direction)
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(indexType);

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0); // Make sure it is empty.
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = numProducts;
            testIndexData.NumberOfFiles = numFiles;
            testIndexData.NumberOfEvents = numEvents;
            testIndexData.NumberOfEventInfos = numEventInfos;
            testIndexData.NumberOfCabs = numCabs;
            testIndexData.NumberOfScriptResults = numCabs;

            m_Utils.CreateTestIndex(0, testIndexData);

            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection();

            // Just get events for even numbered product ids.
            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                if ((enabledProducts == null) || enabledProducts.Contains(productCount + 1))
                    m_Utils.SetProductSynchronizationState(0, productCount + 1, true);

                // This doesn't really do anything - just in here so there is at least 1 search option.
                StackHashSearchCriteria newCriteria = new StackHashSearchCriteria
                {
                    SearchFieldOptions =
                        new StackHashSearchOptionCollection() 
                            {
                                new IntSearchOption{
                                    ObjectType=StackHashObjectType.Product, 
                                    FieldName="Id", 
                                    SearchOptionType=StackHashSearchOptionType.Equal, 
                                    Start=productCount + 1, 
                                    End=productCount + 1},
                            }
                };

                if (addLotsOfSearchOptions)
                {
                    // Add some "always true" options just to complicate things.
                    newCriteria.SearchFieldOptions.Add(new IntSearchOption{
                        ObjectType=StackHashObjectType.Event, 
                        FieldName="Id", 
                        SearchOptionType=StackHashSearchOptionType.RangeExclusive, 
                        Start=0, 
                        End=Int32.MaxValue});
                    newCriteria.SearchFieldOptions.Add(new IntSearchOption{
                        ObjectType=StackHashObjectType.Event, 
                        FieldName="Id", 
                        SearchOptionType=StackHashSearchOptionType.RangeInclusive, 
                        Start=0, 
                        End=Int32.MaxValue});
                    newCriteria.SearchFieldOptions.Add(new IntSearchOption{
                        ObjectType=StackHashObjectType.Event, 
                        FieldName="Id", 
                        SearchOptionType=StackHashSearchOptionType.GreaterThan, 
                        Start=0, 
                        End=0});
                    newCriteria.SearchFieldOptions.Add(new DateTimeSearchOption{
                        ObjectType = StackHashObjectType.EventInfo,
                        FieldName = "DateCreatedLocal",
                        SearchOptionType = StackHashSearchOptionType.RangeExclusive,
                        Start = DateTime.Now.AddYears(-20),
                        End = DateTime.Now.AddYears(20)});
                    newCriteria.SearchFieldOptions.Add(new DateTimeSearchOption
                    {
                        ObjectType = StackHashObjectType.CabInfo,
                        FieldName = "DateCreatedLocal",
                        SearchOptionType = StackHashSearchOptionType.RangeExclusive,
                        Start = DateTime.Now.AddYears(-20),
                        End = DateTime.Now.AddYears(20)
                    });
                }

                if (restrictSearchToParticularProduct)
                {
                    if (((productCount + 1) % 2) == 0)
                        allCriteria.Add(newCriteria);
                }
                else
                {
                    allCriteria.Add(newCriteria);
                }
            }


            // The events will be returned possibly in event id order from the search. 
            // Set the sort order based on the Offset - ascending - this should return the events
            // in reverse event ID order as the offsets descend.
            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="Offset", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.Event, FieldName="DateCreatedLocal", Ascending=true},  
                new StackHashSortOrder{ObjectType=StackHashObjectType.Event, FieldName="DateModifiedLocal", Ascending=true},  
                new StackHashSortOrder{ObjectType=StackHashObjectType.Event, FieldName="EventTypeName", Ascending=true},  
                new StackHashSortOrder{ObjectType=StackHashObjectType.Event, FieldName="Id", Ascending=true},  
                new StackHashSortOrder{ObjectType=StackHashObjectType.Event, FieldName="TotalHits", Ascending=true},  
                new StackHashSortOrder{ObjectType=StackHashObjectType.Event, FieldName="BugId", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ApplicationName", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ApplicationVersion", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ApplicationTimeStamp", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ModuleName", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ModuleVersion", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ModuleTimeStamp", Ascending=true},
                new StackHashSortOrder{ObjectType=StackHashObjectType.EventSignature, FieldName="ExceptionCode", Ascending=true},
            };

            int totalEventsExpected = numProducts * numFiles * numEvents;

            
            List<int> expectedEventIds = new List<int>();

            int expectedEventId = 1;
            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                for (int fileCount = 0; fileCount < numFiles; fileCount++)
                {
                    for (int eventCount = 0; eventCount < numEvents; eventCount++)
                    {
                        // Only add product events for even numbered products.
                        if (restrictSearchToParticularProduct)
                        {
                            if (((productCount + 1) % 2) == 0)
                                expectedEventIds.Add(expectedEventId++);
                            else
                                expectedEventId++;
                        }
                        else if ((enabledProducts != null) && !enabledProducts.Contains(productCount + 1))
                        {
                            expectedEventId++;
                        }
                        else
                        {
                            expectedEventIds.Add(expectedEventId++);
                        }
                    }
                }
            }

            expectedEventIds.Reverse();

            for (int startRow = 1; startRow <= expectedEventIds.Count; startRow++)
            {
                DateTime startTime = DateTime.Now;

                // Get the next window.
                GetWindowedEventPackageResponse allPackages = m_Utils.GetWindowedEvents(0, allCriteria, startRow, windowSize, allSortOrders, direction, startRow == 1);

                TimeSpan totalTime = DateTime.Now - startTime;

                Console.WriteLine("Window: {0}, startRow: {1}, numRows: {2}, duration: {3}", windowSize, startRow, allPackages.EventPackages.Count, totalTime);

                Assert.AreNotEqual(null, allPackages.EventPackages);

                int expectedRowsReturned = startRow + windowSize - 1 > expectedEventIds.Count ? expectedEventIds.Count - startRow + 1 : windowSize;
                if (expectedRowsReturned < 0)
                    expectedRowsReturned = 0;

                Assert.AreEqual(expectedRowsReturned, allPackages.EventPackages.Count);

                for (int eventIndex = 0; eventIndex < expectedRowsReturned; eventIndex++)
                {
                    int nextExpectedEventId = expectedEventIds[startRow + eventIndex - 1];

                    StackHashEventPackage eventRetrieved = allPackages.EventPackages[eventIndex];

                    Assert.AreEqual(nextExpectedEventId, eventRetrieved.EventData.Id);
                    Assert.AreEqual(numCabs, eventRetrieved.Cabs.Count);
                    Assert.AreEqual(numEventInfos, eventRetrieved.EventInfoList.Count);

                    foreach (StackHashCabPackage cabPackage in eventRetrieved.Cabs)
                    {
                        Assert.AreNotEqual(null, cabPackage.Cab);
                        Assert.AreNotEqual(null, cabPackage.CabFileContents);
                        Assert.AreNotEqual(null, cabPackage.CabFileContents.Files);
                        Assert.AreEqual(2, cabPackage.CabFileContents.Files.Count);
                        Assert.AreEqual("cuckusrv.exe.mdmp", cabPackage.CabFileContents.Files[0].FileName);
                        Assert.AreEqual(0x1a5cb, cabPackage.CabFileContents.Files[0].Length);
                        Assert.AreEqual("version.txt", cabPackage.CabFileContents.Files[1].FileName);
                        Assert.AreEqual(0x24, cabPackage.CabFileContents.Files[1].Length);
                    }

                    // Get the event package through the other interface.
                    StackHashProduct product = new StackHashProduct() { Id = eventRetrieved.ProductId };
                    StackHashFile file = new StackHashFile() { Id = eventRetrieved.EventData.FileId };
                    StackHashEventPackage matchedPackage = m_Utils.GetEventPackage(0, product, file, eventRetrieved.EventData).EventPackage;

                    Assert.AreEqual(eventRetrieved.Cabs.Count, matchedPackage.Cabs.Count);

                    foreach (StackHashCabPackage cabPackage in matchedPackage.Cabs)
                    {
                        Assert.AreNotEqual(null, cabPackage.CabFileContents);
                        Assert.AreNotEqual(null, cabPackage.CabFileContents.Files);
                        Assert.AreEqual(2, cabPackage.CabFileContents.Files.Count);
                        Assert.AreEqual("cuckusrv.exe.mdmp", cabPackage.CabFileContents.Files[0].FileName);
                        Assert.AreEqual(0x1a5cb, cabPackage.CabFileContents.Files[0].Length);
                        Assert.AreEqual("version.txt", cabPackage.CabFileContents.Files[1].FileName);
                        Assert.AreEqual(0x24, cabPackage.CabFileContents.Files[1].Length);
                    }
                }
            }

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        /// <summary>
        /// 1 event - window size of 1.
        /// </summary>
        [TestMethod]
        public void GetWindowedEvents1EventWindowSize1()
        {
            int numProducts = 1;
            int numFiles = 1;
            int numEvents = 1;
            int numEventInfos = 1;
            int numCabs = 1;
            int windowSize = 1;
            bool restrictSearchToParticularProduct = false;
            bool addLotsOfSearchOptions = false;

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, null, StackHashSearchDirection.Forwards);
        }

        /// <summary>
        /// 10 events - window size of 1.
        /// </summary>
        [TestMethod]
        public void GetWindowedEvents10EventWindowSize1()
        {
            int numProducts = 1;
            int numFiles = 1;
            int numEvents = 10;
            int numEventInfos = 1;
            int numCabs = 1;
            int windowSize = 1;
            bool restrictSearchToParticularProduct = false;
            bool addLotsOfSearchOptions = false;

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, null, StackHashSearchDirection.Forwards);
        }

        /// <summary>
        /// 10 events - 2 products - window size of 1.
        /// </summary>
        [TestMethod]
        public void GetWindowedEvents2Products5EventsWindowSize1()
        {
            int numProducts = 2;
            int numFiles = 1;
            int numEvents = 5;
            int numEventInfos = 1;
            int numCabs = 1;
            int windowSize = 1;
            bool restrictSearchToParticularProduct = false;
            bool addLotsOfSearchOptions = false;

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, null, StackHashSearchDirection.Forwards);
        }

        
        /// <summary>
        /// 1000 events - window size of 100.
        /// Disabled because it takes too long to run.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void GetWindowedEvents1000EventWindowSize100()
        {
            int numProducts = 2;
            int numFiles = 4;
            int numEvents = 125;
            int numEventInfos = 1;
            int numCabs = 0;
            int windowSize = 100;
            bool restrictSearchToParticularProduct = false;
            bool addLotsOfSearchOptions = false;

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, null, StackHashSearchDirection.Forwards);
        }

        /// <summary>
        /// 100 events - window size of 10.
        /// Just get the events associated with a single product.
        /// </summary>
        [TestMethod]
        public void GetWindowedEvents100EventWindowSize10EvenProductIdsTwoProducts()
        {
            int numProducts = 2;
            int numFiles = 2;
            int numEvents = 25;
            int numEventInfos = 1;
            int numCabs = 1;
            int windowSize = 10;
            bool restrictSearchToParticularProduct = true;
            bool addLotsOfSearchOptions = false;

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, null, StackHashSearchDirection.Forwards);
        }

        /// <summary>
        /// 100 events - window size of 7 - just even product IDs - 10 products.
        /// </summary>
        [TestMethod]
        public void GetWindowedEvents100EventWindowSize7EvenProductIdsTenProducts()
        {
            int numProducts = 10;
            int numFiles = 2;
            int numEvents = 2;
            int numEventInfos = 2;
            int numCabs = 2;
            int windowSize = 7;
            bool restrictSearchToParticularProduct = true;
            bool addLotsOfSearchOptions = true;

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, null, StackHashSearchDirection.Forwards);
        }

        /// <summary>
        /// First of 2 products enabled.
        /// Should only return events for enabled products.
        /// </summary>
        [TestMethod]
        public void GetWindowedEventsFirstProductEnabled()
        {
            int numProducts = 2;
            int numFiles = 1;
            int numEvents = 50;
            int numEventInfos = 1;
            int numCabs = 0;
            int windowSize = 10;
            bool restrictSearchToParticularProduct = false;
            bool addLotsOfSearchOptions = false;
            List<int> enabledProducts = new List<int> { 1 };

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, enabledProducts, StackHashSearchDirection.Forwards);
        }

        /// <summary>
        /// Second of 2 products enabled.
        /// Should only return events for enabled products.
        /// </summary>
        [TestMethod]
        public void GetWindowedEventsSecondProductEnabled()
        {
            int numProducts = 2;
            int numFiles = 1;
            int numEvents = 50;
            int numEventInfos = 1;
            int numCabs = 0;
            int windowSize = 10;
            bool restrictSearchToParticularProduct = false;
            bool addLotsOfSearchOptions = false;
            List<int> enabledProducts = new List<int> { 2 };

            getWindowedEventsByOrder(ErrorIndexType.SqlExpress, numProducts, numFiles, numEvents, numEventInfos, numCabs,
                windowSize, restrictSearchToParticularProduct, addLotsOfSearchOptions, enabledProducts, StackHashSearchDirection.Forwards);
        }


    
    
    }
}
