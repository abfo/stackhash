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
using StackHash.StackHashService;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Shows script results
    /// </summary>
    public partial class ScriptResultViewer : Window
    {
        private const string WindowKey = "ScriptResultViewer";

        /// <summary>
        /// Shows script results
        /// </summary>
        public ScriptResultViewer()
        {
            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StackHashScriptResult scriptResult = this.DataContext as StackHashScriptResult;
            if (scriptResult != null)
            {
                richTextBoxResults.Document.Blocks.Clear();
                richTextBoxResults.Document.Blocks.Add(GetScriptResultParagraph(scriptResult));
            }
        }

        /// <summary>
        /// Gets a Paragraph containing the results of a script run
        /// </summary>
        /// <param name="scriptResult">Script result to parse</param>
        /// <returns>Paragraph</returns>
        public static Paragraph GetScriptResultParagraph(StackHashScriptResult scriptResult)
        {
            Paragraph para = new Paragraph();
            para.FontFamily = new FontFamily("Courier New");

            foreach (StackHashScriptLineResult line in scriptResult.ScriptResults)
            {
                AddResultLineToParagraph(line, ref para);
            }

            return para;
        }

        private static void AddResultLineToParagraph(StackHashScriptLineResult line, ref Paragraph para)
        {
            // bold command
            Span commandSpan = new Span();
            commandSpan.FontWeight = FontWeights.Bold;
            commandSpan.Inlines.Add(new Run(line.ScriptLine.Command));

            // add comment if present
            if (!string.IsNullOrEmpty(line.ScriptLine.Comment))
            {
                Span commentSpan = new Span();
                commentSpan.Foreground = Brushes.Green;
                commentSpan.Inlines.Add(new Run(string.Format(CultureInfo.CurrentCulture,
                    " * {0}",
                    line.ScriptLine.Comment)));

                commandSpan.Inlines.Add(commentSpan);
            }

            // add command/comment to para
            para.Inlines.Add(commandSpan);
            para.Inlines.Add(new LineBreak());

            // add results
            foreach (string outputLine in line.ScriptLineOutput)
            {
                para.Inlines.Add(new Run(outputLine));
                para.Inlines.Add(new LineBreak());
            }

            para.Inlines.Add(new LineBreak());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
