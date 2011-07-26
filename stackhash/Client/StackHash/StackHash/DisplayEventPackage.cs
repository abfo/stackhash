using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using StackHash.StackHashService;
using System.Globalization;
using System.Collections.ObjectModel;

namespace StackHash
{
    /// <summary>
    /// WinQual event, cab list and hit list wrapper class
    /// </summary>
    public class DisplayEventPackage : INotifyPropertyChanged
    {
        private StackHashEventPackage _stackHashEventPackage;
        private ObservableCollection<DisplayCab> _cabs;
        private ObservableCollection<DisplayEventInfo> _eventInfoList;
        private string _exceptionMessage;
        private DisplayCollectionPolicy _cabCollectionPolicy;
        private DisplayMapping _workFlowDisplayMapping;
        private ObservableCollection<DisplayMapping> _availableWorkFlowDisplayMappings;

        /// <summary>
        /// Gets the effective cab collection policy for this event
        /// </summary>
        public DisplayCollectionPolicy CabCollectionPolicy
        {
            get { return _cabCollectionPolicy; }
        }

        /// <summary>
        /// Gets the exception message (if available)
        /// </summary>
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
        }

        /// <summary>
        /// Gets the ObserverableCollection of hits asociated with the event
        /// </summary>
        public ObservableCollection<DisplayEventInfo> EventInfoList
        {
            get { return _eventInfoList; }
        }

        /// <summary>
        /// Gets the ObservableCollection of cabs associated with the event
        /// </summary>
        public ObservableCollection<DisplayCab> Cabs
        {
            get { return _cabs; }
        }

        /// <summary>
        /// Gets the underlying StackHashEventPackage object
        /// </summary>
        public StackHashEventPackage StackHashEventPackage
        {
            get { return _stackHashEventPackage; }
        }

        // properties from StackHashEventPackage

        /// <summary />
        public int ProductId { get { return _stackHashEventPackage.ProductId; } }

        // properties from StackHashEvent

        /// <summary>
        /// Gets or temporarily sets the BugId. Setting a value here is not persisted but is necessary
        /// to display the value in the client until the event / event list is refreshed
        /// </summary>
        public string BugId 
        { 
            get 
            { 
                return _stackHashEventPackage.EventData.BugId; 
            }
            set
            {
                if (_stackHashEventPackage.EventData.BugId != value)
                {
                    _stackHashEventPackage.EventData.BugId = value;
                    RaisePropertyChanged("BugId");
                }
            }
        }


        /// <summary>
        /// Gets or temporarily sets the WorkFlowStatus. Setting a value here is not persisted but is necessary
        /// to display the value in the client until the event / event list is refreshed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorkFlow")]
        public int WorkFlowStatus 
        { 
            get 
            { 
                return _stackHashEventPackage.EventData.WorkFlowStatus; 
            }
            set
            {
                if (_stackHashEventPackage.EventData.WorkFlowStatus != value)
                {
                    _stackHashEventPackage.EventData.WorkFlowStatus = value;
                    RaisePropertyChanged("WorkFlowStatus");
                }
            }
        }

        /// <summary>
        /// Gets or temporarily sets the WorkFlowDisplayMapping. Settigns a value here is not
        /// persisted but is necesary to display the value in the client until the event / event list 
        /// is refreshed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorkFlow")]
        public DisplayMapping WorkFlowDisplayMapping
        {
            get { return _workFlowDisplayMapping; }
            set 
            {
                if (_workFlowDisplayMapping != value)
                {
                    _workFlowDisplayMapping = value;
                    RaisePropertyChanged("WorkFlowDisplayMapping");
                }
            }
        }

        /// <summary>
        /// Gets the list of available DisplayMappings for the WorkFlowDisplayMapping property
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorkFlow")]
        public ObservableCollection<DisplayMapping> AvailableWorkFlowDisplayMappings
        {
            get { return _availableWorkFlowDisplayMappings; }
            private set 
            {
                if (_availableWorkFlowDisplayMappings != value)
                {
                    _availableWorkFlowDisplayMappings = value;
                    RaisePropertyChanged("AvailableWorkFlowDisplayMappings");
                }
            }
        }

