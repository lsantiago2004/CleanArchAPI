using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class OptionRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("option_name", Order = 10, Required = Required.Always)]
        public string OptionName { get; set; }

        [JsonProperty("type", Order = 15)]
        public string Type { get; set; }

        [JsonProperty("order", Order = 20)]
        public int Order { get; set; }

        [JsonProperty("values", Order = 25)]
        public List<OptionValueRequest> Values { get; set; }
        #endregion
    }
}
