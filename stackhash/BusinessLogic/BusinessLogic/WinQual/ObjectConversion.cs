using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis; 

using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashBusinessObjects;

namespace StackHashWinQual
{

    /// <summary>
    /// Provides object conversion functions betweem WER objects and StackHash business objects.
    /// </summary>
    public sealed class ObjectConversion
    {
        private ObjectConversion() {;}

        /// <summary>
        /// Converts a WinQual Product object to a StackHashProduct.
        /// </summary>
        /// <param name="product">WinQual product to convert.</param>
        /// <returns>StackHashProduct equivalent type.</returns>
        public static StackHashProduct ConvertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            // Construct a WinQualProduct from a Product (returned from WinQual site).
            StackHashProduct newProduct = new StackHashProduct(
                product.DateCreatedLocal.ToUniversalTime(),
                product.DateModifiedLocal.ToUniversalTime(),
                product.FilesLink.ToString(),
                product.ID,
                product.Name,
                product.TotalEvents,
                product.TotalResponses,
                product.Version);

            return newProduct;
        }

        /// <summary>
        /// Converts a WinQual product collection to a StackHashProductCollection.
        /// </summary>
        /// <param name="products">Product collection to convert.</param>
        /// <returns>Converted product collection.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002", Justification="ProductCollection defined by Microsoft")]
        public static StackHashProductCollection ConvertProductCollection(ProductCollection products)
        {
            if (products == null)
                return null;

            StackHashProductCollection winQualProductCollection = new StackHashProductCollection();
          
            foreach (Product thisProduct in products)
            {
                StackHashProduct winQualProduct = ConvertProduct(thisProduct);
                winQualProductCollection.Add(winQualProduct);
            }

            return winQualProductCollection;
        }


        /// <summary>
        /// Converts a WinQual ApplicationFile into a StackHashFile.
        /// </summary>
        /// <param name="file">WinQual application file to convert.</param>
        /// <returns>Converted file.</returns>
        public static StackHashFile ConvertFile(ApplicationFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            StackHashFile newFile = new StackHashFile(
                file.DateCreatedLocal.ToUniversalTime(),
                file.DateModifiedLocal.ToUniversalTime(),
                file.ID,
                file.LinkDateLocal.ToUniversalTime(),
                file.Name,
                file.Version);

            return newFile;
        }


        /// <summary>
        /// Converts a WinQual EventSignature into a StackHashEventSignature.
        /// </summary>
        /// <param name="eventSignature">WinQual Event signature to convert.</param>
        /// <returns>Converted event signature.</returns>
        public static StackHashEventSignature ConvertEventSignature(EventSignature eventSignature)
        {
            if (eventSignature == null)
                throw new ArgumentNullException("eventSignature");

            StackHashParameterCollection paramCollection = new StackHashParameterCollection();

            foreach (Parameter param in eventSignature.Parameters)
            {
                StackHashParameter stackHashParam = new StackHashParameter(param.Name, param.Value);
                paramCollection.Add(stackHashParam);
            }

            StackHashEventSignature stackHashEventSignature = new StackHashEventSignature(paramCollection);
            return stackHashEventSignature;
        }


        /// <summary>
        /// Converts a WinQual Event into a StackHashEvent.
        /// </summary>
        /// <param name="thisEvent">WinQual event to convert.</param>
        /// <param name="fileId">Id of the owning file.</param>
        /// <returns>Converted event.</returns>
        public static StackHashEvent ConvertEvent(Event thisEvent, int fileId)
        {
            if (thisEvent == null)
                throw new ArgumentNullException("thisEvent");

            StackHashEventSignature eventSignature = ConvertEventSignature(thisEvent.Signature);

            StackHashEvent newEvent = new StackHashEvent(
                thisEvent.DateCreatedLocal.ToUniversalTime(),
                thisEvent.DateModifiedLocal.ToUniversalTime(),
                thisEvent.EventTypeName,
                thisEvent.ID,
                eventSignature,
                thisEvent.TotalHits,
                fileId);

            return newEvent;
        }

        /// <summary>
        /// Converts a WinQual EventInfo into a StackHashEventInfo.
        /// </summary>
        /// <param name="eventInfo">EventInfo to convert.</param>
        /// <returns>Converted event info.</returns>
        public static StackHashEventInfo ConvertEventInfo(EventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            StackHashEventInfo newEventInfo = new StackHashEventInfo(
                eventInfo.DateCreatedLocal.ToUniversalTime(),
                eventInfo.DateModifiedLocal.ToUniversalTime(),
                eventInfo.HitDateLocal.ToUniversalTime(),
                eventInfo.Language,
                eventInfo.LCID,
                eventInfo.Locale,
                eventInfo.OperatingSystemName,
                eventInfo.OperatingSystemVersion,
                eventInfo.TotalHits);

            return newEventInfo;
        }


        /// <summary>
        /// Converts a WinQual Cab into a StackHashCab.
        /// </summary>
        /// <param name="cab">Cab to convert.</param>
        /// <returns>Converted cab.</returns>
        public static StackHashCab ConvertCab(Cab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            StackHashCab newCab = new StackHashCab(cab.DateCreatedLocal.ToUniversalTime(),
                                                   cab.DateModifiedLocal.ToUniversalTime(), 
                                                   cab.EventID, 
                                                   cab.EventTypeName, 
                                                   cab.FileName, 
                                                   cab.ID, 
                                                   cab.SizeInBytes);
            return newCab;
        }
    }
}
