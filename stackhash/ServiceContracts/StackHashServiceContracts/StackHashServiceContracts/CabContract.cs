using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using StackHashBusinessObjects;
using System.Runtime.Serialization;



namespace StackHashServiceContracts
{
    /// <summary>
    /// Identifies the CAB that is required.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetCabFileRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        String m_FileName;


        /// <summary>
        /// The context ID for which product file event cab is required.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Product to which the files event data refers.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to which the events data refers.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to which the events data refers.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Event to which the events data refers.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        /// <summary>
        /// Name of the file within the cab to get. Set to null if the full cab is required.
        /// </summary>
        [DataMember]
        public String FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }
    }



    /// <summary>
    /// Service Contract provided to the StackHash client. 
    /// This contract permits the client to stream cab files to the client.
    /// This eliminates the need to send large files in a single message and therefore reduces
    /// memory consumption at the client and server.
    /// </summary>
    [ServiceContract(Namespace = "http://www.cucku.com/stackhash/2010/02/17",
            SessionMode=SessionMode.Allowed)]  // Can't use SessionMode.Required with streams.
    public interface ICabContract
    {
        /// <summary>
        /// Gets the specified cab file.
        /// </summary>
        /// <returns>Current settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        Stream GetCabFile(GetCabFileRequest requestData);
    }
}