        /// <summary />
        public DateTime DateCreatedLocal { get { return _stackHashEventPackage.EventData.DateCreatedLocal; } }
        /// <summary />
        public DateTime DateModifiedLocal { get { return _stackHashEventPackage.EventData.DateModifiedLocal; } }
        /// <summary />
        public string EventTypeName { get { return _stackHashEventPackage.EventData.EventTypeName; } }
        /// <summary />
        public int FileId { get { return _stackHashEventPackage.EventData.FileId; } }
        /// <summary />
        public int Id { get { return _stackHashEventPackage.EventData.Id; } }
        /// <summary />
        public int TotalHits { get { return _stackHashEventPackage.EventData.TotalHits; } }
        /// <summary />
        public string PlugInBugId { get { return _stackHashEventPackage.EventData.PlugInBugId; } }
        /// <summary />
        public int CabCount { get { return _stackHashEventPackage.EventData.CabCount; } }

        // properties from StackHashEventSignature

        /// <summary />
        public string ApplicationName { get { return _stackHashEventPackage.EventData.EventSignature.ApplicationName; } }
        /// <summary />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeStamp")]
        public DateTime ApplicationTimeStamp { get { return _stackHashEventPackage.EventData.EventSignature.ApplicationTimeStamp; } }
        /// <summary />
        public string ApplicationVersion { get { return _stackHashEventPackage.EventData.EventSignature.ApplicationVersion; } }
        /// <summary />
        public long ExceptionCode { get { return _stackHashEventPackage.EventData.EventSignature.ExceptionCode; } }
        /// <summary />
        public string ModuleName { get { return _stackHashEventPackage.EventData.EventSignature.ModuleName; } }
        /// <summary />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeStamp")]
        public DateTime ModuleTimeStamp { get { return _stackHashEventPackage.EventData.EventSignature.ModuleTimeStamp; } }
        /// <summary />
        public string ModuleVersion { get { return _stackHashEventPackage.EventData.EventSignature.ModuleVersion; } }
        /// <summary />
        public long Offset { get { return _stackHashEventPackage.EventData.EventSignature.Offset; } }

        /// <summary>
        /// WinQual event, cab list and hit list wrapper class
        /// </summary>
        /// <param name="stackHashEventPackage">StackHashEventPackage</param>
        /// <param name="workFlowLookup">WorkFlow Lookup Dictionary</param>
        /// <param name="workFlowList">List of available WorkFlow DisplayMapping objects</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "workFlow")]
        public DisplayEventPackage(StackHashEventPackage stackHashEventPackage, Dictionary<int, DisplayMapping> workFlowLookup, ObservableCollection<DisplayMapping> workFlowList)
        {
            if (stackHashEventPackage == null) { throw new ArgumentNullException("stackHashEventPackage"); }
            if (workFlowLookup == null) { throw new ArgumentNullException("workFlowLookup"); }
            if (workFlowList == null) { throw new ArgumentNullException("workFlowList"); }
            if (stackHashEventPackage.EventData == null) { throw new ArgumentNullException("stackHashEventPackage", "stackHashEventPackage.EventData is null"); }
            if (stackHashEventPackage.EventData.EventSignature == null) { throw new ArgumentNullException("stackHashEventPackage", "stackHashEventPackage.EventData.EventSignature is null"); }

            _stackHashEventPackage = stackHashEventPackage;

            // update the exception message
            UpdateExceptionMessage();

            // update WorkFlowDisplayMapping
            this.WorkFlowDisplayMapping = workFlowLookup[stackHashEventPackage.EventData.WorkFlowStatus];
            this.AvailableWorkFlowDisplayMappings = workFlowList;

            _cabs = new ObservableCollection<DisplayCab>();
            _eventInfoList = new ObservableCollection<DisplayEventInfo>();

            if (_stackHashEventPackage.Cabs != null)
            {
                foreach (StackHashCabPackage cab in _stackHashEventPackage.Cabs)
                {
                    _cabs.Add(new DisplayCab(cab));
                }
            }

            if (_stackHashEventPackage.EventInfoList != null)
            {
                foreach (StackHashEventInfo hit in _stackHashEventPackage.EventInfoList)
                {
                    _eventInfoList.Add(new DisplayEventInfo(hit));
                }
            }
        }

