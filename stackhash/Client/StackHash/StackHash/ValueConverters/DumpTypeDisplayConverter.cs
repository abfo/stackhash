using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using StackHash.StackHashService;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a StackHashScriptDumpType to a display string
    /// </summary>
    public class DumpTypeDisplayConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            string displayString = string.Empty;

            StackHashScriptDumpType dumpType = (StackHashScriptDumpType)value;
            switch (dumpType)
            {
                case StackHashScriptDumpType.UnmanagedOnly:
                    displayString = Properties.Resources.DumpType_Native;
                    break;

                case StackHashScriptDumpType.ManagedOnly:
                    displayString = Properties.Resources.DumpType_Managed;
                    break;

                case StackHashScriptDumpType.UnmanagedAndManaged:
                    displayString = Properties.Resources.DumpType_All;
                    break;
            }

            return displayString;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
