using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;

namespace StackHashBugTrackerInterfaceV1
{
    [Serializable]
    public class BugTrackerEvent
    {
        String m_BugReference;
        String m_PlugInBugReference;
        long m_EventId;
        String m_EventTypeName;
        long m_TotalHits;
        NameValueCollection m_EventSignature;

        public BugTrackerEvent(String bugReference, String plugInBugReference, long eventId, String eventTypeName, long totalHits, NameValueCollection signature)
        {
            m_BugReference = bugReference;
            m_PlugInBugReference = plugInBugReference;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
            m_TotalHits = totalHits;

            if (m_TotalHits < 0)
                m_TotalHits = 0;

            m_EventSignature = signature;
        }

        public String BugReference
        {
            get { return m_BugReference; }
        }

        public String PlugInBugReference
        {
            get { return m_PlugInBugReference; }
        }

        public long EventId
        {
            get { return m_EventId; }
        }

        public String EventTypeName
        {
            get { return m_EventTypeName; }
        }

        public long TotalHits
        {
            get { return m_TotalHits; }
        }
        public NameValueCollection Signature
        {
            get { return m_EventSignature; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String mainFields = String.Format(CultureInfo.InvariantCulture, "Event: Id={0}, EventTypeName={1}, BugReference={2}, TotalHits={3}",
                m_EventId, m_EventTypeName, m_BugReference, m_TotalHits);

            result.AppendLine(mainFields);

            foreach (String key in m_EventSignature)
            {
                result.Append("   ");
                result.Append(key);
                result.Append("=");

                if (m_EventSignature[key] != null)
                    result.Append(m_EventSignature[key]);
                result.AppendLine();
            }

            return result.ToString();
        }
    }
}
