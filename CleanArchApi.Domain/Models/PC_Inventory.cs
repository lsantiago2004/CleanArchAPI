using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_Inventory
    {
        #region Properties
        public long Id { get; set; }
        
        public string DV_CompanyId { get; set; }
        
        public string InternalId { get; set; }

        public string ST_TableName { get; set; }

        public long PC_LocationId { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? QtyOnHand { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? QtyAvailable { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? Cost { get; set; }
        
        public string CreatedDateTime { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string UpdatedDateTime { get; set; }
        
        public string UpdatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_Inventory()
        {
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.InventoryResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.InventoryResponse();

            // Properties
            response.Id = Id;
            response.LocationId = PC_LocationId;
            response.QtyAvailable = QtyAvailable;
            response.QtyOnHand = QtyOnHand;
            response.Cost = Cost;
            response.ParentId = Convert.ToInt64(InternalId);

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

        public void ConvertToDatabaseObject(string companyId, string tableName, Tagge.Common.Models.InventoryRequest request)
        {
            // Properties
            DV_CompanyId = companyId;
            PC_LocationId = request.LocationId;
            QtyAvailable = request.QtyAvailable;
            QtyOnHand = request.QtyOnHand;
            Cost = request.Cost;
            ST_TableName = tableName;

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }
        #endregion
    }
}
