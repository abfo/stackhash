//---------------------------------------------------------------------
// <summary>
//      Class for enumerating through the event pages.
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
    /// Class for enumerating through the event pages.
    /// </summary>
    public class EventPageReader : Base
    {
        #region Fields
        private EventReader events;
        private Uri originalEventsUrl;
        private Uri nextPageEventsUrl;
        private Uri previousPageEventsUrl;
        private int totalPages;
        private int currentPage;
        private DateTime startDateLocal;
        private DateTime endDateLocal;
        private int recordIndex;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Internal constructor for initializing fields.
        /// </summary>
        /// <param name="eventsUrl">Url to the events feed.</param>
        internal EventPageReader(Uri eventsUrl)
        {
            this.events = null;
            this.originalEventsUrl = eventsUrl;
            this.nextPageEventsUrl = null;
            this.previousPageEventsUrl = null;
            this.recordIndex = -1;
        }
        #endregion Constructors

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the reader for the events.
        /// </summary>
        public EventReader Events
        {
            get { return this.events; }
            internal set { this.events = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Advances the EventPageReader to the next record.
        /// </summary>
        /// <returns>true if there are more rows; otherwise false.</returns>
        public bool Read(ref Login loginObject)
        {
            //
            //
            //
            this.recordIndex++;
            //
            // get the feed response string
            // 
            string responseFromServer = EventPageReader.GetFeedResponse(this.originalEventsUrl, ref loginObject);

            //
            // namespace manager object
            //
            XmlNamespaceManager namespaceMgr;

            //
            // get the feed node
            //
            XmlNode feedNode = EventPageReader.GetFeedNode(responseFromServer, out namespaceMgr);

            //
            // populate the next page, prev page, total pages, current page, start date, end date values.
            //
            XmlNode nextPageNode = feedNode.SelectSingleNode("atom:link[@rel='next']", namespaceMgr);
            if (nextPageNode != null)
            {
                this.nextPageEventsUrl = new Uri(nextPageNode.Attributes["href"].Value);
                this.originalEventsUrl = this.nextPageEventsUrl;
            }

            XmlNode previousPageNode = feedNode.SelectSingleNode("atom:link[@rel='previous']", namespaceMgr);
            if (previousPageNode != null)
            {
                this.previousPageEventsUrl = new Uri(previousPageNode.Attributes["href"].Value);
            }

            XmlNode totalPagesNode = feedNode.SelectSingleNode("wer:totalPages", namespaceMgr);
            if (totalPagesNode != null)
            {
                this.totalPages = int.Parse(totalPagesNode.InnerText);
            }

            XmlNode currentPageNode = feedNode.SelectSingleNode("wer:currentPage", namespaceMgr);
            if (currentPageNode != null)
            {
                this.currentPage = int.Parse(currentPageNode.InnerText);
            }

            XmlNode startDateNode = feedNode.SelectSingleNode("wer:startDate", namespaceMgr);
            if (startDateNode != null)
            {
                this.startDateLocal = DateTime.Parse(startDateNode.InnerText).ToLocalTime();
            }

            XmlNode endDateNode = feedNode.SelectSingleNode("wer:endDate", namespaceMgr);
            if (endDateNode != null)
            {
                this.endDateLocal = DateTime.Parse(endDateNode.InnerText).ToLocalTime();
            }

            //
            // if we do not have any event data then total pages will be 0, in which case return false.
            //
            if (this.totalPages == 0)
            {
                return false;
            }

            if (recordIndex == this.totalPages)
            {
                return false;
            }

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = EventPageReader.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // create an empty events list
            //
            List<Event> eventList = new List<Event>(entryNodes.Count);

            //
            // parse the entry elements and load the event objects
            // and add them to the event group collection
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                Event eventObj = new Event();

                // event id
                eventObj.ID = int.Parse(entryNode.SelectSingleNode("wer:eventID", namespaceMgr).InnerText);

                // event type name
                eventObj.EventTypeName = entryNode.SelectSingleNode("wer:eventTypeName", namespaceMgr).InnerText;

                // total hits
                eventObj.TotalHits = int.Parse(entryNode.SelectSingleNode("wer:totalHits", namespaceMgr).InnerText);

                // updated - file date modified
                eventObj.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - file date created
                eventObj.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // link[rel='cabs'] - cab list url
                XmlNode cabUrlNode = entryNode.SelectSingleNode("atom:link[@rel='cabs']", namespaceMgr);
                if (cabUrlNode != null)
                {
                    eventObj.CabUrl = new Uri(cabUrlNode.Attributes["href"].Value);
                }

                // link[rel='details'] - event details (hit data) url
                XmlNode detailsUrlNode = entryNode.SelectSingleNode("atom:link[@rel='details']", namespaceMgr);
                if (detailsUrlNode != null)
                {
                    eventObj.DetailsUrl = new Uri(detailsUrlNode.Attributes["href"].Value);
                }

                //
                // get the wer:signature element.
                //
                XmlNode signatureNode = entryNode.SelectSingleNode("wer:signature", namespaceMgr);

                if (signatureNode != null)
                {
                    EventSignature eventSignature = new EventSignature();

                    //
                    // get all the parameter nodes.
                    //
                    XmlNodeList parameterNodeList = signatureNode.SelectNodes("wer:parameter", namespaceMgr);

                    if (parameterNodeList != null)
                    {
                        //
                        // loop through all the parameters and add them to the parameter collection.
                        //
                        foreach (XmlNode parameterNode in parameterNodeList)
                        {
                            Parameter parameter = new Parameter(parameterNode.Attributes["wer:name"].Value, parameterNode.Attributes["wer:value"].Value);
                            eventSignature.Parameters.Add(parameter);
                        }
                    }

                    //
                    // assign the event signature to the Signature property.
                    //
                    eventObj.Signature = eventSignature;
                }

                //
                // add the event to the event list.
                //
                eventList.Add(eventObj);
            }

            //
            // create the events reader from the event list.
            //
            EventReader eventReader = new EventReader(eventList);

            //
            // assign the event reader to the Events property.
            //
            this.Events = eventReader;

            return true;
        }
        #endregion Public Methods
        #endregion Methods
    }
}
