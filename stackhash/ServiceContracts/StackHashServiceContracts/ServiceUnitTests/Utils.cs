using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServiceUnitTests.StackHashServices;
using StackHashUtilities;

namespace ServiceUnitTests
{
    public class Utils : IDisposable
    {
        private Collection<AdminReportEventArgs> m_AllReports;
        private Collection<AdminReportEventArgs> m_AllActivationReports;
        private AutoResetEvent m_AdminReportEvent;
        private ServiceProxyControl m_ServiceProxyControl;
        private AutoResetEvent m_SyncCompleteEvent;
        private AutoResetEvent m_SyncStartEvent;
        private AutoResetEvent m_SyncProgressEvent;
        private AutoResetEvent m_BugReportProgressEvent;
        private AutoResetEvent m_LogOnCompleteEvent;
        private AutoResetEvent m_AnalyzeCompleteEvent;
        private AutoResetEvent m_MoveCompleteEvent;
        private AutoResetEvent m_CopyCompleteEvent;
        private AutoResetEvent m_RunScriptCompleteEvent;
        private AutoResetEvent m_DownloadCabCompleteEvent;
        private AutoResetEvent m_UploadMappingFileCompleteEvent;
        private AutoResetEvent m_RunBugReportCompleteEvent;
        private StackHashAdminReport m_RunScriptStartedAdminReport;
        private StackHashAdminReport m_WinQualSyncAdminReport;
        private StackHashAdminReport m_MoveCompleteAdminReport;
        private StackHashAdminReport m_CopyCompleteAdminReport;
        private StackHashAdminReport m_AnalyzeCompleteAdminReport;
        private StackHashAdminReport m_RunScriptAdminReport;
        private StackHashAdminReport m_WinQualLogOnAdminReport;
        private StackHashAdminReport m_DownloadCabAdminReport;
        private List<StackHashAdminReport> m_BugReportProgressReports;
        private StackHashAdminReport m_UploadMappingFileAdminReport;

        private int m_RequestId;
        private Guid m_ApplicationGuid;
        private StackHashClientData m_LastClientData;

        public Collection<AdminReportEventArgs> AllReports
        {
            get
            {
                return m_AllReports;
            }
        }

        public Collection<AdminReportEventArgs> AllActivationReports
        {
            get
            {
                return m_AllActivationReports;
            }
        }

        public static String TestAdminServiceGuid = "754D76A1-F4EE-4030-BB29-A858F52D5D13";
        public static String TestNonAdminServiceGuid = "754D76A2-F4EE-4030-BB29-A858F52D5D13";

        private void initialise(long cabContractMaxReceivedMessageSize)
        {
            m_SyncCompleteEvent = new AutoResetEvent(false);
            m_SyncStartEvent = new AutoResetEvent(false);
            m_SyncProgressEvent = new AutoResetEvent(false);
            m_BugReportProgressEvent = new AutoResetEvent(false);
            m_MoveCompleteEvent = new AutoResetEvent(false);
            m_CopyCompleteEvent = new AutoResetEvent(false);
            m_AnalyzeCompleteEvent = new AutoResetEvent(false);
            m_RunScriptCompleteEvent = new AutoResetEvent(false);
            m_LogOnCompleteEvent = new AutoResetEvent(false);
            m_DownloadCabCompleteEvent = new AutoResetEvent(false);
            m_UploadMappingFileCompleteEvent = new AutoResetEvent(false);
            m_RunBugReportCompleteEvent = new AutoResetEvent(false);
            m_ApplicationGuid = Guid.NewGuid();

            // These objects will be used to call the service.
            m_ServiceProxyControl = new ServiceProxyControl(@"net.tcp://localhost:9000/StackHash/", @"host/localhost", cabContractMaxReceivedMessageSize);
            m_ServiceProxyControl.AdminReport += new EventHandler<AdminReportEventArgs>(this.AdminReportCallback);

            m_AllReports = new Collection<AdminReportEventArgs>();
            m_AllActivationReports = new Collection<AdminReportEventArgs>();
            m_AdminReportEvent = new AutoResetEvent(false);
            m_BugReportProgressReports = new List<StackHashAdminReport>();

        }

        public Utils()
        {
            initialise(64 * 1024);
        }

        public Utils(long cabContractMaxReceivedMessageSize)
        {
            initialise(cabContractMaxReceivedMessageSize);
        }

        public AutoResetEvent AdminReport
        {
            get
            {
                return m_AdminReportEvent;
            }
        }

        public StackHashAdminReport WinQualSyncAdminReport
        {
            get
            {
                return m_WinQualSyncAdminReport;
            }
        }

        public StackHashAdminReport RunScriptAdminReport
        {
            get
            {
                return m_RunScriptAdminReport;
            }
        }

        public StackHashAdminReport DownloadCabAdminReport
        {
            get
            {
                return m_DownloadCabAdminReport;
            }
        }

        public List<StackHashAdminReport> BugReportProgressReports
        {
            get
            {
                return m_BugReportProgressReports;
            }
        }

        
        public StackHashAdminReport WinQualLogOnAdminReport
        {
            get
            {
                return m_WinQualLogOnAdminReport;
            }
        }

        
        public StackHashAdminReport MoveCompleteAdminReport
        {
            get
            {
                return m_MoveCompleteAdminReport;
            }
        }

        public StackHashAdminReport CopyCompleteAdminReport
        {
            get
            {
                return m_CopyCompleteAdminReport;
            }
        }

        public StackHashAdminReport UploadMappingFileAdminReport
        {
            get
            {
                return m_UploadMappingFileAdminReport;
            }
        }

        
        public StackHashClientData LastClientData
        {
            get
            {
                return m_LastClientData;
            }
        }

        public Guid ApplicationGuid
        {
            get
            {
                return m_ApplicationGuid;
            }
        }



