using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;
using StackHashDebug;
using StackHashUtilities;

namespace StackHashDebugUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class RunScriptUnitTests
    {
        string m_TempPath;

        public RunScriptUnitTests()
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
            m_TempPath = Path.GetTempPath() + "StackHashDebugScriptTests";

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

        [TestMethod]
        public void RunSimpleScript()
        {
            // Create a test script.
            String testScriptName = "ScriptName";
            String testCommand = @"r";
            String testComment = @"Just a demo";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String scriptFileName = m_TempPath + @"\GeneratedScript.wds";
            String resultsFileName = m_TempPath + @"\Results.log";
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(scriptFileName, resultsFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);

            // Run the script with the debugger.
            Windbg debugger = new Windbg();
            String dumpFileName = TestSettings.TestDataFolder + @"Dumps\Cucku.exe.mdmp";

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debugger.RunScript(debuggerSettings, true, scriptFileName, dumpFileName, resultsFileName, symPath, exePath, srcPath);

            Assert.AreEqual(true, File.Exists(resultsFileName));
            String[] allResults = File.ReadAllLines(resultsFileName, Encoding.Unicode);

            Assert.AreEqual(true, allResults.Length > 0);
        }


        
        public void simpleScriptNCommands(int numCommands, bool addComment)
        {
            // Create a test script.
            String testScriptName = "ScriptName";
            String testCommand = @"r";

            String testComment = null; 
            if (addComment)
                testComment = @"Just a demo";

            StackHashScript script = new StackHashScript();

            for (int i = 0; i < numCommands; i++)
            {
                script.Add(new StackHashScriptLine(testCommand, testComment + i.ToString()));
            }
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String scriptFileName = m_TempPath + @"\GeneratedScript.wds";
            String resultsFileName = m_TempPath + @"\Results.log";

            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(scriptFileName, resultsFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);

            // Run the script with the debugger.
            Windbg debugger = new Windbg();

            String dumpFileName = TestSettings.TestDataFolder + @"Dumps\Cucku.exe.mdmp";

            DateTime startTime = DateTime.Now.ToUniversalTime();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debugger.RunScript(debuggerSettings, true, scriptFileName, dumpFileName, resultsFileName, symPath, exePath, srcPath);

            DateTime endTime = DateTime.Now.ToUniversalTime();
            
            Assert.AreEqual(true, File.Exists(resultsFileName));

            // Now load in the test results.
            StackHashScriptResult result = new StackHashScriptResult(resultsFileName);


            Assert.AreEqual(scriptSettings.Name, result.Name);
            Assert.AreEqual(scriptSettings.LastModifiedDate.Date, result.LastModifiedDate.Date);
            Assert.AreEqual(scriptSettings.LastModifiedDate.Hour, result.LastModifiedDate.Hour);
            Assert.AreEqual(scriptSettings.LastModifiedDate.Minute, result.LastModifiedDate.Minute);
            Assert.AreEqual(scriptSettings.LastModifiedDate.Second, result.LastModifiedDate.Second);


            // Recorded time is only accurate to the second.
            long ticksInASecond = 10000000;

            startTime = new DateTime((startTime.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);
            endTime = new DateTime((endTime.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

            bool isGreaterEqual = result.RunDate >= startTime;
            bool isLessThanEqual = result.RunDate <= endTime;

            long ticks1 = result.RunDate.Ticks;
            long ticks2 = startTime.Ticks;
            long ticks3 = endTime.Ticks;

            Assert.AreEqual(true, (result.RunDate >= startTime) && (result.RunDate <= endTime));

            Assert.AreEqual(numCommands, scriptSettings.Script.Count);

            for (int i = 0; i < numCommands; i++)
            {
                Assert.AreEqual(scriptSettings.Script[i].Command, result.ScriptResults[i].ScriptLine.Command);
                Assert.AreEqual(scriptSettings.Script[i].Comment, result.ScriptResults[i].ScriptLine.Comment);

                Assert.AreEqual(5, result.ScriptResults[i].ScriptLineOutput.Count);
                Assert.AreEqual(true, result.ScriptResults[i].ScriptLineOutput[0].StartsWith("eax="));
                Assert.AreEqual(true, result.ScriptResults[i].ScriptLineOutput[1].StartsWith("eip="));
                Assert.AreEqual(true, result.ScriptResults[i].ScriptLineOutput[2].StartsWith("cs="));
            }
        }

        [TestMethod]
        public void SimpleScript1Command()
        {
            simpleScriptNCommands(1, true);
        }

        [TestMethod]
        public void SimpleScript2Commands()
        {
            simpleScriptNCommands(2, true);
        }

        [TestMethod]
        public void SimpleScript100Commands()
        {
            simpleScriptNCommands(100, true);
        }

        [TestMethod]
        public void SimpleScript1CommandNoComment()
        {
            simpleScriptNCommands(1, false);
        }
    }
}
