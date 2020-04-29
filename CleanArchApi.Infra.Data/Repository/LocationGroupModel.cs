using Deathstar.Data.Models;
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

    /// <summary>
    /// Its the Location Group Model!
    /// </summary>
    public class LocationGroupModel : ILocationGroupModel
    {
        private readonly IBaseContextModel context;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFieldModel;

        public LocationGroupModel(ICustomFieldModel customFieldModel, IExternalIdModel externalIdModel, IBaseContextModel contextModel, ICategoryAssignmentModel categoryAssignmentModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            ////context = contextModel;
            context = new Models.ContextModel(mongoDbSettings, apiSettings);
            _externalIdModel = externalIdModel;
            _customFieldModel = customFieldModel;
        }

        #region Get Method(s)
        /// <summary>
        /// Get all location groups
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="context"></param>
        /// <param name="count"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Sheev.Common.Models.GetAllResponse> GetAll(string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100)
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
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_LocationGroup").Name;

            // Get MongoDB
            var db = context.Database;
            var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(collectionName);

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
            FilterDefinition<Deathstar.Data.Models.PC_LocationGroup> filters;

            // Handle Action
            switch (action.ToLower())
            {
                case "contains":
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_LocationGroup>(fieldContainsValue: dictionary);
                    break;
                default:
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_LocationGroup>(fieldEqValue: dictionary);
                    break;
            }

            count = locationGroupCollection.Find(filters).CountDocuments();
            var dbLocationGroups = locationGroupCollection.Find(filters).SortByDescending(x => x.CreatedDateTime).Skip(start).Limit(limit).ToList();

            foreach (var dbLocationGroup in dbLocationGroups)
            {
                var singleResponse = new Sheev.Common.Models.GetAllResponse()
                {
                    Id = dbLocationGroup.Id,
                    Name = dbLocationGroup.Name,
                    Description = dbLocationGroup.Description
                };

                response.Add(singleResponse);
            }

            return response;
        }

        /// <summary>
        /// Get a location group by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.LocationGroupResponse> GetById(long id, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_LocationGroup").Name;

            // Get MongoDB
            var db = context.Database;
            var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.IsActive, true);

            var dbLocationGroup = await locationGroupCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbLocationGroup == null)
            {
                string reason = $"Location Group not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (id: {dbProduct.Id}, sku: {dbProduct.Sku}) successfully retrieved.", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Complete, trackingGuid);

            // Build the Response
            var response = dbLocationGroup.ConvertToResponse();

            // Add Id 
            response.Id = dbLocationGroup.Id;

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbLocationGroup.Id.ToString(), "PC_LocationGroup", trackingGuid);

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new location group 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.LocationGroupResponse> Save(Tagge.Common.Models.LocationGroupRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.LocationGroupResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_LocationGroup").Name;

                // Get MongoDB
                var db = context.Database;
                var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "locationgroup_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Word
                var dbLocationGroup = new Deathstar.Data.Models.PC_LocationGroup();

                // Need to do this so that the lookup is not case sensitive
                var lowername = request.Name.ToLower();

                // Check to see if the Location is a duplicate
                var duplicatefilter = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Where(x => x.Name.ToLower() == lowername);
                var duplicateLocation = locationGroupCollection.Find(duplicatefilter).ToList();

                if (duplicateLocation != null && duplicateLocation.Count > 0)
                {
                    string reason = $"Location Group ({request.Name}) already exist.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group ({request.Name}) already exist! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                }

                // Get Id
                var id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Location
                dbLocationGroup.ConvertToDatabaseObject(companyId.ToString(), request);

                // Locations
                if (request.Locations != null && request.Locations.Count > 0)
                {
                    dbLocationGroup.Locations = await LocationModel.SaveOrUpdate(request.Locations, new List<Deathstar.Data.Models.TM_GenericEntry>(), context, trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    ////dbLocationGroup.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_LocationGroup", id.ToString(), trackingGuid);
                }

                // Add Id
                dbLocationGroup.Id = id;

                // Add Created By & Timestamp
                dbLocationGroup.CreatedBy = context.Security.GetEmail();
                dbLocationGroup.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbLocationGroup.IsActive = true;

                // Insert
                await locationGroupCollection.InsertOneAsync(dbLocationGroup);

                // Building the Response
                response = dbLocationGroup.ConvertToResponse();

                // Add Id
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_LocationGroup", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group (id: {dbLocationGroup.Id}, sku: {dbLocationGroup.Name}) successfully saved.", "Save Location", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

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

        #region Save Or Update Method(s)
        /// <summary>
        /// Save or Update Locations
        /// </summary>
        /// <param name="request"></param>
        /// <param name="existingLocations"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<List<Deathstar.Data.Models.TM_GenericEntry>> SaveOrUpdate(List<Sheev.Common.Models.GenericRequest> request, List<Deathstar.Data.Models.TM_GenericEntry> existingLocations, IBaseContextModel context, Guid trackingGuid)
        {
            // Initialize the new collection
            var dbLocations = new List<Deathstar.Data.Models.TM_GenericEntry>();

            try
            {
                // Null check so we dont throw an error after this when we try to access the collection
                if (existingLocations == null || existingLocations.Count <= 0)
                    existingLocations = new List<Deathstar.Data.Models.TM_GenericEntry>();

                foreach (var locationGroupRequest in request)
                {
                    // Check to see if there is a duplicate custom field already in the list 
                    var dupLocationGroup = dbLocations.FirstOrDefault(x => x.InternalId.ToLower() == locationGroupRequest.Id.ToLower());

                    // if so error!
                    if (dupLocationGroup != null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Duplicate location group '{locationGroupRequest.Id}' is not allowed" };

                    // Check to see if the location already exists on the collection
                    var dbLocationGroup = existingLocations.FirstOrDefault(x => x.InternalId.ToLower() == locationGroupRequest.Id.ToLower());

                    // Validate Id
                    if (!long.TryParse(locationGroupRequest.Id, out long locationId))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid location id group '{locationGroupRequest.Id}'" };

                    // Validate Location Group
                    string locationGroupName = await Validate(locationId, context, trackingGuid);

                    // if it does then update else add <- yes that is backwards from the logic below... deal with it
                    if (dbLocationGroup == null)
                    {
                        dbLocationGroup = new Deathstar.Data.Models.TM_GenericEntry();
                        dbLocationGroup.ConvertToDatabaseObject(locationGroupRequest);
                        dbLocationGroup.Name = locationGroupName;
                    }
                    else
                    {
                        dbLocationGroup.InternalId = locationGroupRequest.Id;
                        dbLocationGroup.Name = locationGroupName;
                    }

                    // Add the custom field back into the collection
                    dbLocations.Add(dbLocationGroup);
                }

                return dbLocations;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "LocationModel.UpdateGenericCustomField()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Updates an existing location group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.LocationGroupResponse> Update(long id, Tagge.Common.Models.LocationGroupRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.LocationGroupResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_LocationGroup").Name;

                // Get MongoDB
                var db = context.Database;
                var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(collectionName);

                // Word
                var dbLocationGroup = new Deathstar.Data.Models.PC_LocationGroup();

                // Check to see if the Location is a duplicate
                var filters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.IsActive, true);
                filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                // Does the location group already exist
                var dbExistingLocationGroup = await locationGroupCollection.Find(filters).FirstOrDefaultAsync();

                if (dbExistingLocationGroup == null)
                {
                    string reason = $"Location Group not found by Id: {id}. Use Post to create a new location group";

                    //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group ({request.Name}) already exist! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                // Convert request to Location
                dbLocationGroup.ConvertToDatabaseObject(companyId.ToString(), request);

                // Locations
                if (request.Locations != null && request.Locations.Count > 0)
                {
                    dbLocationGroup.Locations = await LocationModel.SaveOrUpdate(request.Locations, dbExistingLocationGroup.Locations, context, trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    ////dbLocationGroup.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingLocationGroup.CustomFields, "PC_LocationGroup", id.ToString(), trackingGuid);
                }

                // Add Created By & Timestamp
                dbLocationGroup.UpdatedBy = context.Security.GetEmail();
                dbLocationGroup.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbLocationGroup.IsActive = true;

                // Set id to 0 due to serializer
                dbLocationGroup.Id = 0;

                // Update
                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocationGroup, serializerSettings)) } };

                // Update database record
                await locationGroupCollection.UpdateOneAsync(filters, update);

                // Building the Response
                response = dbLocationGroup.ConvertToResponse();

                // Add Id
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_LocationGroup", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group (id: {dbLocationGroup.Id}, sku: {dbLocationGroup.Name}) successfully saved.", "Save Location", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

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
        /// Delete or Reactivate a location group
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
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_LocationGroup").Name;

            // Get MongoDB
            var db = context.Database;
            var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbLocationGroup = locationGroupCollection.Find(filters).FirstOrDefault();

            if (dbLocationGroup == null)
            {
                string reason = $"Location Group not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_LocationGroup>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz"));

            // Update database record
            await locationGroupCollection.UpdateOneAsync(filters, update);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion 

        #region Validate Method(s)
        /// <summary>
        /// Validates a set of generic request location groups
        /// </summary>
        /// <param name="locationGroups"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task Validate(List<Sheev.Common.Models.GenericRequest> locationGroups, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var locationGroupCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_LocationGroup>("PC_LocationGroup");

            if (locationGroups != null && locationGroups.Count > 0)
            {
                foreach (var locationGroup in locationGroups)
                {
                    Int64.TryParse(locationGroup.Id, out long longId);

                    // Filter
                    var filters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, longId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                    var dbLocationGroup = await locationGroupCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                    if (dbLocationGroup == null)
                    {
                        string reason = $"Location group not found by Id:{longId} provided.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location group was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }
                }
            }
        }

        /// <summary>
        /// Validate by Id
        /// </summary>
        /// <param name="locationGroups"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<string> Validate(long locationGroupId, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            // Location Group Table
            var locationGroupCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_LocationGroup>("PC_LocationGroup");

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, locationGroupId);
            filters = filters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbLocationGroup = await locationGroupCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbLocationGroup == null)
            {
                string reason = $"Location group not found by Id:{locationGroupId} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location group was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbLocationGroup.Name;
        }
        #endregion
    }
}