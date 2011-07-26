using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Security.Cryptography;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for LicenseDataUnitTests
    /// </summary>
    [TestClass]
    public class LicenseDataUnitTests
    {
        public LicenseDataUnitTests()
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

        /// <summary>
        /// Construct a StackHashLicenseData and check all the properties.
        /// </summary>
        [TestMethod]
        public void Constructor()
        {
            String licenseId = "License ID7657654876587658765";
            String companyName = "Company Name";
            String departmentName = "Department Name";
            long maxEvents = 0x12345678;
            int maxSeats = 25;
            DateTime expiryUtc = DateTime.Now.AddDays(1);
            bool isTrial = true;

            StackHashLicenseData licenseData = new StackHashLicenseData(true, licenseId, companyName, departmentName, maxEvents, maxSeats, expiryUtc, isTrial);

            Assert.AreEqual(licenseId, licenseData.LicenseId);
            Assert.AreEqual(companyName, licenseData.CompanyName);
            Assert.AreEqual(departmentName, licenseData.DepartmentName);
            Assert.AreEqual(maxEvents, licenseData.MaxEvents);
            Assert.AreEqual(maxSeats, licenseData.MaxSeats);
            Assert.AreEqual(expiryUtc, licenseData.ExpiryUtc);
            Assert.AreEqual(isTrial, licenseData.IsTrialLicense);
        }

        /// <summary>
        /// Set properties.
        /// </summary>
        [TestMethod]
        public void SetProperties()
        {
            String licenseId = "License ID7657654876587658765";
            String companyName = "Company Name";
            String departmentName = "Department Name";
            long maxEvents = 0x12345678;
            int maxSeats = 25;
            DateTime expiryUtc = DateTime.Now.AddDays(1);
            bool isTrial = true;

            StackHashLicenseData licenseData = new StackHashLicenseData(true, "jih", ";oiu", "oiuy", 21, 22010, DateTime.Now.ToUniversalTime(), false);

            licenseData.LicenseId = licenseId;
            licenseData.CompanyName = companyName;
            licenseData.DepartmentName = departmentName;
            licenseData.MaxEvents = maxEvents;
            licenseData.MaxSeats = maxSeats;
            licenseData.ExpiryUtc = expiryUtc;
            licenseData.IsTrialLicense = isTrial;

            Assert.AreEqual(licenseId, licenseData.LicenseId);
            Assert.AreEqual(companyName, licenseData.CompanyName);
            Assert.AreEqual(departmentName, licenseData.DepartmentName);
            Assert.AreEqual(maxEvents, licenseData.MaxEvents);
            Assert.AreEqual(maxSeats, licenseData.MaxSeats);
            Assert.AreEqual(expiryUtc, licenseData.ExpiryUtc);
            Assert.AreEqual(isTrial, licenseData.IsTrialLicense);
        }

        
        /// <summary>
        /// Save and load.
        /// </summary>
        [TestMethod]
        public void SaveLoad()
        {
            String licenseId = "License ID7657654876587658765";
            String companyName = "Company Name";
            String departmentName = "Department Name";
            long maxEvents = 0x12345678;
            int maxSeats = 25;
            DateTime expiryUtc = DateTime.Now.AddDays(1);
            bool isTrial = true;


            StackHashLicenseData licenseData = new StackHashLicenseData(true, licenseId, companyName, departmentName, maxEvents, maxSeats, expiryUtc, isTrial);

            String tempFileName = Path.GetTempFileName();

            try
            {
                licenseData.Save(tempFileName);
                StackHashLicenseData loadedLicenseData = StackHashLicenseData.Load(tempFileName);


                Assert.AreEqual(licenseId, loadedLicenseData.LicenseId);
                Assert.AreEqual(companyName, loadedLicenseData.CompanyName);
                Assert.AreEqual(departmentName, loadedLicenseData.DepartmentName);
                Assert.AreEqual(maxEvents, loadedLicenseData.MaxEvents);
                Assert.AreEqual(maxSeats, loadedLicenseData.MaxSeats);
                Assert.AreEqual(expiryUtc, loadedLicenseData.ExpiryUtc);
                Assert.AreEqual(isTrial, loadedLicenseData.IsTrialLicense);
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        /// <summary>
        /// Save and load and Save and load.
        /// </summary>
        [TestMethod]
        public void SaveLoadSaveLoad()
        {
            String licenseId = "License ID7657654876587658765";
            String companyName = "Company Name";
            String departmentName = "Department Name";
            long maxEvents = 0x12345678;
            int maxSeats = 25;
            DateTime expiryUtc = DateTime.Now.AddDays(1);
            bool isTrial = true;

            String licenseId2 = "License ID76576548765876587652";
            String companyName2 = "Company Name2";
            String departmentName2 = "Department Name2";
            long maxEvents2 = 0x123456782;
            int maxSeats2 = 252;
            DateTime expiryUtc2 = DateTime.Now.AddDays(12);
            bool isTrial2 = false;

            StackHashLicenseData licenseData = new StackHashLicenseData(true, licenseId, companyName, departmentName, maxEvents, maxSeats, expiryUtc, isTrial);

            String tempFileName = Path.GetTempFileName();

            try
            {
                licenseData.Save(tempFileName);
                StackHashLicenseData loadedLicenseData = StackHashLicenseData.Load(tempFileName);


                Assert.AreEqual(licenseId, loadedLicenseData.LicenseId);
                Assert.AreEqual(companyName, loadedLicenseData.CompanyName);
                Assert.AreEqual(departmentName, loadedLicenseData.DepartmentName);
                Assert.AreEqual(maxEvents, loadedLicenseData.MaxEvents);
                Assert.AreEqual(maxSeats, loadedLicenseData.MaxSeats);
                Assert.AreEqual(expiryUtc, loadedLicenseData.ExpiryUtc);
                Assert.AreEqual(isTrial, loadedLicenseData.IsTrialLicense);

                licenseData.LicenseId = licenseId2;
                licenseData.CompanyName = companyName2;
                licenseData.DepartmentName = departmentName2;
                licenseData.MaxEvents = maxEvents2;
                licenseData.MaxSeats = maxSeats2;
                licenseData.ExpiryUtc = expiryUtc2;
                licenseData.IsTrialLicense = isTrial2;

                licenseData.Save(tempFileName);
                StackHashLicenseData loadedLicenseData2 = StackHashLicenseData.Load(tempFileName);

                Assert.AreEqual(licenseId2, loadedLicenseData2.LicenseId);
                Assert.AreEqual(companyName2, loadedLicenseData2.CompanyName);
                Assert.AreEqual(departmentName2, loadedLicenseData2.DepartmentName);
                Assert.AreEqual(maxEvents2, loadedLicenseData2.MaxEvents);
                Assert.AreEqual(maxSeats2, loadedLicenseData2.MaxSeats);
                Assert.AreEqual(expiryUtc2, loadedLicenseData2.ExpiryUtc);
                Assert.AreEqual(isTrial2, loadedLicenseData2.IsTrialLicense);
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        /// <summary>
        /// Saving the same data twice should result in identical data.
        /// This isn't true - there is additional random session data added for each call 
        /// so the output WILL be different for each call.
        /// </summary>
        [TestMethod]
        public void SaveTwiceCompareFiles()
        {
            String licenseId = "License ID7657654876587658765";
            String companyName = "Company Name";
            String departmentName = "Department Name";
            long maxEvents = 0x12345678;
            int maxSeats = 25;
            DateTime expiryUtc = DateTime.Now.AddDays(1);
            bool isTrial = true;


            StackHashLicenseData licenseData = new StackHashLicenseData(true, licenseId, companyName, departmentName, maxEvents, maxSeats, expiryUtc, isTrial);

            String tempFileName = Path.GetTempFileName();
            String tempFileName2 = Path.GetTempFileName();

            try
            {
                licenseData.Save(tempFileName);
                licenseData.Save(tempFileName2);

                StackHashLicenseData loadedLicenseData = StackHashLicenseData.Load(tempFileName);

                Assert.AreEqual(licenseId, loadedLicenseData.LicenseId);
                Assert.AreEqual(companyName, loadedLicenseData.CompanyName);
                Assert.AreEqual(departmentName, loadedLicenseData.DepartmentName);
                Assert.AreEqual(maxEvents, loadedLicenseData.MaxEvents);
                Assert.AreEqual(maxSeats, loadedLicenseData.MaxSeats);
                Assert.AreEqual(expiryUtc, loadedLicenseData.ExpiryUtc);
                Assert.AreEqual(isTrial, loadedLicenseData.IsTrialLicense);

                loadedLicenseData = StackHashLicenseData.Load(tempFileName2);

                Assert.AreEqual(licenseId, loadedLicenseData.LicenseId);
                Assert.AreEqual(companyName, loadedLicenseData.CompanyName);
                Assert.AreEqual(departmentName, loadedLicenseData.DepartmentName);
                Assert.AreEqual(maxEvents, loadedLicenseData.MaxEvents);
                Assert.AreEqual(maxSeats, loadedLicenseData.MaxSeats);
                Assert.AreEqual(expiryUtc, loadedLicenseData.ExpiryUtc);
                Assert.AreEqual(isTrial, loadedLicenseData.IsTrialLicense);

                // Compare the files.
                byte[] licenseBytes1 = File.ReadAllBytes(tempFileName);
                byte[] licenseBytes2 = File.ReadAllBytes(tempFileName2);

                bool match = true;
                Assert.AreEqual(licenseBytes1.Length, licenseBytes2.Length);

                for (int i = 0; i < licenseBytes2.Length; i++)
                {
                    if (licenseBytes1[i] == licenseBytes2[i])
                        match = false;
                }

                // Should not match.
                Assert.AreEqual(false, match);
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        /// <summary>
        /// Encrypt the same license data twice with LocalMachine account should be the same.
        /// This isn't true - there is additional random session data added for each call 
        /// so the output WILL be different for each call.
        /// </summary>
        [TestMethod]
        public void EncryptingSameLicenseDataTwiceShouldBeSame()
        {
            String licenseId = "License ID7657654876587658765";
            String companyName = "Company Name";
            String departmentName = "Department Name";
            long maxEvents = 0x12345678;
            int maxSeats = 25;
            DateTime expiryUtc = DateTime.Now.AddDays(1);
            bool isTrial = true;


            StackHashLicenseData licenseData = new StackHashLicenseData(true, licenseId, companyName, departmentName, maxEvents, maxSeats, expiryUtc, isTrial);

            byte[] bytes1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] bytes2 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            byte[] encryptedBytes1 = ProtectedData.Protect(bytes1, null, DataProtectionScope.LocalMachine);
            byte[] encryptedBytes2 = ProtectedData.Protect(bytes1, null, DataProtectionScope.LocalMachine);

            bool match = true;

            for (int i = 0; i < encryptedBytes1.Length; i++)
            {
                if (encryptedBytes1[i] != encryptedBytes2[i])
                    match = false;
            }

            // Should not match
            Assert.AreEqual(false, match);
        }
    }
}
