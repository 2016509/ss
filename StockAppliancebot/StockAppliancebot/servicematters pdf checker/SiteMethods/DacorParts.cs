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
    public class DacorParts
    {
        public static void Parsing(DatabaseTotalResults request, List<PartListPDFResponse> PartListPDFResponseList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='DacorParts'");
            con.Close();

            string search_result = null;

            try
            {
                search_result = CustomHttpClass.GetToString($"https://www.dacorparts.com/model-lookup/", new List<CustomHttpAdditionals.Headers> { });
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
                    Comment = "STAGE 0",
                    Url = null,
                    Source = "coastparts.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on DacorParts registered. STAGE - 1    Request: {search}");
                return;
            }

            string categories_pattern = @"(?<=<strong><a id="").*?(?="" class=)";
            var categories_regex = Regex.Matches(search_result, categories_pattern, RegexOptions.IgnoreCase);
            foreach (var category_name in categories_regex)
            {
                var category_html_regex = Regex.Matches(search_result, $@"(?<=<a id=""{category_name}"")[\w\W]*?(?=</a></p>)", RegexOptions.IgnoreCase)[0].Value;

                var names_in_category = Regex.Matches(category_html_regex, @"(?<=class=""lookuplink"" title="").*?(?="" href="")", RegexOptions.IgnoreCase);
                var urls_in_category = Regex.Matches(category_html_regex, @"(?<="" href="").*?(?="" target=""_blank"")", RegexOptions.IgnoreCase);

                for (int i = 0; i < names_in_category.Count; i++)
                {
                    if (names_in_category[i].Value.ToString().Equals(search))
                    {
                        //----------------------------------------------------------------------------
                        PartListPDFResponse pl = new()
                        {
                            Category = category_name.ToString(),
                            Priority = priority.PartlistPDFPriority,
                            Source = "dacorparts.com",
                            Title = names_in_category[i].Value,
                            URL = $"{ShortUrl.MakeShortURL($"{urls_in_category[i].Value}")}"
                        };
                        con.Open();
                        string type = "PartListPDF";
                        string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(pl));
                        
                        con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                        con.Close();
                        //----------------------------------------------------------------------------
                        PartListPDFResponseList.Add(new()
                        {
                            Category = category_name.ToString(),
                            Priority = priority.PartlistPDFPriority,
                            Source = "dacorparts.com",
                            Title = names_in_category[i].Value,
                            URL = $"{ShortUrl.MakeShortURL($"{urls_in_category[i].Value}")}"
                        });

                    }
                }
            }

        }

    }
}
