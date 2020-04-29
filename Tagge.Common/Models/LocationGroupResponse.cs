using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tagge.Common.Models
{
    public class LocationGroupResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("locations", Order = 25)]
        public List<Sheev.Common.Models.GenericResponse> Locations { get; set; } = new List<Sheev.Common.Models.GenericResponse>();
        #endregion

        #region Method(s)
        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
        #endregion
    }
}
