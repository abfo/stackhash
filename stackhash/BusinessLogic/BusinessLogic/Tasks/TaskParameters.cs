using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackHashBusinessObjects;
using StackHashErrorIndex;

namespace StackHashTasks
{
    public class TaskParameters
    {
        private string m_Name;
        private bool m_RunInParallel;
        private bool m_IsBackgroundTask;
        private bool m_UseSeparateThread;
        private StackHashClientData m_ClientData;
        private SettingsManager m_SettingsManager;
        private int m_ContextId;
        private ControllerContext m_ControllerContext;
        private IErrorIndex m_ErrorIndex;
        private bool m_IsRetry;

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public bool RunInParallel
        {
            get { return m_RunInParallel; }
            set { m_RunInParallel = value; }
        }

        public bool IsBackgroundTask
        {
            get { return m_IsBackgroundTask; }
            set { m_IsBackgroundTask = value; }
        }

        public bool UseSeparateThread
        {
            get { return m_UseSeparateThread; }
            set { m_UseSeparateThread = value; }
        }

        public int ContextId
        {
            get { return m_ContextId; }
            set { m_ContextId = value; }
        }

        public StackHashClientData ClientData
        {
            get { return m_ClientData; }
            set { m_ClientData = value; }
        }
        public SettingsManager SettingsManager
        {
            get { return m_SettingsManager; }
            set { m_SettingsManager = value; }
        }

        public ControllerContext ControllerContext
        {
            get { return m_ControllerContext; }
            set { m_ControllerContext = value; }
        }

        public IErrorIndex ErrorIndex
        {
            get { return m_ErrorIndex; }
            set { m_ErrorIndex = value; }
        }

        public bool IsRetry
        {
            get { return m_IsRetry; }
            set { m_IsRetry = value; }
        }
    }
}
