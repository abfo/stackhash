using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Globalization;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    /// <summary>
    /// Parameters to the error index copy task.
    /// This task copys the contents of one index to another.
    /// The indexes may be different types.
    /// </summary>
    public class ErrorIndexCopyTaskParameters : TaskParameters
    {
        private IErrorIndex m_SourceIndex;
        private IErrorIndex m_DestinationIndex;
        private bool m_AssignCopyToContext;
        private bool m_DeleteSourceIndexWhenComplete;
        private ErrorIndexSettings m_DestinationErrorIndexSettings;
        private StackHashSqlConfiguration m_DestSqlSettings;
        private int m_EventsPerBlock;

        public IErrorIndex SourceIndex
        {
            get { return m_SourceIndex; }
            set { m_SourceIndex = value; }
        }

        public IErrorIndex DestinationIndex
        {
            get { return m_DestinationIndex; }
            set { m_DestinationIndex = value; }
        }

        public bool AssignCopyToContext
        {
            get { return m_AssignCopyToContext; }
            set { m_AssignCopyToContext = value; }
        }

        public bool DeleteSourceIndexWhenComplete
        {
            get { return m_DeleteSourceIndexWhenComplete; }
            set { m_DeleteSourceIndexWhenComplete = value; }
        }

        
        public ErrorIndexSettings DestinationErrorIndexSettings
        {
            get { return m_DestinationErrorIndexSettings; }
            set { m_DestinationErrorIndexSettings = value; }
        }

        public StackHashSqlConfiguration DestinationSqlSettings
        {
            get { return m_DestSqlSettings; }
            set { m_DestSqlSettings = value; }
        }

        public int EventsPerBlock
        {
            get { return m_EventsPerBlock; }
            set { m_EventsPerBlock = value; }
        }
    }


    /// <summary>
    /// The ErrorIndexCopyTask copies the contents of a source index to a destination index.
    /// Both indexes must exist.
    /// </summary>
    public class ErrorIndexCopyTask : Task
    {
        private long m_CurrentEvent;
        private long m_TotalEvents;
        private int m_LastPercentageProgressReported;
        private IErrorIndex m_SourceIndex;
        private IErrorIndex m_DestinationIndex;

        private ErrorIndexCopyTaskParameters m_TaskParameters;

        public ErrorIndexCopyTask(ErrorIndexCopyTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.ErrorIndexCopyTask)
        {
            m_TaskParameters = taskParameters;
        }

        private void copyStatistics(IErrorIndex sourceIndex, IErrorIndex destinationIndex)
        {
            Type taskEnumType = typeof(StackHashTaskType);
            Array allValues = Enum.GetValues(taskEnumType);

            foreach (StackHashTaskType taskType in allValues)
            {
                StackHashTaskStatus taskStatus = sourceIndex.GetTaskStatistics(taskType);

                destinationIndex.SetTaskStatistics(taskStatus);
            }
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
            if (totalEvents == 0)
                return;

            int thisPercentageComplete = (int)((currentEvent * 100) / totalEvents);

            if (thisPercentageComplete > m_LastPercentageProgressReported)
            {
                StackHashCopyIndexProgressAdminReport progressReport = new StackHashCopyIndexProgressAdminReport();
                progressReport.Operation = StackHashAdminOperation.ErrorIndexCopyProgress;

                progressReport.ClientData = m_TaskParameters.ClientData;
                progressReport.ContextId = m_TaskParameters.ContextId;
                progressReport.CurrentEvent = currentEvent;
                progressReport.TotalEvents = totalEvents;
                progressReport.CurrentEventId = eventId;
                progressReport.SourceIndexFolder = m_TaskParameters.SourceIndex.ErrorIndexPath;
                progressReport.SourceIndexName = m_TaskParameters.SourceIndex.ErrorIndexPath;
                progressReport.DestinationIndexFolder = m_TaskParameters.DestinationErrorIndexSettings.Folder;
                progressReport.DestinationIndexName = m_TaskParameters.DestinationErrorIndexSettings.Name;

                m_LastPercentageProgressReported = thisPercentageComplete;

                if (Reporter.CurrentReporter != null)
                {
                    AdminReportEventArgs adminReportArgs = new AdminReportEventArgs(progressReport, false);
                    Reporter.CurrentReporter.ReportEvent(adminReportArgs);
                }
            }
        }

        private void copyCabFiles(IErrorIndex sourceIndex, IErrorIndex destinationIndex, 
            StackHashProduct product, StackHashFile file, StackHashEventPackage eventPackage, StackHashCab cab)
        {
            // Copy the actual Cab file and any results files.
            String sourceCabFolder = sourceIndex.GetCabFolder(product, file, eventPackage.EventData, cab);
            String destCabFolder = destinationIndex.GetCabFolder(product, file, eventPackage.EventData, cab);

            if (Directory.Exists(sourceCabFolder))
            {
                String sourceCabFileName = sourceIndex.GetCabFileName(product, file, eventPackage.EventData, cab);

                if (File.Exists(sourceCabFileName))
                {
                    if (!Directory.Exists(destCabFolder))
                        Directory.CreateDirectory(destCabFolder);

                    String destinationCabFileName =
                        String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", destCabFolder, Path.GetFileName(sourceCabFileName));

                    if (!File.Exists(destinationCabFileName))
                        File.Copy(sourceCabFileName, destinationCabFileName);
                }


                // Copy the analysis data if it exists.
                String sourceAnalysisFolder = String.Format(CultureInfo.InvariantCulture, "{0}\\Analysis", sourceCabFolder);
                String destAnalysisFolder = String.Format(CultureInfo.InvariantCulture, "{0}\\Analysis", destCabFolder);

                if (Directory.Exists(sourceAnalysisFolder))
                {
                    String[] allFiles = Directory.GetFiles(sourceAnalysisFolder, "*.log");

                    if (allFiles.Length != 0)
                    {
                        if (!Directory.Exists(destAnalysisFolder))
                            Directory.CreateDirectory(destAnalysisFolder);

                        foreach (String fileName in allFiles)
                        {
                            String destAnalysisFile =
                                String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", destAnalysisFolder, Path.GetFileName(fileName));

                            if (!File.Exists(destAnalysisFile))
                                File.Copy(fileName, destAnalysisFile);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Copies the specified event packages to the destination index along with any other event information, such as 
        /// event and cab notes from the source index.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the events.</param>
        /// <param name="eventPackages">Events to copy.</param>
        private void copyEventBlock(StackHashProduct product, StackHashFile file, StackHashEventPackageCollection eventPackages)
        {
            foreach (StackHashEventPackage eventPackage in eventPackages)
            {
                if (this.CurrentTaskState.AbortRequested)
                    throw new StackHashException("Index event copy aborted", StackHashServiceErrorCode.Aborted);

                // Only add the event if it doesn't already exist in the destination index.
                if (!m_DestinationIndex.EventExists(product, file, eventPackage.EventData))
                    m_DestinationIndex.AddEvent(product, file, eventPackage.EventData);

                // Only normalize when copying from an XML index. Normalizing the event infos sets the dates to midnight PST.
                // The old XML index had - non-normalized dates.
                if (m_SourceIndex.IndexType == ErrorIndexType.Xml)
                    eventPackage.EventInfoList = eventPackage.EventInfoList.Normalize();

                // This call will only add new event infos. Duplicates will be discarded.
                m_DestinationIndex.AddEventInfoCollection(product, file, eventPackage.EventData, eventPackage.EventInfoList);

                // Copy the cabs for the event package.
                foreach (StackHashCabPackage cab in eventPackage.Cabs)
                {
                    if (this.CurrentTaskState.AbortRequested)
                        throw new StackHashException("Index cab copy aborted", StackHashServiceErrorCode.Aborted);

                    // Only add the cab if it doesn't already exist.
                    if (!m_DestinationIndex.CabExists(product, file, eventPackage.EventData, cab.Cab))
                        m_DestinationIndex.AddCab(product, file, eventPackage.EventData, cab.Cab, true);

                    // Get the cab notes. This call will retrieve them in the order they were added so no need to sort.
                    StackHashNotes cabNotes = m_SourceIndex.GetCabNotes(product, file, eventPackage.EventData, cab.Cab);
                    StackHashNotes destCabNotes = m_DestinationIndex.GetCabNotes(product, file, eventPackage.EventData, cab.Cab);

                    foreach (StackHashNoteEntry note in cabNotes)
                    {
                        if (this.CurrentTaskState.AbortRequested)
                            throw new StackHashException("Index cab note copy aborted", StackHashServiceErrorCode.Aborted);

                        // Only add the note if it hasn't already been added to the destination index.
                        // This could have happened if the same event is associated with more than 1 file or product.
                        if (!destCabNotes.ContainsNote(note))
                        {
                            // Set the note id to 0 so a new one will be allocated.
                            note.NoteId = 0;
                            m_DestinationIndex.AddCabNote(product, file, eventPackage.EventData, cab.Cab, note);
                        }
                    }

                    // Copy the cab files over.
                    copyCabFiles(m_SourceIndex, m_DestinationIndex, product, file, eventPackage, cab.Cab);
                }

                // Copy the cab notes for this event.
                StackHashNotes eventNotes = m_SourceIndex.GetEventNotes(product, file, eventPackage.EventData);
                StackHashNotes destEventNotes = m_DestinationIndex.GetEventNotes(product, file, eventPackage.EventData);

                foreach (StackHashNoteEntry note in eventNotes)
                {
                    if (this.CurrentTaskState.AbortRequested)
                        throw new StackHashException("Index event note copy aborted", StackHashServiceErrorCode.Aborted);

                    // Only add the note if it hasn't already been added to the destination index.
                    if (!destEventNotes.ContainsNote(note))
                    {
                        // Set the note id to 0 so a new one will be added (and not replace an existing one).
                        note.NoteId = 0;
                        m_DestinationIndex.AddEventNote(product, file, eventPackage.EventData, note);
                    }
                }

                // Report progress to clients. Progress is reported after each event but only for integral % completions.
                // Therefore this call won't actually send an event to the client every time.
                m_CurrentEvent++;
                reportProgress(m_CurrentEvent, m_TotalEvents, eventPackage.EventData.Id);
            }
        }


        /// <summary>
        /// Copy all events associated with the specified product file.
        /// Events are copied in blocks to avoid using too much memory.
        /// </summary>
        /// <param name="product">The product that owns the file.</param>
        /// <param name="file">The file that owns the events to copy.</param>
        private void copyEvents(StackHashProduct product, StackHashFile file)
        {
            // Define the search criteria for the events.
            StackHashSearchCriteriaCollection searchCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(
                        new StackHashSearchOptionCollection()
                        {
                            new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Id, 0),
                            new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file.Id, 0),
                        })
                };


            // Order by the event id and EventTypeName. Need to do both in case there is more than 1 event with the same 
            // event type name. SQL server doesn't guarantee the order in which they would be returned back so one might be 
            // missed if the event type name is not also specified here.
            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection()
                {
                    new StackHashSortOrder(StackHashObjectType.Event, "Id", true),
                    new StackHashSortOrder(StackHashObjectType.Event, "EventTypeName", true)
                };
            

            // Copy 100 events at a time.
            int startRow = 1;
            int numberOfRows = m_TaskParameters.EventsPerBlock;

            StackHashEventPackageCollection events;

            if (m_SourceIndex.IndexType == ErrorIndexType.SqlExpress)
                events = m_SourceIndex.GetEvents(searchCriteria, startRow, numberOfRows, sortOrder, null);
            else
                events = m_SourceIndex.GetFileEvents(product.Id, file.Id, startRow, numberOfRows);

            while ((events != null) && (events.Count > 0))
            {
                if (this.CurrentTaskState.AbortRequested)
                    throw new StackHashException("Index event block copy aborted", StackHashServiceErrorCode.Aborted);

                copyEventBlock(product, file, events);
                startRow += numberOfRows;

                // Get the next block of events.
                if (events.Count >= numberOfRows)
                {
                    if (m_SourceIndex.IndexType == ErrorIndexType.SqlExpress)
                        events = m_SourceIndex.GetEvents(searchCriteria, startRow, numberOfRows, sortOrder, null);
                    else
                        events = m_SourceIndex.GetFileEvents(product.Id, file.Id, startRow, numberOfRows);
                }
                else
                {
                    events.Clear();
                }
            }     
        }


        /// <summary>
        /// Copies all files for the specified product from the source index to the destination index.
        /// </summary>
        /// <param name="product">The product whose files are to be copied.</param>
        private void copyFiles(StackHashProduct product)
        {
            StackHashFileCollection files = m_SourceIndex.LoadFileList(product);

            foreach (StackHashFile file in files)
            {
                if (this.CurrentTaskState.AbortRequested)
                    throw new StackHashException("Index file copy aborted", StackHashServiceErrorCode.Aborted);

                if (!m_DestinationIndex.FileExists(product, file))
                    m_DestinationIndex.AddFile(product, file);

                // Copy all events associated with the specified file.
                copyEvents(product, file);
            }
        }


        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// Copies the contents of an index piece by piece to the destination index.
        /// The destination index must already exist.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            m_SourceIndex = m_TaskParameters.SourceIndex;
            m_DestinationIndex = m_TaskParameters.DestinationIndex;

            // Record whether the index is active or not so it can be reset at the end of the task.
            bool isSourceIndexActive = m_SourceIndex.IsActive;

            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);

                // Don't allow the computer to sleep when this task is running.
                StackHashUtilities.SystemInformation.DisableSleep();

                // Make sure the source and destination indexes are activated.
                m_SourceIndex.Activate();
                m_DestinationIndex.Activate();

                // The % complete progress reports are sent based on the number of events copied so far.
                // Therefore, get the total events in the source index so this % can be calculated.
                m_TotalEvents = m_SourceIndex.TotalStoredEvents;

                // Get a list of all products in the source index.
                StackHashProductCollection products = m_SourceIndex.LoadProductList();

                // Copy each product.
                foreach (StackHashProduct product in products)
                {
                    if (this.CurrentTaskState.AbortRequested)
                        throw new StackHashException("Index product copy aborted", StackHashServiceErrorCode.Aborted);

                    // Add the product if it doesn't already exist.
                    if (!m_DestinationIndex.ProductExists(product))
                        m_DestinationIndex.AddProduct(product, true); // true = update all fields including non-winqual fields.

                    // Get the product control information.
                    m_DestinationIndex.SetLastHitTimeLocal(product.Id, m_SourceIndex.GetLastHitTimeLocal(product.Id));
                    m_DestinationIndex.SetLastSyncCompletedTimeLocal(product.Id, m_SourceIndex.GetLastSyncCompletedTimeLocal(product.Id));
                    m_DestinationIndex.SetLastSyncStartedTimeLocal(product.Id, m_SourceIndex.GetLastSyncStartedTimeLocal(product.Id));
                    m_DestinationIndex.SetLastSyncTimeLocal(product.Id, m_SourceIndex.GetLastSyncTimeLocal(product.Id));

                    copyFiles(product);
                }

                // Copy the general control information.
                copyStatistics(m_SourceIndex, m_DestinationIndex);
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                // Note that the context controller will check if the index needs switching so this does not need
                // to be done here.
                try
                {
                    // Set the source index back to its initial state (probably deactivated) but
                    // don't dispose of it as it is the designated context index at present.
                    if (!isSourceIndexActive)
                        m_SourceIndex.Deactivate();
                }
                catch {}

                m_DestinationIndex.Deactivate();
                m_DestinationIndex.Dispose();

                StackHashUtilities.SystemInformation.EnableSleep();
                SetTaskCompleted(m_TaskParameters.ErrorIndex);
            }
        }


        /// <summary>
        /// Abort the current task. This sets this.CurrentTaskState.AbortRequested, which is checked by the 
        /// loops that copy parts of the index.
        /// </summary>
        public override void StopExternal()
        {
            WritableTaskState.Aborted = true;
            base.StopExternal();
        }
    }
}

