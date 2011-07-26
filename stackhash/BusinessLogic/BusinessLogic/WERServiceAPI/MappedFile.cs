//---------------------------------------------------------------------
// <summary>
//      Class for representing a MappedFile in the API. This object represents
//      the file mapped in the Developer Portal.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using StackHash.WindowsErrorReporting.Services.Data.API;
using System.Collections.Specialized;

namespace StackHash.WindowsErrorReporting.Services.Mapping.API
{
    /// <summary>
    /// Class for representing a MappedFile in the API. This object represents
    /// the file mapped in the Developer Portal.
    /// </summary>
    public class MappedFile : MappedObjectBase
    {
        #region Fields
        private DateTime linkDateLocal;
        #endregion Fields

        #region Properties
        #region Public Properties
        /// <summary>
        /// Gets the linker date of the file.
        /// This date is represented in local date-time.
        /// </summary>
        public DateTime LinkDateLocal
        {
            get { return this.linkDateLocal; }
            internal set { this.linkDateLocal = value; }
        }
        #endregion Public Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to delete the file from a list of mapped products.
        /// </summary>
        /// <param name="mappedProductIDs">List of mapped product ids.</param>
        /// <param name="loginObject">Object containing the login credentials.</param>
        public void DeleteFromMappedProducts(int[] mappedProductIDs, ref Login loginObject)
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
            // throw an exception if the mappedProductIDs array does not have a single element
            //
            if (mappedProductIDs.Length < 1)
            {
                ArgumentOutOfRangeException argumentOutOfRangeException = new ArgumentOutOfRangeException("mappedProductIDs", "The parameter should have atleast one element.");
                throw new FeedException("mappedProductIDs int array should have atleast one element.", argumentOutOfRangeException);
            }

            //
            // create a name value collection object to hold the mapped file id and product ids
            //
            NameValueCollection deleteFileFromProductsCollection = new NameValueCollection(mappedProductIDs.Length + 1);

            //
            // add the mapped file id to the collection
            //
            deleteFileFromProductsCollection.Add("mappedfileid", this.ID.ToString());

            //
            // add the list of mapped product ids to the collection
            //
            foreach (int mappedProductID in mappedProductIDs)
            {
                deleteFileFromProductsCollection.Add("mappedproductid", mappedProductID.ToString());
            }

            //
            // call the base ProcessRequest method to send the request and process the response
            //
            MappedObjectBase.ProcessRequest("deletemappedfilefromproducts.aspx", deleteFileFromProductsCollection, ref loginObject);
        }
        #endregion Public Methods

        #region Private Methods
        #endregion Private Methods
        #endregion Methods
    }
}
