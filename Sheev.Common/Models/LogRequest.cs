using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class LogRequest
    {
        #region Properties
        [JsonProperty("application_id", Order = 5)]
        public int ApplicationId { get; set; }

        [JsonProperty("event_type", Order = 10)]
        public string EventType { get; set; }

        [JsonProperty("location", Order = 15)]
        public string Location { get; set; }

        [JsonProperty("message", Order = 20)]
        public string Message { get; set; }

        [JsonProperty("scope", Order = 25)]
        public string Scope { get; set; }

        [JsonProperty("reference_id", Order = 30)]
        public string ReferenceId { get; set; }
        [JsonProperty("timestamp", Order = 30)]
        public string Timestamp { get; set; }
        #endregion

        #region Constructor(s)
        public LogRequest()
        {
            ;
        }

        public LogRequest(int applicationId, string eventType, string location, string message, string scope, string referenceId)
        {
            this.ApplicationId = applicationId;
            this.EventType = eventType;
            this.Location = location;
            this.Message = message;
            this.Scope = scope;
            this.ReferenceId = referenceId;
        }
        #endregion
    }
}
