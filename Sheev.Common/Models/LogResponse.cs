using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class LogResponse
    {
        #region Properties
        [JsonProperty("id", Order = 5)]
        public Guid Id { get; set; }

        [JsonProperty("application_id", Order = 10)]
        public int ApplicationId { get; set; }

        [JsonProperty("timestamp", Order = 15)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("event_type", Order = 20)]
        public string EventType { get; set; }

        [JsonProperty("location", Order = 25)]
        public string Location { get; set; }

        [JsonProperty("message", Order = 30)]
        public string Message { get; set; }

        [JsonProperty("scope", Order = 35)]
        public string Scope { get; set; }

        [JsonProperty("reference_id", Order = 40)]
        public string ReferenceId { get; set; }
        #endregion

        #region Constructor(s)
        #endregion
    }
}
