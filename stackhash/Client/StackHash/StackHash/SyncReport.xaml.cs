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
    /// Shows the result of the last synchronization attempt
    /// </summary>
    public partial class SyncReport : Window
    {
        private const string WindowKey = "SyncReport";

        private StackHashContextStatus _contextStatus;

        /// <summary>
        /// Shows the result of the last synchronization attempt
        /// </summary>
        /// <param name="contextStatus">Current context status</param>
        public SyncReport(StackHashContextStatus contextStatus)
        {
            // Ok for contextStatus to be null
            _contextStatus = contextStatus;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        private void BuildReport(StackHashTaskStatus syncStatus, bool logonFailed, string logonExceptionText)
        {
            // logonFailed and logonExceptionText don't seem to be working - see Case 599

            if (syncStatus.ServiceErrorCode == StackHashServiceErrorCode.NoError)
            {
                // sync succeeded
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(Properties.Resources.Synchronization + " "));
                
                Span successSpan = new Span();
                successSpan.FontWeight = FontWeights.Bold;
                successSpan.Foreground = Brushes.Green;
                successSpan.Inlines.Add(new Run(Properties.Resources.Succeeded));
                para.Inlines.Add(successSpan);

                DateTime successDateLocal = syncStatus.LastSuccessfulRunTimeUtc.ToLocalTime();

                para.Inlines.Add(new Run(" " + string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SyncReport_AtDateTime,
                    successDateLocal.ToLongDateString(),
                    successDateLocal.ToLongTimeString())));

                richTextBoxSyncReport.Document.Blocks.Add(para);
            }
            else if (syncStatus.ServiceErrorCode == StackHashServiceErrorCode.Aborted)
            {
                DateTime startedDateLocal = syncStatus.LastStartedTimeUtc.ToLocalTime();

                AddPlainParagraph(string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SyncReport_Canceled,
                    startedDateLocal.ToLongDateString(),
                    startedDateLocal.ToLongTimeString()));
            }
            else
            {
                // sync failed
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(Properties.Resources.Synchronization + " "));

                Span successSpan = new Span();
                successSpan.FontWeight = FontWeights.Bold;
                successSpan.Foreground = Brushes.Red;
                successSpan.Inlines.Add(new Run(Properties.Resources.Failed));
                para.Inlines.Add(successSpan);

                DateTime failDateLocal = syncStatus.LastFailedRunTimeUtc.ToLocalTime();

                para.Inlines.Add(new Run(" " + string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SyncReport_AtDateTime,
                    failDateLocal.ToLongDateString(),
                    failDateLocal.ToLongTimeString())));

                richTextBoxSyncReport.Document.Blocks.Add(para);

                AddPlainParagraph(string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SyncReport_FailReason,
                    syncStatus.ServiceErrorCode == StackHashServiceErrorCode.UnexpectedError ? syncStatus.LastException : StackHashMessageBox.GetServiceErrorCodeMessage(syncStatus.ServiceErrorCode)));

                DateTime successDateLocal = syncStatus.LastSuccessfulRunTimeUtc.ToLocalTime();

                AddPlainParagraph(string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SyncReport_LastSuccess,
                    successDateLocal.ToLongDateString(),
                    successDateLocal.ToLongTimeString()));
            }

            TimeSpan syncDuration = new TimeSpan(0, 0, syncStatus.LastDurationInSeconds);
            AddPlainParagraph(string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.SyncReport_Duration,
                TimeSpanDisplayString(syncDuration)));
        }

        private void AddPlainParagraph(string text)
        {
            richTextBoxSyncReport.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        private string TimeSpanDisplayString(TimeSpan timeSpan)
        {
            StringBuilder displayString = new StringBuilder();

            bool hasHours = false;
            bool hasHoursOrMinutes = false;
            int hours = (int)Math.Floor(timeSpan.TotalHours);
            if (hours > 0)
            {
                hasHours = true;
                hasHoursOrMinutes = true;

                if (hours == 1)
                {
                    displayString.AppendFormat(CultureInfo.CurrentCulture, "1 {0}, ", Properties.Resources.Hour);
                }
                else
                {
                    displayString.AppendFormat(CultureInfo.CurrentCulture, "{0:n0} {1}, ", hours, Properties.Resources.Hours);
                }
            }

            if (hasHours || (timeSpan.Minutes > 0))
            {
                hasHoursOrMinutes = true;

                if (timeSpan.Minutes == 1)
                {
                    displayString.AppendFormat(CultureInfo.CurrentCulture, "1 {0} ", Properties.Resources.Minute);
                }
                else
                {
                    displayString.AppendFormat(CultureInfo.CurrentCulture, "{0} {1} ", timeSpan.Minutes, Properties.Resources.Minutes);
                }
            }

            if (hasHoursOrMinutes)
            {
                displayString.AppendFormat(CultureInfo.CurrentCulture, "{0} ", Properties.Resources.And);
            }

            if (timeSpan.Seconds == 1)
            {
                displayString.AppendFormat(CultureInfo.CurrentCulture, "1 {0}", Properties.Resources.Second);
            }
            else
            {
                displayString.AppendFormat(CultureInfo.CurrentCulture, "{0} {1}", timeSpan.Seconds, Properties.Resources.Seconds);
            }

            return displayString.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            richTextBoxSyncReport.Document.Blocks.Clear();

            if (_contextStatus == null)
            {
                // no context status to parse
                AddPlainParagraph(Properties.Resources.SyncReport_NoStatus);
            }
            else
            {
                foreach (StackHashTaskStatus taskStatus in _contextStatus.TaskStatusCollection)
                {
                    if (taskStatus.TaskType == StackHashTaskType.WinQualSynchronizeTask)
                    {
                        if (taskStatus.RunCount == 0)
                        {
                            // sync has not run yet
                            AddPlainParagraph(Properties.Resources.SyncReport_NoSync);
                        }
                        else
                        {
                            // we have some result to report!
                            BuildReport(taskStatus,
                                _contextStatus.LastSynchronizationLogOnFailed,
                                _contextStatus.LastSynchronizationLogOnException);
                        }

                        break;
                    }
                }
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("synchronization-report.htm");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
