using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

using StackHashBusinessObjects;
using StackHashUtilities;


namespace WinQualAtomFeed
{
    public enum RequestType
    {
        Get,
        Post,
    }

    public class AtomFeed : IWebCalls, IAtomFeed
    {
        private bool m_LogDetails;
        private String m_WinQualIPAddress;
        private WebProxy m_Proxy;
        private bool m_AbortRequested;
        private DateTime m_LastSuccessfulLoginTime;
        private int m_NumberOfLogins;
        private int m_IntervalBetweenWinQualLogonsInHours;

        //private const String s_WinQualLoginUrl = "https://131.107.97.31/services/Authentication/Authentication.svc/BasicTicket";
        //private const String s_WinQualLoginSoapAction = "https://131.107.97.31/Services/Authentication/IBasicTicket/GetBasicTicket";
        //private const String s_WinQualProductsUrl = "https://131.107.97.31/services/wer/user/products.aspx";
        //private const String s_WinQualLoginUrl = "https://207.46.52.39/services/Authentication/Authentication.svc/BasicTicket";
        //private const String s_WinQualLoginSoapAction = "https://207.46.52.39/Services/Authentication/IBasicTicket/GetBasicTicket";
        //private const String s_WinQualProductsUrl = "https://207.46.52.39/services/wer/user/products.aspx";
        //private const String s_WinQualLoginUrl = "https://94.245.126.19/services/Authentication/Authentication.svc/BasicTicket";
        //private const String s_WinQualLoginSoapAction = "https://94.245.126.19/Services/Authentication/IBasicTicket/GetBasicTicket";
        //private const String s_WinQualProductsUrl = "https://94.245.126.19/services/wer/user/products.aspx";
        private const String s_WinQualProductsUrl = "https://winqual.microsoft.com/services/wer/user/products.aspx";
        private const String s_WinQualUploadFileUrl = "https://winqual.microsoft.com/services/wer/user/fileupload.aspx";
        private static XNamespace s_AtomNamespace = "http://www.w3.org/2005/Atom";
        private static XNamespace s_WerNamespace = "http://schemas.microsoft.com/windowserrorreporting";
        private int m_RequestTimeout;
        private int m_RequestRetryLimit;
        private byte [] m_ExpectedXmlHeader = {60, 63, 120, 109, 108, 32, 118, 101, 114};
        private String m_UserName;
        private String m_Password;
        private int m_LastCallRetryCount;
        private ILogOn m_LogOn;

        public String UserName
        {
            set { m_UserName = value; }
        }

        public String Password
        {
            set { m_Password = value; }
        }

        /// <summary>
        ///  Called to create a proxy for use with the webrequest.
        /// </summary>
        /// <param name="proxySettings">Credentials and host for the proxy.</param>
        private void createProxy(StackHashProxySettings proxySettings)
        {
            m_Proxy = null;

            try
            {
                if ((proxySettings != null) && (proxySettings.UseProxy))
                {
                    m_Proxy = new WebProxy(proxySettings.ProxyHost, proxySettings.ProxyPort);

                    if (proxySettings.UseProxyAuthentication)
                    {
                        if (String.IsNullOrEmpty(proxySettings.ProxyDomain))
                            m_Proxy.Credentials = new NetworkCredential(proxySettings.ProxyUserName, proxySettings.ProxyPassword);
                        else
                            m_Proxy.Credentials = new NetworkCredential(proxySettings.ProxyUserName, proxySettings.ProxyPassword, proxySettings.ProxyDomain);

                        m_Proxy.UseDefaultCredentials = false;
                    }
                    else
                    {
                        m_Proxy.UseDefaultCredentials = true;
                    }

                    System.Net.WebRequest.DefaultWebProxy = m_Proxy;
                }
                else
                {
                    // no proxy server specified by StackHash, use IE settings
                    System.Net.WebRequest.DefaultWebProxy = System.Net.WebRequest.GetSystemWebProxy();
                }

            }

            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Proxy error", ex);
                m_Proxy = null;
            }
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
        /// <param name="intervalBetweenWinQualLogonsInHours">Interval between each log-on to WinQual.</param>
        public AtomFeed(StackHashProxySettings proxySettings, int requestRetryCount, int requestTimeout, bool logDetails, bool useLiveId,
            String winQualIPAddress, int intervalBetweenWinQualLogonsInHours)
        {
            m_IntervalBetweenWinQualLogonsInHours = intervalBetweenWinQualLogonsInHours;

            m_LogDetails = logDetails;

            if (requestRetryCount == 0)
                m_RequestRetryLimit = 5;
            else
                m_RequestRetryLimit = requestRetryCount;

            if (requestTimeout == 0)
                m_RequestTimeout = 300000;
            else
                m_RequestTimeout = requestTimeout;

            createProxy(proxySettings);

            m_WinQualIPAddress = winQualIPAddress;

            // Create a logon object to control all logon functions.
            if (!useLiveId)
                m_LogOn = new LogOnTicket();
            else
                m_LogOn = new LogOnLiveId();

            m_LogOn.Initialise(this);
        }


        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        public void SetProxySettings(StackHashProxySettings proxySettings)
        {
            createProxy(proxySettings);
        }


        /// <summary>
        /// Logs in to the WinQual service.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>true - success, false - failed to login.</returns>
        public bool Login(string userName, string password)
        {
            // Store these in case a retry is necessary.
            m_UserName = userName;
            m_Password = password;
            bool result = m_LogOn.LogIn(userName, password);

            if (result)
            {
                m_LastSuccessfulLoginTime = DateTime.Now;
                m_NumberOfLogins++;
            }

            return result;
        }

        /// <summary>
        /// Logs out of the WinQual service.
        /// </summary>
        /// <returns>true - success, false - failed to login.</returns>
        public void LogOut()
        {
            m_LogOn.LogOut();
        }

