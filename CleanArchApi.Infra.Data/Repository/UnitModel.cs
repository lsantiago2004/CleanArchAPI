using MongoDB.Driver;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    public class UnitModel : IUnitModel
    {
        private readonly IBaseContextModel context;
        private readonly ICustomFieldModel _customFieldModel;
        private readonly IExternalIdModel _externalId;
        public UnitModel(IBaseContextModel contextModel, ICustomFieldModel customFieldModel, IExternalIdModel externalId)
        {
            context = contextModel;
            _customFieldModel = customFieldModel;
            _externalId = externalId;
        }
        #region Get Method(s)
        #endregion

        #region Get Method(s)
        #endregion

        #region Save Method(s)
        public async Task<List<Deathstar.Data.Models.PC_ProductUnit>> Save(long productId, List<Tagge.Common.Models.ProductUnitRequest> units, Guid trackingGuid)
        {
            var dbOptions = new List<Deathstar.Data.Models.PC_ProductUnit>();

            // Company Id
            var companyId = context.Security.GetCompanyId();

            var productCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");

            // Filters - note always start with the company id
            var filters = Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.Id, productId);
            filters = filters & Builders<Deathstar.Data.Models.PC_Product>.Filter.Eq(x => x.IsActive, true);

            var dbProduct = await productCollection.FindAsync(filters).Result.FirstOrDefaultAsync();

            foreach (var unit in units)
            {
                // check to make sure name exists
                if (string.IsNullOrEmpty(unit.Name))
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Unit name is null or empty" };

                var dbUnit = new Deathstar.Data.Models.PC_ProductUnit();

                // Check to make sure the product and the unit collection are not nukll
                if (dbProduct != null && dbProduct.Units != null)
                {
                    dbUnit = dbProduct.Units.FirstOrDefault(x => x.Name.ToLower() == unit.Name.ToLower() && x.IsActive);
                }

                // if one of these is null create a new unit
                if (dbProduct == null || dbProduct.Units == null || dbUnit == null)
                {
                    // Add
                    dbUnit = new Deathstar.Data.Models.PC_ProductUnit()
                    {
                        Name = unit.Name,
                        Conversion = unit.Conversion,
                        DefaultPrice = unit.DefaultPrice,
                        MSRP = unit.MSRP,
                        SalePrice = unit.SalePrice,
                        IsActive = true,
                        CreatedBy = context.Security.GetEmail(),
                        CreatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz")
                    };

                    // Set Primary Key
                    dbUnit.SetPrimaryKey(productId.ToString(), unit.Name);
                }
                else
                {
                    // Update
                    dbUnit.Conversion = unit.Conversion;
                    dbUnit.DefaultPrice = unit.DefaultPrice;
                    dbUnit.MSRP = unit.MSRP;
                    dbUnit.SalePrice = unit.SalePrice;
                    dbUnit.IsActive = true;
                    dbUnit.UpdatedBy = context.Security.GetEmail();
                    dbUnit.UpdatedDateTime = DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff zzz");
                }

                // Custom Fields 
                if (unit.CustomFields != null && unit.CustomFields.Count > 0)
                {
                    dbUnit.CustomFields = await _customFieldModel.SaveGenericCustomField(unit.CustomFields, "PC_ProductUnit", dbUnit.PC_ProductUnit_Id, trackingGuid);
                }

                // ExternalIds 
                if (unit.ExternalIds != null && unit.ExternalIds.Count > 0)
                {
                    await _externalId.SaveOrUpdateGenericExternalId(unit.ExternalIds, "PC_ProductUnit", dbUnit.PC_ProductUnit_Id, trackingGuid);
                }

                dbOptions.Add(dbUnit);
            }

            return dbOptions;
        }
        #endregion

        #region Delete/Reactivate Method(s)
        public async Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid)
        {


            return Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent;
        }
        #endregion

        #region Validate Method(s)
        #endregion
    }
}
