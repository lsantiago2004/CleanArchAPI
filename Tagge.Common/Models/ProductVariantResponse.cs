using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class ProductVariantResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("sku", Order = 10)]
        public string Sku { get; set; }

        [JsonProperty("default_price", Order = 15)]
        public decimal? DefaultPrice { get; set; }

        [JsonProperty("msrp", Order = 20)]
        public decimal? MSRP { get; set; }

        [JsonProperty("sale_price", Order = 25)]
        public decimal? SalePrice { get; set; }

        [JsonProperty("cost", Order = 30)]
        public decimal? Cost { get; set; }

        [JsonProperty("qty_on_hand", Order = 35)]
        public decimal? QtyOnHand { get; set; }

        [JsonProperty("qty_available", Order = 40)]
        public decimal? QtyAvailable { get; set; }

        [JsonProperty("barcode", Order = 45)]
        public string Barcode { get; set; }

        [JsonProperty("status", Order = 50)]
        public string Status { get; set; }

        [JsonProperty("width", Order = 55)]
        public decimal? Width { get; set; }

        [JsonProperty("height", Order = 60)]
        public decimal? Height { get; set; }

        [JsonProperty("depth", Order = 65)]
        public decimal? Depth { get; set; }

        [JsonProperty("weight", Order = 70)]
        public decimal? Weight { get; set; }

        [JsonProperty("kit", Order = 73)]
        public KitResponse Kit { get; set; }

        [JsonProperty("inventory", Order = 75)]
        public List<InventoryResponse> Inventory { get; set; }

        [JsonProperty("categories", Order = 76)]
        public List<CategoryAssignmentResponse> Categories { get; set; }

        [JsonProperty("options", Order = 80)]
        public List<OptionValueResponse> Options { get; set; }

        [JsonProperty("alternate_ids", Order = 85)]
        public List<ProductAlternateIdResponse> AlternateIds { get; set; }
        #endregion

        public override object GetPrimaryId()
        {
            string retVal;
            var DIM_1 = Options.Find(x => x.Order == 1);
            if (DIM_1 == null)
                return null;
            retVal = DIM_1.Value;

            var DIM_2 = Options.Find(x => x.Order == 2);
            if (DIM_2 == null)
                return retVal;
            retVal += "|" + DIM_2.Value;

            var DIM_3 = Options.Find(x => x.Order == 3);
            if (DIM_3 == null)
                return retVal;
            retVal += "|" + DIM_3.Value;

            return retVal;
        }
    }
}
