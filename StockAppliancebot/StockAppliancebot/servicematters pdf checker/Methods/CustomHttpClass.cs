using Leaf.xNet;
using StockAppliance.Settings;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockAppliance.DatabaseClasses;

namespace StockAppliance.Methods
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

            var taken_proxy = GetRandomProxy();
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
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
            var taken_proxy = GetRandomProxy();
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;


            var myUniqueFileName = string.Format(@"\TempFiles\{0}.jpg", Guid.NewGuid());
            var resp = req.Get(url);

            Stream filestream = resp.ToMemoryStream();
            return filestream;

            /*var resp = req.Get(url);
            return resp.ToMemoryStream();*/




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
            var taken_proxy = GetRandomProxy();
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
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
            var taken_proxy = GetRandomProxy();
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            req.ConnectTimeout = 8000;
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
            var taken_proxy = GetRandomProxy();
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
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
            var taken_proxy = GetRandomProxy();
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }

            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            var resp = req.Get(url);
            if (string.IsNullOrEmpty(resp.Location)) return null;
            else return resp.Location;
        }
    
    
        private static DatabaseProxyData GetRandomProxy()
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var proxy = con.QueryFirstOrDefault<DatabaseProxyData>($"SELECT * FROM proxy_table ORDER BY RAND() LIMIT 1;");
            con.Close();
            return proxy;

        }
    }
}
