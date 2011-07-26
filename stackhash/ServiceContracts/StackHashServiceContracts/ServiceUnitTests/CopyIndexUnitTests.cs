using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.ServiceModel;
using StackHashUtilities;
using System.Globalization;
using System.Data.SqlClient;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for CopyUnitTests
    /// </summary>
    [TestClass]
    public class CopyIndexUnitTests
    {
        Utils m_Utils;
        const String s_TestCab = @"R:\stackhash\BusinessLogic\BusinessLogic\TestData\Cabs\1641909485-Crash32bit-0773522646.cab";
        const String s_DestIndexName = @"UnitTestsCopyIndexName";
        const String s_SourceIndexName = @"AcmeErrorIndex0";

        public CopyIndexUnitTests()
        {
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
            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();

            foreach (StackHashContextSettings settings in getResp.Settings.ContextCollection)
            {
                m_Utils.DeactivateContext(settings.Id);
                m_Utils.DeleteIndex(settings.Id);
                m_Utils.RemoveContext(settings.Id);
            }
            tidyTest();

            if (m_Utils != null)
            {
                m_Utils.Dispose();
                m_Utils = null;
            }

        }

        public void tidyTest()
        {
            StackHashSqlControl.InstallerInterface installerInterface =
                new StackHashSqlControl.InstallerInterface(TestSettings.DefaultConnectionString,
                    "MASTER", "C:\\StackHashDefaultErrorIndex");

            installerInterface.Connect();
            if (installerInterface.DatabaseExists(s_SourceIndexName))
                installerInterface.DeleteDatabase(s_SourceIndexName);
            if (installerInterface.DatabaseExists(s_DestIndexName))
                installerInterface.DeleteDatabase(s_DestIndexName);
            installerInterface.Disconnect();
            SqlConnection.ClearAllPools();

            if (Directory.Exists("C:\\StackHashDefaultErrorIndex"))
                PathUtils.DeleteDirectory("C:\\StackHashDefaultErrorIndex", true);
        }

        String getConnectionString(StackHashSqlConfiguration sqlConfig)
        {
            StringBuilder connectionString = new StringBuilder();
            connectionString.Append(sqlConfig.ConnectionString);

            if (!String.IsNullOrEmpty(sqlConfig.InitialCatalog))
            {
                connectionString.Append(";Initial Catalog=");
                connectionString.Append(sqlConfig.InitialCatalog);
            }

            if (sqlConfig.MinPoolSize != -1)
            {
                connectionString.Append(";Min Pool Size=");
                connectionString.Append(sqlConfig.MinPoolSize.ToString(CultureInfo.InvariantCulture));
            }

            if (sqlConfig.MaxPoolSize != -1)
            {
                connectionString.Append(";Max Pool Size=");
                connectionString.Append(sqlConfig.MaxPoolSize.ToString(CultureInfo.InvariantCulture));
            }

            if (sqlConfig.ConnectionTimeout != -1)
            {
                connectionString.Append(";Connection Timeout=");
                connectionString.Append(sqlConfig.MaxPoolSize.ToString(CultureInfo.InvariantCulture));
            }

            return connectionString.ToString();
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

        public void copyIndex(bool createDestinationIndexLocally, bool switchIndex, StackHashTestIndexData testData)
        {
            try
            {
                m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);
                m_Utils.CreateAndSetNewContext(ErrorIndexType.Xml); // Create a context and give it a non-default name.
                m_Utils.DeleteIndex(0);

                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.NotCreated, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                m_Utils.ActivateContext(0); // Create the index.
                m_Utils.CreateTestIndex(0, testData);


                getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);

                String originalIndexPath = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder;
                String originalIndexName = getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name;

                String destIndexName = "UnitTestsCopyIndexName";
                String destIndexPath = "c:\\stackhashunittests\\copyindexpath\\";

                // Make sure the destination folder does not exist.
                String fullDestPath = Path.Combine(destIndexPath, destIndexName);

                if (Directory.Exists(fullDestPath))
                    PathUtils.DeleteDirectory(fullDestPath, true);


                ErrorIndexSettings destSettings = new ErrorIndexSettings()
                {
                    Folder = destIndexPath,
                    Name = destIndexName,
                    Type = ErrorIndexType.SqlExpress
                };

                StackHashSqlConfiguration sqlConfig = new StackHashSqlConfiguration()
                {
                    ConnectionString = String.Format(TestSettings.DefaultConnectionString, destIndexName),
                    InitialCatalog = destIndexName,
                    MaxPoolSize = 100,
                    MinPoolSize = 2,
                    ConnectionTimeout = 20,    
                };

                if (createDestinationIndexLocally)
                {
                    StackHashSqlControl.InstallerInterface installerInterface =
                        new StackHashSqlControl.InstallerInterface(TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;",
                            destSettings.Name, destSettings.Folder);

                    installerInterface.Connect();
                    installerInterface.CreateDatabase(true);
                    installerInterface.Disconnect();
                    SqlConnection.ClearAllPools();
                }

                m_Utils.DeactivateContext(0);
                m_Utils.CopyIndex(0, destSettings, sqlConfig, switchIndex, 60000);
                m_Utils.ActivateContext(0);

                Assert.AreEqual(true, Directory.Exists(Path.Combine(originalIndexPath, originalIndexName)));
                Assert.AreEqual(true, Directory.Exists(fullDestPath));

                Assert.AreNotEqual(null, m_Utils.CopyCompleteAdminReport);
                Assert.AreEqual(null, m_Utils.CopyCompleteAdminReport.LastException);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.CopyCompleteAdminReport.ResultData);


                getResp = m_Utils.GetContextSettings();

                if (switchIndex)
                {
                    Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);
                    Assert.AreEqual(destSettings.Folder.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder.ToUpperInvariant());
                    Assert.AreEqual(destSettings.Name.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name.ToUpperInvariant());
                }
                else
                {
                    Assert.AreEqual(ErrorIndexStatus.Created, getResp.Settings.ContextCollection[0].ErrorIndexSettings.Status);
                    Assert.AreEqual(originalIndexPath.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Folder.ToUpperInvariant());
                    Assert.AreEqual(originalIndexName.ToUpperInvariant(), getResp.Settings.ContextCollection[0].ErrorIndexSettings.Name.ToUpperInvariant());
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void CopyEmptyIndexToSqlNoSwitch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 0,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            copyIndex(false, false, testIndexData);
        }

        [TestMethod]
        public void CopyNonEmptyIndexToSqlNoSwitch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 1,
                NumberOfFiles = 2,
                NumberOfEvents = 3,
                NumberOfEventInfos = 4,
                NumberOfCabs = 4,
                NumberOfEventNotes = 5,
                NumberOfCabNotes = 5,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            copyIndex(false, false, testIndexData);
        }

        
        [TestMethod]
        public void CopyEmptyIndexToSqlSwitch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 0,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            copyIndex(false, true, testIndexData);
        }

        [TestMethod]
        public void CopyNonEmptyIndexToSqlSwitch()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 1,
                NumberOfFiles = 2,
                NumberOfEvents = 3,
                NumberOfEventInfos = 4,
                NumberOfCabs = 4,
                NumberOfEventNotes = 5,
                NumberOfCabNotes = 5,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            copyIndex(false, true, testIndexData);
        }

