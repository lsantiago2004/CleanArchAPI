using Newtonsoft.Json;
using System;

namespace LT319.Common.Models
{
    public class HeaderRequest
    {
        [JsonProperty("tracking_guid", Order = 1)]
        public Guid TrackingGuid { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("direction", Order = 15)]
        public int? Direction { get; set; }

        [JsonProperty("system_id", Order = 20)]
        public long? SystemId { get; set; }

        [JsonProperty("mapping_collection_type", Order = 25)]
        public int? MappingCollectionType { get; set; }

        [JsonProperty("internal_id", Order = 30)]
        public long? InternalId { get; set; }

        [JsonProperty("external_id", Order = 35)]
        public string ExternalId { get; set; }

        [JsonProperty("start_timestamp", Order = 40)]
        public DateTimeOffset? StartTimestamp { get; set; }

        [JsonProperty("status", Order = 45)]
        public string Status { get; set; }
    }
}
