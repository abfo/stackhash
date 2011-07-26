using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;


using StackHashServiceContracts;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashServiceImplementation
{
    /// <summary>
    /// One service object created for each client session.
    /// Might need to split off the cab implementation object to allow concurrent calls.
    /// Sessions and Streaming
    /// When you have a large amount of data to transfer, the streaming transfer mode in WCF 
    /// is a feasible alternative to the default behavior of buffering and processing messages 
    /// in memory in their entirety. You may get unexpected behavior when streaming calls with 
    /// a session-based binding. All streaming calls are made through a single channel 
    /// (the datagram channel) that does not support sessions even if the binding being used is 
    /// configured to use sessions. If multiple clients make streaming calls to the same service 
    /// object over a session-based binding, and the service object's concurrency mode is set to 
    /// single and its instance context mode is set to PerSession, all calls must go through the 
    /// datagram channel and so only one call is processed at a time. One or more clients may then 
    /// time out. You can work around this issue by either setting the service object's InstanceContextMode 
    /// to PerCall or Concurrency to multiple.
    /// </summary>

    [SuppressMessage("Microsoft.Maintainability", "CA1506")]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class InternalService : IAdminContract, IProjectsContract, ICabContract, IServiceBehavior, ITestContract
    {
        #region Fields

        private const String s_OperationSuccessful = "Operation Successful";
        private static Dictionary<Guid, RegisteredClient> s_AdminCallbacks =
            new Dictionary<Guid, RegisteredClient>();

        #endregion Fields

        #region Registration

        /// <summary>
        /// Called to notify ALL clients of an event.
        /// </summary>
        /// <param name="adminReport">The report to send.</param>
        /// <param name="sendToAll">True - send to all clients. False send to specific client.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void OnAdminNotification(StackHashAdminReport adminReport, bool sendToAll)
        {
            if (adminReport == null)
                throw new ArgumentNullException("adminReport");

            adminReport.ServiceGuid = StaticObjects.TheStaticObjects.TheController.ServiceGuid;
            adminReport.ServiceHost = Environment.MachineName;

            if (!sendToAll)
            {
                IAdminNotificationEvents eventHandler = null;

                Monitor.Enter(s_AdminCallbacks);
                try
                {
                    if (s_AdminCallbacks.ContainsKey(adminReport.ClientData.ApplicationGuid))
                    {
                        eventHandler = s_AdminCallbacks[adminReport.ClientData.ApplicationGuid].ClientCallback;
                    }
                }
                finally
                {
                    Monitor.Exit(s_AdminCallbacks);
                }

                if (eventHandler != null)
                {
                    try
                    {
                        eventHandler.AdminProgressEvent(adminReport);
                    }
                    catch (System.Exception)
                    {
                        Monitor.Enter(s_AdminCallbacks);
                        try
                        {
                            if (s_AdminCallbacks.ContainsKey(adminReport.ClientData.ApplicationGuid))
                                s_AdminCallbacks.Remove(adminReport.ClientData.ApplicationGuid);
                        }
                        finally
                        {
                            Monitor.Exit(s_AdminCallbacks);
                        }
                    }
                }

            }
            else
            {
                List<KeyValuePair<Guid, RegisteredClient>> oldObjects = new List<KeyValuePair<Guid, RegisteredClient>>();
                List<KeyValuePair<Guid, RegisteredClient>> callbacks = new List<KeyValuePair<Guid, RegisteredClient>>();

                Monitor.Enter(s_AdminCallbacks);
                try
                {
                    // Make a copy of the callbacks.
                    foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                    {
                        callbacks.Add(obj);
                    }
                }
                finally
                {
                    Monitor.Exit(s_AdminCallbacks);
                }

                foreach (KeyValuePair<Guid, RegisteredClient> obj in callbacks)
                {
                    try
                    {
                        obj.Value.ClientCallback.AdminProgressEvent(adminReport);
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "On Admin Notification", ex);
                        ; // Ignore - the connection may have gone down.
                        oldObjects.Add(obj);
                    }
                }

                foreach (KeyValuePair<Guid, RegisteredClient> obj in oldObjects)
                {
                    Monitor.Enter(s_AdminCallbacks);

                    try
                    {
                        if (s_AdminCallbacks.ContainsKey(obj.Key))
                        {
                            // Remove the callback from the list.
                            s_AdminCallbacks.Remove(obj.Key);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(s_AdminCallbacks);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the specified user is registered or not.
        /// A user may be logged in on several machines (clients).
        /// </summary>
        /// <param name="clientName">Name of the user.</param>
        /// <returns>True - user logged in, False - user not logged in.</returns>
        private static bool isUserRegistered(String clientName)
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                {
                    if (obj.Value.ClientData.ClientName == clientName)
                        return true;
                }
                return false;
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }            
        }

        private static StackHashClientLicenseUsageCollection getClientData()
        {
            StackHashClientLicenseUsageCollection clientUsageData = new StackHashClientLicenseUsageCollection();

            Monitor.Enter(s_AdminCallbacks);

            try
            {
                foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                {
                    StackHashClientLicenseUsage clientData = new StackHashClientLicenseUsage();
                    clientData.ClientData = obj.Value.ClientData;
                    clientData.ClientConnectTime = obj.Value.FirstRegisteredTime;
                    clientData.LastAccessTime = obj.Value.LastAccessTime;
                    clientUsageData.Add(clientData);
                }

                return clientUsageData;
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }            
        }


        /// <summary>
        /// Determines if the specified client is registered.
        /// </summary>
        /// <param name="clientGuid">ID of the client.</param>
        /// <returns>True - if registered, False - if not registered.</returns>
        private static bool isClientRegistered(Guid clientGuid)
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                return s_AdminCallbacks.ContainsKey(clientGuid);
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }


        /// <summary>
        /// Determines how many different users are logged on at present.
        /// </summary>
        /// <returns>Number of logged on users.</returns>
        private static int registeredUserCount()
        {
            Dictionary<String, bool> users = new Dictionary<string, bool>();

            Monitor.Enter(s_AdminCallbacks);

            try
            {
                foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                {
                    if (!users.ContainsKey(obj.Value.ClientData.ClientName))
                    {
                        if (!obj.Value.ClientData.ClientName.Contains("_StackHashDBConfig"))
                            users[obj.Value.ClientData.ClientName] = true;
                    }
                }
                return users.Count;
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }

        
        /// <summary>
        /// Registers the client. The client GUID is used to identify the client.
        /// If the same user is already logged in then they are permitted to log 
        /// in again on a different client.
        /// </summary>
        /// <param name="clientData">Unique ID for the installation.</param>
        private static bool registerClient(StackHashClientData clientData)
        {
            return registerClient(clientData, true, true);
        }

        /// <summary>
        /// Finds the most recent access date for the user.
        /// A user may be logged on to several machines. We don't want to bump the user
        /// if he is busy accessing the StackHash service on any of those machines.
        /// </summary>
        /// <returns>Most recent access date for this user.</returns>
        private static DateTime findMostRecentAccessDateForUser(String userName)
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                DateTime mostRecentAccessDate = new DateTime(0);

                foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                {
                    if (String.Compare(obj.Value.ClientData.ClientName, userName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (obj.Value.LastAccessTime > mostRecentAccessDate)
                            mostRecentAccessDate = obj.Value.LastAccessTime;
                    }
                }

                return mostRecentAccessDate;
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }


        /// <summary>
        /// Determines if the specified client is running on the same machine as the service.
        /// To do this the client must examine the ServiceInstanceData.xml file in c:\programdata\stackhash and 
        /// return the ServiceGuid returned therein.
        /// If it matches what the service sees then it must be running on the same machine.
        /// </summary>
        /// <param name="serviceGuid">Service guid as seen by the client. Can be null.</param>
        /// <returns>true - local, false - remote.</returns>
        private static bool isLocalClient(String serviceGuid)
        {
            String realServiceGuid = StaticObjects.TheStaticObjects.TheController.ServiceGuid;

            if (String.IsNullOrEmpty(serviceGuid))
                return false;
            else if (String.Compare(serviceGuid, realServiceGuid, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            else
                return false;
        }

        private static void checkAdminCallPermitted(StackHashClientData clientData)
        {
            if (clientData == null)
                throw new ArgumentNullException("clientData");
            if (!isLocalClient(clientData))
                throw new StackHashException("Access denied", StackHashServiceErrorCode.AccessDenied);
        }

        /// <summary>
        /// Find the user that least recently accessed the StackHash service.
        /// </summary>
        /// <returns>User that least recently accessed the StackHash service.</returns>
        private static String findUserWithOldestAccessDate()
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                DateTime oldestClientAccessDate = DateTime.Now;
                String userName = null;

                foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                {
                    // Cannot bump the local client.                   
                    if (isLocalClient(obj.Value.ClientData.ServiceGuid))
                        continue;

                    DateTime mostRecentUserAccessDate = findMostRecentAccessDateForUser(obj.Value.ClientData.ClientName);

                    if (mostRecentUserAccessDate < oldestClientAccessDate)
                    {
                        oldestClientAccessDate = mostRecentUserAccessDate;
                        userName = obj.Value.ClientData.ClientName;
                    }
                }

                return userName;
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }


        /// <summary>
        /// Bumps all instances of the specified user.
        /// </summary>
        /// <param name="userName">User to bump.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private static void bumpUser(String userName)
        {
            List<Guid> clientsToBump = new List<Guid>();

            Monitor.Enter(s_AdminCallbacks);

            try
            {
                foreach (KeyValuePair<Guid, RegisteredClient> obj in s_AdminCallbacks)
                {
                    if (String.Compare(obj.Value.ClientData.ClientName, userName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        clientsToBump.Add(obj.Value.ClientData.ApplicationGuid);
                    }
                }

                foreach (Guid guid in clientsToBump)
                {
                    IAdminNotificationEvents callback = s_AdminCallbacks[guid].ClientCallback;

                    StackHashAdminReport report = new StackHashAdminReport();
                    report.ResultData = StackHashAsyncOperationResult.Failed;
                    report.ServiceGuid = StaticObjects.TheStaticObjects.TheController.ServiceGuid;
                    report.ServiceHost = Environment.MachineName;
                    report.ClientData = s_AdminCallbacks[guid].ClientData;
                    report.LastException = "Client has been bumped because an admin user requires access or the client hasn't accessed the StackHash service for some time.";
                    report.ServiceErrorCode = StackHashServiceErrorCode.ClientBumped;

                    try
                    {
                        if (callback != null)
                            callback.AdminProgressEvent(report);
                    }
                    catch
                    {
                    }

                    s_AdminCallbacks.Remove(guid);
                }
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }

        /// <summary>
        /// Determine if the specified user is admin or not.
        /// i.e. running on the same machine as the service.
        /// </summary>
        /// <param name="clientData">Client identification.</param>
        private static bool isLocalClient(StackHashClientData clientData)
        {
            if (clientData == null)
                throw new ArgumentNullException("clientData");

            return isLocalClient(clientData.ServiceGuid);
        }


        /// <summary>
        /// Registers the client. The client GUID is used to identify the client.
        /// If the same user is already logged in then they are permitted to log 
        /// in again on a different client.
        /// </summary>
        /// <param name="clientData">Unique ID for the installation.</param>
        /// <param name="throwOnError">True - throw an exception on error, false - just return false.</param>
        /// <param name="updateAccessTime">True - update the last access time if, false - don't.</param>
        private static bool registerClient(StackHashClientData clientData, bool throwOnError, bool updateAccessTime)
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                // Get the callback to the client that will be used to notify it of admin reports.
                IAdminNotificationEvents callback =
                    OperationContext.Current.GetCallbackChannel<IAdminNotificationEvents>();

                // Get the number of licensed clients.
                // Always allow at least 1.
                // Always allow a client run on the local machine.
                StackHashLicenseData licenseData = StaticObjects.TheStaticObjects.TheController.LicenseData;
                int maxUsers = 1;
                if ((licenseData != null) && (licenseData.MaxSeats != 0))
                    maxUsers = licenseData.MaxSeats;

                if (!s_AdminCallbacks.ContainsKey(clientData.ApplicationGuid))
                {
                    // Licensing is based on the number of concurrent users.
                    // Get the current number of users - not including diagnostic tool users.
                    // Note that the same user may be logged on to several clients.
                    int currentUsers = registeredUserCount();

                    if (!isUserRegistered(clientData.ClientName) &&
                        !clientData.ClientName.Contains("_StackHashDBConfig"))
                    {
                        if (currentUsers >= maxUsers)
                        {
                            // Decide who is to be bumped.
                            // A user will be bumped if he hasn't accessed the StackHash service for a while.
                            // Also, a user will be bumped to allow the local admin to log in.
                            String candidateUser = findUserWithOldestAccessDate();
                            DateTime candidateLastAccessDate = findMostRecentAccessDateForUser(candidateUser);

                            TimeSpan durationSinceLastAccess = DateTime.Now - candidateLastAccessDate;
                            if (isLocalClient(clientData) ||
                                (!String.IsNullOrEmpty(candidateUser) &&
                                (durationSinceLastAccess.TotalSeconds >= StaticObjects.TheStaticObjects.TheController.ClientTimeoutInSeconds)))
                            {
                                bumpUser(candidateUser);
                            }
                            else
                            {
                                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Failed to register client " + clientData.ClientName);

                                if (!throwOnError)
                                    return false;
                                else
                                    throw new StackHashException("Client limit reached", StackHashServiceErrorCode.LicenseClientLimitExceeded);
                            }
                        }
                    }

                    s_AdminCallbacks.Add(clientData.ApplicationGuid, new RegisteredClient(clientData, callback, DateTime.Now, DateTime.Now));
                }
                else
                {
                    // Replace the existing entry in case the callback has timed out.
                    DateTime firstRegistered = s_AdminCallbacks[clientData.ApplicationGuid].FirstRegisteredTime;
                    DateTime lastAccessTime = s_AdminCallbacks[clientData.ApplicationGuid].LastAccessTime;

                    if (updateAccessTime)
                        lastAccessTime = DateTime.Now;

                    s_AdminCallbacks[clientData.ApplicationGuid] = new RegisteredClient(clientData, callback, lastAccessTime, firstRegistered);
                }

                return true;
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }


        /// <summary>
        /// Check that the client is registered.
        /// If so, record the last access time.
        /// </summary>
        /// <param name="clientData">Id data for the client.</param>
        private static void checkClientRegistered(StackHashClientData clientData)
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                if (!s_AdminCallbacks.ContainsKey(clientData.ApplicationGuid))
                {
                    throw new StackHashException("Client not registered", StackHashServiceErrorCode.ClientNotRegistered);
                }
                else
                {
                    s_AdminCallbacks[clientData.ApplicationGuid].LastAccessTime = DateTime.Now;
                }
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }

        
        /// <summary>
        /// Unregister the specified client.
        /// </summary>
        /// <param name="clientData">ID of the client to unregister.</param>
        /// <returns>True if found, False if not found.</returns>
        private static bool unregisterClient(StackHashClientData clientData)
        {
            Monitor.Enter(s_AdminCallbacks);

            try
            {
                if (clientData.ClientName == "UnregisterAllClients")
                {
                    s_AdminCallbacks.Clear();
                    return true; // Don't send an admin report.
                }
                else if (s_AdminCallbacks.ContainsKey(clientData.ApplicationGuid))
                {
                    s_AdminCallbacks.Remove(clientData.ApplicationGuid);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                Monitor.Exit(s_AdminCallbacks);
            }
        }

        #endregion Registration
        
        #region IAdminContract Members


        /// <summary>
        /// Gets the settings associated with a particular context (profile).
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        public RestartResponse Restart(RestartRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
    
            registerClient(requestData.ClientData);

            RestartResponse resp = new RestartResponse();

            StaticObjects.TheStaticObjects.Restart();

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }
        
        /// <summary>
        /// Gets the settings associated with a particular context (profile).
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        public GetStackHashPropertiesResponse GetStackHashSettings(GetStackHashPropertiesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetStackHashPropertiesResponse resp = new GetStackHashPropertiesResponse();

            resp.Settings = StaticObjects.TheStaticObjects.TheController.StackHashSettings;
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets the status of the service.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        public GetStackHashServiceStatusResponse GetServiceStatus(GetStackHashServiceStatusRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetStackHashServiceStatusResponse resp = new GetStackHashServiceStatusResponse();

            resp.Status = StaticObjects.TheStaticObjects.TheController.Status;
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Tests the specified context's database connection.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        public TestDatabaseConnectionResponse TestDatabaseConnection(TestDatabaseConnectionRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            TestDatabaseConnectionResponse resp = new TestDatabaseConnectionResponse();

            ErrorIndexConnectionTestResults results = null;

            // There are 2 types of connection test. One applies before a profile has been created and the 
            // second is when the profile has been created.
            if (requestData.ContextId == -1)
            {
                results = StaticObjects.TheStaticObjects.TheController.TestDatabaseConnection(
                    requestData.SqlSettings, requestData.TestDatabaseExistence, requestData.CabFolder);
            }
            else
            {
                results = StaticObjects.TheStaticObjects.TheController.TestDatabaseConnection(requestData.ContextId);
            }

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            resp.TestResult = results.Result;
            resp.LastException = results.LastException.BuildDescription();
            resp.IsCabFolderAccessible = results.IsCabFolderAccessible;
            resp.CabFolderAccessLastException = results.CabFolderAccessLastException.BuildDescription();
            return resp;
        }


        
        /// <summary>
        /// Runs the WinQual logon task to check the username and password.
        /// This is an Async call. Listen for an Admin call to get the response.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        public RunWinQualLogOnResponse RunWinQualLogOn(RunWinQualLogOnRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            RunWinQualLogOnResponse resp = new RunWinQualLogOnResponse();

            StaticObjects.TheStaticObjects.TheController.RunWinQualLogOn(requestData.ClientData, requestData.ContextId, requestData.UserName, requestData.Password);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }
        
        /// <summary>
        /// Deletes the index associated with the specified context.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public DeleteIndexResponse DeleteIndex(DeleteIndexRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            DeleteIndexResponse resp = new DeleteIndexResponse();

            StaticObjects.TheStaticObjects.TheController.DeleteIndex(requestData.ContextId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Move the index associated with the specified context.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public MoveIndexResponse MoveIndex(MoveIndexRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            MoveIndexResponse resp = new MoveIndexResponse();

            StaticObjects.TheStaticObjects.TheController.MoveIndex(requestData.ContextId,
                requestData.ClientData, requestData.NewErrorIndexPath, requestData.NewErrorIndexName, requestData.NewSqlSettings);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Copy the index associated with the specified context to another location.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public CopyIndexResponse CopyIndex(CopyIndexRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            CopyIndexResponse resp = new CopyIndexResponse();

            StaticObjects.TheStaticObjects.TheController.CopyIndex(requestData.ContextId,
                requestData.ClientData, requestData.DestinationErrorIndexSettings, requestData.SqlSettings, requestData.SwitchIndexWhenCopyComplete, requestData.DeleteSourceIndexWhenCopyComplete);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }


        
        /// <summary>
        /// Sets the service settings. This contains details such as the event index to use
        /// and WinQual login details.
        /// Not all settings can be set - some are read only.
        /// </summary>
        /// <param name="requestData">The new state of the settings</param>
        /// <returns>State of the settings following this call.</returns>
        public SetStackHashPropertiesResponse SetStackHashSettings(SetStackHashPropertiesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            SetStackHashPropertiesResponse resp = new SetStackHashPropertiesResponse();


            if (requestData.Settings == null)
                throw new ArgumentException("requestData settings cannot be null", "requestData");
            if (requestData.Settings.ContextCollection == null)
                throw new ArgumentException("requestData collection settings cannot be null", "requestData");
            if (requestData.Settings.ContextCollection.Count != 1)
                throw new ArgumentException("requestData collection must contain one context setting only", "requestData");

            StaticObjects.TheStaticObjects.TheController.ChangeContextSettings(requestData.Settings.ContextCollection[0]);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            // Get a fresh copy following the set.
            resp.Settings = StaticObjects.TheStaticObjects.TheController.StackHashSettings;

            return resp;
        }

        /// <summary>
        /// Creates a new stack hash context. Allocates an ID and creates some default values.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>New ID and default settings.</returns>
        public CreateNewStackHashContextResponse CreateNewStackHashContext(CreateNewStackHashContextRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            CreateNewStackHashContextResponse resp = new CreateNewStackHashContextResponse();

            resp.Settings = StaticObjects.TheStaticObjects.TheController.CreateNewContext(requestData.IndexType);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Removes the specified context.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public RemoveStackHashContextResponse RemoveStackHashContext(RemoveStackHashContextRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            RemoveStackHashContextResponse resp = new RemoveStackHashContextResponse();

            StaticObjects.TheStaticObjects.TheController.RemoveContextSettings(requestData.ContextId, requestData.ResetNextContextIdIfAppropriate);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Activate the specified context.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public ActivateStackHashContextResponse ActivateStackHashContext(ActivateStackHashContextRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            ActivateStackHashContextResponse resp = new ActivateStackHashContextResponse();

            StaticObjects.TheStaticObjects.TheController.ActivateContextSettings(requestData.ClientData, requestData.ContextId);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        
        /// <summary>
        /// Deactivate the specified context.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public DeactivateStackHashContextResponse DeactivateStackHashContext(DeactivateStackHashContextRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            DeactivateStackHashContextResponse resp = new DeactivateStackHashContextResponse();

            StaticObjects.TheStaticObjects.TheController.DeactivateContextSettings(requestData.ClientData, requestData.ContextId);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public SetProxyResponse SetProxy(SetProxyRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            SetProxyResponse resp = new SetProxyResponse();

            StaticObjects.TheStaticObjects.TheController.SetProxySettings(requestData.ProxySettings);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Sets the email settings for admin reporting.
        /// Emails are triggered on the events in the email settings.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public SetEmailSettingsResponse SetEmailSettings(SetEmailSettingsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            SetEmailSettingsResponse resp = new SetEmailSettingsResponse();

            StaticObjects.TheStaticObjects.TheController.SetEmailSettings(requestData.ContextId, requestData.EmailSettings);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Enable trace logging at the service.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public EnableLoggingResponse EnableLogging(EnableLoggingRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            EnableLoggingResponse resp = new EnableLoggingResponse();

            StaticObjects.TheStaticObjects.TheController.EnableLogging();
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Disable trace logging at the service.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public DisableLoggingResponse DisableLogging(DisableLoggingRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            DisableLoggingResponse resp = new DisableLoggingResponse();

            StaticObjects.TheStaticObjects.TheController.DisableLogging();
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Enable reporting of stats to the StackHash web service.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public EnableReportingResponse EnableReporting(EnableReportingRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            EnableReportingResponse resp = new EnableReportingResponse();

            StaticObjects.TheStaticObjects.TheController.EnableReporting();
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Disable reporting of stats to the StackHash web service.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public DisableReportingResponse DisableReporting(DisableReportingRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            DisableReportingResponse resp = new DisableReportingResponse();

            StaticObjects.TheStaticObjects.TheController.DisableReporting();
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        
        /// <summary>
        /// Starts the synchronisation task on the service.
        /// This will use the server settings to download the CAB data to the ErrorIndex folder.
        /// Note that this is an asychronous task.
        /// </summary>
        /// <returns>Response result code.</returns>
        public StartSynchronizationResponse StartSynchronization(StartSynchronizationRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            StartSynchronizationResponse resp = new StartSynchronizationResponse();

            StaticObjects.TheStaticObjects.TheController.RunSynchronization(
                requestData.ClientData, requestData.ContextId, requestData.ForceResynchronize, requestData.JustSyncProducts,
                requestData.ProductsToSynchronize);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Aborts the synchronisation task running on the service if there is one.
        /// </summary>
        /// <returns>Response result code.</returns>
        public AbortSynchronizationResponse AbortSynchronization(AbortSynchronizationRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            AbortSynchronizationResponse resp = new AbortSynchronizationResponse();

            StaticObjects.TheStaticObjects.TheController.AbortSynchronization(requestData.ContextId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets a list of current debugger scripts by name.
        /// </summary>
        /// <param name="requestData">Request data.</param>
        /// <returns></returns>
        public GetDebuggerScriptNamesResponse GetDebuggerScriptNames(GetDebuggerScriptNamesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetDebuggerScriptNamesResponse resp = new GetDebuggerScriptNamesResponse();

            resp.ScriptFileData = StaticObjects.TheStaticObjects.TheController.ScriptNames;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Gets a list of current debugger scripts by name.
        /// </summary>
        /// <returns></returns>
        public GetDebugResultFilesResponse GetDebugResultFiles(GetDebugResultFilesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetDebugResultFilesResponse resp = new GetDebugResultFilesResponse();

            resp.ResultFiles = StaticObjects.TheStaticObjects.TheController.GetResultFiles(
                requestData.ContextId, requestData.Product, requestData.File, requestData.Event, requestData.Cab);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets the full results of a particular script run.
        /// </summary>
        /// <returns></returns>
        public GetDebugResultResponse GetDebugResult(GetDebugResultRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetDebugResultResponse resp = new GetDebugResultResponse();
            resp.Result = StaticObjects.TheStaticObjects.TheController.GetResultFileData(
                requestData.ContextId, requestData.Product, requestData.File, requestData.Event, requestData.Cab, requestData.ScriptName);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Removes the script run results.
        /// </summary>
        /// <returns>Result of operation.</returns>
        public RemoveScriptResultResponse RemoveScriptResult(RemoveScriptResultRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RemoveScriptResultResponse resp = new RemoveScriptResultResponse();
            StaticObjects.TheStaticObjects.TheController.RemoveResultFileData(
                requestData.ContextId, requestData.ClientData, requestData.Product, requestData.File, requestData.Event, requestData.Cab, requestData.ScriptName);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        
        
        /// <summary>
        /// Adds a new script. If the script exists already it will be overwritten if the 
        /// parameters indicate overwrite.
        /// </summary>
        /// <param name="requestData"></param>
        public AddDebuggerScriptResponse AddScript(AddDebuggerScriptRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            AddDebuggerScriptResponse resp = new AddDebuggerScriptResponse();
            StaticObjects.TheStaticObjects.TheController.AddScript(requestData.Script, requestData.Overwrite);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Removes the named debugger script. If it doesn't exist then this call has no
        /// effect.
        /// </summary>
        /// <param name="requestData"></param>
        public RemoveDebuggerScriptResponse RemoveScript(RemoveDebuggerScriptRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RemoveDebuggerScriptResponse resp = new RemoveDebuggerScriptResponse();
            StaticObjects.TheStaticObjects.TheController.RemoveScript(requestData.ScriptName);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Renames the specified script to a new name.
        /// </summary>
        /// <param name="requestData"></param>
        public RenameDebuggerScriptResponse RenameScript(RenameDebuggerScriptRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RenameDebuggerScriptResponse resp = new RenameDebuggerScriptResponse();
            StaticObjects.TheStaticObjects.TheController.RenameScript(requestData.OriginalScriptName, requestData.NewScriptName);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Gets the specified script settings.
        /// </summary>
        /// <param name="requestData"></param>
        public GetDebuggerScriptResponse GetScript(GetDebuggerScriptRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetDebuggerScriptResponse resp = new GetDebuggerScriptResponse();
            resp.ScriptSettings = StaticObjects.TheStaticObjects.TheController.GetScript(requestData.ScriptName);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets the specified script settings.
        /// </summary>
        /// <param name="requestData"></param>
        public RunDebuggerScriptResponse RunScript(RunDebuggerScriptRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RunDebuggerScriptResponse resp = new RunDebuggerScriptResponse();
            resp.ScriptResult = StaticObjects.TheStaticObjects.TheController.RunScript(
                requestData.ContextId, requestData.Product, requestData.File, requestData.Event,
                requestData.Cab, requestData.DumpFileName, requestData.ScriptName, requestData.ClientData);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets the specified script settings.
        /// </summary>
        /// <param name="requestData"></param>
        public RunDebuggerScriptAsyncResponse RunScriptAsync(RunDebuggerScriptAsyncRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RunDebuggerScriptAsyncResponse resp = new RunDebuggerScriptAsyncResponse();
            StaticObjects.TheStaticObjects.TheController.RunDebugScriptTask(
                requestData.ContextId, requestData.Product, requestData.File, requestData.Event,
                requestData.Cab, requestData.DumpFileName, requestData.ScriptsToRun, requestData.ClientData, false);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }
        


        
        /// <summary>
        /// Checks the version of the client against the version of the service.
        /// If not a match then an exception will be thrown.
        /// </summary>
        /// <param name="requestData">Client version info</param>
        [SuppressMessage("Microsoft.Security", "CA2122")]
        public CheckVersionResponse CheckVersion(CheckVersionRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            
            // Don't try to register the client here. Always let this call succeed.
            //registerClient(requestData.ClientData);

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int stackHashMajorVersion = fvi.FileMajorPart;
            int stackHashMinorVersion = fvi.FileMinorPart;


            // Make sure the client and server versions match.
            if ((requestData.MajorVersion != stackHashMajorVersion) ||
                (requestData.MinorVersion != stackHashMinorVersion))
            {
                String serviceVersion = String.Format(CultureInfo.InvariantCulture, "Service Version: {0}.{1}", stackHashMajorVersion, stackHashMinorVersion);
                throw new StackHashException(serviceVersion,
                    StackHashServiceErrorCode.ClientVersionMismatch);
            }

            String serviceGuid = StaticObjects.TheStaticObjects.TheController.ServiceGuid;
            
            CheckVersionResponse resp = new CheckVersionResponse();
            if (String.IsNullOrEmpty(requestData.ServiceGuid))
                resp.IsLocalClient = false;
            else if (String.Compare(requestData.ServiceGuid, serviceGuid, StringComparison.OrdinalIgnoreCase) == 0)
                resp.IsLocalClient = true;
            else
                resp.IsLocalClient = false;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Aborts tasks of the specified type.
        /// </summary>
        /// <param name="requestData">Task to be aborted</param>
        [SuppressMessage("Microsoft.Security", "CA2122")]
        public AbortTaskResponse AbortTask(AbortTaskRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            AbortTaskResponse resp = new AbortTaskResponse();
            StaticObjects.TheStaticObjects.TheController.AbortTask(requestData.ContextId, requestData.ClientData,
                requestData.TaskType);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        

        /// <summary>
        /// Register for notifications. 
        /// The client callback is added to the list of callbacks or removed
        /// depending on the state of isRegister.
        /// Upon Registration an AdminRegister event will be sent to the callback. Note that the
        /// event may arrive at the client some time after the client has returned from the call to RegisterForNotifications
        /// because the RegisterForNotifications call IsOneWay.
        /// No event is sent to upon de-registration.
        /// This function is ONE-WAY because it calls back to the client. This would cause a deadlock
        /// if not one way.
        /// </summary>
        /// <param name="requestData">Data containing the request to register.</param>
        [SuppressMessage("Microsoft.Security", "CA2122")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void RegisterForNotifications(RegisterRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            if (requestData.IsRegister)
            {
                bool success = registerClient(requestData.ClientData, false, false); // Don't throw an exception - returns false on error.
                    

                // Here identify the current user.
                //                String name = Thread.CurrentPrincipal.Identity.Name;

                // Signal registration even if already registered.
                StackHashAdminReport report = new StackHashAdminReport();
                report.ContextId = 0;
                report.ClientData = requestData.ClientData;
                report.LastException = null;
                report.Operation = StackHashAdminOperation.AdminRegister;

                if (success)
                {
                    report.ResultData = StackHashAsyncOperationResult.Success;
                    OnAdminNotification(report, false);
                }
                else
                {
                    IAdminNotificationEvents callback =
                        OperationContext.Current.GetCallbackChannel<IAdminNotificationEvents>();
                    report.ResultData = StackHashAsyncOperationResult.Failed;
                    report.ServiceGuid = StaticObjects.TheStaticObjects.TheController.ServiceGuid;
                    report.ServiceHost = Environment.MachineName;
                    report.ServiceErrorCode = StackHashServiceErrorCode.LicenseClientLimitExceeded;
                    report.LastException = "Failed to register. Maximum licensed client seats exceeded.";
                    try
                    {
                        if (callback != null)
                            callback.AdminProgressEvent(report);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                bool removed = false;
                Monitor.Enter(s_AdminCallbacks);

                try
                {
                    removed = unregisterClient(requestData.ClientData);
                }
                finally
                {
                    Monitor.Exit(s_AdminCallbacks);
                }

                if (removed)
                {
                    StackHashAdminReport report = new StackHashAdminReport();
                    report.ContextId = 0;
                    report.ClientData = requestData.ClientData;
                    report.LastException = null;
                    report.Operation = StackHashAdminOperation.AdminUnregister;
                    report.ResultData = StackHashAsyncOperationResult.Success;
                    OnAdminNotification(report, false);
                }
            }
        }





        /// <summary>
        /// Sets the product synchronization state. i.e. whether a sync with WinQual will be made.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public SetProductSynchronizationStateResponse SetProductSynchronizationState(SetProductSynchronizationStateRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            SetProductSynchronizationStateResponse resp = new SetProductSynchronizationStateResponse();
            StaticObjects.TheStaticObjects.TheController.SetProductWinQualState(requestData.ContextId, requestData.ProductId, requestData.Enabled);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Sets the product synchronization data. e.g. the MaxCabs to download.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public SetProductSynchronizationDataResponse SetProductSynchronizationData(SetProductSynchronizationDataRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            SetProductSynchronizationDataResponse resp = new SetProductSynchronizationDataResponse();
            StaticObjects.TheStaticObjects.TheController.SetProductSyncData(requestData.ContextId, requestData.SyncData);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Sets the data collection policy for a context.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public SetDataCollectionPolicyResponse SetDataCollectionPolicy(SetDataCollectionPolicyRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            SetDataCollectionPolicyResponse resp = new SetDataCollectionPolicyResponse();
            StaticObjects.TheStaticObjects.TheController.SetDataCollectionPolicy(requestData.ContextId, requestData.PolicyCollection, requestData.SetAll);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Gets the data collection policy for a context object.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public GetDataCollectionPolicyResponse GetDataCollectionPolicy(GetDataCollectionPolicyRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetDataCollectionPolicyResponse resp = new GetDataCollectionPolicyResponse();
            resp.PolicyCollection = StaticObjects.TheStaticObjects.TheController.GetDataCollectionPolicy(requestData.ContextId,
                requestData.RootObject, requestData.Id, requestData.ConditionObject, requestData.ObjectToCollect, requestData.GetAll);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Gets the active data collection policy for a context object.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public GetActiveDataCollectionPolicyResponse GetActiveDataCollectionPolicy(GetActiveDataCollectionPolicyRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetActiveDataCollectionPolicyResponse resp = new GetActiveDataCollectionPolicyResponse();
            resp.PolicyCollection = StaticObjects.TheStaticObjects.TheController.GetActiveDataCollectionPolicy(requestData.ContextId,
                requestData.ProductId, requestData.FileId, requestData.EventId, requestData.CabId, requestData.ObjectToCollect);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        
        /// <summary>
        /// Removes the data collection policy for a context object.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public RemoveDataCollectionPolicyResponse RemoveDataCollectionPolicy(RemoveDataCollectionPolicyRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RemoveDataCollectionPolicyResponse resp = new RemoveDataCollectionPolicyResponse();
            StaticObjects.TheStaticObjects.TheController.RemoveDataCollectionPolicy(requestData.ContextId, requestData.RootObject, requestData.Id, requestData.ConditionObject, requestData.ObjectToCollect);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Sets the purge options for a context.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public SetPurgeOptionsResponse SetPurgeOptions(SetPurgeOptionsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            SetPurgeOptionsResponse resp = new SetPurgeOptionsResponse();
            StaticObjects.TheStaticObjects.TheController.SetPurgeOptions(requestData.ContextId, requestData.Schedule, requestData.PurgeOptions, requestData.SetAll);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Gets the purge options for a context.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public GetPurgeOptionsResponse GetPurgeOptions(GetPurgeOptionsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetPurgeOptionsResponse resp = new GetPurgeOptionsResponse();
            resp.PurgeOptions = StaticObjects.TheStaticObjects.TheController.GetPurgeOptions(requestData.ContextId,
                requestData.PurgeObject, requestData.Id, requestData.GetAll);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Gets the active purge options for a context.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public GetActivePurgeOptionsResponse GetActivePurgeOptions(GetActivePurgeOptionsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetActivePurgeOptionsResponse resp = new GetActivePurgeOptionsResponse();
            resp.PurgeOptions = StaticObjects.TheStaticObjects.TheController.GetActivePurgeOptions(requestData.ContextId,
                requestData.ProductId, requestData.FileId, requestData.EventId, requestData.CabId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        
        /// <summary>
        /// Removes the purge options for a context.
        /// </summary>
        /// <param name="requestData">.</param>
        /// <returns>Response data.</returns>
        public RemovePurgeOptionsResponse RemovePurgeOptions(RemovePurgeOptionsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            RemovePurgeOptionsResponse resp = new RemovePurgeOptionsResponse();
            StaticObjects.TheStaticObjects.TheController.RemovePurgeOptions(requestData.ContextId, requestData.PurgeObject, requestData.Id);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }


        /// <summary>
        /// Gets the license data used in the client.
        /// </summary>
        /// <param name="requestData">Original request.</param>
        /// <returns>Response containing license data.</returns>
        public GetLicenseDataResponse GetLicenseData(GetLicenseDataRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetLicenseDataResponse resp = new GetLicenseDataResponse();
            resp.LicenseData = StaticObjects.TheStaticObjects.TheController.LicenseData;
            resp.LicenseUsage = StaticObjects.TheStaticObjects.TheController.LicenseUsage;

            // Need to update the usage data with the current connected client list.
            resp.LicenseUsage.ClientLicenseUsageCollection = getClientData();

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Sets the license.
        /// </summary>
        /// <param name="requestData">Original request.</param>
        /// <returns>Response containing license data.</returns>
        public SetLicenseResponse SetLicense(SetLicenseRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            
            // Allow from remote stackhash client.
            //checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            SetLicenseResponse resp = new SetLicenseResponse();
            StaticObjects.TheStaticObjects.TheController.SetLicense(requestData.LicenseId);

            resp.LicenseData = StaticObjects.TheStaticObjects.TheController.LicenseData;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }


        /// <summary>
        /// Uploads a product mapping file to the Winqual service.
        /// </summary>
        /// <param name="requestData">Original request.</param>
        /// <returns>Response containing license data.</returns>
        public UploadMappingFileResponse UploadMappingFile(UploadMappingFileRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            // Allow in user mode - see 1302.
            //            checkAdminCallPermitted(requestData.ClientData);
            registerClient(requestData.ClientData);

            UploadMappingFileResponse resp = new UploadMappingFileResponse();
            StaticObjects.TheStaticObjects.TheController.RunUploadFileTask(requestData.ClientData, requestData.ContextId, requestData.MappingFileData);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        
        /// <summary>
        /// Starts the bug tracker task - or pings it to start work if it is already running.
        /// This might be necessary to "restart" the task processing after it has stopped due to an exception 
        /// calling the bug tracker plugins.
        /// </summary>
        /// <param name="requestData"></param>
        public RunBugTrackerTaskResponse RunBugTracker(RunBugTrackerTaskRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            RunBugTrackerTaskResponse resp = new RunBugTrackerTaskResponse();
            StaticObjects.TheStaticObjects.TheController.RunBugTrackerTask(requestData.ContextId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Gets the BugTracker plugin details.
        /// This includes DLLs that failed to load.
        /// </summary>
        /// <param name="requestData"></param>
        public GetBugTrackerPlugInDiagnosticsResponse GetBugTrackerDiagnostics(GetBugTrackerPlugInDiagnosticsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetBugTrackerPlugInDiagnosticsResponse resp = new GetBugTrackerPlugInDiagnosticsResponse();
            resp.BugTrackerPlugInDiagnostics = StaticObjects.TheStaticObjects.TheController.GetBugTrackerPlugInDiagnostics(
                requestData.ContextId, requestData.PlugInName);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets the BugTracker plugin settings for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        public GetContextBugTrackerPlugInSettingsResponse GetContextBugTrackerPlugInSettings(GetContextBugTrackerPlugInSettingsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetContextBugTrackerPlugInSettingsResponse resp = new GetContextBugTrackerPlugInSettingsResponse();
            resp.BugTrackerPlugInSettings = StaticObjects.TheStaticObjects.TheController.GetContextBugTrackerPlugInSettings(requestData.ContextId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Sets the BugTracker plugin settings for a particular context.
        /// </summary>
        /// <param name="requestData"></param>
        public SetContextBugTrackerPlugInSettingsResponse SetContextBugTrackerPlugInSettings(SetContextBugTrackerPlugInSettingsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            SetContextBugTrackerPlugInSettingsResponse resp = new SetContextBugTrackerPlugInSettingsResponse();
            StaticObjects.TheStaticObjects.TheController.SetContextBugTrackerPlugInSettings(requestData.ContextId, requestData.BugTrackerPlugInSettings);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Sets the client timeout for bumping.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public SetClientTimeoutResponse SetClientTimeout(SetClientTimeoutRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            SetClientTimeoutResponse resp = new SetClientTimeoutResponse();

            StaticObjects.TheStaticObjects.TheController.SetClientTimeoutInSeconds(requestData.ClientTimeoutInSeconds);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Get status mapping request.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public GetStatusMappingsResponse GetStatusMappings(GetStatusMappingsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            GetStatusMappingsResponse resp = new GetStatusMappingsResponse();

            resp.StatusMappings = StaticObjects.TheStaticObjects.TheController.GetMappings(requestData.ContextId, requestData.MappingType);
            resp.MappingType = requestData.MappingType;
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Set status mapping request.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public SetStatusMappingsResponse SetStatusMappings(SetStatusMappingsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            registerClient(requestData.ClientData);

            SetStatusMappingsResponse resp = new SetStatusMappingsResponse();

            StaticObjects.TheStaticObjects.TheController.UpdateMappings(requestData.ContextId, requestData.MappingType, requestData.StatusMappings);
            resp.MappingType = requestData.MappingType;
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        
        #endregion

        #region ProjectsContract

        /// <summary>
        /// Gets the product data associated with a particular context.
        /// </summary>
        /// <returns>Response result code.</returns>
        public GetProductsResponse GetProducts(GetProductsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetProductsResponse resp = new GetProductsResponse();
            StackHashProductInfoCollection products =
                StaticObjects.TheStaticObjects.TheController.GetProducts(requestData.ContextId);

            resp.Products = products;
            resp.LastSiteUpdateTime =
                StaticObjects.TheStaticObjects.TheController.GetLastSiteUpdateTime(requestData.ContextId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Gets the File data associated with a particular Product.
        /// </summary>
        /// <returns>Response result code.</returns>
        public GetFilesResponse GetFiles(GetFilesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetFilesResponse resp = new GetFilesResponse();
            StackHashFileCollection files =
                StaticObjects.TheStaticObjects.TheController.GetFiles(
                    requestData.ContextId, requestData.Product);

            resp.Files = files;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets the File event data associated with a particular product file.
        /// </summary>
        /// <returns>Response result code.</returns>
        public GetEventsResponse GetEvents(GetEventsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetEventsResponse resp = new GetEventsResponse();
            StackHashEventCollection events =
                StaticObjects.TheStaticObjects.TheController.GetEvents(
                    requestData.ContextId, requestData.Product, requestData.File);

            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Events = events;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets all the event data stored about a particular event.
        /// </summary>
        /// <param name="requestData">Request data.</param>
        /// <returns>Event package included per instance and cab data.</returns>
        public GetEventPackageResponse GetEventPackage(GetEventPackageRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetEventPackageResponse resp = new GetEventPackageResponse();
            StackHashEventPackage eventPackage =
                StaticObjects.TheStaticObjects.TheController.GetEventPackage(
                    requestData.ContextId, requestData.Product, requestData.File, requestData.Event);

            resp.EventPackage = eventPackage;
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets all the event data stored about a particular event.
        /// </summary>
        /// <param name="requestData">Request data.</param>
        /// <returns>Event package included per instance and cab data.</returns>
        public GetProductEventPackageResponse GetProductEventPackages(GetProductEventPackageRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetProductEventPackageResponse resp = new GetProductEventPackageResponse();
            StackHashEventPackageCollection eventPackages =
                StaticObjects.TheStaticObjects.TheController.GetProductEvents(requestData.ContextId, requestData.Product);

            resp.EventPackages = eventPackages;
            resp.Product = requestData.Product;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets all the event data matching the specified search criteria.
        /// </summary>
        /// <param name="requestData">Request data.</param>
        /// <returns>Event package included per instance and cab data.</returns>
        public GetAllEventPackageResponse GetEventPackages(GetAllEventPackageRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetAllEventPackageResponse resp = new GetAllEventPackageResponse();
            StackHashEventPackageCollection eventPackages =
                StaticObjects.TheStaticObjects.TheController.GetEvents(requestData.ContextId, requestData.SearchCriteriaCollection);

            resp.EventPackages = eventPackages;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Gets all the event data matching the specified search criteria within the specified row range
        /// when ordered as specified.
        /// </summary>
        /// <param name="requestData">Request data.</param>
        /// <returns>Event package included per instance and cab data.</returns>
        public GetWindowedEventPackageResponse GetWindowedEventPackages(GetWindowedEventPackageRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetWindowedEventPackageResponse resp = new GetWindowedEventPackageResponse();
            StackHashEventPackageCollection eventPackages =
                StaticObjects.TheStaticObjects.TheController.GetEvents(requestData.ContextId, requestData.SearchCriteriaCollection,
                    requestData.StartRow, requestData.NumberOfRows, requestData.SortOrder, requestData.Direction, requestData.CountAllMatches);

            resp.EventPackages = eventPackages;

            // For some reason this isn't visible in a collection so copy out into the message.
            resp.MaximumRowNumber = eventPackages.MaximumRowNumber;
            resp.MinimumRowNumber = eventPackages.MinimumRowNumber;
            resp.TotalRows = eventPackages.TotalRows;

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Gets product summary rollup data.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public GetProductRollupResponse GetProductSummary(GetProductRollupRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetProductRollupResponse resp = new GetProductRollupResponse();
            resp.RollupData =
                StaticObjects.TheStaticObjects.TheController.GetProductSummary(requestData.ContextId, requestData.ProductId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Retrieves the notes recorded against the specified cab file.
        /// </summary>
        /// <param name="requestData">Identifies the CAB file whose notes are required.</param>
        /// <returns>Response data.</returns>
        public GetCabNotesResponse GetCabNotes(GetCabNotesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetCabNotesResponse resp = new GetCabNotesResponse();
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;
            resp.Cab = requestData.Cab;
            resp.Notes = StaticObjects.TheStaticObjects.TheController.GetCabNotes(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Retrieves the extended data recorded against the specified cab file.
        /// </summary>
        /// <param name="requestData">Identifies the CAB file whose data is required.</param>
        /// <returns>Response data.</returns>
        public GetCabPackageResponse GetCabPackage(GetCabPackageRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            GetCabPackageResponse resp = new GetCabPackageResponse();
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;
            resp.CabPackage = StaticObjects.TheStaticObjects.TheController.GetCabPackage(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Adds a notes against the specified cab file.
        /// </summary>
        /// <param name="requestData">Identifies the CAB file to add the note to.</param>
        /// <returns>Response data.</returns>
        public AddCabNoteResponse AddCabNote(AddCabNoteRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            AddCabNoteResponse resp = new AddCabNoteResponse();
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;
            resp.Cab = requestData.Cab;

            StaticObjects.TheStaticObjects.TheController.AddCabNote(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab, requestData.NoteEntry);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Delete a note against the specified cab file.
        /// </summary>
        /// <param name="requestData">Identifies the CAB file to delete the note from.</param>
        /// <returns>Response data.</returns>
        public DeleteCabNoteResponse DeleteCabNote(DeleteCabNoteRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            DeleteCabNoteResponse resp = new DeleteCabNoteResponse();

            StaticObjects.TheStaticObjects.TheController.DeleteCabNote(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab, requestData.NoteId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Downloads the specified cab file.
        /// </summary>
        /// <param name="requestData">Identifies the CAB file to download.</param>
        /// <returns>Response data.</returns>
        public DownloadCabResponse DownloadCab(DownloadCabRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkClientRegistered(requestData.ClientData);

            DownloadCabResponse resp = new DownloadCabResponse();

            StaticObjects.TheStaticObjects.TheController.RunDownloadCabTask(requestData.ClientData, requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab, false);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        /// <summary>
        /// Retrieves the notes recorded against the specified event.
        /// </summary>
        /// <param name="requestData">Identifies the event whose notes are required.</param>
        /// <returns>Response data.</returns>
        public GetEventNotesResponse GetEventNotes(GetEventNotesRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkClientRegistered(requestData.ClientData);

            GetEventNotesResponse resp = new GetEventNotesResponse();
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;
            resp.Notes = StaticObjects.TheStaticObjects.TheController.GetEventNotes(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Adds a note against the specified event.
        /// </summary>
        /// <param name="requestData">Identifies the event file to add the note to.</param>
        /// <returns>Response data.</returns>
        public AddEventNoteResponse AddEventNote(AddEventNoteRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkClientRegistered(requestData.ClientData);

            AddEventNoteResponse resp = new AddEventNoteResponse();
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;

            StaticObjects.TheStaticObjects.TheController.AddEventNote(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.NoteEntry);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Deletes a note from the specified event.
        /// </summary>
        /// <param name="requestData">Identifies the event to delete the note from.</param>
        /// <returns>Response data.</returns>
        public DeleteEventNoteResponse DeleteEventNote(DeleteEventNoteRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkClientRegistered(requestData.ClientData);

            DeleteEventNoteResponse resp = new DeleteEventNoteResponse();

            StaticObjects.TheStaticObjects.TheController.DeleteEventNote(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.NoteId);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        
        /// <summary>
        /// Sets the BugID for a particular event.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public SetEventBugIdResponse SetEventBugId(SetEventBugIdRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkClientRegistered(requestData.ClientData);

            SetEventBugIdResponse resp = new SetEventBugIdResponse();

            StaticObjects.TheStaticObjects.TheController.SetEventBugId(requestData.ContextId, requestData.Product, requestData.File, requestData.Event, requestData.BugId);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        /// <summary>
        /// Sets the work flow status for a particular event.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>Result of the request.</returns>
        public SetEventWorkFlowStatusResponse SetWorkFlowStatus(SetEventWorkFlowStatusRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            SetEventWorkFlowStatusResponse resp = new SetEventWorkFlowStatusResponse();

            StaticObjects.TheStaticObjects.TheController.SetWorkFlowStatus(requestData.ContextId, requestData.Product, requestData.File, requestData.Event, requestData.WorkFlowStatus);
            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }

        
        /// <summary>
        /// Run bug report task.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public RunBugReportTaskResponse RunBugReportTask(RunBugReportTaskRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");
            checkClientRegistered(requestData.ClientData);

            RunBugReportTaskResponse resp = new RunBugReportTaskResponse();
            StaticObjects.TheStaticObjects.TheController.RunBugReportTask(requestData.ContextId, requestData.ClientData,
                requestData.BugReportDataCollection, requestData.PlugIns);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);

            return resp;
        }


        #endregion ProjectsContract

        #region ICabContract Members

        /// <summary>
        /// Retrieves the specified CAB file. The return value is an open stream 
        /// that can be read from in chunks for get the file.
        /// </summary>
        /// <param name="requestData">Identifies the CAB file to retrieve.</param>
        /// <returns>Stream that can be read from.</returns>
        public Stream GetCabFile(GetCabFileRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            Stream cabStream = StaticObjects.TheStaticObjects.TheController.GetCabFile(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab, requestData.FileName);

            return cabStream;
        }

        #endregion

        #region IServiceBehavior Members

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
                return;

            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                dispatcher.ErrorHandlers.Add(new ServiceErrorHandler());
            }

        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }


        #endregion

        #region ITestContract Members

        public CreateTestIndexResponse CreateTestIndex(CreateTestIndexRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            CreateTestIndexResponse resp = new CreateTestIndexResponse();

            StaticObjects.TheStaticObjects.TheController.CreateTestIndex(requestData.ContextId, requestData.TestIndexData);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        public SetTestDataResponse SetTestData(SetTestDataRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            checkClientRegistered(requestData.ClientData);

            SetTestDataResponse resp = new SetTestDataResponse();

            StaticObjects.TheStaticObjects.TheController.SetTestData(requestData.TestData);

            resp.ResultData = new StackHashServiceResultData(
                StackHashServiceResult.Success, s_OperationSuccessful, null);
            return resp;
        }

        /// <summary>
        /// Gets the specified attribute from the testmode.xml file.
        /// </summary>
        /// <param name="requestData">Request parameters</param>
        /// <returns>Response data</returns>
        public GetTestModeSettingResponse GetTestModeSetting(GetTestModeSettingRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            GetTestModeSettingResponse resp = new GetTestModeSettingResponse();
            resp.ResultData = new StackHashServiceResultData(StackHashServiceResult.Success, s_OperationSuccessful, null);

            resp.AttributeName = requestData.AttributeName;
            resp.AttributeValue = StackHashUtilities.TestSettings.GetAttribute(requestData.AttributeName);

            return resp;
        }


        #endregion
    }
}
