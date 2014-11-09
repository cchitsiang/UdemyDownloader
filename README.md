Udemy downloader
================

*Download Udemy course files for offline use*

Usage: udemydl [-u username] [-p password] -c course_link

Note: 
- username and password can be configured in app.config, so that you no need to enter the credentials every time.

Basic description of how everything works:

1. Sign in to Udemy using POST request
    * Save Cookies after successful and use them for all future requests
2. Go the courseUrl and search for courseId which will be used in future API requests
3. Using Udemy API [(https://developers.udemy.com/)](https://developers.udemy.com/ "Udemy API")
    * grab course info
    * grab course curriculum (with full information regarding lectures and assets)
4. Download the files.

Technologies used:

Third party libraries used:

- Command Line Parser Library
- HtmlAgilityPack
- Json.NET

Next:
- Better WPF Client UI for select files / video to be download partially.
- Exposure more events for event handling each state
- Resumable download functionalities