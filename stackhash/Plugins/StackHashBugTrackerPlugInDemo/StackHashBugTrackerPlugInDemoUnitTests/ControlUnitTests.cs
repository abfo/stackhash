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
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ControlUnitTests
    {
        public ControlUnitTests()
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

        [TestMethod]
        public void ControlConstructor()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);
        }

        public int GetNumberOfActiveProfiles(IBugTrackerV1Control plugInControl)
        {
            NameValueCollection diagnostics = plugInControl.PlugInDiagnostics;
            Assert.AreNotEqual(null, diagnostics);

            Assert.AreEqual(true, diagnostics.AllKeys.Contains("NumberOfActiveContexts"));

            int result = Int32.Parse(diagnostics["NumberOfActiveContexts"]);
            return result;
        }


        [TestMethod]
        public void GetPlugInDiagnostics()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            NameValueCollection diagnostics = plugInControl.PlugInDiagnostics;
            Assert.AreNotEqual(null, diagnostics);

            Assert.AreEqual(true, diagnostics.AllKeys.Contains("NumberOfActiveContexts"));
            Assert.AreEqual("0", diagnostics["NumberOfActiveContexts"]);
        }

        [TestMethod]
        public void GetPlugInDefaultProperties()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            NameValueCollection plugInProperties = plugInControl.PlugInDefaultProperties;
            Assert.AreNotEqual(null, plugInProperties);

            Assert.AreEqual(true, plugInProperties.AllKeys.Contains("LogVerbose"));
            Assert.AreEqual("0", plugInProperties["LogVerbose"]);

            Assert.AreEqual(true, plugInProperties.AllKeys.Contains("EnableContext"));
            Assert.AreEqual("1", plugInProperties["EnableContext"]);
        }

        
        [TestMethod]
        public void CreateOneContext()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);
            Assert.AreEqual(1, GetNumberOfActiveProfiles(plugInControl));

            plugInControl.ReleaseContext(context);
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void DeleteAContextThatDoesntExist()
        {
            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);

            IBugTrackerV1Context context = plugInControl.CreateContext();
            Assert.AreNotEqual(null, context);

            plugInControl.ReleaseContext(context);
            plugInControl.ReleaseContext(context); // Should just fail quietly.
            Assert.AreEqual(0, GetNumberOfActiveProfiles(plugInControl));
        }

        [TestMethod]
        public void AddManyContextsAndRelease()
        {
            int numContextsToCreate = 10;

            List<IBugTrackerV1Context> allContexts = new List<IBugTrackerV1Context>();

            IBugTrackerV1Control plugInControl = new FaultDatabaseControl();
            Assert.AreNotEqual(null, plugInControl);


            for (int i = 0; i < numContextsToCreate; i++)
            {
                IBugTrackerV1Context context = plugInControl.CreateContext();
                Assert.AreNotEqual(null, context);

                allContexts.Add(context);
                Assert.AreEqual(i + 1, GetNumberOfActiveProfiles(plugInControl));
            }

            foreach (IBugTrackerV1Context context in allContexts)
            {
                plugInControl.ReleaseContext(context);
                numContextsToCreate--;
                Assert.AreEqual(numContextsToCreate, GetNumberOfActiveProfiles(plugInControl));
            }
        }
    }
}
