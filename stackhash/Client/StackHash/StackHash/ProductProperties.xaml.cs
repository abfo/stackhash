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
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Properties window for a product
    /// </summary>
    public partial class ProductProperties : Window
    {
        private const string WindowKey = "ProductProperties";

        private DisplayProduct _product;
        private ClientLogic _clientLogic;

        /// <summary>
        /// Properties window for a product
        /// </summary>
        /// <param name="product">The product</param>
        /// <param name="clientLogic">Client logic</param>
        public ProductProperties(DisplayProduct product, ClientLogic clientLogic)
        {
            if (product == null) { throw new ArgumentNullException("product"); }
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }

            _product = product;
            _clientLogic = clientLogic;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        void _clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "NotBusy")
                {
                    gridMain.IsEnabled = _clientLogic.NotBusy;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.UIRequest == ClientLogicUIRequest.ProductPropertiesUpdated)
                {
                    this.DialogResult = true;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged += new PropertyChangedEventHandler(_clientLogic_PropertyChanged);
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);

                // update the title
                this.Title = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ProductProperties_Title,
                    _product.Name,
                    _product.Version);

                // update the control with the current effective policies
                collectionPolicyControl.SetPolicies(_product.CabCollectionPolicy.StackHashCollectionPolicy,
                    _product.EventCollectionPolicy.StackHashCollectionPolicy,
                    StackHashCollectionObject.Product,
                    _product.Id);

                // update the effective display policy
                displayPolicyControl.SetHitThreshold(UserSettings.Settings.GetDisplayHitThreshold(_product.Id),
                    false,
                    UserSettings.Settings.HasOverrideHitThreshold(_product.Id));
            }
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;

                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;

            if (!collectionPolicyControl.IsValid)
            {
                valid = false;

                if (tabControl.SelectedItem != tabItemCollectionPolicy)
                {
                    tabControl.SelectedItem = tabItemCollectionPolicy;
                    valid = collectionPolicyControl.IsValid;
                }
            }

            if (valid)
            {
                if (!displayPolicyControl.IsValid)
                {
                    valid = false;
                    if (tabControl.SelectedItem != tabItemDefaultDisplayFilter)
                    {
                        tabControl.SelectedItem = tabItemDefaultDisplayFilter;
                        valid = displayPolicyControl.IsValid;
                    }
                }
            }

            if (valid)
            {
                // save / clear the display policy
                int hitThreshold = displayPolicyControl.GetHitThreshold();
                if (hitThreshold >= 0)
                {
                    UserSettings.Settings.SetDisplayHitThreshold(_product.Id, hitThreshold);
                }
                else
                {
                    UserSettings.Settings.RemoveDisplayHitThreshold(_product.Id);
                }

                StackHashCollectionPolicyCollection policiesToUpdate;
                StackHashCollectionPolicyCollection policiesToRemove;
                collectionPolicyControl.UpdateAndReturnPolicies(out policiesToUpdate, out policiesToRemove);

                _clientLogic.UpdateProductProperties(policiesToUpdate, policiesToRemove);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabControl.SelectedItem == tabItemCollectionPolicy)
            {
                StackHashHelp.ShowTopic("product-properties-collection.htm");
            }
            else if (tabControl.SelectedItem == tabItemDefaultDisplayFilter)
            {
                StackHashHelp.ShowTopic("product-properties-display-filter.htm");
            }
            else
            {
                StackHashHelp.ShowTopic("product-properties.htm");
            }
        }
    }
}
