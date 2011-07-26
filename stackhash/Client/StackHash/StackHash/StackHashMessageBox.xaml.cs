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
using StackHash.StackHashService;
using System.Globalization;
using StackHashUtilities;
using System.Text.RegularExpressions;
using System.ServiceModel;

namespace StackHash
{
    /// <summary>
    /// The result from a StackHashMessageBox
    /// </summary>
    public enum StackHashDialogResult
    {
        /// <summary>
        /// OK
        /// </summary>
        Ok,

        /// <summary>
        /// Cancel
        /// </summary>
        Cancel,

        /// <summary>
        /// Yes
        /// </summary>
        Yes,

        /// <summary>
        /// No
        /// </summary>
        No,

        /// <summary>
        /// Retry
        /// </summary>
        Retry,

        /// <summary>
        /// Exit
        /// </summary>
        Exit,

        /// <summary>
        /// Open Folder
        /// </summary>
        OpenFolder
    }

    /// <summary>
    /// The type of message box to show
    /// </summary>
    public enum StackHashMessageBoxType
    {
        /// <summary>
        /// OK
        /// </summary>
        Ok,

        /// <summary>
        /// OK or Cancel
        /// </summary>
        OkCancel,

        /// <summary>
        /// Yes or No
        /// </summary>
        YesNo,

        /// <summary>
        /// Retry or Exit
        /// </summary>
        RetryExit,

        /// <summary>
        /// Retry or Cancel
        /// </summary>
        RetryCancel,

        /// <summary>
        /// OK or Open Folder
        /// </summary>
        OkOpenFolder,
    }

    /// <summary>
    /// The icon to show
    /// </summary>
    public enum StackHashMessageBoxIcon
    {
        /// <summary>
        /// Information
        /// </summary>
        Information,

        /// <summary>
        /// Question
        /// </summary>
        Question,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    /// <summary>
    /// Interaction logic for StackHashMessageBox.xaml
    /// </summary>
    public partial class StackHashMessageBox : Window
    {
        /// <summary>
        /// Gets the result of the message box
        /// </summary>
        public StackHashDialogResult StackHashDialogResult { get; private set; }

        /// <summary>
        /// Gets the value of the 'don't show this message again' checkbox
        /// </summary>
        public bool DontShowAgain { get; private set; }

        private string _message;
        private string _caption;
        private StackHashMessageBoxType _type;
        private StackHashMessageBoxIcon _icon;
        private Exception _exception;
        private StackHashServiceErrorCode _serviceError;
        private bool _showDontShow;

        /// <summary>
        /// Attempts to parse a StackHashServiceErrorCode from an exception
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>A StackHashServiceErrorCode or NoError</returns>
        public static StackHashServiceErrorCode ParseServiceErrorFromException(Exception ex)
        {
            StackHashServiceErrorCode serviceError = StackHashServiceErrorCode.NoError;

            if (ex != null)
            {
                // try to parse out a service error code
                FaultException<ReceiverFaultDetail> faultException = ex as FaultException<ReceiverFaultDetail>;
                if (faultException != null)
                {
                    serviceError = faultException.Detail.ServiceErrorCode;
                }

                if (ex.Message.Contains("OperationCanceledException"))
                {
                    serviceError = StackHashServiceErrorCode.Aborted;
                }
            }

            return serviceError;
        }

        /// <summary>
        /// Shows a StackHashMessageBox and returns the result
        /// </summary>
        /// <param name="owner">Window that owns the message box</param>
        /// <param name="message">The message</param>
        /// <param name="caption">The caption</param>
        /// <param name="type">The type of message box to display</param>
        /// <param name="icon">The icon to display</param>
        /// <returns>The result of the messsage box</returns>
        public static StackHashDialogResult Show(Window owner, string message, string caption, StackHashMessageBoxType type, StackHashMessageBoxIcon icon)
        {
            bool unused;
            return Show(owner, message, caption, type, icon, null, StackHashServiceErrorCode.NoError, false, out unused);
        }

