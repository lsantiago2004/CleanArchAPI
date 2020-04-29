using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class GenericSetting
    {
        public string Product_Name { get; set; }
        public string IPaaS_Name { get; set; }
        public string UseWebhook { get; set; }
        public string DetailLogging { get; set; }
        public string SecretWebhook { get; set; }
        public string R2D2_Endpoint { get; set; }
        public string APILoggerLogLevel { get; set; }
        public Sheev.Common.Utilities.Constants.ST_Application ApplicationId { get; set; }
        public int HealthCheckIntervalInMinutes { get; set; }
        public int IntervalInMilliseconds { get; set; }
    }
}
