using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHash.StackHashService;
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// StackHashCabFile wrapper class
    /// </summary>
    public class DisplayCabFile : INotifyPropertyChanged
    {
        private StackHashCabFile _stackHashCabFile;

        /// <summary>
        /// Gets the underlying StackHashCabFile object
        /// </summary>
        public StackHashCabFile StackHashCabFile
        {
            get { return _stackHashCabFile; }
        }

        // Properties from StackHashCabFile

        /// <summary />
        public string FileName { get { return _stackHashCabFile.FileName; } }
        /// <summary />
        public long Length { get { return _stackHashCabFile.Length; } }

        /// <summary>
        /// StackHashCabFile wrapper class
        /// </summary>
        /// <param name="stackHashCabFile">The StackHashCabFile</param>
        public DisplayCabFile(StackHashCabFile stackHashCabFile)
        {
            if (stackHashCabFile == null) { throw new ArgumentNullException("stackHashCabFile"); }

            _stackHashCabFile = stackHashCabFile;
        }

        /// <summary>
        /// Updates the DisplayCabFile from a StackHashCabFile
        /// </summary>
        /// <param name="stackHashCabFile">StackHashCabFile</param>
        public void UpdateCabFile(StackHashCabFile stackHashCabFile)
        {
            if (stackHashCabFile == null) { throw new ArgumentNullException("stackHashCabFile"); }
            if (stackHashCabFile.FileName != _stackHashCabFile.FileName) { throw new InvalidOperationException("Cannot update from a cab file with a different FileName"); }

            // save the new cab file
            StackHashCabFile oldCabFile = _stackHashCabFile;
            _stackHashCabFile = stackHashCabFile;

            // check for updates
            if (oldCabFile.Length != _stackHashCabFile.Length) { RaisePropertyChanged("Length"); }
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
