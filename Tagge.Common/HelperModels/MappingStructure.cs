using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.HelperModels
{
    public class MappingStructure
    {
        #region Properties
        [JsonProperty("id")]
        public int Id { get; set; }

        // Source Collection Name
        [JsonProperty("source_collection_name")]
        public string SourceCollectionName { get; set; }
        
        // Destination Collection Name
        [JsonProperty("destination_collection_name")]
        public string DestinationCollectionName { get; set; }

        [JsonProperty("mapping_table_name")]
        public string MappingTableName { get; set; }

        [JsonProperty("mapping_type_id")]
        public int MappingTypeId { get; set; }

        [JsonProperty("mapping_type_name")]
        public string MappingTypeName { get; set; }

        // Either Field or Custom
        [JsonProperty("destination_table")]
        public string DestinationTable { get; set; }

        [JsonProperty("destination_field")]
        public string DestinationField { get; set; }

        [JsonProperty("destination_field_id")]
        public int DestinationFieldId { get; set; }

        [JsonProperty("source_table")]
        public string SourceTable { get; set; }

        [JsonProperty("source_value_id")]
        public int? SourceValueId { get; set; }

        [JsonProperty("source_value")]
        public string SourceValue { get; set; }

        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        [JsonProperty("direction_id")]
        public int DirectionId { get; set; }

        [JsonProperty("lookup_translation_collection_id")]
        public int? LookupTranslationCollectionId { get; set; }

        [JsonProperty("lookup_translation_values")]
        public List<Models.TranslationResponse> LookupTranslation { get; set; }
        #endregion

        #region Constructor(s)
        public MappingStructure() { LookupTranslation = new List<Models.TranslationResponse>(); }
        #endregion

        #region Method(s)
        public string GetLookupDestinationValue(string SourceValue)
        {
            //Modified to allow case-insensitive checking, per TFS task 1268 
            var firstTranslationResponse = LookupTranslation.Find(x => x.SourceValue.ToUpper() == SourceValue.ToUpper());
            if (firstTranslationResponse == null)
            {
                //since we don't have support for default values in translation collections yet, we allow a * to indicate a default value.
                //  If we search for the SourceValue and it doesn't exist, we search for a * value and return that if it exists.
                firstTranslationResponse = LookupTranslation.Find(x => x.SourceValue == "*");
                if (firstTranslationResponse == null) //If we didn't find SourceValue or the * value, return null
                    return null;
            }
            return firstTranslationResponse.DestinationValue;
        }
        #endregion
    }
}
