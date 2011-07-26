using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Globalization;

using StackHashUtilities;

namespace StackHashBusinessObjects
{
    /// <summary>
    /// Status of a Task
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashTaskState
    {
        [EnumMember()]
        NotRunning,     // A task of the specified type is not running.
        [EnumMember()]
        Queued,         // Queued for execution.
        [EnumMember()]
        Running,        // The task is currently running.
        [EnumMember()]
        Faulted,        // The task has run and exceptioned.
        [EnumMember()]
        Completed,      // The task completed ok.
        [EnumMember()]
        Aborted         // The task was aborted.
    }

    /// <summary>
    /// Type of task.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashTaskType
    {
        [EnumMember()]
        WinQualSynchronizeTask,
        [EnumMember()]
        WinQualSynchronizeTimerTask,
        [EnumMember()]
        PurgeTask,
        [EnumMember()]
        PurgeTimerTask,
        [EnumMember()]
        WinQualLogOnTask,
        [EnumMember()]
        AnalyzeTask,
        [EnumMember()]
        ErrorIndexMoveTask,
        [EnumMember()]
        DebugScriptTask,
        [EnumMember()]
        DummyTask, // testing only
        [EnumMember()]
        DownloadCabTask,
        [EnumMember()]
        ErrorIndexCopyTask,
        [EnumMember()]
        BugTrackerTask,
        [EnumMember()]
        BugReportTask,
        [EnumMember()]
        UploadFileTask,
    }

    /// <summary>
    /// Type of task.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashTaskError
    {
        // WinQualLogon errors
        [EnumMember()]
        None,
        [EnumMember()]
        FailedToLogOnToWinQual,
    }

