using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class KitResponse : BaseModels.BaseResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("sku", Order = 10)]
        public string Sku { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("type", Order = 20)]
        public string Type { get; set; }

        [JsonProperty("components", Order = 25)]
        public List<KitComponentResponse> Components { get; set; } = new List<KitComponentResponse>();
    }
}
