using Dapper;
using MySql.Data.MySqlClient;
using StockAppliance.DatabaseClasses;
using StockAppliance.Methods;
using StockAppliance.ResponseClasses;
using StockAppliance.Settings;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace StockAppliance.SiteMethods
{
    public class ReliableParts
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='ReliableParts'");
            con.Close();

            string location = null;

            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.reliableparts.com/catalogsearch/result/?cat=2&q={search}",
                referrer: "https://www.reliableparts.com/",
                acceptencoding: "none");
            }
            catch
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = null,
                    Base64errorData = null,
                    Comment = "STAGE 0",
                    Url = @$"https://www.reliableparts.com/catalogsearch/result/?cat=2&q={search}",
                    Source = "reliableparts.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on ReliableParts registered. STAGE - 0    Request: {search}");
                return;
            }
            string total_result = null;
            string total_url = null;
            string total_count = "All";

            if (string.IsNullOrEmpty(location))
            {
                string search_result = null;

                try
                {
                    search_result = CustomHttpClass.GetToString(@$"https://www.reliableparts.com/search?q={search}",
                referrer: "https://www.reliableparts.com/",
                acceptencoding: "none");
                }
                catch (Exception ex)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(total_result),
                        Base64errorData = TextConvert.ToBase64String(ex.Message.ToString()),
                        Comment = "STAGE 0.1",
                        Url = @$"https://www.reliableparts.com/search?q={search}",
                        Source = "reliableparts.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on ReliableParts registered. STAGE - 0.1    Request: {search}");
                    return;
                    
                }
                try
                {
                    var position_regex = Regex.Matches(search_result, @"https://www.reliableparts.com/lookup/.*?(?="" title="")");
                    string first_position_url = position_regex[0].ToString();
                    total_result = CustomHttpClass.GetToString(first_position_url,
                referrer: $"https://www.reliableparts.com/search?q={search}",
                acceptencoding: "none");
                    total_url = first_position_url;
                    total_count = position_regex.Count.ToString();
                }
                catch { return; }
            }
            else
            {
                total_result = CustomHttpClass.GetToString(@$"{location}",
                referrer: $"https://www.reliableparts.com/catalogsearch/result/?cat=2&q={search}",
                acceptencoding: "none");
                total_url = @$"{location}";
            }

            if (total_result != null)
            {
                try
                {
                    string title = Regex.Matches(total_result, @"(?<=<h1>).*?(?=</h1>)")[0].ToString();

                    //-----------------------------------------------------------------------
                    DiagramWebResponse dw = new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = @$"https://www.reliableparts.com/catalogsearch/result/?cat=2&q={search}",
                        Source = "reliableparts.com",
                        Title = title,
                        Url = total_url,
                        Version = null
                    };
                    con.Open();
                    string type = "DiagramWeb";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //-----------------------------------------------------------------------

                    DiagramWebResponseList.Add(new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = @$"https://www.reliableparts.com/catalogsearch/result/?cat=2&q={search}",
                        Source = "reliableparts.com",
                        Title = title,
                        Url = total_url,
                        Version = null
                    });
                }
                catch (Exception ex)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(total_result),
                        Base64errorData = TextConvert.ToBase64String(ex.Message.ToString()),
                        Comment = "STAGE 3",
                        Url = null,
                        Source = "reliableparts.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on ReliableParts registered. STAGE - 3    Request: {search}");
                    return;
                }
            }

        }

    }
}
