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
    /// Wrapper class for context settings and status
    /// </summary>
    public class DisplayContext : INotifyPropertyChanged
    {
        private StackHashContextSettings _stackHashContextSettings;
        private StackHashContextStatus _stackHashContextStatus;

        /// <summary>
        /// Gets the underlying StackHashContextSettings object
        /// </summary>
        public StackHashContextSettings StackHashContextSettings
        {
            get { return _stackHashContextSettings; }
        }
        
        /// <summary>
        /// Gets the underlying StackHashContextStatus object
        /// </summary>
        public StackHashContextStatus StackHashContextStatus
        {
            get { return _stackHashContextStatus; }
        }

        // properties from StackHashContextSettings

        /// <summary />
        public int Id { get { return _stackHashContextSettings.Id; } }

        /// <summary />
        public bool IsActive { get { return _stackHashContextSettings.IsActive; } }

        /// <summary />
        public string ProfileName { get { return _stackHashContextSettings.WinQualSettings.CompanyName; } }

        /// <summary />
        public string IdText
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ProfileIdText,
                    _stackHashContextSettings.WinQualSettings.CompanyName,
                    _stackHashContextSettings.Id);
            }
        }

        // properties from StackHashContextStatus

        /// <summary />
        public StackHashServiceErrorCode CurrentError 
        { 
            get 
            {
                if (_stackHashContextStatus == null)
                {
                    return StackHashServiceErrorCode.NoError;
                }
                else
                {
                    return _stackHashContextStatus.CurrentError;
                }
            } 
        }

        /// <summary>
        /// Gets a localized version of the current error
        /// </summary>
        public string CurrentErrorText
        {
            get
            {
                if (_stackHashContextStatus == null)
                {
                    return Properties.Resources.Unknown;
                }
                else if (_stackHashContextStatus.CurrentError == StackHashServiceErrorCode.NoError)
                {
                    return Properties.Resources.Ok;
                }
                else
                {
                    return StackHashMessageBox.GetServiceErrorCodeMessage(_stackHashContextStatus.CurrentError);
                }
            }
        }

        /// <summary />
        public string LastContextException 
        { 
            get 
            {
                if (_stackHashContextStatus == null)
                {
                    return null;
                }
                else
                {
                    return _stackHashContextStatus.LastContextException;
                }
            } 
        }

        /// <summary>
        /// Wrapper class for context settings and status
        /// </summary>
        /// <param name="stackHashContextSettings">Context settings</param>
        /// <param name="stackHashContextStatus">Context status (optional)</param>
        public DisplayContext(StackHashContextSettings stackHashContextSettings, StackHashContextStatus stackHashContextStatus)
        {
            if (stackHashContextSettings == null) { throw new ArgumentNullException("stackHashContextSettings"); }
            if (stackHashContextSettings.WinQualSettings == null) { throw new ArgumentNullException("stackHashContextSettings", "stackHashContextSettings.WinQualSettings is null"); }

            // ok for stackHashContextStatus to be null
            _stackHashContextSettings = stackHashContextSettings;
            _stackHashContextStatus = stackHashContextStatus;
        }

        /// <summary>
        /// Updates the settings portion of the display context
        /// </summary>
        /// <param name="stackHashContextSettings">Context settings</param>
        public void UpdateSettings(StackHashContextSettings stackHashContextSettings)
        {
            if (stackHashContextSettings == null) { throw new ArgumentNullException("stackHashContextSettings"); }
            if (stackHashContextSettings.WinQualSettings == null) { throw new ArgumentNullException("stackHashContextSettings", "stackHashContextSettings.WinQualSettings is null"); }
            if (_stackHashContextSettings.Id != stackHashContextSettings.Id) { throw new InvalidOperationException("Cannot update from a context with a different ID"); }

            // save the new settings
            StackHashContextSettings oldSettings = _stackHashContextSettings;
            _stackHashContextSettings = stackHashContextSettings;

            // check for updates
            if (oldSettings.IsActive != _stackHashContextSettings.IsActive) { RaisePropertyChanged("IsActive"); }
            if (oldSettings.WinQualSettings.CompanyName != _stackHashContextSettings.WinQualSettings.CompanyName) { RaisePropertyChanged("ProfileName"); }
        }

        /// <summary>
        /// Updates the status portion of the display context
        /// </summary>
        /// <param name="stackHashContextStatus">Context status</param>
        public void UpdateStatus(StackHashContextStatus stackHashContextStatus)
        {
            if (stackHashContextStatus == null) { throw new ArgumentNullException("stackHashContextStatus"); }
            if (_stackHashContextSettings.Id != stackHashContextStatus.ContextId) { throw new InvalidOperationException("Cannot update from a context with a different ID"); }

            if (_stackHashContextStatus == null)
            {
                // first update (the status isn't necessarily provided in the constructor)
                _stackHashContextStatus = stackHashContextStatus;
                RaisePropertyChanged("CurrentError");
                RaisePropertyChanged("CurrentErrorText");
                RaisePropertyChanged("LastContextException");
            }
            else
            {
                // normal update

                // save the new settings
                StackHashContextStatus oldStatus = _stackHashContextStatus;
                _stackHashContextStatus = stackHashContextStatus;

                if (oldStatus.CurrentError != _stackHashContextStatus.CurrentError) { RaisePropertyChanged("CurrentError"); RaisePropertyChanged("CurrentErrorText"); }
                if (oldStatus.LastContextException != _stackHashContextStatus.LastContextException) { RaisePropertyChanged("LastContextException"); }
            }
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
