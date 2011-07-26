using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Threading;
using System.IO;

using ServiceUnitTests.StackHashServices;
using StackHashUtilities;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for WinQualSyncUnitTests
    /// </summary>
    [TestClass]
    public class WinQualSyncUnitTests
    {
        Utils m_Utils;
        String m_UserName = "UserName";
        String m_Password = "Password";
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";

        public WinQualSyncUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
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
            m_Utils.SetLicense(s_LicenseId);
            m_Utils.EnableReporting();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.SetProxy(null);
                m_Utils.RemoveAllContexts();
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
        public void RunWinQualSyncOnNonExistentContext()
        {
            try
            {
                // Log in to WinQual with bad credentials - Username and Password.
                m_Utils.StartSynchronization(2, 10000);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Invalid ContextId"));
            }
        }

        public void runWinQualSyncOnInactiveContext(ErrorIndexType indexType)
        {
            try
            {
                m_Utils.CreateNewContext(indexType);

                // Log in to WinQual with bad credentials - Username and Password.
                m_Utils.StartSynchronization(0, 10000);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Context is not active"));
            }
        }

        [TestMethod]
        public void RunWinQualSyncOnInactiveContext()
        {
            runWinQualSyncOnInactiveContext(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunWinQualSyncOnInactiveContextSql()
        {
            runWinQualSyncOnInactiveContext(ErrorIndexType.SqlExpress);
        }
        
        
        public void runWinQualSyncInvalidLogin(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            DateTime startTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();
            StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 20000);

            StackHashAdminReport adminReport = m_Utils.WinQualSyncAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);
            Assert.AreNotEqual(null, adminReport.LastException);
            Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);
            Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, adminReport.ServiceErrorCode);

            DateTime endTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

            bool found = false;
            foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
            {
                if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                {
                    found = true;
                    Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, taskStatus.ServiceErrorCode);
                    Assert.AreEqual(1, taskStatus.FailedCount);
                    Assert.AreEqual(0, taskStatus.SuccessCount);
                    Assert.AreEqual(new DateTime(0), taskStatus.LastSuccessfulRunTimeUtc);
                    Assert.AreEqual(true, (taskStatus.LastFailedRunTimeUtc <= endTime) && (taskStatus.LastFailedRunTimeUtc >= startTime));
                    Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                }
            }

            Assert.AreEqual(true, found);
            Assert.AreEqual(true, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreNotEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
            Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnServiceError);


            m_Utils.DeactivateContext(0);
        }

        [TestMethod]
        public void RunWinQualSyncInvalidLogin()
        {
            runWinQualSyncInvalidLogin(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunWinQualSyncInvalidLoginSql()
        {
            runWinQualSyncInvalidLogin(ErrorIndexType.SqlExpress);
        }


        public void runWinQualSyncInvalidLoginWithRestart(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            DateTime startTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();
            StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 20000);

            StackHashAdminReport adminReport = m_Utils.WinQualSyncAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);
            Assert.AreNotEqual(null, adminReport.LastException);
            Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);
            Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, adminReport.ServiceErrorCode);

            DateTime endTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

            bool found = false;
            foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
            {
                if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                {
                    found = true;
                    Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, taskStatus.ServiceErrorCode);
                    Assert.AreEqual(1, taskStatus.FailedCount);
                    Assert.AreEqual(0, taskStatus.SuccessCount);
                    Assert.AreEqual(new DateTime(0), taskStatus.LastSuccessfulRunTimeUtc);
                    Assert.AreEqual(true, (taskStatus.LastFailedRunTimeUtc <= endTime) && (taskStatus.LastFailedRunTimeUtc >= startTime));
                    Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                }
            }

            Assert.AreEqual(true, found);
            Assert.AreEqual(true, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreNotEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
            Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnServiceError);


            m_Utils.RestartService();

            // Get the current status to see if the last login was successful.
            statusResp = m_Utils.GetServiceStatus();

            found = false;
            foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
            {
                if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                {
                    found = true;
                    Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, taskStatus.ServiceErrorCode);
                    Assert.AreEqual(1, taskStatus.FailedCount);
                    Assert.AreEqual(0, taskStatus.SuccessCount);
                    Assert.AreEqual(new DateTime(0), taskStatus.LastSuccessfulRunTimeUtc);
                    Assert.AreEqual(true, (taskStatus.LastFailedRunTimeUtc <= endTime) && (taskStatus.LastFailedRunTimeUtc >= startTime));
                    Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                }
            }

            Assert.AreEqual(true, found);
            Assert.AreEqual(true, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreNotEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
            Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnServiceError);

            m_Utils.DeactivateContext(0);

        }

        public void RunWinQualSyncInvalidLoginWithRestart()
        {
            runWinQualSyncInvalidLoginWithRestart(ErrorIndexType.Xml);
        }

        public void RunWinQualSyncInvalidLoginWithRestartSql()
        {
            runWinQualSyncInvalidLoginWithRestart(ErrorIndexType.SqlExpress);
        }

        
        
        public void runWinQualSyncAbort(ErrorIndexType indexType)
        {
            // Use the dummy winqual.

            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 10;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = 10;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = 10;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = 10;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = 10;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            m_Utils.EnableLogging();

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();
            
            String testPath = "c:\\stackhashunittests\\testindex\\";

            if (Directory.Exists(testPath))
                StackHashUtilities.PathUtils.DeleteDirectory(testPath, true);

            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);
            m_Utils.ActivateContext(0);


            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 20000);

                // Enable sync for all the products.
                GetProductsResponse getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                    m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);
                }

                // Check it has been set.
                getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                }


                // Restart the synchronization - this time it should download all the files etc...
                resp = m_Utils.StartSynchronizationAsync(0);

                StackHashClientData clientData = m_Utils.LastClientData;

                // Now keep getting the status until the WinQualSyncTask appears.
                bool winQualSyncTaskRunning = false;
                GetStackHashServiceStatusResponse statusResp = null;
                while (!winQualSyncTaskRunning || (m_Utils.AllReports.Count < 10))
                {
                    Thread.Sleep(1000);
                    statusResp = m_Utils.GetServiceStatus();

                    winQualSyncTaskRunning = m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask);
                    Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.AnalyzeTask));
                }

                if (statusResp != null)
                    Assert.AreEqual(true, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTimerTask));

                // Now abort.
                m_Utils.AbortSynchronization(0, 10000);


                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Aborted, adminReport.ResultData);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Products);

                Console.WriteLine("Products {0}, Files {1}, Events {2}, Cabs {3} EventInfos {4}", adminReport.ErrorIndexStatistics.Products, adminReport.ErrorIndexStatistics.Files, adminReport.ErrorIndexStatistics.Events, adminReport.ErrorIndexStatistics.Cabs, adminReport.ErrorIndexStatistics.EventInfos);

                Assert.AreEqual(true, adminReport.ErrorIndexStatistics.Products >= 0);

                // Make sure the task is no longer running.
                statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));
                Assert.AreEqual(true, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTimerTask));
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.DisableLogging();
            }
        }
        
        [TestMethod]
        public void RunWinQualSyncAbort()
        {
            runWinQualSyncAbort(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunWinQualSyncAbortSql()
        {
            runWinQualSyncAbort(ErrorIndexType.SqlExpress);
        }


        public void runWinQualSyncDummyStatsCheck(ErrorIndexType indexType, bool enableNewProductsAutomatically)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.

            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            m_Utils.EnableLogging();

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.EnableNewProductsAutomatically = enableNewProductsAutomatically;

            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                StartSynchronizationResponse resp;
                StackHashClientData clientData;
                StackHashWinQualSyncCompleteAdminReport adminReport;

                if (!enableNewProductsAutomatically)
                {
                    // Synchronize so we have a copy of just the product list.
                    resp = m_Utils.StartSynchronization(0, 60000);

                    adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                    Assert.AreNotEqual(null, adminReport);

                    clientData = m_Utils.LastClientData;
                    Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                    Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                    Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                    Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                    Assert.AreEqual(0, adminReport.ContextId);
                    Assert.AreEqual(null, adminReport.LastException);
                    Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                    Assert.AreEqual(numberOfProducts, adminReport.ErrorIndexStatistics.Products);
                    Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Files);
                    Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Events);
                    Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Cabs);
                    Assert.AreEqual(0, adminReport.ErrorIndexStatistics.EventInfos);

                    // Enable sync for all the products.
                    GetProductsResponse getProducts = m_Utils.GetProducts(0);

                    foreach (StackHashProductInfo productInfo in getProducts.Products)
                    {
                        Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                        m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);

                        // Make sure there are no files for this product yet.
                        GetFilesResponse getFiles = m_Utils.GetFiles(0, productInfo.Product);

                        Assert.AreEqual(0, getFiles.Files.Count);
                    }

                    // Check it has been set.
                    getProducts = m_Utils.GetProducts(0);

                    foreach (StackHashProductInfo productInfo in getProducts.Products)
                    {
                        Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                        Assert.AreEqual(new DateTime(0).ToUniversalTime(), productInfo.LastSynchronizeTime);
                        Assert.AreEqual(new DateTime(0).ToUniversalTime(), productInfo.LastSynchronizeCompletedTime);
                    }
                }



                // The task times are only accurate to the nearest second only.
                DateTime startTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                resp = m_Utils.StartSynchronization(0, 120000);
                clientData = m_Utils.LastClientData;

                DateTime endTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                
                if (!enableNewProductsAutomatically)
                    Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Products);
                else
                    Assert.AreEqual(numberOfProducts, adminReport.ErrorIndexStatistics.Products);

                Assert.AreEqual(numberOfProducts * numberOfFiles, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents * numberOfCabs, adminReport.ErrorIndexStatistics.Cabs);
