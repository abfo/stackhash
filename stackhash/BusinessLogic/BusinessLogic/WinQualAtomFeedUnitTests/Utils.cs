using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackHashBusinessObjects;
using WinQualAtomFeed;
using StackHash.WindowsErrorReporting.Services.Data.API;
using StackHashWinQual;


namespace WinQualAtomFeedUnitTests
{
    public static  class Utils
    {

        public static StackHashProductCollection GetProductsAtom(AtomFeed feed)
        {
            // Get the list of products.
            AtomProductCollection atomProducts = feed.GetProducts();

            // Convert to a StackHashProductCollection.
            StackHashProductCollection atomStackHashProducts = new StackHashProductCollection();
            foreach (AtomProduct atomProduct in atomProducts)
            {
                atomStackHashProducts.Add(atomProduct.Product);
            }

            return atomStackHashProducts;
        }

        public static StackHashProductCollection GetProductsApi(ref Login login)
        {
            ProductCollection apiProducts = Product.GetProducts(ref login);

            StackHashProductCollection apiStackHashProducts = new StackHashProductCollection();
            // Add the products first.
            foreach (Product product in apiProducts)
            {
                StackHashProduct stackHashProduct = ObjectConversion.ConvertProduct(product);
                apiStackHashProducts.Add(stackHashProduct);
            }
            return apiStackHashProducts;
        }

        public static StackHashFileCollection GetFilesAtom(AtomFeed feed, AtomProduct product)
        {
            // Get the list of files.
            AtomFileCollection atomFiles = feed.GetFiles(product);

            // Convert to a StackHashFileCollection.
            StackHashFileCollection atomStackHashFiles = new StackHashFileCollection();
            foreach (AtomFile atomFile in atomFiles)
            {
                atomStackHashFiles.Add(atomFile.File);
            }

            return atomStackHashFiles;
        }

        public static StackHashFileCollection GetFilesApi(ref Login login, Product product)
        {
            ApplicationFileCollection apiFiles = product.GetApplicationFiles(ref login);

            StackHashFileCollection apiStackHashFiles = new StackHashFileCollection();
            foreach (ApplicationFile file in apiFiles)
            {
                StackHashFile stackHashFile = ObjectConversion.ConvertFile(file);
                apiStackHashFiles.Add(stackHashFile);
            }
            return apiStackHashFiles;
        }

        public static Product FindProduct(ProductCollection products, int id)
        {
            foreach (Product thisProduct in products)
            {
                if (thisProduct.ID == id)
                    return thisProduct;
            }
            return null;
        }

        public static ApplicationFile FindFile(ApplicationFileCollection files, int id)
        {
            foreach (ApplicationFile file in files)
            {
                if (file.ID == id)
                    return file;
            }
            return null;
        }

        public static StackHashEventCollection GetEventsAtom(AtomFeed feed, AtomFile file)
        {
            // Get the list of events.
            AtomEventCollection atomEvents = feed.GetEvents(file);

            // Convert to a StackHashEventCollection.
            StackHashEventCollection atomStackHashEvents = new StackHashEventCollection();
            foreach (AtomEvent atomEvent in atomEvents)
            {
                atomStackHashEvents.Add(atomEvent.Event);
            }

            return atomStackHashEvents;
        }
        public static StackHashEventCollection GetEventsAtom(AtomFeed feed, AtomFile file, DateTime startTime)
        {
            // Get the list of events.
            AtomEventCollection atomEvents = feed.GetEvents(file, startTime);

            // Convert to a StackHashEventCollection.
            StackHashEventCollection atomStackHashEvents = new StackHashEventCollection();
            foreach (AtomEvent atomEvent in atomEvents)
            {
                atomStackHashEvents.Add(atomEvent.Event);
            }

            return atomStackHashEvents;
        }

