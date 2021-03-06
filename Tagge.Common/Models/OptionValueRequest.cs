﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class OptionValueRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("option_id", Order = 10)]
        public string OptionId { get; set; }

        [JsonProperty("option_name", Order = 15)]
        public string OptionName { get; set; }

        [JsonProperty("value", Order = 20, Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty("detail", Order = 25)]
        public string Detail { get; set; }

        [JsonProperty("order", Order = 30)]
        public int Order { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
