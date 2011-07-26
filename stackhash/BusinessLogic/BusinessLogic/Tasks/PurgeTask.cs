using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Diagnostics;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    /// <summary>
    /// Parameters for the purge task.
    /// </summary>
    public class PurgeTaskParameters : TaskParameters
    {
        private StackHashPurgeOptionsCollection m_PurgeOptions;

        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashPurgeOptionsCollection PurgeOptions
        {
            get { return m_PurgeOptions; }
            set { m_PurgeOptions = value; }
        }
    }


    /// <summary>
    /// Removes cabs, dumps and potentially other data from the index that may have exceeded the WinQual
    /// license agreement time period for holding potentially "Personal Information".
    /// </summary>
    public class PurgeTask : Task
    {
        private PurgeTaskParameters m_TaskParameters;
        private StackHashPurgeStatistics m_Statistics = new StackHashPurgeStatistics();


        public StackHashPurgeStatistics Statistics
        {
            get { return m_Statistics.Clone(); }
        }


        /// <summary>
        /// Constructs the purge task with the specified params.
        /// </summary>
        /// <param name="taskParameters">Params required by the purge task.</param>
        public PurgeTask(PurgeTaskParameters taskParameters) :
            base(taskParameters as TaskParameters, StackHashTaskType.PurgeTask)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");
            m_TaskParameters = taskParameters;
        }


        /// <summary>
        /// Finds the appropriate purge options for the specified product, file, event.
        /// </summary>
        /// <param name="product">The product owning the event.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to be purged.</param>
        /// <returns>Options to use for purge.</returns>
        private StackHashPurgeOptions findPurgeOptions(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            StackHashPurgeOptions globalPurgeOptions = null;
            StackHashPurgeOptions productPurgeOptions = null;
            StackHashPurgeOptions filePurgeOptions = null;
            StackHashPurgeOptions eventPurgeOptions = null;

            foreach (StackHashPurgeOptions purgeOptions in m_TaskParameters.PurgeOptions)
            {
                switch (purgeOptions.PurgeObject)
                {
                    case StackHashPurgeObject.PurgeGlobal:
                        globalPurgeOptions = purgeOptions;
                        break;
                    case StackHashPurgeObject.PurgeProduct:
                        if (purgeOptions.Id == product.Id)
                            productPurgeOptions = purgeOptions;
                        break;
                    case StackHashPurgeObject.PurgeFile:
                        if (purgeOptions.Id == file.Id)
                            filePurgeOptions = purgeOptions;
                        break;
                    case StackHashPurgeObject.PurgeEvent:
                        if (purgeOptions.Id == theEvent.Id)
                            eventPurgeOptions = purgeOptions;
                        break;
                    default:
                        throw new InvalidOperationException("Purge option not known");
                }
            }

            // Prioritize from event up.
            if (eventPurgeOptions != null)
                return eventPurgeOptions;
            else if (filePurgeOptions != null)
                return filePurgeOptions;
            else if (productPurgeOptions != null)
                return productPurgeOptions;
            else if (globalPurgeOptions != null)
                return globalPurgeOptions;
            else
                return null;
        }


        /// <summary>
        /// Purges the specified event if required.
        /// </summary>
        /// <param name="product">The product owning the event.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to be purged.</param>
        /// <param name="purgeOptions">Options to use for purging.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private void purgeEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashPurgeOptions purgeOptions)
        {
            IErrorIndex errorIndex = m_TaskParameters.ErrorIndex;

            // Get a list of all cabs for the event.
            StackHashCabCollection cabs = errorIndex.LoadCabList(product, file, theEvent);
            bool error = false;

            foreach (StackHashCab cab in cabs)
            {
                String cabFileFolder = errorIndex.GetCabFolder(product, file, theEvent, cab);

                // Check if anything to purge.
                if (!Directory.Exists(cabFileFolder))
                    continue;

                // Don't purge if files not that old.
                int daysOld = (DateTime.Now.ToUniversalTime() - cab.DateCreatedLocal).Days;

                if (daysOld < purgeOptions.AgeToPurge)
                    continue;

                if (purgeOptions.PurgeDumpFiles)
                {
                    String[] dumpFiles = Directory.GetFiles(cabFileFolder, "*.*dmp*");

                    foreach (String dumpFile in dumpFiles)
                    {
                        try
                        {
                            // Update the statistics.
                            FileInfo fileInfo = new FileInfo(dumpFile);
                            m_Statistics.NumberOfDumpFiles++;
                            m_Statistics.DumpFilesTotalSize += fileInfo.Length;

                            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                String.Format(CultureInfo.InvariantCulture, "Purged File {0}, {1}, {2}, {3}, {4}", dumpFile, product.Id, file.Id, theEvent.Id, cab.Id));
                            File.Delete(dumpFile);
                        }
                        catch (System.Exception ex)
                        {
                            error = true;
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to purge dump file " + dumpFile, ex);
                        }
                    }
                }
                if (purgeOptions.PurgeCabFiles)
                {
                    String cabFileName = errorIndex.GetCabFileName(product, file, theEvent, cab);

                    if (File.Exists(cabFileName))
                    {
                        try
                        {
                            // Update the statistics.
                            FileInfo fileInfo = new FileInfo(cabFileName);
                            m_Statistics.NumberOfCabs++;
                            m_Statistics.CabsTotalSize += fileInfo.Length;

                            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                String.Format(CultureInfo.InvariantCulture, "Purged Cab File {0}, {1}, {2}, {3}, {4}", cabFileName, product.Id, file.Id, theEvent.Id, cab.Id));
                            File.Delete(cabFileName);
                        }
                        catch (System.Exception ex)
                        {
                            error = true;
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to purge cab file " + cabFileName, ex);
                        }
                    }
                }

                if (!error)
                {
                    // Mark the cab as purged and no longer downloaded.
                    cab.Purged = true;
                    cab.CabDownloaded = false;
                    errorIndex.AddCab(product, file, theEvent, cab, false);
                }
            }
        }


        /// <summary>
        /// Called when an event is found. 
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        private void processEvent(Object sender, ErrorIndexParseEventsEventArgs e)
        {
            StackHashEvent currentEvent = e.Parser.CurrentEvent;

            // Which purge options should be used for this event.
            StackHashPurgeOptions purgeOptions = findPurgeOptions(e.Parser.Product, e.Parser.File, e.Parser.CurrentEvent);

            if (purgeOptions != null)
            {
                // Now purge according to the policy specified.
                purgeEvent(e.Parser.Product, e.Parser.File, e.Parser.CurrentEvent, purgeOptions);
            }
        }


        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// Removes cabs, dumps and potentially other data from the index that may have exceeded the WinQual
        /// license agreement time period for holding potentially "Personal Information".
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                IErrorIndex errorIndex = m_TaskParameters.ErrorIndex;

                SetTaskStarted(m_TaskParameters.ErrorIndex);
                StackHashUtilities.SystemInformation.DisableSleep();


                ErrorIndexEventParser parser = new ErrorIndexEventParser();
                parser.ParseEvent += new EventHandler<ErrorIndexParseEventsEventArgs>(this.processEvent);

                DateTime purgeDate = DateTime.Now.ToUniversalTime().AddDays(-1 * m_TaskParameters.PurgeOptions.FindMostRecentPurgeAge());

                try
                {
                    // Get the list of products.
                    StackHashProductCollection products = errorIndex.LoadProductList();

                    foreach (StackHashProduct product in products)
                    {
                        if (this.CurrentTaskState.AbortRequested)
                            throw new OperationCanceledException("Purging product");

                        // Get the files associated with this product.
                        StackHashFileCollection files = errorIndex.LoadFileList(product);

                        foreach (StackHashFile file in files)
                        {
                            if (this.CurrentTaskState.AbortRequested)
                                throw new OperationCanceledException("Purging file");

                            // Now parse the events one at a time. Instead of getting a list of all the events - use a 
                            // callback to analyze each event. Note this allows for an abort by returning false 
                            // from the callback.
                            parser.Product = product;
                            parser.File = file;

                            parser.SearchCriteriaCollection = new StackHashSearchCriteriaCollection() 
                            {
                                new StackHashSearchCriteria(
                                    new StackHashSearchOptionCollection() 
                                    {
                                        new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Id, 0),
                                        new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, file.Id, 0),
                                        new DateTimeSearchOption(StackHashObjectType.CabInfo, "DateCreatedLocal", StackHashSearchOptionType.LessThanOrEqual, purgeDate, purgeDate),
                                        new IntSearchOption(StackHashObjectType.CabInfo, "Purged", StackHashSearchOptionType.Equal, 0, 0), // 0 is false.
                                        new IntSearchOption(StackHashObjectType.CabInfo, "CabDownloaded", StackHashSearchOptionType.Equal, 1, 0), // 1 is true.
                                    })
                            }; 

                            if (!errorIndex.ParseEvents(product, file, parser))
                                throw new OperationCanceledException("Aborted while purging events");
                        }
                    }
                }
                finally
                {
                    parser.ParseEvent -= new EventHandler<ErrorIndexParseEventsEventArgs>(this.processEvent);
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Information, "Purge task failed", ex);
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
            m_TaskParameters.ErrorIndex.AbortCurrentOperation();
            base.StopExternal();
        }
    }
}