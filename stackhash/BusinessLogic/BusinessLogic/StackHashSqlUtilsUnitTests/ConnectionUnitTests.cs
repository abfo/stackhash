using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using StackHashSqlControl;
using StackHashUtilities;

namespace StackHashSqlUtilsUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ConnectionUnitTests
    {
        private DbProviderFactory m_ProviderFactory;
        private SqlUtils m_SqlUtils;

        public ConnectionUnitTests()
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_ProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_SqlUtils != null)
                m_SqlUtils.Dispose();

            SqlConnection.ClearAllPools();
        }

        #endregion

        [TestMethod]
        public void ConnectDisconnectManyTimes()
        {
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";

            m_SqlUtils = new SqlUtils(m_ProviderFactory, connectionString, connectionString, 1);

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    DbConnection connection = m_SqlUtils.CreateConnection(false);
                    try
                    {
                    }
                    catch (System.Exception ex)
                    {
                        // Process or log errors here.
                        Console.WriteLine(ex);
                        throw;
                    }
                    finally
                    {
                        if (connection != null)
                            connection.Close();
                    }
                }
            }
            finally
            {
                m_SqlUtils.Dispose();
                m_SqlUtils = null;
            }
        }


        [TestMethod]
        public void ExecuteScalarManyTimes()
        {
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";

            m_SqlUtils = new SqlUtils(m_ProviderFactory, connectionString, connectionString, 1);

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    try
                    {
                        DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                        String commandText = @"IF EXISTS(SELECT * FROM sys.databases WHERE name = N'MASTER') SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

                        sqlCommand.CommandText = commandText;
                        int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                        Assert.AreEqual(1, result);
                    
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.ToString());
                    }
                }
            }
            finally
            {
                m_SqlUtils.Dispose();
                m_SqlUtils = null;
            }
        }


        [TestMethod]
        public void ExecuteNonQueryManyTimes()
        {
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";

            m_SqlUtils = new SqlUtils(m_ProviderFactory, connectionString, connectionString, 1);

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    try
                    {
                        DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                        String commandText = @"USE MASTER;";

                        sqlCommand.CommandText = commandText;
                        m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.ToString());
                    }
                }
            }
            finally
            {
                m_SqlUtils.Dispose();
                m_SqlUtils = null;
            }
        }

        [TestMethod]
        public void ExecuteQueryManyTimes()
        {
            String connectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";

            m_SqlUtils = new SqlUtils(m_ProviderFactory, connectionString, connectionString, 1);

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    DbDataReader reader = null;
                    try
                    {
                        DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                        String commandText = @"SELECT * FROM sys.syslogins;";

                        sqlCommand.CommandText = commandText;
                        reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.ToString());
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            finally
            {
                m_SqlUtils.Dispose();
                m_SqlUtils = null;
            }
        }
    }
}
