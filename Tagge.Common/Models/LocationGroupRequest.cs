using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tagge.Common.Models
{
    public class LocationGroupRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("locations", Order = 25)]
        public List<Sheev.Common.Models.GenericRequest> Locations { get; set; }

        #endregion
    }
}