        public static StackHashEventCollection GetEventsApi(ref Login login, ApplicationFile file, out List<Event> rawEvents)
        {
            EventPageReader eventPageReader = file.GetEvents(); // Get all events.
            StackHashEventCollection apiStackHashEvents = new StackHashEventCollection();
            rawEvents = new List<Event>();

            // Read each page of new events.
            while (eventPageReader.Read(ref login) == true)
            {
                // Get the events for the page.
                EventReader events = eventPageReader.Events;

                while (events.Read() == true)
                {
                    // Get the event
                    Event dpEvent = events.Event;
                    rawEvents.Add(dpEvent);
                    StackHashEvent stackHashEvent = ObjectConversion.ConvertEvent(dpEvent, file.ID);


                    apiStackHashEvents.Add(stackHashEvent);
                }
            }
            return apiStackHashEvents;
        }
        public static StackHashEventCollection GetEventsApi(ref Login login, ApplicationFile file, out List<Event> rawEvents, DateTime startTime)
        {
            EventPageReader eventPageReader = file.GetEvents(startTime); // Get all events.
            StackHashEventCollection apiStackHashEvents = new StackHashEventCollection();
            rawEvents = new List<Event>();

            // Read each page of new events.
            while (eventPageReader.Read(ref login) == true)
            {
                // Get the events for the page.
                EventReader events = eventPageReader.Events;

                while (events.Read() == true)
                {
                    // Get the event
                    Event dpEvent = events.Event;
                    rawEvents.Add(dpEvent);
                    StackHashEvent stackHashEvent = ObjectConversion.ConvertEvent(dpEvent, file.ID);


                    apiStackHashEvents.Add(stackHashEvent);
                }
            }
            return apiStackHashEvents;
        }


        public static StackHashEventInfoCollection GetEventInfoAtom(AtomFeed feed, AtomEvent theEvent, int days)
        {
            // Get the list of events.
            AtomEventInfoCollection atomEventInfos = feed.GetEventDetails(theEvent, days);

            // Convert to a StackHashEventInfoCollection.
            StackHashEventInfoCollection atomStackHashEventInfos = new StackHashEventInfoCollection();

            foreach (AtomEventInfo atomEventInfo in atomEventInfos)
            {
                atomStackHashEventInfos.Add(atomEventInfo.EventInfo);
            }

            return atomStackHashEventInfos;
        }

        public static StackHashEventInfoCollection GetEventInfoApi(ref Login login, Event theEvent)
        {
            EventInfoCollection eventInfos = theEvent.GetEventDetails(ref login);
            
            StackHashEventInfoCollection apiStackHashEventInfos = new StackHashEventInfoCollection();

            foreach (EventInfo eventInfo in eventInfos)
            {
                StackHashEventInfo stackHashEventInfo = ObjectConversion.ConvertEventInfo(eventInfo);
                apiStackHashEventInfos.Add(stackHashEventInfo);
            }
            return apiStackHashEventInfos;
        }

        public static StackHashCabCollection GetCabInfoAtom(AtomFeed feed, AtomEvent theEvent)
        {
            // Get the list of events.
            AtomCabCollection atomCabs = feed.GetCabs(theEvent);

            // Convert to a StackHashCabCollection.
            StackHashCabCollection atomStackHashCabss = new StackHashCabCollection();

            foreach (AtomCab atomCab in atomCabs)
            {
                atomStackHashCabss.Add(atomCab.Cab);
            }

            return atomStackHashCabss;
        }

        public static StackHashCabCollection GetCabInfoApi(ref Login login, Event theEvent)
        {
            CabCollection cabs = theEvent.GetCabs(ref login);

            StackHashCabCollection apiStackHashCabs = new StackHashCabCollection();

            foreach (Cab cab in cabs)
            {
                StackHashCab stackHashCab = ObjectConversion.ConvertCab(cab);
                apiStackHashCabs.Add(stackHashCab);
            }
            return apiStackHashCabs;
        }

        public static Event FindApiEvent(List<Event> events, int id)
        {
            foreach (Event theEvent in events)
            {
                if (theEvent.ID == id)
                    return theEvent;
            }
            return null;
        }
    }
}
