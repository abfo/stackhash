using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Globalization;

namespace StackHash.FogBugzPlugin
{
    /// <summary>
    /// Interface to the FogBugz API
    /// </summary>
    sealed class FogBugzApi
    {
        private const int MinimumApiVersion = 6; // minimum acceptable FogBugz API Level

        private string m_ApiEndpoint; // FogBugz API endpoint, retrieved by querying the API url
        private string m_Token; // FogBugz API token, valid until logout

        /// <summary>
        /// Interface to the FogBugz API
        /// </summary>
        /// <remarks>
        /// See http://fogbugz.stackexchange.com/fogbugz-xml-api for API documentation
        /// </remarks>
        /// <param name="apiUrl">URL of the API</param>
        /// <param name="username">FogBugz Username</param>
        /// <param name="password">FogBugz Password</param>
        public FogBugzApi(string apiUrl, string username, string password)
        {
            if (apiUrl == null)
                throw new ArgumentNullException("apiUrl");
            if (username == null)
                throw new ArgumentNullException("username");
            if (password == null)
                throw new ArgumentNullException("password");

            // we're expecting a response like this...
            // <?xml version="1.0" encoding="UTF-8"?>
            // <response>
            //   <version>2</version>
            //   <minversion>1</minversion>
            //   <url>api.asp?</url>
            // </response>

            try
            {
                XDocument xdoc = CallApi(apiUrl, null);
                int apiVersion = Convert.ToInt32(xdoc.Element("response").Element("version").Value);
                int apiMinVersion = Convert.ToInt32(xdoc.Element("response").Element("minversion").Value);
                string apiEndpoint = xdoc.Element("response").Element("url").Value;

                if (apiVersion < MinimumApiVersion)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "FogBugz API not supported (plugin requires at least version {0}, API version is {1}",
                        MinimumApiVersion,
                        apiVersion));

                if (apiMinVersion > MinimumApiVersion)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "FogBugz API not supported (plugin requires at least version {0}, API minimum version is {1}",
                        MinimumApiVersion,
                        apiMinVersion));

                // construct the endpoint for the api
                m_ApiEndpoint = apiUrl.Substring(0, apiUrl.LastIndexOf('/') + 1) + apiEndpoint;
            }
            catch (Exception ex)
            {
                throw new FogBugzApiException("Failed to detect FogBugz API Endpoint (make sure the API URL is the API XML, i.e. http://fogbugz.mycompany.com/api.xml)",
                    ex);
            }

            // attempt to login
            // we're expecting a token
            // <response><token>24dsg34lok43un23</token></response>
            NameValueCollection loginParameters = new NameValueCollection();
            loginParameters["cmd"] = "logon";
            loginParameters["email"] = username;
            loginParameters["password"] = password;

            XDocument xdocLogin = CallApi(m_ApiEndpoint, loginParameters);

            try
            {
                m_Token = xdocLogin.Element("response").Element("token").Value;
            }
            catch (Exception ex)
            {
                throw new FogBugzApiException("Failed to parse FogBugz login token from API response", ex);
            }

            // if we get this far without exception should be logged in and ready to use the api
        }

        /// <summary>
        /// Logout of the FogBugz API
        /// </summary>
        public void Logout()
        {
            if (m_Token == null)
                throw new FogBugzApiException("Not logged in to the FogBugz API");

            NameValueCollection logoutParameters = new NameValueCollection();
            logoutParameters["cmd"] = "logoff";
            logoutParameters["token"] = m_Token;

            CallApi(m_ApiEndpoint, logoutParameters);

            m_Token = null;
        }

        /// <summary>
        /// Adds a case to FogBugz
        /// </summary>
        /// <param name="title">Title of the case</param>
        /// <param name="text">Text for the case</param>
        /// <param name="project">Project (optional)</param>
        /// <param name="area">Area (optional)</param>
        /// <returns>Case Number</returns>
        public int AddCase(string title, string text, string project, string area)
        {
            if (m_Token == null)
                throw new FogBugzApiException("Not logged in to the FogBugz API");
            if (title == null)
                throw new ArgumentNullException("title");
            if (text == null)
                throw new ArgumentNullException("text");

            int caseNumber = -1;

            NameValueCollection addParameters = new NameValueCollection();
            addParameters["cmd"] = "new";
            addParameters["sTitle"] = title;
            addParameters["sEvent"] = text;
            addParameters["token"] = m_Token;

            if (!string.IsNullOrEmpty(project))
            {
                addParameters["sProject"] = project;
            }

            if (!string.IsNullOrEmpty(area))
            {
                addParameters["sArea"] = area;
            }

            XDocument xdoc = CallApi(m_ApiEndpoint, addParameters);

            // response will look like this
            // <response>
            //   <case ixBug="18325" operations="edit,assign,resolve,email,remind"></case>
            // </response>

            try
            {
                caseNumber = Convert.ToInt32(xdoc.Element("response").Element("case").Attribute("ixBug").Value,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new FogBugzApiException("Failed to parse FogBugz case number from API response", ex);
            }

            return caseNumber;
        }

        /// <summary>
        /// Adds a note to an existing FogBugz case
        /// </summary>
        /// <param name="caseNumber">The case number</param>
        /// <param name="text">Text to add to the case</param>
        public void AddNoteToCase(int caseNumber, string text)
        {
            if (m_Token == null)
                throw new FogBugzApiException("Not logged in to the FogBugz API");
            if (text == null)
                throw new ArgumentNullException("text");

            NameValueCollection editParameters = new NameValueCollection();
            editParameters["cmd"] = "edit";
            editParameters["ixBug"] = caseNumber.ToString(CultureInfo.InvariantCulture);
            editParameters["sEvent"] = text;
            editParameters["token"] = m_Token;

            CallApi(m_ApiEndpoint, editParameters);
        }

        private XDocument CallApi(string url, NameValueCollection parameters)
        {
            XDocument xdoc = null;

            using (WebClient wc = new WebClient())
            {
                if (parameters == null)
                {
                    // if no parameters just GET the URL
                    xdoc = XDocument.Parse(wc.DownloadString(url));
                }
                else
                {
                    // otherwise POST the parameters
                    xdoc = XDocument.Parse(Encoding.UTF8.GetString(wc.UploadValues(url, "POST", parameters)));

                    // detect and throw an exception if the API returns an error like this
                    // <response><error code="1">Error Message To Show User</error></response>
                    XElement response = xdoc.Element("response");
                    if (response != null)
                    {
                        XElement error = response.Element("error");
                        if (error != null)
                        {
                            string errorMessage = error.Value;
                            int errorCode = Convert.ToInt32(error.Attribute("code").Value, CultureInfo.InvariantCulture);

                            throw new FogBugzApiException(errorMessage, errorCode);
                        }
                    }
                }
            }

            return xdoc;
        }
    }
}
