//---------------------------------------------------------------------
// <summary>
//      Class for representing the collection of event groups.
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
    /// Class for representing the cab collection of the event in the API.
    /// </summary>
    internal class EventGroupCollection : List<EventGroup>
    {
        #region Fields
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the EventGroupCollection class that is empty and has the default initial capacity. 
        /// </summary>
        internal EventGroupCollection()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the EventGroupCollection class that is empty and has the specified initial capacity. 
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        internal EventGroupCollection(int capacity)
            : base(capacity)
        { }

        /// <summary>
        /// Initializes a new instance of the EventGroupCollection class that contains elements copied from the specified collection
        /// and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        internal EventGroupCollection(IEnumerable<EventGroup> collection)
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
