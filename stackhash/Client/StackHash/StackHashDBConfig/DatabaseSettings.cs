using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace StackHashDBConfig
{
    /// <summary>
    /// Supported database types
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        ///Microsoft SQL Server 2005 or later
        /// </summary>
        SqlServer,
    }

    /// <summary>
    /// Settings for a StackHash database candidate (abstract base class)
    /// </summary>
    public abstract class DatabaseSettings
    {
        /// <summary>
        /// The type of this database
        /// </summary>
        public DatabaseType DatabaseType { get; private set; }

        /// <summary>
        /// Display description of this database
        /// </summary>
        public string TypeDescription { get; private set; }

        /// <summary>
        /// Gets a list of available instances of this database type
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<DatabaseInstance> Instances { get; protected set; }

        /// <summary>
        /// Gets the master connection string for an instance of this database
        /// </summary>
        /// <param name="databaseConnectionString">Database connection string</param>
        /// <returns>Master connection string</returns>
        public abstract string GetMasterConnectionString(string databaseConnectionString);

        /// <summary>
        /// Detects if the data source has changed in a connection string
        /// </summary>
        /// <param name="originalConnectionString">Original connection string</param>
        /// <param name="currentConnectionString">Current connection string</param>
        /// <returns>True if the connection string has changed</returns>
        public abstract bool HasDataSourceChanged(string originalConnectionString, string currentConnectionString);

        /// <summary>
        /// Gets the connection string for a database
        /// </summary>
        /// <param name="instance">Database instance</param>
        /// <param name="database">Database name</param>
        /// <returns>Connection string</returns>
        public abstract string GetDatabaseConnectionString(string instance, string database);

        /// <summary>
        /// Refreshes the list of instances
        /// </summary>
        public virtual void RefreshInstances()
        {

        }

        /// <summary>
        /// Settings for a StackHash database candidate
        /// </summary>
        /// <param name="type">Type of this database candidate</param>
        /// <param name="typeDescription">Display description of this database candidate</param>
        protected DatabaseSettings(DatabaseType type, string typeDescription)
        {
            if (typeDescription == null) { throw new ArgumentNullException("typeDescription"); }

            this.DatabaseType = type;
            this.TypeDescription = typeDescription;
        }
    }
}
