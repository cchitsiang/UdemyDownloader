using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UdemyDownloader
{
    public static class Utilities
    {
       private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        //[DebuggerHidden]
        public static string NormalizePath(string filePath, char replace = '_')
        {
            // In case the path has a question mark
            filePath = filePath.TrimEnd('?');

            filePath = new string(filePath.Select(ch => InvalidFileNameChars.Contains(ch) ? '_' : ch).ToArray());

            if (filePath.Length == 0)
                throw new ArgumentException();

            if (filePath.Length > 245)
                throw new PathTooLongException();

            return filePath;
        }
    }
}
