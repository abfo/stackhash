using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts an IList to true if it has more than one item
    /// </summary>
    public class MoreThanOneConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("targetType must be bool");
            }

            IList list = value as IList;
            if (list == null)
            {
                return false;
            }

            return list.Count > 1;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
