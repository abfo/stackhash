using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

using StackHashUtilities;


namespace StackHashBusinessObjects
{
    /// <summary>
    /// Per product sync information.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProductSyncData
    {
        private int m_ProductId;

        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        public StackHashProductSyncData() { ; }
   
        public StackHashProductSyncData(int productId)
        {
            m_ProductId = productId;
        }
    }

    /// <summary>
    /// A collection of WinQual login contexts and the associated database information.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashProductSyncDataCollection : Collection<StackHashProductSyncData>
    {
        public StackHashProductSyncDataCollection() { ; } // Required for serialization.

        public StackHashProductSyncData FindProduct(int id)
        {
            foreach (StackHashProductSyncData productSyncData in this)
            {
                if (productSyncData.ProductId == id)
                    return productSyncData;
            }
            return null;
        }
    }

    

    /// <summary>
    /// Xml - stores the files in a heirarchical directory structure in XML files.
    /// Sql - SQL Server.
    /// SqlExpress - SQL Server Express version (free).
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum ErrorIndexType
    {
        [EnumMember()]
        Xml,
        [EnumMember()]
        Sql,
        [EnumMember()]
        SqlExpress
    }

    /// <summary>
    /// Defines what state the index is in at present.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum ErrorIndexStatus
    {
        [EnumMember()]
        Unknown,
        [EnumMember()]
        NotCreated,
        [EnumMember()]
        Created
    }

    /// <summary>
    /// Defines what state the database connection is in within the index.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashErrorIndexDatabaseStatus
    {
        [EnumMember()]
        Success,
        [EnumMember()]
        InvalidDatabaseName,
        [EnumMember()]
        FailedToConnectToMaster,
        [EnumMember()]
        ConnectedToMasterButDatabaseDoesNotExist,
        [EnumMember()]
        ConnectedToMasterButFailedToSeeIfDatabaseExists,
        [EnumMember()]
        DatabaseExistsButFailedToConnect,
        [EnumMember()]
        DatabaseDoesNotExist,  // XML Index
        [EnumMember()]
        Unknown,              // Shouldn't happen.
    }

    /// <summary>
    /// Identifies the location and name of the error index.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class ErrorIndexConnectionTestResults
    {
        private StackHashErrorIndexDatabaseStatus m_Result;
        private System.Exception m_LastException;
        private bool m_IsCabFolderAccessible;
        private System.Exception m_CabFolderAccessLastException;

        public ErrorIndexConnectionTestResults() { ; } // Required for serialization.

        public ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus result, System.Exception lastException)
        {
            m_Result = result;
            m_LastException = lastException;
        }

        /// <summary>
        /// Full test result.
        /// </summary>
        [DataMember]
        public StackHashErrorIndexDatabaseStatus Result
        {
            get { return m_Result; }
            set { m_Result = value; }
        }

        /// <summary>
        /// Exception that occurred during connect attempt.
        /// </summary>
        [DataMember]
        public System.Exception LastException
        {
            get { return m_LastException; }
            set { m_LastException = value; }
        }

        /// <summary>
        /// Indicates if the cab folder is accessible or not.
        /// </summary>
        [DataMember]
        public bool IsCabFolderAccessible
        {
            get { return m_IsCabFolderAccessible; }
            set { m_IsCabFolderAccessible = value; }
        }

