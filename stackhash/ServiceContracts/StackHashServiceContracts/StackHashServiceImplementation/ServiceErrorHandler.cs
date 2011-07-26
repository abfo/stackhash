using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Configuration;
using System.ServiceModel.Dispatcher;


using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashServiceImplementation
{
    public class ServiceErrorHandler : IErrorHandler
    {
        bool m_ReentrancyCheck;

        public ServiceErrorHandler()
        {
        }

        // Generate a message from all of the inner exceptions.
        private static String getDescription(Exception ex)
        {
            Exception thisException = ex;

            String message = "";
            while (thisException != null)
            {
                message += thisException.GetType().ToString() + " : ";
                if (!String.IsNullOrEmpty(thisException.Message))
                    message = message + " : " + thisException.Message + ", ";
                thisException = thisException.InnerException;
            }
            return message;
        }

        public bool HandleError(Exception error)
        {
            return true;

        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void ProvideFault(Exception error, 
                                 System.ServiceModel.Channels.MessageVersion version, 
                                 ref System.ServiceModel.Channels.Message fault)
        {
            if (m_ReentrancyCheck)
                return;

            m_ReentrancyCheck = true;

            try
            {
                if ((fault == null) && (error != null))
                {
                    ReceiverFaultDetail receiverFaultDetail = new ReceiverFaultDetail(
                        error.Message, getDescription(error), StackHashException.GetServiceErrorCode(error));

                    FaultException<ReceiverFaultDetail> fe = new FaultException<ReceiverFaultDetail>(
                        receiverFaultDetail, error.Message, FaultCode.CreateReceiverFaultCode(new FaultCode("ReceiverFault")));

                    MessageFault mf = fe.CreateMessageFault();

                    fault = Message.CreateMessage(version, mf, fe.Action);


                    try
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Service exception occurred", error);
                    }
                    catch (System.Exception)
                    {
                        // Ignore the error.
                    }
                }
            }
            finally
            {
                m_ReentrancyCheck = false;
            }
        }
    }
}
