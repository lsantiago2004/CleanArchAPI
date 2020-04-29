using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class WebhookResponse
    {
        [JsonProperty("id", Order = 10)]
        public string Id { get; set; }

        [JsonProperty("companyId", Order = 15)]
        public string CompanyId { get; set; }

        [JsonProperty("destination", Order = 20)]
        public string Destination { get; set; }

        [JsonProperty("scope", Order = 25)]
        public string Scope { get; set; }

        [JsonProperty("type", Order = 30)]
        public string Type { get; set; }

        [JsonProperty("tracking_guid", Order = 35)]
        public Guid? TrackingGuid { get; set; }

        [JsonProperty("headers", Order = 40)]
        public Dictionary<string, string> Headers { get; set; }

        #region Constructor(s)
        #endregion
    }
}
