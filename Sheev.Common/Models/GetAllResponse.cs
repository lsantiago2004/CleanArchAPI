using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class GetAllResponse
    {
        #region Properties
        [JsonProperty("id", Order = 5)]
        public long Id { get; set; }

        [JsonProperty("type", Order = 10)]
        public long Type { get; set; }

        [JsonProperty("name", Order = 15)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 20)]
        public string Description { get; set; }

        [JsonProperty("value", Order = 25)]
        public string Value { get; set; }

        [JsonProperty("attached_items_count", Order = 30)]
        public int AttachedItemsCount { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
