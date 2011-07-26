using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashEventPackage
    {
        private StackHashEventInfoCollection m_EventInfoList;
        private StackHashCabPackageCollection m_Cabs;
        private StackHashEvent m_EventData;
        private int m_ProductId;
        private long m_RowNumber;
        private int m_CriteriaMatchMap;


        public StackHashEventPackage() { ;}  // Needed to serialize;

        public StackHashEventPackage(StackHashEventInfoCollection eventInfoList, StackHashCabPackageCollection cabs, 
            StackHashEvent eventData, int productId)
        {
            m_EventInfoList = eventInfoList;
            m_Cabs = cabs;
            m_EventData = eventData;
            m_ProductId = productId;
            m_CriteriaMatchMap = 0;
        }

        [DataMember]
        public StackHashEventInfoCollection EventInfoList
        {
            get { return m_EventInfoList; }
            set { m_EventInfoList = value; }
        }

        [DataMember]
        public StackHashCabPackageCollection Cabs
        {
            get { return m_Cabs; }
            set { m_Cabs = value; }
        }

        [DataMember]
        public StackHashEvent EventData
        {
            get { return m_EventData; }
            set { m_EventData = value; }
        }

        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        /// <summary>
        /// Only used when sorted searches are used.
        /// </summary>
        [DataMember]
        public long RowNumber
        {
            get { return m_RowNumber; }
            set { m_RowNumber = value; }
        }

        /// <summary>
        /// Bitmap of which search criterias matched this event.
        /// </summary>
        [DataMember]
        public int CriteriaMatchMap
        {
            get { return m_CriteriaMatchMap; }
            set { m_CriteriaMatchMap = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashEventPackageCollection : Collection<StackHashEventPackage>, IComparable
    {
        long m_MaximumRowNumber;
        long m_MinimumRowNumber;
        long m_TotalRows;
        long m_MaximumSqlRows;

        public StackHashEventPackageCollection() { m_MinimumRowNumber = Int32.MaxValue; } // Needed to serialize.

        [DataMember]
        public long MaximumRowNumber
        {
            get { return m_MaximumRowNumber; }
            set { m_MaximumRowNumber = value; }
        }

        [DataMember]
        public long MinimumRowNumber
        {
            get { return m_MinimumRowNumber; }
            set { m_MinimumRowNumber = value; }
        }

        [DataMember]
        public long TotalRows
        {
            get { return m_TotalRows; }
            set { m_TotalRows = value; }
        }

        [DataMember]
        public long MaximumSqlRows
        {
            get { return m_MaximumSqlRows; }
            set { m_MaximumSqlRows = value; }
        }

        /// <summary>
        /// Locates the event package with all fields matching.
        /// </summary>
        /// <param name="eventId">Event ID to find.</param>
        /// <returns>Found event info or null.</returns>
        public StackHashEventPackage FindEventPackage(int eventId, String eventTypeName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if ((this[i].EventData.Id == eventId) &&
                    (this[i].EventData.EventTypeName == eventTypeName))
                    return this[i];
            }
            return null;
        }

        protected override void InsertItem(int index, StackHashEventPackage item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.RowNumber < MinimumRowNumber)
                MinimumRowNumber = item.RowNumber;
            if (item.RowNumber > MaximumRowNumber)
                MaximumRowNumber = item.RowNumber;
            TotalRows++;
            base.InsertItem(index, item);
        }
        
        /// <summary>
        /// Adds all events from the specified collection to the end of this one.
        /// </summary>
        /// <param name="eventsToMerge">Events to merge.</param>
        /// <param name="fromStart">true - copy from start of collection, false - copy from end of collection.</param>
        /// <param name="maxEventsToMerge">Maximum events to merge.</param>
        public StackHashEventPackageCollection MergeEvents(StackHashEventPackageCollection eventsToMerge, bool fromStart, long maxEventsToMerge)
        {
            if (eventsToMerge == null)
                throw new ArgumentNullException("eventsToMerge");

            long numEvents = maxEventsToMerge;
            if (numEvents > eventsToMerge.Count)
                numEvents = eventsToMerge.Count;

            if (fromStart)
            {
                for (int i = 0; i < numEvents; i++)
                {
                    if (eventsToMerge[i].RowNumber > this.MaximumRowNumber)
                        this.MaximumRowNumber = eventsToMerge[i].RowNumber;
                    if (eventsToMerge[i].RowNumber < this.MinimumRowNumber)
                        this.MinimumRowNumber = eventsToMerge[i].RowNumber;

                    this.Add(eventsToMerge[i]);
                }
            }
            else
            {
                // Merge at the start of this array - but only the specified number of entries.
                int added = 0;
                for (int i = eventsToMerge.Count - 1; i >= 0; i--)
                {
                    if (added >= maxEventsToMerge)
                        break;
                    if (this.FindEventPackage(eventsToMerge[i].EventData.Id, eventsToMerge[i].EventData.EventTypeName) == null)
                    {
                        this.InsertItem(0, eventsToMerge[i]);
                        added++;
                    }
                }   
            }
            return this;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashEventPackageCollection events = obj as StackHashEventPackageCollection;

            if (events.Count != this.Count)
                return -1;

            foreach (StackHashEventPackage thisEventPackage in this)
            {
                StackHashEventPackage matchingEventInfo = 
                    events.FindEventPackage(thisEventPackage.EventData.Id, thisEventPackage.EventData.EventTypeName);

                if (matchingEventInfo == null)
                    return -1;

                // Check the event data and the event info and cab data.
                if (matchingEventInfo.EventData.CompareTo(thisEventPackage.EventData) != 0)
                    return -1;

                if (matchingEventInfo.EventInfoList.CompareTo(thisEventPackage.EventInfoList) != 0)
                    return -1;

                if (matchingEventInfo.Cabs.CompareTo(thisEventPackage.Cabs) != 0)
                    return -1;
            }

            // Must be a match.
            return 0;
        }

        #endregion
    }
}
