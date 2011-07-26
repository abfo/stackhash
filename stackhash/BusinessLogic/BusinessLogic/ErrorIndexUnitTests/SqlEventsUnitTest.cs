using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;


namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SqlEventsUnitTest
    /// </summary>
    [TestClass]
    public class SqlEventsUnitTest
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlEventsUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\StackHash_TestCabs";

            if (!Directory.Exists(m_RootCabFolder))
                Directory.CreateDirectory(m_RootCabFolder);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Index != null)
            {
                SqlConnection.ClearAllPools();

                m_Index.Deactivate();
                m_Index.DeleteIndex();
                m_Index.Dispose();
                m_Index = null;
            }
            if (Directory.Exists(m_RootCabFolder))
                PathUtils.DeleteDirectory(m_RootCabFolder, true);
            SqlConnection.ClearAllPools();

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
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void AddEventNullProduct()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            try
            {
                m_Index.Activate();
                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                m_Index.AddEvent(null, file, theEvent);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("product", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void AddEventNullFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            try
            {
                m_Index.Activate();
                StackHashProduct product = new StackHashProduct();
                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                m_Index.AddEvent(product, null, theEvent);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("file", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void AddEventNullEvent()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            try
            {
                m_Index.Activate();
                StackHashProduct product = new StackHashProduct();
                StackHashFile file = new StackHashFile();
                StackHashEvent theEvent = new StackHashEvent();
                m_Index.AddEvent(product, file, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("theEvent", ex.ParamName);
                throw;
            }
        }

        
        [TestMethod]
        public void AddEvent()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", 10, null);
                
            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event1);

            Assert.AreEqual(0, retrievedEvent.CabCount);
            Assert.AreEqual(0, retrievedEvent.CompareTo(event1));
            Assert.AreEqual("Resolved - Responded", retrievedEvent.WorkFlowStatusName);
        }



        [TestMethod]
        public void AddEventWithPlugInBugReference()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "plug-in bug reference");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event1);

            Assert.AreEqual(0, retrievedEvent.CompareTo(event1));
            Assert.AreEqual(0, retrievedEvent.CabCount);
        }

        /// <summary>
        /// Check the EventExists() call when the event does exist for the specified file.
        /// </summary>
        [TestMethod]
        public void EventExistsForFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            Assert.AreEqual(true, m_Index.EventExists(product1, file1, event1));
        }


        /// <summary>
        /// Check the EventExists() event doesn't exist.
        /// </summary>
        [TestMethod]
        public void EventDoesntExistAtAll()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            event1.Id++;
            Assert.AreEqual(false, m_Index.EventExists(product1, file1, event1));
        }

        /// <summary>
        /// Check the EventExists() events but not for the specified file.
        /// </summary>
        [TestMethod]
        public void EventDoesntExistForFile()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);

            Assert.AreEqual(false, m_Index.EventExists(product1, file2, event1));
            Assert.AreEqual(true, m_Index.EventExists(product1, file1, event1));
        }


        /// <summary>
        /// Check the EventExists() - event exists for 2 files.
        /// </summary>
        [TestMethod]
        public void EventExistsForTwoFiles()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event1);

            Assert.AreEqual(true, m_Index.EventExists(product1, file2, event1));
            Assert.AreEqual(true, m_Index.EventExists(product1, file1, event1));
        }


        /// <summary>
        /// Update an event - All fields.
        /// </summary>
        [TestMethod]
        public void UpdateEventAllFields()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1, true);


            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4.5"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(2).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(3).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "12345"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0xfffeeecc"));
            eventSignature2.InterpretParameters();

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(2), modifiedDateTime.AddDays(3), "EventTypeName1", 100, eventSignature, 343, file1.Id, "bug2");

            m_Index.AddEvent(product1, file1, event2, true);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event2);
            Assert.AreEqual(0, retrievedEvent.CompareTo(event2));
            Assert.AreEqual(0, retrievedEvent.CabCount);
        }


        /// <summary>
        /// Update only WinQual fields.
        /// </summary>
        [TestMethod]
        public void UpdateWinQualFields()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1, true);


            StackHashEventSignature eventSignature2 = new StackHashEventSignature();
            eventSignature2.Parameters = new StackHashParameterCollection();
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4.5"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.AddDays(2).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName2"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5.6"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.AddDays(3).ToString()));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "12345"));
            eventSignature2.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0xfffeeecc"));
            eventSignature2.InterpretParameters();

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime.AddDays(2), modifiedDateTime.AddDays(3), "EventTypeName1", 100, eventSignature, 343, file1.Id, "bug2");

            m_Index.AddEvent(product1, file1, event2, false);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event2);
            Assert.AreEqual(0, retrievedEvent.CompareTo(event2, true));
            Assert.AreEqual(-1, retrievedEvent.CompareTo(event2, false));
            Assert.AreEqual(-1, retrievedEvent.CompareTo(event1, false));

            // Hits is a WinQual field.
            Assert.AreEqual(event2.TotalHits, retrievedEvent.TotalHits);

            Assert.AreEqual(event1.BugId, retrievedEvent.BugId);
        }


        [TestMethod]
        public void LoadEventsNoEvents()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);

            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(0, allEvents.Count);
        }


        [TestMethod]
        public void LoadEventOneEvent()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event1);
            Assert.AreEqual(0, retrievedEvent.CompareTo(event1));

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(1, allEvents.Count);
            Assert.AreEqual(0, retrievedEvent.CompareTo(allEvents[0]));
        }


        [TestMethod]
        public void LoadEventOneEventWrongFileId()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 21, creationDateTime, "File2.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event1);
            Assert.AreEqual(0, retrievedEvent.CompareTo(event1));

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file2);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(0, allEvents.Count);
        }


        [TestMethod]
        public void LoadEventOneEventLinkedTo2Files()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            int fileId1 = 20;
            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId1, creationDateTime, "File1.dll", "2.3.4.5");
            int fileId2 = 21;
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId2, creationDateTime, "File2.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddFile(product1, file2);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file2, event1);

            StackHashEvent retrievedEvent = m_Index.GetEvent(product1, file1, event1);
            Assert.AreEqual(0, retrievedEvent.CompareTo(event1));

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(1, allEvents.Count);
            Assert.AreEqual(0, retrievedEvent.CompareTo(allEvents[0]));

            allEvents = m_Index.LoadEventList(product1, file2);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(1, allEvents.Count);

            // Only the file id will be different.
            retrievedEvent.FileId = fileId2;
            Assert.AreEqual(0, retrievedEvent.CompareTo(allEvents[0]));
        
        }


        [TestMethod]
        public void LoadEventNEvents()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            int numEvents = 20;

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashEventCollection addedEvents = new StackHashEventCollection();

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100 + i, eventSignature, 20, file1.Id, "bug");

                m_Index.AddEvent(product1, file1, event1);
                addedEvents.Add(event1);
            }

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents, allEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                Assert.AreEqual(0, addedEvents[i].CompareTo(allEvents[i]));
            }
        }


        [TestMethod]
        public void GetProductEventsNoEvents()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashEventPackageCollection allEvents = m_Index.GetProductEvents(product1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(0, allEvents.Count);
        }


        /// <summary>
        /// Get notes for a particular event when no notes have been added.
        /// </summary>
        [TestMethod]
        public void GetEventNotesNoNotes()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashNotes eventNotes = m_Index.GetEventNotes(product1, file1, event1);
            Assert.AreEqual(0, eventNotes.Count);
        }


        /// <summary>
        /// Add n event notes 
        /// </summary>
        public void addGetNEventNotes(int numNotes, bool unicode)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashNotes originalNotes = new StackHashNotes();

            for (int i = 0; i < numNotes; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime timeOfEntry = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, nowTime.Second);

                String noteText;

                if (unicode)
                    noteText = "This is a message to see if _*\u125C\u2222*&82735t uhgkajuhsd73*&^(&*%__1\"££(Yit is stored correctly" + i.ToString();
                else
                    noteText = "This is a message to see if it is stored correctly" + i.ToString();

                StackHashNoteEntry note = new StackHashNoteEntry(timeOfEntry.AddDays(i), "Service" + i.ToString(), "MachineName\\BigUserName" + i.ToString(),
                    noteText);

                originalNotes.Add(note);
                int eventNoteId = m_Index.AddEventNote(product1, file1, event1, note);

                Assert.AreEqual(i + 1, eventNoteId);
            }

            StackHashNotes eventNotes = m_Index.GetEventNotes(product1, file1, event1);
            Assert.AreEqual(numNotes, eventNotes.Count);

            for (int i = 0; i < numNotes; i++)
            {
                Assert.AreEqual(0, originalNotes[i].CompareTo(eventNotes[i]));

                // Get the individual event note by index (should be 1,2,3,4...)
                StackHashNoteEntry thisNote = m_Index.GetEventNote(i + 1);

                Assert.AreEqual(0, originalNotes[i].CompareTo(thisNote));
            }
        }

        /// <summary>
        /// Add and Get 1 event note.
        /// </summary>
        [TestMethod]
        public void AddGetOneEventNote()
        {
            addGetNEventNotes(1, false);
        }

        /// <summary>
        /// Add and Get 2 event notes.
        /// </summary>
        [TestMethod]
        public void AddGet2EventNotes()
        {
            addGetNEventNotes(2, false);
        }

        /// <summary>
        /// Add and Get 100 event notes.
        /// </summary>
        [TestMethod]
        public void AddGet100EventNotes()
        {
            addGetNEventNotes(100, false);
        }

        /// <summary>
        /// Add and Get 1000 event notes.
        /// </summary>
        [TestMethod]
        public void AddGet1000EventNotes()
        {
            addGetNEventNotes(1000, false);
        }

        /// <summary>
        /// Add and Get 1000 event notes - Non-English chars.
        /// </summary>
        [TestMethod]
        public void AddGet1000EventNotesUnicode()
        {
            addGetNEventNotes(1000, true);
        }

        /// <summary>
        /// Add n event notes 
        /// </summary>
        public void replaceEventNote(int numNotes, bool unicode)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashNotes originalNotes = new StackHashNotes();

            for (int i = 0; i < numNotes; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime timeOfEntry = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, nowTime.Second);

                String noteText;

                if (unicode)
                    noteText = "This is a message to see if _*\u125C\u2222*&82735t uhgkajuhsd73*&^(&*%__1\"££(Yit is stored correctly" + i.ToString();
                else
                    noteText = "This is a message to see if it is stored correctly" + i.ToString();

                StackHashNoteEntry note = new StackHashNoteEntry(timeOfEntry.AddDays(i), "Service" + i.ToString(), "MachineName\\BigUserName" + i.ToString(),
                    noteText);

                originalNotes.Add(note);

                int eventNoteId = m_Index.AddEventNote(product1, file1, event1, note);
                note.NoteId = eventNoteId;

                // Now replace the enty with different text.
                noteText += "NEW";
                note.Note = noteText;

                int eventNoteId2 = m_Index.AddEventNote(product1, file1, event1, note);

                Assert.AreEqual(eventNoteId, eventNoteId2);

                Assert.AreEqual(i + 1, eventNoteId);
            }

            StackHashNotes eventNotes = m_Index.GetEventNotes(product1, file1, event1);
            Assert.AreEqual(numNotes, eventNotes.Count);

            for (int i = 0; i < numNotes; i++)
            {
                Assert.AreEqual(0, originalNotes[i].CompareTo(eventNotes[i]));

                // Get the individual event note by index (should be 1,2,3,4...)
                StackHashNoteEntry thisNote = m_Index.GetEventNote(i + 1);

                Assert.AreEqual(0, originalNotes[i].CompareTo(thisNote));
                Assert.AreEqual(0, originalNotes[i].CompareTo(eventNotes[i]));
            }
        }

        /// <summary>
        /// Add and Get 1 event note.
        /// </summary>
        [TestMethod]
        public void ReplaceEventNote()
        {
            replaceEventNote(1, false);
        }

        /// <summary>
        /// Add and Get 1 event note.
        /// </summary>
        [TestMethod]
        public void ReplaceEventNoteUnicode()
        {
            replaceEventNote(1, true);
        }

        /// <summary>
        /// Add and Get 100 event notes.
        /// </summary>
        [TestMethod]
        public void ReplaceEventNote100()
        {
            replaceEventNote(100, false);
        }

        /// <summary>
        /// Delete n event notes. 
        /// </summary>
        public void deleteEventNotes(int numNotes, bool unicode)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            StackHashEvent event1 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            StackHashNotes originalNotes = new StackHashNotes();

            for (int i = 0; i < numNotes; i++)
            {
                DateTime nowTime = DateTime.Now;
                DateTime timeOfEntry = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, nowTime.Hour, nowTime.Minute, nowTime.Second);

                String noteText;

                if (unicode)
                    noteText = "This is a message to see if _*\u125C\u2222*&82735t uhgkajuhsd73*&^(&*%__1\"££(Yit is stored correctly" + i.ToString();
                else
                    noteText = "This is a message to see if it is stored correctly" + i.ToString();

                StackHashNoteEntry note = new StackHashNoteEntry(timeOfEntry.AddDays(i), "Service" + i.ToString(), "MachineName\\BigUserName" + i.ToString(),
                    noteText);

                originalNotes.Add(note);

                int eventNoteId = m_Index.AddEventNote(product1, file1, event1, note);
                note.NoteId = eventNoteId;

                // Now replace the enty with different text.
                noteText += "NEW";
                note.Note = noteText;

                int eventNoteId2 = m_Index.AddEventNote(product1, file1, event1, note);

                Assert.AreEqual(eventNoteId, eventNoteId2);

                Assert.AreEqual(i + 1, eventNoteId);
            }

            int expectedNotes = originalNotes.Count;
            foreach (StackHashNoteEntry eventNote in originalNotes)
            {
                Assert.AreNotEqual(null, m_Index.GetEventNote(eventNote.NoteId));
                m_Index.DeleteEventNote(product1, file1, event1, eventNote.NoteId);
                expectedNotes--;
                Assert.AreEqual(null, m_Index.GetEventNote(eventNote.NoteId));
                StackHashNotes eventNotes = m_Index.GetEventNotes(product1, file1, event1);
                Assert.AreEqual(expectedNotes, eventNotes.Count);
            }
        }

        /// <summary>
        /// Add and Delete 1 event note.
        /// </summary>
        [TestMethod]
        public void Delete1EventNote()
        {
            deleteEventNotes(1, false);
        }

        /// <summary>
        /// Add and Delete 2 event notes.
        /// </summary>
        [TestMethod]
        public void Delete2EventNote()
        {
            deleteEventNotes(2, false);
        }

        /// <summary>
        /// Add and Delete 100 event notes.
        /// </summary>
        [TestMethod]
        public void Delete100EventNote()
        {
            deleteEventNotes(100, false);
        }

        /// <summary>
        /// Add and Delete 1 event note unicode
        /// </summary>
        [TestMethod]
        public void Delete1EventNoteUnicode()
        {
            deleteEventNotes(1, true);
        }

        
        public void totalStoredEvents(int numEvents)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashEventCollection addedEvents = new StackHashEventCollection();

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100 + i, eventSignature, 20, file1.Id, "bug");

                m_Index.AddEvent(product1, file1, event1);
                addedEvents.Add(event1);

            }

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents, allEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                Assert.AreEqual(0, addedEvents[i].CompareTo(allEvents[i]));
            }

            long totalStoredEvents = m_Index.TotalStoredEvents;

            Assert.AreEqual(numEvents, totalStoredEvents);

            StackHashProduct product2 = m_Index.UpdateProductStatistics(product1);
            Assert.AreEqual(numEvents, product2.TotalStoredEvents);

            StackHashProduct product3 = m_Index.GetProduct(product1.Id);
            Assert.AreEqual(numEvents, product3.TotalStoredEvents);


            m_Index.Deactivate();
            m_Index.Dispose();
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.Activate();

            allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents, allEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                Assert.AreEqual(0, addedEvents[i].CompareTo(allEvents[i]));
            }

            totalStoredEvents = m_Index.TotalStoredEvents;

            Assert.AreEqual(numEvents, totalStoredEvents);
            Assert.AreEqual(numEvents, product2.TotalStoredEvents);

            product3 = m_Index.GetProduct(product1.Id);
            Assert.AreEqual(numEvents, product3.TotalStoredEvents);

        }

        /// <summary>
        /// TotalStoredEvents = 0.
        /// </summary>
        [TestMethod]
        public void TotalStoredEvents0()
        {
            totalStoredEvents(0);
        }

        /// <summary>
        /// TotalStoredEvents = 1.
        /// </summary>
        [TestMethod]
        public void TotalStoredEvents1()
        {
            totalStoredEvents(1);
        }

        /// <summary>
        /// TotalStoredEvents = 2.
        /// </summary>
        [TestMethod]
        public void TotalStoredEvents2()
        {
            totalStoredEvents(2);
        }

        /// <summary>
        /// TotalStoredEvents = 0.
        /// </summary>
        [TestMethod]
        public void TotalStoredEvents100()
        {
            totalStoredEvents(100);
        }

        /// <summary>
        /// TotalStoredEvents across 2 products.
        /// </summary>
        [TestMethod]
        public void TotalStoredEventsAcross2Products()
        {
            int numEvents = 4;
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");
            StackHashProduct product2 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 2, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");
            StackHashFile file2 =
                new StackHashFile(creationDateTime, modifiedDateTime, 201, creationDateTime, "File2.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddProduct(product2);
            m_Index.AddFile(product2, file2);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100 + i, eventSignature, 20, file1.Id, "bug");

                m_Index.AddEvent(product1, file1, event1);
            }
            for (int i = 0; i < numEvents - 1; i++)
            {
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1" + i.ToString(), 100 + i + numEvents, eventSignature, 20, file1.Id, "bug");

                m_Index.AddEvent(product2, file2, event1);
            }

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents, allEvents.Count);

            allEvents = m_Index.LoadEventList(product2, file2);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents - 1, allEvents.Count);

            long totalStoredEvents = m_Index.TotalStoredEvents;

            Assert.AreEqual(numEvents * 2 - 1, totalStoredEvents);
        }

        public void duplicateEventIds(int numEvents)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashEventCollection addedEvents = new StackHashEventCollection();
            int eventId = 100;
            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName" + i.ToString(), eventId, eventSignature, 20, file1.Id, "bug");

                m_Index.AddEvent(product1, file1, event1);
                addedEvents.Add(event1);
            }

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents, allEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEvent matchingEvent = allEvents.FindEvent(addedEvents[i].Id, addedEvents[i].EventTypeName);
                Assert.AreNotEqual(null, matchingEvent);

                Assert.AreEqual(0, addedEvents[i].CompareTo(matchingEvent));
            }

            long totalStoredEvents = m_Index.TotalStoredEvents;

            Assert.AreEqual(numEvents, totalStoredEvents);


            StackHashEventPackageCollection productEvents = m_Index.GetProductEvents(product1);
            Assert.AreNotEqual(null, productEvents);
            Assert.AreEqual(numEvents, productEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventPackage matchingEvent = productEvents.FindEventPackage(addedEvents[i].Id, addedEvents[i].EventTypeName);
                Assert.AreNotEqual(null, matchingEvent);

                Assert.AreEqual(0, addedEvents[i].CompareTo(matchingEvent.EventData));
            }


            // Use the search function to get the events.
            StackHashSearchCriteriaCollection searchCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(new StackHashSearchOptionCollection()
                {
                    new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0)
                })
            };


            StackHashEventPackageCollection searchEvents = m_Index.GetEvents(searchCriteria, null);

            Assert.AreNotEqual(null, searchEvents);
            Assert.AreEqual(numEvents, searchEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventPackage matchingEvent = searchEvents.FindEventPackage(addedEvents[i].Id, addedEvents[i].EventTypeName);
                Assert.AreNotEqual(null, matchingEvent);

                Assert.AreEqual(0, addedEvents[i].CompareTo(matchingEvent.EventData));
            }

        }

        /// <summary>
        /// Same Event ID used with different EventTypeName
        /// </summary>
        [TestMethod]
        public void DuplicateEventIds0()
        {
            duplicateEventIds(0);
        }

        /// <summary>
        /// Same Event ID used with different EventTypeName
        /// </summary>
        [TestMethod]
        public void DuplicateEventIds1()
        {
            duplicateEventIds(1);
        }

        /// <summary>
        /// Same Event ID used with different EventTypeName
        /// </summary>
        [TestMethod]
        public void DuplicateEventIds2()
        {
            duplicateEventIds(2);
        }

        /// <summary>
        /// Same Event ID used with different EventTypeName
        /// </summary>
        [TestMethod]
        public void DuplicateEventIds20()
        {
            duplicateEventIds(20);
        }

        public void negativeEventIds(int numEvents)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();

            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, 1, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, 20, creationDateTime, "File1.dll", "2.3.4.5");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);

            StackHashEventCollection addedEvents = new StackHashEventCollection();
            int eventId = -1 * numEvents;
            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName" + i.ToString(), eventId, eventSignature, 20, file1.Id, "bug");

                m_Index.AddEvent(product1, file1, event1);
                addedEvents.Add(event1);
                eventId++;
            }

            StackHashEventCollection allEvents = m_Index.LoadEventList(product1, file1);
            Assert.AreNotEqual(null, allEvents);
            Assert.AreEqual(numEvents, allEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEvent matchingEvent = allEvents.FindEvent(addedEvents[i].Id, addedEvents[i].EventTypeName);
                Assert.AreNotEqual(null, matchingEvent);

                Assert.AreEqual(0, addedEvents[i].CompareTo(matchingEvent));
            }

            long totalStoredEvents = m_Index.TotalStoredEvents;

            Assert.AreEqual(numEvents, totalStoredEvents);


            StackHashEventPackageCollection productEvents = m_Index.GetProductEvents(product1);
            Assert.AreNotEqual(null, productEvents);
            Assert.AreEqual(numEvents, productEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventPackage matchingEvent = productEvents.FindEventPackage(addedEvents[i].Id, addedEvents[i].EventTypeName);
                Assert.AreNotEqual(null, matchingEvent);

                Assert.AreEqual(0, addedEvents[i].CompareTo(matchingEvent.EventData));
            }


            // Use the search function to get the events.
            StackHashSearchCriteriaCollection searchCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(new StackHashSearchOptionCollection()
                {
                    new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.All, 0, 0)
                })
            };


            StackHashEventPackageCollection searchEvents = m_Index.GetEvents(searchCriteria, null);

            Assert.AreNotEqual(null, searchEvents);
            Assert.AreEqual(numEvents, searchEvents.Count);

            for (int i = 0; i < numEvents; i++)
            {
                StackHashEventPackage matchingEvent = searchEvents.FindEventPackage(addedEvents[i].Id, addedEvents[i].EventTypeName);
                Assert.AreNotEqual(null, matchingEvent);

                Assert.AreEqual(0, addedEvents[i].CompareTo(matchingEvent.EventData));
            }

        }

        /// <summary>
        /// Negative EventId numbers.
        /// </summary>
        [TestMethod]
        public void NegativeEventIds0()
        {
            negativeEventIds(0);
        }

        /// <summary>
        /// Negative EventId numbers.
        /// </summary>
        [TestMethod]
        public void NegativeEventIds1()
        {
            negativeEventIds(1);
        }

        /// <summary>
        /// Negative EventId numbers.
        /// </summary>
        [TestMethod]
        public void NegativeEventIds2()
        {
            negativeEventIds(2);
        }

        /// <summary>
        /// Negative EventId numbers.
        /// </summary>
        [TestMethod]
        public void NegativeEventIds20()
        {
            negativeEventIds(20);
        }


        public void productEventCount(int numProducts, int numFiles, int numEvents, Collection<int> productsToSearch, long expectedEvents, bool duplicateFileId)
        {
            productEventCount(numProducts, numFiles, numEvents, productsToSearch, expectedEvents, duplicateFileId, false, false);
            
        }
        public void productEventCount(int numProducts, int numFiles, int numEvents, Collection<int> productsToSearch, long expectedEvents, bool duplicateFileId, bool duplicateEventId, bool sameEventIdDifferentEventTypeName)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);


            int productId = 1000;
            int fileId = 5000;
            int eventId = 1;

            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                StackHashProduct product =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct" + productId.ToString(), 20, 30, "2.10.02123.1293");

                productId++;

                m_Index.AddProduct(product);


                if (duplicateFileId)
                    fileId = 5000;

                for (int fileCount = 0; fileCount < numFiles; fileCount++)
                {
                    StackHashFile file =
                        new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

                    fileId++;

                    m_Index.AddFile(product, file);

                    if (duplicateFileId || duplicateEventId)
                        eventId = 1;

                    for (int i = 0; i < numEvents; i++)
                    {
                        StackHashEventSignature eventSignature = new StackHashEventSignature();
                        eventSignature.Parameters = new StackHashParameterCollection();
                        eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                        eventSignature.InterpretParameters();


                        String eventTypeName = "EventTypeName";
                        if (sameEventIdDifferentEventTypeName)
                        {
                            eventId = 1;
                            eventTypeName += i.ToString();
                        }

                        StackHashEvent event1 =
                            new StackHashEvent(creationDateTime, modifiedDateTime, eventTypeName, eventId, eventSignature, 20, file.Id, "bug");


                        eventId++;

                        m_Index.AddEvent(product, file, event1);
                    }
                }
            }

            long totalProductEvents = m_Index.GetProductEventCount(productsToSearch);

            Assert.AreEqual(expectedEvents, totalProductEvents);
        }

        /// <summary>
        /// Count events in specified products.
        /// No products in index.
        /// No products in search list.
        /// Result should be 0 events.
        /// </summary>
        [TestMethod]
        public void TotalEmptyIndex_NoSearchProducts()
        {
            long expectedEvents = 0;
            Collection<int> productIds = new Collection<int>() { };
            productEventCount(0, 0, 0, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// No products in index.
        /// Search for 1 product (not present).
        /// Result should be 0 events.
        /// </summary>
        [TestMethod]
        public void TotalEmptyIndex_1SearchProducts()
        {
            long expectedEvents = 0;
            Collection<int> productIds = new Collection<int>() { 22 };
            productEventCount(0, 0, 0, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - no files - no events
        /// Search for 1 product (not present).
        /// Result should be 0 events.
        /// </summary>
        [TestMethod]
        public void Total1Product_1SearchProductsMismatch()
        {
            long expectedEvents = 0;
            Collection<int> productIds = new Collection<int>() { 22 };
            productEventCount(1, 0, 0, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - no files - no events
        /// Search for 1 product (present).
        /// Result should be 0 events.
        /// </summary>
        [TestMethod]
        public void Total1Product_1SearchProductsMatch()
        {
            long expectedEvents = 0;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 0, 0, productIds, expectedEvents, false);
        }

        
        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 1 file - no events
        /// Search for 1 product (present).
        /// Result should be 0 events.
        /// </summary>
        [TestMethod]
        public void Total1Product1File_1SearchProductsMatch()
        {
            long expectedEvents = 0;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 1, 0, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 1 file - 1 event
        /// Search for 1 product (present).
        /// Result should be 1 events.
        /// </summary>
        [TestMethod]
        public void Total1Product1File1Event_1SearchProductsMatch()
        {
            long expectedEvents = 1;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 1, 1, productIds, expectedEvents, false);
        }


        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 1 file - 2 events
        /// Search for 1 product (present).
        /// Result should be 2 events.
        /// </summary>
        [TestMethod]
        public void Total1Product1File2Event_1SearchProductsMatch()
        {
            long expectedEvents = 2;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 1, 2, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 2 file - 1 event
        /// Search for 1 product (present).
        /// Result should be 2 events.
        /// </summary>
        [TestMethod]
        public void Total1Product2File1Event_1SearchProductsMatch()
        {
            long expectedEvents = 2;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 2, 1, productIds, expectedEvents, false);
        }


        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 2 file - 1 event
        /// Search for 1 product (not present).
        /// Result should be 0 events because product doesn't match.
        /// </summary>
        [TestMethod]
        public void Total1Product2File1Event_1SearchProductsMismatch()
        {
            long expectedEvents = 0;
            Collection<int> productIds = new Collection<int>() { 999 };
            productEventCount(1, 2, 1, productIds, expectedEvents, false);
        }

        
        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 1 file - 1 event
        /// Search for 1 product.
        /// Result should be 1 events.
        /// </summary>
        [TestMethod]
        public void Total2Product1File1Event_1SearchProductMatch()
        {
            long expectedEvents = 1;
            Collection<int> productIds = new Collection<int>() { 1001 };
            productEventCount(2, 1, 1, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 1 file - 1 event
        /// Search for 2 products.
        /// Result should be 2 events.
        /// </summary>
        [TestMethod]
        public void Total2Product1File1Event_2SearchProductMatch()
        {
            long expectedEvents = 2;
            Collection<int> productIds = new Collection<int>() { 1000, 1001 };
            productEventCount(2, 1, 1, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 1 file - 1 event
        /// Search for 2 products - one product match only.
        /// Result should be 1 events.
        /// </summary>
        [TestMethod]
        public void Total2Product1File1Event_2SearchProduct_Only1ProductMatch()
        {
            long expectedEvents = 1;
            Collection<int> productIds = new Collection<int>() { 1000, 999 };
            productEventCount(2, 1, 1, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 2 file - 1 event
        /// Search for 1 products.
        /// Result should be 2 events.
        /// </summary>
        [TestMethod]
        public void Total2Product2File1Event_1SearchProductMatch()
        {
            long expectedEvents = 2;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(2, 2, 1, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 2 file - 1 event
        /// Search for 2 products.
        /// Result should be 2 events.
        /// </summary>
        [TestMethod]
        public void Total2Product2File1Event_2SearchProductMatch()
        {
            long expectedEvents = 4;
            Collection<int> productIds = new Collection<int>() { 1000, 1001 };
            productEventCount(2, 2, 1, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 2 file - 3 event
        /// Search for 2 products.
        /// Result should be 12 events.
        /// </summary>
        [TestMethod]
        public void Total2Product2File3Event_2SearchProductMatch()
        {
            long expectedEvents = 12;
            Collection<int> productIds = new Collection<int>() { 1000, 1001, 999 };
            productEventCount(2, 2, 3, productIds, expectedEvents, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 1 file - 1 event
        /// File is shared between the products.
        /// Search for 2 products.
        /// Result should be 1 events.
        /// </summary>
        [TestMethod]
        public void Total2Product1DuplicateFile1Event_2SearchProductMatch()
        {
            long expectedEvents = 1;
            Collection<int> productIds = new Collection<int>() { 1000, 1001 };
            productEventCount(2, 1, 1, productIds, expectedEvents, true);
        }

        /// <summary>
        /// Count events in specified products.
        /// 2 product in index - 1 file - 1 event
        /// File is shared between the products.
        /// Search for 1 products.
        /// Result should be 1 events.
        /// </summary>
        [TestMethod]
        public void Total2Product1DuplicateFile1Event_1SearchProductMatch()
        {
            long expectedEvents = 1;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(2, 1, 1, productIds, expectedEvents, true);
        }

        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 2 files - 1 event - duplicated
        /// Event is shared between 2 files.
        /// </summary>
        [TestMethod]
        public void Total1Product2Files_1EventShared()
        {
            long expectedEvents = 1;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 2, 1, productIds, expectedEvents, false, true, false);
        }

        /// <summary>
        /// Count events in specified products.
        /// 1 product in index - 1 file - 2 events - same Id - differen event type name
        /// </summary>
        [TestMethod]
        public void Total1Product2Files_2Events_SameIdDifferentEventTypeName()
        {
            long expectedEvents = 2;
            Collection<int> productIds = new Collection<int>() { 1000 };
            productEventCount(1, 1, 2, productIds, expectedEvents, false, true, true);
        }
    }
}
