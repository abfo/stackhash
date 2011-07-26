using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackHashBusinessObjects;
using StackHashTasks;
using StackHashDebug;
using StackHashUtilities;
using StackHashErrorIndex;


namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for DebugScriptTaskTests
    /// </summary>
    [TestClass]
    public class DebugScriptTaskTests
    {
        private const int s_TaskTimeout = 120000;
        private String m_TempPath;
        private StackHashDebuggerSettings m_DebuggerSettings;
        private Windbg m_Debugger = new Windbg();


        public DebugScriptTaskTests()
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashRunScriptTests\\";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);

            m_DebuggerSettings = new StackHashDebuggerSettings();

            m_DebuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            m_DebuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;
            m_DebuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            m_DebuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            m_DebuggerSettings.BinaryPath = StackHashSearchPath.DefaultSymbolPath;
            m_DebuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultSymbolPath;
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
                // Mark scripts as not readonly.
                PathUtils.MarkDirectoryWritable(Path.Combine(m_TempPath, "Scripts"), false);
                PathUtils.DeleteDirectory(m_TempPath, true);
            }
        }


        /// <summary>
        /// Run a script on a cab that doesn't exist.
        /// </summary>
        [TestMethod]
        public void RunScriptOnCabThatDoesntExist()
        {
            // Create an index with 1 cab file.
            XmlErrorIndex errorIndex = new XmlErrorIndex(m_TempPath, "ErrorIndex");
            errorIndex.Activate();

            StackHashTestIndexData testData = new StackHashTestIndexData();
            testData.NumberOfProducts = 1;
            testData.NumberOfFiles = 1;
            testData.NumberOfEvents = 1;
            testData.NumberOfEventInfos = 1;
            testData.NumberOfCabs = 1;
            testData.UseLargeCab = false;
            TestManager.CreateTestIndex(errorIndex, testData);

            // Delete the cab file.
            StackHashProduct product = errorIndex.GetProduct(1);
            StackHashFile file = errorIndex.GetFile(product, 1);
            StackHashEventPackageCollection events = errorIndex.GetProductEvents(product);

            String cabFileName = errorIndex.GetCabFileName(product, file, events[0].EventData, events[0].Cabs[0].Cab);
            if (File.Exists(cabFileName))
                File.Delete(cabFileName);

            ScriptManager scriptManager = new ScriptManager(m_TempPath + "Scripts");
            Assert.AreEqual(scriptManager.NumberOfAutoScripts, scriptManager.ScriptNames.Count);

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(errorIndex, scriptManager, m_Debugger, m_DebuggerSettings);

            // Set up parameters for the task.
            DebugScriptTaskParameters runScriptParams = new DebugScriptTaskParameters();

            // Standard task parameters.
            runScriptParams.IsBackgroundTask = true;
            runScriptParams.Name = "TestRunOneTask";
            runScriptParams.RunInParallel = false;
            runScriptParams.UseSeparateThread = true;
            runScriptParams.ContextId = 0;
            runScriptParams.ClientData = new StackHashClientData(Guid.NewGuid(), "MarkJ", 1);
            runScriptParams.Debugger = m_Debugger;
            runScriptParams.TheScriptManager = scriptManager;
            runScriptParams.TheScriptResultsManager = scriptResultsManager;
            runScriptParams.Product = product;
            runScriptParams.File = file;
            runScriptParams.TheEvent = events[0].EventData;
            runScriptParams.Cab = events[0].Cabs[0].Cab;
            runScriptParams.ErrorIndex = errorIndex;

            // Create the task and run it.
            DebugScriptTask debugScriptTask = new DebugScriptTask(runScriptParams);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(debugScriptTask);

            taskManager.WaitForTaskCompletion(debugScriptTask, s_TaskTimeout);

            Assert.AreEqual(true, debugScriptTask.CurrentTaskState.TaskCompleted);

            Assert.AreNotEqual(null, debugScriptTask.LastException);
            Assert.AreEqual(true, debugScriptTask.LastException is StackHashException);

            StackHashException ex = debugScriptTask.LastException as StackHashException;

            Assert.AreEqual(StackHashServiceErrorCode.CabDoesNotExist, ex.ServiceErrorCode);

            StackHashCabCollection cabs = errorIndex.LoadCabList(product, file, events[0].EventData);
            StackHashDumpAnalysis analysis = cabs[0].DumpAnalysis;
        }

        /// <summary>
        /// Run a script on a corrupt cab.
        /// </summary>
        [TestMethod]
        public void RunScriptOnCorruptCab()
        {
            // Create an index with 1 cab file.
            XmlErrorIndex errorIndex = new XmlErrorIndex(m_TempPath, "ErrorIndex");
            errorIndex.Activate();

            StackHashTestIndexData testData = new StackHashTestIndexData();
            testData.NumberOfProducts = 1;
            testData.NumberOfFiles = 1;
            testData.NumberOfEvents = 1;
            testData.NumberOfEventInfos = 1;
            testData.NumberOfCabs = 1;
            testData.UseLargeCab = false;
            TestManager.CreateTestIndex(errorIndex, testData);

            // Delete the cab file.
            StackHashProduct product = errorIndex.GetProduct(1);
            StackHashFile file = errorIndex.GetFile(product, 1);
            StackHashEventPackageCollection events = errorIndex.GetProductEvents(product);

            String cabFileName = errorIndex.GetCabFileName(product, file, events[0].EventData, events[0].Cabs[0].Cab);
            if (File.Exists(cabFileName))
            {
                FileStream fileStream = File.Open(cabFileName, FileMode.Open, FileAccess.ReadWrite);

                try
                {
                    fileStream.SetLength(100);
                }
                finally
                {
                    fileStream.Close();
                }
            }


            ScriptManager scriptManager = new ScriptManager(m_TempPath + "Scripts");
            Assert.AreEqual(scriptManager.NumberOfAutoScripts, scriptManager.ScriptNames.Count);

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(errorIndex, scriptManager, m_Debugger, m_DebuggerSettings);

            // Set up parameters for the task.
            DebugScriptTaskParameters runScriptParams = new DebugScriptTaskParameters();

            // Standard task parameters.
            runScriptParams.IsBackgroundTask = true;
            runScriptParams.Name = "TestRunOneTask";
            runScriptParams.RunInParallel = false;
            runScriptParams.UseSeparateThread = true;
            runScriptParams.ContextId = 0;
            runScriptParams.ClientData = new StackHashClientData(Guid.NewGuid(), "MarkJ", 1);
            runScriptParams.Debugger = m_Debugger;
            runScriptParams.TheScriptManager = scriptManager;
            runScriptParams.TheScriptResultsManager = scriptResultsManager;
            runScriptParams.Product = product;
            runScriptParams.File = file;
            runScriptParams.TheEvent = events[0].EventData;
            runScriptParams.Cab = events[0].Cabs[0].Cab;
            runScriptParams.ErrorIndex = errorIndex;

            // Create the task and run it.
            DebugScriptTask debugScriptTask = new DebugScriptTask(runScriptParams);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(debugScriptTask);

            taskManager.WaitForTaskCompletion(debugScriptTask, s_TaskTimeout);

            Assert.AreEqual(true, debugScriptTask.CurrentTaskState.TaskCompleted);

            Assert.AreNotEqual(null, debugScriptTask.LastException);
            Assert.AreEqual(true, debugScriptTask.LastException is StackHashException);

            StackHashException ex = debugScriptTask.LastException as StackHashException;

            Assert.AreEqual(StackHashServiceErrorCode.CabIsCorrupt, ex.ServiceErrorCode);

            StackHashCabCollection cabs = errorIndex.LoadCabList(product, file, events[0].EventData);
            StackHashDumpAnalysis analysis = cabs[0].DumpAnalysis;
        }
    }
}
