using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;
using WinQualAtomFeed;
using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashWinQual;
using StackHashUtilities;



namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Summary description for GetFilesUnitTests
    /// </summary>
    [TestClass]
    public class GetFilesUnitTests
    {
        public GetFilesUnitTests()
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
        public void GetFiles()
        {
            if (!AtomTestSettings.EnableWinQualFilesTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));


            try
            {
                // ATOM GetProducts.
                AtomProductCollection atomProducts = atomFeed.GetProducts();

                foreach (AtomProduct atomProduct in atomProducts)
                {
                    AtomFileCollection files = atomFeed.GetFiles(atomProduct);

                    Assert.AreEqual(true, files.Count > 0);
                }
            }
            finally
            {
                try
                {
                    atomFeed.LogOut();
                }
                catch
                {
                }
            }


        }
    }
}
