using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for EventInfosUnitTests
    /// </summary>
    [TestClass]
    public class EventInfosUnitTests
    {
        public EventInfosUnitTests()
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
        /// The US server will take the 25-Oct-2010 00:00:00 Local and convert to 25-Oct-2010 07:00:00 UTC (plus 7 hours – date stays the same).
        /// </summary>
        [TestMethod]
        public void NormalizeUSUtc()
        {
            DateTime testTime = new DateTime(2010, 10, 25, 7, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo = new StackHashEventInfo(testTime, testTime, testTime, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfo normalizedEventInfo = eventInfo.Normalize();

            DateTime expectedTime = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            eventInfo.HitDateLocal = expectedTime;


            Assert.AreEqual(0, eventInfo.CompareTo(normalizedEventInfo));

        }

        /// <summary>
        /// The UK server will take the 25-Oct-2010 00:00:00 Local and convert to 24-Oct-2010 23:00:00 UTC (minus 1 hour – causes date change).
        /// </summary>
        [TestMethod]
        public void NormalizeUKUtc()
        {
            DateTime testTime = new DateTime(2010, 10, 24, 23, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo = new StackHashEventInfo(testTime, testTime, testTime, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfo normalizedEventInfo = eventInfo.Normalize();

            DateTime expectedTime = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            eventInfo.HitDateLocal = expectedTime;


            Assert.AreEqual(0, eventInfo.CompareTo(normalizedEventInfo));

        }

        /// <summary>
        /// The Australian server will take the 25-Oct-2010 00:00:00 Local and convert to 24-Oct-2010 15:00:00 UTC (minus 9 hours – causes date change).
        /// </summary>
        [TestMethod]
        public void NormalizeAustraliaUtc()
        {
            DateTime testTime = new DateTime(2010, 10, 24, 15, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo = new StackHashEventInfo(testTime, testTime, testTime, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfo normalizedEventInfo = eventInfo.Normalize();

            DateTime expectedTime = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            eventInfo.HitDateLocal = expectedTime;


            Assert.AreEqual(0, eventInfo.CompareTo(normalizedEventInfo));
        }

        /// <summary>
        /// The Asia server will take the 25-Oct-2010 00:00:00 Local and convert to 24-Oct-2010 16:00:00 UTC (minus 8 hours – causes date change).
        /// </summary>
        [TestMethod]
        public void NormalizeAsiaUtc()
        {
            DateTime testTime = new DateTime(2010, 10, 24, 16, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo = new StackHashEventInfo(testTime, testTime, testTime, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfo normalizedEventInfo = eventInfo.Normalize();

            DateTime expectedTime = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            eventInfo.HitDateLocal = expectedTime;


            Assert.AreEqual(0, eventInfo.CompareTo(normalizedEventInfo));
        }

        /// <summary>
        /// Time = 00:00:00 - should not change. This will be the case when a fix becomes available.
        /// </summary>
        [TestMethod]
        public void NormalizeNoTimeUtc()
        {
            DateTime testTime = new DateTime(2010, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo = new StackHashEventInfo(testTime, testTime, testTime, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfo normalizedEventInfo = eventInfo.Normalize();

            DateTime expectedTime = new DateTime(2010, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            eventInfo.HitDateLocal = expectedTime;


            Assert.AreEqual(0, eventInfo.CompareTo(normalizedEventInfo));
        }


        /// <summary>
        /// EventInfoList.Normalize - no duplicates.
        /// </summary>
        [TestMethod]
        public void NormalizeListNoDuplicates()
        {
            DateTime testTime1 = new DateTime(2010, 10, 24, 16, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime1 = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo1 = new StackHashEventInfo(testTime1, testTime1, testTime1, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            DateTime testTime2 = new DateTime(2010, 10, 25, 16, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime2 = new DateTime(2010, 10, 26, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo2 = new StackHashEventInfo(testTime2, testTime2, testTime2, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfoCollection eventInfos = new StackHashEventInfoCollection();
            eventInfos.Add(eventInfo1);
            eventInfos.Add(eventInfo2);

            StackHashEventInfoCollection normalizedEventInfos = eventInfos.Normalize();

            eventInfo1.HitDateLocal = expectedTime1;
            eventInfo2.HitDateLocal = expectedTime2;

            Assert.AreEqual(0, eventInfos.CompareTo(normalizedEventInfos));
        }


        /// <summary>
        /// EventInfoList.Normalize - duplicates same time zone.
        /// </summary>
        [TestMethod]
        public void NormalizeListDuplicatesSameTimeZone()
        {
            DateTime testTime1 = new DateTime(2010, 10, 24, 16, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime1 = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo1 = new StackHashEventInfo(testTime1, testTime1, testTime1, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            DateTime testTime2 = new DateTime(2010, 10, 24, 16, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime2 = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo2 = new StackHashEventInfo(testTime2, testTime2, testTime2, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            StackHashEventInfoCollection eventInfos = new StackHashEventInfoCollection();
            eventInfos.Add(eventInfo1);
            eventInfos.Add(eventInfo2);

            StackHashEventInfoCollection normalizedEventInfos = eventInfos.Normalize();

            Assert.AreEqual(1, normalizedEventInfos.Count);
            eventInfo1.HitDateLocal = expectedTime1;
            eventInfo1.TotalHits += eventInfo2.TotalHits;

            Assert.AreEqual(0, eventInfos[0].CompareTo(normalizedEventInfos[0]));
        }

        /// <summary>
        /// EventInfoList.Normalize - duplicates different time zone.
        /// </summary>
        [TestMethod]
        public void NormalizeListDuplicatesSameDifferentTimeZone()
        {
            DateTime testTime1 = new DateTime(2010, 10, 24, 16, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime1 = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo1 = new StackHashEventInfo(testTime1, testTime1, testTime1, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            DateTime testTime2 = new DateTime(2010, 10, 24, 15, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime2 = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo2 = new StackHashEventInfo(testTime2, testTime2, testTime2, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            DateTime testTime3 = new DateTime(2010, 10, 24, 23, 0, 0, DateTimeKind.Utc);
            DateTime expectedTime3 = new DateTime(2010, 10, 25, 0, 0, 0, DateTimeKind.Utc);
            StackHashEventInfo eventInfo3 = new StackHashEventInfo(testTime3, testTime3, testTime3, "English", 123, "EN-US", "Vista", "6.0.0.0", 10);

            
            StackHashEventInfoCollection eventInfos = new StackHashEventInfoCollection();
            eventInfos.Add(eventInfo1);
            eventInfos.Add(eventInfo2);
            eventInfos.Add(eventInfo3);

            StackHashEventInfoCollection normalizedEventInfos = eventInfos.Normalize();

            Assert.AreEqual(1, normalizedEventInfos.Count);
            eventInfo1.HitDateLocal = expectedTime1;
            eventInfo1.TotalHits += eventInfo2.TotalHits + eventInfo3.TotalHits;

            Assert.AreEqual(0, eventInfos[0].CompareTo(normalizedEventInfos[0]));
        }

    }
}
