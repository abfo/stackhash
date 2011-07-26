using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for StatusMappingsUnitTests
    /// </summary>
    [TestClass]
    public class StatusMappingsUnitTests
    {
        private Utils m_Utils;


        public StatusMappingsUnitTests()
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
            m_Utils = new Utils();
            m_Utils.SetProxy(null);
            m_Utils.RemoveAllContexts();
            m_Utils.RestartService();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                try
                {
                    m_Utils.DeactivateContext(0);
                    m_Utils.DeleteIndex(0);
                }
                catch
                {
                }
                m_Utils.Dispose();
                m_Utils = null;
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
        public void DefaultWorkFlowMappings()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);
        }

        [TestMethod]
        public void DefaultGroupMappings()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.Group);

            Assert.AreEqual(StackHashMappingType.Group, resp.MappingType);
            Assert.AreEqual(0, resp.StatusMappings.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetWorkFlowMappingTooFew()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, new StackHashMappingCollection());
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Unexpected number of workflow mappings"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetWorkFlowMappingTooMany()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);

            StackHashMapping mapping = new StackHashMapping();
            mapping.Id = 12;
            mapping.MappingType = StackHashMappingType.WorkFlow;
            mapping.Name = "Fred";

            resp.StatusMappings.Add(mapping);

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Unexpected number of workflow mappings"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetMappingMixedTypes()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);

            StackHashMapping mapping = new StackHashMapping();
            mapping.Id = 12;
            mapping.MappingType = StackHashMappingType.WorkFlow;
            mapping.Name = "Fred";

            resp.StatusMappings.Add(mapping);

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.Group, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Mapping types don't match"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetWorkFlowMappingDuplicateIds()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);

            resp.StatusMappings[1].Id = resp.StatusMappings[0].Id;

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Duplicate workflow mapping id"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetWorkFlowMappingIdTooSmall()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);

            resp.StatusMappings[1].Id = -1;

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Invalid negative workflow id"));
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetWorkFlowMappingIdTooLarge()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);

            resp.StatusMappings[1].Id = 17;

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Workflow id too large"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetWorkFlowMappingNullName()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);

            Assert.AreEqual(StackHashMappingType.WorkFlow, resp.MappingType);
            Assert.AreEqual(16, resp.StatusMappings.Count);

            resp.StatusMappings[1].Name = null;

            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Mapping name of null found"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetGroupNotImplemented()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse resp = m_Utils.GetStatusMappings(0, StackHashMappingType.Group);

            Assert.AreEqual(StackHashMappingType.Group, resp.MappingType);
            Assert.AreEqual(0, resp.StatusMappings.Count);

            StackHashMapping mapping = new StackHashMapping();
            mapping.MappingType = StackHashMappingType.Group;
            mapping.Id = 0;
            mapping.Name = "Hello";

            resp.StatusMappings.Add(mapping);
            try
            {
                SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.Group, resp.StatusMappings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Detail.Message.Contains("Groups are not currently implemented"));
                throw;
            }
        }

        
        private StackHashMapping findMapping(StackHashMappingCollection mappings, StackHashMappingType mappingType, int id)
        {
            foreach (StackHashMapping mapping in mappings)
            {
                if ((mapping.MappingType == mappingType) && (mapping.Id == id))
                    return mapping;
            }

            return null;
        }


        [TestMethod]
        public void SetWorkFlowMappingSameValues()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse getResp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);
            Assert.AreEqual(StackHashMappingType.WorkFlow, getResp.MappingType);
            Assert.AreEqual(16, getResp.StatusMappings.Count);

            SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, getResp.StatusMappings);

            Assert.AreEqual(StackHashMappingType.WorkFlow, setResp.MappingType);

            GetStatusMappingsResponse getResp2 = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);
            Assert.AreEqual(StackHashMappingType.WorkFlow, getResp2.MappingType);
            Assert.AreEqual(16, getResp2.StatusMappings.Count);

            foreach (StackHashMapping mapping in getResp.StatusMappings)
            {
                StackHashMapping matchMapping = findMapping(getResp2.StatusMappings, mapping.MappingType, mapping.Id);
                Assert.AreNotEqual(null, matchMapping);

                Assert.AreEqual(mapping.MappingType, matchMapping.MappingType);
                Assert.AreEqual(mapping.Id, matchMapping.Id);
                Assert.AreEqual(mapping.Name, matchMapping.Name);
            }
        
        }

        [TestMethod]
        public void SetWorkFlowMappingChangedValues()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress, true);
            m_Utils.ActivateContext(0);

            GetStatusMappingsResponse getResp = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);
            Assert.AreEqual(StackHashMappingType.WorkFlow, getResp.MappingType);
            Assert.AreEqual(16, getResp.StatusMappings.Count);

            foreach (StackHashMapping mapping in getResp.StatusMappings)
            {
                mapping.Name += "Hello";
            }

            SetStatusMappingsResponse setResp = m_Utils.SetStatusMappings(0, StackHashMappingType.WorkFlow, getResp.StatusMappings);



            Assert.AreEqual(StackHashMappingType.WorkFlow, setResp.MappingType);

            GetStatusMappingsResponse getResp2 = m_Utils.GetStatusMappings(0, StackHashMappingType.WorkFlow);
            Assert.AreEqual(StackHashMappingType.WorkFlow, getResp2.MappingType);
            Assert.AreEqual(16, getResp2.StatusMappings.Count);

            foreach (StackHashMapping mapping in getResp.StatusMappings)
            {
                StackHashMapping matchMapping = findMapping(getResp2.StatusMappings, mapping.MappingType, mapping.Id);
                Assert.AreNotEqual(null, matchMapping);

                Assert.AreEqual(mapping.MappingType, matchMapping.MappingType);
                Assert.AreEqual(mapping.Id, matchMapping.Id);
                Assert.AreEqual(mapping.Name, matchMapping.Name);
            }

        }
    }
}
