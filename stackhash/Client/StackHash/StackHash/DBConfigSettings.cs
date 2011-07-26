using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using StackHashUtilities;
using System.Globalization;
using System.Xml;

namespace StackHash
{
    /// <summary>
    /// Class used to pass settings between StackHash and StackHashDBConfig
    /// </summary>
    public class DBConfigSettings
    {
        private static readonly DBConfigSettings _settings = new DBConfigSettings();

        private string _settingsPath;

        private const int SaveRetryCount = 5;
        private const int SaveRetryWaitMs = 250;
        private const string SettingsFolder = "StackHash";
        private const string SettingsFile = "DBConfigSettings.xml";

        private const string ElementSettings = "DBConfigSettings";

        // persisted fields

        /// <summary>
        /// True if a new profile is being created, false if an existing profile is being edited
        /// </summary>
        public bool IsNewProfile { get; set; }
        private const string ElementIsNewProfile = "IsNewProfile";

        /// <summary>
        /// True if a profile is being upgraded from XML
        /// </summary>
        public bool IsUpgrade { get; set; }
        private const string ElementIsUpgrade = "IsUpgrade";

        /// <summary>
        /// True if the database files are stored in the cab folder
        /// </summary>
        public bool IsDatabaseInCabFolder { get; set; }
        private const string ElementIsDatabaseInCabFolder = "IsDatabaseInCabFolder";

        /// <summary>
        /// After invoking the config tool with IsNewProfile set to false DatabaseCopyRequired
        /// will be set to true if an index copy is required (instead of a move)
        /// </summary>
        public bool DatabaseCopyRequired { get; set; }
        private const string ElementDatabaseCopyRequired = "DatabaseCopyRequired";
        
        /// <summary>
        /// Gets or sets the selected database connection string
        /// </summary>
        public string ConnectionString { get; set; }
        private const string ElementConnectionString = "ConnectionString";

        /// <summary>
        /// Gets or sets the selected profile name - note, this is the database name
        /// </summary>
        public string ProfileName { get; set; }
        private const string ElementProfileName = "ProfileName";

        /// <summary>
        /// Gets or sets the selected profile folder - note, this is the cab folder
        /// </summary>
        public string ProfileFolder { get; set; }
        private const string ElementProfileFolder = "ProfileFolder";

        /// <summary>
        /// Gets or sets the stackhash service host
        /// </summary>
        public string ServiceHost { get; set; }
        private const string ElementServiceHost = "ServiceHost";

        /// <summary>
        /// Gets or sets the stackhash service port
        /// </summary>
        public int ServicePort { get; set; }
        private const string ElementServicePort = "ServicePort";

        /// <summary>
        /// Gets or sets the username of the account to impersonate when accessing the service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username")]
        public string ServiceUsername { get; set; }
        private const string ElementServiceUsername = "ServiceUsername";

        /// <summary>
        /// Gets or sets the password of the account to impersonate when accessing the service
        /// </summary>
        public string ServicePassword { get; set; }
        private const string ElementServicePassword = "ServicePassword";

        /// <summary>
        /// Gets or sets the domain of the account to impersonate when accessing the service
        /// </summary>
        public string ServiceDomain { get; set; }
        private const string ElementServiceDomain = "ServiceDomain";

        /// <summary>
        /// Gets the list of existing profile names
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> ExistingProfileNames { get; private set; }
        private const string ElementExistingProfileNames = "ExistingProfileNames";
        private const string ElementExistingProfileName = "ExistingProfileName";

        /// <summary>
        /// Gets the list of existing profile folders
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> ExistingProfileFolders { get; private set; }
        private const string ElementExistingProfileFolders = "ExistingProfileFolders";
        private const string ElementExistingProfileFolder = "ExistingProfileFolder";

        /// <summary>
        /// Gets the single instance of DBConfigSettings
        /// </summary>
        public static DBConfigSettings Settings
        {
            get { return DBConfigSettings._settings; }
        }

        /// <summary>
        /// Load DBConfigSettings
        /// </summary>
        public void Load()
        {
            ResetSettings();

            if (File.Exists(_settingsPath))
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.CheckCharacters = true;
                readerSettings.CloseInput = true;
                readerSettings.ConformanceLevel = ConformanceLevel.Document;
                readerSettings.IgnoreComments = true;
                readerSettings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(_settingsPath))
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case ElementConnectionString:
                                this.ConnectionString = reader.ReadString();
                                break;

                            case ElementProfileName:
                                this.ProfileName = reader.ReadString();
                                break;

                            case ElementProfileFolder:
                                this.ProfileFolder = reader.ReadString();
                                break;

                            case ElementServiceHost:
                                this.ServiceHost = reader.ReadString();
                                break;

                            case ElementServicePort:
                                this.ServicePort = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                break;

                            case ElementServiceUsername:
                                this.ServiceUsername = ClientUtils.DecryptString(reader.ReadString());
                                break;

                            case ElementServicePassword:
                                this.ServicePassword = ClientUtils.DecryptString(reader.ReadString());
                                break;

                            case ElementServiceDomain:
                                this.ServiceDomain = ClientUtils.DecryptString(reader.ReadString());
                                break;

                            case ElementIsNewProfile:
                                this.IsNewProfile = Convert.ToBoolean(reader.ReadString());
                                break;

                            case ElementIsUpgrade:
                                this.IsUpgrade = Convert.ToBoolean(reader.ReadString());
                                break;

                            case ElementIsDatabaseInCabFolder:
                                this.IsDatabaseInCabFolder = Convert.ToBoolean(reader.ReadString());
                                break;

