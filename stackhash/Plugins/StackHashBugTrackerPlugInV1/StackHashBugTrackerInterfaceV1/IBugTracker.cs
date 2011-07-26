using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

[assembly: CLSCompliant(true)]
namespace StackHashBugTrackerInterfaceV1
{
    public enum BugTrackerReportType
    {
        Automatic,
        ManualFull,
        ManualProduct,
        ManualFile,
        ManualEvent,
        ManualCab,
        ManualScript,        
    }

    public interface IBugTrackerV1Control
    {
        String PlugInName
        {
            get;
        }

        String PlugInDescription
        {
            get;
        }

        [SuppressMessage("Microsoft.Naming", "CA1702")]
        bool PlugInSetsBugReference
        {
            get;
        }

        Uri HelpUrl
        {
            get;
        }

        NameValueCollection PlugInDefaultProperties
        {
            get;
        }

        NameValueCollection PlugInDiagnostics
        {
            get;
        }

        IBugTrackerV1Context CreateContext();
        void ReleaseContext(IBugTrackerV1Context context);
    }

    
    public interface IBugTrackerV1Context
    {
        NameValueCollection Properties
        {
            get;
        }

        NameValueCollection ContextDiagnostics
        {
            get;
        }

        void SetSelectPropertyValues(NameValueCollection propertiesToSet);

        void ProductUpdated(BugTrackerReportType reportType, BugTrackerProduct product);
        void ProductAdded(BugTrackerReportType reportType, BugTrackerProduct product);

        void FileUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file);
        void FileAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file);

        string EventUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent);
        string EventAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent);
        string EventManualUpdateCompleted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent);
        string EventNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerNote note);

        string CabUpdated(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab);
        string CabAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab);
        string CabNoteAdded(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerNote note);

        string DebugScriptExecuted(BugTrackerReportType reportType, BugTrackerProduct product, BugTrackerFile file, BugTrackerEvent theEvent, BugTrackerCab cab, BugTrackerScriptResult scriptResult);
    }
}
