using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashErrorIndex;
using StackHashUtilities;
using StackHashBusinessObjects;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for ErrorIndexPersistedSettingsUnitTests
    /// </summary>
    [TestClass]
    public class ErrorIndexPersistedSettingsUnitTests
    {
        private string m_TempPath;
        private TestContext testContextInstance;

        public ErrorIndexPersistedSettingsUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashTest_ErrorIndex";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath))
                PathUtils.DeleteDirectory(m_TempPath, true);
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

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void ErrorIndexXmlSettingsConstructor()
        {
            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(null);
        }

        [TestMethod]
        public void ErrorIndexXmlSettingsConstructNoFile()
        {
            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);
        }

        [TestMethod]
        public void ErrorIndexXmlSettingsSaveDefaultsAndReConstruct()
        {
            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);

            settings.Save();

            // Reconstruct.
            settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);

            // Not found but should return a default.
            Assert.AreEqual(DateTimeKind.Local, settings.GetLastSyncTime(1).Kind);
        }

        [TestMethod]
        public void ErrorIndexXmlSettingsSaveLoadNonDefaultSettings()
        {
            DateTime testTime = DateTime.Now;
            DateTime testTime2 = DateTime.Now.AddDays(1);


            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);


            settings.SetLastSyncTime(1, testTime);
            settings.SetLastSyncTime(2, testTime2);

            Assert.AreEqual(testTime, settings.GetLastSyncTime(1));
            Assert.AreEqual(testTime2, settings.GetLastSyncTime(2));
                        
            settings.Save();

            // Reconstruct.
            settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreEqual(testTime, settings.GetLastSyncTime(1));
            Assert.AreEqual(testTime2, settings.GetLastSyncTime(2));
            Assert.AreEqual(DateTimeKind.Local, settings.GetLastSyncTime(1).Kind);
            Assert.AreEqual(DateTimeKind.Local, settings.GetLastSyncTime(2).Kind);
        }
        [TestMethod]
        public void ErrorIndexXmlSettingsSaveLoadSameInstance()
        {
            DateTime testTime = DateTime.Now;


            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);


            settings.SetLastSyncTime(20000, testTime);
            settings.Save();
            settings.Load();

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreEqual(testTime, settings.GetLastSyncTime(20000));
        }
        [TestMethod]
        public void ErrorIndexXmlSettingsSetLastTimeShouldBePersistedWithoutExplicitSave()
        {
            DateTime testTime = DateTime.Now;


            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);


            settings.SetLastSyncTime(20000, testTime);

            settings.Load();

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreEqual(testTime, settings.GetLastSyncTime(20000));
        }


        [TestMethod]
        public void ErrorIndexXmlSettingsDefaultNoTaskData()
        {
            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);
            
            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);
            StackHashTaskStatus taskStatus = settings.GetTaskStatistics(StackHashTaskType.AnalyzeTask);

            Assert.AreEqual(0, taskStatus.FailedCount);
            Assert.AreEqual(0, taskStatus.SuccessCount);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);
            Assert.AreEqual(null, taskStatus.LastException);
            Assert.AreEqual(0, taskStatus.LastFailedRunTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastStartedTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);
        }
        [TestMethod]
        public void ErrorIndexXmlSettingsDefaultSetOneTaskData()
        {
            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);
            StackHashTaskStatus taskStatus = settings.GetTaskStatistics(StackHashTaskType.DebugScriptTask);

            Assert.AreEqual(0, taskStatus.FailedCount);
            Assert.AreEqual(0, taskStatus.SuccessCount);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);
            Assert.AreEqual(null, taskStatus.LastException);
            Assert.AreEqual(0, taskStatus.LastFailedRunTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastStartedTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);


            taskStatus.FailedCount = 10;
            taskStatus.LastDurationInSeconds = 20;
            taskStatus.LastException = "Test";
            taskStatus.LastFailedRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(1);
            taskStatus.LastStartedTimeUtc = DateTime.Now.ToUniversalTime().AddDays(2);
            taskStatus.LastSuccessfulRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(3);
            taskStatus.RunCount = 30;
            taskStatus.SuccessCount = 40;
            taskStatus.TaskState = StackHashTaskState.Running;
            taskStatus.TaskType = StackHashTaskType.DebugScriptTask;

            settings.SetTaskStatistics(taskStatus);

            StackHashTaskStatus taskStatus2 = settings.GetTaskStatistics(StackHashTaskType.DebugScriptTask);

            Assert.AreEqual(taskStatus.FailedCount, taskStatus2.FailedCount);
            Assert.AreEqual(taskStatus.LastDurationInSeconds, taskStatus2.LastDurationInSeconds);
            Assert.AreEqual(taskStatus.LastException, taskStatus2.LastException);
            Assert.AreEqual(taskStatus.LastFailedRunTimeUtc, taskStatus2.LastFailedRunTimeUtc);
            Assert.AreEqual(taskStatus.LastStartedTimeUtc, taskStatus2.LastStartedTimeUtc);
            Assert.AreEqual(taskStatus.LastSuccessfulRunTimeUtc, taskStatus2.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(taskStatus.RunCount, taskStatus2.RunCount);
            Assert.AreEqual(taskStatus.SuccessCount, taskStatus2.SuccessCount);
            Assert.AreEqual(taskStatus.TaskState, taskStatus2.TaskState);
            Assert.AreEqual(taskStatus.TaskType, taskStatus2.TaskType);
        }

        [TestMethod]
        public void ErrorIndexXmlSettingsDefaultSetOneTaskDataCheckPersisted()
        {
            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);
            StackHashTaskStatus taskStatus = settings.GetTaskStatistics(StackHashTaskType.DebugScriptTask);

            Assert.AreEqual(0, taskStatus.FailedCount);
            Assert.AreEqual(0, taskStatus.SuccessCount);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);
            Assert.AreEqual(null, taskStatus.LastException);
            Assert.AreEqual(0, taskStatus.LastFailedRunTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastStartedTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);


            taskStatus.FailedCount = 10;
            taskStatus.LastDurationInSeconds = 20;
            taskStatus.LastException = "Test";
            taskStatus.LastFailedRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(1);
            taskStatus.LastStartedTimeUtc = DateTime.Now.ToUniversalTime().AddDays(2);
            taskStatus.LastSuccessfulRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(3);
            taskStatus.RunCount = 30;
            taskStatus.SuccessCount = 40;
            taskStatus.TaskState = StackHashTaskState.Running;
            taskStatus.TaskType = StackHashTaskType.DebugScriptTask;

            settings.SetTaskStatistics(taskStatus);

            settings = new ErrorIndexXmlSettings(settingsFileName);

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);

            StackHashTaskStatus taskStatus2 = settings.GetTaskStatistics(StackHashTaskType.DebugScriptTask);

            Assert.AreEqual(taskStatus.FailedCount, taskStatus2.FailedCount);
            Assert.AreEqual(taskStatus.LastDurationInSeconds, taskStatus2.LastDurationInSeconds);
            Assert.AreEqual(taskStatus.LastException, taskStatus2.LastException);
            Assert.AreEqual(taskStatus.LastFailedRunTimeUtc, taskStatus2.LastFailedRunTimeUtc);
            Assert.AreEqual(taskStatus.LastStartedTimeUtc, taskStatus2.LastStartedTimeUtc);
            Assert.AreEqual(taskStatus.LastSuccessfulRunTimeUtc, taskStatus2.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(taskStatus.RunCount, taskStatus2.RunCount);
            Assert.AreEqual(taskStatus.SuccessCount, taskStatus2.SuccessCount);
            Assert.AreEqual(taskStatus.TaskState, taskStatus2.TaskState);
            Assert.AreEqual(taskStatus.TaskType, taskStatus2.TaskType);
        }
        [TestMethod]
        public void ErrorIndexXmlSettingsDefaultSetTwoTasksDataCheckPersisted()
        {
            string settingsFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\Settings.xml", m_TempPath);

            ErrorIndexXmlSettings settings = new ErrorIndexXmlSettings(settingsFileName);

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);
            StackHashTaskStatus taskStatus = settings.GetTaskStatistics(StackHashTaskType.DebugScriptTask);

            Assert.AreEqual(0, taskStatus.FailedCount);
            Assert.AreEqual(0, taskStatus.SuccessCount);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);
            Assert.AreEqual(null, taskStatus.LastException);
            Assert.AreEqual(0, taskStatus.LastFailedRunTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastStartedTimeUtc.Ticks);
            Assert.AreEqual(0, taskStatus.LastDurationInSeconds);


            taskStatus.FailedCount = 10;
            taskStatus.LastDurationInSeconds = 20;
            taskStatus.LastException = "Test";
            taskStatus.LastFailedRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(1);
            taskStatus.LastStartedTimeUtc = DateTime.Now.ToUniversalTime().AddDays(2);
            taskStatus.LastSuccessfulRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(3);
            taskStatus.RunCount = 30;
            taskStatus.SuccessCount = 40;
            taskStatus.TaskState = StackHashTaskState.Running;
            taskStatus.TaskType = StackHashTaskType.DebugScriptTask;
            taskStatus.ServiceErrorCode = StackHashServiceErrorCode.DebuggerError;

            settings.SetTaskStatistics(taskStatus);

            StackHashTaskStatus taskStatus2 = settings.GetTaskStatistics(StackHashTaskType.PurgeTask);
            taskStatus2.FailedCount = 30;
            taskStatus2.LastDurationInSeconds = 10;
            taskStatus2.LastException = "Test2";
            taskStatus2.LastFailedRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(2);
            taskStatus2.LastStartedTimeUtc = DateTime.Now.ToUniversalTime().AddDays(3);
            taskStatus2.LastSuccessfulRunTimeUtc = DateTime.Now.ToUniversalTime().AddDays(4);
            taskStatus2.RunCount = 50;
            taskStatus2.SuccessCount = 60;
            taskStatus2.TaskState = StackHashTaskState.Completed;
            taskStatus2.TaskType = StackHashTaskType.PurgeTask;

            settings.SetTaskStatistics(taskStatus2);

            settings = new ErrorIndexXmlSettings(settingsFileName);

            Assert.AreEqual(settingsFileName, settings.FileName);
            Assert.AreNotEqual(null, settings.LastSyncTimes);
            Assert.AreEqual(0, settings.LastSyncTimes.Count);

            StackHashTaskStatus taskStatus3 = settings.GetTaskStatistics(StackHashTaskType.DebugScriptTask);

            Assert.AreEqual(taskStatus.FailedCount, taskStatus3.FailedCount);
            Assert.AreEqual(taskStatus.LastDurationInSeconds, taskStatus3.LastDurationInSeconds);
            Assert.AreEqual(taskStatus.LastException, taskStatus3.LastException);
            Assert.AreEqual(taskStatus.LastFailedRunTimeUtc, taskStatus3.LastFailedRunTimeUtc);
            Assert.AreEqual(taskStatus.LastStartedTimeUtc, taskStatus3.LastStartedTimeUtc);
            Assert.AreEqual(taskStatus.LastSuccessfulRunTimeUtc, taskStatus3.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(taskStatus.RunCount, taskStatus3.RunCount);
            Assert.AreEqual(taskStatus.SuccessCount, taskStatus3.SuccessCount);
            Assert.AreEqual(taskStatus.TaskState, taskStatus3.TaskState);
            Assert.AreEqual(taskStatus.TaskType, taskStatus3.TaskType);
            Assert.AreEqual(taskStatus.ServiceErrorCode, StackHashServiceErrorCode.DebuggerError);

            StackHashTaskStatus taskStatus4 = settings.GetTaskStatistics(StackHashTaskType.PurgeTask);

            Assert.AreEqual(taskStatus2.FailedCount, taskStatus4.FailedCount);
            Assert.AreEqual(taskStatus2.LastDurationInSeconds, taskStatus4.LastDurationInSeconds);
            Assert.AreEqual(taskStatus2.LastException, taskStatus4.LastException);
            Assert.AreEqual(taskStatus2.LastFailedRunTimeUtc, taskStatus4.LastFailedRunTimeUtc);
            Assert.AreEqual(taskStatus2.LastStartedTimeUtc, taskStatus4.LastStartedTimeUtc);
            Assert.AreEqual(taskStatus2.LastSuccessfulRunTimeUtc, taskStatus4.LastSuccessfulRunTimeUtc);
            Assert.AreEqual(taskStatus2.RunCount, taskStatus4.RunCount);
            Assert.AreEqual(taskStatus2.SuccessCount, taskStatus4.SuccessCount);
            Assert.AreEqual(taskStatus2.TaskState, taskStatus4.TaskState);
            Assert.AreEqual(taskStatus2.TaskType, taskStatus4.TaskType);
        }
    }
}
