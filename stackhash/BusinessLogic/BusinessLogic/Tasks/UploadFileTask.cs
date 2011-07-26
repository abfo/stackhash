using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;


namespace StackHashTasks
{
    public class UploadFileTaskParameters : TaskParameters
    {
        private WinQualContext m_WinQualContext;
        private WinQualSettings m_WinQualSettings;
        private String m_FileName;

        public WinQualContext ThisWinQualContext
        {
            get { return m_WinQualContext; }
            set { m_WinQualContext = value; }
        }

        public WinQualSettings WinQualSettings
        {
            get { return m_WinQualSettings; }
            set { m_WinQualSettings = value; }
        }

        public String FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }
    }


    public class UploadFileTask : Task
    {
        private UploadFileTaskParameters m_TaskParameters;
        private IWinQualServices m_WinQualServices;

        public UploadFileTask(UploadFileTaskParameters taskParameters) :
            base(taskParameters as TaskParameters, StackHashTaskType.UploadFileTask)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");
            m_TaskParameters = taskParameters;
            m_WinQualServices = m_TaskParameters.ThisWinQualContext.WinQualServices;
        }



        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// An attempt is made to upload the mapping file to the winqual service.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            bool loggedOn = false;

            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);


                // Don't allow the PC to go into sleep mode while syncing.
                StackHashUtilities.SystemInformation.DisableSleep();

                try
                {
                    // Log on to WinQual.
                    m_WinQualServices.LogOn(m_TaskParameters.WinQualSettings.UserName, m_TaskParameters.WinQualSettings.Password);

                    // Upload the file.
                    m_WinQualServices.UploadFile(m_TaskParameters.FileName);
                }
                finally
                {
                    try
                    {
                        if (loggedOn)
                            m_WinQualServices.LogOff();
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to log off Win Qual", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                if (File.Exists(m_TaskParameters.FileName))
                    File.Delete(m_TaskParameters.FileName);

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
            m_WinQualServices.AbortCurrentOperation();
            m_TaskParameters.ErrorIndex.AbortCurrentOperation();
            base.StopExternal();
        }
    }
}
