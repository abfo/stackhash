

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace StackHashBugTrackerInterfaceV1
{
    /// <summary>
    /// Severity code for a diagnostics message
    /// </summary>
    public enum BugTrackerTraceSeverity
    {
        /// <summary>
        /// General information.
        /// </summary>
        Information,

        /// <summary>
        /// Unexpected but handled without significant impact.
        /// </summary>
        Warning,

        /// <summary>
        /// Unexpected and fatal to a component or dialog, but the user can continue.
        /// </summary>
        ComponentFatal,

        /// <summary>
        /// Unexpected and fatal to the application.
        /// </summary>
        ApplicationFatal,
    };

    public class BugTrackerTraceEventArgs : EventArgs
    {
        BugTrackerTraceSeverity m_Severity;
        String m_Message;

        public BugTrackerTraceEventArgs(BugTrackerTraceSeverity severity, String message)
        {
            m_Severity = severity;
            m_Message = message;
        }

        public BugTrackerTraceSeverity Severity
        {
            get { return m_Severity; }
            set { m_Severity = value; }
        }

        public String Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }
    }

    public static class BugTrackerTrace
    {
        public static event EventHandler<BugTrackerTraceEventArgs> LogMessageHook;

        [SuppressMessage("Microsoft.Globalization", "CA1303", Justification="Complaining about : in string being in a resource file")]
        public static void LogMessage(BugTrackerTraceSeverity severity, String plugInName, String message)
        {
            if (plugInName == null)
                throw new ArgumentNullException("plugInName");
            if (message == null)
                throw new ArgumentNullException("message");

            EventHandler<BugTrackerTraceEventArgs> handler = LogMessageHook;

            if (handler != null)
            {
                BugTrackerTraceEventArgs args = new BugTrackerTraceEventArgs(severity, plugInName + ": " + message);

                handler(null, args);
            }
        }

    }
}
