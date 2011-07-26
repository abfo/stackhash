using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Globalization;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    public abstract class AutoScriptBase
    {
        private String m_ScriptName;
        private int m_CurrentVersion;
        private String m_ScriptFileName;
        private StackHashScriptSettings m_ScriptSettings;


        protected AutoScriptBase(String scriptName, int currentVersion, String scriptFolder)
        {
            m_ScriptName = scriptName;
            m_CurrentVersion = currentVersion;
            m_ScriptFileName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", 
                Path.Combine(scriptFolder, scriptName), ScriptManager.ScriptExtension);
        }

        public String ScriptFileName
        {
            get
            {
                return m_ScriptFileName;
            }
        }

        public StackHashScriptSettings ScriptSettings
        {
            get
            {
                if (m_ScriptSettings == null)
                    m_ScriptSettings = GenerateScript();

                return m_ScriptSettings;
            }
        }

        
        /// <summary>
        /// The name of the auto script file (excluding the extension).
        /// </summary>
        public String ScriptName
        {
            get
            {
                return m_ScriptName;
            }
        }

        /// <summary>
        /// The current version of the script file. This is used to determine
        /// if the current saved version of the script file should be updated.
        /// </summary>
        public int CurrentVersion
        {
            get
            {
                return m_CurrentVersion;
            }
        }

        /// <summary>
        /// Determines if the specified version of the auto script file is up to date.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public bool IsScriptCurrent(StackHashScriptSettings script)
        {
            if (script == null)
                throw new ArgumentNullException("script");

            if (script.Name != m_ScriptName)
                throw new ArgumentException("Script file not specified", "script");

            return (script.Version == CurrentVersion);
        }


        /// <summary>
        /// Generates the auto script file in memory.
        /// </summary>
        /// <returns>Full script settings.</returns>
        public abstract StackHashScriptSettings GenerateScript();


        /// <summary>
        /// Gets an option on the specified line of the format...
        ///    name:  value
        /// where value is always returned as a string.
        /// Removes spaces between the : and value.
        /// </summary>
        /// <param name="line">Line of text to search.</param>
        /// <param name="name">Name to look for.</param>
        /// <returns>The value after the : with leading spaces removed. Null if not found.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1307")]
        protected static String GetColonOption(String line, String name)
        {
            if (line == null)
                throw new ArgumentNullException("line");
            if (name == null)
                throw new ArgumentNullException("name");

            int colon = line.IndexOf(name, StringComparison.OrdinalIgnoreCase);

            if (colon != -1)
            {
                colon += name.Length;

                if ((colon < line.Length) && (line[colon] == ':'))
                {
                    // Also strip any whitespace.
                    int startPosition = colon + 1;
                    while (startPosition < line.Length)
                    {
                        if (line[startPosition] != ' ')
                            break;
                        else
                            startPosition++;
                    }

                    if (startPosition < line.Length)
                        return (line.Substring(startPosition));
                }
            }

            return null;
        }


        /// <summary>
        /// Analyzes the output generated when running the autoscript against a dump file.
        /// Various details are extracted and presented in object form in StackHashDumpAnalysis.
        /// </summary>
        /// <param name="analysis">The analysis results so far.</param>
        /// <param name="results">The results file to analyze.</param>
        /// <returns>The results of the analysis. Null fields are underined.</returns>
        public abstract StackHashDumpAnalysis AnalyzeScriptResults(StackHashDumpAnalysis analysis, StackHashScriptResult results);


        /// <summary>
        /// Updates the file copy of the script if the code has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void UpdateScriptFile()
        {
            bool saveAutoScript = false;
            bool fileExists = File.Exists(m_ScriptFileName);

            if (fileExists)
            {
                // Load in the script and check the version number. If there is an error during load
                // then just create a new copy of the file.
                try
                {
                    StackHashScriptSettings thisScript = StackHashScriptSettings.Load(m_ScriptFileName);
                    saveAutoScript = !IsScriptCurrent(thisScript);
                }
                catch (System.Exception ex)
                {
                    String message = String.Format(CultureInfo.InvariantCulture, "Failed to load script {0} - Reconstructing", ScriptName);
                    DiagnosticsHelper.LogException(DiagSeverity.Warning, message, ex);
                    saveAutoScript = true;
                }
            }
            else
            {
                saveAutoScript = true;
            }
            
            FileAttributes currentAttributes;

            if (saveAutoScript)
            {
                if (fileExists)
                {
                    currentAttributes = File.GetAttributes(m_ScriptFileName);

                    // Turn off the readonly permission so the file can be updated.
                    if ((currentAttributes & FileAttributes.ReadOnly) != 0)
                    {
                        // Clear the read only flag.
                        File.SetAttributes(m_ScriptFileName, currentAttributes & ~FileAttributes.ReadOnly);
                    }
                }
                StackHashScriptSettings autoScript = GenerateScript();
                autoScript.Save(m_ScriptFileName);
            }

            // Make sure the file is marked read only so the client can't delete it.
            currentAttributes = File.GetAttributes(m_ScriptFileName);
            if ((currentAttributes & FileAttributes.ReadOnly) == 0)
            {
                // Set the read only flag.
                File.SetAttributes(m_ScriptFileName, currentAttributes | FileAttributes.ReadOnly);
            }
        }

        /// <summary>
        /// Removes the script file.
        /// </summary>
        public void RemoveScriptFile()
        {
            if (!File.Exists(m_ScriptFileName))
                return;

            FileAttributes currentAttributes = File.GetAttributes(m_ScriptFileName);

            // Turn off the readonly permission so the file can be removed.
            if ((currentAttributes & FileAttributes.ReadOnly) != 0)
            {
                // Clear the read only flag.
                File.SetAttributes(m_ScriptFileName, currentAttributes & ~FileAttributes.ReadOnly);
                File.Delete(m_ScriptFileName);
            }
        }
    }
}
