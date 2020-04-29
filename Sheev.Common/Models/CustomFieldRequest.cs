using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class CustomFieldRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("table_name", Order = 15)]
        public string TableName { get; set; }

        [JsonProperty("system_name", Order = 20)]
        public string SystemName { get; set; }

        [JsonProperty("description", Order = 25)]
        public string Description { get; set; }

        [JsonProperty("tracking_guid", Order = 30)]
        public Guid? TrackingGuid { get; set; }
        #endregion
    }
}
