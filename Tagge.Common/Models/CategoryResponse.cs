using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sheev.Common;

namespace Tagge.Common.Models
{
    public class CategoryResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("parent_id", Order = 20)]
        public long? ParentId { get; set; }

        [JsonProperty("category_sets", Order = 25)]
        public List<Sheev.Common.Models.GenericResponse> CategorySets { get; set; }
        #endregion

        #region Constructor(s)
        public CategoryResponse() { ExternalIds = new List<Models.ExternalIdResponse>(); }
        #endregion

        #region Method(s)
        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
        #endregion
    }
}
