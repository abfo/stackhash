using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Specialized;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashBugTrackerInterfaceV1;

namespace StackHashTasks
{
    public class BugTrackerTaskParameters : TaskParameters
    {
        private BugTrackerContext m_BugTrackerContext;
        private BugTrackerManager m_BugTrackerManager;

        private ScriptResultsManager m_ScriptResultsManager;

        public BugTrackerContext PlugInContext
        {
            get { return m_BugTrackerContext; }
            set { m_BugTrackerContext = value; }
        }

        public BugTrackerManager PlugInManager
        {
            get { return m_BugTrackerManager; }
            set { m_BugTrackerManager = value; }
        }

        
        public ScriptResultsManager TheScriptResultsManager
        {
            get { return m_ScriptResultsManager; }
            set { m_ScriptResultsManager = value; }
        }
    }


    public class BugTrackerTask : Task
    {
        private BugTrackerTaskParameters m_TaskParameters;
        private IErrorIndex m_Index = null;

        private static int s_AbortEventIndex = 0;
        private static int s_UpdateArrived = 1;
        private EventWaitHandle[] m_Events;


        /// <summary>
        /// Called to indicate that an update has been made to an event in the database.
        /// Update entries are added to the Update table in the database. This task then 
        /// removes those entries and informs the bug tracking plugins of the update.
        /// </summary>
        private void UpdateArrived()
        {
            m_Events[s_UpdateArrived].Set();
        }

        /// <summary>
        /// Called to prod the bug tracker task to check the Update Table for outstanding entries to process.
        /// This might be necessary to "restart" the task processing after it has stopped due to an exception 
        /// calling the bug tracker plugins.
        /// </summary>
        public void CheckForUpdateEntries()
        {
            m_Events[s_UpdateArrived].Set();
        }

        /// <summary>
        /// Changes to the error index are recorded in the Update Table. This task removes those updates and 
        /// adds invokes the bug tracking system for each change. The entry is then removed from the Update Table.
        /// </summary>
        /// <param name="taskParameters">Configuration information for the task.</param>
        public BugTrackerTask(BugTrackerTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.BugTrackerTask)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");

            m_TaskParameters = taskParameters;
            m_Index = m_TaskParameters.ErrorIndex;

            m_Events = new EventWaitHandle[] 
            { 
                new ManualResetEvent(false),  // Abort event. 
                new AutoResetEvent(false)     // Update event.
            };
        }

        private StackHashEvent setPlugInBugReference(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, String newBugId)
        {
            if (newBugId != null)
            {
                if (theEvent.PlugInBugId != null)
                {
                    if (String.Compare(newBugId, theEvent.PlugInBugId, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        theEvent.PlugInBugId = newBugId;

                        // Update the event - but don't report the change to bug tracking S/W.
                        m_Index.AddEvent(product, file, theEvent, true, false); // BugId is a non-winqual field.  
                    }
                    else
                    {
                        theEvent.PlugInBugId = newBugId;
                    }
                }
                else
                {
                    // No bug Id yet.
                    theEvent.PlugInBugId = newBugId;

                    // Update the event - but don't report the change to bug tracking S/W.
                    m_Index.AddEvent(product, file, theEvent, true, false); // BugId is a non-winqual field.
                }
            }
            return theEvent;        
        }

        /// <summary>
        /// Process a product table update. This may indicate a new item or the change to an existing item.
        /// </summary>
        private bool processProductUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            if (product == null)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Product: Inconsistent Update Table Entry");
                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);

            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                m_TaskParameters.PlugInContext.ProductAdded(null, BugTrackerReportType.Automatic, btProduct);
            else
                m_TaskParameters.PlugInContext.ProductUpdated(null, BugTrackerReportType.Automatic, btProduct);

