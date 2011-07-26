using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;

namespace StackHashServiceContracts
{
    /// <summary>
    /// Parameters to get the products list for a given context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetProductsRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;

        /// <summary>
        /// The context ID for which projects are required.
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
    /// Contains the results of a request to get a list of products for a given context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetProductsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProductInfoCollection m_Products;
        DateTime m_LastSiteUpdateTime;

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
        /// A list of products associated with a particular context.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashProductInfoCollection Products
        {
            get { return m_Products; }
            set { m_Products = value; }
        }

        /// <summary>
        /// Last time the site was updated.
        /// </summary>
        [DataMember]
        public DateTime LastSiteUpdateTime
        {
            get { return m_LastSiteUpdateTime; }
            set { m_LastSiteUpdateTime = value; }
        }
    }

    /// <summary>
    /// Parameters to get the files list for a given product in a given context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetFilesRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;

        /// <summary>
        /// The context ID for which projects files are required.
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
        /// Product for which the files are required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get a list of files for a given product.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetFilesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFileCollection m_Files;

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
        /// Product to who the files refer.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// A list of files associated with a particular product.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashFileCollection Files
        {
            get { return m_Files; }
            set { m_Files = value; }
        }
    }

    /// <summary>
    /// Parameters to get the event list for a given product file in a given context.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetEventsRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;

        /// <summary>
        /// The context ID for which projects file events are required.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Product for which the files events are required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File for which the events are required.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
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
    /// Contains the results of a request to get a list of events for a given product file.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetEventsResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEventCollection m_Events;

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
        /// Product to who the files events refer.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to who the events refer.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }
        
        /// <summary>
        /// A list of files events associated with a particular product file.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashEventCollection Events
        {
            get { return m_Events; }
            set { m_Events = value; }
        }
    }


    /// <summary>
    /// Parameters to get the event data associated with a particular event.
    /// This includes the EventInfo (instance information) and the cab file 
    /// data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetEventPackageRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;

        /// <summary>
        /// The context ID for which projects file events are required.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        /// <summary>
        /// Product for which the files event data is required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File for which the event data is required.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event for which the event data is required.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
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
    /// Contains the results of a request to get event data for a given product file event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetEventPackageResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashEventPackage m_EventPackage;

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
        /// A list of files events associated with a particular product file.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashEventPackage EventPackage
        {
            get { return m_EventPackage; }
            set { m_EventPackage = value; }
        }
    }

    /// <summary>
    /// Parameters to set the bugID of an event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetEventBugIdRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        String m_BugId;

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
        /// Product owning the event.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File owning the event.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to set.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Bug ID to set.
        /// </summary>
        [DataMember]
        public String BugId
        {
            get { return m_BugId; }
            set { m_BugId = value; }
        }

    }

    /// <summary>
    /// Contains the results of a request to set the bug ID for an event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class SetEventBugIdResponse
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
    /// Parameters to set the WorkFlowStatus of an event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [SuppressMessage("Microsoft.Naming", "CA1702")]
    public class SetEventWorkFlowStatusRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        int m_WorkFlowStatus;

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
        /// Product owning the event.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File owning the event.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to set.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// WorkFlow status to set.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public int WorkFlowStatus
        {
            get { return m_WorkFlowStatus; }
            set { m_WorkFlowStatus = value; }
        }

    }

    /// <summary>
    /// Contains the results of a request to set the work flow status for an event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [SuppressMessage("Microsoft.Naming", "CA1702")]
    public class SetEventWorkFlowStatusResponse
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
    /// Parameters to get the event data associated with a particular product.
    /// This includes the EventInfo (instance information) and the cab file 
    /// data for each event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetProductEventPackageRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;

        /// <summary>
        /// The context ID for which projects file events are required.
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
        /// Product for which the files event data is required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get event data for a given product.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetProductEventPackageResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashEventPackageCollection m_EventPackages;

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
        /// Product to which the files event data refers.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// A list of files events associated with a particular product file.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashEventPackageCollection EventPackages
        {
            get { return m_EventPackages; }
            set { m_EventPackages = value; }
        }
    }

    /// <summary>
    /// Parameters to get the event data matching particular search criteria.
    /// This includes the EventInfo (instance information) and the cab file 
    /// data for each event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetAllEventPackageRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashSearchCriteriaCollection m_SearchCriteriaCollection;

        /// <summary>
        /// The context ID for which events are required.
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
        /// Search criteria. Only events matching this criteria will be returned.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashSearchCriteriaCollection SearchCriteriaCollection
        {
            get { return m_SearchCriteriaCollection; }
            set { m_SearchCriteriaCollection = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get event data for a given product.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetAllEventPackageResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashEventPackageCollection m_EventPackages;

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
        /// A list of files events associated with a particular product file.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashEventPackageCollection EventPackages
        {
            get { return m_EventPackages; }
            set { m_EventPackages = value; }
        }
    }

    /// <summary>
    /// Parameters to get the event data matching particular search criteria.
    /// This includes the EventInfo (instance information) and the cab file 
    /// data for each event.
    /// Only events within the specified ordered window are returned.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetWindowedEventPackageRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashSearchCriteriaCollection m_SearchCriteriaCollection;
        long m_StartRow;
        long m_NumberOfRows;
        StackHashSortOrderCollection m_SortOrder;
        StackHashSearchDirection m_Direction;
        bool m_CountAllMatches;

        /// <summary>
        /// The context ID for which events are required.
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
        /// Search criteria. Only events matching this criteria will be returned.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashSearchCriteriaCollection SearchCriteriaCollection
        {
            get { return m_SearchCriteriaCollection; }
            set { m_SearchCriteriaCollection = value; }
        }

        /// <summary>
        /// First row in sorted event list to return (1 upward).
        /// </summary>
        [DataMember]
        public long StartRow
        {
            get { return m_StartRow; }
            set { m_StartRow = value; }
        }

        /// <summary>
        /// First row in sorted event list to return (1 upward).
        /// </summary>
        [DataMember]
        public long NumberOfRows
        {
            get { return m_NumberOfRows; }
            set { m_NumberOfRows = value; }
        }

        /// <summary>
        /// Search criteria. Only events matching this criteria will be returned.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashSortOrderCollection SortOrder
        {
            get { return m_SortOrder; }
            set { m_SortOrder = value; }
        }

        /// <summary>
        /// Forwards or backwards.
        /// </summary>
        [DataMember]
        public StackHashSearchDirection Direction
        {
            get { return m_Direction; }
            set { m_Direction = value; }
        }

        /// <summary>
        /// True - count all matches following this window.
        /// </summary>
        [DataMember]
        public bool CountAllMatches
        {
            get { return m_CountAllMatches; }
            set { m_CountAllMatches = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get windowed event data for a given product.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetWindowedEventPackageResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashEventPackageCollection m_EventPackages;
        long m_MaximumRowNumber;
        long m_MinimumRowNumber;
        long m_TotalRows;

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
        /// A list of files events associated with a particular product file.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashEventPackageCollection EventPackages
        {
            get { return m_EventPackages; }
            set { m_EventPackages = value; }
        }

        /// <summary>
        /// Maximum row number in search (not window).
        /// </summary>
        [DataMember]
        public long MaximumRowNumber
        {
            get { return m_MaximumRowNumber; }
            set { m_MaximumRowNumber = value; }
        }

        /// <summary>
        /// Minimum row number in search (not window).
        /// </summary>
        [DataMember]
        public long MinimumRowNumber
        {
            get { return m_MinimumRowNumber; }
            set { m_MinimumRowNumber = value; }
        }

        /// <summary>
        /// Total rows if available.
        /// </summary>
        [DataMember]
        public long TotalRows
        {
            get { return m_TotalRows; }
            set { m_TotalRows = value; }
        }
    
    }

    /// <summary>
    /// Parameters to get product rollup data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetProductRollupRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        int m_ProductId;

        /// <summary>
        /// The context ID for which rollup data is required.
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
        /// ID of the product for which rollup data is required.
        /// </summary>
        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get product rollup data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetProductRollupResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProductSummary m_RollupData;

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
        /// Rollup data associated with the product.
        /// </summary>
        [DataMember]
        public StackHashProductSummary RollupData
        {
            get { return m_RollupData; }
            set { m_RollupData = value; }
        }
    }

    /// <summary>
    /// Parameters to run the bug report task.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunBugReportTaskRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashBugReportDataCollection m_BugReportCollection;
        StackHashBugTrackerPlugInSelectionCollection m_PlugIns;

        /// <summary>
        /// The context ID for which rollup data is required.
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
        /// What is to be reported.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugReportDataCollection BugReportDataCollection
        {
            get { return m_BugReportCollection; }
            set { m_BugReportCollection = value; }
        }

        /// <summary>
        /// Plugins that should be reported to.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashBugTrackerPlugInSelectionCollection PlugIns
        {
            get { return m_PlugIns; }
            set { m_PlugIns = value; }
        }
    }


    /// <summary>
    /// Contains the results of a request to run a bug report.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class RunBugReportTaskResponse
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
    /// Parameters to get the notes associated with a particular cab.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetCabNotesRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;

        /// <summary>
        /// The context ID for which cab notes are required.
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
        /// The product for which cab notes are required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for which cab notes are required.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for which cab notes are required.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// The cab for which cab notes are required.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get cab notes.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetCabNotesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        StackHashNotes m_Notes;

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
        /// Product to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        
        /// <summary>
        /// Notes recorded on the cab.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashNotes Notes
        {
            get { return m_Notes; }
            set { m_Notes = value; }
        }
    }

    /// <summary>
    /// Parameters to get the notes associated with a particular event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetEventNotesRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;

        /// <summary>
        /// The context ID for which event notes are required.
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
        /// The product for which event notes are required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for which event notes are required.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for which event notes are required.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get event notes.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetEventNotesResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashNotes m_Notes;

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
        /// Product to which the event notes refer.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to which the event notes refer.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to which the event notes refer.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }


        /// <summary>
        /// Notes recorded on the event.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227")]
        public StackHashNotes Notes
        {
            get { return m_Notes; }
            set { m_Notes = value; }
        }
    }

    /// <summary>
    /// Parameters to add a note to a particular cab.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AddCabNoteRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        StackHashNoteEntry m_NoteEntry;

        /// <summary>
        /// The context ID for the cab.
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
        /// The product for the cab.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for the cab.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for the cab.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// The cab to add a note to.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// The note to add.
        /// </summary>
        [DataMember]
        public StackHashNoteEntry NoteEntry
        {
            get { return m_NoteEntry; }
            set { m_NoteEntry = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to add a cab note. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AddCabNoteResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;

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
        /// Product to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab to which the cab notes refer.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }
    }

    /// <summary>
    /// Parameters to delete a note from a particular cab.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeleteCabNoteRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        int m_NoteId;

        /// <summary>
        /// The context ID for the cab.
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
        /// The product for the cab.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for the cab.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for the cab.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// The cab to delete a note from.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// The note to delete.
        /// </summary>
        [DataMember]
        public int NoteId
        {
            get { return m_NoteId; }
            set { m_NoteId = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to delete a cab note. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeleteCabNoteResponse
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
    /// Parameters to add a note to a particular event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AddEventNoteRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashNoteEntry m_NoteEntry;

        /// <summary>
        /// The context ID for the event.
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
        /// The product for the event.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for the event.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for the event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// The note to add.
        /// </summary>
        [DataMember]
        public StackHashNoteEntry NoteEntry
        {
            get { return m_NoteEntry; }
            set { m_NoteEntry = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to add an event note. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class AddEventNoteResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;

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
        /// Product to which the event notes refer.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to which the event notes refer.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to which the event notes refer.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }
    }

    /// <summary>
    /// Parameters to delete a note to a particular event.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeleteEventNoteRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        int m_NoteId;

        /// <summary>
        /// The context ID for the event.
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
        /// The product for the event.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for the event.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for the event.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// The note to delete.
        /// </summary>
        [DataMember]
        public int NoteId
        {
            get { return m_NoteId; }
            set { m_NoteId = value; }
        }
    }
    
    /// <summary>
    /// Contains the results of a request to delete an event note. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DeleteEventNoteResponse
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
    /// Parameters to download a cab.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DownloadCabRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;

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
        /// Product owning the cab.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File owing the cab.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event owing the cab.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab to download.
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
    }

    /// <summary>
    /// Contains the results of a request download a cab.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class DownloadCabResponse
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
    /// Parameters to get extra information associated with a particular cab.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetCabPackageRequest
    {
        int m_ContextId;
        StackHashClientData m_ClientData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;

        /// <summary>
        /// The context ID for which cab notes are required.
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
        /// The product for which cab data is required.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// The file for which cab data is required.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// The event for which cab data is required.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// The cab for which cab data is required.
        /// </summary>
        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }
    }

    /// <summary>
    /// Contains the results of a request to get cab extended data.
    /// </summary>
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class GetCabPackageResponse
    {
        StackHashServiceResultData m_ResultData;
        StackHashProduct m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCabPackage m_CabPackage;

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
        /// Product to which the cab data refers.
        /// </summary>
        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File to which the cab data refers.
        /// </summary>
        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event to which the cab data refers.
        /// </summary>
        [DataMember]
        public StackHashEvent Event
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab to which the cab data refers.
        /// </summary>
        [DataMember]
        public StackHashCabPackage CabPackage
        {
            get { return m_CabPackage; }
            set { m_CabPackage = value; }
        }
    }


    
    /// <summary>
    /// Service Contract provided to the StackHash client. 
    /// This contract permits the client to analyse project crash dump details.
    /// </summary>

    [ServiceContract(Namespace = "http://www.cucku.com/stackhash/2010/02/17",
                SessionMode=SessionMode.Required)]
    public interface IProjectsContract
    {
        /// <summary>
        /// Gets a list of all products.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetProductsResponse GetProducts(GetProductsRequest requestData);

        /// <summary>
        /// Gets a list of files associated with a products.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetFilesResponse GetFiles(GetFilesRequest requestData);

        /// <summary>
        /// Gets a list of files events associated with a product file.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetEventsResponse GetEvents(GetEventsRequest requestData);

        /// <summary>
        /// Gets the list of EventInfo and Cab data associated with an Event.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetEventPackageResponse GetEventPackage(GetEventPackageRequest requestData);

        /// <summary>
        /// Gets the list of EventInfo and Cab data associated with a product.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetProductEventPackageResponse GetProductEventPackages(GetProductEventPackageRequest requestData);

        /// <summary>
        /// Gets the list of EventInfo and Cab data associated with events matching the specified criteria.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetAllEventPackageResponse GetEventPackages(GetAllEventPackageRequest requestData);

        /// <summary>
        /// Gets the list of EventInfo and Cab data associated with events matching the specified criteria
        /// and withing the specified row range when ordered as specified.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetWindowedEventPackageResponse GetWindowedEventPackages(GetWindowedEventPackageRequest requestData);

        /// <summary>
        /// Gets notes on a cab.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetCabNotesResponse GetCabNotes(GetCabNotesRequest requestData);

        /// <summary>
        /// Adds a note to the cab.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        AddCabNoteResponse AddCabNote(AddCabNoteRequest requestData);

        /// <summary>
        /// Deletes a note to the cab.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DeleteCabNoteResponse DeleteCabNote(DeleteCabNoteRequest requestData);

        
        /// <summary>
        /// Gets notes on an event.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetEventNotesResponse GetEventNotes(GetEventNotesRequest requestData);

        /// <summary>
        /// Adds a note to an event.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        AddEventNoteResponse AddEventNote(AddEventNoteRequest requestData);

        /// <summary>
        /// Delete a note from an event.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DeleteEventNoteResponse DeleteEventNote(DeleteEventNoteRequest requestData);
        
        /// <summary>
        /// Sets the bug ID associated with an event.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        SetEventBugIdResponse SetEventBugId(SetEventBugIdRequest requestData);

        /// <summary>
        /// Sets the work flow status associated with an event.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        SetEventWorkFlowStatusResponse SetWorkFlowStatus(SetEventWorkFlowStatusRequest requestData);

        /// <summary>
        /// Downloads a cab.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        DownloadCabResponse DownloadCab(DownloadCabRequest requestData);

        /// <summary>
        /// Gets the rollup stats for a product.
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetProductRollupResponse GetProductSummary(GetProductRollupRequest requestData);


        /// <summary>
        /// Run BugReportTask
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        RunBugReportTaskResponse RunBugReportTask(RunBugReportTaskRequest requestData);

        /// <summary>
        /// Gets a cab package (extended cab information).
        /// </summary>
        /// <returns>Response result code.</returns>
        [OperationContract]
        [FaultContract(typeof(ReceiverFaultDetail))]
        [FaultContract(typeof(SenderFaultDetail))]
        GetCabPackageResponse GetCabPackage(GetCabPackageRequest requestData);
    }
}
