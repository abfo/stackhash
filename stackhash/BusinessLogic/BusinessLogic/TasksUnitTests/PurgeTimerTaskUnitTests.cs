using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using StackHashTasks;
using StackHashBusinessObjects;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for PurgeTimerTaskUnitTests
    /// The PurgeTimerTask is a calendar task that waits until the next scheduled time for a sync and 
    /// just starts the specified real task.
    /// </summary>
    [TestClass]
    public class PurgeTimerTaskUnitTests
    {
        const int s_TaskTimeout = 10000;

        public PurgeTimerTaskUnitTests()
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
        public void TestConstructor()
        {
            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
        }


        [TestMethod]
        public void TestFirstTickTwoSecondsHourly()
        {
            // Create a dummy controller to record the callbacks.
            DummyControllerContext controllerContext = new DummyControllerContext();

            // Set up parameters for the PurgeTimerTask
            PurgeTimerTaskParameters params1 = new PurgeTimerTaskParameters();

            // Standard task parameters.
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            // Specific task parameters.
            params1.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();

            // Set the schedule so the task runs in 2 seconds time (every hour).
            params1.ScheduleCollection = new ScheduleCollection();

            Schedule schedule = new Schedule();
            schedule.DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday |
                DaysOfWeek.Thursday | DaysOfWeek.Friday | DaysOfWeek.Saturday | DaysOfWeek.Sunday;

            DateTime timeToRunTask = DateTime.Now.AddSeconds(2);

            schedule.Time = new ScheduleTime(timeToRunTask.Hour, timeToRunTask.Minute, timeToRunTask.Second);
            schedule.Period = SchedulePeriod.Hourly;

            params1.ScheduleCollection.Add(schedule);


            // Create the task and run it.
            PurgeTimerTask task1 = new PurgeTimerTask(params1);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.RunPurgeCallCount);

            // Now wait for a little over 2 seconds.
            Thread.Sleep(3300);

            Assert.AreEqual(1, controllerContext.RunPurgeCallCount);

            // Task should still be running after a tick.
            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);

            taskManager.AbortAllTasks(false);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            // Should be set to trigger in approximately an hour.

            long timeDiff = Math.Abs(60 * 60 * 1000 - task1.TimeInMilliseconds);
            Console.WriteLine("TimeDiff: " + timeDiff.ToString());
            Assert.AreEqual(true, timeDiff < 2000);
        }



        [TestMethod]
        public void TestFirstTickTwoSecondsDaily()
        {
            // Create a dummy controller to record the callbacks.
            DummyControllerContext controllerContext = new DummyControllerContext();

            // Set up the task parameters.
            PurgeTimerTaskParameters params1 = new PurgeTimerTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            params1.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();

            // Time set to expire in 2 seconds but daily - so the one after should be tomorrow at the same time.
            params1.ScheduleCollection = new ScheduleCollection();
            Schedule schedule = new Schedule();
            schedule.DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday |
                DaysOfWeek.Thursday | DaysOfWeek.Friday | DaysOfWeek.Saturday | DaysOfWeek.Sunday;

            DateTime timeToRunTask = DateTime.Now.AddSeconds(2);

            schedule.Time = new ScheduleTime(timeToRunTask.Hour, timeToRunTask.Minute, timeToRunTask.Second);
            schedule.Period = SchedulePeriod.Daily;

            params1.ScheduleCollection.Add(schedule);


            // Create and run the task.
            PurgeTimerTask task1 = new PurgeTimerTask(params1);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.RunPurgeCallCount);

            // Now wait for a little over 2 seconds.
            Thread.Sleep(3300);

            Assert.AreEqual(1, controllerContext.RunPurgeCallCount);

            // Task should still be running after a tick.
            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);

            taskManager.AbortAllTasks(false);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);
            taskManager.Dispose();

            // Should be set to trigger in approximately an hour.

            long timeDiff = Math.Abs(24 * 60 * 60 * 1000 - task1.TimeInMilliseconds);
            Assert.AreEqual(true, timeDiff < 2000);
        }
        [TestMethod]
        public void TestFirstTickTwoSecondsWeekly()
        {
            // Create a dummy controller to record the call backs.
            DummyControllerContext controllerContext = new DummyControllerContext();

            PurgeTimerTaskParameters params1 = new PurgeTimerTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            params1.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();
            params1.ScheduleCollection = new ScheduleCollection();
            Schedule schedule = new Schedule();
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);


            DateTime timeToRunTask = DateTime.Now.AddSeconds(2);

            schedule.Time = new ScheduleTime(timeToRunTask.Hour, timeToRunTask.Minute, timeToRunTask.Second);
            schedule.Period = SchedulePeriod.Weekly;

            params1.ScheduleCollection.Add(schedule);


            PurgeTimerTask task1 = new PurgeTimerTask(params1);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.RunPurgeCallCount);

            // Now wait for a little over 2 seconds.
            Thread.Sleep(3200);

            Assert.AreEqual(1, controllerContext.RunPurgeCallCount);

            // Task should still be running after a tick.
            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);

            taskManager.AbortAllTasks(false);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);
            taskManager.Dispose();

            // Should be set to trigger in approximately a week.
            long timeDiff = Math.Abs(7 * 24 * 60 * 60 * 1000 - task1.TimeInMilliseconds);
            Assert.AreEqual(true, timeDiff < (500 + 60 * 60 * 1000)); // Could be an hour out if you run this test during the week before a daylight saving change.
        }

        /// <summary>
        /// Timer triggers and tries to run the purge task when it is already running.
        /// The timer task should log the event but then reschedule as normal. It should not exit the app.
        /// See BugzID:868.
        /// </summary>
        [TestMethod]
        public void TestFirstTickTwoSecondsHourlyPurgeTaskAlreadyRunning()
        {
            // Create a dummy controller to record the callbacks.
            DummyControllerContext controllerContext = new DummyControllerContext();
            controllerContext.MakeExceptionInPurge = true;

            // Set up parameters for the PurgeTimerTask
            PurgeTimerTaskParameters params1 = new PurgeTimerTaskParameters();

            // Standard task parameters.
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            // Specific task parameters.
            params1.PurgeOptionsCollection = new StackHashPurgeOptionsCollection();

            // Set the schedule so the task runs in 2 seconds time (every hour).
            params1.ScheduleCollection = new ScheduleCollection();

            Schedule schedule = new Schedule();
            schedule.DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday |
                DaysOfWeek.Thursday | DaysOfWeek.Friday | DaysOfWeek.Saturday | DaysOfWeek.Sunday;

            DateTime timeToRunTask = DateTime.Now.AddSeconds(2);

            schedule.Time = new ScheduleTime(timeToRunTask.Hour, timeToRunTask.Minute, timeToRunTask.Second);
            schedule.Period = SchedulePeriod.Hourly;

            params1.ScheduleCollection.Add(schedule);


            // Create the task and run it.
            PurgeTimerTask task1 = new PurgeTimerTask(params1);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.RunPurgeCallCount);

            // Now wait for a little over 2 seconds.
            Thread.Sleep(3300);

            Assert.AreEqual(1, controllerContext.RunPurgeCallCount);

            // Task should still be running after a tick.
            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);

            taskManager.AbortAllTasks(false);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            // Should be set to trigger in approximately an hour.

            long timeDiff = Math.Abs(60 * 60 * 1000 - task1.TimeInMilliseconds);
            Assert.AreEqual(true, timeDiff < 2000);
        }


    }
}