//                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents * numberOfEventInfos, adminReport.ErrorIndexStatistics.EventInfos);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));

                bool found = false;
                foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
                {
                    if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                    {
                        found = true;
                        Assert.AreEqual(StackHashServiceErrorCode.NoError, taskStatus.ServiceErrorCode);
                        Assert.AreEqual(0, taskStatus.FailedCount);
                        if (!enableNewProductsAutomatically)
                            Assert.AreEqual(2, taskStatus.SuccessCount); // Ran a sync twice - once to get just the product list.
                        else
                            Assert.AreEqual(1, taskStatus.SuccessCount); // Ran a sync twice - once to get just the product list.

                        Assert.AreEqual(true, (taskStatus.LastSuccessfulRunTimeUtc <= endTime) && (taskStatus.LastSuccessfulRunTimeUtc >= startTime));
                        Assert.AreEqual(new DateTime(0), taskStatus.LastFailedRunTimeUtc);
                        Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                    }
                }

                Assert.AreEqual(true, found);
                Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
                Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);

                // Get all the results files and ensure that the Cabs have been unpacked and standard
                // script run.
                GetProductsResponse products = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo thisProduct in products.Products)
                {
                    Assert.AreEqual(numberOfFiles * numberOfEvents, thisProduct.Product.TotalStoredEvents);
                    Assert.AreNotEqual(new DateTime(0).ToUniversalTime(), thisProduct.LastSynchronizeTime);
                    Assert.AreNotEqual(new DateTime(0).ToUniversalTime(), thisProduct.LastSynchronizeCompletedTime);
                    Assert.AreEqual(true, thisProduct.LastSynchronizeTime >= startTime);
                    Assert.AreEqual(true, thisProduct.LastSynchronizeTime <= endTime);
                    Assert.AreEqual(true, thisProduct.LastSynchronizeCompletedTime >= startTime);
                    Assert.AreEqual(true, thisProduct.LastSynchronizeCompletedTime <= endTime);
                    Assert.AreEqual(true, thisProduct.LastSynchronizeCompletedTime >= thisProduct.LastSynchronizeTime);
                }

                GetProductEventPackageResponse events = m_Utils.GetProductEventPackages(0, products.Products[0].Product);

                foreach (StackHashEventPackage package in events.EventPackages)
                {
                    foreach (StackHashCabPackage cab in package.Cabs)
                    {
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis);
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis.ProcessUpTime);
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis.SystemUpTime);
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis.DotNetVersion);
                    }
                }

                // Check the Admin progress events.
                int m_NumDownloadingProducts = 0;
                int m_NumDownloadedProducts = 0;
                int m_NumDownloadingEvents = 0;
                int m_NumDownloadedEvents = 0;
                int m_NumDownloadingCabs = 0;
                int m_NumDownloadedCabs = 0;

                int m_NumEventPages = 0;
                int m_NumCabDownloading = 0;

                foreach (AdminReportEventArgs report in m_Utils.AllReports)
                {
                    StackHashSyncProgressAdminReport typeCheck = null;
                    switch (report.Report.Operation)
                    {
                        case StackHashAdminOperation.WinQualSyncProgressDownloadingProductList:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreEqual(null, typeCheck.Product);
                            Assert.AreEqual(0, typeCheck.TotalPages);
                            Assert.AreEqual(0, typeCheck.CurrentPage);
                            m_NumDownloadingProducts++;
                            break;
                        case StackHashAdminOperation.WinQualSyncProgressProductListUpdated:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreEqual(null, typeCheck.Product);
                            m_NumDownloadedProducts++;
                            break;
                        case StackHashAdminOperation.WinQualSyncProgressDownloadingProductEvents:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeStartedTime >= startTime);
                            Assert.AreEqual(new DateTime(0).ToUniversalTime(), typeCheck.Product.LastSynchronizeCompletedTime);
                            m_NumDownloadingEvents++;
                            break;
                        case StackHashAdminOperation.WinQualSyncProgressProductEventsUpdated:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreNotEqual(null, typeCheck.Product);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeStartedTime <= typeCheck.Product.LastSynchronizeCompletedTime);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeStartedTime >= startTime);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeCompletedTime <= endTime);
                            m_NumDownloadedEvents++;
                            break;  
                        case StackHashAdminOperation.WinQualSyncProgressDownloadingProductCabs:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreNotEqual(null, typeCheck.Product);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeStartedTime >= startTime);
                            Assert.AreEqual(new DateTime(0).ToUniversalTime(), typeCheck.Product.LastSynchronizeCompletedTime);

                            m_NumDownloadingCabs++;
                            break;
                        case StackHashAdminOperation.WinQualSyncProgressProductCabsUpdated:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreNotEqual(null, typeCheck.Product);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeStartedTime <= typeCheck.Product.LastSynchronizeCompletedTime);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeStartedTime >= startTime);
                            Assert.AreEqual(true, typeCheck.Product.LastSynchronizeCompletedTime <= endTime);
                            m_NumDownloadedCabs++;
                            break;
                        case StackHashAdminOperation.WinQualSyncProgressDownloadingEventPage:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreNotEqual(null, typeCheck.Product);
                            Assert.AreNotEqual(null, typeCheck.File);
                            m_NumEventPages++;
                            break;
                        case StackHashAdminOperation.WinQualSyncProgressDownloadingCab:
                            typeCheck = report.Report as StackHashSyncProgressAdminReport;
                            Assert.AreNotEqual(null, typeCheck.Product);
                            Assert.AreNotEqual(null, typeCheck.File);
                            m_NumCabDownloading++;
                            break;
                    }
                }

                // The products will have been synced twice.
                if (!enableNewProductsAutomatically)
                {
                    Assert.AreEqual(2, m_NumDownloadingProducts);
                    Assert.AreEqual(2, m_NumDownloadedProducts);
                }
                else
                {
                    Assert.AreEqual(1, m_NumDownloadingProducts);
                    Assert.AreEqual(1, m_NumDownloadedProducts);

                    // Check the sync data has been set.
                    StackHashProductInfoCollection productData = m_Utils.GetProducts(0).Products;

                    foreach (StackHashProductInfo productInfo in productData)
                    {
                        Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                        Assert.AreNotEqual(new DateTime(0).ToUniversalTime(), productInfo.LastSynchronizeTime);
                        Assert.AreNotEqual(new DateTime(0).ToUniversalTime(), productInfo.LastSynchronizeCompletedTime);
                    }
                }

                Assert.AreEqual(numberOfProducts * numberOfFiles, m_NumDownloadingEvents);
                Assert.AreEqual(numberOfProducts * numberOfFiles, m_NumDownloadedEvents);

                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents, m_NumDownloadingCabs);
                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents, m_NumDownloadedCabs);

                // Not implemented yet.
                Assert.AreEqual(0, m_NumEventPages);
                Assert.AreEqual(0, m_NumCabDownloading);

            }
            finally
            {
                m_Utils.DisableLogging();
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void RunWinQualSyncDummyStatsCheck()
        {
            runWinQualSyncDummyStatsCheck(ErrorIndexType.Xml, false);
        }

        [TestMethod]
        public void RunWinQualSyncDummyStatsCheckSql()
        {
            runWinQualSyncDummyStatsCheck(ErrorIndexType.SqlExpress, false);
        }

        [TestMethod]
        public void RunWinQualSyncDummyStatsCheckSyncNewProductsAutomaticallySql()
        {
            runWinQualSyncDummyStatsCheck(ErrorIndexType.SqlExpress, true);
        }


        public void runWinQualSyncDummyStatsCheckProductsOnly(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.

            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);

                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                Assert.AreEqual(numberOfProducts, adminReport.ErrorIndexStatistics.Products);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Cabs);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.EventInfos);


                // Enable sync for all the products.
                GetProductsResponse getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                    m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);

                    // Make sure there are no files for this product yet.
                    GetFilesResponse getFiles = m_Utils.GetFiles(0, productInfo.Product);

                    Assert.AreEqual(0, getFiles.Files.Count);
                }

                // Check it has been set.
                getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                }


                resp = m_Utils.StartSynchronization(0, 120000, true); // Sync just the products.
                clientData = m_Utils.LastClientData;


                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Products);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Cabs);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.EventInfos);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void RunWinQualSyncDummyStatsCheckProductsOnly()
        {
            runWinQualSyncDummyStatsCheckProductsOnly(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunWinQualSyncDummyStatsCheckProductsOnlySql()
        {
            runWinQualSyncDummyStatsCheckProductsOnly(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// </summary>
        public void runWinQualSuccessThenFailure(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                DateTime startSuccessTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endSuccessTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

                // Enable sync for all the products.
                GetProductsResponse getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                    m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);

                    // Make sure there are no files for this product yet.
                    GetFilesResponse getFiles = m_Utils.GetFiles(0, productInfo.Product);

                    Assert.AreEqual(0, getFiles.Files.Count);
                }

                // Check it has been set.
                getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                }


                // Now prime the dummy winqual services to fail.
                testData.DummyWinQualSettings.FailLogOn = true;
                m_Utils.SetTestData(testData);


                DateTime startTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                DateTime endTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);
 
                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));

                bool found = false;
                foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
                {
                    if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                    {
                        found = true;
                        Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, taskStatus.ServiceErrorCode);
                        Assert.AreEqual(1, taskStatus.FailedCount);
                        Assert.AreEqual(1, taskStatus.SuccessCount); // Ran a sync twice - once to get just the product list.
                        Assert.AreEqual(true, (taskStatus.LastSuccessfulRunTimeUtc <= endSuccessTime) && (taskStatus.LastSuccessfulRunTimeUtc >= startSuccessTime));
                        Assert.AreEqual(true, (taskStatus.LastFailedRunTimeUtc <= endTime) && (taskStatus.LastFailedRunTimeUtc >= startTime));
                        Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                    }
                }

                Assert.AreEqual(true, found);
                Assert.AreEqual(true, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
                Assert.AreNotEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);

                // Get all the results files and ensure that the Cabs have been unpacked and standard
                // script run.
                GetProductsResponse products = m_Utils.GetProducts(0);

                GetProductEventPackageResponse events = m_Utils.GetProductEventPackages(0, products.Products[0].Product);

                foreach (StackHashEventPackage package in events.EventPackages)
                {
                    foreach (StackHashCabPackage cab in package.Cabs)
                    {
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis);
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis.ProcessUpTime);
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis.SystemUpTime);
                        Assert.AreNotEqual(null, cab.Cab.DumpAnalysis.DotNetVersion);
                    }
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// </summary>
        [TestMethod]
        public void RunWinQualSuccessThenFailure()
        {
            runWinQualSuccessThenFailure(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// </summary>
        [TestMethod]
        public void RunWinQualSuccessThenFailureSql()
        {
            runWinQualSuccessThenFailure(ErrorIndexType.SqlExpress);
        }

        /// <summary>
        /// Run sync twice - with SyncsBeforeSync = 2 so second should be forced as a full update.
        /// </summary>
        public void runWinQualWithForcedResync(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            
            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.SyncsBeforeResync = 2; // **************
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync
                
                foreach (AdminReportEventArgs report in m_Utils.AllReports)
                {
                    if (report.Report is StackHashSyncProgressAdminReport)
                    {
                        StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                        Assert.AreEqual(false, progressReport.IsResync);
                    }
                }
                
                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                
                

                m_Utils.AllReports.Clear();

                resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync
                
                foreach (AdminReportEventArgs report in m_Utils.AllReports)
                {
                    if (report.Report is StackHashSyncProgressAdminReport)
                    {
                        StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                        Assert.AreEqual(true, progressReport.IsResync);
                    }
                }
                
                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }


        /// <summary>
        /// Run sync twice - with SyncsBeforeSync = 2 so second should be forced as a full update.
        /// </summary>
        [TestMethod]
        public void RunWinQualWithForcedResync()
        {
            runWinQualWithForcedResync(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync twice - with SyncsBeforeSync = 2 so second should be forced as a full update.
        /// </summary>
        [TestMethod]
        public void RunWinQualWithForcedResyncSql()
        {
            runWinQualWithForcedResync(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run sync 3 times - with SyncsBeforeSync = 2.
        /// Also sync second one with products only so it shouldn't update the count.
        /// </summary>
        public void runWinQualWithForcedResyncProductsOnlyCheck(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.SyncsBeforeResync = 2; // **************
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync

                foreach (AdminReportEventArgs report in m_Utils.AllReports)
                {
                    if (report.Report is StackHashSyncProgressAdminReport)
                    {
                        StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                        Assert.AreEqual(false, progressReport.IsResync);
                        Assert.AreEqual(false, progressReport.IsProductsOnlySync);
                    }
                }

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);



                m_Utils.AllReports.Clear();

                resp = m_Utils.StartSynchronization(0, 60000, true, false); // Don't force resync

                foreach (AdminReportEventArgs report in m_Utils.AllReports)
                {
                    if (report.Report is StackHashSyncProgressAdminReport)
                    {
                        StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                        Assert.AreEqual(false, progressReport.IsResync);
                        Assert.AreEqual(true, progressReport.IsProductsOnlySync);
                    }
                }

                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                m_Utils.AllReports.Clear();

                resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync

                foreach (AdminReportEventArgs report in m_Utils.AllReports)
                {
                    if (report.Report is StackHashSyncProgressAdminReport)
                    {
                        StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                        Assert.AreEqual(true, progressReport.IsResync);
                        Assert.AreEqual(false, progressReport.IsProductsOnlySync);
                    }
                }

                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run sync 3 times - with SyncsBeforeSync = 2.
        /// Also sync second one with products only so it shouldn't update the count.
        /// </summary>
        [TestMethod]
        public void RunWinQualWithForcedResyncProductsOnlyCheck()
        {
            runWinQualWithForcedResyncProductsOnlyCheck(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync 3 times - with SyncsBeforeSync = 2.
        /// Also sync second one with products only so it shouldn't update the count.
        /// </summary>
        [TestMethod]
        public void RunWinQualWithForcedResyncProductsOnlyCheckSql()
        {
            runWinQualWithForcedResyncProductsOnlyCheck(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run sync 4 times - with SyncsBeforeSync = 2. Should resync twice.
        /// </summary>
        public void runWinQualSync4TimesWithForcedResyncEvery2(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.SyncsBeforeResync = 2; // **************
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync

                    foreach (AdminReportEventArgs report in m_Utils.AllReports)
                    {
                        if (report.Report is StackHashSyncProgressAdminReport)
                        {
                            StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                            Assert.AreEqual(((i + 1) % 2) == 0, progressReport.IsResync);
                            Assert.AreEqual(false, progressReport.IsProductsOnlySync);
                        }
                    }

                    StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                    Assert.AreEqual(null, adminReport.LastException);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                    m_Utils.AllReports.Clear();
                }

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run sync 4 times - with SyncsBeforeSync = 2. Should resync twice.
        /// </summary>
        [TestMethod]
        public void RunWinQualSync4TimesWithForcedResyncEvery2()
        {
            runWinQualSync4TimesWithForcedResyncEvery2(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync 4 times - with SyncsBeforeSync = 2. Should resync twice.
        /// </summary>
        [TestMethod]
        public void RunWinQualSync4TimesWithForcedResyncEvery2Sql()
        {
            runWinQualSync4TimesWithForcedResyncEvery2(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run sync 4 times - with SyncsBeforeSync = 2. Should resync twice.
        /// Restarts the service between each sync to ensure persistence works.
        /// </summary>
        public void runWinQualSync4TimesWithForcedResyncEvery2WithReset(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            m_Utils.SetTestData(testData);
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.SyncsBeforeResync = 2; // **************
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    m_Utils.SetTestData(testData);
                    m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

                    StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync

                    foreach (AdminReportEventArgs report in m_Utils.AllReports)
                    {
                        if (report.Report is StackHashSyncProgressAdminReport)
                        {
                            StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                            Assert.AreEqual(((i + 1) % 2) == 0, progressReport.IsResync);
                            Assert.AreEqual(false, progressReport.IsProductsOnlySync);
                        }
                    }

                    StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                    Assert.AreEqual(null, adminReport.LastException);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                    m_Utils.AllReports.Clear();

                    m_Utils.RestartService();
                }

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }


        /// <summary>
        /// Run sync 4 times - with SyncsBeforeSync = 2. Should resync twice.
        /// Restarts the service between each sync to ensure persistence works.
        /// </summary>
        [TestMethod]
        public void RunWinQualSync4TimesWithForcedResyncEvery2WithReset()
        {
            runWinQualSync4TimesWithForcedResyncEvery2WithReset(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync 4 times - with SyncsBeforeSync = 2. Should resync twice.
        /// Restarts the service between each sync to ensure persistence works.
        /// </summary>
        [TestMethod]
        public void RunWinQualSync4TimesWithForcedResyncEvery2WithResetSql()
        {
            runWinQualSync4TimesWithForcedResyncEvery2WithReset(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Run sync 9 times - with SyncsBeforeSync = 3. Should resync 3 times.
        /// </summary>
        public void runWinQualSync9TimesWithForcedResyncEvery3(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.SyncsBeforeResync = 3; // **************
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                for (int i = 0; i < 9; i++)
                {
                    StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000, false, false); // Don't force resync

                    foreach (AdminReportEventArgs report in m_Utils.AllReports)
                    {
                        if (report.Report is StackHashSyncProgressAdminReport)
                        {
                            StackHashSyncProgressAdminReport progressReport = report.Report as StackHashSyncProgressAdminReport;

                            Assert.AreEqual(((i + 1) % 3) == 0, progressReport.IsResync);
                            Assert.AreEqual(false, progressReport.IsProductsOnlySync);
                        }
                    }

                    StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                    Assert.AreEqual(null, adminReport.LastException);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                    m_Utils.AllReports.Clear();
                }

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run sync 9 times - with SyncsBeforeSync = 3. Should resync 3 times.
        /// </summary>
        [TestMethod]
        public void RunWinQualSync9TimesWithForcedResyncEvery3()
        {
            runWinQualSync9TimesWithForcedResyncEvery3(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync 9 times - with SyncsBeforeSync = 3. Should resync 3 times.
        /// </summary>
        [TestMethod]
        public void RunWinQualSync9TimesWithForcedResyncEvery3Sql()
        {
            runWinQualSync9TimesWithForcedResyncEvery3(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// Fail first then success.
        /// </summary>
        public void runWinQualFailureThenSuccess(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailLogOn = true;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                DateTime startFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endFailedTime = DateTime.Now.ToUniversalTime();
                endFailedTime = endFailedTime.RoundToNextSecond();


                // Now prime the dummy winqual services to succeed.
                testData.DummyWinQualSettings.FailLogOn = false ;
                m_Utils.SetTestData(testData);


                DateTime startTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                DateTime endTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));

                bool found = false;
                foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
                {
                    if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                    {
                        found = true;
                        Assert.AreEqual(StackHashServiceErrorCode.NoError, taskStatus.ServiceErrorCode);
                        Assert.AreEqual(1, taskStatus.FailedCount);
                        Assert.AreEqual(1, taskStatus.SuccessCount); // Ran a sync twice - once to get just the product list.
                        Assert.AreEqual(true, (taskStatus.LastSuccessfulRunTimeUtc <= endTime) && (taskStatus.LastSuccessfulRunTimeUtc >= startTime));
                        Assert.AreEqual(true, (taskStatus.LastFailedRunTimeUtc <= endFailedTime) && (taskStatus.LastFailedRunTimeUtc >= startFailedTime));
                        Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                    }
                }

                Assert.AreEqual(true, found);
                Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
                Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
                Assert.AreEqual(StackHashServiceErrorCode.NoError, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnServiceError);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// Fail first then success.
        /// </summary>
        [TestMethod]
        public void RunWinQualFailureThenSuccess()
        {
            runWinQualFailureThenSuccess(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// Fail first then success.
        /// </summary>
        [TestMethod]
        public void RunWinQualFailureThenSuccessSql()
        {
            runWinQualFailureThenSuccess(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// The first is on a timer. The second is due to a retry.
        /// Check that the task status is as expected.
        /// Fail first then success.
        /// </summary>
        public void runWinQualFailureThenSuccessAutoRetry(ErrorIndexType indexType, int retryCount)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 2;
            int numberOfEvents = 1;
            int numberOfEventInfos = 3;
            int numberOfCabs = 2;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailSync = true;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            settings.Settings.ContextCollection[0].WinQualSettings.RetryAfterError = true;
            settings.Settings.ContextCollection[0].WinQualSettings.DelayBeforeRetryInSeconds = 5; // 10 seconds.
            settings.Settings.ContextCollection[0].WinQualSyncSchedule[0].Period = SchedulePeriod.Hourly;
            settings.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time = new ScheduleTime();

            DateTime syncTime = DateTime.Now.AddSeconds(20);

            settings.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Hour = syncTime.Hour;
            settings.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Minute = syncTime.Minute;
            settings.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Second = syncTime.Second;
            settings.Settings.ContextCollection[0].EmailSettings = new StackHashEmailSettings();
            settings.Settings.ContextCollection[0].EmailSettings.OperationsToReport = new StackHashAdminOperationCollection();
            settings.Settings.ContextCollection[0].EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncStarted);
            settings.Settings.ContextCollection[0].EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncCompleted);
            settings.Settings.ContextCollection[0].EmailSettings.SmtpSettings = new StackHashSmtpSettings();

            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);
            m_Utils.SetEmailSettings(0, settings.Settings.ContextCollection[0].EmailSettings);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                DateTime startFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();


                // Wait for the autosync to fail the specified number of times.
                for (int i = 0; i < retryCount; i++)
                {
                    Assert.AreEqual(true, m_Utils.WaitForSyncCompletion(60000));
                }
                DateTime endFailedTime = DateTime.Now.ToUniversalTime();
                endFailedTime = endFailedTime.RoundToNextSecond();


                // Now prime the dummy winqual services to succeed.
                testData.DummyWinQualSettings.FailSync = false;
                m_Utils.SetTestData(testData);


                DateTime startTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                // Wait for the sync complete. This should come in for next event.
                Assert.AreEqual(true, m_Utils.WaitForSyncCompletion(60000));

                StackHashClientData clientData = m_Utils.LastClientData;

                DateTime endTime = DateTime.Now.ToUniversalTime().RoundToNextSecond();

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(null, adminReport.ClientData);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));

                bool found = false;
                foreach (StackHashTaskStatus taskStatus in statusResp.Status.ContextStatusCollection[0].TaskStatusCollection)
                {
                    if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                    {
                        found = true;
                        Assert.AreEqual(StackHashServiceErrorCode.NoError, taskStatus.ServiceErrorCode);
                        Assert.AreEqual(retryCount, taskStatus.FailedCount);
                        Assert.AreEqual(1, taskStatus.SuccessCount); // Ran a sync twice - once to get just the product list.
                        Assert.AreEqual(true, (taskStatus.LastSuccessfulRunTimeUtc <= endTime) && (taskStatus.LastSuccessfulRunTimeUtc >= startTime));
                        Assert.AreEqual(true, (taskStatus.LastFailedRunTimeUtc <= endFailedTime) && (taskStatus.LastFailedRunTimeUtc >= startFailedTime));
                        Assert.AreEqual(true, (taskStatus.LastStartedTimeUtc <= endTime) && (taskStatus.LastStartedTimeUtc >= startTime));
                    }
                }

                Assert.AreEqual(true, found);
                Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
                Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
                Assert.AreEqual(StackHashServiceErrorCode.NoError, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnServiceError);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// Fail first then wait for the auto retry
        /// </summary>
        [TestMethod]
        public void RunWinQualFailureThenSuccessAutoRetry()
        {
            runWinQualFailureThenSuccessAutoRetry(ErrorIndexType.Xml, 1);
        }

        /// <summary>
        /// Run sync twice with a dummy WinQualServices.
        /// Check that the task status is as expected.
        /// Fail first then wait for the auto retry
        /// </summary>
        [TestMethod]
        public void RunWinQualFailureThenSuccessSqlAutoRetry()
        {
            runWinQualFailureThenSuccessAutoRetry(ErrorIndexType.SqlExpress, 1);
        }

        /// <summary>
        /// Test retry mechanism.
        /// AutoSync fail, fail then succeed.
        /// </summary>
        [TestMethod]
        public void RunWinQualFailureThenSuccessSqlAutoRetryTwice()
        {
            runWinQualFailureThenSuccessAutoRetry(ErrorIndexType.SqlExpress, 2);
        }

        

        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// </summary>
        public void licenseMaxEventsExceeded(ErrorIndexType indexType)
        {
            String licenseId = "947ef3ec-ce89-487a-a39c-07a57f172ef2";

            // Set to license with just a few events allowed.
            StackHashLicenseData licenseData = m_Utils.SetLicense(licenseId).LicenseData;

            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 100;
            int numberOfEventInfos = 1;
            int numberOfCabs = 1;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;

                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(0, product.Product.Id, true);
                }

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.LicenseEventCountExceeded, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;

                Assert.AreEqual(licenseData.MaxEvents, events.Count);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceeded()
        {
            licenseMaxEventsExceeded(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceededSql()
        {
            licenseMaxEventsExceeded(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// </summary>
        public void licenseMaxEventsExceededOnSecondSync(ErrorIndexType indexType)
        {
            String licenseId = "947ef3ec-ce89-487a-a39c-07a57f172ef2";

            // Set to license with just a few events allowed - MaxEvents is 5.
            StackHashLicenseData licenseData = m_Utils.SetLicense(licenseId).LicenseData;

            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = (int)licenseData.MaxEvents - 1;
            int numberOfEventInfos = 1;
            int numberOfCabs = 1;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;

                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(0, product.Product.Id, true);
                }

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.NoError, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;

                Assert.AreEqual(licenseData.MaxEvents - 1, events.Count);

                // Now sync again.
                testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = (int)licenseData.MaxEvents * 2;
                m_Utils.SetTestData(testData);

                resp = m_Utils.StartSynchronization(0, 120000);
                clientData = m_Utils.LastClientData;

                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.LicenseEventCountExceeded, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;

                Assert.AreEqual(licenseData.MaxEvents, events.Count);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceededOnSecondSync()
        {
            licenseMaxEventsExceededOnSecondSync(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceededOnSecondSyncSql()
        {
            licenseMaxEventsExceededOnSecondSync(ErrorIndexType.SqlExpress);
        }

        /// <summary>
        /// Run a sync on 1 profile (up to max events - 1).
        /// Run a second sync across profile 2. Should only load 1 extra event.
        /// </summary>
        public void licenseMaxEventsExceededAcrossContexts(ErrorIndexType indexType)
        {
            String licenseId = "947ef3ec-ce89-487a-a39c-07a57f172ef2";

            // Set to license with just a few events allowed - MaxEvents is 5.
            StackHashLicenseData licenseData = m_Utils.SetLicense(licenseId).LicenseData;

            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = (int)licenseData.MaxEvents - 1;
            int numberOfEventInfos = 1;
            int numberOfCabs = 1;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Create 2 contexts.
            m_Utils.CreateNewContext(indexType);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;

            String testPath2 = "c:\\stackhashunittest\\testindex2\\";
            settings.Settings.ContextCollection[1].ErrorIndexSettings.Folder = testPath2;
            settings.Settings.ContextCollection[1].ErrorIndexSettings.Name = "TestIndex2";
            settings.Settings.ContextCollection[1].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[1].WinQualSettings.Password = m_Password;

            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[1]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);
            m_Utils.DeleteIndex(1);

            m_Utils.ActivateContext(0);
            m_Utils.ActivateContext(1);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;

                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(0, product.Product.Id, true);
                }

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.NoError, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;

                Assert.AreEqual(licenseData.MaxEvents - 1, events.Count);

                // Now sync again on the second context.
                // Synchronize so we have a copy of just the product list.
                resp = m_Utils.StartSynchronization(1, 60000);

                products = m_Utils.GetProducts(1).Products;

                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(1, product.Product.Id, true);
                }

                
                testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = (int)licenseData.MaxEvents * 2;
                m_Utils.SetTestData(testData);

                resp = m_Utils.StartSynchronization(1, 120000);
                clientData = m_Utils.LastClientData;

                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(1, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.LicenseEventCountExceeded, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;
                Assert.AreEqual(licenseData.MaxEvents - 1, events.Count);

                events = m_Utils.GetProductEventPackages(1, products[0].Product).EventPackages;
                Assert.AreEqual(1, events.Count);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);

                m_Utils.DeactivateContext(1);
                m_Utils.DeleteIndex(1);

                m_Utils.RemoveContext(0);
                m_Utils.RemoveContext(1);
            }
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license across contexts.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceededAcrossContexts()
        {
            licenseMaxEventsExceededAcrossContexts(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license across contexts.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceededAcrossContextsl()
        {
            licenseMaxEventsExceededAcrossContexts(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run a sync - number of events reaches the license - 1
        /// Files shared across products.
        /// </summary>
        public void licenseMaxEventsNotExceededDuplicateEvents(ErrorIndexType indexType)
        {
            String licenseId = "947ef3ec-ce89-487a-a39c-07a57f172ef2";

            // Set to license with just a few events allowed.
            StackHashLicenseData licenseData = m_Utils.SetLicense(licenseId).LicenseData;

            int numberOfProducts = 2;
            int numberOfFiles = 1;
            int numberOfEvents = (int)licenseData.MaxEvents - 1;   // Store the maximum across all products - 1
            int numberOfEventInfos = 1;
            int numberOfCabs = 0;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.ObjectsToCreate.DuplicateFileIdsAcrossProducts = true;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;

                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(0, product.Product.Id, true);
                }

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.NoError, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;

                Assert.AreEqual(licenseData.MaxEvents - 1, events.Count);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Exactly the correct number of events across 2 products each with a shared file.
        /// The file events should only be counted once.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsNotExceededDuplicateEvents()
        {
            licenseMaxEventsNotExceededDuplicateEvents(ErrorIndexType.SqlExpress);
        }

        /// <summary>
        /// Run a sync - number of events exceeds the license.
        /// Events shared across files across products.
        /// </summary>
        public void licenseMaxEventsExceededDuplicateEvents(ErrorIndexType indexType)
        {
            String licenseId = "947ef3ec-ce89-487a-a39c-07a57f172ef2";

            // Set to license with just a few events allowed.
            StackHashLicenseData licenseData = m_Utils.SetLicense(licenseId).LicenseData;

            int numberOfProducts = 2;
            int numberOfFiles = 1;
            int numberOfEvents = (int)(licenseData.MaxEvents + 1);   // Store the maximum across all products + 1.
            int numberOfEventInfos = 1;
            int numberOfCabs = 0;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.ObjectsToCreate.DuplicateFileIdsAcrossProducts = true;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                DateTime endFailedTime = DateTime.Now.ToUniversalTime().RoundToPreviousSecond();

                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;

                foreach (StackHashProductInfo product in products)
                {
                    m_Utils.SetProductSynchronizationState(0, product.Product.Id, true);
                }

                resp = m_Utils.StartSynchronization(0, 120000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.LicenseEventCountExceeded, adminReport.ServiceErrorCode);

                // Get the events to make sure only the permitted number were added.
                StackHashEventPackageCollection events = m_Utils.GetProductEventPackages(0, products[0].Product).EventPackages;

                Assert.AreEqual(licenseData.MaxEvents, events.Count);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Exactly the correct number of events + 2 across 2 products each with a shared file.
        /// The file events should only be counted once.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseMaxEventsExceededDuplicateEvents()
        {
            licenseMaxEventsExceededDuplicateEvents(ErrorIndexType.SqlExpress);
        }
        
        /// <summary>
        /// Run a sync - license expired.
        /// </summary>
        public void licenseExpired(ErrorIndexType indexType)
        {
            String licenseId = "942adc40-8e65-4913-a8af-28f0d50b86d1";

            // Set to license with just a few events allowed.
            StackHashLicenseData licenseData = m_Utils.SetLicense(licenseId).LicenseData;

            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 100;
            int numberOfEventInfos = 1;
            int numberOfCabs = 1;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.LicenseExpired, adminReport.ServiceErrorCode);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run a sync - license expired.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseExpired()
        {
            licenseExpired(ErrorIndexType.Xml);        
        }

        /// <summary>
        /// Run a sync - license expired.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void LicenseExpiredSql()
        {
            licenseExpired(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Run a sync -no license
        /// </summary>
        public void noLicense(ErrorIndexType indexType)
        {
            m_Utils.SetLicense("Delete");

            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 100;
            int numberOfEventInfos = 1;
            int numberOfCabs = 1;

            // Use the dummy winqual.
            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;
            testData.DummyWinQualSettings.FailLogOn = false;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);
                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

                Assert.AreEqual(StackHashServiceErrorCode.NoError, adminReport.ServiceErrorCode);

            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Run a sync -no license
        /// </summary>
        [TestMethod]
        [Ignore]
        public void NoLicense()
        {
            noLicense(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Run a sync -no license
        /// </summary>
        [TestMethod]
        [Ignore]
        public void NoLicenseSql()
        {
            noLicense(ErrorIndexType.SqlExpress);
        }


        public void runWinQualSyncWhenAlreadyRunningDummy(ErrorIndexType indexType)
        {
            int numberOfProducts = 1;
            int numberOfFiles = 100;
            int numberOfEvents = 5;
            int numberOfEventInfos = 3;
            int numberOfCabs = 10;

            // Use the dummy winqual.

            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);
            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 20000);

                // Enable sync for all the products.
                GetProductsResponse getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                    m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);
                }

                // Check it has been set.
                getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                }

                resp = m_Utils.StartSynchronizationAsync(0);
                StackHashClientData clientData = m_Utils.LastClientData;

                bool exceptionGenerated = false;
                try
                {
                    resp = m_Utils.StartSynchronizationAsync(0);
                }
                catch (FaultException<ReceiverFaultDetail> ex)
                {
                    exceptionGenerated = true;
                    Assert.AreEqual(true, ex.Message.Contains("Win Qual synchronize already in progress"));
                }

                Assert.AreEqual(true, exceptionGenerated);

                m_Utils.WaitForSyncCompletion(120000);
                m_Utils.WaitForAnalyzeCompletion(10000);

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Products);
                Assert.AreEqual(numberOfProducts * numberOfFiles, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents * numberOfCabs, adminReport.ErrorIndexStatistics.Cabs);
//                Assert.AreEqual(numberOfProducts * numberOfFiles * numberOfEvents * numberOfEventInfos, adminReport.ErrorIndexStatistics.EventInfos);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void RunWinQualSyncWhenAlreadyRunningDummy()
        {
            runWinQualSyncWhenAlreadyRunningDummy(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunWinQualSyncWhenAlreadyRunningDummySql()
        {
            runWinQualSyncWhenAlreadyRunningDummy(ErrorIndexType.SqlExpress);
        }


        public void deactivateWhileWinQualSyncRunning(ErrorIndexType indexType)
        {
            int numberOfProducts = 10;
            int numberOfFiles = 10;
            int numberOfEvents = 10;
            int numberOfEventInfos = 10;
            int numberOfCabs = 10;

            // Use the dummy winqual.

            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 20000);

                // Enable sync for all the products.
                GetProductsResponse getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                    m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);
                }

                // Check it has been set.
                getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                }

                resp = m_Utils.StartSynchronizationAsync(0);
                StackHashClientData clientData = m_Utils.LastClientData;

                // Give it a chance to start a sync.
                m_Utils.WaitForSyncProgress(10000);                
                
                // Now deactivate.
                m_Utils.DeactivateContext(0);

                m_Utils.WaitForSyncCompletion(120000);

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreNotEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Aborted, adminReport.ResultData);
                Assert.AreEqual(true, numberOfProducts >= adminReport.ErrorIndexStatistics.Products);
                Assert.AreEqual(true, numberOfProducts * numberOfFiles >= adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(true, numberOfProducts * numberOfFiles * numberOfEvents >= adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(true, numberOfProducts * numberOfFiles * numberOfEvents * numberOfCabs >= adminReport.ErrorIndexStatistics.Cabs);
                Assert.AreEqual(true, numberOfProducts * numberOfFiles * numberOfEvents >= adminReport.ErrorIndexStatistics.EventInfos);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void DeactivateWhileWinQualSyncRunning()
        {
            deactivateWhileWinQualSyncRunning(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void DeactivateWhileWinQualSyncRunningSql()
        {
            deactivateWhileWinQualSyncRunning(ErrorIndexType.SqlExpress);
        }

        public void abortWinQualSyncWhenNotRunning(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            // Should just ignore.
            m_Utils.AbortSynchronization(0, 0);

            m_Utils.DeactivateContext(0);
        }

        [TestMethod]
        public void AbortWinQualSyncWhenNotRunning()
        {
            abortWinQualSyncWhenNotRunning(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void AbortWinQualSyncWhenNotRunningSql()
        {
            abortWinQualSyncWhenNotRunning(ErrorIndexType.SqlExpress);
        }

        
        
        public void runWinQualSyncTestLoginTaskInvalidUsername(ErrorIndexType indexType)
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            RunWinQualLogOnResponse resp = m_Utils.RunWinQualLogOn(0, "Username", "Password");

            StackHashAdminReport adminReport = m_Utils.WinQualLogOnAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);

            Assert.AreNotEqual(null, adminReport.LastException);
            // expected text is... An exception occured while trying to authenticate the username and password
            Assert.AreEqual(true, adminReport.LastException.Contains("Please check that your Windows live id username and password are correct"));
            Assert.AreEqual(StackHashAdminOperation.WinQualLogOnCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

            Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
        }

        [TestMethod]
        public void RunWinQualSyncTestLoginTaskInvalidUsername()
        {
            runWinQualSyncTestLoginTaskInvalidUsername(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RunWinQualSyncTestLoginTaskInvalidUsernameSql()
        {
            runWinQualSyncTestLoginTaskInvalidUsername(ErrorIndexType.SqlExpress);
        }


        public void runWinQualSyncTestLoginTaskValidCredentials(ErrorIndexType indexType)
        {
            String userName = ServiceTestSettings.WinQualUserName;
            String password = ServiceTestSettings.WinQualPassword;
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            RunWinQualLogOnResponse resp = m_Utils.RunWinQualLogOn(0, userName, password);

            StackHashAdminReport adminReport = m_Utils.WinQualLogOnAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);

            Assert.AreEqual(null, adminReport.LastException);
            Assert.AreEqual(StackHashAdminOperation.WinQualLogOnCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

            Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
        }

        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentials()
        {
            runWinQualSyncTestLoginTaskValidCredentials(ErrorIndexType.Xml);
        }

        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentialsSql()
        {
            runWinQualSyncTestLoginTaskValidCredentials(ErrorIndexType.SqlExpress);
        }



        public void runWinQualSyncTestLoginTaskValidCredentialsProxyDisabled(ErrorIndexType indexType)
        {
            String userName = ServiceTestSettings.WinQualUserName;
            String password = ServiceTestSettings.WinQualPassword;
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = false;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = null;
            proxySettings.ProxyHost = null;
            proxySettings.ProxyPort = 0;
            proxySettings.ProxyUserName = null;
            proxySettings.ProxyPassword = null;

            m_Utils.SetProxy(proxySettings);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            RunWinQualLogOnResponse resp = m_Utils.RunWinQualLogOn(0, userName, password);

            StackHashAdminReport adminReport = m_Utils.WinQualLogOnAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);

            Assert.AreEqual(null, adminReport.LastException);
            Assert.AreEqual(StackHashAdminOperation.WinQualLogOnCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

            Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
        }

        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentialsProxyDisabled()
        {
            runWinQualSyncTestLoginTaskValidCredentialsProxyDisabled(ErrorIndexType.Xml);
        }

        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentialsProxyDisabledSql()
        {
            runWinQualSyncTestLoginTaskValidCredentialsProxyDisabled(ErrorIndexType.SqlExpress);
        }


        public void runWinQualSyncTestLoginTaskValidCredentialsProxyUnreachable(ErrorIndexType indexType)
        {
            String userName = ServiceTestSettings.WinQualUserName;
            String password = ServiceTestSettings.WinQualPassword;
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = true;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = null;
            proxySettings.ProxyHost = "poo";
            proxySettings.ProxyPort = 9000;
            proxySettings.ProxyUserName = null;
            proxySettings.ProxyPassword = null;

            m_Utils.SetProxy(proxySettings);

            m_Utils.CreateAndSetNewContext(indexType);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            RunWinQualLogOnResponse resp = m_Utils.RunWinQualLogOn(0, userName, password);

            StackHashAdminReport adminReport = m_Utils.WinQualLogOnAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);

            Assert.AreNotEqual(null, adminReport.LastException);
            Assert.AreEqual(true, adminReport.LastException.Contains("The remote name could not be resolved"));
            Assert.AreEqual(StackHashAdminOperation.WinQualLogOnCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();
        }


        // This test should be enabled when the service proxy code for live id is working again.
        [TestMethod]
        [Ignore] 
        public void RunWinQualSyncTestLoginTaskValidCredentialsProxyUnreachable()
        {
            runWinQualSyncTestLoginTaskValidCredentialsProxyUnreachable(ErrorIndexType.Xml);
        }

        // This test should be enabled when the service proxy code for live id is working again.
        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentialsProxyUnreachableSql()
        {
            runWinQualSyncTestLoginTaskValidCredentialsProxyUnreachable(ErrorIndexType.SqlExpress);
        }


        public void runWinQualSyncTestLoginTaskValidCredentialsProxyUnreachableThenDisableProxy(ErrorIndexType indexType)
        {
            String userName = ServiceTestSettings.WinQualUserName;
            String password = ServiceTestSettings.WinQualPassword;
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = true;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = null;
            proxySettings.ProxyHost = "poo";
            proxySettings.ProxyPort = 9000;
            proxySettings.ProxyUserName = null;
            proxySettings.ProxyPassword = null;

            m_Utils.SetProxy(proxySettings);

            m_Utils.CreateAndSetNewContext(indexType);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            RunWinQualLogOnResponse resp = m_Utils.RunWinQualLogOn(0, userName, password);

            StackHashAdminReport adminReport = m_Utils.WinQualLogOnAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);

            Assert.AreNotEqual(null, adminReport.LastException);
            Assert.AreEqual(true, adminReport.LastException.Contains("The remote name could not be resolved"));
            Assert.AreEqual(StackHashAdminOperation.WinQualLogOnCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Failed, adminReport.ResultData);

            // Get the current status to see if the last login was successful.
            GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

            Assert.AreEqual(true, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreNotEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
            Assert.AreEqual(true, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException.Contains("The remote name could not be resolved"));


            proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = false;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = null;
            proxySettings.ProxyHost = "poo";
            proxySettings.ProxyPort = 9000;
            proxySettings.ProxyUserName = null;
            proxySettings.ProxyPassword = null;

            m_Utils.SetProxy(proxySettings);

            // Log in to WinQual with bad credentials - Username and Password - can take a few seconds to return.
            resp = m_Utils.RunWinQualLogOn(0, userName, password);

            adminReport = m_Utils.WinQualLogOnAdminReport;
            Assert.AreNotEqual(null, adminReport);
            Assert.AreEqual(m_Utils.LastClientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(m_Utils.LastClientData.ClientId, adminReport.ClientData.ClientId);
            Assert.AreEqual(m_Utils.LastClientData.ClientName, adminReport.ClientData.ClientName);
            Assert.AreEqual(m_Utils.LastClientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
            Assert.AreEqual(0, adminReport.ContextId);

            Assert.AreEqual(null, adminReport.LastException);
            Assert.AreEqual(StackHashAdminOperation.WinQualLogOnCompleted, adminReport.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);

            // Get the current status to see if the last login was successful.
            statusResp = m_Utils.GetServiceStatus();

            Assert.AreEqual(false, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnFailed);
            Assert.AreEqual(null, statusResp.Status.ContextStatusCollection[0].LastSynchronizationLogOnException);
        }

        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentialsProxyUnreachableThenDisableProxy()
        {
            runWinQualSyncTestLoginTaskValidCredentialsProxyUnreachableThenDisableProxy(ErrorIndexType.Xml);
        }

        [TestMethod]
        [Ignore]
        public void RunWinQualSyncTestLoginTaskValidCredentialsProxyUnreachableThenDisableProxySql()
        {
            runWinQualSyncTestLoginTaskValidCredentialsProxyUnreachableThenDisableProxy(ErrorIndexType.SqlExpress);
        }

        public void runWinQualSyncDummyStatsCheckSpecificProducts(ErrorIndexType indexType)
        {
            int numberOfProducts = 2;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfEventInfos = 1;
            int numberOfCabs = 0;

            // Use the dummy winqual.

            StackHashTestData testData = new StackHashTestData();
            testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
            testData.DummyWinQualSettings.UseDummyWinQual = true;
            testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
            testData.DummyWinQualSettings.ObjectsToCreate.UseLargeCab = false;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = numberOfProducts;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfFiles = numberOfFiles;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEvents = numberOfEvents;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfEventInfos = numberOfEventInfos;
            testData.DummyWinQualSettings.ObjectsToCreate.NumberOfCabs = numberOfCabs;

            m_Utils.SetTestData(testData);

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
            m_Utils.CreateNewContext(indexType);

            // Set the username and password to something valid.
            GetStackHashPropertiesResponse settings = m_Utils.GetContextSettings();

            String testPath = "c:\\stackhashunittest\\testindex\\";
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Folder = testPath;
            settings.Settings.ContextCollection[0].ErrorIndexSettings.Name = "TestIndex";
            settings.Settings.ContextCollection[0].WinQualSettings.UserName = m_UserName;
            settings.Settings.ContextCollection[0].WinQualSettings.Password = m_Password;
            m_Utils.SetContextSettings(settings.Settings.ContextCollection[0]);

            // Make sure the index starts off empty.
            m_Utils.DeleteIndex(0);

            m_Utils.ActivateContext(0);

            try
            {
                // Synchronize so we have a copy of just the product list.
                StartSynchronizationResponse resp = m_Utils.StartSynchronization(0, 60000);

                StackHashClientData clientData = m_Utils.LastClientData;

                StackHashWinQualSyncCompleteAdminReport adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                Assert.AreEqual(numberOfProducts, adminReport.ErrorIndexStatistics.Products);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Cabs);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.EventInfos);


                // Enable sync for all the products.
                GetProductsResponse getProducts = m_Utils.GetProducts(0);


                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(false, productInfo.SynchronizeEnabled);
                    m_Utils.SetProductSynchronizationState(0, productInfo.Product.Id, true);

                    // Make sure there are no files for this product yet.
                    GetFilesResponse getFiles = m_Utils.GetFiles(0, productInfo.Product);

                    Assert.AreEqual(0, getFiles.Files.Count);

                }


                // Check it has been set.
                getProducts = m_Utils.GetProducts(0);

                foreach (StackHashProductInfo productInfo in getProducts.Products)
                {
                    Assert.AreEqual(true, productInfo.SynchronizeEnabled);
                }

                StackHashProductSyncDataCollection productSyncDataCollection = new StackHashProductSyncDataCollection();
                productSyncDataCollection.Add(new StackHashProductSyncData());
                productSyncDataCollection[0].ProductId = getProducts.Products[getProducts.Products.Count - 1].Product.Id;;

                resp = m_Utils.StartSynchronization(0, 120000, false, false, productSyncDataCollection); // Sync the last product only.

                clientData = m_Utils.LastClientData;


                adminReport = m_Utils.WinQualSyncAdminReport as StackHashWinQualSyncCompleteAdminReport;
                Assert.AreNotEqual(null, adminReport);
                Assert.AreEqual(clientData.ApplicationGuid, adminReport.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, adminReport.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, adminReport.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, adminReport.ClientData.ClientRequestId);
                Assert.AreEqual(0, adminReport.ContextId);
                Assert.AreEqual(null, adminReport.LastException);
                Assert.AreEqual(StackHashAdminOperation.WinQualSyncCompleted, adminReport.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, adminReport.ResultData);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Products); // Should have already added the product.
                Assert.AreEqual(1, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(1, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(0, adminReport.ErrorIndexStatistics.Cabs);
                Assert.AreEqual(1, adminReport.ErrorIndexStatistics.EventInfos);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void RunWinQualSyncDummyStatsCheckSpecificProductsSql()
        {
            runWinQualSyncDummyStatsCheckSpecificProducts(ErrorIndexType.SqlExpress);
        }


    
    }
}
