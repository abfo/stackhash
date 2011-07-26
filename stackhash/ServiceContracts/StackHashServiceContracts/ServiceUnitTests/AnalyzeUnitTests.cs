using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;
using StackHashUtilities;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for AnalyzeUnitTests
    /// </summary>
    [TestClass]
    public class AnalyzeUnitTests
    {
        Utils m_Utils;
        String m_UserName = "UserName";
        String m_Password = "Password";
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";

        public AnalyzeUnitTests()
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
        public void TestMethod1()
        {
            //
            // TODO: Add test logic here
            //
        }
        public void runAnalyzeJustAutoScripts(ErrorIndexType indexType, int numberOfProducts, int numberOfFiles, int numberOfEvents, int numberOfCabs,
            int numberOfAutoUnmanagedAndManagedScripts, int numberOfManualUnmanagedAndManagedScripts,
            bool useUnmanagedCabs, int numberOfAutoManagedScripts, int numberOfManualManagedScripts,
            int numberOfAutoUnmanagedScripts, int numberOfManualUnmanagedScripts)
        {
            int numberOfEventInfos = 1;

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
            testData.DummyWinQualSettings.ObjectsToCreate.UseUnmanagedCab = useUnmanagedCabs;

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

            for (int i = 0; i < numberOfAutoUnmanagedAndManagedScripts; i++)
            {
                StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(i, true, StackHashScriptDumpType.UnmanagedAndManaged, true);
                m_Utils.AddDebuggerScript(scriptSettings, false);
            }

            for (int i = 0; i < numberOfManualUnmanagedAndManagedScripts; i++)
            {
                StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(i + 100, true, StackHashScriptDumpType.UnmanagedAndManaged, false);
                m_Utils.AddDebuggerScript(scriptSettings, false);
            }

            for (int i = 0; i < numberOfAutoManagedScripts; i++)
            {
                StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(i + 200, true, StackHashScriptDumpType.ManagedOnly, true);
                m_Utils.AddDebuggerScript(scriptSettings, false);
            }

            for (int i = 0; i < numberOfManualManagedScripts; i++)
            {
                StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(i + 300, true, StackHashScriptDumpType.ManagedOnly, false);
                m_Utils.AddDebuggerScript(scriptSettings, false);
            }

            for (int i = 0; i < numberOfAutoUnmanagedScripts; i++)
            {
                StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(i + 400, true, StackHashScriptDumpType.UnmanagedOnly, true);
                m_Utils.AddDebuggerScript(scriptSettings, false);
            }

            for (int i = 0; i < numberOfManualUnmanagedScripts; i++)
            {
                StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(i + 500, true, StackHashScriptDumpType.UnmanagedOnly, false);
                m_Utils.AddDebuggerScript(scriptSettings, false);
            }

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

                // Start the sync and wait for the sync and analyze to complete.
                resp = m_Utils.StartSynchronization(0, 120000, false, false, null);

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
                Assert.AreEqual(numberOfFiles * numberOfProducts, adminReport.ErrorIndexStatistics.Files);
                Assert.AreEqual(numberOfEvents * numberOfFiles * numberOfProducts, adminReport.ErrorIndexStatistics.Events);
                Assert.AreEqual(numberOfCabs * numberOfEvents * numberOfFiles * numberOfProducts, adminReport.ErrorIndexStatistics.Cabs);
                Assert.AreEqual(numberOfEventInfos * numberOfEvents * numberOfFiles * numberOfProducts, adminReport.ErrorIndexStatistics.EventInfos);

                // Make sure the task is no longer running.
                GetStackHashServiceStatusResponse statusResp = m_Utils.GetServiceStatus();

                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask));
                Assert.AreEqual(false, m_Utils.IsTaskRunning(statusResp.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.AnalyzeTask));


                // Check that the scripts have been run ok. Both auto scripts should be run.
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                foreach (StackHashProductInfo product in products)
                {
                    StackHashFileCollection files = m_Utils.GetFiles(0, product.Product).Files;

                    foreach (StackHashFile file in files)
                    {
                        StackHashEventCollection events = m_Utils.GetEvents(0, product.Product, file).Events;

                        foreach (StackHashEvent currentEvent in events)
                        {
                            StackHashEventPackage eventPackage = m_Utils.GetEventPackage(0, product.Product, file, currentEvent).EventPackage;

                            foreach (StackHashCabPackage cab in eventPackage.Cabs)
                            {
                                StackHashScriptResultFiles scriptResults = m_Utils.GetDebugResultFiles(0, product.Product, file, currentEvent, cab.Cab).ResultFiles;

                                int numberOfAutoScripts = 2;
                                int expectedResults = numberOfAutoScripts + numberOfAutoUnmanagedAndManagedScripts;

                                if (!useUnmanagedCabs)
                                    expectedResults += numberOfAutoManagedScripts;
                                else
                                    expectedResults += numberOfAutoUnmanagedScripts;

                                Assert.AreEqual(expectedResults, scriptResults.Count);
                            }
                        }
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
        /// Analyze with just System auto scripts.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeJustAutoScripts1Cab()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }


        /// <summary>
        /// Just system autoscripts - 10 cabs.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeJustAutoScripts10Cabs()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 10;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Just system autoscripts - 2 events - 2 cabs each.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeJustAutoScripts2Events2Cabs()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 2;
            int numberOfCabs = 2;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 1 user managed_unmanaged script.
        /// Should see 1 user results + autoscript results.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeAutoScripts1Events1Cab1UserMUMScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 1;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 2 user managed_unmanaged script.
        /// Should see 2 user results + autoscript results.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeJustAutoScripts1Events1Cab2UserMUMScripts()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 2;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }


        /// <summary>
        /// Auto scripts + 1 user managed_unmanaged script - MANUAL
        /// Should see autoscript results only as MANUAL shouldn't be run by analyse task.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeAutoScripts1Events1Cab1UserMUMManualScripts()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 1;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 2 user managed_unmanaged script - MANUAL
        /// Should see autoscript results only as MANUAL shouldn't be run by analyse task.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeAutoScripts1Events1Cab2UserMUMManualScripts()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 2;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }
        /// <summary>
        /// Auto scripts + 2 user managed_unmanaged script - MANUAL
        /// Should see autoscript results only as MANUAL shouldn't be run by analyse task.
        /// 2 cabs
        /// </summary>
        [TestMethod]
        public void RunAnalyzeAutoScripts1Events2Cab2UserMUMManualScripts()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 2;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 2;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 1 user managed_unmanaged script - MANUAL and one AUTO
        /// Should see autoscript results + auto user script only as MANUAL shouldn't be run by analyse task.
        /// 2 cabs
        /// </summary>
        [TestMethod]
        public void RunAnalyzeJustAutoScripts1Events1Cab1UserMUMManual_1UserMUMAutoScripts()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 1;
            int numberOfManualUnmanagedAndManagedScripts = 1;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 1 user managed only script auto. 
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnManagedCab_1UserManagedScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 1;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }


        /// <summary>
        /// Auto scripts + 1 user managed only script auto. 
        /// Should not run because the dump is unmanaged.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnUnmanagedCab_1UserManagedScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 1;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = true;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }


        /// <summary>
        /// Auto scripts + 1 user unmanaged only script auto. 
        /// Should not run because the dump is unmanaged.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnUnmanagedCab_1UserUnmanagedScriptAuto()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = true;
            int numberOfAutoUnmanagedScripts = 1;
            int numberOfManualUnmanagedScripts = 0;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 1 user unmanaged only script manual (shouldn't be run). 
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnUnmanagedCab_1UserUnmanagedScriptManual()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 0;
            int numberOfManualUnmanagedAndManagedScripts = 0;
            int numberOfAutoManagedScripts = 0;
            int numberOfManualManagedScripts = 0;
            bool useUnmanagedCabs = true;
            int numberOfAutoUnmanagedScripts = 0;
            int numberOfManualUnmanagedScripts = 1;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 1 of each user script.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnUnmanagedCab_1OfEachUserScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 1;
            int numberOfManualUnmanagedAndManagedScripts = 1;
            int numberOfAutoManagedScripts = 1;
            int numberOfManualManagedScripts = 1;
            bool useUnmanagedCabs = true;
            int numberOfAutoUnmanagedScripts = 1;
            int numberOfManualUnmanagedScripts = 1;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 2 of each user script.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnUnmanagedCab_2OfEachUserScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 2;
            int numberOfManualUnmanagedAndManagedScripts = 2;
            int numberOfAutoManagedScripts = 2;
            int numberOfManualManagedScripts = 2;
            bool useUnmanagedCabs = true;
            int numberOfAutoUnmanagedScripts = 2;
            int numberOfManualUnmanagedScripts = 2;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 1 of each user script - managed cab.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnManagedCab_1OfEachUserScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 1;
            int numberOfManualUnmanagedAndManagedScripts = 1;
            int numberOfAutoManagedScripts = 1;
            int numberOfManualManagedScripts = 1;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 1;
            int numberOfManualUnmanagedScripts = 1;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + 2 of each user script - managed cab.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnManagedCab_2OfEachUserScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 2;
            int numberOfManualUnmanagedAndManagedScripts = 2;
            int numberOfAutoManagedScripts = 2;
            int numberOfManualManagedScripts = 2;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 2;
            int numberOfManualUnmanagedScripts = 2;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }

        /// <summary>
        /// Auto scripts + various of each user script - managed cab.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnManagedCab_VariousOfEachUserScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 2;
            int numberOfManualUnmanagedAndManagedScripts = 3;
            int numberOfAutoManagedScripts = 4;
            int numberOfManualManagedScripts = 5;
            bool useUnmanagedCabs = false;
            int numberOfAutoUnmanagedScripts = 6;
            int numberOfManualUnmanagedScripts = 7;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }


        /// <summary>
        /// Auto scripts + various of each user script - unmanaged cab.
        /// </summary>
        [TestMethod]
        public void RunAnalyzeOnUnManagedCab_VariousOfEachUserScript()
        {
            int numberOfProducts = 1;
            int numberOfFiles = 1;
            int numberOfEvents = 1;
            int numberOfCabs = 1;
            int numberOfAutoUnmanagedAndManagedScripts = 2;
            int numberOfManualUnmanagedAndManagedScripts = 3;
            int numberOfAutoManagedScripts = 4;
            int numberOfManualManagedScripts = 5;
            bool useUnmanagedCabs = true;
            int numberOfAutoUnmanagedScripts = 6;
            int numberOfManualUnmanagedScripts = 7;

            runAnalyzeJustAutoScripts(ErrorIndexType.SqlExpress, numberOfProducts, numberOfFiles, numberOfEvents, numberOfCabs,
                numberOfAutoUnmanagedAndManagedScripts, numberOfManualUnmanagedAndManagedScripts,
                useUnmanagedCabs, numberOfAutoManagedScripts, numberOfManualManagedScripts, numberOfAutoUnmanagedScripts, numberOfManualUnmanagedScripts);
        }


    }
}
