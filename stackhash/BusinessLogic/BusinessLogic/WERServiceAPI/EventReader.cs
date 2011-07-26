//---------------------------------------------------------------------
// <summary>
//      Class for enumerating through the events.
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
    /// Class for enumerating through the events.
    /// </summary>
    public class EventReader : Base
    {
        #region Fields
        private Event eventObj;
        private List<Event> events;
        private int recordIndex;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EventReader()
        {
        }

        /// <summary>
        /// Constructor that initializes the events list.
        /// </summary>
        /// <param name="events"></param>
        public EventReader(List<Event> events)
        {
            this.events = events;
            this.recordIndex = -1;
        }
        #endregion Constructors

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the reader for the events.
        /// </summary>
        public Event Event
        {
            get { return this.eventObj; }
            internal set { this.eventObj = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Advances the EventReader to the next record.
        /// </summary>
        /// <returns>true if there are more rows; otherwise false.</returns>
        public bool Read()
        {
            if (this.events.Count == 0)
            {
                return false;
            }

            this.recordIndex++;

            if (this.recordIndex == this.events.Count)
            {
                return false;
            }

            this.Event = this.events[recordIndex];
            return true;
        }
        #endregion Public Methods
        #endregion Methods
    }
}
