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
    /// Its the Custom Field Model?
    /// </summary>
    public class CustomFieldModel : ICustomFieldModel
    {
        private readonly IBaseContextModel context;
        private readonly IExternalIdModel _externalIdModel;
        public CustomFieldModel(IBaseContextModel contextModel, IExternalIdModel externalIdModel)
        {
            context = contextModel;
            _externalIdModel = externalIdModel;
        }
 
        #region Save Method(s)
        /// <summary>
        /// Save a custom field
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.GenericCustomFieldResponse> Save(Tagge.Common.Models.GenericCustomFieldRequest request, Guid trackingGuid)
        {
            // Response
            var response = new Tagge.Common.Models.GenericCustomFieldResponse();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            try
            {
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

                // Get tablename from request type
                var tableName = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(request.Type);

                // Break combined id
                string[] ids = request.Id.Split('|');

                // Parent id
                long.TryParse(ids[0], out long internalId);

                // Validate the custom field exists
                ValidateCustomField(request.CustomFieldName, tableName);

                // Create an empty dbCustomField 
                var dbCustomField = new Deathstar.Data.Models.PC_CustomField();

                switch (tableName)
                {
                    case "PC_Product":
                    case "PC_ProductCategory":
                    case "PC_ProductUnit":
                    case "PC_ProductAlternateId":
                    case "PC_Kit":
                    case "PC_KitComponent":
                    case "PC_ProductOption":
                    case "PC_ProductOptionValue":
                        var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                        // Filters - note always start with the company id
                        var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, internalId);

                        // look for product by id
                        var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                        if (dbProduct == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields
                        switch (tableName)
                        {
                            case "PC_Product":
                                // Save/Update the custom field
                                dbProduct.CustomFields = SaveOrUpdateGenericCustomField(request, dbProduct.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductCategory":
                                break;
                            case "PC_ProductUnit":
                                // Find the Unit by id
                                var dbProductUnit = dbProduct.Units.FirstOrDefault(x => x.PC_ProductUnit_Id == request.Id);

                                // Check to see if the object exists
                                if (dbProductUnit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductUnit.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductUnit.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductAlternateId":
                                var dbProductAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == request.Id);

                                // Check to see if the object exists
                                if (dbProductAlternateId == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductAlternateId.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductAlternateId.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_Kit":
                                // Check to see if the object exists
                                if (dbProduct.Kit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProduct.Kit.CustomFields = SaveOrUpdateGenericCustomField(request, dbProduct.Kit.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_KitComponent":
                                var dbProductKitComponent = dbProduct.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == request.Id);

                                // Check to see if the object exists
                                if (dbProductKitComponent == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductKitComponent.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductKitComponent.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductOption":
                                var dbProductOption = dbProduct.Options.FirstOrDefault(x => x.PC_Option_Id == request.Id);

                                // Check to see if the object exists
                                if (dbProductOption == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductOption.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductOption.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductOptionValue":
                                // Get parent id
                                string parentId = string.Empty;
                                int indexOfSteam = request.Id.LastIndexOf("|");
                                if (indexOfSteam >= 0)
                                    parentId = request.Id.Remove(indexOfSteam);

                                // Get the parent
                                var dbParentOption = dbProduct.Options.FirstOrDefault(x => x.PC_Option_Id == parentId);

                                // Check to see if the parent exists
                                if (dbParentOption == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Find the option value
                                var dbOptionValue = dbParentOption.Values.FirstOrDefault(x => x.PC_OptionValue_Id == request.Id);

                                // Check to see if the object exists
                                if (dbOptionValue == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbOptionValue.CustomFields = SaveOrUpdateGenericCustomField(request, dbOptionValue.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
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

                        break;
                    case "PC_ProductVariant":
                    case "PC_ProductVariantCategory":
                    case "PC_ProductVariantAlternateId":
                    case "PC_VariantKit":
                    case "PC_VariantKitComponent":
                        var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                        // Filter
                        var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);
                        variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                        // look for variant by id
                        var dbVariant = await variantCollection.FindAsync(variantFilters).Result.FirstOrDefaultAsync();

                        if (dbVariant == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields
                        switch (tableName)
                        {
                            case "PC_ProductVariant":
                                // Save/Update the custom field
                                dbVariant.CustomFields = SaveOrUpdateGenericCustomField(request, dbVariant.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductVariantCategory":
                                break;
                            case "PC_ProductVariantAlternateId":
                                var dbProductVariantAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == request.Id);

                                // Check to see if the object exists
                                if (dbProductVariantAlternateId == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductVariantAlternateId.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductVariantAlternateId.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_VariantKit":
                                // Check to see if the object exists
                                if (dbVariant.Kit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                dbVariant.Kit.CustomFields = SaveOrUpdateGenericCustomField(request, dbVariant.Kit.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_VariantKitComponent":
                                var dbProductVariantKitComponent = dbVariant.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == request.Id);

                                // Check to see if the object exists
                                if (dbProductVariantKitComponent == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductVariantKitComponent.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductVariantKitComponent.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);
                                break;
                        }

                        // Set variant Id to 0 cause of the serializer
                        var variantId = dbVariant.Id;
                        dbVariant.Id = 0;

                        var variantSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var variantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, variantSerializerSettings)) } };

                        // Update database record
                        await variantCollection.UpdateOneAsync(variantFilters, variantUpdate);

                        break;
                    case "PC_ProductVariantOption":
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

                        var dbProductVariantOption = dbVariantParent.Options.FirstOrDefault(x => x.PC_OptionValue_Id == request.Id);

                        // Check to see if the object exists
                        if (dbProductVariantOption == null)
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                        // Save/Update the custom field
                        dbProductVariantOption.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductVariantOption.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);

                        // Set variant Id to 0 cause of the serializer
                        var variantParentId = dbVariantParent.Id;
                        dbVariantParent.Id = 0;

                        var variantParentSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var variantParentUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariantParent, variantParentSerializerSettings)) } };

                        // Update database record
                        await variantOptionCollection.UpdateOneAsync(variantParentFilters, variantParentUpdate);
                        break;
                    case "PC_Inventory":
                    case "PC_ProductInventory":
                    case "PC_ProductVariantInventory":
                        var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(inventoryCollectionName);

                        // Filter
                        var inventoryFilters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, internalId);
                        inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);

                        // look for Inventory by id
                        var dbInventory = await inventoryCollection.FindAsync(inventoryFilters).Result.FirstOrDefaultAsync();

                        if (dbInventory == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Inventory
                        dbInventory.CustomFields = SaveOrUpdateGenericCustomField(request, dbInventory.CustomFields, tableName, request.Id, ref dbCustomField, trackingGuid);

                        // Set Inventory Id to 0 cause of the serializer
                        var inventoryId = dbInventory.Id;
                        dbInventory.Id = 0;

                        var inventorySerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var inventoryUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbInventory, inventorySerializerSettings)) } };

                        // Update database record
                        await inventoryCollection.UpdateOneAsync(inventoryFilters, inventoryUpdate);
                        break;
                    case "PC_AlternateIdType":
                        var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(alternateIdTypeCollectionName);

                        // Filter
                        var alternateIdTypeFilters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, internalId);
                        alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.IsActive, true);


                        // look for Alternate Id Type by id
                        var dbAlternateIdType = await alternateIdTypeCollection.FindAsync(alternateIdTypeFilters).Result.FirstOrDefaultAsync();

                        if (dbAlternateIdType == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }
                        // Add/update custom fields on a Alternate Id Type
                        dbAlternateIdType.CustomFields = SaveOrUpdateGenericCustomField(request, dbAlternateIdType.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Alternate Id Type Id to 0 cause of the serializer
                        var alternateIdTypeId = dbAlternateIdType.Id;
                        dbAlternateIdType.Id = 0;

                        var alternateIdTypeSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var alternateIdTypeUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbAlternateIdType, alternateIdTypeSerializerSettings)) } };

                        // Update database record
                        await alternateIdTypeCollection.UpdateOneAsync(alternateIdTypeFilters, alternateIdTypeUpdate);
                        break;
                    case "PC_Category":
                        var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(categoryCollectionName);

                        // Filter
                        var categoryFilters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, internalId);
                        categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);

                        // look for Category by id
                        var dbCategory = await categoryCollection.FindAsync(categoryFilters).Result.FirstOrDefaultAsync();

                        if (dbCategory == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Category
                        dbCategory.CustomFields = SaveOrUpdateGenericCustomField(request, dbCategory.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Category Id to 0 cause of the serializer
                        var categoryId = dbCategory.Id;
                        dbCategory.Id = 0;

                        var categorySerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var categoryUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategory, categorySerializerSettings)) } };

                        // Update database record
                        await categoryCollection.UpdateOneAsync(categoryFilters, categoryUpdate);

                        break;
                    case "PC_CategorySet":
                        var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(categorySetCollectionName);

                        // Filter
                        var categorySetFilters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, internalId);
                        categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);

                        // look for Category Set by id
                        var dbCategorySet = await categorySetCollection.FindAsync(categorySetFilters).Result.FirstOrDefaultAsync();

                        if (dbCategorySet == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Category Set
                        dbCategorySet.CustomFields = SaveOrUpdateGenericCustomField(request, dbCategorySet.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Category Set Id to 0 cause of the serializer
                        var categorySetId = dbCategorySet.Id;
                        dbCategorySet.Id = 0;

                        var categorySetserializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var categorySetUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategorySet, categorySetserializerSettings)) } };

                        // Update database record
                        await categorySetCollection.UpdateOneAsync(categorySetFilters, categorySetUpdate);

                        break;
                    case "PC_Location":
                        var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(locationCollectionName);

                        // Filter
                        var locationFilters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, internalId);
                        locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.IsActive, true);

                        // look for Location by id
                        var dbLocation = await locationCollection.FindAsync(locationFilters).Result.FirstOrDefaultAsync();

                        if (dbLocation == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Location
                        dbLocation.CustomFields = SaveOrUpdateGenericCustomField(request, dbLocation.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Location Id to 0 cause of the serializer
                        var locationId = dbLocation.Id;
                        dbLocation.Id = 0;

                        var locationSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var locationUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocation, locationSerializerSettings)) } };

                        // Update database record
                        await locationCollection.UpdateOneAsync(locationFilters, locationUpdate);

                        break;
                    case "PC_LocationGroup":
                        var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(locationTypeCollectionName);

                        // Filter
                        var locationGroupFilters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, internalId);
                        locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.IsActive, true);


                        // look for Location Group by id
                        var dbLocationGroup = await locationGroupCollection.FindAsync(locationGroupFilters).Result.FirstOrDefaultAsync();

                        // It appears the location group is missing so throw an error
                        if (dbLocationGroup == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Location Group
                        dbLocationGroup.CustomFields = SaveOrUpdateGenericCustomField(request, dbLocationGroup.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Location Group Id to 0 cause of the serializer
                        var locationGroupId = dbLocationGroup.Id;
                        dbLocationGroup.Id = 0;

                        // Should look at moving this to the context
                        var locationGroupSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var locationGroupUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocationGroup, locationGroupSerializerSettings)) } };

                        // Update database record
                        await locationGroupCollection.UpdateOneAsync(locationGroupFilters, locationGroupUpdate);

                        break;
                    default:
                        break;
                }

                // Build the Response
                response = dbCustomField.ConvertToResponse();

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Custom Field ( Name: {request.CustomFieldName}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// (Internal use only) Save custom field
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tableName"></param>
        /// <param name="internalId"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_CustomField>> SaveGenericCustomField(List<Tagge.Common.Models.GenericCustomFieldRequest> request, string tableName, string internalId, Guid trackingGuid)
        {
            var dbCustomFields = new List<Deathstar.Data.Models.PC_CustomField>();
            var companyId = context.Security.GetCompanyId();

            IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning Product Custom Field (Table Name: {tableName}) save by user {context.Security.GetEmail()}", "Save External Id", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

            try
            {
                foreach (var customFieldRequest in request)
                {
                    // Check to see if there is a duplicate custom field already in the list 
                    var dupCustomField = dbCustomFields.FirstOrDefault(x => x.CustomFieldName == customFieldRequest.CustomFieldName);

                    // if so error!
                    if (dupCustomField != null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Duplicate custom field '{customFieldRequest.CustomFieldName}' is not allowed" };

                    // Validate the custom field exists
                    ValidateCustomField(customFieldRequest.CustomFieldName, tableName);

                    var dbCustomField = new Deathstar.Data.Models.PC_CustomField();

                    dbCustomField.ConvertToDatabaseObject(customFieldRequest);
                    dbCustomField.SetPrimaryKey(internalId, customFieldRequest.CustomFieldName);

                    dbCustomFields.Add(dbCustomField);
                }

                return dbCustomFields;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{tableName} Product Custom Field failed! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.SaveGenericExternalId()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Save Or Update Method(s)
        /// <summary>
        /// Internal Use Only! Adds/Updates a single Custom Field on a parent update call
        /// </summary>
        /// <param name="request"></param>
        /// <param name="existingCustomFields"></param>
        /// <param name="tableName"></param>
        /// <param name="internalId"></param>
        /// <param name="newCustomField"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<Deathstar.Data.Models.PC_CustomField> SaveOrUpdateGenericCustomField(Tagge.Common.Models.GenericCustomFieldRequest request, List<Deathstar.Data.Models.PC_CustomField> existingCustomFields, string tableName, string internalId, ref Deathstar.Data.Models.PC_CustomField newCustomField, Guid trackingGuid)
        {
            // Initialize the new collection
            var dbCustomFields = new List<Deathstar.Data.Models.PC_CustomField>();

            try
            {
                // Null check so we dont throw an error after this when we try to access the collection
                if (existingCustomFields == null || existingCustomFields.Count <= 0)
                    existingCustomFields = new List<Deathstar.Data.Models.PC_CustomField>();

                // Check to see if the custom field exists in the list already
                var dbCustomField = existingCustomFields.FirstOrDefault(x => x.CustomFieldName.ToLower() == request.CustomFieldName.ToLower());

                // if no then add
                if (dbCustomField == null)
                {
                    // Validate the custom field exists
                    ValidateCustomField(request.CustomFieldName, tableName);

                    dbCustomField = new Deathstar.Data.Models.PC_CustomField();
                    dbCustomField.ConvertToDatabaseObject(request);
                    dbCustomField.SetPrimaryKey(internalId, request.CustomFieldName);
                }
                else // else update that bi***!
                {
                    dbCustomField.Value = request.Value;
                }

                // Add to collection
                dbCustomFields.Add(dbCustomField);

                // Update the reference custom field so it can be returned later
                newCustomField = dbCustomField;

                return dbCustomFields;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.UpdateGenericCustomField()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// Internal Use Only! Adds/Updates a collection of Custom Fields on a parent update call
        /// </summary>
        /// <param name="request"></param>
        /// <param name="existingCustomFields"></param>
        /// <param name="tableName"></param>
        /// <param name="internalId"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_CustomField>> SaveOrUpdateGenericCustomFields(List<Tagge.Common.Models.GenericCustomFieldRequest> request, List<Deathstar.Data.Models.PC_CustomField> existingCustomFields, string tableName, string internalId, Guid trackingGuid)
        {
            // Initialize the new collection
            var dbCustomFields = new List<Deathstar.Data.Models.PC_CustomField>();

            try
            {
                if (existingCustomFields == null || existingCustomFields.Count <= 0)
                    existingCustomFields = new List<Deathstar.Data.Models.PC_CustomField>();

                foreach (var customFieldRequest in request)
                {
                    // Check to see if there is a duplicate custom field already in the list 
                    var dupCustomField = dbCustomFields.FirstOrDefault(x => x.CustomFieldName.ToLower() == customFieldRequest.CustomFieldName.ToLower());

                    // if so error!
                    if (dupCustomField != null)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Duplicate custom field '{customFieldRequest.CustomFieldName}' is not allowed" };

                    // Check to see if the custom field already exists on the collection
                    var dbCustomField = existingCustomFields.FirstOrDefault(x => x.CustomFieldName.ToLower() == customFieldRequest.CustomFieldName.ToLower());

                    // if it does then update else add <- yes that is backwards from the logic below... deal with it
                    if (dbCustomField == null)
                    {
                        // Validate the custom field exists
                        ValidateCustomField(customFieldRequest.CustomFieldName, tableName);

                        dbCustomField = new Deathstar.Data.Models.PC_CustomField();
                        dbCustomField.ConvertToDatabaseObject(customFieldRequest);
                        dbCustomField.SetPrimaryKey(internalId, customFieldRequest.CustomFieldName);
                    }
                    else
                    {
                        dbCustomField.Value = customFieldRequest.Value;
                    }

                    // Add the custom field back into the collection
                    dbCustomFields.Add(dbCustomField);
                }

                return dbCustomFields;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.UpdateGenericCustomField()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Method(s)
        /// <summary>
        /// Updates an custom field from the controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<Tagge.Common.Models.GenericCustomFieldResponse> Update(string id, Tagge.Common.Models.GenericCustomFieldRequest request, Guid trackingGuid)
        {
            // Response
            var response = new Tagge.Common.Models.GenericCustomFieldResponse();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            try
            {
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

                // Get tablename from request type
                var tableName = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(request.Type);

                // Break combined id
                string[] ids = id.Split('|');

                // Parent id
                long.TryParse(ids[0], out long internalId);

                // Validate the custom field exists
                ValidateCustomField(request.CustomFieldName, tableName);

                // Validate the id
                ////Ciscular reference
                await _externalIdModel.ValidateInternalId(request.Id, tableName, trackingGuid);

                // Create an empty dbCustomField 
                var dbCustomField = new Deathstar.Data.Models.PC_CustomField();

                switch (tableName)
                {
                    case "PC_Product":
                    case "PC_ProductCategory":
                    case "PC_ProductUnit":
                    case "PC_ProductAlternateId":
                    case "PC_Kit":
                    case "PC_KitComponent":
                    case "PC_ProductOption":
                    case "PC_ProductOptionValue":
                        var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                        // Filters - note always start with the company id
                        var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, internalId);
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                        // look for product by id
                        var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                        if (dbProduct == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields
                        switch (tableName)
                        {
                            case "PC_Product":
                                // Save/Update the custom field
                                dbProduct.CustomFields = SaveOrUpdateGenericCustomField(request, dbProduct.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductCategory":
                                break;
                            case "PC_ProductUnit":
                                var dbUnitCustomField = dbProduct.Units.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbUnitCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductUnitId = dbUnitCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string index = $"|{dbUnitCustomField.CustomFieldName}";
                                int indexOfSteam = dbProductUnitId.IndexOf(index);
                                if (indexOfSteam >= 0)
                                    dbProductUnitId = dbProductUnitId.Remove(indexOfSteam);

                                // Get the parent
                                var dbProductUnit = dbProduct.Units.FirstOrDefault(x => x.PC_ProductUnit_Id == dbProductUnitId);

                                // Save/Update the custom field
                                dbProductUnit.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductUnit.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductAlternateId":
                                var dbProductAlternateIdCustomField = dbProduct.AlternateIds.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductAlternateIdCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductAlternateIdId = dbProductAlternateIdCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string alternateIdIndex = $"|{dbProductAlternateIdCustomField.CustomFieldName}";
                                int indexOfAlternateId = dbProductAlternateIdId.IndexOf(alternateIdIndex);
                                if (indexOfAlternateId >= 0)
                                    dbProductAlternateIdId = dbProductAlternateIdId.Remove(indexOfAlternateId);

                                // Get the parent
                                var dbProductAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == dbProductAlternateIdId);

                                // Save/Update the custom field
                                dbProductAlternateId.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductAlternateId.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_Kit":
                                // Check to see if the object exists
                                if (dbProduct.Kit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProduct.Kit.CustomFields = SaveOrUpdateGenericCustomField(request, dbProduct.Kit.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_KitComponent":
                                var dbProductKitComponentCustomField = dbProduct.Kit.Components.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductKitComponentCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductKitComponentId = dbProductKitComponentCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string kitComponentIndex = $"|{dbProductKitComponentCustomField.CustomFieldName}";
                                int indexOfProductKitComponent = dbProductKitComponentId.IndexOf(kitComponentIndex);
                                if (indexOfProductKitComponent >= 0)
                                    dbProductKitComponentId = dbProductKitComponentId.Remove(indexOfProductKitComponent);

                                // Get the parent
                                var dbProductKitComponent = dbProduct.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == dbProductKitComponentId);

                                // Save/Update the custom field
                                dbProductKitComponent.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductKitComponent.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductOption":
                                var dbProductOptionCustomField = dbProduct.Options.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductOptionCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductOptionId = dbProductOptionCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string productOptionIndex = $"|{dbProductOptionCustomField.CustomFieldName}";
                                int indexOfOption = dbProductOptionId.IndexOf(productOptionIndex);
                                if (indexOfOption >= 0)
                                    dbProductOptionId = dbProductOptionId.Remove(indexOfOption);

                                // Get the parent
                                var dbProductOption = dbProduct.Options.FirstOrDefault(x => x.PC_Option_Id == dbProductOptionId);

                                // Save/Update the custom field
                                dbProductOption.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductOption.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductOptionValue":
                                var dbProductOptionValueCustomField = dbProduct.Options.SelectMany(x => x.Values).SelectMany(y => y.CustomFields).FirstOrDefault(z => z.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductOptionValueCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductOptionValueId = dbProductOptionValueCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string optionValueIndex = $"|{dbProductOptionValueCustomField.CustomFieldName}";
                                int indexOfOptionValue = dbProductOptionValueId.IndexOf(optionValueIndex);
                                if (indexOfOptionValue >= 0)
                                    dbProductOptionValueId = dbProductOptionValueId.Remove(indexOfOptionValue);

                                // Get the parent
                                var dbOptionValue = dbProduct.Options.SelectMany(x => x.Values).FirstOrDefault(z => z.PC_OptionValue_Id == dbProductOptionValueId);

                                // Save/Update the custom field
                                dbOptionValue.CustomFields = SaveOrUpdateGenericCustomField(request, dbOptionValue.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
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

                        break;
                    case "PC_ProductVariant":
                    case "PC_ProductVariantCategory":
                    case "PC_ProductVariantAlternateId":
                    case "PC_VariantKit":
                    case "PC_VariantKitComponent":
                        var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                        // Filter
                        var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);
                        variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                        // look for variant by id
                        var dbVariant = await variantCollection.FindAsync(variantFilters).Result.FirstOrDefaultAsync();

                        if (dbVariant == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields
                        switch (tableName)
                        {
                            case "PC_ProductVariant":
                                // Save/Update the custom field
                                dbVariant.CustomFields = SaveOrUpdateGenericCustomField(request, dbVariant.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);
                                break;
                            case "PC_ProductVariantCategory":
                                break;
                            case "PC_ProductVariantAlternateId":
                                var dbVariantAlternateIdCustomField = dbVariant.AlternateIds.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbVariantAlternateIdCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbVariantAlternateIdId = dbVariantAlternateIdCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string alternateIdIndex = $"|{dbVariantAlternateIdCustomField.CustomFieldName}";
                                int indexOfAlternateId = dbVariantAlternateIdId.IndexOf(alternateIdIndex);
                                if (indexOfAlternateId >= 0)
                                    dbVariantAlternateIdId = dbVariantAlternateIdId.Remove(indexOfAlternateId);

                                // Get the parent
                                var dbProductVariantAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == dbVariantAlternateIdId);

                                // Save/Update the custom field
                                dbProductVariantAlternateId.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductVariantAlternateId.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_VariantKit":
                                // Check to see if the object exists
                                if (dbVariant.Kit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };

                                dbVariant.Kit.CustomFields = SaveOrUpdateGenericCustomField(request, dbVariant.Kit.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                            case "PC_VariantKitComponent":
                                var dbVariantKitComponentCustomField = dbVariant.Kit.Components.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbVariantKitComponentCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbVariantKitComponentId = dbVariantKitComponentCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string kitComponentIndex = $"|{dbVariantKitComponentCustomField.CustomFieldName}";
                                int indexOfProductKitComponent = dbVariantKitComponentId.IndexOf(kitComponentIndex);
                                if (indexOfProductKitComponent >= 0)
                                    dbVariantKitComponentId = dbVariantKitComponentId.Remove(indexOfProductKitComponent);

                                // Get the parent
                                var dbProductVariantKitComponent = dbVariant.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == dbVariantKitComponentId);

                                // Save/Update the custom field
                                dbProductVariantKitComponent.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductVariantKitComponent.CustomFields, tableName, id, ref dbCustomField, trackingGuid);
                                break;
                        }

                        // Set variant Id to 0 cause of the serializer
                        var variantId = dbVariant.Id;
                        dbVariant.Id = 0;

                        var variantSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var variantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, variantSerializerSettings)) } };

                        // Update database record
                        await variantCollection.UpdateOneAsync(variantFilters, variantUpdate);
                        break;
                    case "PC_ProductVariantOption":
                        // Parent id
                        long.TryParse(ids[1], out long parentInternalVariantId);

                        // Get the table
                        var parentVariantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                        // Filter
                        var parentVariantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        parentVariantFilters = parentVariantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, parentInternalVariantId);
                        parentVariantFilters = parentVariantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                        // look for variant by id
                        var dbParentVariant = await parentVariantCollection.FindAsync(parentVariantFilters).Result.FirstOrDefaultAsync();

                        // Parent could be null
                        if (dbParentVariant == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        var dbVariantOptionCustomField = dbParentVariant.Options.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                        // Check to see if the object exists
                        if (dbVariantOptionCustomField == null)
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                        // So gotta do this to get the collection of custom fields for the object
                        var dbVariantOptionId = dbVariantOptionCustomField.PC_CustomField_Id;

                        // Modify the custom field id to get the parent id
                        string optionIndex = $"|{dbVariantOptionCustomField.CustomFieldName}";
                        int indexOfVariantOption = dbVariantOptionId.IndexOf(optionIndex);
                        if (indexOfVariantOption >= 0)
                            dbVariantOptionId = dbVariantOptionId.Remove(indexOfVariantOption);

                        // Get the parent
                        var dbProductVariantOption = dbParentVariant.Options.FirstOrDefault(x => x.PC_OptionValue_Id == dbVariantOptionId);

                        // Save/Update the custom field
                        dbProductVariantOption.CustomFields = SaveOrUpdateGenericCustomField(request, dbProductVariantOption.CustomFields, tableName, id, ref dbCustomField, trackingGuid);

                        // Set variant Id to 0 cause of the serializer
                        var parentVariantId = dbParentVariant.Id;
                        dbParentVariant.Id = 0;

                        // Serializer Settings to update the parent record
                        var parentVariantSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var parentVariantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbParentVariant, parentVariantSerializerSettings)) } };

                        // Update database record
                        await parentVariantCollection.UpdateOneAsync(parentVariantFilters, parentVariantUpdate);
                        break;
                    case "PC_Inventory":
                    case "PC_ProductInventory":
                    case "PC_ProductVariantInventory":
                        var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(inventoryCollectionName);

                        // Filter
                        var inventoryFilters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, internalId);
                        inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);

                        // look for Inventory by id
                        var dbInventory = await inventoryCollection.FindAsync(inventoryFilters).Result.FirstOrDefaultAsync();

                        if (dbInventory == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Inventory
                        dbInventory.CustomFields = SaveOrUpdateGenericCustomField(request, dbInventory.CustomFields, tableName, id, ref dbCustomField, trackingGuid);

                        // Set Inventory Id to 0 cause of the serializer
                        var inventoryId = dbInventory.Id;
                        dbInventory.Id = 0;

                        var inventorySerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var inventoryUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbInventory, inventorySerializerSettings)) } };

                        // Update database record
                        await inventoryCollection.UpdateOneAsync(inventoryFilters, inventoryUpdate);
                        break;
                    case "PC_AlternateIdType":
                        var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(alternateIdTypeCollectionName);

                        // Filter
                        var alternateIdTypeFilters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, internalId);
                        alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.IsActive, true);


                        // look for Alternate Id Type by id
                        var dbAlternateIdType = await alternateIdTypeCollection.FindAsync(alternateIdTypeFilters).Result.FirstOrDefaultAsync();

                        if (dbAlternateIdType == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }
                        // Add/update custom fields on a Alternate Id Type
                        dbAlternateIdType.CustomFields = SaveOrUpdateGenericCustomField(request, dbAlternateIdType.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Alternate Id Type Id to 0 cause of the serializer
                        var alternateIdTypeId = dbAlternateIdType.Id;
                        dbAlternateIdType.Id = 0;

                        var alternateIdTypeSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var alternateIdTypeUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbAlternateIdType, alternateIdTypeSerializerSettings)) } };

                        // Update database record
                        await alternateIdTypeCollection.UpdateOneAsync(alternateIdTypeFilters, alternateIdTypeUpdate);
                        break;
                    case "PC_Category":
                        var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(categoryCollectionName);

                        // Filter
                        var categoryFilters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, internalId);
                        categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);

                        // look for Category by id
                        var dbCategory = await categoryCollection.FindAsync(categoryFilters).Result.FirstOrDefaultAsync();

                        if (dbCategory == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Category
                        dbCategory.CustomFields = SaveOrUpdateGenericCustomField(request, dbCategory.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Category Id to 0 cause of the serializer
                        var categoryId = dbCategory.Id;
                        dbCategory.Id = 0;

                        var categorySerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var categoryUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategory, categorySerializerSettings)) } };

                        // Update database record
                        await categoryCollection.UpdateOneAsync(categoryFilters, categoryUpdate);

                        break;
                    case "PC_CategorySet":
                        var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(categorySetCollectionName);

                        // Filter
                        var categorySetFilters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, internalId);
                        categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);

                        // look for Category Set by id
                        var dbCategorySet = await categorySetCollection.FindAsync(categorySetFilters).Result.FirstOrDefaultAsync();

                        if (dbCategorySet == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Category Set
                        dbCategorySet.CustomFields = SaveOrUpdateGenericCustomField(request, dbCategorySet.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Category Set Id to 0 cause of the serializer
                        var categorySetId = dbCategorySet.Id;
                        dbCategorySet.Id = 0;

                        var categorySetserializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var categorySetUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategorySet, categorySetserializerSettings)) } };

                        // Update database record
                        await categorySetCollection.UpdateOneAsync(categorySetFilters, categorySetUpdate);

                        break;
                    case "PC_Location":
                        var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(locationCollectionName);

                        // Filter
                        var locationFilters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, internalId);
                        locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.IsActive, true);

                        // look for Location by id
                        var dbLocation = await locationCollection.FindAsync(locationFilters).Result.FirstOrDefaultAsync();

                        if (dbLocation == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Location
                        dbLocation.CustomFields = SaveOrUpdateGenericCustomField(request, dbLocation.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Location Id to 0 cause of the serializer
                        var locationId = dbLocation.Id;
                        dbLocation.Id = 0;

                        var locationSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var locationUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocation, locationSerializerSettings)) } };

                        // Update database record
                        await locationCollection.UpdateOneAsync(locationFilters, locationUpdate);

                        break;
                    case "PC_LocationGroup":
                        var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(locationTypeCollectionName);

                        // Filter
                        var locationGroupFilters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, internalId);
                        locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.IsActive, true);


                        // look for Location Group by id
                        var dbLocationGroup = await locationGroupCollection.FindAsync(locationGroupFilters).Result.FirstOrDefaultAsync();

                        // It appears the location group is missing so throw an error
                        if (dbLocationGroup == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Internal Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Location Group
                        dbLocationGroup.CustomFields = SaveOrUpdateGenericCustomField(request, dbLocationGroup.CustomFields, tableName, internalId.ToString(), ref dbCustomField, trackingGuid);

                        // Set Location Group Id to 0 cause of the serializer
                        var locationGroupId = dbLocationGroup.Id;
                        dbLocationGroup.Id = 0;

                        // Should look at moving this to the context
                        var locationGroupSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var locationGroupUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocationGroup, locationGroupSerializerSettings)) } };

                        // Update database record
                        await locationGroupCollection.UpdateOneAsync(locationGroupFilters, locationGroupUpdate);

                        break;
                    default:
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Type not found or does not exist." };
                }

                // Build the Response
                response = dbCustomField.ConvertToResponse();

                return response;
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Custom Field ( Name: {request.CustomFieldName}) failed to save! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Delete Method(s)
        /// <summary>
        /// Removes a custom field from the parent record
        /// </summary>
        /// <param name="id">Custom Field Id</param>
        /// <param name="userTableName">User defined table name</param>
        /// <param name="reactivate"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task DeleteOrReactivate(string id, string userTableName, bool reactivate, Guid trackingGuid)
        {
            // Company Id
            var companyId = context.Security.GetCompanyId();

            try
            {
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

                // Get tablename from request type
                var dbTableName = Sheev.Common.Utilities.TypeHelper.ConvertToDbType(userTableName);

                // Break combined id
                string[] ids = id.Split('|');

                // Parent id
                long.TryParse(ids[0], out long internalId);

                // Create an empty dbCustomField 
                var dbCustomField = new Deathstar.Data.Models.PC_CustomField();

                switch (dbTableName)
                {
                    case "PC_Product":
                    case "PC_ProductCategory":
                    case "PC_ProductUnit":
                    case "PC_ProductAlternateId":
                    case "PC_Kit":
                    case "PC_KitComponent":
                    case "PC_ProductOption":
                    case "PC_ProductOptionValue":
                        var productCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>(productCollectionName);

                        // Filters - note always start with the company id
                        var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, internalId);
                        filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

                        // look for product by id
                        var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                        if (dbProduct == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields
                        switch (dbTableName)
                        {
                            case "PC_Product":
                                // Save/Update the custom field
                                dbProduct.CustomFields = DeleteGenericCustomField(id, dbProduct.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_ProductCategory":
                                break;
                            case "PC_ProductUnit":
                                var dbUnitCustomField = dbProduct.Units.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbUnitCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductUnitId = dbUnitCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string index = $"|{dbUnitCustomField.CustomFieldName}";
                                int indexOfSteam = dbProductUnitId.IndexOf(index);
                                if (indexOfSteam >= 0)
                                    dbProductUnitId = dbProductUnitId.Remove(indexOfSteam);

                                // Get the parent
                                var dbProductUnit = dbProduct.Units.FirstOrDefault(x => x.PC_ProductUnit_Id == dbProductUnitId);

                                // Save/Update the custom field
                                dbProductUnit.CustomFields = DeleteGenericCustomField(id, dbProductUnit.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_ProductAlternateId":
                                var dbProductAlternateIdCustomField = dbProduct.AlternateIds.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductAlternateIdCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductAlternateIdId = dbProductAlternateIdCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string alternateIdIndex = $"|{dbProductAlternateIdCustomField.CustomFieldName}";
                                int indexOfAlternateId = dbProductAlternateIdId.IndexOf(alternateIdIndex);
                                if (indexOfAlternateId >= 0)
                                    dbProductAlternateIdId = dbProductAlternateIdId.Remove(indexOfAlternateId);

                                // Get the parent
                                var dbProductAlternateId = dbProduct.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == dbProductAlternateIdId);

                                // Save/Update the custom field
                                dbProductAlternateId.CustomFields = DeleteGenericCustomField(id, dbProductAlternateId.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_Kit":
                                // Check to see if the object exists
                                if (dbProduct.Kit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProduct.Kit.CustomFields = DeleteGenericCustomField(id, dbProduct.Kit.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_KitComponent":
                                var dbProductKitComponentCustomField = dbProduct.Kit.Components.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductKitComponentCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductKitComponentId = dbProductKitComponentCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string kitComponentIndex = $"|{dbProductKitComponentCustomField.CustomFieldName}";
                                int indexOfProductKitComponent = dbProductKitComponentId.IndexOf(kitComponentIndex);
                                if (indexOfProductKitComponent >= 0)
                                    dbProductKitComponentId = dbProductKitComponentId.Remove(indexOfProductKitComponent);

                                // Get the parent
                                var dbProductKitComponent = dbProduct.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == dbProductKitComponentId);

                                // Save/Update the custom field
                                dbProductKitComponent.CustomFields = DeleteGenericCustomField(id, dbProductKitComponent.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_ProductOption":
                                var dbProductOptionCustomField = dbProduct.Options.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductOptionCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductOptionId = dbProductOptionCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string productOptionIndex = $"|{dbProductOptionCustomField.CustomFieldName}";
                                int indexOfOption = dbProductOptionId.IndexOf(productOptionIndex);
                                if (indexOfOption >= 0)
                                    dbProductOptionId = dbProductOptionId.Remove(indexOfOption);

                                // Get the parent
                                var dbProductOption = dbProduct.Options.FirstOrDefault(x => x.PC_Option_Id == dbProductOptionId);

                                // Save/Update the custom field
                                dbProductOption.CustomFields = DeleteGenericCustomField(id, dbProductOption.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_ProductOptionValue":
                                var dbProductOptionValueCustomField = dbProduct.Options.SelectMany(x => x.Values).SelectMany(y => y.CustomFields).FirstOrDefault(z => z.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbProductOptionValueCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbProductOptionValueId = dbProductOptionValueCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string optionValueIndex = $"|{dbProductOptionValueCustomField.CustomFieldName}";
                                int indexOfOptionValue = dbProductOptionValueId.IndexOf(optionValueIndex);
                                if (indexOfOptionValue >= 0)
                                    dbProductOptionValueId = dbProductOptionValueId.Remove(indexOfOptionValue);

                                // Get the parent
                                var dbOptionValue = dbProduct.Options.SelectMany(x => x.Values).FirstOrDefault(z => z.PC_OptionValue_Id == dbProductOptionValueId);

                                // Delete the custom field
                                dbOptionValue.CustomFields = DeleteGenericCustomField(id, dbOptionValue.CustomFields, dbTableName, trackingGuid);
                                break;
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

                        break;
                    case "PC_ProductVariant":
                    case "PC_ProductVariantCategory":
                    case "PC_ProductVariantAlternateId":
                    case "PC_VariantKit":
                    case "PC_VariantKitComponent":
                        var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                        // Filter
                        var variantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, internalId);
                        variantFilters = variantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                        // look for variant by id
                        var dbVariant = await variantCollection.FindAsync(variantFilters).Result.FirstOrDefaultAsync();

                        if (dbVariant == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields
                        switch (dbTableName)
                        {
                            case "PC_ProductVariant":
                                // Save/Update the custom field
                                dbVariant.CustomFields = DeleteGenericCustomField(id, dbVariant.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_ProductVariantCategory":
                                break;
                            case "PC_ProductVariantAlternateId":
                                var dbVariantAlternateIdCustomField = dbVariant.AlternateIds.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbVariantAlternateIdCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbVariantAlternateIdId = dbVariantAlternateIdCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string alternateIdIndex = $"|{dbVariantAlternateIdCustomField.CustomFieldName}";
                                int indexOfAlternateId = dbVariantAlternateIdId.IndexOf(alternateIdIndex);
                                if (indexOfAlternateId >= 0)
                                    dbVariantAlternateIdId = dbVariantAlternateIdId.Remove(indexOfAlternateId);

                                // Get the parent
                                var dbProductVariantAlternateId = dbVariant.AlternateIds.FirstOrDefault(x => x.PC_AlternateId_Id == dbVariantAlternateIdId);

                                // Save/Update the custom field
                                dbProductVariantAlternateId.CustomFields = DeleteGenericCustomField(id, dbProductVariantAlternateId.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_VariantKit":
                                // Check to see if the object exists
                                if (dbVariant.Kit == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                dbVariant.Kit.CustomFields = DeleteGenericCustomField(id, dbVariant.Kit.CustomFields, dbTableName, trackingGuid);
                                break;
                            case "PC_VariantKitComponent":
                                var dbVariantKitComponentCustomField = dbVariant.Kit.Components.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                                // Check to see if the object exists
                                if (dbVariantKitComponentCustomField == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // So gotta do this to get the collection of custom fields for the object
                                var dbVariantKitComponentId = dbVariantKitComponentCustomField.PC_CustomField_Id;

                                // Modify the custom field id to get the parent id
                                string kitComponentIndex = $"|{dbVariantKitComponentCustomField.CustomFieldName}";
                                int indexOfProductKitComponent = dbVariantKitComponentId.IndexOf(kitComponentIndex);
                                if (indexOfProductKitComponent >= 0)
                                    dbVariantKitComponentId = dbVariantKitComponentId.Remove(indexOfProductKitComponent);

                                // Get the parent
                                var dbProductVariantKitComponent = dbVariant.Kit.Components.FirstOrDefault(x => x.PC_KitComponent_Id == dbVariantKitComponentId);

                                // Check to see if the object exists
                                if (dbProductVariantKitComponent == null)
                                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                                // Save/Update the custom field
                                dbProductVariantKitComponent.CustomFields = DeleteGenericCustomField(id, dbProductVariantKitComponent.CustomFields, dbTableName, trackingGuid);
                                break;
                        }

                        // Set variant Id to 0 cause of the serializer
                        var variantId = dbVariant.Id;
                        dbVariant.Id = 0;

                        var variantSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var variantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbVariant, variantSerializerSettings)) } };

                        // Update database record
                        await variantCollection.UpdateOneAsync(variantFilters, variantUpdate);

                        break;
                    case "PC_ProductVariantOption":
                        // Parent id
                        long.TryParse(ids[1], out long parentInternalVariantId);

                        // Get the table
                        var parentVariantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(variantCollectionName);

                        // Filter
                        var parentVariantFilters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        parentVariantFilters = parentVariantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, parentInternalVariantId);
                        parentVariantFilters = parentVariantFilters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                        // look for variant by id
                        var dbParentVariant = await parentVariantCollection.FindAsync(parentVariantFilters).Result.FirstOrDefaultAsync();

                        // Parent could be null
                        if (dbParentVariant == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        var dbVariantOptionCustomField = dbParentVariant.Options.SelectMany(x => x.CustomFields).FirstOrDefault(x => x.PC_CustomField_Id == id);

                        // Check to see if the object exists
                        if (dbVariantOptionCustomField == null)
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };

                        // So gotta do this to get the collection of custom fields for the object
                        var dbVariantOptionId = dbVariantOptionCustomField.PC_CustomField_Id;

                        // Modify the custom field id to get the parent id
                        string optionIndex = $"|{dbVariantOptionCustomField.CustomFieldName}";
                        int indexOfVariantOption = dbVariantOptionId.IndexOf(optionIndex);
                        if (indexOfVariantOption >= 0)
                            dbVariantOptionId = dbVariantOptionId.Remove(indexOfVariantOption);

                        // Get the parent
                        var dbProductVariantOption = dbParentVariant.Options.FirstOrDefault(x => x.PC_OptionValue_Id == dbVariantOptionId);

                        // Save/Update the custom field
                        dbProductVariantOption.CustomFields = DeleteGenericCustomField(id, dbProductVariantOption.CustomFields, dbTableName, trackingGuid);

                        // Set variant Id to 0 cause of the serializer
                        var parentVariantId = dbParentVariant.Id;
                        dbParentVariant.Id = 0;

                        // Serializer Settings to update the parent record
                        var parentVariantSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var parentVariantUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbParentVariant, parentVariantSerializerSettings)) } };

                        // Update database record
                        await parentVariantCollection.UpdateOneAsync(parentVariantFilters, parentVariantUpdate);
                        break;
                    case "PC_Inventory":
                    case "PC_ProductInventory":
                    case "PC_ProductVariantInventory":
                        var inventoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Inventory>(inventoryCollectionName);

                        // Filter
                        var inventoryFilters = Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.Id, internalId);
                        inventoryFilters = inventoryFilters & Builders<Deathstar.Data.Models.PC_Inventory>.Filter.Eq(x => x.IsActive, true);

                        // look for Inventory by id
                        var dbInventory = await inventoryCollection.FindAsync(inventoryFilters).Result.FirstOrDefaultAsync();

                        if (dbInventory == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Inventory
                        dbInventory.CustomFields = DeleteGenericCustomField(id, dbInventory.CustomFields, dbTableName, trackingGuid);

                        // Set Inventory Id to 0 cause of the serializer
                        var inventoryId = dbInventory.Id;
                        dbInventory.Id = 0;

                        var inventorySerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var inventoryUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbInventory, inventorySerializerSettings)) } };

                        // Update database record
                        await inventoryCollection.UpdateOneAsync(inventoryFilters, inventoryUpdate);
                        break;
                    case "PC_AlternateIdType":
                        var alternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>(alternateIdTypeCollectionName);

                        // Filter
                        var alternateIdTypeFilters = Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.Id, internalId);
                        alternateIdTypeFilters = alternateIdTypeFilters & Builders<Deathstar.Data.Models.PC_AlternateIdType>.Filter.Eq(x => x.IsActive, true);


                        // look for Alternate Id Type by id
                        var dbAlternateIdType = await alternateIdTypeCollection.FindAsync(alternateIdTypeFilters).Result.FirstOrDefaultAsync();

                        if (dbAlternateIdType == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }
                        // Add/update custom fields on a Alternate Id Type
                        dbAlternateIdType.CustomFields = DeleteGenericCustomField(id, dbAlternateIdType.CustomFields, dbTableName, trackingGuid);

                        // Set Alternate Id Type Id to 0 cause of the serializer
                        var alternateIdTypeId = dbAlternateIdType.Id;
                        dbAlternateIdType.Id = 0;

                        var alternateIdTypeSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var alternateIdTypeUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbAlternateIdType, alternateIdTypeSerializerSettings)) } };

                        // Update database record
                        await alternateIdTypeCollection.UpdateOneAsync(alternateIdTypeFilters, alternateIdTypeUpdate);
                        break;
                    case "PC_Category":
                        var categoryCollection = db.GetCollection<Deathstar.Data.Models.PC_Category>(categoryCollectionName);

                        // Filter
                        var categoryFilters = Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.Id, internalId);
                        categoryFilters = categoryFilters & Builders<Deathstar.Data.Models.PC_Category>.Filter.Eq(x => x.IsActive, true);

                        // look for Category by id
                        var dbCategory = await categoryCollection.FindAsync(categoryFilters).Result.FirstOrDefaultAsync();

                        if (dbCategory == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Category
                        dbCategory.CustomFields = DeleteGenericCustomField(id, dbCategory.CustomFields, dbTableName, trackingGuid);

                        // Set Category Id to 0 cause of the serializer
                        var categoryId = dbCategory.Id;
                        dbCategory.Id = 0;

                        var categorySerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var categoryUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategory, categorySerializerSettings)) } };

                        // Update database record
                        await categoryCollection.UpdateOneAsync(categoryFilters, categoryUpdate);

                        break;
                    case "PC_CategorySet":
                        var categorySetCollection = db.GetCollection<Deathstar.Data.Models.PC_CategorySet>(categorySetCollectionName);

                        // Filter
                        var categorySetFilters = Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.Id, internalId);
                        categorySetFilters = categorySetFilters & Builders<Deathstar.Data.Models.PC_CategorySet>.Filter.Eq(x => x.IsActive, true);

                        // look for Category Set by id
                        var dbCategorySet = await categorySetCollection.FindAsync(categorySetFilters).Result.FirstOrDefaultAsync();

                        if (dbCategorySet == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Category Set
                        dbCategorySet.CustomFields = DeleteGenericCustomField(id, dbCategorySet.CustomFields, dbTableName, trackingGuid);

                        // Set Category Set Id to 0 cause of the serializer
                        var categorySetId = dbCategorySet.Id;
                        dbCategorySet.Id = 0;

                        var categorySetserializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var categorySetUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbCategorySet, categorySetserializerSettings)) } };

                        // Update database record
                        await categorySetCollection.UpdateOneAsync(categorySetFilters, categorySetUpdate);

                        break;
                    case "PC_Location":
                        var locationCollection = db.GetCollection<Deathstar.Data.Models.PC_Location>(locationCollectionName);

                        // Filter
                        var locationFilters = Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.Id, internalId);
                        locationFilters = locationFilters & Builders<Deathstar.Data.Models.PC_Location>.Filter.Eq(x => x.IsActive, true);

                        // look for Location by id
                        var dbLocation = await locationCollection.FindAsync(locationFilters).Result.FirstOrDefaultAsync();

                        if (dbLocation == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Location
                        dbLocation.CustomFields = DeleteGenericCustomField(id, dbLocation.CustomFields, dbTableName, trackingGuid);

                        // Set Location Id to 0 cause of the serializer
                        var locationId = dbLocation.Id;
                        dbLocation.Id = 0;

                        var locationSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var locationUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocation, locationSerializerSettings)) } };

                        // Update database record
                        await locationCollection.UpdateOneAsync(locationFilters, locationUpdate);

                        break;
                    case "PC_LocationGroup":
                        var locationGroupCollection = db.GetCollection<Deathstar.Data.Models.PC_LocationGroup>(locationTypeCollectionName);

                        // Filter
                        var locationGroupFilters = Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                        locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.Id, internalId);
                        locationGroupFilters = locationGroupFilters & Builders<Deathstar.Data.Models.PC_LocationGroup>.Filter.Eq(x => x.IsActive, true);


                        // look for Location Group by id
                        var dbLocationGroup = await locationGroupCollection.FindAsync(locationGroupFilters).Result.FirstOrDefaultAsync();

                        // It appears the location group is missing so throw an error
                        if (dbLocationGroup == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                        }

                        // Add/update custom fields on a Location Group
                        dbLocationGroup.CustomFields = DeleteGenericCustomField(id, dbLocationGroup.CustomFields, dbTableName, trackingGuid);

                        // Set Location Group Id to 0 cause of the serializer
                        var locationGroupId = dbLocationGroup.Id;
                        dbLocationGroup.Id = 0;

                        // Should look at moving this to the context
                        var locationGroupSerializerSettings = new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };
                        var locationGroupUpdate = new BsonDocument() { { "$set", BsonDocument.Parse(JsonConvert.SerializeObject(dbLocationGroup, locationGroupSerializerSettings)) } };

                        // Update database record
                        await locationGroupCollection.UpdateOneAsync(locationGroupFilters, locationGroupUpdate);

                        break;
                    default:
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Type not found or does not exist." };
                }
            }
            catch (HttpResponseException webEx)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Custom Field ( Id: {id}) failed to delete! Reason: {webEx.ReasonPhrase}", $"Status Code: {webEx.StatusCode}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.DeleteOrReactivate()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        /// <summary>
        /// Rather than repeating this over and over in the Delete Or Reactivate I moved 
        /// </summary>
        /// <param name="internalId"></param>
        /// <param name="existingCustomFields"></param>
        /// <param name="tableName"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<Deathstar.Data.Models.PC_CustomField> DeleteGenericCustomField(string internalId, List<Deathstar.Data.Models.PC_CustomField> existingCustomFields, string tableName, Guid trackingGuid)
        {
            try
            {
                // Null check so we dont throw an error after this when we try to access the collection
                if (existingCustomFields == null || existingCustomFields.Count <= 0)
                    existingCustomFields = new List<Deathstar.Data.Models.PC_CustomField>();

                // Check to see if the custom field exists in the list already
                var dbCustomField = existingCustomFields.FirstOrDefault(x => x.PC_CustomField_Id.ToLower() == internalId.ToLower());

                // if not found throw an error cause we don't want that!
                if (dbCustomField == null)
                {
                    // throw error
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Id not found or does not exist." };
                }

                // Remove the custom field 
                existingCustomFields.Remove(dbCustomField);

                return existingCustomFields;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CustomFieldModel.UpdateGenericCustomField()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Validate Method(s)
        public void ValidateCustomField(string customFieldName, string tableName)
        {
            var dbCustomField = Utilities.CoreDatabaseAccess.GetCustomField(customFieldName, tableName, context);

            if (dbCustomField.Id <= 0)
            {
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Custom Field({customFieldName}) does not exist!" };
            }
        }
        #endregion
    }
}