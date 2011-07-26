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
    /// Summary description for GetEventsUnitTests
    /// </summary>
    [TestClass]
    public class GetEventsUnitTests
    {
        public GetEventsUnitTests()
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
        public void GetEvents()
        {
            if (!AtomTestSettings.EnableWinQualEventsTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));


            try
            {
                AtomProductCollection atomProducts = atomFeed.GetProducts();

                int totalEvents = 0;


                // Just keep going until one set of events has been found.
                foreach (AtomProduct atomProduct in atomProducts)
                {
                    // ATOM GetFiles.
                    AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);

                    foreach (AtomFile atomFile in atomFiles)
                    {
                        AtomEventCollection atomEvents = atomFeed.GetEvents(atomFile, DateTime.Now.AddDays(-89));

                        foreach (AtomEvent atomEvent in atomEvents)
                        {
                            Assert.AreEqual(true, atomEvent.Event.Id != 0);
                            totalEvents++;
                        }

                        if (totalEvents > 0)
                            return;
                    }
                }
            }
            finally
            {
                try { atomFeed.LogOut(); }
                catch { }
            }
        }


        [TestMethod]
        [Ignore]
        public void GetEventsStartDate()
        {
            DateTime startTime = DateTime.Now.AddMonths(-1);

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, false, null, 11);

            // ATOM LOGIN.
            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            // ATOM GetProducts.
            AtomProductCollection atomProducts = atomFeed.GetProducts();


            // WERAPI Login.
            Login login = new Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword);
            login.Validate();

            // WERAPI GetProducts.
            ProductCollection apiProducts = Product.GetProducts(ref login);

            Assert.AreEqual(atomProducts.Count, apiProducts.Count);

            bool eventListsDifferent = false;

            foreach (AtomProduct atomProduct in atomProducts)
            {
                // ATOM GetFiles.
                AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);

                // API GetFiles
                Product apiProduct = Utils.FindProduct(apiProducts, atomProduct.Product.Id);
                ApplicationFileCollection apiFiles = apiProduct.GetApplicationFiles(ref login);

                Assert.AreEqual(atomFiles.Count, apiFiles.Count);

                foreach (AtomFile atomFile in atomFiles)
                {
                    ApplicationFile apiFile = Utils.FindFile(apiFiles, atomFile.File.Id);

                    // ATOM GetEvents.
                    StackHashEventCollection atomStackHashEvents = Utils.GetEventsAtom(atomFeed, atomFile, startTime);
                    StackHashEventCollection atomStackHashEventsNoTime = Utils.GetEventsAtom(atomFeed, atomFile);

                    if (atomStackHashEvents.Count != atomStackHashEventsNoTime.Count)
                        eventListsDifferent = true;

                    // WERAPI GetEvents.
                    List<Event> rawApiEvents;
                    StackHashEventCollection apiStackHashEvents = Utils.GetEventsApi(ref login, apiFile, out rawApiEvents, startTime);

                    Assert.AreEqual(apiStackHashEvents.Count, atomStackHashEvents.Count);

                    foreach (StackHashEvent stackHashEvent in atomStackHashEvents)
                    {
                        // Find the equivalent event in the api file list.
                        StackHashEvent apiEvent = apiStackHashEvents.FindEvent(stackHashEvent.Id, stackHashEvent.EventTypeName);

                        Assert.AreNotEqual(null, apiEvent);

                        Assert.AreEqual(true, stackHashEvent.CompareTo(apiEvent) == 0);
                    }

                }
            }

            Assert.AreEqual(true, eventListsDifferent);
        }

        [TestMethod]
        public void GetEventInfos()
        {
            if (!AtomTestSettings.EnableWinQualEventsTests)
                return;

            DateTime startTime = DateTime.Now.AddMonths(-1);

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            try
            {
                // ATOM GetProducts.
                AtomProductCollection atomProducts = atomFeed.GetProducts();

                bool foundAnEventInfo = false;

                foreach (AtomProduct atomProduct in atomProducts)
                {
                    AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);

                    foreach (AtomFile atomFile in atomFiles)
                    {
                        AtomEventCollection atomEvents = atomFeed.GetEvents(atomFile, startTime);

                        foreach (AtomEvent atomEvent in atomEvents)
                        {
                            AtomEventInfoCollection eventInfos = atomFeed.GetEventDetails(atomEvent, 89);

                            foreach (AtomEventInfo eventInfo in eventInfos)
                            {
                                Assert.AreEqual(true, eventInfo.EventInfo.DateCreatedLocal < DateTime.Now);
                                foundAnEventInfo = true;
                            }

                            if (foundAnEventInfo)
                                return;
                        }

                    }
                }
                Assert.AreEqual(true, false); // Should get here.
            }
            finally
            {
                try { atomFeed.LogOut(); }
                catch { }
            }
        }
    }
}
