using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashWinQual;
using StackHashErrorIndex;
using WinQualAtomFeed;


namespace WinQualUnitTests
{
    /// <summary>
    /// Summary description for ShouldCollectCabUnitTests
    /// </summary>
    [TestClass]
    public class ShouldCollectCabUnitTests
    {
        String m_TempPath;
        AutoResetEvent m_WinQualSyncEvent;

        public ShouldCollectCabUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashWinQualUnitTests";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
            m_WinQualSyncEvent = new AutoResetEvent(false);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath))
            {
                PathUtils.MarkDirectoryWritable(m_TempPath, true);
                PathUtils.DeleteDirectory(m_TempPath, true);
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


        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect ALL.
        /// </summary>
        [TestMethod]
        public void GlobalAllTrue()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.All;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab.txt", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            AtomCabCollection cabs = new AtomCabCollection();

            // Requested all cabs to be downloaded - so should be true.
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab, cabs, 0, policyCollection));
        }

        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect NONE.
        /// </summary>
        [TestMethod]
        public void GlobalNoneFalse()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.None;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab.txt", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            AtomCabCollection cabs = new AtomCabCollection();

            // Requested NO cabs should be downloaded.
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab, cabs, 0, policyCollection));
        }

        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 1 only - currently no cabs so should allow 1.
        /// </summary>
        [TestMethod]
        public void GlobalMax1CurrentlyNoCabs()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab.txt", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            AtomCabCollection cabs = new AtomCabCollection();

            // The maximum cabs is 1 - no cabs have yet been downloaded so should return true.
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab, cabs, 0, policyCollection));
        }


        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 1.
        /// A cab exists in the database but no file exists for it so it should allow the next one.
        /// </summary>
        [TestMethod]
        public void GlobalMaxAsReceivedTrue1CabMaxNotReached()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab.txt", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);


            // New cabs arrived.
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 4, "EventTypeName", "cab.txt", 6, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));

            // Should be true because although there is a cab in the index - no cab file exists for it.
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, 0, policyCollection));
        }

        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 1.
        /// There is already a cabinfo + file so can't download this one.
        /// </summary>
        [TestMethod]
        public void GlobalMaxAsReceived1CabMaxReached()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 1000, "EventTypeName", "cab1000.cab", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 1001, "EventTypeName", "cab1001.cab", 6, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));

            // Should not allow because cab file exists in the database.
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, 0, policyCollection));
        }

        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 2.
        /// Already 1 cab file but second should be allowed.
        /// </summary>
        [TestMethod]
        public void GlobalMaxAsReceived1OutOf2()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Maximum = 2;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 1000, "EventTypeName", "cab1000.cab", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 1001, "EventTypeName", "cab1001.cab", 6, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));

            // 1 cab in database - max is 2 so should allow another.
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, 0, policyCollection));
        }


        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 100%.
        /// Should allow all.
        /// </summary>
        [TestMethod]
        public void GlobalPercentage100()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Percentage = 100;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 1000, "EventTypeName", "cab1000.cab", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 1001, "EventTypeName", "cab1001.cab", 6, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));

            // 1 cab in database - 100% allowed so should allow any number of others.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 50%.
        /// Already one in the database so the second shouldn't be allowed.
        /// </summary>
        [TestMethod]
        public void GlobalPercentage50False()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Percentage = 50;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 1000, "EventTypeName", "cab1000.cab", 5, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 1001, "EventTypeName", "cab1001.cab", 6, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));

            // 50 percent allowed - 1 already downloaded so shouldn't allow the second.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 50%.
        /// There are 3 cabs in the database - 2 have been downloaded. 3 new ones arrive - of which only 1 should be added.
        /// </summary>
        [TestMethod]
        public void GlobalPercentage50True3of6()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Percentage = 50;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);
            StackHashCab cab3 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);
            indexCache.AddCab(product, file, theEvent, cab2, false);
            indexCache.AddCab(product, file, theEvent, cab3, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab2);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab2);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            
            // 3 new cabs arrived.
            StackHashCab cab4 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1004.cab", 1004, 100);
            StackHashCab cab5 = new StackHashCab(DateTime.Now, DateTime.Now, 4, "EventTypeName", "cab1004.cab", 1005, 101);
            StackHashCab cab6 = new StackHashCab(DateTime.Now, DateTime.Now, 35, "EventTypeName", "cab1004.cab", 1006, 102);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab4, "cablink"));
            cabs.Add(new AtomCab(cab5, "cablink"));
            cabs.Add(new AtomCab(cab6, "cablink"));

            // There are 3 cabs in the database - 2 have been downloaded. 50 percent of 6 should be 3 so should allow download 
            // of 1 more cab.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab4, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        /// <summary>
        /// Global policy - Cab condition - Cab collection - Collect 50%.
        /// There are 3 cabs in the database - 1 has been downloaded. 50 percent of 4 should be 2 so should allow download 
        /// of the 4th.
        /// </summary>
        [TestMethod]
        public void GlobalPercentage50True()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Percentage = 50;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);
            StackHashCab cab3 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);
            indexCache.AddCab(product, file, theEvent, cab2, false);
            indexCache.AddCab(product, file, theEvent, cab3, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            StackHashCab cab4 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1004.cab", 1004, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab4, "cablink"));

            // There are 3 cabs in the database - 1 has been downloaded. 50 percent of 4 should be 2 so should allow download 
            // of the 4th.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab4, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        
        [TestMethod]
        public void GlobalPercentage50False1OutOf3()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].Percentage = 50;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);
            indexCache.AddCab(product, file, theEvent, cab1, false);
            indexCache.AddCab(product, file, theEvent, cab2, false);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            StackHashCab cab3 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab3, "cablink"));

            // There are 2 cabs in the database - 1 has been downloaded. 50 percent of 3 should be 1.5 so should NOT allow download 
            // of the 3rd.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void GlobalMaxNewestFirst()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the first one only.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        private void AddSampleCab(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            String cabFolder = errorIndex.GetCabFolder(product, file, theEvent, cab);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = errorIndex.GetCabFileName(product, file, theEvent, cab);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
        }


        [TestMethod]
        public void GlobalMaxNewestFirstWithAdd()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the first one only.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));

            indexCache.AddCab(product, file, theEvent, cab1, false);
            AddSampleCab(indexCache, product, file, theEvent, cab1);

            totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);            
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void GlobalMaxNewestFirstWithAdd2()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[0].Maximum = 2;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-3), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the first one only.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));

            indexCache.AddCab(product, file, theEvent, cab1, false);
            AddSampleCab(indexCache, product, file, theEvent, cab1);

            totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void GlobalMaxOldestFirst()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.OldestFirst;
            policyCollection[0].Maximum = 2;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 100);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the second 2 because they are older.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void GlobalMaxBiggestFirst()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 102);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the second one because it is biggest.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void GlobalMaxBiggestFirstMultiple()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].Maximum = 2;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 102);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the last 2 because the
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        
        [TestMethod]
        public void GlobalMaxSmallestFirst()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[0].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);

            String cabFolder = indexCache.GetCabFolder(product, file, theEvent, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, theEvent, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the last one because it is smallest.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void GlobalMaxSmallestFirst2Allowed()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[0].Maximum = 2;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // None in database - should allow the first and last one because they are smallest.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void EventOveridesGlobal()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[0].Maximum = 1;
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Event;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[1].RootId = 3; // Event Id 
            policyCollection[1].Maximum = 2;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // The "smallest 1" global policy should be overridden by the "largest 2" event policy.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void EventOveridesGlobalPolicyOrderReversed()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Event;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].RootId = 3; // Event Id 
            policyCollection[0].Maximum = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Global;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, theEvent);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // The "smallest 1" global policy should be overridden by the "largest 2" event policy.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, theEvent, cabs);
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, theEvent, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void EventOveridesGlobalPolicyNoMatch()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Event;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].RootId = 4; // The second event - with no cabs. 
            policyCollection[0].Maximum = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Global;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].Maximum = 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 4, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddEvent(product, file, event2);

            String cabFolder = indexCache.GetCabFolder(product, file, event1, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, event1, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink"));
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // The "smallest 1" global policy should NOT be overridden by the "largest 2" event policy because the policy ID doesn't match.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void ProductOveridesGlobalPolicyFull()
        {
            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 4, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddEvent(product, file, event2);
            indexCache.AddCab(product, file, event1, cab1, false);

            String cabFolder = indexCache.GetCabFolder(product, file, event1, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, event1, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink")); // Same one again.
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));


            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Event;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].RootId = event2.Id;
            policyCollection[0].Maximum = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Global;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].Maximum = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Product;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.Count;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].RootId = product.Id;
            policyCollection[2].Maximum = 1;



            // The product policy should override - but max = 1 and already collected 1 cab so shouldn't collect any more.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void ProductOveridesGlobalPolicyDownload()
        {
            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashEvent event2 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 4, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 1011);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-3), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddEvent(product, file, event2);
            indexCache.AddCab(product, file, event1, cab1, false);

            String cabFolder = indexCache.GetCabFolder(product, file, event1, cab1);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            String cabFileName = indexCache.GetCabFileName(product, file, event1, cab1);

            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab1, "cablink")); // Same one again.
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));


            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Event;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].RootId = event2.Id;
            policyCollection[0].Maximum = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Global;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].Maximum = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Product;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.Count;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].RootId = product.Id;
            policyCollection[2].Maximum = 2;



            // The product policy should override - but max = 2 and already collected 1 cab so should collect one only.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab1, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }


        [TestMethod]
        public void CabIsBigger()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[0].Maximum = 1;
            policyCollection[0].CollectLarger = true;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 22);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 1010);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddCab(product, file, event1, cab1, false);

            AddSampleCab(indexCache, product, file, event1, cab1);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // Already reached max - but both of these new cabs is bigger than those stored so download the biggest one.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void CabIsNotBigger()
        {
            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[0].Maximum = 1;
            policyCollection[0].CollectLarger = true;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 3, new StackHashEventSignature(), 2, 2);
            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 99);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddCab(product, file, event1, cab1, false);

            AddSampleCab(indexCache, product, file, event1, cab1);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // Already reached max - but both of these new cabs is bigger than those stored so download the biggest one.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void EventLevelSaysDontDownload()
        {
            int eventId = 99;
            int hits = 200;

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[0].Maximum = 100;
            policyCollection[0].CollectLarger = true;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Event;
            policyCollection[1].RootId = eventId;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[1].Minimum = hits + 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", eventId, new StackHashEventSignature(), hits, 2);

            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 99);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddCab(product, file, event1, cab1, false);

            AddSampleCab(indexCache, product, file, event1, cab1);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // Not reached the max at the cab level but the event level says not reached the min hits.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void EventLevelExistsForDifferentEventId()
        {
            int eventId = 99;
            int hits = 200;

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].Maximum = 2;
            policyCollection[0].CollectLarger = true;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Event;
            policyCollection[1].RootId = eventId + 1;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[1].Minimum = hits + 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", eventId, new StackHashEventSignature(), hits, 2);

            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 99);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddCab(product, file, event1, cab1, false);

            AddSampleCab(indexCache, product, file, event1, cab1);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // Not reached the max at the cab level but the event level says not reached the min hits.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }

        [TestMethod]
        public void EventLevelAll()
        {
            int eventId = 99;
            int hits = 200;

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].Maximum = 2;
            policyCollection[0].CollectLarger = true;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.Event;
            policyCollection[1].RootId = eventId;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.All;
            policyCollection[1].Minimum = hits + 1;


            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(m_TempPath, "IndexName");
            ErrorIndexCache indexCache = new ErrorIndexCache(xmlErrorIndex);
            indexCache.Activate();

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.file.com", 1, "StackHash", 1, 2, "1.2.3.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "FileName", "2.3.4.5");
            StackHashEvent event1 = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", eventId, new StackHashEventSignature(), hits, 2);

            StackHashCab cab1 = new StackHashCab(DateTime.Now, DateTime.Now, 3, "EventTypeName", "cab1001.cab", 1001, 100);
            StackHashCab cab2 = new StackHashCab(DateTime.Now.AddDays(-1), DateTime.Now, 3, "EventTypeName", "cab1002.cab", 1002, 99);
            StackHashCab cab3 = new StackHashCab(DateTime.Now.AddDays(-2), DateTime.Now, 3, "EventTypeName", "cab1003.cab", 1003, 100);

            indexCache.AddProduct(product);
            indexCache.AddFile(product, file);
            indexCache.AddEvent(product, file, event1);
            indexCache.AddCab(product, file, event1, cab1, false);

            AddSampleCab(indexCache, product, file, event1, cab1);


            // New cabs arrived.
            AtomCabCollection cabs = new AtomCabCollection();
            cabs.Add(new AtomCab(cab2, "cablink"));
            cabs.Add(new AtomCab(cab3, "cablink"));

            // Not reached the max at the cab level but the event level says not reached the min hits.
            int totalCabsIncludingNewOnes = WinQualAtomFeedServices.GetTotalCabsIncludingNewOnes(indexCache, product, file, event1, cabs);
            Assert.AreEqual(false, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab2, cabs, totalCabsIncludingNewOnes, policyCollection));
            Assert.AreEqual(true, WinQualAtomFeedServices.ShouldCollectCab(indexCache, product, file, event1, cab3, cabs, totalCabsIncludingNewOnes, policyCollection));
        }
    }
}
