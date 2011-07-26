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
    /// Summary description for SqlCabNoteSearchUnitTests
    /// </summary>
    [TestClass]
    public class SqlCabNoteSearchUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;

        public SqlCabNoteSearchUnitTests()
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
        /// Search for cab notes. 
        /// 2 events - no cabs - no notes. 
        /// Expected result: No match.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs0_Notes0_Criteria1_NoMatch()
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
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "Hello", "There", false)
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
        /// Search for cab notes. 
        /// 2 events - 1 cab - no notes. 
        /// Expected result: No match.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes0_Criteria1_NoMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "Hello", "There", false)
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
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// Expected result: No match.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_NoMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false)
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
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// Expected result: 1st event matches.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Event1Matches()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note1", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// Expected result: 2nd event matches.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Event2Matches()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// Expected result: both events match.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_BothEventsMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }


        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// Expected result: both events match.
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_BothEventsMatch_Descending()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }


        /// <summary>
        /// Search for cab notes. 
        /// 1 events - 1 cab - 2 notes. 
        /// Expected result: none match.
        /// </summary>
        [TestMethod]
        public void Events1_Cabs1_Notes2_Criteria1_NoMatch()
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
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            m_Index.AddCab(product1, file1, event1, cab1, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false)
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
        /// Search for cab notes. 
        /// 1 events - 1 cab - 2 notes. 
        /// </summary>
        [TestMethod]
        public void Events1_Cabs1_Notes2_Criteria1_Note1Match()
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
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            m_Index.AddCab(product1, file1, event1, cab1, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "ok", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 1 events - 1 cab - 2 notes. 
        /// </summary>
        [TestMethod]
        public void Events1_Cabs1_Notes2_Criteria1_Note2Match()
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
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            m_Index.AddCab(product1, file1, event1, cab1, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "another", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 1 events - 1 cab - 2 notes. 
        /// </summary>
        [TestMethod]
        public void Events1_Cabs1_Notes2_Criteria1_BothNotesMatch()
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
            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);

            m_Index.AddCab(product1, file1, event1, cab1, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_NoMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);
            
            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp1", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp2", "There", false)
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
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_1stMatchOnly()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp2", "There", false)
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
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_2ndMatchOnly()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false)
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
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_BothMatchEvent2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_BothMatchEvent1()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note1", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_BothMatchBothEvents()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// 2 events - 1 cab - 1 note
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria1_Options2_BothMatchBothEvents_Descending()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false)
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_NoMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
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
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_Event1MatchesCR1()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note1", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
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
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_Event1MatchesCR2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note1", "There", false),
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
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_Event1MatchesCR1AndCR2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note1", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note1", "There", false),
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
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_Event2MatchesCR1()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_Event2MatchesCR2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "dfdf", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false),
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
            Assert.AreEqual(2, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_Event2MatchesCR1AndCR2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "note2", "There", false),
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
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_BothEventsMatchCR1()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(1, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_BothEventsMatchCR2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
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
            Assert.AreEqual(2, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_BothEventsMatchCR1AndCR2()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
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
            Assert.AreEqual(3, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(3, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab notes. 
        /// </summary>
        [TestMethod]
        public void Events2_Cabs1_Notes1_Criteria2_Options1_BothEventsMatchCR1AndCR2_LongSearch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
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
            Assert.AreEqual(3, allPackages[0].CriteriaMatchMap);
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
            Assert.AreEqual(3, allPackages[1].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for event and cab notes.
        /// </summary>
        [TestMethod]
        public void EventAndCabNoteSearch_NoMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Event note hello");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Cab note hello");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
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
        /// Search for event and cab notes.
        /// </summary>
        [TestMethod]
        public void EventAndCabNoteSearch_Criteria1_Options2_EventNoteMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Event note hello");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Cab note hello");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
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
        /// Search for event and cab notes.
        /// </summary>
        [TestMethod]
        public void EventAndCabNoteSearch_Criteria1_Options2_CabNoteMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Event note hello");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Cab note hello");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "blurp", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "hello", "There", false),
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
        /// Search for event and cab notes.
        /// </summary>
        [TestMethod]
        public void EventAndCabNoteSearch_Criteria1_Options2_CabAndEventNotesMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Event note hello");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "System", "MarkJ", "Cab note hello");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddEventNote(product1, file1, event1, note1);
            m_Index.AddCabNote(product1, file1, event1, cab1, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.EventNotes, "Note", StackHashSearchOptionType.StringContains, "Event note", "There", false),
                new StringSearchOption(StackHashObjectType.CabNotes, "Note", StackHashSearchOptionType.StringContains, "Cab note", "There", false),
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
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab note source
        /// </summary>
        [TestMethod]
        public void CabNoteSource_Match()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "MarkJ", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "User", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Source", StackHashSearchOptionType.StringContains, "zzzz", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "Source", StackHashSearchOptionType.StringContains, "User", "There", false),
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
            Assert.AreEqual(2, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab note user
        /// </summary>
        [TestMethod]
        public void CabNoteUser_Match()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "RobE", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now, "User", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "User", StackHashSearchOptionType.StringContains, "MarkJ", "There", false),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new StringSearchOption(StackHashObjectType.CabNotes, "User", StackHashSearchOptionType.StringContains, "Fred bloggs", "There", false),
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
            Assert.AreEqual(1, allPackages[0].CriteriaMatchMap);
        }

        /// <summary>
        /// Search for cab note user
        /// </summary>
        [TestMethod]
        public void CabNoteTimeOfEntry_Match()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 1000, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 1001, 20);

            StackHashNoteEntry note1 = new StackHashNoteEntry(DateTime.Now.AddDays(-1), "System", "RobE", "Hello this ok is note1");
            StackHashNoteEntry note2 = new StackHashNoteEntry(DateTime.Now.RoundToPreviousMinute(), "User", "MarkJ", "Hello this another is note2");


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);

            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            m_Index.AddCabNote(product1, file1, event1, cab1, note1);
            m_Index.AddCabNote(product1, file1, event2, cab2, note2);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.CabNotes, "TimeOfEntry", StackHashSearchOptionType.GreaterThan, DateTime.Now.AddDays(1), DateTime.Now),
            };
            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection() 
            {
                new DateTimeSearchOption(StackHashObjectType.CabNotes, "TimeOfEntry", StackHashSearchOptionType.LessThanOrEqual, DateTime.Now.AddMinutes(-5).RoundToNextMinute(), DateTime.Now ),
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

    }
}
