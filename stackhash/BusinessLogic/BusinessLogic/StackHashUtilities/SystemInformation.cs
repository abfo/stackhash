using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;


namespace StackHashUtilities
{
    public static class SystemInformation
    {
        private const int s_MaxSleepStateRetries = 3;

        /// <summary>
        /// Determines if the process is running as a 32 bit process on Win 64.
        /// This call won't work on versions of Windows prior to XP.
        /// </summary>
        /// <returns>true - Wow64, false - 32 bit</returns>
        public static bool Is64BitSystem()
        {
            bool is64 = false;

            if (IntPtr.Size == 8)
            {
                // we're a 64-bit process
                is64 = true;
            }
            else
            {
                // check for WOW64
                try
                {
                    if (((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)) ||
                        (Environment.OSVersion.Version.Major >= 6))
                    {
                        NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out is64);
                    }
                }
                catch { }
            }

            return is64;
        }

        /// <summary>
        /// Determines if the process is running in admin mode (elevated) or user mode.
        /// </summary>
        /// <returns>true - admin, false - user</returns>
        public static bool IsAdmin()  
        {
	        if (System.Environment.OSVersion.Version.Major >= 5)
            {
		        WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
		        bool currentUserIsAdmin = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
                return currentUserIsAdmin;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if running on XP.
        /// </summary>
        /// <returns>true - XP, false - not XP</returns>
        public static bool IsXP()  
        {
	        if (System.Environment.OSVersion.Version.Major == 5)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Prevents the computer from entering sleep or hibernate until EnableSleep is called
        /// </summary>
        /// <returns>true if sleep was disabled</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        public static bool DisableSleep()
        {
            bool ret = false;
            NativeMethods.EXECUTION_STATE newExecutionState;

            // This function returns the last state (not the state you are setting) - so call again to make sure it is set.
            // The retry is not really necessary.
            for (int i = 0; i < s_MaxSleepStateRetries; i++)
            {
                newExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS |
                    NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED);
                
                if ((newExecutionState & NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED) ==
                    NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED)
                {
                    ret = true;
                    break;
                }
                else
                {
                    if (i > 0)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "Failed to disable Sleep/Hibernate, attempt {0}, new state is {1}",
                            (i + 1),
                            newExecutionState));
                    }
                }
            }

            if (ret)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Sleep/Hibernate disabled");
            }
            else
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Failed to disable Sleep/Hibernate");
            }

            return ret;
        }

        /// <summary>
        /// Allows the computer to enter sleep or hibernate (called after DisableSleep)
        /// </summary>
        /// <returns>true if sleep was enabled</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        public static bool EnableSleep()
        {
            bool ret = false;
            NativeMethods.EXECUTION_STATE newExecutionState;

            // It seems that this function needs to be called twice before the
            // new value will take...
            for (int i = 0; i < s_MaxSleepStateRetries; i++)
            {
                newExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS);
                
                if ((newExecutionState & NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED) !=
                    NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED)
                {
                    ret = true;
                    break;
                }
                else
                {
                    if (i > 0)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "Failed to enable Sleep/Hibernate, attempt {0}, new state is {1}",
                            (i + 1),
                            newExecutionState));
                    }
                }
            }

            if (ret)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Sleep/Hibernate enabled");
            }
            else
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "Failed to enable Sleep/Hibernate");
            }

            return ret;
        }
    }
}
