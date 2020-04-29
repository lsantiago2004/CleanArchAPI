using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Sheev.Common.Models
{
    public class HealthCheckRequest
    {
        #region Public Fields

        /// <summary>
        /// Gets the name of the test.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 5, Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        [JsonProperty(PropertyName = "service_name", Order = 10, Required = Required.Always)]
        public Uri ServiceName { get; set; }

        /// <summary>
        /// Gets the service version.
        /// </summary>
        [JsonProperty(PropertyName = "partition", Order = 20, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid Partition { get; set; }

        /// <summary>
        /// Gets the name of the endpoint. If more than one endpoint is exposed, this will be required.
        /// </summary>
        [JsonProperty(PropertyName = "endpoint", Order = 25, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets the a number containing frequency the test call is made. Defaults to 60.
        /// </summary>
        [JsonProperty(PropertyName = "frequency", Order = 30, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TimeSpan Frequency { get; set; }

        /// <summary>
        /// Gets the suffix path and query parameters to call when conducting the test. Required.
        /// </summary>
        [JsonProperty(PropertyName = "suffix_path", Order = 40, Required = Required.Always)]
        public string SuffixPath { get; set; }

        /// <summary>
        /// Gets the HTTP verb to use when calling the URI. Defaults to GET.
        /// </summary>
        [JsonProperty(PropertyName = "method", Order = 50, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets the content to send in the body of the test request.
        /// </summary>
        [JsonProperty(PropertyName = "content", Order = 60, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Content { get; set; }

        /// <summary>
        /// Gets the content to send in the body of the test request.
        /// </summary>
        [JsonProperty(PropertyName = "media_type", Order = 65, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MediaType { get; set; }

        /// <summary>
        /// Gets the expected duration of the test call in milliseconds. Defaults to 200ms.
        /// </summary>
        [JsonProperty(PropertyName = "expected_duration", Order = 70, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TimeSpan ExpectedDuration { get; set; }

        /// <summary>
        /// Gets the maximum duration of the test call in milliseconds. The test call will be terminated after this interval as passed. Default is 5000 milliseconds.
        /// </summary>
        [JsonProperty(PropertyName = "maximum_duration", Order = 80, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TimeSpan MaximumDuration { get; set; }

        /// <summary>
        /// Gets the headers to add to the request.
        /// </summary>
        [JsonProperty(PropertyName = "headers", Order = 200, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets the headers to add to the request.
        /// </summary>
        [JsonProperty(PropertyName = "warning_status_codes", Order = 220, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<int> WarningStatusCodes { get; set; }

        /// <summary>
        /// Gets the headers to add to the request.
        /// </summary>
        [JsonProperty(PropertyName = "error_status_codes", Order = 230, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<int> ErrorStatusCodes { get; set; }

        /// <summary>
        /// Gets the expected duration of the test call in milliseconds. Defaults to 200ms.
        /// </summary>
        [JsonProperty(PropertyName = "last_attempt", Order = 300, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset LastAttempt { get; set; }

        /// <summary>
        /// Gets the expected duration of the test call in milliseconds. Defaults to 200ms.
        /// </summary>
        [JsonProperty(PropertyName = "failure_count", Order = 310, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long FailureCount { get; set; }

        /// <summary>
        /// Gets the expected duration of the test call in milliseconds. Defaults to 200ms.
        /// </summary>
        [JsonProperty(PropertyName = "result_code", Order = 320, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HttpStatusCode ResultCode { get; set; }

        /// <summary>
        /// Gets the expected duration of the test call in milliseconds. Defaults to 200ms.
        /// </summary>
        [JsonProperty(PropertyName = "duration", Order = 330, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Duration { get; set; }

        [JsonProperty(PropertyName = "status", Order = 340, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> Status { get; set; }

        /// <summary>
        /// Get the Replica Id 
        /// </summary>
        [JsonProperty(PropertyName = "replica_id", Order = 350, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReplicaId { get; set; }

        /// <summary>
        /// Gets the port 
        /// </summary>
        [JsonProperty(PropertyName = "port", Order = 360, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Port { get; set; }
        #endregion
    }
}
