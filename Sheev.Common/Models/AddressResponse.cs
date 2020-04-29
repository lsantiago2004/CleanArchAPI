using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class AddressResponse : Sheev.Common.BaseModels.BaseResponse
    {
        #region Properties
        [JsonProperty("first_name", Order = 10)]
        public string FirstName { get; set; }

        [JsonProperty("last_name", Order = 15)]
        public string LastName { get; set; }

        [JsonProperty("type", Order = 20)]
        public string Type { get; set; }

        [JsonProperty("phone_number", Order = 25)]
        public string PhoneNumber { get; set; }

        [JsonProperty("company", Order = 30)]
        public string Company { get; set; }

        [JsonProperty("address_1", Order = 35)]
        public string Address1 { get; set; }

        [JsonProperty("address_2", Order = 40)]
        public string Address2 { get; set; }

        [JsonProperty("address_3", Order = 45)]
        public string Address3 { get; set; }

        [JsonProperty("city", Order = 50)]
        public string City { get; set; }

        [JsonProperty("region", Order = 55)]
        public string Region { get; set; }

        [JsonProperty("country", Order = 60)]
        public string Country { get; set; }

        [JsonProperty("postal_code", Order = 65)]
        public string PostalCode { get; set; }

        [JsonProperty("is_primary_billing", Order = 70)]
        public bool IsPrimaryBilling { get; set; }

        [JsonProperty("is_primary_shipping", Order = 75)]
        public bool IsPrimaryShipping { get; set; }

        [JsonProperty("customer_id", Order = 80)]
        public long CM_CustomerId { get; set; }
        #endregion

        public override object GetPrimaryId()
        {
            return Convert.ToString(Id);
        }
    }
}
