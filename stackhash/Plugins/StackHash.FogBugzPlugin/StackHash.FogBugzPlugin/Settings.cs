using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;

namespace StackHash.FogBugzPlugin
{
    /// <summary>
    /// Settings wrapper - provides strongly typed settings with methods to convert to and from
    /// the NameValueCollection format required by StackHash
    /// </summary>
    sealed class Settings
    {
        /// <summary>
        /// Uri of the FogBugz API
        /// </summary>
        public string FogBugzApiUri { get; private set; }
        private const string FogBugzApiUriDefault = "";
        private const string FogBugzApiUriDescription = "FogBugz API URL:";

        /// <summary>
        /// Username for FogBugz
        /// </summary>
        public string FogBugzUsername { get; private set; }
        private const string FogBugzUsernameDefault = "";
        private const string FogBugzUsernameDescription = "FogBugz Username:";

        /// <summary>
        /// Password for FogBugz
        /// </summary>
        public string FogBugzPassword { get; private set; }
        private const string FogBugzPasswordDefault = "";
        private const string FogBugzPasswordDescription = "FogBugz Password:";

        /// <summary>
        /// FogBugz Project
        /// </summary>
        public string FogBugzProject { get; private set; }
        private const string FogBugzProjectDefault = "";
        private const string FogBugzProjectDescription = "FogBugz Project:";

        /// <summary>
        /// FogBugz Area
        /// </summary>
        public string FogBugzArea { get; private set; }
        private const string FogBugzAreaDefault = "";
        private const string FogbugzAreaDescription = "FogBugz Area:";

        /// <summary>
        /// True if the plugin should only send manual reports
        /// </summary>
        public bool IsManualOnly { get; private set; }
        private const bool IsManualOnlyDefault = false;
        private const string IsManualOnlyDescription = "Only add manually submitted events (True/False):";

        /// <summary>
        /// Gets the hit threshold for automatic event reporting
        /// </summary>
        public int EventHitAddThreshold { get; private set; }
        private const int EventHitAddThresholdDefault = 0;
        private const string EventHitAddThresholdDescription = "Hit threshold for automatically adding events (0 = add all):";

        /// <summary>
        /// Converts the Settings object to a NameValueCollection
        /// </summary>
        /// <returns>NameValueCollection</returns>
        public NameValueCollection ToNameValueCollection()
        {
            NameValueCollection nameValueCollection = new NameValueCollection();

            nameValueCollection[FogBugzApiUriDescription] = this.FogBugzApiUri;
            nameValueCollection[FogBugzUsernameDescription] = this.FogBugzUsername;
            nameValueCollection[FogBugzPasswordDescription] = this.FogBugzPassword;
            nameValueCollection[FogBugzProjectDescription] = this.FogBugzProject;
            nameValueCollection[FogbugzAreaDescription] = this.FogBugzArea;
            nameValueCollection[IsManualOnlyDescription] = this.IsManualOnly.ToString();
            nameValueCollection[EventHitAddThresholdDescription] = this.EventHitAddThreshold.ToString(CultureInfo.InvariantCulture);

            return nameValueCollection;
        }

        /// <summary>
        /// Gets a NameValueCollection containing the default settings
        /// </summary>
        /// <returns>NameValueCollection containing default settings</returns>
        public static NameValueCollection GetDefaults()
        {
            return new Settings().ToNameValueCollection();
        }

        /// <summary>
        /// Creates a Settings object from a NameValueCollection
        /// </summary>
        /// <param name="nameValueCollection">NameValueCollection</param>
        /// <returns>Settings object</returns>
        public static Settings FromNameValueCollection(NameValueCollection nameValueCollection)
        {
            Settings settings = new Settings();

            settings.FogBugzApiUri = nameValueCollection[FogBugzApiUriDescription];
            settings.FogBugzUsername = nameValueCollection[FogBugzUsernameDescription];
            settings.FogBugzPassword = nameValueCollection[FogBugzPasswordDescription];
            settings.FogBugzProject = nameValueCollection[FogBugzProjectDescription];
            settings.FogBugzArea = nameValueCollection[FogbugzAreaDescription];

            if (!string.IsNullOrEmpty(nameValueCollection[IsManualOnlyDescription]))
            {
                settings.IsManualOnly = Convert.ToBoolean(nameValueCollection[IsManualOnlyDescription]);
            }

            if (!string.IsNullOrEmpty(nameValueCollection[EventHitAddThresholdDescription]))
            {
                settings.EventHitAddThreshold = Convert.ToInt32(nameValueCollection[EventHitAddThresholdDescription], CultureInfo.InvariantCulture);
            }

            return settings;
        }

        /// <summary>
        /// Settings wrapper - provides strongly typed settings with methods to convert to and from
        /// the NameValueCollection format required by StackHash
        /// </summary>
        public Settings()
        {
            this.FogBugzApiUri = FogBugzApiUriDefault;
            this.FogBugzUsername = FogBugzUsernameDefault;
            this.FogBugzPassword = FogBugzPasswordDefault;
            this.FogBugzProject = FogBugzProjectDefault;
            this.FogBugzArea = FogBugzAreaDefault;
            this.IsManualOnly = IsManualOnlyDefault;
            this.EventHitAddThreshold = EventHitAddThresholdDefault;
        }
    }
}
