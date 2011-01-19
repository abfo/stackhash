using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashBugTrackerInterfaceV1;
using System.Collections.Specialized;
using System.Globalization;

namespace StackHash.FogBugzPlugin
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
    public sealed class FogBugzPluginContext : MarshalByRefObject, IBugTrackerV1Context
    {
        #region Fields

        private FogBugzPluginControl m_Control; // Identifies the main DLL control interface.
        private Exception m_LastException = null;  // The last recorded exception.
        private int m_ApiCallsFailed = 0; // number of api call failures
        private int m_CasesAdded = 0; // number of cases added
        private int m_CabsAdded = 0; // number of cabs added
        private int m_EventNotesAdded = 0; // event notes added
        private int m_CabNotesAdded = 0; // cab notes added
        private int m_ScriptResultsAdded = 0; // script results added
        private Settings m_Settings; // strongly typed settings
        private FogBugzApi m_FogBugzApi; // FogBugz API wrapper

        /// <summary>
        /// Define the context specific diagnostics values here. You can define as many name value pairs as required 
        /// by the plug-in.
        /// The StackHash client will display all of the diagnostics that are returned by the context.
        /// These values are not persisted by StackHash and cannot be changed by the user in the StackHash client.
        /// An example is "LastException".
        /// </summary>
        private NameValueCollection m_ContextDiagnostics = new NameValueCollection();

        private static readonly string s_LastExceptionDiagnostic = "Last Exception";
        private static readonly string s_FogBugzApiCallsFailedDiagnostic = "FogBugz API Call Failures";
        private static readonly string s_CasesAddedDiagnostic = "Cases Added";
        private static readonly string s_CabsAddedDiagnostic = "Cabs Added";
        private static readonly string s_EventNotesAddedDiagnostic = "Event Notes Added";
        private static readonly string s_CabNotesAddedDiagnostic = "Cab Notes Added";
        private static readonly string s_ScriptResultsAddedDiagnostic = "Script Results Added";

        #endregion

        #region Constructors

        /// <summary>
        /// A public constructor is required for serialization.
        /// </summary>
        public FogBugzPluginContext()
        {
        }

        /// <summary>
        /// Constructs a plug-in context to manage access to a particular instance of a bug tracking 
        /// database.
        /// </summary>
        /// <param name="controlInterface">Main plug-in control interface.</param>
        internal FogBugzPluginContext(FogBugzPluginControl controlInterface)
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
                m_ContextDiagnostics[s_FogBugzApiCallsFailedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_ApiCallsFailed);
                m_ContextDiagnostics[s_CasesAddedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_CasesAdded);
                m_ContextDiagnostics[s_CabsAddedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_CabsAdded);
                m_ContextDiagnostics[s_EventNotesAddedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_EventNotesAdded);
                m_ContextDiagnostics[s_CabNotesAddedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_CabNotesAdded);
                m_ContextDiagnostics[s_ScriptResultsAddedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_ScriptResultsAdded);

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
        /// Logs out of the FogBugz API (if connected)
        /// </summary>
        public void FogBugzApiLogout()
        {
            try
            {
                if (m_FogBugzApi != null)
                {
                    m_FogBugzApi.Logout();
                    m_FogBugzApi = null;
                }
            }
            catch
            {
                // ignore logout failures, we'll be creating a new connection if needed
            }
        }

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

            // logout of FogBugz if we're logged in - credentials / API URL may have been changed
            FogBugzApiLogout();
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
            // we don't care about product reports
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
            // we don't care about product reports
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
            // we don't care about file reports
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
            // we don't care about file reports
        }

        // combined logic for added and updated events
        private string EventAddedOrUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

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
                RecycleFogBugzApi();

                // see if there's an existing case - we'll get -1 if not
                int existingCaseNumber = ParseCaseNumber(theEvent.PlugInBugReference);
                if (existingCaseNumber == -1)
                {
                    // case needs to be added
                    string title = string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.CaseTitle_Template,
                        theEvent.EventId,
                        theEvent.EventTypeName,
                        product.ProductName,
                        product.ProductVersion);

                    StringBuilder signatureBuilder = new StringBuilder();
                    if (theEvent.Signature != null)
                    {
                        foreach (string key in theEvent.Signature.Keys)
                        {
                            signatureBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}: {1}", key, theEvent.Signature[key]);
                            signatureBuilder.AppendLine();
                        }
                    }

                    string text = string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.CaseText_Template,
                        theEvent.TotalHits,
                        signatureBuilder);

                    int caseNumber = m_FogBugzApi.AddCase(title, text, m_Settings.FogBugzProject, m_Settings.FogBugzArea);

                    // set pluginBugReference to update the database with the new case
                    pluginBugReference = string.Format(CultureInfo.InvariantCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.Case_Template,
                        caseNumber);

                    m_CasesAdded++;
                }
                else
                {
                    // case needs to be updated
                    m_FogBugzApi.AddNoteToCase(existingCaseNumber, string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.EventUpdated_Template,
                        theEvent.TotalHits));
                }
            }
            catch (Exception ex)
            {
                m_ApiCallsFailed++;
                m_LastException = ex;

                // log the error
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to add or update FogBugz case: {0}",
                    ex));

                // try to logout of FogBugz on error
                FogBugzApiLogout();
            }

            return pluginBugReference;
        }

        /// <summary>
        /// If the report type is automatic then this call indicates that an Event has been added to the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// If the report type is manual then this call indicates that an Event already exists in the 
        /// StackHash database. This is the result of a BugReport task being run.
        /// 
        /// A Bug Reference is stored with the Event data in the StackHash database. This can be manually changed
        /// by the StackHash client user. The plug-in can change a plugin bug reference by returning the desired 
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
            return EventAddedOrUpdated(reportType, product, file, theEvent);
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that an Event has been updated in the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// This method is not currently called during a manual BugReport.
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
            return EventAddedOrUpdated(reportType, product, file, theEvent);
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
            // we report each piece of event data individually
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

            // do nothing on automatic reports if we're configured for manual only
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            // see if there's an existing case - we'll get -1 if not
            int existingCaseNumber = ParseCaseNumber(theEvent.PlugInBugReference);
            if (existingCaseNumber >= 0)
            {
                try
                {
                    RecycleFogBugzApi();
                    
                    // add the note
                    m_FogBugzApi.AddNoteToCase(existingCaseNumber, string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.EventNote_Template,
                        note.User,
                        note.TimeOfEntry.ToLongTimeString(),
                        note.TimeOfEntry.ToShortDateString(),
                        note.Source,
                        note.Note));

                    m_EventNotesAdded++;
                }
                catch (Exception ex)
                {
                    m_ApiCallsFailed++;
                    m_LastException = ex;

                    // log the error
                    BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                        m_Control.PlugInName,
                        string.Format(CultureInfo.InvariantCulture,
                        "Failed to add event note to FogBugz case: {0}",
                        ex));

                    // try to logout of FogBugz on error
                    FogBugzApiLogout();
                }
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

            // do nothing on automatic reports if we're configured for manual only
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            // see if there's an existing case - we'll get -1 if not
            int existingCaseNumber = ParseCaseNumber(theEvent.PlugInBugReference);
            if (existingCaseNumber >= 0)
            {
                try
                {
                    RecycleFogBugzApi();

                    // build a string from the cab analysis
                    StringBuilder cabAnalysisBuilder = new StringBuilder();
                    if (cab.AnalysisData != null)
                    {
                        foreach (string key in cab.AnalysisData.Keys)
                        {
                            cabAnalysisBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}: {1}", key, cab.AnalysisData[key]);
                            cabAnalysisBuilder.AppendLine();
                        }
                    }

                    // report the cab
                    m_FogBugzApi.AddNoteToCase(existingCaseNumber, string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.CabAdded_Template,
                        cab.CabId,
                        cab.CabDownloaded,
                        cab.CabPurged,
                        GetSizeString(cab.SizeInBytes),
                        cabAnalysisBuilder));

                    m_CabsAdded++;
                }
                catch (Exception ex)
                {
                    m_ApiCallsFailed++;
                    m_LastException = ex;

                    // log the error
                    BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                        m_Control.PlugInName,
                        string.Format(CultureInfo.InvariantCulture,
                        "Failed to add cab information to FogBugz case: {0}",
                        ex));

                    // try to logout of FogBugz on error
                    FogBugzApiLogout();
                }
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

            // do nothing on automatic reports if we're configured for manual only
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            // see if there's an existing case - we'll get -1 if not
            int existingCaseNumber = ParseCaseNumber(theEvent.PlugInBugReference);
            if (existingCaseNumber >= 0)
            {
                try
                {
                    RecycleFogBugzApi();

                    // build a string from the cab analysis
                    StringBuilder cabAnalysisBuilder = new StringBuilder();
                    if (cab.AnalysisData != null)
                    {
                        foreach (string key in cab.AnalysisData.Keys)
                        {
                            cabAnalysisBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}: {1}", key, cab.AnalysisData[key]);
                            cabAnalysisBuilder.AppendLine();
                        }
                    }

                    // report the updated cab
                    m_FogBugzApi.AddNoteToCase(existingCaseNumber, string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.CabUpdated_Template,
                        cab.CabId,
                        cab.CabDownloaded,
                        cab.CabPurged,
                        GetSizeString(cab.SizeInBytes),
                        cabAnalysisBuilder));
                }
                catch (Exception ex)
                {
                    m_ApiCallsFailed++;
                    m_LastException = ex;

                    // log the error
                    BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                        m_Control.PlugInName,
                        string.Format(CultureInfo.InvariantCulture,
                        "Failed to add updated cab information to FogBugz case: {0}",
                        ex));

                    // try to logout of FogBugz on error
                    FogBugzApiLogout();
                }
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

            // do nothing on automatic reports if we're configured for manual only
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            // see if there's an existing case - we'll get -1 if not
            int existingCaseNumber = ParseCaseNumber(theEvent.PlugInBugReference);
            if (existingCaseNumber >= 0)
            {
                try
                {
                    RecycleFogBugzApi();

                    // add the note
                    m_FogBugzApi.AddNoteToCase(existingCaseNumber, string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.CabNote_Template,
                        note.User,
                        cab.CabId,
                        note.TimeOfEntry.ToLongTimeString(),
                        note.TimeOfEntry.ToShortDateString(),
                        note.Source,
                        note.Note));

                    m_CabNotesAdded++;
                }
                catch (Exception ex)
                {
                    m_ApiCallsFailed++;
                    m_LastException = ex;

                    // log the error
                    BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                        m_Control.PlugInName,
                        string.Format(CultureInfo.InvariantCulture,
                        "Failed to add cab note to FogBugz case: {0}",
                        ex));

                    // try to logout of FogBugz on error
                    FogBugzApiLogout();
                }
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

            // do nothing on automatic reports if we're configured for manual only
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
                return;

            // see if there's an existing case - we'll get -1 if not
            int existingCaseNumber = ParseCaseNumber(theEvent.PlugInBugReference);
            if (existingCaseNumber >= 0)
            {
                try
                {
                    RecycleFogBugzApi();

                    // add the results
                    m_FogBugzApi.AddNoteToCase(existingCaseNumber, string.Format(CultureInfo.CurrentCulture,
                        StackHash.FogBugzPlugin.Properties.Resources.ScriptResults_Template,
                        scriptResult.Name,
                        scriptResult.ScriptVersion,
                        cab.CabId,
                        scriptResult.RunDate.ToLongDateString(),
                        scriptResult.RunDate.ToShortTimeString(),
                        scriptResult.ScriptResults));

                    m_ScriptResultsAdded++;
                }
                catch (Exception ex)
                {
                    m_ApiCallsFailed++;
                    m_LastException = ex;

                    // log the error
                    BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                        m_Control.PlugInName,
                        string.Format(CultureInfo.InvariantCulture,
                        "Failed to add script results to FogBugz case: {0}",
                        ex));

                    // try to logout of FogBugz on error
                    FogBugzApiLogout();
                }
            }
        }

        /// <summary>
        /// Creates and logs into the FogBugz API if needed
        /// </summary>
        private void RecycleFogBugzApi()
        {
            // create API if needed
            if (m_FogBugzApi == null)
            {
                m_FogBugzApi = new FogBugzApi(m_Settings.FogBugzApiUri, m_Settings.FogBugzUsername, m_Settings.FogBugzPassword);
            }
        }

        /// <summary>
        /// Parses a case number from a bug reference - assumes that any
        /// numbers in the bug reference are the case number and discards other characters
        /// </summary>
        /// <param name="bugReference">The current bug reference</param>
        /// <returns>Case number, returns -1 if a number could not be found</returns>
        private int ParseCaseNumber(string bugReference)
        {
            int caseNumber = -1;

            if (!string.IsNullOrEmpty(bugReference))
            {
                // extract digits
                StringBuilder numbersOnly = new StringBuilder(bugReference.Length);
                for (int i = 0; i < bugReference.Length; i++)
                {
                    char c = bugReference[i];
                    if (char.IsNumber(c))
                    {
                        numbersOnly.Append(c);
                    }
                }

                // try to convert to an int
                if (!Int32.TryParse(numbersOnly.ToString(), out caseNumber))
                {
                    caseNumber = -1;
                }
            }

            return caseNumber;
        }

        /// <summary>
        /// Gets a display string for a size in bytes
        /// </summary>
        /// <param name="sizeInBytes">Size in bytes</param>
        /// <returns>Display String</returns>
        private string GetSizeString(long sizeInBytes)
        {
            string postfix = StackHash.FogBugzPlugin.Properties.Resources.Size_KB;

            double size = sizeInBytes / 1024;

            // test for MB
            if (size >= 1024.0)
            {
                size /= 1024;
                postfix = StackHash.FogBugzPlugin.Properties.Resources.Size_MB;
            }

            // test for GB
            if (size >= 1024.0)
            {
                size /= 1024;
                postfix = StackHash.FogBugzPlugin.Properties.Resources.Size_GB;
            }

            return string.Format(CultureInfo.CurrentCulture,
                "{0:n2} {1}",
                size,
                postfix);
        }

        #endregion Methods
    }
}
