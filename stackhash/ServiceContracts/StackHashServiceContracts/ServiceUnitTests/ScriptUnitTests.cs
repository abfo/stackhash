using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using ServiceUnitTests.StackHashServices;
using System.ServiceModel;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for ScriptUnitTests
    /// </summary>
    [TestClass]
    public class ScriptUnitTests
    {
        Utils m_Utils;
        static int s_NumAutoScripts = 2;

        public ScriptUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_Utils = new Utils();

            m_Utils.RemoveAllContexts();
            m_Utils.RemoveAllScripts();
            m_Utils.RestartService();
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
        public void GetScriptListNoScripts()
        {
            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // Autoscript expected.
            Assert.AreEqual(s_NumAutoScripts, resp.ScriptFileData.Count);
            Assert.AreEqual(true, resp.ScriptFileData[0].IsReadOnly);
            Assert.AreEqual("AutoScript", resp.ScriptFileData[0].Name);
        }

        [TestMethod]
        public void AddOneScript()
        {
            StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(0, true);

            m_Utils.AddDebuggerScript(scriptSettings, false);
                        
            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(1 + s_NumAutoScripts, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings.Name));

            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings.Name);

            m_Utils.CheckScriptSettings(scriptSettings, getScriptResp.ScriptSettings, true);
        }

        [TestMethod]
        public void AddOneScriptGetNames()
        {
            StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(0, true, StackHashScriptDumpType.ManagedOnly, true);

            m_Utils.AddDebuggerScript(scriptSettings, false);

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(1 + s_NumAutoScripts, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings.Name));

            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings.Name);

            m_Utils.CheckScriptSettings(scriptSettings, getScriptResp.ScriptSettings, true);

            StackHashScriptFileDataCollection scriptFiles = m_Utils.GetDebuggerScriptNames().ScriptFileData;

            bool found = false;
            foreach (StackHashScriptFileData fileData in scriptFiles)
            {
                if (fileData.Name == scriptSettings.Name)
                {
                    Assert.AreEqual(getScriptResp.ScriptSettings.IsReadOnly, fileData.IsReadOnly);
                    Assert.AreEqual(getScriptResp.ScriptSettings.LastModifiedDate, fileData.LastModifiedDate);
                    Assert.AreEqual(getScriptResp.ScriptSettings.RunAutomatically, fileData.RunAutomatically);
                    Assert.AreEqual(getScriptResp.ScriptSettings.DumpType, fileData.DumpType);
                    Assert.AreEqual(getScriptResp.ScriptSettings.CreationDate, fileData.CreationDate);
                    Assert.AreEqual(getScriptResp.ScriptSettings.LastModifiedDate, fileData.LastModifiedDate);
                    found = true;
                }
            }

            Assert.AreEqual(true, found);
        }

        [TestMethod]
        public void AddOneScriptWithRestart()
        {
            StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(0, true);

            m_Utils.AddDebuggerScript(scriptSettings, false);

            m_Utils.RestartService();

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(1 + s_NumAutoScripts, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings.Name));

            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings.Name);

            m_Utils.CheckScriptSettings(scriptSettings, getScriptResp.ScriptSettings, true);
        }

        private bool containsName(StackHashScriptFileDataCollection scriptNames, String name)
        {
            foreach (StackHashScriptFileData scriptFileData in scriptNames)
            {
                if (scriptFileData.Name == name)
                    return true;
                if ((scriptFileData.Name == "AutoScript") ||
                    (scriptFileData.Name == "GetOSVersion"))
                    Assert.AreEqual(true, scriptFileData.IsReadOnly);
                else
                    Assert.AreEqual(false, scriptFileData.IsReadOnly);
            }

            return false;
        }

        [TestMethod]
        public void AddRemoveScript()
        {
            StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(0, true);

            m_Utils.AddDebuggerScript(scriptSettings, false);

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(1 + s_NumAutoScripts, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings.Name));

            m_Utils.RemoveDebuggerScript(scriptSettings.Name);

            resp = m_Utils.GetDebuggerScriptNames();

            // Just autoscript left.
            Assert.AreEqual(s_NumAutoScripts, resp.ScriptFileData.Count);
        }

        [TestMethod]
        public void AddRemoveScriptNoComments()
        {
            StackHashScriptSettings scriptSettings = m_Utils.MakeScriptSettings(0, false);

            m_Utils.AddDebuggerScript(scriptSettings, false);

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(s_NumAutoScripts + 1, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings.Name));

            m_Utils.RemoveDebuggerScript(scriptSettings.Name);

            resp = m_Utils.GetDebuggerScriptNames();

            // Just autoscript left.
            Assert.AreEqual(s_NumAutoScripts, resp.ScriptFileData.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void GetScriptDoesntExist()
        {
            try
            {
                // Get the actual script back and compare.
                GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript("NoExistentScriptName");
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Script file does not exist"));
                throw;
            }
        }

        [TestMethod]
        public void Add2Scripts()
        {
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
            StackHashScriptSettings scriptSettings2 = m_Utils.MakeScriptSettings(1, true);

            m_Utils.AddDebuggerScript(scriptSettings1, false);
            m_Utils.AddDebuggerScript(scriptSettings2, false);

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 3 including the autoscript.
            Assert.AreEqual(2 + s_NumAutoScripts, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings1.Name));
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings2.Name));


            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp1 = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            GetDebuggerScriptResponse getScriptResp2 = m_Utils.GetDebuggerScript(scriptSettings2.Name);

            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp1.ScriptSettings, true);
            m_Utils.CheckScriptSettings(scriptSettings2, getScriptResp2.ScriptSettings, true);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void OverwriteScriptNoOverwrite()
        {
            try
            {
                StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
                StackHashScriptSettings scriptSettings2 = m_Utils.MakeScriptSettings(1, true);
                scriptSettings2.Name = scriptSettings1.Name;

                m_Utils.AddDebuggerScript(scriptSettings1, false);
                m_Utils.AddDebuggerScript(scriptSettings2, false);

                GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

                Assert.AreEqual(1, resp.ScriptFileData.Count);
                Assert.AreEqual(scriptSettings1.Name, resp.ScriptFileData[0]);


                // Get the actual script back and compare.
                GetDebuggerScriptResponse getScriptResp1 = m_Utils.GetDebuggerScript(scriptSettings1.Name);
                m_Utils.CheckScriptSettings(scriptSettings2, getScriptResp1.ScriptSettings, true);
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(true, ex.Message.Contains("Script file already exists"));
                throw;
            }

        }

        [TestMethod]
        public void OverwriteScriptWithOverwrite()
        {
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
            StackHashScriptSettings scriptSettings2 = m_Utils.MakeScriptSettings(1, true);
            scriptSettings2.Name = scriptSettings1.Name;

            m_Utils.AddDebuggerScript(scriptSettings1, false);
            Thread.Sleep(100);
            m_Utils.AddDebuggerScript(scriptSettings2, true);

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including autoscript.
            Assert.AreEqual(s_NumAutoScripts + 1, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, scriptSettings1.Name));


            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp1 = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            m_Utils.CheckScriptSettings(scriptSettings2, getScriptResp1.ScriptSettings, false);


            Assert.AreEqual(true, scriptSettings1.CreationDate <= getScriptResp1.ScriptSettings.CreationDate);
            Assert.AreEqual(true, scriptSettings2.LastModifiedDate <= getScriptResp1.ScriptSettings.LastModifiedDate);
            Assert.AreEqual(true, getScriptResp1.ScriptSettings.LastModifiedDate != getScriptResp1.ScriptSettings.CreationDate);
        }
        [TestMethod]
        public void RenameScript()
        {
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);

            m_Utils.AddDebuggerScript(scriptSettings1, false);

            GetDebuggerScriptResponse getScriptResp1 = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp1.ScriptSettings, false);

            string newScriptName = "NewScriptName";
            Thread.Sleep(100);
            m_Utils.RenameDebuggerScript(scriptSettings1.Name, newScriptName);

            GetDebuggerScriptNamesResponse resp = m_Utils.GetDebuggerScriptNames();

            // 2 including autoscript.
            Assert.AreEqual(s_NumAutoScripts + 1, resp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(resp.ScriptFileData, newScriptName));

            // Get the actual script back and compare.
            scriptSettings1.Name = newScriptName;

            GetDebuggerScriptResponse getScriptResp2 = m_Utils.GetDebuggerScript(newScriptName);
            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp2.ScriptSettings, false);


            Assert.AreEqual(getScriptResp1.ScriptSettings.CreationDate, getScriptResp2.ScriptSettings.CreationDate);
            Assert.AreEqual(true, getScriptResp1.ScriptSettings.LastModifiedDate < getScriptResp2.ScriptSettings.LastModifiedDate);
        }


        private StackHashEventPackage findCab(out StackHashProduct outProduct, out StackHashFile outFile, out StackHashEvent outEvent)
        {
            outProduct = null;
            outFile = null;
            outEvent = null;

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);


            foreach (StackHashProductInfo productInfo in getProductsResp.Products)
            {
                StackHashProduct product = productInfo.Product;

                GetFilesResponse getFilesResp = m_Utils.GetFiles(0, product);

                foreach (StackHashFile file in getFilesResp.Files)
                {
                    GetEventsResponse getEventsResp = m_Utils.GetEvents(0, product, file);

                    foreach (StackHashEvent theEvent in getEventsResp.Events)
                    {
                        // Get the cab list for the event.
                        GetEventPackageResponse getEventPackageResponse = m_Utils.GetEventPackage(0, product, file, theEvent);

                        // Check if there is a cab.
                        if (getEventPackageResponse.EventPackage.Cabs.Count > 0)
                        {
                            outProduct = product;
                            outFile = file;
                            outEvent = theEvent;

                            return getEventPackageResponse.EventPackage;
                        }
                    }
                }
            }

            return null;
        }


        [TestMethod]
        public void RunScript()
        {
            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;

            m_Utils.CreateTestIndex(0, testIndexData);

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(1, getProductsResp.Products.Count());
            Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
            Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

            // Add a script.
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
            m_Utils.AddDebuggerScript(scriptSettings1, false);

            GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(s_NumAutoScripts + 1, getScriptNamesResp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));


            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Recorded time is only accurate to the second.
            long ticksInASecond = 10000000;

            DateTime now = DateTime.Now.ToUniversalTime();
            now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

            // Run the script on the first cab listed in the event package.
            m_Utils.RunDebuggerScript(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);


            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1, getDebugResultFilesResp.ResultFiles.Count);
            Assert.AreEqual(scriptSettings1.Name, getDebugResultFilesResp.ResultFiles[0].ScriptName);
            Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);
            
            Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        [TestMethod]
        public void RunScriptAsync()
        {
            DateTime startTime = DateTime.Now.ToUniversalTime();

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext(ErrorIndexType.SqlExpress);

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                // Create a test index with one cab file.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 1;
                testIndexData.NumberOfEventInfos = 1;
                testIndexData.NumberOfCabs = 1;

                m_Utils.CreateTestIndex(0, testIndexData);

                // Service is now started with the specified index.
                // Make sure we can get at least the list of products.
                GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

                Assert.AreEqual(1, getProductsResp.Products.Count());
                Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
                Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

                // Add a script.
                StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
                m_Utils.AddDebuggerScript(scriptSettings1, false);

                GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

                // 2 including the autoscript.
                Assert.AreEqual(s_NumAutoScripts + 1, getScriptNamesResp.ScriptFileData.Count);
                Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));


                // Get the actual script back and compare.
                GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
                m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);

                // Find a cab to run a script on.
                StackHashProduct product;
                StackHashFile file;
                StackHashEvent theEvent;
                StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


                // Recorded time is only accurate to the second.
                long ticksInASecond = 10000000;

                DateTime now = DateTime.Now.ToUniversalTime();
                now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

                StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
                scriptNames.Add(scriptSettings1.Name);

                // Run the script on the first cab listed in the event package.
                m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0].Cab, 60000);

                // Get a list of the debug results files.
                GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

                Assert.AreEqual(1 + s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

                foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
                {
                    if (scriptSettings1.Name == scriptResult.ScriptName)
                        Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);
                }

                // Get the full result back.
                GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

                Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);

                Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
                Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

                // Get the cab notes. Should have added a Script Run message.
                GetCabNotesResponse cabNotes = m_Utils.GetCabNotes(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

                String expectedNoteText = String.Format("Script {0} executed", scriptSettings1.Name);

                bool foundNoteEntry = false;
                foreach (StackHashNoteEntry note in cabNotes.Notes)
                {
                    if (note.Note.Contains(expectedNoteText))
                    {
                        foundNoteEntry = true;
                        Assert.AreEqual(true, startTime < note.TimeOfEntry);
                        Assert.AreEqual(Environment.UserName.ToUpperInvariant(), note.User);
                        Assert.AreEqual("StackHash client".ToUpperInvariant(), note.Source);
                        Assert.AreNotEqual(0, note.NoteId);
                    }
                }
                Assert.AreEqual(true, foundNoteEntry);


                // Now get the cab details - should have filled in the diagnostics.
                GetEventPackageResponse eventPackageResp = m_Utils.GetEventPackage(0, product, file, theEvent);

                StackHashCab thisCab = eventPackageResp.EventPackage.Cabs[0].Cab;
                Assert.AreNotEqual(null, thisCab.DumpAnalysis);
                Assert.AreEqual("2.0.50727.3603", thisCab.DumpAnalysis.DotNetVersion);
                Assert.AreEqual("x86", thisCab.DumpAnalysis.MachineArchitecture);
                Assert.AreEqual("Windows XP Version 2600 (Service Pack 3) MP (2 procs) Free x86 compatible", thisCab.DumpAnalysis.OSVersion);
                Assert.AreEqual("0 days 0:00:15.000", thisCab.DumpAnalysis.ProcessUpTime);
                Assert.AreEqual("not available", thisCab.DumpAnalysis.SystemUpTime);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }
        [TestMethod]
        public void RunScriptAsyncCabWithMultipleDumps()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.CabFileName = "496858888-5-0615344527.cab"; // Contains 2 dumps.

            m_Utils.CreateTestIndex(0, testIndexData);

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(1, getProductsResp.Products.Count());
            Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
            Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

            // Add a script.
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
            m_Utils.AddDebuggerScript(scriptSettings1, false);

            GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(s_NumAutoScripts + 1, getScriptNamesResp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));


            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Recorded time is only accurate to the second.
            long ticksInASecond = 10000000;

            DateTime now = DateTime.Now.ToUniversalTime();
            now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

            StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
            scriptNames.Add(scriptSettings1.Name);

            // Run the script on the first cab listed in the event package.
            m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0].Cab, 60000);

            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1 + s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);
            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
            {
                if (scriptSettings1.Name == scriptResult.ScriptName)
                    Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);
            }

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);

            Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }
        [TestMethod]
        public void RunScriptAsyncNoCommandComment()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;

            m_Utils.CreateTestIndex(0, testIndexData);

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(1, getProductsResp.Products.Count());
            Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
            Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

            // Add a script.
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, false);
            m_Utils.AddDebuggerScript(scriptSettings1, false);

            GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

            // 2 including the autoscript.
            Assert.AreEqual(1 + s_NumAutoScripts, getScriptNamesResp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));


            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Recorded time is only accurate to the second.
            long ticksInASecond = 10000000;

            DateTime now = DateTime.Now.ToUniversalTime();
            now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

            StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
            scriptNames.Add(scriptSettings1.Name);

            // Run the script on the first cab listed in the event package.
            m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0].Cab, 60000);

            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1 + s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
            {
                if (scriptResult.ScriptName == scriptSettings1.Name)
                {
                    Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);
                }
            }

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);

            Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }
        [TestMethod]
        public void RunScriptAsyncInvalidScriptFile()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;

            m_Utils.CreateTestIndex(0, testIndexData);

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(1, getProductsResp.Products.Count());
            Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
            Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

            GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();
            String scriptName = "NonExistentScript";

            // 1 = autoscript.
            Assert.AreEqual(s_NumAutoScripts, getScriptNamesResp.ScriptFileData.Count);
            Assert.AreEqual(false, containsName(getScriptNamesResp.ScriptFileData, scriptName));


            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
            scriptNames.Add(scriptName);

            // Run the script on the first cab listed in the event package.
            m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0].Cab, 60000);

            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);
            Assert.AreEqual(s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

            Assert.AreEqual(m_Utils.ApplicationGuid, m_Utils.RunScriptAdminReport.ClientData.ApplicationGuid);
            Assert.AreNotEqual(null, m_Utils.RunScriptAdminReport.LastException);
            Assert.AreEqual(true, m_Utils.RunScriptAdminReport.LastException.Contains("Script file does not exist"));


            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }
        [TestMethod]
        public void RunScriptAsync2ScriptFiles()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;

            m_Utils.CreateTestIndex(0, testIndexData);

            // Service is now started with the specified index.
            // Make sure we can get at least the list of products.
            GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

            Assert.AreEqual(1, getProductsResp.Products.Count());
            Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
            Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

            // Add a script.
            StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
            m_Utils.AddDebuggerScript(scriptSettings1, false);

            // Add a second script.
            StackHashScriptSettings scriptSettings2 = m_Utils.MakeScriptSettings(1, true);
            m_Utils.AddDebuggerScript(scriptSettings2, false);


            GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

            Assert.AreEqual(2 + s_NumAutoScripts, getScriptNamesResp.ScriptFileData.Count);
            Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));
            Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings2.Name));


            // Get the actual script back and compare.
            GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
            m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);
            
            getScriptResp = m_Utils.GetDebuggerScript(scriptSettings2.Name);
            m_Utils.CheckScriptSettings(scriptSettings2, getScriptResp.ScriptSettings, false);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Recorded time is only accurate to the second.
            long ticksInASecond = 10000000;

            DateTime now = DateTime.Now.ToUniversalTime();
            now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

            StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
            scriptNames.Add(scriptSettings1.Name);
            scriptNames.Add(scriptSettings2.Name);

            // Run the script on the first cab listed in the event package.
            m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0].Cab, 60000);

            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(2 + s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

            foreach (StackHashScriptResultFile resultFile in getDebugResultFilesResp.ResultFiles)
            {
                String thisScriptName = getDebugResultFilesResp.ResultFiles[0].ScriptName;

                if (thisScriptName == scriptSettings1.Name)
                    Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);
            }

 
            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);
            Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);
            Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

            getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings2.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);
            Assert.AreEqual(scriptSettings2.Name, getDebugResultResp.Result.Name);
            Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }


        [TestMethod]
        public void RemoveScriptResult()
        {
            DateTime startTime = DateTime.Now.ToUniversalTime();

            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            try
            {
                // Create a test index with one cab file.
                StackHashTestIndexData testIndexData = new StackHashTestIndexData();
                testIndexData.NumberOfProducts = 1;
                testIndexData.NumberOfFiles = 1;
                testIndexData.NumberOfEvents = 1;
                testIndexData.NumberOfEventInfos = 1;
                testIndexData.NumberOfCabs = 1;

                m_Utils.CreateTestIndex(0, testIndexData);

                // Service is now started with the specified index.
                // Make sure we can get at least the list of products.
                GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

                Assert.AreEqual(1, getProductsResp.Products.Count());
                Assert.AreEqual(1, getProductsResp.Products[0].Product.Id);
                Assert.AreEqual("StackHash1", getProductsResp.Products[0].Product.Name);

                // Add a script.
                StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
                m_Utils.AddDebuggerScript(scriptSettings1, false);

                GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

                // 2 including the autoscript.
                Assert.AreEqual(s_NumAutoScripts + 1, getScriptNamesResp.ScriptFileData.Count);
                Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));


                // Get the actual script back and compare.
                GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
                m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);

                // Find a cab to run a script on.
                StackHashProduct product;
                StackHashFile file;
                StackHashEvent theEvent;
                StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


                // Recorded time is only accurate to the second.
                long ticksInASecond = 10000000;

                DateTime now = DateTime.Now.ToUniversalTime();
                now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

                StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
                scriptNames.Add(scriptSettings1.Name);

                // Run the script on the first cab listed in the event package.
                m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0].Cab, 60000);

                // Get a list of the debug results files.
                GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

                Assert.AreEqual(1 + s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

                foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
                {
                    if (scriptSettings1.Name == scriptResult.ScriptName)
                        Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);
                }

                // Get the full result back.
                GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

                Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);

                Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
                Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

                // Get the cab notes. Should have added a Script Run message.
                GetCabNotesResponse cabNotes = m_Utils.GetCabNotes(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

                String expectedNoteText = String.Format("Script {0} executed", scriptSettings1.Name);

                bool foundNoteEntry = false;
                foreach (StackHashNoteEntry note in cabNotes.Notes)
                {
                    if (note.Note.Contains(expectedNoteText))
                    {
                        foundNoteEntry = true;
                        Assert.AreEqual(true, startTime < note.TimeOfEntry);
                        Assert.AreEqual(Environment.UserName, note.User);
                        Assert.AreEqual("StackHash Client", note.Source);
                    }
                }
                Assert.AreEqual(true, foundNoteEntry);


                // Now get the cab details - should have filled in the diagnostics.
                GetEventPackageResponse eventPackageResp = m_Utils.GetEventPackage(0, product, file, theEvent);

                StackHashCab thisCab = eventPackageResp.EventPackage.Cabs[0].Cab;
                Assert.AreNotEqual(null, thisCab.DumpAnalysis);
                Assert.AreEqual("2.0.50727.3603", thisCab.DumpAnalysis.DotNetVersion);
                Assert.AreEqual("x86", thisCab.DumpAnalysis.MachineArchitecture);
                Assert.AreEqual("Windows XP Version 2600 (Service Pack 3) MP (2 procs) Free x86 compatible", thisCab.DumpAnalysis.OSVersion);
                Assert.AreEqual("0 days 0:00:15.000", thisCab.DumpAnalysis.ProcessUpTime);
                Assert.AreEqual("not available", thisCab.DumpAnalysis.SystemUpTime);


                // REMOVE THE RESULTS FILE.
                m_Utils.RemoveScriptResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

                // Get a list of the debug results files.
                getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

                Assert.AreEqual(s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

                foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
                {
                    Assert.AreNotEqual(scriptSettings1.Name, scriptResult.ScriptName);
                }

                // Get the full result back - should not return any data.
                getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0].Cab);

                Assert.AreEqual(null, getDebugResultResp.Result);

                // Get the cab notes. Should have added a Remove Script Results message.
                cabNotes = m_Utils.GetCabNotes(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

                expectedNoteText = String.Format("Script results removed for script: {0}", scriptSettings1.Name);

                foundNoteEntry = false;
                foreach (StackHashNoteEntry note in cabNotes.Notes)
                {
                    if (note.Note.Contains(expectedNoteText))
                    {
                        foundNoteEntry = true;
                        Assert.AreEqual(true, startTime < note.TimeOfEntry);
                        Assert.AreEqual(Environment.UserName, note.User);
                        Assert.AreEqual("Service", note.Source);
                    }
                }
                Assert.AreEqual(true, foundNoteEntry);
            }
            finally
            {
                m_Utils.DeactivateContext(0);
                m_Utils.DeleteIndex(0);
            }
        }

        [TestMethod]
        public void LoadScriptCreatedByTestManager()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.CabFileName = "496858888-5-0615344527.cab"; // Contains 2 dumps.

            m_Utils.CreateTestIndex(0, testIndexData);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1, getDebugResultFilesResp.ResultFiles.Count);
            StackHashScriptResultFile resultFile = null;
            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
            {
                if ("Script0_CAB1" == scriptResult.ScriptName)
                    resultFile = scriptResult;
            }

            Assert.AreNotEqual(null, resultFile);

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, resultFile.ScriptName, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(resultFile.ScriptName, getDebugResultResp.Result.Name);

            Assert.AreEqual(true, DateTime.UtcNow >= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("This is the output from the command - line 1Script0_CAB1.log"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("This is the output from the command - line"));

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        [TestMethod]
        public void LoadScriptCreatedByTestManager_10K()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.ScriptFileSize = 10 * 1024;
            testIndexData.CabFileName = "496858888-5-0615344527.cab"; // Contains 2 dumps.

            m_Utils.CreateTestIndex(0, testIndexData);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1, getDebugResultFilesResp.ResultFiles.Count);
            StackHashScriptResultFile resultFile = null;
            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
            {
                if ("Script0_CAB1" == scriptResult.ScriptName)
                    resultFile = scriptResult;
            }

            Assert.AreNotEqual(null, resultFile);

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, resultFile.ScriptName, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(resultFile.ScriptName, getDebugResultResp.Result.Name);

            Assert.AreEqual(true, DateTime.UtcNow >= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(2, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);

            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("This is the output from the command - line 1Script0_CAB1.log"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("This is the output from the command - line"));

            // Second line should be present and contain duplicate strings.            
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput.Count >= 100);
            for (int lineCount = 0; lineCount < getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput.Count; lineCount++)
            {
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput[lineCount].Contains("This is a dummy line"));
            }

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }


        /// <summary>
        /// 64K is the WCF default. Should allow for greater than this.
        /// </summary>
        [TestMethod]
        public void LoadScriptCreatedByTestManager_65K()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.ScriptFileSize = 65 * 1024;
            testIndexData.CabFileName = "496858888-5-0615344527.cab"; // Contains 2 dumps.

            m_Utils.CreateTestIndex(0, testIndexData);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1, getDebugResultFilesResp.ResultFiles.Count);
            StackHashScriptResultFile resultFile = null;
            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
            {
                if ("Script0_CAB1" == scriptResult.ScriptName)
                    resultFile = scriptResult;
            }

            Assert.AreNotEqual(null, resultFile);

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, resultFile.ScriptName, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(resultFile.ScriptName, getDebugResultResp.Result.Name);

            Assert.AreEqual(true, DateTime.UtcNow >= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(2, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);

            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("This is the output from the command - line 1Script0_CAB1.log"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("This is the output from the command - line"));

            // Second line should be present and contain duplicate strings.            
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput.Count >= 100);
            for (int lineCount = 0; lineCount < getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput.Count; lineCount++)
            {
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput[lineCount].Contains("This is a dummy line"));
            }

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        /// <summary>
        /// 64K is the WCF default. Should allow for greater than this.
        /// </summary>
        [TestMethod]
        public void LoadScriptCreatedByTestManager_2MB()
        {
            m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

            // Add a context.
            CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

            String testPath = "c:\\stackhashunittests\\testindex\\";
            resp.Settings.ErrorIndexSettings.Folder = testPath;
            resp.Settings.ErrorIndexSettings.Name = "TestIndex";
            m_Utils.SetContextSettings(resp.Settings);
            m_Utils.DeleteIndex(0);
            m_Utils.ActivateContext(0);

            // Create a test index with one cab file.
            StackHashTestIndexData testIndexData = new StackHashTestIndexData();
            testIndexData.NumberOfProducts = 1;
            testIndexData.NumberOfFiles = 1;
            testIndexData.NumberOfEvents = 1;
            testIndexData.NumberOfEventInfos = 1;
            testIndexData.NumberOfCabs = 1;
            testIndexData.NumberOfScriptResults = 1;
            testIndexData.ScriptFileSize = 2 * 1024 * 1024;
            testIndexData.CabFileName = "496858888-5-0615344527.cab"; // Contains 2 dumps.

            m_Utils.CreateTestIndex(0, testIndexData);

            // Find a cab to run a script on.
            StackHashProduct product;
            StackHashFile file;
            StackHashEvent theEvent;
            StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


            // Get a list of the debug results files.
            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(1, getDebugResultFilesResp.ResultFiles.Count);
            StackHashScriptResultFile resultFile = null;
            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
            {
                if ("Script0_CAB1" == scriptResult.ScriptName)
                    resultFile = scriptResult;
            }

            Assert.AreNotEqual(null, resultFile);

            // Get the full result back.
            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, resultFile.ScriptName, product, file, theEvent, eventPackage.Cabs[0].Cab);

            Assert.AreEqual(resultFile.ScriptName, getDebugResultResp.Result.Name);

            Assert.AreEqual(true, DateTime.UtcNow >= getDebugResultResp.Result.RunDate);
            Assert.AreEqual(2, getDebugResultResp.Result.ScriptResults.Count);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);

            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count >= 2);
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("This is the output from the command - line 1Script0_CAB1.log"));
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("This is the output from the command - line"));

            // Second line should be present and contain duplicate strings.            
            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput.Count >= 100);
            for (int lineCount = 0; lineCount < getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput.Count; lineCount++)
            {
                Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[1].ScriptLineOutput[lineCount].Contains("This is a dummy line"));
            }

            m_Utils.DeactivateContext(0);
            m_Utils.DeleteIndex(0);
        }

        
        //[TestMethod]
        //public void RunScriptAsyncAlreadyRun()
        //{

        //    m_Utils.RegisterForNotifications(true, m_Utils.ApplicationGuid);

        //    // Add a context.
        //    CreateNewStackHashContextResponse resp = m_Utils.CreateNewContext();

        //    String testPath = "c:\\stackhashunittests\\testindex\\";
        //    resp.Settings.ErrorIndexSettings.Folder = testPath;
        //    resp.Settings.ErrorIndexSettings.Name = "TestIndex";
        //    m_Utils.SetContextSettings(resp.Settings);
        //    m_Utils.ActivateContext(0);

        //    try
        //    {
        //        // Create a test index with one cab file.
        //        StackHashTestIndexData testIndexData = new StackHashTestIndexData();
        //        testIndexData.NumberOfProducts = 1;
        //        testIndexData.NumberOfFiles = 1;
        //        testIndexData.NumberOfEvents = 1;
        //        testIndexData.NumberOfEventInfos = 1;
        //        testIndexData.NumberOfCabs = 1;

        //        m_Utils.CreateTestIndex(0, testIndexData);

        //        // Service is now started with the specified index.
        //        // Make sure we can get at least the list of products.
        //        GetProductsResponse getProductsResp = m_Utils.GetProducts(0);

        //        Assert.AreEqual(1, getProductsResp.Products.Count());
        //        Assert.AreEqual(1, getProductsResp.Products[0].Id);
        //        Assert.AreEqual("Product1", getProductsResp.Products[0].Name);

        //        // Add a script.
        //        StackHashScriptSettings scriptSettings1 = m_Utils.MakeScriptSettings(0, true);
        //        m_Utils.AddDebuggerScript(scriptSettings1, false);

        //        GetDebuggerScriptNamesResponse getScriptNamesResp = m_Utils.GetDebuggerScriptNames();

        //        // 2 including the autoscript.
        //        Assert.AreEqual(s_NumAutoScripts + 1, getScriptNamesResp.ScriptFileData.Count);
        //        Assert.AreEqual(true, containsName(getScriptNamesResp.ScriptFileData, scriptSettings1.Name));


        //        // Get the actual script back and compare.
        //        GetDebuggerScriptResponse getScriptResp = m_Utils.GetDebuggerScript(scriptSettings1.Name);
        //        m_Utils.CheckScriptSettings(scriptSettings1, getScriptResp.ScriptSettings, false);

        //        // Find a cab to run a script on.
        //        StackHashProduct product;
        //        StackHashFile file;
        //        StackHashEvent theEvent;
        //        StackHashEventPackage eventPackage = findCab(out product, out file, out theEvent);


        //        // Recorded time is only accurate to the second.
        //        long ticksInASecond = 10000000;


        //        StackHashScriptNamesCollection scriptNames = new StackHashScriptNamesCollection();
        //        scriptNames.Add(scriptSettings1.Name);

        //        // Run and check the script twice.
        //        for (int i = 0; i < 2; i++)
        //        {
        //            DateTime now = DateTime.Now.ToUniversalTime();
        //            now = new DateTime((now.Ticks / ticksInASecond) * ticksInASecond, DateTimeKind.Utc);

        //            DateTime startTime = DateTime.Now.ToUniversalTime();
        //            Thread.Sleep(1000);
        //            // Run the script on the first cab listed in the event package.
        //            m_Utils.RunDebuggerScriptAsync(0, scriptNames, product, file, theEvent, eventPackage.Cabs[0], 60000);

        //            // Get a list of the debug results files.
        //            GetDebugResultFilesResponse getDebugResultFilesResp = m_Utils.GetDebugResultFiles(0, product, file, theEvent, eventPackage.Cabs[0]);

        //            Assert.AreEqual(1 + s_NumAutoScripts, getDebugResultFilesResp.ResultFiles.Count);

        //            foreach (StackHashScriptResultFile scriptResult in getDebugResultFilesResp.ResultFiles)
        //            {
        //                if (scriptSettings1.Name == scriptResult.ScriptName)
        //                    Assert.AreEqual(true, now <= getDebugResultFilesResp.ResultFiles[0].RunDate);
        //            }

        //            // Get the full result back.
        //            GetDebugResultResponse getDebugResultResp = m_Utils.GetDebugResult(0, scriptSettings1.Name, product, file, theEvent, eventPackage.Cabs[0]);

        //            Assert.AreEqual(scriptSettings1.Name, getDebugResultResp.Result.Name);

        //            Assert.AreEqual(true, now <= getDebugResultResp.Result.RunDate);
        //            Assert.AreEqual(1, getDebugResultResp.Result.ScriptResults.Count);
        //            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput.Count > 4);
        //            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[0].Contains("eax"));
        //            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[1].Contains("eip"));
        //            Assert.AreEqual(true, getDebugResultResp.Result.ScriptResults[0].ScriptLineOutput[2].Contains("cs"));

        //            // Get the cab notes. Should have added a Script Run message.
        //            GetCabNotesResponse cabNotes = m_Utils.GetCabNotes(0, product, file, theEvent, eventPackage.Cabs[0]);

        //            String expectedNoteText = String.Format("Script {0} executed", scriptSettings1.Name);

        //            bool foundNoteEntry = false;
        //            foreach (StackHashNoteEntry note in cabNotes.Notes)
        //            {
        //                if (note.Note.Contains(expectedNoteText))
        //                {
        //                    foundNoteEntry = true;
        //                    Assert.AreEqual(true, startTime < note.TimeOfEntry);
        //                    Assert.AreEqual(Environment.UserName, note.User);
        //                    Assert.AreEqual("Service", note.Source);
        //                }
        //            }
        //            Assert.AreEqual(true, foundNoteEntry);


        //            // Now get the cab details - should have filled in the diagnostics.
        //            GetEventPackageResponse eventPackageResp = m_Utils.GetEventPackage(0, product, file, theEvent);

        //            StackHashCab thisCab = eventPackageResp.EventPackage.Cabs[0];
        //            Assert.AreNotEqual(null, thisCab.DumpAnalysis);
        //            Assert.AreEqual("2.0.50727.3603", thisCab.DumpAnalysis.DotNetVersion);
        //            Assert.AreEqual("x86", thisCab.DumpAnalysis.MachineArchitecture);
        //            Assert.AreEqual("Windows XP Version 2600 (Service Pack 3) MP (2 procs) Free x86 compatible", thisCab.DumpAnalysis.OSVersion);
        //            Assert.AreEqual("0 days 0:00:15.000", thisCab.DumpAnalysis.ProcessUpTime);
        //            Assert.AreEqual("not available", thisCab.DumpAnalysis.SystemUpTime);
        //        }
        //    }
        //    finally
        //    {
        //        m_Utils.DeactivateContext(0);
        //        m_Utils.DeleteIndex(0);
        //    }
        //}
    }
}
