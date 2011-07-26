//---------------------------------------------------------------------
// <summary>
//      Class for holding the constant values used in the API.
// </summary>
//---------------------------------------------------------------------

namespace WinQualAtomFeed
{
    /// <summary>
    /// Class for holding the constant values used in the API.
    /// </summary>
    class Constants
    {
        /// <summary>
        /// Client API Service URL
        /// </summary>
        public const string BaseServiceUrl = "{0}://{1}/services/wer/user/";

        /// <summary>
        /// Service Registry Key for WER
        /// </summary>
        public const string ServiceRegistryKey = @"SOFTWARE\Microsoft\WER\DataService";

        /// <summary>
        /// Default Service Protocol for Web API (HTTPS)
        /// </summary>
        public const string ServiceProtocolHttps = "https";

        /// <summary>
        /// Service Protocol for Web API (HTTP)
        /// </summary>
        public const string ServiceProtocolHttp = "http";

        /// <summary>
        /// Default Service Name for Web API
        /// </summary>
        public const string ServiceName = "winqual.microsoft.com";

        /// <summary>
        /// Live ID environment
        /// </summary>
        public const string LiveEnvironment = "";

        /// <summary>
        /// Live ID Host App Guid
        /// </summary>
        public const string LiveHostAppGuid = "{D482C198-D080-4DB7-9E8A-E19AA18CE61B}";

        /// <summary>
        /// Live ID Host App Name
        /// </summary>
        public const string LiveHostAppName = "StackHash";

        /// <summary>
        /// Default Live ID Auth Policy (MBI_SSL)
        /// </summary>
        public const string LiveAuthPolicySSL = "MBI_SSL";

        /// <summary>
        /// Live ID Auth Policy (MBI)
        /// </summary>
        public const string LiveAuthPolicy = "MBI";

        /// <summary>
        /// Registry value (Host) setting the Service Name for Web API
        /// </summary>
        public const string SettingHost = "Host";

        /// <summary>
        /// Registry value (SSL) setting the Service Protocol for Web API
        /// </summary>
        public const string SettingSSL = "SSL";

        /// <summary>
        /// Registry value (Environment) setting the Environment for Web API
        /// </summary>
        public const string SettingEnvironment = "Environment";
    }
}
