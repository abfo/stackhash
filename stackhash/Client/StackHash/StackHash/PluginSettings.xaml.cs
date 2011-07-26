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
using StackHash.StackHashService;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Configure settings for a plugin
    /// </summary>
    public partial class PluginSettings : Window
    {
        private const string WindowKey = "PluginSettings";

        private string _helpUrl;

        /// <summary>
        /// Configure settings for a plugin
        /// </summary>
        /// <param name="name">Name of the plugin</param>
        /// <param name="settings">Current settings for the plugin</param>
        /// <param name="helpUrl">URL to get help on configuring this plugin</param>
        public PluginSettings(string name, StackHashNameValueCollection settings, string helpUrl)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (settings == null) { throw new ArgumentNullException("settings"); }

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            if (!string.IsNullOrEmpty(helpUrl))
            {
                _helpUrl = helpUrl;
                buttonPluginHelp.ToolTip = _helpUrl;
            }
            else
            {
                buttonPluginHelp.IsEnabled = false;
            }

            itemsControlSettings.ItemsSource = settings;
            this.Title = string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.PluginSettings_Title,
                name);
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_helpUrl))
            {
                DefaultBrowser.OpenUrl(_helpUrl);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
