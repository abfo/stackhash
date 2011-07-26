using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;


namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashDumpAnalysis : ICloneable
    {
        private String m_SystemUpTime;
        private String m_ProcessUpTime;
        private String m_DotNetVersion;
        private String m_OSVersion;
        private String m_MachineArchitecture;

        [DataMember]
        public String SystemUpTime
        {
            get { return m_SystemUpTime; }
            set { m_SystemUpTime = value; }
        }

        [DataMember]
        public String ProcessUpTime
        {
            get { return m_ProcessUpTime; }
            set { m_ProcessUpTime = value; }
        }

        [DataMember]
        public String DotNetVersion
        {
            get { return m_DotNetVersion; }
            set { m_DotNetVersion = value; }
        }

        [DataMember]
        public String OSVersion
        {
            get { return m_OSVersion; }
            set { m_OSVersion = value; }
        }

        [DataMember]
        public String MachineArchitecture
        {
            get { return m_MachineArchitecture; }
            set { m_MachineArchitecture = value; }
        }

        public StackHashDumpAnalysis() { ; } // Required for serialization.

        public StackHashDumpAnalysis(String systemUpTime, String processUpTime, String dotNetVersion, String osVersion, String machineArchitecture)
        {
            m_SystemUpTime = systemUpTime;
            m_ProcessUpTime = processUpTime;
            m_DotNetVersion = dotNetVersion;
            m_OSVersion = osVersion;
            m_MachineArchitecture = machineArchitecture;
        }


        #region ICloneable Members

        public object Clone()
        {
            return new StackHashDumpAnalysis(this.SystemUpTime, this.ProcessUpTime, this.DotNetVersion, this.OSVersion, this.MachineArchitecture);
        }

        #endregion
    }
}