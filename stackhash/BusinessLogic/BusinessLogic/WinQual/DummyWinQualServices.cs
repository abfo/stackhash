using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashErrorIndex;

namespace StackHashWinQual
{
    /// <summary>
    /// Dummy WinQual service interface. 
    /// </summary>
    public class DummyWinQualServices : IWinQualServices
    {
        private bool m_AbortRequested;
        private StackHashProductSyncDataCollection m_ProductsToSynchronize;
        private StackHashCollectionPolicyCollection m_CollectionPolicyCollection;
        private int m_MaxConsecutiveCabDownloadFailures;
        private StackHashTestDummyWinQualSettings m_TestSettings;
        private long m_MaxEvents;
        private StackHashSyncProgress m_SyncProgress;
        private bool m_EnableNewProductsAutomatically;

        #region IWinQualServices Members

        /// <summary>
        /// Hook up to receive progress reports from the sync process.
        /// </summary>
        public event EventHandler<WinQualProgressEventArgs> Progress;

        /// <summary>
        /// Represents a list of product IDs that are to be synchronized during a SynchronizeWithWinQualOnline call.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get
            {
                return m_ProductsToSynchronize;
            }
            set
            {
                // Make a clone as this may change.
                m_ProductsToSynchronize = new StackHashProductSyncDataCollection();

                if (value != null)
                {
                    foreach (StackHashProductSyncData productSyncData in value)
                    {
                        m_ProductsToSynchronize.Add(new StackHashProductSyncData(productSyncData.ProductId));
                    }
                }
            }
        }