        /// <summary>
        /// Throw an exception depending on the feedNode details.
        ///</summary>
        /// <param name="feedNode">XmlNode for the feed.</param>
        public static void ThrowException(XElement feedNode)
        {
            /// Example exception...
            /// <feed xmlns="http://www.w3.org/2005/Atom" xmlns:wer="http://schemas.microsoft.com/windowserrorreporting" wer:status="error">
            ///  <title>Feed Error</title> 
            ///  <link rel="alternate" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/events.aspx?fileid=2422591&startdate=2010-04-02T10:53:20Z2010-04-02T10:53:20Z" /> 
            ///  <updated>2010-06-30 10:54:20Z</updated> 
            ///  <id>Error</id> 
            ///  <entry>
            ///    <updated>2010-06-30 10:54:20Z</updated> 
            ///    <published>2010-06-30 10:54:20Z</published> 
            ///    <title>StartDate query string parameter value should be a valid date-time.</title> 
            ///    <id>Microsoft.WindowsErrorReporting.Services.Data.FeedException</id> 
            ///    <wer:additionalInformation /> 
            ///  </entry>
            /// </feed>        /// 
            // Get the Entry node - first one - should only be 1.
            XElement entryNode = feedNode.Element(s_AtomNamespace + "entry");

            if (entryNode == null)
                throw new StackHashException(@"WinQual error occurred. No entry node found", StackHashServiceErrorCode.InvalidFeedDetected);

            // Get the id of the feed element to decide the type of exception.
            XElement exceptionTypeNode = entryNode.Element(s_AtomNamespace + "id");

            string exceptionType = @"Unknown exception type";
            if (exceptionTypeNode != null)
                exceptionType = exceptionTypeNode.Value;

            // Get the title for the message of the exception.
            XElement messageNode = entryNode.Element(s_AtomNamespace + "title");

            string message = @"Unknown message";
            if (messageNode != null)
                message = messageNode.Value;

            Exception innerException = getInnerException(message, exceptionType, entryNode);

            switch (exceptionType)
            {
                case SpecialExceptionList.FeedException:
                    if (String.Compare(message,
                                       "The XML file uploaded is not valid. Please use the Microsoft Product Mapping Tool to generate the XML file.",
                                       StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new StackHashException(message, innerException, StackHashServiceErrorCode.InvalidMappingFileFormat);
                    }
                    else
                    {
                        throw new StackHashException(message, innerException, StackHashServiceErrorCode.ServerDown);
                    }

                case SpecialExceptionList.FeedAuthenticationException:
                    throw new StackHashException(message, innerException, StackHashServiceErrorCode.AuthenticationFailure);
                case SpecialExceptionList.FeedAccessException:
                    throw new StackHashException(message, innerException, StackHashServiceErrorCode.AccessDenied);
                case SpecialExceptionList.ArgumentOutOfRangeException:
                    throw new ArgumentOutOfRangeException(message, innerException);
                case SpecialExceptionList.FileUploadException:
                    throw new ArgumentOutOfRangeException(message, innerException);
                case SpecialExceptionList.SystemException:
                    throw new StackHashException(message, innerException, StackHashServiceErrorCode.ServerDown);
                default:
                    throw new StackHashException(message, innerException, StackHashServiceErrorCode.ServerDown);
            }
        }


        /// <summary>
        /// Create a proper exception based on the exception type passed.
        /// </summary>
        /// <param name="message">Exception Message</param>
        /// <param name="exceptionType">Exception Type</param>
        /// <param name="entryNode">Xml Node that contains Exception data</param>
        /// <returns>Returns the inner exception.</returns>
        private static Exception getInnerException(string message, string exceptionType, XElement entryNode)
        {
            Exception innerException = new Exception(message);

            XElement dataNode = entryNode.Element(s_WerNamespace + "additionalInformation");
            if (dataNode != null)
            {
                // Select data entries into an anonymous type list.
                var query = from data in dataNode.Descendants(s_WerNamespace + "data")
                            select new
                            {
                                Name = data.Element(s_WerNamespace + "name").Value,
                                Value = (String)data.Element(s_WerNamespace + "value").Value
                            };

                foreach (var item in query)
                {
                    innerException.Data.Add(item.Name, item.Value);
                }
            }

            return innerException;
        }

        /// <summary>
        /// Method to get the feed node from the feed XML.
        /// </summary>
        /// <param name="feedXML">String containing the XML from the feed.</param>
        /// <returns>Gets the feed node of the XML.</returns>
        private static XElement getFeedNode(string feedXML)
        {
            // Example response.
            //<?xml version="1.0" encoding="utf-8" ?> 
            //- <feed xmlns="http://www.w3.org/2005/Atom" xmlns:wer="http://schemas.microsoft.com/windowserrorreporting" wer:status="ok">
            //  <title>Product List</title> 
            //  <updated>2010-06-24 07:00:00Z</updated> 
            //  <id>https://winqual.microsoft.com/services/wer/user/products.aspx</id> 
            //  <link rel="alternate" type="text/html" title="" href="https://winqual.microsoft.com/services/wer/user/products.aspx" /> 
            //- <entry>
            // ...

            // Parse the response into an XML document.
            XDocument xDoc = XDocument.Parse(feedXML);

            // Get the main feed node.
            XElement feedNode = xDoc.Element(s_AtomNamespace + "feed");

            if (feedNode == null)
                throw new StackHashException(@"WinQual error. No feed node found", StackHashServiceErrorCode.InvalidFeedDetected);

            // Check the status. "ok" or "error" are possibilities.
            String status = (String)feedNode.Attribute(s_WerNamespace + "status");

            if (status == null)
                throw new StackHashException(@"WinQual error. No status field found", StackHashServiceErrorCode.InvalidFeedDetected);

            bool validFeed = (status == "ok" ? true : false);

            if (validFeed == false)
            {
                try
                {
                    ThrowException(feedNode);
                }
                catch (System.Exception ex)
                {
                    // Log the exception first.
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, "Atom feed error: " + feedXML, ex);

                    throw;
                }
            }

            return feedNode;
        }


