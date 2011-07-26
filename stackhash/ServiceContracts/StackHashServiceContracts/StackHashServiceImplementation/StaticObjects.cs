using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using StackHashTasks;

namespace StackHashServiceImplementation
{
    public class StaticObjects : IDisposable
    {
        private String m_SettingsPathAndFileName;
        private Controller m_Controller;
        private static StaticObjects s_StaticObjectInstance;
        private bool m_HostedInWindowsService;
        private bool m_IsTestMode;

        /// <summary>
        /// Called when an admin report is sent.
        /// </summary>
        /// <param name="sender">The task manager reporting the event.</param>
        /// <param name="e">The event arguments.</param>
        private void adminReportHandler(Object sender, EventArgs e)
        {
            AdminReportEventArgs adminReportArgs = e as AdminReportEventArgs;

            // Now report the event.
            InternalService.OnAdminNotification(adminReportArgs.Report, adminReportArgs.SendToAll);
        }

        public Controller TheController
        {
            get 
            {
                if (m_Controller == null)
                {
                    throw new InvalidOperationException("Service controller not initialized");
                }
                else
                {
                    return m_Controller;
                }
            }
        }

        public static StaticObjects TheStaticObjects
        {
            get { return s_StaticObjectInstance; }
        }

        public StaticObjects(String settingsPathAndFileName, bool hostedInWindowsService, bool isTestMode)
        {
            m_SettingsPathAndFileName = settingsPathAndFileName;
            m_HostedInWindowsService = hostedInWindowsService;
            m_IsTestMode = isTestMode;

            m_Controller = new Controller(m_SettingsPathAndFileName, m_HostedInWindowsService, m_IsTestMode);

            // Hook up for controller events.
            m_Controller.AdminReports += new EventHandler<AdminReportEventArgs>(adminReportHandler);

            s_StaticObjectInstance = this;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822")]
        public void EnableServices()
        {
            // Start the service hosts.
            ServiceHostControl.OpenServiceHosts(true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822")]
        public void DisableServices()
        {
            // Stop the service hosts.
            ServiceHostControl.CloseServiceHosts();
        }
        
        public void Restart()
        {
            // Recycle the controller.
            m_Controller.AdminReports -= new EventHandler<AdminReportEventArgs>(adminReportHandler);
            m_Controller.Dispose();
            m_Controller = null;

            // Hook up for controller events.
            m_Controller = new Controller(m_SettingsPathAndFileName, m_HostedInWindowsService, m_IsTestMode);
            m_Controller.AdminReports += new EventHandler<AdminReportEventArgs>(adminReportHandler);
        }


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisableServices();
                if (m_Controller != null)
                {
                    m_Controller.AdminReports -= new EventHandler<AdminReportEventArgs>(adminReportHandler);
                    m_Controller.Dispose();
                    m_Controller = null;
                }

                s_StaticObjectInstance = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
