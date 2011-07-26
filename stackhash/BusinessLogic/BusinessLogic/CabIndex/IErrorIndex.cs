using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;

namespace StackHashErrorIndex
{
    public enum ErrorIndexEventType
    {
        NewProduct,
        NewFile,
        NewEvent,
        NewEventInfo,
        NewCab
    }

    public class ErrorIndexEventArgs : EventArgs
    {
        private StackHashBugTrackerUpdate m_ChangeInformation;

        public StackHashBugTrackerUpdate ChangeInformation
        {
            get
            {
                return m_ChangeInformation;
            }
        }

        public ErrorIndexEventArgs(StackHashBugTrackerUpdate changeInformation)
        {
            m_ChangeInformation = changeInformation;
        }
    }

    public class ErrorIndexParseEventsEventArgs : EventArgs
    {
        public ErrorIndexEventParser Parser {get; set;}

        public ErrorIndexParseEventsEventArgs(ErrorIndexEventParser parser)
        {
            Parser = parser;
        }
    }

    public class ErrorIndexMoveEventArgs : EventArgs
    {
        public String FileName { get; set; }
        public int FileCount { get; set; }
        public bool IsCopyStarted { get; set; }

        public ErrorIndexMoveEventArgs(String fileName, int fileCount, bool isCopyStarted)
        {
            FileName = fileName;
            FileCount = fileCount;
            IsCopyStarted = isCopyStarted;
        }
    }

    public class ErrorIndexEventParser
    {
        // Delegate to hear about events being parsed.
        public event EventHandler<ErrorIndexParseEventsEventArgs> ParseEvent;

        public StackHashProduct Product { get; set; }
        public StackHashFile File { get; set; }
        public StackHashEvent CurrentEvent { get; set; }
        public bool Abort { get; set; }
        public ErrorIndexParseEventsEventArgs Args { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashSearchCriteriaCollection SearchCriteriaCollection { get; set; }

        public ErrorIndexEventParser()
        {
            // Allocate this once so that it doesn't flood the GC heap.
            Args = new ErrorIndexParseEventsEventArgs(this);
        }

        public void ProcessEvent()
        {
            EventHandler<ErrorIndexParseEventsEventArgs> parseEvent = ParseEvent;
            if (parseEvent != null)
            {
                parseEvent(this, Args);
            }
        }
    }
    
    public interface IErrorIndex : IDisposable
    {
        String ErrorIndexName
        {
            get;
        }

        // Properties.
        String ErrorIndexPath
        {
            get;
        }


        ErrorIndexStatus Status
        {
            get;
        }

        long TotalStoredEvents
        {
            get;
        }

        long TotalProducts
        {
            get;
        }

        long TotalFiles
        {
            get;
        }

        int SyncCount
        {
            get;
            set;
        }

        bool IsActive
        {
            get;
        }

        bool UpdateTableActive
        {
            get;
            set;
        }

        StackHashSyncProgress SyncProgress
        {
            get;
            set;
        }

        ErrorIndexType IndexType
        {
            get;
        }

        // Delegate to hear about changes to the event index.
        event EventHandler<ErrorIndexEventArgs> IndexUpdated;

        // Delegate to hear about additions to the update table.
        event EventHandler<ErrorIndexEventArgs> IndexUpdateAdded;

        // Delegate to hear about the progress of an index move.
        event EventHandler<ErrorIndexMoveEventArgs> IndexMoveProgress;

        
        // General.
        void DeleteIndex();
        void MoveIndex(String newErrorIndexPath, String newErrorIndexName, StackHashSqlConfiguration sqlSettings, bool allowPhysicalMove);
        void Activate();
        void Activate(bool allowIndexCreation, bool createIndexInDefaultLocation);
        void Deactivate();
        DateTime GetLastSyncTimeLocal(int productId);
        void SetLastSyncTimeLocal(int productId, DateTime lastSyncTime);
        DateTime GetLastSyncCompletedTimeLocal(int productId);
        void SetLastSyncCompletedTimeLocal(int productId, DateTime lastSyncTime);
        DateTime GetLastSyncStartedTimeLocal(int productId);
        void SetLastSyncStartedTimeLocal(int productId, DateTime lastSyncTime);
        DateTime GetLastHitTimeLocal(int productId);
        void SetLastHitTimeLocal(int productId, DateTime lastHitTime);
        StackHashTaskStatus GetTaskStatistics(StackHashTaskType taskType);
        void SetTaskStatistics(StackHashTaskStatus taskStatus);

        [SuppressMessage("Microsoft.Design", "CA1024")]
        ErrorIndexConnectionTestResults GetDatabaseStatus();

