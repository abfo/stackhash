//---------------------------------------------------------------------
// <summary>
//      Use this exception class for user errors . User errors are
//      errors that can be rectified by the user and should be communicated back
//      to the user.
// </summary>
//---------------------------------------------------------------------

namespace StackHashBusinessObjects
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Runtime.Serialization;
    using System.Security.Permissions;


    /// <summary>
    /// Use this exception class for user errors. User errors are
    /// errors that can be rectified by the user and should be communicated back
    /// to the user.
    /// </summary>
    [Serializable]
    public class StackHashException : Exception
    {
        StackHashServiceErrorCode m_ServiceErrorCode;


        public StackHashServiceErrorCode ServiceErrorCode
        {
            get { return m_ServiceErrorCode; }
        }

        /// <summary>
        /// Initializes a new instance of the StackHashException class.
        /// </summary>
        public StackHashException()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the StackHashException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public StackHashException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the StackHashException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public StackHashException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the StackHashException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">Service error code provided for client.</param>
        public StackHashException(string message, StackHashServiceErrorCode errorCode)
            : base(message)
        { m_ServiceErrorCode = errorCode; }

        /// <summary>
        /// Initializes a new instance of the StackHashException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="errorCode">Service error code provided for client.</param>
        public StackHashException(string message, Exception innerException, StackHashServiceErrorCode errorCode)
            : base(message, innerException)
        { m_ServiceErrorCode = errorCode; }


        /// <summary>
        /// Initializes a new instance of the StackHashException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected StackHashException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.m_ServiceErrorCode = (StackHashServiceErrorCode)info.GetInt32("m_ServiceErrorCode");
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
                info.AddValue("m_ServiceErrorCode", m_ServiceErrorCode);
        }

        
        /// <summary>
        /// Determines the error code to return to the client.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static StackHashServiceErrorCode GetServiceErrorCode(System.Exception ex)
        {
            if (ex == null)
                return StackHashServiceErrorCode.NoError;

            StackHashException stackHashEx = ex as StackHashException;

            if (stackHashEx != null)
            {
                return stackHashEx.ServiceErrorCode;
            }
            else if (ex is OperationCanceledException)
            {
                return StackHashServiceErrorCode.Aborted;
            }
            else
            {
                return StackHashServiceErrorCode.UnexpectedError;
            }
        }
    }
}