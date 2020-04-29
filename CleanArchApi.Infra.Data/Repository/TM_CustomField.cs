using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tagge.Models
{
    public class TM_CustomField
    {
        #region Properties
        public long Id { get; set; }
        public long ST_SystemId { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        #endregion
    }
}
