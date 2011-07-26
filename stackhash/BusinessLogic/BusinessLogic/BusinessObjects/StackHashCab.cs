using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashCab: IComparable, ICloneable
    {
        private const int s_ThisStructureVersion = 1;
        private int m_StructureVersion;

        private DateTime m_DateCreatedLocal;
        private DateTime m_DateModifiedLocal;
        private int m_EventId;
        private string m_EventTypeName;
        private string m_FileName;
        private int m_Id;
        private long m_SizeInBytes;
        private StackHashDumpAnalysis m_DumpAnalysis;
        private bool m_CabDownloaded;
        private bool m_Purged;

        public StackHashCab() { ;}

        public StackHashCab(DateTime dateCreatedLocal, DateTime dateModifiedLocal,
            int eventId, string eventTypeName, string fileName, int id, long sizeInBytes)
        {
            m_StructureVersion = s_ThisStructureVersion;

            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
            m_FileName = fileName;
            m_Id = id;
            m_SizeInBytes = sizeInBytes;
            m_DumpAnalysis = new StackHashDumpAnalysis();
            m_CabDownloaded = false;
            m_Purged = false;
        }

        public StackHashCab(DateTime dateCreatedLocal, DateTime dateModifiedLocal, 
            int eventId, string eventTypeName, string fileName, int id, long sizeInBytes, bool cabDownloaded)
        {
            m_StructureVersion = s_ThisStructureVersion;

            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
            m_FileName = fileName;
            m_Id = id;
            m_SizeInBytes = sizeInBytes;
            m_DumpAnalysis = new StackHashDumpAnalysis();
            m_CabDownloaded = cabDownloaded;
            m_Purged = false;
        }

        public StackHashCab(DateTime dateCreatedLocal, DateTime dateModifiedLocal,
            int eventId, string eventTypeName, string fileName, int id, long sizeInBytes, bool cabDownloaded, bool purged)
        {
            m_StructureVersion = s_ThisStructureVersion;

            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
            m_FileName = fileName;
            m_Id = id;
            m_SizeInBytes = sizeInBytes;
            m_DumpAnalysis = new StackHashDumpAnalysis();
            m_CabDownloaded = cabDownloaded;
            m_Purged = purged;
        }

        public StackHashCab(DateTime dateCreatedLocal, DateTime dateModifiedLocal,
            int eventId, string eventTypeName, string fileName, int id, long sizeInBytes, bool cabDownloaded, bool purged, StackHashDumpAnalysis dumpAnalysis)
        {
            m_StructureVersion = s_ThisStructureVersion;

            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dateModifiedLocal;
            m_EventId = eventId;
            m_EventTypeName = eventTypeName;
            m_FileName = fileName;
            m_Id = id;
            m_SizeInBytes = sizeInBytes;
            m_CabDownloaded = cabDownloaded;
            m_Purged = purged;
            m_DumpAnalysis = dumpAnalysis;
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
        public int EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }

        [DataMember]
        public string EventTypeName
        {
            get { return m_EventTypeName; }
            set { m_EventTypeName = value; }
        }

        [DataMember]
        public string FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public long SizeInBytes
        {
            get { return m_SizeInBytes; }
            set { m_SizeInBytes = value; }
        }

        [DataMember]
        public StackHashDumpAnalysis DumpAnalysis
        {
            get { return m_DumpAnalysis; }
            set { m_DumpAnalysis = value; }
        }

        [DataMember]
        public bool CabDownloaded
        {
            get { return m_CabDownloaded; }
            set { m_CabDownloaded = value; }
        }

        [DataMember]
        public int StructureVersion
        {
            get { return m_StructureVersion; }
            set { m_StructureVersion = value; }
        }

        [DataMember]
        public bool Purged
        {
            get { return m_Purged; }
            set { m_Purged = value; }
        }

        public static int ThisStructureVersion
        {
            get { return s_ThisStructureVersion; }
        }

        public int CompareDiagnostics(StackHashCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            if ((this.DumpAnalysis.DotNetVersion == cab.DumpAnalysis.DotNetVersion) &&
                (this.DumpAnalysis.MachineArchitecture == cab.DumpAnalysis.MachineArchitecture) &&
                (this.DumpAnalysis.OSVersion == cab.DumpAnalysis.OSVersion) &&
                (this.DumpAnalysis.ProcessUpTime == cab.DumpAnalysis.ProcessUpTime) &&
                (this.DumpAnalysis.SystemUpTime == cab.DumpAnalysis.SystemUpTime))
            {
                return 0;
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Determines if a change in the cab data should be reported to bug tracking plugins.
        /// </summary>
        /// <param name="newCab">The new cab data.</param>
        /// <param name="checkNonWinQualFields">True - the non-winqual fields are compared too.</param>
        /// <returns>True - significant changes so report it, False - nothing to report.</returns>
        public bool ShouldReportToBugTrackPlugIn(StackHashCab newCab, bool checkNonWinQualFields)
        {
            if (newCab == null)
                throw new ArgumentNullException("newCab");

            if (this.Id != newCab.Id)
                return true;
            if (this.CabDownloaded != newCab.CabDownloaded)
                return true;
            if (this.EventId != newCab.EventId)
                return true;
            if (this.EventTypeName != newCab.EventTypeName)
                return true;
            if (this.Purged != newCab.Purged)
                return true;
            if (this.SizeInBytes != newCab.SizeInBytes)
                return true;
            if (this.FileName != newCab.FileName)
                return true;

            if (checkNonWinQualFields)
            {
                if (CompareDiagnostics(newCab) != 0)
                    return true;
            }

            return false;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashCab cab = obj as StackHashCab;

            // Don't check the cabs downloaded flag.

            if ((this.m_DateCreatedLocal == cab.DateCreatedLocal) &&
                (this.m_DateModifiedLocal == cab.DateModifiedLocal) &&
                (this.m_EventId == cab.EventId) &&
                (this.m_EventTypeName == cab.EventTypeName) &&
                (this.m_FileName == cab.FileName) &&
                (this.m_Id == cab.Id) &&
                (this.m_SizeInBytes == cab.SizeInBytes))
            {
                return 0;
            }
            else
            {
                return -1;
            }

        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            StackHashDumpAnalysis dumpAnalysis = null;
            if (this.DumpAnalysis != null)
                dumpAnalysis = (StackHashDumpAnalysis)this.DumpAnalysis.Clone();

            return new StackHashCab(this.DateCreatedLocal, this.DateModifiedLocal, this.EventId, this.EventTypeName, this.FileName,
                this.Id, this.SizeInBytes, this.CabDownloaded, this.Purged, dumpAnalysis);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder outString = new StringBuilder("CAB: DateCreated:");

            outString.Append(m_DateCreatedLocal);
            outString.Append(" DateModified:");
            outString.Append(m_DateModifiedLocal);
            outString.Append(" Id:");
            outString.Append(m_Id);
            outString.Append(" EventId:");
            outString.Append(m_EventId);
            outString.Append(" EventTypeName:");
            outString.Append(m_EventTypeName);
            outString.Append(" FileName:");
            outString.Append(m_FileName);
            outString.Append(" SizeInBytes:");
            outString.Append(m_SizeInBytes);
            outString.Append(" SizeInBytes:");

            return outString.ToString();
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashCabCollection : Collection<StackHashCab>, IComparable, ICloneable
    {
        public StackHashCabCollection() { } // Needed to serialize.

        /// <summary>
        /// Locates the cab with the specified id.
        /// </summary>
        /// <param name="cabId">Id of file to find.</param>
        /// <returns>Found cab or null.</returns>
        public StackHashCab FindCab(int cabId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Id == cabId)
                    return this[i];
            }

            return null;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashCabCollection cabs = obj as StackHashCabCollection;

            if (cabs.Count != this.Count)
                return -1;

            foreach (StackHashCab thisCab in this)
            {
                StackHashCab matchingCab = cabs.FindCab(thisCab.Id);

                if (matchingCab == null)
                    return -1;

                if (matchingCab.CompareTo(thisCab) != 0)
                    return -1;
            }

            // Must be a match.
            return 0;
        }
        #endregion

        #region ICloneable Members

        public object Clone()
        {
            StackHashCabCollection cabCollection = new StackHashCabCollection();

            foreach (StackHashCab cab in this)
            {
                cabCollection.Add((StackHashCab)cab.Clone());
            }

            return cabCollection;
        }

        #endregion
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashCabFile
    {
        private String m_FileName;
        private long m_Length;

        public StackHashCabFile(String fileName, long length)
        {
            m_FileName = fileName;
            m_Length = length;
        }

        [DataMember]
        public String FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        [DataMember]
        public long Length
        {
            get { return m_Length; }
            set { m_Length = value; }
        }
    }



    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashCabFilesCollection : Collection<StackHashCabFile>
    {
        public StackHashCabFilesCollection() { } // Needed to serialize.
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashCabFileContents
    {
        private StackHashCabFilesCollection m_Files = new StackHashCabFilesCollection();

        public StackHashCabFileContents()
        {
        }

        [DataMember]
        public StackHashCabFilesCollection Files
        {
            get { return m_Files; }
            set { m_Files = value; }
        }

    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashCabPackage : ICloneable, IComparable<StackHashCabPackage>
    {
        private StackHashCab m_Cab;
        private String m_FullPath;
        private StackHashCabFileContents m_CabFileContents;
        private bool m_IsSearchMatch;

        public StackHashCabPackage(StackHashCab cab, String path, StackHashCabFileContents cabFileContents, bool isSearchMatch)
        {
            m_Cab = cab;
            m_FullPath = path;
            m_CabFileContents = cabFileContents;
            m_IsSearchMatch = isSearchMatch;
        }

        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        [DataMember]
        public String FullPath
        {
            get { return m_FullPath; }
            set { m_FullPath = value; }
        }

        [DataMember]
        public StackHashCabFileContents CabFileContents
        {
            get { return m_CabFileContents; }
            set { m_CabFileContents = value; }
        }

        [DataMember]
        public bool IsSearchMatch
        {
            get { return m_IsSearchMatch; }
            set { m_IsSearchMatch = value; }
        }

        #region ICloneable Members

        public object Clone()
        {
            StackHashCabPackage cabPackage = new StackHashCabPackage((StackHashCab)this.Cab.Clone(), this.FullPath, this.CabFileContents, this.IsSearchMatch);

            return cabPackage;
        }

        #endregion

        #region IComparable<StackHashCabPackage> Members

        public int CompareTo(StackHashCabPackage other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return (other.Cab.CompareTo(this.Cab));
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashCabPackageCollection : Collection<StackHashCabPackage>, IComparable<StackHashCabPackageCollection>, ICloneable
    {
        public StackHashCabPackageCollection() { } // Needed to serialize.

        public StackHashCabPackageCollection(StackHashCabCollection cabs)
        {
            if (cabs == null)
                throw new ArgumentNullException("cabs");

            foreach (StackHashCab cab in cabs)
            {
                this.Add(new StackHashCabPackage(cab, null, null, true));
            }           
        }

        /// <summary>
        /// Locates the cab with the specified id.
        /// </summary>
        /// <param name="cabId">Id of file to find.</param>
        /// <returns>Found cab or null.</returns>
        public StackHashCabPackage FindCab(int cabId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Cab.Id == cabId)
                    return this[i];
            }

            return null;
        }

        #region IComparable Members

        public int CompareTo(StackHashCabPackageCollection other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (other.Count != this.Count)
                return -1;

            foreach (StackHashCabPackage thisCab in this)
            {
                StackHashCabPackage matchingCab = other.FindCab(thisCab.Cab.Id);

                if (matchingCab == null)
                    return -1;

                if (matchingCab.CompareTo(thisCab) != 0)
                    return -1;
            }

            // Must be a match.
            return 0;
        }
        #endregion

        #region ICloneable Members

        public object Clone()
        {
            StackHashCabPackageCollection cabCollection = new StackHashCabPackageCollection();

            foreach (StackHashCabPackage cab in this)
            {
                cabCollection.Add((StackHashCabPackage)cab.Clone());
            }

            return cabCollection;
        }

        #endregion
    }


}
