using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class IG_Log
    {
        #region Properties
        public Guid Id { get; set; }
        public int ST_ApplicationId { get; set; }
        public string DV_CompanyId { get; set; }
        public string Timestamp { get; set; }
        public string EventType { get; set; }
        public string Location { get; set; }
        public string Message { get; set; }
        public string Scope { get; set; }
        public string ReferenceId { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public Sheev.Common.Models.LogResponse ConvertToResponse()
        {
            var response = new Sheev.Common.Models.LogResponse();

            // Properties
            response.Id = Id;
            response.ApplicationId = ST_ApplicationId;
            response.EventType = EventType;
            response.Location = Location;
            response.Message = Message;
            response.ReferenceId = ReferenceId;
            response.Scope = Scope;
            response.Timestamp = DateTime.Parse(Timestamp);

            return response;
        }

        public void ConvertToDatabaseObject(Sheev.Common.Models.LogRequest request, Guid companyId)
        {
            // Properties
            DV_CompanyId = companyId.ToString();
            ST_ApplicationId = request.ApplicationId;
            EventType = request.EventType;
            Location = request.Location;
            Message = request.Message;
            ReferenceId = request.ReferenceId;
            Scope = request.Scope;
            //Timestamp = request.Timestamp.HasValue ? request.Timestamp.Value.ToString("yyyy/MM/dd HH:mm:ss.fff zzz") : DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
        }
        #endregion
    }
}
