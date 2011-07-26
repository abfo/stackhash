using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a bool to Visibility.Visible if true, Visibility.Collapsed otherwise
    /// </summary>
    public class TrueNotCollapsedConverter : IValueConverter
    {
        #region IValueConverter Members: IValueConverter

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("targetType must be Visibility");
            }

            bool valueAsBool = (bool)value;

            return valueAsBool ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
