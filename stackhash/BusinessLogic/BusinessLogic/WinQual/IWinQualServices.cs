using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashBusinessObjects;
using StackHashErrorIndex;


namespace StackHashWinQual
{
    /// <summary>
    /// Provides access to the WinQual online service.
    /// </summary>
    public interface IWinQualServices
    {
        /// <summary>
        /// Hook up to receive progress reports from the sync process.
        /// </summary>
        event EventHandler<WinQualProgressEventArgs> Progress;

        /// <summary>
        /// Logs on to the WER online service. The service will exception if it cannot log on.
        /// </summary>
        /// <param name="userName">WER username</param>
        /// <param name="password">WER password</param>
        /// <returns>True - successful logon. False - failed to logon.</returns>
        bool LogOn(string userName, string password);

        /// <summary>
        /// Logs off of WER online service. The service will exception if it cannot log off.
        /// </summary>
        void LogOff();

        
        /// <summary>
        /// Starts a sync to download products, files, events, event infos and cabs.
        /// Data is added to the specified error index.
        /// </summary>
        /// <param name="errorIndex">The index to add the new data to.</param>
        /// <param name="forceResynchronize">True - forces a resync of every event that is available. False - sync new events only.</param>
        /// <param name="lastProgress">Last point at which the sync stopped.</param>
        void SynchronizeWithWinQualOnline(IErrorIndex errorIndex, bool forceResynchronize, StackHashSyncProgress lastProgress);

        /// <summary>
        /// Aborts any sync that is currently in progress.
        /// This will get reset when the next sync occurs.
        /// </summary>
        void AbortCurrentOperation();

        /// <summary>
        /// Set the proxy settings for web requests.
        /// </summary>
        /// <param name="proxySettings">New proxy settings.</param>
        void SetProxySettings(StackHashProxySettings proxySettings);

        /// <summary>
        /// Set the retry parameters for web requests.
        /// </summary>
        /// <param name="requestRetryCount">Number of times to retry.</param>
        /// <param name="requestTimeout">Timeout for each web request.</param>
        void SetWebRequestSettings(int requestRetryCount, int requestTimeout);


        /// <summary>
        /// Sets the license restrictions for the index.
        /// </summary>
        /// <param name="maxEvents">The maximum number of events permitted to be downloaded.</param>
        void SetLicenseRestrictions(long maxEvents);

        /// <summary>
        /// Downloads a specific cab and stores in the specified index.
        /// </summary>
        /// <param name="errorIndex">Error index to store the cab.</param>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to which the cab belongs</param>
        /// <param name="theEvent">Event to which the cab belongs.</param>
        /// <param name="cab">The cab to download.</param>
        /// <returns>Cab file name.</returns>
        String GetCab(IErrorIndex errorIndex, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab);


        /// <summary>
        /// Uploads a mapping file.
        /// </summary>
        /// <param name="fileName">File to be uploaded.</param>
        void UploadFile(String fileName);


        /// <summary>
        /// Represents a list of product IDs that are to be synchronized during a SynchronizeWithWinQualOnline call.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get;
            set;
        }

        /// <summary>
        /// Determines which cabs are to be downloaded during a SynchronizeWithWinQualOnline call.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        StackHashCollectionPolicyCollection CollectionPolicy
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum consecutive cab download failures before the SynchronizeWithWinQualOnline call 
        /// is automatically aborted.
        /// It is possible for a few cabs to be unavailable. These failures should be spread so won't be
        /// consecutive.
        /// </summary>
        int MaxConsecutiveCabDownloadFailures
        {
            get;
            set;
        }


        /// <summary>
        /// Determines if newly added products should be synced as they are found.
        /// </summary>
        bool EnableNewProductsAutomatically
        {
            get;
            set;
        }

        /// <summary>
        /// The point at which the sync operation reached before it stopped.
        /// </summary>
        StackHashSyncProgress SyncProgress
        {
            get;
        }
    }
}
