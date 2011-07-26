using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;


namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProduct : ICloneable, IComparable
    {
        // Version History.
        // ================
        // Version 2 introduced m_TotalStoredEvents.
        private const int s_ThisStructureVersion = 2;

        private int m_StructureVersion;

        private DateTime m_DateCreatedLocal;
        private DateTime m_DateModifiedLocal;
        private string m_FilesLink;
        private int m_Id;
        private string m_Name;
        private int m_TotalEvents;
        private int m_TotalResponses;
        private string m_Version;
        private int m_TotalStoredEvents;

        public StackHashProduct() {;}  // Needed to serialize;

        public StackHashProduct(DateTime dateCreatedLocal,
                               DateTime dateModifiedLocal,
                               string filesLink,
                               int id,
                               string name,
                               int totalEvents,
                               int totalResponses,
                               string version)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_FilesLink = filesLink;
            m_Id = id;
            m_Name = name;
            m_TotalEvents = totalEvents;
            m_TotalResponses = totalResponses;
            m_Version = version;
            m_TotalStoredEvents = 0;
        }

        public StackHashProduct(DateTime dateCreatedLocal,
                               DateTime dateModifiedLocal,
                               string filesLink,
                               int id,
                               string name,
                               int totalEvents,
                               int totalResponses,
                               string version,
                               int totalStoredEvents)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_FilesLink = filesLink;
            m_Id = id;
            m_Name = name;
            m_TotalEvents = totalEvents;
            m_TotalResponses = totalResponses;
            m_Version = version;
            m_TotalStoredEvents = totalStoredEvents;
        }

        [DataMember]
        public DateTime DateCreatedLocal
        {
            get { return m_DateCreatedLocal; }
            set { m_DateCreatedLocal = value; }
        }

        [DataMember]
        public DateTime DateModifiedLocal
        {
            get { return m_DateModifiedLocal; }
            set { m_DateModifiedLocal = value; }
        }

        [DataMember]
        public string FilesLink
        {
            get { return m_FilesLink; }
            set { m_FilesLink = value; }
        }

        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public int TotalEvents
        {
            get { return m_TotalEvents; }
            set { m_TotalEvents = value; }
        }

        [DataMember]
        public int TotalResponses
        {
            get { return m_TotalResponses; }
            set { m_TotalResponses = value; }
        }

        [DataMember]
        public string Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        [DataMember]
        public int StructureVersion
        {
            get { return m_StructureVersion; }
            set { m_StructureVersion = value; }
        }

        public static int ThisStructureVersion
        {
            get { return s_ThisStructureVersion; }
        }

        [DataMember]
        public int TotalStoredEvents
        {
            get { return m_TotalStoredEvents; }
            set { m_TotalStoredEvents = value; }
        }


        /// <summary>
        /// Determines if a change in the product data should be reported to bug tracking plugins.
        /// </summary>
        /// <param name="newProduct">The new product value.</param>
        /// <param name="checkNonWinQualFields">True - the non-winqual fields are compared too.</param>
        /// <returns>True - significant changes so report it, False - nothing to report.</returns>
        public bool ShouldReportToBugTrackPlugIn(StackHashProduct newProduct, bool checkNonWinQualFields)
        {
            if (newProduct == null)
                throw new ArgumentNullException("newProduct");

            if (this.Id != newProduct.Id)
                return true;
            if (this.Name != newProduct.Name)
                return true;
            if (this.Version != newProduct.Version)
                return true;
            if (this.TotalResponses != newProduct.TotalResponses)
                return true;
            if (this.TotalEvents != newProduct.TotalEvents)
                return true;
            if (checkNonWinQualFields)
            {
                if (this.TotalStoredEvents != newProduct.TotalStoredEvents)
                    return true;
            }

            return false;
        }

        
        #region ICloneable Members

        /// <summary>
        /// Clone a copy of the product information.
        /// </summary>
        /// <returns>Cloned copy of the product.</returns>
        public object Clone()
        {
            StackHashProduct product = new StackHashProduct(m_DateCreatedLocal, m_DateModifiedLocal, 
                m_FilesLink, m_Id, m_Name, m_TotalEvents, m_TotalResponses, m_Version, m_TotalStoredEvents);

            return product;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashProduct product = obj as StackHashProduct;

            if ((m_DateCreatedLocal == product.DateCreatedLocal) &&
                (m_DateModifiedLocal == product.DateModifiedLocal) &&
                (m_FilesLink == product.FilesLink) &&
                (m_Id == product.Id) &&
                (m_Name == product.Name) &&
                (m_TotalEvents == product.TotalEvents) &&
                (m_TotalResponses == product.TotalResponses) &&
                (m_Version == product.Version))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        #endregion


        public override string ToString()
        {
            StringBuilder outString = new StringBuilder("Product: DateCreated:");
            outString.Append(m_DateCreatedLocal);
            outString.Append(" ");
            outString.Append(m_DateCreatedLocal.Kind);
            outString.Append(" DateModified:");
            outString.Append(m_DateModifiedLocal);
            outString.Append(" ");
            outString.Append(m_DateModifiedLocal.Kind);
            outString.Append(" ID:");
            outString.Append(m_Id);
            outString.Append(" Name:");
            outString.Append(m_Name);
            outString.Append(" TotalEvents:");
            outString.Append(m_TotalEvents);
            outString.Append(" TotalResponses:");
            outString.Append(m_TotalResponses);
            outString.Append(" TotalStoredEvents:");
            outString.Append(m_TotalStoredEvents);

            return outString.ToString();
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProductInfo
    {
        private StackHashProduct m_Product;
        private bool m_SynchronizeEnabled;
        private DateTime m_LastSynchronizeTime;
        private DateTime m_LastSynchronizeCompletedTime;
        private DateTime m_LastSynchronizeStartedTime;
        private StackHashProductSyncData m_ProductSyncData;


        public StackHashProductInfo() { ;}  // Needed to serialize;

        public StackHashProductInfo(StackHashProduct product, bool synchronizeEnabled,
            DateTime lastSynchronizeTime, StackHashProductSyncData productSyncData, DateTime lastSynchronizeCompletedTime, DateTime lastSynchronizeStartedTime)
        {
            m_Product = product;
            m_SynchronizeEnabled = synchronizeEnabled;
            m_LastSynchronizeTime = lastSynchronizeTime;
            m_ProductSyncData = productSyncData;
            m_LastSynchronizeCompletedTime = lastSynchronizeCompletedTime;
            m_LastSynchronizeStartedTime = lastSynchronizeStartedTime;
        }

        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        [DataMember]
        public bool SynchronizeEnabled
        {
            get { return m_SynchronizeEnabled; }
            set { m_SynchronizeEnabled = value; }
        }

        [DataMember]
        public DateTime LastSynchronizeTime
        {
            get { return m_LastSynchronizeTime; }
            set { m_LastSynchronizeTime = value; }
        }

        [DataMember]
        public DateTime LastSynchronizeStartedTime
        {
            get { return m_LastSynchronizeStartedTime; }
            set { m_LastSynchronizeStartedTime = value; }
        }

        [DataMember]
        public DateTime LastSynchronizeCompletedTime
        {
            get { return m_LastSynchronizeCompletedTime; }
            set { m_LastSynchronizeCompletedTime = value; }
        }

        [DataMember]
        public StackHashProductSyncData ProductSyncData
        {
            get { return m_ProductSyncData; }
            set { m_ProductSyncData = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashProductCollection : Collection<StackHashProduct>
    {
        public StackHashProductCollection(){} // Needed to serialize.

        /// <summary>
        /// Locates the product with the specified id.
        /// </summary>
        /// <param name="productId">Id of product to find.</param>
        /// <returns>Found product or null.</returns>
        public StackHashProduct FindProduct(int productId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Id == productId)
                    return this[i];
            }

            return null;
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashProductInfoCollection : Collection<StackHashProductInfo>
    {
        public StackHashProductInfoCollection() { } // Needed to serialize.

        /// <summary>
        /// Locates the product with the specified id.
        /// </summary>
        /// <param name="productId">Id of product to find.</param>
        /// <returns>Found product or null.</returns>
        public StackHashProductInfo FindProduct(int productId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Product.Id == productId)
                    return this[i];
            }

            return null;
        }
    }
}
