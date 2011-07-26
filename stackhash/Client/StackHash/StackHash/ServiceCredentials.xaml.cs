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

namespace StackHash
{
    /// <summary>
    /// Allows user to enter credentials for accessing the StackHash Service
    /// </summary>
    public partial class ServiceCredentials : Window
    {
        private const string WindowKey = "ServiceCredentials";

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Allows user to enter credentials for accessing the StackHash Service
        /// </summary>
        public ServiceCredentials()
        {
            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxUsername.Text = this.Username ?? string.Empty;
            passwordBoxPassword.Password = this.Password ?? string.Empty;
            textBoxDomain.Text = this.Domain ?? string.Empty;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Username = textBoxUsername.Text;
            this.Password = passwordBoxPassword.Password;
            this.Domain = textBoxDomain.Text;

            DialogResult = true;
            Close();
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("service-credentials.htm");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
