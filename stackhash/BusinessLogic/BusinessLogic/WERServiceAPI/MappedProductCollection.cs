//---------------------------------------------------------------------
// <summary>
//      Class for representing a collection of MappedProduct objects in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using StackHash.WindowsErrorReporting.Services.Data.API;
using System.Xml;
using System.Collections.Specialized;
using System.Collections;

namespace StackHash.WindowsErrorReporting.Services.Mapping.API
{
    /// <summary>
    /// Class for representing a collection of MappedProduct objects in the API.
    /// </summary>
    public class MappedProductCollection : Base, IList
    {
        private List<MappedProduct> mappedProductCollection;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ProductCollection class that is empty and has the default initial capacity.
        /// </summary>
        public MappedProductCollection() : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProductCollection class that is empty and has the specified initial capacity. 
        /// </summary>
        /// <param name="capacity"></param>
        public MappedProductCollection(int capacity)
        {
            this.mappedProductCollection = new List<MappedProduct>(capacity);
        }
        #endregion Constructors

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to get the collection of mapped products.
        /// </summary>
        /// <param name="loginObject">Object containing the login credentials.</param>
        /// <returns>The mapped product collection.</returns>
        public static MappedProductCollection GetMappedProducts(ref Login loginObject)
        {
            //
            // get the feed response string
            // 
            string responseFromServer = Base.GetFeedResponse(new Uri(BASE_URL + "mappedproducts.aspx"), ref loginObject);

            XmlNamespaceManager namespaceMgr;

            //
            // get the entry nodes from the XML
            //
            XmlNodeList entryNodes = Base.GetEntryNodes(responseFromServer, out namespaceMgr);

            //
            // create an empty product collection object.
            //
            MappedProductCollection mappedProducts = new MappedProductCollection();

            //
            // parse the entry elements and load the product objects
            //
            foreach (XmlNode entryNode in entryNodes)
            {
                MappedProduct mappedProduct = new MappedProduct();

                //title - mapped product name
                mappedProduct.Name = entryNode.SelectSingleNode("atom:title", namespaceMgr).InnerText;

                //id - mapped product id
                mappedProduct.ID = int.Parse(entryNode.SelectSingleNode("atom:id", namespaceMgr).InnerText);

                // updated - mapped product date modified
                mappedProduct.DateModifiedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:updated", namespaceMgr).InnerText).ToLocalTime();

                // published - mapped product date created
                mappedProduct.DateCreatedLocal = DateTime.Parse(entryNode.SelectSingleNode("atom:published", namespaceMgr).InnerText).ToLocalTime();

                // link - href - files link
                mappedProduct.MappedFilesLink = new Uri(entryNode.SelectSingleNode("atom:link", namespaceMgr).Attributes["href"].Value);

                // mapped product version
                mappedProduct.Version = entryNode.SelectSingleNode("wer:productVersion", namespaceMgr).InnerText;

                // mapped by
                mappedProduct.MappedBy = entryNode.SelectSingleNode("wer:mappedBy", namespaceMgr).InnerText;

                // mapped by e-mail
                mappedProduct.MappedByEmail = entryNode.SelectSingleNode("wer:mappedByEmail", namespaceMgr).InnerText;

                // status
                mappedProduct.Status = entryNode.SelectSingleNode("wer:status", namespaceMgr).InnerText;

                //
                // add the mapped product to the mapped product collection
                //
                mappedProducts.Add(mappedProduct);
            }

            //
            // return the mapped product collection
            //
            return mappedProducts;
        }

        /// <summary>
        /// Method to delete a list of mapped products.
        /// </summary>
        /// <param name="mappedProductIDs">List of mapped product ids.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        public static void DeleteMappedProducts(int[] mappedProductIDs, ref Login loginObject)
        {
            //
            // throw an exception if the mappedProductIDs is null
            //
            if (mappedProductIDs == null)
            {
                ArgumentNullException argumentNullException = new ArgumentNullException("mappedProductIDs", "The parameter cannot be null.");
                throw new FeedException("mappedProductIDs parameter cannot be null.", argumentNullException);
            }

            //
            // check to verify the mapped product ids contain alteast one element.
            //
            if (mappedProductIDs.Length < 1)
            {
                ArgumentOutOfRangeException argumentOutOfRangeException = new ArgumentOutOfRangeException("mappedProductIDs", "The parameter should have atleast one element.");
                throw new FeedException("mappedProductIDs int array should have atleast one element.", argumentOutOfRangeException);
            }

            //
            // create a name value collection object to hold the mapped product ids
            //
            NameValueCollection mappedProductIDCollection = new NameValueCollection(mappedProductIDs.Length);
            foreach (int mappedProductID in mappedProductIDs)
            {
                mappedProductIDCollection.Add("mappedproductid", mappedProductID.ToString());
            }

            //
            // call the base ProcessRequest method to send the request and process the response
            //
            MappedObjectBase.ProcessRequest("deletemappedproducts.aspx", mappedProductIDCollection, ref loginObject);
        }
        #endregion Public Methods
        #endregion Methods

        #region IList Members
        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            this.mappedProductCollection.Add((MappedProduct)value);
            return (this.mappedProductCollection.Count - 1);
        }

        /// <summary>
        /// Removes all items from the IList.
        /// </summary>
        public void Clear()
        {
            this.mappedProductCollection.Clear();
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return this.mappedProductCollection.Contains((MappedProduct)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return this.mappedProductCollection.IndexOf((MappedProduct)value);
        }

        /// <summary>
        /// Inserts an item to the IList at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="value">The Object to insert into the IList.</param>
        public void Insert(int index, object value)
        {
            this.mappedProductCollection.Insert(index, (MappedProduct)value);
        }

        /// <summary>
        /// Gets a value indicating whether the IList has a fixed size.
        /// </summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the IList is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(object value)
        {
            this.mappedProductCollection.Remove((MappedProduct)value);
        }

        /// <summary>
        /// Removes the IList item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            this.mappedProductCollection.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public object this[int index]
        {
            get
            {
                return this.mappedProductCollection[index];
            }
            set
            {
                this.mappedProductCollection[index] = (MappedProduct)value;
            }
        }
        #endregion IList Members

        #region ICollection Members
        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="array">Array to copy to.</param>
        /// <param name="index">Index at destination.</param>
        public void CopyTo(Array array, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the number of elements contained in the ICollection.
        /// </summary>
        public int Count
        {
            get { return this.mappedProductCollection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the ICollection is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public object SyncRoot
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        #endregion ICollection Members

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.mappedProductCollection.GetEnumerator();
        }
        #endregion IEnumerable Members
    }
}
