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
    public class StackHashAnalysisSettings
    {
        bool m_ForceRerun;

        public StackHashAnalysisSettings() { ; }

        [DataMember]
        public bool ForceRerun
        {
            get { return m_ForceRerun; }
            set { m_ForceRerun = value; }
        }
    }
}

