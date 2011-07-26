using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;

using StackHashTasks;
using StackHashUtilities;
using StackHashTestBugTrackerPlugIn;
using StackHashBugTrackerInterfaceV1;
using StackHashBusinessObjects;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for BugTrackerManagerUnitTest
    /// </summary>
    [TestClass]
    public class BugTrackerManagerUnitTest
    {
        String m_TestFolder;
        int m_CallBackCount;

        public BugTrackerManagerUnitTest()
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
            m_TestFolder = Path.Combine(Path.GetTempPath(), "StackHashBugTrackerTestFolder");
            tidyTest();

            if (!Directory.Exists(m_TestFolder))
                Directory.CreateDirectory(m_TestFolder);
        }

        private void tidyTest()
        {
            if (Directory.Exists(m_TestFolder))
                PathUtils.DeleteDirectory(m_TestFolder, false);
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
        /// Create the bug tracker manager with invalid plugin folder.
        /// Should just log the error but still create ok.
        /// </summary>
        [TestMethod]
        public void InitializeFailInvalidFolder()
        {
            String [] folders = {"c:\\thisfolderdoesntexist"};

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(0, manager.NumberOfPlugIns);
        }

        /// <summary>
        /// Create the bug tracker manager with valid folder - no DLLs.
        /// </summary>
        [TestMethod]
        public void InitializeFailNoPlugIns()
        {
            String[] folders = {m_TestFolder};

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(0, manager.NumberOfPlugIns);
        }

        /// <summary>
        /// Create the bug tracker manager with valid folder and 1 plug-in.
        /// </summary>
        [TestMethod]
        public void InitializeOkOnePlugIn()
        {
            String testPlugInFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { testPlugInFolder };

            BugTrackerManager manager = new BugTrackerManager(folders);

            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);
        }

        /// <summary>
        /// Invoke each method in turn.
        /// </summary>
        [TestMethod]
        public void InvokeEachMethod()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);

            StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
            plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            plugInSettings.PlugInSettings[0].Enabled = true;

            BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

            BugTrackerProduct product = new BugTrackerProduct("Name", "Version", 1);
            BugTrackerFile file = new BugTrackerFile("FileName", "FileVersion", 2);
            BugTrackerEvent theEvent = new BugTrackerEvent("BugRef", "PlugInBugRef", 1, "EventTypeName", 1, new NameValueCollection());
            BugTrackerCab cab = new BugTrackerCab(1, 100, false, false, new NameValueCollection(), "c:\\test.cab");

            plugInContext.ProductAdded(null, BugTrackerReportType.Automatic, product);
            plugInContext.ProductUpdated(null, BugTrackerReportType.Automatic, product);

            plugInContext.FileAdded(null, BugTrackerReportType.Automatic, product, file);
            plugInContext.FileUpdated(null, BugTrackerReportType.Automatic, product, file);

            plugInContext.EventAdded(null, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventUpdated(null, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventNoteAdded(null, BugTrackerReportType.Automatic, product, file, theEvent, new BugTrackerNote());

            plugInContext.CabAdded(null, BugTrackerReportType.Automatic, product, file, theEvent, cab);
            plugInContext.CabUpdated(null, BugTrackerReportType.Automatic, product, file, theEvent, cab);
            plugInContext.CabNoteAdded(null, BugTrackerReportType.Automatic, product, file, theEvent, cab, new BugTrackerNote());


            fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("1", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("1", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["FileAddedCount"]);
            Assert.AreEqual("1", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["EventAddedCount"]);
            Assert.AreEqual("1", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("1", diagnostics["CabAddedCount"]);
            Assert.AreEqual("1", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["CabNoteAddedCount"]);

        }

        /// <summary>
        /// Get context settings.
        /// </summary>
        [TestMethod]
        public void GetContextSettings()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);

            StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
            plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            plugInSettings.PlugInSettings[0].Enabled = true;

            BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);

            Assert.AreEqual("http://www.stackhash.com/", fullDiagnostics[0].HelpUrl.ToString());
            Assert.AreEqual(true, fullDiagnostics[0].Loaded);
            Assert.AreEqual("Plug-in used to control StackHash unit testing.", fullDiagnostics[0].PlugInDescription);
            Assert.AreEqual(true, fullDiagnostics[0].PlugInSetsBugReference);

            NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);
        }

        /// <summary>
        /// Invoke each method in turn - should not report to the plugin because we have specified a plugin list
        /// that doesn't include the TestPlugIn.
        /// </summary>
        [TestMethod]
        public void InvokeEachMethodSpecifyNoPlugIn()
        {
            // Empty so shouldn't report anything.
            StackHashBugTrackerPlugInSelectionCollection plugIns = new StackHashBugTrackerPlugInSelectionCollection();

            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);

            StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
            plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            plugInSettings.PlugInSettings[0].Enabled = true;

            BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

            BugTrackerProduct product = new BugTrackerProduct("Name", "Version", 1);
            BugTrackerFile file = new BugTrackerFile("FileName", "FileVersion", 2);
            BugTrackerEvent theEvent = new BugTrackerEvent("BugRef", "PlugInBugRef", 1, "EventTypeName", 1, new NameValueCollection());
            BugTrackerCab cab = new BugTrackerCab(1, 100, false, false, new NameValueCollection(), "c:\\test.cab");

            plugInContext.ProductAdded(plugIns, BugTrackerReportType.Automatic, product);
            plugInContext.ProductUpdated(plugIns, BugTrackerReportType.Automatic, product);

            plugInContext.FileAdded(plugIns, BugTrackerReportType.Automatic, product, file);
            plugInContext.FileUpdated(plugIns, BugTrackerReportType.Automatic, product, file);

            plugInContext.EventAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventUpdated(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventManualUpdateCompleted(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventNoteAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, new BugTrackerNote());

            plugInContext.CabAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab);
            plugInContext.CabUpdated(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab);
            plugInContext.CabNoteAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab, new BugTrackerNote());


            fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventCompleteCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);
        }

        /// <summary>
        /// Invoke each method in turn - should report to the plugin because we have specified a plugin list
        /// that does include the TestPlugIn.
        /// </summary>
        [TestMethod]
        public void InvokeEachMethodSpecifyCorrectPlugIn()
        {
            StackHashBugTrackerPlugInSelectionCollection plugIns = new StackHashBugTrackerPlugInSelectionCollection();
            plugIns.Add(new StackHashBugTrackerPlugInSelection("TestPlugIn"));


            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);

            StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
            plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            plugInSettings.PlugInSettings[0].Enabled = true;

            BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

            BugTrackerProduct product = new BugTrackerProduct("Name", "Version", 1);
            BugTrackerFile file = new BugTrackerFile("FileName", "FileVersion", 2);
            BugTrackerEvent theEvent = new BugTrackerEvent("BugRef", "PlugInBugRef", 1, "EventTypeName", 1, new NameValueCollection());
            BugTrackerCab cab = new BugTrackerCab(1, 100, false, false, new NameValueCollection(), "c:\\test.cab");

            plugInContext.ProductAdded(plugIns, BugTrackerReportType.Automatic, product);
            plugInContext.ProductUpdated(plugIns, BugTrackerReportType.Automatic, product);

            plugInContext.FileAdded(plugIns, BugTrackerReportType.Automatic, product, file);
            plugInContext.FileUpdated(plugIns, BugTrackerReportType.Automatic, product, file);

            plugInContext.EventAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventUpdated(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventManualUpdateCompleted(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
            plugInContext.EventNoteAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, new BugTrackerNote());

            plugInContext.CabAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab);
            plugInContext.CabUpdated(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab);
            plugInContext.CabNoteAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab, new BugTrackerNote());


            fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("1", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("1", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["FileAddedCount"]);
            Assert.AreEqual("1", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["EventAddedCount"]);
            Assert.AreEqual("1", diagnostics["EventCompleteCount"]);
            Assert.AreEqual("1", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("1", diagnostics["CabAddedCount"]);
            Assert.AreEqual("1", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("1", diagnostics["CabNoteAddedCount"]);
        }

        
        /// <summary>
        /// Exceptions in calls to the interface should be absorbed by the Plugin manager.
        /// </summary>
        [TestMethod]
        public void ProductAddedException()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);
            
            StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
            plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            plugInSettings.PlugInSettings[0].Enabled = true;

            BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

            StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);


            NameValueCollection properties = new NameValueCollection();
            properties.Add("ProductAddedException", "1");

            plugInContext.SetProperties("TestPlugIn", properties);

            plugInContext.ProductAdded(null, BugTrackerReportType.Automatic, null);

            fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("1", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

        }

        /// <summary>
        /// Exceptions in calls to the interface should be absorbed by the Plugin manager.
        /// This one is an unhandled exception in the app domain.
        /// 
        /// *** Fails because unhandled exceptions in AppDomains bring down the whole process
        ///     This is apparently by design in .NET 2.0+
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ProductAddedUnhandledExceptionInPlugIn()
        {
            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerManager manager = new BugTrackerManager(folders);
            Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);

            StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
            plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
            plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            plugInSettings.PlugInSettings[0].Enabled = true;

            BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

            StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);


            NameValueCollection properties = new NameValueCollection();
            properties.Add("UnhandledException", "1");

            plugInContext.SetProperties("TestPlugIn", properties);

            plugInContext.ProductAdded(null, BugTrackerReportType.Automatic, null);

            fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
            Assert.AreEqual(1, fullDiagnostics.Count);
            diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

            Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
            Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["FileAddedCount"]);
            Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventAddedCount"]);
            Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabAddedCount"]);
            Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
            Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);
        }

        /// <summary>
        /// Logs a message to the event log from a plugin.
        /// </summary>
        /// <param name="source">Caller - will be null.</param>
        /// <param name="e">The message to log.</param>
        public void LogPlugInEvent(Object source, BugTrackerTraceEventArgs e)
        {
            m_CallBackCount++;

            Assert.AreNotEqual(null, e);
            Assert.AreNotEqual(null, e.Message);
        }

        
        /// <summary>
        /// Log-in callback from plugin.
        /// </summary>
        [TestMethod]
        public void LogInCallback()
        {
            // Empty so shouldn't report anything.
            StackHashBugTrackerPlugInSelectionCollection plugIns = new StackHashBugTrackerPlugInSelectionCollection();

            String dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String[] folders = { dllLocation };

            BugTrackerTrace.LogMessageHook += new EventHandler<BugTrackerTraceEventArgs>(LogPlugInEvent);

            try
            {
                BugTrackerManager manager = new BugTrackerManager(folders);

                StackHashBugTrackerPlugInSettings plugInSettings = new StackHashBugTrackerPlugInSettings();
                plugInSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
                plugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
                plugInSettings.PlugInSettings[0].Name = "TestPlugIn";
                plugInSettings.PlugInSettings[0].Enabled = true;

                BugTrackerContext plugInContext = new BugTrackerContext(manager, plugInSettings);

                Assert.AreEqual(true, manager.NumberOfPlugIns >= 1);
                Assert.AreEqual(true, m_CallBackCount > 0);

                StackHashBugTrackerPlugInDiagnosticsCollection fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                NameValueCollection diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);

                BugTrackerProduct product = new BugTrackerProduct("Name", "Version", 1);
                BugTrackerFile file = new BugTrackerFile("FileName", "FileVersion", 2);
                BugTrackerEvent theEvent = new BugTrackerEvent("BugRef", "PlugInBugRef", 1, "EventTypeName", 1, new NameValueCollection());
                BugTrackerCab cab = new BugTrackerCab(1, 100, false, false, new NameValueCollection(), "c:\\test.cab");

                plugInContext.ProductAdded(plugIns, BugTrackerReportType.Automatic, product);
                plugInContext.ProductUpdated(plugIns, BugTrackerReportType.Automatic, product);

                plugInContext.FileAdded(plugIns, BugTrackerReportType.Automatic, product, file);
                plugInContext.FileUpdated(plugIns, BugTrackerReportType.Automatic, product, file);

                plugInContext.EventAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
                plugInContext.EventUpdated(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
                plugInContext.EventManualUpdateCompleted(plugIns, BugTrackerReportType.Automatic, product, file, theEvent);
                plugInContext.EventNoteAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, new BugTrackerNote());

                plugInContext.CabAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab);
                plugInContext.CabUpdated(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab);
                plugInContext.CabNoteAdded(plugIns, BugTrackerReportType.Automatic, product, file, theEvent, cab, new BugTrackerNote());


                fullDiagnostics = plugInContext.GetContextDiagnostics("TestPlugIn");
                Assert.AreEqual(1, fullDiagnostics.Count);
                diagnostics = fullDiagnostics[0].Diagnostics.ToNameValueCollection();

                Assert.AreEqual("0", diagnostics["ProductAddedCount"]);
                Assert.AreEqual("0", diagnostics["ProductUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["FileAddedCount"]);
                Assert.AreEqual("0", diagnostics["FileUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventAddedCount"]);
                Assert.AreEqual("0", diagnostics["EventCompleteCount"]);
                Assert.AreEqual("0", diagnostics["EventUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["EventNoteAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabAddedCount"]);
                Assert.AreEqual("0", diagnostics["CabUpdatedCount"]);
                Assert.AreEqual("0", diagnostics["CabNoteAddedCount"]);
            }
            finally
            {
                BugTrackerTrace.LogMessageHook -= new EventHandler<BugTrackerTraceEventArgs>(LogPlugInEvent);
            }
        }
    
    }
}
