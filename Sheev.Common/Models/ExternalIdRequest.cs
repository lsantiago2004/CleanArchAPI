﻿using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class ExternalIdRequest
    {
        #region Properties
        [JsonProperty("table_name", Order = 10, Required = Required.Always)]
        public string TableName { get; set; }

        [JsonProperty("system_id", Order = 15, Required = Required.Always)]
        public long SystemId { get; set; }

        [JsonProperty("external_id", Order = 20, Required = Required.Always)]
        public string ExternalId { get; set; }

        [JsonProperty("internal_id", Order = 25, Required = Required.Always)]
        public long? InternalId { get; set; }

        [JsonProperty("tracking_guid", Order = 30)]
        public Guid? TrackingGuid { get; set; }
        #endregion
    }
}
