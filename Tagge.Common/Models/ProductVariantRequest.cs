using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class ProductVariantRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long? Id { get; set; }

        [JsonProperty("sku", Order = 10, Required = Required.Always)]
        public string Sku { get; set; }

        [JsonProperty("default_price", Order = 15)]
        public decimal? DefaultPrice { get; set; }

        [JsonProperty("msrp", Order = 20)]
        public decimal? MSRP { get; set; }

        [JsonProperty("sale_price", Order = 25)]
        public decimal? SalePrice { get; set; }

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
        public KitRequest Kit { get; set; }

        [JsonProperty("inventory", Order = 75)]
        public List<InventoryRequest> Inventory { get; set; }

        [JsonProperty("categories", Order = 76)]
        public List<Sheev.Common.Models.GenericRequest> Categories { get; set; }

        [JsonProperty("options", Order = 80)]
        public List<OptionValueRequest> Options { get; set; }

        [JsonProperty("alternate_ids", Order = 85)]
        public List<ProductAlternateIdRequest> AlternateIds { get; set; }
        #endregion
    }
}