        /// <summary>
        /// Get the product information associated with this WinQual logon.
        /// </summary>
        /// <returns>Collection of product data.</returns>
        public AtomProductCollection GetProducts()
        {
            AtomProductCollection products = new AtomProductCollection();

            // Get the list of products from the WinQual service.
            String productsXml = WinQualCallWithRetry(s_WinQualProductsUrl, RequestType.Get, null, null);

            // Root node is 
            // <feed xmlns="http://www.w3.org/2005/Atom" xmlns:wer="http://schemas.microsoft.com/windowserrorreporting" wer:status="ok">

            // sample entry:
            //<entry>
            // <title>Cucku Backup</title> 
            // <id>15206</id> 
            // <link rel="related" title="Files for Product ID 15206" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/files.aspx?productid=15206" /> 
            // <updated>2010-06-09 07:01:58Z</updated> 
            // <published>2009-05-05 18:41:26Z</published> 
            // <wer:productVersion>2.00</wer:productVersion> 
            // <wer:totalResponses>2</wer:totalResponses> 
            // <wer:totalEvents>3</wer:totalEvents> 
            // </entry>


            // Get the feed node - throws an exception if an error occurs.
            XElement feedNode = getFeedNode(productsXml);

            // Updated
            DateTime updated = DateTime.Parse(feedNode.Element(s_AtomNamespace + "updated").Value);

            products.DateFeedUpdated = updated;

            // Select entries - returns a list of anonymous types build as below.
            var query = from entry in feedNode.Descendants(s_AtomNamespace + "entry")
                        select new
                        {
                            ProductName = entry.Element(s_AtomNamespace + "title").Value,
                            ProductId = Int32.Parse(entry.Element(s_AtomNamespace + "id").Value),
                            FilesUrl = entry.Element(s_AtomNamespace + "link").Attribute("href").Value,
                            Updated = DateTime.Parse(entry.Element(s_AtomNamespace + "updated").Value),
                            Created = DateTime.Parse(entry.Element(s_AtomNamespace + "published").Value),
                            ProductVersion = entry.Element(s_WerNamespace + "productVersion").Value,
                            TotalResponses = Int32.Parse(entry.Element(s_WerNamespace + "totalResponses").Value),
                            TotalEvents = Int32.Parse(entry.Element(s_WerNamespace + "totalEvents").Value),
                        };


            // Parse all into a StackHashProduct structure.
            foreach (var item in query)
            {
                DateTime dateCreated = item.Created;
                if (dateCreated.Kind == DateTimeKind.Local)
                    dateCreated = dateCreated.ToUniversalTime();

                DateTime dateUpdated = item.Updated;
                if (dateUpdated.Kind == DateTimeKind.Local)
                    dateUpdated = dateUpdated.ToUniversalTime();

                StackHashProduct product =
                    new StackHashProduct(dateCreated, dateUpdated, item.FilesUrl, item.ProductId, item.ProductName, 
                                         item.TotalEvents, item.TotalResponses, item.ProductVersion);

                products.Add(new AtomProduct(product));
            }

            return products;
        }


        /// <summary>
        /// Upload file to the WinQual service.
        /// </summary>
        /// <param name="fileName">The name of the file to upload.</param>
        public void UploadFile(String fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fullName");

            String fileData = File.ReadAllText(fileName);

            // Post the file to the file upload url.
//            String resultXml = WinQualCall(s_WinQualUploadFileUrl, RequestType.Post, fileData, null);
            String resultXml = WinQualClientUploadFile(fileName);
            getFeedNode(resultXml);
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

            //<entry>
            //<title>Diff.dll</title> 
            //<id>3855426</id> 
            //<link rel="related" title="Events for File ID 3855426" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/events.aspx?fileid=3855426" /> 
            //<updated>2010-02-08 23:34:00Z</updated> 
            //<published>2010-02-08 23:34:00Z</published> 
            //<wer:fileVersion>2.11.3610.1220</wer:fileVersion> 
            //<wer:fileLinkDate>2010-02-06 02:33:07Z</wer:fileLinkDate> 
            //</entry>

            String filesXml = WinQualCallWithRetry(product.Product.FilesLink, RequestType.Get, null, null);

            // Get the feed node - throws an exception if an error occurs.
            XElement feedNode = getFeedNode(filesXml);

            // Select entries into an anonymous type list.
            var query = from entry in feedNode.Descendants(s_AtomNamespace + "entry")
                        select new
                        {
                            FileName = entry.Element(s_AtomNamespace + "title").Value,
                            FileId = Int32.Parse(entry.Element(s_AtomNamespace + "id").Value),
                            EventsUrl = entry.Element(s_AtomNamespace + "link").Attribute("href").Value,
                            Updated = DateTime.Parse(entry.Element(s_AtomNamespace + "updated").Value),
                            Created = DateTime.Parse(entry.Element(s_AtomNamespace + "published").Value),
                            FileVersion = entry.Element(s_WerNamespace + "fileVersion").Value,
                            FileLinkDate = DateTime.Parse(entry.Element(s_WerNamespace + "fileLinkDate").Value),
                        };

            foreach (var item in query)
            {
                DateTime dateCreated = item.Created;
                if (dateCreated.Kind == DateTimeKind.Local)
                    dateCreated = dateCreated.ToUniversalTime();

                DateTime dateUpdated = item.Updated;
                if (dateUpdated.Kind == DateTimeKind.Local)
                    dateUpdated = dateUpdated.ToUniversalTime();

                DateTime fileLinkDate = item.FileLinkDate;
                if (fileLinkDate.Kind == DateTimeKind.Local)
                    fileLinkDate = fileLinkDate.ToUniversalTime();

                StackHashFile file = new StackHashFile(dateCreated, dateUpdated, item.FileId, fileLinkDate, item.FileName, item.FileVersion);
                files.Add(new AtomFile(file, item.EventsUrl));
            }

            return files;
        }


