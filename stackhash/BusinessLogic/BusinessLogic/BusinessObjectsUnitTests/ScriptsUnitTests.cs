using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

using StackHashBusinessObjects;
using StackHashUtilities;


namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for ScriptsUnitTests
    /// </summary>
    [TestClass]
    public class ScriptsUnitTests
    {
        String m_TempPath;

        public ScriptsUnitTests()
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
            m_TempPath = Path.GetTempPath() + "StackHashScriptTests";

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
        public void ScriptConstructor()
        {
            String testCommand = @"$$ This is a simple command";
            String testComment = @"Just a demo";
            String testScriptName = "Name";
        
            // Create a script.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            Assert.AreEqual(testCommand, script[0].Command);
            Assert.AreEqual(testComment, script[0].Comment);
            Assert.AreEqual(StackHashScriptDumpType.UnmanagedAndManaged, scriptSettings.DumpType);

            Assert.AreEqual(testScriptName, scriptSettings.Name);
            Assert.AreEqual(scriptSettings.LastModifiedDate, scriptSettings.CreationDate);
            Assert.AreEqual(true, DateTime.Now.ToUniversalTime().Second - scriptSettings.CreationDate.Second <= 2);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void ScriptNullScript()
        {
            String testScriptName = "Name";

            // Create a script.
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, null);
        }

        [TestMethod]
        public void ScriptSymbolPathConstructor()
        {
            StackHashSearchPath symbolPath = new StackHashSearchPath();

            Assert.AreEqual(0, symbolPath.Count);
        }

        [TestMethod]
        public void ScriptSymbolPathDefault()
        {
            StackHashSearchPath symbolPath = StackHashSearchPath.DefaultSymbolPath;

            Assert.AreEqual(1, symbolPath.Count);
            Assert.AreEqual(true, symbolPath[0].Contains(@"SRV*"));
            Assert.AreEqual(true, symbolPath[0].Contains(@"http://msdl.microsoft.com/download/symbols"));
        }

        [TestMethod]
        public void ScriptSymbolPathGetFullMultipleEntries()
        {
            StackHashSearchPath symbolPath = new StackHashSearchPath();
            symbolPath.Add("c:\\path1");
            symbolPath.Add("c:\\path2");
            symbolPath.Add("c:\\path3");
            symbolPath.Add("c:\\path4");

            String fullPath = symbolPath.FullPath;
            String[] pathElements = fullPath.Split(new char [] {';'});

            for (int i = 0; i < pathElements.Length - 1; i++)
            {
                Assert.AreEqual(pathElements[i], "c:\\path" + (i + 1).ToString());
            }
        }

        [TestMethod]
        public void ScriptGenerateNoLines()
        {
            String testScriptName = "Name";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";

            
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);
           
            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should just contain a comment line.
            Assert.AreEqual(4, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[3].Contains(".logclose"));
        }

        [TestMethod]
        public void ScriptGenerate1Line()
        {
            String testScriptName = "Name";
            String testCommand = @"$$ This is a simple command";
            String testComment = @"Just a demo";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String m_TempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(m_TempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);


            // Read all of the data in.
            String[] allLines = File.ReadAllLines(m_TempFileName);

            Assert.AreEqual(true, File.Exists(m_TempFileName));

            // Should contain the script header comment, and one command comment and one command.
            Assert.AreEqual(7, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains(scriptSettings.Script[0].Comment));
            Assert.AreEqual(true, allLines[3].StartsWith(scriptSettings.Script[0].Command));
            Assert.AreEqual(true, allLines[4].Contains(scriptSettings.Script[0].Comment));
            Assert.AreEqual(true, allLines[5].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[6].Contains(".logclose"));
        }
        [TestMethod]
        public void ScriptGenerate2Lines()
        {
            String testScriptName = "Name";
            String testCommand1 = @"$$ Command 1";
            String testComment1 = @"Demo Comment 1";
            String testCommand2 = @"$$ Command 2";
            String testComment2 = @"Another comment";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            Assert.AreEqual(10, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains(scriptSettings.Script[0].Comment));
            Assert.AreEqual(true, allLines[3].StartsWith(scriptSettings.Script[0].Command));
            Assert.AreEqual(true, allLines[4].Contains(scriptSettings.Script[0].Comment));
            Assert.AreEqual(true, allLines[5].Contains(scriptSettings.Script[1].Comment));
            Assert.AreEqual(true, allLines[6].StartsWith(scriptSettings.Script[1].Command));
            Assert.AreEqual(true, allLines[7].Contains(scriptSettings.Script[1].Comment));
            Assert.AreEqual(true, allLines[8].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[9].Contains(".logclose"));
        }
        [TestMethod]
        public void ScriptGenerate2LinesOtherWayAround()
        {
            String testScriptName = "Name";
            String testCommand2 = @"$$ Command 1";
            String testComment2 = @"Demo Comment 1";
            String testCommand1 = @"$$ Command 2";
            String testComment1 = @"Another comment";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(null, symPath);
            Assert.AreEqual(null, exePath);
            Assert.AreEqual(null, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            Assert.AreEqual(10, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains(scriptSettings.Script[0].Comment));
            Assert.AreEqual(true, allLines[3].StartsWith(scriptSettings.Script[0].Command));
            Assert.AreEqual(true, allLines[4].Contains(scriptSettings.Script[0].Comment));
            Assert.AreEqual(true, allLines[5].Contains(scriptSettings.Script[1].Comment));
            Assert.AreEqual(true, allLines[6].StartsWith(scriptSettings.Script[1].Command));
            Assert.AreEqual(true, allLines[7].Contains(scriptSettings.Script[1].Comment));
            Assert.AreEqual(true, allLines[8].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[9].Contains(".logclose"));
        }

        private void scriptSaveLoadNCommands(int numCommands, bool addComment)
        {
            StackHashScript script = new StackHashScript();

            for (int i = 0; i < numCommands; i++)
            {
                String command = "command" + i.ToString();
                String comment = null;
                if (addComment)
                    comment = "comment" + i.ToString();
                script.Add(new StackHashScriptLine(command, comment));
            }
            String testScriptName = m_TempPath + "\\scriptsettings.wds";
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);
            scriptSettings.Name = "TestName";
            scriptSettings.CreationDate = DateTime.Now.ToUniversalTime();
            scriptSettings.LastModifiedDate = DateTime.Now.ToUniversalTime();
            scriptSettings.Owner = StackHashScriptOwner.System;
            scriptSettings.RunAutomatically = true;
            scriptSettings.Version = 2;
            scriptSettings.Save(testScriptName);

            // And load in again and compare.
            StackHashScriptSettings loadedSettings = StackHashScriptSettings.Load(testScriptName);


            Assert.AreEqual(scriptSettings.CreationDate, loadedSettings.CreationDate);
            Assert.AreEqual(scriptSettings.LastModifiedDate, loadedSettings.LastModifiedDate);
            Assert.AreEqual(scriptSettings.Name, loadedSettings.Name);
            Assert.AreEqual(scriptSettings.Script.Count, loadedSettings.Script.Count);
            Assert.AreEqual(scriptSettings.RunAutomatically, loadedSettings.RunAutomatically);
            Assert.AreEqual(scriptSettings.Version, loadedSettings.Version);
            Assert.AreEqual(scriptSettings.Owner, loadedSettings.Owner);

            for (int i = 0; i < scriptSettings.Script.Count; i++)
            {
                Assert.AreEqual(scriptSettings.Script[i].Command, loadedSettings.Script[i].Command);
                Assert.AreEqual(scriptSettings.Script[i].Comment, loadedSettings.Script[i].Comment);
            }
        }

        [TestMethod]
        public void ScriptSaveLoadNoCommand()
        {
            scriptSaveLoadNCommands(0, true);
        }
        [TestMethod]
        public void ScriptSaveLoadOneCommands()
        {
            scriptSaveLoadNCommands(1, true);
        }
        [TestMethod]
        public void ScriptSaveLoad100Commands()
        {
            scriptSaveLoadNCommands(100, true);
        }
        [TestMethod]
        public void ScriptSaveLoadOneCommandNoComment()
        {
            scriptSaveLoadNCommands(1, false);
        }

        [TestMethod]
        public void ScriptContainsSymAndExePathStatement()
        {
            String testScriptName = "Name";
            String symbolPath = "some symbol path **";
            String binaryPath = "some exe path **";
            String sourcePath = "some src path **";
            String testCommand1 = @".exepath " + binaryPath;
            String testComment1 = @"Another comment";
            String testCommand2 = @" .symPath " + symbolPath;
            String testComment2 = @"Demo Comment 1";
            String testCommand3 = @" .SrcPath " + sourcePath;
            String testComment3 = @"Demo Comment 2";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            script.Add(new StackHashScriptLine(testCommand3, testComment3));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String symPath = null;
            String exePath = null;
            String srcPath = null;
            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(symbolPath, symPath);
            Assert.AreEqual(binaryPath, exePath);
            Assert.AreEqual(sourcePath, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            // Should contain an entry for sympath and exepath.
            Assert.AreEqual(10, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));

            Assert.AreEqual(true, allLines[2].Contains("Command"));
            Assert.AreEqual(true, allLines[2].Contains(testCommand1));
            Assert.AreEqual(true, allLines[2].Contains(testComment1));
            Assert.AreEqual(true, allLines[3].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[3].Contains(testCommand1));
            Assert.AreEqual(true, allLines[3].Contains(testComment1));

            Assert.AreEqual(true, allLines[4].Contains("Command"));
            Assert.AreEqual(true, allLines[4].Contains(testCommand2));
            Assert.AreEqual(true, allLines[4].Contains(testComment2));
            Assert.AreEqual(true, allLines[5].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[5].Contains(testCommand2));
            Assert.AreEqual(true, allLines[5].Contains(testComment2));

            Assert.AreEqual(true, allLines[6].Contains("Command"));
            Assert.AreEqual(true, allLines[6].Contains(testCommand3));
            Assert.AreEqual(true, allLines[6].Contains(testComment3));
            Assert.AreEqual(true, allLines[7].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[7].Contains(testCommand3));
            Assert.AreEqual(true, allLines[7].Contains(testComment3));

            Assert.AreEqual(true, allLines[8].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[9].Contains(".logclose"));
        }

        [TestMethod]
        public void ScriptContainsSymAndExePathStatementNoConcat()
        {
            String testScriptName = "Name";
            String symbolPath = "some symbol path **";
            String binaryPath = "some exe path **";
            String sourcePath = "some src path **";
            String testCommand1 = @".exepath " + binaryPath;
            String testComment1 = @"Another comment";
            String testCommand2 = @" .symPath " + symbolPath;
            String testComment2 = @"Demo Comment 1";
            String testCommand3 = @" .SrcPath " + sourcePath;
            String testComment3 = @"Demo Comment 2";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            script.Add(new StackHashScriptLine(testCommand3, testComment3));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String originalSymPath = "c:\\symbols";
            String originalExePath = "c:\\binary";
            String originalSrcPath = "c:\\source";
            String symPath = originalSymPath;
            String exePath = originalExePath;
            String srcPath = originalSrcPath;

            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(symbolPath, symPath);
            Assert.AreEqual(binaryPath, exePath);
            Assert.AreEqual(sourcePath, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            // Should contain an entry for sympath and exepath.
            Assert.AreEqual(10, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains("Command"));
            Assert.AreEqual(true, allLines[2].Contains(testCommand1));
            Assert.AreEqual(true, allLines[2].Contains(testComment1));
            Assert.AreEqual(true, allLines[3].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[3].Contains(testCommand1));
            Assert.AreEqual(true, allLines[3].Contains(testComment1));

            Assert.AreEqual(true, allLines[4].Contains("Command"));
            Assert.AreEqual(true, allLines[4].Contains(testCommand2));
            Assert.AreEqual(true, allLines[4].Contains(testComment2));
            Assert.AreEqual(true, allLines[5].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[5].Contains(testCommand2));
            Assert.AreEqual(true, allLines[5].Contains(testComment2));

            Assert.AreEqual(true, allLines[6].Contains("Command"));
            Assert.AreEqual(true, allLines[6].Contains(testCommand3));
            Assert.AreEqual(true, allLines[6].Contains(testComment3));
            Assert.AreEqual(true, allLines[7].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[7].Contains(testCommand3));
            Assert.AreEqual(true, allLines[7].Contains(testComment3));

            Assert.AreEqual(true, allLines[8].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[9].Contains(".logclose"));
        }
        [TestMethod]
        public void ScriptContainsSymAndExePathStatementWithConcat()
        {
            String testScriptName = "Name";
            String symbolPath = "some symbol path **";
            String binaryPath = "some exe path **";
            String sourcePath = "some src path **";
            String testCommand1 = @".exepath+" + binaryPath;
            String testComment1 = @"Another comment";
            String testCommand2 = @" .symPath + " + symbolPath;
            String testComment2 = @"Demo Comment 1";
            String testCommand3 = @" .SrcPath   +" + sourcePath;
            String testComment3 = @"Demo Comment 2";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            script.Add(new StackHashScriptLine(testCommand3, testComment3));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String originalSymPath = "c:\\symbols";
            String originalExePath = "c:\\binary";
            String originalSrcPath = "c:\\source";
            String symPath = originalSymPath;
            String exePath = originalExePath;
            String srcPath = originalSrcPath;

            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(symPath, originalSymPath + ";" + symbolPath);
            Assert.AreEqual(exePath, originalExePath + ";" + binaryPath);
            Assert.AreEqual(srcPath, originalSrcPath + ";" + sourcePath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            // Should contain an entry for sympath and exepath.
            Assert.AreEqual(10, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains("Command"));
            Assert.AreEqual(true, allLines[2].Contains(testCommand1));
            Assert.AreEqual(true, allLines[2].Contains(testComment1));
            Assert.AreEqual(true, allLines[3].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[3].Contains(testCommand1));
            Assert.AreEqual(true, allLines[3].Contains(testComment1));

            Assert.AreEqual(true, allLines[4].Contains("Command"));
            Assert.AreEqual(true, allLines[4].Contains(testCommand2));
            Assert.AreEqual(true, allLines[4].Contains(testComment2));
            Assert.AreEqual(true, allLines[5].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[5].Contains(testCommand2));
            Assert.AreEqual(true, allLines[5].Contains(testComment2));

            Assert.AreEqual(true, allLines[6].Contains("Command"));
            Assert.AreEqual(true, allLines[6].Contains(testCommand3));
            Assert.AreEqual(true, allLines[6].Contains(testComment3));
            Assert.AreEqual(true, allLines[7].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[7].Contains(testCommand3));
            Assert.AreEqual(true, allLines[7].Contains(testComment3));

            Assert.AreEqual(true, allLines[8].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[9].Contains(".logclose"));
        }

        [TestMethod]
        public void ScriptContainsSymPathMultipleCommands()
        {
            String testScriptName = "Name";
            String symbolPath1 = "c:\\test1";
            String symbolPath2 = "c:\\test2";

            String testCommand1 = @".sympath+" + symbolPath1;
            String testComment1 = @"Comment 1";
            String testCommand2 = @".Sympath +" + symbolPath2;
            String testComment2 = @"Comment 2";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String originalSymPath = "c:\\symbols";
            String originalExePath = "c:\\binary";
            String originalSrcPath = "c:\\source";
            String symPath = originalSymPath;
            String exePath = originalExePath;
            String srcPath = originalSrcPath;

            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(originalSymPath + ";" + symbolPath1 + ";" + symbolPath2, symPath);
            Assert.AreEqual(originalExePath, exePath);
            Assert.AreEqual(originalSrcPath, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            // Should contain an entry for sympath and exepath.
            Assert.AreEqual(8, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));

            Assert.AreEqual(true, allLines[2].Contains("Command"));
            Assert.AreEqual(true, allLines[2].Contains(testCommand1));
            Assert.AreEqual(true, allLines[2].Contains(testComment1));
            Assert.AreEqual(true, allLines[3].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[3].Contains(testCommand1));
            Assert.AreEqual(true, allLines[3].Contains(testComment1));

            Assert.AreEqual(true, allLines[4].Contains("Command"));
            Assert.AreEqual(true, allLines[4].Contains(testCommand2));
            Assert.AreEqual(true, allLines[4].Contains(testComment2));
            Assert.AreEqual(true, allLines[5].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[5].Contains(testCommand2));
            Assert.AreEqual(true, allLines[5].Contains(testComment2));
            
            Assert.AreEqual(true, allLines[6].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[7].Contains(".logclose"));
        }


        [TestMethod]
        public void ScriptContainsSymPathMultipleCommandsWithQuotes()
        {
            String testScriptName = "Name";
            String symbolPath1 = "\"c:\\test1\"";
            String symbolPath2 = "c:\\test2";

            String testCommand1 = @".sympath+" + symbolPath1;
            String testComment1 = @"Comment 1";
            String testCommand2 = @".Sympath +" + symbolPath2;
            String testComment2 = @"Comment 2";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String originalSymPath = "c:\\symbols";
            String originalExePath = "c:\\binary";
            String originalSrcPath = "c:\\source";
            String symPath = originalSymPath;
            String exePath = originalExePath;
            String srcPath = originalSrcPath;

            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(originalSymPath + ";" + "c:\\test1" + ";" + symbolPath2, symPath);
            Assert.AreEqual(originalExePath, exePath);
            Assert.AreEqual(originalSrcPath, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            // Should contain an entry for sympath and exepath.
            Assert.AreEqual(8, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains("Command"));
            Assert.AreEqual(true, allLines[2].Contains(testCommand1));
            Assert.AreEqual(true, allLines[2].Contains(testComment1));
            Assert.AreEqual(true, allLines[3].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[3].Contains(testCommand1));
            Assert.AreEqual(true, allLines[3].Contains(testComment1));

            Assert.AreEqual(true, allLines[4].Contains("Command"));
            Assert.AreEqual(true, allLines[4].Contains(testCommand2));
            Assert.AreEqual(true, allLines[4].Contains(testComment2));
            Assert.AreEqual(true, allLines[5].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[5].Contains(testCommand2));
            Assert.AreEqual(true, allLines[5].Contains(testComment2));

            Assert.AreEqual(true, allLines[6].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[7].Contains(".logclose"));
        }
        [TestMethod]
        public void ScriptContainsSymPathMultipleCommandsWithReplace()
        {
            String testScriptName = "Name";
            String symbolPath1 = "c:\\test1";
            String symbolPath2 = "c:\\test2";

            String testCommand1 = @".sympath+" + symbolPath1;
            String testComment1 = @"Comment 1";
            String testCommand2 = @".Sympath" + symbolPath2; // No + on this one so should replace all previous symbols.
            String testComment2 = @"Comment 2";

            // Create a script - empty.
            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand1, testComment1));
            script.Add(new StackHashScriptLine(testCommand2, testComment2));
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            String tempFileName = m_TempPath + "\\GeneratedScript.wds";
            String outputFileName = "c:\\test\\results.log";
            String originalSymPath = "c:\\symbols";
            String originalExePath = "c:\\binary";
            String originalSrcPath = "c:\\source";
            String symPath = originalSymPath;
            String exePath = originalExePath;
            String srcPath = originalSrcPath;

            scriptSettings.GenerateScriptFile(tempFileName, outputFileName, ref symPath, ref exePath, ref srcPath);

            Assert.AreEqual(symbolPath2, symPath);
            Assert.AreEqual(originalExePath, exePath);
            Assert.AreEqual(originalSrcPath, srcPath);

            // Read all of the data in.
            String[] allLines = File.ReadAllLines(tempFileName);

            Assert.AreEqual(true, File.Exists(tempFileName));

            // Should contain the script header comment plus one command and one comment line for each command.
            // Should contain an entry for sympath and exepath.
            Assert.AreEqual(8, allLines.Length);
            Assert.AreEqual(true, allLines[0].Contains(outputFileName));
            Assert.AreEqual(true, allLines[1].Contains(testScriptName));
            Assert.AreEqual(true, allLines[1].Contains(scriptSettings.LastModifiedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true, allLines[2].Contains("Command"));
            Assert.AreEqual(true, allLines[2].Contains(testCommand1));
            Assert.AreEqual(true, allLines[2].Contains(testComment1));
            Assert.AreEqual(true, allLines[3].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[3].Contains(testCommand1));
            Assert.AreEqual(true, allLines[3].Contains(testComment1));

            Assert.AreEqual(true, allLines[4].Contains("Command"));
            Assert.AreEqual(true, allLines[4].Contains(testCommand2));
            Assert.AreEqual(true, allLines[4].Contains(testComment2));
            Assert.AreEqual(true, allLines[5].Contains("CommandEnd"));
            Assert.AreEqual(true, allLines[5].Contains(testCommand2));
            Assert.AreEqual(true, allLines[5].Contains(testComment2));

            Assert.AreEqual(true, allLines[6].Contains("Script Complete"));
            Assert.AreEqual(true, allLines[7].Contains(".logclose"));
        }


        [TestMethod]
        public void ScriptContainsPssCorX86()
        {
            String testScriptName = "Name";
            String testCommand = @".load psscor.dll";
            String testComment = @"loads psscor from correct location";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            scriptSettings.FixUp(StackHashScriptDumpArchitecture.X86, "2.0.2355.2233",  "C:\\test");

            Assert.AreEqual(0, String.Compare(".load C:\\test\\psscor2\\x86\\psscor2.dll", scriptSettings.Script[0].Command, StringComparison.OrdinalIgnoreCase));
       
        }

        [TestMethod]
        public void ScriptContainsPssCorAmd64()
        {
            String testScriptName = "Name";
            String testCommand = @".load psscor.dll";
            String testComment = @"loads psscor from correct location";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            scriptSettings.FixUp(StackHashScriptDumpArchitecture.Amd64, "2.0.2355.2233", "C:\\test");

            Assert.AreEqual(0, String.Compare(".load C:\\test\\psscor2\\amd64\\psscor2.dll", scriptSettings.Script[0].Command, StringComparison.OrdinalIgnoreCase));

        }

        [TestMethod]
        public void ScriptContainsPssCorIa64()
        {
            String testScriptName = "Name";
            String testCommand = @".load psscor.dll";
            String testComment = @"loads psscor from correct location";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            scriptSettings.FixUp(StackHashScriptDumpArchitecture.IA64, "2.0.2355.2233", "C:\\test");

            Assert.AreEqual(0, String.Compare(".load C:\\test\\psscor2\\ia64\\psscor2.dll", scriptSettings.Script[0].Command, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ScriptContainsPssCorX86Clr4()
        {
            String testScriptName = "Name";
            String testCommand = @".load psscor.dll";
            String testComment = @"loads psscor from correct location";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            scriptSettings.FixUp(StackHashScriptDumpArchitecture.X86, "4.0.2355.2233", "C:\\test");

            Assert.AreEqual(0, String.Compare(".load C:\\test\\psscor4\\x86\\psscor4.dll", scriptSettings.Script[0].Command, StringComparison.OrdinalIgnoreCase));

        }

        [TestMethod]
        public void ScriptContainsPssCorAmd64Clr4()
        {
            String testScriptName = "Name";
            String testCommand = @".load psscor.dll";
            String testComment = @"loads psscor from correct location";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            scriptSettings.FixUp(StackHashScriptDumpArchitecture.Amd64, "4.0.2355.2233", "C:\\test");

            Assert.AreEqual(0, String.Compare(".load C:\\test\\psscor4\\amd64\\psscor4.dll", scriptSettings.Script[0].Command, StringComparison.OrdinalIgnoreCase));

        }

        [TestMethod]
        public void ScriptContainsPssCorIa64Clr4()
        {
            String testScriptName = "Name";
            String testCommand = @".load psscor.dll";
            String testComment = @"loads psscor from correct location";

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(testCommand, testComment));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings(testScriptName, script);

            scriptSettings.FixUp(StackHashScriptDumpArchitecture.IA64, "4.0.2355.2233", "C:\\test");

            Assert.AreEqual(0, String.Compare(".load C:\\test\\psscor4\\ia64\\psscor4.dll", scriptSettings.Script[0].Command, StringComparison.OrdinalIgnoreCase));
        }
    }
}
