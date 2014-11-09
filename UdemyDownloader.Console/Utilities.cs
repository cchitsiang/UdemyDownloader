using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UdemyDownloader
{
    public static class Utilities
    {
       private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

       private static readonly char[] InvalidFilePathChars = Path.GetInvalidPathChars();

        [DebuggerHidden]
        public static string NormalizeFilePath(string filePath, char replace = '_')
        {
            var path = MakeSafePath(Path.GetDirectoryName(filePath.Substring(0, filePath.LastIndexOf('\\') + 1)));

            var fileName = MakeSafeFileName(filePath.Substring(filePath.LastIndexOf('\\') + 1));

            if (fileName.Length == 0)
                throw new ArgumentException();

            filePath = Path.Combine(path,fileName);

            if (filePath.Length > 245)
                throw new PathTooLongException();

            return filePath;
        }

        public static string MakeSafePath(string filePath, char replace = '_')
        {
            filePath = new string(filePath.Select(ch => InvalidFilePathChars.Contains(ch) ? '_' : ch).ToArray());
            return filePath;
        }

        public static string MakeSafeFileName(string fileName, char replace = '_')
        {
            // In case the path has a question mark
            fileName = fileName.TrimEnd('?');

            fileName = new string(fileName.Select(ch => InvalidFileNameChars.Contains(ch) ? '_' : ch).ToArray());

            return fileName;
        }
    }
}
