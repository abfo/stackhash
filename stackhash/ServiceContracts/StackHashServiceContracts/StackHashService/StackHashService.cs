using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using StackHashBusinessObjects;
using StackHashTasks;
using StackHashServiceImplementation;
using StackHashUtilities;


namespace StackHashService
{
    /// <summary>
    /// The service acts as a glue layer between the ServiceContract implementation and the Business logic.
    /// A static Controller is created. This is the main entry point required by the ServiceImplementation. 
    /// The ServiceImplementation gets to the Controller object through a StaticObjects object.
    /// </summary>
    public partial class StackHashService : ServiceBase
    {
        [SuppressMessage("Microsoft.Performance", "CA1823", Justification = "Only ever instantiated - never used here")]
        StaticObjects m_StaticObjects;


        /// <summary>
        /// Dump the error to the log.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Error info.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        [SuppressMessage("Microsoft.Design", "CA1062")]
        public void UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal, "Unhandled exception", (System.Exception)e.ExceptionObject);
        }



        public StackHashService()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            InitializeComponent();
        }


        /// <summary>
        /// Called when the server is first started.
        /// The WCF services are registered with the system.
        /// This thread must exit quickly to avoid the Service Control Manager (SCM) timing out.
        /// Note that the WCF service registration creates a thread which keeps the service alive.
        /// </summary>
        /// <param name="args">arg[0] = time to pause in milliseconds - allows debugger attach (default = 0).</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        protected override void OnStart(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length > 0)
            {
                // The first parameter exists so assume it is the time to wait.
                // This allows a debugger to be attached to the process early on in the debug cycle.
                int timeToWait = 0;
                try
                {
                    timeToWait = int.Parse(args[0], CultureInfo.InvariantCulture);
                }
                catch (System.FormatException)
                {
                    // Just ignore format exceptions and assume that no value was specified.
                }

                if (timeToWait != 0)
                    Thread.Sleep(timeToWait);
            }


            try
            {
                // Tell the diagnostics helper where to log system events to.
                DiagnosticsHelper.SetEventLog(EventLog);

                // Cater for unhandled exceptions. Do this after the event log has been set up.
                // Load in the settings from the application directory if there are any.
                string pathForSystem = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string pathForServiceSettings = pathForSystem + "\\StackHash";
                string pathForTestServiceSettings = pathForServiceSettings + "\\Test";

                if (!Directory.Exists(pathForServiceSettings))
                    Directory.CreateDirectory(pathForServiceSettings);

                string testModeFileName = pathForServiceSettings + "\\testmode.xml";
                string testSettingsFileName = pathForTestServiceSettings + "\\testsettings.xml";
                string settingsFileName = pathForServiceSettings + "\\settings.xml";

                // Now initialise the controller with those settings.
                if (File.Exists(testModeFileName) && (TestSettings.TestMode == "1"))
                {
                    if (!Directory.Exists(pathForTestServiceSettings))
                        Directory.CreateDirectory(pathForTestServiceSettings);

                    m_StaticObjects = new StaticObjects(testSettingsFileName, true, true);
                }
                else
                {
                    m_StaticObjects = new StaticObjects(settingsFileName, true, false);
                }

                m_StaticObjects.EnableServices();
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.ApplicationFatal, "Failed to start Stack Hash service " + ex.ToString());
                throw;
            }
        }


        /// <summary>
        /// Called to stop the service. The controller is disposed.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (m_StaticObjects != null)
                {
                    m_StaticObjects.Dispose();
                    m_StaticObjects = null;
                }
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.ApplicationFatal, "Failed to stop Stack Hash service " + ex.ToString());
                throw;
            }

            AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(UnhandledException);

        }
    }
}
