using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Threading;
using System.IO;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Tests download a single cab given a product, file, event and cab.
    /// </summary>
    [TestClass]
    public class DownloadCabUnitTests
    {
        Utils m_Utils;

        public DownloadCabUnitTests()
        {
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

            m_Utils.SetProxy(null);
            m_Utils.RemoveAllContexts();
            m_Utils.RemoveAllScripts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.Dispose();
                m_Utils = null;
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
        public void DownloadCabNonExistentContext()
        {
            try
            {
                m_Utils.DownloadCab(2, null, null, null, null, 30000);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Invalid ContextId"));
            }
        }


        public void downloadCabOnInactiveContext(ErrorIndexType indexType)
        {
            try
            {
                m_Utils.CreateNewContext(indexType);

                m_Utils.DownloadCab(0, null, null, null, null, 30000);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Context is not active"));
            }
        }

        [TestMethod]
        public void DownloadCabOnInactiveContext()
        {
            downloadCabOnInactiveContext(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void DownloadCabOnInactiveContextSql()
        {
            downloadCabOnInactiveContext(ErrorIndexType.SqlExpress);
        }
        
        /// <summary>
        /// Product = null.
        /// </summary>
        public void downloadCabProductNull(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            try
            {
                DownloadCabResponse resp = m_Utils.DownloadCab(0, null, null, null, null, 30000);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Value cannot be null"));
                Assert.AreEqual(true, ex.Message.Contains("product"));
                Assert.AreEqual(StackHashServiceErrorCode.UnexpectedError, ex.Detail.ServiceErrorCode);
            }

            m_Utils.DeactivateContext(0);
        }

        /// <summary>
        /// Product = null.
        /// </summary>
        public void DownloadCabProductNull()
        {
            downloadCabProductNull(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Product = null.
        /// </summary>
        public void DownloadCabProductNullSql()
        {
            downloadCabProductNull(ErrorIndexType.SqlExpress);
        }
        
        /// <summary>
        /// Try a download using default context username and password - should fail with a feed error.
        /// </summary>
        public void downloadCabInvalidLogin(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 1;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                DownloadCabResponse resp = m_Utils.DownloadCab(0, products[0].Product, files[0], events[0].EventData, events[0].Cabs[0].Cab, 30000);

                StackHashAdminReport adminReport = m_Utils.DownloadCabAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(true, adminReport.LastException.Contains("username and password"));
                Assert.AreEqual(StackHashAdminOperation.DownloadCabCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Try a download using default context username and password - should fail with a feed error.
        /// </summary>
        [TestMethod]
        public void DownloadCabInvalidLogin()
        {
            downloadCabInvalidLogin(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Try a download using default context username and password - should fail with a feed error.
        /// </summary>
        [TestMethod]
        public void DownloadCabInvalidLoginSql()
        {
            downloadCabInvalidLogin(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Product doesn't exist.
        /// </summary>
        public void downloadCabProductDoesntExist(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 1;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                try
                {
                    StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                    StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                    StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                    StackHashCabPackageCollection cabs = events[0].Cabs;

                    products[0].Product.Id++; // Wrong ID.
                    DownloadCabResponse resp = m_Utils.DownloadCab(0, products[0].Product, files[0], events[0].EventData, events[0].Cabs[0].Cab, 30000);
                }
                catch (FaultException<ReceiverFaultDetail> ex)
                {
                    Assert.AreEqual(true, ex.Message.Contains("Product does not exist"));
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Product doesn't exist.
        /// </summary>
        [TestMethod]
        public void DownloadCabProductDoesntExist()
        {
            downloadCabProductDoesntExist(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Product doesn't exist.
        /// </summary>
        [TestMethod]
        public void DownloadCabProductDoesntExistSql()
        {
            downloadCabProductDoesntExist(ErrorIndexType.SqlExpress);
        }
        
        /// <summary>
        /// Make sure notification only comes back to the one client.
        /// Username and password are incorrect so should just fail to login.
        /// </summary>
        public void adminReportShouldBeSentToIndividualClient(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, Guid.NewGuid());
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.RegisterForNotifications(true, Guid.NewGuid());
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            StackHashTestIndexData indexData = new StackHashTestIndexData();
            indexData.NumberOfProducts = 1;
            indexData.NumberOfFiles = 1;
            indexData.NumberOfEvents = 1;
            indexData.NumberOfCabs = 1;
            indexData.NumberOfEventInfos = 1;

            m_Utils.CreateTestIndex(0, indexData);

            try
            {
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashFileCollection files = m_Utils.GetFiles(0, products[0].Product).Files;
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                StackHashCabPackageCollection cabs = events[0].Cabs;

                DownloadCabResponse resp = m_Utils.DownloadCab(0, products[0].Product, files[0], events[0].EventData, events[0].Cabs[0].Cab, 30000);

                StackHashAdminReport adminReport = m_Utils.DownloadCabAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(true, adminReport.LastException.Contains("username and password"));
                Assert.AreEqual(StackHashAdminOperation.DownloadCabCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

                // Should receive 3 admin register + download started + download completed.
                Assert.AreEqual(5, m_Utils.AllReports.Count);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Make sure notification only comes back to the one client.
        /// </summary>
        [TestMethod]
        public void AdminReportShouldBeSentToIndividualClient()
        {
            adminReportShouldBeSentToIndividualClient(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Make sure notification only comes back to the one client.
        /// </summary>
        [TestMethod]
        public void AdminReportShouldBeSentToIndividualClientSql()
        {
            adminReportShouldBeSentToIndividualClient(ErrorIndexType.SqlExpress);
        }

    }
}
