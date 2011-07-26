//---------------------------------------------------------------------
// <summary>
//      Class for uploading the mapping file to the server.
// </summary>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using StackHash.WindowsErrorReporting.Services.Data.API;
using System.Xml;

namespace StackHash.WindowsErrorReporting.Services.Mapping.API
{
    /// <summary>
    /// Class for uploading the mapping file to the server.
    /// </summary>
    public class FileUpload : Base
    {
        #region Fields
        private string fileName;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets or sets the name of the file to upload.
        /// </summary>
        public string FileName
        {
            get { return this.fileName; }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    throw new ArgumentException("FileName value cannot be null or empty.", "FileName");
                }
                this.fileName = value;
            }
        }
        #endregion Public Properties
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Constructor for a new object of the FileUpload class.
        /// </summary>
        /// <param name="fileName">Name of the file to upload.</param>
        public FileUpload(string fileName)
        {
            this.FileName = fileName;
        }
        #endregion Constructors

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to upload the file to the server.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        public void Upload(ref Login loginObject)
        {
            //
            // validate the login object.
            //
            Base.ValidateLoginObject(loginObject);

            //
            // url to the file upload web service
            //
            Uri fileUploadUri = new Uri(BASE_URL + "fileupload.aspx");

            //
            // use the WebClient object to upload the file to the server.
            //
            WebClient webClient = new WebClient();
            webClient.Headers.Add("encryptedTicket", loginObject.EncryptedTicket);
            byte[] responseBytes = webClient.UploadFile(fileUploadUri, this.FileName);

            //
            // get back the encryptedTicket response header for the ticket returned
            // with the response if it has been updated because of sliding expiration.
            // this header might not always be there, so check if it exists first.
            //
            if (webClient.ResponseHeaders["encryptedTicket"] != null)
            {
                loginObject.EncryptedTicket = webClient.ResponseHeaders["encryptedTicket"];
            }

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
        #endregion Methods
    }
}
