using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

using StackHashBusinessObjects;
using StackHashWinQual;
using StackHashErrorIndex;
using StackHashUtilities;

namespace WinQualUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SyncUnitTests
    {
        private String m_TempPath;

        public SyncUnitTests()
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
            m_TempPath = Path.Combine(Path.GetTempPath(), "StackHashWinQualTests");

            tidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            tidyTest();
        }

        private void tidyTest()
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
        public void RunWinQualSync2()
        {
            DateTime start = DateTime.Now;

            XmlErrorIndex index = new XmlErrorIndex("z:\\stackhash2", "TestIndex");
            index.Activate();

            IWinQualServices winQual = new WinQualAtomFeedServices(null, 1, 100000, 1, 1, TestSettings.UseWindowsLiveId, 11);
            winQual.LogOn(TestSettings.WinQualUserName, TestSettings.WinQualPassword);
            winQual.SynchronizeWithWinQualOnline(index, true, null);
            DateTime end = DateTime.Now;

            TimeSpan duration = end - start;
            Console.WriteLine(duration);
        }


        [TestMethod]
        [Ignore]
        public void TestSyncTimeConversion()
        {
            DummyIndex index = new DummyIndex();
            index.Activate();

            IWinQualServices winQual = new WinQualAtomFeedServices(null, 1, 100000, 1, 1, TestSettings.UseWindowsLiveId, 11);
            winQual.ProductsToSynchronize = new StackHashProductSyncDataCollection();
            winQual.ProductsToSynchronize.Add(new StackHashProductSyncData(26066));
            winQual.LogOn(TestSettings.WinQualUserName, TestSettings.WinQualPassword);
            winQual.SynchronizeWithWinQualOnline(index, false, null);
        }

        [TestMethod]
        [Ignore]
        public void Run2WinQualSyncsAtSameTime()
        {
            XmlErrorIndex index = new XmlErrorIndex("z:\\stackhash", "TestIndex");
            index.Activate();

            Thread syncThread = new Thread(delegate()
                {
                    IWinQualServices winQual = new WinQualAtomFeedServices(null, 1, 100000, 1, 1, TestSettings.UseWindowsLiveId, 11);
                    winQual.LogOn(TestSettings.WinQualUserName, TestSettings.WinQualPassword);
                    winQual.SynchronizeWithWinQualOnline(index, true, null);
                });

            syncThread.Start();

            Thread.Sleep(1000);
            Thread syncThread2 = new Thread(delegate()
            {
                IWinQualServices winQual = new WinQualAtomFeedServices(null, 1, 100000, 1, 1, TestSettings.UseWindowsLiveId, 11);
                winQual.LogOn(TestSettings.WinQualUserName, TestSettings.WinQualPassword);
                winQual.SynchronizeWithWinQualOnline(index, true, null);
            });
            syncThread2.Start();

            syncThread.Join();
            syncThread2.Join();

        }


        [TestMethod]
        [Ignore]
        public void RunUsingSameLoginContextAfterTimeout()
        {
            XmlErrorIndex index = new XmlErrorIndex("z:\\stackhash", "TestIndex");
            index.Activate();

            IWinQualServices winQual = new WinQualAtomFeedServices(null, 1, 100000, 1, 1, TestSettings.UseWindowsLiveId, 11);
            
            Assert.AreEqual(true, winQual.LogOn(TestSettings.WinQualUserName, TestSettings.WinQualPassword));
            winQual.SynchronizeWithWinQualOnline(index, true, null);

            Thread.Sleep(30 * 60 * 1000);

            Assert.AreEqual(true, winQual.LogOn(TestSettings.WinQualUserName, TestSettings.WinQualPassword));
            winQual.SynchronizeWithWinQualOnline(index, true, null);
        }
    }
}
