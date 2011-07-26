using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for SqlMappingsUnitTests
    /// </summary>
    [TestClass]
    public class SqlMappingsUnitTests
    {
        private SqlErrorIndex m_Index;
        private String m_RootCabFolder;
        private TestContext testContextInstance;

        public SqlMappingsUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            SqlConnection.ClearAllPools();

            m_RootCabFolder = "C:\\StackHashUnitTests\\MappingsUnitTests";

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
        /// Adds the specified number of Mappings to the MappingsTable.
        /// </summary>
        public void addMappings(StackHashMappingType mappingType, StackHashMappingCollection mappings, int expectedCount)
        {
            // Create a clean index.
            m_Index = new SqlErrorIndex(StackHashSqlConfiguration.Default, SqlUtils.UnitTestDatabase, m_RootCabFolder);
            m_Index.DeleteIndex();
            m_Index.Activate();

            StackHashMappingCollection currentMappings = m_Index.GetMappings(mappingType);

            if (mappingType == StackHashMappingType.WorkFlow)
                Assert.AreEqual(16, currentMappings.Count);
            else
                Assert.AreEqual(0, currentMappings.Count);

            m_Index.AddMappings(mappings);
                        
            currentMappings = m_Index.GetMappings(mappingType);
            Assert.AreEqual(expectedCount, currentMappings.Count);

            
            foreach (StackHashMapping mapping in mappings)
            {
                StackHashMapping matchingMapping = mappings.FindMapping(mapping.MappingType, mapping.Id);
                Assert.AreNotEqual(null, matchingMapping);

                Assert.AreEqual(0, mapping.CompareTo(matchingMapping));
            }
        }

        /// <summary>
        /// Add 0 Updates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullMappingsException()
        {
            StackHashMappingCollection mappings = null;
            addMappings(StackHashMappingType.Group, mappings, 0);
        }

        /// <summary>
        /// Default workflow should be 16.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DefaultWorkFlow()
        {
            StackHashMappingCollection mappings = null;
            addMappings(StackHashMappingType.WorkFlow, mappings, 16);
        }

        
        /// <summary>
        /// Add no mappings.
        /// </summary>
        [TestMethod]
        public void AddNoMappings()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            addMappings(StackHashMappingType.Group, mappings, 0);
        }

        /// <summary>
        /// Add 1 mapping.
        /// </summary>
        [TestMethod]
        public void Add1Mapping()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            addMappings(StackHashMappingType.Group, mappings, 1);
        }
        
        
        /// <summary>
        /// Add 2 mappings.
        /// </summary>
        [TestMethod]
        public void Add2Mappings()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 2, "Active2"));
            addMappings(StackHashMappingType.Group, mappings, 2);
        }

        /// <summary>
        /// Add 2 mappings - duplicate
        /// </summary>
        [TestMethod]
        public void Add2MappingsDuplicates()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            addMappings(StackHashMappingType.Group, mappings, 1);
        }

        /// <summary>
        /// Add 2 mappings - duplicate
        /// </summary>
        [TestMethod]
        public void Add2MappingsDuplicatesNonZeroType()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            addMappings(StackHashMappingType.Group, mappings, 1);
        }

        /// <summary>
        /// Add 2 mappings - different types same id - check workflow.
        /// </summary>
        [TestMethod]
        public void Add2MappingsDifferentTypesSameIdCheckWorkFlow()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 1, "Active"));
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            addMappings(StackHashMappingType.WorkFlow, mappings, 16);
        }

        /// <summary>
        /// Add 2 mappings - different types same id - check group.
        /// </summary>
        [TestMethod]
        public void Add2MappingsDifferentTypesSameIdCheckGroup()
        {
            StackHashMappingCollection mappings = new StackHashMappingCollection();
            mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 1, "Active"));
            mappings.Add(new StackHashMapping(StackHashMappingType.Group, 1, "Active"));
            addMappings(StackHashMappingType.Group, mappings, 1);
        }

    
    }
}