                            case ElementDatabaseCopyRequired:
                                this.DatabaseCopyRequired = Convert.ToBoolean(reader.ReadString());
                                break;

                            case ElementExistingProfileName:
                                this.ExistingProfileNames.Add(reader.ReadString());
                                break;

                            case ElementExistingProfileFolder:
                                this.ExistingProfileFolders.Add(reader.ReadString());
                                break;
                        }

                    }

                }
            }
        }

        /// <summary>
        /// Save DBConfigSettings
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Save()
        {
            Exception finalException = null;

            for (int retry = 0; retry < SaveRetryCount; retry++)
            {
                try
                {
                    finalException = null;

                    if (retry > 0)
                    {
                        // wait a little bit before trying again
                        Thread.Sleep(SaveRetryWaitMs);

                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning,
                            string.Format(CultureInfo.InvariantCulture,
                            "DBConfigSettings.Save Retry attempt {0}",
                            retry));
                    }

                    SaveCore();
                    break;
                }
                catch (Exception ex)
                {
                    finalException = ex;

                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "DBConfigSettings.Save Failed",
                        ex);
                }
            }

            if (finalException != null)
            {
                throw finalException;
            }
        }

        private void SaveCore()
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.CheckCharacters = true;
            writerSettings.CloseOutput = true;
            writerSettings.ConformanceLevel = ConformanceLevel.Document;
            writerSettings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(_settingsPath, writerSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(ElementSettings);

                // ConnectionString
                if (!string.IsNullOrEmpty(this.ConnectionString))
                {
                    writer.WriteStartElement(ElementConnectionString);
                    writer.WriteString(this.ConnectionString);
                    writer.WriteEndElement();
                }

                // DatabaseCopyRequired
                writer.WriteStartElement(ElementDatabaseCopyRequired);
                writer.WriteValue(this.DatabaseCopyRequired);
                writer.WriteEndElement();

                // ExistingProfileFolders
                writer.WriteStartElement(ElementExistingProfileFolders);
                foreach (string folder in this.ExistingProfileFolders)
                {
                    writer.WriteStartElement(ElementExistingProfileFolder);
                    writer.WriteString(folder);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // ExistingProfileNames
                writer.WriteStartElement(ElementExistingProfileNames);
                foreach (string name in this.ExistingProfileNames)
                {
                    writer.WriteStartElement(ElementExistingProfileName);
                    writer.WriteString(name);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // IsDatabaseInCabFolder
                writer.WriteStartElement(ElementIsDatabaseInCabFolder);
                writer.WriteValue(this.IsDatabaseInCabFolder);
                writer.WriteEndElement();

                // IsNewProfile
                writer.WriteStartElement(ElementIsNewProfile);
                writer.WriteValue(this.IsNewProfile);
                writer.WriteEndElement();

                // IsUpgrade
                writer.WriteStartElement(ElementIsUpgrade);
                writer.WriteValue(this.IsUpgrade);
                writer.WriteEndElement();

                // ProfileFolder
                if (!string.IsNullOrEmpty(this.ProfileFolder))
                {
                    writer.WriteStartElement(ElementProfileFolder);
                    writer.WriteString(this.ProfileFolder);
                    writer.WriteEndElement();
                }

                // ProfileName
                if (!string.IsNullOrEmpty(this.ProfileName))
                {
                    writer.WriteStartElement(ElementProfileName);
                    writer.WriteString(this.ProfileName);
                    writer.WriteEndElement();
                }

                // ServiceDomain
                if (!string.IsNullOrEmpty(this.ServiceDomain))
                {
                    writer.WriteStartElement(ElementServiceDomain);
                    writer.WriteString(ClientUtils.EncryptString(this.ServiceDomain));
                    writer.WriteEndElement();
                }

                // ServiceHost
                if (!string.IsNullOrEmpty(this.ServiceHost))
                {
                    writer.WriteStartElement(ElementServiceHost);
                    writer.WriteString(this.ServiceHost);
                    writer.WriteEndElement();
                }

                // ServicePassword
                if (!string.IsNullOrEmpty(this.ServicePassword))
                {
                    writer.WriteStartElement(ElementServicePassword);
                    writer.WriteString(ClientUtils.EncryptString(this.ServicePassword));
                    writer.WriteEndElement();
                }

                // ServicePort
                writer.WriteStartElement(ElementServicePort);
                writer.WriteValue(this.ServicePort);
                writer.WriteEndElement();

                // ServiceUsername
                if (!string.IsNullOrEmpty(this.ServiceUsername))
                {
                    writer.WriteStartElement(ElementServiceUsername);
                    writer.WriteString(ClientUtils.EncryptString(this.ServiceUsername));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Reset to default values
        /// </summary>
        public void ResetSettings()
        {
            this.ConnectionString = null;
            this.ProfileName = null;
            this.ProfileFolder = null;
            this.IsNewProfile = true;
            this.IsUpgrade = false;
            this.IsDatabaseInCabFolder = false;
            this.DatabaseCopyRequired = false;
            this.ExistingProfileFolders.Clear();
            this.ExistingProfileNames.Clear();
            this.ServiceHost = "localhost";
            this.ServicePort = 9000;
            this.ServiceUsername = null;
            this.ServicePassword = null;
            this.ServiceDomain = null;
        }

        private DBConfigSettings()
        {
            string settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    SettingsFolder);

            if (!Directory.Exists(settingsFolder))
            {
                Directory.CreateDirectory(settingsFolder);
            }

            _settingsPath = Path.Combine(settingsFolder, SettingsFile);

            this.ExistingProfileNames = new List<string>();
            this.ExistingProfileFolders = new List<string>();

            ResetSettings();
        }
    }
}
