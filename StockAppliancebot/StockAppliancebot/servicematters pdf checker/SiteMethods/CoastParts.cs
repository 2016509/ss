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
    public class CoastParts
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList, List<PartListPDFResponse> PartListPDFResponse)
        {
            string search = request.Request;
            List<PartListPDFResponse> PartListPDFResonseTemp = new();
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='CoastParts'");



            con.Close();

            string check_search = Regex.Matches(search, @"\w+", RegexOptions.IgnoreCase)[0].ToString();

            try
            {
                string redirect = CustomHttpClass.CheckRedirectGet($"https://www.coastparts.com/search?q={search}", new List<CustomHttpAdditionals.Headers>() {
                    new() { Name = "Upgrade-Insecure-Requests", Value = "1" },
                    new() { Name = "Sec-Fetch-Site", Value = "same-origin" },
                    new() { Name = "Sec-Fetch-Mode", Value = "navigate" },
                    new() { Name = "Sec-Fetch-User", Value = "?1" },
                    new() { Name = "Sec-Fetch-Dest", Value = "document" }}, 
                "https://www.coastparts.com/", 
                "none");


                if (string.IsNullOrEmpty(redirect))
                {
                    string resp = CustomHttpClass.GetToString(
                        $"https://www.coastparts.com/search?q={search}", 
                        new List<CustomHttpAdditionals.Headers>() {
                            new() { Name = "Upgrade-Insecure-Requests", Value = "1" },
                            new() { Name = "Sec-Fetch-Site", Value = "same-origin" },
                            new() { Name = "Sec-Fetch-Mode", Value = "navigate" },
                            new() { Name = "Sec-Fetch-User", Value = "?1" },
                            new() { Name = "Sec-Fetch-Dest", Value = "document" }}, 
                        "https://www.coastparts.com/", 
                        "none");

                    var temp_list_html = Regex.Matches(resp, @"<a class=""list-group-item text-center"" href=""/pdfs/[\w\W]*?</a>", RegexOptions.IgnoreCase);

                    foreach (var temp in temp_list_html)
                    {
                        string temp_html_element = temp.ToString();
                        string name = Regex.Matches(temp_html_element, @"(?<=<strong>)[\w\W]*?(?=</strong>)", RegexOptions.IgnoreCase)[0].ToString();
                        string additional = Regex.Matches(temp_html_element, @"\([\w\W]*?\)", RegexOptions.IgnoreCase)[0].ToString();
                        string href = Regex.Matches(temp_html_element, @"(?<=href="").*?(?="" target=""_blank"">)", RegexOptions.IgnoreCase)[0].ToString();

                        

                            PartListPDFResonseTemp.Add(new()
                            {
                                Category = null,
                                Priority = priority.PartlistPDFPriority,
                                Source = "coastparts.com",
                                Title = name,
                                URL = $"{ShortUrl.MakeShortURL($"https://www.coastparts.com/{href}")}"

                            });
                        
                    }




                }

                else
                {
                    string resp = CustomHttpClass.GetToString($"https://www.coastparts.com{redirect}", new List<CustomHttpAdditionals.Headers>() {
                new() { Name = "Upgrade-Insecure-Requests", Value = "1" },
                new() { Name = "Sec-Fetch-Site", Value = "same-origin" },
                new() { Name = "Sec-Fetch-Mode", Value = "navigate" },
                new() { Name = "Sec-Fetch-User", Value = "?1" },
                new() { Name = "Sec-Fetch-Dest", Value = "document" }
            }, "https://www.coastparts.com/", "none");
                    var title = Regex.Matches(resp, @"(?<=<h1 >).*(?=</h1>)", RegexOptions.IgnoreCase);


                    //----------------------------------------------------------------------------
                    DiagramWebResponse dw = new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = $"https://www.coastparts.com/search?q={search}",
                        Source = "coastparts.com",
                        Title = title[0].ToString(),
                        Url = $"https://www.coastparts.com{redirect}",
                        Version = null,
                        ResultCount = "1"
                    };
                    con.Open();
                    string type = "DiagramWeb";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    
                    //----------------------------------------------------------------------------


                    DiagramWebResponseList.Add(new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = $"https://www.coastparts.com/search?q={search}",
                        Source = "coastparts.com",
                        Title = title[0].ToString(),
                        Url = $"https://www.coastparts.com{redirect}",
                        Version = null,
                        ResultCount = "1"
                    });
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
                    Base64errorData = TextConvert.ToBase64String(ex.Message.ToString()), 
                    Comment = "STAGE 1",
                    Url = null,
                    Source = "coastparts.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on CoastParts registered. STAGE - 1    Request: {search}");
                return;
            }

            if (PartListPDFResonseTemp.Count > 0)
            {
                PartListPDFResonseTemp = PartListPDFResonseTemp.OrderByDescending(x => CheckLength(search, x.Title)).ToList();
                var first = PartListPDFResonseTemp.First();

                con.Open();
                string type = "PartListPDF";
                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(first));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                con.Close();
                PartListPDFResponse.Add(first);
            }



        }

        private static int CheckLength (string search, string title)
        {
            if (title == null) return 0;
            string new_search = search.ToUpper();
            while (true)
            {
                if (new_search.Length <= 2) break; ;

                if (title.Contains(new_search))
                {
                    return new_search.Length;
                }
                else
                {
                    new_search = new_search.Remove(new_search.Length - 1, 1);
                }
            }

            new_search = search.ToLower();

            while (true)
            {
                if (new_search.Length <= 2) return 0;

                if (title.Contains(new_search))
                {
                    return new_search.Length;
                }
                else
                {
                    new_search = new_search.Remove(new_search.Length - 1, 1);
                }
            }
        }
    }
}
