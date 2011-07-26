using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for SchedulesUnitTests
    /// </summary>
    [TestClass]
    public class SchedulesUnitTests
    {
        public SchedulesUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestLastScheduledTimeHourlyScheduleTimePastTodayEnabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();
            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeHourlyScheduleTimePastTodayDisabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(-1).DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);
            scheduledTime = scheduledTime.AddDays(-1);
            scheduledTime = scheduledTime.AddHours(23 - scheduledTime.Hour);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();
            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeHourlyScheduleTimePastSameMinuteTodayEnabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddSeconds(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();
            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeHourlyScheduleTimePastSameMinuteTodayDisabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(-1).DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddSeconds(-1);
            scheduledTime = scheduledTime.AddHours(23 - scheduledTime.Hour);
            scheduledTime = scheduledTime.AddDays(-1);
            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();
            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }


        [TestMethod]
        public void TestLastScheduledTimeHourlyScheduleTimeWasLastHourTodayEnabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            // Note that 1 hour ago could be yesterday so add tomorrow too.
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek) | Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(-1).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            // Should be one hour ago.
            scheduledTime = scheduledTime.AddHours(-1);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();
            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeHourlyScheduleTimePastTodayEnabled()
        {
            // Time has passed this hour - so should return the next hour.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            scheduledTime = scheduledTime.AddHours(1); // Should be next hour.

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeHourlyScheduleTimePastTodayNotEnabled()
        {
            // Time has passed this hour - so should return the next hour.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(1).DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            scheduledTime = scheduledTime.AddDays(1); // Should be next day.
            scheduledTime = scheduledTime.AddHours(-1 * scheduledTime.Hour); // Should 00:MM.

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeHourlyScheduleTimeToComeTodayEnabled()
        {
            // Time has not yet passed this hour- so should return the same hour.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+2);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeHourlyScheduleTimeToComeTodayDisabled()
        {
            // Time has not yet passed this hour- so should return the same hour.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(6).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+2);
            scheduledTime = scheduledTime.AddDays(6);
            scheduledTime = scheduledTime.AddHours(scheduledTime.Hour * -1); // Should be 00:MM

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeDailyScheduleTimePastTodayEnabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeDailyScheduleTimePastTodayDisabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(-1).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);
            scheduledTime = scheduledTime.AddDays(-1);
            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeDailyScheduleTimeFutureYesterdayEnabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(-1).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            scheduledTime = scheduledTime.AddDays(-1);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeDailyScheduleTimeFutureYesterdayDisabled()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(-3).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            scheduledTime = scheduledTime.AddDays(-3);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeDailyScheduleTimePastTomorrowEnabled()
        {
            // Time has passed this day already so next time will be tomorrow.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(1).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            scheduledTime = scheduledTime.AddDays(1); // Should be tomorrow.


            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeDailyScheduleTimePastTomorrowDisabled()
        {
            // Time has passed this day already so next time will be tomorrow.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(2).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            scheduledTime = scheduledTime.AddDays(2); // Should be 2 days time.

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeDailyScheduleTimeInFutureTodayEnabled()
        {
            // Time has yet to come today so should return today.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);


            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }


        [TestMethod]
        public void TestNextScheduledTimeDailyScheduleTimeInFutureTodayDisabled()
        {
            // Time has yet to come today so should return today.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Daily;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.AddDays(6).DayOfWeek);
            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);
            scheduledTime = scheduledTime.AddDays(6);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);


            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }


        [TestMethod]
        public void TestLastScheduledTimeWeeklyScheduleTodayPast()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
            schedule.DaysOfWeek = Schedule.ToDaysOfWeek(scheduledTime.DayOfWeek);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeWeeklyScheduleTodayFuture()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
            schedule.DaysOfWeek = Schedule.ToDaysOfWeek(scheduledTime.DayOfWeek);


            scheduledTime = scheduledTime.AddDays(-7);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestLastScheduledTimeWeeklyScheduleYesterday()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            // Set yesterday.
            int dayToSet = (int)scheduledTime.DayOfWeek;
            if (dayToSet == 0)
                dayToSet = 6;
            else
                dayToSet--;

            schedule.DaysOfWeek = Schedule.ToDaysOfWeek((DayOfWeek)dayToSet);


            scheduledTime = scheduledTime.AddDays(-1);

            DateTime lastScheduledTime = schedule.LastScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, lastScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, lastScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, lastScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, lastScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, lastScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, lastScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeWeeklyScheduleTodayPast()
        {
            // Time was scheduled for today but has passed already so the next
            // schedule will be next week.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(-1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
            schedule.DaysOfWeek = Schedule.ToDaysOfWeek(scheduledTime.DayOfWeek);

            scheduledTime = scheduledTime.AddDays(7);

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeWeeklyScheduleTodayFuture()
        {
            // Time was scheduled for today and is yet to come so should return today.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
            schedule.DaysOfWeek = Schedule.ToDaysOfWeek(scheduledTime.DayOfWeek);

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeWeeklyScheduleTomorrow()
        {
            // Time was scheduled for tomorrow - should return tomorrow.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
            schedule.DaysOfWeek = Schedule.ToDaysOfWeek(scheduledTime.DayOfWeek);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            // Set yesterday.
            int dayToSet = ((int)scheduledTime.DayOfWeek + 1) % 7;

            schedule.DaysOfWeek = Schedule.ToDaysOfWeek((DayOfWeek)dayToSet);

            scheduledTime = scheduledTime.AddDays(1);

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeWeeklyScheduleYesterday()
        {
            // Time was scheduled for yesterday - should return 6 days from now.
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Weekly;

            DateTime scheduledTime = DateTime.Now.AddMinutes(+1);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
            schedule.DaysOfWeek = Schedule.ToDaysOfWeek(scheduledTime.DayOfWeek);

            schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            // Set yesterday.
            int dayToSet = (int)scheduledTime.DayOfWeek;
            if (dayToSet == 0)
                dayToSet = 6;
            else
                dayToSet--;

            schedule.DaysOfWeek = Schedule.ToDaysOfWeek((DayOfWeek)dayToSet);

            scheduledTime = scheduledTime.AddDays(6);

            DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

            Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
            Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
            Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
            Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
            Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
            Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
        }

        [TestMethod]
        public void TestNextScheduledTimeHourlyRepeatCalls()
        {
            Schedule schedule = new Schedule();
            schedule.Period = SchedulePeriod.Hourly;
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);
            for (int i = 0; i < 100000; i++)
            {
                DateTime scheduledTime = DateTime.Now;
                schedule.Time = new ScheduleTime(scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
                scheduledTime = scheduledTime.AddHours(1);


                DateTime nextScheduledTime = schedule.NextScheduledTime.ToLocalTime();

                Assert.AreEqual(scheduledTime.Year, nextScheduledTime.Year);
                Assert.AreEqual(scheduledTime.Month, nextScheduledTime.Month);
                Assert.AreEqual(scheduledTime.Day, nextScheduledTime.Day);
                Assert.AreEqual(scheduledTime.Hour, nextScheduledTime.Hour);
                Assert.AreEqual(scheduledTime.Minute, nextScheduledTime.Minute);
                Assert.AreEqual(scheduledTime.Second, nextScheduledTime.Second);
            }
        }
    }
}
