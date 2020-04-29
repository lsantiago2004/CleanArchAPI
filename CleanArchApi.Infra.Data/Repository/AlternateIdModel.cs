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
    public class AlternateIdModel : IAlternateIdModel
    {
        private readonly IBaseContextModel context;
        private readonly IAlternateIdTypeModel _alternateIdTypeModel;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IExternalIdModel _externalIdModel;
        ////public AlternateIdModel(IExternalIdModel externalIdModel, ICustomFieldModel customFieldModel, IAlternateIdTypeModel alternateIdTypeModel, IBaseContextModel contextModel)
        public AlternateIdModel(ICustomFieldModel customFieldModel, IExternalIdModel externalIdModel, IAlternateIdTypeModel alternateIdTypeModel, IBaseContextModel contextModel)

        {
            context = contextModel;
            _alternateIdTypeModel = alternateIdTypeModel;
            _customFieldModel = customFieldModel;
            _externalIdModel = externalIdModel;
        }
        #region Get Method(s)
        /// <summary>
        /// Get Alternate Id by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<ProductAlternateIdResponse> GetById(IBaseContextModel context ,string id, string tableName, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;

            // DB Object 
            var dbAlternateId = new Deathstar.Data.Models.PC_AlternateId();

            // Break combined id
            string[] ids = id.Split('|');

            // Parent id
            long.TryParse(ids[0], out long parentId);

            // Set table anem for alternate id
            string alternateIdTable = string.Empty;

            if (tableName == "PC_Product")
            {
                var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, parentId);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                if (dbProduct == null)
                {
                    string reason = $"Alternate Id not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                dbAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == id && x.IsActive);
                alternateIdTable = "PC_ProductAlternateId";
            }
            else
            {
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);
                

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, parentId);
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Off to see if a variant lives here
                var dbVariant = await variantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                // Maybe it doesn't?
                if (dbVariant == null)
                {
                    string reason = $"Alternate Id not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                dbAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == id && x.IsActive);
                alternateIdTable = "PC_ProductVariantAlternateId";
            }

            if (dbAlternateId == null)
            {
                string reason = $"Alternate Id Type not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type (id: {dbAlternateId.PC_AlternateId_Id}) successfully retrieved.", "Get Alternate Id Type", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

            // Build the Response
            var response = dbAlternateId.ConvertToResponse(companyId.ToString(), alternateIdTable, db);

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new Alternate Id on a product
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductAlternateIdResponse> Save(IBaseContextModel context, Tagge.Common.Models.AlternateIdRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductAlternateIdResponse();
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

                // Word
                var dbAlternateId = new Deathstar.Data.Models.PC_AlternateId();

                // Id prefix
                string idPrefix = string.Empty;

                // Sku
                string sku = string.Empty;

                // Product
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Sku, request.Sku);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

                if (dbProduct == null)
                {
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Product not found by sku provided: {request.Sku}" };
                }

                // Validate Alternate Id Type
                await _alternateIdTypeModel.Validate(context, request.AlternateIdTypeId, trackingGuid);

                // Convert request to Location
                dbAlternateId.ConvertToDatabaseObject(request);

                // Set Sku
                dbAlternateId.Sku = dbProduct.Sku;

                // Set Primary Key
                dbAlternateId.SetPrimaryKey(dbProduct.Id.ToString(), request.AlternateIdTypeId.ToString(), request.Unit);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    //cr
                    dbAlternateId.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_ProductAlternateId", dbAlternateId.PC_AlternateId_Id, trackingGuid);
                }

                // Add Created By & Timestamp
                dbAlternateId.CreatedBy = context.Security.GetEmail();
                dbAlternateId.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbAlternateId.IsActive = true;

                // Update
                dbProduct.AlternateIds.Add(dbAlternateId);

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
                response = dbAlternateId.ConvertToAlternateIdResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_ProductAlternateId", dbAlternateId.PC_AlternateId_Id, trackingGuid);
                }

                // Log Success
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id (sku: {dbAlternateId.Sku} successfully saved.", "Save Alternate Id", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

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
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "AlternateIdModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// Save a new Alternate Id on a variant
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductAlternateIdResponse> SaveVariant(IBaseContextModel context, Tagge.Common.Models.AlternateIdRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductAlternateIdResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
                string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

                // Get MongoDB
                var db = context.Database;
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                // Word
                var dbAlternateId = new Deathstar.Data.Models.PC_AlternateId();

                // Variant
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Check to see if the Variant exists
                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Sku, request.Sku);
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Still looking for that variant Maybe i should move these to there own method?
                dbVariant = await variantCollection.Find(filters).FirstOrDefaultAsync();

                if (dbVariant == null)
                {
                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid sku: {request.Sku}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid sku: {request.Sku}" };
                }

                // Validate Alternate Id Type
                await _alternateIdTypeModel.Validate(context, request.AlternateIdTypeId, trackingGuid);

                // Convert request to Location
                dbAlternateId.ConvertToDatabaseObject(request);

                // Set Sku
                dbAlternateId.Sku = dbVariant.Sku;

                // Set Primary Key
                dbAlternateId.SetPrimaryKey(dbVariant.Id.ToString(), request.AlternateIdTypeId.ToString(), request.Unit);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    //cr
                    dbAlternateId.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_ProductVariantAlternateId", dbAlternateId.PC_AlternateId_Id, trackingGuid);
                }

                // Add Created By & Timestamp
                dbAlternateId.CreatedBy = context.Security.GetEmail();
                dbAlternateId.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbAlternateId.IsActive = true;

                // Update
                dbVariant.AlternateIds.Add(dbAlternateId);

                // So that the serializer ignores this field
                dbVariant.Id = 0;

                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, serializerSettings)) } };

                // Insert
                await variantCollection.UpdateOneAsync(filters, update);

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id (sku: {dbAlternateId.Sku} successfully saved.", "Save Alternate Id", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Building the Response
                response = dbAlternateId.ConvertToAlternateIdResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_ProductVariantAlternateId", dbAlternateId.PC_AlternateId_Id, trackingGuid);
                }

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
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "LocationModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Save Or Update Method(s)
        /// <summary>
        /// Internal Use Only! Save a new Alternate Id 
        /// </summary>
        /// <param name="internalId">Product/Variant Id</param>
        /// <param name="sku">Product/Variant Sku</param>
        /// <param name="tableName"></param>
        /// <param name="request"></param>
        /// <param name="dbExistingAlternateIds"></param>
        /// <param name="dbProduct"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_AlternateId>> SaveOrUpdate(IBaseContextModel context, long internalId, string sku, string tableName, List<Tagge.Common.Models.ProductAlternateIdRequest> request, List<Deathstar.Data.Models.PC_AlternateId> dbExistingAlternateIds, Deathstar.Data.Models.PC_Product dbProduct, Guid trackingGuid)
        {
            // Get Company id
            var companyId = context.Security.GetCompanyId();

            try
            {
                foreach (var alternateId in request)
                {
                    // Initialize the alternate id
                    var dbAlternateId = new Deathstar.Data.Models.PC_AlternateId();

                    // Validate Alternate Id Type
                    await _alternateIdTypeModel.Validate(context, alternateId.AlternateIdTypeId, trackingGuid);

                    // Check to see if sku is populated if not error
                    if (string.IsNullOrEmpty(alternateId.AlternateId))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Alternate Id is null or empty" };

                    // Validate Unit
                    var dbProductUnit = new Deathstar.Data.Models.PC_ProductUnit();

                    // Check to see if the unit is null or empty
                    if (!string.IsNullOrEmpty(alternateId.Unit) && (dbProduct.Units != null || dbProduct.Units.Count > 0))
                        dbProductUnit = dbProduct.Units.FirstOrDefault(x => x.Name.ToLower() == alternateId.Unit.ToLower() && x.IsActive);

                    // Secondary check to really make sure that unit that exists
                    if (dbProductUnit == null || (dbProduct.Units == null || dbProduct.Units.Count == 0))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"FOR GLORY AND HONOR {alternateId.Unit}" };

                    // Convert request to Alternate Id
                    dbAlternateId.ConvertToDatabaseObject(alternateId);

                    // Set Sku
                    dbAlternateId.Sku = sku;

                    // Set Primary Key
                    dbAlternateId.SetPrimaryKey(internalId.ToString(), alternateId.AlternateIdTypeId.ToString(), alternateId.Unit);

                    // Check to see if the Alternate Id already exists
                    var dbExistingAlternateId = dbExistingAlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == dbAlternateId.PC_AlternateId_Id);

                    // Add the Alternate Id
                    if (dbExistingAlternateId == null)
                    {
                        // Custom Fields 
                        if (alternateId.CustomFields != null && alternateId.CustomFields.Count > 0)
                        {
                            //cr
                            dbAlternateId.CustomFields = await _customFieldModel.SaveGenericCustomField(alternateId.CustomFields, tableName, dbAlternateId.PC_AlternateId_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (alternateId.ExternalIds != null && alternateId.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(alternateId.ExternalIds, tableName, dbAlternateId.PC_AlternateId_Id, trackingGuid);
                        }

                        // Add Created By & Timestamp
                        dbAlternateId.CreatedBy = context.Security.GetEmail();
                        dbAlternateId.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // IsActive
                        dbAlternateId.IsActive = true;

                        // Add to collection
                        dbExistingAlternateIds.Add(dbAlternateId);
                    }
                    else // Update the existing Alternate Id
                    {
                        // Update the existing alternate id with the request
                        dbExistingAlternateId.Description = alternateId.Description;
                        dbExistingAlternateId.AlternateId = alternateId.AlternateId;

                        // Custom Fields 
                        if (alternateId.CustomFields != null && alternateId.CustomFields.Count > 0)
                        {
                            //cr
                            dbExistingAlternateId.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(alternateId.CustomFields, dbExistingAlternateId.CustomFields, tableName, dbAlternateId.PC_AlternateId_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (alternateId.ExternalIds != null && alternateId.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(alternateId.ExternalIds, tableName, dbAlternateId.PC_AlternateId_Id, trackingGuid);
                        }

                        // Add Created By & Timestamp
                        dbExistingAlternateId.UpdatedBy = context.Security.GetEmail();
                        dbExistingAlternateId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // IsActive
                        dbExistingAlternateId.IsActive = true;
                    }

                    //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit (sku: {dbAlternateId.Sku} successfully saved.", "Save Kit", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);
                }

                return dbExistingAlternateIds;
            }
            catch (HttpResponseException webEx)
            {
                //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit ( Sku: {request.Sku}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
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
        /// Updates an Alternate Id on a product
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductAlternateIdResponse> Update(IBaseContextModel context, string id, Tagge.Common.Models.AlternateIdRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductAlternateIdResponse();
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

                // Product
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Word
                var dbAlternateId = new Deathstar.Data.Models.PC_AlternateId();

                // Break combined id
                string[] ids = id.Split('|');

                // Product id
                long.TryParse(ids[0], out long productId);

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, productId);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                // Finding the product - the long wait
                dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                // Product not found so error
                if (dbProduct == null)
                {
                    string reason = $"Alternate Id not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                // Lookup alternate id by id
                var dbExistingAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == id);

                // Alternate Id not found by id so error
                if (dbExistingAlternateId == null)
                {
                    string reason = $"Alternate Id not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                // Convert request to Alternate Id
                dbAlternateId.ConvertToDatabaseObject(request);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    //cr
                    dbAlternateId.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingAlternateId.CustomFields, "PC_ProductAlternateId", id, trackingGuid);
                }

                // Add Created By & Timestamp
                dbAlternateId.UpdatedBy = context.Security.GetEmail();
                dbAlternateId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbAlternateId.IsActive = true;

                // Update
                var update = Builders<Deathstar.Data.Models.PC_Product>.Update.Combine(
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].Unit, dbAlternateId.Unit),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].AlternateId, dbAlternateId.AlternateId),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].Description, dbAlternateId.Description),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].CustomFields, dbAlternateId.CustomFields),
                             //Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].ExternalIds, dbAlternateId.ExternalIds),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].UpdatedBy, dbAlternateId.UpdatedBy),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].UpdatedDateTime, dbAlternateId.UpdatedDateTime),
                             Builders<Deathstar.Data.Models.PC_Product>.Update.Set(x => x.AlternateIds[-1].IsActive, true)
                );


                // Update Filter
                var updateFilters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, productId);
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.ElemMatch(x => x.AlternateIds, x => x.PC_AlternateId_Id == id);

                // Insert
                await productCollection.UpdateOneAsync(updateFilters, update);

                // Building the Response
                response = dbAlternateId.ConvertToAlternateIdResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_ProductAlternateId", id, trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Kit (sku: {dbAlternateId.Sku} successfully saved.", "Save Kit", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

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
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id ( Id: {id}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "AlternateIdModel.Update()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// Updates an Alternate Id on a variant
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductAlternateIdResponse> UpdateVariant(IBaseContextModel context, string id, Tagge.Common.Models.AlternateIdRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductAlternateIdResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
                string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

                // Get MongoDB
                var db = context.Database;
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                // Variant
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Word
                var dbAlternateId = new Deathstar.Data.Models.PC_AlternateId();

                // Break combined id
                string[] ids = id.Split('|');

                // Variant id
                long.TryParse(ids[0], out long variantId);

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, variantId);
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                // Look up in the sky is that a bird? no plane? no its a variant!
                dbVariant = await variantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                if (dbVariant == null)
                {
                    string reason = $"Alternate Id not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                var dbExistingAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == id);

                if (dbExistingAlternateId == null)
                {
                    string reason = $"Alternate Id not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                // Convert request to Location
                dbAlternateId.ConvertToDatabaseObject(request);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    //cr
                    dbAlternateId.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingAlternateId.CustomFields, "PC_ProductVariantAlternateId", id, trackingGuid);
                }

                // Add Created By & Timestamp
                dbAlternateId.UpdatedBy = context.Security.GetEmail();
                dbAlternateId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbAlternateId.IsActive = true;

                // Update
                var update = Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Combine(
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].Unit, dbAlternateId.Unit),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].AlternateId, dbAlternateId.AlternateId),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].Description, dbAlternateId.Description),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].CustomFields, dbAlternateId.CustomFields),
                             //Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].ExternalIds, dbAlternateId.ExternalIds),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].UpdatedBy, dbAlternateId.UpdatedBy),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].UpdatedDateTime, dbAlternateId.UpdatedDateTime),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.AlternateIds[-1].IsActive, true)
                );


                // Update Filter
                var updateFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, variantId);
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                updateFilters = updateFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.ElemMatch(x => x.AlternateIds, x => x.PC_AlternateId_Id == id);

                // Insert
                await variantCollection.UpdateOneAsync(updateFilters, update);

                // Building the Response
                response = dbAlternateId.ConvertToAlternateIdResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_ProductVariantAlternateId", id, trackingGuid);
                }
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
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "AlternateIdModel.UpdateVariant()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or Reactivate a Alternate id
        /// </summary>
        /// <param name="combinedId"></param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(IBaseContextModel context, string combinedId, string tableName, bool reactivate, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
                string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

                // Get MongoDB
                var db = context.Database;

                //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

                // Product
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Variant
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Product Category
                var dbProductAlternateId = new Deathstar.Data.Models.PC_AlternateId();

                // Get Id
                string[] ids = combinedId.Split('|');

                // Product or Variant Id
                var isNumeric = long.TryParse(ids[0], out long internalId);
                if (!isNumeric)
                {
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Id is not an integer" };
                }

                // Alternate Id
                if (string.IsNullOrEmpty(ids[1]))
                {
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Id is not an integer" };
                }

                // Switch between Product & Variant
                if (tableName == "PC_Product")
                {
                    // Get the table ready
                    var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                    // Filters - note always start with the company id
                    var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, internalId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                    // Find product first
                    dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

                    // MissinProductNo
                    if (dbProduct == null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid Id: {combinedId}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid Id: {combinedId}" };
                    }

                    // Find the Alternate Id with in the product
                    dbProductAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == combinedId);

                    if (dbProductAlternateId == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Alternate Id not found by id({combinedId}) provided" };

                    // Set the fields to update the Alternate Id
                    dbProductAlternateId.UpdatedBy = context.Security.GetEmail();
                    dbProductAlternateId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                    dbProductAlternateId.IsActive = reactivate;

                    // Set Product Id to 0 cause of the serializer
                    dbProduct.Id = 0;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbProduct, serializerSettings)) } };

                    // Update database record
                    await productCollection.UpdateOneAsync(filters, update);
                }
                else
                {
                    // Get the table ready
                    var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);

                    var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);
                    variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);
                    variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    dbVariant = await variantCollection.Find(variantFilters).FirstOrDefaultAsync();

                    if (dbVariant == null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid Id: {combinedId}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid Id: {combinedId}" };
                    }

                    // Find the Alternate Id with in the Variant
                    dbProductAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == combinedId);

                    if (dbProductAlternateId == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Alternate Id not found by id({combinedId}) provided" };

                    // Set the fields to update the Alternate Id
                    dbProductAlternateId.UpdatedBy = context.Security.GetEmail();
                    dbProductAlternateId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                    dbProductAlternateId.IsActive = reactivate;

                    // Set Product Id to 0 cause of the serializer
                    dbVariant.Id = 0;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    var variantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, serializerSettings)) } };

                    await variantCollection.UpdateOneAsync(variantFilters, variantUpdate);
                }

                return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
            }
            catch (HttpResponseException webEx)
            {
                //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group ( Name: {request.Name}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "AlternateIdModel.DeleteOrReactivate()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Validate Method(s)
        #endregion
    }
}
