using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml;

using StackHashBusinessObjects;


namespace WinQualAtomFeed
{
    internal class TestDatabaseEvent
    {
        public AtomEvent TheEvent { get; set; }
        public AtomEventInfoCollection EventInfos { get; set; }
        public AtomCabCollection Cabs { get; set; }
    }

    
    internal class TestDatabaseFile
    {
        public AtomFile File {get; set;}
        public List<TestDatabaseEvent> Events {get; set;}
    }

    internal class TestDatabaseProduct
    {
        public AtomProduct Product { get; set; }
        public List<TestDatabaseFile> Files { get; set; }
    }


    public class TestDatabase
    {
        String m_DatabaseFileName;
        List<TestDatabaseProduct> m_Products;


        private void readEventInfo(XmlTextReader xmlReader, TestDatabaseProduct product, TestDatabaseFile file, TestDatabaseEvent theEvent)
        {
            AtomEventInfo eventInfo = new AtomEventInfo();
            eventInfo.EventInfo = new StackHashEventInfo();

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.EndElement) &&
                    (xmlReader.Name.ToUpperInvariant() == "EVENTINFO"))
                {
                    theEvent.EventInfos.Add(eventInfo);
                    return;
                }

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    String elementName = xmlReader.Name.ToUpperInvariant();
                    xmlReader.Read();
                    String elementText = xmlReader.Value;

