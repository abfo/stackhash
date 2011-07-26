using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using StackHashBusinessObjects;
using WinQualAtomFeed;
using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashWinQual;
using StackHashUtilities;

namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Summary description for GetProductsUnitTests
    /// </summary>
    [TestClass]
    public class GetProductsUnitTests
    {
        public GetProductsUnitTests()
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
        public void GetProducts()
        {
            if (!AtomTestSettings.EnableWinQualProductTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            // ATOM LOGIN.
            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            try
            {
                AtomProductCollection products = atomFeed.GetProducts();

                Assert.AreEqual(true, products.Count > 0);

                Assert.AreEqual(true, products.DateFeedUpdated < DateTime.Now);
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


        [TestMethod]
        public void GetProducts2Calls()
        {
            if (!AtomTestSettings.EnableWinQualProductTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            // ATOM LOGIN.
            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            try
            {
                AtomProductCollection products = atomFeed.GetProducts();
                Assert.AreEqual(true, products.Count > 0);

                AtomProductCollection products2 = atomFeed.GetProducts();
                Assert.AreEqual(products.Count, products2.Count);
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
        [TestMethod]
        [Ignore]
        public void GetProductsCalls24Hours()
        {
            if (!AtomTestSettings.EnableWinQualProductTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 10, 100000, false, true, null, 11);

            // ATOM LOGIN.
            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));


            int count = 0;
            DateTime startTime = DateTime.Now;

            try
            {

                Console.WriteLine("StartTime = " + startTime.ToString());


                while (true)
                {
                    count++;
                    Console.WriteLine("Count: " + count.ToString());

                    AtomProductCollection products = atomFeed.GetProducts();
                    Assert.AreEqual(true, products.Count > 10);

                    // Wait for 15 mins before retrying.
                    Thread.Sleep(15 * 60 * 1000);
                }
            }
            catch (System.Exception ex)
            {
                DateTime endTime = DateTime.Now;
                Console.WriteLine("Crashed " + ex.ToString());
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
