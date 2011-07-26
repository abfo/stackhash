using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashBugTrackerInterfaceV1;

[assembly: CLSCompliant(true)]
namespace StackHashTestBugTrackerPlugIn
{
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA2210")]
    public class TestBugTrackerPlugInControl : MarshalByRefObject, IBugTrackerV1Control
    {
        private static string s_PlugInName = "TestPlugIn";
        private static string s_PlugInDescription = "Plug-in used to control StackHash unit testing.";
        private static TestBugTrackerPlugInControl s_ThisInstance;
        private List<TestBugTrackerPlugInContext> m_AllContexts = new List<TestBugTrackerPlugInContext>();
        private NameValueCollection m_Properties = new NameValueCollection();

        public static TestBugTrackerPlugInControl ThisInstance
        {
            get { return s_ThisInstance; }
        }

        // Must define a public default constructor.
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public TestBugTrackerPlugInControl()
        {
            s_ThisInstance = this;

            m_Properties["Param1"] = "1";
            m_Properties["Param2"] = "2";
            m_Properties["Param3"] = "3";
            m_Properties["Param4"] = "4";
            m_Properties["Param5"] = "5";
            m_Properties["Param6"] = "6";
            m_Properties["Param7"] = "7";
            m_Properties["Param8"] = "8";
            m_Properties["Param9"] = "9";
            m_Properties["Param10"] = "10";
            m_Properties["Param11"] = "11";
            m_Properties["Param12"] = "12";
            m_Properties["Param13"] = "Much longer parameter value";
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Control Interface created");
        }

        public String PlugInName
        {
            get
            {
                return s_PlugInName;
            }
        }

