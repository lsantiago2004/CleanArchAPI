using Newtonsoft.Json;
using System;

namespace LT319.Common.Models
{
    public class DetailResponse
    {
        [JsonProperty("application", Order = 10)]
        public string Application { get; set; }

        [JsonProperty("activity", Order = 15)]
        public string Activity { get; set; }

        [JsonProperty("status", Order = 20)]
        public string Status { get; set; }

        [JsonProperty("mapping_collection_type_id", Order = 25)]
        public int? MappingCollectionTypeId { get; set; }

        [JsonProperty("activity_timestamp", Order = 30)]
        public DateTimeOffset ActivityTimestamp { get; set; }

        [JsonProperty("details", Order = 35)]
        public string Details { get; set; }
    }
}
