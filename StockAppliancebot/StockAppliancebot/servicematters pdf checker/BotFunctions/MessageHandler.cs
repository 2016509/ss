using Dapper;
using MySql.Data.MySqlClient;
using StockAppliance.DatabaseClasses;
using StockAppliance.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockAppliance.BotFunctions
{
    /// <summary>
    /// Bot commands Handler
    /// </summary>
    public sealed class MessageHandler
    {
        public static async void TextMessageHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var message = update.Message;

            if (message.Type.ToString().Equals("Text"))
            {
                // /start
                if (message.Text.ToString().Equals("/start"))
                {
                    StartCommand(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("👤 Мои данные"))
                {
                    MyData(botClient, update, cancellationToken); return;
                }
                if (message.Text.ToString().Equals("📋 Настройки таблиц"))
                {
                    SpreadsheetsSettings(botClient, update, cancellationToken); return;
                }
                if (message.Text.ToString().Equals("📤 Настройки выдачи"))
                {
                    OutputSettings(botClient, update, cancellationToken); return;
                }
                if (message.Text.ToString().Equals("🆘 Помощь"))
                {
                    HelpMe(botClient, update, cancellationToken); return;
                }

                if (message.ReplyToMessage != null)
                {
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите ссылку на Вашу гугл-таблицу") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        UpdateGoogleSheetID(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите название листа") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        UpdateGoogleSpreadSheetSheetName(botClient, update, cancellationToken); return;
                    }



                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Diagram WEB в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountDiagramWeb(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Partlist PDF в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountPartlistPDF(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Tech Sheet PDF в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountTechSheetPDF(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Service Manual PDF в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountServiceManualPDF(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Service Manual WEB в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountServiceManualWEB(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Wiring Sheet PDF в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountWiringSheetPDF(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите желаемое количество строк Service Pointer PDF в сокращенной выдаче") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditRowsCount.UpdateCountServicePointerPDF(botClient, update, cancellationToken); return;
                    }






                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите паузу перед первым сообщением") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        UpdateFirstTimeout(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите паузу перед вторым сообщением") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        UpdateSecondTimeout(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Укажите паузу перед финальным сообщением") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        UpdateTotalTimeout(botClient, update, cancellationToken); return;
                    }
                }



                //В случае, если варианты выше не подошли - совершаем добавление в БД задачи.

                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);
                con.Open();
                DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"INSERT INTO totalresults (`MessageID`, `request`, `ChatID`, `RequestStart`) VALUES ('{update.Message.MessageId}', '{update.Message.Text}', '{update.Message.Chat.Id}', '{DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss")}');");
                con.Close();
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $"Задача принята в обработку",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken
                    );


            }

        }

        public static void CallBackQueryHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var callback = update.CallbackQuery;
            //CloseSettingsMenu
            if (callback.Data.ToString().Equals("CloseSettingsMenu"))
            {
                CloseMenu(botClient, update, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Equals("ChangeSpreadsheetQuery"))
            {
                SetGoogleSpreadSheetID(botClient, update, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Equals("ShowMoreQuery"))
            {
                ShowMoreAction(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ShowLessQuery"))
            {
                ShowLessAction(botClient, update, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Equals("ChangeSheetName"))
            {
                SetGoogleSpreadSheetListName(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountOfFirstOutputQuery"))
            {
                SetCountOfFirstOutput(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeFirstTimeoutQuery"))
            {
                SetFirstTimeout(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeSecondTimeoutQuery"))
            {
                SetSecondTimeout(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeTotalTimeoutQuery"))
            {
                SetTotalTimeout(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountDiagramWeb"))
            {
                StartEditRows.SetCountDiagramWeb(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountPartlistPDF"))
            {
                StartEditRows.SetCountPartlistPDF(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountTechSheetPDF"))
            {
                StartEditRows.SetCountTechSheetPDF(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountServiceManualPDF"))
            {
                StartEditRows.SetCountServiceManualPDF(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountServiceManualWEB"))
            {
                StartEditRows.SetCountServiceManualWEB(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountWiringSheetPDF"))
            {
                StartEditRows.SetCountWiringSheetPDF(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ChangeCountServicePointerPDF"))
            {
                StartEditRows.SetCountServicePointerPDF(botClient, update, cancellationToken);
                return;
            }





        }




        /// <summary>
        /// Register/update menu command
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void StartCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            int count = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id}");
            con.Close();

            if (count == 0) //Если нет данных о пользователе в БД пользователей
            {
                con.Open();
                await con.QueryAsync($"INSERT INTO userdata (`userId`) VALUES ('{user_id}')");

                await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: user_id,
                text: $@"Вы зарегистрированы.",
                replyMarkup: Buttons.StartRKM(),
                cancellationToken: cancellationToken);
            }

            else
            {
                await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: user_id,
                text: $@"Меню обновлено.",
                replyMarkup: Buttons.StartRKM(),
                cancellationToken: cancellationToken);
            }
        }
        /// <summary>
        /// Register/update menu command
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void CloseMenu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.CallbackQuery.From.Id;



            await botClient.SendTextMessageAsync(
            chatId: user_id,
            text: $@"Меню обновлено",
            replyMarkup: Buttons.StartRKM(),
            cancellationToken: cancellationToken);
            try
            {
                await botClient.DeleteMessageAsync(
                    chatId: user_id,
                    messageId: update.CallbackQuery.Message.MessageId);
            }
            catch { }


        }

        /// <summary>
        /// Response to user with info about his data
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void MyData(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={user_id}");
            con.Close();

            await botClient.SendTextMessageAsync( //Отправляем сообщение с информацией о пользователе
               chatId: user_id,
               text:
               @$"<b>Пользователь</b>
Ваш ID в БД: {userdata.ID}
Ваш Telegram ID: {userdata.UserId}
<b>Количество строк в сокращенной выдаче</b>
🌐 Diagram WEB: {userdata.CountDiagramWEB}
📙Partlist PDF: {userdata.CountPartlistPDF}
📙Tech Sheet PDF: {userdata.CountTechSheetPDF}
📙Service Manual PDF: {userdata.CountServiceManualPDF}
🌐Service Manual WEB: {userdata.CountServiceManualWEB}
📙Wiring Sheet PDF: {userdata.CountWiringSheetPDF}
📙Service pointer PDF: {userdata.CountServicePointerPDF}


<b>Таблицы</b>
Ссылка на таблицу с моделями: https://docs.google.com/spreadsheets/d/{userdata.SpreadsheetID}/edit 
Название листа: {userdata.Sheet}",

               replyMarkup: Buttons.StartRKM(),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);

        }

        /// <summary>
        /// Response to user with info about his Table Settings
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SpreadsheetsSettings(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={user_id}");
            con.Close();

            await botClient.SendTextMessageAsync( //Отправляем сообщение с информацией о пользователе
               chatId: user_id,
               text:
               @$"<b>Таблицы</b>
Ссылка на таблицу с моделями: https://docs.google.com/spreadsheets/d/{userdata.SpreadsheetID}/edit 
Название листа: {userdata.Sheet}",

               replyMarkup: Buttons.TableIKM(),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);
        }
        /// <summary>
        /// Response to user with info about his Output Settings
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void OutputSettings(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={user_id}");
            con.Close();

            await botClient.SendTextMessageAsync( //Отправляем сообщение с информацией о пользователе
               chatId: user_id,
               text:
               @$"<b>Выдача</b>
<b>Количество строк в сокращенной выдаче</b>
🌐 Diagram WEB: {userdata.CountDiagramWEB}
📙Partlist PDF: {userdata.CountPartlistPDF}
📙Tech Sheet PDF: {userdata.CountTechSheetPDF}
📙Service Manual PDF: {userdata.CountServiceManualPDF}
🌐Service Manual WEB: {userdata.CountServiceManualWEB}
📙Wiring Sheet PDF: {userdata.CountWiringSheetPDF}
📙Service pointer PDF: {userdata.CountServicePointerPDF}
",

               replyMarkup: Buttons.OutputIKM(),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user with info about his Output Rows Count Settings (for Callback!)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SetCountOfFirstOutput(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            long user_id = update.CallbackQuery.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={user_id}");
            con.Close();


            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.EditMessageTextAsync(
                chatId: update.CallbackQuery.Message.Chat.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                text: $@"<b>Количество строк в сокращенной выдаче</b>
🌐 Diagram WEB: {userdata.CountDiagramWEB}
📙Partlist PDF: {userdata.CountPartlistPDF}
📙Tech Sheet PDF: {userdata.CountTechSheetPDF}
📙Service Manual PDF: {userdata.CountServiceManualPDF}
🌐Service Manual WEB: {userdata.CountServiceManualWEB}
📙Wiring Sheet PDF: {userdata.CountWiringSheetPDF}
📙Service pointer PDF: {userdata.CountServicePointerPDF}",
                replyMarkup: Buttons.RowsCountOutputIKM(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken
                );
        }

        /// <summary>
        /// Response to user with info about his Output Rows Count Settings (for Message!)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SendCountOfFirstOutput(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            long user_id = update.Message.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={user_id}");
            con.Close();



            await botClient.SendTextMessageAsync(
                chatId: update.Message.From.Id,
                text: $@"<b>Количество строк в сокращенной выдаче</b>
🌐 Diagram WEB: {userdata.CountDiagramWEB}
📙Partlist PDF: {userdata.CountPartlistPDF}
📙Tech Sheet PDF: {userdata.CountTechSheetPDF}
📙Service Manual PDF: {userdata.CountServiceManualPDF}
🌐Service Manual WEB: {userdata.CountServiceManualWEB}
📙Wiring Sheet PDF: {userdata.CountWiringSheetPDF}
📙Service pointer PDF: {userdata.CountServicePointerPDF}",
                replyMarkup: Buttons.RowsCountOutputIKM(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken
                );
        }

        /// <summary>
        /// Response to user with help info
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void HelpMe(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync( //Отправляем сообщение с информацией о пользователе
               chatId: update.Message.Chat.Id,
               text:
               @$"<b>Раздел помощи</b>

Для корректной работы бота необходимо в <ins><b>обязательно</b></ins> порядке установить ссылку и имя листа Google-таблицы. Делается это в разделе <b>📋 Настройки таблиц</b> (кнопка для быстрого доступа находится в меню снизу)

Также, необходимо заглянуть в раздел <b>📤 Настройки выдачи</b>. В нем устанавливается количество строк, которое будет отображаться в первичной выдаче, а также паузы между сообщениями.

Любые вопросы можно задать в ЛС @n0n3mi1y",

               replyMarkup: Buttons.StartRKM(),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Response to user a request for setting new Google SpreadSheet ID
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SetGoogleSpreadSheetID(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: update.CallbackQuery.From.Id,
                text: $@"Укажите ссылку на Вашу гугл-таблицу",
                replyMarkup: Buttons.SpreadsheetFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Google SpreadSheet List Name
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SetGoogleSpreadSheetListName(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                text: $@"Укажите название листа",
                replyMarkup: Buttons.SpreadsheetSheetNameFRM(),
                cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Response to user a request for setting new First Timeout
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SetFirstTimeout(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: update.CallbackQuery.From.Id,
                text: $@"Укажите паузу перед первым сообщением",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }
        /// <summary>
        /// Response to user a request for setting new Second timeout
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SetSecondTimeout(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: update.CallbackQuery.From.Id,
                text: $@"Укажите паузу перед вторым сообщением",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }
        /// <summary>
        /// Response to user a request for setting new Total timeout
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void SetTotalTimeout(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: update.CallbackQuery.From.Id,
                text: $@"Укажите паузу перед финальным сообщением",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Updating a Google SpreadSheet ID in DB
        /// </summary>
        /// <param name="botClient">Telegram Bot client interface</param>
        /// <param name="update">Update object</param>
        /// <param name="cancellationToken"></param>
        private static async void UpdateGoogleSheetID(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            string total_spreadsheet_id = string.Empty;

            Uri uri = new Uri(update.Message.Text);
            total_spreadsheet_id = uri.AbsolutePath.Split('/')[3];
            using var con = new MySqlConnection(cs);
            con.Open();
            con.QueryFirstOrDefault($"UPDATE userdata SET `SpreadsheetID`='{total_spreadsheet_id}' WHERE `userId`={user_id}");
            int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `SpreadsheetID`='{total_spreadsheet_id}'");
            con.Close();

            if (update_result == 1)
                await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                    chatId: update.Message.Chat.Id,
                    text: $@"✅ Ссылка успешно изменена!",
                    cancellationToken: cancellationToken);
            else
                await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: update.Message.Chat.Id,
                text: $@"❌ В процессе обновления ссылки произошла ошибка!",
                cancellationToken: cancellationToken);

            SpreadsheetsSettings(botClient, update, cancellationToken); // Вывод меню изменения таблиц

        }

        /// <summary>
        /// Updating a Google SpreadSheet List Name in Database
        /// </summary>
        /// <param name="botClient">Telegram Bot client interface</param>
        /// <param name="update"> Update object</param>
        /// <param name="cancellationToken"></param>
        private static async void UpdateGoogleSpreadSheetSheetName(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            string total_spreadsheet_sheet_name = update.Message.Text;


            using var con = new MySqlConnection(cs);
            con.Open();
            con.QueryFirstOrDefault($"UPDATE userdata SET `Sheet`='{total_spreadsheet_sheet_name}' WHERE `userId`={user_id}");
            int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `Sheet`='{total_spreadsheet_sheet_name}'");
            con.Close();

            if (update_result == 1)
                await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                    chatId: update.Message.Chat.Id,
                    text: $@"✅ Название листа успешно изменено!",
                    cancellationToken: cancellationToken);
            else
                await botClient.SendTextMessageAsync( //Отправляем сообщение об успешной регистрации
                chatId: update.Message.Chat.Id,
                text: $@"❌ В процессе обновления названия листа произошла ошибка!",
                cancellationToken: cancellationToken);

            SpreadsheetsSettings(botClient, update, cancellationToken); // Вывод меню изменения таблиц

        }






        /// <summary>
        /// Updating a firstTimeout in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void UpdateFirstTimeout(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `firstTimeout`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `firstTimeout`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Пауза перед первым сообщением успешно обновлена!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения паузы перед первым сообщением произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            OutputSettings(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }
        /// <summary>
        /// Updating a secondTimeout in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void UpdateSecondTimeout(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `secondTimeout`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `secondTimeout`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Пауза перед вторым сообщением успешно обновлена!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения паузы перед вторым сообщением произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            OutputSettings(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }
        /// <summary>
        /// Updating a totalTimeout in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void UpdateTotalTimeout(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `totalTimeout`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `totalTimeout`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Пауза перед финальным сообщением успешно обновлена!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения паузы перед финальным сообщением произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            OutputSettings(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        private static async void ShowMoreAction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            long user_id = update.CallbackQuery.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var result = con.QueryFirstOrDefault<DatabaseTotalResults>($"SELECT * FROM totalresults WHERE `ChatID`={user_id} AND `botMessageID`={update.CallbackQuery.Message.MessageId}");
            con.Close();

            if (string.IsNullOrEmpty(result.fullResult)) await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken,
                text: "There are no more results."
                );
            else
            {
                byte[] encodedDataAsBytesFull = Convert.FromBase64String(result.fullResult);
                byte[] encodedDataAsBytesReduced = Convert.FromBase64String(result.ReducedResult);
                string full_text = System.Text.Encoding.UTF8.GetString(encodedDataAsBytesReduced) + System.Text.Encoding.UTF8.GetString(encodedDataAsBytesFull);


                if (full_text.Length <= 4096)
                    await botClient.EditMessageTextAsync(
                    chatId: result.ChatID,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: full_text,
                    replyMarkup: Buttons.ShowLessIKM(),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken
                    );
                else
                {
                    var splited = full_text.Split(Environment.NewLine);
                    string new_total_message = null;
                    foreach (var line in splited)
                    {
                        if ((new_total_message + line + Environment.NewLine).Length > 4096)
                        {
                            await botClient.EditMessageTextAsync(
                            chatId: result.ChatID,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: new_total_message,
                            replyMarkup: Buttons.ShowLessIKM(),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken
                            );
                            new_total_message = null;
                        }
                        else
                        {
                            new_total_message += line + Environment.NewLine;
                        }
                    }
                }
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void ShowLessAction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.CallbackQuery.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var result = con.QueryFirstOrDefault<DatabaseTotalResults>($"SELECT * FROM totalresults WHERE `ChatID`={user_id} AND `botMessageID`={update.CallbackQuery.Message.MessageId}");
            con.Close();

            if (string.IsNullOrEmpty(result.fullResult)) await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken,
                text: "There are no more results."
                );
            else
            {
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );

                byte[] encodedDataAsBytesReduced = Convert.FromBase64String(result.ReducedResult);
                string full_text = System.Text.Encoding.UTF8.GetString(encodedDataAsBytesReduced);


                if (full_text.Length <= 4096)
                    try
                    {
                        await botClient.EditMessageTextAsync(
                        chatId: result.ChatID,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: full_text,
                        replyMarkup: Buttons.ShowMoreIKM(),
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        disableWebPagePreview: true,
                        cancellationToken: cancellationToken
                        );
                    }
                    catch
                    {
                        await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: update.CallbackQuery.Id,
                        text: "Error callback!",
                        cancellationToken: cancellationToken
                        );
                        return;
                    }
                else
                {
                    var splited = full_text.Split(Environment.NewLine);
                    string new_total_message = null;
                    foreach (var line in splited)
                    {
                        if ((new_total_message + line + Environment.NewLine).Length > 4096)
                        {
                            await botClient.EditMessageTextAsync(
                            chatId: result.ChatID,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: new_total_message,
                            replyMarkup: Buttons.ShowMoreIKM(),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken
                            );
                            new_total_message = null;
                        }
                        else
                        {
                            new_total_message += line + Environment.NewLine;
                        }
                    }
                }
                
            }


        }
    }
}
