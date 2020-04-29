using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_Product
    {
        #region Properties
        public long Id { get; set; }
        public string DV_CompanyId { get; set; }
        
        public string Sku { get; set; }
        
        public string Description { get; set; }
        
        public string Name { get; set; }
        
        public string Type { get; set; }
        
        public string TrackingMethod { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? DefaultPrice { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? MSRP { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? SalePrice { get; set; }
        
        public string TaxClass { get; set; }
        
        public string Barcode { get; set; }
        
        public string Status { get; set; }
        
        public string Unit { get; set; }
        
        public bool? AllowDiscounts { get; set; }
        
        public bool? AllowBackorders { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public decimal? InStockThreshold { get; set; }

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

        [BsonDefaultValue(false)]
        public bool IsActive { get; set; }
        
        public PC_Kit Kit { get; set; }
        
        public List<PC_ProductCategory> Categories { get; set; }
        
        public List<PC_Option> Options { get; set; }
        
        public List<PC_ProductUnit> Units { get; set; }
        
        public List<PC_AlternateId> AlternateIds { get; set; }
        
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_Product()
        {
            Kit = new PC_Kit();
            //Variants = new List<PC_ProductVariant>();
            //Inventory = new List<PC_Inventory>();
            Categories = new List<PC_ProductCategory>();
            Options = new List<PC_Option>();
            Units = new List<PC_ProductUnit>();
            AlternateIds = new List<PC_AlternateId>();
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        /// <summary>
        /// Converts the Product to a Product Response
        /// </summary>
        /// <returns></returns>
        public Tagge.Common.Models.ProductResponse ConvertToResponse(string companyId, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.ProductResponse();

            // Build the response
            response.Id = Id;
            response.Name = Name;
            response.Sku = Sku;
            response.Description = Description;
            response.DefaultPrice = DefaultPrice;
            response.MSRP = MSRP;
            response.SalePrice = SalePrice;
            response.TaxClass = TaxClass;
            response.Barcode = Barcode;
            response.Unit = Unit;
            response.Type = Type;
            response.Status = Status;
            response.TrackingMethod = TrackingMethod;
            response.AllowDiscounts = AllowDiscounts;
            response.AllowBackorders = AllowBackorders;
            response.InStockThreshold = InStockThreshold;
            response.Width = Width;
            response.Height = Height;
            response.Depth = Depth;
            response.Weight = Weight;

            // Collections
            // Kit
            if (Kit != null && Kit.IsActive)
                response.Kit = Kit.ConvertToResponse(companyId, "PC_Kit", db);

            // Variants - Managed in PC_ProductVariant
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
                response.Options = new List<Tagge.Common.Models.OptionResponse>();
                foreach (var option in Options)
                {
                    if(option.IsActive)
                        response.Options.Add(option.ConvertToResponse(companyId, "PC_ProductOption", db));
                }
            }

            // Units
            if (Units != null)
            {
                response.Units = new List<Tagge.Common.Models.ProductUnitResponse>();
                foreach (var unit in Units)
                {
                    if (unit.IsActive)
                        response.Units.Add(unit.ConvertToResponse(companyId, "PC_ProductUnit", db));
                }
            }

            // AlternateIds
            if (AlternateIds != null)
            {
                response.AlternateIds = new List<Tagge.Common.Models.ProductAlternateIdResponse>();
                foreach (var alternateId in AlternateIds)
                {
                    if (alternateId.IsActive)
                        response.AlternateIds.Add(alternateId.ConvertToResponse(companyId, "PC_ProductAlternateId", db));
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

        /// <summary>
        /// Converts the Product to a Get All Response
        /// </summary>
        /// <returns></returns>
        public Sheev.Common.Models.GetAllResponse ConvertToGetAllResponse()
        {
            var response = new Sheev.Common.Models.GetAllResponse();

            // Build the response
            response.Id = Id;
            response.Name = Name;
            response.Description = Description;

            return response;
        }

        /// <summary>
        /// Converts the request to a database object
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="request"></param>
        public void ConvertToDatabaseObject(string companyId, Tagge.Common.Models.ProductRequest request)
        {
            // Properties
            DV_CompanyId = companyId;
            Name = request.Name;
            Sku = request.Sku;
            Description = request.Description;
            DefaultPrice = request.DefaultPrice == null ? 0 : (decimal)request.DefaultPrice;
            MSRP = request.MSRP;
            SalePrice = request.SalePrice;
            TaxClass = request.TaxClass;
            Barcode = request.Barcode;
            Unit = request.Unit;
            AllowDiscounts = request.AllowDiscounts;
            AllowBackorders = request.AllowBackorders;
            InStockThreshold = request.InStockThreshold;
            TrackingMethod = request.TrackingMethod;
            Type = request.Type;
            Status = request.Status;
            Width = request.Width;
            Height = request.Height;
            Depth = request.Depth;
            Weight = request.Weight;
            IsActive = true;

            // Kit
            if (request.Kit != null)
                Kit.ConvertToDatabaseObject(request.Kit);

            // Variants - Managed in PC_ProductVariant

            // Inventory - Managed in PC_Inventory

            // Categories - Managed in PC_Category

            // Options
            if (request.Options != null)
            {
                foreach (var option in request.Options)
                {
                    var dbOption = new PC_Option();
                    dbOption.ConvertToDatabaseObject(option);
                    Options.Add(dbOption);
                }
            }

            // Units
            if (request.Units != null)
            {
                foreach (var unit in request.Units)
                {
                    var dbUnit = new PC_ProductUnit();
                    dbUnit.ConvertToDatabaseObject(unit);
                    Units.Add(dbUnit);
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

        /// <summary>
        /// Checks for empty collections
        /// </summary>
        public void CheckForEmptyCollections()
        {
            //if (Variants.Count <= 0)
            //    Variants = null;

            //if(Inventory.Count <= 0)
            //    Inventory = null;

            if (Categories.Count <= 0)
                Categories = null;

            if (Options.Count <= 0)
                Options = null;

            if (Units.Count <= 0)
                Units = null;

            if (AlternateIds.Count <= 0)
                AlternateIds = null;

            if (CustomFields.Count <= 0)
                CustomFields = null;

            //if (ExternalIds.Count <= 0)
            //    ExternalIds = null;
        }
        #endregion
    }
}
