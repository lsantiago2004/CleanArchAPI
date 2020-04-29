using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class RoleUserRequest
    {
        #region Properties
        [JsonProperty("first_name", Order = 10)]
        public string FirstName { get; set; }

        [JsonProperty("last_name", Order = 15)]
        public string LastName { get; set; }

        [JsonProperty("email_address", Order = 20)]
        public string EmailAddress { get; set; }
        #endregion
    }
}
