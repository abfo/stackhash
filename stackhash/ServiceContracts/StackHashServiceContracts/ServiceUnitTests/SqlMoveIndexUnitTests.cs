using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.ServiceModel;
using StackHashUtilities;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for SqlMoveIndexUnitTests
    /// </summary>
    [TestClass]
    public class SqlMoveIndexUnitTests
    {
        Utils m_Utils;

        public SqlMoveIndexUnitTests()
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

            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();

            foreach (StackHashContextSettings settings in getResp.Settings.ContextCollection)
            {
                m_Utils.DeactivateContext(settings.Id);
                if (settings.ErrorIndexSettings.Status != ErrorIndexStatus.Unknown)
                    m_Utils.DeleteIndex(settings.Id);
                m_Utils.RemoveContext(settings.Id);
            }
            m_Utils.RestartService();

            tidyTest();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();

                foreach (StackHashContextSettings settings in getResp.Settings.ContextCollection)
                {
                    m_Utils.DeactivateContext(settings.Id);
                    if (settings.ErrorIndexSettings.Status != ErrorIndexStatus.Unknown)
                        m_Utils.DeleteIndex(settings.Id);
                    m_Utils.RemoveContext(settings.Id);
                }

                m_Utils.Dispose();
                m_Utils = null;
            }

            tidyTest();
        }

        public void tidyTest()
        {
            if (Directory.Exists("C:\\StackHashDefaultErrorIndex"))
                PathUtils.DeleteDirectory("C:\\StackHashDefaultErrorIndex", true);
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
        public void SetIndexSettingsBeforeCreationSql()
        {
            CreateNewStackHashContextResponse newContextResp = m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            Assert.AreEqual(ErrorIndexStatus.NotCreated, newContextResp.Settings.ErrorIndexSettings.Status);

            String originalIndexPath = newContextResp.Settings.ErrorIndexSettings.Folder;
            Assert.AreEqual(false, Directory.Exists(originalIndexPath));

            String indexName = "ShouldNeverExist";
            String testPath = "c:\\stackhashunittests\\testindex\\";

            m_Utils.DeleteIndex(0);

            // Make sure the destination folder does not exist.
            String fullDestPath = Path.Combine(testPath, indexName);

            if (Directory.Exists(fullDestPath))
                PathUtils.DeleteDirectory(fullDestPath, true);

            newContextResp.Settings.ErrorIndexSettings.Folder = testPath;
            newContextResp.Settings.ErrorIndexSettings.Name = indexName;
            newContextResp.Settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            newContextResp.Settings.SqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            newContextResp.Settings.SqlSettings.InitialCatalog = indexName;
            newContextResp.Settings.SqlSettings.EventsPerBlock = 20;
            newContextResp.Settings.SqlSettings.ConnectionTimeout = 10;
            newContextResp.Settings.SqlSettings.MaxPoolSize = 8;
            newContextResp.Settings.SqlSettings.MinPoolSize = 3;
            m_Utils.SetContextSettings(newContextResp.Settings);

            m_Utils.RestartService();

            // Read them back and make sure they have changed.
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(newContextResp.Settings.ErrorIndexSettings.Folder,
                resp.Settings.ContextCollection[0].ErrorIndexSettings.Folder);
            Assert.AreEqual(newContextResp.Settings.ErrorIndexSettings.Name,
                resp.Settings.ContextCollection[0].ErrorIndexSettings.Name);
            Assert.AreEqual(newContextResp.Settings.ErrorIndexSettings.Type,
                resp.Settings.ContextCollection[0].ErrorIndexSettings.Type);
            Assert.AreEqual(newContextResp.Settings.SqlSettings.ConnectionString,
                resp.Settings.ContextCollection[0].SqlSettings.ConnectionString);
            Assert.AreEqual(newContextResp.Settings.SqlSettings.ConnectionTimeout,
                resp.Settings.ContextCollection[0].SqlSettings.ConnectionTimeout);
            Assert.AreEqual(newContextResp.Settings.SqlSettings.EventsPerBlock,
                resp.Settings.ContextCollection[0].SqlSettings.EventsPerBlock);
            Assert.AreEqual(newContextResp.Settings.SqlSettings.InitialCatalog,
                resp.Settings.ContextCollection[0].SqlSettings.InitialCatalog);
            Assert.AreEqual(newContextResp.Settings.SqlSettings.MaxPoolSize,
                resp.Settings.ContextCollection[0].SqlSettings.MaxPoolSize);
            Assert.AreEqual(newContextResp.Settings.SqlSettings.MinPoolSize,
                resp.Settings.ContextCollection[0].SqlSettings.MinPoolSize);

            
            Assert.AreEqual(ErrorIndexStatus.NotCreated, newContextResp.Settings.ErrorIndexSettings.Status);
            Assert.AreEqual(false, Directory.Exists(Path.Combine(testPath, indexName)));

        }

        [TestMethod]
        public void MoveErrorIndexBeforeCreationSql()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            CreateNewStackHashContextResponse newContextResp = m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);
            Assert.AreEqual(ErrorIndexStatus.NotCreated, newContextResp.Settings.ErrorIndexSettings.Status);

            String originalIndexPath = newContextResp.Settings.ErrorIndexSettings.Folder;
            Assert.AreEqual(false, Directory.Exists(originalIndexPath));

            String testPath = "c:\\stackhashunittests\\testindex\\";
            String indexName = "ShouldNeverExist";

            newContextResp.Settings.ErrorIndexSettings.Folder = testPath;
            newContextResp.Settings.ErrorIndexSettings.Name = indexName;
            newContextResp.Settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;

            m_Utils.SetContextSettings(newContextResp.Settings);

            m_Utils.MoveIndex(0, testPath, indexName, 10000, newContextResp.Settings.SqlSettings);

            // Read them back and make sure they have changed.
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(testPath.ToUpperInvariant(),
                resp.Settings.ContextCollection[0].ErrorIndexSettings.Folder.ToUpperInvariant());
            Assert.AreEqual(indexName.ToUpperInvariant(),
                resp.Settings.ContextCollection[0].ErrorIndexSettings.Name.ToUpperInvariant());
            Assert.AreEqual(ErrorIndexType.SqlExpress,
                resp.Settings.ContextCollection[0].ErrorIndexSettings.Type);

            Assert.AreEqual(ErrorIndexStatus.NotCreated, newContextResp.Settings.ErrorIndexSettings.Status);
            Assert.AreEqual(false, Directory.Exists(Path.Combine(testPath, indexName)));
        }

        [TestMethod]
        public void MoveCannotWhenProfileActive()
        {
            try
            {
                m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
                m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress); // Create a context and give it a non-default name.

                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.NotCreated, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                m_Utils.ActivateContext(0); // Create the index.

                getResp = m_Utils.GetContextSettings();

                String originalIndexPath = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder;
                String originalIndexName = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name;

                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                String indexName = "NewIndexName";
                String testPath = "c:\\stackhashunittests\\testindex\\";

                bool exceptionOccurred = false;
                try
                {
                    m_Utils.MoveIndex(0, testPath, indexName, 20000, getResp.Settings.ContextCollection[0].SqlSettings);
                }
                catch (FaultException<ReceiverFaultDetail> ex)
                {
                    exceptionOccurred = true;
                    Assert.AreEqual(true, ex.Message.Contains("Context must be inactive"));
                }

                Assert.AreEqual(true, exceptionOccurred);

                Assert.AreEqual(true, Directory.Exists(Path.Combine(originalIndexPath, originalIndexName)));
                Assert.AreEqual(false, Directory.Exists(Path.Combine(testPath, indexName)));
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }


        [TestMethod]
        public void MoveEmptyIndex()
        {
            try
            {
                m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
                m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress); // Create a context and give it a non-default name.
                m_Utils.DeleteIndex(0);

                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.NotCreated, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                m_Utils.ActivateContext(0); // Create the index.

                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                String originalIndexPath = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder;
                String originalIndexName = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name;


                String indexName = "NewIndexName";
                String testPath = "c:\\stackhashunittests\\testindex\\";

                // Make sure the destination folder does not exist.
                String fullDestPath = Path.Combine(testPath, indexName);

                if (Directory.Exists(fullDestPath))
                    PathUtils.DeleteDirectory(fullDestPath, true);

                m_Utils.DeactivateContext(0); // Must be inactive before the move.
                m_Utils.MoveIndex(0, testPath, indexName, 200000, getResp.Settings.ContextCollection[0].SqlSettings);

                Assert.AreEqual(false, Directory.Exists(Path.Combine(originalIndexPath, originalIndexName)));
                Assert.AreEqual(true, Directory.Exists(Path.Combine(testPath, indexName)));

                Assert.AreEqual(null, m_Utils.MoveCompleteAdminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.MoveCompleteAdminReport.ResultData);


                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);
                Assert.AreEqual(testPath.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder.ToUpperInvariant());
                Assert.AreEqual(indexName.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name.ToUpperInvariant());
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }
        [TestMethod]
        public void MoveEmptyIndexDestAlreadyExists()
        {
            try
            {
                m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
                m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress); // Create a context and give it a non-default name.

                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.NotCreated, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                m_Utils.ActivateContext(0); // Create the index.

                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                String originalIndexPath = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder;
                String originalIndexName = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name;


                String indexName = "NewIndexName";
                String testPath = "c:\\stackhashunittests\\testindex\\";

                // Make sure the destination folder exists.
                String fullDestPath = Path.Combine(testPath, indexName);

                if (!Directory.Exists(fullDestPath))
                    Directory.CreateDirectory(fullDestPath);

                m_Utils.DeactivateContext(0); // Must be inactive before the move.
                m_Utils.MoveIndex(0, testPath, indexName, 200000, getResp.Settings.ContextCollection[0].SqlSettings);

                Assert.AreEqual(true, Directory.Exists(Path.Combine(originalIndexPath, originalIndexName)));
                Assert.AreEqual(true, Directory.Exists(Path.Combine(testPath, indexName)));

                Assert.AreNotEqual(null, m_Utils.MoveCompleteAdminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_Utils.MoveCompleteAdminReport.ResultData);


                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);
                Assert.AreEqual(originalIndexPath.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder.ToUpperInvariant());
                Assert.AreEqual(originalIndexName.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name.ToUpperInvariant());
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }

        }

        [TestMethod]
        public void MoveNonEmptyIndex()
        {
            try
            {
                m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
                m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress); // Create a context and give it a non-default name.


                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.NotCreated, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                m_Utils.ActivateContext(0); // Create the index.

                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                // Create a small error index.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 1;
                testIndexData.NumberOfEventInfos = 1;
                testIndexData.NumberOfCabs = 1;
                m_Utils.CreateTestIndex(0, testIndexData);

                String originalIndexPath = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder;
                String originalIndexName = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name;


                String indexName = "NewIndexName";
                String testPath = "c:\\stackhashunittests\\testindex\\";

                // TODO: This won't work if testing across machines.

                // Make sure the destination folder does not exist.
                String fullDestPath = Path.Combine(testPath, indexName);

                if (Directory.Exists(fullDestPath))
                    PathUtils.DeleteDirectory(fullDestPath, true);

                m_Utils.DeactivateContext(0); // Must be inactive before the move.
                m_Utils.MoveIndex(0, testPath, indexName, 20000, getResp.Settings.ContextCollection[0].SqlSettings);
                m_Utils.ActivateContext(0); // Must be inactive before the move.

                Assert.AreEqual(false, Directory.Exists(Path.Combine(originalIndexPath, originalIndexName)));
                Assert.AreEqual(true, Directory.Exists(Path.Combine(testPath, indexName)));

                Assert.AreEqual(null, m_Utils.MoveCompleteAdminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.MoveCompleteAdminReport.ResultData);


                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);
                Assert.AreEqual(testPath.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder.ToUpperInvariant());
                Assert.AreEqual(indexName.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name.ToUpperInvariant());

                // Check that the products etc.. have been moved.
                GetProductsResponse products = m_Utils.GetProducts(0);
                Assert.AreEqual(1, products.Products.Count);

                foreach (StackHashProductInfo productInfo in products.Products)
                {
                    StackHashProduct product = productInfo.Product;

                    GetFilesResponse files = m_Utils.GetFiles(0, product);
                    Assert.AreEqual(1, files.Files.Count);

                    foreach (StackHashFile file in files.Files)
                    {
                        GetEventsResponse events = m_Utils.GetEvents(0, product, file);
                        Assert.AreEqual(1, events.Events.Count);

                        foreach (StackHashEvent theEvent in events.Events)
                        {
                            GetEventPackageResponse eventPackage = m_Utils.GetEventPackage(0, product, file, theEvent);

                            Assert.AreEqual(1, eventPackage.EventPackage.Cabs.Count);
                            Assert.AreEqual(1, eventPackage.EventPackage.EventInfoList.Count);
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
    }
}
