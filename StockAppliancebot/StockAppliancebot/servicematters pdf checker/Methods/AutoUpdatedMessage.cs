using Dapper;
using MySql.Data.MySqlClient;
using StockAppliance.BotFunctions;
using StockAppliance.DatabaseClasses;
using StockAppliance.ResponseClasses;
using StockAppliance.Settings;
using StockAppliance.SiteMethods;
using System.Globalization;
using Newtonsoft.Json;
using StockAppliance.Methods;
using Telegram.Bot;

namespace StockAppliance.Methods
{
    public class AutoUpdatedMessage
    {
        public static async void MainUpdater (ITelegramBotClient botClient, CancellationToken cancellationToken, DatabaseTotalResults request, List<Task> allTasks)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var settings = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={request.ChatID}");
            con.Close();

            var DiagramWebResponseList = new List<DiagramWebResponse> { };
            var ServiceManualPDFResponseList = new List<ServiceManualPDFResponse> { };
            var PartListPDFResponseList = new List<PartListPDFResponse> { };
            var ServicePointerPDFResponseList = new List<ServicePointerPDFResponse> { };
            var ServiceManualWEBResponseList = new List<ServiceManualWEBResponse> { };
            var TechSheetPDFResponseList = new List<TechSheetPDFResponse> { };
            var WiringDiagramPDFResponseList = new List<WiringDiagramPDFResponse> { };

            var PhotosFromSitesList = new List<PhotosFromSites> { };

            string search_view_one = "⏳";
            string search_view_two = "⌛️";
            string now_search_view = search_view_one;
            string now_text = $"{now_search_view} Searching..";

            var sended_message = await botClient.SendTextMessageAsync(
              chatId: request.ChatID,
              text: $"{now_search_view} Searching..",
              replyToMessageId: int.Parse(request.MessageID.ToString()),
              parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
              disableWebPagePreview: true,
              cancellationToken: cancellationToken
              );



            con.Open();
            con.QueryFirstOrDefault($"UPDATE totalresults SET `botMessageID`='{sended_message.MessageId}' WHERE `ID`='{request.ID}'");
            con.Close();
            request.BotMessageID = sended_message.MessageId;

            int response_count = 0;

