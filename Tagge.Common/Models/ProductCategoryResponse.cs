using Newtonsoft.Json;

namespace Tagge.Common.Models
{
    public class ProductCategoryResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("product_id", Order = 10)]
        public long ProductId { get; set; }

        [JsonProperty("category_id", Order = 15)]
        public long CategoryId { get; set; }

        [JsonProperty("type", Order = 20)]
        public string Type { get; set; }
    }
}
