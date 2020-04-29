using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.Models
{
    public class ProductRelatedResponse : BaseModels.BaseResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("product_id", Order = 10)]
        public long ProductId { get; set; }

        [JsonProperty("product_sku", Order = 15)]
        public string ProductSku { get; set; }

        [JsonProperty("related_sku", Order = 20)]
        public string RelatedSku { get; set; }

        [JsonProperty("related_product_id", Order = 25)]
        public long RelatedProductId { get; set; }
        
        //not in user story, but needed
        [JsonProperty("related_type_id", Order = 30)]
        public long RelatedTypeId { get; set; }
    }
}
