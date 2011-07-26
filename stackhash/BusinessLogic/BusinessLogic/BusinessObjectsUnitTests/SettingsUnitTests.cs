using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SettingsUnitTests
    {
        string m_TempPath;

        public SettingsUnitTests()
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
            m_TempPath = Path.GetTempPath() + "StackHashTestSettings";

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
        public void SaveLoadSettings()
        {
            StackHashSettings settings = new StackHashSettings();

            settings.ContextCollection = new StackHashContextCollection();

            settings.ProxySettings = new StackHashProxySettings(true, true, "host", 9000, "UserName", "Password", "Domain");

            StackHashContextSettings context1 = new StackHashContextSettings();
            context1.CabFilePurgeSchedule = new ScheduleCollection();
            context1.CabFilePurgeSchedule.Add(new Schedule());
            context1.CabFilePurgeSchedule[0].DaysOfWeek = DaysOfWeek.Monday;
            context1.CabFilePurgeSchedule[0].Period = SchedulePeriod.Hourly;
            context1.CabFilePurgeSchedule[0].Time = new ScheduleTime(1, 2, 3);

            context1.ErrorIndexSettings = new ErrorIndexSettings();
            context1.ErrorIndexSettings.Folder = "C:\\test1";
            context1.ErrorIndexSettings.Name = "Name1";
            context1.ErrorIndexSettings.Type = ErrorIndexType.Xml;

            context1.Id = 1;
            StackHashProductSyncDataCollection productsToSync = new StackHashProductSyncDataCollection();
            productsToSync.Add(new StackHashProductSyncData(23));

            context1.WinQualSettings = new WinQualSettings("username", "password", "cucku", 90, productsToSync, true, 30 * 60, 1, 
                WinQualSettings.DefaultSyncsBeforeResync, true);
            context1.WinQualSyncSchedule = new ScheduleCollection();
            context1.WinQualSyncSchedule.Add(new Schedule());
            context1.WinQualSyncSchedule[0].DaysOfWeek = DaysOfWeek.Tuesday;
            context1.WinQualSyncSchedule[0].Period = SchedulePeriod.Weekly;
            context1.WinQualSyncSchedule[0].Time = new ScheduleTime(4, 5, 6);

            context1.DebuggerSettings = new StackHashDebuggerSettings();
            context1.DebuggerSettings.BinaryPath = new StackHashSearchPath();
            context1.DebuggerSettings.BinaryPath.Add("C:\\Binary");
            context1.DebuggerSettings.BinaryPath.Add("C:\\Path");

            context1.DebuggerSettings.SymbolPath = new StackHashSearchPath();
            context1.DebuggerSettings.SymbolPath.Add("C:\\Symbol");
            context1.DebuggerSettings.SymbolPath.Add("C:\\Path2");

            context1.DebuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            context1.DebuggerSettings.BinaryPath64Bit.Add("C:\\Binary64");
            context1.DebuggerSettings.BinaryPath64Bit.Add("C:\\Path64");

            context1.DebuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            context1.DebuggerSettings.SymbolPath64Bit.Add("C:\\Symbol64");
            context1.DebuggerSettings.SymbolPath64Bit.Add("C:\\Path264");

            context1.DebuggerSettings.DebuggerPathAndFileName = "C:\\debugger64.exe";
            context1.DebuggerSettings.DebuggerPathAndFileName64Bit = "C:\\debugger64.exe";

            context1.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();


            context1.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
            context1.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            context1.BugTrackerSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            context1.BugTrackerSettings.PlugInSettings[0].Enabled = true;
            context1.BugTrackerSettings.PlugInSettings[0].Name = "TestPlugIn";
            context1.BugTrackerSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            context1.BugTrackerSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair("Name1", "Value1"));
            context1.BugTrackerSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair("Longer name with spaces", "Longer text la la la la la "));
            context1.BugTrackerSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair("$pec1al &^%$&\"£! mbols", "Hehhijugiuyhg*(&^%*&^%"));
 
            StackHashPurgeOptions purgeOptions = new StackHashPurgeOptions();
            purgeOptions.Id = 22;
            purgeOptions.AgeToPurge = 90;
            purgeOptions.PurgeCabFiles = true;
            purgeOptions.PurgeDumpFiles = true;
            purgeOptions.PurgeObject = StackHashPurgeObject.PurgeFile;
            context1.PurgeOptionsCollection.Add(purgeOptions);

            settings.ContextCollection.Add(context1);

            // Now save the settings to an XML file.
            string tempFile = m_TempPath + "\\testsettings.xml";

            StackHashSettings.Save(settings, tempFile);

            // Now load the new settings and compare them.
            StackHashSettings loadedSettings = StackHashSettings.Load(tempFile);

            Assert.AreEqual(settings.ProxySettings.UseProxy, loadedSettings.ProxySettings.UseProxy);
            Assert.AreEqual(settings.ProxySettings.UseProxyAuthentication, loadedSettings.ProxySettings.UseProxyAuthentication);
            Assert.AreEqual(settings.ProxySettings.ProxyHost, loadedSettings.ProxySettings.ProxyHost);
            Assert.AreEqual(settings.ProxySettings.ProxyPort, loadedSettings.ProxySettings.ProxyPort);
            Assert.AreEqual(settings.ProxySettings.ProxyDomain, loadedSettings.ProxySettings.ProxyDomain);
            Assert.AreEqual(settings.ProxySettings.ProxyUserName, loadedSettings.ProxySettings.ProxyUserName);
            Assert.AreEqual(settings.ProxySettings.ProxyPassword, loadedSettings.ProxySettings.ProxyPassword);


            Assert.AreEqual(settings.ContextCollection.Count, loadedSettings.ContextCollection.Count);

            for (int i = 0; i < settings.ContextCollection.Count; i++)
            {
                StackHashContextSettings contextOriginal = settings.ContextCollection[i];
                StackHashContextSettings contextLoaded = loadedSettings.ContextCollection[i];

                Assert.AreEqual(contextOriginal.CabFilePurgeSchedule.Count, contextLoaded.CabFilePurgeSchedule.Count);

                for (int j = 0; j < contextOriginal.CabFilePurgeSchedule.Count; j++)
                {
                    Schedule scheduleOriginal = settings.ContextCollection[i].CabFilePurgeSchedule[0];
                    Schedule scheduleLoaded = loadedSettings.ContextCollection[i].CabFilePurgeSchedule[0];

                    Assert.AreEqual(scheduleOriginal.DaysOfWeek, scheduleLoaded.DaysOfWeek);
                    Assert.AreEqual(scheduleOriginal.Period, scheduleLoaded.Period);
                    Assert.AreEqual(scheduleOriginal.Time.Hour, scheduleLoaded.Time.Hour);
                    Assert.AreEqual(scheduleOriginal.Time.Minute, scheduleLoaded.Time.Minute);
                    Assert.AreEqual(scheduleOriginal.Time.Second, scheduleLoaded.Time.Second);
                }

                Assert.AreEqual(contextOriginal.ErrorIndexSettings.Folder, contextLoaded.ErrorIndexSettings.Folder);
                Assert.AreEqual(contextOriginal.ErrorIndexSettings.Name, contextLoaded.ErrorIndexSettings.Name);
                Assert.AreEqual(contextOriginal.ErrorIndexSettings.Type, contextLoaded.ErrorIndexSettings.Type);

                Assert.AreEqual(contextOriginal.Id, contextLoaded.Id);

                Assert.AreEqual(contextOriginal.WinQualSettings.CompanyName, contextLoaded.WinQualSettings.CompanyName);                
                Assert.AreEqual(contextOriginal.WinQualSettings.Password, contextLoaded.WinQualSettings.Password);
                Assert.AreEqual(contextOriginal.WinQualSettings.UserName, contextLoaded.WinQualSettings.UserName);
                Assert.AreEqual(1, contextOriginal.WinQualSettings.ProductsToSynchronize.Count);
                Assert.AreEqual(23, contextOriginal.WinQualSettings.ProductsToSynchronize[0].ProductId);
                Assert.AreEqual(true, contextOriginal.WinQualSettings.EnableNewProductsAutomatically);

                Assert.AreEqual(contextOriginal.WinQualSyncSchedule.Count, contextLoaded.WinQualSyncSchedule.Count);

                for (int j = 0; j <  contextOriginal.CabFilePurgeSchedule.Count; j++)
                {
                    Schedule scheduleOriginal = settings.ContextCollection[i].CabFilePurgeSchedule[0];
                    Schedule scheduleLoaded = loadedSettings.ContextCollection[i].CabFilePurgeSchedule[0];

                    Assert.AreEqual(scheduleOriginal.DaysOfWeek, scheduleLoaded.DaysOfWeek);
                    Assert.AreEqual(scheduleOriginal.Period, scheduleLoaded.Period);
                    Assert.AreEqual(scheduleOriginal.Time.Hour, scheduleLoaded.Time.Hour);
                    Assert.AreEqual(scheduleOriginal.Time.Minute, scheduleLoaded.Time.Minute);
                    Assert.AreEqual(scheduleOriginal.Time.Second, scheduleLoaded.Time.Second);
                }


                Assert.AreEqual(contextOriginal.DebuggerSettings.DebuggerPathAndFileName, contextLoaded.DebuggerSettings.DebuggerPathAndFileName);
                Assert.AreEqual(contextOriginal.DebuggerSettings.BinaryPath[0], contextLoaded.DebuggerSettings.BinaryPath[0]);
                Assert.AreEqual(contextOriginal.DebuggerSettings.BinaryPath[1], contextLoaded.DebuggerSettings.BinaryPath[1]);
                Assert.AreEqual(contextOriginal.DebuggerSettings.SymbolPath[0], contextLoaded.DebuggerSettings.SymbolPath[0]);
                Assert.AreEqual(contextOriginal.DebuggerSettings.SymbolPath[1], contextLoaded.DebuggerSettings.SymbolPath[1]);

                Assert.AreEqual(contextOriginal.DebuggerSettings.DebuggerPathAndFileName64Bit, contextLoaded.DebuggerSettings.DebuggerPathAndFileName64Bit);
                Assert.AreEqual(contextOriginal.DebuggerSettings.BinaryPath64Bit[0], contextLoaded.DebuggerSettings.BinaryPath64Bit[0]);
                Assert.AreEqual(contextOriginal.DebuggerSettings.BinaryPath64Bit[1], contextLoaded.DebuggerSettings.BinaryPath64Bit[1]);
                Assert.AreEqual(contextOriginal.DebuggerSettings.SymbolPath64Bit[0], contextLoaded.DebuggerSettings.SymbolPath64Bit[0]);
                Assert.AreEqual(contextOriginal.DebuggerSettings.SymbolPath64Bit[1], contextLoaded.DebuggerSettings.SymbolPath64Bit[1]);


                Assert.AreEqual(contextOriginal.PurgeOptionsCollection.Count, contextLoaded.PurgeOptionsCollection.Count);
                int purgeOptionIndex = 0;
                foreach (StackHashPurgeOptions oldPurgeOptions in contextOriginal.PurgeOptionsCollection)
                {
                    StackHashPurgeOptions loadedPurgeOptions = contextLoaded.PurgeOptionsCollection[purgeOptionIndex++];

                    Assert.AreEqual(oldPurgeOptions.Id, loadedPurgeOptions.Id);
                    Assert.AreEqual(oldPurgeOptions.AgeToPurge, loadedPurgeOptions.AgeToPurge);
                    Assert.AreEqual(oldPurgeOptions.PurgeCabFiles, loadedPurgeOptions.PurgeCabFiles);
                    Assert.AreEqual(oldPurgeOptions.PurgeDumpFiles, loadedPurgeOptions.PurgeDumpFiles);
                    Assert.AreEqual(oldPurgeOptions.PurgeObject, loadedPurgeOptions.PurgeObject);
                }


                if (contextOriginal.BugTrackerSettings != null)
                {
                    for (int j = 0; j < contextOriginal.BugTrackerSettings.PlugInSettings.Count; j++)
                    {
                        StackHashBugTrackerPlugIn bugTrackerSettings = contextOriginal.BugTrackerSettings.PlugInSettings[j];
                        Assert.AreEqual(contextOriginal.BugTrackerSettings.PlugInSettings[j].Enabled, bugTrackerSettings.Enabled);


                        for (int k = 0; k < bugTrackerSettings.Properties.Count; k++)
                        {
                            Assert.AreEqual(contextOriginal.BugTrackerSettings.PlugInSettings[j].Properties[k],
                                bugTrackerSettings.Properties[k]);
                        }
                    }
                }
            }
        }
    }
}
