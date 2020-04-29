using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sheev.Common.Models
{
    public class HookRequest
    {
        #region Properties
        [JsonProperty("scope", Order = 10)]
        public string Scope { get; set; }

        //An ENUM version of the scope field.
        [JsonIgnore]
        public Utilities.Constants.WH_Scope ScopeEnum
        {
            get
            {
                return ToWebhookScopeValue(Scope);
            }
            set
            {
                Scope = ToWebhookScopeString(value);
            }
        }

        [JsonProperty("destination", Order = 15)]
        public string Destination { get; set; }

        [JsonProperty("headers", Order = 20)]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("authorization_type", Order = 25)]
        public string AuthorizationType { get; set; }

        [JsonProperty("authorization_token", Order = 30)]
        public string AuthorizationToken { get; set; }

        [JsonProperty("is_active", Order = 35)]
        public bool IsActive { get; set; }

        [JsonProperty("tracking_guid", Order = 40)]
        public Guid? TrackingGuid { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public static Utilities.Constants.WH_Scope ToWebhookScopeValue(string value)
        {
            foreach (FieldInfo fi in typeof(Utilities.Constants.WH_Scope).GetFields())
            {
                if (!fi.IsStatic)
                {
                    continue;
                }

                object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs == null || attrs.Length <= 0)
                {
                    continue;
                }

                DescriptionAttribute descr = (DescriptionAttribute)attrs[0];
                if (0 == string.Compare(value, descr.Description))
                {
                    return (Utilities.Constants.WH_Scope)fi.GetValue(null);
                }
            }

            return Utilities.Constants.WH_Scope.UNKNOWN;
        }

        public static string ToWebhookScopeString(Utilities.Constants.WH_Scope value)
        {
            foreach (FieldInfo fi in typeof(Utilities.Constants.WH_Scope).GetFields())
            {
                if (!fi.IsStatic || (Utilities.Constants.WH_Scope)fi.GetValue(null) != value)
                {
                    continue;
                }

                object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs == null || attrs.Length <= 0)
                {
                    continue;
                }

                DescriptionAttribute descr = (DescriptionAttribute)attrs[0];
                return descr.Description;
            }

            return string.Empty;
        }
        #endregion
    }
}
