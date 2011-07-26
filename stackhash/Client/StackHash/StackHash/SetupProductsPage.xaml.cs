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
using System.ComponentModel;
using System.Collections.Specialized;
using StackHash.StackHashService;

namespace StackHash
{
    /// <summary>
    /// Products page for the setup wizard
    /// </summary>
    public partial class SetupProductsPage : SetupBasePage
    {
        private ListViewSorter _listViewSorter;

        /// <summary>
        /// Products page for the setup wizard
        /// </summary>
        public SetupProductsPage()
        {
            InitializeComponent();

            this.NextEnabled = true;
            this.BackEnabled = true;
            this.NextText = Properties.Resources.ButtonText_Next;

            _listViewSorter = new ListViewSorter(listViewProducts);

            _listViewSorter.AddDefaultSort("Name", ListSortDirection.Ascending);
            _listViewSorter.AddDefaultSort("Version", ListSortDirection.Descending);
            _listViewSorter.AddDefaultSort("Id", ListSortDirection.Descending);
            _listViewSorter.AddDefaultSort("SynchronizeEnabled", ListSortDirection.Ascending);
        }

        /// <summary>
        /// Called by the wizard to notify the page that it's now active
        /// </summary>
        public override void PageActivated()
        {
            listViewProducts.ItemsSource = this.ClientLogic.Products;

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
            // Can always go next
            DoRaiseShowNextPage();
        }

        /// <summary>
        /// Request navigation to the previous page - ShowPreviousPage will fire if this is successfull
        /// </summary>
        public override void TryBack()
        {
            // can always go back
            DoRaiseShowPreviousPage();
        }

        private void ClientLogic_ClientLogicSetupWizardPrompt(object sender, ClientLogicSetupWizardPromptEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.Prompt == ClientLogicSetupWizardPromptOperation.ProductListUpdated)
                {
                    listViewProducts.ItemsSource = this.ClientLogic.Products;

                    if (!e.Succeeded)
                    {
                        Mouse.OverrideCursor = null;

                        StackHashMessageBox.Show(Window.GetWindow(this),
                            Properties.Resources.SetupWizard_UpdateProductListFailedMBMessage,
                            Properties.Resources.SetupWizard_UpdateProductListFailedMBTitle,
                            StackHashMessageBoxType.Ok,
                            StackHashMessageBoxIcon.Error,
                            e.LastException,
                            e.LastServiceError);
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicSetupWizardPromptEventArgs>(ClientLogic_ClientLogicSetupWizardPrompt),
                    sender, e);
            }
        }

        private void listViewProductsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewSorter.SortColumn(header);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBoxSource = e.OriginalSource as CheckBox;
            if (checkBoxSource != null)
            {
                DisplayProduct product = checkBoxSource.DataContext as DisplayProduct;
                if (product != null)
                {
                    this.ClientLogic.AdminSetProductSyncState(product.Id, true);
                    e.Handled = true;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBoxSource = e.OriginalSource as CheckBox;
            if (checkBoxSource != null)
            {
                DisplayProduct product = checkBoxSource.DataContext as DisplayProduct;
                if (product != null)
                {
                    this.ClientLogic.AdminSetProductSyncState(product.Id, false);
                    e.Handled = true;
                }
            }
        }
    }
}
