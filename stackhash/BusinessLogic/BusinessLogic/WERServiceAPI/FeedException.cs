//---------------------------------------------------------------------
// <summary>
//      Use this exception class for user errors in the feed. User errors are
//      errors that can be rectified by the user and should be communicated back
//      to the user.
// </summary>
//---------------------------------------------------------------------

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.UI.HtmlControls;
    using System.Runtime.Serialization;

    /// <summary>
    /// Use this exception class for user errors in the feed. User errors are
    /// errors that can be rectified by the user and should be communicated back
    /// to the user.
    /// </summary>
    internal class FeedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the FeedException class.
        /// </summary>
        public FeedException()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the FeedException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FeedException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the FeedException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public FeedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the FeedException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public FeedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}