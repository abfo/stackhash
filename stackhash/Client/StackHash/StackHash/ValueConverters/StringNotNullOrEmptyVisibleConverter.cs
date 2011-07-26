using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using System.Windows;


namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a string to Visibility.Collapsed if it's null or empty (Visibility.Visible otherwise)
    /// </summary>
    public class StringNotNullOrEmptyVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("targetType must be Visibility");
            }

            string valueAsString = value as string;

            return string.IsNullOrEmpty(valueAsString) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
