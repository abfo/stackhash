using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashCabs;
using StackHashDebug;
using StackHashUtilities;

namespace StackHashTasks
{
    public class ScriptResultsManager
    {
        IErrorIndex m_ErrorIndex;
        ScriptManager m_ScriptManager;
        IDebugger m_Debugger;
        StackHashDebuggerSettings m_DebuggerSettings;



        public ScriptResultsManager(IErrorIndex errorIndex, ScriptManager scriptManager, IDebugger debugger, StackHashDebuggerSettings debuggerSettings)
        { 
            m_ErrorIndex = errorIndex;
            m_ScriptManager = scriptManager;
            m_Debugger = debugger;
            m_DebuggerSettings = debuggerSettings;
        }

 
        /// <summary>
        /// Runs a script on a particular cab file.
        /// </summary>
        /// <param name="product">Product data</param>
        /// <param name="file">File data</param>
        /// <param name="theEvent">Event data</param>
        /// <param name="cab">Cab data</param>
        /// <param name="dumpFileName">Name of the dump file or null</param>
        /// <param name="scriptName">Name of script to run on the dump file</param>
        /// <param name="clientData">Client data.</param>
        /// <param name="forceRun">True - forces a run to occur even if results already up to date.</param>
        /// <returns>Full result of running the script</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public StackHashScriptResult RunScript(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, 
            String dumpFileName, String scriptName, bool extractCab, StackHashClientData clientData, bool forceRun)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (scriptName == null)
                throw new ArgumentNullException("scriptName");


            String machineArchitecture = "x86"; // Default to 32 bit.

            if ((cab.DumpAnalysis != null) && !String.IsNullOrEmpty(cab.DumpAnalysis.MachineArchitecture))
                machineArchitecture = cab.DumpAnalysis.MachineArchitecture;


            StackHashScriptResult result = null;
            bool use32BitDebugger = true;

