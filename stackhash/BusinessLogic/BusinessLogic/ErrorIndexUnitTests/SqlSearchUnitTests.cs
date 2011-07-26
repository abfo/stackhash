using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Summary description for SqlEventsUnitTest
    /// </summary>
    [TestClass]
    public class SqlSearchUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlSearchUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\StackHash_TestCabs";

            if (!Directory.Exists(m_RootCabFolder))
                Directory.CreateDirectory(m_RootCabFolder);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Index != null)
            {
                SqlConnection.ClearAllPools();

                m_Index.Deactivate();
                m_Index.DeleteIndex();
                m_Index.Dispose();
                m_Index = null;
            }

            if (Directory.Exists(m_RootCabFolder))
                PathUtils.DeleteDirectory(m_RootCabFolder, true);
            SqlConnection.ClearAllPools();
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


        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - match Id.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOneProductId()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - no match.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchNoMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }

        /// <summary>
        /// X OR Y - where X=TRUE and Y=TRUE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_OR_Y_where_True_True()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }


        /// <summary>
        /// X OR Y - where X=FALSE and Y=TRUE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_OR_Y_where_False_True()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        /// <summary>
        /// X OR Y - where X=TRUE and Y=FALSE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_OR_Y_where_True_False()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JHGHG", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        /// <summary>
        /// X OR Y - where X=FALSE and Y=FALSE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_OR_Y_where_False_False()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JHGHG", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }


        /// <summary>
        /// X AND Y - where X=FALSE and Y=FALSE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_AND_Y_where_False_False()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JHGHG", null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.File, "FileId", StackHashSearchOptionType.Equal, 24, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }


        /// <summary>
        /// X AND Y - where X=TRUE and Y=FALSE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_AND_Y_where_True_False()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JHGHG", null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.File, "FileId", StackHashSearchOptionType.Equal, 24, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }

        /// <summary>
        /// X AND Y - where X=FALSE and Y=TRUE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_AND_Y_where_False_True()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName, null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.File, "FileId", StackHashSearchOptionType.Equal, 24, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }

        /// <summary>
        /// X AND Y - where X=TRUE and Y=TRUE.
        /// </summary>
        [TestMethod]
        public void SearchProduct_X_AND_Y_where_True_True()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName, null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.File, "FileId", StackHashSearchOptionType.Equal, 24, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        [TestMethod]
        public void SearchProduct_X_AND_Y___OR___V_AND_W___where_F_F_F_F()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JHGG", null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.LessThan, totalResponses - 1, 0),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime, creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }

        [TestMethod]
        public void SearchProduct_X_AND_Y___OR___V_AND_W___where_T_F_F_F()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JHGG", null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.LessThan, totalResponses - 1, 0),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime, creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }

        [TestMethod]
        public void SearchProduct_X_AND_Y___OR___V_AND_W___where_T_T_F_F()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName, null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.LessThan, totalResponses - 1, 0),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime, creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        [TestMethod]
        public void SearchProduct_X_AND_Y___OR___V_AND_W___where_F_F_T_T()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "jh", null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.LessThan, totalResponses + 1, 0),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        [TestMethod]
        public void SearchProduct_X_AND_Y___OR___V_AND_W___where_T_T_T_T()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName, null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.LessThan, totalResponses + 1, 0),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        [TestMethod]
        public void SearchProduct_2Products()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(2), null, productId + 1, productName + "IJH", 10, totalResponses + 1, "2.10.02123.2312");

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Version", StackHashSearchOptionType.StringStartsWith, "2.10.02123", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(2, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
            Assert.AreEqual(true, allProductIds.Contains(productId + 1));
        }


        [TestMethod]
        public void SearchProduct_X_AND_Y___OR___V_AND_W___where_F_F_F_T()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "Cucku Backup";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "JH", null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.LessThan, totalResponses - 1, 0),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
        }


        [TestMethod]
        public void SearchFile_MatchingProductAndFileProperties()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 2;
            String productName = "Cucku Backup";

            int fileId = 20;
            String fileName = "StackHashUtilities.dll";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, fileName, "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String productSearchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            Collection<int> allProductIds = m_Index.GetProductMatch(productSearchString);            

            String fileSearchString = allCriteria.ToSqlString(StackHashObjectType.File, "F");
            StackHashFileProductMappingCollection allFileIds = m_Index.GetFilesMatch(allProductIds, fileSearchString);

            Assert.AreEqual(1, allFileIds.Count);
            Assert.AreEqual(true, (allFileIds.FindFile(fileId) != null));
        }

        [TestMethod]
        public void SearchFile_MatchingProductButNotFileProperties()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 2;
            String productName = "Cucku Backup";

            int fileId = 20;
            String fileName = "StackHashUtilities.dll";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, fileName, "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String productSearchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            Collection<int> allProductIds = m_Index.GetProductMatch(productSearchString);

            String fileSearchString = allCriteria.ToSqlString(StackHashObjectType.File, "F");
            StackHashFileProductMappingCollection allFileIds = m_Index.GetFilesMatch(allProductIds, fileSearchString);

            Assert.AreEqual(0, allFileIds.Count);
        }

        [TestMethod]
        public void SearchFile_ProductMismatchButFileMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 2;
            String productName = "Cucku Backup";

            int fileId = 20;
            String fileName = "StackHashUtilities.dll";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, fileName, "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String productSearchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            Collection<int> allProductIds = m_Index.GetProductMatch(productSearchString);

            if (allProductIds.Count > 0)
            {
                String fileSearchString = allCriteria.ToSqlString(StackHashObjectType.File, "F");
                StackHashFileProductMappingCollection allFileIds = m_Index.GetFilesMatch(allProductIds, fileSearchString);

                Assert.AreEqual(0, allFileIds.Count);
            }
        }



        /// <summary>
        /// 2 products each with a file.
        /// One product matches - but both files match.
        /// Should return 1 file.
        /// </summary>
        [TestMethod]
        public void SearchFile_2ProductsBothFilesMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 2;
            String productName = "Cucku Backup";

            int fileId = 20;
            String fileName = "StackHashUtilities.dll";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId + 1, productName, 20, totalResponses, "2.10.02123.2222");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, fileName, "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId + 1, creationDateTime.AddDays(1), fileName + "ZZZ", "1.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product2, file2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String productSearchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            Collection<int> allProductIds = m_Index.GetProductMatch(productSearchString);
            Assert.AreEqual(1, allProductIds.Count);

            if (allProductIds.Count > 0)
            {
                String fileSearchString = allCriteria.ToSqlString(StackHashObjectType.File, "F");
                StackHashFileProductMappingCollection allFileIds = m_Index.GetFilesMatch(allProductIds, fileSearchString);

                Assert.AreEqual(1, allFileIds.Count);
                Assert.AreEqual(true, allFileIds.FindFile(fileId + 1) != null);
            }
        }

        /// <summary>
        /// 2 products each with a file.
        /// Both products matches - both files match.
        /// Should return 2 files.
        /// </summary>
        [TestMethod]
        public void SearchFile_2Products2Files_AllMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int totalResponses = 10;
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 2;
            String productName = "Cucku Backup";

            int fileId = 20;
            String fileName = "StackHashUtilities.dll";

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, totalResponses, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId + 1, productName, 20, totalResponses, "2.10.02123.2222");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, fileName, "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime, fileId + 1, creationDateTime.AddDays(1), fileName + "ZZZ", "1.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product2, file2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, productId - 1, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime.AddDays(-1), creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String productSearchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            Collection<int> allProductIds = m_Index.GetProductMatch(productSearchString);
            Assert.AreEqual(2, allProductIds.Count);

            if (allProductIds.Count > 0)
            {
                String fileSearchString = allCriteria.ToSqlString(StackHashObjectType.File, "F");
                StackHashFileProductMappingCollection allFileIds = m_Index.GetFilesMatch(allProductIds, fileSearchString);

                Assert.AreEqual(2, allFileIds.Count);
                Assert.AreEqual(true, (allFileIds.FindFile(fileId) != null));
                Assert.AreEqual(true, allFileIds.FindFile(fileId + 1) != null);
            }
        }
        
        /// <summary>
        /// Search for events based on product name.
        /// One product - match.
        /// </summary>
        [TestMethod]
        public void SearchEventsProductIdMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based on workflowstatusname
        /// 1 match out of 1.
        /// </summary>
        [TestMethod]
        public void SearchEventsWorkFlowStatusMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[2].Id, workFlowMappings[2].Name);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "WorkFlowStatusName", StackHashSearchOptionType.Equal, workFlowMappings[2].Name, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }


        /// <summary>
        /// Search for events based on workflowstatusname
        /// 0 match out of 1.
        /// </summary>
        [TestMethod]
        public void SearchEventsWorkFlowStatusMismatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[2].Id, workFlowMappings[2].Name);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "WorkFlowStatusName", StackHashSearchOptionType.Equal, workFlowMappings[1].Name, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// Wildcard search for events based on all fields.
        /// 0 match out of 1.
        /// </summary>
        [TestMethod]
        public void WildcardSearchEventsWorkFlowStatusMismatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[2].Id, workFlowMappings[2].Name);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "*", StackHashSearchOptionType.Equal, workFlowMappings[1].Name, null, false),
                new StringSearchOption(StackHashObjectType.EventWorkFlow, "*", StackHashSearchOptionType.Equal, workFlowMappings[1].Name, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// Wildcard search for events based on all fields.
        /// 1 match out of 1.
        /// </summary>
        [TestMethod]
        public void WildcardSearchEventsWorkFlowStatusMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[2].Id, workFlowMappings[2].Name);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "*", StackHashSearchOptionType.Equal, workFlowMappings[2].Name, null, false),
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventWorkFlow, "*", StackHashSearchOptionType.Equal, workFlowMappings[2].Name, null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2),
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// Order by workflow status.
        /// </summary>
        [TestMethod]
        public void WildcardSearchEventsOrderByWorkFlowStatus()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            
            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventWorkFlow, "*", StackHashSearchOptionType.StringContains, "Active", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrder sortOrder = new StackHashSortOrder(StackHashObjectType.EventWorkFlow, "WorkFlowStatusName", true);
            StackHashSortOrderCollection allSortOrder = new StackHashSortOrderCollection() { sortOrder };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrder, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
        }

        /// <summary>
        /// Order by workflow status.
        /// </summary>
        [TestMethod]
        public void WildcardSearchEventsOrderByWorkFlowStatusDescending()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventWorkFlow, "*", StackHashSearchOptionType.StringContains, "Active", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrder sortOrder = new StackHashSortOrder(StackHashObjectType.EventWorkFlow, "WorkFlowStatusName", false);
            StackHashSortOrderCollection allSortOrder = new StackHashSortOrderCollection() { sortOrder };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrder, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
        }

        
        /// <summary>
        /// Search for events based on product name.
        /// One product - match - 2 criteria
        /// </summary>
        [TestMethod]
        public void SearchEventsProductIdMatch2Criteria()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// Product mismatch.
        /// </summary>
        [TestMethod]
        public void SearchEventsProductIdMismatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId + 1, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for all events with the a bugid containing "bug".
        /// Should return 2 out of 3.
        /// </summary>
        [TestMethod]
        public void SearchEventsMatch2OutOf3OnBugId()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;
            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "dont mention the word b u g");
            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 20, file1.Id, "this bug is second");
            StackHashEvent event3 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName3", eventId + 2, eventSignature, 20, file1.Id, "another bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEvent(product1, file1, event3);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "bug", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages.FindEventPackage(event2.Id, event2.EventTypeName).EventData));
            Assert.AreEqual(0, event3.CompareTo(allPackages.FindEventPackage(event3.Id, event3.EventTypeName).EventData));
        }

        /// <summary>
        /// Search for all events with the a bugid containing "bug".
        /// Should return 2 out of 3 across 2 files.
        /// </summary>
        [TestMethod]
        public void SearchEventsMatch2OutOf3OnBugIdAcross2Files()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId + 1, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;
            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "dont mention the word b u g");
            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 20, file1.Id, "this bug is second");
            StackHashEvent event3 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName3", eventId + 2, eventSignature, 20, file2.Id, "another bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEvent(product1, file2, event3);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "bug", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages.FindEventPackage(event2.Id, event2.EventTypeName).EventData));
            Assert.AreEqual(0, event3.CompareTo(allPackages.FindEventPackage(event3.Id, event3.EventTypeName).EventData));
        }


        /// <summary>
        /// Search for events based on cab info.
        /// 1 product, 1 file, 2 events - 1 cab each.
        /// 1st CAB matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsFirstCabMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);            

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThanOrEqual, fileSize, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based on cab info.
        /// 1 product, 1 file, 2 events - 1 cab each.
        /// Second CAB matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsSecondCabMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThan, fileSize, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }


        /// <summary>
        /// Search for events based on cab info.
        /// 1 product, 1 file, 2 events - 1 cab each.
        /// Both CAB matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsBothCabMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThanOrEqual, fileSize, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }


        /// <summary>
        /// Search for events based on cab info.
        /// 1 product, 1 file, 2 events - 1 cab each.
        /// No CAB matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsNoCabMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThan, fileSize, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for events based on cab info.
        /// 1 product, 1 file, 2 events - 1 cabs in each event.
        /// 1 of 2 conditions met.
        /// </summary>
        [TestMethod]
        public void SearchEventsSecondCabMatchesOn2Conditions()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThanOrEqual, fileSize, 0),
                new StringSearchOption(StackHashObjectType.CabInfo, "EventTypeName", StackHashSearchOptionType.Equal, event2.EventTypeName, "", false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }


        /// <summary>
        /// 2 cabs for same event - first matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsOneEvent2Cabs_MatchOnFirstCab()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event1, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.Equal, fileSize, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }


        /// <summary>
        /// 2 cabs for same event - second matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsOneEvent2Cabs_MatchOnSecondCab()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event1, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.Equal, fileSize + 1, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }


        /// <summary>
        /// 2 cabs for same event - both matche.
        /// </summary>
        [TestMethod]
        public void SearchEventsOneEvent2Cabs_MatchOnBothCabs()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event1, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThan, fileSize - 1, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// 2 cabs for same event - none match.
        /// </summary>
        [TestMethod]
        public void SearchEventsOneEvent2Cabs_MatchOnNeitherCab()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file1.Id, "bug2");

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName2", cabId + 1, fileSize + 1);



            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event1, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThan, fileSize, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for events based on EventInfo.
        /// 1st eventinfo matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsFirstEIMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "OperatingSystemName", StackHashSearchOptionType.Equal, "Vista32", "", false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }


        /// <summary>
        /// Search for events based on EventInfo.
        /// Both eventinfo matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsBothEIMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "OperatingSystemName", StackHashSearchOptionType.StringContains, "ista", "", false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based on EventInfo.
        /// 1st eventinfo matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsSecondEIMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.EventInfo, "Lcid", StackHashSearchOptionType.Equal, 1013, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based on EventInfo.
        /// Neither eventinfo matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsNeitherEIMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, "DD", "", false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for events based on EventInfo AND Cab.
        /// Only the cab matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnCabAndEI_OnlyCabMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, "DD", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for events based on EventInfo AND Cab.
        /// Only the event info matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnCabAndEI_OnlyEIMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, "Locale1", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cabId - 1, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for events based on EventInfo AND Cab.
        /// Both match but in different events.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnCabAndEI_BothMatchInDifferentEvents()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, "Locale1", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cabId + 1, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// Search for events based on EventInfo AND Cab.
        /// Both match but in first event.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnCabAndEI_BothMatchInFirstEvent()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, "Locale1", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based on EventInfo AND Cab.
        /// Both match but in second event.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnCabAndEI_BothMatchInSecondEvent()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, "Locale2", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cabId + 1, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based on EventInfo AND Cab.
        /// Both match but in both events.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnCabAndEI_BothMatchInBothEvents()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.StringContains, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.GreaterThanOrEqual, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based All object types - match.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnAllObjectTypes_Match()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThanOrEqual, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.LessThanOrEqual, modifiedDateTime.AddDays(1), modifiedDateTime.AddDays(1)),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.Equal, creationDateTime, creationDateTime),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.Equal, "ModuleName1", "ModuleName", false),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.StringContains, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.GreaterThanOrEqual, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
        }


        /// <summary>
        /// Search for events based All object types - first event matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnAllObjectTypes_FirstEventMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThanOrEqual, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.LessThanOrEqual, modifiedDateTime.AddDays(1), modifiedDateTime.AddDays(1)),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.Equal, creationDateTime, creationDateTime),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.Equal, "ModuleName1", "ModuleName", false),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.StringContains, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
        }

        /// <summary>
        /// Search for events based All object types - no event matches.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnAllObjectTypes_NoEventMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThanOrEqual, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.LessThanOrEqual, modifiedDateTime.AddDays(1), modifiedDateTime.AddDays(1)),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.Equal, creationDateTime, creationDateTime),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.Equal, "ModuleName1", "ModuleName", false),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.StringContains, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.LessThan, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// Search for events based All object types specifying ALL for each.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnAllObjectTypes_AllMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.All, modifiedDateTime.AddDays(1), modifiedDateTime.AddDays(1)),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.All, creationDateTime, creationDateTime),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.All, "ModuleName1", "ModuleName", false),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.All, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.All, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
            Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
            Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
            Assert.AreEqual(0, eventInfos2.CompareTo(event2Retrieved.EventInfoList));
            Assert.AreEqual(0, allCabs2.CompareTo(event2Retrieved.Cabs));
        }


        /// <summary>
        /// Search for events based All object types specifying ALL for each.
        /// Lots of events and cabs.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnAllObjectTypes_AllMatchManyCabsAndEventInfos()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId + 1, eventSignature, 21, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(2), "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(3), "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(4), "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(5), "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(6), "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-10), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-9), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-8), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-7), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-6), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-5), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3),
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(-4), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;
            int numCabs = 20;

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            for (int i = 0; i < numCabs; i++)
            {
                StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId++, fileSize++);
                StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId++, fileSize++);
                allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
                allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));
                m_Index.AddCab(product1, file1, event1, cab1, false);
                m_Index.AddCab(product1, file1, event2, cab2, false);
            }



            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.All, modifiedDateTime.AddDays(1), modifiedDateTime.AddDays(1)),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.All, creationDateTime, creationDateTime),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.All, "ModuleName1", "ModuleName", false),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.All, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.All, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
            Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
            Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
            Assert.AreEqual(0, eventInfos2.CompareTo(event2Retrieved.EventInfoList));
            Assert.AreEqual(0, allCabs2.CompareTo(event2Retrieved.Cabs));
        }
        /// <summary>
        /// Search for events based All object types specifying ALL for each - product id check.
        /// </summary>
        [TestMethod]
        public void SearchEventsOnAllObjectTypes_AllMatchProductIdCheck()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId + 1, "TestProduct2", 20, 30, "2.10.02123.2293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId + 1, creationDateTime, "File2.dll", "3.3.4.5");

            
            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, productId, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.All, modifiedDateTime.AddDays(1), modifiedDateTime.AddDays(1)),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.All, creationDateTime, creationDateTime),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.All, "ModuleName1", "ModuleName", false),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.All, "Locale", "", false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.All, cabId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
            Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
            Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
            Assert.AreEqual(productId, event1Retrieved.ProductId);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2.CompareTo(event2Retrieved.EventData));
            Assert.AreEqual(0, eventInfos2.CompareTo(event2Retrieved.EventInfoList));
            Assert.AreEqual(0, allCabs2.CompareTo(event2Retrieved.Cabs));
            Assert.AreEqual(productId + 1, event2Retrieved.ProductId);
        }


        /// <summary>
        /// Issues a search for every product field in turn - match in each case.
        /// </summary>
        [TestMethod]
        public void SearchAllProductFieldsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;
            
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId + 1, "TestProduct2", 2, 1, "2.10.02123.2291");
            product1.TotalStoredEvents = 2;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId + 1, creationDateTime, "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.Equal, product1.DateCreatedLocal, product1.DateCreatedLocal),
                new DateTimeSearchOption(StackHashObjectType.Product, "DateModifiedLocal", StackHashSearchOptionType.Equal, product1.DateModifiedLocal, product1.DateModifiedLocal),
                new StringSearchOption(StackHashObjectType.Product, "FilesLink", StackHashSearchOptionType.Equal, product1.FilesLink, product1.FilesLink, false),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product1.Id, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, product1.Name, product1.Name, false),
                new IntSearchOption(StackHashObjectType.Product, "TotalEvents", StackHashSearchOptionType.Equal, product1.TotalEvents, 0),
                new IntSearchOption(StackHashObjectType.Product, "TotalResponses", StackHashSearchOptionType.Equal, product1.TotalResponses, 0),
                new StringSearchOption(StackHashObjectType.Product, "Version", StackHashSearchOptionType.Equal, product1.Version, product1.Version, false),
                new IntSearchOption(StackHashObjectType.Product, "TotalStoredEvents", StackHashSearchOptionType.Equal, product1.TotalStoredEvents, 0),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                // Fileslink isn't in the SQL database so should wildcard and hense come back with all events.
                if (option.FieldName != "FilesLink")
                    Assert.AreEqual(1, allPackages.Count);
                else
                    Assert.AreEqual(2, allPackages.Count);

                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
                Assert.AreNotEqual(null, event1Retrieved);
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }


        /// <summary>
        /// Issues a search for every file field in turn - match in each case.
        /// </summary>
        [TestMethod]
        public void SearchAllFileFieldsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId + 1, "TestProduct2", 2, 1, "2.10.02123.2291");
            product1.TotalStoredEvents = 2;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId + 1, eventSignature, 21, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.File, "DateCreatedLocal", StackHashSearchOptionType.Equal, file1.DateCreatedLocal, file1.DateCreatedLocal),
                new DateTimeSearchOption(StackHashObjectType.File, "DateModifiedLocal", StackHashSearchOptionType.Equal, file1.DateModifiedLocal, file1.DateModifiedLocal),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file1.Id, 0),
                new DateTimeSearchOption(StackHashObjectType.File, "LinkDateLocal", StackHashSearchOptionType.Equal, file1.LinkDateLocal, file1.LinkDateLocal),
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.Equal, file1.Name, file1.Name, false),
                new StringSearchOption(StackHashObjectType.File, "Version", StackHashSearchOptionType.Equal, file1.Version, file1.Version, false),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                Assert.AreEqual(1, allPackages.Count);
                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
                Assert.AreNotEqual(null, event1Retrieved);
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }


        /// <summary>
        /// Issues a search for every event field in turn - match in each case.
        /// </summary>
        [TestMethod]
        public void SearchAllEventFieldsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId + 1, "TestProduct2", 2, 1, "2.10.02123.2291");
            product1.TotalStoredEvents = 2;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), "EventTypeName2", eventId + 1, eventSignature, 21, file2.Id, "bug2");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime.AddDays(1), "English", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.Equal, event1.DateCreatedLocal, event1.DateCreatedLocal),
                new DateTimeSearchOption(StackHashObjectType.Event, "DateModifiedLocal", StackHashSearchOptionType.Equal, event1.DateModifiedLocal, event1.DateModifiedLocal),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.Equal, event1.Id, 0),
                new StringSearchOption(StackHashObjectType.Event, "EventTypeName", StackHashSearchOptionType.Equal, event1.EventTypeName, event1.EventTypeName, false),
                new IntSearchOption(StackHashObjectType.Event, "TotalHits", StackHashSearchOptionType.Equal, event1.TotalHits, 0),
                new IntSearchOption(StackHashObjectType.Event, "FileId", StackHashSearchOptionType.Equal, event1.FileId, 0),
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.Equal, event1.BugId, event1.BugId, false),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                if (String.Compare("FileId", option.FieldName, StringComparison.OrdinalIgnoreCase) == 0)
                    Assert.AreEqual(2, allPackages.Count);
                else
                    Assert.AreEqual(1, allPackages.Count);
                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
                Assert.AreNotEqual(null, event1Retrieved);
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }
        /// <summary>
        /// Issues a search for every event info field in turn - match in each case.
        /// </summary>
        [TestMethod]
        public void SearchAllEventInfoFieldsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId + 1, "TestProduct2", 2, 1, "2.10.02123.2291");
            product1.TotalStoredEvents = 2;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), "EventTypeName2", eventId + 1, eventSignature, 21, file2.Id, "bug2");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), hitDateTime.AddDays(1), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.EventInfo, "DateCreatedLocal", StackHashSearchOptionType.Equal, eventInfos1[0].DateCreatedLocal, eventInfos1[0].DateCreatedLocal),
                new DateTimeSearchOption(StackHashObjectType.EventInfo, "DateModifiedLocal", StackHashSearchOptionType.Equal, eventInfos1[0].DateModifiedLocal, eventInfos1[0].DateModifiedLocal),
                new DateTimeSearchOption(StackHashObjectType.EventInfo, "HitDateLocal", StackHashSearchOptionType.Equal, eventInfos1[0].HitDateLocal, eventInfos1[0].HitDateLocal),
                new StringSearchOption(StackHashObjectType.EventInfo, "Language", StackHashSearchOptionType.Equal, eventInfos1[0].Language, eventInfos1[0].Language, false),
                new IntSearchOption(StackHashObjectType.EventInfo, "Lcid", StackHashSearchOptionType.Equal, eventInfos1[0].Lcid, 0),
                new StringSearchOption(StackHashObjectType.EventInfo, "Locale", StackHashSearchOptionType.Equal, eventInfos1[0].Locale, eventInfos1[0].Locale, false),
                new StringSearchOption(StackHashObjectType.EventInfo, "OperatingSystemName", StackHashSearchOptionType.Equal, eventInfos1[0].OperatingSystemName, eventInfos1[0].OperatingSystemName, false),
                new StringSearchOption(StackHashObjectType.EventInfo, "OperatingSystemVersion", StackHashSearchOptionType.Equal, eventInfos1[0].OperatingSystemVersion, eventInfos1[0].OperatingSystemVersion, false),
                new IntSearchOption(StackHashObjectType.EventInfo, "TotalHits", StackHashSearchOptionType.Equal, eventInfos1[0].TotalHits, 0),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                Assert.AreEqual(1, allPackages.Count);
                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
                Assert.AreNotEqual(null, event1Retrieved);
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }

        /// <summary>
        /// Issues a search for every cab field in turn - match in each case.
        /// </summary>
        [TestMethod]
        public void SearchAllCabFieldsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId + 1, "TestProduct2", 2, 1, "2.10.02123.2291");
            product1.TotalStoredEvents = 2;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), "EventTypeName2", eventId + 1, eventSignature, 21, file2.Id, "bug2");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), hitDateTime.AddDays(1), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.CabInfo, "DateCreatedLocal", StackHashSearchOptionType.Equal, cab1.DateCreatedLocal, cab1.DateCreatedLocal),
                new DateTimeSearchOption(StackHashObjectType.CabInfo, "DateModifiedLocal", StackHashSearchOptionType.Equal, cab1.DateModifiedLocal, cab1.DateModifiedLocal),
                new IntSearchOption(StackHashObjectType.CabInfo, "EventId", StackHashSearchOptionType.Equal, cab1.EventId, 0),
                new StringSearchOption(StackHashObjectType.CabInfo, "EventTypeName", StackHashSearchOptionType.Equal, cab1.EventTypeName, cab1.EventTypeName, false),
                new StringSearchOption(StackHashObjectType.CabInfo, "FileName", StackHashSearchOptionType.Equal, cab1.FileName, cab1.FileName, false),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, cab1.Id, 0),
                new LongSearchOption(StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.Equal, cab1.SizeInBytes, 0),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                Assert.AreEqual(1, allPackages.Count);
                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
                Assert.AreNotEqual(null, event1Retrieved);
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }

        /// <summary>
        /// Issues a search for every event signature field in turn - match in each case.
        /// </summary>
        [TestMethod]
        public void SearchAllEventSignatureFieldsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), null, productId + 1, "TestProduct2", 2, 1, "2.10.02123.2291");
            product1.TotalStoredEvents = 2;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), "EventTypeName2", eventId + 1, eventSignature2, 21, file2.Id, "bug2");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), hitDateTime.AddDays(1), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            StackHashCab cab2 = new StackHashCab(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), event2.Id, event2.EventTypeName, "FileName2", cabId + 1, fileSize + 1);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));
            allCabs2.Add(new StackHashCabPackage(cab2, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product2, file2, event2, cab2, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventSignature, "ApplicationName", StackHashSearchOptionType.Equal, eventSignature.ApplicationName, eventSignature.ApplicationName, false),
                new StringSearchOption(StackHashObjectType.EventSignature, "ApplicationVersion", StackHashSearchOptionType.Equal, eventSignature.ApplicationVersion, eventSignature.ApplicationVersion, false),
                new DateTimeSearchOption(StackHashObjectType.EventSignature, "ApplicationTimeStamp", StackHashSearchOptionType.Equal, eventSignature.ApplicationTimeStamp, eventSignature.ApplicationTimeStamp),

                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.Equal, eventSignature.ModuleName, eventSignature.ModuleName, false),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleVersion", StackHashSearchOptionType.Equal, eventSignature.ModuleVersion, eventSignature.ModuleVersion, false),
                new DateTimeSearchOption(StackHashObjectType.EventSignature, "ModuleTimeStamp", StackHashSearchOptionType.Equal, eventSignature.ModuleTimeStamp, eventSignature.ModuleTimeStamp),

                new LongSearchOption(StackHashObjectType.EventSignature, "Offset", StackHashSearchOptionType.Equal, eventSignature.Offset, 0),
                new LongSearchOption(StackHashObjectType.EventSignature, "ExceptionCode", StackHashSearchOptionType.Equal, eventSignature.ExceptionCode, 0),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                Assert.AreEqual(1, allPackages.Count);
                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
                Assert.AreNotEqual(null, event1Retrieved);
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }
        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - date match.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOnDate()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThanOrEqual, creationDateTime, creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }


        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - date match - although time different.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOnDate_TimeMismatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.Equal, creationDateTime.AddMinutes(-10), creationDateTime.AddMinutes(-10))
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - date mismatch.
        /// </summary>
        [TestMethod]
        public void SearchProductNoMatchOnDate()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, creationDateTime, creationDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
            Assert.AreEqual(false, allProductIds.Contains(productId));
        }


        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - date match - but date is less than SQL date.
        /// DateTime: SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM
        /// SmallDateTime: January 1, 1900, through June 6, 2079
        /// </summary>
        [TestMethod]
        public void SearchProductNoMatchOnInvalidDateMin()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime searchDateTime = new DateTime(1752, 04, 04, 22, 9, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.LessThan, searchDateTime, searchDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
            Assert.AreEqual(false, allProductIds.Contains(productId));
        }

        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - date match - but date is greater than SQL date.
        /// DateTime: SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM
        /// SmallDateTime: January 1, 1900, through June 6, 2079
        /// </summary>
        [TestMethod]
        public void SearchProductNoMatchOnInvalidDateMax()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime searchDateTime = new DateTime(2079, 12, 12, 22, 9, 0, DateTimeKind.Utc);

            int productId = 20;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, searchDateTime, searchDateTime)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
            Assert.AreEqual(false, allProductIds.Contains(productId));
        }
        
        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - product name match - unicode.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOnUnicodeProductName()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "UnicodeProductName\ufe98\ufbfc";
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.LessThanOrEqual, productName, productName, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }


        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - product name NO match - unicode.
        /// </summary>
        [TestMethod]
        public void SearchProductNoMatchOnUnicodeProductName()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "UnicodeProductName\ufe98\ufbfc";
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, productName + "F", productName, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(0, allProductIds.Count);
            Assert.AreEqual(false, allProductIds.Contains(productId));
        }

        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - product name match - unicode - <=.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOnUnicodeProductNameLessOrEqual()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productName = "UnicodeProductName\ufe98\ufbfc";
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.LessThanOrEqual, productName + "F", productName, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - product name match - unicode - >=.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOnUnicodeProductNameGreaterOrEqual()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productNameReduced = "UnicodeProductName\ufe98";
            String productName = productNameReduced + "ufbfc";
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.GreaterThanOrEqual, productNameReduced, productName, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }

        /// <summary>
        /// Search for products based on product criteria.
        /// One Product - product name match - unicode - >.
        /// </summary>
        [TestMethod]
        public void SearchProductMatchOnUnicodeProductNameGreater()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            String productNameReduced = "UnicodeProductName\ufe98";
            String productName = productNameReduced + "ufbfc";
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, productName, 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);

            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.GreaterThan, productNameReduced, productName, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String searchString = allCriteria.ToSqlString(StackHashObjectType.Product, "P");

            Collection<int> allProductIds = m_Index.GetProductMatch(searchString);
            Assert.AreEqual(1, allProductIds.Count);
            Assert.AreEqual(true, allProductIds.Contains(productId));
        }


        /// <summary>
        /// Same event recorded against 2 files.
        /// </summary>
        [TestMethod]
        public void SearchProductIdSameEventForDifferentFiles()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), hitDateTime.AddDays(1), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            int fileSize = 1000;
            int cabId = 25;

            StackHashCabPackageCollection allCabs1 = new StackHashCabPackageCollection();
            StackHashCabPackageCollection allCabs2 = new StackHashCabPackageCollection();
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "FileName1", cabId, fileSize);
            allCabs1.Add(new StackHashCabPackage(cab1, null, null, true));

            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event1);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddCab(product1, file1, event1, cab1, false);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product1.Id, 0),
            };

            foreach (StackHashSearchOption option in allOptions)
            {
                StackHashSearchOptionCollection optionCollection = new StackHashSearchOptionCollection();
                optionCollection.Add(option);

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(optionCollection)
                };

                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

                Assert.AreNotEqual(null, allPackages);

                Assert.AreEqual(1, allPackages.Count);
                StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);

                Assert.AreNotEqual(null, event1Retrieved);

                // The file ids may be different.
                event1.FileId = event1Retrieved.EventData.FileId;
                Assert.AreEqual(0, event1.CompareTo(event1Retrieved.EventData));
                Assert.AreEqual(0, eventInfos1.CompareTo(event1Retrieved.EventInfoList));
                Assert.AreEqual(0, allCabs1.CompareTo(event1Retrieved.Cabs));
                Assert.AreEqual(productId, event1Retrieved.ProductId);
            }

        }

        /// <summary>
        /// Same eventId recorded against different products but with different EventTypeName.
        /// Match - but in the wrong one.
        /// Fog 934.
        /// </summary>
        [TestMethod]
        public void SearchSameEventIdButDifferentEventTypeNameInDifferentProducts()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink2", productId + 1, "TestProduct2", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;

            
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product2, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product2, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product2, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product1.Id, 0),
                new DateTimeSearchOption(StackHashObjectType.EventInfo, "HitDateLocal", StackHashSearchOptionType.LessThan, hitDateTime.AddDays(-1), hitDateTime),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(0, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreEqual(null, event1Retrieved);
        }


        /// <summary>
        /// Search for events that don't match the first file name.
        /// </summary>
        [TestMethod]
        public void SearchEventsThatDontMatchFirstFileName()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.NotEqual, file1.Name, null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreEqual(null, event1Retrieved);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2Retrieved.EventData.CompareTo(event2));
        }


        /// <summary>
        /// Search for events that don't match the second file name.
        /// </summary>
        [TestMethod]
        public void SearchEventsThatDontMatchSecondFileName()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.NotEqual, file2.Name, null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);
            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreEqual(null, event2Retrieved);
        }

        /// <summary>
        /// Search for events that don't match the either file name.
        /// </summary>
        [TestMethod]
        public void SearchEventsThatDontMatchNeitherFileName()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.NotEqual, file2.Name + "jghjH", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2Retrieved.EventData.CompareTo(event2));
        }

        /// <summary>
        /// Search for events that don't match the both file names - no events should be returned.
        /// </summary>
        [TestMethod]
        public void SearchEventsThatDontMatchBothFileName()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.NotEqual, file2.Name, null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for String not like first filename.
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameDoesNotContain_FirstFileMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringDoesNotContain, "1", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreEqual(null, event1Retrieved);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2Retrieved.EventData.CompareTo(event2));
        }

        /// <summary>
        /// Search for String not like second filename.
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameDoesNotContain_SecondFileMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringDoesNotContain, "2", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreEqual(null, event2Retrieved);
        }


        /// <summary>
        /// Search for String not like neither
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameDoesNotContain_NeitherFileMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringDoesNotContain, "3", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(2, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2Retrieved.EventData.CompareTo(event2));
        }

        /// <summary>
        /// Search for String not like both
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameDoesNotContain_BothFileMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringDoesNotContain, "ile", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for String wildcard % - 1 match.
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNamePercentWildcardSearch_FirstMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringContains, "%1%", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreEqual(null, event2Retrieved);
        }

        /// <summary>
        /// Search for String wildcard _ - 1 match.
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameUnderscoreWildcardSearch_SecondMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringContains, "____2.___", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreEqual(null, event1Retrieved);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2Retrieved.EventData.CompareTo(event2));
        }


        /// <summary>
        /// Search for String wildcard _ - none match.
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameUnderscoreWildcardSearch_NoMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, "bug");

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.File, "Name", StackHashSearchOptionType.StringContains, "F__le2._l_", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for String not like first filename.
        /// One event has a NULL for the string - this should still be returned.
        /// </summary>
        [TestMethod]
        public void SearchEventsFileNameDoesNotContain_FirstFileMatches_NullFields()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;



            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(1), fileId + 1, creationDateTime.AddDays(1), "File2.dll", "3.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.41"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(1).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1235"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1236"));
            eventSignature2.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", eventId, eventSignature, 20, file2.Id, null);

            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime.AddDays(-10), modifiedDateTime.AddDays(-10), hitDateTime.AddDays(-10), "English2", 1013, "Locale2", "Vista64", "1.2.3.5", 3)
            };  

            m_Index.AddProduct(product1, true);
            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event2);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file2, event2, eventInfos2);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringDoesNotContain, "bug", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreEqual(null, event1Retrieved);

            StackHashEventPackage event2Retrieved = allPackages.FindEventPackage(event2.Id, event2.EventTypeName);
            Assert.AreNotEqual(null, event2Retrieved);
            Assert.AreEqual(0, event2Retrieved.EventData.CompareTo(event2));
        }


        /// <summary>
        /// Criteria map check. Event matches criteria 1.
        /// </summary>
        [TestMethod]
        public void SearchEvents_CriteriaMapCheck_EventMatchesCriteria1()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;


            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };


            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "bug", null, false),
            };
            StackHashSearchOptionCollection allOptions2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringDoesNotContain, "bug", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions),
                new StackHashSearchCriteria(allOptions2)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }


        /// <summary>
        /// Criteria map check. Event matches criteria 2.
        /// </summary>
        [TestMethod]
        public void SearchEvents_CriteriaMapCheck_EventMatchesCriteria2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;


            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };


            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringDoesNotContain, "bug", null, false),
            };
            StackHashSearchOptionCollection allOptions2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "bug", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions),
                new StackHashSearchCriteria(allOptions2)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));
            Assert.AreEqual(2, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Criteria map check. Event matches criteria 1 and 2.
        /// The CriteriaMap should show 3 (i.e. 11 binary).
        /// </summary>
        [TestMethod]
        public void SearchEvents_CriteriaMapCheck_EventMatchesCriteria1And2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;


            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");


            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 100;

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId++, eventSignature, 20, file1.Id, "bug");

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
            {
                new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", 10),
            };


            m_Index.AddProduct(product1, true);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashSearchOptionCollection allOptions = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "bug", null, false),
            };
            StackHashSearchOptionCollection allOptions2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.StringContains, "bug", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(allOptions),
                new StackHashSearchCriteria(allOptions2)
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, null);

            Assert.AreNotEqual(null, allPackages);

            Assert.AreEqual(1, allPackages.Count);

            StackHashEventPackage event1Retrieved = allPackages.FindEventPackage(event1.Id, event1.EventTypeName);
            Assert.AreNotEqual(null, event1Retrieved);
            Assert.AreEqual(0, event1Retrieved.EventData.CompareTo(event1));
            Assert.AreEqual(3, allPackages[0].CriteriaMatchMap);
        }
    }

}
