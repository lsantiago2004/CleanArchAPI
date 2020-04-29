using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class OptionResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("option_name", Order = 10)]
        public string OptionName { get; set; }

        [JsonProperty("type", Order = 15)]
        public string Type { get; set; }

        [JsonProperty("order", Order = 20)]
        public int Order { get; set; }

        [JsonProperty("values", Order = 25)]
        public List<OptionValueResponse> Values { get; set; }
        #endregion

        //public override object GetPrimaryId()
        //{
        //    return Convert.ToString(OptionId);
        //}

    }
}
