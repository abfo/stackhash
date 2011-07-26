using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashUtilities;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashParameter : ICloneable, IComparable
    {
        String m_Name;
        String m_Value;

        public StackHashParameter() {;}
        public StackHashParameter(string name, string value)
        {
            m_Name = name;
            m_Value = value;
        }

        [DataMember]
        public string Name 
        { 
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        #region ICloneable Members

        /// <summary>
        /// Clone a copy of the parameter.
        /// </summary>
        /// <returns>Cloned copy of the parameter.</returns>
        public object Clone()
        {
            StackHashParameter parameter = new StackHashParameter(m_Name, m_Value);
            return parameter;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashParameter thisParam = obj as StackHashParameter;

            if ((thisParam.Name == this.m_Name) &&
                (thisParam.m_Value == this.m_Value))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashParameterCollection : List<StackHashParameter>, IComparable
    {
        public StackHashParameterCollection() { ;}

        public StackHashParameter FindParameter(string name)
        {
            foreach (StackHashParameter thisParam in this)
            {
                if (thisParam.Name == name)
                    return thisParam;
            }
            return null;
        }


        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashParameterCollection parameters = obj as StackHashParameterCollection;

            if (parameters.Count != this.Count)
                return -1;

            foreach (StackHashParameter thisParam in this)
            {
                StackHashParameter matchingParam = parameters.FindParameter(thisParam.Name);

                if (matchingParam == null)
                    return -1;

                if ((matchingParam.Name == StackHashEventSignature.ParamApplicationTimeStamp) ||
                    (matchingParam.Name == StackHashEventSignature.ParamModuleTimeStamp))
                {
                    // Timestamps might be in a different format.
                    DateTime matchingTimeStamp = DateTime.Parse(matchingParam.Value, CultureInfo.InvariantCulture);
                    DateTime thisTimeStamp = DateTime.Parse(thisParam.Value, CultureInfo.InvariantCulture);

                    if (matchingTimeStamp != thisTimeStamp)
                        return -1;
                }
                else
                {
                    if (matchingParam.CompareTo(thisParam) != 0)
                    {
                        return -1;
                    }
                }
            }

            // Must be a match.
            return 0;
        }

        #endregion
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashEventSignature : ICloneable, IComparable
    {
        StackHashParameterCollection m_Parameters;
        String m_ApplicationName;
        String m_ApplicationVersion;
        DateTime m_ApplicationTimeStamp;
        String m_ModuleName;
        String m_ModuleVersion;
        DateTime m_ModuleTimeStamp;
        long m_Offset;
        long m_ExceptionCode;

        private const String s_ParamApplicationName = "applicationName";
        private const String s_ParamApplicationVersion = "applicationVersion";
        private const String s_ParamApplicationTimeStamp = "applicationTimeStamp";
        private const String s_ParamModuleName = "moduleName";
        private const String s_ParamModuleVersion = "moduleVersion";
        private const String s_ParamModuleTimeStamp = "moduleTimeStamp";
        private const String s_ParamExceptionCode = "exceptionCode";
        private const String s_ParamOffset = "offset";


        public StackHashEventSignature() {;}

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void InterpretParameters()
        {
            InterpretParameters(false);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public void InterpretParameters(bool justNumericFields)
        {
            foreach (StackHashParameter param in m_Parameters)
            {
                if (!justNumericFields)
                {
                    if (param.Name == s_ParamApplicationName)
                    {
                        m_ApplicationName = param.Value;
                    }
                    else if (param.Name == s_ParamApplicationVersion)
                    {
                        m_ApplicationVersion = param.Value;
                    }
                    else if (param.Name == s_ParamApplicationTimeStamp)
                    {
                        m_ApplicationTimeStamp = DateTime.Parse(param.Value, CultureInfo.InvariantCulture);
                    }
                    else if (param.Name == s_ParamModuleName)
                    {
                        m_ModuleName = param.Value;
                    }
                    else if (param.Name == s_ParamModuleVersion)
                    {
                        m_ModuleVersion = param.Value;
                    }
                    else if (param.Name == s_ParamModuleTimeStamp)
                    {
                        m_ModuleTimeStamp = DateTime.Parse(param.Value, CultureInfo.InvariantCulture);
                    }
                }

                // The exception code is of the form.... "C0000005" in the examples I have seen.
                // i.e. no 0x or X qualifiers. It is still to be interpretted as a hex number.
                if (param.Name == s_ParamExceptionCode)
                {
                    if (string.IsNullOrEmpty(param.Value))
                    {
                        // Default to 0.
                        m_ExceptionCode = 0;
                    }
                    else
                    {
                        string newValue = param.Value;

                        // Move past the 'x' or 'X'. If not found try to interpret as hex anyway.
                        int index = param.Value.IndexOf('x');

                        if (index == -1)
                            index = param.Value.IndexOf('X');

                        if (index != -1)
                            newValue = param.Value.Substring(index + 1);

                        try
                        {
                            m_ExceptionCode = long.Parse(newValue, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            DiagnosticsHelper.LogException(DiagSeverity.Warning,
                                String.Format(CultureInfo.InvariantCulture, "Invalid exception code: {0}", param.Value),
                                ex);
                        }
                    }

                }

                // The offset is of the form.... "0x12345678" in the examples I have seen so strip the first '0x' and then 
                // parse the rest as a hex number.
                // Note that an offset might be 64 bits on a 64 bit machine.
                else if (param.Name == s_ParamOffset)
                {
                    if (string.IsNullOrEmpty(param.Value))
                    {
                        m_Offset = 0;
                    }
                    else
                    {
                        string newValue = param.Value;
                        int index = param.Value.IndexOf('x');

                        if (index == -1)
                            index = param.Value.IndexOf('X');

                        if (index != -1)
                            newValue = param.Value.Substring(index + 1);

                        try
                        {
                            m_Offset = long.Parse(newValue, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            DiagnosticsHelper.LogException(DiagSeverity.Warning,
                                String.Format(CultureInfo.InvariantCulture, "Invalid offset: {0}", param.Value),
                                ex);
                        }
                    }
                }
            }
        }

        public StackHashEventSignature(StackHashParameterCollection parameters)
        {
            m_Parameters = parameters;
            InterpretParameters();
        }

        [DataMember]
        public StackHashParameterCollection Parameters
        {
            get { return m_Parameters; }
            set { m_Parameters = value; }
        }

        [DataMember]
        public String ApplicationName
        {
            get { return m_ApplicationName; }
            set { m_ApplicationName = value; }
        }

        [DataMember]
        public String ApplicationVersion
        {
            get { return m_ApplicationVersion; }
            set { m_ApplicationVersion = value; }
        }

        [DataMember]
        public DateTime ApplicationTimeStamp
        {
            get { return m_ApplicationTimeStamp; }
            set { m_ApplicationTimeStamp = value; }
        }

        [DataMember]
        public String ModuleName
        {
            get { return m_ModuleName; }
            set { m_ModuleName = value; }
        }

        [DataMember]
        public String ModuleVersion
        {
            get { return m_ModuleVersion; }
            set { m_ModuleVersion = value; }
        }

        [DataMember]
        public DateTime ModuleTimeStamp
        {
            get { return m_ModuleTimeStamp; }
            set { m_ModuleTimeStamp = value; }
        }

        [DataMember]
        public long Offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        [DataMember]
        public long ExceptionCode
        {
            get { return m_ExceptionCode; }
            set { m_ExceptionCode = value; }
        }

        public static String ParamApplicationName
        {
            get { return s_ParamApplicationName; }
        }
        public static String ParamApplicationVersion
        {
            get { return s_ParamApplicationVersion; }
        }
        public static String ParamApplicationTimeStamp
        {
            get { return s_ParamApplicationTimeStamp; }
        }
        public static String ParamModuleName
        {
            get { return s_ParamModuleName; }
        }
        public static String ParamModuleVersion
        {
            get { return s_ParamModuleVersion; }
        }
        public static String ParamModuleTimeStamp
        {
            get { return s_ParamModuleTimeStamp; }
        }
        public static String ParamExceptionCode
        {
            get { return s_ParamExceptionCode; }
        }
        public static String ParamOffset
        {
            get { return s_ParamOffset; }
        }
        
        #region ICloneable Members

        /// <summary>
        /// Clones the event signature.
        /// </summary>
        /// <returns>Cloned copy of the event signature.</returns>
        public object Clone()
        {
            StackHashParameterCollection parameterCollection = new StackHashParameterCollection();

            if (m_Parameters == null)
                m_Parameters = new StackHashParameterCollection();

            foreach (StackHashParameter parameter in m_Parameters)
            {
                parameterCollection.Add(parameter.Clone() as StackHashParameter);
            }
            StackHashEventSignature eventSignature = new StackHashEventSignature(parameterCollection);
            eventSignature.ApplicationName = m_ApplicationName;
            eventSignature.ApplicationTimeStamp = m_ApplicationTimeStamp;
            eventSignature.ApplicationVersion = m_ApplicationVersion;
            eventSignature.ExceptionCode = m_ExceptionCode;
            eventSignature.ModuleName = m_ModuleName;
            eventSignature.ModuleTimeStamp = m_ModuleTimeStamp;
            eventSignature.ModuleVersion = m_ModuleVersion;
            eventSignature.Offset = m_Offset;

            return eventSignature;
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares this object to the specified one.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>0 for equal, or -1 if not.</returns>
        public int CompareTo(object obj)
        {
            StackHashEventSignature eventSignature = obj as StackHashEventSignature;

            if (eventSignature.Parameters.Count != this.m_Parameters.Count)
                return -1;

            if ((eventSignature.ApplicationName == m_ApplicationName) &&
                (eventSignature.ApplicationVersion == m_ApplicationVersion) &&
                (eventSignature.ExceptionCode == m_ExceptionCode) &&
                (eventSignature.ModuleName == m_ModuleName) &&
                (eventSignature.Offset == m_Offset) &&
                (eventSignature.Parameters.CompareTo(m_Parameters) == 0))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }


    /// <summary>
    ///  BucketID is an internal term.  On the WinQual portal, you'll see it as "EventID."  It's generated by WATSON when a new report comes in.  
    ///  Identity-seeded Primary Key on each of the tables (Crash32, Crash64, Generic)
    ///  The Bucket itself is based on the following: 
    ///  1) Application Name
    ///  2) Application Version
    ///  3) Module Name
    ///  4) Module Version
    ///  5) Offset
    ///  6) Exception Code
    ///  
    /// Therefore the EventId on its own is not unique. The EventId + EventType form a unique pairing.
    /// </summary>    
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashEvent : ICloneable, IComparable
    {
        private const int s_ThisStructureVersion = 1;
        private int m_StructureVersion;

        private DateTime m_DateCreatedLocal;
        private DateTime m_DateModifiedLocal;
        private string m_EventTypeName;
        private int m_Id;
        private StackHashEventSignature m_EventSignature;
        private int m_TotalHits;
        private int m_FileId;
        private String m_BugId;
        private String m_PlugInBugId;
        private int m_WorkFlowStatus;
        private String m_WorkFlowStatusName;
        private int m_CabCount;

        public StackHashEvent() {;}  // Needed to serialize;

        public StackHashEvent(DateTime dateCreatedLocal,
                             DateTime dataModifiedLocal,
                             string eventTypeName,
                             int id,
                             StackHashEventSignature eventSignature,
                             int totalHits,
                             int fileId)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dataModifiedLocal;
            m_EventTypeName = eventTypeName;
            m_Id = id;
            m_EventSignature = eventSignature;
            m_TotalHits= totalHits;
            m_FileId = fileId;
            m_WorkFlowStatus = 0;
            m_WorkFlowStatusName = "Unknown";
        }

        public StackHashEvent(DateTime dateCreatedLocal,
                             DateTime dataModifiedLocal,
                             string eventTypeName,
                             int id,
                             StackHashEventSignature eventSignature,
                             int totalHits,
                             int fileId,
                             String bugId)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dataModifiedLocal;
            m_EventTypeName = eventTypeName;
            m_Id = id;
            m_EventSignature = eventSignature;
            m_TotalHits = totalHits;
            m_FileId = fileId;
            m_BugId = bugId;
            m_WorkFlowStatus = 0;
            m_WorkFlowStatusName = "Unknown";
        }

        public StackHashEvent(DateTime dateCreatedLocal,
                             DateTime dataModifiedLocal,
                             string eventTypeName,
                             int id,
                             StackHashEventSignature eventSignature,
                             int totalHits,
                             int fileId,
                             String bugId,
                             String plugInBugId)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dataModifiedLocal;
            m_EventTypeName = eventTypeName;
            m_Id = id;
            m_EventSignature = eventSignature;
            m_TotalHits = totalHits;
            m_FileId = fileId;
            m_BugId = bugId;
            m_PlugInBugId = plugInBugId;
            m_WorkFlowStatus = 0;
            m_WorkFlowStatusName = "Unknown";
        }


        public StackHashEvent(DateTime dateCreatedLocal,
                             DateTime dataModifiedLocal,
                             string eventTypeName,
                             int id,
                             StackHashEventSignature eventSignature,
                             int totalHits,
                             int fileId,
                             String bugId,
                             String plugInBugId,
                             int workFlowStatus,
                             String workFlowStatusName)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = dateCreatedLocal;
            m_DateModifiedLocal = dataModifiedLocal;
            m_EventTypeName = eventTypeName;
            m_Id = id;
            m_EventSignature = eventSignature;
            m_TotalHits = totalHits;
            m_FileId = fileId;
            m_BugId = bugId;
            m_PlugInBugId = plugInBugId;
            m_WorkFlowStatus = workFlowStatus;
            m_WorkFlowStatusName = workFlowStatusName;
        }


        
        public StackHashEvent(int id, string eventTypeName)
        {
            m_StructureVersion = s_ThisStructureVersion;
            m_DateCreatedLocal = DateTime.Now;
            m_DateModifiedLocal = DateTime.Now;
            m_EventTypeName = eventTypeName;
            m_Id = id;
            m_EventSignature = new StackHashEventSignature();
            m_TotalHits = 0;
            m_FileId = 0;
            m_WorkFlowStatus = 0;
            m_WorkFlowStatusName = "Unknown";
        }

        
        [DataMember]
        public DateTime DateCreatedLocal
        {
            get { return m_DateCreatedLocal; }
            set { m_DateCreatedLocal = value; }
        }

        [DataMember]
        public DateTime DateModifiedLocal
        {
            get { return m_DateModifiedLocal; }
            set { m_DateModifiedLocal = value; }
        }

        [DataMember]
        public string EventTypeName
        {
            get { return m_EventTypeName; }
            set { m_EventTypeName = value; }
        }

        [DataMember]
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        [DataMember]
        public StackHashEventSignature EventSignature
        {
            get { return m_EventSignature; }
            set { m_EventSignature = value; }
        }

        [DataMember]
        public int TotalHits
        {
            get { return m_TotalHits; }
            set { m_TotalHits = value; }
        }

        [DataMember]
        public int FileId
        {
            get { return m_FileId; }
            set { m_FileId = value; }
        }

        [DataMember]
        public String BugId
        {
            get { return m_BugId; }
            set { m_BugId = value; }
        }

        [DataMember]
        public int StructureVersion
        {
            get { return m_StructureVersion; }
            set { m_StructureVersion = value; }
        }

        public static int ThisStructureVersion
        {
            get { return s_ThisStructureVersion; }
        }

        [DataMember]
        public String PlugInBugId
        {
            get { return m_PlugInBugId; }
            set { m_PlugInBugId = value; }
        }

        [DataMember]
        public int WorkFlowStatus
        {
            get { return m_WorkFlowStatus; }
            set { m_WorkFlowStatus = value; }
        }

        [DataMember]
        public String WorkFlowStatusName
        {
            get { return m_WorkFlowStatusName; }
            set { m_WorkFlowStatusName = value; }
        }

        [DataMember]
        public int CabCount
        {
            get { return m_CabCount; }
            set { m_CabCount = value; }
        }
        
        public bool UpdateNewFields(int fileId)
        {
            // fileId was added late - so init if set to 0.
            if (m_FileId == 0)
            {
                m_FileId = fileId;
                return true; // Updated.
            }
            else
            {
                return false; // Not updated.
            }
        }

        public void SetWinQualFields(StackHashEvent newEvent)
        {
            if (newEvent == null)
                throw new ArgumentNullException("newEvent");

            // Don't set the fields that are set by the user - e.g. bugid.
            DateCreatedLocal = newEvent.DateCreatedLocal;
            DateModifiedLocal = newEvent.DateModifiedLocal;
            EventTypeName = newEvent.EventTypeName;
            Id = newEvent.Id;
            EventSignature = newEvent.EventSignature;
            TotalHits = newEvent.TotalHits;
            FileId = newEvent.FileId;
        }

        /// <summary>
        /// Determines if a change in the event data should be reported to bug tracking plugins.
        /// </summary>
        /// <param name="newEvent">The new event data.</param>
        /// <param name="checkNonWinQualFields">True - the non-winqual fields are compared too.</param>
        /// <returns>True - significant changes so report it, False - nothing to report.</returns>
        public bool ShouldReportToBugTrackPlugIn(StackHashEvent newEvent, bool checkNonWinQualFields)
        {
            if (newEvent == null)
                throw new ArgumentNullException("newEvent");

            if (this.Id != newEvent.Id)
                return true;
            if (this.EventTypeName != newEvent.EventTypeName)
                return true;
            if (this.EventSignature.CompareTo(newEvent.EventSignature) != 0)
                return true;

            if (checkNonWinQualFields)
            {
                if (this.TotalHits != newEvent.TotalHits)
                    return true;
                if (this.BugId != newEvent.BugId)
                    return true;

                // Don't check the file ID as an event may be associated with several files.
                // Also don't check the m_PlugInBugId because this is set by the plug-in.
            }

            return false;
        }


        #region ICloneable Members

        /// <summary>
        /// Clone a copy of the event.
        /// </summary>
        /// <returns>Cloned copy of the event.</returns>
        public object Clone()
        {
            StackHashEventSignature eventSignatureClone;

            if (m_EventSignature != null)
                eventSignatureClone = m_EventSignature.Clone() as StackHashEventSignature;
            else
                eventSignatureClone = new StackHashEventSignature();

            StackHashEvent theEvent = new StackHashEvent(m_DateCreatedLocal, m_DateModifiedLocal,
                m_EventTypeName, m_Id, eventSignatureClone, m_TotalHits, m_FileId, m_BugId);

            return theEvent;
        }

        #endregion

        #region IComparable Members


        /// <summary>
        /// Compares this object to the specified one.
        /// </summary>
        /// <param name="theEvent">Object to compare to.</param>
        /// <param name="onlyWinQualFields">Only compare the WinQual fields.</param>
        /// <returns>0 for equal, or -1 if not.</returns>
        public int CompareTo(StackHashEvent theEvent, bool onlyWinQualFields)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            if ((theEvent.DateCreatedLocal == m_DateCreatedLocal) &&
                (theEvent.DateModifiedLocal == m_DateModifiedLocal) &&
                (theEvent.EventSignature.CompareTo(m_EventSignature) == 0) &&
                (theEvent.EventTypeName == m_EventTypeName) &&
                (theEvent.Id == m_Id))
            {
                if (!onlyWinQualFields)
                {
                    if ((theEvent.TotalHits == m_TotalHits) &&
                        (theEvent.FileId == m_FileId) &&
                        (theEvent.BugId == m_BugId) &&
                        (theEvent.PlugInBugId == m_PlugInBugId) &&
                        (theEvent.WorkFlowStatus == m_WorkFlowStatus))
                        //(theEvent.WorkFlowStatusName == m_WorkFlowStatusName)) - name is a transient field and may change.
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Compares this object to the specified one.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>0 for equal, or -1 if not.</returns>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as StackHashEvent, false);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder outString = new StringBuilder("Event: DateCreated:");
            outString.Append(m_DateCreatedLocal);
            outString.Append(" DateModified:");
            outString.Append(m_DateModifiedLocal);
            outString.Append(" Id:");
            outString.Append(m_Id);
            outString.Append(" EventTypeName:");
            outString.Append(m_EventTypeName);
            outString.Append(" TotalHits:");
            outString.Append(m_TotalHits);
            outString.Append(" FileId:");
            outString.Append(m_FileId);
            outString.Append(" WorkFlowStatus:");
            outString.Append(m_WorkFlowStatus);
            outString.Append(" WorkFlowStatusName:");
            outString.Append(m_WorkFlowStatusName);

            if ((this.EventSignature != null) && (this.EventSignature.Parameters != null))
            {
                foreach (StackHashParameter thisParam in this.EventSignature.Parameters)
                {
                    outString.Append(" ");
                    if (!String.IsNullOrEmpty(thisParam.Name))
                        outString.Append(thisParam.Name);
                    outString.Append("=");
                    if (!String.IsNullOrEmpty(thisParam.Value))
                        outString.Append(thisParam.Value);
                }
            }

            return outString.ToString();
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashEventCollection : Collection<StackHashEvent>, IComparable
    {
        public StackHashEventCollection() { } // Needed to serialize.

        ///// <summary>
        ///// Locates the event with the specified id.
        ///// </summary>
        ///// <param name="eventId">Id of file to find.</param>
        ///// <returns>Found event or null.</returns>
        //public StackHashEvent FindEvent(int eventId)
        //{
        //    for (int i = 0; i < this.Count; i++)
        //    {
        //        if (this[i].Id == eventId)
        //            return this[i];
        //    }

        //    return null;
        //}
        /// <summary>
        /// Locates the event with the specified id.
        /// </summary>
        /// <param name="eventId">Id of file to find.</param>
        /// <returns>Found event or null.</returns>
        public StackHashEvent FindEvent(int eventId, String eventTypeName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if ((this[i].Id == eventId) &&
                    (this[i].EventTypeName == eventTypeName))
                    return this[i];
            }

            return null;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashEventCollection events = obj as StackHashEventCollection;

            if (events.Count != this.Count)
                return -1;

            foreach (StackHashEvent thisEvent in this)
            {
                StackHashEvent matchingEvent = events.FindEvent(thisEvent.Id, thisEvent.EventTypeName);

                if (matchingEvent == null)
                    return -1;

                if (matchingEvent.CompareTo(thisEvent) != 0)
                    return -1;
            }

            // Must be a match.
            return 0;
        }

        #endregion
    }
}
