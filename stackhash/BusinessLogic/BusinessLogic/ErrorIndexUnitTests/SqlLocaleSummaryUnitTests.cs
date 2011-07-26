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
    /// Summary description for SqlLocaleSummaryUnitTests
    /// </summary>
    [TestClass]
    public class SqlLocaleSummaryUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\LocaleSummaryTests";

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

        public SqlLocaleSummaryUnitTests()
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
        public void addLocaleSummaries(int numLocaleSummaries)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            String localeCodeBase = "US-";
            String localeNameBase = "ENGLISH";
            int productId = 1234;

            // Check a locale that doesn't exist.
            Assert.AreEqual(false, m_Index.LocaleSummaryExists(1245, 12121));

            StackHashProductLocaleSummaryCollection allLocaleSummaries = new StackHashProductLocaleSummaryCollection();

            for (int localeCount = 0; localeCount < numLocaleSummaries; localeCount++)
            {
                int localeId = 1000 + localeCount;
                int totalHits = localeCount + 1;
                m_Index.AddLocale(localeId, localeCodeBase + localeId.ToString(), localeNameBase + localeId.ToString());

                m_Index.AddLocaleSummary(productId, localeId, totalHits, false);

                // Check the locale exists now.
                Assert.AreEqual(true, m_Index.LocaleSummaryExists(productId, localeId));

                // Get the specific locale and make sure it was stored properly.
                StackHashProductLocaleSummary summary = m_Index.GetLocaleSummaryForProduct(productId, localeId);
                Assert.AreEqual(localeId, summary.Lcid);
                Assert.AreEqual(localeCodeBase + localeId.ToString(), summary.Locale);
                Assert.AreEqual(localeNameBase + localeId.ToString(), summary.Language);
                Assert.AreEqual(totalHits, summary.TotalHits);

                allLocaleSummaries.Add(summary);
            }

            StackHashProductLocaleSummaryCollection localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(numLocaleSummaries, localeSummaryCollection.Count);

            foreach (StackHashProductLocaleSummary localeData in localeSummaryCollection)
            {
                // Find the matching locale in the expected list.
                StackHashProductLocaleSummary expectedSummary = allLocaleSummaries.FindLocale(localeData.Lcid);

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(localeData));
            }
        }


        /// <summary>
        /// Add 0 locale summaries.
        /// </summary>
        [TestMethod]
        public void AddLocaleSummaries0()
        {
            addLocaleSummaries(0);
        }

        /// <summary>
        /// Add 1 locale summaries.
        /// </summary>
        [TestMethod]
        public void AddLocaleSummaries1()
        {
            addLocaleSummaries(1);
        }

        /// <summary>
        /// Add 2 locale summaries.
        /// </summary>
        [TestMethod]
        public void AddLocaleSummaries2()
        {
            addLocaleSummaries(2);
        }

        /// <summary>
        /// Add 100 locale summaries.
        /// </summary>
        [TestMethod]
        public void AddLocaleSummaries100()
        {
            addLocaleSummaries(100);
        }

        /// <summary>
        /// Updates existing locale summary total hits.
        /// </summary>
        public void updateLocaleSummaries(int numLocaleSummaries)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            String localeCodeBase = "US-";
            String localeNameBase = "ENGLISH";
            int productId = 1234;

            // Check a locale that doesn't exist.
            Assert.AreEqual(false, m_Index.LocaleSummaryExists(1245, 12121));

            StackHashProductLocaleSummaryCollection allLocaleSummaries = new StackHashProductLocaleSummaryCollection();

            for (int localeCount = 0; localeCount < numLocaleSummaries; localeCount++)
            {
                int localeId = 1000 + localeCount;
                int totalHits = localeCount + 1;
                m_Index.AddLocale(localeId, localeCodeBase + localeId.ToString(), localeNameBase + localeId.ToString());

                m_Index.AddLocaleSummary(productId, localeId, totalHits, false);

                // Check the locale exists now.
                Assert.AreEqual(true, m_Index.LocaleSummaryExists(productId, localeId));

                // Get the specific locale and make sure it was stored properly.
                StackHashProductLocaleSummary summary = m_Index.GetLocaleSummaryForProduct(productId, localeId);
                Assert.AreEqual(localeId, summary.Lcid);
                Assert.AreEqual(localeCodeBase + localeId.ToString(), summary.Locale);
                Assert.AreEqual(localeNameBase + localeId.ToString(), summary.Language);
                Assert.AreEqual(totalHits, summary.TotalHits);

                allLocaleSummaries.Add(summary);
            }

            StackHashProductLocaleSummaryCollection localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(numLocaleSummaries, localeSummaryCollection.Count);

            foreach (StackHashProductLocaleSummary localeData in localeSummaryCollection)
            {
                // Find the matching locale in the expected list.
                StackHashProductLocaleSummary expectedSummary = allLocaleSummaries.FindLocale(localeData.Lcid);

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(localeData));
            }

            // Now update the statistics again using the same values.
            m_Index.UpdateLocaleStatistics(productId, allLocaleSummaries, false);

            // Values should have doubled.
            localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(numLocaleSummaries, localeSummaryCollection.Count);

            foreach (StackHashProductLocaleSummary localeData in localeSummaryCollection)
            {
                // Find the matching locale in the expected list.
                StackHashProductLocaleSummary expectedSummary = allLocaleSummaries.FindLocale(localeData.Lcid);
                expectedSummary.TotalHits *= 2;

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(localeData));
            }

        }


        /// <summary>
        /// Update 0 locale summaries.
        /// </summary>
        [TestMethod]
        public void UpdateLocaleSummaries0()
        {
            updateLocaleSummaries(0);
        }

        /// <summary>
        /// Update 1 locale summaries.
        /// </summary>
        [TestMethod]
        public void UpdateLocaleSummaries1()
        {
            updateLocaleSummaries(1);
        }

        /// <summary>
        /// Update 2 locale summaries.
        /// </summary>
        [TestMethod]
        public void UpdateLocaleSummaries2()
        {
            updateLocaleSummaries(2);
        }

        /// <summary>
        /// Update 10 locale summaries.
        /// </summary>
        [TestMethod]
        public void UpdateLocaleSummaries10()
        {
            updateLocaleSummaries(10);
        }

        /// <summary>
        /// Update 100 locale summaries.
        /// </summary>
        [TestMethod]
        public void UpdateLocaleSummaries100()
        {
            updateLocaleSummaries(100);
        }

        /// <summary>
        /// Checks for localeCode and localeName accepted ok.
        /// </summary>
        public void addSingleLocale(int localeId, String localeCode, String localeName)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int productId = 1234;

            // Check a locale that doesn't exist.
            Assert.AreEqual(false, m_Index.LocaleSummaryExists(1245, 12121));

            StackHashProductLocaleSummaryCollection allLocaleSummaries = new StackHashProductLocaleSummaryCollection();

            int totalHits = 100;
            m_Index.AddLocale(localeId, localeCode, localeName);
            m_Index.AddLocaleSummary(productId, localeId, totalHits, false);

            // Check the locale exists now.
            Assert.AreEqual(true, m_Index.LocaleSummaryExists(productId, localeId));

            // Get the specific locale and make sure it was stored properly.
            StackHashProductLocaleSummary summary = m_Index.GetLocaleSummaryForProduct(productId, localeId);
            Assert.AreEqual(localeId, summary.Lcid);
            Assert.AreEqual(localeCode, summary.Locale);
            Assert.AreEqual(localeName, summary.Language);
            Assert.AreEqual(totalHits, summary.TotalHits);


            StackHashProductLocaleSummaryCollection localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(1, localeSummaryCollection.Count);

            StackHashProductLocaleSummary expectedSummary = new StackHashProductLocaleSummary(localeName, localeId, localeCode, totalHits);
            Assert.AreEqual(0, expectedSummary.CompareTo(localeSummaryCollection[0]));

            // Now update the statistics again using the same values.
            allLocaleSummaries.Add(expectedSummary);
            m_Index.UpdateLocaleStatistics(productId, allLocaleSummaries, false);

            // Values should have doubled.
            localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(1, localeSummaryCollection.Count);

            expectedSummary = new StackHashProductLocaleSummary(localeName, localeId, localeCode, totalHits * 2);
            Assert.AreEqual(0, expectedSummary.CompareTo(localeSummaryCollection[0]));

        }

        /// <summary>
        /// Add a single valid locale and locale summary.
        /// </summary>
        [TestMethod]
        public void AddSingleLocale()
        {
            addSingleLocale(10, "en-us", "English");
        }

        /// <summary>
        /// Null localecode
        /// </summary>
        [TestMethod]
        public void AddSingleLocaleNullCode()
        {
            addSingleLocale(10, null, "English");
        }

        /// <summary>
        /// Null localeCode and localeName
        /// </summary>
        [TestMethod]
        public void AddSingleLocaleNullCodeNullName()
        {
            addSingleLocale(10, null, null);
        }

        /// <summary>
        /// Null localeName
        /// </summary>
        [TestMethod]
        public void AddSingleLocaleNullName()
        {
            addSingleLocale(10, "en-us", null);
        }

        /// <summary>
        /// Checks for localeCode and localeName accepted ok.
        /// </summary>
        public void addMultipleLocales(StackHashProductLocaleSummaryCollection localesToAdd)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int productId = 1234;

            // Check a locale that doesn't exist.
            Assert.AreEqual(false, m_Index.LocaleSummaryExists(1245, 12121));

            StackHashProductLocaleSummaryCollection allLocaleSummaries = new StackHashProductLocaleSummaryCollection();

            foreach (StackHashProductLocaleSummary locale in localesToAdd)
            {
                m_Index.AddLocale(locale.Lcid, locale.Locale, locale.Language);
                m_Index.AddLocaleSummary(productId, locale.Lcid, locale.TotalHits, false);

                // Check the locale exists now.
                Assert.AreEqual(true, m_Index.LocaleSummaryExists(productId, locale.Lcid));

                // Get the specific locale and make sure it was stored properly.
                StackHashProductLocaleSummary summary = m_Index.GetLocaleSummaryForProduct(productId, locale.Lcid);
                Assert.AreEqual(locale.Lcid, summary.Lcid);
                Assert.AreEqual(locale.Locale, summary.Locale);
                Assert.AreEqual(locale.Language, summary.Language);
                Assert.AreEqual(locale.TotalHits, summary.TotalHits);
            }


            StackHashProductLocaleSummaryCollection localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(localesToAdd.Count, localeSummaryCollection.Count);

            foreach (StackHashProductLocaleSummary loadedLocale in localeSummaryCollection)
            {
                StackHashProductLocaleSummary expectedSummary = localesToAdd.FindLocale(loadedLocale.Lcid);
                Assert.AreEqual(0, expectedSummary.CompareTo(loadedLocale));
            }

            // Now update the statistics again using the same values.
            m_Index.UpdateLocaleStatistics(productId, localesToAdd, false);

            // Values should have doubled.
            localeSummaryCollection = m_Index.GetLocaleSummaries(productId);
            Assert.AreEqual(localesToAdd.Count, localeSummaryCollection.Count);

            foreach (StackHashProductLocaleSummary loadedLocale in localeSummaryCollection)
            {
                StackHashProductLocaleSummary expectedSummary = localesToAdd.FindLocale(loadedLocale.Lcid);
                expectedSummary.TotalHits *= 2;
                Assert.AreEqual(0, expectedSummary.CompareTo(loadedLocale));
            }
        }

        /// <summary>
        /// Add all the null variants.
        /// </summary>
        [TestMethod]
        public void AddMultipleLocales()
        {
            StackHashProductLocaleSummaryCollection allLocales = new StackHashProductLocaleSummaryCollection()
            {
                new StackHashProductLocaleSummary(null, 100, null, 10),
                new StackHashProductLocaleSummary("English", 101, null, 10),
                new StackHashProductLocaleSummary(null, 102, "en-us", 10),
                new StackHashProductLocaleSummary("English", 103, "en-us", 10),
            };

            addMultipleLocales(allLocales);
        }

    
    }
}
