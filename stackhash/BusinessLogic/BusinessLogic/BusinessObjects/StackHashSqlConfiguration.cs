using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using StackHashUtilities;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSqlConfiguration
    {
        private String m_ConnectionString;
        private String m_InitialCatalog;
        private int m_MinPoolSize;
        private int m_MaxPoolSize;
        private int m_ConnectionTimeout;
        private int m_EventsPerBlock;

        public StackHashSqlConfiguration() { ; }

        public StackHashSqlConfiguration(String connectionString, String initialCatalog, int minPoolSize, int maxPoolSize, int connectionTimeout, int eventsPerBlock)
        {
            m_ConnectionString = connectionString;
            m_InitialCatalog = initialCatalog;
            m_MinPoolSize = minPoolSize;
            m_MaxPoolSize = maxPoolSize;
            m_ConnectionTimeout = connectionTimeout;
            m_EventsPerBlock = eventsPerBlock;
        }

        [DataMember]
        public String ConnectionString
        {
            get { return m_ConnectionString; }
            set { m_ConnectionString = value; }
        }

        [DataMember]
        public String InitialCatalog
        {
            get { return m_InitialCatalog; }
            set { m_InitialCatalog = value; }
        }

        [DataMember]
        public int MinPoolSize
        {
            get { return m_MinPoolSize; }
            set { m_MinPoolSize = value; }
        }

        [DataMember]
        public int MaxPoolSize
        {
            get { return m_MaxPoolSize; }
            set { m_MaxPoolSize = value; }
        }

        [DataMember]
        public int ConnectionTimeout
        {
            get { return m_ConnectionTimeout; }
            set { m_ConnectionTimeout = value; }
        }

        /// <summary>
        /// Blocks of EventInfos and Cabs are retrieved from the SQL Index at a time to cut down the number of calls made.
        /// The default is to get the cabs and event infos for 100 events at a time. This needs to be configurable in 
        /// case it is still too slow when 250000 events are being retrieved.
        /// </summary>
        [DataMember]
        public int EventsPerBlock
        {
            get { return m_EventsPerBlock; }
            set { m_EventsPerBlock = value; }
        }

        /// <summary>
        /// Constructs a connection string from the sql settings.
        /// </summary>
        /// <returns>Connection string.</returns>
        public String ToConnectionString()
        {
            StringBuilder connectionString = new StringBuilder();
            connectionString.Append(m_ConnectionString);

            // Strip the last semicolon.
            if (m_ConnectionString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                connectionString.Remove(m_ConnectionString.Length - 1, 1);

            if (!String.IsNullOrEmpty(InitialCatalog))
            {
                connectionString.Append(";Initial Catalog=");
                connectionString.Append(InitialCatalog);
            }

            if (MinPoolSize != -1)
            {
                connectionString.Append(";Min Pool Size=");
                connectionString.Append(MinPoolSize.ToString(CultureInfo.InvariantCulture));
            }

            if (MaxPoolSize != -1)
            {
                connectionString.Append(";Max Pool Size=");
                connectionString.Append(MaxPoolSize.ToString(CultureInfo.InvariantCulture));
            }

            if (ConnectionTimeout != -1)
            {
                connectionString.Append(";Connection Timeout=");
                connectionString.Append(ConnectionTimeout.ToString(CultureInfo.InvariantCulture));
            }

            return connectionString.ToString();
        }


        public static StackHashSqlConfiguration Default
        {
            get
            {
                return new StackHashSqlConfiguration(TestSettings.DefaultConnectionString, "StackHash", 6, 100, 15, 100);
            }
        }
        public static StackHashSqlConfiguration DefaultMaster
        {
            get
            {
                return new StackHashSqlConfiguration(TestSettings.DefaultConnectionString, "MASTER", 6, 100, 15, 100);
            }
        }
    }
}
