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
    /// Summary description for EventInfoUnitTests
    /// </summary>
    [TestClass]
    public class EventInfoUnitTests
    {
        string m_TempPath;

        public EventInfoUnitTests()
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

        private void testAddEventInfoNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                index.AddEventInfoCollection(null, file, theEvent, eventInfoCollection);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoNullProduct(indexCache);
        }


        private void testAddEventInfoProductNotFound(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

                index.AddProduct(product);
                index.AddFile(product, file);

                // Change the product id so it doesn't match the added product.
                product.Id++;

                StackHashEvent theEvent = new StackHashEvent();
                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventInfoProductNotFound()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoProductNotFound(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventInfoProductNotFoundCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoProductNotFound(indexCache);
        }


        private void testAddEventInfoNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");

                index.AddProduct(product);
                StackHashEvent theEvent = new StackHashEvent();
                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                index.AddEventInfoCollection(product, null, theEvent, eventInfoCollection);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoNullFile(indexCache);
        }

        private void testAddEventInfoUnknownFile(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

                index.AddProduct(product);
                index.AddFile(product, file);

                // Change the file id so it isn't recognised.
                file.Id++;

                StackHashEvent theEvent = new StackHashEvent();
                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventInfoUnknownFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoUnknownFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventInfoUnknownFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoUnknownFile(indexCache);
        }



        private void testAddEventInfoNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                index.AddProduct(product);
                index.AddFile(product, file);

                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                index.AddEventInfoCollection(product, file, null, eventInfoCollection);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoNullEvent(indexCache);
        }

        private void testAddEventInfoUnknownEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                parameters.Add(new StackHashParameter("param2", "param2value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);

                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                // Change the event id so it isn't recognised.
                theEvent.Id++;

                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventInfoUnknownEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoUnknownEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventInfoUnknownEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoUnknownEvent(indexCache);
        }


        private void testAddEventInfoNullEventInfo(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1", "param1value"));
                parameters.Add(new StackHashParameter("param2", "param2value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);

                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.AddEventInfoCollection(product, file, theEvent, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("eventInfoCollection", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullEventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddEventInfoNullEventInfo(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventInfoNullEventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventInfoNullEventInfo(indexCache);
        }


        private void testAddNEventInfo(IErrorIndex index, int numEventInfos)
        {
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);

            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, -1, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
            int totalHits = 0;
            for (int i = 0; i < numEventInfos; i++)
            {
                int hitsForThisEvent = (i + 1);
                totalHits += hitsForThisEvent;

                StackHashEventInfo eventInfo = new StackHashEventInfo(DateTime.Now.AddDays(i).ToUniversalTime(),
                    DateTime.Now.AddDays(i + 1).ToUniversalTime(), DateTime.Now.AddDays(i + 2).ToUniversalTime(), "English" + i.ToString(),
                    i, "locale" + i.ToString(), "OS" + i.ToString(), "OSVersion" + i.ToString(), hitsForThisEvent);
                eventInfoCollection.Add(eventInfo);
            }

            index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);

            
            // Now get all the event info and make sure it all matches.
            StackHashEventInfoCollection eventInfoCollection2 = index.LoadEventInfoList(product, file, theEvent);

            Assert.AreEqual(0, eventInfoCollection.CompareTo(eventInfoCollection2));

            StackHashEventPackageCollection eventPackages = index.GetProductEvents(product);
            Assert.AreEqual(1, eventPackages.Count);
            Assert.AreEqual(totalHits, eventPackages[0].EventData.TotalHits);
        }

        [TestMethod]
        public void TestAdd1EventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddNEventInfo(index, 1);
        }

        [TestMethod]
        public void TestAdd1EventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNEventInfo(indexCache, 1);
        }

        [TestMethod]
        public void TestAdd2EventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddNEventInfo(index, 2);
        }

        [TestMethod]
        public void TestAdd2EventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNEventInfo(indexCache, 2);
        }

        [TestMethod]
        public void TestAdd100EventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddNEventInfo(index, 100);
        }

        [TestMethod]
        public void TestAdd100EventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNEventInfo(indexCache, 100);
        }


        private void testAddNEventInfoReloadCache(IErrorIndex realIndex, int numEventInfos)
        {
            ErrorIndexCache index = new ErrorIndexCache(realIndex);
            index.Activate();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);

            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, 99, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
            for (int i = 0; i < numEventInfos; i++)
            {
                StackHashEventInfo eventInfo = new StackHashEventInfo(DateTime.Now.AddDays(i),
                    DateTime.Now.AddDays(i + 1), DateTime.Now.AddDays(i + 2), "English" + i.ToString(),
                    i, "locale" + i.ToString(), "OS" + i.ToString(), "OSVersion" + i.ToString(), i * 10);
                eventInfoCollection.Add(eventInfo);
            }

            index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);

            // Now reconnect a cache to make sure that the data has been stored ok.
            index = new ErrorIndexCache(realIndex);
            index.Activate();

            // Now get all the event info and make sure it all matches.
            StackHashEventInfoCollection eventInfoCollection2 = index.LoadEventInfoList(product, file, theEvent);

            Assert.AreEqual(0, eventInfoCollection.CompareTo(eventInfoCollection2));
        }

        [TestMethod]
        public void TestAddNEventInfoReloadCacheXmlIndex()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddNEventInfoReloadCache(index, 10);
        }

        private void testMergeOneNewEventInfo(IErrorIndex index, int numEventInfos)
        {
            // Add event info - then add another new one.
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);

            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, -1, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
            int totalHits = 0;
            for (int i = 0; i < numEventInfos; i++)
            {
                totalHits += i + 1;

                StackHashEventInfo eventInfo = new StackHashEventInfo(DateTime.Now.AddDays(-3 * i),
                    DateTime.Now.AddDays(-2 * i), DateTime.Now.AddDays(-1 * i), "English" + i.ToString(),
                    i, "locale" + i.ToString(), "OS" + i.ToString(), "OSVersion" + i.ToString(), i + 1);
                eventInfoCollection.Add(eventInfo);
            }

            index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);


            StackHashEventInfoCollection eventInfoCollectionNew = new StackHashEventInfoCollection();
            for (int i = numEventInfos; i < numEventInfos * 2; i++)
            {
                totalHits += i + 1;

                StackHashEventInfo eventInfo = new StackHashEventInfo(DateTime.Now.AddDays(-3 * i),
                    DateTime.Now.AddDays(-2 * i), DateTime.Now.AddDays(-1 * i), "English" + i.ToString(),
                    i, "locale" + i.ToString(), "OS" + i.ToString(), "OSVersion" + i.ToString(), i + 1);
                eventInfoCollectionNew.Add(eventInfo);
                eventInfoCollection.Add(eventInfo);
            }

            index.MergeEventInfoCollection(product, file, theEvent, eventInfoCollectionNew);


            // Now get all the event info and make sure it all matches.
            StackHashEventInfoCollection eventInfoCollection2 = index.LoadEventInfoList(product, file, theEvent);

            Assert.AreEqual(0, eventInfoCollection.CompareTo(eventInfoCollection2));

            StackHashEventPackageCollection eventPackages = index.GetProductEvents(product);
            Assert.AreEqual(1, eventPackages.Count);
            Assert.AreEqual(totalHits, eventPackages[0].EventData.TotalHits);
        }

        [TestMethod]
        public void TestMergeOneNewEventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfo(index, 1);
        }

        [TestMethod]
        public void TestMergeOneNewEventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfo(indexCache, 1);
        }

        [TestMethod]
        public void TestMerge2NewEventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfo(index, 2);
        }

        [TestMethod]
        public void TestMerge2NewEventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfo(indexCache, 2);
        }
        [TestMethod]
        public void TestMergeNNewEventInfo()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfo(index, 10);
        }

        [TestMethod]
        public void TestMergeNNewEventInfoCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfo(indexCache, 10);
        }
        private void testMergeOneNewEventInfoWithOverlap(IErrorIndex index, int numEventInfos, int overlap)
        {
            // Add event info - then add another new one.
            index.Activate();
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 39, new DateTime(102), "filename.dll", "1.2.3.4");
            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1", "param1value"));
            parameters.Add(new StackHashParameter("param2", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);

            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 20000, signature, -1, 2);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);

            StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
            int totalHits = 0;
            for (int i = 0; i < numEventInfos; i++)
            {
                if (i < numEventInfos - overlap)
                    totalHits += i + 1;

                StackHashEventInfo eventInfo = new StackHashEventInfo(DateTime.Now.AddDays(-3 * i).Date,
                    DateTime.Now.AddDays(-2 * i).Date, DateTime.Now.AddDays(-1 * i).Date, "English" + i.ToString(),
                    i, "locale" + i.ToString(), "OS" + i.ToString(), "OSVersion" + i.ToString(), i + 1);
                eventInfoCollection.Add(eventInfo);
            }

            index.MergeEventInfoCollection(product, file, theEvent, eventInfoCollection);


            StackHashEventInfoCollection eventInfoCollectionNew = new StackHashEventInfoCollection();
            for (int i = numEventInfos - overlap; i < numEventInfos * 2 - overlap; i++)
            {
                StackHashEventInfo eventInfo = new StackHashEventInfo(DateTime.Now.AddDays(-3 * i).Date,
                    DateTime.Now.AddDays(-2 * i).Date, DateTime.Now.AddDays(-1 * i).Date, "English" + i.ToString(),
                    i, "locale" + i.ToString(), "OS" + i.ToString(), "OSVersion" + i.ToString(), overlap + i + 1);

                if (i >= numEventInfos - overlap)
                    totalHits += overlap + i + 1;

                StackHashEventInfo foundEventInfo = eventInfoCollection.FindEventInfoByHitDate(eventInfo);

                if (foundEventInfo != null)
                    foundEventInfo.SetWinQualFields(eventInfo);
                else
                    eventInfoCollection.Add(eventInfo);

                eventInfoCollectionNew.Add(eventInfo);
            }

            index.MergeEventInfoCollection(product, file, theEvent, eventInfoCollectionNew);


            // Now get all the event info and make sure it all matches.
            StackHashEventInfoCollection eventInfoCollection2 = index.LoadEventInfoList(product, file, theEvent);

            Assert.AreEqual(0, eventInfoCollection.CompareTo(eventInfoCollection2));

            StackHashEventPackageCollection eventPackages = index.GetProductEvents(product);
            Assert.AreEqual(1, eventPackages.Count);
            Assert.AreEqual(totalHits, eventPackages[0].EventData.TotalHits);
        }

        [TestMethod]
        public void TestMergeOneNewEventInfoWithNoOverlap()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfoWithOverlap(index, 1, 0);
        }

        [TestMethod]
        public void TestMergeOneNewEventInfoWithNoOverlapCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfoWithOverlap(indexCache, 1, 0);
        }
        [TestMethod]
        public void TestMerge1NewEventInfoWith1Overlap()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfoWithOverlap(index, 1, 1);
        }

        [TestMethod]
        public void TestMerge1NewEventInfoWith1OverlapCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfoWithOverlap(indexCache, 1, 1);
        }
        [TestMethod]
        public void TestMerge2NewEventInfoWith1Overlap()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfoWithOverlap(index, 2, 1);
        }

        [TestMethod]
        public void TestMerge2NewEventInfoWith1OverlapCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfoWithOverlap(indexCache, 2, 1);
        }
        [TestMethod]
        public void TestMerge10NewEventInfoWith5verlap()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");

            testMergeOneNewEventInfoWithOverlap(index, 10, 5);
        }

        [TestMethod]
        public void TestMerge10NewEventInfoWith5OverlapCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testMergeOneNewEventInfoWithOverlap(indexCache, 10, 5);
        }
    }
}
