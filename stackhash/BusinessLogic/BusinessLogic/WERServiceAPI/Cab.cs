//---------------------------------------------------------------------
// <summary>
//      Class for representing a cab in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Web;
using System.Security;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for representing a cab in the API.
    /// </summary>
    public class Cab : Base
    {
        #region Fields
        private int id;
        private string fileName;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        private long sizeInBytes;
        private Uri cabUrl;
        private int eventID;
        private string eventTypeName;
        private string cabFileName;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets or sets the cab ID.
        /// </summary>
        public int ID
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the cab file name.
        /// </summary>
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.fileName) == true)
                {
                    this.fileName = string.Format("{0}-{1}-{2}", this.EventID, this.EventTypeName.Replace(" ", ""), this.CabFileName);
                }
                return this.fileName;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    throw new ArgumentException("FileName value cannot be null or empty.", "FileName");
                }
                this.fileName = value;
            }
        }

        /// <summary>
        /// Gets the date when the cab was received by the Developer portal service.
        /// This date is in local date-time format.
        /// </summary>
        public DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Gets the date when the cab was received by the Developer portal service.
        /// This will be the same as the date created as the cab does not get modified currently.
        /// This date is in local date-time format.
        /// </summary>
        public DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }

        /// <summary>
        /// Gets or sets the cab size (in bytes).
        /// </summary>
        public long SizeInBytes
        {
            get { return this.sizeInBytes; }
            internal set { this.sizeInBytes = value; }
        }

        /// <summary>
        /// Gets or sets the cab feed url for the individual cabs.
        /// </summary>
        internal Uri CabUrl
        {
            get { return this.cabUrl; }
            set { this.cabUrl = value; }
        }

        /// <summary>
        /// Gets the id of the event associated with the cab.
        /// </summary>
        public int EventID
        {
            get { return this.eventID; }
            internal set { this.eventID = value; }
        }

        /// <summary>
        /// Gets the event type name of the event associated with the cab.
        /// </summary>
        public string EventTypeName
        {
            get { return this.eventTypeName; }
            internal set { this.eventTypeName = value; }
        }

        /// <summary>
        /// Gets or sets the file name of the cab that will be used in the cab download stream.
        /// </summary>
        internal string CabFileName
        {
            get { return this.cabFileName; }
            set { this.cabFileName = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to save the cab to a folder.
        /// </summary>
        /// <param name="folderPath">Path to the folder to save the cab at.</param>
        /// <param name="overWrite">Flag to overwrite the existing cab file</param>
        /// <param name="loginObject">Object containing the login credentials.</param>        
        /// <returns>True if the cab was saved successfully, else false.</returns>
        public bool SaveCab(string folderPath, bool overWrite, ref Login loginObject)
        {
            //
            // Call the overloaded SaveCab method to get the response and save the cab.
            //
            return this.SaveCab(folderPath, this.FileName, overWrite, ref loginObject);
        }

        /// <summary>
        /// Static method to get the list of cabs
        /// </summary>
        /// <param name="eventID">ID of the event associated with the cab.</param>
        /// <param name="eventTypeName">Event type of the event associated with the cab.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>CabCollection object contaning cab objects.</returns>
        public static CabCollection GetCabs(int eventID, string eventTypeName, ref Login loginObject)
        {
            //
            // throw argument exception if event id is less than 1
            //
            if (eventID < 1)
            {
                throw new ArgumentOutOfRangeException("eventID", "eventID cannot be less than 1.");
            }

            //
            // throw null reference exception if null or empty event type name is passed in.
            //
            if (string.IsNullOrEmpty(eventTypeName) == true)
            {
                throw new NullReferenceException("eventTypeName cannot be null or empty.");
            }

            //
            // get the response from the server for the cab collection url.
            //
            string responseFromServer = Cab.GetFeedResponse(
                new Uri(
                    string.Format(
                        "{0}cabs.aspx?eventid={1}&eventtypename={2}"
                        , BASE_URL
                        , eventID
                        , eventTypeName
                    )
                )
                , ref loginObject
            );

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = Event.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // instantiate the cab collection object.
            //
            CabCollection cabCollection = new CabCollection();

            //
            // parse the entry elements and load the cab objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                Cab cab = new Cab();

                // id - cab id
                cab.ID = int.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText);

                // updated - product date modified
                cab.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - product date created
                cab.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // wer:cabSize - cab size
                cab.SizeInBytes = long.Parse(entryNode.SelectSingleNode("wer:cabSize", namespaceMgr).InnerText);

                // wer:eventid = event id
                cab.EventID = int.Parse(entryNode.SelectSingleNode("wer:eventID", namespaceMgr).InnerText);

                // wer:eventTypeName = event type name
                cab.EventTypeName = entryNode.SelectSingleNode("wer:eventTypeName", namespaceMgr).InnerText;

                // wer:cabFileName = cab file name
                cab.CabFileName = entryNode.SelectSingleNode("wer:cabFileName", namespaceMgr).InnerText;

                // link - cab url
                cab.CabUrl = new Uri(entryNode.SelectSingleNode("atom:link[@rel='enclosure']", namespaceMgr).Attributes["href"].Value);

                //
                // add the cab object to the cab collection
                //
                cabCollection.Add(cab);
            }

            //
            // return the cab collection
            //
            return cabCollection;

        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Method to save the cab to a folder with a specified file name.
        /// </summary>
        /// <param name="folderPath">Path to the folder to save the cab at.</param>
        /// <param name="fileName">Name of the file to save the cab file as.</param>
        /// <param name="overWrite">Flag to overwrite the existing cab file</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>true if the cab was saved successfully, else false.</returns>
        private bool SaveCab(string folderPath, string fileName, bool overWrite, ref Login loginObject)
        {
            //
            // make sure folder path is ok.
            //
            if (Directory.Exists(folderPath) == false)
            {
                //
                // TODO: throw directory not accessible exception.
                //
            }

            // check if cab already exists in the folder path before download starts
            //
            if (File.Exists(Path.Combine(folderPath, fileName)) == true && !overWrite)
            {
                return false;
            }

            //
            // get the response from the server for the cab collection url.
            //
            HttpWebResponse cabResponse = Cab.GetFeedResponseObject(this.CabUrl, ref loginObject);

            Stream responseStream = cabResponse.GetResponseStream();
            //Stream responseStream = this.GetCabStream(ref loginObject);

            FileStream fileStream = new FileStream(Path.Combine(folderPath, fileName), FileMode.OpenOrCreate, FileAccess.Write);
            int bufferLength = 256;
            Byte[] buffer = new byte[bufferLength];
            int bytesRead = responseStream.Read(buffer, 0, bufferLength);
            while (bytesRead > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
                bytesRead = responseStream.Read(buffer, 0, bufferLength);
            }

            //
            // Cleanup the streams and the response.
            //
            responseStream.Close();
            cabResponse.Close();
            fileStream.Close();

            return true;
        }
        #endregion Private Methods
        #endregion Methods
    }
}
