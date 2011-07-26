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
using System.Windows.Shapes;

namespace StackHash
{
    /// <summary>
    /// Set local proxy server settings (from UserSettings)
    /// </summary>
    public partial class ProxySettingsWindow : Window
    {
        private const string WindowKey = "ProxySettingsWindow";

        /// <summary>
        /// Set local proxy server settings (from UserSettings)
        /// </summary>
        public ProxySettingsWindow()
        {
            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProxySettings proxySettings = new ProxySettings(UserSettings.Settings.UseProxyServer,
                UserSettings.Settings.UseProxyServerAuthentication,
                UserSettings.Settings.ProxyHost,
                UserSettings.Settings.ProxyPort,
                UserSettings.Settings.ProxyUsername,
                UserSettings.Settings.ProxyPassword,
                UserSettings.Settings.ProxyDomain);

            proxySettingsControl.ProxySettings = proxySettings;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (proxySettingsControl.IsValid)
            {
                UserSettings.Settings.UseProxyServer = proxySettingsControl.ProxySettings.UseProxy;
                UserSettings.Settings.UseProxyServerAuthentication = proxySettingsControl.ProxySettings.UseProxyAuthentication;
                UserSettings.Settings.ProxyHost = proxySettingsControl.ProxySettings.ProxyHost;
                UserSettings.Settings.ProxyPort = proxySettingsControl.ProxySettings.ProxyPort;
                UserSettings.Settings.ProxyUsername = proxySettingsControl.ProxySettings.ProxyUsername;
                UserSettings.Settings.ProxyPassword = proxySettingsControl.ProxySettings.ProxyPassword;
                UserSettings.Settings.ProxyDomain = proxySettingsControl.ProxySettings.ProxyDomain;

                this.DialogResult = true;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("proxy-server.htm");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
