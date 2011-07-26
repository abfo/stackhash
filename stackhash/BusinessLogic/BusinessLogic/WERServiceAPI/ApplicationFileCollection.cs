//---------------------------------------------------------------------
// <summary>
//      Class for representing a collection of ApplicationFile's in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for representing a collection of ApplicationFile objects in the API.
    /// </summary>
    public class ApplicationFileCollection : List<ApplicationFile>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ApplicationFileCollection class that is empty and has the default initial capacity.
        /// </summary>
        public ApplicationFileCollection()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the ApplicationFileCollection class that is empty and has the specified initial capacity. 
        /// </summary>
        /// <param name="capacity"></param>
        public ApplicationFileCollection(int capacity)
            : base(capacity)
        { }
        #endregion Constructors
    }
}
