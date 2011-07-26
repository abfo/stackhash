using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;


namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for DebuggerSettingsUnitTests
    /// </summary>
    [TestClass]
    public class DebuggerSettingsUnitTests
    {
        public DebuggerSettingsUnitTests()
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
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void GetUnquotedPathFromCommandLineNullCommandLine()
        {
            try
            {
                StackHashScriptSettings.GetUnquotedPathFromCommandLine(null, null, null);
            }
            catch (System.ArgumentNullException ex)
            {
                Assert.AreEqual("commandLine", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void GetUnquotedPathFromCommandLineNullCommand()
        {
            try
            {
                StackHashScriptSettings.GetUnquotedPathFromCommandLine(null, ".sympath", null);
            }
            catch (System.ArgumentNullException ex)
            {
                Assert.AreEqual("command", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineNoParameterOriginalNull()
        {
            String path = "";
            String command = ".sympath";
            String commandLine = command + " " + path;
            String originalPath = null;

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            Assert.AreEqual(null, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineParameterOriginalNull()
        {
            String path = "c:\\test";
            String command = ".sympath";
            String commandLine = command + " " + path;
            String originalPath = null;

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            Assert.AreEqual(path, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineNoParameterOriginalNonNull()
        {
            String path = "          ";
            String command = ".sympath";
            String commandLine = command + " " + path;
            String originalPath = "C:\\originalpath";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            Assert.AreEqual(originalPath, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineParamOriginalNonNullNoConcat()
        {
            String path = "c:\\testpath";
            String command = ".sympath";
            String commandLine = command + " " + path;
            String originalPath = "C:\\originalpath";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            Assert.AreEqual(path, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineParamOriginalNullConcat()
        {
            String path = "c:\\testpath";
            String command = ".sympath +";
            String commandLine = command + " " + path;
            String originalPath = null;

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            Assert.AreEqual(path, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineParamOriginalNonNullConcat()
        {
            String path = "c:\\testpath";
            String command = ".sympath +";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            String expectedResult = originalPath + ";" + path;
            Assert.AreEqual(expectedResult, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineQuotes()
        {
            String path = "\"c:\\testpath\"";
            String command = ".sympath +";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            String expectedResult = originalPath + ";" + "c:\\testpath";
            Assert.AreEqual(expectedResult, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandLineQuotesNoConcat()
        {
            String path = "\"c:\\testpath\"";
            String command = ".sympath   ";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            String expectedResult = "c:\\testpath";
            Assert.AreEqual(expectedResult, outPath);
        }


        [TestMethod]
        public void GetUnquotedPathFromCommandLineQuotesAndSpaceAndCase()
        {
            String path = "\"c:\\testpath;z:\\stuff\"";
            String command = "     .SymPath    +    ";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");

            String expectedResult = originalPath + ";" + "c:\\testpath;z:\\stuff";
            Assert.AreEqual(expectedResult, outPath);
        }
        [TestMethod]
        public void GetUnquotedPathFromCommandCommandMismatch()
        {
            String path = "\"c:\\testpath;z:\\stuff\"";
            String command = "     .SymPath    +    ";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".srcpath");

            Assert.AreEqual(originalPath, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandCalledTwiceConcat()
        {
            String path = "c:\\path";
            String command = "     .SymPath    +    ";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");
            outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(outPath, commandLine, ".sympath");

            Assert.AreEqual(originalPath + ";" + path + ";" + path, outPath);
        }

        [TestMethod]
        public void GetUnquotedPathFromCommandCalledTwiceNoConcat()
        {
            String path = "c:\\path";
            String command = "     .SymPath        ";
            String commandLine = command + " " + path;
            String originalPath = "c:\\original";

            String outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(originalPath, commandLine, ".sympath");
            outPath = StackHashScriptSettings.GetUnquotedPathFromCommandLine(outPath, commandLine, ".sympath");

            Assert.AreEqual(path, outPath);
        }
    }
}
