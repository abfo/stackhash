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
    /// Summary description for SqlCabsUnitTest
    /// </summary>
    [TestClass]
    public class SqlCabsUnitTest
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlCabsUnitTest()
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
        public void addCabs(int numCabs, bool checkCabCount, bool setDownloaded)
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

            StackHashCabCollection cabs = new StackHashCabCollection();

            for (int i = 0; i < numCabs; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
                cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

                cab.CabDownloaded = setDownloaded;
                cabs.Add(cab);
                m_Index.AddCab(product1, file1, event1, cab, true);
            }

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(numCabs, cabsRetrieved.Count);

            for (int i = 0; i < numCabs; i++)
            {
                Assert.AreEqual(0, cabsRetrieved[i].CompareTo(cabs[i]));
                Assert.AreEqual(0, cabsRetrieved[i].CompareDiagnostics(cabs[i]));
            }

            if (checkCabCount)
            {
                int retrievedNumCabs = m_Index.GetCabCount(product1, file1, event1);

                Assert.AreEqual(numCabs, retrievedNumCabs);
            }

            // Make sure the event functions return the correct number of cabs.
            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event1);
            Assert.AreEqual(numCabs, retrievedEvent.CabCount);

            StackHashEventCollection eventList = m_Index.LoadEventList(product1, file1);
            Assert.AreEqual(1, eventList.Count);
            Assert.AreEqual(numCabs, eventList[0].CabCount);

            StackHashEventPackageCollection productEvents = m_Index.GetProductEvents(product1);
            Assert.AreEqual(1, productEvents.Count);
            Assert.AreEqual(numCabs, productEvents[0].EventData.CabCount);

            StackHashSearchCriteria criteria = new StackHashSearchCriteria();
            criteria.SearchFieldOptions.Add(new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.Equal, event1.Id, event1.Id));
            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection();
            allCriteria.Add(criteria);
            StackHashEventPackageCollection searchEvents = m_Index.GetEvents(allCriteria, null);
            Assert.AreEqual(1, searchEvents.Count);
            Assert.AreEqual(numCabs, searchEvents[0].EventData.CabCount);
        }


        /// <summary>
        /// Add and Get 0 cabs
        /// </summary>
        [TestMethod]
        public void AddGetNoCab()
        {
            addCabs(0, false, false);
        }

        /// <summary>
        /// Add one cab
        /// </summary>
        [TestMethod]
        public void AddGet1Cab()
        {
            addCabs(1, false, false);
        }

        /// <summary>
        /// Add 2 cabs
        /// </summary>
        [TestMethod]
        public void AddGet2Cab()
        {
            addCabs(2, false, true);
        }

        /// <summary>
        /// Add 10 cabs
        /// </summary>
        [TestMethod]
        public void AddGet10Cab()
        {
            addCabs(10, false, false);
        }

        /// <summary>
        /// Add 100 cabs
        /// </summary>
        [TestMethod]
        public void AddGet100Cab()
        {
            addCabs(100, false, false);
        }


        /// <summary>
        /// Updates the cab - all fields.
        /// </summary>
        [TestMethod]
        public void UpdateCab()
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

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab1 = new StackHashCab(date.AddDays(1), date.AddDays(2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", 1000, 2000);
            cab1.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event1.Id, event1.EventTypeName, "cab12345_232322.cab", 1000, 2001);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mins2", "2.120.222.11212122", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)2", "64 bit windows2");

            m_Index.AddCab(product1, file1, event1, cab1, true);
            m_Index.AddCab(product1, file1, event1, cab2, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(1, cabsRetrieved.Count);

            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab2));
        }

        /// <summary>
        /// Updates the cab - all fields not dumpanalysis.
        /// </summary>
        [TestMethod]
        public void UpdateCabNotDumpAnalysis()
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

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab1 = new StackHashCab(date.AddDays(1), date.AddDays(2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", 1000, 2000);
            cab1.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event1.Id, event1.EventTypeName, "cab12345_232322.cab", 1000, 2001);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mins2", "2.120.222.11212122", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)2", "64 bit windows2");

            m_Index.AddCab(product1, file1, event1, cab1, true);
            m_Index.AddCab(product1, file1, event1, cab2, false);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(1, cabsRetrieved.Count);

            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab1));
        }


        /// <summary>
        /// Add cab - and GetCabs for different event. Should return 0 cabs.
        /// </summary>
        [TestMethod]
        public void GetCabsForDifferentEventIdNoCabs()
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

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab1 = new StackHashCab(date.AddDays(1), date.AddDays(2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", 1000, 2000);
            cab1.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddCab(product1, file1, event1, cab1, true);

            event1.Id++; // Get cabs for a different event.
            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(0, cabsRetrieved.Count);
        }


        /// <summary>
        /// Add cab - and GetCabs for different event. Should return 1 cab.
        /// </summary>
        [TestMethod]
        public void GetCabsForDifferentEventId1Cab()
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

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 21, file1.Id, "bug2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab1 = new StackHashCab(date.AddDays(1), date.AddDays(2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", 1000, 2000);
            cab1.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event2.Id, event2.EventTypeName, "cab12345_232322.cab", 1001, 2000);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mins", "2.120.222.11212122", "Microsoft Windows Vista X64 6.0.212121212 (Build 25002)", "64 bit windows2");

            m_Index.AddCab(product1, file1, event1, cab1, true);
            m_Index.AddCab(product1, file1, event2, cab2, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab1));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab1));

            cabsRetrieved = m_Index.LoadCabList(product1, file1, event2);

            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab2));
        }


        /// <summary>
        /// Add a cab note.
        /// </summary>
        public void addCabNotes(int numNotes)
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

            StackHashCabCollection cabs = new StackHashCabCollection();

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", 
                "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            cabs.Add(cab);
            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(1, cabsRetrieved.Count);

            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab));


            StackHashNotes originalNotes = new StackHashNotes();

            for (int j = 0; j < numNotes; j++)
            {
                StackHashNoteEntry note = new StackHashNoteEntry(date.AddDays(j), "Source" + j.ToString(), "User" + j.ToString(), "This is a note for the cab file to see if it is stored ok" + j.ToString());
                int cabNoteId = m_Index.AddCabNote(product1, file1, event1, cab, note);

                Assert.AreEqual(j + 1, cabNoteId);

                originalNotes.Add(note);
            }

            // Get the cab notes.
            StackHashNotes notes = m_Index.GetCabNotes(product1, file1, event1, cab);

            Assert.AreEqual(numNotes, notes.Count);

            for (int j = 0; j < numNotes; j++)
            {
                Assert.AreEqual(0, notes[j].CompareTo(originalNotes[j]));


                // Get the individual cab note by index (should be 1,2,3,4...)
                StackHashNoteEntry thisNote = m_Index.GetCabNote(j + 1);

                Assert.AreEqual(0, originalNotes[j].CompareTo(thisNote));
            }
        }

        /// <summary>
        /// Add and Get 0 cab notes
        /// </summary>
        [TestMethod]
        public void AddGetNoCabNotes()
        {
            addCabNotes(0);
        }

        /// <summary>
        /// Add and Get 1 cab notes
        /// </summary>
        [TestMethod]
        public void AddGet1CabNotes()
        {
            addCabNotes(1);
        }

        /// <summary>
        /// Add and Get 2 cab notes
        /// </summary>
        [TestMethod]
        public void AddGet2CabNotes()
        {
            addCabNotes(2);
        }

        /// <summary>
        /// Add and Get 10 cab notes
        /// </summary>
        [TestMethod]
        public void AddGet10CabNotes()
        {
            addCabNotes(10);
        }

        /// <summary>
        /// Add and Get 100 cab notes
        /// </summary>
        [TestMethod]
        public void AddGet100CabNotes()
        {
            addCabNotes(100);
        }


        /// <summary>
        /// Add a cab note.
        /// </summary>
        public void replaceCabNotes(int numNotes)
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

            StackHashCabCollection cabs = new StackHashCabCollection();

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212",
                "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            cabs.Add(cab);
            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(1, cabsRetrieved.Count);

            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab));


            StackHashNotes originalNotes = new StackHashNotes();

            for (int j = 0; j < numNotes; j++)
            {
                StackHashNoteEntry note = new StackHashNoteEntry(date.AddDays(j), "Source" + j.ToString(), "User" + j.ToString(), "This is a note for the cab file to see if it is stored ok" + j.ToString());
                int cabNoteId = m_Index.AddCabNote(product1, file1, event1, cab, note);

                Assert.AreEqual(j + 1, cabNoteId);

                originalNotes.Add(note);

                // Now replace the enty with different text.
                note.Note = note.Note + "NEW";
                note.NoteId = cabNoteId;

                int cabNoteId2 = m_Index.AddCabNote(product1, file1, event1, cab, note);

                Assert.AreEqual(cabNoteId, cabNoteId2);

            }

            // Get the cab notes.
            StackHashNotes notes = m_Index.GetCabNotes(product1, file1, event1, cab);

            Assert.AreEqual(numNotes, notes.Count);

            for (int j = 0; j < numNotes; j++)
            {
                Assert.AreEqual(0, notes[j].CompareTo(originalNotes[j]));


                // Get the individual cab note by index (should be 1,2,3,4...)
                StackHashNoteEntry thisNote = m_Index.GetCabNote(j + 1);

                Assert.AreEqual(0, originalNotes[j].CompareTo(thisNote));
                Assert.AreEqual(0, originalNotes[j].CompareTo(notes[j]));
            }
        }

        /// <summary>
        /// Replace and Get 1 cab notes
        /// </summary>
        [TestMethod]
        public void ReplaceCabNotes()
        {
            replaceCabNotes(1);
        }

        /// <summary>
        /// Replace and Get 10 cab notes
        /// </summary>
        [TestMethod]
        public void Replace10CabNotes()
        {
            replaceCabNotes(10);
        }

        
        /// <summary>
        /// Add a cab note.
        /// </summary>
        [TestMethod]
        public void GetNotesForDifferentCab()
        {
            int numNotes = 1;
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

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

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event1.Id, event1.EventTypeName, "cab123452_23232.cab", i + 1001, i * 20001);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mi2ns", "2.120.222.11221212", "Microsoft Win2dows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddCab(product1, file1, event1, cab2, true);
            
            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(2, cabsRetrieved.Count);

            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab));
            Assert.AreEqual(0, cabsRetrieved[1].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[1].CompareDiagnostics(cab2));


            StackHashNotes originalNotes = new StackHashNotes();

            for (int j = 0; j < numNotes; j++)
            {
                StackHashNoteEntry note = new StackHashNoteEntry(date.AddDays(j), "Source" + j.ToString(), "User" + j.ToString(), "This is a note for the cab file to see if it is stored ok" + j.ToString());
                m_Index.AddCabNote(product1, file1, event1, cab, note);
                originalNotes.Add(note);
            }

            // Get the cab notes.
            StackHashNotes notes = m_Index.GetCabNotes(product1, file1, event1, cab);

            Assert.AreEqual(numNotes, notes.Count);

            for (int j = 0; j < numNotes; j++)
            {
                Assert.AreEqual(0, notes[j].CompareTo(originalNotes[j]));
            }


            // Get the notes for the other cab. Should return none.
            notes = m_Index.GetCabNotes(product1, file1, event1, cab2);

            Assert.AreEqual(0, notes.Count);

        }

        /// <summary>
        /// GetCabCount 0 cabs.
        /// </summary>
        [TestMethod]
        public void GetCabCount0()
        {
            addCabs(0, true, false);
        }

        /// <summary>
        /// GetCabCount 1 cabs.
        /// </summary>
        [TestMethod]
        public void GetCabCount1()
        {
            addCabs(1, true, false);
        }

        /// <summary>
        /// GetCabCount 2 cabs.
        /// </summary>
        [TestMethod]
        public void GetCabCount2()
        {
            addCabs(2, true, true);
        }

        /// <summary>
        /// GetCabCount 100 cabs.
        /// </summary>
        [TestMethod]
        public void GetCabCount100()
        {
            addCabs(100, true, false);
        }


        /// <summary>
        /// Add and then Delete cab notes.
        /// </summary>
        public void addDeleteCabNotes(int numNotes)
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

            StackHashCabCollection cabs = new StackHashCabCollection();

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212",
                "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            cabs.Add(cab);
            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(1, cabsRetrieved.Count);

            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab));


            StackHashNotes originalNotes = new StackHashNotes();

            for (int j = 0; j < numNotes; j++)
            {
                StackHashNoteEntry note = new StackHashNoteEntry(date.AddDays(j), "Source" + j.ToString(), "User" + j.ToString(), "This is a note for the cab file to see if it is stored ok" + j.ToString());
                int cabNoteId = m_Index.AddCabNote(product1, file1, event1, cab, note);

                Assert.AreEqual(j + 1, cabNoteId);

                originalNotes.Add(note);
            }

            // Get the cab notes.
            StackHashNotes notes = m_Index.GetCabNotes(product1, file1, event1, cab);

            Assert.AreEqual(numNotes, notes.Count);

            for (int j = 0; j < numNotes; j++)
            {
                Assert.AreEqual(0, notes[j].CompareTo(originalNotes[j]));


                // Get the individual cab note by index (should be 1,2,3,4...)
                StackHashNoteEntry thisNote = m_Index.GetCabNote(j + 1);

                Assert.AreEqual(0, originalNotes[j].CompareTo(thisNote));
            }


            // Delete the cab notes.
            int expectedNotes = originalNotes.Count;
            foreach (StackHashNoteEntry eventNote in originalNotes)
            {
                Assert.AreNotEqual(null, m_Index.GetCabNote(eventNote.NoteId));
                m_Index.DeleteCabNote(product1, file1, event1, cab, eventNote.NoteId);
                expectedNotes--;
                Assert.AreEqual(null, m_Index.GetCabNote(eventNote.NoteId));
                StackHashNotes cabNotes = m_Index.GetCabNotes(product1, file1, event1, cab);
                Assert.AreEqual(expectedNotes, cabNotes.Count);
            }

        }

        /// <summary>
        /// Add and Get 1 cab notes
        /// </summary>
        [TestMethod]
        public void AddDelete1CabNotes()
        {
            addCabNotes(1);
        }

        /// <summary>
        /// Add and Get 2 cab notes
        /// </summary>
        [TestMethod]
        public void AddDelete2CabNotes()
        {
            addCabNotes(2);
        }

        /// <summary>
        /// Add and Get 10 cab notes
        /// </summary>
        [TestMethod]
        public void AddDelete10CabNotes()
        {
            addCabNotes(10);
        }
        
        /// <summary>
        /// GetCabCount - 1 cab in each of 2 events.
        /// </summary>
        [TestMethod]
        public void GetCabCountOneCabInEachEvent()
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

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime, "EventTypeName2", 101, eventSignature, 10, file1.Id, "bug2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event2.Id, event2.EventTypeName, "cab123452_23232.cab", i + 1001, i * 20001);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mi2ns", "2.120.222.11221212", "Microsoft Win2dows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddCab(product1, file1, event2, cab2, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);
            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab));

            int cabCount = m_Index.GetCabCount(product1, file1, event1);
            Assert.AreEqual(1, cabCount);


            cabsRetrieved = m_Index.LoadCabList(product1, file1, event2);
            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab2));

            cabCount = m_Index.GetCabCount(product1, file1, event2);
            Assert.AreEqual(1, cabCount);
        }


        /// <summary>
        /// GetCabCount - 1 cab in 1 event and 0 in the other.
        /// </summary>
        [TestMethod]
        public void GetCabCountOneCabInOneEventOnly()
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

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime, "EventTypeName2", 101, eventSignature, 10, file1.Id, "bug2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

