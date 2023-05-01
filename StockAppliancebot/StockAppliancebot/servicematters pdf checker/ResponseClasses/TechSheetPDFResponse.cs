
namespace StockAppliance.ResponseClasses
{
    /// <summary>
    /// Class for Tech Sheet PDF Data
    /// </summary>
    public sealed class TechSheetPDFResponse
    {
        /// <summary>
        /// Source (site name) of manual
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Version of Manual
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Category (may be null)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Priority of this Source
        /// </summary>
        public int Priority { get; set; }


        /// <summary>
        /// Category (may be null)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL for PDF
        /// </summary>
        public string URL { get; set; }
    }
}
