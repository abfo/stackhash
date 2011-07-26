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
    /// StackHashCollectionPolicy wrapper object
    /// </summary>
    public class DisplayCollectionPolicy : INotifyPropertyChanged, IEquatable<DisplayCollectionPolicy>, IComparable<DisplayCollectionPolicy>, IComparable
    {
        private StackHashCollectionPolicy _collectionPolicy;

        /// <summary>
        /// Gets the underlying StackHashCollectionPolicy object
        /// </summary>
        public StackHashCollectionPolicy StackHashCollectionPolicy
        {
            get { return _collectionPolicy; }
        }

        // Properties from StackHashCollectionPolicy

        /// <summary />
        public StackHashCollectionOrder CollectionOrder { get { return _collectionPolicy.CollectionOrder; } }
        /// <summary />
        public StackHashCollectionType CollectionType { get { return _collectionPolicy.CollectionType; } }
        /// <summary />
        public bool CollectLarger { get { return _collectionPolicy.CollectLarger; } }
        /// <summary />
        public StackHashCollectionObject ConditionObject { get { return _collectionPolicy.ConditionObject; } }
        /// <summary />
        public int Maximum { get { return _collectionPolicy.Maximum; } }
        /// <summary />
        public int Minimum { get { return _collectionPolicy.Minimum; } }
        /// <summary />
        public StackHashCollectionObject ObjectToCollect { get { return _collectionPolicy.ObjectToCollect; } }
        /// <summary />
        public int Percentage { get { return _collectionPolicy.Percentage; } }
        /// <summary />
        public int RootId { get { return _collectionPolicy.RootId; } }
        /// <summary />
        public StackHashCollectionObject RootObject { get { return _collectionPolicy.RootObject; } }

        /// <summary>
        /// Updates the display collection policy from a StackHashCollectionPolicy object
        /// </summary>
        /// <param name="collectionPolicy"></param>
        public void UpdateCollectionPolicy(StackHashCollectionPolicy collectionPolicy)
        {
            if (collectionPolicy == null) { throw new ArgumentNullException("collectionPolicy"); }

            // save the new collection policy
            StackHashCollectionPolicy oldCollectionPolicy = _collectionPolicy;
            _collectionPolicy = collectionPolicy;

            if (oldCollectionPolicy.CollectionOrder != _collectionPolicy.CollectionOrder) { RaisePropertyChanged("CollectionOrder"); }
            if (oldCollectionPolicy.CollectionType != _collectionPolicy.CollectionType) { RaisePropertyChanged("CollectionType"); }
            if (oldCollectionPolicy.CollectLarger != _collectionPolicy.CollectLarger) { RaisePropertyChanged("CollectLarger"); }
            if (oldCollectionPolicy.ConditionObject != _collectionPolicy.ConditionObject) { RaisePropertyChanged("ConditionObject"); }
            if (oldCollectionPolicy.Maximum != _collectionPolicy.Maximum) { RaisePropertyChanged("Maximum"); }
            if (oldCollectionPolicy.Minimum != _collectionPolicy.Minimum) { RaisePropertyChanged("Minimum"); }
            if (oldCollectionPolicy.ObjectToCollect != _collectionPolicy.ObjectToCollect) { RaisePropertyChanged("ObjectToCollect"); }
            if (oldCollectionPolicy.Percentage != _collectionPolicy.Percentage) { RaisePropertyChanged("Percentage"); }
            if (oldCollectionPolicy.RootId != _collectionPolicy.RootId) { RaisePropertyChanged("RootId"); }
            if (oldCollectionPolicy.RootObject != _collectionPolicy.RootObject) { RaisePropertyChanged("RootObject"); }
        }

        /// <summary>
        /// Constructs a DisplayCollectionPolicy by searching a list of all policies for a match
        /// </summary>
        /// <remarks>
        /// Duplicates logic from FindPrioritizedPolicy() in StackHashDataCollectionPolicy.cs (BusinessLogic) 
        /// </remarks>
        /// <param name="policies">Policy list to search</param>
        /// <param name="productId">ID of product to match (or -1)</param>
        /// <param name="fileId">ID of file to match (or -1)</param>
        /// <param name="eventId">ID of event to match (or -1)</param>
        /// <param name="cabId">ID of cab to match (or -1)</param>
        /// <param name="objectToCollect">The object being collected, cannot be Any</param>
        /// <param name="conditionObject">The object being used to determine if collection should take place, cannot be Any</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static DisplayCollectionPolicy FindPolicy(List<StackHashCollectionPolicy> policies,
            int productId, 
            int fileId, 
            int eventId, 
            int cabId, 
            StackHashCollectionObject objectToCollect, 
            StackHashCollectionObject conditionObject)
        {
            if (policies == null) { throw new ArgumentNullException("policies"); }
            if (objectToCollect == StackHashCollectionObject.Any) { throw new ArgumentException("Must specify an object to collect", "objectToCollect"); }
            if (conditionObject == StackHashCollectionObject.Any) { throw new ArgumentException("Must specify a condition object", "conditionObject"); }
            
            DisplayCollectionPolicy displayPolicy = null;

            StackHashCollectionPolicy globalPolicy = null;
            StackHashCollectionPolicy productPolicy = null;
            StackHashCollectionPolicy eventPolicy = null;
            StackHashCollectionPolicy filePolicy = null;
            StackHashCollectionPolicy cabPolicy = null;

            foreach (StackHashCollectionPolicy policy in policies)
            {
                if ((objectToCollect != StackHashCollectionObject.Any) && (objectToCollect != policy.ObjectToCollect))
                    continue;

                if ((conditionObject != StackHashCollectionObject.Any) && (conditionObject != policy.ConditionObject))
                    continue;

                switch (policy.RootObject)
                {
                    case StackHashCollectionObject.Global:
                        globalPolicy = policy;
                        break;
                    case StackHashCollectionObject.Product:
                        if (productId == policy.RootId)
                            productPolicy = policy;
                        break;
                    case StackHashCollectionObject.File:
                        if (fileId == policy.RootId)
                            filePolicy = policy;
                        break;
                    case StackHashCollectionObject.Event:
                        if (eventId == policy.RootId)
                            eventPolicy = policy;
                        break;
                    case StackHashCollectionObject.Cab:
                        if (cabId == policy.RootId)
                            cabPolicy = policy;
                        break;
                    default:
                        throw new InvalidOperationException("Data collection option not known");
                }
            }

            // Prioritize from cab up.
            if (cabPolicy != null) { displayPolicy = new DisplayCollectionPolicy(cabPolicy); }
            else if (eventPolicy != null) { displayPolicy = new DisplayCollectionPolicy(eventPolicy); }
            else if (filePolicy != null) { displayPolicy = new DisplayCollectionPolicy(filePolicy); }
            else if (productPolicy != null) { displayPolicy = new DisplayCollectionPolicy(productPolicy); }
            else if (globalPolicy != null) { displayPolicy = new DisplayCollectionPolicy(globalPolicy); }
            else { throw new InvalidOperationException("Failed to find matching data collection policy"); }

            return displayPolicy;
        }

        private DisplayCollectionPolicy(StackHashCollectionPolicy collectionPolicy)
        {
            if (collectionPolicy == null) { throw new ArgumentNullException("collectionPolicy"); }
            _collectionPolicy = collectionPolicy;
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

        #region IEquatable<DisplayCollectionPolicy> Members

        /// <summary />
        public bool Equals(DisplayCollectionPolicy other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return ((this.CollectionOrder == other.CollectionOrder) &&
                    (this.CollectionType == other.CollectionType) &&
                    (this.CollectLarger == other.CollectLarger) &&
                    (this.ConditionObject == other.ConditionObject) &&
                    (this.Maximum == other.Maximum) &&
                    (this.Minimum == other.Minimum) &&
                    (this.ObjectToCollect == other.ObjectToCollect) &&
                    (this.Percentage == other.Percentage) &&
                    (this.RootId == other.RootId) &&
                    (this.RootObject == other.RootObject));
            }
        }

        /// <summary />
        public override bool Equals(object obj)
        {
            if (obj == null) { return base.Equals(obj); }

            DisplayCollectionPolicy other = obj as DisplayCollectionPolicy;
            if (other == null)
            {
                return false;
            }
            else
            {
                return Equals(other);
            }
        }

        /// <summary />
        public override int GetHashCode()
        {
            return _collectionPolicy.GetHashCode();
        }

        /// <summary />
        public static bool operator ==(DisplayCollectionPolicy displayCollectionLeft, DisplayCollectionPolicy displayCollectionRight)
        {
            if (System.Object.ReferenceEquals(displayCollectionLeft, displayCollectionRight))
            {
                return true;
            }

            if (((object)displayCollectionLeft == null) || ((object)displayCollectionRight == null))
            {
                return false;
            }

            return displayCollectionLeft.Equals(displayCollectionRight);
        }

        /// <summary />
        public static bool operator !=(DisplayCollectionPolicy displayCollectionLeft, DisplayCollectionPolicy displayCollectionRight)
        {
            return (!(displayCollectionLeft == displayCollectionRight));
        }

        #endregion

        #region IComparable<DisplayCollectionPolicy> Members

        /// <summary />
        public int CompareTo(DisplayCollectionPolicy other)
        {
            int result = -1;

            if (other != null)
            {
                result = this.CollectionType.CompareTo(other.CollectionType);

                if (result == 0)
                {
                    switch (this.CollectionType)
                    {
                        case StackHashCollectionType.Count:
                            result = this.Maximum.CompareTo(other.Maximum);
                            break;

                        case StackHashCollectionType.MinimumHitCount:
                            result = this.Minimum.CompareTo(other.Minimum);
                            break;

                        case StackHashCollectionType.Percentage:
                            result = this.Percentage.CompareTo(other.Percentage);
                            break;
                    }
                }

                if (result == 0)
                {
                    result = this.CollectionOrder.CompareTo(other.CollectionOrder);
                }

                if (result == 0)
                {
                    result = this.ObjectToCollect.CompareTo(other.ObjectToCollect);
                }

                if (result == 0)
                {
                    result = this.ConditionObject.CompareTo(other.ConditionObject);
                }

                if (result == 0)
                {
                    result = this.RootObject.CompareTo(other.RootObject);
                }

                if (result == 0)
                {
                    result = this.RootId.CompareTo(other.RootId);
                }
            }

            return result;
        }

        /// <summary />
        public static bool operator <(DisplayCollectionPolicy displayCollectionLeft, DisplayCollectionPolicy displayCollectionRight)
        {
            if (displayCollectionLeft == null) { return false; }
            return (displayCollectionLeft.CompareTo(displayCollectionRight) < 0);
        }

        /// <summary />
        public static bool operator >(DisplayCollectionPolicy displayCollectionLeft, DisplayCollectionPolicy displayCollectionRight)
        {
            if (displayCollectionLeft == null) { return true; }
            return (displayCollectionLeft.CompareTo(displayCollectionRight) > 0);
        }  

        #endregion

        #region IComparable Members

        /// <summary />
        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as DisplayCollectionPolicy);
        }

        #endregion
    }
}
