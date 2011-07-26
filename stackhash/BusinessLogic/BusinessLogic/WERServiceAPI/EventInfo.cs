//---------------------------------------------------------------------
// <summary>
//      Class for representing the Event info (hit data) in the API.
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
    /// Class for representing the Event info (hit data) in the API.
    /// </summary>
    public class EventInfo : Base
    {
        #region Fields
        private DateTime hitDateLocal;
        private DateTime dateCreatedLocal;
        private DateTime dateModifiedLocal;
        private int totalHits;
        private string operatingSystemName;
        private string operatingSystemVersion;
        private int lcid;
        private string locale;
        private string language;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the date of the hit for the event.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime HitDateLocal
        {
            get { return this.hitDateLocal; }
            internal set { this.hitDateLocal = value; }
        }

        /// <summary>
        /// Gets the date created for the event info object. This will be the hit date.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime DateCreatedLocal
        {
            get { return this.dateCreatedLocal; }
            internal set { this.dateCreatedLocal = value; }
        }

        /// <summary>
        /// Gets the modified date for the event info object. This will be the hit date.
        /// This date is represented in the local date-time format.
        /// </summary>
        public DateTime DateModifiedLocal
        {
            get { return this.dateModifiedLocal; }
            internal set { this.dateModifiedLocal = value; }
        }

        /// <summary>
        /// Gets the total hits for the hit date for the event.
        /// </summary>
        public int TotalHits
        {
            get { return this.totalHits; }
            internal set { this.totalHits = value; }
        }

        /// <summary>
        /// Gets the name of the operating system.
        /// </summary>
        public string OperatingSystemName
        {
            get { return this.operatingSystemName; }
            internal set { this.operatingSystemName = value; }
        }

        /// <summary>
        /// Gets the version of the operating system.
        /// </summary>
        public string OperatingSystemVersion
        {
            get { return this.operatingSystemVersion; }
            internal set { this.operatingSystemVersion = value; }
        }

        /// <summary>
        /// Gets the LCID (locale ID) for the event hit.
        /// </summary>
        public int LCID
        {
            get { return this.lcid; }
            internal set { this.lcid = value; }
        }

        /// <summary>
        /// Gets the locale for the event hit.
        /// </summary>
        public string Locale
        {
            get { return this.locale; }
            internal set { this.locale = value; }
        }

        /// <summary>
        /// Gets the language for the event hit.
        /// </summary>
        public string Language
        {
            get { return this.language; }
            internal set { this.language = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        #endregion Public Methods
        #endregion Methods
    }
}
