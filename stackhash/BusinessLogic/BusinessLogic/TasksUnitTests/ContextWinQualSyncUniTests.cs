using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashDebug;
using StackHashUtilities;


namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for ContextWinQualSyncUniTests
    /// </summary>
    [TestClass]
    public class ContextWinQualSyncUniTests
    {
        String m_TempPath;
        String m_ScriptPath;
        AutoResetEvent m_WinQualSyncEvent;
        int m_SyncCount;
        AutoResetEvent m_AnalyzeEvent;
        int m_AnalyzeCount;
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_TestServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";

        public ContextWinQualSyncUniTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashTaskTesting";
            m_ScriptPath = m_TempPath + "\\Scripts";
            m_SyncCount = 0;
            m_AnalyzeCount = 0;
            m_WinQualSyncEvent = new AutoResetEvent(false);
            m_AnalyzeEvent = new AutoResetEvent(false);

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
            if (!Directory.Exists(m_ScriptPath))
                Directory.CreateDirectory(m_ScriptPath);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath))
            {
                PathUtils.MarkDirectoryWritable(m_TempPath, true);
                PathUtils.DeleteDirectory(m_TempPath, true);
            }
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
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs args = e as AdminReportEventArgs;

            if (args.Report.Operation == StackHashAdminOperation.WinQualSyncCompleted)
            {
                m_SyncCount++;
                m_WinQualSyncEvent.Set();
            }
            else if (args.Report.Operation == StackHashAdminOperation.AnalyzeCompleted)
            {
                m_AnalyzeCount++;
                m_AnalyzeEvent.Set();
            }
        }


        /// <summary>
        /// WinQual sync timer task should trigger the winqual sync + analyze tasks.
        /// Dummy WinQualServices used - valid parameters.
        /// </summary>
        [TestMethod]
        public void WinQualSyncRunsOnTimer()
        {
            string settingsFileName = string.Format("{0}\\ServiceSettings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;

            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.ErrorIndexSettings.Name = "TestIndex";
            settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);


            // Make sure the correct winqual login details are used.
            settings.WinQualSettings.UserName = "JoeBloggs";
            settings.WinQualSettings.Password = "SomeCrappyPassword";
            settings.WinQualSyncSchedule = new ScheduleCollection();
            Schedule schedule = new Schedule();
            schedule.DaysOfWeek = Schedule.EveryDay;
            schedule.Period = SchedulePeriod.Daily;

            DateTime now = DateTime.Now; // Must use local time.
            DateTime syncTime = now.AddSeconds(5);

            schedule.Time = new ScheduleTime(syncTime.Hour, syncTime.Minute, syncTime.Second);
            settings.WinQualSyncSchedule.Add(schedule);


            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);
            context.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            try
            {
                context.Activate();

                Assert.AreEqual(true, m_WinQualSyncEvent.WaitOne(8000));
                Assert.AreEqual(true, m_AnalyzeEvent.WaitOne(5000));

                Assert.AreEqual(1, m_SyncCount);
                Assert.AreEqual(1, m_AnalyzeCount);

                context.Deactivate();
                Assert.AreEqual(false, context.IsActive);
                Assert.AreEqual(null, context.WinQualSyncTimerTask);
            }
            finally
            {
                context.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                context.Dispose();
            }
        }


        /// <summary>
        /// Context creation exception - invalid sync schedule
        /// </summary>
        [TestMethod]
        public void CheckWinQualSyncParametersInvalidWinQualSchedule()
        {
            try
            {
                string settingsFileName = string.Format("{0}\\ServiceSettings.xml", m_TempPath);
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                settingsManager.ServiceGuid = s_TestServiceGuid;
                StackHashContextSettings settings = settingsManager.CreateNewContextSettings();
                settings.ErrorIndexSettings.Folder = m_TempPath;
                settings.ErrorIndexSettings.Name = "TestIndex";
                settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;

                ScriptManager scriptManager = new ScriptManager(m_TempPath);
                IDebugger debugger = new Windbg();

                // Make sure the correct winqual login details are used.
                settings.WinQualSettings.UserName = "JoeBloggs";
                settings.WinQualSettings.Password = "SomeCrappyPassword";
                settings.WinQualSyncSchedule = new ScheduleCollection();
                Schedule schedule = new Schedule();
                // schedule.DaysOfWeek = Schedule.EveryDay; - No Daily specified.
                schedule.Period = SchedulePeriod.Daily;

                DateTime now = DateTime.Now; // Must use local time.
                DateTime syncTime = now.AddSeconds(5);

                schedule.Time = new ScheduleTime(syncTime.Hour, syncTime.Minute, syncTime.Second);
                settings.WinQualSyncSchedule.Add(schedule);

                string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                // Should fail with a param exception.
                // TODO: should the test setting here be false??
                ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, null, licenseManager);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.ScheduleFormatError, ex.ServiceErrorCode);
            }
        }

        [TestMethod]
        public void ChangeParamsWhileActive()
        {
            string settingsFileName = string.Format("{0}\\ServiceSettings.xml", m_TempPath);
            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            settingsManager.ServiceGuid = s_TestServiceGuid;

            StackHashContextSettings settings = settingsManager.CreateNewContextSettings();
            settings.ErrorIndexSettings.Folder = m_TempPath;
            settings.ErrorIndexSettings.Name = "TestIndex";
            settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;

            ScriptManager scriptManager = new ScriptManager(m_ScriptPath);
            IDebugger debugger = new Windbg();

            settings.WinQualSettings.UserName = "JoeBloggs";
            settings.WinQualSettings.Password = "SomeCrappyPassword";
            settings.WinQualSyncSchedule = new ScheduleCollection();
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.EveryDay;

            DateTime now = DateTime.Now; // Must get local time.
            DateTime syncTime = now.AddSeconds(100);

            schedule.Time = new ScheduleTime(syncTime.Hour, syncTime.Minute, syncTime.Second);
            settings.WinQualSyncSchedule.Add(schedule);

            string licenseFileName = string.Format("{0}\\License.bin", m_TempPath);
            LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
            licenseManager.SetLicense(s_LicenseId);


            ControllerContext context = new ControllerContext(settings, scriptManager, debugger, settingsManager, true, StackHashTestData.Default, licenseManager);
            context.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

            try
            {
                context.Activate();


                // Create and delete some new context settings - should have no effect.
                StackHashContextSettings settings2 = settingsManager.CreateNewContextSettings();
                settingsManager.RemoveContextSettings(settings2.Id, true);

                // Change the existing active context.
                settings2.Id = 0;
                settings2.ErrorIndexSettings.Folder = m_TempPath;
                settings2.ErrorIndexSettings.Name = "TestIndex";
                settings2.ErrorIndexSettings.Type = ErrorIndexType.Xml;

                settings2.WinQualSettings.UserName = "JoeBloggs2";
                settings2.WinQualSettings.Password = "SomeCrappyPassword2";
                settings2.WinQualSyncSchedule = new ScheduleCollection();
                schedule = new Schedule();
                schedule.Period = SchedulePeriod.Hourly;
                schedule.DaysOfWeek = Schedule.EveryDay;

                now = DateTime.Now;
                syncTime = now.AddSeconds(5);

                schedule.Time = new ScheduleTime(syncTime.Hour, syncTime.Minute, syncTime.Second);
                settings2.WinQualSyncSchedule.Add(schedule);

                context.UpdateSettings(settings2);

                // Wait for the timer to expire.
                Assert.AreEqual(true, m_WinQualSyncEvent.WaitOne(10000));
                Assert.AreEqual(true, m_AnalyzeEvent.WaitOne(10000));

                Assert.AreEqual(1, m_SyncCount);
                Assert.AreEqual(1, m_AnalyzeCount);

                context.Deactivate();
                Assert.AreEqual(false, context.IsActive);
                Assert.AreEqual(null, context.WinQualSyncTimerTask);
            }
            finally
            {
                context.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
                context.Dispose();
            }
        }

    }
}
