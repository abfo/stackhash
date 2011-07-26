using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public interface IAtomFeed
    {
        String UserName
        {
            set;
        }

        String Password
        {
            set;
        }

        /// <summary>
        /// Aborts the currently running operation if there is one.
        /// </summary>
        void AbortCurrentOperation();

        /// <summary>
        /// Sets the proxy settings for the service.
        /// </summary>
        void SetProxySettings(StackHashProxySettings proxySettings);

        /// <summary>
        /// Logs in to the WinQual service.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>true - success, false - failed to login.</returns>
        bool Login(string userName, string password);

        /// <summary>
        /// Logs out of the WinQual service.
        /// </summary>
        /// <returns>true - success, false - failed to login.</returns>
        void LogOut();

        /// <summary>
        /// Get the product information associated with this WinQual logon.
        /// </summary>
        /// <returns>Collection of product data.</returns>
        AtomProductCollection GetProducts();

        /// <summary>
        /// Get a list of all files associated with a product.
        /// </summary>
        /// <param name="product">Product for which the files are required.</param>
        /// <returns>List of files.</returns>
        AtomFileCollection GetFiles(AtomProduct product);

        /// <summary>
        /// Gets a list of events for the specified file.
        /// </summary>
        /// <param name="file">The file to get the events for.</param>
        /// <returns>Collection of events for the file.</returns>
        AtomEventCollection GetEvents(AtomFile file);

        /// <summary>
        /// Gets a list of events for the specified file.
        /// </summary>
        /// <param name="file">The file to get the events for.</param>
        /// <param name="startTime">Time of first event we are interested in.</param>
        /// <returns>Collection of events for the file.</returns>
        AtomEventCollection GetEvents(AtomFile file, DateTime startTime);

        /// <summary>
        /// Gets a list of events in the specified PAGE for the specified file.
        /// </summary>
        /// <param name="eventPageUrl">Url of the page to get.</param>
        /// <param name="file">File for which the events are required.</param>
        /// <param name="totalPages">Total number of pages.</param>
        /// <param name="currentPage">Current page.</param>
        /// <returns>Collection of events for the file.</returns>
        AtomEventCollection GetEventsPage(ref String eventPageUrl, AtomFile file, out int totalPages, out int currentPage);

        /// <summary>
        /// Returns a list of event infos for the specified event.
        /// </summary>
        /// <param name="theEvent">Event for which the event info is required.</param>
        /// <param name="days">Number of days of events to get - max is 90.</param>
        /// <returns>Collection of event infos.</returns>
        AtomEventInfoCollection GetEventDetails(AtomEvent theEvent, int days);

        /// <summary>
        /// Returns a list of Cab data.
        /// </summary>
        /// <param name="theEvent">Event for which the event info is required.</param>
        /// <returns>Collection of event infos.</returns>
        AtomCabCollection GetCabs(AtomEvent theEvent);

        /// <summary>
        /// Download Cab to the specified filename.
        /// </summary>
        /// <param name="cab">Cab data to download.</param>
        /// <param name="overwrite">True - overwrite, false - don't overwrite.</param>
        /// <param name="folder">Full path of the cab.</param>
        /// <returns>Filename.</returns>
        String DownloadCab(AtomCab cab, bool overwrite, String folder);

        /// <summary>
        /// Sets the web request settings.
        /// </summary>
        /// <param name="requestRetryCount">Number of times to retry following a timeout failure.</param>
        /// <param name="requestTimeout">Time to wait for a single response in milliseconds.</param>
        void SetWebRequestSettings(int requestRetryCount, int requestTimeout);

        /// <summary>
        /// Uploads a mapping file.
        /// </summary>
        /// <param name="fileName">File to be uploaded.</param>
        void UploadFile(String fileName);
    }
}
