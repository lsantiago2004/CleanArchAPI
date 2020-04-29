using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace K2SO.Data.Models
{
    public class RoleRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("tracking_guid", Order = 16)]
        public Guid? TrackingGuid { get; set; }

        [JsonProperty("permissions", Order = 20)]
        public List<PermissionRequest> Permissions { get; set; }

        [JsonProperty("users", Order = 25)]
        public List<UserRequest> Users { get; set; }
        #endregion
    }
}
