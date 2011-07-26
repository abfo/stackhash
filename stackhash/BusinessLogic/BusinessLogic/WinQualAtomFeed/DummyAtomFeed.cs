using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;


using StackHashBusinessObjects;
using StackHashUtilities;


namespace WinQualAtomFeed
{
    public class DummyAtomFeed : IAtomFeed
    {
        private bool m_LogDetails;
        private String m_WinQualIPAddress;
        private bool m_AbortRequested;

        private int m_RequestTimeout;
        private int m_RequestRetryLimit;
        private String m_UserName;
        private String m_Password;

        private String m_TestFile;

        private TestDatabase m_TestDatabase;


        public String UserName
        {
            set { m_UserName = value; }
        }

        public String Password
        {
            set { m_Password = value; }
        }


        /// <summary>
        /// Aborts the currently running operation if there is one.
        /// </summary>
        public void AbortCurrentOperation()
        {
            m_AbortRequested = true;
        }


        /// <summary>
        /// Creates and instance of the Atom Feed using the proxy settings.
        /// </summary>
        /// <param name="proxySettings">Proxy settings to use on each call.</param>
        /// <param name="requestRetryCount">Number of times to retry a request before bombing out.</param>
        /// <param name="requestTimeout">Length of time in milliseconds to wait for response.</param>
        /// <param name="logDetails">True - log detailed output, False - don't.</param>
        /// <param name="useLiveId">True - user Windows Live ID, False - use WinQual Ticket.</param>
        /// <param name="winQualIPAddress">IP address to use to connect to the WinQual service - can be null.</param>
        public DummyAtomFeed(StackHashProxySettings proxySettings, int requestRetryCount, int requestTimeout, bool logDetails, bool useLiveId, String winQualIPAddress)
        {
            m_LogDetails = logDetails;

            if (requestRetryCount == 0)
                m_RequestRetryLimit = 5;
            else
                m_RequestRetryLimit = requestRetryCount;

            if (requestTimeout == 0)
                m_RequestTimeout = 300000;
            else
                m_RequestTimeout = requestTimeout;

            m_WinQualIPAddress = winQualIPAddress;


            // Get the test settings.
            m_TestFile = TestSettings.GetAttribute("DummyAtomFeedTestFile");


            m_TestDatabase = new TestDatabase(m_TestFile);
        }


        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        public void SetProxySettings(StackHashProxySettings proxySettings)
        {
        }


        /// <summary>
        /// Logs in to the WinQual service.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>true - success, false - failed to login.</returns>
        public bool Login(string userName, string password)
        {
            return true;
        }

        /// <summary>
        /// Logs out of the WinQual service.
        /// </summary>
        /// <returns>true - success, false - failed to login.</returns>
        public void LogOut()
        {
        }


        /// <summary>
        /// Get the product information associated with this WinQual logon.
        /// </summary>
        /// <returns>Collection of product data.</returns>
        public AtomProductCollection GetProducts()
        {
            AtomProductCollection products = new AtomProductCollection();

            return m_TestDatabase.GetProducts();
        }


        /// <summary>
        /// Get a list of all files associated with a product.
        /// </summary>
        /// <param name="product">Product for which the files are required.</param>
        /// <returns>List of files.</returns>
        public AtomFileCollection GetFiles(AtomProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (String.IsNullOrEmpty(product.Product.FilesLink))
                throw new ArgumentException("No files link present", "product");

            AtomFileCollection files = new AtomFileCollection();

            return m_TestDatabase.GetFiles(product.Product.Id);
        }


        /// <summary>
        /// Gets a list of events for the specified file.
        /// </summary>
        /// <param name="file">The file to get the events for.</param>
        /// <returns>Collection of events for the file.</returns>
        public AtomEventCollection GetEvents(AtomFile file)
        {
            DateTime startDate = DateTime.Now.AddDays(-89);
            AtomEventCollection allEvents = new AtomEventCollection();
            AtomEventCollection currentEvents = new AtomEventCollection();

            return m_TestDatabase.GetEvents(file.ProductId, file.File.Id);
        }


        /// <summary>
        /// Gets a list of events for the specified file.
        /// </summary>
        /// <param name="file">The file to get the events for.</param>
        /// <param name="startTime">Time of first event we are interested in.</param>
        /// <returns>Collection of events for the file.</returns>
        public AtomEventCollection GetEvents(AtomFile file, DateTime startTime)
        {
            AtomEventCollection allEvents = new AtomEventCollection();
            AtomEventCollection currentEvents = new AtomEventCollection();


            return m_TestDatabase.GetEvents(file.ProductId, file.File.Id);
        }


