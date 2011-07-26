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
    /// Select one or more plugins to synchronize with profile data
    /// </summary>
    public partial class SelectPlugins : Window
    {
        private const string WindowKey = "SelectPlugins";
        private const string ListViewPluginsKey = "SelectPlugins.listViewPlugins";

        /// <summary>
        /// Gets the names of the selected plugins (may be null or empty)
        /// </summary>
        public string[] SelectedPlugins { get; private set; }

        private class PluginSelection
        {
            public bool Selected { get; set; }
            public string Name { get; set; }

            public PluginSelection(string name)
            {
                this.Name = name;
            }

            public static List<PluginSelection> FromActivePlugins(ObservableCollection<StackHashBugTrackerPlugIn> activePlugins)
            {
                List<PluginSelection> selections = new List<PluginSelection>();

                foreach (StackHashBugTrackerPlugIn plugin in activePlugins)
                {
                    selections.Add(new PluginSelection(plugin.Name));
                }

                return selections;
            }
        }

        private List<PluginSelection> _selections;
        private ListViewSorter _listViewSorter;

        /// <summary>
        /// Select one or more plugins to synchronize with profile data
        /// </summary>
        /// <param name="activePlugins">List of active plugins</param>
        public SelectPlugins(ObservableCollection<StackHashBugTrackerPlugIn> activePlugins)
        {
            if (activePlugins == null) { throw new ArgumentNullException("activePlugins"); }
            _selections = PluginSelection.FromActivePlugins(activePlugins);

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
            UserSettings.Settings.RestoreGridView(ListViewPluginsKey, listViewPlugins.View as GridView);

            _listViewSorter = new ListViewSorter(listViewPlugins);
            _listViewSorter.AddDefaultSort("Name", System.ComponentModel.ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("Selected", System.ComponentModel.ListSortDirection.Ascending);

            listViewPlugins.ItemsSource = _selections;
            UpdateState();
        }

        private void UpdateState()
        {
            bool anySelected = false;

            foreach (PluginSelection selection in _selections)
            {
                if (selection.Selected)
                {
                    anySelected = true;
                    break;
                }
            }

            buttonOK.IsEnabled = anySelected;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("select-plugins.htm");
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            List<string> selectedNames = new List<string>();

            foreach (PluginSelection selection in _selections)
            {
                if (selection.Selected)
                {
                    selectedNames.Add(selection.Name);
                }
            }

            if (selectedNames.Count > 0)
            {
                this.SelectedPlugins = selectedNames.ToArray();

                DialogResult = true;
                Close();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void listViewPlugins_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewSorter.SortColumn(header);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveGridView(ListViewPluginsKey, listViewPlugins.View as GridView);
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
