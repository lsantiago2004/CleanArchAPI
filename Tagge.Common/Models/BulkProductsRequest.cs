using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.Models
{
    public class BulkProductsRequest
    {
        [JsonProperty("products_list", Order = 10)]
        public List<ProductRequest> ProductsList { get; set; }
    }
}
