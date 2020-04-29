using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class UserRoleResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("first_name", Order = 10)]
        public string FirstName { get; set; }

        [JsonProperty("last_name", Order = 15)]
        public string LastName { get; set; }

        [JsonProperty("email_address", Order = 20)]
        public string EmailAddress { get; set; }

        [JsonProperty("status", Order = 25)]
        public string Status { get; set; }

        [JsonProperty("base64_image", Order = 26)]
        public string Base64Image { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
