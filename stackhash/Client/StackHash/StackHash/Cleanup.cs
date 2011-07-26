using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using StackHashUtilities;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Stores a list of files and folders that should be removed
    /// </summary>
    public sealed class Cleanup
    {
        private static readonly Cleanup _list = new Cleanup();

        private object _lock;
        private StringCollection _paths;
        private string _persistPath;

        private const string SettingsFolder = "StackHash";
        private const string CleanupFile = "Cleanup.xml";
        private const string ElementCleanup = "StackHashCleanup";
        private const string ElementPath = "Path";

        /// <summary>
        /// Gets the singleton cleanup object
        /// </summary>
        public static Cleanup List
        {
            get { return Cleanup._list; }
        }

        /// <summary>
        /// Add a path to the cleanup list - files and directories 
        /// on this list will be removed at application shutdown and
        /// startup
        /// </summary>
        /// <param name="path">Path to add</param>
        public void AddPath(string path)
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    _paths.Add(path);
                }
            }
        }

        /// <summary>
        /// Attempts to delete all of the files and directories in the 
        /// list of cleanup paths
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DeleteAll()
        {
            lock (_lock)
            {
                // do nothing if no paths to remove
                if (_paths.Count == 0)
                {
                    return;
                }

                try
                {
                    StringCollection removedPaths = new StringCollection();

                    foreach (string path in _paths)
                    {
                        bool removed = false;

                        try
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }

                            // if we get this far the path should have been removed
                            removed = true;
                        }
                        catch (Exception ex)
                        {
                            DiagnosticsHelper.LogException(DiagSeverity.Warning,
                                string.Format(CultureInfo.InvariantCulture,
                                "Cleanup: failed to delete {0}",
                                path),
                                ex);
                        }

                        if (removed)
                        {
                            removedPaths.Add(path);
                        }
                    }

                    // remove successfully deleted paths from the path list
                    foreach (string removedPath in removedPaths)
                    {
                        _paths.Remove(removedPath);
                    }

                    // save the updated list
                    Save();
                }
                catch (Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "DeleteAll Failed",
                        ex);
                }
            }
        }

        /// <summary>
        /// Saves the current list of cleanup paths
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Save()
        {
            lock (_lock)
            {
                try
                {
                    XmlWriterSettings writerSettings = new XmlWriterSettings();
                    writerSettings.CheckCharacters = true;
                    writerSettings.CloseOutput = true;
                    writerSettings.ConformanceLevel = ConformanceLevel.Document;
                    writerSettings.Indent = true;

                    using (XmlWriter writer = XmlWriter.Create(_persistPath))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement(ElementCleanup);

                        foreach (string path in _paths)
                        {
                            writer.WriteStartElement(ElementPath);
                            writer.WriteString(path);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
                catch (Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "Failed to save Cleanup list",
                        ex);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Load()
        {
            lock (_lock)
            {
                _paths.Clear();

                try
                {
                    if (File.Exists(_persistPath))
                    {
                        XmlReaderSettings readerSettings = new XmlReaderSettings();
                        readerSettings.CheckCharacters = true;
                        readerSettings.CloseInput = true;
                        readerSettings.ConformanceLevel = ConformanceLevel.Document;
                        readerSettings.IgnoreComments = true;
                        readerSettings.IgnoreWhitespace = true;

                        using (XmlReader reader = XmlReader.Create(_persistPath))
                        {
                            while (reader.Read())
                            {
                                if (reader.Name == ElementPath)
                                {
                                    _paths.Add(reader.ReadString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagnosticsHelper.LogException(DiagSeverity.Warning,
                        "Failed to load Cleanup list",
                        ex);
                }
            }
        }

        private Cleanup()
        {
            _paths = new StringCollection();
            _lock = new object();

            // construct persist path
            string persistFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SettingsFolder);
            if (!Directory.Exists(persistFolder))
            {
                Directory.CreateDirectory(persistFolder);
            }
            _persistPath = Path.Combine(persistFolder, CleanupFile);

            // load 
            Load();
        }
    }
}
