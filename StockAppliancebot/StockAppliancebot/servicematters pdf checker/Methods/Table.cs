using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using StockAppliance.Settings;

namespace StockAppliance.Methods
{
    class Table
    {

        static readonly string ApplicationName = "SerMatEvePDFbot";
        static readonly SheetsService service = new(new Google.Apis.Services.BaseClientService.Initializer()
        {
            HttpClientInitializer = GoogleCredential.FromJsonParameters(AppSettings.Current.GoogleSheetsClientJson),
            ApplicationName = ApplicationName,
        });

        /// <summary>
        /// Reading all rows in the Google Table by columns
        /// </summary>
        /// <param name="start_column">Start column (for ex. "A")</param>
        /// <param name="end_column">End column (for ex. "B")</param>
        /// <returns>ValueRange of result data</returns>
        public static Google.Apis.Sheets.v4.Data.ValueRange ReadAllTable(string start_column, string end_column)
        {


            var range = $"{AppSettings.Current.GoogleSheets.Sheet}!{start_column}:{end_column}";
            var request = service.Spreadsheets.Values.Get(AppSettings.Current.GoogleSheets.SpreadsheetID, range);

            return request.Execute();


        }
    }
}
