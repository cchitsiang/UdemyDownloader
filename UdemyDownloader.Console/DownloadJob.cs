using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace UdemyDownloader
{
    public class DownloadJob : Downloader
    {
        private const string COURSE_INFO_API = "https://www.udemy.com/api-1.1/courses/{0}";
        private const string COURSE_CURRICULUM_API = "https://www.udemy.com/api-1.1/courses/{0}/curriculum?fields[lecture]=@all&fields[asset]=@all";

        private readonly UdemyDownloader _client;
        private string _courseId;
        private int _currentFileIndex;

        public bool IsCancelled { get; set; }
        public Course CourseInfo { get; set; }
        public CourseCurriculum CurriculumInfo { get; set; }
        public List<DownloadableContent> DownloadableContents { get; set; }
        public int ErrorFilesCount { get; set; }

        public DownloadableContent CurrentProcessingFile
        {
            get
            {
                return this.DownloadableContents[_currentFileIndex];
            }
        }

        /// <summary>
        /// Occurs when the download progress of the file has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Occurs when the download is starts.
        /// </summary>
        public event EventHandler DownloadFileStarted;

        /// <summary>
        /// Occurs when the download finished.
        /// </summary>
        public event EventHandler DownloadFileFinished;

        protected void OnDownloadFileStarted(EventArgs e)
        {
            if (this.DownloadFileStarted != null)
            {
                this.DownloadFileStarted(this, e);
            }
        }

        protected void OnDownloadFileFinished(EventArgs e)
        {
            if (this.DownloadFileFinished != null)
            {
                this.DownloadFileFinished(this, e);
            }
        }

        public DownloadJob(UdemyDownloader client, string courseLink, string savePath)
            : base(courseLink, savePath, null)
        {
            _client = client;
            DownloadableContents = new List<DownloadableContent>();
        }

        public void Run()
        {
            this.ErrorFilesCount = 0;

            this._courseId = GetCourseId();

            this.CourseInfo = GetCourseInfo(_courseId);

            this.CurriculumInfo = GetCurriculumInfo(_courseId);

            this.ProcessDownloadableContents();

            this.Execute();
        }

        /// <summary>
        /// Get course id from course detail page, and match with data-courseId 
        /// </summary>
        /// <returns></returns>
        private string GetCourseId()
        {
            var responseHtml = this._client.Session.Get(this.Url);
            var matches = Regex.Matches(responseHtml, @"data-courseId=""(\d+)""");
            return matches[0].Groups[1].Value;
        }

        private Course GetCourseInfo(string courseId)
        {
            var json = this._client.Session.Get(COURSE_INFO_API.ToFormat(courseId));
            var courseInfo = JsonConvert.DeserializeObject<Course>(json);
            return courseInfo;
        }

        private CourseCurriculum GetCurriculumInfo(string courseId)
        {
            var json = this._client.Session.Get(COURSE_CURRICULUM_API.ToFormat(courseId));
            var assets = JsonConvert.DeserializeObject<List<CourseContent>>(json);
            var lectures = new List<CourseContent>();
            var chapters = new Dictionary<int, string>();
            var downloadableResources = new List<Asset>();
            foreach (var asset in assets)
            {
                if (asset.Type.ToLower().Equals("chapter"))
                {
                    chapters.Add(asset.ChapterIndex, asset.Title);
                }
                else if (asset.Type.ToLower().Equals("lecture"))
                {
                    if (AppSettings.IncludeDownloadableResource && asset.Extras != null)
                    {
                        //At this moment, we only care for Asset type
                        foreach (var extra in asset.Extras)
                        {
                            var jsonString = extra.ToString();
                            if (jsonString.IsNotEmpty() && jsonString.Contains("asset"))
                            {
                                var resource = JsonConvert.DeserializeObject<Asset>(jsonString);
                                downloadableResources.Add(resource);
                            }

                        }
                    }
                    lectures.Add(asset);
                }
            }
            return new CourseCurriculum()
            {
                Chapters = chapters,
                Lectures = lectures,
                DownloadableResources = downloadableResources
            };
        }

        private void ProcessDownloadableContents()
        {
            int count = 1;

            if (AppSettings.IncludeDownloadableResource)
            {
                foreach (var resource in this.CurriculumInfo.DownloadableResources)
                {
                    this.DownloadableContents.Add(new DownloadableContent()
                    {
                        Asset = resource,
                        DownloadUrl = resource.DownloadUrl.Download,
                        SavedFolder = this.SavePath,
                        Title = resource.Title.ToLower().Replace(' ', '-'),
                        FileExtension = Path.GetExtension(resource.Data.Name)
                    });
                }
            }

            foreach (var lecture in this.CurriculumInfo.Lectures)
            {
                AssetType assetType;
                if (!Enum.TryParse(lecture.AssetType, out assetType))
                {
                    assetType = AssetType.Invalid;
                }

                if(assetType == AssetType.Invalid)
                    continue;

                //throw new Exception("Cannot determine what type of file to download.");

                //currently we are not interested in Article asset
                if (assetType != AssetType.Article)
                {
                    var chapter = this.CurriculumInfo.Chapters[lecture.ChapterIndex - 1];
                    this.DownloadableContents.Add(new DownloadableContent()
                    {
                        CourseContent = lecture,
                        Asset = lecture.Asset,
                        DownloadUrl = lecture.Asset.DownloadUrl.Download,
                        Title = string.Format("{0}. {1}", count, lecture.Title),
                        FileExtension = Path.GetExtension(lecture.Asset.Data.Name),
                        SavedFolder = Path.Combine(this.SavePath, string.Format("{0} {1}", lecture.ChapterIndex, chapter))
                    });
                }

                count++;
            }
        }

        /// <summary>
        /// Starts the content download.
        /// </summary>
        /// <exception cref="IOException">The content could not be saved.</exception>
        /// <exception cref="WebException">An error occured while downloading the content.</exception>
        public override void Execute()
        {
            this._currentFileIndex = 0;
            this.OnDownloadStarted(EventArgs.Empty);

            foreach (var downloadableContent in DownloadableContents)
            {
                try
                {
                    this.OnDownloadFileStarted(EventArgs.Empty);
                    this.DownloadContent(downloadableContent);
                    this.OnDownloadFileFinished(EventArgs.Empty);
                }
                catch (Exception)
                {
                    this.ErrorFilesCount++;
                }
                finally
                {
                    this._currentFileIndex++;
                }
            }

            this.OnDownloadFinished(EventArgs.Empty);
        }

        private void DownloadContent(DownloadableContent content)
        {
            var request = (HttpWebRequest)WebRequest.Create(content.DownloadUrl);

            if (this.BytesToDownload.HasValue)
            {
                request.AddRange(0, this.BytesToDownload.Value - 1);
            }

            // the following code is alternative, you may implement the function after your needs
            using (WebResponse response = request.GetResponse())
            {
                using (Stream source = response.GetResponseStream())
                {
                    var directory = content.SavedFolder;
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var filePath = Utilities.NormalizeFilePath(Path.Combine(directory, content.FullName));
                    using (FileStream target = File.Open(filePath, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = new byte[1024];
                        bool cancel = false;
                        int bytes;
                        int copiedBytes = 0;

                        while (!cancel && (bytes = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            target.Write(buffer, 0, bytes);

                            copiedBytes += bytes;

                            var eventArgs = new ProgressEventArgs((copiedBytes * 1.0 / response.ContentLength) * 100);

                            if (this.DownloadProgressChanged != null)
                            {
                                this.DownloadProgressChanged(this, eventArgs);

                                if (eventArgs.Cancel)
                                {
                                    cancel = true;
                                    this.IsCancelled = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