            return true;
        }


        /// <summary>
        /// Process a file table update. This may indicate a new item or the change to an existing item.
        /// </summary>
        private bool processFileUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product and file information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            StackHashFile file = m_Index.GetFile(product, (int)update.FileId);

            if (product == null)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing File: Inconsistent Update Table Entry = product not found: " + update.ProductId.ToString(CultureInfo.InvariantCulture));
                return false ;
            }
            if (file == null)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing File: Inconsistent Update Table Entry = file not found: " + update.FileId.ToString(CultureInfo.InvariantCulture));

                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);

            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                m_TaskParameters.PlugInContext.FileAdded(null, BugTrackerReportType.Automatic, btProduct, btFile);
            else
                m_TaskParameters.PlugInContext.FileUpdated(null, BugTrackerReportType.Automatic, btProduct, btFile);

            return true;
        }


        /// <summary>
        /// Process an event table update. This may indicate a new item or the change to an existing item.
        /// </summary>
        private bool processEventUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product and file information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            StackHashFile file = m_Index.GetFile(product, (int)update.FileId);
            StackHashEvent theEvent = m_Index.GetEvent(product, file, new StackHashEvent((int)update.EventId, update.EventTypeName));

            if ((product == null) || (file == null) || (theEvent == null))
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Event: Inconsistent Update Table Entry");
                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);

            NameValueCollection eventSignature = new NameValueCollection();

            foreach (StackHashParameter param in theEvent.EventSignature.Parameters)
            {
                eventSignature.Add(param.Name, param.Value);
            }

            BugTrackerEvent btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, 
                theEvent.TotalHits, eventSignature);

            String newBugId = null;

            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                newBugId = m_TaskParameters.PlugInContext.EventAdded(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent);
            else
                newBugId = m_TaskParameters.PlugInContext.EventUpdated(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent);

            setPlugInBugReference(product, file, theEvent, newBugId);
            return true;
        }


        /// <summary>
        /// Process a cab table update. This may indicate a new item or the change to an existing item.
        /// </summary>
        private bool processCabUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product and file information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            StackHashFile file = m_Index.GetFile(product, (int)update.FileId);
            StackHashEvent theEvent = m_Index.GetEvent(product, file, new StackHashEvent((int)update.EventId, update.EventTypeName));
            StackHashCab cab = m_Index.GetCab(product, file, theEvent, (int)update.CabId);

            if ((product == null) || (file == null) || (theEvent == null) || (cab == null))
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Cab: Inconsistent Update Table Entry");
                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);
            NameValueCollection eventSignature = new NameValueCollection();

            foreach (StackHashParameter param in theEvent.EventSignature.Parameters)
            {
                eventSignature.Add(param.Name, param.Value);
            }

            BugTrackerEvent btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);

            NameValueCollection analysis = new NameValueCollection();
            analysis.Add("DotNetVersion", cab.DumpAnalysis.DotNetVersion);
            analysis.Add("MachineArchitecture", cab.DumpAnalysis.MachineArchitecture);
            analysis.Add("OSVersion", cab.DumpAnalysis.OSVersion);
            analysis.Add("ProcessUpTime", cab.DumpAnalysis.ProcessUpTime);
            analysis.Add("SystemUpTime", cab.DumpAnalysis.SystemUpTime);

            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);
            BugTrackerCab btCab = new BugTrackerCab(cab.Id, cab.SizeInBytes, cab.CabDownloaded, cab.Purged, analysis, cabFileName);

            String newBugId = null;
            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                newBugId = m_TaskParameters.PlugInContext.CabAdded(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent, btCab);
            else
                newBugId = m_TaskParameters.PlugInContext.CabUpdated(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent, btCab);

            setPlugInBugReference(product, file, theEvent, newBugId);

            return true;
        }


        /// <summary>
        /// Process a hit table update. This may indicate a new item or the change to an existing item.
        /// Not reported at present.
        /// </summary>
        private bool processHitUpdate(StackHashBugTrackerUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException("update");
            return true;
        }

        /// <summary>
        /// Process a cab note table update. This may indicate a new item or the change to an existing item.
        /// </summary>
        private bool processCabNoteUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product and file information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            StackHashFile file = m_Index.GetFile(product, (int)update.FileId);
            StackHashEvent theEvent = m_Index.GetEvent(product, file, new StackHashEvent((int)update.EventId, update.EventTypeName));
            StackHashCab cab = m_Index.GetCab(product, file, theEvent, (int)update.CabId);
            StackHashNoteEntry note = m_Index.GetCabNote((int)update.ChangedObjectId);

            if ((product == null) || (file == null) || (theEvent == null) || (cab == null) || (note == null))
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Cab Note: Inconsistent Update Table Entry");
                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);
            NameValueCollection eventSignature = new NameValueCollection();

            foreach (StackHashParameter param in theEvent.EventSignature.Parameters)
            {
                eventSignature.Add(param.Name, param.Value);
            }

            BugTrackerEvent btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, 
                theEvent.TotalHits, eventSignature);

            NameValueCollection analysis = new NameValueCollection();
            analysis.Add("DotNetVersion", cab.DumpAnalysis.DotNetVersion);
            analysis.Add("MachineArchitecture", cab.DumpAnalysis.MachineArchitecture);
            analysis.Add("OSVersion", cab.DumpAnalysis.OSVersion);
            analysis.Add("ProcessUpTime", cab.DumpAnalysis.ProcessUpTime);
            analysis.Add("SystemUpTime", cab.DumpAnalysis.SystemUpTime);

            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);
            BugTrackerCab btCab = new BugTrackerCab(cab.Id, cab.SizeInBytes, cab.CabDownloaded, cab.Purged, analysis, cabFileName);
            
            BugTrackerNote btCabNote = new BugTrackerNote(note.TimeOfEntry, note.Source, note.User, note.Note);

            String newBugId = null;
            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                newBugId = m_TaskParameters.PlugInContext.CabNoteAdded(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent, btCab, btCabNote);

            setPlugInBugReference(product, file, theEvent, newBugId);

            return true;
        }


        /// <summary>
        /// Process an event note table update. This may indicate a new item or the change to an existing item.
        /// </summary>
        private bool processEventNoteUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product and file information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            StackHashFile file = m_Index.GetFile(product, (int)update.FileId);
            StackHashEvent theEvent = m_Index.GetEvent(product, file, new StackHashEvent((int)update.EventId, update.EventTypeName));
            StackHashNoteEntry note = m_Index.GetEventNote((int)update.ChangedObjectId);

            if ((product == null) || (file == null) || (theEvent == null) || (note == null))
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Event Note: Inconsistent Update Table Entry");
                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);
            NameValueCollection eventSignature = new NameValueCollection();

            foreach (StackHashParameter param in theEvent.EventSignature.Parameters)
            {
                eventSignature.Add(param.Name, param.Value);
            }

            BugTrackerEvent btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, 
                theEvent.TotalHits, eventSignature);


            BugTrackerNote btEventNote = new BugTrackerNote(note.TimeOfEntry, note.Source, note.User, note.Note);

            String newBugId = null;
            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                newBugId = m_TaskParameters.PlugInContext.EventNoteAdded(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent, btEventNote);

            setPlugInBugReference(product, file, theEvent, newBugId);

            return true;
        }


        /// <summary>
        /// Process a debug script change.
        /// </summary>
        private bool processDebugScriptUpdate(StackHashBugTrackerUpdate update)
        {
            // Get the associated product and file information.
            StackHashProduct product = m_Index.GetProduct((int)update.ProductId);
            StackHashFile file = m_Index.GetFile(product, (int)update.FileId);
            StackHashEvent theEvent = m_Index.GetEvent(product, file, new StackHashEvent((int)update.EventId, update.EventTypeName));

            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: getting cab data1");
            StackHashCab cab = m_Index.GetCab(product, file, theEvent, (int)update.CabId);
            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: getting cab data2");
            StackHashNoteEntry note = m_Index.GetCabNote((int)update.ChangedObjectId);

            if ((product == null) || (file == null) || (theEvent == null) || (cab == null) || (note == null))
            {
                if (product == null)
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: Inconsistent Update Table Entry : product");
                if (file == null)
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: Inconsistent Update Table Entry : file");
                if (theEvent == null)
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: Inconsistent Update Table Entry : the Event");
                if (cab == null)
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: Inconsistent Update Table Entry : cab: " + update.CabId.ToString(CultureInfo.InvariantCulture));
                if (note == null)
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Processing Debug Script: Inconsistent Update Table Entry : note");
                return false;
            }

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);
            NameValueCollection eventSignature = new NameValueCollection();

            foreach (StackHashParameter param in theEvent.EventSignature.Parameters)
            {
                eventSignature.Add(param.Name, param.Value);
            }

            BugTrackerEvent btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, 
                theEvent.TotalHits, eventSignature);

            NameValueCollection analysis = new NameValueCollection();
            analysis.Add("DotNetVersion", cab.DumpAnalysis.DotNetVersion);
            analysis.Add("MachineArchitecture", cab.DumpAnalysis.MachineArchitecture);
            analysis.Add("OSVersion", cab.DumpAnalysis.OSVersion);
            analysis.Add("ProcessUpTime", cab.DumpAnalysis.ProcessUpTime);
            analysis.Add("SystemUpTime", cab.DumpAnalysis.SystemUpTime);

            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);
            BugTrackerCab btCab = new BugTrackerCab(cab.Id, cab.SizeInBytes, cab.CabDownloaded, cab.Purged, analysis, cabFileName);

            BugTrackerNote btCabNote = new BugTrackerNote(note.TimeOfEntry, note.Source, note.User, note.Note);

            // A note entry will be written when a script is run. Pick out the name of the script.
            // Format is "Script {0} executed".
            int startIndex = btCabNote.Note.IndexOf("Script ", StringComparison.OrdinalIgnoreCase) + "Script ".Length;
            int endIndex = btCabNote.Note.IndexOf("executed", StringComparison.OrdinalIgnoreCase) - 2;
            int length = endIndex - startIndex + 1;
            String scriptName = btCabNote.Note.Substring(startIndex, length);

            StackHashScriptResult stackHashScriptResult = m_TaskParameters.TheScriptResultsManager.GetResultFileData(product, file, theEvent, cab, scriptName);

            if (stackHashScriptResult == null)
                return false;

            BugTrackerScriptResult btScriptResult = new BugTrackerScriptResult(stackHashScriptResult.Name, stackHashScriptResult.ScriptVersion, 
                stackHashScriptResult.LastModifiedDate, stackHashScriptResult.RunDate, stackHashScriptResult.ToString());

            String newBugId = null;
            if (update.TypeOfChange == StackHashChangeType.NewEntry)
                newBugId = m_TaskParameters.PlugInContext.DebugScriptExecuted(null, BugTrackerReportType.Automatic, btProduct, btFile, btEvent, btCab, btScriptResult);
            setPlugInBugReference(product, file, theEvent, newBugId);

            return true;
        }

        
        /// <summary>
        /// Reports the status of the plug-ins after an error.
        /// </summary>
        private void reportPlugInStatus()
        {
            StackHashBugTrackerStatusAdminReport statusReport = new StackHashBugTrackerStatusAdminReport();
            statusReport.Operation = StackHashAdminOperation.BugTrackerPlugInStatus;

            statusReport.ClientData = m_TaskParameters.ClientData;
            statusReport.ContextId = m_TaskParameters.ContextId;
            statusReport.PlugInDiagnostics = m_TaskParameters.PlugInContext.GetContextDiagnostics(null, true);

            if (Reporter.CurrentReporter != null)
            {
                AdminReportEventArgs adminReportArgs = new AdminReportEventArgs(statusReport, true);
                Reporter.CurrentReporter.ReportEvent(adminReportArgs);
            }
        }


        /// <summary>
        /// Gets the next entry from the update table if there is one and calls the bug tracker plugins to process the update.
        /// </summary>
        private void processUpdateTable()
        {
            IErrorIndex index = m_TaskParameters.ErrorIndex;
            StackHashBugTrackerUpdate update;
            bool processedOk = false;

            // Stop processing if a plugin has failed. Allow the user to fix the plugin before we continue.
            if (m_TaskParameters.PlugInContext.NumberOfFailedPlugIns > 0)
                return;

            while ((update = index.GetFirstUpdate()) != null)
            {
                if (this.CurrentTaskState.AbortRequested)
                    throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");

                // Stop processing if a plugin has failed. Allow the user to fix the plugin before we continue.
                if (m_TaskParameters.PlugInContext.NumberOfFailedPlugIns > 0)
                    return;

// DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "UPDATE ENTRY: " + update.DataThatChanged.ToString() + " " + update.EntryId.ToString(CultureInfo.InvariantCulture) + " " + update.ProductId.ToString(CultureInfo.InvariantCulture) + " " + update.FileId.ToString(CultureInfo.InvariantCulture) + " " + update.CabId.ToString(CultureInfo.InvariantCulture));

                // An update was found. Collect the appropriate information and call through to the plugins.
                switch (update.DataThatChanged)
                {
                    case StackHashDataChanged.Product:
                        processedOk = processProductUpdate(update);
                        break;
                    case StackHashDataChanged.File:
                        processedOk = processFileUpdate(update);
                        break;
                    case StackHashDataChanged.Event:
                        processedOk = processEventUpdate(update);
                        break;
                    case StackHashDataChanged.Cab:
                        processedOk = processCabUpdate(update);
                        break;
                    case StackHashDataChanged.Hit:
                        processedOk = processHitUpdate(update);
                        break;
                    case StackHashDataChanged.EventNote:
                        processedOk = processEventNoteUpdate(update);
                        break;
                    case StackHashDataChanged.CabNote:
                        processedOk = processCabNoteUpdate(update);
                        break;
                    case StackHashDataChanged.DebugScript:
                        processedOk = processDebugScriptUpdate(update);
                        break;
                    default:
                        throw new NotSupportedException("Unrecognized update object");                        
                }

                if (m_TaskParameters.PlugInContext.NumberOfFailedPlugIns > 0)
                {
                    reportPlugInStatus();
                }

                // Delete the UpdateTable entry.
                if (processedOk)
                {
                    if (m_TaskParameters.PlugInContext.NumberOfFailedPlugIns == 0)
                        m_Index.RemoveUpdate(update);
                }
                else
                {
                    throw new StackHashException("Failed to process Update table entry. Inconsistent data.");
                }
            }
        }

        /// <summary>
        /// Called when a change occurs to the underlying real index.
        /// Triggers the main thread to actually process the Update Table.
        /// </summary>
        /// <param name="source">Should be the real index.</param>
        /// <param name="e">Identifies the change.</param>
        private void ErrorIndexUpdated(Object source, ErrorIndexEventArgs e)
        {
            m_Events[s_UpdateArrived].Set();
        }


        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);

                // Hook up to receive error index updates.
                m_TaskParameters.ErrorIndex.IndexUpdateAdded += new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdated);

                // Following a PC reboot it may take a while before connections are established etc... so don't automatically 
                // kick off the processing of the Update table (even if there are already entries to process) until a sync 
                // is performed.
                try
                {
                    // Now wait for the an abort or Update event.
                    int eventIndex;
                    while ((eventIndex = WaitHandle.WaitAny(m_Events)) != s_AbortEventIndex)
                    {
                        if (eventIndex == s_UpdateArrived)
                        {
                            processUpdateTable();
                        }
                        else
                        {
                            throw new InvalidOperationException("Unexpected event ID " + eventIndex.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
                finally
                {
                    m_TaskParameters.ErrorIndex.IndexUpdateAdded -= new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdated);
                }
            }
            catch (System.Exception ex)
            {
                if (!CurrentTaskState.AbortRequested)
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Bug Tracker Task Stopped", ex);

                LastException = ex;
            }
            finally
            {
                SetTaskCompleted(/*m_TaskParameters.ErrorIndex*/ null); // Don't update the index task stats for this task.
            }
        }

        /// <summary>
        /// Abort the current task.
        /// </summary>
        public override void StopExternal()
        {
            WritableTaskState.Aborted = true;
            m_Events[s_AbortEventIndex].Set();
            base.StopExternal();
        }
    }
}
