using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Globalization;

namespace StackHashUtilities
{
    public delegate bool CopyDirectoryCallback(bool copyStarted, String currentFile);

    public static class PathUtils
    {
        /// <summary>
        /// Mark the files in the specified folder (and subfolder if specified) as writable.
        /// </summary>
        /// <param name="path">Folder to mark writable.</param>
        /// <param name="recursive">True - recursive, False - not recursive.</param>
        public static void MarkDirectoryWritable(String path, bool recursive)
        {
            if (!Directory.Exists(path))
                return;

            String[] files = Directory.GetFiles(path);

            foreach (String file in files)
            {
                FileAttributes attributes = File.GetAttributes(file);
                if ((attributes & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
            }

            if (recursive)
            {
                String [] folders = Directory.GetDirectories(path);

                foreach (String folder in folders)
                {
                    MarkDirectoryWritable(folder, recursive);
                }
            }
        }

        /// <summary>
        /// Delete a folder recursively - with retry.
        /// Retry is set to 10 seconds with retry every second.
        /// </summary>
        /// <param name="path">Folder to delete.</param>
        /// <param name="recursive">True - recursive, False - not recursive.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static void DeleteDirectory(String path, bool recursive)
        {
            bool deleteOK = true;
            Exception finalException = null;
	        int retryCount = 10;	 // Retry for 10 seconds.
	        int retrySleepMS = 1000; // Retry every 1 second.

            for (int i = 0; i < retryCount; i++)
            {
                // assume that the delete worked
                deleteOK = true;

                try
                {
                    PathUtils.SetFilesWritable(path, true);
			        Directory.Delete(path, recursive);
                }
		        catch (System.IO.DirectoryNotFoundException)
		        {
			        // Assume delete was ok - or the folder never existed in the first place.
		        }
                catch (Exception ex)
                {
                    // Track the exception.
                    finalException = ex;

                    // Flag that the delete failed.
                    deleteOK = false;
                }

                // If the delete worked then we can stop now.
                if (deleteOK)
                {
                    break;
                }

		        Thread.Sleep(retrySleepMS);
            }

            // throw the last exception if the delete failed
            if (!deleteOK)
            {
                throw finalException;
            }
        }

        /// <summary>
        /// Sets all files in the specified folder and subfolders (if required) to read/write.
        /// </summary>
        /// <param name="path">Folder to parse.</param>
        /// <param name="recursive">True - recursive, False - not recursive.</param>
        public static void SetFilesWritable(String path, bool recursive)
        {
            String[] files = Directory.GetFiles(path);

            foreach (String file in files)
            {
                FileAttributes attributes = File.GetAttributes(file);

                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attributes &= ~FileAttributes.ReadOnly;

                    File.SetAttributes(file, attributes);
                }
            }

            // Now parse subfolders if requests.
            if (recursive)
            {
                String[] folders = Directory.GetDirectories(path);

                foreach (String folder in folders)
                {
                    SetFilesWritable(folder, recursive);
                }
            }
        }

        /// <summary>
        /// Copies the specified source folder and subfolders to the specified destination folder.
        /// </summary>
        /// <param name="sourceDirectory">Source folder.</param>
        /// <param name="targetDirectory">Destination folder.</param>
        /// <param name="progress">Object to report progress to.</param>
        public static void CopyDirectory(string sourceDirectory, string targetDirectory, CopyDirectoryCallback progress)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            copyAll(diSource, diTarget, progress);
        }


        /// <summary>
        /// Copies the specified source folder and subfolders to the specified destination folder.
        /// </summary>
        /// <param name="source">Source folder.</param>
        /// <param name="target">Destination folder.</param>
        /// <param name="progress">Object that controls the abort state.</param>
        private static void copyAll(DirectoryInfo source, DirectoryInfo target, CopyDirectoryCallback progress)
        {
            // Check if the target directory exists, if not, create it.
            if (!Directory.Exists(target.FullName))
                Directory.CreateDirectory(target.FullName);

            String targetFolder = target.ToString();

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                String fullFileName = Path.Combine(targetFolder, fi.Name);

                if ((progress != null) && progress(true, fullFileName))
                    throw new OperationCanceledException("Copying folder " + source.ToString() + " to " + targetFolder);

                fi.CopyTo(fullFileName, true);

                if ((progress != null) && progress(false, fullFileName))
                    throw new OperationCanceledException("Copying folder " + source.ToString() + " to " + targetFolder);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                copyAll(diSourceSubDir, nextTargetSubDir, progress);
            }
        }

