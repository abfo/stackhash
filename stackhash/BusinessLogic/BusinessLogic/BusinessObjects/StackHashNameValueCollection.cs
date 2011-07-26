using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashNameValuePair
    {
        private String m_Name;
        private String m_Value;

        public StackHashNameValuePair() { ; }  // Required for serialization.

        public StackHashNameValuePair(String name, String value)
        {
            m_Name = name;
            m_Value = value;
        }

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public String Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashNameValueCollection : Collection<StackHashNameValuePair>
    {
        public StackHashNameValueCollection() { } // Needed to serialize.

        public StackHashNameValueCollection(NameValueCollection nameValueCollection)
        {
            if (nameValueCollection == null)
                throw new ArgumentNullException("nameValueCollection");

            foreach (String key in nameValueCollection.AllKeys)
            {
                this.Add(new StackHashNameValuePair(key, nameValueCollection[key]));
            }
        }

        public NameValueCollection ToNameValueCollection()
        {
            NameValueCollection newSettings = new NameValueCollection();

            foreach (StackHashNameValuePair pair in this)
            {
                newSettings[pair.Name] = pair.Value;
            }
            return newSettings;
        }
    }

}
