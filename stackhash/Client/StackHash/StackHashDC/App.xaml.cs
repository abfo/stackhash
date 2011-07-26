using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace StackHashDC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string SignInAssistantFolder = @"Microsoft Shared\Windows Live";
        private const string SignInAssistantFile = @"wlidcli.dll";

        /// <summary />
        protected override void OnStartup(StartupEventArgs e)
        {
            bool signInAssistantDetected = false;

            // try to detect a file installed with the sign-in assistant
            try
            {
                string searchDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), SignInAssistantFolder);
                if (Directory.Exists(searchDirectoryPath))
                {
                    DirectoryInfo searchDirectory = new DirectoryInfo(searchDirectoryPath);
                    FileInfo[] files = searchDirectory.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (string.Compare(file.Name, SignInAssistantFile, true) == 0)
                        {
                            signInAssistantDetected = true;
                            break;
                        }
                    }
                }
            }
            catch { }

            // if the file exits then just exit
            if (signInAssistantDetected)
            {
                Environment.Exit(0);
            }

            base.OnStartup(e);
        }
    }
}
