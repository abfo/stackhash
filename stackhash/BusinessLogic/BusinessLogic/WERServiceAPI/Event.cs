//---------------------------------------------------------------------
// <summary>
//      Class for representing an Event in the API.
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
    /// Class to represent an event.
    /// </summary>
    public class Event : Base
    {
        #region Fields
        private int id;
        private string eventTypeName;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        private int totalHits;
        private EventSignature signature;
        private Uri cabUrl;
        private Uri detailsUrl;
        private const int DEFAULT_HIT_DAYS = 30;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the ID of the Event.
        /// </summary>
        public int ID
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        /// <summary>
        /// Gets the name of the event type. For example Crash 32bit, Special Exception etc.
        /// </summary>
        public string EventTypeName
        {
            get { return this.eventTypeName; }
            internal set { this.eventTypeName = value; }
        }

        /// <summary>
        /// Developer portal does not currently have the data when an event gets created.
        /// So it is set to the date modified value for sake of consistency across the API.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Gets the modified date for the event. This will be the date when the event last got hits.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }

        /// <summary>
        /// Gets the total hits for the event.
        /// </summary>
        public int TotalHits
        {
            get { return this.totalHits; }
            internal set { this.totalHits = value; }
        }

        /// <summary>
        /// Gets the signature for the event.
        /// </summary>
        public EventSignature Signature
        {
            get { return this.signature; }
            internal set { this.signature = value; }
        }

        /// <summary>
        /// Gets or sets the url to the cab list for the event.
        /// </summary>
        internal Uri CabUrl
        {
            get { return this.cabUrl; }
            set { this.cabUrl = value; }
        }

        /// <summary>
        /// Gets or sets the url to the details (hit data) for the event.
        /// </summary>
        internal Uri DetailsUrl
        {
            get { return this.detailsUrl; }
            set { this.detailsUrl = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to get the event details (hit data).
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Collection of EventInfo objects.</returns>
        public EventInfoCollection GetEventDetails(ref Login loginObject)
        {
            return this.GetEventDetails(DEFAULT_HIT_DAYS, ref loginObject);
        }
        /// <summary>
        /// Method to get the event details (hit data).
        /// </summary>
        /// <param name="days">Number of days of hit data to get.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Collection of EventInfo objects.</returns>
        public EventInfoCollection GetEventDetails(int days, ref Login loginObject)
        {
            //
            // get the number of days to get hit data for.
            //
            if (days < 1)
            {
                throw new ArgumentOutOfRangeException("days", "'days' should be more than zero.");
            }

            Uri requestURL = new Uri(this.DetailsUrl.OriginalString + "&days=" + days.ToString());

            //
            // get the response from the server for the event details url.
            //
            string responseFromServer = Event.GetFeedResponse(requestURL, ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = Event.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // instantiate the event info collection object.
            //
            EventInfoCollection eventInfoCollection = new EventInfoCollection();

            //
            // parse the entry elements and load the event info objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                EventInfo eventInfo = new EventInfo();

                //id - hit date
                eventInfo.HitDateLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText).ToLocalTime();

                // updated - product date modified
                eventInfo.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - product date created
                eventInfo.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // wer:totalHits - total hits
                eventInfo.TotalHits = int.Parse(entryNode.SelectSingleNode("wer:totalHits", namespaceMgr).InnerText);

                // operating system name
                eventInfo.OperatingSystemName = entryNode.SelectSingleNode("wer:operatingSystem", namespaceMgr).Attributes["wer:name"].Value;

                // operating system version
                eventInfo.OperatingSystemVersion = entryNode.SelectSingleNode("wer:operatingSystem", namespaceMgr).Attributes["wer:version"].Value;

                // language name
                eventInfo.Language = entryNode.SelectSingleNode("wer:language", namespaceMgr).Attributes["wer:name"].Value;

                // language lcid
                eventInfo.LCID = int.Parse(entryNode.SelectSingleNode("wer:language", namespaceMgr).Attributes["wer:lcid"].Value);

                // language locale
                eventInfo.Locale = entryNode.SelectSingleNode("wer:language", namespaceMgr).Attributes["wer:locale"].Value;

                //
                // add the eventInfo to the eventInfo collection
                //
                eventInfoCollection.Add(eventInfo);
            }

            //
            // return the eventInfo collection
            //
            return eventInfoCollection;
        }

        /// <summary>
        /// Method to get the event details (hit data).
        /// </summary>
        /// <param name="days">Number of days of hit data to get.</param>
        /// <param name="eventID">The eventID of the WER event</param>
        /// <param name="eventTypeName">The eventTypeName of the WER event, example 'Crash 32bit','Crash 64bit','Special Exception', etc...</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Collection of EventInfo objects.</returns>
        public EventInfoCollection GetEventDetails(int days, Int32 eventID, string eventTypeName, ref Login loginObject)
        {
            
            //
            // setup the eventdetails URL.
            //
            string baseUrl = "https://winqual.microsoft.com";
            string eventDetails = "/services/wer/user/eventdetails.aspx";
            string url = baseUrl + eventDetails + "?eventid=" + eventID + "&eventtypename=" + eventTypeName;

            //
            // get the number of days to get hit data for.
            //
            if (days < 1)
            {
                throw new ArgumentOutOfRangeException("days", "'days' should be more than zero.");
            }

            Uri requestURL = null;

            if (days > 0)
            {
                requestURL = new Uri(url + "&days=" + days.ToString());
            }
            else
            {
                requestURL = new Uri(url);
            }

            //
            // get the response from the server for the event details url.
            //
            string responseFromServer = Event.GetFeedResponse(requestURL, ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = Event.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // instantiate the event info collection object.
            //
            EventInfoCollection eventInfoCollection = new EventInfoCollection();

            //
            // parse the entry elements and load the event info objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                EventInfo eventInfo = new EventInfo();

                //id - hit date
                eventInfo.HitDateLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText).ToLocalTime();

                // updated - product date modified
                eventInfo.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - product date created
                eventInfo.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // wer:totalHits - total hits
                eventInfo.TotalHits = int.Parse(entryNode.SelectSingleNode("wer:totalHits", namespaceMgr).InnerText);

                // operating system name
                eventInfo.OperatingSystemName = entryNode.SelectSingleNode("wer:operatingSystem", namespaceMgr).Attributes["wer:name"].Value;

                // operating system version
                eventInfo.OperatingSystemVersion = entryNode.SelectSingleNode("wer:operatingSystem", namespaceMgr).Attributes["wer:version"].Value;

                // language name
                eventInfo.Language = entryNode.SelectSingleNode("wer:language", namespaceMgr).Attributes["wer:name"].Value;

                // language lcid
                eventInfo.LCID = int.Parse(entryNode.SelectSingleNode("wer:language", namespaceMgr).Attributes["wer:lcid"].Value);

                // language locale
                eventInfo.Locale = entryNode.SelectSingleNode("wer:language", namespaceMgr).Attributes["wer:locale"].Value;

                //
                // add the eventInfo to the eventInfo collection
                //
                eventInfoCollection.Add(eventInfo);
            }

            //
            // return the eventInfo collection
            //
            return eventInfoCollection;
        }
        /// <summary>
        /// Method to get the cab list for the event and event type name.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>Collection of Cab objects.</returns>
        public CabCollection GetCabs(ref Login loginObject)
        {
            //
            // return the cab list using the static method of the cab class.
            //
            return Cab.GetCabs(this.ID, this.EventTypeName, ref loginObject);
        }
        #endregion Public Methods
        #endregion Methods
    }
}
