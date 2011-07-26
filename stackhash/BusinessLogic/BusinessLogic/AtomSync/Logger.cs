using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;


using StackHashUtilities;

namespace AtomSync
{
    public class LogManager : IDisposable
    {
        private TextWriterTraceListener m_Listener;


        public LogManager()
        {
        }
            
        public void StartLogging()
        {
            // Don't start the listener if already started.
            if (m_Listener == null)
            {
                m_Listener = new TextWriterTraceListener();
                m_Listener.Writer = System.Console.Out;
                Trace.Listeners.Add(m_Listener);

                DiagnosticsHelper.LogApplicationStartup();
            }
        }

        public void StopLogging()
        {
            if (m_Listener != null)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Logging disabled");
                Trace.Listeners.Remove(m_Listener);
                m_Listener.Flush();
                m_Listener.Close();
                m_Listener = null;
            }
        }


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopLogging();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
