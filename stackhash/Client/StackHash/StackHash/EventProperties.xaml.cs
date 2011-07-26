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
using System.Globalization;
using StackHash.StackHashService;

namespace StackHash
{
    /// <summary>
    /// Properties window for an event package
    /// </summary>
    public partial class EventProperties : Window
    {
        private const string WindowKey = "EventProperties";
        private const string ListViewRawEventSignatureKey = "EventProperties.listViewRawEventSignature";

        private DisplayEventPackage _eventPackage;
        private ClientLogic _clientLogic;
        private ListViewSorter _listViewSorter;

        /// <summary>
        /// Properties window for an event package
        /// </summary>
        /// <param name="eventPackage">The event package</param>
        /// <param name="clientLogic">Client Logic</param>
        public EventProperties(DisplayEventPackage eventPackage, ClientLogic clientLogic)
        {
            if (eventPackage == null) { throw new ArgumentNullException("eventPackage"); }
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }

            _eventPackage = eventPackage;
            _clientLogic = clientLogic;

            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (UserSettings.Settings.HaveWindow(WindowKey))
                {
                    UserSettings.Settings.RestoreWindow(WindowKey, this);
                }

                UserSettings.Settings.RestoreGridView(ListViewRawEventSignatureKey, listViewRawEventSignature.View as GridView);
                _listViewSorter = new ListViewSorter(listViewRawEventSignature);
                _listViewSorter.AddDefaultSort("Name", ListSortDirection.Ascending);
                _listViewSorter.AddDefaultSort("Value", ListSortDirection.Ascending);
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
                if (e.UIRequest == ClientLogicUIRequest.EventPackagePropertiesUpdated)
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
                    Properties.Resources.EventProperties_Title,
                    _eventPackage.Id);

                // update the control with the cab policy for this event
                collectionPolicyControl.SetPolicies(_eventPackage.CabCollectionPolicy.StackHashCollectionPolicy, 
                    null,
                    StackHashCollectionObject.Event,
                    _eventPackage.Id);

                // listview dumps the raw event signature parameters
                listViewRawEventSignature.ItemsSource = _eventPackage.StackHashEventPackage.EventData.EventSignature.Parameters;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;

                UserSettings.Settings.SaveGridView(ListViewRawEventSignatureKey, listViewRawEventSignature.View as GridView);
                UserSettings.Settings.SaveWindow(WindowKey, this);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (collectionPolicyControl.IsValid)
            {
                StackHashCollectionPolicyCollection policiesToUpdate;
                StackHashCollectionPolicyCollection policiesToRemove;
                collectionPolicyControl.UpdateAndReturnPolicies(out policiesToUpdate, out policiesToRemove);

                _clientLogic.UpdateEventPackageProperties(policiesToUpdate, policiesToRemove);
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
                StackHashHelp.ShowTopic("event-properties-collection.htm");
            }
            else if (tabControl.SelectedItem == tabItemRawEventSignature)
            {
                StackHashHelp.ShowTopic("event-properties-signature.htm");
            }
            else
            {
                StackHashHelp.ShowTopic("event-properties.htm");
            }
        }

        private void listViewRawEventSignature_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewSorter.SortColumn(header);
            }
        }
    }
}
