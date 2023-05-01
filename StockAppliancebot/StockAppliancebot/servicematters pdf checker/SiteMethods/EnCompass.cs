using Dapper;
using MySql.Data.MySqlClient;
using StockAppliance.DatabaseClasses;
using StockAppliance.Methods;
using StockAppliance.ResponseClasses;
using StockAppliance.Settings;
using StockAppliance.SiteMethods.Classes;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace StockAppliance.SiteMethods
{
    internal class EnCompass
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList, List<PartListPDFResponse> PartListPDFResponseList, List<ServiceManualPDFResponse> ServiceManualPDFResponseList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='EnCompass'");
            con.Close();
            string location = null;

            try
            {
                location = CustomHttpClass.CheckRedirectGet($"https://encompass.com/search?searchTerm={search}", use_google_ua: false);
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
                    Comment = "STAGE 0",
                    Url = $"https://encompass.com/search?searchTerm={search}",
                    Source = "encompass.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on EnCompass registered. STAGE - 0    Request: {search}");
                return;
            }
            string search_result = null;
            List<EnCompassClasses.SearchTable> SearchTable = new();
            if (location == null)
            {
                search_result = CustomHttpClass.GetToString($"https://encompass.com/search?searchTerm={search}", use_google_ua: false);

                string table_html = null;
                try
                {
                    int results_count = int.Parse(Regex.Matches(search_result, @"(?<=<h1>).*(?= results)")[0].ToString());
                    if (results_count == 0) return;
                    table_html = Regex.Matches(search_result, @"(?<=<tbody>)[\w\W]*?(?=</tbody>)")[0].ToString();
                }

                catch(Exception ex)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(search_result),
                        Base64errorData = TextConvert.ToBase64String(ex.Message),
                        Comment = "STAGE 1",
                        Url = $"https://encompass.com/search?searchTerm={search}",
                        Source = "encompass.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on EnCompass registered. STAGE - 1    Request: {search}");
                    return;

                    
                }
                var table_rows_html = Regex.Matches(table_html, @"(?<=<tr>)[\w\W]*?(?=</tr>)");



                foreach (var row in table_rows_html)
                {
                    try
                    {
                        string href = Regex.Matches(row.ToString(), @"(?<=href="")[\w\W]*?(?="">)")[0].ToString();
                        string ModelName = Regex.Matches(row.ToString(), @"(?<=</td><td>)[\w\W]*?(?=</td><td class=""text-right"">)")[0].ToString();
                        string count = Regex.Matches(row.ToString(), @"(?<=<td class=""text-right"">)[\w\W]*?(?=</td>)")[0].ToString();
                        try
                        {
                            if (int.Parse(count) == 0 || int.Parse(count) == 1) return;
                            else
                            {
                                SearchTable.Add(new() { Href = href, ModelName = ModelName });
                            }
                        }
                        catch(Exception ex)
                        {
                            con.Open();
                            var error_log = new DatabaseUnregisteredResponses()
                            {
                                RequestId = request.ID,
                                RequestText = request.Request,
                                Base64wrongData = TextConvert.ToBase64String(row.ToString()),
                                Base64errorData = TextConvert.ToBase64String(ex.Message),
                                Comment = "STAGE 3",
                                Url = $"https://encompass.com/search?searchTerm={search}",
                                Source = "encompass.com"

                            };
                            con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                            con.Close();
                            Console.WriteLine($"Error on EnCompass registered. STAGE - 3    Request: {search}");
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
                            Base64wrongData = TextConvert.ToBase64String(row.ToString()),
                            Base64errorData = TextConvert.ToBase64String(ex.Message),
                            Comment = "STAGE 2",
                            Url = $"https://encompass.com/search?searchTerm={search}",
                            Source = "encompass.com"

                        };
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                        con.Close();
                        Console.WriteLine($"Error on EnCompass registered. STAGE - 2    Request: {search}");
                        return;
                    }

                }
            }

            else
            {
                SearchTable.Add(new() { Href = location, ModelName = null });
            }


            foreach (var row in SearchTable)
            {
                CheckRow(request, DiagramWebResponseList, row, priority, PartListPDFResponseList, ServiceManualPDFResponseList, SearchTable.Count, con);
            }




        }

        private static void CheckRow(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList, EnCompassClasses.SearchTable row, DatabaseSitesPriority priority, List<PartListPDFResponse> PartListPDFResponseList, List<ServiceManualPDFResponse> ServiceManualPDFResponseList, int rows_count, MySqlConnection con)
        {
            string search = request.Request;
            List<EnCompassClasses.ServiceManuals> service_manual_hrefs = new();
            List<EnCompassClasses.PartListPDFs> part_list_pdf_hrefs = new();

            string model_data = null;
            
            string model_versions_html = null;
            try
            {
                model_data = CustomHttpClass.GetToString($"https://encompass.com{row.Href}", use_google_ua: false);
                model_versions_html = Regex.Matches(model_data, @"(?<=<tbody>)[\w\W]*?(?=</tbody>)")[0].ToString();

            }
            catch
            {
                var exploded_view_regex = Regex.Matches(model_data, @"(?<=<a href="")/Exploded-View-Assembly/[\w\W]*?(?="">)");

                var service_manual_hrefs_regex = Regex.Matches(model_data, @"(?<=<a href="")/shop/model_research_docs.*.pdf(?="" target=""_blank"">Service Manual</a>)");
                var part_lists_hrefs_regex = Regex.Matches(model_data, @"(?<=<a href="")/shop/model_research_docs.*.pdf(?="" target=""_blank"">)");
                var title_regex = Regex.Matches(model_data, @"(?<=<h1>).*(?=</h1>)");
                string titile_for_classes = null;
                if (title_regex.Count > 0) titile_for_classes = title_regex.First().ToString();
                foreach (var service_manual in service_manual_hrefs_regex)
                {
                    service_manual_hrefs.Add(new() { Href = service_manual.ToString(), Title = titile_for_classes });
                }

                foreach (var partlist_pdf in part_lists_hrefs_regex)
                {
                    part_list_pdf_hrefs.Add(new() { Href = partlist_pdf.ToString(), Title = titile_for_classes });
                }


                string variation = null;


                var variation_regex = Regex.Matches(model_data, @"(?<=<p>Variation<br />).*?(?=</p>)");
                if (variation_regex.Count > 0) variation = variation_regex.First().Value;

                if (exploded_view_regex.Count > 0 && title_regex.Count > 0)
                {
                    //---------------------------------------------
                    DiagramWebResponse dw = new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = $"https://encompass.com/search?searchTerm={search}",
                        Source = "encompass.com",
                        Title = title_regex.First().Value,
                        Url = $"https://encompass.com{exploded_view_regex[0]}",
                        Version = variation,
                        ResultCount = rows_count.ToString()
                    };
                    con.Open();
                    string type = "DiagramWeb";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //---------------------------------------------

                    DiagramWebResponseList.Add(new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = $"https://encompass.com/search?searchTerm={search}",
                        Source = "encompass.com",
                        Title = title_regex.First().Value,
                        Url = $"https://encompass.com{exploded_view_regex[0]}",
                        Version = variation,
                        ResultCount = rows_count.ToString()
                    });

                }
            }
            if (model_versions_html == null)
            {
                Console.WriteLine($"ENCOMPASS CRASH - {search} - MODEL VERSIONS HTML");
                return;
            }
            var model_versions_hrefs = Regex.Matches(model_versions_html, @"(?<=<td><a href="")/model[\w\W]*?(?="")");
            if (model_versions_hrefs.Count > 0)
            {
                foreach (var model_versions_href in model_versions_hrefs)
                {
                    string model_version_html = CustomHttpClass.GetToString($"https://encompass.com{model_versions_href}", use_google_ua: false);

                    var exploded_view_regex = Regex.Matches(model_version_html, @"(?<=<a href="")/Exploded-View-Assembly/[\w\W]*?(?="">)");

                    var service_manual_hrefs_regex = Regex.Matches(model_data, @"(?<=<a href="")/shop/model_research_docs.*.pdf(?="" target=""_blank"">Service Manual</a>)");
                    var part_lists_hrefs_regex = Regex.Matches(model_data, @"(?<=<a href="")/shop/model_research_docs.*.pdf(?="" target=""_blank"">)");

                    var title_regex = Regex.Matches(model_data, @"(?<=<h1>).*(?=</h1>)");
                    string titile_for_classes = null;
                    if (title_regex.Count > 0) titile_for_classes = title_regex.First().ToString();
                    foreach (var service_manual in service_manual_hrefs_regex)
                    {
                        service_manual_hrefs.Add(new() { Href = service_manual.ToString(), Title = titile_for_classes });
                    }

                    foreach (var partlist_pdf in part_lists_hrefs_regex)
                    {
                        part_list_pdf_hrefs.Add(new() { Href = partlist_pdf.ToString(), Title = titile_for_classes });
                    }
                    string variation = null;


                    var variation_regex = Regex.Matches(model_data, @"(?<=<p>Variation<br />).*?(?=</p>)");
                    if (variation_regex.Count > 0) variation = variation_regex.First().Value;

                    if (exploded_view_regex.Count > 0 && title_regex.Count > 0)
                    {
                        //---------------------------------------------
                        DiagramWebResponse dw = new()
                        {
                            Priority = priority.DiagramWEBPriority,
                            SearchUrl = $"https://encompass.com/search?searchTerm={search}",
                            Source = "encompass.com",
                            Title = title_regex.First().Value,
                            Url = $"https://encompass.com{exploded_view_regex[0]}",
                            Version = variation,
                            ResultCount = rows_count.ToString()
                        };
                        con.Open();
                        string type = "DiagramWeb";
                        string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                        con.Close();
                        //---------------------------------------------

                        DiagramWebResponseList.Add(new()
                        {
                            Priority = priority.DiagramWEBPriority,
                            SearchUrl = $"https://encompass.com/search?searchTerm={search}",
                            Source = "encompass.com",
                            Title = title_regex.First().Value,
                            Url = $"https://encompass.com{exploded_view_regex[0]}",
                            Version = variation,
                            ResultCount = rows_count.ToString()
                        });

                    }

                }
            }
            else
            {
                var exploded_view_regex = Regex.Matches(model_data, @"(?<=<a href="")/Exploded-View-Assembly/[\w\W]*?(?="">)");


                var service_manual_hrefs_regex = Regex.Matches(model_data, @"(?<=<a href=)/shop/model_research_docs.*?.pdf(?= target=_blank>Service Manual</a>)");
                var part_lists_hrefs_regex = Regex.Matches(model_data, @"(?<=<a href=)/shop/model_research_docs.*?.pdf(?= target=_blank>)");

                var title_regex = Regex.Matches(model_data, @"(?<=<h1>).*(?=</h1>)");
                string titile_for_classes = null;
                if (title_regex.Count > 0) titile_for_classes = title_regex.First().ToString();
                foreach (var service_manual in service_manual_hrefs_regex)
                {
                    service_manual_hrefs.Add(new() { Href = service_manual.ToString(), Title = titile_for_classes });
                }

                foreach (var partlist_pdf in part_lists_hrefs_regex)
                {
                    part_list_pdf_hrefs.Add(new() { Href = partlist_pdf.ToString(), Title = titile_for_classes });
                }
                string variation = null;


                var variation_regex = Regex.Matches(model_data, @"(?<=<p>Variation<br />).*?(?=</p>)");
                if (variation_regex.Count > 0) variation = variation_regex.First().Value;


                if (exploded_view_regex.Count > 0 && title_regex.Count > 0)
                {
                    //---------------------------------------------
                    DiagramWebResponse dw = new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = $"https://encompass.com/search?searchTerm={search}",
                        Source = "encompass.com",
                        Title = title_regex.First().Value,
                        Url = $"https://encompass.com{exploded_view_regex[0]}",
                        Version = variation,
                        ResultCount = rows_count.ToString()
                    };
                    con.Open();
                    string type = "DiagramWeb";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //---------------------------------------------

                    DiagramWebResponseList.Add(new()
                    {
                        Priority = priority.DiagramWEBPriority,
                        SearchUrl = $"https://encompass.com/search?searchTerm={search}",
                        Source = "encompass.com",
                        Title = title_regex.First().Value,
                        Url = $"https://encompass.com{exploded_view_regex[0]}",
                        Version = variation,
                        ResultCount = rows_count.ToString()
                    });

                }

            }

            foreach(var service_manual in service_manual_hrefs)
            {
                string location = CustomHttpClass.CheckRedirectGet($"https://encompass.com{service_manual.Href}", use_google_ua: false);

                var test = new EnCompassClasses.PartListPDFs() { Href = service_manual.Href, Title = service_manual.Title };
                part_list_pdf_hrefs.Remove(test); 
                

                if (location != null)
                {
                    //---------------------------------------------
                    ServiceManualPDFResponse sm = new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = service_manual.Title,
                        URL = ShortUrl.MakeShortURL(location)
                    };
                    con.Open();
                    string type = "ServiceManualPDF";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(sm));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //---------------------------------------------

                    ServiceManualPDFResponseList.Add(new() { 
                    Priority = priority.ServiceManualPDFProirity,
                    Source = "encompass.com",
                    Title = service_manual.Title,
                    URL = ShortUrl.MakeShortURL(location)
                    });
                }
                else
                {
                    //---------------------------------------------
                    ServiceManualPDFResponse sm = new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = service_manual.Title,
                        URL = ShortUrl.MakeShortURL($"https://encompass.com{service_manual}")
                    };
                    con.Open();
                    string type = "ServiceManualPDF";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(sm));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //---------------------------------------------

                    ServiceManualPDFResponseList.Add(new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = service_manual.Title,
                        URL = ShortUrl.MakeShortURL($"https://encompass.com{service_manual}")
                    });
                }
            }

            foreach (var part_list_pdf in part_list_pdf_hrefs)
            {
                string location = null;

                try
                {
                    location = CustomHttpClass.CheckRedirectGet($"https://encompass.com{part_list_pdf.Href}", use_google_ua: false);
                }
                catch (Exception ex)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(row.ToString()),
                        Base64errorData = TextConvert.ToBase64String(ex.Message),
                        Comment = "STAGE 4",
                        Url = $"https://encompass.com/search?searchTerm={search}",
                        Source = "encompass.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on EnCompass registered. STAGE - 4    Request: {search}");
                    continue;

                }
                if (part_list_pdf.Href.Contains(@"/sm/")) continue;
                if (location != null)
                {
                    //---------------------------------------------
                    PartListPDFResponse pl = new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = part_list_pdf.Title,
                        URL = ShortUrl.MakeShortURL(location)
                    };
                    con.Open();
                    string type = "PartListPDF";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(pl));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //---------------------------------------------

                    PartListPDFResponseList.Add(new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = part_list_pdf.Title,
                        URL = ShortUrl.MakeShortURL(location)
                    });
                }
                else
                {
                    //---------------------------------------------
                    PartListPDFResponse pl = new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = part_list_pdf.Title,
                        URL = ShortUrl.MakeShortURL($"https://encompass.com{part_list_pdf}")
                    };
                    con.Open();
                    string type = "PartListPDF";
                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(pl));
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                    con.Close();
                    //---------------------------------------------

                    PartListPDFResponseList.Add(new()
                    {
                        Priority = priority.ServiceManualPDFProirity,
                        Source = "encompass.com",
                        Title = part_list_pdf.Title,
                        URL = ShortUrl.MakeShortURL($"https://encompass.com{part_list_pdf}")
                    });
                }
            }

        }
    }
}
