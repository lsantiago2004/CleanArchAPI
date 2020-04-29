using Newtonsoft.Json;
using System;

namespace LT319.Common.Models
{
    public class DetailRequest
    {
        [JsonProperty("tracking_guid", Order = 10)]
        public Guid TrackingGuid { get; set; }

        [JsonProperty("application_id", Order = 15)]
        public int ST_ApplicationId { get; set; }

        [JsonProperty("activity", Order = 20)]
        public string Activity { get; set; }

        [JsonProperty("detail_status", Order = 25)]
        public string DetailStatus { get; set; }

        [JsonProperty("mapping_collection_type_id", Order = 30)]
        public int? MappingCollectionTypeId { get; set; }

        [JsonProperty("activity_timestamp", Order = 35)]
        public DateTimeOffset ActivityTimestamp { get; set; }

        [JsonProperty("details", Order = 40)]
        public string Details { get; set; }
    }
}
