using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;
using StackHash.StackHashService;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;

using System.Net;
using System.ServiceModel;
using System.Windows.Controls;
using System.Windows;
using StackHashUtilities;

namespace StackHash
{
    /// <summary>
    /// The default debugging tools to use for a memory dump
    /// </summary>
    public enum DefaultDebugger
    {
        /// <summary>
        /// Debugging Tools for Windows (i.e. windbg.exe)
        /// </summary>
        DebuggingToolsForWindows,

        /// <summary>
        /// Visual Studio (devenv.exe)
        /// </summary>
        VisualStudio
    }

    /// <summary>
    /// User settings for the StackHash Client
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed class UserSettings : IDisposable, INotifyPropertyChanged
    {
        private class SavedSearch : IComparable<SavedSearch>, IComparable, IEquatable<SavedSearch>
        {
            public string Name { get; set; }
            public string Search { get; set; }

            public SavedSearch(string name, string search)
            {
                Debug.Assert(!string.IsNullOrEmpty(name));
                Debug.Assert(!string.IsNullOrEmpty(search));

                this.Name = name;
                this.Search = search;
            }

            #region IComparable<SavedSearch> Members

            public int CompareTo(SavedSearch other)
            {
                if (other == null)
                {
                    return -1;
                }
                else
                {
                    // sort by name, then by search
                    int ret = string.Compare(this.Name, other.Name, StringComparison.CurrentCulture);
                    if (ret == 0)
                    {
                        ret = string.Compare(this.Search, other.Search, StringComparison.CurrentCulture);
                    }
                    return ret;
                }
            }

            #endregion

            #region IComparable Members

            public int CompareTo(object obj)
            {
                SavedSearch search = obj as SavedSearch;
                if (search == null)
                {
                    return -1;
                }
                else
                {
                    return this.CompareTo(search);
                }
            }

            #endregion

            #region IEquatable<SavedSearch> Members

            public bool Equals(SavedSearch other)
            {
                if (other == null)
                {
                    return false;
                }
                else
                {
                    return ((string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0) &&
                        (string.Compare(this.Search, other.Search, StringComparison.OrdinalIgnoreCase) == 0));
                }
            }

            #endregion
        }

        private class ToolBarInfo
        {
            public string Key { get; private set; }
            public int Band { get; private set; }
            public int BandIndex { get; private set; }
            public double Width { get; private set; }

            public ToolBarInfo(string key, int band, int bandIndex, double width)
            {
                this.Key = key;
                this.Band = band;
                this.BandIndex = bandIndex;
                this.Width = width;
            }
        }

        private class WindowInfo
        {
            public string Key { get; private set; }
            public double Top { get; private set; }
            public double Left { get; private set; }
            public double Height { get; private set; }
            public double Width { get; private set; }
            public WindowState State { get; private set; }

            public WindowInfo(string key, double top, double left, double height, double width, WindowState state)
            {
                this.Key = key;
                this.Top = top;
                this.Left = left;
                this.Height = height;
                this.Width = width;
                this.State = state;
            }
        }

        private class GridViewColumnInfo
        {
            public int Index { get; private set; }
            public double Width { get; private set; }
            public string Header { get; private set; }

            public GridViewColumnInfo(int index, double width, string header)
            {
                this.Index = index;
                this.Width = width;
                this.Header = header;
            }
        }

        private class GridViewInfo
        {
            public List<GridViewColumnInfo> ColumnInfos { get; private set; }
            public string Key { get; private set; }

            public GridViewInfo(string key)
            {
                ColumnInfos = new List<GridViewColumnInfo>();
                this.Key = key;
            }
        }

        private static readonly UserSettings _settings = new UserSettings();
        private const int RecentSearchListLength = 10;
        private const int InitialUpdateCheckDays = 1;
        private const int SaveRetryCount = 5;
        private const int SaveRetryWaitMs = 250;
        private const int DefaultEventPageSize = 100;

        private GridLengthConverter _gridLengthConverter;
        private ReaderWriterLockSlim _rwLock;
        private bool _disposed;
        private ClientLogic _clientLogic;

        private const string DemoSwitch = "/demo";
        private const string ResetWindowSwitch = "/resetwindow";
        private const string WebServicesConfigName = "StackHashServiceSoap";
        private const string WebServicesEndpoint = "http://www.stackhash.com/service/StackHashService.asmx";

        /// <summary>
        /// Suppress MainWindow.SearchAllProducts Message
        /// </summary>
        public const string SuppressMainWindowSearchAllProducts = "MainWindow.SearchAllProducts";

        /// <summary>
        /// Suppress MainWindow.SearchScripts Message
        /// </summary>
        public const string SuppressMainWindowSearchScripts = "MainWindow.SearchScripts";

        /// <summary>
        /// Suppress MainWindow.Sync Message
        /// </summary>
        public const string SuppressMainWindowSync = "MainWindow.Sync";

        /// <summary>
        /// Supresss MainWindow.DebuggerCheck Message
        /// </summary>
        public const string SuppressMainWindowDebuggerCheck = "MainWindow.DebuggerCheck";

        /// <summary>
        /// Suppress CabDetails.DeleteScriptRun Message
        /// </summary>
        public const string SuppressCabDetailsDeleteScriptRun = "CabDetails.DeleteScriptRun";

        /// <summary>
        /// True if in demo mode (/demo passed on the command line)
        /// </summary>
        public bool DemoMode { get; private set; }

        /// <summary>
        /// The number of days until the next automatic check for updates
        /// </summary>
        public const int UpdateCheckIntervalInDays = 7;

        /// <summary>
        /// Indicates no current context Id
        /// </summary>
        public const int InvalidContextId = -1;

        /// <summary>
        /// Invalud product Id used to store the default display filter
        /// </summary>
        public const int DefaultDisplayFilterProductId = -1;

        /// <summary>
        /// Default port for the StackHash Service
        /// </summary>
        public const int DefaultServicePort = 9000;

        /// <summary>
        /// Default port for the proxy server
        /// </summary>
        public const int DefaultProxyServerPort = 8080;

        /// <summary>
        /// Default host for the StackHash Service
        /// </summary>
        public const string DefaultServiceHost = "localhost";

        private string _settingsPath;
        private ReadOnlyCollection<string> _searchList;
        private string _version;
        private int _versionMajor;
        private int _versionMinor;
        private string _copyright;
        private TextWriterTraceListener _listener;
        private Guid _sessionId;
        private bool _forceTrace;
        private string _webServiceOverrideAddress;
        private bool _clientIsLocal;
        private Guid _localServiceGuid;

        private const string SettingsFolder = "StackHash";
        private const string SettingsFile = "StackHash.xml";
        private const string DesktopLogTriggerFile = "forceshlog.txt";
        private const string ElementSettings = "StackHashUserSettings";
        private const string TraceFile = "StackHash Client Log.txt";
        private const string TraceName = "StackHashClientLog";

        // persited fields
        // update RaiseAllPropertiesChanged() when adding a new field
        private int _currentContextId;
        private const string ElementCurrentContextId = "CurrentContextId";
        private List<string> _recentSearches;
        private const string ElementRecentSearchList = "RecentSearches";
        private const string ElementRecentSearch = "RecentSearch";
        private List<SavedSearch> _savedSearches;
        private const string ElementSavedSearchList = "SavedSearches";
        private const string ElementSavedSearch = "SavedSearch";
        private const string ElementSavedSearchName = "SavedSearchName";
        private const string ElementSavedSearchSearch = "SavedSearchSearch";
        private string _debuggerPathX86;
        private const string ElementDebuggerPathX86 = "DebuggerPathX86";
        private string _debuggerPathAmd64;
        private const string ElementDebuggerPathAmd64 = "DebuggerPathAMD64";
        private string _debuggerPathVisualStudio;
        private const string ElementDebuggerPathVisualStudio = "DebuggerPathVisualStudio";
        private Guid _instanceId;
        private const string ElementInstanceId = "InstanceId";
        private int _requestId;
        private const string ElementRequestId = "RequestId";
        private bool _diagnosticLogEnabled;
        private const string ElementDiagnosticLogEnabled = "DiagnosticLogEnabled";
        private int _servicePort;
        private const string ElementServicePort = "ServicePort";
        private string _serviceHost;
        private const string ElementServiceHost = "ServiceHost";
        private bool _firstConfigComplete;
        private const string ElementFirstConfigComplete = "FirstConfigComplete";
        private bool _showDisabledProducts;
        private const string ElementShowDisabledProducts = "ShowDisabledProducts";
        private bool _useProxyServer;
        private const string ElementUseProxyServer = "UseProxyServer";
        private bool _useProxyServerAuthentication;
        private const string ElementUseProxyServerAuthentication = "UseProxyServerAuthentication";
        private string _proxyHost;
        private const string ElementProxyHost = "ProxyHost";
        private int _proxyPort;
        private const string ElementProxyPort = "ProxyPort";
        private string _proxyUsername;
        private const string ElementProxyUsername = "ProxyUsername";
        private string _proxyPassword;
        private const string ElementProxyPassword = "ProxyPassword";
        private string _proxyDomain;
        private const string ElementProxyDomain = "ProxyDomain";
        private DefaultDebugger _defaultDebugTool;
        private const string ElementDefaultDebugTool = "DefaultDebugTool";
        private Dictionary<string, GridViewInfo> _gridViews;
        private const string ElementGridViews = "GridViews";
        private const string ElementGridView = "GridView";
        private const string AttributeKey = "key";
        private const string ElementGridViewColumn = "GridViewColumn";
        private const string AttributeIndex = "index";
        private const string AttributeWidth = "width";
        private const string AttributeHeader = "header";
        private Dictionary<string, string> _gridLengths;
        private const string ElementGridLengths = "GridLengths";
        private const string ElementGridLength = "GridLength";
        private const string AttributeLength = "length";
        private Dictionary<string, ToolBarInfo> _toolbars;
        private const string ElementToolbars = "Toolbars";
        private const string ElementToolbar = "Toolbar";
        private const string AttributeBand = "band";
        private const string AttributeBandIndex = "bandindex";
        private double _virtualScreenTop;
        private double _virtualScreenLeft;
        private double _virtualScreenWidth;
        private double _virtualScreenHeight;
        private const string ElementVirtualScreen = "VirtualScreen";
        private const string AttributeLeft = "left";
        private const string AttributeTop = "top";
        private const string AttributeHeight = "height";
        private Dictionary<string, WindowInfo> _windows;
        private const string ElementWindows = "Windows";
        private const string ElementWindow = "Window";
        private const string AttributeState = "state";
        private Dictionary<int, int> _displayFilters;
        private const string ElementDisplayFilters = "DisplayFilters";
        private const string ElementDisplayFilter = "DisplayFilter";
        private const string AttributeProduct = "product";
        private const string AttributeHits = "hits";
        private int _hitScaleProduct;
        private const string ElementHitScaleProduct = "HitScaleProduct";
        private int _hitScaleEvent;
        private const string ElementHitScaleEvent = "HitScaleEvent";
        private int _eventPageSize;
        private const string ElementEventPageSize = "EventPageSize";
        private string _serviceUsername;
        private const string ElementServiceUsername = "ServiceUsername";
        private string _servicePassword;
        private const string ElementServicePassword = "ServicePassword";
        private string _serviceDomain;
        private const string ElementServiceDomain = "ServiceDomain";
        private List<string> _suppressedMessages;
        private const string ElementSuppressedMessages = "SuppressedMessages";
        private const string ElementSuppressedMessage = "SuppressedMessage";
        private bool _showEventsWithoutCabs;
        private const string ElementShowEventsWithoutCabs = "ShowEventsWithoutCabs";

        /// <summary>
        /// True to show events with no associated cab files
        /// </summary>
        public bool ShowEventsWithoutCabs
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _showEventsWithoutCabs;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_showEventsWithoutCabs != value)
                    {
                        _showEventsWithoutCabs = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ShowEventsWithoutCabs");
                }
            }
        }

        /// <summary>
        /// The Guid of the service if the client is on the same machine as the service.
        /// This setting is not persisted, the value is set by ClientLogic on connecting
        /// to the service.
        /// </summary>
        public Guid LocalServiceGuid
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _localServiceGuid;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_localServiceGuid != value)
                    {
                        _localServiceGuid = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("LocalServiceGuid");
                }
            }
        }

        /// <summary>
        /// True if the client is on the same machine as the service. This setting is not
        /// persisted, the value is set by ClientLogic on connecting to the service.
        /// </summary>
        public bool ClientIsLocal
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _clientIsLocal;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_clientIsLocal != value)
                    {
                        _clientIsLocal = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ClientIsLocal");
                }
            }
        }

        /// <summary>
        /// Gets or sets the override address for the StackHash Web Service. Set to null to
        /// use the default (main address). This setting is not persisted, the value is set
        /// when the client connects to the service
        /// </summary>
        public string WebServiceOverrideAddress
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _webServiceOverrideAddress;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_webServiceOverrideAddress != value)
                    {
                        _webServiceOverrideAddress = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("WebServiceOverrideAddress");
                }
            }
        }

        /// <summary>
        /// Gets or sets the username of the account to impersonate when accessing the service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username")]
        public string ServiceUsername
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _serviceUsername;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_serviceUsername != value)
                    {
                        _serviceUsername = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ServiceUsername");
                }
            }
        }

        /// <summary>
        /// Gets or sets the password of the account to impersonate when accessing the service
        /// </summary>
        public string ServicePassword
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _servicePassword;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_servicePassword != value)
                    {
                        _servicePassword = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ServicePassword");
                }
            }
        }

        /// <summary>
        /// Gets or sets the domain of the account to impersonate when accessing the service
        /// </summary>
        public string ServiceDomain
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _serviceDomain;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_serviceDomain != value)
                    {
                        _serviceDomain = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ServiceDomain");
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of events to display per page
        /// </summary>
        public int EventPageSize
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _eventPageSize;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_eventPageSize != value)
                    {
                        _eventPageSize = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("EventPageSize");
                }
            }
        }

        /// <summary>
        /// Gets or sets the hit chart scale for the product list chart
        /// </summary>
        public int HitScaleProduct
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _hitScaleProduct;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_hitScaleProduct != value)
                    {
                        _hitScaleProduct = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("HitScaleProduct");
                }
            }
        }

        /// <summary>
        /// Gets or sets the hit chart scale for the event list chart
        /// </summary>
        public int HitScaleEvent
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _hitScaleEvent;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_hitScaleEvent != value)
                    {
                        _hitScaleEvent = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("HitScaleProduct");
                }
            }
        }

        /// <summary>
        /// Gets or sets the current Context Id for the StackHash service
        /// </summary>
        public int CurrentContextId
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _currentContextId;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_currentContextId != value)
                    {
                        _currentContextId = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("CurrentContextId");
                }
            }
        }

        /// <summary>
        /// Gets or sets the 32-bit debugger path
        /// </summary>
        public string DebuggerPathX86
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _debuggerPathX86;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_debuggerPathX86 != value)
                    {
                        _debuggerPathX86 = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("DebuggerPathX86");
                }
            }
        }

        /// <summary>
        /// Gets or sets the 64-bit debugger path
        /// </summary>
        public string DebuggerPathAmd64
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _debuggerPathAmd64;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_debuggerPathAmd64 != value)
                    {
                        _debuggerPathAmd64 = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("DebuggerPathAMD64");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Visual Studio debugger path
        /// </summary>
        public string DebuggerPathVisualStudio
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _debuggerPathVisualStudio;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_debuggerPathVisualStudio != value)
                    {
                        _debuggerPathVisualStudio = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("DebuggerPathVisualStudio");
                }
            }
        }

        /// <summary>
        /// Gets or sets the default debug tool
        /// </summary>
        public DefaultDebugger DefaultDebugTool
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _defaultDebugTool;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_defaultDebugTool != value)
                    {
                        _defaultDebugTool = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("DefaultDebugTool");
                }
            }
        }

        /// <summary>
        /// Gets the list of recent and saved searches
        /// </summary>
        public ReadOnlyCollection<string> SearchList
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _searchList;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            private set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_searchList != value)
                    {
                        _searchList = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("SearchList");
                }
            }
        }

        /// <summary>
        /// Gets or sets the state of the diagnostic log
        /// </summary>
        public bool DiagnosticLogEnabled
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _diagnosticLogEnabled;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_diagnosticLogEnabled != value)
                    {
                        _diagnosticLogEnabled = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("DiagnosticLogEnabled");

                    // also update the current trace (diagnostic log) state
                    UpdateTrace();
                }
            }
        }

        /// <summary>
        /// Gets or sets the port of the StackHash Service
        /// </summary>
        public int ServicePort
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _servicePort;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_servicePort != value)
                    {
                        _servicePort = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ServicePort");
                }
            }
        }

        /// <summary>
        /// Gets or sets the hostname (or IP address) of the StackHash service
        /// </summary>
        public string ServiceHost
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _serviceHost;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_serviceHost != value)
                    {
                        _serviceHost = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ServiceHost");
                }
            }
        }

        /// <summary>
        /// True if first-run configuration has been completed
        /// </summary>
        public bool FirstConfigComplete
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _firstConfigComplete;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_firstConfigComplete != value)
                    {
                        _firstConfigComplete = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("FirstConfigComplete");
                }
            }
        }

        /// <summary>
        /// True if products that are disabled for sync should be shown in the UI
        /// </summary>
        public bool ShowDisabledProducts
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _showDisabledProducts;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_showDisabledProducts != value)
                    {
                        _showDisabledProducts = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ShowDisabledProducts");
                }
            }
        }

        /// <summary>
        /// True if a proxy server is required to access the Internet
        /// </summary>
        public bool UseProxyServer
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _useProxyServer;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_useProxyServer != value)
                    {
                        _useProxyServer = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("UseProxyServer");
                }
            }
        }

        /// <summary>
        /// True if the proxy server requires validation
        /// </summary>
        public bool UseProxyServerAuthentication
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _useProxyServerAuthentication;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_useProxyServerAuthentication != value)
                    {
                        _useProxyServerAuthentication = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("UseProxyServerAuthentication");
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server host
        /// </summary>
        public string ProxyHost
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _proxyHost;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_proxyHost != value)
                    {
                        _proxyHost = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ProxyHost");
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server port
        /// </summary>
        public int ProxyPort
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _proxyPort;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_proxyPort != value)
                    {
                        _proxyPort = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ProxyPort");
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server username
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username")]
        public string ProxyUsername
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _proxyUsername;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_proxyUsername != value)
                    {
                        _proxyUsername = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ProxyUsername");
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server password
        /// </summary>
        public string ProxyPassword
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _proxyPassword;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_proxyPassword != value)
                    {
                        _proxyPassword = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ProxyPassword");
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server domain
        /// </summary>
        public string ProxyDomain
        {
            get 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                _rwLock.EnterReadLock();
                try
                {
                    return _proxyDomain;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                bool propertyChanged = false;

                _rwLock.EnterWriteLock();
                try
                {
                    if (_proxyDomain != value)
                    {
                        _proxyDomain = value;
                        propertyChanged = true;
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                if (propertyChanged)
                {
                    RaisePropertyChanged("ProxyDomain");
                }
            }
        }

        /// <summary>
        /// Gets the Instance ID (should stay the same for the same install)
        /// </summary>
        public Guid InstanceId
        {
            get 
            { 
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");
                
                _rwLock.EnterReadLock();
                try
                {
                    return _instanceId;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the Session ID (unique for each running instance of the client)
        /// </summary>
        public Guid SessionId
        {
            get 
            { 
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");
                
                _rwLock.EnterReadLock();
                try
                {
                    return _sessionId;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the next Request ID (for service calls)
        /// </summary>
        public int RequestId
        {
            get 
            { 
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");
                
                _rwLock.EnterReadLock();
                try
                {
                    return _requestId;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the current client version
        /// </summary>
        public string Version
        {
            get { if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings"); return _version; }
        }

        /// <summary>
        /// Gets the current client major version
        /// </summary>
        public int VersionMajor
        {
            get { if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings"); return _versionMajor; }
        }

        /// <summary>
        /// Gets the current client minor version
        /// </summary>
        public int VersionMinor
        {
            get { if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings"); return _versionMinor; }
        }

        /// <summary>
        /// Gets the copyright for the current version
        /// </summary>
        public string Copyright
        {
            get { if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings"); return _copyright; }
        }

        /// <summary>
        /// Gets the name to log for the current user
        /// </summary>
        public string UserLogName
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

                if (string.IsNullOrEmpty(Environment.UserDomainName))
                {
                    return string.Format(CultureInfo.CurrentCulture,
                       "{0}\\{1}",
                       Environment.UserDomainName,
                       Environment.UserName);
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture,
                       "{0}\\{1}",
                       Environment.MachineName,
                       Environment.UserName);
                }
            }
        }

        /// <summary>
        /// The single instance of UserSettings
        /// </summary>
        public static UserSettings Settings
        {
            get { return UserSettings._settings; }
        }

        /// <summary>
        /// Generates a new session Id - use this when connecting to a different service
        /// </summary>
        public void GenerateNewSessionId()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _sessionId = Guid.NewGuid();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Create and return ClientLogic. This allows UserSettings to dispose ClientLogic
        /// on app shutdown
        /// </summary>
        /// <param name="guiDispatcher">GUI Dispatcher</param>
        /// <returns>ClientLogic</returns>
        public ClientLogic CreateClientLogic(System.Windows.Threading.Dispatcher guiDispatcher)
        {
            _clientLogic = new ClientLogic(guiDispatcher);
            return _clientLogic;
        }

        /// <summary>
        /// Returns true if a message is supressed
        /// </summary>
        /// <param name="messageId">Message Id to query</param>
        /// <returns>True if supressed</returns>
        public bool IsMessageSuppressed(string messageId)
        {
            return _suppressedMessages.Contains(messageId);
        }

        /// <summary>
        /// Supresses a message
        /// </summary>
        /// <param name="messageId">Message Id to supress</param>
        public void SuppressMessage(string messageId)
        {
            if (!IsMessageSuppressed(messageId))
            {
                _suppressedMessages.Add(messageId);
            }
        }

        /// <summary>
        /// Clears all supressed messages
        /// </summary>
        public void ClearSuppressedMessages()
        {
            _suppressedMessages.Clear();
        }

        /// <summary>
        /// Stores the layout of a GridView
        /// </summary>
        /// <param name="key">Unique key used to store and restore layout</param>
        /// <param name="gridView">The GridView</param>
        public void SaveGridView(string key, GridView gridView)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (gridView == null) { throw new ArgumentNullException("gridView"); }

            if (gridView.Columns.Count > 0)
            {
                GridViewInfo info = new GridViewInfo(key);

                for (int i = 0; i < gridView.Columns.Count; i++)
                {
                    GridViewColumnHeader header = gridView.Columns[i].Header as GridViewColumnHeader;
                    if (header != null)
                    {
                        info.ColumnInfos.Add(new GridViewColumnInfo(i, gridView.Columns[i].ActualWidth, header.Content as string));
                    }
                }

                if (_gridViews.ContainsKey(key))
                {
                    _gridViews[key] = info;
                }
                else
                {
                    _gridViews.Add(key, info);
                }
            }
        }

        /// <summary>
        /// Restores the layout of a GridView
        /// </summary>
        /// <param name="key">Unique key used to store and restore layout</param>
        /// <param name="gridView">The GridView</param>
        public void RestoreGridView(string key, GridView gridView)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (gridView == null) { throw new ArgumentNullException("gridView"); }

            if (_gridViews.ContainsKey(key))
            {
                GridViewInfo info = _gridViews[key];
                for (int i = 0; i < info.ColumnInfos.Count; i++)
                {
                    int currentIndex = GetCurrentColumnIndexSetWidth(info.ColumnInfos[i], gridView);
                    if ((currentIndex >= 0) && (currentIndex != info.ColumnInfos[i].Index))
                    {
                        gridView.Columns.Move(currentIndex, info.ColumnInfos[i].Index);
                    }
                }
            }
        }

        private static int GetCurrentColumnIndexSetWidth(GridViewColumnInfo columnInfo, GridView gridView)
        {
            int index = -1;

            for (int i = 0; i < gridView.Columns.Count; i++)
            {
                GridViewColumnHeader candidateHeader = gridView.Columns[i].Header as GridViewColumnHeader;
                if (candidateHeader != null)
                {
                    string candidateHeaderString = candidateHeader.Content as string;
                    if (columnInfo.Header == candidateHeaderString)
                    {
                        gridView.Columns[i].Width = columnInfo.Width;
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Saves a GridLength
        /// </summary>
        /// <param name="key">Unique key used to store and restore the length</param>
        /// <param name="length"></param>
        public void SaveGridLength(string key, GridLength length)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (length == null) { throw new ArgumentNullException("length"); }

            string convertedLength = _gridLengthConverter.ConvertToInvariantString(length);
            if (_gridLengths.ContainsKey(key))
            {
                _gridLengths[key] = convertedLength;
            }
            else
            {
                _gridLengths.Add(key, convertedLength);
            }
        }

        /// <summary>
        /// Returns true if a length has been stored for a given key
        /// </summary>
        /// <param name="key">Unique key used to store and restore the length</param>
        /// <returns>True if a length has been stored</returns>
        public bool HaveGridLength(string key)
        {
            if (key == null) { throw new ArgumentNullException("key"); }

            return _gridLengths.ContainsKey(key);
        }

        /// <summary>
        /// Returns a stored GridLength (call HaveGridLength first to detemine if a stored value exists)
        /// </summary>
        /// <param name="key">Unique key used to store and restore the length</param>
        /// <returns>The GridLength</returns>
        public GridLength RestoreGridLength(string key)
        {
            if (key == null) { throw new ArgumentNullException("key"); }

            return (GridLength)_gridLengthConverter.ConvertFromInvariantString(_gridLengths[key]);
        }

        /// <summary>
        /// Saves a ToolBar
        /// </summary>
        /// <param name="key">Unique key used to store and restore the toolbar</param>
        /// <param name="toolBar">The ToolBar</param>
        public void SaveToolBar(string key, ToolBar toolBar)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (toolBar == null) { throw new ArgumentNullException("toolBar"); }

            ToolBarInfo info = new ToolBarInfo(key, toolBar.Band, toolBar.BandIndex, toolBar.Width);

            if (_toolbars.ContainsKey(key))
            {
                _toolbars[key] = info;
            }
            else
            {
                _toolbars.Add(key, info);
            }
        }

        /// <summary>
        /// Restores a ToolBar
        /// </summary>
        /// <param name="key">Unique key used to store and restore the toolbar</param>
        /// <param name="toolBar">The ToolBar</param>
        public void RestoreToolBar(string key, ToolBar toolBar)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (toolBar == null) { throw new ArgumentNullException("toolBar"); }

            if (_toolbars.ContainsKey(key))
            {
                toolBar.Band = _toolbars[key].Band;
                toolBar.BandIndex = _toolbars[key].BandIndex;
                toolBar.Width = _toolbars[key].Width;
            }
        }

        /// <summary>
        /// Saves a window
        /// </summary>
        /// <param name="key">Unique key used to store and restore the window</param>
        /// <param name="window">The Window</param>
        public void SaveWindow(string key, Window window)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (window == null) { throw new ArgumentNullException("window"); }

            WindowInfo windowInfo = new WindowInfo(key, window.Top, window.Left, window.Height, window.Width, window.WindowState);

            if (_windows.ContainsKey(key))
            {
                _windows[key] = windowInfo;
            }
            else
            {
                _windows.Add(key, windowInfo);
            }
        }

        /// <summary>
        /// Resores a window
        /// </summary>
        /// <param name="key">Unique key used to store and restore the window</param>
        /// <param name="window">The Window</param>
        public void RestoreWindow(string key, Window window)
        {
            if (key == null) { throw new ArgumentNullException("key"); }
            if (window == null) { throw new ArgumentNullException("window"); }

            if (_windows.ContainsKey(key))
            {
                if ((_windows[key].Width >= window.MinWidth) && (_windows[key].Height >= window.MinHeight))
                {
                    window.Top = _windows[key].Top;
                    window.Left = _windows[key].Left;
                    window.Height = _windows[key].Height;
                    window.Width = _windows[key].Width;

                    // don't restore to minimized
                    if (_windows[key].State == WindowState.Maximized)
                    {
                        window.WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        window.WindowState = WindowState.Normal;
                    }

                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }
            }
        }

        /// <summary>
        /// Returns true if a window has been stored for a given key
        /// </summary>
        /// <param name="key">Unique key used to store and restore the window</param>
        /// <returns>True if a windows has been stored for key</returns>
        public bool HaveWindow(string key)
        {
            if (key == null) { throw new ArgumentNullException("key"); }

            return _windows.ContainsKey(key);
        }

        /// <summary>
        /// Adds a search
        /// </summary>
        /// <param name="search">Search string to add</param>
        public void AddSearch(string search)
        {
            if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

            Debug.Assert(!string.IsNullOrEmpty(search));

            if (SearchBuilder.IsSavedSearch(search))
            {
                AddSavedSearch(search);
            }
            else
            {
                AddRecentSearch(search);
            }

            BuildSearchList();
        }

        private void AddRecentSearch(string search)
        {
            Debug.Assert(!string.IsNullOrEmpty(search));

            if (_recentSearches.Contains(search))
            {
                _recentSearches.Remove(search);
            }

            _recentSearches.Insert(0, search);

            while (_recentSearches.Count > RecentSearchListLength)
            {
                _recentSearches.RemoveAt(_recentSearches.Count - 1);
            }
        }

        private void AddSavedSearch(string search)
        {
            Debug.Assert(!string.IsNullOrEmpty(search));

            string searchName;
            string searchSearch;
            SearchBuilder.CrackSavedSearch(search, out searchName, out searchSearch);

            Debug.Assert(!string.IsNullOrEmpty(searchName));
            Debug.Assert(!string.IsNullOrEmpty(searchSearch));

            bool searchUpdated = false;
            if (_savedSearches.Count > 0)
            {
                foreach (SavedSearch savedSearch in _savedSearches)
                {
                    if (string.Compare(savedSearch.Name, searchName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        savedSearch.Search = searchSearch;
                        searchUpdated = true;
                        break;
                    }
                }
            }

            if (!searchUpdated)
            {
                _savedSearches.Add(new SavedSearch(searchName, searchSearch));
            }
        }

        /// <summary>
        /// Removes a search
        /// </summary>
        /// <param name="search">Search string to remove</param>
        public void RemoveSearch(string search)
        {
            if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

            Debug.Assert(!string.IsNullOrEmpty(search));

            if (SearchBuilder.IsSavedSearch(search))
            {
                RemoveSavedSearch(search);
            }
            else
            {
                RemoveRecentSearch(search);
            }

            BuildSearchList();
        }

        private void RemoveRecentSearch(string search)
        {
            Debug.Assert(!string.IsNullOrEmpty(search));

            if (_recentSearches.Contains(search))
            {
                _recentSearches.Remove(search);
            }
        }

        private void RemoveSavedSearch(string search)
        {
            Debug.Assert(!string.IsNullOrEmpty(search));

            string searchName;
            string searchSearch;
            SearchBuilder.CrackSavedSearch(search, out searchName, out searchSearch);

            Debug.Assert(!string.IsNullOrEmpty(searchName));
            Debug.Assert(!string.IsNullOrEmpty(searchSearch));

            SavedSearch toRemove = new SavedSearch(searchName, searchSearch);
            if (_savedSearches.Contains(toRemove))
            {
                _savedSearches.Remove(toRemove);
            }
        }

        /// <summary>
        /// Generates a StackHashClientData object for use in a service call
        /// </summary>
        /// <returns>StackHashClientData object</returns>
        public StackHashClientData GenerateClientData()
        {
            if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

            _rwLock.EnterWriteLock();
            try
            {
                // increment the request Id
                if (_requestId == Int32.MaxValue)
                {
                    _requestId = 0;
                }
                else
                {
                    _requestId++;
                }

                // create and return client data
                StackHashClientData clientData = new StackHashClientData();
                clientData.ApplicationGuid = _sessionId;
                clientData.ClientId = 0;
                clientData.ClientName = string.Format(CultureInfo.CurrentCulture,
                    "{0}\\{1}",
                    Environment.MachineName,
                    Environment.UserName);
                clientData.ClientRequestId = _requestId;
                clientData.ServiceGuid = _localServiceGuid == Guid.Empty ? null : _localServiceGuid.ToString();

                return clientData;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the display filter hit threshold for a product, or the default if
        /// no specific threshold has been set
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Hit threshold</returns>
        public int GetDisplayHitThreshold(int productId)
        {
            int hitThreshold;

            _rwLock.EnterReadLock();
            try
            {
                if (_displayFilters.ContainsKey(productId))
                {
                    hitThreshold = _displayFilters[productId];
                }
                else
                {
                    hitThreshold = _displayFilters[DefaultDisplayFilterProductId];
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            return hitThreshold;
        }

        /// <summary>
        /// Sets the display filter hit threshold for a product. To set the default
        /// threshold use UserSettings.DefaultDisplayFilterProductId as the productId
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="hitThreshold">Hit threshold</param>
        public void SetDisplayHitThreshold(int productId, int hitThreshold)
        {
            if (hitThreshold < 0) { throw new InvalidOperationException("hitThreshold must be greater than or equal to 0"); }

            _rwLock.EnterWriteLock();
            try
            {
                if (_displayFilters.ContainsKey(productId))
                {
                    _displayFilters[productId] = hitThreshold;
                }
                else
                {
                    _displayFilters.Add(productId, hitThreshold);
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a product display filter if one is present.
        /// </summary>
        /// <param name="productId">Product Id to remove</param>
        public void RemoveDisplayHitThreshold(int productId)
        {
            if (productId != DefaultDisplayFilterProductId)
            {
                _rwLock.EnterWriteLock();
                try
                {
                    if (_displayFilters.ContainsKey(productId))
                    {
                        _displayFilters.Remove(productId);
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Determines if a product has an overriden display hit threshold
        /// </summary>
        /// <param name="productId">Product Id to check</param>
        /// <returns>True if the product has an overriden display hit threshold</returns>
        public bool HasOverrideHitThreshold(int productId)
        {
            bool ret = false;

            if (productId != DefaultDisplayFilterProductId)
            {
                _rwLock.EnterWriteLock();
                try
                {
                    ret = _displayFilters.ContainsKey(productId);
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            }

            return ret;
        }

        /// <summary>
        /// Load/reload user settings
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Load()
        {
            if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

            try
            {
                // force trace on if the trigger file exists
                _forceTrace = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), DesktopLogTriggerFile));

                bool resetWindow = false;

                // check for demo mode
                string[] args = Environment.GetCommandLineArgs();
                if (args.Contains(DemoSwitch))
                {
                    this.DemoMode = true;
                }
                if (args.Contains(ResetWindowSwitch))
                {
                    // don't load window layout
                    resetWindow = true;
                }

                // note, resetWindow also set if virtual screen dimensions have changed

                InitializeSettingsPath();
                Debug.Assert(_settingsPath != null);

                if (File.Exists(_settingsPath))
                {
                    InitializeSettings();

                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.CheckCharacters = true;
                    readerSettings.CloseInput = true;
                    readerSettings.ConformanceLevel = ConformanceLevel.Document;
                    readerSettings.IgnoreComments = true;
                    readerSettings.IgnoreWhitespace = true;

                    _rwLock.EnterWriteLock();
                    try
                    {
                        string savedSearchName = null;
                        string savedSearchSearch = null;
                        GridViewInfo gridViewInfo = null;

                        using (XmlReader reader = XmlReader.Create(_settingsPath))
                        {
                            while (reader.Read())
                            {
                                switch (reader.Name)
                                {
                                    case ElementCurrentContextId:
                                        try
                                        {
                                            _currentContextId = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _currentContextId = InvalidContextId;
                                        }
                                        break;

                                    case ElementEventPageSize:
                                        try
                                        {
                                            _eventPageSize = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _eventPageSize = DefaultEventPageSize;
                                        }
                                        break;

                                    case ElementHitScaleProduct:
                                        try
                                        {
                                            _hitScaleProduct = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _hitScaleProduct = 0;
                                        }
                                        break;

                                    case ElementHitScaleEvent:
                                        try
                                        {
                                            _hitScaleEvent = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _hitScaleEvent = 0;
                                        }
                                        break;

                                    case ElementDebuggerPathX86:
                                        _debuggerPathX86 = reader.ReadString();
                                        break;

                                    case ElementDebuggerPathAmd64:
                                        _debuggerPathAmd64 = reader.ReadString();
                                        break;

                                    case ElementDebuggerPathVisualStudio:
                                        _debuggerPathVisualStudio = reader.ReadString();
                                        break;

                                    case ElementSavedSearch:
                                        if (reader.IsStartElement())
                                        {
                                            // reset
                                            savedSearchName = null;
                                            savedSearchSearch = null;
                                        }
                                        else
                                        {
                                            if ((!string.IsNullOrEmpty(savedSearchName)) &&
                                                (!string.IsNullOrEmpty(savedSearchSearch)))
                                            {
                                                _savedSearches.Add(new SavedSearch(savedSearchName, savedSearchSearch));
                                            }
                                        }
                                        break;

                                    case ElementSavedSearchName:
                                        savedSearchName = reader.ReadString();
                                        break;

                                    case ElementSavedSearchSearch:
                                        savedSearchSearch = reader.ReadString();
                                        break;

                                    case ElementRecentSearch:
                                        _recentSearches.Add(reader.ReadString());
                                        break;

                                    case ElementSuppressedMessage:
                                        _suppressedMessages.Add(reader.ReadString());
                                        break;

                                    case ElementInstanceId:
                                        try
                                        {
                                            _instanceId = new Guid(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _instanceId = Guid.Empty;
                                        }
                                        break;

                                    case ElementRequestId:
                                        try
                                        {
                                            _requestId = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _requestId = 0;
                                        }
                                        break;

                                    case ElementDiagnosticLogEnabled:
                                        try
                                        {
                                            _diagnosticLogEnabled = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _diagnosticLogEnabled = false;
                                        }
                                        break;

                                    case ElementFirstConfigComplete:
                                        try
                                        {
                                            _firstConfigComplete = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _firstConfigComplete = false;
                                        }
                                        break;

                                    case ElementShowDisabledProducts:
                                        try
                                        {
                                            _showDisabledProducts = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _showDisabledProducts = true;
                                        }
                                        break;

                                    case ElementShowEventsWithoutCabs:
                                        try
                                        {
                                            _showEventsWithoutCabs = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _showEventsWithoutCabs = true;
                                        }
                                        break;

                                    case ElementServiceHost:
                                        _serviceHost = reader.ReadString();
                                        break;

                                    case ElementServicePort:
                                        try
                                        {
                                            _servicePort = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _servicePort = DefaultServicePort;
                                        }
                                        break;

                                    case ElementServiceDomain:
                                        try
                                        {
                                            _serviceDomain = ClientUtils.DecryptString(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _serviceDomain = null;
                                        }
                                        break;

                                    case ElementServiceUsername:
                                        try
                                        {
                                            _serviceUsername = ClientUtils.DecryptString(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _serviceUsername = null;
                                        }
                                        break;

                                    case ElementServicePassword:
                                        try
                                        {
                                            _servicePassword = ClientUtils.DecryptString(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _servicePassword = null;
                                        }
                                        break;

                                    case ElementProxyDomain:
                                        try
                                        {
                                            _proxyDomain = ClientUtils.DecryptString(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _proxyDomain = null;
                                        }
                                        break;

                                    case ElementProxyPassword:
                                        try
                                        {
                                            _proxyPassword = ClientUtils.DecryptString(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _proxyPassword = null;
                                        }
                                        break;

                                    case ElementProxyUsername:
                                        try
                                        {
                                            _proxyUsername = ClientUtils.DecryptString(reader.ReadString());
                                        }
                                        catch
                                        {
                                            _proxyUsername = null;
                                        }
                                        break;

                                    case ElementProxyHost:
                                        try
                                        {
                                            _proxyHost = reader.ReadString();
                                        }
                                        catch
                                        {
                                            _proxyHost = null;
                                        }
                                        break;

                                    case ElementProxyPort:
                                        try
                                        {
                                            _proxyPort = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _proxyPort = 8080;
                                        }
                                        break;

                                    case ElementUseProxyServer:
                                        try
                                        {
                                            _useProxyServer = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _useProxyServer = false;
                                        }
                                        break;

                                    case ElementUseProxyServerAuthentication:
                                        try
                                        {
                                            _useProxyServerAuthentication = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            _useProxyServerAuthentication = false;
                                        }
                                        break;

                                    case ElementDefaultDebugTool:
                                        try
                                        {
                                            _defaultDebugTool = (DefaultDebugger)Enum.Parse(typeof(DefaultDebugger), reader.ReadString(), true);
                                        }
                                        catch
                                        {
                                            _defaultDebugTool = DefaultDebugger.DebuggingToolsForWindows;
                                        }
                                        break;

                                    case ElementVirtualScreen:
                                        try
                                        {
                                            if (reader.IsStartElement())
                                            {
                                                _virtualScreenHeight = Convert.ToDouble(reader.GetAttribute(AttributeHeight), CultureInfo.InvariantCulture);
                                                _virtualScreenLeft = Convert.ToDouble(reader.GetAttribute(AttributeLeft), CultureInfo.InvariantCulture);
                                                _virtualScreenTop = Convert.ToDouble(reader.GetAttribute(AttributeTop), CultureInfo.InvariantCulture);
                                                _virtualScreenWidth = Convert.ToDouble(reader.GetAttribute(AttributeWidth), CultureInfo.InvariantCulture);

                                                if ((_virtualScreenHeight != SystemParameters.VirtualScreenHeight) ||
                                                    (_virtualScreenLeft != SystemParameters.VirtualScreenLeft) ||
                                                    (_virtualScreenTop != SystemParameters.VirtualScreenTop) ||
                                                    (_virtualScreenWidth != SystemParameters.VirtualScreenWidth))
                                                {
                                                    // screen(s) have changed size, reset the window
                                                    resetWindow = true;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            _virtualScreenHeight = 0.0;
                                            _virtualScreenLeft = 0.0;
                                            _virtualScreenTop = 0.0;
                                            _virtualScreenWidth = 0.0;

                                            resetWindow = true;
                                        }
                                        break;

                                    case ElementGridView:
                                        if (!resetWindow)
                                        {
                                            try
                                            {
                                                if (reader.IsStartElement())
                                                {
                                                    // create GridViewInfo to store columns
                                                    gridViewInfo = new GridViewInfo(reader.GetAttribute(AttributeKey));
                                                }
                                                else
                                                {
                                                    // add GridViewInfo (with loaded columns)
                                                    if (gridViewInfo != null)
                                                    {
                                                        _gridViews.Add(gridViewInfo.Key, gridViewInfo);
                                                        gridViewInfo = null;
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        break;

                                    case ElementGridViewColumn:
                                        if (!resetWindow)
                                        {
                                            try
                                            {
                                                if (reader.IsStartElement() && (gridViewInfo != null))
                                                {
                                                    gridViewInfo.ColumnInfos.Add(new GridViewColumnInfo(Convert.ToInt32(reader.GetAttribute(AttributeIndex), CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(reader.GetAttribute(AttributeWidth), CultureInfo.InvariantCulture),
                                                        reader.GetAttribute(AttributeHeader)));
                                                }
                                            }
                                            catch { }
                                        }
                                        break;

                                    case ElementGridLength:
                                        if (!resetWindow)
                                        {
                                            try
                                            {
                                                if (reader.IsStartElement())
                                                {
                                                    _gridLengths.Add(reader.GetAttribute(AttributeKey), reader.GetAttribute(AttributeLength));
                                                }
                                            }
                                            catch { }
                                        }
                                        break;

                                    case ElementToolbar:
                                        if (!resetWindow)
                                        {
                                            try
                                            {
                                                if (reader.IsStartElement())
                                                {
                                                    string key = reader.GetAttribute(AttributeKey);
                                                    _toolbars.Add(key, new ToolBarInfo(key,
                                                        Convert.ToInt32(reader.GetAttribute(AttributeBand), CultureInfo.InvariantCulture),
                                                        Convert.ToInt32(reader.GetAttribute(AttributeBandIndex), CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(reader.GetAttribute(AttributeWidth), CultureInfo.InvariantCulture)));
                                                }
                                            }
                                            catch { }
                                        }
                                        break;

                                    case ElementWindow:
                                        if (!resetWindow)
                                        {
                                            try
                                            {
                                                if (reader.IsStartElement())
                                                {
                                                    string key = reader.GetAttribute(AttributeKey);
                                                    _windows.Add(key, new WindowInfo(key,
                                                        Convert.ToDouble(reader.GetAttribute(AttributeTop), CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(reader.GetAttribute(AttributeLeft), CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(reader.GetAttribute(AttributeHeight), CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(reader.GetAttribute(AttributeWidth), CultureInfo.InvariantCulture),
                                                        (WindowState)Enum.Parse(typeof(WindowState), reader.GetAttribute(AttributeState))));
                                                }
                                            }
                                            catch { }
                                        }
                                        break;

                                    case ElementDisplayFilter:
                                        try
                                        {
                                            if (reader.IsStartElement())
                                            {
                                                int productId = Convert.ToInt32(reader.GetAttribute(AttributeProduct), CultureInfo.InvariantCulture);
                                                int hits = Convert.ToInt32(reader.GetAttribute(AttributeHits), CultureInfo.InvariantCulture);

                                                if (_displayFilters.ContainsKey(productId))
                                                {
                                                    _displayFilters[productId] = hits;
                                                }
                                                else
                                                {
                                                    _displayFilters.Add(productId, hits);
                                                }
                                            }
                                        }
                                        catch { }
                                        break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }

                    // build the search list
                    BuildSearchList();

                    // update the proxy
                    ServiceProxy.Services.UpdateServiceEndpointAndAccount(this.ServiceHost, 
                        this.ServicePort,
                        this.ServiceUsername,
                        this.ServicePassword,
                        this.ServiceDomain);

                    // notify that properties have changed after the lock is released
                    RaiseAllPropertiesChanged();

                    // update tracing (diagnostic log)
                    UpdateTrace();
                }

                // create instance Id if necessary
                if (_instanceId == Guid.Empty)
                {
                    _instanceId = Guid.NewGuid();
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal,
                    "UserSettings.Load Failed",
                    ex);
            }
        }

        /// <summary>
        /// Save user settings
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Save()
        {
            Exception finalException = null;

            for (int retry = 0; retry < SaveRetryCount; retry++)
            {
                try
                {
                    finalException = null;

                    if (retry > 0)
                    {
                        // wait a little bit before trying again
                        Thread.Sleep(SaveRetryWaitMs);

                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning,
                            string.Format(CultureInfo.InvariantCulture,
                            "UserSettings.Save Retry attempt {0}",
                            retry));
                    }

                    SaveCore();
                    break;
                }
                catch (Exception ex)
                {
                    finalException = ex;

                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "UserSettings.Save Failed",
                        ex);
                }
            }

            if (finalException != null)
            {
                throw finalException;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void SaveCore()
        {
            if (_disposed) throw new ObjectDisposedException("StackHash.UserSettings");

            InitializeSettingsPath();
            Debug.Assert(_settingsPath != null);

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.CheckCharacters = true;
            writerSettings.CloseOutput = true;
            writerSettings.ConformanceLevel = ConformanceLevel.Document;
            writerSettings.Indent = true;

            _rwLock.EnterReadLock();
            try
            {
                // store the screen size
                _virtualScreenHeight = SystemParameters.VirtualScreenHeight;
                _virtualScreenLeft = SystemParameters.VirtualScreenLeft;
                _virtualScreenTop = SystemParameters.VirtualScreenTop;
                _virtualScreenWidth = SystemParameters.VirtualScreenWidth;

                using (XmlWriter writer = XmlWriter.Create(_settingsPath, writerSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement(ElementSettings);

                    // CurrentContextId
                    writer.WriteStartElement(ElementCurrentContextId);
                    writer.WriteValue(_currentContextId);
                    writer.WriteEndElement();

                    // DebuggerPathAMD64
                    if (!string.IsNullOrEmpty(_debuggerPathAmd64))
                    {
                        writer.WriteStartElement(ElementDebuggerPathAmd64);
                        writer.WriteString(_debuggerPathAmd64);
                        writer.WriteEndElement();
                    }

                    // DebuggerPathVisualStudio
                    if (!string.IsNullOrEmpty(_debuggerPathVisualStudio))
                    {
                        writer.WriteStartElement(ElementDebuggerPathVisualStudio);
                        writer.WriteString(_debuggerPathVisualStudio);
                        writer.WriteEndElement();
                    }

                    // DebuggerPathX86
                    if (!string.IsNullOrEmpty(_debuggerPathX86))
                    {
                        writer.WriteStartElement(ElementDebuggerPathX86);
                        writer.WriteString(_debuggerPathX86);
                        writer.WriteEndElement();
                    }

                    // DefaultDebugTool
                    writer.WriteStartElement(ElementDefaultDebugTool);
                    writer.WriteString(_defaultDebugTool.ToString());
                    writer.WriteEndElement();

                    // DiagnosticLogEnabled
                    writer.WriteStartElement(ElementDiagnosticLogEnabled);
                    writer.WriteValue(_diagnosticLogEnabled);
                    writer.WriteEndElement();

                    // EventPageSize
                    writer.WriteStartElement(ElementEventPageSize);
                    writer.WriteValue(_eventPageSize);
                    writer.WriteEndElement();

                    // FirstConfigComplete
                    writer.WriteStartElement(ElementFirstConfigComplete);
                    writer.WriteValue(_firstConfigComplete);
                    writer.WriteEndElement();

                    // HitScaleEvent
                    writer.WriteStartElement(ElementHitScaleEvent);
                    writer.WriteValue(_hitScaleEvent);
                    writer.WriteEndElement();

                    // HitScaleProduct
                    writer.WriteStartElement(ElementHitScaleProduct);
                    writer.WriteValue(_hitScaleProduct);
                    writer.WriteEndElement();

                    // InstanceID
                    writer.WriteStartElement(ElementInstanceId);
                    writer.WriteString(_instanceId.ToString());
                    writer.WriteEndElement();

                    // ProxyDomain
                    if (!string.IsNullOrEmpty(_proxyDomain))
                    {
                        writer.WriteStartElement(ElementProxyDomain);
                        writer.WriteString(ClientUtils.EncryptString(_proxyDomain));
                        writer.WriteEndElement();
                    }

                    // ProxyHost
                    if (!string.IsNullOrEmpty(_proxyHost))
                    {
                        writer.WriteStartElement(ElementProxyHost);
                        writer.WriteCData(_proxyHost);
                        writer.WriteEndElement();
                    }

                    // ProxyPassword
                    if (!string.IsNullOrEmpty(_proxyPassword))
                    {
                        writer.WriteStartElement(ElementProxyPassword);
                        writer.WriteString(ClientUtils.EncryptString(_proxyPassword));
                        writer.WriteEndElement();
                    }

                    // ProxyPort
                    writer.WriteStartElement(ElementProxyPort);
                    writer.WriteValue(_proxyPort);
                    writer.WriteEndElement();

                    // ProxyUsername
                    if (!string.IsNullOrEmpty(_proxyUsername))
                    {
                        writer.WriteStartElement(ElementProxyUsername);
                        writer.WriteString(ClientUtils.EncryptString(_proxyUsername));
                        writer.WriteEndElement();
                    }

                    // Recent searches
                    if (_recentSearches.Count > 0)
                    {
                        writer.WriteStartElement(ElementRecentSearchList);

                        foreach (string search in _recentSearches)
                        {
                            writer.WriteStartElement(ElementRecentSearch);
                            writer.WriteCData(search);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    // RequestID
                    writer.WriteStartElement(ElementRequestId);
                    writer.WriteValue(_requestId);
                    writer.WriteEndElement();

                    // Saved searches
                    if (_savedSearches.Count > 0)
                    {
                        writer.WriteStartElement(ElementSavedSearchList);

                        foreach (SavedSearch savedSearch in _savedSearches)
                        {
                            writer.WriteStartElement(ElementSavedSearch);

                            writer.WriteStartElement(ElementSavedSearchName);
                            writer.WriteCData(savedSearch.Name);
                            writer.WriteEndElement();

                            writer.WriteStartElement(ElementSavedSearchSearch);
                            writer.WriteCData(savedSearch.Search);
                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    // ServiceDomain
                    if (!string.IsNullOrEmpty(_serviceDomain))
                    {
                        writer.WriteStartElement(ElementServiceDomain);
                        writer.WriteString(ClientUtils.EncryptString(_serviceDomain));
                        writer.WriteEndElement();
                    }

                    // ServiceHost
                    if (!string.IsNullOrEmpty(_serviceHost))
                    {
                        writer.WriteStartElement(ElementServiceHost);
                        writer.WriteCData(_serviceHost);
                        writer.WriteEndElement();
                    }

                    // ServicePassword
                    if (!string.IsNullOrEmpty(_servicePassword))
                    {
                        writer.WriteStartElement(ElementServicePassword);
                        writer.WriteString(ClientUtils.EncryptString(_servicePassword));
                        writer.WriteEndElement();
                    }

                    // ServicePort
                    writer.WriteStartElement(ElementServicePort);
                    writer.WriteValue(_servicePort);
                    writer.WriteEndElement();

                    // ServiceUsername
                    if (!string.IsNullOrEmpty(_serviceUsername))
                    {
                        writer.WriteStartElement(ElementServiceUsername);
                        writer.WriteString(ClientUtils.EncryptString(_serviceUsername));
                        writer.WriteEndElement();
                    }

                    // ShowDisabledProducts
                    writer.WriteStartElement(ElementShowDisabledProducts);
                    writer.WriteValue(_showDisabledProducts);
                    writer.WriteEndElement();

                    // ShowEventsWithoutCabs
                    writer.WriteStartElement(ElementShowEventsWithoutCabs);
                    writer.WriteValue(_showEventsWithoutCabs);
                    writer.WriteEndElement();

                    // Supressed Messages
                    if (_suppressedMessages.Count > 0)
                    {
                        writer.WriteStartElement(ElementSuppressedMessages);

                        foreach (string message in _suppressedMessages)
                        {
                            writer.WriteStartElement(ElementSuppressedMessage);
                            writer.WriteString(message);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    // UseProxyServer
                    writer.WriteStartElement(ElementUseProxyServer);
                    writer.WriteValue(_useProxyServer);
                    writer.WriteEndElement();

                    // UseProxyServerAuthentication
                    writer.WriteStartElement(ElementUseProxyServerAuthentication);
                    writer.WriteValue(_useProxyServerAuthentication);
                    writer.WriteEndElement();

                    // VirtualScreen
                    // IMPORTANT - must be saved before window / element persistence 
                    writer.WriteStartElement(ElementVirtualScreen);
                    writer.WriteAttributeString(AttributeTop, _virtualScreenTop.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString(AttributeLeft, _virtualScreenLeft.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString(AttributeHeight, _virtualScreenHeight.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString(AttributeWidth, _virtualScreenWidth.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();

                    // GridViews
                    writer.WriteStartElement(ElementGridViews);
                    foreach (GridViewInfo info in _gridViews.Values)
                    {
                        writer.WriteStartElement(ElementGridView);
                        writer.WriteAttributeString(AttributeKey, info.Key);
                        foreach (GridViewColumnInfo columnInfo in info.ColumnInfos)
                        {
                            writer.WriteStartElement(ElementGridViewColumn);
                            writer.WriteAttributeString(AttributeIndex, columnInfo.Index.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString(AttributeWidth, columnInfo.Width.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString(AttributeHeader, columnInfo.Header);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // GridLengths
                    writer.WriteStartElement(ElementGridLengths);
                    foreach (string key in _gridLengths.Keys)
                    {
                        writer.WriteStartElement(ElementGridLength);
                        writer.WriteAttributeString(AttributeKey, key);
                        writer.WriteAttributeString(AttributeLength, _gridLengths[key]);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Toolbars
                    writer.WriteStartElement(ElementToolbars);
                    foreach (ToolBarInfo toolbar in _toolbars.Values)
                    {
                        writer.WriteStartElement(ElementToolbar);
                        writer.WriteAttributeString(AttributeKey, toolbar.Key);
                        writer.WriteAttributeString(AttributeBand, toolbar.Band.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeBandIndex, toolbar.BandIndex.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeWidth, toolbar.Width.ToString(CultureInfo.InvariantCulture));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Windows
                    writer.WriteStartElement(ElementWindows);
                    foreach (WindowInfo window in _windows.Values)
                    {
                        writer.WriteStartElement(ElementWindow);
                        writer.WriteAttributeString(AttributeKey, window.Key);
                        writer.WriteAttributeString(AttributeTop, window.Top.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeLeft, window.Left.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeHeight, window.Height.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeWidth, window.Width.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeState, window.State.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // DisplayFilters
                    writer.WriteStartElement(ElementDisplayFilters);
                    foreach (int productId in _displayFilters.Keys)
                    {
                        writer.WriteStartElement(ElementDisplayFilter);
                        writer.WriteAttributeString(AttributeProduct, productId.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString(AttributeHits, _displayFilters[productId].ToString(CultureInfo.InvariantCulture));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            finally
            {                
                _rwLock.ExitReadLock();

                // update the proxy
                ServiceProxy.Services.UpdateServiceEndpointAndAccount(this.ServiceHost, 
                    this.ServicePort, 
                    this.ServiceUsername, 
                    this.ServicePassword, 
                    this.ServiceDomain);
            }
        }

        private void RaiseAllPropertiesChanged()
        {
            RaisePropertyChanged("CurrentContextId");
            RaisePropertyChanged("DebuggerPathX86");
            RaisePropertyChanged("DebuggerPathAMD64");
            RaisePropertyChanged("SearchList");
            RaisePropertyChanged("DiagnosticLogEnabled");
            RaisePropertyChanged("ServiceHost");
            RaisePropertyChanged("ServicePort");
            RaisePropertyChanged("NextUpdateCheck");
            RaisePropertyChanged("FirstConfigComplete");
            RaisePropertyChanged("ShowDisabledProducts");
            RaisePropertyChanged("UseProxyServer");
            RaisePropertyChanged("UseProxyServerAuthentication");
            RaisePropertyChanged("ProxyHost");
            RaisePropertyChanged("ProxyPort");
            RaisePropertyChanged("ProxyUsername");
            RaisePropertyChanged("ProxyPassword");
            RaisePropertyChanged("ProxyDomain");
            RaisePropertyChanged("DebuggerPathVisualStudio");
            RaisePropertyChanged("DefaultDebugTool");
            RaisePropertyChanged("HitScaleProduct");
            RaisePropertyChanged("HitScaleEvent");
            RaisePropertyChanged("EventPageSize");
            RaisePropertyChanged("WebServiceOverrideAddress");
            RaisePropertyChanged("ClientIsLocal");
            RaisePropertyChanged("LocalServiceGuid");
            RaisePropertyChanged("ServiceUsername");
            RaisePropertyChanged("ServicePassword");
            RaisePropertyChanged("ServiceDomain");
            RaisePropertyChanged("ShowEventsWithoutCabs");
        }

        private void BuildSearchList()
        {
            if ((_recentSearches.Count > 0) || (_savedSearches.Count > 0))
            {
                List<string> newList = new List<string>(_recentSearches.Count + _savedSearches.Count);

                // sort saved searches
                _savedSearches.Sort();

                // add saved searches and then recent searches
                foreach (SavedSearch savedSearch in _savedSearches)
                {
                    newList.Add(SearchBuilder.CombineSavedSearch(savedSearch.Name, savedSearch.Search));
                }

                newList.AddRange(_recentSearches);

                this.SearchList = new ReadOnlyCollection<string>(newList);
            }
            else
            {
                // no searches
                this.SearchList = null;
            }
        }

        private void InitializeSettingsPath()
        {
            if (string.IsNullOrEmpty(_settingsPath))
            {
                string settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    SettingsFolder);

                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }

                _settingsPath = Path.Combine(settingsFolder, SettingsFile);
            }
        }

        private void InitializeSettings()
        {
            _currentContextId = InvalidContextId;
            _serviceHost = DefaultServiceHost;
            _servicePort = DefaultServicePort;
            _showDisabledProducts = true;
            _proxyPort = DefaultProxyServerPort;
            _proxyHost = string.Empty;
            _proxyUsername = string.Empty;
            _proxyPassword = string.Empty;
            _proxyDomain = string.Empty;
            _hitScaleProduct = 0;
            _hitScaleEvent = 0;
            _eventPageSize = DefaultEventPageSize;
            _showEventsWithoutCabs = true;

            _recentSearches.Clear();
            _savedSearches.Clear();
            _gridViews.Clear();
            _gridLengths.Clear();
            _toolbars.Clear();
            _windows.Clear();
            _displayFilters.Clear();
            _suppressedMessages.Clear();

            // default is to 0 hits - i.e. show all events
            _displayFilters.Add(DefaultDisplayFilterProductId, 0);
        }

        private void UpdateTrace()
        {
            if ((this.DiagnosticLogEnabled) || _forceTrace)
            {
                StartTrace();
            }
            else
            {
                EndTrace();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void StartTrace()
        {
            try
            {
                if (_listener == null)
                {
                    _listener = new TextWriterTraceListener(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), TraceFile), TraceName);
                    Trace.Listeners.Add(_listener);
                }
            }
            catch { }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void EndTrace()
        {
            try
            {
                if (_listener != null)
                {
                    Trace.Listeners.Remove(_listener);
                    _listener.Dispose();
                    _listener = null;
                }
            }
            catch { }
        }

        private UserSettings()
        {
            // default constructor does not permit recursive locking
            _rwLock = new ReaderWriterLockSlim();
            _recentSearches = new List<string>();
            _savedSearches = new List<SavedSearch>();
            _gridViews = new Dictionary<string, GridViewInfo>();
            _gridLengths = new Dictionary<string, string>();
            _gridLengthConverter = new GridLengthConverter();
            _toolbars = new Dictionary<string, ToolBarInfo>();
            _windows = new Dictionary<string, WindowInfo>();
            _suppressedMessages = new List<string>();
            _sessionId = Guid.NewGuid();
            _displayFilters = new Dictionary<int, int>();
            _serviceUsername = null;
            _servicePassword = null;
            _serviceDomain = null;

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            _copyright = fvi.LegalCopyright;
            _version = string.Format(CultureInfo.CurrentCulture,
                "{0}.{1:00}.{2}.{3}",
                fvi.ProductMajorPart,
                fvi.ProductMinorPart,
                fvi.ProductBuildPart,
                fvi.ProductPrivatePart);

            _versionMajor = fvi.ProductMajorPart;
            _versionMinor = fvi.ProductMinorPart;

            InitializeSettings();
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose UserSettings
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool canDisposeManagedResources)
        {
            if (!_disposed)
            {
                if (canDisposeManagedResources)
                {
                    if (_clientLogic != null)
                    {
                        if (!_clientLogic.IsDisposed)
                        {
                            _clientLogic.Dispose();
                        }
                        _clientLogic = null;
                    }

                    if (_rwLock != null)
                    {
                        _rwLock.Dispose();
                        _rwLock = null;
                    }

                    if (_listener != null)
                    {
                        _listener.Dispose();
                        _listener = null;
                    }
                }
            }
            _disposed = true;
        }

        /// <summary />
        ~UserSettings()
        {
            Dispose(false);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event fired when a proprty changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
