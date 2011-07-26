using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Data.SqlClient;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;


namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for EventUnitTests
    /// </summary>
    [TestClass]
    public class EventUnitTests
    {
        private string m_TempPath;
        private static string m_TestIndexFolder;
        private static string m_TestSqlIndexFolder;
        private Dictionary<int, StackHashEvent> m_ParsedEvents;
        private int m_ProcessEventCallCount;


        public EventUnitTests()
        {
        }

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) 
        {
            m_TestIndexFolder = Path.GetTempPath() + "StackHashTestIndex";
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            index.Deactivate();
            index.DeleteIndex();
            index.Activate();
            makeTestIndex(index, 3, 3, 3, 3, 3);

            m_TestSqlIndexFolder = "c:\\stackhashunittests\\StackHashTestIndexSql";
            //SqlErrorIndex sqlIndex = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_TestSqlIndexFolder);
            //sqlIndex.Deactivate();
            //sqlIndex.DeleteIndex();
            //sqlIndex.Activate();
            //makeTestIndex(sqlIndex, 3, 3, 3, 3, 3);
            //sqlIndex.Dispose();
            //sqlIndex = null;
        }

        [ClassCleanup()]
        public static void MyClassCleanup() 
        {
            if (Directory.Exists(m_TestIndexFolder))
                PathUtils.DeleteDirectory(m_TestIndexFolder, true);
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
            m_ParsedEvents = new Dictionary<int, StackHashEvent>();
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
            m_ProcessEventCallCount = 0;
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

        private void testAddEventNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashFile file = new StackHashFile();

                index.AddEvent(null, file, theEvent);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddEventNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventNullProduct(indexCache);
        }


        private void testAddEventUnknownProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 200,
                        "TestProduct1", 20, 30, "2.10.02123.1293");

                StackHashEvent theEvent = new StackHashEvent();
                StackHashFile file = new StackHashFile();

                index.AddProduct(product);
                product.Id = 201; // Make the product unrecognised.
                index.AddEvent(product, file, theEvent);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventUnknownProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddEventUnknownProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventUnknownProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventUnknownProduct(indexCache);
        }

        private void testAddEventNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 200, 
                        "TestProduct1", 20, 30, "2.10.02123.1293");

                StackHashEvent theEvent = new StackHashEvent();

                index.AddProduct(product);
                index.AddEvent(product, null, theEvent);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddEventNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventNullFile(indexCache);
        }


        private void testAddEventUnknownFile(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 200,
                        "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 10, DateTime.Now, "FileName", "Version");

                StackHashEvent theEvent = new StackHashEvent();

                index.AddProduct(product);
                index.AddEvent(product, file, theEvent);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventUnknownFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddEventUnknownFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddEventUnknownFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventUnknownFile(indexCache);
        }


        private void testAddEventNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 200,
                        "TestProduct1", 20, 30, "2.10.02123.1293");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 10, DateTime.Now, "FileName", "Version");

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddEventNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddEventNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddEventNullEvent(indexCache);
        }


        private void testAddNEventsToOneProductAndOneFile(IErrorIndex index, int numEvents)
        {
            index.Activate();
            StackHashEventCollection allEventsOut = new StackHashEventCollection();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1_" + i.ToString(), "param1value" + i.ToString()));
                parameters.Add(new StackHashParameter("param2_" + i.ToString(), "param2value" + i.ToString()));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);

                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", i + 1, signature, 99, 2);
                allEventsOut.Add(theEvent);

                index.AddEvent(product, file, theEvent);
            }

            StackHashEventCollection allEventsIn = index.LoadEventList(product, file);

            Assert.AreEqual(allEventsOut.Count, allEventsIn.Count);
            Assert.AreEqual(allEventsOut.Count, index.TotalStoredEvents);
            Assert.AreEqual(0, allEventsOut.CompareTo(allEventsIn));
        }

        [TestMethod]
        public void TestAdd1EventToOneProductAndOneFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddNEventsToOneProductAndOneFile(index, 1);
        }

        [TestMethod]
        public void TestAdd1EventToOneProductAndOneFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNEventsToOneProductAndOneFile(indexCache, 1);
        }

        [TestMethod]
        public void TestAdd2EventsToOneProductAndOneFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddNEventsToOneProductAndOneFile(index, 2);
        }

        [TestMethod]
        public void TestAdd2EventsToOneProductAndOneFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNEventsToOneProductAndOneFile(indexCache, 2);
        }

        [TestMethod]
        public void TestAdd100EventsToOneProductAndOneFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddNEventsToOneProductAndOneFile(index, 100);
        }

        [TestMethod]
        public void TestAdd100EventsToOneProductAndOneFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddNEventsToOneProductAndOneFile(indexCache, 100);
        }

        // The event data contains params as part of the signature. Some of these fields are well known as 
        // should be populated if not present using the params list.
        private void testLoad1EventShouldPopulateKnownFields(IErrorIndex index)
        {
            index.Activate();
            StackHashEventCollection allEventsOut = new StackHashEventCollection();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            StackHashParameterCollection parameters = new StackHashParameterCollection();

            StackHashEventSignature signature = new StackHashEventSignature(parameters);

            // Populate the known fields. Do this after construction because other wise the constructor will populate them.
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "CuckuSPMLaunch.exe"));
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "2.10.20509.1119"));
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, "2009-07-24T21:56:31"));
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "kernel32.dll"));
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "5.1.2600.5781"));
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, "2009-03-21T14:06:59"));
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "12345")); // this is hex
            parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x12afb"));

            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 1, signature, 99, 2);

            index.AddEvent(product, file, theEvent);

            StackHashEventCollection allEventsIn = index.LoadEventList(product, file);

            Assert.AreEqual(1, allEventsIn.Count);

            // The event fields should now be populated.
            Assert.AreEqual("CuckuSPMLaunch.exe", allEventsIn[0].EventSignature.ApplicationName);
            Assert.AreEqual("2.10.20509.1119", allEventsIn[0].EventSignature.ApplicationVersion);
            Assert.AreEqual("kernel32.dll", allEventsIn[0].EventSignature.ModuleName);
            Assert.AreEqual("5.1.2600.5781", allEventsIn[0].EventSignature.ModuleVersion);
            Assert.AreEqual(0x12345, allEventsIn[0].EventSignature.ExceptionCode);
            Assert.AreEqual(0x12afb, allEventsIn[0].EventSignature.Offset);

            Assert.AreEqual(2009, allEventsIn[0].EventSignature.ApplicationTimeStamp.Year);
            Assert.AreEqual(7, allEventsIn[0].EventSignature.ApplicationTimeStamp.Month);
            Assert.AreEqual(24, allEventsIn[0].EventSignature.ApplicationTimeStamp.Day);
            Assert.AreEqual(21, allEventsIn[0].EventSignature.ApplicationTimeStamp.Hour);
            Assert.AreEqual(56, allEventsIn[0].EventSignature.ApplicationTimeStamp.Minute);
            Assert.AreEqual(31, allEventsIn[0].EventSignature.ApplicationTimeStamp.Second);

            Assert.AreEqual(2009, allEventsIn[0].EventSignature.ModuleTimeStamp.Year);
            Assert.AreEqual(3, allEventsIn[0].EventSignature.ModuleTimeStamp.Month);
            Assert.AreEqual(21, allEventsIn[0].EventSignature.ModuleTimeStamp.Day);
            Assert.AreEqual(14, allEventsIn[0].EventSignature.ModuleTimeStamp.Hour);
            Assert.AreEqual(06, allEventsIn[0].EventSignature.ModuleTimeStamp.Minute);
            Assert.AreEqual(59, allEventsIn[0].EventSignature.ModuleTimeStamp.Second);
        }

        [TestMethod]
        public void TestLoad1EventShouldPopulateKnownFields()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testLoad1EventShouldPopulateKnownFields(index);
        }


        private void oneProductNEvent_ReturnAllEvents(IErrorIndex index, int numEvents, int numEventInfo, int numCabs)
        {
            index.Activate();
            StackHashEventPackageCollection allEventsOut = new StackHashEventPackageCollection();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            DateTime testTime = DateTime.Now.RoundToPreviousMinute();
            int totalHits = 0;
            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                for (int j = 0; j < numEventInfo; j++)
                {
                    StackHashEventInfo eventInfo =
                        new StackHashEventInfo(testTime, testTime, testTime, "English", i, "locale", "OS", "Version", i);
                    eventInfoCollection.Add(eventInfo);
                    totalHits += j;
                }

                StackHashCabPackageCollection cabCollection = new StackHashCabPackageCollection();

                for (int j = 0; j < numCabs; j++)
                {
                    StackHashCab cab =
                        new StackHashCab(testTime, testTime, i, "EventTypeName", "FileName", i, i * 1000);
                    cabCollection.Add(new StackHashCabPackage(cab, null, null, true));
                }

                
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("applicationName", "Cucku Backup " + i.ToString()));
                parameters.Add(new StackHashParameter("applicationVersion", "1.2.3." + i.ToString()));
                parameters.Add(new StackHashParameter("applicationTimeStamp", testTime.ToString(CultureInfo.InvariantCulture)));
                parameters.Add(new StackHashParameter("moduleName", "Module." + i.ToString()));
                parameters.Add(new StackHashParameter("moduleVersion", "2.3.4." + i.ToString()));
                parameters.Add(new StackHashParameter("moduleTimeStamp", testTime.ToString(CultureInfo.InvariantCulture)));
                parameters.Add(new StackHashParameter("exceptionCode", "0x1234"));
                parameters.Add(new StackHashParameter("offset", "0x1234"));
                
                StackHashEventSignature signature = new StackHashEventSignature(parameters);

                StackHashEvent theEvent = new StackHashEvent(testTime, testTime, "EventType1", i + 1, signature, totalHits, file.Id);
                StackHashEventPackage eventPackage = new StackHashEventPackage(
                    eventInfoCollection, cabCollection, theEvent, product.Id);
                allEventsOut.Add(eventPackage);

                index.AddEvent(product, file, theEvent);
                index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);

                foreach (StackHashCabPackage cab in cabCollection)
                {
                    index.AddCab(product, file, theEvent, cab.Cab, false);
                }
            }


            // Search for all.
            StackHashSearchOption fieldSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0);
            StackHashSearchOption fieldSearchOption2 = new IntSearchOption(
                StackHashObjectType.File, "Id", StackHashSearchOptionType.All, 0, 0);
            StackHashSearchOption fieldSearchOption3 = new IntSearchOption(
                StackHashObjectType.Event, "Id", StackHashSearchOptionType.All, 0, 0);
            StackHashSearchOption fieldSearchOption4 = new IntSearchOption(
                StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.All, 0, 0);
            StackHashSearchOption fieldSearchOption5 = new DateTimeSearchOption(
                StackHashObjectType.EventInfo, "DateCreatedLocal", StackHashSearchOptionType.All, DateTime.Now, DateTime.Now);

            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection();
            fieldSearchOptions.Add(fieldSearchOption1);
            fieldSearchOptions.Add(fieldSearchOption2);
            fieldSearchOptions.Add(fieldSearchOption3);
            fieldSearchOptions.Add(fieldSearchOption4);
            fieldSearchOptions.Add(fieldSearchOption5);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEventsIn = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(allEventsOut.Count, allEventsIn.Count);

            Assert.AreEqual(0, allEventsOut.CompareTo(allEventsIn));
        }

        [TestMethod]
        public void OneProduct1Event_ReturnAllEventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            oneProductNEvent_ReturnAllEvents(indexCache, 1, 0, 0);
        }

        [TestMethod]
        public void OneProduct1Event_ReturnAllEventsSql()
        {
            SqlErrorIndex index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_TestSqlIndexFolder);
            try
            {
                SqlConnection.ClearAllPools();
                index.Deactivate();
                index.DeleteIndex();
                oneProductNEvent_ReturnAllEvents(index, 1, 0, 0);
            }
            finally
            {
                SqlConnection.ClearAllPools();
                index.Deactivate();
                index.DeleteIndex();
                index.Dispose();
            }
        }

        [TestMethod]
        public void OneProduct5Events_ReturnAllEventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            oneProductNEvent_ReturnAllEvents(indexCache, 5, 0, 0);
        }

        [TestMethod]
        public void OneProduct1Event1EventInfo_ReturnAllEventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            oneProductNEvent_ReturnAllEvents(indexCache, 1, 1, 0);
        }

        [TestMethod]
        public void OneProduct1Event1CabInfo_ReturnAllEventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            oneProductNEvent_ReturnAllEvents(indexCache, 1, 0, 1);
        }
        [TestMethod]
        public void OneProduct1Event1EventInfo1CabInfo_ReturnAllEventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            oneProductNEvent_ReturnAllEvents(indexCache, 1, 1, 1);
        }
        private static void makeTestIndex(
            IErrorIndex index, int numProducts, int numFiles, int numEvents, int numEventInfo, int numCabs)
        {
            index.Activate();
            StackHashEventPackageCollection allEventsOut = new StackHashEventPackageCollection();

            int productId = 0;
            int fileId = 0;
            int eventId = 0;
            int cabId = 0;

            int eventHits = 1;

            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                productId++;
                StackHashProduct product =
                    new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com",
                        productId, "TestProduct" + productId.ToString(), productId, 30, "2.10.02123.1293");

                for (int fileCount = 0; fileCount < numFiles; fileCount++)
                {
                    fileId++;
                    DateTime fileTime = new DateTime(2010, 02, 1);
                    StackHashFile file = new StackHashFile(fileTime.AddDays(fileCount), fileTime.AddDays(fileCount + 1), fileId, 
                        fileTime.AddDays(fileCount + 2), "filename.dll", "1.2.3.4");

                    index.AddProduct(product);
                    index.AddFile(product, file);

                    for (int eventCount = 0; eventCount < numEvents; eventCount++)
                    {
                        eventId++;
                        StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                        int totalHits = 0;
                        for (int eventInfoCount = 0; eventInfoCount < numEventInfo; eventInfoCount++)
                        {
                            StackHashEventInfo eventInfo =
                                new StackHashEventInfo(fileTime.AddDays(eventInfoCount), fileTime.AddDays(eventInfoCount), fileTime.AddDays(eventInfoCount), "English", eventInfoCount, "locale", "OS", "Version", eventHits++);
                            eventInfoCollection.Add(eventInfo);
                            totalHits += eventInfo.TotalHits;
                        }

                        StackHashCabCollection cabCollection = new StackHashCabCollection();

                        for (int cabCount = 0; cabCount < numCabs; cabCount++)
                        {
                            cabId++;
                            StackHashCab cab =
                                new StackHashCab(DateTime.Now, DateTime.Now, cabId, "EventTypeName", "FileName", cabId, cabId * 1000);
                            cabCollection.Add(cab);
                        }


                        StackHashParameterCollection parameters = new StackHashParameterCollection();
                        String versionString = eventId.ToString() + "." + eventId.ToString();
                        parameters.Add(new StackHashParameter("moduleVersion", versionString));
                        parameters.Add(new StackHashParameter("moduleName" , "ModuleName" + eventId.ToString()));
                        StackHashEventSignature signature = new StackHashEventSignature(parameters);
                        signature.InterpretParameters();

                        StackHashEvent theEvent = new StackHashEvent(fileTime.AddDays(eventCount), fileTime.AddDays(eventCount + 1), "EventType1", eventId, signature, totalHits, 2);
                        StackHashEventPackage eventPackage = new StackHashEventPackage(
                            eventInfoCollection, new StackHashCabPackageCollection(cabCollection), theEvent, product.Id);
                        allEventsOut.Add(eventPackage);

                        index.AddEvent(product, file, theEvent);
                        index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);

                        foreach (StackHashCab cab in cabCollection)
                        {
                            index.AddCab(product, file, theEvent, cab, false);
                        }
                    }
                }
            }
        }

        void searchForCabId2(IErrorIndex index)
        {
            index.Activate();
            // The index will contain 3 product with 3 files each of 3 events with 3 event infos and 3 cabs.

            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 3, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(1, allEvents.Count);   // Number of events.
            Assert.AreEqual(3, allEvents[0].Cabs.Count); // Number of cabs in the event.
            Assert.AreEqual(true, (allEvents[0].Cabs[0].Cab.Id == 2) || (allEvents[0].Cabs[1].Cab.Id == 2) || (allEvents[0].Cabs[2].Cab.Id == 2));  // Make sure the id matches.
        }

        [TestMethod]
        public void TestSearchForCabId2Cached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForCabId2(indexCache);
        }

        void searchForCabIdGreaterThanID20(IErrorIndex index)
        {
            index.Activate();
            // The index will contain 3 product with 3 files each of 3 events with 3 event infos and 3 cabs so 81 cabs in total.
            int totalCabs = 3 * 3 * 3 * 3;

            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.GreaterThan, totalCabs - 10, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(4, allEvents.Count);   // Number of events.
        }

        [TestMethod]
        public void TestSearchForCabIdGreaterThan20Cached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForCabIdGreaterThanID20(indexCache);
        }


        // Shouldn't find any matches.
        void searchForEventIdCabIdNoMatch(IErrorIndex index)
        {
            index.Activate();
            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.RangeExclusive, 20, 30),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 5, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(0, allEvents.Count);   // Number of events.
        }

        [TestMethod]
        public void SearchForEventIdCabIdNoMatchCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForEventIdCabIdNoMatch(indexCache);
        }

        // Should find 1 match.
        void searchForEventIdCabIdMatch(IErrorIndex index)
        {
            index.Activate();
            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.RangeExclusive, 1, 7),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 5, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(1, allEvents.Count);   // Number of events.
        }

        [TestMethod]
        public void SearchForEventIdCabIdMatchCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForEventIdCabIdMatch(indexCache);
        }

        void searchForFileIdEventIdCabIdMatch(IErrorIndex index)
        {
            index.Activate();
            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.NotEqual, 2, 0),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.RangeInclusive, 1, 6),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.All, 0, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(3, allEvents.Count);   // Number of events.
        }

        [TestMethod]
        public void SearchForFileIdEventIdCabIdMatchCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForFileIdEventIdCabIdMatch(indexCache);
        }

        void searchForProductIdFileIdEventIdCabIdMatch(IErrorIndex index)
        {
            index.Activate();
            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, 2, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.NotEqual, 8, 0),
                new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.All, 0, 0),
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.All, 0, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(6, allEvents.Count);   // Number of events.
        }

        [TestMethod]
        public void SearchForProductIdFileIdEventIdCabIdMatchCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForProductIdFileIdEventIdCabIdMatch(indexCache);
        }

        void searchForHitsLessOrEqualToX(IErrorIndex index)
        {
            index.Activate();

            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Event, "TotalHits", StackHashSearchOptionType.LessThanOrEqual, 19, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            // Hits will be...
            // Event 1 - 1 + 2 + 3 = 6
            // Event 2 - 4 + 5 + 6 = 15
            // etc...

            Assert.AreEqual(2, allEvents.Count);   // Number of events.

            foreach (StackHashEventPackage package in allEvents)
            {
                Assert.AreEqual(true, (package.EventData.TotalHits == 6) || (package.EventData.TotalHits == 15));
            }
        }

        [TestMethod]
        public void SearchForHitsLessOrEqualToXCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForHitsLessOrEqualToX(indexCache);
        }

        void searchForHitsGreaterThanX(IErrorIndex index)
        {
            index.Activate();

            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Event, "TotalHits", StackHashSearchOptionType.GreaterThan, 229, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            // Hits will be...
            // Event 1 - 1 + 2 + 3 = 6
            // Event 2 - 4 + 5 + 6 = 15
            // Event 3 - 7 + 8 + 9 = 24 etc.. 
            // etc...
            // nth event is 9n - 3.
            // There are 27 events in total so to get the last 3 events we need 9(26) -3 = 231


            Assert.AreEqual(2, allEvents.Count);   // Number of events.

            foreach (StackHashEventPackage package in allEvents)
            {
                Assert.AreEqual(true, (package.EventData.TotalHits == 231) || (package.EventData.TotalHits == 240));
            }
        }

        [TestMethod]
        public void SearchForHitsGreaterThanXCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForHitsGreaterThanX(indexCache);
        }

        void searchFor2IdenticalCriteriaDuplicateEventsFog426(IErrorIndex index)
        {
            index.Activate();

            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Event, "TotalHits", StackHashSearchOptionType.GreaterThan, 229, 0),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            // Hits will be...
            // Event 1 - 1 + 2 + 3 = 6
            // Event 2 - 4 + 5 + 6 = 15
            // Event 3 - 7 + 8 + 9 = 24 etc.. 
            // etc...
            // nth event is 9n - 3.
            // There are 27 events in total so to get the last 3 events we need 9(26) -3 = 231


            Assert.AreEqual(2, allEvents.Count);   // Number of events.

            foreach (StackHashEventPackage package in allEvents)
            {
                Assert.AreEqual(true, (package.EventData.TotalHits == 231) || (package.EventData.TotalHits == 240));
            }
        }

        [TestMethod]
        public void SearchFor2IdenticalCriteriaDuplicateEventsFog426Cached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchFor2IdenticalCriteriaDuplicateEventsFog426(indexCache);
        }

        
        void searchForProductIdFileIdEventSignatureMatch(IErrorIndex index)
        {
            index.Activate();
            // Search for all.
            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, 2, 0),
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.NotEqual, 8, 0),
                new StringSearchOption(StackHashObjectType.EventSignature, "ModuleName", StackHashSearchOptionType.StringContains, "ModuleName1", "", false),
            };

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            criteriaCollection.Add(criteria1);

            StackHashEventPackageCollection allEvents = index.GetEvents(criteriaCollection, null);

            Assert.AreEqual(1, allEvents.Count);   // Number of events.
        }

        [TestMethod]
        public void SearchForProductIdFileIdEventSignatureMatch()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TestIndexFolder, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            searchForProductIdFileIdEventSignatureMatch(indexCache);
        }
        private void testAddSetFileIdIfNotSetOnLoad(IErrorIndex index)
        {
            index.Activate();
            StackHashEventCollection allEventsOut = new StackHashEventCollection();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 27, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            StackHashParameterCollection parameters = new StackHashParameterCollection();
            parameters.Add(new StackHashParameter("param1_", "param1value"));
            parameters.Add(new StackHashParameter("param2_", "param2value"));
            StackHashEventSignature signature = new StackHashEventSignature(parameters);

            StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", 1, signature, 99, 0);

            index.AddEvent(product, file, theEvent);

            theEvent.FileId = 27; // Set this now for the comparison later.
            allEventsOut.Add(theEvent);

            StackHashEventCollection allEventsIn = index.LoadEventList(product, file);

            Assert.AreEqual(allEventsOut.Count, allEventsIn.Count);

            Assert.AreEqual(0, allEventsOut.CompareTo(allEventsIn));
        }

        [TestMethod]
        public void TestAddSetFileIdIfNotSetOnLoad()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            testAddSetFileIdIfNotSetOnLoad(index);
        }

        [TestMethod]
        public void TestAddSetFileIdIfNotSetOnLoadCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddSetFileIdIfNotSetOnLoad(indexCache);
        }



        /// <summary>
        /// Called when an event is found. 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        private void processEvent(Object sender, ErrorIndexParseEventsEventArgs e)
        {
            StackHashEvent currentEvent = e.Parser.CurrentEvent;

            m_ParsedEvents[currentEvent.Id] = currentEvent;
            m_ProcessEventCallCount++;
        }

        private void parseEventsNoEvents(IErrorIndex index, int numEvents)
        {
            index.Activate();

            ErrorIndexEventParser parser = new ErrorIndexEventParser();

            parser.ParseEvent += new EventHandler<ErrorIndexParseEventsEventArgs>(this.processEvent);

            StackHashEventCollection allEventsOut = new StackHashEventCollection();

            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 27, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1_", "param1value"));
                parameters.Add(new StackHashParameter("param2_", "param2value"));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);

                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", i + 1, signature, 99, 0);

                index.AddEvent(product, file, theEvent);

                theEvent.FileId = 27; // Set this now for the comparison later.

                allEventsOut.Add(theEvent);
            }

            Assert.AreEqual(true, index.ParseEvents(product, file, parser));

            Assert.AreEqual(allEventsOut.Count, m_ParsedEvents.Count);

            foreach (StackHashEvent thisEvent in allEventsOut)
            {
                StackHashEvent parsedEvent = m_ParsedEvents[thisEvent.Id];

                Assert.AreEqual(0, parsedEvent.CompareTo(thisEvent));
            }

            parser.ParseEvent -= new EventHandler<ErrorIndexParseEventsEventArgs>(this.processEvent);

        }

        [TestMethod]
        [ExpectedException(typeof(System.NotImplementedException))]
        public void parseEventsNoEvents()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            parseEventsNoEvents(index, 0);
        }

        [TestMethod]
        public void parseEventsNoEventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            parseEventsNoEvents(indexCache, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(System.NotImplementedException))]
        public void parseEvents1Event()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            parseEventsNoEvents(index, 1);
        }

        [TestMethod]
        public void parseEvents1EventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            parseEventsNoEvents(indexCache, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(System.NotImplementedException))]
        public void parseEvents10Events()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            parseEventsNoEvents(index, 10);
        }

        [TestMethod]
        public void parseEvents10EventsCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            parseEventsNoEvents(indexCache, 10);
        }

        public void totalStoredEventsNone(IErrorIndex index, int numEvents)
        {
            StackHashProduct product =
                new StackHashProduct(DateTime.Now, DateTime.Now, "http://www.cucku.com", 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file = new StackHashFile(new DateTime(100), new DateTime(101), 1, new DateTime(102), "filename.dll", "1.2.3.4");

            index.AddProduct(product);
            index.AddFile(product, file);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashParameterCollection parameters = new StackHashParameterCollection();
                parameters.Add(new StackHashParameter("param1_" + i.ToString(), "param1value" + i.ToString()));
                parameters.Add(new StackHashParameter("param2_" + i.ToString(), "param2value" + i.ToString()));
                StackHashEventSignature signature = new StackHashEventSignature(parameters);

                StackHashEvent theEvent = new StackHashEvent(new DateTime(102), new DateTime(103), "EventType1", i + 1, signature, 99, 2);

                index.AddEvent(product, file, theEvent);
            }

            
            StackHashProduct product2 = index.UpdateProductStatistics(product);
            Assert.AreEqual(numEvents, product2.TotalStoredEvents);

            StackHashProduct product3 = index.GetProduct(product.Id);
            Assert.AreEqual(numEvents, product3.TotalStoredEvents);

            Assert.AreEqual(numEvents, index.TotalStoredEvents);
        }

        [TestMethod]
        public void TotalStoredEventsNone()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            index.Activate();
            totalStoredEventsNone(index, 0);

        }
        [TestMethod]
        public void TotalStoredEventsNoneCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            indexCache.Activate();
            totalStoredEventsNone(indexCache, 0);
        }

        [TestMethod]
        public void TotalStoredEvents1()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            index.Activate();
            totalStoredEventsNone(index, 1);

        }
        [TestMethod]
        public void TotalStoredEvents1Cached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            indexCache.Activate();
            totalStoredEventsNone(indexCache, 1);
        }
        [TestMethod]
        public void TotalStoredEvents10()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            index.Activate();
            totalStoredEventsNone(index, 10);

        }
        [TestMethod]
        public void TotalStoredEvents10Cached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            indexCache.Activate();
            totalStoredEventsNone(indexCache, 10);
        }

        [TestMethod]
        public void TotalStoredEvents10Reload()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            index.Activate();
            totalStoredEventsNone(index, 10);

            index = new XmlErrorIndex(m_TempPath, "TestIndex");
            index.Activate();
            totalStoredEventsNone(index, 10);
        }
        [TestMethod]
        public void TotalStoredEvents10CachedReload()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "TestIndex");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            indexCache.Activate();
            totalStoredEventsNone(indexCache, 10);

            index = new XmlErrorIndex(m_TempPath, "TestIndex");
            indexCache = new ErrorIndexCache(index);
            indexCache.Activate();
            totalStoredEventsNone(indexCache, 10);
        }

    }
}
