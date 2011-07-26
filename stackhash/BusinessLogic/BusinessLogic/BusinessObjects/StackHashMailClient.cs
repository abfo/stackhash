using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    /// <summary>
    /// Contains the information used to mail a client when admin reports are generated.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSmtpSettings
    {
        private String m_SmtpHost;
        private const String s_SmtpHostDefault = "";
        private const String s_SmtpHostDescription = "SMTP Hostname:";

        private int m_SmtpPort;
        private const int s_SmtpPortDefault = 25;
        private const string s_SmtpPortDescription = "SMTP Port:";

        private String m_SmtpUsername;
        private const string s_SmtpUsernameDefault = "";
        private const string s_SmtpUsernameDescription = "SMTP Username:";

        private String m_SmtpPassword;
        private const string s_SmtpPasswordDefault = "";
        private const string s_SmtpPasswordDescription = "SMTP Password:";

        private String m_SmtpRecipients;
        private const string s_SmtpRecipientsDefault = "";
        private const string s_SmtpRecipientsDescription = "Email To (comma separated):";

        private String m_SmtpFrom;
        private const string s_SmtpFromDefault = "";
        private const string s_SmtpFromDescription = "Email From:";


        public StackHashSmtpSettings()   // Required for serialization.
        {
            m_SmtpHost = s_SmtpHostDefault;
            m_SmtpPort = s_SmtpPortDefault;
            m_SmtpUsername = s_SmtpUsernameDefault;
            m_SmtpPassword = s_SmtpPasswordDefault;
            m_SmtpRecipients = s_SmtpRecipientsDefault;
            m_SmtpFrom = s_SmtpFromDefault;
        }  

        /// <summary>
        /// SMTP mail server host.
        /// </summary>
        [DataMember]
        public String SmtpHost
        {
            get { return m_SmtpHost; }
            set { m_SmtpHost = value; }
        }

        /// <summary>
        /// SMTP mail server port.
        /// </summary>
        [DataMember]
        public int SmtpPort
        {
            get { return m_SmtpPort; }
            set { m_SmtpPort = value; }
        }

        /// <summary>
        /// SMTP user name.
        /// </summary>
        [DataMember]
        public String SmtpUsername
        {
            get { return m_SmtpUsername; }
            set { m_SmtpUsername = value; }
        }

        /// <summary>
        /// SMTP password.
        /// </summary>
        [DataMember]
        public String SmtpPassword
        {
            get { return m_SmtpPassword; }
            set { m_SmtpPassword = value; }
        }

        /// <summary>
        /// Recipients of the email.
        /// </summary>
        [DataMember]
        public String SmtpRecipients
        {
            get { return m_SmtpRecipients; }
            set { m_SmtpRecipients = value; }
        }

        /// <summary>
        /// The From field of all emails.
        /// </summary>
        [DataMember]
        public String SmtpFrom
        {
            get { return m_SmtpFrom; }
            set { m_SmtpFrom = value; }
        }    
    }


    /// <summary>
    /// Contains the information used to mail a client when admin reports are generated.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashEmailSettings
    {
        StackHashSmtpSettings m_SmtpSettings;
        StackHashAdminOperationCollection m_OperationsToReport;

        public StackHashEmailSettings()   // Required for serialization.
        {
            m_SmtpSettings = new StackHashSmtpSettings();
            m_OperationsToReport = new StackHashAdminOperationCollection();
        }

        /// <summary>
        /// Email settings.
        /// </summary>
        [DataMember]
        public StackHashSmtpSettings SmtpSettings
        {
            get { return m_SmtpSettings; }
            set { m_SmtpSettings = value; }
        }

        /// <summary>
        /// The admin operations that will be reported.
        /// Some operations are not reported.
        /// </summary>
        [DataMember]
        public StackHashAdminOperationCollection OperationsToReport
        {
            get { return m_OperationsToReport; }
            set { m_OperationsToReport = value; }
        }
    }

}
