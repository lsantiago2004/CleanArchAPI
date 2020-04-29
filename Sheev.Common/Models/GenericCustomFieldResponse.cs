using Newtonsoft.Json;

namespace Sheev.Common.Models
{
    public class GenericCustomFieldResponse
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public long Id { get; set; }

        [JsonProperty("custom_field_name", Order = 15)]
        public string CustomFieldName { get; set; }

        [JsonProperty("value", Order = 20)]
        public string Value { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
