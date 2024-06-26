﻿using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockPrice.SiteMethods.Canada_Sites
{
    internal class AmazonCa
    {
        private const string Source = "amazon.ca";
        private const string ClassSource = "AmazonCa";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {

            string search = request.Request.ToUpper();

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            await using var con = new MySqlConnection(cs);

            await con.OpenAsync();
            var settings = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={request.ChatID}");
            var takenProxyList = con.Query<DatabaseProxyData>($"SELECT * FROM proxy_table_amazon_ca WHERE `isActive`='1' ORDER BY RAND()").ToList(); ;
            await con.CloseAsync();


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.amazon.ca/s?i=aps&k={search}&ref=nb_sb_noss&url=search-alias=aps", Source = "Amazon.ca", Additional = "🅰" };
            var prices = new List<Prices>();


            string searchRes = null;
            while (takenProxyList.Any())
            {
                var takenProxy = takenProxyList.First();

                try
                {


                    List<CustomHttpAdditionals.Headers> headers = new()
                    {
                        new CustomHttpAdditionals.Headers
                        {
                            Name = "Accept", Value = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
                        },
                        new CustomHttpAdditionals.Headers
                        {
                            Name = "Accept-Language", Value = "en-US, en;q=0.5"
                        }
                    };

                    searchRes = CustomHttpClass.GetToString(@$"https://www.amazon.ca/s?k={search.ToLower()}",
                        headers: headers, acceptencoding: "gzip, deflate",
                        use_chrome_random_ua: true,
                        use_google_ua: false,
                        selected_proxy: takenProxy);
                    break;
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message.ToString(),
                        stage: 0,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: JsonConvert.SerializeObject(takenProxy, Formatting.None),
                        url: null);

                    
                    takenProxyList.Remove(takenProxy);
                    

                }
            }

            if (takenProxyList.Count == 0)
            {
                mpr.NoAnswerOrError = true;
                mpr.ErrorMessage = "Proxy list ended";
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
                return;
            }

            if (searchRes != null)
            {
                var responseResultsHtmlRegex = Regex.Matches(searchRes, @"<div data-asin=""\w.*</div>");
                if (responseResultsHtmlRegex.Count > 0)
                {



                    List<string> searchResults = new();
                    for (int i = 0; i < responseResultsHtmlRegex.Count; i++) searchResults.Add(responseResultsHtmlRegex[i].Value.Trim());

                    searchResults = searchResults.Where(x => x.Contains("search-result")).ToList();
                    searchResults = searchResults.Where(x => !x.Contains("import fees deposit")).ToList();

                    if (searchResults.Count > 1) mpr.MultiChoice = true;
                    if (searchResults.Count == 0)
                    {
                        mpr.NothingFoundOrOutOfStock = true;
                        mainPriceResponsesList.Add(mpr);

                        await ResponseCreator.MakeResponseLog(con: con,
                            mpr: mpr,
                            request: request);
                        return;
                    }

                    foreach (var html_taken_res in searchResults)
                    {
                        string title = null;
                        string url = null;
                        decimal price = 0;
                        int deliveryDays = 0;


                        var titleRegex = Regex.Matches(html_taken_res, @"(?<=a-text-normal"">).*?(?=</span>)");
                        if (titleRegex.Count > 0)
                        {
                            title = titleRegex[0].Value;
                            var checkTitle = Regex.Matches(title, @$"{Regex.Escape(search)}.*?");
                            if (checkTitle.Count == 0) continue;

                        }

                        var urlRegex = Regex.Matches(html_taken_res, @"(?<="" href="").*?(?="")");
                        if (urlRegex.Count > 0)
                        {
                            url = "https://www.amazon.ca" + urlRegex[0].Value;
                        }

                        var priceRegex = Regex.Matches(html_taken_res, @"(?<=>\$).*?(?=<)");
                        if (priceRegex.Count > 0)
                        {
                            price = decimal.Parse(priceRegex.First().Value, CultureInfo.InvariantCulture);
                        }

                        var dateRegex = Regex.Matches(html_taken_res, @"(?<=a-text-bold"">).*?(?=</span>)");
                        if (dateRegex.Count > 0)
                        {
                            string takenAmazonDate = dateRegex.First().Value;

                            var totalAmazonDateRegex = Regex.Matches(takenAmazonDate, @"(?<=,\ )(?i)[a-z]{2,3}.*?\ [0-9]{1,3}");
                            if (totalAmazonDateRegex.Count == 0) totalAmazonDateRegex = Regex.Matches(takenAmazonDate, @"\w{3}\s\d.*\W");
                            if (totalAmazonDateRegex.Count > 0)
                            {
                                string totalAmazonDate = totalAmazonDateRegex.First().Value;

                                var amazonDateSplit = totalAmazonDate.Split(' ');
                                if (amazonDateSplit.Count() >= 2)
                                {
                                    string amazonDateMonthStr = amazonDateSplit[0];
                                    string amazonDateDay = amazonDateSplit[1];
                                    string amazonDateMonth = null;
                                    string amazonDateYear = null;

                                    switch (amazonDateMonthStr)
                                    {
                                        case "Jan":
                                            amazonDateMonth = "01";
                                            break;
                                        case "Feb":
                                            amazonDateMonth = "02";
                                            break;
                                        case "Mar":
                                            amazonDateMonth = "03";
                                            break;
                                        case "Apr":
                                            amazonDateMonth = "04";
                                            break;
                                        case "May":
                                            amazonDateMonth = "05";
                                            break;
                                        case "Jun":
                                            amazonDateMonth = "06";
                                            break;
                                        case "Jul":
                                            amazonDateMonth = "07";
                                            break;
                                        case "Aug":
                                            amazonDateMonth = "08";
                                            break;
                                        case "Sep":
                                            amazonDateMonth = "09";
                                            break;
                                        case "Oct":
                                            amazonDateMonth = "10";
                                            break;
                                        case "Nov":
                                            amazonDateMonth = "11";
                                            break;
                                        case "Dec":
                                            amazonDateMonth = "12";
                                            break;
                                        default: continue;
                                    }

                                    DateTime now_date = DateTime.Now;

                                    if (now_date.Month > int.Parse(amazonDateMonth))
                                    {
                                        amazonDateYear = (now_date.Year + 1).ToString();
                                    }
                                    else amazonDateYear = now_date.Year.ToString();

                                    DateTime amazonDeliveryDate = DateTime.Parse($"{amazonDateDay}.{amazonDateMonth}.{amazonDateYear}");

                                    deliveryDays = (amazonDeliveryDate - now_date).Days + 1;


                                    if (deliveryDays <= settings.MaxDeliveryDays && title != null)
                                    {
                                        
                                        prices.Add(new()
                                        {
                                            DeliveryDays = deliveryDays.ToString(),
                                            Price = price,
                                            Title = null,
                                            Url = ShortUrl.MakeShortURLClckRU(url)
                                        });

                                    }


                                }
                            }
                        }






                    }

                    var pricesOrdered = prices.OrderBy(x => x.Price).ToList();
                    if (pricesOrdered.Count > 0)
                    {
                        mpr.LowestPrice = pricesOrdered.First().Price;
                        mpr.PricesList = prices;

                        mainPriceResponsesList.Add(mpr);

                        await ResponseCreator.MakeResponseLog(con: con,
                            mpr: mpr,
                            request: request);
                    }
                    else
                    {

                        mpr.PricesList = prices;
                        mpr.NothingFoundOrOutOfStock = true;

                        mainPriceResponsesList.Add(mpr);

                        await ResponseCreator.MakeResponseLog(con: con,
                            mpr: mpr,
                            request: request);
                        return;
                    }




                }
                else
                {
                    mpr.PricesList = prices;
                    mpr.NothingFoundOrOutOfStock = true;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }
            }


        }
    }
}
