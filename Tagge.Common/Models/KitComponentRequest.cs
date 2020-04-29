using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class KitComponentRequest : BaseModels.BaseRequest
    {
        [JsonProperty("sku", Order = 10, Required = Required.Always)]
        public string Sku { get; set; }

        //[JsonProperty("parent_id", Order = 15)]
        //public string ParentId { get; set; }

        [JsonProperty("quantity", Order = 20)]
        public decimal Quantity { get; set; }

        [JsonProperty("unit", Order = 25)]
        public string Unit { get; set; } //This is an optional field. If specified, there needs to be a matching entry in PC_ProductUnit (specific to the product)

        [JsonProperty("type", Order = 30)]
        public string Type { get; set; } //This is optional. This comes from TM_FieldEntry: the two options are By Parent Qty or Fixed Qty. By Parent Qty should be the default
    }
}
