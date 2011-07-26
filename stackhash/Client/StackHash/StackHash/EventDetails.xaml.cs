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
using System.Diagnostics;
using StackHash.StackHashService;
using Microsoft.Win32;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Specialized;

namespace StackHash
{
    /// <summary>
    /// Shows details for a StackHashEventPackage
    /// </summary>
    public partial class EventDetails : UserControl
    {
        private const string DummyMenuItemHeader = "Dummy";
        private const string ListViewEventInfosKey = "EventDetails.listViewEventInfos";
        private const string ListViewCabsKey = "EventDetails.listViewCabs";
        private const string Row3Key = "EventDetails.Row3";
        private const string Row5Key = "EventDetails.Row5";
        private const string Column1Key = "EventDetails.Column1";
        private const string Column3Key = "EventDetails.Column3";

        private ListViewSorter _listViewEventInfosSorter;
        private ListViewSorter _listViewCabsSorter;

        /// <summary>
        /// Shows details for a StackHashEventPackage
        /// </summary>
        public EventDetails()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                UserSettings.Settings.RestoreGridView(ListViewEventInfosKey, listViewEventInfos.View as GridView);
                UserSettings.Settings.RestoreGridView(ListViewCabsKey, listViewCabs.View as GridView);

                if (UserSettings.Settings.HaveGridLength(Column1Key))
                {
                    Column1.Width = UserSettings.Settings.RestoreGridLength(Column1Key);
                }

                if (UserSettings.Settings.HaveGridLength(Column3Key))
                {
                    Column3.Width = UserSettings.Settings.RestoreGridLength(Column3Key);
                }

                if (UserSettings.Settings.HaveGridLength(Row3Key))
                {
                    Row3.Height = UserSettings.Settings.RestoreGridLength(Row3Key);
                }

                if (UserSettings.Settings.HaveGridLength(Row5Key))
                {
                    Row5.Height = UserSettings.Settings.RestoreGridLength(Row5Key);
                }

                _listViewEventInfosSorter = new ListViewSorter(listViewEventInfos);
                _listViewCabsSorter = new ListViewSorter(listViewCabs);

                _listViewEventInfosSorter.AddDefaultSort("HitDateLocal", ListSortDirection.Descending);
                _listViewEventInfosSorter.AddDefaultSort("Language", ListSortDirection.Ascending);
                _listViewEventInfosSorter.AddDefaultSort("Locale", ListSortDirection.Ascending);
                _listViewEventInfosSorter.AddDefaultSort("Lcid", ListSortDirection.Ascending);
                _listViewEventInfosSorter.AddDefaultSort("OperatingSystemName", ListSortDirection.Ascending);
                _listViewEventInfosSorter.AddDefaultSort("OperatingSystemVersion", ListSortDirection.Ascending);
                _listViewEventInfosSorter.AddDefaultSort("TotalHits", ListSortDirection.Descending);
                _listViewEventInfosSorter.AddDefaultSort("DateCreatedLocal", ListSortDirection.Descending);
                _listViewEventInfosSorter.AddDefaultSort("DateModifiedLocal", ListSortDirection.Descending);

