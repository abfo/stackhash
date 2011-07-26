using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Displays a long in hexadecimal format
    /// </summary>
    public class LongHexDisplayConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            long valueAsLong = (long)value;
            int padding = 0;
            if (!Int32.TryParse(parameter as string, out padding))
            {
                padding = 0;
            }

            if (valueAsLong == 0)
            {
                return string.Empty;
            }
            else
            {
                if (padding == 16)
                {
                    return string.Format(CultureInfo.CurrentCulture,
                            "0x{0:X16}",
                            valueAsLong);
                }
                else if (padding == 8)
                {
                    return string.Format(CultureInfo.CurrentCulture,
                            "0x{0:X8}",
                            valueAsLong);
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture,
                            "0x{0:X}",
                            valueAsLong);
                }
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
