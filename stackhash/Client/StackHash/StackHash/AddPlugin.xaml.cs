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
using System.Collections.ObjectModel;

namespace StackHash
{
    /// <summary>
    /// Allows the user to pick a plugin to add to a context
    /// </summary>
    public partial class AddPlugin : Window
    {
        private const string WindowKey = "AddPlugin";

        /// <summary>
        /// Gets the selected plugin (after the window has closed successfully)
        /// </summary>
        public StackHashBugTrackerPlugInDiagnostics SelectedPlugin { get; set; }

        /// <summary>
        /// Gets the list of available plugins
        /// </summary>
        public ObservableCollection<StackHashBugTrackerPlugInDiagnostics> AvailablePlugins { get; private set; }

        /// <summary>
        /// Allows the user to pick a plugin to add to a context
        /// </summary>
        /// <param name="availablePlugins">Current list of available plugins</param>
        public AddPlugin(ObservableCollection<StackHashBugTrackerPlugInDiagnostics> availablePlugins)
        {
            if (availablePlugins == null) { throw new ArgumentNullException("availablePlugins"); }
            this.AvailablePlugins = availablePlugins;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            this.DataContext = this;
            UpdateState();
        }

        private void UpdateState()
        {
            StackHashBugTrackerPlugInDiagnostics plugin = comboBoxPlugin.SelectedItem as StackHashBugTrackerPlugInDiagnostics;
            if (plugin != null)
            {
                buttonOK.IsEnabled = true;
                hyperlinkPluginHelp.IsEnabled = plugin.HelpUrl != null;
            }
            else
            {
                buttonOK.IsEnabled = false;
                hyperlinkPluginHelp.IsEnabled = false;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void comboBoxPlugin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateState();
        }

        private void hyperlinkPluginHelp_Click(object sender, RoutedEventArgs e)
        {
            StackHashBugTrackerPlugInDiagnostics plugin = comboBoxPlugin.SelectedItem as StackHashBugTrackerPlugInDiagnostics;
            if ((plugin != null) && (plugin.HelpUrl != null))
            {
                DefaultBrowser.OpenUrl(plugin.HelpUrl.ToString());
            }
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("add-plugin.htm");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
