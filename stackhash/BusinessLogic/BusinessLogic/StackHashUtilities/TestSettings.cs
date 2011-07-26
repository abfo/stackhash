using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using System.Xml;

namespace StackHashUtilities
{
    public static class TestSettings
    {
        private static NameValueCollection s_TestSettings = new NameValueCollection();
        private static bool s_SettingsLoaded;

        private static void loadSettings()
        {
            string pathForSystem = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pathForServiceSettings = pathForSystem + "\\StackHash";
            string testModeFileName = pathForServiceSettings + "\\testmode.xml";


            if (!File.Exists(testModeFileName))
                return;

            // Load the settings from the file testmode.xml file.
            XmlReader xmlReader = null;

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.IgnoreWhitespace = true;
                settings.IgnoreComments = true;
                xmlReader = XmlReader.Create(testModeFileName, settings);

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                        s_TestSettings[xmlReader.Name] = xmlReader.ReadString();
                }
            }
            finally
            {
                if (xmlReader != null)
                    xmlReader.Close();
            }
        }

        public static String DefaultConnectionString
        {
            get
            {
                String defaultConnectionString = TestSettings.GetAttribute("ConnectionString");

                if (String.IsNullOrEmpty(defaultConnectionString))
                    defaultConnectionString = "Data Source=(local)\\SQLEXPRESS;Integrated Security=True;";

                if (!defaultConnectionString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                    defaultConnectionString += ";";

                return defaultConnectionString;
            }
        }

        public static String TestMode
        {
            get
            {
                String defaultUnitTestModeString = TestSettings.GetAttribute("TestMode");

                if (String.IsNullOrEmpty(defaultUnitTestModeString))
                    defaultUnitTestModeString = "0";

                return defaultUnitTestModeString;
            }
        }

        public static String StackHashWebServiceEndpoint
        {
            get
            {
                String defaultString = TestSettings.GetAttribute("StackHashWebServiceEndpoint");

                // Allow null return.
                return defaultString;
            }
        }

        public static bool UseWindowsLiveId
        {
            get
            {
                String defaultString = TestSettings.GetAttribute("UseWindowsLiveId");

                if (String.IsNullOrEmpty(defaultString))
                    return false;

                return (String.Compare(defaultString.ToUpperInvariant(), "TRUE", StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
        
        public static String GetAttribute(String name)
        {
            if (s_SettingsLoaded == false)
            {
                loadSettings();
                s_SettingsLoaded = true;
            }

            if (s_TestSettings.AllKeys.Contains(name))
            {
                return s_TestSettings[name];
            }
            else
            {
                return null;
            }
        }

        public static String WinQualUserName
        {
            get
            {
                return TestSettings.GetAttribute("WinQualUserName");
            }
        }

        public static String WinQualPassword
        {
            get
            {
                return TestSettings.GetAttribute("WinQualPassword");
            }
        }

        public static String TestEmail1
        {
            get
            {
                return TestSettings.GetAttribute("TestEmail1");
            }
        }

        public static String TestEmail1Password
        {
            get
            {
                return TestSettings.GetAttribute("TestEmail1Password");
            }
        }

        public static String TestEmail2
        {
            get
            {
                return TestSettings.GetAttribute("TestEmail2");
            }
        }

        public static String SmtpHost
        {
            get
            {
                return TestSettings.GetAttribute("SmtpHost");
            }
        }

        public static int SmtpPort
        {
            get
            {
                int smtpPort = 0;
                if (Int32.TryParse(TestSettings.GetAttribute("SmtpHost"), out smtpPort))
                    return smtpPort;
                else
                    return 587;
            }
        }

        public static int WinQualCallDelay
        {
            get
            {
                int winQualCallDelay = 0;
                if (Int32.TryParse(TestSettings.GetAttribute("WinQualCallDelay"), out winQualCallDelay))
                    return winQualCallDelay;
                else
                    return 0;
            }
        }

        public static String TestDataFolder
        {
            get
            {
                // Get the folder where this DLL has been loaded from.
                String path = Assembly.GetExecutingAssembly().Location;

                String devPath = @"r:\stackhash\businesslogic\businesslogic\";
                if (Directory.Exists(devPath))
                {
                    return devPath + @"testdata\";
                }
                else
                {
                    int folderIndex = path.IndexOf("results", StringComparison.OrdinalIgnoreCase);

                    if (folderIndex > 0)
                    {
                        // Running in unit test mode on a different machine.
                        path = path.Substring(0, folderIndex);
                        path += "testdata\\";
                        return path;
                    }
                    else
                    {
                        // Running from the TestData button in the main product.
                        path = "c:\\stackhashtestdata\\";
                        return path;
                    }
                }
            }
        }
    }
}
