using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheev.Common.Models
{
    public class CustomFieldResponse
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 15)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 20)]
        public string Description { get; set; }

        [JsonProperty("table_name", Order = 25)]
        public string TableName { get; set; }

        [JsonProperty("table_id", Order = 30)]
        public long TableId { get; set; }

        [JsonProperty("system_id", Order = 35)]
        public long SystemId { get; set; }

        [JsonProperty("system_type_id", Order = 40)]
        public int SystemTypeId { get; set; }

        [JsonProperty("system_name", Order = 45)]
        public string SystemName { get; set; }
        #endregion
    }
}
