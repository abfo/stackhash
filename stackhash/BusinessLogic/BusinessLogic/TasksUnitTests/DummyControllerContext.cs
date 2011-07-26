using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashTasks;
using System.Threading;

using StackHashBusinessObjects;

namespace TasksUnitTests
{
    public class DummyControllerContext : ControllerContext
    {
        private int m_SynchronizeCallCount;
        private int m_RunPurgeCallCount;

        public DummyControllerContext()
        {
        }

        public int SynchronizeCallCount
        {
            get { return m_SynchronizeCallCount; }
        }

        public int RunPurgeCallCount
        {
            get { return m_RunPurgeCallCount; }
        }

        public bool MakeExceptionInPurge { get; set; }

        /// <summary>
        /// Synchronizes the local database with the WinQual service online.
        /// </summary>
        /// <param name="clientData">Client information.</param>
        /// <param name="forceFullSynchronize">True - full sync, false - syncs from last successful sync time.</param>
        /// <param name="waitForCompletion">true - thread waits, false - returns immediately.</param>
        /// <param name="justSyncProducts">true - just sync the product list, false - sync according to enabled products.</param>
        /// <param name="productsToSynchronize">List of products to sync - can be null.</param>
        /// <param name="isTimedSync">True - started by timer, false - started by user.</param>
        /// <param name="isRetry">True - task being started after a retry, false - task not started as a result of a retry.</param>
        public override void RunSynchronizeTask(StackHashClientData clientData, bool forceFullSynchronize, bool waitForCompletion,
            bool justSyncProducts, StackHashProductSyncDataCollection productsToSynchronize, bool isTimedSync, bool isRetry)
        {
            m_SynchronizeCallCount++;
        }

        /// <summary>
        /// Purges old cabs (currently > 180 days) from the index.
        /// </summary>
        /// <param name="clientData">Data passed to the client callback.</param>
        public override void RunPurgeTask(StackHashClientData clientData)
        {
            m_RunPurgeCallCount++;

            // Throw an unexpected error.
            if (MakeExceptionInPurge)
                throw new ArgumentException("clientData");
        }

    }
}

