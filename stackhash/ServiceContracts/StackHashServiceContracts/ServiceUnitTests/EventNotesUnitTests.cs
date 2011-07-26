using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;


namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for EventNotesUnitTests
    /// </summary>
    [TestClass]
    public class EventNotesUnitTests
    {
        Utils m_Utils;

        public EventNotesUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

            m_Utils.RemoveAllContexts();
            m_Utils.RemoveAllScripts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.Dispose();
                m_Utils = null;
            }
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

        public void addRemoveEventNotes(int numberOfNotes)
        {
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 0;
            testIndexData.NumberOfCabs = 0;

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            resp.Settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            m_Utils.CreateTestIndex(0, testIndexData);

            try
            {
                // Enable all products so that they appear in searchs.
                StackHashProductInfoCollection products = m_Utils.GetProducts(0).Products;
                StackHashProduct product = products[0].Product;
                StackHashFileCollection files = m_Utils.GetFiles(0, product).Files;
                StackHashEventCollection events = m_Utils.GetEvents(0, product, files[0]).Events;


                // Add the specified number of event notes.
                for (int eventCount = 0; eventCount < numberOfNotes; eventCount++)
                {
                    StackHashNoteEntry note = new StackHashNoteEntry();
                    note.Note = "Note" + (eventCount + 1).ToString();
                    note.Source = "USER";
                    note.User = "MARKJ";
                    note.TimeOfEntry = DateTime.Now.AddDays(-1);

                    m_Utils.AddEventNote(0, product, files[0], events[0], note);

                    StackHashNotes notes = m_Utils.GetEventNotes(0, product, files[0], events[0]).Notes;

                    Assert.AreEqual(eventCount + 1, notes.Count);
                    bool found = false;
                    foreach (StackHashNoteEntry noteEntry in notes)
                    {
                        if (noteEntry.NoteId == eventCount + 1)
                        {
                            Assert.AreEqual(note.Note, noteEntry.Note);
                            Assert.AreEqual(note.Source, noteEntry.Source);
                            Assert.AreEqual(note.User, noteEntry.User);
                            Assert.AreEqual(DateTime.UtcNow.Date, noteEntry.TimeOfEntry.Date);
                            found = true;
                            break;
                        }
                    }

                    Assert.AreEqual(true, found);
                }

                // Now delete the event notes.
                int expectedEventNotes = numberOfNotes;

                for (int eventCount = 0; eventCount < numberOfNotes; eventCount++)
                {
                    m_Utils.DeleteEventNote(0, product, files[0], events[0], eventCount + 1);

                    expectedEventNotes--;

                    StackHashNotes notes = m_Utils.GetEventNotes(0, product, files[0], events[0]).Notes;

                    Assert.AreEqual(expectedEventNotes, notes.Count);

                    bool found = false;
                    foreach (StackHashNoteEntry noteEntry in notes)
                    {
                        if (noteEntry.NoteId == eventCount + 1)
                        {
                            found = true;
                            break;
                        }
                    }

                    Assert.AreEqual(false, found);
                }
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        /// <summary>
        /// Add and remove 1 event note.
        /// </summary>
        [TestMethod]
        public void AddRemove1EventNotes()
        {
            addRemoveEventNotes(1);
        }
        /// <summary>
        /// Add and remove 2 event notes.
        /// </summary>
        [TestMethod]
        public void AddRemove2EventNotes()
        {
            addRemoveEventNotes(2);
        }
        /// <summary>
        /// Add and remove 10 event notes.
        /// </summary>
        [TestMethod]
        public void AddRemove10EventNotes()
        {
            addRemoveEventNotes(10);
        }
    }
}
