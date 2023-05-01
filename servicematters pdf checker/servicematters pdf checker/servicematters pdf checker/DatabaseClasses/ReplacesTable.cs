

namespace servicematters_pdf_checker.DatabaseClasses
{
    public sealed class ReplacesTable
    {
        /// <summary>
        /// Id of request
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Input data (sku)
        /// </summary>
        public string? InputRequest { get; set; } = null;

        /// <summary>
        /// Title of requested SKU
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// Total replaces list json-formatted 
        /// </summary>
        public string? ReplacesData { get; set; } = null;

        /// <summary>
        /// Status of task. 0 - added, 1 - in work, 2 - ready
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Total replaces count from `ReplacesData` for analyze errors
        /// </summary>
        public int TotalReplacesCount { get; set; }

        /// <summary>
        /// Base64 typed photo Data. May be null.
        /// </summary>
        public string? Base64Photo { get; set; } = null;

    }
}
