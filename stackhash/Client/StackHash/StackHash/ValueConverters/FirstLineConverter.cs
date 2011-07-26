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
    /// Converts a string to the first line of text in the string
    /// </summary>
    public class FirstLineConverter : IValueConverter
    {
        private static readonly char[] LineSeps = new char[] { '\r', '\n' };

        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            string valueAsString = value as string;
            string firstLine = string.Empty;

            if (value != null)
            {
                string[] lines = valueAsString.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    firstLine = lines[0];
                }
            }

            return firstLine;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