            while (!AllTaskReady(allTasks))
            {
                DiagramWebResponseList.Clear();
                ServiceManualPDFResponseList.Clear();
                PartListPDFResponseList.Clear();
                ServicePointerPDFResponseList.Clear();
                ServiceManualWEBResponseList.Clear();
                TechSheetPDFResponseList.Clear();
                WiringDiagramPDFResponseList.Clear();

                var results_in_db = con.Query<DatabaseResponseTempDB>($"SELECT * FROM response_temp_db WHERE `RequestID`={request.ID};");
                if (results_in_db.Count() == response_count)
                {
                    if (now_search_view.Equals("⏳"))
                    {
                        try
                        {
                            sended_message = await botClient.EditMessageTextAsync(
                          chatId: request.ChatID,
                          text: $"{now_text.Replace(now_search_view, "⌛️")}",
                          messageId: int.Parse(request.BotMessageID.ToString()),
                          parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                          disableWebPagePreview: true,
                          cancellationToken: cancellationToken
                          );

                            Thread.Sleep(500);
                            now_text = $"{now_text.Replace(now_search_view, "⌛️")}";
                            now_search_view = "⌛️";

                            continue;
                        }
                        catch
                        {
                            break;  
                        }
                    }

                    if (now_search_view.Equals("⌛️"))
                    {
                        sended_message = await botClient.EditMessageTextAsync(
                      chatId: request.ChatID,
                      text: $"{now_text.Replace(now_search_view, "⏳")}",
                      messageId: int.Parse(request.BotMessageID.ToString()),
                      parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                      disableWebPagePreview: true,
                      cancellationToken: cancellationToken
                      );

                        Thread.Sleep(500);
                        now_text = $"{now_text.Replace(now_search_view, "⌛️")}";
                        now_search_view = "⏳";
                        
                        continue;

                    }





                }
                else response_count = results_in_db.Count();

                foreach (var result in results_in_db)
                {
                    switch (result.Type)
                    {
                        case "DiagramWeb":
                            DiagramWebResponseList.Add(JsonConvert.DeserializeObject<DiagramWebResponse>(result.Data));
                            break;

                        case "ServiceManualPDF":
                            ServiceManualPDFResponseList.Add(JsonConvert.DeserializeObject<ServiceManualPDFResponse>(result.Data));
                            break;

                        case "PartListPDF":
                            PartListPDFResponseList.Add(JsonConvert.DeserializeObject<PartListPDFResponse>(result.Data));
                            break;

                        case "ServicePointerPDF":
                            ServicePointerPDFResponseList.Add(JsonConvert.DeserializeObject<ServicePointerPDFResponse>(result.Data));
                            break;

                        case "ServiceManualWEB":
                            ServiceManualWEBResponseList.Add(JsonConvert.DeserializeObject<ServiceManualWEBResponse>(result.Data));
                            break;

                        case "TechSheetPDF":
                            TechSheetPDFResponseList.Add(JsonConvert.DeserializeObject<TechSheetPDFResponse>(result.Data));
                            break;

                        case "WiringDiagramPDF":
                            WiringDiagramPDFResponseList.Add(JsonConvert.DeserializeObject<WiringDiagramPDFResponse>(result.Data));
                            break;
                        default: break;




                    }
                }




                //Поиск строк, которые должны выдаватсья рандомно
                List <DiagramWebResponse> RandomDiagramWebResponseList = new();
                List <ServiceManualPDFResponse> RandomServiceManualPDFResponseList = new();
                List <PartListPDFResponse> RandomPartListPDFResponseList = new();
                List <ServicePointerPDFResponse> RandomServicePointerPDFResponseList = new();
                List <ServiceManualWEBResponse> RandomServiceManualWEBResponseList = new();
                List <TechSheetPDFResponse> RandomTechSheetPDFResponseList = new();
                List <WiringDiagramPDFResponse> RandomWiringDiagramPDFResponseList = new();


                //Удаление строк из первоначального списка
                /*DiagramWebResponseList.RemoveAll(FindRandomResult);
                ServiceManualPDFResponseList.RemoveAll(FindRandomResult);
                PartListPDFResponseList.RemoveAll(FindRandomResult);
                ServicePointerPDFResponseList.RemoveAll(FindRandomResult);
                ServiceManualWEBResponseList.RemoveAll(FindRandomResult);
                TechSheetPDFResponseList.RemoveAll(FindRandomResult);
                WiringDiagramPDFResponseList.RemoveAll(FindRandomResult);*/


                //Создаем списки для сокращенной выдачи
                List<string> DiagramWebCollapsedResponse = new();
                List<string> ServiceManualPDFCollapsedResponse = new();
                List<string> PartListPDFCollapsedResponse = new();
                List<string> ServicePointerPDFCollapsedResponse = new();
                List<string> ServiceManualWEBCollapsedResponse = new();
                List<string> TechSheetPDFCollapsedResponse = new();
                List<string> WiringDiagramPDFCollapsedResponse = new();

                //Создаем списки для полной выдачи
                List<string> DiagramWebFullResponse = new();
                List<string> ServiceManualPDFFullResponse = new();
                List<string> PartListPDFFullResponse = new();
                List<string> ServicePointerPDFFullResponse = new();
                List<string> ServiceManualWEBFullResponse = new();
                List<string> TechSheetPDFFullResponse = new();
                List<string> WiringDiagramPDFFullResponse = new();


                MakeDiagramWEBResponse(
                    DiagramWebCollapsedResponse,
                    DiagramWebFullResponse,
                    DiagramWebResponseList,
                    RandomDiagramWebResponseList,
                    settings,
                    request.Request);
                MakePartListPDFResponse(
                    PartListPDFCollapsedResponse,
                    PartListPDFFullResponse,
                    PartListPDFResponseList,
                    RandomPartListPDFResponseList,
                    settings,
                    request.Request);
                MakeTechSheetPDFResponse(
                    TechSheetPDFCollapsedResponse,
                    TechSheetPDFFullResponse,
                    TechSheetPDFResponseList,
                    RandomTechSheetPDFResponseList,
                    settings,
                    request.Request);

                MakeServiceManualPDFPDFResponse(
                    ServiceManualPDFCollapsedResponse,
                    ServiceManualPDFFullResponse,
                    ServiceManualPDFResponseList,
                    RandomServiceManualPDFResponseList,
                    settings,
                    request.Request);

                MakeWiringDiagramPDFResponse(
                    WiringDiagramPDFCollapsedResponse,
                    WiringDiagramPDFFullResponse,
                    WiringDiagramPDFResponseList,
                    RandomWiringDiagramPDFResponseList,
                    settings,
                    request.Request);
                MakeServicePointerPDFResponse(
                    ServicePointerPDFCollapsedResponse,
                    ServicePointerPDFFullResponse,
                    ServicePointerPDFResponseList,
                    RandomServicePointerPDFResponseList,
                    settings,
                    request.Request);
                MakeServiceManualWEBResponse(
                    ServiceManualWEBCollapsedResponse,
                    ServiceManualWEBFullResponse,
                    ServiceManualWEBResponseList,
                    RandomServiceManualWEBResponseList,
                    settings,
                    request.Request);

                string text = MakeCollapsedResponse(
                request,
                DiagramWebCollapsedResponse,
                PartListPDFCollapsedResponse,
                TechSheetPDFCollapsedResponse,
                ServiceManualPDFCollapsedResponse,
                WiringDiagramPDFCollapsedResponse, ServicePointerPDFCollapsedResponse,
                ServiceManualWEBCollapsedResponse
                );


                if (now_search_view.Equals("⏳"))
                {
                    now_search_view = "⌛️";
                    sended_message = await botClient.EditMessageTextAsync(
                      chatId: request.ChatID,
                      text: text + Environment.NewLine + Environment.NewLine + $"{now_search_view} Searching..",
                      messageId: int.Parse(request.BotMessageID.ToString()),
                      parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                      disableWebPagePreview: true,
                      cancellationToken: cancellationToken
                      );



                    now_text = text + Environment.NewLine + Environment.NewLine + $"{now_search_view} Searching..";
                    Thread.Sleep(500);
                    continue;
                }
                if (now_search_view.Equals("⌛️"))
                {
                    now_search_view = "⏳";
                    sended_message = await botClient.EditMessageTextAsync(
                      chatId: request.ChatID,
                      text: text + Environment.NewLine + Environment.NewLine + $"{now_search_view} Searching..",
                      messageId: int.Parse(request.BotMessageID.ToString()),
                      parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                      disableWebPagePreview: true,
                      cancellationToken: cancellationToken
                      );



                    now_text = text + Environment.NewLine + Environment.NewLine + $"{now_search_view} Searching..";
                    Thread.Sleep(500);
                    continue;
                }

                


            }

