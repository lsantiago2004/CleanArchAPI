using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Sheev.Common.Models;  

namespace Sheev.Common.BaseModels
{
    public class BaseRequest
    {
        [JsonProperty("id", Order = 1)]
        public long? Id { get; set; }

        [JsonProperty("tracking_guid", Order = 5)]
        public Guid? TrackingGuid { get; set; }

        [JsonProperty("custom_fields", Order = 980)]
        public List<GenericCustomFieldRequest> CustomFields { get; set; }

        [JsonProperty("external_ids", Order = 999)]
        public List<GenericExternalIdRequest> ExternalIds { get; set; }

        //In general, all primary id fields in DockingBay are just the Id field.
        public virtual void SetPrimaryId(object PrimaryId)
        {
            long Id_Value;
            if (long.TryParse(Convert.ToString(PrimaryId), out Id_Value))
                Id = Id_Value;
        }
    }
}
