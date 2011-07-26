using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace StackHashDBConfig
{
    /// <summary>
    /// Represents an instance of a database
    /// </summary>
    public class DatabaseInstance
    {
        /// <summary>
        /// Gets the instance name of this database
        /// </summary>
        public string InstanceName { get; private set; }
        
        /// <summary>
        /// Gets the service name of this instance
        /// </summary>
        public string ServiceName { get; private set; }
        
        /// <summary>
        /// Gets the edition name of this instance
        /// </summary>
        public string EditionName { get; private set; }
        
        /// <summary>
        /// Gets the version name of this instance
        /// </summary>
        public string VersionName { get; private set; }
        
        /// <summary>
        /// Gets the major version of this instance
        /// </summary>
        public int MajorVersion { get; private set; }
        
        /// <summary>
        /// Gets the minor version of this instance
        /// </summary>
        public int MinorVersion { get; private set; }

        /// <summary>
        /// Represents an instance of a database
        /// </summary>
        /// <param name="instance">Instance name</param>
        /// <param name="service">Service name</param>
        /// <param name="edition">Edition name</param>
        /// <param name="version">Version name</param>
        public DatabaseInstance(string instance, string service, string edition, string version)
        {
            if (instance == null) { throw new ArgumentNullException("instance"); }
            if (service == null) { throw new ArgumentNullException("service"); }
            if (edition == null) { throw new ArgumentNullException("edition"); }
            if (version == null) { throw new ArgumentNullException("version"); }

            this.InstanceName = instance;
            this.ServiceName = service;
            this.EditionName = edition;
            this.VersionName = version;

            // crack out the major and minor version
            string[] versionBits = version.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (versionBits.Length >= 2)
            {
                this.MajorVersion = Convert.ToInt32(versionBits[0], CultureInfo.InvariantCulture);
                this.MinorVersion = Convert.ToInt32(versionBits[1], CultureInfo.InvariantCulture);
            }
        }
    }
}
