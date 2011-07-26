//---------------------------------------------------------------------
// <summary>
//      Class for representing a Product in the API.
//      This is the product mapped in the Developer Portal.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class to represent a product.
    /// This is the product mapped in the Developer Portal.
    /// </summary>
    public class Product : Base
    {
        #region Fields
        private int id;
        private string name;
        private string version;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        private Uri filesLink;

        private int totalResponses;
        private int totalEvents;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the ID of the product.
        /// </summary>
        public int ID
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        /// <summary>
        /// Gets the nameof the product.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            internal set { this.name = value; }
        }

        /// <summary>
        /// Gets the version of the product.
        /// </summary>
        public string Version
        {
            get { return this.version; }
            internal set { this.version = value; }
        }

        /// <summary>
        /// Gets the date when the product was mapped in the Developer Portal.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Gets the date when the product was last modified.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }

        /// <summary>
        /// Gets the total responses for the product.
        /// This feature has not been implemented yet.
        /// </summary>
        public int TotalResponses
        {
            get { return this.totalResponses; }
            internal set { this.totalResponses = value; }
        }

        /// <summary>
        /// Gets the total events for the product.
        /// </summary>
        public int TotalEvents
        {
            get { return this.totalEvents; }
            internal set { this.totalEvents = value; }
        }

        /// <summary>
        /// Gets the url to the mapped files for the product.
        /// </summary>
        public Uri FilesLink
        {
            get { return this.filesLink; }
            internal set { this.filesLink = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods

        #region Private Methods
        #endregion Private Methods

        #region Public Methods
        /// <summary>
        /// Method to loads the files associated with the product.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Collection of ApplicationFile objects.</returns>
        public ApplicationFileCollection GetApplicationFiles(ref Login loginObject)
        {
            //
            // get the feed response string
            // 
            string responseFromServer = ApplicationFile.GetFeedResponse(new Uri(BASE_URL + "files.aspx?productid=" + ID.ToString()), ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = ApplicationFile.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // create an empty application file collection object.
            //
            ApplicationFileCollection applicationFileCollection = new ApplicationFileCollection(entryNodes.Count);

            //
            // parse the entry elements and load the product objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                ApplicationFile applicationFile = new ApplicationFile();

                //title - file name
                applicationFile.Name = entryNode.SelectSingleNode("atom:title", namespaceMgr).InnerText;

                //id - file id
                applicationFile.ID = int.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText);

                // updated - file date modified
                applicationFile.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - file date created
                applicationFile.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // link - href - events link
                applicationFile.EventsLink = new Uri(entryNode.SelectSingleNode("atom:link", namespaceMgr).Attributes["href"].Value);

                // file version
                applicationFile.Version = entryNode.SelectSingleNode("wer:fileVersion", namespaceMgr).InnerText;

                // file link date
                applicationFile.LinkDateLocal = DateTime.Parse(entryNode.SelectSingleNode("wer:fileLinkDate", namespaceMgr).InnerText).ToLocalTime();

                //
                // add the file to the file collection
                //
                applicationFileCollection.Add(applicationFile);
            }

            //
            // return the file collection
            //
            return applicationFileCollection;

        }

        /// <summary>
        /// Method to get the list of products mapped to the company.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Returns the collection of Product objects.</returns>
        public static ProductCollection GetProducts(ref Login loginObject)
        {
            //
            // get the feed response string
            // 
            string responseFromServer = Product.GetFeedResponse(new Uri(BASE_URL + "products.aspx"), ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = Product.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // create an empty product collection object.
            //
            ProductCollection productCollection = new ProductCollection();

            //
            // parse the entry elements and load the product objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                Product product = new Product();

                //title - product name
                product.Name = entryNode.SelectSingleNode("atom:title", namespaceMgr).InnerText;

                //id - product id
                product.ID = int.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText);

                // updated - product date modified
                product.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - product date created
                product.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // link - href - files link
                product.FilesLink = new Uri(entryNode.SelectSingleNode("atom:link", namespaceMgr).Attributes["href"].Value);

                // product version
                product.Version = entryNode.SelectSingleNode("wer:productVersion", namespaceMgr).InnerText;

                // total events
                product.TotalEvents = int.Parse(entryNode.SelectSingleNode("wer:totalEvents", namespaceMgr).InnerText);

                // total responses
                product.TotalResponses = int.Parse(entryNode.SelectSingleNode("wer:totalResponses", namespaceMgr).InnerText);

                //
                // add the product to the product collection
                //
                productCollection.Add(product);
            }

            //
            // return the product collection
            //
            return productCollection;
        }
        #endregion Public Methods
        #endregion Methods
    }
}
