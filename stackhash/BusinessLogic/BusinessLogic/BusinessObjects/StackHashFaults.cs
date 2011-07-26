using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [SuppressMessage("Microsoft.Design", "CA1002")]
    public class ReceiverFaultDetail
    {
        private StackHashServiceErrorCode m_ServiceErrorCode;
        private String m_Message;
        private String m_Description;

        public ReceiverFaultDetail() { ; }
        
        public ReceiverFaultDetail(String message, StackHashServiceErrorCode serviceErrorCode)
            : this(message, "", serviceErrorCode)
        {
        }

        public ReceiverFaultDetail(String message, String description, StackHashServiceErrorCode serviceErrorCode)
        {
            this.m_ServiceErrorCode = serviceErrorCode;
            this.m_Message = message;
            this.m_Description = description;
        }

        [DataMember(Name = "Message", IsRequired = true, Order = 0)]
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        [DataMember(Name = "Description", IsRequired = false, Order = 1)]
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [DataMember(Name = "ServiceErrorCode", IsRequired = true, Order = 2)]
        public StackHashServiceErrorCode ServiceErrorCode
        {
            get { return m_ServiceErrorCode; }
            set { m_ServiceErrorCode = value; }
        }

    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [SuppressMessage("Microsoft.Design", "CA1002")]
    public class SenderFaultDetail
    {
        private string m_message;
        private string m_description;
        private List<string> m_failedBodyElements = new List<string>();


        public SenderFaultDetail() { ; }

        public SenderFaultDetail(string message, List<string> bodyElements)
            : this(message, "", bodyElements)
        {
        }

        public SenderFaultDetail(string message)
            : this(message, "", null)
        {
        }

        public SenderFaultDetail(string message, string description, List<string> bodyElements)
        {
            this.m_message = message;
            this.m_description = description;

            if (bodyElements != null)
                this.m_failedBodyElements = bodyElements;
        }

        [DataMember(Name = "Message", IsRequired = true, Order = 0)]
        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        [DataMember(Name = "Description", IsRequired = false, Order = 1)]
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        [DataMember(Name = "FailedBodyElements", IsRequired = true, Order = 2)]
        public List<string> FailedBodyElements
        {
            get { return m_failedBodyElements; }
            set { m_failedBodyElements = value; }
        }

    }
}
