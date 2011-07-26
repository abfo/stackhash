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
    /// Summary description for SqlStatisticsUnitTests
    /// </summary>
    [TestClass]
    public class SqlStatisticsUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

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

        public SqlStatisticsUnitTests()
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

        /// <summary>
        /// Gets the rollup of locales.
        /// </summary>
        public void getLocaleRollup(int numEventInfos, int localeIdModulo)
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
            String localeCodeBase = "US";
            String languageNameBase = "ENGLISH";
            for (int i = 1; i <= numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int localeId = (i % localeIdModulo) + 1;

                totalHits += hitCount;
                StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i * 1), date.AddDays(i * 2), date.AddDays(i * 3), languageNameBase + localeId.ToString(), localeId, localeCodeBase + localeId.ToString(), "Windows Vista" + i.ToString(), "1.2.3.4 build 7", localeId);

                eventInfos.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);

            StackHashProductLocaleSummaryCollection localeSummary = m_Index.GetLocaleSummary(product1.Id);
            StackHashProductLocaleSummaryCollection localeSummaryFresh = m_Index.GetLocaleSummaryFresh(product1.Id);
            Assert.AreEqual(0, localeSummary.CompareTo(localeSummaryFresh));


            Dictionary<int , StackHashProductLocaleSummary> localeCheckList = new Dictionary<int, StackHashProductLocaleSummary>();

            if (localeIdModulo < numEventInfos)
                Assert.AreEqual(localeIdModulo, localeSummary.Count);
            else
                Assert.AreEqual(numEventInfos, localeSummary.Count);

            foreach (StackHashProductLocaleSummary localeData in localeSummary)
            {
                if (localeIdModulo >= numEventInfos)
                    Assert.AreEqual(localeData.Lcid, localeData.TotalHits);
                else
                    Assert.AreEqual(localeData.Lcid * ((numEventInfos / localeIdModulo)), localeData.TotalHits);

                Assert.AreEqual(localeCodeBase + localeData.Lcid.ToString(), localeData.Locale);
                Assert.AreEqual(languageNameBase + localeData.Lcid.ToString(), localeData.Language);
                Assert.AreEqual(false, localeCheckList.ContainsKey(localeData.Lcid));
                localeCheckList.Add(localeData.Lcid, localeData);
            }

            m_Index.UpdateProductStatistics(product1.Id);
            StackHashProductLocaleSummaryCollection localeSummary2 = m_Index.GetLocaleSummary(product1.Id);
            Assert.AreEqual(0, localeSummary.CompareTo(localeSummary2));
        }


        /// <summary>
        /// Add and Get 0 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup0EventInfo()
        {
            getLocaleRollup(0, 10);
        }

        /// <summary>
        /// Add and Get 1 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup1EventInfo()
        {
            getLocaleRollup(1, 10);
        }

        /// <summary>
        /// Add and Get 2 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup2EventInfo()
        {
            getLocaleRollup(2, 10);
        }

        /// <summary>
        /// Add and Get 100 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup100EventInfo()
        {
            getLocaleRollup(100, 1000);
        }

        /// <summary>
        /// Add and Get 1000 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup1000EventInfo()
        {
            getLocaleRollup(1000, 10000);
        }

        /// <summary>
        /// Add and Get 2 event infos - repeated twice
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup2EventInfoRep2()
        {
            getLocaleRollup(2, 1);
        }

        /// <summary>
        /// Add and Get 9 event infos - repeated * 3.
        /// </summary>
        [TestMethod]
        public void GetLocaleRollup9EventInfoRep3()
        {
            getLocaleRollup(9, 3);
        }


        /// <summary>
        /// Gets the rollup of operating systems.
        /// </summary>
        public void getOperatingSystemRollup(int numEventInfos, int osIdModulo)
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
            String localeCodeBase = "US";
            String languageNameBase = "ENGLISH";
            String osBase = "Vista";
            for (int i = 1; i <= numEventInfos; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

                int osId = (i % osIdModulo) + 1;
                int localeId = i;

                totalHits += hitCount;
                StackHashEventInfo eventInfo = new StackHashEventInfo(date.AddDays(i * 1), date.AddDays(i * 2), date.AddDays(i * 3), languageNameBase + localeId.ToString(), localeId, localeCodeBase + localeId.ToString(), osBase + osId.ToString(), osId.ToString(), osId);

                eventInfos.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);

            StackHashProductOperatingSystemSummaryCollection osSummary = m_Index.GetOperatingSystemSummary(product1.Id);
            StackHashProductOperatingSystemSummaryCollection osSummaryFresh = m_Index.GetOperatingSystemSummaryFresh(product1.Id);
            Assert.AreEqual(0, osSummary.CompareTo(osSummaryFresh));

            Dictionary<int, StackHashProductOperatingSystemSummary> osCheckList = new Dictionary<int, StackHashProductOperatingSystemSummary>();

            if (osIdModulo < numEventInfos)
                Assert.AreEqual(osIdModulo, osSummary.Count);
            else
                Assert.AreEqual(numEventInfos, osSummary.Count);
            
            foreach (StackHashProductOperatingSystemSummary osData in osSummary)
            {
                int osId = int.Parse(osData.OperatingSystemVersion);

                if (osIdModulo >= numEventInfos)
                    Assert.AreEqual(osId, osData.TotalHits);
                else
                    Assert.AreEqual(osId * ((numEventInfos / osIdModulo)), osData.TotalHits);

                Assert.AreEqual(osBase + osId.ToString(), osData.OperatingSystemName);
                Assert.AreEqual(false, osCheckList.ContainsKey(osId));
                osCheckList.Add(osId, osData);
            }

            // Update the stats fresh.
            m_Index.UpdateProductStatistics(product1.Id);
            StackHashProductOperatingSystemSummaryCollection osSummary2 = m_Index.GetOperatingSystemSummary(product1.Id);
            Assert.AreEqual(0, osSummary.CompareTo(osSummary2));
        }


        /// <summary>
        /// Add and Get 0 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup0EventInfo()
        {
            getOperatingSystemRollup(0, 10);
        }

        /// <summary>
        /// Add and Get 1 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup1EventInfo()
        {
            getOperatingSystemRollup(1, 10);
        }

        /// <summary>
        /// Add and Get 2 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup2EventInfo()
        {
            getOperatingSystemRollup(2, 10);
        }

        /// <summary>
        /// Add and Get 100 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup100EventInfo()
        {
            getOperatingSystemRollup(100, 1000);
        }

        /// <summary>
        /// Add and Get 1000 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup1000EventInfo()
        {
            getOperatingSystemRollup(1000, 10000);
        }

        /// <summary>
        /// Add and Get 2 event infos - repeated twice
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup2EventInfoRep2()
        {
            getOperatingSystemRollup(2, 1);
        }

        /// <summary>
        /// Add and Get 9 event infos - repeated * 3.
        /// </summary>
        [TestMethod]
        public void GetOperatingSystemRollup9EventInfoRep3()
        {
            getOperatingSystemRollup(9, 3);
        }


        /// <summary>
        /// Gets the rollup of hit dates.
        /// </summary>
        public void getHitDateRollup(int numEventInfos, int hitDateModulo)
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
            String localeCodeBase = "US";
            String languageNameBase = "ENGLISH";
            String osBase = "Vista";

            DateTime nowTime = DateTime.Now;
            DateTime date = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, 0);

            for (int i = 0; i < numEventInfos; i++)
            {
                DateTime hitDate = date.AddDays(i % hitDateModulo);
                totalHits += hitCount;
                int localeId = i;
                int osId = i;
                int hits = (i % hitDateModulo);

                StackHashEventInfo eventInfo = new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDate, languageNameBase + localeId.ToString(), localeId, localeCodeBase + localeId.ToString(), osBase + osId.ToString(), osId.ToString(), hits);

                eventInfos.Add(eventInfo);
            }

            m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos);

            StackHashProductHitDateSummaryCollection hitDateSummary = m_Index.GetHitDateSummary(product1.Id);
            StackHashProductHitDateSummaryCollection hitDateSummaryFresh = m_Index.GetHitDateSummaryFresh(product1.Id);
            Assert.AreEqual(0, hitDateSummary.CompareTo(hitDateSummaryFresh));

            Dictionary<DateTime, StackHashProductHitDateSummary> hitDateCheckList = new Dictionary<DateTime, StackHashProductHitDateSummary>();

            if (hitDateModulo < numEventInfos)
                Assert.AreEqual(hitDateModulo, hitDateSummary.Count);
            else
                Assert.AreEqual(numEventInfos, hitDateSummary.Count);

            int dateCount = 0;
            foreach (StackHashProductHitDateSummary hitData in hitDateSummary)
            {
                int expectedDateAdjust = dateCount % hitDateModulo;

                if (hitDateModulo >= numEventInfos)
                    Assert.AreEqual(dateCount, hitData.TotalHits);
                else
                    Assert.AreEqual(expectedDateAdjust * ((numEventInfos / hitDateModulo)), hitData.TotalHits);

                Assert.AreEqual(false, hitDateCheckList.ContainsKey(hitData.HitDate));
                hitDateCheckList.Add(hitData.HitDate, hitData);
                dateCount++;
            }

            // Update the stats fresh.
            m_Index.UpdateProductStatistics(product1.Id);
            StackHashProductHitDateSummaryCollection hitDateSummary2 = m_Index.GetHitDateSummary(product1.Id);
            Assert.AreEqual(0, hitDateSummary.CompareTo(hitDateSummary2));
        }


        /// <summary>
        /// Add and Get 0 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup0EventInfo()
        {
            getHitDateRollup(0, 10);
        }

        /// <summary>
        /// Add and Get 1 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup1EventInfo()
        {
            getHitDateRollup(1, 10);
        }

        /// <summary>
        /// Add and Get 2 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup2EventInfo()
        {
            getHitDateRollup(2, 10);
        }

        /// <summary>
        /// Add and Get 100 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup100EventInfo()
        {
            getHitDateRollup(100, 1000);
        }

        /// <summary>
        /// Add and Get 1000 event infos - all individual event infos.
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup1000EventInfo()
        {
            getHitDateRollup(1000, 10000);
        }

        /// <summary>
        /// Add and Get 2 event infos - repeated twice
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup2EventInfoRep2()
        {
            getHitDateRollup(2, 1);
        }

        /// <summary>
        /// Add and Get 9 event infos - repeated * 3.
        /// </summary>
        [TestMethod]
        public void GetHitDateRollup9EventInfoRep3()
        {
            getHitDateRollup(9, 3);
        }
    }
}
