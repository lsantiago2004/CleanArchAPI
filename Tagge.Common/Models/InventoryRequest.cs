using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class InventoryRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long? Id { get; set; }

        [JsonProperty("location_id", Order = 10, Required = Required.Always)]
        public long LocationId { get; set; }

        [JsonProperty("qty_on_hand", Order = 15)]
        public decimal? QtyOnHand { get; set; }

        [JsonProperty("qty_available", Order = 20)]
        public decimal? QtyAvailable { get; set; }

        [JsonProperty("cost", Order = 25)]
        public decimal? Cost { get; set; }
        #endregion
    }
}
