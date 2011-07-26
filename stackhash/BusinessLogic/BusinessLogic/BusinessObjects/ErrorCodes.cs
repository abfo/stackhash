using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [SuppressMessage("Microsoft.Design", "CA1027")]
    public enum StackHashServiceErrorCode
    {
        [EnumMember()]
        NoError = 0,                // No error has occurred.

        //
        // Errors that should be reported to the user.
        //
        [EnumMember()]
        UnexpectedError,            // Unexpected error occurred - report this error.
        [EnumMember()]
        DebuggerNotConfigured,      // No debugger has been set up to run on dump files.
        [EnumMember()]
        CabDoesNotExist,            // The cab file does not exist. Try downloading the cab from winqual and try again.
        [EnumMember()]
        CabIsCorrupt,               // Cab file cannot be unpackaged. Try downloading the file from WinQual again.
        [EnumMember()]
        Aborted,                    // Cancelled by user.
        [EnumMember()]
        ServerDown,                 // Feed error - server probably down.
        [EnumMember()]
        AuthenticationFailure,      // User credentials invalid.
        [EnumMember()]
        AccessDenied,               // Not permitted to access the requested resource.
        [EnumMember()]
        CabDownloadFailed,          // Might be an intermittent comms issue - try again later.
        [EnumMember()]
        ContextIsNotActive,         // Operation failed because profile is not active. Activate the profile and try again.
        [EnumMember()]
        ContextIsActive,            // Operation failed because profile is active. Deactivate the profile and try again.
        [EnumMember()]
        CannotOverwriteScriptFile,  // Script file exists and overwrite not specified when attempting to save the script file again.
        [EnumMember()]
        CannotRemoveSystemScripts,  // Attempt was made to remove a system script file.
        [EnumMember()]
        CannotRemoveReadOnlyScripts,// Attempt was made to remove a read only script file.
        [EnumMember()]
        ScriptDoesNotExist,         // Attempted operation on a script failed because script does not exist. Perhaps another client removed the script or drive is inaccessible.
        [EnumMember()]
        DebuggerError,              // Error running the debugger on a dump file.         
        [EnumMember()]
        ScheduleFormatError,        // An error was found in the schedule format.         
        [EnumMember()]
        ErrorIndexAssigned,         // The error index is already assigned to another profile.
        [EnumMember()]
        ErrorIndexNameAssigned,     // The error index name is already assigned to another profile.
        [EnumMember()]
        ErrorIndexNameIsDefault,    // Attempt to activate an error index with a default name. Must change the index name.
        [EnumMember()]
        TaskAlreadyInProgress,      // The task is already in progress and only one of this type of task is allowed.
        [EnumMember()]
        ProfileActive,              // Action not allowed while profile is active.
        [EnumMember()]
        WinQualLogOnFailed,         // Failed to log in - server may be down or username and password may be incorrect.
        [EnumMember()]
        ProfileInactive,            // Cannot perform task on an inactive profile.
        [EnumMember()]
        FailedToGetLicense,         // Failed to get license from the StackHash web site.
        [EnumMember()]
        LicenseNotFound,            // Failed to get license from the StackHash web site - not found.
        [EnumMember()]
        NoLicense,                  // Operation cannot be performed because there is no license.
        [EnumMember()]
        LicenseExpired,             // The installed license has expired.
        [EnumMember()]
        LicenseEventCountExceeded,  // The number of events stored has exceeded the installed license limit.
        [EnumMember()]
        LicenseClientLimitExceeded, // The number of StackHash clients has exceeded the installed client limit.
        [EnumMember()]
        MoveInProgress,             // Cannot perform specified action while move is in progress.
        [EnumMember()]
        CopyInProgress,             // Cannot perform specified action while copy is in progress.
        [EnumMember()]
        ErrorIndexMustExist,        // The operation cannot be performed if the index hasn't been created (activated) once before.
        [EnumMember()]
        ErrorIndexTypeNotSupported, // The specified error index type is not supported.
        [EnumMember()]
        ErrorIndexAccessDenied,     // Unable to access the index folder. Ensure the StackHash login context has access.
        [EnumMember()]
        ClientVersionMismatch,      // The client version predates the current service version. Upgrade to latest client.
        [EnumMember()]
        InvalidDatabaseName,        // The selected database name is invalid.
        [EnumMember()]
        CannotChangeIndexFolderOnceCreated,   // An attempt to change the index settings when index created.
        [EnumMember()]
        CannotMoveToExistingFolder,  // Cannot MOVE a folder to an existing location - unless the database already exists. 
        [EnumMember()]
        CopyIndexDeleteButNoSwitch,  // Cannot copy and index and delete the source unless the context is switched to the new index first.
        [EnumMember()]
        ActivateFailed,              // An attempt to activate a context failed.
        [EnumMember()]
        DeactivateFailed,            // An attempt to deactivate a context failed.
        [EnumMember()]
        ContextLoadError,            // An error occurred during the load of a context.
        [EnumMember()]
        ContextActivationError,      // An error occurred during the activation of a context.
        [EnumMember()]
        SqlConnectionError,          // Failed to connect to SQL server.
        [EnumMember()]
        FailedToCreateDatabase,      // Attempt to create the database failed.
        [EnumMember()]
        DatabaseNotFound,           // The specified database was not found.

        [EnumMember()]
        FailedToSetDatabaseOnlineState,   // Failed to set the database online.
        [EnumMember()]
        FailedToChangeDatabaseLogicalName, // Name could not be changed during a database move or copy.
        [EnumMember()]
        FailedToChangeDatabaseLocation,    // Location of database files could not be changed during a move or copy.
        [EnumMember()]
        FailedToDeleteDatabase,           // Failed to delete the database.
        [EnumMember()]
        FailedToSelectDatabase,           // Failed to select the database.
        [EnumMember()]
        FailedToGetControlData,           // Failed to get the control table entry.
        [EnumMember()]
        FailedToDetermineIfControlEntryExists, // Failed to       
        [EnumMember()]
        FailedToAddControlData,
        [EnumMember()]
        FailedToUpdateControlData,
        [EnumMember()]
        FailedToReadProductControlData,
        [EnumMember()]
        FailedToSeeIfProductControlDataExists,
        [EnumMember()]
        FailedToAddProductControlData,
        [EnumMember()]
        FailedToUpdateProductControlData,
        [EnumMember()]
        FailedToReadTaskControlData,
        [EnumMember()]
        FailedToSeeIfTaskControlDataExists,
        [EnumMember()]
        FailedToAddTaskControlData,
        [EnumMember()]
        FailedToUpdateTaskControlData,
        [EnumMember()]
        FailedToAddProduct,
        [EnumMember()]
        FailedToUpdateProduct,
        [EnumMember()]
        FailedToReadProducts,
        [EnumMember()]
        FailedToSeeIfProductExists,
        [EnumMember()]
        FailedToGetProductEventCount,
        [EnumMember()]
        FailedToGetProductCount,
        [EnumMember()]
        FailedToGetProductMatch,
        [EnumMember()]
        FailedToAddFile,
        [EnumMember()]
        FailedToUpdateFile,
        [EnumMember()]
        FailedToSeeIfFileExists,
        [EnumMember()]
        FailedToGetFileCount,
        [EnumMember()]
        FailedToSeeIfProductFileExists,
        [EnumMember()]
        FailedToAddProductFile,
        [EnumMember()]
        FailedToGetFiles,
        [EnumMember()]
        FailedToGetFilesMatch,
        [EnumMember()]
        FailedToGetFile,
        [EnumMember()]
        FailedToAddEvent,
        [EnumMember()]
        FailedToUpdateEvent,
        [EnumMember()]
        FailedToGetEvent,
        [EnumMember()]
        FailedToGetEventCount,
        [EnumMember()]
        FailedToLoadEventList,
        [EnumMember()]
        GetProductEvents,
        [EnumMember()]
        FailedToGetEvents,
        [EnumMember()]
        FailedToGetWindow,
        [EnumMember()]
        FailedToSeeIfEventExists,
        [EnumMember()]
        FailedToGetEventTypeId,
        [EnumMember()]
        FailedToGetEventType,
        [EnumMember()]
        FailedToSeeIfEventTypeExists,
        [EnumMember()]
        FailedToAddEventType,
        [EnumMember()]
        FailedToAddFileEvent,
        [EnumMember()]
        FailedToSeeIfFileEventExists,
        [EnumMember()]
        FailedToAddEventNote,
        [EnumMember()]
        FailedToGetEventNotes,
        [EnumMember()]
        FailedToAddEventInfo,
        [EnumMember()]
        FailedToAddEventInfos,
        [EnumMember()]
        FailedToGetEventInfos,
        [EnumMember()]
        FailedToGetEventInfoPackageCollection,
        [EnumMember()]
        FailedToGetMostRecentHitDate,
        [EnumMember()]
        FailedToGetHitCount,
        [EnumMember()]
        FailedToGetOperatingSystemId,
        [EnumMember()]
        FailedToGetOperatingSystemDetails,
        [EnumMember()]
        FailedToSeeIfOperatingSystemExists,
        [EnumMember()]
        FailedToAddOperatingSystem,
        [EnumMember()]
        FailedToPrimeLocaleCache,
        [EnumMember()]
        FailedToDetermineIfLocaleExists,
        [EnumMember()]
        FailedToAddLocale,
        [EnumMember()]
        FailedToAddCab,
        [EnumMember()]
        FailedToUpdateCab,
        [EnumMember()]
        FailedToSeeIfCabExists,
        [EnumMember()]
        FailedToGetCab,
        [EnumMember()]
        FailedToLoadCabCollection,
        [EnumMember()]
        FailedToGetCabs,
        [EnumMember()]
        FailedToGetCabCount,
        [EnumMember()]
        FailedToGetCabFileCount,
        [EnumMember()]
        FailedToAddCabNote,
        [EnumMember()]
        FailedToGetCabNotes,
        [EnumMember()]
        FailedToGetUserId,
        [EnumMember()]
        FailedToSeeIfUserExists,
        [EnumMember()]
        FailedToAddUser,
        [EnumMember()]
        FailedToGetSourceId,
        [EnumMember()]
        FailedToSeeIfSourceExists,
        [EnumMember()]
        FailedToAddSource,
        [EnumMember()]
        FailedToGetLocaleSummaryFresh,
        [EnumMember()]
        FailedToGetOperatingSystemSummaryFresh,
        [EnumMember()]
        FailedToGetHitDateSummaryFresh,
        [EnumMember()]
        FailedToSeeIfLocaleSummaryExists,
        [EnumMember()]
        FailedToGetLocaleSummaries,
        [EnumMember()]
        FailedToGetLocaleSummaryForProduct,
        [EnumMember()]
        FailedToAddLocaleSummary,
        [EnumMember()]
        FailedToSeeIfOperatingSystemSummaryExists,
        [EnumMember()]
        FailedToGetOperatingSystemSummaries,
        [EnumMember()]
        FailedToGetOperatingSystemSummaryForProduct,
        [EnumMember()]
        FailedToAddOperatingSystemSummary,
        [EnumMember()]
        FailedToSeeIfHitDateSummaryExists,
        [EnumMember()]
        FailedToGetHitDateSummaries,
        [EnumMember()]
        FailedToGetHitDateSummaryForProduct,
        [EnumMember()]
        FailedToAddHitDateSummary,
        [EnumMember()]
        FailedToReadProduct,
        [EnumMember()]
        FailedToGetDatabaseLogicalFileNames,

        [EnumMember()]
        ClientNotRegistered,  // Client trying to make a call before registering for admin reports.

        [EnumMember()]
        FailedToRunDebugger, // Attempt to run the debugger failed. Check that the debugger paths are pointing to the correct debugger locations - including bitness.

        [EnumMember()]
        FailedToGetUpdateTableEntry,
        [EnumMember()]
        FailedToAddUpdateTableEntry,
        [EnumMember()]
        FailedToRemoveUpdateTableEntry,
        [EnumMember()]
        FailedToGetCabNote,

        [EnumMember()]
        DllNotABugTrackerPlugIn,  // An attempt was made to load a DLL that doesn't have the bug tracker interface.

        [EnumMember()]
        BugTrackerPlugInLoadError, // Plug-in found but was not loadable. See inner exception for more details.
        [EnumMember()]
        BugTrackerPlugInNotFound, // Plug-in not found. Perhaps the internal name is mismatched. Must be identical including case.
        [EnumMember()]
        BugTrackerNoPlugInsFound, // Cannot run the BugReport task on a profile with no plug-ins.
        [EnumMember()]
        BugTrackerPlugInError,      // Call to plugin failed.

        [EnumMember()]
        CannotStopSpecifiedTask, // Can only abort tasks that can be activated by the user directly (e.g. a sync, cab download, error report).


        [EnumMember()]
        FailedToClearUpdateTableEntry,  // Failed to clear the update table of all entries.

        [EnumMember()]
        BugTrackerPlugInReferenceChangeError, // Attempt to load more that one plug-in that sets the bug reference field.
        [EnumMember()]
        BugTrackerPlugInReferenceTooLarge,  // Plug-in returned a reference that is > max of 200 chars.
        [EnumMember()]
        BugTrackerPlugInNotFoundOrCouldNotBeLoaded, // An attempt to load a named plug-in failed because a loaded DLL with the same name could not be found. Check the case of the name and whether the DLL was successfully loaded. 


        [EnumMember()]
        ClientBumped,   // Client no longer registered with the service.

        [EnumMember()]
        FailedToGetTrialLicense,         // Failed to get trial license from the StackHash web site.
        [EnumMember()]
        TrialLicenseDenied,              // Failed to get a trial license.
        [EnumMember()]
        WindowsLiveClientMissing,        // Need Windows Live client to be installed to login to WinQual site.

        [EnumMember()]
        InvalidMappingFileFormat,        // Format of the the specified mapping file is incorrect.

        [EnumMember()]
        FailedToUpdateCabNote,           // Failed to update a cab note.
        [EnumMember()]
        FailedToUpdateEventNote,           // Failed to update a event note.
        [EnumMember()]
        FailedToGetMappings,           // Failed to get mappings data.
        [EnumMember()]
        FailedToAddMappings,           // Failed to add mappings data.

        [EnumMember()]
        FailedToDeleteEventNote,        // Failed to delete an event note.
        [EnumMember()]
        FailedToDeleteCabNote,          // Faield to delete a cab note. 

        // Programmer errors - should be reported to Cucku.
        //
        [EnumMember()]
        InvalidFeedDetected = 0x1000,// Not permitted to access the requested resource.
    }
}