        /// <summary>
        /// Gets a list of events in the specified PAGE for the specified file.
        /// </summary>
        /// <param name="eventPageUrl">Url of the page to get.</param>
        /// <param name="file">File for which the events are required.</param>
        /// <param name="totalPages">Total number of pages.</param>
        /// <param name="currentPage">Current page.</param>
        /// <returns>Collection of events for the file.</returns>
        public AtomEventCollection GetEventsPage(ref String eventPageUrl, AtomFile file, out int totalPages, out int currentPage)
        {
            AtomEventCollection events = new AtomEventCollection();

            totalPages = 1;
            currentPage = 0;

            eventPageUrl = null;

            return m_TestDatabase.GetEvents(file.ProductId, file.File.Id);
        }


        /// <summary>
        /// Returns a list of event infos for the specified event.
        /// </summary>
        /// <param name="theEvent">Event for which the event info is required.</param>
        /// <param name="days">Number of days of events to get - max is 90.</param>
        /// <returns>Collection of event infos.</returns>
        public AtomEventInfoCollection GetEventDetails(AtomEvent theEvent, int days)
        {
            AtomEventInfoCollection eventInfos = new AtomEventInfoCollection();

            return m_TestDatabase.GetEventInfos(theEvent.ProductId, theEvent.FileId, theEvent.Event.Id);
        }


        /// <summary>
        /// Returns a list of Cab data.
        /// </summary>
        /// <param name="theEvent">Event for which the event info is required.</param>
        /// <returns>Collection of event infos.</returns>
        public AtomCabCollection GetCabs(AtomEvent theEvent)
        {
            AtomCabCollection cabs = new AtomCabCollection();

            return m_TestDatabase.GetCabs(theEvent.ProductId, theEvent.FileId, theEvent.Event.Id);
        }



        /// <summary>
        /// Download Cab to the specified filename.
        /// </summary>
        /// <param name="cab">Cab data to download.</param>
        /// <param name="overwrite">True - overwrite, false - don't overwrite.</param>
        /// <param name="folder">Full path of the cab.</param>
        /// <returns>Filename.</returns>
        public String DownloadCab(AtomCab cab, bool overwrite, String folder)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (folder == null)
                throw new ArgumentNullException("folder");

            // No abort requested yet.
            m_AbortRequested = false;

            if (!Directory.Exists(folder))
                throw new ArgumentException("Directory must exist: " + folder, "folder");

            String fullFileName = Path.Combine(folder, cab.Cab.FileName);

            if (File.Exists(fullFileName) && !overwrite)
            {
                FileAttributes attributes = File.GetAttributes(fullFileName);
                File.SetAttributes(fullFileName, attributes & ~FileAttributes.ReadOnly);
                return fullFileName;
            }

            try
            {
                if (m_AbortRequested)
                    throw new OperationCanceledException("Downloading cab aborted");

                String sourceCabFile = TestSettings.GetAttribute("TestCabFile");

                File.Copy(sourceCabFile, fullFileName);

                FileAttributes attributes = File.GetAttributes(fullFileName);
                File.SetAttributes(fullFileName, attributes & ~FileAttributes.ReadOnly);

                return fullFileName;
            }
            catch (System.Exception ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.CabDownloadFailed);
            }
        }


        /// <summary>
        /// Sets the web request settings.
        /// </summary>
        /// <param name="requestRetryCount">Number of times to retry following a timeout failure.</param>
        /// <param name="requestTimeout">Time to wait for a single response in milliseconds.</param>
        public void SetWebRequestSettings(int requestRetryCount, int requestTimeout)
        {
            if (requestRetryCount == 0)
                m_RequestRetryLimit = 5;
            else
                m_RequestRetryLimit = requestRetryCount;

            if (requestTimeout == 0)
                m_RequestTimeout = 300000;
            else
                m_RequestTimeout = requestTimeout;
        }


        /// <summary>
        /// Upload file to the WinQual service.
        /// </summary>
        /// <param name="fileName">The name of the file to upload.</param>
        public void UploadFile(String fileName)
        {
        }
    }
}
