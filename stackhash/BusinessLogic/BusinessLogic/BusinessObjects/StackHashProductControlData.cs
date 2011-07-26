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
    public class StackHashProductControlData 
    {
        private int m_ProductId;
        private DateTime  m_LastSyncTime;
        private DateTime  m_LastSyncCompletedTime;
        private DateTime  m_LastSyncStartedTime;
        private DateTime  m_LastHitTime;
        private bool  m_SyncActive;

        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        [DataMember]
        public DateTime LastSyncTime
        {
            get { return m_LastSyncTime; }
            set { m_LastSyncTime = value; }
        }

        [DataMember]
        public DateTime LastSyncCompletedTime
        {
            get { return m_LastSyncCompletedTime; }
            set { m_LastSyncCompletedTime = value; }
        }

        [DataMember]
        public DateTime LastSyncStartedTime
        {
            get { return m_LastSyncStartedTime; }
            set { m_LastSyncStartedTime = value; }
        }

        [DataMember]
        public DateTime LastHitTime
        {
            get { return m_LastHitTime; }
            set { m_LastHitTime = value; }
        }

        [DataMember]
        public bool SyncActive
        {
            get { return m_SyncActive; }
            set { m_SyncActive = value; }
        }
        
        
        public StackHashProductControlData() { ; } // Required for serialization.

        public StackHashProductControlData(int productId, DateTime lastSyncTime, DateTime lastSyncCompletedTime, DateTime lastSyncStartedTime, DateTime lastHitTime, bool syncActive)
        {
            m_ProductId = productId;
            m_LastSyncTime = lastSyncTime;
            m_LastSyncCompletedTime = lastSyncCompletedTime;
            m_LastSyncStartedTime = lastSyncStartedTime;
            m_LastHitTime = lastHitTime;
            m_SyncActive = syncActive;
        }
    }
}