using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for BugReportUnitTests
    /// </summary>
    [TestClass]
    public class BugReportUnitTests
    {
        public BugReportUnitTests()
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


        /// <summary>
        /// 2 full reports not allowed to run at a time.
        /// </summary>
        [TestMethod]
        public void ConflictBothFull()
        {
            StackHashBugReportData data1 = new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };
            
            StackHashBugReportData data2 = new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData1.IsConflicting(allData2));
            Assert.AreEqual(true, allData2.IsConflicting(allData1));
        }


        /// <summary>
        /// Full running requested product.
        /// </summary>
        [TestMethod]
        public void ConflictFullAndProduct()
        {
            StackHashBugReportData data1 = new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashBugReportData data2 = new StackHashBugReportData(product2, null, null, null, null, StackHashReportOptions.IncludeAllObjects);

            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData1.IsConflicting(allData2));
        }


        /// <summary>
        /// Product running - requested full.
        /// </summary>
        [TestMethod]
        public void ConflictProductAndFull()
        {
            StackHashBugReportData data1 = new StackHashBugReportData(null, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashBugReportData data2 = new StackHashBugReportData(product2, null, null, null, null, StackHashReportOptions.IncludeAllObjects);

            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData2.IsConflicting(allData1));
        }


        /// <summary>
        /// Product running - requested product - same id.
        /// </summary>
        [TestMethod]
        public void ConflictProductAndSameProduct()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashBugReportData data1 = new StackHashBugReportData(product1, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashBugReportData data2 = new StackHashBugReportData(product2, null, null, null, null, StackHashReportOptions.IncludeAllObjects);

            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData2.IsConflicting(allData1));
        }

        /// <summary>
        /// Product running - requested product - different id - allowed.
        /// </summary>
        [TestMethod]
        public void NoConflictProductAndDifferentProduct()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashBugReportData data1 = new StackHashBugReportData(product1, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 2, "StackHash", 0, 0, "1.2.3.4");
            StackHashBugReportData data2 = new StackHashBugReportData(product2, null, null, null, null, StackHashReportOptions.IncludeAllObjects);

            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(false, allData1.IsConflicting(allData2));
            Assert.AreEqual(false, allData2.IsConflicting(allData1));
        }

        /// <summary>
        /// Product running - requested event - same product.
        /// </summary>
        [TestMethod]
        public void ConflictProductAndEventForSameProduct()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event2 = new StackHashEvent(1, "EventTypeName1");

            StackHashBugReportData data2 = new StackHashBugReportData(product2, file2, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData1.IsConflicting(allData2));
        }

        /// <summary>
        /// Event running - requested product - same product.
        /// </summary>
        [TestMethod]
        public void ConflictEventAndSameProduct()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, null, null, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event2 = new StackHashEvent(1, "EventTypeName1");

            StackHashBugReportData data2 = new StackHashBugReportData(product2, file2, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData2.IsConflicting(allData1));
        }

        /// <summary>
        /// Event running - run different Event
        /// </summary>
        [TestMethod]
        public void NoConflictEventAndDifferentEvent()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event1 = new StackHashEvent(2, "EventTypeName1");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, file1, event1, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event2 = new StackHashEvent(1, "EventTypeName1");

            StackHashBugReportData data2 = new StackHashBugReportData(product2, file2, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(false, allData2.IsConflicting(allData1));
        }

        /// <summary>
        /// Event running - run same Event
        /// </summary>
        [TestMethod]
        public void NoConflictEventAndSameEvent()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event1 = new StackHashEvent(1, "EventTypeName1");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, file1, event1, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event2 = new StackHashEvent(1, "EventTypeName1");

            StackHashBugReportData data2 = new StackHashBugReportData(product2, file2, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data2
            };

            Assert.AreEqual(true, allData2.IsConflicting(allData1));
        }

        /// <summary>
        /// Two events running - run event no conflict.
        /// </summary>
        [TestMethod]
        public void NoConflict2EventsAndDifferentEvent()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event1 = new StackHashEvent(1, "EventTypeName1");
            StackHashEvent event2 = new StackHashEvent(2, "EventTypeName1");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, file1, event1, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportData data2 = new StackHashBugReportData(product1, file1, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1,
                data2,
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event3 = new StackHashEvent(3, "EventTypeName1");

            StackHashBugReportData data3 = new StackHashBugReportData(product2, file2, event3, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data3
            };

            Assert.AreEqual(false, allData1.IsConflicting(allData2));
        }

        /// <summary>
        /// Two events running - run event - same as first - conflict.
        /// New event report is for different product/file - this should still be a conflict
        /// because the same event may appear in different products and files.
        /// </summary>
        [TestMethod]
        public void Conflict2EventsAndEventSameAsFirstDifferentProductAndFile()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event1 = new StackHashEvent(1, "EventTypeName1");
            StackHashEvent event2 = new StackHashEvent(2, "EventTypeName1");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, file1, event1, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportData data2 = new StackHashBugReportData(product1, file1, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1,
                data2,
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");

            StackHashBugReportData data3 = new StackHashBugReportData(product2, file2, event1, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data3
            };

            Assert.AreEqual(true, allData1.IsConflicting(allData2));
        }

        /// <summary>
        /// Two events running - run event - same as first - conflict.
        /// New event report is for different product/file - this should still be a conflict
        /// because the same event may appear in different products and files.
        /// </summary>
        [TestMethod]
        public void Conflict2EventsAndEventSameAsSecondDifferentProductAndFile()
        {
            StackHashProduct product1 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file1 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");
            StackHashEvent event1 = new StackHashEvent(1, "EventTypeName1");
            StackHashEvent event2 = new StackHashEvent(2, "EventTypeName1");

            StackHashBugReportData data1 = new StackHashBugReportData(product1, file1, event1, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportData data2 = new StackHashBugReportData(product1, file1, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData1 = new StackHashBugReportDataCollection()
            {
                data1,
                data2,
            };

            StackHashProduct product2 = new StackHashProduct(DateTime.Now, DateTime.Now, null, 1, "StackHash", 0, 0, "1.2.3.4");
            StackHashFile file2 = new StackHashFile(DateTime.Now, DateTime.Now, 1, DateTime.Now, "File1", "1.2.3.4");

            StackHashBugReportData data3 = new StackHashBugReportData(product2, file2, event2, null, null, StackHashReportOptions.IncludeAllObjects);
            StackHashBugReportDataCollection allData2 = new StackHashBugReportDataCollection()
            {
                data3
            };

            Assert.AreEqual(true, allData1.IsConflicting(allData2));
        }
    }
}