        /// <summary>
        /// Updates the DisplayEventPackage from a StackHashEventPackage
        /// </summary>
        /// <param name="stackHashEventPackage">StackHashEventPackage</param>
        /// <param name="workFlowLookup">WorkFlow Lookup Dictionary</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "workFlow"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void UpdateEventPackage(StackHashEventPackage stackHashEventPackage, Dictionary<int, DisplayMapping> workFlowLookup)
        {
            if (stackHashEventPackage == null) { throw new ArgumentNullException("stackHashEventPackage"); }
            if (workFlowLookup == null) { throw new ArgumentNullException("workFlowLookup"); }
            if (stackHashEventPackage.EventData == null) { throw new ArgumentNullException("stackHashEventPackage", "stackHashEventPackage.EventData is null"); }
            if (stackHashEventPackage.EventData.EventSignature == null) { throw new ArgumentNullException("stackHashEventPackage", "stackHashEventPackage.EventData.EventSignature is null"); }
            if (stackHashEventPackage.EventData.Id != _stackHashEventPackage.EventData.Id) { throw new InvalidOperationException("Cannot update from an event pacakge with a different ID"); }

            // save the new event package
            StackHashEventPackage oldStackHashEventPackage = _stackHashEventPackage;
            _stackHashEventPackage = stackHashEventPackage;

            // update WorkFlowDisplayMapping
            this.WorkFlowDisplayMapping = workFlowLookup[stackHashEventPackage.EventData.WorkFlowStatus];

            // check for changes

            if (oldStackHashEventPackage.ProductId != _stackHashEventPackage.ProductId) { RaisePropertyChanged("ProductId"); }

            if (oldStackHashEventPackage.EventData.BugId != _stackHashEventPackage.EventData.BugId) { RaisePropertyChanged("BugId"); }
            if (oldStackHashEventPackage.EventData.DateCreatedLocal != _stackHashEventPackage.EventData.DateCreatedLocal) { RaisePropertyChanged("DateCreatedLocal"); }
            if (oldStackHashEventPackage.EventData.DateModifiedLocal != _stackHashEventPackage.EventData.DateModifiedLocal) { RaisePropertyChanged("DateModifiedLocal"); }
            if (oldStackHashEventPackage.EventData.EventTypeName != _stackHashEventPackage.EventData.EventTypeName) { RaisePropertyChanged("EventTypeName"); }
            if (oldStackHashEventPackage.EventData.FileId != _stackHashEventPackage.EventData.FileId) { RaisePropertyChanged("FileId"); }
            if (oldStackHashEventPackage.EventData.TotalHits != _stackHashEventPackage.EventData.TotalHits) { RaisePropertyChanged("TotalHits"); }
            if (oldStackHashEventPackage.EventData.CabCount != _stackHashEventPackage.EventData.CabCount) { RaisePropertyChanged("CabCount"); }
            if (oldStackHashEventPackage.EventData.PlugInBugId != _stackHashEventPackage.EventData.PlugInBugId) { RaisePropertyChanged("PlugInBugId"); }

            if (oldStackHashEventPackage.EventData.EventSignature.ApplicationName != _stackHashEventPackage.EventData.EventSignature.ApplicationName) { RaisePropertyChanged("ApplicationName"); }
            if (oldStackHashEventPackage.EventData.EventSignature.ApplicationTimeStamp != _stackHashEventPackage.EventData.EventSignature.ApplicationTimeStamp) { RaisePropertyChanged("ApplicationTimeStamp"); }
            if (oldStackHashEventPackage.EventData.EventSignature.ApplicationVersion != _stackHashEventPackage.EventData.EventSignature.ApplicationVersion) { RaisePropertyChanged("ApplicationVersion"); }
            if (oldStackHashEventPackage.EventData.EventSignature.ExceptionCode != _stackHashEventPackage.EventData.EventSignature.ExceptionCode) { RaisePropertyChanged("ExceptionCode"); }
            if (oldStackHashEventPackage.EventData.EventSignature.ModuleName != _stackHashEventPackage.EventData.EventSignature.ModuleName) { RaisePropertyChanged("ModuleName"); }
            if (oldStackHashEventPackage.EventData.EventSignature.ModuleTimeStamp != _stackHashEventPackage.EventData.EventSignature.ModuleTimeStamp) { RaisePropertyChanged("ModuleTimeStamp"); }
            if (oldStackHashEventPackage.EventData.EventSignature.ModuleVersion != _stackHashEventPackage.EventData.EventSignature.ModuleVersion) { RaisePropertyChanged("ModuleVersion"); }
            if (oldStackHashEventPackage.EventData.EventSignature.Offset != _stackHashEventPackage.EventData.EventSignature.Offset) { RaisePropertyChanged("Offset"); }

            // update the exception message
            UpdateExceptionMessage();

            // update cabs
            if (_stackHashEventPackage.Cabs == null)
            {
                _cabs.Clear();
            }
            else
            {
                bool cabFound = false;

                // update / add cabs
                foreach (StackHashCabPackage cab in _stackHashEventPackage.Cabs)
                {
                    cabFound = false;

                    foreach (DisplayCab displayCab in _cabs)
                    {
                        if (cab.Cab.Id == displayCab.Id)
                        {
                            cabFound = true;
                            displayCab.UpdateCab(cab);
                            break;
                        }
                    }

                    if (!cabFound)
                    {
                        _cabs.Add(new DisplayCab(cab));
                    }
                }

                // remove cabs no longer preset
                List<DisplayCab> displayCabsToRemove = new List<DisplayCab>();
                foreach(DisplayCab displayCab in _cabs)
                {
                    cabFound = false;
                    
                    foreach(StackHashCabPackage cab in _stackHashEventPackage.Cabs)
                    {
                        if (cab.Cab.Id == displayCab.Id)
                        {
                            cabFound = true;
                            break;
                        }
                    }

                    if (!cabFound)
                    {
                        displayCabsToRemove.Add(displayCab);
                    }
                }

                foreach(DisplayCab displayCab in displayCabsToRemove)
                {
                    _cabs.Remove(displayCab);
                }
            }

            // update event infos
            if (_stackHashEventPackage.EventInfoList == null)
            {
                _eventInfoList.Clear();
            }
            else
            {
                bool eventInfoFound = false;

                // update / add event infos
                foreach (StackHashEventInfo eventInfo in _stackHashEventPackage.EventInfoList)
                {
                    eventInfoFound = false;

                    foreach (DisplayEventInfo displayEventInfo in _eventInfoList)
                    {
                        if (displayEventInfo.IsMatchingEventInfo(eventInfo))
                        {
                            eventInfoFound = true;
                            displayEventInfo.UpdateEventInfo(eventInfo);
                            break;
                        }
                    }

                    if (!eventInfoFound)
                    {
                        _eventInfoList.Add(new DisplayEventInfo(eventInfo));
                    }
                }

                // remove event infos no longer present
                List<DisplayEventInfo> displayEventInfosToRemove = new List<DisplayEventInfo>();
                foreach (DisplayEventInfo displayEventInfo in _eventInfoList)
                {
                    eventInfoFound = false;

                    foreach (StackHashEventInfo eventInfo in _stackHashEventPackage.EventInfoList)
                    {
                        if (displayEventInfo.IsMatchingEventInfo(eventInfo))
                        {
                            eventInfoFound = true;
                            break;
                        }
                    }

                    if (!eventInfoFound)
                    {
                        displayEventInfosToRemove.Add(displayEventInfo);
                    }
                }

                foreach (DisplayEventInfo displayEventInfo in displayEventInfosToRemove)
                {
                    _eventInfoList.Remove(displayEventInfo);
                }
            }
        }

