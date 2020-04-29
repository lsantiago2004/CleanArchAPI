using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_KitComponent
    {
        #region Properties
        public string PC_KitComponent_Id { get; set; }

        public string Sku { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal Quantity { get; set; }
        
        public string Unit { get; set; }
        
        public string Type { get; set; }
        
        public string CreatedDateTime { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string UpdatedDateTime { get; set; }
        
        public string UpdatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        public List<PC_CustomField> CustomFields { get; set; }
        #endregion

        #region Constructor(s)
        public PC_KitComponent()
        {
            CustomFields = new List<PC_CustomField>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.KitComponentResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.KitComponentResponse();

            // Properties
            response.Id = PC_KitComponent_Id;
            response.Sku = Sku;
            response.Quantity = Quantity;
            response.Unit = Unit;
            response.Type = Type;

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

        public Tagge.Common.Models.KitComponentResponse ConvertToResponse(string companyId, string tableName, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.KitComponentResponse();

            // Properties
            response.Id = PC_KitComponent_Id;
            response.Sku = Sku;
            response.Quantity = Quantity;
            response.Unit = Unit;
            response.Type = Type;

            // Custom Fields
            if (CustomFields != null)
            {
                response.CustomFields = new List<Tagge.Common.Models.GenericCustomFieldResponse>();
                foreach (var customField in CustomFields)
                {
                    response.CustomFields.Add(customField.ConvertToResponse());
                }
            }

            // Modify the tablename slightly
            tableName = tableName + "Component";

            // ExternalIds 
            response.ExternalIds = PC_ExternalId.ConvertToResponse(PC_KitComponent_Id, tableName, companyId, db);

            return response;
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.KitComponentRequest request)
        {
            // Properties
            Sku = request.Sku;
            Quantity = request.Quantity;
            Unit = request.Unit;
            Type = request.Type;

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        /// <summary>
        /// Sets the primary key for this object
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="unit"></param>
        public void SetPrimaryKey(string parentId, string unit)
        {
            PC_KitComponent_Id = $"{parentId}|{unit}";
        }
        #endregion
    }
}
