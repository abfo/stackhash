using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackHashBusinessObjects;


namespace StackHashWinQual
{
    /// <summary>
    /// Shows the progress of a synchronize.
    /// </summary>
    public enum WinQualProgressType
    {
        /// <summary>
        /// Downloading the product list.
        /// </summary>
        DownloadingProductList,

        /// <summary>
        /// Completed download of the product list.
        /// </summary>
        ProductListUpdated,

        /// <summary>
        /// Downloading the events associated with a product.
        /// </summary>
        DownloadingProductEvents,

        /// <summary>
        /// Completed download of events associated with a product.
        /// </summary>
        ProductEventsUpdated,

        /// <summary>
        /// Downloading cabs associated with an event.
        /// </summary>
        DownloadingProductCabs,

        /// <summary>
        /// Completed downloading cabs associated with an event.
        /// </summary>
        ProductCabsUpdated,

        /// <summary>
        /// Sync complete.
        /// </summary>
        Complete,

        /// <summary>
        /// Downloading a page of events for a particular product file.
        /// </summary>
        DownloadingEventPage,

        /// <summary>
        /// Downloading a cab file.
        /// </summary>
        DownloadingCab
    }

    /// <summary>
    /// Progress report event arguments. Indicates the progress of a sync.
    /// </summary>
    public class WinQualProgressEventArgs : EventArgs
    {
        WinQualProgressType m_ProgressType;
        StackHashProductInfo m_Product;
        StackHashFile m_File;
        StackHashEvent m_Event;
        StackHashCab m_Cab;
        int m_TotalPages;
        int m_CurrentPage;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="progressType">Progress type - e.g. downloading cabs.</param>
        /// <param name="product">The owning product.</param>
        public WinQualProgressEventArgs(WinQualProgressType progressType, StackHashProductInfo product)
        {
            m_ProgressType = progressType;
            m_Product = product;
            m_TotalPages = 0;
            m_CurrentPage = 0;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="progressType">Progress type - e.g. downloading cabs.</param>
        /// <param name="product">The owning product.</param>
        /// <param name="currentPage">The current page being downloaded.</param>
        /// <param name="totalPages">Total pages to download.</param>
        public WinQualProgressEventArgs(WinQualProgressType progressType, StackHashProductInfo product, int currentPage, int totalPages)
        {
            m_ProgressType = progressType;
            m_Product = product;
            m_TotalPages = totalPages;
            m_CurrentPage = currentPage;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="progressType">Progress type - e.g. downloading cabs.</param>
        /// <param name="product">The owning product.</param>
        /// <param name="file">The owning file.</param>
        /// <param name="theEvent">The owning event.</param>
        /// <param name="cab">The cab being downloaded.</param>
        /// <param name="currentPage">The current page being downloaded.</param>
        /// <param name="totalPages">Total pages to download.</param>
        public WinQualProgressEventArgs(WinQualProgressType progressType, StackHashProductInfo product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int currentPage, int totalPages)
        {
            m_ProgressType = progressType;
            m_Product = product;
            m_File = file;
            m_Event = theEvent;
            m_Cab = cab;
            m_TotalPages = totalPages;
            m_CurrentPage = currentPage;
        }

        
        /// <summary>
        /// The type of progress being reported. e.g. DownloadingCabs.
        /// </summary>
        public WinQualProgressType ProgressType
        {
            get { return m_ProgressType; }
        }


        /// <summary>
        /// Product being synced.
        /// </summary>
        public StackHashProductInfo Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        /// <summary>
        /// File being synced.
        /// </summary>
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Event being synced.
        /// </summary>
        public StackHashEvent TheEvent
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        /// <summary>
        /// Cab being synced.
        /// </summary>
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        /// <summary>
        /// Total pages.
        /// </summary>
        public int TotalPages
        {
            get { return m_TotalPages; }
            set { m_TotalPages = value; }
        }

        /// <summary>
        /// Current pages.
        /// </summary>
        public int CurrentPage
        {
            get { return m_CurrentPage; }
            set { m_CurrentPage = value; }
        }
    }
}
