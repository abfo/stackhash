using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProductLocaleSummary : IComparable<StackHashProductLocaleSummary>
    {
        private String m_Language;
        private int m_Lcid;
        private String m_Locale;
        private long m_TotalHits;

        public StackHashProductLocaleSummary() { ;}

        public StackHashProductLocaleSummary(String language, int lcid, String locale, long totalHits)
        {
            m_Language = language;
            m_Lcid = lcid;
            m_Locale = locale;
            m_TotalHits = totalHits;
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
        public long TotalHits
        {
            get { return m_TotalHits; }
            set { m_TotalHits = value; }
        }

        #region IComparable<StackHashProductLocaleSummary> Members

        public int CompareTo(StackHashProductLocaleSummary other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (this.Lcid != other.Lcid)
                return -1;
            if (this.Language != other.Language)
                return -1;
            if (this.Locale != other.Locale)
                return -1;
            if (this.TotalHits != other.TotalHits)
                return -1;

            return 0;
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashProductLocaleSummaryCollection : Collection<StackHashProductLocaleSummary>, IComparable<StackHashProductLocaleSummaryCollection>
    {
        public StackHashProductLocaleSummaryCollection() { } // Needed to serialize.

        public StackHashProductLocaleSummary FindLocale(int localeId)
        {
            foreach (StackHashProductLocaleSummary localeSummary in this)
            {
                if (localeSummary.Lcid == localeId)
                    return localeSummary;
            }

            return null;
        }

    
        #region IComparable<StackHashProductLocaleSummaryCollection> Members

        public int CompareTo(StackHashProductLocaleSummaryCollection other)
        {
            if (other == null)
                return -1;
            if (other.Count != this.Count)
                return -1;

            foreach (StackHashProductLocaleSummary localeSummary in this)
            {
                // Find matching locale in other.
                StackHashProductLocaleSummary matchingLocaleSummary = other.FindLocale(localeSummary.Lcid);
                if (matchingLocaleSummary == null)
                    return -1;

                if (localeSummary.CompareTo(matchingLocaleSummary) != 0)
                    return -1;
            }

            return 0;
        }

        #endregion
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProductOperatingSystemSummary : IComparable<StackHashProductOperatingSystemSummary>
    {
        private String m_OperatingSystemName;
        private String m_OperatingSystemVersion;
        private long m_TotalHits;

        public StackHashProductOperatingSystemSummary() { ;}

        public StackHashProductOperatingSystemSummary(String operatingSystemName, String operatingSystemVersion, long totalHits)
        {
            m_OperatingSystemName = operatingSystemName;
            m_OperatingSystemVersion = operatingSystemVersion;
            m_TotalHits = totalHits;
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
        public long TotalHits
        {
            get { return m_TotalHits; }
            set { m_TotalHits = value; }
        }

        #region IComparable<StackHashProductOperatingSystemSummary> Members

        public int CompareTo(StackHashProductOperatingSystemSummary other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (String.Compare(this.OperatingSystemName, other.OperatingSystemName, StringComparison.OrdinalIgnoreCase) != 0)
                return -1;
            if (String.Compare(this.OperatingSystemVersion, other.OperatingSystemVersion, StringComparison.OrdinalIgnoreCase) != 0)
                return -1;
            if (this.TotalHits != other.TotalHits)
                return -1;
            return 0;
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashProductOperatingSystemSummaryCollection : Collection<StackHashProductOperatingSystemSummary>, IComparable<StackHashProductOperatingSystemSummaryCollection>
    {
        public StackHashProductOperatingSystemSummaryCollection() { } // Needed to serialize.

        public StackHashProductOperatingSystemSummary FindOperatingSystem(String operatingSystemName, String operatingSystemVersion)
        {
            foreach (StackHashProductOperatingSystemSummary operatingSystemSummary in this)
            {
                if (String.Compare(operatingSystemSummary.OperatingSystemName, operatingSystemName, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                if (String.Compare(operatingSystemSummary.OperatingSystemVersion, operatingSystemVersion, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                return operatingSystemSummary;
            }

            return null;
        }

        #region IComparable<StackHashProductOperatingSystemSummaryCollection> Members

        public int CompareTo(StackHashProductOperatingSystemSummaryCollection other)
        {
            if (other == null)
                return -1;
            if (other.Count != this.Count)
                return -1;

            foreach (StackHashProductOperatingSystemSummary operatingSystemSummary in this)
            {
                // Find matching in other.
                StackHashProductOperatingSystemSummary matchingSummary = 
                    other.FindOperatingSystem(operatingSystemSummary.OperatingSystemName, operatingSystemSummary.OperatingSystemVersion);
                if (matchingSummary == null)
                    return -1;

                if (operatingSystemSummary.CompareTo(matchingSummary) != 0)
                    return -1;
            }

            return 0;
        }

        #endregion
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProductHitDateSummary : IComparable<StackHashProductHitDateSummary>
    {
        private DateTime m_HitDate;
        private long m_TotalHits;

        public StackHashProductHitDateSummary() { ;}

        public StackHashProductHitDateSummary(DateTime hitDate, long totalHits)
        {
            m_HitDate = hitDate;
            m_TotalHits = totalHits;
        }

        [DataMember]
        public DateTime HitDate
        {
            get { return m_HitDate; }
            set { m_HitDate = value; }
        }

        [DataMember]
        public long TotalHits
        {
            get { return m_TotalHits; }
            set { m_TotalHits = value; }
        }

        #region IComparable<StackHashProductHitDateSummary> Members

        public int CompareTo(StackHashProductHitDateSummary other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (this.HitDate.ToUniversalTime() != other.HitDate.ToUniversalTime())
                return -1;
            if (this.TotalHits != other.TotalHits)
                return -1;

            return 0;
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashProductHitDateSummaryCollection : Collection<StackHashProductHitDateSummary>, IComparable<StackHashProductHitDateSummaryCollection>
    {
        public StackHashProductHitDateSummaryCollection() { } // Needed to serialize.

        public StackHashProductHitDateSummary FindHitDate(DateTime hitDate)
        {
            DateTime hitDateUtc = hitDate.ToUniversalTime();

            foreach (StackHashProductHitDateSummary hitDateSummary in this)
            {
                DateTime thisHitDateUtc = hitDateSummary.HitDate.ToUniversalTime();
                if (thisHitDateUtc == hitDateUtc)
                    return hitDateSummary;
            }
            return null;
        }

        #region IComparable<StackHashProductHitDateSummaryCollection> Members

        public int CompareTo(StackHashProductHitDateSummaryCollection other)
        {
            if (other == null)
                return -1;
            if (other.Count != this.Count)
                return -1;

            foreach (StackHashProductHitDateSummary hitDateSummary in this)
            {
                // Find matching in other.
                StackHashProductHitDateSummary matchingSummary = other.FindHitDate(hitDateSummary.HitDate);
                if (matchingSummary == null)
                    return -1;

                if (hitDateSummary.CompareTo(matchingSummary) != 0)
                    return -1;
            }

            return 0;
        }

        #endregion
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashProductSummary
    {
        private StackHashProductLocaleSummaryCollection m_LocaleSummaryCollection;
        private StackHashProductOperatingSystemSummaryCollection m_OperatingSystemSummaryCollection;
        private StackHashProductHitDateSummaryCollection m_HitDateSummaryCollection;

        public StackHashProductSummary() 
        {
            m_LocaleSummaryCollection = new StackHashProductLocaleSummaryCollection();
            m_OperatingSystemSummaryCollection = new StackHashProductOperatingSystemSummaryCollection();
            m_HitDateSummaryCollection = new StackHashProductHitDateSummaryCollection();
        }

        [DataMember]
        public StackHashProductLocaleSummaryCollection LocaleSummaryCollection
        {
            get { return m_LocaleSummaryCollection; }
            set { m_LocaleSummaryCollection = value; }
        }

        [DataMember]
        public StackHashProductOperatingSystemSummaryCollection OperatingSystemSummary
        {
            get { return m_OperatingSystemSummaryCollection; }
            set { m_OperatingSystemSummaryCollection = value; }
        }

        [DataMember]
        public StackHashProductHitDateSummaryCollection HitDateSummary
        {
            get { return m_HitDateSummaryCollection; }
            set { m_HitDateSummaryCollection = value; }
        }


        /// <summary>
        /// Updates the statistics based on the specified event info.
        /// Each Event info will cause an update to each of the stats lists.
        /// </summary>
        /// <param name="eventInfo">New event info.</param>
        public void AddNewEventInfo(StackHashEventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            // ******************************************************
            // Update the locale rollup stats.
            // ******************************************************
            StackHashProductLocaleSummary localeSummary = m_LocaleSummaryCollection.FindLocale((int)eventInfo.Lcid);

            if (localeSummary == null)
            {
                // Not found so add a new entry.
                localeSummary = new StackHashProductLocaleSummary(eventInfo.Language, eventInfo.Lcid, eventInfo.Locale, eventInfo.TotalHits);
                m_LocaleSummaryCollection.Add(localeSummary);
            }
            else
            {
                // Add the total hits to the existing entry.
                localeSummary.TotalHits += eventInfo.TotalHits;
            }

            // ******************************************************
            // Update the OS stats.
            // ******************************************************
            StackHashProductOperatingSystemSummary operatingSystemSummary =
                m_OperatingSystemSummaryCollection.FindOperatingSystem(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);

            if (operatingSystemSummary == null)
            {
                // Not found so add a new entry.
                operatingSystemSummary = new StackHashProductOperatingSystemSummary(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion, eventInfo.TotalHits);
                m_OperatingSystemSummaryCollection.Add(operatingSystemSummary);
            }
            else
            {
                // Add the total hits to the existing entry.
                operatingSystemSummary.TotalHits += eventInfo.TotalHits;
            }

            // ******************************************************
            // Update the HitDate stats.
            // ******************************************************
            StackHashProductHitDateSummary hitDateSummary = m_HitDateSummaryCollection.FindHitDate(eventInfo.HitDateLocal);

            if (hitDateSummary == null)
            {
                // Not found so add a new entry.
                hitDateSummary = new StackHashProductHitDateSummary(eventInfo.HitDateLocal, eventInfo.TotalHits);
                m_HitDateSummaryCollection.Add(hitDateSummary);
            }
            else
            {
                // Add the total hits to the existing entry.
                hitDateSummary.TotalHits += eventInfo.TotalHits;
            }
        }


        /// <summary>
        /// Updates the statistics based on the specified event info collection.
        /// Each Event info will cause an update to each of the stats lists.
        /// </summary>
        /// <param name="stackHashEventInfoCollection">Collection of new event infos.</param>
        public void AddNewEventInfos(StackHashEventInfoCollection stackHashEventInfoCollection)
        {
            if (stackHashEventInfoCollection == null)
                throw new ArgumentNullException("stackHashEventInfoCollection");

            foreach (StackHashEventInfo eventInfo in stackHashEventInfoCollection)
            {
                AddNewEventInfo(eventInfo);
            }
        }

    }
}
