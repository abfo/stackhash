using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

using ServiceUnitTests.StackHashServices;
using System.ServiceModel;
using StackHashUtilities;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for SettingsUnitTests
    /// </summary>
    [TestClass]
    public class SettingsUnitTests
    {
        Utils m_Utils;
        String m_TempFolder;

        public SettingsUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        private StackHashClientData DefaultClientData
        {
            get
            {
                StackHashClientData clientData = new StackHashClientData();
                clientData.ClientName = Environment.UserName;
                clientData.ClientId = 100;
                clientData.ClientRequestId = 10;
                clientData.ApplicationGuid = Guid.NewGuid();

                return clientData;
            }
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_TempFolder = Path.GetTempPath() + "\\StackHashUnitTestFolder";

            if (!Directory.Exists(m_TempFolder))
                Directory.CreateDirectory(m_TempFolder);
            m_Utils = new Utils();
            tidyTest();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup() 
        {
            if (m_Utils != null)
            {
                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                foreach (StackHashContextSettings settings in resp.Settings.ContextCollection)
                {
                    m_Utils.DeactivateContext(settings.Id);
                    m_Utils.DeleteIndex(settings.Id);
                }

                // Get rid of any test contexts.
                m_Utils.RemoveAllContexts();
                m_Utils.RestartService();
                m_Utils.Dispose();
                m_Utils = null;

                if (Directory.Exists(m_TempFolder))
                    PathUtils.DeleteDirectory(m_TempFolder, true);
            }
        }

        private void tidyTest()
        {
            if (m_Utils != null)
            {
                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                foreach (StackHashContextSettings settings in resp.Settings.ContextCollection)
                {
                    m_Utils.DeactivateContext(settings.Id);
                    m_Utils.DeleteIndex(settings.Id);
                }

                // Get rid of any test contexts.
                m_Utils.RemoveAllContexts();
                m_Utils.RestartService();
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

        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetCurrentSettings()
        {
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // May be an empty collection.
            Assert.AreNotEqual(null, resp.Settings);
            Assert.AreNotEqual(null, resp.Settings.ContextCollection);
        }

        [TestMethod]
        public void RemoveAllSettings()
        {
            m_Utils.RemoveAllContexts();
        }

        public void createNewContext(ErrorIndexType indexType)
        {

            // Add a context.
            CreateNewStackHashContextRequest addRequest = new CreateNewStackHashContextRequest();
            addRequest.ClientData = DefaultClientData;

            CreateNewStackHashContextResponse createNewResp = m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            Assert.AreEqual("UserName", resp.Settings.ContextCollection[0].WinQualSettings.UserName);
            Assert.AreEqual("Password", resp.Settings.ContextCollection[0].WinQualSettings.Password);
            Assert.AreEqual(7, resp.Settings.ContextCollection[0].WinQualSettings.SyncsBeforeResync);
        }

        [TestMethod]
        public void CreateNewContext()
        {
            createNewContext(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void CreateNewContextSql()
        {
            createNewContext(ErrorIndexType.SqlExpress);
        }

        
        public void create2NewContexts(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);
            
            // Make sure it has been added and is inactive.
            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            Assert.AreEqual(1, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create2NewContexts()
        {
            create2NewContexts(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2NewContextsSql()
        {
            create2NewContexts(ErrorIndexType.SqlExpress);
        }

        
        
        public void create2RemoveFirst(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);
    
            // Remove the first context.
            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(1, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

        }

        [TestMethod]
        public void Create2RemoveFirst()
        {
            create2RemoveFirst(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2RemoveFirstSql()
        {
            create2RemoveFirst(ErrorIndexType.SqlExpress);
        }

        

        public void create2RemoveFirstWithRestart(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(1, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
        }

        [TestMethod]
        public void Create2RemoveFirstWithRestart()
        {
            create2RemoveFirstWithRestart(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2RemoveFirstWithRestartSql()
        {
            create2RemoveFirstWithRestart(ErrorIndexType.SqlExpress);
        }
        
        
        public void create2RemoveSecond(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
        }

        [TestMethod]
        public void Create2RemoveSecond()
        {
            create2RemoveSecond(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2RemoveSecondSql()
        {
            create2RemoveSecond(ErrorIndexType.SqlExpress);
        }

        
        
        public void create2RemoveSecondWithRestart(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            // Remove the first context.
            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
        }

        [TestMethod]
        public void Create2RemoveSecondWithRestart()
        {
            create2RemoveSecondWithRestart(ErrorIndexType.Xml);
        }
        [TestMethod]
        public void Create2RemoveSecondWithRestartSql()
        {
            create2RemoveSecondWithRestart(ErrorIndexType.SqlExpress);
        }

        
        public void create2ContextsRemoveFirstThenCreateAThird(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(1, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(2, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create2ContextsRemoveFirstThenCreateAThird()
        {
            create2ContextsRemoveFirstThenCreateAThird(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2ContextsRemoveFirstThenCreateAThirdSql()
        {
            create2ContextsRemoveFirstThenCreateAThird(ErrorIndexType.SqlExpress);
        }
        
        
        public void create2ContextsRemoveFirstThenCreateAThirdWithRestart(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            // Remove the first context.
            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(1, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(2, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create2ContextsRemoveFirstThenCreateAThirdWithRestart()
        {
            create2ContextsRemoveFirstThenCreateAThirdWithRestart(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2ContextsRemoveFirstThenCreateAThirdWithRestartSql()
        {
            create2ContextsRemoveFirstThenCreateAThirdWithRestart(ErrorIndexType.SqlExpress);
        }

        
        public void create2ContextsRemoveSecondThenCreateAThird(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            // Remove the second context.
            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(2, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create2ContextsRemoveSecondThenCreateAThird()
        {
            create2ContextsRemoveSecondThenCreateAThird(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2ContextsRemoveSecondThenCreateAThirdSql()
        {
            create2ContextsRemoveSecondThenCreateAThird(ErrorIndexType.SqlExpress);
        }

        

        public void create2ContextsRemoveSecondThenCreateAThirdWithRestart(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            // Remove the second context.
            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(2, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create2ContextsRemoveSecondThenCreateAThirdWithRestart()
        {
            create2ContextsRemoveSecondThenCreateAThirdWithRestart(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2ContextsRemoveSecondThenCreateAThirdWithRestartSql()
        {
            create2ContextsRemoveSecondThenCreateAThirdWithRestart(ErrorIndexType.SqlExpress);
        }
        
        
        public void create3ContextsRemoveSecond(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            // Remove the second context.
            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(2, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create3ContextsRemoveSecond()
        {
            create3ContextsRemoveSecond(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create3ContextsRemoveSecondSql()
        {
            create3ContextsRemoveSecond(ErrorIndexType.SqlExpress);
        }

        
        public void create3ContextsRemoveSecondWithRestart(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            // Remove the second context.
            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(2, resp.Settings.ContextCollection[1].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void Create3ContextsRemoveSecondWithRestart()
        {
            create3ContextsRemoveSecondWithRestart(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create3ContextsRemoveSecondWithRestartSql()
        {
            create3ContextsRemoveSecondWithRestart(ErrorIndexType.SqlExpress);
        }

        
        
        public void create2ContextsRemoveBothThenAddThird(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);
            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
        }

        [TestMethod]
        public void Create2ContextsRemoveBothThenAddThird()
        {
            create2ContextsRemoveBothThenAddThird(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2ContextsRemoveBothThenAddThirdSql()
        {
            create2ContextsRemoveBothThenAddThird(ErrorIndexType.SqlExpress);
        }
        
        
        public void create2ContextsRemoveBothThenAddThirdRestart(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add the second context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);
            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);

            // Add the third context.
            m_Utils.CreateNewContext(indexType);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();
            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);

            Assert.AreEqual(0, resp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
        }

        [TestMethod]
        public void Create2ContextsRemoveBothThenAddThirdRestart()
        {
            create2ContextsRemoveBothThenAddThirdRestart(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Create2ContextsRemoveBothThenAddThirdRestartSql()
        {
            create2ContextsRemoveBothThenAddThirdRestart(ErrorIndexType.SqlExpress);
        }

        
        
        /// <summary>
        /// Should quietly succeed.
        /// </summary>
        public void removeSameContextTwice(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Add a context.
            m_Utils.CreateNewContext(indexType);

            // Remove the second context.
            m_Utils.DeleteIndex(1);
            m_Utils.RemoveContext(1);

            // Remove the second context.
            m_Utils.RemoveContext(1);
        }

        /// <summary>
        /// Should quietly succeed.
        /// </summary>
        [TestMethod]
        public void RemoveSameContextTwice()
        {
            removeSameContextTwice(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Should quietly succeed.
        /// </summary>
        [TestMethod]
        public void RemoveSameContextTwiceSql()
        {
            removeSameContextTwice(ErrorIndexType.SqlExpress);
        }


        public void set1ContextCheckAllFieldsUpdated(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            StackHashContextSettings setSettings = m_Utils.MakeContextSettings(0);

            SetStackHashPropertiesResponse resp = m_Utils.SetContextSettings(setSettings);

            // Get the settings.
            GetStackHashPropertiesRequest getRequest = new GetStackHashPropertiesRequest();
            getRequest.ClientData = DefaultClientData;

            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
            StackHashContextSettings getSettings = resp.Settings.ContextCollection[0];

            Assert.AreEqual(1, getResp.Settings.ContextCollection.Count);
            Assert.AreEqual(0, getResp.Settings.ContextCollection[0].Id);
            Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);

            m_Utils.CheckContextSettings(setSettings, getSettings);
        }

        [TestMethod]
        public void Set1ContextCheckAllFieldsUpdated()
        {
            set1ContextCheckAllFieldsUpdated(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Set1ContextCheckAllFieldsUpdatedSql()
        {
            set1ContextCheckAllFieldsUpdated(ErrorIndexType.SqlExpress);
        }


        public void set1ContextCheckAllFieldsUpdatedWithRestart(ErrorIndexType indexType)
        {
            try
            {
                // Add a context.
                m_Utils.CreateNewContext(indexType);

                StackHashContextSettings setSettings = m_Utils.MakeContextSettings(0);

                SetStackHashPropertiesResponse resp = m_Utils.SetContextSettings(setSettings);

                // Now restart the service. This tests that persistence of settings is working as expected.
                m_Utils.RestartService();

                // Get the settings.
                GetStackHashPropertiesRequest getRequest = new GetStackHashPropertiesRequest();
                getRequest.ClientData = DefaultClientData;

                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                StackHashContextSettings getSettings = resp.Settings.ContextCollection[0];

                Assert.AreEqual(1, getResp.Settings.ContextCollection.Count);
                Assert.AreEqual(0, getResp.Settings.ContextCollection[0].Id);
                Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);

                m_Utils.CheckContextSettings(setSettings, getSettings);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Console.WriteLine(ex.Message + " " + ex.Detail);
                throw;
            }
        }

        [TestMethod]
        public void Set1ContextCheckAllFieldsUpdatedWithRestart()
        {
            set1ContextCheckAllFieldsUpdatedWithRestart(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void Set1ContextCheckAllFieldsUpdatedWithRestartSql()
        {
            set1ContextCheckAllFieldsUpdatedWithRestart(ErrorIndexType.SqlExpress);
        }

        
        
        public void makeSureCorrectSettingsAreSet(ErrorIndexType indexType)
        {
            // Add a context.
            m_Utils.CreateNewContext(indexType);

            StackHashContextSettings settings1 = m_Utils.MakeContextSettings(0);
            m_Utils.SetContextSettings(settings1);

            m_Utils.CreateNewContext(indexType);

            StackHashContextSettings settings2 = m_Utils.MakeContextSettings(1);
            m_Utils.SetContextSettings(settings2);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);

            m_Utils.CheckContextSettings(settings1, resp.Settings.ContextCollection[0]);
            m_Utils.CheckContextSettings(settings2, resp.Settings.ContextCollection[1]);

        }


        [TestMethod]
        public void MakeSureCorrectSettingsAreSet()
        {
            makeSureCorrectSettingsAreSet(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void MakeSureCorrectSettingsAreSetSql()
        {
            makeSureCorrectSettingsAreSet(ErrorIndexType.SqlExpress);
        }
        

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextInvalidContextId()
        {
            try
            {
                m_Utils.ActivateContext(1);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Invalid ContextId"));
                throw;
            }        
        }


        public void activateDeactivateContext(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.RegisterForNotifications(true, Guid.NewGuid());
            m_Utils.ActivateContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            m_Utils.DeactivateContext(0);

            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
            Assert.AreEqual(1, getResp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);

            m_Utils.DeleteIndex(0);
        }

        [TestMethod]
        public void ActivateDeactivateContext()
        {
            activateDeactivateContext(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ActivateDeactivateContextSql()
        {
            activateDeactivateContext(ErrorIndexType.SqlExpress);
        }

        

        public void removeAnActiveContext(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            m_Utils.RemoveContext(0);

            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
            Assert.AreEqual(0, getResp.Settings.ContextCollection.Count);

            // create the context again so the index can be deleted.
            m_Utils.CreateAndSetNewContext(indexType);
        }

        [TestMethod]
        public void RemoveAnActiveContext()
        {
            removeAnActiveContext(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void RemoveAnActiveContextSql()
        {
            removeAnActiveContext(ErrorIndexType.SqlExpress);
        }

        public void activateSameContextTwice(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            m_Utils.ActivateContext(0);

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            m_Utils.DeactivateContext(0);

            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
            Assert.AreEqual(1, getResp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);
        }


        [TestMethod]
        public void ActivateSameContextTwice()
        {
            activateSameContextTwice(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ActivateSameContextTwiceSql()
        {
            activateSameContextTwice(ErrorIndexType.SqlExpress);
        }


        
        public void activateDeactivateContextNTimes(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            for (int i = 0; i < 100; i++)
            {
                m_Utils.ActivateContext(0);

                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
                Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

                m_Utils.DeactivateContext(0);

                GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
                Assert.AreEqual(1, getResp.Settings.ContextCollection.Count);
                Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);
            }

            m_Utils.DeleteIndex(0);
            m_Utils.RemoveContext(0);
        }

        [TestMethod]
        public void ActivateDeactivateContextNTimes()
        {
            activateDeactivateContextNTimes(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ActivateDeactivateContextNTimesSql()
        {
            activateDeactivateContextNTimes(ErrorIndexType.SqlExpress);
        }

        

        public void activateFirstOf2Contexts(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.SetContextSettings(m_Utils.MakeContextSettings(0));
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.SetContextSettings(m_Utils.MakeContextSettings(1));

            m_Utils.ActivateContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);
            Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void ActivateFirstOf2Contexts()
        {
            activateFirstOf2Contexts(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ActivateFirstOf2ContextsSql()
        {
            activateFirstOf2Contexts(ErrorIndexType.SqlExpress);
        }

        
        public void activateSecondOf2Contexts(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.SetContextSettings(m_Utils.MakeContextSettings(0));
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.SetContextSettings(m_Utils.MakeContextSettings(1));
            m_Utils.ActivateContext(1);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(2, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            Assert.AreEqual(true, resp.Settings.ContextCollection[1].IsActive);
        }

        [TestMethod]
        public void ActivateSecondOf2Contexts()
        {
            activateSecondOf2Contexts(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ActivateSecondOf2ContextsSql()
        {
            activateSecondOf2Contexts(ErrorIndexType.SqlExpress);
        }


        public void activateContextErrorIndexAlreadyInUseInactive(ErrorIndexType indexType)
        {
            try
            {
                m_Utils.CreateAndSetNewContext(indexType);
                m_Utils.CreateAndSetNewContext(indexType);
                m_Utils.ActivateContext(0);
            }

            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Error Index already in use"));

                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                Assert.AreEqual(2, resp.Settings.ContextCollection.Count);
                Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
                Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextErrorIndexAlreadyInUseInactive()
        {
            activateContextErrorIndexAlreadyInUseInactive(ErrorIndexType.Xml);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextErrorIndexAlreadyInUseInactiveSql()
        {
            activateContextErrorIndexAlreadyInUseInactive(ErrorIndexType.SqlExpress);
        }

        

        public void activateContextErrorIndexAlreadyInUseActive(ErrorIndexType indexType)
        {
            try
            {
                m_Utils.CreateAndSetNewContext(indexType);
                m_Utils.ActivateContext(0);

                m_Utils.CreateAndSetNewContext(indexType);
                m_Utils.ActivateContext(1);
            }

            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.AreEqual(true, ex.Message.Contains("Error Index already in use"));

                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                Assert.AreEqual(2, resp.Settings.ContextCollection.Count);
                Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);
                Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextErrorIndexAlreadyInUseActive()
        {
            activateContextErrorIndexAlreadyInUseActive(ErrorIndexType.Xml);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextErrorIndexAlreadyInUseActiveSql()
        {
            activateContextErrorIndexAlreadyInUseActive(ErrorIndexType.SqlExpress);
        }

        

        public void activateContextErrorIndexNameAlreadyInUse(ErrorIndexType indexType)
        {
            try
            {
                StackHashContextSettings settings1 = m_Utils.MakeContextSettings(0);
                StackHashContextSettings settings2 = m_Utils.MakeContextSettings(1);
                settings2.ErrorIndexSettings.Name = settings1.ErrorIndexSettings.Name;

                m_Utils.CreateAndSetNewContext(indexType);
                m_Utils.CreateAndSetNewContext(indexType);
                m_Utils.SetContextSettings(settings1);
                m_Utils.SetContextSettings(settings2);
                m_Utils.ActivateContext(0);
            }

            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Error Index Name already in use"));

                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                Assert.AreEqual(2, resp.Settings.ContextCollection.Count);
                Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
                Assert.AreEqual(false, resp.Settings.ContextCollection[1].IsActive);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextErrorIndexNameAlreadyInUse()
        {
            activateContextErrorIndexNameAlreadyInUse(ErrorIndexType.Xml);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ActivateContextErrorIndexNameAlreadyInUseSql()
        {
            activateContextErrorIndexNameAlreadyInUse(ErrorIndexType.SqlExpress);
        }

        
        public void deactivateSameContextTwice(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);
            m_Utils.ActivateContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            m_Utils.DeactivateContext(0);

            GetStackHashPropertiesResponse getResp = m_Utils.GetContextSettings();
            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);

            m_Utils.DeactivateContext(0);

            getResp = m_Utils.GetContextSettings();
            Assert.AreEqual(1, getResp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, getResp.Settings.ContextCollection[0].IsActive);
        }

        [TestMethod]
        public void DeactivateSameContextTwice()
        {
            deactivateSameContextTwice(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void DeactivateSameContextTwiceSql()
        {
            deactivateSameContextTwice(ErrorIndexType.SqlExpress);
        }

        
        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void DeactivateContextInvalidContextId()
        {
            try
            {
                m_Utils.DeactivateContext(1);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Invalid ContextId"));
                throw;
            }
        }

        
        public void changeSyncTimer(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            m_Utils.ActivateContext(0);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            resp.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time = new ScheduleTime();
            resp.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Hour = 12;
            resp.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Minute = 30;

            m_Utils.SetContextSettings(resp.Settings.ContextCollection[0]);

            // Now make sure the settings have changed.
            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(true, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreEqual(12, resp.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Hour);
            Assert.AreEqual(30, resp.Settings.ContextCollection[0].WinQualSyncSchedule[0].Time.Minute);

            GetStackHashServiceStatusResponse status = m_Utils.GetServiceStatus();

            Assert.AreEqual(1, status.Status.ContextStatusCollection.Count);
            Assert.AreEqual(true, status.Status.ContextStatusCollection[0].IsActive);

            StackHashTaskStatus taskStatus = m_Utils.FindTaskStatus(status.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTimerTask);
            Assert.AreNotEqual(null, taskStatus);
            Assert.AreEqual(StackHashTaskState.Running, taskStatus.TaskState);            
        }

        [TestMethod]
        public void ChangeSyncTimer()
        {
            changeSyncTimer(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ChangeSyncTimerSql()
        {
            changeSyncTimer(ErrorIndexType.SqlExpress);
        }

        
        public void proxySet(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = true;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = "http://www.stackhash.com";
            proxySettings.ProxyHost = "localhost";
            proxySettings.ProxyPort = 9000;
            proxySettings.ProxyUserName = ServiceTestSettings.WinQualUserName;
            proxySettings.ProxyPassword = ServiceTestSettings.WinQualPassword;

            m_Utils.SetProxy(proxySettings);


            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);
            Assert.AreEqual(proxySettings.UseProxy, resp.Settings.ProxySettings.UseProxy);
            Assert.AreEqual(proxySettings.UseProxyAuthentication, resp.Settings.ProxySettings.UseProxyAuthentication);
            Assert.AreEqual(proxySettings.ProxyDomain, resp.Settings.ProxySettings.ProxyDomain);
            Assert.AreEqual(proxySettings.ProxyPort, resp.Settings.ProxySettings.ProxyPort);
            Assert.AreEqual(proxySettings.ProxyUserName, resp.Settings.ProxySettings.ProxyUserName);
            Assert.AreEqual(proxySettings.ProxyPassword, resp.Settings.ProxySettings.ProxyPassword);

            proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = false;
            proxySettings.UseProxyAuthentication = true;
            proxySettings.ProxyDomain = "http://www.stackhash2.com";
            proxySettings.ProxyHost = "localhost2";
            proxySettings.ProxyPort = 8000;
            proxySettings.ProxyUserName = "InvalidUserName";
            proxySettings.ProxyPassword = "InvalidPassword";

            m_Utils.SetProxy(proxySettings);


            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);
            Assert.AreEqual(proxySettings.UseProxy, resp.Settings.ProxySettings.UseProxy);
            Assert.AreEqual(proxySettings.UseProxyAuthentication, resp.Settings.ProxySettings.UseProxyAuthentication);
            Assert.AreEqual(proxySettings.ProxyDomain, resp.Settings.ProxySettings.ProxyDomain);
            Assert.AreEqual(proxySettings.ProxyPort, resp.Settings.ProxySettings.ProxyPort);
            Assert.AreEqual(proxySettings.ProxyUserName, resp.Settings.ProxySettings.ProxyUserName);
            Assert.AreEqual(proxySettings.ProxyPassword, resp.Settings.ProxySettings.ProxyPassword);
        }



        [TestMethod]
        public void ProxySet()
        {
            proxySet(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ProxySetSql()
        {
            proxySet(ErrorIndexType.SqlExpress);
        }


        public void proxySetInvalid(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);
            Assert.AreEqual(false, resp.Settings.ProxySettings.UseProxy);

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = true;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = "localmachine";
            proxySettings.ProxyHost = null;
            proxySettings.ProxyPort = 9000;
            proxySettings.ProxyUserName = ServiceTestSettings.WinQualUserName;
            proxySettings.ProxyPassword = ServiceTestSettings.WinQualPassword;

            m_Utils.SetProxy(proxySettings);
        }


        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ProxySetInvalid()
        {
            proxySetInvalid(ErrorIndexType.Xml);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ProxySetInvalidSql()
        {
            proxySetInvalid(ErrorIndexType.SqlExpress);
        }

        
        public void proxySetWithReset(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = true;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = "localmachine";
            proxySettings.ProxyHost = "localhost";
            proxySettings.ProxyPort = 9000;
            proxySettings.ProxyUserName = ServiceTestSettings.WinQualUserName;
            proxySettings.ProxyPassword = ServiceTestSettings.WinQualPassword;

            m_Utils.SetProxy(proxySettings);

            m_Utils.RestartService();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);
            Assert.AreEqual(proxySettings.UseProxy, resp.Settings.ProxySettings.UseProxy);
            Assert.AreEqual(proxySettings.UseProxyAuthentication, resp.Settings.ProxySettings.UseProxyAuthentication);
            Assert.AreEqual(proxySettings.ProxyDomain, resp.Settings.ProxySettings.ProxyDomain);
            Assert.AreEqual(proxySettings.ProxyPort, resp.Settings.ProxySettings.ProxyPort);
            Assert.AreEqual(proxySettings.ProxyUserName, resp.Settings.ProxySettings.ProxyUserName);
            Assert.AreEqual(proxySettings.ProxyPassword, resp.Settings.ProxySettings.ProxyPassword);

            proxySettings.UseProxy = false;
            m_Utils.SetProxy(proxySettings);
        }
        [TestMethod]
        public void ProxySetWithReset()
        {
            proxySetWithReset(ErrorIndexType.Xml);
        }

        [TestMethod]
        public void ProxySetWithResetSql()
        {
            proxySetWithReset(ErrorIndexType.SqlExpress);
        }

        
        public void proxyNoProxy(ErrorIndexType indexType)
        {
            m_Utils.CreateAndSetNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);
            Assert.AreEqual(false, resp.Settings.ProxySettings.UseProxy);

            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = false;
            proxySettings.UseProxyAuthentication = false;
            proxySettings.ProxyDomain = null;
            proxySettings.ProxyHost = null;
            proxySettings.ProxyPort = 0;
            proxySettings.ProxyUserName = null;
            proxySettings.ProxyPassword = null;

            m_Utils.SetProxy(proxySettings);

            m_Utils.RestartService();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ProxySettings);
            Assert.AreEqual(proxySettings.UseProxy, resp.Settings.ProxySettings.UseProxy);
            Assert.AreEqual(proxySettings.UseProxyAuthentication, resp.Settings.ProxySettings.UseProxyAuthentication);
            Assert.AreEqual(proxySettings.ProxyDomain, resp.Settings.ProxySettings.ProxyDomain);
            Assert.AreEqual(proxySettings.ProxyPort, resp.Settings.ProxySettings.ProxyPort);
            Assert.AreEqual(proxySettings.ProxyUserName, resp.Settings.ProxySettings.ProxyUserName);
            Assert.AreEqual(proxySettings.ProxyPassword, resp.Settings.ProxySettings.ProxyPassword);

        }

        public void ProxyNoProxy()
        {
            proxyNoProxy(ErrorIndexType.Xml);
        }

        public void ProxyNoProxySql()
        {
            proxyNoProxy(ErrorIndexType.SqlExpress);
        }

        
        /// <summary>
        /// Get the default settings for the winqual sync.
        /// </summary>
        public void getWinQualSyncDefaults(ErrorIndexType indexType)
        {
            m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ContextCollection[0].WinQualSettings);

            WinQualSettings winQualSettings = resp.Settings.ContextCollection[0].WinQualSettings;

            Assert.AreEqual(180, winQualSettings.AgeOldToPurgeInDays);
            Assert.AreEqual("CompanyName", winQualSettings.CompanyName);
            Assert.AreEqual(30 * 60, winQualSettings.DelayBeforeRetryInSeconds);
            Assert.AreNotEqual(null, winQualSettings.ProductsToSynchronize);
            Assert.AreEqual(0, winQualSettings.ProductsToSynchronize.Count);
            Assert.AreEqual(5, winQualSettings.RequestRetryCount);
            Assert.AreEqual(5 * 60 * 1000, winQualSettings.RequestTimeout);
            Assert.AreEqual(true, winQualSettings.RetryAfterError);
            Assert.AreEqual("UserName", winQualSettings.UserName);
            Assert.AreEqual(5, winQualSettings.MaxCabDownloadFailuresBeforeAbort);
            Assert.AreEqual(7, winQualSettings.SyncsBeforeResync);
            Assert.AreEqual(false, winQualSettings.EnableNewProductsAutomatically);
        }

        /// <summary>
        /// Get the default settings for the winqual sync.
        /// </summary>
        [TestMethod]
        public void GetWinQualSyncDefaults()
        {
            getWinQualSyncDefaults(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Get the default settings for the winqual sync.
        /// </summary>
        [TestMethod]
        public void GetWinQualSyncDefaultsSql()
        {
            getWinQualSyncDefaults(ErrorIndexType.SqlExpress);
        }



        /// <summary>
        /// Get the default settings for the winqual sync and then set them.
        /// </summary>
        public void setWinQualSync(ErrorIndexType indexType)
        {
            m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ContextCollection[0].WinQualSettings);

            WinQualSettings winQualSettings = resp.Settings.ContextCollection[0].WinQualSettings;
            ErrorIndexSettings indexSettings = resp.Settings.ContextCollection[0].ErrorIndexSettings;

            Assert.AreEqual(180, winQualSettings.AgeOldToPurgeInDays);
            Assert.AreEqual("CompanyName", winQualSettings.CompanyName);
            Assert.AreEqual(30 * 60, winQualSettings.DelayBeforeRetryInSeconds);
            Assert.AreNotEqual(null, winQualSettings.ProductsToSynchronize);
            Assert.AreEqual(0, winQualSettings.ProductsToSynchronize.Count);
            Assert.AreEqual(5, winQualSettings.RequestRetryCount);
            Assert.AreEqual(5 * 60 * 1000, winQualSettings.RequestTimeout);
            Assert.AreEqual(true, winQualSettings.RetryAfterError);
            Assert.AreEqual("UserName", winQualSettings.UserName);

            Assert.AreEqual(ErrorIndexStatus.NotCreated, indexSettings.Status);
            Assert.AreEqual(StackHashErrorIndexLocation.Unknown, indexSettings.Location);

            winQualSettings.AgeOldToPurgeInDays = 179;
            winQualSettings.CompanyName = "Cucku";
            winQualSettings.DelayBeforeRetryInSeconds = 31 * 60;
            winQualSettings.ProductsToSynchronize.Add(new StackHashProductSyncData());
            winQualSettings.ProductsToSynchronize[0].ProductId = 10;

            winQualSettings.RequestRetryCount = 2;
            winQualSettings.RequestTimeout = 4 * 60 * 1000;
            winQualSettings.RetryAfterError = false;
            winQualSettings.MaxCabDownloadFailuresBeforeAbort = 6;
            winQualSettings.UserName = "UserName2";
            winQualSettings.Password = "Password2";
            winQualSettings.SyncsBeforeResync = 2;
            winQualSettings.EnableNewProductsAutomatically = true;

            indexSettings.Location = StackHashErrorIndexLocation.OnSqlServer;

            m_Utils.SetContextSettings(resp.Settings.ContextCollection[0]);


            GetStackHashPropertiesResponse resp2 = m_Utils.GetContextSettings();

            WinQualSettings winQualSettings2 = resp2.Settings.ContextCollection[0].WinQualSettings;

            Assert.AreEqual(winQualSettings2.AgeOldToPurgeInDays, winQualSettings.AgeOldToPurgeInDays);
            Assert.AreEqual(winQualSettings2.CompanyName, winQualSettings.CompanyName);
            Assert.AreEqual(winQualSettings2.DelayBeforeRetryInSeconds, winQualSettings.DelayBeforeRetryInSeconds);
            Assert.AreNotEqual(winQualSettings2.ProductsToSynchronize, winQualSettings.ProductsToSynchronize);
            Assert.AreEqual(winQualSettings2.ProductsToSynchronize.Count, winQualSettings.ProductsToSynchronize.Count);
            Assert.AreEqual(winQualSettings2.RequestRetryCount, winQualSettings.RequestRetryCount);
            Assert.AreEqual(winQualSettings2.RequestTimeout, winQualSettings.RequestTimeout);
            Assert.AreEqual(winQualSettings2.RetryAfterError, winQualSettings.RetryAfterError);
            Assert.AreEqual(winQualSettings2.UserName, winQualSettings.UserName);
            Assert.AreEqual(winQualSettings2.Password, winQualSettings.Password);
            Assert.AreEqual(winQualSettings2.MaxCabDownloadFailuresBeforeAbort, winQualSettings.MaxCabDownloadFailuresBeforeAbort);
            Assert.AreEqual(winQualSettings2.SyncsBeforeResync, winQualSettings.SyncsBeforeResync);
            Assert.AreEqual(winQualSettings2.EnableNewProductsAutomatically, winQualSettings.EnableNewProductsAutomatically);

            Assert.AreEqual(ErrorIndexStatus.NotCreated, indexSettings.Status);
            Assert.AreEqual(StackHashErrorIndexLocation.OnSqlServer, indexSettings.Location);
        }


        /// <summary>
        /// Get the default settings for the winqual sync and then set them.
        /// </summary>
        [TestMethod]
        public void SetWinQualSync()
        {
            setWinQualSync(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Get the default settings for the winqual sync and then set them.
        /// </summary>
        [TestMethod]
        public void SetWinQualSyncSql()
        {
            setWinQualSync(ErrorIndexType.SqlExpress);
        }

        

        /// <summary>
        /// Set WinQualSync settings with reset.
        /// </summary>
        public void setWinQualSyncWithReset(ErrorIndexType indexType)
        {
            m_Utils.CreateNewContext(indexType);

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            Assert.AreNotEqual(null, resp.Settings.ContextCollection[0].WinQualSettings);

            WinQualSettings winQualSettings = resp.Settings.ContextCollection[0].WinQualSettings;

            Assert.AreEqual(180, winQualSettings.AgeOldToPurgeInDays);
            Assert.AreEqual("CompanyName", winQualSettings.CompanyName);
            Assert.AreEqual(30 * 60, winQualSettings.DelayBeforeRetryInSeconds);
            Assert.AreNotEqual(null, winQualSettings.ProductsToSynchronize);
            Assert.AreEqual(0, winQualSettings.ProductsToSynchronize.Count);
            Assert.AreEqual(5, winQualSettings.RequestRetryCount);
            Assert.AreEqual(5 * 60 * 1000, winQualSettings.RequestTimeout);
            Assert.AreEqual(true, winQualSettings.RetryAfterError);
            Assert.AreEqual("UserName", winQualSettings.UserName);


            winQualSettings.AgeOldToPurgeInDays = 179;
            winQualSettings.CompanyName = "Cucku";
            winQualSettings.DelayBeforeRetryInSeconds = 31 * 60;
            winQualSettings.ProductsToSynchronize.Add(new StackHashProductSyncData());
            winQualSettings.ProductsToSynchronize[0].ProductId = 10;

            winQualSettings.RequestRetryCount = 2;
            winQualSettings.RequestTimeout = 4 * 60 * 1000;
            winQualSettings.RetryAfterError = false;
            winQualSettings.MaxCabDownloadFailuresBeforeAbort = 6;
            winQualSettings.UserName = "UserName2";
            winQualSettings.Password = "Password2";
            winQualSettings.SyncsBeforeResync = 2;


            m_Utils.SetContextSettings(resp.Settings.ContextCollection[0]);

            m_Utils.RestartService();

            GetStackHashPropertiesResponse resp2 = m_Utils.GetContextSettings();

            WinQualSettings winQualSettings2 = resp2.Settings.ContextCollection[0].WinQualSettings;

            Assert.AreEqual(winQualSettings2.AgeOldToPurgeInDays, winQualSettings.AgeOldToPurgeInDays);
            Assert.AreEqual(winQualSettings2.CompanyName, winQualSettings.CompanyName);
            Assert.AreEqual(winQualSettings2.DelayBeforeRetryInSeconds, winQualSettings.DelayBeforeRetryInSeconds);
            Assert.AreNotEqual(winQualSettings2.ProductsToSynchronize, winQualSettings.ProductsToSynchronize);
            Assert.AreEqual(winQualSettings2.ProductsToSynchronize.Count, winQualSettings.ProductsToSynchronize.Count);
            Assert.AreEqual(winQualSettings2.RequestRetryCount, winQualSettings.RequestRetryCount);
            Assert.AreEqual(winQualSettings2.RequestTimeout, winQualSettings.RequestTimeout);
            Assert.AreEqual(winQualSettings2.RetryAfterError, winQualSettings.RetryAfterError);
            Assert.AreEqual(winQualSettings2.UserName, winQualSettings.UserName);
            Assert.AreEqual(winQualSettings2.Password, winQualSettings.Password);
            Assert.AreEqual(winQualSettings2.MaxCabDownloadFailuresBeforeAbort, winQualSettings.MaxCabDownloadFailuresBeforeAbort);
            Assert.AreEqual(winQualSettings2.SyncsBeforeResync, winQualSettings.SyncsBeforeResync);
        }

        /// <summary>
        /// Set WinQualSync settings with reset.
        /// </summary>
        [TestMethod]
        public void SetWinQualSyncWithReset()
        {
            setWinQualSyncWithReset(ErrorIndexType.Xml);
        }

        /// <summary>
        /// Set WinQualSync settings with reset.
        /// </summary>
        [TestMethod]
        public void SetWinQualSyncWithResetSql()
        {
            setWinQualSyncWithReset(ErrorIndexType.SqlExpress);
        }

        /// <summary>
        /// Invalid database name.
        /// </summary>
        public void setInvalidDatabaseName(ErrorIndexType indexType)
        {
            try
            {
                m_Utils.CreateNewContext(indexType);

                GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

                Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
                Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

                resp.Settings.ContextCollection[0].ErrorIndexSettings.Name = "_ABC";

                m_Utils.SetContextSettings(resp.Settings.ContextCollection[0]);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.InvalidDatabaseName, ex.Detail.ServiceErrorCode);
                throw;
            }        

        }

        /// <summary>
        /// Set invalid database name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void SetInvalidDatabaseNameSql()
        {
            setInvalidDatabaseName(ErrorIndexType.SqlExpress);
        }


        /// <summary>
        /// Get the default Bug Tracker settings for a context.
        /// </summary>
        [TestMethod]
        public void GetBugTrackerSettingsDefault()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);
        }

        /// <summary>
        /// Set the Bug Tracker settings for a context.
        /// </summary>
        [TestMethod]
        public void SetBugTrackerSettings()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);

            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1"});
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            GetContextBugTrackerPlugInSettingsResponse resp2 = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(1, resp2.BugTrackerPlugInSettings.PlugInSettings.Count);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Enabled);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Name);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name);
            Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value);
        }

        /// <summary>
        /// Set the Bug Tracker settings for a context - with reset to show that the settings are persisted.
        /// </summary>
        [TestMethod]
        public void SetBugTrackerSettingsWithRestart()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);

            try
            {
                GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

                resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

                m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

                m_Utils.RestartService();

                GetContextBugTrackerPlugInSettingsResponse resp2 = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(1, resp2.BugTrackerPlugInSettings.PlugInSettings.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Enabled);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }

        /// <summary>
        /// Set the Bug Tracker settings for a context - with reset to show that the settings are persisted.
        /// </summary>
        [TestMethod]
        public void SetBugTrackerSettingsWithRestartAndActivate()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);
            
            GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
            Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
            Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

            resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugIn";
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
            resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

            m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

            m_Utils.RestartService();
            m_Utils.ActivateContext(0);

            try
            {
                GetContextBugTrackerPlugInSettingsResponse resp2 = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(1, resp2.BugTrackerPlugInSettings.PlugInSettings.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Enabled);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }


        /// <summary>
        /// Set the Bug Tracker settings for a context - with reset to show that the settings are persisted - Unknown plugin name.
        /// </summary>
        [TestMethod]
        public void SetBugTrackerSettingsWithRestartUnknownPlugIn()
        {
            m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);

            try
            {
                GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

                resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugInUnknown";
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

                m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

                m_Utils.RestartService();

                GetContextBugTrackerPlugInSettingsResponse resp2 = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(1, resp2.BugTrackerPlugInSettings.PlugInSettings.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Enabled);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }


        /// <summary>
        /// Set the Bug Tracker settings for a context - with reset to show that the settings are persisted - Unknown plugin name.
        /// Then activate the context.
        /// </summary>
        [TestMethod]
        public void SetBugTrackerSettingsWithRestartUnknownPlugInWithActivate()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            try
            {

                GetContextBugTrackerPlugInSettingsResponse resp = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(0, resp.BugTrackerPlugInSettings.PlugInSettings.Count);

                resp.BugTrackerPlugInSettings.PlugInSettings.Add(new StackHashBugTrackerPlugIn());
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled = true;
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Name = "TestPlugInUnknown";
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties = new StackHashNameValueCollection();
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam1", Value = "TestValue1" });
                resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Add(new StackHashNameValuePair() { Name = "TestParam2", Value = "TestValue2" });

                m_Utils.SetContextBugTrackerPlugInSettings(0, resp.BugTrackerPlugInSettings);

                m_Utils.RestartService();

                try
                {
                    m_Utils.ActivateContext(0);
                }
                catch (FaultException<ReceiverFaultDetail> ex)
                {
                    Assert.AreEqual(true, ex.Detail.Message.Contains("Unable to load plug-in: TestPlugInUnknown."));
                    Assert.AreEqual(StackHashServiceErrorCode.BugTrackerPlugInNotFoundOrCouldNotBeLoaded, ex.Detail.ServiceErrorCode);
                }

                GetContextBugTrackerPlugInSettingsResponse resp2 = m_Utils.GetContextBugTrackerPlugInSettings(0);

                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings);
                Assert.AreNotEqual(null, resp2.BugTrackerPlugInSettings.PlugInSettings);
                Assert.AreEqual(1, resp2.BugTrackerPlugInSettings.PlugInSettings.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Enabled, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Enabled);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties.Count);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[0].Value);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Name);
                Assert.AreEqual(resp.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value, resp2.BugTrackerPlugInSettings.PlugInSettings[0].Properties[1].Value);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }


        /// <summary>
        /// Default email settings.
        /// </summary>
        [TestMethod]
        public void GetDefaultEmailSettings()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            try
            {
                StackHashSettings settings = m_Utils.GetContextSettings().Settings;

                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings);
                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings.OperationsToReport);
                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings.SmtpSettings);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword);
                Assert.AreEqual(25, settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername);
            }

            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }

        /// <summary>
        /// Set email settings.
        /// </summary>
        [TestMethod]
        public void SetEmailSettings()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            try
            {
                StackHashSettings settings = m_Utils.GetContextSettings().Settings;

                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings);
                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings.OperationsToReport);
                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings.SmtpSettings);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword);
                Assert.AreEqual(25, settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername);


                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost = TestSettings.SmtpHost;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort = TestSettings.SmtpPort;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients = TestSettings.TestEmail2;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom = TestSettings.TestEmail1;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername = TestSettings.TestEmail1;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword = TestSettings.TestEmail1Password;
                settings.ContextCollection[0].EmailSettings.OperationsToReport = new StackHashAdminOperationCollection();
                settings.ContextCollection[0].EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncStarted);
                settings.ContextCollection[0].EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncCompleted);

                m_Utils.SetEmailSettings(0, settings.ContextCollection[0].EmailSettings);

                StackHashSettings settings2 = m_Utils.GetContextSettings().Settings;

                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername);

                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.OperationsToReport.Count, settings2.ContextCollection[0].EmailSettings.OperationsToReport.Count);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.OperationsToReport[0], settings2.ContextCollection[0].EmailSettings.OperationsToReport[0]);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.OperationsToReport[1], settings2.ContextCollection[0].EmailSettings.OperationsToReport[1]);
            }

            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }


        /// <summary>
        /// Set email settings with reset.
        /// </summary>
        [TestMethod]
        public void SetEmailSettingsWithReset()
        {
            m_Utils.CreateAndSetNewContext(ErrorIndexType.SqlExpress);

            try
            {
                StackHashSettings settings = m_Utils.GetContextSettings().Settings;

                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings);
                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings.OperationsToReport);
                Assert.AreNotEqual(null, settings.ContextCollection[0].EmailSettings.SmtpSettings);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword);
                Assert.AreEqual(25, settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients);
                Assert.AreEqual("", settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername);


                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost = TestSettings.SmtpHost;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort = TestSettings.SmtpPort;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients = TestSettings.TestEmail2;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom = TestSettings.TestEmail1;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername = TestSettings.TestEmail1;
                settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword = TestSettings.TestEmail1Password;
                settings.ContextCollection[0].EmailSettings.OperationsToReport = new StackHashAdminOperationCollection();
                settings.ContextCollection[0].EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncStarted);
                settings.ContextCollection[0].EmailSettings.OperationsToReport.Add(StackHashAdminOperation.WinQualSyncCompleted);

                m_Utils.SetEmailSettings(0, settings.ContextCollection[0].EmailSettings);


                // Restart the service to make sure the settings were persisted.
                m_Utils.RestartService();


                StackHashSettings settings2 = m_Utils.GetContextSettings().Settings;

                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpFrom);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpHost);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPassword);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpPort);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpRecipients);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername, settings2.ContextCollection[0].EmailSettings.SmtpSettings.SmtpUsername);

                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.OperationsToReport.Count, settings2.ContextCollection[0].EmailSettings.OperationsToReport.Count);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.OperationsToReport[0], settings2.ContextCollection[0].EmailSettings.OperationsToReport[0]);
                Assert.AreEqual(settings.ContextCollection[0].EmailSettings.OperationsToReport[1], settings2.ContextCollection[0].EmailSettings.OperationsToReport[1]);
            }

            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
                m_Utils.RemoveAllContexts();
            }
        }
    }
}
