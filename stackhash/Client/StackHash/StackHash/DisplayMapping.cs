using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using StackHash.StackHashService;

namespace StackHash
{
    /// <summary>
    /// StackHashMapping wrapper class
    /// </summary>
    public class DisplayMapping : INotifyPropertyChanged
    {
        private StackHashMapping _stackHashMapping;

        /// <summary>
        /// Gets the underlying StackHashMapping object
        /// </summary>
        public StackHashMapping StackHashMapping
        {
            get { return _stackHashMapping; }
        }

        // properties from StackHashMapping

        /// <summary />
        public int Id { get { return _stackHashMapping.Id; } }
        /// <summary />
        public StackHashMappingType MappingType { get { return _stackHashMapping.MappingType; } }
        /// <summary />
        public string Name { get { return _stackHashMapping.Name; } }

        /// <summary>
        /// StackHashMapping wrapper class
        /// </summary>
        /// <param name="stackHashMapping">Underlying StackHashMapping object</param>
        public DisplayMapping(StackHashMapping stackHashMapping)
        {
            if (stackHashMapping == null) { throw new ArgumentNullException("stackHashMapping"); }

            _stackHashMapping = stackHashMapping;
        }

        /// <summary>
        /// Updates the DisplayMapping from a StackHashMapping
        /// </summary>
        /// <param name="stackHashMapping">StackHashMapping</param>
        public void UpdateMapping(StackHashMapping stackHashMapping)
        {
            if (stackHashMapping == null) { throw new ArgumentNullException("stackHashMapping"); }
            if (_stackHashMapping.Id != stackHashMapping.Id) { throw new InvalidOperationException("Cannot update from a StackHashMapping with a different ID"); }
            if (_stackHashMapping.MappingType != stackHashMapping.MappingType) { throw new InvalidOperationException("Cannot update from a StackHashMapping with a different MappingType"); }

            // save the new mapping
            StackHashMapping oldMapping = _stackHashMapping;
            _stackHashMapping = stackHashMapping;

            // check for updates
            if (oldMapping.Name != _stackHashMapping.Name) { RaisePropertyChanged("Name"); }
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
