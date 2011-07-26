//---------------------------------------------------------------------
// <summary>
//      Class that represents an authentication exception.
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
    /// Class that represents an authentication exception.
    /// </summary>
    internal class FeedAuthenticationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the FeedAuthenticationException class. Passes
        /// in the default message to the base constructor.
        /// </summary>
        public FeedAuthenticationException() : base(DataServicesResources.GENERAL_AUTHENTICATION_EXCEPTION)
        {
            this.Data.Add(
                DataServicesResources.AUTHENTICATION_FAILURE_DATA_STRING + "1"
                , DataServicesResources.BAD_USERNAME_PASSWORD);
            this.Data.Add(
                DataServicesResources.AUTHENTICATION_FAILURE_DATA_STRING + "2"
                , DataServicesResources.PASSWORD_EXPIRED);
            this.Data.Add(
                DataServicesResources.AUTHENTICATION_FAILURE_DATA_STRING + "3"
                , DataServicesResources.PASSWORD_LOCKED);
            this.Data.Add(
                DataServicesResources.AUTHENTICATION_FAILURE_DATA_STRING + "4"
                , DataServicesResources.COMPANY_DISABLED);
        }

        /// <summary>
        /// Initializes a new instance of the FeedAuthenticationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FeedAuthenticationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the FeedAuthenticationException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public FeedAuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the FeedAuthenticationException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public FeedAuthenticationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}