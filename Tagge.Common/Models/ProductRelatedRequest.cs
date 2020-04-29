using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.Models
{
    public class ProductRelatedRequest //: BaseModels.BaseRequest
    {
        [JsonProperty("id", Order = 1)]
        public long? Id { get; set; }

        [JsonProperty("product_id", Order = 10)]
        public long ProductId { get; set; }

        [JsonProperty("related_sku", Order = 15)]
        public string RelatedSku { get; set; }

        [JsonProperty("related_product_id", Order = 20)]
        public long RelatedProductId { get; set; }

        //not in user story, but needed (not sure if Type Name or Id). Using Id for now
        [JsonProperty("related_type_id", Order = 25)]
        public long RelatedTypeId { get; set; }


    }
}
