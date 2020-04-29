using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class LoginResponse
    {
        #region Properties
        [JsonProperty("id", Order = 5)]
        public long? Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("email_address", Order = 15)]
        public string EmailAddress { get; set; }

        [JsonProperty("session_key", Order = 20)]
        public string SessionKey { get; set; }

        [JsonProperty("return_url", Order = 25)]
        public string ReturnUrl { get; set; }

        //[JsonProperty("message", Order = 30)]
        //public string Message { get; set; }

        //[JsonProperty("current_company", Order = 35)]
        //public string CurrentCompany { get; set; }

        //[JsonProperty("companies", Order = 40)]
        //public int Companies { get; set; }

        [JsonProperty("permissions", Order = 45)]
        public List<PermissionResponse> Permissions { get; set; }

        [JsonProperty("tracking_guid", Order = 50)]
        public Guid? TrackingGuid { get; set; }
        #endregion
    }
}
