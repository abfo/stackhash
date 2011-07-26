using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public class AtomEventInfo
    {
        private StackHashEventInfo m_StackHashEventInfo;

        public AtomEventInfo() { ;}  // Needed to serialize;

        public AtomEventInfo(StackHashEventInfo eventInfo)
        {
            m_StackHashEventInfo = eventInfo;
        }

        public StackHashEventInfo EventInfo
        {
            get { return m_StackHashEventInfo; }
            set { m_StackHashEventInfo = value; }
        }
    }


    public class AtomEventInfoCollection : Collection<AtomEventInfo>
    {
        public AtomEventInfoCollection() { }
    }

}

