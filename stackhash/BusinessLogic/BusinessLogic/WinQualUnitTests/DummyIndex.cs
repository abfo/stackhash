using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using StackHashErrorIndex;
using StackHashBusinessObjects;

namespace WinQualUnitTests
{
    public class DummyIndex : IErrorIndex, IDisposable
    {
        String m_ErrorIndexPath = "c:\\stackhash";
        ErrorIndexStatus m_ErrorIndexStatus = ErrorIndexStatus.Created;
        StackHashSynchronizeStatistics m_Stats = new StackHashSynchronizeStatistics();
        int m_SyncCount = 0;
        StackHashSyncProgress m_SyncProgress;
        private bool m_UpdateTableActive;

        public DummyIndex()
        {
            // These are just here to remove the compiler warning because the events are not used.
            if (IndexUpdated != null)
                IndexUpdated(this, null);

            if (IndexUpdateAdded != null)
                IndexUpdateAdded(this, null);

            if (IndexMoveProgress != null)
                IndexMoveProgress(this, null);
        }

        public ErrorIndexType IndexType
        {
            get
            {
                return ErrorIndexType.Xml;
            }
        }

        public long TotalStoredEvents
        {
            get
            {
                return 0;
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

        public bool IsActive
        {
            get { return true; }
        }
        public String ErrorIndexName
        {
            get { return "IndexName"; }
        }

        // Properties.
        public String ErrorIndexPath
        {
            get {return m_ErrorIndexPath;}
        }


        public ErrorIndexStatus Status
        {
            get {return m_ErrorIndexStatus;}
        }

        public ErrorIndexConnectionTestResults GetDatabaseStatus()
        {
            return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.Unknown, null);
        }


        /// <summary>
        /// Total files in the index - across all products.
        /// </summary>
        public long TotalFiles
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Total products in the index - across all products.
        /// </summary>
        public long TotalProducts
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Number of times that a sync has taken place since the last full resync.
        /// </summary>
        public int SyncCount
        {
            get
            {
                return m_SyncCount;
            }

            set
            {
                m_SyncCount++;
            }
        }

        // Delegate to hear about changes to the event index.
        public event EventHandler<ErrorIndexEventArgs> IndexUpdated;
        public event EventHandler<ErrorIndexEventArgs> IndexUpdateAdded;
        public event EventHandler<ErrorIndexMoveEventArgs> IndexMoveProgress;

        // General.
        public void DeleteIndex() { }
        public void MoveIndex(String newErrorIndexPath, String newErrorIndexName, StackHashSqlConfiguration sqlSettings, bool allowPhysicalMove) { }
        public void Activate() { }
        public void Activate(bool allowIndexCreation, bool createIndexInDefaultLocation) { }
        public void Deactivate() { }
        public DateTime GetLastSyncTimeLocal(int productId) { return new DateTime(0); }
        public void SetLastSyncTimeLocal(int productId, DateTime lastSyncTime) { }
        public DateTime GetLastSyncCompletedTimeLocal(int productId) { return new DateTime(0); }
        public void SetLastSyncCompletedTimeLocal(int productId, DateTime lastSyncTime) { }
        public DateTime GetLastSyncStartedTimeLocal(int productId) { return new DateTime(0); }
        public void SetLastSyncStartedTimeLocal(int productId, DateTime lastSyncTime) { }
        public DateTime GetLastHitTimeLocal(int productId) { return new DateTime(0); }
        public void SetLastHitTimeLocal(int productId, DateTime lastHitTime) { }
        public StackHashTaskStatus GetTaskStatistics(StackHashTaskType taskType) { return null; }
        public void SetTaskStatistics(StackHashTaskStatus taskStatus) { }

        // Products.
        public StackHashProduct AddProduct(StackHashProduct product) { return product;  }
        public StackHashProduct AddProduct(StackHashProduct product, bool updateNonWinQualFields) { return product; }
        public bool ProductExists(StackHashProduct product) { return true; }
        public StackHashProductCollection LoadProductList() { return null; }
        public StackHashProduct GetProduct(int productId) { return null; }
        public StackHashProduct UpdateProductStatistics(StackHashProduct product) { return product; }
        public long GetProductEventCount(Collection<int> products) { return 0; }

        // Files.
        public void AddFile(StackHashProduct product, StackHashFile file) { }
        public bool FileExists(StackHashProduct product, StackHashFile file) { return true; }
        public StackHashFileCollection LoadFileList(StackHashProduct product) { return null; }
        public StackHashFile GetFile(StackHashProduct product, int fileId) { return null; }

        // Events.
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { }
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields) { }
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields, bool reportToBugTrackers) { }
        public StackHashEventCollection LoadEventList(StackHashProduct product, StackHashFile file) { return null; }
        public StackHashEventPackageCollection GetProductEvents(StackHashProduct product) { return null; }
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection, StackHashProductSyncDataCollection enabledProducts) { return null; }
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection, long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts) { return null; }
        public bool EventExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return false; }
        public StackHashEvent GetEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return null; }
        public bool ParseEvents(StackHashProduct product, StackHashFile file, ErrorIndexEventParser parser) { return true; }


        public int AddEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note) { return 0; }
        public void DeleteEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId) {  }
        public StackHashNotes GetEventNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return null; }
        public StackHashNoteEntry GetEventNote(int noteId) { return null; }

        // Event Info.
        public void AddEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection) { }
        public void MergeEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection) { }
        public StackHashEventInfoCollection LoadEventInfoList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return null; }
        public DateTime GetMostRecentHitDate(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return new DateTime(0); }
        public int GetHitCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return 0; }

        // Cabs.
        public StackHashCab AddCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool setDiagnostics) { return null; }
        public StackHashCabCollection LoadCabList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return null; }
        public string GetCabFolder(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab) { return null; }
        public string GetCabFileName(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab) { return null; }
        public int AddCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note) { return 0; }
        public void DeleteCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId) { return; }
        public StackHashNotes GetCabNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab) { return null; }
        public StackHashNoteEntry GetCabNote(int noteId) { return null; }
        public bool CabExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab) { return false; }
        public bool CabFileExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab) { return false; }
        public int GetCabCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return 0; }
        public int GetCabFileCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent) { return 0; }
        public StackHashCab GetCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int cabId) { return null; }


        // Stats
        public StackHashProductLocaleSummaryCollection GetLocaleSummary(int productId) { return new StackHashProductLocaleSummaryCollection(); }
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummary(int productId) { return new StackHashProductOperatingSystemSummaryCollection(); }
        public StackHashProductHitDateSummaryCollection GetHitDateSummary(int productId) { return new StackHashProductHitDateSummaryCollection(); }
        public StackHashProductLocaleSummaryCollection GetLocaleSummaryFresh(int productId) { return new StackHashProductLocaleSummaryCollection(); }
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaryFresh(int productId) { return new StackHashProductOperatingSystemSummaryCollection(); }
        public StackHashProductHitDateSummaryCollection GetHitDateSummaryFresh(int productId) { return new StackHashProductHitDateSummaryCollection(); }

        public bool LocaleSummaryExists(int productId, int localeId) { return false; }
        public StackHashProductLocaleSummaryCollection GetLocaleSummaries(int productId) { return new StackHashProductLocaleSummaryCollection(); }
        public StackHashProductLocaleSummary GetLocaleSummaryForProduct(int productId, int localeId) { return new StackHashProductLocaleSummary(); }
        public void AddLocaleSummary(int productId, int localeId, long totalHits, bool overwrite) { ;}

        public bool OperatingSystemSummaryExists(int productId, String operatingSystemName, String operatingSystemVersion) { return false; }
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaries(int productId) { return new StackHashProductOperatingSystemSummaryCollection(); }
        public StackHashProductOperatingSystemSummary GetOperatingSystemSummaryForProduct(int productId, String operatingSystemName, String operatingSystemVersion) { return new StackHashProductOperatingSystemSummary(); }
        public void AddOperatingSystemSummary(int productId, short operatingSystemId, long totalHits, bool overwrite) { ;}

        public bool HitDateSummaryExists(int productId, DateTime hitDateLocal) { return false; }
        public StackHashProductHitDateSummaryCollection GetHitDateSummaries(int productId) { return new StackHashProductHitDateSummaryCollection(); }
        public StackHashProductHitDateSummary GetHitDateSummaryForProduct(int productId, DateTime hitDateLocal) { return new StackHashProductHitDateSummary(); }
        public void AddHitDateSummary(int productId, DateTime hitDateLocal, long totalHits, bool overwrite) { ;}


        // Locales
        public void AddLocale(int localeId, String localeCode, String localeName) { ;}

        // Operating systems.
        public short GetOperatingSystemId(String operatingSystemName, String operatingSystemVersion) { return -1; }
        public void AddOperatingSystem(String operatingSystemName, String operatingSystemVersion) { ;}

        public StackHashBugTrackerUpdate GetFirstUpdate() { return null; }
        public void AddUpdate(StackHashBugTrackerUpdate update) { ; }
        public void RemoveUpdate(StackHashBugTrackerUpdate update) { ; }
        public void ClearAllUpdates() { ; }
        
        
        // Get stats about the number of products, events etc...
        public StackHashSynchronizeStatistics Statistics
        {
            get { return m_Stats;}
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
            throw new NotImplementedException("Dummy Get File Events");
        }

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
        
        public void AbortCurrentOperation() { }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
