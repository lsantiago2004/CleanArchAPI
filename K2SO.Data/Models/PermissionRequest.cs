using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace K2SO.Data.Models
{
    public class PermissionRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [Required]
        [JsonProperty("access_type_id")]
        public int AccessTypeId { get; set; }

        [JsonProperty("tracking_guid", Order = 30)]
        public Guid? TrackingGuid { get; set; }
        #endregion
    }
}
