﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace StackHash.ValueConverters
{
    /// <summary>
    /// Returns Visibility.Visible for ClientLogicView.EventList, otherwise Visibility.Hidden
    /// </summary>
    public class EventListConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("targetType must be Visibility");
            }

            ClientLogicView view = (ClientLogicView)value;

            return view == ClientLogicView.EventList ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
