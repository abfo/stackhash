//---------------------------------------------------------------------
// <summary>
//      Class for helper functions on WER Service Client API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for helper functions on WER Service Client API.
    /// </summary>
    class Utility
    {
        /// <summary>
        /// Helper method to create service URL by registry key
        /// If the registry key does not exist, return it with the default host
        /// and SSL value. 
        /// </summary>
        /// <param name="urlFormat">Service URL format to return</param>
        /// <returns>url string</returns>
        public static string GetServiceURL(string urlFormat)
        {
            string serviceHost = Constants.serviceHost;
            string serviceProtocol = Constants.serviceProtocol;

            RegistryKey werKey = Registry.LocalMachine.OpenSubKey(Constants.serviceRegistryKey);
            if (werKey != null)
            {
                string host = werKey.GetValue("Host") as string;
                if (!string.IsNullOrEmpty(host))
                {
                    serviceHost = host;
                }

                string ssl = werKey.GetValue("SSL") as string;
                if (!string.IsNullOrEmpty(ssl) && Boolean.FalseString == ssl)
                {
                    serviceProtocol = "http";
                }
            }

            return String.Format(urlFormat, serviceProtocol, serviceHost);
        }
    }
}
