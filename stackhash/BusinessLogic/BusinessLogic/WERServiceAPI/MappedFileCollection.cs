//---------------------------------------------------------------------
// <summary>
//      Class for representing a collection of MappedFile's in the API.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using StackHash.WindowsErrorReporting.Services.Data.API;
using System.Collections;

namespace StackHash.WindowsErrorReporting.Services.Mapping.API
{
    /// <summary>
    /// Class for representing a collection of MappedFile objects in the API.
    /// </summary>
    public class MappedFileCollection : Base, IList
    {
        private List<MappedFile> mappedFileCollection;
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MappedFileCollection class that is empty and has the default initial capacity.
        /// </summary>
        public MappedFileCollection() : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MappedFileCollection class that is empty and has the specified initial capacity. 
        /// </summary>
        /// <param name="capacity"></param>
        public MappedFileCollection(int capacity)
        {
            this.mappedFileCollection = new List<MappedFile>(capacity);
        }
        #endregion Constructors

        #region Methods
        #region Public Methods
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
            this.mappedFileCollection.Add((MappedFile)value);
            return (this.mappedFileCollection.Count - 1);
        }

        /// <summary>
        /// Removes all items from the IList.
        /// </summary>
        public void Clear()
        {
            this.mappedFileCollection.Clear();
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return this.mappedFileCollection.Contains((MappedFile)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return this.mappedFileCollection.IndexOf((MappedFile)value);
        }

        /// <summary>
        /// Inserts an item to the IList at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="value">The Object to insert into the IList.</param>
        public void Insert(int index, object value)
        {
            this.mappedFileCollection.Insert(index, (MappedFile)value);
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
            this.mappedFileCollection.Remove((MappedFile)value);
        }

        /// <summary>
        /// Removes the IList item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            this.mappedFileCollection.RemoveAt(index);
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
                return this.mappedFileCollection[index];
            }
            set
            {
                this.mappedFileCollection[index] = (MappedFile)value;
            }
        }
        #endregion IList Members

        #region ICollection Members
        /// <summary>
        /// Copys to the specified array.
        /// </summary>
        /// <param name="array">Arrays to copy to.</param>
        /// <param name="index">Index to copy from.</param>
        public void CopyTo(Array array, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the number of elements contained in the ICollection.
        /// </summary>
        public int Count
        {
            get { return this.mappedFileCollection.Count; }
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
            return this.mappedFileCollection.GetEnumerator();
        }
        #endregion IEnumerable Members
    }
}
