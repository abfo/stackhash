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
using System.Windows.Navigation;
using System.Windows.Shapes;
using StackHash.StackHashService;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;

namespace StackHash
{
    /// <summary>
    /// Control to edit a StackHash Service Schedule
    /// </summary>
    public partial class ScheduleControl : UserControl
    {
        private class ScheduleValidation : IDataErrorInfo
        {
            public bool ValidationEnabled { get; set; }
            public int Hour { get; set; }
            public int Min { get; set; }

            public ScheduleValidation(int hour, int min)
            {
                Hour = hour;
                Min = min;
            }

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get 
                {
                    if (!ValidationEnabled)
                    {
                        return null;
                    }

                    string result = null;

                    switch (columnName)
                    {
                        case "Hour":
                            if ((Hour < 0) || (Hour > 23))
                            {
                                result = Properties.Resources.ScheduleControl_ValidationHour;
                            }
                            break;

                        case "Min":
                            if ((Min < 0) || (Min > 59))
                            {
                                result = Properties.Resources.ScheduleControl_ValidationMin;
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private ScheduleValidation _scheduleValidation;

        /// <summary>
        /// Control to edit a StackHash Service Schedule
        /// </summary>
        public ScheduleControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the description for this schedule
        /// </summary>
        public string ScheduleDescription
        {
            get
            {
                return this.textBlockDescription.Text;
            }
            set
            {
                this.textBlockDescription.Text = value;
            }
        }

        /// <summary>
        /// True if at least one day of the week is selected
        /// </summary>
        public bool OneDaySelected
        {
            get
            {
                return ((checkMonday.IsChecked == true) ||
                    (checkTuesday.IsChecked == true) ||
                    (checkWednesday.IsChecked == true) ||
                    (checkThursday.IsChecked == true) ||
                    (checkFriday.IsChecked == true) ||
                    (checkSaturday.IsChecked == true) ||
                    (checkSunday.IsChecked == true));
            }
        }

        /// <summary>
        /// Update the control from a StackHash service schedule
        /// </summary>
        /// <param name="schedule">Schedule</param>
        public void UpdateFromSchedule(Schedule schedule)
        {
            Debug.Assert(schedule != null);
            Debug.Assert(schedule.Time != null);

            _scheduleValidation = new ScheduleValidation(schedule.Time.Hour, schedule.Time.Minute);
            _scheduleValidation.ValidationEnabled = true;
            this.DataContext = _scheduleValidation;

            switch (schedule.Period)
            {
                case SchedulePeriod.Daily:
                default:
                    radioDaily.IsChecked = true;
                    break;

                case SchedulePeriod.Hourly:
                    radioHourly.IsChecked = true;
                    break;
            }

            if ((schedule.DaysOfWeek & DaysOfWeek.Monday) == DaysOfWeek.Monday) checkMonday.IsChecked = true;
            if ((schedule.DaysOfWeek & DaysOfWeek.Tuesday) == DaysOfWeek.Tuesday) checkTuesday.IsChecked = true;
            if ((schedule.DaysOfWeek & DaysOfWeek.Wednesday) == DaysOfWeek.Wednesday) checkWednesday.IsChecked = true;
            if ((schedule.DaysOfWeek & DaysOfWeek.Thursday) == DaysOfWeek.Thursday) checkThursday.IsChecked = true;
            if ((schedule.DaysOfWeek & DaysOfWeek.Friday) == DaysOfWeek.Friday) checkFriday.IsChecked = true;
            if ((schedule.DaysOfWeek & DaysOfWeek.Saturday) == DaysOfWeek.Saturday) checkSaturday.IsChecked = true;
            if ((schedule.DaysOfWeek & DaysOfWeek.Sunday) == DaysOfWeek.Sunday) checkSunday.IsChecked = true;
        }

        /// <summary>
        /// Saves the control to a StackHash service schedule
        /// </summary>
        /// <returns>Schedule</returns>
        public Schedule SaveToSchedule()
        {
            Debug.Assert(_scheduleValidation != null);

            Schedule schedule = new Schedule();
            schedule.Time = new ScheduleTime();

            schedule.Time.Hour = _scheduleValidation.Hour;
            schedule.Time.Minute = _scheduleValidation.Min;

            if (radioDaily.IsChecked == true)
            {
                schedule.Period = SchedulePeriod.Daily;
            }
            else
            {
                schedule.Period = SchedulePeriod.Hourly;
            }

            if (checkMonday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Monday;
            if (checkTuesday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Tuesday;
            if (checkWednesday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Wednesday;
            if (checkThursday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Thursday;
            if (checkFriday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Friday;
            if (checkSaturday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Saturday;
            if (checkSunday.IsChecked == true) schedule.DaysOfWeek |= DaysOfWeek.Sunday;

            return schedule;
        }
    }
}