        public static String ProcessPath(String path)
        {
            if (path == null)
                return null;

            // Make sure the path is in upper case and ends with a backslash.
            String newPath = path.ToUpperInvariant();
            if (newPath[newPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                newPath += Path.DirectorySeparatorChar;
            }
            return newPath;
        }

        public static String AddDirectorySeparator(String path)
        {
            if (String.IsNullOrEmpty(path))
                return path;

            if (path[path.Length - 1] != Path.DirectorySeparatorChar)
            {
                return path + Path.DirectorySeparatorChar;
            }
            else
            {
                return path;
            }
        }

        public static String MakeValidPathElement(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // Assume each path element should follow filename rules - not pathname rules. i.e. The backslash and colon
            // should not be allowed.
            String invalidPathCharsNonEscaped = new String(Path.GetInvalidFileNameChars()) + ":";
            String invalidPathChars = Regex.Escape(invalidPathCharsNonEscaped);
            String invalidPathReStr = String.Format(CultureInfo.InvariantCulture, @"[{0}]", invalidPathChars);

            return Regex.Replace(name, invalidPathReStr, "_");
        }


        /// <summary>
        /// Gets the UNC equivalent for the specified network drive. 
        /// Returns null if the drive is not a network drive.
        /// </summary>
        /// <param name="driveLetter">Folder to translate in form "drive:" no backslash</param>
        /// <returns>Mapped path (may be longer than original) or NULL if not a network path</returns>

        public static String GetNetworkDriveUncName(String driveLetter)
        {
	        if (String.IsNullOrEmpty(driveLetter))
		        throw new ArgumentNullException(driveLetter);
        	
	        if ((driveLetter.Length != 2) || (driveLetter[1] != ':'))
		        throw new ArgumentException("DriveLetter format must be drive:");

	        int len = 260;
	        StringBuilder remoteName = new StringBuilder(len);

	        // I have seen some error codes (not connected yet) where the correct path is returned.
	        // so don't check the error code.	        
            int result = NativeMethods.WNetGetConnection(driveLetter, remoteName, ref len);

            if (result == 0)
                return remoteName.ToString();
            else
                return null;
        }

        /// <summary>
        /// Gets the UNC equivalent for the specified full drive path. 
        /// </summary>
        /// <param name="driveLetterPath">Full path of form c:\test</param>
        /// <returns>Full network path.</returns>
        public static String GetUncPath(String driveLetterPath)
        {
	        if (String.IsNullOrEmpty(driveLetterPath))
		        throw new ArgumentNullException("driveLetterPath");
	
	        if ((driveLetterPath.Length < 3) || (driveLetterPath[1] != ':'))
		        throw new ArgumentException("driveLetter format must be drive:", "driveLetterPath");

	        String driveOnly = driveLetterPath.Substring(0, 2);

            String fullPath = GetNetworkDriveUncName(driveOnly);
	        fullPath += driveLetterPath.Substring(2, driveLetterPath.Length - 2);

	        return fullPath;
        }

    
    /// <summary>
        /// Gets the UNC equivalent for the specified drive letter based path. If the path is for a 
        /// drive that is a local drive it is not changed, unless it is a substed drive
        /// in which case it is changed to the physical drive name.
        /// </summary>
        /// <param name="path">Folder translate - drive:\...</param>
        /// <returns>Mapped path (may be longer than original)</returns>

        public static String GetPhysicalPath(String path)
        {	
	        String result;

	        if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            // Get the drive letter
	        String pathRoot = Path.GetPathRoot(path);

	        if(String.IsNullOrEmpty(pathRoot))
                throw new ArgumentException("path must be rooted", "path");

	        String lpDeviceName = pathRoot.Replace("\\", "");

            String substPrefix = "\\??\\";

            StringBuilder lpTargetPath = new StringBuilder(260);

	        if (NativeMethods.QueryDosDevice(lpDeviceName, lpTargetPath, lpTargetPath.Capacity) != 0)
            {
                // If drive is substed, the result will be in the format of "\??\C:\RealPath\".
		        if (lpTargetPath.ToString().StartsWith(substPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // Strip the \??\ prefix.
                    String root = lpTargetPath.ToString().Remove(0, substPrefix.Length);
			        result = Path.Combine(root, path.Replace(Path.GetPathRoot(path), ""));
                }
                else
                {
			        result = GetNetworkDriveUncName(lpDeviceName);

			        if (String.IsNullOrEmpty(result))
			        {
				        return path;
			        }
			        else
			        {
				        result = Path.Combine(result, path.Replace(Path.GetPathRoot(path), ""));
				        return result;
			        }
                }

                return result;
            }
            else
            {
		        result = GetNetworkDriveUncName(lpDeviceName);
		        if (String.IsNullOrEmpty(result))
		        {
			        return path;
		        }
		        else
		        {
			        result = Path.Combine(result, path.Replace(Path.GetPathRoot(path), ""));
			        return result;
		        }
            }
        }
    }
}
