using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using System.Data.SqlClient;

namespace StackHashDBConfig
{
    /// <summary>
    /// Database settings for SQL Server 2005 / 2008
    /// </summary>
    public class DatabaseSettingsSqlServer : DatabaseSettings
    {
        private const string ConnectionStringTemplate = "Data Source={0};Initial Catalog={1};Integrated Security=True";
        private const string DataSource = "Data Source";
        private const string InitialCatalog = "Initial Catalog";
        private const string MasterCatalog = "MASTER";
        private const string LocalSource = "(local)";
        private const string DefaultInstanceName = "MSSQLSERVER";

        /// <summary>
        /// Database settings for SQL Server 2005 / 2008
        /// </summary>
        public DatabaseSettingsSqlServer()
            : base(DatabaseType.SqlServer, Properties.Resources.DatabaseTypeSqlServer)
        {
            
        }

        /// <summary>
        /// Gets the master connection string from a database connection string
        /// </summary>
        /// <param name="databaseConnectionString">Database connection string</param>
        /// <returns>Master connection string</returns>
        public override string GetMasterConnectionString(string databaseConnectionString)
        {
            if (string.IsNullOrEmpty(databaseConnectionString)) { throw new ArgumentException("Connection string is null or empty", "databaseConnectionString"); }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(databaseConnectionString);
            builder[InitialCatalog] = MasterCatalog;

            return builder.ToString();
        }

        /// <summary>
        /// Gets the connection string for a database
        /// </summary>
        /// <param name="instance">Database instance</param>
        /// <param name="database">Database name</param>
        /// <returns>Connection string</returns>
        public override string GetDatabaseConnectionString(string instance, string database)
        {
            string dataSource = LocalSource;

            // if we have an instance and it's not the default SQL Server instance we need to add it to the data source
            if ((!string.IsNullOrEmpty(instance)) &&
                (string.Compare(instance, DefaultInstanceName, StringComparison.InvariantCultureIgnoreCase) != 0))
            {
                dataSource = string.Format(CultureInfo.InvariantCulture,
                    "{0}\\{1}",
                    LocalSource,
                    instance);
            }

            return string.Format(CultureInfo.InvariantCulture,
                ConnectionStringTemplate,
                dataSource,
                database);
        }

        /// <summary>
        /// Detects if the data source has changed in a connection string
        /// </summary>
        /// <param name="originalConnectionString">Original connection string</param>
        /// <param name="currentConnectionString">Current connection string</param>
        /// <returns>True if the connection string has changed</returns>
        public override bool HasDataSourceChanged(string originalConnectionString, string currentConnectionString)
        {
            bool dataSourceChanged = true;

            if ((!string.IsNullOrEmpty(originalConnectionString)) && (!string.IsNullOrEmpty(currentConnectionString)))
            {
                SqlConnectionStringBuilder originalBuilder = new SqlConnectionStringBuilder(originalConnectionString);
                SqlConnectionStringBuilder currentBuilder = new SqlConnectionStringBuilder(currentConnectionString);

                dataSourceChanged = string.Compare((string)originalBuilder[DataSource],
                    (string)currentBuilder[DataSource],
                    StringComparison.OrdinalIgnoreCase) != 0;
            }

            return dataSourceChanged;
        }

        /// <summary>
        /// Refreshes the list of instances
        /// </summary>
        public override void RefreshInstances()
        {
            this.Instances = SqlInstanceHelper.FindInstances();
        }
    }
}
