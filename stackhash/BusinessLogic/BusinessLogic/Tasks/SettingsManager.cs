using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

using StackHashBusinessObjects;
using StackHashUtilities;
using StackHashErrorIndex;
using StackHashSqlControl;

namespace StackHashTasks
{
    public class SettingsManager
    {
        StackHashSettings m_Settings;
        String m_SettingsFileName;
        String m_SettingsFileNameBackup;
        String m_SettingsFileNameSaved;
        private static int s_DefaultPurgeDays = 180;



        /// <summary>
        /// Loads the specified settings file.
        /// If the file is corrupt and XML INNER exception will occur (InvalidOperation outer exception).
        /// In this case return null so that the file can be reconstructed.
        /// If any other error occurs then just leave the file there.
        /// </summary>
        /// <param name="settingsFileName">Settings file name.</param>
        /// <param name="createBackupFileOnSuccess">True - creates a backup if the load is successful.</param>
        /// <param name="backupFileName">Name of backup file to create if createBackupFileOnSuccess is true.</param>
        /// <returns>Loaded settings.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        private StackHashSettings loadSettings(String settingsFileName, bool createBackupFileOnSuccess, String backupFileName)
        {
            StackHashSettings settings = null;

            try
            {
                // Load the settings.
                settings = StackHashSettings.Load(m_SettingsFileName);

                if (createBackupFileOnSuccess)
                {
                    // Loaded ok - so this file is fine to make a backup of.
                    try
                    {
                        File.Copy(settingsFileName, backupFileName, true); // With overwrite.
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Settings file could not be copied to save file: " + backupFileName, ex);
                    }
                }
            }

            catch (System.Xml.XmlException ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Settings file could not be loaded: " + settingsFileName, ex);
            }
            catch (System.InvalidOperationException ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Settings file could not be loaded: " + settingsFileName, ex);

                // XML exceptions are unrecoverable so the file must be reconstructed. If permissions errors etc occur then don't
                // wipe out the existing file.
                if (!ex.ContainsExceptionType(typeof(XmlException)))
                    throw;
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "Settings file could not be loaded: " + settingsFileName, ex);
                throw;
            }

            return settings;
        }


