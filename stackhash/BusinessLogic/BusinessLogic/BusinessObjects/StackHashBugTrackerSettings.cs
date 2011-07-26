using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugTrackerPlugInDiagnostics
    {
        private String m_Name;
        private String m_FileName;
        private bool m_IsLoaded;
        private StackHashNameValueCollection m_Diagnostics;
        private String m_LastException;
        private StackHashNameValueCollection m_DefaultProperties;
        private String m_PlugInDescription;
        private Uri m_HelpUrl;
        private bool m_PlugInSetsBugReference;

        public StackHashBugTrackerPlugInDiagnostics() { ; }  // Required for serialization.

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public String FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        [DataMember]
        public bool Loaded
        {
            get { return m_IsLoaded; }
            set { m_IsLoaded = value; }
        }

        [DataMember]
        public StackHashNameValueCollection Diagnostics
        {
            get { return m_Diagnostics; }
            set { m_Diagnostics = value; }
        }

        [DataMember]
        public String LastException
        {
            get { return m_LastException; }
            set { m_LastException = value; }
        }

        [DataMember]
        public StackHashNameValueCollection DefaultProperties
        {
            get { return m_DefaultProperties; }
            set { m_DefaultProperties = value; }
        }

        [DataMember]
        public String PlugInDescription
        {
            get { return m_PlugInDescription; }
            set { m_PlugInDescription = value; }
        }

        [DataMember]
        public Uri HelpUrl
        {
            get { return m_HelpUrl; }
            set { m_HelpUrl = value; }
        }

        [DataMember]
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public bool PlugInSetsBugReference
        {
            get { return m_PlugInSetsBugReference; }
            set { m_PlugInSetsBugReference = value; }
        }
    }


    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashBugTrackerPlugInDiagnosticsCollection : Collection<StackHashBugTrackerPlugInDiagnostics>
    {
        public StackHashBugTrackerPlugInDiagnosticsCollection() { } // Needed to serialize.
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugTrackerPlugIn
    {
        private String m_Name;
        private bool m_Enabled;
        private StackHashNameValueCollection m_Properties;
        private bool m_ChangesBugReference;
        private String m_PlugInDescription;
        private Uri m_HelpUrl;

        public StackHashBugTrackerPlugIn() { ; }  // Required for serialization.

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public bool Enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        [DataMember]
        public bool ChangesBugReference
        {
            get { return m_ChangesBugReference; }
            set { m_ChangesBugReference = value; }
        }

        [DataMember]
        public StackHashNameValueCollection Properties
        {
            get { return m_Properties; }
            set { m_Properties = value; }
        }

        [DataMember]
        public String PlugInDescription
        {
            get { return m_PlugInDescription; }
            set { m_PlugInDescription = value; }
        }

        [XmlIgnore]
        [DataMember]
        // Can't be XML serialized because Uri doesn't have a default constructor.
        public Uri HelpUrl
        {
            get { return m_HelpUrl; }
            set { m_HelpUrl = value; }
        }

        // Don't add the DateMember attribute as this is should not be visible in the client.
        // This is required because the Uri type can't be serialized by the XML serializer because
        // it doesn't have a default constructor.
        public String HelpString
        {
            get { return HelpUrl == null ? null : HelpUrl.ToString(); }
            set { HelpUrl = (value == null) ? null : new Uri(value); }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashBugTrackerPlugInCollection : Collection<StackHashBugTrackerPlugIn>
    {
        public StackHashBugTrackerPlugInCollection() { } // Needed to serialize.
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugTrackerPlugInSettings
    {
        private StackHashBugTrackerPlugInCollection m_PlugInSettings;

        public StackHashBugTrackerPlugInSettings() { ; }  // Required for serialization.

        [DataMember]
        public StackHashBugTrackerPlugInCollection PlugInSettings
        {
            get { return m_PlugInSettings; }
            set { m_PlugInSettings = value; }
        }
    }
    
}
