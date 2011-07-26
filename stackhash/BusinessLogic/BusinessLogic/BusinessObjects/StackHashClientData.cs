using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashClientData
    {
        private Guid m_ApplicationGuid;
        private String m_ClientName;
        private int m_ClientId;
        private int m_ClientRequestId;
        private String m_ServiceGuid;


        public StackHashClientData() { ;}

        [SuppressMessage("Microsoft.Naming", "CA1720")]
        public StackHashClientData(Guid applicationGuid, String name, int clientId)
        {
            m_ApplicationGuid = applicationGuid;
            m_ClientName = name;
            m_ClientId = clientId;
        }

        [DataMember]
        public Guid ApplicationGuid
        {
            get { return m_ApplicationGuid; }
            set { m_ApplicationGuid = value; }
        }

        [DataMember]
        public String ClientName
        {
            get { return m_ClientName; }
            set { m_ClientName = value; }
        }

        [DataMember]
        public int ClientId
        {
            get { return m_ClientId; }
            set { m_ClientId = value; }
        }

        [DataMember]
        public int ClientRequestId
        {
            get { return m_ClientRequestId; }
            set { m_ClientRequestId = value; }
        }

        [DataMember]
        public String ServiceGuid
        {
            get { return m_ServiceGuid; }
            set { m_ServiceGuid = value; }
        }
    }
}