        /// <summary>
        /// Saves the current context settings.
        /// </summary>
        private void saveSettings()
        {
            Monitor.Enter(this);

            try
            {
                StackHashSettings.Save(m_Settings, m_SettingsFileName);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Loads settings from the specified location - or creates a new settings file
        /// if one does not exist.
        /// 
        /// A new file is created from the backup if possible if the settings file is corrupt.
        /// (note this doesn't include permissions related errors).
        /// </summary>
        /// <param name="settingsFileName"></param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public SettingsManager(String settingsFileName)
        {
            bool reconstructSettings = false;

            m_SettingsFileName = settingsFileName;

            StringBuilder backupSettingsFileName = new StringBuilder(Path.GetDirectoryName(settingsFileName));
            backupSettingsFileName.Append("\\");
            backupSettingsFileName.Append(Path.GetFileNameWithoutExtension(settingsFileName));
            backupSettingsFileName.Append(".bak");
            m_SettingsFileNameBackup = backupSettingsFileName.ToString();

            // Used for recording old files in case of error.
            StringBuilder savedSettingsFileName = new StringBuilder(Path.GetDirectoryName(settingsFileName));
            savedSettingsFileName.Append("\\");
            savedSettingsFileName.Append(Path.GetFileNameWithoutExtension(settingsFileName));
            savedSettingsFileName.Append(".sav");
            m_SettingsFileNameSaved = savedSettingsFileName.ToString();
            
            if (File.Exists(settingsFileName))
            {
                m_Settings = loadSettings(settingsFileName, true, m_SettingsFileNameBackup);

                if (m_Settings == null)
                {
                    // Couldn't load the main file for some reason. Copy it to the SAV file so that it can
                    // be analyzed later.
                    try
                    {
                        File.Copy(settingsFileName, m_SettingsFileNameSaved, true); // With overwrite.
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Settings file could not be copied to save file: " + settingsFileName, ex);
                    }


                    // Try loading the backup copy of the settings if there is one.
                    if (File.Exists(m_SettingsFileNameBackup))
                    {
                        m_Settings = loadSettings(m_SettingsFileNameBackup, false, m_SettingsFileNameBackup);

                        if (m_Settings != null)
                        {
                            // Save the settings so there is something to work with.
                            StackHashSettings.Save(m_Settings, settingsFileName);
                        }
                        else
                        {
                            reconstructSettings = true;
                        }
                    }
                    else
                    {
                        reconstructSettings = true;
                    }
                }
            }
            else
            {
                reconstructSettings = true;
            }

            if (reconstructSettings)
            {
                // Note only corrupt files will be caught here. Any permissions related exceptions will
                // not be caught so won't result in the file being reconstructed.

                // Assume the file is corrupt and create a new one.
                // Create empty settings. A new profile must be added by the client.
                m_Settings = new StackHashSettings();
                m_Settings.ContextCollection = new StackHashContextCollection();

                // Save for later use.
                saveSettings();
            }

            m_SettingsFileName = settingsFileName;
        }


        /// <summary>
        /// Returns the current stackhash settings.
        /// </summary>
        /// <returns></returns>
        public StackHashSettings CurrentSettings
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_Settings;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Enables/Disables the logging setting.
        /// </summary>
        /// <returns></returns>
        public bool EnableLogging
        {
            get
            {
                Monitor.Enter(this);
                try
                {
                    return m_Settings.EnableLogging;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);
                try
                {
                    if (value == true)
                    {
                        // Enable if not already enabled.
                        if (!m_Settings.EnableLogging)
                        {
                            m_Settings.EnableLogging = true;
                            saveSettings();
                        }
                    }
                    else
                    {
                        // Disable logging if not already disabled.
                        if (m_Settings.EnableLogging)
                        {
                            m_Settings.EnableLogging = false;
                            saveSettings();
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }

            }
        }


        /// <summary>
        /// Gets/Sets the client timeout.
        /// </summary>
        public int ClientTimeoutInSeconds
        {
            get
            {
                Monitor.Enter(this);
                try
                {
                    if (m_Settings == null)
                        return StackHashSettings.DefaultClientTimeoutInSeconds;
                    else
                       return m_Settings.ClientTimeoutInSeconds;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);
                try
                {
                    // Can't set to 0.
                    if (value != 0)
                    {
                        if (value != m_Settings.ClientTimeoutInSeconds)
                        {
                            m_Settings.ClientTimeoutInSeconds = value;
                            saveSettings();
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }

            }
        }

                
        /// <summary>
        /// Enables/Disables reporting.
        /// </summary>
        /// <returns></returns>
        public bool ReportingEnabled
        {
            get
            {
                Monitor.Enter(this);
                try
                {
                    return m_Settings.ReportingEnabled;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);
                try
                {
                    if (value == true)
                    {
                        // Enable if not already enabled.
                        if (!m_Settings.ReportingEnabled)
                        {
                            m_Settings.ReportingEnabled = true;
                            saveSettings();
                        }
                    }
                    else
                    {
                        // Disable logging if not already disabled.
                        if (m_Settings.ReportingEnabled)
                        {
                            m_Settings.ReportingEnabled = false;
                            saveSettings();
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Uniquely identifies the instance of the service for reporting stats.
        /// Does not identify the machine.
        /// </summary>
        public String ServiceGuid
        {
            get
            {
                Monitor.Enter(this);
                try
                {
                    return m_Settings.ServiceGuid;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);
                try
                {
                    m_Settings.ServiceGuid = value;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        
        /// <summary>
        /// Gets/Sets the proxy settings.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806")]
        public StackHashProxySettings ProxySettings
        {
            get
            {
                Monitor.Enter(this);
                try
                {
                    if (m_Settings.ProxySettings == null)
                        return null;
                    else
                        return m_Settings.ProxySettings.Clone() as StackHashProxySettings;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            set
            {
                Monitor.Enter(this);
                try
                {
                    if (value == null)
                    {
                        m_Settings.ProxySettings = new StackHashProxySettings(false, false, null, 0, null, null, null);
                    }
                    else
                    {
                        if (value.UseProxy)
                        {
                            // Create a web proxy to validate the params.
                            WebProxy proxy = new WebProxy(value.ProxyHost, value.ProxyPort);
                        }

                        if (value == null)
                            m_Settings.ProxySettings = value;
                        else
                            m_Settings.ProxySettings = value.Clone() as StackHashProxySettings;
                    }
                    saveSettings();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Adds a new context settings. The next free unused int is used as a context ID.
        /// </summary>
        /// <returns>Default settings</returns>
        public StackHashContextSettings CreateNewContextSettings()
        {
            return CreateNewContextSettings(ErrorIndexType.Xml);
        }



        /// <summary>
        /// Adds a new context settings. The next free unused int is used as a context ID.
        /// </summary>
        /// <returns>Default settings</returns>
        public StackHashContextSettings CreateNewContextSettings(ErrorIndexType errorIndexType)
        {
            StackHashContextSettings newSettings = null;

            // Now add the settings to the list.
            Monitor.Enter(this);

            try
            {
                if (errorIndexType == ErrorIndexType.Xml)
                    newSettings = DefaultStackHashContextSettings;
                else if (errorIndexType == ErrorIndexType.SqlExpress)
                    newSettings = DefaultSqlExpressStackHashContextSettings;
                else
                    throw new StackHashException("Sql index type not supported", StackHashServiceErrorCode.ErrorIndexTypeNotSupported);

                newSettings.Id = m_Settings.NextContextId;
                m_Settings.ContextCollection.Add(newSettings);
                m_Settings.NextContextId++;
                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }


            return newSettings;
        }


        /// <summary>
        /// Removes a context setting. 
        /// </summary>
        /// <param name="id">Id of the context.</param>
        /// <param name="resetNextContextIdIfAppropriate">true - resets the context id to 0 if no more contexts.</param>
        public void RemoveContextSettings(int id, bool resetNextContextIdIfAppropriate)
        {
            // Now add the settings to the list.
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(id);
                if (context == null)
                    throw new ArgumentException("Id not found", "id");

                m_Settings.ContextCollection.Remove(context);


                // Also reset the next context to 0.
                if (resetNextContextIdIfAppropriate && (m_Settings.ContextCollection.Count == 0))
                    m_Settings.NextContextId = 0;

                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Determines if the specified schedules are valid. If they are not
        /// a StackHashException is thrown.
        /// </summary>
        /// <param name="schedules">The schedules to check.</param>
        /// <param name="fieldName">The name of the schedule being checked.</param>
        private void validateSchedules(ScheduleCollection schedules, string fieldName)
        {
            // Can be empty but not null.
            if (schedules == null)
                throw new ArgumentNullException("schedules");

            foreach (Schedule schedule in schedules)
            {
                if ((schedule.Period != SchedulePeriod.Daily) &&
                    (schedule.Period != SchedulePeriod.Hourly) &&
                    (schedule.Period != SchedulePeriod.Weekly))
                    throw new StackHashException("Schedule error: Invalid Period in " + fieldName, StackHashServiceErrorCode.ScheduleFormatError);

                // At least one DayOfWeek must be specified.
                if (schedule.DaysOfWeek == 0)
                    throw new StackHashException("Must specify at least one day in a schedule", StackHashServiceErrorCode.ScheduleFormatError);

                if (schedule.Time == null)
                    throw new StackHashException("Time not specified in " + fieldName, StackHashServiceErrorCode.ScheduleFormatError);

                if (schedule.Time.Hour < 0)
                    throw new StackHashException("Hour less than 0 in " + fieldName, StackHashServiceErrorCode.ScheduleFormatError);
                if (schedule.Time.Hour > 23)
                    throw new StackHashException("Hour greater than 23 in " + fieldName, StackHashServiceErrorCode.ScheduleFormatError);
                if (schedule.Time.Minute < 0)
                    throw new StackHashException("Minute less than 23 in " + fieldName, StackHashServiceErrorCode.ScheduleFormatError);
                if (schedule.Time.Minute > 59)
                    throw new StackHashException("Minute greater than 59 in " + fieldName, StackHashServiceErrorCode.ScheduleFormatError);
            }
        }

        /// <summary>
        /// Check if the specified settings conflict with an existing active or inactive profile.
        /// </summary>
        /// <param name="contextId">Id of context to check.</param>
        public void PreActivationCheck(int contextId)
        {
            // Compare against existing contexts.
            Monitor.Enter(this);

            StackHashContextSettings theseSettings = Find(contextId);

            try
            {
                foreach (StackHashContextSettings settings in m_Settings.ContextCollection)
                {
                    if (settings.Id == contextId)
                        continue; // Ignore ourselves.

                    // Two contexts cannot use the same index.
                    if (settings.ErrorIndexSettings.Folder.ToUpperInvariant() ==
                        theseSettings.ErrorIndexSettings.Folder.ToUpperInvariant())
                    {
                        throw new StackHashException("Error Index already in use by another profile", StackHashServiceErrorCode.ErrorIndexAssigned);
                    }

                    // 
                    if (settings.ErrorIndexSettings.Name.ToUpperInvariant() ==
                        theseSettings.ErrorIndexSettings.Name.ToUpperInvariant())
                    {
                        throw new StackHashException("Error Index Name already in use by another profile", StackHashServiceErrorCode.ErrorIndexNameAssigned);
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Determines if the specified context settings are valid. If they are not
        /// a StackHashException is thrown.
        /// </summary>
        /// <param name="contextSettings">The settings to check.</param>
        private void validateContextSettings(StackHashContextSettings contextSettings)
        {
            if (contextSettings == null)
                throw new ArgumentNullException("contextSettings");

            if (contextSettings.Id >= m_Settings.NextContextId)
                throw new ArgumentException("Invalid context ID specified", "contextSettings");

            validateSchedules(contextSettings.CabFilePurgeSchedule, "CAB Purge Schedule");
            validateSchedules(contextSettings.WinQualSyncSchedule, "WinQual Sync Schedule");

            if (!Path.IsPathRooted(contextSettings.ErrorIndexSettings.Folder))
                throw new ArgumentException("Error Index folder not valid: " + contextSettings.ErrorIndexSettings.Folder, "contextSettings");

            if (String.IsNullOrEmpty(contextSettings.ErrorIndexSettings.Name))
                throw new ArgumentException("Error Index name not specified", "contextSettings");

            if (contextSettings.ErrorIndexSettings.Type != ErrorIndexType.Xml)
                if (!SqlUtils.IsValidSqlDatabaseName(contextSettings.ErrorIndexSettings.Name))
                    throw new StackHashException("Invalid SQL database name", StackHashServiceErrorCode.InvalidDatabaseName);

        }


        /// <summary>
        /// Determines if the specified settings are valid. If they are not
        /// a StackHashException is thrown.
        /// </summary>
        /// <param name="settings">The settings to check.</param>
        private void validateSettings(StackHashSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (settings.ContextCollection == null)
                throw new ArgumentException("No context collection specified", "settings");

            if (settings.ContextCollection.Count == 0)
                throw new ArgumentException("No context data provided", "settings");

            foreach (StackHashContextSettings context in settings.ContextCollection)
            {
                validateContextSettings(context);
            }
        }


        /// <summary>
        /// Validates and sets the service settings.
        /// </summary>
        /// <param name="settings">The settings to set.</param>
        public void SetSettings(StackHashSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            Monitor.Enter(this);

            try
            {
                // Only certain parameters can be set. First validate the new state.
                // This call will throw an exception if the data is invalid.
                validateSettings(settings);

                foreach (StackHashContextSettings contextSettings in settings.ContextCollection)
                {
                    SetContextSettings(contextSettings, true);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Validates and sets the service settings for a particular context (profile).
        /// </summary>
        /// <param name="contextSettings">The context settings to set.</param>
        /// <param name="includeIndexSettings">true - index settings saved, false - index settings not saved.</param>
        public void SetContextSettings(StackHashContextSettings contextSettings, bool includeIndexSettings)
        {
            if (contextSettings == null)
                throw new ArgumentNullException("contextSettings");


            // Compare against existing contexts.
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings currentSettings = this.Find(contextSettings.Id);

                if (currentSettings == null)
                    throw new ArgumentException("ContextId not found", "contextSettings");

                // Only certain parameters can be set. First validate the new state.
                // This call will throw an exception if the data is invalid.
                validateContextSettings(contextSettings);


                // Now set the allowable settings.
                currentSettings.CabFilePurgeSchedule = contextSettings.CabFilePurgeSchedule;
                currentSettings.PurgeOptionsCollection = contextSettings.PurgeOptionsCollection;

                // Update the winqual schedule.
                bool winQualScheduleChanged = (currentSettings.WinQualSyncSchedule.CompareTo(contextSettings.WinQualSyncSchedule) != 0);
                currentSettings.WinQualSyncSchedule = contextSettings.WinQualSyncSchedule;

                if (includeIndexSettings)
                    currentSettings.ErrorIndexSettings = contextSettings.ErrorIndexSettings;

                WinQualSettings oldWinQualSettings = currentSettings.WinQualSettings;
                currentSettings.WinQualSettings = contextSettings.WinQualSettings;

                // Update the debugger settings.
                currentSettings.DebuggerSettings = contextSettings.DebuggerSettings;

                currentSettings.SqlSettings = contextSettings.SqlSettings;
                currentSettings.BugTrackerSettings = contextSettings.BugTrackerSettings;

                // And persist to a file.
                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets the current state of the specified context settings.
        /// </summary>
        /// <param name="contextId">The context settings to get.</param>
        public StackHashContextSettings GetContextSettings(int contextId)
        {
            // Compare against existing contexts.
            Monitor.Enter(this);

            try
            {
                return (this.Find(contextId));                
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Sets the purge options and schedule for the context.
        /// </summary>
        /// <param name="contextId">The id of the context to set.</param>
        /// <param name="purgeSchedule">When the automatic purge is to take place.</param>
        /// <param name="purgeOptionsCollection">What is to be purged</param>
        /// <param name="setAll">True - replace, false - individual.</param>
        public void SetPurgeOptions(int contextId, ScheduleCollection purgeSchedule, StackHashPurgeOptionsCollection purgeOptionsCollection, bool setAll)
        {
            if (purgeOptionsCollection == null)
                throw new ArgumentNullException("purgeOptionsCollection");

            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);
                if (purgeSchedule != null)
                    context.CabFilePurgeSchedule = purgeSchedule;

                if (setAll)
                {
                    // Replace the whole list.
                    context.PurgeOptionsCollection = purgeOptionsCollection;
                }
                else
                {
                    // Just update individual entries.
                    foreach (StackHashPurgeOptions purgeOptions in purgeOptionsCollection)
                    {
                        context.PurgeOptionsCollection.AddPurgeOptions(purgeOptions);
                    }
                }

                // And persist to a file.
                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the purge options for the specified context and object type.
        /// </summary>
        /// <param name="contextId">The id of the context.</param>
        /// <param name="purgeObject">Object to get the settings for.</param>
        /// <param name="id">Id of the object.</param>
        /// <param name="getAll">True - gets all purge options, false - gets individual purge option.</param>
        /// <returns>Individual or all purge options as requested.</returns>
        public StackHashPurgeOptionsCollection GetPurgeOptions(int contextId, StackHashPurgeObject purgeObject, int id, bool getAll)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                StackHashPurgeOptionsCollection purgeOptionsCollection = new StackHashPurgeOptionsCollection();

                foreach (StackHashPurgeOptions purgeOptions in context.PurgeOptionsCollection)
                {
                    if (getAll)
                        purgeOptionsCollection.Add(purgeOptions);
                    else if ((purgeOptions.PurgeObject == purgeObject) && (purgeOptions.Id == id))
                        purgeOptionsCollection.Add(purgeOptions);
                }

                return purgeOptionsCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the purge options for the specified context and object type.
        /// </summary>
        /// <param name="contextId">The id of the context.</param>
        /// <param name="productId">ID of product or 0.</param>
        /// <param name="fileId">ID of file or 0.</param>
        /// <param name="eventId">ID of event or 0.</param>
        /// <param name="cabId">ID of cab or 0.</param>
        /// <returns>Prioritized options.</returns>
        public StackHashPurgeOptionsCollection GetActivePurgeOptions(int contextId, int productId, int fileId, int eventId, int cabId)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                StackHashPurgeOptionsCollection purgeOptionsCollection = new StackHashPurgeOptionsCollection();

                StackHashPurgeOptions currentOptions = context.PurgeOptionsCollection.FindActivePurgeOptions(productId, fileId, eventId, cabId);

                if (currentOptions != null)
                    purgeOptionsCollection.Add(currentOptions);

                return purgeOptionsCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Removes the purge options for the specified context and object type.
        /// </summary>
        /// <param name="contextId">The id of the context.</param>
        /// <param name="purgeObject">Object to get the settings for.</param>
        /// <param name="id">Id of the object.</param>
        public void RemovePurgeOptions(int contextId, StackHashPurgeObject purgeObject, int id)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                if (context.PurgeOptionsCollection.RemovePurgeOption(purgeObject, id))
                    saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// These will be set as an atomic action.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="newErrorIndexPath">New path for index.</param>
        /// <param name="newErrorIndexName">New name for index.</param>
        /// <param name="sqlSettings">SqlServer connection settings.</param>
        public void SetContextErrorIndexSettings(int contextId, String newErrorIndexPath, String newErrorIndexName, StackHashSqlConfiguration sqlSettings)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                ErrorIndexSettings indexSettings = new ErrorIndexSettings();
                indexSettings.Folder = newErrorIndexPath;
                indexSettings.Name = newErrorIndexName;
                indexSettings.Status = context.ErrorIndexSettings.Status;
                indexSettings.Location = context.ErrorIndexSettings.Location;
                indexSettings.Type = context.ErrorIndexSettings.Type;

                SetContextErrorIndexSettings(contextId, indexSettings, sqlSettings);                
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Sets the plugin settings for a context.
        /// </summary>
        /// <param name="contextId">Context whose settings are to be set.</param>
        /// <param name="settings">New plugin settings.</param>
        public void SetContextBugTrackerPlugInSettings(int contextId, StackHashBugTrackerPlugInSettings settings)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                context.BugTrackerSettings = settings;
                
                // And persist to a file.
                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the plugin settings for a context.
        /// </summary>
        /// <param name="contextId">Context whose settings are to be retrieved.</param>
        /// <returns>Bug tracker settings for the specified context.</returns>
        public StackHashBugTrackerPlugInSettings GetContextBugTrackerPlugInSettings(int contextId)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                return context.BugTrackerSettings;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        
        /// <summary>
        /// These will be set as an atomic action.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="errorIndexSettings">Error index settings.</param>
        /// <param name="sqlSettings">SqlServer connection settings.</param>
        public void SetContextErrorIndexSettings(int contextId, ErrorIndexSettings errorIndexSettings, StackHashSqlConfiguration sqlSettings)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);
                context.ErrorIndexSettings = errorIndexSettings;
                context.SqlSettings = sqlSettings;

                if ((context.ErrorIndexSettings != null) &&
                    (context.ErrorIndexSettings.Folder != null) &&
                    (context.ErrorIndexSettings.Name != null))
                {
                    // Set the location if not already set.
                    if (context.ErrorIndexSettings.Status == ErrorIndexStatus.Created)
                    {
                        String indexFolder = Path.Combine(context.ErrorIndexSettings.Folder, context.ErrorIndexSettings.Name);
                        if (Directory.Exists(indexFolder))
                        {
                            String[] databaseFiles = Directory.GetFiles(indexFolder, "*.mdf");

                            if (databaseFiles.Length > 0)
                                context.ErrorIndexSettings.Location = StackHashErrorIndexLocation.InCabFolder;
                            else
                                context.ErrorIndexSettings.Location = StackHashErrorIndexLocation.OnSqlServer;
                        }
                    }
                }

                // And persist to a file.
                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// These will be set as an atomic action.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="productId">Id of the product to enable.</param>
        /// <param name="enable">True - enable, false - disable.</param>
        public void SetProductSynchronization(int contextId, int productId, bool enable)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);
                if (context.WinQualSettings.ProductsToSynchronize == null)
                    context.WinQualSettings.ProductsToSynchronize = new StackHashProductSyncDataCollection();

                bool saveRequired = false;

                if (enable)
                {
                    // See if the item is already in the list.
                    if (context.WinQualSettings.ProductsToSynchronize.FindProduct(productId) == null)
                    {
                        // Not in the list so add it.
                        context.WinQualSettings.ProductsToSynchronize.Add(new StackHashProductSyncData(productId));
                        saveRequired = true;
                    }
                }
                else
                {
                    StackHashProductSyncData foundEntry = context.WinQualSettings.ProductsToSynchronize.FindProduct(productId);

                    if (foundEntry != null)
                    {
                        // Remove from the list.
                        context.WinQualSettings.ProductsToSynchronize.Remove(foundEntry);
                        saveRequired = true;
                    }
                }

                // And persist to a file if changed.
                if (saveRequired)
                    saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// These will be set as an atomic action.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="productSyncData">Data to set.</param>
        public void SetProductSyncData(int contextId, StackHashProductSyncData productSyncData)
        {
            if (productSyncData == null)
                throw new ArgumentNullException("productSyncData");

            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);
                if (context.WinQualSettings.ProductsToSynchronize == null)
                    context.WinQualSettings.ProductsToSynchronize = new StackHashProductSyncDataCollection();

                bool saveRequired = false;

                StackHashProductSyncData foundEntry = context.WinQualSettings.ProductsToSynchronize.FindProduct(productSyncData.ProductId);
                if (foundEntry == null)
                {
                    // Not in the list so add it.
                    context.WinQualSettings.ProductsToSynchronize.Add(new StackHashProductSyncData(productSyncData.ProductId));
                    saveRequired = true;
                }

                // And persist to a file if changed.
                if (saveRequired)
                    saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets the data collection policy for the specified object.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="rootObject">Global, Product, File, Event or Cab.</param>
        /// <param name="id">Id of the object to get.</param>
        /// <param name="conditionObject">The object to which the condition applies.</param>
        /// <param name="objectToCollect">The type of object being collected.</param>
        /// <param name="getAll">True - gets all policies, false - gets individual policy.</param>
        public StackHashCollectionPolicyCollection GetDataCollectionPolicy(int contextId, StackHashCollectionObject rootObject, int id, 
            StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect, bool getAll)
        {
            Monitor.Enter(this);

            try
            {
                StackHashCollectionPolicyCollection policyCollection = new StackHashCollectionPolicyCollection();

                StackHashContextSettings context = this.Find(contextId);

                foreach (StackHashCollectionPolicy policy in context.CollectionPolicy)
                {
                    if (getAll)
                    {
                        policyCollection.Add(policy);
                    }
                    else if (((policy.RootObject == rootObject) && (policy.RootId == id)) || (rootObject == StackHashCollectionObject.Any))
                    {
                        if ((policy.ConditionObject == conditionObject) || (conditionObject == StackHashCollectionObject.Any))
                        {
                            if ((policy.ObjectToCollect == objectToCollect) || (objectToCollect == StackHashCollectionObject.Any))
                            {
                                policyCollection.Add(policy);
                            }
                        }
                    }
                }

                return policyCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets the active data collection policy for the specified object.
        /// </summary>
        /// <param name="contextId">ID of the context.</param>
        /// <param name="productId">ID of product or 0.</param>
        /// <param name="fileId">ID of file or 0.</param>
        /// <param name="eventId">ID of event or 0.</param>
        /// <param name="cabId">ID of cab or 0.</param>
        /// <param name="objectToCollect">Object being collected.</param>
        /// <returns>Prioritized policy.</returns>
        public StackHashCollectionPolicyCollection GetActiveDataCollectionPolicy(int contextId, int productId, int fileId, int eventId, int cabId, StackHashCollectionObject objectToCollect)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                StackHashCollectionPolicyCollection policyCollection = context.CollectionPolicy.FindAllPoliciesForObjectBeingCollected(productId, fileId, eventId, cabId, objectToCollect);

                return policyCollection;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Removes the specified policy.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="rootObject">Global, Product, File, Event or Cab.</param>
        /// <param name="id">Id of the object to get.</param>
        /// <param name="conditionObject">Object to which the condition refers.</param>
        /// <param name="objectToCollect">Object being collected.</param>
        public void RemoveDataCollectionPolicy(int contextId, StackHashCollectionObject rootObject, int id, 
            StackHashCollectionObject conditionObject, StackHashCollectionObject objectToCollect)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                StackHashCollectionPolicy policy = context.CollectionPolicy.FindPolicy(rootObject, id, conditionObject, objectToCollect);

                if (policy != null)
                {
                    context.CollectionPolicy.RemovePolicy(policy);
                    saveSettings();
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        
        /// <summary>
        /// Set the data collection policy. This will merge or replace existing policy records unless setAll is specified
        /// in which case the entire collection will be replaced.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="policyCollection">A collection of data collection policies.</param>
        /// <param name="setAll">True - replaces the existing policy list with the new one.</param>
        public void SetDataCollectionPolicy(int contextId, StackHashCollectionPolicyCollection policyCollection, bool setAll)
        {
            if (policyCollection == null)
                throw new ArgumentNullException("policyCollection");

            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                if (setAll)
                {
                    context.CollectionPolicy = policyCollection;
                }
                else
                {
                    foreach (StackHashCollectionPolicy policy in policyCollection)
                    {
                        context.CollectionPolicy.AddPolicy(policy);
                    }
                }

                // And persist to a file if changed.
                saveSettings();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Set the email notification settings for the specified context.
        /// </summary>
        /// <param name="contextId">The context settings to set.</param>
        /// <param name="emailSettings">New email settings</param>
        public void SetEmailSettings(int contextId, StackHashEmailSettings emailSettings)
        {
            if (emailSettings == null)
                throw new ArgumentNullException("emailSettings");

            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                if (context != null)
                {
                    context.EmailSettings = emailSettings;
                    saveSettings();
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        /// <summary>
        /// Determines if the specified product is enabled for synchronization.
        /// </summary>
        /// <param name="contextId">The context settings to get.</param>
        /// <param name="productId">Id of the product to get.</param>
        public bool GetProductSynchronization(int contextId, int productId)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                if (context.WinQualSettings.ProductsToSynchronize == null)
                    context.WinQualSettings.ProductsToSynchronize = new StackHashProductSyncDataCollection();

                StackHashProductSyncData productSyncData = context.WinQualSettings.ProductsToSynchronize.FindProduct(productId);

                return (productSyncData != null);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Returns sync data for the specified product if present.
        /// </summary>
        /// <param name="contextId">The context settings to get.</param>
        /// <param name="productId">Id of the product to get.</param>
        /// <return>null or product sync data</return>
        public StackHashProductSyncData GetProductSyncData(int contextId, int productId)
        {
            Monitor.Enter(this);

            try
            {
                StackHashContextSettings context = this.Find(contextId);

                if (context.WinQualSettings.ProductsToSynchronize == null)
                    context.WinQualSettings.ProductsToSynchronize = new StackHashProductSyncDataCollection();

                StackHashProductSyncData productSyncData = context.WinQualSettings.ProductsToSynchronize.FindProduct(productId);

                return productSyncData;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Default context settings. Sets sensible defaults when creating a new context for an Sql express index.
        /// </summary>
        private StackHashContextSettings DefaultSqlExpressStackHashContextSettings
        {
            get
            {
                StackHashContextSettings settings = DefaultStackHashContextSettings;

                settings.ErrorIndexSettings.Type = ErrorIndexType.SqlExpress;
                return settings;
            }
        }

        /// <summary>
        /// Default context settings. Sets sensible defaults when creating a new context.
        /// </summary>
        private StackHashContextSettings DefaultStackHashContextSettings
        {
            get
            {
                // Default some settings.
                StackHashContextSettings contextSettings = new StackHashContextSettings();

                contextSettings.Id = 0;
                contextSettings.WinQualSettings = new WinQualSettings("UserName", "Password", "CompanyName", s_DefaultPurgeDays, 
                    new StackHashProductSyncDataCollection(), true, 30 * 60, 5, WinQualSettings.DefaultSyncsBeforeResync, false);
                contextSettings.ErrorIndexSettings = new ErrorIndexSettings();
                contextSettings.ErrorIndexSettings.Folder = XmlErrorIndex.DefaultErrorIndexPath;
                contextSettings.ErrorIndexSettings.Name = XmlErrorIndex.DefaultErrorIndexName;
                contextSettings.ErrorIndexSettings.Type = ErrorIndexType.Xml;
                contextSettings.ErrorIndexSettings.Status = ErrorIndexStatus.Unknown;
                contextSettings.ErrorIndexSettings.Location = StackHashErrorIndexLocation.Unknown;
                contextSettings.SqlSettings = StackHashSqlConfiguration.Default;

                // Override the connection string with the App.Config version.
                String defaultAppDataSqlConnectionString = AppSettings.DefaultSqlConnectionString;
                if (!String.IsNullOrEmpty(defaultAppDataSqlConnectionString))
                    contextSettings.SqlSettings.ConnectionString = defaultAppDataSqlConnectionString;

                // Override the connection string if running in test mode.
                String testSettingsConnectionString = TestSettings.GetAttribute("ConnectionString");
                if (!String.IsNullOrEmpty(testSettingsConnectionString))
                    contextSettings.SqlSettings.ConnectionString = testSettingsConnectionString;

                contextSettings.WinQualSyncSchedule = new ScheduleCollection();
                contextSettings.WinQualSyncSchedule.Add(new Schedule());
                contextSettings.WinQualSyncSchedule[0].Period = SchedulePeriod.Daily;
                contextSettings.WinQualSyncSchedule[0].Time = new ScheduleTime(22, 00, 0);
                contextSettings.WinQualSyncSchedule[0].DaysOfWeek =
                    DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday | DaysOfWeek.Thursday |
                    DaysOfWeek.Friday | DaysOfWeek.Saturday | DaysOfWeek.Sunday;

                contextSettings.CabFilePurgeSchedule = new ScheduleCollection();
                contextSettings.CabFilePurgeSchedule.Add(new Schedule());
                contextSettings.CabFilePurgeSchedule[0].Period = SchedulePeriod.Weekly;
                contextSettings.CabFilePurgeSchedule[0].Time = new ScheduleTime(23, 00, 0);
                contextSettings.CabFilePurgeSchedule[0].DaysOfWeek = DaysOfWeek.Sunday;

                contextSettings.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
                contextSettings.PurgeOptionsCollection.Add(new StackHashPurgeOptions());
                contextSettings.PurgeOptionsCollection[0].AgeToPurge = 180;
                contextSettings.PurgeOptionsCollection[0].PurgeObject = StackHashPurgeObject.PurgeGlobal;
                contextSettings.PurgeOptionsCollection[0].PurgeCabFiles = true;
                contextSettings.PurgeOptionsCollection[0].PurgeDumpFiles = true;
                
                contextSettings.DebuggerSettings = new StackHashDebuggerSettings();
                contextSettings.DebuggerSettings.DebuggerPathAndFileName = StackHashDebuggerSettings.Default32BitDebuggerPathAndFileName;
                contextSettings.DebuggerSettings.SymbolPath = StackHashSearchPath.DefaultSymbolPath;
                contextSettings.DebuggerSettings.BinaryPath = StackHashSearchPath.DefaultBinaryPath;

                contextSettings.DebuggerSettings.DebuggerPathAndFileName64Bit = StackHashDebuggerSettings.Default64BitDebuggerPathAndFileName;
                contextSettings.DebuggerSettings.SymbolPath64Bit = StackHashSearchPath.DefaultSymbolPath;
                contextSettings.DebuggerSettings.BinaryPath64Bit = StackHashSearchPath.DefaultBinaryPath;

                contextSettings.AnalysisSettings = new StackHashAnalysisSettings();
                contextSettings.AnalysisSettings.ForceRerun = false;

                contextSettings.CollectionPolicy = StackHashCollectionPolicyCollection.Default;

                // Initialise the bug tracker settings.
                contextSettings.BugTrackerSettings = new StackHashBugTrackerPlugInSettings();
                contextSettings.BugTrackerSettings.PlugInSettings = new StackHashBugTrackerPlugInCollection();

                contextSettings.EmailSettings = new StackHashEmailSettings();

                return contextSettings;
            }
        }

        /// <summary>
        /// Locates the context settings for the specified ID.
        /// </summary>
        /// <param name="id">ID of the context to search for.</param>
        /// <returns>Context settings or null.</returns>
        public StackHashContextSettings Find(int id)
        {
            Monitor.Enter(this);

            try
            {
                foreach (StackHashContextSettings settings in m_Settings.ContextCollection)
                {
                    if (settings.Id == id)
                        return settings;
                }
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}
