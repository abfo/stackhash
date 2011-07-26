using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Globalization;

using StackHashBugTrackerInterfaceV1;
using StackHashUtilities;
using StackHashBusinessObjects;

namespace StackHashTasks
{
    /// <summary>
    /// A plug-in is loaded by the BugTrackerPlugIn class. Once loaded the 2 interfaces are 
    /// located in the plug-in: a control interface and a context interface. 
    /// Only 1 control interface is created and this is managed by the BugTrackerPlugIn class.
    /// This object (BugTrackerPlugInContext) - manages instances of the context object.
    /// A context object is loaded for each StackHash profile that uses this plug-in.
    /// e.g. 2 profiles may use the same plug-in but with different settings - so 2 plug-in
    /// context objects are required.
    /// </summary>
    public class BugTrackerPlugInContext : IBugTrackerV1Context, IDisposable
    {
        private BugTrackerPlugIn m_PlugIn; // DLL manager.
        private IBugTrackerV1Context m_PlugInContext; // Instance of the object within the plug-in DLL.
        private Exception m_LastException;
        private static int s_MaxBugRefSize = 200; // Max size of bug ref that the user can add to the database.

        public Exception LastException
        {
            get
            {
                return m_LastException;
            }
        }

        public String PlugInName
        {
            get
            {
                if (m_PlugIn != null)
                    return m_PlugIn.PlugInName;
                else
                    return null;

            }
        }

