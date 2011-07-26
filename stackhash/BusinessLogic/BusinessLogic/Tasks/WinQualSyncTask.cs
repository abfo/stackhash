using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    public class WinQualSyncTaskParameters : TaskParameters
    {
        private WinQualContext m_WinQualContext;
        private bool m_ForceFullSynchronize;
        private StackHashProductSyncDataCollection m_ProductsToSynchronize;
        private bool m_JustSyncProductList;
        private StackHashPurgeOptionsCollection m_PurgeOptionsCollection;
        private StackHashCollectionPolicyCollection m_CollectionPolicyCollection;
        private LicenseManager m_LicenseManager;
        private WinQualSettings m_WinQualSettings;
        private long m_TotalStoredEvents;
        private bool m_IsTimedSync;
        private bool m_EnableNewProductsAutomatically;
        private bool m_IsSpecificProductSync;


        public WinQualContext ThisWinQualContext
        {
            get { return m_WinQualContext; }
            set { m_WinQualContext = value; }
        }

        public bool ForceFullSynchronize
        {
            get { return m_ForceFullSynchronize; }
            set { m_ForceFullSynchronize = value; }
        }

        public bool JustSyncProductList
        {
            get { return m_JustSyncProductList; }
            set { m_JustSyncProductList = value; }
        }

        public LicenseManager TheLicenseManager
        {
            get { return m_LicenseManager; }
            set { m_LicenseManager = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get { return m_ProductsToSynchronize; }
            set { m_ProductsToSynchronize = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashPurgeOptionsCollection PurgeOptionsCollection
        {
            get { return m_PurgeOptionsCollection; }
            set { m_PurgeOptionsCollection = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashCollectionPolicyCollection CollectionPolicy
        {
            get { return m_CollectionPolicyCollection; }
            set { m_CollectionPolicyCollection = value; }
        }

        public WinQualSettings WinQualSettings
        {
            get { return m_WinQualSettings; }
            set { m_WinQualSettings = value; }
        }

        public long TotalStoredEvents
        {
            get { return m_TotalStoredEvents; }
            set { m_TotalStoredEvents = value; }
        }

        public bool IsTimedSync
        {
            get { return m_IsTimedSync; }
            set { m_IsTimedSync = value; }
        }

        public bool EnableNewProductsAutomatically
        {
            get { return m_EnableNewProductsAutomatically; }
            set { m_EnableNewProductsAutomatically = value; }
        }

        public bool IsSpecificProductSync
        {
            get { return m_IsSpecificProductSync; }
            set { m_IsSpecificProductSync = value; }
        }
    }


    public class WinQualSyncTask : Task
    {
        private WinQualSyncTaskParameters m_TaskParameters;
        private IWinQualServices m_WinQualServices;
        private StackHashSynchronizeStatistics m_Statistics;
        private WinQualContext m_WinQualContext;
        private bool m_IsResync;
        private System.Exception m_LastLogOnException;


        public WinQualSyncTask(WinQualSyncTaskParameters taskParameters) : 
            base(taskParameters as TaskParameters, StackHashTaskType.WinQualSynchronizeTask)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");
            m_TaskParameters = taskParameters;
            m_WinQualServices = m_TaskParameters.ThisWinQualContext.WinQualServices;
        }

        public bool JustSyncProductList
        {
            get { return m_TaskParameters.JustSyncProductList; }
        }

        public StackHashSynchronizeStatistics Statistics
        {
            get { return m_Statistics; }
        }

        public System.Exception LastLogOnException
        {
            get { return m_LastLogOnException; }
        }



        /// <summary>
        /// Determine if newly found products should be enabled for sync by default.
        /// </summary>
        /// <returns>True - new products should be enabled, false - new products should not be enabled.</returns>
        private bool shouldEnableNewProductsAutomatically()
        {
            // Check that the profile settings say it is ok.
            // and the user hasn't requested the sync of a specific product.
            // and this isn't just a product only sync (which only happens on creation of a new profile).
            if (m_TaskParameters.EnableNewProductsAutomatically &&
                !m_TaskParameters.IsSpecificProductSync &&
                !m_TaskParameters.JustSyncProductList)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Called when a change occurs to the underlying real index.
        /// Just reports to any upstream objects.
        /// </summary>
        /// <param name="source">Should be the real index.</param>
        /// <param name="e">Identifies the change.</param>
        private void ErrorIndexUpdated(Object source, ErrorIndexEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            if (e.ChangeInformation == null)
                throw new ArgumentException("Change information is invalid", "e");

            if (e.ChangeInformation.TypeOfChange == StackHashChangeType.NewEntry)
            {
                switch (e.ChangeInformation.DataThatChanged)
                {
                    case StackHashDataChanged.Product:
                        m_Statistics.Products++;

                        // New products should be enabled automatically if requested.
                        if (shouldEnableNewProductsAutomatically())
                        {
                            // Check if auto product enabling is enabled.
                            StackHashProductSyncData productToSync = new StackHashProductSyncData((int)e.ChangeInformation.ProductId);
                            m_TaskParameters.SettingsManager.SetProductSyncData(m_TaskParameters.ContextId, productToSync);
                        }
                        break;
                    case StackHashDataChanged.File:
                        m_Statistics.Files++;
                        break;
                    case StackHashDataChanged.Event:
                        m_Statistics.Events++;
                        break;
                    case StackHashDataChanged.Hit:
                        m_Statistics.EventInfos++;
                        break;
                    case StackHashDataChanged.Cab:
                        m_Statistics.Cabs++;
                        break;
                }
            }
        }



        /// <summary>
        /// Receives progress reports from the WinQualServices when a sync is in progress.
        /// The data is used to generate a progress report for the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WinQualSyncEventHandler(Object sender, WinQualProgressEventArgs args)
        {
            StackHashSyncProgressAdminReport adminReport = new StackHashSyncProgressAdminReport();
            adminReport.ClientData = m_TaskParameters.ClientData;
            adminReport.ContextId = m_TaskParameters.ContextId;
            adminReport.LastException = null;
            adminReport.ResultData = StackHashAsyncOperationResult.Success;
            adminReport.TotalPages = args.TotalPages;
            adminReport.CurrentPage = args.CurrentPage;
            adminReport.IsResync = m_IsResync;
            adminReport.IsProductsOnlySync = m_TaskParameters.JustSyncProductList;
            adminReport.Product = args.Product;

            if (args.Product != null)
            {
                adminReport.Description = String.Format(CultureInfo.InvariantCulture, "{0} {1}", args.Product.Product.Name, args.Product.Product.Version);
            }
            
            if (args.File != null)
                adminReport.File = (StackHashFile)args.File.Clone();
            if (args.TheEvent != null)
                adminReport.TheEvent = (StackHashEvent)args.TheEvent.Clone();
            if (args.Cab != null)
                adminReport.Cab = (StackHashCab)args.Cab.Clone();

            bool sendReport = true;

            switch (args.ProgressType)
            {
                case WinQualProgressType.DownloadingProductList:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressDownloadingProductList;
                    break;
                case WinQualProgressType.ProductListUpdated:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressProductListUpdated;
                    break;
                case WinQualProgressType.DownloadingProductEvents:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressDownloadingProductEvents;
                    break;
                case WinQualProgressType.ProductEventsUpdated:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressProductEventsUpdated;
                    break;
                case WinQualProgressType.DownloadingProductCabs:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressDownloadingProductCabs;
                    break;
                case WinQualProgressType.ProductCabsUpdated:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressProductCabsUpdated;
                    break;
                case WinQualProgressType.DownloadingEventPage:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressDownloadingEventPage;
                    break;
                case WinQualProgressType.DownloadingCab:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncProgressDownloadingCab;
                    break;
                case WinQualProgressType.Complete:
                    adminReport.Operation = StackHashAdminOperation.WinQualSyncCompleted;
                    sendReport = false;
                    break;
                default:
                    throw new InvalidOperationException("Unexpected progress type");
            }

            if (sendReport)
            {
                AdminReportEventArgs adminReportArgs = new AdminReportEventArgs(adminReport, true);
                if (Reporter.CurrentReporter != null)
                    Reporter.CurrentReporter.ReportEvent(adminReportArgs);
            }
        }



        /// <summary>
        /// Checks to see if the license is present and not in an expired state.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private void checkLicense()
        {
            // If no license is defined then try and get a trial license.
            if (!m_TaskParameters.TheLicenseManager.LicenseData.LicenseDefined)
            {
                // TODO: Get the trial license.
            }

            // If still no license defined then bomb.
            if (!m_TaskParameters.TheLicenseManager.LicenseData.LicenseDefined)
            {
                throw new StackHashException("Licensed not installed", StackHashServiceErrorCode.NoLicense);
            }

            try
            {
                // Refresh the license.
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to refresh license", ex);
            }

            // Check expiry date.
            if (m_TaskParameters.TheLicenseManager.ExpiryUtc < DateTime.Now.ToUniversalTime())
            {
                throw new StackHashException("Licensed expired: " +
                    m_TaskParameters.TheLicenseManager.ExpiryUtc.ToString(CultureInfo.InvariantCulture),
                    StackHashServiceErrorCode.LicenseExpired);
            }
        }


        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            bool loginFailed = false;
            bool loggedOn = false;

            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);


                // Don't allow the PC to go into sleep mode while syncing.
                StackHashUtilities.SystemInformation.DisableSleep();

                DateTime startSyncTime = DateTime.Now.ToUniversalTime();

                checkLicense();

                // Hook up to receive events about new error index items.
                m_TaskParameters.ErrorIndex.IndexUpdated += new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdated);
                m_Statistics = new StackHashSynchronizeStatistics();

                try
                {
                    // Hook up to the WinQual service for progress events.
                    m_WinQualServices.Progress += new EventHandler<WinQualProgressEventArgs>(this.WinQualSyncEventHandler);

                    // Create a new WinQual context. This stores information about the 
                    // configuration and state of the WinQual connection.
                    // Note that the WinQualServices is passed in here so that WinQualContext can
                    // be tested with a dummy.
                    m_WinQualContext = new WinQualContext(m_WinQualServices);


                    try
                    {
                        // Log on to WinQual.
                        m_WinQualContext.WinQualServices.LogOn(m_TaskParameters.WinQualSettings.UserName, m_TaskParameters.WinQualSettings.Password);
                        loggedOn = true;
                    }
                    catch (System.Exception ex)
                    {
                        m_LastLogOnException = ex;
                        loginFailed = true;
                        throw;
                    }

                    // Set the products that should not be synced.
                    if (m_TaskParameters.JustSyncProductList)
                        m_WinQualServices.ProductsToSynchronize = null;
                    else
                        m_WinQualServices.ProductsToSynchronize = m_TaskParameters.ProductsToSynchronize;

                    m_WinQualServices.EnableNewProductsAutomatically = shouldEnableNewProductsAutomatically();

                    
                    // Set the collection policy - dictates the number of cabs to download etc...
                    m_WinQualServices.CollectionPolicy = m_TaskParameters.CollectionPolicy;

                    m_WinQualServices.MaxConsecutiveCabDownloadFailures = m_TaskParameters.WinQualSettings.MaxCabDownloadFailuresBeforeAbort;

                    long totalEventsAllowedForThisContext = m_TaskParameters.TheLicenseManager.MaxEvents
                        - (m_TaskParameters.TotalStoredEvents - m_TaskParameters.ErrorIndex.TotalStoredEvents);
                    if (totalEventsAllowedForThisContext < 0)
                        totalEventsAllowedForThisContext = 0;

                    m_WinQualServices.SetLicenseRestrictions(totalEventsAllowedForThisContext);


                    // Determine if we should be doing a full sync or not.
                    // After the nth sync a resync is forced.
                    m_IsResync = m_TaskParameters.ForceFullSynchronize;

                    int syncCount = m_TaskParameters.ErrorIndex.SyncCount;

                    syncCount++;
                    if ((syncCount >= m_TaskParameters.WinQualSettings.SyncsBeforeResync) &&
                        (!m_TaskParameters.JustSyncProductList))
                    {
                        m_IsResync = true;
                    }

                    // Get the last point at which the sync failed.
                    StackHashSyncProgress lastProgress = m_TaskParameters.ErrorIndex.SyncProgress;
                    if (m_IsResync)
                        lastProgress = null;

                    // Call the WinQual services to synchronize.
                    m_WinQualServices.SynchronizeWithWinQualOnline(m_TaskParameters.ErrorIndex, m_IsResync, lastProgress);

                    // Don't update the sync count for a mere product only sync.
                    if (!m_TaskParameters.JustSyncProductList)
                    {
                        if (m_IsResync)
                            m_TaskParameters.ErrorIndex.SyncCount = 0;
                        else
                            m_TaskParameters.ErrorIndex.SyncCount = syncCount;
                    }


                    if (m_TaskParameters.ErrorIndex.TotalStoredEvents >= totalEventsAllowedForThisContext)
                    {
                        throw new StackHashException("Licensed event limit reached: " +
                            m_TaskParameters.ErrorIndex.TotalStoredEvents.ToString(CultureInfo.InvariantCulture),
                            StackHashServiceErrorCode.LicenseEventCountExceeded);
                    }
                }
                finally
                {
                    try
                    {
                        if (loggedOn)
                            m_WinQualContext.WinQualServices.LogOff();
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to log off Win Qual", ex);
                    }

                    // Don't reset the sync progress for just product only syncs or if the login to winqual failed.
                    if (!m_TaskParameters.JustSyncProductList && !loginFailed) 
                        m_TaskParameters.ErrorIndex.SyncProgress = m_WinQualServices.SyncProgress;

                    m_WinQualServices.Progress -= new EventHandler<WinQualProgressEventArgs>(this.WinQualSyncEventHandler);

                    // Unhook from the event index.
                    m_TaskParameters.ErrorIndex.IndexUpdated -= new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdated);
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                StackHashUtilities.SystemInformation.EnableSleep();
                SetTaskCompleted(m_TaskParameters.ErrorIndex);
            }
        }

        /// <summary>
        /// Abort the current task.
        /// </summary>
        public override void StopExternal()        
        {
            WritableTaskState.Aborted = true;
            m_WinQualServices.AbortCurrentOperation();
            m_TaskParameters.ErrorIndex.AbortCurrentOperation();
            base.StopExternal();
        }
    }
}