        /// <summary>
        /// Invoked when an admin report arrives. 
        /// </summary>
        /// <param name="sender">Object invoking this delegate.</param>
        /// <param name="e">Callback events.</param>
        public  void AdminReportCallback(object sender, AdminReportEventArgs e)
        {
            // Dump the report to the windows.
            String outString = string.Format("{0} {1}",
                e.Report.Operation.ToString(), e.Report.ResultData.ToString());

            if (e.Report.LastException != null)
            {
                outString += e.Report.LastException.ToString();
            }

            Console.WriteLine(outString);

            if (e.Report.Operation == StackHashAdminOperation.WinQualSyncCompleted)
            {
                StackHashWinQualSyncCompleteAdminReport report = e.Report as StackHashWinQualSyncCompleteAdminReport;
                if (report.ErrorIndexStatistics == null)
                    report.ErrorIndexStatistics = new StackHashSynchronizeStatistics();

                // Dump the stats.
                outString = string.Format("{0}:{1}:{2}:{3}:{4}", report.ErrorIndexStatistics.Products,
                    report.ErrorIndexStatistics.Files, report.ErrorIndexStatistics.Events, report.ErrorIndexStatistics.EventInfos,
                    report.ErrorIndexStatistics.Cabs);
                Console.WriteLine(outString);

                m_WinQualSyncAdminReport = e.Report;

                m_SyncCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.WinQualSyncStarted)
            {
                m_SyncStartEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.WinQualSyncProgressDownloadingProductList)
            {
                m_SyncProgressEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.BugReportProgress)
            {
                m_BugReportProgressEvent.Set();
                m_BugReportProgressReports.Add(e.Report);
            }
            else if (e.Report.Operation == StackHashAdminOperation.ErrorIndexMoveCompleted)
            {
                m_MoveCompleteAdminReport = e.Report;

                m_MoveCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.AnalyzeCompleted)
            {
                m_AnalyzeCompleteAdminReport = e.Report;

                m_AnalyzeCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.RunScriptStarted)
            {
                m_RunScriptStartedAdminReport = e.Report;
            }
            else if (e.Report.Operation == StackHashAdminOperation.RunScriptCompleted)
            {
                m_RunScriptAdminReport = e.Report;

                m_RunScriptCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.WinQualLogOnCompleted)
            {
                m_WinQualLogOnAdminReport = e.Report;

                m_LogOnCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.DownloadCabCompleted)
            {
                m_DownloadCabAdminReport = e.Report;

                m_DownloadCabCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.UploadFileCompleted)
            {
                m_UploadMappingFileCompleteEvent.Set();
                m_UploadMappingFileAdminReport = e.Report;
            }
            else if (e.Report.Operation == StackHashAdminOperation.BugReportCompleted)
            {
                m_RunBugReportCompleteEvent.Set();
            }
            else if (e.Report.Operation == StackHashAdminOperation.ErrorIndexCopyCompleted)
            {
                m_CopyCompleteAdminReport = e.Report;

                m_CopyCompleteEvent.Set();
            }

            if (e.Report.Operation == StackHashAdminOperation.ContextStateChanged)
            {
                m_AllActivationReports.Add(e);
            }
            else
            {
                m_AllReports.Add(e);
                m_AdminReportEvent.Set();
            }

            Assert.AreNotEqual(null, e.Report.ServiceGuid);
            Assert.AreEqual(Environment.MachineName, e.Report.ServiceHost);
        }


        public StackHashTaskStatus FindTaskStatus(StackHashTaskStatusCollection collection, StackHashTaskType taskType)
        {
            foreach (StackHashTaskStatus taskStatus in collection)
            {
                if (taskStatus.TaskType == taskType)
                    return taskStatus;
            }

            return null;
        }


        public StackHashClientData DefaultClientData
        {
            get
            {
                StackHashClientData clientData = new StackHashClientData();
                clientData.ClientName = Environment.UserName;
                clientData.ClientId = 100;
                clientData.ClientRequestId = m_RequestId++;
                clientData.ApplicationGuid = m_ApplicationGuid;
                clientData.ServiceGuid = Utils.TestAdminServiceGuid;

                m_LastClientData = clientData;

                return clientData;
            }
        }

