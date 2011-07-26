using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using StackHashSqlControl;
using StackHashUtilities;

namespace StackHashSqlUtilsUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class InstallerUnitTests
    {
        public InstallerUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
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
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CreateDatabaseInCabFolder()
        {
            // This is where cab files will be placed. Also - I'm trying to change the creation so that this is the 
            // path that the SQL database will be created in too. For the moment the database will be created in the
            // default location.
            String cabFolder = "C:\\stackhashunittests\\TestCabFolder";
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
            String databaseName = "StackHashInstallTestDatabase";

            InstallerInterface sqlInstaller = new InstallerInterface(connectionString, databaseName, cabFolder);


            try
            {
                try
                {
                    // Connect to SQL Server instance.
                    sqlInstaller.Connect();
                }
                catch (System.Exception ex)
                {
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }

                try
                {
                    // Check if the database exists.
                    if (sqlInstaller.DatabaseExists())
                        return;  // Job done.

                    sqlInstaller.CreateDatabase(false);

                    Assert.AreEqual(true, sqlInstaller.DatabaseExists());

                    String databaseFileName = String.Format("{0}\\{1}\\{1}.mdf", cabFolder, databaseName);
                    String databaseLogFileName = String.Format("{0}\\{1}\\{1}.ldf", cabFolder, databaseName);

                    Assert.AreEqual(true, File.Exists(databaseFileName));
                    Assert.AreEqual(true, File.Exists(databaseLogFileName));
                }
                catch (System.Exception ex)
                {
                    // Failed to create the database.
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }
            }
            finally
            {
                sqlInstaller.DeleteDatabase(databaseName);
                sqlInstaller.Disconnect();
            }
        }


        [TestMethod]
        public void CreateDatabaseInDefaultLocation()        
        {
            // This is where cab files will be placed. Also - I'm trying to change the creation so that this is the 
            // path that the SQL database will be created in too. For the moment the database will be created in the
            // default location.
            String cabFolder = "C:\\stackhashunittests\\TestCabFolder";
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
            String databaseName = "StackHashInstallTestDatabase2";

            InstallerInterface sqlInstaller = new InstallerInterface(connectionString, databaseName, cabFolder);

            try
            {
                try
                {
                    // Connect to SQL Server instance.
                    sqlInstaller.Connect();
                }
                catch (System.Exception ex)
                {
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }

                try
                {
                    // Check if the database exists.
                    if (sqlInstaller.DatabaseExists())
                        return;  // Job done.

                    sqlInstaller.CreateDatabase(true);

                    Assert.AreEqual(true, sqlInstaller.DatabaseExists());

                    String databaseFileName = String.Format("{0}\\{1}\\{1}.mdf", cabFolder, databaseName);
                    String databaseLogFileName = String.Format("{0}\\{1}\\{1}.ldf", cabFolder, databaseName);

                    Assert.AreEqual(false, File.Exists(databaseFileName));
                    Assert.AreEqual(false, File.Exists(databaseLogFileName));
                }
                catch (System.Exception ex)
                {
                    // Failed to create the database.
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }
            }
            finally
            {
                sqlInstaller.DeleteDatabase(databaseName);
                sqlInstaller.Disconnect();
            }
        }


        [TestMethod]
        public void DeleteDatabaseInCabFolder()
        {
            // This is where cab files will be placed. Also - I'm trying to change the creation so that this is the 
            // path that the SQL database will be created in too. For the moment the database will be created in the
            // default location.
            String cabFolder = "C:\\stackhashunittests\\TestCabFolder";
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
            String databaseName = "StackHashInstallTestDatabase";
            String databaseFileName = String.Format("{0}\\{1}\\{1}.mdf", cabFolder, databaseName);
            String databaseLogFileName = String.Format("{0}\\{1}\\{1}.ldf", cabFolder, databaseName);


            InstallerInterface sqlInstaller = new InstallerInterface(connectionString, databaseName, cabFolder);


            try
            {
                try
                {
                    // Connect to SQL Server instance.
                    sqlInstaller.Connect();
                }
                catch (System.Exception ex)
                {
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }

                try
                {
                    // Check if the database exists.
                    if (sqlInstaller.DatabaseExists())
                        return;  // Job done.

                    sqlInstaller.CreateDatabase(false);

                    Assert.AreEqual(true, sqlInstaller.DatabaseExists());

                    Assert.AreEqual(true, File.Exists(databaseFileName));
                    Assert.AreEqual(true, File.Exists(databaseLogFileName));
                }
                catch (System.Exception ex)
                {
                    // Failed to create the database.
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }
            }
            finally
            {
                try
                {
                    sqlInstaller.DeleteDatabase(databaseName);

                    Assert.AreEqual(false, sqlInstaller.DatabaseExists());
                    Assert.AreEqual(false, File.Exists(databaseFileName));
                    Assert.AreEqual(false, File.Exists(databaseLogFileName));
                }
                finally
                {
                    sqlInstaller.Disconnect();
                }
            }
        }

        [TestMethod]
        public void DeleteDatabaseInDefaultLocation()
        {
            // This is where cab files will be placed. Also - I'm trying to change the creation so that this is the 
            // path that the SQL database will be created in too. For the moment the database will be created in the
            // default location.
            String cabFolder = "C:\\stackhashunittests\\TestCabFolder";
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
            String databaseName = "StackHashInstallTestDatabase";
            String databaseFileName = String.Format("{0}\\{1}\\{1}.mdf", cabFolder, databaseName);
            String databaseLogFileName = String.Format("{0}\\{1}\\{1}.ldf", cabFolder, databaseName);


            InstallerInterface sqlInstaller = new InstallerInterface(connectionString, databaseName, cabFolder);


            try
            {
                try
                {
                    // Connect to SQL Server instance.
                    sqlInstaller.Connect();
                }
                catch (System.Exception ex)
                {
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }

                try
                {
                    // Check if the database exists.
                    if (sqlInstaller.DatabaseExists())
                        return;  // Job done.

                    sqlInstaller.CreateDatabase(true);

                    Assert.AreEqual(true, sqlInstaller.DatabaseExists());

                    Assert.AreEqual(false, File.Exists(databaseFileName));
                    Assert.AreEqual(false, File.Exists(databaseLogFileName));
                }
                catch (System.Exception ex)
                {
                    // Failed to create the database.
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }
            }
            finally
            {
                try
                {
                    sqlInstaller.DeleteDatabase(databaseName);

                    Assert.AreEqual(false, sqlInstaller.DatabaseExists());
                    Assert.AreEqual(false, File.Exists(databaseFileName));
                    Assert.AreEqual(false, File.Exists(databaseLogFileName));
                }
                finally
                {
                    sqlInstaller.Disconnect();
                }
            }
        }

        // UNC not allowed for SQLServer databases as performance and resilience become an issue if writes cannot be guaranteed.
        [TestMethod]
        [Ignore] 
        public void CreateDatabaseInUncPath()
        {
            // This is where cab files will be placed. Also - I'm trying to change the creation so that this is the 
            // path that the SQL database will be created in too. For the moment the database will be created in the
            // default location.
            String cabFolder = @"\\localhost\C$\stackhashunittests\TestCabFolder";
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
            String databaseName = "StackHashInstallTestDatabase";


            InstallerInterface sqlInstaller = new InstallerInterface(connectionString, databaseName, cabFolder);


            try
            {
                try
                {
                    // Connect to SQL Server instance.
                    sqlInstaller.Connect();
                }
                catch (System.Exception ex)
                {
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }

                try
                {
                    // Check if the database exists.
                    if (sqlInstaller.DatabaseExists())
                        return;  // Job done.

                    sqlInstaller.CreateDatabase(false);

                    Assert.AreEqual(true, sqlInstaller.DatabaseExists());

                    String databaseFileName = String.Format("{0}\\{1}\\{1}.mdf", cabFolder, databaseName);
                    String databaseLogFileName = String.Format("{0}\\{1}\\{1}.ldf", cabFolder, databaseName);

                    Assert.AreEqual(true, File.Exists(databaseFileName));
                    Assert.AreEqual(true, File.Exists(databaseLogFileName));
                }
                catch (System.Exception ex)
                {
                    // Failed to create the database.
                    // Process or log errors here.
                    Console.WriteLine(ex);
                    throw;
                }
            }
            finally
            {
                sqlInstaller.DeleteDatabase(databaseName);
                sqlInstaller.Disconnect();
            }
        }

        [TestMethod]
        public void ValidDatabaseName()
        {
            String databaseName = "TEST";

            Assert.AreEqual(true, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_InvalidFirstChar_Underscore()
        {
            String databaseName = "_TEST";
            Assert.AreEqual(false, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_InvalidFirstChar_Hat()
        {
            String databaseName = "^TEST";
            Assert.AreEqual(false, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_InvalidCharInsideName()
        {
            String databaseName = "TE1236+ST";
            Assert.AreEqual(false, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_Valid_UnderscoreInName()
        {
            String databaseName = "TE1236__S_T";
            Assert.AreEqual(true, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_Valid_MaxLength()
        {
            String databaseName = "T2345678901234567890123456789012345678901234567890";
            Assert.AreEqual(true, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_Invalid_MaxLengthPlusOne()
        {
            String databaseName = "T23456789012345678901234567890123456789012345678901";
            Assert.AreEqual(false, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_KeywordUsed()
        {
            String databaseName = "Index";
            Assert.AreEqual(false, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }

        [TestMethod]
        public void CheckDatabaseName_ValidButAlmostAKeywordUsed()
        {
            String databaseName = "Index2";
            Assert.AreEqual(true, InstallerInterface.IsValidSqlDatabaseName(databaseName));
        }
    }
}