                    switch (elementName)
                    {
                        case "DATECREATEDLOCAL":
                            eventInfo.EventInfo.DateCreatedLocal = DateTime.Parse(elementText);
                            break;
                        case "DATEMODIFIEDLOCAL":
                            eventInfo.EventInfo.DateModifiedLocal = DateTime.Parse(elementText);
                            break;
                        case "HITDATELOCAL":
                            eventInfo.EventInfo.HitDateLocal = DateTime.Parse(elementText);
                            break;
                        case "LANGUAGE":
                            eventInfo.EventInfo.Language = elementText;
                            break;
                        case "LCID":
                            eventInfo.EventInfo.Lcid = Int32.Parse(elementText);
                            break;
                        case "LOCALE":
                            eventInfo.EventInfo.Locale = elementText;
                            break;
                        case "OPERATINGSYSTEMNAME":
                            eventInfo.EventInfo.OperatingSystemName = elementText;
                            break;
                        case "OPERATINGSYSTEMVERSION":
                            eventInfo.EventInfo.OperatingSystemVersion = elementText;
                            break;
                        case "TOTALHITS":
                            eventInfo.EventInfo.TotalHits = Int32.Parse(elementText);
                            break;
                    }

                }
            }
        }

        private void readCab(XmlTextReader xmlReader, TestDatabaseProduct product, TestDatabaseFile file, TestDatabaseEvent theEvent)
        {
            AtomCab cab = new AtomCab();
            cab.Cab = new StackHashCab();

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.EndElement) &&
                    (xmlReader.Name.ToUpperInvariant() == "CAB"))
                {
                    theEvent.Cabs.Add(cab);
                    return;
                }

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    String elementName = xmlReader.Name.ToUpperInvariant();
                    xmlReader.Read();
                    String elementText = xmlReader.Value;

                    switch (elementName)
                    {
                        case "DATECREATEDLOCAL":
                            cab.Cab.DateCreatedLocal = DateTime.Parse(elementText);
                            break;
                        case "DATEMODIFIEDLOCAL":
                            cab.Cab.DateModifiedLocal = DateTime.Parse(elementText);
                            break;
                        case "EVENTID":
                            cab.Cab.EventId = Int32.Parse(elementText);
                            break;
                        case "EVENTTYPENAME":
                            cab.Cab.EventTypeName = elementText;
                            break;
                        case "FILENAME":
                            cab.Cab.FileName = elementText;
                            break;
                        case "ID":
                            cab.Cab.Id = Int32.Parse(elementText);
                            break;
                        case "SIZEINBYTES":
                            cab.Cab.SizeInBytes = Int64.Parse(elementText);
                            break;
                    }

                }
            }
        }

        private void readEventSignature(XmlTextReader xmlReader, TestDatabaseProduct product, TestDatabaseFile file, TestDatabaseEvent theEvent)
        {
            StackHashEventSignature signature = new StackHashEventSignature();
            signature.Parameters = new StackHashParameterCollection();

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.EndElement) &&
                    (xmlReader.Name.ToUpperInvariant() == "SIGNATURE"))
                {
                    theEvent.TheEvent.Event.EventSignature = signature;
                    return;
                }

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    String elementName = xmlReader.Name;
                    xmlReader.Read();
                    String elementText = xmlReader.Value;
                    signature.Parameters.Add(new StackHashParameter(elementName, elementText));
                }
            }
        }


        
        private void readEvent(XmlTextReader xmlReader, TestDatabaseProduct product, TestDatabaseFile file)
        {
            TestDatabaseEvent theEvent = new TestDatabaseEvent();
            theEvent.TheEvent = new AtomEvent(new StackHashBusinessObjects.StackHashEvent(), "EventInfoLink", "CabLink");
            theEvent.TheEvent.ProductId = product.Product.Product.Id;
            theEvent.TheEvent.FileId = file.File.File.Id;

            if (theEvent.EventInfos == null)
                theEvent.EventInfos = new AtomEventInfoCollection();
            if (theEvent.Cabs == null)
                theEvent.Cabs = new AtomCabCollection();
            if (theEvent.TheEvent.Event.EventSignature == null)
                theEvent.TheEvent.Event.EventSignature = new StackHashEventSignature();

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.EndElement) &&
                    (xmlReader.Name.ToUpperInvariant() == "EVENT"))
                {
                    file.Events.Add(theEvent);
                    theEvent.TheEvent.Event.EventSignature.InterpretParameters();
                    return;
                }

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    String elementName = xmlReader.Name.ToUpperInvariant();
                    xmlReader.Read();
                    String elementText = xmlReader.Value;

                    switch (elementName)
                    {
                        case "EVENTINFO":
                            readEventInfo(xmlReader, product, file, theEvent);
                            break;
                        case "CAB":
                            readCab(xmlReader, product, file, theEvent);
                            break;

                        case "SIGNATURE":
                            readEventSignature(xmlReader, product, file, theEvent);
                            break;

                        case "ID":
                            theEvent.TheEvent.Event.Id = Int32.Parse(elementText);
                            break;
                        case "DATECREATEDLOCAL":
                            theEvent.TheEvent.Event.DateCreatedLocal = DateTime.Parse(elementText);
                            break;
                        case "DATEMODIFIEDLOCAL":
                            theEvent.TheEvent.Event.DateModifiedLocal = DateTime.Parse(elementText);
                            break;
                        case "EVENTTYPENAME":
                            theEvent.TheEvent.Event.EventTypeName = elementText;
                            break;
                        case "TOTALHITS":
                            theEvent.TheEvent.Event.TotalHits = Int32.Parse(elementText);
                            break;
                    }

                }
            }
        }



        private void readFile(XmlTextReader xmlReader, TestDatabaseProduct product)
        {
            TestDatabaseFile file = new TestDatabaseFile();
            file.File = new AtomFile(new StackHashBusinessObjects.StackHashFile(), "events link");
            file.File.ProductId = product.Product.Product.Id;

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.EndElement) &&
                    (xmlReader.Name.ToUpperInvariant() == "FILE"))
                {
                    product.Files.Add(file);
                    return;
                }

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    String elementName = xmlReader.Name.ToUpperInvariant();
                    xmlReader.Read();
                    String elementText = xmlReader.Value;

                    switch (elementName)
                    {
                        case "EVENT":
                            if (file.Events == null)
                                file.Events = new List<TestDatabaseEvent>();
                            readEvent(xmlReader, product, file);
                            
                            break;

                        case "ID":
                            file.File.File.Id = Int32.Parse(elementText);
                            break;
                        case "DATECREATEDLOCAL":
                            file.File.File.DateCreatedLocal = DateTime.Parse(elementText);
                            break;
                        case "DATEMODIFIEDLOCAL":
                            file.File.File.DateModifiedLocal = DateTime.Parse(elementText);
                            break;
                        case "LINKDATELOCAL":
                            file.File.File.LinkDateLocal = DateTime.Parse(elementText);
                            break;
                        case "NAME":
                            file.File.File.Name = elementText;
                            break;
                        case "VERSION":
                            file.File.File.Version = elementText;
                            break;
                    }

                }
            }
        }


        private void readProduct(XmlTextReader xmlReader)
        {
            TestDatabaseProduct product = new TestDatabaseProduct();
            product.Product = new AtomProduct(new StackHashBusinessObjects.StackHashProduct());

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.EndElement) &&
                    (xmlReader.Name.ToUpperInvariant() == "PRODUCT"))                    
                {
                    m_Products.Add(product);
                    return;
                }

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    String elementName = xmlReader.Name.ToUpperInvariant();
                    xmlReader.Read();
                    String elementText = xmlReader.Value;

                    switch (elementName)
                    {
                        case "FILE":
                            if (product.Files == null)
                                product.Files = new List<TestDatabaseFile>();
                            readFile(xmlReader, product);
                            break;

                        case "ID":
                            product.Product.Product.Id = Int32.Parse(elementText);
                            break;
                        case "DATECREATEDLOCAL":
                            product.Product.Product.DateCreatedLocal = DateTime.Parse(elementText);
                            break;
                        case "DATEMODIFIEDLOCAL":
                            product.Product.Product.DateModifiedLocal = DateTime.Parse(elementText);
                            break;
                        case "FILESLINK":
                            product.Product.Product.FilesLink = elementText;
                            break;
                        case "NAME":
                            product.Product.Product.Name = elementText;
                            break;
                        case "TOTALEVENTS":
                            product.Product.Product.TotalEvents = Int32.Parse(elementText);
                            break;
                        case "TOTALRESPONSES":
                            product.Product.Product.TotalResponses = Int32.Parse(elementText);
                            break;
                        case "VERSION":
                            product.Product.Product.Version = elementText;
                            break;
                    }

                }
            }
        }

        public AtomProductCollection GetProducts()
        {
            AtomProductCollection products = new AtomProductCollection();
            foreach (TestDatabaseProduct product in m_Products)
            {
                products.Add(product.Product);
            }
            return products;
        }

        public AtomFileCollection GetFiles(int productId)
        {
            AtomFileCollection files = new AtomFileCollection();
            foreach (TestDatabaseProduct product in m_Products)
            {
                if (product.Product.Product.Id == productId)
                {
                    foreach (TestDatabaseFile file in product.Files)
                    {
                        files.Add(file.File);
                    }
                }
            }
            return files;
        }


        public AtomEventCollection GetEvents(int productId, int fileId)
        {
            AtomEventCollection events = new AtomEventCollection();
            foreach (TestDatabaseProduct product in m_Products)
            {
                if (product.Product.Product.Id == productId)
                {
                    foreach (TestDatabaseFile file in product.Files)
                    {
                        if (file.File.File.Id == fileId)
                        {
                            foreach (TestDatabaseEvent thisEvent in file.Events)
                            {
                                events.Add(thisEvent.TheEvent);
                            }
                        }
                    }
                }
            }
            return events;
        }

        public AtomEventInfoCollection GetEventInfos(int productId, int fileId, int eventId)
        {
            AtomEventInfoCollection eventInfos = new AtomEventInfoCollection();
            foreach (TestDatabaseProduct product in m_Products)
            {
                if (product.Product.Product.Id == productId)
                {
                    foreach (TestDatabaseFile file in product.Files)
                    {
                        if (file.File.File.Id == fileId)
                        {
                            foreach (TestDatabaseEvent thisEvent in file.Events)
                            {
                                if (thisEvent.TheEvent.Event.Id == eventId)
                                {
                                    return thisEvent.EventInfos;
                                }
                            }
                        }
                    }
                }
            }
            return eventInfos;
        }

        public AtomCabCollection GetCabs(int productId, int fileId, int eventId)
        {
            AtomCabCollection cabs = new AtomCabCollection();
            foreach (TestDatabaseProduct product in m_Products)
            {
                if (product.Product.Product.Id == productId)
                {
                    foreach (TestDatabaseFile file in product.Files)
                    {
                        if (file.File.File.Id == fileId)
                        {
                            foreach (TestDatabaseEvent thisEvent in file.Events)
                            {
                                if (thisEvent.TheEvent.Event.Id == eventId)
                                {
                                    return thisEvent.Cabs;
                                }
                            }
                        }
                    }
                }
            }
            return cabs;
        }



        public TestDatabase(String databaseFileName)
        {
            if (String.IsNullOrEmpty(databaseFileName))
                throw new ArgumentNullException("databaseFileName");

            if (!File.Exists(databaseFileName))
                throw new ArgumentException("Test database file does not exist", "databaseFileName");

            m_DatabaseFileName = databaseFileName;

            // Create the products.
            m_Products = new List<TestDatabaseProduct>();

            // Load the file and contents.
            using (XmlTextReader xmlReader = new XmlTextReader(m_DatabaseFileName))
            {
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) &&
                        (xmlReader.Name.ToUpperInvariant() == "PRODUCT"))
                    {
                        readProduct(xmlReader);
                    }
                }
            }
        }
    }
}
