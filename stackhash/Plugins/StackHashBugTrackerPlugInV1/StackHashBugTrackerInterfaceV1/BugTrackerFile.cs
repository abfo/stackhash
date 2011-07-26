using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace StackHashBugTrackerInterfaceV1
{
    [Serializable]
    public class BugTrackerFile
    {
        String m_FileName;
        String m_FileVersion;
        long m_FileId;

        public BugTrackerFile(String fileName, String fileVersion, long fileId)
        {
            m_FileName = fileName;
            m_FileVersion = fileVersion;
            m_FileId = fileId;
        }

        public String FileName
        {
            get { return m_FileName; }
        }

        public String FileVersion
        {
            get { return m_FileVersion; }
        }

        public long FileId
        {
            get { return m_FileId; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String mainFields = String.Format(CultureInfo.InvariantCulture, "File: Id={0}, FileName={1}, FileVersion={2}",
                m_FileId, m_FileName, m_FileVersion);
            result.AppendLine(mainFields);

            return result.ToString();
        }
    }
}
