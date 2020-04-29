using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace K2SO.Data.Models
{
    public class RoleResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("tracking_guid", Order = 16)]
        public Guid? TrackingGuid { get; set; }

        [JsonProperty("permissions", Order = 20)]
        public List<PermissionResponse> Permissions { get; set; }

        [JsonProperty("users", Order = 25)]
        public List<UserResponse> Users { get; set; }
        #endregion

        #region Constructor(s)
        public RoleResponse()
        {
            Permissions = new List<PermissionResponse>();
            Users = new List<UserResponse>();
        }
        #endregion
    }
}
