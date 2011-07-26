using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackHashBusinessObjects;


namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for SearchUnitTests
    /// </summary>
    [TestClass]
    public class SearchUnitTests
    {
        public SearchUnitTests()
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
        public void SearchForProductsOnProductIdEqualMatch()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductIdEqualNoMatch()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdNotEqualTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.NotEqual, 1, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchProductOnIdNotEqualFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.NotEqual, 1, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdGreaterThanTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 3, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdGreaterThanFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdLessThanTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThan, 10, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 8, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdLessThanFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThan, 20, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 21, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }


        [TestMethod]
        public void SearchProductOnIdGreaterOrEqualToTrue_Equal()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThanOrEqual, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdGreaterOrEqualToTrue_Greater()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThanOrEqual, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 3, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdGreaterThanOrEqualToFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThanOrEqual, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdLessOrEqualToTrue_Equal()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThanOrEqual, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdLessOrEqualToTrue_Less()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThanOrEqual, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdLessThanOrEqualToFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.LessThanOrEqual, 2, 0);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 3, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }


        [TestMethod]
        public void SearchProductOnIdRangeInclusiveLessFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeInclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdRangeInclusiveLowerBoundTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeInclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchProductOnIdRangeInclusiveMidTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeInclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 4, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdRangeInclusiveUpperBoundTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeInclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 5, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchProductOnIdRangeInclusiveAboveFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeInclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 6, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdRangeExclusiveLessFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeExclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdRangeExclusiveLowerBoundFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeExclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 2, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchProductOnIdRangeExclusiveMidTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeExclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 4, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchProductOnIdRangeExclusiveUpperBoundFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeExclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 5, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchProductOnIdRangeExclusiveAboveFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new IntSearchOption(
                StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeExclusive, 2, 5);

            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);

            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 6, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }


        [TestMethod]
        public void SearchForProductsOnProductNameEqualMatchCaseMatch()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "Product1", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameEqualMatchCaseNoMatch()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "product1", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameEqualNoCaseMatch()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.Equal, "Product1", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "pRoDuCt1", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SearchFieldNotFound()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(StackHashObjectType.Product, "xxx", StackHashSearchOptionType.Equal, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "product1", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameStartsWithCaseInsensitiveTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringStartsWith, "Product1", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "Product1 product name", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameStartsWithCaseInsensitiveFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringStartsWith, "Product1", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "Product2 product name", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchForProductsOnProductNameStartsWithCaseSensitiveTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringStartsWith, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "Product1 product name", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameStartsWithCaseSensitiveFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringStartsWith, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "ProducT1 product name", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameContainsCaseInsensitiveTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringContains, "Product1", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "This is ProducT1 product name", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameContainsCaseInsensitiveFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringContains, "Product1", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "This is Product2 product name", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameContainsCaseSensitiveTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringContains, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "This is Product1 product name", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchForProductsOnProductNameContainsCaseSensitiveFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(
                StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringContains, "Product1", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "This is ProducT1 product name", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }


        [TestMethod]
        public void SearchDateTimeEqualTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.Equal, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeEqualFalse()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.Equal, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeNotEqualTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.NotEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeNotEqualFalse()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.NotEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchDateTimeGreaterTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchDateTimeGreaterFalse()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThan, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeGreaterOrEqualTrue_Equal()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThanOrEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeGreaterOrEqualTrue_Greater()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThanOrEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeGreaterOrEqualFalse_Greater()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.GreaterThanOrEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(-1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeLessTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.LessThan, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(-1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchDateTimeLessFalse()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.LessThan, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeLessOrEqualTrue_Equal()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.LessThanOrEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeLessOrEqualTrue_Less()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.LessThanOrEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(-1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeLessOrEqualFalse_Greater()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.LessThanOrEqual, thisTime, thisTime);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchDateTimeRangeInclusiveLessFalse()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.RangeInclusive, thisTime, thisTime.AddDays(10));
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(-1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeRangeInclusiveLowerBoundTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.RangeInclusive, thisTime, thisTime.AddDays(10));
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime, thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchDateTimeRangeInclusiveMidTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.RangeInclusive, thisTime, thisTime.AddDays(10));
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(1), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchDateTimeRangeInclusiveUpperBoundTrue()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.RangeInclusive, thisTime, thisTime.AddDays(10));
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(10), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchDateTimeRangeInclusiveAboveFalse()
        {
            DateTime thisTime = DateTime.Now;

            StackHashSearchOption fieldStackHashSearchOption1 = new DateTimeSearchOption(
                StackHashObjectType.Product, "DateCreatedLocal", StackHashSearchOptionType.RangeInclusive, thisTime, thisTime.AddDays(10));
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashProduct product = new StackHashProduct(thisTime.AddDays(11), thisTime, "link", 1, "ProductName", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchLongEqualTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.Equal, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongEqualFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.Equal, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongNotEqualTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.NotEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongNotEqualFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.NotEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongGreaterTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThan, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongGreaterFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThan, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongGreaterOrEqualTrue_Greater()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThanOrEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongGreaterOrEqualTrue_Equal()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThanOrEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongGreaterOrEqualFalse_Less()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.GreaterThanOrEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongLessTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThan, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongLessFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThan, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongLessOrEqualTrue_Less()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThanOrEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongLessOrEqualTrue_Equal()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThanOrEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongLessOrEqualFalse_Greater()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.LessThanOrEqual, sizeInBytes, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongRangeExclusiveLessFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeExclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongRangeExclusiveLowerBoundFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeExclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongRangeExclusiveMidTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeExclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongRangeExclusiveUpperBoundFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeExclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 10);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongRangeExclusiveGreaterFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeExclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 11);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongRangeInclusiveLessFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes - 1);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongRangeInclusiveLowerBoundTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongRangeInclusiveMidTrue()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 1);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }

        [TestMethod]
        public void SearchLongRangeInclusiveUpperBoundFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 10);
            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchLongRangeInclusiveGreaterFalse()
        {
            long sizeInBytes = 0x12345678;

            StackHashSearchOption fieldStackHashSearchOption1 = new LongSearchOption(
                StackHashObjectType.CabInfo, "SizeInBytes", StackHashSearchOptionType.RangeInclusive, sizeInBytes, sizeInBytes + 10);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "EventType", "Filename", 22, sizeInBytes + 11);
            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.CabInfo, cab));
        }
        [TestMethod]
        public void SearchAllIntFieldsTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = 
                new IntSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.Equal, 1, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            // One field should match.
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 1, "product1", 20, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchAllIntFieldsFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 =
                new IntSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.Equal, 1, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            // One field should match.
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 3, "product1", 20, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }
        [TestMethod]
        public void SearchAllIntFieldsSecondFieldMatchesTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 =
                new IntSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.Equal, 1, 0);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            // One field should match.
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 20, "product1", 1, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchAllStringFieldsSecondFieldMatchesTrue()
        {
            StackHashSearchOption fieldStackHashSearchOption1 =
                new StringSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.StringContains, "Fred", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            // One field should match.
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 20, "prodfreduct1", 1, 10, "Version");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchAllStringFieldsSecondFieldMatchesCaseMismatchFalse()
        {
            StackHashSearchOption fieldStackHashSearchOption1 =
                new StringSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.StringContains, "Fred", "", true);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            // One field should match.
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "link", 20, "prodfreduct1", 1, 10, "Version");

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Product, product));
        }

        [TestMethod]
        public void SearchOnNullStringFieldBugId()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.Equal, "SomeValue", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 12, new StackHashEventSignature(), 12, 12, null);

            Assert.AreEqual(false, criteria1.IsMatch(StackHashObjectType.Event, theEvent));
        }

        [TestMethod]
        public void SearchOnNonNullStringFieldBugId()
        {
            StackHashSearchOption fieldStackHashSearchOption1 = new StringSearchOption(StackHashObjectType.Event, "BugId", StackHashSearchOptionType.Equal, "SomeValue", "", false);
            StackHashSearchOptionCollection fieldStackHashSearchOptions = new StackHashSearchOptionCollection();
            fieldStackHashSearchOptions.Add(fieldStackHashSearchOption1);
            StackHashSearchCriteria criteria1 = new StackHashSearchCriteria(fieldStackHashSearchOptions);

            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "EventTypeName", 12, new StackHashEventSignature(), 12, 12, "SomeValue");

            Assert.AreEqual(true, criteria1.IsMatch(StackHashObjectType.Event, theEvent));
        }

    }
}
