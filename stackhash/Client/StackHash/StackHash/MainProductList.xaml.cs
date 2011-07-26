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
using StackHash.StackHashService;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Specialized;

namespace StackHash
{
    /// <summary>
    /// Displays the product list and associated reports
    /// </summary>
    public partial class MainProductList : UserControl
    {
        private const string ListViewProductsKey = "MainProductList.listViewProducts";
        private const string Column1Key = "MainProductList.Column1";
        private const string Column3Key = "MainProductList.Column3";

        private ListViewSorter _listViewSorter;
        private bool _productListHookupRequested;

        /// <summary>
        /// Event fired when the control requests a search
        /// </summary>
        public event EventHandler<SearchRequestEventArgs> SearchRequest;

        /// <summary>
        /// Event fired to populate events for the selected product (running a search
        /// if a search is currently defined)
        /// </summary>
        public event EventHandler SearchOrPopulateEvents;

        /// <summary>
        /// Event fired to request that disabled products are shown
        /// </summary>
        public event EventHandler ShowDisabledProducts;

        /// <summary>
        /// Displays the product list and associated reports
        /// </summary>
        public MainProductList()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                UserSettings.Settings.RestoreGridView(ListViewProductsKey, listViewProducts.View as GridView);

                if (UserSettings.Settings.HaveGridLength(Column1Key))
                {
                    Column1.Width = UserSettings.Settings.RestoreGridLength(Column1Key);
                }

                if (UserSettings.Settings.HaveGridLength(Column3Key))
                {
                    Column3.Width = UserSettings.Settings.RestoreGridLength(Column3Key);
                }

                _listViewSorter = new ListViewSorter(listViewProducts);

                _listViewSorter.AddDefaultSort("Name", ListSortDirection.Ascending);
                _listViewSorter.AddDefaultSort("Version", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("Id", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("SynchronizeEnabled", ListSortDirection.Ascending);
                _listViewSorter.AddDefaultSort("TotalEvents", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("TotalResponses", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("DateCreatedLocal", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("DateModifiedLocal", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("CabCollectionPolicy", ListSortDirection.Descending);
            }
        }

        /// <summary>
        /// Call to hook up the product list to client logic
        /// </summary>
        public void HookUpProductList()
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            if (clientLogic != null)
            {
                listViewProducts.ItemsSource = clientLogic.Products;
                _productListHookupRequested = true;
            }
        }

        /// <summary>
        /// Clears any selection
        /// </summary>
        public void ClearSelection()
        {
            listViewProducts.SelectedItem = null;
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            clientLogic.ClearSelections();
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

            UserSettings.Settings.SaveGridView(ListViewProductsKey, listViewProducts.View as GridView);
            UserSettings.Settings.SaveGridLength(Column1Key, Column1.Width);
            UserSettings.Settings.SaveGridLength(Column3Key, Column3.Width);
        }

        /// <summary>
        /// Selects a product
        /// </summary>
        /// <param name="product">DisplayProduct to select</param>
        public void SelectProduct(DisplayProduct product)
        {
            listViewProducts.SelectedItem = null;
            listViewProducts.SelectedItem = product;
        }

        private void ListEvents()
        {
            // make sure we have the right event list
            DisplayProduct product = listViewProducts.SelectedItem as DisplayProduct;
            if (product != null)
            {
                if (product.SynchronizeEnabled)
                {
                    ClientLogic clientLogic = this.DataContext as ClientLogic;
                    Debug.Assert(clientLogic != null);

                    clientLogic.PopulateEventPackages(product,
                        null,
                        clientLogic.LastEventsSort,
                        1,
                        UserSettings.Settings.EventPageSize,
                        UserSettings.Settings.GetDisplayHitThreshold(product.Id),
                        ClientLogicView.EventList,
                        PageIntention.First,
                        UserSettings.Settings.ShowEventsWithoutCabs);
                }
                else
                {
                    StackHashMessageBox.Show(Window.GetWindow(this),
                        string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.MainProductList_ProductDisabledMBMessage,
                        product.NameAndVersion),
                        Properties.Resources.MainProductList_ProductDisabledMBTitle,
                        StackHashMessageBoxType.Ok,
                        StackHashMessageBoxIcon.Information);
                }
            }
        }

        private void ResetCharts()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                worldMapControl.ResetMap();
                pieChartOs.ClearData();
                dateChartHits.ClearData();
            }
        }

