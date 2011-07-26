using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Formats a UTC DateTime for display as a local date string
    /// </summary>
    public class DateTimeDisplayConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            // all service dates are UTC, convert to local for display
            DateTime date = ((DateTime)value).ToLocalTime();

            if (date > DateTime.MinValue)
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "{0} {1:HH:mm}",
                    date.ToShortDateString(),
                    date);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
