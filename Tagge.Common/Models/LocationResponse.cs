using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tagge.Common.Models
{
    public class LocationResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 15)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 20)]
        public string Description { get; set; }

        [JsonProperty("location_groups", Order = 25)]
        public List<Sheev.Common.Models.GenericResponse> LocationGroups { get; set; }
        #endregion

        #region Method(s)
        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
        #endregion
    }
}
