using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashUtilities;

namespace StackHashUtilitiesUnitTests
{
    /// <summary>
    /// Summary description for SecurityUnitTest
    /// </summary>
    [TestClass]
    public class SecurityUnitTest
    {
        public SecurityUnitTest()
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
        public void EncryptNullString()
        {
            String encryptedString = SecurityUtils.EncryptStringWithUserCredentials(null);
            Assert.AreEqual(String.Empty, encryptedString);
        }

        [TestMethod]
        public void DecryptNullString()
        {
            String decyptedString = SecurityUtils.DecryptStringWithUserCredentials(null);
            Assert.AreEqual(String.Empty, decyptedString);
        }

        [TestMethod]
        public void EncryptEmptyString()
        {
            String encryptedString = SecurityUtils.EncryptStringWithUserCredentials("");
            Assert.AreEqual(String.Empty, encryptedString);
        }

        [TestMethod]
        public void DecryptEmptyString()
        {
            String decyptedString = SecurityUtils.DecryptStringWithUserCredentials("");
            Assert.AreEqual(String.Empty, decyptedString);
        }

        [TestMethod]
        public void EncryptDecrypt1Char()
        {
            String plainTextString = "A";
            String encryptedString = SecurityUtils.EncryptStringWithUserCredentials(plainTextString);
            String decryptedString = SecurityUtils.DecryptStringWithUserCredentials(encryptedString);
            Assert.AreEqual(plainTextString, decryptedString);
        }

        [TestMethod]
        public void EncryptDecryptManyChars()
        {
            String plainTextString = "ABCdeF1012_  sd~@!__123";
            String encryptedString = SecurityUtils.EncryptStringWithUserCredentials(plainTextString);
            String decryptedString = SecurityUtils.DecryptStringWithUserCredentials(encryptedString);
            Assert.AreEqual(plainTextString, decryptedString);
        }

        [TestMethod]
        [Ignore] // Doesn't actually work - seems that Protect returns different arrays for the same input values
        public void EncryptSameStringTwice()
        {
            String plainTextString = "ABCdeF1012_  sd~@!__123";
            String encryptedString1 = SecurityUtils.EncryptStringWithUserCredentials(plainTextString);
            String encryptedString2 = SecurityUtils.EncryptStringWithUserCredentials(plainTextString);
            Assert.AreEqual(encryptedString1, encryptedString2);
        }

        [TestMethod]
        public void EncryptDecryptStringManyTimes()
        {
            String plainTextString = "ABCdeF1012_  sd~@!__123";
            String decryptedString = plainTextString;
            for (int i = 0; i < 10; i++)
            {
                String encryptedString = SecurityUtils.EncryptStringWithUserCredentials(decryptedString);
                decryptedString = SecurityUtils.DecryptStringWithUserCredentials(encryptedString);
                Assert.AreEqual(plainTextString, decryptedString);
            }
        }
    }
}
