using Newtonsoft.Json;
using System;


namespace Sheev.Common.Models
{
    public class HelloWorldResponse
    {
        [JsonProperty("welcome_message", Order = 5)]
        public string WelcomeMessage { get; set; }

        [JsonProperty("version", Order = 10)]
        public string Version { get; set; }

        [JsonProperty("build", Order = 15)]
        public string Build { get; set; }

        //[JsonProperty("db_message")]
        //public string DBMessage;

        //[JsonProperty("db_version")]
        //public string DBVersion;
    }
}
