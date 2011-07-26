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
    public enum StackHashMappingType
    {
        [EnumMember()]
        WorkFlow,

        [EnumMember()]
        Group,                        
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashMapping : IComparable<StackHashMapping>
    {
        private StackHashMappingType m_MappingType;
        private int m_Id;
        private String m_Name;

        public StackHashMapping() { ; }

        public StackHashMapping(StackHashMappingType mappingType, int id, String name)
        {
            m_MappingType = mappingType;
            m_Id = id;
            m_Name = name;
        }

        [DataMember]
        public StackHashMappingType MappingType
        {
            get { return m_MappingType; }
            set { m_MappingType = value; }
        }

        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        #region IComparable<StackHashMapping> Members

        public int CompareTo(StackHashMapping other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if ((m_MappingType == other.MappingType) &&
                (m_Id == other.Id) &&
                (m_Name == other.Name))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashMappingCollection : Collection<StackHashMapping>
    {
        public StackHashMappingCollection() { ; }

        public StackHashMapping FindMapping(StackHashMappingType mappingType, int id)
        {
            foreach (StackHashMapping mapping in this)
            {
                if ((mapping.MappingType == mappingType) && (mapping.Id == id))
                {
                    return mapping;
                }
            }

            return null;
        }

        public static StackHashMappingCollection DefaultWorkFlowMappings
        {
            get
            {
                StackHashMappingCollection mappings = new StackHashMappingCollection();

                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 0, "Active"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 1, "Active - Unassigned"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 2, "Active - Assigned"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 3, "Active - Investigating"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 4, "Active - Not used"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 5, "Active - Not used"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 6, "Active - Not used"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 7, "Active - Not used"));

                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 8, "Resolved"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 9, "Resolved - Fixed"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 10, "Resolved - Responded"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 11, "Resolved - Duplicate"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 12, "Resolved - Won't Fix"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 13, "Resolved - By Design"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 14, "Resolved - Not used"));
                mappings.Add(new StackHashMapping(StackHashMappingType.WorkFlow, 15, "Resolved - Not used"));

                return mappings;

            }
        }
    }
}