    /// <summary>
    /// Gives information regarding the run times etc... of particular tasks.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashTaskStatus : IComparable<StackHashTaskStatus>
    {
        private StackHashTaskType m_TaskType;
        private StackHashTaskState m_TaskState;
        private DateTime m_LastSuccessfulRunTimeUtc;
        private DateTime m_LastFailedRunTimeUtc;
        private DateTime m_LastStartedTimeUtc;
        private int m_LastDurationInSeconds;
        private int m_RunCount;
        private int m_SuccessCount;
        private int m_FailedCount;
        private String m_LastException;
        private StackHashServiceErrorCode m_ServiceErrorCode;
        private bool m_CanBeAbortedByClient;

        /// <summary>
        /// The task type.
        /// </summary>
        [DataMember]
        public StackHashTaskType TaskType
        {
            get
            {
                return m_TaskType;
            }
            set
            {
                m_TaskType = value;
            }
        }

        /// <summary>
        /// Identifies the current state of the task.
        /// </summary>
        [DataMember]
        public StackHashTaskState TaskState
        {
            get
            {
                return m_TaskState;
            }
            set
            {
                m_TaskState = value;
            }
        }

        /// <summary>
        /// Last time the task was successfully run.
        /// </summary>
        [DataMember]
        public DateTime LastSuccessfulRunTimeUtc
        {
            get
            {
                return m_LastSuccessfulRunTimeUtc;
            }
            set
            {
                m_LastSuccessfulRunTimeUtc = value;
            }
        }

        /// <summary>
        /// Last time the task was unsuccessfully run.
        /// </summary>
        [DataMember]
        public DateTime LastFailedRunTimeUtc
        {
            get
            {
                return m_LastFailedRunTimeUtc;
            }
            set
            {
                m_LastFailedRunTimeUtc = value;
            }
        }

        /// <summary>
        /// Number of times the task has been run.
        /// </summary>
        [DataMember]
        public int LastDurationInSeconds
        {
            get
            {
                return m_LastDurationInSeconds;
            }
            set
            {
                m_LastDurationInSeconds = value;
            }
        }

        /// <summary>
        /// Last time the task was started.
        /// </summary>
        [DataMember]
        public DateTime LastStartedTimeUtc
        {
            get
            {
                return m_LastStartedTimeUtc;
            }
            set
            {
                m_LastStartedTimeUtc = value;
            }
        }


        
        /// <summary>
        /// Number of times the task has been run.
        /// </summary>
        [DataMember]
        public int RunCount
        {
            get
            {
                return m_RunCount;
            }
            set
            {
                m_RunCount = value;
            }
        }

        /// <summary>
        /// Number of successful runs.
        /// </summary>
        [DataMember]
        public int SuccessCount
        {
            get
            {
                return m_SuccessCount;
            }
            set
            {
                m_SuccessCount = value;
            }
        }

        /// <summary>
        /// Number of failed runs.
        /// </summary>
        [DataMember]
        public int FailedCount
        {
            get
            {
                return m_FailedCount;
            }
            set
            {
                m_FailedCount = value;
            }
        }

        /// <summary>
        /// The last exception that occurred in the task.
        /// </summary>
        [DataMember]
        public String LastException
        {
            get
            {
                return m_LastException;
            }
            set
            {
                m_LastException = value;
            }
        }

        /// <summary>
        /// Error code.
        /// </summary>
        [DataMember]
        public StackHashServiceErrorCode ServiceErrorCode
        {
            get
            {
                return m_ServiceErrorCode;
            }
            set
            {
                m_ServiceErrorCode = value;
            }
        }

        /// <summary>
        /// True - the task can be aborted at the client. False cannot be aborted by the client.
        /// </summary>
        [DataMember]
        public bool CanBeAbortedByClient
        {
            get
            {
                return m_CanBeAbortedByClient;
            }
            set
            {
                m_CanBeAbortedByClient = value;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append("TaskType=");
            result.Append(m_TaskType.ToString());

            result.Append(", TaskState=");
            result.Append(m_TaskState.ToString());

            result.Append(", LastSuccessfulRunTimeUtc=");
            if (m_LastSuccessfulRunTimeUtc != null)
                result.Append(m_LastSuccessfulRunTimeUtc.ToString());
            else
                result.Append("null");

            result.Append(", LastFailedRunTimeUtc=");
            if (m_LastFailedRunTimeUtc != null)
                result.Append(m_LastFailedRunTimeUtc.ToString());
            else
                result.Append("null");

            result.Append(", LastStartedTimeUtc=");
            if (m_LastStartedTimeUtc != null)
                result.Append(m_LastStartedTimeUtc.ToString());
            else
                result.Append("null");

            result.Append(", LastDurationInSeconds=");
            result.Append(m_LastDurationInSeconds.ToString(CultureInfo.InvariantCulture));

            result.Append(", RunCount=");
            result.Append(m_RunCount.ToString(CultureInfo.InvariantCulture));

            result.Append(", SuccessCount=");
            result.Append(m_SuccessCount.ToString(CultureInfo.InvariantCulture));

            result.Append(", FailedCount=");
            result.Append(m_FailedCount.ToString(CultureInfo.InvariantCulture));

            result.Append(", ServiceErrorCode=");
            result.Append(m_ServiceErrorCode.ToString());

            result.Append(", CanBeAbortedByClient=");
            result.Append(m_CanBeAbortedByClient.ToString());

            result.Append(", LastException=");
            if (m_LastException != null)
                result.Append(m_LastException.ToString());
            else
                result.Append("null");

            return result.ToString();
        }

        #region IComparable<StackHashTaskStatus> Members

        public int CompareTo(StackHashTaskStatus other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (this.FailedCount != other.FailedCount)
                return -1;
            if (this.LastDurationInSeconds != other.LastDurationInSeconds)
                return -1;
            if (this.LastException != other.LastException)
                return -1;
            if (this.LastFailedRunTimeUtc.RoundToPreviousSecond() != other.LastFailedRunTimeUtc.RoundToPreviousSecond())
                return -1;
            if (this.LastStartedTimeUtc.RoundToPreviousSecond() != other.LastStartedTimeUtc.RoundToPreviousSecond())
                return -1;
            if (this.LastSuccessfulRunTimeUtc.RoundToPreviousSecond() != other.LastSuccessfulRunTimeUtc.RoundToPreviousSecond())
                return -1;
            if (this.RunCount != other.RunCount)
                return -1;
            if (this.ServiceErrorCode != other.ServiceErrorCode)
                return -1;
            if (this.SuccessCount != other.SuccessCount)
                return -1;
            if (this.TaskState != other.TaskState)
                return -1;
            if (this.TaskType != other.TaskType)
                return -1;
            if (this.CanBeAbortedByClient != other.CanBeAbortedByClient)
                return -1;
            return 0;
        }

        #endregion
    }

        
    /// <summary>
    /// A collection task status.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashTaskStatusCollection : Collection<StackHashTaskStatus>
    {
        public StackHashTaskStatusCollection() { ; } // Required for serialization.
    }


    /// <summary>
    /// Identifies the status last sync times for products in a context.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSynchronizationInfo
    {
        private int m_ProductId;
        private DateTime m_LastSyncTime;
        private DateTime m_LastHitTime;
        private DateTime m_LastSyncCompletedTime;
        private DateTime m_LastSyncStartedTime;

        public StackHashSynchronizationInfo() {;} // Required for serialization.

        public StackHashSynchronizationInfo(int productId, DateTime lastSyncTime, DateTime lastHitTime, DateTime lastSyncCompletedTime, DateTime lastSyncStartedTime)
        {
            m_ProductId = productId;
            m_LastSyncTime = lastSyncTime;
            m_LastHitTime = lastHitTime;
            m_LastSyncCompletedTime = lastSyncCompletedTime;
            m_LastSyncStartedTime = lastSyncStartedTime;
        }

        /// <summary>
        /// Product to which the information refers.
        /// </summary>
        [DataMember]
        public int ProductId
        {
            get
            {
                return m_ProductId;
            }
            set
            {
                m_ProductId = value;
            }
        }

        /// <summary>
        /// Last sync time for the specified product was started.
        /// </summary>
        [DataMember]
        public DateTime LastSyncTime
        {
            get
            {
                return m_LastSyncTime;
            }
            set
            {
                m_LastSyncTime = value;
            }
        }

        /// <summary>
        /// Last hit time for the specified product.
        /// The most recent event info hit time found for this product.
        /// Resyncs are based on this value.
        /// </summary>
        [DataMember]
        public DateTime LastHitTime
        {
            get
            {
                return m_LastHitTime;
            }
            set
            {
                m_LastHitTime = value;
            }
        }

        /// <summary>
        /// Last sync time for the specified product was completed
        /// </summary>
        [DataMember]
        public DateTime LastSyncCompletedTime
        {
            get
            {
                return m_LastSyncCompletedTime;
            }
            set
            {
                m_LastSyncCompletedTime = value;
            }
        }

        /// <summary>
        /// Last sync time for the specified product was started
        /// </summary>
        [DataMember]
        public DateTime LastSyncStartedTime
        {
            get
            {
                return m_LastSyncStartedTime;
            }
            set
            {
                m_LastSyncStartedTime = value;
            }
        }
    }

    /// <summary>
    /// A collection synchronization info.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashSynchronizationInfoCollection : Collection<StackHashSynchronizationInfo>
    {
        private int m_SyncCount;
        private StackHashSyncProgress m_SyncProgress;

        public StackHashSynchronizationInfoCollection() { ; } // Required for serialization.

        /// <summary>
        /// Number of syncs performed since the last resync.
        /// </summary>
        [DataMember]
        public int SyncCount
        {
            get
            {
                return m_SyncCount;
            }
            set
            {
                m_SyncCount = value;
            }
        }

        /// <summary>
        /// Indicates how far the previous sync got before completing.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get
            {
                return m_SyncProgress;
            }

            set
            {
                m_SyncProgress = value;
            }
        }
    }


