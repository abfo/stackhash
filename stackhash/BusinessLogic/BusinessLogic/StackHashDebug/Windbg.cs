using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using StackHashBusinessObjects;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using StackHashUtilities;

namespace StackHashDebug
{
    public class Windbg : IDebugger
    {
        StringBuilder m_LastStderr;
        bool m_AbortRequested;
        int m_LastExitCode;


        public bool AbortRequested
        {
            get { return m_AbortRequested; }
            set { m_AbortRequested = value; }
        }

        public int LastExitCode
        {
            get { return m_LastExitCode; }
        }

        public String StandardError
        {
            get { return m_LastStderr.ToString(); }
        }


        /// <summary>
        /// Standard error written to by child process.
        /// </summary>
        /// <param name="errLine">The child process.</param>

        void ErrorReceived(Object sendingProcess, DataReceivedEventArgs errLine)
        {
	        if ((errLine != null) && (!String.IsNullOrEmpty(errLine.Data)))
                m_LastStderr.AppendLine(errLine.Data);
        }

        #region IDebugger Members


        /// <summary>
        /// Determines if the 32 bit or 64 bit debugger settings should be used.
        /// The 32 bit settings are used unless...
        /// 
        /// 1) We are running on a 64 bit machine AND
        /// 2) 64 bit debugger specified and exists AND
        /// 3) (Dump is 64 bit) OR (Dump is 32 bit but no 32 bit debugger specified or exists)
        /// </summary>
        /// <param name="dumpIs32Bit">Flavour of the dump file.</param>
        /// <param name="debuggerSettings">Current debugger settings.</param>
        /// <returns>true - use 32 bit debugger settings, false - use 64 bit debugger settings.</returns>
        public bool Use32BitDebugger(bool dumpIs32Bit, StackHashDebuggerSettings debuggerSettings)
        {
            if (debuggerSettings == null)
                throw new ArgumentNullException("debuggerSettings");

            bool hostIs32Bit = !SystemInformation.Is64BitSystem();

            // Only have the option of using the 64 bit debugger on a host that is 64 bit.
            if (!hostIs32Bit)
            {
                if ((debuggerSettings.DebuggerPathAndFileName == null) ||           // No 32 bit debugger specified.
                    (!File.Exists(debuggerSettings.DebuggerPathAndFileName)) ||     // 32 bit debugger specified but path invalid.
                    !dumpIs32Bit)                                                   // The dump is 64 bit.
                {
                    // Still only use the 64 bit debugger if it exists.
                    if ((debuggerSettings.DebuggerPathAndFileName64Bit != null) &&
                        (File.Exists(debuggerSettings.DebuggerPathAndFileName64Bit)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        
        /// <summary>
        /// Runs the specigied debugger script using the settings specified debugger settings.
        /// </summary>
        /// <param name="debuggerSettings">Debugger environment settings.</param>
        /// <param name="dumpIs32Bit">true - dump is 32 bit machine, false - 64 bit dump.</param>
        /// <param name="scriptFileName">Path and filename of the script file.</param>
        /// <param name="dumpPathAndFileName">Full path and filename of the dump file.</param>
        /// <param name="outputPath">Output folder - the file will be named the same as the script.</param>
        /// <param name="overrideSymbolPath">Overridden symbol path.</param>
        /// <param name="overrideExePath">Overridden exe path.</param>
        /// <param name="overrideSourcePath">Overridden source path.</param>
        /// <returns>true - successfully run, false - failed to run.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public bool RunScript(StackHashDebuggerSettings debuggerSettings, 
                              bool dumpIs32Bit,
                              String scriptFileName,
                              String dumpPathAndFileName,
                              String outputPath,
                              String overrideSymbolPath,
                              String overrideExePath,
                              String overrideSourcePath)
        {
            if (debuggerSettings == null)
                throw new ArgumentNullException("debuggerSettings");
            if (scriptFileName == null)
                throw new ArgumentNullException("scriptFileName");

            // Default to using the 32 bit debugger unless the dump and host are 64 bit.
            String debuggerPath = String.Empty;
            String symbolPath = String.Empty;
            String binaryPath = String.Empty;
            String sourcePath = String.Empty;

            // Only have the option of using the 64 bit debugger on a host that is 64 bit.
            if (Use32BitDebugger(dumpIs32Bit, debuggerSettings))
            {
                debuggerPath = debuggerSettings.DebuggerPathAndFileName;

                if (debuggerSettings.SymbolPath != null)
                    symbolPath = debuggerSettings.SymbolPath.FullPath;

                if (debuggerSettings.BinaryPath != null)
                    binaryPath = debuggerSettings.BinaryPath.FullPath;
            }
            else
            {
                debuggerPath = debuggerSettings.DebuggerPathAndFileName64Bit;

                if (debuggerSettings.SymbolPath64Bit != null)
                    symbolPath = debuggerSettings.SymbolPath64Bit.FullPath;

                if (debuggerSettings.BinaryPath64Bit != null)
                    binaryPath = debuggerSettings.BinaryPath64Bit.FullPath;
            }

            if (String.IsNullOrEmpty(debuggerPath))
                throw new ArgumentException("debugger path cannot be null", "debuggerSettings");

            if (overrideSymbolPath != null)
                symbolPath = overrideSymbolPath;

            if (overrideExePath != null)
                binaryPath = overrideExePath;

            if (overrideSourcePath != null)
                sourcePath = overrideSourcePath;
            
            if (symbolPath == null)
                throw new ArgumentException("symbol path cannot be null", "debuggerSettings");

            
            // Clear any previous abort request.
            AbortRequested = false;
            m_LastStderr = new StringBuilder();
            m_LastExitCode = 0;

            //String commandLine = String.Format(CultureInfo.InvariantCulture,
            //    "-y \"{0}\" -i \"{1}\" -z \"{2}\" -c \"$$><{3};q\"",
            //    symbolPath, binaryPath, dumpPathAndFileName, scriptFileName); 

            
            // The $$>< is required because we need to put ;q (quit) after the script is run.
            // and the filename may have spaces.
            // The >< is required so people can use debugger control commands.
            String commandLine = String.Format(CultureInfo.InvariantCulture,
                "-y \"{0}\" -i \"{1}\" -z \"{2}\" -srcpath \"{3}\" -c \"$$><{4};q\"",
                symbolPath, binaryPath, dumpPathAndFileName, sourcePath, scriptFileName);


 //           commandLine = "-y-z-df--djihj$$";

            String debuggerName = Path.GetFileName(debuggerPath);

            if (String.Compare(debuggerName, "windbg.exe", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // WinDbg - command line options.
                // -Q   - Suppresses the "Save Workspace?" dialog box. Workspaces are not automatically saved. See Using Workspaces for details.
                // -QS  - Suppresses the "Reload Source?" dialog box. Source files are not automatically reloaded.
                // -QSY - Suppresses the "Reload Source?" dialog box and automatically reloads source files.
                // -QY  - Suppresses the "Save Workspace?" dialog box and automatically saves workspaces. See Using Workspaces for details.
                commandLine += " -Q -QS -QY -QSY";
            }

            Process debuggerProcess = null;

            try
            {
                debuggerProcess = new Process();
                debuggerProcess.StartInfo.FileName = debuggerPath;
                debuggerProcess.StartInfo.Arguments = commandLine;
                debuggerProcess.StartInfo.UseShellExecute = false;
                debuggerProcess.StartInfo.RedirectStandardError = true;
                debuggerProcess.StartInfo.RedirectStandardOutput = true;
                debuggerProcess.StartInfo.CreateNoWindow = true;

                // StdError doesn't appear to be used - so in the case of a command line error - need the stdout too.
                debuggerProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(this.ErrorReceived);
                debuggerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(this.ErrorReceived);

                try
                {
                    Environment.SetEnvironmentVariable("DBGHELP", "1");
                    Environment.SetEnvironmentVariable("WININET", "1");
                    debuggerProcess.Start();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    throw new StackHashException("Failed to run debugger: " + commandLine, ex, StackHashServiceErrorCode.FailedToRunDebugger);
                }

                // Start the event handler - which will read data written to stderr into m_LastStderr.
                debuggerProcess.BeginErrorReadLine();
                debuggerProcess.BeginOutputReadLine();

                while (!AbortRequested)
                {
                    debuggerProcess.WaitForExit(2000);
                    if (debuggerProcess.HasExited)
                        break;
                }

                if (AbortRequested)
                {
                    // Abort the process.
                    if (!debuggerProcess.HasExited)
                    {
                        try
                        {
                            debuggerProcess.Kill();
                            debuggerProcess.WaitForExit();
                        }
                        catch (System.ComponentModel.Win32Exception) { }
                        catch (System.InvalidOperationException) { }
                        catch (System.SystemException) { }
                    }
                }
                m_LastExitCode = debuggerProcess.ExitCode;

                // Give the stderror text callback a chance to be called one more time or we
                // might miss the final error.
                Thread.Sleep(200);
            }
            finally
            {
                // Dispose any resources used by the process creation.
                if (debuggerProcess != null)
                {
                    try
                    {
                        debuggerProcess.ErrorDataReceived -= new System.Diagnostics.DataReceivedEventHandler(this.ErrorReceived);
                    }
                    finally
                    {
                        debuggerProcess.Dispose();
                    }
                }
            }

	        return !AbortRequested;
        }

        #endregion
    }
}
