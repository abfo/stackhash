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
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// Adds test data to a profile
    /// </summary>
    public partial class AddTestData : Window
    {
        private const string WindowKey = "AddTestData";

        private ClientLogic _clientLogic;
        private StackHashContextSettings _context;
        private StackHashTestIndexData _testData;

        /// <summary>
        /// Adds test data to a profile
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        /// <param name="context">Context (Profile)</param>
        public AddTestData(ClientLogic clientLogic, StackHashContextSettings context)
        {
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }
            if (context == null) { throw new ArgumentNullException("context"); }
            _clientLogic = clientLogic;
            _context = context; 

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _testData = new StackHashTestIndexData();
            _testData.CabFileName = "1630796338-Crash32bit-0760025228.cab";
            _testData.NumberOfCabNotes = 5;
            _testData.NumberOfCabs = 5;
            _testData.NumberOfEventInfos = 100;
            _testData.NumberOfEventNotes = 3;
            _testData.NumberOfEvents = 5000;
            _testData.NumberOfFiles = 7;
            _testData.NumberOfProducts = 10;
            _testData.UnwrapCabs = false;
            _testData.UseLargeCab = false;
            _testData.DuplicateFileIdsAcrossProducts = false;
            _testData.NumberOfScriptResults = 2;

            this.DataContext = _testData;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged += new PropertyChangedEventHandler(_clientLogic_PropertyChanged);
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
            }
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.UIRequest == ClientLogicUIRequest.TestIndexCreated)
                {
                    // close window if test index created
                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        void _clientLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "NotBusy")
                {
                    if (_clientLogic.NotBusy)
                    {
                        gridMain.IsEnabled = true;
                        Mouse.OverrideCursor = null;
                    }
                    else
                    {
                        gridMain.IsEnabled = false;
                        Mouse.OverrideCursor = Cursors.Wait;
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
            }

            UserSettings.Settings.SaveWindow(WindowKey, this);
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (BindingValidator.IsValid(this))
            {
                _clientLogic.TestCreateTestIndex(_context.Id, _testData);
            }
        }
    }
}
