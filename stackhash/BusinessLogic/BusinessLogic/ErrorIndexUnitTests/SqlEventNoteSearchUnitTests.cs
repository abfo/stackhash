using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlClient;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;


namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SqlEventNoteSearchUnitTests
    /// </summary>
    [TestClass]
    public class SqlEventNoteSearchUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlEventNoteSearchUnitTests()
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

        /// <summary>
        /// Search for event notes. 
        /// 2 events with no notes so should be no match.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_NoNotes_NoMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "Hello", "There", false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// Search for event notes. 
        /// 2 events with 1 note each - first event matches.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_FirstEventMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note1", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData)); 
        }


        /// <summary>
        /// Search for event notes. 
        /// 2 events with 1 note each - second event matches.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_SecondEventMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note2", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
        }

        /// <summary>
        /// Search for event notes. 
        /// 2 events with 1 note each - both events match.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_BothEventsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
        }

        /// <summary>
        /// Search for event notes. 
        /// 2 events with 1 note each - both events match - Descending order of event id.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_BothEventsMatch_Descending()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", false),  // Descending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
        }


        /// <summary>
        /// Search for event notes. 
        /// 1 event with 2 notes - no match.
        /// </summary>
        [TestMethod]
        public void Events1_Criteria1_2NotePerEvent_NoMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event1, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", false),  // Descending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// Search for event notes. 
        /// 1 event with 2 notes - first matches.
        /// </summary>
        [TestMethod]
        public void Events1_Criteria1_2NotePerEvent_FirstMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event1, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note1", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", false),  // Descending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
        }

        /// <summary>
        /// Search for event notes. 
        /// 1 event with 2 notes - second matches.
        /// </summary>
        [TestMethod]
        public void Events1_Criteria1_2NotePerEvent_SecondMatches()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event1, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note2", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", false),  // Descending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
        }

        /// <summary>
        /// Search for event notes. 
        /// 1 event with 2 notes - both notes match.
        /// </summary>
        [TestMethod]
        public void Events1_Criteria1_2NotePerEvent_BothMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event1, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", false),  // Descending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
        }

        /// <summary>
        /// 2 search criteria - no matches.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria2_1NotePerEvent_NoEventsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "notenotfound", null, false)
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "anothernotenotfound", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// 2 search criteria - 1st event matches 1st criteria.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria2_1NotePerEvent_Event1MatchesCR1()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note1", null, false)
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "anothernotenotfound", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }


        /// <summary>
        /// 2 search criteria - 1st event matches 2nd criteria.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria2_1NotePerEvent_Event1MatchesCR2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "notthisone", null, false)
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note1", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(2, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// 2 search criteria - 1st event matches both criteria.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria2_1NotePerEvent_Event1MatchesCR1AndCR2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "ok", null, false)
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note1", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(3, allPackages[0].CriteriaMatchMap);
        }


        /// <summary>
        /// 2 search criteria - 2nd event matches both criteria.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria2_1NotePerEvent_Event2MatchesCR1AndCR2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "another", null, false)
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note2", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(3, allPackages[0].CriteriaMatchMap);
        }


        /// <summary>
        /// 2 search criteria - both events match different criteria.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria2_1NotePerEvent_Event1MatchesCR2AndEvent2MatchesCR1()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "another", null, false)
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "ok", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
                new StackHashSearchCriteria(options2)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(2, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }


        /// <summary>
        /// 1 search criteria - 2 options - none match.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_NoMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "bum", null, false),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "titty", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// 1 search criteria - 2 options - event 2 matches CR1 Option1 only (not a match).
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_Event2MatchesCR1Option1()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "another", null, false),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "titty", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }

        /// <summary>
        /// 1 search criteria - 2 options - event 2 matches CR1 Option2 only (not a match).
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_Event2MatchesCR1Option2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", null, false),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "another", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// 1 search criteria - 2 options - event 2 matches CR1 Option 1 and 2 (match).
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_1NotePerEvent_Event2MatchesCR1Options1And2()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "another", null, false),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "another", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }


        /// <summary>
        /// 1 search criteria - Cab and EventNote search options.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_CabAndNotes_CabMatchesOnly()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 1001, 0),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// 1 search criteria - Cab and EventNote search options.
        /// Notes search matches.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_CabAndNotes_NotesMatchesOnly()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 10, 0),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note2", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(0, allPackages.Count);
        }


        /// <summary>
        /// 1 search criteria - Cab and EventNote search options.
        /// Notes and cab search options matches.
        /// Search for event notes. 
        /// 2 events with 1 note each.
        /// </summary>
        [TestMethod]
        public void Events2_Criteria1_CabAndNotes_NotesAndCabMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "Id", StackHashSearchOptionType.Equal, 1001, 0),
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "note2", null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }


        /// <summary>
        /// Search on TimeOfEntry
        /// </summary>
        [TestMethod]
        public void SearchOnTimeOfEntryMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now.RoundToPreviousMinute(), "System", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
//                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "hello", null, false),
                new DateTimeSearchOption(StackHashObjectType.EventNotes, "TimeOfEntry", StackHashSearchOptionType.LessThanOrEqual, DateTime.Now.RoundToNextMinute(), DateTime.Now),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search on Source
        /// </summary>
        [TestMethod]
        public void SearchOnSourceMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now.RoundToPreviousMinute(), "User", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Source", StackHashSearchOptionType.StringContains, "User", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }
        /// <summary>
        /// Search on Source 2 events
        /// </summary>
        [TestMethod]
        public void SearchOnSource_2EventsMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now.RoundToPreviousMinute(), "User", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "s", StackHashSearchOptionType.StringContains, "User", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search on User
        /// </summary>
        [TestMethod]
        public void SearchOnUserMatch()
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection workFlowMappings = m_Index.GetMappings(StackHashMappingType.WorkFlow);

            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, null, productId, "TestProduct1", 20, 30, "2.10.02123.1293");

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");

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
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName1", 100, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[0].Id, workFlowMappings[0].Name);

            StackHashEvent event2 =
                new StackHashEvent(creationDateTime, modifiedDateTime, "EventTypeName2", 101, eventSignature, 20, file1.Id, "bug", "PluginBugId", workFlowMappings[1].Id, workFlowMappings[1].Name);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "RobE", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now.RoundToPreviousMinute(), "User", "MarkJ", "Hello this another is note2");

            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddEventNote(product1, file1, event2, note2);


            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "User", StackHashSearchOptionType.StringContains, "Mark", null, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1),
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "Id", true),  // Ascending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(1, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }
    }
}
