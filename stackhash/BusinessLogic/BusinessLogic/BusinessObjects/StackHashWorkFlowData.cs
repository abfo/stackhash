using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashWorkFlowData
    {
        private StackHashMappingCollection m_WorkFlowMappings;

        public StackHashWorkFlowData() { ; }

        [DataMember]
        public StackHashMappingCollection WorkFlowMappings
        {
            get { return m_WorkFlowMappings; }
            set { m_WorkFlowMappings = value; }
        }
    }
}

