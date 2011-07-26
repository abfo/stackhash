using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Globalization;

using StackHashDebug;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashCabs;

namespace StackHashTasks
{
    public class DebugScriptTaskParameters : TaskParameters
    {
        private StackHashProduct m_Product;
        private StackHashFile m_File;
        private StackHashEvent m_Event;
        private StackHashCab m_Cab;
        private String m_DumpFileName;
        private StackHashScriptNamesCollection m_ScriptsToRun;
        private ScriptResultsManager m_ScriptResultsManager;
        private ScriptManager m_ScriptManager;
        private IDebugger m_Debugger;

        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }
        public StackHashEvent TheEvent
        {
            get { return m_Event; }
            set { m_Event = value; }
        }
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        public String DumpFileName
        {
            get { return m_DumpFileName; }
            set { m_DumpFileName = value; }
        }

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

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashScriptNamesCollection ScriptsToRun
        {
            get { return m_ScriptsToRun; }
            set { m_ScriptsToRun = value; }
        }

        public IDebugger Debugger
        {
            get { return m_Debugger; }
            set { m_Debugger = value; }
        }
    }


    public class DebugScriptTask : Task
    {
        private DebugScriptTaskParameters m_TaskParameters;

        public DebugScriptTask(DebugScriptTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.DebugScriptTask)
        {
            m_TaskParameters = taskParameters;
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

            // Check if the cab file exists.
            if (!File.Exists(cabFileName))
            {
                // Set the downloaded flag appropriately if different.
                StackHashCab loadedCab = m_TaskParameters.ErrorIndex.GetCab(product, file, theEvent, cab.Id);

                if (loadedCab.CabDownloaded)
                {
                    loadedCab.CabDownloaded = false;
                    m_TaskParameters.ErrorIndex.AddCab(product, file, theEvent, loadedCab, false);
                }

                throw new StackHashException(String.Format(CultureInfo.InvariantCulture, "Cab file does not exist {0}", cabFileName), StackHashServiceErrorCode.CabDoesNotExist);
            }


            // Check if already uncabbed.
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


        /// <summary>
        /// Unpacks and runs the specified script + any automatic scripts on the file.
        /// </summary>
        /// <returns>Full dump analysis.</returns>
        private void processCab()
        {
            ScriptResultsManager scriptRunner = m_TaskParameters.TheScriptResultsManager;
            StackHashProduct product = m_TaskParameters.Product;
            StackHashFile file = m_TaskParameters.File;
            StackHashEvent theEvent = m_TaskParameters.TheEvent;
            StackHashCab cab = m_TaskParameters.Cab;

            // Unwrap the cab if it hasn't been unwrapped already.
            unpackCab(product, file, theEvent, cab, false);

            if (cab.DumpAnalysis == null)
                cab.DumpAnalysis = new StackHashDumpAnalysis();
            

            // Run the autoscripts on the cab.
            Collection<AutoScriptBase> autoScripts = m_TaskParameters.TheScriptManager.AutoScripts;

            bool dataFound = false;
            foreach (AutoScriptBase autoScript in autoScripts)
            {
                bool forceAutoScript = false;
                if ((cab.DumpAnalysis == null) || (cab.DumpAnalysis.MachineArchitecture == null) || (cab.DumpAnalysis.OSVersion == null))
                    forceAutoScript = true;

                // Run the auto script on the cab if it has not already been run.
                StackHashScriptResult scriptResult =
                    scriptRunner.RunScript(m_TaskParameters.Product, m_TaskParameters.File, m_TaskParameters.TheEvent,
                        m_TaskParameters.Cab, null, autoScript.ScriptName, false, null, forceAutoScript);

                // Analyze the results.
                if (scriptResult != null)
                {
                    cab.DumpAnalysis = autoScript.AnalyzeScriptResults(cab.DumpAnalysis, scriptResult);
                    dataFound = true;
                }
            }

            if (dataFound)
                m_TaskParameters.ErrorIndex.AddCab(product, file, theEvent, cab, true);                

            // Now run all the specified scripts.
            foreach (String scriptName in m_TaskParameters.ScriptsToRun)
            {
                if (m_TaskParameters.TheScriptManager.IsAutoScript(scriptName))
                    continue; // Will have already been run above.

                if (CurrentTaskState.AbortRequested)
                    throw new OperationCanceledException("Abort requested");

                scriptRunner.RunScript(m_TaskParameters.Product, m_TaskParameters.File, m_TaskParameters.TheEvent,
                    m_TaskParameters.Cab, m_TaskParameters.DumpFileName, scriptName, false, 
                    m_TaskParameters.ClientData, true);
            }
        }

        
        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// The specified cab file is uncabbed and the specified script is run on the largest extracted dump.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);
                StackHashUtilities.SystemInformation.DisableSleep();

                processCab();
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
            m_TaskParameters.Debugger.AbortRequested = true;
            base.StopExternal();
        }
    }
}