        private void TryUpdateCharts()
        {
            DisplayProduct product = listViewProducts.SelectedItem as DisplayProduct;
            if (product != null)
            {
                if (product.LocaleSummary != null)
                {
                    worldMapControl.UpdateMap(product.LocaleSummary);
                }

                if (product.OsSummary != null)
                {
                    List<PieChartDataPoint> pieChartData = new List<PieChartDataPoint>(product.OsSummary.Count);
                    foreach (StackHashProductOperatingSystemSummary os in product.OsSummary)
                    {
                        pieChartData.Add(new PieChartDataPoint(os.TotalHits, os.OperatingSystemName));
                    }
                    pieChartOs.SetData(pieChartData);
                }

                if (product.HitDateSummary != null)
                {
                    Dictionary<DateTime, ulong> hitDateBreakout = new Dictionary<DateTime, ulong>();
                    foreach (StackHashProductHitDateSummary hitDate in product.HitDateSummary)
                    {
                        DateTime hitDateLocal = hitDate.HitDate.ToLocalTime().Date;

                        if (hitDateBreakout.ContainsKey(hitDateLocal))
                        {
                            hitDateBreakout[hitDateLocal] += (ulong)hitDate.TotalHits;
                        }
                        else
                        {
                            hitDateBreakout.Add(hitDateLocal, (ulong)hitDate.TotalHits);
                        }
                    }
                    dateChartHits.SetData(hitDateBreakout);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // note, data context may be null in the designer
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
                if (e.UIRequest == ClientLogicUIRequest.ProductSummaryUpdated)
                {
                    TryUpdateCharts();
                }
                else if (e.UIRequest == ClientLogicUIRequest.ProductsUpdated)
                {
                    ClientLogic clientLogic = this.DataContext as ClientLogic;

                    if (_productListHookupRequested)
                    {
                        listViewProducts.ItemsSource = clientLogic.Products;
                        _productListHookupRequested = false;
                    }

                    listViewProducts.Items.Refresh();

                    if (!clientLogic.AnyEnabledProducts)
                    {
                        // no enabled products, show disabled ones
                        RaiseShowDisabledProducts();
                    }

                    // begin invoke to free up client logic
                    this.Dispatcher.BeginInvoke(new Action(DoSelectionChanged));
                }
                else if (e.UIRequest == ClientLogicUIRequest.ProductListCleared)
                {
                    listViewProducts.ItemsSource = null;
                    listViewProducts.Items.Refresh();

                    _productListHookupRequested = true;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void listViewProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listViewProducts.SelectedItem != null)
            {
                if (ClientUtils.OriginalSourceIsListViewItem(e.OriginalSource))
                {
                    ListEvents();
                }
            }
        }

        private void linkListEvents_Click(object sender, RoutedEventArgs e)
        {
            if (listViewProducts.SelectedItem != null)
            {
                ListEvents();
            }
        }

        private void DoSelectionChanged()
        {
            ResetCharts();

            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            clientLogic.CurrentProduct = listViewProducts.SelectedItem as DisplayProduct;

            if (clientLogic.CurrentProduct != null)
            {
                if (clientLogic.NotBusy)
                {
                    if ((clientLogic.CurrentProduct.HitDateSummary == null) ||
                        (clientLogic.CurrentProduct.OsSummary == null) ||
                        (clientLogic.CurrentProduct.LocaleSummary == null))
                    {
                        clientLogic.PopulateProductSummaries(clientLogic.CurrentProduct.Id);
                    }
                    else
                    {
                        // need to ensure client logic is cycled in case we're in the process
                        // of navigating to a Uri
                        clientLogic.NoOp();

                        TryUpdateCharts();
                    }
                }
            }
        }

        private void listViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DoSelectionChanged();   
        }

        private void listViewProductsHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewSorter.SortColumn(header);
            }
        }

        private void pieChartOs_SegmentDoubleClick(object sender, SegmentDoubleClickEventArgs e)
        {
            RaiseSearchRequest(string.Format(CultureInfo.CurrentCulture,
                "os:\"{0}\"",
                e.SegmentLegend));
        }

        private void dateChartHits_SearchForDate(object sender, SearchForDateEventArgs e)
        {
            RaiseSearchRequest(string.Format(CultureInfo.CurrentCulture,
                "hdate={0}",
                e.Date.ToShortDateString()));
        }

        private void worldMapControl_CountryDoubleClick(object sender, CountryDoubleClickEventArgs e)
        {
            RaiseSearchRequest(e.Locale);
        }

