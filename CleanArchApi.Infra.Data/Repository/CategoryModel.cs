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
    /// Its the Category Model!
    /// </summary>
    public class CategoryModel : ICategoryModel
    {
        private readonly IMongoCollection<PC_Category> _categoryCollection;
        private readonly IBaseContextModel context;
        private readonly ICategorySetModel _categorySetModel;
        private readonly ICategoryAssignmentModel _categoryAssignment;
        private readonly IWebhookModel _webHookModel;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ICustomFieldModel _customFieldModel;

        //cr (circular reference)
        ////public CategoryModel(ICustomFieldModel customFieldModel, IBaseContextModel contextModel, IExternalIdModel externalIdModel, IWebhookModel webHookModel, ICategoryAssignmentModel categoryAssignment, ICategorySetModel categorySetModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        public CategoryModel(ICategoryAssignmentModel categoryAssignment, ICustomFieldModel customFieldModel, IBaseContextModel contextModel, IExternalIdModel externalIdModel, IWebhookModel webHookModel, ICategorySetModel categorySetModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            context = contextModel;
            /////context = new Models.ContextModel(mongoDbSettings, apiSettings);
            _categoryCollection = contextModel.Database.GetCollection<Deathstar.Data.Models.PC_Category>("PC_Category");
            _categorySetModel = categorySetModel;
            _categoryAssignment = categoryAssignment;
            _webHookModel = webHookModel;
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

            //// MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Category").Name;

            //// Get MongoDB
            //var db = context.Database;
            //var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(collectionName);

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
            FilterDefinition<Deathstar.Data.Models.PC_Category> filters;

            // Handle Action
            switch (action.ToLower())
            {
                case "contains":
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_Category>(fieldContainsValue: dictionary);
                    break;
                default:
                    filters = Utilities.MongoFilter.BuildFilters<Deathstar.Data.Models.PC_Category>(fieldEqValue: dictionary);
                    break;
            }

            count = _categoryCollection.Find(filters).CountDocuments();
            var dbCategories = _categoryCollection.Find(filters).SortByDescending(x => x.CreatedDateTime).Skip(start).Limit(limit).ToList();

            foreach (var dbCategory in dbCategories)
            {
                var singleResponse = new Sheev.Common.Models.GetAllResponse()
                {
                    Id = dbCategory.Id,
                    Name = dbCategory.Name,
                    Description = dbCategory.Description
                };

                response.Add(singleResponse);
            }

            return response;
        }

        /// <summary>
        /// Get an existing category by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.CategoryResponse> GetById(IBaseContextModel context, long id, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            //// MongoDB Settings
            //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Category").Name;

            //// Get MongoDB
            //var db = context.Database;
            //var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbCategory = await _categoryCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbCategory == null)
            {
                string reason = $"Category not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category (id: {dbCategory.Id}, name: {dbCategory.Name}) successfully retrieved.", "Get Category", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

            // Build the Response
            var response = dbCategory.ConvertToResponse();

            // Add Id
            response.Id = dbCategory.Id;

            // External Ids
            response.ExternalIds = await _externalIdModel.GetByParentId(dbCategory.Id.ToString(), "PC_Category", trackingGuid);

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Save a new Category
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.CategoryResponse> Save(IBaseContextModel context, Tagge.Common.Models.CategoryRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.CategoryResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                //// MongoDB Settings
                //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Category").Name;

                //// Get MongoDB
                var db = context.Database;
                //var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(collectionName);
                var counterCollection = db.GetCollection<Deathstar.Data.Models.Counter>("Counters");

                // Word
                var dbCategory = new Deathstar.Data.Models.PC_Category();

                // Check to see if the parent id exists
                if (request.ParentId.HasValue && request.ParentId.Value > 0)
                {
                    var parentCategoryFilter = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, request.ParentId.Value);
                    parentCategoryFilter = parentCategoryFilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    parentCategoryFilter = parentCategoryFilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);
                    var parentCategory = _categoryCollection.Find(parentCategoryFilter).FirstOrDefault();

                    if (parentCategory == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Category Parent not found by Id: {request.ParentId}" };
                }

                // Check to see if the Category is a duplicate
                var duplicatefilter = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Name, request.Name);
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.ParentId, request.ParentId);
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                duplicatefilter = duplicatefilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);
                var duplicateCategory = _categoryCollection.Find(duplicatefilter).FirstOrDefault();

                if (duplicateCategory != null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Category Already Exists. Use PUT to update category. (Id = {duplicateCategory.Id})!" };

                // Validate Category Sets
                await _categorySetModel.Validate(context, request.CategorySets, trackingGuid); //new CategorySetModel(context).Validate(request.CategorySets, trackingGuid);

                // filter
                var filter = Builders<Deathstar.Data.Models.Counter>.Filter.Eq(x => x.Id, "category_id");
                var update = Builders<Deathstar.Data.Models.Counter>.Update.Inc("Seq", 1);

                // Get Id
                var id = counterCollection.FindOneAndUpdate(filter, update).Seq;

                // Convert request to Category
                dbCategory.ConvertToDatabaseObject(companyId.ToString(), request);

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbCategory.CustomFields = await _customFieldModel.SaveGenericCustomField(request.CustomFields, "PC_Category", id.ToString(), trackingGuid);
                }

                // Add Id
                dbCategory.Id = id;

                // Add Created By & Timestamp
                dbCategory.CreatedBy = context.Security.GetEmail();
                dbCategory.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // IsActive
                dbCategory.IsActive = true;

                // Insert
                await _categoryCollection.InsertOneAsync(dbCategory);

                // Building the Response
                response = dbCategory.ConvertToResponse();

                // Add Id 
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Category", id.ToString(), trackingGuid);
                }

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category (id: {dbCategory.Id}, name: {dbCategory.Name}) successfully saved.", "Save Category", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

                // Build the Webhook event
                var whRequest = new Sheev.Common.Models.WebhookResponse()
                {
                    CompanyId = companyId.ToString(),
                    Type = "Product Category",
                    Scope = "category/product/created",
                    Id = response.Id.ToString(),
                    TrackingGuid = trackingGuid
                };

                // Trigger the Webhook event
                await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);


                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category ( Name: {request.Name}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CategoryModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }

        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.CategoryResponse> Update(IBaseContextModel context, long id, Tagge.Common.Models.CategoryRequest request, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.CategoryResponse();
            var companyId = context.Security.GetCompanyId();

            try
            {
                //// MongoDB Settings
                //var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                //string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Category").Name;

                //// Get MongoDB
                //var db = context.Database;
                //var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(collectionName);

                // Word
                var dbCategory = new Deathstar.Data.Models.PC_Category();

                // Check to see if the Category exists
                var existsfilter = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, id);
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                existsfilter = existsfilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);
                var existsCategory = _categoryCollection.Find(existsfilter).FirstOrDefault();

                if (existsCategory == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Category not found by Id: {id}. Use Post to create a new category" };

                // Check to see if the parent id exists
                if (request.ParentId.HasValue && request.ParentId.Value > 0)
                {
                    var parentCategoryFilter = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, request.ParentId.Value);
                    parentCategoryFilter = parentCategoryFilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    parentCategoryFilter = parentCategoryFilter & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);
                    var parentCategory = _categoryCollection.Find(parentCategoryFilter).FirstOrDefault();

                    if (parentCategory == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Category Parent not found by Id: {request.ParentId}" };
                }

                // Convert to DB Object
                dbCategory.ConvertToDatabaseObject(companyId.ToString(), request);

                // Check for empty collections and null them
                //dbCategory.CheckForEmptyCollections();

                // Add Updated By & Timestamp
                dbCategory.UpdatedBy = context.Security.GetEmail();
                dbCategory.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                // Custom Fields 
                if (request.CustomFields != null && request.CustomFields.Count > 0)
                {
                    dbCategory.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(request.CustomFields, dbCategory.CustomFields, "PC_Category", id.ToString(), trackingGuid);
                }

                // Filter
                var filters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);
                filters = filters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                // Update
                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategory, serializerSettings)) } };

                // Update database record
                await _categoryCollection.UpdateOneAsync(filters, update);

                // Convert To Response
                response = dbCategory.ConvertToResponse();

                // Add Id
                response.Id = id;

                // ExternalIds 
                if (request.ExternalIds != null && request.ExternalIds.Count > 0)
                {
                    response.ExternalIds = await _externalIdModel.SaveOrUpdateGenericExternalId(request.ExternalIds, "PC_Category", id.ToString(), trackingGuid);
                }

                // Build the Webhook event
                var whRequest = new Sheev.Common.Models.WebhookResponse()
                {
                    CompanyId = companyId.ToString(),
                    Type = "Product Category",
                    Scope = "category/product/updated",
                    Id = response.Id.ToString(),
                    TrackingGuid = trackingGuid
                };

                // Trigger the Webhook event
                await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);

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
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CategoryModel.Update()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete/Reactivate Method(s)
        /// <summary>
        /// Delete or reactivate an existing category
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
            string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Category").Name;
            string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;
            //var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbCategory = _categoryCollection.Find(filters).FirstOrDefault();

            if (dbCategory == null)
            {
                string reason = $"Category not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            // Set the updated timestamp here
            string timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

            // Update the Following Fields
            var update = Builders<Deathstar.Data.Models.PC_Category>.Update
                            .Set(x => x.IsActive, reactivate)
                            .Set(x => x.UpdatedBy, context.Security.GetEmail())
                            .Set(x => x.UpdatedDateTime, timestamp);

            // Update database record
            await _categoryCollection.UpdateOneAsync(filters, update);

            // Remove all category assignments only on delete
            if (!reactivate)
            {
                // Remove all category assignments
                var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                // Find all Products Filter
                var productFilters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                productFilters = productFilters & Builders<Deathstar.Data.Models.PC_Product>.Filter.ElemMatch(x => x.Categories, x => x.CategoryId == id);

                // Find all the products that have this category assignment
                var dbProducts = productCollection.FindAsync(productFilters).Result.ToList();

                // get a list of all the category assignments that match the categroy id
                var dbProductCategoryAssignments = dbProducts.SelectMany(x => x.Categories).Where(y => y.CategoryId == id).ToList();

                foreach (var dbProductCategoryAssignment in dbProductCategoryAssignments)
                {
                    //circular reference
                    await _categoryAssignment.DeleteOrReactivate(context, dbProductCategoryAssignment.PC_ProductCategory_Id, "PC_Product", false,trackingGuid);
                }

                // Variant Filter
                var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.ElemMatch(x => x.Categories, x => x.CategoryId == id);

                // Find all the variants that have this category assignment
                var dbVariants = variantCollection.FindAsync(variantFilters).Result.ToList();

                // get a list of all the category assignments that match the category id
                var dbVariantCategoryAssignments = dbVariants.SelectMany(x => x.Categories).Where(y => y.CategoryId == id).ToList();

                foreach (var dbVariantCategoryAssignment in dbVariantCategoryAssignments)
                {
                    //circular reference
                    await _categoryAssignment.DeleteOrReactivate(context, dbVariantCategoryAssignment.PC_ProductCategory_Id, "PC_ProductVariant", false, trackingGuid);
                }
            }
            else // if reactivating send update webhook
            {
                // Build the Webhook event
                var whRequest = new Sheev.Common.Models.WebhookResponse()
                {
                    CompanyId = companyId.ToString(),
                    Type = "Product Category",
                    Scope = $"category/product/updated",
                    Id = id.ToString(),
                    TrackingGuid = trackingGuid
                };

                // Trigger the Webhook event
                await _webHookModel.FireWebhookEvent(whRequest, context, trackingGuid);
            }

            // Update the corresponding External Id record(s)
            await _externalIdModel.DeleteOrReactivateByParentId(dbCategory.Id.ToString(), reactivate, "PC_Category", timestamp, dbCategory.UpdatedDateTime, trackingGuid, "category/product/deleted", true);

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        /// <summary>
        /// Validates a category by id
        /// </summary>
        /// <param name="id">category id</param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task<string> Validate(long id, IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var categoryCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Category>("PC_Category");

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbCategory = await categoryCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            if (dbCategory == null)
            {
                string reason = $"Category not found by Id: {id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }

            return dbCategory.Name;
        }

        /// <summary>
        /// Validates a set of generic request categories
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public static async Task Validate(List<Sheev.Common.Models.GenericRequest> categories,IBaseContextModel context, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            var categoryCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Category>("PC_Category");

            if (categories != null && categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    Int64.TryParse(category.Id, out long longId);

                    // Filter
                    var filters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, longId);
                    filters = filters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                    var dbCategory = await categoryCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                    if (dbCategory == null)
                    {
                        string reason = $"Category not found by Id:{longId} provided.";

                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
                    }

                    category.Name = dbCategory.Name;
                }
            }
        }
        #endregion
    }
}
