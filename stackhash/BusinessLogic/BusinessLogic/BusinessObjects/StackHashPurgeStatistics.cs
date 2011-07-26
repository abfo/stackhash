using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashPurgeStatistics
    {
        private int m_NumberOfCabs;
        private long m_CabsTotalSize;
        private int m_NumberOfDumpFiles;
        private long m_DumpFilesTotalSize;

        public StackHashPurgeStatistics() {;}
        public StackHashPurgeStatistics(int numberOfCabs, long cabsTotalSize, int numberOfDumps, long dumpsTotalSize)
        {
            m_NumberOfCabs = numberOfCabs;
            m_CabsTotalSize = cabsTotalSize;
            m_NumberOfDumpFiles = numberOfDumps;
            m_DumpFilesTotalSize = dumpsTotalSize;
        }

        [DataMember]
        public int NumberOfCabs 
        {
            get { return m_NumberOfCabs; }
            set { m_NumberOfCabs = value; }
        }

        [DataMember]
        public long CabsTotalSize
        {
            get { return m_CabsTotalSize; }
            set { m_CabsTotalSize = value; }
        }
        [DataMember]
        public int NumberOfDumpFiles
        {
            get { return m_NumberOfDumpFiles; }
            set { m_NumberOfDumpFiles = value; }
        }
        [DataMember]
        public long DumpFilesTotalSize
        {
            get { return m_DumpFilesTotalSize; }
            set { m_DumpFilesTotalSize = value; }
        }

        public void Add(StackHashPurgeStatistics statisticsToAdd)
        {
            if (statisticsToAdd == null)
                throw new ArgumentNullException("statisticsToAdd");

            m_NumberOfCabs += statisticsToAdd.NumberOfCabs;
            m_CabsTotalSize += statisticsToAdd.CabsTotalSize;
            m_NumberOfDumpFiles += statisticsToAdd.NumberOfDumpFiles;
            m_DumpFilesTotalSize += statisticsToAdd.DumpFilesTotalSize;
        }

        public void Subtract(StackHashPurgeStatistics statisticsToSubtract)
        {
            if (statisticsToSubtract == null)
                throw new ArgumentNullException("statisticsToSubtract");

            m_NumberOfCabs += statisticsToSubtract.NumberOfCabs;
            m_CabsTotalSize += statisticsToSubtract.CabsTotalSize;
            m_NumberOfDumpFiles += statisticsToSubtract.NumberOfDumpFiles;
            m_DumpFilesTotalSize += statisticsToSubtract.DumpFilesTotalSize;
        }

        public StackHashPurgeStatistics Clone()
        {
            StackHashPurgeStatistics newStats = new StackHashPurgeStatistics(
                m_NumberOfCabs, m_CabsTotalSize, m_NumberOfDumpFiles, m_DumpFilesTotalSize);
            return newStats;
        }

    }
}
