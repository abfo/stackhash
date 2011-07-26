using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackHashWinQual
{
    /// <summary>
    /// This class represents a logged in WinQual session. It allows for the fact
    /// that a company may be registered on WinQual more than once. i.e. may have 
    /// several WinQual accounts. 
    /// A WinQualContext is defined by its logon credentials.
    /// </summary>
    public class WinQualContext
    {
        private IWinQualServices m_WinQualServices;

        /// <summary>
        /// The service interface used to communicate with WinQual online.
        /// </summary>
        public IWinQualServices WinQualServices
        {
            get { return m_WinQualServices; }
        }

        
        /// <summary>
        /// Create a new WinQual context. The context depends on services provided to it.
        /// This is an interface so that it can be easily replaced with a dummy for testing.
        /// </summary>
        /// <param name="winQualServices">Interface to the real worker object.</param>
        public WinQualContext(IWinQualServices winQualServices)
        {
            m_WinQualServices = winQualServices;
        }
    }
}
