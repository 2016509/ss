using StockAppliance.BotFunctions;
using StockAppliance.Methods;
using StockAppliance.Settings;
using StockAppliance.SiteMethods;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using FlareSolverrSharp;
using Telegram.Bot.Types.Enums;
using System.Text;
using StockAppliance.ResponseClasses;


//ReliableParts.Parsing(new StockAppliance.DatabaseClasses.DatabaseTotalResults() { Request = "WM4370HKA" }, new List<DiagramWebResponse>());
//LGParts.Parsing(@"LSE4616ST", new List<StockAppliance.ResponseClasses.DiagramWebResponse>(), new List<StockAppliance.ResponseClasses.ServiceManualPDFResponse>(), new List<StockAppliance.ResponseClasses.PhotosFromSites>());
//EnCompass.Parsing(@"LDF7774ST", new List<StockAppliance.ResponseClasses.DiagramWebResponse>(), new List<StockAppliance.ResponseClasses.PartListPDFResponse>(), new List<StockAppliance.ResponseClasses.ServiceManualPDFResponse>());

//CoastParts.Parsing(@"CSX27H", new List<StockAppliance.ResponseClasses.DiagramWebResponse>(), new List<StockAppliance.ResponseClasses.PartListPDFResponse>());
//WhirlPoolDigitalAssets.ParsingTechSheetPDF(@"KDTM404KPS", new List<StockAppliance.ResponseClasses.TechSheetPDFResponse>());

//ShortUrl.MakeShortURL(@"https://research.encompass.com/ZEN/sm/LDF7774ST.pdf");
//EasyApplianceParts.Parsing(@"DRR30980RAP", new List<StockAppliance.ResponseClasses.DiagramWebResponse>());

//SearsPartsDirect.Parsing(new StockAppliance.DatabaseClasses.DatabaseTotalResults() { Request = "LSE4616ST" }, new List<StockAppliance.ResponseClasses.DiagramWebResponse>(), new List<StockAppliance.ResponseClasses.PhotosFromSites>());
//ServLib.Parsing(new StockAppliance.DatabaseClasses.DatabaseTotalResults() { Request = "DV42H5000" }, new List<StockAppliance.ResponseClasses.ServiceManualPDFResponse>(), new List<StockAppliance.ResponseClasses.ServiceManualWEBResponse>());
//FixCom.Parsing(new StockAppliance.DatabaseClasses.DatabaseTotalResults() { Request = "DV42H5000" }, new List<StockAppliance.ResponseClasses.DiagramWebResponse>());
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

Console.WriteLine($"Start listening for @{me.Username} thread started!");
Task t = Task.Run(() => InfoCollector.MainTracker(botClient, cts.Token));
Console.ReadLine();
/*
// Send cancellation request to stop bot
cts.Cancel();*/
while (true) { }

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{

    if (update.Type.ToString().Equals("Message"))
    {
        var message = update.Message;
        var messageText = message.Text;
        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a message '{messageText}' in chat {chatId}.");
        await Task.Run(() => MessageHandler.TextMessageHandler(botClient, update, cancellationToken)); //Запуск обработчика текстового сообщения
    }

    if (update.Type.ToString().Equals("CallbackQuery"))
    {

        var chatId = update.CallbackQuery.From.Id;
        var query = update.CallbackQuery.Data;
        Console.WriteLine($"Received a callbackQuery '{query}' in chat {chatId}.");
        await Task.Run(() => MessageHandler.CallBackQueryHandler(botClient, update, cancellationToken)); //Запуск обработчика запросов кнопок
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
    Environment.Exit(Environment.ExitCode);
    return Task.CompletedTask;
}

