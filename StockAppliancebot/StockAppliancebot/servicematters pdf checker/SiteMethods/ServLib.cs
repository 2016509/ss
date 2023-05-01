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
    public class ServLib
    {
        public static void Parsing(DatabaseTotalResults request, List<ServiceManualPDFResponse> ServiceManualPDFResponseList, List<ServiceManualWEBResponse> ServiceManualWEBResponseList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='ServLib'");
            con.Close();

            string search_result = null;

            try
            {
                search_result = CustomHttpClass.GetToString($"https://servlib.com/search.html?searchword={search}&searchphrase=all&limit=10");
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
                    Source = "servlib.com",
                    Url = null

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on servlib.com registered. STAGE - 1    Request: {search}");
                return;
            }
            var results_hrefs_reg = Regex.Matches(search_result, @"(?<=<a href="").*?\.html(?="">)");

            foreach (var href in results_hrefs_reg)
            {
                try
                {
                    string manual_data_html = CustomHttpClass.GetToString($"https://servlib.com{href}");
                    var name_reg = Regex.Matches(manual_data_html, @"(?<=<h1>).*?(?=</h1>)");
                    if (name_reg.Count == 0) continue;
                    string title = name_reg[0].Value.Trim();

                    var dd_reg = Regex.Matches(manual_data_html, @"(?<=<dd>).*?(?=</dd>)");
                    if (dd_reg.Count == 0) continue;

                    string filename = dd_reg.Last().Value.Trim();
                    string total_url = $"https://servlib.com/disk{href}".Split(".html")[0] + $"/{filename}";

                    if (
                        CustomHttpClass.GetIsExist(total_url)
                        )
                    {
                        string file_type = filename.Split('.').Last().ToUpper();
                        switch (file_type)
                        {
                            case "PDF":
                                //--------------------------------------------------------------------
                                ServiceManualPDFResponse sm = new()
                                {
                                    Priority = priority.ServiceManualPDFProirity,
                                    Source = "servlib.com",
                                    Title = title,
                                    URL = ShortUrl.MakeShortURL(total_url)
                                };
                                con.Open();
                                string type = "ServiceManualPDF";
                                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(sm));
                                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                                con.Close();
                                //--------------------------------------------------------------------

                                ServiceManualPDFResponseList.Add(new()
                                {
                                    Priority = priority.ServiceManualPDFProirity,
                                    Source = "servlib.com",
                                    Title = title,
                                    URL = ShortUrl.MakeShortURL(total_url)
                                });
                                return;
                            default:
                                //--------------------------------------------------------------------
                                ServiceManualPDFResponse sm1 = new()
                                {
                                    Priority = priority.ServiceManualPDFProirity,
                                    Source = "servlib.com",
                                    Title = title,
                                    URL = ShortUrl.MakeShortURL(total_url),
                                    Version = file_type
                                };
                                con.Open();
                                string type1 = "ServiceManualPDF";
                                string DataEscaped1 = MySqlHelper.EscapeString(JsonConvert.SerializeObject(sm1));
                                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type1}', '{DataEscaped1}');");
                                con.Close();
                                //--------------------------------------------------------------------

                                ServiceManualPDFResponseList.Add(new()
                                {
                                    Priority = priority.ServiceManualPDFProirity,
                                    Source = "servlib.com",
                                    Title = title,
                                    URL = ShortUrl.MakeShortURL(total_url),
                                    Version = file_type
                                });
                                return;
                        }

                    }
                    else
                    {

                        //-------------------------------------------------------------------------------
                        ServiceManualWEBResponse smw = new()
                        {
                            Priority = priority.ServiceManualPDFProirity,
                            Source = "servlib.com",
                            Title = title,
                            URL = ShortUrl.MakeShortURL($"https://servlib.com{href}")
                        };
                        con.Open();
                        string type = "ServiceManualWEB";
                        string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(smw));
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                        con.Close();
                        //-------------------------------------------------------------------------------


                        ServiceManualWEBResponseList.Add(new()
                        {
                            Priority = priority.ServiceManualPDFProirity,
                            Source = "servlib.com",
                            Title = title,
                            URL = ShortUrl.MakeShortURL($"https://servlib.com{href}")
                        });
                        return;
                    }
                    
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
                        Comment = "STAGE 2",
                        Source = "servlib.com",
                        Url = $"https://servlib.com{href}"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on servlib.com registered. STAGE - 2    Request: {search}");
                    
                }
            }
        }

    }
}
