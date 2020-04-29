using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class ProductResponse : BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("id", Order = 1)]
        public long Id { get; set; }

        [JsonProperty("sku", Order = 10)]
        public string Sku { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("name", Order = 20)]
        public string Name { get; set; }

        [JsonProperty("type", Order = 25)]
        public string Type { get; set; }

        [JsonProperty("tracking_method", Order = 30)]
        public string TrackingMethod { get; set; }

        [JsonProperty("default_price", Order = 35)]
        public decimal? DefaultPrice { get; set; }

        [JsonProperty("msrp", Order = 40)]
        public decimal? MSRP { get; set; }

        [JsonProperty("sale_price", Order = 45)]
        public decimal? SalePrice { get; set; }

        [JsonProperty("tax_class", Order = 50)]
        public string TaxClass { get; set; }

        [JsonProperty("barcode", Order = 55)]
        public string Barcode { get; set; }

        [JsonProperty("status", Order = 60)]
        public string Status { get; set; }

        [JsonProperty("unit", Order = 65)]
        public string Unit { get; set; }

        [JsonProperty("allow_discounts", Order = 70)]
        public bool? AllowDiscounts { get; set; }

        [JsonProperty("allow_backorders", Order = 75)]
        public bool? AllowBackorders { get; set; }

        [JsonProperty("in_stock_threshold", Order = 80)]
        public decimal? InStockThreshold { get; set; }

        [JsonProperty("width", Order = 85)]
        public decimal? Width { get; set; }

        [JsonProperty("height", Order = 90)]
        public decimal? Height { get; set; }

        [JsonProperty("depth", Order = 95)]
        public decimal? Depth { get; set; }

        [JsonProperty("weight", Order = 100)]
        public decimal? Weight { get; set; }

        [JsonProperty("kit", Order = 103)]
        public KitResponse Kit { get; set; }

        [JsonProperty("variants", Order = 105)]
        public List<ProductVariantResponse> Variants { get; set; }

        [JsonProperty("inventory", Order = 110)]
        public List<InventoryResponse> Inventory { get; set; }

        [JsonProperty("categories", Order = 115)]
        public List<CategoryAssignmentResponse> Categories { get; set; }

        [JsonProperty("options", Order = 120)]
        public List<OptionResponse> Options { get; set; }

        [JsonProperty("units", Order = 125)]
        public List<ProductUnitResponse> Units { get; set; }

        [JsonProperty("alternate_ids", Order = 130)]
        public List<ProductAlternateIdResponse> AlternateIds { get; set; }
        #endregion

        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
    }
}
