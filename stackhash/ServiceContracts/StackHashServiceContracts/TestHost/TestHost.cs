using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;
using StackHashTasks;
using StackHashServiceImplementation;


namespace TestAppHost
{
    public partial class TestHost : Form
    {
        [SuppressMessage("Microsoft.Performance", "CA1823", Justification = "Only ever instantiated - never used here")]
        StaticObjects m_StaticObjects;

        public TestHost()
        {
            InitializeComponent();

            string pathForSystem = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pathForServiceSettings = pathForSystem + "\\StackHash";

            if (!Directory.Exists(pathForServiceSettings))
                Directory.CreateDirectory(pathForServiceSettings);

            string testModeFileName = pathForServiceSettings + "\\testmode.xml";
            string testSettingsFileName = pathForServiceSettings + "\\testsettings.xml";
            string settingsFileName = pathForServiceSettings + "\\settings.xml";

            // Now initialise the controller with those settings.
            if (File.Exists(testModeFileName))
                m_StaticObjects = new StaticObjects(testSettingsFileName, false, true);
            else
                m_StaticObjects = new StaticObjects(settingsFileName, false, false);
        }

        private void openServicesButton_Click(object sender, EventArgs e)
        {
            try
            {
                m_StaticObjects.EnableServices();
                statusListBox.Items.Add("Internal and External Services Started");
            }
            catch (System.Exception ex)
            {
                if (ex.Message != null)
                    statusListBox.Items.Add(ex.Message);
            }
        }

        private void TestHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_StaticObjects.Dispose();
        }

        private void closeServicesButton_Click(object sender, EventArgs e)
        {
            try
            {
                m_StaticObjects.DisableServices();
                statusListBox.Items.Add("Internal and External Services Stopped");
            }
            catch (System.Exception ex)
            {
                if (ex.Message != null)
                    statusListBox.Items.Add(ex.Message);
            }
        }
    }
}
