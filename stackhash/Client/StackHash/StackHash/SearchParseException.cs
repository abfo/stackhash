using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHash
{
    /// <summary>
    /// Exception thrown when SearchBuilder fails to parse a search string
    /// </summary>
    [Serializable]
    public class SearchParseException : Exception
    {
        /// <summary>
        /// Exception thrown when SearchBuilder fails to parse a search string
        /// </summary>
        public SearchParseException()
            : base() { }

        /// <summary>
        /// Exception thrown when SearchBuilder fails to parse a search string
        /// </summary>
        /// <param name="message">Message</param>
        public SearchParseException(string message)
            : base(message) { }

        /// <summary>
        /// Exception thrown when SearchBuilder fails to parse a search string
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exception</param>
        public SearchParseException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Exception thrown when SearchBuilder fails to parse a search string
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected SearchParseException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
