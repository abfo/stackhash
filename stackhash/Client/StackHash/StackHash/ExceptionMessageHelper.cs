using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using StackHashUtilities;

namespace StackHash
{
    /// <summary>
    /// Helper class for working with exception messages
    /// </summary>
    public class ExceptionMessageHelper : IDisposable
    {
        private static readonly ExceptionMessageHelper _helper = new ExceptionMessageHelper();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr _ntDllHandle;
        private bool _disposed;

        /// <summary>
        /// Gets the ExceptionMessageHelper instance
        /// </summary>
        public static ExceptionMessageHelper Helper
        {
            get { return ExceptionMessageHelper._helper; }
        }

        /// <summary>
        /// Attempts to retreive the message associated with an exception code
        /// </summary>
        /// <param name="exceptionCode">exception code</param>
        /// <returns>The message for exception code or an empty string if no message
        /// can be retrieved</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public string GetMessage(uint exceptionCode)
        {
            string message = string.Empty;

            if (_disposed)
            {
                throw new ObjectDisposedException("ExceptionMessageHelper");
            }

            // return an empty string for 0
            if (exceptionCode == 0)
            {
                return string.Empty;
            }

            // CLR exception not included so manually detect
            if (exceptionCode == 0xE0434F4D)
            {
                return Properties.Resources.ClrException;
            }

            // access violation message unhelpful so substitute our own
            if (exceptionCode == 0xC0000005)
            {
                return Properties.Resources.AccessViolation;
            }

            // see http://support.microsoft.com/kb/259693

            if (_ntDllHandle == IntPtr.Zero)
            {
                _ntDllHandle = NativeMethods.LoadLibrary("NTDLL.DLL");

                if (_ntDllHandle == IntPtr.Zero)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning,
                        string.Format(CultureInfo.InvariantCulture,
                        "ExceptionMessageHelper failed to load NTDLL.DLL: {0}",
                        Marshal.GetLastWin32Error()));
                }
            }

            if (_ntDllHandle != IntPtr.Zero)
            {
                IntPtr messageBuffer = IntPtr.Zero;

                uint numChars = NativeMethods.FormatMessage(NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_ALLOCATE_BUFFER |
                    NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_IGNORE_INSERTS |
                    NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_FROM_HMODULE |
                    NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_FROM_SYSTEM,
                    _ntDllHandle,
                    exceptionCode,
                    0,
                    out messageBuffer,
                    0,
                    IntPtr.Zero);

                if (numChars > 0)
                {
                    if (messageBuffer != IntPtr.Zero)
                    {
                        message = Marshal.PtrToStringAnsi(messageBuffer);
                        message = message.Replace("\r", " ");
                        message = message.Replace("\n", " ");
                    }
                }
                else
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning,
                        string.Format(CultureInfo.InvariantCulture,
                        "ExceptionMessageHelper FormatMessage failed: {0}",
                        Marshal.GetLastWin32Error()));
                }

                if (messageBuffer != IntPtr.Zero)
                {
                    messageBuffer = NativeMethods.LocalFree(messageBuffer);
                    if (messageBuffer != IntPtr.Zero)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning,
                            string.Format(CultureInfo.InvariantCulture,
                            "ExceptionMessageHelper LocalFree failed: {0}",
                            Marshal.GetLastWin32Error()));
                    }
                }
            }

            return message;
        }

        private ExceptionMessageHelper()
        {
            
        }

        #region IDisposable Members

        /// <summary />
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                _disposed = true;
            }
        }

        /// <summary />
        ~ExceptionMessageHelper()
        {
            Dispose(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "canDisposeManagedResources")]
        private void Dispose(bool canDisposeManagedResources)
        {
            if (_ntDllHandle != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(_ntDllHandle);
                _ntDllHandle = IntPtr.Zero;
            }
        }

        #endregion
    }
}
