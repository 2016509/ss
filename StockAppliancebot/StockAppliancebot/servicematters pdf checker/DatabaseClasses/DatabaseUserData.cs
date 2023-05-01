
namespace StockAppliance.DatabaseClasses
{
    /// <summary>
    /// CLass for userdata table
    /// </summary>
    class DatabaseUserData
    {
        /// <summary>
        /// ID of row in DB
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// User ID (Chat ID)
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [1/7]
        /// </summary>
        public int CountDiagramWEB { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [2/7]
        /// </summary>
        public int CountPartlistPDF { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [3/7]
        /// </summary>
        public int CountTechSheetPDF { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [4/7]
        /// </summary>
        public int CountServiceManualPDF { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [5/7]
        /// </summary>
        public int CountServiceManualWEB { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [6/7]
        /// </summary>
        public int CountWiringSheetPDF { get; set; }

        /// <summary>
        /// Count of lines in the collapsed response [7/7]
        /// </summary>
        public int CountServicePointerPDF { get; set; }

        /// <summary>
        /// Timeout before sending first message
        /// </summary>
        public int FirstTimeout { get; set; }

        /// <summary>
        /// Timeout before sending second message
        /// </summary>
        public int SecondTimeout { get; set; }

        /// <summary>
        /// Timeout before sending last (total) message
        /// </summary>
        public int TotalTimeout { get; set; }

        /// <summary>
        /// GoogleSpreadsheets Spreadsheet ID
        /// </summary>
        public string SpreadsheetID { get; set; }

        /// <summary>
        /// GoogleSpreadsheet Sheet name
        /// </summary>
        public string Sheet { get; set; }

        /// <summary>
        /// Api key for hm.ru
        /// </summary>
        public string ShortURLApiKey { get; set; }

    }
}
