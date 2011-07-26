using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Reflection;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashCollectionObject
    {
        [EnumMember]
        Global,
        [EnumMember]
        Product,
        [EnumMember]
        File,
        [EnumMember]
        Event,
        [EnumMember]
        Cab,
        [EnumMember]
        Any
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashCollectionType
    {
        [EnumMember]
        All,
        [EnumMember]
        None,
        [EnumMember]
        Percentage,
        [EnumMember]
        Count,
        [EnumMember]
        MinimumHitCount,
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashCollectionOrder
    {
        [EnumMember]
        MostRecentFirst,
        [EnumMember]
        OldestFirst,
        [EnumMember]
        LargestFirst,
        [EnumMember]
        SmallestFirst,
        [EnumMember]
        AsReceived
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashCollectionPolicy
    {
        // From this object.
        private StackHashCollectionObject m_RootObject;
        private int m_RootId;

        // Collect this type of object.
        private StackHashCollectionObject m_ObjectToCollect;


        // Conforming to the condition below.
        private StackHashCollectionObject m_ConditionObject;

        private StackHashCollectionType m_CollectionType;
        private StackHashCollectionOrder m_CollectionOrder;
        private int m_Maximum;
        private int m_Minimum;
        private int m_Percentage;
        private bool m_CollectLarger;



        /// <summary>
        /// Defines whether the collection policy refers to global, product, file, event or cab.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject RootObject
        {
            get { return m_RootObject; }
            set { m_RootObject = value; }
        }

        /// <summary>
        /// The object to make a decision on. e.g. Top 10 events. Or top 10% of cabs.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ConditionObject
        {
            get { return m_ConditionObject; }
            set { m_ConditionObject = value; }
        }

        /// <summary>
        /// The type of object to actually collect - cab is the only one currently.
        /// May want to control collection of events or products later.
        /// </summary>
        [DataMember]
        public StackHashCollectionObject ObjectToCollect
        {
            get { return m_ObjectToCollect; }
            set { m_ObjectToCollect = value; }
        }

        
        /// <summary>
        /// Determines the type of collection to take place. 
        /// If this is percentage - then Percentage defines the % of cabs to download.
        /// If this is count - then MaximumCabs defines the max number of cabs to download.
        /// </summary>
        [DataMember]
        public StackHashCollectionType CollectionType
        {
            get { return m_CollectionType; }
            set { m_CollectionType = value; }
        }


        /// <summary>
        /// Determines collection order for those available.
        /// </summary>
        [DataMember]
        public StackHashCollectionOrder CollectionOrder
        {
            get { return m_CollectionOrder; }
            set { m_CollectionOrder = value; }
        }

        
        /// <summary>
        /// Id of the product, file, event or cab. Can be 0 for global.
        /// </summary>
        [DataMember]
        public int RootId
        {
            get { return m_RootId; }
            set { m_RootId = value; }
        }

        /// <summary>
        /// Only valid for Count collection. Defines the max cabs to download.
        /// </summary>
        [DataMember]
        public int Maximum
        {
            get { return m_Maximum; }
            set { m_Maximum = value; }
        }

        /// <summary>
        /// Only valid for Count collection. Defines the min to download.
        /// </summary>
        [DataMember]
        public int Minimum
        {
            get { return m_Minimum; }
            set { m_Minimum = value; }
        }

        
        /// <summary>
        /// Only valid for percentage collection. Defines the percentage of cabs downloaded.
        /// </summary>
        [DataMember]
        public int Percentage
        {
            get { return m_Percentage; }
            set { m_Percentage = value; }
        }


        /// <summary>
        /// Overrides the other collection fields if a new cab is available for download that
        /// is bigger than those already downloaded.
        /// </summary>
        [DataMember]
        public bool CollectLarger
        {
            get { return m_CollectLarger; }
            set { m_CollectLarger = value; }
        }
    }


    /// <summary>
    /// A collection of StackHashDataCollectionPolicy identifying data collection policy at one or more levels.
    /// If a policy exists for a specific cab ID - then it will be used.
    /// Otherwise if a policy exists for the owning event - then it will be used.
    /// Otherwise if a policy exists for the owning file (NOT USED) - then it will be used.
    /// Otherwise if a policy exists for the owning product - then it will be used.
    /// Otherwise if a global policy exists then it will be used.
    /// Otherwise - all cabs will be collected.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashCollectionPolicyCollection : Collection<StackHashCollectionPolicy>
    {
        public StackHashCollectionPolicyCollection() { ; } // Required for serialization.


        /// <summary>
        /// Locates all the top priority policies for a partcular collection object.
        /// Note this may be more than 1. e.g. there may be a cab condition and an event condition for collecting cabs.
        /// e.g. collect 2 cabs for the top 20 events.
        /// </summary>
        /// <param name="productId">Owning product.</param>
        /// <param name="fileId">Owning file.</param>
        /// <param name="eventId">Owning event.</param>
        /// <param name="cabId">The cab to make a decision on.</param>
        /// <param name="objectToCollect">Type of the object to collect.</param>
        /// <returns>One prioritized matching policy.</returns>
        public StackHashCollectionPolicyCollection FindAllPoliciesForObjectBeingCollected(int productId, int fileId, int eventId, int cabId, StackHashCollectionObject objectToCollect)
        {
            StackHashCollectionPolicyCollection allPolicies = new StackHashCollectionPolicyCollection();

            if (objectToCollect == StackHashCollectionObject.Any)
                throw new ArgumentException("Must specify an object to collect", "objectToCollect");

            Monitor.Enter(this);

            try
            {
                foreach (StackHashCollectionObject conditionObject in Enum.GetValues(typeof(StackHashCollectionObject))) 
                {
                    // Only check specific ones.
                    if (conditionObject == StackHashCollectionObject.Any)
                        continue;

                    StackHashCollectionPolicy policy = FindPrioritizedPolicy(productId, fileId, eventId, cabId, objectToCollect, conditionObject);

                    if (policy != null)
                        allPolicies.Add(policy);
                }

                return allPolicies;
            }
            finally
            {
                Monitor.Exit(this);
            }
         }


        /// <summary>
        /// Locates a policy for the specified cab knowing its product, file and event.
        /// The policy used will be the cab policy if present, otherwise the next highest level 
        /// object policy in that order.
        /// </summary>
        /// <param name="productId">Owning product.</param>
        /// <param name="fileId">Owning file.</param>
        /// <param name="eventId">Owning event.</param>
        /// <param name="cabId">The cab to make a decision on.</param>
        /// <param name="objectToCollect">Type of the object to collect.</param>
        /// <param name="conditionObject">Object being compared.</param>
        /// <returns>One prioritized matching policy.</returns>
        public StackHashCollectionPolicy FindPrioritizedPolicy(int productId, int fileId, int eventId, int cabId, StackHashCollectionObject objectToCollect, StackHashCollectionObject conditionObject)
        {
            if (objectToCollect == StackHashCollectionObject.Any)
                throw new ArgumentException("Must specify an object to collect", "objectToCollect");

            if (conditionObject == StackHashCollectionObject.Any)
                throw new ArgumentException("Must specify a condition object", "conditionObject");

            Monitor.Enter(this);

            try
            {
                StackHashCollectionPolicy globalPolicy = null;
                StackHashCollectionPolicy productPolicy = null;
                StackHashCollectionPolicy eventPolicy = null;
                StackHashCollectionPolicy filePolicy = null;
                StackHashCollectionPolicy cabPolicy = null;

                foreach (StackHashCollectionPolicy policy in this)
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
                if (cabPolicy != null)
                    return cabPolicy;
                else if (eventPolicy != null)
                    return eventPolicy;
                else if (filePolicy != null)
                    return filePolicy;
                else if (productPolicy != null)
                    return productPolicy;
                else if (globalPolicy != null)
                    return globalPolicy;
                else
                    return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Locates a policy by object and id. Object is Global, Product, File, Event or Cab.
        /// </summary>
        /// <param name="collectionObject">Object type to look for.</param>
        /// <param name="id">Id of the object.</param>
        /// <param name="conditionObject">Condition object.</param>
        /// <param name="objectToCollect">Object to collect.</param>
        /// <returns>Found policy or null.</returns>
        public StackHashCollectionPolicy FindPolicy(StackHashCollectionObject rootObject, int id, StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashCollectionPolicy policy in this)
                {
                    if ((policy.RootObject == rootObject) && (policy.ConditionObject == conditionObject) &&
                        (policy.ObjectToCollect == objectToCollect) && (policy.RootId == id))
                    {
                        return policy;
                    }
                }
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Adds a policy to the list of policies.
        /// If a policy with the matching object/id already exists then it is replaced.
        /// </summary>
        /// <param name="newPolicy">The new policy to add.</param>
        public void AddPolicy(StackHashCollectionPolicy newPolicy)
        {
            if (newPolicy == null)
                return;
//                throw new ArgumentNullException("newPolicy");
            
            Monitor.Enter(this);

            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if ((this[i].RootObject == newPolicy.RootObject) && (this[i].RootId == newPolicy.RootId) &&
                        (this[i].ConditionObject == newPolicy.ConditionObject) && (this[i].ObjectToCollect == newPolicy.ObjectToCollect))
                    {
                        this[i] = newPolicy;
                        return;
                    }
                }

                // Add a new one.
                this.Add(newPolicy);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Removes the specified policy.
        /// Only the object and id are matched.
        /// Does nothing if the policy does not exist.
        /// </summary>
        /// <param name="policy">Policy to remove.</param>
        public void RemovePolicy(StackHashCollectionPolicy policy)
        {
            if (policy == null)
                throw new ArgumentNullException("policy");

            Monitor.Enter(this);

            try
            {
                StackHashCollectionPolicy foundPolicy = this.FindPolicy(policy.RootObject, policy.RootId, policy.ConditionObject, policy.ObjectToCollect);
                if (foundPolicy != null)
                    this.Remove(foundPolicy);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void RemoveAllPolicies()
        {
            Monitor.Enter(this);

            try
            {
                this.Clear();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public static StackHashCollectionPolicyCollection Default
        {
            get
            {

                StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();
                policyCollection.Add(new StackHashCollectionPolicy());
                policyCollection[0].RootObject = StackHashCollectionObject.Global;
                policyCollection[0].ConditionObject = StackHashCollectionObject.Cab;
                policyCollection[0].ObjectToCollect = StackHashCollectionObject.Cab;
                policyCollection[0].CollectionType = StackHashCollectionType.Count;
                policyCollection[0].Maximum = 2;
                policyCollection[0].Percentage = 0;
                policyCollection[0].CollectionOrder = StackHashCollectionOrder.AsReceived;
                policyCollection[0].CollectLarger = true;

                policyCollection.Add(new StackHashCollectionPolicy());
                policyCollection[1].RootObject = StackHashCollectionObject.Global;
                policyCollection[1].ConditionObject = StackHashCollectionObject.Event;
                policyCollection[1].ObjectToCollect = StackHashCollectionObject.Cab;
                policyCollection[1].CollectionType = StackHashCollectionType.All;
                policyCollection[1].Maximum = 0;
                policyCollection[1].Percentage = 100;
                policyCollection[1].CollectionOrder = StackHashCollectionOrder.AsReceived;
                policyCollection[1].CollectLarger = true;
                return policyCollection;
            }
        }
    }

}
