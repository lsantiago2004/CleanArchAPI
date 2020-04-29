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
    public class HookResponse
    {
        #region Properties
        [JsonProperty("id", Order = 10)]
        public string Id { get; set; }

        [JsonProperty("scope", Order = 15)]
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

        [JsonProperty("destination", Order = 20)]
        public string Destination { get; set; }

        [JsonProperty("created_date", Order = 25)]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("updated_date", Order = 30)]
        public DateTimeOffset? UpdatedDate { get; set; }

        [JsonProperty("is_active", Order = 35)]
        public bool IsActive { get; set; }
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
