using Deathstar.Data.Models;
using Microsoft.Extensions.Options;
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
    /// Its the Location Model!
    /// </summary>
    public class LocationModel : ILocationModel
    {
        private readonly IBaseContextModel context;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFieldModel;

        public LocationModel(ICustomFieldModel customFieldModel, IExternalIdModel externalIdModel, IBaseContextModel contextModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            context = new Models.ContextModel(mongoDbSettings, apiSettings);
            //context = contextModel;
            _externalIdModel = externalIdModel;
            _customFieldModel = customFieldModel;
        }

        #region Get Method(s)
        /// <summary>
        /// Get all locations
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
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Location").Name;

            // Get MongoDB
            var db = context.Database;
            var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(collectionName);

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
            FilterDefinition<Deathstar.Data.Models.PC_Location> filters;

            // Handle Action
            switch (action.ToLower())
            {
                case "contains":
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_Location>(fieldContainsValue: dictionary);
                    break;
                default:
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_Location>(fieldEqValue: dictionary);
                    break;
            }

            count = locationCollection.Find(filters).CountDocuments();
            var dbLocations = locationCollection.Find(filters).SortByDescending(x => x.CreatedDateTime).Skip(start).Limit(limit).ToList();

            foreach (var dbLocation in dbLocations)
            {
                var singleResponse = new Sheev.Common.Models.GetAllResponse()
                {
                    Id = dbLocation.Id,
                    Name = dbLocation.Name,
                    Description = dbLocation.Description
                };

                response.Add(singleResponse);
            }

            return response;
        }

        /// <summary>
        /// Get a location by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.LocationResponse> GetById(long id, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Location").Name;

            // Get MongoDB
            var db = context.Database;
            var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.IsActive, true);

            var dbLocation = await locationCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbLocation == null)
            {
                string reason = $"Location not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product (id: {dbProduct.Id}, sku: {dbProduct.Sku}) successfully retrieved.", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Complete, trackingGuid);

            // Build the Response
            var response = dbLocation.ConvertToResponse();

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbLocation.Id.ToString(), "PC_Location", trackingGuid);

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new location 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.LocationResponse> Save(Tagge.Common.Models.LocationRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.LocationResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Location").Name;

                // Get MongoDB
                var db = context.Database;
                var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "location_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Word
                var dbLocation = new Deathstar.Data.Models.PC_Location();

                // Need to do this so that the lookup is not case sensitive
                var lowername = request.Name.ToLower();

                // Check to see if the Location is a duplicate
                var duplicatefilter = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_Location>.Filter.Where(x => x.Name.ToLower() == lowername);
                var duplicateLocation = locationCollection.Find(duplicatefilter).ToList();

                if (duplicateLocation != null && duplicateLocation.Count > 0)
                {
                    string reason = $"Location ({request.Name}) already exist.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location ({request.Name}) already exist! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                }

                // Get Id
                var id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Location
                dbLocation.ConvertToDatabaseObject(companyId.ToString(), request);

                // Location Groups
                if (request.LocationGroups != null && request.LocationGroups.Count > 0)
                {
                    dbLocation.LocationGroups = await LocationGroupModel.SaveOrUpdate(request.LocationGroups, new List<Deathstar.Data.Models.TM_GenericEntry>(), context,  trackingGuid);
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbLocation.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_Location", id.ToString(), trackingGuid);
                }

                // Add Id
                dbLocation.Id = id;

                // Add Created By & Timestamp
                dbLocation.CreatedBy = context.Security.GetEmail();
                dbLocation.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbLocation.IsActive = true;

                // Insert
                await locationCollection.InsertOneAsync(dbLocation);

                // Building the Response
                response = dbLocation.ConvertToResponse();

                // Add Id
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Location", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location (id: {dbLocation.Id}, sku: {dbLocation.Name}) successfully saved.", "Save Location", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Trigger the Webhook event
                //var whRequest = new Sheev.Common.Models.WebhookResponse()
                //{
                //    CompanyId = companyId.ToString(),
                //    Type = "Location",
                //    Scope = "location/created",
                //    Id = response.Id.ToString(),
                //    TrackingGuid = trackingGuid
                //};

                //await Models.WebhookModel.FireWebhookEvent(whRequest, context, trackingGuid);


                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location ( Name: {request.Name}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
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

        #region Save/Update Method(s)
        public async Task SaveUpdateGeneric(Sheev.Common.Models.GenericRequest request, Guid trackingGuid)
        {

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
        public static async Task<List<Deathstar.Data.Models.TM_GenericEntry>> SaveOrUpdate(List<GenericRequest> request, List<TM_GenericEntry> existingLocations, IBaseContextModel context, Guid trackingGuid)
        {
            // Initialize the new collection
            var dbLocations = new List<Deathstar.Data.Models.TM_GenericEntry>();

            try
            {
                // Null check so we dont throw an error after this when we try to access the collection
                if (existingLocations == null || existingLocations.Count <= 0)
                    existingLocations = new List<Deathstar.Data.Models.TM_GenericEntry>();

                foreach (var locationRequest in request)
                {
                    // Check to see if there is a duplicate custom field already in the list 
                    var dupLocation = dbLocations.FirstOrDefault(x => x.InternalId.ToLower() == locationRequest.Id.ToLower());

                    // if so error!
                    if (dupLocation != null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Duplicate location '{locationRequest.Id}' is not allowed" };

                    // Check to see if the location already exists on the collection
                    var dbLocation = existingLocations.FirstOrDefault(x => x.InternalId.ToLower() == locationRequest.Id.ToLower());

                    // Validate Id
                    if (!long.TryParse(locationRequest.Id, out long locationId))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid location id '{locationRequest.Id}'" };

                    // Validate Location
                    string locationName = await Validate(locationId, context, trackingGuid);

                    // if it does then update else add <- yes that is backwards from the logic below... deal with it
                    if (dbLocation == null)
                    {
                        dbLocation = new Deathstar.Data.Models.TM_GenericEntry();
                        dbLocation.ConvertToDatabaseObject(locationRequest);

                        dbLocation.Name = locationName;
                    }
                    else
                    {
                        dbLocation.InternalId = locationRequest.Id;
                        dbLocation.Name = locationName;
                    }

                    // Add the custom field back into the collection
                    dbLocations.Add(dbLocation);
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
        /// Updates an existing location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.LocationResponse> Update(long id, Tagge.Common.Models.LocationRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.LocationResponse();
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Location").Name;

            // Get MongoDB
            var db = context.Database;
            var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            // Word
            var dbLocation = new Deathstar.Data.Models.PC_Location();

            // Check to see if the Location exists
            var existsfilter = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, id);
            existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.IsActive, true);
            var dbExistingLocation = await locationCollection.FindAsync(existsfilter).Result.FirstOrDefaultAsync();

            if (dbExistingLocation == null)
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Location ({id}) Not Found." };

            // Convert to DB Object
            dbLocation.ConvertToDatabaseObject(companyId.ToString(), request);

            // Check for empty collections and null them
            //dbLocation.CheckForEmptyCollections();

            // Add Updated By & Timestamp
            dbLocation.UpdatedBy = context.Security.GetEmail();
            dbLocation.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            // Location Groups
            if (request.LocationGroups != null && request.LocationGroups.Count > 0)
            {
                dbLocation.LocationGroups = await LocationGroupModel.SaveOrUpdate(request.LocationGroups, dbExistingLocation.LocationGroups, context, trackingGuid);
            }

            // Custom Fields 
            if (request.CustomFields != null && request.CustomFields.Count > 0)
            {
                dbLocation.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbExistingLocation.CustomFields, "PC_Location", id.ToString(), trackingGuid);
            }

            // filter
            var filter = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, id);

            var serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocation, serializerSettings)) } };

            // Update database record
            await locationCollection.UpdateOneAsync(filter, update);

            // Convert To Response
            response = dbLocation.ConvertToResponse();

            // Add Id
            response.Id = id;

            // ExternalIds 
            if (request.ExternalIds != null && request.ExternalIds.Count > 0)
            {
                response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Location", id.ToString(), trackingGuid);
            }

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
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or Reactivate a Location
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
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Location").Name;

            // Get MongoDB
            var db = context.Database;
            var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

            var dbLocation = locationCollection.Find(filters).FirstOrDefault();

            if (dbLocation == null)
            {
                string reason = $"Location not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Set the updated timestamp here
            string timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_Location>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await locationCollection.UpdateOneAsync(filters, update);

            // Update the corresponding External Id record(s)
            await _externalIdModel.DeleteOrReactivateByParentId(dbLocation.Id.ToString(), reactivate, "PC_Location", timestamp, dbLocation.UpdatedDateTime, trackingGuid, "");

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion 

        #region Validate Method(s)
        /// <summary>
        /// Validates a set of generic request locations
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task Validate(List<Sheev.Common.Models.GenericRequest> locations, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var locationCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Location>("PC_Location");

            if (locations != null && locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    Int64.TryParse(location.Id, out long longId);

                    // Filter
                    var filters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, longId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                    var dbLocation = await locationCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                    if (dbLocation == null)
                    {
                        string reason = $"Location not found by Id:{longId} provided.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                    }
                }
            }
        }

        /// <summary>
        /// Validate a single location by id
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<string> Validate(long locationId, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var locationCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Location>("PC_Location");

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, locationId);
            filters = filters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbLocation = await locationCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbLocation == null)
            {
                string reason = $"Location not found by Id:{locationId} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbLocation.Name;
        }
        #endregion
    }
}
