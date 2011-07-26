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
using System.Diagnostics;
using StackHash.StackHashService;
using StackHashUtilities;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Specialized;

namespace StackHash
{
    /// <summary>
    /// Window for adding, editing and removing scripts
    /// </summary>
    public partial class ScriptManager : Window
    {
        private const string WindowKey = "ScriptManager";
        private const string ListViewScriptsKey = "ScriptManager.listViewScripts";

        private enum ActionOnScriptReady
        {
            None,
            Edit,
            Delete
        }

        private ClientLogic _clientLogic;
        private ActionOnScriptReady _actionOnScriptReady;
        private bool _windowClosed;
        private ListViewSorter _listViewSorter;
        private bool _sortingScripts;
        private string _reselectScriptName;

        /// <summary>
        /// Window for adding, editing and removing scripts
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        public ScriptManager(ClientLogic clientLogic)
        {
            Debug.Assert(clientLogic != null);
            _clientLogic = clientLogic;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
            UserSettings.Settings.RestoreGridView(ListViewScriptsKey, listViewScripts.View as GridView);

            _listViewSorter = new ListViewSorter(listViewScripts);
            _listViewSorter.AddDefaultSort("Name", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("IsReadOnly", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("RunAutomatically", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("DumpType", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("LastModifiedDate", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("CreationDate", ListSortDirection.Ascending);

            this.DataContext = _clientLogic;
            UpdateState();
        }

        /// <summary>
        /// Detects if a search contains a script search element
        /// </summary>
        /// <param name="search">StackHashSearchCriteriaCollection</param>
        /// <returns>True if a script search is included</returns>
        public static bool SearchContainsScriptSearch(StackHashSearchCriteriaCollection search)
        {
            bool script = false;

            if (search != null)
            {
                foreach (StackHashSearchCriteria criteria in search)
                {
                    foreach (StackHashSearchOption option in criteria.SearchFieldOptions)
                    {
                        if (option.ObjectType == StackHashObjectType.Script)
                        {
                            script = true;
                            break;
                        }
                    }
                }
            }

            return script;
        }

        void ListViewScripts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_sortingScripts)
            {
                _sortingScripts = true;

                if ((_clientLogic.ScriptData != null) && (_reselectScriptName != null))
                {
                    foreach (StackHashScriptFileData script in _clientLogic.ScriptData)
                    {
                        if (script.Name == _reselectScriptName)
                        {
                            listViewScripts.SelectedItem = script;
                            break;
                        }
                    }
                }

                _listViewSorter.SortLastColumn();

                _sortingScripts = false;
            }
        }

        private void UpdateState()
        {
            StackHashScriptFileData currentScriptData = listViewScripts.SelectedItem as StackHashScriptFileData;
            if (currentScriptData == null)
            {
                buttonTest.IsEnabled = false;
                buttonEdit.IsEnabled = false;
                buttonDelete.IsEnabled = false;

                menuItemTest.IsEnabled = false;
                menuItemEdit.IsEnabled = false;
                menuItemDelete.IsEnabled = false;
            }
            else
            {
                buttonTest.IsEnabled = _clientLogic.CurrentCab != null;
                buttonEdit.IsEnabled = true;

                menuItemTest.IsEnabled = _clientLogic.CurrentCab != null;
                menuItemEdit.IsEnabled = true;

                if (currentScriptData.IsReadOnly)
                {
                    buttonEdit.Content = Properties.Resources.ButtonText_View;
                    buttonDelete.IsEnabled = false;

                    menuItemEdit.Header = Properties.Resources.MenuText_ViewScript;
                    menuItemDelete.IsEnabled = false;
                }
                else
                {
                    buttonEdit.Content = Properties.Resources.ButtonText_Edit;
                    buttonDelete.IsEnabled = true;

                    menuItemEdit.Header = Properties.Resources.MenuText_EditScript;
                    menuItemDelete.IsEnabled = true;
                }
            }
        }

        private void DoEdit()
        {
            Debug.Assert(_clientLogic.CurrentScript != null);

            // used to rename if changed
            string originalName = _clientLogic.CurrentScript.Name;

            ScriptAddEdit scriptEdit = new ScriptAddEdit(_clientLogic, _clientLogic.CurrentScript, false);
            scriptEdit.Owner = this;

            if ((scriptEdit.ShowDialog() == true) && (!_clientLogic.CurrentScript.IsReadOnly))
            {
                _clientLogic.AdminAddScript(_clientLogic.CurrentScript, originalName, true);
            }

            _actionOnScriptReady = ActionOnScriptReady.None;
        }

        private void DoDelete()
        {
            Debug.Assert(_clientLogic.CurrentScript != null);

            if (_clientLogic.CurrentScript != null)
            {
                if (StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ScriptManager_DeleteScriptMBMessage,
                    _clientLogic.CurrentScript.Name),
                    Properties.Resources.ScriptManager_DeleteScriptMBTitle,
                    StackHashMessageBoxType.YesNo,
                    StackHashMessageBoxIcon.Question) == StackHashDialogResult.Yes)
                {
                    _clientLogic.AdminRemoveScript(_clientLogic.CurrentScript.Name);
                    _actionOnScriptReady = ActionOnScriptReady.None;
                }
            }
        }

        private void ShowTestScriptResults()
        {
            Debug.Assert(_clientLogic.CurrentScriptResult != null);

            if (_clientLogic.CurrentScriptResult != null)
            {
                ScriptResultViewer scriptResultViewer = new ScriptResultViewer();
                scriptResultViewer.DataContext = _clientLogic.CurrentScriptResult;
                scriptResultViewer.Owner = this;

                scriptResultViewer.ShowDialog();
            }
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (_windowClosed)
                {
                    return;
                }

                if (e.UIRequest == ClientLogicUIRequest.ScriptReady)
                {
                    switch (_actionOnScriptReady)
                    {
                        case ActionOnScriptReady.Edit:
                            DoEdit();
                            break;

                        case ActionOnScriptReady.Delete:
                            DoDelete();
                            break;
                    }
                }
                else if (e.UIRequest == ClientLogicUIRequest.ScriptResultsReady)
                {
                    ShowTestScriptResults();
                }

                UpdateState();
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void DoAdd()
        {
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings();
            scriptSettings.Script = new StackHashScript();
            scriptSettings.IsReadOnly = false;
            scriptSettings.RunAutomatically = false;
            scriptSettings.Version = 1;
            scriptSettings.Owner = StackHashScriptOwner.User;

            ScriptAddEdit scriptAdd = new ScriptAddEdit(_clientLogic, scriptSettings, true);
            scriptAdd.Owner = this;

            if (scriptAdd.ShowDialog() == true)
            {
                _reselectScriptName = scriptSettings.Name;
                _clientLogic.AdminAddScript(scriptSettings, scriptSettings.Name, false);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            DoAdd();
        }

        private void menuItemAdd_Click(object sender, RoutedEventArgs e)
        {
            DoAdd();
        }

        private void DoStartEdit()
        {
            StackHashScriptFileData currentScriptData = listViewScripts.SelectedItem as StackHashScriptFileData;

            if (currentScriptData != null)
            {
                if ((_clientLogic.CurrentScript == null) ||
                    (string.Compare(currentScriptData.Name, _clientLogic.CurrentScript.Name, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    // need to load the script first
                    _actionOnScriptReady = ActionOnScriptReady.Edit;
                    _clientLogic.AdminGetScript(currentScriptData.Name);
                }
                else
                {
                    DoEdit();
                }
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            DoStartEdit();
        }

        private void menuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            DoStartEdit();
        }

        private void DoStartDelete()
        {
            StackHashScriptFileData currentScriptData = listViewScripts.SelectedItem as StackHashScriptFileData;

            if (currentScriptData != null)
            {
                if ((_clientLogic.CurrentScript == null) ||
                    (string.Compare(currentScriptData.Name, _clientLogic.CurrentScript.Name, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    // need to load the script first
                    _actionOnScriptReady = ActionOnScriptReady.Delete;
                    _clientLogic.AdminGetScript(currentScriptData.Name);
                }
                else
                {
                    DoDelete();
                }
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            DoStartDelete();
        }

        private void menuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            DoStartDelete();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
                ((INotifyCollectionChanged)listViewScripts.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(ListViewScripts_CollectionChanged);

                // refresh script names
                _clientLogic.AdminGetScriptNames();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _windowClosed = true;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
                ((INotifyCollectionChanged)listViewScripts.Items).CollectionChanged -= ListViewScripts_CollectionChanged;

                UserSettings.Settings.SaveGridView(ListViewScriptsKey, listViewScripts.View as GridView);
                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }

        private void DoTest()
        {
            StackHashScriptFileData currentScriptData = listViewScripts.SelectedItem as StackHashScriptFileData;

            if (currentScriptData != null)
            {
                _clientLogic.AdminTestScript(currentScriptData.Name);
            }
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            DoTest();
        }

        private void menuItemTest_Click(object sender, RoutedEventArgs e)
        {
            DoTest();
        }

        private void listViewScripts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientUtils.OriginalSourceIsListViewItem(e.OriginalSource))
            {
                DoStartEdit();
            }
        }

        private void listViewScripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StackHashScriptFileData currentScriptData = listViewScripts.SelectedItem as StackHashScriptFileData;
            if (currentScriptData != null)
            {
                _reselectScriptName = currentScriptData.Name;
            }

            UpdateState();
        }

        private void listViewScriptsHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _sortingScripts = true;
                _listViewSorter.SortColumn(header);
                _sortingScripts = false;
            }
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("script-manager.htm");
        }        
    }
}
