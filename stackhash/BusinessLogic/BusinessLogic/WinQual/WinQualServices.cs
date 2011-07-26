using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashUtilities;

namespace StackHashWinQual
{
    /// <summary>
    /// Interfaces through the WER API to the WinQual online services.
    /// </summary>
    public class WinQualServices : IWinQualServices
    {
        private Login m_Login;
        private bool m_LoginOk;
        private bool m_AbortRequested;
        private StackHashProductSyncDataCollection m_ProductsToSynchronize;
        private static int s_TotalCabDownloadRetries = 5;
        private StackHashCollectionPolicyCollection m_CollectionPolicyCollection;
        private int m_MaxConsecutiveCabDownloadFailures;
        private IErrorIndex m_ErrorIndex;
        private StackHashSyncProgress m_SyncProgress;
        private bool m_EnableNewProductsAutomatically;


        #region IWinQualServices Members

        /// <summary>
        /// Hook up to receive progress reports from the sync process.
        /// </summary>
        public event EventHandler<WinQualProgressEventArgs> Progress;


        private void onProgress(WinQualProgressType progressType, StackHashProduct product)
        {
            EventHandler<WinQualProgressEventArgs> handler = Progress;

            StackHashProductInfo productInfo = null;
            if (product != null)
            {
                productInfo = new StackHashProductInfo(product, !isExcludedProduct(product.Id), 
                    m_ErrorIndex.GetLastSyncTimeLocal(product.Id).ToUniversalTime(), new StackHashProductSyncData(), new DateTime(0, DateTimeKind.Utc),
                    new DateTime(0, DateTimeKind.Utc));
            }

            handler(this, new WinQualProgressEventArgs(progressType, productInfo));
        }