            con.Open();
            con.QueryFirstOrDefault($"DELETE FROM `response_temp_db` WHERE  `RequestID`={request.ID};");
            con.Close();

        }


        private static bool AllTaskReady (List<Task> allTasks)
        {
            foreach(var task in allTasks)
            {
                if (!task.IsCompleted) return false;
            }
            return true;
        }

        private static bool FindRandomResult(dynamic diagram)
        {

            if (diagram.Priority == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string ModifyTitle(string title, string search)
        {
            if (title == null) return title;
            string new_search = search.ToUpper();
            while (true)
            {
                if (new_search.Length <= 2) break; ;

                if (title.Contains(new_search))
                {
                    return title.Replace(new_search, $"<b>{new_search}</b>");
                }
                else
                {
                    new_search = new_search.Remove(new_search.Length - 1, 1);
                }
            }

            new_search = search.ToLower();

            while (true)
            {
                if (new_search.Length <= 2) return title;

                if (title.Contains(new_search))
                {
                    return title.Replace(new_search, $"<b>{new_search}</b>");
                }
                else
                {
                    new_search = new_search.Remove(new_search.Length - 1, 1);
                }
            }
        }


        /// <summary>
        /// Make a Diagram Web Response collapsed and full Response lists
        /// </summary>
        /// <param name="DiagramWebCollapsedResponse"></param>
        /// <param name="DiagramWebFullResponse"></param>
        /// <param name="DiagramWebResponseList"></param>
        /// <param name="RandomDiagramWebResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakeDiagramWEBResponse(
            List<string> DiagramWebCollapsedResponse,
            List<string> DiagramWebFullResponse,
            List<DiagramWebResponse> DiagramWebResponseList,
            List<DiagramWebResponse> RandomDiagramWebResponseList,
            DatabaseUserData settings,
            string search)

        {
            if (settings == null) return;
            DiagramWebResponseList = DiagramWebResponseList.OrderBy(x => x.Priority).ToList();
            RandomDiagramWebResponseList = RandomDiagramWebResponseList.OrderBy(x => x.Priority).ToList();
            var textInfo = CultureInfo.CurrentCulture.TextInfo;

            foreach (var diagramWebResponse in DiagramWebResponseList)
            {
                diagramWebResponse.Source = char.ToUpper(diagramWebResponse.Source[0]) + diagramWebResponse.Source.Substring(1);
            }

            foreach (var diagramWebResponse in RandomDiagramWebResponseList)
            {
                diagramWebResponse.Source = char.ToUpper(diagramWebResponse.Source[0]) + diagramWebResponse.Source.Substring(1);
            }

            while (DiagramWebCollapsedResponse.Count != settings.CountDiagramWEB && (DiagramWebResponseList.Count != 0 || RandomDiagramWebResponseList.Count != 0)
                )
            {
                if (DiagramWebResponseList.Count != 0)
                {
                    var taken = DiagramWebResponseList.First();
                    DiagramWebResponseList.Remove(taken);

                    string data = @$"<b><u>{taken.Source}</u></b>-> ";

                    if (taken.SearchUrl != null && !taken.ResultCount.Equals("0") && !taken.ResultCount.Equals("1")) data += $@"<a href='{taken.SearchUrl}'>{taken.ResultCount} results>>></a> ";

                    if (taken.Version != null)
                    {
                        data += $"| {ModifyTitle(taken.Title, search)} (Version: {taken.Version}) -> <a href='{taken.Url}'>>>> </a>";
                    }
                    else
                    {
                        data += $"| {ModifyTitle(taken.Title, search)} -> <a href='{taken.Url}'>>>> </a>";
                    }

                    DiagramWebCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomDiagramWebResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomDiagramWebResponseList.ElementAt(rand.Next(RandomDiagramWebResponseList.Count));
                    RandomDiagramWebResponseList.Remove(taken);
                    string data = @$"<b><u>{taken.Source}</u></b>-> ";
                    if (taken.SearchUrl != null && !taken.ResultCount.Equals("0") && !taken.ResultCount.Equals("1")) data += $@"<a href='{taken.SearchUrl}'>{taken.ResultCount} results>>></a> ";
                    if (taken.Version != null)
                    {
                        data += $"| {ModifyTitle(taken.Title, search)} (Version: {taken.Version}) -> <a href='{taken.Url}'>>>> </a>";
                    }
                    else
                    {
                        data += $"| {ModifyTitle(taken.Title, search)} -> <a href='{taken.Url}'>>>> </a>";
                    }

                    DiagramWebCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in DiagramWebResponseList)
            {

                string data = @$"<b><u>{taken.Source}</u></b>-> ";
                if (taken.SearchUrl != null && !taken.ResultCount.Equals("0") && !taken.ResultCount.Equals("1")) data += $@"<a href='{taken.SearchUrl}'>{taken.ResultCount} results>>></a> ";
                if (taken.Version != null)
                {
                    data += $"{ModifyTitle(taken.Title, search)} (Version: {taken.Version}) -> <a href='{taken.Url}'> >>> </a>";
                }
                else
                {
                    data += $"{ModifyTitle(taken.Title, search)} -> <a href='{taken.Url}'> >>> </a>";
                }

                DiagramWebFullResponse.Add(data);
            }

            foreach (var taken in RandomDiagramWebResponseList)
            {

                string data = @$"<b><u>{taken.Source}</u></b>-> ";
                if (taken.SearchUrl != null && !taken.ResultCount.Equals("0") && !taken.ResultCount.Equals("1")) data += $@"<a href='{taken.SearchUrl}'>{taken.ResultCount} results>>></a> ";
                if (taken.Version != null)
                {
                    data += $"{ModifyTitle(taken.Title, search)} (Version: {taken.Version}) -> <a href='{taken.Url}'> >>> </a>";
                }
                else
                {
                    data += $"{ModifyTitle(taken.Title, search)} -> <a href='{taken.Url}'> >>> </a>";
                }

                DiagramWebFullResponse.Add(data);
            }
            DiagramWebResponseList.Clear();
            RandomDiagramWebResponseList.Clear();

        }


        /// <summary>
        /// Make a Part list PDF Collapsed and full Response lists
        /// </summary>
        /// <param name="PartListPDFCollapsedResponse"></param>
        /// <param name="PartListPDFFullResponse"></param>
        /// <param name="PartListPDFResponseList"></param>
        /// <param name="RandomPartListPDFResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakePartListPDFResponse(
            List<string> PartListPDFCollapsedResponse,
            List<string> PartListPDFFullResponse,
            List<PartListPDFResponse> PartListPDFResponseList,
            List<PartListPDFResponse> RandomPartListPDFResponseList,
            DatabaseUserData settings,
            string search)

        {
            PartListPDFResponseList = PartListPDFResponseList.OrderBy(x => x.Priority).ToList();
            RandomPartListPDFResponseList = RandomPartListPDFResponseList.OrderBy(x => x.Priority).ToList();

            while (PartListPDFCollapsedResponse.Count != settings.CountPartlistPDF && (PartListPDFResponseList.Count != 0 || RandomPartListPDFResponseList.Count != 0)
                )
            {
                if (PartListPDFResponseList.Count != 0)
                {
                    var taken = PartListPDFResponseList.First();
                    PartListPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    PartListPDFCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomPartListPDFResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomPartListPDFResponseList.ElementAt(rand.Next(RandomPartListPDFResponseList.Count));
                    RandomPartListPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    PartListPDFCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in PartListPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                PartListPDFFullResponse.Add(data);
            }

            foreach (var taken in RandomPartListPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                PartListPDFFullResponse.Add(data);
            }
            RandomPartListPDFResponseList.Clear();
            PartListPDFResponseList.Clear();

        }


        /// <summary>
        /// Make a Tech Sheet PDF collapsed and full Response list
        /// </summary>
        /// <param name="TechSheetPDFCollapsedResponse"></param>
        /// <param name="TechSheetPDFFullResponse"></param>
        /// <param name="TechSheetPDFResponseList"></param>
        /// <param name="RandomTechSheetPDFResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakeTechSheetPDFResponse(
            List<string> TechSheetPDFCollapsedResponse,
            List<string> TechSheetPDFFullResponse,
            List<TechSheetPDFResponse> TechSheetPDFResponseList,
            List<TechSheetPDFResponse> RandomTechSheetPDFResponseList,
            DatabaseUserData settings,
            string search)

        {
            TechSheetPDFResponseList = TechSheetPDFResponseList.OrderBy(x => x.Priority).ToList();
            RandomTechSheetPDFResponseList = RandomTechSheetPDFResponseList.OrderBy(x => x.Priority).ToList();

            while (TechSheetPDFCollapsedResponse.Count != settings.CountTechSheetPDF && (TechSheetPDFResponseList.Count != 0 || RandomTechSheetPDFResponseList.Count != 0)
                )
            {
                if (TechSheetPDFResponseList.Count != 0)
                {
                    var taken = TechSheetPDFResponseList.First();
                    TechSheetPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    TechSheetPDFCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomTechSheetPDFResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomTechSheetPDFResponseList.ElementAt(rand.Next(RandomTechSheetPDFResponseList.Count));
                    RandomTechSheetPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";



                    TechSheetPDFCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in TechSheetPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                TechSheetPDFFullResponse.Add(data);
            }

            foreach (var taken in RandomTechSheetPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                TechSheetPDFFullResponse.Add(data);
            }
            RandomTechSheetPDFResponseList.Clear();
            TechSheetPDFResponseList.Clear();

        }

        /// <summary>
        /// Make a Service Manual PDF collapsed and full Response lists
        /// </summary>
        /// <param name="ServiceManualPDFCollapsedResponse"></param>
        /// <param name="ServiceManualPDFFullResponse"></param>
        /// <param name="ServiceManualPDFResponseList"></param>
        /// <param name="RandomServiceManualPDFResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakeServiceManualPDFPDFResponse(
            List<string> ServiceManualPDFCollapsedResponse,
            List<string> ServiceManualPDFFullResponse,
            List<ServiceManualPDFResponse> ServiceManualPDFResponseList,
            List<ServiceManualPDFResponse> RandomServiceManualPDFResponseList,
            DatabaseUserData settings,
            string search)

        {
            ServiceManualPDFResponseList = ServiceManualPDFResponseList.OrderBy(x => x.Priority).ToList();
            RandomServiceManualPDFResponseList = RandomServiceManualPDFResponseList.OrderBy(x => x.Priority).ToList();

            while (ServiceManualPDFCollapsedResponse.Count != settings.CountServiceManualPDF && (ServiceManualPDFResponseList.Count != 0 || RandomServiceManualPDFResponseList.Count != 0)
                )
            {
                if (ServiceManualPDFResponseList.Count != 0)
                {
                    var taken = ServiceManualPDFResponseList.First();
                    ServiceManualPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    ServiceManualPDFCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomServiceManualPDFResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomServiceManualPDFResponseList.ElementAt(rand.Next(RandomServiceManualPDFResponseList.Count));
                    RandomServiceManualPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";



                    ServiceManualPDFCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in ServiceManualPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                ServiceManualPDFFullResponse.Add(data);
            }

            foreach (var taken in RandomServiceManualPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                ServiceManualPDFFullResponse.Add(data);
            }
            RandomServiceManualPDFResponseList.Clear();
            ServiceManualPDFResponseList.Clear();

        }

        /// <summary>
        /// Make a Wiring Diagram PDF collapsed and full Resposne lists
        /// </summary>
        /// <param name="WiringDiagramPDFCollapsedResponse"></param>
        /// <param name="WiringDiagramPDFFullResponse"></param>
        /// <param name="WiringDiagramPDFResponseList"></param>
        /// <param name="RandomWiringDiagramPDFResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakeWiringDiagramPDFResponse(
            List<string> WiringDiagramPDFCollapsedResponse,
            List<string> WiringDiagramPDFFullResponse,
            List<WiringDiagramPDFResponse> WiringDiagramPDFResponseList,
            List<WiringDiagramPDFResponse> RandomWiringDiagramPDFResponseList,
            DatabaseUserData settings,
            string search)

        {
            WiringDiagramPDFResponseList = WiringDiagramPDFResponseList.OrderBy(x => x.Priority).ToList();
            RandomWiringDiagramPDFResponseList = RandomWiringDiagramPDFResponseList.OrderBy(x => x.Priority).ToList();

            while (WiringDiagramPDFCollapsedResponse.Count != settings.CountWiringSheetPDF && (WiringDiagramPDFResponseList.Count != 0 || RandomWiringDiagramPDFResponseList.Count != 0)
                )
            {
                if (WiringDiagramPDFResponseList.Count != 0)
                {
                    var taken = WiringDiagramPDFResponseList.First();
                    WiringDiagramPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    WiringDiagramPDFCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomWiringDiagramPDFResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomWiringDiagramPDFResponseList.ElementAt(rand.Next(RandomWiringDiagramPDFResponseList.Count));
                    RandomWiringDiagramPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";



                    WiringDiagramPDFCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in WiringDiagramPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                WiringDiagramPDFFullResponse.Add(data);
            }

            foreach (var taken in RandomWiringDiagramPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                WiringDiagramPDFFullResponse.Add(data);
            }
            RandomWiringDiagramPDFResponseList.Clear();
            WiringDiagramPDFResponseList.Clear();

        }


        /// <summary>
        /// Make a Service Pointer PDF collapsed and full Response lists
        /// </summary>
        /// <param name="ServicePointerPDFCollapsedResponse"></param>
        /// <param name="ServicePointerPDFFullResponse"></param>
        /// <param name="ServicePointerPDFResponseList"></param>
        /// <param name="RandomServicePointerPDFResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakeServicePointerPDFResponse(
            List<string> ServicePointerPDFCollapsedResponse,
            List<string> ServicePointerPDFFullResponse,
            List<ServicePointerPDFResponse> ServicePointerPDFResponseList,
            List<ServicePointerPDFResponse> RandomServicePointerPDFResponseList,
            DatabaseUserData settings,
            string search)

        {
            ServicePointerPDFResponseList = ServicePointerPDFResponseList.OrderBy(x => x.Priority).ToList();
            RandomServicePointerPDFResponseList = RandomServicePointerPDFResponseList.OrderBy(x => x.Priority).ToList();

            while (ServicePointerPDFCollapsedResponse.Count != settings.CountServicePointerPDF && (ServicePointerPDFResponseList.Count != 0 || RandomServicePointerPDFResponseList.Count != 0)
                )
            {
                if (ServicePointerPDFResponseList.Count != 0)
                {
                    var taken = ServicePointerPDFResponseList.First();
                    ServicePointerPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    ServicePointerPDFCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomServicePointerPDFResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomServicePointerPDFResponseList.ElementAt(rand.Next(RandomServicePointerPDFResponseList.Count));
                    RandomServicePointerPDFResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";



                    ServicePointerPDFCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in ServicePointerPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                ServicePointerPDFFullResponse.Add(data);
            }

            foreach (var taken in RandomServicePointerPDFResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>({taken.Category})</i> ";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                ServicePointerPDFFullResponse.Add(data);
            }
            RandomServicePointerPDFResponseList.Clear();
            ServicePointerPDFResponseList.Clear();

        }


        /// <summary>
        /// Make a Service Manual WEB collapsed and full Response lists
        /// </summary>
        /// <param name="ServiceManualWEBCollapsedResponse"></param>
        /// <param name="ServiceManualWEBFullResponse"></param>
        /// <param name="ServiceManualWEBResponseList"></param>
        /// <param name="RandomServiceManualWEBResponseList"></param>
        /// <param name="settings"></param>
        /// <param name="search"></param>
        private static void MakeServiceManualWEBResponse(
            List<string> ServiceManualWEBCollapsedResponse,
            List<string> ServiceManualWEBFullResponse,
            List<ServiceManualWEBResponse> ServiceManualWEBResponseList,
            List<ServiceManualWEBResponse> RandomServiceManualWEBResponseList,
            DatabaseUserData settings,
            string search)

        {
            ServiceManualWEBResponseList = ServiceManualWEBResponseList.OrderBy(x => x.Priority).ToList();
            RandomServiceManualWEBResponseList = RandomServiceManualWEBResponseList.OrderBy(x => x.Priority).ToList();

            while (ServiceManualWEBCollapsedResponse.Count != settings.CountServiceManualWEB && (ServiceManualWEBResponseList.Count != 0 || RandomServiceManualWEBResponseList.Count != 0)
                )
            {
                if (ServiceManualWEBResponseList.Count != 0)
                {
                    var taken = ServiceManualWEBResponseList.First();
                    ServiceManualWEBResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>{taken.Category}</i>";
                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";


                    ServiceManualWEBCollapsedResponse.Add(data);
                    continue;

                }

                if (RandomServiceManualWEBResponseList.Count != 0)
                {
                    var rand = new Random();
                    var taken = RandomServiceManualWEBResponseList.ElementAt(rand.Next(RandomServiceManualWEBResponseList.Count));
                    RandomServiceManualWEBResponseList.Remove(taken);
                    string data = string.Empty;
                    if (taken.Category != null) data += $"<i>{taken.Category}</i>";

                    data += $@"{ModifyTitle(taken.Title, search)}";
                    data += $@"<a href='{taken.URL}'> >>> </a> ";



                    ServiceManualWEBCollapsedResponse.Add(data);
                    continue;

                }

            }

            foreach (var taken in ServiceManualWEBResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>{taken.Category}</i>";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                ServiceManualWEBFullResponse.Add(data);
            }

            foreach (var taken in RandomServiceManualWEBResponseList)
            {

                string data = string.Empty;
                if (taken.Category != null) data += $"<i>{taken.Category}</i>";

                data += $@"{ModifyTitle(taken.Title, search)}";
                data += $@"<a href='{taken.URL}'> >>> </a> ";



                ServiceManualWEBFullResponse.Add(data);
            }
            RandomServiceManualWEBResponseList.Clear();
            ServiceManualWEBResponseList.Clear();

        }


        /// <summary>
        /// Make a collapsed response using data-typed lists and save it in DB
        /// </summary>
        /// <param name="request"></param>
        /// <param name="DiagramWebCollapsedResponse"></param>
        /// <param name="PartListPDFCollapsedResponse"></param>
        /// <param name="TechSheetPDFCollapsedResponse"></param>
        /// <param name="ServiceManualPDFCollapsedResponse"></param>
        /// <param name="WiringDiagramPDFCollapsedResponse"></param>
        /// <param name="ServicePointerPDFCollapsedResponse"></param>
        /// <param name="ServiceManualWEBCollapsedResponse"></param>
        /// <returns></returns>
        private static string MakeCollapsedResponse(
            DatabaseTotalResults request,
            List<string> DiagramWebCollapsedResponse,
            List<string> PartListPDFCollapsedResponse,
            List<string> TechSheetPDFCollapsedResponse,
            List<string> ServiceManualPDFCollapsedResponse,
            List<string> WiringDiagramPDFCollapsedResponse,
            List<string> ServicePointerPDFCollapsedResponse,
            List<string> ServiceManualWEBCollapsedResponse)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            string total_text = null;


            if (DiagramWebCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>🌐Diagram WEB: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, DiagramWebCollapsedResponse) + Environment.NewLine + Environment.NewLine;
            if (PartListPDFCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>📙Partlist PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, PartListPDFCollapsedResponse) + Environment.NewLine + Environment.NewLine;
            if (TechSheetPDFCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>📙Tech Sheet PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, TechSheetPDFCollapsedResponse) + Environment.NewLine + Environment.NewLine;
            if (ServiceManualPDFCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>📙Service Manual PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, ServiceManualPDFCollapsedResponse) + Environment.NewLine + Environment.NewLine;
            if (ServiceManualWEBCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>🌐Service Manual WEB: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, ServiceManualWEBCollapsedResponse) + Environment.NewLine + Environment.NewLine;
            if (WiringDiagramPDFCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>📙Wiring Sheet PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, WiringDiagramPDFCollapsedResponse) + Environment.NewLine + Environment.NewLine;
            if (ServicePointerPDFCollapsedResponse.Count > 0)
                total_text += @"<i><u><b>📙Service pointer PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, ServicePointerPDFCollapsedResponse) + Environment.NewLine + Environment.NewLine;




            using var con = new MySqlConnection(cs);
            con.Open();
            var text_for_sql = TextConvert.ToBase64String(total_text);
            try
            {
                con.QueryFirstOrDefault($"UPDATE totalresults SET `reducedResult`='{text_for_sql}' WHERE `ID`={request.ID};");
            }
            catch
            {

            }
            con.Close();
            return total_text;
        }


        /// <summary>
        /// Make a full response using data-typed lists and save it in DB
        /// </summary>
        /// <param name="request"></param>
        /// <param name="DiagramWebFullResponse"></param>
        /// <param name="PartListPDFFullResponse"></param>
        /// <param name="TechSheetPDFFullResponse"></param>
        /// <param name="ServiceManualPDFFullResponse"></param>
        /// <param name="WiringDiagramPDFFullResponse"></param>
        /// <param name="ServicePointerPDFFullResponse"></param>
        /// <param name="ServiceManualWEBFullResponse"></param>
        /// <returns></returns>
        private static bool MakeFullResponse(
            DatabaseTotalResults request,
            List<string> DiagramWebFullResponse,
            List<string> PartListPDFFullResponse,
            List<string> TechSheetPDFFullResponse,
            List<string> ServiceManualPDFFullResponse,
            List<string> WiringDiagramPDFFullResponse,
            List<string> ServicePointerPDFFullResponse,
            List<string> ServiceManualWEBFullResponse,
            string now_search_view)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            string total_text = null;
            if (now_search_view.Equals("⏳")) total_text = "⌛️Searching..";
            if (now_search_view.Equals("⌛️")) total_text = "⏳Searching..";

            if (DiagramWebFullResponse.Count > 0)
                total_text += @"<i><u><b>🌐Diagram WEB: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, DiagramWebFullResponse) + Environment.NewLine + Environment.NewLine;
            if (PartListPDFFullResponse.Count > 0)
                total_text += @"<i><u><b>📙Partlist PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, PartListPDFFullResponse) + Environment.NewLine + Environment.NewLine;
            if (TechSheetPDFFullResponse.Count > 0)
                total_text += @"<i><u><b>📙Tech Sheet PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, TechSheetPDFFullResponse) + Environment.NewLine + Environment.NewLine;
            if (ServiceManualPDFFullResponse.Count > 0)
                total_text += @"<i><u><b>📙Service Manual PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, ServiceManualPDFFullResponse) + Environment.NewLine + Environment.NewLine;
            if (ServiceManualWEBFullResponse.Count > 0)
                total_text += @"<i><u><b>🌐Service Manual WEB: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, ServiceManualWEBFullResponse) + Environment.NewLine + Environment.NewLine;
            if (WiringDiagramPDFFullResponse.Count > 0)
                total_text += @"<i><u><b>📙Wiring Sheet PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, WiringDiagramPDFFullResponse) + Environment.NewLine + Environment.NewLine;
            if (ServicePointerPDFFullResponse.Count > 0)
                total_text += @"<i><u><b>📙Service pointer PDF: </b></u></i>" + Environment.NewLine + string.Join(Environment.NewLine, ServicePointerPDFFullResponse) + Environment.NewLine + Environment.NewLine;




            try
            {
                using var con = new MySqlConnection(cs);
                con.Open();
                var text_for_sql = TextConvert.ToBase64String(total_text);
                if (total_text == null) return false;
                con.QueryFirstOrDefault($"UPDATE totalresults SET `fullResult`='{text_for_sql}' WHERE `ID`={request.ID};");
                con.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
