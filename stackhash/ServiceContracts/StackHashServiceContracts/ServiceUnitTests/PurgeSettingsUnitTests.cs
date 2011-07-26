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
    public class PurgeSettingsUnitTests
    {
        Utils m_Utils;

        public PurgeSettingsUnitTests()
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


        private void checkDefaultPurgeOptions(StackHashPurgeOptions purgeOptions)
        {
            Assert.AreEqual(StackHashPurgeObject.PurgeGlobal, purgeOptions.PurgeObject);
            Assert.AreEqual(0, purgeOptions.Id);
            Assert.AreEqual(180, purgeOptions.AgeToPurge);
            Assert.AreEqual(true, purgeOptions.PurgeCabFiles);
            Assert.AreEqual(true, purgeOptions.PurgeDumpFiles);
        }

        private void checkDefaultPurgeSchedule(ScheduleCollection purgeSchedule)
        {
            Assert.AreNotEqual(null, purgeSchedule);
            Assert.AreEqual(1, purgeSchedule.Count);
            Assert.AreEqual(SchedulePeriod.Weekly, purgeSchedule[0].Period);
            Assert.AreEqual(DaysOfWeek.Sunday, purgeSchedule[0].DaysOfWeek);
            Assert.AreEqual(23, purgeSchedule[0].Time.Hour);
            Assert.AreEqual(0, purgeSchedule[0].Time.Minute);
        }


        [TestMethod]
        public void GetDefaultSettings()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreNotEqual(null, resp.Settings.ContextCollection[0].PurgeOptionsCollection);
            Assert.AreEqual(1, resp.Settings.ContextCollection[0].PurgeOptionsCollection.Count);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
        }


        [TestMethod]
        public void GetPurgeOptionsGetAll()
        {
            m_Utils.CreateAndSetNewContext();

            GetPurgeOptionsResponse resp = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);

            Assert.AreNotEqual(null, resp.PurgeOptions);
            Assert.AreEqual(1, resp.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp.PurgeOptions[0]);
        }

        [TestMethod]
        public void GetPurgeOptionsGlobal()
        {
            m_Utils.CreateAndSetNewContext();

            GetPurgeOptionsResponse resp = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, false);

            Assert.AreNotEqual(null, resp.PurgeOptions);
            Assert.AreEqual(1, resp.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp.PurgeOptions[0]);
        }

        [TestMethod]
        public void GetPurgeOptionsDoesntExist()
        {
            m_Utils.CreateAndSetNewContext();

            GetPurgeOptionsResponse resp = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeFile, 1, false);

            Assert.AreNotEqual(null, resp.PurgeOptions);
            Assert.AreEqual(0, resp.PurgeOptions.Count);
        }

        [TestMethod]
        public void SetAll()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            
            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
            purgeOptions[0].Id = 0;
            purgeOptions[0].AgeToPurge = 10;
            purgeOptions[0].PurgeCabFiles = true;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 1;
            purgeOptions[1].AgeToPurge = 2000;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, true);

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            int index = 0;
            foreach (StackHashPurgeOptions options in resp.Settings.ContextCollection[0].PurgeOptionsCollection)
            {
                Assert.AreEqual(purgeOptions[index].PurgeObject, options.PurgeObject);
                Assert.AreEqual(purgeOptions[index].Id, options.Id);
                Assert.AreEqual(purgeOptions[index].AgeToPurge, options.AgeToPurge);
                Assert.AreEqual(purgeOptions[index].PurgeCabFiles, options.PurgeCabFiles);
                Assert.AreEqual(purgeOptions[index].PurgeDumpFiles, options.PurgeDumpFiles);
                index++;
            }
        }

        [TestMethod]
        public void SetAllWithReset()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
            purgeOptions[0].Id = 0;
            purgeOptions[0].AgeToPurge = 10;
            purgeOptions[0].PurgeCabFiles = true;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 1;
            purgeOptions[1].AgeToPurge = 2000;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, true);

            m_Utils.RestartService();

            resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);

            int index = 0;
            foreach (StackHashPurgeOptions options in resp.Settings.ContextCollection[0].PurgeOptionsCollection)
            {
                Assert.AreEqual(purgeOptions[index].PurgeObject, options.PurgeObject);
                Assert.AreEqual(purgeOptions[index].Id, options.Id);
                Assert.AreEqual(purgeOptions[index].AgeToPurge, options.AgeToPurge);
                Assert.AreEqual(purgeOptions[index].PurgeCabFiles, options.PurgeCabFiles);
                Assert.AreEqual(purgeOptions[index].PurgeDumpFiles, options.PurgeDumpFiles);
                index++;
            }
        }

        private void comparePurgeOptions(StackHashPurgeOptions options1, StackHashPurgeOptions options2)
        {
            Assert.AreEqual(options1.PurgeObject, options2.PurgeObject);
            Assert.AreEqual(options1.Id, options2.Id);
            Assert.AreEqual(options1.AgeToPurge, options2.AgeToPurge);
            Assert.AreEqual(options1.PurgeCabFiles, options2.PurgeCabFiles);
            Assert.AreEqual(options1.PurgeDumpFiles, options2.PurgeDumpFiles);
        }

        /// <summary>
        /// Add a new specific purge option.
        /// Should end up with 2.
        /// </summary>
        [TestMethod]
        public void AddOneNew()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);

            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);

            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);
        }

        /// <summary>
        /// Add a new specific purge option - then try to get it with wrong ID - should not return.
        /// </summary>
        [TestMethod]
        public void AddOneGetWrongId()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeEvent, 0, false);

            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(0, resp2.PurgeOptions.Count);
        }

        /// <summary>
        /// Add a new specific purge option with reset.
        /// Should end up with 2.
        /// </summary>
        [TestMethod]
        public void AddOneNewWithReset()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            m_Utils.RestartService();

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);

            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);

            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);
        }

        /// <summary>
        /// Add a new specific purge option.
        /// Should end up with 2.
        /// </summary>
        [TestMethod]
        public void AddTwoNewAtSameTime()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[1].Id = 3;
            purgeOptions[1].AgeToPurge = 3000;
            purgeOptions[1].PurgeCabFiles = true;
            purgeOptions[1].PurgeDumpFiles = false;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);

            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(3, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);
            comparePurgeOptions(purgeOptions[1], resp2.PurgeOptions[2]);


            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeEvent, 1, false);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(1, resp3.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], resp3.PurgeOptions[0]);

            GetPurgeOptionsResponse resp4 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeFile, 3, false);
            Assert.AreNotEqual(null, resp4.PurgeOptions);
            Assert.AreEqual(1, resp4.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[1], resp4.PurgeOptions[0]);

            GetPurgeOptionsResponse resp5 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, false);
            Assert.AreNotEqual(null, resp5.PurgeOptions);
            Assert.AreEqual(1, resp5.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp5.PurgeOptions[0]);

        }

        /// <summary>
        /// Add the same purge option twice - should replace.
        /// Should only add 1.
        /// </summary>
        [TestMethod]
        public void AddSameOptionTwice()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);
            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);

            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);

            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeEvent, 1, false);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(1, resp3.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], resp3.PurgeOptions[0]);

            GetPurgeOptionsResponse resp4 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, false);
            Assert.AreNotEqual(null, resp4.PurgeOptions);
            Assert.AreEqual(1, resp4.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp4.PurgeOptions[0]);
        }

        
        /// <summary>
        /// Add the same purge option twice - should replace.
        /// Should only add 1.
        /// </summary>
        [TestMethod]
        public void AddSameOptionTwiceAtTheSameTime()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 1;
            purgeOptions[1].AgeToPurge = 2000;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);

            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);

            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeEvent, 1, false);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(1, resp3.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], resp3.PurgeOptions[0]);

            GetPurgeOptionsResponse resp4 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, false);
            Assert.AreNotEqual(null, resp4.PurgeOptions);
            Assert.AreEqual(1, resp4.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp4.PurgeOptions[0]);
        }

        
        /// <summary>
        /// Start off with global.
        /// Add Event.
        /// Remove global
        /// Should end up with just the Event.
        /// </summary>
        [TestMethod]
        public void AddRemoveFirstOfTwo()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);

            m_Utils.RemovePurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0);

            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(1, resp3.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], resp3.PurgeOptions[0]);
        }


        /// <summary>
        /// Start off with global.
        /// Add Event.
        /// Remove Event
        /// Should end up with just the Global.
        /// </summary>
        [TestMethod]
        public void AddRemoveSecondOfTwo()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);

            m_Utils.RemovePurgeOptions(0, StackHashPurgeObject.PurgeEvent, 1);

            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(1, resp3.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp3.PurgeOptions[0]);
        }

        /// <summary>
        /// Add remove with reset.
        /// </summary>
        [TestMethod]
        public void AddRemoveWithReset()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            m_Utils.RestartService();

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);

            m_Utils.RemovePurgeOptions(0, StackHashPurgeObject.PurgeEvent, 1);

            m_Utils.RestartService();

            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(1, resp3.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp3.PurgeOptions[0]);
        }

        
        /// <summary>
        /// Start off with global.
        /// Add Event.
        /// Remove Event wrong ID
        /// Should end up with Event and Global.
        /// </summary>
        [TestMethod]
        public void AddRemoveOneWrongId()
        {
            m_Utils.CreateAndSetNewContext();

            GetStackHashPropertiesResponse resp = m_Utils.GetContextSettings();

            Assert.AreEqual(1, resp.Settings.ContextCollection.Count);
            Assert.AreEqual(false, resp.Settings.ContextCollection[0].IsActive);
            checkDefaultPurgeSchedule(resp.Settings.ContextCollection[0].CabFilePurgeSchedule);
            checkDefaultPurgeOptions(resp.Settings.ContextCollection[0].PurgeOptionsCollection[0]);

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, resp.Settings.ContextCollection[0].CabFilePurgeSchedule, purgeOptions, false);

            GetPurgeOptionsResponse resp2 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp2.PurgeOptions);
            Assert.AreEqual(2, resp2.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp2.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp2.PurgeOptions[1]);

            m_Utils.RemovePurgeOptions(0, StackHashPurgeObject.PurgeEvent, 2);

            GetPurgeOptionsResponse resp3 = m_Utils.GetPurgeOptions(0, StackHashPurgeObject.PurgeGlobal, 0, true);
            Assert.AreNotEqual(null, resp3.PurgeOptions);
            Assert.AreEqual(2, resp3.PurgeOptions.Count);
            checkDefaultPurgeOptions(resp3.PurgeOptions[0]);
            comparePurgeOptions(purgeOptions[0], resp3.PurgeOptions[1]);
        }

        /// <summary>
        /// Default active settings
        /// Global will be the default for any object.
        /// </summary>
        [TestMethod]
        public void GetActiveDefault()
        {
            m_Utils.CreateAndSetNewContext();

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            checkDefaultPurgeOptions(activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Product Id match.
        /// </summary>
        [TestMethod]
        public void GetActiveProductIdMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Product Id no match - should use global.
        /// </summary>
        [TestMethod]
        public void GetActiveProductIdNoMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 2, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            checkDefaultPurgeOptions(activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// File Id match.
        /// </summary>
        [TestMethod]
        public void GetActiveFileIdMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[0].Id = 2;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// File Id no match - should use global.
        /// </summary>
        [TestMethod]
        public void GetActiveFileIdNoMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[0].Id = 2;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 3, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            checkDefaultPurgeOptions(activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Event Id match.
        /// </summary>
        [TestMethod]
        public void GetActiveEventIdMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 3;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Event Id match - on second of two events.
        /// </summary>
        [TestMethod]
        public void GetActive2EventsFirstIdMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 3;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 4;
            purgeOptions[1].AgeToPurge = 2000;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Event Id match - on second of two events.
        /// </summary>
        [TestMethod]
        public void GetActive2EventsSecondIdMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 3;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 4;
            purgeOptions[1].AgeToPurge = 2000;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 4, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[1], activeSettings.PurgeOptions[0]);
        }
        
        /// <summary>
        /// Event Id no match - should use global.
        /// </summary>
        [TestMethod]
        public void GetActiveEventIdNoMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[0].Id = 3;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 4, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            checkDefaultPurgeOptions(activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Cab Id match.
        /// </summary>
        [TestMethod]
        public void GetActiveCabIdMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[0].Id = 4;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Cab Id no match - should use global.
        /// </summary>
        [TestMethod]
        public void GetActiveCabIdNoMatch()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[0].Id = 4;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            checkDefaultPurgeOptions(activeSettings.PurgeOptions[0]);
        }

        
        /// <summary>
        /// Cab overrides event.
        /// </summary>
        [TestMethod]
        public void GetActiveCabOverridesEvent()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[0].Id = 4;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 3;
            purgeOptions[1].AgeToPurge = 2001;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Cab ignored, event used.
        /// </summary>
        [TestMethod]
        public void GetActiveCabIgnoreUseEvent()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[0].Id = 4;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[1].Id = 3;
            purgeOptions[1].AgeToPurge = 2001;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[1], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Cab overrides product.
        /// </summary>
        [TestMethod]
        public void GetActiveCabOverridesProduct()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[0].Id = 4;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[1].Id = 1;
            purgeOptions[1].AgeToPurge = 2001;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 4);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }


        /// <summary>
        /// Cab ignored product used.
        /// </summary>
        [TestMethod]
        public void GetActiveCabIgnoredUseProduct()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[0].Id = 4;
            purgeOptions[0].AgeToPurge = 2000;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[1].Id = 1;
            purgeOptions[1].AgeToPurge = 2001;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[1], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Global, Product, File, Event and Cab - event match.
        /// </summary>
        [TestMethod]
        public void FiveOptionsSelectCab()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2001;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[1].Id = 2;
            purgeOptions[1].AgeToPurge = 2002;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[2].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[2].Id = 3;
            purgeOptions[2].AgeToPurge = 2003;
            purgeOptions[2].PurgeCabFiles = false;
            purgeOptions[2].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[3].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[3].Id = 4;
            purgeOptions[3].AgeToPurge = 2004;
            purgeOptions[3].PurgeCabFiles = false;
            purgeOptions[3].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[2], activeSettings.PurgeOptions[0]);
        }


        /// <summary>
        /// Global, Product, File, Event and Cab - event match.
        /// </summary>
        [TestMethod]
        public void FiveOptionsSelectEvent()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2001;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[1].Id = 2;
            purgeOptions[1].AgeToPurge = 2002;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[2].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[2].Id = 3;
            purgeOptions[2].AgeToPurge = 2003;
            purgeOptions[2].PurgeCabFiles = false;
            purgeOptions[2].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[3].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[3].Id = 4;
            purgeOptions[3].AgeToPurge = 2004;
            purgeOptions[3].PurgeCabFiles = false;
            purgeOptions[3].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 3, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[2], activeSettings.PurgeOptions[0]);
        }

        /// <summary>
        /// Global, Product, File, Event and Cab - file match.
        /// </summary>
        [TestMethod]
        public void FiveOptionsSelectFile()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2001;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[1].Id = 2;
            purgeOptions[1].AgeToPurge = 2002;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[2].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[2].Id = 3;
            purgeOptions[2].AgeToPurge = 2003;
            purgeOptions[2].PurgeCabFiles = false;
            purgeOptions[2].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[3].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[3].Id = 4;
            purgeOptions[3].AgeToPurge = 2004;
            purgeOptions[3].PurgeCabFiles = false;
            purgeOptions[3].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 2, 4, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[1], activeSettings.PurgeOptions[0]);
        }


        /// <summary>
        /// Global, Product, File, Event and Cab - product match.
        /// </summary>
        [TestMethod]
        public void FiveOptionsSelectProduct()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2001;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[1].Id = 2;
            purgeOptions[1].AgeToPurge = 2002;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[2].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[2].Id = 3;
            purgeOptions[2].AgeToPurge = 2003;
            purgeOptions[2].PurgeCabFiles = false;
            purgeOptions[2].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[3].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[3].Id = 4;
            purgeOptions[3].AgeToPurge = 2004;
            purgeOptions[3].PurgeCabFiles = false;
            purgeOptions[3].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 1, 9, 4, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            comparePurgeOptions(purgeOptions[0], activeSettings.PurgeOptions[0]);
        }


        /// <summary>
        /// Global, Product, File, Event and Cab - global.
        /// </summary>
        [TestMethod]
        public void FiveOptionsSelectGlobal()
        {
            m_Utils.CreateAndSetNewContext();

            StackHashPurgeOptionsCollection purgeOptions = new StackHashPurgeOptionsCollection();
            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[0].PurgeObject = StackHashPurgeObject.PurgeProduct;
            purgeOptions[0].Id = 1;
            purgeOptions[0].AgeToPurge = 2001;
            purgeOptions[0].PurgeCabFiles = false;
            purgeOptions[0].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[1].PurgeObject = StackHashPurgeObject.PurgeFile;
            purgeOptions[1].Id = 2;
            purgeOptions[1].AgeToPurge = 2002;
            purgeOptions[1].PurgeCabFiles = false;
            purgeOptions[1].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[2].PurgeObject = StackHashPurgeObject.PurgeEvent;
            purgeOptions[2].Id = 3;
            purgeOptions[2].AgeToPurge = 2003;
            purgeOptions[2].PurgeCabFiles = false;
            purgeOptions[2].PurgeDumpFiles = true;

            purgeOptions.Add(new StackHashPurgeOptions());
            purgeOptions[3].PurgeObject = StackHashPurgeObject.PurgeCab;
            purgeOptions[3].Id = 4;
            purgeOptions[3].AgeToPurge = 2004;
            purgeOptions[3].PurgeCabFiles = false;
            purgeOptions[3].PurgeDumpFiles = true;

            m_Utils.SetPurgeOptions(0, null, purgeOptions, false);

            GetActivePurgeOptionsResponse activeSettings = m_Utils.GetActivePurgeOptions(0, 3, 9, 4, 5);

            Assert.AreNotEqual(null, activeSettings.PurgeOptions);
            Assert.AreEqual(1, activeSettings.PurgeOptions.Count);
            checkDefaultPurgeOptions(activeSettings.PurgeOptions[0]);
        }
    }
}