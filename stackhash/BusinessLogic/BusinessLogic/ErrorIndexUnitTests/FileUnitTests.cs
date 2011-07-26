using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;


namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for FileUnitTests
    /// </summary>
    [TestClass]
    public class FileUnitTests
    {
        string m_TempPath;

        public FileUnitTests()
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
            m_TempPath = Path.GetTempPath() + "StackHashTest_ErrorIndex";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath))
                PathUtils.DeleteDirectory(m_TempPath, true);
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

        private void testAddFileNullPruduct(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashFile file = new StackHashFile();

                index.AddFile(null, file);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddFileNullPruduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddFileNullPruduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddFileNullPruductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddFileNullPruduct(indexCache);
        }

        public void testAddFileNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www", 25, "Product", 1, 2, "1.2.3.4");

                index.AddProduct(product);
                index.AddFile(product, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddFileNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddFileNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddFileNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddFileNullFile(indexCache);
        }


        public void testAddFileProductDoesntExist(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www", 25, "Product", 1, 2, "1.2.3.4");
                StackHashFile file = new StackHashFile();

                index.AddFile(product, file);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddFileProductDoesntExist()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddFileProductDoesntExist(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddFileProductDoesntExistCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddFileProductDoesntExist(indexCache);
        }


        // A single File object which hasn't been initialised. Should reject.
        private void testAdd1FileDefaultData(IErrorIndex index)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            // Default file data - should fail.
            StackHashFile file = new StackHashFile();

            index.AddProduct(product);

            try
            {
                index.AddFile(product, file);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }

            // Should be no files.
            StackHashFileCollection files = index.LoadFileList(product);
            Assert.AreEqual(0, files.Count);

            // Check that the dates are stored in UTC.
            Assert.AreEqual(true, files[0].DateCreatedLocal.Kind == DateTimeKind.Utc);
            Assert.AreEqual(true, files[0].DateModifiedLocal.Kind == DateTimeKind.Utc);

        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAdd1FileDefaultData()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAdd1FileDefaultData(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAdd1FileDefaultDataCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAdd1FileDefaultData(indexCache);
        }

        private void testAdd1FileOk(IErrorIndex index)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");


            index.AddProduct(product);
            index.AddFile(product, file);

            StackHashFileCollection files = index.LoadFileList(product);

            Assert.AreEqual(1, files.Count);

            Assert.AreEqual(file.DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(file.DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(file.Id, files[0].Id);
            Assert.AreEqual(file.LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(file.Name, files[0].Name);
            Assert.AreEqual(file.Version, files[0].Version);


            // Also test the GetFile interface.
            StackHashFile gotFile = index.GetFile(product, file.Id);

            Assert.AreNotEqual(null, gotFile);

            Assert.AreEqual(file.DateCreatedLocal, gotFile.DateCreatedLocal);
            Assert.AreEqual(file.DateModifiedLocal, gotFile.DateModifiedLocal);
            Assert.AreEqual(file.Id, gotFile.Id);
            Assert.AreEqual(file.LinkDateLocal, gotFile.LinkDateLocal);
            Assert.AreEqual(file.Name, gotFile.Name);
            Assert.AreEqual(file.Version, gotFile.Version);

        }

        [TestMethod]
        public void TestAdd1FileOk()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAdd1FileOk(index);
        }

        [TestMethod]
        public void TestAdd1FileOkCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAdd1FileOk(indexCache);
        }


        // Add the same file twice with different data - should replace.
        // A file is the same if its ID, filename and version are the same.
        public void testAddSameFileTwice(IErrorIndex index)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            // Use same ID - should replace existing file data.
            file = new StackHashFile(new DateTime(200), new DateTime(201), 1, new DateTime(101), "filename.dll", "1.2.3.4");
            index.AddFile(product, file);

            StackHashFileCollection files = index.LoadFileList(product);

            Assert.AreEqual(1, files.Count);

            Assert.AreEqual(file.DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(file.DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(file.Id, files[0].Id);
            Assert.AreEqual(file.LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(file.Name, files[0].Name);
            Assert.AreEqual(file.Version, files[0].Version);
        }

        [TestMethod]
        public void TestAddSameFileTwice()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddSameFileTwice(index);
        }

        [TestMethod]
        public void TestAddSameFileTwiceCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddSameFileTwice(indexCache);
        }


        public void testAddSameFileTwiceWithReset(bool useCache)
        {
            IErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            if (useCache)
                index = new ErrorIndexCache(index);


            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            // Reload.
            index = new XmlErrorIndex(m_TempPath, "Cucku");
            if (useCache)
                index = new ErrorIndexCache(index);

            index.Activate();

            // Use same ID - should replace existing file data.
            file = new StackHashFile(new DateTime(200), new DateTime(201), 1, new DateTime(101), "filename.dll", "1.2.3.4");
            index.AddFile(product, file);

            StackHashFileCollection files = index.LoadFileList(product);

            Assert.AreEqual(1, files.Count);

            Assert.AreEqual(file.DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(file.DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(file.Id, files[0].Id);
            Assert.AreEqual(file.LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(file.Name, files[0].Name);
            Assert.AreEqual(file.Version, files[0].Version);
        }

        [TestMethod]
        public void TestAddSameFileTwiceWithReset()
        {
            testAddSameFileTwiceWithReset(false);
        }

        [TestMethod]
        public void TestAddSameFileTwiceWithResetCached()
        {
            testAddSameFileTwiceWithReset(true);
        }

        // Add N files to a single product.
        public void testAddNFiles(IErrorIndex index, int numFiles)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            List<StackHashFile> allFiles = new List<StackHashFile>();

            index.AddProduct(product);

            for (int i = 0; i < numFiles; i++)
            {
                StackHashFile file = new StackHashFile(new DateTime(100 + i), new DateTime(101 + i), 1 + i, 
                    new DateTime(102 + i), "filename.dll" + i.ToString(), "1.2.3.4");
                allFiles.Add(file);
                index.AddFile(product, file);
            }

            // Now make sure all files appear in the database list.
            StackHashFileCollection files = index.LoadFileList(product);

            Assert.AreEqual(numFiles, files.Count);

            // Note this only works because the files are in alphabetical order.
            for (int i = 0; i < numFiles; i++)
            {
                StackHashFile thisFile = index.GetFile(product, 1 + i);
                Assert.AreEqual(allFiles[i].DateCreatedLocal, thisFile.DateCreatedLocal);
                Assert.AreEqual(allFiles[i].DateModifiedLocal, thisFile.DateModifiedLocal);
                Assert.AreEqual(allFiles[i].Id, thisFile.Id);
                Assert.AreEqual(allFiles[i].LinkDateLocal, thisFile.LinkDateLocal);
                Assert.AreEqual(allFiles[i].Name, thisFile.Name);
                Assert.AreEqual(allFiles[i].Version, thisFile.Version);

                thisFile = files.FindFile(allFiles[i].Id);
                Assert.AreNotEqual(null, thisFile);

                Assert.AreEqual(allFiles[i].DateCreatedLocal, thisFile.DateCreatedLocal);
                Assert.AreEqual(allFiles[i].DateModifiedLocal, thisFile.DateModifiedLocal);
                Assert.AreEqual(allFiles[i].Id, thisFile.Id);
                Assert.AreEqual(allFiles[i].LinkDateLocal, thisFile.LinkDateLocal);
                Assert.AreEqual(allFiles[i].Name, thisFile.Name);
                Assert.AreEqual(allFiles[i].Version, thisFile.Version);
            }
        }

        [TestMethod]
        public void TestAdd2Files()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddNFiles(index, 2);
        }

        [TestMethod]
        public void TestAdd2FilesCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNFiles(indexCache, 2);
        }

        [TestMethod]
        public void TestAdd100Files()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddNFiles(index, 100);
        }

        [TestMethod]
        public void TestAdd100FilesCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNFiles(indexCache, 100);
        }

        private void testAdd1FileInFirstOfTwoProducts(IErrorIndex index)
        {
            index.Activate();
            int product1Id = 1000;
            int product2Id = 1001;
            int fileId = 100;
            StackHashProduct product1 =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", product1Id, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", product2Id, "TestProduct2", 20, 30, "2.10.02123.1293");

            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), fileId, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product1);
            index.AddProduct(product2);
            index.AddFile(product1, file);

            // Product 2 doesn't have any files.
            StackHashFileCollection files = index.LoadFileList(product2);
            Assert.AreEqual(0, files.Count);

            // Product 1 should have 1 file.
            files = index.LoadFileList(product1);
            Assert.AreEqual(1, files.Count);

            Assert.AreEqual(files[0].DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(files[0].DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(files[0].Id, files[0].Id);
            Assert.AreEqual(files[0].LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(files[0].Name, files[0].Name);
            Assert.AreEqual(files[0].Version, files[0].Version);

            StackHashFile thisFile = index.GetFile(product2, fileId);
            Assert.AreEqual(null, thisFile);

            thisFile = index.GetFile(product1, fileId);
            Assert.AreEqual(file.DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(file.DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(file.Id, files[0].Id);
            Assert.AreEqual(file.LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(file.Name, files[0].Name);
            Assert.AreEqual(file.Version, files[0].Version);

        }

        [TestMethod]
        public void TestAdd1FileInFirstOfTwoProducts()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAdd1FileInFirstOfTwoProducts(index);
        }

        [TestMethod]
        public void TestAdd1FileInFirstOfTwoProductsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAdd1FileInFirstOfTwoProducts(indexCache);
        }


        private void testAdd1FileInSecondOfTwoProducts(IErrorIndex index)
        {
            index.Activate();
            int product1Id = 1000;
            int product2Id = 1001;
            int fileId = 100;
            StackHashProduct product1 =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", product1Id, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", product2Id, "TestProduct2", 20, 30, "2.10.02123.1293");

            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), fileId, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product1);
            index.AddProduct(product2);
            index.AddFile(product2, file);

            // Product 1 doesn't have any files.
            StackHashFileCollection files = index.LoadFileList(product1);
            Assert.AreEqual(0, files.Count);

            // Product 2 should have 1 file.
            files = index.LoadFileList(product2);
            Assert.AreEqual(1, files.Count);

            Assert.AreEqual(files[0].DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(files[0].DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(files[0].Id, files[0].Id);
            Assert.AreEqual(files[0].LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(files[0].Name, files[0].Name);
            Assert.AreEqual(files[0].Version, files[0].Version);

            StackHashFile thisFile = index.GetFile(product1, fileId);
            Assert.AreEqual(null, thisFile);

            thisFile = index.GetFile(product2, fileId);
            Assert.AreEqual(file.DateCreatedLocal, files[0].DateCreatedLocal);
            Assert.AreEqual(file.DateModifiedLocal, files[0].DateModifiedLocal);
            Assert.AreEqual(file.Id, files[0].Id);
            Assert.AreEqual(file.LinkDateLocal, files[0].LinkDateLocal);
            Assert.AreEqual(file.Name, files[0].Name);
            Assert.AreEqual(file.Version, files[0].Version);

        }

        [TestMethod]
        public void TestAdd1FileInSecondOfTwoProducts()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAdd1FileInFirstOfTwoProducts(index);
        }

        [TestMethod]
        public void TestAdd1FileInSecondOfTwoProductsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAdd1FileInFirstOfTwoProducts(indexCache);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadFileListNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            index.Activate();
            index.LoadFileList(null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadFileListNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            indexCache.Activate();
            indexCache.LoadFileList(null);
        }


        public void testLoadFileListUnknownProduct(IErrorIndex index)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            try
            {
                index.LoadFileList(product);
            }
            catch (System.ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadFileListUnknownProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadFileListUnknownProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadFileListUnknownProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadFileListUnknownProduct(index);
        }

        private void testLoadFileListEmpty(IErrorIndex index)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            index.AddProduct(product);

            StackHashFileCollection files = index.LoadFileList(product);

            Assert.AreNotEqual(null, files);
            Assert.AreEqual(0, files.Count);
        }

        [TestMethod]
        public void TestLoadFileListEmpty()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testLoadFileListEmpty(index);
        }

        [TestMethod]
        public void TestLoadFileListEmptyCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);

            testLoadFileListEmpty(indexCache);
        }

        private void testFileListExists(IErrorIndex index, bool expectedResult)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            index.AddProduct(product);

            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "FileName", "1.2.3.4");
           
            file.Id = 1;

            if (expectedResult)
                index.AddFile(product, file);

            Assert.AreEqual(expectedResult, index.FileExists(product, file));
        }

        [TestMethod]
        public void TestFileListExistsNo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testFileListExists(index, false);
        }

        [TestMethod]
        public void TestFileListExistsNoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);

            testFileListExists(indexCache, false);
        }

        [TestMethod]
        public void XmlIndexGetGetFileFolderValidChars()
        {
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 123456, DateTime.Now, "FileName", "1.2.3.4");

            String filePath = XmlErrorIndex.GetFileFolder(file);

            Assert.AreEqual("F_123456_FileName_1.2.3.4", filePath);
        }

        [TestMethod]
        public void XmlIndexGetGetFileFolderInvalidChars()
        {
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 123456, DateTime.Now, @"F""i:l?e:n\a/me@(XP/2k)", "1.2.3.4");

            String filePath = XmlErrorIndex.GetFileFolder(file);

            Assert.AreEqual("F_123456_F_i_l_e_n_a_me@(XP_2k)_1.2.3.4", filePath);
        }

        [TestMethod]
        public void XmlIndexGetGetFileFolderInvalidCharsInVersion()
        {
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 123456, DateTime.Now, @"F""i:l?e:n\a/me@(XP/2k)", "1:2/3/4");

            String filePath = XmlErrorIndex.GetFileFolder(file);

            Assert.AreEqual("F_123456_F_i_l_e_n_a_me@(XP_2k)_1_2_3_4", filePath);
        }
    }
}
