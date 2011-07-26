using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace StackHashUtilities
{

    /// <summary>
    /// Severity code for a diagnostics message
    /// </summary>
    public enum DiagSeverity
    {
        /// <summary>
        /// General information
        /// </summary>
        Information,
        
        /// <summary>
        /// Unexpected but handled without significant impact
        /// </summary>
        Warning,
        
        /// <summary>
        /// Unexpected and fatal to a component or dialog, but the user can continue
        /// </summary>
        ComponentFatal,
        
        /// <summary>
        /// Unexpected and fatal to the application
        /// </summary>
        ApplicationFatal,
    };

    public static class DiagnosticsHelper
    {
	    private static Object s_LockObject = new Object();
        private static EventLog s_EventLog;

        public static String Source
        {
            get
            {
                return "StackHash";
            }
        }

        public static String Log
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Returns a string containing information about the current runtime environment
        /// </summary>
        /// <returns>String containing the runtime environment, or an empty string on error</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static String EnvironmentInfo
        {
            get
            {
                String envInfo = String.Empty;

                try
                {
                    bool currentUserIsAdmin = SystemInformation.IsAdmin();
                    Assembly thisAssembly = Assembly.GetEntryAssembly();

                    StringBuilder sb = new StringBuilder(1024);

                    if (thisAssembly != null)
                    {
                        // This can be null in test mode - where an unmanaged exe loads a managed dll.
                        // e.g. msbuild.exe (unmanaged) calling a managed DLL.
                        sb.AppendLine(thisAssembly.GetName().Name + " " + thisAssembly.GetName().Version);
                    }

                    sb.AppendLine("Command Line: " + Environment.CommandLine);
                    sb.AppendLine("Current Directory: " + Environment.CurrentDirectory);
                    sb.AppendLine("Framework: " + Environment.Version.ToString());
                    sb.AppendLine("Machine Name: " + Environment.MachineName);
                    sb.AppendLine("OS: " + Environment.OSVersion.VersionString);
                    sb.AppendLine("x64: " + SystemInformation.Is64BitSystem().ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("Processors: " + Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("Current Culture: " + System.Globalization.CultureInfo.CurrentCulture.ToString());
                    sb.AppendLine("Current UI Culture (for current thread): " + Thread.CurrentThread.CurrentUICulture.ToString());
                    sb.AppendLine("Administrator: " + currentUserIsAdmin.ToString());
                    envInfo = sb.ToString();
                }
                catch (System.Exception)
                {
                    envInfo = String.Empty;
                }

                return envInfo;
            }
        }


        /// <summary>
        /// Called to log system information when the application starts.
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public static void LogApplicationStartup()
        {
            LogMessage(DiagSeverity.Information, "Application Startup");
            LogMessage(DiagSeverity.Information, "Application Environment: " + EnvironmentInfo);
        }


        /// <summary>
        /// Log a message indicating that a form has been opened.
        /// </summary>
        /// <param name="formName">The design-time name of the form</param>

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public static void LogFormOpened(String formName)
        {
            LogMessage(DiagSeverity.Information, formName + " opened");
        }


        /// <summary>
        /// Log a message indicating that a form has been closed.
        /// </summary>
        /// <param name="formName">The design-time name of the form</param>
        /// <param name="result">The XAML result for the form</param>
        
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public static void LogFormClosed(String formName, bool? result)
        {
            LogMessage(DiagSeverity.Information,
                       String.Format(CultureInfo.InvariantCulture, "{0} closed ({1})",
                       formName,
                       result));
        }


        /// <summary>
        /// Log a message indicating that a thread has been started.
        /// </summary>
        /// <param name="thread">The thread to log</param>

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public static void LogThreadStart(Thread thread)
        {
            if (thread != null)
            {
                LogMessage(DiagSeverity.Information,
                           String.Format(CultureInfo.InvariantCulture, "Thread Started: {0} (Is Background={1})",
                           thread.Name,
                           thread.IsBackground));
            }
        }


        /// <summary>
        /// Log a message indicating that a thread has ended.
        /// </summary>
        /// <param name="thread">The thread to log</param>

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public static void LogThreadEnd(Thread thread)
        {
            if (thread != null)
            {
                LogMessage(DiagSeverity.Information,
                           String.Format(CultureInfo.InvariantCulture, "Thread Ended: {0}",
                           thread.Name));
            }
        }


        /// <summary>
        /// Log a text message
        /// </summary>
        /// <param name="severity">DiagSeverity for the message</param>
        /// <param name="message">Message to log</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void LogMessage(DiagSeverity severity, String message)
        {
	        Monitor.Enter(s_LockObject);
	        try
	        {
                PrefixForSeverity(severity);
                PrefixDateTime();
		        Trace.WriteLine(message);
		        Trace.Flush();

		        if ((severity == DiagSeverity.ApplicationFatal) ||
			        (severity == DiagSeverity.ComponentFatal))
		        {
			        LogMessageToSysEventLog(message);
		        }		
            }
	        catch (System.Exception)
	        {
	        }
	        finally
	        {
		        Monitor.Exit(s_LockObject);
	        }
        }

        /// <summary>
        /// Log to System log.
        /// </summary>
        /// <param name="message">Message to log</param>

        public static void LogMessageToSysEventLog(String message)
        {
            LogMessageToSysEventLog(message, EventLogEntryType.Error);
        }


        public static void SetEventLog(EventLog eventLog)
        {
            s_EventLog = eventLog;
        }

        /// <summary>
        /// Log to System log.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="eventLogEntryType">Type of logged event</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void LogMessageToSysEventLog(String message, EventLogEntryType eventLogEntryType)
        {
	        if (message == null)
		        return;

            if (s_EventLog == null)
                return;

	        Monitor.Enter(s_LockObject);
	        try
	        {
                s_EventLog.WriteEntry(message, eventLogEntryType);
	        }
	        catch (System.Exception ex)
	        {
		        // Ignore errors logging errors.
		        try
		        {
			        Trace.WriteLine("Error logging to sys log:" + ex.Message);
		        }
		        catch (System.Exception){;}
	        }
	        finally
	        {
		        Monitor.Exit(s_LockObject);
	        }
        }


        /// <summary>
        /// Log an excpetion with a message
        /// </summary>
        /// <param name="severity">DiagSeverity for the message</param>
        /// <param name="message">Message to log</param>
        /// <param name="ex">Exception to log</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public static void LogException(DiagSeverity severity, String message, Exception ex)
        {
	        Monitor.Enter(s_LockObject);
	        try
	        {
                PrefixForSeverity(severity);
                PrefixDateTime();
		        Trace.WriteLine(message);
		        String exceptionText = GetExceptionText(ex);
		        Trace.WriteLine(exceptionText);
		        Trace.Flush();

		        if ((severity == DiagSeverity.ApplicationFatal) ||
			        (severity == DiagSeverity.ComponentFatal))
		        {
                    String fullMessage = String.Format(CultureInfo.InvariantCulture, "{0}:{1}", message, exceptionText);
                    LogMessageToSysEventLog(fullMessage);
		        }		
            }
	        catch (System.Exception)
	        {
	        }
	        finally
	        {
		        Monitor.Exit(s_LockObject);
	        }
        }


        /// <summary>
        /// Called to log application termination marking the end of the log for a session
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void LogAppExit(int exitCode)
        {
	        Monitor.Enter(s_LockObject);
	        try
	        {
                PrefixForSeverity(DiagSeverity.Information);
                PrefixDateTime();
		        Trace.WriteLine("Application Exit (" + exitCode.ToString(CultureInfo.InvariantCulture) + ")");
		        Trace.WriteLine(String.Empty);
		        Trace.Flush();
            }
	        catch (System.Exception)
	        {
	        }

	        finally
	        {
		        Monitor.Exit(s_LockObject);
	        }
        }


        /// <summary>
        /// Log an the date and time
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void PrefixDateTime()
        {
	        try
	        {
		        Trace.Write(String.Format(CultureInfo.InvariantCulture,
                            "{0:yyyy-MM-dd HH:mm:ss} [{1}] ",
					         DateTime.Now,
					         Thread.CurrentThread.ManagedThreadId));
	        }
	        catch (System.Exception)
	        {
	        }
        }


        /// <summary>
        /// Log the severity.
        /// </summary>
        /// <param name="severity">DiagSeverity for the message</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void PrefixForSeverity(DiagSeverity severity)
        {
	        try
	        {
		        switch (severity)
		        {
			        case DiagSeverity.ApplicationFatal:
				        Trace.Write("!!! ");
				        break;

			        case DiagSeverity.ComponentFatal:
				        Trace.Write("!!- ");
				        break;

			        case DiagSeverity.Warning:
				        Trace.Write("!-- ");
				        break;

			        case DiagSeverity.Information:
			        default:
				        Trace.Write("--- ");
				        break;
		        }
	        }
	        catch (System.Exception)
	        {
	        }
        }


        /// <summary>
        /// Get a (non-localized) string containing a complete dump of an exception
        /// </summary>
        /// <param name="ex">Exception to dump</param>
        /// <returns>Non-localized string containing exception dump</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static String GetExceptionText(Exception ex)
        {
	        String exceptionText = String.Empty;
            try
            {
                if (ex != null)
                {
                    StringBuilder sb = new StringBuilder();
			        Type exType = ex.GetType();

                    // The full name of the exception.
                    sb.AppendLine(exType.FullName);

                    // Get properties.
                    PropertyInfo [] properties = exType.GetProperties();

			        foreach (PropertyInfo prop in properties)
                    {
                        // try to dump each property
                        try
                        {
                            sb.AppendFormat("{0}: {1}",
									        prop.Name,
									        prop.GetValue(ex, null));
                            sb.AppendLine();
                        }
                        catch (System.Exception){ }
                    }

                    // Get data (if any).
                    if (ex.Data != null)
                    {
                        if (ex.Data.Count > 0)
                        {
                            foreach (Object key in ex.Data.Keys)
                            {
                                sb.AppendFormat("Data: Key={0}, Value={1}",
										         key,
										         ex.Data[key]);
                                sb.AppendLine();
                            }
                        }
                    }

                    exceptionText += sb.ToString();

                    // Recurse through any inner exceptions.
                    if (ex.InnerException != null)
                    {
                        exceptionText += "\r\n\r\n";
                        exceptionText += GetExceptionText(ex.InnerException);
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing if this fails, don't even log it (we don't want to recurse)
            }

            return exceptionText;
        }
    }
}
