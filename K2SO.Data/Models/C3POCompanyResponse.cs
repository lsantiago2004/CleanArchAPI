using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace K2SO.Data.Models
{
    public class C3POCompanyResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public Guid Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("token", Order = 15)]
        public string Token { get; set; }

        [JsonProperty("systems", Order = 20)]
        public List<Sheev.Common.Models.GetAllResponse> Systems { get; set; }
        #endregion
    }
}
