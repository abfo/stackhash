using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Net;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public class LogOnTicket : ILogOn, IDisposable
    {
        private String m_UserName;
        private String m_Password;
        private String m_Ticket; // Login ticket used for all calls.
        private IWebCalls m_WebCalls;

        private const String s_WinQualLoginUrl = "https://winqual.microsoft.com/services/Authentication/Authentication.svc/BasicTicket";
        private const String s_WinQualLoginSoapAction = "https://winqual.microsoft.com/Services/Authentication/IBasicTicket/GetBasicTicket";


        public void Initialise(IWebCalls webCalls)
        {
            m_WebCalls = webCalls;
        }

        /// <summary>
        /// Logs in to the WinQual service using WinQual Ticket authentication.
        /// </summary>
        /// <param name="username">User name to log in with.</param>
        /// <param name="password">Password for the user.</param>
        /// <returns>true - success, false - failed to login.</returns>
        public bool LogIn(string username, string password)
        {
            int retryCount = 0;
            int retryLimit = 2;

            while (retryCount < 2)
            {
                try
                {
                    getTicket(username, password);

                    if (string.IsNullOrEmpty(m_Ticket))
                    {
                        return false;
                    }
                    else
                    {
                        m_UserName = username;
                        m_Password = password;
                        return true;
                    }
                }

                catch (System.Net.WebException ex)
                {
                    retryCount++;
                    if (retryCount >= retryLimit)
                        throw new StackHashException("Unable to communicate with the WinQual site. Check that the site is available and that any proxy settings are correct", ex, StackHashServiceErrorCode.ServerDown);
                }
            }
            return false;
        }


        public void LogOut()
        {
        }


        /// <summary>
        /// Adds the authentication ticket to the web request.
        /// </summary>
        /// <param name="webRequest"></param>
        public void ProcessRequest(HttpWebRequest webRequest)
        {
            if (m_Ticket != null)
                webRequest.Headers.Add("encryptedTicket", m_Ticket);
        }

        /// <summary>
        /// Adds the authentication ticket to the web request.
        /// </summary>
        /// <param name="webRequestHeaders"></param>
        public void ProcessRequest(WebHeaderCollection webRequestHeaders)
        {
            if (m_Ticket != null)
                webRequestHeaders.Add("encryptedTicket", m_Ticket);
        }


        /// <summary>
        /// Gets the ticket, if present, from the response.
        /// </summary>
        /// <param name="webResponse">Web response.</param>
        public void ProcessResponse(HttpWebResponse webResponse)
        {
            // If the ticket is present in the header it may have been updated so pass the
            // new copy back to the caller.
            String updatedTicket = webResponse.Headers["encryptedTicket"];
            if (!String.IsNullOrEmpty(updatedTicket))
            {
                m_Ticket = updatedTicket;
            }
        }


        /// <summary>
        /// Issues a call to the web services to get a logon ticket.
        /// Exceptions if it fails to log in.
        /// </summary>
        /// <param name="username">WinQual Username to log on with.</param>
        /// <param name="password">WinQual password.</param>
        /// <returns>The logon ticket or null.</returns>
        public void LogInWithException(String username, String password)
        {
            getTicket(username, password);
        }

        public bool ProcessWebException(WebException ex)
        {
            return false;
        }


        /// <summary>
        /// Issues a call to the web services to get a logon ticket.
        /// </summary>
        /// <param name="username">WinQual Username to log on with.</param>
        /// <param name="password">WinQual password.</param>
        private void getTicket(String username, String password)
        {
            String ticket = null;
            String soapRequest;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlWriter writer = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                    writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                    writer.WriteStartElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                    writer.WriteStartElement("GetBasicTicket", "https://winqual.microsoft.com/Services/Authentication/");
                    writer.WriteElementString("userName", username);
                    writer.WriteElementString("password", password);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                soapRequest = Encoding.UTF8.GetString(memoryStream.ToArray());
            }


            // DON'T CALL WinQualCallWithRetry as it will call back to this function on failure.
            String response = m_WebCalls.WinQualCall(s_WinQualLoginUrl, RequestType.Post, soapRequest, s_WinQualLoginSoapAction);

            // Example response (GetBasicTicketResult is empty if password is bad) :
            //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
            //  <s:Body>
            //    <GetBasicTicketResponse xmlns="https://winqual.microsoft.com/Services/Authentication/">
            //      <GetBasicTicketResult>TICKET</GetBasicTicketResult>
            //    </GetBasicTicketResponse>
            //  </s:Body>
            //</s:Envelope>

            XElement xel = XElement.Parse(response);
            foreach (XElement desc in xel.Descendants())
            {
                if (desc.Name.LocalName == "GetBasicTicketResult")
                {
                    ticket = desc.Value;
                    break;
                }
            }

            if (!String.IsNullOrEmpty(ticket))
                m_Ticket = ticket;
        }


        
        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Nothing to do.
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}
