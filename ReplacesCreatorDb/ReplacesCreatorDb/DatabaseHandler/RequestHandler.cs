using MySql.Data.MySqlClient;
using ReplacesCreatorDb.Settings;
using ReplacesCreatorDb.DatabaseDTO;
using ReplacesCreatorDb.AnalogsUpdater;
using Dapper;
using Newtonsoft.Json;
using Serilog;

namespace ReplacesCreatorDb.DatabaseHandler
{
    public class RequestHandler
    {
        public static async Task NewRequestsHandel()
        {
            Log.Information($"Запущен обработчик задач из БД. Максимум потоков: {AppSettings.Current.Other.MaxThreads}");

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            while (true)
            {
                await con.OpenAsync(CancellationToken.None);
                var gotTask = con.QueryFirstOrDefault<ReplacesTable>($"SELECT * FROM replaces_table WHERE `status`<2 ORDER BY `ID` LIMIT 1;");
                await con.CloseAsync(CancellationToken.None);

                if (gotTask != null && ThreadsCount < AppSettings.Current.Other.MaxThreads)
                {
                    Log.Information($"[{ThreadsCount}]Обнаружена задача с ID #{gotTask.Id} и SKU {gotTask.InputRequest}");
                    if (gotTask.InputRequest != null)
                    {
                        await con.OpenAsync(CancellationToken.None);
                        await con.QueryAsync<ReplacesTable>($"UPDATE `replaces_table` SET `status`='2' WHERE `Id`='{gotTask.Id}';");
                        await con.CloseAsync(CancellationToken.None);
                        Task.Run(() => MakeAndUpdateReplace(gotTask));
                        ThreadsCount++;
                    }
                    else
                    {
                        Log.Warning($"Ошибка на стадии #1 при обработки запроса с ID #{gotTask.InputRequest}");
                    }
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
            
            
        }

        private static int ThreadsCount { get; set; } = 0;
        private static async Task MakeAndUpdateReplace(ReplacesTable gotTask)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            var response = Replaces.GetReplaces(gotTask.InputRequest);

            var formattedResponse = JsonConvert.SerializeObject(response.ReplacesList, Formatting.None);
            var escapedFormattedResponse = MySqlHelper.EscapeString(formattedResponse);

            var escapedName = "";
            escapedName = response.Name is not null ? MySqlHelper.EscapeString(response.Name) : null;
            var escapedPhoto = "";
            escapedPhoto = response.Base64Photo is not null ? MySqlHelper.EscapeString(response.Base64Photo) : null;
            var totalReplacesCount = 0;
            totalReplacesCount = response.ReplacesList is not null ? response.ReplacesList.Count : 0;


            await con.QueryAsync(
                $@"UPDATE `replaces_table` SET `replacesData`='{escapedFormattedResponse}', `Name`='{escapedName}', `base64Photo`='{escapedPhoto}', `totalReplacesCount`='{totalReplacesCount}', `status`='3' WHERE `Id`='{gotTask.Id}'");
            Log.Information($"[{ThreadsCount}]Записали реплейсы с ID #{gotTask.Id} и SKU {gotTask.InputRequest}. Итоговое количество реплейсов: {totalReplacesCount}");
            ThreadsCount--;

        }
    }
}
