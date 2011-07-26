using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;

using StackHashBugTrackerPlugInDemo;
using StackHashBugTrackerInterfaceV1;

namespace StackHashBugTrackerPlugInDemoUnitTests
{
    /// <summary>
    /// Summary description for ContextUnitTests
    /// </summary>
    [TestClass]
    public class ContextUnitTests
    {
        public ContextUnitTests()
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

        public int GetNumberOfActiveProfiles(IBugTrackerV1Control plugInControl)
        {
            NameValueCollection diagnostics = plugInControl.PlugInDiagnostics;
            Assert.AreNotEqual(null, diagnostics);

            Assert.AreEqual(true, diagnostics.AllKeys.Contains("NumberOfActiveContexts"));

            int result = Int32.Parse(diagnostics["NumberOfActiveContexts"]);
            return result;
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
        public void GetContextDefaultSettings()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));


            // Get the default settings.
            NameValueCollection defaultSettings = context.Properties;

            Assert.AreNotEqual(null, defaultSettings);

            Assert.AreEqual(true, defaultSettings.AllKeys.Contains("LogVerbose"));
            Assert.AreEqual("0", defaultSettings["LogVerbose"]);

            Assert.AreEqual(true, defaultSettings.AllKeys.Contains("EnableContext"));
            Assert.AreEqual("1", defaultSettings["EnableContext"]);


            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void GetContextDiagnostics()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));


            NameValueCollection diagnostics = context.ContextDiagnostics;

            Assert.AreNotEqual(null, diagnostics);

            Assert.AreEqual(true, diagnostics.AllKeys.Contains("LastException"));
            Assert.AreEqual("No Error", diagnostics["LastException"]);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }


        [TestMethod]
        public void ProductAdded()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);

            context.ProductAdded(BugTrackerReportType.Automatic, product);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void ProductUpdated()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);

            context.ProductUpdated(BugTrackerReportType.Automatic, product);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void FileAdded()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);

            context.FileAdded(BugTrackerReportType.Automatic, product, file);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void FileUpdated()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);

            context.FileUpdated(BugTrackerReportType.Automatic, product, file);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void EventAddedNoEventSignature()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());

            context.EventAdded(BugTrackerReportType.Automatic, product, file, theEvent);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void EventAddedWithEventSignature()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";

            context.EventAdded(BugTrackerReportType.Automatic, product, file, theEvent);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void EventUpdatedWithEventSignature()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";

            context.EventUpdated(BugTrackerReportType.Automatic, product, file, theEvent);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void EventComplete()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";

            context.EventManualUpdateCompleted(BugTrackerReportType.Automatic, product, file, theEvent);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void EventNoteAdded()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";
            BugTrackerNote note = new BugTrackerNote(DateTime.Now, "Source", "MarkJ", "Hello from me");

            context.EventNoteAdded(BugTrackerReportType.Automatic, product, file, theEvent, note);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void CabAddedNullAnalysis()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";
            BugTrackerCab cab = new BugTrackerCab(12243, 12121, false, true, null, "c:\\test\\my.cab");

            context.CabAdded(BugTrackerReportType.Automatic, product, file, theEvent, cab);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void CabAddedNoAnalysis()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";
            BugTrackerCab cab = new BugTrackerCab(12243, 12121, false, true, new NameValueCollection(), "c:\\test\\my.cab");

            context.CabAdded(BugTrackerReportType.Automatic, product, file, theEvent, cab);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void CabAddedSomeAnalysis()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";
            BugTrackerCab cab = new BugTrackerCab(12243, 12121, false, true, new NameValueCollection(), "c:\\test\\my.cab");
            cab.AnalysisData["New1"] = "Data1";
            cab.AnalysisData["New2"] = "Data2";
            context.CabAdded(BugTrackerReportType.Automatic, product, file, theEvent, cab);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void ScriptResultsAdded()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";
            BugTrackerCab cab = new BugTrackerCab(12243, 12121, false, true, new NameValueCollection(), "c:\\test\\my.cab");
            cab.AnalysisData["New1"] = "Data1";
            cab.AnalysisData["New2"] = "Data2";

            BugTrackerScriptResult result = new BugTrackerScriptResult("ScriptName", 1, DateTime.Now, DateTime.Now, "Script results");
            context.DebugScriptExecuted(BugTrackerReportType.Automatic, product, file, theEvent, cab, result);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void ScriptResultsAddedNullScriptData()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 1234568);
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "1.2.3.4", 234567);
            BugTrackerEvent theEvent = new BugTrackerEvent("Reference", "Plugin bug ref", 1111111, "CLR20 - Crash", 122, new NameValueCollection());
            theEvent.Signature["Exception"] = "0x23434";
            BugTrackerCab cab = new BugTrackerCab(12243, 12121, false, true, new NameValueCollection(), "c:\\test\\my.cab");
            cab.AnalysisData["New1"] = "Data1";
            cab.AnalysisData["New2"] = "Data2";

            BugTrackerScriptResult result = new BugTrackerScriptResult("ScriptName", 1, DateTime.Now, DateTime.Now, null);
            context.DebugScriptExecuted(BugTrackerReportType.Automatic, product, file, theEvent, cab, result);

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }
    }
}
