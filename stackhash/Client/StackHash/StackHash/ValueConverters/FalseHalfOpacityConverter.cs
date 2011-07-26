using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts 'False' to half opacity (0.5), fully visible for 'True' (1.0)
    /// </summary>
    public class FalseHalfOpacityConverter : IValueConverter
    {
        #region IValueConverter Members: IValueConverter

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("targetType must be double");
            }

            return ((bool)value) ? 1.0 : 0.5;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
