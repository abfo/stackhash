using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public class AtomFile 
    {
        private StackHashFile m_StackHashFile;
        private String m_EventsLink;
        private int m_ProductId;

        public AtomFile() {;}  // Needed to serialize;

        public AtomFile(StackHashFile file, String eventsLink)
        {
            m_StackHashFile = file;
            m_EventsLink = eventsLink;
        }

        public StackHashFile File
        {
            get { return m_StackHashFile; }
            set { m_StackHashFile = value; }
        }

        public String EventsLink
        {
            get { return m_EventsLink; }
            set { m_EventsLink = value; }
        }

        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }
    }


    public class AtomFileCollection : Collection<AtomFile>
    {
        public AtomFileCollection() { }
    }

}

