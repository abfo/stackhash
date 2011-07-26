using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Globalization;
using StackHashUtilities;

namespace StackHashServiceImplementation
{
    //class CustomHost : ServiceHost
    //{
    //    public CustomHost(Type serviceType) : base (serviceType)         
    //    {
    //    }
    //    protected override void ApplyConfiguration()
    //    {
    //        base.ApplyConfiguration();

    //        //Add a metadata endpoint at each base address
    //        //using the "/mex" addressing convention
    //        this.AddBaseAddress(new Uri("net.tcp://localhost:9000/StackHash"));
    //    }
    //}
    
    public sealed class ServiceHostControl
    {
        static ServiceHost s_InternalServiceHost;
        static ServiceHost s_ExternalServiceHost;

        /// <summary>
        /// Define the default constructor as private so that no public constructor is visible.
        /// </summary>

        private ServiceHostControl() { ;}


        /// <summary>
        /// Opens services associated with the internal or external host.
        /// An internal host uses TCP for comms on an intranet along with HTTP to publish
        /// the metadata (mex - metadata exchange).
        /// An external host exposes services via HTTP to the internet.
        /// On Vista a service cannot use HTTP directly because the whole http:\ namespace is 
        /// owned by AdminUser. Therefore you need to reassign say http:\\localhost\stackhash using
        /// netsh http add urlacl url=http:\\localhost\stackhash /user=NetworkService
        /// </summary>
        /// <param name="internalHost">True - internal contract is registered - false - external contract registered.</param>

        public static void OpenServiceHosts(bool internalHost)
        {
            if (internalHost)
            {

                // Get the environment variables that can override the STACKHASHPORT.
                //String portString = Environment.GetEnvironmentVariable("STACKHASHPORT");
                //int port = -1;
                //try
                //{
                //    if (!String.IsNullOrEmpty(portString))
                //    {
                //        port = Int32.Parse(portString, CultureInfo.InvariantCulture);
                //        DiagnosticsHelper.LogMessage(DiagSeverity.Information, "STACKHASHPORT=" + port.ToString(CultureInfo.InvariantCulture));
                //    }
                //}
                //catch (System.Exception ex)
                //{
                //    DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal, "STACKHASHPORT invalid - defaulting to port 9000", ex);
                //}


                if (s_InternalServiceHost == null)
                {
                    try
                    {
                        s_InternalServiceHost = new ServiceHost(typeof(InternalService));

                        if (s_InternalServiceHost.BaseAddresses != null)
                        {
                            foreach (Uri baseAddress in s_InternalServiceHost.BaseAddresses)
                            {
                                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "WCF Using base address: " + baseAddress.ToString());
                            }
                        }
                        
                        s_InternalServiceHost.Open();
                    }
                    catch (AddressAlreadyInUseException ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal, "WCF port is already in use.", ex);
                        throw;
                    }
                }
            }
            else
            {
                if (s_ExternalServiceHost == null)
                {
                    s_ExternalServiceHost = new ServiceHost(typeof(ExternalService));
                    s_ExternalServiceHost.Open();
                }
            }
        }


        /// <summary>
        /// Closes the internal or external service if open.
        /// </summary>

        public static void CloseServiceHosts()
        {
            if ((s_InternalServiceHost != null) && (s_InternalServiceHost.State != CommunicationState.Closed))
            {
                if (s_InternalServiceHost.State == CommunicationState.Faulted)
                    s_InternalServiceHost.Abort();
                else
                    s_InternalServiceHost.Close();
            }
            if ((s_ExternalServiceHost != null) && (s_ExternalServiceHost.State != CommunicationState.Closed))
            {
                if (s_ExternalServiceHost.State == CommunicationState.Faulted)
                    s_ExternalServiceHost.Abort();
                else
                    s_ExternalServiceHost.Close();
            }
        }
    }
}
