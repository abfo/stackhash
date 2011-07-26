using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using StackHashBusinessObjects;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;



namespace StackHashServiceContracts
{
    /// <summary>
    /// Creates a set of test data for an index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CreateTestIndexRequest
    {
        StackHashClientData m_ClientData;
        int m_ContextId;
        StackHashTestIndexData m_TestIndexData;

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
        /// The context ID to be started. A company may have several WinQual
        /// registrations. This field identifies the correct one.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }
        /// <summary>
        /// Test data.
        /// </summary>
        [DataMember]
        public StackHashTestIndexData TestIndexData
        {
            get { return m_TestIndexData; }
            set { m_TestIndexData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to create a test index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CreateTestIndexResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Sets the test data used by the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetTestDataRequest
    {
        StackHashClientData m_ClientData;
        StackHashTestData m_TestData;

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
        /// Test data.
        /// </summary>
        [DataMember]
        public StackHashTestData TestData
        {
            get { return m_TestData; }
            set { m_TestData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to set the service test data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetTestDataResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Gets the specified attribute from the testmode.xml file.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetTestModeSettingRequest
    {
        StackHashClientData m_ClientData;
        String m_AttributeName;

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
        /// Name of the attribute whose value is required.
        /// </summary>
        [DataMember]
        public String AttributeName
        {
            get { return m_AttributeName; }
            set { m_AttributeName = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to set the service test data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetTestModeSettingResponse
    {
        StackHashServiceResultData m_ResultData;
        String m_AttributeName;
        String m_AttributeValue;

        /// <summary>
        /// The result of the request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// Name of the attribute whose value is required.
        /// </summary>
        [DataMember]
        public String AttributeName
        {
            get { return m_AttributeName; }
            set { m_AttributeName = value; }
        }

        /// <summary>
        /// Value of the attribute whose value is required.
        /// </summary>
        [DataMember]
        public String AttributeValue
        {
            get { return m_AttributeValue; }
            set { m_AttributeValue = value; }
        }
    }

    
    /// <summary>
    /// Service Contract provided to the StackHash client. This contract permits the client to 
    /// manage test indexes and data for client server testing.
    /// </summary>
    [ServiceContract(Namespace = "http://www.cucku.com/stackhash/2010/02/17",
        SessionMode = SessionMode.Required)]
    public interface ITestContract
    {
        /// <summary>
        /// Creates a test index on the server.
        /// Use the admin interface to delete a test index as that is a feature of the release version
        /// of the product.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        CreateTestIndexResponse CreateTestIndex(CreateTestIndexRequest requestData);

        /// <summary>
        /// Sets the test data used by the service in test mode.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetTestDataResponse SetTestData(SetTestDataRequest requestData);

        /// <summary>
        /// Gets testmode.xml attribute settings.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetTestModeSettingResponse GetTestModeSetting(GetTestModeSettingRequest requestData);
    }
}
