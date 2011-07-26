using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using StackHashUtilities;


namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashPurgeObject
    {
        [EnumMember]
        PurgeGlobal,
        [EnumMember]
        PurgeProduct,
        [EnumMember]
        PurgeFile,
        [EnumMember]
        PurgeEvent,
        [EnumMember]
        PurgeCab
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashPurgeOptions
    {
        private StackHashPurgeObject m_PurgeObject;
        private int m_Id;
        private int m_AgeToPurge;

        private bool m_PurgeCabFiles;
        private bool m_PurgeDumpFiles;

        [DataMember]
        public StackHashPurgeObject PurgeObject
        {
            get { return m_PurgeObject; }
            set { m_PurgeObject = value; }
        }

        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public int AgeToPurge
        {
            get { return m_AgeToPurge; }
            set { m_AgeToPurge = value; }
        }

        [DataMember]
        public bool PurgeCabFiles
        {
            get { return m_PurgeCabFiles; }
            set { m_PurgeCabFiles = value; }
        }

        [DataMember]
        public bool PurgeDumpFiles
        {
            get { return m_PurgeDumpFiles; }
            set { m_PurgeDumpFiles = value; }
        }
    }

    /// <summary>
    /// A collection of StackHashPurgeItems identifying which products should be purged and how.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashPurgeOptionsCollection : Collection<StackHashPurgeOptions>
    {
        public StackHashPurgeOptionsCollection() { ; } // Required for serialization.

        /// <summary>
        /// Adds new purge options to the list.
        /// If a match exists, then it is replaced.
        /// </summary>
        /// <param name="newPolicy">The new policy to add.</param>
        public void AddPurgeOptions(StackHashPurgeOptions newPurgeOptions)
        {
            if (newPurgeOptions == null)
                throw new ArgumentNullException("newPurgeOptions");

            Monitor.Enter(this);

            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if ((this[i].PurgeObject == newPurgeOptions.PurgeObject) && (this[i].Id == newPurgeOptions.Id))
                    {
                        this[i] = newPurgeOptions;
                        return;
                    }
                }

                // Add a new one.
                this.Add(newPurgeOptions);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Locates the correct purge policy for the specified cab knowing its product, file and event.
        /// The policy used will be the cab policy if present, otherwise the next highest level 
        /// object policy in that order.
        /// </summary>
        /// <param name="productId">Owning product.</param>
        /// <param name="fileId">Owning file.</param>
        /// <param name="eventId">Owning event.</param>
        /// <param name="cabId">Cab.</param>
        /// <returns>Active options</returns>
        public StackHashPurgeOptions FindActivePurgeOptions(int productId, int fileId, int eventId, int cabId)
        {
            Monitor.Enter(this);

            try
            {
                StackHashPurgeOptions globalOptions = null;
                StackHashPurgeOptions productOptions = null;
                StackHashPurgeOptions fileOptions = null;
                StackHashPurgeOptions eventOptions = null;
                StackHashPurgeOptions cabOptions = null;

                foreach (StackHashPurgeOptions options in this)
                {
                    switch (options.PurgeObject)
                    {
                        case StackHashPurgeObject.PurgeGlobal:
                            globalOptions = options;
                            break;
                        case StackHashPurgeObject.PurgeProduct:
                            if (productId == options.Id)
                                productOptions = options;
                            break;
                        case StackHashPurgeObject.PurgeFile:
                            if (fileId == options.Id)
                                fileOptions = options;
                            break;
                        case StackHashPurgeObject.PurgeEvent:
                            if (eventId == options.Id)
                                eventOptions = options;
                            break;
                        case StackHashPurgeObject.PurgeCab:
                            if (cabId == options.Id)
                                cabOptions = options;
                            break;
                        default:
                            throw new InvalidOperationException("Purge option not known");
                    }
                }

                if (cabOptions != null)
                    return cabOptions;
                else if (eventOptions != null)
                    return eventOptions;
                else if (fileOptions != null)
                    return fileOptions;
                else if (productOptions != null)
                    return productOptions;
                else if (globalOptions != null)
                    return globalOptions;
                else
                    return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Locates the most recent purge date.
        /// </summary>
        /// <returns>Latest age.</returns>
        public int FindMostRecentPurgeAge()
        {
            Monitor.Enter(this);

            try
            {
                int purgeAge = 0xffff;

                foreach (StackHashPurgeOptions options in this)
                {
                    if (options.AgeToPurge < purgeAge)
                        purgeAge = options.AgeToPurge;
                }

                return purgeAge;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        
        /// <summary>
        /// Locates purge object matching the specified object and id.
        /// </summary>
        /// <param name="purgeObject">Type of the object.</param>
        /// <param name="id">Id of the object.</param>
        public StackHashPurgeOptions FindPurgeOptions(StackHashPurgeObject purgeObject, int id)
        {
            Monitor.Enter(this);

            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if ((this[i].PurgeObject == purgeObject) && (this[i].Id == id))
                    {
                        return this[i];
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
        /// Removes the specified purge options.
        /// Only the object and id are matched.
        /// Does nothing if the purge option does not exist.
        /// </summary>
        /// <param name="purgeObject">Object to remove.</param>
        /// <param name="id">Id of object.</param>
        public bool RemovePurgeOption(StackHashPurgeObject purgeObject, int id)
        {
            Monitor.Enter(this);

            try
            {
                StackHashPurgeOptions foundPurgeOptions = this.FindPurgeOptions(purgeObject, id);
                if (foundPurgeOptions != null)
                {
                    this.Remove(foundPurgeOptions);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void RemoveAllPurgeOptions()
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

    }
}