        /// <summary>
        /// Exception that occurred during cab folder access attempt.
        /// </summary>
        [DataMember]
        public System.Exception CabFolderAccessLastException
        {
            get { return m_CabFolderAccessLastException; }
            set { m_CabFolderAccessLastException = value; }
        }

    }

    /// <summary>
    /// Defines where the database resides.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashErrorIndexLocation
    {
        [EnumMember()]
        Unknown,
        [EnumMember()]
        InCabFolder,
        [EnumMember()]
        OnSqlServer,
    }
 
    
    /// <summary>
    /// Identifies the location and name of the error index.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class ErrorIndexSettings
    {
        private String m_Folder;
        private String m_Name;
        private ErrorIndexType m_Type;
        private ErrorIndexStatus m_IndexStatus;
        private StackHashErrorIndexLocation m_IndexLocation;

        /// <summary>
        /// Folder where the error index will be stored.
        /// </summary>
        [DataMember]
        public String Folder
        {
            get { return m_Folder; }
            set { m_Folder = value; }
        }
        /// <summary>
        /// Name given to the index. This does not necessarily indicate the name
        /// of any database files but should be unique just in case.
        /// </summary>
        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        /// <summary>
        /// The type of index to be created. Need 
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Naming", "CA1721")]
        public ErrorIndexType Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        /// <summary>
        /// Creation status of the index.
        /// </summary>
        [DataMember]
        public ErrorIndexStatus Status
        {
            get { return m_IndexStatus; }
            set { m_IndexStatus = value; }
        }

        /// <summary>
        /// Location of the index.
        /// </summary>
        [DataMember]
        public StackHashErrorIndexLocation Location
        {
            get { return m_IndexLocation; }
            set { m_IndexLocation = value; }
        }
        
        public ErrorIndexSettings() { ; } // Required for serialization.

    }


    /// <summary>
    /// Each Context has a unique ID (0 for the first, 1 for the second etc...)
    /// A context contains the WinQual login data and the local store (ErrorIndex)
    /// information.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashContextSettings
    {
        private int m_Id;
        private WinQualSettings m_WinQualSettings;
        private ErrorIndexSettings m_ErrorIndexSettings;
        private ScheduleCollection m_WinQualSyncSchedule;
        private ScheduleCollection m_CabFilePurgeSchedule;
        private StackHashPurgeOptionsCollection m_PurgeOptionsCollection;
        private StackHashDebuggerSettings m_DebuggerSettings;
        private bool m_IsActive;
        private StackHashAnalysisSettings m_AnalysisSettings;
        private StackHashCollectionPolicyCollection m_CollectionPolicyCollection;
        private StackHashSqlConfiguration m_SqlSettings;
        private StackHashBugTrackerPlugInSettings m_BugTrackerSettings;
        private StackHashEmailSettings m_EmailSettings;
        private StackHashMappingCollection m_WorkFlowMappings;
        private bool m_IsIndexCreated;

        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public WinQualSettings WinQualSettings
        {
            get { return m_WinQualSettings; }
            set { m_WinQualSettings = value; }
        }

        [DataMember]
        public ErrorIndexSettings ErrorIndexSettings
        {
            get { return m_ErrorIndexSettings; }
            set { m_ErrorIndexSettings = value; }
        }

        [DataMember]
        public ScheduleCollection WinQualSyncSchedule
        {
            get { return m_WinQualSyncSchedule; }
            set { m_WinQualSyncSchedule = value; }
        }

        [DataMember]
        public ScheduleCollection CabFilePurgeSchedule
        {
            get { return m_CabFilePurgeSchedule; }
            set { m_CabFilePurgeSchedule = value; }
        }

        [DataMember]
        public StackHashDebuggerSettings DebuggerSettings
        {
            get { return m_DebuggerSettings; }
            set { m_DebuggerSettings = value; }
        }

        [DataMember]
        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; }
        }

        [DataMember]
        public StackHashAnalysisSettings AnalysisSettings
        {
            get { return m_AnalysisSettings; }
            set { m_AnalysisSettings = value; }
        }

        [DataMember]
        public StackHashPurgeOptionsCollection PurgeOptionsCollection
        {
            get { return m_PurgeOptionsCollection; }
            set { m_PurgeOptionsCollection = value; }
        }

        [DataMember]
        public StackHashCollectionPolicyCollection CollectionPolicy
        {
            get { return m_CollectionPolicyCollection; }
            set { m_CollectionPolicyCollection = value; }
        }

        [DataMember]
        public StackHashSqlConfiguration SqlSettings
        {
            get { return m_SqlSettings; }
            set { m_SqlSettings = value; }
        }

        [DataMember]
        public StackHashBugTrackerPlugInSettings BugTrackerSettings
        {
            get { return m_BugTrackerSettings; }
            set { m_BugTrackerSettings = value; }
        }

        [DataMember]
        public StackHashEmailSettings EmailSettings
        {
            get { return m_EmailSettings; }
            set { m_EmailSettings = value; }
        }

        [DataMember]
        public StackHashMappingCollection WorkFlowMappings
        {
            get { return m_WorkFlowMappings; }
            set { m_WorkFlowMappings = value; }
        }

        [DataMember]
        public bool IsIndexCreated
        {
            get { return m_IsIndexCreated; }
            set { m_IsIndexCreated = value; }
        }


        public StackHashContextSettings() { ; } // Required for serialization.
    }

    /// <summary>
    /// A collection of WinQual login contexts and the associated database information.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashContextCollection : Collection<StackHashContextSettings>
    {
        public StackHashContextCollection() { ; } // Required for serialization.
    }


    /// <summary>
    /// Full settings for the StackHash service.
    /// A context is defined as the parameters associated with one WinQual logon.
    /// Some companies may have multiple WinQual identities. The service can 
    /// logon and manage multiple of these identities concurrently.
    /// All calls to services should identify the ID.
    /// An ID of 0 is the default and will represent the first context.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSettings
    {
        private StackHashContextCollection m_ContextCollection;
        private static XmlSerializer s_Serializer = new XmlSerializer(
                typeof(StackHashSettings),
                new Type[] { typeof(StackHashSettings), typeof(StackHashContextCollection), typeof(StackHashContextSettings),
                             typeof(ErrorIndexSettings), typeof(ErrorIndexType), typeof(WinQualSettings),
                             typeof(ScheduleCollection), typeof(Schedule), typeof(SchedulePeriod), typeof(ScheduleTime),
                             typeof(DaysOfWeek), typeof(StackHashDebuggerSettings), typeof(StackHashSearchPath),
                             typeof(StackHashAnalysisSettings), typeof(StackHashProxySettings), 
                             typeof(StackHashPurgeOptionsCollection), typeof(StackHashPurgeOptions), typeof(StackHashPurgeObject),
                             typeof(StackHashCollectionPolicyCollection), typeof(StackHashCollectionPolicy), typeof(StackHashCollectionObject),
                             typeof(StackHashCollectionType), typeof(StackHashCollectionOrder), 
                             typeof(StackHashSqlConfiguration), typeof(StackHashErrorIndexLocation), 
                             typeof(StackHashBugTrackerPlugInSettings), typeof(StackHashBugTrackerPlugInCollection), typeof(StackHashBugTrackerPlugIn),
                             typeof(StackHashNameValueCollection), typeof(StackHashEmailSettings), typeof(StackHashAdminOperationCollection),
                             typeof(StackHashMappingCollection), typeof(StackHashMapping), typeof(StackHashMappingType)});

        // Context IDs are not reused unless the last context is removed in which case the IDs will
        // start again at 0.
        // e.g.
        // Create new = ID 0 ---- 0
        // Create new = ID 1 ---- 0,1
        // Delete ID 0       ---- 1
        // Create new = ID 2 ---- 2 (0 not reused).
        // Delete ID 1 and 2 ---- empty
        // Create new = ID 0 ---- restarts at ID 0 because no contexts.
        private int m_NextContextId;
        private bool m_EnableLogging;
        private bool m_ReportingEnabled; // To StackHash service.

        private StackHashProxySettings m_ProxySettings;
        
        private int m_Version;
        private String m_ServiceGuid;

        private int m_ClientTimeoutInSeconds;

        public const int CurrentVersion = 1;
        
        public static int DefaultClientTimeoutInSeconds
        {
            get { return 15 * 60; }
        }

        public StackHashSettings() 
        {
            m_Version = CurrentVersion;

            m_ServiceGuid = Guid.NewGuid().ToString();
            m_ClientTimeoutInSeconds = DefaultClientTimeoutInSeconds;
        } 

        [DataMember]
        public StackHashContextCollection ContextCollection
        {
            get { return m_ContextCollection; }
            set { m_ContextCollection = value; }
        }

        [DataMember]
        public int NextContextId
        {
            get { return m_NextContextId; }
            set { m_NextContextId = value; }
        }

        [DataMember]
        public bool EnableLogging
        {
            get { return m_EnableLogging; }
            set { m_EnableLogging = value; }
        }

        [DataMember]
        public StackHashProxySettings ProxySettings
        {
            get { return m_ProxySettings; }
            set { m_ProxySettings = value; }
        }

        [DataMember]
        public int Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        [DataMember]
        public String ServiceGuid
        {
            get { return m_ServiceGuid; }
            set { m_ServiceGuid = value; }
        }

        [DataMember]
        public bool ReportingEnabled
        {
            get { return m_ReportingEnabled; }
            set { m_ReportingEnabled = value; }
        }

        [DataMember]
        public int ClientTimeoutInSeconds
        {
            get { return m_ClientTimeoutInSeconds; }
            set { m_ClientTimeoutInSeconds = value; }
        }

        /// <summary>
        /// Saves the specified settings object to the specified XML file.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="fileName"></param>
        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public static void Save(StackHashSettings settings, String fileName)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            settings.Version = CurrentVersion;

            if ((settings.ProxySettings != null) && (settings.ProxySettings.ProxyUserName != null))
                settings.ProxySettings.ProxyUserName = SecurityUtils.EncryptStringWithUserCredentials(settings.ProxySettings.ProxyUserName);
            if ((settings.ProxySettings != null) && (settings.ProxySettings.ProxyPassword != null))
                settings.ProxySettings.ProxyPassword = SecurityUtils.EncryptStringWithUserCredentials(settings.ProxySettings.ProxyPassword);
            if ((settings.ProxySettings != null) && (settings.ProxySettings.ProxyDomain != null))
                settings.ProxySettings.ProxyDomain = SecurityUtils.EncryptStringWithUserCredentials(settings.ProxySettings.ProxyDomain);


            // Encrypt the passwords and plug-in settings.
            foreach (StackHashContextSettings contextSettings in settings.ContextCollection)
            {
                contextSettings.WinQualSettings.Password = SecurityUtils.EncryptStringWithUserCredentials(contextSettings.WinQualSettings.Password);

                if ((contextSettings.BugTrackerSettings != null) && (contextSettings.BugTrackerSettings.PlugInSettings != null))
                {
                    foreach (StackHashBugTrackerPlugIn bugTrackerSettings in contextSettings.BugTrackerSettings.PlugInSettings)
                    {
                        if (bugTrackerSettings.Properties != null)
                        {
                            foreach (StackHashNameValuePair nameValuePair in bugTrackerSettings.Properties)
                            {
                                if (nameValuePair.Name != null)
                                    nameValuePair.Name = SecurityUtils.EncryptStringWithUserCredentials(nameValuePair.Name);
                                if (nameValuePair.Value != null)
                                    nameValuePair.Value = SecurityUtils.EncryptStringWithUserCredentials(nameValuePair.Value);
                            }
                        }
                    }
                }

                if ((contextSettings.SqlSettings != null) &&
                    (contextSettings.SqlSettings.ConnectionString != null))
                {
                    contextSettings.SqlSettings.ConnectionString =
                        SecurityUtils.EncryptStringWithUserCredentials(contextSettings.SqlSettings.ConnectionString);
                }

                if (contextSettings.EmailSettings == null)
                {
                    contextSettings.EmailSettings = new StackHashEmailSettings();
                }

                // Encrypt the email username and password.
                contextSettings.EmailSettings.SmtpSettings.SmtpUsername = SecurityUtils.EncryptStringWithUserCredentials(contextSettings.EmailSettings.SmtpSettings.SmtpUsername);
                contextSettings.EmailSettings.SmtpSettings.SmtpPassword = SecurityUtils.EncryptStringWithUserCredentials(contextSettings.EmailSettings.SmtpSettings.SmtpPassword);
            }

            // Simply serializes the specified data to an XML file.
            FileStream xmlFile = null;            

            try
            {
                xmlFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                s_Serializer.Serialize(xmlFile, settings);
                xmlFile.Flush();
            }
            finally
            {
                if (xmlFile != null)
                    xmlFile.Close();

                // Decrypt any settings that were encrypted.
                foreach (StackHashContextSettings contextSettings in settings.ContextCollection)
                {
                    contextSettings.WinQualSettings.Password = SecurityUtils.DecryptStringWithUserCredentials(contextSettings.WinQualSettings.Password);

                    if ((contextSettings.BugTrackerSettings != null) && (contextSettings.BugTrackerSettings.PlugInSettings != null))
                    {
                        foreach (StackHashBugTrackerPlugIn bugTrackerSettings in contextSettings.BugTrackerSettings.PlugInSettings)
                        {
                            if (bugTrackerSettings.Properties != null)
                            {
                                foreach (StackHashNameValuePair nameValuePair in bugTrackerSettings.Properties)
                                {
                                    if (nameValuePair.Name != null)
                                        nameValuePair.Name = SecurityUtils.DecryptStringWithUserCredentials(nameValuePair.Name);
                                    if (nameValuePair.Value != null)
                                        nameValuePair.Value = SecurityUtils.DecryptStringWithUserCredentials(nameValuePair.Value);
                                }
                            }
                        }
                    }

                    if ((contextSettings.SqlSettings != null) &&
                        (contextSettings.SqlSettings.ConnectionString != null))
                    {
                        contextSettings.SqlSettings.ConnectionString =
                            SecurityUtils.DecryptStringWithUserCredentials(contextSettings.SqlSettings.ConnectionString);
                    }

                    // Decrypt the email username and password.
                    contextSettings.EmailSettings.SmtpSettings.SmtpUsername = SecurityUtils.DecryptStringWithUserCredentials(contextSettings.EmailSettings.SmtpSettings.SmtpUsername);
                    contextSettings.EmailSettings.SmtpSettings.SmtpPassword = SecurityUtils.DecryptStringWithUserCredentials(contextSettings.EmailSettings.SmtpSettings.SmtpPassword);
                }

                if ((settings.ProxySettings != null) && (settings.ProxySettings.ProxyUserName != null))
                    settings.ProxySettings.ProxyUserName = SecurityUtils.DecryptStringWithUserCredentials(settings.ProxySettings.ProxyUserName);
                if ((settings.ProxySettings != null) && (settings.ProxySettings.ProxyPassword != null))
                    settings.ProxySettings.ProxyPassword = SecurityUtils.DecryptStringWithUserCredentials(settings.ProxySettings.ProxyPassword);
                if ((settings.ProxySettings != null) && (settings.ProxySettings.ProxyDomain != null))
                    settings.ProxySettings.ProxyDomain = SecurityUtils.DecryptStringWithUserCredentials(settings.ProxySettings.ProxyDomain);

            }
        }

        /// <summary>
        /// Loads the settings from the XML file at the specified location and filename.
        /// </summary>
        /// <param name="fileName">Location and filename of XML settings file.</param>
        /// <returns>Settings loaded from XML file</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Usage", "CA1806")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506")]
        public static StackHashSettings Load(String fileName)
        {
            bool forceASave = false;

            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File does not exist", "fileName");

            // Simply deserializes the specified data from the XML file.
            FileStream xmlFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            StackHashSettings stackHashSettings = null;
            try
            {
                stackHashSettings = s_Serializer.Deserialize(xmlFile) as StackHashSettings;

                if (String.IsNullOrEmpty(stackHashSettings.ServiceGuid))
                {
                    stackHashSettings.ServiceGuid = Guid.NewGuid().ToString();
                    forceASave = true;
                }

                if (stackHashSettings.ClientTimeoutInSeconds == 0)
                    stackHashSettings.ClientTimeoutInSeconds = DefaultClientTimeoutInSeconds;

                // If the next context ID = 0 but there exists at least 1 context then this is probably
                // an old settings - pre NextContextId field. This does not affect the field.
                if (stackHashSettings.NextContextId == 0)
                {
                    forceASave = true;
                    foreach (StackHashContextSettings contextSettings in stackHashSettings.ContextCollection)
                    {
                        if (contextSettings.Id >= stackHashSettings.NextContextId)
                            stackHashSettings.NextContextId = contextSettings.Id + 1;
                    }
                }

                
                // Decrypt the passwords if encrypted.
                foreach (StackHashContextSettings contextSettings in stackHashSettings.ContextCollection)
                {
                    if (contextSettings.EmailSettings == null)
                    {
                        contextSettings.EmailSettings = new StackHashEmailSettings();
                    }
                    else
                    {
                        // Decrypt the email username and password.
                        contextSettings.EmailSettings.SmtpSettings.SmtpUsername = SecurityUtils.DecryptStringWithUserCredentials(contextSettings.EmailSettings.SmtpSettings.SmtpUsername);
                        contextSettings.EmailSettings.SmtpSettings.SmtpPassword = SecurityUtils.DecryptStringWithUserCredentials(contextSettings.EmailSettings.SmtpSettings.SmtpPassword);
                    }

                    if (contextSettings.WinQualSettings.Password.Contains('-'))
                    {
                        contextSettings.WinQualSettings.Password =
                            SecurityUtils.DecryptStringWithUserCredentials(contextSettings.WinQualSettings.Password);
                        forceASave = true;
                    }

                    if ((contextSettings.SqlSettings != null) &&
                        (contextSettings.SqlSettings.ConnectionString != null) &&
                        (contextSettings.SqlSettings.ConnectionString.Length > 3) &&
                        (contextSettings.SqlSettings.ConnectionString[2] == '-'))
                    {
                        contextSettings.SqlSettings.ConnectionString =
                            SecurityUtils.DecryptStringWithUserCredentials(contextSettings.SqlSettings.ConnectionString);
                        forceASave = true;
                    }

                    if (contextSettings.WinQualSettings.ProductsToSynchronize == null)
                    {
                        contextSettings.WinQualSettings.ProductsToSynchronize = new StackHashProductSyncDataCollection();
                        forceASave = true;
                    }

                    // Added in beta 6.
                    if (contextSettings.WinQualSettings.SyncsBeforeResync == 0)
                    {
                        contextSettings.WinQualSettings.SyncsBeforeResync = WinQualSettings.DefaultSyncsBeforeResync;
                    }

                    // Version added in beta 6.
                    if (stackHashSettings.Version == 0)
                    {
                        // This condition only required to support intermediate beta releases.
                        if (contextSettings.WinQualSettings.RequestRetryCount == 0)
                            contextSettings.WinQualSettings.RequestRetryCount = 5;

                        if (contextSettings.WinQualSettings.RequestTimeout == 0)
                            contextSettings.WinQualSettings.RequestTimeout = 300000;

                        contextSettings.WinQualSettings.RetryAfterError = true;
                        contextSettings.WinQualSettings.DelayBeforeRetryInSeconds = 30 * 60; // 30 minutes.

                        contextSettings.WinQualSettings.MaxCabDownloadFailuresBeforeAbort = 5;
                        forceASave = true;
                    }

                    if (contextSettings.PurgeOptionsCollection == null)
                    {
                        contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                        contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                        contextSettings.PurgeOptionsCollection[0].AgeToPurge = 180;
                        contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                        contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                        contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;
                        forceASave = true;
                    }

                    if ((contextSettings.CollectionPolicy == null) || (contextSettings.CollectionPolicy.Count == 0))
                    {
                        contextSettings.CollectionPolicy = StackHashCollectionPolicyCollection.Default;
                        forceASave = true;
                    }

                    if (contextSettings.SqlSettings == null)
                    {
                        contextSettings.SqlSettings = StackHashSqlConfiguration.Default;
                    }

                    if ((contextSettings.ErrorIndexSettings != null) &&
                        (contextSettings.ErrorIndexSettings.Folder != null) &&
                        (contextSettings.ErrorIndexSettings.Name != null))
                    {
                        // Set the location if not already set.
                        if ((contextSettings.ErrorIndexSettings.Status == ErrorIndexStatus.Created) &&
                            (contextSettings.ErrorIndexSettings.Location == StackHashErrorIndexLocation.Unknown))
                        {
                            String indexFolder = Path.Combine(contextSettings.ErrorIndexSettings.Folder, contextSettings.ErrorIndexSettings.Name);
                            if (Directory.Exists(indexFolder))
                            {
                                String[] databaseFiles = Directory.GetFiles(indexFolder, "*.mdf");

                                if (databaseFiles.Length > 0)
                                    contextSettings.ErrorIndexSettings.Location = StackHashErrorIndexLocation.InCabFolder;
                                else
                                    contextSettings.ErrorIndexSettings.Location = StackHashErrorIndexLocation.OnSqlServer;

                                forceASave = true;
                            }
                        }
                    }

                    // Initialise the bug tracker settings.
                    if (contextSettings.BugTrackerSettings == null)
                    {
                        contextSettings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
                        forceASave = true;
                    }
                    if (contextSettings.BugTrackerSettings.PlugInSettings == null)
                    {
                        contextSettings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();
                        forceASave = true;
                    }

                    foreach (StackHashBugTrackerPlugIn bugTrackerSettings in contextSettings.BugTrackerSettings.PlugInSettings)
                    {
                        if (bugTrackerSettings.Properties != null)
                        {
                            foreach (StackHashNameValuePair nameValuePair in bugTrackerSettings.Properties)
                            {
                                bool doEncrypt = false;
                                if (nameValuePair.Name != null)
                                {
                                    bool isEncryptedString = false;

                                    if ((nameValuePair.Name.Length > 3) && (nameValuePair.Name[2] == '-'))
                                        isEncryptedString = true;

                                    // In dev version of beta 9 the settings were not encrypted.
                                    if (doEncrypt || isEncryptedString)
                                    {
                                        try
                                        {
                                            nameValuePair.Name = SecurityUtils.DecryptStringWithUserCredentials(nameValuePair.Name);
                                            doEncrypt = true;
                                        }
                                        catch (System.Exception ex)
                                        {
                                            // Might happen if the user has switched service login account.
                                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to decrypt settings", ex);
                                        }
                                    }
                                }
                                if (nameValuePair.Value != null)
                                {
                                    if (doEncrypt)
                                        nameValuePair.Value = SecurityUtils.DecryptStringWithUserCredentials(nameValuePair.Value);
                                }
                            }
                        }
                    }
                }

                if (stackHashSettings.ProxySettings == null)
                {
                    stackHashSettings.ProxySettings = new StackHashProxySettings(false, false, null, 0, null, null, null);
                    forceASave = true;
                }

                // Decrypt the username and password settings for the proxy.
                if ((stackHashSettings.ProxySettings != null) && (stackHashSettings.ProxySettings.ProxyUserName != null))
                {
                    stackHashSettings.ProxySettings.ProxyUserName = SecurityUtils.DecryptStringWithUserCredentials(stackHashSettings.ProxySettings.ProxyUserName);
                }
                if ((stackHashSettings.ProxySettings != null) && (stackHashSettings.ProxySettings.ProxyPassword != null))
                {
                    stackHashSettings.ProxySettings.ProxyPassword = SecurityUtils.DecryptStringWithUserCredentials(stackHashSettings.ProxySettings.ProxyPassword);
                }
                if ((stackHashSettings.ProxySettings != null) && (stackHashSettings.ProxySettings.ProxyDomain != null))
                {
                    stackHashSettings.ProxySettings.ProxyDomain = SecurityUtils.DecryptStringWithUserCredentials(stackHashSettings.ProxySettings.ProxyDomain);
                }

                // Check the settings seem valid - if not disable the proxy.
                try
                {
                    if (stackHashSettings.ProxySettings.UseProxy)
                    {
                        new WebProxy(stackHashSettings.ProxySettings.ProxyHost, stackHashSettings.ProxySettings.ProxyPort);
                    }
                }
                catch (System.Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Invalid proxy settings - disabling", ex);
                    stackHashSettings.ProxySettings.UseProxy = false;
                    forceASave = true;
                }
            }
            finally
            {
                xmlFile.Close();
            }

            if (forceASave)
                Save(stackHashSettings, fileName);

            return stackHashSettings;
        }
    }
}
