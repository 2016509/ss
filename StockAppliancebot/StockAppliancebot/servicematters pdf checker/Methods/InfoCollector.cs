using Dapper;
using MySql.Data.MySqlClient;
using StockAppliance.BotFunctions;
using StockAppliance.DatabaseClasses;
using StockAppliance.ResponseClasses;
using StockAppliance.Settings;
using StockAppliance.SiteMethods;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockAppliance.Methods
{
    public class InfoCollector
    {
        public static async void MainTracker(ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            Console.WriteLine("MainTracker has been started!");
            while (true)
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);
                con.Open();
                int count = await con.QueryFirstAsync<int>($"SELECT COUNT(*) FROM totalresults WHERE `workStarted`=0");
                if (count > 0)
                {
                    var requests = await con.QueryAsync<DatabaseTotalResults>($"SELECT * FROM totalresults WHERE `workStarted`=0");

                    foreach (var response in requests)
                    {
                        Task sender = Task.Run(() => MainSender(botClient, cancellationToken, response)); //Запускаем таск, отвечающий за отправку результатов
                        con.QueryFirstOrDefault($"UPDATE totalresults SET `workStarted`='1' WHERE `MessageID`={response.MessageID} AND `ChatID`='{response.ChatID}';");

                    }
                }
                con.Close();
                Thread.Sleep(100);


            }
        }
        public static async void MainSender(ITelegramBotClient botClient, CancellationToken cancellationToken, DatabaseTotalResults request)
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

            List<Task> allTasks = new();

            //Запуск потока для SearsPartsDirect
            allTasks.Add(Task.Run(() => SearsPartsDirect.Parsing(request, DiagramWebResponseList, PhotosFromSitesList)));
            //Запуск потока для LGParts
            allTasks.Add(Task.Run(() => LGParts.Parsing(request, DiagramWebResponseList, ServiceManualPDFResponseList, PhotosFromSitesList)));
            //Запуск потока для CoastParts
            allTasks.Add(Task.Run(() => CoastParts.Parsing(request, DiagramWebResponseList, PartListPDFResponseList)));
            //Запуск потока для DacorParts
            allTasks.Add(Task.Run(() => DacorParts.Parsing(request, PartListPDFResponseList)));
            //Запуск потока для EasyApplianceParts
            allTasks.Add(Task.Run(() => EasyApplianceParts.Parsing(request, DiagramWebResponseList)));
            //Запуск потока для FixCom
            allTasks.Add(Task.Run(() => FixCom.Parsing(request, DiagramWebResponseList)));
            //Запуск потока для ReliableParts
            allTasks.Add(Task.Run(() => ReliableParts.Parsing(request, DiagramWebResponseList)));
            //Запуск потока для EnCompass
            allTasks.Add(Task.Run(() => EnCompass.Parsing(request, DiagramWebResponseList, PartListPDFResponseList, ServiceManualPDFResponseList)));
            //Запуск потока для SaveMoreOnParts
            allTasks.Add(Task.Run(() => SaveMoreOnParts.Parsing(request, DiagramWebResponseList)));

            //Запуск потока для WhirlPoolDigitalAssets(1)
            allTasks.Add(Task.Run(() => WhirlPoolDigitalAssets.ParsingPartListPDF(request, PartListPDFResponseList)));
            //Запуск потока для WhirlPoolDigitalAssets(2)
            allTasks.Add(Task.Run(() => WhirlPoolDigitalAssets.ParsingTechSheetPDF(request, TechSheetPDFResponseList)));
            //Запуск потока для WhirlPoolDigitalAssets(3)
            allTasks.Add(Task.Run(() => WhirlPoolDigitalAssets.ParsingServiceManualPDF(request, ServiceManualPDFResponseList)));
            //Запуск потока для WhirlPoolDigitalAssets(4)
            allTasks.Add(Task.Run(() => WhirlPoolDigitalAssets.ParsingWiringDiagramPDF(request, WiringDiagramPDFResponseList)));
            //Запуск потока для WhirlPoolDigitalAssets(5)
            allTasks.Add(Task.Run(() => WhirlPoolDigitalAssets.ParsingServicePointerPDF(request, ServicePointerPDFResponseList)));

            //Запуск потока для ServLib
            allTasks.Add(Task.Run(() => ServLib.Parsing(request, ServiceManualPDFResponseList, ServiceManualWEBResponseList)));

            //Запуск потока для ServiceMatters
            allTasks.Add(Task.Run(() => ServiceMatters.Parsing(request, PartListPDFResponseList)));

            string photo_base_64 = @"/9j/4AAQSkZJRgABAQEBLAEsAAD/4QBWRXhpZgAATU0AKgAAAAgABAEaAAUAAAABAAAAPgEbAAUAAAABAAAARgEoAAMAAAABAAIAAAITAAMAAAABAAEAAAAAAAAAAAEsAAAAAQAAASwAAAAB/+0ALFBob3Rvc2hvcCAzLjAAOEJJTQQEAAAAAAAPHAFaAAMbJUccAQAAAgAEAP/hDIFodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvADw/eHBhY2tldCBiZWdpbj0n77u/JyBpZD0nVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkJz8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0nYWRvYmU6bnM6bWV0YS8nIHg6eG1wdGs9J0ltYWdlOjpFeGlmVG9vbCAxMS44OCc+CjxyZGY6UkRGIHhtbG5zOnJkZj0naHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyc+CgogPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9JycKICB4bWxuczp0aWZmPSdodHRwOi8vbnMuYWRvYmUuY29tL3RpZmYvMS4wLyc+CiAgPHRpZmY6UmVzb2x1dGlvblVuaXQ+MjwvdGlmZjpSZXNvbHV0aW9uVW5pdD4KICA8dGlmZjpYUmVzb2x1dGlvbj4zMDAvMTwvdGlmZjpYUmVzb2x1dGlvbj4KICA8dGlmZjpZUmVzb2x1dGlvbj4zMDAvMTwvdGlmZjpZUmVzb2x1dGlvbj4KIDwvcmRmOkRlc2NyaXB0aW9uPgoKIDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PScnCiAgeG1sbnM6eG1wTU09J2h0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8nPgogIDx4bXBNTTpEb2N1bWVudElEPmFkb2JlOmRvY2lkOnN0b2NrOmRiMTUyNTRhLWMxZGUtNGEyNy1hODg1LTg4Y2E2MmJjMjA4YzwveG1wTU06RG9jdW1lbnRJRD4KICA8eG1wTU06SW5zdGFuY2VJRD54bXAuaWlkOjM3MzAyYzI0LWVjYjUtNGVmNi04MGYyLTIzN2MyZGZhNDVmYjwveG1wTU06SW5zdGFuY2VJRD4KIDwvcmRmOkRlc2NyaXB0aW9uPgo8L3JkZjpSREY+CjwveDp4bXBtZXRhPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAo8P3hwYWNrZXQgZW5kPSd3Jz8+/9sAQwAFAwQEBAMFBAQEBQUFBgcMCAcHBwcPCwsJDBEPEhIRDxERExYcFxMUGhURERghGBodHR8fHxMXIiQiHiQcHh8e/9sAQwEFBQUHBgcOCAgOHhQRFB4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4e/8AAEQgBaAHgAwERAAIRAQMRAf/EABwAAQACAwEBAQAAAAAAAAAAAAAHCAEFBgMCBP/EAFkQAAEDAwEDBgYNCQQFCwUAAAEAAgMEBQYRByExEiJBUXGBCBMUVmGRFhc3QlJydKGxs8HR0iMyMzQ2c5OUshUkYsI1gpKi8CU4Q1NUVWNldaPhREiGw8T/xAAUAQEAAAAAAAAAAAAAAAAAAAAA/8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAwDAQACEQMRAD8AtugwgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgyEBBhAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBBkICDCAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICDIQEGEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEGQgIMICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIMhAQYQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQZCAgwgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgyEBBhAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBBkICDCAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICDIQEGEBAQEBAQEBAQEBAQEBAQDroeSOUega6anqQVabtNzm35XLX1dyqHPjnc2a3zboAASDHyPe6cNRvHFBYjB8qteW2RlztkhGmjZ4HHnwP+C77DwIQb1AQEBAQEH47rdbZaohLdLjSULDwNRM1mvZqd6DTe2BhHnXaP46B7YGEeddp/joHtgYR512n+Oge2BhHnXaf46B7YGEeddp/joHtgYR512n+Oge2BhHnXaf46B7YGEeddp/joP32nJ8cu0nirZfbbVyHgyKpaXHu11Qbfp0QYQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEGQgIMICAgICAgICAgICAgICAgI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";
            var bytes_photo_no_photo = Convert.FromBase64String(photo_base_64);
            using (var stream = new MemoryStream(bytes_photo_no_photo))
            {
                //Отправляем сообщение с фотографией
                var photo_sended = await botClient.SendPhotoAsync(
                    chatId: request.ChatID,
                    photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream),
                    replyToMessageId: (int)request.MessageID,
                    cancellationToken: cancellationToken);

                con.Open();
                con.QueryFirstOrDefault<DatabaseUserData>($"UPDATE totalresults SET `botPhotoMessageID`='{photo_sended.MessageId}' WHERE `ID`='{request.ID}';");
                con.Close();
                request.BotPhotoMessageID = photo_sended.MessageId;
            }

            Task ttt = Task.Run(() => AutoUpdatedMessage.MainUpdater(botClient, cancellationToken, request, allTasks));
            
            

            Task.WaitAll(allTasks.ToArray());
            //Console.ReadLine();

            //await botClient.DeleteMessageAsync(chatId: request.ChatID, messageId: (int)request.BotMessageID, cancellationToken: cancellationToken);


            //Поиск строк, которые должны выдаватсья рандомно
            var RandomDiagramWebResponseList = DiagramWebResponseList.FindAll(FindRandomResult);
            var RandomServiceManualPDFResponseList = ServiceManualPDFResponseList.FindAll(FindRandomResult);
            var RandomPartListPDFResponseList = PartListPDFResponseList.FindAll(FindRandomResult);
            var RandomServicePointerPDFResponseList = ServicePointerPDFResponseList.FindAll(FindRandomResult);
            var RandomServiceManualWEBResponseList = ServiceManualWEBResponseList.FindAll(FindRandomResult);
            var RandomTechSheetPDFResponseList = TechSheetPDFResponseList.FindAll(FindRandomResult);
            var RandomWiringDiagramPDFResponseList = WiringDiagramPDFResponseList.FindAll(FindRandomResult);


            //Удаление строк из первоначального списка
            DiagramWebResponseList.RemoveAll(FindRandomResult);
            ServiceManualPDFResponseList.RemoveAll(FindRandomResult);
            PartListPDFResponseList.RemoveAll(FindRandomResult);
            ServicePointerPDFResponseList.RemoveAll(FindRandomResult);
            ServiceManualWEBResponseList.RemoveAll(FindRandomResult);
            TechSheetPDFResponseList.RemoveAll(FindRandomResult);
            WiringDiagramPDFResponseList.RemoveAll(FindRandomResult);


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
            

            Stream total_photo_stream = null;

            if (PhotosFromSitesList.Count > 0)
            {
                try
                {
                    PhotosFromSitesList = PhotosFromSitesList.OrderBy(x => x.Priority).ToList();
                    total_photo_stream = CustomHttpClass.GetToStream(PhotosFromSitesList.First().PhotoURL, use_google_ua: false);
                }
                catch
                { 
                    total_photo_stream = null; 
                }
            }


            string text = MakeCollapsedResponse(
                request,
                DiagramWebCollapsedResponse,
                PartListPDFCollapsedResponse,
                TechSheetPDFCollapsedResponse,
                ServiceManualPDFCollapsedResponse,
                WiringDiagramPDFCollapsedResponse, ServicePointerPDFCollapsedResponse,
                ServiceManualWEBCollapsedResponse
                );
            
            
            if (MakeFullResponse(
                request,
                DiagramWebFullResponse,
                PartListPDFFullResponse,
                TechSheetPDFFullResponse,
                ServiceManualPDFFullResponse,
                WiringDiagramPDFFullResponse,
                ServicePointerPDFFullResponse,
                ServiceManualWEBFullResponse
                ))
            {
                if (total_photo_stream != null)
                {
                    try
                    {

                       


                        await botClient.EditMessageMediaAsync(
                            chatId: request.ChatID,
                            messageId: (int)request.BotPhotoMessageID,
                            media: new InputMediaPhoto(new InputMedia(total_photo_stream, "test.png")) { },
                            
                            cancellationToken: cancellationToken);

                        /*await botClient.SendPhotoAsync(
                    chatId: request.ChatID,
                    photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(total_photo_stream),
                    cancellationToken: cancellationToken);
                        total_photo_stream.Close();*/
                    }
                    catch(Exception ex) { Console.WriteLine($"Photo error - {PhotosFromSitesList.First().PhotoURL} - {ex.Message}"); }


                   
                }
                if (text == null)
                {
                    await botClient.EditMessageTextAsync(
                   chatId: request.ChatID,
                   text: "No results.",
                   messageId: (int)request.BotMessageID,
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                   disableWebPagePreview: true,
                   cancellationToken: cancellationToken
                   );

                   /* await botClient.SendTextMessageAsync(
                   chatId: request.ChatID,
                   text: "No results.",
                   replyToMessageId: int.Parse(request.MessageID.ToString()),
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                   disableWebPagePreview: true,
                   cancellationToken: cancellationToken
                   );*/
                    return;
                }

                /*var sended_message = await botClient.SendTextMessageAsync(
               chatId: request.ChatID,
               text: text,
               replyToMessageId: int.Parse(request.MessageID.ToString()),
               replyMarkup: Buttons.ShowMoreIKM(),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               disableWebPagePreview: true,
               cancellationToken: cancellationToken
               );*/

                var sended_message = await botClient.EditMessageTextAsync(
               chatId: request.ChatID,
               text: text + Environment.NewLine + Environment.NewLine + $"✅ Search completed",
               messageId: (int)request.BotMessageID,
               replyMarkup: Buttons.ShowMoreIKM(),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               disableWebPagePreview: true,
               cancellationToken: cancellationToken
                   );


                con.Open();
                con.QueryFirstOrDefault($"UPDATE totalresults SET `botMessageID`='{sended_message.MessageId}', `ResponseSent`='{DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss")}' WHERE `ID`='{request.ID}'");
                con.Close();
            }

            else
            {
                if (total_photo_stream != null)
                {
                    try
                    {
                        await botClient.SendPhotoAsync(
                    chatId: request.ChatID,
                    photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(total_photo_stream),
                    cancellationToken: cancellationToken);
                        total_photo_stream.Close();
                    }
                    catch (Exception ex) { Console.WriteLine($"Photo error - {PhotosFromSitesList.First().PhotoURL} - {ex.Message}"); }



                }


                if (text == null) {
                    await botClient.SendTextMessageAsync(
                   chatId: request.ChatID,
                   text: "No results.",
                   replyToMessageId: int.Parse(request.MessageID.ToString()),
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                   disableWebPagePreview: true,
                   cancellationToken: cancellationToken
                   );
                    return;
                }
                var sended_message = await botClient.SendTextMessageAsync(
               chatId: request.ChatID,
               text: text,
               replyToMessageId: int.Parse(request.MessageID.ToString()),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               disableWebPagePreview: true,
               cancellationToken: cancellationToken
               );
               


                con.Open();
                con.QueryFirstOrDefault($"UPDATE totalresults SET `botMessageID`='{sended_message.MessageId}', `ResponseSent`='{DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss")}' WHERE `ID`='{request.ID}'");
                con.Close();
            }


           



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

                string data = @$"<b><u>{taken.Source}</u></b>->";
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
            List<string> ServiceManualWEBFullResponse)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            string total_text = null;

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
