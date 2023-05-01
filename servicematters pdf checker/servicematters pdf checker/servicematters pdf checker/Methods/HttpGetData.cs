using Leaf.xNet;
using servicematters_pdf_checker.Settings;

namespace servicematters_pdf_checker.Methods
{
    class HttpGetData
    {
        /// <summary>
        /// Getting JSON data from https://servicematters.com (Parts List)
        /// </summary>
        /// <param name="search_data">SKU data for search</param>
        /// <returns>JSON of results from Parts List</returns>
        public static string GetPartData(string search_data)
        {


            using (var req = new HttpRequest())
            {
                switch (AppSettings.Current.Proxy.Type)
                {
                    case "HTTP":
                        var HttpProxy = HttpProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                        HttpProxy.Username = AppSettings.Current.Proxy.Login;
                        HttpProxy.Password = AppSettings.Current.Proxy.Password;
                        req.Proxy = HttpProxy;
                        break;

                    case "SOCKS5":
                        var Socks5Proxy = Socks5ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                        Socks5Proxy.Username = AppSettings.Current.Proxy.Login;
                        Socks5Proxy.Password = AppSettings.Current.Proxy.Password;
                        req.Proxy = Socks5Proxy;
                        break;

                    case "SOCKS4":
                        var Socks4Proxy = Socks4ProxyClient.Parse($"{AppSettings.Current.Proxy.Ip}:{AppSettings.Current.Proxy.Port}");
                        Socks4Proxy.Username = AppSettings.Current.Proxy.Login;
                        Socks4Proxy.Password = AppSettings.Current.Proxy.Password;
                        req.Proxy = Socks4Proxy;
                        break;
                    default: break;
                }
                req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                var response = req.Get("https://servicematters.com/en_US/api/guest-search/v2?category=Parts+List&query=" + search_data).ToString();


                return response;

            }
        }

        
    }
}