        /// <summary>
        /// The point at which the sync operation reached before it stopped.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get
            {
                return m_SyncProgress;
            }
        }

        /// <summary>
        /// Determines if newly added products should be synced as they are found.
        /// </summary>
        public bool EnableNewProductsAutomatically
        {
            get { return m_EnableNewProductsAutomatically; }
            set { m_EnableNewProductsAutomatically = value; }
        }

        /// <summary>
        /// Set this property to control how cabs are collected for a particular policy.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashCollectionPolicyCollection CollectionPolicy
        {
            get { return m_CollectionPolicyCollection; }
            set { m_CollectionPolicyCollection = value; }
        }

        /// <summary>
        /// The max number of cab downloads allowed before the task is aborted.
        /// </summary>
        public int MaxConsecutiveCabDownloadFailures
        {
            get { return m_MaxConsecutiveCabDownloadFailures; }
            set { m_MaxConsecutiveCabDownloadFailures = value; }
        }

        /// <summary>
        /// Constructs the dummy.
        /// </summary>
        /// <param name="testData">Test data used to control the dummy behaviour.</param>
        public DummyWinQualServices(StackHashTestDummyWinQualSettings testData)
        {
            m_TestSettings = testData;
        }

        /// <summary>
        /// Sets the license restrictions for the index.
        /// </summary>
        /// <param name="maxEvents">The maximum number of events permitted to be downloaded.</param>
        public void SetLicenseRestrictions(long maxEvents)
        {
            m_MaxEvents = maxEvents;
        }

        /// <summary>
        /// Logs on to the WER online service. The service will exception if it cannot log on.
        /// </summary>
        /// <param name="userName">WER username</param>
        /// <param name="password">WER password</param>
        /// <returns>True - successful logon. False - failed to logon.</returns>
        public bool LogOn(string userName, string password)
        {
            if (m_TestSettings.FailLogOn)
                throw new StackHashException("Unable to login", StackHashServiceErrorCode.WinQualLogOnFailed);
            return true;
        }

        /// <summary>
        /// Logs on to the WinQual service.
        /// </summary>
        public void LogOff()
        {
        }

        /// <summary>
        /// Set the proxy settings for web requests.
        /// </summary>
        /// <param name="proxySettings">New proxy settings.</param>
        public void SetProxySettings(StackHashProxySettings proxySettings)
        { 
        }

        private void onProgress(WinQualProgressType progressType)
        {
            EventHandler<WinQualProgressEventArgs> handler = Progress;

            if (handler != null)
            {
                handler(this, new WinQualProgressEventArgs(progressType, null));
            }
        }

        private bool isExcludedProduct(int productId)
        {
            if (m_ProductsToSynchronize == null)
                return true;
            else
                return (m_ProductsToSynchronize.FindProduct(productId) == null);
        }

        /// <summary>
        /// Reports progress for the sync.
        /// </summary>
        /// <param name="progressType">The type of progress being reported.</param>
        /// <param name="product">The product being reported on.</param>
        private void onProgress(WinQualProgressType progressType, StackHashProduct product)
        {
            onProgress(progressType, product, null, null, null, 0, 0, new DateTime(0), new DateTime(0), new DateTime(0));
        }


        /// <summary>
        /// Reports event page progress for the sync.
        /// </summary>
        /// <param name="progressType">The type of progress being reported.</param>
        /// <param name="product">The product being reported on.</param>
        /// <param name="file">The file being reported on.</param>
        /// <param name="theEvent">The event being reported on.</param>
        /// <param name="cab">The cab being reported on.</param>
        /// <param name="currentPage">The current page being downloaded.</param>
        /// <param name="totalPages">Total pages to download.</param>
        /// <param name="lastProductSyncStarted">Time the product sync started.</param>
        /// <param name="lastProductSyncCompleted">Time the product sync completed.</param>
        /// <param name="lastSuccessfulStarted">Time the product sync was started and successfully completed.</param>
        private void onProgress(WinQualProgressType progressType, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab,
            int currentPage, int totalPages, DateTime lastProductSyncStarted, DateTime lastProductSyncCompleted, DateTime lastSuccessfulStarted)
        {
            EventHandler<WinQualProgressEventArgs> handler = Progress;

            if (handler != null)
            {
                StackHashProductInfo productInfo = null;
                if (product != null)
                {
                    productInfo = new StackHashProductInfo((StackHashProduct)product.Clone(), !isExcludedProduct(product.Id),
                        lastSuccessfulStarted.ToUniversalTime(), new StackHashProductSyncData(), lastProductSyncCompleted.ToUniversalTime(),
                        lastProductSyncStarted.ToUniversalTime());
                }

                handler(this, new WinQualProgressEventArgs(progressType, productInfo, file, theEvent, cab, currentPage, totalPages));
            }
        }

        /// <summary>
        /// Starts a sync to download products, files, events, event infos and cabs.
        /// Data is added to the specified error index according to the directions specified
        /// in the testdata passed into the constructor.
        /// </summary>
        /// <param name="errorIndex">The index to add the new data to.</param>
        /// <param name="forceResynchronize">True - forces a resync of every event that is available. False - sync new events only.</param>
        /// <param name="lastProgress">Point at which the last sync attempt reached.</param>
        public void SynchronizeWithWinQualOnline(StackHashErrorIndex.IErrorIndex errorIndex, bool forceResynchronize, StackHashSyncProgress lastProgress)
        {
            if (errorIndex == null)
                throw new ArgumentNullException("errorIndex");

            int fileId = 1;
            int eventId = 1;
            int cabId = 1;
            m_SyncProgress = new StackHashSyncProgress();

            m_AbortRequested = false;
            onProgress(WinQualProgressType.DownloadingProductList, null);
            m_SyncProgress.SyncPhase = StackHashSyncPhase.ProductsOnly;
            errorIndex.SetLastSyncTimeLocal(-2, DateTime.Now.AddDays(-14));

            for (int i = 0; i < m_TestSettings.ObjectsToCreate.NumberOfProducts; i++)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                StackHashProduct product = new StackHashProduct();
                product.DateCreatedLocal = DateTime.Now.ToUniversalTime();
                product.DateModifiedLocal = product.DateCreatedLocal;
                product.FilesLink = "http://www.cucku.com";
                product.Id = i + 1;
                product.Name = "Product" + (i + 1).ToString(CultureInfo.InvariantCulture);
                product.TotalEvents = i;
                product.TotalResponses = i;
                product.Version = "1.2.3." + i.ToString(CultureInfo.InvariantCulture);

                if (!errorIndex.ProductExists(product))
                {
                    if (m_EnableNewProductsAutomatically)
                        m_ProductsToSynchronize.Add(new StackHashProductSyncData(product.Id));

                    errorIndex.AddProduct(product);
                }
            }
            onProgress(WinQualProgressType.ProductListUpdated, null);

            DateTime eventInfoDate = DateTime.Now.Date;


            if (m_TestSettings.FailSync)
                throw new StackHashException("Test sync failure", StackHashServiceErrorCode.SqlConnectionError);

            m_SyncProgress.SyncPhase = StackHashSyncPhase.EventInfosAndCabs;
            for (int i = 0; i < m_TestSettings.ObjectsToCreate.NumberOfProducts; i++)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                DateTime productSyncStartTime = DateTime.Now;

                StackHashProduct product = new StackHashProduct();
                product.DateCreatedLocal = DateTime.Now.ToUniversalTime();
                product.DateModifiedLocal = product.DateCreatedLocal;
                product.FilesLink = "http://www.cucku.com";
                product.Id = i + 1;
                product.Name = "Product" + (i + 1).ToString(CultureInfo.InvariantCulture);
                product.TotalEvents = i;
                product.TotalResponses = i;
                product.Version = "1.2.3." + i.ToString(CultureInfo.InvariantCulture);

                m_SyncProgress.ProductId = product.Id;

                errorIndex.AddProduct(product);
                if (isExcludedProduct(product.Id))
                    continue;

                if (m_TestSettings.ObjectsToCreate.DuplicateFileIdsAcrossProducts)
                {
                    eventId = 1;
                    cabId = 1;
                }

                for (int j = 0; j < m_TestSettings.ObjectsToCreate.NumberOfFiles; j++)
                {
                    if (m_AbortRequested)
                        throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                    StackHashFile file = new StackHashFile();
                    file.DateCreatedLocal = DateTime.Now.ToUniversalTime();
                    file.DateModifiedLocal = DateTime.Now.ToUniversalTime();
                    file.Id = fileId;
                    file.LinkDateLocal = DateTime.Now.ToUniversalTime();
                    file.Name = "File" + fileId.ToString(CultureInfo.InvariantCulture);
                    file.Version = "1.2.3." + fileId.ToString(CultureInfo.InvariantCulture);

                    m_SyncProgress.FileId = file.Id;

                    errorIndex.AddFile(product, file);
                    fileId++;

                    onProgress(WinQualProgressType.DownloadingProductEvents, product, null, null, null, 0, 0,
                        productSyncStartTime, new DateTime(0), errorIndex.GetLastSyncTimeLocal(product.Id));
                    onProgress(WinQualProgressType.DownloadingProductCabs, product, null, null, null, 0, 0,
                        productSyncStartTime, new DateTime(0), errorIndex.GetLastSyncTimeLocal(product.Id));

                    for (int k = 0; k < m_TestSettings.ObjectsToCreate.NumberOfEvents; k++)
                    {
                        if (m_AbortRequested)
                            throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                        if (errorIndex.TotalStoredEvents >= m_MaxEvents)
                            break;

                        StackHashEvent theEvent = new StackHashEvent();
                        theEvent.DateCreatedLocal = DateTime.Now.ToUniversalTime();
                        theEvent.DateModifiedLocal = theEvent.DateCreatedLocal;
                        theEvent.EventTypeName = "CLR";
                        theEvent.FileId = fileId;
                        theEvent.Id = eventId++;
                        theEvent.EventSignature = new StackHashEventSignature();
                        theEvent.EventSignature.ApplicationName = "AppName";
                        theEvent.EventSignature.ApplicationTimeStamp = DateTime.Now.ToUniversalTime();
                        theEvent.EventSignature.ApplicationVersion = "1.2.3.4";
                        theEvent.EventSignature.ExceptionCode = 123;
                        theEvent.EventSignature.ModuleName = "ModuleName";
                        theEvent.EventSignature.ModuleTimeStamp = DateTime.Now.ToUniversalTime();
                        theEvent.EventSignature.ModuleVersion = "4.3.2.1";
                        theEvent.EventSignature.Offset = 12;
                        theEvent.EventSignature.Parameters = new StackHashParameterCollection();
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("Param", "Value"));

                        if (m_TestSettings.ObjectsToCreate.SetBugId)
                        {
                            theEvent.BugId = "BugId" + theEvent.Id.ToString(CultureInfo.InvariantCulture);
                            theEvent.PlugInBugId = "PlugInOriginalId" + theEvent.Id.ToString(CultureInfo.InvariantCulture);
                        }
                        errorIndex.AddEvent(product, file, theEvent);
                        m_SyncProgress.EventId = theEvent.Id;
                        m_SyncProgress.EventTypeName = theEvent.EventTypeName;

                        StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
                        for (int l = 0; l < m_TestSettings.ObjectsToCreate.NumberOfEventInfos; l++)
                        {
                            if (m_AbortRequested)
                                throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                            StackHashEventInfo eventInfo = new StackHashEventInfo();
                            eventInfo.DateCreatedLocal = DateTime.Now.ToUniversalTime();
                            eventInfo.DateModifiedLocal = DateTime.Now.ToUniversalTime();
                            eventInfo.HitDateLocal = eventInfoDate;
                            eventInfo.Language = "lang";
                            eventInfo.Lcid = 234;
                            eventInfo.Locale = "locale";
                            eventInfo.OperatingSystemName = "XP";
                            eventInfo.OperatingSystemVersion = "SP2";
                            eventInfo.TotalHits = l;
                            eventInfoCollection.Add(eventInfo);

                            eventInfoDate.AddDays(1);
                        }

                        errorIndex.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);


                        for (int m = 0; m < m_TestSettings.ObjectsToCreate.NumberOfCabs; m++)
                        {
                            if (m_AbortRequested)
                                throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                            StackHashCab cab = new StackHashCab();
                            cab.DateCreatedLocal = DateTime.Now.ToUniversalTime();
                            cab.DateModifiedLocal = DateTime.Now.ToUniversalTime();
                            cab.EventId = theEvent.Id;
                            cab.EventTypeName = theEvent.EventTypeName;
                            cab.FileName = "cab" + cabId.ToString(CultureInfo.InvariantCulture) + ".cab";
                            cab.Id = cabId++;
                            cab.SizeInBytes = 100;

                            errorIndex.AddCab(product, file, theEvent, cab, false);
                            m_SyncProgress.CabId = cab.Id;

                            // Copy in a test cab file.

                            String cabFolder = errorIndex.GetCabFolder(product, file, theEvent, cab);
                            if (!Directory.Exists(cabFolder))
                                Directory.CreateDirectory(cabFolder);
                            String cabFileName = errorIndex.GetCabFileName(product, file, theEvent, cab);

                            if (!File.Exists(cabFileName))
                            {
                                if (m_TestSettings.ObjectsToCreate.UseUnmanagedCab)
                                    File.Copy(TestSettings.TestDataFolder + @"Cabs\1629290733-SpecialException-1073810027.cab", cabFileName);
                                else if (m_TestSettings.ObjectsToCreate.UseLargeCab)
                                    File.Copy(TestSettings.TestDataFolder + @"Cabs\1630796338-Crash32bit-0760025228.cab", cabFileName);
                                else
                                    File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

                            }
                            // Make sure the file is not read only.
                            FileAttributes attributes = File.GetAttributes(cabFileName);
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(cabFileName, attributes);
                        }
                    }
                    onProgress(WinQualProgressType.ProductCabsUpdated, product, null, null, null, 0, 0, 
                        productSyncStartTime, DateTime.Now, errorIndex.GetLastSyncTimeLocal(product.Id));
                    onProgress(WinQualProgressType.ProductEventsUpdated, product, null, null, null, 0, 0,
                        productSyncStartTime, DateTime.Now, errorIndex.GetLastSyncTimeLocal(product.Id));

                }
                errorIndex.UpdateProductStatistics(product);
                errorIndex.SetLastSyncTimeLocal(product.Id, productSyncStartTime);
                errorIndex.SetLastSyncCompletedTimeLocal(product.Id, DateTime.Now);

            }
            onProgress(WinQualProgressType.Complete);
        }

        /// <summary>
        /// Downloads a specific cab and stores in the specified index.
        /// </summary>
        /// <param name="errorIndex">Error index to store the cab.</param>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to which the cab belongs</param>
        /// <param name="theEvent">Event to which the cab belongs.</param>
        /// <param name="cab">The cab to download.</param>
        /// <returns>Cab file name.</returns>
        public String GetCab(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (errorIndex == null)
                throw new ArgumentNullException("errorIndex");
            if (cab == null)
                throw new ArgumentNullException("cab");
            cab.CabDownloaded = true;
            errorIndex.AddCab(product, file, theEvent, cab, false);
            String cabFile = errorIndex.GetCabFileName(product, file, theEvent, cab);
            String cabFolder = errorIndex.GetCabFolder(product, file, theEvent, cab);

            if (!Directory.Exists(cabFolder))
                Directory.CreateDirectory(cabFolder);

            FileStream fileStream = File.Create(cabFile);

            fileStream.Close();

            return cabFile;
        }


        /// <summary>
        /// Aborts any sync that is currently in progress.
        /// This will get reset when the next sync occurs.
        /// </summary>
        public void AbortCurrentOperation()
        {
            m_AbortRequested = true;
        }


        /// <summary>
        /// Uploads a mapping file.
        /// </summary>
        /// <param name="fileName">File to be uploaded.</param>
        public void UploadFile(String fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
        }


        /// <summary>
        /// Sets the web request settings.
        /// </summary>
        /// <param name="requestRetryCount">Number of times to retry following a timeout failure.</param>
        /// <param name="requestTimeout">Time to wait for a single response in milliseconds.</param>
        public void SetWebRequestSettings(int requestRetryCount, int requestTimeout)
        {
        }

        #endregion
    }
}
