using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SqlFilesUnitTests
    /// </summary>
    [TestClass]
    public class SqlFilesUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlFilesUnitTests()
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
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\StackHash_TestCabs";

            if (!Directory.Exists(m_RootCabFolder))
                Directory.CreateDirectory(m_RootCabFolder);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            SqlConnection.ClearAllPools();
            if (m_Index != null)
            {
                m_Index.Deactivate();
                m_Index.DeleteIndex();
                m_Index.Dispose();
                m_Index = null;
            }

            if (Directory.Exists(m_RootCabFolder))
                PathUtils.DeleteDirectory(m_RootCabFolder, true);
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void AddFileNullProduct()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            try
            {
                m_Index.Activate();
                StackHashFile file = new StackHashFile();

                m_Index.AddFile(null, file);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void AddFileNullFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            try
            {
                m_Index.Activate();
                
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

                m_Index.AddProduct(product1);

                m_Index.AddFile(product1, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        //[TestMethod]
        //public void AddOneFileGetFileRepeat()
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        if (i != 0)
        //            MyTestInitialize();
        //        AddOneFileGetFile();
        //        MyTestCleanup();
                    
        //    }
        //}

        [TestMethod]
        public void AddOneFileGetFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashFile dbFile1 = m_Index.GetFile(product1, file1.Id);
            Assert.AreEqual(0, file1.CompareTo(dbFile1));
        }

        [TestMethod]
        public void FileExistsForProduct()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashFile dbFile1 = m_Index.GetFile(product1, file1.Id);
            Assert.AreEqual(0, file1.CompareTo(dbFile1));

            Assert.AreEqual(true, m_Index.FileExists(product1, file1));
        }


        [TestMethod]
        public void FileDoesntExist()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashFile dbFile1 = m_Index.GetFile(product1, file1.Id);
            Assert.AreEqual(0, file1.CompareTo(dbFile1));

            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");
            Assert.AreEqual(false, m_Index.FileExists(product1, file2));
            Assert.AreEqual(true, m_Index.FileExists(product1, file1));
        }


        /// <summary>
        /// File exists but for a different product so Index.FileExists should return false so 
        /// it can be added for the second product.
        /// </summary>
        [TestMethod]
        public void FileExistsForDifferentProduct()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 2, "TestProduct1", 21, 31, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product1, file1);

            StackHashFile dbFile1 = m_Index.GetFile(product1, file1.Id);
            Assert.AreEqual(0, file1.CompareTo(dbFile1));

            Assert.AreEqual(true, m_Index.FileExists(product1, file1));
            Assert.AreEqual(false, m_Index.FileExists(product2, file1));
        }

        /// <summary>
        /// File exists in 2 products.
        /// </summary>
        [TestMethod]
        public void FileExistsForTwoProducts()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 2, "TestProduct1", 21, 31, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file1);

            StackHashFile dbFile1 = m_Index.GetFile(product1, file1.Id);
            Assert.AreEqual(0, file1.CompareTo(dbFile1));

            Assert.AreEqual(true, m_Index.FileExists(product1, file1));
            Assert.AreEqual(true, m_Index.FileExists(product2, file1));
        }
   


        [TestMethod]
        public void UpdateFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            StackHashFile dbFile1 = m_Index.GetFile(product1, file1.Id);
            Assert.AreEqual(0, file1.CompareTo(dbFile1));

            StackHashFile file2 =
                new StackHashFile(creationDateTime.AddDays(1), modifiedDateTime.AddDays(2), 20, creationDateTime.AddDays(3), "File2.dll", "1.3.4.5");

            m_Index.AddFile(product1, file2);
            StackHashFile dbFile2 = m_Index.GetFile(product1, file2.Id);
            Assert.AreEqual(0, file2.CompareTo(dbFile2));
            Assert.AreNotEqual(0, file1.CompareTo(dbFile2));
        }

        [TestMethod]
        public void GetFilesForProductNoFiles()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashFileCollection files = m_Index.LoadFileList(product1);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(0, files.Count);
        }


        [TestMethod]
        public void GetFilesForProductOneFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashFileCollection files = m_Index.LoadFileList(product1);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(0, file1.CompareTo(files[0]));
        }

        [TestMethod]
        public void GetFilesForProduct2FilesOnly1ForSelectedProduct()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 2, "TestProduct1", 21, 31, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);

            StackHashFileCollection files = m_Index.LoadFileList(product1);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(0, file1.CompareTo(files[0]));

            files = m_Index.LoadFileList(product2);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(0, file2.CompareTo(files[0]));
        }


        [TestMethod]
        public void GetFilesForProduct2FilesForSelectedProduct()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 2, "TestProduct1", 21, 31, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");
            StackHashFile file3 =
                new StackHashFile(creationDateTime, modifiedDateTime, 22, creationDateTime, "File3.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product2, file2);
            m_Index.AddFile(product2, file3);

            StackHashFileCollection files = m_Index.LoadFileList(product1);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(0, file1.CompareTo(files[0]));

            files = m_Index.LoadFileList(product2);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(2, files.Count);
            Assert.AreEqual(0, file2.CompareTo(files[0]));
            Assert.AreEqual(0, file3.CompareTo(files[1]));
        }


        [TestMethod]
        public void GetFilesForProduct2ProductsWithSameFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 2, "TestProduct1", 21, 31, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");
            StackHashFile file3 =
                new StackHashFile(creationDateTime, modifiedDateTime, 22, creationDateTime, "File3.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddFile(product2, file2);
            m_Index.AddFile(product2, file3);

            StackHashFileCollection files = m_Index.LoadFileList(product1);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(2, files.Count);
            Assert.AreEqual(0, file1.CompareTo(files[0]));
            Assert.AreEqual(0, file2.CompareTo(files[1]));

            files = m_Index.LoadFileList(product2);
            Assert.AreNotEqual(null, files);
            Assert.AreEqual(2, files.Count);
            Assert.AreEqual(0, file2.CompareTo(files[0]));
            Assert.AreEqual(0, file3.CompareTo(files[1]));
        }

        [TestMethod]
        public void Add100Files()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            int numFiles = 100;

            m_Index.AddProduct(product1);

            List<StackHashFile> addedFiles = new List<StackHashFile>();

            for (int i = 0; i < numFiles; i++)
            {
                StackHashFile file1 =
                    new StackHashFile(creationDateTime, modifiedDateTime, 20 + i, creationDateTime, "File1.dll", "2.3.4.5");

                m_Index.AddFile(product1, file1);
                addedFiles.Add(file1);

                StackHashFileCollection files = m_Index.LoadFileList(product1);
                Assert.AreNotEqual(null, files);
                Assert.AreEqual(i + 1, files.Count);

                for (int j = 0; j < i; j++)
                {
                    Assert.AreEqual(0, addedFiles[j].CompareTo(files[j]));
                }
            }

        }


        public void filesCount(int numFiles)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            m_Index.AddProduct(product1);
            for (int i = 0; i < numFiles; i++)
            {
                StackHashFile file1 =
                    new StackHashFile(creationDateTime, modifiedDateTime, 20 + i, creationDateTime, "File1.dll", "2.3.4.5");

                m_Index.AddFile(product1, file1);
            }

            long filesCount = m_Index.TotalFiles;

            Assert.AreEqual(numFiles, filesCount);
        }

        [TestMethod]
        public void FilesCount0()
        {
            filesCount(0);
        }

        [TestMethod]
        public void FilesCount1()
        {
            filesCount(1);
        }
        [TestMethod]
        public void FilesCount2()
        {
            filesCount(2);
        }
        [TestMethod]
        public void FilesCount190()
        {
            filesCount(100);
        }
    }
}
