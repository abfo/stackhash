using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace StackHash
{
    /// <summary>
    /// StackHash specific commands
    /// </summary>
    public static class StackHashCommands
    {
        /// <summary>
        /// Exit the application
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExitCommand;

        /// <summary>
        /// Synchronize with WinQual
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand SyncCommand;

        /// <summary>
        /// Resynchronize with WinQual
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resync")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ResyncCommand;

        /// <summary>
        /// Synchronize a single product with WinQual
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand SyncProductCommand;

        /// <summary>
        /// Resynchronize a single product with WinQual
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resync")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ResyncProductCommand;

        /// <summary>
        /// Cancel current WinQual synchronization
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand CancelSyncCommand;

        /// <summary>
        /// Open the Profile Manager
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ProfileManagerCommand;

        /// <summary>
        /// Open the Script Manager
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ScriptManagerCommand;

        /// <summary>
        /// Search
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand SearchCommand;

        /// <summary>
        /// Clear current search
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ClearSearchCommand;

        /// <summary>
        /// Build a search
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand BuildSearchCommand;

        /// <summary>
        /// About StackHash
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand AboutCommand;

        /// <summary>
        /// Start debugging
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand DebugCommand;

        /// <summary>
        /// Local client options
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand OptionsCommand;

        /// <summary>
        /// Shows the current service status
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ServiceStatusCommand;

        /// <summary>
        /// Show disabled products
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ShowDisabledProductsCommand;

        /// <summary>
        /// Debug using Visual Studio
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand DebugVisualStudioCommand;

        /// <summary>
        /// Debug using 32-bit debugging tools for windows
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand DebugX86Command;

        /// <summary>
        /// Debug using 64-bit debugging tools for windows
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand DebugX64Command;

        /// <summary>
        /// Refresh the current view
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand RefreshCommand;

        /// <summary>
        /// Extract the current cab to a directory of the user's choice
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExtractCabCommand;

        /// <summary>
        /// Extract all cabs for an event to a directory of the user's choice
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExtractAllCabsCommand;

        /// <summary>
        /// Parent command for running debug scripts
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand RunScriptCommand;

        /// <summary>
        /// Run a specific script by name, used in the generated context menu
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand RunScriptByNameCommand;

        /// <summary>
        /// Parent command for debugging with a specific debugger
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand DebugUsingCommand;

        /// <summary>
        /// Parent command for exporting 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExportCommand;

        /// <summary>
        /// Export the current product list
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExportProductListCommand;

        /// <summary>
        /// Export the current event list
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExportEventListCommand;

        /// <summary>
        /// Export the current event list, cabs and eventinfos
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ExportEventListFullCommand;

        /// <summary>
        /// Downloads the currently selected cab
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand DownloadCabCommand;

        /// <summary>
        /// Shows the result of the last sync attempt
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand SyncReportCommand;

        /// <summary>
        /// Send all profile data to selected plugins
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugins")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand SendAllToPluginsCommand;

        /// <summary>
        /// Upload a product mapping file to WinQual
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand UploadMappingCommand;

        /// <summary>
        /// Open the folder containing a cab file (if client is local to the service)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand OpenCabFolderCommand;

        /// <summary>
        /// Show events without associated cabs
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedUICommand ShowEventsWithoutCabsCommand;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static StackHashCommands()
        {
            // Exit command
            InputGestureCollection exitInputs = new InputGestureCollection();
            exitInputs.Add(new KeyGesture(Key.F4, ModifierKeys.Alt));
            ExitCommand = new RoutedUICommand(Properties.Resources.Command_Exit, "Exit", typeof(StackHashCommands), exitInputs);

            SyncCommand = new RoutedUICommand(Properties.Resources.Command_Sync, "Sync", typeof(StackHashCommands));

            SyncProductCommand = new RoutedUICommand(Properties.Resources.Command_SyncProduct, "SyncProduct", typeof(StackHashCommands));

            CancelSyncCommand = new RoutedUICommand(Properties.Resources.Command_CancelSync, "CancelSync", typeof(StackHashCommands));

            ProfileManagerCommand = new RoutedUICommand(Properties.Resources.Command_ProfileManager, "ProfileManager", typeof(StackHashCommands));

            SearchCommand = new RoutedUICommand(Properties.Resources.Command_Search, "Search", typeof(StackHashCommands));

            ClearSearchCommand = new RoutedUICommand(Properties.Resources.Command_ClearSearch, "ClearSearch", typeof(StackHashCommands));

            BuildSearchCommand = new RoutedUICommand(Properties.Resources.Command_BuildSearch, "BuildSearch", typeof(StackHashCommands));

            ResyncCommand = new RoutedUICommand(Properties.Resources.Command_Resync, "Resync", typeof(StackHashCommands));

            ResyncProductCommand = new RoutedUICommand(Properties.Resources.Command_ResyncProduct, "ResyncProduct", typeof(StackHashCommands));

            ScriptManagerCommand = new RoutedUICommand(Properties.Resources.Command_ScriptManager, "ScriptManager", typeof(StackHashCommands));

            AboutCommand = new RoutedUICommand(Properties.Resources.Command_About, "About", typeof(StackHashCommands));

            InputGestureCollection debugInputs = new InputGestureCollection();
            debugInputs.Add(new KeyGesture(Key.F5, ModifierKeys.None));
            DebugCommand = new RoutedUICommand(Properties.Resources.Command_Debug, "Debug", typeof(StackHashCommands), debugInputs);

            OptionsCommand = new RoutedUICommand(Properties.Resources.Command_Options, "Options", typeof(StackHashCommands));

            ServiceStatusCommand = new RoutedUICommand(Properties.Resources.Command_ServiceStatus, "ServiceStatus", typeof(StackHashCommands));

            ShowDisabledProductsCommand = new RoutedUICommand(Properties.Resources.Command_ShowDisabledProducts, "ShowDisabledProducts", typeof(StackHashCommands));

            DebugVisualStudioCommand = new RoutedUICommand(Properties.Resources.Command_DebugVisualStudio, "DebugVisualStudio", typeof(StackHashCommands));

            DebugX86Command = new RoutedUICommand(Properties.Resources.Command_DebugX86, "DebugX86", typeof(StackHashCommands));

            DebugX64Command = new RoutedUICommand(Properties.Resources.Command_DebugX64, "DebugX64", typeof(StackHashCommands));

            InputGestureCollection refreshInputs = new InputGestureCollection();
            refreshInputs.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            RefreshCommand = new RoutedUICommand(Properties.Resources.Command_Refresh, "Refresh", typeof(StackHashCommands), refreshInputs);

            ExtractCabCommand = new RoutedUICommand(Properties.Resources.Command_ExtractCab, "ExtractCab", typeof(StackHashCommands));

            ExtractAllCabsCommand = new RoutedUICommand(Properties.Resources.Command_ExtractAllCabs, "ExtractAllCabs", typeof(StackHashCommands));

            RunScriptCommand = new RoutedUICommand(Properties.Resources.Command_RunScript, "RunScript", typeof(StackHashCommands));

            RunScriptByNameCommand = new RoutedUICommand(Properties.Resources.Command_RunScriptByName, "RunScriptByName", typeof(StackHashCommands));

            DebugUsingCommand = new RoutedUICommand(Properties.Resources.Command_DebugUsing, "DebugUsing", typeof(StackHashCommands));

            ExportCommand = new RoutedUICommand(Properties.Resources.Command_Export, "Export", typeof(StackHashCommands));

            ExportProductListCommand = new RoutedUICommand(Properties.Resources.Command_ExportProductList, "ExportProductList", typeof(StackHashCommands));

            ExportEventListCommand = new RoutedUICommand(Properties.Resources.Command_ExportEventList, "ExportEventList", typeof(StackHashCommands));

            ExportEventListFullCommand = new RoutedUICommand(Properties.Resources.Command_ExportEventListFull, "ExportEventListFull", typeof(StackHashCommands));

            DownloadCabCommand = new RoutedUICommand(Properties.Resources.Command_DownloadCab, "DownloadCab", typeof(StackHashCommands));

            SyncReportCommand = new RoutedUICommand(Properties.Resources.Command_SyncReport, "SyncReport", typeof(StackHashCommands));

            SendAllToPluginsCommand = new RoutedUICommand(Properties.Resources.Command_SendAllToPlugins, "SendAllToPlugins", typeof(StackHashCommands));

            UploadMappingCommand = new RoutedUICommand(Properties.Resources.Command_UploadMappingFile, "UploadMapping", typeof(StackHashCommands));

            OpenCabFolderCommand = new RoutedUICommand(Properties.Resources.Command_OpenCabFolder, "OpenCabFolder", typeof(StackHashCommands));

            ShowEventsWithoutCabsCommand = new RoutedUICommand(Properties.Resources.Command_ShowEventsWithoutCabs, "ShowEventsWithoutCabs", typeof(StackHashCommands));
        }
    }
}
