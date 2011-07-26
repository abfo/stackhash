using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for SynchronizationStatisticsUnitTests
    /// </summary>
    [TestClass]
    public class SynchronizationStatisticsUnitTests
    {
        public SynchronizationStatisticsUnitTests()
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
        public void Constructor()
        {
            StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics(1, 2, 3, 4, 5);

            Assert.AreEqual(1, stats.Products);
            Assert.AreEqual(2, stats.Files);
            Assert.AreEqual(3, stats.Events);
            Assert.AreEqual(4, stats.EventInfos);
            Assert.AreEqual(5, stats.Cabs);
        }

        [TestMethod]
        public void Add()
        {
            StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics(1, 2, 3, 4, 5);
            StackHashSynchronizeStatistics stats2 = new StackHashSynchronizeStatistics(5, 4, 3, 2, 1);

            stats.Add(stats2);

            Assert.AreEqual(6, stats.Products);
            Assert.AreEqual(6, stats.Files);
            Assert.AreEqual(6, stats.Events);
            Assert.AreEqual(6, stats.EventInfos);
            Assert.AreEqual(6, stats.Cabs);
        }

        
        [TestMethod]
        public void Subtract()
        {
            StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics(3, 5, 7, 9, 11);
            StackHashSynchronizeStatistics stats2 = new StackHashSynchronizeStatistics(1, 2, 3, 4, 5);

            stats.Subtract(stats2);

            Assert.AreEqual(2, stats.Products);
            Assert.AreEqual(3, stats.Files);
            Assert.AreEqual(4, stats.Events);
            Assert.AreEqual(5, stats.EventInfos);
            Assert.AreEqual(6, stats.Cabs);
        }

        [TestMethod]
        public void Clone()
        {
            StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics(3, 5, 7, 9, 11);

            StackHashSynchronizeStatistics stats2 = stats.Clone();
            Assert.AreEqual(3, stats2.Products);
            Assert.AreEqual(5, stats2.Files);
            Assert.AreEqual(7, stats2.Events);
            Assert.AreEqual(9, stats2.EventInfos);
            Assert.AreEqual(11, stats2.Cabs);
        }
    }
}
