using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
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
    /// Summary description for SqlEventPackagesUnitTests
    /// </summary>
    [TestClass]
    public class SqlEventPackagesUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlEventPackagesUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

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
        /// Add n event notes 
        /// </summary>
        public void getPackages(int numProducts, int numFiles, int numEvents, int numEventInfos, int numCabs, bool useSameLocale, bool incrementingEventId, bool randomEventIds)
        {
            int cabId = 1000;
            int eventId = 1320080390;
            int fileId = 0x12345678;
            int productId = 122;
            Random rand = new Random(100);


            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            Dictionary<int, StackHashEventPackageCollection> expectedProductEvents = new Dictionary<int,StackHashEventPackageCollection>();
            StackHashProductCollection allProducts = new StackHashProductCollection();

            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, productId++, "TestProduct1", 20, 30, "2.10.02123.1293");

                m_Index.AddProduct(product1);
                allProducts.Add(product1);
                StackHashEventPackageCollection allAddedEvents = new StackHashEventPackageCollection();

                for (int fileCount = 0; fileCount < numFiles; fileCount++)
                {
                    StackHashFile file1 =
                        new StackHashFile(creationDateTime, modifiedDateTime, fileId++, creationDateTime, "File1.dll", "2.3.4.5");

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

                    m_Index.AddFile(product1, file1);

                    for (int eventCount = 0; eventCount < numEvents; eventCount++)
                    {
                        StackHashEvent event1 =
                            new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", eventId, eventSignature, eventCount, file1.Id, "bug" + eventCount.ToString());


                        if (randomEventIds)
                        {
                            eventId = rand.Next(100, 1320080390);
                        }
                        else
                        {
                            if (incrementingEventId)
                                eventId++;
                            else
                                eventId--;
                        }
                        m_Index.AddEvent(product1, file1, event1);

                        StackHashEventInfoCollection eventInfos = new StackHashEventInfoCollection();

                        int hitCount = 1;
                        int totalHits = 0;
                        for (int i = 0; i < numEventInfos; i++)
                        {
                            DateTime nowTime = DateTime.Now;
                            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                            int localeId = 10;
                            if (!useSameLocale)
                                localeId += i;

                            totalHits += hitCount;
                            StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i * 1), date.AddDays(i * 2), date.AddDays(i * 3), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", hitCount++);

                            eventInfos.Add(eventInfo);
                        }
                        event1.TotalHits = totalHits;
                        m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);

                        StackHashCabCollection cabs = new StackHashCabCollection();

                        for (int i = 0; i < numCabs; i++)
                        {
                            DateTime nowTime = DateTime.Now;
                            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", cabId++, i * 2000);
                            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

                            cab.CabDownloaded = false;
                            cabs.Add(cab);
                            m_Index.AddCab(product1, file1, event1, cab, true);
                        }

                        allAddedEvents.Add(new StackHashEventPackage(eventInfos, new StackHashCabPackageCollection(cabs), event1, product1.Id));
                    }
                }

                expectedProductEvents[product1.Id] = allAddedEvents;

            }


            foreach (StackHashProduct product in allProducts)
            {
                StackHashEventPackageCollection allEvents = m_Index.GetProductEvents(product);

                StackHashEventPackageCollection expectedEvents = expectedProductEvents[product.Id];

                Assert.AreNotEqual(null, allEvents);
                Assert.AreEqual(expectedEvents.Count, allEvents.Count);

                foreach (StackHashEventPackage package in allEvents)
                {
                    StackHashEventPackage matchedEvent = expectedEvents.FindEventPackage(package.EventData.Id, package.EventData.EventTypeName);

                    Assert.AreNotEqual(null, matchedEvent);

                    Assert.AreEqual(0, package.EventInfoList.CompareTo(matchedEvent.EventInfoList));
                    Assert.AreEqual(0, package.Cabs.CompareTo(matchedEvent.Cabs));
                }
            }
        }


        /// <summary>
        /// GetProductEvents - no events.
        /// </summary>
        [TestMethod]
        public void GetProductEvents0Events()
        {
            getPackages(1, 1, 0, 0, 0, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 1 event - no hits or cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event0Hits0Cabs()
        {
            getPackages(1, 1, 1, 0, 0, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 1 event - 1 hits - 0 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event1Hits0Cabs()
        {
            getPackages(1, 1, 1, 1, 0, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 1 event - 1 hits - 1 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event1Hits1Cabs()
        {
            getPackages(1, 1, 1, 1, 1, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 1 event - 2 hits - 1 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event2Hits1Cabs()
        {
            getPackages(1, 1, 1, 2, 1, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 1 event - 2 hits - 2 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event2Hits2Cabs()
        {
            getPackages(1, 1, 1, 2, 2, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 2 events - 0 hits - 0 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents2Event0Hits0Cabs()
        {
            getPackages(1, 1, 2, 0, 0, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 2 events - 1 hits - 1 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents2Event1Hits1Cabs()
        {
            getPackages(1, 1, 2, 1, 1, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 2 events - 2 hits - 2 cabs.
        /// </summary>
        [TestMethod]
        public void GetProductEvents2Event2Hits2Cabs()
        {
            getPackages(1, 1, 2, 2, 2, false, true, false);
        }

        /// <summary>
        /// GetProductEvents - 1 events - 110 hits - 120 cabs.
        /// Over 100 - because they are retrieved in blocks of 100.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event110Hits120Cabs()
        {
            getPackages(1, 1, 1, 110, 120, false, true, false);
        }

        /// <summary>
        /// PERFORMANCE TEST.
        /// GetProductEvents - 1000 events - 110 hits - 120 cabs.
        /// Over 100 - because they are retrieved in blocks of 100.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void GetProductEvents1000Event110Hits120Cabs()
        {
            DateTime startTime = DateTime.Now;

            getPackages(1, 1, 1000, 110, 120, false, true, false);

            DateTime endTime = DateTime.Now;
        }

        
        /// <summary>
        /// GetProductEvents - 1 events - 110 hits - 120 cabs - decrementing event id.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Event110Hits120CabsDecrementingEventId()
        {
            getPackages(1, 1, 1, 110, 120, false, false, false);
        }

        /// <summary>
        /// GetProductEvents - 20 events - 2 hits - 2 cabs - random event ID.
        /// </summary>
        [TestMethod]
        public void GetProductEvents20Event2Hits2CabsRandomEventId()
        {
            getPackages(1, 1, 20, 2, 2, false, false, true);
        }

        /// <summary>
        /// GetProductEvents - 1 product - 2 files - 20 events - 2 hits - 2 cabs - random event ID.
        /// </summary>
        [TestMethod]
        public void GetProductEvents1Product2Files20Event2Hits2CabsRandomEventId()
        {
            getPackages(1, 2, 20, 2, 2, false, false, true);
        }

        /// <summary>
        /// GetProductEvents - 2 product - 1 files - 20 events - 2 hits - 2 cabs - random event ID.
        /// </summary>
        [TestMethod]
        public void GetProductEvents2Product1File20Event2Hits2CabsRandomEventId()
        {
            getPackages(2, 1, 20, 2, 2, false, false, true);
        }

        /// <summary>
        /// GetProductEvents - 2 products - 2 files - 20 events - 2 hits - 2 cabs - random event ID.
        /// </summary>
        [TestMethod]
        public void GetProductEvents2Product2File20Event2Hits2CabsRandomEventId()
        {
            getPackages(2, 2, 20, 2, 2, false, false, true);
        }

        /// <summary>
        /// GetProductEvents - 20 products - 5 files - 4 events - 3 hits - 2 cabs - random event ID.
        /// </summary>
        [TestMethod]
        public void GetProductEvents20Product5File4Event3Hits2CabsRandomEventId()
        {
            getPackages(20, 5, 4, 3, 2, false, false, true);
        }
    }
}