        /// <summary>
        /// Gets a list of events for the specified file.
        /// </summary>
        /// <param name="file">The file to get the events for.</param>
        /// <returns>Collection of events for the file.</returns>
        public AtomEventCollection GetEvents(AtomFile file)
        {
            //<wer:totalPages>1</wer:totalPages> 
            //<wer:currentPage>1</wer:currentPage> 

            //<entry>
            //<title>Event ID 1858690183 and Event Type Crash 32bit</title> 
            //<id>1858690183-Crash 32bit</id> 
            //<link rel="cabs" title="" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/cabs.aspx?eventid=1858690183&eventtypename=Crash 32bit" /> 
            //<link rel="details" title="" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/eventdetails.aspx?eventid=1858690183&eventtypename=Crash 32bit" /> 
            //<updated>2010-05-18 04:03:00Z</updated> 
            //<published>2010-05-18 04:03:00Z</published> 
            //<wer:totalHits>-1</wer:totalHits> 
            //<wer:eventID>1858690183</wer:eventID> 
            //<wer:eventTypeName>Crash 32bit</wer:eventTypeName> 
            //<wer:signature>
            //<wer:parameter wer:name="applicationName" wer:value="RunAs.exe" /> 
            //<wer:parameter wer:name="applicationVersion" wer:value="2.10.20509.1119" /> 
            //<wer:parameter wer:name="applicationTimeStamp" wer:value="0001-01-01T00:00:00" /> 
            //<wer:parameter wer:name="moduleName" wer:value="nview.dll" /> 
            //<wer:parameter wer:name="moduleVersion" wer:value="6.13.10.3190" /> 
            //<wer:parameter wer:name="moduleTimeStamp" wer:value="0001-01-01T00:00:00" /> 
            //<wer:parameter wer:name="exceptionCode" wer:value="0" /> 
            //<wer:parameter wer:name="offset" wer:value="0x9ddf" /> 
            //</wer:signature>
            //</entry>

            DateTime startDate = DateTime.Now.AddDays(-89);
            AtomEventPageReader eventPageReader = new AtomEventPageReader(this, file, startDate, DateTime.Now);
            AtomEventCollection allEvents = new AtomEventCollection();
            AtomEventCollection currentEvents = new AtomEventCollection();

            while ((currentEvents = eventPageReader.ReadPage()) != null)
            {
                // Add the events for the full list.
                foreach (AtomEvent theEvent in currentEvents)
                {
                    allEvents.Add(theEvent);
                }
            }

            return allEvents;
        }


        /// <summary>
        /// Gets a list of events for the specified file.
        /// </summary>
        /// <param name="file">The file to get the events for.</param>
        /// <param name="startTime">Time of first event we are interested in.</param>
        /// <returns>Collection of events for the file.</returns>
        public AtomEventCollection GetEvents(AtomFile file, DateTime startTime)
        {
            //<wer:totalPages>1</wer:totalPages> 
            //<wer:currentPage>1</wer:currentPage> 

            //<entry>
            //<title>Event ID 1858690183 and Event Type Crash 32bit</title> 
            //<id>1858690183-Crash 32bit</id> 
            //<link rel="cabs" title="" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/cabs.aspx?eventid=1858690183&eventtypename=Crash 32bit" /> 
            //<link rel="details" title="" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/eventdetails.aspx?eventid=1858690183&eventtypename=Crash 32bit" /> 
            //<updated>2010-05-18 04:03:00Z</updated> 
            //<published>2010-05-18 04:03:00Z</published> 
            //<wer:totalHits>-1</wer:totalHits> 
            //<wer:eventID>1858690183</wer:eventID> 
            //<wer:eventTypeName>Crash 32bit</wer:eventTypeName> 
            //<wer:signature>
            //<wer:parameter wer:name="applicationName" wer:value="RunAs.exe" /> 
            //<wer:parameter wer:name="applicationVersion" wer:value="2.10.20509.1119" /> 
            //<wer:parameter wer:name="applicationTimeStamp" wer:value="0001-01-01T00:00:00" /> 
            //<wer:parameter wer:name="moduleName" wer:value="nview.dll" /> 
            //<wer:parameter wer:name="moduleVersion" wer:value="6.13.10.3190" /> 
            //<wer:parameter wer:name="moduleTimeStamp" wer:value="0001-01-01T00:00:00" /> 
            //<wer:parameter wer:name="exceptionCode" wer:value="0" /> 
            //<wer:parameter wer:name="offset" wer:value="0x9ddf" /> 
            //</wer:signature>
            //</entry>

            AtomEventPageReader eventPageReader = new AtomEventPageReader(this, file, startTime, DateTime.Now);
            AtomEventCollection allEvents = new AtomEventCollection();
            AtomEventCollection currentEvents = new AtomEventCollection();

            while ((currentEvents = eventPageReader.ReadPage()) != null)
            {
                // Add the events for the full list.
                foreach (AtomEvent theEvent in currentEvents)
                {
                    allEvents.Add(theEvent);
                }
            }

            return allEvents;
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
            if (eventPageUrl == null)
                throw new ArgumentNullException("eventPageUrl");

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "GetEventsPage Url: " + eventPageUrl);

            AtomEventCollection events = new AtomEventCollection();

            //<wer:totalPages>1</wer:totalPages> 
            //<wer:currentPage>1</wer:currentPage> 
            //<entry>
            //<title>Event ID 1858690183 and Event Type Crash 32bit</title> 
            //<id>1858690183-Crash 32bit</id> 
            //<link rel="cabs" title="" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/cabs.aspx?eventid=1858690183&eventtypename=Crash 32bit" /> 
            //<link rel="details" title="" type="text/html" href="https://winqual.microsoft.com/Services/wer/user/eventdetails.aspx?eventid=1858690183&eventtypename=Crash 32bit" /> 
            //<updated>2010-05-18 04:03:00Z</updated> 
            //<published>2010-05-18 04:03:00Z</published> 
            //<wer:totalHits>-1</wer:totalHits> 
            //<wer:eventID>1858690183</wer:eventID> 
            //<wer:eventTypeName>Crash 32bit</wer:eventTypeName> 
            //<wer:signature>
            //<wer:parameter wer:name="applicationName" wer:value="RunAs.exe" /> 
            //<wer:parameter wer:name="applicationVersion" wer:value="2.10.20509.1119" /> 
            //<wer:parameter wer:name="applicationTimeStamp" wer:value="0001-01-01T00:00:00" /> 
            //<wer:parameter wer:name="moduleName" wer:value="nview.dll" /> 
            //<wer:parameter wer:name="moduleVersion" wer:value="6.13.10.3190" /> 
            //<wer:parameter wer:name="moduleTimeStamp" wer:value="0001-01-01T00:00:00" /> 
            //<wer:parameter wer:name="exceptionCode" wer:value="0" /> 
            //<wer:parameter wer:name="offset" wer:value="0x9ddf" /> 
            //</wer:signature>
            //</entry>
            totalPages = 0;
            currentPage = 0;


