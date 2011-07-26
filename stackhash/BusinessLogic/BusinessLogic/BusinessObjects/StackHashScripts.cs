using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

using StackHashUtilities;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashScriptDumpType
    {
        [EnumMember()]
        UnmanagedAndManaged,
        [EnumMember()]
        UnmanagedOnly,
        [EnumMember()]
        ManagedOnly,
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashScriptDumpArchitecture
    {
        [EnumMember()]
        X86,
        [EnumMember()]
        Amd64,
        [EnumMember()]
        IA64
    }
    
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashScriptLine
    {
        private String m_Command;
        private String m_Comment;

        public StackHashScriptLine() { } // Needed to serialize.
        public StackHashScriptLine(String command, String comment)
        {
            // Comment CAN be null.
            if (command == null)
                throw new ArgumentNullException("command");

            m_Command = command;

            if (String.IsNullOrEmpty(comment))
                m_Comment = "";
            else
                m_Comment = comment;
        }

        [DataMember]
        public String Command
        {
            get { return m_Command; }
            set { m_Command = value; }
        }
        [DataMember]
        public String Comment
        {
            get { return m_Comment; }
            set { m_Comment = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScript : Collection<StackHashScriptLine>
    {
        public StackHashScript() { } // Needed to serialize.
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashScriptOwner
    {
        [EnumMember()]
        User,
        [EnumMember()]
        System
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashScriptSettings
    {
        private StackHashScript m_Script;
        private String m_Name;
        private DateTime m_CreationDate;
        private DateTime m_LastModifiedDate;
        private int m_Version;
        private StackHashScriptOwner m_Type;
        private bool m_IsReadOnly;
        private bool m_RunAutomatically;
        private StackHashScriptDumpType m_DumpType;
        private static XmlSerializer s_XmlSerializer;

        public StackHashScriptSettings() { ;} // Required for serialization.

        static StackHashScriptSettings()
        {
            initialiseSerializer();
        }

        public StackHashScriptSettings(String name, StackHashScript script)
        {
            if (script == null)
                throw new ArgumentNullException("script");

            m_Name = name;
            m_Script = script;

            // Need to fix the date and time so that both creation date and last modified date are the same.
            DateTime thisDate = DateTime.Now.ToUniversalTime();
            m_CreationDate = thisDate;
            m_LastModifiedDate = thisDate;
        }

        [DataMember]
        public StackHashScript Script
        {
            get { return m_Script; }
            set { m_Script = value; }
        }

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public DateTime CreationDate
        {
            get { return m_CreationDate; }
            set { m_CreationDate = value; }
        }

        [DataMember]
        public DateTime LastModifiedDate
        {
            get { return m_LastModifiedDate; }
            set { m_LastModifiedDate = value; }
        }

        [DataMember]
        public int Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        [DataMember]
        public StackHashScriptOwner Owner
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        [DataMember]
        public bool IsReadOnly
        {
            get { return m_IsReadOnly; }
            set { m_IsReadOnly = value; }
        }

        [DataMember]
        public bool RunAutomatically
        {
            get { return m_RunAutomatically; }
            set { m_RunAutomatically = value; }
        }

        [DataMember]
        public StackHashScriptDumpType DumpType
        {
            get { return m_DumpType; }
            set { m_DumpType = value; }
        }

        /// <summary>
        /// Appends the path part of the specified command line to the originalPath specified.
        /// </summary>
        /// <param name="originalPath">Original path to append to - can be null</param>
        /// <param name="commandLine">Command line to append.</param>
        /// <param name="command">Command within the command line. e.g. ".sympath", ".srcpath", ".exepath"</param>
        /// <returns>originalPath if not found, combined originalPath and commandLine path if found</returns>
        public static String GetUnquotedPathFromCommandLine(String originalPath, String commandLine, String command)
        {
            if (commandLine == null)
                throw new ArgumentNullException("commandLine");
            if (command == null)
                throw new ArgumentNullException("command");


            // Check if the specified command is present in the commandline.
            int index = commandLine.IndexOf(command, StringComparison.OrdinalIgnoreCase);

            // If command not found then just return the original path.
            if (index == -1)
                return originalPath;

            // Move past the command.
            index += command.Length;

            // Check if there is no command parameter. e.g. just .sympath on its own.
            if (index >= commandLine.Length)
                return originalPath;

            // Get the path parameter and strip any leading or training whitespace.
            String pathParameter = commandLine.Substring(index).Trim();

            if (String.IsNullOrEmpty(pathParameter))
                return originalPath;

            bool concatenate = false;
            // Check for the + operator. This means concatenate the path.
            if (pathParameter[0] == '+')
            {
                // Move past the +
                pathParameter = pathParameter.Substring(1);

                concatenate = true;
            }

            /// Also strip any leading and trailing quotes. Also get rid of any inner spaces.
            pathParameter = pathParameter.Trim(new char[] { '"', ' ' });

            // If null or empty then just assume it is a .sympath with extra spaces.
            if (String.IsNullOrEmpty(pathParameter))
                return originalPath;

            // Some characters are specified so assume they form a path.

            if (concatenate)
            {
                // See if an original path was specified.
                if (String.IsNullOrEmpty(originalPath))
                {
                    // Return all but
                    return pathParameter;
                }
                else
                {
                    return String.Format(CultureInfo.InvariantCulture, "{0};{1}", originalPath, pathParameter);
                }
            }
            else
            {
                return pathParameter;
            }
        }

        /// <summary>
        /// Generates a debug script file with the script settings.
        /// </summary>
        /// <param name="scriptFileName">File containing the script settings.</param>
        /// <param name="outputFileName">Name of debug script file.</param>
        /// <param name="overrideSymbolPath">Symbol path extracted from script.</param>
        /// <param name="overrideBinaryPath">Binary path extracted from script.</param>
        /// <param name="overrideSourcePath">Source path extracted from script.</param>
        [SuppressMessage("Microsoft.Design", "CA1045")]
        public void GenerateScriptFile(String scriptFileName, String outputFileName, ref String overrideSymbolPath, ref String overrideBinaryPath, ref String overrideSourcePath)
        {
            // This has been done like this to get over a CA2000 warning about textScriptFile not being disposed even when it is.
            using (StreamWriter textScriptFile = new StreamWriter(scriptFileName))
            {
                generateScriptFile(textScriptFile, outputFileName, ref overrideSymbolPath, ref overrideBinaryPath, ref overrideSourcePath);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1045")]
        private void generateScriptFile(TextWriter textScriptFile, String outputFileName, ref String overrideSymbolPath, ref String overrideBinaryPath, ref String overrideSourcePath)
        {
            // Genetates the script in the specified location.
            textScriptFile.WriteLine(".logopen /u \"{0}\"", outputFileName);
            // Note these dates are stored with current culture so they look good when presented to user. ToString() uses current culture.
            String commentLine = String.Format(CultureInfo.InvariantCulture, ".echo Script2---{0}---LastModified---{1}---RunTime---{2}---Version---{3}", m_Name, m_LastModifiedDate.ToString(CultureInfo.InvariantCulture), DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture), m_Version);
            textScriptFile.WriteLine(commentLine);

            for (int i = 0; i < m_Script.Count; i++)
            {
                const String symbolPathText = ".sympath";
                const String binaryPathText = ".exepath";
                const String srcPathText = ".srcpath";

                // Check if the specified command is present in the commandline.
                if (m_Script[i].Command.IndexOf(symbolPathText, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    overrideSymbolPath = GetUnquotedPathFromCommandLine(overrideSymbolPath, m_Script[i].Command, symbolPathText);

                    commentLine = String.Format(CultureInfo.InvariantCulture, ".echo Command---{0}---{1}", m_Script[i].Command == null ? null : m_Script[i].Command.Replace(';', '&'), m_Script[i].Comment == null ? null : m_Script[i].Comment.Replace(';', '&'));
                    textScriptFile.WriteLine(commentLine);
                    commentLine = String.Format(CultureInfo.InvariantCulture, ".echo CommandEnd---\"{0}\"---\"{1}\"", m_Script[i].Command == null ? null : m_Script[i].Command.Replace(';', '&'), m_Script[i].Comment == null ? null : m_Script[i].Comment.Replace(';', '&'));
                    textScriptFile.WriteLine(commentLine);
                    continue;
                }

                if (m_Script[i].Command.IndexOf(binaryPathText, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    overrideBinaryPath = GetUnquotedPathFromCommandLine(overrideBinaryPath, m_Script[i].Command, binaryPathText);

                    commentLine = String.Format(CultureInfo.InvariantCulture, ".echo Command---{0}---{1}", m_Script[i].Command == null ? null : m_Script[i].Command.Replace(';', '&'), m_Script[i].Comment == null ? null : m_Script[i].Comment.Replace(';', '&'));
                    textScriptFile.WriteLine(commentLine);
                    commentLine = String.Format(CultureInfo.InvariantCulture, ".echo CommandEnd---\"{0}\"---\"{1}\"", m_Script[i].Command == null ? null : m_Script[i].Command.Replace(';', '&'), m_Script[i].Comment == null ? null : m_Script[i].Comment.Replace(';', '&'));
                    textScriptFile.WriteLine(commentLine);
                    continue;
                }

                if (m_Script[i].Command.IndexOf(srcPathText, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    overrideSourcePath = GetUnquotedPathFromCommandLine(overrideSourcePath, m_Script[i].Command, srcPathText);

                    commentLine = String.Format(CultureInfo.InvariantCulture, ".echo Command---{0}---{1}", m_Script[i].Command == null ? null : m_Script[i].Command.Replace(';', '&'), m_Script[i].Comment == null ? null : m_Script[i].Comment.Replace(';', '&'));
                    textScriptFile.WriteLine(commentLine);
                    commentLine = String.Format(CultureInfo.InvariantCulture, ".echo CommandEnd---\"{0}\"---\"{1}\"", m_Script[i].Command == null ? null : m_Script[i].Command.Replace(';', '&'), m_Script[i].Comment == null ? null : m_Script[i].Comment.Replace(';', '&'));
                    textScriptFile.WriteLine(commentLine);
                    continue;
                }

                // Output the comment first with an easily parsable prefix.
                commentLine = String.Format(CultureInfo.InvariantCulture, ".echo Command---{0}---{1}", m_Script[i].Command, m_Script[i].Comment);
                textScriptFile.WriteLine(commentLine);
                String commandLine = String.Format(CultureInfo.InvariantCulture, "{0};", m_Script[i].Command);
                textScriptFile.WriteLine(commandLine);
                commentLine = String.Format(CultureInfo.InvariantCulture, ".echo CommandEnd---{0}---{1}", m_Script[i].Command, m_Script[i].Comment);
                textScriptFile.WriteLine(commentLine);
            }

            textScriptFile.WriteLine(".echo Script Complete:");
            textScriptFile.WriteLine(".logclose");
        }


        private static void initialiseSerializer()
        {
            // Construct an XmlFormatter and use it to serialize the data to the file.
            if (s_XmlSerializer == null)
            {
                s_XmlSerializer = new XmlSerializer(
                    typeof(StackHashScriptSettings),
                    new Type[] { typeof(StackHashScriptSettings), typeof(StackHashScript), typeof(StackHashScriptLine), 
                                 typeof(StackHashScriptOwner)});
            }
        }

        /// <summary>
        /// Fixes up any user commands that need expanding.
        /// 1) Any instance of .load psscor.dll are fixed up with the service install folder path.
        /// This includes the correct version of psscor and the correct architecture version.
        /// </summary>
        /// <param name="machineArchitecture">x86, amd64 or ia64</param>
        /// <param name="clrVersion">Clr version.</param>
        /// <returns>True - fixup occurred. False - no fixup occurred.</returns>
        public bool FixUp(StackHashScriptDumpArchitecture machineArchitecture, String clrVersion, String installFolder)
        {
            if (clrVersion == null)
                throw new ArgumentNullException("clrVersion");

            bool usePssCor2 = clrVersion.StartsWith("2.", StringComparison.OrdinalIgnoreCase) || clrVersion.StartsWith("3.", StringComparison.OrdinalIgnoreCase);

            bool fixUpOccurred = false;

            foreach (StackHashScriptLine line in this.Script)
            {
                if (String.Compare(line.Command, ".load psscor.dll", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    String path = installFolder;
                    if (usePssCor2)
                        path = Path.Combine(path, "psscor2");
                    else
                        path = Path.Combine(path, "psscor4");

                    // Locate the install folder for the service.
                    switch (machineArchitecture)
                    {
                        case StackHashScriptDumpArchitecture.X86:
                            path = Path.Combine(path, "x86");
                            break;
                        case StackHashScriptDumpArchitecture.Amd64:
                            path = Path.Combine(path, "amd64");
                            break;
                        case StackHashScriptDumpArchitecture.IA64:
                            path = Path.Combine(path, "ia64");
                            break;
                    }

                    if (usePssCor2)
                        line.Command = ".load " + path + "\\psscor2.dll";
                    else
                        line.Command = ".load " + path + "\\psscor4.dll";

                    fixUpOccurred = true;
                }
            }

            return fixUpOccurred;
        }

        
        /// <summary>
        /// Loads the settings from the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static StackHashScriptSettings Load(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File does not exist", "fileName");


            // Simply deserializes the specified data from the XML file.
            FileStream xmlFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            StackHashScriptSettings stackHashScriptSettings = null;
            try
            {
                stackHashScriptSettings = s_XmlSerializer.Deserialize(xmlFile) as StackHashScriptSettings;
            }
            finally
            {
                xmlFile.Close();
            }

            return stackHashScriptSettings;
        }

        /// <summary>
        /// Saves the settings to the specified xml file.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            // Clear the read only flag on the file if necessary.
            if (File.Exists(fileName))
            {
                FileAttributes currentAttributes = File.GetAttributes(fileName);
                if ((currentAttributes & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(fileName, currentAttributes & ~FileAttributes.ReadOnly);
            }

            // Simply serializes the specified data to an XML file.
            FileStream xmlFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            try
            {
                s_XmlSerializer.Serialize(xmlFile, this);
            }
            finally
            {
                xmlFile.Close();
            }

            // Set the read only flag on the file.
            if (IsReadOnly)
            {
                FileAttributes currentAttributes = File.GetAttributes(fileName);
                File.SetAttributes(fileName, currentAttributes | FileAttributes.ReadOnly);
            }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptCollection : Collection<StackHashScriptSettings>
    {
        public StackHashScriptCollection() { } // Needed to serialize.

    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashScriptFileData
    {
        private String m_Name;
        private DateTime m_CreationDate;
        private DateTime m_LastModifiedDate;
        private bool m_IsReadOnly;
        private bool m_RunAutomatically;
        private StackHashScriptDumpType m_DumpType;

        public StackHashScriptFileData() { ;} // Required for serialization.

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public DateTime CreationDate
        {
            get { return m_CreationDate; }
            set { m_CreationDate = value; }
        }

        [DataMember]
        public DateTime LastModifiedDate
        {
            get { return m_LastModifiedDate; }
            set { m_LastModifiedDate = value; }
        }

        [DataMember]
        public bool IsReadOnly
        {
            get { return m_IsReadOnly; }
            set { m_IsReadOnly = value; }
        }

        [DataMember]
        public bool RunAutomatically
        {
            get { return m_RunAutomatically; }
            set { m_RunAutomatically = value; }
        }

        [DataMember]
        public StackHashScriptDumpType DumpType
        {
            get { return m_DumpType; }
            set { m_DumpType = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptNamesCollection : Collection<String>
    {
        public StackHashScriptNamesCollection() { } // Needed to serialize.

    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptFileDataCollection : Collection<StackHashScriptFileData>
    {
        public StackHashScriptFileDataCollection() { } // Needed to serialize.

    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptLineOutput : Collection<String>
    {
        public StackHashScriptLineOutput() { } // Needed to serialize.
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashScriptLineResult
    {
        private StackHashScriptLine m_ScriptLine;
        private StackHashScriptLineOutput m_ScriptLineOutput;

        public StackHashScriptLineResult() { } // Needed to serialize.

        public StackHashScriptLineResult(StackHashScriptLine command, StackHashScriptLineOutput result)
        {
            m_ScriptLine = command;
            m_ScriptLineOutput = result;
        }

        [DataMember]
        public StackHashScriptLine ScriptLine
        {
            get { return m_ScriptLine; }
            set { m_ScriptLine = value; }
        }

        [DataMember]
        public StackHashScriptLineOutput ScriptLineOutput
        {
            get { return m_ScriptLineOutput; }
            set { m_ScriptLineOutput = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1720")]
        public bool Search(bool checkForContains, String searchString)
        {
            bool matchFound = false;

            foreach (String outputLine in m_ScriptLineOutput)
            {
                if (outputLine.Contains(searchString))
                {
                    matchFound = true;
                }
            }

            if (checkForContains && matchFound)
                return true;
            else if (!checkForContains && !matchFound)
                return true;
            else
                return false;
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptLineResults : Collection<StackHashScriptLineResult>
    {
        public StackHashScriptLineResults() { } // Needed to serialize.

    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashScriptResult
    {
        private StackHashScriptLineResults m_ScriptResults;
        private String m_Name;
        private DateTime m_LastModifiedDate;
        private DateTime m_RunDate;
        private int m_ScriptVersion;

        public StackHashScriptResult() { ;} // Required for serialization.

        [SuppressMessage("Microsoft.Design", "CA1031")]
        private static DateTime getDateTimeFromText(String dateTimeText, CultureInfo cultureInfo)
        {
            DateTime result;

            try
            {
                // Try the specified culture first - then try anything.
                if (!DateTime.TryParse(dateTimeText, cultureInfo, DateTimeStyles.None, out result))
                {
                    if (!DateTime.TryParse(dateTimeText, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    {
                        if (!DateTime.TryParse(dateTimeText, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
                        {
                            if (!DateTime.TryParse(dateTimeText, out result))
                            {
                                // Try any culture on the local machine.
                                CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                                foreach (CultureInfo culture in allCultures)
                                {
                                    if (DateTime.TryParse(dateTimeText, culture, DateTimeStyles.None, out result))
                                        return result;
                                }

                                result = new DateTime(0);
                            }
                        }
                    }
                }
            }
            catch
            {
                result = DateTime.Parse(dateTimeText, CultureInfo.CurrentCulture);
            }

            return result;
        }

        public StackHashScriptResult(string fileName)
        {
            // Load the script results from the specified file.
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File not found", "fileName");

            // String[] allLines = File.ReadAllLines(fileName, Encoding.Unicode);
            String[] allLines = FileUtils.ReadAllLinesWithTypeCheck(fileName);

            StackHashScriptLineResults allResults = new StackHashScriptLineResults();
            StackHashScriptLineResult thisCommand = null;

            bool isDebuggerVersion = false;
            bool awaitingCommandEnd = false;
            bool standardLogOutput = false;

            for (int i = 0; i < allLines.Length; i++)
            {
                String[] separators = new String[] { "---" };
                if (allLines[i].StartsWith("Script---", StringComparison.OrdinalIgnoreCase) ||
                    allLines[i].StartsWith("Script2---", StringComparison.OrdinalIgnoreCase))
                {
                    int scriptVersion = 1;

                    if (allLines[i].StartsWith("Script2---", StringComparison.OrdinalIgnoreCase))
                        scriptVersion = 2;

                    standardLogOutput = true;

                    // Get the name and LastModified date of the script.
                    String[] components = allLines[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    m_Name = components[1];

                    // Note these dates are stored with current culture so they look good when presented to user.
                    if (scriptVersion == 1)
                    {
                        m_LastModifiedDate = getDateTimeFromText(components[3], CultureInfo.CurrentCulture);
                        m_RunDate = getDateTimeFromText(components[5], CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        m_LastModifiedDate = getDateTimeFromText(components[3], CultureInfo.InvariantCulture);
                        m_RunDate = getDateTimeFromText(components[5], CultureInfo.InvariantCulture);
                    }
                    m_LastModifiedDate = new DateTime(m_LastModifiedDate.Ticks, DateTimeKind.Utc);
                    m_RunDate = new DateTime(m_RunDate.Ticks, DateTimeKind.Utc);

                    if (components.Length >= 8)
                        m_ScriptVersion = int.Parse(components[7], CultureInfo.InvariantCulture);
                }
                else if (allLines[i].StartsWith("Command---", StringComparison.OrdinalIgnoreCase))
                {
                    // Get the command and comment.
                    String[] components = allLines[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    thisCommand = new StackHashScriptLineResult();


                    if (components.Length == 1)
                        thisCommand.ScriptLine = new StackHashScriptLine();
                    else if (components.Length == 2)
                        thisCommand.ScriptLine = new StackHashScriptLine(components[1], null);
                    else if (components.Length == 3)
                        thisCommand.ScriptLine = new StackHashScriptLine(components[1], components[2]);
                    thisCommand.ScriptLineOutput = new StackHashScriptLineOutput();

                    awaitingCommandEnd = true;

                    // It is possible for the command to be an empty line.
                    if ((thisCommand.ScriptLine != null) && (thisCommand.ScriptLine.Command != null))
                        isDebuggerVersion = (thisCommand.ScriptLine.Command == "version");
                    else
                        isDebuggerVersion = false;
                }
                else if (allLines[i].StartsWith("CommandEnd---", StringComparison.OrdinalIgnoreCase))
                {
                    awaitingCommandEnd = false;
                    if (thisCommand != null)
                    {
                        allResults.Add(thisCommand);
                        thisCommand = null;
                        isDebuggerVersion = false;
                    }
                }
                else
                {
                    if (thisCommand != null)
                    {
                        if (isDebuggerVersion)
                        {
                            // Only store the first line of the debugger version.
                            if (allLines[i].Contains("Microsoft (R) Windows Debugger Version"))
                            {
                                thisCommand.ScriptLineOutput.Add(allLines[i]);
                            }
                        }
                        else
                        {
                            thisCommand.ScriptLineOutput.Add(allLines[i]);
                        }
                    }
                }
            }

            // If an error occurred in the script then it may be incomplete. 
            // In this case return the error back to the user.
            if (awaitingCommandEnd)
            {
                if (thisCommand != null)
                {
                    allResults.Add(thisCommand);
                }
            }

            // The !Analysis command produces a load of log files in the script output folder. Just load these as standard text files.
            if (!standardLogOutput)
            {
                thisCommand = new StackHashScriptLineResult();
                thisCommand.ScriptLine = new StackHashScriptLine("Script command", "Special script");
                thisCommand.ScriptLineOutput = new StackHashScriptLineOutput();

                m_Name = Path.GetFileNameWithoutExtension(fileName);

                m_LastModifiedDate = File.GetLastWriteTimeUtc(fileName);
                m_RunDate = File.GetLastWriteTimeUtc(fileName);

                foreach (String line in allLines)
                {
                    thisCommand.ScriptLineOutput.Add(line);
                }
                allResults.Add(thisCommand);
            }
        
            m_ScriptResults = allResults;
        }


        private static void addAnalysisFiles(String folder, List<String> lines)
        {
            // All of the Analysis files have a CLRDUMP_ prefix.

            String[] files = Directory.GetFiles(folder, "CLRDUMP_*");

            foreach (String file in files)
            {
                String [] allLines = FileUtils.ReadAllLinesWithTypeCheck(file);
                foreach (String line in allLines)
                {
                    lines.Add(line);
                }

                File.Delete(file);
            }
        }

        public static StackHashScriptResult MergeAnalysisDumpFiles(string fileName)
        {
            // Load the script results from the specified file.
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File not found", "fileName");

            // String[] allLines = File.ReadAllLines(fileName, Encoding.Unicode);
            String[] allLines = FileUtils.ReadAllLinesWithTypeCheck(fileName);

            StackHashScriptLineResult thisCommand = null;

            String[] separators = new String[] { "---" };
            List<String> outLines = new List<String>();
            bool foundAnalysisCommand = false;
            bool foundCommandStart = false;

            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i].StartsWith("Command---", StringComparison.OrdinalIgnoreCase))
                {
                    // Get the command and comment.
                    String[] components = allLines[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    thisCommand = new StackHashScriptLineResult();


                    if (components.Length == 1)
                        thisCommand.ScriptLine = new StackHashScriptLine();
                    else if (components.Length == 2)
                        thisCommand.ScriptLine = new StackHashScriptLine(components[1], null);
                    else if (components.Length == 3)
                        thisCommand.ScriptLine = new StackHashScriptLine(components[1], components[2]);
                    thisCommand.ScriptLineOutput = new StackHashScriptLineOutput();

                    // Check for the !analysis command.
                    if ((thisCommand.ScriptLine != null) && (thisCommand.ScriptLine.Command != null) && (thisCommand.ScriptLine.Command.IndexOf("!Analysis", StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        foundCommandStart = true;
                    }
                }
                else if (allLines[i].StartsWith("CommandEnd---", StringComparison.OrdinalIgnoreCase))
                {                    
                    if (foundCommandStart)
                    {
                        // If the analysis command end has been found then get the results.
                        foundAnalysisCommand = true;
                        addAnalysisFiles(Path.GetDirectoryName(fileName), outLines);
                        foundCommandStart = false;
                    }
                }
                outLines.Add(allLines[i]);
            }


            if (foundCommandStart)
            {
                foundAnalysisCommand = true;
                // The command end seems to be missing for this !analysis.
                addAnalysisFiles(Path.GetDirectoryName(fileName), outLines);
            }

            if (foundAnalysisCommand)
                File.WriteAllLines(fileName, outLines.ToArray());

            return new StackHashScriptResult(fileName);
        }

        
        [DataMember]
        public StackHashScriptLineResults ScriptResults
        {
            get { return m_ScriptResults; }
            set { m_ScriptResults = value; }
        }

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public DateTime LastModifiedDate
        {
            get { return m_LastModifiedDate; }
            set { m_LastModifiedDate = value; }
        }
        [DataMember]
        public DateTime RunDate
        {
            get { return m_RunDate; }
            set { m_RunDate = value; }
        }
        [DataMember]
        public int ScriptVersion
        {
            get { return m_ScriptVersion; }
            set { m_ScriptVersion = value; }
        }


        public override String ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(String.Format(CultureInfo.InvariantCulture, "Script: {0} Version: {1} RunDate: {2} ", m_Name, m_ScriptVersion, m_RunDate));
            result.AppendLine("");

            foreach (StackHashScriptLineResult resultLine in ScriptResults)
            {
                result.Append(resultLine.ScriptLine.Command);
                result.Append("---");
                result.AppendLine(resultLine.ScriptLine.Comment);

                foreach (String outLine in resultLine.ScriptLineOutput)
                {
                    result.Append("  ");
                    result.AppendLine(outLine);
                }
            }

            return result.ToString();
        }

        public bool Search(StackHashSearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            if (!searchCriteria.ContainsObject(StackHashObjectType.Script))
                return true;

            if (m_ScriptResults == null)
                return false;

            foreach (StackHashSearchOption searchOption in searchCriteria.SearchFieldOptions)
            {
                if (searchOption.ObjectType != StackHashObjectType.Script)
                    continue;

                StringSearchOption stringSearchOption = searchOption as StringSearchOption;

                if (stringSearchOption == null)
                    continue;

                // Only search the result lines.
                bool matchFound = false;

                foreach (StackHashScriptLineResult cmdOutput in m_ScriptResults)
                {
                    if (cmdOutput.Search(true, stringSearchOption.Start))
                        matchFound = true;
                }

                bool optionMatched = false;
                if (stringSearchOption.SearchOptionType == StackHashSearchOptionType.StringContains && matchFound)
                    optionMatched = true;
                else if (stringSearchOption.SearchOptionType == StackHashSearchOptionType.StringDoesNotContain && !matchFound)
                    optionMatched = true;

                if (!optionMatched)
                    return false;
            }

            return true;
        }
    }


    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptResults : Collection<StackHashScriptResult>
    {
        public StackHashScriptResults() { } // Needed to serialize.

    }



    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashScriptResultFile
    {
        private String m_ScriptName;
        private DateTime m_RunDate;
        private String m_UserName; // Name of person who ran the script.

        public StackHashScriptResultFile() { } // Needed to serialize.

        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
        }

        [DataMember]
        public DateTime RunDate
        {
            get { return m_RunDate; }
            set { m_RunDate = value; }
        }

        [DataMember]
        public String UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashScriptResultFiles : Collection<StackHashScriptResultFile>
    {
        public StackHashScriptResultFiles() { } // Needed to serialize.

    }
}
