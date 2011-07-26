using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SyncDataUnitTests
    /// </summary>
    [TestClass]
    public class SyncDataUnitTests
    {
        string m_TempPath;

        public SyncDataUnitTests()
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


        public void testDefaults(IErrorIndex index)
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files", 100, "Cucku Backup", 20, 1, "1.2.3.4");
            index.Activate();
            index.AddProduct(product);

            DateTime lastSyncDate = index.GetLastSyncTimeLocal(product.Id);
            DateTime lastHitDate = index.GetLastHitTimeLocal(product.Id);
            DateTime lastSyncCompletedDate = index.GetLastSyncCompletedTimeLocal(product.Id);
            DateTime lastSyncStartedDate = index.GetLastSyncStartedTimeLocal(product.Id);


            Assert.AreEqual(new DateTime(0), lastSyncDate);
            Assert.AreEqual(new DateTime(0), lastHitDate);
            Assert.AreEqual(new DateTime(0), lastSyncCompletedDate);
            Assert.AreEqual(new DateTime(0), lastSyncStartedDate);
        }

        [TestMethod]
        public void TestDefaults()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            testDefaults(xmlIndex);
        }

        
        [TestMethod]
        public void TestDefaultsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testDefaults(indexCache);
        }

        public void testSetLastSyncData(IErrorIndex index)
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files", 100, "Cucku Backup", 20, 1, "1.2.3.4");
            index.Activate();
            index.AddProduct(product);

            DateTime date = DateTime.Now;

            index.SetLastSyncTimeLocal(product.Id, date);
            
            DateTime lastSyncDate = index.GetLastSyncTimeLocal(product.Id);
            DateTime lastHitDate = index.GetLastHitTimeLocal(product.Id);
            DateTime lastSyncCompletedDate = index.GetLastSyncCompletedTimeLocal(product.Id);
            DateTime lastSyncStartedDate = index.GetLastSyncStartedTimeLocal(product.Id);


            Assert.AreEqual(date, lastSyncDate);
            Assert.AreEqual(new DateTime(0), lastHitDate);
            Assert.AreEqual(new DateTime(0), lastSyncCompletedDate);
            Assert.AreEqual(new DateTime(0), lastSyncStartedDate);
        }

        [TestMethod]
        public void TestSetLastSyncData()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            testSetLastSyncData(xmlIndex);
        }


        [TestMethod]
        public void TestSetLastSyncDataCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSetLastSyncData(indexCache);
        }

        public void testSetLastSyncCompletedData(IErrorIndex index)
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files", 100, "Cucku Backup", 20, 1, "1.2.3.4");
            index.Activate();
            index.AddProduct(product);

            DateTime date = DateTime.Now;

            index.SetLastSyncCompletedTimeLocal(product.Id, date);

            DateTime lastSyncDate = index.GetLastSyncTimeLocal(product.Id);
            DateTime lastHitDate = index.GetLastHitTimeLocal(product.Id);
            DateTime lastSyncCompletedDate = index.GetLastSyncCompletedTimeLocal(product.Id);
            DateTime lastSyncStartedDate = index.GetLastSyncStartedTimeLocal(product.Id);


            Assert.AreEqual(new DateTime(0), lastSyncDate);
            Assert.AreEqual(new DateTime(0), lastHitDate);
            Assert.AreEqual(date, lastSyncCompletedDate);
            Assert.AreEqual(new DateTime(0), lastSyncStartedDate);
        }

        [TestMethod]
        public void TestSetLastSyncCompletedData()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            testSetLastSyncCompletedData(xmlIndex);
        }


        [TestMethod]
        public void TestSetLastSyncCompletedDataCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSetLastSyncCompletedData(indexCache);
        }

        public void testSetLastSyncStartedData(IErrorIndex index)
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files", 100, "Cucku Backup", 20, 1, "1.2.3.4");
            index.Activate();
            index.AddProduct(product);

            DateTime date = DateTime.Now;

            index.SetLastSyncStartedTimeLocal(product.Id, date);

            DateTime lastSyncDate = index.GetLastSyncTimeLocal(product.Id);
            DateTime lastHitDate = index.GetLastHitTimeLocal(product.Id);
            DateTime lastSyncCompletedDate = index.GetLastSyncCompletedTimeLocal(product.Id);
            DateTime lastSyncStartedDate = index.GetLastSyncStartedTimeLocal(product.Id);

            Assert.AreEqual(new DateTime(0), lastSyncDate);
            Assert.AreEqual(new DateTime(0), lastHitDate);
            Assert.AreEqual(new DateTime(0), lastSyncCompletedDate);
            Assert.AreEqual(date, lastSyncStartedDate);
        }

        [TestMethod]
        public void TestSetLastSyncStartedData()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            testSetLastSyncStartedData(xmlIndex);
        }

        [TestMethod]
        public void TestSetLastSyncStartedDataCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSetLastSyncStartedData(indexCache);
        }

        public void testSetLastHitData(IErrorIndex index)
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files", 100, "Cucku Backup", 20, 1, "1.2.3.4");
            index.Activate();
            index.AddProduct(product);

            DateTime date = DateTime.Now;

            index.SetLastHitTimeLocal(product.Id, date);

            DateTime lastSyncDate = index.GetLastSyncTimeLocal(product.Id);
            DateTime lastHitDate = index.GetLastHitTimeLocal(product.Id);
            DateTime lastSyncCompletedDate = index.GetLastSyncCompletedTimeLocal(product.Id);
            DateTime lastSyncStartedDate = index.GetLastSyncStartedTimeLocal(product.Id);


            Assert.AreEqual(new DateTime(0), lastSyncDate);
            Assert.AreEqual(date, lastHitDate);
            Assert.AreEqual(new DateTime(0), lastSyncCompletedDate);
            Assert.AreEqual(new DateTime(0), lastSyncStartedDate);
        }

        [TestMethod]
        public void TestSetLastHitData()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            testSetLastHitData(xmlIndex);
        }


        [TestMethod]
        public void TestSetLastHitDataCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSetLastHitData(indexCache);
        }


        public void testSetAllDates(IErrorIndex index)
        {
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files", 100, "Cucku Backup", 20, 1, "1.2.3.4");
            index.Activate();
            index.AddProduct(product);

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now.AddDays(1);
            DateTime date3 = DateTime.Now.AddDays(2);
            DateTime date4 = DateTime.Now.AddDays(3);

            index.SetLastHitTimeLocal(product.Id, date1);
            index.SetLastSyncCompletedTimeLocal(product.Id, date2);
            index.SetLastSyncTimeLocal(product.Id, date3);
            index.SetLastSyncStartedTimeLocal(product.Id, date4);

            DateTime lastSyncDate = index.GetLastSyncTimeLocal(product.Id);
            DateTime lastHitDate = index.GetLastHitTimeLocal(product.Id);
            DateTime lastSyncCompletedDate = index.GetLastSyncCompletedTimeLocal(product.Id);
            DateTime lastSyncStartedDate = index.GetLastSyncStartedTimeLocal(product.Id);


            Assert.AreEqual(date1, lastHitDate);
            Assert.AreEqual(date2, lastSyncCompletedDate);
            Assert.AreEqual(date3, lastSyncDate);
            Assert.AreEqual(date4, lastSyncStartedDate);
        }

        [TestMethod]
        public void TestSetAllDates()
        {
            XmlErrorIndex xmlIndex = new XmlErrorIndex(m_TempPath, "TestIndex");
            testSetAllDates(xmlIndex);
        }


        [TestMethod]
        public void TestSetAllDatesCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testSetAllDates(indexCache);
        }
    }
}