            String eventsXml = WinQualCallWithRetry(eventPageUrl, RequestType.Get, null, null);

            // Get the feed node - throws an exception if an error occurs.
            XElement feedNode = getFeedNode(eventsXml);


            totalPages = Int32.Parse(feedNode.Element(s_WerNamespace + "totalPages").Value);
            currentPage = Int32.Parse(feedNode.Element(s_WerNamespace + "currentPage").Value);

            // Check for no pages.
            if (totalPages == 0)
            {
                eventPageUrl = null;
                return events;
            }

            // Select entries from the result and add events for each.
            var query = from entry in feedNode.Descendants(s_AtomNamespace + "entry")
                        select new
                        {
                            EventTitle = entry.Element(s_AtomNamespace + "title").Value,
                            EventIdAndType = entry.Element(s_AtomNamespace + "id").Value,
                            CabsUrl = (from link in entry.Descendants(s_AtomNamespace + "link") where (string)link.Attribute("rel") == "cabs" select link).Single<XElement>().Attribute("href").Value,
                            EventDetailsUrl = (from link in entry.Descendants(s_AtomNamespace + "link") where link.Attribute("rel").Value == "details" select link).Single<XElement>().Attribute("href").Value,
                            Updated = DateTime.Parse(entry.Element(s_AtomNamespace + "updated").Value),
                            Created = DateTime.Parse(entry.Element(s_AtomNamespace + "published").Value),
                            TotalHits = Int32.Parse(entry.Element(s_WerNamespace + "totalHits").Value),
                            EventId = Int32.Parse(entry.Element(s_WerNamespace + "eventID").Value),
                            EventTypeName = entry.Element(s_WerNamespace + "eventTypeName").Value,

                            //                                ApplicationName = (from parameter in entry.Descendants(werns + "parameter") where parameter.Attribute(werns + "name").Value == "applicationName" select parameter).Single<XElement>().Attribute(werns + "value").Value,
                            Parameters = from parameter in entry.Descendants(s_WerNamespace + "parameter")
                                            select new
                                            {
                                                Name = parameter.Attribute(s_WerNamespace + "name").Value,
                                                Value = parameter.Attribute(s_WerNamespace + "value").Value,
                                            },
                        };

            foreach (var item in query)
            {
                DateTime dateCreated = item.Created;
                if (dateCreated.Kind == DateTimeKind.Local)
                    dateCreated = dateCreated.ToUniversalTime();

                DateTime dateUpdated = item.Updated;
                if (dateUpdated.Kind == DateTimeKind.Local)
                    dateUpdated = dateUpdated.ToUniversalTime();

                // Convert the parameters in the event signature.
                StackHashParameterCollection paramCollection = new StackHashParameterCollection();

                if (item.Parameters != null)
                {
                    foreach (var param in item.Parameters)
                    {
                        StackHashParameter stackHashParam = new StackHashParameter(param.Name, param.Value);

                        paramCollection.Add(stackHashParam);
                    }
                }
                StackHashEventSignature eventSignature = new StackHashEventSignature(paramCollection);
                StackHashEvent thisEvent = new StackHashEvent(dateCreated, dateUpdated, item.EventTypeName, item.EventId, eventSignature, item.TotalHits, file.File.Id);
                events.Add(new AtomEvent(thisEvent, item.EventDetailsUrl, item.CabsUrl));
            }

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "GetEventsPage Events: " + events.Count.ToString());

            // Try to find the next page.
            eventPageUrl = null;
            XElement nextElement = (from link in feedNode.Descendants(s_AtomNamespace + "link")
                                    where (String)link.Attribute("rel") == "next"
                                    select link).SingleOrDefault<XElement>();
            if (nextElement != null)
            {
                eventPageUrl = nextElement.Attribute("href").Value;
            }
            else
            {
                eventPageUrl = null; // Assume that's the end.
            }


            return events;
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

            //<entry>
            //<title>Total hits for date 2010-05-12 07:00:00Z</title> 
            //<id>2010-05-12 07:00:00Z</id> 
            //<updated>2010-05-12 07:00:00Z</updated> 
            //<published>2010-05-12 07:00:00Z</published> 
            //<wer:totalHits>1</wer:totalHits> 
            //<wer:operatingSystem wer:name="Windows XP SP3" wer:version="5.1.2600.2.3.0" /> 
            //<wer:language wer:name="English - United States" wer:lcid="1033" wer:locale="en-US" /> 
            //</entry>

            // Max out at 90 days on WinQual returns an error. 
            // TODO: This should be configurable in case it changes.
            int daysRequired = days;

            if (days > 90)
                daysRequired = 90;
            else if (days <= 0)
                daysRequired = 1;

            String fullLink = String.Format(CultureInfo.InvariantCulture, "{0}&days={1}", theEvent.EventInfoLink, daysRequired);

            if (m_LogDetails)
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Request: " + fullLink);

            String eventDetailsXml = WinQualCallWithRetry(fullLink, RequestType.Get, null, null);

            // Get the feed node - throws an exception if an error occurs.
            XElement feedNode = getFeedNode(eventDetailsXml);