        public GetStackHashPropertiesResponse GetContextSettings()
        {
            // Make sure it has been added and is inactive.
            GetStackHashPropertiesRequest request = new GetStackHashPropertiesRequest();
            request.ClientData = DefaultClientData;

            GetStackHashPropertiesResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetStackHashSettings(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public GetDataCollectionPolicyResponse GetDataCollectionPolicy(int contextId, StackHashCollectionObject RootObject, int id, bool getAll, StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect)
        {
            GetDataCollectionPolicyRequest request = new GetDataCollectionPolicyRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.RootObject = RootObject;
            request.ConditionObject = conditionObject;
            request.ObjectToCollect = objectToCollect;
            request.Id = id;
            request.GetAll = getAll;

            GetDataCollectionPolicyResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetDataCollectionPolicy(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        
        public GetActiveDataCollectionPolicyResponse GetActiveDataCollectionPolicy(int contextId, int ProductId, int FileId, int EventId, int CabId)
        {
            GetActiveDataCollectionPolicyRequest request = new GetActiveDataCollectionPolicyRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ProductId = ProductId;
            request.FileId = FileId;
            request.EventId = EventId;
            request.CabId = CabId;
            request.ObjectToCollect = StackHashCollectionObject.Cab;

            GetActiveDataCollectionPolicyResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetActiveDataCollectionPolicy(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public GetActiveDataCollectionPolicyResponse GetActiveDataCollectionPolicy(int contextId, int ProductId, int FileId, int EventId, int CabId, StackHashCollectionObject objectToCollect)
        {
            GetActiveDataCollectionPolicyRequest request = new GetActiveDataCollectionPolicyRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ProductId = ProductId;
            request.FileId = FileId;
            request.EventId = EventId;
            request.CabId = CabId;
            request.ObjectToCollect = objectToCollect;

            GetActiveDataCollectionPolicyResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetActiveDataCollectionPolicy(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        
        public RemoveDataCollectionPolicyResponse RemoveDataCollectionPolicy(int contextId, StackHashCollectionObject RootObject, int id, StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect)
        {
            RemoveDataCollectionPolicyRequest request = new RemoveDataCollectionPolicyRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.RootObject = RootObject;
            request.ConditionObject = conditionObject;
            request.ObjectToCollect = objectToCollect;
            request.Id = id;

            RemoveDataCollectionPolicyResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RemoveDataCollectionPolicy(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public SetDataCollectionPolicyResponse SetDataCollectionPolicy(int contextId, StackHashCollectionPolicyCollection policyCollection, bool setAll)
        {
            SetDataCollectionPolicyRequest request = new SetDataCollectionPolicyRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.PolicyCollection = policyCollection;
            request.SetAll = setAll;

            SetDataCollectionPolicyResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetDataCollectionPolicy(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public SetEmailSettingsResponse SetEmailSettings(int contextId, StackHashEmailSettings emailSettings)
        {
            SetEmailSettingsRequest request = new SetEmailSettingsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.EmailSettings = emailSettings;

            SetEmailSettingsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetEmailSettings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }


        
        public GetPurgeOptionsResponse GetPurgeOptions(int contextId, StackHashPurgeObject purgeObject, int id, bool getAll)
        {
            GetPurgeOptionsRequest request = new GetPurgeOptionsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.PurgeObject = purgeObject;
            request.Id = id;
            request.GetAll = getAll;

            GetPurgeOptionsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetPurgeOptions(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public GetActivePurgeOptionsResponse GetActivePurgeOptions(int contextId, int productId, int fileId, int eventId, int cabId)
        {
            GetActivePurgeOptionsRequest request = new GetActivePurgeOptionsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ProductId = productId;
            request.FileId = fileId;
            request.EventId = eventId;
            request.CabId = cabId;

            GetActivePurgeOptionsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetActivePurgeOptions(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        
        public RemovePurgeOptionsResponse RemovePurgeOptions(int contextId, StackHashPurgeObject purgeObject, int id)
        {
            RemovePurgeOptionsRequest request = new RemovePurgeOptionsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.PurgeObject = purgeObject;
            request.Id = id;

            RemovePurgeOptionsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RemovePurgeOptions(request);


            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }
        
        public void GetCab(String destFileName, int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, String fileName)
        {
            // Make sure it has been added and is inactive.
            GetCabFileRequest request = new GetCabFileRequest();
            request.ClientData = DefaultClientData;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;
            request.FileName = fileName;
            request.ContextId = contextId;

            Stream fileStream =
                m_ServiceProxyControl.StackHashCabClient.GetCabFile(request);

            if (fileStream != null)
            {
                FileStream outStream = File.Open(destFileName, FileMode.Create, FileAccess.Write, FileShare.None);

                try
                {
                    int bufferSize = 64 * 1024;
                    byte [] buffer = new byte [bufferSize];

                    int thisSize = 0;
                    while ((thisSize = fileStream.Read(buffer, 0, bufferSize)) != 0)
                    {
                        outStream.Write(buffer, 0, thisSize);
                    }
                }
                finally
                {
                    outStream.Close();
                    fileStream.Close();
                }
            }
        }

        public StartSynchronizationResponse StartSynchronization(int contextId, int timeout)
        {
            return StartSynchronization(contextId, timeout, false);
        }
        public StartSynchronizationResponse StartSynchronization(int contextId, int timeout, bool justSyncProducts)
        {
            return StartSynchronization(contextId, timeout, justSyncProducts, false);
        }
        public StartSynchronizationResponse StartSynchronization(int contextId, int timeout, bool justSyncProducts, bool forceResync)
        {
            return StartSynchronization(contextId, timeout, justSyncProducts, forceResync, null);
        }
        public StartSynchronizationResponse StartSynchronization(int contextId, int timeout, bool justSyncProducts, bool forceResync, StackHashProductSyncDataCollection productsToSync)
        {
            m_SyncStartEvent.Reset();
            m_SyncProgressEvent.Reset();

            StartSynchronizationRequest request = new StartSynchronizationRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ForceResynchronize = forceResync;
            request.JustSyncProducts = justSyncProducts;
            request.ProductsToSynchronize = productsToSync;

            StartSynchronizationResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.StartSynchronization(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the task to complete.
            Assert.AreEqual(true, m_SyncCompleteEvent.WaitOne(timeout));

            Console.WriteLine("SyncComplete signalled");

            // Wait for the cab analyze task to complete too.
            if (m_WinQualSyncAdminReport.ResultData == StackHashAsyncOperationResult.Success)
            {
                // Analyze not started if syncing products only.
                if (!justSyncProducts)
                {
                    Assert.AreEqual(true, m_AnalyzeCompleteEvent.WaitOne(timeout));
                }
            }

            return resp;
        }

        public DownloadCabResponse DownloadCab(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int timeout)
        {
            DownloadCabRequest request = new DownloadCabRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;

            DownloadCabResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.DownloadCab(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the task to complete.
            Assert.AreEqual(true, m_DownloadCabCompleteEvent.WaitOne(timeout));

            return resp;
        }

        public UploadMappingFileResponse UploadMappingFile(int contextId, String mappingData, int timeout)
        {
            UploadMappingFileRequest request = new UploadMappingFileRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.MappingFileData = mappingData;

            UploadMappingFileResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.UploadMappingFile(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the task to complete.
            Assert.AreEqual(true, m_UploadMappingFileCompleteEvent.WaitOne(timeout));

            return resp;
        }

        public RunBugReportTaskResponse RunBugReportTask(int contextId, StackHashBugReportDataCollection reportData, int timeout, bool wait)
        {
            m_BugReportProgressEvent.Reset();
            m_BugReportProgressReports.Clear();

            RunBugReportTaskRequest request = new RunBugReportTaskRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.BugReportDataCollection = reportData;

            RunBugReportTaskResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.RunBugReportTask(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            bool result = m_RunBugReportCompleteEvent.WaitOne(timeout);

            if (wait)
            {
                // Wait for the task to complete.
                Assert.AreEqual(true, result);
            }

            return resp;
        }


        public GetStatusMappingsResponse GetStatusMappings(int contextId, StackHashMappingType mappingType)
        {
            GetStatusMappingsRequest request = new GetStatusMappingsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.MappingType = mappingType;

            GetStatusMappingsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetStatusMappings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public SetStatusMappingsResponse SetStatusMappings(int contextId, StackHashMappingType mappingType, StackHashMappingCollection mappings)
        {
            SetStatusMappingsRequest request = new SetStatusMappingsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.MappingType = mappingType;
            request.StatusMappings = mappings;

            SetStatusMappingsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetStatusMappings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        

        public bool IsTaskRunning(StackHashTaskStatusCollection tasks, StackHashTaskType taskType)
        {
            foreach (StackHashTaskStatus taskStatus in tasks)
            {
                if ((taskStatus.TaskType == taskType) &&
                    (taskStatus.TaskState == StackHashTaskState.Running))
                    return true;
            }
            return false;
        }

        public StartSynchronizationResponse StartSynchronizationAsync(int contextId)
        {
            m_SyncStartEvent.Reset();
            m_SyncProgressEvent.Reset();

            StartSynchronizationRequest request = new StartSynchronizationRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ForceResynchronize = false;

            StartSynchronizationResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.StartSynchronization(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public RunWinQualLogOnResponse RunWinQualLogOn(int contextId, String userName, String password)
        {
            RunWinQualLogOnRequest request = new RunWinQualLogOnRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.UserName = userName;
            request.Password = password;

            RunWinQualLogOnResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RunWinQualLogOn(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the response.
            WaitForLogOnCompletion(20000);

            return resp;
        }

        public SetProductSynchronizationStateResponse SetProductSynchronizationState(int contextId, int productId, bool isEnabled)
        {
            SetProductSynchronizationStateRequest request = new SetProductSynchronizationStateRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ProductId = productId;
            request.Enabled = isEnabled;

            SetProductSynchronizationStateResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetProductSynchronizationState(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public SetProductSynchronizationDataResponse SetProductSynchronizationData(int contextId, StackHashProductSyncData syncData)
        {
            SetProductSynchronizationDataRequest request = new SetProductSynchronizationDataRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.SyncData = syncData;

            SetProductSynchronizationDataResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetProductSynchronizationData(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public bool WaitForLogOnCompletion(int timeout)
        {
            return (m_LogOnCompleteEvent.WaitOne(timeout));
        }


        public bool WaitForSyncStarted(int timeout)
        {
            return (m_SyncStartEvent.WaitOne(timeout));
        }
        public bool WaitForBugReportProgress(int timeout)
        {
            return (m_BugReportProgressEvent.WaitOne(timeout));
        }
        public bool WaitForSyncProgress(int timeout)
        {
            return (m_SyncProgressEvent.WaitOne(timeout));
        }
        public bool WaitForBugReportTaskCompleted(int timeout)
        {
            return (m_RunBugReportCompleteEvent.WaitOne(timeout));
        }



        public bool WaitForSyncCompletion(int timeout)
        {
            bool result = m_SyncCompleteEvent.WaitOne(timeout);
            Console.WriteLine("SyncComplete signalled : " + result.ToString());

            return (result);
        }
        public bool WaitForAnalyzeCompletion(int timeout)
        {
            return (m_AnalyzeCompleteEvent.WaitOne(timeout));
        }

        public AbortSynchronizationResponse AbortSynchronization(int contextId, int timeout)
        {
            AbortSynchronizationRequest request = new AbortSynchronizationRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;

            Console.WriteLine("AbortSync: " + DateTime.Now.ToString());
            AbortSynchronizationResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.AbortSynchronization(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the task to complete.

            if (timeout != 0)
            {
                DateTime startTime = DateTime.Now;

                if (!m_SyncCompleteEvent.WaitOne(timeout))
                {
                    DateTime endTime = DateTime.Now;
                    Console.WriteLine("AbortSync failed took: {0}", (endTime - startTime).TotalSeconds);
                    GetStackHashServiceStatusResponse serviceStatus = GetServiceStatus();

                   
                    StackHashTaskStatus status = FindTaskStatus(serviceStatus.Status.ContextStatusCollection[0].TaskStatusCollection, StackHashTaskType.WinQualSynchronizeTask);

                    Assert.AreNotEqual(StackHashTaskState.Running, status.TaskState);

                    Assert.AreEqual(true, false);
                }

                DateTime endTime2 = DateTime.Now;

                Console.WriteLine("AbortSync took: {0}", (endTime2 - startTime).TotalSeconds);
            }
            
            return resp;
        }


        public GetStackHashServiceStatusResponse GetServiceStatus()
        {
            // Make sure it has been added and is inactive.
            GetStackHashServiceStatusRequest request = new GetStackHashServiceStatusRequest();
            request.ClientData = DefaultClientData;

            GetStackHashServiceStatusResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetServiceStatus(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }


        public void RemoveContext(int id)
        {
            RemoveStackHashContextRequest removeRequest = new RemoveStackHashContextRequest();
            removeRequest.ClientData = DefaultClientData;
            removeRequest.ContextId = id;
            removeRequest.ResetNextContextIdIfAppropriate = true;

            RemoveStackHashContextResponse removeResp =
                m_ServiceProxyControl.StackHashAdminClient.RemoveStackHashContext(removeRequest);

            Assert.AreEqual(StackHashServiceResult.Success, removeResp.ResultData.Result);
            Assert.AreEqual(null, removeResp.ResultData.LastException);
        }

        public TestDatabaseConnectionResponse TestDatabaseConnection(int contextId)
        {
            TestDatabaseConnectionRequest testConnectionRequest = new TestDatabaseConnectionRequest();
            testConnectionRequest.ClientData = DefaultClientData;
            testConnectionRequest.ContextId = contextId;

            TestDatabaseConnectionResponse testResp =
                m_ServiceProxyControl.StackHashAdminClient.TestDatabaseConnection(testConnectionRequest);

            Assert.AreEqual(StackHashServiceResult.Success, testResp.ResultData.Result);
            Assert.AreEqual(null, testResp.ResultData.LastException);

            return testResp;
        }

        public TestDatabaseConnectionResponse TestDatabaseConnection(StackHashSqlConfiguration sqlSettings, bool testDatabaseExistence)
        {
            return TestDatabaseConnection(sqlSettings, testDatabaseExistence, null);
        }

        public TestDatabaseConnectionResponse TestDatabaseConnection(StackHashSqlConfiguration sqlSettings, bool testDatabaseExistence, String cabFolder)
        {
            TestDatabaseConnectionRequest testConnectionRequest = new TestDatabaseConnectionRequest();
            testConnectionRequest.ClientData = DefaultClientData;
            testConnectionRequest.ContextId = -1;
            testConnectionRequest.SqlSettings = sqlSettings;
            testConnectionRequest.TestDatabaseExistence = testDatabaseExistence;

            testConnectionRequest.CabFolder = cabFolder;
            TestDatabaseConnectionResponse testResp =
                m_ServiceProxyControl.StackHashAdminClient.TestDatabaseConnection(testConnectionRequest);

            Assert.AreEqual(StackHashServiceResult.Success, testResp.ResultData.Result);
            Assert.AreEqual(null, testResp.ResultData.LastException);

            return testResp;
        }

        
        public void RestartService()
        {
            RestartRequest restartRequest = new RestartRequest();
            restartRequest.ClientData = DefaultClientData;

            RestartResponse restartResp =
                m_ServiceProxyControl.StackHashAdminClient.Restart(restartRequest);

            Assert.AreEqual(StackHashServiceResult.Success, restartResp.ResultData.Result);
            Assert.AreEqual(null, restartResp.ResultData.LastException);
        }

        public void RemoveAllContexts()
        {
            GetStackHashPropertiesRequest request = new GetStackHashPropertiesRequest();
            request.ClientData = DefaultClientData;

            GetStackHashPropertiesResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetStackHashSettings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);


            foreach (StackHashContextSettings settings in resp.Settings.ContextCollection)
            {
                DeactivateContext(settings.Id);
                DeleteIndex(settings.Id);
                RemoveContext(settings.Id);
            }
        }

        public CreateNewStackHashContextResponse CreateNewContext()
        {
            return CreateNewContext(ErrorIndexType.Xml);
        }

        public CreateNewStackHashContextResponse CreateNewContext(ErrorIndexType indexType)
        {
            // Add a context.
            CreateNewStackHashContextRequest addRequest = new CreateNewStackHashContextRequest();
            addRequest.ClientData = DefaultClientData;
            addRequest.IndexType = indexType;

            CreateNewStackHashContextResponse createNewResp =
                m_ServiceProxyControl.StackHashAdminClient.CreateNewStackHashContext(addRequest);

            Assert.AreEqual(StackHashServiceResult.Success, createNewResp.ResultData.Result);
            Assert.AreEqual(null, createNewResp.ResultData.LastException);
            return createNewResp;
        }

        public SetStackHashPropertiesResponse CreateAndSetNewContext()
        {
            // Add a context.
            CreateNewStackHashContextResponse addRequest = CreateNewContext(ErrorIndexType.Xml);
            
            // Now set the name to something unique.
            addRequest.Settings.ErrorIndexSettings.Name += addRequest.Settings.Id.ToString();
            SetStackHashPropertiesResponse setResp = SetContextSettings(addRequest.Settings);

            
            // Delete any index that might still exist from a previous test.
            DeleteIndex(addRequest.Settings.Id);
            
            return setResp;
        }

        public SetStackHashPropertiesResponse CreateAndSetNewContext(ErrorIndexType errorIndexType)
        {
            return CreateAndSetNewContext(errorIndexType, false);
        }

        public SetStackHashPropertiesResponse CreateAndSetNewContext(ErrorIndexType errorIndexType, bool useValidLogonCredentials)
        {
            // Add a context.
            CreateNewStackHashContextResponse addRequest = CreateNewContext(errorIndexType);

            // Now set the name to something unique.
            addRequest.Settings.ErrorIndexSettings.Name += addRequest.Settings.Id.ToString();
            addRequest.Settings.ErrorIndexSettings.Type = errorIndexType;
            addRequest.Settings.SqlSettings.InitialCatalog = addRequest.Settings.ErrorIndexSettings.Name;

            if (useValidLogonCredentials)
            {
                addRequest.Settings.WinQualSettings.UserName = ServiceTestSettings.WinQualUserName;
                addRequest.Settings.WinQualSettings.Password = ServiceTestSettings.WinQualPassword;
            }

            SetStackHashPropertiesResponse setResp = SetContextSettings(addRequest.Settings);


            // Delete any index that might still exist from a previous test.
            DeleteIndex(addRequest.Settings.Id);

            return setResp;
        }

        
        public StackHashContextSettings MakeContextSettings(int contextId)
        {
            StackHashContextSettings settings = new StackHashContextSettings();

            settings.Id = contextId; // Shouldn't be able to set this.

            settings.CabFilePurgeSchedule = new ScheduleCollection();
            settings.CabFilePurgeSchedule.Add(new Schedule());
            settings.CabFilePurgeSchedule[0].DaysOfWeek = DaysOfWeek.Saturday;
            settings.CabFilePurgeSchedule[0].Period = SchedulePeriod.Hourly;
            settings.CabFilePurgeSchedule[0].Time = new ScheduleTime();
            settings.CabFilePurgeSchedule[0].Time.Hour = 22;
            settings.CabFilePurgeSchedule[0].Time.Minute = 30;
            settings.CabFilePurgeSchedule[0].Time.Second = 15;

            settings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
            settings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
            settings.PurgeOptionsCollection[0].AgeToPurge = 20;
            settings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
            settings.PurgeOptionsCollection[0].PurgeDumpFiles = false;
            settings.PurgeOptionsCollection[0].PurgeCabFiles = true;
            settings.PurgeOptionsCollection[0].Id = 0;
            settings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
            settings.PurgeOptionsCollection[1].AgeToPurge = 20;
            settings.PurgeOptionsCollection[1].PurgeObject = StackHashPurgeObject.PurgeEvent;
            settings.PurgeOptionsCollection[1].PurgeDumpFiles = true;
            settings.PurgeOptionsCollection[1].PurgeCabFiles = false;
            settings.PurgeOptionsCollection[1].Id = 24;

            settings.DebuggerSettings = new StackHashDebuggerSettings();
            settings.DebuggerSettings.BinaryPath = new StackHashSearchPath();
            settings.DebuggerSettings.BinaryPath.Add("C:\\binaries");

            settings.DebuggerSettings.BinaryPath64Bit = new StackHashSearchPath();
            settings.DebuggerSettings.BinaryPath64Bit.Add("C:\\binaries64");

            settings.DebuggerSettings.DebuggerPathAndFileName = "C:\\debuggerpath";
            settings.DebuggerSettings.DebuggerPathAndFileName64Bit = "C:\\debuggerpath64";

            settings.DebuggerSettings.SymbolPath = new StackHashSearchPath();
            settings.DebuggerSettings.SymbolPath.Add("C:\\symbols");

            settings.DebuggerSettings.SymbolPath64Bit = new StackHashSearchPath();
            settings.DebuggerSettings.SymbolPath64Bit.Add("C:\\symbols64");

            settings.ErrorIndexSettings = new ErrorIndexSettings();
            settings.ErrorIndexSettings.Folder = "C:\\errorindexpath" + contextId.ToString();
            settings.ErrorIndexSettings.Name = "TestIndex" + contextId.ToString();
            settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;

            settings.ErrorIndexSettings.Type = ErrorIndexType.Xml;
            settings.IsActive = false;

            settings.SqlSettings = new StackHashSqlConfiguration();
            settings.SqlSettings.ConnectionString = TestSettings.DefaultConnectionString;
            settings.SqlSettings.ConnectionTimeout = 15;
            settings.SqlSettings.EventsPerBlock = 100;
            settings.SqlSettings.InitialCatalog = "StackHash";
            settings.SqlSettings.MaxPoolSize = 20;
            settings.SqlSettings.MinPoolSize = 5;

            settings.WinQualSettings = new WinQualSettings();
            settings.WinQualSettings.AgeOldToPurgeInDays = 91;
            settings.WinQualSettings.CompanyName = "CompanyName";
            settings.WinQualSettings.Password = "TestPassword";
            settings.WinQualSettings.UserName = "TestUsername";

            settings.WinQualSyncSchedule = new ScheduleCollection();
            settings.WinQualSyncSchedule.Add(new Schedule());
            settings.WinQualSyncSchedule[0].DaysOfWeek = DaysOfWeek.Sunday;
            settings.WinQualSyncSchedule[0].Period = SchedulePeriod.Weekly;
            settings.WinQualSyncSchedule[0].Time = new ScheduleTime();
            settings.WinQualSyncSchedule[0].Time.Hour = 12;
            settings.WinQualSyncSchedule[0].Time.Minute = 30;
            settings.WinQualSyncSchedule[0].Time.Second = 34;

            settings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
            settings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();

            return settings;
        }

        public void CheckContextSettings(StackHashContextSettings setSettings, StackHashContextSettings getSettings)
        {
            Assert.AreEqual(setSettings.CabFilePurgeSchedule[0].DaysOfWeek, getSettings.CabFilePurgeSchedule[0].DaysOfWeek);
            Assert.AreEqual(setSettings.CabFilePurgeSchedule[0].Period, getSettings.CabFilePurgeSchedule[0].Period);
            Assert.AreEqual(setSettings.CabFilePurgeSchedule[0].Time.Hour, getSettings.CabFilePurgeSchedule[0].Time.Hour);
            Assert.AreEqual(setSettings.CabFilePurgeSchedule[0].Time.Minute, getSettings.CabFilePurgeSchedule[0].Time.Minute);
            Assert.AreEqual(setSettings.CabFilePurgeSchedule[0].Time.Second, getSettings.CabFilePurgeSchedule[0].Time.Second);

            Assert.AreEqual(setSettings.PurgeOptionsCollection.Count, getSettings.PurgeOptionsCollection.Count);
            for (int i = 0; i < setSettings.PurgeOptionsCollection.Count; i++)
            {
                Assert.AreEqual(setSettings.PurgeOptionsCollection[i].AgeToPurge, getSettings.PurgeOptionsCollection[i].AgeToPurge);
                Assert.AreEqual(setSettings.PurgeOptionsCollection[i].Id, getSettings.PurgeOptionsCollection[i].Id);
                Assert.AreEqual(setSettings.PurgeOptionsCollection[i].PurgeCabFiles, getSettings.PurgeOptionsCollection[i].PurgeCabFiles);
                Assert.AreEqual(setSettings.PurgeOptionsCollection[i].PurgeDumpFiles, getSettings.PurgeOptionsCollection[i].PurgeDumpFiles);
                Assert.AreEqual(setSettings.PurgeOptionsCollection[i].PurgeObject, getSettings.PurgeOptionsCollection[i].PurgeObject);
            }

            Assert.AreEqual(setSettings.PurgeOptionsCollection[0].AgeToPurge, getSettings.PurgeOptionsCollection[0].AgeToPurge);
            Assert.AreEqual(setSettings.PurgeOptionsCollection[0].Id, getSettings.PurgeOptionsCollection[0].Id);
            Assert.AreEqual(setSettings.PurgeOptionsCollection[0].PurgeCabFiles, getSettings.PurgeOptionsCollection[0].PurgeCabFiles);
            Assert.AreEqual(setSettings.PurgeOptionsCollection[0].PurgeDumpFiles, getSettings.PurgeOptionsCollection[0].PurgeDumpFiles);
            Assert.AreEqual(setSettings.PurgeOptionsCollection[0].PurgeObject, getSettings.PurgeOptionsCollection[0].PurgeObject);

            Assert.AreEqual(setSettings.DebuggerSettings.BinaryPath[0], getSettings.DebuggerSettings.BinaryPath[0]);
            Assert.AreEqual(setSettings.DebuggerSettings.BinaryPath64Bit[0], getSettings.DebuggerSettings.BinaryPath64Bit[0]);
            Assert.AreEqual(setSettings.DebuggerSettings.DebuggerPathAndFileName, getSettings.DebuggerSettings.DebuggerPathAndFileName);
            Assert.AreEqual(setSettings.DebuggerSettings.DebuggerPathAndFileName64Bit, getSettings.DebuggerSettings.DebuggerPathAndFileName64Bit);
            Assert.AreEqual(setSettings.DebuggerSettings.SymbolPath[0], getSettings.DebuggerSettings.SymbolPath[0]);
            Assert.AreEqual(setSettings.DebuggerSettings.SymbolPath64Bit[0], getSettings.DebuggerSettings.SymbolPath64Bit[0]);

            Assert.AreEqual(setSettings.ErrorIndexSettings.Folder, getSettings.ErrorIndexSettings.Folder);
            Assert.AreEqual(setSettings.ErrorIndexSettings.Name, getSettings.ErrorIndexSettings.Name);
            Assert.AreEqual(setSettings.ErrorIndexSettings.Type, getSettings.ErrorIndexSettings.Type);


            Assert.AreEqual(setSettings.WinQualSettings.AgeOldToPurgeInDays, getSettings.WinQualSettings.AgeOldToPurgeInDays);
            Assert.AreEqual(setSettings.WinQualSettings.CompanyName, getSettings.WinQualSettings.CompanyName);
            Assert.AreEqual(setSettings.WinQualSettings.Password, getSettings.WinQualSettings.Password);
            Assert.AreEqual(setSettings.WinQualSettings.UserName, getSettings.WinQualSettings.UserName);

            Assert.AreEqual(setSettings.WinQualSyncSchedule[0].DaysOfWeek, getSettings.WinQualSyncSchedule[0].DaysOfWeek);
            Assert.AreEqual(setSettings.WinQualSyncSchedule[0].Period, getSettings.WinQualSyncSchedule[0].Period);
            Assert.AreEqual(setSettings.WinQualSyncSchedule[0].Time.Hour, getSettings.WinQualSyncSchedule[0].Time.Hour);
            Assert.AreEqual(setSettings.WinQualSyncSchedule[0].Time.Minute, getSettings.WinQualSyncSchedule[0].Time.Minute);
            Assert.AreEqual(setSettings.WinQualSyncSchedule[0].Time.Second, getSettings.WinQualSyncSchedule[0].Time.Second);
        }

        public SetStackHashPropertiesResponse SetContextSettings(StackHashContextSettings contextSettings)
        {
            // Set the context fields.
            // Make sure it has been added and is inactive.
            SetStackHashPropertiesRequest request = new SetStackHashPropertiesRequest();
            request.ClientData = DefaultClientData;
            StackHashSettings settings = new StackHashSettings();
            settings.ContextCollection = new StackHashContextCollection();
            settings.ContextCollection.Add(contextSettings);
            request.Settings = settings;

            SetStackHashPropertiesResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetStackHashSettings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }


        public SetProxyResponse SetProxy(StackHashProxySettings proxySettings)
        {
            SetProxyRequest request = new SetProxyRequest();
            request.ClientData = DefaultClientData;
            request.ProxySettings = proxySettings;

            SetProxyResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetProxy(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public SetPurgeOptionsResponse SetPurgeOptions(int contextId, ScheduleCollection schedule, StackHashPurgeOptionsCollection purgeOptions, bool setAll)
        {
            SetPurgeOptionsRequest request = new SetPurgeOptionsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Schedule = schedule;
            request.PurgeOptions = purgeOptions;
            request.SetAll = setAll;

            SetPurgeOptionsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetPurgeOptions(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public ActivateStackHashContextResponse ActivateContext(int contextId)
        {
            ActivateStackHashContextRequest request = new ActivateStackHashContextRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;

            ActivateStackHashContextResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.ActivateStackHashContext(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public DeactivateStackHashContextResponse DeactivateContext(int contextId)
        {
            DeactivateStackHashContextRequest request = new DeactivateStackHashContextRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;

            DeactivateStackHashContextResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.DeactivateStackHashContext(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public EnableLoggingResponse EnableLogging()
        {
            EnableLoggingRequest request = new EnableLoggingRequest();
            request.ClientData = DefaultClientData;

            EnableLoggingResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.EnableLogging(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public SetClientTimeoutResponse SetClientTimeout(int clientTimeoutInSeconds)
        {
            SetClientTimeoutRequest request = new SetClientTimeoutRequest();
            request.ClientData = DefaultClientData;
            request.ClientTimeoutInSeconds = clientTimeoutInSeconds;

            SetClientTimeoutResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetClientTimeout(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        
        public GetBugTrackerPlugInDiagnosticsResponse GetBugTrackerPlugInDiagnostics(int contextId, String plugInName)
        {
            GetBugTrackerPlugInDiagnosticsRequest request = new GetBugTrackerPlugInDiagnosticsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.PlugInName = plugInName;

            GetBugTrackerPlugInDiagnosticsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetBugTrackerDiagnostics(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetContextBugTrackerPlugInSettingsResponse GetContextBugTrackerPlugInSettings(int contextId)
        {
            GetContextBugTrackerPlugInSettingsRequest request = new GetContextBugTrackerPlugInSettingsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;

            GetContextBugTrackerPlugInSettingsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetContextBugTrackerPlugInSettings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public SetContextBugTrackerPlugInSettingsResponse SetContextBugTrackerPlugInSettings(int contextId, StackHashBugTrackerPlugInSettings settings)
        {
            SetContextBugTrackerPlugInSettingsRequest request = new SetContextBugTrackerPlugInSettingsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.BugTrackerPlugInSettings = settings;

            SetContextBugTrackerPlugInSettingsResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetContextBugTrackerPlugInSettings(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        
        public DisableLoggingResponse DisableLogging()
        {
            DisableLoggingRequest request = new DisableLoggingRequest();
            request.ClientData = DefaultClientData;

            DisableLoggingResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.DisableLogging(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public EnableReportingResponse EnableReporting()
        {
            EnableReportingRequest request = new EnableReportingRequest();
            request.ClientData = DefaultClientData;

            EnableReportingResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.EnableReporting(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public DisableReportingResponse DisableReporting()
        {
            DisableReportingRequest request = new DisableReportingRequest();
            request.ClientData = DefaultClientData;

            DisableReportingResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.DisableReporting(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetLicenseDataResponse GetLicenseData()
        {
            GetLicenseDataRequest request = new GetLicenseDataRequest();
            request.ClientData = DefaultClientData;

            GetLicenseDataResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetLicenseData(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public SetLicenseResponse SetLicense(String licenseId)
        {
            SetLicenseRequest request = new SetLicenseRequest();
            request.ClientData = DefaultClientData;
            request.LicenseId = licenseId;

            SetLicenseResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.SetLicense(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public void RegisterForNotifications(bool isRegister, Guid appId)
        {
            RegisterRequest registerRequest = new RegisterRequest();
            registerRequest.ClientData = DefaultClientData;
            registerRequest.ClientData.ApplicationGuid = appId;
            registerRequest.IsRegister = isRegister;


            m_ServiceProxyControl.StackHashAdminClient.RegisterForNotifications(registerRequest);
        }
        public void RegisterForNotifications(bool isRegister, Guid appId, String clientName)
        {
            RegisterForNotifications(isRegister, appId, clientName, null);
        }


        public void RegisterForNotifications(bool isRegister, Guid appId, String clientName, String serviceGuid)
        {
            RegisterRequest registerRequest = new RegisterRequest();
            registerRequest.ClientData = DefaultClientData;
            registerRequest.ClientData.ApplicationGuid = appId;
            registerRequest.ClientData.ClientName = clientName;
            registerRequest.IsRegister = isRegister;
            registerRequest.ClientData.ServiceGuid = serviceGuid;

            m_ServiceProxyControl.StackHashAdminClient.RegisterForNotifications(registerRequest);
        }


        public CheckVersionResponse CheckVersion(int majorVersion, int minorVersion, String serviceGuid)
        {
            CheckVersionRequest checkVersionRequest = new CheckVersionRequest();
            checkVersionRequest.ClientData = DefaultClientData;
            checkVersionRequest.MajorVersion = majorVersion;
            checkVersionRequest.MinorVersion = minorVersion;
            checkVersionRequest.ServiceGuid = serviceGuid;


            return m_ServiceProxyControl.StackHashAdminClient.CheckVersion(checkVersionRequest);
        }

        public GetDebuggerScriptNamesResponse GetDebuggerScriptNames()
        {
            GetDebuggerScriptNamesRequest request = new GetDebuggerScriptNamesRequest();
            request.ClientData = DefaultClientData;

            GetDebuggerScriptNamesResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetDebuggerScriptNames(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }
        public AddDebuggerScriptResponse AddDebuggerScript(StackHashScriptSettings script, bool overwrite)
        {
            AddDebuggerScriptRequest request = new AddDebuggerScriptRequest();
            request.ClientData = DefaultClientData;
            request.Script = script;
            request.Overwrite = overwrite;

            AddDebuggerScriptResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.AddScript(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetDebuggerScriptResponse GetDebuggerScript(String scriptName)
        {
            GetDebuggerScriptRequest request = new GetDebuggerScriptRequest();
            request.ClientData = DefaultClientData;
            request.ScriptName = scriptName;

            GetDebuggerScriptResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetScript(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public RemoveDebuggerScriptResponse RemoveDebuggerScript(String scriptName)
        {
            RemoveDebuggerScriptRequest request = new RemoveDebuggerScriptRequest();
            request.ClientData = DefaultClientData;
            request.ScriptName = scriptName;

            RemoveDebuggerScriptResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RemoveScript(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public StackHashScriptSettings MakeScriptSettings(int i, bool addComment)
        {
            return MakeScriptSettings(i, addComment, StackHashScriptDumpType.UnmanagedAndManaged, false);
        }

        public StackHashScriptSettings MakeScriptSettings(int i, bool addComment, StackHashScriptDumpType dumpType, bool runAutomatically)
        {
            StackHashScript script1 = new StackHashScript();
            script1.Add(new StackHashScriptLine());
            script1[0].Command = "r";

            if (addComment)
                script1[0].Comment = "comment" + i.ToString();
            else
                script1[0].Comment = null;

            StackHashScriptSettings scriptSettings = new StackHashScriptSettings();
 //           scriptSettings.CreationDate = DateTime.Now.ToUniversalTime();
//            scriptSettings.LastModifiedDate = DateTime.Now.ToUniversalTime();
            scriptSettings.Name = "TestScript" + i.ToString();
            scriptSettings.Script = script1;
            scriptSettings.DumpType = dumpType;
            scriptSettings.RunAutomatically = runAutomatically;

            return scriptSettings;
        }

        public void CheckScriptSettings(StackHashScriptSettings settings1, StackHashScriptSettings settings2, bool checkDates)
        {
            if (checkDates)
            {
                Assert.AreEqual(true, settings2.CreationDate >= DateTime.Now.AddMinutes(-30).ToUniversalTime());
                Assert.AreEqual(true, settings2.LastModifiedDate >= DateTime.Now.AddMinutes(-30).ToUniversalTime());
                Assert.AreEqual(true, settings1.CreationDate <= settings2.CreationDate);
                Assert.AreEqual(true, settings1.LastModifiedDate <= settings2.LastModifiedDate);
                Assert.AreEqual(true, settings2.LastModifiedDate >= settings2.CreationDate);
            }
            Assert.AreEqual(settings1.Name, settings2.Name);
            Assert.AreEqual(settings1.DumpType, settings2.DumpType);
            Assert.AreEqual(settings1.IsReadOnly, settings2.IsReadOnly);
            Assert.AreEqual(settings1.Owner, settings2.Owner);
            Assert.AreEqual(settings1.RunAutomatically, settings2.RunAutomatically);
            Assert.AreEqual(settings1.Version, settings2.Version);

            for (int i = 0; i < settings1.Script.Count; i++)
            {
                Assert.AreEqual(settings1.Script[i].Command, settings2.Script[0].Command);
                Assert.AreEqual(settings1.Script[i].Comment, settings2.Script[0].Comment);
            }
        }

        public void RemoveAllScripts()
        {
            GetDebuggerScriptNamesResponse resp = this.GetDebuggerScriptNames();

            foreach (StackHashScriptFileData scriptFileData in resp.ScriptFileData)
            {
                if (!scriptFileData.IsReadOnly)
                    RemoveDebuggerScript(scriptFileData.Name);
            }
        }

        public RenameDebuggerScriptResponse RenameDebuggerScript(String scriptName, String newScriptName)
        {
            RenameDebuggerScriptRequest request = new RenameDebuggerScriptRequest();
            request.ClientData = DefaultClientData;
            request.OriginalScriptName = scriptName;
            request.NewScriptName = newScriptName;

            RenameDebuggerScriptResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RenameScript(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetDebugResultFilesResponse GetDebugResultFiles(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            GetDebugResultFilesRequest request = new GetDebugResultFilesRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;

            GetDebugResultFilesResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetDebugResultFiles(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetDebugResultResponse GetDebugResult(int contextId, String scriptName, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            GetDebugResultRequest request = new GetDebugResultRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ScriptName = scriptName;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;

            GetDebugResultResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.GetDebugResult(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public RemoveScriptResultResponse RemoveScriptResult(int contextId, String scriptName, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            RemoveScriptResultRequest request = new RemoveScriptResultRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ScriptName = scriptName;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;

            RemoveScriptResultResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RemoveScriptResult(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public RunDebuggerScriptResponse RunDebuggerScript(int contextId, String scriptName, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            RunDebuggerScriptRequest request = new RunDebuggerScriptRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;
            request.ScriptName = scriptName;

            RunDebuggerScriptResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RunScript(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public AbortTaskResponse AbortTask(int contextId, StackHashTaskType taskType)
        {
            AbortTaskRequest request = new AbortTaskRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.TaskType = taskType;

            AbortTaskResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.AbortTask(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }


        
        public RunDebuggerScriptAsyncResponse RunDebuggerScriptAsync(int contextId, StackHashScriptNamesCollection scriptNames, 
            StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int timeout)
        {
            RunDebuggerScriptAsyncRequest request = new RunDebuggerScriptAsyncRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;
            request.ScriptsToRun = scriptNames;

            RunDebuggerScriptAsyncResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.RunScriptAsync(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            Assert.AreEqual(true, m_RunScriptCompleteEvent.WaitOne(timeout));

            Assert.AreEqual(DefaultClientData.ApplicationGuid, m_RunScriptStartedAdminReport.ClientData.ApplicationGuid);
            Assert.AreEqual(DefaultClientData.ApplicationGuid, m_RunScriptAdminReport.ClientData.ApplicationGuid);
            return resp;
        }

        public GetProductsResponse GetProducts(int contextId)
        {
            GetProductsRequest request = new GetProductsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;

            GetProductsResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetProducts(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetProductEventPackageResponse GetProductEventPackages(int contextId, StackHashProduct product)
        {
            GetProductEventPackageRequest request = new GetProductEventPackageRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;

            GetProductEventPackageResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetProductEventPackages(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public SetEventBugIdResponse SetEventBugId(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, String bugId)
        {
            SetEventBugIdRequest request = new SetEventBugIdRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.BugId = bugId;


            SetEventBugIdResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.SetEventBugId(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public SetEventWorkFlowStatusResponse SetWorkFlowStatus(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int workFlowStatus)
        {
            SetEventWorkFlowStatusRequest request = new SetEventWorkFlowStatusRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.WorkFlowStatus = workFlowStatus;


            SetEventWorkFlowStatusResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.SetWorkFlowStatus(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        
        public GetFilesResponse GetFiles(int contextId, StackHashProduct product)
        {
            GetFilesRequest request = new GetFilesRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;

            GetFilesResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetFiles(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetEventsResponse GetEvents(int contextId, StackHashProduct product, StackHashFile file)
        {
            GetEventsRequest request = new GetEventsRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;

            GetEventsResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetEvents(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public GetWindowedEventPackageResponse GetWindowedEvents(int contextId, StackHashSearchCriteriaCollection searchCriteria, long startRow, 
            long numRows, StackHashSortOrderCollection sortOrder, StackHashSearchDirection direction, bool countAllMatches)
        {
            GetWindowedEventPackageRequest request = new GetWindowedEventPackageRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.SearchCriteriaCollection = searchCriteria;
            request.StartRow = startRow;
            request.NumberOfRows = numRows;
            request.SortOrder = sortOrder;
            request.Direction = direction;
            request.CountAllMatches = countAllMatches;

            GetWindowedEventPackageResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetWindowedEventPackages(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        
        public GetCabNotesResponse GetCabNotes(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            GetCabNotesRequest request = new GetCabNotesRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;

            GetCabNotesResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetCabNotes(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public AddCabNoteResponse AddCabNote(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note)
        {
            AddCabNoteRequest request = new AddCabNoteRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;
            request.NoteEntry = note;

            AddCabNoteResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.AddCabNote(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public DeleteCabNoteResponse DeleteCabNote(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId)
        {
            DeleteCabNoteRequest request = new DeleteCabNoteRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;
            request.NoteId = noteId;

            DeleteCabNoteResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.DeleteCabNote(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        
        public GetCabPackageResponse GetCabPackage(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            GetCabPackageRequest request = new GetCabPackageRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.Cab = cab;

            GetCabPackageResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetCabPackage(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        
        public GetEventPackageResponse GetEventPackage(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            GetEventPackageRequest request = new GetEventPackageRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;

            GetEventPackageResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetEventPackage(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public CreateTestIndexResponse CreateTestIndex(int contextId, StackHashTestIndexData indexData)
        {
            CreateTestIndexRequest request = new CreateTestIndexRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.TestIndexData = indexData;

            CreateTestIndexResponse resp =
                m_ServiceProxyControl.StackHashTestClient.CreateTestIndex(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public String GetTestModeSetting(String attributeName)
        {
            GetTestModeSettingRequest request = new GetTestModeSettingRequest();
            request.ClientData = DefaultClientData;
            request.AttributeName = attributeName;

            GetTestModeSettingResponse resp =
                m_ServiceProxyControl.StackHashTestClient.GetTestModeSetting(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp.AttributeValue;
        }

        public GetProductRollupResponse GetProductSummary(int contextId, int productId)
        {
            GetProductRollupRequest request = new GetProductRollupRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.ProductId = productId;

            GetProductRollupResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetProductSummary(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }

        public SetTestDataResponse SetTestData(StackHashTestData testData)
        {
            SetTestDataRequest request = new SetTestDataRequest();
            request.ClientData = DefaultClientData;
            request.TestData = testData;

            SetTestDataResponse resp =
                m_ServiceProxyControl.StackHashTestClient.SetTestData(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);
            return resp;
        }
        public DeleteIndexResponse DeleteIndex(int contextId)
        {
            try
            {
                DeleteIndexRequest request = new DeleteIndexRequest();
                request.ClientData = DefaultClientData;
                request.ContextId = contextId;

                DeleteIndexResponse resp =
                    m_ServiceProxyControl.StackHashAdminClient.DeleteIndex(request);

                Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
                Assert.AreEqual(null, resp.ResultData.LastException);
                return resp;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("UTILS: Failed to delete database", ex.ToString());
                return null;
            }
        }

        public MoveIndexResponse MoveIndex(int contextId, String newIndexPath, String newIndexName, int timeout, StackHashSqlConfiguration newSqlSettings)
        {
            MoveIndexRequest request = new MoveIndexRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.NewErrorIndexPath = newIndexPath;
            request.NewErrorIndexName = newIndexName;
            request.NewSqlSettings = newSqlSettings;

            MoveIndexResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.MoveIndex(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the task to complete.
            Assert.AreEqual(true, m_MoveCompleteEvent.WaitOne(timeout));
            return resp;
        }

        public CopyIndexResponse CopyIndex(int contextId, ErrorIndexSettings destSettings, StackHashSqlConfiguration sqlConfig, bool switchIndex, int timeout)
        {
            CopyIndexRequest request = new CopyIndexRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.DestinationErrorIndexSettings = destSettings;
            request.SqlSettings = sqlConfig;
            request.SwitchIndexWhenCopyComplete = switchIndex;

            CopyIndexResponse resp =
                m_ServiceProxyControl.StackHashAdminClient.CopyIndex(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            // Wait for the task to complete.
            Assert.AreEqual(true, m_CopyCompleteEvent.WaitOne(timeout));
            return resp;
        }

        public void CorruptFile(String fileName)
        {
            FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            try
            {
                int numBytes = (int)(fileStream.Length / 2);

                fileStream.Seek(numBytes, SeekOrigin.Begin);

                byte[] bytes = new byte[numBytes];

                fileStream.Write(bytes, 0, numBytes - 1);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        // This method accepts two strings the represent two files to 
        // compare. A return value of 0 indicates that the contents of the files
        // are the same. A return value of any other value indicates that the 
        // files are not the same.
        public bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        public static void DeleteLogFiles(String folder)
        {
            if (!Directory.Exists(folder))
                return;

            String[] files = Directory.GetFiles(folder, "*.txt");

            foreach (String file in files)
            {
                File.Delete(file);
            }
        }


        public AddEventNoteResponse AddEventNote(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note)
        {
            AddEventNoteRequest request = new AddEventNoteRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.NoteEntry = note;

            AddEventNoteResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.AddEventNote(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public DeleteEventNoteResponse DeleteEventNote(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId)
        {
            DeleteEventNoteRequest request = new DeleteEventNoteRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;
            request.NoteId = noteId;

            DeleteEventNoteResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.DeleteEventNote(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }

        public GetEventNotesResponse GetEventNotes(int contextId, StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            GetEventNotesRequest request = new GetEventNotesRequest();
            request.ClientData = DefaultClientData;
            request.ContextId = contextId;
            request.Product = product;
            request.File = file;
            request.Event = theEvent;

            GetEventNotesResponse resp =
                m_ServiceProxyControl.StackHashProjectsClient.GetEventNotes(request);

            Assert.AreEqual(StackHashServiceResult.Success, resp.ResultData.Result);
            Assert.AreEqual(null, resp.ResultData.LastException);

            return resp;
        }
        

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    RegisterForNotifications(false, ApplicationGuid);
                }
                catch
                {
                }
                if (m_ServiceProxyControl != null)
                {
                    m_ServiceProxyControl.AdminReport -= new EventHandler<AdminReportEventArgs>(this.AdminReportCallback);
                    m_ServiceProxyControl.Dispose();
                    m_ServiceProxyControl = null;
                }
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
