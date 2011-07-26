using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using StackHashDebug;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashCabs;

namespace StackHashTasks
{
    public class AnalyzeTaskParameters : TaskParameters
    {
        private ScriptResultsManager m_ScriptResultsManager;
        private ScriptManager m_ScriptManager;
        private StackHashDebuggerSettings m_DebuggerSettings;
        private StackHashScriptSettings m_ScriptSettings;
        private IDebugger m_Debugger;
        private StackHashAnalysisSettings m_AnalysisSettings;
        private bool m_ForceUnpack;
        private StackHashProductSyncDataCollection m_ProductsToSynchronize;


        public ScriptResultsManager TheScriptResultsManager
        {
            get { return m_ScriptResultsManager; }
            set { m_ScriptResultsManager = value; }
        }

        public ScriptManager TheScriptManager
        {
            get { return m_ScriptManager; }
            set { m_ScriptManager = value; }
        }
        
        public StackHashDebuggerSettings DebuggerSettings
        {
            get { return m_DebuggerSettings; }
            set { m_DebuggerSettings = value; }
        }

        public StackHashScriptSettings ScriptSettings
        {
            get { return m_ScriptSettings; }
            set { m_ScriptSettings = value; }
        }

        public IDebugger Debugger
        {
            get { return m_Debugger; }
            set { m_Debugger = value; }
        }

        public bool ForceUnpack
        {
            get { return m_ForceUnpack; }
            set { m_ForceUnpack = value; }
        }

        public StackHashAnalysisSettings AnalysisSettings
        {
            get { return m_AnalysisSettings; }
            set { m_AnalysisSettings = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get { return m_ProductsToSynchronize; }
            set { m_ProductsToSynchronize = value; }
        }
    }


    /// <summary>
    /// The analyze task parses all CAB files in the index and runs automatic debugger scripts on them.
    /// </summary>
    public class AnalyzeTask : Task
    {
        private AnalyzeTaskParameters m_TaskParameters;
        private List<StackHashScriptSettings> m_AutomaticUserScriptNames;
        private int m_ConsecutiveErrors = 0;
        private int m_MaxConsecutiveErrors = 0;

        public AnalyzeTask(AnalyzeTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.AnalyzeTask)
        {
            m_TaskParameters = taskParameters;

            m_MaxConsecutiveErrors = AppSettings.ConsecutiveAnalyzeCabErrorsBeforeAbort;
        }

        /// <summary>
        /// Determines if the specified product should be searched for cabs to process.
        /// </summary>
        /// <param name="product">The product to check.</param>
        /// <returns>True - process the product, false - don't process.</returns>
        private bool shouldProcessProduct(StackHashProduct product)
        {
            if (m_TaskParameters.ProductsToSynchronize == null)
                return false;

            // If no syncData available then the user is not interested in synchronizing thus product.
            StackHashProductSyncData syncData = m_TaskParameters.ProductsToSynchronize.FindProduct(product.Id);

            return (syncData != null);
        }


        /// <summary>
        /// Determines if the specified file should be searched for cabs to process.
        /// </summary>
        /// <param name="product">The product to check.</param>
        /// <param name="file">The file to check.</param>
        /// <returns>True - process the file, false - don't process.</returns>
        private bool shouldProcessFile(StackHashProduct product, StackHashFile file)
        {
            if (product == null)
                return false;
            if (file == null)
                return false;

            return true;
        }


        /// <summary>
        /// Determines if the specified event should be searched for cabs to process.
        /// </summary>
        /// <param name="product">The product to check.</param>
        /// <param name="file">The file to check.</param>
        /// <param name="theEvent">The event to check.</param>
        /// <returns>True - process the event, false - don't process.</returns>
        private bool shouldProcessEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (product == null)
                return false;
            if (file == null)
                return false;
            if (theEvent == null)
                return false;

            return true;
        }


        /// <summary>
        /// Determines if the specified cab should be processed.
        /// </summary>
        /// <param name="product">The product to check.</param>
        /// <param name="file">The file to check.</param>
        /// <param name="theEvent">The event to check.</param>
        /// <param name="cab">The product to check.</param>
        /// <returns>True - process the cab, false - don't process.</returns>
        private bool shouldProcessCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (product == null)
                return false;
            if (file == null)
                return false;
            if (theEvent == null)
                return false;
            if (cab == null)
                return false;

            return true;
        }


