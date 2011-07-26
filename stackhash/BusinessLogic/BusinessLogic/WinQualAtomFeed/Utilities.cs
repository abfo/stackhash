//---------------------------------------------------------------------
// <summary>
//      Class for helper functions on WER Service Client API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Microsoft.Whos.Shared.Client.LiveID;

namespace WinQualAtomFeed
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
            string serviceHost = Constants.ServiceName;
            string serviceProtocol = Constants.ServiceProtocolHttps;

            string host = GetRegistrySetting(Constants.SettingHost);
            if (!String.IsNullOrEmpty(host))
            {
                serviceHost = host;
            }

            string ssl = GetRegistrySetting(Constants.SettingSSL);
            if (!string.IsNullOrEmpty(ssl) &&
                (ssl.Equals(Boolean.FalseString, StringComparison.OrdinalIgnoreCase)))
            {
                serviceProtocol = Constants.ServiceProtocolHttp;
            }

            return String.Format(urlFormat, serviceProtocol, serviceHost);
        }

        /// <summary>
        /// Helper method to initialize the LiveID client with
        /// overridable registry settings. The default settings
        /// are used for each of the non existent registry entries.
        /// </summary>
        /// <returns>LiveID client instance</returns>
        internal static LiveIdAuthentication GetLiveIdAuth()
        {
            string environment = Constants.LiveEnvironment;
            string hostApplicationGUID = Constants.LiveHostAppGuid;
            string hostApplicationName = Constants.LiveHostAppName;
            string serviceName = Constants.ServiceName;
            string serviceAuthenticationPolicy = Constants.LiveAuthPolicySSL;

            //string host = GetRegistrySetting(Constants.SettingHost);
            //if (!String.IsNullOrEmpty(host))
            //{
            //    serviceName = host;
            //}

            //string ssl = GetRegistrySetting(Constants.SettingSSL);
            //if (!string.IsNullOrEmpty(ssl) && (ssl.Equals(Boolean.FalseString, StringComparison.OrdinalIgnoreCase)))
            //{
            //    serviceAuthenticationPolicy = Constants.LiveAuthPolicy;
            //}

            //string env = GetRegistrySetting(Constants.SettingEnvironment);
            //if (!string.IsNullOrEmpty(env))
            //{
            //    environment = env;
            //}

            // When running in a service environment, the windows live id dll wlidcli.dll needs to be on the path.
            String path = Environment.GetEnvironmentVariable("Path");
            
            String commonProgramFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles, Environment.SpecialFolderOption.None);
            String commonProgramFilesFolderX86 = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86, Environment.SpecialFolderOption.None);

            String searchPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles, Environment.SpecialFolderOption.None) 
                + "\\microsoft shared\\windows live";
            String searchPathX86 = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles, Environment.SpecialFolderOption.None)
                + "\\microsoft shared\\windows live";

            if (!path.ToUpperInvariant().Contains(searchPath.ToUpperInvariant()))
                path += ";" + searchPath;
            Environment.SetEnvironmentVariable("Path", path);

            if (!path.ToUpperInvariant().Contains(searchPathX86.ToUpperInvariant()))
                path += ";" + searchPathX86;
            Environment.SetEnvironmentVariable("Path", path);

            return new LiveIdAuthentication(
                environment,
                new Guid(hostApplicationGUID),
                hostApplicationName,
                serviceName,
                serviceAuthenticationPolicy,
                true);
        }

        private static string GetRegistrySetting(string settingName)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Constants.ServiceRegistryKey))
            {
                if (null != key)
                {
                    return key.GetValue(settingName) as string;
                }
                else
                {
                    return null;
                }
            };
        }
    }
}
