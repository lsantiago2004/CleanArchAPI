using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class GenericCustomFieldRequest
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public long Id { get; set; }

        [JsonProperty("type", Order = 15)]
        public string Type { get; set; }

        [JsonProperty("custom_field_name", Order = 20, Required = Required.Always)]
        public string CustomFieldName { get; set; }

        [JsonProperty("value", Order = 25)]
        public string Value { get; set; }

        [JsonProperty("tracking_guid", Order = 30)]
        public Guid? TrackingGuid { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
