using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Threading;
using StackHashUtilities;
using System.Globalization;
using Microsoft.Shell;
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string UnhandledExceptionTitle = "Fatal Error - StackHash";
        private const string UnhandledExceptionMessage = "A fatal error has occurred and StackHash will now exit.";

        private Cleanup _cleanup;

        /// <summary>
        /// Gets or sets the current StackHashUri navigation request
        /// </summary>
        public StackHashUri CurrentStackHashUri { get; set; }

        /// <summary>
        /// Event fired when a new CurrentStackHashUri is available for navigation
        /// </summary>
        public event EventHandler StackHashUriNavigationRequest;       

        /// <summary />
        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance("{E17EF702-0CDB-4CC2-808A-EE33BAA03B34}"))
            {
                var application = new App();

                // parse requested Uri if present
                StackHashUri stackHashUri = null;
                if (StackHashUri.TryParse(Environment.GetCommandLineArgs(), out stackHashUri))
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format(CultureInfo.CurrentCulture,
                        "Command line requests navigation to {0}",
                        stackHashUri.RawUri));

                    application.CurrentStackHashUri = stackHashUri;
                }

                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        /// <summary />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // load user settings
            UserSettings.Settings.Load();

            DiagnosticsHelper.LogApplicationStartup();

            // grab a copy of the cleanup object and attempt to delete any files
            _cleanup = Cleanup.List;
            _cleanup.DeleteAll();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DisplayUnhandledExceptionAndDie(e.ExceptionObject as Exception);
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DisplayUnhandledExceptionAndDie(e.Exception);
            e.Handled = true;
        }

        private static void DisplayUnhandledExceptionAndDie(Exception ex)
        {
            DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal,
                "Unhandled Exception",
                ex);

            try
            {
                Window owner = null;
                if (Application.Current != null)
                {
                    owner = Application.Current.MainWindow;
                }

                StackHashMessageBox.Show(owner,
                    UnhandledExceptionMessage,
                    UnhandledExceptionTitle,
                    StackHashMessageBoxType.Ok,
                    StackHashMessageBoxIcon.Error,
                    ex,
                    StackHashService.StackHashServiceErrorCode.NoError);
            }
            catch (XamlParseException xex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal,
                    "XamlParseException displaying fatal error message",
                    xex);

                try
                {
                    // this will happen if the XAML window can't be created for some reason -
                    // try showing a regular message box in this case
                    MessageBox.Show(UnhandledExceptionMessage,
                        UnhandledExceptionTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Hand);
                }
                catch { }
            }
            catch { }
            finally
            {
                try
                {
                    if (App.Current != null)
                    {
                        App.Current.Shutdown(1);
                    }
                }
                catch { }
            }
        }

        /// <summary />
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                string.Format(CultureInfo.InvariantCulture,
                "Session Ending: {0}",
                e.ReasonSessionEnding));
        }

        /// <summary />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                base.OnExit(e);

                // close any open services on exit
                ServiceProxy.Services.Dispose();

                // dispose the ExceptionMessageHelper
                ExceptionMessageHelper.Helper.Dispose();

                // save and dispose user settings
                UserSettings.Settings.Save();
                UserSettings.Settings.Dispose();

                // attempt to delete any files on the cleanup list
                if (_cleanup != null)
                {
                    _cleanup.DeleteAll();
                }

                Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.ApplicationFatal,
                    "App.OnExit Failed",
                    ex);
            }
            finally
            {
                DiagnosticsHelper.LogAppExit(e.ApplicationExitCode);
            }
        }

        #region ISingleInstanceApp Members

        /// <summary />
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                "Other instance activation...");

            // only take action if no owned windows are active
            if (this.MainWindow.OwnedWindows.Count == 0)
            {
                // activate the main window
                if (this.MainWindow != null)
                {
                    if (this.MainWindow.WindowState == WindowState.Minimized)
                    {
                        this.MainWindow.WindowState = WindowState.Normal;
                    }

                    this.MainWindow.Activate();
                }

                // parse the requested Uri (if present) and request navigation
                StackHashUri stackHashUri = null;
                if (StackHashUri.TryParse(args, out stackHashUri))
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                        string.Format(CultureInfo.CurrentCulture,
                        "Other instance requests navigation to {0}",
                        stackHashUri.RawUri));

                    this.CurrentStackHashUri = stackHashUri;
                    if (StackHashUriNavigationRequest != null)
                    {
                        StackHashUriNavigationRequest(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, 
                    "Other instance activation ignored as owned windows exist");
            }

            return true;
        }

        #endregion
    }
}
