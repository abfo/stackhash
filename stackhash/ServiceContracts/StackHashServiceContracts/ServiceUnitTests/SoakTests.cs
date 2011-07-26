using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for SoakTests
    /// Performance related testing.
    /// </summary>
    [TestClass]
    public class SoakTests
    {
        Utils m_Utils;

        public SoakTests()
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

        public void getProductsNProductsNFilesNEvents(ErrorIndexType indexType, int numProducts, int numFiles, int numEvents, int numEventInfos, int numCabs)
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(indexType);

            String testPath = "c:\\stackhashsoaktest\\";
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

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            int productId = 1;
            int fileId = 1;
            int eventId = 1;

            try
            {
                foreach (StackHashProductInfo productInfo in getProductsResp.Products)
                {
                    StackHashProduct product = productInfo.Product;

                    Assert.AreEqual(productId++, product.Id);

                    GetFilesResponse getFilesResp = m_Utils.GetFiles(0, product);
                    Assert.AreEqual(numFiles, getFilesResp.Files.Count());

                    foreach (StackHashFile file in getFilesResp.Files)
                    {
                        Assert.AreEqual(fileId++, file.Id);

                        GetEventsResponse getEventsResp = m_Utils.GetEvents(0, product, file);
                        Assert.AreEqual(numEvents, getEventsResp.Events.Count());

                        foreach (StackHashEvent theEvent in getEventsResp.Events)
                        {
                            Assert.AreEqual(eventId++, theEvent.Id);

                            GetEventPackageResponse getEventPackageResp = m_Utils.GetEventPackage(0, product, file, theEvent);

                            Assert.AreEqual(numCabs, getEventPackageResp.EventPackage.Cabs.Count);
                            Assert.AreEqual(numEventInfos, getEventPackageResp.EventPackage.EventInfoList.Count);
                        }
                    }
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        [Ignore]
        /// 5000 events in total (50 files of 100 events each)
        public void GetProducts1Products40Files100Event1EventInfo1Cab()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.Xml, 1, 50, 100, 1, 1);
        }

        [TestMethod]
        [Ignore]
        /// 5000 events in total (50 files of 100 events each)
        public void GetProducts1Products40Files100Event1EventInfo1CabSql()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.SqlExpress, 1, 50, 100, 1, 1);
        }

        
        
        public void getProductEventPackages(ErrorIndexType indexType, int numProducts, int numFiles, int numEvents, int numEventInfos, int numCabs)
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(indexType);

            String testPath = "c:\\stackhashsoaktest\\";
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


            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(numProducts, getProductsResp.Products.Count());
            try
            {
                DateTime startTime = DateTime.Now;
                foreach (StackHashProductInfo productInfo in getProductsResp.Products)
                {
                    StackHashProduct product = productInfo.Product;

                    GetProductEventPackageResponse response = m_Utils.GetProductEventPackages(0, product);
                    Assert.AreEqual(1 * numFiles * numEvents, response.EventPackages.Count);

                    foreach (StackHashEventPackage package in response.EventPackages)
                    {
                        Assert.AreEqual(numCabs, package.Cabs.Count);
                        Assert.AreEqual(numEventInfos, package.EventInfoList.Count);
                    }
                }
                DateTime endTime = DateTime.Now;

                TimeSpan duration = endTime - startTime;
                Console.WriteLine("Duration: {0}", duration);


                m_Utils.RestartService();

                startTime = DateTime.Now;
                foreach (StackHashProductInfo productInfo in getProductsResp.Products)
                {
                    StackHashProduct product = productInfo.Product;

                    GetProductEventPackageResponse response = m_Utils.GetProductEventPackages(0, product);
                    Assert.AreEqual(1 * numFiles * numEvents, response.EventPackages.Count);

                    foreach (StackHashEventPackage package in response.EventPackages)
                    {
                        Assert.AreEqual(numCabs, package.Cabs.Count);
                        Assert.AreEqual(numEventInfos, package.EventInfoList.Count);
                    }
                }
                endTime = DateTime.Now;

                duration = endTime - startTime;
                Console.WriteLine("Duration: {0}", duration);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        [Ignore]
        /// 5000 events in total (50 files of 100 events each)
        public void Get5000ProductEventPackages()
        {
            getProductEventPackages(ErrorIndexType.Xml, 1, 50, 100, 1, 1);
        }


        [TestMethod]
        [Ignore]
        /// 20000 events in total 
        public void Get20000ProductEventPackagesNoCabs()
        {
            getProductEventPackages(ErrorIndexType.Xml, 1, 50, 400, 0, 0);
        }

        [TestMethod]
        [Ignore]
        /// 20000 events in total 
        public void Get20000ProductEventPackagesEventsAndCabs()
        {
            getProductEventPackages(ErrorIndexType.Xml, 1, 50, 400, 10, 1);
        }

        [TestMethod]
        [Ignore]
        /// 15000 events in total
        public void Get15000ProductEventsMultipleProducts()
        {
            getProductEventPackages(ErrorIndexType.Xml, 10, 50, 30, 0, 0);
        }

        [TestMethod]
        [Ignore]
        /// 20000 events in total
        public void Get20000ProductEventPackagesAllInSameFolder()
        {
            getProductEventPackages(ErrorIndexType.Xml, 1, 1, 20000, 0, 0);
        }

        [TestMethod]
        [Ignore]
        /// 50000 events in total 
        public void Get50000ProductEventPackages5FilesSql()
        {
            getProductEventPackages(ErrorIndexType.SqlExpress, 1, 5, 20000, 1, 0);
        }
        [TestMethod]
        [Ignore]
        /// 5000 events in total (50 files of 100 events each)
        public void Get5000ProductEventPackagesSql()
        {
            getProductEventPackages(ErrorIndexType.SqlExpress, 1, 50, 100, 1, 1);
        }


        [TestMethod]
        [Ignore]
        /// 20000 events in total 
        public void Get20000ProductEventPackagesNoCabsSql()
        {
            getProductEventPackages(ErrorIndexType.SqlExpress, 1, 50, 400, 0, 0);
        }

        [TestMethod]
        [Ignore]
        /// 20000 events in total 
        public void Get20000ProductEventPackagesEventsAndCabsSql()
        {
            getProductEventPackages(ErrorIndexType.SqlExpress, 1, 50, 400, 10, 1);
        }

        [TestMethod]
        [Ignore]
        /// 15000 events in total
        public void Get15000ProductEventsMultipleProductsSql()
        {
            getProductEventPackages(ErrorIndexType.SqlExpress, 10, 50, 30, 0, 0);
        }

        [TestMethod]
        [Ignore]
        /// 20000 events in total
        public void Get20000ProductEventPackagesAllInSameFolderSql()
        {
            getProductEventPackages(ErrorIndexType.SqlExpress, 1, 1, 20000, 0, 0);
        }
    
    }
}
