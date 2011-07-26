using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHash
{
    /// <summary>
    /// Exception thrown when the StackHash service reports a message as
    /// a string through an AdminReport
    /// </summary>
    [Serializable]
    public class AdminReportException : Exception
    {
        /// <summary>
        /// Exception thrown when the StackHash service reports a message as
        /// a string through an AdminReport
        /// </summary>
        public AdminReportException()
            : base() { }

        /// <summary>
        /// Exception thrown when the StackHash service reports a message as
        /// a string through an AdminReport
        /// </summary>
        /// <param name="message">Message</param>
        public AdminReportException(string message)
            : base(message) { }

        /// <summary>
        /// Exception thrown when the StackHash service reports a message as
        /// a string through an AdminReport
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exception</param>
        public AdminReportException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Exception thrown when the StackHash service reports a message as
        /// a string through an AdminReport
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected AdminReportException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
