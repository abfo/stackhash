using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using StackHashBusinessObjects;
using WinQualAtomFeed;

namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Summary description for TestDatabaseUnitTests
    /// </summary>
    [TestClass]
    public class TestDatabaseUnitTests
    {
        public TestDatabaseUnitTests()
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
        public void ProductLoad()
        {
            String tempFileName = Path.GetTempFileName();

            String contents =
                "<Product> " +
                " <Id>1</Id> " +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> " +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> " +
                " <FilesLink>c:\\test</FilesLink> " +
                " <Name>Cucku Backup V2</Name> " +
                " <TotalEvents>1</TotalEvents> " +
                " <TotalResponses>2</TotalResponses> " +
                " <Version>1.2.3.4</Version> " +
                "</Product>";

            try
            {
                File.WriteAllText(tempFileName, contents);

                TestDatabase database = new TestDatabase(tempFileName);


                AtomProductCollection products = database.GetProducts();
                Assert.AreEqual(1, products.Count);

                StackHashProduct product = products[0].Product;
                Assert.AreEqual(1, product.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-30 10:54:20Z"), product.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-07-30 10:53:20Z"), product.DateModifiedLocal);
                Assert.AreEqual("c:\\test", product.FilesLink);
                Assert.AreEqual("Cucku Backup V2", product.Name);
                Assert.AreEqual(1, product.TotalEvents);
                Assert.AreEqual(2, product.TotalResponses);
                Assert.AreEqual("1.2.3.4", product.Version);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }


        [TestMethod]
        public void FileLoad()
        {
            String tempFileName = Path.GetTempFileName();

            String contents =
                "<Product> " +
                " <Id>1</Id> " +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> " +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> " +
                " <FilesLink>c:\\test</FilesLink> " +
                " <Name>Cucku Backup V2</Name> " +
                " <TotalEvents>1</TotalEvents> " +
                " <TotalResponses>2</TotalResponses> " +
                " <Version>1.2.3.4</Version> " +
                " <File> " +
                "   <Id>1000</Id> " +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> " +
                "   <Name>StackHashBusinessObjects</Name> " +
                "   <Version>1.2.3.3</Version> " +
                "   </File> " + 
                "</Product>";

            try
            {
                File.WriteAllText(tempFileName, contents);

                TestDatabase database = new TestDatabase(tempFileName);

                AtomProductCollection products = database.GetProducts();
                Assert.AreEqual(1, products.Count);

                StackHashProduct product = products[0].Product;
                Assert.AreEqual(1, product.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-30 10:54:20Z"), product.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-07-30 10:53:20Z"), product.DateModifiedLocal);
                Assert.AreEqual("c:\\test", product.FilesLink);
                Assert.AreEqual("Cucku Backup V2", product.Name);
                Assert.AreEqual(1, product.TotalEvents);
                Assert.AreEqual(2, product.TotalResponses);
                Assert.AreEqual("1.2.3.4", product.Version);

                AtomFileCollection files = database.GetFiles(product.Id);
                Assert.AreEqual(1, files.Count);

                StackHashFile file = files[0].File;
                Assert.AreEqual(1000, file.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), file.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), file.DateModifiedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-22 10:54:20Z"), file.LinkDateLocal);
                Assert.AreEqual("StackHashBusinessObjects", file.Name);
                Assert.AreEqual("1.2.3.3", file.Version);

            }
            finally
            {
                File.Delete(tempFileName);
            }
        }


        [TestMethod]
        public void EventLoad()
        {
            String tempFileName = Path.GetTempFileName();

            String contents =
                "<Product> " +
                " <Id>1</Id> " +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> " +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> " +
                " <FilesLink>c:\\test</FilesLink> " +
                " <Name>Cucku Backup V2</Name> " +
                " <TotalEvents>1</TotalEvents> " +
                " <TotalResponses>2</TotalResponses> " +
                " <Version>1.2.3.4</Version> " +
                " <File> " +
                "   <Id>1000</Id> " +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> " +
                "   <Name>StackHashBusinessObjects</Name> " +
                "   <Version>1.2.3.3</Version> " +
                "   <Event> " + 
                "     <Id>1020</Id>" + 
                "     <EventTypeName>Crash32</EventTypeName> " +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "     <TotalHits>24</TotalHits> " +
                "     <Signature> " + 
                "        <applicationName>Cucku.exe</applicationName> " + 
                "        <applicationVersion>1.1.1.1</applicationVersion> " + 
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> " +
                "        <moduleName>SomeDLL.dll</moduleName> " + 
                "        <moduleVersion>2.2.2.2</moduleVersion> " +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> " +
                "        <exceptionCode>0x1234</exceptionCode> " + 
                "        <offset>0x1222</offset> " + 
                "     </Signature> " + 
                "   </Event> " + 
                " </File> " +
                "</Product>";

            try
            {
                File.WriteAllText(tempFileName, contents);

                TestDatabase database = new TestDatabase(tempFileName);

                AtomProductCollection products = database.GetProducts();
                Assert.AreEqual(1, products.Count);

                StackHashProduct product = products[0].Product;
                Assert.AreEqual(1, product.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-30 10:54:20Z"), product.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-07-30 10:53:20Z"), product.DateModifiedLocal);
                Assert.AreEqual("c:\\test", product.FilesLink);
                Assert.AreEqual("Cucku Backup V2", product.Name);
                Assert.AreEqual(1, product.TotalEvents);
                Assert.AreEqual(2, product.TotalResponses);
                Assert.AreEqual("1.2.3.4", product.Version);

                AtomFileCollection files = database.GetFiles(product.Id);
                Assert.AreEqual(1, files.Count);

                StackHashFile file = files[0].File;
                Assert.AreEqual(1000, file.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), file.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), file.DateModifiedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-22 10:54:20Z"), file.LinkDateLocal);
                Assert.AreEqual("StackHashBusinessObjects", file.Name);
                Assert.AreEqual("1.2.3.3", file.Version);

                AtomEventCollection events = database.GetEvents(product.Id, file.Id);
                Assert.AreEqual(1, events.Count);

                StackHashEvent theEvent = events[0].Event;
                theEvent.EventSignature.InterpretParameters();

                Assert.AreEqual(1020, theEvent.Id);
                Assert.AreEqual("Crash32", theEvent.EventTypeName);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), theEvent.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), theEvent.DateModifiedLocal);
                Assert.AreEqual(24, theEvent.TotalHits);

                Assert.AreEqual("Cucku.exe", theEvent.EventSignature.ApplicationName);
                Assert.AreEqual("1.1.1.1", theEvent.EventSignature.ApplicationVersion);
                Assert.AreEqual(DateTime.Parse("2010-01-22 11:54:20Z"), theEvent.EventSignature.ApplicationTimeStamp);
                Assert.AreEqual("SomeDLL.dll", theEvent.EventSignature.ModuleName);
                Assert.AreEqual("2.2.2.2", theEvent.EventSignature.ModuleVersion);
                Assert.AreEqual(DateTime.Parse("2010-01-22 11:55:20Z"), theEvent.EventSignature.ModuleTimeStamp);
                Assert.AreEqual(0x1234, theEvent.EventSignature.ExceptionCode);
                Assert.AreEqual(0x1222, theEvent.EventSignature.Offset);


            }
            finally
            {
                File.Delete(tempFileName);
            }
        }


        [TestMethod]
        public void EventInfo()
        {
            String tempFileName = Path.GetTempFileName();

            String contents =
                "<Product> " +
                " <Id>1</Id> " +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> " +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> " +
                " <FilesLink>c:\\test</FilesLink> " +
                " <Name>Cucku Backup V2</Name> " +
                " <TotalEvents>1</TotalEvents> " +
                " <TotalResponses>2</TotalResponses> " +
                " <Version>1.2.3.4</Version> " +
                " <File> " +
                "   <Id>1000</Id> " +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> " +
                "   <Name>StackHashBusinessObjects</Name> " +
                "   <Version>1.2.3.3</Version> " +
                "   <Event> " + 
                "     <Id>1020</Id>" + 
                "     <EventTypeName>Crash32</EventTypeName> " +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "     <TotalHits>24</TotalHits> " +
                "     <Signature> " + 
                "        <applicationName>Cucku.exe</applicationName> " + 
                "        <applicationVersion>1.1.1.1</applicationVersion> " + 
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> " +
                "        <moduleName>SomeDLL.dll</moduleName> " + 
                "        <moduleVersion>2.2.2.2</moduleVersion> " +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> " +
                "        <exceptionCode>0x1234</exceptionCode> " + 
                "        <offset>0x1222</offset> " + 
                "     </Signature> " + 
                "     <EventInfo> " + 
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> " +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> " +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> " +
                "        <Language>English - United States</Language> " +
                "        <Lcid>1033</Lcid> " +
                "        <Locale>en-US</Locale> " +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> " +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> " +
                "     </EventInfo> " + 
                "   </Event> " + 
                " </File> " +
                "</Product>";

            try
            {
                File.WriteAllText(tempFileName, contents);

                TestDatabase database = new TestDatabase(tempFileName);

                AtomProductCollection products = database.GetProducts();
                Assert.AreEqual(1, products.Count);

                StackHashProduct product = products[0].Product;
                Assert.AreEqual(1, product.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-30 10:54:20Z"), product.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-07-30 10:53:20Z"), product.DateModifiedLocal);
                Assert.AreEqual("c:\\test", product.FilesLink);
                Assert.AreEqual("Cucku Backup V2", product.Name);
                Assert.AreEqual(1, product.TotalEvents);
                Assert.AreEqual(2, product.TotalResponses);
                Assert.AreEqual("1.2.3.4", product.Version);

                AtomFileCollection files = database.GetFiles(product.Id);
                Assert.AreEqual(1, files.Count);

                StackHashFile file = files[0].File;
                Assert.AreEqual(1000, file.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), file.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), file.DateModifiedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-22 10:54:20Z"), file.LinkDateLocal);
                Assert.AreEqual("StackHashBusinessObjects", file.Name);
                Assert.AreEqual("1.2.3.3", file.Version);

                AtomEventCollection events = database.GetEvents(product.Id, file.Id);
                Assert.AreEqual(1, events.Count);

                StackHashEvent theEvent = events[0].Event;
                theEvent.EventSignature.InterpretParameters();

                Assert.AreEqual(1020, theEvent.Id);
                Assert.AreEqual("Crash32", theEvent.EventTypeName);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), theEvent.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), theEvent.DateModifiedLocal);
                Assert.AreEqual(24, theEvent.TotalHits);

                Assert.AreEqual("Cucku.exe", theEvent.EventSignature.ApplicationName);
                Assert.AreEqual("1.1.1.1", theEvent.EventSignature.ApplicationVersion);
                Assert.AreEqual(DateTime.Parse("2010-01-22 11:54:20Z"), theEvent.EventSignature.ApplicationTimeStamp);
                Assert.AreEqual("SomeDLL.dll", theEvent.EventSignature.ModuleName);
                Assert.AreEqual("2.2.2.2", theEvent.EventSignature.ModuleVersion);
                Assert.AreEqual(DateTime.Parse("2010-01-22 11:55:20Z"), theEvent.EventSignature.ModuleTimeStamp);
                Assert.AreEqual(0x1234, theEvent.EventSignature.ExceptionCode);
                Assert.AreEqual(0x1222, theEvent.EventSignature.Offset);

                AtomEventInfoCollection eventInfos = database.GetEventInfos(product.Id, file.Id, theEvent.Id);
                Assert.AreEqual(1, eventInfos.Count);

                StackHashEventInfo eventInfo = eventInfos[0].EventInfo;

                Assert.AreEqual(DateTime.Parse("2011-06-20 10:54:20Z"), eventInfo.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2012-06-21 10:54:20Z"), eventInfo.DateModifiedLocal);
                Assert.AreEqual(DateTime.Parse("2013-06-22 10:54:20Z"), eventInfo.HitDateLocal);
                Assert.AreEqual("English - United States", eventInfo.Language);
                Assert.AreEqual(1033, eventInfo.Lcid);
                Assert.AreEqual("en-US", eventInfo.Locale);
                Assert.AreEqual("Windows 7", eventInfo.OperatingSystemName);
                Assert.AreEqual("6.1.7600.2.0.0", eventInfo.OperatingSystemVersion);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void EventInfoAndCab()
        {
            String tempFileName = Path.GetTempFileName();

            String contents =
                "<Product> " +
                " <Id>1</Id> " +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> " +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> " +
                " <FilesLink>c:\\test</FilesLink> " +
                " <Name>Cucku Backup V2</Name> " +
                " <TotalEvents>1</TotalEvents> " +
                " <TotalResponses>2</TotalResponses> " +
                " <Version>1.2.3.4</Version> " +
                " <File> " +
                "   <Id>1000</Id> " +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> " +
                "   <Name>StackHashBusinessObjects</Name> " +
                "   <Version>1.2.3.3</Version> " +
                "   <Event> " +
                "     <Id>1020</Id>" +
                "     <EventTypeName>Crash32</EventTypeName> " +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> " +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> " +
                "     <TotalHits>24</TotalHits> " +
                "     <Signature> " +
                "        <applicationName>Cucku.exe</applicationName> " +
                "        <applicationVersion>1.1.1.1</applicationVersion> " +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> " +
                "        <moduleName>SomeDLL.dll</moduleName> " +
                "        <moduleVersion>2.2.2.2</moduleVersion> " +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> " +
                "        <exceptionCode>0x1234</exceptionCode> " +
                "        <offset>0x1222</offset> " +
                "     </Signature> " +
                "     <EventInfo> " +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> " +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> " +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> " +
                "        <Language>English - United States</Language> " +
                "        <Lcid>1033</Lcid> " +
                "        <Locale>en-US</Locale> " +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> " +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> " +
                "     </EventInfo> " +
                "     <Cab> " +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> " +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> " +
                "        <EventId>1020</EventId>" +
                "        <EventTypeName>Crash32</EventTypeName> " +
                "        <Id>123456</Id>" +
                "        <FileName>CAB_123456.cab</FileName>" +
                "        <SizeInBytes>10300</SizeInBytes>" +
                "     </Cab> " + 
                "   </Event> " +
                " </File> " +
                "</Product>";

            try
            {
                File.WriteAllText(tempFileName, contents);

                TestDatabase database = new TestDatabase(tempFileName);

                AtomProductCollection products = database.GetProducts();
                Assert.AreEqual(1, products.Count);

                StackHashProduct product = products[0].Product;
                Assert.AreEqual(1, product.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-30 10:54:20Z"), product.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-07-30 10:53:20Z"), product.DateModifiedLocal);
                Assert.AreEqual("c:\\test", product.FilesLink);
                Assert.AreEqual("Cucku Backup V2", product.Name);
                Assert.AreEqual(1, product.TotalEvents);
                Assert.AreEqual(2, product.TotalResponses);
                Assert.AreEqual("1.2.3.4", product.Version);

                AtomFileCollection files = database.GetFiles(product.Id);
                Assert.AreEqual(1, files.Count);

                StackHashFile file = files[0].File;
                Assert.AreEqual(1000, file.Id);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), file.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), file.DateModifiedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-22 10:54:20Z"), file.LinkDateLocal);
                Assert.AreEqual("StackHashBusinessObjects", file.Name);
                Assert.AreEqual("1.2.3.3", file.Version);

                AtomEventCollection events = database.GetEvents(product.Id, file.Id);
                Assert.AreEqual(1, events.Count);

                StackHashEvent theEvent = events[0].Event;
                theEvent.EventSignature.InterpretParameters();

                Assert.AreEqual(1020, theEvent.Id);
                Assert.AreEqual("Crash32", theEvent.EventTypeName);
                Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), theEvent.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), theEvent.DateModifiedLocal);
                Assert.AreEqual(24, theEvent.TotalHits);

                Assert.AreEqual("Cucku.exe", theEvent.EventSignature.ApplicationName);
                Assert.AreEqual("1.1.1.1", theEvent.EventSignature.ApplicationVersion);
                Assert.AreEqual(DateTime.Parse("2010-01-22 11:54:20Z"), theEvent.EventSignature.ApplicationTimeStamp);
                Assert.AreEqual("SomeDLL.dll", theEvent.EventSignature.ModuleName);
                Assert.AreEqual("2.2.2.2", theEvent.EventSignature.ModuleVersion);
                Assert.AreEqual(DateTime.Parse("2010-01-22 11:55:20Z"), theEvent.EventSignature.ModuleTimeStamp);
                Assert.AreEqual(0x1234, theEvent.EventSignature.ExceptionCode);
                Assert.AreEqual(0x1222, theEvent.EventSignature.Offset);

                AtomEventInfoCollection eventInfos = database.GetEventInfos(product.Id, file.Id, theEvent.Id);
                Assert.AreEqual(1, eventInfos.Count);

                StackHashEventInfo eventInfo = eventInfos[0].EventInfo;

                Assert.AreEqual(DateTime.Parse("2011-06-20 10:54:20Z"), eventInfo.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2012-06-21 10:54:20Z"), eventInfo.DateModifiedLocal);
                Assert.AreEqual(DateTime.Parse("2013-06-22 10:54:20Z"), eventInfo.HitDateLocal);
                Assert.AreEqual("English - United States", eventInfo.Language);
                Assert.AreEqual(1033, eventInfo.Lcid);
                Assert.AreEqual("en-US", eventInfo.Locale);
                Assert.AreEqual("Windows 7", eventInfo.OperatingSystemName);
                Assert.AreEqual("6.1.7600.2.0.0", eventInfo.OperatingSystemVersion);

                AtomCabCollection cabs = database.GetCabs(product.Id, file.Id, theEvent.Id);
                Assert.AreEqual(1, cabs.Count);

                StackHashCab cab = cabs[0].Cab;

                Assert.AreEqual(DateTime.Parse("2011-06-22 10:54:20Z"), cab.DateCreatedLocal);
                Assert.AreEqual(DateTime.Parse("2012-06-21 10:54:20Z"), cab.DateModifiedLocal);
                Assert.AreEqual(1020, cab.EventId);
                Assert.AreEqual("Crash32", cab.EventTypeName);
                Assert.AreEqual(123456, cab.Id);
                Assert.AreEqual("CAB_123456.cab", cab.FileName);
                Assert.AreEqual(10300, cab.SizeInBytes);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void Multiple()
        {
            String tempFileName = Path.GetTempFileName();

            String contents =
                "<TestData> \n" + 
                "<Product> \n" +
                " <Id>1</Id> \n" +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> \n" +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> \n" +
                " <FilesLink>c:\\test</FilesLink> \n" +
                " <Name>Cucku Backup V2</Name> \n" +
                " <TotalEvents>1</TotalEvents> \n" +
                " <TotalResponses>2</TotalResponses> \n" +
                " <Version>1.2.3.4</Version> \n" +
                " <File> \n" +
                "   <Id>1000</Id> \n" +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> \n" +
                "   <Name>StackHashBusinessObjects</Name> \n" +
                "   <Version>1.2.3.3</Version> \n" +
                "   <Event> \n" +
                "     <Id>1020</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1020</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>80</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1020</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>81</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                "   <Event> \n" +
                "     <Id>1021</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1021</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>82</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1021</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>83</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                " </File> \n" +
                " <File> \n" +
                "   <Id>1001</Id> \n" +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> \n" +
                "   <Name>StackHashBusinessObjects</Name> \n" +
                "   <Version>1.2.3.3</Version> \n" +
                "   <Event> \n" +
                "     <Id>1022</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1022</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>84</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1022</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>85</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                "   <Event> \n" +
                "     <Id>1023</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1023</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>86</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1023</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>87</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                " </File> \n" +
                "</Product>\n" +
                "<Product> \n" +
                " <Id>2</Id> \n" +
                " <DateCreatedLocal>2010-06-30 10:54:20Z</DateCreatedLocal> \n" +
                " <DateModifiedLocal>2010-07-30 10:53:20Z</DateModifiedLocal> \n" +
                " <FilesLink>c:\\test</FilesLink> \n" +
                " <Name>Cucku Backup V2</Name> \n" +
                " <TotalEvents>1</TotalEvents> \n" +
                " <TotalResponses>2</TotalResponses> \n" +
                " <Version>1.2.3.4</Version> \n" +
                " <File> \n" +
                "   <Id>1002</Id> \n" +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> \n" +
                "   <Name>StackHashBusinessObjects</Name> \n" +
                "   <Version>1.2.3.3</Version> \n" +
                "   <Event> \n" +
                "     <Id>1024</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1024</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>88</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1024</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>89</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                "   <Event> \n" +
                "     <Id>1025</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1025</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>90</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1025</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>91</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                " </File> \n" +
                " <File> \n" +
                "   <Id>1003</Id> \n" +
                "   <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "   <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "   <LinkDateLocal>2010-06-22 10:54:20Z</LinkDateLocal> \n" +
                "   <Name>StackHashBusinessObjects</Name> \n" +
                "   <Version>1.2.3.3</Version> \n" +
                "   <Event> \n" +
                "     <Id>1026</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1026</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>92</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1026</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>93</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                "   <Event> \n" +
                "     <Id>1027</Id>\n" +
                "     <EventTypeName>Crash32</EventTypeName> \n" +
                "     <DateCreatedLocal>2010-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "     <DateModifiedLocal>2010-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "     <TotalHits>24</TotalHits> \n" +
                "     <Signature> \n" +
                "        <applicationName>Cucku.exe</applicationName> \n" +
                "        <applicationVersion>1.1.1.1</applicationVersion> \n" +
                "        <applicationTimeStamp>2010-01-22 11:54:20Z</applicationTimeStamp> \n" +
                "        <moduleName>SomeDLL.dll</moduleName> \n" +
                "        <moduleVersion>2.2.2.2</moduleVersion> \n" +
                "        <moduleTimeStamp>2010-01-22 11:55:20Z</moduleTimeStamp> \n" +
                "        <exceptionCode>0x1234</exceptionCode> \n" +
                "        <offset>0x1222</offset> \n" +
                "     </Signature> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <EventInfo> \n" +
                "        <DateCreatedLocal>2011-06-20 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <HitDateLocal>2013-06-22 10:54:20Z</HitDateLocal> \n" +
                "        <Language>English - United States</Language> \n" +
                "        <Lcid>1033</Lcid> \n" +
                "        <Locale>en-US</Locale> \n" +
                "        <OperatingSystemName>Windows 7</OperatingSystemName> \n" +
                "        <OperatingSystemVersion>6.1.7600.2.0.0</OperatingSystemVersion> \n" +
                "     </EventInfo> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1027</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>94</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "     <Cab> \n" +
                "        <DateCreatedLocal>2011-06-22 10:54:20Z</DateCreatedLocal> \n" +
                "        <DateModifiedLocal>2012-06-21 10:54:20Z</DateModifiedLocal> \n" +
                "        <EventId>1027</EventId>\n" +
                "        <EventTypeName>Crash32</EventTypeName> \n" +
                "        <Id>95</Id>\n" +
                "        <FileName>CAB_123456.cab</FileName>\n" +
                "        <SizeInBytes>10300</SizeInBytes>\n" +
                "     </Cab> \n" +
                "   </Event> \n" +
                " </File> \n" +
                "</Product>\n" + 
                "</TestData> "; 
 
            try
            {
                File.WriteAllText(tempFileName, contents);

                TestDatabase database = new TestDatabase(tempFileName);

                AtomProductCollection products = database.GetProducts();
                Assert.AreEqual(2, products.Count);

                int productId = 1;
                int fileId = 1000;
                int eventId = 1020 - 1;
                int cabId = 80;
                foreach (AtomProduct atomProduct in products)
                {
                    StackHashProduct product = atomProduct.Product;
                    Assert.AreEqual(productId++, product.Id);

                    Assert.AreEqual(DateTime.Parse("2010-06-30 10:54:20Z"), product.DateCreatedLocal);
                    Assert.AreEqual(DateTime.Parse("2010-07-30 10:53:20Z"), product.DateModifiedLocal);
                    Assert.AreEqual("c:\\test", product.FilesLink);
                    Assert.AreEqual("Cucku Backup V2", product.Name);
                    Assert.AreEqual(1, product.TotalEvents);
                    Assert.AreEqual(2, product.TotalResponses);
                    Assert.AreEqual("1.2.3.4", product.Version);

                    AtomFileCollection files = database.GetFiles(product.Id);
                    Assert.AreEqual(2, files.Count);

                    foreach (AtomFile atomFile in files)
                    {
                        StackHashFile file = atomFile.File;
                        Assert.AreEqual(fileId++, file.Id);
                        Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), file.DateCreatedLocal);
                        Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), file.DateModifiedLocal);
                        Assert.AreEqual(DateTime.Parse("2010-06-22 10:54:20Z"), file.LinkDateLocal);
                        Assert.AreEqual("StackHashBusinessObjects", file.Name);
                        Assert.AreEqual("1.2.3.3", file.Version);

                        AtomEventCollection events = database.GetEvents(product.Id, file.Id);
                        Assert.AreEqual(2, events.Count);

                        foreach (AtomEvent atomEvent in events)
                        {
                            eventId++;

                            StackHashEvent theEvent = atomEvent.Event;
                            theEvent.EventSignature.InterpretParameters();

                            Assert.AreEqual(eventId, theEvent.Id);
                            Assert.AreEqual("Crash32", theEvent.EventTypeName);
                            Assert.AreEqual(DateTime.Parse("2010-06-20 10:54:20Z"), theEvent.DateCreatedLocal);
                            Assert.AreEqual(DateTime.Parse("2010-06-21 10:54:20Z"), theEvent.DateModifiedLocal);
                            Assert.AreEqual(24, theEvent.TotalHits);

                            Assert.AreEqual("Cucku.exe", theEvent.EventSignature.ApplicationName);
                            Assert.AreEqual("1.1.1.1", theEvent.EventSignature.ApplicationVersion);
                            Assert.AreEqual(DateTime.Parse("2010-01-22 11:54:20Z"), theEvent.EventSignature.ApplicationTimeStamp);
                            Assert.AreEqual("SomeDLL.dll", theEvent.EventSignature.ModuleName);
                            Assert.AreEqual("2.2.2.2", theEvent.EventSignature.ModuleVersion);
                            Assert.AreEqual(DateTime.Parse("2010-01-22 11:55:20Z"), theEvent.EventSignature.ModuleTimeStamp);
                            Assert.AreEqual(0x1234, theEvent.EventSignature.ExceptionCode);
                            Assert.AreEqual(0x1222, theEvent.EventSignature.Offset);

                            AtomEventInfoCollection eventInfos = database.GetEventInfos(product.Id, file.Id, theEvent.Id);
                            Assert.AreEqual(2, eventInfos.Count);


                            foreach (AtomEventInfo atomEventInfo in eventInfos)
                            {
                                StackHashEventInfo eventInfo = atomEventInfo.EventInfo;

                                Assert.AreEqual(DateTime.Parse("2011-06-20 10:54:20Z"), eventInfo.DateCreatedLocal);
                                Assert.AreEqual(DateTime.Parse("2012-06-21 10:54:20Z"), eventInfo.DateModifiedLocal);
                                Assert.AreEqual(DateTime.Parse("2013-06-22 10:54:20Z"), eventInfo.HitDateLocal);
                                Assert.AreEqual("English - United States", eventInfo.Language);
                                Assert.AreEqual(1033, eventInfo.Lcid);
                                Assert.AreEqual("en-US", eventInfo.Locale);
                                Assert.AreEqual("Windows 7", eventInfo.OperatingSystemName);
                                Assert.AreEqual("6.1.7600.2.0.0", eventInfo.OperatingSystemVersion);
                            }

                            AtomCabCollection cabs = database.GetCabs(product.Id, file.Id, theEvent.Id);
                            Assert.AreEqual(2, cabs.Count);

                            foreach (AtomCab atomCab in cabs)
                            {
                                StackHashCab cab = atomCab.Cab;

                                Assert.AreEqual(DateTime.Parse("2011-06-22 10:54:20Z"), cab.DateCreatedLocal);
                                Assert.AreEqual(DateTime.Parse("2012-06-21 10:54:20Z"), cab.DateModifiedLocal);
                                Assert.AreEqual(eventId, cab.EventId);
                                Assert.AreEqual("Crash32", cab.EventTypeName);
                                Assert.AreEqual(cabId++, cab.Id);
                                Assert.AreEqual("CAB_123456.cab", cab.FileName);
                                Assert.AreEqual(10300, cab.SizeInBytes);
                            }
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
}
