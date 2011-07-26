using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SqlClient;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SqlUnitTests
    /// </summary>
    [TestClass]
    public class SqlUnitTests
    {
        static String s_ConnectionString = TestSettings.DefaultConnectionString + "Initial Catalog={0};";
        DbProviderFactory m_ProviderFactory;
        const String s_UnitTestDatabase = "StackHashUnitTests";
        String m_ErrorIndexPath;

        public SqlUnitTests()
        {
            m_ProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //

         [TestInitialize()]
         public void MyTestInitialize() 
         {
             m_ErrorIndexPath = "C:\\stackhashUnitTests\\CreationTests\\";
         }
        
         [TestCleanup()]
         public void MyTestCleanup() 
         {
             SqlConnection.ClearAllPools();
             if (Directory.Exists(m_ErrorIndexPath))
                 PathUtils.DeleteDirectory(m_ErrorIndexPath, true);
         }
        
        #endregion

        [TestMethod]
        public void OpenCloseConnectionDatabaseExists()
        {
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);

            bool stackHashDbaseExists;

            if (!commands.DatabaseExists(SqlUtils.UnitTestDatabase))
            {
                stackHashDbaseExists = commands.CreateDatabase(m_ErrorIndexPath, SqlUtils.UnitTestDatabase, false);
                Assert.AreEqual(true, stackHashDbaseExists);
            }

            stackHashDbaseExists = commands.DatabaseExists(SqlUtils.UnitTestDatabase);
            Assert.AreEqual(true, stackHashDbaseExists);

            commands.DeleteDatabase(SqlUtils.UnitTestDatabase);
        }

        [TestMethod]
        [ExpectedException(typeof(StackHashException))]
        public void OpenCloseConnectionDatabaseDoesntExist()
        {
            try
            {
                String connectionString = String.Format(s_ConnectionString, "UnknownDatabaseName");

                SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);

                DbConnection connection = commands.CreateConnection(true);

                commands.ReleaseConnection(connection);
                connection = null;
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(true, ex.ContainsExceptionType(typeof(SqlException)));
                Assert.AreEqual(StackHashServiceErrorCode.SqlConnectionError, ex.ServiceErrorCode);
                throw;
            }
        }

        [TestMethod]
        public void CreateDatabase()
        {
            DestroyDatabase();
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);


            commands.CreateDatabase(m_ErrorIndexPath, SqlUtils.UnitTestDatabase, false);

            Assert.AreEqual(true, commands.DatabaseExists(SqlUtils.UnitTestDatabase));

            commands.DeleteDatabase(SqlUtils.UnitTestDatabase);
        }


        [TestMethod]
        public void DestroyDatabase()
        {
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);

            String databaseName = s_UnitTestDatabase;

            if (!commands.DatabaseExists(databaseName))
            {
                bool stackHashDbaseExists = commands.CreateDatabase(m_ErrorIndexPath, databaseName, false);
                Assert.AreEqual(true, stackHashDbaseExists);
            }

            commands.DeleteDatabase(databaseName);

            Assert.AreEqual(false, commands.DatabaseExists(databaseName));
        }

        [TestMethod]
        public void CreateDatabaseUnicode()
        {
            DestroyDatabase();
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);


            commands.CreateDatabase(m_ErrorIndexPath, "\u125c" + SqlUtils.UnitTestDatabase + "\u125c", false);

            Assert.AreEqual(true, commands.DatabaseExists(SqlUtils.UnitTestDatabase));

            commands.DeleteDatabase(SqlUtils.UnitTestDatabase);
        }

        [TestMethod]
        public void GetLogicalNames()
        {
            DestroyDatabase();
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);

            commands.CreateDatabase(m_ErrorIndexPath, SqlUtils.UnitTestDatabase, false);

            Assert.AreEqual(true, commands.DatabaseExists(SqlUtils.UnitTestDatabase));

            Collection<String> logicalFileNames = commands.GetLogicalFileNames(SqlUtils.UnitTestDatabase);

            Assert.AreNotEqual(null, logicalFileNames);
            Assert.AreEqual(2, logicalFileNames.Count);

            String expectedDatLogicalFile = SqlUtils.UnitTestDatabase + "_dat";
            String expectedLogLogicalFile = SqlUtils.UnitTestDatabase + "_log";

            Assert.AreEqual(0, String.Compare(expectedDatLogicalFile, logicalFileNames[0], StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(0, String.Compare(expectedLogLogicalFile, logicalFileNames[1], StringComparison.OrdinalIgnoreCase));

            commands.DeleteDatabase(SqlUtils.UnitTestDatabase);
        }

        [TestMethod]
        public void GetLogicalNamesDefaultLocation()
        {
            DestroyDatabase();
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);

            commands.CreateDatabase(m_ErrorIndexPath, SqlUtils.UnitTestDatabase, true); // Create in default location.

            Assert.AreEqual(true, commands.DatabaseExists(SqlUtils.UnitTestDatabase));

            Collection<String> logicalFileNames = commands.GetLogicalFileNames(SqlUtils.UnitTestDatabase);

            Assert.AreNotEqual(null, logicalFileNames);
            Assert.AreEqual(2, logicalFileNames.Count);

            String expectedDatLogicalFile = SqlUtils.UnitTestDatabase;
            String expectedLogLogicalFile = SqlUtils.UnitTestDatabase + "_log";

            Assert.AreEqual(0, String.Compare(expectedDatLogicalFile, logicalFileNames[0], StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(0, String.Compare(expectedLogLogicalFile, logicalFileNames[1], StringComparison.OrdinalIgnoreCase));

            commands.DeleteDatabase(SqlUtils.UnitTestDatabase);
        }

        [TestMethod]
        public void ChangeLogicalNamesDefaultLocation()
        {
            DestroyDatabase();
            String connectionString = StackHashSqlConfiguration.DefaultMaster.ToConnectionString();

            SqlCommands commands = new SqlCommands(m_ProviderFactory, connectionString, connectionString, 1);

            commands.CreateDatabase(m_ErrorIndexPath, SqlUtils.UnitTestDatabase, true); // Create in default location.

            Assert.AreEqual(true, commands.DatabaseExists(SqlUtils.UnitTestDatabase));

            Collection<String> logicalFileNames = commands.GetLogicalFileNames(SqlUtils.UnitTestDatabase);

            Assert.AreNotEqual(null, logicalFileNames);
            Assert.AreEqual(2, logicalFileNames.Count);

            String expectedDatLogicalFile = SqlUtils.UnitTestDatabase;
            String expectedLogLogicalFile = SqlUtils.UnitTestDatabase + "_log";

            Assert.AreEqual(0, String.Compare(expectedDatLogicalFile, logicalFileNames[0], StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(0, String.Compare(expectedLogLogicalFile, logicalFileNames[1], StringComparison.OrdinalIgnoreCase));

            // Now change the logical filenames.
            String newDatabaseName = "ChangeTest";

            Assert.AreEqual(true, commands.ChangeDatabaseLogicalNames(SqlUtils.UnitTestDatabase, newDatabaseName, false));

            logicalFileNames = commands.GetLogicalFileNames(SqlUtils.UnitTestDatabase);

            Assert.AreNotEqual(null, logicalFileNames);
            Assert.AreEqual(2, logicalFileNames.Count);

            expectedDatLogicalFile = newDatabaseName;
            expectedLogLogicalFile = newDatabaseName + "_log";

            Assert.AreEqual(0, String.Compare(expectedDatLogicalFile, logicalFileNames[0], StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(0, String.Compare(expectedLogLogicalFile, logicalFileNames[1], StringComparison.OrdinalIgnoreCase));

            
            
            commands.DeleteDatabase(SqlUtils.UnitTestDatabase);
        }
    }
}
