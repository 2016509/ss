using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using servicematters_pdf_checker.AnalogsUpdater;
using servicematters_pdf_checker.DatabaseClasses;
using servicematters_pdf_checker.Methods;
using servicematters_pdf_checker.Settings;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;





//var a = Table.ReadAllTable("A", "Z");
//AnalogsUpdater.ApWagnerComNames(@"wp9759243", new List<string>());


var botClient = new TelegramBotClient(AppSettings.Current.Telegram.Token);

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
//AnalogsUpdater.MainChecker(cts.Token);
Task t = Task.Run(() => AnalogsUpdater.MainChecker(cts.Token));


Console.ReadLine();
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();




async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{

    var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
    await using var con = new MySqlConnection(cs);

    if (update != null && update.Message != null
        && update.Message.Text != null
        && update.Type.ToString().Equals("Message")
        && update.Message.Type.ToString().Equals("Text")
        && update.Message.Text != @"/start")

    {

        var message = update.Message;
        var messageText = message.Text;
        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");



        Message sentMessageSearching = await botClient.SendTextMessageAsync(
chatId: chatId,
text: $@"Searching..",
replyToMessageId: message.MessageId,
cancellationToken: cancellationToken);

        try
        {
            dynamic response = JsonConvert.DeserializeObject(CustomHttpClass.GetToString("https://servicematters.com/en_US/api/guest-search/v2?category=Parts+List&query=" + messageText, acceptencoding: "none"));


            if (response.results.Count > 0)
            {
                Stream stream_pdf = CustomHttpClass.GetToStream(response.results[0].url.ToString(), use_google_ua: false);
                string pdf_data_string = PDFData.pdfText(stream_pdf);
                pdf_data_string = pdf_data_string.Replace("\n", " ").Replace(".", " ").Replace(",", " ");
                var pdf_data = pdf_data_string.Split(' ').ToList().Distinct().ToList();
                List<servicematters_pdf_checker.ResponseClass> Responses = new();


                Google.Apis.Sheets.v4.Data.ValueRange tb_res = new();

                while (true)
                {
                    try
                    {
                        tb_res = Table.ReadAllTable("A", "H");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Taken error Google Sheets: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
                bool founded = false;


                con.Open();
                var allAnalogsNow = con.Query<ReplacesTable>($"SELECT `Id`,`inputRequest`,`Name`,`replacesData`,`status`,`totalReplacesCount` FROM `replaces_table`;");
                con.Close();



                foreach (var row in tb_res.Values)
                {

                    string search;
                    try
                    {
                        search = row[0].ToString();
                        if ((pdf_data.Contains(search.ToLower()) || pdf_data.Contains(search.ToUpper())) && !string.IsNullOrEmpty(search))
                        {

                            List<string> values = new();
                            foreach (var row_data in row)
                            {
                                if (row.IndexOf(row_data) == 6) continue;
                                values.Add(row_data.ToString());
                            }
                            Responses.Add(new() { Data = $@"{string.Join(';', values)}", RowNumber = tb_res.Values.IndexOf(row) });

                            /*Message sentMessage = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $@"{string.Join(';', row)}",
                                replyToMessageId: message.MessageId,
                                cancellationToken: cancellationToken);*/
                            founded = true;
                            continue;

                        }
                    }
                    catch { }
                    try
                    {
                        if (row.Count < 3) continue;
                        var searchAnalogsData =
                            allAnalogsNow.FirstOrDefault(x => x.InputRequest != null && x.InputRequest.Trim() == row[0].ToString()?.Trim());
                        if (searchAnalogsData == null) continue;
                        if (searchAnalogsData.TotalReplacesCount == 0) continue;
                        else
                        {
                            List<string> searches =
                                JsonConvert.DeserializeObject<List<string>>(searchAnalogsData.ReplacesData);
                            foreach (var search_analog in searches)
                            {
                                if ((pdf_data.Contains(search_analog.ToLower()) || pdf_data.Contains(search_analog.ToUpper())) && !string.IsNullOrEmpty(search_analog)&&search_analog.Length > 4)
                                {
                                    List<string> values = new();
                                    foreach (var row_data in row)
                                    {
                                        if (row.IndexOf(row_data) == 6 || row.IndexOf(row_data) == 0) continue;
                                        values.Add(row_data.ToString());
                                    }

                                    Responses.Add(new() { Data = $@"{row[0].ToString()}(Rpl {search_analog});{string.Join(';', values)}", RowNumber = tb_res.Values.IndexOf(row) });


                                    founded = true;
                                    continue;

                                }
                            }
                        }

                    }
                    catch { }




                }
                Responses = Responses.DistinctBy(x => x.RowNumber).ToList();
                List<string> results = new List<string>();

                foreach (var taken_response in Responses)
                {
                    if (!string.IsNullOrEmpty(taken_response.Data))
                        results.Add(taken_response.Data);
                }
                stream_pdf.Close();
                if (!founded)
                {
                    Message sentMessage = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Sorry, nothing found",
                                replyToMessageId: message.MessageId,
                                cancellationToken: cancellationToken);
                }
                else
                {
                    List<string> new_results = new();
                    foreach (var result in results)
                    {
                        string ready_send = string.Empty;

                        var spl = result.Split(';');
                        if (spl.Count() > 0) ready_send += $@"<b>#: </b>{spl[0].Trim()}";
                        if (spl.Count() > 5 && !string.IsNullOrEmpty(spl[5].Trim())) ready_send += Environment.NewLine + $@"<b>Name: </b>{spl[5].Trim()}";
                        if (spl.Count() > 2 && !string.IsNullOrEmpty(spl[2].Trim())) ready_send += Environment.NewLine + $@"<b>Qty: </b>{spl[2].Trim()}";
                        if (spl.Count() > 1 && !string.IsNullOrEmpty(spl[1].Trim())) ready_send += $@" // <b>Cond: </b>{spl[1].Trim()}";
                        if (spl.Count() > 4 && !string.IsNullOrEmpty(spl[4].Trim())) ready_send += $@" // <b>Wh#: </b>{spl[4].Trim()}";
                        if (spl.Count() > 3 && !string.IsNullOrEmpty(spl[3].Trim())) ready_send += Environment.NewLine + $@"<b>Cmt: </b>{spl[3].Trim()}";
                        //if (spl.Count() > 6 && !string.IsNullOrEmpty(spl[7].Trim())) ready_send += Environment.NewLine + $@"<b>Wh#: </b>{spl[6].Trim()}";
                        new_results.Add(ready_send);


                    }

                    //new_results = new_results.OrderBy(x => x.Split(';')[3].Trim()).ToList();

                    string total_message_text = string.Join(Environment.NewLine + Environment.NewLine, new_results);
                    if (total_message_text.Length < 3906)
                    {
                        Message sentMessage0 = await botClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: $@"{string.Join(Environment.NewLine + Environment.NewLine, new_results)}" + Environment.NewLine + Environment.NewLine + $@"Search Comlpleted",
                                            replyToMessageId: message.MessageId,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        string new_total_text = "";
                        int _ind = 0;
                        while ((new_total_text + Environment.NewLine + Environment.NewLine + $@"Search Comlpleted").Length < 3906)
                        {
                            new_total_text += new_results[_ind] + Environment.NewLine + Environment.NewLine;

                            _ind++;

                        }

                        Message sentMessage0 = await botClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: $@"{new_total_text}" + Environment.NewLine + Environment.NewLine + $@"Search Comlpleted",
                                            replyToMessageId: message.MessageId,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);

                        using (Stream sr = new MemoryStream())
                        {
                            var writer = new StreamWriter(sr, encoding:System.Text.Encoding.UTF8);
                            string total_message_html = string.Join("<br/><br/>", new_results).Replace(Environment.NewLine, "<br/>");
                            writer.Write(total_message_html);
                            writer.Flush();
                            sr.Position = 0;
                
                            Message send_file_message = await botClient.SendDocumentAsync(
                                chatId: chatId,
                                new Telegram.Bot.Types.InputFiles.InputOnlineFile(sr, $@"{messageText}.html"),
                                caption: @"The answer to this query was too large, so we had to cut it off. The full version can be found here:",
                                replyToMessageId: message.MessageId,
                                parseMode: ParseMode.Html,
                                cancellationToken: cancellationToken
                                );
                        }
                    }

                }
            }

            else
            {
                // Echo received message text
                Message sentMessage = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"The model number {update.Message.Text} was not found. Please check the model number.",
                            replyToMessageId: message.MessageId,
                            cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Got a error.",
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
        }
    }
    else
    {
        
    }




}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

