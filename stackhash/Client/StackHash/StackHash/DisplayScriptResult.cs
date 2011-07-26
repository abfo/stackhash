using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using StackHash.StackHashService;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// StackHashScriptResultFile wrapper class
    /// </summary>
    public class DisplayScriptResult : INotifyPropertyChanged
    {
        private StackHashScriptResultFile _stackHashScriptResultFile;

        /// <summary>
        /// Gets the underlying StackHashScriptResultFile object
        /// </summary>
        public StackHashScriptResultFile StackHashScriptResultFile
        {
            get { return _stackHashScriptResultFile; }
        }

        // properties from StackHashScriptResultFile

        /// <summary />
        public DateTime RunDate { get { return _stackHashScriptResultFile.RunDate; } }
        /// <summary />
        public string ScriptName { get { return _stackHashScriptResultFile.ScriptName; } }
        /// <summary />
        public string UserName { get { return _stackHashScriptResultFile.UserName; } }

        /// <summary>
        /// StackHashScriptResultFile wrapper class
        /// </summary>
        /// <param name="stackHashScriptResultFile">StackHashScriptResultFile</param>
        public DisplayScriptResult(StackHashScriptResultFile stackHashScriptResultFile)
        {
            if (stackHashScriptResultFile == null) { throw new ArgumentNullException("stackHashScriptResultFile"); }

            _stackHashScriptResultFile = stackHashScriptResultFile;
        }

        /// <summary>
        /// Updates the DisplayScriptResult from a StackHashScriptResultFile
        /// </summary>
        /// <param name="stackHashScriptResultFile">StackHashScriptResultFile</param>
        public void UpdateScriptResult(StackHashScriptResultFile stackHashScriptResultFile)
        {
            if (stackHashScriptResultFile == null) { throw new ArgumentNullException("stackHashScriptResultFile"); }
            if (stackHashScriptResultFile.ScriptName != _stackHashScriptResultFile.ScriptName) { throw new InvalidOperationException("Cannot update from a script result with a different script name"); }

            // save the old result
            StackHashScriptResultFile oldResult = _stackHashScriptResultFile;
            _stackHashScriptResultFile = stackHashScriptResultFile;

            // check for updates
            if (oldResult.UserName != _stackHashScriptResultFile.UserName) { RaisePropertyChanged("UserName"); }
            if (oldResult.RunDate != _stackHashScriptResultFile.RunDate) { RaisePropertyChanged("RunDate"); }
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
