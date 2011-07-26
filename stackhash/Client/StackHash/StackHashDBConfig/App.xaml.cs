using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using StackHash;
using System.Xaml;

namespace StackHashDBConfig
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string UnhandledExceptionTitle = "Fatal Error - StackHash Database Configuration";
        private const string UnhandledExceptionMessage = "A fatal error has occurred and StackHash will now exit.";

        /// <summary />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            App.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DisplayUnhandledExceptionAndDie(e.ExceptionObject as Exception);
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            DisplayUnhandledExceptionAndDie(e.Exception);
            e.Handled = true;
        }

        private static void DisplayUnhandledExceptionAndDie(Exception ex)
        {
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
                    StackHash.StackHashService.StackHashServiceErrorCode.NoError);
            }
            catch (XamlParseException)
            {
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
                App.Current.Shutdown();
            }
        }

        /// <summary />
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                base.OnExit(e);

                // close any open services on exit
                ServiceProxy.Services.Dispose();

                Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            }
            catch { }
        }
    }
}
