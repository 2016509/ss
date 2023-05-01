using Newtonsoft.Json;
using StockAppliance.Settings;

namespace StockAppliance.Methods
{
    public sealed class ShortUrl
    {
        public static string MakeShortURL(string url)
        {
            string request_data = JsonConvert.SerializeObject(new ShortUrlJson.Shorten() { ApiKey = AppSettings.Current.HyperMagic.ApiKey, Url = url });

            string result = CustomHttpClass.Post(@"https://api.hm.ru/key/url/shorten", jsonData: request_data, contentType: "application/json");

            var result_parsed = JsonConvert.DeserializeObject<ShortUrlJson.ShortenResponse>(result);

            if (result_parsed.Status == -1) return url;
            else
            {
                return result_parsed.Data.ShurtUrl;
            }

            
        }
        
    }
}
