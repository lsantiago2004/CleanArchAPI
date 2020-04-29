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
    /// <summary>
    /// Its the Category Set Model!
    /// </summary>
    public class CategorySetModel : ICategorySetModel
    {
        private readonly IMongoCollection<PC_CategorySet> _categorySetCollection;
        private readonly IBaseContextModel context;
        //private readonly ICategorySetModel _categorySet;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFielModel;
        ////public CategorySetModel(ICustomFieldModel customFielModel, IExternalIdModel externalIdModel, IBaseContextModel contextModel)//, ICategorySetModel categorySet)
        public CategorySetModel(ICustomFieldModel customFielModel, IExternalIdModel externalIdModel, IBaseContextModel contextModel)//, ICategorySetModel categorySet)
        {
            context = contextModel;
            _categorySetCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_CategorySet>("PC_CategorySet");
            //_categorySet = categorySet;
            _externalIdModel = externalIdModel;
            _customFielModel = customFielModel;
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
            FilterDefinition<Deathstar.Data.Models.PC_CategorySet> filters;

            // Handle Action
            switch (action.ToLower())
            {
                case "contains":
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_CategorySet>(fieldContainsValue: dictionary);
                    break;
                default:
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_CategorySet>(fieldEqValue: dictionary);
                    break;
            }

            count = _categorySetCollection.Find(filters).CountDocuments();
            var dbCategorySets = _categorySetCollection.Find(filters).SortByDescending(x => x.CreatedDateTime).Skip(start).Limit(limit).ToList();

            foreach (var dbCategorySet in dbCategorySets)
            {
                var singleResponse = new Sheev.Common.Models.GetAllResponse()
                {
                    Id = dbCategorySet.Id,
                    Name = dbCategorySet.Name,
                    Description = dbCategorySet.Description
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
        public async Task<Tagge.Common.Models.CategorySetResponse> GetById(IBaseContextModel context, long id, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            //// MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_CategorySet").Name;

            //// Get MongoDB
            //var db = context.Database;
            //var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbCategorySet = await _categorySetCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbCategorySet == null)
            {
                string reason = $"Category Set not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set (id: {dbCategorySet.Id}, name: {dbCategorySet.Name}) successfully retrieved.", "Get Category Set", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

            // Build the Response
            var response = dbCategorySet.ConvertToResponse();

            // Add Id 
            response.Id = dbCategorySet.Id;

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbCategorySet.Id.ToString(), "PC_CategorySet", trackingGuid);

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new category set 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.CategorySetResponse> Save(IBaseContextModel context, Tagge.Common.Models.CategorySetRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.CategorySetResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                //// MongoDB Settings
                //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_CategorySet").Name;

                //// Get MongoDB
                //var db = context.Database;
                //var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(collectionName);
                var counterCollection = context.Database.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Word
                var dbCategorySet = new Deathstar.Data.Models.PC_CategorySet();

                // Check to see if the Category Set is a duplicate
                var duplicatefilter = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Name, request.Name);
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);
                var duplicateCategory = _categorySetCollection.Find(duplicatefilter).FirstOrDefault();

                if (duplicateCategory != null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Category Set Already Exists. Use PUT to update category. (Id = {duplicateCategory.Id})!" };

                // Validate Category Sets
                await Validate(context, request.Categories, trackingGuid);////new CategoryModel(context).Validate(request.Categories, trackingGuid);

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "categoryset_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Get Id
                var id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Category
                dbCategorySet.ConvertToDatabaseObject(companyId.ToString(), request);

                // Categories
                if (request.Categories != null && request.Categories.Count > 0)
                {
                    foreach (var category in request.Categories)
                    {
                        dbCategorySet.Categories.Add(new Deathstar.Data.Models.TM_GenericEntry()
                        {
                            Name = category.Name,
                            InternalId = category.Id
                        });
                    }
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbCategorySet.CustomFields = await _customFielModel.SaveGenericCustomField(request.CustomFields, "PC_CategorySet", id.ToString(), trackingGuid);
                }

                // Add Id
                dbCategorySet.Id = id;

                // Add Created By & Timestamp
                dbCategorySet.CreatedBy = context.Security.GetEmail();
                dbCategorySet.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                dbCategorySet.IsActive = true;

                // Insert
                await _categorySetCollection.InsertOneAsync(dbCategorySet);

                // Building the Response
                response = dbCategorySet.ConvertToResponse();

                // Add Id
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_CategorySet", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set (id: {dbCategorySet.Id}, name: {dbCategorySet.Name}) successfully saved.", "Save Category Set", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Trigger the Webhook event
                //if (_useWebhook)
                //{
                //    var whRequest = new WebhookResponse()
                //    {
                //        CompanyId = companyId.ToString(),
                //        Type = "Product Category",
                //        Scope = "category/product/created",
                //        Id = response.Id.ToString(),
                //        TrackingGuid = trackingGuid
                //    };

                //    WebhookModel.WebhookTriggerEvent(whRequest, trackingGuid);
                //}

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set ( Name: {request.Name}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CategorySet.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }

        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Updates an existing category set 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        //public async Task<Tagge.Common.Models.CategorySetResponse> Update(long id, Tagge.Common.Models.CategorySetRequest request, Models.ContextModel context, Guid trackingGuid)
        public async Task<Tagge.Common.Models.CategorySetResponse> Update(IBaseContextModel context, long id, Tagge.Common.Models.CategorySetRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.CategorySetResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                //// MongoDB Settings
                //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_CategorySet").Name;

                //// Get MongoDB
                //var db = context.Database;
                //var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(collectionName);
                var counterCollection = context.Database.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Word
                var dbCategorySet = new Deathstar.Data.Models.PC_CategorySet();

                // Check to see if the Category Set is a duplicate
                var duplicatefilter = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, id);
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);
                var categoryExists = _categorySetCollection.Find(duplicatefilter).FirstOrDefault();

                if (categoryExists == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Category Set Already Exists. Use PUT to update category. (Id = {categoryExists.Id})!" };

                // Validate Category Sets
                await Validate(context, request.Categories, trackingGuid);////new CategoryModel(context).Validate(request.Categories, trackingGuid);

                // Convert request to Category
                dbCategorySet.ConvertToDatabaseObject(companyId.ToString(), request);

                // Categories
                if (request.Categories != null && request.Categories.Count > 0)
                {
                    foreach (var category in request.Categories)
                    {
                        dbCategorySet.Categories.Add(new Deathstar.Data.Models.TM_GenericEntry()
                        {
                            Name = category.Name,
                            InternalId = category.Id
                        });
                    }
                }

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbCategorySet.CustomFields = await _customFielModel.SaveOrUpdateGenericCustomFields(request.CustomFields, categoryExists.CustomFields, "PC_CategorySet", id.ToString(), trackingGuid);
                }

                // Add Created By & Timestamp
                dbCategorySet.UpdatedBy = context.Security.GetEmail();
                dbCategorySet.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                dbCategorySet.IsActive = true;

                // filter
                var filters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);
                filters = filters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategorySet, serializerSettings)) } };


                // Save or Update database record
                await _categorySetCollection.UpdateOneAsync(filters, update);

                // Building the Response
                response = dbCategorySet.ConvertToResponse();

                // Add Id
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_CategorySet", id.ToString(), trackingGuid);
                }


                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set (id: {dbCategorySet.Id}, name: {dbCategorySet.Name}) successfully saved.", "Save Category Set", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Trigger the Webhook event
                //if (_useWebhook)
                //{
                //    var whRequest = new WebhookResponse()
                //    {
                //        CompanyId = companyId.ToString(),
                //        Type = "Product Category",
                //        Scope = "category/product/created",
                //        Id = response.Id.ToString(),
                //        TrackingGuid = trackingGuid
                //    };

                //    WebhookModel.WebhookTriggerEvent(whRequest, trackingGuid);
                //}

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set ( Name: {request.Name}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CategorySet.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or Reactivate a category set
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteOrReactivate(IBaseContextModel context, long id, bool reactivate, Guid trackingGuid)
        {
            var companyId = context.Security.GetCompanyId();

            //// MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_CategorySet").Name;

            //// Get MongoDB
            //var db = context.Database;
            //var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbCategorySet = _categorySetCollection.Find(filters).FirstOrDefault();

            if (dbCategorySet == null)
            {
                string reason = $"Location Group not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Location Group was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_CategorySet>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz"));

            // Update database record
            await _categorySetCollection.UpdateOneAsync(filters, update);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        /// <summary>
        /// Validates a set of generic request category sets
        /// </summary>
        /// <param name="categorySets"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task Validate(IBaseContextModel context, List<Sheev.Common.Models.GenericRequest> categorySets, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            //var categorySetCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_CategorySet>("PC_CategorySet");

            if (categorySets != null && categorySets.Count > 0)
            {
                foreach (var categorySet in categorySets)
                {
                    Int64.TryParse(categorySet.Id, out long longId);

                    // Filter
                    var filters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, longId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                    var dbCategory = await _categorySetCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                    if (dbCategory == null)
                    {
                        string reason = $"Category Set not found by Id:{longId} provided.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category Set was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }
                }
            }
        }
        #endregion
    }
}
