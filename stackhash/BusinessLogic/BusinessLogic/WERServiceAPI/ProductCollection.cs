//---------------------------------------------------------------------
// <summary>
//      Class for representing a collection of Product objects in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for representing a collection of Product objects in the API.
    /// </summary>
    public class ProductCollection : List<Product>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ProductCollection class that is empty and has the default initial capacity.
        /// </summary>
        public ProductCollection()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the ProductCollection class that is empty and has the specified initial capacity. 
        /// </summary>
        /// <param name="capacity"></param>
        public ProductCollection(int capacity)
            : base(capacity)
        { }
        #endregion Constructors
    }
}
