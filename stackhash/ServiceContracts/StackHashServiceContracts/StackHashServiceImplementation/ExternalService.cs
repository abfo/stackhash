using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

using StackHashServiceContracts;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashServiceImplementation
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class ExternalService : IProjectsContract
    {
        private const String s_OperationSuccessful = "Operation successful";

        #region IProjectsContract Members

        /// <summary>
        /// Gets the product data associated with a particular context.
        /// </summary>
        /// <returns>Response result code.</returns>
        public GetProductsResponse GetProducts(GetProductsRequest requestData)
        {
            if (requestData == null)
                throw new ArgumentNullException("requestData");

            GetProductsResponse resp = new GetProductsResponse();

            StackHashProductInfoCollection products =
                StaticObjects.TheStaticObjects.TheController.GetProducts(requestData.ContextId);

            resp.Products = products;
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

            GetEventsResponse resp = new GetEventsResponse();

            StackHashEventCollection events =
                StaticObjects.TheStaticObjects.TheController.GetEvents(
                    requestData.ContextId, requestData.Product, requestData.File);

            resp.Events = events;

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

            GetWindowedEventPackageResponse resp = new GetWindowedEventPackageResponse();
            StackHashEventPackageCollection eventPackages =
                StaticObjects.TheStaticObjects.TheController.GetEvents(requestData.ContextId, requestData.SearchCriteriaCollection,
                    requestData.StartRow, requestData.NumberOfRows, requestData.SortOrder);

            resp.EventPackages = eventPackages;

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

            GetProductRollupResponse resp = new GetProductRollupResponse();
            resp.RollupData =
                StaticObjects.TheStaticObjects.TheController.GetProductSummary(requestData.ContextId, requestData.ProductId);

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

            GetAllEventPackageResponse resp = new GetAllEventPackageResponse();

            StackHashEventPackageCollection eventPackages =
                StaticObjects.TheStaticObjects.TheController.GetEvents(requestData.ContextId, requestData.SearchCriteriaCollection);

            resp.EventPackages = eventPackages;

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

            GetCabNotesResponse resp = new GetCabNotesResponse();
            resp.Product = requestData.Product;
            resp.File = requestData.File;
            resp.Event = requestData.Event;
            resp.Cab = requestData.Cab;
            resp.Notes = StaticObjects.TheStaticObjects.TheController.GetCabNotes(requestData.ContextId,
                requestData.Product, requestData.File, requestData.Event, requestData.Cab);

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

            SetEventBugIdResponse resp = new SetEventBugIdResponse();

            StaticObjects.TheStaticObjects.TheController.SetEventBugId(requestData.ContextId, requestData.Product, requestData.File, requestData.Event, requestData.BugId);
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

            RunBugReportTaskResponse resp = new RunBugReportTaskResponse();
            StaticObjects.TheStaticObjects.TheController.RunBugReportTask(requestData.ContextId, requestData.ClientData, 
                requestData.BugReportDataCollection, requestData.PlugIns);

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

            SetEventWorkFlowStatusResponse resp = new SetEventWorkFlowStatusResponse();

            StaticObjects.TheStaticObjects.TheController.SetWorkFlowStatus(requestData.ContextId, requestData.Product, requestData.File, requestData.Event, requestData.WorkFlowStatus);
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

        #endregion
    }
}
