using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;

namespace StackHashTasks
{
    public class AutoScript : AutoScriptBase
    {
        // Version 2 introduced support to check for x64 .NET version.
        // Version 4 - forces the RunAutomatically flag to be set.
        // Version 5 - sets dump type to run on all types.
        // Version 6 - supports getting the version of the .NET CLR for a version 4 framework dump file.
        private static int s_CurrentVersion = 6;
        private static String s_ScriptName = "AutoScript";

        private static String s_DebuggerVersionCommand = "version";
        private static String s_DebuggerVersionComment = "Get the debugger version";

        private static String s_VerTargetCommand = "vertarget";
        private static String s_VerTargetComment = "Get the version info and system up time";

        // Note can have more than 1 version of .NET loaded at a time.
        private static String s_MscoreModuleCommand = "lm v m mscorwks";
        private static String s_MscoreModuleComment = "Get the version of .NET loaded";

        private static String s_MscoreModuleCommand2 = "lm v m clr";
        private static String s_MscoreModuleComment2 = "Get the version of .NET 4 loaded";


        public AutoScript(String scriptFolder)
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
            scriptSettings.Name = s_ScriptName;
            scriptSettings.Owner = StackHashScriptOwner.System;
            scriptSettings.Version = s_CurrentVersion;
            scriptSettings.IsReadOnly = true;
            scriptSettings.RunAutomatically = true; // This is automatic but not defined by the user.
            scriptSettings.DumpType = StackHashScriptDumpType.UnmanagedAndManaged; // Run script on all types of dump.
            scriptSettings.Script = new StackHashScript();
            scriptSettings.Script.Add(new StackHashScriptLine(s_DebuggerVersionCommand, s_DebuggerVersionComment));
            scriptSettings.Script.Add(new StackHashScriptLine(s_VerTargetCommand, s_VerTargetComment));
            scriptSettings.Script.Add(new StackHashScriptLine(s_MscoreModuleCommand, s_MscoreModuleComment));
            scriptSettings.Script.Add(new StackHashScriptLine(s_MscoreModuleCommand2, s_MscoreModuleComment2));

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
        /// Process the output of the lm v m mscorwks command.
        /// This gives the .NET version loaded in the dump.
        /// </summary>
        /// <param name="analysis">The results structure to update.</param>
        /// <param name="commandOutput">The result data to analyze</param>
        private static void processMscorwksResults(StackHashDumpAnalysis analysis, StackHashScriptLineResult commandOutput)
        {
            foreach (String line in commandOutput.ScriptLineOutput)
            {
                String value = null;
                if ((value = GetColonOption(line, "Product version")) != null)
                    analysis.DotNetVersion = value;
            }
        }


        /// <summary>
        /// Analyzes the output generated when running the autoscript against a dump file.
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
                else if (commandOutput.ScriptLine.Comment.Contains(s_MscoreModuleComment))
                {
                    // Get the .NET version information. For V3.5 and below the CLR version is in mscorwks.dll
                    processMscorwksResults(analysis, commandOutput);
                }
                else if (commandOutput.ScriptLine.Comment.Contains(s_MscoreModuleComment2))
                {
                    // Get the .NET version information. For V4.0 and above (so far) the CLR version is in clr.dll
                    processMscorwksResults(analysis, commandOutput);
                }
            }

            return analysis;
        }
    }
}
