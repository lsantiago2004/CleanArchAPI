using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_ProductCategory
    {
        #region Properties
        public string PC_ProductCategory_Id { get; set; }
        public string CategoryName { get; set; }

        public long CategoryId { get; set; }

        public bool IsActive { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public Tagge.Common.Models.CategoryResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.CategoryResponse();

            //// Properties
            //response.Sku = Sku;
            //response.Unit = Unit;
            //response.AlternateIdType = AlternateIdType;
            //response.AlternateId = AlternateId;
            //response.Description = Description;

            //// Custom Fields
            //if (CustomFields != null)
            //{
            //    response.CustomFields = new List<Tagge.Common.Models.GenericCustomFieldResponse>();
            //    foreach (var customField in CustomFields)
            //    {
            //        response.CustomFields.Add(customField.ConvertToResponse());
            //    }
            //}

            //// ExternalIds
            //if (ExternalIds != null)
            //{
            //    response.ExternalIds = new List<Sheev.Common.Models.GenericExternalIdResponse>();
            //    foreach (var externalId in ExternalIds)
            //    {
            //        response.ExternalIds.Add(externalId.ConvertToResponse());
            //    }
            //}

            return response;
        }

        public Tagge.Common.Models.CategoryAssignmentResponse ConvertToGenericResponse()
        {
            var response = new Tagge.Common.Models.CategoryAssignmentResponse();

            // Properties
            response.Id = PC_ProductCategory_Id;
            response.CategoryId = CategoryId;
            response.CategoryName = CategoryName;

            return response;
        }

        public void ConvertToGenericDatabaseObject(Sheev.Common.Models.GenericRequest request)
        {

            // Properties
            CategoryId = Convert.ToInt64(request.Id);
            CategoryName = request.Name;
        }

        public void ConvertToDatabaseObject(string categoryName, Tagge.Common.Models.ProductCategoryRequest request)
        {

            // Properties
            CategoryId = Convert.ToInt64(request.CategoryId);
            CategoryName = categoryName;
        }

        /// <summary>
        /// Sets the Primary Key
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="unitName"></param>
        public void SetPrimaryKey(string parentId, string categoryId)
        {
            PC_ProductCategory_Id = $"{parentId}|{categoryId}";
        }
        #endregion
    }
}
