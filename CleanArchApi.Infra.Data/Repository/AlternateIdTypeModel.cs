using Deathstar.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
    public class AlternateIdTypeModel :IAlternateIdTypeModel
    {
        private readonly IMongoCollection<PC_AlternateIdType> _alternateIdTypeCollection;
        private readonly IBaseContextModel context;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFieldModel;

        public AlternateIdTypeModel(ICustomFieldModel customFieldModel, IExternalIdModel externalIdModel, IBaseContextModel contextModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            //context = contextModel;
            context = new Models.ContextModel(mongoDbSettings, apiSettings);
            _alternateIdTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>("PC_AlternateIdType");
            _externalIdModel = externalIdModel;
            _customFieldModel = customFieldModel;
        }

        #region Get Method(s)
        /// <summary>
        /// Get all categories
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="context"></param>
        /// <param name="count"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Sheev.Common.Models.GetAllResponse> GetAll(IBaseContextModel context, string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100)
        {
            // Paging
            int limit = (int)pageSize;
            int start = (int)pageNumber == 1 ? 0 : (int)(pageNumber - 1) * limit;

            // Response
            List<Sheev.Common.Models.GetAllResponse> response = new List<Sheev.Common.Models.GetAllResponse>();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Dictonary
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Action
            string action = string.Empty;

            // MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_AlternateIdType").Name;

            //// Get MongoDB
            //var db = context.Database;
            //var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(collectionName);

            // Add Company Id filter
            dictionary.Add("DV_CompanyId", companyId.ToString());

            // Filter
            if (!string.IsNullOrEmpty(queryString))
            {
                Utilities.MongoFilter.BuildFilterDictionary(queryString, ref action, ref dictionary);
            }

            // Check for invalid filters
            //CheckForInvalidFilters(dictionary);

            // Filter Builder
            FilterDefinition<Deathstar.Data.Models.PC_AlternateIdType> filters;

            // Handle Action
            switch (action.ToLower())
            {
                case "contains":
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_AlternateIdType>(fieldContainsValue: dictionary);
                    break;
                default:
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_AlternateIdType>(fieldEqValue: dictionary);
                    break;
            }

            count = _alternateIdTypeCollection.Find(filters).CountDocuments();
            var dbAlternateIdTypes = _alternateIdTypeCollection.Find(filters).SortByDescending(x => x.CreatedDateTime).Skip(start).Limit(limit).ToList();

            foreach (var dbAlternateIdType in dbAlternateIdTypes)
            {
                var singleResponse = new Sheev.Common.Models.GetAllResponse()
                {
                    Id = dbAlternateIdType.Id,
                    Name = dbAlternateIdType.Name,
                    Description = dbAlternateIdType.Description
                };

                response.Add(singleResponse);
            }

            return response;
        }

        /// <summary>
        /// Get a Alternate Id Type by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.AlternateIdTypeResponse> GetById(IBaseContextModel context, long id, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_AlternateIdType").Name;

            // Get MongoDB
            var db = context.Database;
            var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbAlternateIdType = await alternateIdTypeCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbAlternateIdType == null)
            {
                string reason = $"Alternate Id Type not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type (id: {dbAlternateIdType.Id}) successfully retrieved.", "Get Alternate Id Type", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

            // Build the Response
            var response = dbAlternateIdType.ConvertToResponse();

            // Add Id 
            response.Id = dbAlternateIdType.Id;

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbAlternateIdType.Id.ToString(), "PC_AlternateIdType", trackingGuid);

            return response;
        }
        #endregion

        #region Save Method(s)
        public async Task<Tagge.Common.Models.AlternateIdTypeResponse> Save(IBaseContextModel context, Tagge.Common.Models.AlternateIdTypeRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.AlternateIdTypeResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_AlternateIdType").Name;

                // Get MongoDB
                var db = context.Database;
                var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Word
                var dbAlternateIdType = new Deathstar.Data.Models.PC_AlternateIdType();

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "alternateidtype_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Get Id
                var id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to AlternateIdType
                dbAlternateIdType.ConvertToDatabaseObject(companyId.ToString(), request);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbAlternateIdType.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_AlternateIdType", id.ToString(), trackingGuid);
                }

                // Add Id
                dbAlternateIdType.Id = id;

                // Add Created By & Timestamp
                dbAlternateIdType.CreatedBy = context.Security.GetEmail();
                dbAlternateIdType.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                dbAlternateIdType.IsActive = true;

                // Insert
                await alternateIdTypeCollection.InsertOneAsync(dbAlternateIdType);

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type (id: {dbAlternateIdType.Id}) successfully saved.", "Save Alternate Id Type", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Building the Response
                response = dbAlternateIdType.ConvertToResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_AlternateIdType", id.ToString(), trackingGuid);
                }

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "AlternateIdTypeModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Updates an existing Alternate Id Type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.AlternateIdTypeResponse> Update(IBaseContextModel context, long id, Tagge.Common.Models.AlternateIdTypeRequest request, Guid trackingGuid)
        {
            // Response
            var response = new Tagge.Common.Models.AlternateIdTypeResponse();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_AlternateIdType").Name;

                // Get MongoDB
                var db = context.Database;
                var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(collectionName);

                // Word
                var dbAlternateIdType = new Deathstar.Data.Models.PC_AlternateIdType();

                // Build the filters that will be used for both updating the record and checking to see if it exists
                // Check to see if the Alternate Id Type exists
                var filters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.IsActive, true);

                // Find that alternate id type
                var dbExistingAlternateIdType = alternateIdTypeCollection.Find(filters).FirstOrDefault();

                // Could it be missing? if so throw error
                if (dbExistingAlternateIdType == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Alternate Id Type not found by Id: {id}" };

                // Convert to DB Object
                dbAlternateIdType.ConvertToDatabaseObject(companyId.ToString(), request);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbAlternateIdType.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingAlternateIdType.CustomFields, "PC_AlternateIdType", id.ToString(), trackingGuid);
                }

                // Add Updated By & Timestamp
                dbAlternateIdType.UpdatedBy = context.Security.GetEmail();
                dbAlternateIdType.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbAlternateIdType, serializerSettings)) } };

                // Update database record
                await alternateIdTypeCollection.UpdateOneAsync(filters, update);

                // Convert To Response
                response = dbAlternateIdType.ConvertToResponse();

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_AlternateIdType", id.ToString(), trackingGuid);
                }

                // Add Id
                response.Id = id;

                ////Trigger the Webhook event
                //if (_useWebhook)
                //{
                //    var whRequest = new WebhookResponse()
                //    {
                //        CompanyId = companyId.ToString(),
                //        Type = "Product",
                //        Scope = "product/updated",
                //        Id = response.Id.ToString()
                //    };

                //    Models.WebhookModel.WebhookTriggerEvent(whRequest, trackingGuid);
                //}

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group ( Name: {request.Name}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "LocationGroupModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or Reactivate a Alternate Id Type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(IBaseContextModel context, long id, bool reactivate, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_AlternateIdType").Name;

            // Get MongoDB
            var db = context.Database;
            var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbAlternateIdType = alternateIdTypeCollection.Find(filters).FirstOrDefault();

            if (dbAlternateIdType == null)
            {
                string reason = $"Alternate Id Type not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz"));

            // Update database record
            await alternateIdTypeCollection.UpdateOneAsync(filters, update);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        /// <summary>
        /// Validate a single Alternate Id Type by id
        /// </summary>
        /// <param name="alternateIdTypeId"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task Validate(IBaseContextModel context, long alternateIdTypeId, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var alternateIdTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>("PC_AlternateIdType");

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, alternateIdTypeId);
            filters = filters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbAlternateIdType = await alternateIdTypeCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbAlternateIdType == null)
            {
                string reason = $"Alternate Id Type not found by Id:{alternateIdTypeId} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Alternate Id Type was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion
    }
}