    /// <summary>
    /// Gives information regarding the condition of a context.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashContextStatus
    {
        private int m_ContextId;
        private String m_ContextName;
        private StackHashTaskStatusCollection m_TaskStatusCollection;
        private bool m_IsActive;
        private bool m_LastSynchronizationLogOnFailed;
        private String m_LastSynchronizationLogOnException;
        private StackHashServiceErrorCode m_LastSynchronizationLogOnServiceError;
        private StackHashServiceErrorCode m_CurrentError;
        private String m_LastContextException;
        private StackHashBugTrackerPlugInDiagnosticsCollection m_PlugInDiagnostics;

        /// <summary>
        /// ID of the context.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get
            {
                return m_ContextId;
            }
            set
            {
                m_ContextId = value;
            }
        }

        /// <summary>
        /// Name of the context.
        /// </summary>
        [DataMember]
        public String ContextName
        {
            get
            {
                return m_ContextName;
            }
            set
            {
                m_ContextName = value;
            }
        }

        
        /// <summary>
        /// The list of status for each task in the context.
        /// </summary>
        [DataMember]
        public StackHashTaskStatusCollection TaskStatusCollection
        {
            get
            {
                return m_TaskStatusCollection;
            }
            set
            {
                m_TaskStatusCollection = value;
            }
        }
        /// <summary>
        /// True - the context is active. False otherwise.
        /// </summary>
        [DataMember]
        public bool IsActive
        {
            get
            {
                return m_IsActive;
            }
            set
            {
                m_IsActive = value;
            }
        }
        /// <summary>
        /// True - last winqual sync login was successful - false if failed.
        /// </summary>
        [DataMember]
        public bool LastSynchronizationLogOnFailed
        {
            get
            {
                return m_LastSynchronizationLogOnFailed;
            }
            set
            {
                m_LastSynchronizationLogOnFailed = value;
            }
        }