        /// <summary>
        /// Creates a proxy for the object in the plug-in that implements the Context
        /// interface.
        /// This object uses the BugTrackerPlugInContext to get an instance of the internal
        /// object within the plug-in.
        /// </summary>
        /// <param name="plugIn">The object that manages the DLL.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2001")]
        public BugTrackerPlugInContext(BugTrackerPlugIn plugIn)
        {
            if (plugIn == null)
                throw new ArgumentNullException("plugIn");

            if (!plugIn.Loaded)
                throw new ArgumentException("Plug In not loaded: " + plugIn.FileName, "plugIn");

            m_PlugIn = plugIn;

            m_PlugInContext = m_PlugIn.CreateContext();
        }


        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public NameValueCollection Properties
        {
            get
            {
                try
                {
                    return m_PlugInContext.Properties;
                }
                catch (System.Exception ex)
                {
                    m_LastException = ex;
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "GetPlugInProperties failed for plug-in " + PlugInName, ex);
                    throw;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public NameValueCollection ContextDiagnostics
        {
            get
            {
                try
                {
                    return m_PlugInContext.ContextDiagnostics;
                }
                catch (System.Exception ex)
                {
                    m_LastException = ex;
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "GetContextDiagnostics failed for plug-in " + PlugInName, ex);
                    throw;
                }
            }
        }

        
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void SetSelectPropertyValues(NameValueCollection propertiesToSet)
        {
            try
            {
                m_PlugInContext.SetSelectPropertyValues(propertiesToSet);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "SetPlugInProperties failed for plug-in " + PlugInName, ex);
                throw;
            }
        }


        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void ProductUpdated(BugTrackerReportType reportType, BugTrackerProduct product)
        {
            try
            {
                m_PlugInContext.ProductUpdated(reportType, product);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "ProductUpdated failed for plug-in " + PlugInName, ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void ProductAdded(BugTrackerReportType reportType, BugTrackerProduct product)
        {
            try
            {
                m_PlugInContext.ProductAdded(reportType, product);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "ProductAdded failed for plug-in " + PlugInName, ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void FileUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            try
            {
                m_PlugInContext.FileUpdated(reportType, product, file);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "FileUpdated failed for plug-in " + PlugInName, ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public void FileAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            try
            {
                m_PlugInContext.FileAdded(reportType, product, file);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "FileAdded failed for plug-in " + PlugInName, ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string EventUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                String plugInBugRef = m_PlugInContext.EventUpdated(reportType, product, file, theEvent);

                if ((plugInBugRef != null) && (plugInBugRef.Length > s_MaxBugRefSize))
                    throw new StackHashException("Plug-in specified bug reference that is > " + s_MaxBugRefSize.ToString(CultureInfo.InvariantCulture));

                return plugInBugRef;
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "EventUpdated failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string EventAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                String plugInBugRef = m_PlugInContext.EventAdded(reportType, product, file, theEvent);

                if ((plugInBugRef != null) && (plugInBugRef.Length > s_MaxBugRefSize))
                    throw new StackHashException("Plug-in specified bug reference that is > " + s_MaxBugRefSize.ToString(CultureInfo.InvariantCulture), StackHashServiceErrorCode.BugTrackerPlugInReferenceTooLarge);

                return plugInBugRef;
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "EventAdded failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string EventManualUpdateCompleted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                return m_PlugInContext.EventManualUpdateCompleted(reportType, product, file, theEvent);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "EventManualUpdateCompleted failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        
        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string EventNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerNote note)
        {
            try
            {
                return m_PlugInContext.EventNoteAdded(reportType, product, file, theEvent, note);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "EventNoteAdded failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string CabUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            try
            {
                return m_PlugInContext.CabUpdated(reportType, product, file, theEvent, cab);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "CabUpdated failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string CabAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            try
            {
                return m_PlugInContext.CabAdded(reportType, product, file, theEvent, cab);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "CabAdded failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string CabNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerNote note)
        {
            try
            {
                return m_PlugInContext.CabNoteAdded(reportType, product, file, theEvent, cab, note);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "CabNoteAdded failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        [SuppressMessage("Microsoft.Naming", "CA2204")]
        public string DebugScriptExecuted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerScriptResult scriptResult)
        {
            try
            {
                return m_PlugInContext.DebugScriptExecuted(reportType, product, file, theEvent, cab, scriptResult);
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "DebugScriptExecuted failed for plug-in " + PlugInName, ex);
                return null;
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_PlugIn.ReleaseContext(m_PlugInContext);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }


    /// <summary>
    /// A plug-in is loaded by this class (BugTrackerPlugIn). Once loaded the 2 interfaces are 
    /// located in the plug-in: a control interface and a context interface. 
    /// Only 1 control interface is created and is managed by this class.
    public class BugTrackerPlugIn : IBugTrackerV1Control, IDisposable
    {
        private Assembly m_LoadedAssembly;
        private IBugTrackerV1Control m_PlugInControlObject;
        private String m_PlugInName;
        private Exception m_LastException;
        private bool m_Loaded;
        private String m_FileName;
        private Type m_PlugInContextInterfaceObject;

        public String PlugInName
        {
            get 
            {
                if (m_Loaded)
                {
                    if (m_PlugInName == null)
                    {
                        m_PlugInName = m_PlugInControlObject.PlugInName;
                    }
                    return m_PlugInName;
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        public String PlugInDescription
        {
            get
            {
                if (m_Loaded)
                {
                    return m_PlugInControlObject.PlugInDescription;
                }
                else
                {
                    return null;
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public bool PlugInSetsBugReference
        {
            get
            {
                if (m_Loaded)
                {
                    return m_PlugInControlObject.PlugInSetsBugReference;
                }
                else
                {
                    return false;
                }
            }
        }

        
        public Uri HelpUrl
        {
            get
            {
                if (m_Loaded)
                {
                    try
                    {
                        return m_PlugInControlObject.HelpUrl;
                    }
                    catch (UriFormatException ex)
                    {
                        DiagnosticsHelper.LogException(DiagSeverity.Warning, "Invalid plug-in URI", ex);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        
        public Exception LastException
        {
            get
            {
                return m_LastException;
            }
        }

        public bool Loaded
        {
            get
            {
                return m_Loaded;
            }
        }
        public String FileName
        {
            get
            {
                return m_FileName;
            }
        }
        
        /// <summary>
        /// The plugin represents the interface to the plugin module. 
        /// Any security and protection of must be done in here.
        /// </summary>
        /// <param name="fileName">The name of the DLL file.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2001")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public BugTrackerPlugIn(String fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File does not exist" + fileName, "fileName");

            if (fileName.Contains("StackHashTasks.dll"))
                throw new ArgumentException("Cannot load product DLL", "fileName");

            try
            {
                m_FileName = fileName;

                // Load the candidate DLL.
                m_LoadedAssembly = Assembly.LoadFrom(fileName);


                if (m_LoadedAssembly == null)
                    throw new StackHashException("Unable to load assembly " + m_FileName, StackHashServiceErrorCode.BugTrackerPlugInLoadError);

                Type controlClassType = null;
                Type contextClassType = null;

                Type [] allTypes = null;

                try
                {
                    allTypes = m_LoadedAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder fullExceptionText = new StringBuilder(ex.Message);

                    for (int i = 0; i < ex.LoaderExceptions.Length; i++)
                    {
                        fullExceptionText.AppendLine(ex.Types[i].ToString());
                        fullExceptionText.AppendLine(ex.LoaderExceptions[i].ToString());
                    }

                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, fullExceptionText.ToString(), ex);
                    throw new StackHashException(fullExceptionText.ToString(), ex, StackHashServiceErrorCode.BugTrackerPlugInLoadError);
                }

                // Walk through each type in the assembly looking for a class that supports the correct interface.
                foreach (Type type in allTypes)
                {
                    if (type.IsClass == true)
                    {
                        if (type.GetInterface("IBugTrackerV1Control") == typeof(IBugTrackerV1Control))
                        {
                            controlClassType = type;

                            if (contextClassType != null)
                                break;
                        }

                        if (type.GetInterface("IBugTrackerV1Context") == typeof(IBugTrackerV1Context))
                        {
                            contextClassType = type;
                            if (controlClassType != null)
                                break;
                        }
                    }
                }

                if ((controlClassType != null) && (contextClassType != null))
                {
                    m_PlugInControlObject = m_LoadedAssembly.CreateInstance(controlClassType.FullName) as IBugTrackerV1Control;
                    m_PlugInContextInterfaceObject = contextClassType;
                }
                else
                {
                    throw new StackHashException("Failed to load plug-in " + fileName, StackHashServiceErrorCode.DllNotABugTrackerPlugIn);
                }

                // Set this before getting PlugInName as that property won't call through to the plugin if it isn't marked as loaded.
                m_Loaded = true;

                // Get the name to ensure we can call through to the DLL.
                m_PlugInName = PlugInName;
            }
            catch (System.Exception ex)
            {
                m_LastException = ex;
                m_Loaded = false;
            }
        }


        /// <summary>
        /// Creates an interface object to deal with requests in a particular context.
        /// </summary>
        /// <returns>A new context.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public IBugTrackerV1Context CreateContext()
        {
            try
            {
                return m_PlugInControlObject.CreateContext();
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Create Context failed for plug-in " + m_PlugInName, ex);
                throw;
            }
        }

        /// <summary>
        /// Calls the plug-in to release a context object.
        /// </summary>
        /// <param name="context"></param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void ReleaseContext(IBugTrackerV1Context context)
        {
            try
            {
                m_PlugInControlObject.ReleaseContext(context);
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Release Context failed for plug-in " + m_PlugInName, ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        public NameValueCollection PlugInDiagnostics
        {
            get
            {
                try
                {
                    return m_PlugInControlObject.PlugInDiagnostics;
                }
                catch (System.Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Get Plug-In Diagnostics failed for plugin " + m_PlugInName, ex);
                    return null;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        public NameValueCollection PlugInDefaultProperties
        {
            get
            {
                try
                {
                    return m_PlugInControlObject.PlugInDefaultProperties;
                }
                catch (System.Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Get Plug-In Default Properties failed for plugin " + m_PlugInName, ex);
                    return null;
                }
            }
        }

        
        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void  Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        #endregion
    }


    /// <summary>
    /// Manages the loading of plug-ins and the BugTrackerContexts.
    /// A BugTrackerContext is created for each StackHash profile (context) and 
    /// contains references to one or more plug-in.
    /// </summary>
    public class BugTrackerManager
    {
        private static BugTrackerManager s_CurrentBugTrackerManager;
        private String [] m_PlugInFolders;
        private Collection<BugTrackerPlugIn> m_PlugIns = new Collection<BugTrackerPlugIn>();

        public static BugTrackerManager CurrentBugTrackerManager
        {
            get { return s_CurrentBugTrackerManager; }
        }

        /// <summary>
        /// The number of LOADED plug-ins.
        /// </summary>
        public int NumberOfPlugIns
        {
            get 
            {
                Monitor.Enter(this);

                try
                {
                    // Just return the plug-ins that have been loaded.
                    int count = 0;
                    foreach (BugTrackerPlugIn plugIn in m_PlugIns)
                    {
                        if (plugIn.Loaded)
                        {
                            count++;
                        }
                    }

                    return count;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }


        /// <summary>
        /// Construct the bug tracker manager. 
        /// This will load all DLLs from the specified folders and check that they contain 
        /// the correct plug-in interfaces. It will then load the control interface for each.
        /// All DLLs will have an entry whether they are loaded or not. This allows the client
        /// to display diagnostics as to the reason for the load fail.
        /// </summary>
        /// <param name="plugInFolders"></param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public BugTrackerManager(String[] plugInFolders)
        {
            if (plugInFolders == null)
                throw new ArgumentNullException("plugInFolders");

            s_CurrentBugTrackerManager = this;  // Effectively a global flag.

            m_PlugInFolders = plugInFolders;

            // Enumerate the DLLs in the plugin folder.
            foreach (String plugInFolder in m_PlugInFolders)
            {
                // Get all the DLLs in the selected folder.
                try
                {
                    // Ignore folders that do not exist.
                    if (!Directory.Exists(plugInFolder))
                        continue;

                    String [] dllFullNames = Directory.GetFiles(plugInFolder, "*.dll");

                    foreach (String dllFullName in dllFullNames)
                    {
                        try
                        {
                            String dllName = Path.GetFileName(dllFullName).ToUpperInvariant();

                            if (dllName.Contains("PLUGIN"))
                            {
                                BugTrackerPlugIn plugin = new BugTrackerPlugIn(dllFullName);
                                m_PlugIns.Add(plugin);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Failed to load plug-in " + dllFullName, ex);
                        }
                    }
                }

                catch (System.Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.ComponentFatal, "Failed to load plug-ins from folder " + plugInFolder, ex);                    
                }
            }
        }


        /// <summary>
        /// Gets the full status of the plugins in the plugin folder(s) or a specific plugin.
        /// </summary>
        /// <param name="plugInName">Name of plug-in or null for all plug-ins</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashBugTrackerPlugInDiagnosticsCollection GetBugTrackerPlugInDiagnostics(String plugInName)
        {
            Monitor.Enter(this);

            try
            {
                StackHashBugTrackerPlugInDiagnosticsCollection allPlugInDiagnostics = new StackHashBugTrackerPlugInDiagnosticsCollection();

                foreach (BugTrackerPlugIn plugIn in m_PlugIns)
                {
                    if (!String.IsNullOrEmpty(plugInName))
                        if (plugInName != plugIn.PlugInName)
                            continue;

                    StackHashBugTrackerPlugInDiagnostics diagnostics = new StackHashBugTrackerPlugInDiagnostics();
                    diagnostics.Loaded = plugIn.Loaded;
                    diagnostics.FileName = plugIn.FileName;
                    diagnostics.PlugInDescription = plugIn.PlugInDescription;
                    diagnostics.HelpUrl = plugIn.HelpUrl;
                    diagnostics.PlugInSetsBugReference = plugIn.PlugInSetsBugReference;

                    if (plugIn.LastException == null)
                        diagnostics.LastException = "No Error";
                    else
                        diagnostics.LastException = plugIn.LastException.ToString();
                    diagnostics.Name = plugIn.PlugInName;

                    if (diagnostics.Loaded)
                    {
                        diagnostics.Diagnostics = new StackHashNameValueCollection(plugIn.PlugInDiagnostics);
                        diagnostics.DefaultProperties = new StackHashNameValueCollection(plugIn.PlugInDefaultProperties);
                    }

                    allPlugInDiagnostics.Add(diagnostics);
                }

                return allPlugInDiagnostics;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Request the diagnostics for the specified named plug-in.
        /// </summary>
        /// <param name="plugInName">Name of the plug-in for whicg diagnostics is required.</param>
        /// <returns>Name value pairs describing the current plug-in state.</returns>
        public NameValueCollection GetPlugInDiagnostics(String plugInName)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugIn plugIn in m_PlugIns)
                {
                    if ((plugIn.Loaded) && (plugIn.PlugInName == plugInName))
                    {
                        return plugIn.PlugInDiagnostics;
                    }
                }
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Gets a list of the default plug-in properties for the named plug-in.
        /// </summary>
        /// <param name="plugInName">Plug-in whose default properties are required.</param>
        /// <returns>Default properties or null</returns>
        public NameValueCollection GetPlugInDefaultProperties(String plugInName)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugIn plugIn in m_PlugIns)
                {
                    if ((plugIn.Loaded) && (plugIn.PlugInName == plugInName))
                    {
                        return plugIn.PlugInDefaultProperties;
                    }
                }
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Creates a new context in the specified plug-in.
        /// </summary>
        /// <param name="plugInName">Plug-in in which a context is to be created.</param>
        /// <returns>Newly created context or null.</returns>
        public BugTrackerPlugInContext CreatePlugInContext(String plugInName)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugIn plugIn in m_PlugIns)
                {
                    if (plugIn.PlugInName == plugInName)
                    {
                        if (plugIn.Loaded == true)
                            return new BugTrackerPlugInContext(plugIn);
                        else
                            throw new StackHashException("Unable to load plug-in: " + plugInName, plugIn.LastException, StackHashServiceErrorCode.BugTrackerPlugInLoadError);
                    }
                }
                throw new StackHashException("Unable to load plug-in: " + plugInName + ". Check the name including case.", StackHashServiceErrorCode.BugTrackerPlugInNotFoundOrCouldNotBeLoaded);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }



        /// <summary>
        /// Checks if the plug-in settings have changed since they were saved against a context.
        /// If the plug-in has been updated then this may be the case.
        /// </summary>
        /// <param name="plugInSettings">Plug-in settings to check. May be null.</param>
        /// <returns>True - settings changed, False - settings not changed.</returns>
        public bool UpdatePlugInSettings(StackHashBugTrackerPlugInSettings plugInSettings)
        {
            if (plugInSettings == null)
                return false;

            Monitor.Enter(this);

            bool changed = false;

            try
            {
                foreach (StackHashBugTrackerPlugIn plugInSetting in plugInSettings.PlugInSettings)
                {
                    foreach (BugTrackerPlugIn plugIn in m_PlugIns)
                    {
                        if (plugInSetting.Name == plugIn.PlugInName)
                        {
                            if (plugIn.Loaded == true)
                            {
                                if (plugInSetting.HelpUrl == null)
                                {
                                    if (plugIn.HelpUrl != null)
                                    {
                                        plugInSetting.HelpUrl = plugIn.HelpUrl;
                                        changed = true;
                                    }
                                }
                                else // plugInSetting.HelpUrl != null
                                {
                                    if (plugIn.HelpUrl != null)
                                    {
                                        if (plugInSetting.HelpUrl != plugIn.HelpUrl)
                                        {
                                            plugInSetting.HelpUrl = plugIn.HelpUrl;
                                            changed = true;
                                        }
                                    }
                                    else
                                    {
                                        plugInSetting.HelpUrl = null;
                                        changed = true;
                                    }
                                }

                                if (plugInSetting.PlugInDescription == null)
                                {
                                    if (plugIn.PlugInDescription != null)
                                    {
                                        plugInSetting.PlugInDescription = plugIn.PlugInDescription;
                                        changed = true;
                                    }
                                }
                                else // plugInSetting.PlugInDescription != null
                                {
                                    if (plugIn.PlugInDescription != null)
                                    {
                                        if (plugInSetting.PlugInDescription != plugIn.PlugInDescription)
                                        {
                                            plugInSetting.PlugInDescription = plugIn.PlugInDescription;
                                            changed = true;
                                        }
                                    }
                                    else
                                    {
                                        plugInSetting.PlugInDescription = null;
                                        changed = true;
                                    }
                                }

                                if (plugInSetting.ChangesBugReference != plugIn.PlugInSetsBugReference)
                                {
                                    plugInSetting.ChangesBugReference = plugIn.PlugInSetsBugReference;
                                    changed = true;
                                }
                            }
                        }
                    }
                }

                return changed;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }


    /// <summary>
    /// Manages the particular plugin context objects associated with a StackHash profile.
    /// This object acts as a 1 to many router for the StackHash tasks. A ProductAdded report 
    /// for example will be sent here first and then this object will send it on to all known
    /// loaded plug-ins for this profile.
    /// </summary>
    public class BugTrackerContext : IDisposable
    {
        BugTrackerManager m_BugTrackerManager;
        StackHashBugTrackerPlugInSettings m_Settings;  // Profile settings with respect to BugTracker plug-ins.
        List<BugTrackerPlugInContext> m_PlugIns; // List of proxies for the plug-in Context objects.


        public int NumberOfLoadedPlugIns
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    int numberEnabledPlugIns = 0;
                    foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                    {
                        numberEnabledPlugIns++;
                    }

                    return numberEnabledPlugIns;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        public int NumberOfFailedPlugIns
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    int numberOfFailedPlugIns = 0;
                    foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                    {
                        if (plugIn.LastException != null)
                            numberOfFailedPlugIns++;
                    }

                    return numberOfFailedPlugIns;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }


        /// <summary>
        /// From the settings, attempts to create a Context proxy object for each of the listed plug-in 
        /// names.
        /// </summary>
        /// <param name="bugTrackerManager">The bug tracker manager - manages the known plug-ins.</param>
        /// <param name="settings">Profile settings.</param>
        public BugTrackerContext(BugTrackerManager bugTrackerManager, StackHashBugTrackerPlugInSettings settings)
        {
            if (bugTrackerManager == null)
                throw new ArgumentNullException("bugTrackerManager");
            if (settings == null)
                throw new ArgumentNullException("settings");

            m_BugTrackerManager = bugTrackerManager;
            m_Settings = settings;
            m_PlugIns = new List<BugTrackerPlugInContext>();

            int numberOfPlugInsThatSetBugReference = 0;

            foreach (StackHashBugTrackerPlugIn plugIn in settings.PlugInSettings)
            {
                if (plugIn.Enabled)
                {
                    StackHashBugTrackerPlugInDiagnosticsCollection diagnostics = m_BugTrackerManager.GetBugTrackerPlugInDiagnostics(plugIn.Name);

                    if ((diagnostics != null) && (diagnostics.Count == 1))
                    {
                        if (diagnostics[0].PlugInSetsBugReference)
                            numberOfPlugInsThatSetBugReference++;
                    }            
                }
            }

            if (numberOfPlugInsThatSetBugReference > 1)
                throw new StackHashException("Cannot load more that one plug-in that sets the bug reference", StackHashServiceErrorCode.BugTrackerPlugInReferenceChangeError);

            
            foreach (StackHashBugTrackerPlugIn plugIn in settings.PlugInSettings)
            {
                if (plugIn.Enabled)
                {
                    BugTrackerPlugInContext plugInObject = bugTrackerManager.CreatePlugInContext(plugIn.Name);

                    if (plugIn.Properties != null)
                        plugInObject.SetSelectPropertyValues(plugIn.Properties.ToNameValueCollection());
                    m_PlugIns.Add(plugInObject);
                }
            }
        }

        /// <summary>
        /// Gets the diagnostics for the specified plugin context.
        /// </summary>
        /// <param name="plugInName">Plug-in whose context diagnostics is required.</param>
        /// <returns>Diagnostics for the context.</returns>
        public StackHashBugTrackerPlugInDiagnosticsCollection GetContextDiagnostics(String plugInName)
        {
            return GetContextDiagnostics(plugInName, false);
        }

        /// <summary>
        /// Gets the diagnostics for the specified plugin context.
        /// </summary>
        /// <param name="plugInName">Plug-in whose context diagnostics is required.</param>
        /// <param name="returnPlugInsInError">True - only return plug-ins with an error.</param>
        /// <returns>Diagnostics for the context.</returns>
        public StackHashBugTrackerPlugInDiagnosticsCollection GetContextDiagnostics(String plugInName, bool returnPlugInsInError)
        {
            Monitor.Enter(this);

            try
            {
                StackHashBugTrackerPlugInDiagnosticsCollection diagnostics = new StackHashBugTrackerPlugInDiagnosticsCollection();

                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInName == null) || 
                        ((plugIn.PlugInName != null) && (plugIn.PlugInName == plugInName)))
                    {
                        StackHashBugTrackerPlugInDiagnosticsCollection plugInDiagnostics = m_BugTrackerManager.GetBugTrackerPlugInDiagnostics(plugIn.PlugInName);

                        if (plugInDiagnostics.Count == 1)
                        {
                            if (plugInDiagnostics[0].Loaded)
                            {
                                if ((plugInDiagnostics[0].LastException == null) && returnPlugInsInError)
                                    continue;

                                NameValueCollection contextDiagnostics = plugIn.ContextDiagnostics; // Replace with the context specific diagnostics.
                                plugInDiagnostics[0].Diagnostics = new StackHashNameValueCollection(contextDiagnostics);

                                // Keep the DLL error if there is one. If there is a context error then use that.
                                if (plugIn.LastException != null)
                                    plugInDiagnostics[0].LastException = plugIn.LastException.ToString();

                                diagnostics.Add(plugInDiagnostics[0]);
                            }
                        }
                    }
                }
                return diagnostics;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public NameValueCollection GetProperties(String plugInName)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugIn.PlugInName != null) && (plugIn.PlugInName == plugInName))
                    {
                        return plugIn.Properties;
                    }
                }
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetProperties(String plugInName, NameValueCollection properties)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugIn.PlugInName != null) && (plugIn.PlugInName == plugInName))
                    {
                        plugIn.SetSelectPropertyValues(properties);
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void ProductUpdated(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                        plugIn.ProductUpdated(reportType, product);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void ProductAdded(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                        plugIn.ProductAdded(reportType, product);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void FileUpdated(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                        plugIn.FileUpdated(reportType, product, file);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void FileAdded(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                        plugIn.FileAdded(reportType, product, file);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string EventUpdated(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.EventUpdated(reportType, product, file, theEvent);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string EventAdded(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.EventAdded(reportType, product, file, theEvent);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string EventManualUpdateCompleted(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                        plugIn.EventManualUpdateCompleted(reportType, product, file, theEvent);
                }
                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string EventNoteAdded(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerNote note)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.EventNoteAdded(reportType, product, file, theEvent, note);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }                    
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public System.Exception GetLastError(StackHashBugTrackerPlugInSelectionCollection plugInSelection)
        {
            Monitor.Enter(this);

            try
            {
                System.Exception lastException;

                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        lastException = plugIn.LastException;

                        if (lastException != null)
                            return lastException;
                    }
                }

                return null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string CabUpdated(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.CabUpdated(reportType, product, file, theEvent, cab);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string CabAdded(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.CabAdded(reportType, product, file, theEvent, cab);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string CabNoteAdded(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerNote note)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.CabNoteAdded(reportType, product, file, theEvent, cab, note);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public string DebugScriptExecuted(StackHashBugTrackerPlugInSelectionCollection plugInSelection, BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerScriptResult scriptResult)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            Monitor.Enter(this);

            try
            {
                string bugRef = theEvent.PlugInBugReference;
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    if ((plugInSelection == null) || (plugInSelection.ContainsName(plugIn.PlugInName)))
                    {
                        String newBugRef = plugIn.DebugScriptExecuted(reportType, product, file, theEvent, cab, scriptResult);
                        if (newBugRef != null)
                            bugRef = newBugRef;
                    }
                }
                return bugRef;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        
        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (BugTrackerPlugInContext plugIn in m_PlugIns)
                {
                    plugIn.Dispose();
                }
            }
        }

        public void  Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        #endregion IDisposable Members
    }
}
