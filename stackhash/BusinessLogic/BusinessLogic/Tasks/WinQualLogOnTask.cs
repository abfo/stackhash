using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashWinQual;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashErrorIndex;


namespace StackHashTasks
{
    /// <summary>
    /// Parameters for the LogOn task.
    /// To log on a username and password are required.
    /// </summary>
    public class WinQualLogOnTaskParameters : TaskParameters
    {
        string m_UserName;
        string m_Password;
        IWinQualServices m_WinQualServices;

        public string UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }

        public IWinQualServices WinQualServicesObject
        {
            get { return m_WinQualServices; }
            set { m_WinQualServices = value; }
        }
    }


    /// <summary>
    /// The logon task logs on to the WinQual service on the internet.
    /// </summary>
    public class WinQualLogOnTask : Task
    {
        private WinQualLogOnTaskParameters m_TaskParameters;
        private WinQualContext m_WinQualContext;

        public WinQualContext ThisWinQualContext
        {
            get { return m_WinQualContext; }
        }

        /// <summary>
        /// Constructs the LogOn task. The LogOn task logs on to the Microsoft WinQual service.
        /// The intention is to validate the username and password.
        /// </summary>
        /// <param name="taskParameters">Parameters for the task.</param>
        public WinQualLogOnTask(WinQualLogOnTaskParameters taskParameters)
            : base(taskParameters as TaskParameters, StackHashTaskType.WinQualLogOnTask)
        {
            m_TaskParameters = taskParameters;
        }

        /// <summary>
        /// Entry point of the task. This is where the real work is done.
        /// A logon is issues and the result awaited.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public override void EntryPoint()
        {
            try
            {
                SetTaskStarted(m_TaskParameters.ErrorIndex);
                StackHashUtilities.SystemInformation.DisableSleep();

                // Create a new WinQual context. This stores information about the 
                // configuration and state of the WinQual connection.
                // Note that the WinQualServices is passed in here so that WinQualContext can
                // be tested with a dummy.
                m_WinQualContext = new WinQualContext(m_TaskParameters.WinQualServicesObject);


                // Log on to WinQual.
                m_WinQualContext.WinQualServices.LogOn(m_TaskParameters.UserName, m_TaskParameters.Password);
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
    }
}
