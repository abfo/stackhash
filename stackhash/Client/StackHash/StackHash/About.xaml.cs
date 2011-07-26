using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Globalization;
using System.Timers;
using StackHash.StackHashService;
using System.Collections.ObjectModel;

namespace StackHash
{
    /// <summary>
    /// About StackHash window
    /// </summary>
    public partial class About : Window
    {
        private const string WindowKey = "About";

        /// <summary>
        /// About StackHash window
        /// </summary>
        public About()
        {
            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            if (link != null)
            {
                DefaultBrowser.OpenUrl(link.NavigateUri.ToString());
            }
        }
    }
}
