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
    /// StackHash creates one FaultDatabaseControl object when the plug-in is loaded. 
    /// The Control object is responsible for managing Contexts. 
    /// The Control object reports the default properties for a context and general diagnostics 
    /// regarding the load of the plug-in.
    /// PlugInDefaultProperties and PlugInDiagnostics are not interpretted or persisted by StackHash.
    /// PlugInDefaultProperties offers a means for StackHash to present the settings to the StackHash client 
    /// user when a plug-in is assigned to a StackHash profile.
    /// 
    /// If an exception is thrown by ANY of the plug-in interface methods to properties then the plug-in
    /// is placed in a faulted state and no further calls will be placed to the DLL Add and Update 
    /// methods. The StackHash profile will need to be deactivated and reactivated to continue reporting.
    /// In the event that the DLL does fault, any new report notifications will be stored by StackHash 
    /// until the fault is fixed or a full manual sync report is initiated by the user.
    /// </summary>
    public sealed class FogBugzPluginControl : MarshalByRefObject, IBugTrackerV1Control
    {
        #region Fields

        private static readonly string s_PlugInName = StackHash.FogBugzPlugin.Properties.Resources.Plugin_Name;
        private static readonly string s_PlugInDescription = StackHash.FogBugzPlugin.Properties.Resources.Plugin_Description;
        private static readonly Uri s_HelpUrl = new Uri("http://www.stackhash.com/Help/stackhash-fogbugz-plugin.aspx");

        private Exception m_LastException = null;  // The last recorded exception.

        // Keep a track of the number of context instances that are active.
        private List<FogBugzPluginContext> m_ActiveContexts = new List<FogBugzPluginContext>();

        /// <summary>
        /// Define the diagnostics values here. You can define as many name value pairs as required by the plug-in.
        /// The StackHash client will display all of the diagnostics that are returned by the plug-in.
        /// These values are not persisted by StackHash and cannot be changed by the user in the StackHash client.
        /// Examples are "LastException" and "NumberOfActiveContexts".
        /// </summary>
        private NameValueCollection m_Diagnostics = new NameValueCollection();

        private static readonly string s_LastExceptionDiagnostic = "Last Exception";
        private static readonly string s_NumberOfActiveContextsDiagnostic = "Active Contexts";


        #endregion

        #region Constructors

        /// <summary>
        /// A public default constructor must be defined.
        /// </summary>
        public FogBugzPluginControl()
        {
            // You can use the static LogMessage to log a message to the StackHash service log file which is written to 
            // c:\ProgramData\StackHash on Vista and Win7 and to c:\Documents and Settings\All Users\Application Data\StackHash 
            // on XP systems.
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "FogBugzPluginControl Interface created");
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// This must be unique across all plug-ins. A good name would be a well known abbreviation for the 
        /// bug tracking system that this DLL interfaces to. e.g. Fogbugz, Jira etc... If you have, or may have, 
        /// multiple DLLs that deal with the same bug tracking system then choose a more unique name.
        /// </summary>
        public string PlugInName
        {
            get
            {
                return s_PlugInName;
            }
        }


        /// <summary>
        /// A description of the plug-ins purpose.
        /// </summary>
        public String PlugInDescription
        {
            get
            {
                return s_PlugInDescription;
            }
        }


        /// <summary>
        /// The help Url will be displayed in the StackHash client so that users of the plug-in can get more
        /// help related to the plug-in usage, properties and the meaning of diagnostics.
        /// </summary>
        public Uri HelpUrl
        {
            get
            {
                return s_HelpUrl;
            }
        }


        /// <summary>
        /// Indicate here whether the plug-in requires feedback to update the bug reference field in StackHash.
        /// You should return false here if the plug-in does not change the bug reference in StackHash. If this 
        /// is set to true, the EventUpdated and EventAdded can choose to return a bug reference which is stored
        /// in the StackHash database along with the event data.
        /// Attempting to assign more than one plug-in to a context, both of which set this field to true will
        /// result in failure of the profile to activate. i.e. only one plug-in can be loaded that sets the bug
        /// reference at any time.
        /// </summary>
        public bool PlugInSetsBugReference
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Default settings for the plug-in. The StackHash client will initially use these settings
        /// when assigning a plug-in for use by a service profile.
        /// The defaults can be any string values. These values will be potentially changed by the user at the 
        /// StackHash client so endeavour to make the settings intuitive and easy to change.
        /// When it comes to using properties, you want to allow upper/lower case or short variants. e.g. False, false, F, 0 
        /// all might mean the same thing.
        /// Note that StackHash does not interpret or otherwise use plug-in property settings except to display them in the 
        /// StackHash client and to persist them for use after a service restart.
        /// </summary>
        public NameValueCollection PlugInDefaultProperties
        {
            get
            {
                return Settings.GetDefaults();
            }
        }


        /// <summary>
        /// The plug-in diagnostics are specific to the plug-in but are displayed at the StackHash client
        /// to aid plug-in development. 
        /// The diagnostics are not changeable by the user, they merely report the situation in the DLL at 
        /// present.
        /// The PlugInDiagnostics should report general global information about the state of the plug-in, 
        /// e.g. the number of Context's that are active and any failure information.
        /// Examples might be "LastException" and "NumberOfActiveContexts".
        /// StackHash does not persist diagnostics.
        /// </summary>
        public NameValueCollection PlugInDiagnostics
        {
            get
            {
                // Add other diagnostics here.
                m_Diagnostics[s_LastExceptionDiagnostic] = string.Format(CultureInfo.InvariantCulture, "{0}", m_LastException);
                m_Diagnostics[s_NumberOfActiveContextsDiagnostic] = m_ActiveContexts.Count.ToString(CultureInfo.InvariantCulture);

                return m_Diagnostics;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Called by the StackHash client to create a context. The StackHash client will not directly 
        /// instantiate an instance of the Context class. This gives the plug-in the opportunity to 
        /// reject instance creation.
        /// A context will have its own settings.
        /// The context will be created by StackHash when the StackHash profile is activated. This will be 
        /// on service startup or may happen when the user manually activates the profile at the StackHash
        /// client.
        /// </summary>
        /// <returns>Reference to the created instance.</returns>
        public IBugTrackerV1Context CreateContext()
        {
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Creating FogBugzPluginContext");

            FogBugzPluginContext newContext = new FogBugzPluginContext(this);
            m_ActiveContexts.Add(newContext);
            return newContext;
        }


        /// <summary>
        /// Releases a context that is no longer required.
        /// The context will be released by StackHash when the StackHash profile is deactivated. This will be 
        /// on service closedown or may happen when the user manually deactivates the profile at the StackHash
        /// client.
        /// Once a context has been released, StackHash will no longer attempt to invoke it.
        /// </summary>
        /// <param name="context">The context to release.</param>
        public void ReleaseContext(IBugTrackerV1Context context)
        {
            FogBugzPluginContext fogBugzPluginContext = context as FogBugzPluginContext;
            if (context != null)
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Releasing FogBugzPluginContext");
                m_ActiveContexts.Remove(fogBugzPluginContext);

                // logout of FogBugz (if we logged in)
                fogBugzPluginContext.FogBugzApiLogout();
            }
            else
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Warning, s_PlugInName, "Null or invalid context passed to ReleaseContext");
            }
        }

        #endregion Methods
    }
}
