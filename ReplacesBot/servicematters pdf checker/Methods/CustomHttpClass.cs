using Leaf.xNet;
using ReplacesBot.Settings;

namespace ReplacesBot.Methods
{
    class CustomHttpClass
    {
        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static string GetToString(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true)
        {

            headers ??= new();
            using var req = new HttpRequest();


            switch (AppSettings.Current.Proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) HttpProxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) HttpProxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 10000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;



            var resp = req.Get(url);
            return resp.ToString();




        }

        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static Stream GetToStream(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true)
        {

            headers ??= new();
            using var req = new HttpRequest();
            switch (AppSettings.Current.Proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) HttpProxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) HttpProxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 30000;
            //req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;


            var myUniqueFileName = string.Format(@"\TempFiles\{0}.pdf", Guid.NewGuid());
            Stream filestream = req.Get(url).ToMemoryStream();
            return filestream;

            //resp.ToFile(System.Reflection.Assembly.GetExecutingAssembly().Location + myUniqueFileName);

            /*Stream filestream = new FileStream(System.Reflection.Assembly.GetExecutingAssembly().Location + myUniqueFileName, FileMode.Open);
            return filestream;*/




            /*Stream filestream = resp.ToMemoryStream();
            return filestream;*/




        }


        /// <summary>
        /// Custom method for Post http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="data">Data for POST request</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static string Post(string url, RequestParams data = null, List<CustomHttpAdditionals.Headers> headers = null, string contentType = null, string jsonData = null)
        {

            headers ??= new();
            using var req = new HttpRequest();
            switch (AppSettings.Current.Proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) HttpProxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) HttpProxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";

            string resp = null;
            if (string.IsNullOrEmpty(contentType) && string.IsNullOrEmpty(jsonData)) resp = req.Post(url, data).ToString();
            else resp = req.Post(url, jsonData, contentType).ToString();
            return resp;




        }

        /// <summary>
        /// Download a PDF file from ani URL
        /// </summary>
        /// <param name="url">PDFT url</param>
        /// <returns>Path of downloaded PDF file</returns>
        public static string DownloadPDF(string url)
        {


            using var req = new HttpRequest();
            switch (AppSettings.Current.Proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) HttpProxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) HttpProxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }

            req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";


            var myUniqueFileName = string.Format(@"\TempFiles\{0}.pdf", Guid.NewGuid());
            req.Get(url).ToFile(Environment.CurrentDirectory + myUniqueFileName);


            return Directory.GetCurrentDirectory() + myUniqueFileName;
        }

        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static bool GetIsExist(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true)
        {

            headers ??= new();
            using var req = new HttpRequest();
            switch (AppSettings.Current.Proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) HttpProxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) HttpProxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 10000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;



            try
            {
                var resp = req.Get(url);
                if (resp.IsOK && resp.ContentLength > 100) return true;
                else return false;
            }
            catch
            { return false; }




        }


        public static string CheckRedirectGet(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true)
        {
            headers ??= new();
            using var req = new HttpRequest();
            switch (AppSettings.Current.Proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) HttpProxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) HttpProxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Login)) Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                    if (!string.IsNullOrEmpty(AppSettings.Current.Proxy.Password)) Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }

            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 15000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            var resp = req.Get(url);
            if (string.IsNullOrEmpty(resp.Location)) return null;
            else return resp.Location;
        }



    }
}
