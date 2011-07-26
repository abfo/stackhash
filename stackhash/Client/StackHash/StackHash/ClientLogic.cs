using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using StackHash.StackHashService;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using System.Collections;
using StackHashCabs;
using StackHashUtilities;
using System.Windows.Threading;
using System.Windows.Input;
using System.IO;
using System.Xml;
using System.Reflection;

namespace StackHash
{
    /// <summary>
    /// The intention of a request for a page of events when paging (used when
    /// a search includes script results)
    /// </summary>
    public enum PageIntention
    {
        /// <summary>
        /// Refresh the current page
        /// </summary>
        Reload,

        /// <summary>
        /// First page
        /// </summary>
        First,

        /// <summary>
        /// Previous page
        /// </summary>
        Previous,

        /// <summary>
        /// Next page
        /// </summary>
        Next,

        /// <summary>
        /// Last page
        /// </summary>
        Last
    }

    /// <summary>
    /// Errors reported by ClientLogic
    /// </summary>
    public enum ClientLogicErrorCode
    {
        /// <summary>
        /// An unexpected error occured
        /// </summary>
        Unexpected,

        /// <summary>
        /// Failed to connect to the StackHash service
        /// </summary>
        ServiceConnectFailed,

        /// <summary>
        /// A call to the StackHash service failed
        /// </summary>
        ServiceCallFailed,

        /// <summary>
        /// A timeout occured while attempting to make a service call
        /// </summary>
        ServiceCallTimeout,

        /// <summary>
        /// AdminReport indicates an error has occured
        /// </summary>
        AdminReportError,

        /// <summary>
        /// Failed to logon to WinQual
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Qual")]
        WinQualLogOnFailed,

        /// <summary>
        /// Failed to synchronize with WinQual
        /// </summary>
        SynchronizationFailed,

        /// <summary>
        /// An export failed
        /// </summary>
        ExportFailed,

        /// <summary>
        /// Failed to extract a cab downloaded from the service
        /// </summary>
        LocalCabExtractFailed,

        /// <summary>
        /// Failed to move an error index
        /// </summary>
        MoveIndexFailed,

        /// <summary>
        /// Failed to copy an error index
        /// </summary>
        CopyIndexFailed,

        /// <summary>
        /// Failed to send information to a plugin
        /// </summary>
        BugReportFailed,

        /// <summary>
        /// One or more plugins have failed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin")]
        PluginFailure,

        /// <summary>
        /// Failed to start or complete upload of a product mapping file
        /// </summary>
        UploadMappingFileFailed,
    }

    /// <summary>
    /// The type of UI needed
    /// </summary>
    public enum ClientLogicUIRequest
    {
        /// <summary>
        /// The UI should configure profile settings
        /// </summary>
        ProfileSettings,

        /// <summary>
        /// The list of contexts (profile settings) is ready
        /// </summary>
        ContextCollectionReady,

        /// <summary>
        /// The UI should configure script settings
        /// </summary>
        ScriptSettings,

        /// <summary>
        /// The requested script is ready (CurrentScript)
        /// </summary>
        ScriptReady,

        /// <summary>
        /// The results from running a script are ready (CurrentScriptResults)
        /// </summary>
        ScriptResultsReady,

        /// <summary>
        /// The requested list of event packages is ready
        /// </summary>
        EventPackageListReady,

        /// <summary>
        /// Notes for the current event are ready
        /// </summary>
        EventNotesReady,

        /// <summary>
        /// Notes for the current cab are ready
        /// </summary>
        CabNotesReady,

        /// <summary>
        /// New context settings are ready
        /// </summary>
        NewContextSettingsReady,

        /// <summary>
        /// Cab contents folder is ready for debugging
        /// </summary>
        CabFolderReady,

        /// <summary>
        /// File extracted from a cab is ready for viewing
        /// </summary>
        CabFileReady,

        /// <summary>
        /// An index move has completed
        /// </summary>
        MoveIndexComplete,

        /// <summary>
        /// The current event package has been refreshed
        /// </summary>
        EventPackageRefreshComplete,

        /// <summary>
        /// Proxy server settings have been updated
        /// </summary>
        ProxySettingsUpdated,

        /// <summary>
        /// Testing the WinQual LogOn succeeded
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Qual")]
        TestWinQualLogOnSuccess,

        /// <summary>
        /// An export has completed 
        /// </summary>
        ExportComplete,

        /// <summary>
        /// Product properties have been updated
        /// </summary>
        ProductPropertiesUpdated,

        /// <summary>
        /// Event Package properties have been updated
        /// </summary>
        EventPackagePropertiesUpdated,

        /// <summary>
        /// The products list has been updated
        /// </summary>
        ProductsUpdated,

        /// <summary>
        /// Context status is ready
        /// </summary>
        ContextStatusReady,

        /// <summary>
        /// An index copy has completed
        /// </summary>
        CopyIndexComplete,

        /// <summary>
        /// One or more error indexes need to be upgraded from XML
        /// </summary>
        UpgradeIndexFromXml,

        /// <summary>
        /// Test index has been created
        /// </summary>
        TestIndexCreated,

        /// <summary>
        /// Hit summary information for a product has been updated
        /// </summary>
        ProductSummaryUpdated,

        /// <summary>
        /// Products is now null
        /// </summary>
        ProductListCleared,

        /// <summary>
        /// Another client has deactivated this context
        /// </summary>
        OtherClientDeactivatedThisContext,

        /// <summary>
        /// Test of database connectivity has completed
        /// </summary>
        DatabaseTestComplete,

        /// <summary>
        /// Client has been bumped (inactive when all seats are in use, admin has taken over license)
        /// </summary>
        ClientBumped,

        /// <summary>
        /// Produt mapping file has been uploaded to WinQual
        /// </summary>
        UploadMappingFileCompleted,
    }

    /// <summary>
    /// Notfies the setup wizard that an action has completed
    /// </summary>
    public enum ClientLogicSetupWizardPromptOperation
    {
        /// <summary>
        /// Service connect has completed
        /// </summary>
        AdminServiceConnect,

        /// <summary>
        /// First context has been created / updated
        /// </summary>
        FirstContextCreated,

        /// <summary>
        /// The product list has been updated
        /// </summary>
        ProductListUpdated,

        /// <summary>
        /// Test logon completed
        /// </summary>
        LogOnTestComplete,

        /// <summary>
        /// Proxy settings updated
        /// </summary>
        ProxySettingsUpdated,

        /// <summary>
        /// Failed to synchronize with WinQual
        /// </summary>
        SyncFailed,

        /// <summary>
        /// Global collection policies have been set
        /// </summary>
        CollectionPoliciesSet,

        /// <summary>
        /// Reporting has been enabled or disabled
        /// </summary>
        ReportingUpdated
    }

    /// <summary>
    /// The current view
    /// </summary>
    public enum ClientLogicView
    {
        /// <summary>
        /// Shows the list of mapped products
        /// </summary>
        ProductList,

        /// <summary>
        /// Shows a list of events
        /// </summary>
        EventList,

        /// <summary>
        /// Shows detail for a single event
        /// </summary>
        EventDetail,

        /// <summary>
        /// Shows detail for a single cab
        /// </summary>
        CabDetail
    }

    /// <summary>
    /// Event args reporting that sync has completed
    /// </summary>
    public class SyncCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// True if the sync succeeded
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// The reported service error code
        /// </summary>
        public StackHashServiceErrorCode ServiceError { get; private set; }

