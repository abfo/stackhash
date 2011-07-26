using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public class AtomCab
    {
        private const String s_WinQualCabsUrl = "https://winqual.microsoft.com/services/wer/user/downloadcab.aspx";
        private StackHashCab m_StackHashCab;
        private String m_CabLink;

        public AtomCab() { ; }  // Needed to serialize;

        public AtomCab(StackHashCab cab, String cabLink)
        {
            m_StackHashCab = cab;
            m_CabLink = cabLink;
        }

        public StackHashCab Cab
        {
            get { return m_StackHashCab; }
            set { m_StackHashCab = value; }
        }
        public String CabLink
        {
            get { return m_CabLink; }
            set { m_CabLink = value; }
        }

        public static String MakeLink(String eventTypeName, int eventId, int cabId, long cabSize)
        {
            StringBuilder cabLink = new StringBuilder(s_WinQualCabsUrl);
            cabLink.Append("?cabID=");
            cabLink.Append(cabId.ToString());
            cabLink.Append("&eventid=");
            cabLink.Append(eventId.ToString());
            cabLink.Append("&eventtypename=");
            cabLink.Append(eventTypeName);
            cabLink.Append("&size=");
            cabLink.Append(cabSize.ToString());

            //  href="https://winqual.microsoft.com/Services/wer/user/downloadcab.aspx?cabID=837908536&eventid=1099298431&eventtypename=CLR20 Managed Crash&size=4631248" /> 
            return cabLink.ToString();
        }
    }


    public class AtomCabCollection : Collection<AtomCab>
    {
        public AtomCabCollection() { }
    }

}

