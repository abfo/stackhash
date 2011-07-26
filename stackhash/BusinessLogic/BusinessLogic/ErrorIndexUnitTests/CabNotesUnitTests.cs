using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for CabNotesUnitTests
    /// </summary>
    [TestClass]
    public class CabNotesUnitTests
    {
        string m_TempPath;

        public CabNotesUnitTests()
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath = Path.GetTempPath() + "StackHashTest_ErrorIndex";

            TidyTest();

            if (!Directory.Exists(m_TempPath))
                Directory.CreateDirectory(m_TempPath);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath))
                PathUtils.DeleteDirectory(m_TempPath, true);
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

        //================================================================================================================================
        // GET CAB FOLDER TESTS.
        //================================================================================================================================
        #region AddCabNoteTests

        private void testAddCabNoteNullProduct(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddCabNote(null, file, theEvent, cab, note);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullProduct()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteNullProduct(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullProductCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteNullProduct(indexCache);
        }

        private void testAddCabNoteProductDoesntExist(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");

                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddCabNote(product, file, theEvent, cab, note);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabNoteProductDoesntExist()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteProductDoesntExist(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabNoteProductDoesntExistCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteProductDoesntExist(indexCache);
        }

        private void testAddCabNoteNullFile(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddProduct(product);

                index.AddCabNote(product, null, theEvent, cab, note);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullFile()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteNullFile(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullFileCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteNullFile(indexCache);
        }


        private void testAddCabNoteFileDoesntExist(IErrorIndex index)
        {
            try
            {
                index.Activate();

                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 6, DateTime.Now, "FileName", "FileVersion");
                StackHashEvent theEvent = new StackHashEvent();
                StackHashCab cab = new StackHashCab();
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddProduct(product);
                index.AddCabNote(product, file, theEvent, cab, note);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabNoteFileDoesntExist()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteFileDoesntExist(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabNoteFileDoesntExistCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteFileDoesntExist(indexCache);
        }

        private void testAddCabNoteNullEvent(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 6, DateTime.Now, "FileName", "FileVersion");
                StackHashCab cab = new StackHashCab();
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddProduct(product);
                index.AddFile(product, file);

                index.AddCabNote(product, file, null, cab, note);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullEvent()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteNullEvent(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullEventCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteNullEvent(indexCache);
        }

        private void testAddCabNoteEventDoesntExist(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 6, DateTime.Now, "FileName", "FileVersion");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "TypeName", 10, new StackHashEventSignature(), 10, 10);
                StackHashCab cab = new StackHashCab();
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddProduct(product);
                index.AddFile(product, file);

                index.AddCabNote(product, file, theEvent, cab, note);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabNoteEventDoesntExist()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteEventDoesntExist(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestAddCabNoteEventDoesntExistCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteEventDoesntExist(indexCache);
        }

        private void testAddCabNoteNullCab(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 6, DateTime.Now, "FileName", "FileVersion");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "TypeName", 10, new StackHashEventSignature(), 10, 2);
                StackHashNoteEntry note = new StackHashNoteEntry();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.AddCabNote(product, file, theEvent, null, note);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("cab", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullCab()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteNullCab(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullCabCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteNullCab(indexCache);
        }

        private void testAddCabNoteNullNote(IErrorIndex index)
        {
            try
            {
                index.Activate();
                StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
                StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 6, DateTime.Now, "FileName", "FileVersion");
                StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "TypeName", 10, new StackHashEventSignature(), 10, 2);
                StackHashCab cab = new StackHashCab();

                index.AddProduct(product);
                index.AddFile(product, file);
                index.AddEvent(product, file, theEvent);

                index.AddCabNote(product, file, theEvent, cab, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("note", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullNote()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNoteNullNote(index);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestAddCabNoteNullNoteCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNoteNullNote(indexCache);
        }

        private void testAddCabNNotes(IErrorIndex index, int numNotes)
        {
            index.Activate();
            StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "fileslink", 1, "Name", 10, 11, "version");
            StackHashFile file = new StackHashFile(DateTime.Now, DateTime.Now, 6, DateTime.Now, "FileName", "FileVersion");
            StackHashEvent theEvent = new StackHashEvent(DateTime.Now, DateTime.Now, "TypeName", 10, new StackHashEventSignature(), 10, 2);
            StackHashCab cab = new StackHashCab(DateTime.Now, DateTime.Now, 10, "Type", "EventFileName", 10, 100);

            index.AddProduct(product);
            index.AddFile(product, file);
            index.AddEvent(product, file, theEvent);
            index.AddCab(product, file, theEvent, cab, false);

            StackHashNotes allNotes = new StackHashNotes();

            for (int i = 0; i < numNotes; i++)
            {
                StackHashNoteEntry note = new StackHashNoteEntry(new DateTime(i), "Source" + i.ToString(), "User" + i.ToString(), "Notes...." + i.ToString());
                allNotes.Add(note);
                index.AddCabNote(product, file, theEvent, cab, note);
            }


            // Get the list back.
            StackHashNotes notes = index.GetCabNotes(product, file, theEvent, cab);

            Assert.AreEqual(numNotes, notes.Count);

            for (int i = 0; i < numNotes; i++)
            {
                Assert.AreEqual(allNotes[i].TimeOfEntry, notes[i].TimeOfEntry);
                Assert.AreEqual(allNotes[i].Source, notes[i].Source);
                Assert.AreEqual(allNotes[i].User, notes[i].User);
                Assert.AreEqual(allNotes[i].Note, notes[i].Note);
            }
        }

        [TestMethod]
        public void TestAddCab1Note()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNNotes(index, 1);
        }

        [TestMethod]
        public void TestAddCab1NoteCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNNotes(indexCache, 1);
        }

        [TestMethod]
        public void TestAddCab2Notes()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNNotes(index, 2);
        }

        [TestMethod]
        public void TestAddCab2NotesCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNNotes(indexCache, 2);
        }

        [TestMethod]
        public void TestAddCab100Notes()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            testAddCabNNotes(index, 100);
        }

        [TestMethod]
        public void TestAddCab100NotesCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            testAddCabNNotes(indexCache, 100);
        }
        #endregion
    }
}

