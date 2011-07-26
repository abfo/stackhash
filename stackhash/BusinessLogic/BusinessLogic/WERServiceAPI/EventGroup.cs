//---------------------------------------------------------------------
// <summary>
//      Class for representing an Event group in the API.
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
    /// Class to represent an event group.
    /// </summary>
    internal class EventGroup : Base
    {
        #region Fields
        private DateTime eventGroupDateLocal;
        private int totalEvents;
        private Uri eventsUrl;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets or sets the event group date.
        /// </summary>
        internal DateTime EventGroupDateLocal
        {
            get { return this.eventGroupDateLocal; }
            set { this.eventGroupDateLocal = value; }
        }

        /// <summary>
        /// Gets or sets the total events for the event group.
        /// </summary>
        internal int TotalEvents
        {
            get { return this.totalEvents; }
            set { this.totalEvents = value; }
        }

        /// <summary>
        /// Gets or sets the Url to the events feed.
        /// </summary>
        public Uri EventsUrl
        {
            get { return this.eventsUrl; }
            internal set { this.eventsUrl = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        #endregion Public Methods
        #endregion Methods
    }
}
