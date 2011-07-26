using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace StackHashSqlControl
{
    public class InstallerInterface
    {
        private String m_ConnectionString;  // Identifies the location of the database.
        private String m_DatabaseName;
        private String m_RootCabFolder;
        private String m_ErrorIndexPath;
        private DbProviderFactory m_ProviderFactory;
        private DbConnection m_DbConnection;
        private SqlUtils m_SqlUtils;

        /// <summary>
        /// The databse is not actually created but merely constructs the connection to the database.
        /// Note the database connection string will be different for Express and Enterprise versions of SQL Server.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="rootCabFolder">Root folder for storing cabs.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public InstallerInterface(String connectionString, String databaseName, String rootCabFolder)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (databaseName == null)
                throw new ArgumentNullException("databaseName");
            if (rootCabFolder == null)
                throw new ArgumentNullException("rootCabFolder");


            m_ConnectionString = connectionString;
            m_DatabaseName = databaseName;
            m_RootCabFolder = rootCabFolder;

            // The error index is placed in a subdirectory of the specified folder.
            if (!rootCabFolder.EndsWith("\\", true, CultureInfo.InstalledUICulture))
            {
                m_ErrorIndexPath = rootCabFolder + "\\" + databaseName;
                m_RootCabFolder = rootCabFolder + "\\";
            }
            else
            {
                m_ErrorIndexPath = rootCabFolder + databaseName;
                m_RootCabFolder = rootCabFolder;
            } 

            m_ProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        public void Connect()
        {
            // Should always connect to master.
            m_SqlUtils = new SqlUtils(m_ProviderFactory, m_ConnectionString, m_ConnectionString, 1);
            m_DbConnection = m_SqlUtils.CreateConnection(true);
        }

        public bool DatabaseExists()
        {
            return m_SqlUtils.DatabaseExists(m_DatabaseName, m_DbConnection);
        }

        public bool DatabaseExists(String databaseName)
        {
            return m_SqlUtils.DatabaseExists(databaseName, m_DbConnection);
        }

        public void DeleteDatabase(String databaseName)
        {
            m_SqlUtils.DeleteDatabase(databaseName, m_DbConnection);
        }

        public bool CreateDatabase(bool defaultLocation)
        {
            if (!Directory.Exists(m_ErrorIndexPath))
                Directory.CreateDirectory(m_ErrorIndexPath);

            String databasePath = null;
            if (!defaultLocation)
                databasePath = m_ErrorIndexPath;

            return SqlSchema.CreateStackHashDatabase(m_SqlUtils, databasePath, m_DatabaseName, defaultLocation);
        }


        /// <summary>
        /// Only allow a max of 50 characters starting with a letter and containing only 
        /// letters, numbers and the underscore character. 
        /// This is more restrictive than SQL.
        /// http://msdn.microsoft.com/en-us/library/ms175874.aspx
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static bool IsValidSqlDatabaseName(String databaseName)
        {
            return SqlUtils.IsValidSqlDatabaseName(databaseName);
        }

        public void Disconnect()
        {
            if (m_DbConnection != null)
            {
                m_SqlUtils.SelectDatabase("MASTER", m_DbConnection);
                m_SqlUtils.ReleaseConnection(m_DbConnection);
                m_SqlUtils.Dispose();
                m_DbConnection = null;
            }
        }
    }
}
