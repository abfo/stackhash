//---------------------------------------------------------------------
// <summary>
//      Class for representing an event signature in the API.
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
    /// Class to represent an event signature.
    /// </summary>
    public class EventSignature : Base
    {
        #region Fields
        private ParameterCollection parameters;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the event signature parameter collection.
        /// </summary>
        public ParameterCollection Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new ParameterCollection();
                }
                return this.parameters;
            }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        #endregion Public Methods
        #endregion Methods
    }
}
