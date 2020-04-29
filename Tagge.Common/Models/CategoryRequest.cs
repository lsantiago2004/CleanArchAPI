using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tagge.Common.Models
{
    public class CategoryRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("parent_id", Order = 20)]
        public long? ParentId { get; set; }

        [JsonProperty("category_sets", Order = 25)]
        public List<Sheev.Common.Models.GenericRequest> CategorySets { get; set; }
        #endregion
    }
}
