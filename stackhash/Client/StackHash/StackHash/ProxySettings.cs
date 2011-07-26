using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// Proxy settings - used to pass settings to and from a ProxySettingsControl
    /// </summary>
    public class ProxySettings : IDataErrorInfo
    {
        /// <summary>
        /// True if the IDataErrorInfo interface is enabled for validation
        /// </summary>
        public bool ValidationEnabled { get; set; }
        
        /// <summary>
        /// True to use a proxy server
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// True if the proxy server requires authentication
        /// </summary>
        public bool UseProxyAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the proxy host
        /// </summary>
        public string ProxyHost { get; set; }

        /// <summary>
        /// Gets or sets the proxy port
        /// </summary>
        public int ProxyPort { get; set; }

        /// <summary>
        /// Gets or sets the proxy username
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username")]
        public string ProxyUsername { get; set; }

        /// <summary>
        /// Gets or sets the proxy password
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// Gets or sets the proxy domain
        /// </summary>
        public string ProxyDomain { get; set; }

        /// <summary>
        /// Proxy settings - used to pass settings to and from a ProxySettingsControl
        /// </summary>
        /// <param name="useProxy">True to use a proxy server</param>
        /// <param name="useProxyAuthentication">True if the proxy server requires validation</param>
        /// <param name="proxyHost">The proxy host</param>
        /// <param name="proxyPort">The proxy port</param>
        /// <param name="proxyUsername">The proxy username</param>
        /// <param name="proxyPassword">The proxy password</param>
        /// <param name="proxyDomain">The proxy domain</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username")]
        public ProxySettings(bool useProxy,
            bool useProxyAuthentication,
            string proxyHost,
            int proxyPort,
            string proxyUsername,
            string proxyPassword,
            string proxyDomain)
        {
            this.UseProxy = useProxy;
            this.UseProxyAuthentication = useProxyAuthentication;
            this.ProxyHost = proxyHost;
            this.ProxyPort = proxyPort;
            this.ProxyUsername = proxyUsername;
            this.ProxyPassword = proxyPassword;
            this.ProxyDomain = proxyDomain;
        }

        #region IDataErrorInfo Members

        /// <summary />
        public string Error
        {
            get { return null; }
        }

        /// <summary />
        public string this[string columnName]
        {
            get 
            {
                if (!ValidationEnabled)
                {
                    return null;
                }

                string result = null;

                switch (columnName)
                {
                    case "ProxyHost":
                        if (this.UseProxy)
                        {
                            if (string.IsNullOrEmpty(this.ProxyHost))
                            {
                                result = Properties.Resources.ProxySettings_ValidationHost;
                            }
                        }
                        break;

                    case "ProxyPort":
                        if (this.UseProxy)
                        {
                            if ((this.ProxyPort < 0) || (this.ProxyPort > 65535))
                            {
                                result = Properties.Resources.ProxySettings_ValidationPort;
                            }
                        }
                        break;
                }

                return result;
            }
        }

        #endregion
    }
}
