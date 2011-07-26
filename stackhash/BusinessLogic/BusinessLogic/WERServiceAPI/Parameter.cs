//---------------------------------------------------------------------
// <summary>
//      Class for representing a parameter in the event signature in the API.
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
    /// Class for representing a parameter in the event signature in the API.
    /// </summary>
    public class Parameter : Base
    {
        #region Fields
        private string name;
        private string value;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Parameter()
        {
        }

        /// <summary>
        /// Constructor that takes in the name and value of the parameter.
        /// </summary>
        /// <param name="name">Specifies the name of the parameter.</param>
        /// <param name="value">Specifies the value of the parameter.</param>
        public Parameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        #endregion Constructors

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            internal set { this.name = value; }
        }

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public string Value
        {
            get { return this.value; }
            internal set { this.value = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        #endregion Public Methods
        #endregion Methods
    }
}