#if ISAYSO

        [TestMethod]
        public void CopyEmptyIndexToSqlSwitchCreateLocally()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 0,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            copyIndex(true, true, testIndexData);
        }


        [TestMethod]
        public void CopyNonEmptyIndexToSqlSwitchCreateLocally()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 1,
                NumberOfFiles = 2,
                NumberOfEvents = 3,
                NumberOfEventInfos = 4,
                NumberOfCabs = 4,
                NumberOfEventNotes = 5,
                NumberOfCabNotes = 5,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            copyIndex(true, true, testIndexData);
        }

        [TestMethod]
        public void CopyEmptyIndexToSqlNoSwitchCreateLocally()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 0,
                NumberOfFiles = 0,
                NumberOfEvents = 0,
                NumberOfEventInfos = 0,
                NumberOfCabs = 0,
                NumberOfEventNotes = 0,
                NumberOfCabNotes = 0,
                UnwrapCabs = false,
                CabFileName = s_TestCab
            };

            copyIndex(true, false, testIndexData);
        }

        [TestMethod]
        public void CopyNonEmptyIndexToSqlNoSwitchCreateLocally()
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData
            {
                NumberOfProducts = 1,
                NumberOfFiles = 2,
                NumberOfEvents = 3,
                NumberOfEventInfos = 4,
                NumberOfCabs = 4,
                NumberOfEventNotes = 5,
                NumberOfCabNotes = 5,
                UnwrapCabs = true,
                CabFileName = s_TestCab
            };

            copyIndex(true, false, testIndexData);
        }

#endif

    }
}
