using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for ControllerUnitTests
    /// </summary>
    [TestClass]
    public class ControllerUnitTests
    {
        StackHashSettings m_Properties;
        String m_TempPath;

        public ControllerUnitTests()
        {
            StackHashContextSettings contextSettings = new StackHashContextSettings();
            contextSettings.Id = 0;
            contextSettings.WinQualSettings = new WinQualSettings(TestSettings.WinQualUserName, TestSettings.WinQualPassword, "Cucku", 90, new StackHashProductSyncDataCollection(), 
                false, 0, 1, WinQualSettings.DefaultSyncsBeforeResync, false);
            contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
            contextSettings.ErrorIndexSettings.Folder = Path.GetTempPath() + "\\StackHashIndex";
            contextSettings.ErrorIndexSettings.Name = "TestController";
            contextSettings.ErrorIndexSettings.Type = ErrorIndexType.Xml;

            m_Properties = new StackHashSettings();
            m_Properties.ContextCollection = new StackHashContextCollection();
            m_Properties.ContextCollection.Add(contextSettings);
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
            m_TempPath = Path.GetTempPath() + "StackHashTaskTesting";

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
        [Ignore]
        public void TestSynchronizeAll()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            Controller controller = new Controller(settingsFileName, false, true); // test mode.

//            controller.SynchronizeAll();
        }

        [TestMethod]
        public void Constructor()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            string logFileName = string.Format("{0}\\forceshlog.txt", m_TempPath);
            StreamWriter writer = File.CreateText(logFileName);

            Controller controller = new Controller(settingsFileName, false, true); // test mode.

            controller.Dispose();

            writer.Close();
        }

        [TestMethod]
        public void ConstructorCheckServiceInstanceFile()
        {
            string settingsFileName = string.Format("{0}\\Settings.xml", m_TempPath);
            string logFileName = string.Format("{0}\\forceshlog.txt", m_TempPath);
            StreamWriter writer = File.CreateText(logFileName);

            Controller controller = new Controller(settingsFileName, false, true); // test mode.

            controller.Dispose();

            writer.Close();

            // Also make sure the instance data file has been created.
            String instanceFile = Path.GetDirectoryName(settingsFileName) + "\\ServiceInstanceData.txt";

            Assert.AreEqual(true, File.Exists(instanceFile));
        }
    }
}
