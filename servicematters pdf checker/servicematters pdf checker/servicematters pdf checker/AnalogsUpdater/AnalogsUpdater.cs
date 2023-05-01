using servicematters_pdf_checker.Methods;
using System.Text.RegularExpressions;
using Dapper;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using servicematters_pdf_checker.DatabaseClasses;
using servicematters_pdf_checker.Settings;

namespace servicematters_pdf_checker.AnalogsUpdater
{
    internal class AnalogsUpdater
    {
        public static void MainChecker(CancellationToken cancellationToken)
        {


            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);


            Console.WriteLine("Table Replaces Checker started");
            while (!cancellationToken.IsCancellationRequested)
            {
                Google.Apis.Sheets.v4.Data.ValueRange range = new();

                while (true)
                {
                    try
                    {
                        range = Table.ReadAllTable("A", "H");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Taken error Google Sheets: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }


                List<string> namesList = new List<string>();
                int startRow = 2;
                int endRow = 2;

                range.Values.RemoveAt(0);
                foreach (var row in range.Values)
                {
                    if (row.Count > 5)
                    {
                        startRow++;
                        endRow++;
                        Console.WriteLine($"Пропущена строка {row[0]}. Index: { range.Values.IndexOf(row)}");
                        continue;
                    }
                    if (row.Count > 0)
                    {
                        if (row.Count > 5)
                        {
                            if (namesList.Count > 1)
                            {
                                Table.SetColumn(startRow.ToString(), endRow.ToString(), namesList, "F");
                                Console.WriteLine($"Записали имена с {startRow} до {endRow} [1!]");
                            }
                            startRow = endRow;
                            namesList = new List<string>();
                        }

                        if (namesList.Count >= 50)
                        {
                            Table.SetColumn(startRow.ToString(), endRow.ToString(), namesList, "F");
                            Console.WriteLine($"Записали имена с {startRow} до {endRow} [2!]");
                            startRow = endRow;
                            namesList = new List<string>();
                        }





                        con.Open();
                        var countString =
                            con.Query<ReplacesTable>(
                                $"SELECT * FROM `replaces_table` WHERE `inputRequest`='{MySqlHelper.EscapeString(row[0].ToString())}';");
                        con.Close();
                        if (countString != null)
                        {
                            var countInt = countString.Count();
                            if (countInt == 0)
                            {
                                con.Open();
                                con.Query($"INSERT INTO `replaces_table` (`inputRequest`) VALUES ('{MySqlHelper.EscapeString(row[0].ToString())}');");
                                con.Close();
                                Console.WriteLine($"Добавлена задача для обработки {row[0]}. Index: {range.Values.IndexOf(row)}");
                                namesList.Add("");
                            }
                            else
                            {
                                con.Open();
                                var resp = con.QueryFirstOrDefault<ReplacesTable>($"SELECT * FROM `replaces_table` WHERE `inputRequest`='{MySqlHelper.EscapeString(row[0].ToString())}';");
                                con.Close();
                                if (resp.Status == 3)
                                {
                                    Console.WriteLine($"Обнаружено готовое имя {row[0]}. Index: {range.Values.IndexOf(row)}");
                                    namesList.Add(resp.Name);
                                }
                                else
                                {
                                    Console.WriteLine($"Пишем пустоту в имя {row[0]}. Index: {range.Values.IndexOf(row)}");
                                    namesList.Add("");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"MySql error {row[0]}. Index: {range.Values.IndexOf(row)}");
                            namesList.Add("");

                        }
                        
                        endRow++;
                        Thread.Sleep(200);

                    }
                    else
                    {
                        Console.WriteLine($"Index: {range.Values.IndexOf(row)} incorrect");
                        endRow++;

                    }
                }

                
                Console.WriteLine($"Sleep 1.5 sec");
                Thread.Sleep(1500);


            }
        }
    }
}
