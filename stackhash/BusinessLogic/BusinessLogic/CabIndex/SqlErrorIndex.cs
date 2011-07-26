using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Data.SqlClient;
using System.Threading;

using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashSqlControl;

namespace StackHashErrorIndex
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506")]
    public class SqlErrorIndex : IErrorIndex, IDisposable
    {
        #region Fields
        private bool m_IsActive;
        private bool m_UpdateTableActive;
        private String m_ConnectionString;  // Identifies the location of the database.
        private String m_MasterConnectionString;  // Identifies the location of the MASTER database.
        private StackHashSqlConfiguration m_SqlConfig;
        private String m_DatabaseName;
        private String m_RootCabFolder; // MUST END IN A BACKSLASH
        private String m_ErrorIndexPath; // MUST NOT END IN A BACKSLASH
        private DbProviderFactory m_ProviderFactory;
        private List<DbParameter> m_ParameterList = new List<DbParameter>();
        private SqlCommands m_SqlCommands;
        private bool m_MoveInProgress;
        private int m_MoveIndexCopyFileCount;
        private bool m_MoveAbortRequested;
        private long m_TotalStoredEvents = -1;

        /// <summary>
        /// Version 1 - Initial SQL version of StackHash.
        /// Version 2 - Beta95 - added the Update table.
        /// </summary>
        private static int s_CurrentDatabaseVersion = 3;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructs the SQL error index object.
        /// The index is not actually created but merely constructs the connection to the database.
        /// Note the database connection string will be different for Express and Enterprise versions of SQL Server.
        /// </summary>
        /// <param name="sqlConfig">Full config for the database.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="rootCabFolder">Root folder for storing cabs.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public SqlErrorIndex(StackHashSqlConfiguration sqlConfig, String databaseName, String rootCabFolder)
        {
            if (sqlConfig == null)
                throw new ArgumentNullException("sqlConfig");
            if (rootCabFolder == null)
                throw new ArgumentNullException("rootCabFolder");

            
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

            // Must be done last.
            processSqlSettings(sqlConfig, databaseName);

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Using connection string: " + m_ConnectionString);
        }

        #endregion Constructors


        #region Properties

        /// <summary>
        /// Determines if the index is active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return m_IsActive;
            }
        }

        /// <summary>
        /// Determines if changes should be logged to the Update table.
        /// </summary>
        public bool UpdateTableActive
        {
            get
            {
                return m_UpdateTableActive;
            }
            set
            {
                m_UpdateTableActive = value;
            }
        }

        public ErrorIndexType IndexType
        {
            get
            {
                return ErrorIndexType.SqlExpress;
            }
        }

        /// <summary>
        /// The total events stored across all products in the database.
        /// </summary>
        public static int CurrentDatabaseVersion
        {
            get
            {
                return s_CurrentDatabaseVersion;
            }
        }

        /// <summary>
        /// The total events stored across all products in the database.
        /// </summary>
        public long TotalStoredEvents
        {
            get
            {
                if (m_TotalStoredEvents == -1)
                    m_TotalStoredEvents = m_SqlCommands.GetEventCount();

                return m_TotalStoredEvents;

            }
            set
            {
                m_TotalStoredEvents = value;
            }
        }

        /// <summary>
        /// Total files in the index - across all products.
        /// </summary>
        public long TotalFiles
        {
            get
            {
                return m_SqlCommands.GetFileCount();
            }
        }

        /// <summary>
        /// Total products in the index - across all products.
        /// </summary>
        public long TotalProducts
        {
            get
            {
                return m_SqlCommands.GetProductCount();
            }
        }

        /// <summary>
        /// Number of times that a sync has taken place since the last full resync.
        /// </summary>
        public int SyncCount
        {
            get
            {
                return m_SqlCommands.GetControlData().SyncCount;
            }

            set
            {
                StackHashControlData controlData = m_SqlCommands.GetControlData();
                controlData.SyncCount = value;
                m_SqlCommands.UpdateControl(controlData);
            }
        }

        /// <summary>
        /// Indicates how far the previous sync got before completing.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get
            {
                return m_SqlCommands.GetControlData().LastSyncProgress;
            }

            set
            {
                StackHashControlData controlData = m_SqlCommands.GetControlData();
                controlData.LastSyncProgress = value;
                m_SqlCommands.UpdateControl(controlData);
            }
        }

        
        public string ErrorIndexName
        {
            get { return m_DatabaseName; }
        }

        public string ErrorIndexPath
        {
            get 
            {
                return m_RootCabFolder;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public ErrorIndexStatus Status
        {
            get 
            {
                try
                {
                    // Special index name is never created.
                    if (String.Compare(m_DatabaseName, "AcmeErrorIndex", StringComparison.OrdinalIgnoreCase) == 0)
                        return ErrorIndexStatus.NotCreated;

                    PathUtils.ProcessPath(m_ErrorIndexPath);

                    if (!Directory.Exists(m_ErrorIndexPath))
                        return ErrorIndexStatus.NotCreated;

                    // The database files need not be in the same folder as the cab files as the SqlServer instance
                    // may be on a different machine altogether.
                    if (m_SqlCommands.DatabaseExists(m_DatabaseName))
                        return ErrorIndexStatus.Created;
                    else
                        return ErrorIndexStatus.NotCreated;
                }
                catch (SqlException ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to connect to SQL database", ex);
                    return ErrorIndexStatus.Unknown;
                }
                catch (StackHashException ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to connect to SQL database", ex);
                    return ErrorIndexStatus.Unknown;
                }
            }
        }


        /// <summary>
        /// Performs tests on the the database and cab folders.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public ErrorIndexConnectionTestResults GetDatabaseStatus()
        {
            return m_SqlCommands.GetDatabaseStatus(m_DatabaseName, true);
        }


        public StackHashSqlConfiguration SqlSettings
        {
            get
            {
                return m_SqlConfig;
            }

            set
            {
                processSqlSettings(value, m_DatabaseName);
            }
        }

        
        public StackHashSynchronizeStatistics Statistics
        {
            get { return null; }
        }


        #endregion Properties


        #region Events

        public event EventHandler<ErrorIndexEventArgs> IndexUpdated;

        // Delegate to hear about additions to the update table.
        public event EventHandler<ErrorIndexEventArgs> IndexUpdateAdded;

        // Delegate to hear about the progress of an index move.
        public event EventHandler<ErrorIndexMoveEventArgs> IndexMoveProgress;

        /// <summary>
        /// Notify upstream objects of a change to the error index.
        /// </summary>
        /// <param name="e">Identifies the change.</param>
        public void OnErrorIndexChanged(ErrorIndexEventArgs e)
        {
            EventHandler<ErrorIndexEventArgs> handler = IndexUpdated;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Notify upstream objects of a change to update table.
        /// </summary>
        /// <param name="e">Identifies the change.</param>
        public void OnUpdateTableChanged(ErrorIndexEventArgs e)
        {
            EventHandler<ErrorIndexEventArgs> handler = IndexUpdateAdded;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Notify upstream objects of progress during an index move.
        /// </summary>
        /// <param name="e">Identifies the progress.</param>
        public void OnErrorIndexMoveProgress(ErrorIndexMoveEventArgs e)
        {
            EventHandler<ErrorIndexMoveEventArgs> handler = IndexMoveProgress;

            if (handler != null)
                handler(this, e);
        }

        
        #endregion Events


        #region PrivateMethods


        /// <summary>
        /// Gets the number of events recorded against a product.
        /// </summary>
        /// <returns>Number of events.</returns>
        private int getTotalProductEvents(long productId)
        {
            return m_SqlCommands.GetProductEventCount(productId);
        }

        /// <summary>
        /// Gets the number of events recorded against the listed products.
        /// This accounts for overlaps where events might be shared between files which are
        /// shared between products.
        /// </summary>
        /// <returns>Number of events.</returns>
        public long GetProductEventCount(Collection<int> products)
        {
            return m_SqlCommands.GetProductsEventCount(products);
        }


        /// <summary>
        /// Method to get the SqlDbType from the string type input.
        /// </summary>
        /// <param name="type">The type to intepret into a valid SqlDbType.</param>
        /// <returns>The SqlDbType value.</returns>
        private static SqlDbType GetSqlDbType(string type)
        {
            if (string.IsNullOrEmpty(type) == true)
            {
                throw new ArgumentNullException("type", "type cannot be null or empty.");
            }

            SqlDbType sqlDbType = SqlDbType.NVarChar;
            switch (type)
            {
                case "int":
                    sqlDbType = SqlDbType.Int;
                    break;
                case "nvarchar":
                    sqlDbType = SqlDbType.NVarChar;
                    break;
                case "smalldatetime":
                    sqlDbType = SqlDbType.SmallDateTime;
                    break;
                case "datetime":
                    sqlDbType = SqlDbType.DateTime;
                    break;
                case "char":
                    sqlDbType = SqlDbType.Char;
                    break;
                case "bigint":
                    sqlDbType = SqlDbType.BigInt;
                    break;
                case "varchar":
                    sqlDbType = SqlDbType.VarChar;
                    break;
                case "bit":
                    sqlDbType = SqlDbType.Bit;
                    break;
                default:
                    break;
            }

            return sqlDbType;
        }

        /// <summary>
        /// Method to add a stored procedure parameter to the parameter list.
        /// </summary>
        /// <param name="parameterName">Name of the paramerer.</param>
        /// <param name="type">Data type of the parameter.</param>
        /// <param name="size">Size of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="clearList">Boolean to decide whether to clear the parameter list before adding the parameter.</param>
        public void AddParameter(string parameterName, string type, int size, object value, bool clearList)
        {
            if (clearList == true)
            {
                m_ParameterList.Clear();
            }

            //
            // convert the type to a SqlDbType.
            //
            SqlDbType sqlDbType = SqlErrorIndex.GetSqlDbType(type);

            DbParameter parameter = null;

            //
            // check if parameter already exists.
            //
            foreach (DbParameter p in m_ParameterList)
            {
                if (p.ParameterName == parameterName)
                {
                    parameter = p;
                    break;
                }
            }

            //
            // not found, create new sql parameter.
            //
            if (parameter == null)
            {
                parameter = m_ProviderFactory.CreateParameter();
            }


            //
            //
            // add the name, type and value to the sql parameter.
            parameter.ParameterName = parameterName;
            ((SqlParameter)parameter).SqlDbType = sqlDbType;
            if (size != 0)
            {
                parameter.Size = size;
            }
            parameter.Value = value;

            //
            // add the parameter to the parameter list.
            //
            m_ParameterList.Add(parameter);

        }

        #endregion PrivateMethods


        #region GeneralMethods

        
        /// <summary>
        /// Updates the statistics for a particular product.
        /// </summary>
        /// <param name="product">The product whose stats are to be updated.</param>
        /// <returns></returns>
        public StackHashProduct UpdateProductStatistics(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Load the product details.
            product.TotalStoredEvents = getTotalProductEvents(product.Id);

            // Store the result.
            AddProduct(product, true);

            return product;
        }


        /// <summary>
        /// Delete the index in the specified folder. 
        /// Note this does not assume that the specified index is the current index. 
        /// e.g. during a Move, the new index location becomes current and the old one
        /// is deleted.
        /// </summary>
        /// <param name="errorIndexRoot"></param>
        /// <param name="errorIndexName"></param>
        public void DeleteIndex(String errorIndexRoot, String errorIndexName)
        {
            if (m_IsActive)
                throw new InvalidOperationException("Index not accessible while activated");

            SqlConnection.ClearAllPools();

            if (Status == ErrorIndexStatus.Created)
                m_SqlCommands.DeleteDatabase(errorIndexName);

            String errorIndexPath = Path.Combine(errorIndexRoot, errorIndexName);

            // Delete the cab folder too.
            if (Directory.Exists(errorIndexPath))
                PathUtils.DeleteDirectory(errorIndexPath, true);
        }

        /// <summary>
        /// Deletes the SQL database completely.
        /// </summary>
        public void DeleteIndex()
        {
            DeleteIndex(m_RootCabFolder, m_DatabaseName);
        }


        private void processSqlSettings(StackHashSqlConfiguration sqlSettings, String newErrorIndexName)
        {
            m_SqlConfig = sqlSettings;
            m_SqlConfig.InitialCatalog = "MASTER";
            m_MasterConnectionString = m_SqlConfig.ToConnectionString();
            m_SqlConfig.InitialCatalog = newErrorIndexName;
            m_ConnectionString = m_SqlConfig.ToConnectionString();

            if (m_SqlCommands != null)
                m_SqlCommands.Dispose();

            m_SqlCommands = new SqlCommands(m_ProviderFactory, m_ConnectionString, m_MasterConnectionString, m_SqlConfig.MaxPoolSize);
        }


        /// <summary>
        /// Called by the CopyDirectory method for each file copied.
        /// </summary>
        /// <param name="isCopyStarted">true - staring, false - completed.</param>
        /// <param name="currentFile">Current file being copied.</param>
        /// <returns>true - aborts the copy, false - continue.</returns>
        private bool moveIndexCopyDirectoryCallback(bool isCopyStarted, String currentFile)
        {
            m_MoveIndexCopyFileCount++;

            OnErrorIndexMoveProgress(new ErrorIndexMoveEventArgs(currentFile, m_MoveIndexCopyFileCount, isCopyStarted));
            
            if (m_MoveAbortRequested)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Moves an index to a new location.
        /// </summary>
        /// <param name="newErrorIndexPath"></param>
        /// <param name="newErrorIndexName"></param>
        /// <param name="sqlConfig">Sql configuration name.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public void MoveIndex(string newErrorIndexPath, string newErrorIndexName, StackHashSqlConfiguration sqlSettings, bool allowPhysicalMove)
        {
            if (newErrorIndexPath == null)
                throw new ArgumentNullException("newErrorIndexPath");
            if (newErrorIndexName == null)
                throw new ArgumentNullException("newErrorIndexName");
            
            // Default the sql settings if not specified.
            if (sqlSettings == null)
                sqlSettings = m_SqlConfig;

            if (m_IsActive)
                throw new InvalidOperationException("Index not accessible while activated");

            bool databaseNameChanged = String.Compare(newErrorIndexName, m_DatabaseName, StringComparison.OrdinalIgnoreCase) != 0;
            bool databasePathChanged = String.Compare(PathUtils.ProcessPath(newErrorIndexPath), PathUtils.ProcessPath(m_RootCabFolder), StringComparison.OrdinalIgnoreCase) != 0;

            // Don't do anything if the index hasn't changed.
            if (!databasePathChanged && !databaseNameChanged)
                return;

            // Flags to indicate the "unwinding" of this move if it fails mid flow.
            bool deleteOldFolder = false;
            bool directoryMoved = false;
            bool databaseFileRenamed = false;
            bool databaseLogRenamed = false;
            bool databaseRenamed = false;

            // Record the root destination folder and the subfolder. Add a backslash to the path
            // if necessary.
            String newErrorIndexPathWithSubfolder;
            String newErrorIndexRoot;

            if (!newErrorIndexPath.EndsWith("\\", true, CultureInfo.InstalledUICulture))
            {
                newErrorIndexPathWithSubfolder = newErrorIndexPath + "\\" + newErrorIndexName;
                newErrorIndexRoot = newErrorIndexPath + "\\";
            }
            else
            {
                newErrorIndexPathWithSubfolder = newErrorIndexPath + newErrorIndexName;
                newErrorIndexRoot = newErrorIndexPath;
            }

            // Determine the database file names that are to change.
            String oldDatabaseDataFile = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.mdf", newErrorIndexPathWithSubfolder, m_DatabaseName);
            String oldDatabaseLogFile = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.ldf", newErrorIndexPathWithSubfolder, m_DatabaseName);
            String newDatabaseDataFile = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.mdf", newErrorIndexPathWithSubfolder, newErrorIndexName);
            String newDatabaseLogFile = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.ldf", newErrorIndexPathWithSubfolder, newErrorIndexName);


            // Indicate that a move is in progress so the profile cannot be activated.            
            m_MoveInProgress = true;
            m_MoveAbortRequested = false;

            sqlSettings.InitialCatalog = "MASTER";
            String newMasterConnectionString = sqlSettings.ToConnectionString();
            sqlSettings.InitialCatalog = newErrorIndexName;
            String newDatabaseConnectionString = sqlSettings.ToConnectionString();

            SqlCommands newCommands = new SqlCommands(m_ProviderFactory, newDatabaseConnectionString, newMasterConnectionString, sqlSettings.MaxPoolSize);

            try
            {
                // In some cases all we want to do is record the new settings. i.e. there is no actual move to 
                // perform because either the source database doesn't exist yet or the user wants to switch the 
                // profile to using an alternative existing database.
                // Therefore...
                // If the source database hasn't been created yet then just switch to the new settings.
                // If the destination database exists then don't move either - just switch to the new settings unless
                // the database name is the same in which case we are moving the existing database to a new folder
                // but keeping the name the same.


                if ((String.Compare(m_DatabaseName, "AcmeErrorIndex", StringComparison.OrdinalIgnoreCase) == 0) ||
                    (Status == ErrorIndexStatus.NotCreated) ||
                    (databaseNameChanged && newCommands.DatabaseExists(newErrorIndexName)))
                {
                    // Never been activated so just record the new settings.
                    m_ErrorIndexPath = newErrorIndexPathWithSubfolder;
                    m_RootCabFolder = PathUtils.AddDirectorySeparator(newErrorIndexRoot);
                    m_DatabaseName = newErrorIndexName;

                    processSqlSettings(sqlSettings, newErrorIndexName);
                    return;
                }


                // At this point is has been determined that the original folder DOES exist. i.e. a database has been
                // created - or at least the Cab folder has. Therefore, we are dealing with a physical move from the 
                // current location to a new location OR a change of name OR both.

                // AllowPhysicalMove is set false when the user is updating the profile settings.
                // This just catches the programmer error case where the client tries to do a physical move through the UpdateContextSettings API 
                // rather than the MoveIndex API. The former is only to be used for initial setup. Both come through to this function though.
                if (!allowPhysicalMove)
                    throw new StackHashException("Cannot move index through settings if index exists", StackHashServiceErrorCode.CannotChangeIndexFolderOnceCreated);


                // When performing a physical move - the destination folder cannot exist.
                // If the destination folder (with subfolder) exists already then it may be that a database is already present in that folder
                // Don't allow this database to be moved to that same folder.
                if (Directory.Exists(newErrorIndexPathWithSubfolder))
                    throw new StackHashException("Cannot move to existing folder", StackHashServiceErrorCode.CannotMoveToExistingFolder);

                String originalIndexRoot = m_RootCabFolder;
                String originalIndexName = m_DatabaseName;
                String originalIndexPathWithSubfolder = Path.Combine(originalIndexRoot, originalIndexName);

                // Determine if the original cab folder contains the database too. If not then the database is assumed to be 
                // in a location managed by SqlServer. 
                // Databases managed by SqlServer can be renamed but their filenames cannot be changed. This causes a bit of an 
                // inconsistency. Also, it may be possible that privileges are not given to the NETWORK SERVICE to change the 
                // database name etc... 
                String [] databaseFiles = Directory.GetFiles(originalIndexPathWithSubfolder, "*.mdf");
                bool databaseInCabFolder = (databaseFiles.Length > 0);

                bool isDatabaseOffline = false;
                if (databaseInCabFolder || databaseNameChanged)
                {
                    // Set the Sql database OFFLINE before the moving the database files.
                    // This should fail with a permissions error if the database is being managed by SqlServer and the 
                    // name is being changed. Nothing will have actually been done yet so the database settings are still 
                    // in a consistent state.
                    m_SqlCommands.SetDatabaseOnlineState(m_DatabaseName, false);
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Database set offline.");
                    isDatabaseOffline = true;
                }


                try
                {
                    // MOVE THE CAB FILE FOLDER. 
                    // File.Move does not work across volumes so File.Copy is necessary.
                    // Also does not work across UNC paths.
                    if ((newErrorIndexPath[0] != '\\') && (m_RootCabFolder[0] != '\\') &&
                        (m_RootCabFolder.ToUpperInvariant()[0] == newErrorIndexPath.ToUpperInvariant()[0]))
                    {
                        // The name is used as the subfolder where the index is stored.
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Moving {0} to {1}", m_ErrorIndexPath, newErrorIndexPathWithSubfolder));
                        Directory.Move(m_ErrorIndexPath, newErrorIndexPathWithSubfolder);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Move complete");
                        directoryMoved = true;
                    }
                    else
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Copying {0} to {1}", m_ErrorIndexPath, newErrorIndexPathWithSubfolder));
                        PathUtils.CopyDirectory(m_ErrorIndexPath, newErrorIndexPathWithSubfolder, new CopyDirectoryCallback(this.moveIndexCopyDirectoryCallback));
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Copy complete");
                        deleteOldFolder = true;
                    }

                    // Rename the database files within the destination folder if the name has changed.
                    if (databaseInCabFolder && databaseNameChanged)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Renaming {0} to {1}", oldDatabaseDataFile, newDatabaseDataFile));
                        File.Move(oldDatabaseDataFile, newDatabaseDataFile);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Rename complete");
                        databaseFileRenamed = true;

                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Renaming {0} to {1}", oldDatabaseLogFile, newDatabaseLogFile));
                        File.Move(oldDatabaseLogFile, newDatabaseLogFile);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Rename complete");
                        databaseLogRenamed = true;
                    }

                    // Tell SqlServer of the change in database file locations. This is necessary if the database is stored
                    // in the cab file folder. If the database is stored by SqlServer then the location will not have changed.
                    if (databaseInCabFolder)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Changing database settings");
                        m_SqlCommands.ChangeDatabaseLocation(m_DatabaseName, newErrorIndexName, newErrorIndexPathWithSubfolder);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Database settings changed");
                    }

                    // Set the database online again.
                    if (isDatabaseOffline)
                    {
                        m_SqlCommands.SetDatabaseOnlineState(m_DatabaseName, true);
                        isDatabaseOffline = false;
                    }

                    // Rename the database.
                    if (databaseNameChanged)
                    {
                        m_SqlCommands.RenameDatabase(m_DatabaseName, newErrorIndexName);
                        databaseRenamed = true;

                        m_SqlCommands.ChangeDatabaseLogicalNames(m_DatabaseName, newErrorIndexName, true);
                    }
                }
                catch (System.Exception ex)
                {
                    // REWIND ALL CHANGES IF POSSIBLE.
                    if (databaseRenamed)
                        m_SqlCommands.RenameDatabase(newErrorIndexName, m_DatabaseName);

                    deleteOldFolder = false; // Don't delete anything.
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Failed to move index - attempting rewind.", ex);

                    if (databaseInCabFolder || databaseNameChanged)
                    {
                        m_SqlCommands.SetDatabaseOnlineState(m_DatabaseName, false);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Database set offline.");
                        isDatabaseOffline = true;
                    }

                    
                    if (databaseLogRenamed)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Renaming {0} to {1}", newDatabaseLogFile, oldDatabaseLogFile));
                        File.Move(newDatabaseLogFile, oldDatabaseLogFile);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Rename complete");
                    }

                    if (databaseFileRenamed)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Renaming {0} to {1}", newDatabaseDataFile, oldDatabaseDataFile));
                        File.Move(newDatabaseDataFile, oldDatabaseDataFile);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Rename complete");
                    }

                    if (directoryMoved)
                    {
                        // The name is used as the subfolder where the index is stored.
                        // If a File.Copy took place then the old index will not have been deleted yet so nothing to do.
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format(CultureInfo.InvariantCulture, "Moving {0} to {1}", newErrorIndexPathWithSubfolder, m_ErrorIndexPath));
                        Directory.Move(newErrorIndexPathWithSubfolder, m_ErrorIndexPath);
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Move complete");
                    }

                    throw;
                }
                finally
                {
                    // Always leave the database online.
                    if (isDatabaseOffline)
                    {
                        m_SqlCommands.SetDatabaseOnlineState(m_DatabaseName, true);
                        isDatabaseOffline = false;
                    }
                }

                // Record the new index settings.
                m_ErrorIndexPath = newErrorIndexPathWithSubfolder;
                m_DatabaseName = newErrorIndexName;
                m_RootCabFolder = PathUtils.AddDirectorySeparator(newErrorIndexPath);

                processSqlSettings(sqlSettings, newErrorIndexName);

                // Delete the original cab folder if a File.Copy took place.
                if (deleteOldFolder)
                {
                    if (Directory.Exists(originalIndexPathWithSubfolder))
                        PathUtils.DeleteDirectory(originalIndexPathWithSubfolder, true);
                }
            }
            finally
            {
                if (newCommands != null)
                    newCommands.Dispose();

                m_MoveInProgress = false;
            }
        }


        public void Activate()
        {
            Activate(true, false);
        }

        /// <summary>
        /// Creates the index if necessary or initializes an existing one.
        /// Set allowIndexCreation for test mode only.
        /// </summary>
        /// <param name="allowIndexCreation">True - create the index if it doesn't exist, False - don't create.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void Activate(bool allowIndexCreation, bool createIndexInDefaultLocation)
        {
            try
            {
                if (!m_IsActive)
                {
                    if (m_MoveInProgress)
                        throw new InvalidOperationException("Cannot active - error index move in progress");

                    if (!Directory.Exists(m_ErrorIndexPath))
                        Directory.CreateDirectory(m_ErrorIndexPath);

                    if (allowIndexCreation)
                    {
                        try
                        {
                            // Create database if necessary - only in test mode.
                            SqlSchema.CreateStackHashDatabase(m_SqlCommands.SqlUtilities, m_ErrorIndexPath, m_DatabaseName, createIndexInDefaultLocation);
                        }
                        catch (System.Exception ex)
                        {
                            throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToCreateDatabase);
                        }
                    }

                    if (m_SqlCommands.DatabaseExists(m_DatabaseName))
                    {
                        if (!m_SqlCommands.ControlExists(1))
                        {
                            // Database was not created but no control data existed so add it.
                            // This will happen if the StackHash config tool creates the index.
                            // Set the initial control data.
                            m_SqlCommands.AddControl(new StackHashBusinessObjects.StackHashControlData(SqlErrorIndex.CurrentDatabaseVersion, 0, new StackHashSyncProgress()));
                        }


                        // This call only returns true if the database is up to date.
                        if (m_SqlCommands.UpgradeDatabase(m_DatabaseName))
                        {
                            StackHashControlData controlData = m_SqlCommands.GetControlData();
                            if (controlData.DatabaseVersion != SqlErrorIndex.CurrentDatabaseVersion)
                            {
                                controlData.DatabaseVersion = SqlErrorIndex.CurrentDatabaseVersion;
                                m_SqlCommands.UpdateControl(controlData);
                            }

                            // Add workflow mappings if not already present.
                            StackHashMappingCollection mappings = m_SqlCommands.GetMappings(StackHashMappingType.WorkFlow);
                            StackHashMappingCollection defaultWorkFlowMappings = StackHashMappingCollection.DefaultWorkFlowMappings;
                            if (mappings.Count == 0)
                            {
                                m_SqlCommands.AddMappings(defaultWorkFlowMappings);
                            }
                        }
                    }

                    m_IsActive = true;
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                throw new StackHashException("Unable to access SQL index folder. Ensure that the Stack Hash service account has access to the folder",
                    ex, StackHashServiceErrorCode.ErrorIndexAccessDenied);
            }
        }

        public void Deactivate()
        {
            m_IsActive = false;
        }


        /// <summary>
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public DateTime GetLastSyncTimeLocal(int productId)
        {
            return m_SqlCommands.GetProductControlData(productId).LastSyncTime;   
        }


        /// <summary>
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="lastSyncTime"></param>
        public void SetLastSyncTimeLocal(int productId, DateTime lastSyncTime)
        {
            StackHashProductControlData productControlData = m_SqlCommands.GetProductControlData(productId);
            productControlData.LastSyncTime = lastSyncTime;
            if (!m_SqlCommands.ProductControlExists(productId))
                m_SqlCommands.AddProductControl(productId, productControlData);
            else
                m_SqlCommands.UpdateProductControl(productId, productControlData);
        }

        /// <summary>
        /// Gets the last time that a specified product sync started.
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId">Identifies the product.</param>
        /// <returns>Time last sync was started.</returns>
        public DateTime GetLastSyncStartedTimeLocal(int productId)
        {
            return m_SqlCommands.GetProductControlData(productId).LastSyncStartedTime;
        }


        /// <summary>
        /// Sets the last time the product sync was started.
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId">Product to be set.</param>
        /// <param name="lastSyncTime">Last time the product started a sync.</param>
        public void SetLastSyncStartedTimeLocal(int productId, DateTime lastSyncTime)
        {
            StackHashProductControlData productControlData = m_SqlCommands.GetProductControlData(productId);
            productControlData.LastSyncStartedTime = lastSyncTime;
            if (!m_SqlCommands.ProductControlExists(productId))
                m_SqlCommands.AddProductControl(productId, productControlData);
            else
                m_SqlCommands.UpdateProductControl(productId, productControlData);
        }


        /// <summary>
        /// Gets the last time that a specified product sync completed.
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId">Identifies the product.</param>
        /// <returns>Time last sync was completed.</returns>
        public DateTime GetLastSyncCompletedTimeLocal(int productId)
        {
            return m_SqlCommands.GetProductControlData(productId).LastSyncCompletedTime;
        }


        /// <summary>
        /// Sets the last time the product sync was completed.
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId">Product to be set.</param>
        /// <param name="lastSyncTime">Last time the product completed a sync.</param>
        public void SetLastSyncCompletedTimeLocal(int productId, DateTime lastSyncTime)
        {
            StackHashProductControlData productControlData = m_SqlCommands.GetProductControlData(productId);
            productControlData.LastSyncCompletedTime = lastSyncTime;
            if (!m_SqlCommands.ProductControlExists(productId))
                m_SqlCommands.AddProductControl(productId, productControlData);
            else
                m_SqlCommands.UpdateProductControl(productId, productControlData);
        }


        /// <summary>
        /// Gets the most recent hit date for the product.
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId">Identifies the product.</param>
        /// <returns>The most recent hit date.</returns>
        public DateTime GetLastHitTimeLocal(int productId)
        {
            return m_SqlCommands.GetProductControlData(productId).LastHitTime;
        }


        /// <summary>
        /// Sets the most recent hit date for a product.
        /// Stored as local time and converted to UTC when sent out of the service.
        /// </summary>
        /// <param name="productId">Product to be set.</param>
        /// <param name="lastHitTime">Most recent hit date.</param>
        public void SetLastHitTimeLocal(int productId, DateTime lastHitTime)
        {
            StackHashProductControlData productControlData = m_SqlCommands.GetProductControlData(productId);
            productControlData.LastHitTime = lastHitTime;

            if (!m_SqlCommands.ProductControlExists(productId))
                m_SqlCommands.AddProductControl(productId, productControlData);
            else
                m_SqlCommands.UpdateProductControl(productId, productControlData);
        }



        /// <summary>
        /// Gets the status and stats for the specified task type.
        /// </summary>
        /// <param name="taskType">Task type.</param>
        /// <returns>Full stats and status for the task.</returns>
        public StackHashTaskStatus GetTaskStatistics(StackHashTaskType taskType)
        {
            if (Status == ErrorIndexStatus.Created)
            {
                return m_SqlCommands.GetTaskControlData(taskType);
            }
            else
            {
                StackHashTaskStatus controlData = new StackHashTaskStatus();
                controlData.FailedCount = 0;
                controlData.LastDurationInSeconds = 0;
                controlData.LastException = null;
                controlData.LastFailedRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
                controlData.LastStartedTimeUtc = new DateTime(0, DateTimeKind.Utc);
                controlData.LastSuccessfulRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
                controlData.RunCount = 0;
                controlData.ServiceErrorCode = StackHashServiceErrorCode.NoError;
                controlData.SuccessCount = 0;
                controlData.TaskState = StackHashTaskState.NotRunning;
                controlData.TaskType = taskType;
                return controlData;
            }
        }

        public void SetTaskStatistics(StackHashTaskStatus taskStatus)
        {
            if (taskStatus == null)
                throw new ArgumentNullException("taskStatus");

            if (!m_SqlCommands.TaskControlExists(taskStatus.TaskType))
                m_SqlCommands.AddTaskControl(taskStatus.TaskType, taskStatus);
            else
                m_SqlCommands.UpdateTaskControl(taskStatus.TaskType, taskStatus);
        }


        /// <summary>
        /// Request to abort the current operation has been received. 
        /// If a move is currently in progress then abort it by setting the move abort indicator.
        /// </summary>
        public void AbortCurrentOperation()
        {
            if (m_MoveInProgress)
                m_MoveAbortRequested = true;
        }

        #endregion GeneralMethods


        #region ProductMethods

        /// <summary>
        /// Adds or updates the product updating ONLY the WinQual fields.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public StackHashProduct AddProduct(StackHashProduct product)
        {
            return AddProduct(product, false);
        }

        
        /// <summary>
        /// Adds a product to the SQL database.
        /// </summary>
        /// <param name="product">Product to add.</param>
        /// <param name="updateNonWinQualFields">True if all non-WinQual fields are to be updated.</param>
        /// <returns>A copy of the product.</returns>
        public StackHashProduct AddProduct(StackHashProduct product, bool updateNonWinQualFields)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            StackHashProduct oldProduct = GetProduct(product.Id);

            if (oldProduct == null)
            {
                m_SqlCommands.AddProduct(product);

                StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate(StackHashDataChanged.Product, StackHashChangeType.NewEntry, product.Id, 0, 0, null, 0, product.Id);
                OnErrorIndexChanged(new ErrorIndexEventArgs(update));

                if (m_UpdateTableActive)
                {
                    AddUpdate(update);
                }
            }
            else
            {
                m_SqlCommands.UpdateProduct(product, updateNonWinQualFields);

                StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate(StackHashDataChanged.Product, StackHashChangeType.UpdatedEntry, product.Id, 0, 0, null, 0, product.Id);
                OnErrorIndexChanged(new ErrorIndexEventArgs(update));

                if (m_UpdateTableActive)
                {
                    // Now record the changes if anything significant has changed.
                    bool reportChanges = oldProduct.ShouldReportToBugTrackPlugIn(product, updateNonWinQualFields);

                    if (reportChanges && m_UpdateTableActive)
                    {
                        AddUpdate(update);
                    }
                }
            }

            return product;
        }


        /// <summary>
        /// Checks if the product with the specified ID exists in the database.
        /// </summary>
        /// <param name="product">Product whos ID is to be found.</param>
        /// <returns>True - product is in the database. False - not present.</returns>
        public bool ProductExists(StackHashProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            return m_SqlCommands.ProductExists(product.Id);
        }


        /// <summary>
        /// Gets a list of all products in the database.
        /// </summary>
        /// <returns>List of products.</returns>
        public StackHashProductCollection LoadProductList()
        {
            return m_SqlCommands.GetProducts();
        }


        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public StackHashProduct GetProduct(int productId)
        {
            if (m_SqlCommands.ProductExists(productId))
                return m_SqlCommands.GetProduct(productId);
            else
                return null;
        }
        
        #endregion ProductMethods


        #region FileMethods


        /// <summary>
        /// Adds a file to the index.
        /// This involves updating 2 tables.
        /// The file is first added to the Files table.
        /// The file is then associated with the product by adding it to the ProductFiles table.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="file"></param>
        public void AddFile(StackHashProduct product, StackHashFile file)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");


            StackHashFile originalFile = m_SqlCommands.GetFile(file.Id);

            bool reportChanges = false;
            StackHashChangeType reportChangeType = StackHashChangeType.NewEntry;

            if (m_SqlCommands.FileExists(file.Id))
            {
                m_SqlCommands.UpdateFile(file);

                // Now record the changes if anything significant has changed.
                reportChanges = originalFile.ShouldReportToBugTrackPlugIn(file);

                if (reportChanges)
                    reportChangeType = StackHashChangeType.UpdatedEntry;
            }
            else
            {
                m_SqlCommands.AddFile(file);
                reportChanges = true;
                reportChangeType = StackHashChangeType.NewEntry;
            }

            if (!m_SqlCommands.ProductFileExists(product.Id, file.Id))
            {
                // Add an entry to indicate that this file is now associated with another product.
                m_SqlCommands.AddProductFile(product, file);
            }

            // Report as the last thing we do.
            if (reportChanges)
            {
                StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate(StackHashDataChanged.File, reportChangeType, product.Id, file.Id, 0, null, 0, file.Id);
                OnErrorIndexChanged(new ErrorIndexEventArgs(update));
                if (m_UpdateTableActive)
                {
                    AddUpdate(update);
                }
            }
        }


        /// <summary>
        /// Checks if the specified file exists in the database and is associated with the specified product.
        /// A file may have been added to the database because it is associated with another product.
        /// In this case return FALSE so that an AddFile can add the product/file link.
        /// </summary>
        /// <param name="product">Product for which we are concerned.</param>
        /// <param name="file">File to check.</param>
        /// <returns></returns>
        public bool FileExists(StackHashProduct product, StackHashFile file)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");

            if (m_SqlCommands.FileExists(file.Id) && m_SqlCommands.ProductFileExists(product.Id, file.Id))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Loads a list of files associated with the specified product.
        /// </summary>
        /// <param name="product">Product for which the files are required.</param>
        /// <returns></returns>
        public StackHashFileCollection LoadFileList(StackHashProduct product)
        {
            return m_SqlCommands.GetFiles(product);
        }


        /// <summary>
        /// Gets the specified product file.
        /// </summary>
        /// <param name="product">The product for which the file is required.</param>
        /// <param name="fileId">ID of the file to get.</param>
        /// <returns>File data or null</returns>
        public StackHashFile GetFile(StackHashProduct product, int fileId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (!m_SqlCommands.ProductFileExists(product.Id, fileId))
                return null;
            else
                return m_SqlCommands.GetFile(fileId);
        }

        #endregion FileMethods


        #region EventMethods

        /// <summary>
        /// Adds an event to the database and associates it with a particular file.
        /// </summary>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to add.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            AddEvent(product, file, theEvent, false, true);
        }

        /// <summary>
        /// Adds an event to the database and associates it with a particular file.
        /// </summary>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">Determins if non-winqual fields should be updated if the event already exists.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields)
        {
            AddEvent(product, file, theEvent, updateNonWinQualFields, true);
        }

        /// <summary>
        /// Adds an event to the database and associates it with a particular file.
        /// </summary>
        /// <param name="product">Product to which the event belongs.</param>
        /// <param name="file">File to which the event belongs.</param>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">Determins if non-winqual fields should be updated if the event already exists.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields, bool reportToBugTrackers)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            StackHashEvent originalEvent = GetEvent(product, file, theEvent);

            bool reportUpdate = false;
            StackHashChangeType changeType = StackHashChangeType.NewEntry;


            if (!m_SqlCommands.EventExists(theEvent))
            {
                m_SqlCommands.AddEvent(theEvent);

                if (m_TotalStoredEvents == -1)
                    m_TotalStoredEvents = TotalStoredEvents;  // Property forces the value to be cached.
                else
                    m_TotalStoredEvents++;

                reportUpdate = true;
                changeType = StackHashChangeType.NewEntry;
            }
            else
            {
                m_SqlCommands.UpdateEvent(theEvent, updateNonWinQualFields);

                // Now record the changes if anything significant has changed.
                reportUpdate = originalEvent.ShouldReportToBugTrackPlugIn(theEvent, updateNonWinQualFields);
                changeType = StackHashChangeType.UpdatedEntry;
            }

            if (!m_SqlCommands.FileEventExists(file, theEvent))
                m_SqlCommands.AddFileEvent(file, theEvent);

            if (reportUpdate && reportToBugTrackers)
            {
                StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate(StackHashDataChanged.Event, changeType, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, 0, theEvent.Id);
                OnErrorIndexChanged(new ErrorIndexEventArgs(update));
                if (m_UpdateTableActive)
                {
                    AddUpdate(update);
                }
            }

        }


        /// <summary>
        /// Determines if the specified event exists and is associated with the specified product/file.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to check.</param>
        /// <returns>true - the event exists, false otherwise</returns>
        public bool EventExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (m_SqlCommands.EventExists(theEvent) && m_SqlCommands.FileEventExists(file, theEvent)) // && m_SqlCommands.FileEventExists(file, theEvent))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Gets the specified event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to get.</param>
        /// <returns>The event object.</returns>
        public StackHashEvent GetEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            return m_SqlCommands.GetEvent(file.Id, theEvent.Id, theEvent.EventTypeName);
        }

        
        /// <summary>
        /// Loads all events for the specified product and file.
        /// </summary>
        /// <param name="product">The product to load events for.</param>
        /// <param name="file">The file to load events for.</param>
        /// <returns>Collection of all the event data.</returns>
        public StackHashEventCollection LoadEventList(StackHashProduct product, StackHashFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            return m_SqlCommands.LoadEventList(file.Id);
        }



        /// <summary>
        /// Assigns the cabs in the list to the specified event package - matched on event id and event type name.
        /// </summary>
        /// <param name="allEvents">All event packages.</param>
        /// <param name="cabs">Cabs to process</param>
        private static void processCabs(List<StackHashEventPackage> allEvents, StackHashCabCollection cabs)
        {
            Dictionary<Tuple<int, String>, StackHashCabCollection> sortedCabs = new Dictionary<Tuple<int, String>, StackHashCabCollection>();
            foreach (StackHashCab cab in cabs)
            {
                Tuple<int, String> thisKey = new Tuple<int, String>(cab.EventId, cab.EventTypeName);

                if (!sortedCabs.ContainsKey(thisKey))
                    sortedCabs.Add(thisKey, new StackHashCabCollection());

                sortedCabs[thisKey].Add(cab);
            }


            foreach (StackHashEventPackage theEvent in allEvents)
            {
                Tuple<int, String> thisKey = new Tuple<int, String>(theEvent.EventData.Id, theEvent.EventData.EventTypeName);

                StackHashCabCollection matchingCabs;
                if (sortedCabs.TryGetValue(thisKey, out matchingCabs))
                    theEvent.Cabs = new StackHashCabPackageCollection(matchingCabs);
                else
                    theEvent.Cabs = new StackHashCabPackageCollection();
            }
        }


        /// <summary>
        /// Assigns the event infos in the list to the specified event package - matched on event id and event type name.
        /// Note that the event infos should be listed in the order in which the id/type appear in the event list.
        /// </summary>
        /// <param name="allEvents">All event packages.</param>
        /// <param name="eventInfos">Hits to process</param>
        private static void processEventInfos(List<StackHashEventPackage> allEvents, StackHashEventInfoPackageCollection eventInfos)
        {
            Dictionary<Tuple<int, String>, StackHashEventInfoCollection> sortedEventInfos = new Dictionary<Tuple<int, String>, StackHashEventInfoCollection>();
            foreach (StackHashEventInfoPackage eventInfo in eventInfos)
            {
                Tuple<int, String> thisKey = new Tuple<int, String>(eventInfo.EventId, eventInfo.EventTypeName);

                if (!sortedEventInfos.ContainsKey(thisKey))
                    sortedEventInfos.Add(thisKey, new StackHashEventInfoCollection());

                sortedEventInfos[thisKey].Add(eventInfo.EventInfo);
            }


            foreach (StackHashEventPackage theEvent in allEvents)
            {
                Tuple<int, String> thisKey = new Tuple<int, String>(theEvent.EventData.Id, theEvent.EventData.EventTypeName);


                StackHashEventInfoCollection matchingEventInfos;
                if (sortedEventInfos.TryGetValue(thisKey, out matchingEventInfos))
                    theEvent.EventInfoList = matchingEventInfos;
                else
                    theEvent.EventInfoList = new StackHashEventInfoCollection();
            }
        }


        /// <summary>
        /// Gets all the events associated with a particular product.
        /// </summary>
        /// <param name="product">The product whose events are required.</param>
        /// <returns></returns>
        public StackHashEventPackageCollection GetProductEvents(StackHashProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            StackHashEventPackageCollection allEvents = m_SqlCommands.GetProductEvents(product.Id);

            int eventsPerBlock = m_SqlConfig.EventsPerBlock;
            List<StackHashEventPackage> eventBlock = new List<StackHashEventPackage>();

            for (int eventNum = 0; eventNum < allEvents.Count; eventNum++)
            {
                eventBlock.Add(allEvents[eventNum]);

                if ((eventNum > 0) && ((eventNum % eventsPerBlock) == 0))
                {
                    StackHashCabCollection cabs = m_SqlCommands.GetCabs(eventBlock);
                    processCabs(eventBlock, cabs);

                    StackHashEventInfoPackageCollection eventInfos = m_SqlCommands.GetEventInfoPackageCollection(eventBlock);
                    processEventInfos(eventBlock, eventInfos);

                    eventBlock.Clear();
                }
            }

            if (eventBlock.Count > 0)
            {
                // Process the last few events.
                StackHashCabCollection cabs = m_SqlCommands.GetCabs(eventBlock);
                processCabs(eventBlock, cabs);

                StackHashEventInfoPackageCollection eventInfos = m_SqlCommands.GetEventInfoPackageCollection(eventBlock);
                processEventInfos(eventBlock, eventInfos);
            }

            return allEvents;
        }

        
        /// <summary>
        /// Gets all events matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteriaCollection">The search criteria to match on.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection, StackHashProductSyncDataCollection enabledProducts)
        {
            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            StackHashSortOrderCollection defaultSortOrder = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true)
            };

            return GetWindowedEvents(searchCriteriaCollection, 1, int.MaxValue, defaultSortOrder, enabledProducts);
        }


        /// <summary>
        /// Gets all events matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteriaCollection">The search criteria to match on.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts)
        {
            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            if (sortOptions == null)
                throw new ArgumentNullException("sortOptions");

            if (!sortOptions.Validate())
                throw new ArgumentException("Sort option must include only Event and EventSignature objects and must include at least one item", "sortOptions");


            return GetWindowedEvents(searchCriteriaCollection, startRow, numberOfRows, sortOptions, enabledProducts);

#if OLDCODE
            //Dictionary<int, List<String>> allEventIds = new Dictionary<int, List<String>>();
            //StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            //// Each criteria is processed individually. The events that are returned may overlap so 
            //// need to check that the same event is not being added twice.
            //foreach (StackHashSearchCriteria criteria in searchCriteriaCollection)
            //{
            //    StackHashEventPackageCollection theseEvents = new StackHashEventPackageCollection();

            //    String productSearchString = criteria.ToSqlString(StackHashObjectType.Product, "P");
            //    Collection<int> allProductIds = GetProductMatch(productSearchString);

            //    if ((criteria.ObjectCount(StackHashObjectType.Product) != 0) &&
            //        (allProductIds.Count == 0))
            //    {
            //        continue;
            //    }

            //    String fileSearchString = criteria.ToSqlString(StackHashObjectType.File, "F");
            //    StackHashFileProductMappingCollection allFileIds = GetFilesMatch(allProductIds, fileSearchString);

            //    if ((criteria.ObjectCount(StackHashObjectType.File) != 0) &&
            //        (allFileIds.Count == 0))
            //    {
            //        continue;
            //    }

            //    StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection() { criteria };

            //    theseEvents = m_SqlCommands.GetEvents(allFileIds, criteriaCollection, startRow, numberOfRows, sortOptions);

            //    int eventsPerBlock = m_SqlConfig.EventsPerBlock;
            //    List<StackHashEventPackage> eventBlock = new List<StackHashEventPackage>();

            //    foreach (StackHashEventPackage eventData in theseEvents)
            //    {
            //        // Check if the event has already been processed.
            //        bool alreadyAdded = false;
            //        if (allEventIds.ContainsKey(eventData.EventData.Id))
            //        {
            //            // See if the event type is listed.
            //            foreach (String eventType in allEventIds[eventData.EventData.Id])
            //            {
            //                if (eventType == eventData.EventData.EventTypeName)
            //                {
            //                    alreadyAdded = true;
            //                    break;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            allEventIds[eventData.EventData.Id] = new List<string>();
            //        }

            //        if (alreadyAdded)
            //            continue;

            //        allEventIds[eventData.EventData.Id].Add(eventData.EventData.EventTypeName);

            //        eventData.ProductId = allFileIds.FindFile(eventData.EventData.FileId).ProductId;

            //        eventBlock.Add(eventData);
            //        allEvents.Add(eventData);

            //        if ((eventBlock.Count > 0) && ((eventBlock.Count % eventsPerBlock) == 0))
            //        {
            //            StackHashCabCollection cabs = m_SqlCommands.GetCabs(eventBlock);
            //            processCabs(eventBlock, cabs);

            //            StackHashEventInfoPackageCollection eventInfos = m_SqlCommands.GetEventInfoPackageCollection(eventBlock);
            //            processEventInfos(eventBlock, eventInfos);

            //            eventBlock.Clear();
            //        }
            //    }

            //    // Complete the last block if necessary.
            //    if (eventBlock.Count > 0)
            //    {
            //        StackHashCabCollection cabs = m_SqlCommands.GetCabs(eventBlock);
            //        processCabs(eventBlock, cabs);

            //        StackHashEventInfoPackageCollection eventInfos = m_SqlCommands.GetEventInfoPackageCollection(eventBlock);
            //        processEventInfos(eventBlock, eventInfos);
            //    }
            //}

            //return allEvents;
#endif 
        }

        /// <summary>
        /// Gets all events matching the specified product and file.
        /// </summary>
        /// <param name="productId">Id of the product.</param>
        /// <param name="fileId">Id of the file.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetFileEvents(int productId, int fileId, long startRow, long numberOfRows)
        {
            // Define the search criteria for the events.
            StackHashSearchCriteriaCollection searchCriteria = new StackHashSearchCriteriaCollection()
                {
                    new StackHashSearchCriteria(
                        new StackHashSearchOptionCollection()
                        {
                            new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0),
                            new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, fileId, 0),
                        })
                };


            // Order by the event id and EventTypeName. Need to do both in case there is more than 1 event with the same 
            // event type name. SQL server doesn't guarantee the order in which they would be returned back so one might be 
            // missed if the event type name is not also specified here.
            StackHashSortOrderCollection sortOrder = new StackHashSortOrderCollection()
                {
                    new StackHashSortOrder(StackHashObjectType.Event, "Id", true),
                    new StackHashSortOrder(StackHashObjectType.Event, "EventTypeName", true)
                };


            return GetWindowedEvents(searchCriteria, startRow, numberOfRows, sortOrder, null);
        }

        /// <summary>
        /// Gets all events matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteriaCollection">The search criteria to match on.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetWindowedEvents(StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts)
        {
            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            if (sortOptions == null)
                throw new ArgumentNullException("sortOptions");

            if (!sortOptions.Validate())
                throw new ArgumentException("Sort option must include only Event and EventSignature objects and must include at least one item", "sortOptions");

            StackHashEventPackageCollection allEvents;

            allEvents = m_SqlCommands.GetWindowedEvents(searchCriteriaCollection, startRow, numberOfRows, sortOptions, enabledProducts);

            int eventsPerBlock = m_SqlConfig.EventsPerBlock;
            List<StackHashEventPackage> eventBlock = new List<StackHashEventPackage>();

            foreach (StackHashEventPackage eventData in allEvents)
            {
                eventBlock.Add(eventData);

                if ((eventBlock.Count > 0) && ((eventBlock.Count % eventsPerBlock) == 0))
                {
                    StackHashCabCollection cabs = m_SqlCommands.GetCabs(eventBlock);
                    processCabs(eventBlock, cabs);

                    StackHashEventInfoPackageCollection eventInfos = m_SqlCommands.GetEventInfoPackageCollection(eventBlock);
                    processEventInfos(eventBlock, eventInfos);

                    eventBlock.Clear();
                }
            }

            // Complete the last block if necessary.
            if (eventBlock.Count > 0)
            {
                StackHashCabCollection cabs = m_SqlCommands.GetCabs(eventBlock);
                processCabs(eventBlock, cabs);

                StackHashEventInfoPackageCollection eventInfos = m_SqlCommands.GetEventInfoPackageCollection(eventBlock);
                processEventInfos(eventBlock, eventInfos);
            }

            return allEvents;
        }

        /// <summary>
        /// Adds a note to the specified event.
        /// This also updates an event note if note.id != 0.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event to which the note is to be added.</param>
        /// <param name="note">The note to add.</param>
        /// <returns>The ID of the event note.</returns>
        public int AddEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");

            if (!m_SqlCommands.UserExists(note.User))
                m_SqlCommands.AddUser(note.User.ToUpperInvariant());

            if (!m_SqlCommands.SourceExists(note.Source))
                m_SqlCommands.AddSource(note.Source.ToUpperInvariant());

            int eventNoteId = 0;
            
            if (note.NoteId == 0)
                eventNoteId = m_SqlCommands.AddEventNote(theEvent, note);
            else
                eventNoteId = m_SqlCommands.UpdateEventNote(theEvent, note);

            if (m_UpdateTableActive)
            {
                AddUpdate(new StackHashBugTrackerUpdate(
                    StackHashDataChanged.EventNote, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, 
                    theEvent.EventTypeName, 0, eventNoteId));
            }
            return eventNoteId;
        }


        /// <summary>
        /// Deletes an event note.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event to which the note is to be added.</param>
        /// <param name="noteId">The note to delete.</param>
        public void DeleteEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (noteId < 1)
                throw new ArgumentException("note ID must be greater than 0", "noteId");

            m_SqlCommands.DeleteEventNote(theEvent, noteId);
        }

        /// <summary>
        /// Gets all of the notes associated with a particular event.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event for which the notes are required.</param>
        /// <returns></returns>
        public StackHashNotes GetEventNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            return m_SqlCommands.GetEventNotes(theEvent);
        }

        /// <summary>
        /// Gets the specified event note.
        /// </summary>
        /// <param name="noteId">The note entry required.</param>
        /// <returns>The requested event note or null.</returns>
        public StackHashNoteEntry GetEventNote(int noteId)
        {
            return m_SqlCommands.GetEventNote(noteId);
        }

        /// <summary>
        /// TODO: Need to change this.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="file"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool ParseEvents(StackHashProduct product, StackHashFile file, ErrorIndexEventParser parser)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (parser == null)
                throw new ArgumentNullException("parser");

            StackHashEventPackageCollection allEvents = GetEvents(parser.SearchCriteriaCollection, null);

            foreach (StackHashEventPackage nextEvent in allEvents)
            {
                parser.CurrentEvent = nextEvent.EventData;
                parser.ProcessEvent();

                if (parser.Abort)
                    return false;
            }
            return true;
        }


        public DateTime GetMostRecentHitDate(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (!ProductExists(product))
                throw new ArgumentException("Product does not exit", "product");

            if (file == null)
                throw new ArgumentNullException("file");
            if (!FileExists(product, file))
                throw new ArgumentException("File does not exit", "file");

            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (!EventExists(product, file, theEvent))
                throw new ArgumentException("Event does not exist", "theEvent");


            return m_SqlCommands.GetMostRecentHitDate(theEvent);
        }

        #endregion EventMethods


        #region EventInfoMethods

        /// <summary>
        /// Adds the specified event info to the event info list.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="file"></param>
        /// <param name="theEvent"></param>
        /// <param name="eventInfoCollection"></param>
        public void AddEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            // Get the existing event infos.
            MergeEventInfoCollection(product, file, theEvent, eventInfoCollection);
        }


        public void UpdateLocaleStatistics(int productId, StackHashProductLocaleSummaryCollection localeSummaries, bool overwrite)
        {
            if (localeSummaries == null)
                throw new ArgumentNullException("localeSummaries");

            foreach (StackHashProductLocaleSummary localeSummary in localeSummaries)
            {
                m_SqlCommands.AddLocaleSummary(productId, localeSummary.Lcid, localeSummary.TotalHits, overwrite); 
            }
        }

        public void UpdateOperatingSystemStatistics(int productId, StackHashProductOperatingSystemSummaryCollection operatingSystemSummaries, bool overwrite)
        {
            if (operatingSystemSummaries == null)
                throw new ArgumentNullException("operatingSystemSummaries");

            foreach (StackHashProductOperatingSystemSummary operatingSystemSummary in operatingSystemSummaries)
            {
                short osId = GetOperatingSystemId(operatingSystemSummary.OperatingSystemName, operatingSystemSummary.OperatingSystemVersion);
                m_SqlCommands.AddOperatingSystemSummary(productId, osId, operatingSystemSummary.TotalHits, overwrite);
            }
        }

        public void UpdateHitDateStatistics(int productId, StackHashProductHitDateSummaryCollection hitDateSummaries, bool overwrite)
        {
            if (hitDateSummaries == null)
                throw new ArgumentNullException("hitDateSummaries");

            foreach (StackHashProductHitDateSummary hitDateSummary in hitDateSummaries)
            {
                m_SqlCommands.AddHitDateSummary(productId, hitDateSummary.HitDate, hitDateSummary.TotalHits, overwrite);
            }
        }
        
        /// <summary>
        /// Merges the specified EventInfo collection with the existing one.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the event info.</param>
        /// <param name="eventInfoCollection">Event info to merge.</param>
        public void MergeEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (eventInfoCollection == null)
                throw new ArgumentNullException("eventInfoCollection");


            // First get the current event infos.
            StackHashEventInfoCollection currentEventInfos = m_SqlCommands.GetEventInfoCollection(theEvent);

            // Check for new entries and add them.
            StackHashEventInfoCollection newEventInfos = new StackHashEventInfoCollection();

            foreach (StackHashEventInfo eventInfo in eventInfoCollection)
            {
                if (currentEventInfos.FindEventInfo(eventInfo) == null)
                {
                    // See 836.
                    // The WinQual site may well report back hits for the same day with same OS and locale ID.
                    // In this case - these don't look like duplicates as the hit counts are different. Instead they should be rolled up
                    // to a single hit and added to the database.
                    StackHashEventInfo alreadyAddedEventInfo = newEventInfos.FindEventInfo(eventInfo);

                    if (alreadyAddedEventInfo != null)
                        alreadyAddedEventInfo.TotalHits += eventInfo.TotalHits;
                    else
                        newEventInfos.Add(eventInfo);
                }
            }

            if (newEventInfos.Count > 0)
            {
                m_SqlCommands.AddEventInfos(theEvent, newEventInfos);

                // Now update the hit data statistics.
                StackHashProductSummary eventSummary = new StackHashProductSummary();
                eventSummary.AddNewEventInfos(newEventInfos);
                UpdateLocaleStatistics(product.Id, eventSummary.LocaleSummaryCollection, false);
                UpdateOperatingSystemStatistics(product.Id, eventSummary.OperatingSystemSummary, false);
                UpdateHitDateStatistics(product.Id, eventSummary.HitDateSummary, false);

                foreach (StackHashEventInfo eventInfo in newEventInfos)
                {
                    OnErrorIndexChanged(new ErrorIndexEventArgs(
                        new StackHashBugTrackerUpdate(StackHashDataChanged.Hit, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, 0, theEvent.Id)));
                }
            }
        }


        /// <summary>
        /// Gets a list of all event infos for the event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event whose event info is required.</param>
        /// <returns></returns>
        public StackHashEventInfoCollection LoadEventInfoList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            return m_SqlCommands.GetEventInfoCollection(theEvent);
        }

        /// <summary>
        /// Calculates the total hits for the event from the combined EventInfo hit counts.
        /// </summary>
        /// <param name="product">The product owning the event.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to calculate the hits for.</param>
        public int GetHitCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            return m_SqlCommands.GetHitCount(theEvent);
        }

        
        #endregion EventInfoMethods


        #region CabMethods

        /// <summary>
        /// Adds a cab to the database.
        /// </summary>
        /// <param name="product">Product owning the cab.</param>
        /// <param name="file">File owning the cab.</param>
        /// <param name="theEvent">The event owning the cab.</param>
        /// <param name="cab">The cab iteself.</param>
        /// <param name="setDiagnosticInfo">Determines if the cab diagnostic information should be set.</param>
        /// <returns>A copy of the cab.</returns>
        public StackHashCab AddCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            StackHashCab originalCab = GetCab(product, file, theEvent, cab.Id);

            if (originalCab != null)
            {
                m_SqlCommands.UpdateCab(theEvent, cab, setDiagnosticInfo);

                // Now record the changes if anything significant has changed.
                bool reportChanges = originalCab.ShouldReportToBugTrackPlugIn(cab, setDiagnosticInfo);

                if (reportChanges)
                {
                    StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate(StackHashDataChanged.Cab, StackHashChangeType.UpdatedEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, cab.Id, cab.Id);
                    OnErrorIndexChanged(new ErrorIndexEventArgs(update));
                    if (m_UpdateTableActive)
                    {
                        AddUpdate(update);
                    }
                }

                return m_SqlCommands.GetCab(cab.Id);
            }
            else
            {
                StackHashCab newCab = m_SqlCommands.AddCab(theEvent, cab);

                StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate(StackHashDataChanged.Cab, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id, theEvent.EventTypeName, cab.Id, cab.Id);
                OnErrorIndexChanged(new ErrorIndexEventArgs(update));
                if (m_UpdateTableActive)
                {
                    AddUpdate(update);
                }
                return newCab;
            }
        }


        /// <summary>
        /// Gets the list of cabs for the specified event.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event whose cabs are required.</param>
        /// <returns>The event cabs.</returns>
        public StackHashCabCollection LoadCabList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            return m_SqlCommands.LoadCabCollection(theEvent);
        }


        /// <summary>
        /// Gets the specified cab.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cabId">ID of the cab required.</param>
        /// <returns>The retrieved cab.</returns>
        public StackHashCab GetCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int cabId)
        {
            return m_SqlCommands.GetCab(cabId);
        }


        /// <summary>
        /// Determines if the specified cab exists or not.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">ID of the cab to check.</param>
        /// <returns></returns>
        public bool CabExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            return m_SqlCommands.CabExists(cab.Id);
        }


        /// <summary>
        /// Gets a list of all cabs in the database related to a particular event.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <returns>Number of cabs</returns>
        public int GetCabCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            return m_SqlCommands.GetCabCount(theEvent);
        }


        /// <summary>
        /// Gets a list of all cabs in the database related to a particular event where the cab has been downloaded.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabFileCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            return m_SqlCommands.GetCabFileCount(theEvent);
        }


        
        /// <summary>
        /// Gets the folder where the cab should be stored.
        /// </summary>
        /// <param name="product">Product name</param>
        /// <param name="file">File name</param>
        /// <param name="theEvent">Event</param>
        /// <param name="cab">Cab</param>
        /// <returns>Returns the name of the cab folder.</returns>
        public string GetCabFolder(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            // The cab ID is 10 decimal digits. Split it into the form XX XX XX XX CAB_<CabId>
            String cabIdString = String.Format(CultureInfo.InvariantCulture, "{0:D10}", cab.Id);

            String folder1 = cabIdString.Substring(0, 2);
            String folder2 = cabIdString.Substring(2, 2);
            String folder3 = cabIdString.Substring(4, 2);
            String folder4 = cabIdString.Substring(6, 2);

            String cabFolder = String.Format(CultureInfo.InvariantCulture, "{0}{1}\\{2}\\{3}\\{4}\\{5}\\CAB_{6}", m_RootCabFolder, m_DatabaseName, folder1, folder2, folder3, folder4, cabIdString);

            return cabFolder;
        }


        /// <summary>
        /// Gets the full path and filename of the specified cab.
        /// Note that this function does not test if the file is actually present.
        /// </summary>
        /// <param name="product">Product name</param>
        /// <param name="file">File name</param>
        /// <param name="theEvent">Event</param>
        /// <param name="cab">Cab</param>
        /// <returns>Full path and filename of the cab.</returns>
        public string GetCabFileName(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            // GetCabFolder does all the param checks.
            string cabFileName = GetCabFolder(product, file, theEvent, cab) + "\\" + cab.FileName;

            return cabFileName;
        }


        /// <summary>
        /// Determines if the specified cab has been downloaded and is physically present.
        /// </summary>
        /// <param name="product">Product name</param>
        /// <param name="file">File name</param>
        /// <param name="theEvent">Event</param>
        /// <param name="cab">Cab</param>
        /// <returns>True - cab is present. False - otherwise.</returns>
        public bool CabFileExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            String cabFileName = GetCabFileName(product, file, theEvent, cab);

            return File.Exists(cabFileName);
        }


        /// <summary>
        /// Adds a note to the specified cab.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">cab to add note to.</param>
        /// <param name="note">Note to add</param>
        /// <returns>The note id.</returns>
        public int AddCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");

            if (!m_SqlCommands.UserExists(note.User))
                m_SqlCommands.AddUser(note.User.ToUpperInvariant());

            if (!m_SqlCommands.SourceExists(note.Source))
                m_SqlCommands.AddSource(note.Source.ToUpperInvariant());

            int cabNoteId = 0;
            
            if (note.NoteId == 0)
                cabNoteId = m_SqlCommands.AddCabNote(cab, note);
            else
                cabNoteId = m_SqlCommands.UpdateCabNote(cab, note);

            // Only report external reports.
            if (note.Source != "Service")
            {
                if (m_UpdateTableActive)
                {
                    AddUpdate(new StackHashBugTrackerUpdate(
                        StackHashDataChanged.CabNote, StackHashChangeType.NewEntry, product.Id, file.Id, theEvent.Id,
                        theEvent.EventTypeName, cab.Id, cabNoteId));
                }
            }
            return cabNoteId;
        }

        /// <summary>
        /// Deletes a note from the specified cab.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">cab to delete note from.</param>
        /// <param name="noteId">Note to delete</param>
        /// <returns>The note id.</returns>
        public void DeleteCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (noteId < 1)
                throw new ArgumentException("Note id must be greater than zero", "noteId");

            m_SqlCommands.DeleteCabNote(cab, noteId);
        }



        /// <summary>
        /// Gets all of the notes associated with a particular cab.
        /// </summary>
        /// <param name="product">Product owning the file.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">cab to get notes for.</param>
        /// <returns></returns>
        public StackHashNotes GetCabNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            return m_SqlCommands.GetCabNotes(cab);
        }


        /// <summary>
        /// Gets the specified cab note.
        /// </summary>
        /// <param name="noteId">The cab entry required.</param>
        /// <returns>The requested cab note or null.</returns>
        public StackHashNoteEntry GetCabNote(int noteId)
        {
            return m_SqlCommands.GetCabNote(noteId);
        }

        #endregion CabMethods


        #region ExtraSqlMethods

        public Collection<int> GetProductMatch(String condition)
        {
            return m_SqlCommands.GetProductMatch(condition);
        }

        /// <summary>
        /// Gets files matching the specified criteria.
        /// </summary>
        /// <returns>List of products matching the specified criteria.</returns>
        public StackHashFileProductMappingCollection GetFilesMatch(Collection<int> products, String condition)
        {
            return m_SqlCommands.GetFilesMatch(products, condition);
        }


        #endregion

        #region StatisticsMethods

        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// The data is retrieved from the database summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummary(int productId)
        {
            return m_SqlCommands.GetLocaleSummaries(productId);
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// The data is retrieved from the database summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummary(int productId)
        {
            return m_SqlCommands.GetOperatingSystemSummaries(productId);
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// The data is retrieved from the database summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummary(int productId)
        {
            return m_SqlCommands.GetHitDateSummaries(productId);
        }


        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaryFresh(int productId)
        {
            return m_SqlCommands.GetLocaleSummaryFresh(productId);
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaryFresh(int productId)
        {
            return m_SqlCommands.GetOperatingSystemSummaryFresh(productId);
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaryFresh(int productId)
        {
            return m_SqlCommands.GetHitDateSummaryFresh(productId);
        }

        /// <summary>
        /// Updates the statistics related to a particular product by parsing the database.
        /// This creates a completely accurate version of the data.
        /// This call may take up to 5 minutes if the product has a lot of events and event infos.
        /// </summary>
        /// <param name="productId">Product to parse.</param>
        public void UpdateProductStatistics(int productId)
        {
            StackHashProductLocaleSummaryCollection allLocales = GetLocaleSummaryFresh(productId);
            UpdateLocaleSummaryForProduct(productId, allLocales, true);

            StackHashProductOperatingSystemSummaryCollection allOperatingSystems = GetOperatingSystemSummaryFresh(productId);
            UpdateOperatingSystemSummaryForProduct(productId, allOperatingSystems, true);

            StackHashProductHitDateSummaryCollection allHits = GetHitDateSummaryFresh(productId);
            UpdateHitDateSummaryForProduct(productId, allHits, true);
        }

        #endregion


        #region LocaleSummaryMethods

        /// <summary>
        /// Determines if a locale summary exists.
        /// </summary>
        /// <param name="localeId">ID of the locale to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool LocaleSummaryExists(int productId, int localeId)
        {
            return m_SqlCommands.LocaleSummaryExists(productId, localeId);
        }


        /// <summary>
        /// Gets all of the locale rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaries(int productId)
        {
            return m_SqlCommands.GetLocaleSummaries(productId);
        }

        /// <summary>
        /// Gets a specific locale summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductLocaleSummary GetLocaleSummaryForProduct(int productId, int localeId)
        {
            return m_SqlCommands.GetLocaleSummaryForProduct(productId, localeId);
        }

        /// <summary>
        /// Adds a locale summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose local data is to be updated.</param>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        public void AddLocaleSummary(int productId, int localeId, long totalHits, bool overwrite)
        {
            m_SqlCommands.AddLocaleSummary(productId, localeId, totalHits, overwrite);
        }



        /// <summary>
        /// Updates all of the specified locale information. 
        /// </summary>
        /// <param name="productId">Product to which the data refers.</param>
        /// <param name="locales">All locale information for this product.</param>
        public void UpdateLocaleSummaryForProduct(int productId, StackHashProductLocaleSummaryCollection locales, bool overwrite)
        {
            if (locales == null)
                throw new ArgumentNullException("locales");

            foreach (StackHashProductLocaleSummary locale in locales)
            {
                // Update the entry in the database.
                AddLocaleSummary(productId, locale.Lcid, locale.TotalHits, overwrite);
            }
        }


        #endregion LocaleSummaryMethods

        #region LocaleMethods
        /// <summary>
        /// Adds a locale to the database.
        /// </summary>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="localeCode">Locale code.</param>
        /// <param name="localeName">Locale name.</param>
        public void AddLocale(int localeId, String localeCode, String localeName)
        {
            m_SqlCommands.AddLocale(localeId, localeCode, localeName);
        }

        #endregion LocaleMethods

        #region OperatingSystemSummaryMethods

        /// <summary>
        /// Determines if a OS summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="operatingSystemName">Name of the OS.</param>
        /// <param name="operatingSystemVersion">OS Version.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool OperatingSystemSummaryExists(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            return m_SqlCommands.OperatingSystemSummaryExists(productId, operatingSystemName, operatingSystemVersion);
        }


        /// <summary>
        /// Gets all of the OS rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaries(int productId)
        {
            return m_SqlCommands.GetOperatingSystemSummaries(productId);
        }


        /// <summary>
        /// Gets a specific OS summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductOperatingSystemSummary GetOperatingSystemSummaryForProduct(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            return m_SqlCommands.GetOperatingSystemSummaryForProduct(productId, operatingSystemName, operatingSystemVersion);
        }


        /// <summary>
        /// Adds a OS summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="operatingSystemId">ID of the OS</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        public void AddOperatingSystemSummary(int productId, short operatingSystemId, long totalHits, bool overwrite)
        {
            m_SqlCommands.AddOperatingSystemSummary(productId, operatingSystemId, totalHits, overwrite);
        }

        /// <summary>
        /// Updates all of the specified OS information. 
        /// </summary>
        /// <param name="productId">Product to which the data refers.</param>
        /// <param name="operatingSystems">All OS information for this product.</param>
        public void UpdateOperatingSystemSummaryForProduct(int productId, StackHashProductOperatingSystemSummaryCollection operatingSystems, bool overwrite)
        {
            if (operatingSystems == null)
                throw new ArgumentNullException("operatingSystems");

            foreach (StackHashProductOperatingSystemSummary operatingSystem in operatingSystems)
            {
                // Get the OS ID.
                short osId = GetOperatingSystemId(operatingSystem.OperatingSystemName, operatingSystem.OperatingSystemVersion);

                // Update the entry in the database.
                AddOperatingSystemSummary(productId, osId, operatingSystem.TotalHits, overwrite);
            }
        }

        
        #endregion OperatingSystemSummaryMethods


        #region OperatingSystemMethods

        /// <summary>
        /// Gets the OS type ID with the specified name.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        /// <returns>ID of the OS entry.</returns>
        public short GetOperatingSystemId(String operatingSystemName, String operatingSystemVersion)
        {
            return m_SqlCommands.GetOperatingSystemId(operatingSystemName, operatingSystemVersion);
        }

        /// <summary>
        /// Adds an operating system.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        public void AddOperatingSystem(String operatingSystemName, String operatingSystemVersion)
        {
            m_SqlCommands.AddOperatingSystem(operatingSystemName, operatingSystemVersion);
        }

        #endregion OperatingSystemMethods


        #region HitDateSummaryMethods

        /// <summary>
        /// Determines if a HitDate summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool HitDateSummaryExists(int productId, DateTime hitDateLocal)
        {
            return m_SqlCommands.HitDateSummaryExists(productId, hitDateLocal);
        }


        /// <summary>
        /// Gets all of the HitDate  rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaries(int productId)
        {
            return m_SqlCommands.GetHitDateSummaries(productId);
        }


        /// <summary>
        /// Gets a specific HitDate summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="hitDateLocal">Hit date to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductHitDateSummary GetHitDateSummaryForProduct(int productId, DateTime hitDateLocal)
        {
            return m_SqlCommands.GetHitDateSummaryForProduct(productId, hitDateLocal);
        }


        /// <summary>
        /// Adds a HitDate summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <param name="totalHits">Running total of all hits for this hit date.</param>
        public void AddHitDateSummary(int productId, DateTime hitDateLocal, long totalHits, bool overwrite)
        {
            m_SqlCommands.AddHitDateSummary(productId, hitDateLocal, totalHits, overwrite);
        }

        /// <summary>
        /// Updates all of the specified hit date information. 
        /// </summary>
        /// <param name="productId">Product to which the data refers.</param>
        /// <param name="operatingSystems">All HitDate information for this product.</param>
        public void UpdateHitDateSummaryForProduct(int productId, StackHashProductHitDateSummaryCollection hitDates, bool overwrite)
        {
            if (hitDates == null)
                throw new ArgumentNullException("hitDates");

            foreach (StackHashProductHitDateSummary hitDate in hitDates)
            {
                // Update the entry in the database.
                AddHitDateSummary(productId, hitDate.HitDate, hitDate.TotalHits, overwrite);
            }
        }

        
        #endregion HitDateSummaryMethods

        #region UpdateTableMethods;


        /// <summary>
        /// Gets the first entry in the Update Table belonging to this profile.
        /// </summary>
        /// <returns>The update located - or null if no update entry exists.</returns>
        public StackHashBugTrackerUpdate GetFirstUpdate()
        {
            return m_SqlCommands.GetFirstUpdate();
        }


        /// <summary>
        /// Adds a new update entry to the Update Table.
        /// Updates indicate changes that have occurred to objects in other tables.
        /// This table exists to feed the bug tracker plugins changes that have occurred
        /// to the database.
        /// Entries are normally added by the WinQualSync task and when notes are added.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void AddUpdate(StackHashBugTrackerUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException("update");

            if (m_UpdateTableActive)
            {
                m_SqlCommands.AddUpdate(update);
                OnUpdateTableChanged(new ErrorIndexEventArgs(update));
            }
        }


        /// <summary>
        /// Removes the specified entry from the update table.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void RemoveUpdate(StackHashBugTrackerUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException("update");

            m_SqlCommands.RemoveUpdate(update);
        }

        /// <summary>
        /// Clears all entries in the update table.
        /// </summary>
        public void ClearAllUpdates()
        {
            m_SqlCommands.ClearAllUpdates();
        }

        
        #endregion

        #region MappingTableMethods

        /// <summary>
        /// Gets the mappings of a particular type.
        /// </summary>
        /// <returns>Collection of mappings.</returns>
        public StackHashMappingCollection GetMappings(StackHashMappingType mappingType)
        {
            return m_SqlCommands.GetMappings(mappingType);
        }

        /// <summary>
        /// Adds the specified mappings. If they exist already they will be overwritten.
        /// </summary>
        /// <returns>Collection of mappings.</returns>
        public void AddMappings(StackHashMappingCollection mappings)
        {
            m_SqlCommands.AddMappings(mappings);
        }

        #endregion

        #region IDisposable Members


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_SqlCommands.Dispose();
                SqlConnection.ClearAllPools();
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
