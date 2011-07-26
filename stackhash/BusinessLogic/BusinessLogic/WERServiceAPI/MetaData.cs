//---------------------------------------------------------------------
// <summary>
//      Class for getting the meta data for the data services.
//      Initially this would be the date modified for the data services.
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
    /// Class for getting the meta data for the data services.
    /// Initially this would be the date modified for the data services.
    /// </summary>
    public class MetaData : Base
    {
        #region Fields
        private int id;
        private string name;
        private string version;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// ID of the feed.
        /// </summary>
        public int ID
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        /// <summary>
        /// Product name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            internal set { this.name = value; }
        }

        /// <summary>
        /// Product version.
        /// </summary>
        public string Version
        {
            get { return this.version; }
            internal set { this.version = value; }
        }

        /// <summary>
        /// Feed created date.
        /// </summary>
        public DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Feed modified date.
        /// </summary>
        public DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods

        #region Private Methods
        #endregion Private Methods

        #region Public Methods
        /// <summary>
        /// Method to get the date time when the Developer Portal database was last updated.
        /// This date is represented in the local date-time format.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastUpdatedDate(ref Login loginObject)
        {
            //
            // get the feed response string
            // 
            string responseFromServer = MetaData.GetFeedResponse(new Uri(BASE_URL + "metadata.aspx"), ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the feed node for getting the date modified element value.
            //
            XmlNode feedNode = MetaData.GetFeedNode(responseFromServer, out namespaceMgr);

            //
            // return the modified date.
            //
            return DateTime.Parse(feedNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();
        }
        #endregion Public Methods
        #endregion Methods
    }
}
