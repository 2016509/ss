using Dapper;
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
    public class SearsPartsDirect
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList, List<PhotosFromSites> PhotosFromSitesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='SearsPartsDirect'");
            var searspartdata = con.QueryFirstOrDefault<DatabaseSearsPartsDirectData>($"SELECT * FROM searspartsdirect_data");
            con.Close();

            string search_result = null;
            try
            {



                string operationName = "modelSearch";
                string variables = JsonConvert.SerializeObject(new SearsPartsDirectClasses.VariablesConstructor.Variables() { Q = search.ToLower() });
                string extensions = JsonConvert.SerializeObject(
                    new SearsPartsDirectClasses.ExtesionsConstructor.Extensions()
                    {
                        PersistedQuery = new()
                        {
                            Version = 1,
                            Sha256Hash = searspartdata.SHA256Hash
                        }
                    });


                search_result = CustomHttpClass.GetToString(
                    url: $"https://catalog-staging.partsdirect.io/graphql?operationName={operationName}&variables={variables}&extensions={extensions}",
                    headers: new() {
                        new() { Name = "x-trace-id", Value = searspartdata.TraceId},
                        new() { Name = "x-api-key", Value = searspartdata.ApiKey },
                        new() { Name = "X-Apollo-Operation-Name", Value = "modelSearch" }
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
                    Comment = "STAGE 1",
                    Source = "searspartsdirect.com",
                    Url = null

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on searspartsdirect.com registered. STAGE - 1    Request: {search}");
                return;
            }

            if (!string.IsNullOrEmpty(search_result))
            {
                try
                {
                    dynamic _js = JsonConvert.DeserializeObject(search_result);
                    string model_page_html = CustomHttpClass.GetToString($"https://www.searspartsdirect.com/model/{_js.data.modelSearch.models[0].id}");

                    var title_regex = Regex.Matches(model_page_html, @"(?<=<h1>).*?(?=</h1>)");
                    if (title_regex.Count() > 0)
                    {

                        //-------------------------------------------------------------------------------------------
                        DiagramWebResponse dw = new()
                        {
                            SearchUrl = $"https://www.searspartsdirect.com/search?q={search.ToLower()}#modeltab",
                            Priority = priority.DiagramWEBPriority,
                            ResultCount = _js.data.modelSearch.totalCount.ToString(),
                            Source = "searspartsdirect.com",
                            Title = $"{title_regex.First()}",
                            Url = $"https://www.searspartsdirect.com/model/{_js.data.modelSearch.models[0].id}",
                            Version = null
                        };
                        con.Open();
                        string type = "DiagramWeb";
                        string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                        con.Close();
                        //-------------------------------------------------------------------------------------------

                        DiagramWebResponseList.Add(new()
                        {
                            SearchUrl = $"https://www.searspartsdirect.com/search?q={search.ToLower()}#modeltab",
                            Priority = priority.DiagramWEBPriority,
                            ResultCount = _js.data.modelSearch.totalCount.ToString(),
                            Source = "searspartsdirect.com",
                            Title = $"{title_regex.First()}",
                            Url = $"https://www.searspartsdirect.com/model/{_js.data.modelSearch.models[0].id}",
                            Version = null
                        });

                        if(_js.data.modelSearch.models[0].number.ToString().Equals(search))
                        {
                            try
                            {
                                PhotosFromSitesList.Add(new()
                                {
                                    PhotoURL = _js.data.modelSearch.models[0].images[0].ToString(),
                                    Priority = priority.PhotoPriority,
                                    Source = "searspartsdirect.com"
                                });
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                    {
                        //-------------------------------------------------------------------------------------------
                        DiagramWebResponse dw = new()
                        {
                            SearchUrl = $"https://www.searspartsdirect.com/search?q={search.ToLower()}#modeltab",
                            Priority = priority.DiagramWEBPriority,
                            ResultCount = _js.data.modelSearch.totalCount.ToString(),
                            Source = "searspartsdirect.com",
                            Title = $"{_js.data.modelSearch.models[0].brand.name} {_js.data.modelSearch.models[0].title}",
                            Url = $"https://www.searspartsdirect.com/model/{_js.data.modelSearch.models[0].id}",
                            Version = null
                        };
                        con.Open();
                        string type = "DiagramWeb";
                        string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(dw));
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                        con.Close();
                        //-------------------------------------------------------------------------------------------

                        DiagramWebResponseList.Add(new()
                        {
                            SearchUrl = $"https://www.searspartsdirect.com/search?q={search.ToLower()}#modeltab",
                            Priority = priority.DiagramWEBPriority,
                            ResultCount = _js.data.modelSearch.totalCount.ToString(),
                            Source = "searspartsdirect.com",
                            Title = $"{_js.data.modelSearch.models[0].brand.name} {_js.data.modelSearch.models[0].title}",
                            Url = $"https://www.searspartsdirect.com/model/{_js.data.modelSearch.models[0].id}",
                            Version = null
                        });

                        if (_js.data.modelSearch.models[0].number.ToString().Equals(search))
                        {
                            try
                            {
                                PhotosFromSitesList.Add(new()
                                {
                                    PhotoURL = _js.data.modelSearch.models[0].images[0].ToString(),
                                    Priority = priority.PhotoPriority,
                                    Source = "searspartsdirect.com"
                                });
                            }
                            catch
                            {

                            }
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
                        Base64wrongData = TextConvert.ToBase64String(search_result),
                        Base64errorData = TextConvert.ToBase64String(ex.Message),
                        Comment = "STAGE 2",
                        Source = "searspartsdirect.com",
                        Url = null

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on searspartsdirect.com registered. STAGE - 2    Request: {search}");
                    return;
                }
            }

        }
    }
}
