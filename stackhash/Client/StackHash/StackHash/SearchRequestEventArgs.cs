using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Event args requesing a StackHash search
    /// </summary>
    public class SearchRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the search
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// Event args requesing a StackHash search
        /// </summary>
        /// <param name="search">The search</param>
        public SearchRequestEventArgs(string search)
        {
            Debug.Assert(search != null);

            this.Search = search;
        }
    }
}
