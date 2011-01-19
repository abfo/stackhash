using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackHashBugTrackerInterfaceV1;
using System.Collections.Specialized;
using System.Globalization;
using System.Resources;


namespace StackHash.EmailPlugin
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
    public sealed class EmailPluginContext : MarshalByRefObject, IBugTrackerV1Context, IDisposable
    {
        #region Fields

        private EmailPluginControl m_Control; // Identifies the main DLL control interface.

        private Exception m_LastException = null;  // The last recorded exception.
        private int m_EmailsSent = 0; // Number of emails sent
        private int m_FailedEmails = 0; // Number of failed emails
        private bool m_disposed; // True if object has been disposed

        /// <summary>
        /// Define the context specific diagnostics values here. You can define as many name value pairs as required 
        /// by the plug-in.
        /// The StackHash client will display all of the diagnostics that are returned by the context.
        /// These values are not persisted by StackHash and cannot be changed by the user in the StackHash client.
        /// An example is "LastException".
        /// </summary>
        private NameValueCollection m_ContextDiagnostics = new NameValueCollection();

        private static readonly string s_LastExceptionDiagnostic = "LastException";
        private static readonly string s_EmailsSentDiagnostic = "Emails Sent";
        private static readonly string s_EmailsFailedDiagnostic = "Failed Emails";

        /// <summary>
        /// Plugin settings (wraps the NameValueCollection provided by StackHash in a strongly typed object)
        /// </summary>
        private Settings m_Settings;

        #endregion

        #region Constructors

        /// <summary>
        /// A public constructor is required for serialization.
        /// </summary>
        public EmailPluginContext()
        {
            m_Settings = new Settings();
        }

        /// <summary />
        ~EmailPluginContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Constructs a plug-in context to manage access to a particular instance of a bug tracking 
        /// database.
        /// </summary>
        /// <param name="controlInterface">Main plug-in control interface.</param>
        internal EmailPluginContext(EmailPluginControl controlInterface)
            : this()
        {
            m_Control = controlInterface;
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
                if (m_disposed)
                    throw new ObjectDisposedException("EmailPluginContext");

                m_ContextDiagnostics[s_LastExceptionDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0}", m_LastException);
                m_ContextDiagnostics[s_EmailsSentDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_EmailsSent);
                m_ContextDiagnostics[s_EmailsFailedDiagnostic] = string.Format(CultureInfo.CurrentCulture, "{0:n0}", m_FailedEmails);

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
                if (m_disposed)
                    throw new ObjectDisposedException("EmailPluginContext");

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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (propertiesToSet == null)
                throw new ArgumentNullException("propertiesToSet");

            if (m_Settings != null)
            {
                m_Settings.Dispose();
                m_Settings = null;
            }

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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");

            // report new products during sync (not during manual reporting) if configured to in settings
            if ((reportType == BugTrackerReportType.Automatic) &&
                (m_Settings.ReportProducts) &&
                (!m_Settings.IsManualOnly))
            {
                try
                {
                    string subject = string.Format(CultureInfo.CurrentCulture,
                        StackHash.EmailPlugin.Properties.Resources.ProductAdded_SubjectTemplate,
                        product.ProductName,
                        product.ProductVersion);

                    string message = string.Format(CultureInfo.CurrentCulture,
                        StackHash.EmailPlugin.Properties.Resources.ProductAdded_MessageTemplate,
                        product.ProductName,
                        product.ProductVersion,
                        product.ProductId);

                    m_Settings.SendEmail(subject, message);
                    m_EmailsSent++;
                }
                catch (Exception ex)
                {
                    BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                        m_Control.PlugInName,
                        string.Format(CultureInfo.InvariantCulture,
                        "Failed to send new product email: {0}",
                        ex));

                    m_FailedEmails++;
                    m_LastException = ex;
                }
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // do nothing on an automatic report if we're in manaual mode
            if ((reportType == BugTrackerReportType.Automatic) && (m_Settings.IsManualOnly))
            {
                return null;
            }

            string subjectTemplate = null;
            string messageTemplate = null;

            if (reportType == BugTrackerReportType.Automatic)
            {
                subjectTemplate = StackHash.EmailPlugin.Properties.Resources.EventAdded_SubjectTemplate;
                messageTemplate = StackHash.EmailPlugin.Properties.Resources.EventAdded_MessageTemplate;
            }
            else
            {
                subjectTemplate = StackHash.EmailPlugin.Properties.Resources.EventManualReport_SubjectTemplate;
                messageTemplate = StackHash.EmailPlugin.Properties.Resources.EventManaulReport_MessageTemplate;
            }

            // build a string containing all event signature parameters
            StringBuilder eventSignatureBuilder = new StringBuilder();
            foreach (string key in theEvent.Signature.Keys)
            {
                eventSignatureBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} = {1}", key, theEvent.Signature[key]);
                eventSignatureBuilder.AppendLine();
                eventSignatureBuilder.AppendLine();
            }

            try
            {
                string subject = string.Format(CultureInfo.CurrentCulture,
                    subjectTemplate,
                    product.ProductName,
                    product.ProductVersion,
                    theEvent.EventId);

                string message = string.Format(CultureInfo.CurrentCulture,
                    messageTemplate,
                    product.ProductName,
                    product.ProductVersion,
                    theEvent.EventId,
                    theEvent.EventTypeName,
                    theEvent.TotalHits,
                    eventSignatureBuilder);

                m_Settings.SendEmail(subject, message);
                m_EmailsSent++;
            }
            catch (Exception ex)
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.ComponentFatal,
                    m_Control.PlugInName,
                    string.Format(CultureInfo.InvariantCulture,
                    "Failed to send event email: {0}",
                    ex));

                m_LastException = ex;
                m_FailedEmails++;
            }

            // Don't change the plugin bug reference. Setting this to any other value will update the plugin bug reference in the 
            // StackHash database.
            string pluginBugReference = null;
            return pluginBugReference;
        }


        /// <summary>
        /// If the report type is automatic then this call indicates that an Event has been updated in the 
        /// StackHash database by way of a Synchronize with the WinQual web site.
        /// 
        /// This method is not currently called during a manual BugReport.
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Don't change the plugin bug reference. Setting this to any other value will update the plugin bug reference in the 
            // StackHash database.
            String pluginBugReference = null;
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
        /// by the StackHash client user. The plug-in can change a plugin bug reference by returning the desired 
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            // Don't change the plugin bug reference. Setting this to any other value will update the plugin bug reference in the 
            // StackHash database.
            String pluginBugReference = null;
            return pluginBugReference;
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

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
            if (m_disposed)
                throw new ObjectDisposedException("EmailPluginContext");

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
        }


        #endregion Methods

        #region IDisposable Members

        /// <summary />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool canDisposeManagedResources)
        {
            if (!m_disposed)
            {
                if (canDisposeManagedResources)
                {
                    if (m_Settings != null)
                    {
                        m_Settings.Dispose();
                        m_Settings = null;
                    }
                }

                m_disposed = true;
            }
        }

        #endregion
    }
}