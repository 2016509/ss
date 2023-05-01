using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockAppliance.DatabaseClasses;
using StockAppliance.Methods;
using StockAppliance.ResponseClasses;
using StockAppliance.Settings;
using System.Text.RegularExpressions;

namespace StockAppliance.SiteMethods
{
    public class ServiceMatters
    {
        public static void Parsing(DatabaseTotalResults request, List<PartListPDFResponse> PartListPDFResonseList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='ServiceMatters'");
            con.Close();
            search = Regex.Matches(search, @"\w+", RegexOptions.IgnoreCase)[0].ToString();

            try
            {
                string result_search = CustomHttpClass.GetToString($"https://servicematters.com/en_US/api/guest-search/v2?orderBy=modifiedAt&direction=desc&category=Parts+List&query={search}");

                try
                {
                    dynamic _js = JsonConvert.DeserializeObject(result_search);
                    int count = _js.results.Count;
                    if (count > 0)
                    {
                        string url = _js.results[0].url.ToString();
                        string title = _js.results[0].header.ToString();


                        //--------------------------------------------------------------------------------------------
                        PartListPDFResponse pl = new()
                        {
                            Priority = priority.PartlistPDFPriority,
                            Title = title,
                            URL = ShortUrl.MakeShortURL(url),
                            Source = "servicematters.com"
                        };
                        con.Open();
                        string type = "PartListPDF";
                        string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(pl));
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                        con.Close();
                        //--------------------------------------------------------------------------------------------


                        PartListPDFResonseList.Add(new()
                        {
                            Priority = priority.PartlistPDFPriority,
                            Title = title,
                            URL = ShortUrl.MakeShortURL(url),
                            Source = "servicematters.com"
                        });
                    }
                }
                catch { return; }
            }
            catch (Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = null,
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 1",
                    Source = "servicematters.com",
                    Url = null

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on servicematters.com registered. STAGE - 1    Request: {search}");
                return;
            }
        }
    }
}
