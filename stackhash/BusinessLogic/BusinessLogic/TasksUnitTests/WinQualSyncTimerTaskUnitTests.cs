using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using StackHashTasks;
using StackHashBusinessObjects;
using StackHashWinQual;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for WinQualSyncTimerTaskUnitTests
    /// The WinQualSyncTimerTask is a calendar task that waits until the next scheduled time for a sync and 
    /// just starts the specified real task.
    /// </summary>
    [TestClass]
    public class WinQualSyncTimerTaskUnitTests
    {
        const int s_TaskTimeout = 10000;

        public WinQualSyncTimerTaskUnitTests()
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

            // Set up parameters for the WinQualSyncTimerTask
            WinQualSyncTimerTaskParameters params1 = new WinQualSyncTimerTaskParameters();

            // Standard task parameters.
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            // Specific task parameters.
            params1.ThisWinQualContext = new WinQualContext(null);

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
            WinQualSyncTimerTask task1 = new WinQualSyncTimerTask(params1);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.SynchronizeCallCount);

            // Now wait for a little over 2 seconds.
            Thread.Sleep(4000);

            Assert.AreEqual(1, controllerContext.SynchronizeCallCount);
            
            // Task should still be running after a tick.
            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);

            taskManager.AbortAllTasks(false);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            // Should be set to trigger in approximately an hour.

            long timeDiff = Math.Abs(60 * 60 * 1000 - task1.TimeInMilliseconds);
            Assert.AreEqual(true, timeDiff < 2000);
        }

        [TestMethod]
        public void TestFirstTickTwoSecondsDaily()
        {
            // Create a dummy controller to record the callbacks.
            DummyControllerContext controllerContext = new DummyControllerContext();

            // Set up the task parameters.
            WinQualSyncTimerTaskParameters params1 = new WinQualSyncTimerTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            params1.ThisWinQualContext = new WinQualContext(null);

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
            WinQualSyncTimerTask task1 = new WinQualSyncTimerTask(params1);
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.SynchronizeCallCount);

            // Now wait for a little over 2 seconds (+1)
            Thread.Sleep(4000);

            Assert.AreEqual(1, controllerContext.SynchronizeCallCount);

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

            WinQualSyncTimerTaskParameters params1 = new WinQualSyncTimerTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.ControllerContext = controllerContext;

            params1.ThisWinQualContext = new WinQualContext(null);
            params1.ScheduleCollection = new ScheduleCollection();
            Schedule schedule = new Schedule();
            schedule.DaysOfWeek = Schedule.ConvertDateTimeDayToStackHashDay(DateTime.Now.DayOfWeek);


            DateTime timeToRunTask = DateTime.Now.AddSeconds(2);

            schedule.Time = new ScheduleTime(timeToRunTask.Hour, timeToRunTask.Minute, timeToRunTask.Second);
            schedule.Period = SchedulePeriod.Weekly;

            params1.ScheduleCollection.Add(schedule);


            WinQualSyncTimerTask task1 = new WinQualSyncTimerTask(params1);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            Assert.AreEqual(0, controllerContext.SynchronizeCallCount);

            // Now wait for a little over 2 seconds + 1 (the extra 1 is because the sync task starts 1 second after the timer triggers).
            Thread.Sleep(4000);

            Assert.AreEqual(1, controllerContext.SynchronizeCallCount);

            // Task should still be running after a tick.
            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);

            taskManager.AbortAllTasks(false);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);
            taskManager.Dispose();

            // Should be set to trigger in approximately an hour.
            long timeDiff = Math.Abs(7 * 24 * 60 * 60 * 1000 - task1.TimeInMilliseconds);
            Assert.AreEqual(true, timeDiff < (500 + 60 * 60 * 1000)); // Could be an hour out if you run this test during the week before a daylight saving change.
        }
    }
}
