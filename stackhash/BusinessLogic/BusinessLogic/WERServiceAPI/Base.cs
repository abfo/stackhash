//---------------------------------------------------------------------
// <summary>
//      Base class for all other classes in the API. Contains helper
//      methods commonly used by other classes.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Specialized;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Base class for all other classes in the API. Contains helper
    /// methods commonly used by other classes.
    /// </summary>
    public abstract class Base
    {
        #region Fields
        
        /// <summary>
        /// ATOM Namespace URL
        /// </summary>
        protected static string ATOM_NAMESPACE_URL = "http://www.w3.org/2005/Atom";
        
        /// <summary>
        /// Windows Error Reporting Namespace URL
        /// </summary>
        protected static string WER_NAMESPACE_URL = "http://schemas.microsoft.com/windowserrorreporting";

        /// <summary>
        /// Base URL for Client API service
        /// </summary>
        protected static string BASE_URL = Utility.GetServiceURL(Constants.baseServiceURL);
        
        #endregion Fields

        #region Methods
        #region Protected Methods
        /// <summary>
        /// Method to return the response object. This method is specifically used for cab download.
        /// </summary>
        /// <param name="feedUrl">Url to the feed.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>The HttpWebResponse object containing the response from the web service call.</returns>
        protected static HttpWebResponse GetFeedResponseObject(Uri feedUrl, ref Login loginObject)
        {
            //
            // validate the login object.
            //
            Base.ValidateLoginObject(loginObject);

            //
            // request the specified url from the server.
            //
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(feedUrl);
            httpWebRequest.Headers["encryptedTicket"] = loginObject.EncryptedTicket;

            //
            // access the response sent back by the server.
            //
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            //
            // get the encrypted ticket back as it might have been updated with the sliding expiration.
            //
            // this is not returning back the encrypted ticket so adding the is null check.
            if (httpWebResponse.Headers["encryptedTicket"] != null)
            {
                loginObject.EncryptedTicket = httpWebResponse.Headers["encryptedTicket"];
            }

            return httpWebResponse;
        }

        /// <summary>
        /// Gets the response string for the feed.
        /// </summary>
        /// <param name="feedUrl">Url to the feed.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>String containing the response from the web service call.</returns>
        protected static string GetFeedResponse(Uri feedUrl, ref Login loginObject)
        {
            //
            // validate the login object.
            //
            Base.ValidateLoginObject(loginObject);

            //
            // request the specified url from the server.
            //
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(feedUrl);
            httpWebRequest.Headers["encryptedTicket"] = loginObject.EncryptedTicket;

            //
            // access the response sent back by the server.
            //
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            //
            // get the encrypted ticket back as it might have been updated with the sliding expiration.
            //
            // this is not returning back the encrypted ticket so adding the is null check.
            if (httpWebResponse.Headers["encryptedTicket"] != null)
            {
                loginObject.EncryptedTicket = httpWebResponse.Headers["encryptedTicket"];
            }

            //
            // get the feed XML from the response
            //
            Stream dataStream = httpWebResponse.GetResponseStream();

            //
            // Open the stream using a StreamReader for easy access.
            //
            StreamReader reader = new StreamReader(dataStream);

            //
            // Read the content.
            //
            string responseFromServer = reader.ReadToEnd();

            //
            // close the heavy objects.
            //
            reader.Close();
            dataStream.Close();
            httpWebResponse.Close();

            //
            // return the response string
            //
            return responseFromServer;
        }

        /// <summary>
        /// Method to get the feed node from the feed XML.
        /// </summary>
        /// <param name="feedXML">String containing the XML from the feed.</param>
        /// <param name="xmlNamespaceManager">Represents the namespace manager.</param>
        /// <returns>Gets the feed node of the XML.</returns>
        protected static XmlNode GetFeedNode(string feedXML, out XmlNamespaceManager xmlNamespaceManager)
        {            
            //
            // parse the feed XML
            //
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(feedXML);

            XmlNamespaceManager namespaceMgr = new XmlNamespaceManager(xDoc.NameTable);
            namespaceMgr.AddNamespace("atom", ATOM_NAMESPACE_URL);
            namespaceMgr.AddNamespace("wer", WER_NAMESPACE_URL);

            //
            // assign the namespace manager to the out parameter.
            //
            xmlNamespaceManager = namespaceMgr;

            //
            // get the feed node
            //
            XmlNode feedNode = xDoc.SelectSingleNode("//atom:feed", namespaceMgr);

            //
            // check the wer:status value of attribute wer:status of the feed element.
            // if the value is "error" then throw an exception.
            //
            bool validFeed = (feedNode.Attributes["wer:status"].Value == "ok" ? true : false);

            if (validFeed == false)
            {
                //
                // throw appropriate exception depending on return data
                // 
                Base.ThrowException(feedNode, namespaceMgr);
            }

            return feedNode;
        }

        /// <summary>
        /// Gets the entry node list from the feed XML string.
        /// </summary>
        /// <param name="feedXML">String containing the feed XML.</param>
        /// <param name="xmlNamespaceManager">Represents the namespace manager.</param>
        /// <returns>Entry node list.</returns>
        protected static XmlNodeList GetEntryNodes(string feedXML, out XmlNamespaceManager xmlNamespaceManager)
        {
            //
            // declare the namespace manager object.
            //
            XmlNamespaceManager namespaceMgr;
            
            //
            // get the feed node
            //
            XmlNode feedNode = Base.GetFeedNode(feedXML, out namespaceMgr);

            //
            // assign the namespace manager to the out parameter.
            //
            xmlNamespaceManager = namespaceMgr;

            //
            // get the entry elements that have the product information
            //
            XmlNodeList entryNodes = feedNode.SelectNodes("atom:entry", namespaceMgr);

            //
            // return the entry node list.
            //
            return entryNodes;
        }

        /// <summary>
        /// Throw an exception depending on the feedNode details.
        /// </summary>
        /// <param name="namespaceMgr">XmlNamespace Manager</param>
        /// <param name="feedNode">XmlNode for the feed.</param>
        protected static void ThrowException(XmlNode feedNode, XmlNamespaceManager namespaceMgr)
        {
            //
            // get the entry node
            //
            XmlNode entryNode = feedNode.SelectSingleNode("atom:entry", namespaceMgr);

            if (entryNode == null)
            {
                throw new FeedException(DataServicesResources.FEED_EXCEPTION_DEFAULT_MESSAGE);
            }

            //
            // get the id of the feed element to decide the type of exception to throw
            //
            XmlNode exceptionTypeNode = entryNode.SelectSingleNode("atom:id", namespaceMgr);

            if (entryNode == null)
            {
                throw new FeedException(DataServicesResources.FEED_EXCEPTION_DEFAULT_MESSAGE);
            }

            string exceptionType = exceptionTypeNode.InnerText;

            //
            // get the title for the message of the exception.
            //
            XmlNode messageNode = entryNode.SelectSingleNode("atom:title", namespaceMgr);

            if (messageNode == null)
            {
                throw new FeedException(DataServicesResources.FEED_EXCEPTION_DEFAULT_MESSAGE);
            }

            //
            // throw the appropriate exception based on the exception type.
            //
            string message = messageNode.InnerText;
            Exception innerException = GetInnerException(message, exceptionType, entryNode, namespaceMgr);
            
            switch (exceptionType)
            {
                case SpecialExceptionList.FeedException:
                    throw new FeedException(message, innerException);
                case SpecialExceptionList.FeedAuthenticationException:
                    throw new FeedAuthenticationException(message, innerException);
                case SpecialExceptionList.FeedAccessException:
                    throw new FeedAccessException(message, innerException);
                case SpecialExceptionList.ArgumentOutOfRangeException:
                    throw new ArgumentOutOfRangeException(message, innerException);
                case SpecialExceptionList.FileUploadException:
                    throw new ArgumentOutOfRangeException(message, innerException);
                default:
                    throw new FeedException(DataServicesResources.FEED_EXCEPTION_DEFAULT_MESSAGE);
            }
        }

        /// <summary>
        /// Validate the login object. Throws an exception if the Login object is null or the EncryptedTicket is null or empty.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        protected static void ValidateLoginObject(Login loginObject)
        {
            //
            // throw an exception if the login object is null
            //
            if (loginObject == null)
            {
                throw new FeedAuthenticationException("The Login parameter is null. It should be a valid Login object.");
            }

            //
            // throw an exception if the Encrypted ticket value is null or empty
            //
            if (String.IsNullOrEmpty(loginObject.EncryptedTicket) == true)
            {
                throw new FeedAuthenticationException("The EncryptedTicket property of the Login parameter is null. It should be a valid encrypted ticket value.");
            }
        }
        #endregion Protected Methods

        #region Private Methods
        /// <summary>
        /// Create a proper exception based on the exception type passed.
        /// </summary>
        /// <param name="message">Exception Message</param>
        /// <param name="exceptionType">Exception Type</param>
        /// <param name="entryNode">Xml Node that contains Exception data</param>
        /// <param name="namespaceMgr">Xml Namespace Manager</param>
        /// <returns>Returns the inner exception.</returns>
        private static Exception GetInnerException(string message, string exceptionType, XmlNode entryNode, XmlNamespaceManager namespaceMgr)
        {
            Exception innerException = new Exception(message);

            if (SpecialExceptionList.SpecialInnerExceptionNameList.Contains(exceptionType))
            {
                XmlNode dataNode = dataNode = entryNode.SelectSingleNode("wer:additionalInformation", namespaceMgr);
                if (dataNode != null)
                {
                    XmlNodeList errorDataList = dataNode.SelectNodes("wer:data", namespaceMgr);
                    foreach (XmlNode errorData in errorDataList)
                    {
                        string name = string.Empty;
                        string value = string.Empty;
                        XmlNode nameNode = errorData.SelectSingleNode("wer:name", namespaceMgr);
                        if (nameNode != null)
                        {
                            name = nameNode.InnerText;
                        }
                        XmlNode valueNode = errorData.SelectSingleNode("wer:value", namespaceMgr);
                        if (valueNode != null)
                        {
                            value = valueNode.InnerText;
                        }
                        innerException.Data.Add(name, value);
                    }
                }
            }

            return innerException;
        }
        #endregion Private Methods
        #endregion Methods
    }
}
