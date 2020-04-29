using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class KitRequest : BaseModels.BaseRequest
    {
        [JsonProperty("sku", Order = 10, Required = Required.Always)]
        public string Sku { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("type", Order = 20, Required = Required.Always)]
        public string Type { get; set; } //The possible values for this are: Bill of Material, Basic, Add-on

        [JsonProperty("components", Order = 25)]
        public List<KitComponentRequest> Components { get; set; }
    }
}
