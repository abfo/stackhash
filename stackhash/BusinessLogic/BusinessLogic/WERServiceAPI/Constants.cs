//---------------------------------------------------------------------
// <summary>
//      Class for holding the constant values used in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for holding the constant values used in the API.
    /// </summary>
    class Constants
    {
        /// <summary>
        /// Web Service URL for Authentication
        /// </summary>
        public const string authenticationURL = "{0}://{1}/services/Authentication/Authentication.svc/BasicTicket";

        /// <summary>
        /// Client API Service URL
        /// </summary>
        public const string baseServiceURL = "{0}://{1}/services/wer/user/";
        
        /// <summary>
        /// Service Registry Key for WER
        /// </summary>
        public const string serviceRegistryKey = @"SOFTWARE\Microsoft\WER\DataService";

        /// <summary>
        /// Default Service Protocol for Web API
        /// </summary>
        public const string serviceProtocol = "https";

        /// <summary>
        /// Default Service Host for Web API
        /// </summary>
        public const string serviceHost = "winqual.microsoft.com";
    }
}
