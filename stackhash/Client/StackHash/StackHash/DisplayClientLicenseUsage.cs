using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHash.StackHashService;
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// ClientLicenseUsage Wrapper
    /// </summary>
    public class DisplayClientLicenseUsage : INotifyPropertyChanged
    {
        private StackHashClientLicenseUsage _stackHashClientLicenseUsage;

        /// <summary>
        /// Gets the underlying StackHashClientLicenseUsage object
        /// </summary>
        public StackHashClientLicenseUsage StackHashClientLicenseUsage
        {
            get { return _stackHashClientLicenseUsage; }
        }

        // properties from StackHashClientLicenseUsage

        /// <summary />
        public DateTime ClientConnectTime { get { return _stackHashClientLicenseUsage.ClientConnectTime; } }
        /// <summary />
        public DateTime LastAccessTime { get { return _stackHashClientLicenseUsage.LastAccessTime; } }

        // properties from StackHashClientData

        /// <summary />
        public string ClientName { get { return _stackHashClientLicenseUsage.ClientData.ClientName; } }
        /// <summary />
        public Guid ApplicationGuid { get { return _stackHashClientLicenseUsage.ClientData.ApplicationGuid; } }

        /// <summary>
        /// ClientLicenseUsage Wrapper
        /// </summary>
        /// <param name="clientLicenseUsage">StackHashClientLicenseUsage</param>
        public DisplayClientLicenseUsage(StackHashClientLicenseUsage clientLicenseUsage)
        {
            if (clientLicenseUsage == null) { throw new ArgumentNullException("clientLicenseUsage"); }
            if (clientLicenseUsage.ClientData == null) {throw new ArgumentNullException("clientLicenseUsage", "clientLicenseUsage.ClientData is null");}

            _stackHashClientLicenseUsage = clientLicenseUsage;
        }

        /// <summary>
        /// Updates the DisplayClientLicenseUsage from a StackHashClientLicenseUsage
        /// </summary>
        /// <param name="clientLicenseUsage">StackHashClientLicenseUsage</param>
        public void UpdateClientLicenseUsage(StackHashClientLicenseUsage clientLicenseUsage)
        {
            if (clientLicenseUsage == null) { throw new ArgumentNullException("clientLicenseUsage"); }
            if (clientLicenseUsage.ClientData == null) { throw new ArgumentNullException("clientLicenseUsage", "clientLicenseUsage.ClientData is null"); }
            if (clientLicenseUsage.ClientData.ApplicationGuid != _stackHashClientLicenseUsage.ClientData.ApplicationGuid) { throw new InvalidOperationException("Cannot update with a different ApplicationGuid"); }

            StackHashClientLicenseUsage oldClientLicenseUsage = _stackHashClientLicenseUsage;
            _stackHashClientLicenseUsage = clientLicenseUsage;

            if (oldClientLicenseUsage.ClientConnectTime != _stackHashClientLicenseUsage.ClientConnectTime) { RaisePropertyChanged("ClientConnectTime"); }
            if (oldClientLicenseUsage.LastAccessTime != _stackHashClientLicenseUsage.LastAccessTime) { RaisePropertyChanged("LastAccessTime"); }
            if (oldClientLicenseUsage.ClientData.ClientName != _stackHashClientLicenseUsage.ClientData.ClientName) { RaisePropertyChanged("ClientName"); }
        }

        #region INotifyPropertyChanged Members

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary />
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
