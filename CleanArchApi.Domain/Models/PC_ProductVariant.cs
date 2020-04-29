using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_ProductVariant
    {
        #region Properties
        public long Id { get; set; }
        
        public string DV_CompanyId { get; set; }
        
        public long ParentId { get; set; }
        
        public string Sku { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? DefaultPrice { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? MSRP { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? SalePrice { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? Cost { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? QtyOnHand { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? QtyAvailable { get; set; }

        public string Barcode { get; set; }

        public string Status { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? Width { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? Height { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? Depth { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? Weight { get; set; }
        
        public string CreatedDateTime { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string UpdatedDateTime { get; set; }
        
        public string UpdatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        public PC_Kit Kit { get; set; }
        
        public List<PC_ProductCategory> Categories { get; set; }
        
        public List<PC_OptionValue> Options { get; set; }
        
        public List<PC_AlternateId> AlternateIds { get; set; }
        
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_ProductVariant()
        {
            Kit = new PC_Kit();
            Categories = new List<PC_ProductCategory>();
            Options = new List<PC_OptionValue>();
            AlternateIds = new List<PC_AlternateId>();
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.ProductVariantResponse ConvertToResponse(string companyId, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.ProductVariantResponse();

            response.Id = Id;
            response.Sku = Sku;
            response.DefaultPrice = DefaultPrice;
            response.MSRP = MSRP;
            response.SalePrice = SalePrice;
            response.Barcode = Barcode;
            response.Width = Width;
            response.Height = Height;
            response.Depth = Depth;
            response.Weight = Weight;
            response.Status = Status;

            // Collections
            // Kit
            if (Kit != null && Kit.IsActive)
                response.Kit = Kit.ConvertToResponse(companyId, "PC_VariantKit", db);


            // Inventory - Managed in PC_Inventory

            // Categories
            if (Categories != null)
            {
                response.Categories = new List<Tagge.Common.Models.CategoryAssignmentResponse>();
                foreach (var category in Categories)
                {
                    if(category.IsActive)
                        response.Categories.Add(category.ConvertToGenericResponse());
                }
            }

            // Options
            if (Options != null)
            {
                response.Options = new List<Tagge.Common.Models.OptionValueResponse>();
                foreach (var option in Options)
                {
                    if(option.IsActive)
                        response.Options.Add(option.ConvertToResponse(companyId, "PC_ProductVariantOption", db));
                }
            }

            // AlternateIds
            if (AlternateIds != null)
            {
                response.AlternateIds = new List<Tagge.Common.Models.ProductAlternateIdResponse>();
                foreach (var alternateId in AlternateIds)
                {
                    if (alternateId.IsActive)
                        response.AlternateIds.Add(alternateId.ConvertToResponse(companyId, "PC_ProductVariantAlternateId", db));
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

        public void ConvertToDatabaseObject(string companyId, Tagge.Common.Models.ProductVariantRequest request)
        {
            // Properties
            DV_CompanyId = companyId;
            //ParentId = productId;
            Sku = request.Sku;
            DefaultPrice = request.DefaultPrice;
            MSRP = request.MSRP;
            SalePrice = request.SalePrice;
            Barcode = request.Barcode;
            Width = request.Width;
            Height = request.Height;
            Depth = request.Depth;
            Weight = request.Weight;
            Status = request.Status;

            // Kit
            if (request.Kit != null)
                Kit.ConvertToDatabaseObject(request.Kit);

            // Inventory - Managed in PC_Inventory

            // Categories - Managed in PC_Category

            // Option Values
            if (request.Options != null)
            {
                foreach (var option in request.Options)
                {
                    var dbOptionValue = new PC_OptionValue();
                    dbOptionValue.ConvertToDatabaseObject(option);
                    Options.Add(dbOptionValue);
                }
            }

            // AlternateIds
            if (request.AlternateIds != null)
            {
                foreach (var alternateId in request.AlternateIds)
                {
                    var dbAlternateId = new PC_AlternateId();
                    dbAlternateId.ConvertToDatabaseObject(alternateId);
                    AlternateIds.Add(dbAlternateId);
                }
            }

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }
        #endregion
    }
}
