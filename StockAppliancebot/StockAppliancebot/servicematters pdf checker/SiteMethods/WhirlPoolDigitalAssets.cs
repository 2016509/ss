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
    public class WhirlPoolDigitalAssets
    {
        public static void ParsingServicePointerPDF(DatabaseTotalResults request, List<ServicePointerPDFResponse> thisList)
        {
            string search = request.Request;
            string type = "ParsingServicePointer";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='WhirlPoolDigitalAssets'");
            con.Close();

            string search_result = null;
            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.19_values=asset-type:service-pointer&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0", use_google_ua: false);
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
                    Comment = $"[{type}] STAGE 0",
                    Source = "whirlpooldigitalassets.com",
                    Url = $"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.19_values=asset-type:service-pointer&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 0    Request: {search}");
                return;

            }
            var results_html = Regex.Matches(search_result, @"<article data-asset-share-id=""asset""[\w\W]*?</article>");

            foreach (var result in results_html)
            {
                var page_reg = Regex.Matches(result.ToString(), @"(?<=<a href="")[\w\W]*?(?="">)");
                if (page_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 1",
                        Source = "whirlpooldigitalassets.com"
                        
                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 1  Request: {search}");
                    return;
                }
                var name_reg = Regex.Matches(result.ToString(), @$"(?<=<a href=""{page_reg[0].Value}"">)[\w\W]*?(?=</a>)");

                if (name_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 2",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 2  Request: {search}");
                    return;
                }
                string title = name_reg[0].Value.Trim();

                var href_reg = Regex.Matches(result.ToString(), @"(?<=<button class=""ui link button"" data-asset-share-id=""download-asset"" data-asset-share-asset="")[\w\W]*?(?="">Download</button>)");

                if (href_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 3",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 3  Request: {search}");
                    return;
                }

                string href = href_reg[0].Value;


                //--------------------------------------------------------------------------------
                ServicePointerPDFResponse data = new()
                {
                    Priority = priority.ServicePointerPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                };
                con.Open();
                string type1 = "ServicePointerPDF";
                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(data));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type1}', '{DataEscaped}');");
                con.Close();
                //--------------------------------------------------------------------------------


                thisList.Add(new()
                {
                    Priority = priority.ServicePointerPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                });
            }


        }

        public static void ParsingServiceManualPDF(DatabaseTotalResults request, List<ServiceManualPDFResponse> thisList)
        {
            string search = request.Request;
            string type = "ParsingServiceManualPDF";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='WhirlPoolDigitalAssets'");
            con.Close();

            string search_result = null;
            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.0_values=asset-type:manual&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0", use_google_ua: false);
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
                    Comment = $"[{type}] STAGE 0",
                    Source = "whirlpooldigitalassets.com",
                    Url = $"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.0_values=asset-type:manual&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 0    Request: {search}");
                return;

            }

            var results_html = Regex.Matches(search_result, @"<article data-asset-share-id=""asset""[\w\W]*?</article>");

            foreach (var result in results_html)
            {
                var page_reg = Regex.Matches(result.ToString(), @"(?<=<a href="")[\w\W]*?(?="">)");
                if (page_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 1",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 1  Request: {search}");
                    return;
                }
                var name_reg = Regex.Matches(result.ToString(), @$"(?<=<a href=""{page_reg[0].Value}"">)[\w\W]*?(?=</a>)");

                if (name_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 2",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 2  Request: {search}");
                    return;
                }
                string title = name_reg[0].Value.Trim();

                var href_reg = Regex.Matches(result.ToString(), @"(?<=<button class=""ui link button"" data-asset-share-id=""download-asset"" data-asset-share-asset="")[\w\W]*?(?="">Download</button>)");

                if (href_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 3",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 3  Request: {search}");
                    return;
                }

                string href = href_reg[0].Value;

                //--------------------------------------------------------------------------------
                ServiceManualPDFResponse data = new()
                {
                    Priority = priority.ServiceManualPDFProirity,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                };
                con.Open();
                string type1 = "ServiceManualPDF";
                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(data));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type1}', '{DataEscaped}');");
                con.Close();
                //--------------------------------------------------------------------------------

                thisList.Add(new()
                {
                    Priority = priority.ServiceManualPDFProirity,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                });
            }


        }

        public static void ParsingWiringDiagramPDF(DatabaseTotalResults request, List<WiringDiagramPDFResponse> thisList)
        {
            string search = request.Request;
            string type = "ParsingWiringDiagramPDF";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='WhirlPoolDigitalAssets'");
            con.Close();

            string search_result = null;

            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.1_values=asset-type:wiring-sheet&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0", use_google_ua: false);

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
                    Comment = $"[{type}] STAGE 0",
                    Source = "whirlpooldigitalassets.com",
                    Url = $"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.0_values=asset-type:manual&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 0    Request: {search}");
                return;

            }


            var results_html = Regex.Matches(search_result, @"<article data-asset-share-id=""asset""[\w\W]*?</article>");

            foreach (var result in results_html)
            {
                var page_reg = Regex.Matches(result.ToString(), @"(?<=<a href="")[\w\W]*?(?="">)");
                if (page_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 1",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 1  Request: {search}");
                    return;
                }
                var name_reg = Regex.Matches(result.ToString(), @$"(?<=<a href=""{page_reg[0].Value}"">)[\w\W]*?(?=</a>)");

                if (name_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 2",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 2  Request: {search}");
                    return;
                }
                string title = name_reg[0].Value.Trim();

                var href_reg = Regex.Matches(result.ToString(), @"(?<=<button class=""ui link button"" data-asset-share-id=""download-asset"" data-asset-share-asset="")[\w\W]*?(?="">Download</button>)");

                if (href_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 3",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 3  Request: {search}");
                    return;
                }

                string href = href_reg[0].Value;

                //--------------------------------------------------------------------------------
                WiringDiagramPDFResponse data = new()
                {
                    Priority = priority.ServiceManualPDFProirity,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                };
                con.Open();
                string type1 = "WiringDiagramPDF";
                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(data));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type1}', '{DataEscaped}');");
                con.Close();
                //--------------------------------------------------------------------------------

                thisList.Add(new()
                {
                    Priority = priority.WiringSheetPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                });
            }


        }

        public static void ParsingPartListPDF(DatabaseTotalResults request, List<PartListPDFResponse> thisList)
        {
            string search = request.Request;
            string type = "ParsingPartListPDF";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='WhirlPoolDigitalAssets'");
            con.Close();
            string search_result = null;
            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.6_values=asset-type:parts-list&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0", use_google_ua: false);
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
                    Comment = $"[{type}] STAGE 0",
                    Source = "whirlpooldigitalassets.com",
                    Url = $"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.0_values=asset-type:manual&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 0    Request: {search}");
                return;

            }
            var results_html = Regex.Matches(search_result, @"<article data-asset-share-id=""asset""[\w\W]*?</article>");

            foreach (var result in results_html)
            {
                var page_reg = Regex.Matches(result.ToString(), @"(?<=<a href="")[\w\W]*?(?="">)");
                if (page_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 1",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 1  Request: {search}");
                    return;
                }
                var name_reg = Regex.Matches(result.ToString(), @$"(?<=<a href=""{page_reg[0].Value}"">)[\w\W]*?(?=</a>)");

                if (name_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 2",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 2  Request: {search}");
                    return;
                }
                string title = name_reg[0].Value.Trim();

                var href_reg = Regex.Matches(result.ToString(), @"(?<=<button class=""ui link button"" data-asset-share-id=""download-asset"" data-asset-share-asset="")[\w\W]*?(?="">Download</button>)");

                if (href_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 3",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 3  Request: {search}");
                    return;
                }

                string href = href_reg[0].Value;

                //--------------------------------------------------------------------------------
                PartListPDFResponse data = new()
                {
                    Priority = priority.PartlistPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                };
                con.Open();
                string type1 = "PartListPDF";

                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(data));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type1}', '{DataEscaped}');");
                con.Close();
                //--------------------------------------------------------------------------------


                thisList.Add(new()
                {
                    Priority = priority.PartlistPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                });
            }


        }

        public static void ParsingTechSheetPDF(DatabaseTotalResults request, List<TechSheetPDFResponse> thisList)
        {
            string search = request.Request;
            string type = "ParsingTechSheetPDF";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='WhirlPoolDigitalAssets'");
            con.Close();
            string search_result = null;
            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.24_values=asset-type:tech-sheet&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0", use_google_ua: false);
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
                    Comment = $"[{type}] STAGE 0",
                    Source = "whirlpooldigitalassets.com",
                    Url = $"https://www.whirlpooldigitalassets.com/content/wdl/en/assets.results.html?fulltext={search}&5_group.propertyvalues.property=./jcr:content/metadata/wp:imageType&5_group.propertyvalues.operation=equals&5_group.propertyvalues.0_values=asset-type:manual&orderby=@jcr:content/jcr:lastModified&orderby.sort=desc&layout=card&p.offset=0"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 0    Request: {search}");
                return;

            }
            var results_html = Regex.Matches(search_result, @"<article data-asset-share-id=""asset""[\w\W]*?</article>");

            foreach (var result in results_html)
            {
                var page_reg = Regex.Matches(result.ToString(), @"(?<=<a href="")[\w\W]*?(?="">)");
                if (page_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 1",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 1  Request: {search}");
                    return;
                }
                var name_reg = Regex.Matches(result.ToString(), @$"(?<=<a href=""{page_reg[0].Value}"">)[\w\W]*?(?=</a>)");

                if (name_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 2",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 2  Request: {search}");
                    return;
                }
                string title = name_reg[0].Value.Trim();

                var href_reg = Regex.Matches(result.ToString(), @"(?<=<button class=""ui link button"" data-asset-share-id=""download-asset"" data-asset-share-asset="")[\w\W]*?(?="">Download</button>)");

                if (href_reg.Count == 0)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = TextConvert.ToBase64String(result.ToString()),
                        Base64errorData = null,
                        Comment = $"[{type}] STAGE 3",
                        Source = "whirlpooldigitalassets.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on whirlpooldigitalassets.com [{type}] registered. STAGE - 3  Request: {search}");
                    return;
                }

                string href = href_reg[0].Value;

                //--------------------------------------------------------------------------------
                TechSheetPDFResponse data = new()
                {
                    Priority = priority.PartlistPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                };
                con.Open();
                string type1 = "TechSheetPDF";
                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(data));
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type1}', '{DataEscaped}');");
                con.Close();
                //--------------------------------------------------------------------------------

                thisList.Add(new()
                {
                    Priority = priority.TechSheetPDFPriority,
                    Source = "whirlpooldigitalassets.com",
                    Title = title,
                    URL = ShortUrl.MakeShortURL($"https://www.whirlpooldigitalassets.com{href}")
                });
            }


        }
    }
}
