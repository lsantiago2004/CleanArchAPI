using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class TM_Type
    {
        #region Properties
        public long Id { get; set; }
        public string ST_TableName { get; set; }
        public string TM_FieldName { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        #endregion
    }
}
