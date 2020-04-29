using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class ProductAlternateIdResponse : BaseModels.BaseResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("sku", Order = 10)]
        public string Sku { get; set; }

        [JsonProperty("unit", Order = 15)]
        public string Unit { get; set; }

        [JsonProperty("alternate_id_type_id", Order = 20)]
        public long? AlternateIdTypeId { get; set; }

        [JsonProperty("alternate_id", Order = 25)]
        public string AlternateId { get; set; }

        [JsonProperty("description", Order = 30)]
        public string Description { get; set; }
    }
}
