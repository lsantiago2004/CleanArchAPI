using Newtonsoft.Json;

namespace Sheev.Common.Models
{
    public class GenericResponse
    {
        #region Properties
        [JsonProperty("id", Order = 5)]
        public string Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }
        #endregion
    }
}