        private void RaiseSearchRequest(string search)
        {
            if (SearchRequest != null)
            {
                SearchRequest(this, new SearchRequestEventArgs(search));
            }
        }

        private void RaiseShowDisabledProducts()
        {
            if (ShowDisabledProducts != null)
            {
                ShowDisabledProducts(this, EventArgs.Empty);
            }
        }

        private void RaiseSearchOrPopulateEvents()
        {
            if (SearchOrPopulateEvents != null)
            {
                SearchOrPopulateEvents(this, EventArgs.Empty);
            }
        }

        private void menuItemToggleSyncEnabled_Click(object sender, RoutedEventArgs e)
        {
            DisplayProduct productInfo = listViewProducts.SelectedItem as DisplayProduct;
            if (productInfo != null)
            {
                ClientLogic clientLogic = this.DataContext as ClientLogic;
                Debug.Assert(clientLogic != null);

                clientLogic.AdminSetProductSyncState(productInfo.Id, !productInfo.SynchronizeEnabled);
                e.Handled = true;
            }
        }

        private void menuItemOpenProductPage_Click(object sender, RoutedEventArgs e)
        {
            DisplayProduct productInfo = listViewProducts.SelectedItem as DisplayProduct;
            if (productInfo != null)
            {
                string eventListUrl = string.Format(CultureInfo.InvariantCulture,
                    Properties.Settings.Default.WinQualEventListTemplate,
                    productInfo.Id,
                    productInfo.Name,
                    productInfo.Version);

                DefaultBrowser.OpenUrlInInternetExplorer(eventListUrl);
            }
        }

        private void listViewProducts_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DisplayProduct productInfo = listViewProducts.SelectedItem as DisplayProduct;
            if (productInfo != null)
            {
                // set appropriate text
                menuItemToggleSyncEnabled.IsEnabled = true;
                menuItemToggleSyncEnabled.Header = productInfo.SynchronizeEnabled ? Properties.Resources.DisableSync : Properties.Resources.EnableSync;
                menuItemOpenProductPage.IsEnabled = true;
                menuItemSendProductToPlugin.IsEnabled = productInfo.SynchronizeEnabled;
                menuItemCopyProductUrl.IsEnabled = productInfo.SynchronizeEnabled;
                menuItemClearSelection.IsEnabled = true;
            }
            else
            {
                // no selection
                menuItemToggleSyncEnabled.IsEnabled = false;
                menuItemToggleSyncEnabled.Header = Properties.Resources.EnableSync;
                menuItemOpenProductPage.IsEnabled = false;
                menuItemSendProductToPlugin.IsEnabled = false;
                menuItemCopyProductUrl.IsEnabled = false;
                menuItemClearSelection.IsEnabled = false;
            }
        }

        private void commandBindingProperties_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = listViewProducts.SelectedItem is DisplayProduct;
            e.Handled = true;
        }

        private void commandBindingProperties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayProduct product = listViewProducts.SelectedItem as DisplayProduct;
            if (product != null)
            {
                int currentFilter = UserSettings.Settings.GetDisplayHitThreshold(product.Id);

                ClientLogic clientLogic = this.DataContext as ClientLogic;
                Debug.Assert(clientLogic != null);

                ProductProperties productProperties = new ProductProperties(product, clientLogic);
                productProperties.Owner = Window.GetWindow(this);
                productProperties.ShowDialog();

                listViewProducts.Items.Refresh();
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

        private void menuItemSendProductToPlugin_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                StackHashBugTrackerPlugIn plugin = menuItem.Tag as StackHashBugTrackerPlugIn;
                if (plugin != null)
                {
                    DisplayProduct productInfo = listViewProducts.SelectedItem as DisplayProduct;
                    if (productInfo != null)
                    {
                        clientLogic.SendProductToPlugin(productInfo, plugin.Name);
                    }
                }
            }
        }

        private void menuItemCopyProductUrl_Click(object sender, RoutedEventArgs e)
        {
            DisplayProduct productInfo = listViewProducts.SelectedItem as DisplayProduct;
            if (productInfo != null)
            {
                Clipboard.SetText(StackHashUri.CreateUriString(UserSettings.Settings.CurrentContextId, productInfo.Id));
            }
        }

        private void menuItemClearSelection_Click(object sender, RoutedEventArgs e)
        {
            ClientLogic clientLogic = this.DataContext as ClientLogic;
            Debug.Assert(clientLogic != null);

            listViewProducts.SelectedItem = null;
            clientLogic.ClearSelections();
        }
    }
}
