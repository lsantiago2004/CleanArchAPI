using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.Models
{
    public class LocationGroupLocationResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("location_group_id", Order = 10)]
        public long LocationGroupId { get; set; }

        [JsonProperty("location_id", Order = 15)]
        public long LocationId { get; set; }
        #endregion 
    }
}