        /// <summary>
        /// Shows a StackHashMessageBox and returns the result
        /// </summary>
        /// <param name="owner">Window that owns the message box</param>
        /// <param name="message">The message</param>
        /// <param name="caption">The caption</param>
        /// <param name="type">The type of message box to display</param>
        /// <param name="icon">The icon to display</param>
        /// <param name="showDontShow">Show the don't show this message again checkbox</param>
        /// <param name="dontShow">Returns the value of the don't show this message again checkbox</param>
        /// <returns>The result of the messsage box</returns>
        public static StackHashDialogResult Show(Window owner, string message, string caption, StackHashMessageBoxType type, StackHashMessageBoxIcon icon, bool showDontShow, out bool dontShow)
        {
            return Show(owner, message, caption, type, icon, null, StackHashServiceErrorCode.NoError, showDontShow, out dontShow);
        }

        /// <summary>
        /// Shows a StackHashMessageBox and returns the result
        /// </summary>
        /// <param name="owner">Window that owns the message box</param>
        /// <param name="message">The message</param>
        /// <param name="caption">The caption</param>
        /// <param name="type">The type of message box to display</param>
        /// <param name="icon">The icon to display</param>
        /// <param name="ex">Exception associated with this message</param>
        /// <param name="serviceError">Service error associates with this message (NoError if none)</param>
        /// <returns>The result of the messsage box</returns>
        public static StackHashDialogResult Show(Window owner,
            string message,
            string caption,
            StackHashMessageBoxType type,
            StackHashMessageBoxIcon icon,
            Exception ex,
            StackHashServiceErrorCode serviceError)
        {
            bool unused;
            return Show(owner, message, caption, type, icon, ex, serviceError, false, out unused);
        }

