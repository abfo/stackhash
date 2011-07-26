using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashUtilities;

namespace StackHashUtilitiesUnitTests
{
    /// <summary>
    /// Summary description for TestDataUnitTests
    /// </summary>
    [TestClass]
    public class TestDataUnitTests
    {
        public TestDataUnitTests()
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
        public void RunUnderTheDebugger()
        {
            String testDataFolder = TestSettings.TestDataFolder;

            Console.WriteLine(testDataFolder);
        }

        [TestMethod]
        public void LoadTestSettingsAndGetTestMode()
        {
            String testMode = TestSettings.GetAttribute("TestMode");
            Assert.AreEqual("1", testMode);
        }

        // Not always set in testmode.xml
        [TestMethod]
        [Ignore]
        public void LoadTestSettingsAndGetConnectionString()
        {
            String connectionString = TestSettings.GetAttribute("ConnectionString");
            Assert.AreEqual(false, String.IsNullOrEmpty(connectionString));
        }
    }
}
