using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace UdemyDownloader
{
    /// <summary>
    /// Udemy Downloader Console client
    /// </summary>
    public class UdemyDownloader
    {
        #region Events handling
        /// <summary>
        /// Delegate to invoke when processing for a file.
        /// </summary>
        public EventHandler ProcessFileStarted;

        /// <summary>
        /// Delegate to invoke when processing for a file has been completed.
        /// </summary>
        public EventHandler ProcessFileCompleted;

        /// <summary>
        /// Delegate to invoke when processing for a file failures.
        /// </summary>
        public EventHandler ProcessFileFailed;

        /// <summary>
        /// Delegate to invoke when processing for a download job.
        /// </summary>
        public EventHandler ProcessDownloadJobStarted;

        /// <summary>
        /// Delegate to invoke when processing for a download job has been completed.
        /// </summary>
        public EventHandler ProcessDownloadJobFinished;

        void downloadJob_DownloadFinished(object sender, EventArgs e)
        {
            if (this.ProcessDownloadJobFinished != null)
            {
                this.ProcessDownloadJobFinished(sender, e);
            }
        }

        void downloadJob_DownloadStarted(object sender, EventArgs e)
        {
            if (this.ProcessDownloadJobStarted != null)
            {
                this.ProcessDownloadJobStarted(sender, e);
            }
        }

        void downloadJob_DownloadFileFinished(object sender, EventArgs e)
        {
            if (this.ProcessFileCompleted != null)
            {
                this.ProcessFileCompleted(sender, e);
            }
        }

        void downloadJob_DownloadFileStarted(object sender, EventArgs e)
        {
            if (this.ProcessFileStarted != null)
            {
                this.ProcessFileStarted(sender, e);
            }
        }
        #endregion

        public Session Session { get; set; }

        public List<DownloadJob> Jobs { get; set; }

        public string DefaultSavePath { get; set; }

        public UdemyDownloader(string username, string password, string defaultSavePath = "")
        {
            this.Jobs = new List<DownloadJob>();
            this.Session = new Session(username, password);
            if (defaultSavePath.IsEmpty())
            {
                this.DefaultSavePath = Environment.CurrentDirectory;
            }
            else
            {
                this.DefaultSavePath = Path.GetFullPath(defaultSavePath);
            }
        }

        public void Start()
        {
            this.Session.Login();
            foreach (var downloadJob in Jobs)
            {
                downloadJob.DownloadFinished += downloadJob_DownloadFinished;
                downloadJob.DownloadStarted += downloadJob_DownloadStarted;
                downloadJob.DownloadFileStarted += downloadJob_DownloadFileStarted;
                downloadJob.DownloadFileFinished += downloadJob_DownloadFileFinished;
                downloadJob.Run();
            }
        }

        public void Add(string courseLink, string savePath = null)
        {
            if (savePath.IsEmpty())
            {
                savePath = this.DefaultSavePath;
            }
            this.Jobs.Add(new DownloadJob(this, courseLink, savePath));
        }
    }
}
