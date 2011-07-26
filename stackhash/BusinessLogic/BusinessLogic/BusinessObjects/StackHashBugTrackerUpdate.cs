using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackHashBusinessObjects
{
    public enum StackHashDataChanged
    {
        Product,
        File,
        Event,
        EventNote,
        Hit,
        Cab,
        CabNote,
        DebugScript
    }

    public enum StackHashChangeType
    {
        NewEntry,
        UpdatedEntry
    }

    
    public class StackHashBugTrackerUpdate : IComparable<StackHashBugTrackerUpdate>
    {
        private int m_EntryId;
        private DateTime m_DateChanged;
        private StackHashDataChanged m_DataThatChanged;
        private StackHashChangeType m_TypeOfChange;

        private long m_ProductId;
        private long m_FileId;
        private long m_EventId;
        private String m_EventTypeName;
        private long m_CabId;
        private long m_ChangedObjectId;

        public StackHashBugTrackerUpdate() { ; }

        public StackHashBugTrackerUpdate(StackHashDataChanged dataThatChanged, StackHashChangeType typeOfChange,
            long productId, long fileId, long eventId, String eventTypeName, long cabId, long changedObjectId)
        {
            m_DateChanged = DateTime.Now.ToUniversalTime();
            m_DataThatChanged = dataThatChanged;
            m_TypeOfChange = typeOfChange;
            m_ProductId = productId;
            m_FileId = fileId;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
            m_CabId = cabId;
            m_ChangedObjectId = changedObjectId;
        }

        public int EntryId
        {
            get { return m_EntryId; }
            set { m_EntryId = value; }
        }

        public DateTime DateChanged
        {
            get { return m_DateChanged; }
            set { m_DateChanged = value; }
        }

        public StackHashDataChanged DataThatChanged
        {
            get { return m_DataThatChanged; }
            set { m_DataThatChanged = value; }
        }

        public StackHashChangeType TypeOfChange
        {
            get { return m_TypeOfChange; }
            set { m_TypeOfChange = value; }
        }

        public long ProductId 
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        public long FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }

        public long EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }

        public String EventTypeName
        {
            get { return m_EventTypeName; }
            set { m_EventTypeName = value; }
        }

        public long CabId
        {
            get { return m_CabId; }
            set { m_CabId = value; }
        }

        public long ChangedObjectId
        {
            get { return m_ChangedObjectId; }
            set { m_ChangedObjectId = value; }
        }


        #region IComparable<StackHashBugTrackerUpdate> Members

        public int CompareTo(StackHashBugTrackerUpdate other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (EntryId != other.EntryId)
                return -1;
            if (DateChanged != other.DateChanged)
                return -1;
            if (DataThatChanged != other.DataThatChanged)
                return -1;
            if (TypeOfChange != other.TypeOfChange)
                return -1;
            if (ProductId != other.ProductId)
                return -1;
            if (FileId != other.FileId)
                return -1;
            if (EventId != other.EventId)
                return -1;
            if (EventTypeName != other.EventTypeName)
                return -1;
            if (CabId != other.CabId)
                return -1;
            if (ChangedObjectId != other.ChangedObjectId)
                return -1;

            return 0;
        }

        #endregion
    }
}