        /// <summary>
        /// Only valid if LastSyncLoginFailed is true.
        /// Contains the last exception for the login activity.
        /// </summary>
        [DataMember]
        public String LastSynchronizationLogOnException
        {
            get
            {
                return m_LastSynchronizationLogOnException;
            }
            set
            {
                m_LastSynchronizationLogOnException = value;
            }
        }


        /// <summary>
        /// Last error code for a logon from the WinQualSync service - not the logon task.
        /// </summary>
        [DataMember]
        public StackHashServiceErrorCode LastSynchronizationLogOnServiceError
        {
            get
            {
                return m_LastSynchronizationLogOnServiceError;
            }
            set
            {
                m_LastSynchronizationLogOnServiceError = value;
            }
        }

        /// <summary>
        /// The current state of the context.
        /// </summary>
        [DataMember]
        public StackHashServiceErrorCode CurrentError
        {
            get
            {
                return m_CurrentError;
            }
            set
            {
                m_CurrentError = value;
            }
        }


        /// <summary>
        /// Last exception that occurred during activation.
        /// </summary>
        [DataMember]
        public String LastContextException
        {
            get
            {
                return m_LastContextException;
            }
            set
            {
                m_LastContextException = value;
            }
        }

        /// <summary>
        /// Plug-in diagnostics for this context.
        /// </summary>
        [DataMember]
        public StackHashBugTrackerPlugInDiagnosticsCollection PlugInDiagnostics
        {
            get
            {
                return m_PlugInDiagnostics;
            }
            set
            {
                m_PlugInDiagnostics = value;
            }
        }
    }

    /// <summary>
    /// A collection task status.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashContextStatusCollection : Collection<StackHashContextStatus>
    {
        public StackHashContextStatusCollection() { ; } // Required for serialization.
    }



    /// <summary>
    /// Gives information regarding the condition of a context.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashStatus
    {
        private StackHashContextStatusCollection m_ContextStatusCollection;
        private bool m_HostRunningInTestMode;
        private bool m_InitializationFailed;

        /// <summary>
        /// List of context status values.
        /// </summary>
        [DataMember]
        public StackHashContextStatusCollection ContextStatusCollection
        {
            get
            {
                return m_ContextStatusCollection;
            }
            set
            {
                m_ContextStatusCollection = value;
            }
        }

        /// <summary>
        /// Indicates if the host is running in test mode or not.
        /// If false then this is a production run.
        /// </summary>
        [DataMember]
        public bool HostRunningInTestMode
        {
            get
            {
                return m_HostRunningInTestMode;
            }
            set
            {
                m_HostRunningInTestMode = value;
            }
        }

        /// <summary>
        /// Indicates if initialization failed.
        /// </summary>
        [DataMember]
        public bool InitializationFailed
        {
            get
            {
                return m_InitializationFailed;
            }
            set
            {
                m_InitializationFailed = value;
            }
        }
    }
}
