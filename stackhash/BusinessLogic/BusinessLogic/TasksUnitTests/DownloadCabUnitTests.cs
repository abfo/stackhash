using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashDebug;
using StackHashUtilities;
using StackHashWinQual;

namespace TasksUnitTests
{
    /// <summary>
    /// Tests the task that downloads cabs from the WinQual site directly without
    /// going through all products, files, events etc...
    /// </summary>
    [TestClass]
    public class DownloadCabUnitTests
    {
        List<AdminReportEventArgs> m_AdminReports = new List<AdminReportEventArgs>();
        AutoResetEvent m_DownloadCompleteEvent = new AutoResetEvent(false);
        private static String s_LicenseId = "800766a4-0e2c-4ba5-8bb7-2045cf43a892";
        private static String s_TestServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";

        public DownloadCabUnitTests()
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
        //[ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) 
        // {
        // }

        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        //public static void MyClassCleanup() 
        //{

        //}

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_AdminReports.Clear();
            m_DownloadCompleteEvent.Reset();
        }

        [TestCleanup()]
        public void MyTestCleanup() { }

        #endregion


        /// <summary>
        /// Called by the contoller context objects to report an admin event 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnAdminReport(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminArgs = e as AdminReportEventArgs;
            if (adminArgs.Report.Operation != StackHashAdminOperation.ContextStateChanged)
                m_AdminReports.Add(adminArgs);


