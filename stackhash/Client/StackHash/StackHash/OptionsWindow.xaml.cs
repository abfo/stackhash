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
using Microsoft.Win32;
using System.ComponentModel;
using StackHashUtilities;
using System.Diagnostics;
using StackHash.StackHashService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// StackHash local (client) options 
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private const string WindowKey = "OptionsWindow";

        private class ContextValidation : IDataErrorInfo
        {
            public class DefaultDebuggerItem
            {
                public DefaultDebugger DefaultDebugger { get; set; }
                public string DebuggerName { get; set; }

                public DefaultDebuggerItem(DefaultDebugger defaultDebugger, string debuggerName)
                {
                    this.DefaultDebugger = defaultDebugger;
                    this.DebuggerName = debuggerName;
                }

                public static ObservableCollection<DefaultDebuggerItem> GetList()
                {
                    ObservableCollection<DefaultDebuggerItem> list = new ObservableCollection<DefaultDebuggerItem>();

                    list.Add(new DefaultDebuggerItem(DefaultDebugger.DebuggingToolsForWindows, Properties.Resources.DebuggerName_DebuggingToolsForWindows));
                    list.Add(new DefaultDebuggerItem(DefaultDebugger.VisualStudio, Properties.Resources.DebuggerName_VisualStudio));

                    return list;
                }
            }

            public bool ValidationEnabled { get; set; }
            public string DebuggerPathX86 { get; set; }
            public string DebuggerPathAmd64 { get; set; }
            public string DebuggerPathVisualStudio { get; set; }
            public bool DiagnosticLogEnabled { get; set; }
            public string ServiceHost { get; set; }
            public int ServicePort { get; set; }
            public DefaultDebugger DefaultDebugger { get; set; }
            public int EventsPerPage { get; set; }

            public ObservableCollection<DefaultDebuggerItem> Debuggers { get; private set; }

            private DefaultDebuggerItem _selectedDebugger;

            public DefaultDebuggerItem SelectedDebugger
            {
                get { return _selectedDebugger; }
                set 
                { 
                    _selectedDebugger = value;
                    this.DefaultDebugger = _selectedDebugger.DefaultDebugger;
                }
            }
            
            private bool _is64;

            public ContextValidation(string debuggerPathX86, 
                string debuggerPathAmd64,
                string debuggerPathVisualStudio,
                DefaultDebugger defaultDebugger,
                bool is64, 
                bool diagnosticLogEnabled, 
                string serviceHost, 
                int servicePort,
                int eventsPerPage)
            {
                this.DebuggerPathX86 = debuggerPathX86;
                this.DebuggerPathAmd64 = debuggerPathAmd64;
                this.DebuggerPathVisualStudio = debuggerPathVisualStudio;
                this.DefaultDebugger = defaultDebugger;
                this.DiagnosticLogEnabled = diagnosticLogEnabled;
                _is64 = is64;
                this.ServiceHost = serviceHost;
                this.ServicePort = servicePort;
                this.EventsPerPage = eventsPerPage;

                this.Debuggers = DefaultDebuggerItem.GetList();

                foreach (DefaultDebuggerItem debugger in this.Debuggers)
                {
                    if (debugger.DefaultDebugger == this.DefaultDebugger)
                    {
                        this.SelectedDebugger = debugger;
                        break;
                    }
                }
            }

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get 
                {
                    if (!ValidationEnabled)
                    {
                        return null;
                    }

                    string result = null;

                    switch (columnName)
                    {
                        case "DebuggerPathX86":
                            // must be empty or exist
                            if ((!string.IsNullOrEmpty(this.DebuggerPathX86)) && (!System.IO.File.Exists(this.DebuggerPathX86)))
                            {
                                result = Properties.Resources.OptionsWindow_ValidationErrorX86DebuggerPath;
                            }
                            break;

                        case "DebuggerPathVisualStudio":
                            // must be empty or exist
                            if ((!string.IsNullOrEmpty(this.DebuggerPathVisualStudio)) && (!System.IO.File.Exists(this.DebuggerPathVisualStudio)))
                            {
                                result = Properties.Resources.OptionsWindow_ValidationErrorVisualStudioPath;
                            }
                            break;

                        case "DebuggerPathAmd64":
                            // only validate if on a 64-bit system (must be empty or exit)
                            if (_is64 && (!string.IsNullOrEmpty(this.DebuggerPathAmd64)) && (!System.IO.File.Exists(this.DebuggerPathAmd64)))
                            {
                                result = Properties.Resources.OptionsWindow_ValidationErrorAmd64DebuggerPath;
                            }
                            break;

                        case "ServicePort":
                            if ((this.ServicePort < 0) || (this.ServicePort > 65535))
                            {
                                result = Properties.Resources.OptionsWindow_ValidationErrorServicePort;
                            }
                            break;

                        case "ServiceHost":
                            if (string.IsNullOrEmpty(this.ServiceHost))
                            {
                                result = Properties.Resources.OptionsWindow_ValidationErrorServiceHost;
                            }
                            break;

                        case "EventsPerPage":
                            if (this.EventsPerPage < 1)
                            {
                                result = Properties.Resources.OptionsWindow_ValidationErrorEventsPerPage;
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private ObservableCollection<DisplayContext> _activeContexts;
        private ContextValidation _contextValidation;
        private ClientLogic _clientLogic;
        private string _originalHost;
        private int _originalPort;
        private int _originalContext;
        private string _originalServiceUsername;
        private string _originalServicePassword;
        private string _originalServiceDomain;

        /// <summary>
        /// StackHash local (client) options
        /// </summary>
        public OptionsWindow(ClientLogic clientLogic)
        {
            Debug.Assert(clientLogic != null);

            _clientLogic = clientLogic;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            _activeContexts = new ObservableCollection<DisplayContext>();
            comboBoxProfiles.ItemsSource = _activeContexts;
            UpdateActiveContexts();

            displayPolicyControl.SetHitThreshold(UserSettings.Settings.GetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId), true, true);
        }

        private void UpdateActiveContexts()
        {
            _activeContexts.Clear();

            if (_clientLogic.ContextCollection != null)
            {
                foreach (DisplayContext context in _clientLogic.ContextCollection)
                {
                    if (context.IsActive)
                    {
                        _activeContexts.Add(context);

                        if (context.Id == UserSettings.Settings.CurrentContextId)
                        {
                            comboBoxProfiles.SelectedItem = context;
                        }
                    }
                }
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            // enable validation on OK
            _contextValidation.ValidationEnabled = true;

            bool valid = true;

            // check the proxy settings control
            if (valid)
            {
                if (!proxySettingsControl.IsValid)
                {
                    valid = false;

                    if (tabControl.SelectedItem != tabItemProxyServer)
                    {
                        tabControl.SelectedItem = tabItemProxyServer;

                        // validate again to highlight error
                        valid = proxySettingsControl.IsValid;
                    }
                }
            }

            // check the display policy control
            if (valid)
            {
                if (!displayPolicyControl.IsValid)
                {
                    valid = false;

                    if (tabControl.SelectedItem != tabItemDefaultDisplayFilter)
                    {
                        tabControl.SelectedItem = tabItemDefaultDisplayFilter;

                        // validate again to highlight error
                        valid = displayPolicyControl.IsValid;
                    }
                }
            }

            // check other tabs
            if (valid)
            {
                foreach (TabItem tab in tabControl.Items)
                {
                    if (!BindingValidator.IsValid(tab))
                    {
                        valid = false;

                        // activate the bad tab if it's not the current one
                        if (tabControl.SelectedItem != tab)
                        {
                            tabControl.SelectedItem = tab;

                            // need to validate again to highlight errors
                            BindingValidator.IsValid(tab);
                        }

                        break;
                    }
                }
            }

            // prompt the user if a profile has not been selected
            if (valid)
            {
                if (!(comboBoxProfiles.SelectedItem is DisplayContext))
                {
                    if (StackHashMessageBox.Show(this,
                        Properties.Resources.OptionsWindow_SelectProfileMBMessage,
                        Properties.Resources.OptionsWindow_SelectProfileMBTitle,
                        StackHashMessageBoxType.YesNo,
                        StackHashMessageBoxIcon.Question) != StackHashDialogResult.Yes)
                    {
                        valid = false;

                        tabControl.SelectedItem = tabItemServiceConnection;
                    }
                    else
                    {
                        UserSettings.Settings.CurrentContextId = -1;
                    }
                }
            }

            if (valid)
            {
                UserSettings.Settings.DebuggerPathX86 = _contextValidation.DebuggerPathX86;
                UserSettings.Settings.DebuggerPathAmd64 = _contextValidation.DebuggerPathAmd64;
                UserSettings.Settings.DebuggerPathVisualStudio = _contextValidation.DebuggerPathVisualStudio;
                UserSettings.Settings.DefaultDebugTool = _contextValidation.DefaultDebugger;
                UserSettings.Settings.DiagnosticLogEnabled = _contextValidation.DiagnosticLogEnabled;
                UserSettings.Settings.ServiceHost = _contextValidation.ServiceHost;
                UserSettings.Settings.ServicePort = _contextValidation.ServicePort;
                UserSettings.Settings.EventPageSize = _contextValidation.EventsPerPage;

                // save proxy server settings
                UserSettings.Settings.UseProxyServer = proxySettingsControl.ProxySettings.UseProxy;
                UserSettings.Settings.UseProxyServerAuthentication = proxySettingsControl.ProxySettings.UseProxyAuthentication;
                UserSettings.Settings.ProxyHost = proxySettingsControl.ProxySettings.ProxyHost;
                UserSettings.Settings.ProxyPort = proxySettingsControl.ProxySettings.ProxyPort;
                UserSettings.Settings.ProxyUsername = proxySettingsControl.ProxySettings.ProxyUsername;
                UserSettings.Settings.ProxyPassword = proxySettingsControl.ProxySettings.ProxyPassword;
                UserSettings.Settings.ProxyDomain = proxySettingsControl.ProxySettings.ProxyDomain;

                // save default display policy
                UserSettings.Settings.SetDisplayHitThreshold(UserSettings.DefaultDisplayFilterProductId,
                    displayPolicyControl.GetHitThreshold());

                DialogResult = true;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            // restore the original host/port (may have been changed if the user clicked Manage)
            ServiceProxy.Services.UpdateServiceEndpointAndAccount(_originalHost, 
                _originalPort,
                _originalServiceUsername,
                _originalServicePassword,
                _originalServiceDomain);
            
            UserSettings.Settings.CurrentContextId = _originalContext;
            UserSettings.Settings.ServiceUsername = _originalServiceUsername;
            UserSettings.Settings.ServicePassword = _originalServicePassword;
            UserSettings.Settings.ServiceDomain = _originalServiceDomain;
        }

        private void buttonBrowseX86_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textBoxDebuggerX86.Text))
            {
                ofd.FileName = textBoxDebuggerX86.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.OptionsWindow_BrowseX86DebuggerTitle;

            if (ofd.ShowDialog(this) == true)
            {
                textBoxDebuggerX86.Text = ofd.FileName;
            }
        }

        private void buttonBrowseAmd64_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textBoxDebuggerAmd64.Text))
            {
                ofd.FileName = textBoxDebuggerAmd64.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.OptionsWindow_BrowseAmd64DebuggerTitle;

            if (ofd.ShowDialog(this) == true)
            {
                textBoxDebuggerAmd64.Text = ofd.FileName;
            }
        }

        private void buttonBrowseVisualStudio_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (System.IO.File.Exists(textBoxDebuggerVisualStudio.Text))
            {
                ofd.FileName = textBoxDebuggerVisualStudio.Text;
            }
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = false;
            ofd.ValidateNames = true;
            ofd.Filter = Properties.Resources.ExeBrowseFilter;
            ofd.Title = Properties.Resources.OptionsWindow_BrowseVisualStudio;

            if (ofd.ShowDialog(this) == true)
            {
                textBoxDebuggerVisualStudio.Text = ofd.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // save the current host/port to use on cancel
            _originalHost = UserSettings.Settings.ServiceHost;
            _originalPort = UserSettings.Settings.ServicePort;
            _originalContext = UserSettings.Settings.CurrentContextId;
            _originalServiceUsername = UserSettings.Settings.ServiceUsername;
            _originalServicePassword = UserSettings.Settings.ServicePassword;
            _originalServiceDomain = UserSettings.Settings.ServiceDomain;

            // set current proxy server settings
            proxySettingsControl.ProxySettings = new ProxySettings(UserSettings.Settings.UseProxyServer,
                UserSettings.Settings.UseProxyServerAuthentication,
                UserSettings.Settings.ProxyHost,
                UserSettings.Settings.ProxyPort,
                UserSettings.Settings.ProxyUsername,
                UserSettings.Settings.ProxyPassword,
                UserSettings.Settings.ProxyDomain);

            bool is64 = SystemInformation.Is64BitSystem();

            labelDebuggerAmd64.IsEnabled = is64;
            textBoxDebuggerAmd64.IsEnabled = is64;
            buttonBrowseAmd64.IsEnabled = is64;

            _contextValidation = new ContextValidation(UserSettings.Settings.DebuggerPathX86,
                UserSettings.Settings.DebuggerPathAmd64,
                UserSettings.Settings.DebuggerPathVisualStudio,
                UserSettings.Settings.DefaultDebugTool,
                is64,
                UserSettings.Settings.DiagnosticLogEnabled,
                UserSettings.Settings.ServiceHost,
                UserSettings.Settings.ServicePort,
                UserSettings.Settings.EventPageSize);

            this.DataContext = _contextValidation;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
                _clientLogic.PropertyChanged += new PropertyChangedEventHandler(_clientLogic_PropertyChanged);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;

                UserSettings.Settings.SaveWindow(WindowKey, this);
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
            if (e.UIRequest == ClientLogicUIRequest.ContextCollectionReady)
            {
                this.Dispatcher.BeginInvoke(new Action(UpdateActiveContexts));
            }
        }

        private void buttonManageProfiles_Click(object sender, RoutedEventArgs e)
        {
            if (_clientLogic.ServiceIsLocal)
            {
                // enable validation on OK
                _contextValidation.ValidationEnabled = true;

                if (BindingValidator.IsValid(tabItemServiceConnection))
                {
                    // need to update in case the user has changed the service details
                    ServiceProxy.Services.UpdateServiceEndpointAndAccount(_contextValidation.ServiceHost,
                        _contextValidation.ServicePort,
                        UserSettings.Settings.ServiceUsername,
                        UserSettings.Settings.ServicePassword,
                        UserSettings.Settings.ServiceDomain);

                    ProfileManager profileManager = new ProfileManager(_clientLogic);
                    profileManager.Owner = this;
                    profileManager.ShowDialog();

                    UpdateActiveContexts();
                }
                else
                {
                    // select and revalidate the service connection tab if necessary
                    if (tabControl.SelectedItem != tabItemServiceConnection)
                    {
                        tabControl.SelectedItem = tabItemServiceConnection;
                        BindingValidator.IsValid(tabItemServiceConnection);
                    }
                }
            }
            else
            {
                StackHashMessageBox.Show(this,
                    string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ServiceOptionsUnavailable_MBMessage,
                    _clientLogic.ServiceHost),
                    Properties.Resources.ServiceOptionsUnavailable_MBTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Information);
            }
        }

        private void comboBoxProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayContext selectedContext = comboBoxProfiles.SelectedItem as DisplayContext;

            if (selectedContext != null)
            {
                UserSettings.Settings.CurrentContextId = selectedContext.Id;
            }
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabControl.SelectedItem == tabItemServiceConnection)
            {
                StackHashHelp.ShowTopic("client-options-service-connection.htm");
            }
            else if (tabControl.SelectedItem == tabItemDebuggers)
            {
                StackHashHelp.ShowTopic("client-options-debuggers.htm");
            }
            else if (tabControl.SelectedItem == tabItemProxyServer)
            {
                StackHashHelp.ShowTopic("client-options-proxy-server.htm");
            }
            else if (tabControl.SelectedItem == tabItemDefaultDisplayFilter)
            {
                StackHashHelp.ShowTopic("client-options-display-filter.htm");
            }
            else if (tabControl.SelectedItem == tabItemAdvanced)
            {
                StackHashHelp.ShowTopic("client-options-advanced.htm");
            }
            else
            {
                StackHashHelp.ShowTopic("client-options.htm");
            }
        }

        private void DoRefreshProfiles()
        {
            // enable validation on OK
            _contextValidation.ValidationEnabled = true;

            // validate the service connection tab - must be on this tab to click the refresh profiles button
            if (BindingValidator.IsValid(tabItemServiceConnection))
            {
                // need to update in case the user has changed the service details
                ServiceProxy.Services.UpdateServiceEndpointAndAccount(_contextValidation.ServiceHost,
                    _contextValidation.ServicePort,
                    UserSettings.Settings.ServiceUsername,
                    UserSettings.Settings.ServicePassword,
                    UserSettings.Settings.ServiceDomain);

                _activeContexts.Clear();

                _clientLogic.RefreshContextSettings();
            }
        }

        private void buttonRefreshProfiles_Click(object sender, RoutedEventArgs e)
        {
            DoRefreshProfiles();
        }

        private void textBoxServiceHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_activeContexts.Count > 0)
            {
                if (textBoxServiceHost.Text != ServiceProxy.Services.ServiceHost)
                {
                    _activeContexts.Clear();
                }
            }
        }

        private void textBoxServicePort_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_activeContexts.Count > 0)
            {
                bool clearContexts = true;

                int port;
                if (Int32.TryParse(textBoxServicePort.Text, out port))
                {
                    if (port == ServiceProxy.Services.ServicePort)
                    {
                        clearContexts = false;
                    }
                }

                if (clearContexts)
                {
                    _activeContexts.Clear();
                }
            }
        }

        private void hyperlinkCredentials_Click(object sender, RoutedEventArgs e)
        {
            ServiceCredentials serviceCredentials = new ServiceCredentials();
            serviceCredentials.Owner = Window.GetWindow(this);
            serviceCredentials.Username = UserSettings.Settings.ServiceUsername;
            serviceCredentials.Password = UserSettings.Settings.ServicePassword;
            serviceCredentials.Domain = UserSettings.Settings.ServiceDomain;

            if (serviceCredentials.ShowDialog() == true)
            {
                UserSettings.Settings.ServiceUsername = serviceCredentials.Username;
                UserSettings.Settings.ServicePassword = serviceCredentials.Password;
                UserSettings.Settings.ServiceDomain = serviceCredentials.Domain;

                DoRefreshProfiles();
            }
        }

        private void linkResetMessages_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Settings.ClearSuppressedMessages();
            linkResetMessages.IsEnabled = false;
        }
    }
}
