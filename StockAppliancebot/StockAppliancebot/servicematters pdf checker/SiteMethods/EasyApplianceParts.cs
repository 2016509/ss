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
    public class EasyApplianceParts
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='EasyApplianceParts'");



            con.Close();
            string search_result = null;

            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.easyapplianceparts.com/fuzzysearchresult.aspx?term={search.ToLower()}",
                new List<CustomHttpAdditionals.Headers> { },
                $"https://www.easyapplianceparts.com/SearchSuggestion.aspx?term={search.ToLower()}"
                );
            }
            catch(Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = null,
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 1",
                    Url = $"https://www.easyapplianceparts.com/fuzzysearchresult.aspx?term={search.ToLower()}",
                    Source = "easyapplianceparts.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on EasyApplianceParts registered. STAGE - 1    Request: {search}");
                return;

                
            }
            try
            {
                var href_regex = Regex.Matches(search_result, @"(?<=href="").*?(?="">)", RegexOptions.IgnoreCase);
                var href = System.Web.HttpUtility.HtmlDecode(href_regex[0].ToString());
                var title_regex = Regex.Matches(search_result, @"(?<="">)<b>.*?(?=</a></li>)", RegexOptions.IgnoreCase);
                string title = Regex.Replace(title_regex[0].ToString(), "<.*?>", String.Empty);
                //---------------------------------------------------------------------------------------------------
                DiagramWebResponse dw = new()
                {
                    Priority = priority.DiagramWEBPriority,
                    SearchUrl = $"https://www.easyapplianceparts.com/SearchSuggestion.aspx?term={search.ToLower()}",
                    Source = "easyapplianceparts.com",
                    Title = title,
                    Url = $"https://www.easyapplianceparts.com{href}",
                    Version = null,
                    ResultCount = title_regex.Count.ToString()
                };
                con.Open();
                string type = "DiagramWeb";
                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                con.Close();
                //---------------------------------------------------------------------------------------------------

                DiagramWebResponseList.Add(new()
                {
                    Priority = priority.DiagramWEBPriority,
                    SearchUrl = $"https://www.easyapplianceparts.com/SearchSuggestion.aspx?term={search.ToLower()}",
                    Source = "easyapplianceparts.com",
                    Title = title,
                    Url = $"https://www.easyapplianceparts.com{href}",
                    Version = null,
                    ResultCount = title_regex.Count.ToString()
                });
            }
            catch (Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    
                    Base64wrongData = TextConvert.ToBase64String(search_result),
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 2",
                    Url = $"https://www.easyapplianceparts.com/fuzzysearchresult.aspx?term={search.ToLower()}",
                    Source = "easyapplianceparts.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on EasyApplianceParts registered. STAGE - 2    Request: {search}");
                return;
            }

        }
    }
}
