using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class CategorySetRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("name", Order = 10, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("categories", Order = 25)]
        public List<Sheev.Common.Models.GenericRequest> Categories { get; set; }
        #endregion
    }
}
