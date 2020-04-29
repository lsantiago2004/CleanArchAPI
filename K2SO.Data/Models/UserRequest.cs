using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace K2SO.Data.Models
{
    public class UserRequest
    {
        #region Properties
        [JsonProperty("first_name", Order = 10, Required = Required.Always)]
        public string FirstName { get; set; }

        [JsonProperty("last_name", Order = 15, Required = Required.Always)]
        public string LastName { get; set; }

        [JsonProperty("email_address", Order = 20, Required = Required.Always)]
        public string EmailAddress { get; set; }

        [JsonProperty("base64_image", Order = 23)]
        public string Base64Image { get; set; }

        [JsonProperty("tracking_guid", Order = 25)]
        public Guid? TrackingGuid { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
