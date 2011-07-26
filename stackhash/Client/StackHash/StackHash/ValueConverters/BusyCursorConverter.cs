using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a bool value which is true if ClientLogic is not busy to a busy cursor when not busy is false
    /// </summary>
    public class BusyCursorConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Cursor))
            {
                throw new InvalidOperationException("targetType must be Cursor");
            }

            bool notBusy = (bool)value;

            return notBusy ? null : Cursors.Wait;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
