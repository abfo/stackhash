using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlClient;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ProductUnitTests
    {
        String m_TempPath;
        String m_RootCabFolder;
        SqlErrorIndex m_Index;

        public ProductUnitTests()
        {
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
            m_TempPath = Path.GetTempPath() + "StackHashTest_ErrorIndex";
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\StackHash_TestCabs";


            TidyTest();

            if (!Directory.Exists(m_RootCabFolder))
                Directory.CreateDirectory(m_RootCabFolder);

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
        }

         [TestCleanup()]
         public void MyTestCleanup() 
         {
             TidyTest();
             SqlConnection.ClearAllPools();
         }

         public void TidyTest()
         {
             if (m_Index != null)
             {
                 SqlConnection.ClearAllPools();

                 m_Index.Deactivate();
                 m_Index.DeleteIndex();
                 m_Index.Dispose();
                 m_Index = null;
             }

             if (Directory.Exists(m_TempPath))
                 PathUtils.DeleteDirectory(m_TempPath, true);
             if (Directory.Exists(m_RootCabFolder))
                 PathUtils.DeleteDirectory(m_RootCabFolder, true);
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
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestConstructorNullPath()
        {
            string errorIndexName = "TestIndex";

            XmlErrorIndex index = new XmlErrorIndex(null, errorIndexName);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestConstructorNullFilename()
        {
            string errorIndexPath = "c:\\Test";

            XmlErrorIndex index = new XmlErrorIndex(errorIndexPath, null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestConstructorNullIndexCached()
        {
            ErrorIndexCache indexCached = new ErrorIndexCache(null);
        }
        
        
        private void testSaveNProducts(IErrorIndex index, int numProducts)
        {
            index.Activate();
            StackHashProductCollection allProducts = new StackHashProductCollection();
            for (int i = 0; i < numProducts; i++)
            {
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, 1 + i, "TestProduct1" + i.ToString(), 20 + i, 30 + i, "2.10.02123.1293" + i.ToString());

                index.AddProduct(product1);
                allProducts.Add(product1);
            }

            // Get the product list.
            StackHashProductCollection products = index.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(numProducts, products.Count);

            for (int i = 0; i < allProducts.Count; i++)
            {
                StackHashProduct thisProduct = products.FindProduct(allProducts[i].Id);
                Assert.AreNotEqual(null, thisProduct);

                Assert.AreEqual(0, allProducts[i].CompareTo(thisProduct));

                thisProduct = index.GetProduct(allProducts[i].Id);
                Assert.AreNotEqual(null, thisProduct);
                Assert.AreEqual(0, allProducts[i].CompareTo(thisProduct));


                // Check that the dates are stored in UTC.
                Assert.AreEqual(true, thisProduct.DateCreatedLocal.Kind == DateTimeKind.Utc);
                Assert.AreEqual(true, thisProduct.DateModifiedLocal.Kind == DateTimeKind.Utc);
            }

            Assert.AreEqual(numProducts, index.TotalProducts);
        }

        [TestMethod]
        public void TestSaveNoProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testSaveNProducts(index, 0);
        }

        [TestMethod]
        public void TestSaveNoProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSaveNProducts(indexCache, 0);
        }

        [TestMethod]
        public void TestSaveNoProductSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testSaveNProducts(m_Index, 0);
        }

        
        [TestMethod]
        public void TestSave1Product()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testSaveNProducts(index, 1);
        }

        [TestMethod]
        public void TestSave1ProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSaveNProducts(indexCache, 1);
        }
        [TestMethod]
        public void TestSave1ProductSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testSaveNProducts(m_Index, 1);
        }

        [TestMethod]
        public void TestSave2Products()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testSaveNProducts(index, 2);
        }

        [TestMethod]
        public void TestSave2ProductsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSaveNProducts(indexCache, 2);
        }

        [TestMethod]
        public void TestSave2ProductsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testSaveNProducts(m_Index, 2);
        }

        
        [TestMethod]
        public void TestSave100Products()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testSaveNProducts(index, 100);
        }

        [TestMethod]
        public void TestSave100ProductsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSaveNProducts(indexCache, 100);
        }

        [TestMethod]
        public void TestSave100ProductsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testSaveNProducts(m_Index, 100);
        }

        private void testLoadProductListEmptyCalledTwice(IErrorIndex index)
        {
            index.Activate();
            StackHashProductCollection products = index.LoadProductList();
            Assert.AreNotEqual(null, products);
            Assert.AreEqual(0, products.Count);

            products = index.LoadProductList();
            Assert.AreNotEqual(null, products);
            Assert.AreEqual(0, products.Count);
        }

        [TestMethod]
        public void TestLoadProductListEmptyCalledTwice()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadProductListEmptyCalledTwice(index);
        }

        [TestMethod]
        public void TestLoadProductListEmptyCalledTwiceCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadProductListEmptyCalledTwice(indexCache);
        }

        [TestMethod]
        public void TestLoadProductListEmptyCalledTwiceSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testLoadProductListEmptyCalledTwice(m_Index);
        }

        
        private void testAddSameProductTwice(IErrorIndex index)
        {
            index.Activate();
            int i = 0;
            int productId = 200;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293", 20);

            index.AddProduct(product1);

            i = 1; // Causes the product fields to change (except the product id).
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId, "TestProduct1", 20 + i + 1, 30 + i + 2, "2.10.02123.1293", 100);

            // Add the same product ID again - should replace.
            index.AddProduct(product2);

            // Get the product list.
            StackHashProductCollection products = index.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(1, products.Count);

            Assert.AreEqual(0, product2.CompareTo(products[0]));

            // Should not have updated the non-WinQual fields.
            Assert.AreEqual(product1.TotalStoredEvents, products[0].TotalStoredEvents);

            StackHashProduct thisProduct = index.GetProduct(product2.Id);
            Assert.AreNotEqual(null, thisProduct);
            Assert.AreEqual(0, product2.CompareTo(thisProduct));
        }

        [TestMethod]
        public void TestAddSameProductTwice()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddSameProductTwice(index);
        }

        [TestMethod]
        public void TestAddSameProductTwiceCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddSameProductTwice(indexCache);
        }

        [TestMethod]
        public void TestAddSameProductTwiceSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testAddSameProductTwice(m_Index);
        }

        
        private void updateAllWinQualFieldsInProduct(IErrorIndex index)
        {
            index.Activate();
            int i = 0;
            int productId = 200;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293", 20);

            index.AddProduct(product1); 

            i = 1; // Causes the product fields to change (except the product id).
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId, "TestProduct1", 20 + i + 1, 30 + i + 2, "2.10.02123.1293", 100);

            // Add the same product ID again - should replace all fields.
            index.AddProduct(product2, true);

            // Get the product list.
            StackHashProductCollection products = index.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(1, products.Count);

            Assert.AreEqual(0, product2.CompareTo(products[0]));
            Assert.AreEqual(product2.TotalStoredEvents, products[0].TotalStoredEvents);

            StackHashProduct thisProduct = index.GetProduct(product2.Id);
            Assert.AreNotEqual(null, thisProduct);
            Assert.AreEqual(0, product2.CompareTo(thisProduct));
        }

        [TestMethod]
        public void UpdateAllWinQualFieldsInProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            updateAllWinQualFieldsInProduct(index);
        }

        [TestMethod]
        public void UpdateAllWinQualFieldsInProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            updateAllWinQualFieldsInProduct(indexCache);
        }

        [TestMethod]
        public void UpdateAllWinQualFieldsInProductSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            updateAllWinQualFieldsInProduct(m_Index);
        }

        private void testAddSameProductTwiceWithReset(bool useCache)
        {
            IErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            if (useCache)
                index = new ErrorIndexCache(index);


            index.Activate();
            int i = 0;
            int productId = 200;
            StackHashProduct product1 =
                new StackHashProduct(DateTime.Now.AddDays(i), DateTime.Now.AddDays(i), null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293");

            index.AddProduct(product1);

            i = 1; // Causes the product fields to change (except the product id).
            product1 =
                new StackHashProduct(DateTime.Now.AddDays(i), DateTime.Now.AddDays(i), null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293");

            // Add the same product ID again - should replace.
            index.AddProduct(product1);

            // Reload.
            index = new XmlErrorIndex(m_TempPath, "Cucku");
            if (useCache)
                index = new ErrorIndexCache(index);

            index.Activate();

            // Get the product list.
            StackHashProductCollection products = index.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(1, products.Count);

            Assert.AreEqual(0, product1.CompareTo(products[0]));

            StackHashProduct thisProduct = index.GetProduct(product1.Id);
            Assert.AreNotEqual(null, thisProduct);
            Assert.AreEqual(0, product1.CompareTo(thisProduct));
        }

        [TestMethod]
        public void TestAddSameProductTwiceWithReload()
        {
            testAddSameProductTwiceWithReset(false);
        }

        [TestMethod]
        public void TestAddSameProductTwiceWithReloadCached()
        {
            testAddSameProductTwiceWithReset(true);
        }


        private void testGetProductUnknownId(IErrorIndex index)
        {
            index.Activate();
            int i = 0;
            int productId = 200;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293");

            index.AddProduct(product1);

            // Get the product list.
            StackHashProductCollection products = index.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(1, products.Count);

            Assert.AreEqual(0, product1.CompareTo(products[0]));

            StackHashProduct thisProduct = index.GetProduct(product1.Id + 1);
            Assert.AreEqual(null, thisProduct);
        }

        [TestMethod]
        public void TestGetProductUnknownId()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetProductUnknownId(index);
        }

        [TestMethod]
        public void TestGetProductUnknownIdCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetProductUnknownId(indexCache);
        }
        [TestMethod]
        public void TestGetProductUnknownIdSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            testGetProductUnknownId(m_Index);
        }


        private void testCacheInitialiseNProducts(IErrorIndex realIndex, int numProducts)
        {
            realIndex.Activate();
            StackHashProductCollection allProducts = new StackHashProductCollection();

            for (int i = 0; i < numProducts; i++)
            {
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc).AddDays(i);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc).AddDays(i);
                int productId = 200 + i;
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293");

                realIndex.AddProduct(product1);
            }

            // Hook up the cache and call LoadProductList.
            ErrorIndexCache indexCache = new ErrorIndexCache(realIndex);
            indexCache.Activate();

            // Get the product list.
            StackHashProductCollection products = indexCache.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(numProducts, products.Count);

            for (int i = 0; i < allProducts.Count; i++)
            {
                Assert.AreEqual(0, allProducts[0].CompareTo(products.FindProduct(allProducts[0].Id)));
            }

            // Hook up the cache afresh and call GetProduct.
            indexCache = new ErrorIndexCache(realIndex);
            indexCache.Activate();
            for (int i = 0; i < allProducts.Count; i++)
            {
                StackHashProduct thisProduct = indexCache.GetProduct(allProducts[i].Id);
                Assert.AreNotEqual(null, thisProduct);
                Assert.AreEqual(0, allProducts[i].CompareTo(thisProduct));
            }
        }

        [TestMethod]
        public void TestCacheInitialise1Product()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testCacheInitialiseNProducts(index, 1);
        }
        [TestMethod]
        public void TestCacheInitialise2Product()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testCacheInitialiseNProducts(index, 2);
        }
        [TestMethod]
        public void TestCacheInitialise20Product()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testCacheInitialiseNProducts(index, 20);
        }

        private void testCacheInitialiseNProductsAddAnotherProduct(IErrorIndex realIndex, int numProducts)
        {
            realIndex.Activate();
            StackHashProductCollection allProducts = new StackHashProductCollection();

            for (int i = 0; i < numProducts; i++)
            {
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc).AddDays(i);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc).AddDays(i);
                int productId = 200 + i;
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20 + i, 30 + i, "2.10.02123.1293");

                realIndex.AddProduct(product1);
            }

            // Hook up the cache and call LoadProductList.
            ErrorIndexCache indexCache = new ErrorIndexCache(realIndex);
            indexCache.Activate();

            // Get the product list.
            StackHashProductCollection products = indexCache.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(numProducts, products.Count);

            for (int i = 0; i < allProducts.Count; i++)
            {
                Assert.AreEqual(0, allProducts[0].CompareTo(products.FindProduct(allProducts[0].Id)));
            }

            // Hook up the cache afresh and call GetProduct.
            indexCache = new ErrorIndexCache(realIndex);
            indexCache.Activate();

            for (int i = 0; i < allProducts.Count; i++)
            {
                StackHashProduct thisProduct = indexCache.GetProduct(allProducts[i].Id);
                Assert.AreNotEqual(null, thisProduct);
                Assert.AreEqual(0, allProducts[i].CompareTo(thisProduct));
            }


            // Now add one more product.
            StackHashProduct finalProduct =
                new StackHashProduct(DateTime.Now.AddDays(99), DateTime.Now.AddDays(99), null, 99, "TestProduct99", 99, 98, "2.10.02123.1293");
            indexCache.AddProduct(finalProduct);

            // Now reload and check it is still there.
            indexCache = new ErrorIndexCache(realIndex);
            indexCache.Activate();

            products = indexCache.LoadProductList();

            Assert.AreNotEqual(null, products);
            Assert.AreEqual(numProducts + 1, products.Count);

            StackHashProduct latestProduct = indexCache.GetProduct(finalProduct.Id);
            Assert.AreNotEqual(null, latestProduct);
            Assert.AreEqual(0, finalProduct.CompareTo(latestProduct));
        }
        [TestMethod]
        public void TestCacheInitialise5ProductsAddAnotherProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testCacheInitialiseNProductsAddAnotherProduct(index, 5);
        }

        
        [TestMethod]
        public void XmlIndexGetProductFolderValidChars()
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.files.com", 25, "ProductName", 0, 0, "1.2.3.4");

            String productPath = XmlErrorIndex.GetProductFolder(product);

            Assert.AreEqual("P_25_ProductName_1.2.3.4", productPath);
        }

        [TestMethod]
        public void XmlIndexGetProductFolderInvalidChars()
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.files.com", 25, @"P""r:o?d:u\c/t@(XP/2k)", 0, 0, "1.2.3.4");

            String productPath = XmlErrorIndex.GetProductFolder(product);

            Assert.AreEqual("P_25_P_r_o_d_u_c_t@(XP_2k)_1.2.3.4", productPath);
        }

        [TestMethod]
        public void XmlIndexGetProductFolderInvalidCharsInVersion()
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.files.com", 25, @"P""r:o?d:u\c/t@(XP/2k)", 0, 0, "1:2:3:4");

            String productPath = XmlErrorIndex.GetProductFolder(product);

            Assert.AreEqual("P_25_P_r_o_d_u_c_t@(XP_2k)_1_2_3_4", productPath);
        }

        
        public void updateProductStatsNoEvents(IErrorIndex index, int numProducts, int numFiles, int numEvents)
        {
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product = new StackHashProduct(creationDateTime, modifiedDateTime, "www.files.com", 25, @"P""r:o?d:u@(XP_2k)", 0, 0, "1:2:3:4");

            if (numProducts != 0)
            {
                index.AddProduct(product);
            }

            int eventId = 2000;
            for (int i = 0; i < numFiles; i++)
            {
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, i + 1000, DateTime.Now, "FileName", "1.2.3.4");
                index.AddFile(product, file);

                for (int j = 0; j < numEvents; j++)
                {
                    StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "Event type", eventId++, new StackHashEventSignature(), j, i + 1000);
                    theEvent.EventSignature.Parameters = new StackHashParameterCollection();
                    index.AddEvent(product, file, theEvent);
                }
            }

            product = index.UpdateProductStatistics(product);

            Assert.AreEqual(numFiles * numEvents, product.TotalStoredEvents);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateProductStatsIndexNotActive()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");

            updateProductStatsNoEvents(xmlIndex, 0, 0, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateProductStatsIndexNotActiveCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);

            updateProductStatsNoEvents(cacheIndex, 0, 0, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateProductStatsIndexNotActiveSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            updateProductStatsNoEvents(m_Index, 0, 0, 0);
        }


        [TestMethod]
        public void UpdateProductStatsIndexNoProduct()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 0, 0, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndexNoProductCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 0, 0, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndexNoProductSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 0, 0, 0);
        }

        
        [TestMethod]
        public void UpdateProductStatsIndexNoFiles()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 0, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndexNoFilesCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 0, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndexNoFilesSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 1, 0, 0);
        }


        [TestMethod]
        public void UpdateProductStatsIndex1FilesNoEvents()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 1, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1FilesNoEventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 1, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1FilesNoEventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 1, 1, 0);
        }


        [TestMethod]
        public void UpdateProductStatsIndex1Files1Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 1, 1);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1Files1EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 1, 1);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1Files1EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 1, 1, 1);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1Files2Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 1, 2);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1Files2EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 1, 2);
        }
        [TestMethod]
        public void UpdateProductStatsIndex1Files2EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 1, 1, 2);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1Files100Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 1, 100);
        }

        [TestMethod]
        public void UpdateProductStatsIndex1Files100EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 1, 100);
        }
        [TestMethod]
        public void UpdateProductStatsIndex1Files100EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();


            updateProductStatsNoEvents(m_Index, 1, 1, 100);
        }

        [TestMethod]
        public void UpdateProductStatsIndex10Files100Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 10, 100);
        }

        [TestMethod]
        public void UpdateProductStatsIndex10Files100EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 10, 100);
        }

        [TestMethod]
        public void UpdateProductStatsIndex10Files100EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 1, 10, 100);
        }

        [TestMethod]
        public void UpdateProductStatsIndex2Files0Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            updateProductStatsNoEvents(xmlIndex, 1, 2, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndex2Files0EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            updateProductStatsNoEvents(cacheIndex, 1, 2, 0);
        }

        [TestMethod]
        public void UpdateProductStatsIndex2Files0EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            updateProductStatsNoEvents(m_Index, 1, 2, 0);
        }


        [TestMethod]
        public void AddProductDisconnectReconnectGetProducts()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product = new StackHashProduct(creationDateTime, modifiedDateTime, "www.files.com", 25, @"P""r:o?d:u@(XP_2k)", 0, 0, "1:2:3:4");

            m_Index.AddProduct(product);

            Assert.AreEqual(1, m_Index.TotalProducts);
            Assert.AreEqual(true, m_Index.ProductExists(product));

            m_Index.Deactivate();
            m_Index.Dispose();

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.Activate();

            StackHashProductCollection products = m_Index.LoadProductList();
            Assert.AreEqual(1, products.Count);

            Assert.AreEqual(1, m_Index.TotalProducts);
            Assert.AreEqual(true, m_Index.ProductExists(product));
        }

        public void getProductEvents(IErrorIndex index, int numProducts, int numFiles, int numEvents)
        {
            StackHashProductCollection products = new StackHashProductCollection();
            int productId = 0x1234567;
            int eventsForThisProduct = numEvents;
            int eventId = 10000;
            int fileId = 20;
            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
                StackHashProduct product = new StackHashProduct(creationDateTime, modifiedDateTime, "www.files.com", productId+productCount, @"P""r:o?d:u@(XP_2k)", productCount, productCount+1, "1:2:3:4");

                index.AddProduct(product);
                products.Add(product);

                for (int i = 0; i < numFiles; i++)
                {
                    StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, fileId++, DateTime.Now, "FileName", "1.2.3.4");
                    index.AddFile(product, file);

                    for (int j = 0; j < eventsForThisProduct; j++)
                    {
                        StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "Event type", eventId++, new StackHashEventSignature(), j, i + 1000);
                        theEvent.EventSignature.Parameters = new StackHashParameterCollection();
                        index.AddEvent(product, file, theEvent);
                    }
                }
                eventsForThisProduct += 10;
            }

            eventsForThisProduct = numEvents;
            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                StackHashEventPackageCollection events = index.GetProductEvents(products[productCount]);

                Assert.AreEqual(numFiles * eventsForThisProduct, events.Count);
                eventsForThisProduct += 10;
            }
        }


        [TestMethod]
        public void GetProductEvents0Products0Files0Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 0, 0, 0);
        }

        [TestMethod]
        public void GetProductEvents0Products0Files0EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 0, 0, 0);
        }

        [TestMethod]
        public void GetProductEvents0Products0Files0EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 0, 0, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products0Files0Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 1, 0, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products0Files0EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 1, 0, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products0Files0EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 1, 0, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files0Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 1, 1, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files0EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 1, 1, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files0EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 1, 1, 0);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files1Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 1, 1, 1);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files1EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 1, 1, 1);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files1EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 1, 1, 1);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files2Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 1, 1, 2);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files2EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 1, 1, 2);
        }

        [TestMethod]
        public void GetProductEvents1Products1Files2EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 1, 1, 2);
        }


        [TestMethod]
        public void GetProductEvents1Products2Files1Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 1, 2, 1);
        }

        [TestMethod]
        public void GetProductEvents1Products2Files1EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 1, 2, 1);
        }

        [TestMethod]
        public void GetProductEvents1Products2Files1EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 1, 2, 1);
        }

        [TestMethod]
        public void GetProductEvents1Products2Files2Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 1, 2, 2);
        }

        [TestMethod]
        public void GetProductEvents1Products2Files2EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 1, 2, 2);
        }

        [TestMethod]
        public void GetProductEvents1Products2Files2EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 1, 2, 2);
        }

        [TestMethod]
        public void GetProductEvents2Products1Files2Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 2, 1, 2);
        }

        [TestMethod]
        public void GetProductEvents2Products1Files2EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 2, 1, 2);
        }

        [TestMethod]
        public void GetProductEvents2Products1Files2EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 2, 1, 2);
        }

        [TestMethod]
        public void GetProductEvents2Products2Files2Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 2, 2, 2);
        }

        [TestMethod]
        public void GetProductEvents2Products2Files2EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 2, 2, 2);
        }

        [TestMethod]
        public void GetProductEvents2Products2Files2EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 2, 2, 2);
        }

        [TestMethod]
        public void GetProductEvents20Products5Files3Events()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            xmlIndex.Activate();

            getProductEvents(xmlIndex, 20, 5, 3);
        }

        [TestMethod]
        public void GetProductEvents20Products5Files3EventsCached()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlIndex);
            cacheIndex.Activate();

            getProductEvents(cacheIndex, 20, 5, 3);
        }

        [TestMethod]
        public void GetProductEvents20Products5Files3EventsSql()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            getProductEvents(m_Index, 20, 5, 3);
        }
    }
}
