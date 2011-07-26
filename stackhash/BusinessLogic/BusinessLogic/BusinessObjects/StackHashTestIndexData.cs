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
    public class StackHashTestIndexData
    {
        private int m_NumberOfProducts;
        private int m_NumberOfFiles;
        private int m_NumberOfEvents;
        private int m_NumberOfEventInfos;
        private int m_NumberOfCabs;
        private bool m_UseLargeCab;
        private String m_CabFileName;
        private bool m_UnwrapCabs;
        private bool m_UseUnmanagedCab;
        private int m_NumberOfEventNotes;
        private int m_NumberOfCabNotes;
        private bool m_SetBugId;
        private bool m_DuplicateFileIdsAcrossProducts;
        private int m_NumberOfScriptResults;
        private int m_EventsToAssignCabs;
        private int m_ScriptFileSize;

        public StackHashTestIndexData() { ;}

        [DataMember]
        public int NumberOfProducts
        {
            get { return m_NumberOfProducts; }
            set { m_NumberOfProducts = value; }
        }

        [DataMember]
        public int NumberOfFiles
        {
            get { return m_NumberOfFiles; }
            set { m_NumberOfFiles = value; }
        }

        [DataMember]
        public int NumberOfEvents
        {
            get { return m_NumberOfEvents; }
            set { m_NumberOfEvents = value; }
        }
        [DataMember]
        public int NumberOfEventInfos
        {
            get { return m_NumberOfEventInfos; }
            set { m_NumberOfEventInfos = value; }
        }
        [DataMember]
        public int NumberOfCabs
        {
            get { return m_NumberOfCabs; }
            set { m_NumberOfCabs = value; }
        }
        [DataMember]
        public bool UseLargeCab
        {
            get { return m_UseLargeCab; }
            set { m_UseLargeCab = value; }
        }
        [DataMember]
        public String CabFileName
        {
            get { return m_CabFileName; }
            set { m_CabFileName = value; }
        }
        [DataMember]
        public bool UnwrapCabs
        {
            get { return m_UnwrapCabs; }
            set { m_UnwrapCabs = value; }
        }
        [DataMember]
        public bool UseUnmanagedCab
        {
            get { return m_UseUnmanagedCab; }
            set { m_UseUnmanagedCab = value; }
        }
        [DataMember]
        public int NumberOfEventNotes
        {
            get { return m_NumberOfEventNotes; }
            set { m_NumberOfEventNotes = value; }
        }
        [DataMember]
        public int NumberOfCabNotes
        {
            get { return m_NumberOfCabNotes; }
            set { m_NumberOfCabNotes = value; }
        }
        [DataMember]
        public bool SetBugId
        {
            get { return m_SetBugId; }
            set { m_SetBugId = value; }
        }
        [DataMember]
        public bool DuplicateFileIdsAcrossProducts
        {
            get { return m_DuplicateFileIdsAcrossProducts; }
            set { m_DuplicateFileIdsAcrossProducts = value; }
        }
        [DataMember]
        public int NumberOfScriptResults
        {
            get { return m_NumberOfScriptResults; }
            set { m_NumberOfScriptResults = value; }
        }
        [DataMember]
        public int EventsToAssignCabs
        {
            get { return m_EventsToAssignCabs; }
            set { m_EventsToAssignCabs = value; }
        }
        [DataMember]
        public int ScriptFileSize
        {
            get { return m_ScriptFileSize; }
            set { m_ScriptFileSize = value; }
        }
    }
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashTestDummyWinQualSettings
    {
        StackHashTestIndexData m_ObjectsToCreate;
        bool m_UseDummyWinQual;
        bool m_FailLogOn;
        bool m_FailSync;

        [DataMember]
        public bool UseDummyWinQual
        {
            get
            {
                return m_UseDummyWinQual;
            }

            set
            {
                m_UseDummyWinQual = value;
            }
        }

        [DataMember]
        public StackHashTestIndexData ObjectsToCreate
        {
            get
            {
                return m_ObjectsToCreate;
            }

            set
            {
                m_ObjectsToCreate = value;
            }
        }

        [DataMember]
        public bool FailLogOn
        {
            get
            {
                return m_FailLogOn;
            }

            set
            {
                m_FailLogOn = value;
            }
        }
        [DataMember]
        public bool FailSync
        {
            get
            {
                return m_FailSync;
            }

            set
            {
                m_FailSync = value;
            }
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashTestData
    {
        StackHashTestDummyWinQualSettings m_DummyWinQualSettings;

        [DataMember]
        public StackHashTestDummyWinQualSettings DummyWinQualSettings
        {
            get
            {
                return m_DummyWinQualSettings;
            }

            set
            {
                m_DummyWinQualSettings = value;
            }
        }


        public static StackHashTestData Default
        {
            get
            {
                StackHashTestData testData = new StackHashTestData();
                testData.DummyWinQualSettings = new StackHashTestDummyWinQualSettings();
                testData.DummyWinQualSettings.UseDummyWinQual = true;
                testData.DummyWinQualSettings.ObjectsToCreate = new StackHashTestIndexData();
                testData.DummyWinQualSettings.ObjectsToCreate.NumberOfProducts = 0;

                return testData;
            }
        }
    }
}
