using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashBusinessObjects;

namespace StackHashDebug
{
    public interface IDebugger
    {
        bool AbortRequested
        {
            get;
            set;
        }

        String StandardError
        {
            get;
        }
        bool RunScript(StackHashDebuggerSettings debuggerSettings, bool dumpIs32Bit, String scriptFileName,
            String dumpPathAndFileName, String outputPath, String overrideSymbolPath, String overrideExePath, String overrideSourcePath);
        bool Use32BitDebugger(bool dumpIs32Bit, StackHashDebuggerSettings debuggerSettings);
    }
}
