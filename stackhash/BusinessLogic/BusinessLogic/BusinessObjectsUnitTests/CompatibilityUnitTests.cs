using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.IO;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for CompatibilityUnitTests
    /// </summary>
    [TestClass]
    public class CompatibilityUnitTests
    {
        public CompatibilityUnitTests()
        {
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
        public void Beta5EventUpgradeBugId()
        {
            String beta4EventFile = TestSettings.TestDataFolder + @"settings\beta4event.xml";
            
            XmlSerializer m_EventSerializer = new XmlSerializer(typeof(StackHashEvent),
                new Type[] { typeof(StackHashEvent), typeof(StackHashEventSignature), 
                             typeof(StackHashParameterCollection), typeof(StackHashParameter) });

            FileStream fileStream = File.Open(beta4EventFile, FileMode.Open, FileAccess.Read);

            try
            {
                StackHashEvent theEvent = m_EventSerializer.Deserialize(fileStream) as StackHashEvent;

                // Beta 4 fields.
                Assert.AreEqual(DateTime.Parse("2010-04-19T10:20:00Z").ToUniversalTime(), theEvent.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-04-19T10:20:00Z").ToUniversalTime(), theEvent.DateModifiedLocal);
                Assert.AreEqual(@"CLR20 Managed Crash", theEvent.EventTypeName);
                Assert.AreEqual(1099298216, theEvent.Id);
                Assert.AreEqual(1, theEvent.TotalHits);
                Assert.AreEqual(4232330, theEvent.FileId);
                Assert.AreEqual(@"Crashy.exe", theEvent.EventSignature.ApplicationName);
                Assert.AreEqual(@"1.2.3.4", theEvent.EventSignature.ApplicationVersion);
                Assert.AreEqual(DateTime.Parse("2010-04-16T22:21:32"), theEvent.EventSignature.ApplicationTimeStamp);
                Assert.AreEqual(@"Crashy", theEvent.EventSignature.ModuleName);
                Assert.AreEqual(@"1.2.3.4", theEvent.EventSignature.ModuleVersion);
                Assert.AreEqual(DateTime.Parse("2010-04-16T22:21:32"), theEvent.EventSignature.ModuleTimeStamp);
                Assert.AreEqual(152, theEvent.EventSignature.Offset);
                Assert.AreEqual(0, theEvent.EventSignature.ExceptionCode);

                // Beta 5 fields.
                Assert.AreEqual(null, theEvent.BugId);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        [TestMethod]
        public void Beta5CabUpgradeStructureVersionAndDownloadedFlag()
        {
            String beta4CabInfoFile = TestSettings.TestDataFolder + @"settings\beta4cabInfo.xml";

            XmlSerializer cabSerializer = new XmlSerializer(typeof(StackHashCab), new Type[] { typeof(StackHashCab), typeof(StackHashDumpAnalysis) });

            FileStream fileStream = File.Open(beta4CabInfoFile, FileMode.Open, FileAccess.Read);

            try
            {
                StackHashCab theCab = cabSerializer.Deserialize(fileStream) as StackHashCab;

                // Beta 4 fields.
                Assert.AreEqual(DateTime.Parse("2010-05-03T18:44:00Z").ToUniversalTime(), theCab.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-05-03T18:44:00Z").ToUniversalTime(), theCab.DateModifiedLocal);
                Assert.AreEqual(1099304277, theCab.EventId);
                Assert.AreEqual(@"CLR20 Managed Crash", theCab.EventTypeName);
                Assert.AreEqual(@"1099304277-CLR20ManagedCrash-0845934887.cab", theCab.FileName);
                Assert.AreEqual(845934887, theCab.Id);
                Assert.AreEqual(9301398, theCab.SizeInBytes);
                Assert.AreEqual("4 days 2:10:35.391", theCab.DumpAnalysis.SystemUpTime);
                Assert.AreEqual("0 days 0:00:12.000", theCab.DumpAnalysis.ProcessUpTime);
                Assert.AreEqual("2.0.50727.4927", theCab.DumpAnalysis.DotNetVersion);
                Assert.AreEqual("Windows 7 Version 7600 MP (8 procs) Free x64", theCab.DumpAnalysis.OSVersion);
                Assert.AreEqual("x64", theCab.DumpAnalysis.MachineArchitecture);

                // Beta 5 fields.
                Assert.AreEqual(0, theCab.StructureVersion);
                Assert.AreEqual(false, theCab.CabDownloaded);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }
    }
}
