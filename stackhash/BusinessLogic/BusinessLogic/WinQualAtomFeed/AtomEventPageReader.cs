using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WinQualAtomFeed;

namespace WinQualAtomFeed
{
    public class AtomEventPageReader
    {
        private IAtomFeed m_AtomFeed;
        private String m_NextPageLink;
        private AtomFile m_AtomFile;
        private int m_TotalPages;
        private int m_CurrentPage;
        private DateTime m_StartTime;
        private DateTime m_EndTime;

        public AtomEventPageReader(IAtomFeed atomFeed, AtomFile file, DateTime startTime, DateTime endTime)
        {
            m_AtomFeed = atomFeed;
            m_AtomFile = file;
            m_NextPageLink = m_AtomFile.EventsLink;
            m_StartTime = startTime;
            m_EndTime = endTime;
        }

        public int TotalPages
        {
            get { return m_TotalPages; }
        }

        public int CurrentPage
        {
            get { return m_CurrentPage; }
        }

        public AtomEventCollection ReadPage()
        {
            if (String.IsNullOrEmpty(m_NextPageLink))
                return null;

            m_EndTime = DateTime.Now;


            // The call to get the first page should include the start date.
            if ((m_CurrentPage == 0) && (m_NextPageLink != null))
            {
                // Specifying an end date can fail if the clock of the PC is wrong.
                //m_NextPageLink = String.Format("{0}&startdate={1:s}Z&enddate={2:s}Z",
                //    m_NextPageLink,
                //    (m_StartTime.Kind == DateTimeKind.Utc ? m_StartTime : m_StartTime.ToUniversalTime()),
                //    (m_EndTime.Kind == DateTimeKind.Utc ? m_EndTime : m_EndTime.ToUniversalTime()));
                m_NextPageLink = String.Format("{0}&startdate={1:s}Z",
                    m_NextPageLink,
                    (m_StartTime.Kind == DateTimeKind.Utc ? m_StartTime : m_StartTime.ToUniversalTime()));
            }

            // This seems to happen occasionally. The next link seems to have caps for ID. Change it just in case it
            // could cause a problem.
            m_NextPageLink = m_NextPageLink.Replace("fileID", "fileid");

            // Need to take out the enddate as we are not setting it. For some reason the next link 
            // includes the enddate= as an empty field. This results in a failure on pages after the first page.
            m_NextPageLink = m_NextPageLink.Replace("&enddate=", "");

            // Get the events for this page.
            AtomEventCollection events = m_AtomFeed.GetEventsPage(ref m_NextPageLink, m_AtomFile, out m_TotalPages, out m_CurrentPage);

            return events;
        }
    }
}
