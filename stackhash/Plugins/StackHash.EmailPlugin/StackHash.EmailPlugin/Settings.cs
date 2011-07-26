using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using StackHashBugTrackerInterfaceV1;
using System.Threading;

namespace StackHash.EmailPlugin
{
    /// <summary>
    /// Settings wrapper - provides strongly typed settings with methods to convert to and from
    /// the NameValueCollection format required by StackHash
    /// </summary>
    sealed class Settings : IDisposable
    {
        private SmtpClient m_smtp;
        private bool m_disposed;

        /// <summary>
        /// Gets the SMTP server host
        /// </summary>
        public string SmtpHost { get; private set; }
        private const string SmtpHostDefault = "";
        private const string SmtpHostDescription = "SMTP Hostname:";

        /// <summary>
        /// Gets the SMTP server port
        /// </summary>
        public int SmtpPort { get; private set; }
        private const int SmtpPortDefault = 25;
        private const string SmtpPortDescription = "SMTP Port:";

        /// <summary>
        /// Gets the SMTP server username
        /// </summary>
        public string SmtpUsername { get; private set; }
        private const string SmtpUsernameDefault = "";
        private const string SmtpUsernameDescription = "SMTP Username:";

        /// <summary>
        /// Gets the SMTP server password
        /// </summary>
        public string SmtpPassword { get; private set; }
        private const string SmtpPasswordDefault = "";
        private const string SmtpPasswordDescription = "SMTP Password:";

        /// <summary>
        /// Gets a comma separated list of recipients for emails
        /// </summary>
        public string SmtpRecipients { get; private set; }
        private const string SmtpRecipientsDefault = "";
        private const string SmtpRecipientsDescription = "Email To (comma separated):";

        /// <summary>
        /// Gets the from address for email
        /// </summary>
        public string SmtpFrom { get; private set; }
        private const string SmtpFromDefault = "";
        private const string SmtpFromDescription = "Email From:";

        /// <summary>
        /// True if the plugin should only send manual reports
        /// </summary>
        public bool IsManualOnly { get; private set; }
        private const bool IsManualOnlyDefault = true;
        private const string IsManualOnlyDescription = "Manual mode (True/False):";

        /// <summary>
        /// True if the plugin should report new products
        /// </summary>
        public bool ReportProducts { get; private set; }
        private const bool ReportProductsDefault = true;
        private const string ReportProductsDescription = "Report new products (True/False):";

        /// <summary>
        /// Attempts to send an email using current settings
        /// </summary>
        /// <param name="subject">Email subject</param>
        /// <param name="message">Email message</param>
        public void SendEmail(string subject, string message)
        {
            if (m_disposed)
                throw new ObjectDisposedException("Settings");

            if (subject == null)
                throw new ArgumentNullException("subject");

            if (message == null)
                throw new ArgumentNullException("message");

            if (m_smtp == null)
                RecycleSmtpClient();

            if (m_smtp == null)
                throw new InvalidOperationException("Faild to create SMTP Client");

            try
            {
                m_smtp.Send(this.SmtpFrom,
                    this.SmtpRecipients,
                    subject,
                    message);
            }
            catch
            {
                // dispose the SMTP client on failure
                m_smtp.Dispose();
                m_smtp = null;

                throw;
            }
        }

        /// <summary>
        /// Converts the Settings object to a NameValueCollection
        /// </summary>
        /// <returns>NameValueCollection</returns>
        public NameValueCollection ToNameValueCollection()
        {
            if (m_disposed)
                throw new ObjectDisposedException("Settings");

            NameValueCollection nameValueCollection = new NameValueCollection();

            nameValueCollection[SmtpHostDescription] = this.SmtpHost;
            nameValueCollection[SmtpPortDescription] = this.SmtpPort.ToString(CultureInfo.InvariantCulture);
            nameValueCollection[SmtpUsernameDescription] = this.SmtpUsername;
            nameValueCollection[SmtpPasswordDescription] = this.SmtpPassword;
            nameValueCollection[SmtpRecipientsDescription] = this.SmtpRecipients;
            nameValueCollection[SmtpFromDescription] = this.SmtpFrom;
            nameValueCollection[IsManualOnlyDescription] = this.IsManualOnly.ToString();
            nameValueCollection[ReportProductsDescription] = this.ReportProducts.ToString();

            return nameValueCollection;
        }

