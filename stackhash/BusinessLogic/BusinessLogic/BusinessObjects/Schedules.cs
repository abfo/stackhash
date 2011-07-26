using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;


namespace StackHashBusinessObjects
{
    [FlagsAttribute]
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [SuppressMessage("Microsoft.Naming", "CA1714")]
    // Make sure these values match the order in DayOfWeek + 1.
    public enum DaysOfWeek : int
    {
        [EnumMember()]
        Sunday = 0x01,
        [EnumMember()]
        Monday = 0x02,
        [EnumMember()]
        Tuesday = 0x04,
        [EnumMember()]
        Wednesday = 0x08,
        [EnumMember()]
        Thursday = 0x10,
        [EnumMember()]
        Friday = 0x20,
        [EnumMember()]
        Saturday = 0x40,
        [EnumMember()]
        All = 0x7f,
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum SchedulePeriod
    {
        [EnumMember()]
        Hourly,
        [EnumMember()]
        Daily,
        [EnumMember()]
        Weekly
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class ScheduleTime
    {
        private int m_Hour;
        private int m_Minute;
        private int m_Second;

        [DataMember]
        public int Hour
        {
            get { return m_Hour; }
            set { m_Hour = value; }
        }
        [DataMember]
        public int Minute
        {
            get { return m_Minute; }
            set { m_Minute = value; }
        }

        [DataMember]
        public int Second
        {
            get { return m_Second; }
            set { m_Second = value; }
        }

        public ScheduleTime(int hour, int minute, int second)
        {
            m_Hour = hour;
            m_Minute = minute;
            m_Second = second;
        }

        public ScheduleTime() { ; } // Required for serialization.

    }

    /// <summary>
    /// For hourly, only the Min and Seconds of ScheduleTime is used.
    /// For daily, only the full time is used.
    /// For weekly, the days in week and the time are used. You can therefore 
    /// specify any combination of days.
    /// If you want to set a number of days with different times then create a collection
    /// with a number of schedules - one for each day (each set on a weekly type).
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class Schedule : IComparable
    {
        private SchedulePeriod m_Period;
        private DaysOfWeek m_DaysOfWeek;
        private ScheduleTime m_Time; // The date will be ignored.

        [DataMember]
        public SchedulePeriod Period
        {
            get { return m_Period; }
            set { m_Period = value; }
        }

        [DataMember]
        public DaysOfWeek DaysOfWeek
        {
            get { return m_DaysOfWeek; }
            set { m_DaysOfWeek = value; }
        }

        [DataMember]
        public ScheduleTime Time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        public Schedule() { ; }

        public DateTime LastScheduledTime
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime lastScheduledTime;
                int startDay;


                switch (m_Period)
                {
                    case SchedulePeriod.Hourly:
                        // Assume the time this hour has not arrived yet.
                        lastScheduledTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 
                            m_Time.Minute, m_Time.Second, DateTimeKind.Local);

                        // Only the min and seconds are used.
                        if ((m_Time.Minute > now.Minute) ||
                            ((m_Time.Minute == now.Minute) && (m_Time.Second > now.Second)))
                        {
                            // Time was in last hour.
                            lastScheduledTime = lastScheduledTime.AddHours(-1);
                        }

                        if (!IsDayEnabled((int)ConvertDateTimeDayToStackHashDay(lastScheduledTime.DayOfWeek)))
                        {
                            // Today is not enabled so find the next day that is.
                            startDay = (int)(lastScheduledTime.DayOfWeek);

                            lastScheduledTime = FindPreviousDay(startDay, lastScheduledTime);

                            // Should be MM just before midnight so set hours to 23.
                            lastScheduledTime = lastScheduledTime.AddHours(23 - lastScheduledTime.Hour);
                        }

                        break;

                    case SchedulePeriod.Daily:
                    case SchedulePeriod.Weekly:
                        // The time indicates the time we should run today.
                        lastScheduledTime = new DateTime(now.Year, now.Month, now.Day,
                            m_Time.Hour, m_Time.Minute, m_Time.Second, 0, DateTimeKind.Local);

                        // If the time is yet to come today then it was last hit yesterday.
                        if (lastScheduledTime > now)
                            lastScheduledTime = lastScheduledTime.AddDays(-1);

                        if (!IsDayEnabled((int)ConvertDateTimeDayToStackHashDay(lastScheduledTime.DayOfWeek)))
                        {
                            startDay = (int)(lastScheduledTime.DayOfWeek);

                            // Today is not enabled so find the next day that is.
                            lastScheduledTime = FindPreviousDay(startDay, lastScheduledTime);
                        }

                        break;

                    
                    default:
                        throw new NotSupportedException("Unknown period type");
                }

                if (lastScheduledTime.Kind == DateTimeKind.Local)
                    return lastScheduledTime.ToUniversalTime();
                else
                    return lastScheduledTime;
            }
        }

