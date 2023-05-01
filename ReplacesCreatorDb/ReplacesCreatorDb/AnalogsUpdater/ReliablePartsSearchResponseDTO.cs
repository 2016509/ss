
namespace ReplacesCreatorDb.AnalogsUpdater
{
    public sealed class ReliablePartsSearchResponseDTO
    {
       public string Status { get; set; }
       public Data Data { get; set; }
       public string Errors { get; set; }
    }
    public partial class Data
    {
        public Part[] Parts { get; set; }
    }

    public partial class Part
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public Uri UrlKey { get; set; }
        public string Brand { get; set; }
    }
}
