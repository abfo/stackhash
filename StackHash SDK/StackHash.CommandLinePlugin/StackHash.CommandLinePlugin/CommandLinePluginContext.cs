using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashBugTrackerInterfaceV1;
using System.Collections.Specialized;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace StackHash.CommandLinePlugin
{
    /// <summary>
    /// A Context is object is created to manage the interface between StackHash and a particular instance 
    /// of the bug tracking database.
    /// StackHash will create one Context per StackHash Profile that lists this plug-in.
    /// A Context has its own settings which are persisted by StackHash and Set shortly after Context creation.
    /// Context creation is performed through the Control object CreateContext() method as opposed to direct 
    /// instantiation.
    /// The settings are defined by the plug-in developer. They are transported to the StackHash client as
    /// name/value pairs which are in no way interpretted by StackHash. 
    /// StackHash offers the StackHash client user the ability to change the settings.
    /// Additionally, the ContextDiagnostics property allows the plug-in to offer internal state information to
    /// help during plug-in development.
    /// </summary>
    public sealed class CommandLinePluginContext : MarshalByRefObject, IBugTrackerV1Context
    {
        #region Fields

        private CommandLinePluginControl m_Control; // Identifies the main DLL control interface.
        private Settings m_Settings; // strongly typed settings

        private int m_ScriptFailures = 0; // number of script call failures
        private int m_ProductsReported = 0; // number of products (new or updated) reported
        private int m_EventsReported = 0; // number of events (new or updated) reported
        private int m_EventNotesReported = 0; // number of event notes reported
        private int m_CabsReported = 0; // number of cabs (new or updated) reported
        private int m_CabNotesReported = 0; // number of cab notes reported
        private int m_DebugScriptsReported = 0; // number of debug script results reported
        private Exception m_LastException = null;  // The last recorded exception.

        private static readonly string s_Manual = "Manual";
        private static readonly string s_Automatic = "Automatic";

        /// <summary>
        /// Define the context specific diagnostics values here. You can define as many name value pairs as required 
        /// by the plug-in.
        /// The StackHash client will display all of the diagnostics that are returned by the context.
        /// These values are not persisted by StackHash and cannot be changed by the user in the StackHash client.
        /// An example is "LastException".
        /// </summary>
        private NameValueCollection m_ContextDiagnostics = new NameValueCollection();

        private static readonly string s_LastExceptionDiagnostic = "Last Exception";
        private static readonly string s_ScriptFailuresDiagnositc = "Script Call Failures";
        private static readonly string s_ProductsReportedDiagnostic = "Products Reported";
        private static readonly string s_EventsReportedDiagnostic = "Events Reported";
        private static readonly string s_EventNotesReportedDiagnostic = "Event Notes Reported";
        private static readonly string s_CabsReportedDiagnostic = "Cabs Reported";
        private static readonly string s_CabNotesReportedDiagnostic = "Cab Notes Reported";
        private static readonly string s_DebugScriptsReportedDiagnostic = "Debug Script Results Reported";

        #endregion

        #region Constructors

        /// <summary>
        /// A public constructor is required for serialization.
        /// </summary>
        public CommandLinePluginContext()
        {
        }

        /// <summary>
        /// Constructs a plug-in context to manage access to a particular instance of a bug tracking 
        /// database.
        /// </summary>
        /// <param name="controlInterface">Main plug-in control interface.</param>
        internal CommandLinePluginContext(CommandLinePluginControl controlInterface)
        {
            m_Control = controlInterface;

            // Initialise to the default settings for the plug-in.
            m_Settings = new Settings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Diagnostics specific to the particular plug-in context.
        /// If there are no context specific diagnostics you may want to return the main DLL diagnostics or perhaps
        /// return the DLL diagnostics + context diagnostics. 
        /// The StackHash client will display all of the diagnostics that are returned by the context.
        /// These values are not persisted by StackHash and cannot be changed by the user in the StackHash client.
        /// An example is "LastException" which might show the last exception that occurred in the context interface.
        /// </summary>
        public NameValueCollection ContextDiagnostics
        {
            get
            {
                m_ContextDiagnostics[s_LastExceptionDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0}", m_LastException);
                m_ContextDiagnostics[s_ScriptFailuresDiagnositc] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_ScriptFailures);
                m_ContextDiagnostics[s_ProductsReportedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_ProductsReported);
                m_ContextDiagnostics[s_EventsReportedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_EventsReported);
                m_ContextDiagnostics[s_EventNotesReportedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_EventNotesReported);
                m_ContextDiagnostics[s_CabsReportedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_CabsReported);
                m_ContextDiagnostics[s_CabNotesReportedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_CabNotesReported);
                m_ContextDiagnostics[s_DebugScriptsReportedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_DebugScriptsReported);

                return m_ContextDiagnostics;
            }
        }


        /// <summary>
        /// Gets the current context properties. Initially, these should be set to the DLL defaults. 
        /// StackHash will call SetSelectPropertyValues with the persisted settings shortly after context 
        /// creation.
        /// </summary>
        public NameValueCollection Properties
        {
            get
            {
                return m_Settings.ToNameValueCollection();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the context specific properties.
        /// StackHash will always set the properties with the persisted values just after the context is created.
        /// This may happen at service startup or when a profile is activated.
        /// </summary>
        /// <param name="propertiesToSet">Set of properties to change along with their new values.</param>
        public void SetSelectPropertyValues(NameValueCollection propertiesToSet)
        {
            if (propertiesToSet == null)
                throw new ArgumentNullException("propertiesToSet");

            m_Settings = Settings.FromNameValueCollection(propertiesToSet);
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a Product has been added to the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// If the report type is manual then this call indicates that a Product already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// Note that it is therefore possible that the product existence has already been reported, so the
        /// code here should check, if necessary, that this is not a duplicate before creating new items in 
        /// the destination bug tracking system.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product being added.</param>
        public void ProductAdded(BugTrackerReportType reportType, BugTrackerProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            // do nothing if a product added script has not been configured
            if (string.IsNullOrEmpty(m_Settings.ProductAddedScript))
                return;

            // do nothing for automatic reports in manual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                
                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.ProductAddedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();

                m_ProductsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call product added script: {0}",
                    ex));
            }
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a Product has been changed in the 
        /// StackHash database by way of a Synchronize with the WinQual web site. 
        /// The change may include the number of events stored has increased.
        ///
        /// This method is not currently called for manual reports.
        /// 
        /// Note that it is possible that an Updated is reported when no Added has been invoked. This can happen if the
        /// plug-in has been installed after the StackHash database has been created. Performing a manual Bug Report of
        /// the entire database when the plug-in is installed will limit the likelihood of this case.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product being updated.</param>
        public void ProductUpdated(BugTrackerReportType reportType, BugTrackerProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            // do nothing if a product added script has not been configured
            if (string.IsNullOrEmpty(m_Settings.ProductUpdatedScript))
                return;

            // do nothing for automatic reports in manual mode
            if (m_Settings.IsManualOnly)
                return;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.ProductUpdatedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();

                m_ProductsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call product updated script: {0}",
                    ex));
            }
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a File has been added to the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// If the report type is manual then this call indicates that a File already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// Files are mapped to products on the WinQual site. Note that the same file may be mapped to one
        /// or more products and therefore the same event (associated with a file) may "belong" refer to more 
        /// than one product.
        /// 
        /// Note that it is therefore possible that the file existence has already been reported, so the
        /// code here should check, if necessary, that this is not a duplicate before creating new items in 
        /// the destination bug tracking system.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file being added.</param>
        public void FileAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            // no action on file reports
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a File has been updated in the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// This method isn't currently called when performing a manual Bug Report.
        ///
        /// Files are mapped to products on the WinQual site. Note that the same file may be mapped to one
        /// or more products and therefore the same event (associated with a file) may "belong" refer to more 
        /// than one product.
        /// 
        /// Note that it is possible that an Updated is reported when no Added has been invoked. This can happen if the
        /// plug-in has been installed after the StackHash database has been created. Performing a manual Bug Report of
        /// the entire database when the plug-in is installed will limit the likelihood of this case.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file being updated.</param>
        public void FileUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            // no action on file reports
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that an Event has been added to the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// If the report type is manual then this call indicates that an Event already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// A Bug Reference is stored with the Event data in the StackHash database. This can be manually changed
        /// by the StackHash client user. The plug-in can also change a plugin bug reference by returning the desired 
        /// plugin bug reference from this call. 
        /// Return null if you do NOT want to change the plugin bug reference stored in the StackHash database.
        /// Return any other string (including an empty string) and this value will be used to overwrite the 
        /// plugin bug reference in the StackHash database.
        /// 
        /// Files are mapped to products on the WinQual site. Note that the same file may be mapped to one
        /// or more products and therefore the same event (associated with a file) may "belong" refer to more 
        /// than one product.
        /// 
        /// Note that it is therefore possible that the event existence has already been reported, so the
        /// code here should check, if necessary, that this is not a duplicate before creating new items in 
        /// the destination bug tracking system.
        /// 
        /// Important Note: AN EVENT IS IDENTIFIED BY ITS ID AND EVENTTYPENAME TOGETHER. Therefore it is possible to 
        /// have events with the same ID but different EventTypeName.
        /// 
        /// Note also that event IDs can be negative. This is a consequence of the WinQual site storing event numbers as
        /// 32 bit signed quantities. When the event ID range was exhausted the WinQual events went negative. The StackHash
        /// database stores event IDs as 64 bit signed quantities to future proof against any future changes in the 
        /// WinQual values.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event being added.</param>
        /// <returns>Null - if the plugin bug reference in the StackHash database should not be changed, Otherwise the new value for the plugin bug reference.</returns>
        public string EventAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // do nothing if an event added script hasn't been configured
            if (string.IsNullOrEmpty(m_Settings.EventAddedScript))
                return null;

            // do nothing on automatic reports if we're configured for manual only, or if the event hasn't hit the threshold
            if (reportType == BugTrackerReportType.Automatic)
            {
                if (m_Settings.IsManualOnly)
                    return null;

                if (theEvent.TotalHits < m_Settings.EventHitAddThreshold)
                    return null;
            }

            // Don't change the bug reference. Setting this to any other value will update the bug reference in the 
            // StackHash database.
            string pluginBugReference = null;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic: s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.BugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.TotalHits);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["applicationName"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["applicationVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["applicationTimeStamp"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["moduleName"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["moduleVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["moduleTimeStamp"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["exceptionCode"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["offset"]);

                processStartInfo.Arguments = argumentsBuilder.ToString();
                
                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.EventAddedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();
                int exitCode = scriptProcess.ExitCode;
                if (exitCode > 0)
                {
                    pluginBugReference = exitCode.ToString(CultureInfo.InvariantCulture);
                }

                m_EventsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call event added script: {0}",
                    ex));
            }

            return pluginBugReference;
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that an Event has been updated in the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// This method is not currently called during a manual BugReport.
        /// 
        /// A Bug Reference is stored with the Event data in the StackHash database. This can be manually changed
        /// by the StackHash client user. The plug-in can change the plugin bug reference by returning the desired 
        /// plugin bug reference from this call. 
        /// Return null if you do NOT want to change the plugin bug reference stored in the StackHash database.
        /// Return any other string (including an empty string) and this value will be used to overwrite the 
        /// plugin bug reference in the StackHash database.
        /// 
        /// Files are mapped to products on the WinQual site. Note that the same file may be mapped to one
        /// or more products and therefore the same event (associated with a file) may "belong" refer to more 
        /// than one product.
        /// 
        /// Note that it is possible that an Updated is reported when no Added has been invoked. This can happen if the
        /// plug-in has been installed after the StackHash database has been created. Performing a manual Bug Report of
        /// the entire database when the plug-in is installed will limit the likelihood of this case.
        /// 
        /// Important Note: AN EVENT IS IDENTIFIED BY ITS ID AND EVENTTYPENAME TOGETHER. Therefore it is possible to 
        /// have events with the same ID but different EventTypeName.
        /// 
        /// Note also that event IDs can be negative. This is a consequence of the WinQual site storing event numbers as
        /// 32 bit signed quantities. When the event ID range was exhausted the WinQual events went negative. The StackHash
        /// database stores event IDs as 64 bit signed quantities to future proof against any future changes in the 
        /// WinQual values.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event being added.</param>
        /// <returns>Null - if the plugin bug reference in the StackHash database should not be changed, Otherwise the new value for the plugin bug reference.</returns>
        public string EventUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // do nothing if an event updated script hasn't been configured
            if (string.IsNullOrEmpty(m_Settings.EventUpdatedScript))
                return null;

            // do nothing on automatic reports if we're configured for manual only, or if the event hasn't hit the threshold
            if (reportType == BugTrackerReportType.Automatic)
            {
                if (m_Settings.IsManualOnly)
                    return null;

                if (theEvent.TotalHits < m_Settings.EventHitAddThreshold)
                    return null;
            }


            // Don't change the bug reference. Setting this to any other value will update the bug reference in the 
            // StackHash database.
            String pluginBugReference = null;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.BugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.TotalHits);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["applicationName"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["applicationVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["applicationTimeStamp"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["moduleName"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["moduleVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["moduleTimeStamp"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["exceptionCode"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.Signature["offset"]);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.EventUpdatedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();
                int exitCode = scriptProcess.ExitCode;
                if (exitCode > 0)
                {
                    pluginBugReference = exitCode.ToString(CultureInfo.InvariantCulture);
                }

                m_EventsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call event updated script: {0}",
                    ex));
            }

            return pluginBugReference;
        }


        /// <summary>
        /// This method is called when the processing of an Event has completed. Events have associated Cabs, Scripts and notes which 
        /// may all be reported during a MANUAL Bug Report. This call effectively delimits the reporting of all
        /// information associated with an event so, if desired, the plug-in can gather all of the information together in one 
        /// go for sending to the fault database. This may improve performance for events with large amounts of stored data.
        /// 
        /// The plug-in may thus see... 
        ///    EventAdded 
        ///    EventNoteAdded, EventNoteAdded,... 
        ///    CabAdded, CabAdded,... 
        ///    CabNoteAdded, CabNoteAdded,... 
        ///    ScriptResultAdded, ScriptResultAdded,...
        ///    EventManualUpdateCompleted.
        ///    
        /// A Bug Reference is stored with the Event data in the StackHash database. This can be manually changed
        /// by the StackHash client user. The plug-in can also change a plugin bug reference by returning the desired 
        /// plugin bug reference from this call. 
        /// Return null if you do NOT want to change the plugin bug reference stored in the StackHash database.
        /// Return any other string (including an empty string) and this value will be used to overwrite the 
        /// plugin bug reference in the StackHash database.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event whose details have now been reported.</param>
        /// <returns>Null - if the plugin bug reference in the StackHash database should not be changed, Otherwise the new value for the plugin bug reference.</returns>
        public string EventManualUpdateCompleted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            // not used by this plugin
            return null;
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that an Event note has been added to the 
        /// StackHash database by a StackHash client.
        /// 
        /// If the report type is manual then this call indicates that an Event note already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// Note that it is possible that the event note existence has already been reported, so the
        /// code here should check, if necessary, that this is not a duplicate before creating new items in 
        /// the destination bug tracking system.
        /// 
        /// Important Note: AN EVENT IS IDENTIFIED BY ITS ID AND EVENTTYPENAME TOGETHER. Therefore it is possible to 
        /// have events with the same ID but different EventTypeName.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event to which the note belongs.</param>
        /// <param name="note">The event note to add.</param>
        public void EventNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerNote note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");

            // do nothing if an event note added script has not been configured
            if (string.IsNullOrEmpty(m_Settings.EventNoteAddedScript))
                return;

            // do nothing for automatic reports in manual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            string noteContentFilePath = null;
            try
            {
                noteContentFilePath = Path.GetTempFileName();
                File.WriteAllText(noteContentFilePath, note.Note);

                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", note.Source);
                argumentsBuilder.AppendFormat("\"{0}\" ", note.User);
                argumentsBuilder.AppendFormat("\"{0}\" ", note.TimeOfEntry);
                argumentsBuilder.AppendFormat("\"{0}\" ", noteContentFilePath);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.EventNoteAddedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();
                int exitCode = scriptProcess.ExitCode;

                m_EventNotesReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call event note added script: {0}",
                    ex));
            }
            finally
            {
                try
                {
                    File.Delete(noteContentFilePath);
                }
                catch { }
            }
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a Cab has been added to the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// If the report type is manual then this call indicates that an Cab already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// A Cab entry indicates that a cab is available on the WinQual site. The cab has not necessarily been
        /// downloaded yet. Once the cab is downloaded, the CabUpdated method will be called with Cab.CabDownloaded == true.
        /// 
        /// Cabs are associated with particular events (identified by EventId, EventTypeName).
        /// 
        /// Note that it is possible that the cab existence has already been reported, so the
        /// code here should check, if necessary, that this is not a duplicate before creating new items in 
        /// the destination bug tracking system.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event to which the cab belongs.</param>
        /// <param name="cab">The cab being added.</param>
        public void CabAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            // do nothing if a cab added script has not been configured
            if (string.IsNullOrEmpty(m_Settings.CabAddedScript))
                return;

            // do nothing for automatic reports in manual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabId);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.SizeInBytes);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabDownloaded);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabPurged);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["DotNetVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["MachineArchitecture"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["OSVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["ProcessUpTime"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["SystemUpTime"]);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.CabAddedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();

                m_CabsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call cab added script: {0}",
                    ex));
            }
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a Cab has been updated in the 
        /// StackHash database by way of: a Synchronize with the WinQual web site; downloading a cab manually from the
        /// StackHash client; or a script providing further diagnostic information.
        /// 
        /// This method is not currently called during a manual BugReport.
        /// 
        /// A Cab entry indicates that a cab is available on the WinQual site. The cab has not necessarily been
        /// downloaded yet. Once the cab is downloaded, the CabUpdated method will be called with Cab.CabDownloaded == true.
        /// 
        /// Cabs are associated with particular events (identified by EventId, EventTypeName).
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event to which the cab belongs.</param>
        /// <param name="cab">The cab being added.</param>
        public void CabUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            // do nothing if a cab updated script has not been configured
            if (string.IsNullOrEmpty(m_Settings.CabUpdatedScript))
                return;

            // do nothing for automatic reports in manual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabId);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.SizeInBytes);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabDownloaded);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabPurged);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["DotNetVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["MachineArchitecture"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["OSVersion"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["ProcessUpTime"]);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.AnalysisData["SystemUpTime"]);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.CabUpdatedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();

                m_CabsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call cab updated script: {0}",
                    ex));
            }
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a Cab Note has been added to the 
        /// StackHash database by way of the StackHash user adding one.
        /// 
        /// If the report type is manual then this call indicates that an Cab Note already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event to which the cab belongs.</param>
        /// <param name="cab">The cab to which the note belongs.</param>
        /// <param name="note">The cab note to add.</param>
        public void CabNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerNote note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");

            // do nothing if a cab note added script has not been configured
            if (string.IsNullOrEmpty(m_Settings.CabNoteAddedScript))
                return;

            // do nothing for automatic reports in manual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            string noteContentFilePath = null;
            try
            {
                noteContentFilePath = Path.GetTempFileName();
                File.WriteAllText(noteContentFilePath, note.Note);

                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabId);
                argumentsBuilder.AppendFormat("\"{0}\" ", note.Source);
                argumentsBuilder.AppendFormat("\"{0}\" ", note.User);
                argumentsBuilder.AppendFormat("\"{0}\" ", note.TimeOfEntry);
                argumentsBuilder.AppendFormat("\"{0}\" ", noteContentFilePath);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.CabNoteAddedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();
                int exitCode = scriptProcess.ExitCode;

                m_CabNotesReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call event note added script: {0}",
                    ex));
            }
            finally
            {
                try
                {
                    File.Delete(noteContentFilePath);
                }
                catch { }
            }
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that a Debug Script Result has been added to the 
        /// StackHash database by way the Cab Analysis task running or the StackHash client manually running a script
        /// on a cab.
        /// 
        /// If the report type is manual then this call indicates that an Debug Script Result already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// scriptResult.ToString() can be used to format the string for addition to a Fault Database system.
        /// 
        /// Automatic reports may arrived interleaved with manual reports. This may happen when, say a WinQual sync
        /// is happening at the same time as a manual report is requested. 
        /// </summary>
        /// <param name="reportType">Manual or automatic. If manual identifies what level of report is taking place.</param>
        /// <param name="product">The product to which the file belongs.</param>
        /// <param name="file">The file to which the event belongs.</param>
        /// <param name="theEvent">The event to which the cab belongs.</param>
        /// <param name="cab">The cab to which the debug script result belongs.</param>
        /// <param name="scriptResult">The result of running a debug script on the cab dump file.</param>
        public void DebugScriptExecuted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerScriptResult scriptResult)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (scriptResult == null)
                throw new ArgumentNullException("scriptResult");

            // do nothing if a script result added script has not been configured
            if (string.IsNullOrEmpty(m_Settings.DebugScriptExecutedScript))
                return;

            // do nothing for automatic reports in manual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            string scriptContentFilePath = null;
            try
            {
                scriptContentFilePath = Path.GetTempFileName();
                File.WriteAllText(scriptContentFilePath, scriptResult.ScriptResults);

                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.AppendFormat("\"{0}\" ", reportType == BugTrackerReportType.Automatic ? s_Automatic : s_Manual);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductId);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductName);
                argumentsBuilder.AppendFormat("\"{0}\" ", product.ProductVersion);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventId);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.EventTypeName);
                argumentsBuilder.AppendFormat("\"{0}\" ", theEvent.PlugInBugReference);
                argumentsBuilder.AppendFormat("\"{0}\" ", cab.CabId);
                argumentsBuilder.AppendFormat("\"{0}\" ", scriptResult.Name);
                argumentsBuilder.AppendFormat("\"{0}\" ", scriptResult.RunDate);
                argumentsBuilder.AppendFormat("\"{0}\" ", scriptContentFilePath);

                processStartInfo.Arguments = argumentsBuilder.ToString();

                processStartInfo.ErrorDialog = false;
                processStartInfo.FileName = m_Settings.DebugScriptExecutedScript;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process scriptProcess = Process.Start(processStartInfo);
                scriptProcess.WaitForExit();
                int exitCode = scriptProcess.ExitCode;

                m_DebugScriptsReported++;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                m_ScriptFailures++;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to call debug script executed script: {0}",
                    ex));
            }
            finally
            {
                try
                {
                    File.Delete(scriptContentFilePath);
                }
                catch { }
            }
        }


        #endregion Methods
    }
}
