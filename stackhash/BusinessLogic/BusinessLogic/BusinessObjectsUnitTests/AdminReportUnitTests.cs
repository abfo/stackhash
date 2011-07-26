using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for AdminReportUnitTests
    /// </summary>
    [TestClass]
    public class AdminReportUnitTests
    {
        public AdminReportUnitTests()
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
        public void BaseAdminReport()
        {
            StackHashAdminReport adminReport = new StackHashAdminReport();

            adminReport.Operation = StackHashAdminOperation.WinQualSyncStarted;
            adminReport.ServiceErrorCode = StackHashServiceErrorCode.WinQualLogOnFailed;
            adminReport.ClientData = new StackHashClientData(Guid.NewGuid(), "ClientName", 12);
            adminReport.LastException = "Exception text";

            String text = adminReport.ToString();

            Assert.AreEqual("Operation: Synchronize with WinQual on-line has started\r\nResult: WinQualLogOnFailed\r\nInitiator: ClientName\r\nError detail: Exception text\r\n",
                text);
        }

        [TestMethod]
        public void PurgeCompletedAdminReport()
        {
            StackHashPurgeCompleteAdminReport adminReport = new StackHashPurgeCompleteAdminReport();

            adminReport.Operation = StackHashAdminOperation.PurgeCompleted;
            adminReport.ServiceErrorCode = StackHashServiceErrorCode.AccessDenied;
            adminReport.ClientData = new StackHashClientData(Guid.NewGuid(), "ClientName", 12);
            adminReport.LastException = null; // Try null here.

            adminReport.PurgeStatistics = new StackHashPurgeStatistics(1, 2, 3, 4);

            String text = adminReport.ToString();

            Assert.AreEqual("Operation: Purge of old cabs has completed\r\nResult: AccessDenied\r\nInitiator: ClientName\r\nNumber of cabs purged: 1\r\nSize of cabs purged: 2\r\nNumber of dump files purged: 3\r\nSize of dump files purged: 4\r\n",
                text);
        }

        [TestMethod]
        public void WinQualSyncCompletedAdminReport()
        {
            StackHashWinQualSyncCompleteAdminReport adminReport = new StackHashWinQualSyncCompleteAdminReport();

            adminReport.Operation = StackHashAdminOperation.WinQualSyncStarted;
            adminReport.ServiceErrorCode = StackHashServiceErrorCode.WinQualLogOnFailed;
            adminReport.ClientData = new StackHashClientData(Guid.NewGuid(), "ClientName", 12);
            adminReport.LastException = "Exception text";

            adminReport.ErrorIndexStatistics = new StackHashSynchronizeStatistics();
            adminReport.ErrorIndexStatistics.Products = 1;
            adminReport.ErrorIndexStatistics.Files = 2;
            adminReport.ErrorIndexStatistics.Events = 3;
            adminReport.ErrorIndexStatistics.EventInfos = 4;
            adminReport.ErrorIndexStatistics.Cabs = 5;


            String text = adminReport.ToString();

            Assert.AreEqual("Operation: Synchronize with WinQual on-line has started\r\nResult: WinQualLogOnFailed\r\nInitiator: ClientName\r\nError detail: Exception text\r\nMapped products added: 1\r\nMapped files added: 2\r\nEvents added: 3\r\nHits added: 4\r\nCabs added: 5\r\n",
                text);
        }
    }
}
