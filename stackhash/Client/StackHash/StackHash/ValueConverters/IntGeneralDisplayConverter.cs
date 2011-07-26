using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Genereral display format for an int
    /// </summary>
    public class IntGeneralDisplayConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            int valueAsInt = (int)value;

            return string.Format(CultureInfo.CurrentCulture,
                "{0:n0}",
                valueAsInt);
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