            if (m_LogDetails)
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, eventDetailsXml);

            // Select the enties into a collection of anon types.
            var query = from entry in feedNode.Descendants(s_AtomNamespace + "entry")
                        select new
                        {
                            Title = entry.Element(s_AtomNamespace + "title").Value,
                            HitDate = DateTime.Parse(entry.Element(s_AtomNamespace + "id").Value),  // ID is the hit date.
                            Updated = DateTime.Parse(entry.Element(s_AtomNamespace + "updated").Value),
                            Created = DateTime.Parse(entry.Element(s_AtomNamespace + "published").Value),
                            TotalHits = Int32.Parse(entry.Element(s_WerNamespace + "totalHits").Value),
                            OperatingSystemName = entry.Element(s_WerNamespace + "operatingSystem").Attribute(s_WerNamespace + "name").Value,
                            OperatingSystemVersion = entry.Element(s_WerNamespace + "operatingSystem").Attribute(s_WerNamespace + "version").Value,
                            Language = entry.Element(s_WerNamespace + "language").Attribute(s_WerNamespace + "name").Value,
                            Lcid = Int32.Parse(entry.Element(s_WerNamespace + "language").Attribute(s_WerNamespace + "lcid").Value),
                            Locale = entry.Element(s_WerNamespace + "language").Attribute(s_WerNamespace + "locale").Value,
                        };

            foreach (var item in query)
            {
                DateTime dateCreated = item.Created;
                if (dateCreated.Kind == DateTimeKind.Local)
                    dateCreated = dateCreated.ToUniversalTime();

                DateTime dateUpdated = item.Updated;
                if (dateUpdated.Kind == DateTimeKind.Local)
                    dateUpdated = dateUpdated.ToUniversalTime();

                DateTime hitDate = item.HitDate;
                if (hitDate.Kind == DateTimeKind.Local)
                {
                    hitDate = hitDate.ToUniversalTime();
                }

                StackHashEventInfo eventInfo = new StackHashEventInfo(dateCreated, dateUpdated, hitDate, item.Language, 
                    item.Lcid, item.Locale, item.OperatingSystemName, item.OperatingSystemVersion, item.TotalHits);
                eventInfos.Add(new AtomEventInfo(eventInfo));
            }

            return eventInfos;
        }


        /// <summary>
        /// Returns a list of Cab data.
        /// </summary>
        /// <param name="theEvent">Event for which the event info is required.</param>
        /// <returns>Collection of event infos.</returns>
        public AtomCabCollection GetCabs(AtomEvent theEvent)
        {
            AtomCabCollection cabs = new AtomCabCollection();

            // Example link: "https://winqual.microsoft.com/Services/wer/user/cabs.aspx?eventid=1211773602&eventtypename=CLR20 Managed Crash"
            String cabsXml = WinQualCallWithRetry(theEvent.CabsLink, RequestType.Get, null, null);

            // Get the feed node - throws an exception if an error occurs.
            XElement feedNode = getFeedNode(cabsXml);


            // Select the enties into a collection of anon types.
            var query = from entry in feedNode.Descendants(s_AtomNamespace + "entry")
                        select new
                        {
                            Title = entry.Element(s_AtomNamespace + "title").Value,
                            Id = Int32.Parse(entry.Element(s_AtomNamespace + "id").Value),
                            CabLink = (from link in entry.Descendants(s_AtomNamespace + "link") where (string)link.Attribute("rel") == "enclosure" select link).Single<XElement>().Attribute("href").Value,
                            Updated = DateTime.Parse(entry.Element(s_AtomNamespace + "updated").Value),
                            Created = DateTime.Parse(entry.Element(s_AtomNamespace + "published").Value),
                            CabSize = Int32.Parse(entry.Element(s_WerNamespace + "cabSize").Value),
                            EventID = Int32.Parse(entry.Element(s_WerNamespace + "eventID").Value),
                            EventTypeName = entry.Element(s_WerNamespace + "eventTypeName").Value,
                            CabFileName = entry.Element(s_WerNamespace + "cabFileName").Value,
                        };

            foreach (var item in query)
            {
                DateTime dateCreated = item.Created;
                if (dateCreated.Kind == DateTimeKind.Local)
                    dateCreated = dateCreated.ToUniversalTime();

                DateTime dateUpdated = item.Updated;
                if (dateUpdated.Kind == DateTimeKind.Local)
                    dateUpdated = dateUpdated.ToUniversalTime();

                // Need to make the filename the same as for the API.
                String fileName = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", item.EventID, item.EventTypeName.Replace(" ", ""), item.CabFileName);

                StackHashCab cab = new StackHashCab(dateCreated, dateUpdated, item.EventID, item.EventTypeName,
                    fileName, item.Id, item.CabSize);
 
                cabs.Add(new AtomCab(cab, item.CabLink));
            }

            return cabs;
        }


        private Stream getResponseStream(HttpWebResponse webResponse)
        {
            if (webResponse.StatusCode != HttpStatusCode.OK)
            {                
                String webError;
                if (webResponse.StatusDescription != null)
                    webError = String.Format(CultureInfo.InvariantCulture, "WebResponse failed: {0} {1} {2}", webResponse.StatusCode, webResponse.StatusDescription, webResponse.ResponseUri);
                else
                    webError = String.Format(CultureInfo.InvariantCulture, "WebResponse failed: {0} {1}", webResponse.StatusCode, webResponse.ResponseUri);

                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, webError);
            }


            Stream responseStream = webResponse.GetResponseStream();

            return responseStream;
        }


        /// <summary>
        /// Determines if the specified buffer contains an XML header.
        /// </summary>
        /// <param name="buffer">Buffer to check.</param>
        /// <returns>True - contains header, false - doesn't</returns>
        private bool containsXmlHeader(byte [] buffer)
        {
            bool match = true;
            for (int i = 0; i < m_ExpectedXmlHeader.Length; i++)
            {
                if (buffer[i] != m_ExpectedXmlHeader[i])
                {
                    match = false;
                    break;
                }
            }

            return match;
        }


        /// <summary>
        /// Throws a feed exception using the data contained in the web response.
        /// </summary>
        /// <param name="uri">Web URI.</param>
        /// <param name="buffer">Full message read from the response stream.</param>
        /// <param name="bytesRead">Bytes in the buffer that are valid.</param>
        public static void ProcessResponseError(String uri, byte[] buffer, int bytesRead)
        {
            String responseXml;
            StringBuilder errorText = new StringBuilder("Feed Error: ");

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (uri != null)
            {
                errorText.Append(uri);
                errorText.Append("--");
            }

            // Turn the buffer into a text stream.
            using (MemoryStream responseStream = new MemoryStream(buffer))
            {
                // Don't read beyond the end of the useful buffer or an XML exception will occur later.
                responseStream.SetLength(bytesRead);

                using (StreamReader responseStreamReader = new StreamReader(responseStream))
                {
                    responseXml = responseStreamReader.ReadToEnd();
                }
            }

            // 
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

            // Get the feed node - throws an exception because an error is present.
            XElement feedNode = getFeedNode(responseXml);
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
                return fullFileName;

            HttpWebResponse webresponse = null;
            int retryCount = 0;

            try
            {
                do
                {
                    try
                    {
                        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(cab.CabLink);

                        // Set the method type as specified.
                        webrequest.Method = "GET";
                        webrequest.Timeout = m_RequestTimeout; // Default is only 100 seconds.

                        // The logon ticket belongs in the header.
                        m_LogOn.ProcessRequest(webrequest);


                        // NOTE - DO NOT set a UserAgent, the WinQual site redirects all requests to
                        // their help page if they think it's not from Internet Explorer.

                        // The response stream may return a text error as below or the binary file data.
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

                        webresponse = (HttpWebResponse)webrequest.GetResponse();

                        bool firstRead = true;
                        bool deleteFile = false;

                        using (Stream responseStream = getResponseStream(webresponse))
                        {
                            FileStream fileStream = null;

                            try
                            {
                                if (m_AbortRequested)
                                    throw new OperationCanceledException("Abort requested during cab download");

                                fileStream = new FileStream(fullFileName, FileMode.OpenOrCreate, FileAccess.Write);

                                int bufferLength = 24 * 1024;
                                Byte[] buffer = new byte[bufferLength];
                                int bytesRead = responseStream.Read(buffer, 0, bufferLength);

                                if (firstRead && (bytesRead < bufferLength) && (bytesRead != cab.Cab.SizeInBytes) && (containsXmlHeader(buffer)))
                                {
                                    deleteFile = true; // Error occurred during download so make sure the file is deleted..
                                    ProcessResponseError(webresponse.ResponseUri.ToString(), buffer, bytesRead);
                                }

                                firstRead = false;
                                while (bytesRead > 0)
                                {
                                    if (m_AbortRequested)
                                    {
                                        deleteFile = true; // Don't leave a partial file lying around.
                                        throw new OperationCanceledException("Abort requested during Win Qual synchronize");
                                    }

                                    fileStream.Write(buffer, 0, bytesRead);
                                    bytesRead = responseStream.Read(buffer, 0, bufferLength);
                                }
                            }
                            finally
                            {
                                if (fileStream != null)
                                    fileStream.Close();
                                if (deleteFile)
                                    if (File.Exists(fullFileName))
                                        File.Delete(fullFileName);
                            }
                        }

                        m_LogOn.ProcessResponse(webresponse);

                        return fullFileName;
                    }
                    catch (System.Exception ex)
                    {
                        if (m_AbortRequested)
                            throw new OperationCanceledException("Abort requested during cab download");

                        WebException webException = ex as WebException;
                        StackHashException stackHashException = ex as StackHashException;

                        bool logTrace = true;
                        if ((webException != null) && m_LogOn.ProcessWebException(webException))
                        {
                            // This is a challenge from the server for LiveID so don't log as an error.
                            logTrace = false;
                        }
                        
                        if ((webException != null) || (stackHashException != null))
                        {
                            retryCount++;
                            if (logTrace)
                            {
                                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "DownloadCab Url: " + cab.CabLink);
                                DiagnosticsHelper.LogException(DiagSeverity.Warning, "DownloadCab failed", ex);
                                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "DownloadCab failed: retryCount = " + retryCount.ToString(CultureInfo.InvariantCulture));
                            }

                            if (retryCount >= m_RequestRetryLimit)
                            {
                                if (ex.Message.Contains("The remote server returned an error: (404)"))
                                    throw new StackHashException("The WinQual service is currently unavailable for downloading events. Try again later or wait for the scheduled sync to occur", ex, StackHashServiceErrorCode.ServerDown);
                                else
                                    throw;
                            }

                            // About to retry. First check if the login has expired (i.e. the reason for the last failure.
                            if (stackHashException != null)
                            {
                                if (stackHashException.ServiceErrorCode == StackHashServiceErrorCode.AuthenticationFailure)
                                {
                                    m_LogOn.LogInWithException(m_UserName, m_Password);
                                }
                            }

                            // Ignore the error and retry.
                        }
                        else
                        {
                            // Unexpected error so fail the call.
                            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "DownloadCab Url: " + cab.CabLink);
                            DiagnosticsHelper.LogException(DiagSeverity.Warning, "DownloadCab failed", ex);
                            throw;
                        }
                    }
                    finally
                    {
                        if (webresponse != null)
                            webresponse.Close();
                    }

                } while ((retryCount < m_RequestRetryLimit) && (!m_AbortRequested));

                if (m_AbortRequested)
                    throw new OperationCanceledException("Abort requested during cab download");
            }
            catch (System.Exception ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.CabDownloadFailed);
            }

            return null;
        }



        /// <summary>
        /// Using a web client takes care of adding pre and post block headers to the data.
        /// UTF8 encoded Header: 
        /// -----------------------8cda56ba1025d1c\r\n
        /// Content-Disposition: form-data; name="file"; filename="1.0.4511.261.xml"\r\n
        /// Content-Type: application/octet-stream\r\n
        /// \r\n"
        /// ASCII Encoded Footer: \r\n
        /// ---------------------8cda56ba1025d1c\r\n
        /// </summary>
        /// <param name="fileName">File to send.</param>
        /// <returns>Response string.</returns>
        public String WinQualClientUploadFile(String fileName)
        {
            String response = null;

            m_AbortRequested = false;

            bool retry = false;

            do
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        m_LogOn.ProcessRequest(client.Headers);

                        byte [] bytes = client.UploadFile(s_WinQualUploadFileUrl, fileName);
                        response = Encoding.UTF8.GetString(bytes);

                        retry = false;
                    }
                }
                catch (WebException ex)
                {
                    if (m_LogOn.ProcessWebException(ex))
                    {
                        // Was a challenge error so send the request again.
                        retry = true;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (retry && !m_AbortRequested);

            if (m_AbortRequested)
                throw new OperationCanceledException("Abort requested during Win Qual file upload");

            return response;
        }


        /// <summary>
        /// Make a call to the WinQual service.
        /// </summary>
        /// <param name="url">Service URL</param>
        /// <param name="requestType">Get or Post</param>
        /// <param name="ticket">Ticket (null if no ticket yet)</param>
        /// <param name="postPayload">POST payload - if null then GET is assumed</param>
        /// <param name="soapAction">SOAP action header, currently used for authentication only, null otherwise</param>
        /// <returns>Full string response to the request.</returns>
        public String WinQualCall(String url, RequestType requestType, String postPayload, String soapAction)
        {
            String response = null;
            HttpWebResponse webresponse = null;
            bool retry = false;
            bool retried = false;

            do
            {
                try
                {
                    if (retry)
                        retried = true;

                    if (m_WinQualIPAddress != null)
                        url = url.Replace("winqual.microsoft.com", m_WinQualIPAddress);

                    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);

                    // Set the method type as specified.
                    webrequest.Method = (requestType == RequestType.Get) ? "GET" : "POST";
                    webrequest.Timeout = m_RequestTimeout; // Default is only 100 seconds.

                    // The logon authentication headers.
                    m_LogOn.ProcessRequest(webrequest);


                    // NOTE - DO NOT set a UserAgent, the WinQual site redirects all requests to
                    // their help page if they think it's not from Internet Explorer.

                    // Add POST to request.
                    if ((requestType == RequestType.Post) && (postPayload != null))
                    {
                        byte[] postPayloadBytes = Encoding.UTF8.GetBytes(postPayload);

                        webrequest.ContentType = "text/xml";
                        webrequest.ContentLength = postPayloadBytes.Length;

                        if (soapAction != null)
                            webrequest.Headers.Add("SOAPAction", soapAction);

                        using (Stream requestStream = webrequest.GetRequestStream())
                        {
                            requestStream.Write(postPayloadBytes, 0, postPayloadBytes.Length);
                        }
                    }

                    // read the response as a string
                    webresponse = (HttpWebResponse)webrequest.GetResponse();
                    using (Stream responseStream = webresponse.GetResponseStream())
                    {
                        using (StreamReader responseStreamReader = new StreamReader(responseStream))
                        {
                            response = responseStreamReader.ReadToEnd();
                        }
                    }

                    m_LogOn.ProcessResponse(webresponse);

                    // All worked - no need to retry again.
                    retry = false;
                }
                catch (WebException ex)
                {
                    // Only retry if not already retried.
                    if (m_LogOn.ProcessWebException(ex) && !retried)
                    {
                        // Was a challenge error so send the request again.
                        retry = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (webresponse != null)
                        webresponse.Close();
                }

            }
            while (retry && !retried && !m_AbortRequested);

            if (m_AbortRequested)
                throw new OperationCanceledException("Abort requested during Win Qual call");

            return response;
        }


                
        public int LastCallRetryCount
        {
            get { return m_LastCallRetryCount; }
        }


        /// <summary>
        /// Make a call to the WinQual service.
        /// </summary>
        /// <param name="url">Service URL</param>
        /// <param name="requestType">Get or Post</param>
        /// <param name="postPayload">POST payload - if null then GET is assumed</param>
        /// <param name="soapAction">SOAP action header, currently used for authentication only, null otherwise</param>
        /// <returns>Full string response to the request.</returns>
        public String WinQualCallWithRetry(String url, RequestType requestType, String postPayload, String soapAction)
        {
            if (m_WinQualIPAddress != null)
                url = url.Replace("winqual.microsoft.com", m_WinQualIPAddress);

            String response = null;
            m_LastCallRetryCount = 0;

            if ((DateTime.Now - m_LastSuccessfulLoginTime).TotalHours > m_IntervalBetweenWinQualLogonsInHours)
            {
                // Recycle the login.
                m_LogOn.LogOut();
                m_LogOn.LogInWithException(m_UserName, m_Password);
                m_LastSuccessfulLoginTime = DateTime.Now;
                m_NumberOfLogins++;
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Logged on to WinQual: " + m_NumberOfLogins.ToString(CultureInfo.InvariantCulture));
            }

            if (TestSettings.WinQualCallDelay > 0)
                Thread.Sleep(TestSettings.WinQualCallDelay);

            do
            {
                try
                {
                    response = WinQualCall(url, requestType, postPayload, soapAction);

                    // Get the root element of the feed xml. This will throw an exception if there is an error reported in 
                    // the feed.
                    getFeedNode(response);

                    // If it gets here there was no error.
                    break;
                }
                catch (System.Exception ex)
                {
                    WebException webException = ex as WebException;
                    StackHashException stackHashException = ex as StackHashException;

                    if ((webException != null) || (stackHashException != null))
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "WinQualCall Url: " + url);
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "WinQualCall failed", ex);

                        m_LastCallRetryCount++;
                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "WinQualCall failed: retryCount = " + m_LastCallRetryCount.ToString(CultureInfo.InvariantCulture));

                        if (m_LastCallRetryCount >= m_RequestRetryLimit)
                        {
                            if (ex.Message.Contains("The remote server returned an error: (404)"))
                                throw new StackHashException("The WinQual service is currently unavailable for downloading events. Try again later or wait for the scheduled sync to occur", ex, StackHashServiceErrorCode.ServerDown);
                            else
                                throw;
                        }

                        // About to retry. First check if the login has expired (i.e. the reason for the last failure.
                        if (stackHashException != null)
                        {
                            if (stackHashException.ServiceErrorCode == StackHashServiceErrorCode.AuthenticationFailure)
                            {
                                m_LogOn.LogInWithException(m_UserName, m_Password);
                                m_LastSuccessfulLoginTime = DateTime.Now;
                                m_NumberOfLogins++;
                            }
                        }

                        // Ignore the error and retry.
                    }
                    else
                    {
                        // Unexpected error so fail the call.
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "WinQualCall Url: " + url);
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "WinQualCall failed", ex);
                        throw;
                    }
                }
            } while ((m_LastCallRetryCount < m_RequestRetryLimit) && !m_AbortRequested);

            if (m_AbortRequested)
                throw new OperationCanceledException("Abort requested during cab download");

            return response;
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



    }
}
