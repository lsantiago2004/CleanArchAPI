using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_ProductUnit
    {
        #region Properties
        public string PC_ProductUnit_Id { get; set; }
        
        public string Name { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal Conversion { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal DefaultPrice { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal MSRP { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal SalePrice { get; set; }
        
        public string CreatedDateTime { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string UpdatedDateTime { get; set; }
        
        public string UpdatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        public List<PC_CustomField> CustomFields { get; set; }
        #endregion

        #region Constructor(s)
        public PC_ProductUnit()
        {
            CustomFields = new List<PC_CustomField>();
        }
        #endregion

        #region Method(s)
        /// <summary>
        /// Converts the database object to a response
        /// </summary>
        /// <returns></returns>
        public Tagge.Common.Models.ProductUnitResponse ConvertToUnitResponse()
        {
            var response = new Tagge.Common.Models.ProductUnitResponse();

            // Properties
            response.Id = PC_ProductUnit_Id;
            response.Name = Name;
            response.Conversion = Conversion;
            response.DefaultPrice = DefaultPrice;
            response.MSRP = MSRP;
            response.SalePrice = SalePrice;

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

        /// <summary>
        /// Converts the database object to a response
        /// </summary>
        /// <returns></returns>
        public Tagge.Common.Models.ProductUnitResponse ConvertToResponse(string companyId, string tableName, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.ProductUnitResponse();

            // Properties
            response.Id = PC_ProductUnit_Id;
            response.Name = Name;
            response.Conversion = Conversion;
            response.DefaultPrice = DefaultPrice;
            response.MSRP = MSRP;
            response.SalePrice = SalePrice;

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
            response.ExternalIds = PC_ExternalId.ConvertToResponse(PC_ProductUnit_Id, tableName, companyId, db);

            return response;
        }

        /// <summary>
        /// Converts the request to the database object
        /// </summary>
        /// <param name="request"></param>
        public void ConvertToDatabaseObject(Tagge.Common.Models.ProductUnitRequest request)
        {
            // Properties
            Name = request.Name;
            Conversion = request.Conversion;
            DefaultPrice = request.DefaultPrice;
            MSRP = request.MSRP;
            SalePrice = request.SalePrice;

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        /// <summary>
        /// Sets the Primary Key
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="unitName"></param>
        public void SetPrimaryKey(string parentId, string unitName)
        {
            PC_ProductUnit_Id = $"{parentId}|{unitName}";
        }
        #endregion
    }
}