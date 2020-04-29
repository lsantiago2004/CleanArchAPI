using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class KitComponentResponse : BaseModels.BaseResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("sku", Order = 10)]
        public string Sku { get; set; }

        [JsonProperty("quantity", Order = 20)]
        public decimal Quantity { get; set; }

        [JsonProperty("unit", Order = 25)]
        public string Unit { get; set; }

        [JsonProperty("type", Order = 30)]
        public string Type { get; set; }
    }
}
