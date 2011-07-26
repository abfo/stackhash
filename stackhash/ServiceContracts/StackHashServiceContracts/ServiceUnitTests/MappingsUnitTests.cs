using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for MappingsUnitTests
    /// </summary>
    [TestClass]
    public class MappingsUnitTests
    {
        private Utils m_Utils;

        public MappingsUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();
            m_Utils.SetProxy(null);
            m_Utils.RemoveAllContexts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                try
                {
                    m_Utils.DeactivateContext(0);
                    m_Utils.DeleteIndex(0);
                }
                catch
                {
                }
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


        // This test requires a valid mapping file for one of your products to run. 
        [TestMethod]
        public void UploadValidMappingsFile()
        {

            String mappingFileText = File.ReadAllText(@"R:\stackhash\BusinessLogic\BusinessLogic\TestData\MappingFiles\1.0.4511.261.xml");

            if (!File.Exists(mappingFileText))
                return;

            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);

            m_Utils.ActivateContext(0);
            UploadMappingFileResponse resp = m_Utils.UploadMappingFile(0, mappingFileText, 60000);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Check the admin report.
            Assert.AreEqual(null, m_Utils.UploadMappingFileAdminReport.LastException);
            Assert.AreEqual(StackHashServiceErrorCode.NoError, m_Utils.UploadMappingFileAdminReport.ServiceErrorCode);
        }

        [TestMethod]
        [Ignore]
        public void UploadInvalidMappingsFile()
        {
            String mappingFileText = "StackHash Unit Test for testing invalid mapping file upload - runs twice a day";

            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);

            m_Utils.ActivateContext(0);
            UploadMappingFileResponse resp = m_Utils.UploadMappingFile(0, mappingFileText, 120000);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Check the admin report.
            Assert.AreNotEqual(null, m_Utils.UploadMappingFileAdminReport.LastException);
            Assert.AreEqual(StackHashServiceErrorCode.InvalidMappingFileFormat, m_Utils.UploadMappingFileAdminReport.ServiceErrorCode);
        }
    }
}
