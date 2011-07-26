using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using WinQualAtomFeed;

namespace StackHashWinQual
{
    internal sealed class AtomObjectConversion
    {
        private AtomObjectConversion() { ;}

        /// <summary>
        /// Converts a WinQual Atom Product object to a StackHashProduct.
        /// </summary>
        /// <param name="product">WinQual product to convert.</param>
        /// <returns>StackHashProduct equivalent type.</returns>
        public static StackHashProduct ConvertProduct(AtomProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            return (product.Product);
        }

        /// <summary>
        /// Converts a WinQual Atom product collection to a StackHashProductCollection.
        /// </summary>
        /// <param name="products">Product collection to convert.</param>
        /// <returns>Converted product collection.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002", Justification = "ProductCollection defined by Microsoft")]
        [SuppressMessage("Microsoft.Performance", "CA1811")]
        public static StackHashProductCollection ConvertProductCollection(AtomProductCollection products)
        {
            if (products == null)
                return null;

            StackHashProductCollection winQualProductCollection = new StackHashProductCollection();

            foreach (AtomProduct thisProduct in products)
            {
                winQualProductCollection.Add(thisProduct.Product);
            }

            return winQualProductCollection;
        }


        /// <summary>
        /// Converts a WinQual Atom ApplicationFile into a StackHashFile.
        /// </summary>
        /// <param name="file">WinQual application file to convert.</param>
        /// <returns>Converted file.</returns>
        public static StackHashFile ConvertFile(AtomFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            return file.File;
        }


        /// <summary>
        /// Converts a WinQual Atom Event into a StackHashEvent.
        /// </summary>
        /// <param name="thisEvent">WinQual event to convert.</param>
        /// <returns>Converted event.</returns>
        public static StackHashEvent ConvertEvent(AtomEvent thisEvent)
        {
            if (thisEvent == null)
                throw new ArgumentNullException("thisEvent");

            return thisEvent.Event;
        }


        /// <summary>
        /// Converts a WinQual Atom EventInfo into a StackHashEventInfo.
        /// </summary>
        /// <param name="eventInfo">EventInfo to convert.</param>
        /// <returns>Converted event info.</returns>
        public static StackHashEventInfo ConvertEventInfo(AtomEventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            return eventInfo.EventInfo;
        }


        /// <summary>
        /// Converts a WinQual Atom Cab into a StackHashCab.
        /// </summary>
        /// <param name="cab">Cab to convert.</param>
        /// <returns>Converted cab.</returns>
        public static StackHashCab ConvertCab(AtomCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");
            return cab.Cab;
        }

        /// <summary>
        /// Converts a StackHashCab to a WinQual Atom Cab.
        /// </summary>
        /// <param name="cab">Cab to convert.</param>
        /// <returns>Converted cab.</returns>
        public static AtomCab ConvertToAtomCab(StackHashCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            String cabLink = AtomCab.MakeLink(cab.EventTypeName, cab.EventId, cab.Id, cab.SizeInBytes);

            AtomCab atomCab = new AtomCab(cab, cabLink);

            return atomCab;
        }
    }
}
