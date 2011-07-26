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
    public class BugReportTaskParameters : TaskParameters
    {
        private BugTrackerContext m_BugTrackerContext;
        private BugTrackerManager m_BugTrackerManager;
        private ScriptResultsManager m_ScriptResultsManager;
        private StackHashBugReportDataCollection m_ReportRequest;
        private StackHashBugTrackerPlugInSelectionCollection m_PlugIns;

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

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugReportDataCollection ReportRequest
        {
            get { return m_ReportRequest; }
            set { m_ReportRequest = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugTrackerPlugInSelectionCollection PlugIns
        {
            get { return m_PlugIns; }
            set { m_PlugIns = value; }
        }
    }


    public class BugReportTask : Task
    {
        private BugReportTaskParameters m_TaskParameters;
        private IErrorIndex m_Index = null;
        private BugTrackerReportType m_ReportType;
        private StackHashBugTrackerPlugInSelectionCollection m_PlugIns;
        private long m_CurrentEvent;
        private long m_TotalEvents;
        private int m_LastPercentageProgressReported;

        public StackHashBugReportDataCollection BugReportDataCollection
        {
            get { return m_TaskParameters.ReportRequest; }
        }


        /// <summary>
        /// Reports the selected products/files/events/cabs and script data through the installed plug-ins.
        /// This is used to either prime the plug-in with all data from the database or to deliver 
        /// individual events to the bug tracker.
        /// </summary>
        /// <param name="taskParameters">Configuration information for the task.</param>
        public BugReportTask(BugReportTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.BugReportTask)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");

            m_TaskParameters = taskParameters;
            m_Index = m_TaskParameters.ErrorIndex;
        }


        /// <summary>
        /// Report progress to the client.
        /// An updated progress is only sent for new integer percentage values. e.g. 1, 2, 3...
        /// If there are fewer events than 100 then you will see pecentage reported thus... 1, 4, 7...
        /// </summary>
        /// <param name="currentEvent">The current event number.</param>
        /// <param name="totalEvents">Total events to copy.</param>
        /// <param name="eventId">The event id of the last event copied.</param>
        private void reportProgress(long currentEvent, long totalEvents, int eventId)
        {
            // Don't bother reporting progress if only 1 event - only give progress when reporting products or whole index.
            if ((totalEvents == 0) || (totalEvents == 1))
                return;

            int thisPercentageComplete = (int)((currentEvent * 100) / totalEvents);

            if (thisPercentageComplete > m_LastPercentageProgressReported)
            {
                StackHashBugReportProgressAdminReport progressReport = new StackHashBugReportProgressAdminReport();
                progressReport.Operation = StackHashAdminOperation.BugReportProgress;

                progressReport.ClientData = m_TaskParameters.ClientData;
                progressReport.ContextId = m_TaskParameters.ContextId;
                progressReport.CurrentEvent = currentEvent;
                progressReport.TotalEvents = totalEvents;
                progressReport.CurrentEventId = eventId;

                m_LastPercentageProgressReported = thisPercentageComplete;

                if (Reporter.CurrentReporter != null)
                {
                    AdminReportEventArgs adminReportArgs = new AdminReportEventArgs(progressReport, false);
                    Reporter.CurrentReporter.ReportEvent(adminReportArgs);
                }
            }
        }

        
        /// <summary>
        /// Processes a specific cab.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506")]
        private StackHashEvent processCab(StackHashBugReportData request, StackHashProduct product, StackHashFile file,
            StackHashEvent theEvent, StackHashCab cab, BugTrackerProduct btProduct, BugTrackerFile btFile, BugTrackerEvent btEvent)
        {
            if (this.CurrentTaskState.AbortRequested)
                throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
            
            NameValueCollection analysis = new NameValueCollection();
            analysis.Add("DotNetVersion", cab.DumpAnalysis.DotNetVersion);
            analysis.Add("MachineArchitecture", cab.DumpAnalysis.MachineArchitecture);
            analysis.Add("OSVersion", cab.DumpAnalysis.OSVersion);
            analysis.Add("ProcessUpTime", cab.DumpAnalysis.ProcessUpTime);
            analysis.Add("SystemUpTime", cab.DumpAnalysis.SystemUpTime);

            String cabFileName = m_Index.GetCabFileName(product, file, theEvent, cab);

            BugTrackerCab btCab = new BugTrackerCab(cab.Id, cab.SizeInBytes, cab.CabDownloaded, cab.Purged, analysis, cabFileName);
            
            if (((request.Options & StackHashReportOptions.IncludeCabs) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                String newBugId = m_TaskParameters.PlugInContext.CabAdded(m_PlugIns, m_ReportType, btProduct, btFile, btEvent, btCab);
                checkPlugInStatus(m_PlugIns);
                theEvent = setPlugInBugReference(product, file, theEvent, newBugId);
                // Reset this in case it has changed.
                btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, btEvent.Signature);
            }

            if (((request.Options & StackHashReportOptions.IncludeCabNotes) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                StackHashNotes notes = m_Index.GetCabNotes(product, file, theEvent, cab);
                foreach (StackHashNoteEntry note in notes)
                {
                    if (this.CurrentTaskState.AbortRequested)
                        throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
                    
                    BugTrackerNote btEventNote = new BugTrackerNote(note.TimeOfEntry, note.Source, note.User, note.Note);
                    String newBugId = m_TaskParameters.PlugInContext.CabNoteAdded(m_PlugIns, m_ReportType, btProduct, btFile, btEvent, btCab, btEventNote);
                    checkPlugInStatus(m_PlugIns);
                    theEvent = setPlugInBugReference(product, file, theEvent, newBugId);
                    // Reset this in case it has changed.
                    btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, btEvent.Signature);
                }
            }

            if (((request.Options & StackHashReportOptions.IncludeScriptResults) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                StackHashScriptResultFiles scriptResults = m_TaskParameters.TheScriptResultsManager.GetResultFiles(product, file, theEvent, cab);

                foreach (StackHashScriptResultFile scriptResultFile in scriptResults)
                {
                    if (this.CurrentTaskState.AbortRequested)
                        throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
                    try
                    {
                        StackHashScriptResult scriptResult = m_TaskParameters.TheScriptResultsManager.GetResultFileData(product, file, theEvent, cab, scriptResultFile.ScriptName);

                        if (scriptResult != null)
                        {
                            BugTrackerScriptResult btScriptResult = new BugTrackerScriptResult(scriptResult.Name, scriptResult.ScriptVersion,
                                scriptResult.LastModifiedDate, scriptResult.RunDate, scriptResult.ToString());

                            String newBugId = m_TaskParameters.PlugInContext.DebugScriptExecuted(m_PlugIns, m_ReportType, btProduct, btFile, btEvent, btCab, btScriptResult);
                            checkPlugInStatus(m_PlugIns);
                            theEvent = setPlugInBugReference(product, file, theEvent, newBugId);
                            // Reset this in case it has changed.
                            btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, btEvent.Signature);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Information,
                            "Failed to load script file for reporting " + cab.Id.ToString(CultureInfo.InvariantCulture) + " " + scriptResultFile.ScriptName,
                            ex);
                    }
                }
            }

            return theEvent;
        }


        private void checkPlugInStatus(StackHashBugTrackerPlugInSelectionCollection plugIns)
        {
            System.Exception ex = m_TaskParameters.PlugInContext.GetLastError(plugIns);


            if (ex != null)
            {
                throw new StackHashException("Plug-in call failed", ex, StackHashServiceErrorCode.BugTrackerPlugInError);
            }
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
        /// Processes a specific event.
        /// </summary>
        private void processEventPackage(StackHashBugReportData request, StackHashProduct product, StackHashFile file, 
            StackHashEventPackage eventPackage)
        {
            if (this.CurrentTaskState.AbortRequested)
                throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
            
            StackHashEvent theEvent = eventPackage.EventData;

            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);
            NameValueCollection eventSignature = new NameValueCollection();
            foreach (StackHashParameter param in theEvent.EventSignature.Parameters)
            {
                eventSignature.Add(param.Name, param.Value);
            }
            BugTrackerEvent btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);

            if (((request.Options & StackHashReportOptions.IncludeEvents) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                String newBugId = m_TaskParameters.PlugInContext.EventAdded(m_PlugIns, m_ReportType, btProduct, btFile, btEvent);
                checkPlugInStatus(m_PlugIns);
                theEvent = setPlugInBugReference(product, file, theEvent, newBugId);
                // Reset this in case it has changed.
                btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);
            }

            if (((request.Options & StackHashReportOptions.IncludeEventNotes) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                StackHashNotes notes = m_Index.GetEventNotes(product, file, theEvent);
                foreach (StackHashNoteEntry note in notes)
                {
                    if (this.CurrentTaskState.AbortRequested)
                        throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
                    
                    BugTrackerNote btEventNote = new BugTrackerNote(note.TimeOfEntry, note.Source, note.User, note.Note);
                    String newBugId = m_TaskParameters.PlugInContext.EventNoteAdded(m_PlugIns, m_ReportType, btProduct, btFile, btEvent, btEventNote);
                    checkPlugInStatus(m_PlugIns);
                    theEvent = setPlugInBugReference(product, file, theEvent, newBugId);
                    // Reset this in case it has changed.
                    btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);
                }
            }

            if (request.Cab == null)
            {
                foreach (StackHashCabPackage cab in eventPackage.Cabs)
                {
                    processCab(request, product, file, theEvent, cab.Cab, btProduct, btFile, btEvent);
                    // Reset this in case it has changed.
                    btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);
                }
            }
            else
            {
                StackHashCab cab = m_Index.GetCab(product, file, theEvent, request.Cab.Id);
                if (cab != null)
                {
                    theEvent = processCab(request, product, file, theEvent, cab, btProduct, btFile, btEvent);
                    // Reset this in case it has changed.
                    btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);
                }
            }

            // Signal the completion of this event.
            if (((request.Options & StackHashReportOptions.IncludeEvents) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                String newBugId = m_TaskParameters.PlugInContext.EventManualUpdateCompleted(m_PlugIns, m_ReportType, btProduct, btFile, btEvent);
                checkPlugInStatus(m_PlugIns);
                theEvent = setPlugInBugReference(product, file, theEvent, newBugId);
                // Reset this in case it has changed.
                btEvent = new BugTrackerEvent(theEvent.BugId, theEvent.PlugInBugId, theEvent.Id, theEvent.EventTypeName, theEvent.TotalHits, eventSignature);
            }

            m_CurrentEvent++;
            reportProgress(m_CurrentEvent, m_TotalEvents, eventPackage.EventData.Id);
        }


        /// <summary>
        /// Processes a specific file.
        /// </summary>
        private void processFile(StackHashBugReportData request, StackHashProduct product, StackHashFile file)
        {
            if (this.CurrentTaskState.AbortRequested)
                throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
            
            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);
            BugTrackerFile btFile = new BugTrackerFile(file.Name, file.Version, file.Id);

            if (((request.Options & StackHashReportOptions.IncludeFiles) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                m_TaskParameters.PlugInContext.FileAdded(m_PlugIns, m_ReportType, btProduct, btFile);
                checkPlugInStatus(m_PlugIns);
            }

            if (request.TheEvent == null)
            {
                // Parse the events.
                StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection()
                    {
                        new StackHashSortOrder(StackHashObjectType.Event, "Id", true),
                        new StackHashSortOrder(StackHashObjectType.Event, "EventTypeName", true)
                    };

                StackHashSearchOptionCollection searchOptions = new StackHashSearchOptionCollection() 
                    {
                        new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Id, 0),
                        new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file.Id, 0),
                    };

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                    {
                        new StackHashSearchCriteria(searchOptions)
                    };


                int startRow = 1;
                int numberOfRows = 100;
                StackHashEventPackageCollection allPackages = null;
                do
                {
                    allPackages = m_Index.GetEvents(allCriteria, startRow, numberOfRows, sortOrder, null);

                    foreach (StackHashEventPackage eventPackage in allPackages)
                    {
                        processEventPackage(request, product, file, eventPackage);
                    }

                    startRow += numberOfRows;

                } while (allPackages.Count > 0);
            }
            else
            {
                StackHashSearchOptionCollection searchOptions = new StackHashSearchOptionCollection() 
                    {
                        new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Id, 0),
                        new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file.Id, 0),
                        new IntSearchOption(StackHashObjectType.Event, "Id", StackHashSearchOptionType.Equal, request.TheEvent.Id, 0),
                        new StringSearchOption(StackHashObjectType.Event, "EventTypeName", StackHashSearchOptionType.Equal, request.TheEvent.EventTypeName, request.TheEvent.EventTypeName, false),
                    };

                StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
                    {
                        new StackHashSearchCriteria(searchOptions)
                    };

                StackHashEventPackageCollection eventPackages = m_Index.GetEvents(allCriteria, null);

                if ((eventPackages != null) && (eventPackages.Count == 1))
                    processEventPackage(request, product, file, eventPackages[0]);
            }
        }


        /// <summary>
        /// Processes a specific product.
        /// </summary>
        private void processProduct(StackHashBugReportData request, StackHashProduct product)
        {
            if (this.CurrentTaskState.AbortRequested)
                throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
            
            BugTrackerProduct btProduct = new BugTrackerProduct(product.Name, product.Version, product.Id);

            if (((request.Options & StackHashReportOptions.IncludeProducts) != 0) ||
                ((request.Options & StackHashReportOptions.IncludeAllObjects) != 0))
            {
                m_TaskParameters.PlugInContext.ProductAdded(m_PlugIns, m_ReportType, btProduct);
                checkPlugInStatus(m_PlugIns);
            }

            if (request.File == null)
            {
                StackHashFileCollection allFiles = m_Index.LoadFileList(product);

                foreach (StackHashFile file in allFiles)
                {
                    processFile(request, product, file);
                }
            }
            else
            {
                StackHashFile file = m_Index.GetFile(product, request.File.Id);

                if (file != null)
                    processFile(request, product, file);
            }
        }


        /// <summary>
        /// Get the level of report that has been requested.
        /// </summary>
        /// <param name="request">Full request.</param>
        /// <returns>Report level.</returns>
        private BugTrackerReportType getReportType(StackHashBugReportData request)
        {
            BugTrackerReportType reportType = BugTrackerReportType.ManualFull;

            // Determin what the report type is. The report type is reported to the plugin. It indicates
            // whether this manual report is for a whole product, file, event, cab, script or what.
            if (request.Product == null)
            {
                reportType = BugTrackerReportType.ManualFull;
            }
            else
            {
                if (request.File == null)
                {
                    reportType = BugTrackerReportType.ManualProduct;
                }
                else
                {
                    if (request.TheEvent == null)
                    {
                        reportType = BugTrackerReportType.ManualFile;
                    }
                    else
                    {
                        if (request.Cab == null)
                        {
                            m_ReportType = BugTrackerReportType.ManualEvent;
                        }
                        else
                        {
                            if (request.ScriptName == null)
                            {
                                m_ReportType = BugTrackerReportType.ManualCab;
                            }
                            else
                            {
                                // TODO: Support manual script writing.
                                m_ReportType = BugTrackerReportType.ManualScript;
                            }
                        }
                    }
                }
            }
            return reportType;
        }


        /// <summary>
        /// Get the total number of events to be reported for this request.
        /// </summary>
        /// <param name="request">Full request.</param>
        /// <param name="reportType">The type of the request.</param>
        /// <returns>Number of events.</returns>
        private long getNumberOfEvents(StackHashBugReportData request, BugTrackerReportType reportType)
        {
            long totalEvents = 0;
            StackHashProduct product = null;

            switch (reportType)
            {
                case BugTrackerReportType.ManualFull:
                    totalEvents = m_Index.TotalStoredEvents;
                    break;
                case BugTrackerReportType.ManualProduct:
                    product = m_Index.GetProduct(request.Product.Id);
                    if (product != null)
                        totalEvents = product.TotalStoredEvents;
                    break;
                case BugTrackerReportType.ManualFile:
                    product = m_Index.GetProduct(request.Product.Id);
                    if (product != null)
                        totalEvents = product.TotalStoredEvents;
                    break;
                default:
                    totalEvents = 1;
                    break;
            }

            return totalEvents;
        }


        /// <summary>
        /// Get the number of events to be reported across all requests.
        /// </summary>
        /// <param name="requests">All requests.</param>
        /// <returns>Number of events.</returns>
        private long getNumberOfEvents(StackHashBugReportDataCollection requests)
        {
            long totalEvents = 0;
            foreach (StackHashBugReportData request in requests)
            {
                totalEvents += getNumberOfEvents(request, getReportType(request));
            }

            return totalEvents;
        }


        /// <summary>
        /// Get the highest level of the 2 specified report types.
        /// The heirarchy is...
        /// Full - Product - File - Event - Cab - Script - Automatic
        /// </summary>
        /// <param name="reportType1">First report type to compare.</param>
        /// <param name="reportType2">Second report type to compare.</param>
        /// <returns>Highest level report type of the parameters.</returns>
        private BugTrackerReportType getHigherLevelReport(BugTrackerReportType reportType1, BugTrackerReportType reportType2)
        {
            if ((reportType1 == BugTrackerReportType.ManualFull) || (reportType2 == BugTrackerReportType.ManualFull))
                return BugTrackerReportType.ManualFull;
            if ((reportType1 == BugTrackerReportType.ManualProduct) || (reportType2 == BugTrackerReportType.ManualProduct))
                return BugTrackerReportType.ManualProduct;
            if ((reportType1 == BugTrackerReportType.ManualFile) || (reportType2 == BugTrackerReportType.ManualFile))
                return BugTrackerReportType.ManualFile;
            if ((reportType1 == BugTrackerReportType.ManualEvent) || (reportType2 == BugTrackerReportType.ManualEvent))
                return BugTrackerReportType.ManualEvent;
            if ((reportType1 == BugTrackerReportType.ManualCab) || (reportType2 == BugTrackerReportType.ManualCab))
                return BugTrackerReportType.ManualCab;
            if ((reportType1 == BugTrackerReportType.ManualScript) || (reportType2 == BugTrackerReportType.ManualScript))
                return BugTrackerReportType.ManualScript;

            return BugTrackerReportType.Automatic;
        }


        /// <summary>
        /// Get the highest level report in the set of requests.
        /// </summary>
        /// <param name="requests">All requests.</param>
        /// <returns>Highest level report.</returns>
        private BugTrackerReportType getHighestLevelReport(StackHashBugReportDataCollection requests)
        {
            BugTrackerReportType highestReportType = BugTrackerReportType.Automatic;

            foreach (StackHashBugReportData request in requests)
            {
                BugTrackerReportType reportType = getReportType(request);

                highestReportType = getHigherLevelReport(reportType, highestReportType);
            }

            return highestReportType;
        }

        
        /// <summary>
        /// Processes a specific report request.
        /// </summary>
        private void processReportRequest(StackHashBugReportData request)
        {
            if (this.CurrentTaskState.AbortRequested)
                throw new OperationCanceledException("Reporting events to Bug Tracker plug-ins");
            
            // Determine what the report type is. The report type is reported to the plugin. It indicates
            // whether this manual report is for a whole product, file, event, cab, script or what.
            m_ReportType = getReportType(request);

            if (request.Product == null)
            {
                // Loop through all products.
                StackHashProductCollection allProducts = m_Index.LoadProductList();

                foreach (StackHashProduct product in allProducts)
                {
                    processProduct(request, product);
                }
            }
            else
            {
                StackHashProduct product = m_Index.GetProduct(request.Product.Id);

                if (product != null)
                    processProduct(request, product);
            }
        }


        /// <summary>
        /// Processes each report request in turn.
        /// </summary>
        private void processAllReportRequests()
        {
            StackHashBugReportDataCollection allRequests = m_TaskParameters.ReportRequest;

            m_TotalEvents = getNumberOfEvents(allRequests);
            
            foreach (StackHashBugReportData request in allRequests)
            {
                processReportRequest(request);
            }            
        }



        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            SetTaskStarted(m_TaskParameters.ErrorIndex);
            StackHashUtilities.SystemInformation.DisableSleep();

            m_PlugIns = m_TaskParameters.PlugIns;

            try
            {
                // If a full sync has been requested then stop the BugTrackerTask reporting any events until the 
                // sync is complete.
                BugTrackerReportType highestReportType = getHighestLevelReport(m_TaskParameters.ReportRequest);

                if (highestReportType == BugTrackerReportType.ManualFull)
                {
                    // Disable logging by the BugTrackerManager.
                    m_TaskParameters.ErrorIndex.UpdateTableActive = false;

                    // Clear the update list.
                    m_TaskParameters.ErrorIndex.ClearAllUpdates();
                }

                processAllReportRequests();
            }
            catch (System.Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                try
                {
                    // Reenable logging by the BugTrackerManager.
                    m_TaskParameters.ErrorIndex.UpdateTableActive = true;
                }
                finally
                {
                    StackHashUtilities.SystemInformation.EnableSleep();
                    SetTaskCompleted(m_TaskParameters.ErrorIndex);
                }
            }
        }

        /// <summary>
        /// Abort the current task.
        /// </summary>
        public override void StopExternal()
        {
            WritableTaskState.Aborted = true;
            base.StopExternal();
        }
    }
}
