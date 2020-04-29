using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    /// <summary>
    /// This request is same as ExternalIdRequest but without the Table Name
    /// </summary>
    public class GenericExternalIdRequest
    {
        #region Properties

        [JsonProperty("system_id", Order = 15)]
        public long SystemId { get; set; }

        [JsonProperty("external_id", Order = 20)]
        public string ExternalId { get; set; }

        [JsonProperty("internal_id", Order = 25)]
        public long? InternalId { get; set; }

        [JsonProperty("tracking_guid", Order = 30)]
        public Guid? TrackingGuid { get; set; }
        #endregion
    }
}
