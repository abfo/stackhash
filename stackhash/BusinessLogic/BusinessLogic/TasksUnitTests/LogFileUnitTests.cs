using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashUtilities;
using StackHashBusinessObjects;
using StackHashTasks;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for LogFileUnitTests
    /// </summary>
    [TestClass]
    public class LogFileUnitTests
    {
        public LogFileUnitTests()
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
            String expectedLogFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "StackHash\\test\\logs");

            deleteLogFiles(expectedLogFilePath);
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
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private void deleteLogFiles(String folder)
        {
            String [] files = Directory.GetFiles(folder, "*.txt");

            foreach (String file in files)
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public void Constructor_CheckFolder_TestMode()
        {
            LogManager logManager = new LogManager(true, true);

            String logFilePath = Path.GetDirectoryName(logManager.LogFileName);
            String expectedLogFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "StackHash\\test\\logs");

            Assert.AreEqual(0, String.Compare(logFilePath, expectedLogFilePath, StringComparison.OrdinalIgnoreCase));
            logManager.Dispose();
        }

        [TestMethod]
        public void Constructor_CheckFolder_ProductMode()
        {
            LogManager logManager = new LogManager(true, false);

            String logFilePath = Path.GetDirectoryName(logManager.LogFileName);
            String expectedLogFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "StackHash\\logs");

            Assert.AreEqual(0, String.Compare(logFilePath, expectedLogFilePath, StringComparison.OrdinalIgnoreCase));
            logManager.Dispose();
        }

        
        [TestMethod]
        public void Constructor_TestMode_GetLogFileName_NoFiles()
        {
            LogManager logManager = new LogManager(true, true);

            String logFileName = Path.GetFileName(logManager.LogFileName);

            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));
            logManager.Dispose();
        }

        [TestMethod]
        public void StartLogging_TestMode_NoLogFiles()
        {
            LogManager logManager = new LogManager(true, true);

            String logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            logManager.StartLogging(true);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            String allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test"));
            logManager.Dispose();
        }


        /// <summary>
        /// Call start twice (by user). Should create the a new log file.
        /// </summary>
        [TestMethod]
        public void StartLogging_TestMode_CallStartTwice()
        {
            LogManager logManager = new LogManager(true, true);

            String logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            logManager.StartLogging(true);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 1");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            String allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test 1"));

            // Start the logger again as if by the user.
            logManager.StartLogging(true);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 2");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000002.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test 2"));
            Assert.AreEqual(false, allText.Contains("Log Manager Test 1"));
            logManager.Dispose();

        }


        /// <summary>
        /// Call start twice by system. Should create only 1 log file.
        /// </summary>
        [TestMethod]
        public void StartLogging_TestMode_CallStartTwiceBySystem()
        {
            LogManager logManager = new LogManager(true, true);

            String logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            logManager.StartLogging(false);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 1");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            String allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test 1"));

            // Start the logger again as if by the user.
            logManager.StartLogging(false);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 2");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test 2"));
            Assert.AreEqual(true, allText.Contains("Log Manager Test 1"));
            logManager.Dispose();
        }


        /// <summary>
        /// Create file by system then load new logger and start by system.
        /// </summary>
        [TestMethod]
        public void StartLogging_TestMode_CallStartTwiceBySystem_Reload()
        {
            LogManager logManager = new LogManager(true, true);

            String logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            logManager.StartLogging(false);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 1");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            logManager.Dispose();

            logManager = new LogManager(true, true);

            String allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test 1"));

            // Start the logger again as if by the user.
            logManager.StartLogging(false);

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 2");
            logFileName = Path.GetFileName(logManager.LogFileName);
            Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_00000001.txt", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(true, File.Exists(logManager.LogFileName));
            logManager.StopLogging();

            allText = File.ReadAllText(logManager.LogFileName);

            Assert.AreEqual(true, allText.Contains("Log Manager Test 2"));
            Assert.AreEqual(true, allText.Contains("Log Manager Test 1"));

            logManager.Dispose();
        }

        /// <summary>
        /// Call 100 times by user.
        /// </summary>
        [TestMethod]
        public void StartLogging_TestMode_CallStartNTimer()
        {
            LogManager logManager = new LogManager(true, true);

            for (int i = 1; i < 100; i++)
            {
                logManager.StartLogging(true);

                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Log Manager Test 1");
                String logFileName = Path.GetFileName(logManager.LogFileName);
                Assert.AreEqual(0, String.Compare(logFileName, "StackHashServiceDiagnosticsLog_" + i.ToString("D8") + ".txt", StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(true, File.Exists(logManager.LogFileName));
                logManager.StopLogging();

                String allText = File.ReadAllText(logManager.LogFileName);

                Assert.AreEqual(true, allText.Contains("Log Manager Test 1"));
            }

            logManager.Dispose();

        }
    }
}
