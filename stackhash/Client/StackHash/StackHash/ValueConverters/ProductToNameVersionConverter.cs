using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a display product to the name and version as a string
    /// </summary>
    public class ProductToNameVersionConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            DisplayProduct product = value as DisplayProduct;
            if (product == null)
            {
                return string.Empty;
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "{0} {1}",
                    product.Name,
                    product.Version);
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
