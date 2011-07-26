using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [Flags] 
    public enum StackHashReportOptions
    {
        [EnumMember()]
        IncludeAllObjects = 1,
        [EnumMember()]
        IncludeProducts = 2,
        [EnumMember()]
        IncludeFiles = 4,
        [EnumMember()]
        IncludeEvents = 8,
        [EnumMember()]
        IncludeCabs = 16,
        [EnumMember()]
        IncludeScriptResults = 32,
        [EnumMember()]
        IncludeEventNotes = 64,
        [EnumMember()]
        IncludeCabNotes = 128,
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugTrackerPlugInSelection
    {
        private String m_Name;

        public StackHashBugTrackerPlugInSelection() { ; }  // Required for serialization.

        public StackHashBugTrackerPlugInSelection(String name)
        {
            m_Name = name;
        }

        [DataMember]
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashBugTrackerPlugInSelectionCollection : Collection<StackHashBugTrackerPlugInSelection>
    {
        public StackHashBugTrackerPlugInSelectionCollection() { } // Needed to serialize.

        public bool ContainsName(String plugInName)
        {
            foreach (StackHashBugTrackerPlugInSelection plugInSelection in this)
            {
                if (plugInSelection.Name == plugInName)
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashBugReportData
    {
        private StackHashProduct m_Product;
        private StackHashFile m_File;
        private StackHashEvent m_Event;
        private StackHashCab m_Cab;
        private String m_ScriptName;
        private StackHashReportOptions m_Options;

        public StackHashBugReportData() { ;}

        public StackHashBugReportData(StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab,
            String scriptName, StackHashReportOptions options)
        {
            m_Product = product;
            m_File = file;
            m_Event = theEvent;
            m_Cab = cab;
            m_ScriptName = scriptName;
            m_Options = options;
        }

        [DataMember]
        public StackHashProduct Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }

        [DataMember]
        public StackHashFile File
        {
            get { return m_File; }
            set { m_File = value; }
        }

        [DataMember]
        public StackHashEvent TheEvent
        {
            get { return m_Event; }
            set { m_Event = value; }
        }

        [DataMember]
        public StackHashCab Cab
        {
            get { return m_Cab; }
            set { m_Cab = value; }
        }

        [DataMember]
        public String ScriptName
        {
            get { return m_ScriptName; }
            set { m_ScriptName = value; }
        }

        [DataMember]
        public StackHashReportOptions Options
        {
            get { return m_Options; }
            set { m_Options = value; }
        }

        /// <summary>
        /// Determines if the specified report request are conflicting with this one.
        /// A bug report is in conflict.
        /// 1) Either is a FULL report. Cannot run any other report while a full report is running and 
        ///    cannot start a fill report if any other report is currently running.
        /// 2) 2 product reports for the same product or a product and any other sync for the same product.
        /// 3) 2 events reports for the same event.
        /// </summary>
        /// <param name="other">Report data to compare to.</param>
        /// <returns>True - in conflict, False - not in conflict.</returns>
        public bool IsConflicting(StackHashBugReportData other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            // You cannot run ANY other type of report when a FULL report is already running.
            // You cannot run a FULL report when any other report is running.
            if ((this.Product == null) || (other.Product == null))
            {
                return true;
            }

            // Neither is a FULL report. Check for Product reports.
            if ((this.File == null) || (other.File == null))
            {
                // If either is a product report then the other cannot be for the same product.
                if (this.Product.Id == other.Product.Id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // This condition shouldn't happen. If it does it is a programmer error.
                if ((this.TheEvent == null) || (other.TheEvent == null))
                    throw new ArgumentException("Invalid report data", "other");

                // Both are event reports. Only allow them to run if they apply to different
                // events.
                // The same event may apply to different products/files so don't check for matching product and 
                // files.
                if ((this.TheEvent.Id != other.TheEvent.Id) ||
                    (this.TheEvent.EventTypeName != other.TheEvent.EventTypeName))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Overridden to provide string for email reports.
        /// </summary>
        /// <returns>Description of report.</returns>
        public override String ToString()
        {
            StringBuilder result = new StringBuilder();

            if (this.Product == null)
            {
                result.AppendLine("Full Report");
            }
            else if (this.File == null)
            {
                result.AppendLine("Product Report for: ");
                result.AppendLine(m_Product.ToString());
            }
            else if (this.TheEvent == null)
            {
                result.AppendLine("File Report for: ");
                result.AppendLine(Product.ToString());
                result.AppendLine(File.ToString());
            }
            else if (this.Cab == null)
            {
                result.AppendLine("Event Report for: ");
                result.AppendLine(Product.ToString());
                result.AppendLine(File.ToString());
                result.AppendLine(TheEvent.ToString());
            }
            else if (this.ScriptName == null)
            {
                result.AppendLine("Cab Report for: ");
                result.AppendLine(Product.ToString());
                result.AppendLine(File.ToString());
                result.AppendLine(TheEvent.ToString());
                result.AppendLine(Cab.ToString());
            }
            else
            {
                result.AppendLine("Script Report for: ");
                result.AppendLine(Product.ToString());
                result.AppendLine(File.ToString());
                result.AppendLine(TheEvent.ToString());
                result.AppendLine(Cab.ToString());
                result.AppendLine("Script: ");
                result.AppendLine(ScriptName);
            }

            return result.ToString();
        }
    }


    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashBugReportDataCollection : Collection<StackHashBugReportData>
    {
        public StackHashBugReportDataCollection() { } // Needed to serialize.


        /// <summary>
        /// Checks if any of the individual reports conflict with any other.
        /// </summary>
        /// <param name="other">The report collection to compare to.</param>
        /// <returns>True - conflicting, False - not conflicting.</returns>
        public bool IsConflicting(StackHashBugReportDataCollection other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (StackHashBugReportData reportData in this)
            {
                foreach (StackHashBugReportData otherReportData in other)
                {
                    if (reportData.IsConflicting(otherReportData))
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Overridden to provide string for email reports.
        /// </summary>
        /// <returns>Description of report.</returns>
        public override String ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (StackHashBugReportData reportData in this)
            {
                result.AppendLine(reportData.ToString());            
            }

            return result.ToString();
        }
    }
}