        /// <summary>
        /// Represents a list of product IDs that are to be synchronized during a SynchronizeWithWinQualOnline call.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get { return m_ProductsToSynchronize; }
            set { m_ProductsToSynchronize = value; }
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
        /// The max number of cab downloads allowed before the task is aborted.
        /// </summary>
        public int MaxConsecutiveCabDownloadFailures
        {
            get { return m_MaxConsecutiveCabDownloadFailures; }
            set { m_MaxConsecutiveCabDownloadFailures = value; }
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

        
        private bool isExcludedProduct(int productId)
        {
            if (m_ProductsToSynchronize == null)
                return true;
            else
                return (m_ProductsToSynchronize.FindProduct(productId) == null);
        }


        /// <summary>
        /// Logs on to the WinQual service.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool LogOn(string userName, string password)
        {
            m_Login = new Login(userName, password);
            if (m_Login.Validate())
            {
                m_LoginOk = true;
            }

            return m_LoginOk;
        }

        /// <summary>
        /// Logs on to the WinQual service.
        /// </summary>
        public void LogOff()
        {
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
        /// Property identifying a list of WinQual Products registered by the developer.
        /// </summary>
        public StackHashProductCollection StackHashProducts
        {
            get
            {
                ProductCollection products = Product.GetProducts(ref m_Login);
                StackHashProductCollection winQualProducts = ObjectConversion.ConvertProductCollection(products);
                return winQualProducts;
            }
        }

        /// <summary>
        /// Aborts the currently running operation if there is one.
        /// </summary>
        public void AbortCurrentOperation()
        {
            m_AbortRequested = true;
        }


        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        public void SetProxySettings(StackHashProxySettings proxySettings)
        {
        }


        /// <summary>
        /// Sets the web request settings.
        /// </summary>
        /// <param name="requestRetryCount">Number of times to retry following a timeout failure.</param>
        /// <param name="requestTimeout">Time to wait for a single response in milliseconds.</param>
        public void SetWebRequestSettings(int requestRetryCount, int requestTimeout)
        {
        }

        /// <summary>
        /// Sets the license restrictions for the index.
        /// </summary>
        /// <param name="maxEvents">The maximum number of events permitted to be downloaded.</param>
        public void SetLicenseRestrictions(long maxEvents)
        {
        }

        /// <summary>
        /// Syncs up with the products on winqual.
        /// </summary>
        /// <param name="forceResynchronize">True - force sync of everything, false - sync from last sync time</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="products">Products to sync.</param>
        /// <param name="getEvents">True - get events associated with the products, false - just get the products.</param>
        /// <param name="getCabs">True - get the cabs associated with the events, false - just get the events.</param>
        [SuppressMessage("Microsoft.Design", "CA1002")]
        private void UpdateProducts(bool forceResynchronize, IErrorIndex errorIndex, ProductCollection products, bool getEvents, bool getCabs)
        {
            // Product -1 is the special product ID that indicates when the last sync of products
            // was complete.
            DateTime lastProductPullDate = errorIndex.GetLastSyncTimeLocal(-1);

            if (forceResynchronize)
                lastProductPullDate = new DateTime(0, DateTimeKind.Local);

            // Add the products first.
            foreach (Product product in products)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                m_SyncProgress.ProductId = product.ID;

                StackHashProduct stackHashProduct = ObjectConversion.ConvertProduct(product);

                // Check the date created. If it is greater than the last pull date then
                // this is a new product so add a product record.
                if (product.DateCreatedLocal > lastProductPullDate)
                {
                    errorIndex.AddProduct(stackHashProduct);
                }
                else if (product.DateModifiedLocal > lastProductPullDate)
                {
                    // Update the product information if product last modified date is greater than 
                    // the last pull date.
                    errorIndex.AddProduct(stackHashProduct);
                }
                else
                {
                    // Check if the file exists. If not then add it.
                    if (!errorIndex.ProductExists(stackHashProduct))
                        errorIndex.AddProduct(stackHashProduct);
                }

                if (getEvents && !isExcludedProduct(stackHashProduct.Id))
                {
                    // Get the last date that this product was synchronized on WinQual.
                    DateTime lastPullDate = errorIndex.GetLastSyncTimeLocal(stackHashProduct.Id);

                    if (forceResynchronize)
                        lastPullDate = new DateTime(0, DateTimeKind.Local);

                    UpdateFiles(lastPullDate, errorIndex, product, stackHashProduct, getCabs);

                    if (getCabs)
                        onProgress(WinQualProgressType.ProductCabsUpdated, stackHashProduct);
                    else
                        onProgress(WinQualProgressType.ProductEventsUpdated, stackHashProduct);

                    if (getCabs)
                    {
                        // Completed getting all events and cabs for this product.
                        errorIndex.SetLastSyncTimeLocal(stackHashProduct.Id, DateTime.Now);
                    }
                }
            }

            errorIndex.SetLastSyncTimeLocal(-1, DateTime.Now);
        }


        /// <summary>
        /// Syncs up with the files/events on winqual.
        /// </summary>
        /// <param name="lastPullDate">Last time the data was synced.</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="product">Product to sync.</param>
        /// <param name="stackHashProduct">StackHash version of the product data.</param>
        /// <param name="getCabs">True - get the cabs associated with the events, false - just get the events.</param>
        private void UpdateFiles(DateTime lastPullDate, IErrorIndex errorIndex, Product product, StackHashProduct stackHashProduct, bool getCabs)
        {
            // Get the files associated with the product.
            ApplicationFileCollection files = product.GetApplicationFiles(ref m_Login);

            foreach (ApplicationFile file in files)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                StackHashFile stackHashFile = ObjectConversion.ConvertFile(file);

                m_SyncProgress.FileId = stackHashFile.Id;

                // Check the date the file record was created. If the date created is 
                // greater than the last pull date then this is a new file and so add a file
                // record.
                if (file.DateCreatedLocal > lastPullDate)
                {
                    errorIndex.AddFile(stackHashProduct, stackHashFile);
                }
                else if (file.DateModifiedLocal > lastPullDate)
                {
                    // Update the product information if product last modified date is greater than 
                    // the last pull date.
                    errorIndex.AddFile(stackHashProduct, stackHashFile);
                }
                else
                {
                    // Check if the file exists. If not then add it.
                    if (!errorIndex.FileExists(stackHashProduct, stackHashFile))
                        errorIndex.AddFile(stackHashProduct, stackHashFile);
                }

                UpdateEvents(lastPullDate, errorIndex, stackHashProduct, getCabs, file, stackHashFile);
            }
        }

        
        /// <summary>
        /// Syncs up with the events on winqual.
        /// </summary>
        /// <param name="lastPullDate">Last time the data was synced.</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="stackHashProduct">StackHash version of the product data.</param>
        /// <param name="getCabs">True - downloads cabs. False - only downloads events.</param>
        /// <param name="file">Winqua file.</param>
        /// <param name="stackHashFile">StackHash version of the file data.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        private void UpdateEvents(DateTime lastPullDate, IErrorIndex errorIndex,
            StackHashProduct stackHashProduct, bool getCabs, ApplicationFile file, StackHashFile stackHashFile)
        {
            // Get the events for the file with the start date as last pull date + 1.
            // Only stores the last 90 days worth - this will exception if you specify a date
            // before that time. In the case of the last pulldown date being close to 90 days ago
            // just get ALL the events.
            DateTime startTime = lastPullDate; // This is a local time.
            EventPageReader eventPageReader;
            TimeSpan timeSinceLastSync = (DateTime.UtcNow - lastPullDate.ToUniversalTime());

            if (timeSinceLastSync.Days >= 89)
            {
                StackHashUtilities.DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                    String.Format(CultureInfo.InvariantCulture, "Updating Events for {0} {1} ALL",
                                  stackHashProduct.Name, stackHashFile.Name));

                eventPageReader = file.GetEvents(); // Get all events.
            }
            else
            {
                StackHashUtilities.DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                    String.Format(CultureInfo.InvariantCulture, "Updating Events for {0} {1} since {2} {3}",
                                  stackHashProduct.Name, stackHashFile.Name, startTime, startTime.Kind));
                eventPageReader = file.GetEvents(startTime);
            }

            // Read each page of new events.
            while (eventPageReader.Read(ref m_Login) == true)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                // Get the events for the page.
                EventReader events = eventPageReader.Events;

                while (events.Read() == true)
                {
                    if (m_AbortRequested)
                        throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                    // Get the event
                    Event dpEvent = events.Event;

                    StackHashEvent stackHashEvent = ObjectConversion.ConvertEvent(dpEvent, stackHashFile.Id);

                    m_SyncProgress.EventId = stackHashEvent.Id;
                    m_SyncProgress.EventTypeName = stackHashEvent.EventTypeName;

                    // Check the date created. If it is greater than the last
                    // pull date then this is a new event and hence insert.
                    if (dpEvent.DateCreatedLocal > lastPullDate)
                    {
                        errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);
                    }
                    else if (dpEvent.DateModifiedLocal > lastPullDate)
                    {
                        // update the event information if event modified
                        // date is greater than the last pull date
                        errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);
                    }
                    else
                    {
                        // Check if the event exists. If not then add it.
                        if (!errorIndex.EventExists(stackHashProduct, stackHashFile, stackHashEvent))
                            errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);
                    }

                    // Get the details for the event.
                    EventInfoCollection infoCollection = dpEvent.GetEventDetails(ref m_Login);

                    // Loop through the event info.
                    StackHashEventInfo stackHashEventInfo = null;
                    StackHashEventInfoCollection stackHashEventInfoCollection = new StackHashEventInfoCollection();
                    foreach (EventInfo info in infoCollection)
                    {
                        if (m_AbortRequested)
                            throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                        stackHashEventInfo = ObjectConversion.ConvertEventInfo(info);
                        stackHashEventInfoCollection.Add(stackHashEventInfo);
                    }

                    errorIndex.MergeEventInfoCollection(stackHashProduct, stackHashFile, stackHashEvent, stackHashEventInfoCollection);

                    // Now get the total hits.
                    StackHashEventInfoCollection newEventInfos = errorIndex.LoadEventInfoList(stackHashProduct, stackHashFile, stackHashEvent);

                    int hits = 0;
                    foreach (StackHashEventInfo theEvent in newEventInfos)
                    {
                        hits += theEvent.TotalHits;
                    }

                    // Update the hits count.
                    stackHashEvent.TotalHits = hits;
                    errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);

                    if (getCabs)
                    {
                        UpdateCabs(lastPullDate, errorIndex, stackHashProduct, dpEvent, stackHashFile, stackHashEvent);
                    }
                }
            }
        }


        /// <summary>
        /// Syncs up with the cabs on winqual.
        /// </summary>
        /// <param name="lastPullDate">Last time the data was synced.</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="stackHashProduct">StackHash version of the product data.</param>
        /// <param name="dpEvent">WinQual event.</param>
        /// <param name="stackHashFile">StackHash version of the file data.</param>
        /// <param name="stackHashEvent">StackHash version of the event data.</param>

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        private void UpdateCabs(DateTime lastPullDate, IErrorIndex errorIndex,
            StackHashProduct stackHashProduct, Event dpEvent, StackHashFile stackHashFile, StackHashEvent stackHashEvent)
        {

            // Get the cabs for the event.
            CabCollection cabs = dpEvent.GetCabs(ref m_Login);

            // Work out how many cabs should be downloaded.
//            int cabsDownloaded = errorIndex.GetCabCount(stackHashProduct, stackHashFile, stackHashEvent);
//            int maxCabsToDownload = m_ProductsToSynchronize.GetMaxCabs(stackHashProduct.Id);

            // Loop through the cab collection
            foreach (Cab cab in cabs)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                // Disabled till beta 5.
//                if (cabsDownloaded >= maxCabsToDownload)
//                    break;
                   
                StackHashCab stackHashCab = ObjectConversion.ConvertCab(cab);
                
                m_SyncProgress.EventId = stackHashEvent.Id;

                String cabFileName = errorIndex.GetCabFileName(stackHashProduct, stackHashFile, stackHashEvent, stackHashCab);

                // Add the cab if not already in the database.
                if (!errorIndex.CabExists(stackHashProduct, stackHashFile, stackHashEvent, stackHashCab) ||
                    !File.Exists(cabFileName))
                {
                    if (cab.DateModifiedLocal <= lastPullDate)
                    {
                        // This shouldn't happen so log it.
                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Cab date older than last pull date but didn't exist in database - adding anyway: " + cab.ID.ToString(CultureInfo.InvariantCulture));
                    }

                    // Save the cab to a folder.
                    string cabFolder = errorIndex.GetCabFolder(stackHashProduct, stackHashFile,
                        stackHashEvent, stackHashCab);

                    if (!Directory.Exists(cabFolder))
                        Directory.CreateDirectory(cabFolder);

                    
                    int retryCount = 0;

                    do
                    {
                        cab.SaveCab(cabFolder, false, ref m_Login);

                        retryCount++;

                        if (File.Exists(cabFileName))
                        {
                            FileInfo fileInfo = new FileInfo(cabFileName);

                            if (fileInfo.Length != cab.SizeInBytes)
                            {
                                // File was only partially downloaded so delete it and retry.
                                File.Delete(cabFileName);
                            }
                            else
                            {
                                // All appears ok so add the cab to the database.
                                errorIndex.AddCab(stackHashProduct, stackHashFile, stackHashEvent, stackHashCab, false);

                                // Done.
                                break;
                            }
                        }
                    } while (retryCount < s_TotalCabDownloadRetries);
                }
            }
        }





        /// <summary>
        /// Synchronizes the online WinQual database with the local copy. 
        /// The local copy is effectively a cache of the online copy.
        /// </summary>
        /// <param name="errorIndex">Error index to add to.</param>
        /// <param name="forceResynchronize">True - sync everything, false - just sync from last sync time.</param>
        /// <param name="lastProgress">Last point at which the sync stopped.</param>
        /// <returns></returns>
        public void SynchronizeWithWinQualOnline(IErrorIndex errorIndex, bool forceResynchronize, StackHashSyncProgress lastProgress)
        {
            if (errorIndex == null)
                throw new ArgumentNullException("errorIndex");
            m_ErrorIndex = errorIndex;

            m_SyncProgress = new StackHashSyncProgress();

            m_AbortRequested = false;

            ProductCollection products = Product.GetProducts(ref m_Login);

            UpdateProducts(forceResynchronize, errorIndex, products, false, false); // Don't get events or cabs first time around.
            onProgress(WinQualProgressType.ProductListUpdated, null);

            UpdateProducts(forceResynchronize, errorIndex, products, true, false); // Get the events but not the cabs.
            UpdateProducts(forceResynchronize, errorIndex, products, true, true); // Get everything.
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
            throw new NotImplementedException("Get Cab");
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


        #endregion
    }
}
