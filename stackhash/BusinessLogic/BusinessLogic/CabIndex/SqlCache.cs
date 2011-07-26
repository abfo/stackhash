using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace StackHashErrorIndex
{
    public class SqlEventType
    {
        private short m_EventTypeId;
        private String m_EventTypeName;

        public SqlEventType(short eventTypeId, String eventTypeName)
        {
            m_EventTypeId = eventTypeId;
            m_EventTypeName = eventTypeName;
        }

        public short EventTypeId
        {
            get { return m_EventTypeId; }
            set { m_EventTypeId = value; }
        }

        public String EventTypeName
        {
            get { return m_EventTypeName; }
            set { m_EventTypeName = value; }
        }
    }

    public class SqlEventTypeCache
    {
        private Dictionary<short, SqlEventType> m_EventTypes;

        public SqlEventTypeCache()
        {
            m_EventTypes = new Dictionary<short, SqlEventType>();
        }

        public void Add(SqlEventType newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException("newValue");

            m_EventTypes.Add(newValue.EventTypeId, newValue);
        }

        public String GetEventTypeName(short eventTypeId)
        {
            if (m_EventTypes.ContainsKey(eventTypeId))
            {
                return m_EventTypes[eventTypeId].EventTypeName;
            }
            else
            {
                return null;
            }
        }

        public short GetEventTypeId(String eventTypeName)
        {
            foreach (SqlEventType eventType in m_EventTypes.Values)
            {
                if (String.Compare(eventType.EventTypeName, eventTypeName, StringComparison.OrdinalIgnoreCase) == 0)
                    return eventType.EventTypeId;
            }
            return -1;
        }
    }

    public class SqlOperatingSystem
    {
        private short m_OperatingSystemId;
        private String m_OperatingSystemName;
        private String m_OperatingSystemVersion;

        public SqlOperatingSystem(short operatingSystemId, String operatingSystemName, String operatingSystemVersion)
        {
            m_OperatingSystemId = operatingSystemId;
            m_OperatingSystemName = operatingSystemName;
            m_OperatingSystemVersion = operatingSystemVersion;
        }

        public short OperatingSystemId
        {
            get { return m_OperatingSystemId; }
            set { m_OperatingSystemId = value; }
        }

        public String OperatingSystemName
        {
            get { return m_OperatingSystemName; }
            set { m_OperatingSystemName = value; }
        }

        public String OperatingSystemVersion
        {
            get { return m_OperatingSystemVersion; }
            set { m_OperatingSystemVersion = value; }
        }
    }

    public class SqlOperatingSystemCache
    {
        private Dictionary<short, SqlOperatingSystem> m_OperatingSystems;

        public SqlOperatingSystemCache()
        {
            m_OperatingSystems = new Dictionary<short, SqlOperatingSystem>();
        }

        public void Add(SqlOperatingSystem newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException("newValue");

            m_OperatingSystems.Add(newValue.OperatingSystemId, newValue);
        }

        public SqlOperatingSystem GetOperatingSystem(short operatingSystemId)
        {
            if (m_OperatingSystems.ContainsKey(operatingSystemId))
            {
                return m_OperatingSystems[operatingSystemId];
            }
            else
            {
                return null;
            }
        }

        public short GetId(SqlOperatingSystem operatingSystem)
        {
            if (operatingSystem == null)
                throw new ArgumentNullException("operatingSystem");

            foreach (SqlOperatingSystem os in m_OperatingSystems.Values)
            {
                bool nameMatch = false;

                if (((os.OperatingSystemName == null) && (operatingSystem.OperatingSystemName == null)) ||
                    ((os.OperatingSystemName != null) && (operatingSystem.OperatingSystemName != null) &&
                    (String.Compare(os.OperatingSystemName, operatingSystem.OperatingSystemName, StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    nameMatch = true;
                }

                bool versionMatch = false;
                if (((os.OperatingSystemVersion == null) && (operatingSystem.OperatingSystemVersion == null)) ||
                    ((os.OperatingSystemVersion != null) && (operatingSystem.OperatingSystemVersion != null) &&
                    (String.Compare(os.OperatingSystemVersion, operatingSystem.OperatingSystemVersion, StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    versionMatch = true;
                }

                if (nameMatch && versionMatch)
                    return os.OperatingSystemId;
            }
            return -1;
        }
    }


    public class SqlLocale
    {
        private short m_LocaleId;
        private String m_LocaleCode;
        private String m_LocaleName;

        public SqlLocale(short localeId, String localeCode, String localeName)
        {
            m_LocaleId = localeId;
            m_LocaleCode = localeCode;
            m_LocaleName = localeName;
        }

        public short LocaleId
        {
            get { return m_LocaleId; }
            set { m_LocaleId = value; }
        }

        public String LocaleCode
        {
            get { return m_LocaleCode; }
            set { m_LocaleCode = value; }
        }

        public String LocaleName
        {
            get { return m_LocaleName; }
            set { m_LocaleName = value; }
        }
    }

    public class SqlLocaleCache
    {
        private Dictionary<short, SqlLocale> m_Locales;
        private bool m_Primed;

        public SqlLocaleCache()
        {
            m_Locales = new Dictionary<short, SqlLocale>();
        }

        public bool Primed
        {
            get { return m_Primed; }
            set { m_Primed = value; }
        }

        public void Add(SqlLocale newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException("newValue");
            m_Locales.Add(newValue.LocaleId, newValue);
        }

        public SqlLocale GetLocale(short localeId)
        {
            if (m_Locales.ContainsKey(localeId))
            {
                return m_Locales[localeId];
            }
            else
            {
                return null;
            }
        }

        public short GetId(SqlLocale locale)
        {
            if (locale == null)
                throw new ArgumentNullException("locale");
            foreach (SqlLocale l in m_Locales.Values)
            {
                bool localeCodeMatch = false;
                if (((l.LocaleCode == null) && (locale.LocaleCode == null)) ||
                    ((l.LocaleCode != null) && (locale.LocaleCode != null) &&
                    (String.Compare(l.LocaleCode, locale.LocaleCode, StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    localeCodeMatch = true;
                }

                bool LocaleNameMatch = false;
                if (((l.LocaleName == null) && (locale.LocaleName == null)) ||
                    ((l.LocaleName != null) && (locale.LocaleName != null) &&
                    (String.Compare(l.LocaleName, locale.LocaleName, StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    LocaleNameMatch = true;
                }

                if (localeCodeMatch && LocaleNameMatch)
                    return l.LocaleId;
            }
            return -1;
        }
    }
}
