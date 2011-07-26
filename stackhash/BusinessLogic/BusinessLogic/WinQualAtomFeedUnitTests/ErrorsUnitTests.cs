using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

using StackHashBusinessObjects;
using WinQualAtomFeed;
using StackHashWinQual;

namespace WinQualAtomFeedUnitTests
{
    /// <summary>
    /// Summary description for ErrorsUnitTests
    /// </summary>
    [TestClass]
    public class ErrorsUnitTests
    {
        public ErrorsUnitTests()
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
        public void ProcessErrorResponse()
        {
            // This is a valid error response returned from the server.
            //<?xml version="1.0" encoding="utf-8" ?> 
            //- <feed xmlns="http://www.w3.org/2005/Atom" xmlns:wer="http://schemas.microsoft.com/windowserrorreporting" wer:status="error">
            //    <title>Feed Error</title> 
            //    <link rel="alternate" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/downloadcab.aspx?cabID=837908536&eventid=1099298431&eventtypename=CLR20 Managed Crash&size=4631248" /> 
            //    <updated>2010-07-08 18:51:41Z</updated> 
            //    <id>Error</id> 
            //-   <entry>
            //      <updated>2010-07-08 18:51:41Z</updated> 
            //      <published>2010-07-08 18:51:41Z</published> 
            //      <title>Unhandled exception has occured during the processing of your request.</title> 
            //      <id>System.Exception</id> 
            //      <wer:additionalInformation /> 
            //     </entry>
            //  </feed>

            byte[] allBytes = {60,63,120,109,108,32,118,101,114,115,105,111,110,61,34,49,46,48,34,32,101,110,99,111,100,105,110,103,61,34,117,116,102,45,56,34,32,63,62,60,102,101,101,100,32,120,109,108,110,115,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,50,48,48,53,47,65,116,111,109,34,32,120,109,108,110,115,58,119,101,114,61,34,104,116,116,112,58,47,47,115,99,104,101,109,97,115,46,109,105,99,114,111,115,111,102,116,46,99,111,109,47,119,105,110,100,111,119,115,101,114,114,111,114,114,101,112,111,114,116,105,110,103,34,32,119,101,114,58,115,116,97,116,117,115,61,34,101,114,114,111,114,34,62,60,116,105,116,108,101,62,70,101,101,100,32,69,114,114,111,114,60,47,116,105,116,108,101,62,60,108,105,110,107,32,114,101,108,61,34,97,108,116,101,114,110,97,116,101,34,32,116,121,112,101,61,34,116,101,120,116,47,104,116,109,108,34,32,104,114,101,102,61,34,104,116,116,112,115,58,47,47,119,105,110,113,117,97,108,46,109,105,99,114,111,115,111,102,116,46,99,111,109,47,83,101,114,118,105,99,101,115,47,119,101,114,47,117,115,101,114,47,100,111,119,110,108,111,97,100,99,97,98,46,97,115,112,120,63,99,97,98,73,68,61,56,51,55,57,48,56,53,51,54,38,97,109,112,59,101,118,101,110,116,105,100,61,49,48,57,57,50,57,56,52,51,49,38,97,109,112,59,101,118,101,110,116,116,121,112,101,110,97,109,101,61,67,76,82,50,48,32,77,97,110,97,103,101,100,32,67,114,97,115,104,38,97,109,112,59,115,105,122,101,61,52,54,51,49,50,52,56,34,47,62,60,117,112,100,97,116,101,100,62,50,48,49,48,45,48,55,45,48,56,32,49,56,58,53,49,58,52,49,90,60,47,117,112,100,97,116,101,100,62,60,105,100,62,69,114,114,111,114,60,47,105,100,62,60,101,110,116,114,121,62,60,117,112,100,97,116,101,100,62,50,48,49,48,45,48,55,45,48,56,32,49,56,58,53,49,58,52,49,90,60,47,117,112,100,97,116,101,100,62,60,112,117,98,108,105,115,104,101,100,62,50,48,49,48,45,48,55,45,48,56,32,49,56,58,53,49,58,52,49,90,60,47,112,117,98,108,105,115,104,101,100,62,60,116,105,116,108,101,62,85,110,104,97,110,100,108,101,100,32,101,120,99,101,112,116,105,111,110,32,104,97,115,32,111,99,99,117,114,101,100,32,100,117,114,105,110,103,32,116,104,101,32,112,114,111,99,101,115,115,105,110,103,32,111,102,32,121,111,117,114,32,114,101,113,117,101,115,116,46,60,47,116,105,116,108,101,62,60,105,100,62,83,121,115,116,101,109,46,69,120,99,101,112,116,105,111,110,60,47,105,100,62,60,119,101,114,58,97,100,100,105,116,105,111,110,97,108,73,110,102,111,114,109,97,116,105,111,110,62,60,47,119,101,114,58,97,100,100,105,116,105,111,110,97,108,73,110,102,111,114,109,97,116,105,111,110,62,60,47,101,110,116,114,121,62,60,47,102,101,101,100,62 };

            try
            {
                AtomFeed.ProcessResponseError("testuri", allBytes, allBytes.Length);
            }
            catch (StackHashException ex)
            {
                String expectedMessage = @"Unhandled exception has occured during the processing of your request.";
                Assert.AreEqual(expectedMessage, ex.Message);
                Assert.AreEqual(expectedMessage, ex.InnerException.Message);
            }

        }
        [TestMethod]
        public void ProcessErrorResponseExtraZeros()
        {
            // This is a valid error response returned from the server.
            //<?xml version="1.0" encoding="utf-8" ?> 
            //- <feed xmlns="http://www.w3.org/2005/Atom" xmlns:wer="http://schemas.microsoft.com/windowserrorreporting" wer:status="error">
            //    <title>Feed Error</title> 
            //    <link rel="alternate" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/downloadcab.aspx?cabID=837908536&eventid=1099298431&eventtypename=CLR20 Managed Crash&size=4631248" /> 
            //    <updated>2010-07-08 18:51:41Z</updated> 
            //    <id>Error</id> 
            //-   <entry>
            //      <updated>2010-07-08 18:51:41Z</updated> 
            //      <published>2010-07-08 18:51:41Z</published> 
            //      <title>Unhandled exception has occured during the processing of your request.</title> 
            //      <id>System.Exception</id> 
            //      <wer:additionalInformation /> 
            //     </entry>
            //  </feed>

            byte[] allBytes = { 60, 63, 120, 109, 108, 32, 118, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 48, 34, 32, 101, 110, 99, 111, 100, 105, 110, 103, 61, 34, 117, 116, 102, 45, 56, 34, 32, 63, 62, 60, 102, 101, 101, 100, 32, 120, 109, 108, 110, 115, 61, 34, 104, 116, 116, 112, 58, 47, 47, 119, 119, 119, 46, 119, 51, 46, 111, 114, 103, 47, 50, 48, 48, 53, 47, 65, 116, 111, 109, 34, 32, 120, 109, 108, 110, 115, 58, 119, 101, 114, 61, 34, 104, 116, 116, 112, 58, 47, 47, 115, 99, 104, 101, 109, 97, 115, 46, 109, 105, 99, 114, 111, 115, 111, 102, 116, 46, 99, 111, 109, 47, 119, 105, 110, 100, 111, 119, 115, 101, 114, 114, 111, 114, 114, 101, 112, 111, 114, 116, 105, 110, 103, 34, 32, 119, 101, 114, 58, 115, 116, 97, 116, 117, 115, 61, 34, 101, 114, 114, 111, 114, 34, 62, 60, 116, 105, 116, 108, 101, 62, 70, 101, 101, 100, 32, 69, 114, 114, 111, 114, 60, 47, 116, 105, 116, 108, 101, 62, 60, 108, 105, 110, 107, 32, 114, 101, 108, 61, 34, 97, 108, 116, 101, 114, 110, 97, 116, 101, 34, 32, 116, 121, 112, 101, 61, 34, 116, 101, 120, 116, 47, 104, 116, 109, 108, 34, 32, 104, 114, 101, 102, 61, 34, 104, 116, 116, 112, 115, 58, 47, 47, 119, 105, 110, 113, 117, 97, 108, 46, 109, 105, 99, 114, 111, 115, 111, 102, 116, 46, 99, 111, 109, 47, 83, 101, 114, 118, 105, 99, 101, 115, 47, 119, 101, 114, 47, 117, 115, 101, 114, 47, 100, 111, 119, 110, 108, 111, 97, 100, 99, 97, 98, 46, 97, 115, 112, 120, 63, 99, 97, 98, 73, 68, 61, 56, 51, 55, 57, 48, 56, 53, 51, 54, 38, 97, 109, 112, 59, 101, 118, 101, 110, 116, 105, 100, 61, 49, 48, 57, 57, 50, 57, 56, 52, 51, 49, 38, 97, 109, 112, 59, 101, 118, 101, 110, 116, 116, 121, 112, 101, 110, 97, 109, 101, 61, 67, 76, 82, 50, 48, 32, 77, 97, 110, 97, 103, 101, 100, 32, 67, 114, 97, 115, 104, 38, 97, 109, 112, 59, 115, 105, 122, 101, 61, 52, 54, 51, 49, 50, 52, 56, 34, 47, 62, 60, 117, 112, 100, 97, 116, 101, 100, 62, 50, 48, 49, 48, 45, 48, 55, 45, 48, 56, 32, 49, 56, 58, 53, 49, 58, 52, 49, 90, 60, 47, 117, 112, 100, 97, 116, 101, 100, 62, 60, 105, 100, 62, 69, 114, 114, 111, 114, 60, 47, 105, 100, 62, 60, 101, 110, 116, 114, 121, 62, 60, 117, 112, 100, 97, 116, 101, 100, 62, 50, 48, 49, 48, 45, 48, 55, 45, 48, 56, 32, 49, 56, 58, 53, 49, 58, 52, 49, 90, 60, 47, 117, 112, 100, 97, 116, 101, 100, 62, 60, 112, 117, 98, 108, 105, 115, 104, 101, 100, 62, 50, 48, 49, 48, 45, 48, 55, 45, 48, 56, 32, 49, 56, 58, 53, 49, 58, 52, 49, 90, 60, 47, 112, 117, 98, 108, 105, 115, 104, 101, 100, 62, 60, 116, 105, 116, 108, 101, 62, 85, 110, 104, 97, 110, 100, 108, 101, 100, 32, 101, 120, 99, 101, 112, 116, 105, 111, 110, 32, 104, 97, 115, 32, 111, 99, 99, 117, 114, 101, 100, 32, 100, 117, 114, 105, 110, 103, 32, 116, 104, 101, 32, 112, 114, 111, 99, 101, 115, 115, 105, 110, 103, 32, 111, 102, 32, 121, 111, 117, 114, 32, 114, 101, 113, 117, 101, 115, 116, 46, 60, 47, 116, 105, 116, 108, 101, 62, 60, 105, 100, 62, 83, 121, 115, 116, 101, 109, 46, 69, 120, 99, 101, 112, 116, 105, 111, 110, 60, 47, 105, 100, 62, 60, 119, 101, 114, 58, 97, 100, 100, 105, 116, 105, 111, 110, 97, 108, 73, 110, 102, 111, 114, 109, 97, 116, 105, 111, 110, 62, 60, 47, 119, 101, 114, 58, 97, 100, 100, 105, 116, 105, 111, 110, 97, 108, 73, 110, 102, 111, 114, 109, 97, 116, 105, 111, 110, 62, 60, 47, 101, 110, 116, 114, 121, 62, 60, 47, 102, 101, 101, 100, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            try
            {
                AtomFeed.ProcessResponseError("testuri", allBytes, 716);
            }
            catch (StackHashException ex)
            {
                String expectedMessage = @"Unhandled exception has occured during the processing of your request.";
                Assert.AreEqual(expectedMessage, ex.Message);
                Assert.AreEqual(expectedMessage, ex.InnerException.Message);
            }

        }
    }
}
