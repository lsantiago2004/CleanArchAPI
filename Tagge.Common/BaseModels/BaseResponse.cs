using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.BaseModels
{
    public class BaseResponse
    {
        [JsonProperty("tracking_guid", Order = 5)]
        public Guid? TrackingGuid { get; set; }

        [JsonProperty("custom_fields", Order = 980)]
        public List<Models.GenericCustomFieldResponse> CustomFields { get; set; }

        [JsonProperty("external_ids", Order = 999)]
        public List<Models.ExternalIdResponse> ExternalIds { get; set; }

        public virtual object GetPrimaryId()
        {
            throw new NotImplementedException("GetPrimaryId was not implemented for this Response method");
        }
    }
}
