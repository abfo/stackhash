using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.ServiceModel;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{

    /// <summary>
    /// Summary description for CabUnitTests
    /// </summary>
    [TestClass]
    public class CabUnitTests
    {
        Utils m_Utils;

        public CabUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

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

        public void streamCabs(ErrorIndexType errorIndexType, bool useBigCabs, int numProducts, int numFiles, int numEvents, int numEventInfos, int numCabs, String fileName, String cabFileName)
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(errorIndexType);

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            resp.Settings.ErrorIndexSettings.Type = errorIndexType;
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = numProducts;
            testIndexData.NumberOfFiles = numFiles;
            testIndexData.NumberOfEvents = numEvents;
            testIndexData.NumberOfEventInfos = numEventInfos;
            testIndexData.NumberOfCabs = numCabs;
            testIndexData.UseLargeCab = useBigCabs;
            testIndexData.CabFileName = cabFileName;

            m_Utils.CreateTestIndex(0, testIndexData);

            // Find the cab.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);
            Assert.AreEqual(numProducts, getProductsResp.Products.Count());

            int productId = 1;
            int fileId = 1;
            int eventId = 1;

            try
            {
                foreach (StackHashProductInfo productInfo in getProductsResp.Products)
                {
                    StackHashProduct product = productInfo.Product;

                    Assert.AreEqual(productId++, product.Id);

                    GetFilesResponse getFilesResp = m_Utils.GetFiles(0, product);
                    Assert.AreEqual(numFiles, getFilesResp.Files.Count());

                    foreach (StackHashFile file in getFilesResp.Files)
                    {
                        Assert.AreEqual(fileId++, file.Id);

                        GetEventsResponse getEventsResp = m_Utils.GetEvents(0, product, file);
                        Assert.AreEqual(numEvents, getEventsResp.Events.Count());

                        foreach (StackHashEvent theEvent in getEventsResp.Events)
                        {
                            Assert.AreEqual(eventId++, theEvent.Id);

                            GetEventPackageResponse getEventPackageResp = m_Utils.GetEventPackage(0, product, file, theEvent);

                            Assert.AreEqual(numCabs, getEventPackageResp.EventPackage.Cabs.Count);
                            Assert.AreEqual(numEventInfos, getEventPackageResp.EventPackage.EventInfoList.Count);

                            // Stream the cabs.
                            foreach (StackHashCabPackage cab in getEventPackageResp.EventPackage.Cabs)
                            {
                                String tempCabFileName = Path.GetTempFileName();
                                File.Delete(tempCabFileName);

                                m_Utils.GetCab(tempCabFileName, 0, product, file, theEvent, cab.Cab, fileName);

                                try
                                {
                                    if (String.IsNullOrEmpty(fileName))
                                    {
                                        Assert.AreEqual(true, File.Exists(tempCabFileName));

                                        FileInfo fileInfo = new FileInfo(tempCabFileName);

                                        if (useBigCabs)
                                        {
                                            Assert.AreEqual(true, fileInfo.Length > 64 * 1024);
                                        }
                                        else if (cabFileName != null)
                                        {
                                            FileInfo sourceCabFileInfo = new FileInfo(cabFileName);
                                            Assert.AreEqual(true, fileInfo.Length == sourceCabFileInfo.Length);
                                            Assert.AreEqual(true, fileInfo.Length > 20000000);
                                        }
                                        else
                                        {
                                            Assert.AreEqual(true, fileInfo.Length <= 64 * 1024);
                                        }
                                    }
                                    else if (String.Compare("version.txt", fileName, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        String allText = File.ReadAllText(tempCabFileName);

                                        Assert.AreEqual(true, allText.Contains("Architecture:"));
                                    }
                                }
                                finally
                                {
                                    File.Delete(tempCabFileName);
                                }

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

        [TestMethod]
        public void StreamASingleCabAllOk()
        {
            streamCabs(ErrorIndexType.SqlExpress, false, 1, 1, 1, 1, 1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ServiceModel.CommunicationException))]
        public void StreamASingleCabOver64KDefaultMaxReceivedMessageSize()
        {
            streamCabs(ErrorIndexType.SqlExpress, true, 1, 1, 1, 1, 1, null, null);
        }

        [TestMethod]
        public void StreamChangeMaxMessageSize()
        {
            m_Utils.Dispose();
            m_Utils = new Utils(100000);
            streamCabs(ErrorIndexType.SqlExpress, true, 1, 1, 1, 1, 1, null, null);
        }

        [TestMethod]
        public void SqlStreamASingleCabAllOk()
        {
            streamCabs(ErrorIndexType.SqlExpress, false, 1, 1, 1, 1, 1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ServiceModel.CommunicationException))]
        public void SqlStreamASingleCabOver64KDefaultMaxReceivedMessageSize()
        {
            streamCabs(ErrorIndexType.SqlExpress, true, 1, 1, 1, 1, 1, null, null);
        }

        [TestMethod]
        public void SqlStreamChangeMaxMessageSize()
        {
            m_Utils.Dispose();
            m_Utils = new Utils(100000);
            streamCabs(ErrorIndexType.SqlExpress, true, 1, 1, 1, 1, 1, null, null);
        }

        [TestMethod]
        public void SqlStreamContentsVersionTxt()
        {
            m_Utils.Dispose();
            m_Utils = new Utils(100000);
            streamCabs(ErrorIndexType.SqlExpress, true, 1, 1, 1, 1, 1, "version.txt", null);
        }

        /// <summary>
        /// Shouldnt be allowed to access any files in parent folders. This would be a security risk.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SqlStreamContentsPrivateFile()
        {
            m_Utils.Dispose();
            m_Utils = new Utils(100000);
            streamCabs(ErrorIndexType.SqlExpress, true, 1, 1, 1, 1, 1, "..\\version.txt", null);
        }

        [TestMethod]
        [Ignore]
        public void SqlStreamASingleLargeCabAllOk()
        {
            m_Utils.Dispose();
            m_Utils = new Utils(300000000);
            String cabFileName = @"R:\stackhash\BusinessLogic\BusinessLogic\TestData\Cabs\crashy64managed4.cab";
            streamCabs(ErrorIndexType.SqlExpress, false, 1, 1, 1, 1, 1, null, cabFileName);
        }
    }
}
