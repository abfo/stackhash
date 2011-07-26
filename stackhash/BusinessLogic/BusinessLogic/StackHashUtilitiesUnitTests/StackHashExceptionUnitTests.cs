//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using StackHashUtilities;

//namespace StackHashUtilitiesUnitTests
//{
//    /// <summary>
//    /// Tests the StackHashException class.
//    /// </summary>
//    [TestClass]
//    public class StackHashExceptionUnitTests
//    {
//        public StackHashExceptionUnitTests()
//        {
//            //
//            // TODO: Add constructor logic here
//            //
//        }

//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        //
//        // You can use the following additional attributes as you write your tests:
//        //
//        // Use ClassInitialize to run code before running the first test in the class
//        // [ClassInitialize()]
//        // public static void MyClassInitialize(TestContext testContext) { }
//        //
//        // Use ClassCleanup to run code after all tests in a class have run
//        // [ClassCleanup()]
//        // public static void MyClassCleanup() { }
//        //
//        // Use TestInitialize to run code before running each test 
//        // [TestInitialize()]
//        // public void MyTestInitialize() { }
//        //
//        // Use TestCleanup to run code after each test has run
//        // [TestCleanup()]
//        // public void MyTestCleanup() { }
//        //
//        #endregion

//        [TestMethod]
//        public void TestDefaultConstructor()
//        {
//            StackHashException ex = new StackHashException();

//            // Defaults should be set for fields.
//            Assert.AreEqual(true, ex.SourceModule == StackHashExceptionSource.Unknown);
//            Assert.AreEqual(null, ex.InnerException);
//        }

//        [TestMethod]
//        public void TestStringConstructor()
//        {
//            string message = "Error message";

//            StackHashException ex = new StackHashException(message);

//            // Defaults should be set for fields.
//            Assert.AreEqual(true, ex.SourceModule == StackHashExceptionSource.Unknown);
//            Assert.AreEqual(null, ex.InnerException);
//            Assert.AreEqual(message, ex.Message);
//        }

//        [TestMethod]
//        public void TestStringSourceExceptionConstructor()
//        {
//            string message = "Error message";
//            ArgumentException ex1 = new ArgumentException("fred");

//            StackHashException ex = new StackHashException(message, StackHashExceptionSource.WcfServices, ex1);

//            // Defaults should be set for fields.
//            Assert.AreEqual(true, ex.SourceModule == StackHashExceptionSource.WcfServices);
//            Assert.AreEqual(ex1, ex.InnerException);
//            Assert.AreEqual(message, ex.Message);
//        }

//        [TestMethod]
//        public void TestStringExceptionConstructor()
//        {
//            string message = "Error message";
//            ArgumentException ex1 = new ArgumentException("fred");

//            StackHashException ex = new StackHashException(message, ex1);

//            // Defaults should be set for fields.
//            Assert.AreEqual(true, ex.SourceModule == StackHashExceptionSource.Unknown);
//            Assert.AreEqual(ex1, ex.InnerException);
//            Assert.AreEqual(message, ex.Message);
//        }
//    }
//}
