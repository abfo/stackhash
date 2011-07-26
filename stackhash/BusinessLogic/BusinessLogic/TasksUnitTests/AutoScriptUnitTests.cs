using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

using StackHashUtilities;
using StackHashBusinessObjects;
using StackHashTasks;
using StackHashErrorIndex;
using StackHashDebug;


namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for AutoScriptUnitTests
    /// </summary>
    [TestClass]
    public class AutoScriptUnitTests
    {
        String m_TempPath;
        StackHashDebuggerSettings m_DebuggerSettings;


        public AutoScriptUnitTests()
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
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            scriptManager.RemoveAutoScripts();
            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);

            m_DebuggerSettings = new StackHashDebuggerSettings();

            m_DebuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            m_DebuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;
            m_DebuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            m_DebuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            m_DebuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            m_DebuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
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
                PathUtils.SetFilesWritable(m_TempPath, true);
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
        public void RunAutoScript()
        {
            String dumpFileName = m_TempPath + "Cucku.exe.mdmp";
            String scriptFileName = m_TempPath + "AutoScript.wds";
            String resultsFileName = m_TempPath + "AutoScript.log";

            // First make a copy of the test cab file.
            File.Copy(TestSettings.TestDataFolder + @"Dumps\Cucku.exe.mdmp",
                dumpFileName, true);
            FileAttributes attributes = File.GetAttributes(dumpFileName);
            File.SetAttributes(dumpFileName, attributes & ~FileAttributes.ReadOnly);

            AutoScript autoScript = new AutoScript(m_TempPath);
            StackHashScriptSettings scriptSettings = autoScript.GenerateScript();
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(scriptFileName, resultsFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);

            // Now execute the script.
            StackHashDebug.Windbg winDbg = new Windbg();
            winDbg.RunScript(m_DebuggerSettings, false, scriptFileName, dumpFileName, m_TempPath, symPath, exePath, srcPath);

            // Load the results file.
            StackHashScriptResult scriptResults = new StackHashScriptResult(resultsFileName);

            // Analyse the results.
            StackHashDumpAnalysis analysis = new StackHashDumpAnalysis();
            analysis = autoScript.AnalyzeScriptResults(analysis, scriptResults);

            Assert.AreEqual("not available", analysis.SystemUpTime);
            Assert.AreEqual("0 days 0:02:20.000", analysis.ProcessUpTime);

            Assert.AreEqual("2.0.50727.3603", analysis.DotNetVersion);
        }
        [TestMethod]
        public void RunGetOSVersionScript()
        {
            String dumpFileName = m_TempPath + "Cucku.exe.mdmp";
            String scriptFileName = m_TempPath + "AutoScript.wds";
            String resultsFileName = m_TempPath + "AutoScript.log";

            // First make a copy of the test cab file.
            File.Copy(TestSettings.TestDataFolder + @"Dumps\Cucku.exe.mdmp",
                dumpFileName, true);
            FileAttributes attributes = File.GetAttributes(dumpFileName);
            File.SetAttributes(dumpFileName, attributes & ~FileAttributes.ReadOnly);

            GetOSVersionScript autoScript = new GetOSVersionScript(m_TempPath);
            StackHashScriptSettings scriptSettings = autoScript.GenerateScript();
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(scriptFileName, resultsFileName, ref symPath, ref exePath, ref srcPath);

            // Now execute the script.
            StackHashDebug.Windbg winDbg = new Windbg();
            winDbg.RunScript(m_DebuggerSettings, false, scriptFileName, dumpFileName, m_TempPath, symPath, exePath, srcPath);

            // Load the results file.
            StackHashScriptResult scriptResults = new StackHashScriptResult(resultsFileName);

            // Analyse the results.
            StackHashDumpAnalysis analysis = new StackHashDumpAnalysis();
            analysis = autoScript.AnalyzeScriptResults(analysis, scriptResults);

            Assert.AreEqual("not available", analysis.SystemUpTime);
            Assert.AreEqual("0 days 0:02:20.000", analysis.ProcessUpTime);
        }
        [TestMethod]
        public void RunAllAutoScripts()
        {
            String dumpFileName = m_TempPath + "Cucku.exe.mdmp";

            // First make a copy of the test cab file.
            File.Copy(TestSettings.TestDataFolder + @"Dumps\Cucku.exe.mdmp",
                dumpFileName, true);
            FileAttributes attributes = File.GetAttributes(dumpFileName);
            File.SetAttributes(dumpFileName, attributes & ~FileAttributes.ReadOnly);

            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            // Now execute the script.
            StackHashDebug.Windbg winDbg = new Windbg();

            try
            {
                Collection<AutoScriptBase> autoScripts = scriptManager.AutoScripts;

                foreach (AutoScriptBase autoScript in autoScripts)
                {
                    // Generate the script settings structure in memory. 
                    StackHashScriptSettings scriptSettings = autoScript.GenerateScript();

                    // Those settings are now used to create a WinDbg script file (wds). This file has a command
                    // to create a log file (the resultsFileName).
                    String resultsFileName = String.Format("{0}.log", Path.Combine(m_TempPath, autoScript.ScriptName));
                    String scriptFileName = String.Format("{0}.wds", Path.Combine(m_TempPath, autoScript.ScriptName));
                    String symPath = null;
                    String exePath = null;
                    String srcPath = null;
                    scriptSettings.GenerateScriptFile(scriptFileName, resultsFileName, ref symPath, ref exePath, ref srcPath);

                    // Run the wds file through the debugger to produce the results.log file.
                    winDbg.RunScript(m_DebuggerSettings, false, scriptFileName, dumpFileName, m_TempPath, symPath, exePath, srcPath);

                    // Load the results.log file.
                    StackHashScriptResult scriptResults = new StackHashScriptResult(resultsFileName);

                    // Analyse the results.
                    StackHashDumpAnalysis analysis = new StackHashDumpAnalysis();
                    analysis = autoScript.AnalyzeScriptResults(analysis, scriptResults);
                }
            }
            finally
            {
                scriptManager.RemoveAutoScripts();
            }
        }
    }
}
