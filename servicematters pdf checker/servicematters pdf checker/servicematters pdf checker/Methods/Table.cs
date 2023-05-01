using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using servicematters_pdf_checker.Settings;

namespace servicematters_pdf_checker.Methods
{
    class Table
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "SerMatEvePDFbot";
        static SheetsService service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
        {
            HttpClientInitializer = GoogleCredential.FromJsonParameters(AppSettings.Current.GoogleSheetsClientJson),
            ApplicationName = ApplicationName
        });

        /// <summary>
        /// Reading all rows in the Google Table by columns
        /// </summary>
        /// <param name="start_column">Start column (for ex. "A")</param>
        /// <param name="end_column">End column (for ex. "B")</param>
        /// <returns>ValueRange of result data</returns>
        public static ValueRange ReadAllTable(string start_column, string end_column)
        {


            var range = $"{AppSettings.Current.GoogleSheets.Sheet}!{start_column}:{end_column}";
            var request = service.Spreadsheets.Values.Get(AppSettings.Current.GoogleSheets.SpreadsheetID, range);
            request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;

            return request.Execute();


        }

        public static void SetColumn(string start_row, string end_row, List<string> data, string column)
        {

            String range2 = $"{AppSettings.Current.GoogleSheets.Sheet}!{column}{start_row}:{column}{end_row}";  // update cell B{x}
            ValueRange valueRange = new ValueRange();
            valueRange.MajorDimension = "COLUMNS";//"ROWS";//COLUMNS
            var oblist = new List<object>();

            foreach (var row_data in data)
                oblist.Add(row_data);

            valueRange.Values = new List<IList<object>> { oblist };

            SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, AppSettings.Current.GoogleSheets.SpreadsheetID, range2);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse result2 = update.Execute();
        }
    }
}
