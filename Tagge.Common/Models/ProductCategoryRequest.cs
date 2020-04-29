using Newtonsoft.Json;
using System;

namespace Tagge.Common.Models
{
    public class ProductCategoryRequest
    {
        [JsonProperty("category_id", Order = 10, Required = Required.Always)]
        public long CategoryId { get; set; }

        [JsonProperty("internal_id", Order = 20, Required = Required.Always)]
        public long InternalId { get; set; }
    }
}
