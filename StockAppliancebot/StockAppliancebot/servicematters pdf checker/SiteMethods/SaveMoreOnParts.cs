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
    public class SaveMoreOnParts
    {
        public static void Parsing(DatabaseTotalResults request, List<DiagramWebResponse> DiagramWebResponseList)
        {
            string search = request.Request;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var priority = con.QueryFirstOrDefault<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='SaveMoreOnParts'");
            con.Close();

            string total_search = search.Split(' ')[0].ToString();
            string search_result_uri_encoded = null;

            try
            {
                search_result_uri_encoded = CustomHttpClass.GetToString($"https://www.savemoreonparts.com/Home/SearchPartModelList?searchString={total_search}&type=model");

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
                    Comment = "STAGE 0",
                    Source = "savemoreonparts.com",
                    Url = $"https://www.savemoreonparts.com/Home/SearchPartModelList?searchString={total_search}&type=model"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on savemoreonparts.com registered. STAGE - 0    Request: {search}");
                return;
            }
            string search_result = Regex.Unescape(search_result_uri_encoded);
            
            var name_reg = Regex.Matches(search_result, @"(?<=<div class=""model_link"" style=""width:100%; float:left;"">)[\w\W]*?(?=<span>)");
            var href_reg = Regex.Matches(search_result, @"(?<=<a id=""item"" href="")[\w\W]*?(?="">)");

            if (name_reg.Count > 0 && href_reg.Count > 0 && name_reg.Count == href_reg.Count)
            {
                //-------------------------------------------------------------------
                DiagramWebResponse dw = new()
                {
                    Priority = priority.DiagramWEBPriority,
                    SearchUrl = null,
                    Source = "savemoreonparts.com",
                    Title = name_reg[0].ToString().Trim(),
                    Url = $"https://www.savemoreonparts.com{href_reg[0]}"
                };
                con.Open();
                string type = "DiagramWeb";
                string DataEscaped = JsonConvert.SerializeObject(dw);
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                con.Close();
                //-------------------------------------------------------------------

                DiagramWebResponseList.Add(new()
                {
                    Priority = priority.DiagramWEBPriority,
                    SearchUrl = null,
                    Source = "savemoreonparts.com",
                    Title = name_reg[0].ToString().Trim(),
                    Url = $"https://www.savemoreonparts.com{href_reg[0]}"
                });
            }
        }

    }
}
