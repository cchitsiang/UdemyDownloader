using System;
using System.Configuration;

namespace UdemyDownloader
{

    public class AppSettings
    {
        public static string Username
        {
            get
            {
                return ConfigurationManager.AppSettings["Username"];
            }
        }

        public static string Password
        {
            get
            {
                return ConfigurationManager.AppSettings["Password"];
            }
        }

        public static bool IncludeDownloadableResource
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["IncludeDownloadableResource"]);
            }
        }
    }
}
