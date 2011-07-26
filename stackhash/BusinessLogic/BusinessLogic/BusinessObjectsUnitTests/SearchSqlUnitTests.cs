using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackHashBusinessObjects;


namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for SearchUnitTests
    /// </summary>
    [TestClass]
    public class SearchSqlUnitTests
    {
        public SearchSqlUnitTests()
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
        public void ToSqlStringProduct_ProductId()
        {
            int productId = 20;
            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("(P.ProductId={0})", productId);
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// No match should come back as an empty string.
        /// </summary>
        [TestMethod]
        public void ToSqlStringNoMatches()
        {
            int productId = 20;
            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "ProductId", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.File, "F");
            String expected = String.Empty;
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// The field Id should get converted to "ProductId".
        /// </summary>
        [TestMethod]
        public void ToSqlStringProduct_Id()
        {
            int productId = 20;
            StackHashSearchOptionCollection options = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options)                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("(P.ProductId={0})", productId);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Two criteria - both match.
        /// </summary>
        [TestMethod]
        public void ToSqlStringToCriteriaMatch()
        {
            int productId = 20;
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("((P.ProductId={0}) OR (P.ProductId={0}))", productId);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Two criteria - only one match.
        /// </summary>
        [TestMethod]
        public void ToSqlStringOneOfTwoCriteriaMatch()
        {
            int productId = 20;
            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.File, "Id", StackHashSearchOptionType.Equal, productId, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2)                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("(P.ProductId={0})", productId);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// One criteria - two options - both match.
        /// </summary>
        [TestMethod]
        public void ToSqlStringOneOfTwoOptionsBothMatch()
        {
            int productId = 20;
            String productName = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringContains, productName, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("((P.ProductId>{0}) AND (P.ProductName LIKE N'%{1}%'))", productId, productName);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// One criteria - 1 string.
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringDoesNotContain()
        {
            int productId = 20;
            String productName = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringDoesNotContain, productName, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("(P.ProductName NOT LIKE N'%MyProduct%' OR P.ProductName IS NULL)", productId, productName);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// One criteria - 1 string - 2 options.
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringDoesNotContain2Options()
        {
            int productId = 20;
            String productName = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringDoesNotContain, productName, null, false),
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, 1, 0)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("((P.ProductName NOT LIKE N'%MyProduct%' OR P.ProductName IS NULL) AND (P.ProductId=1))", productId, productName);
            Assert.AreEqual(expected, result);
        }



        /// <summary>
        /// 2 criteria, 2 options.
        /// </summary>
        [TestMethod]
        public void ToSqlStringTwoCriteriaAndTwoOptions()
        {
            int productId = 20;
            String productName = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.GreaterThan, productId, 0),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringContains, productName, null, false)
            };

            StackHashSearchOptionCollection options2 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.RangeInclusive, productId, productId+5),
                new StringSearchOption(StackHashObjectType.Product, "Name", StackHashSearchOptionType.StringStartsWith, productName, null, false)
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
                new StackHashSearchCriteria(options2),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("(((P.ProductId>{0}) AND (P.ProductName LIKE N'%{1}%')) OR ((P.ProductId BETWEEN {0} AND {2}) AND (P.ProductName LIKE N'{1}%')))", productId, productName, productId + 5);
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Product - Integer
        /// </summary>
        [TestMethod]
        public void ToSqlStringIntegerWildcardOnProduct()
        {
            int productId = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.GreaterThan, productId, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("((P.ProductId>{0}) OR (P.TotalEvents>{0}) OR (P.TotalResponses>{0}) OR (P.TotalStoredEvents>{0}))", productId);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on Product - Long
        /// </summary>
        [TestMethod]
        public void ToSqlStringLongWildcardOnProduct()
        {
            long searchLong = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new LongSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.GreaterThan, searchLong, searchLong),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = "";  // No long fields.
            Assert.AreEqual(expected, result);
        }

        
        /// <summary>
        /// 1 criteria, 1 option, wildcard on Product - String
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringWildcardOnProduct()
        {
            String searchString = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.StringStartsWith, searchString, searchString, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("((P.ProductName LIKE N'{0}%') OR (P.Version LIKE N'{0}%'))", searchString);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on Product - DateTime
        /// </summary>
        [TestMethod]
        public void ToSqlStringDateTimeWildcardOnProduct()
        {
            DateTime searchDateTime = new DateTime(2010, 11, 12);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Product, "*", StackHashSearchOptionType.RangeExclusive, searchDateTime, searchDateTime),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Product, "P");
            String expected = String.Format("((P.DateCreatedLocal>'2010-11-12T00:00:00' AND P.DateCreatedLocal<'2010-11-12T00:00:00') OR (P.DateModifiedLocal>'2010-11-12T00:00:00' AND P.DateModifiedLocal<'2010-11-12T00:00:00'))");
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event - Integer
        /// </summary>
        [TestMethod]
        public void ToSqlStringIntegerWildcardOnEvent()
        {
            int searchInt = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.Event, "*", StackHashSearchOptionType.GreaterThan, searchInt, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Event, "E");

            String expected = String.Format("((E.EventId>20) OR (E.TotalHits>20) OR (E.WorkFlowStatus>20) OR (CABCOUNTER.CabCount>20))");
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event - Long
        /// </summary>
        [TestMethod]
        public void ToSqlStringLongWildcardOnEvent()
        {
            long searchLong = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new LongSearchOption(StackHashObjectType.Event, "*", StackHashSearchOptionType.RangeInclusive, searchLong, searchLong + 1),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Event, "E");
            String expected = "";  // No long fields.
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event - String
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringWildcardOnEvent()
        {
            String searchString = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.Event, "*", StackHashSearchOptionType.RangeInclusive, searchString, searchString, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Event, "E");

            String expected = "((ET.EventTypeName BETWEEN N'MyProduct' AND N'MyProduct') OR (E.BugId BETWEEN N'MyProduct' AND N'MyProduct') OR (E.PlugInBugId BETWEEN N'MyProduct' AND N'MyProduct'))";
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event - DateTime
        /// </summary>
        [TestMethod]
        public void ToSqlStringDateTimeWildcardOnEvent()
        {
            DateTime searchDateTime = new DateTime(2010, 11, 12);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.Event, "*", StackHashSearchOptionType.RangeExclusive, searchDateTime, searchDateTime.AddDays(2)),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.Event, "E");

            String expected = "((E.DateCreatedLocal>'2010-11-12T00:00:00' AND E.DateCreatedLocal<'2010-11-14T00:00:00') OR (E.DateModifiedLocal>'2010-11-12T00:00:00' AND E.DateModifiedLocal<'2010-11-14T00:00:00'))";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on EventSignature - Integer
        /// </summary>
        [TestMethod]
        public void ToSqlStringIntegerWildcardOnEventSignature()
        {
            int searchInt = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.EventSignature, "*", StackHashSearchOptionType.GreaterThan, searchInt, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventSignature, "ES");

            String expected = "";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event Signature- Long
        /// </summary>
        [TestMethod]
        public void ToSqlStringLongWildcardOnEventSignature()
        {
            long searchLong = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new LongSearchOption(StackHashObjectType.EventSignature, "*", StackHashSearchOptionType.RangeInclusive, searchLong, searchLong + 1),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventSignature, "ES");
            String expected = "((ES.Offset BETWEEN 20 AND 21) OR (ES.ExceptionCode BETWEEN 20 AND 21))";  // No long fields.
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event Signature- String
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringWildcardOnEventSignature()
        {
            String searchString = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.EventSignature, "*", StackHashSearchOptionType.RangeInclusive, searchString, searchString, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventSignature, "ES");

            String expected = "((ES.ApplicationName BETWEEN N'MyProduct' AND N'MyProduct') OR (ES.ApplicationVersion BETWEEN N'MyProduct' AND N'MyProduct') OR (ES.ModuleName BETWEEN N'MyProduct' AND N'MyProduct') OR (ES.ModuleVersion BETWEEN N'MyProduct' AND N'MyProduct'))";
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event Signature- DateTime
        /// </summary>
        [TestMethod]
        public void ToSqlStringDateTimeWildcardOnEventSignature()
        {
            DateTime searchDateTime = new DateTime(2010, 11, 12);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.EventSignature, "*", StackHashSearchOptionType.RangeExclusive, searchDateTime, searchDateTime.AddDays(2)),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventSignature, "ES");

            String expected = "((ES.ApplicationTimeStamp>'2010-11-12T00:00:00' AND ES.ApplicationTimeStamp<'2010-11-14T00:00:00') OR (ES.ModuleTimeStamp>'2010-11-12T00:00:00' AND ES.ModuleTimeStamp<'2010-11-14T00:00:00'))";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on EventInfo - Integer
        /// </summary>
        [TestMethod]
        public void ToSqlStringIntegerWildcardOnEventInfo()
        {
            int searchInt = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.EventInfo, "*", StackHashSearchOptionType.GreaterThan, searchInt, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventInfo, "EI");

            String expected = "((EI.LocaleId>20) OR (EI.TotalHits>20))";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on EventInfo- Long
        /// </summary>
        [TestMethod]
        public void ToSqlStringLongWildcardOnEventInfo()
        {
            long searchLong = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new LongSearchOption(StackHashObjectType.EventInfo, "*", StackHashSearchOptionType.RangeInclusive, searchLong, searchLong + 1),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventInfo, "EI");
            String expected = "";
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on EventInfo- String
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringWildcardOnEventInfo()
        {
            String searchString = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.EventInfo, "*", StackHashSearchOptionType.RangeInclusive, searchString, searchString, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventInfo, "EI");

            String expected = "((l.LocaleName BETWEEN N'MyProduct' AND N'MyProduct') OR (l.LocaleCode BETWEEN N'MyProduct' AND N'MyProduct') OR (O.OperatingSystemName BETWEEN N'MyProduct' AND N'MyProduct') OR (O.OperatingSystemVersion BETWEEN N'MyProduct' AND N'MyProduct'))";
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on Event Info- DateTime
        /// </summary>
        [TestMethod]
        public void ToSqlStringDateTimeWildcardOnEventInfo()
        {
            DateTime searchDateTime = new DateTime(2010, 11, 12);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.EventInfo, "*", StackHashSearchOptionType.RangeExclusive, searchDateTime, searchDateTime.AddDays(2)),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.EventInfo, "EI");

            String expected = "((EI.DateCreatedLocal>'2010-11-12T00:00:00' AND EI.DateCreatedLocal<'2010-11-14T00:00:00') OR (EI.DateModifiedLocal>'2010-11-12T00:00:00' AND EI.DateModifiedLocal<'2010-11-14T00:00:00') OR (EI.HitDateLocal>'2010-11-12T00:00:00' AND EI.HitDateLocal<'2010-11-14T00:00:00'))";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on CabInfo - Integer
        /// </summary>
        [TestMethod]
        public void ToSqlStringIntegerWildcardOnCabInfo()
        {
            int searchInt = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new IntSearchOption(StackHashObjectType.CabInfo, "*", StackHashSearchOptionType.GreaterThan, searchInt, 0),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.CabInfo, "C");

            String expected = "((C.EventId>20) OR (C.CabId>20))";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1 criteria, 1 option, wildcard on CabInfo- Long
        /// </summary>
        [TestMethod]
        public void ToSqlStringLongWildcardOnCabInfo()
        {
            long searchLong = 20;

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new LongSearchOption(StackHashObjectType.CabInfo, "*", StackHashSearchOptionType.RangeInclusive, searchLong, searchLong + 1),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.CabInfo, "C");
            String expected = "(C.SizeInBytes BETWEEN 20 AND 21)";
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on CabInfo- String
        /// </summary>
        [TestMethod]
        public void ToSqlStringStringWildcardOnCabInfo()
        {
            String searchString = "MyProduct";

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new StringSearchOption(StackHashObjectType.CabInfo, "*", StackHashSearchOptionType.RangeInclusive, searchString, searchString, false),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.CabInfo, "C");

            String expected = "((CET.EventTypeName BETWEEN N'MyProduct' AND N'MyProduct') OR (C.CabFileName BETWEEN N'MyProduct' AND N'MyProduct'))";
            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// 1 criteria, 1 option, wildcard on CabInfo- DateTime
        /// </summary>
        [TestMethod]
        public void ToSqlStringDateTimeWildcardOnCabInfo()
        {
            DateTime searchDateTime = new DateTime(2010, 11, 12);

            StackHashSearchOptionCollection options1 = new StackHashSearchOptionCollection()
            {
                new DateTimeSearchOption(StackHashObjectType.CabInfo, "*", StackHashSearchOptionType.RangeExclusive, searchDateTime, searchDateTime.AddDays(2)),
            };

            StackHashSearchCriteriaCollection allCriteria = new StackHashSearchCriteriaCollection() 
            {
                new StackHashSearchCriteria(options1),                    
            };

            String result = allCriteria.ToSqlString(StackHashObjectType.CabInfo, "C");

            String expected = "((C.DateCreatedLocal>'2010-11-12T00:00:00' AND C.DateCreatedLocal<'2010-11-14T00:00:00') OR (C.DateModifiedLocal>'2010-11-12T00:00:00' AND C.DateModifiedLocal<'2010-11-14T00:00:00'))";
            Assert.AreEqual(expected, result);
        }
    }
}
