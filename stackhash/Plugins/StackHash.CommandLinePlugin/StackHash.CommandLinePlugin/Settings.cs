using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;

namespace StackHash.CommandLinePlugin
{
    /// <summary>
    /// Settings wrapper - provides strongly typed settings with methods to convert to and from
    /// the NameValueCollection format required by StackHash
    /// </summary>
    sealed class Settings
    {
        /// <summary>
        /// Script to run when a product is added
        /// </summary>
        public string ProductAddedScript { get; private set; }
        private const string ProductAddedScriptDefault = "";
        private const string ProductAddedScriptDescription = "Product Added:";

        /// <summary>
        /// Script to run when a product is updated
        /// </summary>
        public string ProductUpdatedScript { get; private set; }
        private const string ProductUpdatedScriptDefault = "";
        private const string ProductUpdatedScriptDescription = "Product Updated:";

        /// <summary>
        /// Script to run when an event is added
        /// </summary>
        public string EventAddedScript { get; private set; }
        private const string EventAddedScriptDefault = "";
        private const string EventAddedScriptDescription = "Event Added:";

        /// <summary>
        /// Script to run when an event is updated
        /// </summary>
        public string EventUpdatedScript { get; private set; }
        private const string EventUpdatedScriptDefault = "";
        private const string EventUpdatedScriptDescription = "Event Updated:";

        /// <summary>
        /// Script to run when an event note is added
        /// </summary>
        public string EventNoteAddedScript { get; private set; }
        private const string EventNoteAddedScriptDefault = "";
        private const string EventNoteAddedScriptDescription = "Event Note Added:";

        /// <summary>
        /// Script to run when a cab is added
        /// </summary>
        public string CabAddedScript { get; private set; }
        private const string CabAddedScriptDefault = "";
        private const string CabAddedScriptDescription = "Cab Added:";

        /// <summary>
        /// Script to run when a cab is updated
        /// </summary>
        public string CabUpdatedScript { get; private set; }
        private const string CabUpdatedScriptDefault = "";
        private const string CabUpdatedScriptDescription = "Cab Updated:";

        /// <summary>
        /// Script to run when a cab note is added
        /// </summary>
        public string CabNoteAddedScript { get; private set; }
        private const string CabNoteAddedScriptDefault = "";
        private const string CabNoteAddedScriptDescription = "Cab Note Added:";

        /// <summary>
        /// Script to run when script results are added
        /// </summary>
        public string DebugScriptExecutedScript { get; private set; }
        private const string DebugScriptExecutedScriptDefault = "";
        private const string DebugScriptExecutedScriptDescription = "Debug Script Executed:";

        /// <summary>
        /// True if the plugin should only send manual reports
        /// </summary>
        public bool IsManualOnly { get; private set; }
        private const bool IsManualOnlyDefault = false;
        private const string IsManualOnlyDescription = "Only report manually submitted events (True/False):";

        /// <summary>
        /// Gets the hit threshold for automatic event reporting
        /// </summary>
        public int EventHitAddThreshold { get; private set; }
        private const int EventHitAddThresholdDefault = 0;
        private const string EventHitAddThresholdDescription = "Hit threshold for automatically reporting events (0 = report all):";

        /// <summary>
        /// Converts the Settings object to a NameValueCollection
        /// </summary>
        /// <returns>NameValueCollection</returns>
        public NameValueCollection ToNameValueCollection()
        {
            NameValueCollection nameValueCollection = new NameValueCollection();

            nameValueCollection[ProductAddedScriptDescription] = this.ProductAddedScript;
            nameValueCollection[ProductUpdatedScriptDescription] = this.ProductUpdatedScript;
            nameValueCollection[EventAddedScriptDescription] = this.EventAddedScript;
            nameValueCollection[EventUpdatedScriptDescription] = this.EventUpdatedScript;
            nameValueCollection[EventNoteAddedScriptDescription] = this.EventNoteAddedScript;
            nameValueCollection[CabAddedScriptDescription] = this.CabAddedScript;
            nameValueCollection[CabUpdatedScriptDescription] = this.CabUpdatedScript;
            nameValueCollection[CabNoteAddedScriptDescription] = this.CabNoteAddedScript;
            nameValueCollection[DebugScriptExecutedScriptDescription] = this.DebugScriptExecutedScript;
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

            if (!string.IsNullOrEmpty(nameValueCollection[ProductAddedScriptDescription]))
            {
                settings.ProductAddedScript = nameValueCollection[ProductAddedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[ProductUpdatedScriptDescription]))
            {
                settings.ProductUpdatedScript = nameValueCollection[ProductUpdatedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[EventAddedScriptDescription]))
            {
                settings.EventAddedScript = nameValueCollection[EventAddedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[EventUpdatedScriptDescription]))
            {
                settings.EventUpdatedScript = nameValueCollection[EventUpdatedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[EventNoteAddedScriptDescription]))
            {
                settings.EventNoteAddedScript = nameValueCollection[EventNoteAddedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[CabAddedScriptDescription]))
            {
                settings.CabAddedScript = nameValueCollection[CabAddedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[CabUpdatedScriptDescription]))
            {
                settings.CabUpdatedScript = nameValueCollection[CabUpdatedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[CabNoteAddedScriptDescription]))
            {
                settings.CabNoteAddedScript = nameValueCollection[CabNoteAddedScriptDescription];
            }

            if (!string.IsNullOrEmpty(nameValueCollection[DebugScriptExecutedScriptDescription]))
            {
                settings.DebugScriptExecutedScript = nameValueCollection[DebugScriptExecutedScriptDescription];
            }

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
            this.ProductAddedScript = ProductAddedScriptDefault;
            this.ProductUpdatedScript = ProductUpdatedScriptDefault;
            this.EventAddedScript = EventAddedScriptDefault;
            this.EventUpdatedScript = EventUpdatedScriptDefault;
            this.EventNoteAddedScript = EventNoteAddedScriptDefault;
            this.CabAddedScript = CabAddedScriptDefault;
            this.CabUpdatedScript = CabUpdatedScriptDefault;
            this.CabNoteAddedScript = CabNoteAddedScriptDefault;
            this.DebugScriptExecutedScript = DebugScriptExecutedScriptDefault;
            this.IsManualOnly = IsManualOnlyDefault;
            this.EventHitAddThreshold = EventHitAddThresholdDefault;
        }
    }
}
