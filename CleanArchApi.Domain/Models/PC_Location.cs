using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_Location
    {
        #region Properties
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DV_CompanyId { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<TM_GenericEntry> LocationGroups { get; set; }
        public List<PC_CustomField> CustomFields { get; set; }
        #endregion

        #region Constructor(s)
        public PC_Location()
        {
            LocationGroups = new List<TM_GenericEntry>();
            CustomFields = new List<PC_CustomField>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.LocationResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.LocationResponse();

            // Properties
            response.Id = Id;
            response.Name = Name;
            response.Description = Description;

            // Location Groups
            if(LocationGroups != null)
            {
                response.LocationGroups = new List<Sheev.Common.Models.GenericResponse>();
                foreach(var locationGroup in LocationGroups)
                {
                    response.LocationGroups.Add(locationGroup.ConvertToResponse());
                }
            }

            // Custom Fields
            if (CustomFields != null)
            {
                response.CustomFields = new List<Tagge.Common.Models.GenericCustomFieldResponse>();
                foreach (var customField in CustomFields)
                {
                    response.CustomFields.Add(customField.ConvertToResponse());
                }
            }

            // ExternalIds - Managed in PC_ExternalId

            return response;
        }

        public void ConvertToDatabaseObject(string companyId, Tagge.Common.Models.LocationRequest request)
        {
            // Properties
            DV_CompanyId = companyId;
            Name = request.Name;
            Description = request.Description;

            // Location Groups - Managed in PC_LocatonGroup

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }
        #endregion
    }
}
