using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;

using StackHashUtilities;

namespace StackHashTasks
{
    public class LogManager : IDisposable
    {
        private bool m_IsRunningInService;
        private TextWriterTraceListener m_Listener;
        private String m_LogFileFolder;
        private static String s_StackHashServiceInApplicationDiagnosticsLog = "StackHashServiceInApplicationDiagnosticsLog.txt";
        private String m_TraceName;
        private static String s_ServiceTraceName = "StackHashService";
        private static String s_ServiceInApplicationTraceName = "StackHashServiceApplication";
        private static String s_LogFilePrefix = "StackHashServiceDiagnosticsLog_";
        private static String s_LogFileWildcard = s_LogFilePrefix + "*.txt";
        private int m_CurrentLogFileNumber;

        public String LogFileName
        {
            get { return makeLogFileName(m_LogFileFolder, m_CurrentLogFileNumber); }
        }

        /// <summary>
        /// Creates a log manager. 
        /// The log manager behaves slightly differently if running in the service than if running in an app.
        /// If running in a service, the service event log is also written to for errors.
        /// In test mode the log file is placed in the Test subfolder.
        /// </summary>
        /// <param name="isRunningInService">True - running in service. False - running in a normal app.</param>
        /// <param name="isTestMode">True - unit test mode. False - product mode.</param>
        public LogManager(bool isRunningInService, bool isTestMode)
        {
            // The service could be hosted in a Windows Service a standard forms application. 
            m_IsRunningInService = isRunningInService;

            if (m_IsRunningInService)
            {
                m_LogFileFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "StackHash");
                if (isTestMode)
                    m_LogFileFolder = Path.Combine(m_LogFileFolder, "Test");

                m_LogFileFolder = Path.Combine(m_LogFileFolder, "Logs");

                m_TraceName = s_ServiceTraceName;
            }
            else
            {
                m_LogFileFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
                if (isTestMode)
                    m_LogFileFolder = Path.Combine(m_LogFileFolder, "Test");
                m_LogFileFolder = Path.Combine(m_LogFileFolder, s_StackHashServiceInApplicationDiagnosticsLog);
                m_TraceName = s_ServiceInApplicationTraceName;
            }

            // Get the path to the log file. Make sure it is created.
            if (!Directory.Exists(m_LogFileFolder))
                Directory.CreateDirectory(m_LogFileFolder);

            m_CurrentLogFileNumber = findMostRecentLogFileNumber();
            if (m_CurrentLogFileNumber == 0)
                m_CurrentLogFileNumber = 1;
        }


        /// <summary>
        /// Make a log file name out of the path and file number.
        /// </summary>
        /// <param name="path">Folder where the log files reside.</param>
        /// <param name="logFileNumber">Log number.</param>
        /// <returns>Full path to the log file. </returns>
        private String makeLogFileName(String path, int logFileNumber)
        {
            StringBuilder result = new StringBuilder();
            result.Append(s_LogFilePrefix);
            result.Append(logFileNumber.ToString("D8", CultureInfo.InvariantCulture));
            result.Append(".txt");
            return Path.Combine(path, result.ToString());
        }


        /// <summary>
        /// Log files have the format:
        /// StackHashServiceDiagnosticsLog_N.log.
        /// The largest N is the latest log.
        /// </summary>
        /// <returns></returns>
        private int findMostRecentLogFileNumber()
        {
            String [] files = Directory.GetFiles(m_LogFileFolder, s_LogFileWildcard);

            int maxFileNumber = 0;
            foreach (String file in files)
            {
                String fileName = Path.GetFileNameWithoutExtension(file);
                String fileNumberString = fileName.Substring(s_LogFilePrefix.Length);
                int fileNumber = 0;

                if (!Int32.TryParse(fileNumberString, out fileNumber))
                    fileNumber = 0;

                if (fileNumber > maxFileNumber)
                    maxFileNumber = fileNumber;
            }
                
            return maxFileNumber;
        }
            

        /// <summary>
        /// Starts logging. From this point on, logged events (via DiagnosticsHelper.Log*) will be written to file.
        /// If the user strarts the logging then a new log file is created.
        /// </summary>
        /// <param name="userInvoked">True - user started logging. False - service started logging.</param>
        public void StartLogging(bool userInvoked)
        {
            // Don't start the listener if already started.
            if (m_Listener == null)
            {
                m_CurrentLogFileNumber = findMostRecentLogFileNumber();

                if (userInvoked || (m_CurrentLogFileNumber == 0))
                    m_CurrentLogFileNumber++;

                String logFileName = makeLogFileName(m_LogFileFolder, m_CurrentLogFileNumber);

                m_Listener = new TextWriterTraceListener(logFileName, m_TraceName);
                Trace.Listeners.Add(m_Listener);

                DiagnosticsHelper.LogApplicationStartup();
            }
        }


        /// <summary>
        /// Stop logging.
        /// </summary>
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

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">True - dispose called.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopLogging();
            }
        }

        /// <summary>
        /// Disposed the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
