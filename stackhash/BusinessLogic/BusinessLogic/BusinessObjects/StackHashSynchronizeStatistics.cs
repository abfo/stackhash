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
    public class StackHashSynchronizeStatistics
    {
        private int m_Products;
        private int m_Files;
        private int m_Events;
        private int m_EventInfos;
        private int m_Cabs;

        public StackHashSynchronizeStatistics() {;}
        public StackHashSynchronizeStatistics(int products, int files, int events, int eventInfos, int cabs)
        {
            m_Products = products;
            m_Files = files;
            m_Events = events;
            m_EventInfos = eventInfos;
            m_Cabs = cabs;
        }

        [DataMember]
        public int Products 
        {
            get { return m_Products; }
            set { m_Products = value; }
        }
        [DataMember]
        public int Files
        {
            get { return m_Files; }
            set { m_Files = value; }
        }
        [DataMember]
        public int Events
        {
            get { return m_Events; }
            set { m_Events = value; }
        }
        [DataMember]
        public int EventInfos
        {
            get { return m_EventInfos; }
            set { m_EventInfos = value; }
        }
        [DataMember]
        public int Cabs
        {
            get { return m_Cabs; }
            set { m_Cabs = value; }
        }

        public void Add(StackHashSynchronizeStatistics statisticsToAdd)
        {
            if (statisticsToAdd == null)
                throw new ArgumentNullException("statisticsToAdd");

            m_Products += statisticsToAdd.Products;
            m_Files += statisticsToAdd.Files;
            m_Events += statisticsToAdd.Events;
            m_EventInfos += statisticsToAdd.EventInfos;
            m_Cabs += statisticsToAdd.Cabs;
        }

        public void Subtract(StackHashSynchronizeStatistics statisticsToSubtract)
        {
            if (statisticsToSubtract == null)
                throw new ArgumentNullException("statisticsToSubtract");

            m_Products -= statisticsToSubtract.Products;
            m_Files -= statisticsToSubtract.Files;
            m_Events -= statisticsToSubtract.Events;
            m_EventInfos -= statisticsToSubtract.EventInfos;
            m_Cabs -= statisticsToSubtract.Cabs;
        }

        public StackHashSynchronizeStatistics Clone()
        {
            StackHashSynchronizeStatistics newStats = new StackHashSynchronizeStatistics(
                m_Products, m_Files, m_Events, m_EventInfos, m_Cabs);
            return newStats;
        }

    }
}
