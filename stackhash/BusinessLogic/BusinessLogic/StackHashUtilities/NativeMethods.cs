using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace StackHashUtilities
{
    internal static class NativeMethods
    {

        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            // Legacy flag, should not be used.
            // ES_USER_PRESENT   = 0x00000004,
            ES_CONTINUOUS = 0x80000000,
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(
             [In] IntPtr hProcess,
             [Out] [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

    
	    [DllImport("mpr.dll", CharSet=CharSet.Unicode, SetLastError=true, BestFitMapping=false, ThrowOnUnmappableChar=true)]
        public static extern int WNetGetConnection(
		    [MarshalAs(UnmanagedType.LPWStr)] String localName,
		    [MarshalAs(UnmanagedType.LPWStr)] StringBuilder remoteName, 
		    ref int length);

	    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true, BestFitMapping=false, ThrowOnUnmappableChar=true)]
        public static extern uint QueryDosDevice(
            [MarshalAs(UnmanagedType.LPWStr)] String lpDeviceName,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpTargetPath, 
            int ucchMax);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
    }
}
