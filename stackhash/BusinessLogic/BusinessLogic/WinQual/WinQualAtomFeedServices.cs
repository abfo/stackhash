using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Configuration;

using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashUtilities;
using WinQualAtomFeed;



namespace StackHashWinQual
{
    /// <summary>
    /// Provides access to the WinQual online service by accessing the Atom feed directly.
    /// 
    /// </summary>
    public class WinQualAtomFeedServices : IWinQualServices
    {
        private bool m_Busy;
        private  IAtomFeed m_AtomFeed;
        private bool m_AbortRequested;
        private StackHashProductSyncDataCollection m_ProductsToSynchronize;
        private StackHashCollectionPolicyCollection m_CollectionPolicyCollection;
        private int m_PullDateMinimumDuration;
        private int m_ConsecutiveCabDownloadFailures;
        private int m_MaxConsecutiveCabDownloadFailures;
        private int m_NumCabDownloads;
        private long m_MaxEvents;
        private DateTime m_SyncStartTime;
        private bool m_FullSync;
        private static DateTime s_TimeZeroUct = new DateTime(0).ToUniversalTime();
        private StackHashSyncProgress m_SyncProgress;
        private StackHashSyncProgress m_LastSyncProgress; // This is the point at which this sync should start.
        private bool m_LastSyncPositionReached;
        private bool m_EnableNewProductsAutomatically;


        #region Constructors

