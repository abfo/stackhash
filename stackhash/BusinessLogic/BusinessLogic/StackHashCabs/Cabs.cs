using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CabLib;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Globalization;

namespace StackHashCabs
{
    public class CabinetFileInfo
    {
        public DateTime Time { get; set; }
        public String FileName { get; set; }
        public String FullPath { get; set; }
        public String Path { get; set; }
        public String RelativePath { get; set; }
        public String Subfolder { get; set; }
        public int Size { get; set; }
        public ushort FileAttributes { get; set; }

        public CabinetFileInfo(DateTime time, String file, String fullPath, String path, String relativePath, String subfolder, int size, ushort fileAttributes)
        {
            Time = time;
            FileName = file;
            FullPath = fullPath;
            Path = path;
            RelativePath = relativePath;
            Subfolder = subfolder;
            Size = size;
            FileAttributes = fileAttributes;
        }
    }

    internal class CabParser : IDisposable
    {
        private Collection<CabinetFileInfo> m_FileInfo = new Collection<CabinetFileInfo>();
        private CabLib.Extract m_CabExtract = new CabLib.Extract();
        private String m_CabFileName;

        public CabParser(CabLib.Extract cabExtract, String cabFileName)
        {
            m_CabExtract = cabExtract;
            m_CabFileName = cabFileName;
        }

        public Collection<CabinetFileInfo> FileInfo
        {
            get { return m_FileInfo;}
        }

        public bool BeforeCopyFileCallback(Extract.kCabinetFileInfo fileInfo)
        {
            CabinetFileInfo cabFileInfo = new CabinetFileInfo(fileInfo.k_Time, fileInfo.s_File, fileInfo.s_FullPath, fileInfo.s_Path, 
                fileInfo.s_RelPath, fileInfo.s_SubFolder, fileInfo.s32_Size, fileInfo.u16_Attribs);
            m_FileInfo.Add(cabFileInfo);
            return false;
        }

        public void PopulateFileData()
        {
            m_CabExtract.evBeforeCopyFile += new Extract.delBeforeCopyFile(this.BeforeCopyFileCallback);

            // Install a delegate callback to stop the actual extraction.
            m_CabExtract.ExtractFile(m_CabFileName, Path.GetTempPath());
        }


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unhook the delegate.
                m_CabExtract.evBeforeCopyFile -= new Extract.delBeforeCopyFile(this.BeforeCopyFileCallback);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public static class Cabs
    {
        public static void ExtractCab(string cabFile, string folder)
        {
            if (cabFile == null)
                throw new ArgumentNullException("cabFile");
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (!File.Exists(cabFile))
                throw new ArgumentException("File does not exist: " + cabFile, "cabFile");
            if (!Directory.Exists(folder))
                throw new ArgumentException("Folder does not exist: " + folder, "folder");

            CabLib.Extract cabExtract = new CabLib.Extract();

            try
            {
                // Now extract into subdirectory and the corresponding subdirectories in the CAB file
                cabExtract.ExtractFile(cabFile, folder);
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("The file is not a cabinet") || ex.Message.Contains("corrupt") || ex.Message.Contains("Corrupt"))
                {
                    // Must be a dud cab file. Delete it so the next resync causes a download again.
                    File.Delete(cabFile);
                }
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801")]
        public static bool IsUncabbed(string cabFile, string folder)
        {
            return false;
        }


        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static Collection<CabinetFileInfo> GetCabFiles(string cabFileName)
        {
            CabLib.Extract cabExtract = new CabLib.Extract();

            CabParser parser = new CabParser(cabExtract, cabFileName);
            try
            {
                parser.PopulateFileData();
                return parser.FileInfo;
            }
            catch (System.Exception)
            {
                return null;
            }
            finally
            {
                parser.Dispose();
            }
        }
    }
}
