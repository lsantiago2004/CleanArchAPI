using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    public class KitComponentModel : IKitComponentModel
    {
        private readonly IBaseContextModel context;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IExternalIdModel _externalIdModel;
        private readonly ITypeModel _typeModel;
        public KitComponentModel(ICustomFieldModel customFieldModel, IBaseContextModel contextModel, ITypeModel typeModel, IExternalIdModel externalIdModel)
        {
            _customFieldModel = customFieldModel;
            _externalIdModel = externalIdModel;
            _typeModel = typeModel;
            context = contextModel;
        }
        #region Get Method(s)
        #endregion

        #region Save Method(s)
        /// <summary>
        /// Internal Use Only! Save a Kit Component
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_KitComponent>> Save(string parentId, string tableName, List<Tagge.Common.Models.KitComponentRequest> request, Guid trackingGuid)
        {
            var dbKitComponents = new List<Deathstar.Data.Models.PC_KitComponent>();

            try
            {
                foreach (var component in request)
                {
                    // Check to see if sku is populated if not error
                    if (string.IsNullOrEmpty(component.Sku))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Sku is null or empty" };

                    // Is quantity greater than 0 if so you good
                    if (component.Quantity <= 0)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Qty is less or equal to '0'" };

                    // Unit is a required field if its missing error!
                    if (string.IsNullOrEmpty(component.Unit))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Unit value not found" };

                    // Validate the sku & unit
                    await ValidateModel.ValidateSkuAndUnit(component.Sku, component.Unit, context, trackingGuid);

                    // Validate Kit Component Type
                    await _typeModel.ValidateKitComponentType(component.Type, trackingGuid);

                    // Building the component
                    // Why not using ConvertToDatabaseObject?
                    var dbComponent = new Deathstar.Data.Models.PC_KitComponent();

                    // Convert To Database object
                    dbComponent.ConvertToDatabaseObject(component);

                    // Set Primary Key
                    dbComponent.SetPrimaryKey(parentId, component.Unit);

                    if (!tableName.Contains("Component"))
                        tableName = tableName + "Component";

                    // Custom Fields 
                    if (component.CustomFields != null && component.CustomFields.Count > 0)
                    {
                        dbComponent.CustomFields = await _customFieldModel.SaveGenericCustomField(component.CustomFields, tableName, dbComponent.PC_KitComponent_Id, trackingGuid);
                    }

                    // ExternalIds 
                    if (component.ExternalIds != null && component.ExternalIds.Count > 0)
                    {
                        await _externalIdModel.SaveOrUpdateGenericExternalId(component.ExternalIds, tableName, dbComponent.PC_KitComponent_Id, trackingGuid);
                    }

                    // Add Created By & Timestamp
                    dbComponent.CreatedBy = context.Security.GetEmail();
                    dbComponent.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                    // IsActive
                    dbComponent.IsActive = true;

                    // Add to Response
                    dbKitComponents.Add(dbComponent);
                }
            }
            catch (HttpResponseException)
            {
                throw;
            }

            return dbKitComponents;
        }
        #endregion

        #region Save or Update Method(s)
        /// <summary>
        /// Internal Use Only! Save a Kit Component
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="tableName"></param>
        /// <param name="request"></param>
        /// <param name="dbExistingComponents"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        public async Task<List<Deathstar.Data.Models.PC_KitComponent>> SaveOrUpdate(string parentId, string tableName, List<Tagge.Common.Models.KitComponentRequest> request, List<Deathstar.Data.Models.PC_KitComponent> dbExistingComponents, Guid trackingGuid)
        {
            try
            {
                foreach (var requestComponent in request)
                {
                    // Check to see if sku is populated if not error
                    if (string.IsNullOrEmpty(requestComponent.Sku))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Sku is null or empty" };

                    // Is quantity greater than 0 if so you good
                    if (requestComponent.Quantity <= 0)
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Qty is less or equal to '0'" };

                    // Unit is a required field if its missing error!
                    if (string.IsNullOrEmpty(requestComponent.Unit))
                        throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Unit value not found" };

                    // Validate Kit Component Type
                    await _typeModel.ValidateKitComponentType(requestComponent.Type, trackingGuid);

                    // Validate the sku & unit
                    await ValidateModel.ValidateSkuAndUnit(requestComponent.Sku, requestComponent.Unit,context, trackingGuid);

                    // Building the component
                    // Why not using ConvertToDatabaseObject?
                    var dbComponent = new Deathstar.Data.Models.PC_KitComponent();

                    // Convert To Database object
                    dbComponent.ConvertToDatabaseObject(requestComponent);

                    // Set Primary Key
                    dbComponent.SetPrimaryKey(parentId, requestComponent.Unit);

                    // Check to see if the component already exists on this kit
                    var dbExistingComponent = dbExistingComponents.FirstOrDefault(x => x.PC_KitComponent_Id == dbComponent.PC_KitComponent_Id);

                    // Set correct tablename
                    if (!tableName.Contains("Component"))
                        tableName = tableName + "Component";

                    // Add the Component
                    if (dbExistingComponent == null)
                    {
                        // Custom Fields 
                        if (requestComponent.CustomFields != null && requestComponent.CustomFields.Count > 0)
                        {
                            dbComponent.CustomFields = await _customFieldModel.SaveGenericCustomField(requestComponent.CustomFields, tableName, dbComponent.PC_KitComponent_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (requestComponent.ExternalIds != null && requestComponent.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(requestComponent.ExternalIds, tableName, dbComponent.PC_KitComponent_Id, trackingGuid);
                        }

                        // Add Created By & Timestamp
                        dbComponent.CreatedBy = context.Security.GetEmail();
                        dbComponent.CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // IsActive
                        dbComponent.IsActive = true;

                        // Add to Response
                        dbExistingComponents.Add(dbComponent);
                    }
                    else // Update the Component
                    {
                        // Set existing component to current component
                        dbExistingComponent.Quantity = requestComponent.Quantity;
                        dbExistingComponent.Type = requestComponent.Type;

                        // Custom Fields 
                        if (requestComponent.CustomFields != null && requestComponent.CustomFields.Count > 0)
                        {
                            dbExistingComponent.CustomFields = await _customFieldModel.SaveOrUpdateGenericCustomFields(requestComponent.CustomFields, dbExistingComponent.CustomFields, tableName, dbComponent.PC_KitComponent_Id, trackingGuid);
                        }

                        // ExternalIds 
                        if (requestComponent.ExternalIds != null && requestComponent.ExternalIds.Count > 0)
                        {
                            await _externalIdModel.SaveOrUpdateGenericExternalId(requestComponent.ExternalIds, tableName, dbComponent.PC_KitComponent_Id, trackingGuid);
                        }

                        // Add Created By & Timestamp
                        dbExistingComponent.UpdatedBy = context.Security.GetEmail();
                        dbExistingComponent.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");

                        // IsActive
                        dbExistingComponent.IsActive = true;
                    }
                }
            }
            catch (HttpResponseException)
            {
                throw;
            }

            return dbExistingComponents;
        }
        #endregion

        #region Delete/Reactivate Method(s)
        #endregion


    }
}
