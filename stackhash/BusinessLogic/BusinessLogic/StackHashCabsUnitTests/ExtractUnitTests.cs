using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashCabs;
using StackHashUtilities;

namespace StackHashCabsUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExtractUnitTests
    {
        String m_TempPath;

        public ExtractUnitTests()
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
            m_TempPath = Path.GetTempPath() + "StackHashCabs";

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
        public void ExtractTestCab()
        {
            String testCabName = TestSettings.TestDataFolder + @"cabs\1641909485-Crash32bit-0773522646.cab";

            Cabs.ExtractCab(testCabName, m_TempPath);

            Assert.AreEqual(true, File.Exists(m_TempPath + "\\cuckusrv.exe.mdmp"));
            Assert.AreEqual(true, File.Exists(m_TempPath + "\\version.txt"));
        }

        [TestMethod]
        public void ExtractSameCabTwiceToSameLocation()
        {
            String testCabName = TestSettings.TestDataFolder + @"cabs\1641909485-Crash32bit-0773522646.cab";

            Cabs.ExtractCab(testCabName, m_TempPath);
            DateTime lastModifiedTime = File.GetLastWriteTime(m_TempPath + "\\cuckusrv.exe.mdmp");

            Thread.Sleep(2000);

            Cabs.ExtractCab(testCabName, m_TempPath);
            DateTime lastModifiedTime2 = File.GetLastWriteTime(m_TempPath + "\\cuckusrv.exe.mdmp");

            Assert.AreEqual(true, File.Exists(m_TempPath + "\\cuckusrv.exe.mdmp"));
            Assert.AreEqual(true, File.Exists(m_TempPath + "\\version.txt"));

            Assert.AreEqual(lastModifiedTime, lastModifiedTime2);
        }
    }
}
