using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashEventInfo : ICloneable, IComparable
    {
        private const int s_ThisStructureVersion = 1;
        private int m_StructureVersion;

        private DateTime m_DateCreatedLocal;
        private DateTime m_DateModifiedLocal;
        private DateTime m_HitDateLocal;
        private string m_Language;
        private int m_Lcid;
        private string m_Locale;
        private string m_OperatingSystemName;
        private string m_OperatingSystemVersion;
        private int m_TotalHits;


        public StackHashEventInfo() {;}  // Needed to serialize;

        public StackHashEventInfo(DateTime dateCreatedLocal,
                                  DateTime dateModifiedLocal,
                                  DateTime hitDateLocal,
                                  string language,
                                  int lcid,
                                  string locale,
                                  string operatingSystemName,
                                  string operatingSystemVersion,
                                  int totalHits)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_HitDateLocal = hitDateLocal;
            m_Language = language;
            m_Lcid = lcid;
            m_Locale = locale;
            m_OperatingSystemName = operatingSystemName;
            m_OperatingSystemVersion = operatingSystemVersion;
            m_TotalHits = totalHits;
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
        public DateTime HitDateLocal
        {
            get { return m_HitDateLocal; }
            set { m_HitDateLocal = value; }
        }

        [DataMember]
        public string Language
        {
            get { return m_Language; }
            set { m_Language = value; }
        }

        [DataMember]
        public int Lcid
        {
            get { return m_Lcid; }
            set { m_Lcid = value; }
        }

        [DataMember]
        public string Locale
        {
            get { return m_Locale; }
            set { m_Locale = value; }
        }

        [DataMember]
        public string OperatingSystemName
        {
            get { return m_OperatingSystemName; }
            set { m_OperatingSystemName = value; }
        }

        [DataMember]
        public string OperatingSystemVersion
        {
            get { return m_OperatingSystemVersion; }
            set { m_OperatingSystemVersion = value; }
        }

        [DataMember]
        public int TotalHits
        {
            get { return m_TotalHits; }
            set { m_TotalHits = value; }
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


        
        #region ICloneable Members

        /// <summary>
        /// Clones a copy of the event info.
        /// </summary>
        /// <returns>Cloned copy of EventInfo.</returns>
        public object Clone()
        {
            StackHashEventInfo eventInfo = new StackHashEventInfo(m_DateCreatedLocal, m_DateModifiedLocal, 
                m_HitDateLocal, m_Language, m_Lcid, m_Locale, m_OperatingSystemName, m_OperatingSystemVersion, 
                m_TotalHits);
            return eventInfo;
        }

        #endregion

        public bool IsMatchingEventInfo(StackHashEventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            if (this.m_HitDateLocal != eventInfo.HitDateLocal)
                return false;

            if (this.m_Lcid != eventInfo.Lcid)
                return false;
            //if (String.Compare(this.m_Locale, eventInfo.Locale, StringComparison.OrdinalIgnoreCase) != 0)
            //    return false;
            //if (String.Compare(this.m_Language, eventInfo.Language, StringComparison.OrdinalIgnoreCase) != 0) 
            //    return false;
            if (String.Compare(this.m_OperatingSystemName, eventInfo.OperatingSystemName, StringComparison.OrdinalIgnoreCase) != 0)
                return false;
            if (String.Compare(this.m_OperatingSystemVersion, eventInfo.m_OperatingSystemVersion, StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            return true;
        }
        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashEventInfo eventInfo = obj as StackHashEventInfo;

            if ((this.m_DateCreatedLocal == eventInfo.DateCreatedLocal) &&
                (this.m_DateModifiedLocal == eventInfo.DateModifiedLocal) &&
                (this.m_HitDateLocal == eventInfo.HitDateLocal) &&
                (this.m_Language == eventInfo.Language) &&
                (this.m_Lcid == eventInfo.Lcid) &&
                (this.m_Locale == eventInfo.m_Locale) &&
                (this.m_OperatingSystemName == eventInfo.OperatingSystemName) &&
                (this.m_OperatingSystemVersion == eventInfo.m_OperatingSystemVersion) &&
                (this.m_TotalHits == eventInfo.TotalHits))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        
        }

        #endregion


        public void SetWinQualFields(StackHashEventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            m_DateCreatedLocal = eventInfo.DateCreatedLocal;
            m_DateModifiedLocal = eventInfo.DateModifiedLocal;
            m_HitDateLocal = eventInfo.HitDateLocal;
            m_Language = eventInfo.Language;
            m_Lcid = eventInfo.Lcid;
            m_Locale = eventInfo.Locale;
            m_OperatingSystemName = eventInfo.OperatingSystemName;
            m_OperatingSystemVersion = eventInfo.OperatingSystemVersion;
            m_TotalHits = eventInfo.TotalHits;
            m_StructureVersion = eventInfo.StructureVersion;
        }

        public override string ToString()
        {
            StringBuilder outString = new StringBuilder("EventInfo: DateCreated:");
            outString.Append(m_DateCreatedLocal);
            outString.Append(" DateModified:");
            outString.Append(m_DateModifiedLocal);
            outString.Append(" HitDate:");
            outString.Append(m_HitDateLocal);
            outString.Append(" Language:");
            if (!String.IsNullOrEmpty(m_Language))
                outString.Append(m_Language);
            outString.Append(" Lcid:");
            outString.Append(m_Lcid);
            outString.Append(" Locale:");
            if (!String.IsNullOrEmpty(m_Locale))
                outString.Append(m_Locale);
            outString.Append(" OS:");
            if (!String.IsNullOrEmpty(m_OperatingSystemName))
                outString.Append(m_OperatingSystemName);
            outString.Append(" OSVersion:");
            if (!String.IsNullOrEmpty(m_OperatingSystemVersion))
                outString.Append(m_OperatingSystemVersion);
            outString.Append(" TotalHits:");
            outString.Append(m_TotalHits);
            return outString.ToString();
        }



        /// <summary>
        /// There are 4 WinQual mirror sites.
        ///
        /// US - Americas.winqual.microsoft.com (131.107.97.31) – (UTC minus 7)
        /// This always returns a time of 07:00:00 UTC for hit dates. Date = say 25-Oct-2010
        ///
        /// UK - Europe.winqual.microsoft.com (94.245.126.63) (UTC plus 1)
        /// This always returns a time of 23:00:00 UTC for hit dates. Date = say 24-Oct-2010
        ///
        /// Japan - Asia.winqual.microsoft.com (207.46.84.113) (UTC plus 8)
        /// This always returns a time of 15:00:00 UTC for hit dates. Date = say 24-Oct-2010
        ///
        /// Perth Australia - ?.winqual.microsoft.com (207.46.52.39) (UTC plus 9)
        /// This always returns a time of 16:00:00 UTC for hit dates. Date = say 24-Oct-2010
        ///
        /// When I make a request to https:\\winqual.microsoft.com I get a different resolved IP address each time (Verified by continuous pinging - I suspect this is the load balancing in action). i.e. sometimes I get directed to Japan, other times to US etc...
        /// This results in one of the different times / dates that I see for the same EventInfo.
        ///
        /// The reason for the difference in date and time is, I believe, as follows.
        ///
        /// The code on the winqual server gets the hit date (STORED AS DATE ONLY) from the database – and uses it to create a DateTime object which will default to LOCAL TIME. The date will therefore be something like “25-Oct-2010 00:00:00 Local”. 
        /// When sending in the Atom Feed, it needs to turn it into UTC for transmission. This Local to UTC conversion will erroneously change the date and time.
        ///
        /// e.g. 
        /// The US server will take the 25-Oct-2010 00:00:00 Local and convert to 25-Oct-2010 07:00:00 UTC (plus 7 hours – date stays the same).
        /// The UK server will take the 25-Oct-2010 00:00:00 Local and convert to 24-Oct-2010 23:00:00 UTC (minus 1 hour – causes date change).
        /// The Australian server will take the 25-Oct-2010 00:00:00 Local and convert to 24-Oct-2010 15:00:00 UTC (minus 9 hours – causes date change).
        /// The Japanese server will take the 25-Oct-2010 00:00:00 Local and convert to 24-Oct-2010 16:00:00 UTC (minus 8 hours – causes date change).
        ///
        /// This is exactly what I am seeing. If I ALWAYS connected to the same server, then this wouldn’t be a problem. However, because I end up connecting to different servers depending on load and possibly location, then I get different data back from each server resulting in what appears to me as different EventInfo’s.
        ///
        /// WORKAROUND.
        /// If this is the issue, then I can work around the issue by converting affected dates as follows.
        ///
        /// If (Time part of the date >= 12:00:00)
        ///     Increment the date.
        ///     Set the time part to 00:00:00
        /// Else
        ///     Set the time part to 00:00:00  // The date should be correct.
        ///
        /// This effectively unwinds the Local to UTC conversion that the server is applying to give me the correct date.
        ///
        /// </summary>
        /// <returns>Normalized copy of the event info.</returns>
        public StackHashEventInfo Normalize()
        {
            // Make a copy of the event info before changing anything.
            StackHashEventInfo normalizedEventInfo = (StackHashEventInfo)this.Clone();

            normalizedEventInfo.HitDateLocal = NormalizeDate(m_HitDateLocal);

            return normalizedEventInfo;
        }

        public static DateTime NormalizeDate(DateTime originalDateTime)
        {
            // The hit date is changed according to the comment above.
            DateTime newHitDateTime;

            // Make sure the date is represented as UTC.
            if (originalDateTime.Kind != DateTimeKind.Utc)
                newHitDateTime = originalDateTime.ToUniversalTime();
            else
                newHitDateTime = originalDateTime;

            if (newHitDateTime.Hour >= 12)
                newHitDateTime = newHitDateTime.AddDays(1);

            newHitDateTime = new DateTime(newHitDateTime.Year, newHitDateTime.Month, newHitDateTime.Day, 0, 0, 0, DateTimeKind.Utc);

            return newHitDateTime;
        }

    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashEventInfoCollection : Collection<StackHashEventInfo>, IComparable, ICloneable
    {
        public StackHashEventInfoCollection() { } // Needed to serialize.

        /// <summary>
        /// Locates the eventInfo with all fields matching.
        /// </summary>
        /// <param name="eventInfo">Full event fields to match.</param>
        /// <returns>Found event info or null.</returns>
        public StackHashEventInfo FindEventInfo(StackHashEventInfo eventInfo)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].IsMatchingEventInfo(eventInfo))
                    return this[i];
            }

            return null;
        }

        /// <summary>
        /// Counts the matching eventInfos with all fields matching.
        /// </summary>
        /// <param name="eventInfo">Full event fields to match.</param>
        /// <returns>Found event info or null.</returns>
        public int GetEventInfoMatchCount(StackHashEventInfo eventInfo)
        {
            int numMatches = 0;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].IsMatchingEventInfo(eventInfo))
                    numMatches++;
            }

            return numMatches;
        }

        /// <summary>
        /// Locates the eventInfo with all fields matching.
        /// </summary>
        /// <param name="eventInfo">Full event fields to match.</param>
        /// <returns>Found event info or null.</returns>
        public StackHashEventInfo FindEventInfoByHitDate(StackHashEventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");


            for (int i = 0; i < this.Count; i++)
            {
                // Only the date part need match.
                if (this[i].HitDateLocal.Date == eventInfo.HitDateLocal.Date)
                   return this[i];
            }

            return null;
        }


        public StackHashEventInfoCollection Normalize()
        {
            StackHashEventInfoCollection newEventInfos = new StackHashEventInfoCollection();

            foreach (StackHashEventInfo eventInfo in this)
            {
                StackHashEventInfo newEventInfo = eventInfo.Normalize();

                StackHashEventInfo foundEventInfo = newEventInfos.FindEventInfo(newEventInfo);

                if (foundEventInfo == null)
                    newEventInfos.Add(newEventInfo);
                else
                    foundEventInfo.TotalHits += newEventInfo.TotalHits;
            }

            return newEventInfos;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            StackHashEventInfoCollection events = obj as StackHashEventInfoCollection;

            if (events.Count != this.Count)
                return -1;

            foreach (StackHashEventInfo thisEventInfo in this)
            {
                StackHashEventInfo matchingEventInfo = events.FindEventInfo(thisEventInfo);

                if (matchingEventInfo == null)
                    return -1;
            }

            // Must be a match.
            return 0;
        }

        #endregion 
    
        #region ICloneable Members

        public object Clone()
        {
            StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();

            foreach (StackHashEventInfo eventInfo in this)
            {
                eventInfoCollection.Add((StackHashEventInfo)eventInfo.Clone());
            }

            return eventInfoCollection;
        }

        #endregion
    }



    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashEventInfoPackage
    {
        private StackHashEventInfo m_EventInfo;
        private int m_EventId;
        private String m_EventTypeName;


        public StackHashEventInfoPackage() { ;}  // Needed to serialize;

        public StackHashEventInfoPackage(int eventId, String eventTypeName, StackHashEventInfo eventInfo)
        {
            m_EventInfo = eventInfo;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
        }


        [DataMember]
        public StackHashEventInfo EventInfo
        {
            get { return m_EventInfo; }
            set { m_EventInfo = value; }
        }

        [DataMember]
        public int EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }

        [DataMember]
        public String EventTypeName
        {
            get { return m_EventTypeName; }
            set { m_EventTypeName = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashEventInfoPackageCollection : Collection<StackHashEventInfoPackage>
    {
        public StackHashEventInfoPackageCollection() { } // Needed to serialize.
    }
}
