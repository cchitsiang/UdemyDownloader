using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using UdemyDownloader.Exceptions;

namespace UdemyDownloader
{
    public class Session
    {
        private const string LOGIN_URL = "https://www.udemy.com/join/login-popup";
        private const string LOGIN_POST_URL = "https://www.udemy.com/join/login-submit";
        private string _csrfToken;
        private string _setCookie;
        private string _accessToken;
        private string _clientId;

        public string Username { get; set; }
        public string Password { get; set; }
        public CookieContainer Cookies { get; set; }
        public bool IsAuthenticated { get; set; }

        public Session(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            this.Cookies = new CookieContainer();
        }

        private string GetCSRF()
        {
            var html = Get(LOGIN_URL);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return htmlDoc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='csrf']").Attributes["value"]
                    .Value;
        }

        private string BuildPostData()
        {
            NameValueCollection postData = HttpUtility.ParseQueryString(String.Empty);
            postData.Add("isSubmitted", "1");
            postData.Add("email", this.Username);
            postData.Add("password", this.Password);
            postData.Add("displayType", "json");
            postData.Add("csrf", this._csrfToken);

            return postData.ToString();
        }

        private HttpWebRequest CreateRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            if (this._clientId.IsNotEmpty())
            {
                request.Headers.Add("X-Udemy-Client-Id", this._clientId);
            }
            if (this._accessToken.IsNotEmpty())
            {
                request.Headers.Add("X-Udemy-Bearer-Token", this._accessToken);
            }
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.42 Safari/537.36";
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            return request;
        }

        public void Login()
        {
            if(this.Username.IsEmpty() || this.Password.IsEmpty())
                throw new MissingCredentialsException();

            this._csrfToken = GetCSRF();
            var request = CreateRequest(LOGIN_POST_URL);
            request.Method = WebRequestMethods.Http.Post;
            request.Accept = "application/json";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Referer = "http://www.udemy.com/";
            request.CookieContainer = this.Cookies;
            request.KeepAlive = true;

            byte[] requestBody = Encoding.ASCII.GetBytes(BuildPostData());
            request.ContentLength = requestBody.Length;
            Stream postStream = request.GetRequestStream();
            postStream.Write(requestBody, 0, requestBody.Length);
            postStream.Flush();
            postStream.Close();

            try
            {
                HttpWebResponse resp;
                using (resp = (HttpWebResponse) request.GetResponse())
                {
                    //TODO checking whether has {"returnUrl":"https:\/\/www.udemy.com\/user\/complete-signup\/"}
                    this._setCookie = resp.Headers.Get("Set-Cookie");

                    var setCookieValues = this._setCookie.Split(';');
                    foreach (var cookieValue in setCookieValues)
                    {
                        var args = cookieValue.Replace("path=/,",string.Empty).Split('=');
                        if (args.Length == 2)
                        {
                            var key = args[0].Trim();

                            if (key == "client_id")
                            {
                                this._clientId = args[1];
                            }

                            if (key == "access_token")
                            {
                                this._accessToken = args[1];
                            }
                        }
                    }
                    this.IsAuthenticated = true;
                }
            }
            catch (WebException)
            {
                // Add your error handling here
                return;
            }
            catch (Exception)
            {
                // Add your error handling here
                return;
            }
        }

        public string Get(string url)
        {
            var request = CreateRequest(url);
            request.Method = WebRequestMethods.Http.Get;
            request.CookieContainer = this.Cookies;
            request.KeepAlive = true;

            System.Threading.Tasks.Task<WebResponse> task = System.Threading.Tasks.Task.Factory.FromAsync(
               request.BeginGetResponse,
               asyncResult => request.EndGetResponse(asyncResult),
               null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result)).Result;
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var sr = new StreamReader(responseStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }

            return string.Empty;
        }
    }
}
