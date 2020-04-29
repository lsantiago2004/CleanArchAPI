using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class CompanyResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public Guid Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }
        #endregion
    }
}
