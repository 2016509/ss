using Dapper;
using MySql.Data.MySqlClient;
using StockAppliance.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;


namespace StockAppliance.BotFunctions
{
    public class EditRowsCount
    {

        /// <summary>
        /// Updating a countDiagramWEB in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountDiagramWeb(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countDiagramWEB`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countDiagramWEB`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Diagram WEB в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Diagram WEB в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        /// <summary>
        /// Updating a countPartlistPDF in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountPartlistPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countPartlistPDF`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countPartlistPDF`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Partlist PDF в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Partlist PDF в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        /// <summary>
        /// Updating a countTechSheetPDF in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountTechSheetPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countTechSheetPDF`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countTechSheetPDF`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Tech Sheet PDF в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Tech Sheet PDF в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        /// <summary>
        /// Updating a countServiceManualPDF in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountServiceManualPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countServiceManualPDF`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countServiceManualPDF`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Service Manual PDF в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Service Manual PDF в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        /// <summary>
        /// Updating a countServiceManualWEB in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountServiceManualWEB(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countServiceManualWEB`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countServiceManualWEB`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Service Manual WEB в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Service Manual WEB в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        /// <summary>
        /// Updating a countWiringSheetPDF in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountWiringSheetPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countWiringSheetPDF`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countWiringSheetPDF`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Wiring Sheet PDF в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Wiring Sheet PDF в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }

        /// <summary>
        /// Updating a countServicePointerPDF in Database
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void UpdateCountServicePointerPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";



            int _;
            try
            {
                _ = int.Parse(update.Message.Text.Trim());
                using var con = new MySqlConnection(cs);
                con.Open();
                con.QueryFirstOrDefault($"UPDATE userdata SET `countServicePointerPDF`={_} WHERE `userId`={user_id}");
                int update_result = con.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={user_id} AND `countServicePointerPDF`={_}");
                con.Close();

                if (update_result == 1)
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $@"✅ Количество строк Service pointer PDF в сокращенной выдаче успешно изменено!",
                        cancellationToken: cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ В процессе изменения количества строк Service pointer PDF в сокращенной выдаче произошла ошибка!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"❌ Вы должны указать целочисленное значение! 1, 2, 3 и т.д.",
                    cancellationToken: cancellationToken);
            }

            MessageHandler.SendCountOfFirstOutput(botClient, update, cancellationToken); // Вывод меню изменения вывода

        }
    }
}
