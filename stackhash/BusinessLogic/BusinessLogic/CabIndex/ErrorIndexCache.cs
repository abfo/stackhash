using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashBusinessObjects;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace StackHashErrorIndex
{

    /// <summary>
    /// Cached EVENT information.
    /// </summary>
    internal class ErrorIndexCacheEvent : IDisposable
    {
        private StackHashProduct m_ParentProduct;
        private StackHashFile m_ParentFile;

        private IErrorIndex m_RealErrorIndex;
        private StackHashEvent m_Event;
        private StackHashEventInfoCollection m_EventInfo;
        private Dictionary<int, StackHashCab> m_CabInfo;

        /// <summary>
        /// Data associated with the real event.
        /// </summary>
        public StackHashEvent Event
        {
            get 
            {
                Monitor.Enter(this);
                try
                {
                    return m_Event;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            set 
            { 
                Monitor.Enter(this);
                try
                {
                    m_Event = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Creates a cache version of the Event object.
        /// </summary>
        /// <param name="realErrorIndex">Location of the real error index.</param>
        /// <param name="parentProduct">Owning parent product.</param>
        /// <param name="parentFile">Owning parent file.</param>
        /// <param name="theEvent">The new event data.</param>
        public ErrorIndexCacheEvent(IErrorIndex realErrorIndex, StackHashProduct parentProduct, 
            StackHashFile parentFile, StackHashEvent theEvent)
        {
            m_RealErrorIndex = realErrorIndex;
            m_ParentProduct = parentProduct;
            m_ParentFile = parentFile;
            m_Event = theEvent;
        }

        /// <summary>
        /// Safely clones the specified event data.
        /// </summary>
        /// <returns>Cloned event</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811")]
        public StackHashEvent CloneEvent()
        {
            Monitor.Enter(this);
            try
            {
                return (StackHashEvent)m_Event.Clone();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Loads the event info list into the cache. 
        /// </summary>
        /// <returns>Null if already cached or event list.</returns>
        private StackHashEventInfoCollection cacheEventInfoList()
        {
            // Check if the event info list has been loaded yet.

            if (m_EventInfo == null)
            {
                // Not yet loaded so load from the real database.
                m_EventInfo = m_RealErrorIndex.LoadEventInfoList(m_ParentProduct, m_ParentFile, m_Event);
            }

            return m_EventInfo;
        }


        /// <summary>
        /// Loads the cab info list into the cache. 
        /// </summary>
        /// <returns>Null if already cached or event list.</returns>
        private StackHashCabCollection cacheCabList()
        {
            StackHashCabCollection cabs  = new StackHashCabCollection();

            // Check if the event info list has been loaded yet.
            if (m_CabInfo == null)
            {
                m_CabInfo = new Dictionary<int, StackHashCab>();

                // Not yet loaded so load from the real database.
                cabs = m_RealErrorIndex.LoadCabList(m_ParentProduct, m_ParentFile, m_Event);

                foreach (StackHashCab cab in cabs)
                {
                    m_CabInfo.Add(cab.Id, cab);
                }
            }

            return cabs;
        }


        /// <summary>
        /// Loads the event info list associated with this event.
        /// </summary>
        /// <returns>List of files.</returns>
        public StackHashEventInfoCollection LoadEventInfoList()
        {
            StackHashEventInfoCollection events = null;

            Monitor.Enter(this);
            try
            {
                // Loads and return the event list.
                if (m_EventInfo == null)
                {
                    // Not yet loaded so load from the real database.
                    events = cacheEventInfoList();
                }
                else
                {
                    // Derive from the cache.
                    events = m_EventInfo;
                }

                return (StackHashEventInfoCollection)events.Clone();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Loads the cab info list associated with this event.
        /// </summary>
        /// <returns>List of files.</returns>
        public StackHashCabCollection LoadCabList()
        {
            StackHashCabCollection cabCollection;

            Monitor.Enter(this);
            try
            {
                // Loads and return the event list.
                if (m_CabInfo == null)
                {
                    // Not yet loaded so load from the real database.
                    cabCollection = cacheCabList();
                }
                else
                {
                    cabCollection = new StackHashCabCollection();

                    foreach (StackHashCab cab in m_CabInfo.Values)
                    {
                        cabCollection.Add(cab);
                    }
                }

                return (StackHashCabCollection)cabCollection.Clone();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Check if the specified cab exists in the database.
        /// </summary>
        /// <param name="theCab">The event to search for.</param>
        public bool CabExists(StackHashCab cab)
        {
            Monitor.Enter(this);
            try
            {
                if (cab == null)
                    throw new ArgumentNullException("cab");

                // Check if the cab list has been loaded.
                if (m_CabInfo == null)
                    LoadCabList();

                // Now check if the cab ID is present in the event list.
                return (m_CabInfo.ContainsKey(cab.Id));
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Returns the cab with the specified ID.
        /// </summary>
        /// <param name="cabId">ID of cab to get.</param>
        /// <returns>The cab or null if not found.</returns>
        public StackHashCab GetCab(int cabId)
        {
            Monitor.Enter(this);
            try
            {
                // Check if the cab list has been loaded.
                if (m_CabInfo == null)
                    LoadCabList();

                if (m_CabInfo.ContainsKey(cabId))
                    return (StackHashCab)m_CabInfo[cabId].Clone();
                else
                    return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Returns the number of cabs present for the this event.
        /// </summary>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabCount()
        {
            Monitor.Enter(this);
            try
            {
                // Check if the cab list has been loaded.
                if (m_CabInfo == null)
                    LoadCabList();

                return m_CabInfo.Count;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the most recent hit date in the event info collection.
        /// </summary>
        /// <returns>The most recent hit date.</returns>
        public DateTime GetMostRecentHitDate()
        {
            Monitor.Enter(this);
            try
            {
                // Check if the cab list has been loaded.
                if (m_EventInfo == null)
                    LoadEventInfoList();

                DateTime mostRecentDate = new DateTime(0, DateTimeKind.Local);
                foreach (StackHashEventInfo eventInfo in m_EventInfo)
                {
                    if (eventInfo.DateCreatedLocal > mostRecentDate)
                        mostRecentDate = eventInfo.DateCreatedLocal;
                }

                return mostRecentDate;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Adds event info to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="eventInfoCollection">The event collection to add.</param>
        public void AddEventInfoCollection(StackHashEventInfoCollection eventInfoCollection)
        {
            Monitor.Enter(this);

            try
            {
                // Check if the event list has been loaded.
                if (m_EventInfo == null)
                    cacheEventInfoList();

                m_EventInfo = (StackHashEventInfoCollection)eventInfoCollection.Clone();

                // Update the database.
                m_RealErrorIndex.AddEventInfoCollection(m_ParentProduct, m_ParentFile, m_Event, eventInfoCollection);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Merges event info to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="eventInfoCollection">The event collection to add.</param>
        public void MergeEventInfoCollection(StackHashEventInfoCollection eventInfoCollection)
        {
            Monitor.Enter(this);
            try
            {
                // Check if the event list has been loaded.
                if (m_EventInfo == null)
                    cacheEventInfoList();

                foreach (StackHashEventInfo eventInfo in eventInfoCollection)
                {
                    // Find the entry by hit date (effectively the ID).
                    StackHashEventInfo existingEventInfo = m_EventInfo.FindEventInfo(eventInfo);

                    if (existingEventInfo != null)
                    {
                        // Already exists so just copy the fields.
                        existingEventInfo.SetWinQualFields(eventInfo);
                    }
                    else
                    {
                        // A new one to be added.
                        m_EventInfo.Add((StackHashEventInfo)eventInfo.Clone());
                    }
                }

                // Update the database - don't call merge as we are replacing the existing entry.
                m_RealErrorIndex.AddEventInfoCollection(m_ParentProduct, m_ParentFile, m_Event, m_EventInfo);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Adds cab info to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="cab">The cab to add.</param>
        /// <param name="setDiagnosticInfo">True - sets diagnostic info, false doesn't.</param>
        public StackHashCab AddCab(StackHashCab cab, bool setDiagnosticInfo)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            Monitor.Enter(this);
            try
            {
                // Check if the cab list has been loaded.
                if (m_CabInfo == null)
                    cacheCabList();

                // Update the real database - might change.
                StackHashCab newCab = m_RealErrorIndex.AddCab(m_ParentProduct, m_ParentFile, m_Event, cab, setDiagnosticInfo);

                if (m_CabInfo.ContainsKey(cab.Id))
                {
                    m_CabInfo[cab.Id] = (StackHashCab)newCab.Clone();
                }
                else
                {
                    // Add a new entry.
                    m_CabInfo.Add(cab.Id, (StackHashCab)newCab.Clone());
                }

                return (StackHashCab)newCab.Clone();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Retrieves stats associated with the number of events etc...
        /// </summary>
        /// <returns>Database statistics.</returns>
        public StackHashSynchronizeStatistics Statistics
        {
            get
            {
                Monitor.Enter(this);
                try
                {
                    StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics();

                    // Check if the event info list has been loaded.
                    if (m_EventInfo == null)
                        cacheEventInfoList();

                    // Check if the cab list has been loaded.
                    if (m_CabInfo == null)
                        cacheCabList();

                    stats.EventInfos = m_EventInfo.Count;
                    stats.Cabs = m_CabInfo.Count;

                    return stats;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }


    /// <summary>
    /// Each file represents an installed component of the product.
    /// A file has associated events and event info.
    /// </summary>
    internal class ErrorIndexCacheFile : IDisposable
    {
        private StackHashProduct m_ParentProduct;
        private StackHashFile m_File; // Information about this file.
        private IErrorIndex m_RealErrorIndex; 
        private Dictionary<int, ErrorIndexCacheEvent> m_Events; // Indexed by event ID.
        private ReaderWriterLockSlim m_EventsLock; // Protects m_Events.


        public ErrorIndexCacheFile(IErrorIndex realErrorIndex, 
                                   StackHashProduct parentProduct, 
                                   StackHashFile file)
        {
            m_ParentProduct = parentProduct;
            m_File = file;
            m_RealErrorIndex = realErrorIndex;
            m_EventsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        /// <summary>
        /// Loads the event list into the cache. 
        /// </summary>
        /// <returns>Null if already cached or event list.</returns>
        private StackHashEventCollection cacheEventList()
        {
            StackHashEventCollection events = null;

            m_EventsLock.EnterWriteLock();

            try
            {
                // Check if the event list has been loaded yet.
                if (m_Events == null)
                {
                    m_Events = new Dictionary<int, ErrorIndexCacheEvent>();

                    // Not yet loaded so load from the real database.
                    events = m_RealErrorIndex.LoadEventList(m_ParentProduct, m_File);

                    // Add them to the local cache.
                    foreach (StackHashEvent theEvent in events)
                    {
                        ErrorIndexCacheEvent cacheEvent =
                            new ErrorIndexCacheEvent(m_RealErrorIndex, m_ParentProduct, m_File, theEvent);
                        m_Events.Add(theEvent.Id, cacheEvent);
                    }
                }

                return events;
            }
            finally
            {
                m_EventsLock.ExitWriteLock();
            }
        }


        private void checkEventIsUpToDate(ErrorIndexCacheEvent cacheEvent)
        {
            if ((cacheEvent.Event.TotalHits == -1) ||
                (cacheEvent.Event.TotalHits == 0))
            {
                // Read the data in from the real index again.
                cacheEvent.Event = m_RealErrorIndex.GetEvent(m_ParentProduct, m_File, cacheEvent.Event);
            }
        }


        /// <summary>
        /// Safely gets a pointer to object specified by the id.
        /// </summary>
        /// <param name="id">Id of the event object to get.</param>
        /// <returns>The event object with the specified id or null.</returns>
        private ErrorIndexCacheEvent getEventObject(int id)
        {
            m_EventsLock.EnterReadLock();

            try
            {
                if (m_Events != null)
                    if (m_Events.ContainsKey(id))
                        return m_Events[id];

                return null;
            }
            finally
            {
                m_EventsLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Returns the number of cabs present for the specified event.
        /// </summary>
        /// <param name="theEvent">The event to count the cabs for</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabCount(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);

            if (cacheEvent == null)
                throw new ArgumentException("Event does not exist", "theEvent");

            return cacheEvent.GetCabCount();
        }


        /// <summary>
        /// Gets the most recent hit date in the event info collection.
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <returns>The most recent hit date.</returns>
        public DateTime GetMostRecentHitDate(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);

            if (cacheEvent == null)
                throw new ArgumentException("Event does not exist", "theEvent");

            return cacheEvent.GetMostRecentHitDate();
        }

        /// <summary>
        /// Returns the cab with the specified ID.
        /// </summary>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cabId">ID of cab to get.</param>
        /// <returns>The cab or null if not found.</returns>
        public StackHashCab GetCab(StackHashEvent theEvent, int cabId)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (m_Events == null)
                cacheEventList();

            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent != null)
            {
                return cacheEvent.GetCab(cabId);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Refreshes the specified event data.
        /// </summary>
        /// <param name="theEvent">The event to refresh.</param>
        /// <returns>The refreshed event data.</returns>
        public StackHashEvent GetEvent(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Events == null)
                cacheEventList();

            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent != null)
            {
                checkEventIsUpToDate(cacheEvent);

                return (StackHashEvent)cacheEvent.Event.Clone();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parses all events associated with the specified product/file.
        /// </summary>
        /// <param name="parser">Parsing callback details.</param>
        /// <returns>true - aborted, false otherwise.</returns>
        public bool ParseEvents(ErrorIndexEventParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            // Check if the product list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // This may take some time so writes will be locked - i.e. the WinQualSync.
            m_EventsLock.EnterReadLock();

            // Create a list of keys.
            int [] keys = new int [m_Events.Count];

            try
            {
                int index = 0;
                foreach (int key in m_Events.Keys)
                {
                    keys[index++] = key;
                }
            }
            finally
            {
                m_EventsLock.ExitReadLock();
            }

            ErrorIndexCacheEvent entry = null;

            foreach (int key in keys)
            {
                m_EventsLock.EnterReadLock();

                try
                {
                    if (m_Events.ContainsKey(key))
                        entry = m_Events[key];
                }
                finally
                {
                    m_EventsLock.ExitReadLock();
                }

                Monitor.Enter(entry);
                try
                {
                    parser.CurrentEvent = entry.Event;

                    // Call the parser.
                    parser.ProcessEvent();

                    if (parser.Abort)
                        return false;
                }
                finally
                {
                    Monitor.Exit(entry);
                }
            }

            return true;
        }


        /// <summary>
        /// Loads the event list associated with this product file.
        /// </summary>
        /// <returns>List of files.</returns>
        public StackHashEventCollection LoadEventList()
        {
            StackHashEventCollection events = null;

            // Loads and return the event list.
            if (m_Events == null)
            {
                // Not yet loaded so load from the real database.
                events = cacheEventList();
            }
            else
            {
                // Derive from the cache.
                events = new StackHashEventCollection();

                m_EventsLock.EnterReadLock();

                try
                {
                    foreach (ErrorIndexCacheEvent cacheEvent in m_Events.Values)
                    {
                        checkEventIsUpToDate(cacheEvent);

                        events.Add((StackHashEvent)cacheEvent.Event.Clone());
                    }
                }
                finally
                {
                    m_EventsLock.ExitReadLock();
                }
            }

            return events;
        }

        /// <summary>
        /// Loads the event info list for the specified event.
        /// </summary>
        /// <returns>List of events.</returns>
        public StackHashEventInfoCollection LoadEventInfoList(StackHashEvent theEvent)
        {
            // Make sure the file list is cached.
            if (m_Events == null)
            {
                // Not yet loaded so load from the real database.
                cacheEventList();
            }

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent == null)
                throw new ArgumentException("Unknown event ID", "theEvent");

            // Load the event info for the event.
            StackHashEventInfoCollection events = cacheEvent.LoadEventInfoList();

            return events;
        }


        /// <summary>
        /// Check if the specified event exists in the database.
        /// </summary>
        /// <param name="theEvent">The event to search for.</param>
        public bool EventExists(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);

            // Now check if the event ID is present in the event list.
            return (cacheEvent != null);
        }


        /// <summary>
        /// Check if the specified cab exists in the database.
        /// </summary>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="theCab">The cab to search for.</param>
        public bool CabExists(StackHashEvent theEvent, StackHashCab cab)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (cab == null)
                throw new ArgumentNullException("cab");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent == null)
                throw new ArgumentException("Event does not exist", "theEvent");

            return cacheEvent.CabExists(cab);
        }

        
        /// <summary>
        /// Loads the cab data for the specified event.
        /// </summary>
        /// <returns>List of cabs.</returns>
        public StackHashCabCollection LoadCabList(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Make sure the file list is cached.
            if (m_Events == null)
            {
                // Not yet loaded so load from the real database.
                cacheEventList();
            }

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent == null)
                throw new ArgumentException("Unknown event ID", "theEvent");

            // Load the cab for the event.
            StackHashCabCollection cabs = cacheEvent.LoadCabList();

            return cabs;
        }


        /// <summary>
        /// Adds an event to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields.</param>
        public void AddEvent(StackHashEvent theEvent, bool updateNonWinQualFields)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            m_EventsLock.EnterWriteLock();

            try
            {
                if (m_Events.ContainsKey(theEvent.Id))
                {
                    if (updateNonWinQualFields)
                    {
                        m_Events[theEvent.Id].Event = (StackHashEvent)theEvent.Clone();
                    }
                    else
                    {
                        // Only overwrite the WinQual fields.
                        StackHashEvent storedEvent = m_Events[theEvent.Id].Event;
                        storedEvent.SetWinQualFields(theEvent);
                        theEvent = m_Events[theEvent.Id].Event;
                    }
                }
                else
                {
                    // Add the event to the cache and database.
                    m_Events.Add(theEvent.Id,
                        new ErrorIndexCacheEvent(m_RealErrorIndex, m_ParentProduct, m_File, (StackHashEvent)theEvent.Clone()));
                }

                // Update the database.
                m_RealErrorIndex.AddEvent(m_ParentProduct, m_File, theEvent, true);
            }
            finally
            {
                m_EventsLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds event info to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="eventInfoCollection">The event collection to add.</param>
        public void AddEventInfoCollection(StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent == null)
                throw new ArgumentException("Event not found: " + theEvent.Id, "theEvent");

            // Let the product update the event info.
            cacheEvent.AddEventInfoCollection(eventInfoCollection);
        }

        /// <summary>
        /// Merges event info to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="eventInfoCollection">The event collection to add.</param>
        public void MergeEventInfoCollection(StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent == null)
                throw new ArgumentException("Event not found: " + theEvent.Id, "theEvent");

            // Let the product update the event info.
            cacheEvent.MergeEventInfoCollection(eventInfoCollection);
        }


        /// <summary>
        /// Adds cab info to the cache.
        /// If the event is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="cab">The cab to add.</param>
        /// <param name="setDiagnosticInfo">True - set diagnostic data, False - don't.</param>
        public StackHashCab AddCab(StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the event list has been loaded.
            if (m_Events == null)
                cacheEventList();

            // Now check if the event ID is present in the event list.
            ErrorIndexCacheEvent cacheEvent = getEventObject(theEvent.Id);
            if (cacheEvent == null)
                throw new ArgumentException("Event not found: " + theEvent.Id, "theEvent");

            // Let the product update the event.
            return cacheEvent.AddCab(cab, setDiagnosticInfo);
        }

        /// <summary>
        /// Gets all event information associated with all events the file knows about.
        /// </summary>
        /// <returns>List of all events.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811")]
        public StackHashEventPackageCollection GetAllEvents(int filter)
        {
            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            // Check if the events list has been loaded.
            if (m_Events == null)
                cacheEventList();

            if (filter != 0)
                throw new NotImplementedException();

            m_EventsLock.EnterReadLock();

            try
            {
                foreach (ErrorIndexCacheEvent thisEvent in m_Events.Values)
                {
                    checkEventIsUpToDate(thisEvent);

                    StackHashEventInfoCollection eventInfos = thisEvent.LoadEventInfoList();
                    StackHashCabCollection cabs = thisEvent.LoadCabList();

                    StackHashEventPackage eventPackage = new StackHashEventPackage(eventInfos, new StackHashCabPackageCollection(cabs), 
                        (StackHashEvent)thisEvent.Event.Clone(), m_ParentProduct.Id);

                    allEvents.Add(eventPackage);
                }

                return allEvents;
            }
            finally
            {
                m_EventsLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all events within the specified range.
        /// </summary>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetEvents(long startRow, long numberOfRows)
        {
            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            // Check if the events list has been loaded.
            if (m_Events == null)
                cacheEventList();

            m_EventsLock.EnterReadLock();

            try
            {
                int currentRow = 1;
                foreach (ErrorIndexCacheEvent thisEvent in m_Events.Values)
                {
                    if (currentRow >= startRow)
                    {
                        if (currentRow < startRow + numberOfRows)
                        {
                            StackHashEventInfoCollection eventInfos = thisEvent.LoadEventInfoList();
                            StackHashCabCollection cabs = thisEvent.LoadCabList();

                            StackHashEventPackage eventPackage = new StackHashEventPackage(eventInfos, new StackHashCabPackageCollection(cabs), 
                                (StackHashEvent)thisEvent.Event.Clone(), m_ParentProduct.Id);

                            allEvents.Add(eventPackage);
                        }
                        else
                        {
                            break;
                        }
                    }
                    currentRow++;
                }

                return allEvents;
            }
            finally
            {
                m_EventsLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Gets all event information matching the specified search options.
        /// </summary>
        /// <param name="StackHashSearchCriteria">Characteristics to look for.</param>
        /// <param name="allEvents">List to add to.</param>
        public void GetEvents(StackHashSearchCriteria stackHashSearchCriteria, StackHashEventPackageCollection allEvents)
        {
            if (stackHashSearchCriteria == null)
                throw new ArgumentNullException("stackHashSearchCriteria");

            // Check if the file list has been loaded.
            if (m_Events == null)
                cacheEventList();

            m_EventsLock.EnterReadLock();

            try
            {
                // Check for a product field match.
                foreach (ErrorIndexCacheEvent thisEvent in m_Events.Values)
                {
                    checkEventIsUpToDate(thisEvent);

                    // Don't need to add the same event twice.
                    // TODO: SPEED THIS UP.
                    if (allEvents.FindEventPackage(thisEvent.Event.Id, thisEvent.Event.EventTypeName) != null)
                        continue;

                    if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.Event) ||
                        stackHashSearchCriteria.IsMatch(StackHashObjectType.Event, thisEvent.Event))
                    {
                        StackHashEventInfoCollection eventInfos = thisEvent.LoadEventInfoList();

                        // If the search criteria contains an EventInfo option then there must be at least 1
                        // matching event info found here. If not then all event infos are deemed to match.
                        // If there is an event info option - all of those options must match.

                        bool eventSignatureMatch = false;
                        if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.EventSignature))
                        {
                            eventSignatureMatch = true;
                        }
                        else
                        {
                            if (stackHashSearchCriteria.ContainsObject(StackHashObjectType.EventSignature))
                            {
                                if (stackHashSearchCriteria.IsMatch(StackHashObjectType.EventSignature, thisEvent.Event.EventSignature))
                                {
                                    eventSignatureMatch = true;
                                }
                            }
                            else
                            {
                                eventSignatureMatch = true;
                            }
                        }

                        bool eventInfoMatch = false;
                        if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.EventInfo))
                        {
                            eventInfoMatch = true;
                        }
                        else
                        {
                            if (stackHashSearchCriteria.ContainsObject(StackHashObjectType.EventInfo))
                            {
                                foreach (StackHashEventInfo eventInfo in eventInfos)
                                {
                                    if (stackHashSearchCriteria.IsMatch(StackHashObjectType.EventInfo, eventInfo))
                                    {
                                        eventInfoMatch = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                eventInfoMatch = true;
                            }
                        }


                        StackHashCabCollection cabs = thisEvent.LoadCabList();

                        bool cabInfoMatch = false;

                        if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.CabInfo))
                        {
                            cabInfoMatch = true;
                        }
                        else
                        {
                            if (stackHashSearchCriteria.ContainsObject(StackHashObjectType.CabInfo))
                            {
                                foreach (StackHashCab cab in cabs)
                                {
                                    if (stackHashSearchCriteria.IsMatch(StackHashObjectType.CabInfo, cab))
                                    {

                                        cabInfoMatch = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                cabInfoMatch = true;
                            }
                        }

                        if (eventInfoMatch && cabInfoMatch && eventSignatureMatch)
                        {
                            StackHashEventPackage eventPackage = new StackHashEventPackage(eventInfos, new StackHashCabPackageCollection(cabs), 
                                (StackHashEvent)thisEvent.Event.Clone(), m_ParentProduct.Id);
                            allEvents.Add(eventPackage);
                        }
                    }
                }
            }
            finally
            {
                m_EventsLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Retrieves stats associated with the number of events etc...
        /// </summary>
        /// <returns>Database statistics.</returns>
        public StackHashSynchronizeStatistics Statistics
        {
            get
            {
                StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics();

                // Check if the product list has been loaded.
                if (m_Events == null)
                    cacheEventList();

                m_EventsLock.EnterReadLock();

                try
                {
                    // Work through all the products and files.
                    foreach (ErrorIndexCacheEvent thisEvent in m_Events.Values)
                    {
                        stats.Add(thisEvent.Statistics.Clone());
                    }

                    return stats;
                }
                finally
                {
                    m_EventsLock.ExitReadLock();
                }
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_EventsLock != null)
                    m_EventsLock.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }



    /// <summary>
    /// Each product in the database contains product data and a file list accessed by fileId.
    /// Products could represent different versions of the same product (including public betas)
    /// as well as entirely different products (Word, IE, Excel, ...) along with their versions.
    /// </summary>
    internal class ErrorIndexCacheProduct : IDisposable
    {
        StackHashProduct m_Product;
        IErrorIndex m_RealErrorIndex;

        Dictionary<int, ErrorIndexCacheFile> m_Files; // Indexed by file ID.
        ReaderWriterLockSlim m_FileLock;


        public ErrorIndexCacheProduct(IErrorIndex realErrorIndex, StackHashProduct product)
        {
            m_RealErrorIndex = realErrorIndex;
            m_Product = product;
            m_FileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        private ErrorIndexCacheFile getFileObject(int id)
        {

            m_FileLock.EnterReadLock();


            try
            {
                if (m_Files != null)
                    if (m_Files.ContainsKey(id))
                        return m_Files[id];

                return null;
            }
            finally
            {
                m_FileLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Total files in the index - across all products.
        /// </summary>
        public long TotalFiles
        {
            get
            {
                if (m_Files == null)
                    cacheFileList();

                m_FileLock.EnterReadLock();

                try
                {
                    long totalStoredFiles = m_Files.Count;
                    return totalStoredFiles;
                }
                finally
                {
                    m_FileLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Loads the file list into the cache. 
        /// </summary>
        /// <returns>Null if already cached or product list.</returns>
        private StackHashFileCollection cacheFileList()
        {
            StackHashFileCollection files = null;

            m_FileLock.EnterWriteLock();

            try
            {
                // Check if the product list has been loaded yet.
                if (m_Files == null)
                {
                    m_Files = new Dictionary<int, ErrorIndexCacheFile>();

                    // Not yet loaded so load from the real database.
                    files = m_RealErrorIndex.LoadFileList(m_Product);

                    // Add them to the local cache.
                    foreach (StackHashFile file in files)
                    {
                        ErrorIndexCacheFile cacheProduct = new ErrorIndexCacheFile(m_RealErrorIndex, m_Product, file);
                        m_Files.Add(file.Id, cacheProduct);
                    }
                }

                return files;
            }
            finally
            {
                m_FileLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Refreshes the specified event data.
        /// </summary>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to refresh.</param>
        /// <returns>The refreshed event data.</returns>
        public StackHashEvent GetEvent(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            ErrorIndexCacheFile fileObject = getFileObject(file.Id);

            if (fileObject != null)
                return fileObject.GetEvent(theEvent);
            else
                return null;
        }

        /// <summary>
        /// Returns the cab with the specified ID.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cabId">ID of cab to get.</param>
        /// <returns>The cab or null if not found.</returns>
        public StackHashCab GetCab(StackHashFile file, StackHashEvent theEvent, int cabId)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            ErrorIndexCacheFile fileObject = getFileObject(file.Id);

            if (fileObject != null)
                return fileObject.GetCab(theEvent, cabId);
            else
                return null;
        }

        
        /// <summary>
        /// Retrieves the data associated with the specified file.
        /// </summary>
        /// <param name="fileId">ID if file to retrieve.</param>
        /// <returns>Retrieved file data.</returns>
        public StackHashFile GetFile(int fileId)
        {
            // Check if the product list has been loaded.
            if (m_Files == null)
                cacheFileList();

            m_FileLock.EnterReadLock();

            try
            {
                ErrorIndexCacheFile fileObject = getFileObject(fileId);

                if (fileObject != null)
                    return (StackHashFile)fileObject.File.Clone();
                else
                    return null;
            }
            finally
            {
                m_FileLock.ExitReadLock();
            }
        }
 
        /// <summary>
        /// Loads the files associated with this product.
        /// </summary>
        /// <returns>List of files.</returns>
        public StackHashFileCollection LoadFileList()
        {
            StackHashFileCollection files = null;

            // Loads and return the product list.
            if (m_Files == null)
            {
                // Not yet loaded so load from the real database.
                files = cacheFileList();
            }
            else
            {
                // Derive from the cache.
                files = new StackHashFileCollection();

                m_FileLock.EnterReadLock();

                try
                {
                    foreach (ErrorIndexCacheFile cacheFile in m_Files.Values)
                    {
                        files.Add((StackHashFile)cacheFile.File.Clone());
                    }
                }
                finally
                {
                    m_FileLock.ExitReadLock();
                }

            }

            return files;
        }


        /// <summary>
        /// Loads the event list for the specified file.
        /// </summary>
        /// <returns>List of events.</returns>
        public StackHashEventCollection LoadEventList(StackHashFile file)
        {
            // Make sure the file list is cached.
            if (m_Files == null)
            {
                // Not yet loaded so load from the real database.
                cacheFileList();
            }

            // Check that the file ID exists.
            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);

            if (fileObject == null)
                throw new ArgumentException("Unknown file ID", "file");

            // Load the event list for the product.
            StackHashEventCollection events = fileObject.LoadEventList();

            return events;
        }


        /// <summary>
        /// Loads the event info list for the specified event.
        /// </summary>
        /// <returns>List of events.</returns>
        public StackHashEventInfoCollection LoadEventInfoList(StackHashFile file, StackHashEvent theEvent)
        {
            // Make sure the file list is cached.
            if (m_Files == null)
            {
                // Not yet loaded so load from the real database.
                cacheFileList();
            }

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("Unknown file ID", "file");

            // Load the event info for the event.
            StackHashEventInfoCollection eventInfo = fileObject.LoadEventInfoList(theEvent);

            return eventInfo;
        }


        /// <summary>
        /// Gets the most recent hit date in the event info collection.
        /// </summary>
        /// <param name="file">File.</param>
        /// <param name="theEvent">The event for which the info is required.</param>
        /// <returns>The most recent hit date.</returns>
        public DateTime GetMostRecentHitDate(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (m_Files == null)
                cacheFileList();

            // Now check if the file exists.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File doesn't exist: " + file.Id, "file");

            return fileObject.GetMostRecentHitDate(theEvent);
        }

        
        /// <summary>
        /// Returns the number of cabs present for the specified file/event.
        /// </summary>
        /// <param name="file">File to find.</param>
        /// <param name="theEvent">The event to count the cabs for</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabCount(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (m_Files == null)
                cacheFileList();

            // Now check if the file exists.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File doesn't exist: " + file.Id, "file");

            return fileObject.GetCabCount(theEvent);
        }

        /// <summary>
        /// Parses all events associated with the specified product/file.
        /// </summary>
        /// <param name="file">File to parse the events for.</param>
        /// <param name="parser">Parsing callback details.</param>
        /// <returns>true - aborted, false otherwise.</returns>
        public bool ParseEvents(StackHashFile file, ErrorIndexEventParser parser)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (parser == null)
                throw new ArgumentNullException("parser");

            // Check if the product list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file exists.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject != null)
            {
                return fileObject.ParseEvents(parser);
            }
            else
            {
                return false;
            }
        }

        
        /// <summary>
        /// Check if the specified cab exists in the database.
        /// </summary>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cab">The cab to search for.</param>
        public bool CabExists(StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            // Check if the product list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File doesn't exist: " + file.Id, "file");

            return fileObject.CabExists(theEvent, cab);
        }

        /// <summary>
        /// Loads the cab list for the specified event.
        /// </summary>
        /// <returns>List of cabs.</returns>
        public StackHashCabCollection LoadCabList(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Make sure the file list is cached.
            if (m_Files == null)
            {
                // Not yet loaded so load from the real database.
                cacheFileList();
            }

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File doesn't exist: " + file.Id, "file");

            // Load the cab for the event.
            StackHashCabCollection cabs = fileObject.LoadCabList(theEvent);

            return cabs;
        }

        /// <summary>
        /// Check if the specified file exists in the database.
        /// </summary>
        /// <param name="product">Product to which the file belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        public bool FileExists(StackHashFile file)
        {
            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            if (file == null)
                throw new ArgumentNullException("file");

            // Now check if the file ID is present in the product list.
            m_FileLock.EnterReadLock();

            bool fileExists = false;
            try
            {
                fileExists = m_Files.ContainsKey(file.Id);
                return fileExists;
            }
            finally
            {
                m_FileLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Adds a file to the cache.
        /// If the file is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="file">File to add.</param>
        public void AddFile(StackHashFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (file.Id == 0)
                throw new ArgumentException("File object not initialised", "file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            m_FileLock.EnterWriteLock();

            try
            {
                // Now check if the file ID is present in the file list.
                if (m_Files.ContainsKey(file.Id))
                {
                    // Update the file information.
                    m_Files[file.Id].File = (StackHashFile)file.Clone();
                    m_RealErrorIndex.AddFile(m_Product, m_Files[file.Id].File);
                }
                else
                {
                    // Update the database and then the cache.
                    m_Files.Add(file.Id, new ErrorIndexCacheFile(m_RealErrorIndex, m_Product, (StackHashFile)file.Clone()));
                    m_RealErrorIndex.AddFile(m_Product, m_Files[file.Id].File);
                }
            }
            finally
            {
                m_FileLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Updates the statistics associated with a product. This includes fields like the 
        /// TotalStoredEvents. 
        /// This information is not provided by WinQual.
        /// </summary>
        /// <param name="product">The product to update.</param>
        /// <returns>The updated product.</returns>
        public StackHashProduct UpdateProductStatistics()
        {
            // Set the true number of events.
            return m_RealErrorIndex.UpdateProductStatistics(m_Product);
        }

   
         /// <summary>
        /// Adds an event associated with the specified file in a product.
        /// </summary>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields.</param>
        public void AddEvent(StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File not found: " + file.Id, "file");

            bool newEvent = !fileObject.EventExists(theEvent);

            // Let the file update the event.
            fileObject.AddEvent(theEvent, updateNonWinQualFields);

            if (newEvent)
            {
                m_Product.TotalStoredEvents++;
            }
        }

        /// <summary>
        /// Check if the specified event exists in the database.
        /// </summary>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        public bool EventExists(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File not found: " + file.Id, "file");

            return fileObject.EventExists(theEvent);
        }

        
        /// <summary>
        /// Adds event info associated with the specified file event.
        /// </summary>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="eventInfoCollection">The event collection to add.</param>
        public void AddEventInfoCollection(StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File not found: " + file.Id, "file");

            // Let the file update the event info.
            fileObject.AddEventInfoCollection(theEvent, eventInfoCollection);
        }

        /// <summary>
        /// Adds event info associated with the specified file event.
        /// </summary>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="eventInfoCollection">The event collection to add.</param>
        public void MergeEventInfoCollection(StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File not found: " + file.Id, "file");

            // Let the file update the event info.
            fileObject.MergeEventInfoCollection(theEvent, eventInfoCollection);
        }

        /// <summary>
        /// Adds cab info associated with the specified file event.
        /// </summary>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="cab">Cab data to add.</param>
        /// <param name="setDiagnosticInfo">True - set diagnostic data, False - don't.</param>
        public StackHashCab AddCab(StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            // Now check if the file ID is present in the file list.
            ErrorIndexCacheFile fileObject = getFileObject(file.Id);
            if (fileObject == null)
                throw new ArgumentException("File not found: " + file.Id, "file");

            // Let the file update the cab info.
            return fileObject.AddCab(theEvent, cab, setDiagnosticInfo);
        }

        /// <summary>
        /// Gets all event information associated with all files the product knows about.
        /// </summary>
        /// <returns>List of all events.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811")]
        public StackHashEventPackageCollection GetAllEvents(int filter)
        {
            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            if (filter != 0)
                throw new NotImplementedException();

            m_FileLock.EnterReadLock();
            try
            {
                foreach (ErrorIndexCacheFile file in m_Files.Values)
                {
                    StackHashEventPackageCollection eventPackages = m_Files[file.File.Id].GetAllEvents(0);

                    foreach (StackHashEventPackage eventPackage in eventPackages)
                    {
                        allEvents.Add(eventPackage);
                    }
                }

                return allEvents;
            }
            finally
            {
                m_FileLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Gets all event information for all files matching the specified search options.
        /// </summary>
        /// <param name="StackHashSearchCriteriaCollection">Characteristics to look for.</param>
        /// <param name="allEvents">allEvents - list to add to.</param>
        public void GetEvents(StackHashSearchCriteria stackHashSearchCriteria, StackHashEventPackageCollection allEvents)
        {
            if (stackHashSearchCriteria == null)
                throw new ArgumentNullException("stackHashSearchCriteria");

            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            m_FileLock.EnterReadLock();
            try
            {
                foreach (ErrorIndexCacheFile file in m_Files.Values)
                {
                    if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.File) ||
                        stackHashSearchCriteria.IsMatch(StackHashObjectType.File, file.File))
                    {
                        file.GetEvents(stackHashSearchCriteria, allEvents);
                    }
                }
            }
            finally
            {
                m_FileLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all events matching the specified file within the specified range.
        /// </summary>
        /// <param name="fileId">Id of the file.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetFileEvents(int fileId, long startRow, long numberOfRows)
        {
            // Check if the file list has been loaded.
            if (m_Files == null)
                cacheFileList();

            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            m_FileLock.EnterReadLock();
            try
            {
                foreach (ErrorIndexCacheFile file in m_Files.Values)
                {
                    if (file.File.Id == fileId)
                    {
                        allEvents = file.GetEvents(startRow, numberOfRows);
                        break;
                    }
                }

                return allEvents;
            }
            finally
            {
                m_FileLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Retrieves stats associated with the number of events etc...
        /// </summary>
        /// <returns>Database statistics.</returns>
        public StackHashSynchronizeStatistics Statistics
        {
            get
            {
                StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics();

                // Check if the product list has been loaded.
                if (m_Files == null)
                    cacheFileList();

                m_FileLock.EnterReadLock();
                try
                {
                    // Work through all the products and files.
                    foreach (ErrorIndexCacheFile file in m_Files.Values)
                    {
                        stats.Add(file.Statistics);
                    }

                    return stats;
                }
                finally
                {
                    m_FileLock.ExitReadLock();
                }
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_FileLock != null)
                    m_FileLock.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }



    /// <summary>
    /// The ErrorIndex cache contains a heirarchical view of all objects in the database.
    /// The cache is an in-memory version of the data in the ErrorIndex which aims to cut down 
    /// on file access. 
    /// For a large databases it may not be reasonable to enable the cache. It is therefore a 
    /// configurable parameter to the ErrorIndex.
    /// Note that if the cache is enabled then all changes to the database must also be reflected
    /// in the cache. e.g. adding a file or updating an event.
    /// </summary>
    public class ErrorIndexCache : IErrorIndex, IDisposable
    {
        private Dictionary<int, ErrorIndexCacheProduct> m_Products; // Indexed by product ID.
        private IErrorIndex m_RealErrorIndex;
        private bool m_IsActive;
        private bool m_UpdateTableActive;

        // Delegate to hear about changes to the event index.
        public event EventHandler<ErrorIndexEventArgs> IndexUpdated;
        public event EventHandler<ErrorIndexEventArgs> IndexUpdateAdded;
        public event EventHandler<ErrorIndexMoveEventArgs> IndexMoveProgress;

        ReaderWriterLockSlim m_ProductLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);


        public ErrorIndexType IndexType
        {
            get
            {
                return m_RealErrorIndex.IndexType;
            }
        }

        /// <summary>
        /// Determines if changes should be logged to the Update table.
        /// </summary>
        public bool UpdateTableActive
        {
            get
            {
                return m_UpdateTableActive;
            }
            set
            {
                m_UpdateTableActive = value;
            }
        }

        private ErrorIndexCacheProduct getProductObject(int id)
        {
            m_ProductLock.EnterReadLock();

            try
            {
                if (m_Products != null)
                    if (m_Products.ContainsKey(id))
                        return m_Products[id];

                return null;
            }
            finally
            {
                m_ProductLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines if the index is active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_IsActive;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Total events in the index - across all products.
        /// </summary>
        public long TotalStoredEvents
        {
            get
            {
                if (m_Products == null)
                    cacheProductList();

                m_ProductLock.EnterReadLock();

                try
                {
                    long totalStoredEvents = 0;
                    foreach (ErrorIndexCacheProduct cacheProduct in m_Products.Values)
                    {
                        totalStoredEvents += cacheProduct.Product.TotalStoredEvents;
                    }

                    return totalStoredEvents;
                }
                finally
                {
                    m_ProductLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Total products in the index.
        /// </summary>
        public long TotalProducts
        {
            get
            {
                if (m_Products == null)
                    cacheProductList();

                m_ProductLock.EnterReadLock();

                try
                {
                    if (m_Products != null)
                        return m_Products.Count;
                    else
                        return 0;
                }
                finally
                {
                    m_ProductLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Total files in the index - across all products.
        /// </summary>
        public long TotalFiles
        {
            get
            {
                if (m_Products == null)
                    cacheProductList();

                m_ProductLock.EnterReadLock();

                try
                {
                    long totalStoredFiles = 0;
                    foreach (ErrorIndexCacheProduct cacheProduct in m_Products.Values)
                    {
                        totalStoredFiles += cacheProduct.TotalFiles;
                    }

                    return totalStoredFiles;
                }
                finally
                {
                    m_ProductLock.ExitReadLock();
                }
            }
        }

        
        /// <summary>
        /// Name of the index.
        /// </summary>
        public String ErrorIndexName
        {
            get 
            {
                if (m_RealErrorIndex == null)
                    return String.Empty;
                else
                    return m_RealErrorIndex.ErrorIndexName;
            }
        }

        /// <summary>
        /// Path of the index.
        /// </summary>
        public String ErrorIndexPath
        {
            get
            {
                if (m_RealErrorIndex == null)
                    return String.Empty;
                else
                    return m_RealErrorIndex.ErrorIndexPath;
            }
        }

        /// <summary>
        /// Number of times that a sync has taken place since the last full resync.
        /// </summary>
        public int SyncCount
        {
            get
            {
                if (m_RealErrorIndex == null)
                    return 0;
                else
                    return m_RealErrorIndex.SyncCount;
            }

            set
            {
                if (m_RealErrorIndex != null)
                    m_RealErrorIndex.SyncCount = value;
            }
        }


        /// <summary>
        /// Indicates how far the previous sync got before completing.
        /// </summary>
        public StackHashSyncProgress SyncProgress
        {
            get
            {
                if (m_RealErrorIndex == null)
                    return new StackHashSyncProgress();
                else
                    return m_RealErrorIndex.SyncProgress;
            }

            set
            {
                if (m_RealErrorIndex != null)
                    m_RealErrorIndex.SyncProgress = value;
            }
        }

        
        public ErrorIndexStatus Status
        {
            get
            {
                if (m_RealErrorIndex == null)
                    return ErrorIndexStatus.Unknown;
                else
                    return m_RealErrorIndex.Status;
            }
        }

        /// <summary>
        /// Performs tests on the the database and cab folders.
        /// </summary>
        /// <returns></returns>
        public ErrorIndexConnectionTestResults GetDatabaseStatus()
        {
            if (m_RealErrorIndex == null)
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.Unknown, null);
            else
                return m_RealErrorIndex.GetDatabaseStatus();
        }

        
        /// <summary>
        /// Called when a change occurs to the underlying real index.
        /// Just reports to any upstream objects.
        /// </summary>
        /// <param name="source">Should be the real index.</param>
        /// <param name="e">Identifies the change.</param>
        private void ErrorIndexUpdated(Object source, ErrorIndexEventArgs e)
        {
            OnErrorIndexChanged(e);
        }

        /// <summary>
        /// Called when a change occurs to the underlying real index.
        /// Just reports to any upstream objects.
        /// </summary>
        /// <param name="source">Should be the real index.</param>
        /// <param name="e">Identifies the change.</param>
        private void ErrorIndexUpdateAdded(Object source, ErrorIndexEventArgs e)
        {
            OnErrorIndexUpdateAdded(e);
        }

        
        /// <summary>
        /// Notify upstream objects of a change to the error index.
        /// </summary>
        /// <param name="e">Identifies the change.</param>
        public void OnErrorIndexChanged(ErrorIndexEventArgs e)
        {
            EventHandler<ErrorIndexEventArgs> handler = IndexUpdated;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Notify upstream objects of a change to the error index Update Table.
        /// </summary>
        /// <param name="e">Identifies the change.</param>
        public void OnErrorIndexUpdateAdded(ErrorIndexEventArgs e)
        {
            EventHandler<ErrorIndexEventArgs> handler = IndexUpdateAdded;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Notify upstream objects of progress during an index move.
        /// </summary>
        /// <param name="e">Identifies the progress.</param>
        public void OnErrorIndexMoveProgress(ErrorIndexMoveEventArgs e)
        {
            EventHandler<ErrorIndexMoveEventArgs> handler = IndexMoveProgress;

            if (handler != null)
                handler(this, e);
        }

        
        /// <summary>
        /// Constructor for the cache. The realErrorIndex will be used to load data that is not
        /// currently in the cache.
        /// </summary>
        /// <param name="realErrorIndex">The base physical persistent database.</param>
        public ErrorIndexCache(IErrorIndex realErrorIndex)
        {
            if (realErrorIndex == null)
                throw new ArgumentNullException("realErrorIndex");

            m_RealErrorIndex = realErrorIndex;

            // Hook up to receive events.
            m_RealErrorIndex.IndexUpdated += new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdated);
            m_RealErrorIndex.IndexUpdateAdded += new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdateAdded);
        }


        /// <summary>
        /// Get the last time the product was synchronized successfully with WinQual.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last successful sync time.</returns>
        public DateTime GetLastSyncTimeLocal(int productId)
        {
            return m_RealErrorIndex.GetLastSyncTimeLocal(productId);
        }


        /// <summary>
        /// Sets the last time the product was synchronized following a successfully sync with WinQual.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastSyncTime">The last time the product was successfully synced (GMT).</param>
        public void SetLastSyncTimeLocal(int productId, DateTime lastSyncTime)
        {
            m_RealErrorIndex.SetLastSyncTimeLocal(productId, lastSyncTime);
        }

        /// <summary>
        /// Get the last time the product was synchronized successfully with WinQual.
        /// This is the time the sync completed for this product.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last successful sync complete time.</returns>
        public DateTime GetLastSyncCompletedTimeLocal(int productId)
        {
            return m_RealErrorIndex.GetLastSyncCompletedTimeLocal(productId);
        }

        /// <summary>
        /// Sets the last time the product was synchronized following a successfully sync with WinQual.
        /// This is the time it completed.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastSyncTime">The last time sync for the product was successfully completed.</param>
        public void SetLastSyncCompletedTimeLocal(int productId, DateTime lastSyncTime)
        {
            m_RealErrorIndex.SetLastSyncCompletedTimeLocal(productId, lastSyncTime);
        }

        /// <summary>
        /// Get the last time the product was synchronized with WinQual.
        /// This is the time the sync started for this product.
        /// Note that the sync may have failed.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last sync start time.</returns>
        public DateTime GetLastSyncStartedTimeLocal(int productId)
        {
            return m_RealErrorIndex.GetLastSyncStartedTimeLocal(productId);
        }

        /// <summary>
        /// Sets the last time the product sync started for the product.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastSyncTime">The last time sync for the product was started.</param>
        public void SetLastSyncStartedTimeLocal(int productId, DateTime lastSyncTime)
        {
            m_RealErrorIndex.SetLastSyncStartedTimeLocal(productId, lastSyncTime);
        }

        /// <summary>
        /// Get the most recent (last) hit time for the product.
        /// </summary>
        /// <param name="productId">The product to check.</param>
        /// <returns>Last hit time.</returns>
        public DateTime GetLastHitTimeLocal(int productId)
        {
            return m_RealErrorIndex.GetLastHitTimeLocal(productId);
        }

        /// <summary>
        /// Sets the most recent event info hit time for the specified product.
        /// </summary>
        /// <param name="productId">The product to set.</param>
        /// <param name="lastHitTime">The most recent Hit time for that product.</param>
        public void SetLastHitTimeLocal(int productId, DateTime lastHitTime)
        {
            m_RealErrorIndex.SetLastHitTimeLocal(productId, lastHitTime);
        }
        
        /// <summary>
        /// Loads the product list into the cache. 
        /// </summary>
        /// <returns>Null if already cached or product list.</returns>
        private StackHashProductCollection cacheProductList()
        {
            m_ProductLock.EnterWriteLock();

            try
            {
                StackHashProductCollection products = null;

                // Check if the product list has been loaded yet.
                if (m_Products == null)
                {
                    m_Products = new Dictionary<int, ErrorIndexCacheProduct>();

                    // Not yet loaded so load from the real database.
                    products = m_RealErrorIndex.LoadProductList();

                    // Add them to the local cache.
                    foreach (StackHashProduct product in products)
                    {
                        ErrorIndexCacheProduct cacheProduct = new ErrorIndexCacheProduct(m_RealErrorIndex, product);
                        m_Products.Add(product.Id, cacheProduct);
                    }
                }

                return products;
            }
            finally
            {
                m_ProductLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Returns a list of products.
        /// </summary>
        /// <returns></returns>
        public StackHashProductCollection LoadProductList()
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            StackHashProductCollection products = null;

            // Loads and return the product list.
            if (m_Products == null)
            {
                // Not yet loaded so load from the real database.
                products = cacheProductList();
            }
            else
            {
                m_ProductLock.EnterReadLock();

                try
                {
                    // Derive from the cache.
                    products = new StackHashProductCollection();

                    foreach (ErrorIndexCacheProduct cacheProduct in m_Products.Values)
                    {
                        products.Add((StackHashProduct)cacheProduct.Product.Clone());
                    }
                }
                finally
                {
                    m_ProductLock.ExitReadLock();
                }
            }

            return products;
        }


        /// <summary>
        /// Loads the file list for a particular product. 
        /// </summary>
        /// <param name="productId">Product whos file list is required.</param>
        /// <returns>List of files associated with the product.</returns>
        public StackHashFileCollection LoadFileList(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);

            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Load the file list for the product.
            StackHashFileCollection files = cacheProduct.LoadFileList();

            return files;
        }


        /// <summary>
        /// Loads the event list for the specified file.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public StackHashEventCollection LoadEventList(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Load the event list for the product.
            StackHashEventCollection events = cacheProduct.LoadEventList(file);

            return events;
        }


        /// <summary>
        /// Loads the event info for the specified product.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event for which the event info is required.</param>
        /// <returns>Loaded event info.</returns>
        public StackHashEventInfoCollection LoadEventInfoList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Load the event info for the event.
            StackHashEventInfoCollection events = cacheProduct.LoadEventInfoList(file, theEvent);

            return events;
        }


        /// <summary>
        /// Count the hits for the specified event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event for which the event info is required.</param>
        /// <returns>Loaded event info.</returns>
        public int GetHitCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");


            StackHashEventInfoCollection eventInfoList = LoadEventInfoList(product, file, theEvent);

            int totalHits = 0;
            foreach (StackHashEventInfo eventInfo in eventInfoList)
            {
                totalHits += eventInfo.TotalHits;
            }
            return totalHits;
        }

        
        /// <summary>
        /// Loads the cab info for the specified product event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event for which the cab info is required.</param>
        /// <returns>Loaded cab info.</returns>
        public StackHashCabCollection LoadCabList(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Load the event info for the event.
            StackHashCabCollection events = cacheProduct.LoadCabList(file, theEvent);

            return events;
        }

        /// <summary>
        /// Adds a product to the cache. Just updates the WinQual fields.
        /// If the product index is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="product">Product to add.</param>
        /// <returns>Updated product.</returns>
        public StackHashProduct AddProduct(StackHashProduct product)
        {
            return AddProduct(product, false);
        }

            
        /// <summary>
        /// Adds a product to the cache.
        /// If the product index is present already then the current entry is overwritten.
        /// The data is also persisted to the real database.
        /// </summary>
        /// <param name="product">Product to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields, False - update just WinQual fields.</param>
        /// <returns>Updated product.</returns>
        public StackHashProduct AddProduct(StackHashProduct product, bool updateNonWinQualFields)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            m_ProductLock.EnterWriteLock();

            try
            {
                // Now check if the product ID is present in the product list.
                if (m_Products.ContainsKey(product.Id))
                {
                    // Update the product information.
                    m_Products[product.Id].Product = m_RealErrorIndex.AddProduct(product, updateNonWinQualFields);
                }
                else
                {
                    // Update the database and then the cache.
                    product = m_RealErrorIndex.AddProduct(product, false);
                    m_Products.Add(product.Id, new ErrorIndexCacheProduct(m_RealErrorIndex, (StackHashProduct)product.Clone()));
                }

                return m_Products[product.Id].Product;
            }
            finally
            {
                m_ProductLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Adds a new file to the product file list and persists to store.
        /// If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="file"></param>
        public void AddFile(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
 
            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Let the product update the file.
            cacheProduct.AddFile(file);
        }


        /// <summary>
        /// Adds an event associated with the specified file in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields)
        {
            AddEvent(product, file, theEvent, updateNonWinQualFields, true);
        }

        /// <summary>
        /// Adds an event associated with the specified file in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to add.</param>
        /// <param name="updateNonWinQualFields">True - update all fields.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, bool updateNonWinQualFields, bool reportToBugTrackers)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " + product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Update this field in case it isn't set.
            if (theEvent.FileId == 0)
                theEvent.FileId = file.Id;

            // Let the product update the event.
            cacheProduct.AddEvent(file, theEvent, updateNonWinQualFields);
        }

        /// <summary>
        /// Adds an event associated with the specified file in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event to add.</param>
        public void AddEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            AddEvent(product, file, theEvent, false);
        }

        /// <summary>
        /// Adds event info associated with the specified file event in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="eventInfoCollection">The event info to add.</param>
        public void AddEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (eventInfoCollection == null)
                throw new ArgumentNullException("eventInfoCollection");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found" + product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Let the product update the event info.
            cacheProduct.AddEventInfoCollection(file, theEvent, eventInfoCollection);
        }


        /// <summary>
        /// Merges event info associated with the specified file event in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="eventInfoCollection">The event info to add.</param>
        public void MergeEventInfoCollection(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashEventInfoCollection eventInfoCollection)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (eventInfoCollection == null)
                throw new ArgumentNullException("eventInfoCollection");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found" + product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Let the product update the event info.
            cacheProduct.MergeEventInfoCollection(file, theEvent, eventInfoCollection);
        }


        /// <summary>
        /// Adds cab info associated with the specified file event in a product.
        /// </summary>
        /// <param name="product">The product owning the file.</param>
        /// <param name="file">The file owning the event.</param>
        /// <param name="theEvent">The event owning the event info.</param>
        /// <param name="cab">The cab to add.</param>
        /// <param name="setDiagnosticInfo">True - set diagnostic data. False - don't.</param>
        public StackHashCab AddCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Let the product update the event.
            return m_Products[product.Id].AddCab(file, theEvent, cab, setDiagnosticInfo);
        }

        /// <summary>
        /// Gets the folder where the CAB file will be stored.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to which the cab belongs.</param>
        /// <param name="theEvent">Event to which the cab belongs.</param>
        /// <param name="cab">The cab whose folder is required.</param>
        /// <returns></returns>
        public string GetCabFolder(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            return m_RealErrorIndex.GetCabFolder(product, file, theEvent, cab);
        }

        /// <summary>
        /// Gets the filename where the CAB file will be stored.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to which the cab belongs.</param>
        /// <param name="theEvent">Event to which the cab belongs.</param>
        /// <param name="cab">The cab whose filename is required.</param>
        /// <returns></returns>
        public string GetCabFileName(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            return m_RealErrorIndex.GetCabFileName(product, file, theEvent, cab);
        }

        /// <summary>
        /// Gets all event information associated with a particular product.
        /// </summary>
        /// <param name="product"></param>
        /// <returns>List of all events.</returns>
        public StackHashEventPackageCollection GetProductEvents(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            // Set up a suitable search criteria.
            StackHashSearchCriteria searchCriteria = new StackHashSearchCriteria();
            searchCriteria.SearchFieldOptions = new StackHashSearchOptionCollection();
            searchCriteria.SearchFieldOptions.Add(new IntSearchOption(StackHashObjectType.Product, "Id", StackHashSearchOptionType.Equal, product.Id, 0));
            
            StackHashEventPackageCollection eventPackages = new StackHashEventPackageCollection();
            cacheProduct.GetEvents(searchCriteria, eventPackages);

            return eventPackages;
        }


        /// <summary>
        /// Gets the number of events recorded against the listed products.
        /// This accounts for overlaps where events might be shared between files which are
        /// shared between products.
        /// </summary>
        /// <returns>Number of events.</returns>
        public long GetProductEventCount(Collection<int> products)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (products == null)
                throw new ArgumentNullException("products");

            int totalEvents = 0;

            Monitor.Enter(this);

            try
            {
                // Check if the product list has been loaded.
                if (m_Products == null)
                    cacheProductList();

                foreach (int productId in products)
                {
                    ErrorIndexCacheProduct cacheProduct = getProductObject(productId);
                    if (cacheProduct != null)
                    {
                        totalEvents += cacheProduct.Product.TotalStoredEvents;
                    }
                }

                return totalEvents;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// Gets all event information for all products matching the specified search options.
        /// </summary>
        /// <param name="searchCriteriaCollection">Characteristics to look for.</param>
        /// <returns>List of all events matching the specified search options.</returns>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection, StackHashProductSyncDataCollection enabledProducts)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            m_ProductLock.EnterReadLock();

            try
            {
                // Check for a product field match.
                foreach (ErrorIndexCacheProduct product in m_Products.Values)
                {
                    foreach (StackHashSearchCriteria stackHashSearchCriteria in searchCriteriaCollection)
                    {
                        if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.Product) ||
                            stackHashSearchCriteria.IsMatch(StackHashObjectType.Product, product.Product))
                        {
                            product.GetEvents(stackHashSearchCriteria, allEvents);
                        }
                    }

                }

                return allEvents;
            }
            finally
            {
                m_ProductLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all events matching the specified product and file.
        /// </summary>
        /// <param name="productId">Id of the product.</param>
        /// <param name="fileId">Id of the file.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <returns>List of matching events.</returns>
        public StackHashEventPackageCollection GetFileEvents(int productId, int fileId, long startRow, long numberOfRows)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            m_ProductLock.EnterReadLock();

            try
            {
                foreach (ErrorIndexCacheProduct product in m_Products.Values)
                {
                    if (product.Product.Id == productId)
                    {
                        allEvents = product.GetFileEvents(fileId, startRow, numberOfRows);
                        break;
                    }
                }

                return allEvents;
            }
            finally
            {
                m_ProductLock.ExitReadLock();
            }
        }
    

        /// <summary>
        /// Gets all event information for all products matching the specified search options.
        /// </summary>
        /// <param name="searchCriteriaCollection">Characteristics to look for.</param>
        /// <param name="startRow">The first row to get.</param>
        /// <param name="numberOfRows">Window size.</param>
        /// <param name="sortOrder">The order in which to sort the events returned.</param>
        /// <returns>List of all events matching the specified search options.</returns>
        public StackHashEventPackageCollection GetEvents(StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

            m_ProductLock.EnterReadLock();

            try
            {
                // Check for a product field match.
                foreach (ErrorIndexCacheProduct product in m_Products.Values)
                {
                    foreach (StackHashSearchCriteria stackHashSearchCriteria in searchCriteriaCollection)
                    {
                        if (stackHashSearchCriteria.IncludeAll(StackHashObjectType.Product) ||
                            stackHashSearchCriteria.IsMatch(StackHashObjectType.Product, product.Product))
                        {
                            if (allEvents.Count >= startRow + numberOfRows - 1)
                                break;

                            product.GetEvents(stackHashSearchCriteria, allEvents);
                        }
                    }

                }

                // Only return those events requested.
                if (startRow > allEvents.Count)
                    return new StackHashEventPackageCollection();

                StackHashEventPackageCollection eventWindow = new StackHashEventPackageCollection();

                int eventIndex = (int)startRow - 1;
                for (long startEvent = 0; startEvent < numberOfRows; startEvent++)
                {
                    if (eventIndex >= allEvents.Count)
                        break;
                    eventWindow.Add(allEvents[eventIndex]);
                    eventIndex++;
                }

                return eventWindow;
            }
            finally
            {
                m_ProductLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Parses all events associated with the specified product/file.
        /// </summary>
        /// <param name="product">The product to search.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="parser">Parser to report progress.</param>
        /// <returns>List of all events matching the specified search options.</returns>
        public bool ParseEvents(StackHashProduct product, StackHashFile file, ErrorIndexEventParser parser)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (parser == null)
                throw new ArgumentNullException("parser");


            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            if (cacheProduct != null)
            {
                return cacheProduct.ParseEvents(file, parser);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a note to a cab file.
        /// </summary>
        /// <param name="product">Product owning the cab.</param>
        /// <param name="file">File owning the cab.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">Cab to which the note is to be added.</param>
        /// <param name="note">Note to add to the cab.</param>
        public int AddCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, StackHashNoteEntry note)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");

            return m_RealErrorIndex.AddCabNote(product, file, theEvent, cab, note);
        }

        /// <summary>
        /// Deletes a note from a cab file.
        /// </summary>
        /// <param name="product">Product owning the cab.</param>
        /// <param name="file">File owning the cab.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">Cab to which the note is to be added.</param>
        /// <param name="note">Note to delete to the cab.</param>
        public void DeleteCabNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int noteId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            m_RealErrorIndex.DeleteCabNote(product, file, theEvent, cab, noteId);
        }

        
        /// <summary>
        /// Check if the specified cab exists in the database.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cab">The cab to search for.</param>
        public bool CabExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.CabExists(file, theEvent, cab);
        }


        /// <summary>
        /// Check if the specified cab FILE exists in the database.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cab">The cab to search for.</param>
        public bool CabFileExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            return m_RealErrorIndex.CabFileExists(product, file, theEvent, cab);
        }

        /// <summary>
        /// Gets all notes recorded against a cab file.
        /// </summary>
        /// <param name="product">Product owning the cab.</param>
        /// <param name="file">File owning the cab.</param>
        /// <param name="theEvent">Event owning the cab.</param>
        /// <param name="cab">The cab for which the notes are required.</param>
        /// <returns></returns>
        public StackHashNotes GetCabNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            return m_RealErrorIndex.GetCabNotes(product, file, theEvent, cab);
        }

        /// <summary>
        /// Gets the specified cab note.
        /// </summary>
        /// <param name="noteId">The cab entry required.</param>
        /// <returns>The requested cab note or null.</returns>
        public StackHashNoteEntry GetCabNote(int noteId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            return null;
        }

        
        /// <summary>
        /// Adds a note to an event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to add the note to.</param>
        /// <param name="note">The note to be added.</param>
        public int AddEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashNoteEntry note)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");

            return m_RealErrorIndex.AddEventNote(product, file, theEvent, note);
        }

        /// <summary>
        /// Delete a note to an event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to delete the note from.</param>
        /// <param name="note">The note to be delete.</param>
        public void DeleteEventNote(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int noteId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            m_RealErrorIndex.DeleteEventNote(product, file, theEvent, noteId);
        }

        
        /// <summary>
        /// Gets all notes recorded against an event.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to get the notes for.</param>
        /// <returns></returns>
        public StackHashNotes GetEventNotes(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            return m_RealErrorIndex.GetEventNotes(product, file, theEvent);
        }

        /// <summary>
        /// Gets the specified event note.
        /// </summary>
        /// <param name="noteId">The note entry required.</param>
        /// <returns>The requested event note or null.</returns>
        public StackHashNoteEntry GetEventNote(int noteId)
        {
            return null; 
        }

        
        /// <summary>
        /// Refreshes the specified event data.
        /// </summary>
        /// <param name="product">Product owning the event.</param>
        /// <param name="file">File owning the event.</param>
        /// <param name="theEvent">The event to refresh.</param>
        /// <returns>The refreshed event data.</returns>
        public StackHashEvent GetEvent(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct != null)
                return cacheProduct.GetEvent(file, theEvent);
            else
                return null;
        }

        
        /// <summary>
        /// Check if the specified file exists in the database.
        /// </summary>
        /// <param name="product">Product to which the file belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        public bool FileExists(StackHashProduct product, StackHashFile file)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.FileExists(file);
        }

        /// <summary>
        /// Check if the specified event exists in the database.
        /// </summary>
        /// <param name="product">Product to which the file belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        public bool EventExists(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.EventExists(file, theEvent);
        }


        /// <summary>
        /// Gets the most recent hit date in the event info collection.
        /// </summary>
        /// <param name="product">Product.</param>
        /// <param name="file">File.</param>
        /// <param name="theEvent">Event.</param>
        /// <returns>The most recent hit date.</returns>
        public DateTime GetMostRecentHitDate(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " + product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.GetMostRecentHitDate(file, theEvent);
        }

        
        /// <summary>
        /// Returns the number of cabs present for the specified product/file/event.
        /// </summary>
        /// <param name="product">Product to find.</param>
        /// <param name="file">File to find.</param>
        /// <param name="theEvent">The event to count the cabs for</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            // Now check if the product ID is present in the product list.
            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.GetCabCount(file, theEvent);
        }


        /// <summary>
        /// Returns the number of cab file present for the specified product/file/event.
        /// </summary>
        /// <param name="product">Product to find.</param>
        /// <param name="file">File to find.</param>
        /// <param name="theEvent">The event to count the cabs for</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabFileCount(StackHashProduct product, StackHashFile file, StackHashEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            return m_RealErrorIndex.GetCabFileCount(product, file, theEvent);
        }

        
        /// <summary>
        /// Check if the specified product exists in the database.
        /// </summary>
        /// <param name="product">Product to which the file belongs.</param>
        public bool ProductExists(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);

            return (cacheProduct != null);
        }


        /// <summary>
        /// Retrieves the data associated with the specified product ID.
        /// </summary>
        /// <param name="productId">ID if product to retrieve.</param>
        /// <returns>Retrieved product data.</returns>
        public StackHashProduct GetProduct(int productId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            m_ProductLock.EnterReadLock();

            try
            {
                ErrorIndexCacheProduct cacheProduct = getProductObject(productId);
                if (cacheProduct != null)
                    return (StackHashProduct)cacheProduct.Product.Clone();
                else
                    return null;
            }
            finally
            {
                m_ProductLock.ExitReadLock();
            }

        }


        /// <summary>
        /// Updates the statistics associated with a product. This includes fields like the 
        /// TotalStoredEvents. 
        /// This information is not provided by WinQual.
        /// </summary>
        /// <param name="product">The product to update.</param>
        /// <returns>The updated product.</returns>
        public StackHashProduct UpdateProductStatistics(StackHashProduct product)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            Monitor.Enter(this);

            try
            {
                // Check if the product list has been loaded.
                if (m_Products == null)
                    cacheProductList();

                ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
                if (cacheProduct != null)
                {
                    return cacheProduct.UpdateProductStatistics();
                }
                else
                {
                    return product;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Retrieves the data associated with the specified file.
        /// </summary>
        /// <param name="productId">product to retrieve.</param>
        /// <param name="fileId">ID if file to retrieve.</param>
        /// <returns>Retrieved file data.</returns>
        public StackHashFile GetFile(StackHashProduct product, int fileId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " +  product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.GetFile(fileId);
        }

        /// <summary>
        /// Returns the cab with the specified ID.
        /// </summary>
        /// <param name="product">Product to which the cab belongs.</param>
        /// <param name="file">File to find - only the id is used.</param>
        /// <param name="theEvent">The event to search for.</param>
        /// <param name="cabId">ID of cab to get.</param>
        /// <returns>The cab or null if not found.</returns>
        public StackHashCab GetCab(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, int cabId)
        {
            if (!m_IsActive)
                throw new InvalidOperationException("Index not accessible until activated");

            if (product == null)
                throw new ArgumentNullException("product");

            // Check if the product list has been loaded.
            if (m_Products == null)
                cacheProductList();

            ErrorIndexCacheProduct cacheProduct = getProductObject(product.Id);
            if (cacheProduct == null)
                throw new ArgumentException("Product not found " + product.Id.ToString(CultureInfo.InvariantCulture), "product");

            return cacheProduct.GetCab(file, theEvent, cabId);
        }


        /// <summary>
        /// Retrieves stats associated with the number of events etc...
        /// </summary>
        /// <returns>Database statistics.</returns>
        public StackHashSynchronizeStatistics Statistics
        {
            get
            {
                StackHashSynchronizeStatistics stats = new StackHashSynchronizeStatistics();

                // Check if the product list has been loaded.
                if (m_Products == null)
                    cacheProductList();

                m_ProductLock.EnterReadLock();

                try
                {
                    // Work through all the products and files.
                    foreach (ErrorIndexCacheProduct product in m_Products.Values)
                    {
                        stats.Add(product.Statistics);
                    }
                    return stats;
                }
                finally
                {
                    m_ProductLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets stats associated with the specified task type.
        /// Tasks are run on the data in the index. e.g. a Sync, Analyze, Purge etc...
        /// </summary>
        /// <param name="taskType">The task type whos stats is required.</param>
        /// <returns>Latest stored stats.</returns>
        public StackHashTaskStatus GetTaskStatistics(StackHashTaskType taskType)
        {
            return m_RealErrorIndex.GetTaskStatistics(taskType);
        }

        /// <summary>
        /// Sets stats associated with the specified task type.
        /// Tasks are run on the data in the index. e.g. a Sync, Analyze, Purge etc...
        /// </summary>
        /// <param name="taskStatus">The task status to set.</param>
        public void SetTaskStatistics(StackHashTaskStatus taskStatus)
        {
            m_RealErrorIndex.SetTaskStatistics(taskStatus);
        }

        
        public void AbortCurrentOperation()
        {
            m_RealErrorIndex.AbortCurrentOperation();
        }


        private void clearIndex()
        {
            m_ProductLock.EnterWriteLock();

            try
            {
                // Clear the products from memory. Note they might not have been cached yet.
                if (m_Products != null)
                    m_Products.Clear();
            }
            finally
            {
                m_ProductLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Renames the error index folder.
        /// </summary>
        /// <param name="newErrorIndexPath">Root folder for the index.</param>
        /// <param name="newErrorIndexName">Name and subfolder.</param>
        /// <param name="sqlSettings">Not used.</param>
        public void MoveIndex(String newErrorIndexPath, String newErrorIndexName, StackHashSqlConfiguration sqlSettings, bool allowPhysicalMove)
        {
            if (m_IsActive)
                throw new StackHashException("Index not accessible while activated", StackHashServiceErrorCode.ProfileActive);

            if (newErrorIndexPath == null)
                throw new ArgumentNullException("newErrorIndexPath");
            if (newErrorIndexName == null)
                throw new ArgumentNullException("newErrorIndexName");

            m_ProductLock.EnterWriteLock();

            try
            {
                m_RealErrorIndex.MoveIndex(newErrorIndexPath, newErrorIndexName, sqlSettings, allowPhysicalMove);
            }
            finally
            {
                m_ProductLock.ExitWriteLock();
            }
        }
    

        public void DeleteIndex()
        {
            if (m_IsActive)
                throw new StackHashException("Index not accessible while activated", StackHashServiceErrorCode.ProfileActive);

            m_ProductLock.EnterWriteLock();

            try
            {
                clearIndex();
                m_RealErrorIndex.DeleteIndex();
            }
            finally
            {
                m_ProductLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummary(int productId)
        {
            return new StackHashProductLocaleSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummary(int productId)
        {
            return new StackHashProductOperatingSystemSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummary(int productId)
        {
            return new StackHashProductHitDateSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaryFresh(int productId)
        {
            return new StackHashProductLocaleSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaryFresh(int productId)
        {
            return new StackHashProductOperatingSystemSummaryCollection();
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// The database is parsed afresh rather than relying on the stats summary tables.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaryFresh(int productId)
        {
            return new StackHashProductHitDateSummaryCollection();
        }

        /// <summary>
        /// Loads the settings and prepares for use.
        /// </summary>
        public void Activate()
        {
            m_RealErrorIndex.Activate();
            m_IsActive = true;
        }

        /// <summary>
        /// Creates the index if necessary or initializes an existing one.
        /// Set allowIndexCreation for test mode only.
        /// </summary>
        /// <param name="allowIndexCreation">True - create the index if it doesn't exist, False - don't create.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void Activate(bool allowIndexCreation, bool createIndexInDefaultLocation)
        {
            m_RealErrorIndex.Activate(allowIndexCreation, createIndexInDefaultLocation);
            m_IsActive = true;
        }


        /// <summary>
        /// Unloads. 
        /// </summary>
        public void Deactivate()
        {
            m_RealErrorIndex.Deactivate();
            m_IsActive = false;
        }

        #region LocaleSummaryMethods

        /// <summary>
        /// Determines if a locale summary exists.
        /// </summary>
        /// <param name="localeId">ID of the locale to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool LocaleSummaryExists(int productId, int localeId)
        {
            return m_RealErrorIndex.LocaleSummaryExists(productId, localeId);
        }


        /// <summary>
        /// Gets all of the locale rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaries(int productId)
        {
            return m_RealErrorIndex.GetLocaleSummaries(productId);
        }

        /// <summary>
        /// Gets a specific locale summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductLocaleSummary GetLocaleSummaryForProduct(int productId, int localeId)
        {
            return m_RealErrorIndex.GetLocaleSummaryForProduct(productId, localeId);
        }

        /// <summary>
        /// Adds a locale summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose local data is to be updated.</param>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        public void AddLocaleSummary(int productId, int localeId, long totalHits, bool overwrite)
        {
            m_RealErrorIndex.AddLocaleSummary(productId, localeId, totalHits, overwrite);
        }

        #endregion LocaleSummaryMethods


        #region OperatingSystemSummaryMethods

        /// <summary>
        /// Determines if a OS summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="operatingSystemName">Name of the OS.</param>
        /// <param name="operatingSystemVersion">OS Version.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool OperatingSystemSummaryExists(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            return m_RealErrorIndex.OperatingSystemSummaryExists(productId, operatingSystemName, operatingSystemVersion);
        }


        /// <summary>
        /// Gets all of the OS rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaries(int productId)
        {
            return m_RealErrorIndex.GetOperatingSystemSummaries(productId);
        }


        /// <summary>
        /// Gets a specific OS summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductOperatingSystemSummary GetOperatingSystemSummaryForProduct(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            return m_RealErrorIndex.GetOperatingSystemSummaryForProduct(productId, operatingSystemName, operatingSystemVersion);
        }


        /// <summary>
        /// Adds a OS summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="operatingSystemId">ID of the OS</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        public void AddOperatingSystemSummary(int productId, short operatingSystemId, long totalHits, bool overwrite)
        {
            m_RealErrorIndex.AddOperatingSystemSummary(productId, operatingSystemId, totalHits, overwrite);
        }

        #endregion OperatingSystemSummaryMethods
        
        
        #region LocaleMethods

        /// <summary>
        /// Adds a locale to the database.
        /// </summary>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="localeCode">Locale code.</param>
        /// <param name="localeName">Locale name.</param>
        public void AddLocale(int localeId, String localeCode, String localeName)
        {
            m_RealErrorIndex.AddLocale(localeId, localeCode, localeName);
        }

        #endregion LocaleMethods

        #region OperatingSystemMethods

        /// <summary>
        /// Gets the OS type ID with the specified name.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        /// <returns>ID of the OS entry.</returns>
        public short GetOperatingSystemId(String operatingSystemName, String operatingSystemVersion)
        {
            return m_RealErrorIndex.GetOperatingSystemId(operatingSystemName, operatingSystemVersion);
        }

        /// <summary>
        /// Adds an operating system.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        public void AddOperatingSystem(String operatingSystemName, String operatingSystemVersion)
        {
            m_RealErrorIndex.AddOperatingSystem(operatingSystemName, operatingSystemVersion);
        }

        #endregion OperatingSystemMethods

        #region HitDateSummaryMethods

        /// <summary>
        /// Determines if a HitDate summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <returns>True - is present. False - not present.</returns>
        public bool HitDateSummaryExists(int productId, DateTime hitDateLocal)
        {
            return m_RealErrorIndex.HitDateSummaryExists(productId, hitDateLocal);
        }


        /// <summary>
        /// Gets all of the HitDate  rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaries(int productId)
        {
            return m_RealErrorIndex.GetHitDateSummaries(productId);
        }


        /// <summary>
        /// Gets a specific HitDate summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="hitDateLocal">Hit date to get.</param>
        /// <returns>Product rollup information.</returns>
        public StackHashProductHitDateSummary GetHitDateSummaryForProduct(int productId, DateTime hitDateLocal)
        {
            return m_RealErrorIndex.GetHitDateSummaryForProduct(productId, hitDateLocal);
        }


        /// <summary>
        /// Adds a HitDate summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <param name="totalHits">Running total of all hits for this hit date.</param>
        public void AddHitDateSummary(int productId, DateTime hitDateLocal, long totalHits, bool overwrite)
        {
            m_RealErrorIndex.AddHitDateSummary(productId, hitDateLocal, totalHits, overwrite);
        }

        #endregion HitDateSummaryMethods

        #region UpdateTableMethods;


        /// <summary>
        /// Gets the first entry in the Update Table belonging to this profile.
        /// </summary>
        /// <returns>The update located - or null if no update entry exists.</returns>
        public StackHashBugTrackerUpdate GetFirstUpdate()
        {
            return m_RealErrorIndex.GetFirstUpdate();
        }


        /// <summary>
        /// Adds a new update entry to the Update Table.
        /// Updates indicate changes that have occurred to objects in other tables.
        /// This table exists to feed the bug tracker plugins changes that have occurred
        /// to the database.
        /// Entries are normally added by the WinQualSync task and when notes are added.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void AddUpdate(StackHashBugTrackerUpdate update)
        {
            m_RealErrorIndex.AddUpdate(update);
        }


        /// <summary>
        /// Clear all elements in the update table.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void ClearAllUpdates()
        {
            m_RealErrorIndex.ClearAllUpdates();
        }

        /// <summary>
        /// Removes the specified entry from the update table.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void RemoveUpdate(StackHashBugTrackerUpdate update)
        {
            m_RealErrorIndex.RemoveUpdate(update);
        }


        #endregion

        #region MappingTableMethods

        /// <summary>
        /// Gets the mappings of a particular type.
        /// </summary>
        /// <returns>Collection of mappings.</returns>
        public StackHashMappingCollection GetMappings(StackHashMappingType mappingType)
        {
            return m_RealErrorIndex.GetMappings(mappingType);
        }

        /// <summary>
        /// Adds the specified mappings. If they exist already they will be overwritten.
        /// </summary>
        /// <returns>Collection of mappings.</returns>
        public void AddMappings(StackHashMappingCollection mappings)
        {
            m_RealErrorIndex.AddMappings(mappings);
        }

        #endregion

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Hook up to receive events.
                m_RealErrorIndex.IndexUpdated -= new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdated);
                m_RealErrorIndex.IndexUpdateAdded -= new EventHandler<ErrorIndexEventArgs>(this.ErrorIndexUpdateAdded);
                m_RealErrorIndex.Dispose();
                if (m_ProductLock != null)
                    m_ProductLock.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
