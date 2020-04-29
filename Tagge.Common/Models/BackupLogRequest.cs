using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sheev.Common.Models;

namespace Tagge.Common.Models
{
    public class BackupLogRequest : LogRequest
    {
        [JsonProperty("company_id", Order = 10)]
        public Guid? CompanyId { get; set; }
    }
}
