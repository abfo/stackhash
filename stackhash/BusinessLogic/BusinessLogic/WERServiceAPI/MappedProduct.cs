//---------------------------------------------------------------------
// <summary>
//      Class for representing a mapped product in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using StackHash.WindowsErrorReporting.Services.Data.API;
using System.Collections.Specialized;

namespace StackHash.WindowsErrorReporting.Services.Mapping.API
{
    /// <summary>
    /// Class to represent a mapped product.
    /// </summary>
    public class MappedProduct : MappedObjectBase
    {
        #region Fields
        private Uri mappedFilesLink;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the url to the mapped files for the product.
        /// </summary>
        public Uri MappedFilesLink
        {
            get { return this.mappedFilesLink; }
            internal set { this.mappedFilesLink = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods

        #region Private Methods
        #endregion Private Methods

        #region Public Methods
        /// <summary>
        /// Method to loads the mapped files associated with the mapped product.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Collection of MappedFile objects.</returns>
        public MappedFileCollection GetMappedFiles(ref Login loginObject)
        {
            //
            // get the feed response string
            // 
            string responseFromServer = ApplicationFile.GetFeedResponse(new Uri(BASE_URL + "mappedproductfiles.aspx?mappedproductid=" + this.ID.ToString()), ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = ApplicationFile.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // create an empty mapped file collection object.
            //
            MappedFileCollection mappedFileCollection = new MappedFileCollection(entryNodes.Count);

            //
            // parse the entry elements and load the product objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                MappedFile mappedFile = new MappedFile();

                //title - file name
                mappedFile.Name = entryNode.SelectSingleNode("atom:title", namespaceMgr).InnerText;

                //id - file id
                mappedFile.ID = int.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText);

                // updated - file date modified
                mappedFile.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - file date created
                mappedFile.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // link - href - events link
                //mappedFile.EventsLink = new Uri(entryNode.SelectSingleNode("atom:link", namespaceMgr).Attributes["href"].Value);

                // file version
                mappedFile.Version = entryNode.SelectSingleNode("wer:fileVersion", namespaceMgr).InnerText;

                // file link date
                mappedFile.LinkDateLocal = DateTime.Parse(entryNode.SelectSingleNode("wer:fileLinkDate", namespaceMgr).InnerText).ToLocalTime();

                // mapped by
                mappedFile.MappedBy = entryNode.SelectSingleNode("wer:mappedBy", namespaceMgr).InnerText;

                // mapped by e-mail
                mappedFile.MappedByEmail = entryNode.SelectSingleNode("wer:mappedByEmail", namespaceMgr).InnerText;

                // status
                mappedFile.Status = entryNode.SelectSingleNode("wer:status", namespaceMgr).InnerText;

                //
                // add the file to the file collection
                //
                mappedFileCollection.Add(mappedFile);
            }

            //
            // return the file collection
            //
            return mappedFileCollection;
        }

        /// <summary>
        /// Method to delete a list of files from a mapped product.
        /// </summary>
        /// <param name="mappedFileIDs">List of mapped file ids.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        public void DeleteMappedFiles(int[] mappedFileIDs, ref Login loginObject)
        {
            //
            // throw an exception if the mappedFileIDs is null
            //
            if (mappedFileIDs == null)
            {
                ArgumentNullException argumentNullException = new ArgumentNullException("mappedFileIDs", "The parameter cannot be null.");
                throw new FeedException("mappedFileIDs parameter cannot be null.", argumentNullException);
            }

            //
            // check to verify the mapped file ids contain alteast one element.
            //
            if (mappedFileIDs.Length < 1)
            {
                ArgumentOutOfRangeException argumentOutOfRangeException = new ArgumentOutOfRangeException("mappedFileIDs", "The parameter should have atleast one element.");
                throw new FeedException("mappedFileIDs int array should have atleast one element.", argumentOutOfRangeException);
            }

            //
            // create a name value collection object to hold the mapped product id and file ids
            //
            NameValueCollection deleteFilesForProductCollection = new NameValueCollection(mappedFileIDs.Length + 1);

            //
            // add the mapped product id to the collection
            //
            deleteFilesForProductCollection.Add("mappedproductid", this.ID.ToString());

            //
            // add the list of mapped file ids to the collection
            //
            foreach (int mappedFileID in mappedFileIDs)
            {
                deleteFilesForProductCollection.Add("mappedfileid", mappedFileID.ToString());
            }

            //
            // call the base ProcessRequest method to send the request and process the response
            //
            MappedObjectBase.ProcessRequest("deletemappedfilesforproduct.aspx", deleteFilesForProductCollection, ref loginObject);
        }
        #endregion Public Methods
        #endregion Methods
    }
}
