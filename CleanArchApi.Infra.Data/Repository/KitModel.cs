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
    public class KitModel : IKitModel
    {
        private readonly IBaseContextModel context;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IKitComponentModel _kitComponentModel;
        private readonly ITypeModel _typeModel;
        
        public KitModel(ITypeModel typeModel, IKitComponentModel kitComponentModel, ICustomFieldModel customFieldModel, IBaseContextModel contextModel, IExternalIdModel externalIdModel)
        {
            context = contextModel;
            _externalIdModel = externalIdModel;
            _customFieldModel = customFieldModel;
            _kitComponentModel = kitComponentModel;
            _typeModel = typeModel;
        }

        #region Get Method(s)
        /// <summary>
        /// Get a kit by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.KitResponse> GetById(string id, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.KitResponse();
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(collectionName);
            var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

            // Break combined id
            string[] ids = id.Split('|');

            // Parent id
            long.TryParse(ids[0], out long parentId);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, parentId);
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

            // Does the product exist?
            var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            // Looks like it does
            if (dbProduct != null)
            {
                // Build the Product version Response
                response = dbProduct.Kit.ConvertToKitResponse();

                // Add Id 
                response.Id = dbProduct.Kit.PC_Kit_Id;

                // External Ids
                response.ExternalIds = await _externalIdModel.GetByParentId(dbProduct.Kit.PC_Kit_Id.ToString(), "PC_Kit", trackingGuid);
            }
            else
            {
                // Product wasnt found now gotta try the variant
                var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, parentId);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Where in the world is the variant?
                var dbVariant = await variantCollection.FindAsync(variantFilters).Result.FirstOrDefaultAsync();

                // Its missing of course!
                if (dbVariant == null)
                {
                    string reason = $"Kit not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                // Build the Variant version Response
                response = dbVariant.Kit.ConvertToKitResponse();

                // Add Id 
                response.Id = dbVariant.Kit.PC_Kit_Id;

                // External Ids
                response.ExternalIds = await _externalIdModel.GetByParentId(dbVariant.Kit.PC_Kit_Id.ToString(), "PC_VariantKit", trackingGuid);
            }

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (id: {dbProduct.Id}, sku: {dbProduct.Sku}) successfully retrieved.", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Complete, trackingGuid);   

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new Kit 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.KitResponse> Save(Tagge.Common.Models.KitRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.KitResponse();
            var companyId = context.Security.GetCompanyId();

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

                // Word
                var dbKit = new Deathstar.Data.Models.PC_Kit();

                // Id prefix
                string idPrefix = string.Empty;

                // Table
                string tableName = string.Empty;

                // Product
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Variant
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Variant Filter 
                var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Sku, request.Sku);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Sku, request.Sku);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                // Find product first
                dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

                if (dbProduct != null)
                {
                    idPrefix = dbProduct.Id.ToString();
                    tableName = "PC_Product";
                }
                else
                {
                    // Find that Variant!
                    dbVariant = await variantCollection.Find(variantFilters).FirstOrDefaultAsync();

                    // Missing Variant oh noes!
                    if (dbVariant == null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid sku: {request.Sku}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid sku: {request.Sku}" };
                    }

                    idPrefix = dbVariant.Id.ToString();
                    tableName = "PC_ProductVariant";
                }

                // Validate Kit Type
                await _typeModel.ValidateKitType(request.Type, trackingGuid);

                // Convert request to Location
                dbKit.ConvertToDatabaseObject(request);

                // Set Primary Key
                dbKit.SetPrimaryKey(idPrefix, request.Sku);

                // Add Created By & Timestamp
                dbKit.CreatedBy = context.Security.GetEmail();
                dbKit.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbKit.IsActive = true;

                // Insert
                if (tableName == "PC_Product")
                {
                    // Custom Fields 
                    if (request.CustomFields != null && request.CustomFields.Count > 0)
                    {
                        dbKit.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_Kit", dbKit.PC_Kit_Id, trackingGuid);
                    }

                    // Components
                    if (request.Components != null && request.Components.Count > 0)
                    {
                        dbKit.Components = await _kitComponentModel.Save(dbKit.PC_Kit_Id, "PC_KitComponent", request.Components, trackingGuid);
                    }

                    // Add
                    dbProduct.Kit = dbKit;

                    // So that the serializer ignores this field
                    dbProduct.Id = 0;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbProduct, serializerSettings)) } };

                    // Insert
                    await productCollection.UpdateOneAsync(filters, update);

                    // Building the Response
                    response = dbKit.ConvertToResponse(companyId.ToString(), "PC_Kit", db);

                    // Add Id
                    response.Id = dbKit.PC_Kit_Id;

                    // ExternalIds 
                    if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                    {
                        response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Kit", dbKit.PC_Kit_Id, trackingGuid);
                    }
                }
                else
                {
                    // Custom Fields 
                    if (request.CustomFields != null && request.CustomFields.Count > 0)
                    {
                        dbKit.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_VariantKit", dbKit.PC_Kit_Id, trackingGuid);
                    }

                    // Components
                    if (request.Components != null && request.Components.Count > 0)
                    {
                        dbKit.Components = await _kitComponentModel.Save(dbKit.PC_Kit_Id, "PC_VariantKitComponent", request.Components, trackingGuid);
                    }

                    // Add
                    dbVariant.Kit = dbKit;

                    // So that the serializer ignores this field
                    dbVariant.Id = 0;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, serializerSettings)) } };

                    // Insert
                    await variantCollection.UpdateOneAsync(variantFilters, update);

                    // Building the Response
                    response = dbKit.ConvertToResponse(companyId.ToString(), "PC_VariantKit", db);

                    // Add Id
                    response.Id = dbKit.PC_Kit_Id;

                    // ExternalIds 
                    if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                    {
                        response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_VariantKit", dbKit.PC_Kit_Id, trackingGuid);
                    }
                }


                // Log success
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit (sku: {dbKit.Sku} successfully saved.", "Save Kit", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Trigger the Webhook event
                //if (_useWebhook)
                //{
                //    var whRequest = new WebhookResponse()
                //    {
                //        CompanyId = companyId.ToString(),
                //        Type = "Product",
                //        Scope = "product/created",
                //        Id = response.Id.ToString(),
                //        TrackingGuid = trackingGuid
                //    };

                //    WebhookModel.WebhookTriggerEvent(whRequest, trackingGuid);
                //}

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit ( Sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "KitModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// Internal Use Only! Save a new Kit 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="internalId"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Deathstar.Data.Models.PC_Kit> Save(long internalId, string tableName, Tagge.Common.Models.KitRequest request, Guid trackingGuid)
        {
            // Word
            var dbKit = new Deathstar.Data.Models.PC_Kit();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // Validate Kit Type
                await _typeModel.ValidateKitType(request.Type, trackingGuid);

                // Convert request to Location
                dbKit.ConvertToDatabaseObject(request);

                // Setting the Primary Key or Id field
                dbKit.SetPrimaryKey(internalId.ToString(), request.Sku);

                // Components
                if (request.Components != null && request.Components.Count > 0)
                {
                    dbKit.Components = await _kitComponentModel.Save(dbKit.PC_Kit_Id, tableName, request.Components, trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbKit.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, tableName, dbKit.PC_Kit_Id, trackingGuid);
                }

                // Add Created By & Timestamp
                dbKit.CreatedBy = context.Security.GetEmail();
                dbKit.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbKit.IsActive = true;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, tableName, dbKit.PC_Kit_Id, trackingGuid);
                }

                // Log Success
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit (sku: {dbKit.Sku} successfully saved.", "Save Kit", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Trigger the Webhook event
                //if (_useWebhook)
                //{
                //    var whRequest = new WebhookResponse()
                //    {
                //        CompanyId = companyId.ToString(),
                //        Type = "Product",
                //        Scope = "product/created",
                //        Id = response.Id.ToString(),
                //        TrackingGuid = trackingGuid
                //    };

                //    WebhookModel.WebhookTriggerEvent(whRequest, trackingGuid);
                //}

                return dbKit;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit ( Sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "LocationModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Internal Use Only! saves or Updates a Kit 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dbExistingKit"></param>
        /// <param name="internalId"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Deathstar.Data.Models.PC_Kit> SaveOrUpdate(long internalId, string tableName, Deathstar.Data.Models.PC_Kit dbExistingKit, Tagge.Common.Models.KitRequest request, Guid trackingGuid)
        {
            // Is update or new?
            bool isUpdate = true;

            // INitialize the kit
            var dbKit = new Deathstar.Data.Models.PC_Kit();

            // Word
            if (dbExistingKit == null)
            {
                dbKit = new Deathstar.Data.Models.PC_Kit();
                isUpdate = false;
            }

            try
            {
                if (isUpdate)
                {
                    // Setting the Primary Key or Id field to check if the existing kit matches the incoming update kit
                    dbKit.SetPrimaryKey(internalId.ToString(), request.Sku);

                    if (dbExistingKit.PC_Kit_Id != dbKit.PC_Kit_Id)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Kit Id does not found" };

                    // Set Existing Kit to the new one
                    dbKit = dbExistingKit;

                    // Validate Kit Type
                    await _typeModel.ValidateKitType(request.Type, trackingGuid);

                    // Update
                    // Convert request to Location
                    dbKit.ConvertToDatabaseObject(request);

                    // Components
                    if (request.Components != null && request.Components.Count > 0)
                    {
                        dbKit.Components = await _kitComponentModel.SaveOrUpdate(dbKit.PC_Kit_Id, tableName, request.Components, dbExistingKit.Components, trackingGuid);
                    }

                    // Custom Fields 
                    if (request.CustomFields != null && request.CustomFields.Count > 0)
                    {
                        dbKit.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbKit.CustomFields, tableName, dbKit.PC_Kit_Id, trackingGuid);
                    }

                    // Add Created By & Timestamp
                    dbKit.UpdatedBy = context.Security.GetEmail();
                    dbKit.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                    // IsActive
                    dbKit.IsActive = true;

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit (sku: {dbKit.Sku} successfully updated.", "Update Kit", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                    // ExternalIds 
                    if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                    {
                        await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, tableName, dbKit.PC_Kit_Id, trackingGuid);
                    }
                }
                else
                {
                    // Add
                    dbKit = await Save(internalId, tableName, request, trackingGuid);
                }

                return dbKit;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit ( Sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "LocationModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// Updates an existing kit 
        /// </summary>
        /// <param name="combinedId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.KitResponse> Update(string combinedId, Tagge.Common.Models.KitRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.KitResponse();
            var companyId = context.Security.GetCompanyId();

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

                // Word
                var dbKit = new Deathstar.Data.Models.PC_Kit();

                // Id prefix
                string idPrefix = string.Empty;

                // Table
                string tableName = string.Empty;

                // Kit table name
                string kitTableName = string.Empty;

                // Product
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Variant
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Existing Kit
                var dbExistingKit = new Deathstar.Data.Models.PC_Kit();

                // Break combined id
                string[] ids = combinedId.Split('|');

                // Product id
                long.TryParse(ids[0], out long internalId);

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, internalId);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                // Variant Filters
                var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Find product first
                dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

                if (dbProduct != null)
                {
                    idPrefix = dbProduct.Id.ToString();
                    tableName = "PC_Product";
                    kitTableName = "PC_Kit";
                    dbExistingKit = dbProduct.Kit;
                }
                else
                {
                    // Find that variant
                    dbVariant = await variantCollection.Find(variantFilters).FirstOrDefaultAsync();

                    // Missing variant check
                    if (dbVariant == null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid Id: {combinedId}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid sku: {request.Sku}" };
                    }

                    idPrefix = dbVariant.Id.ToString();
                    tableName = "PC_ProductVariant";
                    kitTableName = "PC_VariantKit";
                    dbExistingKit = dbVariant.Kit;
                }

                // Validate Kit Type
                await _typeModel.ValidateKitType(request.Type, trackingGuid);

                // Convert request to Location
                dbKit.ConvertToDatabaseObject(request);

                // Set Primary Key
                dbKit.SetPrimaryKey(idPrefix, request.Sku);

                // Components
                if (request.Components != null && request.Components.Count > 0)
                {
                    dbKit.Components = await _kitComponentModel.SaveOrUpdate(dbKit.PC_Kit_Id, tableName, request.Components, dbExistingKit.Components, trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbKit.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbKit.CustomFields, kitTableName, dbKit.PC_Kit_Id, trackingGuid);
                }

                // Add Updated By & Timestamp
                dbKit.UpdatedBy = context.Security.GetEmail();
                dbKit.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbKit.IsActive = true;

                // Insert
                if (tableName == "PC_Product")
                {
                    // Add
                    dbProduct.Kit = dbKit;

                    // So that the serializer ignores this field
                    dbProduct.Id = 0;

                    // Update
                    var update = Builders<Deathstar.Data.Models.PC_Product>.Update.Combine(
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.Type, dbKit.Type),
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.Description, dbKit.Description),
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.Components, dbKit.Components),
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.CustomFields, dbKit.CustomFields),
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.UpdatedBy, dbKit.UpdatedBy),
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.UpdatedDateTime, dbKit.UpdatedDateTime),
                                 Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.IsActive, true)
                    );

                    // Insert
                    await productCollection.UpdateOneAsync(filters, update);

                    // Building the Response
                    response = dbKit.ConvertToKitResponse();

                    // Add Id
                    response.Id = dbKit.PC_Kit_Id;

                    // ExternalIds 
                    if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                    {
                        response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, kitTableName, dbKit.PC_Kit_Id, trackingGuid);
                    }
                }
                else
                {
                    // Add
                    dbVariant.Kit = dbKit;

                    // So that the serializer ignores this field
                    dbVariant.Id = 0;

                    // Update
                    var update = Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Combine(
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.Type, dbKit.Type),
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.Description, dbKit.Description),
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.Components, dbKit.Components),
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.CustomFields, dbKit.CustomFields),
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.UpdatedBy, dbKit.UpdatedBy),
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.UpdatedDateTime, dbKit.UpdatedDateTime),
                                 Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Kit.IsActive, true)
                    );

                    // Insert
                    await variantCollection.UpdateOneAsync(variantFilters, update);

                    // Building the Response
                    response = dbKit.ConvertToKitResponse();

                    // Add Id
                    response.Id = dbKit.PC_Kit_Id;

                    // ExternalIds 
                    if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                    {
                        response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, kitTableName, dbKit.PC_Kit_Id, trackingGuid);
                    }
                }


                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit (sku: {dbKit.Sku} successfully saved.", "Save Kit", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Trigger the Webhook event
                //if (_useWebhook)
                //{
                //    var whRequest = new WebhookResponse()
                //    {
                //        CompanyId = companyId.ToString(),
                //        Type = "Product",
                //        Scope = "product/created",
                //        Id = response.Id.ToString(),
                //        TrackingGuid = trackingGuid
                //    };

                //    WebhookModel.WebhookTriggerEvent(whRequest, trackingGuid);
                //}

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit ( Sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "KitModel.Update()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or Reactivate a Kit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(string id, bool reactivate, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;
            var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);
            var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

            // Break combined id
            string[] ids = id.Split('|');

            // Parent id
            long.TryParse(ids[0], out long parentId);

            // Table
            string tableName = string.Empty;

            // Product
            var dbProduct = new Deathstar.Data.Models.PC_Product();

            // Variant
            var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, parentId);
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

            // Find that product!
            dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

            if (dbProduct != null)
            {
                // Delete or Reactivate the Alternate id
                var update = Builders<Deathstar.Data.Models.PC_Product>.Update.Combine(
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.UpdatedBy, context.Security.GetEmail()),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.UpdatedDateTime, DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz")),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.Kit.IsActive, reactivate)
                 );

                // Update Filter
                var updateFilters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, parentId);
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

                // Insert
                await productCollection.UpdateOneAsync(updateFilters, update);

                // Delete or Reactivate the External id
            }
            else
            {
                var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, parentId);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                dbVariant = await variantCollection.Find(variantFilters).FirstOrDefaultAsync();

                if (dbVariant == null)
                {
                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid sku: {id}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid sku: {id}" };
                }

                // Update the Following Fields
                var update = Builders<Deathstar.Data.Models.PC_ProductVariant>.Update
                                .Set(x => x.Kit.IsActive, reactivate)
                                .Set(x => x.Kit.UpdatedBy, context.Security.GetEmail())
                                .Set(x => x.Kit.UpdatedDateTime, DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz"));

                // Update database record
                await variantCollection.UpdateOneAsync(variantFilters, update);

                // Delete or Reactivate the External id
            }

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion 

        #region Validate Method(s)
        /// <summary>
        /// Validate a kit by sku
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task Validate(string sku, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var productCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Kit.Sku, sku);

            var dbLocation = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbLocation == null)
            {
                string reason = $"Kit not found by Sku:{sku} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }
        }
        #endregion
    }
}
