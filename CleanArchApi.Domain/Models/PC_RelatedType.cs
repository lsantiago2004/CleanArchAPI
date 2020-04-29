using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_RelatedType
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public void ConvertToResponse()
        {
            throw new NotImplementedException();
        }

        public void ConvertToDatabaseObject(object request, Guid companyId)
        {
            // Properties
            throw new NotImplementedException();
        }
        #endregion
    }
}
