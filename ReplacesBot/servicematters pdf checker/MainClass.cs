using ReplacesBot.Settings;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using ReplacesBot;
using Telegram.Bot.Types.Enums;
using ReplacesBot.AnalogsUpdater;









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
//Task t = Task.Run(() => AnalogsUpdater.GetReplaces(cts.Token));

Console.ReadLine();
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();




async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    var message = update.Message;
    var messageText = message.Text;
    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");


    if (update != null && update.Message != null
        && update.Message.Text != null
        && update.Type.ToString().Equals("Message")
        && update.Message.Type.ToString().Equals("Text")
        && update.Message.Text != @"/start")

    {



        Message sentMessageSearching = await botClient.SendTextMessageAsync(
chatId: chatId,
text: $@"Searching..",
replyToMessageId: message.MessageId,
cancellationToken: cancellationToken);
        Task t = Task.Run(() => Replaces.GetReplaces(botClient, cancellationToken, update));



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

