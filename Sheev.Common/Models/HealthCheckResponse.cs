using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class HealthCheckResponse
    {
        public bool IsHealthy { get; set; }
        public string Database { get; set; }

        public Dictionary<string, string> Status { get; set; }

        public List<int> ErrorStatusCodes { get; set; }
    }
}
