using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for ProductsFilesEventsUnitTests
    /// </summary>
    [TestClass]
    public class ProductsFilesEventsUnitTests
    {
        Utils m_Utils;

        public ProductsFilesEventsUnitTests()
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

        [TestMethod]
        public void GetProductsNoProducts()
        {
            m_Utils.CreateAndSetNewContext();
            m_Utils.ActivateContext(0);

            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(0, getProductsResp.Products.Count);
        }

        public void getProductsNProductsNFilesNEvents(ErrorIndexType indexType, int numProducts, int numFiles, int numEvents, int numEventInfos, int numCabs)
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

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreNotEqual(new DateTime(0), getProductsResp.LastSiteUpdateTime);
            Assert.AreEqual(true, Math.Abs((DateTime.Now - getProductsResp.LastSiteUpdateTime).TotalDays) <= 15);

            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            int productId = 1;
            int fileId = 1;
            int eventId = 1;

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

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        [TestMethod]
        public void GetProducts1Products1Files1Event1EventInfo1Cab()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.Xml, 1, 1, 1, 1, 1);
        }

        [TestMethod]
        public void GetProducts2Products2Files2Event2EventInfo2Cab()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.Xml, 2, 2, 2, 2, 2);
        }

        [TestMethod]
        public void GetProducts1Products10Files5Event2EventInfo3Cab()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.Xml, 1, 10, 5, 2, 3);
        }

        [TestMethod]
        public void GetProducts1Products1Files1Event1EventInfo1CabSql()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.SqlExpress, 1, 1, 1, 1, 1);
        }

        [TestMethod]
        public void GetProducts2Products2Files2Event2EventInfo2CabSql()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.SqlExpress, 2, 2, 2, 2, 2);
        }

        [TestMethod]
        public void GetProducts1Products10Files5Event2EventInfo3CabSql()
        {
            getProductsNProductsNFilesNEvents(ErrorIndexType.SqlExpress, 1, 10, 5, 2, 3);
        }

        
        // Get the 
        public void productInfoGet(ErrorIndexType indexType, int numProducts, bool resetService)
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
            testIndexData.NumberOfFiles = 0;
            testIndexData.NumberOfEvents = 0;
            testIndexData.NumberOfEventInfos = 0;
            testIndexData.NumberOfCabs = 0;

            m_Utils.CreateTestIndex(0, testIndexData);

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                StackHashProduct product = productInfo.Product;

                Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                Assert.AreEqual(null, productInfo.ProductSyncData);
            }

            // Now enable the products for sync.
            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);
            }

            // Get the list of product data again.
            getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                StackHashProduct product = productInfo.Product;

                Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                Assert.AreNotEqual(null, productInfo.ProductSyncData);
                Assert.AreEqual(productInfo.Product.Id, productInfo.ProductSyncData.ProductId);
            }


            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                StackHashProductSyncData syncData = new StackHashProductSyncData();
                syncData.ProductId = productInfo.Product.Id;
                m_Utils.SetProductSynchronizationData(0, syncData);
            }

            // Get the list of product data again.
            getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            if (resetService)
                m_Utils.RestartService();

            // Get the list of product data again.
            getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                StackHashProduct product = productInfo.Product;

                Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                Assert.AreNotEqual(null, productInfo.ProductSyncData);
                Assert.AreEqual(productInfo.Product.Id, productInfo.ProductSyncData.ProductId);
            }

            // Disable all products for sync.
            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, false);
            }

            
            getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                StackHashProduct product = productInfo.Product;

                Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                Assert.AreEqual(null, productInfo.ProductSyncData);
            }



            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        [TestMethod]
        public void ProductInfoGet1Product()
        {
            productInfoGet(ErrorIndexType.Xml, 1, false);
        }

        [TestMethod]
        public void ProductInfoGet2Products()
        {
            productInfoGet(ErrorIndexType.Xml, 2, false);
        }

        [TestMethod]
        public void ProductInfoGet20Products()
        {
            productInfoGet(ErrorIndexType.Xml, 20, false);
        }

        [TestMethod]
        public void ProductInfoGet1ProductWithReset()
        {
            productInfoGet(ErrorIndexType.Xml, 1, true);
        }

        [TestMethod]
        public void ProductInfoGet2ProductsWithReset()
        {
            productInfoGet(ErrorIndexType.Xml, 2, true);
        }

        [TestMethod]
        public void ProductInfoGet20ProductsWithReset()
        {
            productInfoGet(ErrorIndexType.Xml, 20, true);
        }

        [TestMethod]
        public void ProductInfoGet1ProductSql()
        {
            productInfoGet(ErrorIndexType.SqlExpress, 1, false);
        }

        [TestMethod]
        public void ProductInfoGet2ProductsSql()
        {
            productInfoGet(ErrorIndexType.SqlExpress, 2, false);
        }

        [TestMethod]
        public void ProductInfoGet20ProductsSql()
        {
            productInfoGet(ErrorIndexType.SqlExpress, 20, false);
        }

        [TestMethod]
        public void ProductInfoGet1ProductWithResetSql()
        {
            productInfoGet(ErrorIndexType.SqlExpress, 1, true);
        }

        [TestMethod]
        public void ProductInfoGet2ProductsWithResetSql()
        {
            productInfoGet(ErrorIndexType.SqlExpress, 2, true);
        }

        [TestMethod]
        public void ProductInfoGet20ProductsWithResetSql()
        {
            productInfoGet(ErrorIndexType.SqlExpress, 20, true);
        }

    
        // Get the 
        public void setEventBugId(ErrorIndexType indexType, bool resetService)
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
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 0;
            testIndexData.NumberOfCabs = 0;

            m_Utils.CreateTestIndex(0, testIndexData);

            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(1, getProductsResp.Products.Count());
            
            GetFilesResponse allFiles = m_Utils.GetFiles(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allFiles.Files.Count());

            GetProductEventPackageResponse allEvents = m_Utils.GetProductEventPackages(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allEvents.EventPackages.Count());

    
            // Set the event bug ID.
            String bugId = "1234567B";

            m_Utils.SetEventBugId(0, getProductsResp.Products[0].Product, allFiles.Files[0], allEvents.EventPackages[0].EventData, bugId);

            if (resetService)
                m_Utils.RestartService();

            allEvents = m_Utils.GetProductEventPackages(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allEvents.EventPackages.Count());

            Assert.AreEqual(bugId, allEvents.EventPackages[0].EventData.BugId);
            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);

        }

        [TestMethod]
        public void SetEventBugId()
        {
            setEventBugId(ErrorIndexType.Xml, false);
        }

        [TestMethod]
        public void SetEventBugIdWithReset()
        {
            setEventBugId(ErrorIndexType.Xml, true);
        }

        [TestMethod]
        public void SetEventBugIdSql()
        {
            setEventBugId(ErrorIndexType.SqlExpress, false);
        }

        [TestMethod]
        public void SetEventBugIdWithResetSql()
        {
            setEventBugId(ErrorIndexType.SqlExpress, true);
        }

        // Set the workflow status.
        public void setWorkFlowStatus(ErrorIndexType indexType, bool resetService)
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
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 0;
            testIndexData.NumberOfCabs = 0;

            m_Utils.CreateTestIndex(0, testIndexData);

            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(1, getProductsResp.Products.Count());

            GetFilesResponse allFiles = m_Utils.GetFiles(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allFiles.Files.Count());

            GetProductEventPackageResponse allEvents = m_Utils.GetProductEventPackages(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allEvents.EventPackages.Count());


            // Set the workflow status
            int workFlowStatus = 10;

            m_Utils.SetWorkFlowStatus(0, getProductsResp.Products[0].Product, allFiles.Files[0], allEvents.EventPackages[0].EventData, workFlowStatus);

            if (resetService)
                m_Utils.RestartService();

            allEvents = m_Utils.GetProductEventPackages(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allEvents.EventPackages.Count());

            Assert.AreEqual(workFlowStatus, allEvents.EventPackages[0].EventData.WorkFlowStatus);
            Assert.AreEqual("Resolved - Responded", allEvents.EventPackages[0].EventData.WorkFlowStatusName);
            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);

        }

        [TestMethod]
        public void SetWorkFlowStatus()
        {
            setWorkFlowStatus(ErrorIndexType.SqlExpress, false);
        }

        [TestMethod]
        public void SetWorkFlowStatusWithReset()
        {
            setWorkFlowStatus(ErrorIndexType.SqlExpress, true);
        }
        // Set the workflow status.
        public void getDefaultWorkFlowStatus(ErrorIndexType indexType, bool resetService)
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(indexType);

            Assert.AreEqual(null, resp.Settings.WorkFlowMappings);

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0); // Make sure it is empty.

            // Workflow should still be null.
            GetStackHashPropertiesResponse getSettingsResp = m_Utils.GetContextSettings();
            Assert.AreEqual(null, resp.Settings.WorkFlowMappings);
                        
            m_Utils.ActivateContext(0);

            // Workflow should now be set.
            getSettingsResp = m_Utils.GetContextSettings();
            Assert.AreNotEqual(null, getSettingsResp.Settings.ContextCollection[0].WorkFlowMappings);
            Assert.AreEqual(16, getSettingsResp.Settings.ContextCollection[0].WorkFlowMappings.Count);

            if (resetService)
                m_Utils.RestartService();

            // Workflow should now be set.
            getSettingsResp = m_Utils.GetContextSettings();
            Assert.AreNotEqual(null, getSettingsResp.Settings.ContextCollection[0].WorkFlowMappings);
            Assert.AreEqual(16, getSettingsResp.Settings.ContextCollection[0].WorkFlowMappings.Count);
            
            m_Utils.DeactivateContext(0);

            // Workflow should still be set when deactivated.
            getSettingsResp = m_Utils.GetContextSettings();
            Assert.AreNotEqual(null, getSettingsResp.Settings.ContextCollection[0].WorkFlowMappings);
            Assert.AreEqual(16, getSettingsResp.Settings.ContextCollection[0].WorkFlowMappings.Count);
            
            m_Utils.DeleteIndex(0);

        }

        [TestMethod]
        public void GetDefaultWorkFlowStatus()
        {
            getDefaultWorkFlowStatus(ErrorIndexType.SqlExpress, false);
        }

        [TestMethod]
        public void GetDefaultWorkFlowStatusWithReset()
        {
            getDefaultWorkFlowStatus(ErrorIndexType.SqlExpress, true);
        }

        
        // Get the 
        public void getCabPackage(ErrorIndexType indexType)
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
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 0;
            testIndexData.NumberOfCabs = 1;

            m_Utils.CreateTestIndex(0, testIndexData);

            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(1, getProductsResp.Products.Count());

            GetFilesResponse allFiles = m_Utils.GetFiles(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allFiles.Files.Count());

            GetProductEventPackageResponse allEvents = m_Utils.GetProductEventPackages(0, getProductsResp.Products[0].Product);
            Assert.AreEqual(1, allEvents.EventPackages.Count());
            Assert.AreEqual(1, allEvents.EventPackages[0].Cabs.Count);


            // Get the Cab package.
            GetCabPackageResponse cabPackageResp = m_Utils.GetCabPackage(0, getProductsResp.Products[0].Product, allFiles.Files[0], allEvents.EventPackages[0].EventData, 
                allEvents.EventPackages[0].Cabs[0].Cab);

            Assert.AreEqual("c:\\stackhashunittests\\testindex\\TestIndex\\00\\00\\00\\00\\CAB_0000000001\\1-Crash 32bit-0.cab", cabPackageResp.CabPackage.FullPath);
        }

        [TestMethod]
        public void GetCabPackageSql()
        {
            getCabPackage(ErrorIndexType.SqlExpress);
        }

    }
}
