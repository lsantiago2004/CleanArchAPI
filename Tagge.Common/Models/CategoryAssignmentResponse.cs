using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.Models
{
    public class CategoryAssignmentResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("category_id", Order = 15)]
        public long CategoryId { get; set; }

        [JsonProperty("category_name", Order = 20)]
        public string CategoryName { get; set; }
    }
}
