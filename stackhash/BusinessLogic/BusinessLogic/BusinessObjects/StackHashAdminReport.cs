using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StackHashBusinessObjects
{
    /// <summary>
    /// Asynchronous admin operations.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashAdminOperation
    {
        /// <summary>
        /// An admin client has registered for events.
        /// </summary>
        [EnumMember()]
        AdminRegister,

        /// <summary>
        /// An admin client has unregistered for events.
        /// </summary>
        [EnumMember()]
        AdminUnregister,

        /// <summary>
        /// The WinQualSync task has started. Note it may have been started by a different client
        /// or it may have been started on a schedule.
        /// </summary>
        [EnumMember()]
        WinQualSyncStarted,

        /// <summary>
        /// The WinQualSync task has completed.
        /// </summary>
        [EnumMember()]
        WinQualSyncCompleted,

        /// <summary>
        /// The purge task has started. Note it may have been started by a different client
        /// or it may have been started on a schedule. 
        /// </summary>
        [EnumMember()]
        PurgeStarted,

        /// <summary>
        /// The purge task has completed.
        /// </summary>
        [EnumMember()]
        PurgeCompleted,

        /// <summary>
        /// Move of the error index started.
        /// </summary>
        [EnumMember()]
        ErrorIndexMoveStarted,

        /// <summary>
        /// Move of the error index completed.
        /// </summary>
        [EnumMember()]
        ErrorIndexMoveCompleted,

        /// <summary>
        /// Cab analysis started.
        /// </summary>
        [EnumMember()]
        AnalyzeStarted,

        /// <summary>
        /// Cab analysis completed.
        /// </summary>
        [EnumMember()]
        AnalyzeCompleted,

        /// <summary>
        /// Run script task started.
        /// </summary>
        [EnumMember()]
        RunScriptStarted,

        /// <summary>
        /// Run script task completed.
        /// </summary>
        [EnumMember()]
        RunScriptCompleted,

        /// <summary>
        /// The WinQualSync task announces progress.
        /// </summary>
        [EnumMember()]
        WinQualSyncProgressDownloadingProductList,
        [EnumMember()]
        WinQualSyncProgressProductListUpdated,
        [EnumMember()]
        WinQualSyncProgressDownloadingProductEvents,
        [EnumMember()]
        WinQualSyncProgressProductEventsUpdated,
        [EnumMember()]
        WinQualSyncProgressDownloadingProductCabs,
        [EnumMember()]
        WinQualSyncProgressProductCabsUpdated,

        /// <summary>
        /// Used to test out the WinQual logon credentials.
        /// </summary>
        [EnumMember()]
        WinQualLogOnStarted,

        /// <summary>
        /// Used to test out the WinQual logon credentials.
        /// </summary>
        [EnumMember()]
        WinQualLogOnCompleted,

        /// <summary>
        /// Download cab started.
        /// </summary>
        [EnumMember()]
        DownloadCabStarted,

        /// <summary>
        /// Download cab completed.
        /// </summary>
        [EnumMember()]
        DownloadCabCompleted,

        [EnumMember()]
        WinQualSyncProgressDownloadingEventPage,

        [EnumMember()]
        WinQualSyncProgressDownloadingCab,
    
        [EnumMember()]
        ErrorIndexCopyStarted,

        [EnumMember()]
        ErrorIndexCopyCompleted,

        [EnumMember()]
        ErrorIndexCopyProgress,

        [EnumMember()]
        ContextStateChanged,

        [EnumMember()]
        BugTrackerStarted,

        [EnumMember()]
        BugTrackerCompleted,

        [EnumMember()]
        BugReportStarted,

        [EnumMember()]
        BugReportCompleted,

        [EnumMember()]
        BugReportProgress,

        [EnumMember()]
        BugTrackerPlugInStatus,

        [EnumMember()]
        ErrorIndexMoveProgress,

        [EnumMember()]
        UploadFileStarted,

        [EnumMember()]
        UploadFileCompleted,

        // IF YOU ADD ANOTHER EMAIL - ALSO ADD TO THE FRIENDLY STRING BELOW.
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashAdminOperationCollection : Collection<StackHashAdminOperation>
    {
        public StackHashAdminOperationCollection() { } // Needed to serialize.

        public static String GetFriendlyName(StackHashAdminOperation operation)
        {
            switch (operation)
            {
                case StackHashAdminOperation.WinQualSyncStarted:
                    return ("Synchronize with WinQual on-line has started");

                case StackHashAdminOperation.WinQualSyncCompleted:
                    return ("Synchronize with WinQual on-line has completed");

                case StackHashAdminOperation.PurgeStarted:
                    return ("Purge of old cabs has started");

                case StackHashAdminOperation.PurgeCompleted:
                    return ("Purge of old cabs has completed");

                case StackHashAdminOperation.ErrorIndexMoveStarted:
                    return ("Event database move has started");

                case StackHashAdminOperation.ErrorIndexMoveCompleted:
                    return ("Event database move has completed");

                case StackHashAdminOperation.AnalyzeStarted:
                    return ("Analysis of cabs has started");

                case StackHashAdminOperation.AnalyzeCompleted:
                    return ("Analysis of cabs has completed");

                case StackHashAdminOperation.ErrorIndexCopyStarted:
                    return ("Event database copy has started");

                case StackHashAdminOperation.ErrorIndexCopyCompleted:
                    return ("Event database copy has completed");

                case StackHashAdminOperation.BugReportStarted:
                    return ("Plug-in reporting has started");

                case StackHashAdminOperation.BugReportCompleted:
                    return ("Plug-in reporting has completed");

                case StackHashAdminOperation.UploadFileStarted:
                    return ("Upload of mapping file has started");

                case StackHashAdminOperation.UploadFileCompleted:
                    return ("Upload of mapping file has completed");

                default:
                    return operation.ToString();
            }
        }
    }

    
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashAsyncOperationResult
    {
        [EnumMember()]
        Success,
        [EnumMember()]
        Failed,
        [EnumMember()]
        Aborted
    }


    /// <summary>
    /// Contains server diagnostic report information.
    /// This will usually be the result of a previous asynchronous operation. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [KnownType(typeof(StackHashPurgeCompleteAdminReport))]
    [KnownType(typeof(StackHashWinQualSyncCompleteAdminReport))]
    [KnownType(typeof(StackHashSyncProgressAdminReport))]
    [KnownType(typeof(StackHashCopyIndexProgressAdminReport))]
    [KnownType(typeof(StackHashContextStateAdminReport))]
    [KnownType(typeof(StackHashBugReportProgressAdminReport))]
    [KnownType(typeof(StackHashMoveIndexProgressAdminReport))]
    [KnownType(typeof(StackHashBugTrackerStatusAdminReport))]
    public class StackHashAdminReport
    {
        StackHashAsyncOperationResult m_ResultData;
        StackHashAdminOperation m_AdminOperation;
        StackHashServiceErrorCode m_ServiceErrorCode;
        String m_Message;
        String m_LastException;
        int m_ContextId;
        StackHashClientData m_ClientData;
        String m_Description;
        String m_ServiceGuid;
        String m_ServiceHost;
        bool m_IsRetry;

        /// <summary>
        /// The result of the request.
        /// </summary>
        [DataMember]
        public StackHashAsyncOperationResult ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// The result of the request.
        /// </summary>
        [DataMember]
        public StackHashAdminOperation Operation
        {
            get { return m_AdminOperation; }
            set { m_AdminOperation = value; }
        }
 
        /// <summary>
        /// The last exception that occurred in the task in string form.
        /// </summary>
        [DataMember]
        public String LastException
        {
            get { return m_LastException; }
            set { m_LastException = value; }
        }

        /// <summary>
        /// Indentifies the error that occurred. 
        /// </summary>
        [DataMember]
        public StackHashServiceErrorCode ServiceErrorCode
        {
            get { return m_ServiceErrorCode; }
            set { m_ServiceErrorCode = value; }
        }

        /// <summary>
        /// Message (in English) corresponding to the error. 
        /// </summary>
        [DataMember]
        public String Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        /// <summary>
        /// WinQual login context to which the event refers.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Identifies the client and request.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }


        /// <summary>
        /// Description - textual description of the event.
        /// </summary>
        [DataMember]
        public String Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        /// <summary>
        /// Service GUID. Unique per settings file. 
        /// </summary>
        [DataMember]
        public String ServiceGuid
        {
            get { return m_ServiceGuid; }
            set { m_ServiceGuid = value; }
        }

        /// <summary>
        /// Service host name. 
        /// </summary>
        [DataMember]
        public String ServiceHost
        {
            get { return m_ServiceHost; }
            set { m_ServiceHost = value; }
        }

        /// <summary>
        /// Indicates if the report was sent for a task that was initiated by the service as
        /// a retry.
        /// e.g. if the WinQualSync fails then it will retry periodically.
        /// </summary>
        [DataMember]
        public bool IsRetry
        {
            get { return m_IsRetry; }
            set { m_IsRetry = value; }
        }

        public override String ToString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("Operation: " + StackHashAdminOperationCollection.GetFriendlyName(m_AdminOperation));

            if (m_ServiceErrorCode == StackHashServiceErrorCode.NoError)
                result.AppendLine("Result: Success");
            else
                result.AppendLine("Result: " + m_ServiceErrorCode.ToString());

            if ((m_ClientData != null) && (m_ClientData.ClientName != null))
                result.AppendLine("Initiator: " + m_ClientData.ClientName);

            if (m_LastException != null)
                result.AppendLine("Error detail: " + m_LastException);

            return result.ToString();
        }
    }

    /// <summary>
    /// Specialization of the admin report for Purge reports.
    /// Purge report contains statistics about files purged and their sizes.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashPurgeCompleteAdminReport : StackHashAdminReport
    {
        StackHashPurgeStatistics m_PurgeStatistics;

        public StackHashPurgeCompleteAdminReport() : base() { ; } // Required for serialization.


        /// <summary>
        /// The result of the request.
        /// </summary>
        [DataMember]
        public StackHashPurgeStatistics PurgeStatistics
        {
            get { return m_PurgeStatistics; }
            set { m_PurgeStatistics = value; }
        }

        public override String ToString()
        {
            StringBuilder result = new StringBuilder(base.ToString());

            if (m_PurgeStatistics != null)
            {
                result.AppendLine("Number of cabs purged: " + m_PurgeStatistics.NumberOfCabs.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Size of cabs purged: " + m_PurgeStatistics.CabsTotalSize.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Number of dump files purged: " + m_PurgeStatistics.NumberOfDumpFiles.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Size of dump files purged: " + m_PurgeStatistics.DumpFilesTotalSize.ToString(CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }
    }


    /// <summary>
    /// Specialization of the winqual sync task admin report.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashWinQualSyncCompleteAdminReport : StackHashAdminReport
    {
        StackHashSynchronizeStatistics m_ErrorIndexStatistics;

        public StackHashWinQualSyncCompleteAdminReport() : base() { ; } // Required for serialization.


        /// <summary>
        /// Error index statistics. Only set if the WinQualCompletion task.
        /// In this case it means the NEW events.
        /// </summary>
        [DataMember]
        public StackHashSynchronizeStatistics ErrorIndexStatistics
        {
            get { return m_ErrorIndexStatistics; }
            set { m_ErrorIndexStatistics = value; }
        }

        
        public override String ToString()
        {
            StringBuilder result = new StringBuilder(base.ToString());

            if (m_ErrorIndexStatistics != null)
            {
                result.AppendLine("Mapped products added: " + m_ErrorIndexStatistics.Products.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Mapped files added: " + m_ErrorIndexStatistics.Files.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Events added: " + m_ErrorIndexStatistics.Events.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Hits added: " + m_ErrorIndexStatistics.EventInfos.ToString(CultureInfo.InvariantCulture));
                result.AppendLine("Cabs added: " + m_ErrorIndexStatistics.Cabs.ToString(CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }
    }

    /// <summary>
    /// Specialization of the admin report for WinQualSync progress reports.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSyncProgressAdminReport : StackHashAdminReport
    {
        StackHashProductInfo m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;

        int m_TotalPages;
        int m_CurrentPage;

        bool m_IsResync;
        bool m_IsProductsOnlySync;


        public StackHashSyncProgressAdminReport() : base() { ; } // Required for serialization.


        /// <summary>
        /// Product being synced.
        /// </summary>
        [DataMember]
        public StackHashProductInfo Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File being synced.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event being synced.
        /// </summary>
        [DataMember]
        public StackHashEvent TheEvent
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab being synced.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// Total pages.
        /// </summary>
        [DataMember]
        public int TotalPages
        {
            get { return m_TotalPages; }
            set { m_TotalPages = value; }
        }

        /// <summary>
        /// Current pages.
        /// </summary>
        [DataMember]
        public int CurrentPage
        {
            get { return m_CurrentPage; }
            set { m_CurrentPage = value; }
        }

        /// <summary>
        /// Indicates if this is a full resync.
        /// </summary>
        [DataMember]
        public bool IsResync
        {
            get { return m_IsResync; }
            set { m_IsResync = value; }
        }

        /// <summary>
        /// Just syncing the products.
        /// </summary>
        [DataMember]
        public bool IsProductsOnlySync
        {
            get { return m_IsProductsOnlySync; }
            set { m_IsProductsOnlySync = value; }
        }
    }

    /// <summary>
    /// Specialization of the admin report for CopyIndex progress reports.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashCopyIndexProgressAdminReport : StackHashAdminReport
    {
        String m_SourceIndexFolder;
        String m_SourceIndexName;
        String m_DestinationIndexFolder;
        String m_DestinationIndexName;

        long m_CurrentEvent;
        long m_TotalEvents;
        int m_CurrentEventId;

        public StackHashCopyIndexProgressAdminReport() : base() { ; } // Required for serialization.


        /// <summary>
        /// Old index folder.
        /// </summary>
        [DataMember]
        public String SourceIndexFolder
        {
            get { return m_SourceIndexFolder; }
            set { m_SourceIndexFolder = value; }
        }

        /// <summary>
        /// Old index name.
        /// </summary>
        [DataMember]
        public String SourceIndexName
        {
            get { return m_SourceIndexName; }
            set { m_SourceIndexName = value; }
        }

        /// <summary>
        /// New index folder.
        /// </summary>
        [DataMember]
        public String DestinationIndexFolder
        {
            get { return m_DestinationIndexFolder; }
            set { m_DestinationIndexFolder = value; }
        }

        /// <summary>
        /// New index name.
        /// </summary>
        [DataMember]
        public String DestinationIndexName
        {
            get { return m_DestinationIndexName; }
            set { m_DestinationIndexName = value; }
        }

        /// <summary>
        /// The current event - not the id.
        /// </summary>
        [DataMember]
        public long CurrentEvent
        {
            get { return m_CurrentEvent; }
            set { m_CurrentEvent = value; }
        }

        /// <summary>
        /// Total events.
        /// </summary>
        [DataMember]
        public long TotalEvents
        {
            get { return m_TotalEvents; }
            set { m_TotalEvents = value; }
        }

        /// <summary>
        /// Current Event ID.
        /// </summary>
        [DataMember]
        public int CurrentEventId
        {
            get { return m_CurrentEventId; }
            set { m_CurrentEventId = value; }
        }
    }

    /// <summary>
    /// Specialization of the admin report for MoveIndex progress reports.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashMoveIndexProgressAdminReport : StackHashAdminReport
    {
        private String m_CurrentFileName;
        private int m_FileCount;
        private bool m_IsCopyStart;

        public StackHashMoveIndexProgressAdminReport() : base() { ; } // Required for serialization.


        /// <summary>
        /// Current file being copied (or which has been copied).
        /// </summary>
        [DataMember]
        public String CurrentFileName
        {
            get { return m_CurrentFileName; }
            set { m_CurrentFileName = value; }
        }

        /// <summary>
        /// Number of files copied so far.
        /// </summary>
        [DataMember]
        public int FileCount
        {
            get { return m_FileCount; }
            set { m_FileCount = value; }
        }

        /// <summary>
        /// Is this the start or end of a file copy?
        /// </summary>
        [DataMember]
        public bool IsCopyStart
        {
            get { return m_IsCopyStart; }
            set { m_IsCopyStart = value; }
        }
    }

    
    /// <summary>
    /// Specialization of the admin report for Context related progress reports.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashContextStateAdminReport : StackHashAdminReport
    {
        bool m_IsActive;
        bool m_IsActivationAttempt;

        public StackHashContextStateAdminReport() : base() { ; } // Required for serialization.

        public StackHashContextStateAdminReport(bool isActive, bool isActivationAttempt)
        {
            m_IsActive = isActive;
            m_IsActivationAttempt = isActivationAttempt;
        }

        /// <summary>
        /// Indicates if the context is now active or inactive.
        /// </summary>
        [DataMember]
        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; }
        }

        /// <summary>
        /// Indicates if this was an attempt to activate or deactivate the context.
        /// </summary>
        [DataMember]
        public bool IsActivationAttempt
        {
            get { return m_IsActivationAttempt; }
            set { m_IsActivationAttempt = value; }
        }
    }


    /// <summary>
    /// Specialization of the admin report for BugReport progress reports.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugReportProgressAdminReport : StackHashAdminReport
    {
        long m_CurrentEvent;
        long m_TotalEvents;
        int m_CurrentEventId;

        public StackHashBugReportProgressAdminReport() : base() { ; } // Required for serialization.


        /// <summary>
        /// The current event - not the id.
        /// </summary>
        [DataMember]
        public long CurrentEvent
        {
            get { return m_CurrentEvent; }
            set { m_CurrentEvent = value; }
        }

        /// <summary>
        /// Total events.
        /// </summary>
        [DataMember]
        public long TotalEvents
        {
            get { return m_TotalEvents; }
            set { m_TotalEvents = value; }
        }

        /// <summary>
        /// Current Event ID.
        /// </summary>
        [DataMember]
        public int CurrentEventId
        {
            get { return m_CurrentEventId; }
            set { m_CurrentEventId = value; }
        }
    }


    /// <summary>
    /// Specialization of the admin report for BugTackerStatus reports.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugTrackerStatusAdminReport : StackHashAdminReport
    {
        StackHashBugTrackerPlugInDiagnosticsCollection m_PlugInDiagnostics;

        public StackHashBugTrackerStatusAdminReport() : base() { ; } // Required for serialization.

        /// <summary>
        /// Plug-in diagnostics.
        /// </summary>
        [DataMember]
        public StackHashBugTrackerPlugInDiagnosticsCollection PlugInDiagnostics
        {
            get { return m_PlugInDiagnostics; }
            set { m_PlugInDiagnostics = value; }
        }
    }
}
