using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackHashBusinessObjects;
using StackHashServiceContracts;

namespace StackHashServiceImplementation
{
    public class RegisteredClient
    {
        private StackHashClientData m_ClientData;
        private IAdminNotificationEvents m_ClientCallback;
        private DateTime m_LastAccessTime;
        private DateTime m_FirstRegisteredTime;

        public RegisteredClient(StackHashClientData clientData, IAdminNotificationEvents clientCallback, DateTime lastAccessTime, DateTime firstRegisteredTime)
        {
            m_ClientData = clientData;
            m_ClientCallback = clientCallback;
            m_LastAccessTime = lastAccessTime;
            m_FirstRegisteredTime = firstRegisteredTime;
        }

        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        public IAdminNotificationEvents ClientCallback
        {
            get { return m_ClientCallback; }
            set { m_ClientCallback = value; }
        }

        public DateTime LastAccessTime
        {
            get { return m_LastAccessTime; }
            set { m_LastAccessTime = value; }
        }

        public DateTime FirstRegisteredTime
        {
            get { return m_FirstRegisteredTime; }
            set { m_FirstRegisteredTime = value; }
        }
    }
}
