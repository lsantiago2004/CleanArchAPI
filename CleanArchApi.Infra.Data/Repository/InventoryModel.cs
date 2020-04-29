using Deathstar.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Common.Models;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    /// <summary>
    /// Inventory Model!
    /// </summary>
    public class InventoryModel : IInventoryModel
    {
        private readonly IBaseContextModel context;
        private readonly IWebhookModel _webHookModel;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFieldModel;

        public InventoryModel(ICustomFieldModel customFieldModel, IExternalIdModel externalIdModel, IWebhookModel webHookModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel)
        {
            _webHookModel = webHookModel;
            context = contextModel;
            _externalIdModel = externalIdModel;
            _customFieldModel = customFieldModel;
        }

        #region Get Method(s)
        /// <summary>
        /// Get all inventories by product id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Tagge.Common.Models.InventoryResponse>> GetAll(long productId, string tableName, Guid trackingGuid)
        {
            // Response structure
            var response = new List<Tagge.Common.Models.InventoryResponse>();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

            // Get MongoDB
            var db = context.Database;
            var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.InternalId, productId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.ST_TableName, tableName);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);

            var dbInventories = await inventoryCollection.Find(filters).ToListAsync();

            // Build the Response
            foreach (var dbInventory in dbInventories)
            {
                var singleResponse = new Tagge.Common.Models.InventoryResponse();
                singleResponse = dbInventory.ConvertToResponse();

                // Inventory Table
                string inventoryTable = string.Empty;

                // Set inventory table
                if (dbInventory.ST_TableName == "PC_Product")
                {
                    inventoryTable = "PC_ProductInventory";
                }
                else
                {
                    inventoryTable = "PC_ProductVariantInventory";
                }

                // External Ids
                singleResponse.ExternalIds = await _externalIdModel.GetByParentId(dbInventory.Id.ToString(), inventoryTable, trackingGuid);

                response.Add(singleResponse);


            }

            return response;
        }

        /// <summary>
        /// Get a inventory by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.InventoryResponse> GetById(long id, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

            // Get MongoDB
            var db = context.Database;
            var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);

            var dbInventory = await inventoryCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbInventory == null)
            {
                string reason = $"Inventory not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory (id: {dbInventory.Id}) successfully retrieved.", "Get Inventory", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

            // Build the Response
            var response = dbInventory.ConvertToResponse();

            // Add Id 
            response.Id = dbInventory.Id;

            // Inventory Table
            string inventoryTable = string.Empty;

            // Set inventory table
            if (dbInventory.ST_TableName == "PC_Product")
            {
                inventoryTable = "PC_ProductInventory";
            }
            else
            {
                inventoryTable = "PC_ProductVariantInventory";
            }

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbInventory.Id.ToString(), inventoryTable, trackingGuid);

            return response;
        }

        /// <summary>
        /// Internal Use Only:Get a inventory by product/variant id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Tagge.Common.Models.InventoryResponse>> GetById(string id, string tableName, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

            // Get MongoDB
            var db = context.Database;
            var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.InternalId, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.ST_TableName, tableName);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbInventories = await inventoryCollection.FindAsync(filters).Result.ToListAsync();

            // Build the Response
            var response = new List<Tagge.Common.Models.InventoryResponse>();

            // Inventory Table
            string inventoryTable = string.Empty;

            // Set inventory table
            if (tableName == "PC_Product")
            {
                inventoryTable = "PC_ProductInventory";
            }
            else
            {
                inventoryTable = "PC_ProductVariantInventory";
            }

            foreach (var dbInventory in dbInventories)
            {
                var singleResponse = dbInventory.ConvertToResponse();

                // Add Id 
                singleResponse.Id = dbInventory.Id;

                // External Ids
                singleResponse.ExternalIds = await _externalIdModel.GetByParentId(dbInventory.Id.ToString(), inventoryTable, trackingGuid);

                response.Add(singleResponse);
            }

            return response;
        }
        #endregion

        #region Switch Method(s)
        public async Task<Tagge.Common.Models.InventoryResponse> SaveOrUpdate(InventoryRequest request, string tableName, string internalId, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.InventoryResponse();

            if (request.Id.HasValue && request.Id.Value > 0)
            {
                // Update
                response = await Update(request, request.Id.Value, trackingGuid);
            }
            else
            {
                // Add
                response = await Save(request, tableName, internalId, trackingGuid);
            }

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new inventory
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tableName"></param>
        /// <param name="internalId"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <param name="fromController"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.InventoryResponse> Save(InventoryRequest request, string tableName, string internalId, Guid trackingGuid, bool fromController = false)
        {
            var response = new Tagge.Common.Models.InventoryResponse();
            var companyId = context.Security.GetCompanyId();
            long id = 0;

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

                // Get MongoDB
                var db = context.Database;
                var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Inventory table
                string inventoryTable = string.Empty;

                // Webhook Scope
                string scope = string.Empty;

                // Verify Locations
                await LocationModel.Validate(request.LocationId, context, trackingGuid);

                // Validate Product/Variant and set inventory table
                if (tableName == "PC_Product")
                {
                    await ProductModel.ValidateById(internalId,context, trackingGuid);
                    inventoryTable = "PC_ProductInventory";
                    scope = "product";
                }
                else
                {
                    await VariantModel.ValidateById(internalId, context, trackingGuid);
                    inventoryTable = "PC_ProductVariantInventory";
                    scope = "product/variant";
                }

                // Check to see if the Inventory exists
                var existsfilter = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.InternalId, internalId); // Product/Variant Id
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.PC_LocationId, request.LocationId);
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.ST_TableName, inventoryTable);
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);
                var dbExistingInventory = inventoryCollection.Find(existsfilter).FirstOrDefault();

                if (dbExistingInventory != null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Inventory record already exists. Use PUT to update the inventory. (Id = {dbExistingInventory.Id})!" };

                // Word
                var dbInventory = new Deathstar.Data.Models.PC_Inventory();

                // Save Inventory
                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "inventory_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Get Id
                id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Inventory
                dbInventory.ConvertToDatabaseObject(companyId.ToString(), tableName, request);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbInventory.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, inventoryTable, id.ToString(), trackingGuid);
                }

                // Add Id
                dbInventory.Id = id;

                // Add Product/Variant Id
                dbInventory.InternalId = internalId;

                // Add Created By & Timestamp
                dbInventory.CreatedBy = context.Security.GetEmail();
                dbInventory.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                dbInventory.IsActive = true;

                // Insert
                await inventoryCollection.InsertOneAsync(dbInventory);

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory (id: {dbInventory.Id}) successfully saved.", "Save Inventory", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Building the Response
                response = dbInventory.ConvertToResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, inventoryTable, id.ToString(), trackingGuid);
                }

                // Build the Webhook event
                if (fromController)
                {
                    var whRequest = new Sheev.Common.Models.WebhookResponse()
                    {
                        CompanyId = companyId.ToString(),
                        Type = "Inventory",
                        Scope = $"{scope}/inventory/created",
                        Id = response.Id.ToString()
                    };

                    // Trigger the Webhook event
                    await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);
                }

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);

                // Rollback the inventory to a failure
                await RollbackInventory(id, tableName);

                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "InventoryModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                // Rollback the inventory due to a failure
                await RollbackInventory(id, tableName);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Updates an existing inventory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="fromController"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.InventoryResponse> Update(InventoryRequest request, long id, Guid trackingGuid, bool fromController = false)
        {
            var response = new Tagge.Common.Models.InventoryResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

                // Get MongoDB
                var db = context.Database;
                var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

                // Filter
                var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);
                filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                // Inventory table
                string inventoryTable = string.Empty;

                // Webhook Scope
                string scope = string.Empty;

                // Word
                var dbInventory = new Deathstar.Data.Models.PC_Inventory();

                // Check to see if the Inventory exists
                var existsfilter = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, id);
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);
                var existsInventory = inventoryCollection.Find(existsfilter).FirstOrDefault();

                if (existsInventory == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Inventory not found by Id: {id}" };

                // Validate Product/Variant and set inventory table
                if (existsInventory.ST_TableName == "PC_Product")
                {
                    inventoryTable = "PC_ProductInventory";
                    scope = "product";
                }
                else
                {
                    inventoryTable = "PC_ProductVariantInventory";
                    scope = "product/variant";
                }

                // Verify Locations
                await LocationModel.Validate(request.LocationId, context, trackingGuid);

                // Convert to DB Object
                dbInventory.ConvertToDatabaseObject(companyId.ToString(), existsInventory.ST_TableName, request);

                // Add Updated By & Timestamp
                dbInventory.UpdatedBy = context.Security.GetEmail();
                dbInventory.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbInventory.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbInventory.CustomFields, inventoryTable, id.ToString(), trackingGuid);
                }

                // Update
                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbInventory, serializerSettings)) } };

                // Update database record
                await inventoryCollection.UpdateOneAsync(filters, update);

                // Convert To Response
                response = dbInventory.ConvertToResponse();

                // Add Id
                response.Id = id;

                // Add back the Parent Id
                response.ParentId = Convert.ToInt64(existsInventory.InternalId);

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, inventoryTable, id.ToString(), trackingGuid);
                }

                // Only send webhooks when making a direct call from the controller
                if (fromController)
                {
                    // Build the Webhook event
                    var whRequest = new Sheev.Common.Models.WebhookResponse()
                    {
                        CompanyId = companyId.ToString(),
                        Type = "Inventory",
                        Scope = $"{scope}/inventory/updated",
                        Id = response.Id.ToString()
                    };

                    // Trigger the Webhook event
                    await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);
                }

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "InventoryModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or Reactivate a inventory
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
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

            // Get MongoDB
            var db = context.Database;
            var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            // Timestamp 
            string timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            var dbInventory = inventoryCollection.Find(filters).FirstOrDefault();

            if (dbInventory == null)
            {
                string reason = $"Inventory not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_Inventory>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await inventoryCollection.UpdateOneAsync(filters, update);

            // Update the corresponding External Id record(s)
            await _externalIdModel.DeleteOrReactivateByParentId(dbInventory.Id.ToString(), reactivate, dbInventory.ST_TableName, timestamp, dbInventory.UpdatedDateTime, trackingGuid, "");

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Internal Use Only: Delete or Reactive inventory on a Product or Variant
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reactivate"></param>
        /// <param name="tableName"></param>
        /// <param name="timestamp"></param>
        /// <param name="orginialTimestamp"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(long id, bool reactivate, string tableName, string timestamp, string orginialTimestamp, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

            // Get MongoDB
            var db = context.Database;
            var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.InternalId, id.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.ST_TableName, tableName);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, !reactivate);
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            if (reactivate)
            {
                filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.UpdatedDateTime, orginialTimestamp);
            }

            var dbInventories = await inventoryCollection.Find(filters).ToListAsync();

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_Inventory>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await inventoryCollection.UpdateManyAsync(filters, update);

            foreach (var dbInventory in dbInventories)
            {
                // Update the corresponding External Id record(s)
                await _externalIdModel.DeleteOrReactivateByParentId(dbInventory.Id.ToString(), reactivate, "PC_ProductVariant", timestamp, orginialTimestamp, trackingGuid, "");
            }

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Rollback Method(s)
        public async Task RollbackInventory(long id, string tableName)
        {
            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;

            // Get MongoDB
            var db = context.Database;
            var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.InternalId, id.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.ST_TableName, tableName);

            var dbInventories = await inventoryCollection.Find(filters).ToListAsync();

            // Delete
            await inventoryCollection.DeleteManyAsync(filters);

            // Inventory Table
            string inventoryTable = string.Empty;

            // Set inventory table
            if (tableName == "PC_Product")
            {
                inventoryTable = "PC_ProductInventory";
            }
            else
            {
                inventoryTable = "PC_ProductVariantInventory";
            }

            foreach (var dbInventory in dbInventories)
            {
                // External Ids
                await _externalIdModel.RollbackExternalId(dbInventory.Id, inventoryTable);
            }
        }
        #endregion

    }
}