            if (machineArchitecture != null)
            {
                if ((String.Compare(machineArchitecture, "x64", StringComparison.OrdinalIgnoreCase) == 0) ||
                    (String.Compare(machineArchitecture, "X64", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    use32BitDebugger = false;
                }
            }


            // Unwrap the cab if necessary.
            String cabFileName = m_ErrorIndex.GetCabFileName(product, file, theEvent, cab);
            String cabFileFolder = Path.GetDirectoryName(cabFileName);

            if (!File.Exists(cabFileName))
                throw new StackHashException("Cab file does not exist: " + cabFileName, StackHashServiceErrorCode.CabDoesNotExist);

            if (extractCab)
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
                        StackHashCab loadedCab = m_ErrorIndex.GetCab(product, file, theEvent, cab.Id);

                        if (loadedCab != null)
                        {
                            if (loadedCab.CabDownloaded)
                            {
                                loadedCab.CabDownloaded = false;
                                m_ErrorIndex.AddCab(product, file, theEvent, loadedCab, false);
                            }
                        }
                    }

                    throw new StackHashException("Cab file cannot be unpackaged. Try downloading the file from again.", ex, StackHashServiceErrorCode.CabIsCorrupt);
                }
            }

            // Now get the dump filename - mdmp and dmp files should be returned.
            String[] allDumpFiles = Directory.GetFiles(cabFileFolder, "*.*dmp");

            if (allDumpFiles.Length == 0)
                return null;
            
            // Choose the largest dump file to process.
            String fullDumpFilePath = null;
 
            long largestFileSize = 0;
            foreach (String fileName in allDumpFiles)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Length > largestFileSize)
                {
                    largestFileSize = fileInfo.Length;
                    fullDumpFilePath = fileName;
                }
            }

            if (!String.IsNullOrEmpty(dumpFileName))
                fullDumpFilePath = cabFileFolder + "\\" + dumpFileName;

            // Find the script that the user wants to run.
            StackHashScriptSettings scriptSettings = m_ScriptManager.LoadScript(scriptName);

            // Auto generate the script file.
            String dumpAnalysisFolder = cabFileFolder + "\\Analysis";

            if (!Directory.Exists(dumpAnalysisFolder))
                Directory.CreateDirectory(dumpAnalysisFolder);

            // Check if the file has already been run.
            bool runScript = true;
            String resultsFileName = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.log", dumpAnalysisFolder, scriptSettings.Name);
            if (File.Exists(resultsFileName))
            {
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(resultsFileName);

                if (scriptSettings.LastModifiedDate <= lastWriteTime)
                    runScript = false;
            }

            String installFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (runScript || forceRun)
            {
                String overrideSymbolPath = null;
                String overrideBinaryPath = null;
                String overrideSourcePath = null;

                if (m_Debugger.Use32BitDebugger(use32BitDebugger, m_DebuggerSettings))
                {
                    overrideSymbolPath = m_DebuggerSettings.SymbolPath.FullPath;
                    overrideBinaryPath = m_DebuggerSettings.BinaryPath.FullPath;
                    overrideSourcePath = null;

                    // Check the script for PSSCOR loading. 
                    String clrVersion = String.Empty;
                    if ((cab.DumpAnalysis != null) && (!String.IsNullOrEmpty(cab.DumpAnalysis.DotNetVersion)))
                        clrVersion = cab.DumpAnalysis.DotNetVersion;
                    scriptSettings.FixUp(StackHashScriptDumpArchitecture.X86, clrVersion, installFolder);
                }
                else
                {
                    overrideSymbolPath = m_DebuggerSettings.SymbolPath64Bit.FullPath;
                    overrideBinaryPath = m_DebuggerSettings.BinaryPath64Bit.FullPath;
                    overrideSourcePath = null;

                    // Check the script for PSSCOR loading. 
                    String clrVersion = String.Empty;
                    if ((cab.DumpAnalysis != null) && (!String.IsNullOrEmpty(cab.DumpAnalysis.DotNetVersion)))
                        clrVersion = cab.DumpAnalysis.DotNetVersion;
                    scriptSettings.FixUp(StackHashScriptDumpArchitecture.Amd64, clrVersion, installFolder);
                }

                String scriptFileName = Path.GetTempFileName();
                scriptSettings.GenerateScriptFile(scriptFileName, resultsFileName, ref overrideSymbolPath, ref overrideBinaryPath, ref overrideSourcePath);

                DateTime timeOfRun = DateTime.Now.ToUniversalTime();
                try
                {
                    m_Debugger.RunScript(m_DebuggerSettings, use32BitDebugger, scriptFileName, fullDumpFilePath, resultsFileName, overrideSymbolPath, overrideBinaryPath, overrideSourcePath);

                    // Check if the results were generated. If not there must be a command line error so get the output from the 
                    // debugger and throw an exception.
                    if (!File.Exists(resultsFileName))
                        throw new StackHashException("Debugger Error: " + m_Debugger.StandardError, StackHashServiceErrorCode.DebuggerError);


                    // Load in the results.
                    result = StackHashScriptResult.MergeAnalysisDumpFiles(resultsFileName);

                    // Add a script run note to the error index.
                    StackHashNoteEntry note = new StackHashNoteEntry();

                    // MUST KEEP THIS TEXT THE SAME - because it is used as a search string by the BugTrackerTask.
                    note.Note = String.Format(CultureInfo.CurrentCulture, "Script {0} executed", scriptName);
                    if ((clientData == null) || (clientData.ClientName == null))
                    {
                        note.Source = "Service";
                        note.User = "Service";
                    }
                    else
                    {
                        note.Source = "StackHash Client";
                        note.User = clientData.ClientName;
                    }

                    note.TimeOfEntry = timeOfRun;
                    int cabNoteId = m_ErrorIndex.AddCabNote(product, file, theEvent, cab, note);

                    // Report the event to bug tracking plug-ins.
                    m_ErrorIndex.AddUpdate(new StackHashBugTrackerUpdate(
                        StackHashDataChanged.DebugScript, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, cab.Id, cabNoteId));
                }
                catch (System.Exception ex)
                {
                    // Add a script run note to the error index.
                    StackHashNoteEntry note = new StackHashNoteEntry();
                    note.Note = String.Format(CultureInfo.CurrentCulture, "Script {0} execution failed: {1}", scriptName, ex.Message);
                    note.Source = "Service";
                    if ((clientData == null) || (clientData.ClientName == null))
                        note.User = "Service";
                    else
                        note.User = clientData.ClientName;
                    note.TimeOfEntry = timeOfRun;
                    m_ErrorIndex.AddCabNote(product, file, theEvent, cab, note);
                    throw;
                }
                finally
                {
                    if (File.Exists(scriptFileName))
                        File.Delete(scriptFileName);
                }
            }

            return result;
        }

        
        /// <summary>
        /// Gets the results files for the specified cab.
        /// </summary>
        /// <param name="product">Product to which the cab belongs</param>
        /// <param name="file">File to which the cab belongs</param>
        /// <param name="theEvent">Event to which the cab belongs</param>
        /// <param name="cab">Cab to get results for.</param>
        /// <returns>All script results for specified cab.</returns>
        public StackHashScriptResultFiles GetResultFiles(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            // Unwrap the cab if necessary.
            String cabFileFolder = m_ErrorIndex.GetCabFolder(product, file, theEvent, cab);
            cabFileFolder += "\\Analysis";

            // Get a list of the results log files.
            String[] files;
            if (Directory.Exists(cabFileFolder))
                files = Directory.GetFiles(cabFileFolder, "*.log");
            else
                files = new String[0];

            StackHashScriptResultFiles resultFiles = new StackHashScriptResultFiles();

            foreach (String fileName in files)
            {
                // Need to get whether it is an auto or user script.
                String fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);
                StackHashScriptSettings scriptSettings = m_ScriptManager.LoadScript(fileNameNoExtension, true);

                StackHashScriptResultFile resultFile = new StackHashScriptResultFile();
                resultFile.RunDate = File.GetLastWriteTimeUtc(fileName);
                resultFile.ScriptName = Path.GetFileNameWithoutExtension(fileName);

                if ((scriptSettings == null) || (scriptSettings.Owner == StackHashScriptOwner.System))
                    resultFile.UserName = "Auto";
                else
                    resultFile.UserName = "User";

                resultFiles.Add(resultFile);
            }

            return resultFiles;
        }


        /// <summary>
        /// Gets the specified results file for the specified cab.
        /// </summary>
        /// <param name="product">Product to which the cab belongs</param>
        /// <param name="file">File to which the cab belongs</param>
        /// <param name="theEvent">Event to which the cab belongs</param>
        /// <param name="cab">Cab to get results for.</param>
        /// <param name="scriptName">Script whose results are required.</param>
        /// <returns>Specified result file or null if doesn't exist.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public StackHashScriptResult GetResultFileData(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String scriptName)
        {
            if (scriptName == null)
                throw new ArgumentNullException("scriptName");
            if (cab == null)
                throw new ArgumentNullException("cab");

            String cabFileFolder = m_ErrorIndex.GetCabFolder(product, file, theEvent, cab);

            String resultsFileName = String.Format(CultureInfo.InvariantCulture,
                "{0}\\Analysis\\{1}.log", cabFileFolder, scriptName);

            if (!File.Exists(resultsFileName))
                return null;

            StackHashScriptResult result = null;

            try
            {
                result = new StackHashScriptResult(resultsFileName);
            }
            catch (System.Exception ex)
            {
                // Don't allow corrupt files to stop the search.
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Corrupt or missing results file: " + scriptName +
                    " for cab " + cab.Id.ToString(CultureInfo.InvariantCulture), ex);
            }

            return result;
        }


        /// <summary>
        /// Removes the script data for the specified script.
        /// </summary>
        /// <param name="product">Product to which the cab belongs</param>
        /// <param name="file">File to which the cab belongs</param>
        /// <param name="theEvent">Event to which the cab belongs</param>
        /// <param name="cab">Cab to remove the results for.</param>
        /// <param name="scriptName">The script file to remove.</param>
        public void RemoveResultFileData(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String scriptName)
        {
            if (scriptName == null)
                throw new ArgumentNullException("scriptName");

            String cabFileFolder = m_ErrorIndex.GetCabFolder(product, file, theEvent, cab);

            String resultsFileName = String.Format(CultureInfo.InvariantCulture,
                "{0}\\Analysis\\{1}.log", cabFileFolder, scriptName);

            if (File.Exists(resultsFileName))
                File.Delete(resultsFileName);
        }


        ///// <summary>
        ///// Gets the results contained in the specified file.
        ///// </summary>
        ///// <param name="scriptPathAndFileName">File containing the script results.</param>
        ///// <returns>Script results or null.</returns>
        //public StackHashScriptResult GetResultFileData(String scriptPathAndFileName)
        //{
        //    if (scriptPathAndFileName == null)
        //        throw new ArgumentNullException("scriptPathAndFileName");

        //    if (!File.Exists(scriptPathAndFileName))
        //        throw new StackHashException("Script file does not exist: " + scriptPathAndFileName, StackHashServiceErrorCode.ScriptDoesNotExist);

        //    StackHashScriptResult result = new StackHashScriptResult(scriptPathAndFileName);

        //    return result;
        //}


        ///// <summary>
        ///// Search results files according to the specified search criteria.
        ///// </summary>
        ///// <param name="product">Product to which the cab belongs</param>
        ///// <param name="file">File to which the cab belongs</param>
        ///// <param name="theEvent">Event to which the cab belongs</param>
        ///// <param name="cab">Cab to get results for.</param>
        ///// <param name="allSearchCriteria">Options on which to search the result file.</param>
        ///// <returns>True - match, False - no match.</returns>
        //public bool CabMatchesSearchCriteria(StackHashProduct product,
        //    StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashSearchCriteriaCollection allSearchCriteria)
        //{
        //    if (product == null)
        //        throw new ArgumentNullException("product");
        //    if (file == null)
        //        throw new ArgumentNullException("file");
        //    if (theEvent == null)
        //        throw new ArgumentNullException("theEvent");
        //    if (cab == null)
        //        throw new ArgumentNullException("cab");
        //    if (allSearchCriteria == null)
        //        throw new ArgumentNullException("allSearchCriteria");

        //    // Get a list of script result files for this cab.
        //    StackHashScriptResultFiles resultFiles = GetResultFiles(product, file, theEvent, cab);

        //    if ((resultFiles == null) || (resultFiles.Count == 0))
        //        return false;

        //    foreach (StackHashSearchCriteria criteria in allSearchCriteria)
        //    {
        //        foreach (StackHashScriptResultFile resultFile in resultFiles)
        //        {
        //            try
        //            {
        //                StackHashScriptResult resultFileData = GetResultFileData(product, file, theEvent, cab, resultFile.ScriptName);

        //                if (resultFileData.Search(criteria))
        //                    return true;
        //            }
        //            catch (System.Exception ex)
        //            {
        //                // Don't allow corrupt files to stop the search.
        //                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Corrupt or missing results file: " + resultFile.ScriptName + 
        //                    " for cab " + cab.Id.ToString(CultureInfo.InvariantCulture), ex);
        //            }
        //        }
        //    }

        //    return false;
        //}


        /// <summary>
        /// Search results files according to the specified search criteria.
        /// </summary>
        /// <param name="product">Product to which the cab belongs</param>
        /// <param name="file">File to which the cab belongs</param>
        /// <param name="theEvent">Event to which the cab belongs</param>
        /// <param name="cab">Cab to get results for.</param>
        /// <param name="searchCriteria">Options on which to search the result file.</param>
        /// <returns>True - match, False - no match.</returns
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public bool CabMatchesSearchCriteria(StackHashProduct product,
            StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashSearchCriteria searchCriteria)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            // Get a list of script result files for this cab.
            StackHashScriptResultFiles resultFiles = GetResultFiles(product, file, theEvent, cab);

            if ((resultFiles == null) || (resultFiles.Count == 0))
                return false;

            foreach (StackHashScriptResultFile resultFile in resultFiles)
            {
                try
                {
                    StackHashScriptResult resultFileData = GetResultFileData(product, file, theEvent, cab, resultFile.ScriptName);

                    if (resultFileData.Search(searchCriteria))
                        return true;
                }
                catch (System.Exception ex)
                {
                    // Don't allow corrupt files to stop the search.
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Corrupt or missing results file: " + resultFile.ScriptName + 
                        " for cab " + cab.Id.ToString(CultureInfo.InvariantCulture), ex);
                }
            }

            return false;
        }
    }
}
