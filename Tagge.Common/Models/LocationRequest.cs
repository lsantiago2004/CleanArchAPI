using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tagge.Common.Models
{
    public class LocationRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("location_groups", Order = 25)]
        public List<Sheev.Common.Models.GenericRequest> LocationGroups { get; set; }
        #endregion
    }
}
