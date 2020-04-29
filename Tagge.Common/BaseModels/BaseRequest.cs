using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.BaseModels
{
    public class BaseRequest
    {
        //[JsonProperty("id", Order = 1)]
        //public string Id { get; set; }

        [JsonProperty("tracking_guid", Order = 5)]
        public Guid? TrackingGuid { get; set; }

        [JsonProperty("custom_fields", Order = 980)]
        public List<Models.GenericCustomFieldRequest> CustomFields { get; set; }

        [JsonProperty("external_ids", Order = 999)]
        public List<Models.GenericExternalIdRequest> ExternalIds { get; set; }

        //In general, all primary id fields in DockingBay are just the Id field.
        //public virtual void SetPrimaryId(object primaryId)
        //{
        //    Id = Convert.ToString(primaryId);
        //}
    }
}
