//---------------------------------------------------------------------
// <summary>
//      Base class for mapped objects in the API.
// </summary>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using StackHash.WindowsErrorReporting.Services.Data.API;
using System.Collections.Specialized;
using System.Net;
using System.Xml;

namespace StackHash.WindowsErrorReporting.Services.Mapping.API
{
    /// <summary>
    /// Base class for mapped objects in the API.
    /// </summary>
    public abstract class MappedObjectBase : Base
    {
        #region Fields
        private int id;
        private string name;
        private string version;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        private string mappedBy;
        private string mappedByEmail;
        private string status;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the ID of the product.
        /// </summary>
        public virtual int ID
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
            internal set { this.name = value; }
        }

        /// <summary>
        /// Gets the version of the product.
        /// </summary>
        public virtual string Version
        {
            get { return this.version; }
            internal set { this.version = value; }
        }
        /// <summary>
        /// Gets the date when the product was mapped in the Developer Portal.
        /// This date is represented in the local date-time format.
        /// </summary>
        public virtual DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Gets the date when the product was last modified.
        /// This date is represented in the local date-time format.
        /// </summary>
        public virtual DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }

        /// <summary>
        /// Gets the name of the user who mapped the product.
        /// </summary>
        public virtual string MappedBy
        {
            get { return this.mappedBy; }
            internal set { this.mappedBy = value; }
        }

        /// <summary>
        /// Gets the e-mail id of the user who mapped the product.
        /// </summary>
        public virtual string MappedByEmail
        {
            get { return this.mappedByEmail; }
            internal set { this.mappedByEmail = value; }
        }

        /// <summary>
        /// Gets the status of the product.
        /// </summary>
        public virtual string Status
        {
            get { return this.status; }
            internal set { this.status = value; }
        }
        #endregion Public Properties
        #region Protected Properties
        #endregion Protected Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to send the request to the server using the WebClient object
        /// and parse the response for feed exception.
        /// </summary>
        /// <param name="webServicePage">The name of the web service page.</param>
        /// <param name="uploadValues">The NameValueCollection of values to POST with the request.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        public static void ProcessRequest(string webServicePage, NameValueCollection uploadValues, ref Login loginObject)
        {
            //
            // validate the login object.
            //
            Base.ValidateLoginObject(loginObject);

            //
            // create the request uri from the web service page input
            //
            Uri requestUri = new Uri(BASE_URL + webServicePage);

            //
            // create the WebClient object
            //
            WebClient webClient = new WebClient();
            webClient.Headers.Add("encryptedTicket", loginObject.EncryptedTicket);

            //
            // upload the values
            //
            byte[] responseBytes = webClient.UploadValues(requestUri, uploadValues);

            //
            // get back the encryptedTicket response header for the ticket returned
            // with the response if it has been updated because of sliding expiration.
            // this header might not always be there, so check if it exists first.
            //
            if (webClient.ResponseHeaders["encryptedTicket"] != null)
            {
                loginObject.EncryptedTicket = webClient.ResponseHeaders["encryptedTicket"];
            }
            
            //
            // get the response
            //
            UTF8Encoding encoding = new UTF8Encoding();
            string response = encoding.GetString(responseBytes);

            //
            // declare the namespace manager object.
            //
            XmlNamespaceManager namespaceMgr;

            //
            // get the feed node, this will also parse the response for feed exceptions
            //
            XmlNode feedNode = Base.GetFeedNode(response, out namespaceMgr);
        }
        #endregion Public Methods
        #region Protected Methods
        #endregion Protected Methods
        #endregion Methods
    }
}