//            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event2.Id, event2.EventTypeName, "cab123452_23232.cab", i + 1001, i * 20001);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mi2ns", "2.120.222.11221212", "Microsoft Win2dows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddCab(product1, file1, event2, cab2, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);
            Assert.AreEqual(0, cabsRetrieved.Count);

            int cabCount = m_Index.GetCabCount(product1, file1, event1);
            Assert.AreEqual(0, cabCount);


            cabsRetrieved = m_Index.LoadCabList(product1, file1, event2);
            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab2));

            cabCount = m_Index.GetCabCount(product1, file1, event2);
            Assert.AreEqual(1, cabCount);
        }


        /// <summary>
        /// GetCabFileCount 
        /// </summary>
        public void getCabFileCount(int numCabs, int numToSetDownloaded)
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

            StackHashCabCollection cabs = new StackHashCabCollection();

            int numSetDownloaded = 0;

            for (int i = 0; i < numCabs; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
                cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

                cab.CabDownloaded = (numSetDownloaded < numToSetDownloaded);
                numSetDownloaded++;
                cabs.Add(cab);
                m_Index.AddCab(product1, file1, event1, cab, true);
            }

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);

            Assert.AreEqual(numCabs, cabsRetrieved.Count);

            for (int i = 0; i < numCabs; i++)
            {
                Assert.AreEqual(0, cabsRetrieved[i].CompareTo(cabs[i]));
                Assert.AreEqual(0, cabsRetrieved[i].CompareDiagnostics(cabs[i]));
            }

            int retrievedNumCabs = m_Index.GetCabCount(product1, file1, event1);
            Assert.AreEqual(numCabs, retrievedNumCabs);

            int retrievedNumFileCabs = m_Index.GetCabFileCount(product1, file1, event1);
            Assert.AreEqual(numToSetDownloaded, retrievedNumFileCabs);
        }

        /// <summary>
        /// GetCabFileCount 0 cabs - 0 marked downloaded
        /// </summary>
        [TestMethod]
        public void GetCabFileCount0_0()
        {
            getCabFileCount(0, 0);
        }

        /// <summary>
        /// GetCabFileCount 1 cabs - 0 marked downloaded
        /// </summary>
        [TestMethod]
        public void GetCabFileCount1_0()
        {
            getCabFileCount(1, 0);
        }

        /// <summary>
        /// GetCabFileCount 1 cabs - 1 marked downloaded
        /// </summary>
        [TestMethod]
        public void GetCabFileCount1_1()
        {
            getCabFileCount(1, 1);
        }

        /// <summary>
        /// GetCabFileCount 2 cabs - 1 marked downloaded
        /// </summary>
        [TestMethod]
        public void GetCabFileCount2_1()
        {
            getCabFileCount(2, 1);
        }

        /// <summary>
        /// GetCabFileCount 20 cabs - 5 marked downloaded
        /// </summary>
        [TestMethod]
        public void GetCabFileCount20_5()
        {
            getCabFileCount(20, 5);
        }


        /// <summary>
        /// Get Cab folder.
        /// </summary>
        [TestMethod]
        public void GetCabFolder()
        {
            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);
            StackHashCab cab = new StackHashCab(date, date, 1, "EventType", "1756169919-Crash32bit-0911628304.cab", 911628304, 2000);

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);

            String cabFolder = m_Index.GetCabFolder(null, null, null, cab);

            Assert.AreEqual(m_RootCabFolder +  "\\" + SqlUtils.UnitTestDatabase + "\\09\\11\\62\\83\\CAB_0911628304", cabFolder);
        }

        /// <summary>
        /// Get Cab filename.
        /// </summary>
        [TestMethod]
        public void GetCabFileName()
        {
            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);
            StackHashCab cab = new StackHashCab(date, date, 1, "EventType", "1756169919-Crash32bit-0911628304.cab", 911628304, 2000);

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);

            String cabFileName = m_Index.GetCabFileName(null, null, null, cab);

            Assert.AreEqual(m_RootCabFolder +  "\\" + SqlUtils.UnitTestDatabase + "\\09\\11\\62\\83\\CAB_0911628304\\1756169919-Crash32bit-0911628304.cab", cabFileName);
        }

        /// <summary>
        /// Cab Exists - No
        /// </summary>
        [TestMethod]
        public void CabExistsNo()
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

            int i = 1;
            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, "EventType", "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            Assert.AreEqual(false, m_Index.CabExists(product1, file1, event1, cab));

        }


        /// <summary>
        /// Cab Exists - Yes
        /// </summary>
        [TestMethod]
        public void CabExistsYes()
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

            int i = 1;
            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab, true);

            Assert.AreEqual(true, m_Index.CabExists(product1, file1, event1, cab));

        }


        /// <summary>
        /// Cab File Exists - No
        /// </summary>
        [TestMethod]
        public void CabFileExistsNo()
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

            int i = 1;
            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab, true);

            Assert.AreEqual(false, m_Index.CabFileExists(product1, file1, event1, cab));

        }

        /// <summary>
        /// Cab File Exists - Yes
        /// </summary>
        [TestMethod]
        public void CabFileExistsYes()
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

            int i = 1;
            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab, true);

            // Pretend the cab file exists
            if (!Directory.Exists(m_Index.GetCabFolder(product1, file1, event1, cab)))
                Directory.CreateDirectory(m_Index.GetCabFolder(product1, file1, event1, cab));

            File.Create(m_Index.GetCabFileName(product1, file1, event1, cab)).Close();

            Assert.AreEqual(true, m_Index.CabFileExists(product1, file1, event1, cab));
        }

        /// <summary>
        /// GetCabCount - Same Event ID used for different EventType.
        /// </summary>
        [TestMethod]
        public void GetCabsAssociatedWithEventsWithSameId()
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

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(1), modifiedDateTime, "EventTypeName2", 100, eventSignature, 10, file1.Id, "bug2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            int i = 1;

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            StackHashCab cab = new StackHashCab(date.AddDays(i * 1), date.AddDays(i * 2), event1.Id, event1.EventTypeName, "cab12345_23232.cab", i + 1000, i * 2000);
            cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");
            cab.CabDownloaded = false;
            m_Index.AddCab(product1, file1, event1, cab, true);

            StackHashCab cab2 = new StackHashCab(date.AddDays(2), date.AddDays(3), event2.Id, event2.EventTypeName, "cab123452_23232.cab", i + 1001, i * 20001);
            cab2.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins2", "1 hour, 2 mi2ns", "2.120.222.11221212", "Microsoft Win2dows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");
            cab2.CabDownloaded = true;

            m_Index.AddCab(product1, file1, event2, cab2, true);

            StackHashCabCollection cabsRetrieved = m_Index.LoadCabList(product1, file1, event1);
            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab));

            int cabCount = m_Index.GetCabCount(product1, file1, event1);
            Assert.AreEqual(1, cabCount);

            int cabFileCount = m_Index.GetCabFileCount(product1, file1, event1);
            Assert.AreEqual(0, cabFileCount);

            cabsRetrieved = m_Index.LoadCabList(product1, file1, event2);
            Assert.AreEqual(1, cabsRetrieved.Count);
            Assert.AreEqual(0, cabsRetrieved[0].CompareTo(cab2));
            Assert.AreEqual(0, cabsRetrieved[0].CompareDiagnostics(cab2));

            cabCount = m_Index.GetCabCount(product1, file1, event2);
            Assert.AreEqual(1, cabCount);

            cabFileCount = m_Index.GetCabFileCount(product1, file1, event2);
            Assert.AreEqual(1, cabFileCount);
        }


    }
}
