using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for MultiSearchUnitTests
    /// </summary>
    [TestClass]
    public class MultiSearchUnitTests
    {
        public MultiSearchUnitTests()
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
        public void SearchFirstOf2MatchesCriteriaFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);

            StackHashSearchOption fieldSearchOption2 = new IntSearchOption(
                StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 21, 0);

            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection();
            fieldSearchOptions.Add(fieldSearchOption1);
            fieldSearchOptions.Add(fieldSearchOption2);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes+1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchSecondOf2MatchesCriteriaFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);

            StackHashSearchOption fieldSearchOption2 = new IntSearchOption(
                StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 22, 0);

            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection();
            fieldSearchOptions.Add(fieldSearchOption1);
            fieldSearchOptions.Add(fieldSearchOption2);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchNoneOf2MatchesCriteriaFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);

           StackHashSearchOption fieldSearchOption2 = new IntSearchOption(
                StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 21, 0);

            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection();
            fieldSearchOptions.Add(fieldSearchOption1);
            fieldSearchOptions.Add(fieldSearchOption2);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchBothOf2MatchesCriteriaFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);

            StackHashSearchOption fieldSearchOption2 = new IntSearchOption(
                StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 21, 0);

            StackHashSearchOptionCollection fieldSearchOptions = new StackHashSearchOptionCollection();
            fieldSearchOptions.Add(fieldSearchOption1);
            fieldSearchOptions.Add(fieldSearchOption2);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 21, sizeInBytes + 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

    }
}
