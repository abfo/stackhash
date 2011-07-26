using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace StackHashTasks
{
    public class Reporter
    {
        private static Reporter s_Reporter;
        private Controller m_Controller;

        public static Reporter CurrentReporter
        {
            get
            {
                return s_Reporter;
            }
        }

        public Reporter(Controller controller)
        {
            s_Reporter = this;
            m_Controller = controller;
        }

        public void ReportEvent(EventArgs eventData)
        {
            Monitor.Enter(this);
            try
            {
                m_Controller.OnAdminReport(this, eventData);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}