        /// <summary>
        /// Updates the effective cab collection policy for this event from a list of all policies
        /// </summary>
        /// <param name="policies">List of all policies</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public void UpdateCabCollectionPolicy(List<StackHashCollectionPolicy> policies)
        {
            if (policies == null) { throw new ArgumentNullException("policies"); }

            DisplayCollectionPolicy currentCabCollectionPolicy = DisplayCollectionPolicy.FindPolicy(policies,
                this.ProductId,
                this.FileId,
                this.Id,
                -1,
                StackHashCollectionObject.Cab,
                StackHashCollectionObject.Cab);

            if (currentCabCollectionPolicy != _cabCollectionPolicy)
            {
                if (_cabCollectionPolicy == null)
                {
                    _cabCollectionPolicy = currentCabCollectionPolicy;
                }
                else
                {
                    _cabCollectionPolicy.UpdateCollectionPolicy(currentCabCollectionPolicy.StackHashCollectionPolicy);
                }

                RaisePropertyChanged("CabCollectionPolicy");
            }
        }

        private void UpdateExceptionMessage()
        {
            string newExceptionMessage = ExceptionMessageHelper.Helper.GetMessage((uint)this.ExceptionCode);
            if (newExceptionMessage != _exceptionMessage)
            {
                _exceptionMessage = newExceptionMessage;
                RaisePropertyChanged("ExceptionMessage");
            }
        }

        #region INotifyPropertyChanged Members

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary />
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
