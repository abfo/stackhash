//---------------------------------------------------------------------
// <summary>
//      Class to perform HTTP web requests to a Live ID protected resource.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Net;
using System.Security;
using Microsoft.Whos.Shared.Client.LiveID;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    /// <summary>
    /// Class to perform HTTP web requests to a Live ID protected resource.
    /// </summary>
    public class HttpLiveClient
    {
        #region Fields
        private LiveIdAuthentication liveAuth;
        private string username;
        private SecureString password;
        private string challenge;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Creates an instance of HttpLiveClient with
        /// the Live ID username and password.
        /// </summary>
        /// <param name="username">Live ID username</param>
        /// <param name="password">Live ID password</param>
        public HttpLiveClient(string username, SecureString password)
        {
            this.username = username;
            this.password = password;
            this.challenge = null;
            this.liveAuth = Utility.GetLiveIdAuth();
        }
        #endregion Constructors

        #region Methods
        #region Public Methods
        /// <summary>
        /// Ensures the account is logged into Live ID
        /// </summary>
        public void EnsureLogin()
        {
            if (!this.liveAuth.IsAuthenticated)
            {
                if (!this.liveAuth.Authenticate(this.username, this.password))
                {
                    throw new StackHashException("Unable to log on to the Win Qual service. Please check that your Windows live id username and password are correct and associated with your Win Qual account.", StackHashServiceErrorCode.WinQualLogOnFailed);
                }
            }
        }

        /// <summary>
        /// Ensures the account is logged out from Live ID
        /// </summary>
        public void EnsureLogout()
        {
            if (this.liveAuth.IsAuthenticated)
            {
                this.liveAuth.SignOut();
            }
        }

        /// <summary>
        /// Downloads the resource located in url 
        /// and returns the HttpWebResponse object.
        /// </summary>
        /// <param name="url">Resource location</param>
        /// <returns>The HttpWebResponse instance</returns>
        public HttpWebResponse DownloadObject(Uri url)
        {
            return ExecuteRequest(() => ExecuteHttpWebRequest(url));
        }

        /// <summary>
        /// Downloads the resource located in url 
        /// and returns the string representation 
        /// of the resource.
        /// </summary>
        /// <param name="url">Resource location</param>
        /// <returns>String representation</returns>
        public string DownloadString(Uri url)
        {
            return ExecuteRequest(() => ExecuteWebClient((c) => c.DownloadString(url)));
        }

        /// <summary>
        /// Uploads a file to a resource location and
        /// returns the HTTP response in a byte array.
        /// </summary>
        /// <param name="url">Resource location</param>
        /// <param name="fileName">Filename to upload</param>
        /// <returns>Response in a byte array</returns>
        public byte[] UploadFile(Uri url, string fileName)
        {
            return ExecuteRequest(() => ExecuteWebClient((c) => c.UploadFile(url, fileName)));
        }

        /// <summary>
        /// Uploads a collection of values file to a resource 
        /// location and returns the HTTP response in a byte array.
        /// </summary>
        /// <param name="url">Resource location</param>
        /// <param name="uploadValues">Collection of values to upload</param>
        /// <returns>Response in a byte array</returns>
        public byte[] UploadValues(Uri url, NameValueCollection uploadValues)
        {
            return ExecuteRequest(() => ExecuteWebClient((c) => c.UploadValues(url, uploadValues)));
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Executes an HTTP request ensuring login
        /// is done before the request is executed.
        /// If a WebException occurs containing a valid
        /// challenge, the request is attempted again.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private T ExecuteRequest<T>(Func<T> func)
        {
            this.EnsureLogin();

            try
            {
                return func();
            }
            catch (WebException ex)
            {
                this.challenge = LiveIdAuthentication.GetChallengeFromWebException(ex);
                if (String.IsNullOrEmpty(this.challenge))
                {
                    throw;
                }
            }

            return func();
        }

        public bool ProcessWebException(WebException ex)
        {
            this.challenge = LiveIdAuthentication.GetChallengeFromWebException(ex);
            if (String.IsNullOrEmpty(this.challenge))
            {
                return false;
            }
            else
            {
                return true;            
            }
        }

        
        /// <summary>
        /// Creates and sends an HTTP Request to a remote
        /// resource with a LiveID credential token.
        /// </summary>
        /// <param name="url">Remote resource location</param>
        /// <returns>HTTP Response</returns>
        private HttpWebResponse ExecuteHttpWebRequest(Uri url)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
            SetAuthorization(httpReq.Headers);
            return (HttpWebResponse)httpReq.GetResponse();
        }

        /// <summary>
        /// Creates and sends an HTTP Request to a remote
        /// resource with a LiveID credential token.
        /// </summary>
        /// <param name="url">Remote resource location</param>
        /// <returns>The output of the web client call.</returns>
        private T ExecuteWebClient<T>(Func<WebClient, T> func)
        {
            using (WebClient client = new WebClient())
            {
                SetAuthorization(client.Headers);
                return func(client);
            }
        }

        /// <summary>
        /// If there is a cached challenge, it adds an authorization 
        /// header to the argument HTTP Request headers containing the
        /// challenge response.
        /// </summary>
        /// <param name="headers">HTTP Request headers</param>
        public void SetAuthorization(WebHeaderCollection headers)
        {
            if (!String.IsNullOrEmpty(this.challenge))
            {
                LiveIdAuthentication.AddAuthorizationHeaderToRequestHeaders(
                    headers, this.liveAuth.GetResponseForHttpChallenge(this.challenge));
            }
        }
        #endregion Private Methods
        #endregion Methods
    }
}
