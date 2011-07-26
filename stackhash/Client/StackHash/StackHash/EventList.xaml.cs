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
using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// Displays a list of events
    /// </summary>
    public partial class EventList : UserControl
    {
        private const string ListViewEventsKey = "EventList.listViewEvents";
        private const string Column1Key = "EventList.Column1";
        private const string Column3Key = "EventList.Column3";

        private GridViewColumnHeader _lastSortColumn;
        private ListSortDirection _lastSortDirection;
        private SortDirectionAdorner _lastSortAdorner;
        private SortDirectionAdorner _nextSortAdorner;

        /// <summary>
        /// Event fired when the control requests a search
        /// </summary>
        public event EventHandler<SearchRequestEventArgs> SearchRequest;

        /// <summary>
        /// Displays a list of events
        /// </summary>
        public EventList()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                UserSettings.Settings.RestoreGridView(ListViewEventsKey, listViewEvents.View as GridView);

                if (UserSettings.Settings.HaveGridLength(Column1Key))
                {
                    Column1.Width = UserSettings.Settings.RestoreGridLength(Column1Key);
                }

                if (UserSettings.Settings.HaveGridLength(Column3Key))
                {
                    Column3.Width = UserSettings.Settings.RestoreGridLength(Column3Key);
                }
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
            }

            UserSettings.Settings.SaveGridView(ListViewEventsKey, listViewEvents.View as GridView);
            UserSettings.Settings.SaveGridLength(Column1Key, Column1.Width);
            UserSettings.Settings.SaveGridLength(Column3Key, Column3.Width);
        }

        private void UpdateState()
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            if (clientLogic != null)
            {
                buttonFirstPage.IsEnabled = clientLogic.CurrentEventsPage > 1;
                buttonPrevPage.IsEnabled = clientLogic.CurrentEventsPage > 1;
                buttonNextPage.IsEnabled = clientLogic.CurrentEventsPage < clientLogic.CurrentEventsMaxPage;
                buttonLastPage.IsEnabled = clientLogic.CurrentEventsPage < clientLogic.CurrentEventsMaxPage;

                if (clientLogic.CurrentEventsMaxPage <= 1)
                {
                    stackPanelPageNavigation.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPanelPageNavigation.Visibility = Visibility.Visible;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            if (clientLogic != null)
            {
                clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(clientLogic_ClientLogicUI);
            }
        }

        void clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.UIRequest == ClientLogicUIRequest.EventPackageRefreshComplete)
                {
                    listViewEvents.Items.Refresh();
                }
                else if (e.UIRequest == ClientLogicUIRequest.EventPackageListReady)
                {
                    UpdateState();

                    // if we just sorted then apply the relevant adorner
                    if ((_lastSortColumn != null) && (_nextSortAdorner != null))
                    {
                        AdornerLayer.GetAdornerLayer(_lastSortColumn).Add(_nextSortAdorner);
                        _lastSortAdorner = _nextSortAdorner;
                        _nextSortAdorner = null;
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(clientLogic_ClientLogicUI), sender, e);
            }
        }

        /// <summary>
        /// Selects an event package and optionally shows the event details
        /// </summary>
        /// <param name="eventPackage">DisplayEventPackage to select</param>
        /// <param name="showDetails">True if the event details view should be loaded</param>
        public void SelectAndShowEventDetails(DisplayEventPackage eventPackage, bool showDetails)
        {
            listViewEvents.SelectedItem = eventPackage;
            if (showDetails)
            {
                ShowSelectedEventDetails();
            }
        }

        private void ShowSelectedEventDetails()
        {
            // set current event package and switch views
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            if (eventPackage != null)
            {
                clientLogic.CurrentEventPackage = eventPackage;
                clientLogic.GetEventNotes();
                clientLogic.CurrentView = ClientLogicView.EventDetail;
            }
        }

        private void linkMappedProducts_Click(object sender, RoutedEventArgs e)
        {
            // switch views
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);
            clientLogic.CurrentView = ClientLogicView.ProductList;
        }

        private void linkShowDetails_Click(object sender, RoutedEventArgs e)
        {
            ShowSelectedEventDetails();
        }

        private void listViewEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientUtils.OriginalSourceIsListViewItem(e.OriginalSource))
            {
                ShowSelectedEventDetails();
            }
        }

        private void listViewEventsHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                // can only sort columns with an associated tag
                string tagName = header.Tag as string;
                if (!string.IsNullOrEmpty(tagName))
                {
                    // remove previous sort adorner
                    if ((_lastSortColumn != null) && (_lastSortAdorner != null))
                    {
                        AdornerLayer.GetAdornerLayer(_lastSortColumn).Remove(_lastSortAdorner);
                    }
                    _lastSortAdorner = null;

                    ListSortDirection direction = ListSortDirection.Descending;

                    if (header == _lastSortColumn)
                    {
                        if (_lastSortDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    // to be applied after the event packages are fetched
                    _nextSortAdorner = new SortDirectionAdorner(header, direction);

                    _lastSortDirection = direction;
                    _lastSortColumn = header;

                    StackHashSortOrder sortOrder = new StackHashSortOrder();
                    sortOrder.Ascending = (direction == ListSortDirection.Ascending);
                    sortOrder.FieldName = tagName;

                    switch (tagName)
                    {
                        case "Id":
                        case "TotalHits":
                        case "CabCount":
                        case "BugId":
                        case "PlugInBugId":
                        case "EventTypeName":
                        case "DateCreatedLocal":
                        case "DateModifiedLocal":
                        case "WorkFlowStatusName":
                            // these fields are from the event
                            sortOrder.ObjectType = StackHashObjectType.Event;
                            break;
                          
                        default:
                            // all other tags are from the event signature
                            sortOrder.ObjectType = StackHashObjectType.EventSignature;
                            break;
                    }

                    StackHashSortOrderCollection sort = new StackHashSortOrderCollection();
                    sort.Add(sortOrder);

                    ClientLogic clientLogic = this.DataContext as ClientLogic;
                    Debug.Assert(clientLogic != null);

                    // run the sort, returning to the first page
                    clientLogic.PopulateEventPackages(clientLogic.CurrentProduct,
                        clientLogic.LastEventsSearch,
                        sort,
                        1,
                        UserSettings.Settings.EventPageSize,
                        clientLogic.CurrentProduct == null ? 
                        UserSettings.Settings.GetDisplayHitThreshold(UserSettings.InvalidContextId) : 
                        UserSettings.Settings.GetDisplayHitThreshold(clientLogic.CurrentProduct.Id),
                        ClientLogicView.EventList,
                        PageIntention.First,
                        UserSettings.Settings.ShowEventsWithoutCabs);
                }
            }
        }

        private void listViewEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            // update charts
            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            if (eventPackage != null)
            {
                clientLogic.CurrentEventPackage = eventPackage;

                // !!! optimize this and other charts by calculating in ClientLogic (or even in the service)
                Dictionary<string, ulong> operatingSystemBreakout = new Dictionary<string, ulong>();
                Dictionary<string, ulong> languageBreakout = new Dictionary<string, ulong>();
                Dictionary<DateTime, ulong> hitDateBreakout = new Dictionary<DateTime, ulong>();

                foreach (DisplayEventInfo eventInfo in eventPackage.EventInfoList)
                {
                    if (operatingSystemBreakout.ContainsKey(eventInfo.OperatingSystemName))
                    {
                        operatingSystemBreakout[eventInfo.OperatingSystemName] += (ulong)eventInfo.TotalHits;
                    }
                    else
                    {
                        operatingSystemBreakout.Add(eventInfo.OperatingSystemName, (ulong)eventInfo.TotalHits);
                    }

                    if (languageBreakout.ContainsKey(eventInfo.Language))
                    {
                        languageBreakout[eventInfo.Language] += (ulong)eventInfo.TotalHits;
                    }
                    else
                    {
                        languageBreakout.Add(eventInfo.Language, (ulong)eventInfo.TotalHits);
                    }

                    // make sure we key on just year/month/day
                    DateTime hitDate = new DateTime(eventInfo.HitDateLocal.Year,
                        eventInfo.HitDateLocal.Month,
                        eventInfo.HitDateLocal.Day);
                    if (hitDateBreakout.ContainsKey(hitDate))
                    {
                        hitDateBreakout[hitDate] += (ulong)eventInfo.TotalHits;
                    }
                    else
                    {
                        hitDateBreakout.Add(hitDate, (ulong)eventInfo.TotalHits);
                    }
                }

                List<PieChartDataPoint> pieChartOSData = new List<PieChartDataPoint>(operatingSystemBreakout.Count);
                foreach (string key in operatingSystemBreakout.Keys)
                {
                    pieChartOSData.Add(new PieChartDataPoint(operatingSystemBreakout[key],
                        key));
                }

                pieChartOs.SetData(pieChartOSData);

                List<PieChartDataPoint> pieChartLanguageData = new List<PieChartDataPoint>(operatingSystemBreakout.Count);
                foreach (string key in languageBreakout.Keys)
                {
                    pieChartLanguageData.Add(new PieChartDataPoint(languageBreakout[key],
                        key));
                }

                pieChartLang.SetData(pieChartLanguageData);

                dateChartHits.SetData(hitDateBreakout);
            }
            else
            {
                clientLogic.CurrentEventPackage = null;

                pieChartLang.ClearData();
                pieChartOs.ClearData();
                dateChartHits.ClearData();
            }
        }

        private void listViewEvents_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            if (eventPackage != null)
            {
                menuitemOpenEventPage.IsEnabled = true;
                menuItemSendEventToPlugin.IsEnabled = ((clientLogic.ActivePlugins != null) && (clientLogic.ActivePlugins.Count > 0) && (!clientLogic.PluginHasError));
                menuItemCopyEventUrl.IsEnabled = true;
            }
            else
            {
                menuitemOpenEventPage.IsEnabled = false;
                menuItemSendEventToPlugin.IsEnabled = false;
                menuItemCopyEventUrl.IsEnabled = false;
            }
        }

        private void dateChartHits_SearchForDate(object sender, SearchForDateEventArgs e)
        {
            RaiseSearchRequest(string.Format(CultureInfo.CurrentCulture,
                "hdate={0}",
                e.Date.ToShortDateString()));
        }

        private void pieChartOs_SegmentDoubleClick(object sender, SegmentDoubleClickEventArgs e)
        {
            RaiseSearchRequest(string.Format(CultureInfo.CurrentCulture,
                "os:\"{0}\"",
                e.SegmentLegend));
        }

        private void pieChartLang_SegmentDoubleClick(object sender, SegmentDoubleClickEventArgs e)
        {
            RaiseSearchRequest(string.Format(CultureInfo.CurrentCulture,
                "language:\"{0}\"",
                e.SegmentLegend));
        }

        private void RaiseSearchRequest(string search)
        {
            if (SearchRequest != null)
            {
                SearchRequest(this, new SearchRequestEventArgs(search));
            }
        }

        private void menuitemOpenEventPage_Click(object sender, RoutedEventArgs e)
        {
            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            if (eventPackage != null)
            {
                string eventDetailsUrl = string.Format(CultureInfo.InvariantCulture,
                    Properties.Settings.Default.WinQualEventDetailsTemplate,
                    eventPackage.Id,
                    eventPackage.EventTypeName);

                DefaultBrowser.OpenUrlInInternetExplorer(eventDetailsUrl);
            }
        }

        private void menuItemSendEventToPlugin_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                StackHashBugTrackerPlugIn plugin = menuItem.Tag as StackHashBugTrackerPlugIn;
                if (plugin != null)
                {
                   DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
                   if (eventPackage != null)
                   {
                       clientLogic.SendEventToPlugin(eventPackage, plugin.Name);
                   }
                }
            }
        }

        private void SetEventBugIdCore(TextBox textBox)
        {
            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            ClientLogic clientLogic = this.DataContext as ClientLogic;

            if ((textBox != null) && (eventPackage != null) && (clientLogic != null))
            {
                if (textBox.Text != eventPackage.BugId)
                {
                    eventPackage.BugId = textBox.Text;
                    clientLogic.SetEventBugId(eventPackage, textBox.Text);
                }
            }
        }

        private void textBoxReference_LostFocus(object sender, RoutedEventArgs e)
        {
            SetEventBugIdCore(e.OriginalSource as TextBox);
        }

        private void textBoxReference_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                if ((e.Key == Key.Return) || (e.Key == Key.Enter))
                {
                    SetEventBugIdCore(textBox);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
                    if ((eventPackage != null) && (eventPackage.BugId != null))
                    {
                        textBox.Text = eventPackage.BugId;
                        e.Handled = true;
                    }
                }
            }
        }

        private void commandBindingProperties_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = listViewEvents.SelectedItem is DisplayEventPackage;
            e.Handled = true;
        }

        private void commandBindingProperties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            if (eventPackage != null)
            {
                ClientLogic clientLogic = this.DataContext as ClientLogic;
                Debug.Assert(clientLogic != null);

                EventProperties eventProperties = new EventProperties(eventPackage, clientLogic);
                eventProperties.Owner = Window.GetWindow(this);
                eventProperties.ShowDialog();

                listViewEvents.Items.Refresh();
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

        private void buttonFirstPage_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            clientLogic.PopulateEventPackages(clientLogic.CurrentProduct,
                clientLogic.LastEventsSearch,
                clientLogic.LastEventsSort,
                1,
                UserSettings.Settings.EventPageSize,
                UserSettings.Settings.GetDisplayHitThreshold(clientLogic.CurrentProduct == null ? -1 : clientLogic.CurrentProduct.Id),
                ClientLogicView.EventList,
                PageIntention.First,
                UserSettings.Settings.ShowEventsWithoutCabs);
        }

        private void buttonPrevPage_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            int targetPage = clientLogic.CurrentEventsPage - 1;
            if (targetPage < 1)
            {
                targetPage = 1;
            }

            clientLogic.PopulateEventPackages(clientLogic.CurrentProduct,
                clientLogic.LastEventsSearch,
                clientLogic.LastEventsSort,
                targetPage,
                UserSettings.Settings.EventPageSize,
                UserSettings.Settings.GetDisplayHitThreshold(clientLogic.CurrentProduct == null ? -1 : clientLogic.CurrentProduct.Id),
                ClientLogicView.EventList,
                PageIntention.Previous,
                UserSettings.Settings.ShowEventsWithoutCabs);
        }

        private void buttonNextPage_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            int targetPage = clientLogic.CurrentEventsPage + 1;
            if (targetPage > clientLogic.CurrentEventsMaxPage)
            {
                targetPage = clientLogic.CurrentEventsMaxPage;
            }

            clientLogic.PopulateEventPackages(clientLogic.CurrentProduct,
                clientLogic.LastEventsSearch,
                clientLogic.LastEventsSort,
                targetPage,
                UserSettings.Settings.EventPageSize,
                UserSettings.Settings.GetDisplayHitThreshold(clientLogic.CurrentProduct == null ? -1 : clientLogic.CurrentProduct.Id),
                ClientLogicView.EventList,
                PageIntention.Next,
                UserSettings.Settings.ShowEventsWithoutCabs);
        }

        private void buttonLastPage_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            clientLogic.PopulateEventPackages(clientLogic.CurrentProduct,
                clientLogic.LastEventsSearch,
                clientLogic.LastEventsSort,
                clientLogic.CurrentEventsMaxPage,
                UserSettings.Settings.EventPageSize,
                UserSettings.Settings.GetDisplayHitThreshold(clientLogic.CurrentProduct == null ? -1 : clientLogic.CurrentProduct.Id),
                ClientLogicView.EventList,
                PageIntention.Last,
                UserSettings.Settings.ShowEventsWithoutCabs);
        }

        private void textBoxReference_Loaded(object sender, RoutedEventArgs e)
        {
            // focus to the text box if the mouse was directly over it
            TextBox textBoxReference = sender as TextBox;
            if (textBoxReference != null)
            {
                Point p = Mouse.GetPosition(textBoxReference);
                if ((p.X >= 0) && (p.X <= textBoxReference.ActualWidth)
                    && (p.Y >= 0) && (p.Y <= textBoxReference.ActualHeight))
                {
                    Keyboard.Focus(textBoxReference);
                }
            }
        }

        private void comboBoxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            ComboBox comboBox = e.OriginalSource as ComboBox;
            ClientLogic clientLogic = this.DataContext as ClientLogic;

            if ((comboBox != null) && (eventPackage != null) && (clientLogic != null))
            {
                DisplayMapping workFlowMapping = comboBox.SelectedItem as DisplayMapping;
                if (workFlowMapping != null)
                {
                    if (workFlowMapping.Id != eventPackage.WorkFlowStatus)
                    {
                        eventPackage.WorkFlowStatus = workFlowMapping.Id;
                        clientLogic.SetEventWorkFlow(eventPackage, workFlowMapping.Id);
                    }
                }
            }
        }

        private void menuItemCopyEventUrl_Click(object sender, RoutedEventArgs e)
        {
            DisplayEventPackage eventPackage = listViewEvents.SelectedItem as DisplayEventPackage;
            if (eventPackage != null)
            {
                Clipboard.SetText(StackHashUri.CreateUriString(UserSettings.Settings.CurrentContextId,
                    eventPackage.ProductId,
                    eventPackage.Id,
                    eventPackage.EventTypeName));
            }
        }
    }
}
