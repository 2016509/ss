
namespace StockAppliance.ResponseClasses
{
    /// <summary>
    /// Class for Service Pointer PDF Data
    /// </summary>
    public sealed class ServicePointerPDFResponse
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
