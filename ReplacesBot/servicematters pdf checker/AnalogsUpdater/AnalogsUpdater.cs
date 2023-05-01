using Newtonsoft.Json;
using ReplacesBot.Methods;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace ReplacesBot.AnalogsUpdater
{
    internal class Replaces
    {


        public static async void GetReplaces(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
        {
            string search = update.Message.Text;

            List<string> replaces_list = new();
            List<string> names_list = new();
            List<PhotosData> photos_url_list = new();
            List<MainSKUData> mainSKU_list = new();

            List<Task> tasks = new();
            
            //Запускаем поток для Fix.Com
            tasks.Add(Task.Run(() => FixComReplaces(search, replaces_list, names_list, photos_url_list, mainSKU_list)));
            //Запускаем поток для reliableparts.com
            tasks.Add(Task.Run(() => ReliablePartsReplaces(search, replaces_list, names_list, photos_url_list, mainSKU_list)));
            //Запускаем поток для reliableparts.com
            tasks.Add(Task.Run(() => AppliancePartsHQReplaces(search, replaces_list, names_list, photos_url_list, mainSKU_list)));
            //Запускаем поток для reliableparts.com
            tasks.Add(Task.Run(() => AppliancePartsPros(search, replaces_list, names_list, photos_url_list, mainSKU_list)));
            //Запускаем поток для easyapplianceparts.ca
            tasks.Add(Task.Run(() => EasyAppliancePartsCaNames(search, names_list)));
            //Запускаем поток для partselect.com
            tasks.Add(Task.Run(() => PartSelectComNames(search, names_list)));
            //Запускаем поток для partselect.ca
            tasks.Add(Task.Run(() => PartSelectCaNames(search, names_list)));
            //Запускаем поток для apwagner.com
            tasks.Add(Task.Run(() => ApWagnerComNames(search, names_list, photos_url_list)));
            //Запускаем поток для midbec.com
            tasks.Add(Task.Run(() => MidbecComReplaces(search, replaces_list, mainSKU_list)));
            //Запускаем поток для PartAdventage.com
            tasks.Add(Task.Run(() => PartAdventageReplaces(search, replaces_list, photos_url_list, mainSKU_list)));
            //Запускаем поток для AmreSupply.com
            tasks.Add(Task.Run(() => AmreSupplyComPhotos(search, photos_url_list)));
            //Запускаем поток для AmreSupply.com
            tasks.Add(Task.Run(() => WhirlPoolpartsComPhotos(search, photos_url_list)));
            
            Task.WaitAll(tasks.ToArray());

            replaces_list = replaces_list.Distinct().ToList();



            //Удаляем из реплейса вхождения с поисковым запросом
            replaces_list.Remove(search.ToLower());
            replaces_list.Remove(search.ToUpper());

            //Удаляем из листа mainSKU вхождение с поисковым запросом
            mainSKU_list.RemoveAll(x => x.Data.ToUpper().Equals(search.ToUpper()));
            mainSKU_list.RemoveAll(x => x.Data.ToLower().Equals(search.ToLower()));

            //Удаляем имена кривые
            names_list.RemoveAll(x => x.Trim().Contains(@"Searching, please wait...") || x.ToUpper().Contains("USE WPL"));

            if (replaces_list.Count == 0)
            {
                var msg = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    text: @"Sorry, nothing found",
                    replyToMessageId: update.Message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    cancellationToken: cancellationToken
                    );
                return;
            }

            string main_sku = null;

            if (mainSKU_list.Count > 0)
            {
                main_sku = mainSKU_list.OrderBy(x => x.Priority).First().Data;
                replaces_list.Add(search);
            }
            else
            {
                main_sku = search;
            }



            string title = null;

            if (names_list.Count > 0)
            {
                int minLength = names_list.Min(y => y.Length); // this gets you the shortest length of all elements in names
                title = names_list.FirstOrDefault(x => x.Length == minLength);
                
            }

            string photo_url = null;
            string photo_source = null;
            Stream total_photo_stream = null;

            if (photos_url_list.Count > 0)
            {
                photo_url = photos_url_list.OrderBy(x => x.Priority).First().URL;
                photo_source = photos_url_list.OrderBy(x => x.Priority).First().Source;

                try
                {
                    total_photo_stream = CustomHttpClass.GetToStream(photo_url, use_google_ua: false);
                }
                catch
                {
                    total_photo_stream = null;

                }
            }


            
            

            if (total_photo_stream != null)
            {
                string photo_description = @$"<b><u>{title}</u></b>" + Environment.NewLine +
                    main_sku + Environment.NewLine +
                    @$"<b><u>Replaces: </u></b>" + Environment.NewLine +
                    String.Join(Environment.NewLine, replaces_list);


                try
                {
                    var msg = await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id,
                        photo: new InputOnlineFile(total_photo_stream),
                        caption: photo_description,
                        replyToMessageId: update.Message.MessageId,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        cancellationToken: cancellationToken
                        );
                }
                catch
                {
                    var msg = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    text: photo_description,
                    replyToMessageId: update.Message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    cancellationToken: cancellationToken
                    );
                    Console.WriteLine($@"Error with photo. URL: {photo_url}, search: {update.Message.Text}");
                }
            }
            else
            {
                string photo_description = @$"<b><u>{title}</u></b>" + Environment.NewLine +
                    main_sku + Environment.NewLine +
                    @$"<b><u>Replaces: </u></b>" + Environment.NewLine +
                    String.Join(Environment.NewLine, replaces_list);

                var msg = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    text: photo_description,
                    replyToMessageId: update.Message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    cancellationToken: cancellationToken
                    );
            }


        }

        public static void FixComReplaces(string search, List<string> replaces, List<string> tempNames, List<PhotosData> photos_url_list, List<MainSKUData> mainSKU_list)
        {


            try
            {
            startlocation:
                string location = CustomHttpClass.CheckRedirectGet(@$"https://www.fix.com/Search.ashx?SearchTerm={search}&SearchMethod=standard");
                if (!string.IsNullOrEmpty(location))
                {
                    string html_result = CustomHttpClass.GetToString($@"{location}");
                    var title_regex = Regex.Matches(html_result, @"(?<=itemprop=""name"">).*(?=</h1>)");

                    //Добавляем тайтл
                    if (title_regex.Count > 0)
                    {
                        if (title_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(title_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                search = wpl_regex.First().Value.Trim();
                                goto startlocation;
                            }


                        }
                        tempNames.Add(title_regex.First().Value.Trim());
                    }

                        var manufacture_regex = Regex.Matches(html_result, @"(?<=<span itemprop=""mpn"">).*?(?=</span></div>)");
                        if (manufacture_regex.Count > 0) replaces.Add(manufacture_regex.First().Value.Trim());

                        var photos_regex = Regex.Matches(html_result, @"(?<=""MagicZoom-PartImage-Images"" href="").*(?="" class=""MagicZoom"")");
                        if (photos_regex.Count > 0) photos_url_list.Add(new()
                        {
                            Priority = 7,
                            Source = "fix.com",
                            URL = photos_regex.First().Value.Trim()
                        });

                        var replaces_regex_html_block = Regex.Matches(html_result, @"(?<=replaces these:</div>)[\w\W]*?</div>");
                        if (replaces_regex_html_block.Count > 0)
                        {

                            var replaces_regex = Regex.Matches(replaces_regex_html_block.First().Value, @"(?<=>)[\w\W]*?(?=</div>)");
                            if (replaces_regex.Count > 0)
                            {
                                var replaces_split = Regex.Replace(replaces_regex.First().Value.Replace(".","").Replace("Show more","").Replace("Show less","").Replace("\r", "").Replace("\n",""), @"<(.|\n)*?>", "").Split(',').ToList();
                                foreach (var replace in replaces_split)
                                {
                                    replaces.Add(replace.Trim());
                                }
                            }

                        }

                    

                }
            }
            catch
            {
                Console.WriteLine("Error FIX.COM - STAGE 1 - " + search);
            }

        }

        public static void ReliablePartsReplaces(string search, List<string> replaces, List<string> tempNames, List<PhotosData> photos_url_list, List<MainSKUData> mainSKU_list)
        {
            string location = null;

            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.reliableparts.com/search?q={search}",
                referrer: "https://www.reliableparts.com/",
                acceptencoding: "none");
            }
            catch
            {

                Console.WriteLine($"Error on ReliableParts registered. STAGE - 0    Request: {search}");
                return;
            }

            if (location != null)
            {
               
                try
                {
                    string total_result = CustomHttpClass.GetToString(@$"https://www.reliableparts.com{location}",
                 referrer: $"https://www.reliableparts.com/search?q={search}",
                 acceptencoding: "none");

                    var manufacture_regex = Regex.Matches(total_result, @"(?<=""partNumber"">Part #: ).*(?=</h2>)");
                    if (manufacture_regex.Count > 0)
                    {
                        mainSKU_list.Add(new()
                        {
                            Source = "reliableparts.com",
                            Priority = 1,
                            Data = manufacture_regex.First().Value.Trim()
                        });
                        replaces.Add(manufacture_regex.First().Value.Trim());
                    }


                    var photo_regex = Regex.Matches(total_result, @"(?<=data-image="").*(?="" class=""MagicZoom"")");
                    if (photo_regex.Count > 0) photos_url_list.Add(new()
                    {
                        Source = "reliableparts.com",
                        Priority = 1,
                        URL = photo_regex.First().Value.Trim()

                    });

                    var name_regex = Regex.Matches(total_result, @"(?<=id=""mainHeading"">).*(?=</h1>)");
                    if (name_regex.Count > 0) tempNames.Add(name_regex.First().Value.Trim());


                    var replaces_table_regex = Regex.Matches(total_result, @"(?<=<div class=""accordian-point"">)[\w\W]*?(?=</div>)");
                    if (replaces_table_regex.Count > 0)
                    {
                        string replaces_table = replaces_table_regex.First().Value;
                        var replaces_regex = Regex.Matches(replaces_table, @"(?<=<li>).*(?=</li>)");

                        foreach (var replace in replaces_regex)
                        {
                            if (replace.ToString().Contains("href") || replace.ToString().Contains("http")) continue;
                            replaces.Add(replace.ToString().Trim());
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"Error on ReliableParts registered. STAGE - 1    Request: {search}");
                    return;
                }
            }
        }

        public static void AppliancePartsHQReplaces(string search, List<string> replaces, List<string> tempNames, List<PhotosData> photos_url_list, List<MainSKUData> mainSKU_list)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.appliancepartshq.ca/search?hq={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    List<string> tempreplaces = new();
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none");

                    var manufacture_regex0 = Regex.Matches(result, @"(?<=SKU: <span>).*?(?=</span></div>)");
                    if (manufacture_regex0.Count > 0) tempreplaces.Add(manufacture_regex0.First().Value.Trim());
                    var replaces_regex0 = Regex.Matches(result, @"(?<=Replaces Old Numbers:</span></strong> &nbsp;)[\w\W]*?(?=</p>)");
                    if (replaces_regex0.Count > 0)
                    {
                        var replaces_table = replaces_regex0.First().Value;
                        var replaces_split = replaces_table.Split(',');
                        foreach (var replace in replaces_split)
                        {
                            tempreplaces.Add(replace.Trim());
                        }
                    }

                    if (tempreplaces.Contains(search.ToLower()) || tempreplaces.Contains(search.ToUpper()))
                    {

                        var manufacture_regex = Regex.Matches(result, @"(?<=SKU: <span>).*?(?=</span></div>)");
                        if (manufacture_regex.Count > 0)
                        {
                            mainSKU_list.Add(new()
                            {
                                Priority = 2,
                                Source = "appliancepartshq.ca",
                                Data = manufacture_regex.First().Value.Trim()
                            });
                            replaces.Add(manufacture_regex.First().Value.Trim());
                        }

                        var name_regex = Regex.Matches(result, @"(?<=<title>).*(?=</title>)");
                        if (name_regex.Count > 0)
                        {
                            string tempname = name_regex.First().Value.Trim();
                            var tempname_split = tempname.Split(':').ToList();
                            tempNames.Add(tempname_split.Last().Trim());
                        }

                        var photo_regex = Regex.Matches(result, @"(?<="" src="").*?(?="">)");
                        if (photo_regex.Count > 0)
                            photos_url_list.Add(new()
                            {
                                Priority = 8,
                                Source = "appliancepartshq.ca",
                                URL = photo_regex.First().Value.Trim()
                            });

                        var replaces_regex = Regex.Matches(result, @"(?<=Replaces Old Numbers:</span></strong> &nbsp;)[\w\W]*?(?=</p>)");
                        if (replaces_regex.Count > 0)
                        {
                            var replaces_table = replaces_regex.First().Value;
                            var replaces_split = replaces_table.Split(',');
                            foreach (var replace in replaces_split)
                            {
                                replaces.Add(replace.Trim());
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 1 - {search}");
                }
            }
            else
            {
                string search_result = null;
                try
                {
                    search_result = CustomHttpClass.GetToString(@$"https://www.appliancepartshq.ca/search?hq={search}", acceptencoding: "none");


                }
                catch
                {
                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 2 - {search}");
                }
                if (search_result != null)
                {
                    var search_result_regex_block = Regex.Matches(search_result, @"(?<=<div class=""productListing"">)[\w\W]*?(?=</div>)");

                    foreach (var search_result_block in search_result_regex_block)
                    {
                        var href_regex = Regex.Matches(search_result_block.ToString(), @"(?<=<a href="").*?(?="" title=)");
                        var title_regex = Regex.Matches(search_result_block.ToString(), @"(?<=title="").*?(?="">)");

                        if (href_regex.Count > 0 && title_regex.Count > 0 && href_regex.Count == title_regex.Count)
                        {
                            List<string> title_list = title_regex.First().Value.Split(' ').ToList();
                            string href = href_regex.First().Value;

                            if (title_list.Contains(search.ToLower()) || title_list.Contains(search.ToUpper()))
                            {
                                location = href;

                                string result = null;
                                try
                                {
                                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none");

                                    var manufacture_regex = Regex.Matches(result, @"(?<=SKU: <span>).*?(?=</span></div>)");
                                    if (manufacture_regex.Count > 0) replaces.Add(manufacture_regex.First().Value.Trim());

                                    var replaces_regex = Regex.Matches(result, @"(?<=Replaces Old Numbers:</span></strong> &nbsp;)[\w\W]*?(?=</p>)");
                                    if (replaces_regex.Count > 0)
                                    {
                                        var replaces_table = replaces_regex.First().Value;
                                        var replaces_split = replaces_table.Split(',');
                                        foreach (var replace in replaces_split)
                                        {
                                            replaces.Add(replace.Trim());
                                        }
                                    }

                                    var photo_regex = Regex.Matches(result, @"(?<="" src="").*?(?="">)");
                                    if (photo_regex.Count > 0)
                                        photos_url_list.Add(new()
                                        {
                                            Priority = 8,
                                            Source = "appliancepartshq.ca",
                                            URL = photo_regex.First().Value.Trim()
                                        });
                                }
                                catch
                                {
                                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 3 - {search}");
                                }

                            }
                        }
                    }
                }
            }

        }

        public static void AppliancePartsPros(string search, List<string> replaces, List<string> tempNames, List<PhotosData> photos_url_list, List<MainSKUData> mainSKU_list)
        {
            //(?<=replaces).*?(?=\.) - для реплейсов (вывод через запятую, требутеся трим)

            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.appliancepartspros.com/search.aspx?q={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"https://www.appliancepartspros.com{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=itemprop=""name"">).*(?=</h1>)");
                    if (name_regex.Count > 0) tempNames.Add(name_regex.First().Value.Trim());

                    var replaces_regex = Regex.Matches(result, @"(?<=replaces).*?(?=\.)");
                    if (replaces_regex.Count > 0)
                    {
                        var replaces_split = replaces_regex.First().Value.Split(',');
                        foreach (var replace in replaces_split)
                            replaces.Add(replace.Trim());
                    }
                }
                catch
                {
                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void EasyAppliancePartsCaNames(string search, List<string> tempNames)
        {
            //(?<=replaces).*?(?=\.) - для реплейсов (вывод через запятую, требутеся трим)
            startlocation:
            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.easyapplianceparts.ca/Search.ashx?SearchTerm={search}&SearchMethod=standard", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"EasyAppliancePartsCaNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=""standard-blue-title"">).*(?=</h1>)");
                    if (name_regex.Count > 0)
                    {
                        if (name_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(name_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                search = wpl_regex.First().Value.Trim();
                                goto startlocation;
                            }

                            
                        }
                        tempNames.Add(name_regex.First().Value.Trim());
                    }

                }
                catch
                {
                    Console.WriteLine($"EasyAppliancePartsCaNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void PartSelectComNames(string search, List<string> tempNames)
        {

            startlocation:
            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.partselect.com/Search.ashx?SearchTerm={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"PartSelectComNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=itemprop=""name"">).*(?=</h1>)");
                    if (name_regex.Count > 0)
                    {
                        if (name_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(name_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                search = wpl_regex.First().Value.Trim();
                                goto startlocation;
                            }

                            
                        }
                        tempNames.Add(name_regex.First().Value.Trim());
                    }

                }
                catch
                {
                    Console.WriteLine($"PartSelectComNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void PartSelectCaNames(string search, List<string> tempNames)
        {

            startlocation:
            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.partselect.ca/Search.ashx?SearchTerm={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"PartSelectCaNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=itemprop=""name"">).*(?=</h1>)");
                    if (name_regex.Count > 0)
                    {
                        if (name_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(name_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                search = wpl_regex.First().Value.Trim();
                                goto startlocation;
                            }

                            
                        }
                        tempNames.Add(name_regex.First().Value.Trim());
                    }

                }
                catch
                {
                    Console.WriteLine($"PartSelectCaNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void ApWagnerComNames(string search, List<string> tempNames, List<PhotosData> photos_url_list)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.apwagner.com/search/{search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"ApWagnerComNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"https://www.apwagner.com{location}", acceptencoding: "none");

                    var title_result_regex = Regex.Matches(result, @"(?<=<title>)[\w\W]*?(?=</title>)");
                    if (title_result_regex.Count > 0 && title_result_regex.First().Value.Trim().Contains(@"USE WPL"))
                    {
                        var href_use_wpl_regex = Regex.Matches(result, @"(?<=href="").*(?="">USE WPL)");
                        if (href_use_wpl_regex.Count > 0)
                        {
                            try
                            {
                                result = CustomHttpClass.GetToString($@"https://www.apwagner.com{href_use_wpl_regex.First().Value.Trim()}", acceptencoding: "none");
                            }
                            catch
                            {
                                Console.WriteLine($"ApWagnerComNames ERROR - STAGE 2 - {search}");
                            }

                            var name_regex = Regex.Matches(result, @"(?<=<h1 itemprop=""name"">)[\w\W]*?(?=</h1>)");
                            if (name_regex.Count > 0)
                            {
                                tempNames.Add(name_regex.First().Value.Trim());
                            }
                        }

                    }
                    else
                    {
                        var name_regex = Regex.Matches(result, @"(?<=<h1 itemprop=""name"">)[\w\W]*?(?=</h1>)");
                        if (name_regex.Count > 0) tempNames.Add(name_regex.First().Value.Trim());

                        var photo_regex = Regex.Matches(result, @"(?<=class=""defaultimage"" src="").*?(?="")");
                        if (photo_regex.Count > 0)
                            foreach (var photo in photo_regex)
                                if (CustomHttpClass.GetIsExist(url: photo.ToString()))
                                {
                                    photos_url_list.Add(new() { Priority = 2, Source = "apwagner.com", URL = photo_regex.First().Value });
                                    return;
                                }

                    }




                }
                catch
                {
                    Console.WriteLine($"ApWagnerComNames ERROR - STAGE 1 - {search}");
                }

            }
            else
            {
                if (photos_url_list.Count > 0) return;
                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString(@$"https://www.apwagner.com/search/{search}", acceptencoding: "none");

                }
                catch
                {
                    Console.WriteLine($"ApWagnerComNames ERROR - STAGE 2 - {search}");
                }
                if (result != null)
                {
                    var results_htmls = Regex.Matches(result, @"(?<=<tr>)[\w\W]*?(?=</tr>)");
                    foreach(var results_html in results_htmls)
                    {
                        var photo_regex = Regex.Matches(results_html.ToString(), @"(?<=src="")[\w\W]*?(?="" onerror)");
                        if (photo_regex.Count > 0)
                        if (CustomHttpClass.GetIsExist(photo_regex.First().Value.Trim(), acceptencoding: "none"))
                            {
                                photos_url_list.Add(new()
                                {
                                    Priority = 2,
                                    Source = "apwagner.com",
                                    URL = photo_regex.First().Value.Trim()
                                });
                            }
                    }
                }
            }

        }

        public static void MidbecComReplaces(string search, List<string> replaces, List<MainSKUData> mainSKU_list)
        {






            string result = null;
            try
            {

                result = CustomHttpClass.Post(url: @"https://midbec.com/core/direct/router",
                    jsonData: $@"[{{""action"":""ecommerce.ajax"",""method"":""updateProductTableList"",""data"":[{{""search"":""{search}"",""categories"":[],""currentPage"":1,""offset"":0,""nbPerPage"":""15"",""sortBy"":""relevance"",""pricerange"":""0"",""manufacturer"":[""""],""pageTagName"":""products""}}],""type"":""rpc"",""tid"":0}}]",
                    contentType: "application/json");




            }
            catch
            {
                Console.WriteLine($"ApWagnerComNames ERROR - STAGE 1 - {search}");
            }

            if (result != null)
                try
                {
                    dynamic _j = JsonConvert.DeserializeObject(result);
                    string replace = _j[0].result.products[0].sku.ToString();
                    mainSKU_list.Add(new()
                    {
                        Source = "midbec",
                        Priority = 3,
                        Data = replace.Trim() 
                    });
                    replaces.Add(replace.Trim());
                }
                catch
                {

                }


        }
        public static void PartAdventageReplaces(string search, List<string> replaces, List<PhotosData> photos_url_list, List<MainSKUData> mainSKU_list)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.partadvantage.com/search?q={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"PartAdventageReplaces ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"https://www.partadvantage.com{location}", acceptencoding: "none");

                    var manufacture_regex = Regex.Matches(result, @"(?<=clearBoth productDescription"">Part #:).*,(?=<)");
                    if (manufacture_regex.Count > 0)
                    {
                        mainSKU_list.Add(new()
                        {
                            Source = "partadventage",
                            Priority = 4,
                            Data = manufacture_regex.First().Value.Trim()
                        });
                        replaces.Add(manufacture_regex.First().Value.Trim());
                    }


                    var replaces_regex = Regex.Matches(result, @"(?<=<strong>Replaces Old Numbers:</strong> &nbsp; )[\w\W]*?(?=</p>)");
                    if (replaces_regex.Count > 0)
                    {
                        var replaces_table = replaces_regex.First().Value;
                        var replaces_split = replaces_table.Split(',');
                        foreach (var replace in replaces_split)
                        {
                            replaces.Add(replace.Trim());
                        }
                    }

                    var photo_regex = Regex.Matches(result, @"(?<=<div class=""primaryImage""><a href="").*(?="" data-image=)");
                    if (photo_regex.Count > 0) photos_url_list.Add(new()
                    {
                        Priority = 10,
                        Source = "partadvantage.com",
                        URL = photo_regex.First().Value.Trim()
                    });

                }
                catch
                {
                    Console.WriteLine($"PartAdventageReplaces ERROR - STAGE 1 - {search}");
                }

            }

        }


        public static void AmreSupplyComPhotos(string search, List<PhotosData> photos_url_list)
        {

            if (photos_url_list.Count > 0) return;
            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.amresupply.com/search?q={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"AmreSupplyComPhotos ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var photo_regex = Regex.Matches(result, @"(?<=src="").*(?="" class=""img-responsive mainImage"")");
                    if (photo_regex.Count > 0) photos_url_list.Add(new()
                    {
                        Priority = 4,
                        URL = photo_regex.First().Value.Trim(),
                        Source = "amresupply.com"
                    });


                }
                catch
                {
                    Console.WriteLine($"AmreSupplyComPhotos ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void WhirlPoolpartsComPhotos(string search, List<PhotosData> photos_url_list)
        {

            if (photos_url_list.Count > 0) return;
            
            string location_first = null;
            try
            {
                int rnd = new Random().Next(2000, 20000);
                string response_search = CustomHttpClass.Post(url: $@"https://www.whirlpoolparts.com/PartSearch/GetSearchUrlSSL?searchText={search}&searchContext=w1", 
                    jsonData: @$"textBoxHistory=[{{""text"":"""",""time"":0}},{{""text"":""{search}"",""time"":{rnd}}}]",
                    contentType: "application/x-www-form-urlencoded");
                dynamic _j = JsonConvert.DeserializeObject(response_search);
                location_first = _j.url.ToString();
            }
            catch
            {
                Console.WriteLine($"WhirlPoolpartsComPhotos ERROR - STAGE 0 - {search}");
            }


            if (location_first == null) return;

            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.whirlpoolparts.com{location_first}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"WhirlPoolpartsComPhotos ERROR - STAGE 1 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"https://www.whirlpoolparts.com{location}", acceptencoding: "none", use_google_ua: false);

                    var photo_regex = Regex.Matches(result, @"(?<=<img src="")https://www.*?jpg(?="" alt="")");
                    if (photo_regex.Count > 0) photos_url_list.Add(new()
                    {
                        Priority = 6,
                        URL = photo_regex.First().Value.Trim(),
                        Source = "whirlpoolparts.com"
                    });


                }
                catch
                {
                    Console.WriteLine($"WhirlPoolpartsComPhotos ERROR - STAGE 2 - {search}");
                }

            }

        }

    }
}
