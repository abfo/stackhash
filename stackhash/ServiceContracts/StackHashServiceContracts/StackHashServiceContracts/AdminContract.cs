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
    /// The general result of a request to the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashServiceResult
    {
        /// <summary>
        /// Request successfully processed.
        /// </summary>
        [EnumMember]
        Success,

        /// <summary>
        /// Request failed - see Message and LastException for further details.
        /// </summary>
        [EnumMember]
        Failed, // See error message.
    }

    /// <summary>
    /// General result data returned from all service requests.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashServiceResultData
    {
        StackHashServiceResult m_Result; // If Failed then see m_Message for description.
        String m_Message;                // Reason for the failure.
        String m_LastException;       // Last exception that occurred in the service.

        /// <summary>
        /// Result of a request attempt - Success or Failed.
        /// </summary>
        [DataMember]
        public StackHashServiceResult Result
        {
            get { return m_Result; }
            set { m_Result = value; }
        }

        /// <summary>
        /// A textual description of the failure if Result is Failed.
        /// </summary>
        [DataMember]
        public String Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        /// <summary>
        /// The last exception to occur during service request processing.
        /// Check the inner exception.
        /// </summary>
        [DataMember]
        public String LastException
        {
            get { return m_LastException; }
            set { m_LastException = value; }
        }

        /// <summary>
        /// Default constructor required for serialization.
        /// </summary>
        public StackHashServiceResultData() { ; }

        /// <summary>
        /// Creates a result structure.
        /// </summary>
        /// <param name="result">Success or Failed.</param>
        /// <param name="message">Textual description of the error.</param>
        /// <param name="lastException">Last exception causing the error - see inner exception also.</param>
        public StackHashServiceResultData(StackHashServiceResult result, String message, Exception lastException)
        {
            m_Result = result;
            m_Message = message;
            if (lastException != null)
                m_LastException = lastException.ToString();
        }
    }

    /// <summary>
    /// Registers the application to receive admin notifications.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RegisterRequest
    {
        StackHashClientData m_ClientData;
        bool m_IsRegister;

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
        /// True - register, false - unregister.
        /// </summary>
        [DataMember]
        public bool IsRegister
        {
            get { return m_IsRegister; }
            set { m_IsRegister = value; }
        }
    }

    /// <summary>
    /// Checks the client version.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CheckVersionRequest
    {
        StackHashClientData m_ClientData;
        int m_MajorVersion;
        int m_MinorVersion;
        String m_ServiceGuid;

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
        /// StackHash Client Major Version number.
        /// </summary>
        [DataMember]
        public int MajorVersion
        {
            get { return m_MajorVersion; }
            set { m_MajorVersion = value; }
        }

        /// <summary>
        /// StackHash Client Minor Version number.
        /// </summary>
        [DataMember]
        public int MinorVersion
        {
            get { return m_MinorVersion; }
            set { m_MinorVersion = value; }
        }

        /// <summary>
        /// Service guid. The client gets this from c:\programdata\stackhash\serviceinstance.xml.
        /// </summary>
        [DataMember]
        public String ServiceGuid
        {
            get { return m_ServiceGuid; }
            set { m_ServiceGuid = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request check the client version.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CheckVersionResponse
    {
        StackHashServiceResultData m_ResultData;
        bool m_IsLocalClient;

        /// <summary>
        /// The result of the check version request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// Client is running on same machine as the service.
        /// </summary>
        [DataMember]
        public bool IsLocalClient
        {
            get { return m_IsLocalClient; }
            set { m_IsLocalClient = value; }
        }
    }


    
    /// <summary>
    /// Re-initialises the entire service without closing the application.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RestartRequest
    {
        StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get the StackHash service settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RestartResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the restart request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }


    /// <summary>
    /// Deletes the specified context index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeleteIndexRequest
    {
        StackHashClientData m_ClientData;
        int m_ContextId;

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
        /// Id of the context to remove.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

    }

    /// <summary>
    /// Contains the results of a request to delete a context index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeleteIndexResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the delete index request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Tests the specified context's database connection.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class TestDatabaseConnectionRequest
    {
        StackHashClientData m_ClientData;
        int m_ContextId;
        StackHashSqlConfiguration m_SqlSettings;
        bool m_TestDatabaseExistence;
        String m_CabFolder;

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
        /// Id of the context to test. Set to -1 for testing outside a context.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Settings to test - can be null if context is not -1.
        /// </summary>
        [DataMember]
        public StackHashSqlConfiguration SqlSettings
        {
            get { return m_SqlSettings; }
            set { m_SqlSettings = value; }
        }

        /// <summary>
        /// true - test for database existence. False - don't.
        /// Only valid if ContextId == -1.
        /// </summary>
        [DataMember]
        public bool TestDatabaseExistence
        {
            get { return m_TestDatabaseExistence; }
            set { m_TestDatabaseExistence = value; }
        }

        /// <summary>
        /// The cab folder to test.
        /// </summary>
        [DataMember]
        public String CabFolder
        {
            get { return m_CabFolder; }
            set { m_CabFolder = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to test a context database connection.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class TestDatabaseConnectionResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashErrorIndexDatabaseStatus m_TestResult;
        String m_LastException;
        bool m_IsCabFolderAccessible;
        String m_CabFolderAccessLastException;

        /// <summary>
        /// The result of the test database connection attempt. This does 
        /// not include the actual test results.
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// The result of the test database connection response. 
        /// </summary>
        [DataMember]
        public StackHashErrorIndexDatabaseStatus TestResult
        {
            get { return m_TestResult; }
            set { m_TestResult = value; }
        }

        /// <summary>
        /// Last exception - can be null or empty.
        /// </summary>
        [DataMember]
        public String LastException
        {
            get { return m_LastException; }
            set { m_LastException = value; }
        }

        /// <summary>
        /// True if the cab folder is accessible. Otherwise false.
        /// </summary>
        [DataMember]
        public bool IsCabFolderAccessible
        {
            get { return m_IsCabFolderAccessible; }
            set { m_IsCabFolderAccessible = value; }
        }

        /// <summary>
        /// Cab Folder Access Last exception - can be null or empty.
        /// </summary>
        [DataMember]
        public String CabFolderAccessLastException
        {
            get { return m_CabFolderAccessLastException; }
            set { m_CabFolderAccessLastException = value; }
        }
    }

    
    /// <summary>
    /// Moves the specified context index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class MoveIndexRequest
    {
        StackHashClientData m_ClientData;
        int m_ContextId;
        String m_NewErrorIndexPath;
        String m_NewErrorIndexName;
        StackHashSqlConfiguration m_NewSqlSettings;

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
        /// Id of the context to move.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }
        /// <summary>
        /// Id of the context to move.
        /// </summary>
        [DataMember]
        public String NewErrorIndexPath
        {
            get { return m_NewErrorIndexPath; }
            set { m_NewErrorIndexPath = value; }
        }
        /// <summary>
        /// Id of the context to move.
        /// </summary>
        [DataMember]
        public String NewErrorIndexName
        {
            get { return m_NewErrorIndexName; }
            set { m_NewErrorIndexName = value; }
        }
        /// <summary>
        /// Sql configuration.
        /// </summary>
        [DataMember]
        public StackHashSqlConfiguration NewSqlSettings
        {
            get { return m_NewSqlSettings; }
            set { m_NewSqlSettings = value; }
        }

    }

    /// <summary>
    /// Contains the results of a request to move a context index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class MoveIndexResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the move index request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Copy the specified context index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CopyIndexRequest
    {
        StackHashClientData m_ClientData;
        int m_ContextId;
        ErrorIndexSettings m_DestinationErrorIndexSettings;
        StackHashSqlConfiguration m_SqlSettings;
        bool m_SwitchIndexWhenCopyComplete;
        bool m_DeleteSourceIndexWhenCopyComplete;

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
        /// Id of the context to move.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }
        /// <summary>
        /// Destination error index settings. The index must exist if it is Sql.
        /// </summary>
        [DataMember]
        public ErrorIndexSettings DestinationErrorIndexSettings
        {
            get { return m_DestinationErrorIndexSettings; }
            set { m_DestinationErrorIndexSettings = value; }
        }
        /// <summary>
        /// Sql settings if the destination index is Sql.
        /// </summary>
        [DataMember]
        public StackHashSqlConfiguration SqlSettings
        {
            get { return m_SqlSettings; }
            set { m_SqlSettings = value; }
        }

        /// <summary>
        /// True - index is switched to the copy when the copy is complete.
        /// </summary>
        [DataMember]
        public bool SwitchIndexWhenCopyComplete
        {
            get { return m_SwitchIndexWhenCopyComplete; }
            set { m_SwitchIndexWhenCopyComplete = value; }
        }

        /// <summary>
        /// True - Delete the index when 
        /// </summary>
        [DataMember]
        public bool DeleteSourceIndexWhenCopyComplete
        {
            get { return m_DeleteSourceIndexWhenCopyComplete; }
            set { m_DeleteSourceIndexWhenCopyComplete = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to copy a context index.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CopyIndexResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the copy index request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }
    /// <summary>
    /// Parameters required to identify the settings required.
    /// Not currently used.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetStackHashPropertiesRequest
    {
        StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get the StackHash service settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetStackHashPropertiesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashSettings m_Settings;

        /// <summary>
        /// The result of the GetProperties request. 
        /// If this is Failed then the Settings will be null.
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// The service properties. Only valid if the request was successful.
        /// </summary>
        [DataMember]
        public StackHashSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

    }

    /// <summary>
    /// Parameters required to get service status.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetStackHashServiceStatusRequest
    {
        StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get the StackHash service status.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetStackHashServiceStatusResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashStatus m_Status;

        /// <summary>
        /// The result of the GetProperties request. 
        /// If this is Failed then the Settings will be null.
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// The service status. Only valid if the request was successful.
        /// </summary>
        [DataMember]
        public StackHashStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

    }

    /// <summary>
    /// Parameters to the SetStackHashProperties service call.
    /// The new state of the parameters are set.
    /// Some paramaters cannot be set.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetStackHashPropertiesRequest
    {
        StackHashClientData m_ClientData;
        StackHashSettings m_Settings;

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
        /// The service properties to set.
        /// </summary>
        [DataMember]
        public StackHashSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to set the StackHash service settings.
    /// The "final" state of the settings is also returned following the set.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetStackHashPropertiesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashSettings m_Settings;

        /// <summary>
        /// The result of the SetProperties request. 
        /// If this is Failed then the Settings will still contain the current
        /// settings.
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// The service properties. Valid whether the set request was successful or not.
        /// </summary>
        [DataMember]
        public StackHashSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
    }

    /// <summary>
    /// Parameters to the CreateNewStackHashContext service call.
    /// The new state of the parameters are set.
    /// Some paramaters cannot be set.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CreateNewStackHashContextRequest
    {
        StackHashClientData m_ClientData;
        ErrorIndexType m_ErrorIndexType;

        
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
        /// Identifies the type of index required.
        /// </summary>
        [DataMember]
        public ErrorIndexType IndexType
        {
            get { return m_ErrorIndexType; }
            set { m_ErrorIndexType = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to create a new context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class CreateNewStackHashContextResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashContextSettings m_Settings;

        /// <summary>
        /// The result of the CreateNewProperties request. 
        /// If this is Failed then the Settings will still contain the current
        /// settings.
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// Default context settings.
        /// </summary>
        [DataMember]
        public StackHashContextSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
    }

    /// <summary>
    /// Parameters to the RemoveStackHashContext service call.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveStackHashContextRequest
    {
        private int m_ContextId;
        private bool m_ResetNextContextIdIfAppropriate;
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Id of the context to remove.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Id of the context to remove.
        /// </summary>
        [DataMember]
        public bool ResetNextContextIdIfAppropriate
        {
            get { return m_ResetNextContextIdIfAppropriate; }
            set { m_ResetNextContextIdIfAppropriate = value; }
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
    }


    /// <summary>
    /// Contains the results of a request to remove a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveStackHashContextResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the RemoveContext request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Parameters to the ActivateStackHashContext service call.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class ActivateStackHashContextRequest
    {
        private int m_ContextId;
        private StackHashClientData m_ClientData;


        /// <summary>
        /// Id of the context to activate.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
    }


    /// <summary>
    /// Contains the results of a request to activate a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class ActivateStackHashContextResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the ActivateContext request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Enables logging at the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class EnableLoggingRequest
    {
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to enable logging at the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class EnableLoggingResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the EnableLogging request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Enables reporting of stats to the StackHash web service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class EnableReportingRequest
    {
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }


    /// <summary>
    /// Response to attempt to enable reporting of stats to the StackHash web service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class EnableReportingResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the EnableReporting request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Disables reporting of stats to the StackHash web service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DisableReportingRequest
    {
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }


    /// <summary>
    /// Response to attempt to disable reporting of stats to the StackHash web service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DisableReportingResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the EnableReporting request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }
    
    /// <summary>
    /// Sets the proxy settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetProxyRequest
    {
        private StackHashClientData m_ClientData;
        private StackHashProxySettings m_ProxySettings;

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
        /// New proxy settings.
        /// </summary>
        [DataMember]
        public StackHashProxySettings ProxySettings
        {
            get { return m_ProxySettings; }
            set { m_ProxySettings = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to set the proxy.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetProxyResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the SetProxy request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }


    /// <summary>
    /// Sets the email settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetEmailSettingsRequest
    {
        private StackHashClientData m_ClientData;
        private StackHashEmailSettings m_EmailSettings;
        private int m_ContextId;

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
        /// Id of the context to remove.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// New email settings.
        /// </summary>
        [DataMember]
        public StackHashEmailSettings EmailSettings
        {
            get { return m_EmailSettings; }
            set { m_EmailSettings = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to set the email settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetEmailSettingsResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the SetEmailSettings request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    
    /// <summary>
    /// Sets the purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetPurgeOptionsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private ScheduleCollection m_Schedule;
        private StackHashPurgeOptionsCollection m_PurgeOptions;
        private bool m_SetAll;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// When the purge is to take place.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public ScheduleCollection Schedule
        {
            get { return m_Schedule; }
            set { m_Schedule = value; }
        }

        /// <summary>
        /// What is to be purged.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashPurgeOptionsCollection PurgeOptions
        {
            get { return m_PurgeOptions; }
            set { m_PurgeOptions = value; }
        }

        /// <summary>
        /// True - replaces, false - merges.
        /// </summary>
        [DataMember]
        public bool SetAll
        {
            get { return m_SetAll; }
            set { m_SetAll = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to set the purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetPurgeOptionsResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the SetProxy request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Gets the purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetPurgeOptionsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private StackHashPurgeObject m_PurgeObject;
        private int m_Id;
        private bool m_GetAll;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }


        /// <summary>
        /// The object to get.
        /// </summary>
        [DataMember]
        public StackHashPurgeObject PurgeObject
        {
            get { return m_PurgeObject; }
            set { m_PurgeObject = value; }
        }

        /// <summary>
        /// The ID of the object.
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        /// <summary>
        /// true - gets all purge options.
        /// false - gets purge options for object specified by PolictObject and Id.
        /// </summary>
        [DataMember]
        public bool GetAll
        {
            get { return m_GetAll; }
            set { m_GetAll = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to get the purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetPurgeOptionsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashPurgeOptionsCollection m_PurgeOptions;

        /// <summary>
        /// The result of the SetProxy request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// What is to be purged.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashPurgeOptionsCollection PurgeOptions
        {
            get { return m_PurgeOptions; }
            set { m_PurgeOptions = value; }
        }
    }


    /// <summary>
    /// Gets the active purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetActivePurgeOptionsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private int m_ProductId;
        private int m_FileId;
        private int m_EventId;
        private int m_CabId;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// The ID of the product.
        /// </summary>
        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        /// <summary>
        /// The ID of the file.
        /// </summary>
        [DataMember]
        public int FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }

        /// <summary>
        /// The ID of the event.
        /// </summary>
        [DataMember]
        public int EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }

        /// <summary>
        /// The ID of the cab.
        /// </summary>
        [DataMember]
        public int CabId
        {
            get { return m_CabId; }
            set { m_CabId = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to get the active purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetActivePurgeOptionsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashPurgeOptionsCollection m_PurgeOptions;

        /// <summary>
        /// The result of the SetProxy request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// Active purge options.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashPurgeOptionsCollection PurgeOptions
        {
            get { return m_PurgeOptions; }
            set { m_PurgeOptions = value; }
        }
    }
    
    
    /// <summary>
    /// Removes the purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemovePurgeOptionsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private StackHashPurgeObject m_PurgeObject;
        private int m_Id;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }


        /// <summary>
        /// The object to get.
        /// </summary>
        [DataMember]
        public StackHashPurgeObject PurgeObject
        {
            get { return m_PurgeObject; }
            set { m_PurgeObject = value; }
        }

        /// <summary>
        /// The ID of the object.
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to remove the purge options for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemovePurgeOptionsResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the SetProxy request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }
    
    /// <summary>
    /// Sets the data collection policy for a context.
    /// This is the number of cabs etc... to be downloaded.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetDataCollectionPolicyRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private bool m_SetAll;
        private StackHashCollectionPolicyCollection m_PolicyCollection;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// True - replace entire structure.
        /// </summary>
        [DataMember]
        public bool SetAll
        {
            get { return m_SetAll; }
            set { m_SetAll = value; }
        }

        /// <summary>
        /// New policy.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashCollectionPolicyCollection PolicyCollection
        {
            get { return m_PolicyCollection; }
            set { m_PolicyCollection = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to set the collection policy.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetDataCollectionPolicyResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the Set the database collection policy. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Gets the data collection policy for the specified object. 
    /// If ID is 0 then gets all policies.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDataCollectionPolicyRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private StackHashCollectionObject m_RootObject;
        private int m_Id;
        private bool m_GetAll;
        private StackHashCollectionObject m_ObjectToCollect;
        private StackHashCollectionObject m_ConditionObject;

 
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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Object for which policy is required (global, product, file, cab).
        /// </summary>
        [DataMember]
        public StackHashCollectionObject RootObject
        {
            get { return m_RootObject; }
            set { m_RootObject = value; }
        }

        /// <summary>
        /// Object ID.
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        /// <summary>
        /// true - gets all purge options.
        /// false - gets purge options for object specified by PolictObject and Id.
        /// </summary>
        [DataMember]
        public bool GetAll
        {
            get { return m_GetAll; }
            set { m_GetAll = value; }
        }

        /// <summary>
        /// Condition object.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ConditionObject
        {
            get { return m_ConditionObject; }
            set { m_ConditionObject = value; }
        }

        /// <summary>
        /// Object to collect.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ObjectToCollect
        {
            get { return m_ObjectToCollect; }
            set { m_ObjectToCollect = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to get the collection policy.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDataCollectionPolicyResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashCollectionPolicyCollection m_PolicyCollection;

        /// <summary>
        /// The result of getting the database collection policy. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// Existing policy.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashCollectionPolicyCollection PolicyCollection
        {
            get { return m_PolicyCollection; }
            set { m_PolicyCollection = value; }
        }
    }

    /// <summary>
    /// Gets the active data collection policy for the specified object. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetActiveDataCollectionPolicyRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private int m_ProductId;
        private int m_FileId;
        private int m_EventId;
        private int m_CabId;
        private StackHashCollectionObject m_ObjectToCollect;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// The ID of the product.
        /// </summary>
        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        /// <summary>
        /// The ID of the file.
        /// </summary>
        [DataMember]
        public int FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }

        /// <summary>
        /// The ID of the event.
        /// </summary>
        [DataMember]
        public int EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }

        /// <summary>
        /// The ID of the cab.
        /// </summary>
        [DataMember]
        public int CabId
        {
            get { return m_CabId; }
            set { m_CabId = value; }
        }

        /// <summary>
        /// Object to collect.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ObjectToCollect
        {
            get { return m_ObjectToCollect; }
            set { m_ObjectToCollect = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to get the active collection policy.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetActiveDataCollectionPolicyResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashCollectionPolicyCollection m_PolicyCollection;

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
        /// Existing active policy.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashCollectionPolicyCollection PolicyCollection
        {
            get { return m_PolicyCollection; }
            set { m_PolicyCollection = value; }
        }
    }

    /// <summary>
    /// Removes the data collection policy for the specified object. 
    /// If ID is 0 then deletes all policies.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveDataCollectionPolicyRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private StackHashCollectionObject m_RootObject;
        private int m_Id;
        private StackHashCollectionObject m_ObjectToCollect;
        private StackHashCollectionObject m_ConditionObject;

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
        /// Context to set.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Object for which policy is required (global, product, file, cab).
        /// </summary>
        [DataMember]
        public StackHashCollectionObject RootObject
        {
            get { return m_RootObject; }
            set { m_RootObject = value; }
        }

        /// <summary>
        /// Object ID.
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        /// <summary>
        /// Object to compare.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ConditionObject
        {
            get { return m_ConditionObject; }
            set { m_ConditionObject = value; }
        }

        /// <summary>
        /// Object to collect.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ObjectToCollect
        {
            get { return m_ObjectToCollect; }
            set { m_ObjectToCollect = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to remove the collection policy.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveDataCollectionPolicyResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of removing a data collection policy. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Disables logging at the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DisableLoggingRequest
    {
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to disable logging at the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DisableLoggingResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the EnableLogging request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Parameters to the DeactivateStackHashContext service call.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeactivateStackHashContextRequest
    {
        private int m_ContextId;
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Id of the context to deactivate.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
    }


    /// <summary>
    /// Contains the results of a request to deactivate a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeactivateStackHashContextResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the DeactivateContext request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Gets a copy of the current license data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetLicenseDataRequest
    {
        private StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to get the license data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetLicenseDataResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashLicenseData m_LicenseData;
        StackHashLicenseUsage m_LicenseUsage;

        /// <summary>
        /// The result of request. 
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }

        /// <summary>
        /// The retrieved license data. 
        /// </summary>
        [DataMember]
        public StackHashLicenseData LicenseData
        {
            get { return m_LicenseData; }
            set { m_LicenseData = value; }
        }

        /// <summary>
        /// The retrieved license usage. 
        /// </summary>
        [DataMember]
        public StackHashLicenseUsage LicenseUsage
        {
            get { return m_LicenseUsage; }
            set { m_LicenseUsage = value; }
        }
    }

    /// <summary>
    /// Sets the license ID.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetLicenseRequest
    {
        private StackHashClientData m_ClientData;
        private String m_LicenseId;

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
        /// License ID
        /// </summary>
        [DataMember]
        public String LicenseId
        {
            get { return m_LicenseId; }
            set { m_LicenseId = value; }
        }
    }


    /// <summary>
    /// Response to attempt to sets the license ID.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetLicenseResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashLicenseData m_LicenseData;

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
        /// The retrieved license data. 
        /// </summary>
        [DataMember]
        public StackHashLicenseData LicenseData
        {
            get { return m_LicenseData; }
            set { m_LicenseData = value; }
        }
    }

    /// <summary>
    /// Run the bug tracker task.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunBugTrackerTaskRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;

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
        /// Identifies the context for which the request is destined.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }
    }


    /// <summary>
    /// Response to attempt to sets the license ID.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunBugTrackerTaskResponse
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
    /// Gets diagnostics about loaded and not-loaded Bug Tracker DLLs.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetBugTrackerPlugInDiagnosticsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private String m_PlugInName;


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
        /// Identifies the context for which the request is destined.
        /// If -1 then then the main DLL diagnostics are returned.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// The name of the plugin whose diagnostics are required.
        /// If null then all plugin diagnostics are returned.
        /// </summary>
        [DataMember]
        public String PlugInName
        {
            get { return m_PlugInName; }
            set { m_PlugInName = value; }
        }
    }


    /// <summary>
    /// Response to attempt to get bug tracker diagnostics.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetBugTrackerPlugInDiagnosticsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashBugTrackerPlugInDiagnosticsCollection m_PlugInDiagnostics;

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
        /// Full diagnostics of all plugins - loaded or not. 
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugTrackerPlugInDiagnosticsCollection BugTrackerPlugInDiagnostics
        {
            get { return m_PlugInDiagnostics; }
            set { m_PlugInDiagnostics = value; }
        }

    }

    /// <summary>
    /// Gets bug tracker plugin settings for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetContextBugTrackerPlugInSettingsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;

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
        /// Identifies the context for which the request is destined.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }
    }


    /// <summary>
    /// Response to attempt to get bug tracker context settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetContextBugTrackerPlugInSettingsResponse
    {
        private StackHashServiceResultData m_ResultData;
        private StackHashBugTrackerPlugInSettings m_PlugInSettings;

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
        /// Full settings for the context.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugTrackerPlugInSettings BugTrackerPlugInSettings
        {
            get { return m_PlugInSettings; }
            set { m_PlugInSettings = value; }
        }

    }


    /// <summary>
    /// Sets bug tracker plugin settings for a context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetContextBugTrackerPlugInSettingsRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ContextId;
        private StackHashBugTrackerPlugInSettings m_PlugInSettings;

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
        /// Identifies the context for which the request is destined.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Full settings for the context.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugTrackerPlugInSettings BugTrackerPlugInSettings
        {
            get { return m_PlugInSettings; }
            set { m_PlugInSettings = value; }
        }

    }


    /// <summary>
    /// Response to attempt to set bug tracker context settings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetContextBugTrackerPlugInSettingsResponse
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
    /// Parameters to the StartSynchronization service call.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StartSynchronizationRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        bool m_ForceResynchronize;
        bool m_JustSyncProducts;
        StackHashProductSyncDataCollection m_ProductsToSynchronize;

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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        /// <summary>
        /// True - forces a resync, false - results from last successful sync time.
        /// </summary>
        [DataMember]
        public bool ForceResynchronize
        {
            get { return m_ForceResynchronize; }
            set { m_ForceResynchronize = value; }
        }

        /// <summary>
        /// True - only sync the product list, false - syncs products according to enabled state.
        /// </summary>
        [DataMember]
        public bool JustSyncProducts
        {
            get { return m_JustSyncProducts; }
            set { m_JustSyncProducts = value; }
        }

        /// <summary>
        /// List of products to sync. If null then this field is ignored.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        [DataMember]
        public StackHashProductSyncDataCollection ProductsToSynchronize
        {
            get { return m_ProductsToSynchronize; }
            set { m_ProductsToSynchronize = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to start the synchronizations service on
    /// the server.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StartSynchronizationResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the request.
        /// Success means that the service has started the sync (not necessarily finished it).
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Parameters to the StartSynchronization service call.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AbortSynchronizationRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;

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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }
    

    /// <summary>
    /// Contains the results of a request to start the synchronizations service on
    /// the server.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AbortSynchronizationResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the request.
        /// Success means that the service has started the sync (not necessarily finished it).
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }

    /// <summary>
    /// Parameters to abort a task in the service.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AbortTaskRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashTaskType m_TaskType;

        /// <summary>
        /// The context ID.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Task to be aborted.
        /// </summary>
        [DataMember]
        public StackHashTaskType TaskType
        {
            get { return m_TaskType; }
            set { m_TaskType = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to abort a task.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AbortTaskResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// The result of the request.
        /// Success means that the service has started the sync (not necessarily finished it).
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }


    /// <summary>
    /// Contains request to get a list of debugger scripts.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebuggerScriptNamesRequest
    {
        StackHashClientData m_ClientData;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get a list of debugger scripts.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebuggerScriptNamesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashScriptFileDataCollection m_ScriptFileData;

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
        /// Names of all known scripts.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        [DataMember]
        public StackHashScriptFileDataCollection ScriptFileData
        {
            get { return m_ScriptFileData; }
            set { m_ScriptFileData = value; }
        }
    }

    /// <summary>
    /// Parameters to the AddDebuggerScript.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AddDebuggerScriptRequest
    {
        bool m_Overwrite;
        StackHashClientData m_ClientData;
        StackHashScriptSettings m_Script;

        /// <summary>
        /// Determines if the script should be overwritten if it exists already.
        /// </summary>
        [DataMember]
        public bool Overwrite
        {
            get { return m_Overwrite; }
            set { m_Overwrite = value; }
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
        /// The new script.
        /// </summary>
        [DataMember]
        public StackHashScriptSettings Script
        {
            get { return m_Script; }
            set { m_Script = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to add a debugger script.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AddDebuggerScriptResponse
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
    /// Parameters to the RemoveDebuggerScript.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveDebuggerScriptRequest
    {
        String m_ScriptName;
        StackHashClientData m_ClientData;

        /// <summary>
        /// Name of script to remove. If it doesn't exist this call will silently succeed.
        /// </summary>
        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
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
    }

    /// <summary>
    /// Contains the results of a request to remove a debugger script.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveDebuggerScriptResponse
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
    /// Parameters to the RenameDebuggerScript.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RenameDebuggerScriptRequest
    {
        StackHashClientData m_ClientData;
        String m_OriginalScriptName;
        String m_NewScriptName;

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
        /// Name of script to rename. 
        /// </summary>
        [DataMember]
        public String OriginalScriptName
        {
            get { return m_OriginalScriptName; }
            set { m_OriginalScriptName = value; }
        }

        /// <summary>
        /// New name
        /// </summary>
        [DataMember]
        public String NewScriptName
        {
            get { return m_NewScriptName; }
            set { m_NewScriptName = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to rename a debugger script.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RenameDebuggerScriptResponse
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
    /// Parameters to the RenameDebuggerScript.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebuggerScriptRequest
    {
        StackHashClientData m_ClientData;
        String m_ScriptName;

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
        /// Name of script to get. 
        /// </summary>
        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to rename a debugger script.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebuggerScriptResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashScriptSettings m_ScriptSettings;

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
        /// Full script settings.
        /// </summary>
        [DataMember]
        public StackHashScriptSettings ScriptSettings
        {
            get { return m_ScriptSettings; }
            set { m_ScriptSettings = value; }
        }
    }


    /// <summary>
    /// Parameters to the RunDebuggerScript.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunDebuggerScriptRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        String m_ScriptName;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        String m_DumpFileName;


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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        /// <summary>
        /// Name of script to get. 
        /// </summary>
        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
        }

        /// <summary>
        /// Product.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// Cab.
        /// </summary>
        [DataMember]
        public String DumpFileName
        {
            get { return m_DumpFileName; }
            set { m_DumpFileName = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to run a debugger script on a cab
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunDebuggerScriptResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashScriptResult m_ScriptResult;

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
        /// Full script settings.
        /// </summary>
        [DataMember]
        public StackHashScriptResult ScriptResult
        {
            get { return m_ScriptResult; }
            set { m_ScriptResult = value; }
        }
    }

    /// <summary>
    /// Parameters to RunDebuggerScriptAsync.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunDebuggerScriptAsyncRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        String m_DumpFileName;
        StackHashScriptNamesCollection m_ScriptsToRun;


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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        /// <summary>
        /// Product.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// Dump filename - can be null in which case the largest dump file is chosen.
        /// </summary>
        [DataMember]
        public String DumpFileName
        {
            get { return m_DumpFileName; }
            set { m_DumpFileName = value; }
        }

        /// <summary>
        /// Scripts to run on the cab.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        [DataMember]
        public StackHashScriptNamesCollection ScriptsToRun
        {
            get { return m_ScriptsToRun; }
            set { m_ScriptsToRun = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to run a debugger script on a cab asynchronously.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunDebuggerScriptAsyncResponse
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
    /// Parameters to the GetDebugResultFiles.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebugResultFilesRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;


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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        /// <summary>
        /// Product.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get a list of debugger script run results.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebugResultFilesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashScriptResultFiles m_ResultFiles;

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
        /// Full script settings.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashScriptResultFiles ResultFiles
        {
            get { return m_ResultFiles; }
            set { m_ResultFiles = value; }
        }
    }

    /// <summary>
    /// Parameters to the GetDebugResult.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebugResultRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        String m_ScriptName;


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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }

        /// <summary>
        /// Product.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// ScriptName.
        /// </summary>
        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to get the result of a debug run.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetDebugResultResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashScriptResult m_Result;

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
        /// Full script settings.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashScriptResult Result
        {
            get { return m_Result; }
            set { m_Result = value; }
        }
    }

    /// <summary>
    /// Parameters to the RemoveScriptResult.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveScriptResultRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        String m_ScriptName;


        /// <summary>
        /// A company may have several WinQual
        /// registrations. This field identifies the correct one.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Product.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// ScriptName ro remove
        /// </summary>
        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to remove a script result file.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RemoveScriptResultResponse
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
    /// Parameters to the SetProductSynchronizationState.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetProductSynchronizationStateRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        int m_ProductId;
        bool m_Enabled;

        /// <summary>
        /// A company may have several WinQual
        /// registrations. This field identifies the correct one.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Product.
        /// </summary>
        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        /// <summary>
        /// True - enable syncing.
        /// </summary>
        [DataMember]
        public bool Enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to set the product synchronization state.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetProductSynchronizationStateResponse
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
    /// Parameters to the SetProductSynchronizationData.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetProductSynchronizationDataRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProductSyncData m_ProductSyncData;

        /// <summary>
        /// A company may have several WinQual
        /// registrations. This field identifies the correct one.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Product.
        /// </summary>
        [DataMember]
        public StackHashProductSyncData SyncData
        {
            get { return m_ProductSyncData; }
            set { m_ProductSyncData = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to set the product synchronization data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetProductSynchronizationDataResponse
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
    /// Parameters to the RunWinQualLogOn.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunWinQualLogOnRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        String m_UserName;
        String m_Password;

        /// <summary>
        /// A company may have several WinQual
        /// registrations. This field identifies the correct one.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// WinQual username.
        /// </summary>
        [DataMember]
        public String UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        /// <summary>
        /// WinQual password
        /// </summary>
        [DataMember]
        public String Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request log on to WinQual.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunWinQualLogOnResponse
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
    /// Parameters to the UploadMappingFileRequest.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class UploadMappingFileRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        String m_MappingFileData;

        /// <summary>
        /// ID of the context to which the mapping belongs.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Mapping file data.
        /// </summary>
        [DataMember]
        public String MappingFileData
        {
            get { return m_MappingFileData; }
            set { m_MappingFileData = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to upload product mapping data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class UploadMappingFileResponse
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
    /// Parameters to the GetStatusMappings.
    /// e.g. WorkFlow and Groups.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetStatusMappingsRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashMappingType m_MappingType;

        /// <summary>
        /// ID of the context to which the mapping belongs.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashMappingType MappingType
        {
            get { return m_MappingType; }
            set { m_MappingType = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to get the status mappings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetStatusMappingsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashMappingType m_MappingType;
        StackHashMappingCollection m_StatusMappings;

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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashMappingType MappingType
        {
            get { return m_MappingType; }
            set { m_MappingType = value; }
        }

        /// <summary>
        /// Status mapping values.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashMappingCollection StatusMappings
        {
            get { return m_StatusMappings; }
            set { m_StatusMappings = value; }
        }
    }


    /// <summary>
    /// Parameters to the SetStatusMappings.
    /// e.g. WorkFlow and Groups.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetStatusMappingsRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashMappingType m_MappingType;
        StackHashMappingCollection m_StatusMappings;

        /// <summary>
        /// ID of the context to which the mapping belongs.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashMappingType MappingType
        {
            get { return m_MappingType; }
            set { m_MappingType = value; }
        }

        /// <summary>
        /// Status mapping values.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashMappingCollection StatusMappings
        {
            get { return m_StatusMappings; }
            set { m_StatusMappings = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to get the status mappings.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetStatusMappingsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashMappingType m_MappingType;

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
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashMappingType MappingType
        {
            get { return m_MappingType; }
            set { m_MappingType = value; }
        }

    }

    
    
    /// <summary>
    /// Callback notification interface. Informs the client of server activity.
    /// </summary>
    public interface IAdminNotificationEvents
    {
        /// <summary>
        /// Callback from the service to the client informing of progress.
        /// </summary>
        /// <param name="adminReport">Info about async operation status.</param>
        [OperationContract(IsOneWay = true)]
        void AdminProgressEvent(StackHashAdminReport adminReport);
    }

    /// <summary>
    /// Sets the client bumping timeout.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetClientTimeoutRequest
    {
        private StackHashClientData m_ClientData;
        private int m_ClientTimeoutInSeconds;

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
        /// Sets the client timeout for bumping.
        /// </summary>
        [DataMember]
        public int ClientTimeoutInSeconds
        {
            get { return m_ClientTimeoutInSeconds; }
            set { m_ClientTimeoutInSeconds = value; }
        }
    }


    /// <summary>
    /// Response to an attempt to set the client timeout.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetClientTimeoutResponse
    {
        StackHashServiceResultData m_ResultData;

        /// <summary>
        /// Result of setting the client timeout.
        /// </summary>
        [DataMember]
        public StackHashServiceResultData ResultData
        {
            get { return m_ResultData; }
            set { m_ResultData = value; }
        }
    }


    /// <summary>
    /// Service Contract provided to the StackHash client. This contract permits the client to 
    /// manage the setup and other administrative functions of the StackHash service.
    /// Functionality includes:
    ///     1) Setting the username and password credentials for login to WinQual
    /// </summary>
    [ServiceContract(Namespace = "http://www.cucku.com/stackhash/2010/02/17",
        CallbackContract = typeof(IAdminNotificationEvents),
        SessionMode=SessionMode.Required)]
    [SuppressMessage("Microsoft.Maintainability", "CA1506")]
    public interface IAdminContract
    {
        /// <summary>
        /// Register for admin notification events. You will recieve notification of all 
        /// asynchronous events from the service.
        /// Upon Registration an AdminRegister event will be sent to the callback. Note that the
        /// event may arrive at the client some time after the client has returned from the call to RegisterForNotifications
        /// because the RegisterForNotifications call IsOneWay.
        /// No event is sent upon de-registration.
        /// This function is ONE-WAY because it calls back to the client. This would cause a deadlock
        /// if not one way.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        [OperationContract(IsOneWay = true)]
        void RegisterForNotifications(RegisterRequest requestData);

        /// <summary>
        /// Gets the settings associated with a particular context (profile).
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RestartResponse Restart(RestartRequest requestData);

        /// <summary>
        /// Checks the client version against the service version.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        CheckVersionResponse CheckVersion(CheckVersionRequest requestData);


        /// <summary>
        /// Tests the database connection for a particular context.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        TestDatabaseConnectionResponse TestDatabaseConnection(TestDatabaseConnectionRequest requestData);

        
        /// <summary>
        /// Deletes the index associated with the specified context.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DeleteIndexResponse DeleteIndex(DeleteIndexRequest requestData);

        /// <summary>
        /// Moves or renames an error index.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        MoveIndexResponse MoveIndex(MoveIndexRequest requestData);

        /// <summary>
        /// Copies the current context index to the specified index.
        /// This call can be used to backup the index - or to copy from XML to SQL and vice versa.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        CopyIndexResponse CopyIndex(CopyIndexRequest requestData);
        
        /// <summary>
        /// Gets the current admin settings for the server.
        /// </summary>
        /// <returns>Current settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetStackHashPropertiesResponse GetStackHashSettings(GetStackHashPropertiesRequest requestData);

        /// <summary>
        /// Gets the current admin settings for the server.
        /// </summary>
        /// <returns>Current settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetStackHashServiceStatusResponse GetServiceStatus(GetStackHashServiceStatusRequest requestData);

        
        /// <summary>
        /// Sets the current admin settings for the server.
        /// Returns the current state of the settings (whether this call succeeds or not).
        /// </summary>
        /// <returns>Settings to set.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetStackHashPropertiesResponse SetStackHashSettings(SetStackHashPropertiesRequest requestData);

        /// <summary>
        /// Adds a new set of context settings. Allocates a new ID and returns the default settings.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        CreateNewStackHashContextResponse CreateNewStackHashContext(CreateNewStackHashContextRequest requestData);

        /// <summary>
        /// Removes a stackhash context.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RemoveStackHashContextResponse RemoveStackHashContext(RemoveStackHashContextRequest requestData);

        /// <summary>
        /// Activate a stackhash context.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        ActivateStackHashContextResponse ActivateStackHashContext(ActivateStackHashContextRequest requestData);

        /// <summary>
        /// Deactivate a stackhash context.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DeactivateStackHashContextResponse DeactivateStackHashContext(DeactivateStackHashContextRequest requestData);

        /// <summary>
        /// Enable trace logging at service.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        EnableLoggingResponse EnableLogging(EnableLoggingRequest requestData);

        /// <summary>
        /// Disables trace logging at the service.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DisableLoggingResponse DisableLogging(DisableLoggingRequest requestData);

        /// <summary>
        /// Enable reporting of stats to the StackHash web service.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        EnableReportingResponse EnableReporting(EnableReportingRequest requestData);

        /// <summary>
        /// Disable reporting of stats to the StackHash web service.
        /// </summary>
        /// <returns>Default settings.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DisableReportingResponse DisableReporting(DisableReportingRequest requestData);

        /// <summary>
        /// Starts the synchronisation task on the service.
        /// This will use the server settings to download the CAB data to the ErrorIndex folder.
        /// Note that this is an asychronous task.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        StartSynchronizationResponse StartSynchronization(StartSynchronizationRequest requestData);

        /// <summary>
        /// Aborts the synchronisation task running on the service if there is one.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        AbortSynchronizationResponse AbortSynchronization(AbortSynchronizationRequest requestData);


        /// <summary>
        /// Gets a list of current debugger scripts by name.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetDebuggerScriptNamesResponse GetDebuggerScriptNames(GetDebuggerScriptNamesRequest requestData);


        /// <summary>
        /// Adds a new script. If the script exists already it will be overwritten if the 
        /// parameters indicate overwrite.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        AddDebuggerScriptResponse AddScript(AddDebuggerScriptRequest requestData);

        /// <summary>
        /// Removes the named debugger script. If it doesn't exist then this call has no
        /// effect.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RemoveDebuggerScriptResponse RemoveScript(RemoveDebuggerScriptRequest requestData);

        /// <summary>
        /// Renames the specified script to a new name.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RenameDebuggerScriptResponse RenameScript(RenameDebuggerScriptRequest requestData);

        /// <summary>
        /// Gets the script settings for the specified script name.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetDebuggerScriptResponse GetScript(GetDebuggerScriptRequest requestData);

        /// <summary>
        /// Runs a named script on a product, file, event, cab.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RunDebuggerScriptResponse RunScript(RunDebuggerScriptRequest requestData);

        /// <summary>
        /// Runs a named script on a product, file, event, cab - asynchronously.
        /// The caller should listen for an admin event and then call GetScriptResult.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RunDebuggerScriptAsyncResponse RunScriptAsync(RunDebuggerScriptAsyncRequest requestData);

        /// <summary>
        /// Gets a list of debug result files available for a product, file, event, cab.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetDebugResultFilesResponse GetDebugResultFiles(GetDebugResultFilesRequest requestData);

        /// <summary>
        /// Gets results for the specified test run.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetDebugResultResponse GetDebugResult(GetDebugResultRequest requestData);

        /// <summary>
        /// Removes the result of the specified test run.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RemoveScriptResultResponse RemoveScriptResult(RemoveScriptResultRequest requestData);

        /// <summary>
        /// Sets the product synchronization state.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetProductSynchronizationStateResponse SetProductSynchronizationState(SetProductSynchronizationStateRequest requestData);


        /// <summary>
        /// Sets the product synchronization data.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetProductSynchronizationDataResponse SetProductSynchronizationData(SetProductSynchronizationDataRequest requestData);


        /// <summary>
        /// Runs a WinQual log on using the specified username and password.
        /// Note that a context must exist - although it can be either active or inactive.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RunWinQualLogOnResponse RunWinQualLogOn(RunWinQualLogOnRequest requestData);

        /// <summary>
        /// Sets the service proxy details.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetProxyResponse SetProxy(SetProxyRequest requestData);

        /// <summary>
        /// Sets the email settings for admin reports.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetEmailSettingsResponse SetEmailSettings(SetEmailSettingsRequest requestData);

        
        /// <summary>
        /// Sets the purge options for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetPurgeOptionsResponse SetPurgeOptions(SetPurgeOptionsRequest requestData);

        /// <summary>
        /// Gets the purge options for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetPurgeOptionsResponse GetPurgeOptions(GetPurgeOptionsRequest requestData);

        /// <summary>
        /// Gets the active purge options for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetActivePurgeOptionsResponse GetActivePurgeOptions(GetActivePurgeOptionsRequest requestData);

        /// <summary>
        /// Removes the purge options for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RemovePurgeOptionsResponse RemovePurgeOptions(RemovePurgeOptionsRequest requestData);
        
        /// <summary>
        /// Sets the data collection policy.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetDataCollectionPolicyResponse SetDataCollectionPolicy(SetDataCollectionPolicyRequest requestData);

        /// <summary>
        /// Gets the data collection policy for the specified object - or all policies.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetDataCollectionPolicyResponse GetDataCollectionPolicy(GetDataCollectionPolicyRequest requestData);

        /// <summary>
        /// Gets the active data collection policy for the specified object..
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetActiveDataCollectionPolicyResponse GetActiveDataCollectionPolicy(GetActiveDataCollectionPolicyRequest requestData);

        
        /// <summary>
        /// Remove data collection policy for the specified object - or all policies.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RemoveDataCollectionPolicyResponse RemoveDataCollectionPolicy(RemoveDataCollectionPolicyRequest requestData);

        /// <summary>
        /// Gets the current license data for the application.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetLicenseDataResponse GetLicenseData(GetLicenseDataRequest requestData);

        /// <summary>
        /// Set the current license.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetLicenseResponse SetLicense(SetLicenseRequest requestData);

        /// <summary>
        /// Starts the bug tracker task - or pings it to start work if it is already running.
        /// This might be necessary to "restart" the task processing after it has stopped due to an exception 
        /// calling the bug tracker plugins.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RunBugTrackerTaskResponse RunBugTracker(RunBugTrackerTaskRequest requestData);

        /// <summary>
        /// Gets the BugTracker plugin details.
        /// This includes DLLs that failed to load.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetBugTrackerPlugInDiagnosticsResponse GetBugTrackerDiagnostics(GetBugTrackerPlugInDiagnosticsRequest requestData);

        /// <summary>
        /// Gets the BugTracker plugin settings for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetContextBugTrackerPlugInSettingsResponse GetContextBugTrackerPlugInSettings(GetContextBugTrackerPlugInSettingsRequest requestData);

        /// <summary>
        /// Sets the BugTracker plugin settings for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetContextBugTrackerPlugInSettingsResponse SetContextBugTrackerPlugInSettings(SetContextBugTrackerPlugInSettingsRequest requestData);

        /// <summary>
        /// Aborts the specified task in the service. Not all task types can be aborted.
        /// </summary>
        /// <param name="requestData"></param>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        AbortTaskResponse AbortTask(AbortTaskRequest requestData);

        
        /// <summary>
        /// Sets the client bumping timeout.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetClientTimeoutResponse SetClientTimeout(SetClientTimeoutRequest requestData);

        /// <summary>
        /// Uploads the specified mapping file data.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        UploadMappingFileResponse UploadMappingFile(UploadMappingFileRequest requestData);

        /// <summary>
        /// Get status mapping request.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetStatusMappingsResponse GetStatusMappings(GetStatusMappingsRequest requestData);

        /// <summary>
        /// Set status mapping request.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetStatusMappingsResponse SetStatusMappings(SetStatusMappingsRequest requestData);
    }
}
