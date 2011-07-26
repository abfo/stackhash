using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Threading;
using System.IO;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Tests running the bug report task in the service.
    /// </summary>
    [TestClass]
    public class BugReportTaskUnitTests
    {
        Utils m_Utils;

        public BugReportTaskUnitTests()
        {
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

            m_Utils.SetProxy(null);
            m_Utils.RemoveAllContexts();
            m_Utils.RemoveAllScripts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.Dispose();
                m_Utils = null;
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
        /// Run a report on a test index - specific event.
        /// </summary>
        [TestMethod]
        public void RunBugReportSpecificEvent()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            m_Utils.RestartService();
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 1;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
                bugReportDataCollection.Add(new StackHashBugReportData()
                {
                    Product = products[0].Product,
                    File = files[0], 
                    TheEvent = events[0].EventData, 
                    Cab = null, 
                    ScriptName = null, 
                    Options = StackHashReportOptions.IncludeAllObjects
                });
            
                m_Utils.RunBugReportTask(0, bugReportDataCollection, 30000, true);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run a report on a test index - whole index.
        /// </summary>
        [TestMethod]
        public void RunBugReportOnFullIndex()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            m_Utils.ActivateContext(0);
            
            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 2;
            indexData.NumberOfFiles = 3;
            indexData.NumberOfEvents = 4;
            indexData.NumberOfCabs = 5;
            indexData.NumberOfEventInfos = 6;
            indexData.NumberOfCabNotes = 7;
            indexData.NumberOfEventNotes = 8;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
                bugReportDataCollection.Add(new StackHashBugReportData()
                {
                    Product = null,
                    File = null,
                    TheEvent = null,
                    Cab = null,
                    ScriptName = null,
                    Options = StackHashReportOptions.IncludeAllObjects
                });

                m_Utils.RunBugReportTask(0, bugReportDataCollection, 1000 * 60, true);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run a report on a test index - specific product - check progress reports.
        /// </summary>
        [TestMethod]
        public void RunBugReportSpecificFileProgressCheck()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            m_Utils.RestartService();
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 2;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
                bugReportDataCollection.Add(new StackHashBugReportData()
                {
                    Product = products[0].Product,
                    File = files[0],
                    TheEvent = null,
                    Cab = null,
                    ScriptName = null,
                    Options = StackHashReportOptions.IncludeAllObjects
                });

                m_Utils.RunBugReportTask(0, bugReportDataCollection, 30000, true);

                // Check the progress reports.
                Assert.AreEqual(2, m_Utils.BugReportProgressReports.Count);

                StackHashBugReportProgressAdminReport progress = m_Utils.BugReportProgressReports[0] as StackHashBugReportProgressAdminReport;

                Assert.AreEqual(1, progress.CurrentEvent);
                Assert.AreEqual(2, progress.TotalEvents);

                progress = m_Utils.BugReportProgressReports[1] as StackHashBugReportProgressAdminReport;

                Assert.AreEqual(2, progress.CurrentEvent);
                Assert.AreEqual(2, progress.TotalEvents);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }
        /// <summary>
        /// Run a report on a test index - specific product - check progress reports.
        /// 200 events so should only report <= 100 times.
        /// </summary>
        [TestMethod]
        public void RunBugReportSpecificFileProgressCheckManyEvents()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            m_Utils.RestartService();
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 200;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
                bugReportDataCollection.Add(new StackHashBugReportData()
                {
                    Product = products[0].Product,
                    File = files[0],
                    TheEvent = null,
                    Cab = null,
                    ScriptName = null,
                    Options = StackHashReportOptions.IncludeAllObjects
                });

                m_Utils.RunBugReportTask(0, bugReportDataCollection, 30000, true);

                // Check the progress reports.
                Assert.AreEqual(true, m_Utils.BugReportProgressReports.Count <= 100);

                long lastProgress = -1;
                foreach (StackHashAdminReport report in m_Utils.BugReportProgressReports)
                {
                    StackHashBugReportProgressAdminReport progress = report as StackHashBugReportProgressAdminReport;

                    Assert.AreEqual(true, progress.CurrentEvent > lastProgress);
                    Assert.AreEqual(true, progress.CurrentEvent <= progress.TotalEvents);
                    lastProgress = progress.CurrentEvent;
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }


        /// <summary>
        /// Run a report on a test index - specific product - check progress reports.
        /// 200 events so should only report <= 100 times.
        /// </summary>
        [TestMethod]
        public void RunBugReportSpecificFileProgressCheckManyEventsAbortTest()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            m_Utils.RestartService();
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 200;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                StackHashBugReportDataCollection bugReportDataCollection = new StackHashBugReportDataCollection();
                bugReportDataCollection.Add(new StackHashBugReportData()
                {
                    Product = products[0].Product,
                    File = files[0],
                    TheEvent = null,
                    Cab = null,
                    ScriptName = null,
                    Options = StackHashReportOptions.IncludeAllObjects
                });

                m_Utils.RunBugReportTask(0, bugReportDataCollection, 0, false); // Don't wait.
                m_Utils.WaitForBugReportProgress(30000);
                m_Utils.AbortTask(0, StackHashTaskType.BugReportTask);
                m_Utils.WaitForBugReportTaskCompleted(30000);

                // Check the progress reports.
                Assert.AreEqual(true, m_Utils.BugReportProgressReports.Count < 100);

                long lastProgress = -1;
                foreach (StackHashAdminReport report in m_Utils.BugReportProgressReports)
                {
                    StackHashBugReportProgressAdminReport progress = report as StackHashBugReportProgressAdminReport;

                    Assert.AreEqual(true, progress.CurrentEvent > lastProgress);
                    Assert.AreEqual(true, progress.CurrentEvent < progress.TotalEvents);
                    lastProgress = progress.CurrentEvent;
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }
    }
}
