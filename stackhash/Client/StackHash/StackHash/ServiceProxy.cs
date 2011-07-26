using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHash.StackHashService;
using System.ServiceModel.Description;
using System.Diagnostics;
using System.ServiceModel;
using System.Globalization;
using StackHashUtilities;
using System.Net;

namespace StackHash
{
    /// <summary>
    /// Parameters pass to the AdminReport delegate.
    /// </summary>
    public class AdminReportEventArgs : EventArgs
    {
        private StackHashAdminReport _report;

        /// <summary>
        /// Parameters pass to the AdminReport delegate.
        /// </summary>
        /// <param name="report">The report</param>
        public AdminReportEventArgs(StackHashAdminReport report)
        {
            _report = report;
        }

        /// <summary>
        /// Report containing information about the async service event.
        /// </summary>
        public StackHashAdminReport Report
        {
            get { return _report; }
        }
    }

    /// <summary>
    /// Manages the client's connection to the StackHash service
    /// </summary>
    public class ServiceProxy : IDisposable, IAdminContractCallback
    {
        private static readonly ServiceProxy _services = new ServiceProxy();

        private const string AdminServiceDir = "Admin";
        private const string AdminConfigName = "NetTcpBinding_IAdminContract";
        private const string ProjectsServiceDir = "Projects";
        private const string ProjectsConfigName = "NetTcpBinding_IProjectsContract";
        private const string CabsServiceDir = "Cabs";
        private const string CabsConfigName = "NetTcpBinding_ICabContract";
        private const string TestServiceDir = "Test";
        private const string TestConfigName = "NetTcpBinding_ITestContract";

        private const string EndpointTemplate = "net.tcp://{0}:{1}/StackHash/{2}"; // hostname, port, service dir
        private const string ServicePrincipalTemplate = "host/{0}"; // hostname
        private const double IncreaseMaxReceivedMessageSizeFactor = 1.2; // add 20% to requested size when increasing

        private object _lock;
        private bool _disposed;
        private bool _recycleAdminNeeded;
        private bool _recycleProjectsNeeded;
        private bool _recycleCabsNeeded;
        private bool _recycleTestNeeded;
        private string _serviceHost;
        private int _servicePort;
        private int _maxCabContractReceivedMessageSize;
        private int _maxProjectsContractReceivedMessageSize;
        private NetworkCredential _impersonateCredential;
        private string _serviceUsername;
        private string _servicePassword;
        private string _serviceDomain;

        private AdminContractClient _admin;
        private ProjectsContractClient _projects;
        private CabContractClient _cabs;
        private TestContractClient _test;

        /// <summary>
        /// Provdes access to the StackHash service(s)
        /// </summary>
        public static ServiceProxy Services
        {
            get 
            { 
                return ServiceProxy._services; 
            }
        }

        /// <summary>
        /// Hook up to this event to receive admin reports from the server.
        /// </summary>
        public event EventHandler<AdminReportEventArgs> AdminReport;

        /// <summary>
        /// Gets the currently configured host for services
        /// </summary>
        public string ServiceHost
        {
            get 
            {
                string ret;
                lock (_lock)
                {
                    ret = _serviceHost;
                }
                return ret; 
            }
        }

        /// <summary>
        /// Gets the currently configured port for services
        /// </summary>
        public int ServicePort
        {
            get 
            {
                int ret;
                lock (_lock)
                {
                    ret = _servicePort;
                }
                return ret; 
            }
        }

        /// <summary>
        /// The AdminContractClient service
        /// </summary>
        public AdminContractClient Admin
        {
            get 
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("ServiceProxy");
                }

