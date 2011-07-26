using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;

using StackHashBusinessObjects;
using StackHashDebug;
using StackHashTasks;
using StackHashErrorIndex;
using StackHashUtilities;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for ScriptResultsManagerUnitTests
    /// </summary>
    [TestClass]
    public class ScriptResultsManagerUnitTests
    {
        private String m_TempPath;
        private SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public ScriptResultsManagerUnitTests()
        {
            m_TempPath = Path.GetTempPath() + "StackHashTasksUnitTests";
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\StackHash_TestCabs";

            if (!Directory.Exists(m_RootCabFolder))
                Directory.CreateDirectory(m_RootCabFolder);
            copyPsscorInstallData();

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
            tidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
        }

        private void tidyTest()
        {
            if (Directory.Exists(m_TempPath))
            {
                PathUtils.SetFilesWritable(m_TempPath, true);
                PathUtils.DeleteDirectory(m_TempPath, true);
            }
        }

        
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Index != null)
            {
                SqlConnection.ClearAllPools();

                m_Index.Deactivate();
                m_Index.DeleteIndex();
                m_Index.Dispose();
                m_Index = null;
            }

            if (Directory.Exists(m_RootCabFolder))
                PathUtils.DeleteDirectory(m_RootCabFolder, true);
            SqlConnection.ClearAllPools();
            tidyTest();
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
        //
        #endregion

        [TestMethod]
        public void Constructor()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(xmlErrorIndex, scriptManager, debugger, debuggerSettings);
        }

        [TestMethod]
        public void RunScriptInvalidProduct()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.txt", 4, 1000);
            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = m_TempPath + "\\script.scr";
            bool extractCab = false;
            bool forceRun = true;

            try
            {
                scriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (System.ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
            }
        }

        [TestMethod]
        public void RunScriptInvalidFile()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.txt", 4, 1000);

            cacheIndex.AddProduct(product);

            
            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = m_TempPath + "\\script.scr";
            bool extractCab = false;
            bool forceRun = true;

            try
            {
                scriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (System.ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
            }
        }
        [TestMethod]
        public void RunScriptInvalidEvent()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.txt", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);


            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = m_TempPath + "\\script.scr";
            bool extractCab = false;
            bool forceRun = true;

            try
            {
                scriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (System.ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
            }
        }

        [TestMethod]
        public void RunScriptInvalidCab()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.txt", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = m_TempPath + "\\script.scr";
            bool extractCab = true;
            bool forceRun = true;

            try
            {
                scriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.CabDoesNotExist, ex.ServiceErrorCode);
            }
        }

        [TestMethod]
        public void RunScriptUnWrapCabNoScript()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = "autoscript2";
            bool extractCab = true;
            bool forceRun = true;

            try
            {
                scriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.ScriptDoesNotExist, ex.ServiceErrorCode);
            }
        }
        [TestMethod]
        public void RunScriptNoUnwrapNoDmpFile()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = "autoscript";
            bool extractCab = false;
            bool forceRun = true;

            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, dumpFileName, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void RunScriptUnwrapDumpExistsSimpleScript()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = "autoscript";
            bool extractCab = true;
            bool forceRun = true;

            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual("AutoScript", result.Name);

        }

        [TestMethod]
        public void RunScriptCorruptCab()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);
            cab.CabDownloaded = true;

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            // Now corrupt the cab file.
            FileStream cabStream = new FileStream(cabFileName, FileMode.Open, FileAccess.ReadWrite);

            try
            {
                cabStream.Seek(cabStream.Length / 2, SeekOrigin.Begin);
                cabStream.WriteByte(0xff);
            }
            finally
            {
                cabStream.Close();
            }

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = "autoscript";
            bool extractCab = true;
            bool forceRun = true;

            try
            {
                StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.CabIsCorrupt, ex.ServiceErrorCode);

                Assert.AreEqual(false, File.Exists(cabFileName));

                StackHashCab newCab = cacheIndex.GetCab(product, file, theEvent, cab.Id);
                Assert.AreEqual(false, newCab.CabDownloaded);
            }
        }

        
        [TestMethod]
        public void RunScriptError()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            String fog543SymbolPath =
                @"SRV*C:\Websymbols*http://msdl.microsoft.com/download/symbols;\\ssd\shares\Dev\Builds\CounterSpy\Consumer\4.0\3282-HF3\BuildOutput\Symbol;\\ssd\shares\Dev\Builds\Network Security Tools\Sunbelt Firewall SDK\5.0\5.0.2276\symbols;""\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core Engine\3.9.x\3.9.2424\Bin"";\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core something\3.2\bin";


            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(fog543SymbolPath);
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(fog543SymbolPath);
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);
            cab.CabDownloaded = true;

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = "autoscript";
            bool extractCab = true;
            bool forceRun = true;

            try
            {
                StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(true, ex.Message.StartsWith("Debugger Error: "));
                Assert.AreEqual(StackHashServiceErrorCode.DebuggerError, ex.ServiceErrorCode);

                // Cab should still exist.
                Assert.AreEqual(true, File.Exists(cabFileName));

                StackHashCab newCab = cacheIndex.GetCab(product, file, theEvent, cab.Id);
                Assert.AreEqual(true, newCab.CabDownloaded);
            }
        }

        [TestMethod]
        public void RunScriptLongSymbolPath()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            String fog543SymbolPath =
                @"SRV*C:\Websymbols*http://msdl.microsoft.com/download/symbols;\\ssd\shares\Dev\Builds\CounterSpy\Consumer\4.0\3282-HF3\BuildOutput\Symbol;\\ssd\shares\Dev\Builds\Network Security Tools\Sunbelt Firewall SDK\5.0\5.0.2276\symbols;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core Engine\3.9.x\3.9.2424\Bin;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core something\3.2\bin";


            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(fog543SymbolPath);
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(fog543SymbolPath);
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            String scriptName = "autoscript";
            bool extractCab = true;
            bool forceRun = true;

            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);
            Assert.AreEqual("AutoScript", result.Name);
        }



        [TestMethod]
        public void LoadScriptResult_RunDateIsInvariantCultureV2()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            String fog543SymbolPath =
                @"SRV*C:\Websymbols*http://msdl.microsoft.com/download/symbols;\\ssd\shares\Dev\Builds\CounterSpy\Consumer\4.0\3282-HF3\BuildOutput\Symbol;\\ssd\shares\Dev\Builds\Network Security Tools\Sunbelt Firewall SDK\5.0\5.0.2276\symbols;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core Engine\3.9.x\3.9.2424\Bin;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core something\3.2\bin";


            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(fog543SymbolPath);
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(fog543SymbolPath);
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";

            // Create the analysis folder for the results files.
            String resultsFolder = Path.Combine(cabFileFolder, "analysis");
            if (!Directory.Exists(resultsFolder))
                Directory.CreateDirectory(resultsFolder);

            TestManager.CreateScriptFile(99, 1000, resultsFolder, 123, CultureInfo.InvariantCulture, 2, 0);

            StackHashScriptResultFiles allResults = scriptResultsManager.GetResultFiles(product, file, theEvent, cab);
            Assert.AreEqual(1, allResults.Count);

            foreach (StackHashScriptResultFile resultFile in allResults)
            {
                StackHashScriptResult resultData = scriptResultsManager.GetResultFileData(product, file, theEvent, cab, resultFile.ScriptName);

                if (resultData.Name.Contains("99"))
                {
                    // Check the date is as expected. Test manager creates a last mod date of yesterday.
                    Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Date, resultData.LastModifiedDate.Date);
                    Assert.AreEqual(DateTime.UtcNow.Date, resultData.RunDate.Date);
                }
            }

        }

        [TestMethod]
        public void LoadScriptResult_RunDateIsCurrentCulture()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            String fog543SymbolPath =
                @"SRV*C:\Websymbols*http://msdl.microsoft.com/download/symbols;\\ssd\shares\Dev\Builds\CounterSpy\Consumer\4.0\3282-HF3\BuildOutput\Symbol;\\ssd\shares\Dev\Builds\Network Security Tools\Sunbelt Firewall SDK\5.0\5.0.2276\symbols;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core Engine\3.9.x\3.9.2424\Bin;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core something\3.2\bin";


            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(fog543SymbolPath);
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(fog543SymbolPath);
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";

            // Create the analysis folder for the results files.
            String resultsFolder = Path.Combine(cabFileFolder, "analysis");
            if (!Directory.Exists(resultsFolder))
                Directory.CreateDirectory(resultsFolder);

            TestManager.CreateScriptFile(99, 1000, resultsFolder, 123, CultureInfo.CurrentCulture, 1, 0);

            StackHashScriptResultFiles allResults = scriptResultsManager.GetResultFiles(product, file, theEvent, cab);
            Assert.AreEqual(1, allResults.Count);

            foreach (StackHashScriptResultFile resultFile in allResults)
            {
                StackHashScriptResult resultData = scriptResultsManager.GetResultFileData(product, file, theEvent, cab, resultFile.ScriptName);

                if (resultData.Name.Contains("99"))
                {
                    // Check the date is as expected. Test manager creates a last mod date of yesterday.
                    Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Date, resultData.LastModifiedDate.Date);
                    Assert.AreEqual(DateTime.UtcNow.Date, resultData.RunDate.Date);
                }
            }

        }


        [TestMethod]
        public void LoadScriptResult_RunDateIsUnknownCulture()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            String fog543SymbolPath =
                @"SRV*C:\Websymbols*http://msdl.microsoft.com/download/symbols;\\ssd\shares\Dev\Builds\CounterSpy\Consumer\4.0\3282-HF3\BuildOutput\Symbol;\\ssd\shares\Dev\Builds\Network Security Tools\Sunbelt Firewall SDK\5.0\5.0.2276\symbols;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core Engine\3.9.x\3.9.2424\Bin;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core something\3.2\bin";


            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(fog543SymbolPath);
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(fog543SymbolPath);
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";

            // Create the analysis folder for the results files.
            String resultsFolder = Path.Combine(cabFileFolder, "analysis");
            if (!Directory.Exists(resultsFolder))
                Directory.CreateDirectory(resultsFolder);

            CultureInfo culture = new CultureInfo("el-GR");
            TestManager.CreateScriptFile(99, 1000, resultsFolder, 123, culture, 1, 0);

            StackHashScriptResultFiles allResults = scriptResultsManager.GetResultFiles(product, file, theEvent, cab);
            Assert.AreEqual(1, allResults.Count);

            foreach (StackHashScriptResultFile resultFile in allResults)
            {
                StackHashScriptResult resultData = scriptResultsManager.GetResultFileData(product, file, theEvent, cab, resultFile.ScriptName);

                if (resultData.Name.Contains("99"))
                {
                    // Check the date is as expected. Test manager creates a last mod date of yesterday.
                    Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Date, resultData.LastModifiedDate.Date);
                    Assert.AreEqual(DateTime.UtcNow.Date, resultData.RunDate.Date);
                }
            }

        }

        [TestMethod]
        public void LoadScriptResult_RunDateIsUnknownCultureV2()
        {
            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);
            Windbg debugger = new Windbg();

            String fog543SymbolPath =
                @"SRV*C:\Websymbols*http://msdl.microsoft.com/download/symbols;\\ssd\shares\Dev\Builds\CounterSpy\Consumer\4.0\3282-HF3\BuildOutput\Symbol;\\ssd\shares\Dev\Builds\Network Security Tools\Sunbelt Firewall SDK\5.0\5.0.2276\symbols;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core Engine\3.9.x\3.9.2424\Bin;\\ssd\shares\Dev\Builds\CounterSpy\VIPRE Core something\3.2\bin";


            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(fog543SymbolPath);
            debuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;
            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(fog543SymbolPath);
            debuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            XmlErrorIndex xmlErrorIndex = new XmlErrorIndex(errorIndexPath, "TestIndex");
            ErrorIndexCache cacheIndex = new ErrorIndexCache(xmlErrorIndex);

            cacheIndex.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(cacheIndex, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 3, "CLR Managed crash", "cab.cab", 4, 1000);

            cacheIndex.AddProduct(product);
            cacheIndex.AddFile(product, file);
            cacheIndex.AddEvent(product, file, theEvent);
            cacheIndex.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = cacheIndex.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = cacheIndex.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";

            // Create the analysis folder for the results files.
            String resultsFolder = Path.Combine(cabFileFolder, "analysis");
            if (!Directory.Exists(resultsFolder))
                Directory.CreateDirectory(resultsFolder);

            CultureInfo culture = new CultureInfo("el-GR");
            TestManager.CreateScriptFile(99, 1000, resultsFolder, 123, culture, 2, 0);

            StackHashScriptResultFiles allResults = scriptResultsManager.GetResultFiles(product, file, theEvent, cab);
            Assert.AreEqual(1, allResults.Count);

            foreach (StackHashScriptResultFile resultFile in allResults)
            {
                StackHashScriptResult resultData = scriptResultsManager.GetResultFileData(product, file, theEvent, cab, resultFile.ScriptName);

                if (resultData.Name.Contains("99"))
                {
                    // Check the date is as expected. Test manager creates a last mod date of yesterday.
                    Assert.AreEqual(DateTime.UtcNow.Date.AddDays(-1).Date, resultData.LastModifiedDate.Date);
                    Assert.AreEqual(DateTime.UtcNow.Date, resultData.RunDate.Date);
                }
            }

        }

        private void copyPsscorInstallData()
        {
            String installFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (File.Exists(Path.Combine(installFolder, "psscor2\\ia64\\psscor2.dll")))
                return;

            Directory.CreateDirectory(Path.Combine(installFolder, "psscor2\\x86"));
            Directory.CreateDirectory(Path.Combine(installFolder, "psscor2\\amd64"));
            Directory.CreateDirectory(Path.Combine(installFolder, "psscor2\\ia64"));
            File.Copy("r:\\3rdparty\\psscor2\\x86\\psscor2.dll", Path.Combine(installFolder, "psscor2\\x86\\psscor2.dll"), true);
            File.Copy("r:\\3rdparty\\psscor2\\amd64\\psscor2.dll", Path.Combine(installFolder, "psscor2\\amd64\\psscor2.dll"), true);
            File.Copy("r:\\3rdparty\\psscor2\\ia64\\psscor2.dll", Path.Combine(installFolder, "psscor2\\ia64\\psscor2.dll"), true);
            Directory.CreateDirectory(Path.Combine(installFolder, "psscor4\\x86"));
            Directory.CreateDirectory(Path.Combine(installFolder, "psscor4\\amd64"));
            File.Copy("r:\\3rdparty\\psscor4\\x86\\psscor4.dll", Path.Combine(installFolder, "psscor4\\x86\\psscor4.dll"), true);
            File.Copy("r:\\3rdparty\\psscor4\\amd64\\psscor4.dll", Path.Combine(installFolder, "psscor4\\amd64\\psscor4.dll"), true);
        }


        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2Amd64_CLR20()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy64Managed2.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("2."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("amd64"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor2"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }

        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2Amd64_CLR30()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy64Managed3.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("2."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("amd64"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor2"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }


        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2Amd64_CLR35()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy64Managed3_5.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("2."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("amd64"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor2"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }

        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2Amd64_CLR4()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy64Managed4.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("4."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("amd64"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor4"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message") || resultLine.Contains("There is no current managed exception on this thread"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }


        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2X86_CLR2()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy32Managed2.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("2."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("x86"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor2"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }

        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2X86_CLR3()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy32Managed3.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("2."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("x86"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor2"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }

        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2X86_CLR3_5()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy32Managed3_5.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("2."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("x86"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor2"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }
        [TestMethod]
        [Ignore]
        public void RunScriptLoadPsscor2X86_CLR4()
        {
            if (!SystemInformation.Is64BitSystem())
                return;

            String errorIndexPath = m_TempPath;
            ScriptManager scriptManager = new ScriptManager(m_TempPath);

            StackHashScript script = new StackHashScript();
            script.Add(new StackHashScriptLine(".load psscor.dll", "Test for loading psscor.dll"));
            script.Add(new StackHashScriptLine("!pe", "print exception"));

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings("psscor", script);
            scriptManager.AddScript(scriptSettings, true);

            Windbg debugger = new Windbg();

            StackHashDebuggerSettings debuggerSettings = new StackHashDebuggerSettings();

            String localStore = "SRV*" + m_TempPath + @"*http://msdl.microsoft.com/download/symbols";
            debuggerSettings.BinaryPath = new StackHashSearchPath();
            debuggerSettings.BinaryPath.Add(localStore);

            debuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            debuggerSettings.BinaryPath64Bit.Add(localStore);

            debuggerSettings.SymbolPath = new StackHashSearchPath();
            debuggerSettings.SymbolPath.Add(localStore);

            debuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            debuggerSettings.SymbolPath64Bit.Add(localStore);

            debuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
            debuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;

            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            ScriptResultsManager scriptResultsManager = new ScriptResultsManager(m_Index, scriptManager, debugger, debuggerSettings);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "files.com", 1, "Product1", 1, 1, "1.0.2.4");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 2, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "CLR20 Managed Crash", 3, new StackHashEventSignature(), 1, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, theEvent.Id, theEvent.EventTypeName, "Crashy64Managed.dmp", 4, 1000);

            m_Index.AddProduct(product);
            m_Index.AddFile(product, file);
            m_Index.AddEvent(product, file, theEvent);
            m_Index.AddCab(product, file, theEvent, cab, false);

            String cabFileFolder = m_Index.GetCabFolder(product, file, theEvent, cab);
            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            if (!Directory.Exists(cabFileFolder))
                Directory.CreateDirectory(cabFileFolder);

            // Create a dump file.
            File.Copy(TestSettings.TestDataFolder + @"dumps\Crashy32Managed4.dmp", cabFileName);
            FileAttributes fileAttributes = File.GetAttributes(cabFileName);
            File.SetAttributes(cabFileName, fileAttributes & ~FileAttributes.ReadOnly);

            String dumpFileName = m_TempPath + "\\dump.dmp";
            bool extractCab = false; // Just a dump file.
            bool forceRun = true;
            String scriptName = "psscor";


            StackHashScriptResult result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, "AutoScript", extractCab, new StackHashClientData(), forceRun);

            AutoScriptBase autoScript = new AutoScript(m_TempPath);
            cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, result);

            Assert.AreEqual(true, cab.DumpAnalysis.DotNetVersion.StartsWith("4."));

            result = scriptResultsManager.RunScript(product, file, theEvent, cab, null, scriptName, extractCab, new StackHashClientData(), forceRun);
            Assert.AreNotEqual(null, result);

            Assert.AreEqual(scriptName, result.Name);

            Assert.AreEqual(2, result.ScriptResults.Count);
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("x86"));
            Assert.AreEqual(true, result.ScriptResults[0].ScriptLine.Command.Contains("psscor4"));

            bool messageFound = false;
            foreach (String resultLine in result.ScriptResults[1].ScriptLineOutput)
            {
                if (resultLine.Contains("Some message"))
                    messageFound = true;
            }

            Assert.AreEqual(true, messageFound);
        }
    }
}
