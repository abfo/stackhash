/// ***********************************************************************************************
/// Name:
///		ServiceProxyControl.
///
/// Description:
///     Contains methods to control the creation of a proxy to talk to the StackHash client.
///     The proxy is a C# object with all the methods defined - including all of the types used
///     inside those methods.
///     
///     To create a client...
///     1) Create the client project.
///     2) Run the TestHost application contained in this project (as admin).
///     3) Add a service reference (right click references) - then select http://localhost:8000 
///        and click GO. Select the service from the list and choose a namespace (bottom of page)
///        called "StackHashServices".
///     4) Add a copy of this file to your project.
///     5) Call OpenServiceProxy() on application load.
///     6) Call CloseServiceProxy() on application close.
///     7) You can make calls directly to the returned proxy.
///     
///     ** If the service contracts change you MUST delete the service reference and re-add it.
///     Apparently the Service Reference Update feature doesn't work for all updates.
///     
///     Advice
///     a) Don't add references to the StackHashDataContracts or StackHashServiceImplemention or 
///        StackHashServiceContracts. Just import the service data and create the proxy code as 
///        described.
///     b) Make sure you Abort rather than Close if the state is 
///        System.ServiceModel.CommunicationState.Faulted
/// ***********************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;


namespace ServiceUnitTests
{
    /// <summary>
    /// Parameters pass to the AdminReport delegate.
    /// </summary>
    public class AdminReportEventArgs : EventArgs
    {
        private StackHashServices.StackHashAdminReport m_Report;

        /// <summary>
        /// Report containing information about the async service event.
        /// </summary>
        public StackHashServices.StackHashAdminReport Report
        {
            get { return m_Report; }
            set { m_Report = value; }
        }
    }


    /// <summary>
    /// Client access to the StackHash server.
    /// Hook up to AdminReport to receive admin events from the server. Then call Register
    /// </summary>
    public class ServiceProxyControl : StackHashServices.IAdminContractCallback, IDisposable
    {
        long m_CabContractMaxReceivedMessageSize;
        StackHashServices.AdminContractClient m_StackHashAdminClient;
        StackHashServices.ProjectsContractClient m_StackHashProjectsClient;
        StackHashServices.CabContractClient m_StackHashCabClient;
        StackHashServices.TestContractClient m_StackHashTestClient;
        String m_ServiceEndpoint;
        String m_ServicePrincipalName; // e.g. host/CuckuSrv

        /// <summary>
        /// Hook up to this event to receive admin reports from the server.
        /// </summary>
        public event EventHandler<AdminReportEventArgs> AdminReport;


        /// <summary>
        /// Access to the stackhash server admin interface.
        /// </summary>
        public StackHashServices.AdminContractClient StackHashAdminClient
        {
            get
            {
                // Make sure the service channel is open.
                openAdminServiceProxy();
                return m_StackHashAdminClient;
            }
        }

        /// <summary>
        /// Access to the stackhash server project interface.
        /// </summary>
        public StackHashServices.ProjectsContractClient StackHashProjectsClient
        {
            get
            {
                openProjectsServiceProxy();
                return m_StackHashProjectsClient;
            }
        }

        /// <summary>
        /// Access to the stackhash server Cab interface.
        /// </summary>
        public StackHashServices.CabContractClient StackHashCabClient
        {
            get
            {
                // Make sure the service channel is open.
                openCabServiceProxy();
                return m_StackHashCabClient;
            }
        }

        /// <summary>
        /// Access to the stackhash server Test interface.
        /// </summary>
        public StackHashServices.TestContractClient StackHashTestClient
        {
            get
            {
                // Make sure the service channel is open.
                openTestServiceProxy();
                return m_StackHashTestClient;
            }
        }

        /// <summary>
        /// Define the default constructor as private so that no public constructor is visible.
        /// </summary>
        /// <param name="serviceEndpoint">The uri name of the endpoint e.g. net.tcp.</param>
        /// <param name="servicePrincipalName">The host name at the service host/devquad.</param>
        /// <param name="cabContractMaxReceivedMessageSize">The cab contract streaming message size.</param>
        public ServiceProxyControl(String serviceEndpoint, String servicePrincipalName, long cabContractMaxReceivedMessageSize)
        {
            m_ServiceEndpoint = serviceEndpoint;
            m_ServicePrincipalName = servicePrincipalName;
            m_CabContractMaxReceivedMessageSize = cabContractMaxReceivedMessageSize;

            openAdminServiceProxy();
            openProjectsServiceProxy();
            openCabServiceProxy();
            openTestServiceProxy();
        }


