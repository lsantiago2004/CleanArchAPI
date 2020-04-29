using System;
using System.Collections.Generic;
using System.Text;

namespace IG2000.Data.Models
{
    public class IG_ErrorHistory
    {
        #region Properties
        public Guid Id { get; set; }
        public string DV_CompanyId { get; set; }
        public string CreatedDateTime { get; set; }
        public long EH_ErrorId { get; set; } // Reference Id
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Severity { get; set; }
        public long SourceSystemId { get; set; }
        public string Destination { get; set; }
        public string Scope { get; set; }
        public string ExternalId { get; set; }
        public int EH_StatusId { get; set; } //Dismissed, Succesful Retry, etc.
        public string ReferenceId { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public void ConvertToResponse()
        {
            throw new NotImplementedException();
        }

        public void ConvertToDatabaseObject(Sheev.Common.Models.LogRequest request, Guid companyId)
        {
            // Properties
            DV_CompanyId = companyId.ToString();
            CreatedDateTime = CreatedDateTime;
            EH_ErrorId = EH_ErrorId;
            Title = request.Location;
            Detail = request.Message;
            Severity = Severity;
            SourceSystemId = SourceSystemId;
            Destination = Destination;
            Scope = request.Scope;
            ExternalId = ExternalId;
            EH_StatusId = EH_StatusId;
            ReferenceId = request.ReferenceId;
        }
        #endregion
    }
}
