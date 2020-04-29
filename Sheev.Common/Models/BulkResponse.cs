using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheev.Common.Models
{
    public class BulkResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; } = new List<Item>();
    }

    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("failed_reason")]
        public string FailedReason { get; set; }
    }
}
