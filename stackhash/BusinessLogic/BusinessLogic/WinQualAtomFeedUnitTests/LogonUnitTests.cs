using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Security;

using StackHashBusinessObjects;
using StackHashUtilities;
using WinQualAtomFeed;


namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Tests the logon to WinQual.
    /// </summary>
    [TestClass]
    public class LogonUnitTests
    {
        public LogonUnitTests()
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
        public void LogonWithBadCredentials()
        {
            if (!AtomTestSettings.EnableWinQualLogOnTests)
                return;

            // Define the certificate policy for the application.
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(MyCertificateValidation.ValidateServerCertificate);
            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            try
            {
                Assert.AreEqual(false, atomFeed.Login("Username", "Password"));
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(true,
                                ex.Message.Contains("Please check that your Windows live id username and password are correct"));
            }
        }

        [TestMethod]
        public void LogonWithGoodCredentials()
        {
            if (!AtomTestSettings.EnableWinQualLogOnTests)
                return;

            // Define the certificate policy for the application.
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(MyCertificateValidation.ValidateServerCertificate);

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            atomFeed.LogOut();
        }

        // Need to sort out the proxy settings for the new liveid.
        // Enable this test when fixed.
        [TestMethod]
        [Ignore]
        public void LogonWithGoodCredentialsInvalidProxy()
        {
            if (!AtomTestSettings.EnableWinQualLogOnTests)
                return;

            // Define the certificate policy for the application.
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(MyCertificateValidation.ValidateServerCertificate);

            StackHashProxySettings proxySettings = new StackHashProxySettings(true, false, "Buddy", 200, null, null, null);

            AtomFeed atomFeed = new AtomFeed(proxySettings, 1, 100000, false, true, null, 11);

            Assert.AreEqual(false, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));
        }
    }
}
