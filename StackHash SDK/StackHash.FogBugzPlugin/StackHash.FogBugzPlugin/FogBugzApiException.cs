using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHash.FogBugzPlugin
{
    /// <summary>
    /// Exception thrown when an error occurs accessing the FogBugz API
    /// </summary>
    [Serializable]
    public sealed class FogBugzApiException : Exception, ISerializable
    {
        /// <summary>
        /// Gets the error code returned from the FogBugz API (-1 if not set)
        /// </summary>
        public int FogBugzApiErrorCode { get; private set; }
        private const string FogBugzApiErrorCodeSerializeName = "FogBugzApiErrorCode";

        /// <summary>
        /// Exception thrown when an error occurs accessing the FogBugz API
        /// </summary>
        public FogBugzApiException()
            : this(null, null, -1) {}

        /// <summary>
        /// Exception thrown when an error occurs accessing the FogBugz API
        /// </summary>
        /// <param name="message">Message</param>
        public FogBugzApiException(string message)
            : this(message, null, -1) {}

        /// <summary>
        /// Exception thrown when an error occurs accessing the FogBugz API
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exception</param>
        public FogBugzApiException(string message, Exception innerException)
            : this(message, innerException, -1) {}

        /// <summary>
        /// Exception thrown when an error occurs accessing the FogBugz API
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="fogBugzApiErrorCode">FogBugz API Error Code</param>
        public FogBugzApiException(string message, int fogBugzApiErrorCode)
            : this(message, null, fogBugzApiErrorCode) {}

        /// <summary>
        /// Exception thrown when an error occurs accessing the FogBugz API
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exception</param>
        /// <param name="fogBugzApiErrorCode">FogBugz API Error Code</param>
        public FogBugzApiException(string message, Exception innerException, int fogBugzApiErrorCode)
            : base(message, innerException)
        {
            this.FogBugzApiErrorCode = fogBugzApiErrorCode;
        }
        
        /// <summary>
        /// Exception thrown when an error occurs accessing the FogBugz API
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        private FogBugzApiException(SerializationInfo info, StreamingContext context)  
            : base(info, context)
        {
            if (info == null)
            {
                this.FogBugzApiErrorCode = -1;
            }
            else
            {
                this.FogBugzApiErrorCode = info.GetInt32(FogBugzApiErrorCodeSerializeName);
            }
        }

        #region ISerializable Members

        /// <summary />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                base.GetObjectData(info, context);
                info.AddValue(FogBugzApiErrorCodeSerializeName, this.FogBugzApiErrorCode);
            }
        }

        #endregion
    }
}
