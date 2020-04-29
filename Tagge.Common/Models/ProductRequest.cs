using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tagge.Common.Models
{
    public class ProductRequest : BaseModels.BaseRequest
    {
        #region Properties
        [JsonProperty("sku", Order = 10, Required = Required.Always)]
        public string Sku { get; set; }

        [JsonProperty("description", Order = 15)]
        public string Description { get; set; }

        [JsonProperty("name", Order = 20, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Order = 25, Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("tracking_method", Order = 30, Required = Required.Always)]
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

        [JsonProperty("status", Order = 60, Required = Required.Always)]
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

        [JsonProperty("kit", Order = 104)]
        public KitRequest Kit { get; set; }

        [JsonProperty("variants", Order = 105)]
        public List<ProductVariantRequest> Variants { get; set; }

        [JsonProperty("inventory", Order = 110)]
        public List<InventoryRequest> Inventory { get; set; }

        [JsonProperty("categories", Order = 115)]
        public List<Sheev.Common.Models.GenericRequest> Categories { get; set; }

        [JsonProperty("options", Order = 120)]
        public List<OptionRequest> Options { get; set; }

        [JsonProperty("units", Order = 125)]
        public List<ProductUnitRequest> Units { get; set; }

        [JsonProperty("alternate_ids", Order = 130)]
        public List<ProductAlternateIdRequest> AlternateIds { get; set; }
        #endregion
    }
}
