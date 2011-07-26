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
    /// Summary description for SqlControlTableUnitTests
    /// </summary>
    [TestClass]
    public class SqlControlTableUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlControlTableUnitTests()
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
        /// Get the default product control data.
        /// </summary>
        [TestMethod]
        public void ProductControlDataGetDefault()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int productId = 1000;

            Assert.AreEqual(new DateTime(0), m_Index.GetLastHitTimeLocal(productId));
            Assert.AreEqual(new DateTime(0), m_Index.GetLastSyncCompletedTimeLocal(productId));
            Assert.AreEqual(new DateTime(0), m_Index.GetLastSyncStartedTimeLocal(productId));
            Assert.AreEqual(new DateTime(0), m_Index.GetLastSyncTimeLocal(productId));
        }

        /// <summary>
        /// Set and Get each product control item.
        /// </summary>
        [TestMethod]
        public void ProductControlDataSetGet()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int productId = 1000;

            DateTime testDate = new DateTime(2010, 08, 10, 10, 30, 40, DateTimeKind.Utc);

            m_Index.SetLastHitTimeLocal(productId, testDate);
            m_Index.SetLastSyncCompletedTimeLocal(productId, testDate.AddDays(1));
            m_Index.SetLastSyncStartedTimeLocal(productId, testDate.AddDays(2));
            m_Index.SetLastSyncTimeLocal(productId, testDate.AddDays(3));

            Assert.AreEqual(testDate, m_Index.GetLastHitTimeLocal(productId));
            Assert.AreEqual(testDate.AddDays(1), m_Index.GetLastSyncCompletedTimeLocal(productId));
            Assert.AreEqual(testDate.AddDays(2), m_Index.GetLastSyncStartedTimeLocal(productId));
            Assert.AreEqual(testDate.AddDays(3), m_Index.GetLastSyncTimeLocal(productId));
        }


        /// <summary>
        /// Set and Get for 2 different product ids.
        /// </summary>
        [TestMethod]
        public void ProductControlDataSetGet2Products()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int productId1 = 1000;
            int productId2 = 2000;

            DateTime testDate = new DateTime(2010, 08, 10, 10, 30, 40, DateTimeKind.Utc);

            m_Index.SetLastHitTimeLocal(productId1, testDate);
            m_Index.SetLastSyncCompletedTimeLocal(productId1, testDate.AddDays(1));
            m_Index.SetLastSyncStartedTimeLocal(productId1, testDate.AddDays(2));
            m_Index.SetLastSyncTimeLocal(productId1, testDate.AddDays(3));

            m_Index.SetLastHitTimeLocal(productId2, testDate.AddDays(4));
            m_Index.SetLastSyncCompletedTimeLocal(productId2, testDate.AddDays(5));
            m_Index.SetLastSyncStartedTimeLocal(productId2, testDate.AddDays(6));
            m_Index.SetLastSyncTimeLocal(productId2, testDate.AddDays(7));

            
            Assert.AreEqual(testDate, m_Index.GetLastHitTimeLocal(productId1));
            Assert.AreEqual(testDate.AddDays(1), m_Index.GetLastSyncCompletedTimeLocal(productId1));
            Assert.AreEqual(testDate.AddDays(2), m_Index.GetLastSyncStartedTimeLocal(productId1));
            Assert.AreEqual(testDate.AddDays(3), m_Index.GetLastSyncTimeLocal(productId1));

            Assert.AreEqual(testDate.AddDays(4), m_Index.GetLastHitTimeLocal(productId2));
            Assert.AreEqual(testDate.AddDays(5), m_Index.GetLastSyncCompletedTimeLocal(productId2));
            Assert.AreEqual(testDate.AddDays(6), m_Index.GetLastSyncStartedTimeLocal(productId2));
            Assert.AreEqual(testDate.AddDays(7), m_Index.GetLastSyncTimeLocal(productId2));
        }


        /// <summary>
        /// Get the default control data.
        /// </summary>
        [TestMethod]
        public void ControlDataGetDefault()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            Assert.AreEqual(0, m_Index.SyncCount);
            Assert.AreNotEqual(null, m_Index.SyncProgress);
            Assert.AreEqual(0, m_Index.SyncProgress.ProductId);
            Assert.AreEqual(0, m_Index.SyncProgress.FileId);
            Assert.AreEqual(0, m_Index.SyncProgress.EventId);
            Assert.AreEqual(0, m_Index.SyncProgress.CabId);
            Assert.AreEqual(null, m_Index.SyncProgress.EventTypeName);
        }

        /// <summary>
        /// Set Get SyncCount
        /// </summary>
        [TestMethod]
        public void GetSetSyncCount()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            Assert.AreEqual(0, m_Index.SyncCount);
            m_Index.SyncCount = 1;
            Assert.AreEqual(1, m_Index.SyncCount);
        }

        /// <summary>
        /// Set Get SyncCount twice
        /// </summary>
        [TestMethod]
        public void GetSetSyncCountTwice()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            Assert.AreEqual(0, m_Index.SyncCount);
            m_Index.SyncCount = 1;
            Assert.AreEqual(1, m_Index.SyncCount);
            m_Index.SyncCount = 1000;
            Assert.AreEqual(1000, m_Index.SyncCount);
        }

        /// <summary>
        /// Set SyncProgress
        /// </summary>
        [TestMethod]
        public void GetSetSyncProgress()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashSyncProgress syncProgress = new StackHashSyncProgress(1, 2, 3, "EventTypeName", 4, StackHashSyncPhase.Events);
            m_Index.SyncProgress = syncProgress;

            StackHashSyncProgress newProgress = m_Index.SyncProgress;

            Assert.AreEqual(0, newProgress.CompareTo(syncProgress));
        }


        /// <summary>
        /// Set SyncProgress - reload.
        /// </summary>
        [TestMethod]
        public void GetSetSyncProgressReload()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashSyncProgress syncProgress = new StackHashSyncProgress(1, 2, 3, "EventTypeName", 4, StackHashSyncPhase.Events);
            m_Index.SyncProgress = syncProgress;

            StackHashSyncProgress newProgress = m_Index.SyncProgress;

            Assert.AreEqual(0, newProgress.CompareTo(syncProgress));


            m_Index.Deactivate();
            m_Index.Dispose();

            // Load again to make sure the data was persisted.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.Activate();

            newProgress = m_Index.SyncProgress;

            Assert.AreEqual(0, newProgress.CompareTo(syncProgress));
        }

        /// <summary>
        /// Set SyncProgress N times.
        /// </summary>
        [TestMethod]
        public void GetSetSyncProgressNTimes()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            int numTimes = 100;

            Random rand = new Random(1);

            for (int i = 0; i < numTimes; i++)
            {
                StackHashSyncPhase phase = (StackHashSyncPhase)rand.Next(0, 4);

                StackHashSyncProgress syncProgress = new StackHashSyncProgress(1 + i, 2 + i, 3 + i, "EventTypeName" + i.ToString(), 4 + i, phase);
                m_Index.SyncProgress = syncProgress;

                StackHashSyncProgress newProgress = m_Index.SyncProgress;
                Assert.AreEqual(0, newProgress.CompareTo(syncProgress));
            }
        }
        
        
        /// <summary>
        /// Get task control data default.
        /// </summary>
        [TestMethod]
        public void GetTaskControlDataDefault()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashTaskStatus controlData = m_Index.GetTaskStatistics(StackHashTaskType.DummyTask);

            Assert.AreEqual(0, controlData.FailedCount);
            Assert.AreEqual(0, controlData.LastDurationInSeconds);
            Assert.AreEqual(null, controlData.LastException);
            Assert.AreEqual(new DateTime(0), controlData.LastFailedRunTimeUtc);
            Assert.AreEqual(new DateTime(0), controlData.LastStartedTimeUtc);
            Assert.AreEqual(new DateTime(0), controlData.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(0, controlData.RunCount);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, controlData.ServiceErrorCode);
            Assert.AreEqual(0, controlData.SuccessCount);
            Assert.AreEqual(StackHashTaskState.NotRunning, controlData.TaskState);
            Assert.AreEqual(StackHashTaskType.DummyTask, controlData.TaskType);
        }

        /// <summary>
        /// Get task control data default.
        /// </summary>
        [TestMethod]
        public void SetTaskControlData()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime testDate = new DateTime(2010, 08, 10, 10, 30, 40, DateTimeKind.Utc);


            StackHashTaskStatus setControlData = new StackHashTaskStatus();
            setControlData.TaskType = StackHashTaskType.DummyTask;
            setControlData.FailedCount = 1;
            setControlData.LastDurationInSeconds = 2;
            setControlData.LastFailedRunTimeUtc = testDate;
            setControlData.LastStartedTimeUtc = testDate.AddDays(1);
            setControlData.LastSuccessfulRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
            setControlData.LastException = "Some exception text";
            setControlData.RunCount = 3;
            setControlData.ServiceErrorCode = StackHashServiceErrorCode.NoLicense;
            setControlData.SuccessCount = 4;
            setControlData.TaskState = StackHashTaskState.Queued;

            m_Index.SetTaskStatistics(setControlData);

            StackHashTaskStatus controlData = m_Index.GetTaskStatistics(StackHashTaskType.DummyTask);

            Assert.AreEqual(setControlData.FailedCount, controlData.FailedCount);
            Assert.AreEqual(setControlData.LastDurationInSeconds, controlData.LastDurationInSeconds);
            Assert.AreEqual(setControlData.LastException, controlData.LastException);
            Assert.AreEqual(setControlData.LastFailedRunTimeUtc, controlData.LastFailedRunTimeUtc);
            Assert.AreEqual(setControlData.LastStartedTimeUtc, controlData.LastStartedTimeUtc);
            Assert.AreEqual(setControlData.LastSuccessfulRunTimeUtc, controlData.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(setControlData.RunCount, controlData.RunCount);
            Assert.AreEqual(setControlData.ServiceErrorCode, controlData.ServiceErrorCode);
            Assert.AreEqual(setControlData.SuccessCount, controlData.SuccessCount);
            Assert.AreEqual(setControlData.TaskState, controlData.TaskState);
            Assert.AreEqual(setControlData.TaskType, controlData.TaskType);
        }

        /// <summary>
        /// Get task control data default.
        /// SQL strings can't be set to null.
        /// </summary>
        [TestMethod]
        public void SetTaskControlDataNullValues()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime testDate = new DateTime(0);


            StackHashTaskStatus setControlData = new StackHashTaskStatus();
            setControlData.TaskType = StackHashTaskType.DummyTask;
            setControlData.FailedCount = 0;
            setControlData.LastDurationInSeconds = 0;
            setControlData.LastFailedRunTimeUtc = testDate;
            setControlData.LastStartedTimeUtc = testDate;
            setControlData.LastSuccessfulRunTimeUtc = testDate;
            setControlData.LastException = null;
            setControlData.RunCount = 0;
            setControlData.ServiceErrorCode = StackHashServiceErrorCode.NoLicense;
            setControlData.SuccessCount = 0;
            setControlData.TaskState = StackHashTaskState.Queued;

            m_Index.SetTaskStatistics(setControlData);

            StackHashTaskStatus controlData = m_Index.GetTaskStatistics(StackHashTaskType.DummyTask);

            Assert.AreEqual(setControlData.FailedCount, controlData.FailedCount);
            Assert.AreEqual(setControlData.LastDurationInSeconds, controlData.LastDurationInSeconds);
            Assert.AreEqual(setControlData.LastException, controlData.LastException);
            Assert.AreEqual(setControlData.LastFailedRunTimeUtc, controlData.LastFailedRunTimeUtc);
            Assert.AreEqual(setControlData.LastStartedTimeUtc, controlData.LastStartedTimeUtc);
            Assert.AreEqual(setControlData.LastSuccessfulRunTimeUtc, controlData.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(setControlData.RunCount, controlData.RunCount);
            Assert.AreEqual(setControlData.ServiceErrorCode, controlData.ServiceErrorCode);
            Assert.AreEqual(setControlData.SuccessCount, controlData.SuccessCount);
            Assert.AreEqual(setControlData.TaskState, controlData.TaskState);
            Assert.AreEqual(setControlData.TaskType, controlData.TaskType);
        }

        /// <summary>
        /// Get task control data default 2 task types
        /// </summary>
        [TestMethod]
        public void SetTaskControlData2Tasks()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime testDate = new DateTime(2010, 08, 10, 10, 30, 40, DateTimeKind.Utc);


            StackHashTaskStatus setControlData = new StackHashTaskStatus();
            setControlData.TaskType = StackHashTaskType.DummyTask;
            setControlData.FailedCount = 1;
            setControlData.LastDurationInSeconds = 2;
            setControlData.LastFailedRunTimeUtc = testDate;
            setControlData.LastStartedTimeUtc = testDate.AddDays(1);
            setControlData.LastSuccessfulRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
            setControlData.LastException = "Some exception text";
            setControlData.RunCount = 3;
            setControlData.ServiceErrorCode = StackHashServiceErrorCode.NoLicense;
            setControlData.SuccessCount = 4;
            setControlData.TaskState = StackHashTaskState.Queued;

            m_Index.SetTaskStatistics(setControlData);

            StackHashTaskStatus setControlData2 = new StackHashTaskStatus();
            setControlData2.TaskType = StackHashTaskType.ErrorIndexMoveTask;
            setControlData2.FailedCount = 2;
            setControlData2.LastDurationInSeconds = 3;
            setControlData2.LastFailedRunTimeUtc = testDate.AddDays(3);
            setControlData2.LastStartedTimeUtc = testDate.AddDays(4);
            setControlData2.LastSuccessfulRunTimeUtc = testDate.AddDays(5);
            setControlData2.LastException = "Some exception text 2";
            setControlData2.RunCount = 4;
            setControlData2.ServiceErrorCode = StackHashServiceErrorCode.MoveInProgress;
            setControlData2.SuccessCount = 5;
            setControlData2.TaskState = StackHashTaskState.Faulted;

            m_Index.SetTaskStatistics(setControlData2);


            StackHashTaskStatus controlData = m_Index.GetTaskStatistics(setControlData.TaskType);

            Assert.AreEqual(setControlData.FailedCount, controlData.FailedCount);
            Assert.AreEqual(setControlData.LastDurationInSeconds, controlData.LastDurationInSeconds);
            Assert.AreEqual(setControlData.LastException, controlData.LastException);
            Assert.AreEqual(setControlData.LastFailedRunTimeUtc, controlData.LastFailedRunTimeUtc);
            Assert.AreEqual(setControlData.LastStartedTimeUtc, controlData.LastStartedTimeUtc);
            Assert.AreEqual(setControlData.LastSuccessfulRunTimeUtc, controlData.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(setControlData.RunCount, controlData.RunCount);
            Assert.AreEqual(setControlData.ServiceErrorCode, controlData.ServiceErrorCode);
            Assert.AreEqual(setControlData.SuccessCount, controlData.SuccessCount);
            Assert.AreEqual(setControlData.TaskState, controlData.TaskState);
            Assert.AreEqual(setControlData.TaskType, controlData.TaskType);


            StackHashTaskStatus controlData2 = m_Index.GetTaskStatistics(setControlData2.TaskType);

            Assert.AreEqual(setControlData2.FailedCount, controlData2.FailedCount);
            Assert.AreEqual(setControlData2.LastDurationInSeconds, controlData2.LastDurationInSeconds);
            Assert.AreEqual(setControlData2.LastException, controlData2.LastException);
            Assert.AreEqual(setControlData2.LastFailedRunTimeUtc, controlData2.LastFailedRunTimeUtc);
            Assert.AreEqual(setControlData2.LastStartedTimeUtc, controlData2.LastStartedTimeUtc);
            Assert.AreEqual(setControlData2.LastSuccessfulRunTimeUtc, controlData2.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(setControlData2.RunCount, controlData2.RunCount);
            Assert.AreEqual(setControlData2.ServiceErrorCode, controlData2.ServiceErrorCode);
            Assert.AreEqual(setControlData2.SuccessCount, controlData2.SuccessCount);
            Assert.AreEqual(setControlData2.TaskState, controlData2.TaskState);
            Assert.AreEqual(setControlData2.TaskType, controlData2.TaskType);
        }

    }
}
