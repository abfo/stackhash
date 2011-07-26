using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using StackHash.StackHashService;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// WinQual hit / event info wrapper class
    /// </summary>
    public class DisplayEventInfo : INotifyPropertyChanged
    {
        private StackHashEventInfo _stackHashEventInfo;

        /// <summary>
        /// Gets the underlying StackHashEventInfo object
        /// </summary>
        public StackHashEventInfo StackHashEventInfo
        {
            get { return _stackHashEventInfo; }
        }

        // properties from StackHashEventInfo

        /// <summary />
        public DateTime DateCreatedLocal { get { return _stackHashEventInfo.DateCreatedLocal; } }
        /// <summary />
        public DateTime DateModifiedLocal { get { return _stackHashEventInfo.DateModifiedLocal; } }
        /// <summary />
        public DateTime HitDateLocal { get { return _stackHashEventInfo.HitDateLocal; } }
        /// <summary />
        public string Language { get { return _stackHashEventInfo.Language; } }
        /// <summary />
        public int Lcid { get { return _stackHashEventInfo.Lcid; } }
        /// <summary />
        public string Locale { get { return _stackHashEventInfo.Locale; } }
        /// <summary />
        public string OperatingSystemName { get { return _stackHashEventInfo.OperatingSystemName; } }
        /// <summary />
        public string OperatingSystemVersion { get { return _stackHashEventInfo.OperatingSystemVersion; } }
        /// <summary />
        public int TotalHits { get { return _stackHashEventInfo.TotalHits; } }

        /// <summary>
        /// WinQual hit / event info wrapper class
        /// </summary>
        /// <param name="stackHashEventInfo">StackHashEventInfo</param>
        public DisplayEventInfo(StackHashEventInfo stackHashEventInfo)
        {
            if (stackHashEventInfo == null) { throw new ArgumentNullException("stackHashEventInfo"); }

            _stackHashEventInfo = stackHashEventInfo;
        }

        /// <summary>
        /// Updates the DisplayEventInfo from a StackHashEventInfo
        /// </summary>
        /// <param name="stackHashEventInfo"></param>
        public void UpdateEventInfo(StackHashEventInfo stackHashEventInfo)
        {
            if (stackHashEventInfo == null) { throw new ArgumentNullException("stackHashEventInfo"); }
            if (!IsMatchingEventInfo(stackHashEventInfo)) { throw new InvalidOperationException("Cannot update from an event info that doesn't match this one"); }

            // save the new event info
            StackHashEventInfo oldEventInfo = _stackHashEventInfo;
            _stackHashEventInfo = stackHashEventInfo;

            // check for updates - note, only fields not included in IsMatchingEventInfo can change
            if (oldEventInfo.DateModifiedLocal != _stackHashEventInfo.DateModifiedLocal) { RaisePropertyChanged("DateModifiedLocal"); }
            if (oldEventInfo.TotalHits != _stackHashEventInfo.TotalHits) { RaisePropertyChanged("TotalHits"); }
        }

        /// <summary>
        /// Returns true if a StackHashEventInfo is a match for this DisplayEventInfo
        /// </summary>
        /// <param name="stackHashEventInfo"></param>
        /// <returns></returns>
        public bool IsMatchingEventInfo(StackHashEventInfo stackHashEventInfo)
        {
            bool matches = false;

            if (stackHashEventInfo != null)
            {
                matches = ((this.DateCreatedLocal == stackHashEventInfo.DateCreatedLocal) &&
                    (this.HitDateLocal == stackHashEventInfo.HitDateLocal) &&
                    (this.Language == stackHashEventInfo.Language) &&
                    (this.Lcid == stackHashEventInfo.Lcid) &&
                    (this.Locale == stackHashEventInfo.Locale) &&
                    (this.OperatingSystemName == stackHashEventInfo.OperatingSystemName) &&
                    (this.OperatingSystemVersion == stackHashEventInfo.OperatingSystemVersion));
            }

            return matches;
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
