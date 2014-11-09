using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdemyDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var username = AppSettings.Username;
            var password = AppSettings.Password;
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args);

            if (!result.Errors.Any())
            {
                if (result.Value.Username.IsNotEmpty())
                {
                    username = result.Value.Username;
                }

                if (result.Value.Password.IsNotEmpty())
                {
                    password = result.Value.Password;
                }

                string downloadFolderPath = result.Value.DownloadFolderPath.IsNotEmpty() ? result.Value.DownloadFolderPath : string.Empty;

                string courseLink = result.Value.CourseUrl;

                var udemyDownloader = new UdemyDownloader(username, password, downloadFolderPath);

                udemyDownloader.Add(courseLink);

                udemyDownloader.ProcessDownloadJobStarted += ProcessDownloadJobStarted;
                udemyDownloader.ProcessDownloadJobFinished += ProcessDownloadJobFinished;
                udemyDownloader.ProcessFileStarted += ProcessFileStarted;
                udemyDownloader.Start();
            }
            else
            {
                Console.WriteLine("Usage: udemydl [-u username] [-p password] -c course_link");
            }


        }

        private static void ProcessFileStarted(object sender, EventArgs eventArgs)
        {
            var job = sender as DownloadJob;
            if (job != null)
            {
                var currentFile = job.CurrentProcessingFile;
                if (currentFile.CourseContent != null)
                {
                    Console.WriteLine("  - Downloading Lecture {0}: {1} and saved as {2}", 
                        currentFile.CourseContent.LectureIndex, currentFile.CourseContent.Title, currentFile.FullName);
                }
                else
                {
                    Console.WriteLine("  - Downloading {0}", currentFile.FullName);                    
                }
            }
        }

        private static void ProcessDownloadJobFinished(object sender, EventArgs eventArgs)
        {
            var job = sender as DownloadJob;
            if (job != null)
            {
                Console.WriteLine("Finished processing for downloading content of course: {0} from {1}.", job.CourseInfo.Title, job.Url);
            }
        }

        private static void ProcessDownloadJobStarted(object sender, EventArgs eventArgs)
        {
            var job = sender as DownloadJob;
            if (job != null)
            {
                Console.WriteLine("Content will be saved into {0}.", job.SavePath);
                Console.WriteLine("Processing for downloading content of course: {0} from {1}... ", job.CourseInfo.Title, job.Url);
            }
        }
    }
}
