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
    /// Summary description for SqlWindowSearchUnitTests
    /// </summary>
    [TestClass]
    public class SqlWindowSearchUnitTests
    {
        SqlErrorIndex m_Index;
        String m_RootCabFolder;
        StackHashEventPackageCollection m_Events;
        DateTime m_StartCreationDate;

        public SqlWindowSearchUnitTests()
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
        /// Creates a test index with the specified number of events.
        /// Hit numbers are descending.
        /// </summary>
        public void createTestIndex(int numEvents, int daysToAddForEvenEventIds, int bugIdModulo)
        {
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            m_Events = new StackHashEventPackageCollection();

            
            DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
            m_StartCreationDate = creationDateTime;

            DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
            DateTime hitDateTime = new DateTime(2010, 05, 05, 10, 10, 0, DateTimeKind.Utc);

            int productId = 20;
            int fileId = 10000;
            StackHashProduct product1 =
                new StackHashProduct(creationDateTime, modifiedDateTime, "FilesLink1", productId, "TestProduct1", 20, 30, "2.10.02123.1293");
            product1.TotalStoredEvents = 1;
            m_Index.AddProduct(product1, true);

            StackHashFile file1 =
                new StackHashFile(creationDateTime, modifiedDateTime, fileId, creationDateTime, "File1.dll", "2.3.4.5");
            m_Index.AddFile(product1, file1);

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName1"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
            eventSignature.InterpretParameters();

            int eventId = 1;

            for (int i = 0; i < numEvents; i++)
            {
                int totalHits = 1000 - i;

                int daysToAdd = i;

                if (((i + 1) % 2) == 0)
                {
                    daysToAdd += daysToAddForEvenEventIds;
                }

                // BugId strings alternate according to the specified modulo.
                int bugId = (i + 1) % bugIdModulo;


                StackHashEvent event1 =
                    new StackHashEvent(creationDateTime.AddDays(daysToAdd), modifiedDateTime.AddDays(daysToAdd), "EventTypeName", eventId + i, eventSignature, totalHits,
                        file1.Id, "bug" + bugId.ToString(), "PluginId", i % 16, null);

                StackHashEventInfoCollection eventInfos1 = new StackHashEventInfoCollection()
                {
                    new StackHashEventInfo(creationDateTime, modifiedDateTime, hitDateTime, "English", 1012, "Locale1", "Vista32", "1.2.3.4", event1.TotalHits),
                };
                m_Index.AddEvent(product1, file1, event1);
                m_Index.AddEventInfoCollection(product1, file1, event1, eventInfos1);

                m_Events.Add(new StackHashEventPackage(eventInfos1, null, event1, product1.Id)); 
            }
        }        
        
        /// <summary>
        /// N Events.
        /// Window of M - return each window in turn.
        /// Events should be received in the reverse order.
        /// The hits per event id get less.
        /// </summary>
        public void eventsN_WindowM_Ascending(int numEvents, int windowSize)
        {
            createTestIndex(numEvents, 0, numEvents);


            // TotalHit values for the events are descending and start from 1000.
            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(
                    new StackHashSearchOptionCollection() 
                    {
                        new IntSearchOption(StackHashObjectType.EventInfo, "TotalHits", StackHashSearchOptionType.GreaterThan, 100, 100),
                    })
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true),  // TotalHits Ascending.
            };


            for (int startRow = 1; startRow <= m_Events.Count; startRow++)
            {
                // Get the next window.
                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, startRow, windowSize, allSortOrders, null);

                Assert.AreNotEqual(null, allPackages);
                Assert.AreEqual(m_Events.Count, allPackages.TotalRows);

                int expectedRowsReturned = startRow + windowSize - 1 > m_Events.Count ? m_Events.Count - startRow + 1 : windowSize;

                Assert.AreEqual(expectedRowsReturned, allPackages.Count);

                for (int eventIndex = 0; eventIndex < expectedRowsReturned; eventIndex++)
                {
                    // The events have IDs starting from 1, 2, 3,...
                    int nextExpectedEventId = m_Events.Count - (startRow - 1) - eventIndex;
                    StackHashEventPackage eventRetrieved = allPackages[eventIndex];
                    Assert.AreNotEqual(null, eventRetrieved);

                    StackHashEventPackage eventOriginal = m_Events.FindEventPackage(nextExpectedEventId, "EventTypeName");
                    Assert.AreNotEqual(null, eventOriginal);

                    Assert.AreEqual(0, eventOriginal.EventData.CompareTo(eventRetrieved.EventData));
                    Assert.AreEqual(0, eventOriginal.EventInfoList.CompareTo(eventRetrieved.EventInfoList));
                }
            }
        }

        /// <summary>
        /// 3 Events.
        /// Window of 1 - return each window in turn.
        /// Events should be received in the ascending order.
        /// </summary>
        [TestMethod]
        public void Events3_Window1_Ascending()
        {
            eventsN_WindowM_Ascending(3, 1);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 2 - return each window in turn.
        /// Events should be received in the Ascending order.
        /// </summary>
        [TestMethod]
        public void Events3_Window2_Ascending()
        {
            eventsN_WindowM_Ascending(3, 2);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 3 - return each window in turn.
        /// Events should be received in the Ascending order.
        /// </summary>
        [TestMethod]
        public void Events3_Window3_Ascending()
        {
            eventsN_WindowM_Ascending(3, 3);
        }

        /// <summary>
        /// 4 Events.
        /// Window of 4 - return each window in turn.
        /// Events should be received in the Ascending order.
        /// </summary>
        [TestMethod]
        public void Events3_Window4_Ascending()
        {
            eventsN_WindowM_Ascending(3, 4);
        }

        /// <summary>
        /// 100 Events.
        /// Window of 20 - return each window in turn.
        /// Events should be received in the Ascending order.
        /// </summary>
        [TestMethod]
        public void Events100_Window20_Ascending()
        {
            eventsN_WindowM_Ascending(100, 20);
        }

        /// <summary>
        /// N Events.
        /// Window of M - return each window in turn.
        /// Events should be received in descending order of TotalHits.
        /// </summary>
        public void eventsN_WindowM_Descending(int numEvents, int windowSize)
        {
            createTestIndex(numEvents, 0, numEvents);


            // TotalHit values for the events are descending and start from 1000.
            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(
                    new StackHashSearchOptionCollection() 
                    {
                        new IntSearchOption(StackHashObjectType.EventInfo, "TotalHits", StackHashSearchOptionType.GreaterThan, 100, 100),
                    })
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", false),  // TotalHits Descending.
            };


            for (int startRow = 1; startRow <= m_Events.Count; startRow++)
            {
                // Get the next window.
                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, startRow, windowSize, allSortOrders, null);

                Assert.AreNotEqual(null, allPackages);

                Assert.AreEqual(m_Events.Count, allPackages.TotalRows);

                int expectedRowsReturned = startRow + windowSize - 1 > m_Events.Count ? m_Events.Count - startRow + 1 : windowSize;

                Assert.AreEqual(expectedRowsReturned, allPackages.Count);

                for (int eventIndex = 0; eventIndex < expectedRowsReturned; eventIndex++)
                {
                    // The events have IDs starting from 1, 2, 3,...
                    int nextExpectedEventId = startRow + eventIndex;
                    StackHashEventPackage eventRetrieved = allPackages[eventIndex];
                    Assert.AreNotEqual(null, eventRetrieved);

                    StackHashEventPackage eventOriginal = m_Events.FindEventPackage(nextExpectedEventId, "EventTypeName");
                    Assert.AreNotEqual(null, eventOriginal);

                    Assert.AreEqual(0, eventOriginal.EventData.CompareTo(eventRetrieved.EventData));
                    Assert.AreEqual(0, eventOriginal.EventInfoList.CompareTo(eventRetrieved.EventInfoList));
                }
            }
        }

        
        /// <summary>
        /// 3 Events.
        /// Window of 1 - return each window in turn.
        /// Events should be received in eventId order.
        /// </summary>
        [TestMethod]
        public void Events3_Window1_Descending()
        {
            eventsN_WindowM_Descending(3, 1);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 2 - return each window in turn.
        /// Events should be received in eventId order.
        /// </summary>
        [TestMethod]
        public void Events3_Window2_Descending()
        {
            eventsN_WindowM_Descending(3, 2);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 3 - return each window in turn.
        /// Events should be received in eventId order.
        /// </summary>
        [TestMethod]
        public void Events3_Window3_Descending()
        {
            eventsN_WindowM_Descending(3, 3);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 4 - return each window in turn.
        /// Events should be received in eventId order.
        /// </summary>
        [TestMethod]
        public void Events3_Window4_Descending()
        {
            eventsN_WindowM_Descending(3, 4);
        }

        /// <summary>
        /// 100 Events.
        /// Window of 21 - return each window in turn.
        /// Events should be received in eventId order.
        /// </summary>
        [TestMethod]
        public void Events100_Window21_Descending()
        {
            eventsN_WindowM_Descending(100, 21);
        }


        /// <summary>
        /// N Events.
        /// Window of M - return each window in turn.
        /// </summary>
        public void eventsN_WindowM_SelectByEventId_OrderByCreationDate(int numEvents, int windowSize)
        {
            createTestIndex(numEvents, -200, numEvents); // Every even event id will have a creation date earlier.


            // Even events will have a dateCreated prior to m_StartCreationDate.
            // Get all the even events.
            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(
                    new StackHashSearchOptionCollection() 
                    {
                        new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.LessThan, m_StartCreationDate, m_StartCreationDate),
                    })
            };

            // The events will be returned possibly in event id order from the search. 
            // Set the sort order based on the TotalHits - ascending - this should return the events
            // in reverse event ID order.
            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true),  // TotalHits Ascending.
            };


            // Only the even numbers are expected. e.g. 8, 6, 4, 2.
            List<int> expectedEventIds = new List<int>();
            for (int i = numEvents; i > 0; i--)
            {
                if ((i % 2) == 0)
                    expectedEventIds.Add(i);
            }

            for (int startRow = 1; startRow <= m_Events.Count; startRow++)
            {
                // Get the next window.
                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, startRow, windowSize, allSortOrders, null);

                Assert.AreNotEqual(null, allPackages);
                if (allPackages.Count > 0)
                    Assert.AreEqual(expectedEventIds.Count, allPackages.TotalRows);

                int expectedRowsReturned = startRow + windowSize - 1 > m_Events.Count / 2 ? m_Events.Count / 2 - startRow + 1 : windowSize;
                if (expectedRowsReturned < 0)
                    expectedRowsReturned = 0;

                Assert.AreEqual(expectedRowsReturned, allPackages.Count);

                for (int eventIndex = 0; eventIndex < expectedRowsReturned; eventIndex++)
                {
                    int nextExpectedEventId = expectedEventIds[startRow + eventIndex - 1];

                    StackHashEventPackage eventRetrieved = allPackages[eventIndex];
                    Assert.AreNotEqual(null, eventRetrieved);

                    StackHashEventPackage eventOriginal = m_Events.FindEventPackage(nextExpectedEventId, "EventTypeName");
                    Assert.AreNotEqual(null, eventOriginal);

                    Assert.AreEqual(0, eventOriginal.EventData.CompareTo(eventRetrieved.EventData));
                    Assert.AreEqual(0, eventOriginal.EventInfoList.CompareTo(eventRetrieved.EventInfoList));
                }
            }
        }

        /// <summary>
        /// 3 Events.
        /// Window of 1 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events3_Window1_SelectByEventId_OrderByCreationDate()
        {
            eventsN_WindowM_SelectByEventId_OrderByCreationDate(3, 1);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 2 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events3_Window2_SelectByEventId_OrderByCreationDate()
        {
            eventsN_WindowM_SelectByEventId_OrderByCreationDate(3, 2);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 3 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events3_Window3_SelectByEventId_OrderByCreationDate()
        {
            eventsN_WindowM_SelectByEventId_OrderByCreationDate(3, 3);
        }

        /// <summary>
        /// 3 Events.
        /// Window of 4 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events3_Window4_SelectByEventId_OrderByCreationDate()
        {
            eventsN_WindowM_SelectByEventId_OrderByCreationDate(3, 4);
        }

        /// <summary>
        /// 100 Events.
        /// Window of 25 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events100_Window25_SelectByEventId_OrderByCreationDate()
        {
            eventsN_WindowM_SelectByEventId_OrderByCreationDate(100, 25);
        }


        /// <summary>
        /// N Events.
        /// Window of M - return each window in turn.
        /// Multiple order parameters.
        /// BugIds will have numbers 0, 1, 2, ... , (modulo - 1), 0, 1, 2, ...
        /// </summary>
        public void eventsN_WindowM_SelectByEventId_OrderByTotaHitsAndBugId(int numEvents, int windowSize, int bugIdModulo)
        {
            createTestIndex(numEvents, -200, bugIdModulo); // Every even event id will have a creation date earlier.


            // Even events will have a dateCreated prior to m_StartCreationDate.
            // Get all the even events.
            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(
                    new StackHashSearchOptionCollection() 
                    {
                        new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.LessThan, m_StartCreationDate, m_StartCreationDate),
                    })
            };

            // The events will be returned possibly in event id order from the search. 
            // Set the sort order based on the TotalHits - ascending - this should return the events
            // in reverse event ID order.
            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "BugId", true), // bug IDs ascending.
                new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", true),  // TotalHits Ascending.
            };


            // Only the even numbers are expected. e.g. 8, 6, 4, 2.
            SortedList<Tuple<int, int>, int> expectedEventIds = new SortedList<Tuple<int, int>, int>();

            for (int i = 0; i < m_Events.Count; i++)
            {
                if (((m_Events[i].EventData.Id) % 2) == 0)
                {
                    Tuple<int, int> thisTuple = new Tuple<int, int>(m_Events[i].EventData.Id % bugIdModulo, m_Events[i].EventData.TotalHits);

                    expectedEventIds.Add(thisTuple, m_Events[i].EventData.Id);
                }
            }

            for (int startRow = 1; startRow <= m_Events.Count; startRow++)
            {
                // Get the next window.
                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, startRow, windowSize, allSortOrders, null);

                Assert.AreNotEqual(null, allPackages);

                int expectedRowsReturned = startRow + windowSize - 1 > m_Events.Count / 2 ? m_Events.Count / 2 - startRow + 1 : windowSize;
                if (expectedRowsReturned < 0)
                    expectedRowsReturned = 0;

                if (allPackages.Count > 0)
                    Assert.AreEqual(expectedEventIds.Count, allPackages.TotalRows);

                Assert.AreEqual(expectedRowsReturned, allPackages.Count);

                for (int eventIndex = 0; eventIndex < expectedRowsReturned; eventIndex++)
                {
                    int nextExpectedEventId = expectedEventIds.ElementAt(startRow + eventIndex - 1).Value;

                    StackHashEventPackage eventRetrieved = allPackages[eventIndex];
                    Assert.AreNotEqual(null, eventRetrieved);

                    StackHashEventPackage eventOriginal = m_Events.FindEventPackage(nextExpectedEventId, "EventTypeName");
                    Assert.AreNotEqual(null, eventOriginal);

                    Assert.AreEqual(0, eventOriginal.EventData.CompareTo(eventRetrieved.EventData));
                    Assert.AreEqual(0, eventOriginal.EventInfoList.CompareTo(eventRetrieved.EventInfoList));
                }
            }
        }

        /// <summary>
        /// 3 Events - bug ID module 3.
        /// Window of 1 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events3_Window1_SelectByEventId_OrderByTotaHitsAndBugId()
        {
            eventsN_WindowM_SelectByEventId_OrderByTotaHitsAndBugId(3, 1, 3);
        }

        /// <summary>
        /// 10 Events - bug ID module 3.
        /// Window of 1 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events10_Window1_SelectByEventId_OrderByTotaHitsAndBugId()
        {
            eventsN_WindowM_SelectByEventId_OrderByTotaHitsAndBugId(10, 1, 3);
        }

        /// <summary>
        /// 100 Events - bug ID module 7.
        /// Window of 10 - return each window in turn.
        /// Only even numbered events will be returned.
        /// </summary>
        [TestMethod]
        public void Events100_Window10_Mod7_SelectByEventId_OrderByTotaHitsAndBugId()
        {
            eventsN_WindowM_SelectByEventId_OrderByTotaHitsAndBugId(100, 10, 7);
        }


        /// <summary>
        /// N Events.
        /// Order by ALL fields.
        /// </summary>
        public void orderByAllSortFieldsAscending(int numEvents, int windowSize, int bugIdModulo, bool ascending)
        {
            createTestIndex(numEvents, -2 * numEvents, bugIdModulo); // Every even event id will have a creation date earlier.


            // Even events will have a dateCreated prior to m_StartCreationDate.
            // Get all the even events.
            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(
                    new StackHashSearchOptionCollection() 
                    {
                        new DateTimeSearchOption(StackHashObjectType.Event, "DateCreatedLocal", StackHashSearchOptionType.LessThan, m_StartCreationDate, m_StartCreationDate),
                    })
            };

            // The events will be returned possibly in event id order from the search. 
            // Set the sort order based on the TotalHits - ascending - this should return the events
            // in reverse event ID order.
            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "DateCreatedLocal", ascending),  
                new StackHashSortOrder(StackHashObjectType.Event, "DateModifiedLocal", ascending),  
                new StackHashSortOrder(StackHashObjectType.Event, "EventTypeName", ascending),  
                new StackHashSortOrder(StackHashObjectType.Event, "Id", ascending),  
                new StackHashSortOrder(StackHashObjectType.Event, "TotalHits", ascending),  
                new StackHashSortOrder(StackHashObjectType.Event, "BugId", ascending),
                new StackHashSortOrder(StackHashObjectType.Event, "PlugInBugId", ascending),
                new StackHashSortOrder(StackHashObjectType.Event, "WorkFlowStatusId", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ApplicationName", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ApplicationVersion", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ApplicationTimeStamp", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ModuleName", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ModuleVersion", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ModuleTimeStamp", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "Offset", ascending),
                new StackHashSortOrder(StackHashObjectType.EventSignature, "ExceptionCode", ascending),
            };


            // Only the even numbers are expected. e.g. 2, 4, 6,...

            List<int> expectedEventIds = new List<int>();

            for (int i = 0; i < m_Events.Count; i++)
            {
                if ((m_Events[i].EventData.Id % 2) == 0)
                {
                    expectedEventIds.Add(m_Events[i].EventData.Id);
                }
            }

            if (!ascending)
                expectedEventIds.Reverse();


            for (int startRow = 1; startRow <= m_Events.Count; startRow++)
            {
                // Get the next window.
                StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, startRow, windowSize, allSortOrders, null);

                Assert.AreNotEqual(null, allPackages);
                if (allPackages.Count > 0)
                    Assert.AreEqual(expectedEventIds.Count, allPackages.TotalRows);

                int expectedRowsReturned = startRow + windowSize - 1 > m_Events.Count / 2 ? m_Events.Count / 2 - startRow + 1 : windowSize;
                if (expectedRowsReturned < 0)
                    expectedRowsReturned = 0;

                Assert.AreEqual(expectedRowsReturned, allPackages.Count);

                for (int eventIndex = 0; eventIndex < expectedRowsReturned; eventIndex++)
                {
                    int nextExpectedEventId = expectedEventIds[startRow + eventIndex - 1];

                    StackHashEventPackage eventRetrieved = allPackages[eventIndex];
                    Assert.AreNotEqual(null, eventRetrieved);

                    StackHashEventPackage eventOriginal = m_Events.FindEventPackage(nextExpectedEventId, "EventTypeName");
                    Assert.AreNotEqual(null, eventOriginal);

                    Assert.AreEqual(0, eventOriginal.EventData.CompareTo(eventRetrieved.EventData));
                    Assert.AreEqual(0, eventOriginal.EventInfoList.CompareTo(eventRetrieved.EventInfoList));
                }
            }
        }

        /// <summary>
        /// GetEvents - sorted by ALL valid event and event signature fields - ascending.
        /// </summary>
        [TestMethod]
        public void OrderByAllSortFieldsAscending()
        {
            orderByAllSortFieldsAscending(10, 2, 2, true);
        }

        /// <summary>
        /// GetEvents - sorted by ALL valid event and event signature fields - descending.
        /// </summary>
        [TestMethod]
        public void OrderByAllSortFieldsDescending()
        {
            orderByAllSortFieldsAscending(10, 2, 2, false);
        }


        /// <summary>
        /// Order by WorkFlowStatusName ASC
        /// </summary>
        [TestMethod]
        public void OrderByWorkFlowStatusNameAsc()
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
                new StringSearchOption(StackHashObjectType.Event, "WorkFlowStatusName", StackHashSearchOptionType.StringContains, workFlowMappings[0].Name, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "WorkFlowStatusName", true),  
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
        }

        /// <summary>
        /// Order by WorkFlowStatusName DESC
        /// </summary>
        [TestMethod]
        public void OrderByWorkFlowStatusNameDesc()
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
                new StringSearchOption(StackHashObjectType.Event, "WorkFlowStatusName", StackHashSearchOptionType.StringContains, workFlowMappings[0].Name, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection()
            {
                new StackHashSearchCriteria(options1)
            };

            StackHashSortOrderCollection allSortOrders = new StackHashSortOrderCollection()
            {
                new StackHashSortOrder(StackHashObjectType.Event, "WorkFlowStatusName", false),  // Descending.
            };

            StackHashEventPackageCollection allPackages = m_Index.GetEvents(allCriteria, 0, 100, allSortOrders, null);

            Assert.AreNotEqual(null, allPackages);
            Assert.AreEqual(2, allPackages.Count);
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
        }

        /// <summary>
        /// Search for cab count > 0.
        /// No matches.
        /// </summary>
        [TestMethod]
        public void SearchCabCountGreaterThan0NoMatch()
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
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
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
        /// Search for cab count > 0.
        /// 1st of 2 events match.
        /// </summary>
        [TestMethod]
        public void SearchCabCountGreaterThan0_FirstOfTwoEventsMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 100, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
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
        /// Search for cab count > 0.
        /// 2nd of 2 events match.
        /// </summary>
        [TestMethod]
        public void SearchCabCountGreaterThan0_SecondOfTwoEventsMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename", 100, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event2, cab1, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
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
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
        }

        /// <summary>
        /// Search for cab count >= 0.
        /// Both events match.
        /// </summary>
        [TestMethod]
        public void SearchCabCountGreaterThan0_BothEventsMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename", 100, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 101, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event1, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.GreaterThan, 0, 0)
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
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData)); // Should be in descending order.
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
        }

        /// <summary>
        /// Search for cab count == 2.
        /// Second event matches only.
        /// </summary>
        [TestMethod]
        public void SearchCabCountEquals2_SecondEventMatches()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename", 100, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 101, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event2, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.Equal, 2, 0)
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
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData)); 
        }

        /// <summary>
        /// Search cab count = 0. 1 match.
        /// There is a bug in the service which stops this test from working.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void SearchCabCountEquals0_FirstEventMatches()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename", 100, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 101, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event2, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.Equal, 0, 0)
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
        /// Search for cab count > 1 and count < 3.
        /// Second event matches only.
        /// </summary>
        [TestMethod]
        public void SearchCabCountRange_SecondEventMatches()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename", 100, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 101, 10);
            StackHashCab cab3 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename3", 102, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event2, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);
            m_Index.AddCab(product1, file1, event1, cab3, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.RangeExclusive, 1, 3)
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
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData));
        }

        /// <summary>
        /// Search for cab count >= 1 and count <= 2.
        /// Both events match.
        /// </summary>
        [TestMethod]
        public void SearchCabCountRange_BothMatch()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename", 100, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 101, 10);
            StackHashCab cab3 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename3", 102, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event2, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);
            m_Index.AddCab(product1, file1, event1, cab3, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.RangeInclusive, 1, 2)
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
            Assert.AreEqual(0, event2.CompareTo(allPackages[0].EventData)); // Descending order.
            Assert.AreEqual(0, event1.CompareTo(allPackages[1].EventData));
        }

        /// <summary>
        /// Search for cab count >= 1 and count <= 2.
        /// Both events match.
        /// Ascending order on Event Id.
        /// </summary>
        [TestMethod]
        public void SearchCabCountRange_BothMatchAscendingOrder()
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

            StackHashCab cab1 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename", 100, 10);
            StackHashCab cab2 = new StackHashCab(creationDateTime, modifiedDateTime, event2.Id, event2.EventTypeName, "Filename2", 101, 10);
            StackHashCab cab3 = new StackHashCab(creationDateTime, modifiedDateTime, event1.Id, event1.EventTypeName, "Filename3", 102, 10);


            m_Index.AddProduct(product1);
            m_Index.AddFile(product1, file1);
            m_Index.AddEvent(product1, file1, event1);
            m_Index.AddEvent(product1, file1, event2);
            m_Index.AddCab(product1, file1, event2, cab1, false);
            m_Index.AddCab(product1, file1, event2, cab2, false);
            m_Index.AddCab(product1, file1, event1, cab3, false);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection() 
            {
                new IntSearchOption(StackHashObjectType.Event, "CabCount", StackHashSearchOptionType.RangeInclusive, 1, 2)
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
            Assert.AreEqual(0, event1.CompareTo(allPackages[0].EventData)); // ascending
            Assert.AreEqual(0, event2.CompareTo(allPackages[1].EventData));
        }
    }
}
