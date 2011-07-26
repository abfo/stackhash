using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashUtilities;
using StackHashBusinessObjects;
using StackHashErrorIndex;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for AdminUnitTest
    /// </summary>
    [TestClass]
    public class AdminUnitTest
    {
        string m_TempPath1;
        string m_TempPath2;
        string m_TempPath3;

        public AdminUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempPath1 = Path.GetTempPath() + "StackHashTestIndex1";
            m_TempPath2 = Path.GetTempPath() + "StackHashTestIndex2";
            m_TempPath3 = "r:\\stackhash\\results\\stackhashtestindex";

            TidyTest();

            if (!Directory.Exists(m_TempPath1))
                Directory.CreateDirectory(m_TempPath1);
            if (!Directory.Exists(m_TempPath2))
                Directory.CreateDirectory(m_TempPath2);
            if (!Directory.Exists(m_TempPath3))
                Directory.CreateDirectory(m_TempPath3);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            TidyTest();
        }

        public void TidyTest()
        {
            if (Directory.Exists(m_TempPath1))
                PathUtils.DeleteDirectory(m_TempPath1, true);
            if (Directory.Exists(m_TempPath2))
                PathUtils.DeleteDirectory(m_TempPath2, true);
            if (Directory.Exists(m_TempPath3))
                PathUtils.DeleteDirectory(m_TempPath3, true);
        }

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

        public void moveIndex(IErrorIndex errorIndex, int numProducts, String newIndexPath, String newIndexName, bool activate)
        {
            String originalIndexPath = errorIndex.ErrorIndexPath;
            String originalIndexName = errorIndex.ErrorIndexName;

            if (activate)
            {
                errorIndex.Activate();

                for (int i = 0; i < numProducts; i++)
                {
                    StackHashProduct product = new StackHashProduct(DateTime.Now, DateTime.Now, "www.link.com", i, "p" + i.ToString(),
                        i, i, "version");
                    errorIndex.AddProduct(product);
                }

                // Now move the index.
                errorIndex.Deactivate();
            }

            errorIndex.MoveIndex(newIndexPath, newIndexName, null, true);
            if (activate)
            {
                errorIndex.Activate();
            }

            // Check the results.
            if (activate)
            {
                Assert.AreEqual(false, Directory.Exists(originalIndexPath + "\\" + originalIndexName));
                Assert.AreEqual(true, Directory.Exists(newIndexPath + "\\" + newIndexName));
            }
            else
            {
                // Shouldn't have done anything - if never activated then even the source should not exist.
                Assert.AreEqual(false, Directory.Exists(originalIndexPath + "\\" + originalIndexName));
                Assert.AreEqual(false, Directory.Exists(newIndexPath + "\\" + newIndexName));
            }
        }


        [TestMethod]
        public void ChangeIndexName()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            moveIndex(index, 1, m_TempPath1, "Cucku2", true);

        }

        [TestMethod]
        public void ChangeIndexNameCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            moveIndex(indexCache, 1, m_TempPath1, "Cucku2", true);
        }

        [TestMethod]
        public void ChangePathOnly()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            moveIndex(index, 1, m_TempPath2, "Cucku", true);
        }

        [TestMethod]
        public void ChangePathOnlyCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            moveIndex(indexCache, 1, m_TempPath2, "Cucku", true);
        }

        [TestMethod]
        public void ChangePathAndName()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            moveIndex(index, 1, m_TempPath2, "Cucku2", true);
        }

        [TestMethod]
        public void ChangePathAndNameCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            moveIndex(indexCache, 1, m_TempPath2, "Cucku2", true);
        }

        [TestMethod]
        public void MoveToNewDrive()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            moveIndex(index, 1, m_TempPath3, "Cucku2", true);
        }

        [TestMethod]
        public void MoveToNewDriveCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            moveIndex(indexCache, 1, m_TempPath3, "Cucku2", true);
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void MoveToExistingFolder()
        {
            String subfolder = Path.Combine(m_TempPath2, "Cucku");
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            if (!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);
            moveIndex(index, 1, m_TempPath2, "Cucku", true);
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void MoveToExistingFolderCached()
        {
            String subfolder = Path.Combine(m_TempPath2, "Cucku");
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            if (!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);
            moveIndex(index, 1, m_TempPath2, "Cucku", true);
        }

        [TestMethod]
        public void MoveBack()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            moveIndex(index, 1, m_TempPath2, "Cucku2", true);
            moveIndex(index, 1, m_TempPath1, "Cucku", true);
        }

        [TestMethod]
        public void MoveBackCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            moveIndex(index, 1, m_TempPath2, "Cucku2", true);
            moveIndex(index, 1, m_TempPath1, "Cucku", true);
        }

        [TestMethod]
        public void IndexNeverActivated()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            moveIndex(index, 1, m_TempPath2, "Cucku2", false);
        }

        [TestMethod]
        public void IndexNeverActivatedCached()
        {
            XmlErrorIndex index = new XmlErrorIndex(m_TempPath1, "Cucku");
            ErrorIndexCache indexCache = new ErrorIndexCache(index);
            moveIndex(index, 1, m_TempPath2, "Cucku2", false);
        }
    }
}