            if (adminArgs.Report.Operation == StackHashAdminOperation.DownloadCabCompleted)
                m_DownloadCompleteEvent.Set();
        }


        private void waitForDownloadCompleted(int timeout)
        {
            if (!m_DownloadCompleteEvent.WaitOne(timeout))
                throw new TimeoutException("Timed out waiting for cab download to complete");
            
        }




        [TestMethod]
        [Ignore]
        public void DownloadCabProductDoesNotExist()
        {
            String errorIndexFolder = Path.GetTempPath() + "StackHashCabDownloadTests";
            String errorIndexName = "CabDownloads";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                contextSettings.WinQualSettings = new WinQualSettings("UserName", "Password", "Company", 10, 
                    new StackHashProductSyncDataCollection(), false, 0, 1, WinQualSettings.DefaultSyncsBeforeResync, false);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, StackHashTestData.Default, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the download task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);


                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.files.com", 12345678, "CuckuBackup", 10, 1, "1.2.3.4");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 23, DateTime.Now, "FileName", "2.3.4.5");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 10, new StackHashEventSignature(), 1, 23);
                StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventTypeName", "this.cab", 100, 10000);


                try
                {
                    controllerContext.RunDownloadCabTask(clientData, product, file, theEvent, cab, false);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual(true, ex.Message.Contains("Product does not exist"));
                }
                controllerContext.Deactivate();
                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
            }
            finally
            {
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public void DownloadCabAllOkDummy()
        {
            String errorIndexFolder = Path.GetTempPath() + "StackHashCabDownloadTests";
            String errorIndexName = "CabDownloads";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                contextSettings.WinQualSettings = new WinQualSettings("UserName", "Password", "Company", 10,
                    new StackHashProductSyncDataCollection(), false, 0, 1, WinQualSettings.DefaultSyncsBeforeResync, false);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, true, StackHashTestData.Default, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the download task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);


                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.files.com", 12345678, "CuckuBackup", 10, 1, "1.2.3.4");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 23, DateTime.Now, "FileName", "2.3.4.5");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 10, new StackHashEventSignature(), 1, 23);
                StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventTypeName", "this.cab", 100, 10000);

                controllerContext.ErrorIndex.AddProduct(product);
                controllerContext.ErrorIndex.AddFile(product, file);
                controllerContext.ErrorIndex.AddEvent(product, file, theEvent);

                cab.DumpAnalysis = new StackHashDumpAnalysis();
                cab.DumpAnalysis.DotNetVersion = "xxx";
                controllerContext.ErrorIndex.AddCab(product, file, theEvent, cab, false);


                controllerContext.RunDownloadCabTask(clientData, product, file, theEvent, cab, false);

                // Wait for the download task to complete.
                waitForDownloadCompleted(30000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabStarted, m_AdminReports[0].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[0].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[0].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[0].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[0].Report.ClientData.ClientRequestId);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabCompleted, m_AdminReports[1].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[1].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[1].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[1].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[1].Report.ClientData.ClientRequestId);

                // Get the cab data to ensure that it is set to downloaded.
                StackHashCabCollection cabs = controllerContext.ErrorIndex.LoadCabList(product, file, theEvent);
                Assert.AreEqual(true, cabs[0].CabDownloaded);

                Assert.AreEqual("xxx", cabs[0].DumpAnalysis.DotNetVersion);

                controllerContext.Deactivate();
                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
            }
            finally
            {
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }


        [TestMethod]
        [Ignore]
        public void DownloadCabAllOkLive()
        {
            String errorIndexFolder = Path.GetTempPath() + "StackHashCabDownloadTests";
            String errorIndexName = "CabDownloads";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                contextSettings.WinQualSettings = new WinQualSettings(TestSettings.WinQualUserName, TestSettings.WinQualPassword, "Company", 10,
                    new StackHashProductSyncDataCollection(), false, 0, 1, WinQualSettings.DefaultSyncsBeforeResync, false);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, false, null, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the download task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);


                // Most of these params are real taking from Crashy.
                StackHashProduct product = new StackHashProduct(DateTime.Parse("2010-04-16T22:31:02Z"), DateTime.Parse("2010-07-07T08:20:58Z"), 
                    @"https://winqual.microsoft.com/Services/wer/user/files.aspx?productid=25299", 25299, "Crashy", 10, 0, "1.2.3.4");
                StackHashFile file = new StackHashFile(DateTime.Parse("2010-04-19T00:15:00Z"), DateTime.Parse("2010-04-19T00:15:00Z"), 
                    4232330, DateTime.Parse("2010-04-17T05:21:32Z"), "Crashy.exe", "1.2.3.4");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Parse("2010-04-19T10:20:00Z"), DateTime.Parse("2010-04-19T10:20:00Z"),
                    "CLR20 Managed Crash", 1099299922, new StackHashEventSignature(), 13, 4232330);
                StackHashCab cab = new StackHashCab(DateTime.Parse("2010-04-21T16:49:00Z"), DateTime.Parse("2010-04-21T16:49:00Z"),
                    1099299922, "CLR20 Managed Crash", "1099299922-CLR20ManagedCrash-0837914903.cab", 837914903, 9313620);

                controllerContext.ErrorIndex.AddProduct(product);
                controllerContext.ErrorIndex.AddFile(product, file);
                controllerContext.ErrorIndex.AddEvent(product, file, theEvent);

                cab.DumpAnalysis = new StackHashDumpAnalysis();
                cab.DumpAnalysis.DotNetVersion = "xxx";
                controllerContext.ErrorIndex.AddCab(product, file, theEvent, cab, true);


                controllerContext.RunDownloadCabTask(clientData, product, file, theEvent, cab, false);

                // Wait for the download task to complete - could take 5 mins to download.
                waitForDownloadCompleted(5 * 60000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabStarted, m_AdminReports[0].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[0].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[0].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[0].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[0].Report.ClientData.ClientRequestId);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_AdminReports[0].Report.ResultData);
                Assert.AreEqual(StackHashServiceErrorCode.NoError, m_AdminReports[0].Report.ServiceErrorCode);

                Assert.AreEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabCompleted, m_AdminReports[1].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[1].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[1].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[1].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[1].Report.ClientData.ClientRequestId);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_AdminReports[1].Report.ResultData);
                Assert.AreEqual(StackHashServiceErrorCode.CabDoesNotExist, m_AdminReports[1].Report.ServiceErrorCode);

                controllerContext.Deactivate();
                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
            }
            finally
            {
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }


        /// <summary>
        /// Try downloading a cab with an unknown cab Id - the event id etc... are all valid.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void DownloadCabAllOkRandomCabId()
        {
            String errorIndexFolder = Path.GetTempPath() + "StackHashCabDownloadTests";
            String errorIndexName = "CabDownloads";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                contextSettings.WinQualSettings = new WinQualSettings(TestSettings.WinQualUserName, TestSettings.WinQualPassword, "Company", 10,
                    new StackHashProductSyncDataCollection(), false, 0, 1, WinQualSettings.DefaultSyncsBeforeResync, false);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, false, null, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the download task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);


                // Most of these params are real taking from Crashy.
                StackHashProduct product = new StackHashProduct(DateTime.Parse("2010-04-16T22:31:02Z"), DateTime.Parse("2010-07-07T08:20:58Z"),
                    @"https://winqual.microsoft.com/Services/wer/user/files.aspx?productid=25299", 25299, "Crashy", 10, 0, "1.2.3.4");
                StackHashFile file = new StackHashFile(DateTime.Parse("2010-04-19T00:15:00Z"), DateTime.Parse("2010-04-19T00:15:00Z"),
                    4232330, DateTime.Parse("2010-04-17T05:21:32Z"), "Crashy.exe", "1.2.3.4");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Parse("2010-04-19T10:20:00Z"), DateTime.Parse("2010-04-19T10:20:00Z"),
                    "CLR20 Managed Crash", 1099299922, new StackHashEventSignature(), 13, 4232330);
                StackHashCab cab = new StackHashCab(DateTime.Parse("2010-04-21T16:49:00Z"), DateTime.Parse("2010-04-21T16:49:00Z"),
                    1099299922, "CLR20 Managed Crash", "1099299922-CLR20ManagedCrash-0837914903.cab", 939529172, /* 856326908 */ /* 600247507 */ 9313620);

                controllerContext.ErrorIndex.AddProduct(product);
                controllerContext.ErrorIndex.AddFile(product, file);
                controllerContext.ErrorIndex.AddEvent(product, file, theEvent);
                controllerContext.ErrorIndex.AddCab(product, file, theEvent, cab, false);


                controllerContext.RunDownloadCabTask(clientData, product, file, theEvent, cab, false);

                // Wait for the download task to complete - could take 5 mins to download.
                waitForDownloadCompleted(5 * 60000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabStarted, m_AdminReports[0].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[0].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[0].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[0].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[0].Report.ClientData.ClientRequestId);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_AdminReports[0].Report.ResultData);

                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabCompleted, m_AdminReports[1].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[1].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[1].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[1].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[1].Report.ClientData.ClientRequestId);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_AdminReports[1].Report.ResultData);


                Assert.AreNotEqual(null, m_AdminReports[1].Report.LastException);

                if ((m_AdminReports[1].Report.ServiceErrorCode != StackHashServiceErrorCode.CabDoesNotExist) &&
                    (m_AdminReports[1].Report.ServiceErrorCode != StackHashServiceErrorCode.ServerDown))
                    Assert.AreEqual(StackHashServiceErrorCode.CabDownloadFailed, m_AdminReports[1].Report.ServiceErrorCode);
                
                controllerContext.Deactivate();
                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
            }
            finally
            {
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }


        /// <summary>
        /// Try downloading a cab with an invalid cab Id - the event id etc... are all valid.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void DownloadCabIdInvalid()
        {
            String errorIndexFolder = Path.GetTempPath() + "StackHashCabDownloadTests";
            String errorIndexName = "CabDownloads";
            String scriptFolder = errorIndexFolder + "\\Scripts";

            if (Directory.Exists(errorIndexFolder))
            {
                PathUtils.SetFilesWritable(errorIndexFolder, true);
                PathUtils.DeleteDirectory(errorIndexFolder, true);
            }

            Directory.CreateDirectory(errorIndexFolder);
            Directory.CreateDirectory(scriptFolder);

            try
            {
                // Create a settings manager and a new context.
                SettingsManager settingsManager = new SettingsManager(errorIndexFolder + "\\ServiceSettings.XML");
                StackHashContextSettings contextSettings = settingsManager.CreateNewContextSettings();

                contextSettings.WinQualSettings = new WinQualSettings(TestSettings.WinQualUserName, TestSettings.WinQualPassword, "Company", 10,
                    new StackHashProductSyncDataCollection(), false, 0, 1, WinQualSettings.DefaultSyncsBeforeResync, false);

                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = errorIndexFolder;
                contextSettings.ErrorIndexSettings.Name = errorIndexName;

                ScriptManager scriptManager = new ScriptManager(scriptFolder);

                string licenseFileName = string.Format("{0}\\License.bin", errorIndexFolder);
                LicenseManager licenseManager = new LicenseManager(licenseFileName, s_TestServiceGuid);
                licenseManager.SetLicense(s_LicenseId);

                ControllerContext controllerContext = new ControllerContext(contextSettings, scriptManager, new Windbg(),
                    settingsManager, false, null, licenseManager);

                // Activate the context and the associated index.
                controllerContext.Activate();

                // Hook up to receive admin reports.
                controllerContext.AdminReports += new EventHandler<AdminReportEventArgs>(this.OnAdminReport);

                // Run the download task.
                Guid guid = new Guid();

                StackHashClientData clientData = new StackHashClientData(guid, "GuidName", 1);


                // Most of these params are real taking from Crashy.
                StackHashProduct product = new StackHashProduct(DateTime.Parse("2010-04-16T22:31:02Z"), DateTime.Parse("2010-07-07T08:20:58Z"),
                    @"https://winqual.microsoft.com/Services/wer/user/files.aspx?productid=25299", 25299, "Crashy", 10, 0, "1.2.3.4");
                StackHashFile file = new StackHashFile(DateTime.Parse("2010-04-19T00:15:00Z"), DateTime.Parse("2010-04-19T00:15:00Z"),
                    4232330, DateTime.Parse("2010-04-17T05:21:32Z"), "Crashy.exe", "1.2.3.4");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Parse("2010-04-19T10:20:00Z"), DateTime.Parse("2010-04-19T10:20:00Z"),
                    "CLR20 Managed Crash", 1099299922, new StackHashEventSignature(), 13, 4232330);
                StackHashCab cab = new StackHashCab(DateTime.Parse("2010-04-21T16:49:00Z"), DateTime.Parse("2010-04-21T16:49:00Z"),
                    1099299922, "CLR20 Managed Crash", "1099299922-CLR20ManagedCrash-0837914903.cab", 0x12345678, 9313620);

                controllerContext.ErrorIndex.AddProduct(product);
                controllerContext.ErrorIndex.AddFile(product, file);
                controllerContext.ErrorIndex.AddEvent(product, file, theEvent);
                controllerContext.ErrorIndex.AddCab(product, file, theEvent, cab, true);


                controllerContext.RunDownloadCabTask(clientData, product, file, theEvent, cab, false);

                // Wait for the download task to complete - could take 5 mins to download.
                waitForDownloadCompleted(5 * 60000);

                Assert.AreEqual(2, m_AdminReports.Count);

                Assert.AreEqual(null, m_AdminReports[0].Report.LastException);
                Assert.AreEqual(0, m_AdminReports[0].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabStarted, m_AdminReports[0].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[0].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[0].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[0].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[0].Report.ClientData.ClientRequestId);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_AdminReports[0].Report.ResultData);

                Assert.AreEqual(0, m_AdminReports[1].Report.ContextId);
                Assert.AreEqual(StackHashAdminOperation.DownloadCabCompleted, m_AdminReports[1].Report.Operation);
                Assert.AreEqual(clientData.ApplicationGuid, m_AdminReports[1].Report.ClientData.ApplicationGuid);
                Assert.AreEqual(clientData.ClientId, m_AdminReports[1].Report.ClientData.ClientId);
                Assert.AreEqual(clientData.ClientName, m_AdminReports[1].Report.ClientData.ClientName);
                Assert.AreEqual(clientData.ClientRequestId, m_AdminReports[1].Report.ClientData.ClientRequestId);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_AdminReports[1].Report.ResultData);


                Assert.AreNotEqual(null, m_AdminReports[1].Report.LastException);
                Assert.AreEqual(StackHashServiceErrorCode.CabDoesNotExist, m_AdminReports[1].Report.ServiceErrorCode);

                controllerContext.Deactivate();
                controllerContext.AdminReports -= new EventHandler<AdminReportEventArgs>(this.OnAdminReport);
            }
            finally
            {
                if (Directory.Exists(errorIndexFolder))
                {
                    PathUtils.SetFilesWritable(errorIndexFolder, true);
                    PathUtils.DeleteDirectory(errorIndexFolder, true);
                }
            }
        }
    }
}
