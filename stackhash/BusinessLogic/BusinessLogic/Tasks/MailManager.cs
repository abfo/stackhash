using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Mail;
using System.Net;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashUtilities;


namespace StackHashTasks
{
    public class MailManager : IDisposable
    {
        private StackHashEmailSettings m_MailSettings;
        private SmtpClient m_SmtpClient;
        private bool m_Disposed;
        private String m_ProfileName;


        public MailManager(StackHashEmailSettings mailSettings, String profileName)
        {
            if (mailSettings == null)
                throw new ArgumentNullException("mailSettings");

            m_MailSettings = mailSettings;
            m_ProfileName = profileName;
        }

        public String ProfileName
        {
            get { return m_ProfileName; }
            set { m_ProfileName = value; }
        }

        /// <summary>
        /// Change the current email settings.
        /// </summary>
        /// <param name="mailSettings">New email settings.</param>
        public void SetEmailSettings(StackHashEmailSettings mailSettings)
        {
            Monitor.Enter(this);

            try
            {
                m_MailSettings = mailSettings;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Generate an SMTP client for sending emails.
        /// </summary>
        private void recycleSmtpClient()
        {
            if (m_SmtpClient != null)
            {
                m_SmtpClient.Dispose();
                m_SmtpClient = null;
            }

            // Must have host.
            if (String.IsNullOrEmpty(this.m_MailSettings.SmtpSettings.SmtpHost))
                return;

            m_SmtpClient = new SmtpClient(this.m_MailSettings.SmtpSettings.SmtpHost, this.m_MailSettings.SmtpSettings.SmtpPort);

            if ((!String.IsNullOrEmpty(this.m_MailSettings.SmtpSettings.SmtpUsername)) &&
                (!String.IsNullOrEmpty(this.m_MailSettings.SmtpSettings.SmtpPassword)))
            {
                NetworkCredential credentials = new NetworkCredential(this.m_MailSettings.SmtpSettings.SmtpUsername, this.m_MailSettings.SmtpSettings.SmtpPassword);
                m_SmtpClient.Credentials = credentials;
            }
        }


        /// <summary>
        /// Checks to see if the specified admin report should be emailed to interested users.
        /// If so, the email is sent to those registered to receive it.
        /// </summary>
        /// <param name="report">Admin report to check.</param>
        public void SendAdminEmails(StackHashAdminReport report)
        {
            if (m_Disposed)
                throw new ObjectDisposedException("MailManager");
            if (report == null)
                throw new ArgumentNullException("report");

            Monitor.Enter(this);

            try
            {
                // Only send an email if the operation is of interest.
                if (this.m_MailSettings.OperationsToReport.Contains(report.Operation))
                {
                    // Don't report sync started if a retry.
                    if ((report.Operation == StackHashAdminOperation.WinQualSyncStarted) && report.IsRetry)
                        return;

                    // Only report retry sync completes if succeeded.
                    if ((report.Operation == StackHashAdminOperation.WinQualSyncCompleted) && 
                         report.IsRetry && 
                        (report.ServiceErrorCode != StackHashServiceErrorCode.NoError))
                        return;

                    // Send an email as specified.
                    String subject = "StackHash report: " + StackHashAdminOperationCollection.GetFriendlyName(report.Operation);

                    StringBuilder message = new StringBuilder();
                    message.AppendLine("Service Local Time: " + DateTime.Now.ToString());
                    message.AppendLine("Service Profile: " + m_ProfileName);
                    message.Append(report.ToString());

                    if (!String.IsNullOrEmpty(report.Description))
                        message.AppendLine(report.Description);

                    message.AppendLine("---");
                    message.AppendLine("www.stackhash.com");

                    sendEmails(subject, message.ToString());
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }        


        /// <summary>
        /// Sends the specified email details to the registered users.
        /// </summary>
        /// <param name="subject">Subject field for the email.</param>
        /// <param name="message">Message contents.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private void sendEmails(String subject, String message)
        {
            if (m_SmtpClient == null)
                recycleSmtpClient();

            if (m_SmtpClient == null)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Failed to create email client.");
                return;
            }

            try
            {
                m_SmtpClient.Send(this.m_MailSettings.SmtpSettings.SmtpFrom,
                    this.m_MailSettings.SmtpSettings.SmtpRecipients,
                    subject,
                    message);
            }
            catch (System.Exception ex)
            {
                // dispose the SMTP client on failure
                m_SmtpClient.Dispose();
                m_SmtpClient = null;

                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to send email: ", ex);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of all resources.
        /// </summary>
        /// <param name="disposing">True - disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_SmtpClient != null)
                {
                    m_SmtpClient.Dispose();
                    m_SmtpClient = null;
                    m_Disposed = true;
                }
            }
        }

        /// <summary>
        /// Dispose of managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
