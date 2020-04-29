using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_CategorySet
    {
        #region Properties
        public long Id { get; set; }
        public string DV_CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<TM_GenericEntry> Categories { get; set; }
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_CategorySet()
        {
            Categories = new List<TM_GenericEntry>();
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.CategorySetResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.CategorySetResponse();

            // Properties
            response.Id = Id;
            response.Name = Name;
            response.Description = Description;

            // Categories
            if (Categories != null)
            {
                response.Categories = new List<Sheev.Common.Models.GenericResponse>();
                foreach (var category in Categories)
                {
                    var singleSimpleResponse = new Sheev.Common.Models.GenericResponse()
                    {
                        Id = category.InternalId.ToString(),
                        Name = category.Name
                    };

                    response.Categories.Add(singleSimpleResponse);
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

        public void ConvertToDatabaseObject(string companyId, Tagge.Common.Models.CategorySetRequest request)
        {
            // Properties
            Name = request.Name;
            Description = request.Description;
            DV_CompanyId = companyId;

            // Categories - Managed in PC_Category

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }
        #endregion
    }
}
