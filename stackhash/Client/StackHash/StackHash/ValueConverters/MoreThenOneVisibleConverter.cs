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
    /// Converts an IList to Visibility.Visible if it has more than one item, Visibility.Collapsed otherwise
    /// </summary>
    public class MoreThenOneVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("targetType must be Visibility");
            }

            Visibility ret = Visibility.Collapsed;

            IList list = value as IList;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    ret = Visibility.Visible;
                }
            }

            return ret;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
