using Deathstar.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    public class VariantModel : IVariantModel
    {
        private readonly ICategoryAssignmentModel _categoryAssignmentModel;
        private readonly IBaseContextModel context;
        private readonly IMongoCollection<PC_ProductVariant> _productVariantCollection;
        private readonly IWebhookModel _webHookModel;
        private readonly IInventoryModel _inventoryModel;
        private readonly IExternalIdModel _externalIdModel;
        private readonly IKitModel _kitModel;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IOptionValueModel _optionValueModel;
        private readonly IAlternateIdModel _alternateIdModel;
        private readonly ITypeModel _typeModel;

        //cr
        ////public VariantModel(ITypeModel typeModel, IAlternateIdModel alternateIdModel, IOptionValueModel optionValueModel, ICustomFieldModel customFieldModel, IKitModel kitModel, IExternalIdModel externalIdModel, IInventoryModel inventoryModel, IProductModel productModel, IWebhookModel webHookModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel)
        ////public VariantModel(ITypeModel typeModel, IAlternateIdModel alternateIdModel, IOptionValueModel optionValueModel, IKitModel kitModel, IExternalIdModel externalIdModel, IInventoryModel inventoryModel, IProductModel productModel, IWebhookModel webHookModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel)
        public VariantModel(ICustomFieldModel customFieldModel, ITypeModel typeModel, IAlternateIdModel alternateIdModel, IOptionValueModel optionValueModel, IKitModel kitModel, IExternalIdModel externalIdModel, IInventoryModel inventoryModel, IWebhookModel webHookModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel)
        {
            _categoryAssignmentModel = categoryAssignmentModel;
            _productVariantCollection = contextModel.Database.GetCollection<Deathstar.Data.Models.PC_ProductVariant>("PC_ProductVariant");
            _webHookModel = webHookModel;
            _inventoryModel = inventoryModel;
            _externalIdModel = externalIdModel;
            _kitModel = kitModel;
            _customFieldModel = customFieldModel;
            _optionValueModel = optionValueModel;
            _alternateIdModel = alternateIdModel;
            context = contextModel;
            _typeModel = typeModel;
            
        }
        #region Get Method(s)
        /// <summary>
        /// Get all variants
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Tagge.Common.Models.ProductVariantResponse>> GetAll(long productId, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            //var db = context.Database;
            //var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.ParentId, productId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

            // Looking high and low for the variants!
            var dbVariants = await _productVariantCollection.FindAsync(filters).Result.ToListAsync();

            // Build the Response
            var response = new List<Tagge.Common.Models.ProductVariantResponse>();

            foreach (var dbVariant in dbVariants)
            {
                var variantReponse = dbVariant.ConvertToResponse(companyId.ToString(), context.Database);

                // Add Id 
                variantReponse.Id = dbVariant.Id;

                // Inventory
                variantReponse.Inventory = await _inventoryModel.GetById(dbVariant.Id.ToString(), "PC_ProductVariant", trackingGuid);

                // External Ids
                variantReponse.ExternalIds = await _externalIdModel.GetByParentId(dbVariant.Id.ToString(), "PC_ProductVariant", trackingGuid);

                response.Add(variantReponse);
            }

            return response;
        }

        /// <summary>
        /// Get Variant by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductVariantResponse> GetById(long id, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            //// Get MongoDB
            //var db = context.Database;
            //var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

            var dbVariant = await _productVariantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbVariant == null)
            {
                string reason = $"Product Variant not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Variant was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (id: {dbProduct.Id}, sku: {dbProduct.Sku}) successfully retrieved.", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Complete, trackingGuid);

            // Build the Response
            var response = dbVariant.ConvertToResponse(companyId.ToString(), context.Database);

            // Add Id 
            response.Id = dbVariant.Id;

            // Alternate Ids
            foreach (var altId in response.AlternateIds)
            {
                altId.ExternalIds = await _externalIdModel.GetByParentId(altId.Id.ToString(), "PC_ProductVariantAlternateId", trackingGuid);
            }

            // Options
            foreach (var option in response.Options)
            {
                option.ExternalIds = await _externalIdModel.GetByParentId(option.Id.ToString(), "PC_ProductVariantOption", trackingGuid);
            }

            // Inventory
            response.Inventory = await _inventoryModel.GetById(dbVariant.Id.ToString(), "PC_ProductVariant", trackingGuid);

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbVariant.Id.ToString(), "PC_ProductVariant", trackingGuid);

            return response;
        }
        #endregion

        #region Switch Method(s)
        /// <summary>
        /// Internal Use Only! Saves or Updates a variant based on id provided in the variant request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dbProduct"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductVariantResponse> SaveOrUpdate(Tagge.Common.Models.ProductVariantRequest request, Deathstar.Data.Models.PC_Product dbProduct, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductVariantResponse();

            if (request.Id.HasValue && request.Id.Value > 0)
            {
                // Update
                response = await Update(request, dbProduct.Id, trackingGuid);
            }
            else
            {
                // Add
                response = await Save(request, dbProduct, trackingGuid);
            }

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Internal Use Only! Save a new Variant
        /// </summary>
        /// <param name="dbProduct">product id</param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductVariantResponse> Save(Tagge.Common.Models.ProductVariantRequest request, Deathstar.Data.Models.PC_Product dbProduct, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductVariantResponse();
            var companyId = context.Security.GetCompanyId();

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning Product Variant save by user {context.Security.GetEmail()},", "Save Product Variant", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
                string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

                // Get MongoDB
                var db = context.Database;
                var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "productvariant_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Word
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Check to see if the Sku is a duplicate at product level
                //cr (circular reference)
                //await _productModel.CheckForDuplicateSkus(request.Sku, companyId.ToString(), productCollection, trackingGuid);

                // Check to see if the Sku is a duplicate at variant level
                await CheckForDuplicateSkus(request.Sku, companyId.ToString(), variantCollection, trackingGuid);

                // Get Id
                var id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Product
                dbVariant.ConvertToDatabaseObject(companyId.ToString(), request);

                // Validate Status
                if (!string.IsNullOrEmpty(request.Status))
                    await _typeModel.ValidateStatusType(request.Status, trackingGuid);

                // Add Parent Id 
                dbVariant.ParentId = dbProduct.Id;

                // Save Kit
                if (request.Kit != null)
                    dbVariant.Kit = await _kitModel.Save(id, "PC_VariantKit", request.Kit, trackingGuid);

                // Category Assignment
                if (request.Categories != null && request.Categories.Count > 0)
                    dbVariant.Categories = await _categoryAssignmentModel.Save(context, id, request.Categories, dbVariant.Categories, trackingGuid);

                // Options
                if (request.Options != null && request.Options.Count > 0)
                    dbVariant.Options = await _optionValueModel.SaveOrUpdateVariantOptions(id, dbProduct, request.Options, new List<Deathstar.Data.Models.PC_OptionValue>(), trackingGuid); // Note this sends an empty list since options dont already exist on a new variant

                // Alternate Ids
                if (request.AlternateIds != null && request.AlternateIds.Count > 0)
                    dbVariant.AlternateIds = await _alternateIdModel.SaveOrUpdate(context, id, dbVariant.Sku, "PC_ProductVariantAlternateId", request.AlternateIds, new List<Deathstar.Data.Models.PC_AlternateId>(), dbProduct,trackingGuid);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbVariant.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_ProductVariant", id.ToString(), trackingGuid);
                }

                // Add Created By & Timestamp
                dbVariant.CreatedBy = context.Security.GetEmail();
                dbVariant.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                dbVariant.IsActive = true;

                // Add Id
                dbVariant.Id = id;

                // Insert
                await variantCollection.InsertOneAsync(dbVariant);

                // Building the Response
                response = dbVariant.ConvertToResponse(companyId.ToString(), context.Database);

                // If the variant doesnt save then this is not hit
                // Inventory
                if (request.Inventory != null && request.Inventory.Count > 0)
                {
                    response.Inventory = new List<Common.Models.InventoryResponse>();

                    foreach (var inventoryRequest in request.Inventory)
                    {
                        response.Inventory.Add(await _inventoryModel.Save(inventoryRequest, "PC_ProductVariant", id.ToString(), trackingGuid));
                    }
                }

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_ProductVariant", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Variant (id: {dbVariant.Id}, sku: {dbVariant.Sku}) successfully saved.", "Save Product Variant", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);
                //await RollbackVariant(id, context);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CatalogModel.SaveProduct()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }

        }

        #endregion

        #region Update Method(s)
        /// <summary>
        /// Adds/Updates a variant on a product
        /// </summary>
        /// <param name="id">product id</param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductVariantResponse> Update(Tagge.Common.Models.ProductVariantRequest request, long id, Guid trackingGuid, bool fromController = false)
        {
            var response = new Tagge.Common.Models.ProductVariantResponse();
            var companyId = context.Security.GetCompanyId();

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning a product update by user {context.Security.GetEmail()}", "Update Product", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

                // Get MongoDB
                var db = context.Database;
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Validate product
                var dbProduct = await ProductModel.ValidateById(id.ToString(),context, trackingGuid);

                // Word
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Convert to DB Object
                dbVariant.ConvertToDatabaseObject(companyId.ToString(), request);

                // variant id
                long variantId = 0;

                // Validate Status
                if (!string.IsNullOrEmpty(request.Status))
                    await _typeModel.ValidateStatusType(request.Status, trackingGuid);

                // Set to active
                dbVariant.IsActive = true;

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                // If the id is passed in use it
                if (request.Id.HasValue && request.Id.Value > 0)
                    filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, request.Id.Value);
                else // otherwise use the sku
                    filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Sku, request.Sku);

                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Check to see if the variant exists
                var dbExistingVariant = await variantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                // Save or Update database record
                if (dbExistingVariant == null)
                {
                    // Save the variant
                    response = await Save(request, dbProduct, trackingGuid);

                    // Build the Webhook event
                    var whRequest = new Sheev.Common.Models.WebhookResponse()
                    {
                        CompanyId = companyId.ToString(),
                        Type = "Product Variant",
                        Scope = "product/variant/created",
                        Id = response.Id.ToString()
                    };

                    // Trigger the Webhook event
                    await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);

                    // set variant id 
                    variantId = response.Id;
                }
                else
                {
                    // set variant id 
                    variantId = dbExistingVariant.Id;

                    // Save Kit
                    if (request.Kit != null)
                        dbVariant.Kit = await _kitModel.SaveOrUpdate(variantId, "PC_VariantKit", dbExistingVariant.Kit, request.Kit, trackingGuid);

                    // Category Assignment
                    if (request.Categories != null && request.Categories.Count > 0)
                        dbVariant.Categories = await _categoryAssignmentModel.Save(context, variantId, request.Categories, dbExistingVariant.Categories, trackingGuid);

                    // Options
                    if (request.Options != null && request.Options.Count > 0)
                        dbVariant.Options = await _optionValueModel.SaveOrUpdateVariantOptions(variantId, dbProduct, request.Options, dbExistingVariant.Options, trackingGuid);

                    // Alternate Ids
                    if (request.AlternateIds != null && request.AlternateIds.Count > 0)
                        dbVariant.AlternateIds = await _alternateIdModel.SaveOrUpdate(context, variantId, dbExistingVariant.Sku, "PC_ProductVariantAlternateId", request.AlternateIds, dbExistingVariant.AlternateIds, dbProduct, trackingGuid);

                    // Custom Fields 
                    if (request.CustomFields != null && request.CustomFields.Count > 0)
                    {
                        dbVariant.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingVariant.CustomFields, "PC_ProductVariant", variantId.ToString(), trackingGuid);
                    }

                    // Add Updated By & Timestamp
                    dbVariant.UpdatedBy = context.Security.GetEmail();
                    dbVariant.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, serializerSettings)) } };

                    await variantCollection.UpdateOneAsync(filters, update);

                    // Convert To Response
                    // Needs to come first before the inventory or external ids as it will wipe them from the response
                    response = dbVariant.ConvertToResponse(companyId.ToString(), context.Database);

                    // Inventory
                    if (request.Inventory != null && request.Inventory.Count > 0)
                    {
                        response.Inventory = new List<Common.Models.InventoryResponse>();

                        foreach (var inventoryRequest in request.Inventory)
                        {
                            response.Inventory.Add(await _inventoryModel.SaveOrUpdate(inventoryRequest, "PC_ProductVariant", variantId.ToString(), trackingGuid));
                        }
                    }

                    // ExternalIds 
                    if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                    {
                        response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_ProductVariant", variantId.ToString(), trackingGuid);
                    }

                    // Add Id
                    response.Id = variantId;

                    // Send the hook only if it comes from the controller
                    if (fromController)
                    {
                        // Build the Webhook event
                        var whRequest = new Sheev.Common.Models.WebhookResponse()
                        {
                            CompanyId = companyId.ToString(),
                            Type = "Product Variant",
                            Scope = "product/variant/updated",
                            Id = response.Id.ToString()
                        };

                        // Trigger the Webhook event
                        await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);
                    }

                }

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
        /// Delete/Reactivate a variant
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
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;
            var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, id);

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

            var dbVariant = variantCollection.Find(filters).FirstOrDefault();

            if (dbVariant == null)
            {
                string reason = $"Product Variant not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Variant was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            string timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_ProductVariant>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await variantCollection.UpdateOneAsync(filters, update);

            // Update corresponding Inventory record
            await _inventoryModel.DeleteOrReactivate(dbVariant.Id, reactivate, "PC_ProductVariant", timestamp, dbVariant.UpdatedDateTime, trackingGuid);

            // Build the Webhook event
            var whRequest = new Sheev.Common.Models.WebhookResponse()
            {
                CompanyId = companyId.ToString(),
                Type = "Product Variant",
                Scope = "product/variant/updated",
                Id = id.ToString()
            };

            // Trigger the Webhook event
            await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Internal Use Only! Delete/Reactivate a variants by product id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reactivate"></param>
        /// <param name="timestamp"></param>
        /// <param name="orginialTimestamp"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivateByProductId(long id, bool reactivate, string timestamp, string orginialTimestamp, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);
            var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);

            // Filter
            var productFilters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            productFilters = productFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, id);

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

            // Find the product
            var dbProduct = productCollection.Find(productFilters).FirstOrDefault();

            // Oh noes the product is missing
            if (dbProduct == null)
            {
                string reason = $"Product Variant not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Variant was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.ParentId, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, !reactivate);

            // If true add an additional filter
            if (reactivate)
            {
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.UpdatedDateTime, orginialTimestamp);
            }

            // Finding variants!
            var dbVariants = await variantCollection.Find(filters).ToListAsync();

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_ProductVariant>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await variantCollection.UpdateManyAsync(filters, update);

            foreach (var dbVariant in dbVariants)
            {
                // Update corresponding Inventory record
                await _inventoryModel.DeleteOrReactivate(dbVariant.Id, reactivate, "PC_ProductVariant", timestamp, orginialTimestamp, trackingGuid);

                // Update the corresponding External Id record(s)
                await _externalIdModel.DeleteOrReactivateByParentId(dbVariant.Id.ToString(), reactivate, "PC_ProductVariant", timestamp, orginialTimestamp, trackingGuid, "");
            }
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
        public async Task CheckForDuplicateSkus(string sku, string companyId, IMongoCollection<Deathstar.Data.Models.PC_ProductVariant> productvariantCollection, Guid trackingGuid)
        {
            var duplicatefilter = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Sku, sku);
            duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId);
            var duplicateSku = await productvariantCollection.Find(duplicatefilter).ToListAsync();

            if (duplicateSku != null && duplicateSku.Count > 0)
            {
                string reason = $"Duplicate Sku ({sku}) found on another Product Variant";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be saved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }

        /// <summary>
        /// Validates a Variant by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task ValidateById(string id, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Get the collection
            var variantCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_ProductVariant>("PC_ProductVariant");

            // Parse the id
            Int64.TryParse(id, out long longId);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, longId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

            // Find that variant
            var dbVariant = await variantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            // Variant appears not to exist
            if (dbVariant == null)
            {
                string reason = $"Variant not found by Id:{longId} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Variant was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

        }

        /// <summary>
        /// Validates a Variant by Sku and parent unit (actually validates the unit seperately)
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="unitName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<Deathstar.Data.Models.PC_ProductVariant> ValidateBySku(string sku, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Get the collection
            var variantCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_ProductVariant>("PC_ProductVariant");

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Sku, sku);
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

            // Find that variant
            var dbVariant = await variantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            // Variant appears not to exist
            if (dbVariant == null)
            {
                string reason = $"Variant not found by Sku:{sku} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Variant was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbVariant;
        }
        #endregion

        #region Rollback Method(s)
        public async Task RollbackVariant(long id)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;
            var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.ParentId, id);

            var dbVariants = await variantCollection.Find(filters).ToListAsync();

            // Delete
            await variantCollection.DeleteManyAsync(filters);

            // Loop through the former variants and remove
            if (dbVariants != null && dbVariants.Count > 0)
            {
                foreach (var dbVariant in dbVariants)
                {
                    // Inventory
                    await _inventoryModel.RollbackInventory(dbVariant.Id, "PC_ProductVariant");

                    // External Ids
                    await _externalIdModel.RollbackExternalId(dbVariant.Id, "PC_ProductVariant");
                }
            }
        }
        #endregion
    }
}
