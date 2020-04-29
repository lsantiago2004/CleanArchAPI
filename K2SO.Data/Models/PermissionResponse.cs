using Newtonsoft.Json;
using System;

namespace K2SO.Data.Models
{
    public class PermissionResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public int Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("access_type_id", Order = 20)]
        public int AccessTypeId { get; set; }

        [JsonProperty("access_type", Order = 25)]
        public string AccessType { get; set; }

        [JsonProperty("tracking_guid", Order = 30)]
        public Guid? TrackingGuid { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
