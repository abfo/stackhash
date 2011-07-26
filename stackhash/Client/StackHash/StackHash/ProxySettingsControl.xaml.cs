using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StackHash
{
    /// <summary>
    /// Configures proxy server settings
    /// </summary>
    public partial class ProxySettingsControl : UserControl
    {
        private ProxySettings _proxySettings;

        /// <summary>
        /// Configures proxy server settings
        /// </summary>
        public ProxySettingsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the ProxySettings for the control
        /// </summary>
        public ProxySettings ProxySettings
        {
            get { return _proxySettings; }
            set
            {
                if (value == null) { throw new ArgumentNullException("value"); }

                _proxySettings = value;
                this.DataContext = _proxySettings;
                passwordBoxPassword.Password = _proxySettings.ProxyPassword;

                UpdateState();
            }
        }

        /// <summary>
        /// True if the control is currently in a valid state
        /// </summary>
        public bool IsValid
        {
            get
            {
                _proxySettings.ValidationEnabled = true;
                return BindingValidator.IsValid(this);
            }
        }

        private void UpdateState()
        {
            if (checkBoxUseProxy.IsChecked == true)
            {
                labelHost.IsEnabled = true;
                textBoxHost.IsEnabled = true;
                labelPort.IsEnabled = true;
                textBoxPort.IsEnabled = true;
                checkBoxUseProxyAuthentication.IsEnabled = true;

                if (checkBoxUseProxyAuthentication.IsChecked == true)
                {
                    labelUsername.IsEnabled = true;
                    textBoxUsername.IsEnabled = true;
                    labelPassword.IsEnabled = true;
                    passwordBoxPassword.IsEnabled = true;
                    labelDomain.IsEnabled = true;
                    textBoxDomain.IsEnabled = true;
                }
                else
                {
                    labelUsername.IsEnabled = false;
                    textBoxUsername.IsEnabled = false;
                    labelPassword.IsEnabled = false;
                    passwordBoxPassword.IsEnabled = false;
                    labelDomain.IsEnabled = false;
                    textBoxDomain.IsEnabled = false;
                }
            }
            else
            {
                labelHost.IsEnabled = false;
                textBoxHost.IsEnabled = false;
                labelPort.IsEnabled = false;
                textBoxPort.IsEnabled = false;
                checkBoxUseProxyAuthentication.IsEnabled = false;
                labelUsername.IsEnabled = false;
                textBoxUsername.IsEnabled = false;
                labelPassword.IsEnabled = false;
                passwordBoxPassword.IsEnabled = false;
                labelDomain.IsEnabled = false;
                textBoxDomain.IsEnabled = false;
            }
        }

        private void checkBoxUseProxy_Checked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxUseProxy_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxUseProxyAuthentication_Checked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void checkBoxUseProxyAuthentication_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void passwordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _proxySettings.ProxyPassword = passwordBoxPassword.Password;
        }
    }
}
