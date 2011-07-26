using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SqlUpdateTableUnitTests
    /// </summary>
    [TestClass]
    public class SqlUpdateTableUnitTests
    {
        private SqlErrorIndex m_Index;
        private String m_RootCabFolder;
        private TestContext testContextInstance;

        public SqlUpdateTableUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        /// Adds the specified number of Updates to the UpdateTable.
        /// </summary>
        public void addUpdates(int numUpdates, bool incrementIds)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();
            m_Index.UpdateTableActive = true;

            int productId = 11111111;
            int fileId = 22222222;
            int eventId = 33333333;
            String eventTypeName = "EventTypeName";
            int componentId = 44444444;

            // Check that no update exists.
            StackHashBugTrackerUpdate update = m_Index.GetFirstUpdate();
            Assert.AreEqual(null, update);

            List<StackHashBugTrackerUpdate> allUpdates = new List<StackHashBugTrackerUpdate>();

            for (int updateCount = 0; updateCount < numUpdates; updateCount++)
            {
                StackHashBugTrackerUpdate newUpdate = new StackHashBugTrackerUpdate()
                {
                    EntryId = updateCount + 1,  // This will be the expected entryid - this is an automatic update field in the database.
                    DateChanged = DateTime.Now.RoundToNextSecond(),  // Database dates are not as accurate as .NET dates.
                    DataThatChanged = StackHashDataChanged.Event,
                    TypeOfChange = StackHashChangeType.NewEntry,
                    ProductId = productId,
                    FileId = fileId,
                    EventId = eventId,
                    EventTypeName = eventTypeName,
                    ChangedObjectId = componentId,
                };

                if (incrementIds)
                {
                    productId++;
                    fileId++;
                    eventId++;
                    componentId++;
                }


                m_Index.AddUpdate(newUpdate);

                allUpdates.Add(newUpdate); 
            }

            int nextIndex = 0;
            StackHashBugTrackerUpdate retrievedUpdate;
            while ((retrievedUpdate = m_Index.GetFirstUpdate()) != null)
            {
                Assert.AreEqual(0, retrievedUpdate.CompareTo(allUpdates[nextIndex]));

                // Delete the entry.
                m_Index.RemoveUpdate(retrievedUpdate);

                nextIndex++;
            }

            Assert.AreEqual(allUpdates.Count, nextIndex);

            update = m_Index.GetFirstUpdate();
            Assert.AreEqual(null, update);
        }


        /// <summary>
        /// Add 0 Updates.
        /// </summary>
        [TestMethod]
        public void AddZeroUpdate()
        {
            addUpdates(0, true);
        }

        /// <summary>
        /// Add 1 Update.
        /// </summary>
        [TestMethod]
        public void Add1Update()
        {
            addUpdates(1, true);
        }

        /// <summary>
        /// Add 2 Updates.
        /// </summary>
        [TestMethod]
        public void Add2Update()
        {
            addUpdates(2, true);
        }

        /// <summary>
        /// Add 100 Updates.
        /// </summary>
        [TestMethod]
        public void Add100Update()
        {
            addUpdates(100, true);
        }

        /// <summary>
        /// Add 2 Updates - identical entries
        /// </summary>
        [TestMethod]
        public void Add2UpdateIdentical()
        {
            addUpdates(2, false);
        }


        /// <summary>
        /// Adds the specified number of Updates to the UpdateTable and then CLEARS them.
        /// </summary>
        public void addAndClearAllUpdates(int numUpdates, bool incrementIds)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();
            m_Index.UpdateTableActive = true;

            int productId = 11111111;
            int fileId = 22222222;
            int eventId = 33333333;
            String eventTypeName = "EventTypeName";
            int componentId = 44444444;

            // Check that no update exists.
            StackHashBugTrackerUpdate update = m_Index.GetFirstUpdate();
            Assert.AreEqual(null, update);

            List<StackHashBugTrackerUpdate> allUpdates = new List<StackHashBugTrackerUpdate>();

            for (int updateCount = 0; updateCount < numUpdates; updateCount++)
            {
                StackHashBugTrackerUpdate newUpdate = new StackHashBugTrackerUpdate()
                {
                    EntryId = updateCount + 1,  // This will be the expected entryid - this is an automatic update field in the database.
                    DateChanged = DateTime.Now.RoundToNextSecond(),  // Database dates are not as accurate as .NET dates.
                    DataThatChanged = StackHashDataChanged.Event,
                    TypeOfChange = StackHashChangeType.NewEntry,
                    ProductId = productId,
                    FileId = fileId,
                    EventId = eventId,
                    EventTypeName = eventTypeName,
                    ChangedObjectId = componentId,
                };

                if (incrementIds)
                {
                    productId++;
                    fileId++;
                    eventId++;
                    componentId++;
                }


                m_Index.AddUpdate(newUpdate);

                allUpdates.Add(newUpdate);
            }

            m_Index.ClearAllUpdates();

            Assert.AreEqual(null, m_Index.GetFirstUpdate());
        }


        /// <summary>
        /// Add 0 Updates.
        /// </summary>
        [TestMethod]
        public void AddAndClearAllUpdates0Updates()
        {
            addAndClearAllUpdates(0, true);
        }

        /// <summary>
        /// Add 1 Update.
        /// </summary>
        [TestMethod]
        public void AddAndClearAllUpdates1Updates()
        {
            addAndClearAllUpdates(1, true);
        }

        /// <summary>
        /// Add 2 Update.
        /// </summary>
        [TestMethod]
        public void AddAndClearAllUpdates2Updates()
        {
            addAndClearAllUpdates(2, true);
        }

        /// <summary>
        /// Add 20 Update.
        /// </summary>
        [TestMethod]
        public void AddAndClearAllUpdates20Updates()
        {
            addAndClearAllUpdates(20, true);
        }
    }
}
