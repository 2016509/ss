
namespace StockAppliance.ResponseClasses
{

    /// <summary>
    /// Class for DiagramWeb Data
    /// </summary>
    public sealed class DiagramWebResponse
    {
        /// <summary>
        /// Source (site name) of Diagram Web
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Priority [if == 0 => random]
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Search url 
        /// </summary>
        public string SearchUrl { get; set; }

        /// <summary>
        /// Title of first item in search response
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL of diagram Web
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Version 
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Count of Results on site
        /// </summary>
        public string ResultCount { get; set; } = "All";

    }
}
