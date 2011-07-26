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
using System.Globalization;
using StackHash.StackHashService;

namespace StackHash
{
    /// <summary>
    /// Welcome page for the setup wizard
    /// </summary>
    public partial class SetupWelcomePage : SetupBasePage
    {
        private string _host;
        private int _port;

        /// <summary>
        /// Welcome page for the setup wizard
        /// </summary>
        public SetupWelcomePage()
        {
            _port = -1;

            InitializeComponent();
            this.NextEnabled = true;
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's now active
        /// </summary>
        public override void PageActivated()
        {
            textBoxHost.Text = UserSettings.Settings.ServiceHost;
            textBoxPort.Text = UserSettings.Settings.ServicePort.ToString(CultureInfo.CurrentCulture);
            UpdateState();

            this.ClientLogic.ClientLogicSetupWizardPrompt += new EventHandler<ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt);
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's no longer active
        /// </summary>
        public override void PageDeactivated()
        {
            this.ClientLogic.ClientLogicSetupWizardPrompt -= ClientLogic_ClientLogicSetupWizardPrompt;
        }

        /// <summary>
        /// Request navigation to the next page - ShowNextPage will fire if this is successfull
        /// </summary>
        public override void TryNext()
        {
            UpdateState();
            if (this.NextEnabled)
            {
                ServiceProxy.Services.UpdateServiceEndpointAndAccount(_host, 
                    _port,
                    UserSettings.Settings.ServiceUsername,
                    UserSettings.Settings.ServicePassword,
                    UserSettings.Settings.ServiceDomain);

                // try to connect to the service
                this.ClientLogic.AdminServiceConnect();
            }
        }

        private void UpdatateServiceProxySettings()
        {
            StackHashProxySettings proxySettings = new StackHashProxySettings();
            proxySettings.UseProxy = UserSettings.Settings.UseProxyServer;
            proxySettings.UseProxyAuthentication = UserSettings.Settings.UseProxyServerAuthentication;
            proxySettings.ProxyHost = UserSettings.Settings.ProxyHost;
            proxySettings.ProxyPort = UserSettings.Settings.ProxyPort;
            proxySettings.ProxyUserName = UserSettings.Settings.ProxyUsername;
            proxySettings.ProxyPassword = UserSettings.Settings.ProxyPassword;
            proxySettings.ProxyDomain = UserSettings.Settings.ProxyDomain;
            this.ClientLogic.ServiceProxySettings = proxySettings;

            this.ClientLogic.AdminUpdateServiceProxySettingsAndClientTimeout();
        }

        private void ClientLogic_ClientLogicSetupWizardPrompt(object sender, ClientLogicSetupWizardPromptEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                switch (e.Prompt)
                {
                    case ClientLogicSetupWizardPromptOperation.AdminServiceConnect:
                        if (e.Succeeded)
                        {
                            // save service proxy settings
                            UpdatateServiceProxySettings();
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;

                            StackHashMessageBox.Show(Window.GetWindow(this),
                                Properties.Resources.SetupWizard_ServiceConnectFailedMBMessage,
                                Properties.Resources.SetupWizard_ServiceConnectFailedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error,
                                e.LastException,
                                e.LastServiceError);
                        }
                        break;

                    case ClientLogicSetupWizardPromptOperation.ProxySettingsUpdated:
                        if (e.Succeeded)
                        {
                            UserSettings.Settings.ServiceHost = _host;
                            UserSettings.Settings.ServicePort = _port;

                            // go to the next page
                            if (((this.ClientLogic.ContextCollection != null) && (this.ClientLogic.ContextCollection.Count > 0)) ||
                                (!this.ClientLogic.ServiceIsLocal))
                            {
                                // profiles exist or we're not on the service computer so the wizard should just select a profile
                                DoRaiseConfigureForProfileOnly();
                                DoRaiseShowNextPage();
                            }
                            else
                            {
                                // we're on the service computer and there are no profiles so continue with normal setup
                                DoRaiseConfigureForInitialSetup();
                                DoRaiseShowNextPage();
                            }
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;

                            StackHashMessageBox.Show(Window.GetWindow(this),
                                Properties.Resources.SetupWizard_ProxyUpdateFailedMBMessage,
                                Properties.Resources.SetupWizard_ProxyUpdateFailedMBTitle,
                                StackHashMessageBoxType.Ok,
                                StackHashMessageBoxIcon.Error,
                                e.LastException,
                                e.LastServiceError);
                        }
                        break;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt),
                    sender, e);
            }
        }

        private void UpdateState()
        {
            this.NextEnabled = ((!string.IsNullOrEmpty(_host)) &&
                (_port >= 0) &&
                (_port <= 65535));
        }

        private void textBoxHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            _host = textBoxHost.Text;

            UpdateState();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void textBoxPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (textBoxPort.Text.Length > 0)
                {
                    _port = Convert.ToInt32(textBoxPort.Text, CultureInfo.CurrentCulture);
                }
                else
                {
                    _port = -1;
                }
            }
            catch 
            {
                _port = -1;
            }

            UpdateState();
        }

        private void hyperlinkProxySettings_Click(object sender, RoutedEventArgs e)
        {
            ProxySettingsWindow proxySettingsWindow = new ProxySettingsWindow();
            proxySettingsWindow.Owner = Window.GetWindow(this);
            proxySettingsWindow.ShowDialog();
        }

        private void hyperlinkGettingStarted_Click(object sender, RoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("how-to-getting-started.htm");
        }

        private void hyperlinkCredentials_Click(object sender, RoutedEventArgs e)
        {
            ServiceCredentials serviceCredentials = new ServiceCredentials();
            serviceCredentials.Owner = Window.GetWindow(this);
            serviceCredentials.Username = UserSettings.Settings.ServiceUsername;
            serviceCredentials.Password = UserSettings.Settings.ServicePassword;
            serviceCredentials.Domain = UserSettings.Settings.ServiceDomain;

            if (serviceCredentials.ShowDialog() == true)
            {
                UserSettings.Settings.ServiceUsername = serviceCredentials.Username;
                UserSettings.Settings.ServicePassword = serviceCredentials.Password;
                UserSettings.Settings.ServiceDomain = serviceCredentials.Domain;
            }
        }        
    }
}
