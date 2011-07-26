using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Net;
using System.IO;

using StackHashUtilities;
using StackHashBusinessObjects;


namespace WinQualAtomFeed
{
    public class LogOnLiveId : ILogOn
    {
        private String m_UserName;
        private String m_Password;
        private HttpLiveClient m_LiveClient;
        private IWebCalls m_WebCalls;

        public void Initialise(IWebCalls webCalls)
        {
            m_WebCalls = webCalls;

        }

        /// <summary>
        /// Logs in to the WinQual service using Windows Live ID.
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
                    initialiseLiveClient(username, password);

                    m_UserName = username;
                    m_Password = password;
                    return true;
                }
                catch (System.DllNotFoundException ex)
                {
                    throw new StackHashException("Windows LiveID distributable component is missing.", ex, StackHashServiceErrorCode.WindowsLiveClientMissing);
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


        /// <summary>
        /// Logout of the client.
        /// </summary>
        public void LogOut()
        {
            if (m_LiveClient != null)
                m_LiveClient.EnsureLogout();
            m_LiveClient = null;
        }

        public bool ProcessWebException(WebException ex)
        {
            return m_LiveClient.ProcessWebException(ex);
        }

        /// <summary>
        /// Adds the authentication ticket to the web request.
        /// </summary>
        /// <param name="webRequest"></param>
        public void ProcessRequest(HttpWebRequest webRequest)
        {
            m_LiveClient.SetAuthorization(webRequest.Headers);
        }

        /// <summary>
        /// Adds the authentication ticket to the web request.
        /// </summary>
        /// <param name="webRequestHeaders"></param>
        public void ProcessRequest(WebHeaderCollection webRequestHeaders)
        {
            m_LiveClient.SetAuthorization(webRequestHeaders);
        }

        /// <summary>
        /// Gets the ticket, if present, from the response.
        /// </summary>
        /// <param name="webResponse">Web response.</param>
        public void ProcessResponse(HttpWebResponse webResponse)
        {
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
            initialiseLiveClient(username, password);
        }


        /// <summary>
        /// Issues a call to get a Windows Live logon client.
        /// </summary>
        /// <param name="username">WinQual Username to log on with.</param>
        /// <param name="password">WinQual password.</param>
        private void initialiseLiveClient(String username, String password)
        {
            SecureString ss = new SecureString();
            password.ToList().ForEach((c) => ss.AppendChar(c));

            if (m_LiveClient != null)
            {
                m_LiveClient.EnsureLogout();
                m_LiveClient = null;
            }

            // Attempt login the credentials to ensure they are 
            // valid at service startup. If the credentials
            // are not valid, the service will stop.
            m_LiveClient = new HttpLiveClient(username, ss);
            m_LiveClient.EnsureLogin();
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
