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
    public class FixCom
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='FixCom'");
            con.Close();



            try
            {
                string _js_search_result = CustomHttpClass.Post("https://www.fix.com/AjaxService.asmx/Search",
                contentType: "application/json",
                jsonData: @$"{{""searchTerm"":""{search}"",""numResults"":6}}");
                try
                {
                    dynamic _js = JsonConvert.DeserializeObject(_js_search_result);
                    if ((int)_js.d.matches > 0)
                    {
                        string diagram_search = _js.d.items[0].ToString();

                        string location = CustomHttpClass.CheckRedirectGet(@$"https://www.fix.com/Search.ashx?SearchTerm={diagram_search}&SearchMethod=standard");
                        if (!string.IsNullOrEmpty(location))
                        {
                            string html_result = CustomHttpClass.GetToString($@"{location}");
                            string title = Regex.Matches(html_result, @"(?<=""title-main"">).*?(?=</h1>)")[0].ToString();


                            //------------------------------------------------------------
                            DiagramWebResponse dw = new()
                            {
                                Priority = priority.DiagramWEBPriority,
                                SearchUrl = null,
                                Source = "fix.com",
                                Title = title,
                                Url = location,
                                Version = null,
                                ResultCount = _js.d.matches.ToString()
                            };
                            con.Open();
                            string type = "DiagramWeb";
                            string DataEscaped = JsonConvert.SerializeObject(dw);
                            con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                            con.Close();
                            //------------------------------------------------------------


                            DiagramWebResponseList.Add(new()
                            {
                                Priority = priority.DiagramWEBPriority,
                                SearchUrl = null,
                                Source = "fix.com",
                                Title = title,
                                Url = location,
                                Version = null,
                                ResultCount = _js.d.matches.ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(_js_search_result),
                        Base64errorData = TextConvert.ToBase64String(ex.Message),
                        Comment = "STAGE 2",
                        Url = $"https://www.fix.com/AjaxService.asmx/Search",
                        Source = "fix.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on FIX.COM registered. STAGE - 2    Request: {search}");

                    return;
                    
                }
            }
            catch(Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData =  null,
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 1",
                    Source = "fix.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on FIX.COM registered. STAGE - 1    Request: {search}");

                return;
            }
        }

    }
}
