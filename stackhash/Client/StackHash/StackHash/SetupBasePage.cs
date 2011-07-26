using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls;
using System;

namespace StackHash
{
    /// <summary>
    /// Event args indicating the dialog result for closing the wizard
    /// </summary>
    public class ExitWizardEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the dialog result
        /// </summary>
        public bool DialogResult { get; private set; }

        /// <summary>
        /// Event args indicating the dialog result for closing the wizard
        /// </summary>
        /// <param name="dialogResult">The dialog result</param>
        public ExitWizardEventArgs(bool dialogResult)
        {
            this.DialogResult = dialogResult;
        }
    }

    /// <summary>
    /// Event args to force the wizard to be enabled or disabled
    /// </summary>
    public class DisableWizardEventArgs : EventArgs
    {
        /// <summary>
        /// True if the wizard should be disabled
        /// </summary>
        public bool ForceDisable { get; private set; }

        /// <summary>
        /// Event args to force the wizard to be enabled or disabled
        /// </summary>
        /// <param name="forceDisable">True if the wizard should be disabled</param>
        public DisableWizardEventArgs(bool forceDisable)
        {
            this.ForceDisable = forceDisable;
        }
    }

    /// <summary>
    /// Base UserControl for a wizard page
    /// </summary>
    public class SetupBasePage : UserControl, INotifyPropertyChanged
    {
        private ClientLogic _clientLogic;
        private bool _backEnabled;
        private bool _nextEnabled;
        private string _nextText;
        private bool _disableWizard;
        private string _helpTopic;

        /// <summary>
        /// Event fired to ask the wizard to show the next page
        /// </summary>
        public event EventHandler ShowNextPage;

        /// <summary>
        /// Event fired to ask the wizard to show the previous page
        /// </summary>
        public event EventHandler ShowPreviousPage;

        /// <summary>
        /// Event fired to ask the wizard to configure itself to only
        /// select a profile (used if profiles exist or the wizard is run
        /// on a different machine to the service)
        /// </summary>
        public event EventHandler ConfigureForProfileOnly;

        /// <summary>
        /// Event fired to ask the wizard to configure itself for normal 
        /// setup (running on the same machine as the service and no
        /// exiting profile to select);
        /// </summary>
        public event EventHandler ConfigureForInitialSetup;

        /// <summary>
        /// Event fired to ask the wizard to close
        /// </summary>
        public event EventHandler<ExitWizardEventArgs> ExitWizard;

        /// <summary>
        /// Event fired to enable to disable the wizard
        /// </summary>
        public event EventHandler<DisableWizardEventArgs> DisableEnableWizard;

        /// <summary>
        /// Base UserControl for a wizard page
        /// </summary>
        public SetupBasePage()
        {
            // default is Next
            this.NextText = Properties.Resources.ButtonText_Next;
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's now active
        /// </summary>
        public virtual void PageActivated()
        {

        }

        /// <summary>
        /// Called by the wizard to notify the page that it's no longer active
        /// </summary>
        public virtual void PageDeactivated()
        {

        }

        /// <summary>
        /// Request navigation to the next page - ShowNextPage will fire if this is successfull
        /// </summary>
        public virtual void TryNext()
        {
            
        }

        /// <summary>
        /// Request navigation to the previous page - ShowPreviousPage will fire if this is successfull
        /// </summary>
        public virtual void TryBack()
        {
            
        }

        /// <summary>
        /// True if the wizard should be disabled
        /// </summary>
        public bool DisableWizard
        {
            get { return _disableWizard; }
            set 
            { 
                _disableWizard = value;

                if (DisableEnableWizard != null)
                {
                    DisableEnableWizard(this, new DisableWizardEventArgs(_disableWizard));
                }
            }
        }

        /// <summary>
        /// True if the wizard back button should be enabled
        /// </summary>
        public bool BackEnabled
        {
            get { return _backEnabled; }
            set 
            {
                if (_backEnabled != value)
                {
                    _backEnabled = value;
                    DoRaisePropertyChanged("BackEnabled");
                }
            }
        }

        /// <summary>
        /// True if the wizard next button should be enabled
        /// </summary>
        public bool NextEnabled
        {
            get { return _nextEnabled; }
            set 
            {
                if (_nextEnabled != value)
                {
                    _nextEnabled = value;
                    DoRaisePropertyChanged("NextEnabled");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text to display on the next button
        /// </summary>
        public string NextText
        {
            get { return _nextText; }
            set 
            {
                if (_nextText != value)
                {
                    _nextText = value;
                    DoRaisePropertyChanged("NextText");
                }
            }
        }

        /// <summary>
        /// Gets the current ClientLogic object
        /// </summary>
        public ClientLogic ClientLogic
        {
            get { return _clientLogic; }
            set { _clientLogic = value; }
        }

        /// <summary>
        /// Gets or sets the HelpTopic for the page
        /// </summary>
        public string HelpTopic
        {
            get { return _helpTopic; }
            set { _helpTopic = value; }
        }

        /// <summary>
        /// Raises the ShowNextPage event
        /// </summary>
        protected void DoRaiseShowNextPage()
        {
            if (ShowNextPage != null)
            {
                ShowNextPage(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the ShowPreviousPage event
        /// </summary>
        protected void DoRaiseShowPreviousPage()
        {
            if (ShowPreviousPage != null)
            {
                ShowPreviousPage(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the ConfigureForProfileOnly event
        /// </summary>
        protected void DoRaiseConfigureForProfileOnly()
        {
            if (ConfigureForProfileOnly != null)
            {
                ConfigureForProfileOnly(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the ConfigureForInitialSetup event
        /// </summary>
        protected void DoRaiseConfigureForInitialSetup()
        {
            if (ConfigureForInitialSetup != null)
            {
                ConfigureForInitialSetup(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the ExitWizard event
        /// </summary>
        /// <param name="dialogResult">Dialog result</param>
        protected void DoRaiseExitWizard(bool dialogResult)
        {
            if (ExitWizard != null)
            {
                ExitWizard(this, new ExitWizardEventArgs(dialogResult));
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Changed property</param>
        protected void DoRaisePropertyChanged(string propertyName)
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
