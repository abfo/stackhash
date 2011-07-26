using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;

namespace StackHashBugTrackerInterfaceV1
{
    [Serializable]
    public class BugTrackerCab
    {
        long m_CabId;
        long m_SizeInBytes;
        bool m_CabDownloaded;
        bool m_CabPurged;
        NameValueCollection m_AnalysisData;
        String m_CabPathAndFileName;

        public BugTrackerCab(long cabId, long sizeInBytes, bool cabDownloaded, bool cabPurged, NameValueCollection analysisData, String cabPathAndFileName)
        {
            m_CabId = cabId;
            m_SizeInBytes = sizeInBytes;
            m_CabDownloaded = cabDownloaded;
            m_CabPurged = cabPurged;
            m_AnalysisData = analysisData;
            m_CabPathAndFileName = cabPathAndFileName;
        }

        public long CabId
        {
            get { return m_CabId; }
        }

        public long SizeInBytes
        {
            get { return m_SizeInBytes; }
        }

        public bool CabDownloaded
        {
            get { return m_CabDownloaded; }
        }

        public bool CabPurged
        {
            get { return m_CabPurged; }
        }

        public String CabPathAndFileName
        {
            get { return m_CabPathAndFileName; }
        }

        public NameValueCollection AnalysisData
        {
            get { return m_AnalysisData; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String mainFields = String.Format(CultureInfo.InvariantCulture, 
                "Cab: Id={0}, Size={1}, Downloaded={2}, Purged={3}, Path={4}",
                m_CabId, m_SizeInBytes, m_CabDownloaded, m_CabPurged, m_CabPathAndFileName);

            result.AppendLine(mainFields);

            if (m_AnalysisData != null)
            {
                foreach (String key in m_AnalysisData)
                {
                    result.Append("   ");
                    result.Append(key);
                    result.Append("=");

                    if (m_AnalysisData[key] != null)
                        result.Append(m_AnalysisData[key]);
                    result.AppendLine();
                }
            }

            return result.ToString();
        }
    }
}
