using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class ProductUnitResponse : BaseModels.BaseResponse
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty("conversion", Order = 15)]
        public decimal Conversion { get; set; }

        [JsonProperty("default_price", Order = 20)]
        public decimal DefaultPrice { get; set; }

        [JsonProperty("msrp", Order = 25)]
        public decimal MSRP { get; set; }

        [JsonProperty("sale_price", Order = 30)]
        public decimal SalePrice { get; set; }

        #region Method(s)
        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
        #endregion
    }
}
