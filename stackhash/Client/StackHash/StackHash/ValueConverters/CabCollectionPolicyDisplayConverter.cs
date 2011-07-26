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
    /// Converts a DisplayCollectionPolicy for cab collection to a string
    /// </summary>
    public class CabCollectionPolicyDisplayConverter : IValueConverter
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

            DisplayCollectionPolicy cabCollectionPolicy = value as DisplayCollectionPolicy;
            if (cabCollectionPolicy != null)
            {
                // cab collection can be none, all, a percentage or a number
                switch (cabCollectionPolicy.CollectionType)
                {
                    case StackHashCollectionType.All:
                        displayString = Properties.Resources.All;
                        break;

                    case StackHashCollectionType.None:
                        displayString = Properties.Resources.None;
                        break;

                    case StackHashCollectionType.Percentage:
                        displayString = string.Format(CultureInfo.CurrentCulture, 
                            "{0}% ({1}{2})", 
                            cabCollectionPolicy.Percentage, 
                            GetOrderString(cabCollectionPolicy.CollectionOrder),
                            cabCollectionPolicy.CollectLarger ? Properties.Resources.Collection_CollectLarger : string.Empty);
                        break;

                    case StackHashCollectionType.Count:
                        displayString = string.Format(CultureInfo.CurrentCulture, 
                            "{0:n0} ({1}{2})", 
                            cabCollectionPolicy.Maximum, 
                            GetOrderString(cabCollectionPolicy.CollectionOrder),
                            cabCollectionPolicy.CollectLarger ? Properties.Resources.Collection_CollectLarger : string.Empty);
                        break;
                }
            }

            return displayString;
        }

        private static string GetOrderString(StackHashCollectionOrder order)
        {
            switch (order)
            {
                case StackHashCollectionOrder.AsReceived:
                    return Properties.Resources.CollectionOrderShort_AsReceived;

                case StackHashCollectionOrder.LargestFirst:
                    return Properties.Resources.CollectionOrderShort_LargestFirst;

                case StackHashCollectionOrder.MostRecentFirst:
                    return Properties.Resources.CollectionOrderShort_MostRecentFirst;

                case StackHashCollectionOrder.OldestFirst:
                    return Properties.Resources.CollectionOrderShort_OldestFirst;

                case StackHashCollectionOrder.SmallestFirst:
                    return Properties.Resources.CollectionOrderShort_SmallestFirst;

                default:
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
