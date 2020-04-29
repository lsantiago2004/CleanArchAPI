using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_AlternateId
    {
        #region Properties
        public string PC_AlternateId_Id { get; set; }
        
        public string Sku { get; set; }
        
        public string Unit { get; set; }
        
        public long? PC_AlternateIdTypeId { get; set; }
        
        public string AlternateId { get; set; }
        
        public string Description { get; set; }
        
        public string CreatedDateTime { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string UpdatedDateTime { get; set; }
        
        public string UpdatedBy { get; set; }

        [BsonRepresentation(BsonType.Boolean)]
        public bool IsActive { get; set; }
        
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_AlternateId()
        {
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.ProductAlternateIdResponse ConvertToAlternateIdResponse()
        {
            var response = new Tagge.Common.Models.ProductAlternateIdResponse();

            // Properties
            response.Id = PC_AlternateId_Id;
            response.Sku = Sku;
            response.Unit = Unit;
            response.AlternateIdTypeId = PC_AlternateIdTypeId;
            response.AlternateId = AlternateId;
            response.Description = Description;

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

        public Tagge.Common.Models.ProductAlternateIdResponse ConvertToResponse(string companyId, string tableName, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.ProductAlternateIdResponse();

            // Properties
            response.Id = PC_AlternateId_Id;
            response.Sku = Sku;
            response.Unit = Unit;
            response.AlternateIdTypeId = PC_AlternateIdTypeId;
            response.AlternateId = AlternateId;
            response.Description = Description;

            // Custom Fields
            if (CustomFields != null)
            {
                response.CustomFields = new List<Tagge.Common.Models.GenericCustomFieldResponse>();
                foreach (var customField in CustomFields)
                {
                    response.CustomFields.Add(customField.ConvertToResponse());
                }
            }

            // ExternalIds 
            response.ExternalIds = PC_ExternalId.ConvertToResponse(PC_AlternateId_Id, tableName, companyId, db);

            return response;
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.ProductAlternateIdRequest request)
        {
            // Properties
            //Sku = request.Sku;
            Unit = request.Unit;
            PC_AlternateIdTypeId = request.AlternateIdTypeId;
            AlternateId = request.AlternateId;
            Description = request.Description;

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.AlternateIdRequest request)
        {
            // Properties
            Sku = request.Sku;
            Unit = request.Unit;
            PC_AlternateIdTypeId = request.AlternateIdTypeId;
            AlternateId = request.AlternateId;
            Description = request.Description;

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        /// <summary>
        /// Set the primary key for this object
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="type"></param>
        /// <param name="sku"></param>
        public void SetPrimaryKey(string parentId, string type, string unit)
        {
            string baseUnit = string.Empty;
            
            // Check to see if the unit has a value if not ignore
            if (!string.IsNullOrEmpty(unit))
                baseUnit = $"|{unit}";

            PC_AlternateId_Id = $"{parentId}|{type}{baseUnit}";
        }
        #endregion
    }
}