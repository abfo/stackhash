using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProxySettings : ICloneable
    {
        private bool m_UserProxy;
        private bool m_UseProxyAuthentication;
        private String m_ProxyHost;
        private int m_ProxyPort;
        private String m_ProxyUserName;
        private String m_ProxyPassword;
        private String m_ProxyDomain;

        public StackHashProxySettings() { ; } // Required for serialization.
        
        /// <summary>
        /// True to use a proxy server
        /// </summary>
        [DataMember]
        public bool UseProxy 
        { 
            get { return m_UserProxy; }
            set { m_UserProxy = value; }
        }


        /// <summary>
        /// True if the proxy server requires authentication
        /// </summary>
        [DataMember]
        public bool UseProxyAuthentication 
        { 
            get { return m_UseProxyAuthentication; }
            set { m_UseProxyAuthentication = value; }
        }

        /// <summary>
        /// Gets or sets the proxy host
        /// </summary>
        [DataMember]
        public String ProxyHost
        { 
            get { return m_ProxyHost; }
            set { m_ProxyHost = value; }
        }

        /// <summary>
        /// Gets or sets the proxy port
        /// </summary>
        [DataMember]
        public int ProxyPort 
        { 
            get { return m_ProxyPort; }
            set { m_ProxyPort = value; }
        }

        /// <summary>
        /// Gets or sets the proxy username
        /// </summary>
        [DataMember]
        public String ProxyUserName
        { 
            get { return m_ProxyUserName; }
            set { m_ProxyUserName = value; }
        }

        /// <summary>
        /// Gets or sets the proxy password
        /// </summary>
        [DataMember]
        public String ProxyPassword
        { 
            get { return m_ProxyPassword; }
            set { m_ProxyPassword = value; }
        }

        /// <summary>
        /// Gets or sets the proxy domain
        /// </summary>
        [DataMember]
        public String ProxyDomain
        { 
            get { return m_ProxyDomain; }
            set { m_ProxyDomain = value; }
        }

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
        public StackHashProxySettings(bool useProxy, bool useProxyAuthentication, String proxyHost, int proxyPort,
            String proxyUserName, String proxyPassword, String proxyDomain)
        {
            this.UseProxy = useProxy;
            this.UseProxyAuthentication = useProxyAuthentication;
            this.ProxyHost = proxyHost;
            this.ProxyPort = proxyPort;
            this.ProxyUserName = proxyUserName;
            this.ProxyPassword = proxyPassword;
            this.ProxyDomain = proxyDomain;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new StackHashProxySettings(this.UseProxy, this.UseProxyAuthentication, this.ProxyHost, this.ProxyPort, this.ProxyUserName, this.ProxyPassword, this.ProxyDomain);
        }

        #endregion
    }

}
