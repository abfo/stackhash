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
using System.Windows.Threading;

namespace StackHash
{
    /// <summary>
    /// Window displayed while the current context has been deactivated by another client
    /// </summary>
    public partial class ContextInactive : Window
    {
        private ClientLogic _clientLogic;
        private bool _closed;
        private DispatcherTimer _dispatcherTimer;

        /// <summary>
        /// Window displayed while the current context has been deactivated by another client
        /// </summary>
        /// <param name="clientLogic">ClientLogic</param>
        public ContextInactive(ClientLogic clientLogic)
        {
            if (clientLogic == null) { throw new ArgumentNullException("clientLogic"); }
            _clientLogic = clientLogic;

            InitializeComponent();

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            _dispatcherTimer.Tick += new EventHandler(_dispatcherTimer_Tick);
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!_closed)
            {
                if (!_clientLogic.OtherClientHasDisabledContext)
                {
                    // close the dialog with a successfull result if the context is activated again
                    _dispatcherTimer.Stop();
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closed = true;
            _dispatcherTimer.Stop();
        }
    }
}
