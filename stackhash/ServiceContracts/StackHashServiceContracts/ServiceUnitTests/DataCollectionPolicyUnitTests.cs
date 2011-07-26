using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;
using System.ServiceModel;


namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for DataCollectionPolicyUnitTests
    /// </summary>
    [TestClass]
    public class DataCollectionPolicyUnitTests
    {
        Utils m_Utils;
        int m_NumDefaultPolicies = 2;
        int m_DefaultCabConditionPolicy = 0;
        int m_DefaultEventConditionPolicy = 1;

        StackHashCollectionPolicyCollection m_DefaultPolicyCollection;

        public DataCollectionPolicyUnitTests()
        {
            m_DefaultPolicyCollection = new StackHashCollectionPolicyCollection();

            m_DefaultPolicyCollection.Add(new StackHashCollectionPolicy());
            m_DefaultPolicyCollection[0].RootObject = StackHashCollectionObject.Global;
            m_DefaultPolicyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            m_DefaultPolicyCollection[0].CollectionType = StackHashCollectionType.Count;
            m_DefaultPolicyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            m_DefaultPolicyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            m_DefaultPolicyCollection[0].CollectLarger = true;
            m_DefaultPolicyCollection[0].RootId = 0;
            m_DefaultPolicyCollection[0].Maximum = 2;
            m_DefaultPolicyCollection[0].Percentage = 0;

            m_DefaultPolicyCollection.Add(new StackHashCollectionPolicy());
            m_DefaultPolicyCollection[1].RootObject = StackHashCollectionObject.Global;
            m_DefaultPolicyCollection[1].CollectionOrder = StackHashCollectionOrder.AsReceived;
            m_DefaultPolicyCollection[1].CollectionType = StackHashCollectionType.All;
            m_DefaultPolicyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            m_DefaultPolicyCollection[1].ConditionObject = StackHashCollectionObject.Event;
            m_DefaultPolicyCollection[1].CollectLarger = true;
            m_DefaultPolicyCollection[1].RootId = 0;
            m_DefaultPolicyCollection[1].Maximum = 0;
            m_DefaultPolicyCollection[1].Percentage = 100;
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();
            tidyTest();
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

        private void tidyTest()
        {
            if (m_Utils != null)
            {
                // Get rid of any test contexts.
                m_Utils.RemoveAllContexts();
                m_Utils.RestartService();
            }
        }

        private void checkDefaultPolicyCabCondition(StackHashCollectionPolicy policy)
        {
            Assert.AreEqual(StackHashCollectionObject.Global, policy.RootObject);
            Assert.AreEqual(StackHashCollectionOrder.AsReceived, policy.CollectionOrder);
            Assert.AreEqual(StackHashCollectionType.Count, policy.CollectionType);
            Assert.AreEqual(StackHashCollectionObject.Cab, policy.ObjectToCollect);
            Assert.AreEqual(StackHashCollectionObject.Cab, policy.ConditionObject);
            Assert.AreEqual(true, policy.CollectLarger);
            Assert.AreEqual(0, policy.RootId);
            Assert.AreEqual(2, policy.Maximum);
            Assert.AreEqual(0, policy.Percentage);
        }

        private void checkDefaultPolicyEventCondition(StackHashCollectionPolicy policy)
        {
            Assert.AreEqual(StackHashCollectionObject.Global, policy.RootObject);
            Assert.AreEqual(StackHashCollectionOrder.AsReceived, policy.CollectionOrder);
            Assert.AreEqual(StackHashCollectionType.All, policy.CollectionType);
            Assert.AreEqual(StackHashCollectionObject.Cab, policy.ObjectToCollect);
            Assert.AreEqual(StackHashCollectionObject.Event, policy.ConditionObject);
            Assert.AreEqual(true, policy.CollectLarger);
            Assert.AreEqual(0, policy.RootId);
            Assert.AreEqual(0, policy.Maximum);
            Assert.AreEqual(100, policy.Percentage);
        }

        private void comparePolicy(StackHashCollectionPolicy policy1, StackHashCollectionPolicy policy2)
        {
            Assert.AreEqual(policy1.RootObject, policy2.RootObject);
            Assert.AreEqual(policy1.CollectionOrder, policy2.CollectionOrder);
            Assert.AreEqual(policy1.CollectionType, policy2.CollectionType);
            Assert.AreEqual(policy1.CollectLarger, policy2.CollectLarger);
            Assert.AreEqual(policy1.RootId, policy2.RootId);
            Assert.AreEqual(policy1.Maximum, policy2.Maximum);
            Assert.AreEqual(policy1.Percentage, policy2.Percentage);
            Assert.AreEqual(policy1.ConditionObject, policy2.ConditionObject);
            Assert.AreEqual(policy1.ObjectToCollect, policy2.ObjectToCollect);
        }


        private void checkDefaults(StackHashCollectionPolicyCollection policyCollection, int index)
        {
            Assert.AreNotEqual(null, policyCollection);
            Assert.AreEqual(true, policyCollection.Count >= m_DefaultPolicyCollection.Count + index);

            int defaultIndex = 0;
            for (int i = index; i < index + policyCollection.Count; i++, defaultIndex++)
            {
                comparePolicy(m_DefaultPolicyCollection[defaultIndex], policyCollection[i]);
            }
        }

        /// <summary>
        /// Gets the main StackHashSettings and ensures that the default policies are
        /// present.
        /// </summary>
        public void getSettingsDefaultSettings(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            checkDefaults(resp.Settings.ContextCollection[0].CollectionPolicy, 0);
        }

        /// <summary>
        /// Gets the main StackHashSettings and ensures that the default policies are
        /// present.
        /// </summary>
        [TestMethod]
        public void GetSettingsDefaultSettings()
        {
            getSettingsDefaultSettings(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Gets the main StackHashSettings and ensures that the default policies are
        /// present.
        /// </summary>
        [TestMethod]
        public void GetSettingsDefaultSettingsSql()
        {
            getSettingsDefaultSettings(ErrorIndexType.SqlExpress);
        }

        

        /// <summary>
        /// GetDataCollectionPolicy - get ALL policies for global.
        /// </summary>
        public void getDataCollectionDefaultSettingsGetAllTrue(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            // The root object passed in here will be ignored because GetAll is set true.
            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            checkDefaults(resp.PolicyCollection, 0);
        }


        /// <summary>
        /// GetDataCollectionPolicy - get ALL policies for global.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllTrue()
        {
            getDataCollectionDefaultSettingsGetAllTrue(ErrorIndexType.Xml);
        }

        /// <summary>
        /// GetDataCollectionPolicy - get ALL policies for global.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllTrueSql()
        {
            getDataCollectionDefaultSettingsGetAllTrue(ErrorIndexType.SqlExpress);
        }


        
        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Any condition, Any ObjectToCollect.
        /// Should return both defaults.
        /// </summary>
        public void getDataCollectionDefaultSettingsGetAllFalseAnyAny(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, false, StackHashCollectionObject.Any, StackHashCollectionObject.Any);

            checkDefaults(resp.PolicyCollection, 0);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Any condition, Any ObjectToCollect.
        /// Should return both defaults.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseAnyAny()
        {
            getDataCollectionDefaultSettingsGetAllFalseAnyAny(ErrorIndexType.Xml);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Any condition, Any ObjectToCollect.
        /// Should return both defaults.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseAnyAnySql()
        {
            getDataCollectionDefaultSettingsGetAllFalseAnyAny(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Any condition, ObjectToCollect = Cab.
        /// Should return both defaults.
        /// </summary>
        public void getDataCollectionDefaultSettingsGetAllFalseAnyCab(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            checkDefaults(resp.PolicyCollection, 0);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Any condition, ObjectToCollect = Cab.
        /// Should return both defaults.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseAnyCab()
        {
            getDataCollectionDefaultSettingsGetAllFalseAnyCab(ErrorIndexType.Xml);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Any condition, ObjectToCollect = Cab.
        /// Should return both defaults.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseAnyCabSql()
        {
            getDataCollectionDefaultSettingsGetAllFalseAnyCab(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Condition = Cab, ObjectToCollect = Any.
        /// Should return just the Cab condition default.
        /// </summary>
        public void getDataCollectionDefaultSettingsGetAllFalseCabAny(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, false, StackHashCollectionObject.Cab, StackHashCollectionObject.Any);

            Assert.AreEqual(1, resp.PolicyCollection.Count);
            comparePolicy(resp.PolicyCollection[0], m_DefaultPolicyCollection[m_DefaultCabConditionPolicy]);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Condition = Cab, ObjectToCollect = Any.
        /// Should return just the Cab condition default.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseCabAny()
        {
            getDataCollectionDefaultSettingsGetAllFalseCabAny(ErrorIndexType.Xml);     
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Condition = Cab, ObjectToCollect = Any.
        /// Should return just the Cab condition default.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseCabAnySql()
        {
            getDataCollectionDefaultSettingsGetAllFalseCabAny(ErrorIndexType.SqlExpress);
        }

        

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Condition = Event, ObjectToCollect = Any.
        /// Should return just the Event condition default.
        /// </summary>
        public void getDataCollectionDefaultSettingsGetAllFalseEventAny(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext();

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, false, StackHashCollectionObject.Event, StackHashCollectionObject.Any);

            Assert.AreEqual(1, resp.PolicyCollection.Count);
            comparePolicy(resp.PolicyCollection[0], m_DefaultPolicyCollection[m_DefaultEventConditionPolicy]);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Condition = Event, ObjectToCollect = Any.
        /// Should return just the Event condition default.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseEventAny()
        {
            getDataCollectionDefaultSettingsGetAllFalseEventAny(ErrorIndexType.Xml);
        }

        /// <summary>
        /// GetDataCollectionPolicy. GetAll is false so should use the object fields.
        /// Condition = Event, ObjectToCollect = Any.
        /// Should return just the Event condition default.
        /// </summary>
        [TestMethod]
        public void GetDataCollectionDefaultSettingsGetAllFalseEventAnySql()
        {
            getDataCollectionDefaultSettingsGetAllFalseEventAny(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Set ALL - should replace with list specified.
        /// </summary>
        public void setAndGetGlobalSetAll(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 0;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;
           
            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }
        }

        /// <summary>
        /// Set ALL - should replace with list specified.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalSetAll()
        {
            setAndGetGlobalSetAll(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Set ALL - should replace with list specified.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalSetAllSql()
        {
            setAndGetGlobalSetAll(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// The default will be 1 global policy. Change it using SetAll = true. 
        /// Reset between set and get to make sure values are persisted.
        /// </summary>
        public void setAndGetGlobalSetAllWithReset(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext();

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 0;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            m_Utils.RestartService();

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }
        }

        /// <summary>
        /// The default will be 1 global policy. Change it using SetAll = true. 
        /// Reset between set and get to make sure values are persisted.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalSetAllWithReset()
        {
            setAndGetGlobalSetAllWithReset(ErrorIndexType.Xml);
        }

        /// <summary>
        /// The default will be 1 global policy. Change it using SetAll = true. 
        /// Reset between set and get to make sure values are persisted.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalSetAllWithResetSql()
        {
            setAndGetGlobalSetAllWithReset(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Replace the global default policy.
        /// </summary>
        public void setAndGetGlobalNotSetAll(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 0;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            Assert.AreEqual(m_NumDefaultPolicies, resp.PolicyCollection.Count);
            checkDefaultPolicyCabCondition(resp.PolicyCollection[0]);
            comparePolicy(policyCollection[0], resp.PolicyCollection[1]);
        }

        /// <summary>
        /// Replace the global default policy.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalNotSetAll()
        {
            setAndGetGlobalNotSetAll(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Replace the global default policy.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalNotSetAllSql()
        {
            setAndGetGlobalNotSetAll(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Replace the global policy. With reset.
        /// </summary>
        public void setAndGetGlobalSetOneWithReset(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Global;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 0;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            m_Utils.RestartService();

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            Assert.AreEqual(2, resp.PolicyCollection.Count);
            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
            comparePolicy(m_DefaultPolicyCollection[m_DefaultEventConditionPolicy], resp.PolicyCollection[1]);
        }

        /// <summary>
        /// Replace the global policy. With reset.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalSetOneWithReset()
        {
            setAndGetGlobalSetOneWithReset(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Replace the global policy. With reset.
        /// </summary>
        [TestMethod]
        public void SetAndGetGlobalSetOneWithResetSql()
        {
            setAndGetGlobalSetOneWithReset(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// Add a new policy.
        /// </summary>
        public void addNewPolicy(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext();

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 20;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;
            
            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count + m_NumDefaultPolicies, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i + m_NumDefaultPolicies]);
            }


            // Get the specific policy.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, policyCollection[0].RootId, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Add a new policy.
        /// </summary>
        [TestMethod]
        public void AddNewPolicy()
        {
            addNewPolicy(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add a new policy.
        /// </summary>
        [TestMethod]
        public void AddNewPolicySql()
        {
            addNewPolicy(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// Add a new policy with reset between the set and get.
        /// </summary>
        public void addNewPolicyWithReset(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext();

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 20;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            m_Utils.RestartService();

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count + m_NumDefaultPolicies, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i + m_NumDefaultPolicies]);
            }


            // Get the specific policy.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, policyCollection[0].RootId, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Add a new policy with reset between the set and get.
        /// </summary>
        [TestMethod]
        public void AddNewPolicyWithReset()
        {
            addNewPolicyWithReset(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add a new policy with reset between the set and get.
        /// </summary>
        [TestMethod]
        public void AddNewPolicyWithResetSql()
        {
            addNewPolicyWithReset(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// Add 2 the same policy type - different IDs.
        /// </summary>
        public void add2NewPolicy(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            StackHashCollectionPolicyCollection policyCollection2 = new StackHashCollectionPolicyCollection();
            policyCollection2.Add(new StackHashCollectionPolicy());

            policyCollection2[0].RootObject = StackHashCollectionObject.Product;
            policyCollection2[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection2[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection2[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection2[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection2[0].CollectLarger = false;
            policyCollection2[0].RootId = 2;
            policyCollection2[0].Maximum = 20;
            policyCollection2[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection2, false);


            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2 + m_NumDefaultPolicies, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[m_NumDefaultPolicies]);
            comparePolicy(policyCollection2[0], resp.PolicyCollection[m_NumDefaultPolicies + 1]);

            // Get the specific policy product 1.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);

            // Get the specific policy product 2.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, 2, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            comparePolicy(policyCollection2[0], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Add 2 the same policy type - different IDs.
        /// </summary>
        [TestMethod]
        public void Add2NewPolicy()
        {
            add2NewPolicy(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add 2 the same policy type - different IDs.
        /// </summary>
        [TestMethod]
        public void Add2NewPolicySql()
        {
            add2NewPolicy(ErrorIndexType.SqlExpress);
        }

        

        /// <summary>
        /// Add 2 the same policy type - different IDs - replace original (setall = true)
        /// </summary>
        public void add2NewPolicyReplace(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.File;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }


            // Get the specific policy product 1.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(1, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);

            // Get the specific policy file 1.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.File, 1, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(1, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[1], resp.PolicyCollection[0]);
        }


        /// <summary>
        /// Add 2 the same policy type - different IDs - replace original (setall = true)
        /// </summary>
        [TestMethod]
        public void Add2NewPolicyReplace()
        {
            add2NewPolicyReplace(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add 2 the same policy type - different IDs - replace original (setall = true)
        /// </summary>
        [TestMethod]
        public void Add2NewPolicyReplaceSql()
        {
            add2NewPolicyReplace(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Add remove policy.
        /// </summary>
        public void addRemovePolicy(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext();

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 20;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count + m_NumDefaultPolicies, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i + m_NumDefaultPolicies]);
            }


            // Get the specific policy.
            GetDataCollectionPolicyResponse resp2 = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, policyCollection[0].RootId, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp2.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp2.PolicyCollection[0]);

            // Now remove the policy.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 20, policyCollection[0].ConditionObject, policyCollection[0].ObjectToCollect);

            // Check the correct one was removed.
            GetDataCollectionPolicyResponse resp3 = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(m_NumDefaultPolicies, resp3.PolicyCollection.Count);
            checkDefaults(resp3.PolicyCollection, 0);
        }

        /// <summary>
        /// Add remove policy.
        /// </summary>
        [TestMethod]
        public void AddRemovePolicy()
        {
            addRemovePolicy(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add remove policy.
        /// </summary>
        [TestMethod]
        public void AddRemovePolicySql()
        {
            addRemovePolicy(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Add and then remove with reset.
        /// </summary>
        public void addRemovePolicyWithReset(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext();

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 20;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, false);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count + m_NumDefaultPolicies, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i + m_NumDefaultPolicies]);
            }


            // Get the specific policy.
            GetDataCollectionPolicyResponse resp2 = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, policyCollection[0].RootId, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp2.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp2.PolicyCollection[0]);


            // Now remove the policy.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 20, policyCollection[0].ConditionObject, policyCollection[0].ObjectToCollect);

            m_Utils.RestartService();

            // Check the correct one was removed.
            GetDataCollectionPolicyResponse resp3 = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(m_NumDefaultPolicies, resp3.PolicyCollection.Count);
            checkDefaults(resp3.PolicyCollection, 0);
        }

        /// <summary>
        /// Add and then remove with reset.
        /// </summary>
        [TestMethod]
        public void AddRemovePolicyWithReset()
        {
            addRemovePolicyWithReset(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add and then remove with reset.
        /// </summary>
        [TestMethod]
        public void AddRemovePolicyWithResetSql()
        {
            addRemovePolicyWithReset(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Remove non-existent.
        /// </summary>
        public void addRemoveWrongId(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[0].CollectLarger = false;
            policyCollection[0].RootId = 20;
            policyCollection[0].Maximum = 20;
            policyCollection[0].Percentage = 10;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }


            // Get the specific policy.
            GetDataCollectionPolicyResponse resp2 = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, policyCollection[0].RootId, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(policyCollection.Count, resp2.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp2.PolicyCollection[0]);


            // Now remove the policy - wrong ID so shouldn't remove it.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 10, policyCollection[0].ConditionObject, policyCollection[0].ObjectToCollect); // Wrong ID shouldn't delete.

            GetDataCollectionPolicyResponse resp3 = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }
        }

        /// <summary>
        /// Remove non-existent.
        /// </summary>
        [TestMethod]
        public void AddRemoveWrongId()
        {
            addRemoveWrongId(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Remove non-existent.
        /// </summary>
        [TestMethod]
        public void AddRemoveWrongIdSql()
        {
            addRemoveWrongId(ErrorIndexType.SqlExpress);
        }

        /// <summary>
        /// The active default should come back with both default policies.
        /// </summary>
        public void getActiveDefault(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetActiveDataCollectionPolicyResponse resp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4);

            // Returned in the opposite order than normal because global looked at first etc...
            Assert.AreEqual(2, resp.PolicyCollection.Count);
            checkDefaultPolicyEventCondition(resp.PolicyCollection[0]);
            checkDefaultPolicyCabCondition(resp.PolicyCollection[1]);
        }

        /// <summary>
        /// The active default should come back with both default policies.
        /// </summary>
        [TestMethod]
        public void GetActiveDefault()
        {
            getActiveDefault(ErrorIndexType.Xml);
        }

        /// <summary>
        /// The active default should come back with both default policies.
        /// </summary>
        [TestMethod]
        public void GetActiveDefaultSql()
        {
            getActiveDefault(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Global, Product, File, Event, Cab - cab match.
        /// </summary>
        public void getActiveCabMatch(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].CollectLarger = true;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 1;
            policyCollection[0].Percentage = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.File;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[1].CollectLarger = false;
            policyCollection[1].RootId = 2;
            policyCollection[1].Maximum = 2;
            policyCollection[1].Percentage = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Event;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.All;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].CollectLarger = false;
            policyCollection[2].RootId = 3;
            policyCollection[2].Maximum = 3;
            policyCollection[2].Percentage = 3;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[3].RootObject = StackHashCollectionObject.Cab;
            policyCollection[3].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[3].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[3].CollectionType = StackHashCollectionType.None;
            policyCollection[3].CollectionOrder = StackHashCollectionOrder.OldestFirst;
            policyCollection[3].CollectLarger = false;
            policyCollection[3].RootId = 4;
            policyCollection[3].Maximum = 4;
            policyCollection[3].Percentage = 4;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[4].RootObject = StackHashCollectionObject.Global;
            policyCollection[4].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[4].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[4].CollectionType = StackHashCollectionType.Count;
            policyCollection[4].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[4].CollectLarger = false;
            policyCollection[4].RootId = 0;
            policyCollection[4].Maximum = 2;
            policyCollection[4].Percentage = 0;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetActiveDataCollectionPolicyResponse resp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4);

            Assert.AreEqual(1, resp.PolicyCollection.Count);

            Assert.AreNotEqual(null, resp.PolicyCollection[0]);
            Assert.AreEqual(1, resp.PolicyCollection.Count);
            comparePolicy(policyCollection[3], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - cab match.
        /// </summary>
        [TestMethod]
        public void GetActiveCabMatch()
        {
            getActiveCabMatch(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - cab match.
        /// </summary>
        [TestMethod]
        public void GetActiveCabMatchSql()
        {
            getActiveCabMatch(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Global, Product, File, Event, Cab - event match.
        /// </summary>
        public void getActiveEventMatch(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].CollectLarger = true;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 1;
            policyCollection[0].Percentage = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.File;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[1].CollectLarger = false;
            policyCollection[1].RootId = 2;
            policyCollection[1].Maximum = 2;
            policyCollection[1].Percentage = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Event;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.All;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].CollectLarger = false;
            policyCollection[2].RootId = 3;
            policyCollection[2].Maximum = 3;
            policyCollection[2].Percentage = 3;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[3].RootObject = StackHashCollectionObject.Cab;
            policyCollection[3].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[3].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[3].CollectionType = StackHashCollectionType.None;
            policyCollection[3].CollectionOrder = StackHashCollectionOrder.OldestFirst;
            policyCollection[3].CollectLarger = false;
            policyCollection[3].RootId = 4;
            policyCollection[3].Maximum = 4;
            policyCollection[3].Percentage = 4;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[4].RootObject = StackHashCollectionObject.Global;
            policyCollection[4].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[4].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[4].CollectionType = StackHashCollectionType.Count;
            policyCollection[4].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[4].CollectLarger = false;
            policyCollection[4].RootId = 0;
            policyCollection[4].Maximum = 2;
            policyCollection[4].Percentage = 0;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetActiveDataCollectionPolicyResponse resp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 6);

            Assert.AreEqual(1, resp.PolicyCollection.Count);

            Assert.AreNotEqual(null, resp.PolicyCollection[0]);
            comparePolicy(policyCollection[2], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - event match.
        /// </summary>
        [TestMethod]
        public void GetActiveEventMatch()
        {
            getActiveEventMatch(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - event match.
        /// </summary>
        [TestMethod]
        public void GetActiveEventMatchSql()
        {
            getActiveEventMatch(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Global, Product, File, Event, Cab - file match.
        /// </summary>
        public void getActiveFileMatch(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].CollectLarger = true;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 1;
            policyCollection[0].Percentage = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.File;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[1].CollectLarger = false;
            policyCollection[1].RootId = 2;
            policyCollection[1].Maximum = 2;
            policyCollection[1].Percentage = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Event;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.All;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].CollectLarger = false;
            policyCollection[2].RootId = 3;
            policyCollection[2].Maximum = 3;
            policyCollection[2].Percentage = 3;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[3].RootObject = StackHashCollectionObject.Cab;
            policyCollection[3].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[3].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[3].CollectionType = StackHashCollectionType.None;
            policyCollection[3].CollectionOrder = StackHashCollectionOrder.OldestFirst;
            policyCollection[3].CollectLarger = false;
            policyCollection[3].RootId = 4;
            policyCollection[3].Maximum = 4;
            policyCollection[3].Percentage = 4;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[4].RootObject = StackHashCollectionObject.Global;
            policyCollection[4].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[4].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[4].CollectionType = StackHashCollectionType.Count;
            policyCollection[4].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[4].CollectLarger = false;
            policyCollection[4].RootId = 0;
            policyCollection[4].Maximum = 2;
            policyCollection[4].Percentage = 0;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetActiveDataCollectionPolicyResponse resp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 4, 6);

            Assert.AreEqual(1, resp.PolicyCollection.Count);

            Assert.AreNotEqual(null, resp.PolicyCollection[0]);
            comparePolicy(policyCollection[1], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - file match.
        /// </summary>
        [TestMethod]
        public void GetActiveFileMatch()
        {
            getActiveFileMatch(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - file match.
        /// </summary>
        [TestMethod]
        public void GetActiveFileMatchSql()
        {
            getActiveFileMatch(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Global, Product, File, Event, Cab - product match.
        /// </summary>
        public void getActiveProductMatch(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].CollectLarger = true;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 1;
            policyCollection[0].Percentage = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.File;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[1].CollectLarger = false;
            policyCollection[1].RootId = 2;
            policyCollection[1].Maximum = 2;
            policyCollection[1].Percentage = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Event;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.All;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].CollectLarger = false;
            policyCollection[2].RootId = 3;
            policyCollection[2].Maximum = 3;
            policyCollection[2].Percentage = 3;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[3].RootObject = StackHashCollectionObject.Cab;
            policyCollection[3].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[3].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[3].CollectionType = StackHashCollectionType.None;
            policyCollection[3].CollectionOrder = StackHashCollectionOrder.OldestFirst;
            policyCollection[3].CollectLarger = false;
            policyCollection[3].RootId = 4;
            policyCollection[3].Maximum = 4;
            policyCollection[3].Percentage = 4;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[4].RootObject = StackHashCollectionObject.Global;
            policyCollection[4].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[4].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[4].CollectionType = StackHashCollectionType.Count;
            policyCollection[4].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[4].CollectLarger = false;
            policyCollection[4].RootId = 0;
            policyCollection[4].Maximum = 2;
            policyCollection[4].Percentage = 0;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetActiveDataCollectionPolicyResponse resp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 3, 4, 6);

            Assert.AreEqual(1, resp.PolicyCollection.Count);

            Assert.AreNotEqual(null, resp.PolicyCollection[0]);
            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - product match.
        /// </summary>
        [TestMethod]
        public void GetActiveProductMatch()
        {
            getActiveProductMatch(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - product match.
        /// </summary>
        [TestMethod]
        public void GetActiveProductMatchSql()
        {
            getActiveProductMatch(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Global, Product, File, Event, Cab - global match.
        /// </summary>
        public void getActiveGlobalMatch(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.Count;
            policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[0].CollectLarger = true;
            policyCollection[0].RootId = 1;
            policyCollection[0].Maximum = 1;
            policyCollection[0].Percentage = 1;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[1].RootObject = StackHashCollectionObject.File;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Percentage;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.LargestFirst;
            policyCollection[1].CollectLarger = false;
            policyCollection[1].RootId = 2;
            policyCollection[1].Maximum = 2;
            policyCollection[1].Percentage = 2;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[2].RootObject = StackHashCollectionObject.Event;
            policyCollection[2].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[2].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[2].CollectionType = StackHashCollectionType.All;
            policyCollection[2].CollectionOrder = StackHashCollectionOrder.MostRecentFirst;
            policyCollection[2].CollectLarger = false;
            policyCollection[2].RootId = 3;
            policyCollection[2].Maximum = 3;
            policyCollection[2].Percentage = 3;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[3].RootObject = StackHashCollectionObject.Cab;
            policyCollection[3].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[3].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[3].CollectionType = StackHashCollectionType.None;
            policyCollection[3].CollectionOrder = StackHashCollectionOrder.OldestFirst;
            policyCollection[3].CollectLarger = false;
            policyCollection[3].RootId = 4;
            policyCollection[3].Maximum = 4;
            policyCollection[3].Percentage = 4;

            policyCollection.Add(new StackHashCollectionPolicy());
            policyCollection[4].RootObject = StackHashCollectionObject.Global;
            policyCollection[4].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[4].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[4].CollectionType = StackHashCollectionType.Count;
            policyCollection[4].CollectionOrder = StackHashCollectionOrder.AsReceived;
            policyCollection[4].CollectLarger = true;
            policyCollection[4].RootId = 2;
            policyCollection[4].Maximum = 3;
            policyCollection[4].Percentage = 4;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);

            GetActiveDataCollectionPolicyResponse resp = m_Utils.GetActiveDataCollectionPolicy(0, 2, 3, 4, 6);

            Assert.AreEqual(1, resp.PolicyCollection.Count);

            Assert.AreNotEqual(null, resp.PolicyCollection[0]);
            comparePolicy(policyCollection[4], resp.PolicyCollection[0]);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - global match.
        /// </summary>
        [TestMethod]
        public void GetActiveGlobalMatch()
        {
            getActiveGlobalMatch(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Global, Product, File, Event, Cab - global match.
        /// </summary>
        [TestMethod]
        public void GetActiveGlobalMatchSql()
        {
            getActiveGlobalMatch(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Add 2 different conditions for the same Object ID for cab collection.
        /// </summary>
        public void twoConditionsSameObjectId(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }


            // Get the specific cab collection policy product 1 - should return both.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, false, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
            comparePolicy(policyCollection[1], resp.PolicyCollection[1]);

            // Get the specific ANY policy product 1 - should return both.
            resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, false, StackHashCollectionObject.Any, StackHashCollectionObject.Any);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
            comparePolicy(policyCollection[1], resp.PolicyCollection[1]);
        }

        /// <summary>
        /// Add 2 different conditions for the same Object ID for cab collection.
        /// </summary>
        [TestMethod]
        public void TwoConditionsSameObjectId()
        {
            twoConditionsSameObjectId(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add 2 different conditions for the same Object ID for cab collection.
        /// </summary>
        [TestMethod]
        public void TwoConditionsSameObjectIdSql()
        {
            twoConditionsSameObjectId(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Add 2 different conditions for the same Object ID for cab collection - GetActive
        /// Should return both.
        /// </summary>
        public void twoConditionsSameObjectIdGetActive(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }


            // Get the specific cab collection policy product 1 - should return both.
            GetActiveDataCollectionPolicyResponse activeResp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, activeResp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], activeResp.PolicyCollection[0]);
            comparePolicy(policyCollection[1], activeResp.PolicyCollection[1]);
        }

        /// <summary>
        /// Add 2 different conditions for the same Object ID for cab collection - GetActive
        /// Should return both.
        /// </summary>
        [TestMethod]
        public void TwoConditionsSameObjectIdGetActive()
        {
            twoConditionsSameObjectIdGetActive(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Add 2 different conditions for the same Object ID for cab collection - GetActive
        /// Should return both.
        /// </summary>
        [TestMethod]
        public void TwoConditionsSameObjectIdGetActiveSql()
        {
            twoConditionsSameObjectIdGetActive(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// GetActive - Add 2 different conditions for the same Object ID for cab collection.
        /// Ask for Event collection policies - should return null.
        /// </summary>
        public void getActiveNoPolicyOfSameCollectionObject(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }


            // Get the specific cab collection policy product 1 - should return both.
            GetActiveDataCollectionPolicyResponse activeResp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4, StackHashCollectionObject.Event);
            Assert.AreEqual(0, activeResp.PolicyCollection.Count);
        }

        /// <summary>
        /// GetActive - Add 2 different conditions for the same Object ID for cab collection.
        /// Ask for Event collection policies - should return null.
        /// </summary>
        [TestMethod]
        public void GetActiveNoPolicyOfSameCollectionObject()
        {
            getActiveNoPolicyOfSameCollectionObject(ErrorIndexType.Xml);
        }

        /// <summary>
        /// GetActive - Add 2 different conditions for the same Object ID for cab collection.
        /// Ask for Event collection policies - should return null.
        /// </summary>
        [TestMethod]
        public void GetActiveNoPolicyOfSameCollectionObjectSql()
        {
            getActiveNoPolicyOfSameCollectionObject(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Remove policy - wrong condition - should not remove.
        /// </summary>
        public void removePolicyWrongCondition(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            // Wrong condition so shouldn't remove.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, StackHashCollectionObject.File, StackHashCollectionObject.Cab);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], resp.PolicyCollection[i]);
            }

            // Get the specific cab collection policy product 1 - should return both.
            GetActiveDataCollectionPolicyResponse activeResp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, activeResp.PolicyCollection.Count);
            for (int i = 0; i < policyCollection.Count; i++)
            {
                comparePolicy(policyCollection[i], activeResp.PolicyCollection[i]);
            }
        }

        /// <summary>
        /// Remove policy - wrong condition - should not remove.
        /// </summary>
        [TestMethod]
        public void RemovePolicyWrongCondition()
        {
            removePolicyWrongCondition(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Remove policy - wrong condition - should not remove.
        /// </summary>
        [TestMethod]
        public void RemovePolicyWrongConditionSql()
        {
            removePolicyWrongCondition(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Remove policy - First of Two for same object.
        /// </summary>
        public void removePolicyFirstOf2Conditions(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            // Wrong condition so shouldn't remove.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, StackHashCollectionObject.Event, StackHashCollectionObject.Cab);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(1, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[1], resp.PolicyCollection[0]);

            // Get the specific cab collection policy product 1 - should return both.
            GetActiveDataCollectionPolicyResponse activeResp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4, StackHashCollectionObject.Cab);
            Assert.AreEqual(1, activeResp.PolicyCollection.Count);
            comparePolicy(policyCollection[1], activeResp.PolicyCollection[0]);
        }

        /// <summary>
        /// Remove policy - First of Two for same object.
        /// </summary>
        [TestMethod]
        public void RemovePolicyFirstOf2Conditions()
        {
            removePolicyFirstOf2Conditions(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Remove policy - First of Two for same object.
        /// </summary>
        [TestMethod]
        public void RemovePolicyFirstOf2ConditionsSql()
        {
            removePolicyFirstOf2Conditions(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// Remove policy - Second of Two for same object.
        /// </summary>
        public void removePolicyOneOf2Conditions(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            // Wrong condition so shouldn't remove.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, StackHashCollectionObject.Cab, StackHashCollectionObject.Cab);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(1, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);

            // Get the specific cab collection policy product 1 - should return both.
            GetActiveDataCollectionPolicyResponse activeResp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4, StackHashCollectionObject.Cab);
            Assert.AreEqual(1, activeResp.PolicyCollection.Count);
            comparePolicy(policyCollection[0], activeResp.PolicyCollection[0]);
        }

        /// <summary>
        /// Remove policy - Second of Two for same object.
        /// </summary>
        [TestMethod]
        public void RemovePolicyOneOf2Conditions()
        {
            removePolicyOneOf2Conditions(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Remove policy - Second of Two for same object.
        /// </summary>
        [TestMethod]
        public void RemovePolicyOneOf2ConditionsSql()
        {
            removePolicyOneOf2Conditions(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// Remove policy - Wrong collection object.
        /// </summary>
        public void removePolicyWrongCollectionObject(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[0].RootObject = StackHashCollectionObject.Product;
            policyCollection[0].ConditionObject = StackHashCollectionObject.Event;
            policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[0].CollectionType = StackHashCollectionType.MinimumHitCount;
            policyCollection[0].RootId = 1;
            policyCollection[0].Minimum = 20;

            policyCollection.Add(new StackHashCollectionPolicy());

            policyCollection[1].RootObject = StackHashCollectionObject.Product;
            policyCollection[1].ConditionObject = StackHashCollectionObject.Cab;
            policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
            policyCollection[1].CollectionType = StackHashCollectionType.Count;
            policyCollection[1].CollectionOrder = StackHashCollectionOrder.SmallestFirst;
            policyCollection[1].CollectLarger = true;
            policyCollection[1].RootId = 1;
            policyCollection[1].Maximum = 10;
            policyCollection[1].Percentage = 20;

            m_Utils.SetDataCollectionPolicy(0, policyCollection, true);


            // Wrong condition so shouldn't remove.
            m_Utils.RemoveDataCollectionPolicy(0, StackHashCollectionObject.Product, 1, StackHashCollectionObject.Cab, StackHashCollectionObject.Event);

            GetDataCollectionPolicyResponse resp = m_Utils.GetDataCollectionPolicy(0, StackHashCollectionObject.Global, 0, true, StackHashCollectionObject.Any, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, resp.PolicyCollection.Count);

            comparePolicy(policyCollection[0], resp.PolicyCollection[0]);
            comparePolicy(policyCollection[1], resp.PolicyCollection[1]);

            // Get the specific cab collection policy product 1 - should return both.
            GetActiveDataCollectionPolicyResponse activeResp = m_Utils.GetActiveDataCollectionPolicy(0, 1, 2, 3, 4, StackHashCollectionObject.Cab);
            Assert.AreEqual(2, activeResp.PolicyCollection.Count);
            comparePolicy(policyCollection[0], activeResp.PolicyCollection[0]);
            comparePolicy(policyCollection[1], activeResp.PolicyCollection[1]);
        }


        /// <summary>
        /// Remove policy - Wrong collection object.
        /// </summary>
        [TestMethod]
        public void RemovePolicyWrongCollectionObject()
        {
            removePolicyWrongCollectionObject(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Remove policy - Wrong collection object.
        /// </summary>
        [TestMethod]
        public void RemovePolicyWrongCollectionObjectSql()
        {
            removePolicyWrongCollectionObject(ErrorIndexType.SqlExpress);
        }
    }
}