        public bool IsDayEnabled(int dayOfWeek)
        {
            int daysOfWeek = (int)m_DaysOfWeek;
            return ((daysOfWeek & dayOfWeek) != 0);
        }


        public static DaysOfWeek EveryDay
        {
            get
            {
                return DaysOfWeek.Sunday | DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday | DaysOfWeek.Thursday | DaysOfWeek.Friday | DaysOfWeek.Saturday;
            }
        }
        public static DaysOfWeek ConvertDateTimeDayToStackHashDay(DayOfWeek dateTimeDay)
        {
            DaysOfWeek stackHashDaysOfWeek;

            switch (dateTimeDay)
            {
                case DayOfWeek.Sunday:
                    stackHashDaysOfWeek = DaysOfWeek.Sunday;
                    break;
                case DayOfWeek.Monday:
                    stackHashDaysOfWeek = DaysOfWeek.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    stackHashDaysOfWeek = DaysOfWeek.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    stackHashDaysOfWeek = DaysOfWeek.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    stackHashDaysOfWeek = DaysOfWeek.Thursday;
                    break;
                case DayOfWeek.Friday:
                    stackHashDaysOfWeek = DaysOfWeek.Friday;
                    break;
                case DayOfWeek.Saturday:
                    stackHashDaysOfWeek = DaysOfWeek.Saturday;
                    break;
                default:
                    throw new ArgumentException("Invalid day of week specified", "dateTimeDay");
            }

            return stackHashDaysOfWeek;
        }

        public DateTime FindPreviousDay(int startDay, DateTime lastScheduledTime)
        {
            bool foundDay = false;
            int currentDay = startDay;

            // Loop forward through the days.
            for (int days = 0; days < 7; days++)
            {
                if (((int)m_DaysOfWeek & (1 << currentDay)) != 0)
                {
                    foundDay = true;
                    break;
                }
                else
                {
                    if (currentDay == 0)
                        currentDay = 6;
                    else
                        currentDay--;
                    lastScheduledTime = lastScheduledTime.AddDays(-1);
                }
            }

            if (!foundDay)
                return new DateTime(0); // Some time way in the future.
            else
                return lastScheduledTime;
        }

        public DateTime FindNextDay(int startDay, DateTime nextScheduledTime)
        {
            bool foundDay = false;
            int currentDay = startDay;

            // Loop forward through the days.
            for (int days = 0; days < 7; days++)
            {
                if (((int)m_DaysOfWeek & (1 << currentDay)) != 0)
                {
                    foundDay = true;
                    break;
                }
                else
                {
                    currentDay = (currentDay + 1) % 7;
                    nextScheduledTime = nextScheduledTime.AddDays(1);
                }
            }

            if (!foundDay)
                return new DateTime(2100, 1, 1); // Some time way in the future.
            else
                return nextScheduledTime;
        }


        /// <summary>
        /// Returns the next scheduled time in UTC.
        /// </summary>
        public DateTime NextScheduledTime
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime nextScheduledTime;
                int startDay;


