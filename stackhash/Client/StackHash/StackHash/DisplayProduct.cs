using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using StackHash.StackHashService;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// WinQual product wrapper class
    /// </summary>
    public class DisplayProduct : INotifyPropertyChanged
    {
        private StackHashProductInfo _productInfo;
        private DisplayCollectionPolicy _cabCollectionPolicy;
        private DisplayCollectionPolicy _eventCollectionPolicy;
        private int _eventDisplayHitThreshold;
        private StackHashProductHitDateSummaryCollection _hitDateSummary;
        private StackHashProductLocaleSummaryCollection _localeSummary;
        private StackHashProductOperatingSystemSummaryCollection _osSummary;

        /// <summary>
        /// Gets the summary of hits by hit date (may be null)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public StackHashProductHitDateSummaryCollection HitDateSummary
        {
            get { return _hitDateSummary; }
        }

        /// <summary>
        /// Gets the summary of hits by locale (may be null)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public StackHashProductLocaleSummaryCollection LocaleSummary
        {
            get { return _localeSummary; }
        }

        /// <summary>
        /// Gets the summary of hits by operating system (may be null)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Os"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public StackHashProductOperatingSystemSummaryCollection OsSummary
        {
            get { return _osSummary; }
        }

        /// <summary>
        /// Gets the effective event display hit threshold for this product
        /// </summary>
        public int EventDisplayHitThreshold
        {
            get { return _eventDisplayHitThreshold; }
        }

        /// <summary>
        /// Gets the effective event collection policy for this product
        /// </summary>
        public DisplayCollectionPolicy EventCollectionPolicy
        {
            get { return _eventCollectionPolicy; }
        }

        /// <summary>
        /// Gets the effective cab collection policy for this product
        /// </summary>
        public DisplayCollectionPolicy CabCollectionPolicy
        {
            get { return _cabCollectionPolicy; }
        }

        /// <summary>
        /// Gets the underlying StackHashProductInfo object
        /// </summary>
        public StackHashProductInfo StackHashProductInfo
        {
            get { return _productInfo; }
        }

        /// <summary>
        /// Gets the product name and version as a string
        /// </summary>
        public string NameAndVersion { get; private set; }

        // properties from StackHashProductInfo

        /// <summary />
        public bool SynchronizeEnabled { get { return _productInfo.SynchronizeEnabled; } }
        /// <summary />
        public DateTime LastSynchronizeTime { get { return _productInfo.LastSynchronizeTime; } }
        /// <summary />
        public DateTime LastSynchronizeStartedTime { get { return _productInfo.LastSynchronizeStartedTime; } }
        /// <summary />
        public DateTime LastSynchronizeCompletedTime { get { return _productInfo.LastSynchronizeCompletedTime; } }

        // properties from StackHashProduct

        /// <summary />
        public DateTime DateCreatedLocal { get { return _productInfo.Product.DateCreatedLocal; } }
        /// <summary />
        public DateTime DateModifiedLocal { get { return _productInfo.Product.DateModifiedLocal; } }
        /// <summary />
        public int Id { get { return _productInfo.Product.Id; } }
        /// <summary />
        public string Name { get { return _productInfo.Product.Name; } }
        /// <summary />
        public int TotalEvents { get { return _productInfo.Product.TotalEvents; } }
        /// <summary />
        public int TotalResponses { get { return _productInfo.Product.TotalResponses; } }
        /// <summary />
        public string Version { get { return _productInfo.Product.Version; } }
        /// <summary />
        public int TotalStoredEvents { get { return _productInfo.Product.TotalStoredEvents; } }

        /// <summary>
        /// WinQual product wrapper class
        /// </summary>
        /// <param name="productInfo">StackHashProductInfo</param>
        public DisplayProduct(StackHashProductInfo productInfo)
        {
            if (productInfo == null) { throw new ArgumentNullException("productInfo"); }
            if (productInfo.Product == null) { throw new ArgumentNullException("productInfo", "productInfo.Product is null"); }
            
            _productInfo = productInfo;
            UpdateNameAndVersion();
            UpdateDisplayFilter();
        }

        /// <summary>
        /// Updates the display product from a StackHashProductInfo object
        /// </summary>
        /// <param name="productInfo">StackHashProductInfo</param>
        public void UpdateProduct(StackHashProductInfo productInfo)
        {
            if (productInfo == null) { throw new ArgumentNullException("productInfo"); }
            if (productInfo.Product == null) { throw new ArgumentNullException("productInfo", "productInfo.Product is null"); }
            if (productInfo.Product.Id != _productInfo.Product.Id) { throw new InvalidOperationException("Cannot update from a product with a different ID"); }

            // save the new product
            StackHashProductInfo oldProductInfo = _productInfo;
            _productInfo = productInfo;

            // check for updates
            if (oldProductInfo.LastSynchronizeTime != _productInfo.LastSynchronizeTime) { RaisePropertyChanged("LastSynchronizeTime"); }
            if (oldProductInfo.SynchronizeEnabled != _productInfo.SynchronizeEnabled) { RaisePropertyChanged("SynchronizeEnabled"); }
            if (oldProductInfo.LastSynchronizeCompletedTime != _productInfo.LastSynchronizeCompletedTime) { RaisePropertyChanged("LastSynchronizeCompletedTime"); }
            if (oldProductInfo.LastSynchronizeStartedTime != _productInfo.LastSynchronizeStartedTime) { RaisePropertyChanged("LastSynchronizeStartedTime"); }
            if (oldProductInfo.Product.DateCreatedLocal != _productInfo.Product.DateCreatedLocal) { RaisePropertyChanged("DateCreatedLocal"); }
            if (oldProductInfo.Product.DateModifiedLocal != _productInfo.Product.DateModifiedLocal) { RaisePropertyChanged("DateModifiedLocal"); }
            if (oldProductInfo.Product.Name != _productInfo.Product.Name) { RaisePropertyChanged("Name"); }
            if (oldProductInfo.Product.TotalEvents != _productInfo.Product.TotalEvents) { RaisePropertyChanged("TotalEvents"); }
            if (oldProductInfo.Product.TotalResponses != _productInfo.Product.TotalResponses) { RaisePropertyChanged("TotalResponses"); }
            if (oldProductInfo.Product.Version != _productInfo.Product.Version) { RaisePropertyChanged("Version"); }
            if (oldProductInfo.Product.TotalStoredEvents != _productInfo.Product.TotalStoredEvents) { RaisePropertyChanged("TotalStoredEvents"); }

            UpdateNameAndVersion();
            UpdateDisplayFilter();
        }

        /// <summary>
        /// Updates the effective cab collection policy for this product from a list of all policies
        /// </summary>
        /// <param name="policies">List of all policies</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void UpdateCabCollectionPolicy(List<StackHashCollectionPolicy> policies)
        {
            if (policies == null) { throw new ArgumentNullException("policies"); }

            DisplayCollectionPolicy currentCabCollectionPolicy = DisplayCollectionPolicy.FindPolicy(policies, 
                this.Id, 
                -1, 
                -1, 
                -1, 
                StackHashCollectionObject.Cab, 
                StackHashCollectionObject.Cab);

            if (currentCabCollectionPolicy != _cabCollectionPolicy)
            {
                if (_cabCollectionPolicy == null)
                {
                    _cabCollectionPolicy = currentCabCollectionPolicy;
                }
                else
                {
                    _cabCollectionPolicy.UpdateCollectionPolicy(currentCabCollectionPolicy.StackHashCollectionPolicy);
                }

                RaisePropertyChanged("CabCollectionPolicy");
            }

            UpdateDisplayFilter();
        }

        /// <summary>
        /// Updates the effective event collection policy for this product from a list of all policies
        /// </summary>
        /// <param name="policies">List of all policies</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void UpdateEventCollectionPolicy(List<StackHashCollectionPolicy> policies)
        {
            if (policies == null) { throw new ArgumentNullException("policies"); }

            DisplayCollectionPolicy currentEventCollectionPolicy = DisplayCollectionPolicy.FindPolicy(policies,
                this.Id,
                -1,
                -1,
                -1,
                StackHashCollectionObject.Cab,
                StackHashCollectionObject.Event);

            if (currentEventCollectionPolicy != _eventCollectionPolicy)
            {
                if (_eventCollectionPolicy == null)
                {
                    _eventCollectionPolicy = currentEventCollectionPolicy;
                }
                else
                {
                    _eventCollectionPolicy.UpdateCollectionPolicy(currentEventCollectionPolicy.StackHashCollectionPolicy);
                }

                RaisePropertyChanged("EventCollectionPolicy");
            }

            UpdateDisplayFilter();
        }

        /// <summary>
        /// Updates hit summary information for this product
        /// </summary>
        /// <param name="hitDateSummary">Summary of hits by hit date</param>
        /// <param name="localeSummary">Summary of hits by locale</param>
        /// <param name="osSummary">Summary of hits by operating system</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void UpdateHitSummaries(StackHashProductHitDateSummaryCollection hitDateSummary,
            StackHashProductLocaleSummaryCollection localeSummary,
            StackHashProductOperatingSystemSummaryCollection osSummary)
        {
            if (hitDateSummary != _hitDateSummary)
            {
                _hitDateSummary = hitDateSummary;
                RaisePropertyChanged("HitDateSummary");
            }

            if (localeSummary != _localeSummary)
            {
                _localeSummary = localeSummary;
                RaisePropertyChanged("LocaleSummary");
            }

            if (osSummary != _osSummary)
            {
                _osSummary = osSummary;
                RaisePropertyChanged("OsSummary");
            }
        }

        private void UpdateNameAndVersion()
        {
            this.NameAndVersion = string.Format(CultureInfo.CurrentCulture,
                "{0} {1}",
                this.Name,
                this.Version);

            RaisePropertyChanged("NameAndVersion");
        }

        /// <summary>
        /// Update the display filter for the product
        /// </summary>
        public void UpdateDisplayFilter()
        {
            int newThreshold = UserSettings.Settings.GetDisplayHitThreshold(this.Id);
            if (newThreshold != _eventDisplayHitThreshold)
            {
                _eventDisplayHitThreshold = newThreshold;
                RaisePropertyChanged("EventDisplayHitThreshold");
            }
        }

        #region INotifyPropertyChanged Members

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary />
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
