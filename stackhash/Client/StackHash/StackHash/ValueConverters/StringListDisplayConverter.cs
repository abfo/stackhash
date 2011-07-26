using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Formats a list of strings as a single string with linebreaks
    /// </summary>
    public class StringListDisplayConverter : IValueConverter
    {
         #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            List<string> stringList = value as List<string>;

            if (stringList == null)
            {
                throw new InvalidOperationException("value must be List<string>");
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stringList.Count; i++)
            {
                sb.AppendLine(stringList[i]);
            }
            return sb.ToString();
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
