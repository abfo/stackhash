using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using StackHashUtilities;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using StackHash.StackHashService;

namespace StackHash
{
    /// <summary>
    /// The architecture of a crash dump
    /// </summary>
    public enum DumpArchitecture
    {
        /// <summary>
        /// the architecture is unknown
        /// </summary>
 
        Unknown,

        /// <summary>
        /// 32-bit (Intel)
        /// </summary>
        X86,

        /// <summary>
        /// 64-bit (Intel)
        /// </summary>
        X64,

        /// <summary>
        /// 64-bit (Itanium)
        /// </summary>
        IA64
    }

    /// <summary>
    /// Misc client utiltiy functions
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class ClientUtils
    {
        private static readonly Regex ValidEmailRegex = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ValidEmailEnvelopeRegex = new Regex(@"^.*<[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}>$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly char[] ValidEmailSeps = new char[] { ',' };
        private static readonly byte[] ClientUtilsEntropy = new byte[] { 75, 22, 101, 4, 12, 34, 98, 72 };
        private const char SearchPathSeparator = ';';
        private const string Cdb32on32Default = @"C:\Program Files\Debugging Tools for Windows\cdb.exe";
        private const string Cdb32on32Default2 = @"C:\Program Files\Debugging Tools for Windows (x86)\cdb.exe";
        private const string Cdb32on64Default = @"C:\Program Files (x86)\Debugging Tools for Windows (x86)\cdb.exe";
        private const string Cdb64on64Default = @"C:\Program Files\Debugging Tools for Windows\cdb.exe";
        private const string Cdb64on64Default2 = @"C:\Program Files\Debugging Tools for Windows (x64)\cdb.exe";
        private const string DdkFolder = @"WinDDK";

        /// <summary>
        /// Standard brush for highlighting mouse over on charts
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static SolidColorBrush ChartHighlightBrush = Brushes.Gold;

        /// <summary>
        /// Determines if an email address (or list of email addresses) is valid
        /// </summary>
        /// <param name="email"></param>
        /// <param name="allowList"></param>
        /// <returns></returns>
        public static bool IsEmailValid(string email, bool allowList)
        {
            bool valid = false;

            if (!string.IsNullOrEmpty(email))
            {
                if (allowList)
                {
                    // split and validate each email in the list
                    string[] emails = email.Split(ValidEmailSeps, StringSplitOptions.RemoveEmptyEntries);
                    if ((emails != null) && (emails.Length > 0))
                    {
                        foreach (string splitEmail in emails)
                        {
                            valid = IsEmailValid(splitEmail, false);
                            if (!valid)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    string trimmedEmail = email.Trim();
                    if ((ValidEmailRegex.IsMatch(trimmedEmail)) ||
                        (ValidEmailEnvelopeRegex.IsMatch(trimmedEmail)))
                    {
                        valid = true;
                    }
                }
            }

            return valid;
        }

        /// <summary>
        /// Attempts to locate cdb.exe for a supported architecture
        /// </summary>
        /// <param name="architecture">ImageFileMachine.AMD64 or ImageFileMachine.I386</param>
        /// <returns>Full path to cdb.exe or null if cdb.exe cannot be found</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cdb")]
        public static string GetCdbPath(ImageFileMachine architecture)
        {
            string path = null;

            switch (architecture)
            {
                case ImageFileMachine.AMD64:
                    if (ClientUtils.VerifyArchitecture(architecture, Cdb64on64Default))
                    {
                        path = Cdb64on64Default;
                    }
                    else if (ClientUtils.VerifyArchitecture(architecture, Cdb64on64Default2))
                    {
                        path = Cdb64on64Default2;
                    }
                    else
                    {
                        path = SearchDdkDirForCdb(architecture);
                    }
                    break;

                case ImageFileMachine.I386:
                    if (SystemInformation.Is64BitSystem())
                    {
                        if (ClientUtils.VerifyArchitecture(architecture, Cdb32on64Default))
                        {
                            path = Cdb32on64Default;
                        }
                        else
                        {
                            path = SearchDdkDirForCdb(architecture);
                        }
                    }
                    else
                    {
                        if (ClientUtils.VerifyArchitecture(architecture, Cdb32on32Default))
                        {
                            path = Cdb32on32Default;
                        }
                        else if (ClientUtils.VerifyArchitecture(architecture, Cdb32on32Default2))
                        {
                            path = Cdb32on32Default2;
                        }
                        else
                        {
                            path = SearchDdkDirForCdb(architecture);
                        }
                    }
                    break;

                case ImageFileMachine.IA64:
                default:
                    // not supported
                    break;
            }

            return path;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static string SearchDdkDirForCdb(ImageFileMachine architecture)
        {
            string path = null;

            try
            {
                string ddkFolder = System.IO.Path.Combine(System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)), DdkFolder);
                if (Directory.Exists(ddkFolder))
                {
                    DirectoryInfo ddkFolderInfo = new DirectoryInfo(ddkFolder);
                    FileInfo[] candidates = ddkFolderInfo.GetFiles("cdb.exe", SearchOption.AllDirectories);

                    if (candidates.Length > 0)
                    {
                        for (int i = candidates.Length - 1; i >= 0; i--)
                        {
                            if (ClientUtils.VerifyArchitecture(architecture, candidates[i].FullName))
                            {
                                path = candidates[i].FullName;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning,
                    "SearchDdkDirForCdb Failed",
                    ex);
            }

            return path;
        }

        /// <summary>
        /// Converts a StackHashSearchPath to a string
        /// </summary>
        /// <param name="searchPath">StackHashSearchPath</param>
        /// <returns>string representation of the StackHashSearchPath</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static string SearchPathToString(StackHashSearchPath searchPath)
        {
            string s = string.Empty;

            if ((searchPath != null) && (searchPath.Count > 0))
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < searchPath.Count; i++)
                {
                    sb.Append(searchPath[i]);

                    if (i < (searchPath.Count - 1))
                    {
                        sb.Append(SearchPathSeparator);
                    }
                }

                s = sb.ToString();
            }

            return s;
        }

        /// <summary>
        /// Converts a symbol or image search path (semicolon separated) to a StackHashSearchPath
        /// </summary>
        /// <param name="s">Search path string</param>
        /// <returns>StackHashSearchPath</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static StackHashSearchPath StringToSearchPath(string s)
        {
            StackHashSearchPath searchPath = new StackHashSearchPath();

            if (!string.IsNullOrEmpty(s))
            {
                string[] pathElements = s.Split(new char[] { SearchPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string element in pathElements)
                {
                    searchPath.Add(element);
                }
            }

            return searchPath;
        }

        /// <summary>
        /// Determines if the original source of an event is a ListViewItem, used to 
        /// detect if a ListView double-click should be responded to or not
        /// </summary>
        /// <param name="originalSource">OriginalSource from an event</param>
        /// <returns>True if the original source is a list view item</returns>
        public static bool OriginalSourceIsListViewItem(object originalSource)
        {
            bool isListViewItem = false;

            DependencyObject dependencyObject = originalSource as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ListViewItem)
                {
                    isListViewItem = true;
                    break;
                }
                else if (dependencyObject is ListView)
                {
                    // if we get as far as a ListView without finding a ListViewItem we can stop
                    break;
                }

                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            return isListViewItem;
        }

        /// <summary>
        /// Gets a string describing the rules for a profile name (as validated by IsValidSqlIdentifier)
        /// </summary>
        public static string ProfileNameRulesText
        {
            get
            {
                return Properties.Resources.ProfileNameRules;
            }
        }

        /// <summary>
        /// Gets a small shield icon as a BitmapSource
        /// </summary>
        /// <returns>BitmapSource containing small shield icon</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static BitmapSource GetShieldIconAsBitmapSource()
        {
            BitmapSource shieldSource = null;
            IntPtr hIcon = IntPtr.Zero;

            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    // Windows Vista / 2008 or later, get the stock icon
                    NativeMethods.SHSTOCKICONINFO sii = new NativeMethods.SHSTOCKICONINFO();
                    sii.cbSize = (UInt32) Marshal.SizeOf(typeof(NativeMethods.SHSTOCKICONINFO));

                    Marshal.ThrowExceptionForHR(NativeMethods.SHGetStockIconInfo(NativeMethods.SHSTOCKICONID.SIID_SHIELD,
                        NativeMethods.SHGSI.SHGSI_ICON | NativeMethods.SHGSI.SHGSI_SMALLICON,
                        ref sii));

                    hIcon = sii.hIcon;

                    shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                else
                {
                    // Older platform, use SystemIcons.Shield... can't use this on Win 7 because it returns the wrong icon
                    shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Shield.Handle,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning,
                    "GetShieldIconAsBitmapSource failed",
                    ex);
            }
            finally
            {
                // free icon handle if one was created
                if (hIcon != IntPtr.Zero)
                {
                    NativeMethods.DestroyIcon(hIcon);
                }
            }

            return shieldSource;                    
        }

        /// <summary>
        /// Makes a safe directory name by replacing any invalid characters with an underscore
        /// </summary>
        /// <param name="name">The name to make safe</param>
        /// <returns>The safe directory name</returns>
        public static string MakeSafeDirectoryName(string name)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (name.Length == 0) { throw new ArgumentException("name must not be zero length"); }

            StringBuilder sb = new StringBuilder(name.Length);

            List<char> badchars = new List<char>();
            badchars.AddRange(System.IO.Path.GetInvalidPathChars());
            badchars.AddRange(System.IO.Path.GetInvalidFileNameChars());
            bool isbadchar = false;

            for (int i = 0; i < name.Length; i++)
            {
                isbadchar = false;

                foreach(char bad in badchars)
                {
                    if (name[i] == bad)
                    {
                        isbadchar = true;
                        break;
                    }
                }

                if (isbadchar)
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(name[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="toEncrypt">The string to encrypt</param>
        /// <returns>Encrypted version of the string</returns>
        public static string EncryptString(string toEncrypt)
        {
            Debug.Assert(!string.IsNullOrEmpty(toEncrypt));

            byte[] toEncryptBytes = Encoding.Unicode.GetBytes(toEncrypt);
            byte[] encryptedBytes = ProtectedData.Protect(toEncryptBytes, ClientUtilsEntropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes, Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Decrypts a string
        /// </summary>
        /// <param name="toDecrypt">The string to decrypt</param>
        /// <returns>Decrypted version of the string</returns>
        public static string DecryptString(string toDecrypt)
        {
            Debug.Assert(!string.IsNullOrEmpty(toDecrypt));

            byte[] toDecryptBytes = Convert.FromBase64String(toDecrypt);
            byte[] decryptedBytes = ProtectedData.Unprotect(toDecryptBytes, ClientUtilsEntropy, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(decryptedBytes);
        }

        /// <summary>
        /// Verifies that an EXE of DLL is of an expected architecture (32 or 64-bit)
        /// </summary>
        /// <remarks>
        /// See http://stackoverflow.com/questions/1001404/check-if-unmanaged-dll-is-32-bit-or-64-bit
        /// </remarks>
        /// <param name="architecture">Architecture to verify</param>
        /// <param name="exePath">Path to EXE or DLL</param>
        /// <returns>True if exePath matches architecture</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool VerifyArchitecture(ImageFileMachine architecture, string exePath)
        {
            bool matches = false;

            if (exePath == null)
            {
                return false;
            }

            try
            {
                if (File.Exists(exePath))
                {
                    int machineType = 0;

                    using (FileStream fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader br = new BinaryReader(fs);

                        fs.Seek(0x3c, SeekOrigin.Begin);
                        int peOffset = br.ReadInt32();
                        fs.Seek(peOffset, SeekOrigin.Begin);
                        uint peHead = br.ReadUInt32();
                        if (peHead == 0x00004550)
                        {
                            machineType = br.ReadUInt16();
                        }
                    }

                    switch (architecture)
                    {
                        case ImageFileMachine.AMD64:
                            matches = machineType == 0x8664;
                            break;

                        case ImageFileMachine.I386:
                            matches = machineType == 0x14c;
                            break;

                        default:
                            // not supported
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning,
                    "VerifyArchitecture Failed",
                    ex);
            }

            return matches;
        }

        /// <summary>
        /// Attempts to get the architecture of a crash dump from the Version.txt file
        /// </summary>
        /// <param name="versionFilePath">Full path to Version.txt</param>
        /// <returns>DumpArchitecture</returns>
        public static DumpArchitecture GetArchitectureFromVersionFile(string versionFilePath)
        {
            Debug.Assert(versionFilePath != null);
            Debug.Assert(File.Exists(versionFilePath));

            DumpArchitecture arch = DumpArchitecture.Unknown;

            char[] splitChars = new char[] {' ', ':'};

            // looking for (i.e.) "Architecture: X64"
            using (StreamReader sr = File.OpenText(versionFilePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] bits = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (bits.Length >= 2)
                    {
                        // found the architecture line
                        if (string.Compare("Architecture", bits[0], StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (string.Compare("X86", bits[1], StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                arch = DumpArchitecture.X86;
                            }
                            else if (string.Compare("X64", bits[1], StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                arch = DumpArchitecture.X64;
                            }
                            else if (string.Compare("IA64", bits[1], StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                arch = DumpArchitecture.IA64;
                            }
                            else
                            {
                                DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                                    string.Format(CultureInfo.InvariantCulture,
                                    "GetArchitectureFromVersionFile: Unknown Architecture: {0}",
                                    bits[1]));
                            }

                            // break out of the while loop
                            break;
                        }
                    }
                }
            }

            return arch;
        }
    }
}
