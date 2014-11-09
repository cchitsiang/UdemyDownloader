namespace UdemyDownloader
{
    public class DownloadableContent
    {
        public Asset Asset { get; set; }
        public CourseContent CourseContent { get; set; }
        public string DownloadUrl { get; set; }
        
        public string FullName
        {
            get
            {
                return string.Format("{0}{1}", this.Title, this.FileExtension);
            }
        }

        public string Title { get; set; }

        public string FileExtension { get; set; }

        public string SavedFolder { get; set; }
    }
}
