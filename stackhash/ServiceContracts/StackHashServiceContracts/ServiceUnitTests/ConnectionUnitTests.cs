using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.ServiceModel;
using System.Diagnostics;
using System.Reflection;
using System.IO;

using ServiceUnitTests.StackHashServices;

namespace ServiceUnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ConnectionUnitTests
    {
        Guid m_AppId1;
        private Utils m_Utils;


        public ConnectionUnitTests()
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
            // Also register for admin reports.
            m_AppId1 = Guid.NewGuid();
            
            m_Utils = new Utils();
            m_Utils.RegisterForNotifications(false, m_Utils.ApplicationGuid, "UnregisterAllClients");
            m_Utils.AllReports.Clear();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (m_Utils != null)
            {
                m_Utils.SetClientTimeout(15 * 60); 
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
        public void ConnectionEstablishmentAndEventRegistration()
        {
            // Hook up the event callback first.            
            m_Utils.RegisterForNotifications(true, m_AppId1);

            // The registration event will be returned asychronously so wait for it to arrive.
            Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

            Assert.AreEqual(1, m_Utils.AllReports.Count);
            Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[0].Report.ResultData);
            Assert.AreEqual(null, m_Utils.AllReports[0].Report.LastException);

            // Unhook.            
            m_Utils.RegisterForNotifications(false, m_AppId1);
        }

        [TestMethod]
        public void RegisterDeregister()
        {
            m_Utils.RegisterForNotifications(true, m_AppId1);

            // The registration event will be returned asychronously so wait for it to arrive.
            Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

            Assert.AreEqual(1, m_Utils.AllReports.Count);
            Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[0].Report.ResultData);
            Assert.AreEqual(null, m_Utils.AllReports[0].Report.LastException);

            Assert.AreEqual(Utils.TestAdminServiceGuid, m_Utils.AllReports[0].Report.ServiceGuid);
            Assert.AreEqual(Environment.MachineName, m_Utils.AllReports[0].Report.ServiceHost);
           
            // Unhook.            
            // Hook up the event callback first.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(1, m_Utils.AllReports.Count);
        }


        [TestMethod]
        public void RegisterTwiceShouldSendAdminResportOnce()
        {
            // Hook up the event callback first.            
            m_Utils.RegisterForNotifications(true, m_AppId1);

            // The registration event will be returned asychronously so wait for it to arrive.
            Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

            Assert.AreEqual(1, m_Utils.AllReports.Count);
            Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[0].Report.ResultData);
            Assert.AreEqual(null, m_Utils.AllReports[0].Report.LastException);

            m_Utils.RegisterForNotifications(true, m_AppId1);

            // The registration event will be returned asychronously so wait for it to arrive.
            Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(2000));

            Assert.AreEqual(2, m_Utils.AllReports.Count);
            Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[1].Report.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[1].Report.ResultData);
            Assert.AreEqual(null, m_Utils.AllReports[1].Report.LastException);

            // Unhook.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(2, m_Utils.AllReports.Count);
        }
        [TestMethod]
        public void RegisterDeregisterTwice()
        {
            m_Utils.RegisterForNotifications(true, m_AppId1);

            // The registration event will be returned asychronously so wait for it to arrive.
            Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

            Assert.AreEqual(1, m_Utils.AllReports.Count);
            Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[0].Report.Operation);
            Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[0].Report.ResultData);
            Assert.AreEqual(null, m_Utils.AllReports[0].Report.LastException);

            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            // Unhook.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(1, m_Utils.AllReports.Count);
        }


        /// <summary>
        /// Same user many times with same GUID.
        /// Note that utils registers for admin notifications with a different GUID.
        /// </summary>
        [TestMethod]
        public void RegisterSameUserManyTimesSameMachine()
        {
            int numRegisters = 100;
            for (int i = 0; i < numRegisters; i++)
            {
                m_Utils.RegisterForNotifications(true, m_AppId1);

                // The registration event will be returned asychronously so wait for it to arrive.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
            }


            // Unregister.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(numRegisters, m_Utils.AllReports.Count);
        }


        /// <summary>
        /// MaxSeat limit check. Should all work.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void MaxSeatsReached()
        {
            int maxSeats = m_Utils.GetLicenseData().LicenseData.MaxSeats;

            List<Guid> allGuids = new List<Guid>();


            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats - 1; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i], "Client" + i.ToString());

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }
            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats - 1, m_Utils.AllReports.Count);
        }

        /// <summary>
        /// Check the client usage data is returned.
        /// </summary>
        [TestMethod]
        public void CheckClientUsageData()
        {
            GetLicenseDataResponse licenseDataResp = m_Utils.GetLicenseData();

            int maxSeats = licenseDataResp.LicenseData.MaxSeats;
            if (maxSeats > 5)
                maxSeats = 5;

            Assert.AreEqual(1, licenseDataResp.LicenseUsage.ClientLicenseUsageCollection.Count);
            Assert.AreEqual(0, licenseDataResp.LicenseUsage.ContextLicenseUsageCollection.Count);

            List<Guid> allGuids = new List<Guid>();

            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats - 1; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i], "Client" + i.ToString());

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }

                licenseDataResp = m_Utils.GetLicenseData();
                Assert.AreEqual(maxSeats, licenseDataResp.LicenseUsage.ClientLicenseUsageCollection.Count);
            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats - 1, m_Utils.AllReports.Count);
        }

        
        /// <summary>
        /// MaxSeat limit exceeded.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void MaxSeatsExceeded()
        {
            int maxSeats = m_Utils.GetLicenseData().LicenseData.MaxSeats;

            List<Guid> allGuids = new List<Guid>();

            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats - 1; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i], "Client" + i.ToString());

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }

                // Now try to register one more - should fail.
                allGuids.Add(Guid.NewGuid());
                m_Utils.RegisterForNotifications(true, allGuids[maxSeats - 1], "Client" + maxSeats.ToString());

                // The registration event will be returned asychronously so wait for it to arrive.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(maxSeats, m_Utils.AllReports.Count);
                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats - 1].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_Utils.AllReports[maxSeats - 1].Report.ResultData);
                Assert.AreNotEqual(null, m_Utils.AllReports[maxSeats - 1].Report.LastException);
            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // Unregister.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats, m_Utils.AllReports.Count);
        }

        /// <summary>
        /// MaxSeat limit exceeded. The next seat is admin so allow it to bump oldest client.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void MaxSeatsExceededAdminShouldBump()
        {
            int maxSeats = m_Utils.GetLicenseData().LicenseData.MaxSeats;

            List<Guid> allGuids = new List<Guid>();

            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats - 1; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i], "Client" + i.ToString());

                    Thread.Sleep(100);

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }

                // When the first service call is made, the service will attempt to auto register the client.
                // Call this instance again so that it doesn't get bumped.
                int test = m_Utils.GetLicenseData().LicenseData.MaxSeats;

                // Now try to register one more - this time it is admin - so it should also pass.
                allGuids.Add(Guid.NewGuid());
                m_Utils.RegisterForNotifications(true, allGuids[maxSeats - 1], "Client" + maxSeats.ToString(), Utils.TestAdminServiceGuid);

                // The registration event will be preceded by a failed registration for the bumped client.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats - 1].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_Utils.AllReports[maxSeats - 1].Report.ResultData);
                Assert.AreEqual(StackHashServiceErrorCode.ClientBumped, m_Utils.AllReports[maxSeats - 1].Report.ServiceErrorCode);
                Assert.AreEqual(allGuids[0], m_Utils.AllReports[maxSeats - 1].Report.ClientData.ApplicationGuid);
                Assert.AreEqual("Client0", m_Utils.AllReports[maxSeats - 1].Report.ClientData.ClientName);

                // The registration event will be returned asychronously so wait for it to arrive.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[maxSeats].Report.ResultData);
                Assert.AreEqual(StackHashServiceErrorCode.NoError, m_Utils.AllReports[maxSeats].Report.ServiceErrorCode);
                Assert.AreEqual(allGuids[allGuids.Count - 1], m_Utils.AllReports[maxSeats].Report.ClientData.ApplicationGuid);
            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // Unregister.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats + 1, m_Utils.AllReports.Count); // Extra one for the bump event.
        }


        /// <summary>
        /// MaxSeat limit exceeded. The next seat is non-admin but the client timeout is set so that 
        /// bumping will still be permitted.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void MaxSeatsExceededNonAdminShouldBumpAfterNSeconds()
        {
            int maxSeats = m_Utils.GetLicenseData().LicenseData.MaxSeats;
            
            List<Guid> allGuids = new List<Guid>();

            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats - 1; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i], "Client" + i.ToString());

                    Thread.Sleep(100);

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }

                // When the first service call is made, the service will attempt to auto register the client.
                // Call this instance again so that it doesn't get bumped.
                int test = m_Utils.GetLicenseData().LicenseData.MaxSeats;


                m_Utils.SetClientTimeout(1); // Set client timeout to 1 second to allow bumping.

                Thread.Sleep(2000);

                // Now try to register one more - this time it is admin - so it should also pass.
                allGuids.Add(Guid.NewGuid());
                m_Utils.RegisterForNotifications(true, allGuids[maxSeats - 1], "Client" + maxSeats.ToString(), Utils.TestAdminServiceGuid);

                // The registration event will be preceded by a failed registration for the bumped client.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats - 1].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_Utils.AllReports[maxSeats - 1].Report.ResultData);
                Assert.AreEqual(StackHashServiceErrorCode.ClientBumped, m_Utils.AllReports[maxSeats - 1].Report.ServiceErrorCode);
                Assert.AreEqual(allGuids[0], m_Utils.AllReports[maxSeats - 1].Report.ClientData.ApplicationGuid);
                Assert.AreEqual("Client0", m_Utils.AllReports[maxSeats - 1].Report.ClientData.ClientName);

                // The registration event will be returned asychronously so wait for it to arrive.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[maxSeats].Report.ResultData);
                Assert.AreEqual(StackHashServiceErrorCode.NoError, m_Utils.AllReports[maxSeats].Report.ServiceErrorCode);
                Assert.AreEqual(allGuids[allGuids.Count - 1], m_Utils.AllReports[maxSeats].Report.ClientData.ApplicationGuid);
            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // Unregister.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats + 1, m_Utils.AllReports.Count); // Extra one for the bump event.
        }

        
        /// <summary>
        /// MaxSeat limit reached. Then unregister one seat and ensure can register another.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void MaxSeatsExceededUnregisterOneSeat()
        {
            int maxSeats = m_Utils.GetLicenseData().LicenseData.MaxSeats;

            List<Guid> allGuids = new List<Guid>();

            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats - 1; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i], "Client" + i.ToString());

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }

                // Now try to register one more - should fail.
                allGuids.Add(Guid.NewGuid());
                m_Utils.RegisterForNotifications(true, allGuids[maxSeats - 1], "Client" + maxSeats.ToString());

                // The registration event will be returned asychronously so wait for it to arrive.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(maxSeats, m_Utils.AllReports.Count);
                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats - 1].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Failed, m_Utils.AllReports[maxSeats - 1].Report.ResultData);
                Assert.AreNotEqual(null, m_Utils.AllReports[maxSeats - 1].Report.LastException);

                // Unregister the first guid in the list and try again.
                m_Utils.RegisterForNotifications(false, allGuids[0]);
                allGuids.RemoveAt(0);

                // Now try to register one more - should pass this time.
                maxSeats++;
                int newSeatId = 2000;
                allGuids.Add(Guid.NewGuid());
                m_Utils.RegisterForNotifications(true, allGuids[allGuids.Count - 1], "Client" + newSeatId.ToString());

                // The registration event will be returned asychronously so wait for it to arrive.
                Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                Assert.AreEqual(maxSeats, m_Utils.AllReports.Count);
                Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[maxSeats - 1].Report.Operation);
                Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[maxSeats - 1].Report.ResultData);
                Assert.AreEqual(null, m_Utils.AllReports[maxSeats - 1].Report.LastException);
            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // Unregister.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats, m_Utils.AllReports.Count);
        }


        /// <summary>
        /// Register the same user many times beyond the seat limit.
        /// This emulates the same user logging on from different machines.
        /// </summary>
        [TestMethod]
        public void RegisterSameUserNTimes()
        {
            int maxSeats = m_Utils.GetLicenseData().LicenseData.MaxSeats;

            if (maxSeats > 5)
                maxSeats = 5;

            List<Guid> allGuids = new List<Guid>();

            try
            {
                // Utils registers once so only have maxSeats-1 to play with.
                for (int i = 0; i < maxSeats * 2; i++)
                {
                    allGuids.Add(Guid.NewGuid());

                    m_Utils.RegisterForNotifications(true, allGuids[i]); // Default logged in user.

                    // The registration event will be returned asychronously so wait for it to arrive.
                    Assert.AreEqual(true, m_Utils.AdminReport.WaitOne(10000));

                    Assert.AreEqual(i + 1, m_Utils.AllReports.Count);
                    Assert.AreEqual(StackHashAdminOperation.AdminRegister, m_Utils.AllReports[i].Report.Operation);
                    Assert.AreEqual(StackHashAsyncOperationResult.Success, m_Utils.AllReports[i].Report.ResultData);
                    Assert.AreEqual(null, m_Utils.AllReports[i].Report.LastException);
                }

            }
            finally
            {
                // Unregister.    
                foreach (Guid thisGuid in allGuids)
                {
                    m_Utils.RegisterForNotifications(false, thisGuid);
                }
            }

            // Unregister.            
            m_Utils.RegisterForNotifications(false, m_AppId1);

            // There should be no deregistration event.
            Thread.Sleep(1000);

            Assert.AreEqual(maxSeats * 2, m_Utils.AllReports.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(FaultException<ReceiverFaultDetail>))]
        public void ConnectionEstablishmentCheckVersionWrongVersion()
        {
            try
            {
                m_Utils.CheckVersion(0, 5, null); // Old beta client.
            }
            catch (FaultException<ReceiverFaultDetail> ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.ClientVersionMismatch, ex.Detail.ServiceErrorCode);
                throw;
            }        
        }

        [TestMethod]
        public void CheckVersionNullServiceGuid()
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int stackHashMajorVersion = fvi.FileMajorPart;
            int stackHashMinorVersion = fvi.FileMinorPart;

            String serviceGuid = null;

            CheckVersionResponse resp = m_Utils.CheckVersion(stackHashMajorVersion, stackHashMinorVersion, serviceGuid);

            Assert.AreEqual(false, resp.IsLocalClient);
        }

        [TestMethod]
        public void CheckVersionEmptyServiceGuid()
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int stackHashMajorVersion = fvi.FileMajorPart;
            int stackHashMinorVersion = fvi.FileMinorPart;

            String serviceGuid = String.Empty;

            CheckVersionResponse resp = m_Utils.CheckVersion(stackHashMajorVersion, stackHashMinorVersion, serviceGuid);

            Assert.AreEqual(false, resp.IsLocalClient);
        }

        [TestMethod]
        public void CheckVersionWrongServiceGuid()
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int stackHashMajorVersion = fvi.FileMajorPart;
            int stackHashMinorVersion = fvi.FileMinorPart;

            String serviceGuid = Guid.NewGuid().ToString();

            CheckVersionResponse resp = m_Utils.CheckVersion(stackHashMajorVersion, stackHashMinorVersion, serviceGuid);

            Assert.AreEqual(false, resp.IsLocalClient);
        }

        [TestMethod]
        public void CheckVersionSameServiceGuid()
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int stackHashMajorVersion = fvi.FileMajorPart;
            int stackHashMinorVersion = fvi.FileMinorPart;

            // Get the service guid.
            String [] serviceData = File.ReadAllLines("C:\\programdata\\stackhash\\test\\serviceinstancedata.txt"); 
            String serviceGuid = null;

            foreach (String line in serviceData)
            {
                if (line.StartsWith("ServiceGuid=", StringComparison.OrdinalIgnoreCase))
                {
                    serviceGuid = line.Substring("ServiceGuid=".Length);
                }
            }            

            CheckVersionResponse resp = m_Utils.CheckVersion(stackHashMajorVersion, stackHashMinorVersion, serviceGuid);

            Assert.AreEqual(true, resp.IsLocalClient);
        }
    }
}