        /// <summary>
        /// Event args reporting that sync has completed
        /// </summary>
        /// <param name="succeeded">True if the sync succeeded</param>
        /// <param name="serviceError">The reported service error code</param>
        public SyncCompleteEventArgs(bool succeeded, StackHashServiceErrorCode serviceError)
        {
            this.Succeeded = succeeded;
            this.ServiceError = serviceError;
        }
    }

    /// <summary>
    /// Event args reporting an error in ClientLogic
    /// </summary>
    public class ClientLogicErrorEventArgs : EventArgs
    {
        private Exception _exception;
        private ClientLogicErrorCode _error;
        private StackHashServiceErrorCode _serviceError;

        /// <summary>
        /// Event args reporting an error in ClientLogic
        /// </summary>
        /// <param name="exception">Associated exception, may be null</param>
        /// <param name="error">Error code</param>
        /// <param name="serviceError">Service error code (if any)</param>
        public ClientLogicErrorEventArgs(Exception exception, ClientLogicErrorCode error, StackHashServiceErrorCode serviceError)
        {
            _exception = exception;
            _error = error;
            _serviceError = serviceError;
        }
        
        /// <summary>
        /// Gets the associated exception, not this may be null
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }
        
        /// <summary>
        /// Gets the error
        /// </summary>
        public ClientLogicErrorCode Error
        {
            get { return _error; }
        }

        /// <summary>
        /// Gets the service error code (if any)
        /// </summary>
        public StackHashServiceErrorCode ServiceError
        {
            get { return _serviceError; }
        }
    }

    /// <summary>
    /// Notifies the setup wizard that an operation of interest has completed
    /// </summary>
    public class ClientLogicSetupWizardPromptEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the prompt (operation)
        /// </summary>
        public ClientLogicSetupWizardPromptOperation Prompt { get; private set; }

        /// <summary>
        /// True if the operation succeeded
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Gets the last service error (if any)
        /// </summary>
        public StackHashServiceErrorCode LastServiceError { get; set; }

        /// <summary>
        /// Gets the last exception (if any)
        /// </summary>
        public Exception LastException { get; set; }

        /// <summary>
        /// Notifies the setup wizard that an operation of interest has completed
        /// </summary>
        /// <param name="prompt">The prompt (operation)</param>
        /// <param name="succeeded">True if the operation succeeded</param>
        /// <param name="lastServiceError">Last service error (if any)</param>
        /// <param name="lastException">Last exception (if any)</param>
        public ClientLogicSetupWizardPromptEventArgs(ClientLogicSetupWizardPromptOperation prompt, 
            bool succeeded, 
            StackHashServiceErrorCode lastServiceError, 
            Exception lastException)
        {
            this.Prompt = prompt;
            this.Succeeded = succeeded;
            this.LastServiceError = lastServiceError;
            this.LastException = lastException;
        }
    }

    /// <summary>
    /// Event args reporting that ClientLogic is ready for UI
    /// </summary>
    public class ClientLogicUIEventArgs : EventArgs
    {
        private ClientLogicUIRequest _uiRequest;

        /// <summary>
        /// Event args reporting that ClientLogic is ready for UI
        /// </summary>
        /// <param name="uiRequest">The UI request</param>
        public ClientLogicUIEventArgs(ClientLogicUIRequest uiRequest)
        {
            _uiRequest = uiRequest;
        }

        /// <summary>
        /// Gets the UI request
        /// </summary>
        public ClientLogicUIRequest UIRequest
        {
            get { return _uiRequest; }
        }
    }

    /// <summary>
    /// StackHash Client Logic
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed class ClientLogic : INotifyPropertyChanged, IDisposable
    {
        private const string DefaultIndexFolderBase = "StackHash";
        private const string DefaultIndexName = "Index";

        private enum WorkerType
        {
            ServiceConnect,
            GetContextSettings,
            SetContextSettings,
            GetProductList,
            GetProductEventPackages,
            StartSync,
            GetEventPackages,
            GetDebuggerScriptNames,
            GetScript,
            RemoveScript,
            RenameScript,
            AddScript,
            TestScript,
            GetResultFiles,
            GetResult,
            AddGetCabNotes,
            AddGetEventNotes,
            ActivateStackHashContext,
            DeactivateStackHashContext,
            CreateNewStackHashContext,
            RemoveStackHashContext,
            EnableLogging,
            DisableLogging,
            GetCabContents,
            GetCabFile,
            MoveIndex,
            RemoveScriptResult,
            CreateFirstContext,
            SetProductSyncState,
            RunWinQualLogOn,
            RefreshEventPackage,
            SetProxy,
            SetEventBugId,
            AbortSync,
            ExportProductList,
            ExportEventList,
            GetCollectionPolicies,
            UpdateProductProperties,
            UpdateEventPackageProperties,
            SetCollectionPolicies,
            DownloadCab,
            GetContextStatus,
            EnableDisableReporting,
            CopyIndex,
            CreateTestIndex,
            GetWindowedEventPackages,
            UpdateProductSummary,
            TestDatabase,
            GetPluginDiagnostics,
            SendEventToPlugin,
            SendProductToPlugin,
            SendCabToPlugin,
            SendAllToPlugins,
            UploadMappingFile,
            SetEventWorkFlow,
            NoOp,

            // generic results for certain common actions
            ResultOnlyRefreshContextSettings,
            ResultOnlyNoAction,
            Export
        }

        #region WorkerArg Private Classes

        private class WorkerArg
        {
            private WorkerType _type;

            public WorkerArg(WorkerType type)
            {
                _type = type;
            }

            public WorkerType Type
            {
                get { return _type; }
            }
        }

        private class WorkerArgNoOp : WorkerArg
        {
            public WorkerArgNoOp()
                : base(WorkerType.NoOp) { }
        }

        private class WorkerArgServiceConnect : WorkerArg
        {
            public WorkerArgServiceConnect()
                : base(WorkerType.ServiceConnect) { }
        }

        private class WorkerArgGetContextSettings : WorkerArg
        {
            public WorkerArgGetContextSettings()
                : base(WorkerType.GetContextSettings) { }
        }

        private class WorkerArgSetContextSettings : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashSettings Settings { get; private set; }
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }
            public StackHashBugTrackerPlugInCollection PlugIns { get; private set; }
            public StackHashMappingCollection WorkFlowMappings { get; private set; }
            public bool IsActive { get; private set; }

            public WorkerArgSetContextSettings(int contextId,
                StackHashSettings settings, 
                StackHashCollectionPolicyCollection collectionPolicies, 
                StackHashBugTrackerPlugInCollection plugIns, 
                StackHashMappingCollection workFlowMappings,
                bool isActive)
                : base(WorkerType.SetContextSettings)
            {
                this.ContextId = contextId;
                this.Settings = settings;
                this.CollectionPolicies = collectionPolicies;
                this.PlugIns = plugIns;
                this.WorkFlowMappings = workFlowMappings;
                this.IsActive = isActive;
            }
        }

        private class WorkerArgGetProductList : WorkerArg
        {
            private int _contextId;

            public WorkerArgGetProductList(int contextId)
                : base(WorkerType.GetProductList)
            {
                _contextId = contextId;
            }

            public int ContextId
            {
                get { return _contextId; }
            }
        }

        private class WorkerArgStartSync : WorkerArg
        {
            public int ContextId { get; private set; }
            public bool ForceResync { get; private set; }
            public bool ProductsOnly { get; private set; }
            public List<int> ProductsToSync { get; private set; }

            public WorkerArgStartSync(int contextId, bool forceResync, bool productsOnly, List<int> productsToSync)
                : base(WorkerType.StartSync)
            {
                this.ContextId = contextId;
                this.ForceResync = forceResync;
                this.ProductsOnly = productsOnly;
                this.ProductsToSync = productsToSync;
            }
        }

        private class WorkerArgUpdateProductSummary : WorkerArg
        {
            public int ContextId { get; private set; }
            public int ProductId { get; private set; }

            public WorkerArgUpdateProductSummary(int contextId, int productId)
                : base(WorkerType.UpdateProductSummary)
            {
                this.ContextId = contextId;
                this.ProductId = productId;
            }
        }

        private class WorkerArgGetWindowedEventPackages : WorkerArg
        {
            public int ContextId { get; private set; }
            public int Page { get; private set; }
            public int EventsPerPage { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashSearchCriteriaCollection Search { get; private set; }
            public StackHashSortOrderCollection Sort { get; private set; }
            public bool UpdateFileCache { get; private set; }
            public ClientLogicView LoadView { get; private set; }
            public bool IsScriptSearch { get; private set; }
            public PageIntention PageIntention { get; private set; }
            public long LastMinRow { get; private set; }
            public long LastMaxRow { get; private set; }
            public long LastTotalRows { get; private set; }

            public WorkerArgGetWindowedEventPackages(int contextId,
                int page,
                int eventsPerPage,
                StackHashProduct product,
                StackHashSearchCriteriaCollection search,
                StackHashSortOrderCollection sort,
                bool updateFileCache,
                ClientLogicView loadView,
                bool isScriptSearch,
                PageIntention pageIntention,
                long lastMinRow,
                long lastMaxRow,
                long lastTotalRows)
                : base(WorkerType.GetWindowedEventPackages)
            {
                this.ContextId = contextId;
                this.Page = page;
                this.EventsPerPage = eventsPerPage;
                this.Product = product;
                this.Search = search;
                this.Sort = sort;
                this.UpdateFileCache = updateFileCache;
                this.LoadView = loadView;
                this.IsScriptSearch = isScriptSearch;
                this.PageIntention = pageIntention;
                this.LastMinRow = lastMinRow;
                this.LastMaxRow = lastMaxRow;
                this.LastTotalRows = lastTotalRows;
            }
        }

        private class WorkerArgGetDebuggerScriptNames : WorkerArg
        {
            public WorkerArgGetDebuggerScriptNames()
                : base(WorkerType.GetDebuggerScriptNames) { }
        }

        private class WorkerArgGetScript : WorkerArg
        {
            private string _scriptName;

            public WorkerArgGetScript(string scriptName)
                : base(WorkerType.GetScript)
            {
                _scriptName = scriptName;
            }

            public string ScriptName
            {
                get { return _scriptName; }
            }
        }

        private class WorkerArgRemoveScript : WorkerArg
        {
            private string _scriptName;

            public WorkerArgRemoveScript(string scriptName)
                : base(WorkerType.RemoveScript)
            {
                _scriptName = scriptName;
            }

            public string ScriptName
            {
                get { return _scriptName; }
            }
        }

        private class WorkerArgRenameScript : WorkerArg
        {
            private string _orignalName;
            private string _newName;

            public WorkerArgRenameScript(string originalName, string newName)
                : base(WorkerType.RenameScript)
            {
                _orignalName = originalName;
                _newName = newName;
            }

            public string OrignalName
            {
                get { return _orignalName; }
            }
           
            public string NewName
            {
                get { return _newName; }
            }
        }

        private class WorkerArgAddScript : WorkerArg
        {
            private StackHashScriptSettings _scriptSettings;
            private string _originalName;
            private bool _overwrite;

            public WorkerArgAddScript(StackHashScriptSettings scriptSettings, string originalName, bool overwrite)
                : base(WorkerType.AddScript)
            {
                _scriptSettings = scriptSettings;
                _originalName = originalName;
                _overwrite = overwrite;
            }

            public StackHashScriptSettings ScriptSettings
            {
                get { return _scriptSettings; }
            }

            public string OriginalName
            {
                get { return _originalName; }
            }

            public bool Overwrite
            {
                get { return _overwrite; }
            }
        }

        private class WorkerArgTestScript : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string ScriptName { get; private set; }

            public WorkerArgTestScript(int contextId, 
                StackHashProduct stackHashProduct, 
                StackHashEvent stackHashEvent, 
                StackHashCab stackHashCab, 
                string scriptName)
                : base(WorkerType.TestScript)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
                this.ScriptName = scriptName;
            }
        }

        private class WorkerArgGetResultFiles : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }

            public WorkerArgGetResultFiles(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                StackHashCab stackHashCab)
                : base(WorkerType.GetResultFiles)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
            }
        }

        private class WorkerArgGetResult : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string ScriptName { get; private set; }

            public WorkerArgGetResult(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                StackHashCab stackHashCab,
                string scriptName)
                : base(WorkerType.GetResult)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
                this.ScriptName = scriptName;
            }
        }

        private class WorkerArgAddGetEventNotes : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public string Note { get; private set; }
            public string User { get; private set; }
            public int NoteId { get; private set; }

            /// <summary>
            /// Note - pass null for note and user to just get the current notes
            /// </summary>
            public WorkerArgAddGetEventNotes(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                string note,
                string user,
                int noteId)
                : base(WorkerType.AddGetEventNotes)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Note = note;
                this.User = user;
                this.NoteId = noteId;
            }
        }

        private class WorkerArgAddGetCabNotes : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string Note { get; private set; }
            public string User { get; private set; }
            public int NoteId { get; private set; }

            /// <summary>
            /// Note - pass null for note and user to just get the current notes
            /// </summary>
            public WorkerArgAddGetCabNotes(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                StackHashCab stackHashCab,
                string note,
                string user,
                int noteId)
                : base(WorkerType.AddGetCabNotes)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
                this.Note = note;
                this.User = user;
                this.NoteId = noteId;
            }
        }

        private class WorkerArgActivateStackHashContext : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgActivateStackHashContext(int contextId)
                : base(WorkerType.ActivateStackHashContext)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgDeactivateStackHashContext : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgDeactivateStackHashContext(int contextId)
                : base(WorkerType.DeactivateStackHashContext)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgRemoveStackHashContext : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgRemoveStackHashContext(int contextId)
                : base(WorkerType.RemoveStackHashContext)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgCreateNewStackHashContext : WorkerArg
        {
            public WorkerArgCreateNewStackHashContext()
                : base(WorkerType.CreateNewStackHashContext) { }
        }

        private class WorkerArgEnableLogging : WorkerArg
        {
            public WorkerArgEnableLogging()
                : base(WorkerType.EnableLogging) { }
        }

        private class WorkerArgDisableLogging : WorkerArg
        {
            public WorkerArgDisableLogging()
                : base(WorkerType.DisableLogging) { }
        }

        private class WorkerArgGetCabContents : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string ExtractFolder { get; private set; }

            // if extractFolder is null use temp folder and delete files
            // during cleanup
            public WorkerArgGetCabContents(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                StackHashCab stackHashCab,
                string extractFolder)
                : base(WorkerType.GetCabContents)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
                this.ExtractFolder = extractFolder;
            }
        }

        private class WorkerArgGetCabFile : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string FileName { get; private set; }
            public long FileLength { get; private set; }

            // if extractFolder is null use temp folder and delete files
            // during cleanup
            public WorkerArgGetCabFile(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                StackHashCab stackHashCab,
                string fileName,
                long fileLength)
                : base(WorkerType.GetCabFile)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
                this.FileName = fileName;
                this.FileLength = fileLength;
            }
        }

        private class WorkerArgMoveIndex : WorkerArg
        {
            public int ContextId { get; private set; }
            public string NewPath { get; private set; }
            public string NewName { get; private set; }
            public string NewConnectionString { get; private set; }
            public string NewInitialCatalog { get; private set; }
            public bool IsActive { get; private set; }
            public int MinPoolSize { get; private set; }
            public int MaxPoolSize { get; private set; }
            public int ConnectionTimeout { get; private set; }
            public int EventsPerBlock { get; private set; }

            public WorkerArgMoveIndex(int contextId, 
                string newPath, 
                string newName, 
                string newConnectionString, 
                string newInitialCatalog, 
                bool isActive,
                int minPoolSize,
                int maxPoolSize,
                int connectionTimeout,
                int eventsPerBlock)
                : base(WorkerType.MoveIndex)
            {
                this.ContextId = contextId;
                this.NewPath = newPath;
                this.NewName = newName;
                this.NewConnectionString = newConnectionString;
                this.NewInitialCatalog = newInitialCatalog;
                this.IsActive = isActive;
                this.MinPoolSize = minPoolSize;
                this.MaxPoolSize = maxPoolSize;
                this.ConnectionTimeout = connectionTimeout;
                this.EventsPerBlock = eventsPerBlock;
            }
        }

        private class WorkerArgCopyIndex : WorkerArg
        {
            public int ContextId { get; private set; }
            public string NewPath { get; private set; }
            public string NewName { get; private set; }
            public string NewConnectionString { get; private set; }
            public string NewInitialCatalog { get; private set; }
            public bool IsActive { get; private set; }
            public bool IsDatabaseInCabFolder { get; private set; }

            public WorkerArgCopyIndex(int contextId, 
                string newPath, 
                string newName, 
                string newConnectionString, 
                string newInitialCatalog, 
                bool isActive,
                bool isDatabaseInCabFolder)
                : base(WorkerType.CopyIndex)
            {
                this.ContextId = contextId;
                this.NewPath = newPath;
                this.NewName = newName;
                this.NewConnectionString = newConnectionString;
                this.NewInitialCatalog = newInitialCatalog;
                this.IsActive = isActive;
                this.IsDatabaseInCabFolder = isDatabaseInCabFolder;
            }
        }

        private class WorkerArgRemoveScriptResult : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string ScriptName { get; private set; }

            public WorkerArgRemoveScriptResult(int contextId,
                StackHashProduct stackHashProduct,
                StackHashEvent stackHashEvent,
                StackHashCab stackHashCab,
                string scriptName)
                : base(WorkerType.RemoveScriptResult)
            {
                this.ContextId = contextId;
                this.Product = stackHashProduct;
                this.Event = stackHashEvent;
                this.Cab = stackHashCab;
                this.ScriptName = scriptName;
            }
        }

        private class WorkerArgCreateFirstContext : WorkerArg
        {
            public string ContextName { get; private set; }
            public string Username { get; private set; }
            public string Password { get; private set; }
            public string CdbPath32 { get; private set; }
            public string CdbPath64 { get; private set; }
            public string IndexFolder { get; private set; }
            public string ConnectionString { get; private set; }
            public string ProfileName { get; private set; }
            public bool IsDatabaseInIndexFolder {get; private set;}

            public WorkerArgCreateFirstContext(string contextName,
                string username,
                string password,
                string cdbPath32,
                string cdbPath64,
                string indexFolder,
                string connectionString,
                string profileName,
                bool isDatabaseInIndexFolder)
                : base(WorkerType.CreateFirstContext)
            {
                this.ContextName = contextName;
                this.Username = username;
                this.Password = password;
                this.CdbPath32 = cdbPath32;
                this.CdbPath64 = cdbPath64;
                this.IndexFolder = indexFolder;
                this.ConnectionString = connectionString;
                this.ProfileName = profileName;
                this.IsDatabaseInIndexFolder = isDatabaseInIndexFolder;
            }
        }

        private class WorkerArgSetProductSyncState : WorkerArg
        {
            public int ContextId { get; private set; }
            public int ProductId { get; private set; }
            public bool SyncState { get; private set; }

            public WorkerArgSetProductSyncState(int contextId, int productId, bool syncState)
                : base(WorkerType.SetProductSyncState)
            {
                this.ContextId = contextId;
                this.ProductId = productId;
                this.SyncState = syncState;
            }
        }

        private class WorkerArgRunWinQualLogOn : WorkerArg
        {
            public int ContextId { get; private set; }
            public string UserName { get; private set; }
            public string Password { get; private set; }

            public WorkerArgRunWinQualLogOn(int contextId, string userName, string password)
                : base(WorkerType.RunWinQualLogOn)
            {
                this.ContextId = contextId;
                this.UserName = userName;
                this.Password = password;
            }
        }

        private class WorkerArgRefreshEventPackage : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }

            public WorkerArgRefreshEventPackage(int contextId, StackHashProduct product, StackHashEvent stackHashEvent)
                : base(WorkerType.RefreshEventPackage)
            {
                this.ContextId = contextId;
                this.Product = product;
                this.Event = stackHashEvent;
            }
        }

        private class WorkerArgSetProxy : WorkerArg
        {
            public StackHashProxySettings ProxySettings { get; private set; }
            public int ClientTimeoutInSeconds { get; private set; }
            public int ContextId { get; private set; }
            public bool ServiceIsLocal { get; private set; }

            public WorkerArgSetProxy(StackHashProxySettings proxySettings,
                int clientTimeoutInSeconds,
                int contextId,
                bool serviceIsLocal)
                : base(WorkerType.SetProxy)
            {
                this.ProxySettings = proxySettings;
                this.ClientTimeoutInSeconds = clientTimeoutInSeconds;
                this.ContextId = contextId;
                this.ServiceIsLocal = serviceIsLocal;
            }
        }

        private class WorkerArgSetEventBugId : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public string BugId { get; private set; }

            public WorkerArgSetEventBugId(int contextId, StackHashProduct product, StackHashEvent stackHashEvent, string bugId)
                : base(WorkerType.SetEventBugId)
            {
                this.ContextId = contextId;
                this.Product = product;
                this.Event = stackHashEvent;
                this.BugId = bugId;
            }
        }

        private class WorkerArgSetEventWorkFlow : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public int workFlowId { get; private set; }

            public WorkerArgSetEventWorkFlow(int contextId, StackHashProduct product, StackHashEvent stackHashEvent, int workFlowId)
                : base(WorkerType.SetEventWorkFlow)
            {
                this.ContextId = contextId;
                this.Product = product;
                this.Event = stackHashEvent;
                this.workFlowId = workFlowId;
            }
        }

        private class WorkerArgAbortSync : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgAbortSync(int contextId)
                : base(WorkerType.AbortSync)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgExportProductList : WorkerArg
        {
            public ObservableCollection<DisplayProduct> Products { get; private set; }
            public string ExportPath { get; private set; }

            public WorkerArgExportProductList(ObservableCollection<DisplayProduct> products, string exportPath)
                : base(WorkerType.ExportProductList)
            {
                this.Products = products;
                this.ExportPath = exportPath;
            }
        }

        private class WorkerArgExportEventList : WorkerArg
        {
            public ObservableCollection<DisplayEventPackage> EventPackages { get; private set; }
            public string ExportPath { get; private set; }
            public bool ExportCabsAndEventInfos { get; private set; }

            public WorkerArgExportEventList(ObservableCollection<DisplayEventPackage> eventPackages, string exportPath, bool exportCabsAndEventInfos)
                : base(WorkerType.ExportEventList)
            {
                this.EventPackages = eventPackages;
                this.ExportPath = exportPath;
                this.ExportCabsAndEventInfos = exportCabsAndEventInfos;
            }
        }

        private class WorkerArgGetCollectionPolicies : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgGetCollectionPolicies(int contextId)
                : base(WorkerType.GetCollectionPolicies)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgUpdateProductProperties : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashCollectionPolicyCollection CollectionPolicies { get; set; }
            public StackHashCollectionPolicyCollection CollectionPoliciesToRemove { get; set; }

            public WorkerArgUpdateProductProperties(int contextId, 
                StackHashCollectionPolicyCollection collectionPolicies,
                StackHashCollectionPolicyCollection collectionPoliciesToRemove)
                : base(WorkerType.UpdateProductProperties)
            {
                this.ContextId = contextId;
                this.CollectionPolicies = collectionPolicies;
                this.CollectionPoliciesToRemove = collectionPoliciesToRemove;
            }
        }

        private class WorkerArgUpdateEventPackageProperties : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashCollectionPolicyCollection CollectionPolicies { get; set; }
            public StackHashCollectionPolicyCollection CollectionPoliciesToRemove { get; set; }

            public WorkerArgUpdateEventPackageProperties(int contextId, 
                StackHashCollectionPolicyCollection collectionPolicies,
                StackHashCollectionPolicyCollection collectionPoliciesToRemove)
                : base(WorkerType.UpdateEventPackageProperties)
            {
                this.ContextId = contextId;
                this.CollectionPolicies = collectionPolicies;
                this.CollectionPoliciesToRemove = collectionPoliciesToRemove;
            }
        }

        private class WorkerArgSetCollectionPolicies : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashCollectionPolicyCollection CollectionPolicies { get; set; }

            public WorkerArgSetCollectionPolicies(int contextId, StackHashCollectionPolicyCollection collectionPolicies)
                : base(WorkerType.SetCollectionPolicies)
            {
                this.ContextId = contextId;
                this.CollectionPolicies = collectionPolicies;
            }
        }

        private class WorkerArgDownloadCab : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashCab Cab { get; private set; }

            public WorkerArgDownloadCab(int contextId, StackHashProduct product, StackHashEvent stackHashEvent, StackHashCab cab)
                : base(WorkerType.DownloadCab)
            {
                this.ContextId = contextId;
                this.Product = product;
                this.Event = stackHashEvent;
                this.Cab = cab;
            }
        }

        private class WorkerArgGetContextStatus : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgGetContextStatus(int contextId)
                : base(WorkerType.GetContextStatus)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgEnableDisableReporting : WorkerArg
        {
            public bool EnableReporting { get; private set; }

            public WorkerArgEnableDisableReporting(bool enableReporting)
                : base(WorkerType.EnableDisableReporting)
            {
                this.EnableReporting = enableReporting;
            }
        }

        private class WorkerArgCreateTestIndex : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashTestIndexData TestIndexData { get; private set; }

            public WorkerArgCreateTestIndex(int contextId, StackHashTestIndexData testIndexData)
                : base(WorkerType.CreateTestIndex)
            {
                this.ContextId = contextId;
                this.TestIndexData = testIndexData;
            }
        }

        private class WorkerArgTestDatabase : WorkerArg
        {
            public int ContextId { get; private set; }

            public WorkerArgTestDatabase(int contextId)
                : base(WorkerType.TestDatabase)
            {
                this.ContextId = contextId;
            }
        }

        private class WorkerArgGetPluginDiagnostics : WorkerArg
        {
            public int ContextId { get; private set; }
            public string PluginName { get; private set; }

            public WorkerArgGetPluginDiagnostics(int contextId, string pluginName)
                : base(WorkerType.GetPluginDiagnostics)
            {
                this.ContextId = contextId;
                this.PluginName = pluginName;
            }
        }

        private class WorkerArgSendProductToPlugin : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashProduct Product { get; private set; }
            public string PluginName { get; private set; }

            public WorkerArgSendProductToPlugin(int contextId, StackHashProduct product, string pluginName)
                : base(WorkerType.SendProductToPlugin)
            {
                this.ContextId = contextId;
                this.Product = product;
                this.PluginName = pluginName;
            }
        }

        private class WorkerArgSendEventToPlugin : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashProduct Product { get; private set; }
            public string PluginName { get; private set; }

            public WorkerArgSendEventToPlugin(int contextId, StackHashEvent theEvent, StackHashProduct product, string pluginName)
                : base(WorkerType.SendEventToPlugin)
            {
                this.ContextId = contextId;
                this.Event = theEvent;
                this.Product = product;
                this.PluginName = pluginName;
            }
        }

        private class WorkerArgSendCabToPlugin : WorkerArg
        {
            public int ContextId { get; private set; }
            public StackHashEvent Event { get; private set; }
            public StackHashProduct Product { get; private set; }
            public StackHashCab Cab { get; private set; }
            public string PluginName { get; private set; }

            public WorkerArgSendCabToPlugin(int contextId, StackHashEvent theEvent, StackHashProduct product, StackHashCab cab, string pluginName)
                : base(WorkerType.SendCabToPlugin)
            {
                this.ContextId = contextId;
                this.Event = theEvent;
                this.Product = product;
                this.Cab = cab;
                this.PluginName = pluginName;
            }
        }

        private class WorkerArgSendAllToPlugins : WorkerArg
        {
            public int ContextId { get; private set; }
            public string[] Plugins { get; private set; }

            public WorkerArgSendAllToPlugins(int contextId, string[] plugins)
                : base(WorkerType.SendAllToPlugins)
            {
                this.ContextId = contextId;
                this.Plugins = plugins;
            }
        }

        private class WorkerArgUploadMappingFile : WorkerArg
        {
            public int ContextId { get; private set; }
            public string MappingFileData { get; private set; }

            public WorkerArgUploadMappingFile(int contextId, string mappingFileData)
                : base(WorkerType.UploadMappingFile)
            {
                this.ContextId = contextId;
                this.MappingFileData = mappingFileData;
            }
        }

        #endregion

        #region WorkerResult Private Classes

        private class WorkerResult
        {
            private Exception _ex;
            private WorkerType _type;

            public WorkerResult(Exception ex, WorkerType type)
            {
                _ex = ex;
                _type = type;
            }

            public Exception Ex
            {
                get { return _ex; }
            }

            public WorkerType Type
            {
                get { return _type; }
            }
        }

        private class WorkerResultNoOp : WorkerResult
        {
            public WorkerResultNoOp()
                : base(null, WorkerType.NoOp) { }
        }

        private class WorkerResultServiceConnect : WorkerResult
        {
            public StackHashSettings ContextSettings { get; private set; }
            public StackHashStatus ServiceStatus { get; private set; }
            public StackHashScriptFileDataCollection ScriptDataCollection { get; private set; }
            public StackHashBugTrackerPlugInDiagnosticsCollection AvailablePlugIns { get; private set; }
            public string WebServiceOverrideAddress { get; private set; }
            public Guid LocalServiceGuid { get; private set; }
            public bool ServiceIsLocal { get; private set; }

            public WorkerResultServiceConnect(Exception ex,  
                StackHashSettings contextSettings, 
                StackHashStatus serviceStatus,
                StackHashScriptFileDataCollection scriptDataCollection,
                StackHashBugTrackerPlugInDiagnosticsCollection availablePlugIns,
                string webServiceOverrideAddress,
                Guid localServiceGuid,
                bool serviceIsLocal)
                : base(ex, WorkerType.ServiceConnect) 
            {
                this.ContextSettings = contextSettings;
                this.ServiceStatus = serviceStatus;
                this.ScriptDataCollection = scriptDataCollection;
                this.AvailablePlugIns = availablePlugIns;
                this.WebServiceOverrideAddress = webServiceOverrideAddress;
                this.LocalServiceGuid = localServiceGuid;
                this.ServiceIsLocal = serviceIsLocal;
            }
        }

        private class WorkerResultGetContextSettings : WorkerResult
        {
            private StackHashSettings _contextSettings;
            public StackHashStatus ServiceStatus { get; private set; }
            public StackHashBugTrackerPlugInDiagnosticsCollection AvailablePlugIns { get; private set; }

            public WorkerResultGetContextSettings(Exception ex, 
                StackHashSettings contextSettings, 
                StackHashStatus serviceStatus, 
                StackHashBugTrackerPlugInDiagnosticsCollection availablePlugIns)
                : base(ex, WorkerType.GetContextSettings)
            {
                _contextSettings = contextSettings;
                this.ServiceStatus = serviceStatus;
                this.AvailablePlugIns = availablePlugIns;
            }

            public StackHashSettings ContextSettings
            {
                get { return _contextSettings; }
            }
        }

        private class WorkerResultSetContextSettings : WorkerResult
        {
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }
            public StackHashMappingCollection WorkFlowMappings { get; private set; }

            public WorkerResultSetContextSettings(Exception ex, 
                StackHashCollectionPolicyCollection collectionPolicies,
                StackHashMappingCollection workFlowMappings)
                : base(ex, WorkerType.SetContextSettings)
            {
                this.CollectionPolicies = collectionPolicies;
                this.WorkFlowMappings = workFlowMappings;
            }
        }

        private class WorkerResultGetProductList : WorkerResult
        {
            public StackHashProductInfoCollection ProductCollection { get; private set; }
            public StackHashStatus ServiceStatus { get; private set; }
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }
            public DateTime LastWinQualSiteUpdate { get; private set; }
            public StackHashMappingCollection WorkFlowMappings { get; private set; }

            public WorkerResultGetProductList(Exception ex, 
                StackHashProductInfoCollection productCollection, 
                StackHashStatus serviceStatus,
                StackHashCollectionPolicyCollection collectionPolicies,
                DateTime lastWinQualSiteUpdate,
                StackHashMappingCollection workFlowMappings)
                : base(ex, WorkerType.GetProductList)
            {
                this.ProductCollection = productCollection;
                this.ServiceStatus = serviceStatus;
                this.CollectionPolicies = collectionPolicies;
                this.LastWinQualSiteUpdate = lastWinQualSiteUpdate;
                this.WorkFlowMappings = workFlowMappings;
            }
        }

        private class WorkerResultGetWindowedEventPackages : WorkerResult
        {
            public StackHashEventPackageCollection EventPackageCollection {get; private set;}
            public StackHashProduct Product { get; private set; }
            public List<StackHashFile> Files { get; private set; }
            public int EventsPage { get; private set; }
            public long TotalRows { get; private set; }
            public int EventsPerPage { get; private set; }
            public ClientLogicView LoadView { get; private set; }
            public long MinRow { get; private set; }
            public long MaxRow { get; private set; }

            public WorkerResultGetWindowedEventPackages(Exception ex,
                StackHashEventPackageCollection eventPackageCollection,
                StackHashProduct product,
                List<StackHashFile> files,
                int eventsPage,
                long maximumRowNumber,
                int eventsPerPage,
                ClientLogicView loadView,
                long minRow,
                long maxRow)
                : base(ex, WorkerType.GetWindowedEventPackages)
            {
                this.EventPackageCollection = eventPackageCollection;
                this.Product = product;
                this.Files = files;
                this.EventsPage = eventsPage;
                this.TotalRows = maximumRowNumber;
                this.EventsPerPage = eventsPerPage;
                this.LoadView = loadView;
                this.MinRow = minRow;
                this.MaxRow = maxRow;
            }
        }

        private class WorkerResultStartSync : WorkerResult
        {
            public WorkerResultStartSync(Exception ex)
                : base(ex, WorkerType.StartSync) { }
        }

        private class WorkerResultGetDebuggerScriptNames : WorkerResult
        {
            public StackHashScriptFileDataCollection ScriptData { get; private set; }

            public WorkerResultGetDebuggerScriptNames(Exception ex, StackHashScriptFileDataCollection scriptData)
                : base(ex, WorkerType.GetDebuggerScriptNames)
            {
                this.ScriptData = scriptData;
            }
        }

        private class WorkerResultGetScript : WorkerResult
        {
            private StackHashScriptSettings _scriptSettings;

            public WorkerResultGetScript(Exception ex, StackHashScriptSettings scriptSettings)
                : base(ex, WorkerType.GetScript)
            {
                _scriptSettings = scriptSettings;
            }

            public StackHashScriptSettings ScriptSettings
            {
                get { return _scriptSettings; }
            }
        }

        private class WorkerResultRemoveScript : WorkerResult
        {
            public WorkerResultRemoveScript(Exception ex)
                : base(ex, WorkerType.RemoveScript) { }
        }

        private class WorkerResultRenameScript : WorkerResult
        {
            public WorkerResultRenameScript(Exception ex)
                : base(ex, WorkerType.RenameScript) { }
        }

        private class WorkerResultAddScript : WorkerResult
        {
            public WorkerResultAddScript(Exception ex)
                : base(ex, WorkerType.AddScript) { }
        }

        private class WorkerResultTestScipt : WorkerResult
        {
            public WorkerResultTestScipt(Exception ex)
                : base(ex, WorkerType.TestScript) {}
        }

        private class WorkerResultGetResultFiles : WorkerResult
        {
            public StackHashScriptResultFiles ResultFiles { get; private set; }

            public WorkerResultGetResultFiles(Exception ex, StackHashScriptResultFiles resultFiles)
                : base(ex, WorkerType.GetResultFiles)
            {
                this.ResultFiles = resultFiles;
            }
        }

        private class WorkerResultGetResult : WorkerResult
        {
            public StackHashScriptResult ScriptResult { get; private set; }
            public StackHashEventPackage EventPackage { get; private set; }
            public StackHashScriptResultFiles ResultFiles { get; private set; }

            public WorkerResultGetResult(Exception ex, 
                StackHashScriptResult scriptResult, 
                StackHashEventPackage eventPackage,
                StackHashScriptResultFiles resultFiles)
                : base(ex, WorkerType.GetResult)
            {
                this.ScriptResult = scriptResult;
                this.EventPackage = eventPackage;
                this.ResultFiles = resultFiles;
            }
        }

        private class WorkerResultUpdateProductSummary : WorkerResult
        {
            public int ProductId { get; private set; }
            public StackHashProductHitDateSummaryCollection HitDateSummary { get; private set; }
            public StackHashProductLocaleSummaryCollection LocaleSummary { get; private set; }
            public StackHashProductOperatingSystemSummaryCollection OsSummary { get; private set; }

            public WorkerResultUpdateProductSummary(Exception ex,
                int productId,
                StackHashProductHitDateSummaryCollection hitDateSummary,
                StackHashProductLocaleSummaryCollection localeSummary,
                StackHashProductOperatingSystemSummaryCollection osSummary)
                : base(ex, WorkerType.UpdateProductSummary)
            {
                this.ProductId = productId;
                this.HitDateSummary = hitDateSummary;
                this.LocaleSummary = localeSummary;
                this.OsSummary = osSummary;
            }
        }

        private class WorkerResultAddGetEventNotes : WorkerResult
        {
            public StackHashNotes Notes { get; private set; }

            public WorkerResultAddGetEventNotes(Exception ex, StackHashNotes notes)
                : base(ex, WorkerType.AddGetEventNotes)
            {
                this.Notes = notes;
            }
        }

        private class WorkerResultAddGetCabNotes : WorkerResult
        {
            public StackHashNotes Notes { get; private set; }

            public WorkerResultAddGetCabNotes(Exception ex, StackHashNotes notes)
                : base(ex, WorkerType.AddGetCabNotes)
            {
                this.Notes = notes;
            }
        }

        private class WorkerResultResultOnlyRefreshContextSettings : WorkerResult
        {
            public StackHashStatus ServiceStatus { get; private set; }

            public WorkerResultResultOnlyRefreshContextSettings(Exception ex, StackHashStatus serviceStatus)
                : base(ex, WorkerType.ResultOnlyRefreshContextSettings) 
            {
                this.ServiceStatus = serviceStatus;
            }
        }

        private class WorkerResultResultOnlyNoAction : WorkerResult
        {
            public WorkerResultResultOnlyNoAction(Exception ex)
                : base(ex, WorkerType.ResultOnlyNoAction) { }
        }

        private class WorkerResultCreateNewStackHashContext : WorkerResult
        {
            public StackHashContextSettings Context { get; private set; }

            public WorkerResultCreateNewStackHashContext(Exception ex, StackHashContextSettings context)
                : base(ex, WorkerType.CreateNewStackHashContext)
            {
                this.Context = context;
            }
        }

        private class WorkerResultGetCabContents : WorkerResult
        {
            public string ExtractedCabFolder { get; private set; }

            public WorkerResultGetCabContents(Exception ex, string extractedCabFolder)
                : base(ex, WorkerType.GetCabContents)
            {
                this.ExtractedCabFolder = extractedCabFolder;
            }
        }

        private class WorkerResultGetCabFile : WorkerResult
        {
            public string FileContents { get; private set; }

            public WorkerResultGetCabFile(Exception ex, string fileContents)
                : base(ex, WorkerType.GetCabFile)
            {
                this.FileContents = fileContents;
            }
        }

        private class WorkerResultMoveIndex : WorkerResult
        {
            public WorkerResultMoveIndex(Exception ex)
                : base(ex, WorkerType.MoveIndex) { }
        }

        private class WorkerResultCopyIndex : WorkerResult
        {
            public WorkerResultCopyIndex(Exception ex)
                : base(ex, WorkerType.CopyIndex) { }
        }

        private class WorkerResultRemoveScriptResult : WorkerResult
        {
            public WorkerResultRemoveScriptResult(Exception ex)
                : base(ex, WorkerType.RemoveScriptResult) { }
        }

        private class WorkerResultCreateFirstContext : WorkerResult
        {
            public int ContextId { get; private set; }
            public StackHashSettings ContextSettings { get; private set; }

            public WorkerResultCreateFirstContext(Exception ex, int contextId, StackHashSettings contextSettings)
                : base(ex, WorkerType.CreateFirstContext) 
            {
                this.ContextId = contextId;
                this.ContextSettings = contextSettings;
            }
        }

        private class WorkerResultSetProductSyncState : WorkerResult
        {
            public StackHashProductInfoCollection ProductCollection { get; private set; }
            public DateTime LastWinQualSiteUpdate { get; private set; }

            public WorkerResultSetProductSyncState(Exception ex, 
                StackHashProductInfoCollection productCollection,
                DateTime lastWinQualSiteUpdate)
                : base(ex, WorkerType.SetProductSyncState)
            {
                this.ProductCollection = productCollection;
                this.LastWinQualSiteUpdate = lastWinQualSiteUpdate;
            }
        }

        private class WorkerResultRunWinQualLogOn : WorkerResult
        {
            public WorkerResultRunWinQualLogOn(Exception ex)
                : base(ex, WorkerType.RunWinQualLogOn) { }
        }

        private class WorkerResultRefeshEventPackage : WorkerResult
        {
            public StackHashEventPackage EventPackage { get; private set; }

            public WorkerResultRefeshEventPackage(Exception ex, StackHashEventPackage eventPackage)
                : base(ex, WorkerType.RefreshEventPackage)
            {
                this.EventPackage = eventPackage;
            }
        }

        private class WorkerResultSetProxy : WorkerResult
        {
            public StackHashStatus ServiceStatus { get; private set; }
            public StackHashContextCollection ContextCollection { get; private set; }
            public int ContextId { get; private set; }

            public WorkerResultSetProxy(Exception ex, StackHashStatus status, int contextId, StackHashContextCollection contextCollection)
                : base(ex, WorkerType.SetProxy) 
            {
                this.ServiceStatus = status;
                this.ContextId = contextId;
                this.ContextCollection = contextCollection;
            }
        }

        private class WorkerResultSetEventBugId : WorkerResult
        {
            public WorkerResultSetEventBugId(Exception ex)
                : base(ex, WorkerType.SetEventBugId) { }
        }

        private class WorkerResultSetEventWorkFlow : WorkerResult
        {
            public WorkerResultSetEventWorkFlow(Exception ex)
                : base(ex, WorkerType.SetEventWorkFlow) { }
        }

        private class WorkerResultAbortSync : WorkerResult
        {
            public WorkerResultAbortSync(Exception ex)
                : base(ex, WorkerType.AbortSync) { }
        }

        private class WorkerResultExport : WorkerResult
        {
            public WorkerResultExport(Exception ex)
                : base(ex, WorkerType.Export) { }
        }

        private class WorkerResultGetCollectionPolicies : WorkerResult
        {
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }

            public WorkerResultGetCollectionPolicies(Exception ex, StackHashCollectionPolicyCollection collectionPolicies)
                : base(ex, WorkerType.GetCollectionPolicies)
            {
                this.CollectionPolicies = collectionPolicies;
            }
        }

        private class WorkerResultUpdateProductProperties : WorkerResult
        {
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }

            public WorkerResultUpdateProductProperties(Exception ex, StackHashCollectionPolicyCollection collectionPolicies)
                : base(ex, WorkerType.UpdateProductProperties)
            {
                this.CollectionPolicies = collectionPolicies;
            }
        }

        private class WorkerResultUpdateEventPackageProperties : WorkerResult
        {
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }

            public WorkerResultUpdateEventPackageProperties(Exception ex, StackHashCollectionPolicyCollection collectionPolicies)
                : base(ex, WorkerType.UpdateEventPackageProperties)
            {
                this.CollectionPolicies = collectionPolicies;
            }
        }

        private class WorkerResultSetCollectionPolicies : WorkerResult
        {
            public StackHashCollectionPolicyCollection CollectionPolicies { get; private set; }

            public WorkerResultSetCollectionPolicies(Exception ex, StackHashCollectionPolicyCollection collectionPolicies)
                : base(ex, WorkerType.SetCollectionPolicies)
            {
                this.CollectionPolicies = collectionPolicies;
            }
        }

        private class WorkerResultDownloadCab : WorkerResult
        {
            public WorkerResultDownloadCab(Exception ex)
                : base(ex, WorkerType.DownloadCab) { }
        }

        private class WorkerResultGetContextStatus : WorkerResult
        {
            public StackHashContextStatus ContextStatus { get; private set; }

            public WorkerResultGetContextStatus(Exception ex, StackHashContextStatus contextStatus)
                : base(ex, WorkerType.GetContextStatus)
            {
                this.ContextStatus = contextStatus;
            }
        }

        private class WorkerResultEnableDisableReporting : WorkerResult
        {
            public bool EnableReporting { get; private set; }

            public WorkerResultEnableDisableReporting(Exception ex, bool enableReporting)
                : base(ex, WorkerType.EnableDisableReporting) 
            {
                this.EnableReporting = enableReporting;
            }
        }

        private class WorkerResultCreateTestIndex : WorkerResult
        {
            public WorkerResultCreateTestIndex(Exception ex)
                : base(ex, WorkerType.CreateTestIndex) { }
        }

        private class WorkerResultTestDatabase : WorkerResult
        {
            public StackHashErrorIndexDatabaseStatus TestStatus { get; set; }
            public string TestLastExceptionText { get; set; }

            public WorkerResultTestDatabase(Exception ex, StackHashErrorIndexDatabaseStatus testStatus, string testLastExceptionText)
                : base(ex, WorkerType.TestDatabase)
            {
                this.TestStatus = testStatus;
                this.TestLastExceptionText = testLastExceptionText;
            }
        }

        private class WorkerResultGetPluginDiagnostics : WorkerResult
        {
            public StackHashNameValueCollection Diagnostics { get; private set; }

            public WorkerResultGetPluginDiagnostics(Exception ex, StackHashNameValueCollection diagnostics)
                : base(ex, WorkerType.GetPluginDiagnostics)
            {
                this.Diagnostics = diagnostics;
            }
        }

        private class WorkerResultSendEventToPlugin : WorkerResult
        {
            public WorkerResultSendEventToPlugin(Exception ex)
                : base(ex, WorkerType.SendEventToPlugin) { }
        }

        private class WorkerResultSendProductToPlugin : WorkerResult
        {
            public WorkerResultSendProductToPlugin(Exception ex)
                : base(ex, WorkerType.SendProductToPlugin) { }
        }

        private class WorkerResultSendCabToPlugin : WorkerResult
        {
            public WorkerResultSendCabToPlugin(Exception ex)
                : base(ex, WorkerType.SendCabToPlugin) { }
        }

        private class WorkerResultSendAllToPlugins : WorkerResult
        {
            public WorkerResultSendAllToPlugins(Exception ex)
                : base(ex, WorkerType.SendAllToPlugins) { }
        }

        private class WorkerResultUploadMappingFile : WorkerResult
        {
            public WorkerResultUploadMappingFile(Exception ex)
                : base(ex, WorkerType.UploadMappingFile) { }
        }

        #endregion

        /// <summary>
        /// True if ClientLogic has been disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        private const int AdminReregisterTimeoutMS = 3600000; // 1 hour
        private const int CabCopyBufferSize = 32768;
        private const int WaitForWorkerTimeoutMS = 5000; // 5 seconds
        private const int WaitForWorkerSleepMS = 250;
        private const int FaultedServiceCallRetryLimit = 4;
        private const int LicenseExpiryWarningDays = 7;
        private const int DefaultSqlMinPoolSize = 6;
        private const int DefaultSqlMaxPoolSize = 100;
        private const int DefaultSqlConnectionTimeout = 15;
        private const int DefaultSqlEventsPerBlock = 100;
        private const string LocalServiceGuidPath = @"StackHash\ServiceInstanceData.txt";

        private const string ExportElementProductRollup = "ProductRollupData";
        private const string ExportElementRow = "Row";
        private const string ExportElementProductName = "ProductName";
        private const string ExportElementProductVersion = "ProductVersion";
        private const string ExportElementTotalEvents = "TotalEvents";
        private const string ExportElementTotalResponses = "TotalResponses";
        private const string ExportElementDateCreated = "DateCreatedUTC";
        private const string ExportElementDateModified = "DateModifiedUTC";
        private const string ExportElementProductID = "ProductID";
        private const string ExportElementLastSyncUTC = "StackHashLastSyncUTC";
        private const string ExportElementSyncEnabled = "StackHashSyncEnabled";
        private const string ExportElementEventList = "EventListData";
        private const string ExportElementEventType = "EventType";
        private const string ExportElementActiveResponse = "ActiveResponse";
        private const string ExportElementTotalHits = "TotalHits";
        private const string ExportElementAvgHits = "AvgHits";
        private const string ExportElementApplicationName = "ApplicationName";
        private const string ExportElementApplicationVersion = "ApplicationVersion";
        private const string ExportElementModuleName = "ModuleName";
        private const string ExportElementModuleVersion = "ModuleVersion";
        private const string ExportElementOffset = "Offset";
        private const string ExportElementEventID = "EventID";
        private const string ExportElementGrowthPercent = "GrowthPercent";
        private const string ExportElementReference = "StackHashReference";
        private const string ExportElementApplicationTimestamp = "ApplicationTimestamp";
        private const string ExportElementModeuleTimestamp = "ModuleTimestamp";
        private const string ExportElementExceptionCode = "ExceptionCode";
        private const string ExportElementCab = "Cab";
        private const string ExportElementCabs = "Cabs";
        private const string ExportElementCabID = "CabID";
        private const string ExportElementEventInfo = "EventInfo";
        private const string ExportElementEventInfos = "EventInfos";
        private const string ExportElementLanguage = "Language";
        private const string ExportElementSizeInBytes = "SizeInBytes";
        private const string ExportElementDotNetVersion = "DotNetVersion";
        private const string ExportElementMachineArchitecture = "MachineArchitecture";
        private const string ExportElementOSVersion = "OSVersion";
        private const string ExportElementProcessUptime = "ProcessUptime";
        private const string ExportElementSystemUptime = "SystemUptime";
        private const string ExportElementHitDate = "HitDateUTC";
        private const string ExportElementLcid = "Lcid";
        private const string ExportElementLocale = "Locale";
        private const string ExportElementOSName = "OSName";
        
        private Dispatcher _guiDispatcher;
        private bool _initialServiceConnectComplete;
        private bool _initialServiceRegisterComplete;
        private string _initialServiceConnectHost;
        private int _initialServiceConnectPort;
        private BackgroundWorker _worker;
        private int _contextId;
        private StackHashContextSettings _newContext;
        private string _pendingScriptName;
        private Dictionary<int, int> _productIdToProductsIndex;
        private Dictionary<Tuple<int, string>, int> _eventIdToEventsIndex;
        private Dictionary<int, StackHashFile> _fileIdToFile;
        private Dictionary<int, DisplayMapping> _workFlowIdToMapping;
        private int _lastEventPackageListUpdateProductId;
        private List<StackHashCollectionPolicy> _collectionPolicies;
        private Timer _registerTimer;
        
        private string _statusText;
        private bool _notBusy;
        private string _eventListTitle;
        private ObservableCollection<DisplayContext> _contextCollection;
        private ObservableCollection<DisplayContext> _activeContextCollection;
        private ObservableCollection<DisplayProduct> _products;
        private ObservableCollection<DisplayEventPackage> _eventPackages;
        private ObservableCollection<StackHashScriptFileData> _scriptData;
        private ObservableCollection<DisplayScriptResult> _scriptResultFiles;
        private ObservableCollection<StackHashNoteEntry> _currentEventNotes;
        private ObservableCollection<StackHashNoteEntry> _currentCabNotes;
        private ObservableCollection<StackHashBugTrackerPlugInDiagnostics> _availablePlugIns;
        private ObservableCollection<StackHashBugTrackerPlugIn> _activePlugins;
        private ObservableCollection<DisplayMapping> _workFlowMappings;
        private StackHashProxySettings _serviceProxySettings;
        private int _clientTimeoutInSeconds;
        private DisplayContext _currentContext;
        private DisplayProduct _currentProduct;
        private ClientLogicView _currentView;
        private DisplayEventPackage _currentEventPackage;
        private DisplayCab _currentCab;
        private StackHashCabPackage _currentCabPackage;
        private string _currentCabFolder;
        private StackHashNameValueCollection _currentPluginDiagnostics;
        private StackHashScriptSettings _currentScript;
        private StackHashScriptResult _currentScriptResult;
        private StackHashContextStatus _contextStatus;
        private StackHashStatus _serviceStatus;
        private string _currentCabFileContents;
        private string _mainWindowTitle;
        private bool _serviceLogEnabled;
        private bool _serviceReportingEnabled;
        private bool _anyEnabledProducts;
        private string _serviceStatusText;
        private string _syncStageText;
        private string _moveProgressText;
        private bool _syncRunning;
        private bool _syncIsResync;
        private bool _syncRunningUpdatedByAdminReport;
        private bool _analyzeRunning;
        private bool _analyzeRunningUpdatedByAdminReport;
        private bool _purgeRunning;
        private bool _purgeRunningUpdatedByAdminReport;
        private bool _pluginReportRunning;
        private bool _pluginReportRunningUpdatedByAdminReport;
        private bool _moveIndexRunning;
        private bool _moveIndexRunningUpdatedByAdminReport;
        private object _serviceStateLockObject;
        private object _productListLockObject;
        private object _otherClientContextStateLock;
        private bool _otherClientHasDisabledContext;
        private bool _updateProductListAfterSync;
        private bool _activateContextAfterMove;
        private int _activateContextAfterMoveContextId;
        private bool _activateContextAfterCopy;
        private int _activateContextAfterCopyContextId;
        private StackHashSearchCriteriaCollection _lastPopulateEventsSearch;
        private StackHashSortOrderCollection _lastPopulateEventsSort;
        private DisplayProduct _lastPopulateEventsProduct;
        private int _lastPopulateEventsPage;
        private int _lastPopulateEventsEventsPerPage;
        private int _lastPopulateEventsDisplayThreshold;
        private ClientLogicView _lastPopulateEventsLoadView;
        private StackHashSearchCriteriaCollection _lastEventsSearch;
        private StackHashSortOrderCollection _lastEventsSort;
        private long _lastPopulateEventsMinRow;
        private long _lastPopulateEventsMaxRow;
        private long _lastPopulateEventsTotalRows;
        private int _currentEventsPage;
        private int _currentEventsMaxPage;
        private int _serviceProgress;
        private bool _serviceProgressVisible;
        private string _serviceHost;
        private bool _serviceIsLocal;
        private StackHashErrorIndexDatabaseStatus _lastDatabaseTestStatus;
        private string _lastDatabaseTestExceptionText;
        private bool _refreshEventOnPluginReportComplete;
        private bool _pluginHasError;
        private bool _clientIsBumped;
        private DateTime _lastWinQualSiteUpdate;

        /// <summary>
        /// Event fired when an error occurs in the ClientLogic
        /// </summary>
        public event EventHandler<ClientLogicErrorEventArgs> ClientLogicError;

        /// <summary>
        /// Event fired when UI is needed / ready
        /// </summary>
        public event EventHandler<ClientLogicUIEventArgs> ClientLogicUI;

        /// <summary>
        /// Event fired to notify the setup wizard that an operation of interest has completed
        /// </summary>
        public event EventHandler<ClientLogicSetupWizardPromptEventArgs> ClientLogicSetupWizardPrompt;

        /// <summary>
        /// Event fired when sync completes
        /// </summary>
        public event EventHandler<SyncCompleteEventArgs> SyncComplete;

        /// <summary>
        /// Event fired when client logic has updated its context Id
        /// </summary>
        public event EventHandler ClientLogicContextIdChanged;

        /// <summary>
        /// StackHash Client Logic
        /// </summary>
        /// <param name="guiDispatcher">GUI Dispatcher</param>
        public ClientLogic(Dispatcher guiDispatcher)
        {
            if (guiDispatcher == null) { throw new ArgumentNullException("guiDispatcher"); }

            this.StatusText = Properties.Resources.Status_Startup;
            this.CurrentView = ClientLogicView.ProductList;
            this.NotBusy = true;

            _serviceStateLockObject = new object();
            _productListLockObject = new object();
            _otherClientContextStateLock = new object();
            _lastEventPackageListUpdateProductId = -1;

            _guiDispatcher = guiDispatcher;

            _mainWindowTitle = Properties.Resources.DefaultMainWindowTitle;

            ServiceProxy.Services.AdminReport += new EventHandler<AdminReportEventArgs>(Services_AdminReport);

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);

            _contextId = UserSettings.InvalidContextId;

            _registerTimer = new Timer(new TimerCallback(RegisterTimerCallback), null, AdminReregisterTimeoutMS, AdminReregisterTimeoutMS);
        }

        /// <summary>
        /// Causes ClientLogic to cycle NotBusy
        /// </summary>
        public void NoOp()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_NoOp;

                _worker.RunWorkerAsync(new WorkerArgNoOp());
            }
        }

        /// <summary>
        /// Connects to the StackHash service
        /// </summary>
        public void AdminServiceConnect()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_ServiceConnect,
                    ServiceProxy.Services.ServiceHost,
                    ServiceProxy.Services.ServicePort);

                _worker.RunWorkerAsync(new WorkerArgServiceConnect());
            }
        }

        /// <summary>
        /// Enable service logging
        /// </summary>
        public void AdminEnableLogging()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_EnableLogging;

                _worker.RunWorkerAsync(new WorkerArgEnableLogging());
            }
        }

        /// <summary>
        /// Disable service logging
        /// </summary>
        public void AdminDisableLogging()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_DisableLogging;

                _worker.RunWorkerAsync(new WorkerArgDisableLogging());
            }
        }

        /// <summary>
        /// Enables or disables service reporting
        /// </summary>
        /// <param name="enableReporting">True to enable, false to disable</param>
        public void AdminUpdateReportingState(bool enableReporting)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_EnableDisableReporting;

                _worker.RunWorkerAsync(new WorkerArgEnableDisableReporting(enableReporting));
            }
        }

        /// <summary>
        /// Request the creation of a new context
        /// </summary>
        public void AdminCreateNewContext()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_NewContext;

                _worker.RunWorkerAsync(new WorkerArgCreateNewStackHashContext());
            }
        }

        /// <summary>
        /// Collects new context settings (after a call to AdminCreateNewContext)
        /// </summary>
        /// <returns>New context settings</returns>
        public StackHashContextSettings AdminCollectNewContext()
        {
            Debug.Assert(_newContext != null);

            StackHashContextSettings ret = _newContext;
            _newContext = null;

            return ret;
        }

        /// <summary>
        /// Creates (and updates) the fist context - call only from the setup wizard
        /// </summary>
        /// <param name="contextName">The context database name</param>
        /// <param name="username">WinQual username</param>
        /// <param name="password">WinQual password</param>
        /// <param name="cdbPath32">Path to 32-bit cdb.exe (optional)</param>
        /// <param name="cdbPath64">Path to 64-bit cdb.exe (optional)</param>
        /// <param name="indexFolder">Path to the index folder</param>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="profileName">Profile display name</param>
        /// <param name="isDatabaseInIndexFolder">True if the database is stored in the index folder</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "cdb"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username")]
        public void AdminCreateFirstContext(string contextName,
            string username,
            string password,
            string cdbPath32,
            string cdbPath64,
            string indexFolder,
            string connectionString,
            string profileName,
            bool isDatabaseInIndexFolder)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextName != null);
            Debug.Assert(username != null);
            Debug.Assert(password != null);
            Debug.Assert(indexFolder != null);
            Debug.Assert(connectionString != null);
            Debug.Assert(profileName != null);
            // cdbPath32 and cdbPath64 may be null

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_FirstContext;

                _worker.RunWorkerAsync(new WorkerArgCreateFirstContext(contextName,
                    username,
                    password,
                    cdbPath32,
                    cdbPath64,
                    indexFolder,
                    connectionString,
                    profileName,
                    isDatabaseInIndexFolder));
            }
        }

        /// <summary>
        /// Tests a WinQual username and password
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="userName">UserName to test</param>
        /// <param name="password">Password to test</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Qual")]
        public void AdminTestWinQualLogOn(int contextId, string userName, string password)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);
            if (userName == null) { throw new ArgumentNullException("userName"); }
            if (password == null) { throw new ArgumentNullException("password"); }

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_RunWinQualLogOn;

                _worker.RunWorkerAsync(new WorkerArgRunWinQualLogOn(contextId, userName, password));
            }
        }

        /// <summary>
        /// Activates a context
        /// </summary>
        /// <param name="contextId">Context ID to activate</param>
        public void AdminActivateContext(int contextId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_ActivateProfile;

                _worker.RunWorkerAsync(new WorkerArgActivateStackHashContext(contextId));
            }
        }

        /// <summary>
        /// Deactivate a context
        /// </summary>
        /// <param name="contextId">Context ID to deactivate</param>
        public void AdminDeactivateContext(int contextId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_DeactivateProfile;

                _worker.RunWorkerAsync(new WorkerArgDeactivateStackHashContext(contextId));
            }
        }

        /// <summary>
        /// Remove a context
        /// </summary>
        /// <param name="contextId">Context ID to remove</param>
        public void AdminRemoveContext(int contextId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_DeleteProfile;

                _worker.RunWorkerAsync(new WorkerArgRemoveStackHashContext(contextId));
            }
        }

        /// <summary>
        /// Refreshes the list of context (profile) settings - ContextSettings
        /// </summary>
        public void RefreshContextSettings()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_LoadProfiles;

                _worker.RunWorkerAsync(new WorkerArgGetContextSettings());
            }
        }

        /// <summary>
        /// Saves context settings
        /// </summary>
        /// <param name="contextId">The context id</param>
        /// <param name="settings">Settings to set</param>
        /// <param name="collectionPolicies">Collection policies to set (optional)</param>
        /// <param name="plugIns">PlugIns to set (optional)</param>
        /// <param name="workFlowMappings">WorkFlow mappings to set (optional)</param>
        /// <param name="isActive">True if the context is currently active</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "workFlow"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void SaveContextSettings(int contextId,
            StackHashContextSettings settings, 
            StackHashCollectionPolicyCollection collectionPolicies,
            StackHashBugTrackerPlugInCollection plugIns,
            StackHashMappingCollection workFlowMappings,
            bool isActive)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);
            Debug.Assert(settings != null);
            // OK for collectionPolicies, plugIns and workFlowMappings to be null

            if (!_worker.IsBusy)
            {
                StackHashSettings saveSettings = new StackHashSettings();
                saveSettings.ContextCollection = new StackHashContextCollection();
                saveSettings.ContextCollection.Add(settings);

                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_SaveProfile;
                _worker.RunWorkerAsync(new WorkerArgSetContextSettings(contextId, 
                    saveSettings, 
                    collectionPolicies, 
                    plugIns, 
                    workFlowMappings,
                    isActive));
            }
        }

        /// <summary>
        /// Refreshes data collection policies for the current context
        /// </summary>
        public void AdminRefreshCollectionPolicies()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_RefreshCollectionPolicies;

                _worker.RunWorkerAsync(new WorkerArgGetCollectionPolicies(_contextId));
            }
        }

        /// <summary>
        /// Gets/updates the context status for the current context
        /// </summary>
        public void AdminGetContextStatus()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_GetContextStatus;

                _worker.RunWorkerAsync(new WorkerArgGetContextStatus(_contextId));
            }
        }

        /// <summary>
        /// Updates the sync state for a product in the current context and then refreshes the list of products
        /// </summary>
        /// <param name="productId">Product Id to update</param>
        /// <param name="syncState">True to sync, false to ignore</param>
        public void AdminSetProductSyncState(int productId, bool syncState)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SetProductSyncState,
                    syncState ? Properties.Resources.Enabling : Properties.Resources.Disabling,
                    DisplayNameForProductId(productId));

                _worker.RunWorkerAsync(new WorkerArgSetProductSyncState(_contextId, productId, syncState));
            }
        }

        /// <summary>
        /// Request that the StackHash service starts synchronization of the current context
        /// </summary>
        /// <param name="forceResync">True to force a full resync</param>
        /// <param name="productsToSync">List of product Ids to sync, null to sync all</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resync")]
        public void AdminStartSync(bool forceResync, List<int> productsToSync)
        {
            AdminStartSync(forceResync, false, false, productsToSync);
        }

        /// <summary>
        /// Request that the StackHash service starts synchronization of the current context
        /// </summary>
        /// <param name="forceResync">True to force a full resync</param>
        /// <param name="updateProductList">True to update the product list after the sync completes</param>
        /// <param name="productsOnly">True to only update the product list</param>
        /// <param name="productsToSync">List of product Ids to sync, null to sync all</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resync")]
        public void AdminStartSync(bool forceResync, bool updateProductList, bool productsOnly, List<int> productsToSync)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            // OK for productsToSync to be null

            if ((!_worker.IsBusy) && (!this.SyncRunning))
            {
                int currentContextId = UserSettings.Settings.CurrentContextId;

                // in case the setup wizard changed the initial context Id before closing...
                if (_contextId != currentContextId)
                {
                    _contextId = currentContextId;
                    UpdateCurrentContext();
                    this.Products = null;
                }

                _updateProductListAfterSync = updateProductList;

                _syncIsResync = forceResync;

                this.NotBusy = false;
                this.StatusText = forceResync ? Properties.Resources.Status_RequestWinQualResync : Properties.Resources.Status_RequestWinQualSync;
                _worker.RunWorkerAsync(new WorkerArgStartSync(_contextId, forceResync, productsOnly, productsToSync));
            }
        }

        /// <summary>
        /// Request that the current synchronization is canceled
        /// </summary>
        public void AdminCancelSync()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.SyncRunning);

            if ((!_worker.IsBusy) && (this.SyncRunning))
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_CancelSync;

                _worker.RunWorkerAsync(new WorkerArgAbortSync(_contextId));
            }
        }

        /// <summary>
        /// Gets or updates the list of debugger script names
        /// </summary>
        public void AdminGetScriptNames()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_ListScripts;
                _worker.RunWorkerAsync(new WorkerArgGetDebuggerScriptNames());
            }
        }

        /// <summary>
        /// Gets a debugger script
        /// </summary>
        /// <param name="scriptName">The name of the script to retrieve</param>
        public void AdminGetScript(string scriptName)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(!string.IsNullOrEmpty(scriptName));

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_LoadScript,
                    scriptName);

                _worker.RunWorkerAsync(new WorkerArgGetScript(scriptName));
            }
        }

        /// <summary>
        /// Adds a debugger script
        /// </summary>
        /// <param name="scriptSettings">The script to add</param>
        /// <param name="originalName">The original name of the script (a rename will be 
        /// performed if the original name does not match the current name)</param>
        /// <param name="overwrite">True to overwrite</param>
        public void AdminAddScript(StackHashScriptSettings scriptSettings, string originalName, bool overwrite)
        {
            WaitForWorker();
            if (scriptSettings == null) { throw new ArgumentNullException("scriptSettings"); }
            if (originalName == null) { throw new ArgumentNullException("originalName"); }

            Debug.Assert(!_worker.IsBusy);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SaveScript,
                    scriptSettings.Name);

                _worker.RunWorkerAsync(new WorkerArgAddScript(scriptSettings, originalName, overwrite));
            }
        }

        /// <summary>
        /// Removes a debugger script
        /// </summary>
        /// <param name="scriptName">The name of the script to remove</param>
        public void AdminRemoveScript(string scriptName)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(!string.IsNullOrEmpty(scriptName));

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_DeleteScript,
                    scriptName);

                _worker.RunWorkerAsync(new WorkerArgRemoveScript(scriptName));
            }
        }

        /// <summary>
        /// Renames a debugger script
        /// </summary>
        /// <param name="currentName">The current name of the script</param>
        /// <param name="newName">The new name of the script</param>
        public void AdminRenameScript(string currentName, string newName)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(!string.IsNullOrEmpty(currentName));
            Debug.Assert(!string.IsNullOrEmpty(newName));

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_RenameScript,
                    currentName,
                    newName);

                _worker.RunWorkerAsync(new WorkerArgRenameScript(currentName, newName));
            }
        }

        /// <summary>
        /// Tests a script on the currently selected cab
        /// </summary>
        /// <param name="scriptName">The name of the script to run</param>
        public void AdminTestScript(string scriptName)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(!string.IsNullOrEmpty(scriptName));
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentProduct != null);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);

            // used to retrive results
            _pendingScriptName = scriptName;

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_RequestScriptRun,
                    scriptName);

                _worker.RunWorkerAsync(new WorkerArgTestScript(_contextId, 
                    this.CurrentProduct.StackHashProductInfo.Product, 
                    this.CurrentEventPackage.StackHashEventPackage.EventData, 
                    this.CurrentCab.StackHashCabPackage.Cab, 
                    scriptName));
            }
        }

        /// <summary>
        /// Gets the list of result files for the currently selected cab
        /// </summary>
        public void AdminGetResultFiles()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentProduct != null);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_ListScriptResults;
                _worker.RunWorkerAsync(new WorkerArgGetResultFiles(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab));
            }
        }

        /// <summary>
        /// Gets a script result for the currently selected cab
        /// </summary>
        /// <param name="scriptName">The name of the script to get results for</param>
        public void AdminGetResult(string scriptName)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(!string.IsNullOrEmpty(scriptName));
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentProduct != null);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);

            if (!_worker.IsBusy)
            {
                this.CurrentScriptResult = null;

                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_LoadScriptResult,
                    scriptName);

                _worker.RunWorkerAsync(new WorkerArgGetResult(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab,
                    scriptName));
            }
        }

        /// <summary>
        /// Removes the result for a script for the currently selected cab
        /// </summary>
        /// <param name="scriptName">The name of the script to remove the result for</param>
        public void AdminRemoveResult(string scriptName)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(!string.IsNullOrEmpty(scriptName));
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentProduct != null);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_RemoveScriptResult,
                    scriptName);

                _worker.RunWorkerAsync(new WorkerArgRemoveScriptResult(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab,
                    scriptName));
            }
        }

        /// <summary>
        /// Moves an index (use if the index type is the same, to change the index type
        /// use AdminCopyIndex)
        /// </summary>
        /// <param name="contextId">The context Id of the index to move</param>
        /// <param name="newPath">The new path for the index</param>
        /// <param name="newName">The new name for the index</param>
        /// <param name="newConnectionString">The new connection string for the index</param>
        /// <param name="newInitialCatalog">The new initial catalog for the index</param>
        /// <param name="isActive">True if the context is currently active</param>
        /// <param name="minPoolSize">SQL Min Pool Size</param>
        /// <param name="maxPoolSize">SQL Max Pool Size</param>
        /// <param name="eventsPerBlock">SQL Events Per Block</param>
        /// <param name="connectionTimeout">SQL Connection Timeout</param>
        public void AdminMoveIndex(int contextId, 
            string newPath, 
            string newName,
            string newConnectionString, 
            string newInitialCatalog, 
            bool isActive,
            int minPoolSize,
            int maxPoolSize,
            int connectionTimeout,
            int eventsPerBlock)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);
            Debug.Assert(!string.IsNullOrEmpty(newPath));
            Debug.Assert(!string.IsNullOrEmpty(newName));
            Debug.Assert(!string.IsNullOrEmpty(newConnectionString));
            Debug.Assert(!string.IsNullOrEmpty(newInitialCatalog));

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_MoveIndex,
                    newPath,
                    newName);

                _worker.RunWorkerAsync(new WorkerArgMoveIndex(contextId,
                    newPath,
                    newName,
                    newConnectionString,
                    newInitialCatalog,
                    isActive,
                    minPoolSize,
                    maxPoolSize,
                    connectionTimeout,
                    eventsPerBlock));
            }
        }

        /// <summary>
        /// Copies an index (required if the index type is changing, currently only supported
        /// for migrating an XML index to a SQL index)
        /// </summary>
        /// <param name="contextId">The context Id of the index to copy</param>
        /// <param name="newPath">The new path for the index</param>
        /// <param name="newName">The new name for the index</param>
        /// <param name="newConnectionString">The new connection string for the index</param>
        /// <param name="newInitialCatalog">The new initial catalog for the index</param>
        /// <param name="isActive">True if the context is currently active</param>
        /// <param name="isDatabaseInCabFolder">True if the database is stored in the cab folder (informational - this can't change)</param>
        public void AdminCopyIndex(int contextId, 
            string newPath, 
            string newName, 
            string newConnectionString, 
            string newInitialCatalog, 
            bool isActive,
            bool isDatabaseInCabFolder)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);
            Debug.Assert(!string.IsNullOrEmpty(newPath));
            Debug.Assert(!string.IsNullOrEmpty(newName));
            Debug.Assert(!string.IsNullOrEmpty(newConnectionString));
            Debug.Assert(!string.IsNullOrEmpty(newInitialCatalog));

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_CopyIndex,
                    newPath,
                    newName,
                    newInitialCatalog);

                _worker.RunWorkerAsync(new WorkerArgCopyIndex(contextId,
                    newPath,
                    newName,
                    newConnectionString,
                    newInitialCatalog,
                    isActive,
                    isDatabaseInCabFolder));
            }
        }

        /// <summary>
        /// Updates the Service proxy settings from the ServiceProxySettings property
        /// and client timeout from the ClientTimeoutInSeconds property
        /// </summary>
        public void AdminUpdateServiceProxySettingsAndClientTimeout()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(this.ServiceProxySettings != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_UpdateProxySettings;

                _worker.RunWorkerAsync(new WorkerArgSetProxy(this.ServiceProxySettings, this.ClientTimeoutInSeconds, _contextId, this.ServiceIsLocal));
            }
        }

        /// <summary>
        /// Test database connectivity for a context Id
        /// </summary>
        /// <param name="contextId">Context Id to test</param>
        public void AdminTestDatabase(int contextId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_TestDatabase;

                _worker.RunWorkerAsync(new WorkerArgTestDatabase(contextId));
            }
        }

        /// <summary>
        /// Gets context specific diagnostics for a plugin
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="pluginName">Plugin Name</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin")]
        public void AdminGetPluginDiagnostics(int contextId, string pluginName)
        {
            if (pluginName == null) { throw new ArgumentNullException("pluginName"); }

            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.InvariantCulture,
                    Properties.Resources.Status_GetPluginDiagnostics,
                    pluginName);

                _worker.RunWorkerAsync(new WorkerArgGetPluginDiagnostics(contextId, pluginName));
            }
        }

        /// <summary>
        /// Upload a mapping file to WinQual
        /// </summary>
        /// <param name="mappingFileData">Contents of the mapping file</param>
        public void AdminUploadMappingFile(string mappingFileData)
        {
            if (mappingFileData == null) { throw new ArgumentNullException("mappingFileData"); }

            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_RequestMappingUpload;

                _worker.RunWorkerAsync(new WorkerArgUploadMappingFile(_contextId, mappingFileData));
            }
        }

        /// <summary>
        /// Clears all selected items and the event list (does not clear the product list)
        /// </summary>
        public void ClearSelections()
        {
            this.CurrentCab = null;
            this.CurrentCabPackage = null;
            this.CurrentCabFileContents = null;
            this.CurrentCabNotes = null;
            this.CurrentEventNotes = null;
            this.CurrentEventPackage = null;
            this.EventPackages = null;
            this.CurrentProduct = null;
            this.ScriptResultFiles = null;
        }

        /// <summary>
        /// Refreshes the current view
        /// </summary>
        public void RefreshCurrentView()
        {
            // connect to the service if necessary
            if (!_initialServiceConnectComplete)
            {
                AdminServiceConnect();
            }

            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                switch (this.CurrentView)
                {
                    case ClientLogicView.ProductList:
                        PopulateProducts();
                        break;

                    case ClientLogicView.EventList:
                        if ((_lastPopulateEventsProduct != null) || (_lastPopulateEventsSearch != null))
                        {
                            PopulateEventPackagesNoAdjustThreshold(_lastPopulateEventsProduct,
                                _lastPopulateEventsSearch,
                                _lastPopulateEventsSort,
                                _lastPopulateEventsPage,
                                _lastPopulateEventsEventsPerPage,
                                _lastPopulateEventsDisplayThreshold,
                                _lastPopulateEventsLoadView,
                                PageIntention.Reload);
                        }
                        break;

                    case ClientLogicView.EventDetail:
                        if ((this.CurrentProduct != null) && (this.CurrentEventPackage != null))
                        {
                            RefreshEventPackage();
                        }
                        break;

                    case ClientLogicView.CabDetail:
                        if ((this.CurrentProduct != null) && (this.CurrentEventPackage != null) && (this.CurrentCab != null))
                        {
                            AdminGetResultFiles();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Updates collection policies
        /// </summary>
        /// <param name="policies">Collection policies to update</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void SetCollectionPolicies(StackHashCollectionPolicyCollection policies)
        {
            WaitForWorker();
            if (policies == null) { throw new ArgumentNullException("policies"); }
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_SetCollectionPolicies;

                _worker.RunWorkerAsync(new WorkerArgSetCollectionPolicies(_contextId, policies));
            }
        }

        /// <summary>
        /// Updates properties for a product
        /// </summary>
        /// <param name="policies">Collection policies to update</param>
        /// <param name="policiesToRemove">Collection policies to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void UpdateProductProperties(StackHashCollectionPolicyCollection policies, StackHashCollectionPolicyCollection policiesToRemove)
        {
            WaitForWorker();
            // policies and/or policiesToRemove may be null
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_UpdateProductProperties;

                _worker.RunWorkerAsync(new WorkerArgUpdateProductProperties(_contextId, policies, policiesToRemove));
            }
        }

        /// <summary>
        /// Updated properties for an event package
        /// </summary>
        /// <param name="policies">Collection policies to update</param>
        /// <param name="policiesToRemove">Collection policies to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void UpdateEventPackageProperties(StackHashCollectionPolicyCollection policies, StackHashCollectionPolicyCollection policiesToRemove)
        {
            WaitForWorker();
            // policies and/or policiesToRemove may be null
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_UpdateEventPackageProperties;

                _worker.RunWorkerAsync(new WorkerArgUpdateEventPackageProperties(_contextId, policies, policiesToRemove));
            }
        }

        /// <summary>
        /// Populates the product list
        /// </summary>
        public void PopulateProducts()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_LoadProducts;

                _worker.RunWorkerAsync(new WorkerArgGetProductList(_contextId));
            }
        }

        /// <summary>
        /// Populates summary data for a product
        /// </summary>
        /// <param name="productId">Product Id</param>
        public void PopulateProductSummaries(int productId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(_productIdToProductsIndex != null);
            Debug.Assert(_productIdToProductsIndex.ContainsKey(productId));

            if ((!_worker.IsBusy) && (_productIdToProductsIndex != null) && (_productIdToProductsIndex.ContainsKey(productId)))
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_PopulateProuductSummaries,
                    DisplayNameForProductId(productId));

                _worker.RunWorkerAsync(new WorkerArgUpdateProductSummary(_contextId, productId));
            }
        }

        /// <summary>
        /// Populates event packages for a product or search
        /// </summary>
        /// <param name="product">Product to populate - may be null if searching across products</param>
        /// <param name="search">Search - may be null if populating a single product</param>
        /// <param name="sort">Sort order collection - default applied if null</param>
        /// <param name="page">Page of event packages to return (starts at 1)</param>
        /// <param name="eventsPerPage">Number of event packages per page</param>
        /// <param name="eventDisplayHitThreshold">Event display hit threshold</param>
        /// <param name="loadView">View to load once event packages are populated</param>
        /// <param name="pageIntention">Pagination intention</param>
        /// <param name="showEventsWithoutCabs">Show events without cabs</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void PopulateEventPackages(DisplayProduct product,
            StackHashSearchCriteriaCollection search,
            StackHashSortOrderCollection sort,
            int page,
            int eventsPerPage,
            int eventDisplayHitThreshold,
            ClientLogicView loadView,
            PageIntention pageIntention,
            bool showEventsWithoutCabs)
        {
            if ((search == null) && (product == null)) { throw new InvalidOperationException("Either search or product must be provided"); }

            // store the user requested search / sort - this will probably be different from the
            // actual search/sort as updated below
            this.LastEventsSearch = search;
            this.LastEventsSort = sort;

            // set the event list title
            if (search == null)
            {
                this.CurrentProduct = product;

                if (eventDisplayHitThreshold > 0)
                {
                    // Filtered view of a product
                    this.EventListTitle = string.Format(CultureInfo.InvariantCulture,
                        Properties.Resources.ClientLogic_EventListTitleProductFiltered,
                        product.Name,
                        product.Version);
                }
                else
                {
                    // Unfiltered view of a product
                    this.EventListTitle = string.Format(CultureInfo.InvariantCulture,
                        Properties.Resources.ClientLogic_EventListTitleProduct,
                        product.Name,
                        product.Version);
                }
            }
            else
            {
                // Search result
                this.EventListTitle = Properties.Resources.ClientLogic_EventListTitleSearch;
            }

            if (search == null)
            {
                // add product to search
                IntSearchOption productSearch = new IntSearchOption();
                productSearch.FieldName = "Id";
                productSearch.ObjectType = StackHashObjectType.Product;
                productSearch.SearchOptionType = StackHashSearchOptionType.Equal;
                productSearch.SearchType = StackHashSearchFieldType.Integer;
                productSearch.Start = product.Id;

                StackHashSearchOptionCollection searchOptionCollection = new StackHashSearchOptionCollection();
                searchOptionCollection.Add(productSearch);

                // add event threshold to search if in force
                if (eventDisplayHitThreshold > 0)
                {
                    IntSearchOption eventHitSearch = new IntSearchOption();
                    eventHitSearch.FieldName = "TotalHits";
                    eventHitSearch.ObjectType = StackHashObjectType.Event;
                    eventHitSearch.SearchOptionType = StackHashSearchOptionType.GreaterThanOrEqual;
                    eventHitSearch.SearchType = StackHashSearchFieldType.Integer;
                    eventHitSearch.Start = eventDisplayHitThreshold;
                
                    searchOptionCollection.Add(eventHitSearch);
                }

                // supress events without cabs if necessary
                if (!showEventsWithoutCabs)
                {
                    IntSearchOption haveCabsSearch = new IntSearchOption();
                    haveCabsSearch.FieldName = "CabCount";
                    haveCabsSearch.ObjectType = StackHashObjectType.Event;
                    haveCabsSearch.SearchOptionType = StackHashSearchOptionType.GreaterThan;
                    haveCabsSearch.SearchType = StackHashSearchFieldType.Integer;
                    haveCabsSearch.Start = 0;

                    searchOptionCollection.Add(haveCabsSearch);
                }

                StackHashSearchCriteria searchCriteria = new StackHashSearchCriteria();
                searchCriteria.SearchFieldOptions = searchOptionCollection;

                search = new StackHashSearchCriteriaCollection();
                search.Add(searchCriteria);
            }
            else
            {
                // if there's a hit threshold add it to the search
                if (eventDisplayHitThreshold > 0)
                {
                    // hit threshold search option
                    IntSearchOption eventHitSearch = new IntSearchOption();
                    eventHitSearch.FieldName = "TotalHits";
                    eventHitSearch.ObjectType = StackHashObjectType.Event;
                    eventHitSearch.SearchOptionType = StackHashSearchOptionType.GreaterThanOrEqual;
                    eventHitSearch.SearchType = StackHashSearchFieldType.Integer;
                    eventHitSearch.Start = eventDisplayHitThreshold;

                    foreach (StackHashSearchCriteria criteria in search)
                    {
                        bool hasHitOption = false;

                        foreach(StackHashSearchOption option in criteria.SearchFieldOptions)
                        {
                            if ((option.FieldName == "TotalHits") &&
                                (option.ObjectType == StackHashObjectType.Event))
                            {
                                hasHitOption = true;
                                break;
                            }
                        }

                        // if the criteria doesn't already have a hit option add the display filter
                        if (!hasHitOption)
                        {
                            criteria.SearchFieldOptions.Add(eventHitSearch);
                        }
                    }
                }

                // supress events without cabs if necessary
                if (!showEventsWithoutCabs)
                {
                    // have cabs search option
                    IntSearchOption haveCabsSearch = new IntSearchOption();
                    haveCabsSearch.FieldName = "CabCount";
                    haveCabsSearch.ObjectType = StackHashObjectType.Event;
                    haveCabsSearch.SearchOptionType = StackHashSearchOptionType.GreaterThan;
                    haveCabsSearch.SearchType = StackHashSearchFieldType.Integer;
                    haveCabsSearch.Start = 0;

                    foreach (StackHashSearchCriteria criteria in search)
                    {
                        bool hasCabCountOption = false;

                        foreach (StackHashSearchOption option in criteria.SearchFieldOptions)
                        {
                            if ((option.FieldName == "CabCount") &&
                                (option.ObjectType == StackHashObjectType.Event))
                            {
                                hasCabCountOption = true;
                                break;
                            }
                        }

                        // if the criteria doesn't already have a hit option add the display filter
                        if (!hasCabCountOption)
                        {
                            criteria.SearchFieldOptions.Add(haveCabsSearch);
                        }
                    }
                }
            }

            if (sort == null)
            {
                // default sort when loading is total hits, descending
                StackHashSortOrder hits = new StackHashSortOrder();
                hits.Ascending = false;
                hits.FieldName = "TotalHits";
                hits.ObjectType = StackHashObjectType.Event;

                sort = new StackHashSortOrderCollection();
                sort.Add(hits);
            }

            PopulateEventPackagesNoAdjustThreshold(product, search, sort, page, eventsPerPage, eventDisplayHitThreshold, loadView, pageIntention);
        }

        private void PopulateEventPackagesNoAdjustThreshold(DisplayProduct product,
            StackHashSearchCriteriaCollection search,
            StackHashSortOrderCollection sort,
            int page,
            int eventsPerPage,
            int eventDisplayHitThreshold,
            ClientLogicView loadView,
            PageIntention pageIntention)
        {
            WaitForWorker();

            // update the file list if the product changes (or we don't have a file list)
            bool refreshFiles = ((product != _lastPopulateEventsProduct) ||
                (_fileIdToFile == null) ||
                (_fileIdToFile.Count == 0));

            // if the page, search or sort changes flush the event id to event index - this will
            // cause a full rebuild of the event list
            if ((page != _lastPopulateEventsPage) ||
                (search != _lastPopulateEventsSearch) ||
                (sort != _lastPopulateEventsSort))
            {
                _eventIdToEventsIndex = null;
            }

            bool isScriptSearch = ScriptManager.SearchContainsScriptSearch(search);   

            // store the last call in case we need to repeat it (i.e. refreshing the current event list)
            _lastPopulateEventsProduct = product;
            _lastPopulateEventsSearch = search;
            _lastPopulateEventsSort = sort;
            _lastPopulateEventsPage = page;
            _lastPopulateEventsEventsPerPage = eventsPerPage;
            _lastPopulateEventsDisplayThreshold = eventDisplayHitThreshold;
            _lastPopulateEventsLoadView = loadView;

            StackHashProduct stackHashProduct = null;
            if (product != null)
            {
                stackHashProduct = product.StackHashProductInfo.Product;
            }

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;

                if (product == null)
                {
                    this.StatusText = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.Status_LoadEventsNoProduct,
                        page);
                }
                else
                {
                    this.StatusText = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.Status_LoadEventsWithProduct,
                        page,
                        product.NameAndVersion);
                }

                _worker.RunWorkerAsync(new WorkerArgGetWindowedEventPackages(_contextId,
                    page,
                    eventsPerPage,
                    stackHashProduct,
                    search,
                    sort,
                    refreshFiles,
                    loadView,
                    isScriptSearch,
                    pageIntention,
                    _lastPopulateEventsMinRow,
                    _lastPopulateEventsMaxRow,
                    _lastPopulateEventsTotalRows));
            }
        }

        /// <summary>
        /// Refreshes the currently selected event package
        /// </summary>
        public void RefreshEventPackage()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_RefreshEventPackage,
                    this.CurrentEventPackage.Id);

                _worker.RunWorkerAsync(new WorkerArgRefreshEventPackage(_contextId, 
                    this.CurrentProduct.StackHashProductInfo.Product, 
                    this.CurrentEventPackage.StackHashEventPackage.EventData));
            }
        }

        /// <summary>
        /// Sets the bug id field for an event
        /// </summary>
        /// <param name="eventPackage">EventPackage associated with the event</param>
        /// <param name="bugId">Bug Id to set</param>
        public void SetEventBugId(DisplayEventPackage eventPackage, string bugId)
        {
            WaitForWorker();
            if (eventPackage == null) { throw new ArgumentNullException("eventPackage"); }

            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(bugId != null);

            DisplayProduct product = ProductForId(eventPackage.ProductId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SetEventBugId,
                    eventPackage.Id);

                _worker.RunWorkerAsync(new WorkerArgSetEventBugId(_contextId, 
                    product.StackHashProductInfo.Product, 
                    eventPackage.StackHashEventPackage.EventData,
                    bugId));
            }
        }

        /// <summary>
        /// Sets the WorkFlow status field for an event
        /// </summary>
        /// <param name="eventPackage">EventPackage associated with the event</param>
        /// <param name="workFlowId">WorkFlow Id to set</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "workFlow"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorkFlow")]
        public void SetEventWorkFlow(DisplayEventPackage eventPackage, int workFlowId)
        {
            WaitForWorker();
            if (eventPackage == null) { throw new ArgumentNullException("eventPackage"); }

            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            DisplayProduct product = ProductForId(eventPackage.ProductId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SetEventWorkFlow,
                    eventPackage.Id);

                _worker.RunWorkerAsync(new WorkerArgSetEventWorkFlow(_contextId,
                    product.StackHashProductInfo.Product,
                    eventPackage.StackHashEventPackage.EventData,
                    workFlowId));
            }
        }

        /// <summary>
        /// Sends a product to a plugin
        /// </summary>
        /// <param name="product">DisplayProduct to send</param>
        /// <param name="pluginName">Plugin name</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugin")]
        public void SendProductToPlugin(DisplayProduct product, string pluginName)
        {
            WaitForWorker();
            if (product == null) { throw new ArgumentNullException("product"); }
            if (pluginName == null) { throw new ArgumentNullException("pluginName"); }

            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SendProductToPlugin,
                    product.NameAndVersion,
                    pluginName);

                _worker.RunWorkerAsync(new WorkerArgSendProductToPlugin(_contextId,
                    product.StackHashProductInfo.Product,
                    pluginName));
            }
        }

        /// <summary>
        /// Sends an event to a plugin
        /// </summary>
        /// <param name="eventPackage">EventPackage for the event to send</param>
        /// <param name="pluginName">Plugin name</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin")]
        public void SendEventToPlugin(DisplayEventPackage eventPackage, string pluginName)
        {
            WaitForWorker();
            if (eventPackage == null) { throw new ArgumentNullException("eventPackage"); }
            if (pluginName == null) { throw new ArgumentNullException("pluginName"); }

            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            DisplayProduct product = ProductForId(eventPackage.ProductId);

            if (!_worker.IsBusy)
            {
                _refreshEventOnPluginReportComplete = true;
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SendEventToPlugin,
                    eventPackage.Id,
                    pluginName);

                _worker.RunWorkerAsync(new WorkerArgSendEventToPlugin(_contextId, 
                    eventPackage.StackHashEventPackage.EventData, 
                    product.StackHashProductInfo.Product,
                    pluginName));
            }
        }

        /// <summary>
        /// Sends a cab to a plugin
        /// </summary>
        /// <param name="cab">Cab to send</param>
        /// <param name="eventPackage">EventPackage for the cab to send</param>
        /// <param name="pluginName">Plugin name</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugin")]
        public void SendCabToPlugin(DisplayCab cab, DisplayEventPackage eventPackage, string pluginName)
        {
            WaitForWorker();
            if (cab == null) { throw new ArgumentNullException("cab"); }
            if (eventPackage == null) { throw new ArgumentNullException("eventPackage"); }
            if (pluginName == null) { throw new ArgumentNullException("pluginName"); }

            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            DisplayProduct product = ProductForId(eventPackage.ProductId);

            if (!_worker.IsBusy)
            {
                _refreshEventOnPluginReportComplete = true;
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SendCabToPlugin,
                    cab.Id,
                    pluginName);

                _worker.RunWorkerAsync(new WorkerArgSendCabToPlugin(_contextId,
                    eventPackage.StackHashEventPackage.EventData,
                    product.StackHashProductInfo.Product,
                    cab.StackHashCabPackage.Cab,
                    pluginName));
            }
        }

        /// <summary>
        /// Sends all data for the current context to one or more plugins
        /// </summary>
        /// <param name="plugins">List of plugin names to send account data to</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugins"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugins")]
        public void SendAllToPlugins(string[] plugins)
        {
            WaitForWorker();
            if (plugins == null) { throw new ArgumentNullException("plugins"); }

            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);

            if (!_worker.IsBusy)
            {
                _refreshEventOnPluginReportComplete = false;
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_SendAllToPlugins;

                _worker.RunWorkerAsync(new WorkerArgSendAllToPlugins(_contextId, plugins));
            }
        }

        /// <summary>
        /// Gets event notes for the currently selected event
        /// </summary>
        public void GetEventNotes()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_LoadEventNotes,
                    this.CurrentEventPackage.Id);

                this.CurrentEventNotes = null;

                _worker.RunWorkerAsync(new WorkerArgAddGetEventNotes(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    null,
                    null,
                    0));
            }
        }

        /// <summary>
        /// Adds a note to the currently selected event
        /// </summary>
        /// <param name="note">The note to add</param>
        /// <param name="noteId">NoteId to edit (0 to add)</param>
        public void AddEventNote(string note, int noteId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(!string.IsNullOrEmpty(note));
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SaveEventNote,
                    this.CurrentEventPackage.Id);

                this.CurrentEventNotes = null;

                _worker.RunWorkerAsync(new WorkerArgAddGetEventNotes(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    note,
                    UserSettings.Settings.UserLogName,
                    noteId));
            }
        }

        /// <summary>
        /// Gets cab notes for the current selected cab
        /// </summary>
        public void GetCabNotes()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_LoadCabNotes,
                    this.CurrentCab.Id);

                this.CurrentCabNotes = null;

                _worker.RunWorkerAsync(new WorkerArgAddGetCabNotes(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab,
                    null,
                    null,
                    0));
            }
        }

        /// <summary>
        /// Adds a note to the currently selected cab
        /// </summary>
        /// <param name="note">The note to add</param>
        /// <param name="noteId">Note Id to edit (0 to add)</param>
        public void AddCabNote(string note, int noteId)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);
            Debug.Assert(!string.IsNullOrEmpty(note));
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_SaveCabNote,
                    this.CurrentCab.Id);

                this.CurrentCabNotes = null;

                _worker.RunWorkerAsync(new WorkerArgAddGetCabNotes(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab,
                    note,
                    UserSettings.Settings.UserLogName,
                    noteId));
            }
        }

        /// <summary>
        /// Extracts the currently selected cab file
        /// </summary>
        public void ExtractCurrentCab()
        {
            ExtractCurrentCab(null);
        }

        /// <summary>
        /// Gets the contents of a file from the currently selected cab
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileLength">File length</param>
        public void GetCabFile(string fileName, long fileLength)
        {
            if (fileName == null) { throw new ArgumentNullException("fileName"); }

            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_ExtractCabFile,
                    fileName,
                    this.CurrentCab.Id);

                _worker.RunWorkerAsync(new WorkerArgGetCabFile(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab,
                    fileName,
                    fileLength));
            }
        }

        /// <summary>
        /// Extracts the currently selected cab file
        /// </summary>
        /// <param name="extractFolder">Folder to extract CAB to (null to use temp)</param>
        public void ExtractCurrentCab(string extractFolder)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);
            Debug.Assert(this.CurrentProduct != null);
            
            // it's ok if extractFolder is null

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_ExtractCab,
                    this.CurrentCab.Id,
                    extractFolder == null ? Properties.Resources.Temp : extractFolder);

                _worker.RunWorkerAsync(new WorkerArgGetCabContents(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab,
                    extractFolder));
            }
        }

        /// <summary>
        /// Extracts the specified cab for the currently selected product and event package
        /// </summary>
        /// <param name="extractFolder">Folder to extract the cab to</param>
        /// <param name="cab">Cab to extract</param>
        public void ExtractCab(string extractFolder, StackHashCab cab)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentProduct != null);

            if (extractFolder == null) { throw new ArgumentNullException("extractFolder"); }
            if (cab == null) { throw new ArgumentNullException("cab"); }

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_ExtractCab,
                    cab.Id,
                    extractFolder);

                _worker.RunWorkerAsync(new WorkerArgGetCabContents(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    cab,
                    extractFolder));
            }
        }

        /// <summary>
        /// Downloads the currently selected cab file
        /// </summary>
        public void DownloadCab()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(_contextId != UserSettings.InvalidContextId);
            Debug.Assert(this.CurrentEventPackage != null);
            Debug.Assert(this.CurrentCab != null);
            Debug.Assert(this.CurrentProduct != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.Status_DownloadCabRequest,
                    this.CurrentCab.Id);

                _worker.RunWorkerAsync(new WorkerArgDownloadCab(_contextId,
                    this.CurrentProduct.StackHashProductInfo.Product,
                    this.CurrentEventPackage.StackHashEventPackage.EventData,
                    this.CurrentCab.StackHashCabPackage.Cab));
            }
        }

        /// <summary>
        /// Exports the product list
        /// </summary>
        /// <param name="exportPath">Export output path</param>
        public void ExportProductList(string exportPath)
        {
            WaitForWorker();
            if (exportPath == null) { throw new ArgumentNullException("exportPath"); }
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(this.Products != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_ExportProductList;

                _worker.RunWorkerAsync(new WorkerArgExportProductList(this.Products, exportPath));
            }
        }

        /// <summary>
        /// Export the current event list
        /// </summary>
        /// <param name="exportPath">Export output path</param>
        /// <param name="exportCabsAndEventInfos">Also export cabs and eventinfos</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Infos")]
        public void ExportEventList(string exportPath, bool exportCabsAndEventInfos)
        {
            WaitForWorker();
            if (exportPath == null) { throw new ArgumentNullException("exportPath"); }
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(this.EventPackages != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_ExportEvents;

                _worker.RunWorkerAsync(new WorkerArgExportEventList(this.EventPackages, exportPath, exportCabsAndEventInfos));
            }
        }

        /// <summary>
        /// Creates test index data
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="testIndexData">StackHashTestIndexData defining the test index</param>
        public void TestCreateTestIndex(int contextId, StackHashTestIndexData testIndexData)
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);
            Debug.Assert(contextId != UserSettings.InvalidContextId);
            Debug.Assert(testIndexData != null);

            if (!_worker.IsBusy)
            {
                this.NotBusy = false;
                this.StatusText = Properties.Resources.Status_CreateTestIndex;

                _worker.RunWorkerAsync(new WorkerArgCreateTestIndex(contextId, testIndexData));
            }
        }

        /// <summary>
        /// Get the last reported service error for a context in the service status
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <returns>Service error code (may be NoError)</returns>
        public StackHashServiceErrorCode ServiceStatusErrorForContext(int contextId)
        {
            StackHashServiceErrorCode error = StackHashServiceErrorCode.NoError;

            if ((this.ServiceStatus != null) && (this.ServiceStatus.ContextStatusCollection != null))
            {
                foreach (StackHashContextStatus contextStatus in this.ServiceStatus.ContextStatusCollection)
                {
                    if (contextStatus.ContextId == contextId)
                    {
                        error = contextStatus.CurrentError;
                        break;
                    }
                }
            }

            return error;
        }

        /// <summary>
        /// Get the last reported exception text for a context in the service status
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <returns>Exception text or an empty string if none found</returns>
        public string ServiceStatusExceptionTextForContext(int contextId)
        {
            string exceptionText = string.Empty;

            if ((this.ServiceStatus != null) && (this.ServiceStatus.ContextStatusCollection != null))
            {
                foreach (StackHashContextStatus contextStatus in this.ServiceStatus.ContextStatusCollection)
                {
                    if (contextStatus.ContextId == contextId)
                    {
                        exceptionText = contextStatus.LastContextException;
                        break;
                    }
                }
            }

            return exceptionText;
        }

        /// <summary>
        /// Gets the DateTime that the WinQual site was last updated
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Qual")]
        public DateTime LastWinQualSiteUpdate
        {
            get { return _lastWinQualSiteUpdate; }
            private set 
            {
                if (_lastWinQualSiteUpdate != value)
                {
                    _lastWinQualSiteUpdate = value;
                    
                    UpdateMainWindowTitle();
                    NotifyPropertyChanged("LastWinQualSiteUpdate");
                }
            }
        }

        /// <summary>
        /// Gets the status of the last database test
        /// </summary>
        public StackHashErrorIndexDatabaseStatus LastDatabaseTestStatus
        {
            get { return _lastDatabaseTestStatus; }
            private set 
            {
                if (_lastDatabaseTestStatus != value)
                {
                    _lastDatabaseTestStatus = value;
                    NotifyPropertyChanged("LastDatabaseTestStatus");
                }
            }
        }

        /// <summary>
        /// Gets the exception text for the last database test
        /// </summary>
        public string LastDatabaseTestExceptionText
        {
            get { return _lastDatabaseTestExceptionText; }
            private set 
            {
                if (_lastDatabaseTestExceptionText != value)
                {
                    _lastDatabaseTestExceptionText = value;
                    NotifyPropertyChanged("LastDatabaseTestExceptionText");
                }
            }
        }

        /// <summary>
        /// Gets the hostname of the computer running the service
        /// </summary>
        public string ServiceHost
        {
            get { return _serviceHost; }
            private set 
            {
                if (_serviceHost != value)
                {
                    _serviceHost = value;
                    NotifyPropertyChanged("ServiceHost");
                }
            }
        }

        /// <summary>
        /// True if the service is running on the same computer as this client.
        /// </summary>
        public bool ServiceIsLocal
        {
            get { return _serviceIsLocal; }
            private set 
            {
                if (_serviceIsLocal != value)
                {
                    _serviceIsLocal = value;
                    // update user settings so the correct value is set in ClientData
                    UserSettings.Settings.ClientIsLocal = _serviceIsLocal;
                    NotifyPropertyChanged("ServiceIsLocal");
                }
            }
        }

        /// <summary>
        /// Gets the current status text
        /// </summary>
        public string StatusText
        {
            get { return _statusText; }
            private set 
            {
                if (_statusText != value)
                {
                    _statusText = value;

                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format(CultureInfo.InvariantCulture,
                        "Client Status: {0}",
                        _statusText));

                    NotifyPropertyChanged("StatusText");
                }
            }
        }

        /// <summary>
        /// True if another client has disabled the current context
        /// </summary>
        public bool OtherClientHasDisabledContext
        {
            get 
            {
                bool ret;

                lock (_otherClientContextStateLock)
                {
                    ret = _otherClientHasDisabledContext;
                }

                return ret; 
            }
            private set 
            {
                lock (_otherClientContextStateLock)
                {
                    _otherClientHasDisabledContext = value;
                }
            }
        }

        /// <summary>
        /// True if at least one plugin has an error that the user needs to fix
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin")]
        public bool PluginHasError
        {
            get { return _pluginHasError; }
            set 
            {
                if (_pluginHasError != value)
                {
                    _pluginHasError = value;
                    NotifyPropertyChanged("PluginHasError");
                }
            }
        }

        /// <summary>
        /// True if ClientLogic is currently not busy
        /// </summary>
        public bool NotBusy
        {
            get { return _notBusy; }
            set 
            {
                if (_notBusy != value)
                {
                    _notBusy = value;
                    NotifyPropertyChanged("NotBusy");
                }
            }
        }

        /// <summary>
        /// Gets the title to display for the event list page
        /// </summary>
        public string EventListTitle
        {
            get { return _eventListTitle; }
            private set 
            {
                if (_eventListTitle != value)
                {
                    _eventListTitle = value;
                    NotifyPropertyChanged("EventListTitle");
                }
            }
        }

        /// <summary>
        /// Gets the current list of context (profile) settings. May be null.
        /// Call RefreshContextSettings to refresh.
        /// </summary>
        public ObservableCollection<DisplayContext> ContextCollection
        {
            get { return _contextCollection; }
            private set 
            {
                if (_contextCollection != value)
                {
                    _contextCollection = value;
                    NotifyPropertyChanged("ContextCollection");
                }
            }
        }

        /// <summary>
        /// Gets the current list of active context (profile) settings. May be null.
        /// Call RefreshContextSettings to refresh.
        /// </summary>
        public ObservableCollection<DisplayContext> ActiveContextCollection
        {
            get { return _activeContextCollection; }
            private set 
            {
                if (_activeContextCollection != value)
                {
                    _activeContextCollection = value;
                    NotifyPropertyChanged("ActiveContextCollection");
                }
            }
        }

        /// <summary>
        /// Gets or sets the active context - update UserSettings and call CheckForContextIdChange
        /// when setting this property
        /// </summary>
        public DisplayContext CurrentContext
        {
            get { return _currentContext; }
            set 
            {
                if (_currentContext != value)
                {
                    _currentContext = value;
                    NotifyPropertyChanged("CurrentContext");
                }
            }
        }

        private void UpdateCurrentContext()
        {
            DisplayContext currentContext = null;

            if (this.ActiveContextCollection != null)
            {
                foreach (DisplayContext context in this.ActiveContextCollection)
                {
                    if (context.Id == _contextId)
                    {
                        currentContext = context;
                        break;
                    }
                }
            }

            this.CurrentContext = currentContext;
        }

        /// <summary>
        /// Gets the current list of products
        /// </summary>
        public ObservableCollection<DisplayProduct> Products
        {
            get { return _products; }
            private set 
            {
                if (_products != value)
                {
                    _products = value;
                    NotifyPropertyChanged("Products");

                    // if products is null also clear the product index
                    if (_products == null)
                    {
                        _productIdToProductsIndex = null;
                        RequestUi(ClientLogicUIRequest.ProductListCleared);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of EventPackages for the current product
        /// </summary>
        public ObservableCollection<DisplayEventPackage> EventPackages
        {
            get { return _eventPackages; }
            private set 
            {
                if (_eventPackages != value)
                {
                    _eventPackages = value;
                    NotifyPropertyChanged("EventPackages");
                }
            }
        }

        /// <summary>
        /// Gets the list of debugger script data
        /// </summary>
        public ObservableCollection<StackHashScriptFileData> ScriptData
        {
            get { return _scriptData; }
            private set 
            {
                if (_scriptData != value)
                {
                    _scriptData = value;
                    NotifyPropertyChanged("ScriptData");
                }
            }
        }

        /// <summary>
        /// Gets the list of debugger script result files
        /// </summary>
        public ObservableCollection<DisplayScriptResult> ScriptResultFiles
        {
            get { return _scriptResultFiles; }
            private set 
            {
                if (_scriptResultFiles != value)
                {
                    _scriptResultFiles = value;
                    NotifyPropertyChanged("ScriptResultFiles");
                }
            }
        }

        /// <summary>
        /// Gets the collection of notes for the current event
        /// </summary>
        public ObservableCollection<StackHashNoteEntry> CurrentEventNotes
        {
            get { return _currentEventNotes; }
            private set 
            {
                if (_currentEventNotes != value)
                {
                    _currentEventNotes = value;
                    NotifyPropertyChanged("CurrentEventNotes");
                }
            }
        }

        /// <summary>
        /// Gets the collection of notes for the current cab
        /// </summary>
        public ObservableCollection<StackHashNoteEntry> CurrentCabNotes
        {
            get { return _currentCabNotes; }
            private set 
            {
                if (_currentCabNotes != value)
                {
                    _currentCabNotes = value;
                    NotifyPropertyChanged("CurrentCabNotes");
                }
            }
        }
        
        /// <summary>
        /// Gets the list of available plugins (note, includes plug-ins that may have failed to load) 
        /// </summary>
        public ObservableCollection<StackHashBugTrackerPlugInDiagnostics> AvailablePlugIns
        {
            get { return _availablePlugIns; }
            private set 
            {
                if (_availablePlugIns != value)
                {
                    _availablePlugIns = value;
                    NotifyPropertyChanged("AvailablePlugIns");
                }
            }
        }

        /// <summary>
        /// Gets the list of active plugins
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugins")]
        public ObservableCollection<StackHashBugTrackerPlugIn> ActivePlugins
        {
            get { return _activePlugins; }
            private set 
            {
                if (_activePlugins != value)
                {
                    _activePlugins = value;
                    NotifyPropertyChanged("ActivePlugins");
                }
            }
        }

        /// <summary>
        /// Gets the list of workflow mappings
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorkFlow")]
        public ObservableCollection<DisplayMapping> WorkFlowMappings
        {
            get { return _workFlowMappings; }
            private set 
            {
                if (_workFlowMappings != value)
                {
                    _workFlowMappings = value;
                    NotifyPropertyChanged("WorkFlowMappings");
                }
            }
        }

        /// <summary>
        /// Gets or sets the current proxy settings to use for the service - note, 
        /// setting this property is not sufficient to update the service
        /// </summary>
        public StackHashProxySettings ServiceProxySettings
        {
            get { return _serviceProxySettings; }
            set 
            {
                if (_serviceProxySettings != value)
                {
                    _serviceProxySettings = value;
                    NotifyPropertyChanged("ServiceProxySettings");
                }
            }
        }

        /// <summary>
        /// Gets or sets the client timeout in seconds - note, setting
        /// this property is not sufficient to update the service setting
        /// </summary>
        public int ClientTimeoutInSeconds
        {
            get { return _clientTimeoutInSeconds; }
            set 
            {
                if (_clientTimeoutInSeconds != value)
                {
                    _clientTimeoutInSeconds = value;
                    NotifyPropertyChanged("ClientTimeoutInSeconds");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UpdateMainWindowTitle()
        {
            if (_guiDispatcher.CheckAccess())
            {
                DateTime localWinQualUpdate = DateTime.MinValue;
                if (this.LastWinQualSiteUpdate > DateTime.MinValue)
                {
                    try
                    {
                        localWinQualUpdate = this.LastWinQualSiteUpdate.ToLocalTime();
                    }
                    catch
                    {
                        localWinQualUpdate = DateTime.MinValue;
                    }
                }

                string productString = string.Empty;
                if (this.CurrentProduct != null)
                {
                    productString = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.MainWindowTitlePrefix,
                        this.CurrentProduct.NameAndVersion);
                }

                string updateString = string.Empty;
                if (localWinQualUpdate > DateTime.MinValue)
                {
                    updateString = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.MainWindowTitlePostfix,
                        localWinQualUpdate.ToLongDateString());
                }

                this.MainWindowTitle = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.MainWindowTitleTemplate,
                    productString,
                    updateString);
            }
            else
            {
                _guiDispatcher.BeginInvoke(new Action(UpdateMainWindowTitle));
            }
        }

        /// <summary>
        /// Gets or sets the current product (null if no selection or more than one product)
        /// </summary>
        public DisplayProduct CurrentProduct
        {
            get { return _currentProduct; }
            set 
            {
                if (_currentProduct != value)
                {
                    _currentProduct = value;

                    if (_currentProduct == null)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Current Product cleared");
                    }
                    else
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "Current Product is {0} {1} ({2})",
                            _currentProduct.Name,
                            _currentProduct.Version,
                            _currentProduct.Id));
                    }

                    UpdateMainWindowTitle();
                    NotifyPropertyChanged("CurrentProduct");
                }
            }
        }

        /// <summary>
        /// Gets the title for the main application window
        /// </summary>
        public string MainWindowTitle
        {
            get { return _mainWindowTitle; }
            private set 
            {
                if (_mainWindowTitle != value)
                {
                    _mainWindowTitle = value;
                    NotifyPropertyChanged("MainWindowTitle");
                }
            }
        }

        /// <summary>
        /// Gets or sets the current event package. EventInfoList and Cabs
        /// are sorted when this property is set.
        /// </summary>
        public DisplayEventPackage CurrentEventPackage
        {
            get { return _currentEventPackage; }
            set 
            {
                if (_currentEventPackage != value)
                {
                    _currentEventPackage = value;

                    // update the current product - this is necessary in case the event list
                    // contains search results from multiple products
                    if (_currentEventPackage == null)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Current Event Package cleared");
                    }
                    else
                    {
                        this.CurrentProduct = ProductForId(_currentEventPackage.ProductId);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "Current Event Package is {0}",
                            _currentEventPackage.Id));
                    }

                    NotifyPropertyChanged("CurrentEventPackage");

                    // clear the current cab when the event package changes
                    this.CurrentCab = null;
                    this.CurrentCabPackage = null;
                    this.ScriptResultFiles = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current cab
        /// </summary>
        public DisplayCab CurrentCab
        {
            get { return _currentCab; }
            set 
            {
                if (_currentCab != value)
                {
                    _currentCab = value;

                    if (_currentCab == null)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Current Cab cleared");
                    }
                    else
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "Current Cab is {0}",
                            _currentCab.Id));
                    }

                    // clear extract folder and script result when the cab changes
                    this.CurrentCabFolder = null;
                    this.CurrentScriptResult = null;

                    NotifyPropertyChanged("CurrentCab");
                }
            }
        }

        /// <summary>
        /// Gets the current cab package (currently used only on the service machine to 
        /// open the local folder containing the cab)
        /// </summary>
        public StackHashCabPackage CurrentCabPackage
        {
            get { return _currentCabPackage; }
            private set 
            {
                if (_currentCabPackage != value)
                {
                    _currentCabPackage = value;
                    NotifyPropertyChanged("CurrentCabPackage");
                }
            }
        }

        /// <summary>
        /// Gets the current extracted cab folder
        /// </summary>
        public string CurrentCabFolder
        {
            get { return _currentCabFolder; }
            private set 
            {
                if (_currentCabFolder != value)
                {
                    _currentCabFolder = value;
                    NotifyPropertyChanged("CurrentCabFolder");
                }
            }
        }

        /// <summary>
        /// Gets or sets the current collection of plugin diagnostics
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public StackHashNameValueCollection CurrentPluginDiagnostics
        {
            get { return _currentPluginDiagnostics; }
            set 
            {
                if (_currentPluginDiagnostics != value)
                {
                    _currentPluginDiagnostics = value;
                    NotifyPropertyChanged("CurrentPluginDiagnostics");
                }
            }
        }

        /// <summary>
        /// Gets the current events page number
        /// </summary>
        public int CurrentEventsPage
        {
            get { return _currentEventsPage; }
            private set 
            {
                if (_currentEventsPage != value)
                {
                    _currentEventsPage = value; 
                    NotifyPropertyChanged("CurrentEventsPage");
                }
            }
        }

        /// <summary>
        /// Gets the contents of the last requested file from within a cab
        /// </summary>
        public string CurrentCabFileContents
        {
            get { return _currentCabFileContents; }
            private set 
            {
                if (_currentCabFileContents != value)
                {
                    _currentCabFileContents = value;
                    NotifyPropertyChanged("CurrentCabFileContents");
                }
            }
        }

        /// <summary>
        /// Gets the current events maximum page number
        /// </summary>
        public int CurrentEventsMaxPage
        {
            get { return _currentEventsMaxPage; }
            private set 
            {
                if (_currentEventsMaxPage != value)
                {
                    _currentEventsMaxPage = value; 
                    NotifyPropertyChanged("CurrentEventsMaxPage");
                }
            }
        }

        /// <summary>
        /// Gets the last search passed to PopulateEventPackages
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public StackHashSearchCriteriaCollection LastEventsSearch
        {
            get { return _lastEventsSearch; }
            private set 
            {
                if (_lastEventsSearch != value)
                {
                    _lastEventsSearch = value;
                    NotifyPropertyChanged("LastEventsSearch");
                }
            }
        }

        /// <summary>
        /// Gets the last sort passed to PopulateEventPackagages
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public StackHashSortOrderCollection LastEventsSort
        {
            get { return _lastEventsSort; }
            private set 
            {
                if (_lastEventsSort != value)
                {
                    _lastEventsSort = value;
                    NotifyPropertyChanged("LastEventsSort");
                }
            }
        }

        /// <summary>
        /// Gets the current script (StackHashScriptSettings)
        /// </summary>
        public StackHashScriptSettings CurrentScript
        {
            get { return _currentScript; }
            private set 
            {
                if (_currentScript != value)
                {
                    _currentScript = value;
                    NotifyPropertyChanged("CurrentScript");
                }
            }
        }

        /// <summary>
        /// Gets the current script result
        /// </summary>
        public StackHashScriptResult CurrentScriptResult
        {
            get { return _currentScriptResult; }
            private set 
            {
                if (_currentScriptResult != value)
                {
                    _currentScriptResult = value;
                    NotifyPropertyChanged("CurrentScriptResult");
                }
            }
        }

        /// <summary>
        /// Gets the most recently retreived context status
        /// </summary>
        public StackHashContextStatus ContextStatus
        {
            get { return _contextStatus; }
            private set 
            {
                if (_contextStatus != value)
                {
                    _contextStatus = value;
                    NotifyPropertyChanged("ContextStatus");
                }
            }
        }
        
        /// <summary>
        /// Gets the most recent service status
        /// </summary>
        public StackHashStatus ServiceStatus
        {
            get { return _serviceStatus; }
            set 
            {
                if (_serviceStatus != value)
                {
                    _serviceStatus = value;

                    NotifyPropertyChanged("ServiceStatus");
                }

                if (_serviceStatus != null)
                {
                    UpdateContextListStatusesCore(_serviceStatus);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current view
        /// </summary>
        public ClientLogicView CurrentView
        {
            get { return _currentView; }
            set 
            {
                if (_currentView != value)
                {
                    _currentView = value;

                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format(CultureInfo.CurrentCulture,
                        "Client view chaging to {0}",
                        _currentView));

                    // de-select cab at product or event-list level
                    // clear last events search when returning to the product list
                    switch (_currentView)
                    {
                        case ClientLogicView.ProductList:
                            this.CurrentCab = null;
                            this.CurrentCabPackage = null;
                            this.CurrentCabFileContents = null;
                            this.ScriptResultFiles = null;
                            this.LastEventsSearch = null;
                            break;

                        case ClientLogicView.EventList:
                            this.CurrentCab = null;
                            this.CurrentCabPackage = null;
                            this.CurrentCabFileContents = null;
                            this.ScriptResultFiles = null;
                            break;
                    }

                    NotifyPropertyChanged("CurrentView");
                }
            }
        }

        /// <summary>
        /// Gets the current state of the service log. This property is valid after a call
        /// to RefreshContextSettings() but is not updated when the log state is changed
        /// (i.e. call RefreshContextSettings() again to get the correct state)
        /// </summary>
        public bool ServiceLogEnabled
        {
            get { return _serviceLogEnabled; }
            private set 
            {
                if (_serviceLogEnabled != value)
                {
                    _serviceLogEnabled = value;
                    NotifyPropertyChanged("ServiceLogEnabled");
                }
            }
        }

        /// <summary>
        /// Gets the current state of the service reporting preference.
        /// </summary>
        public bool ServiceReportingEnabled
        {
            get { return _serviceReportingEnabled; }
            set 
            {
                if (_serviceReportingEnabled != value)
                {
                    _serviceReportingEnabled = value;
                    NotifyPropertyChanged("ServiceReportingEnabled");
                }
            }
        }

        /// <summary>
        /// True if any products in the current list are enabled
        /// </summary>
        public bool AnyEnabledProducts
        {
            get { return _anyEnabledProducts; }
            private set 
            {
                if (_anyEnabledProducts != value)
                {
                    _anyEnabledProducts = value;
                    NotifyPropertyChanged("AnyEnabledProducts");
                }
            }
        }

        /// <summary>
        /// Gets the current service status text
        /// </summary>
        public string ServiceStatusText
        {
            get { return _serviceStatusText; }
            private set 
            {
                if (_serviceStatusText != value)
                {
                    _serviceStatusText = value;
                    NotifyPropertyChanged("ServiceStatusText");
                }
            }
        }

        /// <summary>
        /// True if the synchronization task for the current context is running
        /// </summary>
        public bool SyncRunning
        {
            get { return _syncRunning; }
            private set 
            {
                if (_syncRunning != value)
                {
                    _syncRunning = value;
                    NotifyPropertyChanged("SyncRunning");
                    UpdateServiceStatusText();
                }
            }
        }

        /// <summary>
        /// True if the analyze task for the current context is running
        /// </summary>
        public bool AnalyzeRunning
        {
            get { return _analyzeRunning; }
            private set 
            {
                if (_analyzeRunning != value)
                {
                    _analyzeRunning = value;
                    NotifyPropertyChanged("AnalyzeRunning");
                    UpdateServiceStatusText();
                }
            }
        }

        /// <summary>
        /// True if the purge task for the current context is running
        /// </summary>
        public bool PurgeRunning
        {
            get { return _purgeRunning; }
            private set 
            {
                if (_purgeRunning != value)
                {
                    _purgeRunning = value;
                    NotifyPropertyChanged("PurgeRunning");
                    UpdateServiceStatusText();
                }
            }
        }

        /// <summary>
        /// True if events are being reported to plugin(s)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin")]
        public bool PluginReportRunning
        {
            get { return _pluginReportRunning; }
            private set 
            {
                if (_pluginReportRunning != value)
                {
                    _pluginReportRunning = value;
                    NotifyPropertyChanged("PluginReportRunning");
                    UpdateServiceStatusText();
                }
            }
        }

        /// <summary>
        /// True if an index is currently being moved
        /// </summary>
        public bool MoveIndexRunning
        {
            get { return _moveIndexRunning; }
            private set 
            {
                if (_moveIndexRunning != value)
                {
                    _moveIndexRunning = value;
                    NotifyPropertyChanged("MoveIndexRunning");
                    UpdateServiceStatusText();
                }
            }
        }

        /// <summary>
        /// Gets the current service progress
        /// </summary>
        public int ServiceProgress
        {
            get { return _serviceProgress; }
            private set 
            {
                if (_serviceProgress != value)
                {
                    _serviceProgress = value;
                    NotifyPropertyChanged("ServiceProgress");
                }
            }
        }

        /// <summary>
        /// Gets the visibility of the current service progress (true if the progress value is currently meaningful)
        /// </summary>
        public bool ServiceProgressVisible
        {
            get { return _serviceProgressVisible; }
            private set 
            {
                if (_serviceProgressVisible != value)
                {
                    _serviceProgressVisible = value;
                    NotifyPropertyChanged("ServiceProgressVisible");
                }
            }
        }

        private void UpdateServiceProgress(int progress, bool visible)
        {
            if (_guiDispatcher.CheckAccess())
            {
                this.ServiceProgress = progress;
                this.ServiceProgressVisible = visible;
            }
            else
            {
                _guiDispatcher.Invoke(new Action<int, bool>(UpdateServiceProgress), progress, visible);
            }
        }

        /// <summary>
        /// Checks to see if the Context Id had changed. If it has the product list
        /// is refreshed.
        /// </summary>
        public void CheckForContextIdChange()
        {
            WaitForWorker();
            Debug.Assert(!_worker.IsBusy);

            bool startedBusyAction = false;

            if (!_worker.IsBusy)
            {
                // if the service location has changed we need to reconnect
                if ((ServiceProxy.Services.ServiceHost != _initialServiceConnectHost) ||
                    (ServiceProxy.Services.ServicePort != _initialServiceConnectPort))
                {
                    // connect again
                    _initialServiceConnectComplete = false;

                    // clear all selections
                    ClearSelections();
                    this.Products = null;

                    // return to the product list view
                    this.CurrentView = ClientLogicView.ProductList;

                    // report that the context Id had changed as we're talking to a new server
                    ReportClientLogicContextIdChanged();
                }

                if (_initialServiceConnectComplete)
                {
                    int currentContextId = UserSettings.Settings.CurrentContextId;

                    if ((_contextId != currentContextId) || (this.Products == null))
                    {
                        _contextId = currentContextId;
                        UpdateCurrentContext();
                        ReportClientLogicContextIdChanged();

                        // clear any existing product list
                        this.Products = null;

                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "ClientLogic Context ID is {0}",
                            _contextId));

                        if (_contextId != UserSettings.InvalidContextId)
                        {
                            startedBusyAction = true;
                            if (HasContextEverSynced(_contextId))
                            {
                                // populate the product list
                                PopulateProducts();
                            }
                            else
                            {
                                // sync the product list only
                                AdminStartSync(false, true, true, null);
                            }
                        }

                        // return to the product list
                        this.CurrentView = ClientLogicView.ProductList;
                    }
                }
                else
                {
                    // connect to the service first
                    startedBusyAction = true;
                    AdminServiceConnect();
                }
            }

            if (!startedBusyAction)
            {
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
        }

        private void WaitForWorker()
        {
            if (_worker.IsBusy)
            {
                int timeoutGuard = 0;

                while (_worker.IsBusy)
                {
                    if (timeoutGuard > WaitForWorkerTimeoutMS)
                    {
                        Debug.Assert(false, "Failed to wait for worker");
                        break;
                    }

                    Thread.Sleep(WaitForWorkerSleepMS);
                    timeoutGuard += WaitForWorkerSleepMS;
                }
            }
        }

        private DisplayProduct ProductForId(int productId)
        {
            DisplayProduct productForId = null;

            if ((_productIdToProductsIndex != null) && (_productIdToProductsIndex.ContainsKey(productId)))
            {
                productForId = this.Products[_productIdToProductsIndex[productId]];
            }

            return productForId;
        }

        private void UpdateServiceStatusText()
        {
            List<string> tasks = new List<string>();

            if (this.SyncRunning)
            {
                string syncText;
                if (_syncIsResync)
                {
                    syncText = Properties.Resources.ServiceStatus_Resync;
                }
                else
                {
                    syncText = Properties.Resources.ServiceStatus_Sync;
                }

                if (!string.IsNullOrEmpty(_syncStageText))
                {
                    tasks.Add(string.Format(CultureInfo.CurrentCulture,
                        "{0} ({1})",
                        syncText,
                        _syncStageText));
                }
                else
                {
                    tasks.Add(syncText);
                }
            }

            if (this.MoveIndexRunning)
            {
                if (string.IsNullOrEmpty(_moveProgressText))
                {
                    tasks.Add(Properties.Resources.ServiceStatus_MoveIndex);
                }
                else
                {
                    tasks.Add(string.Format(CultureInfo.CurrentCulture,
                        "{0} ({1})",
                        Properties.Resources.ServiceStatus_MoveIndex,
                        _moveProgressText));
                }
            }

            if (this.AnalyzeRunning)
            {
                tasks.Add(Properties.Resources.ServiceStatus_Analyze);
            }

            if (this.PurgeRunning)
            {
                tasks.Add(Properties.Resources.ServiceStatus_Purge);
            }

            if (this.PluginReportRunning)
            {
                tasks.Add(Properties.Resources.ServiceStatus_PluginReportRunning);
            }

            if (tasks.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Properties.Resources.ServiceStatus_Prefix);
                sb.Append(" ");

                for (int i = 0; i < tasks.Count; i++)
                {
                    sb.Append(tasks[i]);

                    // separate each task status with a pipe
                    if (i < (tasks.Count -1))
                    {
                        sb.Append(" | ");
                    }
                }

                this.ServiceStatusText = sb.ToString();
            }
            else
            {
                this.ServiceStatusText = string.Empty;
            }
        }

        private void ReportSyncComplete(bool succeeded, StackHashServiceErrorCode serviceError)
        {
            if (SyncComplete != null)
            {
                SyncComplete(this, new SyncCompleteEventArgs(succeeded, serviceError));
            }
        }

        private void ReportClientLogicContextIdChanged()
        {
            if (ClientLogicContextIdChanged != null)
            {
                ClientLogicContextIdChanged(this, EventArgs.Empty);
            }
        }

        private static StackHashServiceErrorCode ServiceErrorCodeFromException(Exception ex)
        {
            StackHashServiceErrorCode ret = StackHashServiceErrorCode.NoError;

            if (ex != null)
            {
                FaultException<ReceiverFaultDetail> faultException = ex as FaultException<ReceiverFaultDetail>;
                if (faultException != null)
                {
                    ret = faultException.Detail.ServiceErrorCode;
                }
            }

            return ret;
        }

        private void ReportError(Exception exception, ClientLogicErrorCode error)
        {
            ReportError(exception, error, ServiceErrorCodeFromException(exception));
        }

        private void ReportError(Exception exception, ClientLogicErrorCode error, StackHashServiceErrorCode serviceError)
        {
            DiagnosticsHelper.LogException(DiagSeverity.Warning,
                string.Format(CultureInfo.InvariantCulture,
                "ClientLogic Error: {0}, Service Error Code: {1}",
                error,
                serviceError),
                exception);

            if (ClientLogicError != null)
            {
                ClientLogicError(this, new ClientLogicErrorEventArgs(exception, error, serviceError));
            }
        }

        private void RequestUi(ClientLogicUIRequest uiRequest)
        {
            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                string.Format(CultureInfo.InvariantCulture,
                "ClientLogic UI Request: {0}",
                uiRequest));

            if (ClientLogicUI != null)
            {
                ClientLogicUI(this, new ClientLogicUIEventArgs(uiRequest));
            }
        }

        private void NotifySetupWizard(ClientLogicSetupWizardPromptOperation prompt, bool succeeded, StackHashServiceErrorCode lastServiceError, Exception lastException)
        {
            if (ClientLogicSetupWizardPrompt != null)
            {
                // pull out the service error code if available
                if ((lastException != null) && (lastServiceError == StackHashServiceErrorCode.NoError))
                {
                    lastServiceError = ServiceErrorCodeFromException(lastException);
                }

                DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                    string.Format(CultureInfo.InvariantCulture,
                    "ClientLogic Setup Wizard Prompt: {0}, Succeeded = {1}, Last Service Error = {2}, Last Exception (message) = {3}",
                    prompt,
                    succeeded,
                    lastServiceError,
                    lastException == null ? string.Empty : lastException.Message));

                ClientLogicSetupWizardPrompt(this, new ClientLogicSetupWizardPromptEventArgs(prompt, succeeded, lastServiceError, lastException));
            }
        }

        private string DisplayNameForProductId(int productId)
        {
            if (_productIdToProductsIndex.ContainsKey(productId))
            {
                return this.Products[_productIdToProductsIndex[productId]].NameAndVersion;
            }
            else
            {
                return productId.ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the Guid of the service running on this computer (Guid.Empty if no service detected)
        /// </summary>
        /// <returns>Local service Guid</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static Guid GetLocalServiceGuid()
        {
            Guid localServiceGuid = Guid.Empty;

            try
            {
                string serviceInstanceDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), LocalServiceGuidPath);
                if (File.Exists(serviceInstanceDataPath))
                {
                    // Example...
                    // ServiceGuid=f1fd1803-0bb7-4171-89c3-bc9a3e5251c6
                    // ServiceMachineName=KATE-QUAD

                    string[] lines = File.ReadAllLines(serviceInstanceDataPath);
                    foreach (string line in lines)
                    {
                        string[] elements = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (elements.Length == 2)
                        {
                            if (elements[0] == "ServiceGuid")
                            {
                                if (Guid.TryParse(elements[1], out localServiceGuid))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                    "GetLocalServiceGuid Failed",
                    ex);
            }

            return localServiceGuid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void HandleAdminReportThisClientInitiated(StackHashAdminReport report, AdminReportException adminReportException)
        {
            switch (report.Operation)
            {
                case StackHashAdminOperation.BugReportStarted:
                    UpdateServiceProgress(0, true);
                    break;

                case StackHashAdminOperation.BugReportCompleted:
                    UpdateServiceProgress(0, false);
                    if (adminReportException == null)
                    {
                        if (_refreshEventOnPluginReportComplete)
                        {
                            // refresh the event package
                            WaitForWorker();
                            RefreshEventPackage();
                        }
                        else
                        {
                            // success without a refresh - just return to not busy
                            this.NotBusy = true;
                            this.StatusText = Properties.Resources.Status_Ready;
                        }
                    }
                    else
                    {
                        // report to plugin failed
                        this.NotBusy = true;
                        this.StatusText = Properties.Resources.Status_Ready;

                        this.PluginHasError = true;
                        ReportError(adminReportException, ClientLogicErrorCode.BugReportFailed, report.ServiceErrorCode);
                    }
                    break;

                case StackHashAdminOperation.AdminRegister:
                    // note the service host name
                    this.ServiceHost = report.ServiceHost;

                    // detect if we've been bumped
                    if (report.ServiceErrorCode == StackHashServiceErrorCode.ClientBumped)
                    {
                        // only notify the user once when bumped... flag is cleared after we register again
                        if (!_clientIsBumped)
                        {
                            _clientIsBumped = true;
                            RequestUi(ClientLogicUIRequest.ClientBumped);
                        }
                    }
                    break;

                case StackHashAdminOperation.ErrorIndexMoveCompleted:
                    if (adminReportException == null)
                    {
                        // wait for the current worker op to complete
                        WaitForWorker();

                        if (_activateContextAfterMove)
                        {
                            // activate the context Id again
                            AdminActivateContext(_activateContextAfterMoveContextId);

                            _activateContextAfterMove = false;
                            _activateContextAfterMoveContextId = -1;
                        }
                        else
                        {
                            // if we're not reactivating flag the move complete - no need to do
                            // this if we're reactivating as this will update contexts
                            RequestUi(ClientLogicUIRequest.MoveIndexComplete);
                        }
                    }
                    else
                    {
                        // move index failed
                        this.NotBusy = true;
                        this.StatusText = Properties.Resources.Status_Ready;

                        ReportError(adminReportException, ClientLogicErrorCode.MoveIndexFailed, report.ServiceErrorCode);
                    }
                    break;

                case StackHashAdminOperation.ErrorIndexCopyCompleted:
                    UpdateServiceProgress(0, false);
                    if (adminReportException == null)
                    {
                        // wait for the current worker op to complete
                        WaitForWorker();

                        if (_activateContextAfterCopy)
                        {
                            // activate the context Id again
                            AdminActivateContext(_activateContextAfterCopyContextId);

                            _activateContextAfterCopy = false;
                            _activateContextAfterCopyContextId = -1;
                        }
                        else
                        {
                            // if we're not reactivating flag the copy complete - no need to do
                            // this if we're reactivating as this will update contexts
                            RequestUi(ClientLogicUIRequest.CopyIndexComplete);
                        }
                    }
                    else
                    {
                        // copy index failed
                        this.NotBusy = true;
                        this.StatusText = Properties.Resources.Status_Ready;

                        ReportError(adminReportException, ClientLogicErrorCode.CopyIndexFailed, report.ServiceErrorCode);
                    }
                    break;

                case StackHashAdminOperation.DownloadCabStarted:
                    this.NotBusy = false;
                    this.StatusText = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.Status_DownloadCab,
                        this.CurrentCab.Id);
                    break;

                case StackHashAdminOperation.DownloadCabCompleted:
                    if (adminReportException == null)
                    {
                        // wait for the current worker op to complete
                        WaitForWorker();

                        // refresh the current view on success
                        RefreshCurrentView();
                    }
                    else
                    {
                        // cab download failed

                        this.NotBusy = true;
                        this.StatusText = Properties.Resources.Status_Ready;

                        ReportError(adminReportException, ClientLogicErrorCode.AdminReportError, report.ServiceErrorCode);
                    }
                    break;

                case StackHashAdminOperation.WinQualLogOnCompleted:
                    // report error on failure
                    if (adminReportException != null)
                    {
                        ReportError(adminReportException, ClientLogicErrorCode.WinQualLogOnFailed, report.ServiceErrorCode);
                    }
                    else
                    {
                        RequestUi(ClientLogicUIRequest.TestWinQualLogOnSuccess);
                    }

                    // notify wizard if listening
                    NotifySetupWizard(ClientLogicSetupWizardPromptOperation.LogOnTestComplete, adminReportException == null, report.ServiceErrorCode, adminReportException);

                    this.NotBusy = true;
                    this.StatusText = Properties.Resources.Status_Ready;
                    break;

                case StackHashAdminOperation.RunScriptStarted:
                    // flag that the script run is in progress
                    this.NotBusy = false;
                    this.StatusText = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.Status_RunScript,
                        _pendingScriptName);
                    break;

                case StackHashAdminOperation.RunScriptCompleted:
                    if (adminReportException == null)
                    {
                        // wait for the current worker op to complete
                        WaitForWorker();

                        Debug.Assert(!string.IsNullOrEmpty(_pendingScriptName));

                        // script run succeeded, get the results
                        AdminGetResult(_pendingScriptName);
                    }
                    else
                    {
                        // script run failed, report this to the user
                        this.NotBusy = true;
                        this.StatusText = Properties.Resources.Status_Ready;
                        ReportError(adminReportException, ClientLogicErrorCode.AdminReportError, report.ServiceErrorCode);
                    }
                    break;

                case StackHashAdminOperation.UploadFileStarted:
                    this.NotBusy = false;
                    this.StatusText = Properties.Resources.Status_UploadingMapping;
                    break;

                case StackHashAdminOperation.UploadFileCompleted:
                    this.NotBusy = true;
                    this.StatusText = Properties.Resources.Status_Ready;

                    if (adminReportException == null)
                    {
                        RequestUi(ClientLogicUIRequest.UploadMappingFileCompleted);
                    }
                    else
                    {
                        ReportError(adminReportException, ClientLogicErrorCode.UploadMappingFileFailed, report.ServiceErrorCode);
                    }
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void HandleAdminReportTaskStatusUpdate(StackHashAdminReport report, AdminReportException adminReportException)
        {
            lock (_serviceStateLockObject)
            {
                StackHashSyncProgressAdminReport syncReport = report as StackHashSyncProgressAdminReport;
                if (syncReport != null)
                {
                    _syncIsResync = ((syncReport.IsResync) && (!syncReport.IsProductsOnlySync));
                }

                switch (report.Operation)
                {
                    case StackHashAdminOperation.BugReportStarted:
                        this.PluginReportRunning = true;
                        _pluginReportRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.BugReportCompleted:
                        this.PluginReportRunning = false;
                        _pluginReportRunningUpdatedByAdminReport = true;
                        if (report.ServiceErrorCode == StackHashServiceErrorCode.BugTrackerPlugInError)
                        {
                            this.PluginHasError = true;
                        }
                        break;

                    case StackHashAdminOperation.PurgeStarted:
                        this.PurgeRunning = true;
                        _purgeRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.PurgeCompleted:
                        this.PurgeRunning = false;
                        _purgeRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.ErrorIndexMoveStarted:
                        _moveProgressText = null;
                        this.MoveIndexRunning = true;
                        _moveIndexRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.ErrorIndexMoveProgress:
                        this.MoveIndexRunning = true;
                        _moveIndexRunningUpdatedByAdminReport = true;
                        StackHashMoveIndexProgressAdminReport moveAdminReport = report as StackHashMoveIndexProgressAdminReport;
                        if (moveAdminReport != null)
                        {
                            _moveProgressText = moveAdminReport.CurrentFileName == null ? string.Empty : System.IO.Path.GetFileName(moveAdminReport.CurrentFileName);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.ErrorIndexMoveCompleted:
                        _moveProgressText = null;
                        this.MoveIndexRunning = false;
                        _moveIndexRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.AnalyzeStarted:
                        this.AnalyzeRunning = true;
                        _analyzeRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.AnalyzeCompleted:
                        this.AnalyzeRunning = false;
                        _analyzeRunningUpdatedByAdminReport = true;
                        break;

                    case StackHashAdminOperation.WinQualSyncStarted:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        _syncStageText = null;
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressDownloadingProductList:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        _syncStageText = Properties.Resources.Status_WinQualSyncDownloadingProducts;
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressProductListUpdated:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        _syncStageText = Properties.Resources.Status_WinQualSyncProductsComplete;
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressDownloadingProductEvents:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        if ((syncReport != null) && (syncReport.Product != null))
                        {
                            _syncStageText = string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.Status_WinQualSyncDownloadingEvents,
                               syncReport.Product.Product.Name,
                               syncReport.Product.Product.Version);
                        }
                        if (syncReport.Product != null)
                        {
                            UpdateOrAddProductCore(syncReport.Product);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressDownloadingEventPage:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        if ((syncReport != null) && (syncReport.Product != null) && (syncReport.File != null))
                        {
                            _syncStageText = string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.Status_WinQualSyncDownloadingEventPage,
                                syncReport.CurrentPage,
                                Math.Max(syncReport.CurrentPage, syncReport.TotalPages),
                                syncReport.Product.Product.Name,
                                syncReport.Product.Product.Version,
                                syncReport.File.Name);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressProductEventsUpdated:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        if ((syncReport != null) && (syncReport.Product != null))
                        {
                            _syncStageText = string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.Status_WinQualSyncEventsComplete,
                                syncReport.Product.Product.Name,
                                syncReport.Product.Product.Version);
                        }
                        if (syncReport.Product != null)
                        {
                            UpdateOrAddProductCore(syncReport.Product);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressDownloadingProductCabs:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        if ((syncReport != null) && (syncReport.Product != null))
                        {
                            _syncStageText = string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.Status_WinQualSyncDownloadingCabs,
                                syncReport.Product.Product.Name,
                                syncReport.Product.Product.Version);
                        }
                        if (syncReport.Product != null)
                        {
                            UpdateOrAddProductCore(syncReport.Product);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressDownloadingCab:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        if ((syncReport != null) && (syncReport.Product != null) && (syncReport.Cab != null) && (syncReport.TheEvent != null))
                        {
                            _syncStageText = string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.Status_WinQualSyncDownloadingCab,
                                syncReport.Cab.Id,
                                syncReport.TheEvent.Id,
                                syncReport.Product.Product.Name,
                                syncReport.Product.Product.Version);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncProgressProductCabsUpdated:
                        this.SyncRunning = true;
                        _syncRunningUpdatedByAdminReport = true;
                        if ((syncReport != null) && (syncReport.Product != null))
                        {
                            _syncStageText = string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.Status_WinQualSyncCabsComplete,
                                syncReport.Product.Product.Name,
                                syncReport.Product.Product.Version);
                        }
                        if (syncReport.Product != null)
                        {
                            UpdateOrAddProductCore(syncReport.Product);
                        }
                        UpdateServiceStatusText();
                        break;

                    case StackHashAdminOperation.WinQualSyncCompleted:
                        this.SyncRunning = false;
                        _syncRunningUpdatedByAdminReport = true;
                        _syncStageText = null;
                        UpdateServiceStatusText();

                        if (adminReportException == null)
                        {
                            // populate products if requested (the setup wizard asks for this)
                            if (_updateProductListAfterSync)
                            {
                                // wait for the current worker op to complete
                                WaitForWorker();

                                PopulateProducts();
                            }
                        }
                        else
                        {
                            // let the setup wizard know that the sync failed
                            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.SyncFailed, false, report.ServiceErrorCode, adminReportException);
                        }

                        _updateProductListAfterSync = false;

                        ReportSyncComplete(adminReportException == null, report.ServiceErrorCode);
                        break;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void Services_AdminReport(object sender, AdminReportEventArgs e)
        {
            Guid adminReportGuid = Guid.Empty;
            if (e.Report.ClientData != null)
            {
                adminReportGuid = e.Report.ClientData.ApplicationGuid;
            }

            // we're interested in reports for this specific client session or for the current context Id
            if ((e.Report.ContextId == _contextId) || (adminReportGuid == UserSettings.Settings.SessionId))
            {
                // log the reported operation
                DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                    string.Format(CultureInfo.InvariantCulture,
                    "AdminReport: {0} ({1}), {2}, {3}",
                    e.Report.Operation,
                    e.Report.Description,
                    e.Report.ContextId,
                    adminReportGuid));

                // log errors
                AdminReportException adminReportException = null;
                if (e.Report.LastException != null)
                {
                    adminReportException = new AdminReportException(e.Report.LastException);

                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "AdminReport LastException",
                        adminReportException);
                }
                if (e.Report.ServiceErrorCode != StackHashServiceErrorCode.NoError)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning,
                        string.Format("AdminReport ServiceErrorCode: {0}",
                        e.Report.ServiceErrorCode));
                }

                // log purge statistics if present
                StackHashPurgeCompleteAdminReport purgeCompleteAdminReport = e.Report as StackHashPurgeCompleteAdminReport;
                if ((purgeCompleteAdminReport != null) && (purgeCompleteAdminReport.PurgeStatistics != null))
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format("AdminReport PurgeStatistics: Cabs: {0}, Cabs Size: {1}, Dumps: {2}, Dumps Size: {3}",
                        purgeCompleteAdminReport.PurgeStatistics.NumberOfCabs,
                        purgeCompleteAdminReport.PurgeStatistics.CabsTotalSize,
                        purgeCompleteAdminReport.PurgeStatistics.NumberOfDumpFiles,
                        purgeCompleteAdminReport.PurgeStatistics.DumpFilesTotalSize));
                }

                // log sync statistics if present
                StackHashWinQualSyncCompleteAdminReport syncCompleteAdminReport = e.Report as StackHashWinQualSyncCompleteAdminReport;
                if ((syncCompleteAdminReport != null) && (syncCompleteAdminReport.ErrorIndexStatistics != null))
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format(CultureInfo.InvariantCulture,
                        "AdminReport ErrorIndexStatistics: Products: {0}, Files: {1}, Events: {2}, EventInfos: {3}, Cabs: {4}",
                        syncCompleteAdminReport.ErrorIndexStatistics.Products,
                        syncCompleteAdminReport.ErrorIndexStatistics.Files,
                        syncCompleteAdminReport.ErrorIndexStatistics.Events,
                        syncCompleteAdminReport.ErrorIndexStatistics.EventInfos,
                        syncCompleteAdminReport.ErrorIndexStatistics.Cabs));
                }

                // log/handle change in context state
                StackHashContextStateAdminReport contextStateAdminReport = e.Report as StackHashContextStateAdminReport;
                if (contextStateAdminReport != null)
                {
                    // we're only interested if the report concerns the current context
                    if (contextStateAdminReport.ContextId == _contextId)
                    {
                        // log the message
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "AdminReport ContextState: Is Activation Attempt: {0}, Is Active: {1}",
                            contextStateAdminReport.IsActivationAttempt,
                            contextStateAdminReport.IsActive));

                        // see if the report relates to a different client
                        if (adminReportGuid != UserSettings.Settings.SessionId)
                        {
                            // set the state
                            this.OtherClientHasDisabledContext = !contextStateAdminReport.IsActive;

                            // request UI when disabled is true
                            if (this.OtherClientHasDisabledContext)
                            {
                                RequestUi(ClientLogicUIRequest.OtherClientDeactivatedThisContext);
                            }
                        }
                    }
                }

                // update copy progress if present
                StackHashCopyIndexProgressAdminReport copyIndexProgressAdminReport = e.Report as StackHashCopyIndexProgressAdminReport;
                if ((copyIndexProgressAdminReport != null) && (e.Report.ContextId == _contextId))
                {
                    int progress = (int)(((double)copyIndexProgressAdminReport.CurrentEvent * 100.0) / (double)copyIndexProgressAdminReport.TotalEvents);

                    if (progress < 0) { progress = 0; }
                    if (progress > 100) { progress = 100; }

                    UpdateServiceProgress(progress, true);
                }

                // update bug report progress if present
                StackHashBugReportProgressAdminReport bugReportProgressAdminReport = e.Report as StackHashBugReportProgressAdminReport;
                if ((bugReportProgressAdminReport != null) && (e.Report.ContextId == _contextId))
                {
                    int progress = (int)(((double)bugReportProgressAdminReport.CurrentEvent * 100.0) / (double)bugReportProgressAdminReport.TotalEvents);

                    if (progress < 0) { progress = 0; }
                    if (progress > 100) { progress = 100; }

                    UpdateServiceProgress(progress, true);
                }

                // handle plugin error during sync
                StackHashBugTrackerStatusAdminReport bugTrackerStatusAdminReport = e.Report as StackHashBugTrackerStatusAdminReport;
                if ((bugTrackerStatusAdminReport != null) && (e.Report.ContextId == _contextId))
                {
                    // update plugin error status
                    UpdatePluginErrorFlagFromPlugInDiagnostics(bugTrackerStatusAdminReport.PlugInDiagnostics);
                }

                // handle reports aimed at the current context Id
                if (e.Report.ContextId == _contextId)
                {
                    HandleAdminReportTaskStatusUpdate(e.Report, adminReportException);
                }

                // handle reports directed specifically at us
                if (adminReportGuid == UserSettings.Settings.SessionId)
                {
                    HandleAdminReportThisClientInitiated(e.Report, adminReportException);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Debug.Assert(worker != null);

            WorkerArg arg = e.Argument as WorkerArg;
            Debug.Assert(arg != null);

            int retryCount = 0;
            bool retry = true;
            bool retryIsQuota = false;
            WorkerResult result = null;

            while (retry)
            {
                result = null;

                if (retryIsQuota)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format(CultureInfo.InvariantCulture,
                        "QuotaExceededException after {0}, Projects max message increased to {1:n0} bytes and trying again",
                        arg,
                        ServiceProxy.Services.MaxProjectsContractReceivedMessageSize));
                }
                else
                {
                    if (retryCount > 0)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "Service faulted after {0}, retry attempt {1}/{2}",
                            arg,
                            retryCount,
                            FaultedServiceCallRetryLimit));
                    }
                }

                switch (arg.Type)
                {
                    case WorkerType.ServiceConnect:
                        result = WorkerServiceConnect();
                        break;

                    case WorkerType.GetContextSettings:
                        result = WorkerGetContextSettings();
                        break;

                    case WorkerType.SetContextSettings:
                        result = WorkerSetContextSettings(arg as WorkerArgSetContextSettings);
                        break;

                    case WorkerType.GetProductList:
                        result = WorkerGetProductList(arg as WorkerArgGetProductList);
                        break;

                    case WorkerType.StartSync:
                        result = WorkerStartSync(arg as WorkerArgStartSync);
                        break;

                    case WorkerType.GetDebuggerScriptNames:
                        result = WorkerGetDebuggerScriptNames();
                        break;

                    case WorkerType.GetScript:
                        result = WorkerGetScript(arg as WorkerArgGetScript);
                        break;

                    case WorkerType.RemoveScript:
                        result = WorkerRemoveScript(arg as WorkerArgRemoveScript);
                        break;

                    case WorkerType.RenameScript:
                        result = WorkerRenameScript(arg as WorkerArgRenameScript);
                        break;

                    case WorkerType.AddScript:
                        result = WorkerAddScript(arg as WorkerArgAddScript);
                        break;

                    case WorkerType.TestScript:
                        result = WorkerTestScript(arg as WorkerArgTestScript);
                        break;

                    case WorkerType.GetResultFiles:
                        result = WorkerGetResultFiles(arg as WorkerArgGetResultFiles);
                        break;

                    case WorkerType.GetResult:
                        result = WorkerGetResult(arg as WorkerArgGetResult);
                        break;

                    case WorkerType.AddGetEventNotes:
                        result = WorkerAddGetEventNotes(arg as WorkerArgAddGetEventNotes);
                        break;

                    case WorkerType.AddGetCabNotes:
                        result = WorkerAddGetCabNotes(arg as WorkerArgAddGetCabNotes);
                        break;

                    case WorkerType.ActivateStackHashContext:
                        result = WorkerActivateStackHashContext(arg as WorkerArgActivateStackHashContext);
                        break;

                    case WorkerType.DeactivateStackHashContext:
                        result = WorkerDeactivateStackHashContext(arg as WorkerArgDeactivateStackHashContext);
                        break;

                    case WorkerType.RemoveStackHashContext:
                        result = WorkerRemoveStackHashContext(arg as WorkerArgRemoveStackHashContext);
                        break;

                    case WorkerType.CreateNewStackHashContext:
                        result = WorkerCreateNewStackHashContext();
                        break;

                    case WorkerType.EnableLogging:
                        result = WorkerEnableLogging();
                        break;

                    case WorkerType.DisableLogging:
                        result = WorkerDisableLogging();
                        break;

                    case WorkerType.GetCabContents:
                        result = WorkerGetCabContents(arg as WorkerArgGetCabContents);
                        break;

                    case WorkerType.GetCabFile:
                        result = WorkerGetCabFile(arg as WorkerArgGetCabFile);
                        break;

                    case WorkerType.MoveIndex:
                        result = WorkerMoveIndex(arg as WorkerArgMoveIndex);
                        break;

                    case WorkerType.RemoveScriptResult:
                        result = WorkerRemoveScriptResult(arg as WorkerArgRemoveScriptResult);
                        break;

                    case WorkerType.CreateFirstContext:
                        result = WorkerCreateFirstContext(arg as WorkerArgCreateFirstContext);
                        break;

                    case WorkerType.SetProductSyncState:
                        result = WorkerSetProductSyncState(arg as WorkerArgSetProductSyncState);
                        break;

                    case WorkerType.RunWinQualLogOn:
                        result = WorkerRunWinQualLogOn(arg as WorkerArgRunWinQualLogOn);
                        break;

                    case WorkerType.RefreshEventPackage:
                        result = WorkerRefreshEventPackage(arg as WorkerArgRefreshEventPackage);
                        break;

                    case WorkerType.SetProxy:
                        result = WorkerSetProxy(arg as WorkerArgSetProxy);
                        break;

                    case WorkerType.SetEventBugId:
                        result = WorkerSetEventBugid(arg as WorkerArgSetEventBugId);
                        break;

                    case WorkerType.SetEventWorkFlow:
                        result = WorkerSetEventWorkFlow(arg as WorkerArgSetEventWorkFlow);
                        break;

                    case WorkerType.AbortSync:
                        result = WorkerAbortSync(arg as WorkerArgAbortSync);
                        break;

                    case WorkerType.ExportProductList:
                        result = WorkerExportProductList(arg as WorkerArgExportProductList);
                        break;

                    case WorkerType.ExportEventList:
                        result = WorkerExportEventList(arg as WorkerArgExportEventList);
                        break;

                    case WorkerType.GetCollectionPolicies:
                        result = WorkerGetCollectionPolicies(arg as WorkerArgGetCollectionPolicies);
                        break;

                    case WorkerType.UpdateProductProperties:
                        result = WorkerUpdateProductProperties(arg as WorkerArgUpdateProductProperties);
                        break;

                    case WorkerType.UpdateEventPackageProperties:
                        result = WorkerUpdateEventPackageProperties(arg as WorkerArgUpdateEventPackageProperties);
                        break;

                    case WorkerType.SetCollectionPolicies:
                        result = WorkerSetCollectionPolicies(arg as WorkerArgSetCollectionPolicies);
                        break;

                    case WorkerType.DownloadCab:
                        result = WorkerDownloadCab(arg as WorkerArgDownloadCab);
                        break;

                    case WorkerType.GetContextStatus:
                        result = WorkerGetContextStatus(arg as WorkerArgGetContextStatus);
                        break;

                    case WorkerType.EnableDisableReporting:
                        result = WorkerEnableDisableReporting(arg as WorkerArgEnableDisableReporting);
                        break;

                    case WorkerType.CopyIndex:
                        result = WorkerCopyIndex(arg as WorkerArgCopyIndex);
                        break;

                    case WorkerType.CreateTestIndex:
                        result = WorkerCreateTestIndex(arg as WorkerArgCreateTestIndex);
                        break;

                    case WorkerType.GetWindowedEventPackages:
                        result = WorkerGetWindowedEventPackages(arg as WorkerArgGetWindowedEventPackages);
                        break;

                    case WorkerType.UpdateProductSummary:
                        result = WorkerUpdateProductSummary(arg as WorkerArgUpdateProductSummary);
                        break;

                    case WorkerType.TestDatabase:
                        result = WorkerTestDatabase(arg as WorkerArgTestDatabase);
                        break;

                    case WorkerType.GetPluginDiagnostics:
                        result = WorkerGetPluginDiagnostics(arg as WorkerArgGetPluginDiagnostics);
                        break;

                    case WorkerType.SendEventToPlugin:
                        result = WorkerSendEventToPlugin(arg as WorkerArgSendEventToPlugin);
                        break;

                    case WorkerType.SendProductToPlugin:
                        result = WorkerSendProductToPlugin(arg as WorkerArgSendProductToPlugin);
                        break;

                    case WorkerType.SendCabToPlugin:
                        result = WorkerSendCabToPlugin(arg as WorkerArgSendCabToPlugin);
                        break;

                    case WorkerType.SendAllToPlugins:
                        result = WorkerSendAllToPlugins(arg as WorkerArgSendAllToPlugins);
                        break;

                    case WorkerType.UploadMappingFile:
                        result = WorkerUploadMappingFile(arg as WorkerArgUploadMappingFile);
                        break;

                    case WorkerType.NoOp:
                        result = WorkerNoOp();
                        break;
                }

                // determine if we need to retry, assuming not
                retry = false;
                StackHashServiceErrorCode serviceError = ServiceErrorCodeFromException(result.Ex);
                if ((result.Ex is CommunicationException) && (!worker.CancellationPending))
                {
                    if (result.Ex.InnerException is QuotaExceededException)
                    {
                        // in the case that a quota was exceeded try bumping up the projects client max message size
                        long quota = ServiceProxy.Services.MaxProjectsContractReceivedMessageSize;
                        
                        // double the quota
                        quota *= 2;

                        // unless we've blown through MaxInt retry
                        if (quota < Int32.MaxValue)
                        {
                            ServiceProxy.Services.MaxProjectsContractReceivedMessageSize = (int)quota;
                            retry = true;
                            retryIsQuota = true;
                        }
                    }
                    else
                    {
                        // if a service has faulted as a result of a communications exception then a retry makes sense
                        // (if not faulted the there is probably a hard error)
                        if (ServiceProxy.Services.AnyFaulted())
                        {
                            retryCount++;

                            // make sure we haven't reached the retry limit
                            if (retryCount <= FaultedServiceCallRetryLimit)
                            {
                                retry = true;
                                retryIsQuota = false;

                                // ServiceConnect will re-register regardless, if the failure was on
                                // another call then we attempt the re-register before the retry
                                if ((serviceError == StackHashServiceErrorCode.ClientNotRegistered) && (result.Type != WorkerType.ServiceConnect))
                                {
                                    TryReRegisterForNotifications();
                                }
                            }
                        }
                        else
                        {
                            // if the license client limit has been exceeded then retry - this is because we may
                            // be on the same machine as the service but haven't received an admin register message yet
                            // so we don't know it - if the retry happens after we know we're local to the service
                            // then we'll bump another client
                            if ((serviceError == StackHashServiceErrorCode.LicenseClientLimitExceeded) && (result.Type == WorkerType.ServiceConnect))
                            {
                                retryCount++;

                                // make sure we haven't reached the retry limit
                                if (retryCount <= FaultedServiceCallRetryLimit)
                                {
                                    retry = true;
                                    retryIsQuota = false;
                                }
                            }
                            else if (serviceError == StackHashServiceErrorCode.ClientNotRegistered)
                            {
                                // if the client isn't registered then try again - we may have been bumped at some
                                // point but now a license might be free again so we should try to connect

                                retryCount++;

                                // make sure we haven't reached the retry limit
                                if (retryCount <= FaultedServiceCallRetryLimit)
                                {
                                    retry = true;
                                    retryIsQuota = false;

                                    // ServiceConnect will re-register regardless, if the failure was on
                                    // another call then we attempt the re-register before the retry
                                    if (result.Type != WorkerType.ServiceConnect)
                                    {
                                        TryReRegisterForNotifications();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                Debug.Assert(result != null);

                e.Result = result;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ReportError(e.Error, ClientLogicErrorCode.Unexpected);
            }
            else if (!e.Cancelled)
            {
                WorkerResult result = e.Result as WorkerResult;
                Debug.Assert(result != null);

                switch (result.Type)
                {
                    case WorkerType.ServiceConnect:
                        ServiceConnectResult(result as WorkerResultServiceConnect);
                        break;

                    case WorkerType.GetContextSettings:
                        GetContextSettingsResult(result as WorkerResultGetContextSettings);
                        break;

                    case WorkerType.SetContextSettings:
                        SetContextSettingsResult(result as WorkerResultSetContextSettings);
                        break;

                    case WorkerType.GetProductList:
                        GetProductListResult(result as WorkerResultGetProductList);
                        break;

                    case WorkerType.StartSync:
                        StartSyncResult(result as WorkerResultStartSync);
                        break;

                    case WorkerType.GetDebuggerScriptNames:
                        GetDebuggerScriptNamesResult(result as WorkerResultGetDebuggerScriptNames);
                        break;

                    case WorkerType.GetScript:
                        GetScriptResult(result as WorkerResultGetScript);
                        break;

                        // these actions all result in script names being reloaded
                    case WorkerType.RemoveScript:
                    case WorkerType.RenameScript:
                    case WorkerType.AddScript:
                        ReloadScriptNamesResult(result);
                        break;

                    case WorkerType.ResultOnlyRefreshContextSettings:
                        // a generic result that causes the context settings collection to be updated
                        HandleResultResultOnlyRefreshContextSettings(result as WorkerResultResultOnlyRefreshContextSettings);
                        break;

                    case WorkerType.ResultOnlyNoAction:
                        // a generic result that had no next action (error handling only)
                        HandleResultResultOnlyNoAction(result as WorkerResultResultOnlyNoAction);
                        break;

                    case WorkerType.TestScript:
                        TestScriptResult(result as WorkerResultTestScipt);
                        break;

                    case WorkerType.GetResultFiles:
                        GetResultFilesResult(result as WorkerResultGetResultFiles);
                        break;

                    case WorkerType.GetResult:
                        GetResultResult(result as WorkerResultGetResult);
                        break;

                    case WorkerType.AddGetEventNotes:
                        HandleResultAddGetEventNotes(result as WorkerResultAddGetEventNotes);
                        break;

                    case WorkerType.AddGetCabNotes:
                        HandleResultAddGetCabNotes(result as WorkerResultAddGetCabNotes);
                        break;

                    case WorkerType.CreateNewStackHashContext:
                        HandleResultCreateNewStackHashContext(result as WorkerResultCreateNewStackHashContext);
                        break;

                    case WorkerType.GetCabContents:
                        HandleResultGetCabContents(result as WorkerResultGetCabContents);
                        break;

                    case WorkerType.GetCabFile:
                        HandleResultGetCabFile(result as WorkerResultGetCabFile);
                        break;

                    case WorkerType.MoveIndex:
                        HandleResultMoveIndex(result as WorkerResultMoveIndex);
                        break;

                    case WorkerType.RemoveScriptResult:
                        HandleResultRemoveScriptResult(result as WorkerResultRemoveScriptResult);
                        break;

                    case WorkerType.CreateFirstContext:
                        HandleResultCreateFirstContext(result as WorkerResultCreateFirstContext);
                        break;

                    case WorkerType.SetProductSyncState:
                        HandleResultSetProductSyncState(result as WorkerResultSetProductSyncState);
                        break;

                    case WorkerType.RunWinQualLogOn:
                        HandleResultRunWinQualLogOn(result as WorkerResultRunWinQualLogOn);
                        break;

                    case WorkerType.RefreshEventPackage:
                        HandleResultRefreshEventPackage(result as WorkerResultRefeshEventPackage);
                        break;

                    case WorkerType.SetProxy:
                        HandleResultSetProxy(result as WorkerResultSetProxy);
                        break;

                    case WorkerType.SetEventBugId:
                        HandleResultSetEventBugId(result as WorkerResultSetEventBugId);
                        break;

                    case WorkerType.SetEventWorkFlow:
                        HandleResultSetEventWorkFlow(result as WorkerResultSetEventWorkFlow);
                        break;

                    case WorkerType.AbortSync:
                        HandleResultAbortSync(result as WorkerResultAbortSync);
                        break;

                    case WorkerType.Export:
                        HandleResultExport(result as WorkerResultExport);
                        break;

                    case WorkerType.GetCollectionPolicies:
                        HandleResultGetCollectionPolicies(result as WorkerResultGetCollectionPolicies);
                        break;

                    case WorkerType.UpdateProductProperties:
                        HandleResultUpdateProductProperties(result as WorkerResultUpdateProductProperties);
                        break;

                    case WorkerType.UpdateEventPackageProperties:
                        HandleResultUpdateEventPackageProperties(result as WorkerResultUpdateEventPackageProperties);
                        break;

                    case WorkerType.SetCollectionPolicies:
                        HandleResultSetCollectionPolicies(result as WorkerResultSetCollectionPolicies);
                        break;

                    case WorkerType.DownloadCab:
                        HandleResultDownloadCab(result as WorkerResultDownloadCab);
                        break;

                    case WorkerType.GetContextStatus:
                        HandleResultGetContextStatus(result as WorkerResultGetContextStatus);
                        break;

                    case WorkerType.EnableDisableReporting:
                        HandleResultEnableDisableReporting(result as WorkerResultEnableDisableReporting);
                        break;

                   case WorkerType.CopyIndex:
                        HandleResultCopyIndex(result as WorkerResultCopyIndex);
                        break;

                    case WorkerType.CreateTestIndex:
                        HandleResultCreateTestIndex(result as WorkerResultCreateTestIndex);
                        break;

                    case WorkerType.GetWindowedEventPackages:
                        HandleResultGetWindowedEventPackages(result as WorkerResultGetWindowedEventPackages);
                        break;

                    case WorkerType.UpdateProductSummary:
                        HandleResultUpdateProductSummary(result as WorkerResultUpdateProductSummary);
                        break;

                    case WorkerType.TestDatabase:
                        HandleResultTestDatabase(result as WorkerResultTestDatabase);
                        break;

                    case WorkerType.GetPluginDiagnostics:
                        HandleResultGetPluginDiagnostics(result as WorkerResultGetPluginDiagnostics);
                        break;

                    case WorkerType.SendEventToPlugin:
                        HandleResultSendEventToPlugin(result as WorkerResultSendEventToPlugin);
                        break;

                    case WorkerType.SendProductToPlugin:
                        HandleResultSendProductToPlugin(result as WorkerResultSendProductToPlugin);
                        break;

                    case WorkerType.SendCabToPlugin:
                        HandleResultSendCabToPlugin(result as WorkerResultSendCabToPlugin);
                        break;

                    case WorkerType.SendAllToPlugins:
                        HandleResultSendAllToPlugins(result as WorkerResultSendAllToPlugins);
                        break;

                    case WorkerType.UploadMappingFile:
                        HandleResultUploadMappingFile(result as WorkerResultUploadMappingFile);
                        break;

                    case WorkerType.NoOp:
                        HandleResultNoOp();
                        break;
                }
            }
        }

        // NoOp
        private static WorkerResultNoOp WorkerNoOp()
        {
            return new WorkerResultNoOp();
        }

        private void HandleResultNoOp()
        {
            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // UploadMappingFile

        private static WorkerResultUploadMappingFile WorkerUploadMappingFile(WorkerArgUploadMappingFile arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                UploadMappingFileRequest request = new UploadMappingFileRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.MappingFileData = arg.MappingFileData;

                ServiceProxy.Services.Admin.UploadMappingFile(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultUploadMappingFile(returnException);
        }

        private void HandleResultUploadMappingFile(WorkerResultUploadMappingFile result)
        {
            Debug.Assert(result != null);
           
            if (result.Ex != null)
            {
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;

                ReportError(result.Ex, ClientLogicErrorCode.UploadMappingFileFailed);
            }

            // if no error stay busy and wait for the upload to complete
        }

        // SendAllToPlugins

        private static WorkerResultSendAllToPlugins WorkerSendAllToPlugins(WorkerArgSendAllToPlugins arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                StackHashBugReportData bugReportData = new StackHashBugReportData();
                bugReportData.Options = StackHashReportOptions.IncludeAllObjects;

                RunBugReportTaskRequest request = new RunBugReportTaskRequest();
                request.BugReportDataCollection = new StackHashBugReportDataCollection();
                request.BugReportDataCollection.Add(bugReportData);
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.PlugIns = new StackHashBugTrackerPlugInSelectionCollection();
                foreach(string pluginName in arg.Plugins)
                {
                    StackHashBugTrackerPlugInSelection selection = new StackHashBugTrackerPlugInSelection();
                    selection.Name = pluginName;
                    request.PlugIns.Add(selection);
                }

                ServiceProxy.Services.Projects.RunBugReportTask(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSendAllToPlugins(returnException);
        }

        private void HandleResultSendAllToPlugins(WorkerResultSendAllToPlugins result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
        }

        // SendProductToPlugin

        private static WorkerResultSendProductToPlugin WorkerSendProductToPlugin(WorkerArgSendProductToPlugin arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                StackHashBugReportData bugReportData = new StackHashBugReportData();
                bugReportData.Options = StackHashReportOptions.IncludeCabNotes |
                    StackHashReportOptions.IncludeCabs |
                    StackHashReportOptions.IncludeEventNotes |
                    StackHashReportOptions.IncludeEvents |
                    StackHashReportOptions.IncludeScriptResults |
                    StackHashReportOptions.IncludeProducts;

                bugReportData.Product = arg.Product;

                StackHashBugTrackerPlugInSelection pluginSelection = new StackHashBugTrackerPlugInSelection();
                pluginSelection.Name = arg.PluginName;

                RunBugReportTaskRequest request = new RunBugReportTaskRequest();
                request.BugReportDataCollection = new StackHashBugReportDataCollection();
                request.BugReportDataCollection.Add(bugReportData);
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.PlugIns = new StackHashBugTrackerPlugInSelectionCollection();
                request.PlugIns.Add(pluginSelection);

                ServiceProxy.Services.Projects.RunBugReportTask(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSendProductToPlugin(returnException);
        }

        private void HandleResultSendProductToPlugin(WorkerResultSendProductToPlugin result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;

                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            // if no error stay busy until the report completes
        }

        // SendEventToPlugin

        private WorkerResultSendEventToPlugin WorkerSendEventToPlugin(WorkerArgSendEventToPlugin arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                StackHashBugReportData bugReportData = new StackHashBugReportData();
                bugReportData.Options = StackHashReportOptions.IncludeCabNotes |
                    StackHashReportOptions.IncludeCabs |
                    StackHashReportOptions.IncludeEventNotes |
                    StackHashReportOptions.IncludeEvents |
                    StackHashReportOptions.IncludeScriptResults;
                bugReportData.TheEvent = arg.Event;
                bugReportData.Product = arg.Product;
                bugReportData.File = GetFile(arg.Event.FileId);

                StackHashBugTrackerPlugInSelection pluginSelection = new StackHashBugTrackerPlugInSelection();
                pluginSelection.Name = arg.PluginName;

                RunBugReportTaskRequest request = new RunBugReportTaskRequest();
                request.BugReportDataCollection = new StackHashBugReportDataCollection();
                request.BugReportDataCollection.Add(bugReportData);
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.PlugIns = new StackHashBugTrackerPlugInSelectionCollection();
                request.PlugIns.Add(pluginSelection);

                ServiceProxy.Services.Projects.RunBugReportTask(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSendEventToPlugin(returnException);
        }

        private void HandleResultSendEventToPlugin(WorkerResultSendEventToPlugin result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;

                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            // if no error stay busy until the report completes
        }

        // SendCabToPlugin

        private WorkerResultSendCabToPlugin WorkerSendCabToPlugin(WorkerArgSendCabToPlugin arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                StackHashBugReportData bugReportData = new StackHashBugReportData();
                bugReportData.Options = StackHashReportOptions.IncludeCabNotes |
                    StackHashReportOptions.IncludeCabs;

                bugReportData.TheEvent = arg.Event;
                bugReportData.Product = arg.Product;
                bugReportData.File = GetFile(arg.Event.FileId);
                bugReportData.Cab = arg.Cab;

                StackHashBugTrackerPlugInSelection pluginSelection = new StackHashBugTrackerPlugInSelection();
                pluginSelection.Name = arg.PluginName;

                RunBugReportTaskRequest request = new RunBugReportTaskRequest();
                request.BugReportDataCollection = new StackHashBugReportDataCollection();
                request.BugReportDataCollection.Add(bugReportData);
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.PlugIns = new StackHashBugTrackerPlugInSelectionCollection();
                request.PlugIns.Add(pluginSelection);

                ServiceProxy.Services.Projects.RunBugReportTask(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSendCabToPlugin(returnException);
        }

        private void HandleResultSendCabToPlugin(WorkerResultSendCabToPlugin result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;

                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            // if no error stay busy until the report completes
        }

        // GetPluginDiagnostics
        private static WorkerResultGetPluginDiagnostics WorkerGetPluginDiagnostics(WorkerArgGetPluginDiagnostics arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashNameValueCollection diagnostics = null;

            try
            {
                GetBugTrackerPlugInDiagnosticsRequest request = new GetBugTrackerPlugInDiagnosticsRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.PlugInName = arg.PluginName;

                GetBugTrackerPlugInDiagnosticsResponse response = ServiceProxy.Services.Admin.GetBugTrackerDiagnostics(request);
                if (response.BugTrackerPlugInDiagnostics != null)
                {
                    foreach (StackHashBugTrackerPlugInDiagnostics candidate in response.BugTrackerPlugInDiagnostics)
                    {
                        if (string.Compare(arg.PluginName, candidate.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            diagnostics = candidate.Diagnostics;
                            break;
                        }
                    }
                }
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetPluginDiagnostics(returnException, diagnostics);
        }

        private void HandleResultGetPluginDiagnostics(WorkerResultGetPluginDiagnostics result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            if (result.Ex != null)
            {
                this.CurrentPluginDiagnostics = null;
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                this.CurrentPluginDiagnostics = result.Diagnostics;
            }
        }

        // TestDatabase
        private static WorkerResultTestDatabase WorkerTestDatabase(WorkerArgTestDatabase arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashErrorIndexDatabaseStatus status = StackHashErrorIndexDatabaseStatus.Unknown;
            string exceptionText = null;

            try
            {
                TestDatabaseConnectionRequest request = new TestDatabaseConnectionRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.SqlSettings = null;
                request.TestDatabaseExistence = true;

                TestDatabaseConnectionResponse response = ServiceProxy.Services.Admin.TestDatabaseConnection(request);
                status = response.TestResult;
                exceptionText = response.LastException;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultTestDatabase(returnException, status, exceptionText);
        }

        private void HandleResultTestDatabase(WorkerResultTestDatabase result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                this.LastDatabaseTestExceptionText = result.TestLastExceptionText;
                this.LastDatabaseTestStatus = result.TestStatus;

                RequestUi(ClientLogicUIRequest.DatabaseTestComplete);
            }
        }

        // UpdateProductSummary

        private static WorkerResultUpdateProductSummary WorkerUpdateProductSummary(WorkerArgUpdateProductSummary arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashProductHitDateSummaryCollection hitDateSummary = null;
            StackHashProductLocaleSummaryCollection localeSummary = null;
            StackHashProductOperatingSystemSummaryCollection osSummary = null;

            try
            {
                GetProductRollupRequest request = new GetProductRollupRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.ProductId = arg.ProductId;

                GetProductRollupResponse response = ServiceProxy.Services.Projects.GetProductSummary(request);
                if (response.RollupData != null)
                {
                    hitDateSummary = response.RollupData.HitDateSummary;
                    localeSummary = response.RollupData.LocaleSummaryCollection;
                    osSummary = response.RollupData.OperatingSystemSummary;
                }
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultUpdateProductSummary(returnException, arg.ProductId, hitDateSummary, localeSummary, osSummary);
        }

        private void HandleResultUpdateProductSummary(WorkerResultUpdateProductSummary result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                this.Products[_productIdToProductsIndex[result.ProductId]].UpdateHitSummaries(result.HitDateSummary,
                    result.LocaleSummary,
                    result.OsSummary);

                RequestUi(ClientLogicUIRequest.ProductSummaryUpdated);
            }
        }

        // CreateTestIndex

        private static WorkerResultCreateTestIndex WorkerCreateTestIndex(WorkerArgCreateTestIndex arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                CreateTestIndexRequest request = new CreateTestIndexRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.TestIndexData = arg.TestIndexData;

                ServiceProxy.Services.Test.CreateTestIndex(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultCreateTestIndex(returnException);
        }

        private void HandleResultCreateTestIndex(WorkerResultCreateTestIndex result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                RequestUi(ClientLogicUIRequest.TestIndexCreated);
            }
        }

        // EnableDisableReporting

        private static WorkerResultEnableDisableReporting WorkerEnableDisableReporting(WorkerArgEnableDisableReporting arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                if (arg.EnableReporting)
                {
                    EnableReportingRequest request = new EnableReportingRequest();
                    request.ClientData = UserSettings.Settings.GenerateClientData();
                    ServiceProxy.Services.Admin.EnableReporting(request);
                }
                else
                {
                    DisableReportingRequest request = new DisableReportingRequest();
                    request.ClientData = UserSettings.Settings.GenerateClientData();
                    ServiceProxy.Services.Admin.DisableReporting(request);
                }
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultEnableDisableReporting(returnException, arg.EnableReporting);
        }

        private void HandleResultEnableDisableReporting(WorkerResultEnableDisableReporting result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                this.ServiceReportingEnabled = result.EnableReporting;
            }

            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.ReportingUpdated, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        // GetContextStatus

        private static WorkerResultGetContextStatus WorkerGetContextStatus(WorkerArgGetContextStatus arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashContextStatus contextStatus = null;

            try
            {
                GetStackHashServiceStatusRequest request = new GetStackHashServiceStatusRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse response = ServiceProxy.Services.Admin.GetServiceStatus(request);
                if ((response.Status != null) && (response.Status.ContextStatusCollection != null))
                {
                    foreach (StackHashContextStatus status in response.Status.ContextStatusCollection)
                    {
                        if (status.ContextId == arg.ContextId)
                        {
                            contextStatus = status;
                            break;
                        }
                    }
                }
                
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetContextStatus(returnException, contextStatus);
        }

        private void HandleResultGetContextStatus(WorkerResultGetContextStatus result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                this.ContextStatus = result.ContextStatus;
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;

                RequestUi(ClientLogicUIRequest.ContextStatusReady);
            }
            else
            {
                // report error
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;

                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
        }

        // DownloadCab

        private WorkerResultDownloadCab WorkerDownloadCab(WorkerArgDownloadCab arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                DownloadCabRequest request = new DownloadCabRequest();
                request.Cab = arg.Cab;
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = GetFile(arg.Event.FileId);
                request.Product = arg.Product;

                ServiceProxy.Services.Projects.DownloadCab(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultDownloadCab(returnException);
        }

        private void HandleResultDownloadCab(WorkerResultDownloadCab result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
            
            // do nothing on success - async call
        }

        // SetCollectionPolicies

        private static WorkerResultSetCollectionPolicies WorkerSetCollectionPolicies(WorkerArgSetCollectionPolicies arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashCollectionPolicyCollection collectionPolicies = null;

            try
            {
                SetDataCollectionPolicyRequest setRequest = new SetDataCollectionPolicyRequest();
                setRequest.ClientData = UserSettings.Settings.GenerateClientData();
                setRequest.ContextId = arg.ContextId;
                setRequest.PolicyCollection = arg.CollectionPolicies;
                setRequest.SetAll = false;

                ServiceProxy.Services.Admin.SetDataCollectionPolicy(setRequest);

                GetDataCollectionPolicyRequest request = new GetDataCollectionPolicyRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ConditionObject = StackHashCollectionObject.Any;
                request.ContextId = arg.ContextId;
                request.GetAll = true;
                request.Id = 0;
                request.ObjectToCollect = StackHashCollectionObject.Any;
                request.RootObject = StackHashCollectionObject.Any;

                GetDataCollectionPolicyResponse response = ServiceProxy.Services.Admin.GetDataCollectionPolicy(request);
                collectionPolicies = response.PolicyCollection;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSetCollectionPolicies(returnException, collectionPolicies);
        }

        private void HandleResultSetCollectionPolicies(WorkerResultSetCollectionPolicies result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateCollectionPoliciesCore(result.CollectionPolicies);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.CollectionPoliciesSet, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        // UpdateEventPackageProperties

        private static WorkerResultUpdateEventPackageProperties WorkerUpdateEventPackageProperties(WorkerArgUpdateEventPackageProperties arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashCollectionPolicyCollection collectionPolicies = null;

            try
            {
                // remove policies (if any)
                if ((arg.CollectionPoliciesToRemove != null) && (arg.CollectionPoliciesToRemove.Count > 0))
                {
                    foreach (StackHashCollectionPolicy policyToRemove in arg.CollectionPoliciesToRemove)
                    {
                        RemoveDataCollectionPolicyRequest removeRequest = new RemoveDataCollectionPolicyRequest();
                        removeRequest.ClientData = UserSettings.Settings.GenerateClientData();
                        removeRequest.ConditionObject = policyToRemove.ConditionObject;
                        removeRequest.ContextId = arg.ContextId;
                        removeRequest.Id = policyToRemove.RootId;
                        removeRequest.ObjectToCollect = policyToRemove.ObjectToCollect;
                        removeRequest.RootObject = policyToRemove.RootObject;

                        ServiceProxy.Services.Admin.RemoveDataCollectionPolicy(removeRequest);
                    }
                }

                // update policies (if any)
                if ((arg.CollectionPolicies != null) && (arg.CollectionPolicies.Count > 0))
                {
                    SetDataCollectionPolicyRequest setRequest = new SetDataCollectionPolicyRequest();
                    setRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    setRequest.ContextId = arg.ContextId;
                    setRequest.PolicyCollection = arg.CollectionPolicies;
                    setRequest.SetAll = false;

                    ServiceProxy.Services.Admin.SetDataCollectionPolicy(setRequest);
                }

                GetDataCollectionPolicyRequest request = new GetDataCollectionPolicyRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ConditionObject = StackHashCollectionObject.Any;
                request.ContextId = arg.ContextId;
                request.GetAll = true;
                request.Id = 0;
                request.ObjectToCollect = StackHashCollectionObject.Any;
                request.RootObject = StackHashCollectionObject.Any;

                GetDataCollectionPolicyResponse response = ServiceProxy.Services.Admin.GetDataCollectionPolicy(request);
                collectionPolicies = response.PolicyCollection;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultUpdateEventPackageProperties(returnException, collectionPolicies);
        }

        private void HandleResultUpdateEventPackageProperties(WorkerResultUpdateEventPackageProperties result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateCollectionPoliciesCore(result.CollectionPolicies);
                RequestUi(ClientLogicUIRequest.EventPackagePropertiesUpdated);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // UpdateProductProperties

        private static WorkerResultUpdateProductProperties WorkerUpdateProductProperties(WorkerArgUpdateProductProperties arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashCollectionPolicyCollection collectionPolicies = null;

            try
            {
                // remove policies (if any)
                if ((arg.CollectionPoliciesToRemove != null) && (arg.CollectionPoliciesToRemove.Count > 0))
                {
                    foreach (StackHashCollectionPolicy policyToRemove in arg.CollectionPoliciesToRemove)
                    {
                        RemoveDataCollectionPolicyRequest removeRequest = new RemoveDataCollectionPolicyRequest();
                        removeRequest.ClientData = UserSettings.Settings.GenerateClientData();
                        removeRequest.ConditionObject = policyToRemove.ConditionObject;
                        removeRequest.ContextId = arg.ContextId;
                        removeRequest.Id = policyToRemove.RootId;
                        removeRequest.ObjectToCollect = policyToRemove.ObjectToCollect;
                        removeRequest.RootObject = policyToRemove.RootObject;

                        ServiceProxy.Services.Admin.RemoveDataCollectionPolicy(removeRequest);
                    }
                }

                // update policies (if any)
                if ((arg.CollectionPolicies != null) && (arg.CollectionPolicies.Count > 0))
                {
                    SetDataCollectionPolicyRequest setRequest = new SetDataCollectionPolicyRequest();
                    setRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    setRequest.ContextId = arg.ContextId;
                    setRequest.PolicyCollection = arg.CollectionPolicies;
                    setRequest.SetAll = false;

                    ServiceProxy.Services.Admin.SetDataCollectionPolicy(setRequest);
                }

                GetDataCollectionPolicyRequest request = new GetDataCollectionPolicyRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ConditionObject = StackHashCollectionObject.Any;
                request.ContextId = arg.ContextId;
                request.GetAll = true;
                request.Id = 0;
                request.ObjectToCollect = StackHashCollectionObject.Any;
                request.RootObject = StackHashCollectionObject.Any;

                GetDataCollectionPolicyResponse response = ServiceProxy.Services.Admin.GetDataCollectionPolicy(request);
                collectionPolicies = response.PolicyCollection;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultUpdateProductProperties(returnException, collectionPolicies);
        }

        private void HandleResultUpdateProductProperties(WorkerResultUpdateProductProperties result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateCollectionPoliciesCore(result.CollectionPolicies);
                RequestUi(ClientLogicUIRequest.ProductPropertiesUpdated);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }


        // GetCollectionPolicies

        private static WorkerResultGetCollectionPolicies WorkerGetCollectionPolicies(WorkerArgGetCollectionPolicies arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashCollectionPolicyCollection collectionPolicies = null;

            try
            {
                GetDataCollectionPolicyRequest request = new GetDataCollectionPolicyRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ConditionObject = StackHashCollectionObject.Any;
                request.ContextId = arg.ContextId;
                request.GetAll = true;
                request.Id = 0;
                request.ObjectToCollect = StackHashCollectionObject.Any;
                request.RootObject = StackHashCollectionObject.Any;

                GetDataCollectionPolicyResponse response = ServiceProxy.Services.Admin.GetDataCollectionPolicy(request);
                collectionPolicies = response.PolicyCollection;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetCollectionPolicies(returnException, collectionPolicies);
        }

        private void HandleResultGetCollectionPolicies(WorkerResultGetCollectionPolicies result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateCollectionPoliciesCore(result.CollectionPolicies);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        /// <summary>
        /// Refreshes product and event package policies from the most recent policy collection (if present)
        /// </summary>
        public void RefreshCurrentPolicies()
        {
            if (_collectionPolicies != null)
            {
                if (this.Products != null)
                {
                    foreach (DisplayProduct product in this.Products)
                    {
                        product.UpdateCabCollectionPolicy(_collectionPolicies);
                        product.UpdateEventCollectionPolicy(_collectionPolicies);
                        product.UpdateDisplayFilter();
                    }
                }

                if (this.EventPackages != null)
                {
                    foreach (DisplayEventPackage eventPackage in this.EventPackages)
                    {
                        eventPackage.UpdateCabCollectionPolicy(_collectionPolicies);
                    }
                }
            }
        }

        private void UpdateCollectionPoliciesCore(StackHashCollectionPolicyCollection policyCollection)
        {
            if (_guiDispatcher.CheckAccess())
            {
                _collectionPolicies = new List<StackHashCollectionPolicy>(policyCollection);
                RefreshCurrentPolicies();
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashCollectionPolicyCollection>(UpdateCollectionPoliciesCore), policyCollection);
            }
        }

        // ExportEventList
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static WorkerResultExport WorkerExportEventList(WorkerArgExportEventList arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.CheckCharacters = true;
                writerSettings.CloseOutput = true;
                writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
                writerSettings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(arg.ExportPath, writerSettings))
                {
                    writer.WriteStartElement(ExportElementEventList);

                    foreach (DisplayEventPackage eventPackage in arg.EventPackages) 
                    {
                        writer.WriteStartElement(ExportElementRow);

                        // EventType
                        writer.WriteStartElement(ExportElementEventType);
                        writer.WriteString(eventPackage.EventTypeName);
                        writer.WriteEndElement();

                        // ActiveResponse
                        writer.WriteStartElement(ExportElementActiveResponse);
                        // !!! TBD
                        writer.WriteEndElement();

                        // TotalHits
                        writer.WriteStartElement(ExportElementTotalHits);
                        writer.WriteValue(eventPackage.TotalHits);
                        writer.WriteEndElement();

                        // AvgHits
                        writer.WriteStartElement(ExportElementAvgHits);
                        // !!! TBD
                        writer.WriteEndElement();

                        // GrowthPercent
                        writer.WriteStartElement(ExportElementGrowthPercent);
                        // !!! TBD
                        writer.WriteEndElement();

                        // ApplicationName
                        writer.WriteStartElement(ExportElementApplicationName);
                        writer.WriteString(eventPackage.ApplicationName);
                        writer.WriteEndElement();

                        // ApplicationVersion
                        writer.WriteStartElement(ExportElementApplicationVersion);
                        writer.WriteString(eventPackage.ApplicationVersion);
                        writer.WriteEndElement();

                        // ModuleName
                        writer.WriteStartElement(ExportElementModuleName);
                        writer.WriteString(eventPackage.ModuleName);
                        writer.WriteEndElement();

                        // ModuleVersion
                        writer.WriteStartElement(ExportElementModuleVersion);
                        writer.WriteString(eventPackage.ModuleVersion);
                        writer.WriteEndElement();

                        // Offset
                        writer.WriteStartElement(ExportElementOffset);
                        writer.WriteString(string.Format(CultureInfo.CurrentCulture, "0x{0:x}", eventPackage.Offset));
                        writer.WriteEndElement();

                        // EventID
                        writer.WriteStartElement(ExportElementEventID);
                        writer.WriteValue(eventPackage.Id);
                        writer.WriteEndElement();

                        // ProductID
                        writer.WriteStartElement(ExportElementProductID);
                        writer.WriteValue(eventPackage.ProductId);
                        writer.WriteEndElement();

                        // ExceptionCode
                        writer.WriteStartElement(ExportElementExceptionCode);
                        if (eventPackage.ExceptionCode != 0)
                        {
                            writer.WriteString(string.Format(CultureInfo.CurrentCulture, "0x{0:X8}", eventPackage.ExceptionCode));
                        }
                        writer.WriteEndElement();

                        // ApplicationTimestamp
                        writer.WriteStartElement(ExportElementApplicationTimestamp);
                        writer.WriteString(eventPackage.ApplicationTimeStamp.ToString("u", CultureInfo.CurrentCulture));
                        writer.WriteEndElement();

                        // ModeuleTimestamp
                        writer.WriteStartElement(ExportElementModeuleTimestamp);
                        writer.WriteString(eventPackage.ModuleTimeStamp.ToString("u", CultureInfo.CurrentCulture));
                        writer.WriteEndElement();

                        // Reference
                        writer.WriteStartElement(ExportElementReference);
                        if (!string.IsNullOrEmpty(eventPackage.BugId))
                        {
                            writer.WriteCData(eventPackage.BugId);
                        }
                        writer.WriteEndElement();

                        if (arg.ExportCabsAndEventInfos)
                        {
                            // Cabs
                            writer.WriteStartElement(ExportElementCabs);
                            foreach (DisplayCab cab in eventPackage.Cabs)
                            {
                                writer.WriteStartElement(ExportElementCab);

                                // Id
                                writer.WriteStartElement(ExportElementCabID);
                                writer.WriteValue(cab.Id);
                                writer.WriteEndElement();

                                // DateCreated
                                writer.WriteStartElement(ExportElementDateCreated);
                                writer.WriteString(cab.DateCreatedLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                                writer.WriteEndElement();

                                // DateModified
                                writer.WriteStartElement(ExportElementDateModified);
                                writer.WriteString(cab.DateModifiedLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                                writer.WriteEndElement();

                                // SizeInBytes
                                writer.WriteStartElement(ExportElementSizeInBytes);
                                writer.WriteValue(cab.SizeInBytes);
                                writer.WriteEndElement();

                                // DotNetVersion
                                writer.WriteStartElement(ExportElementDotNetVersion);
                                writer.WriteString(cab.DotNetVersion);
                                writer.WriteEndElement();

                                // MachineArchitecture
                                writer.WriteStartElement(ExportElementMachineArchitecture);
                                writer.WriteString(cab.MachineArchitecture);
                                writer.WriteEndElement();

                                // OSVersion
                                writer.WriteStartElement(ExportElementOSVersion);
                                writer.WriteString(cab.OSVersion);
                                writer.WriteEndElement();

                                // ProcessUptime
                                writer.WriteStartElement(ExportElementProcessUptime);
                                writer.WriteString(cab.ProcessUpTime);
                                writer.WriteEndElement();

                                // SystemUptime
                                writer.WriteStartElement(ExportElementSystemUptime);
                                writer.WriteString(cab.SystemUpTime);
                                writer.WriteEndElement();

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            // Event Infos
                            writer.WriteStartElement(ExportElementEventInfos);
                            foreach (DisplayEventInfo eventInfo in eventPackage.EventInfoList)
                            {
                                writer.WriteStartElement(ExportElementEventInfo);

                                // TotalHits
                                writer.WriteStartElement(ExportElementTotalHits);
                                writer.WriteValue(eventInfo.TotalHits);
                                writer.WriteEndElement();

                                // OSName
                                writer.WriteStartElement(ExportElementOSName);
                                writer.WriteString(eventInfo.OperatingSystemName);
                                writer.WriteEndElement();

                                // OSVersion
                                writer.WriteStartElement(ExportElementOSVersion);
                                writer.WriteString(eventInfo.OperatingSystemVersion);
                                writer.WriteEndElement();

                                // Language
                                writer.WriteStartElement(ExportElementLanguage);
                                writer.WriteString(eventInfo.Language);
                                writer.WriteEndElement();

                                // Lcid
                                writer.WriteStartElement(ExportElementLcid);
                                writer.WriteValue(eventInfo.Lcid);
                                writer.WriteEndElement();

                                // Locale
                                writer.WriteStartElement(ExportElementLocale);
                                writer.WriteString(eventInfo.Locale);
                                writer.WriteEndElement();

                                // HitDate
                                writer.WriteStartElement(ExportElementHitDate);
                                writer.WriteString(eventInfo.HitDateLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                                writer.WriteEndElement();

                                // DateCreated
                                writer.WriteStartElement(ExportElementDateCreated);
                                writer.WriteString(eventInfo.DateCreatedLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                                writer.WriteEndElement();

                                // DateModified
                                writer.WriteStartElement(ExportElementDateModified);
                                writer.WriteString(eventInfo.DateModifiedLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                                writer.WriteEndElement();

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                returnException = ex;
            }

            return new WorkerResultExport(returnException);
            
        }

        // ExportProductList

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static WorkerResultExport WorkerExportProductList(WorkerArgExportProductList arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                // used to filter products that have never been synced
                DateTime noDate = new DateTime(2, 1, 1);

                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.CheckCharacters = true;
                writerSettings.CloseOutput = true;
                writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
                writerSettings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(arg.ExportPath, writerSettings))
                {
                    writer.WriteStartElement(ExportElementProductRollup);

                    foreach (DisplayProduct product in arg.Products)
                    {
                        writer.WriteStartElement(ExportElementRow);

                        // ProductName
                        writer.WriteStartElement(ExportElementProductName);
                        writer.WriteString(product.Name);
                        writer.WriteEndElement();

                        // ProductVersion
                        writer.WriteStartElement(ExportElementProductVersion);
                        writer.WriteString(product.Version);
                        writer.WriteEndElement();

                        // TotalEvents
                        writer.WriteStartElement(ExportElementTotalEvents);
                        writer.WriteValue(product.TotalEvents);
                        writer.WriteEndElement();

                        // TotalResponses
                        writer.WriteStartElement(ExportElementTotalResponses);
                        writer.WriteValue(product.TotalResponses);
                        writer.WriteEndElement();

                        // DateCreated
                        writer.WriteStartElement(ExportElementDateCreated);
                        writer.WriteString(product.DateCreatedLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                        writer.WriteEndElement();

                        // DateModified
                        writer.WriteStartElement(ExportElementDateModified);
                        writer.WriteString(product.DateModifiedLocal.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                        writer.WriteEndElement();

                        // Id
                        writer.WriteStartElement(ExportElementProductID);
                        writer.WriteValue(product.Id);
                        writer.WriteEndElement();

                        // LastSynchronizeTime
                        writer.WriteStartElement(ExportElementLastSyncUTC);
                        if (product.LastSynchronizeTime > noDate)
                        {
                            writer.WriteString(product.LastSynchronizeTime.ToUniversalTime().ToString("u", CultureInfo.CurrentCulture));
                        }
                        writer.WriteEndElement();

                        // SynchronizeEnabled
                        writer.WriteStartElement(ExportElementSyncEnabled);
                        writer.WriteValue(product.SynchronizeEnabled);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                returnException = ex;
            }

            return new WorkerResultExport(returnException);
        }

        private void HandleResultExport(WorkerResultExport result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                RequestUi(ClientLogicUIRequest.ExportComplete);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ExportFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // AbortSync

        private static WorkerResultAbortSync WorkerAbortSync(WorkerArgAbortSync arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                AbortSynchronizationRequest request = new AbortSynchronizationRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;

                ServiceProxy.Services.Admin.AbortSynchronization(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultAbortSync(returnException);
        }

        private void HandleResultAbortSync(WorkerResultAbortSync result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // SetEventWorkFlow

        private WorkerResultSetEventWorkFlow WorkerSetEventWorkFlow(WorkerArgSetEventWorkFlow arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                StackHashFile file = GetFile(arg.Event.FileId);

                SetEventWorkFlowStatusRequest request = new SetEventWorkFlowStatusRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = file;
                request.Product = arg.Product;
                request.WorkFlowStatus = arg.workFlowId;

                ServiceProxy.Services.Projects.SetWorkFlowStatus(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSetEventWorkFlow(returnException);
        }

        private void HandleResultSetEventWorkFlow(WorkerResultSetEventWorkFlow result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // SetEventBugId

        private WorkerResultSetEventBugId WorkerSetEventBugid(WorkerArgSetEventBugId arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                StackHashFile file = GetFile(arg.Event.FileId);

                SetEventBugIdRequest bugIdRequest = new SetEventBugIdRequest();
                bugIdRequest.BugId = arg.BugId;
                bugIdRequest.ClientData = UserSettings.Settings.GenerateClientData();
                bugIdRequest.ContextId = arg.ContextId;
                bugIdRequest.Event = arg.Event;
                bugIdRequest.File = file;
                bugIdRequest.Product = arg.Product;

                ServiceProxy.Services.Projects.SetEventBugId(bugIdRequest);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSetEventBugId(returnException);
        }

        private void HandleResultSetEventBugId(WorkerResultSetEventBugId result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);  
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // SetProxy

        private static WorkerResultSetProxy WorkerSetProxy(WorkerArgSetProxy arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashStatus status = null;
            StackHashContextCollection contextCollection = null;

            try
            {
                if (arg.ServiceIsLocal)
                {
                    SetProxyRequest request = new SetProxyRequest();
                    request.ClientData = UserSettings.Settings.GenerateClientData();
                    request.ProxySettings = arg.ProxySettings;

                    ServiceProxy.Services.Admin.SetProxy(request);

                    SetClientTimeoutRequest timeoutRequest = new SetClientTimeoutRequest();
                    timeoutRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    timeoutRequest.ClientTimeoutInSeconds = arg.ClientTimeoutInSeconds;

                    ServiceProxy.Services.Admin.SetClientTimeout(timeoutRequest);
                }

                GetStackHashServiceStatusRequest statusRequest = new GetStackHashServiceStatusRequest();
                statusRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse statusResponse = ServiceProxy.Services.Admin.GetServiceStatus(statusRequest);
                status = statusResponse.Status;

                GetStackHashPropertiesRequest propertiesRequest = new GetStackHashPropertiesRequest();
                propertiesRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashPropertiesResponse propertiesResponse = ServiceProxy.Services.Admin.GetStackHashSettings(propertiesRequest);
                contextCollection = propertiesResponse.Settings.ContextCollection;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSetProxy(returnException, status, arg.ContextId, contextCollection);
        }

        private void HandleResultSetProxy(WorkerResultSetProxy result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                // update state for the current context
                UpdateServiceState(result.ServiceStatus, result.ContextId);

                UpdateContextListCore(result.ContextCollection);
                UpdateCurrentContext();

                // notify the UI that proxy settings have been updated
                RequestUi(ClientLogicUIRequest.ProxySettingsUpdated);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.ProxySettingsUpdated, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        // RefreshEventPackage

        private WorkerResultRefeshEventPackage WorkerRefreshEventPackage(WorkerArgRefreshEventPackage arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashEventPackage eventPackage = null;

            try
            {
                StackHashFile file = GetFile(arg.Event.FileId);

                GetEventPackageRequest request = new GetEventPackageRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = file;
                request.Product = arg.Product;

                GetEventPackageResponse response = ServiceProxy.Services.Projects.GetEventPackage(request);
                eventPackage = response.EventPackage;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultRefeshEventPackage(returnException, eventPackage);
        }

        private void HandleResultRefreshEventPackage(WorkerResultRefeshEventPackage result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateEventPackageCore(result.EventPackage);
                RequestUi(ClientLogicUIRequest.EventPackageRefreshComplete);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // RunWinQualLogOn

        private static WorkerResultRunWinQualLogOn WorkerRunWinQualLogOn(WorkerArgRunWinQualLogOn arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                RunWinQualLogOnRequest request = new RunWinQualLogOnRequest();
                request.ContextId = arg.ContextId;
                request.UserName = arg.UserName;
                request.Password = arg.Password;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.RunWinQualLogOn(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultRunWinQualLogOn(returnException);
        }

        private void HandleResultRunWinQualLogOn(WorkerResultRunWinQualLogOn result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
                NotifySetupWizard(ClientLogicSetupWizardPromptOperation.LogOnTestComplete, false, StackHashServiceErrorCode.NoError, result.Ex);
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }

            // if no error wait for call to complete
        }

        // SetProductSyncState
        private static WorkerResultSetProductSyncState WorkerSetProductSyncState(WorkerArgSetProductSyncState arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashProductInfoCollection productCollection = null;
            DateTime lastWinQualSiteUpdate = DateTime.MinValue;

            try
            {
                // update sync state
                SetProductSynchronizationStateRequest syncRequest = new SetProductSynchronizationStateRequest();
                syncRequest.ClientData = UserSettings.Settings.GenerateClientData();
                syncRequest.ContextId = arg.ContextId;
                syncRequest.Enabled = arg.SyncState;
                syncRequest.ProductId = arg.ProductId;
                ServiceProxy.Services.Admin.SetProductSynchronizationState(syncRequest);

                // get the product list again
                GetProductsRequest productsRequest = new GetProductsRequest();
                productsRequest.ClientData = UserSettings.Settings.GenerateClientData();
                productsRequest.ContextId = arg.ContextId;
                GetProductsResponse productsResponse = ServiceProxy.Services.Projects.GetProducts(productsRequest);

                productCollection = productsResponse.Products;
                lastWinQualSiteUpdate = productsResponse.LastSiteUpdateTime;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSetProductSyncState(returnException, productCollection, lastWinQualSiteUpdate);
        }

        private void HandleResultSetProductSyncState(WorkerResultSetProductSyncState result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateProductListCore(result.ProductCollection, result.LastWinQualSiteUpdate);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.ProductListUpdated, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        // CreateFirstContext

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static WorkerResultCreateFirstContext WorkerCreateFirstContext(WorkerArgCreateFirstContext arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            int contextId = UserSettings.InvalidContextId;
            StackHashSettings returnSettings = null;

            try
            {
                StackHashContextSettings contextSettings = null;
                int currentContextId = UserSettings.Settings.CurrentContextId;

                // make sure that NETWORK SERVICE has access to the index folder
                FolderPermissionHelper.NSAddAccess(arg.IndexFolder);

                // this might get called more than once so we also need to handle updating an existing context
                GetStackHashPropertiesRequest propertiesRequest = new GetStackHashPropertiesRequest();
                propertiesRequest.ClientData = UserSettings.Settings.GenerateClientData();
                GetStackHashPropertiesResponse propertiesResponse = ServiceProxy.Services.Admin.GetStackHashSettings(propertiesRequest);

                // find existing settings by Id
                if ((propertiesResponse.Settings != null ) && (propertiesResponse.Settings.ContextCollection != null))
                {
                    foreach (StackHashContextSettings settings in propertiesResponse.Settings.ContextCollection)
                    {
                        if (settings.Id == currentContextId)
                        {
                            contextSettings = settings;
                            break;
                        }
                    }
                }

                // create new settings if necessary
                if (contextSettings == null)
                {
                    CreateNewStackHashContextRequest createRequest = new CreateNewStackHashContextRequest();
                    createRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    createRequest.IndexType = ErrorIndexType.SqlExpress;
                    CreateNewStackHashContextResponse createResponse = ServiceProxy.Services.Admin.CreateNewStackHashContext(createRequest);

                    contextSettings = createResponse.Settings;
                    contextSettings.WinQualSettings.EnableNewProductsAutomatically = true;

                    // set schedules
                    SetNewContextSchedulesAndDebuggerDefaults(ref contextSettings);
                }

                // update context
                contextSettings.ErrorIndexSettings.Folder = arg.IndexFolder;
                contextSettings.ErrorIndexSettings.Name = arg.ContextName;
                contextSettings.ErrorIndexSettings.Location = arg.IsDatabaseInIndexFolder ? StackHashErrorIndexLocation.InCabFolder : StackHashErrorIndexLocation.OnSqlServer;
                contextSettings.WinQualSettings.CompanyName = arg.ProfileName;
                contextSettings.WinQualSettings.UserName = arg.Username;
                contextSettings.WinQualSettings.Password = arg.Password;
                
                // database settings
                contextSettings.SqlSettings = new StackHashSqlConfiguration();
                contextSettings.SqlSettings.ConnectionString = arg.ConnectionString;
                contextSettings.SqlSettings.InitialCatalog = arg.ContextName;
                contextSettings.SqlSettings.ConnectionTimeout = DefaultSqlConnectionTimeout;
                contextSettings.SqlSettings.EventsPerBlock = DefaultSqlEventsPerBlock;
                contextSettings.SqlSettings.MaxPoolSize = DefaultSqlMaxPoolSize;
                contextSettings.SqlSettings.MinPoolSize = DefaultSqlMinPoolSize;

                if (!string.IsNullOrEmpty(arg.CdbPath32))
                {
                    contextSettings.DebuggerSettings.DebuggerPathAndFileName = arg.CdbPath32;
                }
                else
                {
                    contextSettings.DebuggerSettings.DebuggerPathAndFileName = string.Empty;
                }

                if (!string.IsNullOrEmpty(arg.CdbPath64))
                {
                    contextSettings.DebuggerSettings.DebuggerPathAndFileName64Bit = arg.CdbPath64;
                }
                else
                {
                    contextSettings.DebuggerSettings.DebuggerPathAndFileName64Bit = string.Empty;
                }

                // save the context id
                UserSettings.Settings.CurrentContextId = contextSettings.Id;
                contextId = contextSettings.Id;

                // save the context setttings
                SetStackHashPropertiesRequest setRequest = new SetStackHashPropertiesRequest();
                setRequest.ClientData = UserSettings.Settings.GenerateClientData();
                setRequest.Settings = propertiesResponse.Settings;
                setRequest.Settings.ContextCollection.Clear();
                setRequest.Settings.ContextCollection.Add(contextSettings);
                UpdatePropertiesRequestToUNC(ref setRequest);
                ServiceProxy.Services.Admin.SetStackHashSettings(setRequest);

                // activate the context if necessary
                if (!contextSettings.IsActive)
                {
                    ActivateStackHashContextRequest activateRequest = new ActivateStackHashContextRequest();
                    activateRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    activateRequest.ContextId = UserSettings.Settings.CurrentContextId;
                    ServiceProxy.Services.Admin.ActivateStackHashContext(activateRequest);
                }

                // set defualt global collection policies
                SetDefaultCollectionPolicy(UserSettings.Settings.CurrentContextId);

                // get settings again
                propertiesRequest.ClientData = UserSettings.Settings.GenerateClientData();
                propertiesResponse = ServiceProxy.Services.Admin.GetStackHashSettings(propertiesRequest);
                returnSettings = propertiesResponse.Settings;
            }
            catch (Exception ex)
            {
                returnException = ex;
            }

            return new WorkerResultCreateFirstContext(returnException, contextId, returnSettings);
        }

        private static StackHashCollectionPolicyCollection SetDefaultCollectionPolicy(int contextId)
        {
            StackHashCollectionPolicy cabPolicy = new StackHashCollectionPolicy();
            cabPolicy.CollectionOrder = StackHashCollectionOrder.LargestFirst;
            cabPolicy.CollectionType = StackHashCollectionType.Count;
            cabPolicy.CollectLarger = false;
            cabPolicy.ConditionObject = StackHashCollectionObject.Cab;
            cabPolicy.Maximum = 5;
            cabPolicy.Percentage = 100;
            cabPolicy.ObjectToCollect = StackHashCollectionObject.Cab;
            cabPolicy.RootId = 0;
            cabPolicy.RootObject = StackHashCollectionObject.Global;

            StackHashCollectionPolicy eventPolicy = new StackHashCollectionPolicy();
            eventPolicy.CollectionType = StackHashCollectionType.MinimumHitCount;
            eventPolicy.ConditionObject = StackHashCollectionObject.Event;
            eventPolicy.Minimum = 0;
            eventPolicy.ObjectToCollect = StackHashCollectionObject.Cab;
            eventPolicy.RootId = 0;
            eventPolicy.RootObject = StackHashCollectionObject.Global;

            StackHashCollectionPolicyCollection policies = new StackHashCollectionPolicyCollection();
            policies.Add(cabPolicy);
            policies.Add(eventPolicy);

            SetDataCollectionPolicyRequest request = new SetDataCollectionPolicyRequest();
            request.ClientData = UserSettings.Settings.GenerateClientData();
            request.ContextId = contextId;
            request.PolicyCollection = policies;

            ServiceProxy.Services.Admin.SetDataCollectionPolicy(request);

            return policies;
        }

        private static void UpdatePropertiesRequestToUNC(ref SetStackHashPropertiesRequest request)
        {
            foreach (StackHashContextSettings settings in request.Settings.ContextCollection)
            {
                if (settings.ErrorIndexSettings.Folder.IndexOf("\\\\", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    settings.ErrorIndexSettings.Folder = PathUtils.GetPhysicalPath(settings.ErrorIndexSettings.Folder);
                }
            }
        }

        private void HandleResultCreateFirstContext(WorkerResultCreateFirstContext result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                if (result.ContextSettings != null)
                {
                    UpdateContextListCore(result.ContextSettings.ContextCollection);

                    if (result.ContextId != UserSettings.InvalidContextId)
                    {
                        UpdateActivePlugins(result.ContextSettings, result.ContextId);
                    }

                    if (result.ContextSettings.ProxySettings != null)
                    {
                        this.ServiceProxySettings = result.ContextSettings.ProxySettings;
                    }
                    else
                    {
                        this.ServiceProxySettings = null;
                    }

                    this.ClientTimeoutInSeconds = result.ContextSettings.ClientTimeoutInSeconds;
                }
            }
            
            if (result.ContextId != UserSettings.InvalidContextId)
            {
                _contextId = result.ContextId;
                UpdateCurrentContext();
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.FirstContextCreated, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        // RemoveScriptResult

        private WorkerResultRemoveScriptResult WorkerRemoveScriptResult(WorkerArgRemoveScriptResult arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                RemoveScriptResultRequest request = new RemoveScriptResultRequest();
                request.Cab = arg.Cab;
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = GetFile(arg.Event.FileId);
                request.Product = arg.Product;
                request.ScriptName = arg.ScriptName;

                ServiceProxy.Services.Admin.RemoveScriptResult(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultRemoveScriptResult(returnException);
        }

        private void HandleResultRemoveScriptResult(WorkerResultRemoveScriptResult result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // refresh the list of available result files
                AdminGetResultFiles();
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
        }

        // CopyIndex

        private WorkerResultCopyIndex WorkerCopyIndex(WorkerArgCopyIndex arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                // if context is active need to deactivate before the copy
                if (arg.IsActive)
                {
                    DeactivateStackHashContextRequest deactivateRequest = new DeactivateStackHashContextRequest();
                    deactivateRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    deactivateRequest.ContextId = arg.ContextId;

                    ServiceProxy.Services.Admin.DeactivateStackHashContext(deactivateRequest);

                    _activateContextAfterCopy = true;
                    _activateContextAfterCopyContextId = arg.ContextId;
                }

                // copy the index
                CopyIndexRequest request = new CopyIndexRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.SwitchIndexWhenCopyComplete = true;
                request.DeleteSourceIndexWhenCopyComplete = true;

                request.DestinationErrorIndexSettings = new ErrorIndexSettings();
                request.DestinationErrorIndexSettings.Folder = arg.NewPath;
                request.DestinationErrorIndexSettings.Name = arg.NewName;
                request.DestinationErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
                request.DestinationErrorIndexSettings.Location = arg.IsDatabaseInCabFolder ? StackHashErrorIndexLocation.InCabFolder : StackHashErrorIndexLocation.OnSqlServer;

                request.SqlSettings = new StackHashSqlConfiguration();
                request.SqlSettings.ConnectionString = arg.NewConnectionString;
                request.SqlSettings.InitialCatalog = arg.NewInitialCatalog;
                request.SqlSettings.MinPoolSize = DefaultSqlMinPoolSize;
                request.SqlSettings.MaxPoolSize = DefaultSqlMinPoolSize;
                request.SqlSettings.ConnectionTimeout = DefaultSqlConnectionTimeout;
                request.SqlSettings.EventsPerBlock = DefaultSqlEventsPerBlock;

                ServiceProxy.Services.Admin.CopyIndex(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultCopyIndex(returnException);
        }

        private void HandleResultCopyIndex(WorkerResultCopyIndex result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
                UpdateServiceProgress(0, false);
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                // if no error then async copy is in progress
                UpdateServiceProgress(0, true);
            }
        }

        // MoveIndex

        private WorkerResultMoveIndex WorkerMoveIndex(WorkerArgMoveIndex arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                // if context is active need to deactivate before the move
                if (arg.IsActive)
                {
                    DeactivateStackHashContextRequest deactivateRequest = new DeactivateStackHashContextRequest();
                    deactivateRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    deactivateRequest.ContextId = arg.ContextId;

                    ServiceProxy.Services.Admin.DeactivateStackHashContext(deactivateRequest);

                    _activateContextAfterMove = true;
                    _activateContextAfterMoveContextId = arg.ContextId;
                }

                // move the index...
                MoveIndexRequest request = new MoveIndexRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.NewErrorIndexName = arg.NewName;
                request.NewErrorIndexPath = arg.NewPath;
                
                request.NewSqlSettings = new StackHashSqlConfiguration();
                request.NewSqlSettings.ConnectionString = arg.NewConnectionString;
                request.NewSqlSettings.ConnectionTimeout = arg.ConnectionTimeout;
                request.NewSqlSettings.EventsPerBlock = arg.EventsPerBlock;
                request.NewSqlSettings.InitialCatalog = arg.NewInitialCatalog;
                request.NewSqlSettings.MaxPoolSize = arg.MaxPoolSize;
                request.NewSqlSettings.MinPoolSize = arg.MinPoolSize;

                ServiceProxy.Services.Admin.MoveIndex(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultMoveIndex(returnException);
        }

        private void HandleResultMoveIndex(WorkerResultMoveIndex result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }           

            // if no error then async move is in progress
        }

        // ExtractCabFile

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private WorkerResultGetCabFile WorkerGetCabFile(WorkerArgGetCabFile arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            string fileContents = null;

            try
            {
                // let ServiceProxy know the size of the download
                ServiceProxy.Services.NotifyCabDownloadLength(arg.FileLength);

                GetCabFileRequest request = new GetCabFileRequest();
                request.Cab = arg.Cab;
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = GetFile(arg.Event.FileId);
                request.Product = arg.Product;
                request.FileName = arg.FileName;

                using (StreamReader cabStreamReader = new StreamReader(ServiceProxy.Services.Cabs.GetCabFile(request)))
                {
                    fileContents = cabStreamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                returnException = ex;
            }

            return new WorkerResultGetCabFile(returnException, fileContents);
        }

        private void HandleResultGetCabFile(WorkerResultGetCabFile result)
        {
            Debug.Assert(result != null);

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            this.CurrentCabFileContents = result.FileContents;

            if (result.Ex == null)
            {
                RequestUi(ClientLogicUIRequest.CabFileReady);
            }
            else
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
        }

        // ExtractCabContents

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private WorkerResultGetCabContents WorkerGetCabContents(WorkerArgGetCabContents arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            string extractedCabFolder = null;
            string tempCabFile = null;
            
            try
            {
                // if the user specified the folder we don't schedule the extracted contents for cleanup
                bool userFolder = arg.ExtractFolder != null;

                // create a temp file path for the cab
                tempCabFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                    string.Format(CultureInfo.InvariantCulture,
                    "StackHash_{0}_{1}.cab",
                    arg.Event.Id,
                    arg.Cab.Id));

                // add the file to the cleanup list
                Cleanup.List.AddPath(tempCabFile);

                // let ServiceProxy know the size of the download
                ServiceProxy.Services.NotifyCabDownloadLength(arg.Cab.SizeInBytes);

                // download cab from server
                GetCabFileRequest request = new GetCabFileRequest();
                request.Cab = arg.Cab;
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = GetFile(arg.Event.FileId);
                request.Product = arg.Product;
                request.FileName = null;

                using (System.IO.Stream cabStream = ServiceProxy.Services.Cabs.GetCabFile(request))
                {
                    using (System.IO.FileStream localStream = System.IO.File.Create(tempCabFile))
                    {
                        byte[] buffer = new byte[CabCopyBufferSize];

                        int bytesRead = 0;
                        while ((bytesRead = cabStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            localStream.Write(buffer, 0, bytesRead);
                        }

                        buffer = null;
                    }
                }

                if (userFolder)
                {
                    // append folders for the event and cab ids
                    extractedCabFolder = System.IO.Path.Combine(arg.ExtractFolder,
                        string.Format(CultureInfo.InvariantCulture,
                        @"Event {0}\Cab {1}",
                        arg.Event.Id,
                        arg.Cab.Id));

                    if (!System.IO.Directory.Exists(extractedCabFolder))
                    {
                        System.IO.Directory.CreateDirectory(extractedCabFolder);
                    }
                }
                else
                {
                    // create a temp folder for the cab contents
                    extractedCabFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                        string.Format(CultureInfo.InvariantCulture,
                        "StackHash_{0}_{1}",
                        arg.Event.Id,
                        arg.Cab.Id));

                    // add the folder to the cleanup list
                    Cleanup.List.AddPath(extractedCabFolder);

                    if (System.IO.Directory.Exists(extractedCabFolder))
                    {
                        // remove the folder if it already exists
                        System.IO.Directory.Delete(extractedCabFolder, true);
                    }
                    System.IO.Directory.CreateDirectory(extractedCabFolder);
                }

                // extract cab to temp folder
                Cabs.ExtractCab(tempCabFile, extractedCabFolder);
            }
            catch (Exception ex)
            {
                returnException = ex;
            }
            finally
            {
                if (tempCabFile != null)
                {
                    // if we created a temp cab file try to delete it
                    if (System.IO.File.Exists(tempCabFile))
                    {
                        try
                        {
                            System.IO.File.Delete(tempCabFile);
                        }
                        catch { }
                    }
                }
            }

            return new WorkerResultGetCabContents(returnException, extractedCabFolder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private void HandleResultGetCabContents(WorkerResultGetCabContents result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // set the extracted cab folder
                this.CurrentCabFolder = result.ExtractedCabFolder;

                // let the UI know that the requested can has been extracted
                RequestUi(ClientLogicUIRequest.CabFolderReady);
            }
            else
            {
                // see Case 609
                string lowerMessage = result.Ex.Message.ToLowerInvariant();
                if (lowerMessage.Contains("corrupt") || lowerMessage.Contains("the file is not a cabinet"))
                {
                    // report cab extract error
                    ReportError(result.Ex, ClientLogicErrorCode.LocalCabExtractFailed);
                }
                else
                {
                    // report error
                    ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
                }
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // DisableLogging

        private static WorkerResultResultOnlyNoAction WorkerDisableLogging()
        {
            Exception returnException = null;

            try
            {
                DisableLoggingRequest request = new DisableLoggingRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.DisableLogging(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultResultOnlyNoAction(returnException);
        }

        // EnableLogging

        private static WorkerResultResultOnlyNoAction WorkerEnableLogging()
        {
            Exception returnException = null;

            try
            {
                EnableLoggingRequest request = new EnableLoggingRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.EnableLogging(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultResultOnlyNoAction(returnException);
        }

        // CreateNewStackHashContext

        private static void SetNewContextSchedulesAndDebuggerDefaults(ref StackHashContextSettings context)
        {
            // set random sync schedule and default purge schedule
            Random rng = new Random();

            Schedule sync = new Schedule();
            sync.DaysOfWeek = DaysOfWeek.All;
            sync.Period = SchedulePeriod.Daily;
            sync.Time = new ScheduleTime();
            sync.Time.Hour = rng.Next(19, 23); // between 7 and 11pm
            sync.Time.Minute = rng.Next(0, 59);
            sync.Time.Second = 0;

            Schedule purge = new Schedule();
            purge.DaysOfWeek = DaysOfWeek.Sunday;
            purge.Period = SchedulePeriod.Daily;
            purge.Time = new ScheduleTime();
            purge.Time.Hour = 23;
            purge.Time.Minute = 0;
            purge.Time.Second = 0;

            context.WinQualSyncSchedule = new ScheduleCollection();
            context.WinQualSyncSchedule.Add(sync);

            context.CabFilePurgeSchedule = new ScheduleCollection();
            context.CabFilePurgeSchedule.Add(purge);

            // set image and symbol search paths from environment variables if present
            bool is64 = SystemInformation.Is64BitSystem();

            bool imageSearchPathSet = false;
            string imageSearchPath = Environment.GetEnvironmentVariable("_NT_EXECUTABLE_IMAGE_PATH");
            if (!string.IsNullOrEmpty(imageSearchPath))
            {
                context.DebuggerSettings.BinaryPath = ClientUtils.StringToSearchPath(imageSearchPath);
                if (is64) { context.DebuggerSettings.BinaryPath64Bit = ClientUtils.StringToSearchPath(imageSearchPath); }
                imageSearchPathSet = true;
            }

            string symbolSearchPath = Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH");
            if (!string.IsNullOrEmpty(symbolSearchPath))
            {
                context.DebuggerSettings.SymbolPath = ClientUtils.StringToSearchPath(symbolSearchPath);
                if (is64) { context.DebuggerSettings.SymbolPath64Bit = ClientUtils.StringToSearchPath(symbolSearchPath); }

                // if _NT_EXECUTABLE_IMAGE_PATH not found then default the image search path to the symbol search path
                if (!imageSearchPathSet)
                {
                    context.DebuggerSettings.BinaryPath = ClientUtils.StringToSearchPath(symbolSearchPath);
                    if (is64) { context.DebuggerSettings.BinaryPath64Bit = ClientUtils.StringToSearchPath(symbolSearchPath); }
                }
            }

            // set cdb.exe if missing
            if ((string.IsNullOrEmpty(context.DebuggerSettings.DebuggerPathAndFileName)) ||
                (!File.Exists(context.DebuggerSettings.DebuggerPathAndFileName)))
            {
                bool debuggerSet = false;

                string debugger32 = UserSettings.Settings.DebuggerPathX86;
                if (!string.IsNullOrEmpty(debugger32))
                {
                    debugger32 = Path.Combine(Path.GetDirectoryName(debugger32), "cdb.exe");
                    if (File.Exists(debugger32))
                    {
                        context.DebuggerSettings.DebuggerPathAndFileName = debugger32;
                        debuggerSet = true;
                    }
                }

                if (!debuggerSet)
                {
                    debugger32 = ClientUtils.GetCdbPath(ImageFileMachine.I386);
                    if (!string.IsNullOrEmpty(debugger32))
                    {
                        context.DebuggerSettings.DebuggerPathAndFileName = debugger32;
                    }
                }
            }

            if (is64)
            {
                if ((string.IsNullOrEmpty(context.DebuggerSettings.DebuggerPathAndFileName64Bit)) ||
                    (!File.Exists(context.DebuggerSettings.DebuggerPathAndFileName64Bit)))
                {
                    bool debuggerSet = false;

                    string debugger64 = UserSettings.Settings.DebuggerPathAmd64;
                    if (!string.IsNullOrEmpty(debugger64))
                    {
                        debugger64 = Path.Combine(Path.GetDirectoryName(debugger64), "cdb.exe");
                        if (File.Exists(debugger64))
                        {
                            context.DebuggerSettings.DebuggerPathAndFileName64Bit = debugger64;
                            debuggerSet = true;
                        }
                    }

                    if (!debuggerSet)
                    {
                        debugger64 = ClientUtils.GetCdbPath(ImageFileMachine.AMD64);
                        if (!string.IsNullOrEmpty(debugger64))
                        {
                            context.DebuggerSettings.DebuggerPathAndFileName64Bit = debugger64;
                        }
                    }
                }
            }
        }

        private static WorkerResultCreateNewStackHashContext WorkerCreateNewStackHashContext()
        {
            Exception returnException = null;
            StackHashContextSettings context = null;

            try
            {
                CreateNewStackHashContextRequest request = new CreateNewStackHashContextRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();
                request.IndexType = ErrorIndexType.SqlExpress;
                
                CreateNewStackHashContextResponse response = ServiceProxy.Services.Admin.CreateNewStackHashContext(request);

                context = response.Settings;

                // set default collection policies
                context.CollectionPolicy = SetDefaultCollectionPolicy(context.Id);

                // remove defaults that don't make sense
                context.ErrorIndexSettings.Folder = string.Empty;
                context.ErrorIndexSettings.Name = string.Empty;
                context.SqlSettings.InitialCatalog = string.Empty;
                context.SqlSettings.ConnectionString = string.Empty;
                context.WinQualSettings.CompanyName = string.Empty;
                context.WinQualSettings.UserName = string.Empty;
                context.WinQualSettings.Password = string.Empty;
                context.WinQualSettings.EnableNewProductsAutomatically = true;
                context.DebuggerSettings.DebuggerPathAndFileName = string.Empty;
                context.DebuggerSettings.DebuggerPathAndFileName64Bit = string.Empty;

                // set schedules
                SetNewContextSchedulesAndDebuggerDefaults(ref context);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultCreateNewStackHashContext(returnException, context);
        }

        private void HandleResultCreateNewStackHashContext(WorkerResultCreateNewStackHashContext result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)  
            {
                // set the new context
                _newContext = result.Context;

                // let the UI know that the requested script results are ready
                RequestUi(ClientLogicUIRequest.NewContextSettingsReady);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // RemoveStackHashContext

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static WorkerResultResultOnlyRefreshContextSettings WorkerRemoveStackHashContext(WorkerArgRemoveStackHashContext arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashStatus status = null;

            try
            {
                RemoveStackHashContextRequest request = new RemoveStackHashContextRequest();
                request.ContextId = arg.ContextId;
                request.ResetNextContextIdIfAppropriate = false;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.RemoveStackHashContext(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            // always try to get updated settings
            try
            {
                GetStackHashServiceStatusRequest request = new GetStackHashServiceStatusRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse response = ServiceProxy.Services.Admin.GetServiceStatus(request);
                status = response.Status;
            }
            catch { }

            return new WorkerResultResultOnlyRefreshContextSettings(returnException, status);
        }

        // DeactivateStackHashContext

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static WorkerResultResultOnlyRefreshContextSettings WorkerDeactivateStackHashContext(WorkerArgDeactivateStackHashContext arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashStatus status = null;

            try
            {
                DeactivateStackHashContextRequest request = new DeactivateStackHashContextRequest();
                request.ContextId = arg.ContextId;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.DeactivateStackHashContext(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            // always try to get updated settings
            try
            {
                GetStackHashServiceStatusRequest request = new GetStackHashServiceStatusRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse response = ServiceProxy.Services.Admin.GetServiceStatus(request);
                status = response.Status;
            }
            catch { }

            return new WorkerResultResultOnlyRefreshContextSettings(returnException, status);
        }

        // ActivateStackHashContext

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static WorkerResultResultOnlyRefreshContextSettings WorkerActivateStackHashContext(WorkerArgActivateStackHashContext arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashStatus status = null;

            try
            {
                ActivateStackHashContextRequest request = new ActivateStackHashContextRequest();
                request.ContextId = arg.ContextId;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.ActivateStackHashContext(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            // always try to get updated settings
            try
            {
                GetStackHashServiceStatusRequest request = new GetStackHashServiceStatusRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse response = ServiceProxy.Services.Admin.GetServiceStatus(request);
                status = response.Status;
            }
            catch { }

            return new WorkerResultResultOnlyRefreshContextSettings(returnException, status);
        }

        private void HandleResultResultOnlyRefreshContextSettings(WorkerResultResultOnlyRefreshContextSettings result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // refresh context settings on success
                RefreshContextSettings();

                // don't clear not busy
            }
            else
            {
                // if the call failed we might still have an updated status
                if (result.ServiceStatus != null)
                {
                    this.ServiceStatus = result.ServiceStatus;
                }

                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);

                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
        }

        private void HandleResultResultOnlyNoAction(WorkerResultResultOnlyNoAction result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // AddGetCabNotes

        private WorkerResultAddGetCabNotes WorkerAddGetCabNotes(WorkerArgAddGetCabNotes arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashNotes notes = null;

            try
            {
                // get the file associated with this event
                StackHashFile file = GetFile(arg.Event.FileId);
                Debug.Assert(file != null);

                // first add a note if we have one
                if ((!string.IsNullOrEmpty(arg.Note)) &&
                    (!string.IsNullOrEmpty(arg.User)))
                {
                    StackHashNoteEntry noteEntry = new StackHashNoteEntry();
                    noteEntry.Note = arg.Note;
                    noteEntry.Source = "StackHash Client";
                    noteEntry.User = arg.User;
                    noteEntry.NoteId = arg.NoteId;

                    AddCabNoteRequest addRequest = new AddCabNoteRequest();
                    addRequest.Cab = arg.Cab;
                    addRequest.ContextId = arg.ContextId;
                    addRequest.Event = arg.Event;
                    addRequest.File = file;
                    addRequest.NoteEntry = noteEntry;
                    addRequest.Product = arg.Product;
                    addRequest.ClientData = UserSettings.Settings.GenerateClientData();

                    ServiceProxy.Services.Projects.AddCabNote(addRequest);
                }

                // get all notes
                GetCabNotesRequest getRequest = new GetCabNotesRequest();
                getRequest.Cab = arg.Cab;
                getRequest.ContextId = arg.ContextId;
                getRequest.Event = arg.Event;
                getRequest.File = file;
                getRequest.Product = arg.Product;
                getRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetCabNotesResponse getResponse = ServiceProxy.Services.Projects.GetCabNotes(getRequest);

                notes = getResponse.Notes;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultAddGetCabNotes(returnException, notes);
        }

        private void HandleResultAddGetCabNotes(WorkerResultAddGetCabNotes result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                if (result.Notes != null)
                {
                    this.CurrentCabNotes = new ObservableCollection<StackHashNoteEntry>(result.Notes);
                }
                else
                {
                    this.CurrentCabNotes = null;
                }

                // let the UI know that the requested script results are ready
                RequestUi(ClientLogicUIRequest.CabNotesReady);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // AddGetEventNotes

        private WorkerResultAddGetEventNotes WorkerAddGetEventNotes(WorkerArgAddGetEventNotes arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashNotes notes = null;

            try
            {
                // get the file associated with this event
                StackHashFile file = GetFile(arg.Event.FileId);
                Debug.Assert(file != null);

                // first add a note if we have one
                if ((!string.IsNullOrEmpty(arg.Note)) &&
                    (!string.IsNullOrEmpty(arg.User)))
                {
                    StackHashNoteEntry noteEntry = new StackHashNoteEntry();
                    noteEntry.Note = arg.Note;
                    noteEntry.Source = "StackHash Client";
                    noteEntry.User = arg.User;
                    noteEntry.NoteId = arg.NoteId;

                    AddEventNoteRequest addRequest = new AddEventNoteRequest();
                    addRequest.ContextId = arg.ContextId;
                    addRequest.Event = arg.Event;
                    addRequest.File = file;
                    addRequest.NoteEntry = noteEntry;
                    addRequest.Product = arg.Product;
                    addRequest.ClientData = UserSettings.Settings.GenerateClientData();

                    ServiceProxy.Services.Projects.AddEventNote(addRequest);
                }

                // get all notes
                GetEventNotesRequest getRequest = new GetEventNotesRequest();
                getRequest.ContextId = arg.ContextId;
                getRequest.Event = arg.Event;
                getRequest.File = file;
                getRequest.Product = arg.Product;
                getRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetEventNotesResponse getResponse = ServiceProxy.Services.Projects.GetEventNotes(getRequest);

                notes = getResponse.Notes;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultAddGetEventNotes(returnException, notes);
        }

        private void HandleResultAddGetEventNotes(WorkerResultAddGetEventNotes result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                if (result.Notes != null)
                {
                    this.CurrentEventNotes = new ObservableCollection<StackHashNoteEntry>(result.Notes);
                }
                else
                {
                    this.CurrentEventNotes = null;
                }

                // let the UI know that the requested script results are ready
                RequestUi(ClientLogicUIRequest.EventNotesReady);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // GetResult

        private WorkerResultGetResult WorkerGetResult(WorkerArgGetResult arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashScriptResult scriptResult = null;
            StackHashEventPackage eventPackage = null;
            StackHashScriptResultFiles resultFiles = null;

            try
            {
                StackHashFile file = GetFile(arg.Event.FileId);
                Debug.Assert(file != null);

                GetDebugResultRequest request = new GetDebugResultRequest();
                request.Cab = arg.Cab;
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = file;
                request.Product = arg.Product;
                request.ScriptName = arg.ScriptName;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetDebugResultResponse response = ServiceProxy.Services.Admin.GetDebugResult(request);
                scriptResult = response.Result;

                // also refresh the event package to get updated cab info
                GetEventPackageRequest eventPackageRequest = new GetEventPackageRequest();
                eventPackageRequest.ClientData = UserSettings.Settings.GenerateClientData();
                eventPackageRequest.ContextId = arg.ContextId;
                eventPackageRequest.Event = arg.Event;
                eventPackageRequest.File = file;
                eventPackageRequest.Product = arg.Product;

                GetEventPackageResponse eventPackageResponse = ServiceProxy.Services.Projects.GetEventPackage(eventPackageRequest);
                eventPackage = eventPackageResponse.EventPackage;

                // and get the updated list of result files
                GetDebugResultFilesRequest resultFilesRequest = new GetDebugResultFilesRequest();
                resultFilesRequest.Cab = arg.Cab;
                resultFilesRequest.ClientData = UserSettings.Settings.GenerateClientData();
                resultFilesRequest.ContextId = arg.ContextId;
                resultFilesRequest.Event = arg.Event;
                resultFilesRequest.File = file;
                resultFilesRequest.Product = arg.Product;

                GetDebugResultFilesResponse resultFilesResponse = ServiceProxy.Services.Admin.GetDebugResultFiles(resultFilesRequest);
                resultFiles = resultFilesResponse.ResultFiles;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetResult(returnException, scriptResult, eventPackage, resultFiles);
        }

        private void UpdateEventPackageCore(StackHashEventPackage eventPackage)
        {
            if (_guiDispatcher.CheckAccess())
            {
                if (eventPackage == null) { return; }
                if (this.EventPackages == null) { return; }

                this.EventPackages[_eventIdToEventsIndex[new Tuple<int, string>(eventPackage.EventData.Id, eventPackage.EventData.EventTypeName)]].UpdateEventPackage(eventPackage, _workFlowIdToMapping);
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashEventPackage>(UpdateEventPackageCore), eventPackage);
            }
        }

        private void GetResultResult(WorkerResultGetResult result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                this.CurrentScriptResult = result.ScriptResult;

                // update the event package
                UpdateEventPackageCore(result.EventPackage);

                // update result files
                UpdateScriptResultsCore(result.ResultFiles);

                // let the UI know that the requested script results are ready
                RequestUi(ClientLogicUIRequest.ScriptResultsReady);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // GetResultFiles

        private WorkerResultGetResultFiles WorkerGetResultFiles(WorkerArgGetResultFiles arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashScriptResultFiles resultFiles = null;

            try
            {
                GetDebugResultFilesRequest request = new GetDebugResultFilesRequest();
                request.Cab = arg.Cab;
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = GetFile(arg.Event.FileId);
                request.Product = arg.Product;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetDebugResultFilesResponse response = ServiceProxy.Services.Admin.GetDebugResultFiles(request);
                resultFiles = response.ResultFiles;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetResultFiles(returnException, resultFiles);
        }

        private void GetResultFilesResult(WorkerResultGetResultFiles result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                this.UpdateScriptResultsCore(result.ResultFiles);

                // get cab notes immediately after listing result files
                GetCabNotes();

                // don't return to NotBusy
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
                
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
        }

        // TestScript

        private WorkerResultTestScipt WorkerTestScript(WorkerArgTestScript arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                RunDebuggerScriptAsyncRequest request = new RunDebuggerScriptAsyncRequest();

                request.Cab = arg.Cab;
                request.ContextId = arg.ContextId;
                request.Event = arg.Event;
                request.File = GetFile(arg.Event.FileId);
                request.Product = arg.Product;
                request.ScriptsToRun = new StackHashScriptNamesCollection();
                request.ScriptsToRun.Add(arg.ScriptName);
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.RunScriptAsync(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultTestScipt(returnException);
        }

        private void UpdateFileListCore(List<StackHashFile> files)
        {
            if (_fileIdToFile == null)
            {
                _fileIdToFile = new Dictionary<int, StackHashFile>();
            }

            if ((files != null) && (files.Count > 0))
            {
                foreach (StackHashFile file in files)
                {
                    if (!_fileIdToFile.ContainsKey(file.Id))
                    {
                        _fileIdToFile.Add(file.Id, file);
                    }
                }
            }
        }

        private StackHashFile GetFile(int fileId)
        {
            StackHashFile file = null;

            if ((_fileIdToFile != null) && (_fileIdToFile.ContainsKey(fileId)))
            {
                file = _fileIdToFile[fileId];
            }

            return file;
        }

        private void TestScriptResult(WorkerResultTestScipt result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
                
                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }

            // do nothing on success, we'll get adminreports when the script run starts and completes
        }

        // AddScript

        private static WorkerResultAddScript WorkerAddScript(WorkerArgAddScript arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                // check for rename
                if (string.Compare(arg.ScriptSettings.Name, arg.OriginalName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    RenameDebuggerScriptRequest renameRequest = new RenameDebuggerScriptRequest();
                    renameRequest.OriginalScriptName = arg.OriginalName;
                    renameRequest.NewScriptName = arg.ScriptSettings.Name;
                    renameRequest.ClientData = UserSettings.Settings.GenerateClientData();

                    ServiceProxy.Services.Admin.RenameScript(renameRequest);
                }

                AddDebuggerScriptRequest request = new AddDebuggerScriptRequest();
                request.Script = arg.ScriptSettings;
                request.Overwrite = arg.Overwrite;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.AddScript(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultAddScript(returnException);
        }

        private void ReloadScriptNamesResult(WorkerResult result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);

                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
            else
            {
                // get script names again
                AdminGetScriptNames();
            }
        }

        // RenameScript

        private static WorkerResultRenameScript WorkerRenameScript(WorkerArgRenameScript arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                RenameDebuggerScriptRequest request = new RenameDebuggerScriptRequest();
                request.OriginalScriptName = arg.OrignalName;
                request.NewScriptName = arg.NewName;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.RenameScript(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultRenameScript(returnException);
        }

        // RemoveScript

        private static WorkerResultRemoveScript WorkerRemoveScript(WorkerArgRemoveScript arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;

            try
            {
                RemoveDebuggerScriptRequest request = new RemoveDebuggerScriptRequest();
                request.ScriptName = arg.ScriptName;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                ServiceProxy.Services.Admin.RemoveScript(request);
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultRemoveScript(returnException);
        }

        // GetScript

        private static WorkerResultGetScript WorkerGetScript(WorkerArgGetScript arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashScriptSettings scriptSettings = null;

            try
            {
                GetDebuggerScriptRequest request = new GetDebuggerScriptRequest();
                request.ScriptName = arg.ScriptName;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetDebuggerScriptResponse response = ServiceProxy.Services.Admin.GetScript(request);
                scriptSettings = response.ScriptSettings;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetScript(returnException, scriptSettings);
        }

        private void GetScriptResult(WorkerResultGetScript result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                this.CurrentScript = result.ScriptSettings;

                // let the UI know that the requested script is ready
                RequestUi(ClientLogicUIRequest.ScriptReady);
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // GetDebuggerScriptNames

        private static WorkerResultGetDebuggerScriptNames WorkerGetDebuggerScriptNames()
        {
            Exception returnException = null;
            StackHashScriptFileDataCollection scriptData = null;

            try
            {
                GetDebuggerScriptNamesRequest request = new GetDebuggerScriptNamesRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetDebuggerScriptNamesResponse response = ServiceProxy.Services.Admin.GetDebuggerScriptNames(request);
                scriptData = response.ScriptFileData;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetDebuggerScriptNames(returnException, scriptData);
        }

        private void GetDebuggerScriptNamesResult(WorkerResultGetDebuggerScriptNames result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // update event packages list
                if (result.ScriptData == null)
                {
                    this.ScriptData = null;
                }
                else
                {
                    this.ScriptData = new ObservableCollection<StackHashScriptFileData>(result.ScriptData);

                    // sort by name
                    SortCollection(this.ScriptData, "Name", ListSortDirection.Ascending);

                    // let the UI know to show script settings
                    RequestUi(ClientLogicUIRequest.ScriptSettings);
                }
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // StartSync

        private static WorkerResultStartSync WorkerStartSync(WorkerArgStartSync arg)
        {
            Debug.Assert(arg != null);

            Exception returnExcpetion = null;

            try
            {
                StartSynchronizationRequest request = new StartSynchronizationRequest();
                request.ContextId = arg.ContextId;
                request.ForceResynchronize = arg.ForceResync;
                request.JustSyncProducts = arg.ProductsOnly;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                if ((arg.ProductsToSync != null) && (arg.ProductsToSync.Count > 0))
                {
                    StackHashProductSyncDataCollection syncDataCollection = new StackHashProductSyncDataCollection();
                    foreach (int productToSync in arg.ProductsToSync)
                    {
                        StackHashProductSyncData syncData = new StackHashProductSyncData();
                        syncData.ProductId = productToSync;
                        syncDataCollection.Add(syncData);
                    }

                    request.ProductsToSynchronize = syncDataCollection;
                }
                else
                {
                    request.ProductsToSynchronize = null;
                }

                ServiceProxy.Services.Admin.StartSynchronization(request);
            }
            catch (CommunicationException ex)
            {
                returnExcpetion = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnExcpetion = timeoutException;
            }

            return new WorkerResultStartSync(returnExcpetion);
        }

        private void StartSyncResult(WorkerResultStartSync result)
        {
            Debug.Assert(result != null);

            if (result.Ex != null)
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }
            else
            {
                // flag that the sync has started
                this.SyncRunning = true;
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // GetWindowedEventPackages 

        private static WorkerResultGetWindowedEventPackages WorkerGetWindowedEventPackages(WorkerArgGetWindowedEventPackages arg)
        {
            Exception returnException = null;
            StackHashEventPackageCollection eventPackageCollection = null;
            List<StackHashFile> files = new List<StackHashFile>();
            long totalRows = 1;
            long minRow = 1;
            long maxRow = 1;

            try
            {
                if (arg.IsScriptSearch)
                {
                    // logic if script results are being searched

                    // calculate what we need to ask for
                    GetWindowedEventPackageRequest request = new GetWindowedEventPackageRequest();
                    request.ClientData = UserSettings.Settings.GenerateClientData();
                    request.ContextId = arg.ContextId;
                    request.NumberOfRows = arg.EventsPerPage; 
                    request.SearchCriteriaCollection = arg.Search;
                    request.SortOrder = arg.Sort;

                    switch (arg.PageIntention)
                    {
                        case PageIntention.First:
                            request.StartRow = 1;
                            request.Direction = StackHashSearchDirection.Forwards;
                            request.CountAllMatches = true;
                            break;

                        case PageIntention.Last:
                            request.StartRow = Int64.MaxValue;
                            request.Direction = StackHashSearchDirection.Backwards;
                            break;

                        case PageIntention.Next:
                            request.StartRow = arg.LastMaxRow + 1;
                            request.Direction = StackHashSearchDirection.Forwards;
                            break;

                        case PageIntention.Previous:
                            request.StartRow = arg.LastMinRow - arg.EventsPerPage;
                            if (request.StartRow < 1) { request.StartRow = 1; }
                            request.Direction = StackHashSearchDirection.Backwards;
                            break;

                        case PageIntention.Reload:
                            request.StartRow = arg.LastMinRow;
                            request.Direction = StackHashSearchDirection.Forwards;
                            break;
                    }                    

                    GetWindowedEventPackageResponse response = ServiceProxy.Services.Projects.GetWindowedEventPackages(request);
                    eventPackageCollection = response.EventPackages;
                    minRow = response.MinimumRowNumber;
                    maxRow = response.MaximumRowNumber;

                    // for script search total rows is only valid on the first page
                    if (arg.PageIntention == PageIntention.First)
                    {
                        totalRows = response.TotalRows;
                    }
                    else
                    {
                        totalRows = arg.LastTotalRows;
                    }
                }
                else
                {
                    // logic if script results are not included
                    int startRow = ((arg.Page - 1) * arg.EventsPerPage) + 1;

                    GetWindowedEventPackageRequest request = new GetWindowedEventPackageRequest();
                    request.ClientData = UserSettings.Settings.GenerateClientData();
                    request.ContextId = arg.ContextId;
                    request.NumberOfRows = arg.EventsPerPage;
                    request.SearchCriteriaCollection = arg.Search;
                    request.SortOrder = arg.Sort;
                    request.StartRow = startRow;

                    GetWindowedEventPackageResponse response = ServiceProxy.Services.Projects.GetWindowedEventPackages(request);
                    eventPackageCollection = response.EventPackages;
                    totalRows = response.TotalRows;
                }

                if (arg.UpdateFileCache)
                {
                    // if search is for a single product just get the file list, if for multiple
                    // products figure out which ones and get all related files
                    if (arg.Product == null)
                    {
                        // possibly multiple products - get all files for all products
                        GetProductsRequest productsRequest = new GetProductsRequest();
                        productsRequest.ClientData = UserSettings.Settings.GenerateClientData();
                        productsRequest.ContextId = arg.ContextId;

                        GetProductsResponse productsResponse = ServiceProxy.Services.Projects.GetProducts(productsRequest);
                        foreach (StackHashProductInfo productInfo in productsResponse.Products)
                        {
                            GetFilesRequest filesRequest = new GetFilesRequest();
                            filesRequest.ClientData = UserSettings.Settings.GenerateClientData();
                            filesRequest.ContextId = arg.ContextId;
                            filesRequest.Product = productInfo.Product;

                            GetFilesResponse filesResponse = ServiceProxy.Services.Projects.GetFiles(filesRequest);
                            files.AddRange(filesResponse.Files);
                        }
                    }
                    else
                    {
                        // only one product - get files for just this product
                        GetFilesRequest filesRequest = new GetFilesRequest();
                        filesRequest.ClientData = UserSettings.Settings.GenerateClientData();
                        filesRequest.ContextId = arg.ContextId;
                        filesRequest.Product = arg.Product;

                        GetFilesResponse filesResponse = ServiceProxy.Services.Projects.GetFiles(filesRequest);
                        files.AddRange(filesResponse.Files);
                    }
                }
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetWindowedEventPackages(returnException, 
                eventPackageCollection, 
                arg.Product,
                files, 
                arg.Page, 
                totalRows, 
                arg.EventsPerPage, 
                arg.LoadView,
                minRow,
                maxRow);
        }

        private void HandleResultGetWindowedEventPackages(WorkerResultGetWindowedEventPackages result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                UpdateFileListCore(result.Files);

                // set current/max events page
                this.CurrentEventsPage = result.EventsPage;
                this.CurrentEventsMaxPage = (int) result.TotalRows / result.EventsPerPage;
                if ((result.TotalRows % result.EventsPerPage) > 0)
                {
                    this.CurrentEventsMaxPage++;
                }
                _lastPopulateEventsMaxRow = result.MaxRow;
                _lastPopulateEventsMinRow = result.MinRow;
                _lastPopulateEventsTotalRows = result.TotalRows;

                // update event packages list
                if (result.EventPackageCollection == null)
                {
                    this.EventPackages = null;
                }
                else
                {
                    UpdateEventPackageListCore(result.EventPackageCollection, result.Product == null ? -1 : result.Product.Id);
                }

                RequestUi(ClientLogicUIRequest.EventPackageListReady);
                this.CurrentView = result.LoadView;
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        private void UpdateScriptResultsCore(StackHashScriptResultFiles resultFiles)
        {
            if (_guiDispatcher.CheckAccess())
            {
                if (resultFiles == null)
                {
                    this.ScriptResultFiles = null;
                }
                else
                {
                    if (this.ScriptResultFiles == null)
                    {
                        this.ScriptResultFiles = new ObservableCollection<DisplayScriptResult>();
                        foreach (StackHashScriptResultFile resultFile in resultFiles)
                        {
                            this.ScriptResultFiles.Add(new DisplayScriptResult(resultFile));
                        }
                    }
                    else
                    {
                        // construct a dictionary of result files to update
                        Dictionary<string, StackHashScriptResultFile> resultFileDictionary = new Dictionary<string, StackHashScriptResultFile>();
                        foreach (StackHashScriptResultFile resultFile in resultFiles)
                        {
                            if (!resultFileDictionary.ContainsKey(resultFile.ScriptName))
                            {
                                resultFileDictionary.Add(resultFile.ScriptName, resultFile);
                            }
                        }

                        // construct a dictionary of existing result files
                        Dictionary<string, DisplayScriptResult> displayResultDictionary = new Dictionary<string, DisplayScriptResult>();
                        foreach (DisplayScriptResult displayResult in this.ScriptResultFiles)
                        {
                            if (!displayResultDictionary.ContainsKey(displayResult.ScriptName))
                            {
                                displayResultDictionary.Add(displayResult.ScriptName, displayResult);
                            }
                        }

                        // add and update
                        foreach (string scriptName in resultFileDictionary.Keys)
                        {
                            if (displayResultDictionary.ContainsKey(scriptName))
                            {
                                displayResultDictionary[scriptName].UpdateScriptResult(resultFileDictionary[scriptName]);
                            }
                            else
                            {
                                DisplayScriptResult displayResult = new DisplayScriptResult(resultFileDictionary[scriptName]);
                                this.ScriptResultFiles.Add(displayResult);
                                displayResultDictionary.Add(scriptName, displayResult);
                            }
                        }

                        // remove
                        List<DisplayScriptResult> displayResultsToRemove = new List<DisplayScriptResult>();
                        foreach (string displayResultName in displayResultDictionary.Keys)
                        {
                            if (!resultFileDictionary.ContainsKey(displayResultName))
                            {
                                displayResultsToRemove.Add(displayResultDictionary[displayResultName]);
                            }
                        }
                        foreach (DisplayScriptResult displayResultToRemove in displayResultsToRemove)
                        {
                            this.ScriptResultFiles.Remove(displayResultToRemove);
                        }
                    }
                }
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashScriptResultFiles>(UpdateScriptResultsCore), resultFiles);
            }
        }

        private void UpdateEventPackageListCore(StackHashEventPackageCollection eventPackageCollection, int productId)
        {
            if (_guiDispatcher.CheckAccess())
            {
                // if we don't have an event package list (or index) or the product Id has changed just build a new list
                // (note that the product Id will be -1 for a search)
                if ((this.EventPackages == null) || 
                    (_eventIdToEventsIndex == null) || 
                    (_lastEventPackageListUpdateProductId != productId))
                {
                    this.EventPackages = new ObservableCollection<DisplayEventPackage>();
                    foreach(StackHashEventPackage eventPackage in eventPackageCollection)
                    {
                        this.EventPackages.Add(new DisplayEventPackage(eventPackage, _workFlowIdToMapping, _workFlowMappings));
                    }
                }
                else
                {
                    // construct a dictionary of event packages
                    Dictionary<Tuple<int, string>, StackHashEventPackage> eventPackageDictionary = new Dictionary<Tuple<int, string>, StackHashEventPackage>();
                    foreach (StackHashEventPackage eventPackage in eventPackageCollection)
                    {
                        Tuple<int, string> key = new Tuple<int, string>(eventPackage.EventData.Id, eventPackage.EventData.EventTypeName);

                        // make sure there are no duplicate event Ids
                        if (!eventPackageDictionary.ContainsKey(key))
                        {
                            eventPackageDictionary.Add(key, eventPackage);
                        }
                    }

                    // add and update
                    foreach (StackHashEventPackage eventPackage in eventPackageDictionary.Values)
                    {
                        Tuple<int, string> key = new Tuple<int, string>(eventPackage.EventData.Id, eventPackage.EventData.EventTypeName);

                        if (_eventIdToEventsIndex.ContainsKey(key))
                        {
                            this.EventPackages[_eventIdToEventsIndex[key]].UpdateEventPackage(eventPackage, _workFlowIdToMapping);
                        }
                        else
                        {
                            this.EventPackages.Add(new DisplayEventPackage(eventPackage, _workFlowIdToMapping, _workFlowMappings));
                        }
                    }

                    // remove
                    List<DisplayEventPackage> eventPackagesToRemove = new List<DisplayEventPackage>();
                    foreach (Tuple<int, string> eventPackageId in _eventIdToEventsIndex.Keys)
                    {
                        if (!eventPackageDictionary.ContainsKey(eventPackageId))
                        {
                            eventPackagesToRemove.Add(this.EventPackages[_eventIdToEventsIndex[eventPackageId]]);
                        }
                    }
                    foreach (DisplayEventPackage eventPackageToRemove in eventPackagesToRemove)
                    {
                        this.EventPackages.Remove(eventPackageToRemove);
                    }
                }

                // update the event package index
                if (_eventIdToEventsIndex == null)
                {
                    _eventIdToEventsIndex = new Dictionary<Tuple<int, string>, int>();
                }
                else
                {
                    _eventIdToEventsIndex.Clear();
                }
                for (int i = 0; i < this.EventPackages.Count; i++)
                {
                    Tuple<int, string> key = new Tuple<int, string>(this.EventPackages[i].Id, this.EventPackages[i].EventTypeName);

                    // make sure there are no duplicate event Ids
                    if (!_eventIdToEventsIndex.ContainsKey(key))
                    {
                        _eventIdToEventsIndex.Add(key, i);
                    }
                }

                // update cab collection policies
                if (_collectionPolicies != null)
                {
                    foreach (DisplayEventPackage eventPackage in this.EventPackages)
                    {
                        eventPackage.UpdateCabCollectionPolicy(_collectionPolicies);
                    }
                }

                // store the last product Id
                _lastEventPackageListUpdateProductId = productId;
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashEventPackageCollection, int>(UpdateEventPackageListCore), eventPackageCollection, productId);
            }
        }

        // GetProductList

        private WorkerResultGetProductList WorkerGetProductList(WorkerArgGetProductList arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashProductInfoCollection productCollection = null;
            StackHashStatus serviceStatus = null;
            StackHashCollectionPolicyCollection collectionPolicies = null;
            DateTime lastWinQualSiteUpdate = DateTime.MinValue;
            StackHashMappingCollection workFlowMappings = null;

            try
            {
                GetProductsRequest request = new GetProductsRequest();
                request.ContextId = arg.ContextId;
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetProductsResponse response = ServiceProxy.Services.Projects.GetProducts(request);
                productCollection = response.Products;
                lastWinQualSiteUpdate = response.LastSiteUpdateTime;

                // clear flags used to indicate if the service status actually has the most recent information
                // if an update occurs through an AdminReport between now and using the ServiceState data it
                // will be ignored
                lock (_serviceStateLockObject)
                {
                    _syncRunningUpdatedByAdminReport = false;
                    _purgeRunningUpdatedByAdminReport = false;
                    _analyzeRunningUpdatedByAdminReport = false;
                    _pluginReportRunningUpdatedByAdminReport = false;
                    _moveIndexRunningUpdatedByAdminReport = false;
                }

                GetStackHashServiceStatusRequest serviceStatusRequest = new GetStackHashServiceStatusRequest();
                serviceStatusRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse serviceStatusResponse = ServiceProxy.Services.Admin.GetServiceStatus(serviceStatusRequest);
                serviceStatus = serviceStatusResponse.Status;

                GetDataCollectionPolicyRequest policyRequest = new GetDataCollectionPolicyRequest();
                policyRequest.ClientData = UserSettings.Settings.GenerateClientData();
                policyRequest.ConditionObject = StackHashCollectionObject.Any;
                policyRequest.ContextId = arg.ContextId;
                policyRequest.GetAll = true;
                policyRequest.Id = 0;
                policyRequest.ObjectToCollect = StackHashCollectionObject.Any;
                policyRequest.RootObject = StackHashCollectionObject.Any;

                GetDataCollectionPolicyResponse policyResponse = ServiceProxy.Services.Admin.GetDataCollectionPolicy(policyRequest);
                collectionPolicies = policyResponse.PolicyCollection;

                GetStatusMappingsRequest workFlowMappingsRequest = new GetStatusMappingsRequest();
                workFlowMappingsRequest.ClientData = UserSettings.Settings.GenerateClientData();
                workFlowMappingsRequest.ContextId = arg.ContextId;
                workFlowMappingsRequest.MappingType = StackHashMappingType.WorkFlow;

                GetStatusMappingsResponse workFlowMappingsResponse = ServiceProxy.Services.Admin.GetStatusMappings(workFlowMappingsRequest);
                workFlowMappings = workFlowMappingsResponse.StatusMappings;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetProductList(returnException, 
                productCollection, 
                serviceStatus, 
                collectionPolicies, 
                lastWinQualSiteUpdate,
                workFlowMappings);
        }

        private void UpdateOrAddProductCore(StackHashProductInfo productInfo)
        {
            if (_guiDispatcher.CheckAccess())
            {
                lock (_productListLockObject)
                {
                    // create product lists if needed
                    if (this.Products == null)
                    {
                        this.Products = new ObservableCollection<DisplayProduct>();
                        _productIdToProductsIndex = new Dictionary<int, int>();
                    }

                    // add or update the product
                    if (_productIdToProductsIndex.ContainsKey(productInfo.Product.Id))
                    {
                        this.Products[_productIdToProductsIndex[productInfo.Product.Id]].UpdateProduct(productInfo);
                    }
                    else
                    {
                        this.Products.Add(new DisplayProduct(productInfo));
                        int index = -1;
                        for (int i = this.Products.Count - 1; i >= 0; i++)
                        {
                            if (this.Products[i].Id == productInfo.Product.Id)
                            {
                                index = i;
                                break;
                            }
                        }
                        _productIdToProductsIndex.Add(productInfo.Product.Id, index);
                    }
                }
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashProductInfo>(UpdateOrAddProductCore), productInfo);
            }
        }

        private void UpdateContextListCore(StackHashContextCollection contextCollection)
        {
            if (_guiDispatcher.CheckAccess())
            {
                if (contextCollection == null)
                {
                    // no contexts so make the display context list null
                    this.ContextCollection = null;
                    this.ActiveContextCollection = null;
                }
                else if (this.ContextCollection == null)
                {
                    // display context list is null so create it and add the list of contexts
                    this.ContextCollection = new ObservableCollection<DisplayContext>();
                    this.ActiveContextCollection = new ObservableCollection<DisplayContext>();

                    foreach (StackHashContextSettings settings in contextCollection)
                    {
                        this.ContextCollection.Add(new DisplayContext(settings, null));

                        if (settings.IsActive)
                        {
                            this.ActiveContextCollection.Add(new DisplayContext(settings, null));
                        }
                    }
                }
                else
                {
                    // we already have a list of contexts so need to do a full update

                    // ALL contexts

                    // index existing contexts
                    Dictionary<int, int> _contextIdToContext = new Dictionary<int, int>();
                    for (int i = 0; i < this.ContextCollection.Count; i++)
                    {
                        _contextIdToContext.Add(this.ContextCollection[i].Id, i);
                    }

                    // add/update
                    List<int> validIds = new List<int>();
                    foreach (StackHashContextSettings settings in contextCollection)
                    {
                        if (_contextIdToContext.ContainsKey(settings.Id))
                        {
                            this.ContextCollection[_contextIdToContext[settings.Id]].UpdateSettings(settings);
                        }
                        else
                        {
                            this.ContextCollection.Add(new DisplayContext(settings, null));
                        }

                        validIds.Add(settings.Id);
                    }

                    // remove
                    List<DisplayContext> contextsToRemove = new List<DisplayContext>();
                    foreach (int contextId in _contextIdToContext.Keys)
                    {
                        if (!validIds.Contains(contextId))
                        {
                            contextsToRemove.Add(this.ContextCollection[_contextIdToContext[contextId]]);
                        }
                    }
                    foreach (DisplayContext contextToRemove in contextsToRemove)
                    {
                        this.ContextCollection.Remove(contextToRemove);
                    }

                    // ACTIVE contexts

                    // index existing contexts
                    Dictionary<int, int> contextIdToActiveContext = new Dictionary<int, int>();
                    for (int i = 0; i < this.ActiveContextCollection.Count; i++)
                    {
                        contextIdToActiveContext.Add(this.ActiveContextCollection[i].Id, i);
                    }

                    // add/update
                    List<int> validActiveIds = new List<int>();
                    foreach (StackHashContextSettings settings in contextCollection)
                    {
                        if (settings.IsActive)
                        {
                            if (contextIdToActiveContext.ContainsKey(settings.Id))
                            {
                                this.ActiveContextCollection[contextIdToActiveContext[settings.Id]].UpdateSettings(settings);
                            }
                            else
                            {
                                this.ActiveContextCollection.Add(new DisplayContext(settings, null));
                            }

                            validActiveIds.Add(settings.Id);
                        }
                    }

                    // remove
                    List<DisplayContext> contextsToRemoveFromActive = new List<DisplayContext>();
                    foreach (int contextId in contextIdToActiveContext.Keys)
                    {
                        if (!validActiveIds.Contains(contextId))
                        {
                            contextsToRemoveFromActive.Add(this.ActiveContextCollection[contextIdToActiveContext[contextId]]);
                        }
                    }
                    foreach (DisplayContext contextToRemove in contextsToRemoveFromActive)
                    {
                        this.ActiveContextCollection.Remove(contextToRemove);
                    }
                }
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashContextCollection>(UpdateContextListCore), contextCollection);
            }
        }

        private void UpdateContextListStatusesCore(StackHashStatus status)
        {
            if (_guiDispatcher.CheckAccess())
            {
                if ((this.ContextCollection != null) && (status != null))
                {
                    // index existing contexts
                    Dictionary<int, int> contextIdToContext = new Dictionary<int, int>();
                    for (int i = 0; i < this.ContextCollection.Count; i++)
                    {
                        contextIdToContext.Add(this.ContextCollection[i].Id, i);
                    }

                    Dictionary<int, int> contextIdToActiveContext = new Dictionary<int, int>();
                    for (int i = 0; i < this.ActiveContextCollection.Count; i++)
                    {
                        contextIdToActiveContext.Add(this.ActiveContextCollection[i].Id, i);
                    }

                    // update
                    foreach (StackHashContextStatus contextStatus in status.ContextStatusCollection)
                    {
                        if (contextIdToContext.ContainsKey(contextStatus.ContextId))
                        {
                            this.ContextCollection[contextIdToContext[contextStatus.ContextId]].UpdateStatus(contextStatus);
                        }

                        if (contextIdToActiveContext.ContainsKey(contextStatus.ContextId))
                        {
                            this.ActiveContextCollection[contextIdToActiveContext[contextStatus.ContextId]].UpdateStatus(contextStatus);
                        }
                    }
                }
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashStatus>(UpdateContextListStatusesCore), status);
            }
        }

        private void UpdateProductListCore(StackHashProductInfoCollection productCollection, DateTime lastWinQualSiteUpdate)
        {            
            if (_guiDispatcher.CheckAccess())
            {
                this.LastWinQualSiteUpdate = lastWinQualSiteUpdate;

                lock (_productListLockObject)
                {
                    // update product list
                    if (productCollection == null)
                    {
                        this.Products = null;
                    }
                    else
                    {
                        // build a list of products to display
                        Dictionary<int, StackHashProductInfo> filteredProducts = new Dictionary<int, StackHashProductInfo>(productCollection.Count);

                        if (UserSettings.Settings.DemoMode)
                        {
                            // in demo mode only include non-Cucku products
                            foreach (StackHashProductInfo product in productCollection)
                            {
                                if (product.Product.Name.Contains("Crashy"))
                                {
                                    filteredProducts.Add(product.Product.Id, product);
                                }
                            }
                        }
                        else
                        {
                            if (UserSettings.Settings.ShowDisabledProducts)
                            {
                                // show all products
                                foreach (StackHashProductInfo product in productCollection)
                                {
                                    filteredProducts.Add(product.Product.Id, product);
                                }
                            }
                            else
                            {
                                // filter out disabled products
                                foreach (StackHashProductInfo product in productCollection)
                                {
                                    if (product.SynchronizeEnabled)
                                    {
                                        filteredProducts.Add(product.Product.Id, product);
                                    }
                                }
                            }
                        }

                        // add / update / remove products
                        bool anyEnabled = false;
                        if (this.Products == null)
                        {
                            this.Products = new ObservableCollection<DisplayProduct>();
                            _productIdToProductsIndex = new Dictionary<int, int>();
                        }

                        // add and update
                        foreach (StackHashProductInfo product in filteredProducts.Values)
                        {
                            // see if any product is enabled
                            if (product.SynchronizeEnabled)
                            {
                                anyEnabled = true;
                            }

                            // add or update
                            if (_productIdToProductsIndex.ContainsKey(product.Product.Id))
                            {
                                this.Products[_productIdToProductsIndex[product.Product.Id]].UpdateProduct(product);
                            }
                            else
                            {
                                this.Products.Add(new DisplayProduct(product));
                            }
                        }

                        // remove
                        List<DisplayProduct> productsToRemove = new List<DisplayProduct>();
                        foreach (int productId in _productIdToProductsIndex.Keys)
                        {
                            if (!filteredProducts.ContainsKey(productId))
                            {
                                productsToRemove.Add(this.Products[_productIdToProductsIndex[productId]]);
                            }
                        }
                        foreach (DisplayProduct productToRemove in productsToRemove)
                        {
                            this.Products.Remove(productToRemove);
                        }

                        // update index
                        _productIdToProductsIndex.Clear();
                        for (int i = 0; i < this.Products.Count; i++)
                        {
                            _productIdToProductsIndex.Add(this.Products[i].Id, i);
                        }

                        // flag if any products are enabled
                        this.AnyEnabledProducts = anyEnabled;

                        // sort by name then version - need to do this for products as main sorting disabled during setup wizard
                        ICollectionView dataView = CollectionViewSource.GetDefaultView(this.Products);
                        dataView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        dataView.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
                        dataView.Refresh();
                    }
                }

                RequestUi(ClientLogicUIRequest.ProductsUpdated);
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashProductInfoCollection, DateTime>(UpdateProductListCore), 
                    productCollection, 
                    lastWinQualSiteUpdate);
            }
        }

        private void UpdateMappingsCore(StackHashMappingCollection workFlowMappings)
        {
            if (_guiDispatcher.CheckAccess())
            {
                if (workFlowMappings == null) { return; }

                if (this.WorkFlowMappings == null)
                {
                    this.WorkFlowMappings = new ObservableCollection<DisplayMapping>();
                    _workFlowIdToMapping = new Dictionary<int, DisplayMapping>();
                }

                foreach (StackHashMapping mapping in workFlowMappings)
                {
                    if (_workFlowIdToMapping.ContainsKey(mapping.Id))
                    {
                        _workFlowIdToMapping[mapping.Id].UpdateMapping(mapping);
                    }
                    else
                    {
                        DisplayMapping displayMapping = new DisplayMapping(mapping);

                        this.WorkFlowMappings.Add(displayMapping);
                        _workFlowIdToMapping.Add(mapping.Id, displayMapping);
                    }
                }
            }
            else
            {
                _guiDispatcher.Invoke(new Action<StackHashMappingCollection>(UpdateMappingsCore), workFlowMappings);
            }
        }

        private void GetProductListResult(WorkerResultGetProductList result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                this.ServiceStatus = result.ServiceStatus;
                UpdateServiceState(result.ServiceStatus, _contextId);
                UpdateProductListCore(result.ProductCollection, result.LastWinQualSiteUpdate);
                UpdateCollectionPoliciesCore(result.CollectionPolicies);
                UpdateMappingsCore(result.WorkFlowMappings);

                // clear any existing product summary data
                foreach(DisplayProduct product in this.Products)
                {
                    product.UpdateHitSummaries(null, null, null);
                }
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;

            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.ProductListUpdated, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        // SetContextSettings

        private static WorkerResultSetContextSettings WorkerSetContextSettings(WorkerArgSetContextSettings arg)
        {
            Debug.Assert(arg != null);

            Exception returnException = null;
            StackHashCollectionPolicyCollection collectionPolicies = null;
            StackHashMappingCollection workFlowMappings = null;

            try
            {
                // if context is active need to deactivate before saving settings
                if (arg.IsActive)
                {
                    DeactivateStackHashContextRequest deactivateRequest = new DeactivateStackHashContextRequest();
                    deactivateRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    deactivateRequest.ContextId = arg.ContextId;

                    ServiceProxy.Services.Admin.DeactivateStackHashContext(deactivateRequest);
                }

                SetStackHashPropertiesRequest request = new SetStackHashPropertiesRequest();
                request.Settings = arg.Settings;
                request.ClientData = UserSettings.Settings.GenerateClientData();
                UpdatePropertiesRequestToUNC(ref request);

                ServiceProxy.Services.Admin.SetStackHashSettings(request);

                StackHashEmailSettings emailSettings = null;
                foreach (StackHashContextSettings contextSettings in arg.Settings.ContextCollection)
                {
                    if (contextSettings.Id == arg.ContextId)
                    {
                        emailSettings = contextSettings.EmailSettings;
                    }
                }

                if (emailSettings != null)
                {
                    SetEmailSettingsRequest emailRequest = new SetEmailSettingsRequest();
                    emailRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    emailRequest.ContextId = arg.ContextId;
                    emailRequest.EmailSettings = emailSettings;

                    ServiceProxy.Services.Admin.SetEmailSettings(emailRequest);
                }

                if (arg.CollectionPolicies != null)
                {
                    // save the updated collection policies
                    SetDataCollectionPolicyRequest setPolicyRequest = new SetDataCollectionPolicyRequest();
                    setPolicyRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    setPolicyRequest.ContextId = arg.Settings.ContextCollection[0].Id;
                    setPolicyRequest.PolicyCollection = arg.CollectionPolicies;
                    setPolicyRequest.SetAll = false;

                    ServiceProxy.Services.Admin.SetDataCollectionPolicy(setPolicyRequest);

                    // get a fresh list of all policies
                    GetDataCollectionPolicyRequest getPolicyRequest = new GetDataCollectionPolicyRequest();
                    getPolicyRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    getPolicyRequest.ConditionObject = StackHashCollectionObject.Any;
                    getPolicyRequest.ContextId = arg.Settings.ContextCollection[0].Id;
                    getPolicyRequest.GetAll = true;
                    getPolicyRequest.Id = 0;
                    getPolicyRequest.ObjectToCollect = StackHashCollectionObject.Any;
                    getPolicyRequest.RootObject = StackHashCollectionObject.Any;

                    GetDataCollectionPolicyResponse getPolicyResponse = ServiceProxy.Services.Admin.GetDataCollectionPolicy(getPolicyRequest);
                    collectionPolicies = getPolicyResponse.PolicyCollection;
                }

                if (arg.PlugIns != null)
                {
                    // save plugins
                    SetContextBugTrackerPlugInSettingsRequest setPluginsRequest = new SetContextBugTrackerPlugInSettingsRequest();
                    setPluginsRequest.BugTrackerPlugInSettings = new StackHashBugTrackerPlugInSettings();
                    setPluginsRequest.BugTrackerPlugInSettings.PlugInSettings = arg.PlugIns;
                    setPluginsRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    setPluginsRequest.ContextId = arg.ContextId;

                    ServiceProxy.Services.Admin.SetContextBugTrackerPlugInSettings(setPluginsRequest);
                }

                if (arg.WorkFlowMappings != null)
                {
                    // save workflow mappings
                    SetStatusMappingsRequest setWorkFlowMappingsRequest = new SetStatusMappingsRequest();
                    setWorkFlowMappingsRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    setWorkFlowMappingsRequest.ContextId = arg.ContextId;
                    setWorkFlowMappingsRequest.MappingType = StackHashMappingType.WorkFlow;
                    setWorkFlowMappingsRequest.StatusMappings = arg.WorkFlowMappings;

                    ServiceProxy.Services.Admin.SetStatusMappings(setWorkFlowMappingsRequest);

                    // get a fresh collection of mappings
                    GetStatusMappingsRequest getWorkFlowMappingsRequest = new GetStatusMappingsRequest();
                    getWorkFlowMappingsRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    getWorkFlowMappingsRequest.ContextId = arg.ContextId;
                    getWorkFlowMappingsRequest.MappingType = StackHashMappingType.WorkFlow;

                    GetStatusMappingsResponse getWorkFlowMappingsResponse = ServiceProxy.Services.Admin.GetStatusMappings(getWorkFlowMappingsRequest);
                    workFlowMappings = getWorkFlowMappingsResponse.StatusMappings;
                }

                // if context was active need to activate after saving settings
                if (arg.IsActive)
                {
                    ActivateStackHashContextRequest activateRequest = new ActivateStackHashContextRequest();
                    activateRequest.ClientData = UserSettings.Settings.GenerateClientData();
                    activateRequest.ContextId = arg.ContextId;

                    ServiceProxy.Services.Admin.ActivateStackHashContext(activateRequest);
                }
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultSetContextSettings(returnException, collectionPolicies, workFlowMappings);
        }

        private void SetContextSettingsResult(WorkerResultSetContextSettings result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // update the list of collection policies
                if (result.CollectionPolicies != null)
                {
                    UpdateCollectionPoliciesCore(result.CollectionPolicies);
                }

                // update the list of workflow mappings
                if (result.WorkFlowMappings != null)
                {
                    UpdateMappingsCore(result.WorkFlowMappings);
                }

                // refresh the list of context settings
                RefreshContextSettings();

                // don't clear NotBusy
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);

                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }
        }

        // GetContextSettings

        private static WorkerResultGetContextSettings WorkerGetContextSettings()
        {
            Exception returnException = null;
            StackHashSettings settings = null;
            StackHashStatus serviceStatus = null;
            StackHashBugTrackerPlugInDiagnosticsCollection availablePlugIns = null;

            try
            {
                GetStackHashPropertiesRequest request = new GetStackHashPropertiesRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashPropertiesResponse response = ServiceProxy.Services.Admin.GetStackHashSettings(request);
                settings = response.Settings;

                GetStackHashServiceStatusRequest serviceStatusRequest = new GetStackHashServiceStatusRequest();
                serviceStatusRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashServiceStatusResponse serviceStatusResponse = ServiceProxy.Services.Admin.GetServiceStatus(serviceStatusRequest);
                serviceStatus = serviceStatusResponse.Status;

                GetBugTrackerPlugInDiagnosticsRequest pluginRequest = new GetBugTrackerPlugInDiagnosticsRequest();
                pluginRequest.ClientData = UserSettings.Settings.GenerateClientData();
                pluginRequest.ContextId = -1;
                pluginRequest.PlugInName = null;

                GetBugTrackerPlugInDiagnosticsResponse pluginResponse = ServiceProxy.Services.Admin.GetBugTrackerDiagnostics(pluginRequest);
                availablePlugIns = pluginResponse.BugTrackerPlugInDiagnostics;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }

            return new WorkerResultGetContextSettings(returnException, settings, serviceStatus, availablePlugIns);
        }

        private void GetContextSettingsResult(WorkerResultGetContextSettings result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // we should have a list of settings
                if ((result.ContextSettings == null) || (result.ContextSettings.ContextCollection == null))
                {
                    this.ContextCollection = null;
                    this.ServiceLogEnabled = false;
                    this.ServiceReportingEnabled = false;
                    this.ServiceProxySettings = null;
                }
                else
                {
                    UpdateContextListCore(result.ContextSettings.ContextCollection);

                    // update plugins for the current context (if we have one)
                    if (_contextId != UserSettings.InvalidContextId)
                    {
                        UpdateActivePlugins(result.ContextSettings, _contextId);
                    }

                    this.ServiceLogEnabled = result.ContextSettings.EnableLogging;
                    this.ServiceReportingEnabled = result.ContextSettings.ReportingEnabled;
                    this.ServiceProxySettings = result.ContextSettings.ProxySettings;
                }

                if (result.ContextSettings != null)
                {
                    this.ClientTimeoutInSeconds = result.ContextSettings.ClientTimeoutInSeconds;
                }

                // update service status after updating the context collection
                this.ServiceStatus = result.ServiceStatus;

                // update service state for the current context (if we have one)
                if (_contextId != UserSettings.InvalidContextId)
                {
                    UpdateServiceState(result.ServiceStatus, _contextId);
                }

                // update available plugin list
                if (result.AvailablePlugIns == null)
                {
                    this.AvailablePlugIns = null;
                }
                else
                {
                    this.AvailablePlugIns = new ObservableCollection<StackHashBugTrackerPlugInDiagnostics>(result.AvailablePlugIns);
                }

                // let the UI know that the context settings collection has been updated
                RequestUi(ClientLogicUIRequest.ContextCollectionReady);
            }
            else
            {
                this.ContextCollection = null;
                this.ServiceLogEnabled = false;
                this.ServiceReportingEnabled = false;
                this.ServiceProxySettings = null;

                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceCallFailed);
            }

            this.StatusText = Properties.Resources.Status_Ready;
            this.NotBusy = true;
        }

        // ServiceConnect

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private WorkerResultServiceConnect WorkerServiceConnect()
        {
            Exception returnException = null;
            StackHashSettings settings = null;
            StackHashStatus serviceStatus = null;
            StackHashScriptFileDataCollection scriptDataCollection = null;
            StackHashBugTrackerPlugInDiagnosticsCollection availablePlugIns = null;
            string webServiceOverrideAddress = null;
            Guid localServiceGuid = Guid.Empty;
            bool clientIsLocal = false;

            try
            {
                ServiceProxy.Services.OpenAll();

                // if we're registered before unregister and grab a new session Id
                if (_initialServiceRegisterComplete)
                {
                    UnregisterCurrentSessionId();
                    UserSettings.Settings.GenerateNewSessionId();
                }

                // Get the local service Guid (if it exists)
                localServiceGuid = GetLocalServiceGuid();
                UserSettings.Settings.LocalServiceGuid = localServiceGuid;

                // make sure we're compatible with the service - the call will exception if not
                CheckVersionRequest checkVersionRequest = new CheckVersionRequest();
                checkVersionRequest.ServiceGuid = localServiceGuid == Guid.Empty ? null : localServiceGuid.ToString();
                checkVersionRequest.ClientData = UserSettings.Settings.GenerateClientData();
                checkVersionRequest.MajorVersion = UserSettings.Settings.VersionMajor;
                checkVersionRequest.MinorVersion = UserSettings.Settings.VersionMinor;
                CheckVersionResponse checkVersionResponse = ServiceProxy.Services.Admin.CheckVersion(checkVersionRequest);

                // we now now if we're local to the service
                clientIsLocal = checkVersionResponse.IsLocalClient;

                RegisterRequest registerRequest = new RegisterRequest();
                registerRequest.ClientData = UserSettings.Settings.GenerateClientData();
                registerRequest.IsRegister = true;
                ServiceProxy.Services.Admin.RegisterForNotifications(registerRequest);

                _initialServiceRegisterComplete = true;

                // get the web service override address (if any)
                GetTestModeSettingRequest testModeSettingRequest = new GetTestModeSettingRequest();
                testModeSettingRequest.AttributeName = "StackHashWebServiceEndpoint";
                testModeSettingRequest.ClientData = UserSettings.Settings.GenerateClientData();
                GetTestModeSettingResponse testModeSettingResponse = ServiceProxy.Services.Test.GetTestModeSetting(testModeSettingRequest);
                webServiceOverrideAddress = testModeSettingResponse.AttributeValue;

                GetStackHashPropertiesRequest request = new GetStackHashPropertiesRequest();
                request.ClientData = UserSettings.Settings.GenerateClientData();

                GetStackHashPropertiesResponse response = ServiceProxy.Services.Admin.GetStackHashSettings(request);
                settings = response.Settings;

                GetDebuggerScriptNamesRequest scriptNamesRequest = new GetDebuggerScriptNamesRequest();
                scriptNamesRequest.ClientData = UserSettings.Settings.GenerateClientData();

                GetDebuggerScriptNamesResponse scriptNamesResponse = ServiceProxy.Services.Admin.GetDebuggerScriptNames(scriptNamesRequest);
                scriptDataCollection = scriptNamesResponse.ScriptFileData;

                GetBugTrackerPlugInDiagnosticsRequest pluginRequest = new GetBugTrackerPlugInDiagnosticsRequest();
                pluginRequest.ClientData = UserSettings.Settings.GenerateClientData();
                pluginRequest.ContextId = -1;
                pluginRequest.PlugInName = null;

                GetBugTrackerPlugInDiagnosticsResponse pluginResponse = ServiceProxy.Services.Admin.GetBugTrackerDiagnostics(pluginRequest);
                availablePlugIns = pluginResponse.BugTrackerPlugInDiagnostics;

                GetStackHashServiceStatusRequest serviceStatusRequest = new GetStackHashServiceStatusRequest();
                serviceStatusRequest.ClientData = UserSettings.Settings.GenerateClientData();

                // clear flags used to indicate if the service status actually has the most recent information
                // if an update occurs through an AdminReport between now and using the ServiceState data it
                // will be ignored
                lock (_serviceStateLockObject)
                {
                    _syncRunningUpdatedByAdminReport = false;
                    _purgeRunningUpdatedByAdminReport = false;
                    _analyzeRunningUpdatedByAdminReport = false;
                    _pluginReportRunningUpdatedByAdminReport = false;
                    _moveIndexRunningUpdatedByAdminReport = false;
                }

                GetStackHashServiceStatusResponse serviceStatusResponse = ServiceProxy.Services.Admin.GetServiceStatus(serviceStatusRequest);
                serviceStatus = serviceStatusResponse.Status;
            }
            catch (CommunicationException ex)
            {
                returnException = ex;
            }
            catch (TimeoutException timeoutException)
            {
                returnException = timeoutException;
            }
            catch (Exception exception)
            {
                returnException = exception;
            }

            return new WorkerResultServiceConnect(returnException, 
                settings, 
                serviceStatus, 
                scriptDataCollection, 
                availablePlugIns, 
                webServiceOverrideAddress,
                localServiceGuid,
                clientIsLocal);
        }

        private bool HasContextEverSynced(int contextId)
        {
            bool hasEverSynced = false;

            if (this.ServiceStatus != null)
            {
                foreach(StackHashContextStatus contextStatus in this.ServiceStatus.ContextStatusCollection)
                {
                    if (contextStatus.ContextId == contextId)
                    {
                        foreach (StackHashTaskStatus taskStatus in contextStatus.TaskStatusCollection)
                        {
                            if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                            {
                                hasEverSynced = taskStatus.RunCount > 0;

                                // stop searching tasks
                                break;
                            }
                        }

                        // stop searching contexts
                        break;
                    }
                }
            }

            return hasEverSynced;
        }

        private void UpdateActivePlugins(StackHashSettings settings, int contextId)
        {
            ObservableCollection<StackHashBugTrackerPlugIn> activePlugins = new ObservableCollection<StackHashBugTrackerPlugIn>();

            if ((settings != null) && (settings.ContextCollection != null))
            {
                foreach (StackHashContextSettings contextSettings in settings.ContextCollection)
                {
                    if (contextSettings.Id == contextId)
                    {
                        if ((contextSettings.BugTrackerSettings != null) && (contextSettings.BugTrackerSettings.PlugInSettings != null))
                        {
                            foreach (StackHashBugTrackerPlugIn plugin in contextSettings.BugTrackerSettings.PlugInSettings)
                            {
                                if (plugin.Enabled)
                                {
                                    activePlugins.Add(plugin);
                                }
                            }
                        }

                        // found it, we can stop looking
                        break;
                    }
                }
            }

            this.ActivePlugins = activePlugins;
        }

        private void UpdatePluginErrorFlagFromPlugInDiagnostics(StackHashBugTrackerPlugInDiagnosticsCollection diagnostics)
        {
            bool pluginHasError = false;

            // check for plugin errors
            if (diagnostics != null)
            {
                foreach (StackHashBugTrackerPlugInDiagnostics plugin in diagnostics)
                {
                    // is there a last exception?
                    if (!string.IsNullOrEmpty(plugin.LastException))
                    {
                        // if it's anything other than No Error we have a plugin failure
                        if (string.Compare("No Error", plugin.LastException, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            pluginHasError = true;
                            break;
                        }
                    }
                }
            }

            this.PluginHasError = pluginHasError;
        }

        private void UpdateServiceState(StackHashStatus status, int contextId)
        {
            if (status == null) { return; }
            if (status.ContextStatusCollection == null) { return; }
            if (contextId == UserSettings.InvalidContextId) { return; }

            this.ServiceStatus = status;

            lock (_serviceStateLockObject)
            {
                _syncStageText = null;

                foreach (StackHashContextStatus contextStatus in status.ContextStatusCollection)
                {
                    if (contextStatus.ContextId == contextId)
                    {
                        // update plugin error status
                        UpdatePluginErrorFlagFromPlugInDiagnostics(contextStatus.PlugInDiagnostics);

                        // update task status
                        foreach (StackHashTaskStatus taskStatus in contextStatus.TaskStatusCollection)
                        {
                            switch (taskStatus.TaskType)
                            {
                                case StackHashTaskType.AnalyzeTask:
                                    if (!_analyzeRunningUpdatedByAdminReport)
                                    {
                                        this.AnalyzeRunning = taskStatus.TaskState == StackHashTaskState.Running;
                                    }
                                    break;

                                case StackHashTaskType.PurgeTask:
                                    if (!_purgeRunningUpdatedByAdminReport)
                                    {
                                        this.PurgeRunning = taskStatus.TaskState == StackHashTaskState.Running;
                                    }
                                    break;

                                case StackHashTaskType.WinQualSynchronizeTask:
                                    if (!_syncRunningUpdatedByAdminReport)
                                    {
                                        this.SyncRunning = taskStatus.TaskState == StackHashTaskState.Running;
                                    }
                                    break;

                                case StackHashTaskType.BugReportTask:
                                    if (!_pluginReportRunningUpdatedByAdminReport)
                                    {
                                        this.PluginReportRunning = taskStatus.TaskState == StackHashTaskState.Running;
                                    }
                                    break;

                                case StackHashTaskType.ErrorIndexMoveTask:
                                    if (!_moveIndexRunningUpdatedByAdminReport)
                                    {
                                        this.MoveIndexRunning = taskStatus.TaskState == StackHashTaskState.Running;
                                    }
                                    break;
                            }
                        }

                        // stop searching through the contexts
                        break;
                    }
                }

                UpdateServiceStatusText();
            }
        }

        private void ServiceConnectResult(WorkerResultServiceConnect result)
        {
            Debug.Assert(result != null);

            if (result.Ex == null)
            {
                // update the local client status
                UserSettings.Settings.LocalServiceGuid = result.LocalServiceGuid;
                this.ServiceIsLocal = result.ServiceIsLocal;

                // set the web service override address
                UserSettings.Settings.WebServiceOverrideAddress = result.WebServiceOverrideAddress;

                // save initial list of script names
                if (result.ScriptDataCollection != null)
                {
                    this.ScriptData = new ObservableCollection<StackHashScriptFileData>(result.ScriptDataCollection);

                    // sort by name
                    SortCollection(this.ScriptData, "Name", ListSortDirection.Ascending);
                }

                // save list of available plugins
                if (result.AvailablePlugIns != null)
                {
                    this.AvailablePlugIns = new ObservableCollection<StackHashBugTrackerPlugInDiagnostics>(result.AvailablePlugIns);
                }

                // flag that we've connected OK at least once this session
                _initialServiceConnectComplete = true;
                _initialServiceConnectHost = ServiceProxy.Services.ServiceHost;
                _initialServiceConnectPort = ServiceProxy.Services.ServicePort;

                // if we get this far we're not bumped any more
                _clientIsBumped = false;

                bool contextIdOk = false;
                bool xmlIndexExits = false;

                // we should have a list of settings
                if ((result.ContextSettings == null) || (result.ContextSettings.ContextCollection == null))
                {
                    this.ContextCollection = null;
                    this.ServiceLogEnabled = false;
                    this.ServiceProxySettings = null;
                }
                else
                {
                    UpdateContextListCore(result.ContextSettings.ContextCollection);
                    this.ServiceLogEnabled = result.ContextSettings.EnableLogging;
                    this.ServiceProxySettings = result.ContextSettings.ProxySettings;

                    // check to see if any XML indexes are present
                    foreach (DisplayContext settings in this.ContextCollection)
                    {
                        if (settings.StackHashContextSettings.ErrorIndexSettings.Type == ErrorIndexType.Xml)
                        {
                            xmlIndexExits = true;
                            break;
                        }
                    }

                    // if a context Id has been selected make sure it's still valid
                    if (UserSettings.Settings.CurrentContextId != UserSettings.InvalidContextId)
                    {
                        foreach (DisplayContext context in this.ContextCollection)
                        {
                            if (context.Id == UserSettings.Settings.CurrentContextId)
                            {
                                // found - the context Id - it most also be active
                                contextIdOk = context.IsActive;
                                break;
                            }
                        }
                    }
                }

                if (result.ContextSettings != null)
                {
                    this.ClientTimeoutInSeconds = result.ContextSettings.ClientTimeoutInSeconds;
                }

                // update service status - do this after updating the context collection
                this.ServiceStatus = result.ServiceStatus;

                if (xmlIndexExits)
                {
                    // user needs to updgrade from XML
                    this.StatusText = Properties.Resources.Status_Ready;
                    this.NotBusy = true;
                    RequestUi(ClientLogicUIRequest.UpgradeIndexFromXml);
                }
                else
                {
                    // no XML index found, continue with other checks

                    // if no valid context Id clear the current selection
                    if (!contextIdOk)
                    {
                        UserSettings.Settings.CurrentContextId = UserSettings.InvalidContextId;
                    }

                    // success, check for valid context id
                    if (UserSettings.Settings.CurrentContextId == UserSettings.InvalidContextId)
                    {
                        this.StatusText = Properties.Resources.Status_Configure;
                        this.NotBusy = true;

                        // tell the Ui that we need to configure a profile
                        RequestUi(ClientLogicUIRequest.ProfileSettings);
                    }
                    else
                    {
                        // valid context Id, we're good to go
                        UpdateServiceState(result.ServiceStatus, UserSettings.Settings.CurrentContextId);
                        UpdateActivePlugins(result.ContextSettings, UserSettings.Settings.CurrentContextId);

                        // Update based on context id
                        CheckForContextIdChange();
                    }
                }
            }
            else
            {
                // report error
                ReportError(result.Ex, ClientLogicErrorCode.ServiceConnectFailed);

                this.StatusText = Properties.Resources.Status_Ready;
                this.NotBusy = true;
            }

            // notify the setup wizard (if it's listening)
            NotifySetupWizard(ClientLogicSetupWizardPromptOperation.AdminServiceConnect, result.Ex == null, StackHashServiceErrorCode.NoError, result.Ex);
        }

        private static void SortCollection(ICollection collection, string sortProperty, ListSortDirection direction)
        {
            Debug.Assert(collection != null);
            Debug.Assert(sortProperty != null);

            ICollectionView dataView = CollectionViewSource.GetDefaultView(collection);
            dataView.SortDescriptions.Add(new SortDescription(sortProperty, direction));
            dataView.Refresh();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RegisterTimerCallback(object unused)
        {
            try
            {
                if ((_initialServiceConnectComplete) && (!_clientIsBumped))
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Re-registering for service notifications (timer).");

                    RegisterRequest request = new RegisterRequest();
                    request.ClientData = UserSettings.Settings.GenerateClientData();
                    request.IsRegister = true;
                    ServiceProxy.Services.Admin.RegisterForNotifications(request);
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning,
                    "RegisterTimerCallback Failed",
                    ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void TryReRegisterForNotifications()
        {
            try
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Attempting to Re-register for service notifications.");

                // if we're registered before unregister and grab a new session Id
                if (_initialServiceRegisterComplete)
                {
                    UnregisterCurrentSessionId();
                    UserSettings.Settings.GenerateNewSessionId();
                }

                // attempt to register again
                RegisterRequest registerRequest = new RegisterRequest();
                registerRequest.ClientData = UserSettings.Settings.GenerateClientData();
                registerRequest.IsRegister = true;
                ServiceProxy.Services.Admin.RegisterForNotifications(registerRequest);

                // if we get this far we're not bumped any more
                _clientIsBumped = false;
            }
            catch { }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void UnregisterCurrentSessionId()
        {
            try
            {
                RegisterRequest registerRequest = new RegisterRequest();
                registerRequest.ClientData = UserSettings.Settings.GenerateClientData();
                registerRequest.IsRegister = false;
                ServiceProxy.Services.Admin.RegisterForNotifications(registerRequest);
            }
            catch { }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event fired when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose the ClientLogic object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Dispose(bool canDisposeManagedResources)
        {
            if (canDisposeManagedResources)
            {
                if (_worker != null)
                {
                    _worker.Dispose();
                    _worker = null;
                }

                if (_registerTimer != null)
                {
                    _registerTimer.Dispose();
                    _registerTimer = null;
                }

                UnregisterCurrentSessionId();

                this.IsDisposed = true;
            }
        }

        /// <summary />
        ~ClientLogic()
        {
            Dispose(false);
        }

        #endregion
    }
}
