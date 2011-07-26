using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for StatisticsUnitTests
    /// </summary>
    [TestClass]
    public class StatisticsUnitTests
    {
        Utils m_Utils;

        public StatisticsUnitTests()
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

        public void getProductSummary(ErrorIndexType indexType, int numProducts, int numFiles, int numEvents, int numEventInfos, int numCabs, int productId)
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

            m_Utils.CreateTestIndex(0, testIndexData);

            GetProductRollupResponse resp2 = m_Utils.GetProductSummary(0, productId);

            Assert.AreEqual(numFiles * numEvents * numEventInfos, resp2.RollupData.HitDateSummary.Count);
            Assert.AreEqual(numFiles * numEvents * numEventInfos, resp2.RollupData.LocaleSummaryCollection.Count);
            Assert.AreEqual(numFiles * numEvents * numEventInfos, resp2.RollupData.OperatingSystemSummary.Count);

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        [TestMethod]
        public void GetProductSummary1()
        {
            int productId = 1;
            getProductSummary(ErrorIndexType.SqlExpress, 1, 1, 1, 1, 1, productId);
        }

        [TestMethod]
        public void GetProductSummary10()
        {
            int productId = 1;
            getProductSummary(ErrorIndexType.SqlExpress, 1, 1, 1, 10, 1, productId);
        }

        [TestMethod]
        public void GetProductSummary5In2Products()
        {
            int productId = 1;
            getProductSummary(ErrorIndexType.SqlExpress, 2, 1, 1, 5, 1, productId);
        }
    }
}
