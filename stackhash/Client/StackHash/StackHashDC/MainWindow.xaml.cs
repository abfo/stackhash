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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.Globalization;

namespace StackHashDC
{
    /// <summary>
    /// Window informing the user that dependencies need to be installed
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string _defaultBrowserPath;

        /// <summary>
        /// Window informing the user that dependencies need to be installed
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("http://www.microsoft.com/downloads/en/details.aspx?FamilyID=9BA199EF-3086-4F12-970D-8745BE104600&displaylang=en");
        }

        private static void OpenUrl(string url)
        {
            if ((string.IsNullOrEmpty(url)) || (!Uri.IsWellFormedUriString(url, UriKind.Absolute)))
            {
                return;
            }

            try
            {
                // find the browser if necessary
                if (_defaultBrowserPath == null)
                {
                    SetBrowserPath();
                }

                if (_defaultBrowserPath == null)
                {
                    // can't find the default browser, try to launch without
                    Process.Start(url);
                }
                else
                {
                    // launch using the default browser
                    Process.Start(_defaultBrowserPath, url);
                }
            }
            catch
            {

            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void SetBrowserPath()
        {
            RegistryKey userDefault = null;
            RegistryKey systemDefault = null;
            RegistryKey browserPath = null;

            try
            {
                string defaultKey = null;

                // if the user has set a default browser it's in HKCU
                userDefault = Registry.CurrentUser.OpenSubKey(@"Software\Clients\StartMenuInternet");
                if (userDefault != null)
                {
                    defaultKey = userDefault.GetValue(string.Empty) as string;
                }
                else
                {
                    // if not then we need to look in HKLM
                    systemDefault = Registry.LocalMachine.OpenSubKey(@"Software\Clients\StartMenuInternet");
                    if (systemDefault != null)
                    {
                        defaultKey = systemDefault.GetValue(string.Empty) as string;
                    }
                }

                //  use the value form HKCU or HKLM to get the path to the browser
                if (defaultKey != null)
                {
                    string browserPathKey = string.Format(CultureInfo.InvariantCulture,
                        @"Software\Clients\StartMenuInternet\{0}\shell\open\command",
                        defaultKey);

                    browserPath = Registry.LocalMachine.OpenSubKey(browserPathKey);
                    if (browserPath != null)
                    {
                        _defaultBrowserPath = browserPath.GetValue(string.Empty) as string;
                        if (_defaultBrowserPath != null)
                        {
                            // remove any quotes from the path
                            _defaultBrowserPath = _defaultBrowserPath.Trim(new char[] { '"' });
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (userDefault != null)
                {
                    userDefault.Close();
                    userDefault = null;
                }

                if (systemDefault != null)
                {
                    systemDefault.Close();
                    systemDefault = null;
                }

                if (browserPath != null)
                {
                    browserPath.Close();
                    browserPath = null;
                }
            }
        }
    }
}
