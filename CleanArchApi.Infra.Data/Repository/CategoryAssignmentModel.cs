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
    /// Its the Category Assignment Model!
    /// </summary>
    public class CategoryAssignmentModel : ICategoryAssignmentModel
    {
        /// <summary>
        /// </summary>
        private readonly IWebhookModel _webHookModel;
        private readonly IBaseContextModel context;
        public CategoryAssignmentModel(IBaseContextModel contextModel,IWebhookModel webHookModel)
        ////public CategoryAssignmentModel(IBaseContextModel contextModel, IWebhookModel webHookModel, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _webHookModel = webHookModel;
            context = contextModel;
            //context = new Models.ContextModel(mongoDbSettings, apiSettings);
        }
        
        #region Get Method(s)
        /// <summary>
        /// Get a Category Assignment by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductCategoryResponse> GetById(IBaseContextModel context, string id, string tableName, Guid trackingGuid)
        {
            // Response
            var response = new Tagge.Common.Models.ProductCategoryResponse();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            // MongoDB Settings
            var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string productCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_Product").Name;
            string variantCollectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

            // Get MongoDB
            var db = context.Database;

            // DB Object 
            var dbProductCategory = new Deathstar.Data.Models.PC_ProductCategory();

            // Break combined id
            string[] ids = id.Split('|');

            // Parent id
            long.TryParse(ids[0], out long parentId);

            // Switch determined by tablename sent in
            if (tableName == "PC_Product")
            {
                var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);
                

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, parentId);
                filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                // Find the product
                var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                // Product not found if true
                if (dbProduct == null)
                {
                    string reason = $"Product Category not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                dbProductCategory = dbProduct.Categories.FirstOrDefault(x => x.PC_ProductCategory_Id == id && x.IsActive);
            }
            else
            {
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                // Filter
                var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, parentId);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);
                variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

                var dbVariant = await variantCollection.FindAsync(variantFilters).Result.FirstOrDefaultAsync();

                if (dbVariant == null)
                {
                    string reason = $"Product Category not found by Id:{id} provided.";

                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
                }

                dbProductCategory = dbVariant.Categories.FirstOrDefault(x => x.PC_ProductCategory_Id == id && x.IsActive);
            }

            if (dbProductCategory == null)
            {
                string reason = $"Product Category not found by Id:{id} provided.";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Category was unable to be retrieved! Reason: {reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = reason };
            }

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Product Category Type (id: {dbProductCategory.PC_ProductCategory_Id}) successfully retrieved.", "Get Alternate Id Type", LT319.Common.Utilities.Constants.TrackingStatus.Complete, context, trackingGuid);

            response = new Common.Models.ProductCategoryResponse()
            {
                Id = dbProductCategory.PC_ProductCategory_Id,
                ProductId = parentId,
                CategoryId = dbProductCategory.CategoryId,
                Type = "Variant"
            };

            return response;
        }
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Saves a category assignment
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tableName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.ProductCategoryResponse> Save(IBaseContextModel context, Tagge.Common.Models.ProductCategoryRequest request, string tableName, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductCategoryResponse();

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

                // Word
                var dbCategory = new Deathstar.Data.Models.PC_Category();

                // Id prefix
                string idPrefix = string.Empty;

                // Validate Category
                ////Circular Reference
                string dbCategoryName = await CategoryModel.Validate(request.CategoryId, context, trackingGuid);// _new CategoryModel(context).Validate(request.CategoryId, trackingGuid);

                // Product Category
                var dbProductCategory = new Deathstar.Data.Models.PC_ProductCategory();

                // Convert To Database Object
                dbProductCategory.ConvertToDatabaseObject(dbCategoryName, request);

                // Set Primary Key
                dbProductCategory.SetPrimaryKey(request.InternalId.ToString(), request.CategoryId.ToString());

                // Set Is Active to true
                dbProductCategory.IsActive = true;

                switch (tableName)
                {
                    case "PC_Product":
                        var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                        // Product
                        var dbProduct = new Deathstar.Data.Models.PC_Product();

                        // Filters - note always start with the company id
                        var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, request.InternalId);
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                        // Find the product
                        dbProduct = await productCollection.Find(filters).FirstOrDefaultAsync();

                        // The product was not valid
                        if (dbProduct == null)
                        {
                            var message = $"Product Id not valid: {request.InternalId}";

                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = message };
                        }

                        if (dbProduct.Categories != null && dbProduct.Categories.Count > 0)
                        {
                            var dbtempCategory = dbProduct.Categories.FirstOrDefault(x => x.PC_ProductCategory_Id == dbProductCategory.PC_ProductCategory_Id && x.CategoryId == request.CategoryId);

                            if (dbtempCategory == null)
                            {
                                // Add the Category Assignment
                                dbProduct.Categories.Add(dbProductCategory);
                            }
                            else
                            {
                                // Update the Category Assignment
                                dbProductCategory = dbtempCategory;
                                dbProductCategory.IsActive = true;
                            }
                        }
                        else
                        {
                            // Add the Category Assignment
                            dbProduct.Categories.Add(dbProductCategory);
                        }

                        // Set Product Id to 0 cause of the serializer
                        var productId = dbProduct.Id;
                        dbProduct.Id = 0;

                        var serializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var update = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbProduct, serializerSettings)) } };

                        // Update database record
                        await productCollection.UpdateOneAsync(filters, update);

                        response = new Common.Models.ProductCategoryResponse()
                        {
                            Id = dbProductCategory.PC_ProductCategory_Id,
                            ProductId = productId,
                            CategoryId = request.CategoryId,
                            Type = "Product"
                        };

                        // Build the Webhook event
                        var whProductRequest = new Sheev.Common.Models.WebhookResponse()
                        {
                            CompanyId = companyId.ToString(),
                            Type = "Product Category Assignment",
                            Scope = $"product/category/created",
                            Id = response.Id,
                            TrackingGuid = trackingGuid
                        };

                        // Trigger the Webhook event
                        await _webHookModel.FireWebhookEvent(whProductRequest, context, trackingGuid);

                        break;
                    case "PC_ProductVariant":
                        var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                        // Variant
                        var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                        var variantFilter = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, request.InternalId);
                        variantFilter = variantFilter & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);
                        variantFilter = variantFilter & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        dbVariant = await variantCollection.Find(variantFilter).FirstOrDefaultAsync();

                        if (dbVariant == null)
                        {
                            var message = $"Product Variant Id not valid: {request.InternalId}";

                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = message };
                        }

                        if (dbVariant.Categories != null && dbVariant.Categories.Count > 0)
                        {
                            var dbtempCategory = dbVariant.Categories.FirstOrDefault(x => x.PC_ProductCategory_Id == dbProductCategory.PC_ProductCategory_Id && x.CategoryId == request.CategoryId && x.IsActive);

                            if (dbtempCategory == null)
                            {
                                // Add the Category Assignment
                                dbVariant.Categories.Add(dbProductCategory);
                            }
                            else
                            {
                                // Update the Category Assignment
                                dbProductCategory = dbtempCategory;
                                dbProductCategory.IsActive = true;
                            }
                        }
                        else
                        {
                            // Add the Category Assignment
                            dbVariant.Categories.Add(dbProductCategory);
                        }

                        // Set Product Id to 0 cause of the serializer
                        var variantId = dbVariant.Id;
                        dbVariant.Id = 0;

                        var variantSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };

                        var variantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, variantSerializerSettings)) } };

                        // Update database record
                        await variantCollection.UpdateOneAsync(variantFilter, variantUpdate);

                        response = new Common.Models.ProductCategoryResponse()
                        {
                            Id = dbProductCategory.PC_ProductCategory_Id,
                            ProductId = variantId,
                            CategoryId = request.CategoryId,
                            Type = "Variant"
                        };

                        // Build the Webhook event
                        var whVariantRequest = new Sheev.Common.Models.WebhookResponse()
                        {
                            CompanyId = companyId.ToString(),
                            Type = "Product Category Assignment",
                            Scope = $"product/variant/category/created",
                            Id = response.Id,
                            TrackingGuid = trackingGuid
                        };

                        // Trigger the Webhook event
                        await _webHookModel.FireWebhookEvent(whVariantRequest, context, trackingGuid);

                        break;
                    default:
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid Type: {tableName}. Please specify a correct type: Product or Variant" };
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

        /// <summary>
        /// Saves a product category assignment when attached to a product
        /// Internal Use Only!
        /// </summary>
        /// <param name="internalId">Parent id like product or variant id</param>
        /// <param name="request"></param>
        /// <param name="existingCategories"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_ProductCategory>> Save(IBaseContextModel context, long internalId, List<Sheev.Common.Models.GenericRequest> request, List<Deathstar.Data.Models.PC_ProductCategory> existingCategories, Guid trackingGuid)
        {
            List<Deathstar.Data.Models.PC_ProductCategory> dbProductCategories = new List<Deathstar.Data.Models.PC_ProductCategory>();

            try
            {
                // Verify that the category exists
                ////Circular Reference
                await CategoryModel.Validate(request, context, trackingGuid);

                // if null then create an empty list
                if (existingCategories == null)
                    existingCategories = new List<Deathstar.Data.Models.PC_ProductCategory>();
                else // Add existing kids to the group so they are not left at the park
                    dbProductCategories = existingCategories;

                foreach (var productCategoryRequest in request)
                {
                    // make sure the id is a big int
                    long.TryParse(productCategoryRequest.Id, out long categoryId);

                    // Check to see if it already exists on the product/variant
                    var duplicateCategory = existingCategories.FirstOrDefault(x => x.CategoryId == categoryId);

                    // If its not already on the parent then add it else ignore
                    if (duplicateCategory == null)
                    {
                        var dbProductCategory = new Deathstar.Data.Models.PC_ProductCategory();
                        dbProductCategory.ConvertToGenericDatabaseObject(productCategoryRequest);

                        // Set Primary Key
                        dbProductCategory.SetPrimaryKey(internalId.ToString(), productCategoryRequest.Id);

                        dbProductCategory.IsActive = true;

                        dbProductCategories.Add(dbProductCategory);
                    }
                }

                return dbProductCategories;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CategoryAssignmentModel.SaveToProduct()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
            throw new NotImplementedException();
        }
        #endregion

        #region Delete Method(s)
        public async Task DeleteOrReactivate(IBaseContextModel context, string combinedId, string tableName, bool reactivate, Guid trackingGuid)
        {
            var response = new Tagge.Common.Models.ProductCategoryResponse();

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

                // Word
                var dbCategory = new Deathstar.Data.Models.PC_Category();

                // Id prefix
                string idPrefix = string.Empty;

                // Product Category
                var dbProductCategory = new Deathstar.Data.Models.PC_ProductCategory();

                // Product
                var dbProduct = new Deathstar.Data.Models.PC_Product();

                // Variant
                var dbVariant = new Deathstar.Data.Models.PC_ProductVariant();

                // Break combined id
                string[] ids = combinedId.Split('|');

                // Product id
                long.TryParse(ids[0], out long internalId);

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

                    if (dbProduct == null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid Id: {combinedId}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid Id: {combinedId}" };
                    }

                    // Find the category within the product
                    dbProductCategory = dbProduct.Categories.FirstOrDefault(x => x.PC_ProductCategory_Id == combinedId);

                    if (dbProductCategory == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Product Category not found by id({combinedId}) provided" };

                    dbProduct.Categories.Remove(dbProductCategory);

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

                    // Build the Webhook event
                    var whProductRequest = new Sheev.Common.Models.WebhookResponse()
                    {
                        CompanyId = companyId.ToString(),
                        Type = "Product Category Assignment",
                        Scope = $"product/category/deleted",
                        Id = combinedId
                    };

                    // Trigger the Webhook event
                    await _webHookModel.FireWebhookEvent(whProductRequest, context, trackingGuid);
                }
                else
                {
                    // Get the table ready
                    var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);

                    // Filter building
                    var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);
                    variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);
                    variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                    dbVariant = await variantCollection.Find(variantFilters).FirstOrDefaultAsync();

                    if (dbVariant == null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Invalid Id: {combinedId}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid Id: {combinedId}" };
                    }

                    // Find the category
                    dbProductCategory = dbVariant.Categories.FirstOrDefault(x => x.PC_ProductCategory_Id == combinedId);

                    // Category is missing
                    if (dbProductCategory == null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Product Category not found by id({combinedId}) provided" };

                    dbVariant.Categories.Remove(dbProductCategory);

                    // Set Product Id to 0 cause of the serializer
                    dbVariant.Id = 0;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    var variantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, serializerSettings)) } };

                    await variantCollection.UpdateOneAsync(variantFilters, variantUpdate);

                    // Build the Webhook event
                    var whVariantRequest = new Sheev.Common.Models.WebhookResponse()
                    {
                        CompanyId = companyId.ToString(),
                        Type = "Product Category Assignment",
                        Scope = $"product/variant/category/deleted",
                        Id = combinedId
                    };

                    // Trigger the Webhook event
                    await _webHookModel.FireWebhookEvent(whVariantRequest, context, trackingGuid);
                }
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

        #region Validate Method(s)
        #endregion
    }
}
