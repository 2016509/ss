using Dapper;
using Leaf.xNet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockAppliance.DatabaseClasses;
using StockAppliance.Methods;
using StockAppliance.ResponseClasses;
using StockAppliance.Settings;
using StockAppliance.SiteMethods.Classes;
using System.Text.RegularExpressions;

namespace StockAppliance.SiteMethods
{
    public class LGParts
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList, List<ServiceManualPDFResponse> ServiceManualPDFResponseList, List<PhotosFromSites> PhotosFromSitesList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='LGParts'");
            con.Close();
            string search_result = null;

            try
            {
                search_result = CustomHttpClass.GetToString($"https://lgparts.com/search?type=product&q=\"{search}\"&view=json", new List<CustomHttpAdditionals.Headers> { });
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
                    Comment = "STAGE 0",
                    Source = "lgparts.com",
                    Url = $"https://lgparts.com/search?type=product&q=\"{search}\"&view=json"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on LFParts.com registered. STAGE - 0    Request: {search}");
                return;

            }
            string order_url = "";
            string total_title = "";
            string search_result_count = "All";
            try
            {
                dynamic _js = JsonConvert.DeserializeObject(search_result);

                for (int i = 0; i < _js.results.Count; i++)
                {
                    if (_js.results[i].title.ToString().Contains(search))
                    {
                        order_url = _js.results[i].url;
                        total_title = _js.results[i].title.ToString();
                        search_result_count = _js.results.Count.ToString();
                        break;
                    }
                }
            }
            catch  (Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = null,
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 1",
                    Source = "lgparts.com",
                    Url = $"https://lgparts.com/search?type=product&q=\"{search}\"&view=json"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on LFParts.com registered. STAGE - 1    Request: {search}");
                return;
                
            }

            if (string.IsNullOrEmpty(order_url)) return;
            string search_result_second = null;

            try
            {
                search_result_second = CustomHttpClass.GetToString($"https://lgparts.com{order_url}", new List<CustomHttpAdditionals.Headers> { });
            }
            catch (Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = TextConvert.ToBase64String(search_result_second),
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 2",
                    Source = "lgparts.com",
                    Url = $"https://lgparts.com{order_url}"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on LFParts.com registered. STAGE - 2    Request: {search}");
                return;

            }
            var photo_regex = Regex.Matches(search_result_second, @"(?<=<meta property=""og:image:secure_url"" content="").*(?="">)");
            if (photo_regex.Count > 0) PhotosFromSitesList.Add(new()
            {
                PhotoURL = photo_regex.First().Value,
                Priority = priority.PhotoPriority,
                Source = "lgparts.com"
            });
            string patern = @"(?<=window.BOLD.subscriptions.data.platform.product = ).*(?=;)";
            var regex = Regex.Matches(search_result_second, patern, RegexOptions.IgnoreCase);
            string model_data_versions = regex[0].Value;
            string handle = "";
            List<LGPartsClasses.Variants> variants = new();
            try
            {
                dynamic _js = JsonConvert.DeserializeObject(model_data_versions);
                for (int i = 0; i < _js.variants.Count; i++)
                {
                    variants.Add(new()
                    {
                        Barcode = _js.variants[i].barcode,
                        SKU = _js.variants[i].sku,
                        Title = _js.variants[i].title

                    });
                }

                handle = _js.handle;
            }

            catch (Exception ex)
            {
                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = TextConvert.ToBase64String(search_result_second),
                    Base64errorData = TextConvert.ToBase64String(ex.Message),
                    Comment = "STAGE 3",
                    Source = "lgparts.com",
                    Url = $"https://lgparts.com{order_url}"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on LFParts.com registered. STAGE - 3    Request: {search}");
                return;
                
            }

            if (variants.Count == 0) return;

            foreach (var variant in variants)
            {

                RequestParams rp = new();
                rp["action"] = "get_variants_details";
                rp["shop"] = "lgparts.com";
                rp["model_id"] = $"{variant.Barcode}";
                rp["model_variation"] = variant.Title;
                rp["product_handle"] = handle;
                rp["user_id"] = "";
                rp["model_number"] = variant.SKU;
                rp["variantCount"] = variants.Count;

                string this_version_data = CustomHttpClass.Post(
                    url: "https://fridgeparts.us/lgparts/admin_ajax.php",
                    data: rp,
                    headers: new List<CustomHttpAdditionals.Headers>()
                    );
                var buttons_regex = Regex.Matches(this_version_data, @"<div class=""model-links"">[\w\W]*?</div>");

                if (buttons_regex.Count > 0)
                {
                    var buttons_names = Regex.Matches(buttons_regex[0].Value, @"(?<=>).*?(?=</a>)");
                    var hrefs = Regex.Matches(buttons_regex[0].Value, @"(?<= href="").*(?="" target=""_blank"">)");

                    if (hrefs.Count == buttons_names.Count)
                    {
                        for (int i = 0; i < hrefs.Count; i++)
                        {

                            string href = hrefs[i].Value;
                            string name_data = buttons_names[i].Value;

                            if (name_data.Contains("Service Manual"))
                            {

                                //------------------------------------------------------------
                                ServiceManualPDFResponse sm = new()
                                {
                                    URL = ShortUrl.MakeShortURL(href),
                                    Version = variant.Title,
                                    Source = "lgparts.com",
                                    Title = total_title
                                };
                                con.Open();
                                string type = "ServiceManualPDF";
                                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(sm));
                                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                                con.Close();
                                //------------------------------------------------------------


                                ServiceManualPDFResponseList.Add(new()
                                {
                                    URL = ShortUrl.MakeShortURL(href),
                                    Version = variant.Title,
                                    Source = "lgparts.com",
                                    Title = total_title
                                });
                                continue;
                            }
                            if (name_data.Contains("Interactive Exploded View"))
                            {

                                //------------------------------------------------------------
                                DiagramWebResponse dw = new()
                                {
                                    Version = variant.Title,
                                    Priority = priority.DiagramWEBPriority,
                                    Source = "lgparts.com",
                                    Title = total_title,
                                    Url = href,
                                    ResultCount = search_result_count,
                                    SearchUrl = $@"https://lgparts.com/search?type=product&q=""{search}"""
                                };
                                con.Open();
                                string type = "DiagramWeb";
                                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                                con.Close();
                                //------------------------------------------------------------


                                DiagramWebResponseList.Add(new()
                                {
                                    Version = variant.Title,
                                    Priority = priority.DiagramWEBPriority,
                                    Source = "lgparts.com",
                                    Title = total_title,
                                    Url = href,
                                    ResultCount = search_result_count,
                                    SearchUrl = $@"https://lgparts.com/search?type=product&q=""{search}"""
                                });
                            }



                        }


                    }


                }
                
            }


        }
    }
}
