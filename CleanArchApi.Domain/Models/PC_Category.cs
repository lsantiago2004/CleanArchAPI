using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_Category
    {
        #region Properties
        public long Id { get; set; }
        public string DV_CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? ParentId { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<TM_GenericEntry> CategorySets { get; set; }
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_Category() 
        {
            CategorySets = new List<TM_GenericEntry>();
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.CategoryResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.CategoryResponse();

            // Properties
            response.Id = Id;
            response.Name = Name;
            response.Description = Description;
            response.ParentId = ParentId;

            // Category Sets
            if(CategorySets != null)
            {
                response.CategorySets = new List<Sheev.Common.Models.GenericResponse>();
                foreach(var categorySet in CategorySets)
                {
                    var singleSimpleResponse = new Sheev.Common.Models.GenericResponse()
                    {
                        Id = categorySet.InternalId.ToString(),
                        Name = categorySet.Name
                    };

                    response.CategorySets.Add(singleSimpleResponse);
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

        public void ConvertToDatabaseObject(string companyId, Tagge.Common.Models.CategoryRequest request)
        {
            // Properties
            Name = request.Name;
            Description = request.Description;
            ParentId = request.ParentId;
            DV_CompanyId = companyId;

            // Category Sets - Managed in PC_CategorySet

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }
        #endregion
    }
}
