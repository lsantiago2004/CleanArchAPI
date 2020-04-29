using Deathstar.Data.Models;
using MongoDB.Driver;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Common.Models;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    public class OptionModel : IOptionModel
    {
        private readonly IBaseContextModel context;
        private readonly IOptionValueModel _optionValueModel;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IExternalIdModel _externalIdModel;
        public OptionModel(IExternalIdModel externalIdModel, ICustomFieldModel customFieldModel, IOptionValueModel optionValueModel, IBaseContextModel contextModel)
        {
            context = contextModel;
            _optionValueModel = optionValueModel;
            _customFieldModel = customFieldModel;
            _externalIdModel = externalIdModel;
        }

        #region Save/Update Method(s)
        /// <summary>
        /// Internal Use Only! Save/Update a list of options
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_Option>> Save(long productId, List<Tagge.Common.Models.OptionRequest> options, Guid trackingGuid)
        {
            var dbOptions = new List<Deathstar.Data.Models.PC_Option>();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            var productCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, productId);

            var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            try
            {
                if (options != null && options.Count > 0)
                {
                    foreach (var option in options)
                    {
                        var dbOption = new Deathstar.Data.Models.PC_Option();

                        if (dbProduct != null && dbProduct.Options != null)
                            dbOption = dbProduct.Options.FirstOrDefault(x => x.Name.ToLower() == option.OptionName.ToLower() && x.IsActive);

                        if (dbProduct == null || dbProduct.Options == null || dbOption == null)
                        {
                            // Add
                            dbOption = new Deathstar.Data.Models.PC_Option()
                            {
                                Name = option.OptionName,
                                Order = option.Order,
                                Type = option.Type,
                                IsActive = true,
                                CreatedBy = context.Security.GetEmail(),
                                CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz")
                            };

                            // Set Primary Key
                            dbOption.SetPrimaryKey(productId.ToString(), option.OptionName);
                        }
                        else
                        {
                            // Update
                            dbOption.Order = option.Order;
                            dbOption.Type = option.Type;
                            dbOption.IsActive = true;
                            dbOption.UpdatedBy = context.Security.GetEmail();
                            dbOption.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                        }

                        // Option Values
                        dbOption.Values = await _optionValueModel.Save(productId, dbOption, option.Values, trackingGuid);

                        // Custom Fields 
                        if (option.CustomFields != null && option.CustomFields.Count > 0)
                        {
                            dbOption.CustomFields = await _customFieldModel.SaveGenericCustomField(option.CustomFields, "PC_ProductOption", dbOption.PC_Option_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (option.ExternalIds != null && option.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(option.ExternalIds, "PC_ProductOption", dbOption.PC_Option_Id, trackingGuid);
                        }

                        dbOptions.Add(dbOption);
                    }
                }

                return dbOptions;
            }
            catch (HttpResponseException webEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "OptionModel.Save()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }

        }

        #endregion

        #region Delete/Reactivate Method(s)
        public async Task<int> DeleteOrReactivate(string id, bool reactivate, Guid trackingGuid)
        {
            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        /// <summary>
        /// Checks to see if the option already exists on the product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task Validate(long productId, List<Tagge.Common.Models.OptionRequest> options, Models.ContextModel context, Guid trackingGuid)
        {

        }
        #endregion
    }
}
