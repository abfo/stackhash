//---------------------------------------------------------------------
// <summary>
//      Class for representing parameters of the event signature in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for representing parameters of the event signature in the API.
    /// </summary>
    public class ParameterCollection : List<Parameter>
    {
        #region Fields
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ParameterCollection class that is empty and has the default initial capacity. 
        /// </summary>
        public ParameterCollection()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the ParameterCollection class that is empty and has the specified initial capacity. 
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public ParameterCollection(int capacity)
            : base(capacity)
        { }

        /// <summary>
        /// Initializes a new instance of the ParameterCollection class that contains elements copied from the specified collection
        /// and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public ParameterCollection(IEnumerable<Parameter> collection)
            : base(collection)
        { }
        #endregion Constructors

        #region Properties
        #region Public Properties
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        #endregion Public Methods
        #endregion Methods
    }
}
