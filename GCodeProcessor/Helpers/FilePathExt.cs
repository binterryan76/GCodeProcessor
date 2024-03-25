using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StringHelpers;

namespace GCodeProcessor.Helpers
{
    internal static class FilePathExt
    {
        /// <summary>
        /// Appends strToAppendToFilePath to filePath but leaves the file extension unmodified.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="strToAppendToFilePath"></param>
        /// <returns></returns>
        public static string AppendToFileName(this string filePath, string strToAppendToFilePath)
        {
            if (filePath.IsNullOrWhitespace())
                return filePath;

            string? dir = Path.GetDirectoryName(filePath);

            if (dir == null) 
                return filePath;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExt = Path.GetExtension(filePath);

            return Path.Combine(dir, fileName + strToAppendToFilePath + fileExt);
        }

        /// <summary>
        /// Returns the next available file path by appending a number to the file name.
        /// https://stackoverflow.com/questions/1078003/how-would-you-make-a-unique-filename-by-adding-a-number
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MakeUnique(this string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);
            string uniquePath = path;

            for (int i = 1; ; ++i)
            {
                if (!File.Exists(uniquePath))
                    return uniquePath;

                uniquePath = Path.Combine(dir, fileName + " " + i + fileExt);
            }
        }
    }
}
