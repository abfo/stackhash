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
    public class StackHashControlData 
    {
        private int  m_DatabaseVersion;
        private int  m_SyncCount;
        private StackHashSyncProgress m_LastSyncProgress;

        [DataMember]
        public int DatabaseVersion
        {
            get { return m_DatabaseVersion; }
            set { m_DatabaseVersion = value; }
        }

        [DataMember]
        public int SyncCount
        {
            get { return m_SyncCount; }
            set { m_SyncCount = value; }
        }

        [DataMember]
        public StackHashSyncProgress LastSyncProgress
        {
            get { return m_LastSyncProgress; }
            set { m_LastSyncProgress = value; }
        }

        
        public StackHashControlData() { ; } // Required for serialization.

        public StackHashControlData(int databaseVersion, int syncCount, StackHashSyncProgress lastSyncProgress)
        {
            m_DatabaseVersion = databaseVersion;
            m_SyncCount = syncCount;
            m_LastSyncProgress = lastSyncProgress;
        }
    }
}