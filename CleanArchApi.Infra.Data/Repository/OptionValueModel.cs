using MongoDB.Driver;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    public class OptionValueModel : IOptionValueModel
    {
        private readonly IBaseContextModel context;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IExternalIdModel _externalIdModel;
        public OptionValueModel(IExternalIdModel externalIdModel, ICustomFieldModel customFieldModel, IBaseContextModel contextModel)
        {
            context = contextModel;
            _customFieldModel = customFieldModel;
            _externalIdModel = externalIdModel;
        }
        #region Save Method(s)
        /// <summary>
        /// Internal Use Only! Save an Option Value on an Product Option
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="dbOption"></param>
        /// <param name="optionvalues"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_OptionValue>> Save(long productId, Deathstar.Data.Models.PC_Option dbOption, List<Tagge.Common.Models.OptionValueRequest> optionvalues, Guid trackingGuid)
        {
            {
                var dbOptionValues = new List<Deathstar.Data.Models.PC_OptionValue>();

                // Company Id
                var companyId = context.Security.GetCompanyId();

                try
                {
                    if (optionvalues != null && optionvalues.Count > 0)
                    {
                        foreach (var optionValue in optionvalues)
                        {
                            var dbOptionValue = new Deathstar.Data.Models.PC_OptionValue();

                            dbOptionValue = dbOption.Values.FirstOrDefault(x => x.Value.ToLower() == optionValue.Value.ToLower() && x.IsActive);

                            if (string.IsNullOrEmpty(optionValue.Value))
                                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Option Value is null or empty" };

                            if (dbOptionValue == null)
                            {
                                // Add
                                dbOptionValue = new Deathstar.Data.Models.PC_OptionValue()
                                {
                                    OptionName = dbOption.Name,
                                    Value = optionValue.Value,
                                    Detail = optionValue.Detail,
                                    Order = optionValue.Order,
                                    IsActive = true,
                                    CreatedBy = context.Security.GetEmail(),
                                    CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz")
                                };

                                dbOptionValue.SetPrimaryKey(productId.ToString(), null, dbOption.Name, optionValue.Value);
                            }
                            else
                            {
                                // Update
                                dbOptionValue.Value = optionValue.Value;
                                dbOptionValue.Detail = optionValue.Detail;
                                dbOptionValue.Order = optionValue.Order;
                                dbOptionValue.IsActive = true;
                                dbOptionValue.UpdatedBy = context.Security.GetEmail();
                                dbOptionValue.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                            }

                            // Custom Fields 
                            if (optionValue.CustomFields != null && optionValue.CustomFields.Count > 0)
                            {
                                dbOptionValue.CustomFields = await _customFieldModel.SaveGenericCustomField(optionValue.CustomFields, "PC_ProductOptionValue", dbOptionValue.PC_OptionValue_Id, trackingGuid);
                            }

                            // ExternalIds 
                            if (optionValue.ExternalIds != null && optionValue.ExternalIds.Count > 0)
                            {
                                await _externalIdModel.SaveOrUpdateGenericExternalId(optionValue.ExternalIds, "PC_ProductOptionValue", dbOptionValue.PC_OptionValue_Id, trackingGuid);
                            }

                            dbOptionValues.Add(dbOptionValue);
                        }
                    }

                    return dbOptionValues;
                }
                catch (HttpResponseException webEx)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "OptionValueModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
                }


            }
        }
        #endregion

        #region Save Variant Method(s)
        /// <summary>
        /// Internal Use Only! Save variant options
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="dbProduct"></param>
        /// <param name="request"></param>
        /// <param name="dbExistingVariantOptions"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_OptionValue>> SaveOrUpdateVariantOptions(long variantId, Deathstar.Data.Models.PC_Product dbProduct, List<Tagge.Common.Models.OptionValueRequest> request, List<Deathstar.Data.Models.PC_OptionValue> dbExistingVariantOptions, Guid trackingGuid)
        {
            // The Response
            var dbOptionValues = new List<Deathstar.Data.Models.PC_OptionValue>();

            try
            {
                foreach (var optionValue in request)
                {
                    // Product Option
                    var dbProductOption = new Deathstar.Data.Models.PC_Option();

                    // Look up existing options for parent product
                    var dbProductOptions = dbProduct.Options.Where(x => x.IsActive).ToList();

                    // Check request option against parent product options based on Name
                    if (string.IsNullOrEmpty(optionValue.OptionName))
                    {
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Variant Option Name is Required" };
                    }

                    dbProductOption = dbProductOptions.FirstOrDefault(x => x.Name.ToLower() == optionValue.OptionName.ToLower());

                    if (dbProductOption == null)
                    {
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"The Product(Id: {dbProduct.Id}) this Variant(Id: {variantId}) is a part of does not have any Product Options set that match the Variant Option Request. Please add options at the product level first." };
                    }

                    // Check request option value against parent product option values- Bug 3199
                    if (!string.IsNullOrEmpty(optionValue.Value))
                    {
                        var dbProductOptionValue = dbProductOption.Values.FirstOrDefault(x => x.Value.ToLower() == optionValue.Value.ToLower() && x.IsActive);

                        if (dbProductOptionValue == null)
                        {
                            throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Product Variant Option Value: {optionValue.Value} must first be set as Option Value for parent Product Id: {dbProduct.Id}, Option: {dbProductOption.Name}." };
                        }
                    }

                    // Bug: 3198, 3398 <- See pervious version of the api for these bugs
                    if (string.IsNullOrEmpty(optionValue.Value))
                    {
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Variant Option Value is Required" };
                    }

                    // Look up if Variant Option already exists in the db
                    var dbOptionValue = new Deathstar.Data.Models.PC_OptionValue();

                    // Convert Request to database object
                    dbOptionValue.ConvertToDatabaseObject(optionValue);

                    // Set Primary Key
                    dbOptionValue.SetPrimaryKey(dbProduct.Id.ToString(), variantId.ToString(), optionValue.OptionName, optionValue.Value);

                    // Check to see if the variant option already exists
                    var dbExistingOptionValue = dbExistingVariantOptions.FirstOrDefault(x => x.PC_OptionValue_Id == dbOptionValue.PC_OptionValue_Id);

                    // Add existing option value
                    if (dbExistingOptionValue == null)
                    {
                        // Set Created By & Created Date Time
                        dbOptionValue.CreatedBy = context.Security.GetEmail();
                        dbOptionValue.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // Custom Fields 
                        if (optionValue.CustomFields != null && optionValue.CustomFields.Count > 0)
                        {
                            dbOptionValue.CustomFields = await _customFieldModel.SaveGenericCustomField(optionValue.CustomFields, "PC_ProductVariantOption", dbOptionValue.PC_OptionValue_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (optionValue.ExternalIds != null && optionValue.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(optionValue.ExternalIds, "PC_ProductVariantOption", dbOptionValue.PC_OptionValue_Id, trackingGuid);
                        }
                    }
                    else // Update existing option value
                    {
                        // Set existing option value to current option value
                        dbOptionValue = dbExistingOptionValue;

                        // Set Updated By & Updated Date Time
                        dbOptionValue.UpdatedBy = context.Security.GetEmail();
                        dbOptionValue.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // Custom Fields 
                        if (optionValue.CustomFields != null && optionValue.CustomFields.Count > 0)
                        {
                            dbOptionValue.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(optionValue.CustomFields, dbExistingOptionValue.CustomFields, "PC_ProductVariantOption", dbOptionValue.PC_OptionValue_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (optionValue.ExternalIds != null && optionValue.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(optionValue.ExternalIds, "PC_ProductVariantOption", dbOptionValue.PC_OptionValue_Id, trackingGuid);
                        }
                    }

                    // Set Is Active to true
                    dbOptionValue.IsActive = true;

                    dbOptionValues.Add(dbOptionValue);
                }

                return dbOptionValues;
            }
            catch (HttpResponseException webEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CatalogModel.SaveProduct()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
        #endregion

        #region Update Variant Method(s)
        #endregion

        #region Delete/Reactivate Method(s)
        public async Task<int> DeleteOrReactivateVariantOptions(long id, string optionName, bool reactivate, Guid trackingGuid)
        {
            try
            {
                var companyId = context.Security.GetCompanyId();

                // MongoDB Settings
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "PC_ProductVariant").Name;

                // Get MongoDB
                var db = context.Database;
                var variantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>(collectionName);

                // Filters - note always start with the company id
                var filters = Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.Id, id);
                filters = filters & Builders<Deathstar.Data.Models.PC_ProductVariant>.Filter.Eq(x => x.IsActive, true);

                //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Beginning get product by Id by user: {context.Security.GetEmail()},", "Get Product (MongoDB)", LT319.Common.Utilities.Constants.TrackingStatus.Active, trackingGuid);

                var dbVariant = await variantCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

                if (dbVariant == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Product Variant not found by Id:{id} provided." };

                var dbOptionValue = dbVariant.Options.FirstOrDefault(x => x.OptionName == optionName && x.IsActive);

                if (dbOptionValue == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, ReasonPhrase = $"Product Variant Option not found by Name:{optionName} provided." };

                // Delete or Reactivate the Category Assignment
                var update = Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Combine(
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Options[-1].OptionName, optionName),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Options[-1].UpdatedBy, context.Security.GetEmail()),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Options[-1].UpdatedDateTime, DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz")),
                             Builders<Deathstar.Data.Models.PC_ProductVariant>.Update.Set(x => x.Options[-1].IsActive, reactivate)
                 );

                await variantCollection.UpdateOneAsync(filters, update);

            }
            catch (HttpResponseException webEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "CatalogModel.SaveProduct()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }

            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion
    }
}
