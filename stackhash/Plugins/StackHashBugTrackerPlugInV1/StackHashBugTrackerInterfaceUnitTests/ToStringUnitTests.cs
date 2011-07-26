using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;

using StackHashBugTrackerInterfaceV1;

namespace StackHashBugTrackerInterfaceUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ToStringUnitTests
    {
        public ToStringUnitTests()
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
        public void ProductToString()
        {
            BugTrackerProduct product = new BugTrackerProduct("StackHash", "1.2.3.4", 12345678);

            String result = product.ToString();

            Assert.AreEqual("Product: Id=12345678, Name=StackHash, Version=1.2.3.4\r\n", result);
        }

        [TestMethod]
        public void FileToString()
        {        
            BugTrackerFile file = new BugTrackerFile("StackHash.dll", "2.3.4.5", 987654);

            String result = file.ToString();

            Assert.AreEqual("File: Id=987654, FileName=StackHash.dll, FileVersion=2.3.4.5\r\n", result);
        }

        [TestMethod]
        public void EventToString()
        {
            NameValueCollection signature = new NameValueCollection();
            signature.Add("Exception", "0x123456");
            signature.Add("Offset", "0x123457");
            
            BugTrackerEvent theEvent = new BugTrackerEvent("Bug 1234", "Plugin bug ref", 12345678, "CLR20 32-Bit", 10000, signature);

            String result = theEvent.ToString();

            Assert.AreEqual("Event: Id=12345678, EventTypeName=CLR20 32-Bit, BugReference=Bug 1234, TotalHits=10000\r\n   Exception=0x123456\r\n   Offset=0x123457\r\n", result);
        }

        [TestMethod]
        public void EventToStringWithNullSignatureFields()
        {
            NameValueCollection signature = new NameValueCollection();
            signature.Add("Exception", null);
            signature.Add("Offset", "0x123457");

            BugTrackerEvent theEvent = new BugTrackerEvent("Bug 1234", "Plugin bug ref", 12345678, "CLR20 32-Bit", 10000, signature);

            String result = theEvent.ToString();

            Assert.AreEqual("Event: Id=12345678, EventTypeName=CLR20 32-Bit, BugReference=Bug 1234, TotalHits=10000\r\n   Exception=\r\n   Offset=0x123457\r\n", result);
        }

        
        
        [TestMethod]
        public void CabToStringNullCabFileName()
        {
            NameValueCollection diagnostics = new NameValueCollection();
            diagnostics.Add("Exception", "0x123456");
            diagnostics.Add("Offset", "0x123457");

            BugTrackerCab theCab = new BugTrackerCab(11111111, 98765432, true, false, diagnostics, null);

            String result = theCab.ToString();

            Assert.AreEqual("Cab: Id=11111111, Size=98765432, Downloaded=True, Purged=False, Path=\r\n   Exception=0x123456\r\n   Offset=0x123457\r\n", result);
        }

        [TestMethod]
        public void CabToString()
        {
            NameValueCollection diagnostics = new NameValueCollection();
            diagnostics.Add("Exception", "0x123456");
            diagnostics.Add("Offset", "0x123457");

            BugTrackerCab theCab = new BugTrackerCab(11111111, 98765432, true, false, diagnostics, "c:\\test\\somecab.cab");

            String result = theCab.ToString();

            Assert.AreEqual("Cab: Id=11111111, Size=98765432, Downloaded=True, Purged=False, Path=c:\\test\\somecab.cab\r\n   Exception=0x123456\r\n   Offset=0x123457\r\n", result);
        }

        
        [TestMethod]
        public void NoteToString()
        {
            DateTime now = new DateTime(2010, 12, 6);

            BugTrackerNote note = new BugTrackerNote(now, "Client", "MarkJ", "This is my note");

            String result = note.ToString();

            Assert.AreEqual("Note: TimeOfEntry=12/06/2010 00:00:00, Source=Client, User=MarkJ\r\nThis is my note\r\n", result);
        }

        [TestMethod]
        public void ScriptToString()
        {
            DateTime now = new DateTime(2010, 12, 6);

            BugTrackerScriptResult script = new BugTrackerScriptResult("AutoScript", 1, now, now, "Loads of results data here");
            String result = script.ToString();

            Assert.AreEqual("Script: RunDate=12/06/2010 00:00:00, Name=AutoScript, Version=1, LastModifiedDate=12/06/2010 00:00:00\r\nLoads of results data here\r\n", result);
        }
    }
}