                _listViewCabsSorter.AddDefaultSort("DateCreatedLocal", ListSortDirection.Descending);
                _listViewCabsSorter.AddDefaultSort("CabDownloaded", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("OSVersion", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("MachineArchitecture", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("DotNetVersion", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("ProcessUpTime", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("SystemUpTime", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("EventTypeName", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("SizeInBytes", ListSortDirection.Ascending);
                _listViewCabsSorter.AddDefaultSort("Id", ListSortDirection.Descending);
                _listViewCabsSorter.AddDefaultSort("DateModifiedLocal", ListSortDirection.Descending);
                _listViewCabsSorter.AddDefaultSort("Purged", ListSortDirection.Ascending);
            }
        }

        /// <summary>
        /// Called when the main StackHash window is closed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StackHashMainWindowClosed()
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            if (clientLogic != null)
            {
                clientLogic.ClientLogicUI -= clientLogic_ClientLogicUI;
                clientLogic.PropertyChanged -= clientLogic_PropertyChanged;
            }

            UserSettings.Settings.SaveGridView(ListViewEventInfosKey, listViewEventInfos.View as GridView);
            UserSettings.Settings.SaveGridView(ListViewCabsKey, listViewCabs.View as GridView);
            UserSettings.Settings.SaveGridLength(Column1Key, Column1.Width);
            UserSettings.Settings.SaveGridLength(Column3Key, Column3.Width);
            UserSettings.Settings.SaveGridLength(Row3Key, Row3.Height);
            UserSettings.Settings.SaveGridLength(Row5Key, Row5.Height);
        }

        /// <summary>
        /// Selects a cab
        /// </summary>
        /// <param name="cab">DisplayCab to select</param>
        public void SelectCab(DisplayCab cab)
        {
            listViewCabs.SelectedItem = cab;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // note - won't have a DataContext in the designer
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            if (clientLogic != null)
            {
                clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(clientLogic_ClientLogicUI);
                clientLogic.PropertyChanged += new PropertyChangedEventHandler(clientLogic_PropertyChanged);
            }

            notesControl.SaveNote += new EventHandler<SaveNoteEventArgs>(notesControl_SaveNote);
        }

        void clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                ClientLogic clientLogic = this.DataContext as ClientLogic;

                if ((e.PropertyName == "CurrentView") && (clientLogic.CurrentView == ClientLogicView.EventDetail))
                {
                    _listViewCabsSorter.SortLastColumn();
                    _listViewEventInfosSorter.SortLastColumn();

                    DisplayCab selectedCab = listViewCabs.SelectedItem as DisplayCab;
                    if (selectedCab != null)
                    {
                        clientLogic.CurrentCab = selectedCab;
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(clientLogic_PropertyChanged), sender, e);
            }
        }

        void notesControl_SaveNote(object sender, SaveNoteEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            clientLogic.AddEventNote(e.Note, e.NoteId);
        }

        void clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                ClientLogic clientLogic = this.DataContext as ClientLogic;
                Debug.Assert(clientLogic != null);

                switch (e.UIRequest)
                {
                    case ClientLogicUIRequest.EventPackageRefreshComplete:
                        listViewCabs.Items.Refresh();
                        listViewEventInfos.Items.Refresh();
                        clientLogic.GetEventNotes();
                        break;

                    case ClientLogicUIRequest.EventNotesReady:
                        // if we have some notes display them
                        if (clientLogic.CurrentEventNotes != null)
                        {
                            notesControl.UpdateNotes(clientLogic.CurrentEventNotes);
                        }
                        else
                        {
                            notesControl.ClearNotes();
                        }
                        break;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void linkMappedProducts_Click(object sender, RoutedEventArgs e)
        {
            // switch views
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);
            clientLogic.CurrentView = ClientLogicView.ProductList;
        }

        private void linkCurrentProduct_Click(object sender, RoutedEventArgs e)
        {
            // switch views
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);
            clientLogic.CurrentView = ClientLogicView.EventList;
        }

        private void listViewEventInfosHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewEventInfosSorter.SortColumn(header);
            }
        }

        private void listViewCabsHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewCabsSorter.SortColumn(header);
            }
        }

        private void listViewCabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            DisplayCab selectedCab = listViewCabs.SelectedItem as DisplayCab;
            if (selectedCab != null)
            {
                clientLogic.CurrentCab = selectedCab;
            }            
        }

        private void LoadCabDetails()
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            DisplayCab selectedCab = listViewCabs.SelectedItem as DisplayCab;
            if (selectedCab != null)
            {
                clientLogic.CurrentCab = selectedCab;
                clientLogic.AdminGetResultFiles();
                clientLogic.CurrentView = ClientLogicView.CabDetail;
            }
        }

        private void linkShowDetails_Click(object sender, RoutedEventArgs e)
        {
            LoadCabDetails();
        }

        private void listViewCabs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientUtils.OriginalSourceIsListViewItem(e.OriginalSource))
            {
                LoadCabDetails();
            }
        }

        private void MenuItem_IsEnabledChangedDimIcon(object sender, DependencyPropertyChangedEventArgs e)
        {
            // dim icon (if present) if disabled... can't seem to do this through a style in App.xaml like
            // with the buttons
            MenuItem menu = sender as MenuItem;
            if (menu != null)
            {
                Image menuImage = menu.Icon as Image;
                if (menuImage != null)
                {
                    if (menu.IsEnabled)
                    {
                        menuImage.Opacity = 1.0;
                    }
                    else
                    {
                        menuImage.Opacity = 0.5;
                    }
                }
            }
        }

        private void menuItemSendCabToPlugin_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                StackHashBugTrackerPlugIn plugin = menuItem.Tag as StackHashBugTrackerPlugIn;
                if (plugin != null)
                {
                    DisplayCab selectedCab = listViewCabs.SelectedItem as DisplayCab;
                    if ((selectedCab != null) && (clientLogic.CurrentEventPackage != null))
                    {
                        clientLogic.SendCabToPlugin(selectedCab, clientLogic.CurrentEventPackage, plugin.Name);
                    }
                }
            }
        }

        private void menuItemCopyCabUrl_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            DisplayCab selectedCab = listViewCabs.SelectedItem as DisplayCab;
            if ((selectedCab != null) && (clientLogic.CurrentEventPackage != null))
            {
                Clipboard.SetText(StackHashUri.CreateUriString(UserSettings.Settings.CurrentContextId,
                    clientLogic.CurrentEventPackage.ProductId,
                    clientLogic.CurrentEventPackage.Id,
                    clientLogic.CurrentEventPackage.EventTypeName,
                    selectedCab.Id));
            }
        }

        private void listViewCabs_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DisplayCab selectedCab = listViewCabs.SelectedItem as DisplayCab;
            if (selectedCab != null)
            {
                menuItemSendCabToPlugin.IsEnabled = true;
                menuItemCopyCabUrl.IsEnabled = true;
            }
            else
            {
                menuItemSendCabToPlugin.IsEnabled = false;
                menuItemCopyCabUrl.IsEnabled = false;
            }
        }
    }
}