        /// <summary>
        /// Gets a NameValueCollection containing the default settings
        /// </summary>
        /// <returns>NameValueCollection containing default settings</returns>
        public static NameValueCollection GetDefaults()
        {
            return new Settings().ToNameValueCollection();
        }

        /// <summary>
        /// Creates a Settings object from a NameValueCollection
        /// </summary>
        /// <param name="nameValueCollection">NameValueCollection</param>
        /// <returns>Settings object</returns>
        public static Settings FromNameValueCollection(NameValueCollection nameValueCollection)
        {
            if (nameValueCollection == null)
                throw new ArgumentNullException("nameValueCollection");

            Settings settings = new Settings();

            settings.SmtpHost = nameValueCollection[SmtpHostDescription];

            if (!string.IsNullOrEmpty(nameValueCollection[SmtpPortDescription]))
            {
                settings.SmtpPort = Convert.ToInt32(nameValueCollection[SmtpPortDescription], CultureInfo.InvariantCulture);
            }

            settings.SmtpUsername = nameValueCollection[SmtpUsernameDescription];
            settings.SmtpPassword = nameValueCollection[SmtpPasswordDescription];
            settings.SmtpRecipients = nameValueCollection[SmtpRecipientsDescription];
            settings.SmtpFrom = nameValueCollection[SmtpFromDescription];

            if (!string.IsNullOrEmpty(nameValueCollection[IsManualOnlyDescription]))
            {
                settings.IsManualOnly = Convert.ToBoolean(nameValueCollection[IsManualOnlyDescription]);
            }

            if (!string.IsNullOrEmpty(nameValueCollection[ReportProductsDescription]))
            {
                settings.ReportProducts = Convert.ToBoolean(nameValueCollection[ReportProductsDescription]);
            }

            return settings;
        }

        /// <summary>
        /// Settings wrapper - provides strongly typed settings with methods to convert to and from
        /// the NameValueCollection format required by StackHash
        /// </summary>
        public Settings()
        {
            this.SmtpHost = SmtpHostDefault;
            this.SmtpPort = SmtpPortDefault;
            this.SmtpUsername = SmtpUsernameDefault;
            this.SmtpPassword = SmtpPasswordDefault;
            this.SmtpRecipients = SmtpRecipientsDefault;
            this.SmtpFrom = SmtpFromDefault;
            this.IsManualOnly = IsManualOnlyDefault;
            this.ReportProducts = ReportProductsDefault;
        }

        /// <summary />
        ~Settings()
        {
            Dispose(false);
        }

        private void RecycleSmtpClient()
        {
            if (m_smtp != null)
            {
                m_smtp.Dispose();
                m_smtp = null;
            }

            // must have host
            if (string.IsNullOrEmpty(this.SmtpHost))
                return;

            m_smtp = new SmtpClient(this.SmtpHost, this.SmtpPort);

            if ((!string.IsNullOrEmpty(this.SmtpUsername)) &&
                (!string.IsNullOrEmpty(this.SmtpPassword)))
            {
                // add credentials if specified
                NetworkCredential credentials = new NetworkCredential(this.SmtpUsername, this.SmtpPassword);
                m_smtp.Credentials = credentials;
            }
        }

        #region IDisposable Members

        /// <summary />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool canDisposeManagedResorces)
        {
            if (!m_disposed)
            {
                if (canDisposeManagedResorces)
                {
                    if (m_smtp != null)
                    {
                        m_smtp.Dispose();
                        m_smtp = null;
                    }
                }

                m_disposed = true;
            }
        }

        #endregion
    }
}
