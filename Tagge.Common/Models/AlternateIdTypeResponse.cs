﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class AlternateIdTypeResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }
        #endregion
    }
}
