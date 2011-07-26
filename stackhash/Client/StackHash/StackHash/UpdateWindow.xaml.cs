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
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Informs the user that an update is available
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private const string WindowKey = "UpdateWindow";

        /// <summary>
        /// Informs the user that an update is available
        /// </summary>
        /// <param name="updateUrl">URL with information about the update</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public UpdateWindow(string updateUrl)
        {
            Debug.Assert(!string.IsNullOrEmpty(updateUrl));

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            linkUpdateUrl.NavigateUri = new Uri(updateUrl);
            linkUpdateUrl.ToolTip = updateUrl;
        }

        private void linkUpdateUrl_Click(object sender, RoutedEventArgs e)
        {
            DefaultBrowser.OpenUrl(linkUpdateUrl.NavigateUri.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
