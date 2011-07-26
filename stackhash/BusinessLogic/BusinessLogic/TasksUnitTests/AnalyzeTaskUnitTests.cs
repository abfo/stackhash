using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.IO;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashWinQual;
using StackHashDebug;
using StackHashUtilities;
using StackHashErrorIndex;



namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for AnalyzeTaskUnitTests
    /// </summary>
    [TestClass]
    public class AnalyzeTaskUnitTests
    {
        private const int s_TaskTimeout = 120000;
        private String m_TempPath;
        private StackHashDebuggerSettings m_DebuggerSettings;
        private Windbg m_Debugger = new Windbg();

        public AnalyzeTaskUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashAutoScriptTests\\";

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
        public void TestConstructor()
        {
        }

        [TestMethod]
        public void AutoScriptOnOneCab()
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

            int productId = 1;


            ScriptManager scriptManager = new ScriptManager(m_TempPath + "Scripts");
            Assert.AreEqual(scriptManager.NumberOfAutoScripts, scriptManager.ScriptNames.Count);

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(errorIndex, scriptManager, m_Debugger, m_DebuggerSettings);

            // Set up parameters for the task.
            AnalyzeTaskParameters analyzeParams = new AnalyzeTaskParameters();

            // Standard task parameters.
            analyzeParams.IsBackgroundTask = true;
            analyzeParams.Name = "TestRunOneTask";
            analyzeParams.RunInParallel = false;
            analyzeParams.UseSeparateThread = true;
            analyzeParams.AnalysisSettings = new StackHashAnalysisSettings();
            analyzeParams.AnalysisSettings.ForceRerun = true;
            analyzeParams.ContextId = 0;
            analyzeParams.ClientData = new StackHashClientData(Guid.NewGuid(), "MarkJ", 1);
            analyzeParams.Debugger = m_Debugger;
            analyzeParams.DebuggerSettings = m_DebuggerSettings;
            analyzeParams.TheScriptManager = scriptManager;
            analyzeParams.TheScriptResultsManager = scriptResultsManager;
            analyzeParams.ProductsToSynchronize = new StackHashProductSyncDataCollection();
            analyzeParams.ProductsToSynchronize.Add(new StackHashProductSyncData(productId));

            analyzeParams.ErrorIndex = errorIndex;

            // Create the task and run it.
            AnalyzeTask analyzeTask = new AnalyzeTask(analyzeParams);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(analyzeTask);

            taskManager.WaitForTaskCompletion(analyzeTask, s_TaskTimeout);

            Assert.AreEqual(true, analyzeTask.CurrentTaskState.TaskCompleted);

            StackHashProductCollection products = errorIndex.LoadProductList();
            StackHashFileCollection files = errorIndex.LoadFileList(products[0]);
            StackHashEventCollection events = errorIndex.LoadEventList(products[0], files[0]);
            StackHashCabCollection cabs = errorIndex.LoadCabList(products[0], files[0], events[0]);

            StackHashDumpAnalysis analysis = cabs[0].DumpAnalysis;

            Assert.AreEqual("not available", analysis.SystemUpTime);
            Assert.AreEqual("0 days 0:00:15.000", analysis.ProcessUpTime);
            Assert.AreEqual("2.0.50727.3603", analysis.DotNetVersion);
            Assert.AreEqual("x86", analysis.MachineArchitecture);
            Assert.AreEqual("Windows XP Version 2600 (Service Pack 3) MP (2 procs) Free x86 compatible", analysis.OSVersion);
        }


        /// <summary>
        /// Run autoscript on a cab - but product is not enabled so shouldn't.
        /// </summary>
        [TestMethod]
        public void AutoScriptOnOneCabProductNotEnabled()
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


            ScriptManager scriptManager = new ScriptManager(m_TempPath + "Scripts");
            Assert.AreEqual(scriptManager.NumberOfAutoScripts, scriptManager.ScriptNames.Count);

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(errorIndex, scriptManager, m_Debugger, m_DebuggerSettings);

            // Set up parameters for the task.
            AnalyzeTaskParameters analyzeParams = new AnalyzeTaskParameters();

            // Standard task parameters.
            analyzeParams.IsBackgroundTask = true;
            analyzeParams.Name = "TestRunOneTask";
            analyzeParams.RunInParallel = false;
            analyzeParams.UseSeparateThread = true;
            analyzeParams.AnalysisSettings = new StackHashAnalysisSettings();
            analyzeParams.AnalysisSettings.ForceRerun = true;
            analyzeParams.ContextId = 0;
            analyzeParams.ClientData = new StackHashClientData(Guid.NewGuid(), "MarkJ", 1);
            analyzeParams.Debugger = m_Debugger;
            analyzeParams.DebuggerSettings = m_DebuggerSettings;
            analyzeParams.TheScriptManager = scriptManager;
            analyzeParams.TheScriptResultsManager = scriptResultsManager;
            analyzeParams.ProductsToSynchronize = new StackHashProductSyncDataCollection();

            analyzeParams.ErrorIndex = errorIndex;

            // Create the task and run it.
            AnalyzeTask analyzeTask = new AnalyzeTask(analyzeParams);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(analyzeTask);

            taskManager.WaitForTaskCompletion(analyzeTask, s_TaskTimeout);

            Assert.AreEqual(true, analyzeTask.CurrentTaskState.TaskCompleted);

            StackHashProductCollection products = errorIndex.LoadProductList();
            StackHashFileCollection files = errorIndex.LoadFileList(products[0]);
            StackHashEventCollection events = errorIndex.LoadEventList(products[0], files[0]);
            StackHashCabCollection cabs = errorIndex.LoadCabList(products[0], files[0], events[0]);

            StackHashDumpAnalysis analysis = cabs[0].DumpAnalysis;

            Assert.AreEqual(null, analysis.DotNetVersion);
            Assert.AreEqual(null, analysis.MachineArchitecture);
            Assert.AreEqual(null, analysis.OSVersion);
            Assert.AreEqual(null, analysis.ProcessUpTime);
            Assert.AreEqual(null, analysis.SystemUpTime);
        }

        
        [TestMethod]
        public void AutoScriptOnOneCabAlreadyRun()
        {
            // If the auto task sees the AutoScript.log file exists and has the same 
            // version as the current AutoScript.xml then it shouldn't run it again.
            // Determine this by checking the file time after a second run.

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



            ScriptManager scriptManager = new ScriptManager(m_TempPath + "Scripts");
            Assert.AreEqual(scriptManager.NumberOfAutoScripts, scriptManager.ScriptNames.Count);

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(errorIndex, scriptManager, m_Debugger, m_DebuggerSettings);

            // Set up parameters for the task.
            AnalyzeTaskParameters analyzeParams = new AnalyzeTaskParameters();

            // Standard task parameters.
            analyzeParams.IsBackgroundTask = true;
            analyzeParams.Name = "TestRunOneTask";
            analyzeParams.RunInParallel = false;
            analyzeParams.UseSeparateThread = true;
            analyzeParams.AnalysisSettings = new StackHashAnalysisSettings();
            analyzeParams.AnalysisSettings.ForceRerun = true;
            analyzeParams.ContextId = 0;
            analyzeParams.ClientData = new StackHashClientData(Guid.NewGuid(), "MarkJ", 1);
            analyzeParams.Debugger = m_Debugger;
            analyzeParams.DebuggerSettings = m_DebuggerSettings;
            analyzeParams.TheScriptManager = scriptManager;
            analyzeParams.TheScriptResultsManager = scriptResultsManager;

            int productId = 1;
            analyzeParams.ProductsToSynchronize = new StackHashProductSyncDataCollection();
            analyzeParams.ProductsToSynchronize.Add(new StackHashProductSyncData(productId));

            analyzeParams.ErrorIndex = errorIndex;

            // Create the task and run it.
            AnalyzeTask analyzeTask = new AnalyzeTask(analyzeParams);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(analyzeTask);
            taskManager.WaitForTaskCompletion(analyzeTask, s_TaskTimeout);


            StackHashProductCollection products = errorIndex.LoadProductList();
            StackHashFileCollection files = errorIndex.LoadFileList(products[0]);
            StackHashEventCollection events = errorIndex.LoadEventList(products[0], files[0]);
            StackHashCabCollection cabs = errorIndex.LoadCabList(products[0], files[0], events[0]);

            StackHashScriptResult script1 = scriptResultsManager.GetResultFileData(products[0], files[0], events[0], cabs[0], "AutoScript");

            // Wait for 1 second - so file time granularity exceeded.
            Thread.Sleep(1000);

            // Now run the task again.
            analyzeTask = new AnalyzeTask(analyzeParams);
            taskManager.Enqueue(analyzeTask);
            taskManager.WaitForTaskCompletion(analyzeTask, s_TaskTimeout);


            Assert.AreEqual(true, analyzeTask.CurrentTaskState.TaskCompleted);

            // Refresh the cab list data.
            cabs = errorIndex.LoadCabList(products[0], files[0], events[0]);

            StackHashDumpAnalysis analysis = cabs[0].DumpAnalysis;

            Assert.AreEqual("not available", analysis.SystemUpTime);
            Assert.AreEqual("0 days 0:00:15.000", analysis.ProcessUpTime);
            Assert.AreEqual("2.0.50727.3603", analysis.DotNetVersion);

            
            StackHashScriptResult script2 = scriptResultsManager.GetResultFileData(products[0], files[0], events[0], cabs[0], "AutoScript");

            Assert.AreEqual(script1.RunDate, script2.RunDate);
        }

        [TestMethod]
        public void AutoScriptOnOneCabAlreadyRunButNewerVersion()
        {
            // If the auto task sees the AutoScript.log file exists and has the same 
            // version as the current AutoScript.xml then it shouldn't run it again.
            // Determine this by checking the file time after a second run.

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



            ScriptManager scriptManager = new ScriptManager(m_TempPath + "Scripts");
            Assert.AreEqual(scriptManager.NumberOfAutoScripts, scriptManager.ScriptNames.Count);

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(errorIndex, scriptManager, m_Debugger, m_DebuggerSettings);

            // Set up parameters for the task.
            AnalyzeTaskParameters analyzeParams = new AnalyzeTaskParameters();

            // Standard task parameters.
            analyzeParams.IsBackgroundTask = true;
            analyzeParams.Name = "TestRunOneTask";
            analyzeParams.RunInParallel = false;
            analyzeParams.UseSeparateThread = true;
            analyzeParams.AnalysisSettings = new StackHashAnalysisSettings();
            analyzeParams.AnalysisSettings.ForceRerun = true;
            analyzeParams.ContextId = 0;
            analyzeParams.ClientData = new StackHashClientData(Guid.NewGuid(), "MarkJ", 1);
            analyzeParams.Debugger = m_Debugger;
            analyzeParams.DebuggerSettings = m_DebuggerSettings;
            analyzeParams.TheScriptManager = scriptManager;
            analyzeParams.TheScriptResultsManager = scriptResultsManager;
            int productId = 1;
            analyzeParams.ProductsToSynchronize = new StackHashProductSyncDataCollection();
            analyzeParams.ProductsToSynchronize.Add(new StackHashProductSyncData(productId));

            analyzeParams.ErrorIndex = errorIndex;

            // Create the task and run it.
            AnalyzeTask analyzeTask = new AnalyzeTask(analyzeParams);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(analyzeTask);
            taskManager.WaitForTaskCompletion(analyzeTask, s_TaskTimeout);


            StackHashProductCollection products = errorIndex.LoadProductList();
            StackHashFileCollection files = errorIndex.LoadFileList(products[0]);
            StackHashEventCollection events = errorIndex.LoadEventList(products[0], files[0]);
            StackHashCabCollection cabs = errorIndex.LoadCabList(products[0], files[0], events[0]);

            StackHashScriptResult script1 = scriptResultsManager.GetResultFileData(products[0], files[0], events[0], cabs[0], "AutoScript");

            // Wait for 1 second - so file time granularity exceeded.
            Thread.Sleep(1000);

            // Change the version on the autoscript.
            StackHashScriptSettings settings = scriptManager.LoadScript("AutoScript");
            settings.LastModifiedDate = DateTime.Now;
            scriptManager.AddScript(settings, true, true);

            // Now run the task again.
            analyzeTask = new AnalyzeTask(analyzeParams);
            taskManager.Enqueue(analyzeTask);
            taskManager.WaitForTaskCompletion(analyzeTask, s_TaskTimeout);


            Assert.AreEqual(true, analyzeTask.CurrentTaskState.TaskCompleted);

            // Refresh the cab list data.
            cabs = errorIndex.LoadCabList(products[0], files[0], events[0]);

            StackHashDumpAnalysis analysis = cabs[0].DumpAnalysis;

            Assert.AreEqual("not available", analysis.SystemUpTime);
            Assert.AreEqual("0 days 0:00:15.000", analysis.ProcessUpTime);
            Assert.AreEqual("2.0.50727.3603", analysis.DotNetVersion);


            StackHashScriptResult script2 = scriptResultsManager.GetResultFileData(products[0], files[0], events[0], cabs[0], "AutoScript");

            Assert.AreEqual(true, script2.RunDate > script1.RunDate);
        }

    }
}
