using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class TM_GenericEntry
    {
        #region Properties
        public string InternalId { get; set; }
        public string Name { get; set; }
        #endregion

        #region Constructor(s)
        public TM_GenericEntry() { }
        #endregion

        #region Method(s)
        public Sheev.Common.Models.GenericResponse ConvertToResponse()
        {
            var response = new Sheev.Common.Models.GenericResponse();

            // Properties
            response.Id = InternalId;
            response.Name = Name;

            return response;
        }

        public void ConvertToDatabaseObject(Sheev.Common.Models.GenericRequest request)
        {
            // Properties
            InternalId = request.Id;
            Name = request.Name;
        }
        #endregion
    }
}
