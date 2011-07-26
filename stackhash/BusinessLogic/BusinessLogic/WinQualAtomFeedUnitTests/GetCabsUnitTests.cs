using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;
using WinQualAtomFeed;
using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashWinQual;
using StackHashUtilities;

namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Summary description for GetCabsUnitTests
    /// </summary>
    [TestClass]
    public class GetCabsUnitTests
    {
        public GetCabsUnitTests()
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

        // This test could potentially take a long time.
        [TestMethod]
        [Ignore]
        public void GetCabs()
        {
            if (!AtomTestSettings.EnableWinQualCabDownloadTests)
                return;

            DateTime startTime = DateTime.Now.AddDays(-89);

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            try
            {
                // ATOM GetProducts.
                AtomProductCollection atomProducts = atomFeed.GetProducts();

                bool foundCab = false;

                foreach (AtomProduct atomProduct in atomProducts)
                {
                    AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);

                    foreach (AtomFile atomFile in atomFiles)
                    {
                        AtomEventCollection atomEvents = atomFeed.GetEvents(atomFile, startTime);

                        foreach (AtomEvent atomEvent in atomEvents)
                        {
                            AtomCabCollection eventCabs = atomFeed.GetCabs(atomEvent);

                            foreach (AtomCab eventCab in eventCabs)
                            {
                                Assert.AreEqual(true, eventCab.Cab.DateCreatedLocal< DateTime.Now);
                                foundCab = true;
                            }

                            if (foundCab)
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

        // This test could potentially take a long time.
        [TestMethod]
        [Ignore]
        public void DownloadCab()
        {
            if (!AtomTestSettings.EnableWinQualCabDownloadTests)
                return;

            DateTime startTime = DateTime.Now.AddDays(-89);

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, true, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            try
            {
                // ATOM GetProducts.
                AtomProductCollection atomProducts = atomFeed.GetProducts();

                bool foundCab = false;

                foreach (AtomProduct atomProduct in atomProducts)
                {
                    AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);

                    foreach (AtomFile atomFile in atomFiles)
                    {
                        AtomEventCollection atomEvents = atomFeed.GetEvents(atomFile, startTime);

                        foreach (AtomEvent atomEvent in atomEvents)
                        {
                            AtomCabCollection atomCabs = atomFeed.GetCabs(atomEvent);

                            foreach (AtomCab atomCab in atomCabs)
                            {
                                Assert.AreEqual(true, atomCab.Cab.DateCreatedLocal< DateTime.Now);

                                String tempFolder = Path.GetTempPath();

                                String fileName = atomFeed.DownloadCab(atomCab, true, tempFolder);

                                try
                                {
                                    Assert.AreEqual(true, File.Exists(fileName));
                                    FileInfo fileInfo = new FileInfo(fileName);

                                    Assert.AreEqual(atomCab.Cab.SizeInBytes, fileInfo.Length);
                                }
                                finally
                                {
                                    if (File.Exists(fileName))
                                        File.Delete(fileName);
                                }

                                foundCab = true;
                            }

                            if (foundCab)
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

        [TestMethod]
        [Ignore]
        public void DownloadSingleCab()
        {
            if (!AtomTestSettings.EnableWinQualCabDownloadTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 1, 100000, false, false, null, 11);

            // ATOM LOGIN.
            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            // ATOM GetProducts.
            AtomProductCollection atomProducts = atomFeed.GetProducts();


            // WERAPI Login.
            Login login = new Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword);
            login.Validate();

            foreach (AtomProduct atomProduct in atomProducts)
            {
                Console.WriteLine("Processing product " + atomProduct.Product.Name + " " + atomProduct.Product.Id.ToString());

                // ATOM GetFiles.
                AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);

                foreach (AtomFile atomFile in atomFiles)
                {
                    Console.WriteLine("Processing file " + atomFile.File.Name + " " + atomFile.File.Id.ToString());
                    // ATOM GetEvents.
                    AtomEventCollection atomEvents = atomFeed.GetEvents(atomFile);

                    foreach (AtomEvent atomEvent in atomEvents)
                    {
                        Console.WriteLine("Processing event " + atomEvent.Event.Id.ToString());

                        // ATOM GetCabs.
                        AtomCabCollection atomCabs = atomFeed.GetCabs(atomEvent);

                        if (atomCabs.Count != 0)
                        {
                            StackHashCab cab = atomCabs[0].Cab;

                            // Convert back to an AtomCab.
                            AtomCab newCab = new AtomCab(cab, AtomCab.MakeLink(atomEvent.Event.EventTypeName, atomEvent.Event.Id, cab.Id, cab.SizeInBytes));

                            Console.WriteLine("Downloading cab " + cab.Id.ToString());

                            String tempFolder = Path.GetTempPath();

                            String fileName = atomFeed.DownloadCab(newCab, true, tempFolder);

                            try
                            {
                                Assert.AreEqual(true, File.Exists(fileName));
                                FileInfo fileInfo = new FileInfo(fileName);

                                Assert.AreEqual(cab.SizeInBytes, fileInfo.Length);
                            }
                            finally
                            {
                                if (File.Exists(fileName))
                                    File.Delete(fileName);
                            }

                            // 1 is enough.
                            return;
                        }
                    }

                }
            }
        }

        [TestMethod]
        [Ignore]
        public void DownloadSpecificCab()
        {
            if (!AtomTestSettings.EnableWinQualCabDownloadTests)
                return;

            AtomFeed atomFeed = new AtomFeed(null, 2, 100000, false, true, null, 11);

            // ATOM LOGIN.
            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 1886116627, "CLR20 Managed Crash", "fred.txt", 1188442827, 924210);
            AtomCab newCab = new AtomCab(cab, AtomCab.MakeLink("CLR20 Managed Crash", 1886116627, 1188442827, 924210));

            String tempFolder = Path.GetTempPath();

            String fileName = atomFeed.DownloadCab(newCab, true, tempFolder);

            try
            {
                Assert.AreEqual(true, File.Exists(fileName));
                FileInfo fileInfo = new FileInfo(fileName);

                Assert.AreEqual(946386, fileInfo.Length);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }
    }
}
