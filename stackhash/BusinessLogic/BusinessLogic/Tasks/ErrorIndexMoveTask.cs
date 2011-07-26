using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    /// <summary>
    /// Parameters to the error index move task.
    /// </summary>
    public class ErrorIndexMoveTaskParameters : TaskParameters
    {
        private String m_NewErrorIndexPath;
        private String m_NewErrorIndexName;
        private StackHashSqlConfiguration m_NewSqlSettings;
        private int m_IntervalBetweenProgressReportsInSeconds;

        public string NewErrorIndexPath
        {
            get { return m_NewErrorIndexPath; }
            set { m_NewErrorIndexPath = value; }
        }

        public string NewErrorIndexName
        {
            get { return m_NewErrorIndexName; }
            set { m_NewErrorIndexName = value; }
        }

        public StackHashSqlConfiguration NewSqlSettings
        {
            get { return m_NewSqlSettings; }
            set { m_NewSqlSettings = value; }
        }

        public int IntervalBetweenProgressReportsInSeconds
        {
            get { return m_IntervalBetweenProgressReportsInSeconds; }
            set { m_IntervalBetweenProgressReportsInSeconds = value; }
        }
    }


    /// <summary>
    /// The ErrorIndexMoveTask moves the index to a new location.
    /// The new location can be on the same drive or a different drive.
    /// </summary>
    public class ErrorIndexMoveTask : Task
    {
        private ErrorIndexMoveTaskParameters m_TaskParameters;
        private int m_FileCount;
        private DateTime m_LastProgressSentTime;

        public ErrorIndexMoveTask(ErrorIndexMoveTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.ErrorIndexMoveTask)
        {
            m_TaskParameters = taskParameters;
        }



        /// <summary>
        /// Report progress to the client.
        /// </summary>
        /// <param name="currentFileName">Current file being copied (or just copied).</param>
        /// <param name="fileCount">Number of files copied so far.</param>
        /// <param name="isCopyStart">True - start of a copy. False - end of a copy.</param>
        private void reportProgress(String currentFileName, int fileCount, bool isCopyStart)
        {
            StackHashMoveIndexProgressAdminReport progressReport = new StackHashMoveIndexProgressAdminReport();
            progressReport.Operation = StackHashAdminOperation.ErrorIndexMoveProgress;

            progressReport.ClientData = m_TaskParameters.ClientData;
            progressReport.ContextId = m_TaskParameters.ContextId;
            progressReport.CurrentFileName = currentFileName;
            progressReport.FileCount = fileCount;
            progressReport.IsCopyStart = isCopyStart;

            if (Reporter.CurrentReporter != null)
            {
                AdminReportEventArgs adminReportArgs = new AdminReportEventArgs(progressReport, false);
                Reporter.CurrentReporter.ReportEvent(adminReportArgs);
            }
        }

        
        /// <summary>
        /// Called when a file is copied during an error index move operation.
        /// This will only get called if the move is to a different drive.
        /// </summary>
        /// <param name="sender">The error index.</param>
        /// <param name="e">Information about the file.</param>
        private void errorIndexMoveCallback(Object sender, ErrorIndexMoveEventArgs e)
        {
            m_FileCount++;

            // Only report the start of a file copy for now.
            if (e.IsCopyStarted == false)
                return;

            TimeSpan timeSinceLastProgressReport = DateTime.Now - m_LastProgressSentTime;

            String fileExtension = Path.GetExtension(e.FileName);


            // Always report the copying of the database files because these may take some time and the user should see
            // that those file copies are in progress.
            bool alwaysSendReport = false;
            if (!String.IsNullOrEmpty(fileExtension))
            {
                String normalizedExtension = fileExtension.ToUpperInvariant();
                if ((normalizedExtension == ".MDF") || (normalizedExtension == ".LDF"))
                    alwaysSendReport = true;
            }

            if ((timeSinceLastProgressReport.TotalSeconds >= m_TaskParameters.IntervalBetweenProgressReportsInSeconds) ||
                alwaysSendReport)
            {
                m_LastProgressSentTime = DateTime.Now;
                reportProgress(e.FileName, e.FileCount, e.IsCopyStarted);
            }
        }

        
        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// Move the index to the specified location.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            m_TaskParameters.ErrorIndex.IndexMoveProgress += new EventHandler<ErrorIndexMoveEventArgs>(this.errorIndexMoveCallback);

            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);
                StackHashUtilities.SystemInformation.DisableSleep();

                m_TaskParameters.ErrorIndex.MoveIndex(m_TaskParameters.NewErrorIndexPath, 
                    m_TaskParameters.NewErrorIndexName, m_TaskParameters.NewSqlSettings, true);

                // Move succeeded so set the new context settings.
                m_TaskParameters.SettingsManager.SetContextErrorIndexSettings(
                    m_TaskParameters.ContextId, m_TaskParameters.NewErrorIndexPath, m_TaskParameters.NewErrorIndexName, m_TaskParameters.NewSqlSettings);
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                m_TaskParameters.ErrorIndex.IndexMoveProgress -= new EventHandler<ErrorIndexMoveEventArgs>(this.errorIndexMoveCallback);
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
            base.StopExternal();
        }
    }
}

