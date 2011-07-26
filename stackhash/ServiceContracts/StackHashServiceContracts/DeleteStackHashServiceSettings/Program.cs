using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeleteStackHashServiceSettings
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathForSystem = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pathForServiceSettings = pathForSystem + "\\StackHash";
            DateTime now = DateTime.Now;

            string backupFolder = pathForServiceSettings + now.Year + now.Month + now.Day + now.Hour + now.Minute + now.Second;
            if (Directory.Exists(pathForServiceSettings))
            {
                Directory.Move(pathForServiceSettings, backupFolder);
            }
        }
    }
}
