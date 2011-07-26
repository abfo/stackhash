using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a bool to true if the bool is false
    /// </summary>
    public class NotConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("targetType must be bool");
            }

            bool valueAsBool = (bool)value;

            return !valueAsBool;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
