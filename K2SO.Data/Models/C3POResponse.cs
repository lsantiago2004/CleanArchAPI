using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class C3POResponse
    {
        #region Properties
        [JsonProperty("run_type", Order = 10)]
        public long RunType { get; set; }

        [JsonProperty("companies", Order = 40)]
        public List<C3POCompanyResponse> Companies { get; set; }
        #endregion

        #region Constructor(s)
        public C3POResponse() { Companies = new List<C3POCompanyResponse>(); }
        #endregion
    }

    
}
