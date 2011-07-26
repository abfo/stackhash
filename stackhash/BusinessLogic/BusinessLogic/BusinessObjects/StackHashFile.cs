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
    public class StackHashFile : ICloneable, IComparable
    {
        private const int s_ThisStructureVersion = 1;
        private int m_StructureVersion;

        private DateTime m_DateCreatedLocal;
        private DateTime m_DateModifiedLocal;
        private int m_Id;
        private DateTime m_LinkDateLocal;
        private string m_Name;
        private string m_Version;


        public StackHashFile() {;}  // Needed to serialize;

        public StackHashFile(DateTime dateCreatedLocal,
                             DateTime dataModifiedLocal,
                             int id,
                             DateTime linkDateLocal,
                             string name,
                             string version)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dataModifiedLocal;
            m_Id = id;
            m_LinkDateLocal = linkDateLocal;
            m_Name = name;
            m_Version = version;
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
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public DateTime LinkDateLocal
        {
            get { return m_LinkDateLocal; }
            set { m_LinkDateLocal = value; }
        }

        [DataMember]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public string Version
        {
            get { return m_Version; }
            set { m_Version = value; }
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

        /// <summary>
        /// Determines if a change in the file data should be reported to bug tracking plugins.
        /// </summary>
        /// <param name="newFile">The new file data.</param>
        /// <returns>True - significant changes so report it, False - nothing to report.</returns>
        public bool ShouldReportToBugTrackPlugIn(StackHashFile newFile)
        {
            if (newFile == null)
                throw new ArgumentNullException("newFile");

            if (this.Id != newFile.Id)
                return true;
            if (this.Name != newFile.Name)
                return true;
            if (this.Version != newFile.Version)
                return true;

            return false;
        }

        
        #region ICloneable Members

        /// <summary>
        /// Clone a copy of the file information.
        /// </summary>
        /// <returns>Cloned copy of the file.</returns>
        public object Clone()
        {
            StackHashFile file = new StackHashFile(m_DateCreatedLocal, m_DateModifiedLocal, 
                m_Id, m_LinkDateLocal, m_Name, m_Version);

            return file;
        }

        #endregion


        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashFile file = obj as StackHashFile;

            if ((file.DateCreatedLocal == this.DateCreatedLocal) &&
                (file.DateModifiedLocal == this.DateModifiedLocal) &&
                (file.Id == this.Id) &&
                (file.LinkDateLocal == this.LinkDateLocal) &&
                (file.Name == this.Name) &&
                (file.Version == this.Version))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        #endregion

        public override string ToString()
        {
            StringBuilder outString = new StringBuilder("File: DateCreated:");
            outString.Append(m_DateCreatedLocal);
            outString.Append(" DateModified:");
            outString.Append(m_DateModifiedLocal);
            outString.Append(" Id:");
            outString.Append(m_Id);
            outString.Append(" Name:");
            outString.Append(m_Name);
            outString.Append(" Version:");
            outString.Append(m_Version);
            outString.Append(" LinkDateLocal:");
            outString.Append(m_LinkDateLocal);
            outString.Append(" Version:");

            return outString.ToString();
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashFileCollection : Collection<StackHashFile>
    {
        public StackHashFileCollection() { } // Needed to serialize.


        /// <summary>
        /// Locates the file with the specified id.
        /// </summary>
        /// <param name="fileId">Id of file to find.</param>
        /// <returns>Found file or null.</returns>
        public StackHashFile FindFile(int fileId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Id == fileId)
                    return this[i];
            }

            return null;
        }

    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashFileProductMapping
    {
        private int m_FileId;
        private int m_ProductId;

        public StackHashFileProductMapping() { ;}  // Needed to serialize;

        public StackHashFileProductMapping(int fileId, int productId)
        {
            m_FileId = fileId;
            m_ProductId = productId;
        }

        [DataMember]
        public int FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }

        [DataMember]
        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }
    }


    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashFileProductMappingCollection : Collection<StackHashFileProductMapping>
    {
        public StackHashFileProductMappingCollection() { } // Needed to serialize.


        /// <summary>
        /// Locates the file with the specified id.
        /// </summary>
        /// <param name="fileId">Id of file to find.</param>
        /// <returns>Found file or null.</returns>
        public StackHashFileProductMapping FindFile(int fileId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].FileId == fileId)
                    return this[i];
            }

            return null;
        }

    }
}
