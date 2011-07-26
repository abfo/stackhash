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
    /// StackHashCabPackage wrapper class
    /// </summary>
    public class DisplayCab : INotifyPropertyChanged
    {
        private StackHashCabPackage _stackHashCabPackage;
        private ObservableCollection<DisplayCabFile> _files;

        /// <summary>
        /// Gets the underlying StackHashCab object
        /// </summary>
        public StackHashCabPackage StackHashCabPackage
        {
            get { return _stackHashCabPackage; }
        }

        /// <summary>
        /// Gets the files contained within the cab
        /// </summary>
        public ObservableCollection<DisplayCabFile> Files
        {
            get { return _files; }
        }

        // properties from StackHashCabPackage

        /// <summary />
        public string FullPath { get { return _stackHashCabPackage.FullPath; } }

        // properties from StackHashCab

        /// <summary />
        public bool CabDownloaded { get { return _stackHashCabPackage.Cab.CabDownloaded; } }
        /// <summary />
        public DateTime DateCreatedLocal { get { return _stackHashCabPackage.Cab.DateCreatedLocal; } }
        /// <summary />
        public DateTime DateModifiedLocal { get { return _stackHashCabPackage.Cab.DateModifiedLocal; } }
        /// <summary />
        public int EventId { get { return _stackHashCabPackage.Cab.EventId; } }
        /// <summary />
        public string EventTypeName { get { return _stackHashCabPackage.Cab.EventTypeName; } }
        /// <summary />
        public string FileName { get { return _stackHashCabPackage.Cab.FileName; } }
        /// <summary />
        public int Id { get { return _stackHashCabPackage.Cab.Id; } }
        /// <summary />
        public bool Purged { get { return _stackHashCabPackage.Cab.Purged; } }
        /// <summary />
        public long SizeInBytes { get { return _stackHashCabPackage.Cab.SizeInBytes; } }

        // properties from StackHashDumpAnalysis

        /// <summary />
        public string DotNetVersion { get { return _stackHashCabPackage.Cab.DumpAnalysis.DotNetVersion; } }
        /// <summary />
        public string MachineArchitecture { get { return _stackHashCabPackage.Cab.DumpAnalysis.MachineArchitecture; } }
        /// <summary />
        public string OSVersion { get { return _stackHashCabPackage.Cab.DumpAnalysis.OSVersion; } }
        /// <summary />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UpTime")]
        public string ProcessUpTime { get { return _stackHashCabPackage.Cab.DumpAnalysis.ProcessUpTime; } }
        /// <summary />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UpTime")]
        public string SystemUpTime { get { return _stackHashCabPackage.Cab.DumpAnalysis.SystemUpTime; } }

        /// <summary>
        /// StackHashCabPackage wrapper class
        /// </summary>
        /// <param name="stackHashCabPackage">StackHashCabPackage</param>
        public DisplayCab(StackHashCabPackage stackHashCabPackage)
        {
            if (stackHashCabPackage == null) { throw new ArgumentNullException("stackHashCabPackage"); }
            if (stackHashCabPackage.Cab == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.Cab is null"); }
            if (stackHashCabPackage.Cab.DumpAnalysis == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.Cab.DumpAnalysis is null"); }
            if (stackHashCabPackage.CabFileContents == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.CabFileContents is null"); }
            if (stackHashCabPackage.CabFileContents.Files == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.CabFileContents.Files is null"); }

            _files = new ObservableCollection<DisplayCabFile>();

            _stackHashCabPackage = stackHashCabPackage;

            foreach (StackHashCabFile file in _stackHashCabPackage.CabFileContents.Files)
            {
                _files.Add(new DisplayCabFile(file));
            }
        }

        /// <summary>
        /// Updates the DisplayCab from a StackHashCabPackage
        /// </summary>
        /// <param name="stackHashCabPackage">StackHashCabPackage</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void UpdateCab(StackHashCabPackage stackHashCabPackage)
        {
            if (stackHashCabPackage == null) { throw new ArgumentNullException("stackHashCabPackage"); }
            if (stackHashCabPackage.Cab == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.Cab is null"); }
            if (stackHashCabPackage.Cab.DumpAnalysis == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.Cab.DumpAnalysis is null"); }
            if (stackHashCabPackage.CabFileContents == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.CabFileContents is null"); }
            if (stackHashCabPackage.CabFileContents.Files == null) { throw new ArgumentNullException("stackHashCabPackage", "stackHashCabPackage.CabFileContents.Files is null"); }
            if (_stackHashCabPackage.Cab.Id != stackHashCabPackage.Cab.Id) { throw new InvalidOperationException("Cannot update from a cab with a different ID"); }

            // save the new cab
            StackHashCabPackage oldCab = _stackHashCabPackage;
            _stackHashCabPackage = stackHashCabPackage;

            // check for updates
            if (oldCab.FullPath != _stackHashCabPackage.FullPath) { RaisePropertyChanged("FullPath"); }

            if (oldCab.Cab.CabDownloaded != _stackHashCabPackage.Cab.CabDownloaded) { RaisePropertyChanged("CabDownloaded"); }
            if (oldCab.Cab.DateCreatedLocal != _stackHashCabPackage.Cab.DateCreatedLocal) { RaisePropertyChanged("DateCreatedLocal"); }
            if (oldCab.Cab.DateModifiedLocal != _stackHashCabPackage.Cab.DateModifiedLocal) { RaisePropertyChanged("DateModifiedLocal"); }
            if (oldCab.Cab.EventId != _stackHashCabPackage.Cab.EventId) { RaisePropertyChanged("EventId"); }
            if (oldCab.Cab.EventTypeName != _stackHashCabPackage.Cab.EventTypeName) { RaisePropertyChanged("EventTypeName"); }
            if (oldCab.Cab.FileName != _stackHashCabPackage.Cab.FileName) { RaisePropertyChanged("FileName"); }
            if (oldCab.Cab.Purged != _stackHashCabPackage.Cab.Purged) { RaisePropertyChanged("Purged"); }
            if (oldCab.Cab.SizeInBytes != _stackHashCabPackage.Cab.SizeInBytes) { RaisePropertyChanged("SizeInBytes"); }

            if (oldCab.Cab.DumpAnalysis.DotNetVersion != _stackHashCabPackage.Cab.DumpAnalysis.DotNetVersion) { RaisePropertyChanged("DotNetVersion"); }
            if (oldCab.Cab.DumpAnalysis.MachineArchitecture != _stackHashCabPackage.Cab.DumpAnalysis.MachineArchitecture) { RaisePropertyChanged("MachineArchitecture"); }
            if (oldCab.Cab.DumpAnalysis.OSVersion != _stackHashCabPackage.Cab.DumpAnalysis.OSVersion) { RaisePropertyChanged("OSVersion"); }
            if (oldCab.Cab.DumpAnalysis.ProcessUpTime != _stackHashCabPackage.Cab.DumpAnalysis.ProcessUpTime) { RaisePropertyChanged("ProcessUpTime"); }
            if (oldCab.Cab.DumpAnalysis.SystemUpTime != _stackHashCabPackage.Cab.DumpAnalysis.SystemUpTime) { RaisePropertyChanged("SystemUpTime"); }

            // update files
            bool fileFound = false;

            foreach (StackHashCabFile file in _stackHashCabPackage.CabFileContents.Files)
            {
                fileFound = false;

                foreach (DisplayCabFile displayFile in _files)
                {
                    if (displayFile.FileName == file.FileName)
                    {
                        fileFound = true;
                        displayFile.UpdateCabFile(file);
                        break;
                    }
                }

                if (!fileFound)
                {
                    _files.Add(new DisplayCabFile(file));
                }
            }

            List<DisplayCabFile> displayFilesToRemove = new List<DisplayCabFile>();
            foreach (DisplayCabFile displayFile in _files)
            {
                fileFound = false;

                foreach (StackHashCabFile file in _stackHashCabPackage.CabFileContents.Files)
                {
                    if (displayFile.FileName == file.FileName)
                    {
                        fileFound = true;
                        break;
                    }
                }

                if (!fileFound)
                {
                    displayFilesToRemove.Add(displayFile);
                }
            }

            foreach (DisplayCabFile displayFile in displayFilesToRemove)
            {
                _files.Remove(displayFile);
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