        /// <summary>
        /// Called by the proxy when the server reports an admin event.
        /// </summary>
        /// <param name="adminReport"></param>
        public void AdminProgressEvent(StackHashServices.StackHashAdminReport adminReport)
        {
            EventHandler<AdminReportEventArgs> handler = AdminReport;

            AdminReportEventArgs adminReportArgs = new AdminReportEventArgs();
            adminReportArgs.Report = adminReport;
            if (handler != null)
                handler(this, adminReportArgs);
        }


        /// <summary>
        /// Creates a proxy for communicating with the StackHash service Admin contract.
        /// The proxy is only created once. Trying to call more than once will have no
        /// additionaly effect. If the service is faulted then call CloseServiceProxy 
        /// and then call this function again.
        /// A proxy is faulted if...
        /// Proxy.State == System.ServiceModel.CommunicationState.Faulted.
        /// </summary>

        private void openAdminServiceProxy()
        {
            if (m_StackHashAdminClient == null)
            {
                // create custom endpoint address in code - based on input in the textbox 
                EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity(m_ServicePrincipalName);
                EndpointAddress epa = new EndpointAddress(new Uri(m_ServiceEndpoint + "Admin"), identity);

                // This class implements the admin callback.
                InstanceContext context = new InstanceContext(this);
                m_StackHashAdminClient = new StackHashServices.AdminContractClient(context, "NetTcpBinding_IAdminContract", epa);
                m_StackHashAdminClient.InnerChannel.OperationTimeout = new TimeSpan(0, 15, 0);
            }
            else
            {
                if (m_StackHashAdminClient.State != CommunicationState.Opened)
                {
                    closeAdminServiceProxy();
                    openAdminServiceProxy();
                }
            }
        }

        private void setDataSerializerOperationBehaviour(ChannelFactory<StackHashServices.IProjectsContract> channelFactory)
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


        /// <summary>
        /// Creates a proxy for communicating with the StackHash service Admin contract.
        /// The proxy is only created once. Trying to call more than once will have no
        /// additionaly effect. If the service is faulted then call CloseServiceProxy 
        /// and then call this function again.
        /// A proxy is faulted if...
        /// Proxy.State == System.ServiceModel.CommunicationState.Faulted.
        /// </summary>

        private void openProjectsServiceProxy()
        {
            if (m_StackHashProjectsClient == null)
            {
                // create custom endpoint address in code - based on input in the textbox 
                EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity(m_ServicePrincipalName);
                EndpointAddress epa = new EndpointAddress(new Uri(m_ServiceEndpoint + "Projects"), identity);

                m_StackHashProjectsClient = new StackHashServices.ProjectsContractClient("NetTcpBinding_IProjectsContract", epa);
                NetTcpBinding binding = m_StackHashProjectsClient.ChannelFactory.Endpoint.Binding as NetTcpBinding;
                binding.MaxReceivedMessageSize = 50000000;
                binding.MaxBufferSize = 50000000;
                m_StackHashProjectsClient.InnerChannel.OperationTimeout = new TimeSpan(0, 15, 0);
                setDataSerializerOperationBehaviour(m_StackHashProjectsClient.ChannelFactory);
            }
            else
            {
                if (m_StackHashProjectsClient.State != CommunicationState.Opened)
                {
                    closeProjectsServiceProxy();
                    openProjectsServiceProxy();
                }
            }
        }

        /// <summary>
        /// Creates a proxy for communicating with the StackHash service Cab contract.
        /// The proxy is only created once. Trying to call more than once will have no
        /// additionaly effect. If the service is faulted then call CloseServiceProxy 
        /// and then call this function again.
        /// A proxy is faulted if...
        /// Proxy.State == System.ServiceModel.CommunicationState.Faulted.
        /// </summary>

