using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for SqlSettingsUnitTests
    /// </summary>
    [TestClass]
    public class SqlSettingsUnitTests
    {
        public SqlSettingsUnitTests()
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
        public void ToConnectionStringWithSemicolon()
        {

            // Data Source=(local)\SQLEXPRESS;Integrated Security=True;Initial Catalog=MoveIndexTestDatabase1;
            //   Min Pool Size=6;Max Pool Size=100;Connection Timeout=15

            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration(@"Data Source=(local)\SQLEXPRESS;Integrated Security=True;",
                "MoveIndexTestDatabase1", 6, 100, 15, 100);

            String connectionString = sqlSettings.ToConnectionString();

            Assert.AreEqual(connectionString, @"Data Source=(local)\SQLEXPRESS;Integrated Security=True;Initial Catalog=MoveIndexTestDatabase1;Min Pool Size=6;Max Pool Size=100;Connection Timeout=15");
        }

        [TestMethod]
        public void ToConnectionStringWithoutSemicolon()
        {

            // Data Source=(local)\SQLEXPRESS;Integrated Security=True;Initial Catalog=MoveIndexTestDatabase1;
            //   Min Pool Size=6;Max Pool Size=100;Connection Timeout=15

            StackHashSqlConfiguration sqlSettings = new StackHashSqlConfiguration(@"Data Source=(local)\SQLEXPRESS;Integrated Security=True",
                "MoveIndexTestDatabase1", 6, 100, 15, 100);

            String connectionString = sqlSettings.ToConnectionString();

            Assert.AreEqual(connectionString, @"Data Source=(local)\SQLEXPRESS;Integrated Security=True;Initial Catalog=MoveIndexTestDatabase1;Min Pool Size=6;Max Pool Size=100;Connection Timeout=15");
        }
    }
}