        /// <summary>
        /// Shows a StackHashMessageBox and returns the result
        /// </summary>
        /// <param name="owner">Window that owns the message box</param>
        /// <param name="message">The message</param>
        /// <param name="caption">The caption</param>
        /// <param name="type">The type of message box to display</param>
        /// <param name="icon">The icon to display</param>
        /// <param name="ex">Exception associated with this message</param>
        /// <param name="serviceError">Service error associates with this message (NoError if none)</param>
        /// <param name="showDontShow">Show the don't show this message again checkbox</param>
        /// <param name="dontShow">Returns the value of the don't show this message again checkbox</param>
        /// <returns>The result of the messsage box</returns>
        public static StackHashDialogResult Show(Window owner, 
            string message, 
            string caption, 
            StackHashMessageBoxType type, 
            StackHashMessageBoxIcon icon, 
            Exception ex, 
            StackHashServiceErrorCode serviceError,
            bool showDontShow,
            out bool dontShow)
        {
            // ok for owner and ex to be null
            if (string.IsNullOrEmpty(message)) { throw new ArgumentException("message cannot be null or an empty string"); }
            if (string.IsNullOrEmpty(caption)) { throw new ArgumentException("caption cannot be null or an empty string"); }

            StackHashMessageBox stackHashMessageBox = new StackHashMessageBox(message, caption, type, icon, ex, serviceError, showDontShow);

            // if no owner try the main window
            if (owner == null)
            {
                if (Application.Current != null)
                {
                    if (Application.Current.MainWindow != stackHashMessageBox)
                    {
                        owner = Application.Current.MainWindow;
                    }
                }
            }

            // if still no owner
            if (owner == null)
            {
                stackHashMessageBox.ShowInTaskbar = true;
                stackHashMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            stackHashMessageBox.Owner = owner;
            stackHashMessageBox.ShowDialog();

            dontShow = stackHashMessageBox.DontShowAgain;

            return stackHashMessageBox.StackHashDialogResult;
        }

        private StackHashMessageBox(string message, string caption, StackHashMessageBoxType type, StackHashMessageBoxIcon icon, Exception ex, StackHashServiceErrorCode serviceError, bool showDontShow)
        {
            InitializeComponent();

            _message = message;
            _caption = caption;
            _type = type;
            _icon = icon;
            _exception = ex;
            _serviceError = serviceError;
            _showDontShow = showDontShow;

            // default to cancel unless we explictly set a different return value
            this.StackHashDialogResult = StackHashDialogResult.Cancel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // set title and message
            this.Title = _caption;
            textMessage.Text = AddServiceErrorToMessage(_message, _serviceError);

            labelException.Visibility = Visibility.Collapsed;
            textBoxException.Visibility = Visibility.Collapsed;

            if (_showDontShow)
            {
                checkBoxDontShowAgain.Visibility = Visibility.Visible;
            }
            else
            {
                checkBoxDontShowAgain.Visibility = Visibility.Collapsed;
            }

            // set exception
            if (_exception == null)
            {
                textBlockShowDetail.Visibility = Visibility.Collapsed;
            }
            else
            {
                textBoxException.Text = string.Format(CultureInfo.CurrentCulture,
                    "{0}\r\n\r\n{1}\r\n\r\nService Error Code: {2}",
                    DiagnosticsHelper.GetExceptionText(_exception),
                    DiagnosticsHelper.EnvironmentInfo,
                    _serviceError);
            }

            // set icon
            string relativePath;
            switch (_icon)
            {
                case StackHashMessageBoxIcon.Information:
                default:
                    relativePath = @"Help\information32.png";

                    System.Media.SystemSounds.Asterisk.Play();
                    break;

                case StackHashMessageBoxIcon.Question:
                    relativePath = @"Help\question32.png";

                    System.Media.SystemSounds.Question.Play();
                    break;

                case StackHashMessageBoxIcon.Warning:
                    relativePath = @"Help\warning32.png";

                    System.Media.SystemSounds.Exclamation.Play();
                    break;

                case StackHashMessageBoxIcon.Error:
                    relativePath = @"Help\error32.png";

                    System.Media.SystemSounds.Hand.Play();
                    break;
            }
            imageIcon.Source = new BitmapImage(new Uri(relativePath, UriKind.Relative));

            // set buttons
            switch (_type)
            {
                case StackHashMessageBoxType.Ok:
                    button1.Content = Properties.Resources.Ok;
                    button1.IsDefault = true;
                    button2.Visibility = Visibility.Collapsed;
                    break;

                case StackHashMessageBoxType.OkCancel:
                    button1.Content = Properties.Resources.Cancel;
                    button1.IsCancel = true;
                    button2.Content = Properties.Resources.Ok;
                    button2.IsDefault = true;
                    break;

                case StackHashMessageBoxType.OkOpenFolder:
                    button1.Content = Properties.Resources.Ok;
                    button1.IsDefault = true;
                    button2.Content = Properties.Resources.OpenFolder;
                    button2.Width = 110;
                    break;

                case StackHashMessageBoxType.YesNo:
                    button1.Content = Properties.Resources.No;
                    button1.IsCancel = true;
                    button2.Content = Properties.Resources.Yes;
                    button2.IsDefault = true;
                    break;

                case StackHashMessageBoxType.RetryExit:
                    button1.Content = Properties.Resources.Exit;
                    button1.IsCancel = true;
                    button2.Content = Properties.Resources.Retry;
                    button2.IsDefault = true;
                    break;

                case StackHashMessageBoxType.RetryCancel:
                    button1.Content = Properties.Resources.Cancel;
                    button1.IsCancel = true;
                    button2.Content = Properties.Resources.Retry;
                    button2.IsDefault = true;
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch (_type)
            {
                case StackHashMessageBoxType.Ok:
                    Debug.Assert(false, "button2 should not be visible");
                    this.StackHashDialogResult = StackHashDialogResult.Ok;
                    break;

                case StackHashMessageBoxType.OkCancel:
                    this.StackHashDialogResult = StackHashDialogResult.Ok;
                    break;

                case StackHashMessageBoxType.OkOpenFolder:
                    this.StackHashDialogResult = StackHash.StackHashDialogResult.OpenFolder;
                    break;

                case StackHashMessageBoxType.YesNo:
                    this.StackHashDialogResult = StackHashDialogResult.Yes;
                    break;

                case StackHashMessageBoxType.RetryExit:
                case StackHashMessageBoxType.RetryCancel:
                    this.StackHashDialogResult = StackHashDialogResult.Retry;
                    break;

            }

            Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (_type)
            {
                case StackHashMessageBoxType.Ok:
                case StackHashMessageBoxType.OkOpenFolder:
                    this.StackHashDialogResult = StackHashDialogResult.Ok;
                    break;

                case StackHashMessageBoxType.OkCancel:
                    this.StackHashDialogResult = StackHashDialogResult.Cancel;
                    break;

                case StackHashMessageBoxType.YesNo:
                    this.StackHashDialogResult = StackHashDialogResult.No;
                    break;

                case StackHashMessageBoxType.RetryExit:
                    this.StackHashDialogResult = StackHashDialogResult.Exit;
                    break;

                case StackHashMessageBoxType.RetryCancel:
                    this.StackHashDialogResult = StackHashDialogResult.Cancel;
                    break;
            }

            Close();
        }

        private void hyperlinkShowDetail_Click(object sender, RoutedEventArgs e)
        {
            textBlockShowDetail.Visibility = Visibility.Collapsed;

            labelException.Visibility = Visibility.Visible;
            textBoxException.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Gets the localized string associated with a StackHashServiceErrorCode
        /// See file://R:\StackHash\BusinessLogic\BusinessLogic\BusinessObjects\ErrorCodes.cs
        /// Last Updated: 2011-02-11
        /// </summary>
        /// <param name="serviceError">The StackHashServiceErrorCode</param>
        /// <returns>Localized string</returns>
        public static string GetServiceErrorCodeMessage(StackHashServiceErrorCode serviceError)
        {
            string serviceErrorMessage = null;

            if (serviceError != StackHashServiceErrorCode.NoError)
            {
                string resourceKey = string.Format(CultureInfo.InvariantCulture,
                    "ServiceError_{0}",
                    serviceError);

                try
                {
                    serviceErrorMessage = Properties.Resources.ResourceManager.GetString(resourceKey);
                }
                catch (Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "GetServiceErrorCodeMessage Failed",
                        ex);
                }

                if (serviceErrorMessage == null)
                {
                    Debug.Assert(false,
                        string.Format(CultureInfo.InvariantCulture,
                        "Missing localization for StackHashServiceErrorCode: {0}",
                        serviceError));

                    serviceErrorMessage = Properties.Resources.ServiceError_UnexpectedError;
                }
            }
            else
            {
                serviceErrorMessage = string.Empty;
            }

            return serviceErrorMessage.Trim();
        }

        /// <summary>
        /// Gets the localized string associated with a StackHashErrorIndexDatabaseStatus
        /// </summary>
        /// <param name="databaseStatus">The StackHashErrorIndexDatabaseStatus</param>
        /// <returns>Localized string</returns>
        public static string GetDatabaseStatusMessage(StackHashErrorIndexDatabaseStatus databaseStatus)
        {
            string statusMessage = null;

            string resourceKey = string.Format(CultureInfo.InvariantCulture,
                "DatabaseStatus_{0}",
                databaseStatus);

            try
            {
                statusMessage = Properties.Resources.ResourceManager.GetString(resourceKey);
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning,
                    "GetDatabaseStatusMessage Failed",
                    ex);
            }

            if (statusMessage == null)
            {
                Debug.Assert(false,
                    string.Format(CultureInfo.InvariantCulture,
                    "Missing localization for StackHashErrorIndexDatabaseStatus: {0}",
                    databaseStatus));

                statusMessage = Properties.Resources.DatabaseStatus_Unknown;
            }

            return statusMessage;
        }

        private static string AddServiceErrorToMessage(string message, StackHashServiceErrorCode serviceError)
        {
            if (serviceError == StackHashServiceErrorCode.NoError)
            {
                return message;
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "{0}\r\n\r\n({1})",
                    message,
                    GetServiceErrorCodeMessage(serviceError));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DontShowAgain = checkBoxDontShowAgain.IsChecked == true;
        }
    }
}
