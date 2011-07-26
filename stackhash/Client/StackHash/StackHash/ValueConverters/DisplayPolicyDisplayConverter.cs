using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a hit threshold to a display policy string
    /// </summary>
    public class DisplayPolicyDisplayConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            string displayString = string.Empty;

            int hitThreshold = (int)value;
            if (hitThreshold == 0)
            {
                displayString = Properties.Resources.EventDisplayPolicy_All;
            }
            else if (hitThreshold == 1)
            {
                displayString = Properties.Resources.EventDisplayPolicy_One;
            }
            else
            {
                displayString = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.EventDisplayPolicy_Many,
                    hitThreshold);
            }

            return displayString;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
