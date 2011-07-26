using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace StackHashBugTrackerInterfaceV1
{
    [Serializable]
    public class BugTrackerNote
    {
        private DateTime m_TimeOfEntry;
        private String m_Source;
        private String m_Note;
        private String m_User;

        public BugTrackerNote() 
        { 
            m_TimeOfEntry = DateTime.Now.ToUniversalTime();
        }

        public BugTrackerNote(DateTime timeOfEntry, String source, String user, String note)
        {
            m_TimeOfEntry = timeOfEntry;
            m_Source = source;
            m_User = user;
            m_Note = note;
        }

        public DateTime TimeOfEntry
        {
            get { return m_TimeOfEntry; }
            set { m_TimeOfEntry = value; }
        }

        public String Source
        {
            get { return m_Source; }
            set { m_Source = value; }
        }

        public String Note
        {
            get { return m_Note; }
            set { m_Note = value; }
        }

        public String User
        {
            get { return m_User; }
            set { m_User = value; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String mainFields = String.Format(CultureInfo.InvariantCulture, "Note: TimeOfEntry={0}, Source={1}, User={2}",
                m_TimeOfEntry, m_Source, m_User);
            result.AppendLine(mainFields);
            result.AppendLine(m_Note);

            return result.ToString();
        }
    }
}
