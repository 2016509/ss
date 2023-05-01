
namespace StockAppliance.Settings
{
    public sealed class GoogleSheetsSettings
    {
        /// <summary>
        /// SpreadSheet table ID
        /// </summary>
        public string SpreadsheetID { get; set; }

        /// <summary>
        /// The name of the sheet in the SpreadSheet table
        /// </summary>
        public string Sheet { get; set; }
    }
}
