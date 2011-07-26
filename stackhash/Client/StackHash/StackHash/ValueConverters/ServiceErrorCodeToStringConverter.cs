using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHash.StackHashService;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a StackHashServiceErrorCode to a string
    /// </summary>
    public class ServiceErrorCodeToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            return StackHashMessageBox.GetServiceErrorCodeMessage((StackHashServiceErrorCode)value);
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
