using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;

namespace StackHashTasks
{
    internal static class AppSettings
    {
        /// <summary>
        /// The last hit date will be used to determine the date for which events will be requested. However, it is possible
        /// that hits will be added for events prior to this date after we have done a sync so always overlap the syncs by
        /// a number of days. This config setting determines that overlap. Hard coded to 7 days if param not present.
        /// </summary>
        public static int PullDateMinimumDuration
        {
            get
            {                
                int pullDateMinimumDuration = 7;

                try
                {
                    // Get the setting from the App.Config file.
                    String pullDateMinimumDurationConfigString = System.Configuration.ConfigurationManager.AppSettings["PullDateMinimumDuration"];

                    if (pullDateMinimumDurationConfigString != null)
                        pullDateMinimumDuration = Int32.Parse(pullDateMinimumDurationConfigString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return pullDateMinimumDuration;
            }
        }


        /// <summary>
        /// During the analyze task cab processing may fail one or more times. This values defines how many 
        /// consecutive errors are allowed to occur before giving up and aborting the task.
        /// </summary>
        public static int ConsecutiveAnalyzeCabErrorsBeforeAbort
        {
            get
            {
                int consecutiveAnalyzeCabErrorsBeforeAbort = 10;

                try
                {
                    String configString = System.Configuration.ConfigurationManager.AppSettings["ConsecutiveAnalyzeCabErrorsBeforeAbort"];

                    if (configString != null)
                        consecutiveAnalyzeCabErrorsBeforeAbort = Int32.Parse(configString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return consecutiveAnalyzeCabErrorsBeforeAbort;
            }
        }

        /// <summary>
        /// The interval between attempts to reactivate contexts after reboot.
        /// </summary>
        public static int ContextRetryTimeoutInSeconds
        {
            get
            {
                int contextRetryTimeoutInSeconds = 60;

                try
                {
                    String configString = System.Configuration.ConfigurationManager.AppSettings["ContextRetryTimeoutInSeconds"];

                    if (configString != null)
                        contextRetryTimeoutInSeconds = Int32.Parse(configString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return contextRetryTimeoutInSeconds;
            }
        }

        /// <summary>
        /// Total time from reboot to retry.
        /// </summary>
        public static int ContextRetryPeriodInSeconds
        {
            get
            {
                int contextRetryPeriodInSeconds = 60 * 20; // 20 minutes.

                try
                {
                    String configString = System.Configuration.ConfigurationManager.AppSettings["ContextRetryPeriodInSeconds"];

                    if (configString != null)
                        contextRetryPeriodInSeconds = Int32.Parse(configString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return contextRetryPeriodInSeconds;
            }
        }

        
        /// <summary>
        /// Used for testing to force all database creation to be SQL.
        /// </summary>
        public static bool ForceSqlDatabase
        {
            get
            {
                bool forceSqlDatabase = false;

                try
                {
                    String configString = System.Configuration.ConfigurationManager.AppSettings["ForceSqlDatabase"];

                    if (configString != null)
                        forceSqlDatabase = bool.Parse(configString);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return forceSqlDatabase;
            }
        }

        /// <summary>
        /// True - use WindowsLiveId login to WinQual, False - use Ticket.
        /// </summary>
        public static bool UseWindowsLiveId
        {
            get
            {
                bool useWindowsLiveId = true;

                try
                {
                    String configString = System.Configuration.ConfigurationManager.AppSettings["UseWindowsLiveId"];

                    if (configString != null)
                        useWindowsLiveId = bool.Parse(configString);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return useWindowsLiveId;
            }
        }

        /// <summary>
        /// The number of events to be copied at a time during an index copy.
        /// Default should be approx 1000. Too large and the memory usage will be too big.
        /// Too small and the time taken in SQL filtering the events will be too big.
        /// </summary>
        public static int CopyIndexEventsPerBlock
        {
            get
            {
                // Set this to 100 as the default for standalone testing (no service so no app.config).
                // The service app.config will contain the true default of 1000.
                int copyIndexEventsPerBlock = 100; 

                try
                {
                    // Get the setting from the App.Config file.
                    String copyIndexEventsPerBlockConfigString = System.Configuration.ConfigurationManager.AppSettings["CopyIndexEventsPerBlock"];

                    if (copyIndexEventsPerBlockConfigString != null)
                        copyIndexEventsPerBlock = Int32.Parse(copyIndexEventsPerBlockConfigString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return copyIndexEventsPerBlock;
            }
        }

        /// <summary>
        /// Interval between sending progress reports to clients during an Index Move.
        /// </summary>
        public static int IntervalBetweenProgressReportsInSeconds
        {
            get
            {
                int intervalBetweenProgressReportsInSeconds = 1; // One second for testing. The default in app.config should be 10 seconds.

                try
                {
                    // Get the setting from the App.Config file.
                    String intervalBetweenProgressReportsInSecondsString = System.Configuration.ConfigurationManager.AppSettings["IntervalBetweenProgressReportsInSeconds"];

                    if (intervalBetweenProgressReportsInSecondsString != null)
                        intervalBetweenProgressReportsInSeconds = Int32.Parse(intervalBetweenProgressReportsInSecondsString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return intervalBetweenProgressReportsInSeconds;
            }
        }

        /// <summary>
        /// Default connection string settings.
        /// </summary>
        public static String DefaultSqlConnectionString
        {
            get
            {
                string defaultSqlConnectionString = "Data Source=(local)\\STACKHASH;Integrated Security=True"; 

                try
                {
                    // Get the setting from the App.Config file.
                    String newDefaultSqlConnectionString = System.Configuration.ConfigurationManager.AppSettings["DefaultSqlConnectionString"];

                    if (newDefaultSqlConnectionString != null)
                        defaultSqlConnectionString = newDefaultSqlConnectionString;
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return defaultSqlConnectionString;
            }
        }

        /// <summary>
        /// Interval between attempts to log on to WinQual. The winqual live id login may expire after 12 hours and cause
        /// problems. This timeout forces StackHash to logout and log back in again after the specified time during a sync.
        /// </summary>
        public static int IntervalBetweenWinQualLogonsInHours
        {
            get
            {
                int intervalBetweenWinQualLogonsInHours = 11; 

                try
                {
                    // Get the setting from the App.Config file.
                    String intervalBetweenWinQualLogonsInHoursString = System.Configuration.ConfigurationManager.AppSettings["IntervalBetweenWinQualLogonsInHours"];

                    if (intervalBetweenWinQualLogonsInHoursString != null)
                        intervalBetweenWinQualLogonsInHours = Int32.Parse(intervalBetweenWinQualLogonsInHoursString, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException) { }
                catch (System.Configuration.ConfigurationErrorsException) { }

                return intervalBetweenWinQualLogonsInHours;
            }
        }
    }
}