                // create / recycle as needed
                if ((_admin == null) || (_admin.State != CommunicationState.Opened) || _recycleAdminNeeded)
                {
                    lock (_lock)
                    {
                        if ((_admin == null) || (_admin.State != CommunicationState.Opened) || _recycleAdminNeeded)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                string.Format(CultureInfo.InvariantCulture,
                                "ServiceProxy: Creating/recycling admin client: Client: {0}, State: {1}, Recycle: {2}",
                                _admin == null ? "null" : _admin.ToString(),
                                _admin == null ? "n/a" : _admin.State.ToString(),
                                _recycleAdminNeeded));

                            CloseAdmin();

                            InstanceContext context = new InstanceContext(this);

                            _admin = new AdminContractClient(context,
                                AdminConfigName,
                                EndpointAddressForServiceDir(AdminServiceDir));

                            if (_impersonateCredential != null)
                            {
                                _admin.ClientCredentials.Windows.ClientCredential = _impersonateCredential;
                                _admin.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                            }

                            // set the max receive message size
                            NetTcpBinding tcpBinding = _admin.Endpoint.Binding as NetTcpBinding;
                            tcpBinding.MaxReceivedMessageSize = Properties.Settings.Default.ContractAdminMaxReceiveBytes;
                            tcpBinding.MaxBufferSize = Properties.Settings.Default.ContractAdminMaxReceiveBytes;
                            tcpBinding.ReaderQuotas.MaxStringContentLength = Properties.Settings.Default.ContractsMaxStringSize;
                            tcpBinding.ReaderQuotas.MaxArrayLength = Properties.Settings.Default.ContractsMaxArraySize;

                            // set the operation timeout
                            _admin.InnerChannel.OperationTimeout = new TimeSpan(0, Properties.Settings.Default.ContractAdminOperationTimeoutInMinutes, 0);

                            _recycleAdminNeeded = false;
                        }
                    }
                }

                return _admin; 
            }
        }

        /// <summary>
        /// The ProjectsContractClient service
        /// </summary>
        public ProjectsContractClient Projects
        {
            get 
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("ServiceProxy");
                }

                // create / recycle as needed
                if ((_projects == null) || (_projects.State != CommunicationState.Opened) || _recycleProjectsNeeded)
                {
                    lock (_lock)
                    {
                        if ((_projects == null) || (_projects.State != CommunicationState.Opened) || _recycleProjectsNeeded)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                string.Format(CultureInfo.InvariantCulture,
                                "ServiceProxy: Creating/recycling projects client: Client: {0}, State: {1}, Recycle: {2}",
                                _projects == null ? "null" : _projects.ToString(),
                                _projects == null ? "n/a" : _projects.State.ToString(),
                                _recycleProjectsNeeded));

                            CloseProjects();

                            _projects = new ProjectsContractClient(ProjectsConfigName,
                                EndpointAddressForServiceDir(ProjectsServiceDir));

                            if (_impersonateCredential != null)
                            {
                                _projects.ClientCredentials.Windows.ClientCredential = _impersonateCredential;
                                _projects.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                            }

                            SetDataSerializerOperationBehaviour(_projects.ChannelFactory);

                            // set the max receive message size
                            NetTcpBinding tcpBinding = _projects.Endpoint.Binding as NetTcpBinding;
                            tcpBinding.MaxReceivedMessageSize = _maxProjectsContractReceivedMessageSize;
                            tcpBinding.MaxBufferSize = _maxProjectsContractReceivedMessageSize;
                            tcpBinding.ReaderQuotas.MaxStringContentLength = Properties.Settings.Default.ContractsMaxStringSize;
                            tcpBinding.ReaderQuotas.MaxArrayLength = Properties.Settings.Default.ContractsMaxArraySize;

                            // set the operation timeout
                            _projects.InnerChannel.OperationTimeout = new TimeSpan(0, Properties.Settings.Default.ContractProjectsOperationTimeoutInMinutes, 0);

                            _recycleProjectsNeeded = false;
                        }
                    }
                }

                return _projects; 
            }
        }

        /// <summary>
        /// The CabContractClient service
        /// </summary>
        public CabContractClient Cabs
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("ServiceProxy");
                }

                // create / recycle as needed
                if ((_cabs == null) || (_cabs.State != CommunicationState.Opened) || _recycleCabsNeeded)
                {
                    lock (_lock)
                    {
                        if ((_cabs == null) || (_cabs.State != CommunicationState.Opened) || _recycleCabsNeeded)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                string.Format(CultureInfo.InvariantCulture,
                                "ServiceProxy: Creating/recycling cabs client: Client: {0}, State: {1}, Recycle: {2}",
                                _cabs == null ? "null" : _cabs.ToString(),
                                _cabs == null ? "n/a" : _cabs.State.ToString(),
                                _recycleCabsNeeded));

                            CloseCabs();

                            _cabs = new CabContractClient(CabsConfigName,
                                EndpointAddressForServiceDir(CabsServiceDir));

                            if (_impersonateCredential != null)
                            {
                                _cabs.ClientCredentials.Windows.ClientCredential = _impersonateCredential;
                                _cabs.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                            }

                            // set the max receive message size
                            NetTcpBinding tcpBinding = _cabs.Endpoint.Binding as NetTcpBinding;
                            tcpBinding.MaxReceivedMessageSize = _maxCabContractReceivedMessageSize;
                            tcpBinding.ReaderQuotas.MaxStringContentLength = Properties.Settings.Default.ContractsMaxStringSize;
                            tcpBinding.ReaderQuotas.MaxArrayLength = Properties.Settings.Default.ContractsMaxArraySize;

                            // set the operation timeout
                            _cabs.InnerChannel.OperationTimeout = new TimeSpan(0, Properties.Settings.Default.ContractCabsOperationTimeoutInMinutes, 0);

                            _recycleCabsNeeded = false;
                        }
                    }
                }

                return _cabs;
            }
        }

        /// <summary>
        /// The TestContractClient service
        /// </summary>
        public TestContractClient Test
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("ServiceProxy");
                }

                // create / recycle as needed
                if ((_test == null) || (_test.State != CommunicationState.Opened) || _recycleTestNeeded)
                {
                    lock (_lock)
                    {
                        if ((_test == null) || (_test.State != CommunicationState.Opened) || _recycleTestNeeded)
                        {
                            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                string.Format(CultureInfo.InvariantCulture,
                                "ServiceProxy: Creating/recycling test client: Client: {0}, State: {1}, Recycle: {2}",
                                _test == null ? "null" : _test.ToString(),
                                _test == null ? "n/a" : _test.State.ToString(),
                                _recycleTestNeeded));

                            CloseTest();

                            _test = new TestContractClient(TestConfigName,
                                EndpointAddressForServiceDir(TestServiceDir));

                            if (_impersonateCredential != null)
                            {
                                _test.ClientCredentials.Windows.ClientCredential = _impersonateCredential;
                                _test.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                            }

                            SetDataSerializerOperationBehaviour(_projects.ChannelFactory);

                            NetTcpBinding tcpBinding = _test.Endpoint.Binding as NetTcpBinding;
                            tcpBinding.ReaderQuotas.MaxStringContentLength = Properties.Settings.Default.ContractsMaxStringSize;
                            tcpBinding.ReaderQuotas.MaxArrayLength = Properties.Settings.Default.ContractsMaxArraySize;

                            // set the operation timeout
                            _test.InnerChannel.OperationTimeout = new TimeSpan(0, Properties.Settings.Default.ContractTestOperationTimeoutInMinutes, 0);

                            _recycleTestNeeded = false;
                        }
                    }
                }

                return _test;
            }
        }

        /// <summary>
        /// Gets or sets the maximum message size for the projects contract - on set the 
        /// projects client will be recycled on next use
        /// </summary>
        public int MaxProjectsContractReceivedMessageSize
        {
            get 
            {
                int ret;

                lock (_lock)
                {
                    ret = _maxProjectsContractReceivedMessageSize;
                }

                return ret; 
            }
            set 
            {
                lock (_lock)
                {
                    if (_maxProjectsContractReceivedMessageSize != value)
                    {
                        _maxProjectsContractReceivedMessageSize = value;
                        _recycleProjectsNeeded = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if any of the service clients are currently in the CommunicationState.Faulted state
        /// </summary>
        /// <returns></returns>
        public bool AnyFaulted()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ServiceProxy");
            }

            bool anyFaulted = false;

            lock (_lock)
            {
                if (_admin != null)
                {
                    if (_admin.State == CommunicationState.Faulted)
                    {
                        anyFaulted = true;
                    }
                }

                if (_projects != null)
                {
                    if (_projects.State == CommunicationState.Faulted)
                    {
                        anyFaulted = true;
                    }
                }

                if (_cabs != null)
                {
                    if (_cabs.State == CommunicationState.Faulted)
                    {
                        anyFaulted = true;
                    }
                }
            }

            return anyFaulted;
        }

        /// <summary>
        /// Opens all services
        /// </summary>
        public void OpenAll()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ServiceProxy");
            }

            Admin.ToString();
            Projects.ToString();
            Cabs.ToString();
            Test.ToString();
        }

        /// <summary>
        /// Notify ServiceProxy that a cab is about to be downloaded.
        /// If the number of bytes is greater that the current interface
        /// maximum then the interface is recycled to accommodate the download.
        /// </summary>
        /// <param name="bytesToDownload">Length of the cab in bytes</param>
        public void NotifyCabDownloadLength(long bytesToDownload)
        {
            // include a safety margin (check for overflow)
            int bytesToDownloadWithMargin;
            checked
            {
                bytesToDownloadWithMargin = (int)(bytesToDownload * IncreaseMaxReceivedMessageSizeFactor);
            }

            if (bytesToDownloadWithMargin > _maxCabContractReceivedMessageSize)
            {
                lock (_lock)
                {
                    if (bytesToDownloadWithMargin > _maxCabContractReceivedMessageSize)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "ServiceProxy: Increasing cabs client MaxReceivedMessageSize to {0:n0} bytes",
                            bytesToDownloadWithMargin));

                        _maxCabContractReceivedMessageSize = bytesToDownloadWithMargin;
                        
                        // recycle the cabs service
                        _recycleCabsNeeded = true;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the current service endpoint and account to impersonate
        /// </summary>
        /// <param name="host">Service host</param>
        /// <param name="port">Service port</param>
        /// <param name="username">Username of the account to impersonate (optional)</param>
        /// <param name="password">Password of the account to impersonate (optional)</param>
        /// <param name="domain">Domain of the account to impersonate (optional)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username")]
        public void UpdateServiceEndpointAndAccount(string host, int port, string username, string password, string domain)
        {
            if ((host != _serviceHost) || (port != _servicePort) || 
                (username != _serviceUsername) || (password != _servicePassword) || (domain != _serviceDomain))
            {
                lock (_lock)
                {
                    if ((host != _serviceHost) || (port != _servicePort))
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "ServiceProxy: Updating service endpoint to {0}:{1}",
                            host,
                            port));

                        _serviceHost = host;
                        _servicePort = port;
                        
                        // recycle services the next time they are accessed
                        _recycleAdminNeeded = true;
                        _recycleProjectsNeeded = true;
                        _recycleCabsNeeded = true;
                        _recycleTestNeeded = true;
                    }

                    if ((username != _serviceUsername) || (password != _servicePassword) || (domain != _serviceDomain))
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "ServiceProxy: Updating service account to {0}{1}",
                            string.IsNullOrEmpty(domain) ? "" : domain + "\\",
                            string.IsNullOrEmpty(username) ? "current credentials" : username));

                        _impersonateCredential = null;
                        _serviceUsername = username;
                        _servicePassword = password;
                        _serviceDomain = domain;

                        if ((!string.IsNullOrEmpty(_serviceUsername)) && (!string.IsNullOrEmpty(_servicePassword)))
                        {
                            if (string.IsNullOrEmpty(_serviceDomain))
                            {
                                _impersonateCredential = new NetworkCredential(_serviceUsername, _servicePassword);
                            }
                            else
                            {
                                _impersonateCredential = new NetworkCredential(_serviceUsername, _servicePassword, _serviceDomain);
                            }
                        }

                        // recycle services the next time they are accessed
                        _recycleAdminNeeded = true;
                        _recycleProjectsNeeded = true;
                        _recycleCabsNeeded = true;
                        _recycleTestNeeded = true;
                    }
                }
            }
        }

        /// <summary>
        /// Closes all services
        /// </summary>
        public void CloseAll()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ServiceProxy");
            }

            lock (_lock)
            {
                CloseAdmin();
                CloseProjects();
                CloseCabs();
                CloseTest();
            }
        }

        private EndpointAddress EndpointAddressForServiceDir(string serviceDir)
        {
            EndpointIdentity endpointIdentity = EndpointIdentity.CreateSpnIdentity(string.Format(CultureInfo.InvariantCulture,
                ServicePrincipalTemplate,
                _serviceHost));

            return new EndpointAddress(new Uri(string.Format(CultureInfo.InvariantCulture,
                EndpointTemplate,
                _serviceHost,
                _servicePort,
                serviceDir)),
                endpointIdentity);
        }

        private static void SetDataSerializerOperationBehaviour(ChannelFactory<StackHashService.IProjectsContract> channelFactory)
        {
            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior =
                  op.Behaviors[typeof(DataContractSerializerOperationBehavior)] as DataContractSerializerOperationBehavior;

                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CloseAdmin()
        {
            try
            {
                if ((_admin != null) && (_admin.State != CommunicationState.Closed))
                {
                    if (_admin.State == CommunicationState.Faulted)
                    {
                        _admin.Abort();
                    }
                    else
                    {
                        _admin.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                    "ServiceProxy: Failed to close Admin interface",
                    ex);
            }
            finally
            {
                _admin = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CloseProjects()
        {
            try
            {
                if ((_projects != null) && (_projects.State != CommunicationState.Closed))
                {
                    if (_projects.State == CommunicationState.Faulted)
                    {
                        _projects.Abort();
                    }
                    else
                    {
                        _projects.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                    "ServiceProxy: Failed to close Projects interface",
                    ex);
            }
            finally
            {
                _projects = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CloseTest()
        {
            try
            {
                if ((_test != null) && (_test.State != CommunicationState.Closed))
                {
                    if (_test.State == CommunicationState.Faulted)
                    {
                        _test.Abort();
                    }
                    else
                    {
                        _test.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                    "ServiceProxy: Failed to close Test interface",
                    ex);
            }
            finally
            {
                _test = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CloseCabs()
        {
            try
            {
                if ((_cabs != null) && (_cabs.State != CommunicationState.Closed))
                {
                    if (_cabs.State == CommunicationState.Faulted)
                    {
                        _cabs.Abort();
                    }
                    else
                    {
                        _cabs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal,
                    "ServiceProxy: Failed to close Cabs interface",
                    ex);
            }
            finally
            {
                _cabs = null;
            }
        }

        private ServiceProxy()
        {
            _lock = new object();
            _serviceHost = UserSettings.DefaultServiceHost;
            _servicePort = UserSettings.DefaultServicePort;
            _maxCabContractReceivedMessageSize = Properties.Settings.Default.ContractCabsInitialMaxReceiveBytes;
            _maxProjectsContractReceivedMessageSize = Properties.Settings.Default.ContractProjectsMaxReceiveBytes;
            _impersonateCredential = null;
            _serviceUsername = null;
            _servicePassword = null;
            _serviceDomain = null;
        }

        #region IDisposable Members

        /// <summary />
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                
                _disposed = true;
            }
        }

        /// <summary />
        ~ServiceProxy()
        {
            Dispose(false);
        }

        private void Dispose(bool canDisposeManagedResources)
        {
            if (canDisposeManagedResources)
            {
                CloseAll();
            }
        }

        #endregion

        #region IAdminContractCallback Members

        /// <summary>
        /// Called by the proxy when the server reports an admin event.
        /// </summary>
        /// <param name="adminReport">The report</param>
        public void AdminProgressEvent(StackHashAdminReport adminReport)
        {
            Debug.Assert(adminReport != null);

            if (AdminReport != null)
            {
                AdminReport(this, new AdminReportEventArgs(adminReport));
            }
        }

        #endregion
    }
}
