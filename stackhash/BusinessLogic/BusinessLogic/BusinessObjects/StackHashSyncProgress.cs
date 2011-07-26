using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashSyncPhase
    {
        [EnumMember()]
        Unknown = 0,

        [EnumMember()]
        ProductsOnly,

        [EnumMember()]
        Events,

        [EnumMember()]
        EventInfosAndCabs
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSyncProgress : IComparable<StackHashSyncProgress>
    {
        int m_ProductId;
        int m_FileId;
        int m_EventId;
        int m_CabId;
        String m_EventTypeName;
        StackHashSyncPhase m_SyncPhase;

        public StackHashSyncProgress() { ; }

        public StackHashSyncProgress(int productId, int fileId, int eventId, String eventTypeName, int cabId, StackHashSyncPhase syncPhase)
        {
            m_ProductId = productId;
            m_FileId = fileId;
            m_EventId = eventId;
            m_CabId = cabId;
            m_EventTypeName = eventTypeName;
            m_SyncPhase = syncPhase;
        }

        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        [DataMember]
        public int FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }

        [DataMember]
        public int EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }

        [DataMember]
        public String EventTypeName
        {
            get { return m_EventTypeName; }
            set { m_EventTypeName = value; }
        }

        [DataMember]
        public int CabId
        {
            get { return m_CabId; }
            set { m_CabId = value; }
        }

        [DataMember]
        public StackHashSyncPhase SyncPhase
        {
            get { return m_SyncPhase; }
            set { m_SyncPhase = value; }
        }

        #region IComparable<StackHashSyncProgress> Members

        public int CompareTo(StackHashSyncProgress other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (m_ProductId != other.ProductId)
                return -1;
            if (m_FileId != other.FileId)
                return -1;
            if (m_EventId != other.EventId)
                return -1;
            if (m_CabId != other.CabId)
                return -1;
            if (m_EventTypeName != other.EventTypeName)
                return -1;
            if (m_SyncPhase != other.SyncPhase)
                return -1;

            return 0;
        }

        #endregion
    }
}