        private void openCabServiceProxy()
        {
            if (m_StackHashCabClient == null)
            {
                // create custom endpoint address in code - based on input in the textbox 
                EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity(m_ServicePrincipalName);
                EndpointAddress epa = new EndpointAddress(new Uri(m_ServiceEndpoint + "Cabs"), identity);

                m_StackHashCabClient = new StackHashServices.CabContractClient("NetTcpBinding_ICabContract", epa);
                NetTcpBinding binding = m_StackHashCabClient.ChannelFactory.Endpoint.Binding as NetTcpBinding;
                binding.MaxReceivedMessageSize = m_CabContractMaxReceivedMessageSize;
            }
            else
            {
                if (m_StackHashCabClient.State != CommunicationState.Opened)
                {
                    closeCabServiceProxy();
                    openCabServiceProxy();
                }
            }
        }

        /// <summary>
        /// Creates a proxy for communicating with the StackHash service Cab contract.
        /// The proxy is only created once. Trying to call more than once will have no
        /// additionaly effect. If the service is faulted then call CloseServiceProxy 
        /// and then call this function again.
        /// A proxy is faulted if...
        /// Proxy.State == System.ServiceModel.CommunicationState.Faulted.
        /// </summary>

        private void openTestServiceProxy()
        {
            if (m_StackHashTestClient == null)
            {
                // create custom endpoint address in code - based on input in the textbox 
                EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity(m_ServicePrincipalName);
                EndpointAddress epa = new EndpointAddress(new Uri(m_ServiceEndpoint + "Test"), identity);

                m_StackHashTestClient = new StackHashServices.TestContractClient("NetTcpBinding_ITestContract", epa);
                m_StackHashTestClient.InnerChannel.OperationTimeout = new TimeSpan(1, 0, 0);
                
                //IContextChannel contextProxy = m_StackHashTestClient as IContextChannel;
                //contextProxy.OperationTimeout = new TimeSpan(1, 0, 0); // Might take an hour to create a very large index.
            }
            else
            {
                if (m_StackHashTestClient.State != CommunicationState.Opened)
                {
                    closeTestServiceProxy();
                    openTestServiceProxy();
                }
            }
        }

        /// <summary>
        /// Closes the proxy used for Admin contract with the StackHashService.
        /// </summary>

        private void closeAdminServiceProxy()
        {
            if ((m_StackHashAdminClient != null) && (m_StackHashAdminClient.State != CommunicationState.Closed))
            {
                if (m_StackHashAdminClient.State == CommunicationState.Faulted)
                    m_StackHashAdminClient.Abort();
                else if (m_StackHashAdminClient.State == CommunicationState.Opened)
                    m_StackHashAdminClient.Close();
                m_StackHashAdminClient = null;
            }
        }

        /// <summary>
        /// Closes the proxy used for Projects contract with the StackHashService.
        /// </summary>

        private void closeProjectsServiceProxy()
        {
            if (m_StackHashProjectsClient != null)
            {
                if (m_StackHashProjectsClient.State == CommunicationState.Faulted)
                    m_StackHashProjectsClient.Abort();
                else if (m_StackHashProjectsClient.State == CommunicationState.Opened)
                    m_StackHashProjectsClient.Close();
                m_StackHashProjectsClient = null;
            }
        }

        /// <summary>
        /// Closes the proxy used for cab contract with the StackHashService.
        /// </summary>

        private void closeCabServiceProxy()
        {
            if (m_StackHashCabClient != null)
            {
                if (m_StackHashCabClient.State == CommunicationState.Faulted)
                    m_StackHashCabClient.Abort();
                else if (m_StackHashCabClient.State == CommunicationState.Opened)
                    m_StackHashCabClient.Close();
                m_StackHashCabClient = null;
            }
        }

        /// <summary>
        /// Closes the proxy used for test contract with the StackHashService.
        /// </summary>

        private void closeTestServiceProxy()
        {
            if (m_StackHashTestClient != null)
            {
                if (m_StackHashTestClient.State == CommunicationState.Faulted)
                    m_StackHashTestClient.Abort();
                else if (m_StackHashTestClient.State == CommunicationState.Opened)
                    m_StackHashTestClient.Close();
                m_StackHashTestClient = null;
            }
        }

        #region IDisposable Members


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                closeAdminServiceProxy();
                closeProjectsServiceProxy();
                closeCabServiceProxy();
                closeTestServiceProxy();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}
