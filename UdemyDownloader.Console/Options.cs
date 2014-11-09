using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace UdemyDownloader
{
    class Options
    {
        [Option('u', "username", Required = false, HelpText = "Username of Udemy account")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, HelpText = "Password of Udemy account")]
        public string Password { get; set; }

        [Option('d', "dir", Required = false, HelpText = "Download directory of file to be saved")]
        public string DownloadFolderPath { get; set; }

        [Option('c', "course", Required = true, HelpText = "Url of Udemy course")]
        public string CourseUrl { get; set; }

    }
}

