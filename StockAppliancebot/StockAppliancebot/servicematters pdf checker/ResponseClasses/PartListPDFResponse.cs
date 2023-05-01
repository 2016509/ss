

namespace StockAppliance.ResponseClasses
{
    /// <summary>
    /// Class for Parlist PDF data
    /// </summary>
    public sealed class PartListPDFResponse
    {
        /// <summary>
        /// Source of Part List
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Priority of this Source
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Category (may be null)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Title 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL for PDF
        /// </summary>
        public string URL { get; set; }
    }
}
