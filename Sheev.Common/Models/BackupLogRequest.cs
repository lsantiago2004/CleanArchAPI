using Newtonsoft.Json;
using System;

namespace Sheev.Common.Models
{
    public class BackupLogRequest : LogRequest
    {
        [JsonProperty("company_id", Order = 10)]
        public Guid? CompanyId { get; set; }
    }
}
