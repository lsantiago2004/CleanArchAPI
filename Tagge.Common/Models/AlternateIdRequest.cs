using Newtonsoft.Json;

namespace Tagge.Common.Models
{
    public class AlternateIdRequest : BaseModels.BaseRequest
    {
        [JsonProperty("sku", Order = 10, Required = Required.Always)]
        public string Sku { get; set; }

        [JsonProperty("unit", Order = 15)]
        public string Unit { get; set; } //This is an optional field. If specified, there needs to be a matching entry in PC_ProductUnit (specific to the product)

        [JsonProperty("alternate_id_type_id", Order = 20, Required = Required.Always)]
        public long AlternateIdTypeId { get; set; } 

        [JsonProperty("alternate_id", Order = 25)]
        public string AlternateId { get; set; }

        [JsonProperty("description", Order = 30)]
        public string Description { get; set; }
    }
}
