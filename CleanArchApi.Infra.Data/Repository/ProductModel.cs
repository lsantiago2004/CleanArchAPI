using Deathstar.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    /// <summary>
    /// Its the Product Model!
    /// </summary>
    public class ProductModel : IProductModel
    {
        private readonly ICategoryAssignmentModel _categoryAssignmentModel;
        private readonly IBaseContextModel context;
        private readonly IWebhookModel _webHookModel;
        private readonly IVariantModel _variantModel;
        private readonly IExternalIdModel _externalIdModel;
        private readonly IInventoryModel _inventoryModel;
        private readonly IUnitModel _unitModel;
        private readonly IOptionModel _optionModel;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IKitModel _kitModel;
        private readonly IAlternateIdModel _alternateIdModel;
        private readonly ITypeModel _typeModel;
        ////public ProductModel(ITypeModel typeModel, IAlternateIdModel alternateIdModel, IKitModel kitModel, ICustomFieldModel customFieldModel, IOptionModel optionModel, IUnitModel unitModel, IInventoryModel inventoryModel, IExternalIdModel externalIdModel, IVariantModel variantModel, IWebhookModel webHookModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel)
        public ProductModel(ICustomFieldModel customFieldModel, ITypeModel typeModel, IAlternateIdModel alternateIdModel, IKitModel kitModel, IOptionModel optionModel, IUnitModel unitModel, IInventoryModel inventoryModel, IExternalIdModel externalIdModel, IVariantModel variantModel,IWebhookModel webHookModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _categoryAssignmentModel = categoryAssignmentModel;
            _webHookModel = webHookModel;
            context = new Models.ContextModel(mongoDbSettings, apiSettings);
            ////context = contextModel;
            _variantModel = variantModel;
            _externalIdModel = externalIdModel;
            _inventoryModel = inventoryModel;
            _unitModel = unitModel;
            _optionModel = optionModel;
            _customFieldModel = customFieldModel;
            _kitModel = kitModel;
            _alternateIdModel = alternateIdModel;
            _typeModel = typeModel;
        }
        #region Get(s)
        /// <summary>
        /// Gets all Products
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="context">Database context see ContextModel.cs</param>
        /// <param name="count"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<GetAllResponse> GetAllProducts(string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100)
        {
            // Paging
            int limit = (int)pageSize;
            int start = (int)pageNumber == 1 ? 0 : (int)(pageNumber - 1) * limit;

            // Response
            List<GetAllResponse> response = new List<GetAllResponse>();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Dictonary
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Action
            string action = string.Empty;

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);

            // Add Company Id filter
            dictionary.Add("DV_CompanyId", companyId.ToString());

            // TODO add a filter in here that if isactive is not passed in so that by default all products that are active will be returned

            // Filter
            if (!string.IsNullOrEmpty(queryString))
            {
                Utilities.MongoFilter.BuildFilterDictionary(queryString, ref action, ref dictionary);
            }

            // Check to see if IsActive is being added if not add it
            if (!dictionary.ContainsKey("IsActive"))
                dictionary.Add("IsActive", "true");

            // Check for invalid filters
            //CheckForInvalidFilters(dictionary);

            // Filter Builder
            FilterDefinition<Deathstar.Data.Models.PC_Product> filters;

            // Handle Action
            switch (action.ToLower())
            {
                case "contains":
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_Product>(fieldContainsValue: dictionary);
                    break;
                default:
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_Product>(fieldEqValue: dictionary);
                    break;
            }

            count = productCollection.Find(filters).CountDocuments();
            var dbProducts = productCollection.Find(filters).SortByDescending(x => x.CreatedDateTime).Skip(start).Limit(limit).ToList();

            foreach (var dbProduct in dbProducts)
            {
                var singleResponse = new GetAllResponse()
                {
                    Id = dbProduct.Id,
                    Name = dbProduct.Name,
                    Description = dbProduct.Description,
                    Value = dbProduct.Sku,
                };

                response.Add(singleResponse);
            }

            return response;
        }

        /// <summary>
        /// Get a single product by Id
        /// </summary>
        /// <param name="id">product id</param>
        /// <param name="context">Database context see ContextModel.cs</param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductResponse> GetProductById(long id,  Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

            var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbProduct == null)
            {
                string reason = $"Product not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (id: {dbProduct.Id}, sku: {dbProduct.Sku}) successfully retrieved.", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Complete, trackingGuid);

            // Build the Response
            var response = dbProduct.ConvertToResponse(companyId.ToString(), context.Database);

            // Inventory
            response.Inventory = await _inventoryModel.GetById(dbProduct.Id.ToString(), "PC_Product", trackingGuid);

            // Variant
            response.Variants = await _variantModel.GetAll(dbProduct.Id, trackingGuid);

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbProduct.Id.ToString(), "PC_Product", trackingGuid);

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Saves New Product
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductResponse> Save(Tagge.Common.Models.ProductRequest request,  Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductResponse();
            var companyId = context.Security.GetCompanyId();
            long id = 0;

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning Product save by user {context.Security.GetEmail()},", "Save Product", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;

                // Get MongoDB
                var db = context.Database;
                var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "product_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Word
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Check to see if the Sku is a duplicate
                await CheckForDuplicateSkus(request.Sku, companyId.ToString(), productCollection, context, trackingGuid);

                // Validate Status
                await _typeModel.ValidateStatusType(request.Status, trackingGuid);

                // Validate Type
                await _typeModel.ValidateProductType(request.Type, trackingGuid);

                // Validate Tracking Method
                await _typeModel.ValidateTrackingMethodType(request.TrackingMethod, trackingGuid);

                // Check to see if the tracking method is set to products if it is make sure no variants are sent in as well
                if (request.TrackingMethod.ToLower() == "product" && request.Variants != null && request.Variants.Count > 0)
                {
                    string reason = $"The Tracking Method is set to Product, which does not allow for variants.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                }

                // Get Id
                id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Product
                dbProduct.ConvertToDatabaseObject(companyId.ToString(), request);

                // Units
                if (request.Units != null && request.Units.Count > 0)
                {
                    dbProduct.Units = await _unitModel.Save(id, request.Units, trackingGuid);
                }

                // Save Kit
                if (request.Kit != null)
                {
                    dbProduct.Kit = await _kitModel.Save(id, "PC_Kit", request.Kit, trackingGuid);
                }

                // Category Assignment
                if (request.Categories != null && request.Categories.Count > 0)
                {
                    dbProduct.Categories = await _categoryAssignmentModel.Save(context, id, request.Categories, dbProduct.Categories, trackingGuid);
                }

                // Options
                if (request.Options != null && request.Options.Count > 0)
                {
                    dbProduct.Options = await _optionModel.Save(id, request.Options, trackingGuid);
                }

                // Save Alternate Ids
                if (request.AlternateIds != null && request.AlternateIds.Count > 0)
                {
                    dbProduct.AlternateIds = await _alternateIdModel.SaveOrUpdate(context, id, dbProduct.Sku, "PC_ProductAlternateId", request.AlternateIds, new List<Deathstar.Data.Models.PC_AlternateId>(), dbProduct, trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    ////dbProduct.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_Product", id.ToString(), trackingGuid);
                }

                // Add Id
                dbProduct.Id = id;

                // Add Created By & Timestamp
                dbProduct.CreatedBy = context.Security.GetEmail();
                dbProduct.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // Insert
                await productCollection.InsertOneAsync(dbProduct);

                // Building the Response
                response = dbProduct.ConvertToResponse(companyId.ToString(), context.Database);

                // If the product doesnt save then this will not be hit

                // Inventory
                if (request.Inventory != null && request.Inventory.Count > 0)
                {
                    response.Inventory = new List<Common.Models.InventoryResponse>();

                    foreach (var inventoryRequest in request.Inventory)
                    {
                        response.Inventory.Add(await _inventoryModel.Save(inventoryRequest, "PC_Product", id.ToString(), trackingGuid));
                    }
                }

                // Variant
                if (request.Variants != null && request.Variants.Count > 0)
                {
                    response.Variants = new List<Common.Models.ProductVariantResponse>();

                    foreach (var variantRequest in request.Variants)
                    {
                        response.Variants.Add(await _variantModel.Save(variantRequest, dbProduct, trackingGuid));
                    }
                }

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Product", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (id: {dbProduct.Id}, sku: {dbProduct.Sku}) successfully saved.", "Save Product", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Build the Webhook event
                var whRequest = new Sheev.Common.Models.WebhookResponse()
                {
                    CompanyId = companyId.ToString(),
                    Type = "Product",
                    Scope = "product/created",
                    Id = response.Id.ToString()
                };

                // Trigger the Webhook event
                await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);
                await RollbackProduct(id);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CatalogModel.SaveProduct()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                await RollbackProduct(id);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Updates a Product by Product Id
        /// </summary>
        /// <param name="id">Product Id</param>
        /// <param name="request">Product Request</param>
        /// <param name="context">Database context see ContextModel.cs</param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductResponse> Update(long id, Tagge.Common.Models.ProductRequest request,  Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductResponse();
            var companyId = context.Security.GetCompanyId();

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning a product update by user {context.Security.GetEmail()}", "Update Product", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;

                // Get MongoDB
                var db = context.Database;
                var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);

                // Word
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Validate Status
                await _typeModel.ValidateStatusType(request.Status, trackingGuid);

                // Validate Type
                await _typeModel.ValidateProductType(request.Type, trackingGuid);

                // Validate Tracking Method
                await _typeModel.ValidateTrackingMethodType(request.TrackingMethod, trackingGuid);

                // Convert to DB Object
                dbProduct.ConvertToDatabaseObject(companyId.ToString(), request);

                // Check for empty collections and null them
                dbProduct.CheckForEmptyCollections();

                // Add Updated By & Timestamp
                dbProduct.UpdatedBy = context.Security.GetEmail();
                dbProduct.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                var dbExistingProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                if (dbExistingProduct == null)
                {
                    string reason = $"Product not found for Id: {id}. Use Post to create a new product.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                // Units
                if (request.Units != null && request.Units.Count > 0)
                {
                    dbProduct.Units = await _unitModel.Save(id, request.Units, trackingGuid);
                }

                // Save Kit
                if (request.Kit != null)
                    dbProduct.Kit = await _kitModel.SaveOrUpdate(id, "PC_Kit", dbExistingProduct.Kit, request.Kit, trackingGuid);

                // Category Assignment
                if (request.Categories != null && request.Categories.Count > 0)
                {
                    dbProduct.Categories = await _categoryAssignmentModel.Save(context, id, request.Categories, dbExistingProduct.Categories, trackingGuid);
                }

                // Options
                if (request.Options != null && request.Options.Count > 0)
                {
                    dbProduct.Options = await _optionModel.Save(id, request.Options, trackingGuid);
                }

                // Save Alternate Ids
                if (request.AlternateIds != null && request.AlternateIds.Count > 0)
                {
                    dbProduct.AlternateIds = await _alternateIdModel.SaveOrUpdate(context, id, dbExistingProduct.Sku, "PC_ProductAlternateId", request.AlternateIds, dbExistingProduct.AlternateIds, dbProduct, trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    ////dbProduct.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingProduct.CustomFields, "PC_Product", id.ToString(), trackingGuid);
                }

                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbProduct, serializerSettings)) } };

                // Update database record
                await productCollection.UpdateOneAsync(filters, update);

                // Convert To Response
                response = dbProduct.ConvertToResponse(companyId.ToString(), context.Database);

                // Inventory
                if (request.Inventory != null && request.Inventory.Count > 0)
                {
                    // Initialize the response subcollection
                    response.Inventory = new List<Common.Models.InventoryResponse>();

                    foreach (var inventoryRequest in request.Inventory)
                    {
                        response.Inventory.Add(await _inventoryModel.SaveOrUpdate(inventoryRequest, "PC_Product", id.ToString(), trackingGuid));
                    }
                }

                // Variant
                if (request.Variants != null && request.Variants.Count > 0)
                {
                    // Set the id back in the dbProduct
                    // Its set to 0 for the serializer above to ignore it and not add a new id field in when saving to mongo
                    dbProduct.Id = id;

                    // Initialize the response subcollection
                    response.Variants = new List<Common.Models.ProductVariantResponse>();

                    foreach (var variantRequest in request.Variants)
                    {
                        response.Variants.Add(await _variantModel.SaveOrUpdate(variantRequest, dbProduct, trackingGuid));
                    }
                }

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Product", id.ToString(), trackingGuid);
                }

                // Add Id
                response.Id = id;

                // Build the Webhook event
                var whRequest = new Sheev.Common.Models.WebhookResponse()
                {
                    CompanyId = companyId.ToString(),
                    Type = "Product",
                    Scope = "product/updated",
                    Id = response.Id.ToString()
                };

                // Trigger the Webhook event
                await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (sku: {request.Sku}) failed to update! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CatalogModel.UpdateProduct()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or reactivate a product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, id);

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

            var dbProduct = productCollection.Find(filters).FirstOrDefault();

            if (dbProduct == null)
            {
                string reason = $"Product not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Set the updated timestamp here
            string timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_Product>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await productCollection.UpdateOneAsync(filters, update);

            // Update the corresponding Inventory record
            await _inventoryModel.DeleteOrReactivate(dbProduct.Id, reactivate, "PC_Product", timestamp, dbProduct.UpdatedDateTime, trackingGuid);

            // Update the corresponding Variant record(s)
            await _variantModel.DeleteOrReactivateByProductId(dbProduct.Id, reactivate, timestamp, dbProduct.UpdatedDateTime, trackingGuid);

            // Update the corresponding External Id record(s)
            await _externalIdModel.DeleteOrReactivateByParentId(dbProduct.Id.ToString(), reactivate, "PC_Product", timestamp, dbProduct.UpdatedDateTime, trackingGuid, "");

            // determine if the hook should be a delete or update
            string whScope = reactivate ? "updated" : "deleted";

            // Build the Webhook event
            var whRequest = new Sheev.Common.Models.WebhookResponse()
            {
                CompanyId = companyId.ToString(),
                Type = "Product",
                Scope = $"product/{whScope}",
                Id = id.ToString()
            };

            // Trigger the Webhook event
            await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        /// <summary>
        /// Checks for duplicate skus 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sku"></param>
        /// <returns></returns>
        public static async Task CheckForDuplicateSkus(string sku, string companyId, IMongoCollection<Deathstar.Data.Models.PC_Product> productCollection,  IBaseContextModel context, Guid trackingGuid)
        {
            var duplicatefilter = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Sku, sku);
            duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId);
            var duplicateSku = await productCollection.Find(duplicatefilter).ToListAsync();

            if (duplicateSku != null && duplicateSku.Count > 0)
            {
                string reason = $"Duplicate Sku ({sku}) found on another Product";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be saved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }

        /// <summary>
        /// Validates a Product by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<Deathstar.Data.Models.PC_Product> ValidateById(string id, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var productCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");

            Int64.TryParse(id, out long longId);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, longId);
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

            var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbProduct == null)
            {
                string reason = $"Product not found by Id:{longId} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbProduct;
        }

        /// <summary>
        /// Validates a Product by Sku and unit (actually validates the unit seperately)
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="unitName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<Deathstar.Data.Models.PC_Product> ValidateBySku(string sku,  IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var productCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Sku, sku);
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

            var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbProduct == null)
            {
                string reason = $"Product not found by Sku:{sku} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbProduct;
        }
        #endregion

        #region Rollback Method(s)
        public async Task RollbackProduct(long id)
        {
            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);

            // Filter
            var filter = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, id);

            // Delete
            await productCollection.DeleteOneAsync(filter);

            // Inventory
            await _inventoryModel.RollbackInventory(id, "PC_Product");

            // Variant
            await _variantModel.RollbackVariant(id);

            // External Ids
            await _externalIdModel.RollbackExternalId(id, "PC_Product");
        }
        #endregion
    }
}
