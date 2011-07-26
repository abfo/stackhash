using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for SearchUnitTests
    /// </summary>
    [TestClass]
    public class SearchUnitTests
    {
        Utils m_Utils;

        public SearchUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

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


        public GetWindowedEventPackageResponse windowSearch(ErrorIndexType errorIndexType, StackHashTestIndexData testIndexData, StackHashSearchCriteriaCollection allSearchCriteria,
            long startRow, long numRows, StackHashSortOrderCollection sortOrder, StackHashSearchDirection direction, bool countAllMatches)
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(errorIndexType);

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            resp.Settings.ErrorIndexSettings.Type = errorIndexType;
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            m_Utils.CreateTestIndex(0, testIndexData);

            try
            {
                // Enable all products so that they appear in searchs.
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(0, product.Product.Id, true);
                }

                GetWindowedEventPackageResponse eventPackages = m_Utils.GetWindowedEvents(0, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);
                return eventPackages;
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// 1 event in index - get ALL events that match (window size = massive).
        /// </summary>
        [TestMethod]
        public void MaxWindowSize_1Event()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() 
                {ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0});
            
            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 1;
            long numRows = Int64.MaxValue;
            bool countAllMatches = true;
            StackHashSearchDirection direction = StackHashSearchDirection.Forwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(1, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(1, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(1, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(1, eventPackageResp.TotalRows);
        }

        /// <summary>
        /// 1 event in index - get ALL events that match (window size = massive).
        /// Script search specified but no scripts present so no matches found.
        /// </summary>
        [TestMethod]
        public void MaxIndexSizeWithScriptSearch_0EventsReturned()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() { ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0 });
            criteria1.SearchFieldOptions.Add(new StringSearchOption() { ObjectType = StackHashObjectType.Script, FieldName = "Content", SearchOptionType = StackHashSearchOptionType.StringContains, Start = "Hello", End = null, CaseSensitive = false });

            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 1;
            bool countAllMatches = true;
            long numRows = Int64.MaxValue;
            StackHashSearchDirection direction = StackHashSearchDirection.Forwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(0, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(0x7fffffff, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(0, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(0, eventPackageResp.TotalRows);
        }

        /// <summary>
        /// 1 event in index - get ALL events that match (window size = massive).
        /// Script search matches
        /// </summary>
        [TestMethod]
        public void MaxIndexSizeWithScriptSearch_1EventsReturned()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() { ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0 });
            criteria1.SearchFieldOptions.Add(new StringSearchOption() { ObjectType = StackHashObjectType.Script, FieldName = "Content", SearchOptionType = StackHashSearchOptionType.StringContains, Start = "Script", End = null, CaseSensitive = false });

            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 1;
            long numRows = Int64.MaxValue;
            bool countAllMatches = true;
            StackHashSearchDirection direction = StackHashSearchDirection.Forwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(1, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(1, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(1, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(1, eventPackageResp.TotalRows);
        }

        /// <summary>
        /// get 2 windows.
        /// </summary>
        [TestMethod]
        public void MaxIndexSizeWithScriptSearch_2EventsReturned_NextPage()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() { ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0 });
            criteria1.SearchFieldOptions.Add(new StringSearchOption() { ObjectType = StackHashObjectType.Script, FieldName = "Content", SearchOptionType = StackHashSearchOptionType.StringContains, Start = "Script", End = null, CaseSensitive = false });

            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 2;
            long numRows = 1;
            bool countAllMatches = false;
            StackHashSearchDirection direction = StackHashSearchDirection.Forwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(1, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(2, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(2, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(1, eventPackageResp.TotalRows);
        }

        /// <summary>
        /// get Prev of 2 windows.
        /// </summary>
        [TestMethod]
        public void MaxIndexSizeWithScriptSearch_2EventsReturned_PrevPage()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() { ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0 });
            criteria1.SearchFieldOptions.Add(new StringSearchOption() { ObjectType = StackHashObjectType.Script, FieldName = "Content", SearchOptionType = StackHashSearchOptionType.StringContains, Start = "Script", End = null, CaseSensitive = false });

            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 1;
            long numRows = 1;
            bool countAllMatches = false;
            StackHashSearchDirection direction = StackHashSearchDirection.Backwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(1, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(1, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(1, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(1, eventPackageResp.TotalRows);
        }

        /// <summary>
        /// Count all matches on first page.
        /// </summary>
        [TestMethod]
        public void CountAllMatchesOnFirstPage()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() { ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0 });
            criteria1.SearchFieldOptions.Add(new StringSearchOption() { ObjectType = StackHashObjectType.Script, FieldName = "Content", SearchOptionType = StackHashSearchOptionType.StringContains, Start = "Script", End = null, CaseSensitive = false });

            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 1;
            long numRows = 1;
            bool countAllMatches = true;
            StackHashSearchDirection direction = StackHashSearchDirection.Forwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(1, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(1, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(1, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(2, eventPackageResp.TotalRows);
        }

        /// <summary>
        /// Dont count all matches on first page. Should just return the window.
        /// </summary>
        [TestMethod]
        public void DontCountAllMatchesOnFirstPage()
        {
            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 2;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.UseLargeCab = false;

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria();
            criteria1.SearchFieldOptions = new StackHashSearchOptionCollection();
            criteria1.SearchFieldOptions.Add(new IntSearchOption() { ObjectType = StackHashObjectType.Event, FieldName = "Id", SearchOptionType = StackHashSearchOptionType.GreaterThan, Start = 0, End = 0 });
            criteria1.SearchFieldOptions.Add(new StringSearchOption() { ObjectType = StackHashObjectType.Script, FieldName = "Content", SearchOptionType = StackHashSearchOptionType.StringContains, Start = "Script", End = null, CaseSensitive = false });

            StackHashSearchCriteriaCollection allSearchCriteria = new StackHashSearchCriteriaCollection();
            allSearchCriteria.Add(criteria1);

            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection();
            sortOrder.Add(new StackHashSortOrder() { ObjectType = StackHashObjectType.Event, FieldName = "Id", Ascending = true });

            long startRow = 1;
            long numRows = 1;
            bool countAllMatches = false;
            StackHashSearchDirection direction = StackHashSearchDirection.Forwards;

            GetWindowedEventPackageResponse eventPackageResp =
                windowSearch(ErrorIndexType.SqlExpress, testIndexData, allSearchCriteria, startRow, numRows, sortOrder, direction, countAllMatches);

            Assert.AreEqual(1, eventPackageResp.EventPackages.Count);
            Assert.AreEqual(1, eventPackageResp.MinimumRowNumber);
            Assert.AreEqual(1, eventPackageResp.MaximumRowNumber);
            Assert.AreEqual(numRows, eventPackageResp.TotalRows);
        }

    }
}
