using Newtonsoft.Json;

namespace StockAppliance.SiteMethods.Classes
{
    public sealed class SearsPartsDirectClasses
    {
        public sealed class VariablesConstructor

        {

            public sealed class Variables
            {
                [JsonProperty("q")]
                public string Q { get; set; }

                [JsonProperty("page")]
                public Page Page { get; set; } = new();

                [JsonProperty("priceFilter")]
                public PriceFilters PriceFilters { get; set; } = new();

                [JsonProperty("filters")]
                public Filters Filters { get; set; } = new();
            }
            public class Page
            {
                [JsonProperty("from")]
                public int From { get; set; } = 0;

                [JsonProperty("size")]
                public int Size { get; set; } = 20;

            }

            public class PriceFilters
            {
                [JsonProperty("name")]
                public string Name { get; set; } = "PRICE";

                [JsonProperty("type")]
                public string Type { get; set; } = "RANGE";

                [JsonProperty("values")]
                public List<string> Values { get; set; } = new() { ">1" };
            }

            public class Filters
            {
                [JsonProperty("name")]
                public string Name { get; set; } = "SCHEMATICCOUNT";

                [JsonProperty("type")]
                public string Type { get; set; } = "RANGE";

                [JsonProperty("values")]
                public List<string> Values { get; set; } = new() { ">0" };
            }
        }

        public sealed class ExtesionsConstructor
        {
            public sealed class Extensions
            {
                [JsonProperty("persistedQuery")]
                public PersistedQuery PersistedQuery { get; set; }
            }

            public sealed class PersistedQuery
            {
                [JsonProperty("version")]
                public int Version { get; set; }

                [JsonProperty("sha256Hash")]

                public string Sha256Hash { get; set; }
            }
        }
    }
}
