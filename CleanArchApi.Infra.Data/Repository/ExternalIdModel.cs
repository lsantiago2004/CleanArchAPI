using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    /// <summary>
    /// Its the External Id Model!
    /// </summary>
    public class ExternalIdModel : IExternalIdModel
    {
        private readonly IWebhookModel _webHookModel;
        private readonly IBaseContextModel context;
        public ExternalIdModel(IBaseContextModel contextModel, IWebhookModel webHookModel)
        {
            _webHookModel = webHookModel;
            context = contextModel;
        }

        #region Get Method(s)
        /// <summary>
        /// Get an external id by its parent id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Tagge.Common.Models.ExternalIdResponse>> GetByParentId(string id, string tableName, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Check to see if the context has systems loaded if not load them
            if (context.Systems == null)
                context.Systems = Utilities.CoreDatabaseAccess.GetSystems(context);

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

            // Get MongoDB
            var db = context.Database;
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, tableName);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

            var dbExternalIds = await externalIdCollection.FindAsync(filters).Result.ToListAsync();

            // Build the Response
            var response = new List<Tagge.Common.Models.ExternalIdResponse>();

            foreach (var dbExternalId in dbExternalIds)
            {
                // Create the single response
                var singleResponse = dbExternalId.ConvertToFullResponse();

                // Find the system
                var dbSystem = context.Systems.FirstOrDefault(x => x.Id == singleResponse.SystemId);

                // Set the system name
                singleResponse.SystemName = dbSystem != null ? dbSystem.Name : string.Empty;

                // Add single response to response
                response.Add(singleResponse);
            }

            return response;
        }

        /// <summary>
        /// Get a single external Id - Called from External Id Controller
        /// </summary>
        /// <param name="internalId"></param>
        /// <param name="systemId"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<string> GetExternalId(string internalId, long systemId, string tableName, Guid trackingGuid)
        {
            // Response
            var response = string.Empty;

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

            // Get MongoDB
            var db = context.Database;
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Table Name corrected
            string dbTable = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(tableName);
            if (string.IsNullOrEmpty(dbTable))
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"{tableName} Table not found." };

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, internalId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, systemId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, dbTable);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

            var dbExternalId = await externalIdCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbExternalId == null)
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"{tableName} External Id (Id:{internalId}) Not Found." };

            response = dbExternalId.ExternalId;

            return response;
        }

        /// <summary>
        /// Get a single internal Id - Called from External Id Controller
        /// </summary>
        /// <param name="externalId"></param>
        /// <param name="systemId"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<string> GetInternalId(string externalId, long systemId, string tableName, Guid trackingGuid)
        {
            // Response
            var response = string.Empty;

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

            // Get MongoDB
            var db = context.Database;
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Table Name corrected
            string dbTable = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(tableName);
            if (string.IsNullOrEmpty(dbTable))
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"{tableName} Table not found." };

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ExternalId, externalId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, systemId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, dbTable);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

            var dbExternalId = await externalIdCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbExternalId == null)
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"{tableName} External Id (Id:{externalId}) Not Found." };

            response = dbExternalId.InternalId;

            return response;
        }
        #endregion

        #region Save Or Update Method(s)
        /// <summary>
        /// Internal Use Only! Adds or updates an existing external id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tableName"></param>
        /// <param name="internalId"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<List<Tagge.Common.Models.ExternalIdResponse>> SaveOrUpdateGenericExternalId(List<Tagge.Common.Models.GenericExternalIdRequest> request, string tableName, string internalId, Guid trackingGuid)
        {
            // Response
            var response = new List<Tagge.Common.Models.ExternalIdResponse>();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // id
            long id = 0;

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

                // Get MongoDB
                var db = context.Database;
                var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                foreach (var externalIdRequest in request)
                {
                    // Validate the system exists
                    string systemName = await ValidateSystem(externalIdRequest.SystemId, trackingGuid);

                    // Filter
                    var duplicateFilters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ExternalId, externalIdRequest.ExternalId);
                    duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, tableName);
                    duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, externalIdRequest.SystemId);
                    duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

                    // Word
                    var dbExternalId = new Deathstar.Data.Models.PC_ExternalId();

                    // Check to see if the External Id is a duplicate
                    var duplicateExternalId = externalIdCollection.Find(duplicateFilters).FirstOrDefault();

                    // If the external id does exist and doesn't match the request internal id
                    if (duplicateExternalId != null && duplicateExternalId.InternalId != internalId)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"{externalIdRequest.ExternalId} External Id already exists" };

                    // Filters
                    var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, internalId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, tableName);
                    filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, externalIdRequest.SystemId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

                    // Check to see if the External Id exists
                    var existsExternalId = await externalIdCollection.Find(filters).FirstOrDefaultAsync();

                    // Set Is Active to true
                    dbExternalId.IsActive = true;

                    // Convert to DB Object
                    dbExternalId.ConvertToDatabaseObject(companyId.ToString(), tableName, externalIdRequest);
                    dbExternalId.InternalId = internalId;

                    // Either update the existing external or add a new one
                    if (existsExternalId == null)
                    {
                        // Add Updated By & Timestamp
                        dbExternalId.CreatedBy = context.Security.GetEmail();
                        dbExternalId.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // auto incrementing id filter
                        var filterId = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "externalid_id");
                        var updateId = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                        // Get Id
                        id = counterCollection.FindOneAndUpdate(filterId, updateId).Seq;

                        // Set Id
                        dbExternalId.Id = id;

                        // Save database record
                        await externalIdCollection.InsertOneAsync(dbExternalId);
                    }
                    else
                    {
                        // Set the existing external id to the new one so that nothing is lost
                        dbExternalId = existsExternalId;

                        // Update the external id since thats what we want to do
                        dbExternalId.ExternalId = externalIdRequest.ExternalId;

                        // Add Updated By & Timestamp
                        dbExternalId.UpdatedBy = context.Security.GetEmail();
                        dbExternalId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // Set Id for response
                        id = existsExternalId.Id;

                        // Set id to 0 to prevent it from adding this new column
                        dbExternalId.Id = 0;

                        // Update
                        var serializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };

                        var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbExternalId, serializerSettings)) } };

                        // Update database record
                        await externalIdCollection.UpdateOneAsync(filters, update);
                    }

                    // Convert To Response
                    var singleResponse = dbExternalId.ConvertToFullResponse();
                    singleResponse.Id = id;
                    singleResponse.SystemName = systemName;
                    response.Add(singleResponse);
                }

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"External Id failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "ExternalIdModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Adds or updates an existing external id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ExternalIdResponse> Update(Tagge.Common.Models.ExternalIdRequest request, Guid trackingGuid)
        {
            // Response
            var response = new Tagge.Common.Models.ExternalIdResponse();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // id
            long id = 0;

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

                // Get MongoDB
                var db = context.Database;
                var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Table Name corrected
                string dbTable = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(request.TableName);
                if (string.IsNullOrEmpty(dbTable))
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"{request.TableName} Table not found." };

                // Validate system exists
                var systemName = await ValidateSystem(request.SystemId, trackingGuid);

                // Validate Internal Id
                await ValidateInternalId(request.InternalId, dbTable, trackingGuid);

                // Duplicate Filters
                var duplicateFilters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ExternalId, request.ExternalId);
                duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, dbTable);
                duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, request.SystemId);
                duplicateFilters = duplicateFilters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

                // auto incrementing id filter
                var filterId = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "externalid_id");
                var updateId = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Word
                var dbExternalId = new Deathstar.Data.Models.PC_ExternalId();

                // Check to see if the there is a duplicate external id
                var duplicateExternalId = externalIdCollection.Find(duplicateFilters).FirstOrDefault();

                // If the external id does exist and doesn't match the request internal id
                if (duplicateExternalId != null && duplicateExternalId.InternalId != request.InternalId)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"{request.ExternalId} External Id already exists" };

                // Set Is Active to true
                dbExternalId.IsActive = true;

                // Convert to DB Object
                dbExternalId.ConvertToFullDatabaseObject(companyId.ToString(), dbTable, request);

                // Filters
                var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, request.InternalId);
                filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, dbTable);
                filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, request.SystemId);
                filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);

                // Check to see if the External Id exists
                var existsExternalId = await externalIdCollection.Find(filters).FirstOrDefaultAsync();

                // Save or Update database record
                if (existsExternalId == null)
                {
                    // Add Updated By & Timestamp
                    dbExternalId.CreatedBy = context.Security.GetEmail();
                    dbExternalId.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                    // Get Id
                    id = counterCollection.FindOneAndUpdate(filterId, updateId).Seq;

                    // Add Id
                    dbExternalId.Id = id;

                    // Insert
                    await externalIdCollection.InsertOneAsync(dbExternalId);
                }
                else
                {
                    // Add Updated By & Timestamp
                    dbExternalId.UpdatedBy = context.Security.GetEmail();
                    dbExternalId.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                    id = existsExternalId.Id;

                    // Update
                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };

                    var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbExternalId, serializerSettings)) } };

                    // Update database record
                    await externalIdCollection.UpdateOneAsync(filters, update);
                }

                // Convert To Response
                response = dbExternalId.ConvertToFullResponse();
                response.SystemName = systemName;
                response.Id = id;

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"External Id failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "ExternalIdModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Internal Use Only: Delete or Reactive external id by parent collection id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(Tagge.Common.Models.ExternalIdRequest request, bool reactivate, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

            // Get MongoDB
            var db = context.Database;
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Validate the system exists
            await ValidateSystem(request.SystemId, trackingGuid);

            // Table Name corrected
            string dbTable = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(request.TableName);
            if (string.IsNullOrEmpty(dbTable))
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"{request.TableName} Table not found." };

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ExternalId, request.ExternalId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, request.InternalId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.SystemId, request.SystemId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, dbTable);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, !reactivate);

            var dbExternalId = await externalIdCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbExternalId == null)
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"{request.TableName} External Id (Id:{request.InternalId}) Not Found." };

            // Timestamp
            string timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_ExternalId>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await externalIdCollection.UpdateManyAsync(filters, update);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Internal Use Only: Delete or Reactive external id by parent collection id
        /// </summary>
        /// <param name="internalId">parent id</param>
        /// <param name="reactivate"></param>
        /// <param name="tableName"></param>
        /// <param name="timestamp"></param>
        /// <param name="orginialTimestamp"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="sendWebhook"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivateByParentId(string internalId, bool reactivate, string tableName, string timestamp, string orginialTimestamp, Guid trackingGuid, string scope, bool sendWebhook = false)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

            // Get MongoDB
            var db = context.Database;
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, internalId);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, tableName);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, !reactivate);

            if (reactivate)
            {
                filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.UpdatedDateTime, orginialTimestamp);
            }

            // Find all existing exernal ids for this filter collection
            var dbExternalIds = externalIdCollection.Find(filters).ToList();


            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_ExternalId>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await externalIdCollection.UpdateManyAsync(filters, update);

            // if this setting is set to true wew should send out webhooks for each of these
            if (sendWebhook)
            {
                foreach (var dbExternalId in dbExternalIds)
                {
                    // Build the Webhook event
                    var whRequest = new Sheev.Common.Models.WebhookResponse()
                    {
                        CompanyId = companyId.ToString(),
                        Type = "External Id",
                        Scope = scope,
                        Destination = dbExternalId.SystemId.ToString(),
                        Id = dbExternalId.ExternalId
                    };

                    // Trigger the Webhook event
                    await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);
                }
            }

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        /// <summary>
        /// Validate a system
        /// </summary>
        /// <param name="systemId"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<string> ValidateSystem(long systemId,  Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Check to see if the context has systems loaded if not load them
            if (context.Systems == null)
                context.Systems = Utilities.CoreDatabaseAccess.GetSystems(context);

            // Validate the system exists
            var dbSystem = context.Systems.FirstOrDefault(x => x.Id == systemId);

            if (dbSystem == null)
            {
                string reason = $"System not found by Id: {systemId} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"System was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbSystem.Name;
        }

        /// <summary>
        /// Validates an internal id. This should be moved to another class since two different classes uses it at the moment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task ValidateInternalId(string id, string tableName, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;
            string inventoryCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Inventory").Name;
            string alternateIdTypeCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_AlternateIdType").Name;
            string categoryCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Category").Name;
            string categorySetCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_CategorySet").Name;
            string locationCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Location").Name;
            string locationTypeCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_LocationGroup").Name;

            // Get MongoDB
            var db = context.Database;

            // Break combined id
            string[] ids = id.Split('|');

            // Parent id
            long.TryParse(ids[0], out long internalId);

            switch (tableName)
            {
                case "PC_Category":
                    // Get the table
                    var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(categoryCollectionName);

                    // Filters - note always start with the company id
                    var categoryFilters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, internalId);
                    categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);

                    // Find the category
                    var dbCategory = await categoryCollection.FindAsync(categoryFilters).Result.FirstOrDefaultAsync();

                    // Check to see if the category exists if it doesn't error
                    if (dbCategory == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }

                    break;
                case "PC_CategorySet":
                    // Get the table
                    var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(categorySetCollectionName);

                    // Filters - note always start with the company id
                    var categorySetFilters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, internalId);
                    categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);

                    // Find the category set
                    var dbCategorySet = await categorySetCollection.FindAsync(categorySetFilters).Result.FirstOrDefaultAsync();

                    // Check to see if the category set exists if it doesn't error
                    if (dbCategorySet == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }

                    break;
                case "PC_Product":
                case "PC_ProductOption":
                case "PC_ProductUnit":
                case "PC_ProductOptionValue":
                case "PC_ProductAlternateId":
                case "PC_Kit":
                case "PC_KitComponent":
                    // Get the table
                    var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                    // Filters - note always start with the company id
                    var productFilters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    productFilters = productFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, internalId);
                    productFilters = productFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                    var dbProduct = await productCollection.FindAsync(productFilters).Result.FirstOrDefaultAsync();

                    if (dbProduct == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }

                    // This is the real check!
                    switch (tableName)
                    {
                        case "PC_ProductCategory":
                            break;
                        case "PC_ProductUnit":
                            var dbProductUnit = dbProduct.Units.FirstOrDefault(x => x.PC_ProductUnit_Id == id);

                            // Check to see if the object exists
                            if (dbProductUnit == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_ProductAlternateId":
                            var dbProductAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == id);

                            // Check to see if the object exists
                            if (dbProductAlternateId == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_Kit":
                            // Check to see if the object exists
                            if (dbProduct.Kit == null || dbProduct.Kit.PC_Kit_Id != id)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_KitComponent":
                            // Sanity Check
                            if (dbProduct.Kit == null || string.IsNullOrEmpty(dbProduct.Kit.PC_Kit_Id))
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                            var dbProductKitComponent = dbProduct.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == id);

                            // Check to see if the object exists
                            if (dbProductKitComponent == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_ProductOption":
                            var dbProductOption = dbProduct.Options.FirstOrDefault(x => x.PC_Option_Id == id);

                            // Check to see if the object exists
                            if (dbProductOption == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_ProductOptionValue":
                            // Get parent id
                            string parentId = string.Empty;
                            int indexOfSteam = id.LastIndexOf("|");
                            if (indexOfSteam >= 0)
                                parentId = id.Remove(indexOfSteam);

                            // Get the parent
                            var dbParentOption = dbProduct.Options.FirstOrDefault(x => x.PC_Option_Id == parentId);

                            // Check to see if the parent exists
                            if (dbParentOption == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                            // Find the option value
                            var dbOptionValue = dbParentOption.Values.FirstOrDefault(x => x.PC_OptionValue_Id == id);

                            // Check to see if the object exists
                            if (dbOptionValue == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                    }

                    break;
                case "PC_ProductVariant":
                case "PC_ProductVariantAlternateId":
                case "PC_VariantKit":
                case "PC_VariantKitComponent":
                    // Get the table
                    var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                    // Filters - note always start with the company id
                    var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);
                    variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                    var dbVariant = await variantCollection.FindAsync(variantFilters).Result.FirstOrDefaultAsync();

                    if (dbVariant == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }

                    // Still this is the real check!
                    switch (tableName)
                    {
                        case "PC_ProductVariantCategory":
                            break;
                        case "PC_ProductVariantAlternateId":
                            var dbProductVariantAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == id);

                            // Check to see if the object exists
                            if (dbProductVariantAlternateId == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_VariantKit":
                            // Check to see if the object exists
                            if (dbVariant.Kit == null || dbVariant.Kit.PC_Kit_Id != id)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                        case "PC_VariantKitComponent":
                            // Sanity Check
                            if (dbVariant.Kit == null || string.IsNullOrEmpty(dbVariant.Kit.PC_Kit_Id))
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                            var dbProductVariantKitComponent = dbVariant.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == id);

                            // Check to see if the object exists
                            if (dbProductVariantKitComponent == null)
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                            break;
                    }
                    break;
                case "PC_ProductVariantOption":
                    if (ids.Count() < 3)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid Id." };

                    // Cause the variant id is found in the second space
                    long.TryParse(ids[1], out long variantIdForOption);

                    var variantOptionCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                    // Filter
                    var variantParentFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    variantParentFilters = variantParentFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, variantIdForOption);
                    variantParentFilters = variantParentFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                    // look for variant by id
                    var dbVariantParent = await variantOptionCollection.FindAsync(variantParentFilters).Result.FirstOrDefaultAsync();

                    if (dbVariantParent == null)
                    {
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                    }

                    var dbProductVariantOption = dbVariantParent.Options.FirstOrDefault(x => x.PC_OptionValue_Id == id);

                    // Check to see if the object exists
                    if (dbProductVariantOption == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                    break;
                case "PC_ProductInventory":
                case "PC_ProductVariantInventory":
                    // Get the table
                    var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(inventoryCollectionName);

                    // Filters - note always start with the company id
                    var inventoryFilters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, internalId);
                    inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);

                    var dbInventory = await inventoryCollection.FindAsync(inventoryFilters).Result.FirstOrDefaultAsync();

                    if (dbInventory == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Inventory was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }
                    break;
                case "PC_Location":
                    // Get the table
                    var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(locationCollectionName);

                    // Filters - note always start with the company id
                    var locationFilters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, internalId);
                    locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.IsActive, true);

                    var dbLocation = await locationCollection.FindAsync(locationFilters).Result.FirstOrDefaultAsync();

                    if (dbLocation == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }
                    break;
                case "PC_LocationGroup":
                    // Get the table
                    var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(locationTypeCollectionName);

                    // Filters - note always start with the company id
                    var locationGroupFilters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, internalId);
                    locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.IsActive, true);

                    var dbLocationGroup = await locationGroupCollection.FindAsync(locationGroupFilters).Result.FirstOrDefaultAsync();

                    if (dbLocationGroup == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }


                    break;
                case "PC_AlternateIdType":

                    // Get the table
                    var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(alternateIdTypeCollectionName);

                    // Filters - note always start with the company id
                    var alternateIdTypeFilters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, internalId);
                    alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.IsActive, true);

                    var dbAlternateIdType = await alternateIdTypeCollection.FindAsync(alternateIdTypeFilters).Result.FirstOrDefaultAsync();

                    if (dbAlternateIdType == null)
                    {
                        string reason = $"Internal Id({id}) not found or does not exist.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }


                    break;
                    //case "pc_categorysetassignment":
                    //    var dbCatSetAssign = db.PC_CategorySetAssignment.FirstOrDefault(x => x.Id == id && x.DV_CompanyId == companyId && x.IsActive);
                    //    if (dbCatSetAssign != null)
                    //        doesExist = true;
                    //    break;
                    //case "pc_productcategoryassignment":
                    //case "pc_productvariantcategoryassignment":
                    //    var dbProdCatAssign = db.PC_ProductCategory.FirstOrDefault(x => x.Id == id && x.IsActive);
                    //    if (dbProdCatAssign != null)
                    //        doesExist = true;
                    //    break;
            }
        }
        #endregion

        #region Rollback Method(s)
        public async Task RollbackExternalId(long id, string tableName)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ExternalId").Name;

            // Get MongoDB
            var db = context.Database;
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, id.ToString());

            var dbVariants = await externalIdCollection.Find(filters).ToListAsync();

            // Delete
            await externalIdCollection.DeleteManyAsync(filters);
        }
        #endregion
    }
}