        // Products.
        StackHashProduct AddProduct(StackHashProduct product);
        StackHashProduct AddProduct(StackHashProduct product, bool updateNonWinQualFields);
        bool ProductExists(StackHashProduct product);
        StackHashProductCollection LoadProductList();
        StackHashProduct GetProduct(int productId);
        StackHashProduct UpdateProductStatistics(StackHashProduct product);
        long GetProductEventCount(Collection<int> products);

        // Files.
        void AddFile(StackHashProduct product, StackHashFile file);
        bool FileExists(StackHashProduct product, StackHashFile file);
        StackHashFileCollection LoadFileList(StackHashProduct product);
        StackHashFile GetFile(StackHashProduct product, int fileId);

        // Events.
        void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields);
        void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields, bool reportToBugTrackers);
        StackHashEventCollection LoadEventList(StackHashProduct product, StackHashFile file);
        StackHashEventPackageCollection GetProductEvents(StackHashProduct product);
        StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection, StackHashProductSyncDataCollection enabledProducts);
        StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts);
        StackHashEventPackageCollection GetFileEvents(int productId, int fileId, long startRow, long numberOfRows);
        bool EventExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        StackHashEvent GetEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        int AddEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note);
        void DeleteEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId);
        StackHashNotes GetEventNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        StackHashNoteEntry GetEventNote(int noteId);
        bool ParseEvents(StackHashProduct product, StackHashFile file, ErrorIndexEventParser parser);

        // Event Info.
        void AddEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection);
        StackHashEventInfoCollection LoadEventInfoList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        void MergeEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection);
        DateTime GetMostRecentHitDate(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        int GetHitCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);

        // Cabs.
        StackHashCab AddCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo);
        StackHashCabCollection LoadCabList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        string GetCabFolder(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab);
        string GetCabFileName(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab);
        int AddCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note);
        void DeleteCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId);
        StackHashNotes GetCabNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab);
        StackHashNoteEntry GetCabNote(int noteId);
        bool CabExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab);
        bool CabFileExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab);
        int GetCabCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        int GetCabFileCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent);
        StackHashCab GetCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int cabId);

        // Locales
        void AddLocale(int localeId, String localeCode, String localeName);

        // Operating systems.
        short GetOperatingSystemId(String operatingSystemName, String operatingSystemVersion);
        void AddOperatingSystem(String operatingSystemName, String operatingSystemVersion);

        // Rollup statistics.
        StackHashProductLocaleSummaryCollection GetLocaleSummary(int productId);
        StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummary(int productId);
        StackHashProductHitDateSummaryCollection GetHitDateSummary(int productId);

        StackHashProductLocaleSummaryCollection GetLocaleSummaryFresh(int productId);
        StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaryFresh(int productId);
        StackHashProductHitDateSummaryCollection GetHitDateSummaryFresh(int productId);

        bool LocaleSummaryExists(int productId, int localeId);
        StackHashProductLocaleSummaryCollection GetLocaleSummaries(int productId);
        StackHashProductLocaleSummary GetLocaleSummaryForProduct(int productId, int localeId);
        void AddLocaleSummary(int productId, int localeId, long totalHits, bool overwrite);

        bool OperatingSystemSummaryExists(int productId, String operatingSystemName, String operatingSystemVersion);
        StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaries(int productId);
        StackHashProductOperatingSystemSummary GetOperatingSystemSummaryForProduct(int productId, String operatingSystemName, String operatingSystemVersion);
        void AddOperatingSystemSummary(int productId, short operatingSystemId, long totalHits, bool overwrite);

        bool HitDateSummaryExists(int productId, DateTime hitDateLocal);
        StackHashProductHitDateSummaryCollection GetHitDateSummaries(int productId);
        StackHashProductHitDateSummary GetHitDateSummaryForProduct(int productId, DateTime hitDateLocal);
        void AddHitDateSummary(int productId, DateTime hitDateLocal, long totalHits, bool overwrite);

        [SuppressMessage("Microsoft.Design", "CA1024")]
        StackHashBugTrackerUpdate GetFirstUpdate();
        void AddUpdate(StackHashBugTrackerUpdate update);
        void RemoveUpdate(StackHashBugTrackerUpdate update);
        void ClearAllUpdates();
        
        // Mapping data. This consists of uint to string mappings.
        StackHashMappingCollection GetMappings(StackHashMappingType mappingType);
        void AddMappings(StackHashMappingCollection mappings);
        
        // Get stats about the number of products, events etc...
        StackHashSynchronizeStatistics Statistics
        {
            get;
        }

        void AbortCurrentOperation();
    }
}
