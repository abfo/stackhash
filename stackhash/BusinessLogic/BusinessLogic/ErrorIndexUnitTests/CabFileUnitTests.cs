using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for CabFileUnitTests
    /// </summary>
    [TestClass]
    public class CabFileUnitTests
    {
        string m_TempPath;

        public CabFileUnitTests()
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

        //================================================================================================================================
        // GET CAB FOLDER TESTS.
        //================================================================================================================================
        #region GetCabFolderTests

        private void testGetCabFolderNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();

                index.GetCabFolder(null, file, theEvent, cab);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderNullProduct(indexCache);
        }

        private void testGetCabFolderUnknownProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the product to one that is not recognised.
                product.Id++;

                index.GetCabFolder(product, file, theEvent, cab);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFolderUnknownProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderUnknownProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFolderUnknownProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderUnknownProduct(indexCache);
        }

        private void testGetCabFolderNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.GetCabFolder(product, null, theEvent, cab);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderNullFile(indexCache);
        }

        private void testGetCabFolderUnknownFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the file to one that is not recognised.
                file.Id++;

                index.GetCabFolder(product, file, theEvent, cab);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFolderUnknownFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderUnknownFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFolderUnknownFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderUnknownFile(indexCache);
        }

        private void testGetCabFolderNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.GetCabFolder(product, file, null, cab);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderNullEvent(indexCache);
        }

        private void testGetCabFolderUnknownEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the event to one that is not recognised.
                theEvent.Id++;

                index.GetCabFolder(product, file, theEvent, cab);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFolderUnknownEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderUnknownEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFolderUnknownEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderUnknownEvent(indexCache);
        }


        private void testGetCabFolderNullCab(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.GetCabFolder(product, file, theEvent, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("cab", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullCab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderNullCab(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFolderNullCabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderNullCab(indexCache);
        }


        public void testGetCabFolderOk(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, "EventName", "XYZ", 22, 1000);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            string cabFolder = index.GetCabFolder(product, file, theEvent, cab);
            Console.WriteLine(cabFolder);

            // This may fail if the structure of the database is changed.
            Assert.AreEqual(true, cabFolder.EndsWith("\\P_1_TestProduct1_2.10.02123.1293\\F_39_filename.dll_1.2.3.4\\E_20000\\CAB_22"));
        }

        [TestMethod]
        public void TestGetCabFolderOk()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFolderOk(index);
        }

        [TestMethod]
        public void TestGetCabFolderOkCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFolderOk(indexCache);
        }

        #endregion GetCabFolderTests

        //================================================================================================================================
        // GET CAB FILENAME TESTS.
        //================================================================================================================================
        #region GetCabFileNameTests

        private void testGetCabFileNameNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();

                index.GetCabFileName(null, file, theEvent, cab);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameNullProduct(indexCache);
        }

        private void testGetCabFileNameUnknownProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the product to one that is not recognised.
                product.Id++;

                index.GetCabFileName(product, file, theEvent, cab);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFileNameUnknownProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameUnknownProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFileNameUnknownProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameUnknownProduct(indexCache);
        }

        private void testGetCabFileNameNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.GetCabFileName(product, null, theEvent, cab);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameNullFile(indexCache);
        }

        private void testGetCabFileNameUnknownFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the file to one that is not recognised.
                file.Id++;

                index.GetCabFileName(product, file, theEvent, cab);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFileNameUnknownFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameUnknownFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFileNameUnknownFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameUnknownFile(indexCache);
        }

        private void testGetCabFileNameNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.GetCabFileName(product, file, null, cab);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameNullEvent(indexCache);
        }

        private void testGetCabFileNameUnknownEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the event to one that is not recognised.
                theEvent.Id++;

                index.GetCabFileName(product, file, theEvent, cab);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFileNameUnknownEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameUnknownEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestGetCabFileNameUnknownEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameUnknownEvent(indexCache);
        }


        private void testGetCabFileNameNullCab(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.GetCabFileName(product, file, theEvent, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("cab", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullCab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameNullCab(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestGetCabFileNameNullCabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameNullCab(indexCache);
        }


        public void testGetCabFileNameOk(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, "EventName", "XYZ.CAB", 22, 1000);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            string cabFolder = index.GetCabFileName(product, file, theEvent, cab);
            Console.WriteLine(cabFolder);

            // This may fail if the structure of the database is changed.
            Assert.AreEqual(true, cabFolder.EndsWith("\\P_1_TestProduct1_2.10.02123.1293\\F_39_filename.dll_1.2.3.4\\E_20000\\CAB_22\\XYZ.CAB"));
        }

        [TestMethod]
        public void TestGetCabFileNameOk()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testGetCabFileNameOk(index);
        }

        [TestMethod]
        public void TestGetCabFileNameOkCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testGetCabFileNameOk(indexCache);
        }
        #endregion

        //================================================================================================================================
        // ADD CAB TESTS.
        //================================================================================================================================
        #region AddCabTests


        private void testAddCabNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();

                index.AddCab(null, file, theEvent, cab, false);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNullProduct(indexCache);
        }

        private void testAddCabUnknownProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the product to one that is not recognised.
                product.Id++;

                index.AddCab(product, file, theEvent, cab, false);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabUnknownProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabUnknownProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabUnknownProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabUnknownProduct(indexCache);
        }

        private void testAddCabNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.AddCab(product, null, theEvent, cab, false);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNullFile(indexCache);
        }

        private void testAddCabUnknownFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the file to one that is not recognised.
                file.Id++;

                index.AddCab(product, file, theEvent, cab, false);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabUnknownFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabUnknownFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabUnknownFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabUnknownFile(indexCache);
        }

        private void testAddCabNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.AddCab(product, file, null, cab, false);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNullEvent(indexCache);
        }

        private void testAddCabUnknownEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the event to one that is not recognised.
                theEvent.Id++;

                index.AddCab(product, file, theEvent, cab, false);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabUnknownEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabUnknownEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabUnknownEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabUnknownEvent(indexCache);
        }


        private void testAddCabNullCab(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.AddCab(product, file, theEvent, null, false);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("cab", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullCab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNullCab(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNullCabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNullCab(indexCache);
        }


        public void testAddCabOk(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, "EventName", "XYZ", 22, 1000);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            index.AddCab(product, file, theEvent, cab, false);
        }

        [TestMethod]
        public void TestAddCabOk()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabOk(index);
        }

        [TestMethod]
        public void TestAddCabOkCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabOk(indexCache);
        }
        

        private void addCabs(IErrorIndex index, int numCabs)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashCabCollection originalCabs = new StackHashCabCollection();

            for (int i = 0; i < numCabs; i++)
            {
                StackHashCab cab = new StackHashCab(new DateTime(10 * i), new DateTime(20 * i), i, "Type" + i.ToString(),
                    "FRED" + i.ToString(), 234 + i, 100 + i);
                originalCabs.Add(cab);
                index.AddCab(product, file, theEvent, cab, false);
            }


            // Now get the cabs back.
            StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

            Assert.AreEqual(0, originalCabs.CompareTo(cabs));

        }


        [TestMethod]
        public void TestAdd1Cab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            addCabs(index, 1);
        }

        [TestMethod]
        public void TestAdd1CabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            addCabs(indexCache, 1);
        }

        [TestMethod]
        public void TestAdd2Cabs()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            addCabs(index, 2);
        }

        [TestMethod]
        public void TestAdd2CabsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            addCabs(indexCache, 2);
        }

        [TestMethod]
        public void TestAdd20Cabs()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            addCabs(index, 100);
        }

        [TestMethod]
        public void TestAdd20CabsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            addCabs(indexCache, 100);
        }


        private void testAddSameCabTwice(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            List<StackHashCab> originalCabs = new List<StackHashCab>();

            int cabId = 234;

            StackHashCab cab = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type", "FRED", cabId, 100);
            index.AddCab(product, file, theEvent, cab, false);

            // Add same Cab twice - with different params - should replace the existing entry.
            cab = new StackHashCab(new DateTime(11), new DateTime(21), 2, "Type3", "FRED3", cabId, 123);
            index.AddCab(product, file, theEvent, cab, false);
            originalCabs.Add(cab);

            // Now get the cabs back.
            StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

            Assert.AreEqual(1, cabs.Count);

            for (int i = 0; i < 1; i++)
            {
                // Compare the cabs.
                Assert.AreEqual(originalCabs[i].DateCreatedLocal, cabs[i].DateCreatedLocal);
                Assert.AreEqual(originalCabs[i].DateModifiedLocal, cabs[i].DateModifiedLocal);
                Assert.AreEqual(originalCabs[i].EventId, cabs[i].EventId);
                Assert.AreEqual(originalCabs[i].EventTypeName, cabs[i].EventTypeName);
                Assert.AreEqual(originalCabs[i].FileName, cabs[i].FileName);
                Assert.AreEqual(originalCabs[i].Id, cabs[i].Id);
                Assert.AreEqual(originalCabs[i].SizeInBytes, cabs[i].SizeInBytes);
            }
        }

        [TestMethod]
        public void TestAddSameCabTwice()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddSameCabTwice(index);
        }

        [TestMethod]
        public void TestAddSameCabTwiceCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddSameCabTwice(indexCache);
        }

        #endregion // AddCabFileTests.

        //================================================================================================================================
        // LOAD CAB LIST TESTS.
        //================================================================================================================================    

        #region LoadCabListTests

        private void testLoadCabListNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();

                index.LoadCabList(null, file, theEvent);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadCabListNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadCabListNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListNullProduct(indexCache);
        }

        private void testLoadCabListUnknownProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the product to one that is not recognised.
                product.Id++;

                index.LoadCabList(product, file, theEvent);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadCabListUnknownProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListUnknownProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadCabListUnknownProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListUnknownProduct(indexCache);
        }

        private void testLoadCabListNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.LoadCabList(product, null, theEvent);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadCabListNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadCabListNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListNullFile(indexCache);
        }

        private void testLoadCabListUnknownFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the file to one that is not recognised.
                file.Id++;

                index.LoadCabList(product, file, theEvent);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadCabListUnknownFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListUnknownFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadCabListUnknownFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListUnknownFile(indexCache);
        }

        private void testLoadCabListNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.LoadCabList(product, file, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadCabListNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestLoadCabListNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListNullEvent(indexCache);
        }

        private void testLoadCabListUnknownEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);
                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the event to one that is not recognised.
                theEvent.Id++;

                index.LoadCabList(product, file, theEvent);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadCabListUnknownEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListUnknownEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestLoadCabListUnknownEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListUnknownEvent(indexCache);
        }


        public void testLoadCabListOk(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, "EventName", "XYZ", 22, 1000);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            index.LoadCabList(product, file, theEvent);
        }

        [TestMethod]
        public void TestLoadCabListOk()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testLoadCabListOk(index);
        }

        [TestMethod]
        public void TestLoadCabListOkCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testLoadCabListOk(indexCache);
        }


        private void loadCabLists(IErrorIndex index, int numCabs)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashCabCollection originalCabs = new StackHashCabCollection();

            for (int i = 0; i < numCabs; i++)
            {
                StackHashCab cab = new StackHashCab(new DateTime(10 * i), new DateTime(20 * i), i, "Type" + i.ToString(),
                    "FRED" + i.ToString(), 234 + i, 100 + i);
                originalCabs.Add(cab);
                index.AddCab(product, file, theEvent, cab, false);
            }

            StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

            Assert.AreEqual(0, originalCabs.CompareTo(cabs));

        }


        [TestMethod]
        public void TestLoad1Cab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            loadCabLists(index, 1);
        }

        [TestMethod]
        public void TestLoad1CabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            loadCabLists(indexCache, 1);
        }

        [TestMethod]
        public void TestLoad2Cabs()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            loadCabLists(index, 2);
        }

        [TestMethod]
        public void TestLoad2CabsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            loadCabLists(indexCache, 2);
        }

        [TestMethod]
        public void TestLoad20Cabs()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            loadCabLists(index, 100);
        }

        [TestMethod]
        public void TestLoad20CabsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            loadCabLists(indexCache, 100);
        }

        [TestMethod]
        // Just for laughs.
        public void TestLoad20CabsCachedCache()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            ErrorIndexCache indexCache2 = new ErrorIndexCache(indexCache);
            loadCabLists(indexCache2, 100);
        }

        [TestMethod]
        public void ReplaceCabNotDiagnisticsData()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);

            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashCabCollection originalCabs = new StackHashCabCollection();

            int i = 100;
            StackHashCab cab = new StackHashCab(new DateTime(10 * i), new DateTime(20 * i), i, "Type" + i.ToString(),
                "FRED" + i.ToString(), 234 + i, 100 + i);
            cab.DumpAnalysis = new StackHashDumpAnalysis();
            cab.DumpAnalysis.DotNetVersion = "2.1.2.1";
            cab.DumpAnalysis.MachineArchitecture = "x86";
            cab.DumpAnalysis.OSVersion = "Windows XP";
            cab.DumpAnalysis.ProcessUpTime = "3 days";
            cab.DumpAnalysis.SystemUpTime = "4 days";
            originalCabs.Add(cab);
            index.AddCab(product, file, theEvent, cab, true);

            // Now replace with a null dumpanalysis field.
            cab.DumpAnalysis = null;
            index.AddCab(product, file, theEvent, cab, false);

            StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

            Assert.AreEqual(0, originalCabs.CompareTo(cabs));

        }

        private void addCabNoOverwriteDiagnostics(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashCabCollection originalCabs = new StackHashCabCollection();

            StackHashCab cab = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type",
                "FRED", 234, 100);
            cab.DumpAnalysis = new StackHashDumpAnalysis();
            cab.DumpAnalysis.OSVersion = "Windows XP";
            cab.DumpAnalysis.MachineArchitecture = "X86";
            originalCabs.Add(cab);
            index.AddCab(product, file, theEvent, cab, true);


            // Now overwrite - should NOT overwrite the diagnostics.
            StackHashCab cab2 = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type",
                "FRED", 234, 100);
            index.AddCab(product, file, theEvent, cab, false);


            StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

            Assert.AreEqual(cab.DateCreatedLocal, cabs[0].DateCreatedLocal);
            Assert.AreEqual(cab.DateModifiedLocal, cabs[0].DateModifiedLocal);
            Assert.AreEqual(cab.EventId, cabs[0].EventId);
            Assert.AreEqual(cab.EventTypeName, cabs[0].EventTypeName);
            Assert.AreEqual(cab.FileName, cabs[0].FileName);
            Assert.AreEqual(cab.Id, cabs[0].Id);
            Assert.AreEqual(cab.SizeInBytes, cabs[0].SizeInBytes);
            Assert.AreNotEqual(null, cabs[0].DumpAnalysis);
            Assert.AreEqual(cab.DumpAnalysis.DotNetVersion, cabs[0].DumpAnalysis.DotNetVersion);
            Assert.AreEqual(cab.DumpAnalysis.MachineArchitecture, cabs[0].DumpAnalysis.MachineArchitecture);
            Assert.AreEqual(cab.DumpAnalysis.OSVersion, cabs[0].DumpAnalysis.OSVersion);
            Assert.AreEqual(cab.DumpAnalysis.ProcessUpTime, cabs[0].DumpAnalysis.ProcessUpTime);
            Assert.AreEqual(cab.DumpAnalysis.SystemUpTime, cabs[0].DumpAnalysis.SystemUpTime);
        }


        [TestMethod]
        public void TestAddCabNoOverwriteDiagnostics()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            addCabNoOverwriteDiagnostics(index);
        }

        [TestMethod]
        public void TestAddCabNoOverwriteDiagnosticsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            addCabNoOverwriteDiagnostics(indexCache);
        }

        private void addCabNoOverwriteDumpAnalysisNoNullShouldOverwrite(IErrorIndex index)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashCabCollection originalCabs = new StackHashCabCollection();

            StackHashCab cab = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type",
                "FRED", 234, 100);
            cab.DumpAnalysis = new StackHashDumpAnalysis();
            cab.DumpAnalysis.OSVersion = "Windows XP";
            cab.DumpAnalysis.MachineArchitecture = "X86";
            originalCabs.Add(cab);
            index.AddCab(product, file, theEvent, cab, true);


            // Now overwrite - should overwrite the diagnostics.
            StackHashCab cab2 = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type",
                "FRED", 234, 100);
            cab2.DumpAnalysis = new StackHashDumpAnalysis();
            cab2.DumpAnalysis.OSVersion = "Windows XP 2";
            cab2.DumpAnalysis.MachineArchitecture = "X86 2";
            index.AddCab(product, file, theEvent, cab, true);


            StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

            Assert.AreEqual(cab2.DateCreatedLocal, cabs[0].DateCreatedLocal);
            Assert.AreEqual(cab2.DateModifiedLocal, cabs[0].DateModifiedLocal);
            Assert.AreEqual(cab2.EventId, cabs[0].EventId);
            Assert.AreEqual(cab2.EventTypeName, cabs[0].EventTypeName);
            Assert.AreEqual(cab2.FileName, cabs[0].FileName);
            Assert.AreEqual(cab2.Id, cabs[0].Id);
            Assert.AreEqual(cab2.SizeInBytes, cabs[0].SizeInBytes);
            Assert.AreNotEqual(null, cabs[0].DumpAnalysis);
            Assert.AreEqual(cab2.DumpAnalysis.DotNetVersion, cabs[0].DumpAnalysis.DotNetVersion);
            Assert.AreEqual(cab2.DumpAnalysis.MachineArchitecture, cabs[0].DumpAnalysis.MachineArchitecture);
            Assert.AreEqual(cab2.DumpAnalysis.OSVersion, cabs[0].DumpAnalysis.OSVersion);
            Assert.AreEqual(cab2.DumpAnalysis.ProcessUpTime, cabs[0].DumpAnalysis.ProcessUpTime);
            Assert.AreEqual(cab2.DumpAnalysis.SystemUpTime, cabs[0].DumpAnalysis.SystemUpTime);
        }


        [TestMethod]
        public void AddCabNoOverwriteDumpAnalysisNoNullShouldOverwrite()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            addCabNoOverwriteDiagnostics(index);
        }

        [TestMethod]
        public void AddCabNoOverwriteDumpAnalysisNoNullShouldOverwriteCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            addCabNoOverwriteDiagnostics(indexCache);
        }
        private void doesCabExist(IErrorIndex index, bool addTheCab)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashCabCollection originalCabs = new StackHashCabCollection();

            StackHashCab cab = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type",
                "FRED", 234, 100);
            cab.DumpAnalysis = new StackHashDumpAnalysis();
            cab.DumpAnalysis.OSVersion = "Windows XP";
            cab.DumpAnalysis.MachineArchitecture = "X86";
            originalCabs.Add(cab);

            bool cabExists;

            String cabFileName = index.GetCabFileName(product, file, theEvent, cab);
            if (addTheCab)
            {
                index.AddCab(product, file, theEvent, cab, true);
                FileStream cabFile = File.Create(cabFileName);

                if (cabFile != null)
                    cabFile.Close();
            }
            cabExists = index.CabExists(product, file, theEvent, cab);

            if (File.Exists(cabFileName))
                File.Delete(cabFileName);

            Assert.AreEqual(addTheCab, cabExists);
        }


        [TestMethod]
        public void doesCabExistNo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            doesCabExist(index, false);
        }

        [TestMethod]
        public void doesCabExistNoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            doesCabExist(indexCache, false);
        }

        [TestMethod]
        public void doesCabExistYes()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            doesCabExist(index, true);
        }

        [TestMethod]
        public void doesCabExistYesCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            doesCabExist(indexCache, true);
        }

        
        
        #endregion // LoadCabListTests.

        private void getCabCount(IErrorIndex index, int numCabs)
        {
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);
            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 22, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            for (int i = 0; i < numCabs; i++)
            {
                // Get the cab count.
                int cabCount = index.GetCabCount(product, file, theEvent);
                Assert.AreEqual(i, cabCount);

                StackHashCab cab = new StackHashCab(new DateTime(10), new DateTime(20), 1, "Type",
                    "FRED", i, 100);
                cab.DumpAnalysis = new StackHashDumpAnalysis();
                cab.DumpAnalysis.OSVersion = "Windows XP";
                cab.DumpAnalysis.MachineArchitecture = "X86";
                index.AddCab(product, file, theEvent, cab, true);


                // Get the cab count.
                cabCount = index.GetCabCount(product, file, theEvent);

                Assert.AreEqual(i + 1, cabCount);
            }

            if (numCabs == 0)
            {
                // Get the cab count.
                int cabCount = index.GetCabCount(product, file, theEvent);
                Assert.AreEqual(0, cabCount);
            }
        }


        [TestMethod]
        public void GetCabCountNoCab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            getCabCount(index, 0);
        }

        [TestMethod]
        public void GetCabCountNoCabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            getCabCount(indexCache, 0);
        }


        [TestMethod]
        public void GetCabCount1Cab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            getCabCount(index, 1);
        }

        [TestMethod]
        public void GetCabCount1CabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            getCabCount(indexCache, 1);
        }

        [TestMethod]
        public void GetCabCount2Cabs()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            getCabCount(index, 2);
        }

        [TestMethod]
        public void GetCabCount2CabsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            getCabCount(indexCache, 2);
        }

        [TestMethod]
        public void GetCabCountNCabs()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            getCabCount(index, 30);
        }

        [TestMethod]
        public void GetCabCountNCabsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            getCabCount(indexCache, 40);
        }
    
    }
}
