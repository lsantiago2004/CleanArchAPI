using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class PasswordResetRequest
    {
        [JsonProperty("email_address", Order = 10, Required = Required.Always)]
        public string EmailAddress { get; set; }

        [JsonProperty("password", Order = 15, Required = Required.Always)]
        public string Password { get; set; }

        [JsonProperty("code", Order = 20, Required = Required.Always)]
        public string Code { get; set; }

        [JsonProperty("tracking_guid", Order = 25)]
        public Guid? TrackingGuid { get; set; }
    }
}
