//---------------------------------------------------------------------
// <summary>
//      Class for representing a File in the API. This object represents
//      the file mapped in the Developer Portal.
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
    /// Class for representing a File in the API. This object represents
    /// the file mapped in the Developer Portal.
    /// </summary>
    public class ApplicationFile : Base
    {
        #region Fields
        private int id;
        private string name;
        private string version;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        private DateTime linkDateLocal;
        private Uri eventsLink;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the identifier used to represent the file uniquely when storing
        /// in the database.
        /// </summary>
        public int ID
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            internal set { this.name = value; }
        }

        /// <summary>
        /// Gets the version of the file.
        /// </summary>
        public string Version
        {
            get { return this.version; }
            internal set { this.version = value; }
        }

        /// <summary>
        /// Gets the date when the file was mapped in the Developer Portal.
        /// This date is represented in local date-time.
        /// </summary>
        public DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Gets the date when one of the properties of the file was changed.
        /// This date is represented in local date-time.
        /// </summary>
        public DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }

        /// <summary>
        /// Gets the linker date of the file.
        /// This date is represented in local date-time.
        /// </summary>
        public DateTime LinkDateLocal
        {
            get { return this.linkDateLocal; }
            internal set { this.linkDateLocal = value; }
        }

        /// <summary>
        /// Gets or sets the url to the events for the file.
        /// </summary>
        internal Uri EventsLink
        {
            get { return this.eventsLink; }
            set { this.eventsLink = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to get all the event pages for the file.
        /// </summary>
        /// <returns>EventPageReader object containing event pages.</returns>
        public EventPageReader GetEvents()
        {
            //
            // call the helper method to get the events page reader.
            //
            return this.GetEvents(this.EventsLink);
        }

        /// <summary>
        /// Method to get all the event pages for the file from the start date parameter.
        /// </summary>
        /// <param name="startDateLocal">Gets all the events modified from this date. Pass in the local data-time for this parameter.</param>
        /// <returns>EventPageReader object containing event pages.</returns>
        public EventPageReader GetEvents(DateTime startDateLocal)
        {
            //
            // create the events url
            //
            Uri eventsUrl = new Uri(
                string.Format(
                    "{0}&startdate={1:s}Z"
                    , this.EventsLink
                    , (startDateLocal.Kind == DateTimeKind.Utc ? startDateLocal : startDateLocal.ToUniversalTime())
                )
            );

            //
            // call the helper method to get the events page reader.
            //
            return this.GetEvents(eventsUrl);
        }

        /// <summary>
        /// Method to get all the event pages for the file from the start date parameter till the end date parameter.
        /// </summary>
        /// <param name="startDateLocal">Gets all the events modified from this date. Pass in the local data-time for this parameter.</param>
        /// <param name="endDateLocal">Gets all the events modified till this date. Pass in the local data-time for this parameter.</param>
        /// <returns>EventPageReader object containing event pages.</returns>
        public EventPageReader GetEvents(DateTime startDateLocal, DateTime endDateLocal)
        {
            //
            // set the events url
            //
            Uri eventsUrl = new Uri(
                string.Format(
                    "{0}&startdate={1:s}Z&enddate={2:s}Z"
                    , this.EventsLink
                    , (startDateLocal.Kind == DateTimeKind.Utc ? startDateLocal : startDateLocal.ToUniversalTime())
                    , (endDateLocal.Kind == DateTimeKind.Utc ? endDateLocal : endDateLocal.ToUniversalTime())
                )
            );

            //
            // call the helper method to get the events page reader.
            //
            return this.GetEvents(eventsUrl);
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Helper method to get the event page reader from the events URL.
        /// </summary>
        /// <param name="eventsUrl">Uri for event list.</param>
        /// <returns>EventPageReader object containing the page list of events.</returns>
        private EventPageReader GetEvents(Uri eventsUrl)
        {
            //
            // create the EventPageReader object and pass in the events url.
            //
            EventPageReader eventPageReader = new EventPageReader(eventsUrl);

            //
            // return the reader object.
            //
            return eventPageReader;
        }
        #endregion Private Methods
        #endregion Methods
    }
}
