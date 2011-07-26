using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Attempts to return a message for an exception code
    /// </summary>
    public class ExceptionMessageConverter : IValueConverter
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

            if (valueAsLong == 0)
            {
                return string.Empty;
            }
            else
            {
                return ExceptionMessageHelper.Helper.GetMessage((uint)valueAsLong);
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
