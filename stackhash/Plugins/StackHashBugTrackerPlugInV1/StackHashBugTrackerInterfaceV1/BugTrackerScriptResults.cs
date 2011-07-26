using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace StackHashBugTrackerInterfaceV1
{
    [Serializable]
    public class BugTrackerScriptResult
    {
        private String m_ScriptResults;
        private String m_Name;
        private DateTime m_LastModifiedDate;
        private DateTime m_RunDate;
        private int m_ScriptVersion;

        public BugTrackerScriptResult() { ; }

        public BugTrackerScriptResult(String name, int scriptVersion, DateTime lastModified, DateTime runDate, String scriptResults)
        {
            m_Name = name;
            m_ScriptVersion = scriptVersion;
            m_LastModifiedDate = lastModified;
            m_RunDate = runDate;
            m_ScriptResults = scriptResults;
        }

        public String ScriptResults
        {
            get { return m_ScriptResults; }
            set { m_ScriptResults = value; }
        }

        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public DateTime LastModifiedDate
        {
            get { return m_LastModifiedDate; }
            set { m_LastModifiedDate = value; }
        }

        public DateTime RunDate
        {
            get { return m_RunDate; }
            set { m_RunDate = value; }
        }

        public int ScriptVersion
        {
            get { return m_ScriptVersion; }
            set { m_ScriptVersion = value; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String mainFields = String.Format(CultureInfo.InvariantCulture, "Script: RunDate={0}, Name={1}, Version={2}, LastModifiedDate={3}",
                m_RunDate, m_Name, m_ScriptVersion, m_LastModifiedDate);
            result.AppendLine(mainFields);

            result.AppendLine(m_ScriptResults);

            return result.ToString();
        }
    }
}
