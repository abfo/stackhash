using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.ServiceModel;
using System.IO;
using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FaultUnitTests
    {
        private TestContext testContextInstance;
        private Utils m_Utils;
        private String m_LogFolder;

        public FaultUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();
            m_Utils.DisableLogging();
            // Delete any log files that may be lying around.
            m_LogFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "StackHash\\Test\\logs");
            Utils.DeleteLogFiles(m_LogFolder);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.Dispose();
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
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void FaultSettingsNull()
        {
            String diagnosticsFileName = Path.Combine(m_LogFolder, "StackHashServiceDiagnosticsLog_00000001.txt");

            double size1 = 0;
            double size2 = 0;

            try
            {
                m_Utils.EnableLogging();
                FileInfo info = new FileInfo(diagnosticsFileName);
                size1 = info.Length;
                SetStackHashPropertiesResponse resp = m_Utils.SetContextSettings(null);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                FileInfo info = new FileInfo(diagnosticsFileName);
                size2 = info.Length;
                m_Utils.DisableLogging();
                Assert.AreEqual(true, ex.Message.Contains("Value cannot be null"));
                Assert.AreEqual(true, ex.Message.Contains("contextSettings"));
                Assert.AreEqual(StackHashServiceErrorCode.UnexpectedError, ex.Detail.ServiceErrorCode);
                Assert.AreEqual(true, size1 < size2);
                throw;
            }        
        }


    }
}
