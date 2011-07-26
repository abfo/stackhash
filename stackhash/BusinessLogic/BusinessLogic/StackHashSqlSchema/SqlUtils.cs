using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Common;
using System.Data;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Data.SqlTypes;

using StackHashUtilities;

namespace StackHashSqlControl
{
    public class SqlUtils : IDisposable
    {
        private DbProviderFactory m_ProviderFactory;
//        private DbConnection m_CachedConnection;
        private String m_ConnectionString;
        private String m_MasterConnectionString;
        private int m_ConnectionRetryLimit;
        private int m_ConnectReleaseToggle;

        // SQL command formats.
        private const String s_DatabaseExistsSql =
            @"IF EXISTS(SELECT * FROM sys.databases WHERE name = N'{0}') SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_CreateDatabaseSql =
            @"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = N'{0}') CREATE DATABASE {0} "; // This command is appended to so don't add a ;

        private const String s_DeleteDatabaseSql =
            @"ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            " IF EXISTS(SELECT * FROM sys.databases WHERE name = N'{0}') DROP DATABASE {0};";

        private const String s_SelectDatabaseSql =
            @"USE {0};";

        private const String s_RenameDatabaseSql =
            "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            "ALTER DATABASE {0} MODIFY NAME={1}; " + 
            "ALTER DATABASE {1} SET MULTI_USER; ";

        private const String s_ChangeDatabaseLocationSql =
            "ALTER DATABASE {0} MODIFY FILE (NAME = {0}_dat, FILENAME = '{2}{1}.mdf'); " +
            "ALTER DATABASE {0} MODIFY FILE (NAME = {0}_log, FILENAME = '{2}{1}.ldf'); ";

        private const String s_ChangeDatabaseLogicalNameSql =
             "ALTER DATABASE {1} MODIFY FILE (NAME = {0}_dat, NEWNAME = {1}_dat); " +
             "ALTER DATABASE {1} MODIFY FILE (NAME = {0}_log, NEWNAME = {1}_log); ";

        private const String s_ChangeDatabaseLogicalNameIndividualSql =
             "ALTER DATABASE {0} MODIFY FILE (NAME = {1}, NEWNAME = {2}); ";

        
        private const String s_GetDatabaseLogicalNamesSql =
            "SELECT name " + 
            "FROM sys.master_files " + 
            "WHERE database_id = DB_ID(@DatabaseName); ";
        
        private const String s_SetDatabaseOfflineSql =
            "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            "ALTER DATABASE {0} SET OFFLINE; ";

        private const String s_SetDatabaseOnlineSql =
            "ALTER DATABASE {0} SET ONLINE; " +
            "ALTER DATABASE {0} SET MULTI_USER WITH NO_WAIT; ";


        public SqlUtils(DbProviderFactory provider, String connectionString, String masterConnectionString, int connectionRetryLimit)
        {
            m_ProviderFactory = provider;
            m_ConnectionString = connectionString;
            m_ConnectionRetryLimit = connectionRetryLimit;
            m_MasterConnectionString = masterConnectionString;
        }

        /// <summary>
        /// A string column in the database may be empty DBNull. This is not the same as NULL for an object ref.
        /// </summary>
        /// <param name="sqlObject">SqlString object to convert.</param>
        /// <returns>Null or String</returns>
        public static String GetNullableString(Object sqlObject)
        {
            String result = null;
            if (sqlObject != DBNull.Value)
                result = (String)sqlObject;

            return result;

        }
        /// <summary>
        /// An integer column in the database may be empty DBNull. This is not the same as NULL for an object ref.
        /// </summary>
        /// <param name="sqlObject">SqlInteger object to convert.</param>
        /// <returns>0 or the value</returns>
        public static int GetNullableInteger(Object sqlObject)
        {
            int result = 0;
            if (sqlObject != DBNull.Value)
                result = (int)sqlObject;

            return result;
        }


        public static Object MakeSqlCompliantString(String originalString)
        {
            if (originalString == null)
                return DBNull.Value;
            else
                return originalString;
        }

