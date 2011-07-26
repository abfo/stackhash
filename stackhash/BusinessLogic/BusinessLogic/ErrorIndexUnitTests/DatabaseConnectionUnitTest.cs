using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for DatabaseConnectionUnitTest
    /// </summary>
    [TestClass]
    public class DatabaseConnectionUnitTest
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        
        public DatabaseConnectionUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\StackHash_TestCabs";

            if (!Directory.Exists(m_RootCabFolder))
                Directory.CreateDirectory(m_RootCabFolder);
        }


        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Index != null)
            {
                SqlConnection.ClearAllPools();

                m_Index.Deactivate();
                m_Index.DeleteIndex();
                m_Index.Dispose();
                m_Index = null;
            }

            if (Directory.Exists(m_RootCabFolder))
                PathUtils.DeleteDirectory(m_RootCabFolder, true);

            SqlConnection.ClearAllPools();

        }

        
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
        public void InvalidDatabaseName()
        {
            String cabFolder = m_RootCabFolder;
            String databaseName = "Index"; // This is invalid because Index is a reserved Sql keyword.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, databaseName, cabFolder);

            ErrorIndexConnectionTestResults result = m_Index.GetDatabaseStatus();

            Assert.AreNotEqual(null, result);
            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.InvalidDatabaseName, result.Result);
            Assert.AreEqual(null, result.LastException);
        }

        [TestMethod]
        public void InvalidWrongInstance()
        {
            String cabFolder = m_RootCabFolder;
            String databaseName = "TestIndex";
            StackHashSqlConfiguration sqlConfig = StackHashSqlConfiguration.Default;
            sqlConfig.ConnectionTimeout = 10;
            sqlConfig.ConnectionString = "Data Source=(local)\\SQLEXPRESSSS;Integrated Security=True;";

            m_Index = new SqlErrorIndex(sqlConfig, databaseName, cabFolder);

            ErrorIndexConnectionTestResults result = m_Index.GetDatabaseStatus();

            Assert.AreNotEqual(null, result);
            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.FailedToConnectToMaster, result.Result);
            Assert.AreNotEqual(null, result.LastException);
        }

        [TestMethod]
        public void ConnectedToMasterButDatabaseDoesNotExist()
        {
            String cabFolder = m_RootCabFolder;
            String databaseName = "TestIndex";
            StackHashSqlConfiguration sqlConfig = StackHashSqlConfiguration.Default;
            sqlConfig.ConnectionTimeout = 10;

            m_Index = new SqlErrorIndex(sqlConfig, databaseName, cabFolder);

            ErrorIndexConnectionTestResults result = m_Index.GetDatabaseStatus();

            Assert.AreNotEqual(null, result);
            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.ConnectedToMasterButDatabaseDoesNotExist, result.Result);
            Assert.AreEqual(null, result.LastException);
        }

        [TestMethod]
        public void DatabaseExistsOk()
        {
            String cabFolder = m_RootCabFolder;
            String databaseName = "TestIndex";
            StackHashSqlConfiguration sqlConfig = StackHashSqlConfiguration.Default;
            sqlConfig.ConnectionTimeout = 10;

            m_Index = new SqlErrorIndex(sqlConfig, databaseName, cabFolder);
            m_Index.Activate(true, false); // Allow database to be created.

            ErrorIndexConnectionTestResults result = m_Index.GetDatabaseStatus();

            Assert.AreNotEqual(null, result);
            Assert.AreEqual(StackHashErrorIndexDatabaseStatus.Success, result.Result);
            Assert.AreEqual(null, result.LastException);
        }
    }
}