        /// <summary>
        /// Creates the AtomFeed object used for communication with the 
        /// </summary>
        /// <param name="proxySettings">Proxy settings.</param>
        /// <param name="requestRetryCount">Number of times to retry a request before bombing out.</param>
        /// <param name="requestTimeout">Length of time in milliseconds to wait for response.</param>
        /// <param name="pullDateMinimumDuration">Minimum time to ask for events etc...</param>
        /// <param name="maxConsecutiveCabDownloadFailures">The maximum number of consecutive cab download failures before giving up.</param>
        /// <param name="useLiveId">True - uses Windows Live Id, False - uses Ticket.</param>
        /// <param name="intervalBetweenWinQualLogonsInHours">Interval between each log-on to WinQual.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public WinQualAtomFeedServices(StackHashProxySettings proxySettings, int requestRetryCount, int requestTimeout, int pullDateMinimumDuration, 
            int maxConsecutiveCabDownloadFailures, bool useLiveId, int intervalBetweenWinQualLogonsInHours)
        {
            String testFileName = TestSettings.GetAttribute("DummyAtomFeedTestFile");


            if ((testFileName != null) && File.Exists(testFileName))
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Using dummy database: " + testFileName);
                m_AtomFeed = new DummyAtomFeed(proxySettings, requestRetryCount, requestTimeout, false, useLiveId, null);
            }
            else
            {
                m_AtomFeed = new AtomFeed(proxySettings, requestRetryCount, requestTimeout, false, useLiveId, null, intervalBetweenWinQualLogonsInHours);
            }
            m_Busy = false;
            m_PullDateMinimumDuration = pullDateMinimumDuration;
            m_MaxConsecutiveCabDownloadFailures = maxConsecutiveCabDownloadFailures;
        }

        #endregion


        #region IWinQualServices Members

        /// <summary>
        /// Hook up to this event to receive progress about the sync.
        /// </summary>
        public event EventHandler<WinQualProgressEventArgs> Progress;

        /// <summary>
        /// The point at which the sync operation reached before it stopped.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get 
            {
                if (m_SyncProgress == null)
                    return new StackHashSyncProgress();

                // Fog 1058.
                if ((m_LastSyncProgress != null) && !m_LastSyncPositionReached)
                {
                    // Didn't reach the last sync position so just return the same position for the next retry.
                    return m_LastSyncProgress;
                }

                // If the sync never got to an event then just return defaults (0) so that the next sync doesn't try
                // to skip anything.
                if ((m_SyncProgress.ProductId == 0) || (m_SyncProgress.FileId == 0) || (m_SyncProgress.EventId == 0))
                    return new StackHashSyncProgress();
                else
                    return m_SyncProgress; 
            }
        }

        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        public void SetProxySettings(StackHashProxySettings proxySettings)
        {
            if (m_AtomFeed != null)
                m_AtomFeed.SetProxySettings(proxySettings);
        }


        /// <summary>
        /// Sets the web request settings.
        /// </summary>
        /// <param name="requestRetryCount">Number of times to retry following a timeout failure.</param>
        /// <param name="requestTimeout">Time to wait for a single response in milliseconds.</param>
        public void SetWebRequestSettings(int requestRetryCount, int requestTimeout)
        {
            if (m_AtomFeed != null)
                m_AtomFeed.SetWebRequestSettings(requestRetryCount, requestTimeout);
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
        /// Reports progress for the sync.
        /// </summary>
        /// <param name="progressType">The type of progress being reported.</param>
        /// <param name="product">The product being reported on.</param>
        private void onProgress(WinQualProgressType progressType, StackHashProduct product)
        {
            onProgress(progressType, product, null, null, null, 0, 0, s_TimeZeroUct, s_TimeZeroUct, s_TimeZeroUct);
        }

        /// <summary>
        /// Reports event page progress for the sync.
        /// </summary>
        /// <param name="progressType">The type of progress being reported.</param>
        /// <param name="product">The product being reported on.</param>
        /// <param name="file">The file being reported on.</param>
        /// <param name="theEvent">The event being reported on.</param>
        /// <param name="cab">The cab being reported on.</param>
        private void onProgress(WinQualProgressType progressType, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            onProgress(progressType, product, file, theEvent, cab, 0, 0, s_TimeZeroUct, s_TimeZeroUct, s_TimeZeroUct);
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
                        lastSuccessfulStarted, new StackHashProductSyncData(), lastProductSyncCompleted,
                        lastProductSyncStarted);
                }

                handler(this, new WinQualProgressEventArgs(progressType, productInfo, file, theEvent, cab, currentPage, totalPages));
            }
        }

        
        /// <summary>
        /// Set this property to control whether a sync is performed on a particular product
        /// and how many cabs to download for the product.
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
        /// The max number of cab downloads allowed before the task is aborted.
        /// </summary>
        public int MaxConsecutiveCabDownloadFailures
        {
            get { return m_MaxConsecutiveCabDownloadFailures; }
            set { m_MaxConsecutiveCabDownloadFailures = value; }
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
        /// Checks to see if the product is excluded from sync. A product is excluded if it
        /// doesn't appear in the ProductsToSynchronize list.
        /// </summary>
        /// <param name="productId">ID of the product to sync.</param>
        /// <returns>True - if should NOT be synced, False - otherwise.</returns>
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
            if (m_Busy)
                return true;  // As the logon must have worked previously.

            if (!m_AtomFeed.Login(userName, password))
                throw new StackHashException("Unable to log on to the Win Qual service. Please check that your username and password are correct.", StackHashServiceErrorCode.WinQualLogOnFailed);

            return true;
        }


        /// <summary>
        /// Logs on to the WinQual service.
        /// </summary>
        public void LogOff()
        {
            if (m_Busy)
                return;

            m_AtomFeed.LogOut();
        }


        
        /// <summary>
        /// Aborts the currently running operation if there is one.
        /// </summary>
        public void AbortCurrentOperation()
        {
            if (m_Busy)
                m_AbortRequested = true;
            if (m_AtomFeed != null)
                m_AtomFeed.AbortCurrentOperation();
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
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        private void UpdateProducts(bool forceResynchronize, IErrorIndex errorIndex, AtomProductCollection products, bool getEvents, bool getCabs)
        {
            // Product -1 is the special product ID that indicates when the last sync of products
            // was complete.
            DateTime lastProductPullDate = errorIndex.GetLastHitTimeLocal(-1);
            if (lastProductPullDate != new DateTime(0, DateTimeKind.Local))
                lastProductPullDate = lastProductPullDate.AddDays(m_PullDateMinimumDuration * -1);

            if (forceResynchronize)
                lastProductPullDate = new DateTime(0, DateTimeKind.Local);

            // Add the products first.
            foreach (AtomProduct product in products)
            {
                m_SyncProgress.ProductId = product.Product.Id;

                // Skip this product - keep going instead until we reach the last product we stopped at during the last sync.
                if ((m_LastSyncProgress != null) && (!m_LastSyncPositionReached) && (m_LastSyncProgress.ProductId != m_SyncProgress.ProductId))
                    continue;

                DateTime productSyncStartTime = DateTime.Now;

                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                StackHashProduct stackHashProduct = AtomObjectConversion.ConvertProduct(product);

                // Check the date created. If it is greater than the last pull date then
                // this is a new product so add a product record.
                if (!errorIndex.ProductExists(stackHashProduct))
                {
                    errorIndex.AddProduct(stackHashProduct);

                    if (m_EnableNewProductsAutomatically)
                        if (m_ProductsToSynchronize.FindProduct(stackHashProduct.Id) == null)
                            m_ProductsToSynchronize.Add(new StackHashProductSyncData(stackHashProduct.Id));
                }
                else if (stackHashProduct.DateCreatedLocal > lastProductPullDate)
                {
                    errorIndex.AddProduct(stackHashProduct);
                }
                else if (stackHashProduct.DateModifiedLocal > lastProductPullDate)
                {
                    errorIndex.AddProduct(stackHashProduct);
                }

                // May only be getting the product list.
                // Also, the product might not be enabled for downloading events.
                if (getEvents && !isExcludedProduct(stackHashProduct.Id))
                {
                    DateTime currentProductCompleteTime = errorIndex.GetLastSyncCompletedTimeLocal(stackHashProduct.Id);

                    // This method is called once to get the events then the next time to get the cabs.
                    if (getCabs)
                    {
                        onProgress(WinQualProgressType.DownloadingProductCabs, stackHashProduct, null, null, null, 0, 0,
                            productSyncStartTime.ToUniversalTime(), currentProductCompleteTime.ToUniversalTime(), errorIndex.GetLastSyncTimeLocal(stackHashProduct.Id).ToUniversalTime());
                    }
                    else
                    {
                        // Record the start time of the sync and set the end time to 0 (no end time).
                        errorIndex.SetLastSyncStartedTimeLocal(stackHashProduct.Id, productSyncStartTime);
                        onProgress(WinQualProgressType.DownloadingProductEvents, stackHashProduct, null, null, null, 0, 0,
                            productSyncStartTime.ToUniversalTime(), currentProductCompleteTime.ToUniversalTime(), errorIndex.GetLastSyncTimeLocal(stackHashProduct.Id).ToUniversalTime());
                    }

                    // Get the last hit date and subtract a few days to ensure we don't miss any events that were 
                    // added after the last sync but with an earlier hit date.
                    DateTime lastMostRecentHitDate = errorIndex.GetLastHitTimeLocal(stackHashProduct.Id);
                    DateTime thisPullDate;
                    if (lastMostRecentHitDate != new DateTime(0, DateTimeKind.Local))
                        thisPullDate = lastMostRecentHitDate.AddDays(m_PullDateMinimumDuration * -1);
                    else
                        thisPullDate = lastMostRecentHitDate;

                    if (forceResynchronize)
                        thisPullDate = new DateTime(0, DateTimeKind.Local);

                    String message = String.Format(CultureInfo.InvariantCulture, "Product: {0}({1}) - Pull Date: {2}", product.Product.Name, product.Product.Version, thisPullDate);
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, message);

                    DateTime earliestFinalHitDate;
                    try
                    {
                        earliestFinalHitDate = UpdateFiles(thisPullDate, errorIndex, product, stackHashProduct, getCabs, m_PullDateMinimumDuration);
                    }
                    finally
                    {
                        errorIndex.UpdateProductStatistics(stackHashProduct);
                    }


                    DateTime productSyncCompleteTime = DateTime.Now;


                    if (getCabs)
                    {
                        // Only set the last sync date after downloading the cabs (phase 3 of the sync).

                        // Check if not new hits found. If not then just stick with the last hit date.
                        if (earliestFinalHitDate == new DateTime(0, DateTimeKind.Local))
                            earliestFinalHitDate = lastMostRecentHitDate;

                        // Completed getting all events and cabs for this product.
                        errorIndex.SetLastHitTimeLocal(stackHashProduct.Id, earliestFinalHitDate);

                        // Use the start date of the sync (of this product) as the sync might take days and events 
                        // might arrive that we miss while syncing.
                        errorIndex.SetLastSyncTimeLocal(stackHashProduct.Id, productSyncStartTime);

                        // Only set the sync completed after the last cab.
                        errorIndex.SetLastSyncCompletedTimeLocal(stackHashProduct.Id, productSyncCompleteTime);

                        onProgress(WinQualProgressType.ProductCabsUpdated, stackHashProduct, null, null, null, 0, 0,
                            productSyncStartTime.ToUniversalTime(), productSyncCompleteTime.ToUniversalTime(), errorIndex.GetLastSyncTimeLocal(stackHashProduct.Id).ToUniversalTime());
                    }
                    else
                    {
                        onProgress(WinQualProgressType.ProductEventsUpdated, stackHashProduct, null, null, null, 0, 0,
                            productSyncStartTime.ToUniversalTime(), currentProductCompleteTime.ToUniversalTime(), errorIndex.GetLastSyncTimeLocal(stackHashProduct.Id).ToUniversalTime());
                    }
                }

            }

            // Use the start date of the sync as the sync might take days and events might arrive that we miss while syncing.
            errorIndex.SetLastHitTimeLocal(-1, m_SyncStartTime);
            errorIndex.SetLastSyncTimeLocal(-1, m_SyncStartTime);
        }


        /// <summary>
        /// Syncs up with the files/events on winqual.
        /// </summary>
        /// <param name="lastPullDate">Last time the data was synced.</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="product">Product to sync.</param>
        /// <param name="stackHashProduct">StackHash version of the product data.</param>
        /// <param name="getCabs">True - get the cabs associated with the events, false - just get the events.</param>
        /// <param name="daysOverlap">Number of days overlap.</param>
        /// <returns>Earliest final hit date across events.</returns>
        private DateTime UpdateFiles(DateTime lastPullDate, IErrorIndex errorIndex, AtomProduct product, StackHashProduct stackHashProduct, 
            bool getCabs, int daysOverlap)
        {
            DateTime earliestFinalHitDate = new DateTime(0, DateTimeKind.Local);

            // Get the files associated with the product.
            AtomFileCollection files = m_AtomFeed.GetFiles(product);

            foreach (AtomFile file in files)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                StackHashFile stackHashFile = AtomObjectConversion.ConvertFile(file);

                m_SyncProgress.FileId = file.File.Id;

                // Skip this file - keep going instead until we reach the last file we stopped at during the last sync.
                if ((m_LastSyncProgress != null) && (!m_LastSyncPositionReached) && (m_LastSyncProgress.FileId != m_SyncProgress.FileId))
                    continue;


                // Check the date the file record was created. If the date created is 
                // greater than the last pull date then this is a new file and so add a file
                // record.
                if (stackHashFile.DateCreatedLocal > lastPullDate)
                {
                    errorIndex.AddFile(stackHashProduct, stackHashFile);
                }
                else if (stackHashFile.DateModifiedLocal > lastPullDate)
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

                DateTime fileEarliestFinalHitDate = UpdateEvents(lastPullDate, errorIndex, stackHashProduct, getCabs, file, stackHashFile, daysOverlap);

                if (fileEarliestFinalHitDate < earliestFinalHitDate)
                    earliestFinalHitDate = fileEarliestFinalHitDate;

                // Assume that if this file was processed then we must have also reached the point at which 
                // the sync last stopped at.
                m_LastSyncPositionReached = true;
            }

            return earliestFinalHitDate;
        }


        /// <summary>
        /// Gets the most recent hit date in the index for this event.
        /// </summary>
        /// <param name="index">The index containing the event.</param>
        /// <param name="stackHashProduct">Product owning the event.</param>
        /// <param name="stackHashFile">File owning the event.</param>
        /// <param name="stackHashEvent">The event.</param>
        /// <returns></returns>
        private static DateTime getMostRecentHitDateInIndex(IErrorIndex index, StackHashProduct stackHashProduct, StackHashFile stackHashFile, StackHashEvent stackHashEvent)
        {
            return index.GetMostRecentHitDate(stackHashProduct, stackHashFile, stackHashEvent);
        }


        /// <summary>
        /// Updates the event info associated with an event.
        /// Merges the data in with the event info already in the database.
        /// </summary>
        /// <param name="errorIndex">Index to update.</param>
        /// <param name="stackHashProduct">Product.</param>
        /// <param name="stackHashFile">File</param>
        /// <param name="stackHashEvent">Event whose info is required.</param>
        /// <param name="atomEvent">Atom version of the event whose info is required.</param>
        /// <param name="daysOverlap">Number of days earlier to ask for data.</param>
        /// <returns>Most recent hit date</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        private DateTime UpdateEventInfo(IErrorIndex errorIndex, StackHashProduct stackHashProduct, StackHashFile stackHashFile, 
            StackHashEvent stackHashEvent, AtomEvent atomEvent, int daysOverlap)
        {
            DateTime mostRecentHitDate = new DateTime(0, DateTimeKind.Local);

            int daysSinceLastPull = 0;
            if (m_FullSync)
            {
                daysSinceLastPull = 90;
            }
            else
            {
                DateTime mostRecentHitStored = getMostRecentHitDateInIndex(errorIndex, stackHashProduct, stackHashFile, stackHashEvent);

                daysSinceLastPull = (DateTime.Now - mostRecentHitStored).Days + daysOverlap; // Overlap just in case we missed a few event infos.
                if (daysSinceLastPull > 90)
                    daysSinceLastPull = 90;
            }

            StackHashUtilities.DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                   String.Format(CultureInfo.InvariantCulture, "Updating Event Info for {0} {1} Event Id: {2}",
                   stackHashProduct.Name, stackHashFile.Name, stackHashEvent.Id));

            AtomEventInfoCollection infoCollection = m_AtomFeed.GetEventDetails(atomEvent, daysSinceLastPull);

            // Loop through the event info.
            StackHashEventInfo stackHashEventInfo = null;
            StackHashEventInfoCollection stackHashEventInfoCollection = new StackHashEventInfoCollection();
            foreach (AtomEventInfo info in infoCollection)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                stackHashEventInfo = AtomObjectConversion.ConvertEventInfo(info);

                // BugzID:808 - Hit dates are incorrect.
                stackHashEventInfo = stackHashEventInfo.Normalize();

                stackHashEventInfoCollection.Add(stackHashEventInfo);

                if (stackHashEventInfo.HitDateLocal > mostRecentHitDate)
                    mostRecentHitDate = stackHashEventInfo.HitDateLocal;
            }

            // Add this new event info list to the existing data in the database.
            errorIndex.MergeEventInfoCollection(stackHashProduct, stackHashFile, stackHashEvent, stackHashEventInfoCollection);
                    
            // Update the hits count.
            stackHashEvent.TotalHits = errorIndex.GetHitCount(stackHashProduct, stackHashFile, stackHashEvent);

            // The event must have been added and associated with the file id, so it is safe to get the event
            // here without checking EventExists.
            StackHashEvent existingEvent = errorIndex.GetEvent(stackHashProduct, stackHashFile, stackHashEvent);

            if ((existingEvent.CompareTo(stackHashEvent, true) != 0) ||
                (existingEvent.TotalHits != stackHashEvent.TotalHits))               
            {
                errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);
            }
            return mostRecentHitDate;
        }


        /// <summary>
        /// Syncs up with the events on winqual.
        /// </summary>
        /// <param name="lastPullDate">Last time the data was synced.</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="stackHashProduct">StackHash version of the product data.</param>
        /// <param name="getCabs">True - retrieve cab files. False - just retrieve events.</param>
        /// <param name="file">Winqua file.</param>
        /// <param name="stackHashFile">StackHash version of the file data.</param>
        /// <param name="daysOverlap">Number of days overlap</param>
        /// <returns>Earliest final hit date.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        private DateTime UpdateEvents(DateTime lastPullDate, IErrorIndex errorIndex,
            StackHashProduct stackHashProduct, bool getCabs, AtomFile file, StackHashFile stackHashFile, int daysOverlap)
        {
            DateTime earliestFinalHitDate = new DateTime(0, DateTimeKind.Local);
            AtomEventPageReader eventPageReader;

            // Get the events for the file with the start date as last pull date + 1.
            // Only stores the last 90 days worth - this will exception if you specify a date
            // before that time. 
            DateTime startTime = lastPullDate; // This is a local time.
            TimeSpan timeSinceLastSync = (DateTime.UtcNow - lastPullDate.ToUniversalTime());

            if (timeSinceLastSync.Days >= 89)
                startTime = DateTime.Now.AddDays(-89);

            StackHashUtilities.DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                String.Format(CultureInfo.InvariantCulture, "Updating Events for {0} {1} since {2} {3}",
                                stackHashProduct.Name, stackHashFile.Name, startTime, startTime.Kind));

            eventPageReader = new AtomEventPageReader(m_AtomFeed, file, startTime, DateTime.Now);


            // Read each page of new events.
            AtomEventCollection currentEvents;
            while ((currentEvents = eventPageReader.ReadPage()) != null)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                // Only report event progress if not downloading cabs. If downloading cabs then the progress will reported as 
                // cab updates not event updates. Don't bother reporting if there are no events on this page.
                if (!getCabs && (currentEvents.Count != 0))
                    onProgress(WinQualProgressType.DownloadingEventPage, stackHashProduct, stackHashFile, null, null, eventPageReader.CurrentPage, eventPageReader.TotalPages, s_TimeZeroUct, s_TimeZeroUct, s_TimeZeroUct);

                foreach (AtomEvent atomEvent in currentEvents)
                {
                    if (m_AbortRequested)
                        throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                    StackHashEvent stackHashEvent = AtomObjectConversion.ConvertEvent(atomEvent);

                    m_SyncProgress.EventId = atomEvent.Event.Id;
                    m_SyncProgress.EventTypeName = atomEvent.Event.EventTypeName;

                    // Skip this event - keep going instead until we reach the last event we stopped at during the last sync.
                    if ((m_LastSyncProgress != null) && 
                        (!m_LastSyncPositionReached) &&
                        ((m_LastSyncProgress.EventId != m_SyncProgress.EventId) ||
                        (m_LastSyncProgress.EventTypeName != m_SyncProgress.EventTypeName)))
                        continue;

                    // Reached the point at which the last sync stopped..
                    if ((m_LastSyncProgress != null) &&
                        (m_LastSyncProgress.ProductId == m_SyncProgress.ProductId) &&
                        (m_LastSyncProgress.FileId == m_SyncProgress.FileId) &&
                        (m_LastSyncProgress.EventId == m_SyncProgress.EventId) &&
                        (m_LastSyncProgress.EventTypeName == m_SyncProgress.EventTypeName))
                    {
                        m_LastSyncPositionReached = true;
                    }



                    // The event only exists for this file if 1) the event exists in the event table AND 2) the 
                    // file events table has been updated for this event.
                    // There are 3 cases.
                    // 1) the event doesn't exist at all.
                    // 2) the event exists but not connected to this file.
                    // 3) the event exists and is already connected to this file.
                    // Need to call AddEvent in cases 1 and 2 which will update the event and fileevents tables.
                    // In case 3 the event needs to be updated only if it has changed. Note that the hits returned
                    // from winqual is always -1 so set this to whatever the stored value is if there is one.
                    bool eventExists = errorIndex.EventExists(stackHashProduct, stackHashFile, stackHashEvent);

                    // If the event limit has been exceeded then skip this event unless the event already exists
                    // in which case allow the event data to be updated. i.e. existing events will continue to be updated
                    // even after the event threshold has been reached.
                    if (!eventExists && (errorIndex.TotalStoredEvents >= m_MaxEvents))
                        continue;

                    // Get the existing event data. Note this could return non-null if the event has been previously 
                    // created but associated with a different file id.
                    StackHashEvent existingEvent = errorIndex.GetEvent(stackHashProduct, stackHashFile, stackHashEvent);

                    // The hits returned from winqual is always -1 so set this to whatever the stored value is if there is one.
                    if (existingEvent != null)
                        stackHashEvent.TotalHits = existingEvent.TotalHits;

                    if (!eventExists)
                    {
                        // New event for this file. May have already existed for another file.
                        // Plugins will see an EventAdded or EventUpdated event here.
                        errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);
                    }
                    else
                    {
                        // The event already existed for this file. Only update it if any of the winqual fields changed.
                        // This is so that plugins don't see an unnecesssary EventUpdated call.
                        // existingEvent should never be null here, but in case it is - add the event too.
                        if ((existingEvent == null) || (existingEvent.CompareTo(stackHashEvent, true) != 0))
                            errorIndex.AddEvent(stackHashProduct, stackHashFile, stackHashEvent);
                    }


                    // Update the event info associated with an event.
                    DateTime thisMostRecentHitDate = UpdateEventInfo(errorIndex, stackHashProduct, stackHashFile, stackHashEvent, atomEvent, daysOverlap);

                    if (thisMostRecentHitDate < earliestFinalHitDate)
                        earliestFinalHitDate = thisMostRecentHitDate;

                    if (getCabs)
                    {
                        UpdateCabs(lastPullDate, errorIndex, stackHashProduct, atomEvent, stackHashFile, stackHashEvent);
                    }
                }
            }

            return earliestFinalHitDate;
        }

        /// <summary>
        /// Gets the total number of cabs from the index and the new cabs that are available as shown in newCabs.
        /// The 2 lists are effectively combined to eliminate overlap and the new count returned.
        /// </summary>
        /// <param name="errorIndex">The error index containing cabs.</param>
        /// <param name="product">The product whos cabs are to be tested.</param>
        /// <param name="file">The file whos cabs are to be tested.</param>
        /// <param name="theEvent">The event whos cabs are to be tested.</param>
        /// <param name="newCabs">The second list of new cabs returned by WinQual site.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1062")]
        public static int GetTotalCabsIncludingNewOnes(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, 
            StackHashEvent theEvent, AtomCabCollection newCabs)
        {
            int alreadyPresent = 0;
            foreach (AtomCab cab in newCabs)
            {
                if (errorIndex.CabExists(product, file, theEvent, cab.Cab))
                {
                    alreadyPresent++;
                }
            }

            int cabInfoCount = errorIndex.GetCabCount(product, file, theEvent);
            int totalCabInfos = cabInfoCount + (newCabs.Count - alreadyPresent);

            return (totalCabInfos);
        }


        /// <summary>
        /// Determines if the specified cab should be collected based on the cab collection criteria.
        /// </summary>
        /// <param name="errorIndex">The index where the cab will be placed.</param>
        /// <param name="product">The product owning the cab.</param>
        /// <param name="file">The file owning the cab.</param>
        /// <param name="theEvent">The event owning the cab.</param>
        /// <param name="cabs">List of new cabs.</param>
        /// <param name="cab">The cab to check.</param>
        /// <param name="cabsToDownload">Total number of cabs that are permitted to be downloaded.</param>
        /// <param name="collectionPolicy">The user cab collection policy.</param>
        /// <returns></returns>
        private static bool shouldCollectCab(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, 
            AtomCabCollection cabs, StackHashCab cab, int cabsToDownload, StackHashCollectionPolicy collectionPolicy)
        {
            if (cabsToDownload == 0)
                return false;

            int olderCount = 0; // Number of cabs older than this one.
            int newerCount = 0; // Number of cabs younger than this one.
            int largerCount = 0; // Number of cabs bigger than this one.
            int smallerCount = 0;

            if (collectionPolicy.CollectionOrder == StackHashCollectionOrder.AsReceived)
                return true;

            foreach (AtomCab newCab in cabs)
            {
                // If there is a cab entry and the file exists then ignore it.
                if (errorIndex.CabExists(product, file, theEvent, newCab.Cab) && errorIndex.CabFileExists(product, file, theEvent, newCab.Cab))
                    continue;

                switch (collectionPolicy.CollectionOrder)
                {
                    case StackHashCollectionOrder.LargestFirst:
                        if (newCab.Cab.SizeInBytes > cab.SizeInBytes)
                        {
                            largerCount++;
                            if (largerCount >= cabsToDownload)
                                return false;
                        }                       
                        break;

                    case StackHashCollectionOrder.SmallestFirst:
                        if (newCab.Cab.SizeInBytes < cab.SizeInBytes)
                        {
                            smallerCount++;
                            if (smallerCount >= cabsToDownload)
                                return false;
                        }                       
                        break;

                    case StackHashCollectionOrder.MostRecentFirst:
                        if (newCab.Cab.DateCreatedLocal > cab.DateCreatedLocal)
                        {
                            newerCount++;
                            if (newerCount >= cabsToDownload)
                                return false;
                        }

                        break;

                    case StackHashCollectionOrder.OldestFirst:
                        if (newCab.Cab.DateCreatedLocal < cab.DateCreatedLocal)
                        {
                            olderCount++;
                            if (olderCount >= cabsToDownload)
                                return false;
                        }                       
                        break;

                    default:
                        throw new InvalidOperationException("Invalid stack hash collection order");
                }
            }

            return true;
        }


        /// <summary>
        /// Determines if the cab is larger than the cabs in the index and also the 
        /// largest of the cabs available for download.
        /// </summary>
        /// <param name="errorIndex">Error index.</param>
        /// <param name="product">Product.</param>
        /// <param name="file">File.</param>
        /// <param name="theEvent">Event.</param>
        /// <param name="cab">Cab to check.</param>
        /// <param name="cabs">Cabs available for download.</param>
        /// <returns>true - cab is the biggest, false - cab is not biggest </returns>
        private static bool cabIsBigger(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, 
            StackHashEvent theEvent, StackHashCab cab, AtomCabCollection cabs)
        {
            long maxCabSize = 0;
            StackHashCabCollection existingCabs = errorIndex.LoadCabList(product, file, theEvent);

            for (int i = 0; i < existingCabs.Count; i++)
            {
                if (existingCabs[i].SizeInBytes > maxCabSize)
                    maxCabSize = existingCabs[i].SizeInBytes;
            }

            if (cab.SizeInBytes > maxCabSize)
            {
                // Find the biggest cab available now for download.
                long maxAvailableCabSize = 0;
                foreach (AtomCab atomCab in cabs)
                {
                    // Ignore any that have already been downloaded.
                    if (errorIndex.CabExists(product, file, theEvent, atomCab.Cab) &&
                        errorIndex.CabFileExists(product, file, theEvent, atomCab.Cab))
                        continue;

                    if (atomCab.Cab.SizeInBytes > maxAvailableCabSize)
                        maxAvailableCabSize = atomCab.Cab.SizeInBytes;
                }

                if (cab.SizeInBytes == maxAvailableCabSize)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Determines if a cab should be collected or not based on the collection policy.
        /// This method assumes that all cabs presented
        /// </summary>
        /// <param name="errorIndex">Error index.</param>
        /// <param name="product">Product owning the cab.</param>
        /// <param name="file">File owning the cab.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">The cab to examine.</param>
        /// <param name="cabs">Cabs available for download.</param>
        /// <param name="totalCabsIncludingNewOnes">Total cabs including new ones.</param>
        /// <param name="collectionPolicyCollection">The cab collection policy.</param>
        /// <returns>True - collect the cab, false - don't collect.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062")]
        public static bool ShouldCollectCab(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, 
            StackHashCab cab, AtomCabCollection cabs, int totalCabsIncludingNewOnes, StackHashCollectionPolicyCollection collectionPolicyCollection)
        {
            // If the cab already exists then don't need to collect again.
            if (errorIndex.CabExists(product, file, theEvent, cab) && errorIndex.CabFileExists(product, file, theEvent, cab))
                return false;

            // This effectively means collect all cabs.
            if (collectionPolicyCollection == null)
                return true;

            // Get the event level policy for cab collection.
            StackHashCollectionPolicy eventCriteriaForCabCollection =
                collectionPolicyCollection.FindPrioritizedPolicy(product.Id, file.Id, theEvent.Id, 0,
                    StackHashCollectionObject.Cab, StackHashCollectionObject.Event);

            // Only collect cabs if there isn't an event level policy or the event criteria is met.
            bool policySaysDownloadCabs = (eventCriteriaForCabCollection == null) ||
                ((eventCriteriaForCabCollection.CollectionType == StackHashCollectionType.MinimumHitCount) &&
                 (theEvent.TotalHits >= eventCriteriaForCabCollection.Minimum) ||
                 (eventCriteriaForCabCollection.CollectionType == StackHashCollectionType.All));

            if (policySaysDownloadCabs == false)
                return false;


            // Check the cab collection policy at the cab level.
            StackHashCollectionPolicy collectionPolicy = collectionPolicyCollection.FindPrioritizedPolicy(product.Id, file.Id, theEvent.Id, cab.Id, StackHashCollectionObject.Cab, StackHashCollectionObject.Cab);

            // If no policy in the list assume collect all cabs.
            if (collectionPolicy == null)
                return true;

            switch (collectionPolicy.CollectionType)
            {
                case StackHashCollectionType.All:
                    return true;
                
                case StackHashCollectionType.None:
                    return false;

                case StackHashCollectionType.Count:
                    int cabsDownloaded = errorIndex.GetCabFileCount(product, file, theEvent);
                    int cabsToDownload = collectionPolicy.Maximum - cabsDownloaded;

                    if (cabsToDownload > 0)
                    {
                        return shouldCollectCab(errorIndex, product, file, theEvent, cabs, cab, cabsToDownload, collectionPolicy);
                    }
                    else
                    {
                        if (collectionPolicy.CollectLarger)
                            return cabIsBigger(errorIndex, product, file, theEvent, cab, cabs);
                        else
                            return false;
                    }

                case StackHashCollectionType.Percentage:
                    if (collectionPolicy.Percentage == 0)
                        throw new InvalidOperationException("Percentage collection cannot be 0");

                    int cabsAlreadyDownloaded = errorIndex.GetCabFileCount(product, file, theEvent);
                    int totalCabsRequired = collectionPolicy.Percentage * totalCabsIncludingNewOnes / 100;
                    int totalCabsRemainingToDownload = totalCabsRequired - cabsAlreadyDownloaded;

                    if (totalCabsRemainingToDownload > 0)
                    {
                        return shouldCollectCab(errorIndex, product, file, theEvent, cabs, cab, totalCabsRemainingToDownload, collectionPolicy);
                    }
                    else
                    {
                        if (collectionPolicy.CollectLarger)
                            return cabIsBigger(errorIndex, product, file, theEvent, cab, cabs);
                        else
                            return false;
                    }

                default:
                    throw new InvalidOperationException("Unknown collection type");
            }
        }


        /// <summary>
        /// Syncs up with the cabs on winqual.
        /// </summary>
        /// <param name="lastPullDate">Last time the data was synced.</param>
        /// <param name="errorIndex">Index to add to.</param>
        /// <param name="stackHashProduct">StackHash version of the product data.</param>
        /// <param name="atomEvent">WinQual event.</param>
        /// <param name="stackHashFile">StackHash version of the file data.</param>
        /// <param name="stackHashEvent">StackHash version of the event data.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        private void UpdateCabs(DateTime lastPullDate, IErrorIndex errorIndex,
            StackHashProduct stackHashProduct, AtomEvent atomEvent, StackHashFile stackHashFile, StackHashEvent stackHashEvent)
        {
            // Get the cabs for the event.
            AtomCabCollection cabs = m_AtomFeed.GetCabs(atomEvent);

            int totalCabInfos = GetTotalCabsIncludingNewOnes(errorIndex, stackHashProduct, stackHashFile, stackHashEvent, cabs);

            // Loop through the cab collection.
            foreach (AtomCab cab in cabs)
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during Win Qual synchronize");

                m_SyncProgress.CabId = cab.Cab.Id;

                // Check if the cab info already exists in the database.
                bool cabInfoAlreadyExists = errorIndex.CabExists(stackHashProduct, stackHashFile, stackHashEvent, cab.Cab);

                // Check if the cab file has already been downloaded.
                bool cabFileExists = false;
                if (cabInfoAlreadyExists && errorIndex.CabFileExists(stackHashProduct, stackHashFile, stackHashEvent, cab.Cab))
                    cabFileExists = true;

                StackHashCab stackHashCab = AtomObjectConversion.ConvertCab(cab);

                // Add the cab if not already in the database.
                if (!cabInfoAlreadyExists || !cabFileExists)
                {
                    // If this is a new cab info then check the date.
                    if ((stackHashCab.DateModifiedLocal <= lastPullDate) && (!cabInfoAlreadyExists))
                    {
                        // This shouldn't happen so log it.
                        // This may possibly happen if the cabs are added to the database after the last sync - but they are old cabs
                        // just becoming available.
                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Cab date older than last pull date but didn't exist in database - adding anyway: " + cab.Cab.Id.ToString(CultureInfo.InvariantCulture));
                    }

                    bool cabDownloaded = cabFileExists;

                    // Determine if the cab should be downloaded based on the data collection policy.
                    bool collectCabFile = ShouldCollectCab(errorIndex, stackHashProduct, stackHashFile, stackHashEvent, 
                        stackHashCab, cabs, totalCabInfos, m_CollectionPolicyCollection);

                    if (!cabFileExists && collectCabFile)
                    {
                        // Save the cab to a folder.
                        string cabFolder = errorIndex.GetCabFolder(stackHashProduct, stackHashFile, stackHashEvent, stackHashCab);

                        if (!Directory.Exists(cabFolder))
                            Directory.CreateDirectory(cabFolder);

                        try
                        {
                            onProgress(WinQualProgressType.DownloadingCab, stackHashProduct, stackHashFile, stackHashEvent, stackHashCab);

                            String cabFileName = m_AtomFeed.DownloadCab(cab, false, cabFolder);

                            if (cabFileName == null)
                                throw new StackHashException("Unable to download cab: " + cab.Cab.Id.ToString(CultureInfo.InvariantCulture), StackHashServiceErrorCode.CabDownloadFailed);

                            if (File.Exists(cabFileName))
                            {
                                FileInfo fileInfo = new FileInfo(cabFileName);

                                if (fileInfo.Length != cab.Cab.SizeInBytes)
                                {
                                    // File was only partially downloaded so delete it.
                                    File.Delete(cabFileName);

                                    throw new StackHashException("Corrupt cab download", StackHashServiceErrorCode.CabIsCorrupt); 
                                }
                                else
                                {
                                    cabDownloaded = true;
                                    m_ConsecutiveCabDownloadFailures = 0;
                                    m_NumCabDownloads++;
                                }
                            }
                        }
                        catch (StackHashException)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Total Cabs Downloaded = " + m_NumCabDownloads.ToString(CultureInfo.InvariantCulture));

                            m_ConsecutiveCabDownloadFailures++;

                            if (m_ConsecutiveCabDownloadFailures >= m_MaxConsecutiveCabDownloadFailures)
                                throw;
                        }
                    }

                    // Add the cab to the database.
                    stackHashCab.CabDownloaded = cabDownloaded;
                    errorIndex.AddCab(stackHashProduct, stackHashFile, stackHashEvent, stackHashCab, false);
                }
            }
        }


        /// <summary>
        /// Synchronizes the online WinQual database with the local copy. 
        /// The local copy is effectively a cache of the online copy.
        /// SyncProgress:
        /// =============
        /// During a sync, the PHASE, and how far into that phase the sync got are recorded. This includes the product, file, event and cab 
        /// that was being processed when the sync stopped (or was cancelled).
        /// The sync will resume from the point it left off when restarted (unless a resync was specified). 
        /// The call to sync passes in the LastSyncProgress. If this is null then it isn't used and sync progresses as normal.
        /// If the LastSyncProgress is specified then the sync will proceed to the correct phase (GetProducts only phase is always executed).
        /// During the phase, no products or files or events will actually be processed until the sync finds the ProductId, FileId and EventId
        /// of the last sync failure. From them, all remaining products, files and events are processed.
        /// 
        /// It is assumed that the product list, file list, event list will be delivered in the same order as the previous call.
        /// </summary>
        /// <param name="errorIndex">Error index to add to.</param>
        /// <param name="forceResynchronize">True - sync everything, false - just sync from last sync time.</param>
        /// <param name="lastProgress">Last point at which the sync stopped.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void SynchronizeWithWinQualOnline(IErrorIndex errorIndex, bool forceResynchronize, StackHashSyncProgress lastProgress)
        {
            if (m_Busy)
                throw new StackHashException("Cannot synchronize when already synchronizing", StackHashServiceErrorCode.TaskAlreadyInProgress);

            if (errorIndex == null)
                throw new ArgumentNullException("errorIndex");

            m_AbortRequested = false;
            m_Busy = true;
            m_ConsecutiveCabDownloadFailures = 0;
            m_NumCabDownloads = 0;
            m_FullSync = forceResynchronize;
            m_SyncStartTime = DateTime.Now;

            if ((lastProgress != null) && (lastProgress.EventTypeName != null))
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Syncing with: Force Sync: " + forceResynchronize.ToString() +
                    " Last Progress: " + lastProgress.ProductId.ToString(CultureInfo.InvariantCulture) + ", " +
                    lastProgress.FileId.ToString(CultureInfo.InvariantCulture) + ", " +
                    lastProgress.EventId.ToString(CultureInfo.InvariantCulture) + ", " +
                    lastProgress.EventTypeName + ", " +
                    lastProgress.CabId.ToString(CultureInfo.InvariantCulture) + ", " +
                    lastProgress.SyncPhase.ToString());
            }
            else
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Syncing with: Force Sync: " + forceResynchronize.ToString());
            }


            // The last progress may have stopped during a product which is now disabled. In this case restart from the beginning.
            if ((lastProgress != null) && isExcludedProduct(lastProgress.ProductId) && lastProgress.ProductId != 0)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Syncing with: Progress is for excluded product " + lastProgress.ProductId.ToString(CultureInfo.InvariantCulture));
                lastProgress = null;
            }


            // Record the sync progress.
            m_LastSyncProgress = lastProgress;
            m_LastSyncPositionReached = false;

            if (m_LastSyncProgress != null)
            {
                if ((m_LastSyncProgress.ProductId == 0) || (m_LastSyncProgress.FileId == 0) || (m_LastSyncProgress.EventId == 0) ||
                    (m_LastSyncProgress.SyncPhase == StackHashSyncPhase.Unknown))
                {
                    m_LastSyncProgress = null;
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Syncing: Ignoring last progress: ");
                }
            }

            m_SyncProgress = new StackHashSyncProgress();

            if (m_FullSync)
                m_LastSyncProgress = null;

            try
            {
                AtomProductCollection products = m_AtomFeed.GetProducts();

                // Update the last updated time. A product ID of -2 is a special case used to store the last time the 
                // site was updated.
                errorIndex.SetLastSyncTimeLocal(-2, products.DateFeedUpdated);

                // First download just the product list.
                onProgress(WinQualProgressType.DownloadingProductList, null);
                m_SyncProgress.SyncPhase = StackHashSyncPhase.ProductsOnly;
                UpdateProducts(forceResynchronize, errorIndex, products, false, false); // Don't get events or cabs first time around.
                onProgress(WinQualProgressType.ProductListUpdated, null);

                // Check if any products require a sync.
                if ((ProductsToSynchronize != null) && (ProductsToSynchronize.Count != 0))
                {
                    // Don't redo the events phase. Just get the cabs and eventinfos if the last sync stopped in that phase.
                    if ((m_LastSyncProgress == null) || 
                        ((m_LastSyncProgress != null) && (m_LastSyncProgress.SyncPhase != StackHashSyncPhase.EventInfosAndCabs)))
                    {
                        m_SyncProgress = new StackHashSyncProgress();
                        m_SyncProgress.SyncPhase = StackHashSyncPhase.Events;
                        UpdateProducts(forceResynchronize, errorIndex, products, true, false); // Get the events but not the cabs.
                    }

                    m_SyncProgress = new StackHashSyncProgress();
                    m_SyncProgress.SyncPhase = StackHashSyncPhase.EventInfosAndCabs;
                    UpdateProducts(forceResynchronize, errorIndex, products, true, true); // Get everything.

                    // Clear the progress.
                    m_SyncProgress = new StackHashSyncProgress();
                }
            }
            finally
            {
                m_Busy = false;
            }
        }


        /// <summary>
        /// Retrieves the specified cab file from the WinQual server.
        /// </summary>
        /// <param name="errorIndex">The error index where the cab is to be downloaded.</param>
        /// <param name="product">The product owning the cab.</param>
        /// <param name="file">The file owning the cab.</param>
        /// <param name="theEvent">The event owning the cab.</param>
        /// <param name="cab">The cab to download.</param>
        /// <returns></returns>
        public String GetCab(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (errorIndex == null)
                throw new ArgumentNullException("errorIndex");
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");


            bool cabDownloaded = errorIndex.CabFileExists(product, file, theEvent, cab);
            String cabFileName = null;
            
            // If the cab file is already present then down bother downloading again.
            if (!cabDownloaded)
            {
                // Save the cab to a folder - create the folder if it doesn't exist.
                String cabFolder = errorIndex.GetCabFolder(product, file, theEvent, cab);

                if (!Directory.Exists(cabFolder))
                    Directory.CreateDirectory(cabFolder);

                // Convert to an atom cab so can be downloaded.
                AtomCab atomCab = AtomObjectConversion.ConvertToAtomCab(cab);


                // Download the cab.
                cabFileName = m_AtomFeed.DownloadCab(atomCab, false, cabFolder);

                if (cabFileName == null)
                    throw new StackHashException("Unable to download cab: " + cab.Id.ToString(CultureInfo.InvariantCulture), StackHashServiceErrorCode.CabDownloadFailed);

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
                        // cabsDownloaded++;
                        cabDownloaded = true;
                    }
                }
            }

            // Add the cab to the database.
            cab.CabDownloaded = cabDownloaded;
            errorIndex.AddCab(product, file, theEvent, cab, false);

            if (cabDownloaded)
                return cabFileName;
            else
                return null;
        }


        /// <summary>
        /// Uploads a mapping file.
        /// </summary>
        /// <param name="fileName">File to be uploaded.</param>
        public void UploadFile(String fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            m_AtomFeed.UploadFile(fileName);        
        }



        #endregion
    }
}
