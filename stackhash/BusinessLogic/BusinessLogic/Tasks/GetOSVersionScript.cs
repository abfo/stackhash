using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;

namespace StackHashTasks
{
    public class GetOSVersionScript : AutoScriptBase
    {
        // Version 2 - forces the RunAutomatically flag to be set.
        // Version 3 - sets dump type to both.
        private static int s_CurrentVersion = 3;
        private static String s_ScriptName = "GetOSVersion";

        private static String s_VerTargetCommand = "vertarget";
        private static String s_VerTargetComment = "Get the version info and system up time";

        /// <summary>
        /// Constructor for th GetOSVersionSCript. Initializes the base Script class.
        /// </summary>
        /// <param name="scriptFolder">Folder where script is stored.</param>
        public GetOSVersionScript(String scriptFolder)
            : base(s_ScriptName, s_CurrentVersion, scriptFolder)
        {
        }

        /// <summary>
        /// Generates the auto script file in memory.
        /// </summary>
        /// <returns>Full script settings.</returns>
        public override StackHashScriptSettings GenerateScript()
        {
            StackHashScriptSettings scriptSettings = new StackHashScriptSettings();
            scriptSettings.CreationDate = DateTime.Now.ToUniversalTime();
            scriptSettings.LastModifiedDate = scriptSettings.CreationDate;
            scriptSettings.Name = base.ScriptName;
            scriptSettings.Owner = StackHashScriptOwner.System;
            scriptSettings.RunAutomatically = true;
            scriptSettings.Version = base.CurrentVersion;
            scriptSettings.IsReadOnly = true;
            scriptSettings.DumpType = StackHashScriptDumpType.UnmanagedAndManaged; // Run script on all types of dump.
            scriptSettings.Script = new StackHashScript();
            scriptSettings.Script.Add(new StackHashScriptLine(s_VerTargetCommand, s_VerTargetComment));

            return scriptSettings;
        }


        /// <summary>
        /// Process the output of the vertarget command.
        /// </summary>
        /// <param name="analysis">The results structure to update.</param>
        /// <param name="commandOutput">The result data to analyze.</param>
        private static void processVerTargetResults(StackHashDumpAnalysis analysis, StackHashScriptLineResult commandOutput)
        {
            foreach (String line in commandOutput.ScriptLineOutput)
            {
                String value = null;
                if ((value = GetColonOption(line, "System Uptime")) != null)
                    analysis.SystemUpTime = value;
                else if ((value = GetColonOption(line, "Process Uptime")) != null)
                    analysis.ProcessUpTime = value;
                else if (line.StartsWith("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Contains("x64") || line.Contains("X64"))
                    {
                        analysis.OSVersion = line;
                        analysis.MachineArchitecture = "x64";
                    }
                    else if (line.Contains("x86") || line.Contains("X86"))
                    {
                        analysis.OSVersion = line;
                        analysis.MachineArchitecture = "x86";
                    }
                }
            }
        }


        /// <summary>
        /// Analyzes the output generated when running the script against a dump file.
        /// Various details are extracted and presented in object form in StackHashDumpAnalysis.
        /// </summary>
        /// <param name="analysis">The analysis results so far.</param>
        /// <param name="results">The results file to analyze.</param>
        /// <returns>The results of the analysis. Null fields are underined.</returns>
        public override StackHashDumpAnalysis AnalyzeScriptResults(StackHashDumpAnalysis analysis, StackHashScriptResult results)
        {
            if (analysis == null)
                throw new ArgumentNullException("analysis");
            if (results == null)
                throw new ArgumentNullException("results");

            // Work through all of the command output looking for useful data.
            foreach (StackHashScriptLineResult commandOutput in results.ScriptResults)
            {
                if (commandOutput.ScriptLine.Comment.Contains(s_VerTargetComment))
                {
                    // Get the version information.
                    processVerTargetResults(analysis, commandOutput);
                }
            }

            return analysis;
        }
    }
}
