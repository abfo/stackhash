using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashUtilities;


namespace StackHashBusinessObjects
{
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashSearchPath : Collection<String>
    {
        public StackHashSearchPath() { } // Needed to serialize.

        [DataMember]
        private const String s_MicrosoftSymbolServer = @"http://msdl.microsoft.com/download/symbols";

        [DataMember]
        private const String s_DefaultLocalStore = @"c:\localcache";

        public static StackHashSearchPath DefaultSymbolPath
        {
            get
            {
                StackHashSearchPath defaultSymbolPath = new StackHashSearchPath();

                // Add the default microsoft symbol server and store.
                String symServer = String.Format(CultureInfo.InvariantCulture, "SRV*{0}*{1}", s_DefaultLocalStore, s_MicrosoftSymbolServer);

                defaultSymbolPath.Add(symServer);
                return defaultSymbolPath;
            }
        }

        public static StackHashSearchPath DefaultBinaryPath
        {
            get
            {
                StackHashSearchPath defaultBinaryPath = new StackHashSearchPath();

                // Add the default microsoft symbol server and store.
                String symServer = String.Format(CultureInfo.InvariantCulture, "SRV*{0}*{1}", s_DefaultLocalStore, s_MicrosoftSymbolServer);

                defaultBinaryPath.Add(symServer);
                return defaultBinaryPath;
            }
        }

        public String FullPath
        {
            get
            {
                String fullPath = "";
                foreach (String path in this)
                {
                    fullPath += String.Format(CultureInfo.InvariantCulture, "{0};", path);
                }
                return fullPath;
            }
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashDebuggerSettings
    {
        private StackHashSearchPath m_SymbolPath;
        private StackHashSearchPath m_BinaryPath;
        private String m_DebuggerPathAndFileName;

        private StackHashSearchPath m_SymbolPath64Bit;
        private StackHashSearchPath m_BinaryPath64Bit;
        private String m_DebuggerPathAndFileName64Bit;

        [DataMember]
        private const String s_Default32BitDebuggerToolsForWindowsPathOn32BitWindows = @"C:\Program Files\Debugging Tools for Windows\cdb.exe";
        [DataMember]
        private const String s_Default32BitDebuggerToolsForWindowsPathOn32BitWindowsAlternative = @"C:\Program Files\Debugging Tools for Windows (x86)\cdb.exe";
        [DataMember]
        private const String s_Default32BitDebuggerToolsForWindowsPathOn64BitWindows = @"C:\Program Files (x86)\Debugging Tools for Windows (x86)\cdb.exe";

        // 64 bit debugger location on 64 bit machine and 32 bit machine - NB: Can't run a 64 bit debugger on 32 bit windows.
        [DataMember]
        private const String s_Default64BitDebuggerToolsForWindowsPathOn32BitWindows = @"C:\Program Files\Debugging Tools for Windows (x64)\cdb.exe";
        [DataMember]
        private const String s_Default64BitDebuggerToolsForWindowsPathOn64BitWindows = @"C:\Program Files\Debugging Tools for Windows (x64)\cdb.exe";

        [DataMember]
        public StackHashSearchPath SymbolPath
        {
            get { return m_SymbolPath; }
            set { m_SymbolPath = value; }
        }

        [DataMember]
        public StackHashSearchPath BinaryPath
        {
            get { return m_BinaryPath; }
            set { m_BinaryPath = value; }
        }

        [DataMember]
        public String DebuggerPathAndFileName
        {
            get { return m_DebuggerPathAndFileName; }
            set { m_DebuggerPathAndFileName = value; }
        }

        [DataMember]
        public StackHashSearchPath SymbolPath64Bit
        {
            get { return m_SymbolPath64Bit; }
            set { m_SymbolPath64Bit = value; }
        }

        [DataMember]
        public StackHashSearchPath BinaryPath64Bit
        {
            get { return m_BinaryPath64Bit; }
            set { m_BinaryPath64Bit = value; }
        }

        [DataMember]
        public String DebuggerPathAndFileName64Bit
        {
            get { return m_DebuggerPathAndFileName64Bit; }
            set { m_DebuggerPathAndFileName64Bit = value; }
        }

        public static String Default32BitDebuggerPathAndFileName
        {
            get
            {
                if (StackHashUtilities.SystemInformation.Is64BitSystem())
                {
                    return StackHashDebuggerSettings.s_Default32BitDebuggerToolsForWindowsPathOn64BitWindows;
                }
                else
                {
                    if (File.Exists(s_Default32BitDebuggerToolsForWindowsPathOn32BitWindowsAlternative))
                        return StackHashDebuggerSettings.s_Default32BitDebuggerToolsForWindowsPathOn32BitWindowsAlternative;
                    else
                        return StackHashDebuggerSettings.s_Default32BitDebuggerToolsForWindowsPathOn32BitWindows;
                }
            }
        }
        public static String Default64BitDebuggerPathAndFileName
        {
            get
            {
                if (StackHashUtilities.SystemInformation.Is64BitSystem())
                    return StackHashDebuggerSettings.s_Default64BitDebuggerToolsForWindowsPathOn64BitWindows;
                else
                    return StackHashDebuggerSettings.s_Default64BitDebuggerToolsForWindowsPathOn32BitWindows;
            }
        }
    }



}
