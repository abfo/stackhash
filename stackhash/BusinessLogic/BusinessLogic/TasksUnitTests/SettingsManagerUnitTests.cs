using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using StackHashTasks;
using StackHashBusinessObjects;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for SettingsManagerUnitTests
    /// </summary>
    [TestClass]
    public class SettingsManagerUnitTests
    {
        public SettingsManagerUnitTests()
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
        public void ProductSyncListDefault()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settings = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settings.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);
                Assert.AreEqual(0, contextSettings.WinQualSettings.ProductsToSynchronize.Count);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }

        [TestMethod]
        public void ProductSyncListAddOneEntry()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);
                settingsManager.SetProductSynchronization(0, 10, true);
                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);

                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);

                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }

        [TestMethod]
        public void ProductSyncListAddOneEntryTwice()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);
                settingsManager.SetProductSynchronization(0, 10, true);
                settingsManager.SetProductSynchronization(0, 10, true);
                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);


                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);

                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }


        
        [TestMethod]
        public void ProductSyncListAdd2Products()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);
                settingsManager.SetProductSynchronization(0, 10, true);
                settingsManager.SetProductSynchronization(0, 85, true);

                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(2, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);

                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(85);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(85, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(2, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);


                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(85);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(85, productSyncData.ProductId);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }

        [TestMethod]
        public void ProductSyncListAddRemove1()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);

                settingsManager.SetProductSynchronization(0, 10, true);
                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                settingsManager.SetProductSynchronization(0, 10, false);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(0, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(0, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }

        [TestMethod]
        public void ProductSyncListAddRemoveNotPresent()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);

                settingsManager.SetProductSynchronization(0, 10, true);
                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                settingsManager.SetProductSynchronization(0, 20, false);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }


        [TestMethod]
        public void ProductSyncListAddTwoEntriesRemoveFirst()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);

                settingsManager.SetProductSynchronization(0, 10, true);
                settingsManager.SetProductSynchronization(0, 20, true);
                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(2, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(20, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(2, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(20, productSyncData.ProductId);

                settingsManager.SetProductSynchronization(0, 10, false);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(20, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(20, productSyncData.ProductId);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }
        [TestMethod]
        public void ProductSyncListAddTwoEntriesRemoveSecond()
        {
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            try
            {
                SettingsManager settingsManager = new SettingsManager(settingsFileName);
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();
                Assert.AreNotEqual(null, contextSettings.WinQualSettings.ProductsToSynchronize);

                settingsManager.SetProductSynchronization(0, 10, true);
                settingsManager.SetProductSynchronization(0, 20, true);
                StackHashSettings currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(2, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                StackHashProductSyncData productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(20, productSyncData.ProductId);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(2, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(20, productSyncData.ProductId);

                settingsManager.SetProductSynchronization(0, 20, false);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreEqual(null, productSyncData);

                // Check the settings are persisted.
                settingsManager = new SettingsManager(settingsFileName);
                currentSettings = settingsManager.CurrentSettings;
                Assert.AreEqual(1, currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.Count);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(10);
                Assert.AreNotEqual(null, productSyncData);
                Assert.AreEqual(10, productSyncData.ProductId);
                productSyncData = currentSettings.ContextCollection[0].WinQualSettings.ProductsToSynchronize.FindProduct(20);
                Assert.AreEqual(null, productSyncData);
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
            }
        }


        /// <summary>
        /// Settings XML Error - No BAK file: Creates SAV and reconstructs Settings.
        /// </summary>
        [TestMethod]
        public void CorruptSettingsFileZeroLengthNoBackup()
        {
            // This will actually create a file of length 0.
            String settingsFileName = Path.GetTempFileName();
            String bakFile = settingsFileName.Replace(".tmp", ".bak");
            String savFile = settingsFileName.Replace(".tmp", ".sav");
            String copFile = settingsFileName.Replace(".tmp", ".cop");

            try
            {
                // Make a copy of the corrupt file for comparison later.
                File.Copy(settingsFileName, copFile, true);

                SettingsManager settingsManager = new SettingsManager(settingsFileName);

                Assert.AreEqual(0, settingsManager.CurrentSettings.ContextCollection.Count);
                Assert.AreEqual(0, settingsManager.CurrentSettings.NextContextId);

                Assert.AreEqual(true, File.Exists(savFile));
                Assert.AreEqual(false, File.Exists(bakFile));

                Assert.AreEqual(true, compareFiles(savFile, copFile));
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
                if (File.Exists(bakFile))
                    File.Delete(bakFile);
                if (File.Exists(savFile))
                    File.Delete(savFile);
                if (File.Exists(copFile))
                    File.Delete(copFile);
            }
        }

        private bool compareFiles(String fileName1, String fileName2)
        {
            if (!File.Exists(fileName1))
                return false;

            if (!File.Exists(fileName2))
                return false;

            byte[] file1Bytes = File.ReadAllBytes(fileName1);
            byte[] file2Bytes = File.ReadAllBytes(fileName2);

            if (file1Bytes.Length != file2Bytes.Length)
                return false;

            for (int fileOffset = 0; fileOffset < file1Bytes.Length; fileOffset++)
            {
                if (file1Bytes[fileOffset] != file2Bytes[fileOffset])
                    return false;
            }

            return true;
        }


        private void corruptFile(String fileName)
        {
            FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            try
            {
                int numBytes = (int)(fileStream.Length / 2);

                fileStream.Seek(numBytes, SeekOrigin.Begin);

                byte [] bytes = new byte [numBytes];

                fileStream.Write(bytes, 0, numBytes - 1);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// Settings XML Error - BAK file exists: Creates SAV and reconstructs Settings from BAK.
        /// </summary>
        [TestMethod]
        public void CorruptSettingsBackupOk()
        {
            // This will actually create a file of length 0.
            String settingsFileName = Path.GetTempFileName();
            String bakFile = settingsFileName.Replace(".tmp", ".bak");
            String savFile = settingsFileName.Replace(".tmp", ".sav");
            String copFile = settingsFileName.Replace(".tmp", ".cop");

            try
            {
                // Delete the temp file so that the settings will be created normally.
                File.Delete(settingsFileName);
                SettingsManager settingsManager = new SettingsManager(settingsFileName);

                // Make a manual copy of the settings.
                File.Copy(settingsFileName, bakFile, true);

                // Corrupt the main settings file.
                corruptFile(settingsFileName);

                // Make a copy of the corrupt file for comparison later.
                File.Copy(settingsFileName, copFile, true);

                // This time it should see the corrupt settings file and load the backup file.
                // The corrupt settings file should be copied to the SAV file.
                settingsManager = new SettingsManager(settingsFileName);

                Assert.AreEqual(true, File.Exists(savFile));
                Assert.AreEqual(true, File.Exists(bakFile));

                Assert.AreEqual(true, compareFiles(savFile, copFile));

                // Files will differ by GUID.
                // Assert.AreEqual(true, compareFiles(settingsFileName, bakFile));
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
                if (File.Exists(bakFile))
                    File.Delete(bakFile);
                if (File.Exists(savFile))
                    File.Delete(savFile);
                if (File.Exists(copFile))
                    File.Delete(copFile);
            }
        }


        /// <summary>
        /// Settings XML Error - BAK file error: Creates SAV and reconstructs Settings from scratch.
        /// </summary>
        [TestMethod]
        public void CorruptSettingsBackupCorrupt()
        {
            // This will actually create a file of length 0.
            String settingsFileName = Path.GetTempFileName();
            String bakFile = settingsFileName.Replace(".tmp", ".bak");
            String savFile = settingsFileName.Replace(".tmp", ".sav");
            String copFile = settingsFileName.Replace(".tmp", ".cop");

            try
            {
                // Delete the temp file so that the settings will be created normally.
                File.Delete(settingsFileName);
                SettingsManager settingsManager = new SettingsManager(settingsFileName);

                // Corrupt the main settings file.
                corruptFile(settingsFileName);

                // Make a manual copy of the settings - corrupt also.
                File.Copy(settingsFileName, bakFile, true);


                // Make a copy of the corrupt file for comparison later.
                File.Copy(settingsFileName, copFile, true);

                // This time it should see the corrupt settings file and load the backup file. However,
                // the backup file is also corrupt so the settings should be reconstructed.
                // The corrupt settings file should be copied to the SAV file.
                settingsManager = new SettingsManager(settingsFileName);

                Assert.AreEqual(true, File.Exists(savFile));
                Assert.AreEqual(true, File.Exists(bakFile));

                Assert.AreEqual(true, compareFiles(savFile, copFile));
                Assert.AreEqual(false, compareFiles(settingsFileName, bakFile)); // Settings should be fixed.
            }
            finally
            {
                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
                if (File.Exists(bakFile))
                    File.Delete(bakFile);
                if (File.Exists(savFile))
                    File.Delete(savFile);
                if (File.Exists(copFile))
                    File.Delete(copFile);
            }
        }

        /// <summary>
        /// Settings Error (access) - BAK file exists: Should just exception.
        /// </summary>
        [TestMethod]
        public void CorruptSettingsAccessError()
        {
            // This will actually create a file of length 0.
            String settingsFileName = Path.GetTempFileName();
            String bakFile = settingsFileName.Replace(".tmp", ".bak");
            String savFile = settingsFileName.Replace(".tmp", ".sav");
            String copFile = settingsFileName.Replace(".tmp", ".cop");
            FileStream fileStream = null;

            try
            {
                // Make a copy of the corrupt file for comparison later.
                File.Copy(settingsFileName, copFile, true);

                // Hold the file open so the settings manager cannot open it.
                fileStream = File.Open(settingsFileName, FileMode.Open, FileAccess.Read, FileShare.None);

                try
                {
                    SettingsManager settingsManager = new SettingsManager(settingsFileName);
                }
                catch (IOException)
                {
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                Assert.AreEqual(false, File.Exists(savFile));
                Assert.AreEqual(false, File.Exists(bakFile));

                Assert.AreEqual(true, compareFiles(settingsFileName, copFile));
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();

                if (File.Exists(settingsFileName))
                    File.Delete(settingsFileName);
                if (File.Exists(bakFile))
                    File.Delete(bakFile);
                if (File.Exists(savFile))
                    File.Delete(savFile);
                if (File.Exists(copFile))
                    File.Delete(copFile);
            }
        }

        /// <summary>
        /// Load settings - fresh - should create a guid.
        /// </summary>
        [TestMethod]
        public void ServiceGuidCreationCheck()
        {
            // This will actually create a file of length 0.
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            Assert.AreNotEqual(null, settingsManager.ServiceGuid);
        }

        /// <summary>
        /// Reload settings - fresh - should be same guid.
        /// </summary>
        [TestMethod]
        public void ServiceGuidCreationCheckReload()
        {
            // This will actually create a file of length 0.
            String settingsFileName = Path.GetTempFileName();
            File.Delete(settingsFileName);

            SettingsManager settingsManager = new SettingsManager(settingsFileName);
            String guid = settingsManager.ServiceGuid;

            settingsManager = new SettingsManager(settingsFileName);

            Assert.AreEqual(guid, settingsManager.ServiceGuid);
        }
    }
}
