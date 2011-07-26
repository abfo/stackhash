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
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TaskUnitTests
    {
        public TaskUnitTests()
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

        /// <summary>
        /// Run before each test. 
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize() 
        {
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
        public void TestTaskNotStarted()
        {
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;

            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);
        }


        [TestMethod]
        public void TestTaskCompleteOk()
        {
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;

            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);

            testTask.Run();
            testTask.TaskStartedNotificationEvent.Set();
            testTask.Join(60000); // Shouldn't take more than 60 seconds to complete.

            Assert.AreEqual(null, testTask.LastException);
            Assert.AreEqual(true, testTask.EntryPointExecuted);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);

        }


        [TestMethod]
        public void TestTaskCompleteWithException()
        {
            // Force the task to return an exception.
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;
            testParams.SetLastException = true;
            testParams.LastExceptionToSet = new ArgumentException("hello");

            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);



            testTask.Run();
            testTask.TaskStartedNotificationEvent.Set();
            testTask.Join(60000); // Shouldn't take more than 60 seconds to complete.

            Assert.AreEqual(testParams.LastExceptionToSet, testTask.LastException);
            Assert.AreEqual(true, testTask.EntryPointExecuted);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);
        }


        [TestMethod]
        public void TestTaskExternalAbort()
        {
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;
            
            // Force the task to return an exception.
            testParams.WaitForAbort = true;


            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);

            testTask.Run();
            testTask.TaskStartedNotificationEvent.Set();

            // Wait for a second and then Abort the task.
            Thread.Sleep(1000);

            // Shouldn't be stopped yet.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);

            testTask.StopExternal();

            testTask.Join(60000); // Shouldn't take more than 60 seconds to complete.

            Assert.AreNotEqual(null, testTask.LastException);

            // Check the task state.
            Assert.AreEqual(true, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(true, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);
        }

        [TestMethod]
        public void TestTaskInternalAbort()
        {
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;
            testParams.WaitForAbort = true;


            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);

            // Force the task to return an exception.
            testTask.Run();
            testTask.TaskStartedNotificationEvent.Set();

            // Wait for a second and then Abort the task.
            Thread.Sleep(1000);

            // Shouldn't be stopped yet.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);

            testTask.ForceCallToStopInternal();

            testTask.Join(60000); // Shouldn't take more than 60 seconds to complete.

            Assert.AreNotEqual(null, testTask.LastException);

            // Check the task state.
            Assert.AreEqual(true, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(true, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);
        }

        [TestMethod]
        public void TestTaskSuspendResume()
        {
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;
            // Force the task to return an exception.
            testParams.WaitForSuspend = true;

            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);


            // Also set the state of the task to suspended before it starts so that it 
            // is guaranteed to see SuspendRequested when calling CheckSuspend.
            testTask.Suspend();
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(true, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Run the task - the task will suspend and hang waiting for a resume.
            testTask.Run();
            testTask.TaskStartedNotificationEvent.Set();

            // Wait for the task to suspend (or until we time out).
            int totalTimeWaited = 0;
            int timeoutValue = 100;
            int maxTimeToWait = 10000;

            while (!testTask.CurrentTaskState.Suspended)
            {
                Thread.Sleep(timeoutValue);
                totalTimeWaited += timeoutValue;
                if (totalTimeWaited > maxTimeToWait)
                    Assert.AreEqual(true, totalTimeWaited < maxTimeToWait);
            }

            // Should be suspended.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(true, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);


            testTask.Resume();

            testTask.Join(60000); // Shouldn't take more than 60 seconds to complete.

            Assert.AreEqual(null, testTask.LastException);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);
        }

        [TestMethod]
        public void TestTaskSuspendAbort()
        {
            DummyTaskParameters testParams = new DummyTaskParameters();
            testParams.Name = "DummyTask";
            testParams.RunInParallel = true;
            testParams.IsBackgroundTask = true;
            testParams.UseSeparateThread = true;
            // Force the task to return an exception.
            testParams.WaitForSuspend = true;

            DummyTask testTask = new DummyTask(testParams);

            // Check the initialisation data has been stored ok.
            Assert.AreEqual(testParams.Name, testTask.Name);
            Assert.AreEqual(testParams.RunInParallel, testTask.InitializationData.RunInParallel);
            Assert.AreEqual(testParams.IsBackgroundTask, testTask.InitializationData.IsBackgroundTask);
            Assert.AreEqual(testParams.UseSeparateThread, testTask.InitializationData.UseSeparateThread);

            // Check the task state.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(false, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Check other properties initial state.
            Assert.AreEqual(null, testTask.LastException);


            // Also set the state of the task to suspended before it starts so that it 
            // is guaranteed to see SuspendRequested when calling CheckSuspend.
            testTask.Suspend();
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(true, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskStarted);

            // Run the task - the task will suspend and hang waiting for a resume.
            testTask.Run();
            testTask.TaskStartedNotificationEvent.Set();

            // Wait for the task to suspend (or until we time out).
            int totalTimeWaited = 0;
            int timeoutValue = 100;
            int maxTimeToWait = 10000;

            while (!testTask.CurrentTaskState.Suspended)
            {
                Thread.Sleep(timeoutValue);
                totalTimeWaited += timeoutValue;
                if (totalTimeWaited > maxTimeToWait)
                    Assert.AreEqual(true, totalTimeWaited < maxTimeToWait);
            }

            // Should be suspended.
            Assert.AreEqual(false, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(false, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(true, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);


            testTask.StopExternal();

            testTask.Join(60000); // Shouldn't take more than 60 seconds to complete.

            Assert.AreNotEqual(null, testTask.LastException);

            // Check the task state.
            Assert.AreEqual(true, testTask.CurrentTaskState.Aborted);
            Assert.AreEqual(true, testTask.CurrentTaskState.AbortRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.ExternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.InternalAbortRequested);
            Assert.AreEqual(false, testTask.CurrentTaskState.Suspended);
            Assert.AreEqual(true, testTask.CurrentTaskState.SuspendRequested);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskCompleted);
            Assert.AreEqual(true, testTask.CurrentTaskState.TaskStarted);
        }
    }
}
