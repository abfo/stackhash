using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using StackHash.StackHashService;
using System.Globalization;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a DisplayCollectionPolicy for event collection to a string
    /// </summary>
    public class EventCollectionPolicyDisplayConverter : IValueConverter
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

            DisplayCollectionPolicy eventCollectionPolicy = value as DisplayCollectionPolicy;
            if (eventCollectionPolicy != null)
            {
                if (eventCollectionPolicy.Minimum == 0)
                {
                    displayString = Properties.Resources.EventCollectionPolicy_All;
                }
                else if (eventCollectionPolicy.Minimum == 1)
                {
                    displayString = Properties.Resources.EventCollectionPolicy_AtLeastOne;
                }
                else
                {
                    displayString = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.EventCollectionPolicy_AtLeastX,
                        eventCollectionPolicy.Minimum);
                }
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
