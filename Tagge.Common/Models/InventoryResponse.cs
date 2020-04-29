using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class InventoryResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("parent_id", Order = 10)]
        public long ParentId { get; set; }

        [JsonProperty("location_id", Order = 15)]
        public long LocationId { get; set; }

        [JsonProperty("qty_on_hand", Order = 20)]
        public decimal? QtyOnHand { get; set; }

        [JsonProperty("qty_available", Order = 25)]
        public decimal? QtyAvailable { get; set; }

        [JsonProperty("cost", Order = 30)]
        public decimal? Cost { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
        #endregion
    }
}
