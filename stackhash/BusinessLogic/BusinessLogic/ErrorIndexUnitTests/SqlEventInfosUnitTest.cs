using System;
using System.Text;
using System.Collections.Generic;
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
    /// Summary description for SqlEventInfosUnitTest
    /// </summary>
    [TestClass]
    public class SqlEventInfosUnitTest
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlEventInfosUnitTest()
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
        public void addGetNEventInfos(int numEventInfos, bool useSameLocale)
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
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfos.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                Assert.AreEqual(0, eventInfos[i].CompareTo(eventInfosRetrieved[i]));
            }

            int totalHitsRetrieved = m_Index.GetHitCount(product1, file1, event1);
            Assert.AreEqual(totalHits, totalHitsRetrieved);
        }


        /// <summary>
        /// Add and Get 0 event infos.
        /// </summary>
        [TestMethod]
        public void AddGetNoEventInfo()
        {
            addGetNEventInfos(0, false);
        }

        /// <summary>
        /// Add and Get 1 event infos.
        /// </summary>
        [TestMethod]
        public void AddGetOneEventInfo()
        {
            addGetNEventInfos(1, false);
        }

        /// <summary>
        /// Add and Get 2 event infos.
        /// </summary>
        [TestMethod]
        public void AddGet2EventInfo()
        {
            addGetNEventInfos(2, false);
        }

        /// <summary>
        /// Add and Get 2 event infos.
        /// </summary>
        [TestMethod]
        public void AddGet10EventInfo()
        {
            addGetNEventInfos(10, false);
        }

        /// <summary>
        /// Add and Get 100 event infos.
        /// </summary>
        [TestMethod]
        public void AddGet100EventInfo()
        {
            addGetNEventInfos(100, false);
        }

        /// <summary>
        /// Add and Get 1 event infos - same locale.
        /// </summary>
        [TestMethod]
        public void AddGetOneEventInfoSameLocale()
        {
            addGetNEventInfos(1, true);
        }

        public void addSameEventInfoTwice(int numEventInfos, bool useSameLocale)
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

            StackHashEventInfoCollection eventInfos = new StackHashEventInfoCollection();

            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = 10;
                if (!useSameLocale)
                    localeId += i;

                StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i * 1), date.AddDays(i * 2), date.AddDays(i * 3), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", 100);

                eventInfos.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfosRetrieved.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                Assert.AreEqual(0, eventInfos[i].CompareTo(eventInfosRetrieved[i]));
            }
        }

        /// <summary>
        /// Add and Get 1 event infos - same locale - add them twice - should merge.
        /// </summary>
        [TestMethod]
        public void AddSameEventInfoTwice()
        {
            addSameEventInfoTwice(1, true);
        }

        /// <summary>
        /// Add and Get 2 event infos - same locale add them twice - should merge
        /// </summary>
        [TestMethod]
        public void AddSame2EventInfoTwice()
        {
            addSameEventInfoTwice(2, true);
        }


        /// <summary>
        /// Merge 2 event info lists of size 2. One in common.
        /// </summary>
        [TestMethod]
        public void MergeOneInCommon()
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

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();
            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection();



            for (int i = 0; i < 3; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;

                StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i * 1), date.AddDays(i * 2), date.AddDays(i * 3), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", 100);

                if (i < 2)
                    eventInfos1.Add(eventInfo);
                if (i > 0)
                    eventInfos2.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);
            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos2);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(3, eventInfosRetrieved.Count);

            int realListIndex = 0;
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(0, eventInfos1[i].CompareTo(eventInfosRetrieved[realListIndex]));
                realListIndex++;
            }
            for (int i = 1; i < 3; i++)
            {
                Assert.AreEqual(0, eventInfos2[i - 1].CompareTo(eventInfosRetrieved[realListIndex - 1]));
                realListIndex++;
            }
        }


        /// <summary>
        /// MostRecentHitDate - should return latest date.
        /// Latest date is the last EventInfo added.
        /// </summary>
        [TestMethod]
        public void GetMostRecentHitDateLastEntry()
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

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();


            int numEventInfos = 3;
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;

                StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(i), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", 100);

                eventInfos1.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfosRetrieved.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                StackHashEventInfo eventInfo = eventInfosRetrieved.FindEventInfo(eventInfos1[i]);

                Assert.AreEqual(0, eventInfos1[i].CompareTo(eventInfo));
            }

            DateTime mostRecentHitDate = m_Index.GetMostRecentHitDate(product1, file1, event1);

            Assert.AreEqual(eventInfos1[numEventInfos - 1].HitDateLocal, mostRecentHitDate);
            Assert.AreNotEqual(eventInfos1[0].HitDateLocal, mostRecentHitDate);

        }


        /// <summary>
        /// MostRecentHitDate - should return latest date.
        /// Latest date is the first EventInfo added.
        /// </summary>
        [TestMethod]
        public void GetMostRecentHitDateFirstEntry()
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

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();


            int numEventInfos = 3;
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;

                StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(-1 * i), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", 100);

                eventInfos1.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfosRetrieved.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                StackHashEventInfo eventInfo = eventInfosRetrieved.FindEventInfo(eventInfos1[i]);

                Assert.AreEqual(0, eventInfos1[i].CompareTo(eventInfo));
            }

            DateTime mostRecentHitDate = m_Index.GetMostRecentHitDate(product1, file1, event1);

            Assert.AreNotEqual(eventInfos1[numEventInfos - 1].HitDateLocal, mostRecentHitDate);
            Assert.AreEqual(eventInfos1[0].HitDateLocal, mostRecentHitDate);

        }

        /// <summary>
        /// MostRecentHitDate - should return latest date.
        /// Latest date is the second EventInfo added.
        /// </summary>
        [TestMethod]
        public void GetMostRecentHitDateSecondEntry()
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

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();


            int numEventInfos = 3;
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;



                StackHashEventInfo eventInfo = 
                    new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(i == 1 ? 0 : -1 * (i + 1)), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", 100);

                eventInfos1.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfosRetrieved.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                StackHashEventInfo eventInfo = eventInfosRetrieved.FindEventInfo(eventInfos1[i]);

                Assert.AreEqual(0, eventInfos1[i].CompareTo(eventInfo));
            }

            DateTime mostRecentHitDate = m_Index.GetMostRecentHitDate(product1, file1, event1);

            Assert.AreNotEqual(eventInfos1[numEventInfos -1].HitDateLocal, mostRecentHitDate);
            Assert.AreNotEqual(eventInfos1[0].HitDateLocal, mostRecentHitDate);
            Assert.AreEqual(eventInfos1[1].HitDateLocal, mostRecentHitDate);

        }


        /// <summary>
        /// MostRecentHitDate - should return latest date.
        /// Latest date is the last EventInfo added.
        /// Other EventInfos present for other EventIds with later dates.
        /// </summary>
        [TestMethod]
        public void GetMostRecentHitDateLastEntryOtherEventsExist()
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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 101, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();

            int numEventInfos = 3;
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;
                StackHashEventInfo eventInfo =
                    new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(i), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", event1.Id);

                eventInfos1.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);


            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection();

            // Add some other event infos for a different event.
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;
                StackHashEventInfo eventInfo =
                    new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(i * 2), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", event2.Id);

                eventInfos2.Add(eventInfo);
            }

            
            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfosRetrieved.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                StackHashEventInfo eventInfo = eventInfosRetrieved.FindEventInfo(eventInfos1[i]);

                Assert.AreEqual(0, eventInfos1[i].CompareTo(eventInfo));
            }

            DateTime mostRecentHitDate = m_Index.GetMostRecentHitDate(product1, file1, event1);

            Assert.AreEqual(eventInfos1[numEventInfos - 1].HitDateLocal, mostRecentHitDate);
            Assert.AreNotEqual(eventInfos1[0].HitDateLocal, mostRecentHitDate);
            Assert.AreNotEqual(eventInfos1[1].HitDateLocal, mostRecentHitDate);

            DateTime mostRecentHitDate2 = m_Index.GetMostRecentHitDate(product1, file1, event2);

            Assert.AreEqual(eventInfos2[numEventInfos - 1].HitDateLocal, mostRecentHitDate2);
            Assert.AreNotEqual(eventInfos2[0].HitDateLocal, mostRecentHitDate2);
            Assert.AreNotEqual(eventInfos2[1].HitDateLocal, mostRecentHitDate2);
        }


        /// <summary>
        /// MostRecentHitDate - should return latest date.
        /// Latest date is the last EventInfo added.
        /// Other EventInfos present for same EventId - different EventTypeName with later dates.
        /// </summary>
        [TestMethod]
        public void GetMostRecentHitDateDuplicateEventIds()
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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();

            int numEventInfos = 3;
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;
                StackHashEventInfo eventInfo =
                    new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(i), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", event1.Id);

                eventInfos1.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);


            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection();

            // Add some other event infos for a different event.
            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = i;
                StackHashEventInfo eventInfo =
                    new StackHashEventInfo(date.AddDays(i + 2), date.AddDays(i + 1), date.AddDays(i * 2), "US", localeId, "English", "Windows Vista" + i.ToString(), "1.2.3.4 build 7", event2.Id);

                eventInfos2.Add(eventInfo);
            }


            m_Index.AddEventInfoCollection(product1, file1, event2, eventInfos2);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(numEventInfos, eventInfosRetrieved.Count);

            for (int i = 0; i < numEventInfos; i++)
            {
                StackHashEventInfo eventInfo = eventInfosRetrieved.FindEventInfo(eventInfos1[i]);

                Assert.AreEqual(0, eventInfos1[i].CompareTo(eventInfo));
            }

            DateTime mostRecentHitDate = m_Index.GetMostRecentHitDate(product1, file1, event1);

            Assert.AreEqual(eventInfos1[numEventInfos - 1].HitDateLocal, mostRecentHitDate);
            Assert.AreNotEqual(eventInfos1[0].HitDateLocal, mostRecentHitDate);
            Assert.AreNotEqual(eventInfos1[1].HitDateLocal, mostRecentHitDate);

            DateTime mostRecentHitDate2 = m_Index.GetMostRecentHitDate(product1, file1, event2);

            Assert.AreEqual(eventInfos2[numEventInfos - 1].HitDateLocal, mostRecentHitDate2);
            Assert.AreNotEqual(eventInfos2[0].HitDateLocal, mostRecentHitDate2);
            Assert.AreNotEqual(eventInfos2[1].HitDateLocal, mostRecentHitDate2);
        }

        /// <summary>
        /// Merge with duplicates in new list - 836
        /// </summary>
        [TestMethod]
        public void MergeWithDuplicatesInNewList()
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

            StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection();
            StackHashEventInfoCollection eventInfos2 = new StackHashEventInfoCollection();



            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            int localeId = 123;

            // These events just different by num hits.
            StackHashEventInfo eventInfo1 = new StackHashEventInfo(date, date, date, "US", localeId, "English", "Windows Vista", "1.2.3.4 build 7", 100);
            StackHashEventInfo eventInfo2 = new StackHashEventInfo(date, date, date, "US", localeId, "English", "Windows Vista", "1.2.3.4 build 7", 101);
            StackHashEventInfo expectedEventInfo = (StackHashEventInfo)eventInfo1.Clone();
            expectedEventInfo.TotalHits = eventInfo1.TotalHits + eventInfo2.TotalHits;

            eventInfos1.Add(eventInfo1);
            eventInfos1.Add(eventInfo2);

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

            StackHashEventInfoCollection eventInfosRetrieved = m_Index.LoadEventInfoList(product1, file1, event1);

            Assert.AreEqual(1, eventInfosRetrieved.Count);


            Assert.AreEqual(0, expectedEventInfo.CompareTo(eventInfosRetrieved[0]));        
        }

    }
}