        public Object ExecuteScalarWithRetry(DbCommand sqlCommand)
        {
            sqlCommand.Connection = null;

            try
            {
                for (int retryCount = 0; retryCount < m_ConnectionRetryLimit; retryCount++)
                {
                    // Connect to the the DBMS.
                    // An exception might occur here under heavy load. i.e. the connection pool is exhausted and 
                    // a timeout occurred getting the connection. Let that exception filter back to the client.
                    sqlCommand.Connection = CreateConnection(false);

                    try
                    {
                        return sqlCommand.ExecuteScalar();
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "ExecuteScalarWithRetry failed for command: " + sqlCommand.CommandText, ex);

                        // Retry or not.
                        ReleaseConnection(sqlCommand.Connection);
                        sqlCommand.Connection = null;

                        if (retryCount >= m_ConnectionRetryLimit - 1)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "ExecuteScalarWithRetry failed. Retry limit exceeeded.");

                            throw;
                        }
                    }
                }

                // Shouldn't get here.
                return 0;
            }
            finally
            {
                if (sqlCommand.Connection != null)
                {
                    ReleaseConnection(sqlCommand.Connection);
                    sqlCommand.Connection = null;
                }
            }
        }

        public void DumpConnectionStatistics()
        {
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Connections Used: " + m_ConnectReleaseToggle.ToString(CultureInfo.InvariantCulture));
        }

        public DbConnection CreateConnection(bool master)
        {
            //if (m_CachedConnection != null)
            //    if (m_CachedConnection.State == ConnectionState.Open)
            //        return m_CachedConnection;

            DbConnection connection = m_ProviderFactory.CreateConnection();

            if (master)
                connection.ConnectionString = m_MasterConnectionString;
            else
                connection.ConnectionString = m_ConnectionString;

            try
            {
                connection.Open();
                m_ConnectReleaseToggle++;
//                m_CachedConnection = connection;
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to open connection", ex);
                DumpConnectionStatistics();
                throw;
            }
            return connection;
        }

        public void ReleaseConnection(DbConnection connection)
        {
//            if (m_CachedConnection != connection)
            try
            {
                connection.Close();
                connection.Dispose();
                m_ConnectReleaseToggle--;
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Failed to release connection", ex);
                DumpConnectionStatistics();
                throw;
            }
        }


        
        public int ExecuteNonQueryWithRetry(DbCommand sqlCommand)
        {
            sqlCommand.Connection = null;

            try
            {
                for (int retryCount = 0; retryCount < m_ConnectionRetryLimit; retryCount++)
                {
                    // Connect to the the DBMS.
                    // An exception might occur here under heavy load. i.e. the connection pool is exhausted and 
                    // a timeout occurred getting the connection. Let that exception filter back to the client.
                    sqlCommand.Connection = CreateConnection(false);

                    try
                    {
                        return sqlCommand.ExecuteNonQuery();
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "ExecuteNonQueryWithRetry failed for command: " + sqlCommand.CommandText, ex);

                        // Retry or not.
                        ReleaseConnection(sqlCommand.Connection);
                        sqlCommand.Connection = null;

                        if (retryCount >= m_ConnectionRetryLimit - 1)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "ExecuteNonQueryWithRetry failed. Retry limit exceeeded.");
                            throw;
                        }
                    }
                }

                // Shouldn't get here.
                return 0;
            }
            finally
            {
                if (sqlCommand.Connection != null)
                {
                    ReleaseConnection(sqlCommand.Connection);
                    sqlCommand.Connection = null;
                }
            }
        }

        public DbDataReader ExecuteReaderWithRetry(DbCommand sqlCommand)
        {
            sqlCommand.Connection = null;
            try
            {
                for (int retryCount = 0; retryCount < m_ConnectionRetryLimit; retryCount++)
                {
                    // Connect to the the DBMS.
                    // An exception might occur here under heavy load. i.e. the connection pool is exhausted and 
                    // a timeout occurred getting the connection. Let that exception filter back to the client.
                    sqlCommand.Connection = CreateConnection(false);

                    try
                    {
                        return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "ExecuteReader failed for command: " + sqlCommand.CommandText, ex);

                        // Retry or not.
                        ReleaseConnection(sqlCommand.Connection);
                        sqlCommand.Connection = null;

                        if (retryCount >= m_ConnectionRetryLimit - 1)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "ExecuteReader failed. Retry limit exceeeded.");
                            throw;
                        }
                    }
                }

                // Shouldn't get here.
                return null;
            }
            finally
            {
                // Don't free the connection. When the caller closes the data reader, the connection will be closed.
            }
        }


        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool DatabaseExists(String databaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_DatabaseExistsSql, databaseName);

            int result = (int)sqlCommand.ExecuteScalar();

            return (result != 0);
        }


        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool CreateDatabase(String databaseFolder, String databaseName, DbConnection connection, bool createIndexInDefaultLocation)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            if (DatabaseExists(databaseName, connection))
                throw new InvalidOperationException("Database already exists");


            //CREATE DATABASE Sales
            //ON 
            //( NAME = Sales_dat,
            //    FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\saledat.mdf',
            //    SIZE = 10,
            //    MAXSIZE = 50,
            //    FILEGROWTH = 5 )
            //LOG ON
            //( NAME = Sales_log,
            //    FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\salelog.ldf',
            //    SIZE = 5MB,
            //    MAXSIZE = 25MB,
            //    FILEGROWTH = 5MB ) ;
            //GO

            

            String dataFileConfig = String.Empty;
            String logFileConfig = String.Empty;
            if (!createIndexInDefaultLocation)
            {
                dataFileConfig = String.Format(" ON (NAME = {0}_dat, FILENAME = '{1}\\{0}.mdf', SIZE=10) ", databaseName, databaseFolder);
                logFileConfig = logFileConfig = String.Format(" LOG ON (NAME = {0}_log, FILENAME = '{1}\\{0}.ldf', SIZE=5); ", databaseName, databaseFolder);

                if (!Directory.Exists(databaseFolder))
                    Directory.CreateDirectory(databaseFolder);
            }

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_CreateDatabaseSql, databaseName);

            if (databaseFolder != null)
                sqlCommand.CommandText += dataFileConfig + logFileConfig;

            sqlCommand.ExecuteNonQuery();

            return true;
        }


        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool RenameDatabase(String oldDatabaseName, String newDatabaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(oldDatabaseName))
                return false;
            if (String.IsNullOrEmpty(newDatabaseName))
                return false;

            if (!DatabaseExists(oldDatabaseName, connection))
                throw new InvalidOperationException("Database not found: " + oldDatabaseName);

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_RenameDatabaseSql, oldDatabaseName, newDatabaseName);

            sqlCommand.ExecuteNonQuery();

            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ChangeDatabaseLocation(String databaseName, String newDatabaseName, String newErrorIndexPath, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");
            if (String.IsNullOrEmpty(newDatabaseName))
                throw new ArgumentNullException("newDatabaseName");
            if (String.IsNullOrEmpty(newErrorIndexPath))
                throw new ArgumentNullException("newErrorIndexPath");
            if (!Directory.Exists(newErrorIndexPath))
                throw new ArgumentException("Directory must exist", "newErrorIndexPath");

            if (!DatabaseExists(databaseName, connection))
                throw new ArgumentException("Database not found: " + databaseName, "databaseName");

            // Append a \\ on the end of the file path if one is not present.
            if (newErrorIndexPath[newErrorIndexPath.Length - 1] != Path.DirectorySeparatorChar)
                newErrorIndexPath += Path.DirectorySeparatorChar;

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_ChangeDatabaseLocationSql, databaseName, newDatabaseName, newErrorIndexPath);

            sqlCommand.ExecuteNonQuery();

            return true;
        }


        /// <summary>
        /// Get a list of logical names associated with a database.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>true - success, false - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public Collection<String> GetLogicalFileNames(String databaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            Collection<String> logicalFileNames = new Collection<string>();

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_GetDatabaseLogicalNamesSql, databaseName);
            AddParameter(sqlCommand, "DatabaseName", DbType.String, databaseName);

            DbDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                String logicalName = (String)reader["name"];
                logicalFileNames.Add(logicalName);
            }

            return logicalFileNames;
        }


        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Design", "CA1006")]
        public bool ChangeDatabaseLogicalNameList(String databaseName, Collection<Tuple<String, String>> changes, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");
            if (changes == null)
                throw new ArgumentNullException("changes");

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;

            sqlCommand.CommandText = "";

            foreach (Tuple<String, String> change in changes)
            {
                sqlCommand.CommandText += String.Format(CultureInfo.InvariantCulture, s_ChangeDatabaseLogicalNameIndividualSql, databaseName, change.Item1, change.Item2);
            }

            sqlCommand.ExecuteNonQuery();

            return true;
        }

        
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ChangeDatabaseLogicalName(String databaseName, String newDatabaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");
            if (String.IsNullOrEmpty(newDatabaseName))
                throw new ArgumentNullException("newDatabaseName");

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_ChangeDatabaseLogicalNameSql, databaseName, newDatabaseName);

            sqlCommand.ExecuteNonQuery();

            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool SetDatabaseOnlineState(String databaseName, DbConnection connection, bool setOnline)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;

            if (setOnline)
                sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_SetDatabaseOnlineSql, databaseName);
            else
                sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_SetDatabaseOfflineSql, databaseName);

            sqlCommand.ExecuteNonQuery();

            return true;
        }

        
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool DeleteDatabase(String databaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;


            if (!DatabaseExists(databaseName, connection))
                return true;

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_DeleteDatabaseSql, databaseName);

            sqlCommand.ExecuteNonQuery();

            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool SelectDatabase(String databaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            if (!DatabaseExists(databaseName, connection))
                return false;

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_SelectDatabaseSql, databaseName);
            sqlCommand.Connection = connection;
            sqlCommand.ExecuteNonQuery();

            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void ExecuteNonQuery(String command, DbConnection connection)
        {
            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = command;

            sqlCommand.ExecuteNonQuery();
        }


        public void AddParameter(DbCommand command, String paramName, DbType type, Object value)
        {
            DbParameter param = m_ProviderFactory.CreateParameter();
            param.ParameterName = paramName;
            param.DbType = type;
            param.Value = value;
            command.Parameters.Add(param);
        }


        /// <summary>
        /// SQL dates start at around 1700 so convert DateTimes into 1900 date so it can be converted back and forth.
        /// 1st Jan 1900 is defined as DateTime(0) for our purposes.
        /// </summary>
        /// <param name="dateTime">Date to check.</param>
        /// <returns>Converted date.</returns>
        public static DateTime ConvertToUtc(DateTime dateTime)
        {
            if (dateTime.Year == 1900)
                return new DateTime(0, DateTimeKind.Utc);
            else if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            else
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
        }

        /// <summary>
        /// SQL dates start at around 1700 so convert DateTimes into 1900 date so it can be converted back and forth.
        /// 1st Jan 1900 is defined as DateTime(0) for our purposes.
        /// </summary>
        /// <param name="dateTime">Date to check.</param>
        /// <returns>Converted date.</returns>
        public static DateTime ConvertToLocal(DateTime dateTime)
        {
            if (dateTime.Year == 1900)
                return new DateTime(0, DateTimeKind.Local);
            else if (dateTime.Kind == DateTimeKind.Local)
                return dateTime;
            else
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Local);
        }

        /// <summary>
        /// SQL dates start at around 1700 so convert DateTimes into 1900 date so it can be converted back and forth.
        /// </summary>
        /// <param name="dateTime">Date to check.</param>
        /// <returns>Converted date.</returns>
        public static DateTime MakeDateSqlCompliant(DateTime dateTime)
        {
            if (dateTime.Year < 1700)
                return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            else
                return dateTime;
        }


        public static String GetSqlCompliantDate(DateTime dateTime)
        {
            SqlDateTime sqlDateTime = new SqlDateTime(dateTime);
            return sqlDateTime.ToString();
        }

        private static String[] s_ReservedSqlKeyword =
        {
            "ADD", "EXCEPT", "PERCENT",
            "ALL", "EXEC",  "PLAN", 
            "ALTER", "EXECUTE", "PRECISION",
            "AND", "EXISTS", "PRIMARY", 
            "ANY", "EXIT", "PRINT", 
            "AS", "FETCH", "PROC", 
            "ASC", "FILE", "PROCEDURE", 
            "AUTHORIZATION", "FILLFACTOR", "PUBLIC",    
            "BACKUP", "FOR", "RAISERROR", 
            "BEGIN", "FOREIGN", "READ", 
            "BETWEEN", "FREETEXT", "READTEXT", 
            "BREAK", "FREETEXTTABLE", "RECONFIGURE", 
            "BROWSE", "FROM", "REFERENCES", 
            "BULK", "FULL", "REPLICATION", 
            "BY", "FUNCTION", "RESTORE", 
            "CASCADE", "GOTO", "RESTRICT", 
            "CASE", "GRANT", "RETURN", 
            "CHECK", "GROUP", "REVOKE", 
            "CHECKPOINT", "HAVING", "RIGHT", 
            "CLOSE", "HOLDLOCK", "ROLLBACK", 
            "CLUSTERED", "IDENTITY", "ROWCOUNT", 
            "COALESCE", "IDENTITY_INSERT", "ROWGUIDCOL", 
            "COLLATE", "IDENTITYCOL", "RULE", 
            "COLUMN", "IF", "SAVE", 
            "COMMIT", "IN", "SCHEMA", 
            "COMPUTE", "INDEX", "SELECT", 
            "CONSTRAINT", "INNER", "SESSION_USER", 
            "CONTAINS", "INSERT", "SET", 
            "CONTAINSTABLE", "INTERSECT", "SETUSER", 
            "CONTINUE", "INTO", "SHUTDOWN",     
            "CONVERT", "IS", "SOME", 
            "CREATE", "JOIN", "STATISTICS", 
            "CROSS", "KEY", "SYSTEM_USER", 
            "CURRENT", "KILL", "TABLE", 
            "CURRENT_DATE", "LEFT", "TEXTSIZE", 
            "CURRENT_TIME", "LIKE", "THEN", 
            "CURRENT_TIMESTAMP", "LINENO", "TO", 
            "CURRENT_USER", "LOAD", "TOP", 
            "CURSOR", "NATIONAL", "TRAN", 
            "DATABASE", "NOCHECK", "TRANSACTION", 
            "DBCC", "NONCLUSTERED", "TRIGGER", 
            "DEALLOCATE", "NOT", "TRUNCATE", 
            "DECLARE", "NULL", "TSEQUAL", 
            "DEFAULT", "NULLIF", "UNION", 
            "DELETE", "OF", "UNIQUE", 
            "DENY", "OFF", "UPDATE", 
            "DESC", "OFFSETS", "UPDATETEXT", 
            "DISK", "ON", "USE", 
            "DISTINCT", "OPEN", "USER", 
            "DISTRIBUTED", "OPENDATASOURCE", "VALUES", 
            "DOUBLE", "OPENQUERY", "VARYING", 
            "DROP", "OPENROWSET", "VIEW", 
            "DUMMY", "OPENXML", "WAITFOR", 
            "DUMP", "OPTION", "WHEN", 
            "ELSE", "OR", "WHERE", 
            "END", "ORDER", "WHILE", 
            "ERRLVL", "OUTER", "WITH", 
            "ESCAPE", "OVER", "WRITETEXT", 
        };

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
            bool isFirstLetter = true;

            if (databaseName.Length > 50)
                return false;

            foreach (char ch in databaseName)
            {
                // First character must be a letter.
                if (isFirstLetter)
                {
                    if (!char.IsLetter(ch))
                        return false;
                    isFirstLetter = false;
                    continue;
                }

                if (!char.IsLetter(ch) && !char.IsDigit(ch) && (ch != '_'))
                    return false;
            }

            // Compare to SQL reserved words.
            foreach (String keyword in s_ReservedSqlKeyword)
            {
                if (String.Compare(databaseName, keyword, StringComparison.OrdinalIgnoreCase) == 0)
                    return false;
            }

            return true;
        }


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // These objects are owned by the SqlErrorIndex so don't close them - just get rid of the reference.
                m_ProviderFactory = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
