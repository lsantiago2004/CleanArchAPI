﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tagge.Common.Models
{
    public class GenericCustomFieldResponse
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public string Id { get; set; }

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