        public String PlugInDescription
        {
            get
            {
                return s_PlugInDescription;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public bool PlugInSetsBugReference
        {
            get { return true; }
        }

        public Uri HelpUrl
        {
            get
            {
                return new Uri("http://www.stackhash.com");
            }
        }

        
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public NameValueCollection PlugInDiagnostics
        {
            get
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Getting plug-in diagnostics");
                NameValueCollection diagnostics = new NameValueCollection();
                return diagnostics;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public NameValueCollection PlugInDefaultProperties
        {
            get
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Getting plug-in default properties");
                return m_Properties;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public IBugTrackerV1Context CreateContext()
        {
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Creating new context");
            TestBugTrackerPlugInContext newContext = new TestBugTrackerPlugInContext();
            m_AllContexts.Add(newContext);
            return newContext;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void ReleaseContext(IBugTrackerV1Context context)
        {
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Releasing context");
            m_AllContexts.Remove(context as TestBugTrackerPlugInContext);            
        }
    }

        
    [Serializable]
    public class TestBugTrackerPlugInContext : MarshalByRefObject, IBugTrackerV1Context, IDisposable
    {
        private int m_ProductAddedCount;
        private int m_ProductUpdatedCount;
        private int m_FileAddedCount;
        private int m_FileUpdatedCount;
        private int m_EventAddedCount;
        private int m_EventUpdatedCount;
        private int m_EventCompleteCount;
        private int m_CabAddedCount;
        private int m_CabUpdatedCount;
        private int m_CabNoteAddedCount;
        private int m_EventNoteAddedCount;
        private int m_ScriptRunCount;
        private String m_LastEventNote;
        private String m_LastCabNote;
        private static String s_PlugInName = "TestPlugIn";
        private TestBugTrackerPlugInControl m_Controller;
        private Dictionary<long, int> m_CabsPerEvent = new Dictionary<long, int>();

        private NameValueCollection m_Properties = new NameValueCollection();

        public TestBugTrackerPlugInContext()
        {
        }

        // Must define a public default constructor.
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public TestBugTrackerPlugInContext(TestBugTrackerPlugInControl controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "context created");
            m_Controller = controller;
            m_Properties = m_Controller.PlugInDefaultProperties;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public NameValueCollection ContextDiagnostics
        {
            get
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Getting context diagnostics");
                NameValueCollection diagnostics = new NameValueCollection();

                diagnostics.Add("ProductAddedCount", m_ProductAddedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("ProductUpdatedCount", m_ProductUpdatedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("FileAddedCount", m_FileAddedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("FileUpdatedCount", m_FileUpdatedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("EventAddedCount", m_EventAddedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("EventUpdatedCount", m_EventUpdatedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("EventNoteAddedCount", m_EventNoteAddedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("EventCompleteCount", m_EventCompleteCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("CabAddedCount", m_CabAddedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("CabUpdatedCount", m_CabUpdatedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("CabNoteAddedCount", m_CabNoteAddedCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("ScriptRunCount", m_ScriptRunCount.ToString(CultureInfo.InvariantCulture));
                diagnostics.Add("LastEventNote", m_LastEventNote);
                diagnostics.Add("LastCabNote", m_LastCabNote);

                return diagnostics;
            }
        }


        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public NameValueCollection Properties
        {
            get
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Getting properties");
                return m_Properties;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void SetSelectPropertyValues(NameValueCollection propertiesToSet)
        {
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Setting properties");
            if (propertiesToSet == null)
                throw new ArgumentNullException("propertiesToSet");

            foreach (String key in propertiesToSet)
            {
                m_Properties[key] = propertiesToSet[key];
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void ProductUpdated(BugTrackerReportType reportType, BugTrackerProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Product Updated: " + product.ToString());
            if (product == null)
                throw new ArgumentNullException("product");

            m_ProductUpdatedCount++;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void ProductAdded(BugTrackerReportType reportType, BugTrackerProduct product)
        {
            m_ProductAddedCount++;

            if (product == null)
                throw new ArgumentNullException("product");

            if (m_Properties["ProductAddedException"] != null)
                throw new ArgumentException("Test exception");

            if (m_Properties["UnhandledException"] != null)
            {
                // Spawn a new thread to cause the exception.
                Thread exceptionThread = new Thread(delegate()
                {
                    throw new ArgumentException("Thread exception");
                });

                exceptionThread.Start();

                Thread.Sleep(2000);
                return;
            }

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Product Added: " + product.ToString());
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void FileUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "File Updated: " + file.ToString());

            m_FileUpdatedCount++;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void FileAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "File Added: " + file.ToString());

            m_FileAddedCount++;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string EventUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            String bugId = theEvent.BugReference;
            if (m_Properties["SetBugId"] == "True")
                bugId = "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Event Updated: " + theEvent.ToString());

            m_EventUpdatedCount++;
            return bugId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string EventManualUpdateCompleted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Event Manual Update Completed: " + theEvent.ToString());

            m_EventCompleteCount++;
            return theEvent.BugReference;
        }


        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string EventAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            m_EventAddedCount++;
            String bugId = theEvent.BugReference;
            if (m_Properties["SetBugId"] == "True")
                bugId = "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Event Added: " + theEvent.ToString());
            return bugId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string EventNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerNote note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");
            m_LastEventNote = note.Note;

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Event Note Added: " + note.ToString());
            // If the SetBugId is set then the plugin should have set the bug reference so subsequent calls using that event should
            // also contain the bug reference.
            if (m_Properties["SetBugId"] == "True")
                if (theEvent.PlugInBugReference != "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture))
                    throw new ArgumentException("Bug ref not set", "theEvent");
            m_EventNoteAddedCount++;
            return null;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string CabUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (cab.CabPathAndFileName == null)
                throw new ArgumentException("Cab path is null", "cab");

            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Cab Updated: " + cab.ToString());
            // If the SetBugId is set then the plugin should have set the bug reference so subsequent calls using that event should
            // also contain the bug reference.
            if (m_Properties["SetBugId"] == "True")
                if (theEvent.PlugInBugReference != "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture))
                    throw new ArgumentException("Bug ref not set", "theEvent");

            String bugId = null;
            if (m_Properties["CabUpdatedSetBugId"] == "True")
                bugId = "TestCabUpdatedPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);
            if ((m_Properties["ManualCabAddedSetBugId"] == "True") && (reportType != BugTrackerReportType.Automatic))
                bugId = "ManualCabAddedSetBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);
            m_CabUpdatedCount++;

            return bugId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string CabAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (cab.CabPathAndFileName == null)
                throw new ArgumentException("Cab path is null", "cab");
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Cab Added: " + cab.ToString());

            // If the SetBugId is set then the plugin should have set the bug reference so subsequent calls using that event should
            // also contain the bug reference.
            if (m_Properties["SetBugId"] == "True")
                if (theEvent.PlugInBugReference != "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture))
                    throw new ArgumentException("Bug ref not set", "theEvent");

            String bugId = null;
            if (m_Properties["CabAddedSetBugId"] == "True")
                bugId = "TestCabAddedPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);

            if ((m_Properties["ManualCabAddedSetBugId"] == "True") && (reportType != BugTrackerReportType.Automatic))
            {
                if (m_CabsPerEvent.ContainsKey(theEvent.EventId))
                {
                    if (!theEvent.PlugInBugReference.StartsWith("ManualCabAddedSetBugId", StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException("Plug-in bug reference should be set already", "theEvent");
                    m_CabsPerEvent[theEvent.EventId]++;
                }
                else
                {
                    m_CabsPerEvent[theEvent.EventId] = 1;
                }
                bugId = "ManualCabAddedSetBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);
            }
            m_CabAddedCount++;
            return bugId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string CabNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerNote note)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");
            if (cab.CabPathAndFileName == null)
                throw new ArgumentException("Cab path is null", "cab");
            m_LastCabNote = note.Note;
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Cab Note Added: " + note.ToString());

            // If the SetBugId is set then the plugin should have set the bug reference so subsequent calls using that event should
            // also contain the bug reference.
            if (m_Properties["SetBugId"] == "True")
                if (theEvent.PlugInBugReference != "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture))
                    throw new ArgumentException("Bug ref not set", "theEvent");
            m_CabNoteAddedCount++;

            String bugId = null;
            if (m_Properties["CabNoteAddedBugId"] == "True")
                bugId = "TestCabNoteAddedPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);
            if ((m_Properties["ManualCabAddedSetBugId"] == "True") && (reportType != BugTrackerReportType.Automatic))
                bugId = "ManualCabAddedSetBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);

            return bugId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public string DebugScriptExecuted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerScriptResult scriptResult)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (scriptResult == null)
                throw new ArgumentNullException("scriptResult");
            BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Script Added: " + scriptResult.ToString());
            // If the SetBugId is set then the plugin should have set the bug reference so subsequent calls using that event should
            // also contain the bug reference.
            if (m_Properties["SetBugId"] == "True")
                if (theEvent.PlugInBugReference != "TestPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture))
                    throw new ArgumentException("Bug ref not set", "theEvent");
            m_ScriptRunCount++;
            String bugId = null;
            if (m_Properties["DebugScriptExecutedBugId"] == "True")
                bugId = "TestDebugScriptExecutedPlugInBugId" + theEvent.EventId.ToString(CultureInfo.InvariantCulture);

            return bugId;
        }


        #region IDisposable Members

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                BugTrackerTrace.LogMessage(BugTrackerTraceSeverity.Information, s_PlugInName, "Context disposing");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion IDisposable Members
    }
}
