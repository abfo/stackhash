using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace StackHash
{
    /// <summary>
    /// Launches context sensitive help
    /// </summary>
    public static class StackHashHelp
    {
        private static readonly string _helpPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "StackHash.chm");

        /// <summary>
        /// Opens help and displays the default topic
        /// </summary>
        public static void ShowDefaultTopic()
        {
            Help.ShowHelp(null, _helpPath);
        }

        /// <summary>
        /// Opens help and displays a specific topic
        /// </summary>
        /// <param name="topic"></param>
        public static void ShowTopic(string topic)
        {
            if (topic == null) { throw new ArgumentNullException("topic"); }

            Help.ShowHelp(null, _helpPath, HelpNavigator.Topic, topic);
        }
    }
}
