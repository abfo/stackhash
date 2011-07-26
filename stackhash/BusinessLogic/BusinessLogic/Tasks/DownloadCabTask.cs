using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using StackHashWinQual;
using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    /// <summary>
    /// Parameters passed into the download cab task.
    /// </summary>
    public class DownloadCabTaskParameters : TaskParameters
    {
        private String m_UserName;
        private String m_Password;
        private WinQualContext m_WinQualContext;
        private StackHashProduct m_Product;
        private StackHashFile m_File;
        private StackHashEvent m_Event;
        private StackHashCab m_Cab;

        public String UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        public String Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }

        public WinQualContext ThisWinQualContext
        {
            get { return m_WinQualContext; }
            set { m_WinQualContext = value; }
        }

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

        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }
    }


    /// <summary>
    /// DownloadCabTask downloads the specified cab file from the WinQual site.
    /// The cab link is created on the fly. If the structure of the link changes then this task may not work.
    /// </summary>
    public class DownloadCabTask : Task
    {
        private DownloadCabTaskParameters m_TaskParameters;
        private IWinQualServices m_WinQualServices;


        public DownloadCabTask(DownloadCabTaskParameters taskParameters) :
            base(taskParameters as TaskParameters, StackHashTaskType.DownloadCabTask)
        {
            if (taskParameters == null)
                throw new ArgumentNullException("taskParameters");
            m_TaskParameters = taskParameters;
            m_WinQualServices = m_TaskParameters.ThisWinQualContext.WinQualServices;
        }


        /// <summary>
        /// Entry point of the task. 
        /// The task logs in to the winqual site and then requests the specified cab to be downloaded.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);
                StackHashUtilities.SystemInformation.DisableSleep();

                try
                {
                    // Log on to WinQual.
                    m_WinQualServices.LogOn(m_TaskParameters.UserName, m_TaskParameters.Password);

                    // Call the WinQual services to download the cab. The cab info entry will also be updated.
                    m_WinQualServices.GetCab(
                         m_TaskParameters.ErrorIndex, m_TaskParameters.Product, m_TaskParameters.File, m_TaskParameters.Event, m_TaskParameters.Cab);
                }
                finally
                {
                }
            }
            catch (Exception ex)
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
            m_WinQualServices.AbortCurrentOperation();
            m_TaskParameters.ErrorIndex.AbortCurrentOperation();
            base.StopExternal();
        }
    }
}
