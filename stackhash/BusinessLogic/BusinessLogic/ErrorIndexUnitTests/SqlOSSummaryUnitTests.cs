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
    /// Summary description for SqOperatingSystemSummaryUnitTests
    /// </summary>
    [TestClass]
    public class SqOperatingSystemSummaryUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\OperatingSystemSummaryTests";

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

        public SqOperatingSystemSummaryUnitTests()
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
        /// Adds the rollup of OperatingSystems.
        /// </summary>
        public void addOperatingSystemSummaries(int numOperatingSystemSummaries)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            String operatingSystemNameBase = "Microsoft Windows Vista";
            String operatingSystemVersionBase = "6.0.0.1212.0";
            int productId = 1234;

            // Check an OperatingSystem that doesn't exist.
            Assert.AreEqual(false, m_Index.OperatingSystemSummaryExists(productId, operatingSystemNameBase, operatingSystemVersionBase));

            StackHashProductOperatingSystemSummaryCollection allOperatingSystemSummaries = new StackHashProductOperatingSystemSummaryCollection();

            for (int operatingSystemCount = 0; operatingSystemCount < numOperatingSystemSummaries; operatingSystemCount++)
            {
                String operatingSystemName = operatingSystemNameBase + operatingSystemCount.ToString();
                String operatingSystemVersion = operatingSystemVersionBase + operatingSystemCount.ToString();

                int totalHits = operatingSystemCount + 1;
                m_Index.AddOperatingSystem(operatingSystemName, operatingSystemVersion);
                short operatingSystemId = m_Index.GetOperatingSystemId(operatingSystemName, operatingSystemVersion);

                m_Index.AddOperatingSystemSummary(productId, operatingSystemId, totalHits, false);

                // Check the OperatingSystem exists now.
                Assert.AreEqual(true, m_Index.OperatingSystemSummaryExists(productId, operatingSystemName, operatingSystemVersion));

                // Get the specific OperatingSystem and make sure it was stored properly.
                StackHashProductOperatingSystemSummary summary = m_Index.GetOperatingSystemSummaryForProduct(productId, operatingSystemName, operatingSystemVersion);
                Assert.AreEqual(operatingSystemName, summary.OperatingSystemName);
                Assert.AreEqual(operatingSystemVersion, summary.OperatingSystemVersion);
                Assert.AreEqual(totalHits, summary.TotalHits);

                allOperatingSystemSummaries.Add(summary);
            }

            StackHashProductOperatingSystemSummaryCollection operatingSystemSummaryCollection = m_Index.GetOperatingSystemSummaries(productId);
            Assert.AreEqual(numOperatingSystemSummaries, operatingSystemSummaryCollection.Count);

            foreach (StackHashProductOperatingSystemSummary operatingSystemData in operatingSystemSummaryCollection)
            {
                // Find the matching OperatingSystem in the expected list.
                StackHashProductOperatingSystemSummary expectedSummary = 
                    allOperatingSystemSummaries.FindOperatingSystem(operatingSystemData.OperatingSystemName, operatingSystemData.OperatingSystemVersion);

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(operatingSystemData));
            }
        }


        /// <summary>
        /// Add 0 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void AddOperatingSystemSummaries0()
        {
            addOperatingSystemSummaries(0);
        }

        /// <summary>
        /// Add 1 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void AddOperatingSystemSummaries1()
        {
            addOperatingSystemSummaries(1);
        }

        /// <summary>
        /// Add 2 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void AddOperatingSystemSummaries2()
        {
            addOperatingSystemSummaries(2);
        }

        /// <summary>
        /// Add 100 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void AddOperatingSystemSummaries100()
        {
            addOperatingSystemSummaries(100);
        }


        /// <summary>
        /// Updates the rollup of OperatingSystems.
        /// </summary>
        public void updateOperatingSystemSummaries(int numOperatingSystemSummaries)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            String operatingSystemNameBase = "Microsoft Windows Vista";
            String operatingSystemVersionBase = "6.0.0.1212.0";
            int productId = 1234;

            // Check an OperatingSystem that doesn't exist.
            Assert.AreEqual(false, m_Index.OperatingSystemSummaryExists(productId, operatingSystemNameBase, operatingSystemVersionBase));

            StackHashProductOperatingSystemSummaryCollection allOperatingSystemSummaries = new StackHashProductOperatingSystemSummaryCollection();

            for (int operatingSystemCount = 0; operatingSystemCount < numOperatingSystemSummaries; operatingSystemCount++)
            {
                String operatingSystemName = operatingSystemNameBase + operatingSystemCount.ToString();
                String operatingSystemVersion = operatingSystemVersionBase + operatingSystemCount.ToString();

                int totalHits = operatingSystemCount + 1;
                m_Index.AddOperatingSystem(operatingSystemName, operatingSystemVersion);
                short operatingSystemId = m_Index.GetOperatingSystemId(operatingSystemName, operatingSystemVersion);

                m_Index.AddOperatingSystemSummary(productId, operatingSystemId, totalHits, false);

                // Check the OperatingSystem exists now.
                Assert.AreEqual(true, m_Index.OperatingSystemSummaryExists(productId, operatingSystemName, operatingSystemVersion));

                // Get the specific OperatingSystem and make sure it was stored properly.
                StackHashProductOperatingSystemSummary summary = m_Index.GetOperatingSystemSummaryForProduct(productId, operatingSystemName, operatingSystemVersion);
                Assert.AreEqual(operatingSystemName, summary.OperatingSystemName);
                Assert.AreEqual(operatingSystemVersion, summary.OperatingSystemVersion);
                Assert.AreEqual(totalHits, summary.TotalHits);

                allOperatingSystemSummaries.Add(summary);
            }

            StackHashProductOperatingSystemSummaryCollection operatingSystemSummaryCollection = m_Index.GetOperatingSystemSummaries(productId);
            Assert.AreEqual(numOperatingSystemSummaries, operatingSystemSummaryCollection.Count);

            foreach (StackHashProductOperatingSystemSummary operatingSystemData in operatingSystemSummaryCollection)
            {
                // Find the matching OperatingSystem in the expected list.
                StackHashProductOperatingSystemSummary expectedSummary =
                    allOperatingSystemSummaries.FindOperatingSystem(operatingSystemData.OperatingSystemName, operatingSystemData.OperatingSystemVersion);

                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(operatingSystemData));
            }

            // Update the stats again.
            m_Index.UpdateOperatingSystemStatistics(productId, allOperatingSystemSummaries, false);

            operatingSystemSummaryCollection = m_Index.GetOperatingSystemSummaries(productId);
            Assert.AreEqual(numOperatingSystemSummaries, operatingSystemSummaryCollection.Count);

            foreach (StackHashProductOperatingSystemSummary operatingSystemData in operatingSystemSummaryCollection)
            {
                // Find the matching OperatingSystem in the expected list.
                StackHashProductOperatingSystemSummary expectedSummary =
                    allOperatingSystemSummaries.FindOperatingSystem(operatingSystemData.OperatingSystemName, operatingSystemData.OperatingSystemVersion);
                expectedSummary.TotalHits *= 2; // Expect twice the number of hits back.
                Assert.AreNotEqual(null, expectedSummary);
                Assert.AreEqual(0, expectedSummary.CompareTo(operatingSystemData));
            }
        }


        /// <summary>
        /// Update 0 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void UpdateOperatingSystemSummaries0()
        {
            updateOperatingSystemSummaries(0);
        }

        /// <summary>
        /// Update 1 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void UpdateOperatingSystemSummaries1()
        {
            updateOperatingSystemSummaries(1);
        }

        /// <summary>
        /// Update 2 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void UpdateOperatingSystemSummaries2()
        {
            updateOperatingSystemSummaries(2);
        }

        /// <summary>
        /// Update 100 OperatingSystem summaries.
        /// </summary>
        [TestMethod]
        public void UpdateOperatingSystemSummaries100()
        {
            updateOperatingSystemSummaries(100);
        }

        /// <summary>
        /// Checks for OS name and version nulls.
        /// </summary>
        public void addMultipleOperatingSystems(StackHashProductOperatingSystemSummaryCollection operatingSystemsToAdd)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int productId = 1234;

            StackHashProductOperatingSystemSummaryCollection allOSSummaries = new StackHashProductOperatingSystemSummaryCollection();

            foreach (StackHashProductOperatingSystemSummary operatingSystem in operatingSystemsToAdd)
            {
                m_Index.AddOperatingSystem(operatingSystem.OperatingSystemName, operatingSystem.OperatingSystemVersion);
                short osId = m_Index.GetOperatingSystemId(operatingSystem.OperatingSystemName, operatingSystem.OperatingSystemVersion);
                m_Index.AddOperatingSystemSummary(productId, osId, operatingSystem.TotalHits, false);

                // Check the OS exists now.
                Assert.AreEqual(true, m_Index.OperatingSystemSummaryExists(productId, operatingSystem.OperatingSystemName, operatingSystem.OperatingSystemVersion));

                // Get the specific OS and make sure it was stored properly.
                StackHashProductOperatingSystemSummary summary = m_Index.GetOperatingSystemSummaryForProduct(productId, operatingSystem.OperatingSystemName, operatingSystem.OperatingSystemVersion);
                Assert.AreEqual(operatingSystem.OperatingSystemName, summary.OperatingSystemName);
                Assert.AreEqual(operatingSystem.OperatingSystemVersion, summary.OperatingSystemVersion);
            }


            StackHashProductOperatingSystemSummaryCollection osSummaryCollection = m_Index.GetOperatingSystemSummaries(productId);
            Assert.AreEqual(operatingSystemsToAdd.Count, osSummaryCollection.Count);

            foreach (StackHashProductOperatingSystemSummary os in osSummaryCollection)
            {
                StackHashProductOperatingSystemSummary expectedSummary = operatingSystemsToAdd.FindOperatingSystem(os.OperatingSystemName, os.OperatingSystemVersion);
                Assert.AreEqual(0, expectedSummary.CompareTo(os));
            }

            // Now update the statistics again using the same values.
            m_Index.UpdateOperatingSystemStatistics(productId, operatingSystemsToAdd, false);

            osSummaryCollection = m_Index.GetOperatingSystemSummaries(productId);
            Assert.AreEqual(operatingSystemsToAdd.Count, osSummaryCollection.Count);

            foreach (StackHashProductOperatingSystemSummary os in osSummaryCollection)
            {
                StackHashProductOperatingSystemSummary expectedSummary = operatingSystemsToAdd.FindOperatingSystem(os.OperatingSystemName, os.OperatingSystemVersion);
                expectedSummary.TotalHits *= 2;
                Assert.AreEqual(0, expectedSummary.CompareTo(os));
            }

            // Now update the statistics again using the same values.
            m_Index.UpdateOperatingSystemStatistics(productId, operatingSystemsToAdd, false);
        }

        /// <summary>
        /// Simple valid add - no nulls.
        /// </summary>
        [TestMethod]
        public void AddMultipleOperatingSystemsOneOs()
        {
            StackHashProductOperatingSystemSummaryCollection allOperatingSystems = new StackHashProductOperatingSystemSummaryCollection()
            {
                new StackHashProductOperatingSystemSummary("Vista", "1.2.3.4", 20)
            };

            addMultipleOperatingSystems(allOperatingSystems);
        }

        
        /// <summary>
        /// Null OS Name
        /// </summary>
        [TestMethod]
        public void AddMultipleOperatingSystemsNullOSName()
        {
            StackHashProductOperatingSystemSummaryCollection allOperatingSystems = new StackHashProductOperatingSystemSummaryCollection()
            {
                new StackHashProductOperatingSystemSummary(null, "1.2.3.4", 20)
            };

            addMultipleOperatingSystems(allOperatingSystems);
        }

        /// <summary>
        /// Null OS Version
        /// </summary>
        [TestMethod]
        public void AddMultipleOperatingSystemsNullOSVersion()
        {
            StackHashProductOperatingSystemSummaryCollection allOperatingSystems = new StackHashProductOperatingSystemSummaryCollection()
            {
                new StackHashProductOperatingSystemSummary("Vista", null, 20)
            };

            addMultipleOperatingSystems(allOperatingSystems);
        }

        /// <summary>
        /// Null OS Version and name
        /// </summary>
        [TestMethod]
        public void AddMultipleOperatingSystemsNullOSVersionNullName()
        {
            StackHashProductOperatingSystemSummaryCollection allOperatingSystems = new StackHashProductOperatingSystemSummaryCollection()
            {
                new StackHashProductOperatingSystemSummary(null, null, 20)
            };

            addMultipleOperatingSystems(allOperatingSystems);
        }

        /// <summary>
        /// Multiple null variants.
        /// </summary>
        [TestMethod]
        public void AddMultipleOperatingSystemsNullvariants()
        {
            StackHashProductOperatingSystemSummaryCollection allOperatingSystems = new StackHashProductOperatingSystemSummaryCollection()
            {
                new StackHashProductOperatingSystemSummary(null, null, 10),
                new StackHashProductOperatingSystemSummary("Vista", null, 20),
                new StackHashProductOperatingSystemSummary(null, "1.2.3.4", 30),
                new StackHashProductOperatingSystemSummary("Vista", "1.2.3.4", 40)
            };

            addMultipleOperatingSystems(allOperatingSystems);
        }

    }
}