                switch (m_Period)
                {
                    case SchedulePeriod.Hourly:

                        // Assume the time this hour has not arrived yet.
                        nextScheduledTime = new DateTime(now.Year, now.Month, now.Day, now.Hour,
                            m_Time.Minute, m_Time.Second, DateTimeKind.Local);

                        // Only the min and seconds are used.
                        // If the time has already passed this hour then it will be the next hour.
                        if ((m_Time.Minute < now.Minute) ||
                            ((m_Time.Minute == now.Minute) && (m_Time.Second <= now.Second)))
                        {
                            // Time was in last hour.
                            nextScheduledTime = nextScheduledTime.AddHours(1);
                        }

                        // Check to see if the day today is included in the schedule. If not 
                        // then find the next day that is.
                        startDay = (int)now.DayOfWeek;

                        if (!IsDayEnabled((int)ConvertDateTimeDayToStackHashDay(nextScheduledTime.DayOfWeek)))
                        {
                            // Today is not enabled so find the next day that is.
                            nextScheduledTime = FindNextDay(startDay, nextScheduledTime);

                            // Should be MM past midnight so set hours to 0.
                            nextScheduledTime = nextScheduledTime.AddHours(nextScheduledTime.Hour * -1);
                        }
                        break;

                    case SchedulePeriod.Daily:
                    case SchedulePeriod.Weekly:
                        // The time indicates the time we should run today.
                        nextScheduledTime = new DateTime(now.Year, now.Month, now.Day,
                            m_Time.Hour, m_Time.Minute, m_Time.Second, 0, DateTimeKind.Local);


                        // If the time has passed then we need to wait till tomorrow.
                        if (nextScheduledTime <= now)
                            nextScheduledTime = nextScheduledTime.AddDays(1);

                        // Check to see if the day today is included in the schedule. If not 
                        // then find the next day that is.
                        startDay = (int)nextScheduledTime.DayOfWeek;

                        // Today is not enabled so find the next day that is.
                        nextScheduledTime = FindNextDay(startDay, nextScheduledTime);

                        break;

                    default:
                        throw new NotSupportedException("Unknown period type");
                }

                if (nextScheduledTime.Kind == DateTimeKind.Local)
                    return nextScheduledTime.ToUniversalTime();
                else
                    return nextScheduledTime;
            }
        }

        public static DaysOfWeek ToDaysOfWeek(DayOfWeek dayOfWeek)
        {
            int dayOfWeekInt = (1 << (int)dayOfWeek);

            return (DaysOfWeek)dayOfWeekInt;
        }
        #region IComparable Members

        public int CompareTo(object obj)
        {
            Schedule targetSchedule = obj as Schedule;

            if (this.DaysOfWeek != targetSchedule.DaysOfWeek)
                return -1;
            if (this.Period != targetSchedule.Period)
                return -1;
            if (this.Time.Hour != targetSchedule.Time.Hour)
                return -1;
            if (this.Time.Minute != targetSchedule.Time.Minute)
                return -1;
            if (this.Time.Second != targetSchedule.Time.Second)
                return -1;

            // Same.
            return 0;
        }

        #endregion
    }


    /// <summary>
    /// Schedule for running a particular task. 
    /// Allows specifying different times for different days by adding multiple
    /// schedules - one for each day.
    /// You can also run twice a day in the same way.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class ScheduleCollection : Collection<Schedule>, IComparable
    {
        public ScheduleCollection() { ; }

        public DateTime NextScheduledTime
        {
            get 
            {
                DateTime closestTime = new DateTime(2200, 12, 1);

                foreach (Schedule Schedule in this)
                {
                    DateTime nextTime = Schedule.NextScheduledTime;
                    if (nextTime < closestTime)
                        closestTime = nextTime;
                }

                return closestTime;
            }
        }

        public DateTime LastScheduledTime
        {
            get
            {
                DateTime closestTime = new DateTime(0);

                foreach (Schedule Schedule in this)
                {
                    DateTime lastTime = Schedule.LastScheduledTime;
                    if (lastTime > closestTime)
                        closestTime = lastTime;
                }

                return closestTime;
            }
        }


        #region IComparable Members

        public int CompareTo(object obj)
        {
            ScheduleCollection targetScheduleCollection = obj as ScheduleCollection;

            if (targetScheduleCollection.Count != this.Count)
                return -1;

            for (int i = 0; i < this.Count; i++)
            {
                // Compare schedules.
                int compareResult = this[i].CompareTo(targetScheduleCollection[i]);

                if (compareResult != 0)
                    return compareResult;
            }

            // Same.
            return 0;
        }

        #endregion
    }

}
