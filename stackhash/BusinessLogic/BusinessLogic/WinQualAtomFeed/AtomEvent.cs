using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public class AtomEvent
    {
        private StackHashEvent m_StackHashEvent;
        private String m_EventInfoLink;
        private String m_CabsLink;
        private int m_ProductId;
        private int m_FileId;

        public AtomEvent() { ;}  // Needed to serialize;

        public AtomEvent(StackHashEvent theEvent, String eventInfoLink, String cabsLink)
        {
            m_StackHashEvent = theEvent;
            m_EventInfoLink = eventInfoLink;
            m_CabsLink = cabsLink;
        }

        public StackHashEvent Event
        {
            get { return m_StackHashEvent; }
            set { m_StackHashEvent = value; }
        }

        public String EventInfoLink
        {
            get { return m_EventInfoLink; }
            set { m_EventInfoLink = value; }
        }

        public String CabsLink
        {
            get { return m_CabsLink; }
            set { m_CabsLink = value; }
        }

        public int ProductId
        {
            get { return m_ProductId; }
            set { m_ProductId = value; }
        }

        public int FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }
    }


    public class AtomEventCollection : Collection<AtomEvent>
    {
        public AtomEventCollection() { }
    }

}

