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
using System.ComponentModel;
using StackHash.StackHashService;

namespace StackHash
{
    /// <summary>
    /// Displays diagnostic information for plugins
    /// </summary>
    public partial class PluginDiagnostics : Window
    {
        private const string WindowKey = "PluginDiagnostics";
        private const string ListViewAllPluginsKey = "PluginDiagnostics.listViewAllPlugins";
        private const string ListViewGlobalDiagnosticsKey = "PluginDiagnostics.listViewGlobalDiagnostics";
        private const string ListViewProfileDiagnosticsKey = "PluginDiagnostics.listViewProfileDiagnostics";

        private ListViewSorter _allPluginsSorter;
        private ListViewSorter _globalDiagnosticsSorter;
        private ListViewSorter _profileDiagnosticsSorter;
        private ClientLogic _clientLogic;
        private int _contextId;
        private bool _contextIsActive;

        /// <summary>
        /// Displays diagnostic information for plugins
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        /// <param name="contextId">Context Id (may be invalid)</param>
        /// <param name="contextIsActive">True if the context is currently active</param>
        public PluginDiagnostics(ClientLogic clientLogic, int contextId, bool contextIsActive)
        {
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }
            _clientLogic = clientLogic;
            _contextId = contextId;
            _contextIsActive = contextIsActive;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            UserSettings.Settings.RestoreGridView(ListViewAllPluginsKey, listViewAllPlugins.View as GridView);
            UserSettings.Settings.RestoreGridView(ListViewGlobalDiagnosticsKey, listViewGlobalDiagnostics.View as GridView);
            UserSettings.Settings.RestoreGridView(ListViewProfileDiagnosticsKey, listViewProfileDiagnostics.View as GridView);

            this.DataContext = _clientLogic;

            _allPluginsSorter = new ListViewSorter(listViewAllPlugins);
            _allPluginsSorter.AddDefaultSort("Name", System.ComponentModel.ListSortDirection.Ascending);
            _allPluginsSorter.AddDefaultSort("FileName", System.ComponentModel.ListSortDirection.Ascending);
            _allPluginsSorter.AddDefaultSort("Loaded", System.ComponentModel.ListSortDirection.Ascending);
            _allPluginsSorter.AddDefaultSort("LastException", System.ComponentModel.ListSortDirection.Ascending);

            _globalDiagnosticsSorter = new ListViewSorter(listViewGlobalDiagnostics);
            _globalDiagnosticsSorter.AddDefaultSort("Name", System.ComponentModel.ListSortDirection.Ascending);
            _globalDiagnosticsSorter.AddDefaultSort("Value", System.ComponentModel.ListSortDirection.Ascending);

            _profileDiagnosticsSorter = new ListViewSorter(listViewProfileDiagnostics);
            _profileDiagnosticsSorter.AddDefaultSort("Name", System.ComponentModel.ListSortDirection.Ascending);
            _profileDiagnosticsSorter.AddDefaultSort("Value", System.ComponentModel.ListSortDirection.Ascending);

            // no profile diagnostics if no profile specified
            if ((contextId == UserSettings.InvalidContextId) || (!contextIsActive))
            {
                textBlockProfileDiagnostics.IsEnabled = false;
                listViewProfileDiagnostics.IsEnabled = false;
            }
        }

        private void UpdateGridState()
        {
            gridMain.IsEnabled = _clientLogic.NotBusy;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("plugin-diagnostics.htm");
        }

        private void listViewAllPlugins_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _allPluginsSorter.SortColumn(header);
            }
        }

        private void listViewAllPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // clear current diagnostics
            _clientLogic.CurrentPluginDiagnostics = null;

            StackHashBugTrackerPlugInDiagnostics diagnostics = listViewAllPlugins.SelectedItem as StackHashBugTrackerPlugInDiagnostics;
            if ((diagnostics != null) && (_contextId != UserSettings.InvalidContextId) && _contextIsActive)
            {
                _clientLogic.AdminGetPluginDiagnostics(_contextId, diagnostics.Name);
            }
        }

        private void listViewGlobalDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _globalDiagnosticsSorter.SortColumn(header);
            }
        }

        private void listViewProfileDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _profileDiagnosticsSorter.SortColumn(header);
            }
        }

        void _clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "NotBusy")
                {
                    UpdateGridState();
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // clear current diagnostics
            _clientLogic.CurrentPluginDiagnostics = null;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                UpdateGridState();
                _clientLogic.PropertyChanged += new PropertyChangedEventHandler(_clientLogic_PropertyChanged);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // clear current diagnostics
            _clientLogic.CurrentPluginDiagnostics = null;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;

                UserSettings.Settings.SaveGridView(ListViewAllPluginsKey, listViewAllPlugins.View as GridView);
                UserSettings.Settings.SaveGridView(ListViewGlobalDiagnosticsKey, listViewGlobalDiagnostics.View as GridView);
                UserSettings.Settings.SaveGridView(ListViewProfileDiagnosticsKey, listViewProfileDiagnostics.View as GridView);
                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }
    }
}
