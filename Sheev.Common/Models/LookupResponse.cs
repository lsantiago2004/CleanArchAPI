using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sheev.Common.Models
{
    public class LookupResponse
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 15)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 20)]
        public string Description { get; set; }

        [JsonProperty("mapping_collection_type_id", Order = 25)]
        public int? TM_MappingCollectionTypeId { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}