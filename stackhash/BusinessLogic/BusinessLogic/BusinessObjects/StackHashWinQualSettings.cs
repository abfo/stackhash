using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    /// <summary>
    /// Login details required to access the WinQual site.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class WinQualSettings
    {
        private String m_UserName;
        private String m_Password;
        private String m_CompanyName;
        private int m_AgeOldToPurgeInDays;
        private StackHashProductSyncDataCollection m_ProductsToSynchronize;
        private int m_RequestRetryCount;
        private int m_RequestTimeout;
        private bool m_RetryAfterError;
        private int m_DelayBeforeRetryInSeconds;
        private int m_MaxCabDownloadFailuresBeforeAbort;
        private int m_SyncsBeforeResync;
        private bool m_EnableNewProductsAutomatically;

        private static int s_SyncsBeforeResyncDefault = 7;

        [DataMember]
        public String UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }
        [DataMember]
        public String Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }
        /// <summary>
        /// Note used for login but should uniquely identify the company e.g. Microsoft.
        /// </summary>
        [DataMember]
        public String CompanyName
        {
            get { return m_CompanyName; }
            set { m_CompanyName = value; }
        }

        /// <summary>
        /// Cabs older than this date will be purged.
        /// </summary>
        [DataMember]
        public int AgeOldToPurgeInDays
        {
            get { return m_AgeOldToPurgeInDays; }
            set { m_AgeOldToPurgeInDays = value; }
        }

        /// <summary>
        /// List of products that should be sychronized.
        /// </summary>
        [DataMember]
        public StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get { return m_ProductsToSynchronize; }
            set { m_ProductsToSynchronize = value; }
        }

        /// <summary>
        /// Number of times to retry a web request. 1 = try once.
        /// </summary>
        [DataMember]
        public int RequestRetryCount
        {
            get { return m_RequestRetryCount; }
            set { m_RequestRetryCount = value; }
        }

        /// <summary>
        /// Number of milliseconds to wait for a web response.
        /// </summary>
        [DataMember]
        public int RequestTimeout
        {
            get { return m_RequestTimeout; }
            set { m_RequestTimeout = value; }
        }

        /// <summary>
        /// True - if the scheduled task fails then restart automatically after "DelayBeforeRetry" minutes.
        /// </summary>
        [DataMember]
        public bool RetryAfterError
        {
            get { return m_RetryAfterError; }
            set { m_RetryAfterError = value; }
        }

        /// <summary>
        /// If RetryAfterError is True then this field indicates the delay in seconds before retrying.
        /// </summary>
        [DataMember]
        public int DelayBeforeRetryInSeconds
        {
            get { return m_DelayBeforeRetryInSeconds; }
            set { m_DelayBeforeRetryInSeconds = value; }
        }

        /// <summary>
        /// Maximum number of cab download failures before the WinQualSync task is aborted.
        /// </summary>
        [DataMember]
        public int MaxCabDownloadFailuresBeforeAbort
        {
            get { return m_MaxCabDownloadFailuresBeforeAbort; }
            set { m_MaxCabDownloadFailuresBeforeAbort = value; }
        }

        /// <summary>
        /// Then number of syncs before a full resync is forced.
        /// </summary>
        [DataMember]
        public int SyncsBeforeResync
        {
            get { return m_SyncsBeforeResync; }
            set { m_SyncsBeforeResync = value; }
        }

        /// <summary>
        /// Enable new products automatically.
        /// </summary>
        [DataMember]
        public bool EnableNewProductsAutomatically
        {
            get { return m_EnableNewProductsAutomatically; }
            set { m_EnableNewProductsAutomatically = value; }
        }

        public static int DefaultSyncsBeforeResync
        {
            get { return s_SyncsBeforeResyncDefault;}
        }

        public WinQualSettings() { ; } // Required for serialization.

        public WinQualSettings(String userName, String password, String companyName, int ageOldToPurgeInDays,
            StackHashProductSyncDataCollection productsToSynchronize, bool retryAfterError, int secondsDelayBeforeRetry, 
            int maxCabDownloadFailuresBeforeAbort, int syncsBeforeResync, bool enableNewProductsAutomatically)
        {
            m_UserName = userName;
            m_Password = password;
            m_CompanyName = companyName;
            m_AgeOldToPurgeInDays = ageOldToPurgeInDays;
            m_ProductsToSynchronize = productsToSynchronize;
            m_RequestRetryCount = 5;
            m_RequestTimeout = 300000;
            m_RetryAfterError = retryAfterError;
            m_DelayBeforeRetryInSeconds = secondsDelayBeforeRetry;
            m_MaxCabDownloadFailuresBeforeAbort = maxCabDownloadFailuresBeforeAbort;
            m_SyncsBeforeResync = syncsBeforeResync;
            m_EnableNewProductsAutomatically = enableNewProductsAutomatically;
        }
    }
}
