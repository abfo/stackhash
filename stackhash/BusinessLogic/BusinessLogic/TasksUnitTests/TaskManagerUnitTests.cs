using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackHashTasks;
using System.Threading;

namespace TasksUnitTests
{
    /// <summary>
    /// Summary description for TaskManagerUnitTests
    /// </summary>
    [TestClass]
    public class TaskManagerUnitTests
    {
        const int s_TaskTimeout = 10000;

        public TaskManagerUnitTests()
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
            taskManager.Dispose();
        }

        [TestMethod]
        public void TestRunOneTask()
        {
            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;

            DummyTask task1 = new DummyTask(params1);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            Assert.AreEqual(1, task1.ExecutionCount);
            Assert.AreEqual(null, task1.LastException);
            taskManager.Dispose();
        }

        [TestMethod]
        public void TestRunTwoTasks()
        {
            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask1";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;

            DummyTask task1 = new DummyTask(params1);
            
            DummyTaskParameters params2 = new DummyTaskParameters();
            params2.IsBackgroundTask = true;
            params2.Name = "TestRunOneTask2";
            params2.RunInParallel = false;
            params2.UseSeparateThread = true;

            DummyTask task2 = new DummyTask(params2);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);
            taskManager.Enqueue(task2);
            taskManager.WaitForTaskCompletion(task2, s_TaskTimeout);

            Assert.AreEqual(1, task1.ExecutionCount);
            Assert.AreEqual(1, task2.ExecutionCount);
            Assert.AreEqual(null, task1.LastException);
            Assert.AreEqual(null, task2.LastException);

            Assert.AreEqual(true, task1.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, task2.CurrentTaskState.TaskCompleted);
            taskManager.Dispose();
        }


        [TestMethod]
        public void TestRunSameTaskManyTimes()
        {
            int numTasks = 1000;

            TaskManager taskManager = new TaskManager("Test");

            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = false;
            params1.Name = "TestRunOneTask1";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;


            DummyTask [] dummyTasks = new DummyTask [numTasks];

            // Queue all the tasks.
            for (int i = 0; i < numTasks; i++)
            {
                dummyTasks[i] = new DummyTask(params1);
                taskManager.Enqueue(dummyTasks[i]);
            }

            // Wait for the last one to complete.
            taskManager.WaitForTaskCompletion(dummyTasks[numTasks - 1], s_TaskTimeout * numTasks);


            // Check all the tasks completed ok.
            for (int i = 0; i < numTasks; i++)
            {
                Assert.AreEqual(1, dummyTasks[i].ExecutionCount);
                Assert.AreEqual(null, dummyTasks[i].LastException);
                Assert.AreEqual(true, dummyTasks[i].CurrentTaskState.TaskCompleted);
            }
            taskManager.Dispose();
        }


        [TestMethod]
        public void TestAbortOneRunningTask()
        {
            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;
            params1.WaitForAbort = true;

            DummyTask task1 = new DummyTask(params1);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.Enqueue(task1);

            // Give it a second to start running the task then abort.
            Thread.Sleep(1000);
            taskManager.AbortCurrentTask(true);

            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            Assert.AreEqual(1, task1.ExecutionCount);
            Assert.AreNotEqual(null, task1.LastException);
            Assert.AreEqual(true, task1.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, task1.CurrentTaskState.Aborted);
            Assert.AreEqual(true, task1.CurrentTaskState.ExternalAbortRequested);
            taskManager.Dispose();
        }


        [TestMethod]
        public void TestRunOneConcurrentTask()
        {
            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "TestRunOneTask";
            params1.RunInParallel = false;
            params1.UseSeparateThread = true;

            DummyTask task1 = new DummyTask(params1);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.RunConcurrentTask(task1);
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            Assert.AreEqual(1, task1.ExecutionCount);
            Assert.AreEqual(null, task1.LastException);

            taskManager.Dispose();
        }

        [TestMethod]
        public void TestRunTwoConcurrentTasks()
        {
            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "DummyTask1";
            params1.RunInParallel = true;
            params1.UseSeparateThread = true;
            params1.WaitForAbort = true;

            DummyTask task1 = new DummyTask(params1);

            DummyTaskParameters params2 = new DummyTaskParameters();
            params2.IsBackgroundTask = true;
            params2.Name = "DummyTask2";
            params2.RunInParallel = true;
            params2.UseSeparateThread = true;
            params2.TimeToWait = 1000; // Wait one second.

            DummyTask task2 = new DummyTask(params2);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.RunConcurrentTask(task1);
            taskManager.RunConcurrentTask(task2);
            taskManager.WaitForTaskCompletion(task2, s_TaskTimeout);

            // Task 2 should complete first - then abort task1.
            Assert.AreEqual(1, task2.ExecutionCount);
            Assert.AreEqual(1, task1.ExecutionCount);
            Assert.AreEqual(null, task2.LastException);
            Assert.AreEqual(true, task2.CurrentTaskState.TaskCompleted);


            Assert.AreEqual(false, task1.CurrentTaskState.TaskCompleted);
            taskManager.AbortAllConcurrentTasks();
            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);

            Assert.AreEqual(null, task2.LastException);
            Assert.AreNotEqual(null, task1.LastException);
            Assert.AreEqual(true, task1.CurrentTaskState.TaskCompleted);

            taskManager.Dispose();

        }

        [TestMethod]
        public void TestAbortTwoConcurrentTasks()
        {
            DummyTaskParameters params1 = new DummyTaskParameters();
            params1.IsBackgroundTask = true;
            params1.Name = "DummyTask1";
            params1.RunInParallel = true;
            params1.UseSeparateThread = true;
            params1.WaitForAbort = true;

            DummyTask task1 = new DummyTask(params1);

            DummyTaskParameters params2 = new DummyTaskParameters();
            params2.IsBackgroundTask = true;
            params2.Name = "DummyTask2";
            params2.RunInParallel = true;
            params2.UseSeparateThread = true;
            params2.WaitForAbort = true;

            DummyTask task2 = new DummyTask(params2);

            // Create a task manager for the context.
            TaskManager taskManager = new TaskManager("Test");
            taskManager.RunConcurrentTask(task1);
            taskManager.RunConcurrentTask(task2);

            // Now abort both tasks.
            taskManager.AbortAllConcurrentTasks();

            taskManager.WaitForTaskCompletion(task1, s_TaskTimeout);
            taskManager.WaitForTaskCompletion(task2, s_TaskTimeout);

            Assert.AreEqual(1, task1.ExecutionCount);
            Assert.AreEqual(1, task2.ExecutionCount);
            Assert.AreNotEqual(null, task1.LastException);
            Assert.AreNotEqual(null, task2.LastException);
            Assert.AreEqual(true, task1.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, task2.CurrentTaskState.TaskCompleted);

            taskManager.Dispose();
        }
    }
}
