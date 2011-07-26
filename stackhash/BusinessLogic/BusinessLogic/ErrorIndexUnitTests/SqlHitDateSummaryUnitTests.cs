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
    /// Summary description for SqlHitDateSummaryUnitTests
    /// </summary>
    [TestClass]
    public class SqlHitDateSummaryUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\HitDateSummaryTests";

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

        public SqlHitDateSummaryUnitTests()
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
        /// Adds the rollup of hitDates.
        /// </summary>
        public void addHitDateSummaries(int numHitDateSummaries)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime hitDateBase = DateTime.Now.RoundToNextSecond();
            int productId = 1234;

            // Check a hitDate that doesn't exist.
            Assert.AreEqual(false, m_Index.HitDateSummaryExists(productId, DateTime.Now));

            StackHashProductHitDateSummaryCollection allHitDateSummaries = new StackHashProductHitDateSummaryCollection();

            for (int hitDateCount = 0; hitDateCount < numHitDateSummaries; hitDateCount++)
            {
                int totalHits = hitDateCount + 1;
                DateTime hitDate = hitDateBase.AddDays(hitDateCount);

                m_Index.AddHitDateSummary(productId, hitDate, totalHits, false);

                // Check the hitDate exists now.
                Assert.AreEqual(true, m_Index.HitDateSummaryExists(productId, hitDate));

                // Get the specific hitDate and make sure it was stored properly.
                StackHashProductHitDateSummary summary = m_Index.GetHitDateSummaryForProduct(productId, hitDate);
                Assert.AreEqual(hitDate, summary.HitDate.ToLocalTime());
                Assert.AreEqual(totalHits, summary.TotalHits);

                allHitDateSummaries.Add(summary);
            }

            StackHashProductHitDateSummaryCollection hitDateSummaryCollection = m_Index.GetHitDateSummaries(productId);
            Assert.AreEqual(numHitDateSummaries, hitDateSummaryCollection.Count);

            foreach (StackHashProductHitDateSummary hitDateData in hitDateSummaryCollection)
            {
                // Find the matching hitDate in the expected list.
                StackHashProductHitDateSummary expectedSummary = allHitDateSummaries.FindHitDate(hitDateData.HitDate);

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(hitDateData));
            }
        }


        /// <summary>
        /// Add 0 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void AddHitDateSummaries0()
        {
            addHitDateSummaries(0);
        }

        /// <summary>
        /// Add 1 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void AddHitDateSummaries1()
        {
            addHitDateSummaries(1);
        }

        /// <summary>
        /// Add 2 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void AddHitDateSummaries2()
        {
            addHitDateSummaries(2);
        }

        /// <summary>
        /// Add 100 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void AddHitDateSummaries100()
        {
            addHitDateSummaries(100);
        }


        /// <summary>
        /// Updates the rollup of hitDates.
        /// </summary>
        public void updateHitDateSummaries(int numHitDateSummaries)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime hitDateBase = DateTime.Now.RoundToNextSecond();
            int productId = 1234;

            // Check a hitDate that doesn't exist.
            Assert.AreEqual(false, m_Index.HitDateSummaryExists(productId, DateTime.Now));

            StackHashProductHitDateSummaryCollection allHitDateSummaries = new StackHashProductHitDateSummaryCollection();

            for (int hitDateCount = 0; hitDateCount < numHitDateSummaries; hitDateCount++)
            {
                int totalHits = hitDateCount + 1;
                DateTime hitDate = hitDateBase.AddDays(hitDateCount);

                m_Index.AddHitDateSummary(productId, hitDate, totalHits, false);

                // Check the hitDate exists now.
                Assert.AreEqual(true, m_Index.HitDateSummaryExists(productId, hitDate));

                // Get the specific hitDate and make sure it was stored properly.
                StackHashProductHitDateSummary summary = m_Index.GetHitDateSummaryForProduct(productId, hitDate);
                Assert.AreEqual(hitDate, summary.HitDate.ToLocalTime());
                Assert.AreEqual(totalHits, summary.TotalHits);

                allHitDateSummaries.Add(summary);
            }

            StackHashProductHitDateSummaryCollection hitDateSummaryCollection = m_Index.GetHitDateSummaries(productId);
            Assert.AreEqual(numHitDateSummaries, hitDateSummaryCollection.Count);

            foreach (StackHashProductHitDateSummary hitDateData in hitDateSummaryCollection)
            {
                // Find the matching hitDate in the expected list.
                StackHashProductHitDateSummary expectedSummary = allHitDateSummaries.FindHitDate(hitDateData.HitDate);

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(hitDateData));
            }


            // Update the stats again using the same data.
            m_Index.UpdateHitDateStatistics(productId, allHitDateSummaries, false);

            hitDateSummaryCollection = m_Index.GetHitDateSummaries(productId);
            Assert.AreEqual(numHitDateSummaries, hitDateSummaryCollection.Count);

            foreach (StackHashProductHitDateSummary hitDateData in hitDateSummaryCollection)
            {
                // Find the matching hitDate in the expected list.
                StackHashProductHitDateSummary expectedSummary = allHitDateSummaries.FindHitDate(hitDateData.HitDate);
                expectedSummary.TotalHits *= 2;  // Should be double the number of hits.

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(hitDateData));
            }

        }


        /// <summary>
        /// Update 0 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void UpdateHitDateSummaries0()
        {
            updateHitDateSummaries(0);
        }

        /// <summary>
        /// Add 1 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void UpdateHitDateSummaries1()
        {
            updateHitDateSummaries(1);
        }

        /// <summary>
        /// Add 2 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void UpdateHitDateSummaries2()
        {
            updateHitDateSummaries(2);
        }

        /// <summary>
        /// Add 100 hitDate summaries.
        /// </summary>
        [TestMethod]
        public void UpdateHitDateSummaries100()
        {
            updateHitDateSummaries(100);
        }

    }
}
