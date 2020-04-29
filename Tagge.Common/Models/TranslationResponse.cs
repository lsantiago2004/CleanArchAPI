using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class TranslationResponse
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public long Id { get; set; }

        [JsonProperty("translation_collection_id", Order = 15)]
        public long TranslationCollectionId { get; set; }

        //[JsonProperty("mapping_type_id")]
        //public int MappingTypeId { get; set; }

        [JsonProperty("destination_value", Order = 20)]
        public string DestinationValue { get; set; }

        [JsonProperty("source_value", Order = 25)]
        public string SourceValue { get; set; }

        [JsonProperty("source_mapping_type_id", Order = 30)]
        public int SourceMappingTypeId { get; set; }

        [JsonProperty("sequence_number", Order = 35)]
        public int? SequenceNumber { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        #endregion
    }
}
