using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Returns true if a string is not null or empty
    /// </summary>
    public class StringNotNullOrEmptyConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("targetType must be bool");
            }

            string valueAsString = value as string;

            return !string.IsNullOrEmpty(valueAsString);
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
