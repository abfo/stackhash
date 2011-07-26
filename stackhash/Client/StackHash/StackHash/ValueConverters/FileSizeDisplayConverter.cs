using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Converts a filesize in bytes to an appropriate KB, MB, or GB display string
    /// </summary>
    public class FileSizeDisplayConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("targetType must be string");
            }

            double size = (double)((long)value);
            Debug.Assert(size >= 0);

            // default is KB
            size /= 1024;
            string postfix = Properties.Resources.KB;

            // MB
            if (size > 1024)
            {
                size /= 1024;
                postfix = Properties.Resources.MB;
            }

            // GB
            if (size > 1024)
            {
                size /= 1024;
                postfix = Properties.Resources.GB;
            }

            return string.Format(CultureInfo.CurrentCulture,
                "{0:n2} {1}",
                size,
                postfix);
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
