using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashErrorIndex
{
    [Serializable]
    public class ErrorIndexXmlSettings 
    {
        private string m_FileName;
        private StackHashSynchronizationInfoCollection m_LastSyncTimes;
        private StackHashTaskStatusCollection m_TaskStatistics;
        private bool m_Initialized;

        [NonSerialized]
        private static XmlSerializer s_ErrorIndexSettingsSerializer;

        public ErrorIndexXmlSettings() 
        {
            m_LastSyncTimes = new StackHashSynchronizationInfoCollection();
            m_TaskStatistics = new StackHashTaskStatusCollection();
        } 

        public ErrorIndexXmlSettings(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            m_FileName = fileName;

            m_LastSyncTimes = new StackHashSynchronizationInfoCollection();
            m_TaskStatistics = new StackHashTaskStatusCollection();

            // Construct an XmlFormatter and use it to serialize the data to the file.

            if (s_ErrorIndexSettingsSerializer == null)
            {
                s_ErrorIndexSettingsSerializer = new XmlSerializer(
                    typeof(ErrorIndexXmlSettings),
                    new Type[] { typeof(ErrorIndexXmlSettings), typeof(StackHashSynchronizationInfoCollection), typeof(StackHashSynchronizationInfo),
                    typeof(StackHashTaskStatusCollection), typeof(StackHashTaskStatus), typeof(StackHashTaskState), typeof(StackHashTaskType), 
                    typeof(StackHashServiceErrorCode), typeof(DateTime), typeof(StackHashSyncProgress), typeof(StackHashSyncPhase)});
            }

            Load();
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashSynchronizationInfoCollection LastSyncTimes
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LastSyncTimes;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            set 
            { 
                Monitor.Enter(this);

                try
                {
                    m_LastSyncTimes = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        public string FileName
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_FileName;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            set
            {
                Monitor.Enter(this);

                try
                {
                    m_FileName = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
//        [SuppressMessage("Microsoft.Naming", "CA1721")]
        public StackHashTaskStatusCollection TaskData
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_TaskStatistics;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            set 
            { 
                Monitor.Enter(this);

                try
                {
                    m_TaskStatistics = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }


        public DateTime GetLastSyncTime(int productId)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                        return syncInfo.LastSyncTime;
                }
                return new DateTime(0, DateTimeKind.Local);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetLastSyncTime(int productId, DateTime lastSyncTime)
        {
            Monitor.Enter(this);

            try
            {
                bool found = false;
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                    {
                        syncInfo.LastSyncTime = lastSyncTime;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // No product was found matching the ID so add a new one.
                    m_LastSyncTimes.Add(new StackHashSynchronizationInfo(productId, lastSyncTime, new DateTime(0), new DateTime(0), new DateTime(0)));
                }

                // Save the result.
                Save();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public DateTime GetLastSyncCompletedTime(int productId)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                        return syncInfo.LastSyncCompletedTime;
                }
                return new DateTime(0, DateTimeKind.Local);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetLastSyncCompletedTime(int productId, DateTime lastSyncTime)
        {
            Monitor.Enter(this);

            try
            {
                bool found = false;
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                    {
                        syncInfo.LastSyncCompletedTime = lastSyncTime;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // No product was found matching the ID so add a new one.
                    m_LastSyncTimes.Add(new StackHashSynchronizationInfo(productId, new DateTime(0), new DateTime(0), lastSyncTime, new DateTime(0)));
                }

                // Save the result.
                Save();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public DateTime GetLastSyncStartedTime(int productId)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                        return syncInfo.LastSyncStartedTime;
                }
                return new DateTime(0, DateTimeKind.Local);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetLastSyncStartedTime(int productId, DateTime lastSyncTime)
        {
            Monitor.Enter(this);

            try
            {
                bool found = false;
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                    {
                        syncInfo.LastSyncStartedTime = lastSyncTime;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // No product was found matching the ID so add a new one.
                    m_LastSyncTimes.Add(new StackHashSynchronizationInfo(productId, new DateTime(0), new DateTime(0), new DateTime(0), lastSyncTime));
                }

                // Save the result.
                Save();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public DateTime GetLastHitTime(int productId)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                        return syncInfo.LastHitTime;
                }
                return new DateTime(0, DateTimeKind.Local);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetLastHitTime(int productId, DateTime lastHitTime)
        {
            Monitor.Enter(this);

            try
            {
                bool found = false;
                foreach (StackHashSynchronizationInfo syncInfo in m_LastSyncTimes)
                {
                    if (syncInfo.ProductId == productId)
                    {
                        syncInfo.LastHitTime = lastHitTime;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // No product was found matching the ID so add a new one.
                    m_LastSyncTimes.Add(new StackHashSynchronizationInfo(productId, new DateTime(0), lastHitTime, new DateTime(0), new DateTime(0)));
                }

                // Save the result.
                Save();
            }
            finally
            {
                Monitor.Exit(this);
            }

        }


        /// <summary>
        /// Number of times that a sync has taken place since the last full resync.
        /// </summary>
        public int SyncCount
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    if (m_LastSyncTimes != null)
                        return m_LastSyncTimes.SyncCount;
                    else
                        return 0;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);

                try
                {
                    if (m_LastSyncTimes != null)
                        m_LastSyncTimes.SyncCount = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Indicates how far the previous sync got before completing.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    if (m_LastSyncTimes != null)
                        return m_LastSyncTimes.SyncProgress;
                    else
                        return new StackHashSyncProgress();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);

                try
                {
                    if (m_LastSyncTimes != null)
                        m_LastSyncTimes.SyncProgress = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        
        public StackHashTaskStatus GetTaskStatistics(StackHashTaskType taskType)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashTaskStatus taskStatus in m_TaskStatistics)
                {
                    if (taskStatus.TaskType == taskType)
                        return taskStatus;
                }

                // Not found so create a default.
                StackHashTaskStatus defaultTaskStatus = new StackHashTaskStatus();

                defaultTaskStatus.FailedCount = 0;
                defaultTaskStatus.LastDurationInSeconds = 0;
                defaultTaskStatus.LastException = null;
                defaultTaskStatus.LastFailedRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
                defaultTaskStatus.LastSuccessfulRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
                defaultTaskStatus.LastStartedTimeUtc = new DateTime(0, DateTimeKind.Utc);
                defaultTaskStatus.RunCount = 0;
                defaultTaskStatus.SuccessCount = 0;
                defaultTaskStatus.TaskState = StackHashTaskState.NotRunning;
                defaultTaskStatus.TaskType = taskType;
                defaultTaskStatus.ServiceErrorCode = StackHashServiceErrorCode.NoError;

                return defaultTaskStatus;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetTaskStatistics(StackHashTaskStatus newTaskStatus)
        {
            if (newTaskStatus == null)
                throw new ArgumentNullException("newTaskStatus");

            Monitor.Enter(this);

            try
            {
                bool found = false;
                foreach (StackHashTaskStatus taskStatus in m_TaskStatistics)
                {
                    if (taskStatus.TaskType == newTaskStatus.TaskType)
                    {
                        taskStatus.TaskType = newTaskStatus.TaskType;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // Not currently known so add it.
                    m_TaskStatistics.Add(newTaskStatus);
                }

                // Save the result.
                Save();
            }
            finally
            {
                Monitor.Exit(this);
            }

        }
       
        /// <summary>
        /// Save the error index settings.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        public void Save()
        {
            Monitor.Enter(this);

            if (!m_Initialized)
                return;

            // Simply serializes the specified data to an XML file.
            FileStream xmlFile = null;

            try
            {
                xmlFile = new FileStream(m_FileName, FileMode.Create, FileAccess.Write);
 
                s_ErrorIndexSettingsSerializer.Serialize(xmlFile, this);
            }
            finally
            {
                if (xmlFile != null)
                {
                    xmlFile.Flush();
                    xmlFile.Close();
                }
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Load the settings.
        /// </summary>
        public void Load()
        {
            Monitor.Enter(this);

            bool saveRequired = false;

            try
            {
                if (File.Exists(m_FileName))
                {

                    // Simply serializes the specified data to an XML file.
                    FileStream xmlFile = new FileStream(m_FileName, FileMode.Open, FileAccess.Read);

                    try
                    {
                        ErrorIndexXmlSettings settings = s_ErrorIndexSettingsSerializer.Deserialize(xmlFile) as ErrorIndexXmlSettings;

                        // Copy the relevant settings.
                        if (settings.LastSyncTimes != null)
                            m_LastSyncTimes = settings.LastSyncTimes;

                        if (m_TaskStatistics != null)
                            m_TaskStatistics = settings.TaskData;
                    }
                    finally
                    {
                        xmlFile.Close();
                    }
                }
                else
                {
                    // Default the settings.
                    m_LastSyncTimes = new StackHashSynchronizationInfoCollection();
                    m_TaskStatistics = new StackHashTaskStatusCollection();
                    saveRequired = true;
                }

                // Data now initialized.
                m_Initialized = true;

                if (saveRequired)
                    Save();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }

    public class XmlErrorIndex : IErrorIndex, IDisposable
    {
        private const string s_ErrorIndexSettingsFileName = "Settings.xml";

        private const string s_ProductFolderPrefix = "P_";
        private const string s_FileFolderPrefix = "F_";
        private const string s_EventFolderPrefix = "E_";
        private const string s_EventInfoFileNamePrefix = "EI_";
        private const string s_CabFolderPrefix = "CAB_";

        private const string s_ProductFileName = "Product.xml";
        private const string s_FileFileName = "File.xml";
        private const string s_EventFileName = "Event.xml";
        private const string s_EventInfoFileName = "EventInfo.xml";
        private const string s_EventNotesFileName = "EventNotes.xml";
        private const string s_CabInfoFileName = "CabInfo.xml";
        private const string s_CabNotesFileName = "CabNotes.xml";

        private const string s_DefaultErrorIndexPath = "c:\\StackHashDefaultErrorIndex";
        private const string s_DefaultErrorIndexName = "AcmeErrorIndex";
        private bool m_IsActive;
        private bool m_UpdateTableActive;
        private bool m_MoveInProgress;

        private string m_ErrorIndexPath;
        private string m_ErrorIndexName;
        private string m_ErrorIndexRoot;

        private ErrorIndexXmlSettings m_ErrorIndexSettings;
        private bool m_AbortRequested;

        private XmlSerializer m_ProductSerializer;
        private XmlSerializer m_FileSerializer;
        private XmlSerializer m_EventSerializer;
        private XmlSerializer m_EventInfoSerializer;
        private XmlSerializer m_CabSerializer;
        private XmlSerializer m_NoteSerializer;

        // Delegate to hear about changes to the event index.
        public event EventHandler<ErrorIndexEventArgs> IndexUpdated;
        public event EventHandler<ErrorIndexEventArgs> IndexUpdateAdded;
        public event EventHandler<ErrorIndexMoveEventArgs> IndexMoveProgress;

        /// <summary>
        /// Determines if the index is active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_IsActive;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Determines if changes should be logged to the Update table.
        /// </summary>
        public bool UpdateTableActive
        {
            get
            {
                return m_UpdateTableActive;
            }
            set
            {
                m_UpdateTableActive = value;
            }
        }

        public ErrorIndexType IndexType
        {
            get
            {
                return ErrorIndexType.Xml;
            }
        }


        /// <summary>
        /// Number of times that a sync has taken place since the last full resync.
        /// </summary>
        public int SyncCount
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    if (m_ErrorIndexSettings != null)
                        return m_ErrorIndexSettings.SyncCount;
                    else
                        return 0;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);

                try
                {
                    if (m_ErrorIndexSettings != null)
                        m_ErrorIndexSettings.SyncCount = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Indicates how far the previous sync got before completing.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    if (m_ErrorIndexSettings != null)
                        return m_ErrorIndexSettings.SyncProgress;
                    else
                        return new StackHashSyncProgress();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);

                try
                {
                    if (m_ErrorIndexSettings != null)
                        m_ErrorIndexSettings.SyncProgress = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        
        /// <summary>
        /// Notify upstream objects of a change to the error index.
        /// </summary>
        /// <param name="e">Identifies the change.</param>
        public void OnErrorIndexChanged(ErrorIndexEventArgs e)
        {
            EventHandler<ErrorIndexEventArgs> handler = IndexUpdated;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Notify upstream objects of a change to the error index Update table.
        /// </summary>
        /// <param name="e">Identifies the change.</param>
        public void OnErrorIndexUpdateAdded(ErrorIndexEventArgs e)
        {
            EventHandler<ErrorIndexEventArgs> handler = IndexUpdateAdded;

            if (handler != null)
                handler(this, e);
        }


        /// <summary>
        /// Notify upstream objects of progress during an index move.
        /// </summary>
        /// <param name="e">Identifies the progress.</param>
        public void OnErrorIndexMoveProgress(ErrorIndexMoveEventArgs e)
        {
            EventHandler<ErrorIndexMoveEventArgs> handler = IndexMoveProgress;

            if (handler != null)
                handler(this, e);
        }


        /// <summary>
        /// The XmlErrorIndex stores the WinQual information in a series of XML files at the 
        /// specified location. A directory structure is created as follows.
        /// 
        ///     Root - Product1 - File1 - Event1 - EventInfo1 - CabFile
        ///                                      - EventInfo2
        ///                                      - EventInfoN
        ///                             - Event2
        ///                             - EventN
        ///                     - File2 
        ///                     - FileN
        ///            Product2 
        ///            ProductN 
        ///            
        /// The ProductN folders contain a file called Product.XML.
        /// ProductN is "Product" + ProductName and Version strings.
        /// 
        /// The FileN folders contain a file called File.XML.
        /// FileN is "File" + Filename + Version strings.
        /// 
        /// The EventN folders contain a file called Event.XML.
        /// EventN is "Event" + Event name.
        ///     
        /// </summary>
        /// <param name="errorIndexPath"></param>
        /// <param name="errorIndexName"></param>

        [SuppressMessage("Microsoft.Performance", "CA1804")]
        public XmlErrorIndex(string errorIndexPath, string errorIndexName)
        {
            if (errorIndexPath == null)
                throw new ArgumentNullException("errorIndexPath");
            if (errorIndexName == null)
                throw new ArgumentNullException("errorIndexName");
            
            // The error index is placed in a subdirectory of the specified folder.
            m_ErrorIndexName = errorIndexName;
            if (!errorIndexPath.EndsWith("\\", true, CultureInfo.InstalledUICulture))
            {
                m_ErrorIndexPath = errorIndexPath + "\\" + m_ErrorIndexName;
                m_ErrorIndexRoot = errorIndexPath + "\\";
            }
            else
            {
                m_ErrorIndexPath = errorIndexPath + m_ErrorIndexName;
                m_ErrorIndexRoot = errorIndexPath;
            }

            if (Directory.Exists(m_ErrorIndexPath))
            {
                // Load the error index settings.
                string errorIndexSettingsFileName = getSettingsFileName(m_ErrorIndexPath);
                if (File.Exists(errorIndexSettingsFileName))
                    m_ErrorIndexSettings = new ErrorIndexXmlSettings(errorIndexSettingsFileName);
                else
                    m_ErrorIndexSettings = new ErrorIndexXmlSettings();
            }
            else
            {
                m_ErrorIndexSettings = new ErrorIndexXmlSettings();
            }


            // Create serialization objects so they only get created once.
            m_ProductSerializer = new XmlSerializer(typeof(StackHashProduct), new Type[] { typeof(StackHashProduct) });
            m_FileSerializer = new XmlSerializer(typeof(StackHashFile), new Type[] { typeof(StackHashFile) });
            m_EventSerializer = new XmlSerializer(typeof(StackHashEvent),
                new Type[] { typeof(StackHashEvent), typeof(StackHashEventSignature), 
                             typeof(StackHashParameterCollection), typeof(StackHashParameter) });
            m_EventInfoSerializer = new XmlSerializer(typeof(StackHashEventInfoCollection),
                new Type[] { typeof(StackHashEventInfoCollection) });
            m_CabSerializer = new XmlSerializer(typeof(StackHashCab), new Type[] { typeof(StackHashCab), typeof(StackHashDumpAnalysis) });
            m_NoteSerializer = new XmlSerializer(typeof(StackHashNotes), new Type[] { typeof(StackHashNotes), typeof(StackHashNoteEntry) });

        }

        #region IErrorIndex Members


        /// <summary>
        /// Get the last time the product was synchronized successfully with WinQual.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last successful sync time.</returns>
        public DateTime GetLastSyncTimeLocal(int productId)
        {
            return m_ErrorIndexSettings.GetLastSyncTime(productId);
        }

        /// <summary>
        /// Sets the last time the product was synchronized following a successfully sync with WinQual.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastSyncTime">The last time the product was successfully synced (GMT).</param>
        public void SetLastSyncTimeLocal(int productId, DateTime lastSyncTime)
        {
            m_ErrorIndexSettings.SetLastSyncTime(productId, lastSyncTime);
        }

        /// <summary>
        /// Get the last time the product was synchronized successfully with WinQual.
        /// This is the time the sync completed for this product.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last successful sync complete time.</returns>
        public DateTime GetLastSyncCompletedTimeLocal(int productId)
        {
            return m_ErrorIndexSettings.GetLastSyncCompletedTime(productId);
        }

        /// <summary>
        /// Sets the last time the product was synchronized following a successfully sync with WinQual.
        /// This is the time it completed.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastSyncTime">The last time sync for the product was successfully completed.</param>
        public void SetLastSyncCompletedTimeLocal(int productId, DateTime lastSyncTime)
        {
            m_ErrorIndexSettings.SetLastSyncCompletedTime(productId, lastSyncTime);
        }

        /// <summary>
        /// Get the last time the product was synchronized with WinQual.
        /// This is the time the sync started for this product.
        /// Note that the sync may have failed.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last sync start time.</returns>
        public DateTime GetLastSyncStartedTimeLocal(int productId)
        {
            return m_ErrorIndexSettings.GetLastSyncStartedTime(productId);
        }

        /// <summary>
        /// Sets the last time the product sync started for the product.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastSyncTime">The last time sync for the product was started.</param>
        public void SetLastSyncStartedTimeLocal(int productId, DateTime lastSyncTime)
        {
            m_ErrorIndexSettings.SetLastSyncStartedTime(productId, lastSyncTime);
        }

        /// <summary>
        /// Get the most recent (last) hit time for the product.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last hit time.</returns>
        public DateTime GetLastHitTimeLocal(int productId)
        {
            return m_ErrorIndexSettings.GetLastHitTime(productId);
        }


        /// <summary>
        /// Sets the most recent event info hit time for the specified product.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastHitTime">The most recent Hit time for that product.</param>
        public void SetLastHitTimeLocal(int productId, DateTime lastHitTime)
        {
            m_ErrorIndexSettings.SetLastHitTime(productId, lastHitTime);
        }

        /// <summary>
        /// Gets stats associated with the specified task type.
        /// Tasks are run on the data in the index. e.g. a Sync, Analyze, Purge etc...
        /// </summary>
        /// <param name="taskType">The task type whos stats is required.</param>
        /// <returns>Latest stored stats.</returns>
        public StackHashTaskStatus GetTaskStatistics(StackHashTaskType taskType)
        {
            return m_ErrorIndexSettings.GetTaskStatistics(taskType);
        }

        /// <summary>
        /// Sets stats associated with the specified task type.
        /// Tasks are run on the data in the index. e.g. a Sync, Analyze, Purge etc...
        /// </summary>
        /// <param name="taskStatus">The task status to set.</param>
        public void SetTaskStatistics(StackHashTaskStatus taskStatus)
        {
            m_ErrorIndexSettings.SetTaskStatistics(taskStatus);
        }

        public void Activate()
        {
            Activate(true, false);
        }

        /// <summary>
        /// Creates the index if necessary or initializes an existing one.
        /// Set allowIndexCreation for test mode only.
        /// </summary>
        /// <param name="allowIndexCreation">True - create the index if it doesn't exist, False - don't create.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void Activate(bool allowIndexCreation, bool createIndexInDefaultLocation)
        {
            if (!m_IsActive)
            {
                if (m_MoveInProgress)
                    throw new InvalidOperationException("Cannot active - error index move in progress");

                Monitor.Enter(this);

                try
                {
                    if (!Directory.Exists(m_ErrorIndexPath))
                        Directory.CreateDirectory(m_ErrorIndexPath);

                    // Load the error index settings.
                    string errorIndexSettingsFileName = getSettingsFileName(m_ErrorIndexPath);

                    // This will create and save defaults if necessary.
                    m_ErrorIndexSettings = new ErrorIndexXmlSettings(errorIndexSettingsFileName);

                    m_AbortRequested = false;
                    m_IsActive = true;
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    throw new StackHashException("Unable to access index folder. Ensure that the StackHash service account has access to the folder", ex, StackHashServiceErrorCode.ErrorIndexAccessDenied);
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }


        /// <summary>
        /// Unloads. 
        /// </summary>
        public void Deactivate()
        {
            m_IsActive = false;
        }


        /// <summary>
        /// Name of the index.
        /// </summary>
        public String ErrorIndexName
        {
            get { return m_ErrorIndexName; }
        }

        /// <summary>
        /// Path of the index.
        /// </summary>
        public String ErrorIndexPath
        {
            get { return m_ErrorIndexRoot; } 
        }

        public ErrorIndexStatus Status
        {
            get
            {
                // Determine if the index exists.
                if (!Directory.Exists(m_ErrorIndexPath))
                    return ErrorIndexStatus.NotCreated;
                String errorIndexSettingsFileName = getSettingsFileName(m_ErrorIndexPath);
                if (!File.Exists(errorIndexSettingsFileName))
                    return ErrorIndexStatus.NotCreated;

                return ErrorIndexStatus.Created;
            }
        }

        /// <summary>
        /// Performs tests on the the database and cab folders.
        /// </summary>
        /// <returns></returns>
        public ErrorIndexConnectionTestResults GetDatabaseStatus()
        {
            if (Status == ErrorIndexStatus.Created)
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.Success, null);
            else if (Status == ErrorIndexStatus.NotCreated)
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.DatabaseDoesNotExist, null);
            else
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.Unknown, null);
        }


        public static String DefaultErrorIndexPath
        {
            get
            {
                return s_DefaultErrorIndexPath;
            }
        }

        public static String DefaultErrorIndexName
        {
            get
            {
                return s_DefaultErrorIndexName;
            }
        }

        /// <summary>
        /// Loads the product information from the specified file.
        /// </summary>
        /// <param name="productFileName">File containing the product information.</param>
        /// <returns>Product data loaded.</returns>
        private StackHashProduct loadProduct(string productFileName)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            // Simply serializes the specified data to an XML file.
            FileStream xmlFile = new FileStream(productFileName, FileMode.Open, FileAccess.Read);

            try
            {
                StackHashProduct product = m_ProductSerializer.Deserialize(xmlFile) as StackHashProduct;

                return product;
            }
            finally
            {
                xmlFile.Close();
            }
        }


        public long TotalStoredEvents
        {
            get
            {
                long totalStoredEvents = 0;
                StackHashProductCollection products = LoadProductList();
                foreach (StackHashProduct product in products)
                {
                    totalStoredEvents += product.TotalStoredEvents;
                }

                return totalStoredEvents;
            }
        }

        /// <summary>
        /// Total products in the index.
        /// </summary>
        public long TotalProducts
        {
            get
            {
                StackHashProductCollection products = LoadProductList();

                return products.Count();
            }
        }


        /// <summary>
        /// Total files in the index - across all products.
        /// </summary>
        public long TotalFiles
        {
            get
            {
                long totalFiles = 0;
                StackHashProductCollection products = LoadProductList();
                foreach (StackHashProduct product in products)
                {
                    StackHashFileCollection files = LoadFileList(product);
                    totalFiles += files.Count;
                }

                return totalFiles;
            }
        }

        /// <summary>
        /// Loads information about all products in the database.
        /// The product.xml files are loaded in each p_* subfolder.
        /// </summary>
        /// <returns>Full product list</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public StackHashProductCollection LoadProductList()
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                StackHashProductCollection productCollection = new StackHashProductCollection();

                // Find all of the product subfolders;
                string[] subfolders = Directory.GetDirectories(m_ErrorIndexPath, s_ProductFolderPrefix + "*");

                string productFileName = null;

                foreach (string folder in subfolders)
                {
                    productFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", folder, s_ProductFileName);

                    if (File.Exists(productFileName))
                    {
                        try
                        {
                            StackHashProduct product = loadProduct(productFileName);

                            if (product.StructureVersion < 2)
                            {
                                product.StructureVersion = StackHashProduct.ThisStructureVersion;
                                product.TotalStoredEvents = getTotalProductEvents(product);
                                AddProduct(product, true);
                            }

                            productCollection.Add(product);
                        }
                        catch (System.Exception ex)
                        {
                            // Just log and ignore.
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load product file " + productFileName, ex);
                        }
                    }
                }

                return productCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets data associated with the specified product.
        /// </summary>
        /// <param name="productId">Id of the product whose data is required.</param>
        /// <returns>Product data or null if not found.</returns>

        public StackHashProduct GetProduct(int productId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                // Find all of the product subfolders matching the specified ID - should only be 1.
                string[] subfolders = Directory.GetDirectories(m_ErrorIndexPath, getProductFolderWildcard(productId));

                if (subfolders.Length == 0)
                    return null;

                if (subfolders.Length > 1)
                    throw new InvalidOperationException("2 products found with same ID");

                string productFileName = string.Format(CultureInfo.InstalledUICulture, "{0}\\{1}",
                    subfolders[0], s_ProductFileName);

                // Load the requested product information.
                StackHashProduct product = loadProduct(productFileName);

                return product;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Adds a new product to the database. 
        /// The product is not added if it already exists.
        /// </summary>
        /// <param name="product"></param>
        /// <returns>Updated product.</returns>
        public StackHashProduct AddProduct(StackHashProduct product)
        {
            return AddProduct(product, false);
        }


        /// <summary>
        /// Adds a new product to the database. 
        /// The product is not added if it already exists.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="updateNonWinQualFields">True updates all fields. False - updates just winqual fields.</param>
        /// <returns>Updated product.</returns>
        public StackHashProduct AddProduct(StackHashProduct product, bool updateNonWinQualFields)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");

                bool productExists = ProductExists(product);

                if (productExists && !updateNonWinQualFields)
                {
                    // Read in the original product.
                    StackHashProduct originalProduct = GetProduct(product.Id);

                    product.TotalStoredEvents = originalProduct.TotalStoredEvents;
                }


                // Get the product XML filename.
                string productPath = string.Format(CultureInfo.InvariantCulture, 
                    "{0}\\{1}", m_ErrorIndexPath, GetProductFolder(product));
                string productPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", productPath, "product.xml");

                if (!Directory.Exists(productPath))
                    Directory.CreateDirectory(productPath);

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(productPathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_ProductSerializer.Serialize(xmlFile, product);
                }
                finally
                {
                    xmlFile.Close();
                }

                if (!productExists)
                    OnErrorIndexChanged(new ErrorIndexEventArgs(
                        new StackHashBugTrackerUpdate(StackHashDataChanged.Product, StackHashChangeType.NewEntry, product.Id, 0, 0, null, 0, product.Id)));
                return product;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Updates the product information. 
        /// If the product does not exist, a new product is added.
        /// </summary>
        /// <param name="product"></param>
        public void UpdateProduct(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                AddProduct(product);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Check if the specified file exists in the database.
        /// </summary>
        /// <param name="product">Product to which the file belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>

        public bool FileExists(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");


            Monitor.Enter(this);

            try
            {
                string filePath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file));

                string filePathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", filePath, "file.xml");

                return (File.Exists(filePathAndFilename));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Returns the cab with the specified ID.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cabId">ID of cab to get.</param>
        /// <returns>The cab or null if not found.</returns>
        public StackHashCab GetCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int cabId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                StackHashCabCollection cabList = LoadCabList(product, file, theEvent);
                return cabList.FindCab(cabId);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        
        
        /// <summary>
        /// Check if the specified cab exists in the database.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cab">The cab to search for.</param>
        public bool CabExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            Monitor.Enter(this);

            try
            {
                StackHashCabCollection cabList = LoadCabList(product, file, theEvent);

                return (cabList.FindCab(cab.Id) != null);
            }
            finally
            {
                Monitor.Exit(this);
            }

        }

        /// <summary>
        /// Check if the specified cab file exists in the database.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cab">The cab to search for.</param>
        public bool CabFileExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            Monitor.Enter(this);

            try
            {
                String cabFileName = GetCabFileName(product, file, theEvent, cab);
                return (Directory.Exists(Path.GetDirectoryName(cabFileName)) && File.Exists(cabFileName));
            }
            finally
            {
                Monitor.Exit(this);
            }

        }

        /// <summary>
        /// Adds a new file to the database. The file is not added if it already 
        /// exists.
        /// </summary>
        /// <param name="product">Product to add file to.</param>
        /// <param name="file">File to add.</param>
        public void AddFile(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");

                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");
                
                if (file == null)
                    throw new ArgumentNullException("file");

                if (file.Id == 0)
                    throw new ArgumentException("File object not initialised", "file");

                // Get the product XML filename.
                string filePath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", 
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file));
                string filePathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", filePath, "file.xml");

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(filePathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_FileSerializer.Serialize(xmlFile, file);
                }
                finally
                {
                    xmlFile.Close();
                }
                OnErrorIndexChanged(new ErrorIndexEventArgs(
                    new StackHashBugTrackerUpdate(StackHashDataChanged.File, StackHashChangeType.NewEntry, product.Id, file.Id, 0, null, 0, file.Id)));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Updates the file record. If the file does not exists a new file record
        /// is added.
        /// </summary>
        /// <param name="product">Product to update file in.</param>
        /// <param name="file">File to update.</param>
        public void UpdateFile(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                AddFile(product, file);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets a list of all file folders for the specified product.
        /// </summary>
        /// <param name="product">Product to get files for.</param>
        /// <returns>List of file folders in the product.</returns>
        private String[] getFileFolders(StackHashProduct product)
        {
            // Find a folder with the specified ID in the name.
            String fileFolder = string.Format(CultureInfo.InvariantCulture,
                "{0}\\{1}", m_ErrorIndexPath, GetProductFolder(product));

            // Find all of the file subfolders;
            String fileFolderWildCard = getFileFolderWildcard(0);
            String[] files = Directory.GetDirectories(fileFolder, fileFolderWildCard);
            return files;
        }


        /// <summary>
        /// Gets data associated with the specified file.
        /// </summary>
        /// <param name="product">Product whose data is required.</param>
        /// <param name="fileId">ID of file to retrieve.</param>
        /// <returns>File data or null if not found.</returns>

        public StackHashFile GetFile(StackHashProduct product, int fileId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");

                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                // Find a folder with the specified ID in the name.
                string fileFolder = string.Format(CultureInfo.InvariantCulture,
                    "{0}\\{1}", m_ErrorIndexPath, GetProductFolder(product));

                // Find all of the file subfolders;
                string fileFolderWildCard = getFileFolderWildcard(fileId);
                string[] files = Directory.GetDirectories(fileFolder, fileFolderWildCard);

                if (files.Length == 0)
                    return null;

                if (files.Length > 1)
                    throw new InvalidOperationException("2 file folders found with same ID");

                string fileName = string.Format(CultureInfo.InstalledUICulture, "{0}\\{1}", files[0], s_FileFileName);

                // Load the requested file information.
                StackHashFile file = loadFile(fileName);

                return file;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Determines if the specified product exists in the database.
        /// </summary>
        /// <param name="product">Product to find</param>
        /// <returns>true - product exists, false - product does not exist.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification="Not all implementations of this I/F method will be static")]
        public bool ProductExists(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                string productFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                    m_ErrorIndexPath, GetProductFolder(product));
                return (Directory.Exists(productFolder));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }



        /// <summary>
        /// Loads information about a specified file from the specified XML file.
        /// </summary>
        /// <param name="fileName">Name of settings file containing file data.</param>
        /// <returns>File data</returns>

        private StackHashFile loadFile(string fileName)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (File.Exists(fileName))
            {
                // Simply deserializes the specified data from the XML file.
                FileStream xmlFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                try
                {
                    // Construct an XmlFormatter and use it to serialize the data to the file.
                    StackHashFile file = m_FileSerializer.Deserialize(xmlFile) as StackHashFile;
                    return file;
                }
                finally
                {
                    xmlFile.Close();
                }
            }

            return null;
        }


        /// <summary>
        /// Loads information about all files associated with a specific product.
        /// The file.xml files are loaded in each F_* subfolder.
        /// </summary>
        /// <returns>Full file list</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public StackHashFileCollection LoadFileList(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");

                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                StackHashFileCollection fileCollection = new StackHashFileCollection();

                // Find all of the product subfolders;
                string rootFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", 
                    m_ErrorIndexPath, GetProductFolder(product));

                string[] subfolders = Directory.GetDirectories(rootFolder, s_FileFolderPrefix + "*");

                foreach (string folder in subfolders)
                {
                    string fileFilename = null;

                    fileFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", folder, s_FileFileName);

                    if (File.Exists(fileFilename))
                    {
                        // Simply deserializes the specified data from the XML file.
                        FileStream xmlFile = new FileStream(fileFilename, FileMode.Open, FileAccess.Read);

                        try
                        {
                            // Construct an XmlFormatter and use it to serialize the data to the file.
                            StackHashFile file = m_FileSerializer.Deserialize(xmlFile) as StackHashFile;
                            fileCollection.Add(file);
                        }
                        catch (System.Exception ex)
                        {
                            // Just log and ignore.
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load file " + fileFilename, ex);
                        }

                        finally
                        {
                            xmlFile.Close();
                        }
                    }
                }

                return fileCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Check if the specified event exists in the database.
        /// </summary>
        /// <param name="product">Product to which the file belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="event">Event to look for.</param>

        public bool EventExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (file == null)
                    throw new ArgumentNullException("file");
                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");

                string eventPath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent));
                string eventPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", eventPath, s_EventFileName);

                return (File.Exists(eventPathAndFilename));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Adds a new event to the database. The event is replaced if it already exists.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to add.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            AddEvent(product, file, theEvent, false);
        }

        /// <summary>
        /// Adds an event associated with the specified file in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields)
        {
            AddEvent(product, file, theEvent, updateNonWinQualFields, true);
        }


        /// <summary>
        /// Adds a new event to the database. The event is replaced if it already exists.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields, bool reportToBugTrackers)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");

                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                if (file == null)
                    throw new ArgumentNullException("file");

                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exist", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");

                bool eventExists = EventExists(product, file, theEvent);

                // Get the product XML filename.
                string eventPath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}", 
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent));
                string eventPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", eventPath, s_EventFileName);

                if (!Directory.Exists(eventPath))
                    Directory.CreateDirectory(eventPath);

                if (!updateNonWinQualFields)
                {
                    if (File.Exists(eventPathAndFilename))
                    {
                        StackHashEvent existingEvent = GetEvent(product, file, theEvent);
                        existingEvent.SetWinQualFields(theEvent);
                        theEvent = existingEvent;
                    }
                }

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(eventPathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_EventSerializer.Serialize(xmlFile, theEvent);
                }
                finally
                {
                    xmlFile.Close();
                }

                if (!eventExists)
                {
                    // New event so update the product stats.
                    StackHashProduct existingProduct = GetProduct(product.Id);
                    existingProduct.TotalStoredEvents++;
                    AddProduct(existingProduct, true);
                }

                OnErrorIndexChanged(new ErrorIndexEventArgs(
                    new StackHashBugTrackerUpdate(StackHashDataChanged.Event, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, 0, theEvent.Id)));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Updates the event. If the event does not exist a new one is added.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        public void UpdateEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                AddEvent(product, file, theEvent);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Calculates the total hits for the event from the combined EventInfo hit counts.
        /// </summary>
        /// <param name="product">The product owning the event.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to calculate the hits for.</param>
        private void updateHitCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            // Load the event list.
            StackHashEventInfoCollection eventInfos = LoadEventInfoList(product, file, theEvent);

            int totalHits = 0;
            foreach (StackHashEventInfo eventInfo in eventInfos)
            {
                totalHits += eventInfo.TotalHits;
            }

            theEvent.TotalHits = totalHits;
        }


        /// <summary>
        /// Gets the specified event data from file.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to refresh.</param>
        /// <returns>The refreshed event data.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public StackHashEvent GetEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            StackHashEvent newEvent = null;

            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");
            Monitor.Enter(this);
            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");

                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                if (file == null)
                    throw new ArgumentNullException("file");

                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exist", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");

                // Get the event XML filename.
                string eventPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}\\{4}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent), s_EventFileName);

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(eventPathAndFilename, FileMode.Open, FileAccess.Read);

                try
                {
                    newEvent = m_EventSerializer.Deserialize(xmlFile) as StackHashEvent;
                }
                catch (System.Exception ex)
                {
                    // Just log and ignore.
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load event " + eventPathAndFilename, ex);
                }                                            
                finally
                {
                    xmlFile.Close();
                }

                // Check if the hit count needs updating.
                if ((newEvent.TotalHits == 0) || (theEvent.TotalHits == -1))
                {
                    updateHitCount(product, file, newEvent);
                }

                return newEvent;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private StackHashEvent loadEvent(StackHashProduct product, StackHashFile file, String fileName)
        {
            StackHashEvent theEvent = null;
            bool updated = false;

            if (File.Exists(fileName))
            {
                // Simply deserializes the specified data from the XML file.
                FileStream xmlFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                try
                {
                    theEvent = m_EventSerializer.Deserialize(xmlFile) as StackHashEvent;

                    // The event signature is primarily a list of parameters - some of these are well known so
                    // the signature object has specific fields for them. These need to be set manually in 
                    // case the object was previously saved without them. (new ones may be added).
                    theEvent.EventSignature.InterpretParameters();
                    updated = theEvent.UpdateNewFields(file.Id);

                    // Check if the hit count needs updating.
                    if ((theEvent.TotalHits == 0) || (theEvent.TotalHits == -1))
                    {
                        updateHitCount(product, file, theEvent);
                        updated = true;
                    }
                }
                finally
                {
                    xmlFile.Close();
                }

                if (updated)
                {
                    // Write the new object data back to the file.
                    AddEvent(product, file, theEvent);
                }
            }
            return theEvent;
        }


        /// <summary>
        /// Calculates the total hits for the event from the combined EventInfo hit counts.
        /// </summary>
        /// <param name="product">The product owning the event.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to calculate the hits for.</param>
        public int GetHitCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (file == null)
                    throw new ArgumentNullException("file");
                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");

                // Load the event list.
                StackHashEventInfoCollection eventInfos = LoadEventInfoList(product, file, theEvent);

                int totalHits = 0;
                foreach (StackHashEventInfo eventInfo in eventInfos)
                {
                    totalHits += eventInfo.TotalHits;
                }

                return totalHits;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// Loads information about all events associated with a specific file in a specific product.
        /// The event.xml files are loaded in each E_* subfolder.
        /// </summary>
        /// <returns>Full event list</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public StackHashEventCollection LoadEventList(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (file == null)
                    throw new ArgumentNullException("file");


                StackHashEventCollection eventCollection = new StackHashEventCollection();

                // Find all of the product subfolders;
                string rootFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file));

                string[] subfolders = Directory.GetDirectories(rootFolder, s_EventFolderPrefix + "*");

                StackHashEvent theEvent = null;

                foreach (string folder in subfolders)
                {
                    string fileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", folder, s_EventFileName);

                    if (File.Exists(fileName))
                    {
                        try
                        {
                            theEvent = loadEvent(product, file, fileName);

                            if (theEvent != null)
                                eventCollection.Add(theEvent);
                        }
                        catch (System.Exception ex)
                        {
                            // Just log and ignore.
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load event " + fileName, ex);
                        }
                    }
                }

                return eventCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }



        /// <summary>
        /// Adds new event info to the database. The event is not added if it already 
        /// exists.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="eventInfoCollection">Event Info to add.</param>
        public void AddEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exit", "product");

                if (file == null)
                    throw new ArgumentNullException("file");
                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exit", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");
                if (!EventExists(product, file, theEvent))
                    throw new ArgumentException("Event does not exist", "theEvent");
                
                
                if (eventInfoCollection == null)
                    throw new ArgumentNullException("eventInfoCollection");

                // Get the folder and filename where the data is stored.
                string eventInfoPath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file),
                    GetEventFolder(theEvent));
                string eventPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", eventInfoPath, s_EventInfoFileName);

                if (!Directory.Exists(eventInfoPath))
                    Directory.CreateDirectory(eventInfoPath);

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(eventPathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_EventInfoSerializer.Serialize(xmlFile, eventInfoCollection);
                }
                finally
                {
                    xmlFile.Close();
                }

                // Report each EventInfo.
                for (int i = 0; i < eventInfoCollection.Count; i++)
                    OnErrorIndexChanged(new ErrorIndexEventArgs(
                        new StackHashBugTrackerUpdate(StackHashDataChanged.Hit, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, 0, theEvent.Id)));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the most recent hit date in the event info collection.
        /// </summary>
        /// <param name="product">Product.</param>
        /// <param name="file">File.</param>
        /// <param name="theEvent">Event.</param>
        /// <returns>The most recent hit date.</returns>
        public DateTime GetMostRecentHitDate(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exit", "product");

                if (file == null)
                    throw new ArgumentNullException("file");
                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exit", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");
                if (!EventExists(product, file, theEvent))
                    throw new ArgumentException("Event does not exist", "theEvent");

                StackHashEventInfoCollection eventInfos = LoadEventInfoList(product, file, theEvent);


                DateTime mostRecentDate = new DateTime(0, DateTimeKind.Local);
                foreach (StackHashEventInfo eventInfo in eventInfos)
                {
                    if (eventInfo.DateCreatedLocal > mostRecentDate)
                        mostRecentDate = eventInfo.DateCreatedLocal;
                }

                return mostRecentDate;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// Merges new event info to the database. The event is not added if it already 
        /// exists.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="eventInfoCollection">Event Info to add.</param>
        public void MergeEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exit", "product");

                if (file == null)
                    throw new ArgumentNullException("file");
                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exit", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");
                if (!EventExists(product, file, theEvent))
                    throw new ArgumentException("Event does not exist", "theEvent");


                if (eventInfoCollection == null)
                    throw new ArgumentNullException("eventInfoCollection");

                // Get the folder and filename where the data is stored.
                string eventInfoPath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file),
                    GetEventFolder(theEvent));
                string eventPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", eventInfoPath, s_EventInfoFileName);

                if (!Directory.Exists(eventInfoPath))
                    Directory.CreateDirectory(eventInfoPath);

                // Load the existing list first.

                StackHashEventInfoCollection currentEventInfos = LoadEventInfoList(product, file, theEvent);

                foreach (StackHashEventInfo eventInfo in eventInfoCollection)
                {
                    // Find the entry by hit date (effectively the ID).
                    StackHashEventInfo existingEventInfo = currentEventInfos.FindEventInfo(eventInfo);

                    if (existingEventInfo != null)
                    {
                        // Already exists so just copy the fields.
                        existingEventInfo.SetWinQualFields(eventInfo);
                    }
                    else
                    {
                        // A new one to be added.
                        currentEventInfos.Add(eventInfo);
                    }
                }

                
                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(eventPathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_EventInfoSerializer.Serialize(xmlFile, currentEventInfos);
                }
                finally
                {
                    xmlFile.Close();
                }
                OnErrorIndexChanged(new ErrorIndexEventArgs(
                    new StackHashBugTrackerUpdate(StackHashDataChanged.Hit, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, 0, theEvent.Id)));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Adds new cab to the database. The event is not added if it already 
        /// exists.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="cab">Cab to add.</param>
        /// <param name="setDiagnosticInfo">True - set diagnostic data. False - don't.</param>
        public StackHashCab AddCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            Monitor.Enter(this);

            try
            {
                // Parameters will be checked by this call. Get the product XML filename.
                string cabPath = GetCabFolder(product, file, theEvent, cab);
                string cabPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", cabPath, s_CabInfoFileName);

                if (!Directory.Exists(cabPath))
                    Directory.CreateDirectory(cabPath);

                if (!setDiagnosticInfo)
                {
                    // Read the diagnostics from the existing file if there is one.
                    StackHashCab oldCab = loadCab(cabPathAndFilename);

                    if (oldCab != null)
                        cab.DumpAnalysis = oldCab.DumpAnalysis;
                }

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(cabPathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_CabSerializer.Serialize(xmlFile, cab);
                }
                finally
                {
                    xmlFile.Close();
                }
                OnErrorIndexChanged(new ErrorIndexEventArgs(
                    new StackHashBugTrackerUpdate(StackHashDataChanged.Cab, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, cab.Id, cab.Id)));

                return cab;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        
        /// <summary>
        /// Adds new cab to the database. The event is not added if it already 
        /// exists.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="cab">Cab to add.</param>
        /// <param name="notes">Notes relating to the cab.</param>
        public int AddCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                // Parameters will be checked by this call. Get the product XML filename.
                string cabPath = GetCabFolder(product, file, theEvent, cab);
                string cabPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", cabPath, s_CabNotesFileName);

                if (note == null)
                    throw new ArgumentNullException("note");

                if (!Directory.Exists(cabPath))
                    Directory.CreateDirectory(cabPath);

                // Get the existing notes and add the new one.
                StackHashNotes notes = this.GetCabNotes(product, file, theEvent, cab);
                notes.Add(note);

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(cabPathAndFilename, FileMode.Create, FileAccess.Write);

                try
                {
                    m_NoteSerializer.Serialize(xmlFile, notes);
                }
                finally
                {
                    xmlFile.Close();
                }

                return 0;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Deletes a cab note.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="cab">Cab to add.</param>
        /// <param name="noteId">Notes relating to the cab.</param>
        public void DeleteCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets all notes for a particular cab.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="cab">Cab whose notes are required.</param>
        /// <returns>List of notes.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public StackHashNotes GetCabNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                // Parameters will be checked by this call. Get the product XML filename.
                string cabPath = GetCabFolder(product, file, theEvent, cab);
                string cabPathAndFilename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", cabPath, s_CabNotesFileName);

                StackHashNotes notes = new StackHashNotes();

                // No folder = no notes.
                if ((!Directory.Exists(cabPath) || !File.Exists(cabPathAndFilename)))
                    return notes;

                // Open the notes file and deserialize each note - one at a time.
                FileStream xmlFile = new FileStream(cabPathAndFilename, FileMode.Open, FileAccess.Read);

                try
                {
                    notes = m_NoteSerializer.Deserialize(xmlFile) as StackHashNotes;
                }
                catch (System.Exception ex)
                {
                    // Just log and ignore.
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load cab notes " + cabPathAndFilename, ex);
                }
                finally
                {
                    xmlFile.Close();
                }

                return notes;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the specified cab note.
        /// </summary>
        /// <param name="noteId">The cab entry required.</param>
        /// <returns>The requested cab note or null.</returns>
        public StackHashNoteEntry GetCabNote(int noteId)
        {
            return null;
        }

        
        /// <summary>
        /// Deletes a note to an event.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="noteId">Note to be deleted.</param>
        public void DeleteEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId)
        {
        }


        /// <summary>
        /// Adds a note to an event.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="file">File to update.</param>
        /// <param name="theEvent">Event to update.</param>
        /// <param name="note">Note relating to the event.</param>
        public int AddEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                // Parameters will be checked by this call. Get the product XML filename.
                // Find the Event folder.
                string eventFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent));

                string notesPathAndFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                    eventFolder, s_EventNotesFileName);

                if (note == null)
                    throw new ArgumentNullException("note");

                if (!Directory.Exists(eventFolder))
                    Directory.CreateDirectory(eventFolder);

                // Get the existing notes and add the new one.
                StackHashNotes notes = this.GetEventNotes(product, file, theEvent);
                notes.Add(note);

                // Simply serializes the specified data to an XML file.
                FileStream xmlFile = new FileStream(notesPathAndFileName, FileMode.Create, FileAccess.Write);

                try
                {
                    m_NoteSerializer.Serialize(xmlFile, notes);
                }
                finally
                {
                    xmlFile.Close();
                }

                return 0;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets all notes for a particular event.
        /// </summary>
        /// <param name="product">Product to retrieve.</param>
        /// <param name="file">File to retrieve.</param>
        /// <param name="theEvent">Event to retrieve.</param>
        /// <returns>List of notes.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public StackHashNotes GetEventNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                // Parameters will be checked by this call. Get the product XML filename.
                // Find the Event folder.
                string eventFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent));

                string notesPathAndFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                    eventFolder, s_EventNotesFileName);

                StackHashNotes notes = new StackHashNotes();

                // No folder = no notes.
                if ((!Directory.Exists(eventFolder) || !File.Exists(notesPathAndFileName)))
                    return notes;

                // Open the notes file and deserialize each note - one at a time.
                FileStream xmlFile = new FileStream(notesPathAndFileName, FileMode.Open, FileAccess.Read);

                try
                {
                    notes = m_NoteSerializer.Deserialize(xmlFile) as StackHashNotes;
                }
                catch (System.Exception ex)
                {
                    // Just log and ignore.
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load event notes " + notesPathAndFileName, ex);
                }
                finally
                {
                    xmlFile.Close();
                }

                return notes;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets the specified event note.
        /// </summary>
        /// <param name="noteId">The note entry required.</param>
        /// <returns>The requested event note or null.</returns>
        public StackHashNoteEntry GetEventNote(int noteId)
        {
            return null;
        }

        
        /// <summary>
        /// Loads information about all events instances associated with a specific event type.
        /// The eventinfo.xml file contains all event info fields and is stored in the same folder as the event.
        /// </summary>
        /// <returns>Full event info list</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public StackHashEventInfoCollection LoadEventInfoList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                if (file == null)
                    throw new ArgumentNullException("file");
                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exist", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");
                if (!EventExists(product, file, theEvent))
                    throw new ArgumentException("Event does not exist", "theEvent");


                StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

                // Find the Event folder.
                string eventFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent));

                string eventInfoFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                    eventFolder, s_EventInfoFileName);

                // There may not be any event info.
                if (File.Exists(eventInfoFileName))
                {
                    // Simply deserializes the specified data from the XML file.
                    FileStream xmlFile = new FileStream(eventInfoFileName, FileMode.Open, FileAccess.Read);

                    try
                    {
                        eventInfoCollection = m_EventInfoSerializer.Deserialize(xmlFile) as StackHashEventInfoCollection;
                    }
                    catch (System.Exception ex)
                    {
                        // Just log and ignore.
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load event inf " + eventInfoFileName, ex);
                    }                        
                    finally
                    {
                        xmlFile.Close();
                    }
                }

                return eventInfoCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private StackHashCab loadCab(String cabFilename)
        {
            StackHashCab cab = null;
            if (File.Exists(cabFilename))
            {
                // Simply deserializes the specified data from the XML file.
                FileStream xmlFile = new FileStream(cabFilename, FileMode.Open, FileAccess.Read);

                try
                {
                    cab = m_CabSerializer.Deserialize(xmlFile) as StackHashCab;
                }
                finally
                {
                    xmlFile.Close();
                }
            }

            return cab;
        }


        /// <summary>
        /// Loads information about all cab files associated with a specific event type.
        /// The cab.xml files are loaded in each C_* subfolder.
        /// </summary>
        /// <returns>Full cab list</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public StackHashCabCollection LoadCabList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                if (file == null)
                    throw new ArgumentNullException("file");
                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exist", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");
                if (!EventExists(product, file, theEvent))
                    throw new ArgumentException("Event does not exist", "theEvent");


                StackHashCabCollection cabCollection = new StackHashCabCollection();

                // Find the event folder.
                string rootFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file), GetEventFolder(theEvent));


                string[] cabFolders = Directory.GetDirectories(rootFolder, s_CabFolderPrefix + "*");

                foreach (string folder in cabFolders)
                {
                    string cabFileName = string.Format(CultureInfo.InstalledUICulture, "{0}\\{1}", folder, s_CabInfoFileName);

                    if (File.Exists(cabFileName))
                    {
                        try
                        {
                            StackHashCab cab = loadCab(cabFileName);

                            if (cab != null)
                            {
                                if (cab.DumpAnalysis == null)
                                    cab.DumpAnalysis = new StackHashDumpAnalysis();

                                // Beta 5 introduced the Structure Version and the CabDownloadedFlag.
                                if (cab.StructureVersion == 0)
                                {
                                    cab.CabDownloaded = File.Exists(GetCabFileName(product, file, theEvent, cab));
                                    cab.StructureVersion = StackHashCab.ThisStructureVersion;

                                    // Write the cab file back.
                                    AddCab(product, file, theEvent, cab, true);
                                }

                                cabCollection.Add(cab);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            // Just log and ignore.
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load cab " + cabFileName, ex);
                        }                        
                        
                    }
                }

                return cabCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Returns the CAB folder for the specified event.
        /// CABS for an event are stored in a subfolder for the event.
        /// </summary>
        /// <param name="thisEvent">Event for which the cab folder is required.</param>
        /// <returns>Cab file folder</returns>
        public string GetCabFolder(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            Monitor.Enter(this);

            try
            {
                if (product == null)
                    throw new ArgumentNullException("product");
                if (!ProductExists(product))
                    throw new ArgumentException("Product does not exist", "product");

                if (file == null)
                    throw new ArgumentNullException("file");
                if (!FileExists(product, file))
                    throw new ArgumentException("File does not exist", "file");

                if (theEvent == null)
                    throw new ArgumentNullException("theEvent");
                if (!EventExists(product, file, theEvent))
                    throw new ArgumentException("Event does not exist", "theEvent");

                if (cab == null)
                    throw new ArgumentNullException("cab");

                string cabFolder = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}\\{3}\\{4}{5}",
                    m_ErrorIndexPath, GetProductFolder(product), GetFileFolder(file),
                    GetEventFolder(theEvent), s_CabFolderPrefix, cab.Id.ToString(CultureInfo.InvariantCulture));

                return cabFolder;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Returns the CAB filename for the specified event.
        /// CABS for an event are stored in a subfolder for the event.
        /// </summary>
        /// <param name="thisEvent">Event for which the cab folder is required.</param>
        /// <returns>Cab file name</returns>
        public string GetCabFileName(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            Monitor.Enter(this);

            try
            {
                // GetCabFolder does all the param checks.
                string cabFileName = GetCabFolder(product, file, theEvent, cab) + "\\" + cab.FileName;

                return cabFileName;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        #endregion


        /// <summary>
        /// Returns the Subfolder where the product data is kept.
        /// </summary>
        /// <param name="product">Product whose subfolder is required.</param>
        /// <returns></returns>
        public static string GetProductFolder(StackHashProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string productFolder = string.Format(CultureInfo.InvariantCulture, "{0}{1}_{2}_{3}",
                s_ProductFolderPrefix, product.Id, PathUtils.MakeValidPathElement(product.Name), PathUtils.MakeValidPathElement(product.Version));
            return productFolder;
        }

        /// <summary>
        /// Returns the Subfolder where the product data is kept.
        /// </summary>
        /// <param name="product">Id of the required product.</param>
        /// <returns></returns>
        private static string getProductFolderWildcard(int productId)
        {
            string productFolder = string.Format(CultureInfo.InvariantCulture, "{0}{1}_*",
                s_ProductFolderPrefix, productId);

            return productFolder;
        }

        /// <summary>
        /// Returns the Subfolder where the file data is kept.
        /// </summary>
        /// <param name="file">File whose subfolder is required.</param>
        /// <returns></returns>
        public static string GetFileFolder(StackHashFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            string fileFolder = string.Format(CultureInfo.InvariantCulture, "{0}{1}_{2}_{3}", 
                s_FileFolderPrefix, file.Id, PathUtils.MakeValidPathElement(file.Name), PathUtils.MakeValidPathElement(file.Version));
            return fileFolder;
        }

        /// <summary>
        /// Returns the Subfolder where the file data is kept assuming only the id is known.
        /// </summary>
        /// <param name="fileId">File ID of file whose subfolder is required.</param>
        /// <returns>The wildcard folder name for the specified ID</returns>
        private static string getFileFolderWildcard(int fileId)
        {
            string fileFolder;
            if (fileId != 0)
                fileFolder = string.Format(CultureInfo.InvariantCulture, "{0}{1}_*", s_FileFolderPrefix, fileId);
            else
                fileFolder = string.Format(CultureInfo.InvariantCulture, "{0}*_*", s_FileFolderPrefix);
            return fileFolder;
        }

        
        /// <summary>
        /// Returns the Subfolder where the event data is kept.
        /// </summary>
        /// <param name="thisEvent">Event whose subfolder is required.</param>
        /// <returns></returns>
        public static string GetEventFolder(StackHashEvent thisEvent)
        {
            if (thisEvent == null)
                throw new ArgumentNullException("thisEvent");

            string eventFolder = string.Format(CultureInfo.InvariantCulture, "{0}{1}", 
                s_EventFolderPrefix, thisEvent.Id);
            return eventFolder;
        }

        /// <summary>
        /// Returns the filename where the event data is kept.
        /// </summary>
        /// <param name="eventInfo">Event whose subfolder is required.</param>
        /// <returns></returns>
        public static string GetEventInfoFileName(StackHashEventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            string dateTime = eventInfo.DateCreatedLocal.ToString(CultureInfo.InvariantCulture);
            // Strip any illegal chars.
            dateTime = dateTime.Replace(Path.DirectorySeparatorChar, '_');
            dateTime = dateTime.Replace(':', '_');
            dateTime = dateTime.Replace('/', '_');

            string eventInfoFileName = string.Format(CultureInfo.InvariantCulture, "{0}{1}.xml",
                s_EventInfoFileNamePrefix, dateTime);
            return eventInfoFileName;
        }

        /// <summary>
        /// Gets all event information associated with a particular product.
        /// </summary>
        /// <param name="product"></param>
        /// <returns>List of all events.</returns>
        public StackHashEventPackageCollection GetProductEvents(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            Monitor.Enter(this);

            try
            {
                StackHashEventPackageCollection eventPackages = new StackHashEventPackageCollection();

                // Cycle through getting all files, events and then the event info.
                StackHashFileCollection files = LoadFileList(product);

                foreach (StackHashFile file in files)
                {
                    // Get the list of events.
                    StackHashEventCollection events = LoadEventList(product, file);

                    foreach (StackHashEvent thisEvent in events)
                    {
                        StackHashEventInfoCollection eventInfo = LoadEventInfoList(product, file, thisEvent);
                        StackHashCabCollection cabs = LoadCabList(product, file, thisEvent);

                        StackHashCabPackageCollection cabPackages = new StackHashCabPackageCollection();
                        foreach (StackHashCab cab in cabs)
                        {
                            cabPackages.Add(new StackHashCabPackage(cab, GetCabFileName(product, file, thisEvent, cab), null, true));
                        }

                        StackHashEventPackage newEventData = new StackHashEventPackage(eventInfo, cabPackages, thisEvent, product.Id);
                        eventPackages.Add(newEventData);
                    }
                }

                return eventPackages;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Returns events matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteriaCollection">Search spec</param>
        /// <returns>List of events matching the spec.</returns>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection, StackHashProductSyncDataCollection enabledProducts)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets all events matching the specified product and file.
        /// </summary>
        /// <param name="productId">Id of the product.</param>
        /// <param name="fileId">Id of the file.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetFileEvents(int productId, int fileId, long startRow, long numberOfRows)
        {
            throw new NotImplementedException("XML Get File Events");
        }


        /// <summary>
        /// Returns events matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteriaCollection">Search spec</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <returns>List of events matching the spec.</returns>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            Monitor.Enter(this);

            try
            {
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// Retrieves stats associated with the number of events etc...
        /// </summary>
        /// <returns>Database statistics.</returns>
        public StackHashSynchronizeStatistics Statistics
        {
            get
            {
                if (m_AbortRequested)
                    return null;
                return null;
            }
        }

        /// <summary>
        /// Aborts the currently running method if there is one.
        /// </summary>
        public void AbortCurrentOperation()
        {
            m_AbortRequested = true;
        }



        private static String getSettingsFileName(String errorIndexPath)
        {
            string errorIndexSettingsFileName = Path.Combine(errorIndexPath, s_ErrorIndexSettingsFileName);
            return errorIndexSettingsFileName;
        }

        /// <summary>
        /// Renames the error index folder.
        /// </summary>
        /// <param name="newErrorIndexPath">Root folder for the index.</param>
        /// <param name="newErrorIndexName">Name and subfolder.</param>
        /// <param name="sqlSettings">Not used.</param>
        public void MoveIndex(String newErrorIndexPath, String newErrorIndexName, StackHashSqlConfiguration sqlSettings, bool allowPhysicalMove)
        {
            if (m_IsActive)
                throw new InvalidOperationException("Index not accessible while activated");

            if (newErrorIndexPath == null)
                throw new ArgumentNullException("newErrorIndexPath");
            if (newErrorIndexName == null)
                throw new ArgumentNullException("newErrorIndexName");

            Monitor.Enter(this);
            m_MoveInProgress = false;
            try
            {
                // Don't do anything if the index hasn't changed.
                if ((PathUtils.ProcessPath(newErrorIndexPath) == PathUtils.ProcessPath(m_ErrorIndexRoot)) &&
                    (String.Compare(newErrorIndexName, m_ErrorIndexName, StringComparison.OrdinalIgnoreCase) == 0))
                    return;

                String newErrorIndexPathWithSubfolder;
                String newErrorIndexRoot;

                if (!newErrorIndexPath.EndsWith("\\", true, CultureInfo.InstalledUICulture))
                {
                    newErrorIndexPathWithSubfolder = newErrorIndexPath + "\\" + newErrorIndexName;
                    newErrorIndexRoot = newErrorIndexPath + "\\";
                }
                else
                {
                    newErrorIndexPathWithSubfolder = newErrorIndexPath + newErrorIndexName;
                    newErrorIndexRoot = newErrorIndexPath;
                }

                // If the index hasn't been created then there is nothing to move.

                if (Status == ErrorIndexStatus.NotCreated)
                {
                    // Never been activated so just record the new settings.
                    m_ErrorIndexPath = newErrorIndexPathWithSubfolder;
                    m_ErrorIndexRoot = newErrorIndexRoot;
                    m_ErrorIndexName = newErrorIndexName;

                    // Make sure the settings point to the correct folder.
//                    if (m_ErrorIndexSettings != null)
                        m_ErrorIndexSettings.FileName = getSettingsFileName(m_ErrorIndexPath);

                    return;
                }

                if (Directory.Exists(newErrorIndexPathWithSubfolder))
                    throw new InvalidOperationException("Cannot move index to an existing folder");


                // Move does not work across volumes so copy if necessary.
                // Also doesn't work across UNC paths.
                if ((newErrorIndexPath[0] != '\\') && (m_ErrorIndexRoot[0] != '\\') &&
                    (m_ErrorIndexRoot.ToUpperInvariant()[0] == newErrorIndexPath.ToUpperInvariant()[0]))
                {
                    // The name is used as the subfolder where the index is stored.
                    Directory.Move(m_ErrorIndexPath, newErrorIndexPathWithSubfolder);

                    m_ErrorIndexPath = newErrorIndexPathWithSubfolder;
                    m_ErrorIndexName = newErrorIndexName;
                    m_ErrorIndexRoot = newErrorIndexPath;

                    // Make sure the settings point to the correct folder.
                    // No need to reload them though.
                    if (m_ErrorIndexSettings != null)
                        m_ErrorIndexSettings.FileName = getSettingsFileName(m_ErrorIndexPath);
                }
                else
                {
                    String originalIndexRoot = m_ErrorIndexRoot;
                    String originalIndexName = m_ErrorIndexName;

                    // Not implemented at present.
                    PathUtils.CopyDirectory(m_ErrorIndexPath, newErrorIndexPathWithSubfolder, null);

                    // Do this here before the delete - as it doesn't matter if the delete fails.
                    m_ErrorIndexPath = newErrorIndexPathWithSubfolder;
                    m_ErrorIndexName = newErrorIndexName;
                    m_ErrorIndexRoot = newErrorIndexPath;

                    // Make sure the settings point to the correct folder.
                    // No need to reload them though.
                    if (m_ErrorIndexSettings != null)
                        m_ErrorIndexSettings.FileName = getSettingsFileName(m_ErrorIndexPath);

                    DeleteIndex(originalIndexRoot, originalIndexName);
                }
            }
            finally
            {
                Monitor.Exit(this);
                m_MoveInProgress = false;
            }
        }


        /// <summary>
        /// Returns the number of cabs present for the specified product/file/event.
        /// </summary>
        /// <param name="product">Product to find.</param>
        /// <param name="file">File to find.</param>
        /// <param name="theEvent">The event to count the cabs for</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                StackHashCabCollection cabs = LoadCabList(product, file, theEvent);
                return cabs.Count;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Returns the number of cabs present with downloaded cab files for the specified product/file/event.
        /// </summary>
        /// <param name="product">Product to find.</param>
        /// <param name="file">File to find.</param>
        /// <param name="theEvent">The event to count the cabs for</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabFileCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                int totalCabFiles = 0;
                StackHashCabCollection cabs = LoadCabList(product, file, theEvent);
                foreach (StackHashCab cab in cabs)
                {
                    if (File.Exists(GetCabFileName(product, file, theEvent, cab)))
                    {
                        totalCabFiles++;
                    }
                }
                return totalCabFiles;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Delete the current index.
        /// This removes all the index files and settings.
        /// </summary>
        public void DeleteIndex()
        {
            if (m_IsActive)
                throw new InvalidOperationException("Index not accessible while activated");

            DeleteIndex(m_ErrorIndexRoot, m_ErrorIndexName);
            
            // The settings file is no longer valid.
            m_ErrorIndexSettings = new ErrorIndexXmlSettings();
        }


        /// <summary>
        /// Get the total number of events stored for the specified product.
        /// </summary>
        /// <param name="product">Product to update</param>
        /// <returns>Number of events stored for specified product.</returns>
        private int getTotalProductEvents(StackHashProduct product)
        {
            int numEvents = 0;
            String productFolder = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", m_ErrorIndexPath, GetProductFolder(product));
            String eventFolderWildcard = String.Format(CultureInfo.InvariantCulture, "{0}*", s_EventFolderPrefix);


            if (Directory.Exists(productFolder))
            {
                // Now parse the file folders.
                String [] fileFolders = getFileFolders(product);

                foreach (String file in fileFolders)
                {
                    if (Directory.Exists(file))
                    {
                        String[] eventFolders = Directory.GetDirectories(file, eventFolderWildcard);
                        numEvents += eventFolders.Length;
                    }
                }
            }

            return numEvents;
        }


        /// <summary>
        /// Updates the statistics associated with a product. This includes fields like the 
        /// TotalStoredEvents. 
        /// This information is not provided by WinQual.
        /// </summary>
        /// <param name="product">The product to update.</param>
        /// <returns>The updated product.</returns>
        public StackHashProduct UpdateProductStatistics(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Load the product details.
            product.TotalStoredEvents = getTotalProductEvents(product);

            // Store the result.
            AddProduct(product, true);

            return product;
        }


        /// <summary>
        /// Gets the number of events recorded against the listed products.
        /// </summary>
        /// <returns>Number of events.</returns>
        public long GetProductEventCount(Collection<int> products)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (products == null)
                throw new ArgumentNullException("products");

            int totalEvents = 0;

            foreach (int productId in products)
            {
                // Load the product details.
                StackHashProduct product = GetProduct(productId);

                totalEvents += product.TotalStoredEvents;
            }

            return totalEvents;
        }


        /// <summary>
        /// Delete the index in the specified folder. 
        /// Note this does not assume that the specified index is the current index. 
        /// e.g. during a Move, the new index location becomes current and the old one
        /// is deleted.
        /// </summary>
        /// <param name="errorIndexRoot"></param>
        /// <param name="errorIndexName"></param>
        public void DeleteIndex(String errorIndexRoot, String errorIndexName)
        {
            if (m_IsActive)
                throw new InvalidOperationException("Index not accessible while activated");

            Monitor.Enter(this);

            String errorIndexPath = Path.Combine(errorIndexRoot, errorIndexName);
            try
            {
                // Check if nothing created yet.
                if (!Directory.Exists(errorIndexPath))
                    return; 

                // Load the error index settings.
                string errorIndexSettingsFileName = getSettingsFileName(errorIndexPath);

                // No settings file implies nothing to delete.
                if (!File.Exists(errorIndexSettingsFileName))
                    return;

                // Quick check to make sure the path is not something like c:\ by accident.
                if (errorIndexPath.Length <= 5)
                    throw new InvalidOperationException("Error index path too short");

                // Delete the settings file.
                File.Delete(errorIndexSettingsFileName);

                // Delete all the individual folders (safer than a blanket delete all).

                // Find all of the product subfolders;
                string[] subfolders = Directory.GetDirectories(errorIndexPath, s_ProductFolderPrefix + "*");
                foreach (string folder in subfolders)
                {
                    PathUtils.DeleteDirectory(folder, true);
                }

                PathUtils.DeleteDirectory(errorIndexPath, false);

                // Don't reset the settings because in the case of a move the settings will be loaded from the new location.
//                m_ErrorIndexSettings = new ErrorIndexXmlSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public bool ParseEvents(StackHashProduct product, StackHashFile file, ErrorIndexEventParser parser)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummary(int productId)
        {
            return new StackHashProductLocaleSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummary(int productId)
        {
            return new StackHashProductOperatingSystemSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummary(int productId)
        {
            return new StackHashProductHitDateSummaryCollection();
        }

        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaryFresh(int productId)
        {
            return new StackHashProductLocaleSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaryFresh(int productId)
        {
            return new StackHashProductOperatingSystemSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaryFresh(int productId)
        {
            return new StackHashProductHitDateSummaryCollection();
        }

        #region LocaleSummaryMethods

        /// <summary>
        /// Determines if a locale summary exists.
        /// </summary>
        /// <param name="localeId">ID of the locale to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool LocaleSummaryExists(int productId, int localeId)
        {
            return false;
        }


        /// <summary>
        /// Gets all of the locale rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaries(int productId)
        {
            return new StackHashProductLocaleSummaryCollection();
        }

        /// <summary>
        /// Gets a specific locale summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductLocaleSummary GetLocaleSummaryForProduct(int productId, int localeId)
        {
            return new StackHashProductLocaleSummary();
        }

        /// <summary>
        /// Adds a locale summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose local data is to be updated.</param>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        public void AddLocaleSummary(int productId, int localeId, long totalHits, bool overwrite)
        {
        }

        #endregion LocaleSummaryMethods


        #region OperatingSystemSummaryMethods

        /// <summary>
        /// Determines if a OS summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="operatingSystemName">Name of the OS.</param>
        /// <param name="operatingSystemVersion">OS Version.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool OperatingSystemSummaryExists(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            return false;
        }


        /// <summary>
        /// Gets all of the OS rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaries(int productId)
        {
            return new StackHashProductOperatingSystemSummaryCollection();
        }


        /// <summary>
        /// Gets a specific OS summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductOperatingSystemSummary GetOperatingSystemSummaryForProduct(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            return new StackHashProductOperatingSystemSummary();
        }


        /// <summary>
        /// Adds a OS summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="operatingSystemId">ID of the OS</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        public void AddOperatingSystemSummary(int productId, short operatingSystemId, long totalHits, bool overwrite)
        {
        }

        #endregion OperatingSystemSummaryMethods

        
        #region LocaleMethods

        /// <summary>
        /// Adds a locale to the database.
        /// </summary>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="localeCode">Locale code.</param>
        /// <param name="localeName">Locale name.</param>
        public void AddLocale(int localeId, String localeCode, String localeName)
        {
            return;
        }

        #endregion LocaleMethods

        #region OperatingSystemMethods

        /// <summary>
        /// Gets the OS type ID with the specified name.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        /// <returns>ID of the OS entry.</returns>
        public short GetOperatingSystemId(String operatingSystemName, String operatingSystemVersion)
        {
            return -1;
        }

        /// <summary>
        /// Adds an operating system.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        public void AddOperatingSystem(String operatingSystemName, String operatingSystemVersion)
        {
        }

        #endregion OperatingSystemMethods


        #region HitDateSummaryMethods

        /// <summary>
        /// Determines if a HitDate summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool HitDateSummaryExists(int productId, DateTime hitDateLocal)
        {
            return false;
        }


        /// <summary>
        /// Gets all of the HitDate  rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaries(int productId)
        {
            return new StackHashProductHitDateSummaryCollection();
        }


        /// <summary>
        /// Gets a specific HitDate summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="hitDateLocal">Hit date to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductHitDateSummary GetHitDateSummaryForProduct(int productId, DateTime hitDateLocal)
        {
            return new StackHashProductHitDateSummary();
        }


        /// <summary>
        /// Adds a HitDate summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <param name="totalHits">Running total of all hits for this hit date.</param>
        public void AddHitDateSummary(int productId, DateTime hitDateLocal, long totalHits, bool overwrite)
        {
        }

        #endregion HitDateSummaryMethods


        #region UpdateTableMethods;


        /// <summary>
        /// Gets the first entry in the Update Table belonging to this profile.
        /// </summary>
        /// <param name="contextId">Context ID to which the update refers.</param>
        /// <returns>The update located - or null if no update entry exists.</returns>
        public StackHashBugTrackerUpdate GetFirstUpdate()
        {
            return null;
        }


        /// <summary>
        /// Adds a new update entry to the Update Table.
        /// Updates indicate changes that have occurred to objects in other tables.
        /// This table exists to feed the bug tracker plugins changes that have occurred
        /// to the database.
        /// Entries are normally added by the WinQualSync task and when notes are added.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void AddUpdate(StackHashBugTrackerUpdate update)
        {
            OnErrorIndexUpdateAdded(new ErrorIndexEventArgs(update));
        }

        /// <summary>
        /// Clear all elements in the update table.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void ClearAllUpdates()
        {
        }


        /// <summary>
        /// Removes the specified entry from the update table.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void RemoveUpdate(StackHashBugTrackerUpdate update)
        {
        }


        #endregion

        #region MappingTableMethods

        /// <summary>
        /// Gets the mappings of a particular type.
        /// </summary>
        /// <returns>Collection of mappings.</returns>
        public StackHashMappingCollection GetMappings(StackHashMappingType mappingType)
        {
            return null;
        }

        /// <summary>
        /// Adds the specified mappings. If they exist already they will be overwritten.
        /// </summary>
        /// <returns>Collection of mappings.</returns>
        public void AddMappings(StackHashMappingCollection mappings)
        {           
        }

        #endregion

        
        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
