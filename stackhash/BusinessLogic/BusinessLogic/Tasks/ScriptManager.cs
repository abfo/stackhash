using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    public class ScriptManager
    {
        private String m_ScriptFolder;
        private static String s_ScriptExtension = "xml";
        private static String s_ScriptWildcard = "*.xml";
        private List<AutoScriptBase> m_AutoScripts = new List<AutoScriptBase>();


        public static String ScriptExtension
        {
            get { return s_ScriptExtension; }
        }


        /// <summary>
        /// Manages debugger script files in the specified location.
        /// </summary>
        /// <param name="scriptFolder">Script file folder.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public ScriptManager(String scriptFolder)
        {
            if (scriptFolder == null)
                throw new ArgumentNullException("scriptFolder");

            m_ScriptFolder = scriptFolder;


            if (!Directory.Exists(scriptFolder))
                Directory.CreateDirectory(scriptFolder);

            // These are the scripts that will be run automatically on CAB files.
            // Add new auto scripts here.
            // Note that the GetOSVersionScript must be first as it gets the architecture which is used
            // to determine whether to invoke the 32 bit or 64 bit debugger for later scripts.
            m_AutoScripts.Add(new GetOSVersionScript(m_ScriptFolder));
            m_AutoScripts.Add(new AutoScript(m_ScriptFolder));

            foreach (AutoScriptBase autoScript in m_AutoScripts)
            {
                // Make sure the file is up to date.
                autoScript.UpdateScriptFile();
            }
        }

        public Collection<AutoScriptBase> AutoScripts
        {
            get
            {
                Collection<AutoScriptBase> autoScripts = new Collection<AutoScriptBase>();
                foreach (AutoScriptBase autoScript in m_AutoScripts)
                {
                    autoScripts.Add(autoScript);
                }
                return autoScripts;
            }
        }


        public bool IsAutoScript(String scriptName)
        {
            foreach (AutoScriptBase autoScript in m_AutoScripts)
            {
                if (String.Compare(autoScript.ScriptName, scriptName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }

        public int NumberOfAutoScripts
        {
            get
            {
                return m_AutoScripts.Count;
            }
        }

        private bool isAutoScript(String scriptName)
        {
            foreach (AutoScriptBase autoScript in m_AutoScripts)
            {
                if (String.Compare(autoScript.ScriptName, scriptName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Gets all script contexts.
        /// </summary>
        /// <returns>Full list of all the script settings.</returns>
        private StackHashScriptCollection GetAllScripts()
        {
            StackHashScriptCollection allScripts = new StackHashScriptCollection();

            String [] allScriptFiles = Directory.GetFiles(m_ScriptFolder, s_ScriptWildcard);

            foreach (String fileName in allScriptFiles)
            {
                StackHashScriptSettings thisScript = StackHashScriptSettings.Load(fileName);

                allScripts.Add(thisScript);
            }

            return allScripts;
        }


        /// <summary>
        /// Gets a list of script names.
        /// This will include the readonly AutoScript.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public StackHashScriptFileDataCollection ScriptNames
        {
            get
            {
                StackHashScriptFileDataCollection allScripts = new StackHashScriptFileDataCollection();

                String[] allScriptFiles = Directory.GetFiles(m_ScriptFolder, s_ScriptWildcard);

                foreach (String fileName in allScriptFiles)
                {
                    // The filename will be the name of the script.
                    StackHashScriptFileData fileData = new StackHashScriptFileData();
                    fileData.Name = Path.GetFileNameWithoutExtension(fileName);
                    fileData.CreationDate = File.GetCreationTimeUtc(fileName);
                    fileData.LastModifiedDate = File.GetLastWriteTimeUtc(fileName);
                    fileData.IsReadOnly = ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) != 0);

                    try
                    {
                        // Load the script file to determine the settings.
                        StackHashScriptSettings scriptSettings = LoadScript(fileData.Name);
                        fileData.IsReadOnly = scriptSettings.IsReadOnly;
                        fileData.RunAutomatically = scriptSettings.RunAutomatically;
                        fileData.DumpType = scriptSettings.DumpType;
                        fileData.CreationDate = scriptSettings.CreationDate;
                        fileData.LastModifiedDate = scriptSettings.LastModifiedDate;
                    }
                    catch (System.Exception ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Unable to load script file " + fileData.Name, ex);
                    }

                    allScripts.Add(fileData);
                }

                return allScripts;
            }
        }


        /// <summary>
        /// Add a new script.
        /// </summary>
        /// <param name="script">Full script settings.</param>
        /// <param name="overwrite">true - Overwrite any old script of the same name.</param>
        /// <param name="allowReadOnlyScriptOverwrite">true - allow read only script to be overwritten.</param>
        public void AddScript(StackHashScriptSettings script, bool overwrite, bool allowReadOnlyScriptOverwrite)
        {
            if (script == null)
                throw new ArgumentNullException("script");

            String fileName = getScriptFileName(script.Name);

            if (File.Exists(fileName))
            {
                if (!overwrite)
                {
                    throw new StackHashException("Script file already exists and overWrite not set", StackHashServiceErrorCode.CannotOverwriteScriptFile);
                }
                else
                {
                    // File already exists. The Created time should remain the same. The update time should change.
                    StackHashScriptSettings oldScript = StackHashScriptSettings.Load(fileName);

                    if (oldScript.IsReadOnly && !allowReadOnlyScriptOverwrite)
                        throw new InvalidOperationException("Cannot overwrite Read Only script");

                    script.CreationDate = oldScript.CreationDate;
                    script.LastModifiedDate = DateTime.Now.ToUniversalTime();
                }
            }
            else
            {
                script.CreationDate = DateTime.Now.ToUniversalTime();
                script.LastModifiedDate = script.CreationDate;
            }

            script.Save(fileName);
        }


        /// <summary>
        /// Add a new script.
        /// </summary>
        /// <param name="script">Full script settings.</param>
        /// <param name="overwrite">true - Overwrite any old script of the same name.</param>
        public void AddScript(StackHashScriptSettings script, bool overwrite)
        {
            AddScript(script, overwrite, false); // Don't allow read only scripts to be overwritten.
        }


        /// <summary>
        /// Removes the specified script.
        /// </summary>
        /// <param name="scriptName">Name of script to remove.</param>
        public void RemoveScript(String scriptName)
        {
            // Cannot create an autoscript from the client.
            if (isAutoScript(scriptName))
                throw new StackHashException("Cannot remove system scripts", StackHashServiceErrorCode.CannotRemoveSystemScripts);

            String fileName = getScriptFileName(scriptName);

            // Load it to see if it is read only.
            if (File.Exists(fileName))
            {
                StackHashScriptSettings script = StackHashScriptSettings.Load(fileName);

                if (!script.IsReadOnly)
                    File.Delete(fileName);
                else
                    throw new StackHashException("Cannot remove Read Only script", StackHashServiceErrorCode.CannotRemoveSystemScripts);
            }
        }

        /// <summary>
        /// Removes the automatic scripts.
        /// </summary>
        public void RemoveAutoScripts()
        {
            foreach (AutoScriptBase autoScript in m_AutoScripts)
            {
                autoScript.RemoveScriptFile();
            }
        }

        /// <summary>
        /// Renames the specified script.
        /// </summary>
        /// <param name="oldScriptName">Current name of the script.</param>
        /// <param name="newScriptName">New name of the script.</param>
        public void RenameScript(String oldScriptName, String newScriptName)
        {
            if (string.IsNullOrEmpty(oldScriptName))
                throw new ArgumentNullException("oldScriptName");
            if (string.IsNullOrEmpty(newScriptName))
                throw new ArgumentNullException("newScriptName");

            // Don't do anything if the name hasn't changed.
            if (oldScriptName == newScriptName)
                return;

            String fileName = getScriptFileName(oldScriptName);

            if (!File.Exists(fileName))
                throw new StackHashException("Script name does not exist during rename", StackHashServiceErrorCode.ScriptDoesNotExist);

            // Load in the script and save it to the new name.
            StackHashScriptSettings thisScript = StackHashScriptSettings.Load(fileName);
            thisScript.Name = newScriptName;
            thisScript.LastModifiedDate = DateTime.Now.ToUniversalTime();

            String newFileName = getScriptFileName(newScriptName);

            thisScript.Save(newFileName);

            // Delete the old script as the renamed version is now available.
            File.Delete(fileName);
        }


        /// <summary>
        /// Gets the full path to the specified script name.
        /// </summary>
        /// <param name="scriptName">Name of the script to find.</param>
        /// <returns>Full path and filename of the script.</returns>
        private String getScriptFileName(string scriptName)
        {
            String fileName = String.Format(CultureInfo.InvariantCulture,
                "{0}\\{1}.{2}", m_ScriptFolder, scriptName, s_ScriptExtension);

            return fileName;
        }


        /// <summary>
        /// Loads the specified script file.
        /// </summary>
        /// <param name="scriptName">Script to load.</param>
        /// <returns>The script settings.</returns>
        public StackHashScriptSettings LoadScript(string scriptName)
        {
            return LoadScript(scriptName, false);
        }

        /// <summary>
        /// Loads the specified script file.
        /// </summary>
        /// <param name="scriptName">Script to load.</param>
        /// <returns>The script settings.</returns>
        public StackHashScriptSettings LoadScript(string scriptName, bool noException)
        {
            String fileName = getScriptFileName(scriptName);

            if (!File.Exists(fileName))
            {
                if (noException)
                    return null;
                else
                    throw new StackHashException("Script file does not exist when loading script.", StackHashServiceErrorCode.ScriptDoesNotExist);
            }

            StackHashScriptSettings newScriptSettings = StackHashScriptSettings.Load(fileName);
            return newScriptSettings;
        }


        /// <summary>
        /// Determines if the specified script exists or not.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public bool ScriptExists(String scriptName)
        {
            String fileName = getScriptFileName(scriptName);

            return File.Exists(fileName);
        }
    }
}
