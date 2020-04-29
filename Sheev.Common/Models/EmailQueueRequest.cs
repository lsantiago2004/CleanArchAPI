using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class EmailQueueRequest
    {
        #region Properties
        public Guid DV_CompanyId { get; set; }
        public long EX_EmailTemplateId { get; set; }
        public string ST_TableName { get; set; }
        public long? InternalId { get; set; }
        // Serialize an object into this class if Jar Jar needs access to another database
        public string JsonClassObject { get; set; }
        public int AttemptCount { get; set; }
        #endregion
    }
}
