using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StackHashUtilities
{
    public static class FileUtils
    {
        /// <summary>
        /// Reads all the lines from the specified file but checks for the unicode versus other file types.
        /// </summary>
        /// <param name="fileName">File to read from.</param>
        /// <returns>Array of lines read from the file.</returns>
        public static String[] ReadAllLinesWithTypeCheck(String fileName)
        {
            byte[] allBytes = File.ReadAllBytes(fileName);

            // Check for double byte file with no Byte Order Mark (BOM).

            if (((allBytes.Length >= 2) && (allBytes[0] == 0xff) && (allBytes[1] == 0xfe)) ||  // UTF16 LE (Little Endian)                 
                ((allBytes.Length >= 2) && (allBytes[0] == 0xff) && (allBytes[1] == 0xff)) ||  // UTF16 BE (Big Endian)
                ((allBytes.Length >= 3) && (allBytes[0] == 0xef) && (allBytes[1] == 0xbb) && (allBytes[2] == 0xbf))) // UTF8
            {
                return File.ReadAllLines(fileName);
            }
            else
            {
                bool isDoubleByte = false;
                for (int i = 1; i < allBytes.Length; i += 2)
                {
                    if (allBytes[i] == 0)
                        isDoubleByte = true;
                }

                if (isDoubleByte)
                    return File.ReadAllLines(fileName, Encoding.Unicode); // Force to Unicode.
                else 
                    return File.ReadAllLines(fileName);  // Let the OS decide.
            }
        }
    }
}