        /// <summary>
        /// Unpack the cab file if not already unpacked.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to which the cab belongs.</param>
        /// <param name="theEvent">Event to which the cab belongs.</param>
        /// <param name="cab">The cab object to unpack.</param>
        /// <param name="forceUnpack">True - force an unpack even if the cab has already been unpacked.</param>
        /// <returns></returns>
        private void unpackCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool forceUnpack)
        {
            String cabFileName = m_TaskParameters.ErrorIndex.GetCabFileName(product, file, theEvent, cab);
            String cabFileFolder = Path.GetDirectoryName(cabFileName);

            if (!Cabs.IsUncabbed(cabFileName, cabFileFolder) || forceUnpack)
            {
                try
                {
                    Cabs.ExtractCab(cabFileName, cabFileFolder);
                }
                catch (System.Exception ex)
                {
                    if (ex.Message.Contains("The file is not a cabinet") || ex.Message.Contains("corrupt") || ex.Message.Contains("Corrupt"))
                    {
                        // Set the downloaded flag appropriately if different.
                        StackHashCab loadedCab = m_TaskParameters.ErrorIndex.GetCab(product, file, theEvent, cab.Id);

                        if (loadedCab != null)
                        {
                            if (loadedCab.CabDownloaded)
                            {
                                loadedCab.CabDownloaded = false;
                                m_TaskParameters.ErrorIndex.AddCab(product, file, theEvent, loadedCab, false);
                            }
                        }
                    }

                    throw new StackHashException("Cab file cannot be unpackaged. Try downloading the file from again.", ex, StackHashServiceErrorCode.CabIsCorrupt);
                }
            }
        }

        private bool runAutoScripts(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashScriptDumpType dumpType)
        {
            Collection<AutoScriptBase> autoScripts = m_TaskParameters.TheScriptManager.AutoScripts;
            ScriptResultsManager scriptRunner = m_TaskParameters.TheScriptResultsManager;

            bool dumpAnalysisProduced = false;
            // Analyse the Unmanaged mode autoscripts first - as these determine if the dump is managed or not and hence whether
            // the managed scripts will be run. The AutoScripts property above returns the autoscripts in the correct order.
            foreach (AutoScriptBase autoScript in autoScripts)
            {
                bool dumpIsManaged = ((cab.DumpAnalysis != null) && (!String.IsNullOrEmpty(cab.DumpAnalysis.DotNetVersion)));

                if (autoScript.ScriptSettings.DumpType != dumpType)
                    continue;

                if ((autoScript.ScriptSettings.DumpType == StackHashScriptDumpType.ManagedOnly) && (!dumpIsManaged))
                    continue;

                bool forceAutoScript = false;
                if ((cab.DumpAnalysis == null) || (cab.DumpAnalysis.MachineArchitecture == null) || (cab.DumpAnalysis.OSVersion == null))
                    forceAutoScript = true;

                // Run the auto script on the cab if it hasn't already been run.
                StackHashScriptResult scriptResult =
                    scriptRunner.RunScript(product, file, theEvent, cab, null, autoScript.ScriptName, false, null, forceAutoScript);

                // Analyze the results.
                if (scriptResult != null)
                {
                    cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, scriptResult);
                    dumpAnalysisProduced = true;
                    m_ConsecutiveErrors = 0;
                }
            }

            return dumpAnalysisProduced;
        }



        /// <summary>
        /// Unpacks and runs the autoscript and runs automatic scripts on the file.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to which the cab belongs.</param>
        /// <param name="theEvent">Event to which the cab belongs.</param>
        /// <param name="cab">The cab object to process.</param>
        /// <returns>Full dump analysis.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private StackHashDumpAnalysis processCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            try
            {
                ScriptResultsManager scriptRunner = m_TaskParameters.TheScriptResultsManager;
                String cabFileName = m_TaskParameters.ErrorIndex.GetCabFileName(product, file, theEvent, cab);

                // It is legitimate for a cab entry to exist in the database but no cab file to exist.
                if (!File.Exists(cabFileName))
                {
                    // Make sure the cab is marked as not downloaded.
                    // Set the downloaded flag appropriately if different.
                    StackHashCab loadedCab = m_TaskParameters.ErrorIndex.GetCab(product, file, theEvent, cab.Id);

                    if (loadedCab != null)
                    {
                        if (loadedCab.CabDownloaded)
                        {
                            loadedCab.CabDownloaded = false;
                            m_TaskParameters.ErrorIndex.AddCab(product, file, theEvent, loadedCab, false);
                        }
                    }

                    return null;
                }

                // Unwrap the cab if it hasn't been unwrapped already.
                unpackCab(product, file, theEvent, cab, m_TaskParameters.ForceUnpack);

                if (cab.DumpAnalysis == null)
                    cab.DumpAnalysis = new StackHashDumpAnalysis();


                // Process the auto scripts first.
                Collection<AutoScriptBase> autoScripts = m_TaskParameters.TheScriptManager.AutoScripts;

                bool dumpAnalysisProduced = false;

                if (runAutoScripts(product, file, theEvent, cab, StackHashScriptDumpType.UnmanagedOnly))
                    dumpAnalysisProduced = true;
                if (runAutoScripts(product, file, theEvent, cab, StackHashScriptDumpType.UnmanagedAndManaged))
                    dumpAnalysisProduced = true;
                if (runAutoScripts(product, file, theEvent, cab, StackHashScriptDumpType.ManagedOnly))
                    dumpAnalysisProduced = true;
                
                // Should know by now if the dump is managed or not.
                bool dumpIsManaged = ((cab.DumpAnalysis != null) && (!String.IsNullOrEmpty(cab.DumpAnalysis.DotNetVersion)));
                
                if (dumpAnalysisProduced)
                    m_TaskParameters.ErrorIndex.AddCab(product, file, theEvent, cab, true);

                // Run all user based scripts marked as automatic.
                foreach (StackHashScriptSettings script in m_AutomaticUserScriptNames)
                {
                    if ((script.DumpType == StackHashScriptDumpType.ManagedOnly) && (!dumpIsManaged)) 
                        continue;
                    if ((script.DumpType == StackHashScriptDumpType.UnmanagedOnly) && (dumpIsManaged)) 
                        continue;

                    // Run the script.
                    scriptRunner.RunScript(product, file, theEvent, cab, null, script.Name, false, null, false);
                }

                return cab.DumpAnalysis;
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to process CAB file", ex);
                m_ConsecutiveErrors++;

                if (m_ConsecutiveErrors >= m_MaxConsecutiveErrors)
                    throw;

                return null;
            }
        }


        /// <summary>
        /// Parses the database looking for cabs to unwrap. 
        /// </summary>
        private void analyzeAllCabs()
        {
            IErrorIndex index = m_TaskParameters.ErrorIndex;

            // Get a list of products.
            StackHashProductCollection products = index.LoadProductList();

            foreach (StackHashProduct product in products)
            {
                if (CurrentTaskState.AbortRequested)
                    throw new OperationCanceledException("Task aborted");

                if (!shouldProcessProduct(product))
                    continue;

                // Get the file list.
                StackHashFileCollection files = index.LoadFileList(product);

                foreach (StackHashFile file in files)
                {
                    if (CurrentTaskState.AbortRequested)
                        throw new OperationCanceledException("Task aborted");

                    if (!shouldProcessFile(product, file))
                        continue;

                    // Get the event data.
                    StackHashEventCollection events = index.LoadEventList(product, file);

                    foreach (StackHashEvent theEvent in events)
                    {
                        if (CurrentTaskState.AbortRequested)
                            throw new OperationCanceledException("Task aborted");

                        if (!shouldProcessEvent(product, file, theEvent))
                            continue;

                        StackHashCabCollection cabs = index.LoadCabList(product, file, theEvent);

                        // Process each cab.
                        foreach (StackHashCab cab in cabs)
                        {
                            if (CurrentTaskState.AbortRequested)
                                throw new OperationCanceledException("Task aborted");

                            if (!shouldProcessCab(product, file, theEvent, cab))
                                continue;

                            processCab(product, file, theEvent, cab);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the automated scripts defined by the user.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private void getAutomatedScripts()
        {
            m_AutomaticUserScriptNames = new List<StackHashScriptSettings>();

            // Now run all the scripts marked as automatic.
            StackHashScriptFileDataCollection scripts = m_TaskParameters.TheScriptManager.ScriptNames;

            foreach (StackHashScriptFileData scriptData in scripts)
            {
                StackHashScriptSettings script;

                try
                {
                    // Get the script details to see if it is automatic.
                    script = m_TaskParameters.TheScriptManager.LoadScript(scriptData.Name);

                    if ((script.RunAutomatically) && (script.Owner == StackHashScriptOwner.User))
                    {
                        m_AutomaticUserScriptNames.Add(script);
                    }
                }
                catch (System.Exception ex)
                {
                    // Ignore invalid script files.
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Invalid script file found: " + scriptData.Name, ex);
                }
            }
        }


        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// Steps through all products, files and events in the database looking for cab
        /// files that have yet to be processed in some way.
        /// Processing includes...
        /// 1) Uncabbing the cab file.
        /// 2) Running the autoscript file on the dump files and analyzing the results which
        ///    are added to the Cab object.
        /// 3) Running user defined scripts on the cabs as instructed.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);
                StackHashUtilities.SystemInformation.DisableSleep();

                // Make sure that the user has selected a debugger, otherwise just bomb.
                if (!(((m_TaskParameters.DebuggerSettings.DebuggerPathAndFileName != null) &&
                    File.Exists(m_TaskParameters.DebuggerSettings.DebuggerPathAndFileName)) ||
                    ((m_TaskParameters.DebuggerSettings.DebuggerPathAndFileName64Bit != null) &&
                    File.Exists(m_TaskParameters.DebuggerSettings.DebuggerPathAndFileName64Bit))))
                {
                    // No debugger specified or present.
                    throw new StackHashException("No debugger has been specified", StackHashServiceErrorCode.DebuggerNotConfigured);
                }

                // Get automatic script names.
                getAutomatedScripts();
                
                // Parse the database looking for new cabs to analyze.
                analyzeAllCabs();                
            }
            catch (System.Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                StackHashUtilities.SystemInformation.EnableSleep();
                SetTaskCompleted(m_TaskParameters.ErrorIndex);
            }
        }

        /// <summary>
        /// Abort the current task.
        /// </summary>
        public override void StopExternal()
        {
            WritableTaskState.Aborted = true; 
            m_TaskParameters.Debugger.AbortRequested = true;
            base.StopExternal();
        }
    }
}

