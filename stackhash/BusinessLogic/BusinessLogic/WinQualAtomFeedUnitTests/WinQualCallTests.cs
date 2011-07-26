using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using StackHashBusinessObjects;
using StackHashUtilities;
using WinQualAtomFeed;

namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Summary description for WinQualCallTests
    /// </summary>
    [TestClass]
    public class WinQualCallTests
    {
        public WinQualCallTests()
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

        /// <summary>
        /// Test the retry logic - retry just 2 times.
        /// This will give an accessed denied because the event is not one of ours.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void CallFailsSoRetry()
        {
            AtomFeed atomFeed = new AtomFeed(null, 2, 100000, false, false, null, 11);

            Assert.AreEqual(true, atomFeed.Login(TestSettings.WinQualUserName, TestSettings.WinQualPassword));
            String url = "https://winqual.microsoft.com/Services/wer/user/eventdetails.aspx?eventid=102983&eventtypename=Crash 32bit";

            try
            {
                atomFeed.WinQualCallWithRetry(url, RequestType.Get, null, null);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.AccessDenied, ex.ServiceErrorCode);
            }

            Assert.AreEqual(2, atomFeed.LastCallRetryCount);
        }

        /// <summary>
        /// Test the retry logic + auto login.
        /// This will give an accessed denied because the event is not one of ours.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void CallFailsSoRetryNoLogOn()
        {
            AtomFeed atomFeed = new AtomFeed(null, 2, 100000, false, false, null, 11);

            // Use a valid old ticket.
//            String ticket = "E8F9D063F4C77065B0A8D04C5B6B72B4ED939D4768CBD146E7EC2C6F93455A9FAA326F9F8A9B9718012EBA384343650181C0006B31964A392E1BA44A6195F48E180A27743D9C32A8345F75338665C8E5DC339ED6AF734A9BCBDC8BC525610ABE3F8221B67771EB1AE2A84F158E58558BEC68A57AF28C3468926C9D35901C3446BF427009AF987CE5DC08D7D1DCB6B52BC0E2A5A55A783CE04E2F50AF6B55D3BE23217AACB924011D6D07B6690D1A27C9CB786BE504FE70183DE3DDF888E18A211FCE625A766E6511599E0A2D52B3FF96B17B5FC0C607A52B7C175BE22A6E2E3D5C2827DDF5B264E8036E03C68CB537B24002404320C41A31B6C268B89A201CF955E448E49BC380754C814D293332868E43EDAC6648E601D14C6A1838755CD59E";
            String url = "https://winqual.microsoft.com/Services/wer/user/eventdetails.aspx?eventid=102983&eventtypename=Crash 32bit";

            try
            {
                atomFeed.UserName = TestSettings.WinQualUserName;
                atomFeed.Password = TestSettings.WinQualPassword;
//                atomFeed.Ticket = ticket;
                atomFeed.WinQualCallWithRetry(url, RequestType.Get, null, null);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.AccessDenied, ex.ServiceErrorCode);
            }

            Assert.AreEqual(2, atomFeed.LastCallRetryCount);
        }

        /// <summary>
        /// Test the retry logic + auto login.
        /// This will give an accessed denied because the event is not one of ours.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void CallFailsSoRetryLogonCredentialsBad()
        {
            AtomFeed atomFeed = new AtomFeed(null, 2, 100000, false, false, null, 11);

            // Use a valid old ticket.
//            String ticket = "E8F9D063F4C77065B0A8D04C5B6B72B4ED939D4768CBD146E7EC2C6F93455A9FAA326F9F8A9B9718012EBA384343650181C0006B31964A392E1BA44A6195F48E180A27743D9C32A8345F75338665C8E5DC339ED6AF734A9BCBDC8BC525610ABE3F8221B67771EB1AE2A84F158E58558BEC68A57AF28C3468926C9D35901C3446BF427009AF987CE5DC08D7D1DCB6B52BC0E2A5A55A783CE04E2F50AF6B55D3BE23217AACB924011D6D07B6690D1A27C9CB786BE504FE70183DE3DDF888E18A211FCE625A766E6511599E0A2D52B3FF96B17B5FC0C607A52B7C175BE22A6E2E3D5C2827DDF5B264E8036E03C68CB537B24002404320C41A31B6C268B89A201CF955E448E49BC380754C814D293332868E43EDAC6648E601D14C6A1838755CD59E";
            String url = "https://winqual.microsoft.com/Services/wer/user/eventdetails.aspx?eventid=102983&eventtypename=Crash 32bit";

            try
            {
                atomFeed.UserName = "UserName";
                atomFeed.Password = TestSettings.WinQualPassword;
//                atomFeed.Ticket = ticket;
                atomFeed.WinQualCallWithRetry(url, RequestType.Get, null, null);
            }
            catch (StackHashException ex)
            {
                Assert.AreEqual(StackHashServiceErrorCode.WinQualLogOnFailed, ex.ServiceErrorCode);
            }

            Assert.AreEqual(2, atomFeed.LastCallRetryCount);
        }
    }
}
